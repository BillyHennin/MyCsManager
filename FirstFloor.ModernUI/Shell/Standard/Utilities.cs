// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FirstFloor.ModernUI.Shell.Standard
{
    internal enum SafeCopyFileOptions
    {
        PreserveOriginal,
        Overwrite,
        FindBetterName,
    }

    internal static class Utility
    {
        private static readonly Random _randomNumberGenerator = new Random();
        private static readonly bool _isNotAtRuntime = (bool) DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue;
        private static int s_bitDepth;
        private static readonly Version _osVersion = Environment.OSVersion.Version;
        public static bool IsInDesignMode { get { return _isNotAtRuntime; } }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool IsOSVistaOrNewer { get { return _osVersion >= new Version(6, 0); } }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool IsOSWindows7OrNewer { get { return _osVersion >= new Version(6, 1); } }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static bool _MemCmp(IntPtr left, IntPtr right, long cb)
        {
            var offset = 0;
            for(; offset < (cb - sizeof(Int64)); offset += sizeof(Int64))
            {
                var left64 = Marshal.ReadInt64(left, offset);
                var right64 = Marshal.ReadInt64(right, offset);
                if(left64 != right64)
                {
                    return false;
                }
            }
            for(; offset < cb; offset += sizeof(byte))
            {
                var left8 = Marshal.ReadByte(left, offset);
                var right8 = Marshal.ReadByte(right, offset);
                if(left8 != right8)
                {
                    return false;
                }
            }
            return true;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static Exception FailableFunction<T>(Func<T> function, out T result)
        {
            return FailableFunction(5, function, out result);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static T FailableFunction<T>(Func<T> function)
        {
            T result;
            var e = FailableFunction(function, out result);
            if(e != null)
            {
                throw e;
            }
            return result;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static T FailableFunction<T>(int maxRetries, Func<T> function)
        {
            T result;
            var e = FailableFunction(maxRetries, function, out result);
            if(e != null)
            {
                throw e;
            }
            return result;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static Exception FailableFunction<T>(int maxRetries, Func<T> function, out T result)
        {
            Assert.IsNotNull(function);
            Assert.BoundedInteger(1, maxRetries, 100);
            var i = 0;
            while(true)
            {
                try
                {
                    result = function();
                    return null;
                }
                catch(Exception e)
                {
                    if(i == maxRetries)
                    {
                        result = default(T);
                        return e;
                    }
                }
                ++i;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static string GetHashString(string value)
        {
            using(var md5 = MD5.Create())
            {
                var signatureHash = md5.ComputeHash(Encoding.UTF8.GetBytes(value));
                var signature = signatureHash.Aggregate(new StringBuilder(), (sb, b) => sb.Append(b.ToString("x2", CultureInfo.InvariantCulture))).ToString();
                return signature;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static int GET_X_LPARAM(IntPtr lParam)
        {
            return LOWORD(lParam.ToInt32());
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static int GET_Y_LPARAM(IntPtr lParam)
        {
            return HIWORD(lParam.ToInt32());
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static int HIWORD(int i)
        {
            return (short) (i >> 16);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static int LOWORD(int i)
        {
            return (short) (i & 0xFFFF);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public static bool AreStreamsEqual(Stream left, Stream right)
        {
            if(null == left)
            {
                return right == null;
            }
            if(null == right)
            {
                return false;
            }
            if(!left.CanRead || !right.CanRead)
            {
                throw new NotSupportedException("The streams can't be read for comparison");
            }
            if(left.Length != right.Length)
            {
                return false;
            }
            var length = (int) left.Length;

            left.Position = 0;
            right.Position = 0;

            var totalReadLeft = 0;
            var totalReadRight = 0;

            var leftBuffer = new byte[512];
            var rightBuffer = new byte[512];

            var handleLeft = GCHandle.Alloc(leftBuffer, GCHandleType.Pinned);
            var ptrLeft = handleLeft.AddrOfPinnedObject();

            var handleRight = GCHandle.Alloc(rightBuffer, GCHandleType.Pinned);
            var ptrRight = handleRight.AddrOfPinnedObject();
            try
            {
                var cbReadLeft = 0;
                var cbReadRight = 0;
                while(totalReadLeft < length)
                {
                    Assert.AreEqual(totalReadLeft, totalReadRight);
                    cbReadLeft = left.Read(leftBuffer, 0, leftBuffer.Length);
                    cbReadRight = right.Read(rightBuffer, 0, rightBuffer.Length);

                    if(cbReadLeft != cbReadRight)
                    {
                        return false;
                    }
                    if(!_MemCmp(ptrLeft, ptrRight, cbReadLeft))
                    {
                        return false;
                    }
                    totalReadLeft += cbReadLeft;
                    totalReadRight += cbReadRight;
                }
                Assert.AreEqual(cbReadLeft, cbReadRight);
                Assert.AreEqual(totalReadLeft, totalReadRight);
                Assert.AreEqual(length, totalReadLeft);
                return true;
            }
            finally
            {
                handleLeft.Free();
                handleRight.Free();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool GuidTryParse(string guidString, out Guid guid)
        {
            Verify.IsNeitherNullNorEmpty(guidString, "guidString");
            try
            {
                guid = new Guid(guidString);
                return true;
            }
            catch(FormatException) {}
            catch(OverflowException) {}

            guid = default(Guid);
            return false;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool IsFlagSet(int value, int mask)
        {
            return 0 != (value & mask);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool IsFlagSet(uint value, uint mask)
        {
            return 0 != (value & mask);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool IsFlagSet(long value, long mask)
        {
            return 0 != (value & mask);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool IsFlagSet(ulong value, ulong mask)
        {
            return 0 != (value & mask);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool IsInterfaceImplemented(Type objectType, Type interfaceType)
        {
            Assert.IsNotNull(objectType);
            Assert.IsNotNull(interfaceType);
            Assert.IsTrue(interfaceType.IsInterface);
            return objectType.GetInterfaces().Any(type => type == interfaceType);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static string SafeCopyFile(string sourceFileName, string destFileName, SafeCopyFileOptions options)
        {
            switch(options)
            {
                case SafeCopyFileOptions.PreserveOriginal:
                    if(!File.Exists(destFileName))
                    {
                        File.Copy(sourceFileName, destFileName);
                        return destFileName;
                    }
                    return null;
                case SafeCopyFileOptions.Overwrite:
                    File.Copy(sourceFileName, destFileName, true);
                    return destFileName;
                case SafeCopyFileOptions.FindBetterName:
                    var directoryPart = Path.GetDirectoryName(destFileName);
                    var fileNamePart = Path.GetFileNameWithoutExtension(destFileName);
                    var extensionPart = Path.GetExtension(destFileName);
                    foreach(var path in GenerateFileNames(directoryPart, fileNamePart, extensionPart))
                    {
                        if(!File.Exists(path))
                        {
                            File.Copy(sourceFileName, path);
                            return path;
                        }
                    }
                    return null;
            }
            throw new ArgumentException(@"Invalid enumeration value", "options");
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void SafeDeleteFile(string path)
        {
            if(!string.IsNullOrEmpty(path))
            {
                File.Delete(path);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void SafeDispose<T>(ref T disposable) where T : IDisposable
        {
            IDisposable t = disposable;
            disposable = default(T);
            if(null != t)
            {
                t.Dispose();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void GeneratePropertyString(StringBuilder source, string propertyName, string value)
        {
            Assert.IsNotNull(source);
            Assert.IsFalse(string.IsNullOrEmpty(propertyName));
            if(0 != source.Length)
            {
                source.Append(' ');
            }
            source.Append(propertyName);
            source.Append(": ");
            if(string.IsNullOrEmpty(value))
            {
                source.Append("<null>");
            }
            else
            {
                source.Append('\"');
                source.Append(value);
                source.Append('\"');
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [Obsolete]
        public static string GenerateToString<T>(T @object) where T : struct
        {
            var sbRet = new StringBuilder();
            foreach(var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if(0 != sbRet.Length)
                {
                    sbRet.Append(", ");
                }
                Assert.AreEqual(0, property.GetIndexParameters().Length);
                var value = property.GetValue(@object, null);
                var format = null == value ? "{0}: <null>" : "{0}: \"{1}\"";
                sbRet.AppendFormat(format, property.Name, value);
            }
            return sbRet.ToString();
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void CopyStream(Stream destination, Stream source)
        {
            Assert.IsNotNull(source);
            Assert.IsNotNull(destination);
            destination.Position = 0;

            if(source.CanSeek)
            {
                source.Position = 0;

                destination.SetLength(source.Length);
            }
            var buffer = new byte[4096];
            int cbRead;
            do
            {
                cbRead = source.Read(buffer, 0, buffer.Length);
                if(0 != cbRead)
                {
                    destination.Write(buffer, 0, cbRead);
                }
            }
            while(buffer.Length == cbRead);

            destination.Position = 0;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static string HashStreamMD5(Stream stm)
        {
            stm.Position = 0;
            var hashBuilder = new StringBuilder();
            using(var md5 = MD5.Create())
            {
                foreach(var b in md5.ComputeHash(stm))
                {
                    hashBuilder.Append(b.ToString("x2", CultureInfo.InvariantCulture));
                }
            }
            return hashBuilder.ToString();
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void EnsureDirectory(string path)
        {
            if(!path.EndsWith(@"\", StringComparison.Ordinal))
            {
                path += @"\";
            }
            path = Path.GetDirectoryName(path);
            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool MemCmp(byte[] left, byte[] right, int cb)
        {
            Assert.IsNotNull(left);
            Assert.IsNotNull(right);
            Assert.IsTrue(cb <= Math.Min(left.Length, right.Length));

            var handleLeft = GCHandle.Alloc(left, GCHandleType.Pinned);
            var ptrLeft = handleLeft.AddrOfPinnedObject();

            var handleRight = GCHandle.Alloc(right, GCHandleType.Pinned);
            var ptrRight = handleRight.AddrOfPinnedObject();
            var fRet = _MemCmp(ptrLeft, ptrRight, cb);
            handleLeft.Free();
            handleRight.Free();
            return fRet;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static string UrlDecode(string url)
        {
            if(url == null)
            {
                return null;
            }
            var decoder = new _UrlDecoder(url.Length, Encoding.UTF8);
            var length = url.Length;
            for(var i = 0; i < length; ++i)
            {
                var ch = url[i];
                if(ch == '+')
                {
                    decoder.AddByte((byte) ' ');
                    continue;
                }
                if(ch == '%' && i < length - 2)
                {
                    if(url[i + 1] == 'u' && i < length - 5)
                    {
                        var a = _HexToInt(url[i + 2]);
                        var b = _HexToInt(url[i + 3]);
                        var c = _HexToInt(url[i + 4]);
                        var d = _HexToInt(url[i + 5]);
                        if(a >= 0 && b >= 0 && c >= 0 && d >= 0)
                        {
                            decoder.AddChar((char) ((a << 12) | (b << 8) | (c << 4) | d));
                            i += 5;
                            continue;
                        }
                    }
                    else
                    {
                        var a = _HexToInt(url[i + 1]);
                        var b = _HexToInt(url[i + 2]);
                        if(a >= 0 && b >= 0)
                        {
                            decoder.AddByte((byte) ((a << 4) | b));
                            i += 2;
                            continue;
                        }
                    }
                }

                if((ch & 0xFF80) == 0)
                {
                    decoder.AddByte((byte) ch);
                }
                else
                {
                    decoder.AddChar(ch);
                }
            }
            return decoder.GetString();
        }

        /// Safe characters are defined in RFC2396 (http://www.ietf.org/rfc/rfc2396.txt).
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static string UrlEncode(string url)
        {
            if(url == null)
            {
                return null;
            }
            var bytes = Encoding.UTF8.GetBytes(url);
            var needsEncoding = false;
            var unsafeCharCount = 0;
            foreach(var b in bytes)
            {
                if(b == ' ')
                {
                    needsEncoding = true;
                }
                else if(!_UrlEncodeIsSafe(b))
                {
                    ++unsafeCharCount;
                    needsEncoding = true;
                }
            }
            if(needsEncoding)
            {
                var buffer = new byte[bytes.Length + (unsafeCharCount * 2)];
                var writeIndex = 0;
                foreach(var b in bytes)
                {
                    if(_UrlEncodeIsSafe(b))
                    {
                        buffer[writeIndex++] = b;
                    }
                    else if(b == ' ')
                    {
                        buffer[writeIndex++] = (byte) '+';
                    }
                    else
                    {
                        buffer[writeIndex++] = (byte) '%';
                        buffer[writeIndex++] = _IntToHex((b >> 4) & 0xF);
                        buffer[writeIndex++] = _IntToHex(b & 0xF);
                    }
                }
                bytes = buffer;
                Assert.AreEqual(buffer.Length, writeIndex);
            }
            return Encoding.ASCII.GetString(bytes);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static bool _UrlEncodeIsSafe(byte b)
        {
            if(_IsAsciiAlphaNumeric(b))
            {
                return true;
            }
            switch((char) b)
            {
                case '-':
                case '_':
                case '.':
                case '!':

                case '*':
                case '\'':
                case '(':
                case ')':
                    return true;
            }
            return false;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static bool _IsAsciiAlphaNumeric(byte b)
        {
            return (b >= 'a' && b <= 'z') || (b >= 'A' && b <= 'Z') || (b >= '0' && b <= '9');
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static byte _IntToHex(int n)
        {
            Assert.BoundedInteger(0, n, 16);
            if(n <= 9)
            {
                return (byte) (n + '0');
            }
            return (byte) (n - 10 + 'A');
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static int _HexToInt(char h)
        {
            if(h >= '0' && h <= '9')
            {
                return h - '0';
            }
            if(h >= 'a' && h <= 'f')
            {
                return h - 'a' + 10;
            }
            if(h >= 'A' && h <= 'F')
            {
                return h - 'A' + 10;
            }
            Assert.Fail("Invalid hex character " + h);
            return -1;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static string MakeValidFileName(string invalidPath)
        {
            return
                invalidPath.Replace('\\', '_').Replace('/', '_').Replace(':', '_').Replace('*', '_').Replace('?', '_').Replace('\"', '_').Replace('<', '_')
                    .Replace('>', '_').Replace('|', '_');
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static IEnumerable<string> GenerateFileNames(string directory, string primaryFileName, string extension)
        {
            Verify.IsNeitherNullNorEmpty(directory, "directory");
            Verify.IsNeitherNullNorEmpty(primaryFileName, "primaryFileName");
            primaryFileName = MakeValidFileName(primaryFileName);
            for(var i = 0; i <= 50; ++i)
            {
                if(0 == i)
                {
                    yield return Path.Combine(directory, primaryFileName) + extension;
                }
                else if(40 >= i)
                {
                    yield return Path.Combine(directory, primaryFileName) + " (" + i.ToString((IFormatProvider) null) + ")" + extension;
                }
                else
                {
                    yield return Path.Combine(directory, primaryFileName) + " (" + _randomNumberGenerator.Next(41, 9999) + ")" + extension;
                }
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool TryFileMove(string sourceFileName, string destFileName)
        {
            if(!File.Exists(destFileName))
            {
                try
                {
                    File.Move(sourceFileName, destFileName);
                }
                catch(IOException)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static byte[] GetBytesFromBitmapSource(BitmapSource bmp)
        {
            var width = bmp.PixelWidth;
            var height = bmp.PixelHeight;
            var stride = width * ((bmp.Format.BitsPerPixel + 7) / 8);
            var pixels = new byte[height * stride];
            bmp.CopyPixels(pixels, stride, 0);
            return pixels;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static BitmapSource GenerateBitmapSource(ImageSource img)
        {
            return GenerateBitmapSource(img, img.Width, img.Height);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static BitmapSource GenerateBitmapSource(ImageSource img, double renderWidth, double renderHeight)
        {
            var dv = new DrawingVisual();
            using(var dc = dv.RenderOpen())
            {
                dc.DrawImage(img, new Rect(0, 0, renderWidth, renderHeight));
            }
            var bmp = new RenderTargetBitmap((int) renderWidth, (int) renderHeight, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(dv);
            return bmp;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static BitmapSource GenerateBitmapSource(UIElement element, double renderWidth, double renderHeight, bool performLayout)
        {
            if(performLayout)
            {
                element.Measure(new Size(renderWidth, renderHeight));
                element.Arrange(new Rect(new Size(renderWidth, renderHeight)));
            }
            var bmp = new RenderTargetBitmap((int) renderWidth, (int) renderHeight, 96, 96, PixelFormats.Pbgra32);
            var dv = new DrawingVisual();
            using(var dc = dv.RenderOpen())
            {
                dc.DrawRectangle(new VisualBrush(element), null, new Rect(0, 0, renderWidth, renderHeight));
            }
            bmp.Render(dv);
            return bmp;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void SaveToPng(BitmapSource source, string fileName)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(source));
            using(var stream = File.Create(fileName))
            {
                encoder.Save(stream);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static int _GetBitDepth()
        {
            if(s_bitDepth == 0)
            {
                using(var dc = SafeDC.GetDesktop())
                {
                    s_bitDepth = NativeMethods.GetDeviceCaps(dc, DeviceCap.BITSPIXEL) * NativeMethods.GetDeviceCaps(dc, DeviceCap.PLANES);
                }
            }
            return s_bitDepth;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static BitmapFrame GetBestMatch(IList<BitmapFrame> frames, int width, int height)
        {
            return _GetBestMatch(frames, _GetBitDepth(), width, height);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static int _MatchImage(BitmapFrame frame, int bitDepth, int width, int height, int bpp)
        {
            var score = 2 * _WeightedAbs(bpp, bitDepth, false) + _WeightedAbs(frame.PixelWidth, width, true) + _WeightedAbs(frame.PixelHeight, height, true);
            return score;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static int _WeightedAbs(int valueHave, int valueWant, bool fPunish)
        {
            var diff = (valueHave - valueWant);
            if(diff < 0)
            {
                diff = (fPunish ? -2 : -1) * diff;
            }
            return diff;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static BitmapFrame _GetBestMatch(IList<BitmapFrame> frames, int bitDepth, int width, int height)
        {
            var bestScore = int.MaxValue;
            var bestBpp = 0;
            var bestIndex = 0;
            var isBitmapIconDecoder = frames[0].Decoder is IconBitmapDecoder;
            for(var i = 0; i < frames.Count && bestScore != 0; ++i)
            {
                var currentIconBitDepth = isBitmapIconDecoder ? frames[i].Thumbnail.Format.BitsPerPixel : frames[i].Format.BitsPerPixel;
                if(currentIconBitDepth == 0)
                {
                    currentIconBitDepth = 8;
                }
                var score = _MatchImage(frames[i], bitDepth, width, height, currentIconBitDepth);
                if(score < bestScore)
                {
                    bestIndex = i;
                    bestBpp = currentIconBitDepth;
                    bestScore = score;
                }
                else if(score == bestScore)
                {
                    if(bestBpp < currentIconBitDepth)
                    {
                        bestIndex = i;
                        bestBpp = currentIconBitDepth;
                    }
                }
            }
            return frames[bestIndex];
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static int RGB(Color c)
        {
            return c.B | (c.G << 8) | (c.R << 16);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static int AlphaRGB(Color c)
        {
            return c.B | (c.G << 8) | (c.R << 16) | (c.A << 24);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static Color ColorFromArgbDword(uint color)
        {
            return Color.FromArgb((byte) ((color & 0xFF000000) >> 24), (byte) ((color & 0x00FF0000) >> 16), (byte) ((color & 0x0000FF00) >> 8),
                (byte) ((color & 0x000000FF) >> 0));
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool AreImageSourcesEqual(ImageSource left, ImageSource right)
        {
            if(null == left)
            {
                return right == null;
            }
            if(null == right)
            {
                return false;
            }
            var leftBmp = GenerateBitmapSource(left);
            var rightBmp = GenerateBitmapSource(right);
            var leftPixels = GetBytesFromBitmapSource(leftBmp);
            var rightPixels = GetBytesFromBitmapSource(rightBmp);
            if(leftPixels.Length != rightPixels.Length)
            {
                return false;
            }
            return MemCmp(leftPixels, rightPixels, leftPixels.Length);
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static IntPtr GenerateHICON(ImageSource image, Size dimensions)
        {
            if(image == null)
            {
                return IntPtr.Zero;
            }

            var bf = image as BitmapFrame;
            if(bf != null)
            {
                bf = GetBestMatch(bf.Decoder.Frames, (int) dimensions.Width, (int) dimensions.Height);
            }
            else
            {
                var drawingDimensions = new Rect(0, 0, dimensions.Width, dimensions.Height);

                var renderRatio = dimensions.Width / dimensions.Height;
                var aspectRatio = image.Width / image.Height;

                if(image.Width <= dimensions.Width && image.Height <= dimensions.Height)
                {
                    drawingDimensions = new Rect((dimensions.Width - image.Width) / 2, (dimensions.Height - image.Height) / 2, image.Width, image.Height);
                }
                else if(renderRatio > aspectRatio)
                {
                    var scaledRenderWidth = (image.Width / image.Height) * dimensions.Width;
                    drawingDimensions = new Rect((dimensions.Width - scaledRenderWidth) / 2, 0, scaledRenderWidth, dimensions.Height);
                }
                else if(renderRatio < aspectRatio)
                {
                    var scaledRenderHeight = (image.Height / image.Width) * dimensions.Height;
                    drawingDimensions = new Rect(0, (dimensions.Height - scaledRenderHeight) / 2, dimensions.Width, scaledRenderHeight);
                }
                var dv = new DrawingVisual();
                var dc = dv.RenderOpen();
                dc.DrawImage(image, drawingDimensions);
                dc.Close();
                var bmp = new RenderTargetBitmap((int) dimensions.Width, (int) dimensions.Height, 96, 96, PixelFormats.Pbgra32);
                bmp.Render(dv);
                bf = BitmapFrame.Create(bmp);
            }

            using(var memstm = new MemoryStream())
            {
                BitmapEncoder enc = new PngBitmapEncoder();
                enc.Frames.Add(bf);
                enc.Save(memstm);
                using(var istm = new ManagedIStream(memstm))
                {
                    var bitmap = IntPtr.Zero;
                    try
                    {
                        var gpStatus = NativeMethods.GdipCreateBitmapFromStream(istm, out bitmap);
                        if(Status.Ok != gpStatus)
                        {
                            return IntPtr.Zero;
                        }
                        IntPtr hicon;
                        gpStatus = NativeMethods.GdipCreateHICONFromBitmap(bitmap, out hicon);
                        if(Status.Ok != gpStatus)
                        {
                            return IntPtr.Zero;
                        }

                        return hicon;
                    }
                    finally
                    {
                        SafeDisposeImage(ref bitmap);
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void AddDependencyPropertyChangeListener(object component, DependencyProperty property, EventHandler listener)
        {
            if(component == null)
            {
                return;
            }
            Assert.IsNotNull(property);
            Assert.IsNotNull(listener);
            var dpd = DependencyPropertyDescriptor.FromProperty(property, component.GetType());
            dpd.AddValueChanged(component, listener);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void RemoveDependencyPropertyChangeListener(object component, DependencyProperty property, EventHandler listener)
        {
            if(component == null)
            {
                return;
            }
            Assert.IsNotNull(property);
            Assert.IsNotNull(listener);
            var dpd = DependencyPropertyDescriptor.FromProperty(property, component.GetType());
            dpd.RemoveValueChanged(component, listener);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool IsNonNegative(this Thickness thickness)
        {
            if(!thickness.Top.IsFiniteAndNonNegative())
            {
                return false;
            }
            if(!thickness.Left.IsFiniteAndNonNegative())
            {
                return false;
            }
            if(!thickness.Bottom.IsFiniteAndNonNegative())
            {
                return false;
            }
            if(!thickness.Right.IsFiniteAndNonNegative())
            {
                return false;
            }
            return true;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool IsValid(this CornerRadius cornerRadius)
        {
            if(!cornerRadius.TopLeft.IsFiniteAndNonNegative())
            {
                return false;
            }
            if(!cornerRadius.TopRight.IsFiniteAndNonNegative())
            {
                return false;
            }
            if(!cornerRadius.BottomLeft.IsFiniteAndNonNegative())
            {
                return false;
            }
            if(!cornerRadius.BottomRight.IsFiniteAndNonNegative())
            {
                return false;
            }
            return true;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void SafeDestroyIcon(ref IntPtr hicon)
        {
            var p = hicon;
            hicon = IntPtr.Zero;
            if(IntPtr.Zero != p)
            {
                NativeMethods.DestroyIcon(p);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void SafeDeleteObject(ref IntPtr gdiObject)
        {
            var p = gdiObject;
            gdiObject = IntPtr.Zero;
            if(IntPtr.Zero != p)
            {
                NativeMethods.DeleteObject(p);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void SafeDestroyWindow(ref IntPtr hwnd)
        {
            var p = hwnd;
            hwnd = IntPtr.Zero;
            if(NativeMethods.IsWindow(p))
            {
                NativeMethods.DestroyWindow(p);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void SafeDisposeImage(ref IntPtr gdipImage)
        {
            var p = gdipImage;
            gdipImage = IntPtr.Zero;
            if(IntPtr.Zero != p)
            {
                NativeMethods.GdipDisposeImage(p);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public static void SafeCoTaskMemFree(ref IntPtr ptr)
        {
            var p = ptr;
            ptr = IntPtr.Zero;
            if(IntPtr.Zero != p)
            {
                Marshal.FreeCoTaskMem(p);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public static void SafeFreeHGlobal(ref IntPtr hglobal)
        {
            var p = hglobal;
            hglobal = IntPtr.Zero;
            if(IntPtr.Zero != p)
            {
                Marshal.FreeHGlobal(p);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public static void SafeRelease<T>(ref T comObject) where T : class
        {
            var t = comObject;
            comObject = default(T);
            if(null != t)
            {
                Assert.IsTrue(Marshal.IsComObject(t));
                Marshal.ReleaseComObject(t);
            }
        }

        private class _UrlDecoder
        {
            private readonly byte[] _byteBuffer;
            private readonly char[] _charBuffer;
            private readonly Encoding _encoding;
            private int _byteCount;
            private int _charCount;

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public _UrlDecoder(int size, Encoding encoding)
            {
                _encoding = encoding;
                _charBuffer = new char[size];
                _byteBuffer = new byte[size];
            }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public void AddByte(byte b)
            {
                _byteBuffer[_byteCount++] = b;
            }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public void AddChar(char ch)
            {
                _FlushBytes();
                _charBuffer[_charCount++] = ch;
            }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            private void _FlushBytes()
            {
                if(_byteCount > 0)
                {
                    _charCount += _encoding.GetChars(_byteBuffer, 0, _byteCount, _charBuffer, _charCount);
                    _byteCount = 0;
                }
            }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public string GetString()
            {
                _FlushBytes();
                if(_charCount > 0)
                {
                    return new string(_charBuffer, 0, _charCount);
                }
                return "";
            }
        }
    }
}