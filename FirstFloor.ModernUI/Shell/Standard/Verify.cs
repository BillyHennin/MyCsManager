// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading;

namespace FirstFloor.ModernUI.Shell.Standard
{
    internal static class Verify
    {
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void IsApartmentState(ApartmentState requiredState, string message)
        {
            if(Thread.CurrentThread.GetApartmentState() != requiredState)
            {
                Assert.Fail();
                throw new InvalidOperationException(message);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength")]
        [DebuggerStepThrough]
        public static void IsNeitherNullNorEmpty(string value, string name)
        {
            Assert.IsNeitherNullNorEmpty(name);

            const string errorMessage = "The parameter can not be either null or empty.";
            if(null == value)
            {
                Assert.Fail();
                throw new ArgumentNullException(name, errorMessage);
            }
            if("" == value)
            {
                Assert.Fail();
                throw new ArgumentException(errorMessage, name);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength")]
        [DebuggerStepThrough]
        public static void IsNeitherNullNorWhitespace(string value, string name)
        {
            Assert.IsNeitherNullNorEmpty(name);

            const string errorMessage = "The parameter can not be either null or empty or consist only of white space characters.";
            if(null == value)
            {
                Assert.Fail();
                throw new ArgumentNullException(name, errorMessage);
            }
            if("" == value.Trim())
            {
                Assert.Fail();
                throw new ArgumentException(errorMessage, name);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void IsNotDefault<T>(T obj, string name) where T : struct
        {
            if(default(T).Equals(obj))
            {
                Assert.Fail();
                throw new ArgumentException(@"The parameter must not be the default value.", name);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void IsNotNull<T>(T obj, string name) where T : class
        {
            if(null == obj)
            {
                Assert.Fail();
                throw new ArgumentNullException(name);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void IsNotNull<T>(T obj, string name, string message) where T : class
        {
            if(null == obj)
            {
                Assert.Fail();
                throw new ArgumentNullException(name, message);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void IsNull<T>(T obj, string name) where T : class
        {
            if(null != obj)
            {
                Assert.Fail();
                throw new ArgumentException(@"The parameter must be null.", name);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void PropertyIsNotNull<T>(T obj, string name) where T : class
        {
            if(null == obj)
            {
                Assert.Fail();
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The property {0} cannot be null at this time.", name));
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void PropertyIsNull<T>(T obj, string name) where T : class
        {
            if(null != obj)
            {
                Assert.Fail();
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The property {0} must be null at this time.", name));
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void IsTrue(bool statement, string name, string message = null)
        {
            if(!statement)
            {
                Assert.Fail();
                throw new ArgumentException(message ?? "", name);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void IsFalse(bool statement, string name, string message = null)
        {
            if(statement)
            {
                Assert.Fail();
                throw new ArgumentException(message ?? "", name);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void AreEqual<T>(T expected, T actual, string parameterName, string message)
        {
            if(null == expected)
            {
                if(null != actual && !actual.Equals(expected))
                {
                    Assert.Fail();
                    throw new ArgumentException(message, parameterName);
                }
            }
            else if(!expected.Equals(actual))
            {
                Assert.Fail();
                throw new ArgumentException(message, parameterName);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void AreNotEqual<T>(T notExpected, T actual, string parameterName, string message)
        {
            if(null == notExpected)
            {
                if(null == actual || actual.Equals(notExpected))
                {
                    Assert.Fail();
                    throw new ArgumentException(message, parameterName);
                }
            }
            else if(notExpected.Equals(actual))
            {
                Assert.Fail();
                throw new ArgumentException(message, parameterName);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void UriIsAbsolute(Uri uri, string parameterName)
        {
            IsNotNull(uri, parameterName);
            if(!uri.IsAbsoluteUri)
            {
                Assert.Fail();
                throw new ArgumentException(@"The URI must be absolute.", parameterName);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void BoundedInteger(int lowerBoundInclusive, int value, int upperBoundExclusive, string parameterName)
        {
            if(value < lowerBoundInclusive || value >= upperBoundExclusive)
            {
                Assert.Fail();
                throw new ArgumentException(
                    string.Format(CultureInfo.InvariantCulture, "The integer value must be bounded with [{0}, {1})", lowerBoundInclusive, upperBoundExclusive),
                    parameterName);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void BoundedDoubleInc(double lowerBoundInclusive, double value, double upperBoundInclusive, string message, string parameter)
        {
            if(value < lowerBoundInclusive || value > upperBoundInclusive)
            {
                Assert.Fail();
                throw new ArgumentException(message, parameter);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void TypeSupportsInterface(Type type, Type interfaceType, string parameterName)
        {
            Assert.IsNeitherNullNorEmpty(parameterName);
            IsNotNull(type, "type");
            IsNotNull(interfaceType, "interfaceType");
            if(type.GetInterface(interfaceType.Name) == null)
            {
                Assert.Fail();
                throw new ArgumentException(@"The type of this parameter does not support a required interface", parameterName);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void FileExists(string filePath, string parameterName)
        {
            IsNeitherNullNorEmpty(filePath, parameterName);
            if(!File.Exists(filePath))
            {
                Assert.Fail();
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "No file exists at \"{0}\"", filePath), parameterName);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        internal static void ImplementsInterface(object parameter, Type interfaceType, string parameterName)
        {
            Assert.IsNotNull(parameter);
            Assert.IsNotNull(interfaceType);
            Assert.IsTrue(interfaceType.IsInterface);
            var isImplemented = false;
            foreach(var ifaceType in parameter.GetType().GetInterfaces())
            {
                if(ifaceType == interfaceType)
                {
                    isImplemented = true;
                    break;
                }
            }
            if(!isImplemented)
            {
                Assert.Fail();
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The parameter must implement interface {0}.", interfaceType),
                    parameterName);
            }
        }
    }
}