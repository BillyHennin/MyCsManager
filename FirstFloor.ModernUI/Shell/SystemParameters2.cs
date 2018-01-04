// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

using FirstFloor.ModernUI.Shell.Standard;

namespace FirstFloor.ModernUI.Shell
{
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class SystemParameters2 : INotifyPropertyChanged
    {
        [ThreadStatic]
        private static SystemParameters2 _threadLocalSingleton;
        private readonly Dictionary<WM, List<_SystemMetricUpdate>> _UpdateTable;
        private Rect _captionButtonLocation;
        private double _captionHeight;
        private Color _glassColor;
        private SolidColorBrush _glassColorBrush;
        private bool _isGlassEnabled;
        private bool _isHighContrast;
        private MessageWindow _messageHwnd;
        private Size _smallIconSize;
        private string _uxThemeColor;
        private string _uxThemeName;
        private CornerRadius _windowCornerRadius;
        private Thickness _windowNonClientFrameThickness;
        private Thickness _windowResizeBorderThickness;

        #region Initialization and Update Methods

        private void _InitializeIsGlassEnabled()
        {
            IsGlassEnabled = NativeMethods.DwmIsCompositionEnabled();
        }

        private void _UpdateIsGlassEnabled(IntPtr wParam, IntPtr lParam)
        {
            _InitializeIsGlassEnabled();
        }

        private void _InitializeGlassColor()
        {
            bool isOpaque;
            uint color;
            NativeMethods.DwmGetColorizationColor(out color, out isOpaque);
            color |= isOpaque ? 0xFF000000 : 0;
            WindowGlassColor = Utility.ColorFromArgbDword(color);
            var glassBrush = new SolidColorBrush(WindowGlassColor);
            glassBrush.Freeze();
            WindowGlassBrush = glassBrush;
        }

        private void _UpdateGlassColor(IntPtr wParam, IntPtr lParam)
        {
            var isOpaque = lParam != IntPtr.Zero;
            var color = unchecked((uint) (int) wParam.ToInt64());
            color |= isOpaque ? 0xFF000000 : 0;
            WindowGlassColor = Utility.ColorFromArgbDword(color);
            var glassBrush = new SolidColorBrush(WindowGlassColor);
            glassBrush.Freeze();
            WindowGlassBrush = glassBrush;
        }

        private void _InitializeCaptionHeight()
        {
            var ptCaption = new Point(0, NativeMethods.GetSystemMetrics(SM.CYCAPTION));
            WindowCaptionHeight = DpiHelper.DevicePixelsToLogical(ptCaption).Y;
        }

        private void _UpdateCaptionHeight(IntPtr wParam, IntPtr lParam)
        {
            _InitializeCaptionHeight();
        }

        private void _InitializeWindowResizeBorderThickness()
        {
            var frameSize = new Size(NativeMethods.GetSystemMetrics(SM.CXSIZEFRAME), NativeMethods.GetSystemMetrics(SM.CYSIZEFRAME));
            var frameSizeInDips = DpiHelper.DeviceSizeToLogical(frameSize);
            WindowResizeBorderThickness = new Thickness(frameSizeInDips.Width, frameSizeInDips.Height, frameSizeInDips.Width, frameSizeInDips.Height);
        }

        private void _UpdateWindowResizeBorderThickness(IntPtr wParam, IntPtr lParam)
        {
            _InitializeWindowResizeBorderThickness();
        }

        private void _InitializeWindowNonClientFrameThickness()
        {
            var frameSize = new Size(NativeMethods.GetSystemMetrics(SM.CXSIZEFRAME), NativeMethods.GetSystemMetrics(SM.CYSIZEFRAME));
            var frameSizeInDips = DpiHelper.DeviceSizeToLogical(frameSize);
            var captionHeight = NativeMethods.GetSystemMetrics(SM.CYCAPTION);
            var captionHeightInDips = DpiHelper.DevicePixelsToLogical(new Point(0, captionHeight)).Y;
            WindowNonClientFrameThickness = new Thickness(frameSizeInDips.Width, frameSizeInDips.Height + captionHeightInDips, frameSizeInDips.Width,
                frameSizeInDips.Height);
        }

        private void _UpdateWindowNonClientFrameThickness(IntPtr wParam, IntPtr lParam)
        {
            _InitializeWindowNonClientFrameThickness();
        }

        private void _InitializeSmallIconSize()
        {
            SmallIconSize = new Size(NativeMethods.GetSystemMetrics(SM.CXSMICON), NativeMethods.GetSystemMetrics(SM.CYSMICON));
        }

        private void _UpdateSmallIconSize(IntPtr wParam, IntPtr lParam)
        {
            _InitializeSmallIconSize();
        }

        private void _LegacyInitializeCaptionButtonLocation()
        {
            var captionX = NativeMethods.GetSystemMetrics(SM.CXSIZE);
            var captionY = NativeMethods.GetSystemMetrics(SM.CYSIZE);
            var frameX = NativeMethods.GetSystemMetrics(SM.CXSIZEFRAME) + NativeMethods.GetSystemMetrics(SM.CXEDGE);
            var frameY = NativeMethods.GetSystemMetrics(SM.CYSIZEFRAME) + NativeMethods.GetSystemMetrics(SM.CYEDGE);
            var captionRect = new Rect(0, 0, captionX * 3, captionY);
            captionRect.Offset(-frameX - captionRect.Width, frameY);
            WindowCaptionButtonsLocation = captionRect;
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        private void _InitializeCaptionButtonLocation()
        {
            if(!Utility.IsOSVistaOrNewer || !NativeMethods.IsThemeActive())
            {
                _LegacyInitializeCaptionButtonLocation();
                return;
            }
            var tbix = new TITLEBARINFOEX {cbSize = Marshal.SizeOf(typeof(TITLEBARINFOEX))};
            var lParam = Marshal.AllocHGlobal(tbix.cbSize);
            try
            {
                Marshal.StructureToPtr(tbix, lParam, false);

                NativeMethods.ShowWindow(_messageHwnd.Handle, SW.SHOW);
                NativeMethods.SendMessage(_messageHwnd.Handle, WM.GETTITLEBARINFOEX, IntPtr.Zero, lParam);
                tbix = (TITLEBARINFOEX) Marshal.PtrToStructure(lParam, typeof(TITLEBARINFOEX));
            }
            finally
            {
                NativeMethods.ShowWindow(_messageHwnd.Handle, SW.HIDE);
                Utility.SafeFreeHGlobal(ref lParam);
            }

            var rcAllCaptionButtons = RECT.Union(tbix.rgrect_CloseButton, tbix.rgrect_MinimizeButton);

            Assert.AreEqual(rcAllCaptionButtons, RECT.Union(rcAllCaptionButtons, tbix.rgrect_MaximizeButton));
            var rcWindow = NativeMethods.GetWindowRect(_messageHwnd.Handle);

            var deviceCaptionLocation = new Rect(rcAllCaptionButtons.Left - rcWindow.Width - rcWindow.Left, rcAllCaptionButtons.Top - rcWindow.Top,
                rcAllCaptionButtons.Width, rcAllCaptionButtons.Height);
            var logicalCaptionLocation = DpiHelper.DeviceRectToLogical(deviceCaptionLocation);
            WindowCaptionButtonsLocation = logicalCaptionLocation;
        }

        private void _UpdateCaptionButtonLocation(IntPtr wParam, IntPtr lParam)
        {
            _InitializeCaptionButtonLocation();
        }

        private void _InitializeHighContrast()
        {
            var hc = NativeMethods.SystemParameterInfo_GetHIGHCONTRAST();
            HighContrast = (hc.dwFlags & HCF.HIGHCONTRASTON) != 0;
        }

        private void _UpdateHighContrast(IntPtr wParam, IntPtr lParam)
        {
            _InitializeHighContrast();
        }

        private void _InitializeThemeInfo()
        {
            if(!NativeMethods.IsThemeActive())
            {
                UxThemeName = "Classic";
                UxThemeColor = "";
                return;
            }
            string name;
            string color;
            string size;
            NativeMethods.GetCurrentThemeName(out name, out color, out size);

            UxThemeName = Path.GetFileNameWithoutExtension(name);
            UxThemeColor = color;
        }

        private void _UpdateThemeInfo(IntPtr wParam, IntPtr lParam)
        {
            _InitializeThemeInfo();
        }

        private void _InitializeWindowCornerRadius()
        {
            Assert.IsNeitherNullNorEmpty(UxThemeName);

            var cornerRadius = default(CornerRadius);

            switch(UxThemeName.ToUpperInvariant())
            {
                case "LUNA":
                    cornerRadius = new CornerRadius(6, 6, 0, 0);
                    break;
                case "AERO":
                    cornerRadius = NativeMethods.DwmIsCompositionEnabled() ? new CornerRadius(8) : new CornerRadius(6, 6, 0, 0);
                    break;
                default:
                    cornerRadius = new CornerRadius(0);
                    break;
            }
            WindowCornerRadius = cornerRadius;
        }

        private void _UpdateWindowCornerRadius(IntPtr wParam, IntPtr lParam)
        {
            _InitializeWindowCornerRadius();
        }

        #endregion

        private SystemParameters2()
        {
            _messageHwnd = new MessageWindow(0, WS.OVERLAPPEDWINDOW | WS.DISABLED, 0, new Rect(-16000, -16000, 100, 100), "", _WndProc);
            _messageHwnd.Dispatcher.ShutdownStarted += (sender, e) => Utility.SafeDispose(ref _messageHwnd);

            _InitializeIsGlassEnabled();
            _InitializeGlassColor();
            _InitializeCaptionHeight();
            _InitializeWindowNonClientFrameThickness();
            _InitializeWindowResizeBorderThickness();
            _InitializeCaptionButtonLocation();
            _InitializeSmallIconSize();
            _InitializeHighContrast();
            _InitializeThemeInfo();

            _InitializeWindowCornerRadius();
            _UpdateTable = new Dictionary<WM, List<_SystemMetricUpdate>>
            {
                {
                    WM.THEMECHANGED,
                    new List<_SystemMetricUpdate> {_UpdateThemeInfo, _UpdateHighContrast, _UpdateWindowCornerRadius, _UpdateCaptionButtonLocation,}
                },
                {
                    WM.SETTINGCHANGE,
                    new List<_SystemMetricUpdate>
                    {
                        _UpdateCaptionHeight,
                        _UpdateWindowResizeBorderThickness,
                        _UpdateSmallIconSize,
                        _UpdateHighContrast,
                        _UpdateWindowNonClientFrameThickness,
                        _UpdateCaptionButtonLocation,
                    }
                },
                {WM.DWMNCRENDERINGCHANGED, new List<_SystemMetricUpdate> {_UpdateIsGlassEnabled}},
                {WM.DWMCOMPOSITIONCHANGED, new List<_SystemMetricUpdate> {_UpdateIsGlassEnabled}},
                {WM.DWMCOLORIZATIONCOLORCHANGED, new List<_SystemMetricUpdate> {_UpdateGlassColor}},
            };
        }

        public static SystemParameters2 Current { get { return _threadLocalSingleton ?? (_threadLocalSingleton = new SystemParameters2()); } }

        public bool IsGlassEnabled
        {
            get { return NativeMethods.DwmIsCompositionEnabled(); }
            private set
            {
                if(value != _isGlassEnabled)
                {
                    _isGlassEnabled = value;
                    _NotifyPropertyChanged("IsGlassEnabled");
                }
            }
        }

        public Color WindowGlassColor
        {
            get { return _glassColor; }
            private set
            {
                if(value != _glassColor)
                {
                    _glassColor = value;
                    _NotifyPropertyChanged("WindowGlassColor");
                }
            }
        }

        public SolidColorBrush WindowGlassBrush
        {
            get { return _glassColorBrush; }
            private set
            {
                Assert.IsNotNull(value);
                Assert.IsTrue(value.IsFrozen);
                if(_glassColorBrush == null || value.Color != _glassColorBrush.Color)
                {
                    _glassColorBrush = value;
                    _NotifyPropertyChanged("WindowGlassBrush");
                }
            }
        }

        public Thickness WindowResizeBorderThickness
        {
            get { return _windowResizeBorderThickness; }
            private set
            {
                if(value != _windowResizeBorderThickness)
                {
                    _windowResizeBorderThickness = value;
                    _NotifyPropertyChanged("WindowResizeBorderThickness");
                }
            }
        }

        public Thickness WindowNonClientFrameThickness
        {
            get { return _windowNonClientFrameThickness; }
            private set
            {
                if(value != _windowNonClientFrameThickness)
                {
                    _windowNonClientFrameThickness = value;
                    _NotifyPropertyChanged("WindowNonClientFrameThickness");
                }
            }
        }

        public double WindowCaptionHeight
        {
            get { return _captionHeight; }
            private set
            {
                if(value != _captionHeight)
                {
                    _captionHeight = value;
                    _NotifyPropertyChanged("WindowCaptionHeight");
                }
            }
        }

        public Size SmallIconSize
        {
            get { return new Size(_smallIconSize.Width, _smallIconSize.Height); }
            private set
            {
                if(value != _smallIconSize)
                {
                    _smallIconSize = value;
                    _NotifyPropertyChanged("SmallIconSize");
                }
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifieMANAGERhouldBeCasedCorrectly", MessageId = "Ux")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifieMANAGERhouldBeSpelledCorrectly", MessageId = "Ux")]
        public string UxThemeName
        {
            get { return _uxThemeName; }
            private set
            {
                if(value != _uxThemeName)
                {
                    _uxThemeName = value;
                    _NotifyPropertyChanged("UxThemeName");
                }
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifieMANAGERhouldBeCasedCorrectly", MessageId = "Ux")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifieMANAGERhouldBeSpelledCorrectly", MessageId = "Ux")]
        public string UxThemeColor
        {
            get { return _uxThemeColor; }
            private set
            {
                if(value != _uxThemeColor)
                {
                    _uxThemeColor = value;
                    _NotifyPropertyChanged("UxThemeColor");
                }
            }
        }

        public bool HighContrast
        {
            get { return _isHighContrast; }
            private set
            {
                if(value != _isHighContrast)
                {
                    _isHighContrast = value;
                    _NotifyPropertyChanged("HighContrast");
                }
            }
        }

        public CornerRadius WindowCornerRadius
        {
            get { return _windowCornerRadius; }
            private set
            {
                if(value != _windowCornerRadius)
                {
                    _windowCornerRadius = value;
                    _NotifyPropertyChanged("WindowCornerRadius");
                }
            }
        }

        public Rect WindowCaptionButtonsLocation
        {
            get { return _captionButtonLocation; }
            private set
            {
                if(value != _captionButtonLocation)
                {
                    _captionButtonLocation = value;
                    _NotifyPropertyChanged("WindowCaptionButtonsLocation");
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void _NotifyPropertyChanged(string propertyName)
        {
            Assert.IsNeitherNullNorEmpty(propertyName);
            var handler = PropertyChanged;
            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        private IntPtr _WndProc(IntPtr hwnd, WM msg, IntPtr wParam, IntPtr lParam)
        {
            if(_UpdateTable != null)
            {
                List<_SystemMetricUpdate> handlers;
                if(_UpdateTable.TryGetValue(msg, out handlers))
                {
                    Assert.IsNotNull(handlers);
                    foreach(var handler in handlers)
                    {
                        handler(wParam, lParam);
                    }
                }
            }
            return NativeMethods.DefWindowProc(hwnd, msg, wParam, lParam);
        }

        private delegate void _SystemMetricUpdate(IntPtr wParam, IntPtr lParam);
    }
}