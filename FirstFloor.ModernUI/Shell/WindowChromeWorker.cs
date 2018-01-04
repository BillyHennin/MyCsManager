// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

using FirstFloor.ModernUI.Shell.Standard;

namespace FirstFloor.ModernUI.Shell
{
    internal class WindowChromeWorker : DependencyObject
    {
        private static readonly Version _presentationFrameworkVersion = Assembly.GetAssembly(typeof(Window)).GetName().Version;
        public static readonly DependencyProperty WindowChromeWorkerProperty = DependencyProperty.RegisterAttached("WindowChromeWorker",
            typeof(WindowChromeWorker), typeof(WindowChromeWorker), new PropertyMetadata(null, _OnChromeWorkerChanged));

        [SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
        private static readonly HT[,] _HitTestBorders =
        {
            {HT.TOPLEFT, HT.TOP, HT.TOPRIGHT}, {HT.LEFT, HT.CLIENT, HT.RIGHT},
            {HT.BOTTOMLEFT, HT.BOTTOM, HT.BOTTOMRIGHT}
        };

        public WindowChromeWorker()
        {
            _messageTable = new List<KeyValuePair<WM, MessageHandler>>
            {
                new KeyValuePair<WM, MessageHandler>(WM.SETTEXT, _HandleSetTextOrIcon),
                new KeyValuePair<WM, MessageHandler>(WM.SETICON, _HandleSetTextOrIcon),
                new KeyValuePair<WM, MessageHandler>(WM.NCACTIVATE, _HandleNCActivate),
                new KeyValuePair<WM, MessageHandler>(WM.NCCALCSIZE, _HandleNCCalcSize),
                new KeyValuePair<WM, MessageHandler>(WM.NCHITTEST, _HandleNCHitTest),
                new KeyValuePair<WM, MessageHandler>(WM.NCRBUTTONUP, _HandleNCRButtonUp),
                new KeyValuePair<WM, MessageHandler>(WM.SIZE, _HandleSize),
                new KeyValuePair<WM, MessageHandler>(WM.WINDOWPOSCHANGED, _HandleWindowPosChanged),
                new KeyValuePair<WM, MessageHandler>(WM.DWMCOMPOSITIONCHANGED, _HandleDwmCompositionChanged),
            };
            if(_IsPresentationFrameworkVersionLessThan4)
            {
                _messageTable.AddRange(new[]
                {
                    new KeyValuePair<WM, MessageHandler>(WM.SETTINGCHANGE, _HandleSettingChange),
                    new KeyValuePair<WM, MessageHandler>(WM.ENTERSIZEMOVE, _HandleEnterSizeMove),
                    new KeyValuePair<WM, MessageHandler>(WM.EXITSIZEMOVE, _HandleExitSizeMove), new KeyValuePair<WM, MessageHandler>(WM.MOVE, _HandleMove)
                });
            }
        }

        private static bool _IsPresentationFrameworkVersionLessThan4 { get { return _presentationFrameworkVersion < new Version(4, 0); } }

        private bool _IsWindowDocked
        {
            get
            {
                Assert.IsTrue(_IsPresentationFrameworkVersionLessThan4);
                if(_window.WindowState != WindowState.Normal)
                {
                    return false;
                }
                var adjustedOffset = _GetAdjustedWindowRect(new RECT {Bottom = 100, Right = 100});
                var windowTopLeft = new Point(_window.Left, _window.Top);
                windowTopLeft -= (Vector) DpiHelper.DevicePixelsToLogical(new Point(adjustedOffset.Left, adjustedOffset.Top));
                return _window.RestoreBounds.Location != windowTopLeft;
            }
        }

        #region WindowProc and Message Handlers

        private IntPtr _WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            Assert.AreEqual(hwnd, _hwnd);
            var message = (WM) msg;
            foreach(var handlePair in _messageTable)
            {
                if(handlePair.Key == message)
                {
                    return handlePair.Value(message, wParam, lParam, out handled);
                }
            }
            return IntPtr.Zero;
        }

        private IntPtr _HandleSetTextOrIcon(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            var modified = _ModifyStyle(WS.VISIBLE, 0);

            var lRet = NativeMethods.DefWindowProc(_hwnd, uMsg, wParam, lParam);

            if(modified)
            {
                _ModifyStyle(0, WS.VISIBLE);
            }
            handled = true;
            return lRet;
        }

        private IntPtr _HandleNCActivate(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            var lRet = NativeMethods.DefWindowProc(_hwnd, WM.NCACTIVATE, wParam, new IntPtr(-1));
            handled = true;
            return lRet;
        }

        private IntPtr _HandleNCCalcSize(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            if(_chromeInfo.SacrificialEdge != SacrificialEdge.None)
            {
                var windowResizeBorderThicknessDevice = DpiHelper.LogicalThicknessToDevice(SystemParameters2.Current.WindowResizeBorderThickness);
                var rcClientArea = (RECT) Marshal.PtrToStructure(lParam, typeof(RECT));
                if(Utility.IsFlagSet((int) _chromeInfo.SacrificialEdge, (int) SacrificialEdge.Top))
                {
                    rcClientArea.Top += (int) windowResizeBorderThicknessDevice.Top;
                }
                if(Utility.IsFlagSet((int) _chromeInfo.SacrificialEdge, (int) SacrificialEdge.Left))
                {
                    rcClientArea.Left += (int) windowResizeBorderThicknessDevice.Left;
                }
                if(Utility.IsFlagSet((int) _chromeInfo.SacrificialEdge, (int) SacrificialEdge.Bottom))
                {
                    rcClientArea.Bottom -= (int) windowResizeBorderThicknessDevice.Bottom;
                }
                if(Utility.IsFlagSet((int) _chromeInfo.SacrificialEdge, (int) SacrificialEdge.Right))
                {
                    rcClientArea.Right -= (int) windowResizeBorderThicknessDevice.Right;
                }
                Marshal.StructureToPtr(rcClientArea, lParam, false);
            }
            handled = true;
            return new IntPtr((int) WVR.REDRAW);
        }

        private HT _GetHTFromResizeGripDirection(ResizeGripDirection direction)
        {
            var compliment = _window.FlowDirection == FlowDirection.RightToLeft;
            switch(direction)
            {
                case ResizeGripDirection.Bottom:
                    return HT.BOTTOM;
                case ResizeGripDirection.BottomLeft:
                    return compliment ? HT.BOTTOMRIGHT : HT.BOTTOMLEFT;
                case ResizeGripDirection.BottomRight:
                    return compliment ? HT.BOTTOMLEFT : HT.BOTTOMRIGHT;
                case ResizeGripDirection.Left:
                    return compliment ? HT.RIGHT : HT.LEFT;
                case ResizeGripDirection.Right:
                    return compliment ? HT.LEFT : HT.RIGHT;
                case ResizeGripDirection.Top:
                    return HT.TOP;
                case ResizeGripDirection.TopLeft:
                    return compliment ? HT.TOPRIGHT : HT.TOPLEFT;
                case ResizeGripDirection.TopRight:
                    return compliment ? HT.TOPLEFT : HT.TOPRIGHT;
                case ResizeGripDirection.Caption:
                    return HT.CAPTION;
                default:
                    return HT.NOWHERE;
            }
        }

        private IntPtr _HandleNCHitTest(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            var mousePosScreen = new Point(Utility.GET_X_LPARAM(lParam), Utility.GET_Y_LPARAM(lParam));
            var windowPosition = _GetWindowRect();
            var mousePosWindow = mousePosScreen;
            mousePosWindow.Offset(-windowPosition.X, -windowPosition.Y);
            mousePosWindow = DpiHelper.DevicePixelsToLogical(mousePosWindow);

            var inputElement = _window.InputHitTest(mousePosWindow);
            if(inputElement != null)
            {
                if(WindowChrome.GetIsHitTestVisibleInChrome(inputElement))
                {
                    handled = true;
                    return new IntPtr((int) HT.CLIENT);
                }
                var direction = WindowChrome.GetResizeGripDirection(inputElement);
                if(direction != ResizeGripDirection.None)
                {
                    handled = true;
                    return new IntPtr((int) _GetHTFromResizeGripDirection(direction));
                }
            }

            if(_chromeInfo.UseAeroCaptionButtons)
            {
                if(Utility.IsOSVistaOrNewer && _chromeInfo.GlassFrameThickness != default(Thickness) && _isGlassEnabled)
                {
                    IntPtr lRet;
                    handled = NativeMethods.DwmDefWindowProc(_hwnd, uMsg, wParam, lParam, out lRet);
                    if(IntPtr.Zero != lRet)
                    {
                        return lRet;
                    }
                }
            }
            var ht = _HitTestNca(DpiHelper.DeviceRectToLogical(windowPosition), DpiHelper.DevicePixelsToLogical(mousePosScreen));
            handled = true;
            return new IntPtr((int) ht);
        }

        private IntPtr _HandleNCRButtonUp(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            if(HT.CAPTION == (HT) wParam.ToInt32())
            {
                SystemCommands.ShowSystemMenuPhysicalCoordinates(_window, new Point(Utility.GET_X_LPARAM(lParam), Utility.GET_Y_LPARAM(lParam)));
            }
            handled = false;
            return IntPtr.Zero;
        }

        private IntPtr _HandleSize(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            const int SIZE_MAXIMIZED = 2;

            WindowState? state = null;
            if(wParam.ToInt32() == SIZE_MAXIMIZED)
            {
                state = WindowState.Maximized;
            }
            _UpdateSystemMenu(state);

            handled = false;
            return IntPtr.Zero;
        }

        private IntPtr _HandleWindowPosChanged(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            // http://blogs.msdn.com/oldnewthing/archive/2008/01/15/7113860.aspx

            _UpdateSystemMenu(null);
            if(!_isGlassEnabled)
            {
                Assert.IsNotDefault(lParam);
                var wp = (WINDOWPOS) Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));
                _SetRoundingRegion(wp);
            }

            handled = false;
            return IntPtr.Zero;
        }

        private IntPtr _HandleDwmCompositionChanged(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            _UpdateFrameState(false);
            handled = false;
            return IntPtr.Zero;
        }

        private IntPtr _HandleSettingChange(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            Assert.IsTrue(_IsPresentationFrameworkVersionLessThan4);
            _FixupTemplateIssues();
            handled = false;
            return IntPtr.Zero;
        }

        private IntPtr _HandleEnterSizeMove(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            Assert.IsTrue(_IsPresentationFrameworkVersionLessThan4);
            _isUserResizing = true;

            Assert.Implies(_window.WindowState == WindowState.Maximized, Utility.IsOSWindows7OrNewer);
            if(_window.WindowState != WindowState.Maximized)
            {
                if(!_IsWindowDocked)
                {
                    _windowPosAtStartOfUserMove = new Point(_window.Left, _window.Top);
                }
            }
            handled = false;
            return IntPtr.Zero;
        }

        private IntPtr _HandleExitSizeMove(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            Assert.IsTrue(_IsPresentationFrameworkVersionLessThan4);
            _isUserResizing = false;

            if(_window.WindowState == WindowState.Maximized)
            {
                Assert.IsTrue(Utility.IsOSWindows7OrNewer);
                _window.Top = _windowPosAtStartOfUserMove.Y;
                _window.Left = _windowPosAtStartOfUserMove.X;
            }
            handled = false;
            return IntPtr.Zero;
        }

        private IntPtr _HandleMove(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            Assert.IsTrue(_IsPresentationFrameworkVersionLessThan4);
            if(_isUserResizing)
            {
                _hasUserMovedWindow = true;
            }
            handled = false;
            return IntPtr.Zero;
        }

        #endregion

        public void SetWindowChrome(WindowChrome newChrome)
        {
            VerifyAccess();
            Assert.IsNotNull(_window);
            if(newChrome == _chromeInfo)
            {
                return;
            }
            if(_chromeInfo != null)
            {
                _chromeInfo.PropertyChangedThatRequiresRepaint -= _OnChromePropertyChangedThatRequiresRepaint;
            }
            _chromeInfo = newChrome;
            if(_chromeInfo != null)
            {
                _chromeInfo.PropertyChangedThatRequiresRepaint += _OnChromePropertyChangedThatRequiresRepaint;
            }
            _ApplyNewCustomChrome();
        }

        private void _OnChromePropertyChangedThatRequiresRepaint(object sender, EventArgs e)
        {
            _UpdateFrameState(true);
        }

        private static void _OnChromeWorkerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var w = (Window) d;
            var cw = (WindowChromeWorker) e.NewValue;

            Assert.IsNotNull(w);
            Assert.IsNotNull(cw);
            Assert.IsNull(cw._window);
            cw._SetWindow(w);
        }

        private void _SetWindow(Window window)
        {
            Assert.IsNull(_window);
            Assert.IsNotNull(window);
            _window = window;

            _hwnd = new WindowInteropHelper(_window).Handle;

            Utility.AddDependencyPropertyChangeListener(_window, Control.TemplateProperty, _OnWindowPropertyChangedThatRequiresTemplateFixup);
            Utility.AddDependencyPropertyChangeListener(_window, FrameworkElement.FlowDirectionProperty, _OnWindowPropertyChangedThatRequiresTemplateFixup);
            _window.Closed += _UnsetWindow;

            if(IntPtr.Zero != _hwnd)
            {
                _hwndSource = HwndSource.FromHwnd(_hwnd);
                Assert.IsNotNull(_hwndSource);
                _window.ApplyTemplate();
                if(_chromeInfo != null)
                {
                    _ApplyNewCustomChrome();
                }
            }
            else
            {
                _window.SourceInitialized += (sender, e) =>
                {
                    _hwnd = new WindowInteropHelper(_window).Handle;
                    Assert.IsNotDefault(_hwnd);
                    _hwndSource = HwndSource.FromHwnd(_hwnd);
                    Assert.IsNotNull(_hwndSource);
                    if(_chromeInfo != null)
                    {
                        _ApplyNewCustomChrome();
                    }
                };
            }
        }

        private void _UnsetWindow(object sender, EventArgs e)
        {
            Utility.RemoveDependencyPropertyChangeListener(_window, Control.TemplateProperty, _OnWindowPropertyChangedThatRequiresTemplateFixup);
            Utility.RemoveDependencyPropertyChangeListener(_window, FrameworkElement.FlowDirectionProperty, _OnWindowPropertyChangedThatRequiresTemplateFixup);
            if(_chromeInfo != null)
            {
                _chromeInfo.PropertyChangedThatRequiresRepaint -= _OnChromePropertyChangedThatRequiresRepaint;
            }
            _RestoreStandardChromeState(true);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static WindowChromeWorker GetWindowChromeWorker(Window window)
        {
            Verify.IsNotNull(window, "window");
            return (WindowChromeWorker) window.GetValue(WindowChromeWorkerProperty);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static void SetWindowChromeWorker(Window window, WindowChromeWorker chrome)
        {
            Verify.IsNotNull(window, "window");
            window.SetValue(WindowChromeWorkerProperty, chrome);
        }

        private void _OnWindowPropertyChangedThatRequiresTemplateFixup(object sender, EventArgs e)
        {
            if(_chromeInfo != null && _hwnd != IntPtr.Zero)
            {
                _window.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (_Action) _FixupTemplateIssues);
            }
        }

        private void _ApplyNewCustomChrome()
        {
            if(_hwnd == IntPtr.Zero)
            {
                return;
            }
            if(_chromeInfo == null)
            {
                _RestoreStandardChromeState(false);
                return;
            }
            if(!_isHooked)
            {
                _hwndSource.AddHook(_WndProc);
                _isHooked = true;
            }
            _FixupTemplateIssues();

            _UpdateSystemMenu(_window.WindowState);
            _UpdateFrameState(true);
            NativeMethods.SetWindowPos(_hwnd, IntPtr.Zero, 0, 0, 0, 0, _SwpFlags);
        }

        private void _FixupTemplateIssues()
        {
            Assert.IsNotNull(_chromeInfo);
            Assert.IsNotNull(_window);
            if(_window.Template == null)
            {
                return;
            }

            if(VisualTreeHelper.GetChildrenCount(_window) == 0)
            {
                _window.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (_Action) _FixupTemplateIssues);
                return;
            }
            var templateFixupMargin = default(Thickness);
            Transform templateFixupTransform = null;
            if(_IsPresentationFrameworkVersionLessThan4)
            {
                var rcWindow = NativeMethods.GetWindowRect(_hwnd);
                var rcAdjustedClient = _GetAdjustedWindowRect(rcWindow);
                var rcLogicalWindow = DpiHelper.DeviceRectToLogical(new Rect(rcWindow.Left, rcWindow.Top, rcWindow.Width, rcWindow.Height));
                var rcLogicalClient =
                    DpiHelper.DeviceRectToLogical(new Rect(rcAdjustedClient.Left, rcAdjustedClient.Top, rcAdjustedClient.Width, rcAdjustedClient.Height));
                var nonClientThickness = new Thickness(rcLogicalWindow.Left - rcLogicalClient.Left, rcLogicalWindow.Top - rcLogicalClient.Top,
                    rcLogicalClient.Right - rcLogicalWindow.Right, rcLogicalClient.Bottom - rcLogicalWindow.Bottom);
                templateFixupMargin = new Thickness(0, 0, -(nonClientThickness.Left + nonClientThickness.Right),
                    -(nonClientThickness.Top + nonClientThickness.Bottom));

                templateFixupTransform = _window.FlowDirection == FlowDirection.RightToLeft
                    ? new MatrixTransform(1, 0, 0, 1, -(nonClientThickness.Left + nonClientThickness.Right), 0) : null;
            }
            if(_chromeInfo.SacrificialEdge != SacrificialEdge.None)
            {
                if(Utility.IsFlagSet((int) _chromeInfo.SacrificialEdge, (int) SacrificialEdge.Top))
                {
                    templateFixupMargin.Top -= SystemParameters2.Current.WindowResizeBorderThickness.Top;
                }
                if(Utility.IsFlagSet((int) _chromeInfo.SacrificialEdge, (int) SacrificialEdge.Left))
                {
                    templateFixupMargin.Left -= SystemParameters2.Current.WindowResizeBorderThickness.Left;
                }
                if(Utility.IsFlagSet((int) _chromeInfo.SacrificialEdge, (int) SacrificialEdge.Bottom))
                {
                    templateFixupMargin.Bottom -= SystemParameters2.Current.WindowResizeBorderThickness.Bottom;
                }
                if(Utility.IsFlagSet((int) _chromeInfo.SacrificialEdge, (int) SacrificialEdge.Right))
                {
                    templateFixupMargin.Right -= SystemParameters2.Current.WindowResizeBorderThickness.Right;
                }
            }
            var rootElement = (FrameworkElement) VisualTreeHelper.GetChild(_window, 0);
            rootElement.Margin = templateFixupMargin;
            rootElement.RenderTransform = templateFixupTransform;
            if(_IsPresentationFrameworkVersionLessThan4)
            {
                if(!_isFixedUp)
                {
                    _hasUserMovedWindow = false;
                    _window.StateChanged += _FixupRestoreBounds;
                    _isFixedUp = true;
                }
            }
        }

        private void _FixupRestoreBounds(object sender, EventArgs e)
        {
            Assert.IsTrue(_IsPresentationFrameworkVersionLessThan4);
            if(_window.WindowState == WindowState.Maximized || _window.WindowState == WindowState.Minimized)
            {
                if(_hasUserMovedWindow)
                {
                    _hasUserMovedWindow = false;
                    var wp = NativeMethods.GetWindowPlacement(_hwnd);
                    var adjustedDeviceRc = _GetAdjustedWindowRect(new RECT {Bottom = 100, Right = 100});
                    var adjustedTopLeft =
                        DpiHelper.DevicePixelsToLogical(new Point(wp.rcNormalPosition.Left - adjustedDeviceRc.Left,
                            wp.rcNormalPosition.Top - adjustedDeviceRc.Top));
                    _window.Top = adjustedTopLeft.Y;
                    _window.Left = adjustedTopLeft.X;
                }
            }
        }

        private RECT _GetAdjustedWindowRect(RECT rcWindow)
        {
            Assert.IsTrue(_IsPresentationFrameworkVersionLessThan4);
            var style = (WS) NativeMethods.GetWindowLongPtr(_hwnd, GWL.STYLE);
            var exstyle = (WS_EX) NativeMethods.GetWindowLongPtr(_hwnd, GWL.EXSTYLE);
            return NativeMethods.AdjustWindowRectEx(rcWindow, style, false, exstyle);
        }

        private bool _ModifyStyle(WS removeStyle, WS addStyle)
        {
            Assert.IsNotDefault(_hwnd);
            var dwStyle = (WS) NativeMethods.GetWindowLongPtr(_hwnd, GWL.STYLE).ToInt32();
            var dwNewStyle = (dwStyle & ~removeStyle) | addStyle;
            if(dwStyle == dwNewStyle)
            {
                return false;
            }
            NativeMethods.SetWindowLongPtr(_hwnd, GWL.STYLE, new IntPtr((int) dwNewStyle));
            return true;
        }

        private WindowState _GetHwndState()
        {
            var wpl = NativeMethods.GetWindowPlacement(_hwnd);
            switch(wpl.showCmd)
            {
                case SW.SHOWMINIMIZED:
                    return WindowState.Minimized;
                case SW.SHOWMAXIMIZED:
                    return WindowState.Maximized;
            }
            return WindowState.Normal;
        }

        private Rect _GetWindowRect()
        {
            var windowPosition = NativeMethods.GetWindowRect(_hwnd);
            return new Rect(windowPosition.Left, windowPosition.Top, windowPosition.Width, windowPosition.Height);
        }

        private void _UpdateSystemMenu(WindowState? assumeState)
        {
            const MF mfEnabled = MF.ENABLED | MF.BYCOMMAND;
            const MF mfDisabled = MF.GRAYED | MF.DISABLED | MF.BYCOMMAND;
            var state = assumeState ?? _GetHwndState();
            if(null != assumeState || _lastMenuState != state)
            {
                _lastMenuState = state;
                var modified = _ModifyStyle(WS.VISIBLE, 0);
                var hmenu = NativeMethods.GetSystemMenu(_hwnd, false);
                if(IntPtr.Zero != hmenu)
                {
                    var dwStyle = (WS) NativeMethods.GetWindowLongPtr(_hwnd, GWL.STYLE).ToInt32();
                    var canMinimize = Utility.IsFlagSet((int) dwStyle, (int) WS.MINIMIZEBOX);
                    var canMaximize = Utility.IsFlagSet((int) dwStyle, (int) WS.MAXIMIZEBOX);
                    var canSize = Utility.IsFlagSet((int) dwStyle, (int) WS.THICKFRAME);
                    switch(state)
                    {
                        case WindowState.Maximized:
                            NativeMethods.EnableMenuItem(hmenu, SC.RESTORE, mfEnabled);
                            NativeMethods.EnableMenuItem(hmenu, SC.MOVE, mfDisabled);
                            NativeMethods.EnableMenuItem(hmenu, SC.SIZE, mfDisabled);
                            NativeMethods.EnableMenuItem(hmenu, SC.MINIMIZE, canMinimize ? mfEnabled : mfDisabled);
                            NativeMethods.EnableMenuItem(hmenu, SC.MAXIMIZE, mfDisabled);
                            break;
                        case WindowState.Minimized:
                            NativeMethods.EnableMenuItem(hmenu, SC.RESTORE, mfEnabled);
                            NativeMethods.EnableMenuItem(hmenu, SC.MOVE, mfDisabled);
                            NativeMethods.EnableMenuItem(hmenu, SC.SIZE, mfDisabled);
                            NativeMethods.EnableMenuItem(hmenu, SC.MINIMIZE, mfDisabled);
                            NativeMethods.EnableMenuItem(hmenu, SC.MAXIMIZE, canMaximize ? mfEnabled : mfDisabled);
                            break;
                        default:
                            NativeMethods.EnableMenuItem(hmenu, SC.RESTORE, mfDisabled);
                            NativeMethods.EnableMenuItem(hmenu, SC.MOVE, mfEnabled);
                            NativeMethods.EnableMenuItem(hmenu, SC.SIZE, canSize ? mfEnabled : mfDisabled);
                            NativeMethods.EnableMenuItem(hmenu, SC.MINIMIZE, canMinimize ? mfEnabled : mfDisabled);
                            NativeMethods.EnableMenuItem(hmenu, SC.MAXIMIZE, canMaximize ? mfEnabled : mfDisabled);
                            break;
                    }
                }
                if(modified)
                {
                    _ModifyStyle(0, WS.VISIBLE);
                }
            }
        }

        private void _UpdateFrameState(bool force)
        {
            if(IntPtr.Zero == _hwnd)
            {
                return;
            }

            var frameState = NativeMethods.DwmIsCompositionEnabled();
            if(force || frameState != _isGlassEnabled)
            {
                _isGlassEnabled = frameState && _chromeInfo.GlassFrameThickness != default(Thickness);
                if(!_isGlassEnabled)
                {
                    _SetRoundingRegion(null);
                }
                else
                {
                    _ClearRoundingRegion();
                    _ExtendGlassFrame();
                }
                NativeMethods.SetWindowPos(_hwnd, IntPtr.Zero, 0, 0, 0, 0, _SwpFlags);
            }
        }

        private void _ClearRoundingRegion()
        {
            NativeMethods.SetWindowRgn(_hwnd, IntPtr.Zero, NativeMethods.IsWindowVisible(_hwnd));
        }

        private void _SetRoundingRegion(WINDOWPOS? wp)
        {
            const int MONITOR_DEFAULTTONEAREST = 0x00000002;

            var wpl = NativeMethods.GetWindowPlacement(_hwnd);
            if(wpl.showCmd == SW.SHOWMAXIMIZED)
            {
                int left;
                int top;
                if(wp.HasValue)
                {
                    left = wp.Value.x;
                    top = wp.Value.y;
                }
                else
                {
                    var r = _GetWindowRect();
                    left = (int) r.Left;
                    top = (int) r.Top;
                }
                var hMon = NativeMethods.MonitorFromWindow(_hwnd, MONITOR_DEFAULTTONEAREST);
                var mi = NativeMethods.GetMonitorInfo(hMon);
                var rcMax = mi.rcWork;

                rcMax.Offset(-left, -top);
                var hrgn = IntPtr.Zero;
                try
                {
                    hrgn = NativeMethods.CreateRectRgnIndirect(rcMax);
                    NativeMethods.SetWindowRgn(_hwnd, hrgn, NativeMethods.IsWindowVisible(_hwnd));
                    hrgn = IntPtr.Zero;
                }
                finally
                {
                    Utility.SafeDeleteObject(ref hrgn);
                }
            }
            else
            {
                Size windowSize;

                if(null != wp && !Utility.IsFlagSet(wp.Value.flags, (int) SWP.NOSIZE))
                {
                    windowSize = new Size(wp.Value.cx, wp.Value.cy);
                }
                else if(null != wp && (_lastRoundingState == _window.WindowState))
                {
                    return;
                }
                else
                {
                    windowSize = _GetWindowRect().Size;
                }
                _lastRoundingState = _window.WindowState;
                var hrgn = IntPtr.Zero;
                try
                {
                    var shortestDimension = Math.Min(windowSize.Width, windowSize.Height);
                    var topLeftRadius = DpiHelper.LogicalPixelsToDevice(new Point(_chromeInfo.CornerRadius.TopLeft, 0)).X;
                    topLeftRadius = Math.Min(topLeftRadius, shortestDimension / 2);
                    if(_IsUniform(_chromeInfo.CornerRadius))
                    {
                        hrgn = _CreateRoundRectRgn(new Rect(windowSize), topLeftRadius);
                    }
                    else
                    {
                        hrgn = _CreateRoundRectRgn(new Rect(0, 0, windowSize.Width / 2 + topLeftRadius, windowSize.Height / 2 + topLeftRadius), topLeftRadius);
                        var topRightRadius = DpiHelper.LogicalPixelsToDevice(new Point(_chromeInfo.CornerRadius.TopRight, 0)).X;
                        topRightRadius = Math.Min(topRightRadius, shortestDimension / 2);
                        var topRightRegionRect = new Rect(0, 0, windowSize.Width / 2 + topRightRadius, windowSize.Height / 2 + topRightRadius);
                        topRightRegionRect.Offset(windowSize.Width / 2 - topRightRadius, 0);
                        Assert.AreEqual(topRightRegionRect.Right, windowSize.Width);
                        _CreateAndCombineRoundRectRgn(hrgn, topRightRegionRect, topRightRadius);
                        var bottomLeftRadius = DpiHelper.LogicalPixelsToDevice(new Point(_chromeInfo.CornerRadius.BottomLeft, 0)).X;
                        bottomLeftRadius = Math.Min(bottomLeftRadius, shortestDimension / 2);
                        var bottomLeftRegionRect = new Rect(0, 0, windowSize.Width / 2 + bottomLeftRadius, windowSize.Height / 2 + bottomLeftRadius);
                        bottomLeftRegionRect.Offset(0, windowSize.Height / 2 - bottomLeftRadius);
                        Assert.AreEqual(bottomLeftRegionRect.Bottom, windowSize.Height);
                        _CreateAndCombineRoundRectRgn(hrgn, bottomLeftRegionRect, bottomLeftRadius);
                        var bottomRightRadius = DpiHelper.LogicalPixelsToDevice(new Point(_chromeInfo.CornerRadius.BottomRight, 0)).X;
                        bottomRightRadius = Math.Min(bottomRightRadius, shortestDimension / 2);
                        var bottomRightRegionRect = new Rect(0, 0, windowSize.Width / 2 + bottomRightRadius, windowSize.Height / 2 + bottomRightRadius);
                        bottomRightRegionRect.Offset(windowSize.Width / 2 - bottomRightRadius, windowSize.Height / 2 - bottomRightRadius);
                        Assert.AreEqual(bottomRightRegionRect.Right, windowSize.Width);
                        Assert.AreEqual(bottomRightRegionRect.Bottom, windowSize.Height);
                        _CreateAndCombineRoundRectRgn(hrgn, bottomRightRegionRect, bottomRightRadius);
                    }
                    NativeMethods.SetWindowRgn(_hwnd, hrgn, NativeMethods.IsWindowVisible(_hwnd));
                    hrgn = IntPtr.Zero;
                }
                finally
                {
                    Utility.SafeDeleteObject(ref hrgn);
                }
            }
        }

        private static IntPtr _CreateRoundRectRgn(Rect region, double radius)
        {
            if(DoubleUtilities.AreClose(0, radius))
            {
                return NativeMethods.CreateRectRgn((int) Math.Floor(region.Left), (int) Math.Floor(region.Top), (int) Math.Ceiling(region.Right),
                    (int) Math.Ceiling(region.Bottom));
            }

            return NativeMethods.CreateRoundRectRgn((int) Math.Floor(region.Left), (int) Math.Floor(region.Top), (int) Math.Ceiling(region.Right) + 1,
                (int) Math.Ceiling(region.Bottom) + 1, (int) Math.Ceiling(radius), (int) Math.Ceiling(radius));
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "HRGNs")]
        private static void _CreateAndCombineRoundRectRgn(IntPtr hrgnSource, Rect region, double radius)
        {
            var hrgn = IntPtr.Zero;
            try
            {
                hrgn = _CreateRoundRectRgn(region, radius);
                var result = NativeMethods.CombineRgn(hrgnSource, hrgnSource, hrgn, RGN.OR);
                if(result == CombineRgnResult.ERROR)
                {
                    throw new InvalidOperationException("Unable to combine two HRGNs.");
                }
            }
            finally
            {
                Utility.SafeDeleteObject(ref hrgn);
            }
        }

        private static bool _IsUniform(CornerRadius cornerRadius)
        {
            if(!DoubleUtilities.AreClose(cornerRadius.BottomLeft, cornerRadius.BottomRight))
            {
                return false;
            }
            if(!DoubleUtilities.AreClose(cornerRadius.TopLeft, cornerRadius.TopRight))
            {
                return false;
            }
            if(!DoubleUtilities.AreClose(cornerRadius.BottomLeft, cornerRadius.TopRight))
            {
                return false;
            }
            return true;
        }

        private void _ExtendGlassFrame()
        {
            Assert.IsNotNull(_window);

            if(!Utility.IsOSVistaOrNewer)
            {
                return;
            }
            if(IntPtr.Zero == _hwnd)
            {
                return;
            }

            if(!NativeMethods.DwmIsCompositionEnabled())
            {
                _hwndSource.CompositionTarget.BackgroundColor = SystemColors.WindowColor;
            }
            else
            {
                _hwndSource.CompositionTarget.BackgroundColor = Colors.Transparent;

                var deviceGlassThickness = DpiHelper.LogicalThicknessToDevice(_chromeInfo.GlassFrameThickness);
                if(_chromeInfo.SacrificialEdge != SacrificialEdge.None)
                {
                    var windowResizeBorderThicknessDevice = DpiHelper.LogicalThicknessToDevice(SystemParameters2.Current.WindowResizeBorderThickness);
                    if(Utility.IsFlagSet((int) _chromeInfo.SacrificialEdge, (int) SacrificialEdge.Top))
                    {
                        deviceGlassThickness.Top -= windowResizeBorderThicknessDevice.Top;
                        deviceGlassThickness.Top = Math.Max(0, deviceGlassThickness.Top);
                    }
                    if(Utility.IsFlagSet((int) _chromeInfo.SacrificialEdge, (int) SacrificialEdge.Left))
                    {
                        deviceGlassThickness.Left -= windowResizeBorderThicknessDevice.Left;
                        deviceGlassThickness.Left = Math.Max(0, deviceGlassThickness.Left);
                    }
                    if(Utility.IsFlagSet((int) _chromeInfo.SacrificialEdge, (int) SacrificialEdge.Bottom))
                    {
                        deviceGlassThickness.Bottom -= windowResizeBorderThicknessDevice.Bottom;
                        deviceGlassThickness.Bottom = Math.Max(0, deviceGlassThickness.Bottom);
                    }
                    if(Utility.IsFlagSet((int) _chromeInfo.SacrificialEdge, (int) SacrificialEdge.Right))
                    {
                        deviceGlassThickness.Right -= windowResizeBorderThicknessDevice.Right;
                        deviceGlassThickness.Right = Math.Max(0, deviceGlassThickness.Right);
                    }
                }
                var dwmMargin = new MARGINS
                {
                    cxLeftWidth = (int) Math.Ceiling(deviceGlassThickness.Left),
                    cxRightWidth = (int) Math.Ceiling(deviceGlassThickness.Right),
                    cyTopHeight = (int) Math.Ceiling(deviceGlassThickness.Top),
                    cyBottomHeight = (int) Math.Ceiling(deviceGlassThickness.Bottom),
                };
                NativeMethods.DwmExtendFrameIntoClientArea(_hwnd, ref dwmMargin);
            }
        }

        private HT _HitTestNca(Rect windowPosition, Point mousePosition)
        {
            var uRow = 1;
            var uCol = 1;
            var onResizeBorder = false;

            if(mousePosition.Y >= windowPosition.Top && mousePosition.Y < windowPosition.Top + _chromeInfo.ResizeBorderThickness.Top + _chromeInfo.CaptionHeight)
            {
                onResizeBorder = (mousePosition.Y < (windowPosition.Top + _chromeInfo.ResizeBorderThickness.Top));
                uRow = 0;
            }
            else if(mousePosition.Y < windowPosition.Bottom && mousePosition.Y >= windowPosition.Bottom - (int) _chromeInfo.ResizeBorderThickness.Bottom)
            {
                uRow = 2;
            }

            if(mousePosition.X >= windowPosition.Left && mousePosition.X < windowPosition.Left + (int) _chromeInfo.ResizeBorderThickness.Left)
            {
                uCol = 0;
            }
            else if(mousePosition.X < windowPosition.Right && mousePosition.X >= windowPosition.Right - _chromeInfo.ResizeBorderThickness.Right)
            {
                uCol = 2;
            }

            if(uRow == 0 && uCol != 1 && !onResizeBorder)
            {
                uRow = 1;
            }
            var ht = _HitTestBorders[uRow, uCol];
            if(ht == HT.TOP && !onResizeBorder)
            {
                ht = HT.CAPTION;
            }
            return ht;
        }

        #region Remove Custom Chrome Methods

        private void _RestoreStandardChromeState(bool isClosing)
        {
            VerifyAccess();
            _UnhookCustomChrome();
            if(!isClosing)
            {
                _RestoreTemplateFixups();
                _RestoreGlassFrame();
                _RestoreHrgn();
                _window.InvalidateMeasure();
            }
        }

        private void _UnhookCustomChrome()
        {
            Assert.IsNotDefault(_hwnd);
            Assert.IsNotNull(_window);
            if(_isHooked)
            {
                _hwndSource.RemoveHook(_WndProc);
                _isHooked = false;
            }
        }

        private void _RestoreTemplateFixups()
        {
            Assert.Implies(_IsPresentationFrameworkVersionLessThan4, () => _isFixedUp);
            var rootElement = (FrameworkElement) VisualTreeHelper.GetChild(_window, 0);

            rootElement.Margin = new Thickness();
            _window.StateChanged -= _FixupRestoreBounds;
            _isFixedUp = false;
        }

        private void _RestoreGlassFrame()
        {
            Assert.IsNull(_chromeInfo);
            Assert.IsNotNull(_window);

            if(!Utility.IsOSVistaOrNewer || _hwnd == IntPtr.Zero)
            {
                return;
            }
            _hwndSource.CompositionTarget.BackgroundColor = SystemColors.WindowColor;
            if(NativeMethods.DwmIsCompositionEnabled())
            {
                var dwmMargin = new MARGINS();
                NativeMethods.DwmExtendFrameIntoClientArea(_hwnd, ref dwmMargin);
            }
        }

        private void _RestoreHrgn()
        {
            _ClearRoundingRegion();
            NativeMethods.SetWindowPos(_hwnd, IntPtr.Zero, 0, 0, 0, 0, _SwpFlags);
        }

        #endregion

        private delegate void _Action();

        #region Fields

        private const SWP _SwpFlags = SWP.FRAMECHANGED | SWP.NOSIZE | SWP.NOMOVE | SWP.NOZORDER | SWP.NOOWNERZORDER | SWP.NOACTIVATE;
        private readonly List<KeyValuePair<WM, MessageHandler>> _messageTable;

        private WindowChrome _chromeInfo;
        private bool _hasUserMovedWindow;

        private IntPtr _hwnd;
        private HwndSource _hwndSource;

        private bool _isFixedUp;
        private bool _isGlassEnabled;
        private bool _isHooked;
        private bool _isUserResizing;
        private WindowState _lastMenuState;
        private WindowState _lastRoundingState;

        private Window _window;
        private Point _windowPosAtStartOfUserMove = default(Point);

        #endregion
    }
}