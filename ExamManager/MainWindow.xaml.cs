using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using WinRT.Interop;
using Microsoft.UI.Windowing;
using System.Runtime.InteropServices;
using System.Diagnostics;
using ExamManager.Modules.User;
using ExamManager.Modules.Util;
using System.Threading.Tasks;
using ExamManager.Modules.Info;

// The main window for Exam Manager

namespace ExamManager
{
    public sealed partial class MainWindow : Window
    {
        // Services and primary objects
        private AppWindow _appWindow;
        private User32Dll _user32dll;
        private SQLService _sqlService;
        private HasPermission _hasPermission;

        private string username;
        private int usrLevel;

        private bool signedIn;

        private object currentPage;
        private string selectedCandidateYear;
        private string selectedCandidate;

        private bool developerMode = false;
        private string version = "2";

        public MainWindow()
        {
            this.InitializeComponent();

            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            _appWindow = AppWindow.GetFromWindowId(windowId);

            // Set custom icon
            var exeHandle = GetModuleHandle(Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName));
            var icon = LoadImage(exeHandle, IDI_APPLICATION, IMAGE_ICON, 16, 16, 0);
            Debug.WriteLine("Successfully started window object");
            AppWindow.SetIcon(Win32Interop.GetIconIdFromIcon(icon));

            // Set tray icon
            _user32dll = new User32Dll();
            _user32dll.InitializeTrayIcon(this, icon);

            this.Closed += closeWindow;

            // Setup SQL stuff
            _sqlService = new SQLService();
            _sqlService.Initialize();

            // Has permssion service
            _hasPermission = new HasPermission();
            _hasPermission.Initialize();

            // Setup testing user
            if (!_sqlService.CheckTestUser("Test"))
            {
                _sqlService.AddUserToDB("Test", "Test", "3");
            }

            if (!_sqlService.CheckTestUser("dev"))
            {
                _sqlService.AddUserToDB("dev", "a", "4");
            }

            if (this.developerMode)
            {
                this.signedIn = true;
                this.username = "DeveloperTestingAccount";
                this.signUserIn(true);
                this.usrLevel = 4;

                this.DevInfoName1.Text = $"{this.DevInfoName1.Text} ~ VERSION {this.version}";
                this.DevInfoName2.Text = $"{this.DevInfoName1.Text} ~ VERSION {this.version}";
                this.DevInfoName1.Visibility = Visibility.Visible;
                this.DevInfoName2.Visibility = Visibility.Visible;

                this.SignedOutPage.Visibility = Visibility.Collapsed;
                Debug.WriteLine("DEVELOPER MODE IS CURRENTLY ENABLED.");
            } else
            {
                this.DevInfoName1.Visibility = Visibility.Collapsed;
                this.DevInfoName2.Visibility = Visibility.Collapsed;
                this.signedIn = false;
            }

            this.selectedCandidate = "0";
        }

        private void SignInShow(object sender, RoutedEventArgs e)
        {
            this.SignedOutPage.Visibility = Visibility.Collapsed;
            this.SignedInPage.Visibility = Visibility.Visible;
            this.startFancySignedInPageCountdown();
        }

        private void signUserIn(bool state) // adjusts the visibility of the signin page and homepage
        {
            if (state) {
                this.SignedInPage.Visibility = Visibility.Collapsed;
                this.Homepage.Visibility = Visibility.Visible;
                this.WelcomePage.Visibility = Visibility.Visible;
                this.UserName.Text = $"Welcome, {this.username}!";
                this.currentPage = this.WelcomePage;
            } else
            {
                this.SignedOutPage.Visibility = Visibility.Visible;
                this.Homepage.Visibility = Visibility.Collapsed;
                this.UserName.Text = "OnceIWasTwentyYearsOld";
                this.signedIn = false;
                this.user.Text = "";
                this.password.Text = "";
                ((StackPanel)this.currentPage).Visibility = Visibility.Collapsed;
                this.CleanupCandidates();
                this.currentPage = this.SignedOutPage;
            }
        }

        private async void startFancySignedInPageCountdown()
        {
            while (!signedIn)
            {
                await Task.Delay(30000);
                
                if (!signedIn)
                {
                    this.SignedInPage.Visibility = Visibility.Collapsed;
                    this.SignedOutPage.Visibility = Visibility.Visible;
                    break;
                } else
                {
                    break;
                }
            }
        }

        private void SignOut(object sender, RoutedEventArgs e)
        {
            if (!this.developerMode)
            {
                this.signUserIn(false);
            } else
            {
                Debug.WriteLine("signing out whilst in developer mode is disabled.");
            }
        }

        private void ShowCandidates(object sender, RoutedEventArgs e)
        {
            if (_hasPermission.CheckPermission(this.usrLevel, "Menu:CandidateManagement"))
            {
                ((StackPanel)this.currentPage).Visibility = Visibility.Collapsed;
                this.CandidateManagement.Visibility = Visibility.Visible;
                this.currentPage = this.CandidateManagement;
            }
        }

        private void ReturnToHomepage(object sender, RoutedEventArgs e)
        {
            ((StackPanel)this.currentPage).Visibility = Visibility.Collapsed;
            this.WelcomePage.Visibility = Visibility.Visible;
            this.currentPage = this.WelcomePage;
        }

        private void About(object sender, RoutedEventArgs e)
        {
            if (_hasPermission.CheckPermission(this.usrLevel, "Menu:AboutPage"))
            {
                ((StackPanel)this.currentPage).Visibility = Visibility.Collapsed;
                this.AboutPage.Visibility = Visibility.Visible;
                this.currentPage = this.AboutPage;
            }
        }

        private void ShowExamHall(object sender, RoutedEventArgs e)
        {
            if (_hasPermission.CheckPermission(this.usrLevel, "Menu:ExamHallManagement"))
            {
                ((StackPanel)this.currentPage).Visibility = Visibility.Collapsed;
                this.ExamHallManagement.Visibility = Visibility.Visible;
                this.currentPage = this.ExamHallManagement;
            }
        }

        private void setupExamHall() // good luck future myself
        {
            var button = new Button();
            button.AccessKey = "test";
            this.ExamHallGrid.Children.Add(button);
        }

        private void CandidateList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.CandidateList.SelectedValue == null) { return; }
            string nameOfCandidate = this.CandidateList.SelectedValue.ToString();

            var getInfoAboutCandidate = ((string name, string number, string group))_sqlService.GetCandidateByNameAndYear(nameOfCandidate, this.selectedCandidateYear);

            if (getInfoAboutCandidate.name == "N/A")
            {
                this.CandidateInfo.Text = "Unable to find candidate.";
                this.selectedCandidate = "0";
            }

            this.CandidateInfo.Text = $"Name: {getInfoAboutCandidate.name} - Candidate Number: {getInfoAboutCandidate.number} - Candidate Year Group: {getInfoAboutCandidate.group} ";
            this.selectedCandidate = getInfoAboutCandidate.number;
        }

        private void GetCandidates(object sender, RoutedEventArgs e)
        {
            this.CandidateList.Items.Clear();
            this.selectedCandidateYear = "2008";
            var candidates = (IEnumerable < (string name, string number, string group) >)_sqlService.GetCandidates("2008");
            
            foreach (var (name, number, group) in candidates)
            {
                this.CandidateList.Items.Add($"{name}");
            }

            this.CandidateList.SelectionChanged += CandidateList_SelectionChanged;

        }

        private void CleanupCandidates()
        {
            this.CandidateList.Items.Clear();
            this.CandidateInfo.Text = "";
        }

        private void AddCandidate(object sender, RoutedEventArgs e)
        {
            string name = this.candidateName.Text;
            string number = this.candidateNumber.Text;
            string ageGroup = this.candidateAgeGroup.Text;

            _sqlService.AddCandidate(name, number, ageGroup);
            this.GetCandidates(sender, e);
        }

        private void RemoveCandidate(object sender, RoutedEventArgs e)
        {
            if (this.selectedCandidate != "0")
            {
                _sqlService.RemoveCandidate(this.selectedCandidate);
                this.selectedCandidate = "0";
                this.CandidateInfo.Text = "This candidate was deleted.";
                this.GetCandidates(sender, e);
            }
        }
        
        private void CheckCredentials(object sender, RoutedEventArgs e)
        {
            string user = this.user.Text;
            string password = this.password.Text;

            if (user == null || password == null)
            {
                return;
            }

            var checkUserCredentials = ((bool isUser, int level))_sqlService.CheckUserCredentials(user, password);

            if (checkUserCredentials.isUser)
            {
                Debug.WriteLine("Signed in!");
                this.username = user;
                this.signedIn = true;
                this.usrLevel = checkUserCredentials.level;
                this.signUserIn(true);
            } else
            {
                Debug.WriteLine("Invalid credentials");
            }
        }

        private void closeWindow(object sender, WindowEventArgs e)
        {
            _user32dll.RemoveTrayIcon();
        }

        private const int IDI_APPLICATION = 32512;
        private const int IMAGE_ICON = 1;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern nint GetModuleHandle(string lpModuleName);

        [DllImport("user32", CharSet = CharSet.Unicode)]
        private static extern nint LoadImage(nint hinst, nint name, int type, int cx, int cy, int fuLoad);

        // call this if/when you want to destroy the icon (window closed, etc.)
        [DllImport("user32")]
        private static extern bool DestroyIcon(nint hIcon);
    }
}