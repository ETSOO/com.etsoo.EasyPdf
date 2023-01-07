namespace com.etsoo.EasyPdf.Font
{
    internal enum FontNameId : byte
    {
        CopyrightNotice = 0,
        FamilyName = 1,

        //  The name of the style. Bold
        SubfamilyName = 2,

        UniqueIdentifier = 3,
        FullName = 4,
        VersionString = 5,
        PostScriptName = 6,
        Trademark = 7,
        ManufacturerName = 8,
        Designer = 9,
        Description = 10,
        URLVendor = 11,
        URLDesigner = 12,
        LicenseDescription = 13,
        URLLicense = 14,
        Reserved = 15,

        // If name ID 16 is absent, then name ID 1 is considered to be the typographic family name
        PreferredFamilyName = 16,

        // If it is absent, then name ID 2 is considered to be the typographic subfamily name
        PreferredSubfamily = 17,

        // Macintosh only
        CompatibleFullName = 18,

        SampleText = 19,
        PostScriptCID = 20,
        WWSFamilyName = 21,
        WWSSubfamilyName = 22,
        LightBackgroundPalette = 23,
        DarkBackgroundPalette = 24,
        VariationsPostScriptNamePrefix = 25
    }

    internal class FontName
    {
        internal FontNamePlatform Platform { get; set; }
        internal int EncodingID { get; set; }
        internal int LanguageID { get; set; }
        internal FontNameId NameId { get; set; }
        internal string Name { get; set; } = default!;
    }
}
