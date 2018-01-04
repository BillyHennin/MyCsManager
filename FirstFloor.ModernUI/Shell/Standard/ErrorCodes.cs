// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

namespace FirstFloor.ModernUI.Shell.Standard
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct Win32Error
    {
        [FieldOffset(0)]
        private readonly int _value;

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_SUCCESS = new Win32Error(0);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_INVALID_FUNCTION = new Win32Error(1);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_FILE_NOT_FOUND = new Win32Error(2);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_PATH_NOT_FOUND = new Win32Error(3);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_TOO_MANY_OPEN_FILES = new Win32Error(4);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_ACCESS_DENIED = new Win32Error(5);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_INVALID_HANDLE = new Win32Error(6);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_OUTOFMEMORY = new Win32Error(14);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_NO_MORE_FILES = new Win32Error(18);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_SHARING_VIOLATION = new Win32Error(32);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_INVALID_PARAMETER = new Win32Error(87);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_INSUFFICIENT_BUFFER = new Win32Error(122);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_NESTING_NOT_ALLOWED = new Win32Error(215);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_KEY_DELETED = new Win32Error(1018);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_NOT_FOUND = new Win32Error(1168);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_NO_MATCH = new Win32Error(1169);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_BAD_DEVICE = new Win32Error(1200);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_CANCELLED = new Win32Error(1223);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_CANNOT_FIND_WND_CLASS = new Win32Error(1407);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_CLASS_ALREADY_EXISTS = new Win32Error(1410);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_INVALID_DATATYPE = new Win32Error(1804);

        public Win32Error(int i)
        {
            _value = i;
        }

        public static explicit operator HRESULT(Win32Error error)
        {
            if(error._value <= 0)
            {
                return new HRESULT((uint) error._value);
            }
            return HRESULT.Make(true, Facility.Win32, error._value & 0x0000FFFF);
        }

        public HRESULT ToHRESULT()
        {
            return (HRESULT) this;
        }

        public static Win32Error GetLastError()
        {
            return new Win32Error(Marshal.GetLastWin32Error());
        }

        public override bool Equals(object obj)
        {
            try
            {
                return ((Win32Error) obj)._value == _value;
            }
            catch(InvalidCastException)
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static bool operator ==(Win32Error errLeft, Win32Error errRight)
        {
            return errLeft._value == errRight._value;
        }

        public static bool operator !=(Win32Error errLeft, Win32Error errRight)
        {
            return !(errLeft == errRight);
        }
    }

    internal enum Facility
    {
        Null = 0,

        Rpc = 1,

        Dispatch = 2,

        Storage = 3,

        Itf = 4,

        Win32 = 7,

        Windows = 8,

        Control = 10,

        Ese = 0xE5E,

        WinCodec = 0x898,
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct HRESULT
    {
        [FieldOffset(0)]
        private readonly uint _value;

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT S_OK = new HRESULT(0x00000000);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT S_FALSE = new HRESULT(0x00000001);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT E_PENDING = new HRESULT(0x8000000A);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT E_NOTIMPL = new HRESULT(0x80004001);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT E_NOINTERFACE = new HRESULT(0x80004002);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT E_POINTER = new HRESULT(0x80004003);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT E_ABORT = new HRESULT(0x80004004);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT E_FAIL = new HRESULT(0x80004005);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT E_UNEXPECTED = new HRESULT(0x8000FFFF);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT STG_E_INVALIDFUNCTION = new HRESULT(0x80030001);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT OLE_E_ADVISENOTSUPPORTED = new HRESULT(0x80040003);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT DV_E_FORMATETC = new HRESULT(0x80040064);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT DV_E_TYMED = new HRESULT(0x80040069);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT DV_E_CLIPFORMAT = new HRESULT(0x8004006A);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT DV_E_DVASPECT = new HRESULT(0x8004006B);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT REGDB_E_CLASSNOTREG = new HRESULT(0x80040154);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT DESTS_E_NO_MATCHING_ASSOC_HANDLER = new HRESULT(0x80040F03);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT DESTS_E_NORECDOCS = new HRESULT(0x80040F04);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT DESTS_E_NOTALLCLEARED = new HRESULT(0x80040F05);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT E_ACCESSDENIED = new HRESULT(0x80070005);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT E_OUTOFMEMORY = new HRESULT(0x8007000E);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT E_INVALIDARG = new HRESULT(0x80070057);

        public static readonly HRESULT INTSAFE_E_ARITHMETIC_OVERFLOW = new HRESULT(0x80070216);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT COR_E_OBJECTDISPOSED = new HRESULT(0x80131622);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WC_E_GREATERTHAN = new HRESULT(0xC00CEE23);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WC_E_SYNTAX = new HRESULT(0xC00CEE2D);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_GENERIC_ERROR = E_FAIL;
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_INVALIDPARAMETER = E_INVALIDARG;
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_OUTOFMEMORY = E_OUTOFMEMORY;
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_NOTIMPLEMENTED = E_NOTIMPL;
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_ABORTED = E_ABORT;
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_ACCESSDENIED = E_ACCESSDENIED;
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_VALUEOVERFLOW = INTSAFE_E_ARITHMETIC_OVERFLOW;
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_WRONGSTATE = Make(true, Facility.WinCodec, 0x2f04);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_VALUEOUTOFRANGE = Make(true, Facility.WinCodec, 0x2f05);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_UNKNOWNIMAGEFORMAT = Make(true, Facility.WinCodec, 0x2f07);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_UNSUPPORTEDVERSION = Make(true, Facility.WinCodec, 0x2f0B);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_NOTINITIALIZED = Make(true, Facility.WinCodec, 0x2f0C);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_ALREADYLOCKED = Make(true, Facility.WinCodec, 0x2f0D);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_PROPERTYNOTFOUND = Make(true, Facility.WinCodec, 0x2f40);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_PROPERTYNOTSUPPORTED = Make(true, Facility.WinCodec, 0x2f41);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_PROPERTYSIZE = Make(true, Facility.WinCodec, 0x2f42);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_CODECPRESENT = Make(true, Facility.WinCodec, 0x2f43);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_CODECNOTHUMBNAIL = Make(true, Facility.WinCodec, 0x2f44);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_PALETTEUNAVAILABLE = Make(true, Facility.WinCodec, 0x2f45);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_CODECTOOMANYSCANLINES = Make(true, Facility.WinCodec, 0x2f46);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_INTERNALERROR = Make(true, Facility.WinCodec, 0x2f48);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_SOURCERECTDOESNOTMATCHDIMENSIONS = Make(true, Facility.WinCodec, 0x2f49);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_COMPONENTNOTFOUND = Make(true, Facility.WinCodec, 0x2f50);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_IMAGESIZEOUTOFRANGE = Make(true, Facility.WinCodec, 0x2f51);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_TOOMUCHMETADATA = Make(true, Facility.WinCodec, 0x2f52);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_BADIMAGE = Make(true, Facility.WinCodec, 0x2f60);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_BADHEADER = Make(true, Facility.WinCodec, 0x2f61);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_FRAMEMISSING = Make(true, Facility.WinCodec, 0x2f62);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_BADMETADATAHEADER = Make(true, Facility.WinCodec, 0x2f63);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_BADSTREAMDATA = Make(true, Facility.WinCodec, 0x2f70);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_STREAMWRITE = Make(true, Facility.WinCodec, 0x2f71);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_STREAMREAD = Make(true, Facility.WinCodec, 0x2f72);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_STREAMNOTAVAILABLE = Make(true, Facility.WinCodec, 0x2f73);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_UNSUPPORTEDPIXELFORMAT = Make(true, Facility.WinCodec, 0x2f80);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_UNSUPPORTEDOPERATION = Make(true, Facility.WinCodec, 0x2f81);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_INVALIDREGISTRATION = Make(true, Facility.WinCodec, 0x2f8A);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_COMPONENTINITIALIZEFAILURE = Make(true, Facility.WinCodec, 0x2f8B);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_INSUFFICIENTBUFFER = Make(true, Facility.WinCodec, 0x2f8C);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_DUPLICATEMETADATAPRESENT = Make(true, Facility.WinCodec, 0x2f8D);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_PROPERTYUNEXPECTEDTYPE = Make(true, Facility.WinCodec, 0x2f8E);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_UNEXPECTEDSIZE = Make(true, Facility.WinCodec, 0x2f8F);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_INVALIDQUERYREQUEST = Make(true, Facility.WinCodec, 0x2f90);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_UNEXPECTEDMETADATATYPE = Make(true, Facility.WinCodec, 0x2f91);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_REQUESTONLYVALIDATMETADATAROOT = Make(true, Facility.WinCodec, 0x2f92);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_INVALIDQUERYCHARACTER = Make(true, Facility.WinCodec, 0x2f93);

        public HRESULT(uint i)
        {
            _value = i;
        }

        public HRESULT(int i)
        {
            _value = unchecked((uint) i);
        }

        public static explicit operator int(HRESULT hr)
        {
            unchecked
            {
                return (int) hr._value;
            }
        }

        public static HRESULT Make(bool severe, Facility facility, int code)
        {
            Assert.Implies((int) facility != ((int) facility & 0x1FF), facility == Facility.Ese || facility == Facility.WinCodec);

            Assert.AreEqual(code, code & 0xFFFF);
            return new HRESULT((uint) ((severe ? (1 << 31) : 0) | ((int) facility << 16) | code));
        }

        public Facility Facility { get { return GetFacility((int) _value); } }

        public static Facility GetFacility(int errorCode)
        {
            return (Facility) ((errorCode >> 16) & 0x1fff);
        }

        public int Code { get { return GetCode((int) _value); } }

        public static int GetCode(int error)
        {
            return (error & 0xFFFF);
        }

        #region Object class override members

        public override string ToString()
        {
            foreach(var publicStaticField in typeof(HRESULT).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                if(publicStaticField.FieldType == typeof(HRESULT))
                {
                    var hr = (HRESULT) publicStaticField.GetValue(null);
                    if(hr == this)
                    {
                        return publicStaticField.Name;
                    }
                }
            }

            if(Facility == Facility.Win32)
            {
                foreach(var publicStaticField in typeof(Win32Error).GetFields(BindingFlags.Static | BindingFlags.Public))
                {
                    if(publicStaticField.FieldType == typeof(Win32Error))
                    {
                        var error = (Win32Error) publicStaticField.GetValue(null);
                        if((HRESULT) error == this)
                        {
                            return "HRESULT_FROM_WIN32(" + publicStaticField.Name + ")";
                        }
                    }
                }
            }

            return string.Format(CultureInfo.InvariantCulture, "0x{0:X8}", _value);
        }

        public override bool Equals(object obj)
        {
            try
            {
                return ((HRESULT) obj)._value == _value;
            }
            catch(InvalidCastException)
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        #endregion

        public static bool operator ==(HRESULT hrLeft, HRESULT hrRight)
        {
            return hrLeft._value == hrRight._value;
        }

        public static bool operator !=(HRESULT hrLeft, HRESULT hrRight)
        {
            return !(hrLeft == hrRight);
        }

        public bool Succeeded { get { return (int) _value >= 0; } }
        public bool Failed { get { return (int) _value < 0; } }

        public void ThrowIfFailed()
        {
            ThrowIfFailed(null);
        }

        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Justification = "Only recreating Exceptions that were already raised.")]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public void ThrowIfFailed(string message)
        {
            if(Failed)
            {
                if(string.IsNullOrEmpty(message))
                {
                    message = ToString();
                }
#if DEBUG
                else
                {
                    message += " (" + ToString() + ")";
                }
#endif

                var e = Marshal.GetExceptionForHR((int) _value, new IntPtr(-1));
                Assert.IsNotNull(e);

                Assert.IsFalse(e is ArgumentNullException);

                if(e.GetType() == typeof(COMException))
                {
                    switch(Facility)
                    {
                        case Facility.Win32:
                            e = new Win32Exception(Code, message);
                            break;
                        default:
                            e = new COMException(message, (int) _value);
                            break;
                    }
                }
                else
                {
                    var cons = e.GetType().GetConstructor(new[] {typeof(string)});
                    if(null != cons)
                    {
                        e = cons.Invoke(new object[] {message}) as Exception;
                        Assert.IsNotNull(e);
                    }
                }
                throw e;
            }
        }

        public static void ThrowLastError()
        {
            ((HRESULT) Win32Error.GetLastError()).ThrowIfFailed();

            Assert.Fail();
        }
    }
}