// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

using FirstFloor.ModernUI.Shell.Standard;

namespace FirstFloor.ModernUI.Shell
{
    public static class SystemCommands
    {
        static SystemCommands()
        {
            CloseWindowCommand = new RoutedCommand("CloseWindow", typeof(SystemCommands));
            MaximizeWindowCommand = new RoutedCommand("MaximizeWindow", typeof(SystemCommands));
            MinimizeWindowCommand = new RoutedCommand("MinimizeWindow", typeof(SystemCommands));
            RestoreWindowCommand = new RoutedCommand("RestoreWindow", typeof(SystemCommands));
            ShowSystemMenuCommand = new RoutedCommand("ShowSystemMenu", typeof(SystemCommands));
        }

        public static RoutedCommand CloseWindowCommand { get; private set; }
        public static RoutedCommand MaximizeWindowCommand { get; private set; }
        public static RoutedCommand MinimizeWindowCommand { get; private set; }
        public static RoutedCommand RestoreWindowCommand { get; private set; }
        public static RoutedCommand ShowSystemMenuCommand { get; private set; }

        private static void _PostSystemCommand(Window window, SC command)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            if(hwnd == IntPtr.Zero || !NativeMethods.IsWindow(hwnd))
            {
                return;
            }
            NativeMethods.PostMessage(hwnd, WM.SYSCOMMAND, new IntPtr((int) command), IntPtr.Zero);
        }

        public static void CloseWindow(Window window)
        {
            Verify.IsNotNull(window, "window");
            _PostSystemCommand(window, SC.CLOSE);
        }

        public static void MaximizeWindow(Window window)
        {
            Verify.IsNotNull(window, "window");
            _PostSystemCommand(window, SC.MAXIMIZE);
        }

        public static void MinimizeWindow(Window window)
        {
            Verify.IsNotNull(window, "window");
            _PostSystemCommand(window, SC.MINIMIZE);
        }

        public static void RestoreWindow(Window window)
        {
            Verify.IsNotNull(window, "window");
            _PostSystemCommand(window, SC.RESTORE);
        }

        public static void ShowSystemMenu(Window window, Point screenLocation)
        {
            Verify.IsNotNull(window, "window");
            ShowSystemMenuPhysicalCoordinates(window, DpiHelper.LogicalPixelsToDevice(screenLocation));
        }

        internal static void ShowSystemMenuPhysicalCoordinates(Window window, Point physicalScreenLocation)
        {
            const uint TPM_RETURNCMD = 0x0100;
            const uint TPM_LEFTBUTTON = 0x0;
            Verify.IsNotNull(window, "window");
            var hwnd = new WindowInteropHelper(window).Handle;
            if(hwnd == IntPtr.Zero || !NativeMethods.IsWindow(hwnd))
            {
                return;
            }
            var hmenu = NativeMethods.GetSystemMenu(hwnd, false);
            var cmd = NativeMethods.TrackPopupMenuEx(hmenu, TPM_LEFTBUTTON | TPM_RETURNCMD, (int) physicalScreenLocation.X, (int) physicalScreenLocation.Y, hwnd,
                IntPtr.Zero);
            if(0 != cmd)
            {
                NativeMethods.PostMessage(hwnd, WM.SYSCOMMAND, new IntPtr(cmd), IntPtr.Zero);
            }
        }
    }
}