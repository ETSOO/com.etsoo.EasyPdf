namespace com.etsoo.EasyPdf.Support
{
    internal interface IPdfRandomAccessSource
    {
        int Get(long position);

        int Get(long position, byte[] bytes, int off, int len);

        long Length { get; }

        void Close();

        void Dispose();
    }

    internal class PdfArrayRandomAccessSource : IPdfRandomAccessSource
    {
        private byte[] array;

        public PdfArrayRandomAccessSource(byte[] array)
        {
            this.array = array;
        }

        public int Get(long offset)
        {
            if (offset >= array.Length) return -1;
            return 0xff & array[(int)offset];
        }

        public int Get(long offset, byte[] bytes, int off, int len)
        {
            if (offset >= array.Length)
                return -1;

            if (offset + len > array.Length)
                len = (int)(array.Length - offset);

            Array.Copy(array, (int)offset, bytes, off, len);

            return len;
        }

        public long Length
        {
            get
            {
                return array.Length;
            }
        }

        public void Close()
        {
            array = [];
        }

        public void Dispose()
        {
            Close();
        }
    }

    internal class PdfGroupedRandomAccessSource : IPdfRandomAccessSource
    {
        private readonly SourceEntry[] sources;

        private SourceEntry currentSourceEntry;

        private readonly long size;

        public PdfGroupedRandomAccessSource(ICollection<IPdfRandomAccessSource> sources)
        {
            this.sources = new SourceEntry[sources.Count];

            var totalSize = 0L;
            var i = 0;
            foreach (var ras in sources)
            {
                this.sources[i] = new SourceEntry(i++, ras, totalSize);
                totalSize += ras.Length;
            }
            size = totalSize;
            currentSourceEntry = this.sources[sources.Count-1];
            SourceInUse(currentSourceEntry.source);
        }

        protected internal virtual int GetStartingSourceIndex(long offset)
        {
            if (offset >= currentSourceEntry.firstByte)
                return currentSourceEntry.index;

            return 0;
        }

        private SourceEntry? GetSourceEntryForOffset(long offset)
        {
            if (offset >= size)
                return null;

            if (offset >= currentSourceEntry.firstByte && offset <= currentSourceEntry.lastByte)
                return currentSourceEntry;

            SourceReleased(currentSourceEntry.source);

            var startAt = GetStartingSourceIndex(offset);

            for (var i = startAt; i < sources.Length; i++)
            {
                if (offset >= sources[i].firstByte && offset <= sources[i].lastByte)
                {
                    currentSourceEntry = sources[i];
                    SourceInUse(currentSourceEntry.source);
                    return currentSourceEntry;
                }
            }

            return null;
        }

        protected internal virtual void SourceReleased(IPdfRandomAccessSource source)
        {
        }

        protected internal virtual void SourceInUse(IPdfRandomAccessSource source)
        {
        }

        public int Get(long position)
        {
            var entry = GetSourceEntryForOffset(position);

            if (entry == null)
                return -1;

            return entry.source.Get(entry.OffsetN(position));

        }

        public int Get(long position, byte[] bytes, int off, int len)
        {
            var entry = GetSourceEntryForOffset(position);

            if (entry == null)
                return -1;

            var offN = entry.OffsetN(position);

            var remaining = len;

            while (remaining > 0)
            {
                if (entry == null)
                    break;

                if (offN > entry.source.Length)
                    break;

                var count = entry.source.Get(offN, bytes, off, remaining);
                if (count == -1)
                    break;

                off += count;
                position += count;
                remaining -= count;

                offN = 0;
                entry = GetSourceEntryForOffset(position);
            }

            return remaining == len ? -1 : len - remaining;
        }

        public long Length
        {
            get
            {
                return size;
            }
        }

        public void Close()
        {
            foreach (var entry in sources)
            {
                entry.source.Close();
            }
        }

        public void Dispose()
        {
            Close();
        }

        private sealed class SourceEntry
        {
            internal readonly IPdfRandomAccessSource source;

            internal readonly long firstByte;

            internal readonly long lastByte;

            internal readonly int index;

            internal SourceEntry(int index, IPdfRandomAccessSource source, long offset)
            {
                this.index = index;
                this.source = source;
                firstByte = offset;
                lastByte = offset + source.Length - 1;
            }

            internal long OffsetN(long absoluteOffset)
            {
                return absoluteOffset - firstByte;
            }
        }
    }

    internal class PdfRAFRandomAccessSource : IPdfRandomAccessSource
    {
        private readonly Stream raf;

        private readonly long length;

        public PdfRAFRandomAccessSource(Stream raf)
        {
            this.raf = raf;
            length = raf.Length;
        }

        public int Get(long position)
        {
            if (position > length)
                return -1;

            if (raf.Position != position)
                raf.Seek(position, SeekOrigin.Begin);

            return raf.ReadByte();
        }

        public int Get(long position, byte[] bytes, int off, int len)
        {
            if (position > length)
                return -1;

            if (raf.Position != position)
                raf.Seek(position, SeekOrigin.Begin);

            int n = raf.Read(bytes, off, len);
            return n == 0 ? -1 : n;
        }

        public long Length
        {
            get
            {
                return length;
            }
        }

        public void Close()
        {
            raf.Close();
        }

        public void Dispose()
        {
            Close();
        }
    }

    internal class PdfWindowRandomAccessSource : IPdfRandomAccessSource
    {
        private readonly IPdfRandomAccessSource source;

        private readonly long offset;

        private readonly long length;

        public PdfWindowRandomAccessSource(IPdfRandomAccessSource source, long offset) : this(source, offset, source.Length - offset)
        {
        }

        public PdfWindowRandomAccessSource(IPdfRandomAccessSource source, long offset, long length)
        {
            this.source = source;
            this.offset = offset;
            this.length = length;
        }

        public int Get(long position)
        {
            if (position >= length) return -1;
            return source.Get(offset + position);
        }

        public int Get(long position, byte[] bytes, int off, int len)
        {
            if (position >= length)
                return -1;

            long toRead = Math.Min(len, length - position);
            return source.Get(offset + position, bytes, off, (int)toRead);
        }

        public long Length
        {
            get
            {
                return length;
            }
        }

        public void Close()
        {
            source.Close();
        }

        public void Dispose()
        {
            Close();
        }
    }

    internal class PdfIndependentRandomAccessSource : IPdfRandomAccessSource
    {
        private readonly IPdfRandomAccessSource source;

        public PdfIndependentRandomAccessSource(IPdfRandomAccessSource source)
        {
            this.source = source;
        }

        public int Get(long position)
        {
            return source.Get(position);
        }

        public int Get(long position, byte[] bytes, int off, int len)
        {
            return source.Get(position, bytes, off, len);
        }

        public long Length
        {
            get
            {
                return source.Length;
            }
        }

        public void Close()
        {
        }

        public void Dispose()
        {
            Close();
        }
    }
}
