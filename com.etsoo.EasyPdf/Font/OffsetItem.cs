namespace com.etsoo.EasyPdf.Font
{
    /// <summary>
    /// Offset item
    /// </summary>
    internal class OffsetItem
    {
        internal uint Checksum { get; set; }

        internal uint Offset { get; set; }

        internal uint Length { get; set; }

        internal byte[]? Data { get; set; }

        internal OffsetItem(uint checksum, uint offset, uint length)
        {
            Checksum = checksum;
            Offset = offset;
            Length = length;
        }
    }
}
