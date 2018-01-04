// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace FirstFloor.ModernUI.Shell.Standard
{
    internal sealed class MessageWindow : DispatcherObject, IDisposable
    {
        private static readonly WndProc s_WndProc = _WndProc;
        private static readonly Dictionary<IntPtr, MessageWindow> s_windowLookup = new Dictionary<IntPtr, MessageWindow>();
        private readonly WndProc _wndProcCallback;
        private string _className;
        private bool _isDisposed;

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public MessageWindow(CS classStyle, WS style, WS_EX exStyle, Rect location, string name, WndProc callback)
        {
            _wndProcCallback = callback;
            _className = "MessageWindowClass+" + Guid.NewGuid().ToString();
            var wc = new WNDCLASSEX
            {
                cbSize = Marshal.SizeOf(typeof(WNDCLASSEX)),
                style = classStyle,
                lpfnWndProc = s_WndProc,
                hInstance = NativeMethods.GetModuleHandle(null),
                hbrBackground = NativeMethods.GetStockObject(StockObject.NULL_BRUSH),
                lpszMenuName = "",
                lpszClassName = _className,
            };
            NativeMethods.RegisterClassEx(ref wc);
            var gcHandle = default(GCHandle);
            try
            {
                gcHandle = GCHandle.Alloc(this);
                var pinnedThisPtr = (IntPtr) gcHandle;
                Handle = NativeMethods.CreateWindowEx(exStyle, _className, name, style, (int) location.X, (int) location.Y, (int) location.Width,
                    (int) location.Height, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, pinnedThisPtr);
            }
            finally
            {
                gcHandle.Free();
            }
        }

        public IntPtr Handle { get; private set; }

        public void Dispose()
        {
            _Dispose(true, false);
            GC.SuppressFinalize(this);
        }

        ~MessageWindow()
        {
            _Dispose(false, false);
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "disposing")]
        private void _Dispose(bool disposing, bool isHwndBeingDestroyed)
        {
            if(_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            var hwnd = Handle;
            var className = _className;
            if(isHwndBeingDestroyed)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, (DispatcherOperationCallback) (arg => _DestroyWindow(IntPtr.Zero, className)));
            }
            else if(Handle != IntPtr.Zero)
            {
                if(CheckAccess())
                {
                    _DestroyWindow(hwnd, className);
                }
                else
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, (DispatcherOperationCallback) (arg => _DestroyWindow(hwnd, className)));
                }
            }
            s_windowLookup.Remove(hwnd);
            _className = null;
            Handle = IntPtr.Zero;
        }

        [SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
        private static IntPtr _WndProc(IntPtr hwnd, WM msg, IntPtr wParam, IntPtr lParam)
        {
            var ret = IntPtr.Zero;
            MessageWindow hwndWrapper = null;
            if(msg == WM.CREATE)
            {
                var createStruct = (CREATESTRUCT) Marshal.PtrToStructure(lParam, typeof(CREATESTRUCT));
                var gcHandle = GCHandle.FromIntPtr(createStruct.lpCreateParams);
                hwndWrapper = (MessageWindow) gcHandle.Target;
                s_windowLookup.Add(hwnd, hwndWrapper);
            }
            else
            {
                if(!s_windowLookup.TryGetValue(hwnd, out hwndWrapper))
                {
                    return NativeMethods.DefWindowProc(hwnd, msg, wParam, lParam);
                }
            }
            Assert.IsNotNull(hwndWrapper);
            var callback = hwndWrapper._wndProcCallback;
            ret = callback != null ? callback(hwnd, msg, wParam, lParam) : NativeMethods.DefWindowProc(hwnd, msg, wParam, lParam);
            if(msg == WM.NCDESTROY)
            {
                hwndWrapper._Dispose(true, true);
                GC.SuppressFinalize(hwndWrapper);
            }
            return ret;
        }

        private static object _DestroyWindow(IntPtr hwnd, string className)
        {
            Utility.SafeDestroyWindow(ref hwnd);
            NativeMethods.UnregisterClass(className, NativeMethods.GetModuleHandle(null));
            return null;
        }
    }
}