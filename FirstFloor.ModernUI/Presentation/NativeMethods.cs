// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace FirstFloor.ModernUI.Presentation
{
    internal static class NativeMethods
    {
        public delegate IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [Flags]
        public enum FolderBrowserOptions
        {
            None = 0,
            FolderOnly = 0x0001,
            FindComputer = 0x0002,
            ShowStatusText = 0x0004,
            ReturnAncestors = 0x0008,
            ShowEditBox = 0x0010,
            ValidateResult = 0x0020,
            UseNewStyle = 0x0040,
            UseNewStyleWithEditBox = (UseNewStyle | ShowEditBox),
            AllowUrls = 0x0080,
            ShowUsageHint = 0x0100,
            HideNewFolderButton = 0x0200,
            GetShortcuts = 0x0400,
            BrowseComputers = 0x1000,
            BrowsePrinters = 0x2000,
            BrowseFiles = 0x4000,
            BrowseShares = 0x8000
        }

        [SecurityCritical]
        public static IMalloc GetSHMalloc()
        {
            var ppMalloc = new IMalloc[1];
            SHGetMalloc(ppMalloc);
            return ppMalloc[0];
        }

        [SecurityCritical]
        [DllImport("shell32")]
        private static extern int SHGetMalloc([Out] [MarshalAs(UnmanagedType.LPArray)] IMalloc[] ppMalloc);

        [SecurityCritical]
        [DllImport("shell32")]
        public static extern int SHGetFolderLocation(IntPtr hwndOwner, Int32 nFolder, IntPtr hToken, uint dwReserved, out IntPtr ppidl);

        [SecurityCritical]
        [DllImport("shell32")]
        public static extern int SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] string pszName, IntPtr pbc, out IntPtr ppidl, uint sfgaoIn,
            out uint psfgaoOut);

        [SecurityCritical]
        [DllImport("shell32")]
        public static extern IntPtr SHBrowseForFolder(ref BROWSEINFO lbpi);

        [SecurityCritical]
        [DllImport("shell32", CharSet = CharSet.Auto)]
        public static extern bool SHGetPathFromIDList(IntPtr pidl, IntPtr pszPath);

        [StructLayout(LayoutKind.Sequential)]
        public struct BROWSEINFO
        {
            public IntPtr HwndOwner;
            public IntPtr Root;
            [MarshalAs(UnmanagedType.LPStr)]
            public string DisplayName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string Title;
            public uint Flags;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public WndProc Callback;
            public int LParam;
            public int Image;
        }

        [ComImport]
        [SuppressUnmanagedCodeSecurity]
        [Guid("00000002-0000-0000-c000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IMalloc
        {
            [PreserveSig]
            IntPtr Alloc(int cb);

            [PreserveSig]
            IntPtr Realloc(IntPtr pv, int cb);

            [PreserveSig]
            void Free(IntPtr pv);

            [PreserveSig]
            int GetSize(IntPtr pv);

            [PreserveSig]
            int DidAlloc(IntPtr pv);

            [PreserveSig]
            void HeapMinimize();
        }
    }
}