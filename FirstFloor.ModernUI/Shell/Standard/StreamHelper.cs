// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

using STATSTG = System.Runtime.InteropServices.ComTypes.STATSTG;

namespace FirstFloor.ModernUI.Shell.Standard
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal sealed class ComStream : Stream
    {
        private const int STATFLAG_NONAME = 1;
        private IStream _source;

        public ComStream(ref IStream stream)
        {
            Verify.IsNotNull(stream, "stream");
            _source = stream;

            stream = null;
        }

        #region Overridden Stream Methods

        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return true; } }

        public override bool CanWrite
        {
            get
            {
#if FEATURE_MUTABLE_COM_STREAMS
    
                return true;
#endif
                return false;
            }
        }

        public override long Length
        {
            get
            {
                _Validate();
                STATSTG statstg;
                _source.Stat(out statstg, STATFLAG_NONAME);
                return statstg.cbSize;
            }
        }

        public override long Position { get { return Seek(0, SeekOrigin.Current); } set { Seek(value, SeekOrigin.Begin); } }

        public override void Close()
        {
            if(null != _source)
            {
#if FEATURE_MUTABLE_COM_STREAMS
                Flush();
#endif
                Utility.SafeRelease(ref _source);
            }
        }

        public override void Flush()
        {
#if FEATURE_MUTABLE_COM_STREAMS
            _Validate();
            
            try
            {
                _source.Commit(STGC_DEFAULT);
            }
            catch { }
#endif
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            _Validate();
            var pcbRead = IntPtr.Zero;
            try
            {
                pcbRead = Marshal.AllocHGlobal(sizeof(Int32));

                var tempBuffer = new byte[count];
                _source.Read(tempBuffer, count, pcbRead);
                Array.Copy(tempBuffer, 0, buffer, offset, Marshal.ReadInt32(pcbRead));
                return Marshal.ReadInt32(pcbRead);
            }
            finally
            {
                Utility.SafeFreeHGlobal(ref pcbRead);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            _Validate();
            var plibNewPosition = IntPtr.Zero;
            try
            {
                plibNewPosition = Marshal.AllocHGlobal(sizeof(Int64));
                _source.Seek(offset, (int) origin, plibNewPosition);
                return Marshal.ReadInt64(plibNewPosition);
            }
            finally
            {
                Utility.SafeFreeHGlobal(ref plibNewPosition);
            }
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
#if FEATURE_MUTABLE_COM_STREAMS
            _Validate();
            _source.SetSize(value);
#endif
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
#if FEATURE_MUTABLE_COM_STREAMS
            _Validate();
            
            byte[] tempBuffer = new byte[buffer.Length - offset];
            Array.Copy(buffer, offset, tempBuffer, 0, tempBuffer.Length);
            _source.Write(tempBuffer, tempBuffer.Length, IntPtr.Zero);
#endif
        }

        #endregion

        private void _Validate()
        {
            if(null == _source)
            {
                throw new ObjectDisposedException("this");
            }
        }
    }

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal sealed class ManagedIStream : IStream, IDisposable
    {
        private const int STGTY_STREAM = 2;
        private const int STGM_READWRITE = 2;
        private const int LOCK_EXCLUSIVE = 2;
        private Stream _source;

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public ManagedIStream(Stream source)
        {
            Verify.IsNotNull(source, "source");
            _source = source;
        }

        private void _Validate()
        {
            if(null == _source)
            {
                throw new ObjectDisposedException("this");
            }
        }

        #region IStream Members

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "Standard.HRESULT.ThrowIfFailed(System.String)")]
        [Obsolete("The method is not implemented", true)]
        public void Clone(out IStream ppstm)
        {
            ppstm = null;
            HRESULT.STG_E_INVALIDFUNCTION.ThrowIfFailed("The method is not implemented.");
        }

        public void Commit(int grfCommitFlags)
        {
            _Validate();
            _source.Flush();
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public void CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
        {
            Verify.IsNotNull(pstm, "pstm");
            _Validate();

            var buffer = new byte[4096];
            long cbWritten = 0;
            while(cbWritten < cb)
            {
                var cbRead = _source.Read(buffer, 0, buffer.Length);
                if(0 == cbRead)
                {
                    break;
                }

                pstm.Write(buffer, cbRead, IntPtr.Zero);
                cbWritten += cbRead;
            }
            if(IntPtr.Zero != pcbRead)
            {
                Marshal.WriteInt64(pcbRead, cbWritten);
            }
            if(IntPtr.Zero != pcbWritten)
            {
                Marshal.WriteInt64(pcbWritten, cbWritten);
            }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "Standard.HRESULT.ThrowIfFailed(System.String)")]
        [Obsolete("The method is not implemented", true)]
        public void LockRegion(long libOffset, long cb, int dwLockType)
        {
            HRESULT.STG_E_INVALIDFUNCTION.ThrowIfFailed("The method is not implemented.");
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public void Read(byte[] pv, int cb, IntPtr pcbRead)
        {
            _Validate();
            var cbRead = _source.Read(pv, 0, cb);
            if(IntPtr.Zero != pcbRead)
            {
                Marshal.WriteInt32(pcbRead, cbRead);
            }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "Standard.HRESULT.ThrowIfFailed(System.String)")]
        [Obsolete("The method is not implemented", true)]
        public void Revert()
        {
            HRESULT.STG_E_INVALIDFUNCTION.ThrowIfFailed("The method is not implemented.");
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
        {
            _Validate();
            var position = _source.Seek(dlibMove, (SeekOrigin) dwOrigin);
            if(IntPtr.Zero != plibNewPosition)
            {
                Marshal.WriteInt64(plibNewPosition, position);
            }
        }

        public void SetSize(long libNewSize)
        {
            _Validate();
            _source.SetLength(libNewSize);
        }

        public void Stat(out STATSTG pstatstg, int grfStatFlag)
        {
            pstatstg = default(STATSTG);
            _Validate();
            pstatstg.type = STGTY_STREAM;
            pstatstg.cbSize = _source.Length;
            pstatstg.grfMode = STGM_READWRITE;
            pstatstg.grfLocksSupported = LOCK_EXCLUSIVE;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "Standard.HRESULT.ThrowIfFailed(System.String)")]
        [Obsolete("The method is not implemented", true)]
        public void UnlockRegion(long libOffset, long cb, int dwLockType)
        {
            HRESULT.STG_E_INVALIDFUNCTION.ThrowIfFailed("The method is not implemented.");
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public void Write(byte[] pv, int cb, IntPtr pcbWritten)
        {
            _Validate();
            _source.Write(pv, 0, cb);
            if(IntPtr.Zero != pcbWritten)
            {
                Marshal.WriteInt32(pcbWritten, cb);
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _source = null;
        }

        #endregion
    }

#if CONSIDER_ADDING
    
    
    
    internal class ReadonlyStream : Stream
    {
        private Stream _stream;
        public ReadonlyStream(Stream source)
        {
            Verify.IsNotNull(source, "source");
            _stream = source;
        }
        public override bool CanRead
        {
            get
            {
                return _stream.CanRead;
            }
        }
        public override bool CanSeek
        {
            get
            {
                return _stream.CanSeek;
            }
        }
        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }
        public override void Flush() { }
        public override long Length
        {
            get
            {
                return _stream.Length;
            }
        }
        public override long Position
        {
            get
            {
                return _stream.Position;
            }
            set
            {
                _stream.Position = value;
            }
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }
        public override void SetLength(long value)
        {
            throw new NotSupportedException("The stream doesn't support modifications.");
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("The stream doesn't support modifications.");
        }
        public override void Close()
        {
            base.Close();
        }
    }
    
    
    
    internal class StringStream : Stream
    {
        private string _source;
        private int _position;
        public StringStream(string source)
        {
            _source = source;
            _position = 0;
        }
        public override bool CanRead
        {
            get { return true; }
        }
        public override bool CanSeek
        {
            get { return true; }
        }
        public override bool CanWrite
        {
            get { return false; }
        }
        public override void Flush()
        {
            throw new NotSupportedException();
        }
        public override long Length
        {
            get { return _source.Length * 2; }
        }
        public override long Position
        {
            get
            {
                return _position;
            }
            set
            {
                Validate.BoundedInteger(0, (int)value, (int)Length + 1, "value");
                _position = (int)value;
            }
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            int cbRead = 0;
            for (; cbRead < count; ++cbRead)
            {
                if (Length <= Position)
                {
                    break;
                }
                buffer[offset + cbRead] = (byte)(0xFF & (_source[(int)Position / 2] >> ((0 == Position % 2) ? 0 : 8)));
                ++Position;
            }
            return cbRead;
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = Length + offset;
                    break;
                default:
                    throw new FormatException("Bad value for origin");
            }
            return Position;
        }
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
#endif
}