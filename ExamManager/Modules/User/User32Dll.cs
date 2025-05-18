using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

// This script handles all the tray icon stuff

namespace ExamManager.Modules.User
{
    internal class User32Dll
    {
        private const int WM_USER = 0x0400;
        private const int WM_TRAYICON = WM_USER + 1;

        private nint _hWnd;
        private NotifyIconData _notifyIconData;

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern bool Shell_NotifyIcon(NotifyIconMessage dwMessage, ref NotifyIconData lpData);

        [DllImport("user32.dll")]
        private static extern nint LoadIcon(nint hInstance, nint lpIconName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern nint LoadImage(nint hInstance, string lpName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

        private const uint IMAGE_ICON = 1;
        private const uint LR_LOADFROMFILE = 0x00000010;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct NotifyIconData
        {
            public int cbSize;
            public nint hWnd;
            public int uID;
            public NotifyIconFlags uFlags;
            public int uCallbackMessage;
            public nint hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szTip;
        }

        [Flags]
        private enum NotifyIconFlags
        {
            Message = 0x01,
            Icon = 0x02,
            Tip = 0x04
        }

        private enum NotifyIconMessage
        {
            Add = 0x00,
            Modify = 0x01,
            Delete = 0x02
        }

        public void InitializeTrayIcon(MainWindow windows, nint icon)
        {
            // Get the window handle for the main window
            _hWnd = WinRT.Interop.WindowNative.GetWindowHandle(windows);

            // ---------Load a custom icon from a file ---------
            nint customIcon = icon;
            if (customIcon == nint.Zero)
            {
                System.Diagnostics.Debug.WriteLine("Failed to load custom icon. Using default icon.");
                customIcon = LoadIcon(nint.Zero, new nint(32512)); // Default application icon
            }

            // Populate the NotifyIconData structure
            _notifyIconData = new NotifyIconData
            {
                cbSize = Marshal.SizeOf(typeof(NotifyIconData)),
                hWnd = _hWnd,
                uID = 1,
                uFlags = NotifyIconFlags.Message | NotifyIconFlags.Icon | NotifyIconFlags.Tip,
                uCallbackMessage = WM_TRAYICON,
                hIcon = customIcon, // Use the custom icon
                szTip = "Exam Manager" // Tooltip text
            };

            // Add the icon to the system tray
            bool result = Shell_NotifyIcon(NotifyIconMessage.Add, ref _notifyIconData);
            if (!result)
            {
                System.Diagnostics.Debug.WriteLine("Failed to add tray icon.");
            } else
            {
                Debug.WriteLine("Started Tray Icon.");
            }
        }

        public void RemoveTrayIcon()
        {
            // Remove the icon from the system tray
            Shell_NotifyIcon(NotifyIconMessage.Delete, ref _notifyIconData);
        }
    }
}
