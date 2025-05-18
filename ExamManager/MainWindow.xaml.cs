using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT.Interop;
using Microsoft.UI.Windowing;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.UI.Xaml.Automation.Peers;
using ExamManager.Modules.User;
using ExamManager.Modules.Util;
using System.Timers;
using System.Threading.Tasks;
using System.Threading;

// The main window for Exam Manager

namespace ExamManager
{
    public sealed partial class MainWindow : Window
    {
        private AppWindow _appWindow;
        private User32Dll _user32dll;
        private SQLService _sqlService;

        private string username;
        private bool signedIn;

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

            // Setup testing user
            if (!_sqlService.CheckTestUser("Test"))
            {
                _sqlService.AddUserToDB("Test", "Test", "Admin");
            }

            this.signedIn = false;
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
                this.UserName.Text = $"Welcome, {this.username}!";
            } else
            {
                this.SignedOutPage.Visibility = Visibility.Visible;
                this.Homepage.Visibility = Visibility.Collapsed;
                this.UserName.Text = "OnceIWasTwentyYearsOld";
                this.signedIn = false;
                this.user.Text = "";
                this.password.Text = "";
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
            this.signUserIn(false);
        }

        private void CheckCredentials(object sender, RoutedEventArgs e)
        {
            string user = this.user.Text;
            string password = this.password.Text;

            if (user == null || password == null)
            {
                return;
            }

            if (_sqlService.CheckUserCredentials(user, password))
            {
                Debug.WriteLine("Signed in!");
                this.username = user;
                this.signedIn = true;
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