// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System.Diagnostics.CodeAnalysis;

namespace FirstFloor.ModernUI.Shell.Standard
{
    internal static class DoubleUtilities
    {
        private const double Epsilon = 0.00000153;

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool AreClose(double value1, double value2)
        {
            if(value1 == value2)
            {
                return true;
            }
            var delta = value1 - value2;
            return (delta < Epsilon) && (delta > -Epsilon);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool IsCloseTo(this double value1, double value2)
        {
            return AreClose(value1, value2);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool IsStrictlyLessThan(this double value1, double value2)
        {
            return (value1 < value2) && !AreClose(value1, value2);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool IsStrictlyGreaterThan(this double value1, double value2)
        {
            return (value1 > value2) && !AreClose(value1, value2);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool IsLessThanOrCloseTo(this double value1, double value2)
        {
            return (value1 < value2) || AreClose(value1, value2);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool IsGreaterThanOrCloseTo(this double value1, double value2)
        {
            return (value1 > value2) || AreClose(value1, value2);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool IsFinite(this double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool IsValidSize(this double value)
        {
            return IsFinite(value) && value.IsGreaterThanOrCloseTo(0);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool IsFiniteAndNonNegative(this double d)
        {
            if(double.IsNaN(d) || double.IsInfinity(d) || d < 0)
            {
                return false;
            }
            return true;
        }
    }
}