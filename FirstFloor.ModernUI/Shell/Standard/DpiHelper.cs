// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace FirstFloor.ModernUI.Shell.Standard
{
    internal static class DpiHelper
    {
        private static Matrix _transformToDevice;
        private static Matrix _transformToDip;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static DpiHelper()
        {
            using(var desktop = SafeDC.GetDesktop())
            {
                var pixelsPerInchX = NativeMethods.GetDeviceCaps(desktop, DeviceCap.LOGPIXELSX);
                var pixelsPerInchY = NativeMethods.GetDeviceCaps(desktop, DeviceCap.LOGPIXELSY);
                _transformToDip = Matrix.Identity;
                _transformToDip.Scale(96d / pixelsPerInchX, 96d / pixelsPerInchY);
                _transformToDevice = Matrix.Identity;
                _transformToDevice.Scale(pixelsPerInchX / 96d, pixelsPerInchY / 96d);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static Point LogicalPixelsToDevice(Point logicalPoint)
        {
            return _transformToDevice.Transform(logicalPoint);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static Point DevicePixelsToLogical(Point devicePoint)
        {
            return _transformToDip.Transform(devicePoint);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static Rect LogicalRectToDevice(Rect logicalRectangle)
        {
            var topLeft = LogicalPixelsToDevice(new Point(logicalRectangle.Left, logicalRectangle.Top));
            var bottomRight = LogicalPixelsToDevice(new Point(logicalRectangle.Right, logicalRectangle.Bottom));
            return new Rect(topLeft, bottomRight);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static Rect DeviceRectToLogical(Rect deviceRectangle)
        {
            var topLeft = DevicePixelsToLogical(new Point(deviceRectangle.Left, deviceRectangle.Top));
            var bottomRight = DevicePixelsToLogical(new Point(deviceRectangle.Right, deviceRectangle.Bottom));
            return new Rect(topLeft, bottomRight);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static Size LogicalSizeToDevice(Size logicalSize)
        {
            var pt = LogicalPixelsToDevice(new Point(logicalSize.Width, logicalSize.Height));
            return new Size {Width = pt.X, Height = pt.Y};
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static Size DeviceSizeToLogical(Size deviceSize)
        {
            var pt = DevicePixelsToLogical(new Point(deviceSize.Width, deviceSize.Height));
            return new Size(pt.X, pt.Y);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static Thickness LogicalThicknessToDevice(Thickness logicalThickness)
        {
            var topLeft = LogicalPixelsToDevice(new Point(logicalThickness.Left, logicalThickness.Top));
            var bottomRight = LogicalPixelsToDevice(new Point(logicalThickness.Right, logicalThickness.Bottom));
            return new Thickness(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y);
        }
    }
}