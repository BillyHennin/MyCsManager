// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

using FirstFloor.ModernUI.Shell.Standard;

namespace FirstFloor.ModernUI.Shell
{
    public enum TaskbarItemProgressState
    {
        None,
        Indeterminate,
        Normal,
        Error,
        Paused,
    }

    public sealed class TaskbarItemInfo : Freezable
    {
        private const int c_MaximumThumbButtons = 7;

        private static readonly WM WM_TASKBARBUTTONCREATED = NativeMethods.RegisterWindowMessage("TaskbarButtonCreated");
        private static readonly Thickness _EmptyThickness = new Thickness();
        private readonly Size _overlaySize;
        private readonly ITaskbarList3 _taskbarList;
        private SafeGdiplusStartupToken _gdipToken;
        private bool _haveAddedButtons;
        private HwndSource _hwndSource;
        private bool _isAttached;
        private Window _window;

        public TaskbarItemInfo()
        {
            if(!DesignerProperties.GetIsInDesignMode(this))
            {
                ITaskbarList taskbarList = null;
                try
                {
                    taskbarList = CLSID.CoCreateInstance<ITaskbarList>(CLSID.TaskbarList);
                    taskbarList.HrInit();

                    _taskbarList = taskbarList as ITaskbarList3;
                    taskbarList = null;
                }
                finally
                {
                    Utility.SafeRelease(ref taskbarList);
                }
                _overlaySize = new Size(NativeMethods.GetSystemMetrics(SM.CXSMICON), NativeMethods.GetSystemMetrics(SM.CYSMICON));
            }

            ThumbButtonInfos = new ThumbButtonInfoCollection();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new TaskbarItemInfo();
        }

        private IntPtr _GetHICONFromImageSource(ImageSource image, Size dimensions)
        {
            if(null == _gdipToken)
            {
                _gdipToken = SafeGdiplusStartupToken.Startup();
            }
            return Utility.GenerateHICON(image, dimensions);
        }

        private void _SetWindow(Window window)
        {
            Assert.IsNull(_window);
            if(null == window)
            {
                return;
            }
            _window = window;

            if(_taskbarList == null)
            {
                return;
            }

            var hwnd = new WindowInteropHelper(_window).Handle;
            var isAttached = hwnd != IntPtr.Zero;
            if(!isAttached)
            {
                _window.SourceInitialized += _OnWindowSourceInitialized;
            }
            else
            {
                _hwndSource = HwndSource.FromHwnd(hwnd);
                _hwndSource.AddHook(_WndProc);
                _OnIsAttachedChanged(true);
            }
        }

        private void _OnWindowSourceInitialized(object sender, EventArgs e)
        {
            _window.SourceInitialized -= _OnWindowSourceInitialized;
            var hwnd = new WindowInteropHelper(_window).Handle;
            _hwndSource = HwndSource.FromHwnd(hwnd);

            _hwndSource.AddHook(_WndProc);

            MSGFLTINFO dontCare;
            NativeMethods.ChangeWindowMessageFilterEx(hwnd, WM_TASKBARBUTTONCREATED, MSGFLT.ALLOW, out dontCare);
            NativeMethods.ChangeWindowMessageFilterEx(hwnd, WM.COMMAND, MSGFLT.ALLOW, out dontCare);
        }

        private IntPtr _WndProc(IntPtr hwnd, int uMsg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            var message = (WM) uMsg;
            if(message == WM_TASKBARBUTTONCREATED)
            {
                _OnIsAttachedChanged(true);
                _isAttached = true;
                handled = false;
            }
            else
            {
                switch(message)
                {
                    case WM.COMMAND:
                        if(Utility.HIWORD(wParam.ToInt32()) == THUMBBUTTON.THBN_CLICKED)
                        {
                            var index = Utility.LOWORD(wParam.ToInt32());
                            ThumbButtonInfos[index].InvokeClick();
                            handled = true;
                        }
                        break;
                    case WM.SIZE:
                        _UpdateThumbnailClipping(_isAttached);
                        handled = false;
                        break;
                }
            }
            return IntPtr.Zero;
        }

        private void _OnIsAttachedChanged(bool attached)
        {
            if(attached)
            {
                Assert.IsNotNull(_window);
                Assert.IsNotNull(_hwndSource);
            }

            _haveAddedButtons = false;

            if(!attached && _hwndSource == null)
            {
                return;
            }
            _UpdateOverlay(attached);
            _UpdateProgressState(attached);
            _UpdateProgressValue(attached);
            _UpdateTooltip(attached);
            _UpdateThumbnailClipping(attached);
            _UpdateThumbButtons(attached);
            if(!attached)
            {
                _hwndSource = null;
            }
        }

        private void _DetachWindow()
        {
            Assert.IsNotNull(_window);

            _window.SourceInitialized -= _OnWindowSourceInitialized;

            _isAttached = false;
            _OnIsAttachedChanged(false);
            _window = null;
        }

        private HRESULT _UpdateOverlay(bool attached)
        {
            var source = Overlay;

            if(null == source || !attached)
            {
                return _taskbarList.SetOverlayIcon(_hwndSource.Handle, IntPtr.Zero, null);
            }
            var hicon = IntPtr.Zero;
            try
            {
                hicon = _GetHICONFromImageSource(source, _overlaySize);
                return _taskbarList.SetOverlayIcon(_hwndSource.Handle, hicon, null);
            }
            finally
            {
                Utility.SafeDestroyIcon(ref hicon);
            }
        }

        private HRESULT _UpdateTooltip(bool attached)
        {
            var tooltip = Description ?? "";
            if(!attached)
            {
                tooltip = "";
            }
            return _taskbarList.SetThumbnailTooltip(_hwndSource.Handle, tooltip);
        }

        private HRESULT _UpdateProgressValue(bool attached)
        {
            if(!attached || ProgressState == TaskbarItemProgressState.None || ProgressState == TaskbarItemProgressState.Indeterminate)
            {
                return HRESULT.S_OK;
            }
            const ulong precisionValue = 1000;

            Assert.BoundedDoubleInc(0, ProgressValue, 1);
            var intValue = (ulong) (ProgressValue * precisionValue);
            return _taskbarList.SetProgressValue(_hwndSource.Handle, intValue, precisionValue);
        }

        private HRESULT _UpdateProgressState(bool attached)
        {
            var ps = ProgressState;
            var tbpf = TBPF.NOPROGRESS;
            if(attached)
            {
                switch(ps)
                {
                    case TaskbarItemProgressState.Error:
                        tbpf = TBPF.ERROR;
                        break;
                    case TaskbarItemProgressState.Indeterminate:
                        tbpf = TBPF.INDETERMINATE;
                        break;
                    case TaskbarItemProgressState.None:
                        tbpf = TBPF.NOPROGRESS;
                        break;
                    case TaskbarItemProgressState.Normal:
                        tbpf = TBPF.NORMAL;
                        break;
                    case TaskbarItemProgressState.Paused:
                        tbpf = TBPF.PAUSED;
                        break;
                    default:

                        Assert.Fail();
                        tbpf = TBPF.NOPROGRESS;
                        break;
                }
            }
            var hr = _taskbarList.SetProgressState(_hwndSource.Handle, tbpf);
            if(hr.Succeeded)
            {
                hr = _UpdateProgressValue(attached);
            }
            return hr;
        }

        private HRESULT _UpdateThumbnailClipping(bool attached)
        {
            Assert.IsNotNull(_window);
            RefRECT interopRc = null;
            if(attached && ThumbnailClipMargin != _EmptyThickness)
            {
                var margin = ThumbnailClipMargin;

                var physicalClientRc = NativeMethods.GetClientRect(_hwndSource.Handle);
                var logicalClientRc =
                    DpiHelper.DeviceRectToLogical(new Rect(physicalClientRc.Left, physicalClientRc.Top, physicalClientRc.Width, physicalClientRc.Height));

                if(margin.Left + margin.Right >= logicalClientRc.Width || margin.Top + margin.Bottom >= logicalClientRc.Height)
                {
                    interopRc = new RefRECT(0, 0, 0, 0);
                }
                else
                {
                    var logicalClip = new Rect(margin.Left, margin.Top, logicalClientRc.Width - margin.Left - margin.Right,
                        logicalClientRc.Height - margin.Top - margin.Bottom);
                    var physicalClip = DpiHelper.LogicalRectToDevice(logicalClip);
                    interopRc = new RefRECT((int) physicalClip.Left, (int) physicalClip.Top, (int) physicalClip.Right, (int) physicalClip.Bottom);
                }
            }

            var hr = _taskbarList.SetThumbnailClip(_hwndSource.Handle, interopRc);
            Assert.IsTrue(hr.Succeeded);
            return hr;
        }

        private HRESULT _RegisterThumbButtons()
        {
            var hr = HRESULT.S_OK;
            if(!_haveAddedButtons)
            {
                var nativeButtons = new THUMBBUTTON[c_MaximumThumbButtons];
                for(var i = 0; i < c_MaximumThumbButtons; ++i)
                {
                    nativeButtons[i] = new THUMBBUTTON
                    {
                        iId = (uint) i,
                        dwFlags = THBF.NOBACKGROUND | THBF.DISABLED | THBF.HIDDEN,
                        dwMask = THB.FLAGS | THB.ICON | THB.TOOLTIP
                    };
                }

                hr = _taskbarList.ThumbBarAddButtons(_hwndSource.Handle, (uint) nativeButtons.Length, nativeButtons);
                if(hr == HRESULT.E_INVALIDARG)
                {
                    hr = HRESULT.S_FALSE;
                }
                _haveAddedButtons = hr.Succeeded;
            }
            return hr;
        }

        private HRESULT _UpdateThumbButtons(bool attached)
        {
            var nativeButtons = new THUMBBUTTON[c_MaximumThumbButtons];
            var hr = _RegisterThumbButtons();
            if(hr.Failed)
            {
                return hr;
            }
            var thumbButtons = ThumbButtonInfos;
            try
            {
                uint currentButton = 0;
                if(attached && null != thumbButtons)
                {
                    foreach(var wrappedTB in thumbButtons)
                    {
                        var nativeTB = new THUMBBUTTON {iId = currentButton, dwMask = THB.FLAGS | THB.TOOLTIP | THB.ICON,};
                        switch(wrappedTB.Visibility)
                        {
                            case Visibility.Collapsed:

                                nativeTB.dwFlags = THBF.HIDDEN;
                                break;
                            case Visibility.Hidden:

                                nativeTB.dwFlags = THBF.DISABLED | THBF.NOBACKGROUND;
                                nativeTB.hIcon = IntPtr.Zero;
                                break;
                            default:
                                nativeTB.szTip = wrappedTB.Description ?? "";
                                nativeTB.hIcon = _GetHICONFromImageSource(wrappedTB.ImageSource, _overlaySize);
                                if(!wrappedTB.IsBackgroundVisible)
                                {
                                    nativeTB.dwFlags |= THBF.NOBACKGROUND;
                                }
                                if(!wrappedTB.IsEnabled)
                                {
                                    nativeTB.dwFlags |= THBF.DISABLED;
                                }
                                else
                                {
                                    nativeTB.dwFlags |= THBF.ENABLED;
                                }

                                if(!wrappedTB.IsInteractive)
                                {
                                    nativeTB.dwFlags |= THBF.NONINTERACTIVE;
                                }
                                if(wrappedTB.DismissWhenClicked)
                                {
                                    nativeTB.dwFlags |= THBF.DISMISSONCLICK;
                                }
                                break;
                        }
                        nativeButtons[currentButton] = nativeTB;
                        ++currentButton;
                        if(currentButton == c_MaximumThumbButtons)
                        {
                            break;
                        }
                    }
                }

                for(; currentButton < c_MaximumThumbButtons; ++currentButton)
                {
                    nativeButtons[currentButton] = new THUMBBUTTON
                    {
                        iId = currentButton,
                        dwFlags = THBF.NOBACKGROUND | THBF.DISABLED | THBF.HIDDEN,
                        dwMask = THB.FLAGS | THB.ICON | THB.TOOLTIP
                    };
                }

                return _taskbarList.ThumbBarUpdateButtons(_hwndSource.Handle, (uint) nativeButtons.Length, nativeButtons);
            }
            finally
            {
                foreach(var nativeButton in nativeButtons)
                {
                    var hico = nativeButton.hIcon;
                    if(IntPtr.Zero != hico)
                    {
                        Utility.SafeDestroyIcon(ref hico);
                    }
                }
            }
        }

        #region Attached Properties and support methods.

        public static readonly DependencyProperty TaskbarItemInfoProperty = DependencyProperty.RegisterAttached("TaskbarItemInfo", typeof(TaskbarItemInfo),
            typeof(TaskbarItemInfo), new PropertyMetadata(null, _OnTaskbarItemInfoChanged, _CoerceTaskbarItemInfoValue));

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static TaskbarItemInfo GetTaskbarItemInfo(Window window)
        {
            Verify.IsNotNull(window, "window");
            return (TaskbarItemInfo) window.GetValue(TaskbarItemInfoProperty);
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static void SetTaskbarItemInfo(Window window, TaskbarItemInfo value)
        {
            Verify.IsNotNull(window, "window");
            window.SetValue(TaskbarItemInfoProperty, value);
        }

        private static void _OnTaskbarItemInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(DesignerProperties.GetIsInDesignMode(d))
            {
                return;
            }
            var window = (Window) d;
            var oldbar = (TaskbarItemInfo) e.OldValue;
            var newbar = (TaskbarItemInfo) e.NewValue;
            if(oldbar == newbar)
            {
                return;
            }
            if(!Utility.IsOSWindows7OrNewer)
            {
                return;
            }
            if(oldbar != null && oldbar._window != null)
            {
                oldbar._DetachWindow();
            }
            if(newbar != null)
            {
                newbar._SetWindow(window);
            }
        }

        private static object _CoerceTaskbarItemInfoValue(DependencyObject d, object value)
        {
            if(DesignerProperties.GetIsInDesignMode(d))
            {
                return value;
            }
            Verify.IsNotNull(d, "d");
            var w = (Window) d;
            var superbar = (TaskbarItemInfo) value;

            if(superbar != null)
            {
                if(superbar._window != null && superbar._window != w)
                {
                    throw new NotSupportedException();
                }
            }
            w.VerifyAccess();
            return superbar;
        }

        #endregion

        #region Dependency Properties and support methods.

        public static readonly DependencyProperty ProgressStateProperty = DependencyProperty.Register("ProgressState", typeof(TaskbarItemProgressState),
            typeof(TaskbarItemInfo),
            new PropertyMetadata(TaskbarItemProgressState.None, (d, e) => ((TaskbarItemInfo) d)._OnProgressStateChanged(),
                (d, e) => _CoerceProgressState((TaskbarItemProgressState) e)));

        public static readonly DependencyProperty ProgressValueProperty = DependencyProperty.Register("ProgressValue", typeof(double), typeof(TaskbarItemInfo),
            new PropertyMetadata(0d, (d, e) => ((TaskbarItemInfo) d)._OnProgressValueChanged(), (d, e) => _CoerceProgressValue((double) e)));

        public static readonly DependencyProperty OverlayProperty = DependencyProperty.Register("Overlay", typeof(ImageSource), typeof(TaskbarItemInfo),
            new PropertyMetadata(null, (d, e) => ((TaskbarItemInfo) d)._OnOverlayChanged()));

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(TaskbarItemInfo),
            new PropertyMetadata(string.Empty, (d, e) => ((TaskbarItemInfo) d)._OnDescriptionChanged()));

        public static readonly DependencyProperty ThumbnailClipMarginProperty = DependencyProperty.Register("ThumbnailClipMargin", typeof(Thickness),
            typeof(TaskbarItemInfo),
            new PropertyMetadata(default(Thickness), (d, e) => ((TaskbarItemInfo) d)._OnThumbnailClipMarginChanged(),
                (d, e) => _CoerceThumbnailClipMargin((Thickness) e)));

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifieMANAGERhouldBeSpelledCorrectly", MessageId = "Infos")]
        public static readonly DependencyProperty ThumbButtonInfosProperty = DependencyProperty.Register("ThumbButtonInfos", typeof(ThumbButtonInfoCollection),
            typeof(TaskbarItemInfo), new PropertyMetadata(null, (d, e) => ((TaskbarItemInfo) d)._OnThumbButtonsChanged()));

        public TaskbarItemProgressState ProgressState
        {
            get { return (TaskbarItemProgressState) GetValue(ProgressStateProperty); }
            set { SetValue(ProgressStateProperty, value); }
        }

        public double ProgressValue { get { return (double) GetValue(ProgressValueProperty); } set { SetValue(ProgressValueProperty, value); } }

        public ImageSource Overlay { get { return (ImageSource) GetValue(OverlayProperty); } set { SetValue(OverlayProperty, value); } }

        public string Description { get { return (string) GetValue(DescriptionProperty); } set { SetValue(DescriptionProperty, value); } }

        public Thickness ThumbnailClipMargin
        {
            get { return (Thickness) GetValue(ThumbnailClipMarginProperty); }
            set { SetValue(ThumbnailClipMarginProperty, value); }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifieMANAGERhouldBeSpelledCorrectly", MessageId = "Infos")]
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public ThumbButtonInfoCollection ThumbButtonInfos
        {
            get { return (ThumbButtonInfoCollection) GetValue(ThumbButtonInfosProperty); }
            set { SetValue(ThumbButtonInfosProperty, value); }
        }

        private void _OnProgressStateChanged()
        {
            if(!_isAttached)
            {
                return;
            }
            _UpdateProgressState(true);
        }

        private static TaskbarItemProgressState _CoerceProgressState(TaskbarItemProgressState value)
        {
            switch(value)
            {
                case TaskbarItemProgressState.Error:
                case TaskbarItemProgressState.Indeterminate:
                case TaskbarItemProgressState.None:
                case TaskbarItemProgressState.Normal:
                case TaskbarItemProgressState.Paused:
                    break;
                default:

                    value = TaskbarItemProgressState.None;
                    break;
            }
            return value;
        }

        private void _OnProgressValueChanged()
        {
            if(!_isAttached)
            {
                return;
            }
            _UpdateProgressValue(true);
        }

        private static double _CoerceProgressValue(double progressValue)
        {
            if(double.IsNaN(progressValue))
            {
                progressValue = 0;
            }
            progressValue = Math.Max(progressValue, 0);
            progressValue = Math.Min(1, progressValue);
            return progressValue;
        }

        private void _OnOverlayChanged()
        {
            if(!_isAttached)
            {
                return;
            }
            _UpdateOverlay(true);
        }

        private void _OnDescriptionChanged()
        {
            if(!_isAttached)
            {
                return;
            }
            _UpdateTooltip(true);
        }

        private void _OnThumbnailClipMarginChanged()
        {
            if(!_isAttached)
            {
                return;
            }
            _UpdateThumbnailClipping(true);
        }

        private static Thickness _CoerceThumbnailClipMargin(Thickness margin)
        {
            if(margin.Left < 0 || margin.Right < 0 || margin.Top < 0 || margin.Bottom < 0)
            {
                return _EmptyThickness;
            }
            return margin;
        }

        private void _OnThumbButtonsChanged()
        {
            if(!_isAttached)
            {
                return;
            }
            _UpdateThumbButtons(true);
        }

        #endregion
    }
}