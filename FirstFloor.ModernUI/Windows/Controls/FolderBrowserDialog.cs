// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

using FirstFloor.ModernUI.Presentation;

using Microsoft.Win32;

namespace FirstFloor.ModernUI.Windows.Controls
{
    public enum RootType
    {
        SpecialFolder,
        Path,
    }

    public sealed class FolderBrowserDialog : CommonDialog
    {
        private NativeMethods.FolderBrowserOptions _dialogOptions;

        [SecurityCritical]
        public FolderBrowserDialog()
        {
            Initialize();
        }

        public RootType RootType { get; set; }

        public string RootPath { get; set; }

        public Environment.SpecialFolder RootSpecialFolder { get; set; }

        public string SelectedPath { get; set; }

        public string Title { get; set; }

        public bool BrowseFiles
        {
            get { return GetOption(NativeMethods.FolderBrowserOptions.BrowseFiles); }
            [SecurityCritical] set { SetOption(NativeMethods.FolderBrowserOptions.BrowseFiles, value); }
        }

        public bool ShowEditBox
        {
            get { return GetOption(NativeMethods.FolderBrowserOptions.ShowEditBox); }
            [SecurityCritical] set { SetOption(NativeMethods.FolderBrowserOptions.ShowEditBox, value); }
        }

        public bool BrowseShares
        {
            get { return GetOption(NativeMethods.FolderBrowserOptions.BrowseShares); }
            [SecurityCritical] set { SetOption(NativeMethods.FolderBrowserOptions.BrowseShares, value); }
        }

        public bool ShowStatusText
        {
            get { return GetOption(NativeMethods.FolderBrowserOptions.ShowStatusText); }
            [SecurityCritical] set { SetOption(NativeMethods.FolderBrowserOptions.ShowStatusText, value); }
        }

        public bool ValidateResult
        {
            get { return GetOption(NativeMethods.FolderBrowserOptions.ValidateResult); }
            [SecurityCritical] set { SetOption(NativeMethods.FolderBrowserOptions.ValidateResult, value); }
        }

        [SecurityCritical]
        public override void Reset()
        {
            new FileIOPermission(PermissionState.Unrestricted).Demand();

            Initialize();
        }

        [SecurityCritical]
        protected override bool RunDialog(IntPtr hwndOwner)
        {
            var result = false;

            IntPtr pidlRoot = IntPtr.Zero, pszPath = IntPtr.Zero, pidlSelected = IntPtr.Zero;

            SelectedPath = string.Empty;

            try
            {
                if(RootType == RootType.SpecialFolder)
                {
                    NativeMethods.SHGetFolderLocation(hwndOwner, (int) RootSpecialFolder, IntPtr.Zero, 0, out pidlRoot);
                }
                else
                {
                    uint iAttribute;
                    NativeMethods.SHParseDisplayName(RootPath, IntPtr.Zero, out pidlRoot, 0, out iAttribute);
                }

                var browseInfo = new NativeMethods.BROWSEINFO
                {
                    HwndOwner = hwndOwner,
                    Root = pidlRoot,
                    DisplayName = new string(' ', 256),
                    Title = Title,
                    Flags = (uint) _dialogOptions,
                    LParam = 0,
                    Callback = HookProc
                };

                pidlSelected = NativeMethods.SHBrowseForFolder(ref browseInfo);

                if(pidlSelected != IntPtr.Zero)
                {
                    result = true;

                    pszPath = Marshal.AllocHGlobal((260 * Marshal.SystemDefaultCharSize));
                    NativeMethods.SHGetPathFromIDList(pidlSelected, pszPath);

                    SelectedPath = Marshal.PtrToStringAuto(pszPath);
                }
            }
            finally
            {
                var malloc = NativeMethods.GetSHMalloc();

                if(pidlRoot != IntPtr.Zero)
                {
                    malloc.Free(pidlRoot);
                }

                if(pidlSelected != IntPtr.Zero)
                {
                    malloc.Free(pidlSelected);
                }

                if(pszPath != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pszPath);
                }

                Marshal.ReleaseComObject(malloc);
            }

            return result;
        }

        [SecurityCritical]
        private void Initialize()
        {
            RootType = RootType.SpecialFolder;
            RootSpecialFolder = Environment.SpecialFolder.Desktop;
            RootPath = string.Empty;
            Title = string.Empty;

            _dialogOptions = NativeMethods.FolderBrowserOptions.BrowseFiles | NativeMethods.FolderBrowserOptions.ShowEditBox
                             | NativeMethods.FolderBrowserOptions.UseNewStyle | NativeMethods.FolderBrowserOptions.BrowseShares
                             | NativeMethods.FolderBrowserOptions.ShowStatusText | NativeMethods.FolderBrowserOptions.ValidateResult;
        }

        private bool GetOption(NativeMethods.FolderBrowserOptions option)
        {
            return ((_dialogOptions & option) != NativeMethods.FolderBrowserOptions.None);
        }

        [SecurityCritical]
        private void SetOption(NativeMethods.FolderBrowserOptions option, bool value)
        {
            if(value)
            {
                _dialogOptions |= option;
            }
            else
            {
                _dialogOptions &= ~option;
            }
        }
    }
}