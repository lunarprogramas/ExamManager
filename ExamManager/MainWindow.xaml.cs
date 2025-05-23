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
using System.Data.Entity.Core.Common.EntitySql;

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
        private ExamHallManager _examHallLayout;

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

            // Setup the exam hall stuff
            _examHallLayout = new ExamHallManager();
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

        private void ShowExaminers(object sender, RoutedEventArgs e)
        {
            if (_hasPermission.CheckPermission(this.usrLevel, "Menu:ExaminerManagement"))
            {
                ((StackPanel)this.currentPage).Visibility = Visibility.Collapsed;
                this.ExaminerManagement.Visibility = Visibility.Visible;
                this.currentPage = this.ExaminerManagement;
            }
        }

        private void AddExaminer(object sender, RoutedEventArgs e)
        {
            var username = this.examinerName.Text;
            string password = "ExamManagerExaminer";
            if (username.Length > 0) {
                _sqlService.AddUserToDB(username, password, "2");
                this.GetExaminers(sender, e);
            }
        }

        private void RemoveExaminer(object sender, RoutedEventArgs e)
        {
            var username = this.ExaminerList.SelectedValue.ToString();

            if (_sqlService.RemoveUser(username)) {
                this.GetExaminers(sender, e);
                this.ExaminerInfo.Text = "This examiner was removed";
            }
        }

        private void GetExaminers(object sender, RoutedEventArgs e)
        {
            var users = (IEnumerable<(string username, string userType)>)_sqlService.GetUsers();
            this.ExaminerList.Items.Clear();
            foreach (var user in users) {
                this.ExaminerList.Items.Add($"{user.username}");
            }

            this.ExaminerList.SelectionChanged += ExaminerList_SelectionChanged;
        }

        private void ExaminerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ExaminerList.SelectedValue == null) { return; }
            var users = (IEnumerable<(string username, string userType)>)_sqlService.GetUsers();
            foreach (var user in users)
            {
                if (user.username == this.ExaminerList.SelectedValue.ToString())
                {
                    this.ExaminerInfo.Text = $"Username: {user.username} : UserLevel: {user.userType}";
                }
            }
        }

        private void SeeSeatingPlan(object sender, RoutedEventArgs e)
        {
            this.SetupExamHall();
        }

        private void SetupExamHall() // good luck future myself
        {
            string selectedYear = this.GetCandidateYear.Text;

            _examHallLayout.ResetExamHall();
            
            if (selectedYear.Length > 0)
            {
                var candidates = (IEnumerable<(string name, string number, string group)>)_sqlService.GetCandidates(selectedYear);
                var grid = this.ExamHallGrid;

                grid.Children.Clear();

                foreach (var (name, number, group) in candidates)
                {
                    var seatConfig = ((string RowString, int RowInt, int Seat))_examHallLayout.DetermineCandidateSeat();

                    var button = new Button();
                    button.Content = $"{seatConfig.RowString}{seatConfig.Seat}";

                    Debug.WriteLine($"{seatConfig.RowInt}");

                    Grid.SetColumn(button, seatConfig.RowInt);
                    Grid.SetRow(button, seatConfig.Seat);

                    grid.Children.Add(button);

                    button.Click += (object sender, RoutedEventArgs e) =>
                    {
                        this.CandidateInfoExam.Text = $"Name: {name} Candidate Number: {number}";
                    };
                }
            } else
            {
                var candidates = (IEnumerable<(string name, string number, string group)>)_sqlService.GetCandidates(DateTime.Now.Year.ToString());
                var grid = this.ExamHallGrid;

                grid.Children.Clear();

                foreach (var (name, number, group) in candidates)
                {
                    var seatConfig = ((string RowString, int RowInt, int Seat))_examHallLayout.DetermineCandidateSeat();

                    var button = new Button();
                    button.Content = $"{seatConfig.RowString}{seatConfig.Seat}";

                    Grid.SetColumn(button, seatConfig.RowInt);
                    Grid.SetRow(button, seatConfig.Seat);

                    grid.Children.Add(button);

                    button.Click += (object sender, RoutedEventArgs e) =>
                    {
                        this.CandidateInfoExam.Text = $"Name: {name} Candidate Number: {number}";
                    };
                }
            }
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
            if (this.candidateAgeGroup.Text.Length == 0)
            {
                this.selectedCandidateYear = DateTime.Now.Year.ToString();
            } else
            {
                this.selectedCandidateYear = this.candidateAgeGroup.Text;
            }

            var candidates = (IEnumerable < (string name, string number, string group) >)_sqlService.GetCandidates(this.selectedCandidateYear);
            
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