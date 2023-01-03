using System;
using System.IO;
using System.Text;

namespace com.etsoo.EasyPdf.IO
{
    internal class PdfRandomAccessor : IDisposable
    {
        private static Encoding? encoding1252;
        internal static Encoding Encoding1252
        {
            get
            {
                if (encoding1252 == null)
                {
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    encoding1252 = Encoding.GetEncoding(1252);
                }
                return encoding1252;
            }
        }

        private readonly IPdfRandomAccessSource byteSource;

        private long byteSourcePosition;

        private byte back;

        private bool isBack = false;

        public PdfRandomAccessor(IPdfRandomAccessSource byteSource)
        {
            this.byteSource = byteSource;
        }

        public PdfRandomAccessor(Stream inp)
            : this(PdfRandomAccessSourceFactory.CreateSource(inp))
        {
        }

        protected internal IPdfRandomAccessSource GetByteSource()
        {
            return byteSource;
        }

        public void PushBack(byte b)
        {
            back = b;
            isBack = true;
        }

        public int Read()
        {
            if (isBack)
            {
                isBack = false;
                return back & 0xff;
            }
            return byteSource.Get(byteSourcePosition++);
        }

        public int Read(byte[] b, int off, int len)
        {
            if (len == 0)
                return 0;

            var count = 0;
            if (isBack && len > 0)
            {
                isBack = false;
                b[off++] = back;
                --len;
                count++;
            }

            if (len > 0)
            {
                int byteSourceCount = byteSource.Get(byteSourcePosition, b, off, len);
                if (byteSourceCount > 0)
                {
                    count += byteSourceCount;
                    byteSourcePosition += byteSourceCount;
                }
            }

            if (count == 0)
                return -1;

            return count;
        }

        public int Read(byte[] b)
        {
            return Read(b, 0, b.Length);
        }

        public void ReadFully(byte[] b)
        {
            ReadFully(b, 0, b.Length);
        }

        public void ReadFully(byte[] b, int off, int len)
        {
            if (len == 0)
                return;
            var n = 0;
            do
            {
                int count = Read(b, off + n, len - n);
                if (count <= 0)
                    throw new EndOfStreamException();
                n += count;
            } while (n < len);
        }

        public long Skip(long n)
        {
            return SkipBytes(n);
        }

        public long SkipBytes(long n)
        {
            if (n <= 0)
            {
                return 0;
            }
            int adj = 0;
            if (isBack)
            {
                isBack = false;
                if (n == 1)
                {
                    return 1;
                }
                else
                {
                    --n;
                    adj = 1;
                }
            }
            long pos;
            long len;
            long newpos;

            pos = FilePointer;
            len = Length;
            newpos = pos + n;
            if (newpos > len)
            {
                newpos = len;
            }
            Seek(newpos);

            return newpos - pos + adj;
        }

        public void Close()
        {
            isBack = false;
            byteSource.Close();
        }

        public long Length
        {
            get { return byteSource.Length; }
        }

        public void Seek(long pos)
        {
            byteSourcePosition = pos;
            isBack = false;
        }

        public void Seek(int pos)
        {
            Seek((long)pos);
        }

        public long FilePointer
        {
            get { return byteSourcePosition - (isBack ? 1 : 0); }
        }

        public bool ReadBoolean()
        {
            int ch = Read();
            if (ch < 0)
                throw new EndOfStreamException();
            return (ch != 0);
        }

        public byte ReadByte()
        {
            int ch = Read();
            if (ch < 0)
                throw new EndOfStreamException();
            return (byte)(ch);
        }

        public int ReadUnsignedByte()
        {
            int ch = Read();
            if (ch < 0)
                throw new EndOfStreamException();
            return ch;
        }

        public short ReadShort()
        {
            int ch1 = Read();
            int ch2 = Read();
            if ((ch1 | ch2) < 0)
                throw new EndOfStreamException();
            return (short)((ch1 << 8) + ch2);
        }

        /**
        * Reads a signed 16-bit number from this stream in little-endian order.
        * The method reads two
        * bytes from this stream, starting at the current stream pointer.
        * If the two bytes read, in order, are
        * <code>b1</code> and <code>b2</code>, where each of the two values is
        * between <code>0</code> and <code>255</code>, inclusive, then the
        * result is equal to:
        * <blockquote><pre>
        *     (short)((b2 &lt;&lt; 8) | b1)
        * </pre></blockquote>
        * <p>
        * This method blocks until the two bytes are read, the end of the
        * stream is detected, or an exception is thrown.
        *
        * @return     the next two bytes of this stream, interpreted as a signed
        *             16-bit number.
        * @exception  EOFException  if this stream reaches the end before reading
        *               two bytes.
        * @exception  IOException   if an I/O error occurs.
        */

        public short ReadShortLE()
        {
            int ch1 = Read();
            int ch2 = Read();
            if ((ch1 | ch2) < 0)
                throw new EndOfStreamException();
            return (short)((ch2 << 8) + (ch1 << 0));
        }

        public ushort ReadUnsignedShort()
        {
            var ch1 = Read();
            var ch2 = Read();
            if ((ch1 | ch2) < 0)
                throw new EndOfStreamException();
            return (ushort)((ch1 << 8) + ch2);
        }

        /**
        * Reads an unsigned 16-bit number from this stream in little-endian order.
        * This method reads
        * two bytes from the stream, starting at the current stream pointer.
        * If the bytes read, in order, are
        * <code>b1</code> and <code>b2</code>, where
        * <code>0&nbsp;&lt;=&nbsp;b1, b2&nbsp;&lt;=&nbsp;255</code>,
        * then the result is equal to:
        * <blockquote><pre>
        *     (b2 &lt;&lt; 8) | b1
        * </pre></blockquote>
        * <p>
        * This method blocks until the two bytes are read, the end of the
        * stream is detected, or an exception is thrown.
        *
        * @return     the next two bytes of this stream, interpreted as an
        *             unsigned 16-bit integer.
        * @exception  EOFException  if this stream reaches the end before reading
        *               two bytes.
        * @exception  IOException   if an I/O error occurs.
        */

        public int ReadUnsignedShortLE()
        {
            int ch1 = Read();
            int ch2 = Read();
            if ((ch1 | ch2) < 0)
                throw new EndOfStreamException();
            return (ch2 << 8) + (ch1 << 0);
        }

        virtual public char ReadChar()
        {
            int ch1 = Read();
            int ch2 = Read();
            if ((ch1 | ch2) < 0)
                throw new EndOfStreamException();
            return (char)((ch1 << 8) + ch2);
        }

        /**
        * Reads a Unicode character from this stream in little-endian order.
        * This method reads two
        * bytes from the stream, starting at the current stream pointer.
        * If the bytes read, in order, are
        * <code>b1</code> and <code>b2</code>, where
        * <code>0&nbsp;&lt;=&nbsp;b1,&nbsp;b2&nbsp;&lt;=&nbsp;255</code>,
        * then the result is equal to:
        * <blockquote><pre>
        *     (char)((b2 &lt;&lt; 8) | b1)
        * </pre></blockquote>
        * <p>
        * This method blocks until the two bytes are read, the end of the
        * stream is detected, or an exception is thrown.
        *
        * @return     the next two bytes of this stream as a Unicode character.
        * @exception  EOFException  if this stream reaches the end before reading
        *               two bytes.
        * @exception  IOException   if an I/O error occurs.
        */

        public char ReadCharLE()
        {
            int ch1 = Read();
            int ch2 = Read();
            if ((ch1 | ch2) < 0)
                throw new EndOfStreamException();
            return (char)((ch2 << 8) + (ch1 << 0));
        }

        public int ReadInt()
        {
            int ch1 = Read();
            int ch2 = Read();
            int ch3 = Read();
            int ch4 = Read();
            if ((ch1 | ch2 | ch3 | ch4) < 0)
                throw new EndOfStreamException();
            return ((ch1 << 24) + (ch2 << 16) + (ch3 << 8) + ch4);
        }

        public uint ReadUInt()
        {
            var ch1 = Read();
            var ch2 = Read();
            var ch3 = Read();
            var ch4 = Read();
            if ((ch1 | ch2 | ch3 | ch4) < 0)
                throw new EndOfStreamException();
            return (uint)((ch1 << 24) + (ch2 << 16) + (ch3 << 8) + ch4);
        }

        /**
        * Reads a signed 32-bit integer from this stream in little-endian order.
        * This method reads 4
        * bytes from the stream, starting at the current stream pointer.
        * If the bytes read, in order, are <code>b1</code>,
        * <code>b2</code>, <code>b3</code>, and <code>b4</code>, where
        * <code>0&nbsp;&lt;=&nbsp;b1, b2, b3, b4&nbsp;&lt;=&nbsp;255</code>,
        * then the result is equal to:
        * <blockquote><pre>
        *     (b4 &lt;&lt; 24) | (b3 &lt;&lt; 16) + (b2 &lt;&lt; 8) + b1
        * </pre></blockquote>
        * <p>
        * This method blocks until the four bytes are read, the end of the
        * stream is detected, or an exception is thrown.
        *
        * @return     the next four bytes of this stream, interpreted as an
        *             <code>int</code>.
        * @exception  EOFException  if this stream reaches the end before reading
        *               four bytes.
        * @exception  IOException   if an I/O error occurs.
        */

        public int ReadIntLE()
        {
            int ch1 = Read();
            int ch2 = Read();
            int ch3 = Read();
            int ch4 = Read();
            if ((ch1 | ch2 | ch3 | ch4) < 0)
                throw new EndOfStreamException();
            return ((ch4 << 24) + (ch3 << 16) + (ch2 << 8) + (ch1 << 0));
        }

        /**
        * Reads an unsigned 32-bit integer from this stream. This method reads 4
        * bytes from the stream, starting at the current stream pointer.
        * If the bytes read, in order, are <code>b1</code>,
        * <code>b2</code>, <code>b3</code>, and <code>b4</code>, where
        * <code>0&nbsp;&lt;=&nbsp;b1, b2, b3, b4&nbsp;&lt;=&nbsp;255</code>,
        * then the result is equal to:
        * <blockquote><pre>
        *     (b1 &lt;&lt; 24) | (b2 &lt;&lt; 16) + (b3 &lt;&lt; 8) + b4
        * </pre></blockquote>
        * <p>
        * This method blocks until the four bytes are read, the end of the
        * stream is detected, or an exception is thrown.
        *
        * @return     the next four bytes of this stream, interpreted as a
        *             <code>long</code>.
        * @exception  EOFException  if this stream reaches the end before reading
        *               four bytes.
        * @exception  IOException   if an I/O error occurs.
        */

        public long ReadUnsignedInt()
        {
            long ch1 = Read();
            long ch2 = Read();
            long ch3 = Read();
            long ch4 = Read();
            if ((ch1 | ch2 | ch3 | ch4) < 0)
                throw new EndOfStreamException();
            return ((ch1 << 24) + (ch2 << 16) + (ch3 << 8) + (ch4 << 0));
        }

        public long ReadUnsignedIntLE()
        {
            long ch1 = Read();
            long ch2 = Read();
            long ch3 = Read();
            long ch4 = Read();
            if ((ch1 | ch2 | ch3 | ch4) < 0)
                throw new EndOfStreamException();
            return ((ch4 << 24) + (ch3 << 16) + (ch2 << 8) + (ch1 << 0));
        }

        public long ReadLong()
        {
            return ((long)(ReadInt()) << 32) + (ReadInt() & 0xFFFFFFFFL);
        }

        public long ReadLongLE()
        {
            int i1 = ReadIntLE();
            int i2 = ReadIntLE();
            return ((long)i2 << 32) + (i1 & 0xFFFFFFFFL);
        }

        public float ReadFloat()
        {
            int[] a = { ReadInt() };
            float[] b = { 0 };
            Buffer.BlockCopy(a, 0, b, 0, 4);
            return b[0];
        }

        public float ReadFloatLE()
        {
            int[] a = { ReadIntLE() };
            float[] b = { 0 };
            Buffer.BlockCopy(a, 0, b, 0, 4);
            return b[0];
        }

        public double ReadDouble()
        {
            long[] a = { ReadLong() };
            double[] b = { 0 };
            Buffer.BlockCopy(a, 0, b, 0, 8);
            return b[0];
        }

        public double ReadDoubleLE()
        {
            long[] a = { ReadLongLE() };
            double[] b = { 0 };
            Buffer.BlockCopy(a, 0, b, 0, 8);
            return b[0];
        }

        public string ReadLine()
        {
            var input = new StringBuilder();
            int c = -1;
            bool eol = false;

            while (!eol)
            {
                switch (c = Read())
                {
                    case -1:
                    case '\n':
                        eol = true;
                        break;
                    case '\r':
                        eol = true;
                        long cur = FilePointer;
                        if ((Read()) != '\n')
                        {
                            Seek(cur);
                        }
                        break;
                    default:
                        input.Append((char)c);
                        break;
                }
            }

            if ((c == -1) && (input.Length == 0))
                return string.Empty;
            return input.ToString();
        }

        public string ReadUnicodeString(int length)
        {
            var buf = new StringBuilder();
            length /= 2;
            for (int k = 0; k < length; ++k)
            {
                buf.Append(ReadChar());
            }
            return buf.ToString();
        }

        public string ReadString(int length)
        {
            var buf = new byte[length];
            ReadFully(buf);
            return Encoding1252.GetString(buf);
        }

        public void Dispose()
        {
            byteSource.Dispose();
        }
    }
}
