//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.6.6
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from C:\Users\Win10\source\repos\BNFCTest\ANTLR Test\Spreadsheet.g4 by ANTLR 4.6.6

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace ANTLR_Test {
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.6.6")]
[System.CLSCompliant(false)]
public partial class SpreadsheetLexer : Lexer {
	public const int
		T__0=1, T__1=2, T__2=3, T__3=4, T__4=5, T__5=6, T__6=7, T__7=8, T__8=9, 
		T__9=10, T__10=11, T__11=12, T__12=13, T__13=14, T__14=15, T__15=16, T__16=17, 
		T__17=18, T__18=19, T__19=20, T__20=21, T__21=22, T__22=23, WS=24, COMMENT=25, 
		EMPTY=26, CHAR=27, NAME=28, INT=29, DECIMAL=30, STRING=31;
	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] ruleNames = {
		"T__0", "T__1", "T__2", "T__3", "T__4", "T__5", "T__6", "T__7", "T__8", 
		"T__9", "T__10", "T__11", "T__12", "T__13", "T__14", "T__15", "T__16", 
		"T__17", "T__18", "T__19", "T__20", "T__21", "T__22", "WS", "COMMENT", 
		"EMPTY", "CHAR", "NAME", "INT", "DECIMAL", "STRING", "EscapeSequence", 
		"NUMBER"
	};


	public SpreadsheetLexer(ICharStream input)
		: base(input)
	{
		_interp = new LexerATNSimulator(this,_ATN);
	}

	private static readonly string[] _LiteralNames = {
		null, "';'", "'C['", "','", "']'", "'='", "'\"'", "'*'", "'/'", "'%'", 
		"'+'", "'-'", "'<<'", "'>>'", "'<'", "'>'", "'<='", "'>='", "'=='", "'!='", 
		"'&&'", "'||'", "'true'", "'false'", "' '", null, "'-/-'"
	};
	private static readonly string[] _SymbolicNames = {
		null, null, null, null, null, null, null, null, null, null, null, null, 
		null, null, null, null, null, null, null, null, null, null, null, null, 
		"WS", "COMMENT", "EMPTY", "CHAR", "NAME", "INT", "DECIMAL", "STRING"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[System.Obsolete("Use Vocabulary instead.")]
	public static readonly string[] tokenNames = GenerateTokenNames(DefaultVocabulary, _SymbolicNames.Length);

	private static string[] GenerateTokenNames(IVocabulary vocabulary, int length) {
		string[] tokenNames = new string[length];
		for (int i = 0; i < tokenNames.Length; i++) {
			tokenNames[i] = vocabulary.GetLiteralName(i);
			if (tokenNames[i] == null) {
				tokenNames[i] = vocabulary.GetSymbolicName(i);
			}

			if (tokenNames[i] == null) {
				tokenNames[i] = "<INVALID>";
			}
		}

		return tokenNames;
	}

	[System.Obsolete("Use IRecognizer.Vocabulary instead.")]
	public override string[] TokenNames
	{
		get
		{
			return tokenNames;
		}
	}

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "Spreadsheet.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override string SerializedAtn { get { return _serializedATN; } }

	public static readonly string _serializedATN =
		"\x3\xAF6F\x8320\x479D\xB75C\x4880\x1605\x191C\xAB37\x2!\xDC\b\x1\x4\x2"+
		"\t\x2\x4\x3\t\x3\x4\x4\t\x4\x4\x5\t\x5\x4\x6\t\x6\x4\a\t\a\x4\b\t\b\x4"+
		"\t\t\t\x4\n\t\n\x4\v\t\v\x4\f\t\f\x4\r\t\r\x4\xE\t\xE\x4\xF\t\xF\x4\x10"+
		"\t\x10\x4\x11\t\x11\x4\x12\t\x12\x4\x13\t\x13\x4\x14\t\x14\x4\x15\t\x15"+
		"\x4\x16\t\x16\x4\x17\t\x17\x4\x18\t\x18\x4\x19\t\x19\x4\x1A\t\x1A\x4\x1B"+
		"\t\x1B\x4\x1C\t\x1C\x4\x1D\t\x1D\x4\x1E\t\x1E\x4\x1F\t\x1F\x4 \t \x4!"+
		"\t!\x4\"\t\"\x3\x2\x3\x2\x3\x3\x3\x3\x3\x3\x3\x4\x3\x4\x3\x5\x3\x5\x3"+
		"\x6\x3\x6\x3\a\x3\a\x3\b\x3\b\x3\t\x3\t\x3\n\x3\n\x3\v\x3\v\x3\f\x3\f"+
		"\x3\r\x3\r\x3\r\x3\xE\x3\xE\x3\xE\x3\xF\x3\xF\x3\x10\x3\x10\x3\x11\x3"+
		"\x11\x3\x11\x3\x12\x3\x12\x3\x12\x3\x13\x3\x13\x3\x13\x3\x14\x3\x14\x3"+
		"\x14\x3\x15\x3\x15\x3\x15\x3\x16\x3\x16\x3\x16\x3\x17\x3\x17\x3\x17\x3"+
		"\x17\x3\x17\x3\x18\x3\x18\x3\x18\x3\x18\x3\x18\x3\x18\x3\x19\x3\x19\x3"+
		"\x19\x3\x19\x3\x1A\x3\x1A\x3\x1A\x3\x1A\a\x1A\x8C\n\x1A\f\x1A\xE\x1A\x8F"+
		"\v\x1A\x3\x1A\x5\x1A\x92\n\x1A\x3\x1A\x3\x1A\x3\x1A\x3\x1A\x3\x1A\a\x1A"+
		"\x99\n\x1A\f\x1A\xE\x1A\x9C\v\x1A\x3\x1A\x3\x1A\x5\x1A\xA0\n\x1A\x3\x1A"+
		"\x3\x1A\x3\x1B\x3\x1B\x3\x1B\x3\x1B\x3\x1C\x3\x1C\x3\x1D\x3\x1D\a\x1D"+
		"\xAC\n\x1D\f\x1D\xE\x1D\xAF\v\x1D\x3\x1E\x6\x1E\xB2\n\x1E\r\x1E\xE\x1E"+
		"\xB3\x3\x1F\x6\x1F\xB7\n\x1F\r\x1F\xE\x1F\xB8\x3\x1F\x3\x1F\x6\x1F\xBD"+
		"\n\x1F\r\x1F\xE\x1F\xBE\x5\x1F\xC1\n\x1F\x3 \x3 \x3 \a \xC6\n \f \xE "+
		"\xC9\v \x3 \x3 \x3!\x3!\x3!\x3!\x5!\xD1\n!\x3!\x5!\xD4\n!\x3\"\x3\"\a"+
		"\"\xD8\n\"\f\"\xE\"\xDB\v\"\x3\x9A\x2\x2#\x3\x2\x3\x5\x2\x4\a\x2\x5\t"+
		"\x2\x6\v\x2\a\r\x2\b\xF\x2\t\x11\x2\n\x13\x2\v\x15\x2\f\x17\x2\r\x19\x2"+
		"\xE\x1B\x2\xF\x1D\x2\x10\x1F\x2\x11!\x2\x12#\x2\x13%\x2\x14\'\x2\x15)"+
		"\x2\x16+\x2\x17-\x2\x18/\x2\x19\x31\x2\x1A\x33\x2\x1B\x35\x2\x1C\x37\x2"+
		"\x1D\x39\x2\x1E;\x2\x1F=\x2 ?\x2!\x41\x2\x2\x43\x2\x2\x3\x2\v\x4\x2\f"+
		"\f\xF\xF\x4\x2\x43\\\x63|\x5\x2\x43\\\x61\x61\x63|\x6\x2\x32;\x43\\\x61"+
		"\x61\x63|\x3\x2\x32;\x4\x2..\x30\x30\x4\x2$$^^\f\x2$$))^^\x63\x64hhpp"+
		"ttvvxx||\x3\x2\x33;\xE7\x2\x3\x3\x2\x2\x2\x2\x5\x3\x2\x2\x2\x2\a\x3\x2"+
		"\x2\x2\x2\t\x3\x2\x2\x2\x2\v\x3\x2\x2\x2\x2\r\x3\x2\x2\x2\x2\xF\x3\x2"+
		"\x2\x2\x2\x11\x3\x2\x2\x2\x2\x13\x3\x2\x2\x2\x2\x15\x3\x2\x2\x2\x2\x17"+
		"\x3\x2\x2\x2\x2\x19\x3\x2\x2\x2\x2\x1B\x3\x2\x2\x2\x2\x1D\x3\x2\x2\x2"+
		"\x2\x1F\x3\x2\x2\x2\x2!\x3\x2\x2\x2\x2#\x3\x2\x2\x2\x2%\x3\x2\x2\x2\x2"+
		"\'\x3\x2\x2\x2\x2)\x3\x2\x2\x2\x2+\x3\x2\x2\x2\x2-\x3\x2\x2\x2\x2/\x3"+
		"\x2\x2\x2\x2\x31\x3\x2\x2\x2\x2\x33\x3\x2\x2\x2\x2\x35\x3\x2\x2\x2\x2"+
		"\x37\x3\x2\x2\x2\x2\x39\x3\x2\x2\x2\x2;\x3\x2\x2\x2\x2=\x3\x2\x2\x2\x2"+
		"?\x3\x2\x2\x2\x3\x45\x3\x2\x2\x2\x5G\x3\x2\x2\x2\aJ\x3\x2\x2\x2\tL\x3"+
		"\x2\x2\x2\vN\x3\x2\x2\x2\rP\x3\x2\x2\x2\xFR\x3\x2\x2\x2\x11T\x3\x2\x2"+
		"\x2\x13V\x3\x2\x2\x2\x15X\x3\x2\x2\x2\x17Z\x3\x2\x2\x2\x19\\\x3\x2\x2"+
		"\x2\x1B_\x3\x2\x2\x2\x1D\x62\x3\x2\x2\x2\x1F\x64\x3\x2\x2\x2!\x66\x3\x2"+
		"\x2\x2#i\x3\x2\x2\x2%l\x3\x2\x2\x2\'o\x3\x2\x2\x2)r\x3\x2\x2\x2+u\x3\x2"+
		"\x2\x2-x\x3\x2\x2\x2/}\x3\x2\x2\x2\x31\x83\x3\x2\x2\x2\x33\x9F\x3\x2\x2"+
		"\x2\x35\xA3\x3\x2\x2\x2\x37\xA7\x3\x2\x2\x2\x39\xA9\x3\x2\x2\x2;\xB1\x3"+
		"\x2\x2\x2=\xB6\x3\x2\x2\x2?\xC2\x3\x2\x2\x2\x41\xD3\x3\x2\x2\x2\x43\xD5"+
		"\x3\x2\x2\x2\x45\x46\a=\x2\x2\x46\x4\x3\x2\x2\x2GH\a\x45\x2\x2HI\a]\x2"+
		"\x2I\x6\x3\x2\x2\x2JK\a.\x2\x2K\b\x3\x2\x2\x2LM\a_\x2\x2M\n\x3\x2\x2\x2"+
		"NO\a?\x2\x2O\f\x3\x2\x2\x2PQ\a$\x2\x2Q\xE\x3\x2\x2\x2RS\a,\x2\x2S\x10"+
		"\x3\x2\x2\x2TU\a\x31\x2\x2U\x12\x3\x2\x2\x2VW\a\'\x2\x2W\x14\x3\x2\x2"+
		"\x2XY\a-\x2\x2Y\x16\x3\x2\x2\x2Z[\a/\x2\x2[\x18\x3\x2\x2\x2\\]\a>\x2\x2"+
		"]^\a>\x2\x2^\x1A\x3\x2\x2\x2_`\a@\x2\x2`\x61\a@\x2\x2\x61\x1C\x3\x2\x2"+
		"\x2\x62\x63\a>\x2\x2\x63\x1E\x3\x2\x2\x2\x64\x65\a@\x2\x2\x65 \x3\x2\x2"+
		"\x2\x66g\a>\x2\x2gh\a?\x2\x2h\"\x3\x2\x2\x2ij\a@\x2\x2jk\a?\x2\x2k$\x3"+
		"\x2\x2\x2lm\a?\x2\x2mn\a?\x2\x2n&\x3\x2\x2\x2op\a#\x2\x2pq\a?\x2\x2q("+
		"\x3\x2\x2\x2rs\a(\x2\x2st\a(\x2\x2t*\x3\x2\x2\x2uv\a~\x2\x2vw\a~\x2\x2"+
		"w,\x3\x2\x2\x2xy\av\x2\x2yz\at\x2\x2z{\aw\x2\x2{|\ag\x2\x2|.\x3\x2\x2"+
		"\x2}~\ah\x2\x2~\x7F\a\x63\x2\x2\x7F\x80\an\x2\x2\x80\x81\au\x2\x2\x81"+
		"\x82\ag\x2\x2\x82\x30\x3\x2\x2\x2\x83\x84\a\"\x2\x2\x84\x85\x3\x2\x2\x2"+
		"\x85\x86\b\x19\x2\x2\x86\x32\x3\x2\x2\x2\x87\x88\a\x31\x2\x2\x88\x89\a"+
		"\x31\x2\x2\x89\x8D\x3\x2\x2\x2\x8A\x8C\n\x2\x2\x2\x8B\x8A\x3\x2\x2\x2"+
		"\x8C\x8F\x3\x2\x2\x2\x8D\x8B\x3\x2\x2\x2\x8D\x8E\x3\x2\x2\x2\x8E\x91\x3"+
		"\x2\x2\x2\x8F\x8D\x3\x2\x2\x2\x90\x92\a\xF\x2\x2\x91\x90\x3\x2\x2\x2\x91"+
		"\x92\x3\x2\x2\x2\x92\x93\x3\x2\x2\x2\x93\xA0\a\f\x2\x2\x94\x95\a\x31\x2"+
		"\x2\x95\x96\a,\x2\x2\x96\x9A\x3\x2\x2\x2\x97\x99\v\x2\x2\x2\x98\x97\x3"+
		"\x2\x2\x2\x99\x9C\x3\x2\x2\x2\x9A\x9B\x3\x2\x2\x2\x9A\x98\x3\x2\x2\x2"+
		"\x9B\x9D\x3\x2\x2\x2\x9C\x9A\x3\x2\x2\x2\x9D\x9E\a,\x2\x2\x9E\xA0\a\x31"+
		"\x2\x2\x9F\x87\x3\x2\x2\x2\x9F\x94\x3\x2\x2\x2\xA0\xA1\x3\x2\x2\x2\xA1"+
		"\xA2\b\x1A\x2\x2\xA2\x34\x3\x2\x2\x2\xA3\xA4\a/\x2\x2\xA4\xA5\a\x31\x2"+
		"\x2\xA5\xA6\a/\x2\x2\xA6\x36\x3\x2\x2\x2\xA7\xA8\t\x3\x2\x2\xA8\x38\x3"+
		"\x2\x2\x2\xA9\xAD\t\x4\x2\x2\xAA\xAC\t\x5\x2\x2\xAB\xAA\x3\x2\x2\x2\xAC"+
		"\xAF\x3\x2\x2\x2\xAD\xAB\x3\x2\x2\x2\xAD\xAE\x3\x2\x2\x2\xAE:\x3\x2\x2"+
		"\x2\xAF\xAD\x3\x2\x2\x2\xB0\xB2\t\x6\x2\x2\xB1\xB0\x3\x2\x2\x2\xB2\xB3"+
		"\x3\x2\x2\x2\xB3\xB1\x3\x2\x2\x2\xB3\xB4\x3\x2\x2\x2\xB4<\x3\x2\x2\x2"+
		"\xB5\xB7\t\x6\x2\x2\xB6\xB5\x3\x2\x2\x2\xB7\xB8\x3\x2\x2\x2\xB8\xB6\x3"+
		"\x2\x2\x2\xB8\xB9\x3\x2\x2\x2\xB9\xC0\x3\x2\x2\x2\xBA\xBC\t\a\x2\x2\xBB"+
		"\xBD\t\x6\x2\x2\xBC\xBB\x3\x2\x2\x2\xBD\xBE\x3\x2\x2\x2\xBE\xBC\x3\x2"+
		"\x2\x2\xBE\xBF\x3\x2\x2\x2\xBF\xC1\x3\x2\x2\x2\xC0\xBA\x3\x2\x2\x2\xC0"+
		"\xC1\x3\x2\x2\x2\xC1>\x3\x2\x2\x2\xC2\xC7\a$\x2\x2\xC3\xC6\x5\x41!\x2"+
		"\xC4\xC6\n\b\x2\x2\xC5\xC3\x3\x2\x2\x2\xC5\xC4\x3\x2\x2\x2\xC6\xC9\x3"+
		"\x2\x2\x2\xC7\xC5\x3\x2\x2\x2\xC7\xC8\x3\x2\x2\x2\xC8\xCA\x3\x2\x2\x2"+
		"\xC9\xC7\x3\x2\x2\x2\xCA\xCB\a$\x2\x2\xCB@\x3\x2\x2\x2\xCC\xCD\a^\x2\x2"+
		"\xCD\xD4\t\t\x2\x2\xCE\xD0\a^\x2\x2\xCF\xD1\a\xF\x2\x2\xD0\xCF\x3\x2\x2"+
		"\x2\xD0\xD1\x3\x2\x2\x2\xD1\xD2\x3\x2\x2\x2\xD2\xD4\a\f\x2\x2\xD3\xCC"+
		"\x3\x2\x2\x2\xD3\xCE\x3\x2\x2\x2\xD4\x42\x3\x2\x2\x2\xD5\xD9\t\n\x2\x2"+
		"\xD6\xD8\t\x6\x2\x2\xD7\xD6\x3\x2\x2\x2\xD8\xDB\x3\x2\x2\x2\xD9\xD7\x3"+
		"\x2\x2\x2\xD9\xDA\x3\x2\x2\x2\xDA\x44\x3\x2\x2\x2\xDB\xD9\x3\x2\x2\x2"+
		"\x11\x2\x8D\x91\x9A\x9F\xAD\xB3\xB8\xBE\xC0\xC5\xC7\xD0\xD3\xD9\x3\b\x2"+
		"\x2";
	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN.ToCharArray());
}
} // namespace ANTLR_Test
