namespace com.etsoo.EasyPdf.Font
{
    /// <summary>
    /// The components of table 'head'
    /// This table gives global information about the font
    /// https://docs.microsoft.com/en-us/typography/opentype/spec/head
    /// </summary>
    internal class HeadTable
    {
        internal int Flags { get; set; }
        internal int UnitsPerEm { get; set; }
        internal short XMin { get; set; }
        internal short YMin { get; set; }
        internal short XMax { get; set; }
        internal short YMax { get; set; }
        internal int MacStyle { get; set; }

        // 0 for short offsets (Offset16), 1 for long (Offset32)
        internal short IndexToLocFormat { get; set; }

        internal bool LocaShortVersion => IndexToLocFormat == 0;
    }
}
