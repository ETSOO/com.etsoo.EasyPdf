using Microsoft.IO;
using System.Text;

namespace com.etsoo.EasyPdf
{
    /// <summary>
    /// PDF constants
    /// PDF 常量
    /// </summary>
    internal static class PdfConstants
    {
        public const char Null = '\0';

        /// <summary>
        /// '\0'
        /// </summary>
        public const byte NullByte = 0;

        public const char HorizontalTab = '\t';

        /// <summary>
        /// '\t'
        /// </summary>
        public const byte HorizontalTabByte = 9;

        public const char FormFeed = '\f';

        /// <summary>
        /// '\f'
        /// </summary>
        public const byte FormFeedByte = 12;

        public const char CarriageReturn = '\r';

        /// <summary>
        /// '\r'
        /// </summary>
        public const byte CarriageReturnByte = 13;

        public const char LineFeed = '\n';

        /// <summary>
        /// '\n'
        /// </summary>
        public const byte LineFeedByte = 10;

        public const char Space = ' ';

        /// <summary>
        /// ' '
        /// </summary>
        public const byte SpaceByte = 32;

        public const char NumberSign = '#';

        /// <summary>
        /// '#'
        /// </summary>
        public const byte NumberSignByte = 35;

        public const char LeftParenthesis = '(';

        /// <summary>
        /// '('
        /// </summary>
        public const byte LeftParenthesisByte = 40;

        public const char RightParenthesis = ')';

        /// <summary>
        /// ')'
        /// </summary>
        public const byte RightParenthesisByte = 41;

        public const char LessThanSign = '<';

        /// <summary>
        /// '<'
        /// </summary>
        public const byte LessThanSignByte = 60;

        public const char GreaterThanSign = '>';

        /// <summary>
        /// '>'
        /// </summary>
        public const byte GreaterThanSignByte = 62;

        public const char LeftSquareBracket = '[';

        /// <summary>
        /// '['
        /// </summary>
        public const byte LeftSquareBracketByte = 91;

        public const char RightSquareBracket = ']';

        /// <summary>
        /// ']'
        /// </summary>
        public const byte RightSquareBracketByte = 93;

        public const char LeftCurlyBracket = '{';

        /// <summary>
        /// '{'
        /// </summary>
        public const byte LeftCurlyBracketByte = 123;

        public const char RightCurlyBracket = '}';

        /// <summary>
        /// '}'
        /// </summary>
        public const byte RightCurlyBracketByte = 125;

        public const char PercentSign = '%';

        /// <summary>
        /// '%'
        /// </summary>
        public const byte PercentSignByte = 37;

        public const char Solidus = '/';

        /// <summary>
        /// '/'
        /// </summary>
        public const byte SolidusByte = 47;

        public const char ReverseSolidus = '\\';

        /// <summary>
        /// '\'
        /// </summary>
        public const byte ReverseSolidusByte = 92;

        public const char Reference = 'R';

        /// <summary>
        /// 'R'
        /// </summary>
        public const byte ReferenceByte = 82;

        /// <summary>
        /// Dictionary start bytes
        /// </summary>
        public static readonly byte[] DictionaryStartBytes = new[] { LessThanSignByte, LessThanSignByte };

        /// <summary>
        /// Dictionary end bytes
        /// </summary>
        public static readonly byte[] DictionaryEndBytes = new[] { GreaterThanSignByte, GreaterThanSignByte };

        /// <summary>
        /// White-space characters
        /// All white-space characters are equivalent, except in comments, strings and streams.
        /// In all other contexts, PDF treats any sequence of consecutive white-space characters as one character.
        /// 空白字符
        /// </summary>
        public static readonly byte[] WhiteSpaceCharacters = new[] { NullByte, HorizontalTabByte, LineFeedByte, FormFeedByte, CarriageReturnByte, SpaceByte };

        /// <summary>
        /// Delimiter start characters
        /// 分隔开始字符
        /// </summary>
        public static readonly byte[] DelimiterStartCharacters = new[] { LeftParenthesisByte, LessThanSignByte, LeftSquareBracketByte, LeftCurlyBracketByte, SolidusByte, PercentSignByte };

        /// <summary>
        /// Delimiter characters
        /// 分隔字符
        /// </summary>
        public static readonly byte[] DelimiterCharacters = DelimiterStartCharacters.Concat(new[] { RightParenthesisByte, GreaterThanSignByte, RightSquareBracketByte, RightCurlyBracketByte }).ToArray();

        /// <summary>
        /// Delimiter start characters with space
        /// 分隔开始字符+空格
        /// </summary>
        public static readonly byte[] DelimiterStartCharactersWithSpace = DelimiterStartCharacters.Prepend(SpaceByte).ToArray();

        /// <summary>
        /// Delimiter start characters with all space characters
        /// 分隔开始字符+所有空格字符
        /// </summary>
        public static readonly byte[] DelimiterStartCharactersWithAllSpaces = DelimiterStartCharacters.Concat(WhiteSpaceCharacters).ToArray();

        /// <summary>
        /// startxref bytes
        /// startxref 字节
        /// </summary>
        public static readonly byte[] StartXRefBytes = Encoding.ASCII.GetBytes("startxref");

        /// <summary>
        /// xref bytes
        /// xref 字节
        /// </summary>
        public static readonly byte[] XrefBytes = Encoding.ASCII.GetBytes("xref");

        /// <summary>
        /// trailer bytes
        /// 尾部标识字节
        /// </summary>
        public static readonly byte[] TrailerBytes = Encoding.ASCII.GetBytes("trailer");

        /// <summary>
        /// Stream manager
        /// 流管理器
        /// </summary>
        public static readonly RecyclableMemoryStreamManager StreamManager = new();
    }
}
