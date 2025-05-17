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

// The main window for Exam Manager

namespace ExamManager
{
    public sealed partial class MainWindow : Window
    {
        private AppWindow _appWindow;
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

        }

        private void SignInShow(object sender, RoutedEventArgs e)
        {
            this.SignedOutPage.Visibility = Visibility.Collapsed;
            this.SignedInPage.Visibility = Visibility.Visible;
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