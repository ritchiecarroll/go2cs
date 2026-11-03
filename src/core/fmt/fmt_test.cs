// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using static fmt_package;
using race = @internal.race_package;
using Δio = io_package;
using Δmath = math_package;
using reflect = reflect_package;
using strings = strings_package;
using Δtesting = testing_package;
using time = time_package;
using Δunicode = unicode_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using fmt = fmt_package;
using static go.fmt_internal_test_package;
using ꓸꓸꓸany = Span<any>;

partial class fmt_test_package {

partial struct renamedBool /*bool*/;

partial struct renamedInt /*num:nint*/;

partial struct renamedInt8 /*num:int8*/;

partial struct renamedInt16 /*num:int16*/;

partial struct renamedInt32 /*num:int32*/;

partial struct renamedInt64 /*num:int64*/;

partial struct renamedUint /*num:nuint*/;

partial struct renamedUint8 /*num:uint8*/;

partial struct renamedUint16 /*num:uint16*/;

partial struct renamedUint32 /*num:uint32*/;

partial struct renamedUint64 /*num:uint64*/;

partial struct renamedUintptr /*num:uintptr*/;

partial struct renamedString /*@string*/;

partial struct renamedBytes /*[]byte*/;

partial struct renamedFloat32 /*num:float32*/;

partial struct renamedFloat64 /*num:float64*/;

partial struct renamedComplex64 /*num:complex64*/;

partial struct renamedComplex128 /*num:complex128*/;

public static void TestFmtInterface(ж<Δtesting.T> t) {
    any i1 = default;
    i1 = (@string)"abc";
    @string s = Sprintf("%s"u8, i1);
    if (s != "abc"u8) {
        t.Errorf(@"Sprintf(""%%s"", empty(""abc"")) = %q want %q", s, (@string)"abc");
    }
}

public static readonly ж<float64> ᏑNaN = new StaticBox<float64>(Δmath.NaN());
public static ref float64 NaN => ref ((StaticBox<float64>)ᏑNaN).ValueSlot;
internal static float64 posInf = Δmath.Inf(1);
internal static float64 negInf = Δmath.Inf(-1);

internal static readonly ж<nint> ᏑintVar = new StaticBox<nint>(0);
internal static ref nint intVar => ref ((StaticBox<nint>)ᏑintVar).ValueSlot;

internal static readonly ж<array<nint>> ᏑΔarray = new StaticBox<array<nint>>(new nint[]{1, 2, 3, 4, 5}.array());
internal static ref array<nint> Δarray => ref ((StaticBox<array<nint>>)ᏑΔarray).ValueSlot;
internal static readonly ж<array<any>> Ꮡiarray = new StaticBox<array<any>>(new any[]{(nint)1, (@string)"hello", 2.5D, default}.array());
internal static ref array<any> iarray => ref ((StaticBox<array<any>>)Ꮡiarray).ValueSlot;
internal static readonly ж<slice<nint>> ᏑΔslice = new StaticBox<slice<nint>>(Δarray[..]);
internal static ref slice<nint> Δslice => ref ((StaticBox<slice<nint>>)ᏑΔslice).ValueSlot;
internal static readonly ж<slice<any>> Ꮡislice = new StaticBox<slice<any>>(iarray[..]);
internal static ref slice<any> islice => ref ((StaticBox<slice<any>>)Ꮡislice).ValueSlot;

partial struct A {
    internal nint i;
    internal nuint j;
    internal @string s;
    internal slice<nint> x;
}

partial struct I /*num:nint*/;

public static @string String(this I i) {
    return Sprintf("<%d>"u8, (nint)i);
}

partial struct B {
    public I I;
    internal nint j;
}

partial struct C {
    internal nint i;
    /*embed*/ public B B;
}

partial struct F /*num:nint*/;

public static void Format(this F f, fmt.State s, rune c) {
    Fprintf(new fmt_test_package.fmt_StateᴠWriter(s), "<%c=F(%d)>"u8, c, (nint)f);
}

partial struct G /*num:nint*/;

public static @string GoString(this G g) {
    return Sprintf("GoString(%d)"u8, (nint)g);
}

partial struct S {
    public F F; // a struct field that Formats
    public G G; // a struct field that GoStrings
}

partial struct SI {
    public any I;
}

// P is a type with a String method with pointer receiver for testing %p.
partial struct P /*num:nint*/;

internal static readonly ж<P> ᏑpValue = new StaticBox<P>(default(P));
internal static ref P pValue => ref ((StaticBox<P>)ᏑpValue).ValueSlot;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string stringPˢ => ref stringPˢᵇ;
private static readonly @string stringPˢᵇ = "String(p)"u8;

public static @string String(this ref P p) {
    return stringPˢ;
}

// Fn is a function type with a String method.
public delegate nint Fn();

public static @string String(this Fn fn) {
    return "String(fn)";
}

internal static Fn fnValue;

// U is a type with two unexported function fields.
partial struct U {
    internal Func<@string> u;
    internal Fn fn;
}

internal static readonly ж<array<renamedUint8>> Ꮡbarray = new StaticBox<array<renamedUint8>>(new renamedUint8[]{1, 2, 3, 4, 5}.array());
internal static ref array<renamedUint8> barray => ref ((StaticBox<array<renamedUint8>>)Ꮡbarray).ValueSlot;

internal static readonly ж<slice<renamedUint8>> Ꮡbslice = new StaticBox<slice<renamedUint8>>(barray[..]);
internal static ref slice<renamedUint8> bslice => ref ((StaticBox<slice<renamedUint8>>)Ꮡbslice).ValueSlot;

partial struct byteStringer /*num:byte*/;

internal static @string String(this byteStringer _) {
    return "X";
}

internal static slice<byteStringer> byteStringerSlice = slice<byteStringer>([(rune)'h', (rune)'e', (rune)'l', (rune)'l', (rune)'o']);

partial struct byteFormatter /*num:byte*/;

internal static void Format(this byteFormatter _Δp0, fmt.State f, rune _Δp2) {
    Fprint(new fmt_test_package.fmt_StateᴠWriter(f), "X");
}

internal static slice<byteFormatter> byteFormatterSlice = slice<byteFormatter>([(rune)'h', (rune)'e', (rune)'l', (rune)'l', (rune)'o']);

partial struct writeStringFormatter /*@string*/;

internal static void Format(this writeStringFormatter sf, fmt.State f, rune c) {
    {
        var (sw, ok) = f._<Δio.StringWriter>(ᐧ); if (ok) {
            sw.WriteString("***"u8 + ((@string)sf) + "***"u8);
        }
    }
}

partial struct fmtTestsᴛ1 /*dyn*/ {
    internal @string fmt;
    internal any val;
    internal @string @out;
}
internal static slice<fmtTestsᴛ1> fmtTests = slice<fmtTestsᴛ1>([
    new("%d", (nint)12345, "12345"),
    new("%v", (nint)12345, "12345"),
    new("%t", true, "true"),

    // basic string
    new("%s", (@string)"abc", "abc"),
    new("%q", (@string)"abc", @"""abc"""),
    new("%x", (@string)"abc", "616263"),
    new("%x", ((@string)(new byte[]{0xff, 0xf0, 0x0f, 0xff})), "fff00fff"),
    new("%X", ((@string)(new byte[]{0xff, 0xf0, 0x0f, 0xff})), "FFF00FFF"),
    new("%x", (@string)""u8, ""),
    new("% x", (@string)""u8, ""),
    new("%#x", (@string)""u8, ""),
    new("%# x", (@string)""u8, ""),
    new("%x", (@string)"xyz", "78797a"),
    new("%X", (@string)"xyz", "78797A"),
    new("% x", (@string)"xyz", "78 79 7a"),
    new("% X", (@string)"xyz", "78 79 7A"),
    new("%#x", (@string)"xyz", "0x78797a"),
    new("%#X", (@string)"xyz", "0X78797A"),
    new("%# x", (@string)"xyz", "0x78 0x79 0x7a"),
    new("%# X", (@string)"xyz", "0X78 0X79 0X7A"),

    // basic bytes
    new("%s", slice<byte>("abc"u8), "abc"),
    new("%s", new byte[]{(byte)'a', (byte)'b', (byte)'c'}.array(), "abc"),
    new("%s", Ꮡ(new byte[]{(byte)'a', (byte)'b', (byte)'c'}.array()), "&abc"),
    new("%q", slice<byte>("abc"u8), @"""abc"""),
    new("%x", slice<byte>("abc"u8), "616263"),
    new("%x", slice<byte>(((@string)(new byte[]{0xff, 0xf0, 0x0f, 0xff}))), "fff00fff"),
    new("%X", slice<byte>(((@string)(new byte[]{0xff, 0xf0, 0x0f, 0xff}))), "FFF00FFF"),
    new("%x", slice<byte>(""u8), ""),
    new("% x", slice<byte>(""u8), ""),
    new("%#x", slice<byte>(""u8), ""),
    new("%# x", slice<byte>(""u8), ""),
    new("%x", slice<byte>("xyz"u8), "78797a"),
    new("%X", slice<byte>("xyz"u8), "78797A"),
    new("% x", slice<byte>("xyz"u8), "78 79 7a"),
    new("% X", slice<byte>("xyz"u8), "78 79 7A"),
    new("%#x", slice<byte>("xyz"u8), "0x78797a"),
    new("%#X", slice<byte>("xyz"u8), "0X78797A"),
    new("%# x", slice<byte>("xyz"u8), "0x78 0x79 0x7a"),
    new("%# X", slice<byte>("xyz"u8), "0X78 0X79 0X7A"),

    // escaped strings
    new("%q", (@string)""u8, @""""""),
    new("%#q", (@string)""u8, "``"),
    new("%q", (@string)"\"", @"""\"""""),
    new("%#q", (@string)"\"", "`\"`"),
    new("%q", (@string)"`", @""""u8 + "`"u8 + @""""u8),
    new("%#q", (@string)"`", @""""u8 + "`"u8 + @""""u8),
    new("%q", (@string)"\n", @"""\n"""),
    new("%#q", (@string)"\n", @"""\n"""),
    new("%q", (@string)@"\n", @"""\\n"""),
    new("%#q", (@string)@"\n", "`\\n`"),
    new("%q", (@string)"abc", @"""abc"""),
    new("%#q", (@string)"abc", "`abc`"),
    new("%q", (@string)"日本語", @"""日本語"""),
    new("%+q", (@string)"日本語", @"""\u65e5\u672c\u8a9e"""),
    new("%#q", (@string)"日本語", "`日本語`"),
    new("%#+q", (@string)"日本語", "`日本語`"),
    new("%q", (@string)"\a\b\f\n\r\t\v\"\\", @"""\a\b\f\n\r\t\v\""\\"""),
    new("%+q", (@string)"\a\b\f\n\r\t\v\"\\", @"""\a\b\f\n\r\t\v\""\\"""),
    new("%#q", (@string)"\a\b\f\n\r\t\v\"\\", @"""\a\b\f\n\r\t\v\""\\"""),
    new("%#+q", (@string)"\a\b\f\n\r\t\v\"\\", @"""\a\b\f\n\r\t\v\""\\"""),
    new("%q", (@string)"☺", @"""☺"""),
    new("% q", (@string)"☺", @"""☺"""), // The space modifier should have no effect.
    new("%+q", (@string)"☺", @"""\u263a"""),
    new("%#q", (@string)"☺", "`☺`"),
    new("%#+q", (@string)"☺", "`☺`"),
    new("%10q", (@string)"⌘", @"       ""⌘"""),
    new("%+10q", (@string)"⌘", @"  ""\u2318"""),
    new("%-10q", (@string)"⌘", @"""⌘""       "),
    new("%+-10q", (@string)"⌘", @"""\u2318""  "),
    new("%010q", (@string)"⌘", @"0000000""⌘"""),
    new("%+010q", (@string)"⌘", @"00""\u2318"""),
    new("%-010q", (@string)"⌘", @"""⌘""       "), // 0 has no effect when - is present.
    new("%+-010q", (@string)"⌘", @"""\u2318""  "),
    new("%#8q", (@string)"\n", @"    ""\n"""),
    new("%#+8q", (@string)"\r", @"    ""\r"""),
    new("%#-8q", (@string)"\t", "`	`     "),
    new("%#+-8q", (@string)"\b", @"""\b""    "),
    new("%q", ((@string)(new byte[]{0x61, 0x62, 0x63, 0xff, 0x64, 0x65, 0x66})), @"""abc\xffdef"""),
    new("%+q", ((@string)(new byte[]{0x61, 0x62, 0x63, 0xff, 0x64, 0x65, 0x66})), @"""abc\xffdef"""),
    new("%#q", ((@string)(new byte[]{0x61, 0x62, 0x63, 0xff, 0x64, 0x65, 0x66})), @"""abc\xffdef"""),
    new("%#+q", ((@string)(new byte[]{0x61, 0x62, 0x63, 0xff, 0x64, 0x65, 0x66})), @"""abc\xffdef"""),
    // Runes that are not printable.
    new("%q", (@string)"\U0010ffff", @"""\U0010ffff"""),
    new("%+q", (@string)"\U0010ffff", @"""\U0010ffff"""),
    new("%#q", (@string)"\U0010ffff", "`􏿿`"),
    new("%#+q", (@string)"\U0010ffff", "`􏿿`"),
    // Runes that are not valid.
    new("%q", (@string)(rune)0x110000, @"""�"""),
    new("%+q", (@string)(rune)0x110000, @"""\ufffd"""),
    new("%#q", (@string)(rune)0x110000, "`�`"),
    new("%#+q", (@string)(rune)0x110000, "`�`"),

    // characters
    new("%c", (nuint)'x', "x"),
    new("%c", (nint)0xe4, "ä"),
    new("%c", (nint)0x672c, "本"),
    new("%c", (rune)'日', "日"),
    new("%.0c", (rune)'⌘', "⌘"), // Specifying precision should have no effect.
    new("%3c", (rune)'⌘', "  ⌘"),
    new("%-3c", (rune)'⌘', "⌘  "),
    new("%c", (uint64)0x100000000UL, "\ufffd"),
    // Runes that are not printable.
    new("%c", (rune)'\U00000e00', "\u0e00"),
    new("%c", (rune)0x10FFFF, "\U0010ffff"),
    // Runes that are not valid.
    new("%c", (nint)(-1), "�"),
    new("%c", (nint)0xDC80, "�"),
    new("%c", (rune)0x110000, "�"),
    new("%c", (int64)0xFFFFFFFFFL, "�"),
    new("%c", (uint64)0xFFFFFFFFFUL, "�"),

    // escaped characters
    new("%q", (nuint)0, @"'\x00'"),
    new("%+q", (nuint)0, @"'\x00'"),
    new("%q", (rune)'"', @"'""'"),
    new("%+q", (rune)'"', @"'""'"),
    new("%q", (rune)'\'', @"'\''"),
    new("%+q", (rune)'\'', @"'\''"),
    new("%q", (rune)'`', "'`'"),
    new("%+q", (rune)'`', "'`'"),
    new("%q", (rune)'x', @"'x'"),
    new("%+q", (rune)'x', @"'x'"),
    new("%q", (rune)'ÿ', @"'ÿ'"),
    new("%+q", (rune)'ÿ', @"'\u00ff'"),
    new("%q", (rune)'\n', @"'\n'"),
    new("%+q", (rune)'\n', @"'\n'"),
    new("%q", (rune)'☺', @"'☺'"),
    new("%+q", (rune)'☺', @"'\u263a'"),
    new("% q", (rune)'☺', @"'☺'"),  // The space modifier should have no effect.
    new("%.0q", (rune)'☺', @"'☺'"), // Specifying precision should have no effect.
    new("%10q", (rune)'⌘', @"       '⌘'"),
    new("%+10q", (rune)'⌘', @"  '\u2318'"),
    new("%-10q", (rune)'⌘', @"'⌘'       "),
    new("%+-10q", (rune)'⌘', @"'\u2318'  "),
    new("%010q", (rune)'⌘', @"0000000'⌘'"),
    new("%+010q", (rune)'⌘', @"00'\u2318'"),
    new("%-010q", (rune)'⌘', @"'⌘'       "), // 0 has no effect when - is present.
    new("%+-010q", (rune)'⌘', @"'\u2318'  "),
    // Runes that are not printable.
    new("%q", (rune)'\U00000e00', @"'\u0e00'"),
    new("%q", (rune)0x10FFFF, @"'\U0010ffff'"),
    // Runes that are not valid.
    new("%q", (int32)(-1), @"'�'"),
    new("%q", (nint)0xDC80, @"'�'"),
    new("%q", (rune)0x110000, @"'�'"),
    new("%q", (int64)0xFFFFFFFFFL, @"'�'"),
    new("%q", (uint64)0xFFFFFFFFFUL, @"'�'"),

    // width
    new("%5s", (@string)"abc", "  abc"),
    new("%5s", slice<byte>("abc"u8), "  abc"),
    new("%2s", (@string)"\u263a", " ☺"),
    new("%2s", slice<byte>("\u263a"u8), " ☺"),
    new("%-5s", (@string)"abc", "abc  "),
    new("%-5s", slice<byte>("abc"u8), "abc  "),
    new("%05s", (@string)"abc", "00abc"),
    new("%05s", slice<byte>("abc"u8), "00abc"),
    new("%5s", (@string)"abcdefghijklmnopqrstuvwxyz", "abcdefghijklmnopqrstuvwxyz"),
    new("%5s", slice<byte>("abcdefghijklmnopqrstuvwxyz"u8), "abcdefghijklmnopqrstuvwxyz"),
    new("%.5s", (@string)"abcdefghijklmnopqrstuvwxyz", "abcde"),
    new("%.5s", slice<byte>("abcdefghijklmnopqrstuvwxyz"u8), "abcde"),
    new("%.0s", (@string)"日本語日本語", ""),
    new("%.0s", slice<byte>("日本語日本語"u8), ""),
    new("%.5s", (@string)"日本語日本語", "日本語日本"),
    new("%.5s", slice<byte>("日本語日本語"u8), "日本語日本"),
    new("%.10s", (@string)"日本語日本語", "日本語日本語"),
    new("%.10s", slice<byte>("日本語日本語"u8), "日本語日本語"),
    new("%08q", (@string)"abc", @"000""abc"""),
    new("%08q", slice<byte>("abc"u8), @"000""abc"""),
    new("%-8q", (@string)"abc", @"""abc""   "),
    new("%-8q", slice<byte>("abc"u8), @"""abc""   "),
    new("%.5q", (@string)"abcdefghijklmnopqrstuvwxyz", @"""abcde"""),
    new("%.5q", slice<byte>("abcdefghijklmnopqrstuvwxyz"u8), @"""abcde"""),
    new("%.5x", (@string)"abcdefghijklmnopqrstuvwxyz", "6162636465"),
    new("%.5x", slice<byte>("abcdefghijklmnopqrstuvwxyz"u8), "6162636465"),
    new("%.3q", (@string)"日本語日本語", @"""日本語"""),
    new("%.3q", slice<byte>("日本語日本語"u8), @"""日本語"""),
    new("%.1q", (@string)"日本語", @"""日"""),
    new("%.1q", slice<byte>("日本語"u8), @"""日"""),
    new("%.1x", (@string)"日本語", "e6"),
    new("%.1X", slice<byte>("日本語"u8), "E6"),
    new("%10.1q", (@string)"日本語日本語", @"       ""日"""),
    new("%10.1q", slice<byte>("日本語日本語"u8), @"       ""日"""),
    new("%10v", default, "     <nil>"),
    new("%-10v", default, "<nil>     "),

    // integers
    new("%d", (nuint)12345, "12345"),
    new("%d", (nint)(-12345), "-12345"),
    new("%d", unchecked((uint8)(~(uint8)0)), "255"),
    new("%d", unchecked((uint16)(~(uint16)0)), "65535"),
    new("%d", ~(uint32)0, "4294967295"),
    new("%d", ~(uint64)0, "18446744073709551615"),
    new("%d", (int8)((int8)(-1 << 7)), "-128"),
    new("%d", (int16)((int16)(-1 << 15)), "-32768"),
    new("%d", (int32)((int32)(-1 << 31)), "-2147483648"),
    new("%d", (int64)(-9223372036854775808L), "-9223372036854775808"),
    new("%.d", (nint)0, ""),
    new("%.0d", (nint)0, ""),
    new("%6.0d", (nint)0, "      "),
    new("%06.0d", (nint)0, "      "),
    new("% d", (nint)12345, " 12345"),
    new("%+d", (nint)12345, "+12345"),
    new("%+d", (nint)(-12345), "-12345"),
    new("%b", (nint)7, "111"),
    new("%b", (nint)(-6), "-110"),
    new("%#b", (nint)7, "0b111"),
    new("%#b", (nint)(-6), "-0b110"),
    new("%b", ~(uint32)0, "11111111111111111111111111111111"),
    new("%b", ~(uint64)0, "1111111111111111111111111111111111111111111111111111111111111111"),
    new("%b", (int64)(-9223372036854775808L), zeroFill("-1", 63, "")),
    new("%o", (nint)668, "1234"),
    new("%o", (nint)(-668), "-1234"),
    new("%#o", (nint)668, "01234"),
    new("%#o", (nint)(-668), "-01234"),
    new("%O", (nint)668, "0o1234"),
    new("%O", (nint)(-668), "-0o1234"),
    new("%o", ~(uint32)0, "37777777777"),
    new("%o", ~(uint64)0, "1777777777777777777777"),
    new("%#X", (nint)0, "0X0"),
    new("%x", (nint)0x12abcdef, "12abcdef"),
    new("%X", (nint)0x12abcdef, "12ABCDEF"),
    new("%x", ~(uint32)0, "ffffffff"),
    new("%X", ~(uint64)0, "FFFFFFFFFFFFFFFF"),
    new("%.20b", (nint)7, "00000000000000000111"),
    new("%10d", (nint)12345, "     12345"),
    new("%10d", (nint)(-12345), "    -12345"),
    new("%+10d", (nint)12345, "    +12345"),
    new("%010d", (nint)12345, "0000012345"),
    new("%010d", (nint)(-12345), "-000012345"),
    new("%20.8d", (nint)1234, "            00001234"),
    new("%20.8d", (nint)(-1234), "           -00001234"),
    new("%020.8d", (nint)1234, "            00001234"),
    new("%020.8d", (nint)(-1234), "           -00001234"),
    new("%-20.8d", (nint)1234, "00001234            "),
    new("%-20.8d", (nint)(-1234), "-00001234           "),
    new("%-#20.8x", (nint)0x1234abc, "0x01234abc          "),
    new("%-#20.8X", (nint)0x1234abc, "0X01234ABC          "),
    new("%-#20.8o", (nint)668, "00001234            "),

    // Test correct f.intbuf overflow checks.
    new("%068d", (nint)1, zeroFill("", 68, "1")),
    new("%068d", (nint)(-1), zeroFill("-", 67, "1")),
    new("%#.68x", (nint)42, zeroFill("0x", 68, "2a")),
    new("%.68d", (nint)(-42), zeroFill("-", 68, "42")),
    new("%+.68d", (nint)42, zeroFill("+", 68, "42")),
    new("% .68d", (nint)42, zeroFill(" ", 68, "42")),
    new("% +.68d", (nint)42, zeroFill("+", 68, "42")),

    // unicode format
    new("%U", (nint)0, "U+0000"),
    new("%U", (nint)(-1), "U+FFFFFFFFFFFFFFFF"),
    new("%U", (rune)'\n', @"U+000A"),
    new("%#U", (rune)'\n', @"U+000A"),
    new("%+U", (rune)'x', @"U+0078"),       // Plus flag should have no effect.
    new("%# U", (rune)'x', @"U+0078 'x'"),  // Space flag should have no effect.
    new("%#.2U", (rune)'x', @"U+0078 'x'"), // Precisions below 4 should print 4 digits.
    new("%U", (rune)'\u263a', @"U+263A"),
    new("%#U", (rune)'\u263a', @"U+263A '☺'"),
    new("%U", (rune)0x1D6C2, @"U+1D6C2"),
    new("%#U", (rune)0x1D6C2, @"U+1D6C2 '𝛂'"),
    new("%#14.6U", (rune)'⌘', "  U+002318 '⌘'"),
    new("%#-14.6U", (rune)'⌘', "U+002318 '⌘'  "),
    new("%#014.6U", (rune)'⌘', "  U+002318 '⌘'"),
    new("%#-014.6U", (rune)'⌘', "U+002318 '⌘'  "),
    new("%.68U", (nuint)42, zeroFill("U+", 68, "2A")),
    new("%#.68U", (rune)'日', zeroFill("U+", 68, "65E5") + " '日'"u8),

    // floats
    new("%+.3e", 0.0D, "+0.000e+00"),
    new("%+.3e", 1.0D, "+1.000e+00"),
    new("%+.3x", 0.0D, "+0x0.000p+00"),
    new("%+.3x", 1.0D, "+0x1.000p+00"),
    new("%+.3f", -1.0D, "-1.000"),
    new("%+.3F", -1.0D, "-1.000"),
    new("%+.3F", (float32)(-1.0F), "-1.000"),
    new("%+07.2f", 1.0D, "+001.00"),
    new("%+07.2f", -1.0D, "-001.00"),
    new("%-07.2f", 1.0D, "1.00   "),
    new("%-07.2f", -1.0D, "-1.00  "),
    new("%+-07.2f", 1.0D, "+1.00  "),
    new("%+-07.2f", -1.0D, "-1.00  "),
    new("%-+07.2f", 1.0D, "+1.00  "),
    new("%-+07.2f", -1.0D, "-1.00  "),
    new("%+10.2f", +1.0D, "     +1.00"),
    new("%+10.2f", -1.0D, "     -1.00"),
    new("% .3E", -1.0D, "-1.000E+00"),
    new("% .3e", 1.0D, " 1.000e+00"),
    new("% .3X", -1.0D, "-0X1.000P+00"),
    new("% .3x", 1.0D, " 0x1.000p+00"),
    new("%+.3g", 0.0D, "+0"),
    new("%+.3g", 1.0D, "+1"),
    new("%+.3g", -1.0D, "-1"),
    new("% .3g", -1.0D, "-1"),
    new("% .3g", 1.0D, " 1"),
    new("%b", (float32)1.0F, "8388608p-23"),
    new("%b", 1.0D, "4503599627370496p-52"),
    // Test sharp flag used with floats.
    new("%#g", 1e-323D, "1.00000e-323"),
    new("%#g", -1.0D, "-1.00000"),
    new("%#g", 1.1D, "1.10000"),
    new("%#g", 123456.0D, "123456."),
    new("%#g", 1234567.0D, "1.234567e+06"),
    new("%#g", 1230000.0D, "1.23000e+06"),
    new("%#g", 1000000.0D, "1.00000e+06"),
    new("%#.0f", 1.0D, "1."),
    new("%#.0e", 1.0D, "1.e+00"),
    new("%#.0x", 1.0D, "0x1.p+00"),
    new("%#.0g", 1.0D, "1."),
    new("%#.0g", 1100000.0D, "1.e+06"),
    new("%#.4f", 1.0D, "1.0000"),
    new("%#.4e", 1.0D, "1.0000e+00"),
    new("%#.4x", 1.0D, "0x1.0000p+00"),
    new("%#.4g", 1.0D, "1.000"),
    new("%#.4g", 100000.0D, "1.000e+05"),
    new("%#.4g", 1.234D, "1.234"),
    new("%#.4g", 0.1234D, "0.1234"),
    new("%#.4g", 1.23D, "1.230"),
    new("%#.4g", 0.123D, "0.1230"),
    new("%#.4g", 1.2D, "1.200"),
    new("%#.4g", 0.12D, "0.1200"),
    new("%#.4g", 10.2D, "10.20"),
    new("%#.4g", 0.0D, "0.000"),
    new("%#.4g", 0.012D, "0.01200"),
    new("%#.0f", 123.0D, "123."),
    new("%#.0e", 123.0D, "1.e+02"),
    new("%#.0x", 123.0D, "0x1.p+07"),
    new("%#.0g", 123.0D, "1.e+02"),
    new("%#.4f", 123.0D, "123.0000"),
    new("%#.4e", 123.0D, "1.2300e+02"),
    new("%#.4x", 123.0D, "0x1.ec00p+06"),
    new("%#.4g", 123.0D, "123.0"),
    new("%#.4g", 123000.0D, "1.230e+05"),
    new("%#9.4g", 1.0D, "    1.000"),
    // The sharp flag has no effect for binary float format.
    new("%#b", 1.0D, "4503599627370496p-52"),
    // Precision has no effect for binary float format.
    new("%.4b", (float32)1.0F, "8388608p-23"),
    new("%.4b", -1.0D, "-4503599627370496p-52"),
    // Test correct f.intbuf boundary checks.
    new("%.68f", 1.0D, zeroFill("1.", 68, "")),
    new("%.68f", -1.0D, zeroFill("-1.", 68, "")),
    // float infinites and NaNs
    new("%f", posInf, "+Inf"),
    new("%.1f", negInf, "-Inf"),
    new("% f", NaN, " NaN"),
    new("%20f", posInf, "                +Inf"),
    new("% 20F", posInf, "                 Inf"),
    new("% 20e", negInf, "                -Inf"),
    new("% 20x", negInf, "                -Inf"),
    new("%+20E", negInf, "                -Inf"),
    new("%+20X", negInf, "                -Inf"),
    new("% +20g", negInf, "                -Inf"),
    new("%+-20G", posInf, "+Inf                "),
    new("%20e", NaN, "                 NaN"),
    new("%20x", NaN, "                 NaN"),
    new("% +20E", NaN, "                +NaN"),
    new("% +20X", NaN, "                +NaN"),
    new("% -20g", NaN, " NaN                "),
    new("%+-20G", NaN, "+NaN                "),
    // Zero padding does not apply to infinities and NaN.
    new("%+020e", posInf, "                +Inf"),
    new("%+020x", posInf, "                +Inf"),
    new("%-020f", negInf, "-Inf                "),
    new("%-020E", NaN, "NaN                 "),
    new("%-020X", NaN, "NaN                 "),

    // complex values
    new("%.f", 0D.i(), "(0+0i)"),
    new("% .f", 0D.i(), "( 0+0i)"),
    new("%+.f", 0D.i(), "(+0+0i)"),
    new("% +.f", 0D.i(), "(+0+0i)"),
    new("%+.3e", 0D.i(), "(+0.000e+00+0.000e+00i)"),
    new("%+.3x", 0D.i(), "(+0x0.000p+00+0x0.000p+00i)"),
    new("%+.3f", 0D.i(), "(+0.000+0.000i)"),
    new("%+.3g", 0D.i(), "(+0+0i)"),
    new("%+.3e", 1D + 2D.i(), "(+1.000e+00+2.000e+00i)"),
    new("%+.3x", 1D + 2D.i(), "(+0x1.000p+00+0x1.000p+01i)"),
    new("%+.3f", 1D + 2D.i(), "(+1.000+2.000i)"),
    new("%+.3g", 1D + 2D.i(), "(+1+2i)"),
    new("%.3e", 0D.i(), "(0.000e+00+0.000e+00i)"),
    new("%.3x", 0D.i(), "(0x0.000p+00+0x0.000p+00i)"),
    new("%.3f", 0D.i(), "(0.000+0.000i)"),
    new("%.3F", 0D.i(), "(0.000+0.000i)"),
    new("%.3F", (complex64)0F.i(), "(0.000+0.000i)"),
    new("%.3g", 0D.i(), "(0+0i)"),
    new("%.3e", 1D + 2D.i(), "(1.000e+00+2.000e+00i)"),
    new("%.3x", 1D + 2D.i(), "(0x1.000p+00+0x1.000p+01i)"),
    new("%.3f", 1D + 2D.i(), "(1.000+2.000i)"),
    new("%.3g", 1D + 2D.i(), "(1+2i)"),
    new("%.3e", -1D + -2D.i(), "(-1.000e+00-2.000e+00i)"),
    new("%.3x", -1D + -2D.i(), "(-0x1.000p+00-0x1.000p+01i)"),
    new("%.3f", -1D + -2D.i(), "(-1.000-2.000i)"),
    new("%.3g", -1D + -2D.i(), "(-1-2i)"),
    new("% .3E", -1D + -2D.i(), "(-1.000E+00-2.000E+00i)"),
    new("% .3X", -1D + -2D.i(), "(-0X1.000P+00-0X1.000P+01i)"),
    new("%+.3g", 1D + 2D.i(), "(+1+2i)"),
    new("%+.3g", (complex64)(1F + 2F.i()), "(+1+2i)"),
    new("%#g", 1D + 2D.i(), "(1.00000+2.00000i)"),
    new("%#g", 123456D + 789012D.i(), "(123456.+789012.i)"),
    new("%#g", 1e-10D.i(), "(0.00000+1.00000e-10i)"),
    new("%#g", -1e+10D + -1.11e+100D.i(), "(-1.00000e+10-1.11000e+100i)"),
    new("%#.0f", 1.23D + 1D.i(), "(1.+1.i)"),
    new("%#.0e", 1.23D + 1D.i(), "(1.e+00+1.e+00i)"),
    new("%#.0x", 1.23D + 1D.i(), "(0x1.p+00+0x1.p+00i)"),
    new("%#.0g", 1.23D + 1D.i(), "(1.+1.i)"),
    new("%#.0g", 100000D.i(), "(0.+1.e+05i)"),
    new("%#.0g", 1.23e+06D + 0D.i(), "(1.e+06+0.i)"),
    new("%#.4f", 1D + 1.23D.i(), "(1.0000+1.2300i)"),
    new("%#.4e", 123D + 1D.i(), "(1.2300e+02+1.0000e+00i)"),
    new("%#.4x", 123D + 1D.i(), "(0x1.ec00p+06+0x1.0000p+00i)"),
    new("%#.4g", 123D + 1.23D.i(), "(123.0+1.230i)"),
    new("%#12.5g", 100000D.i(), "(      0.0000 +1.0000e+05i)"),
    new("%#12.5g", 1.23e+06D + 0D.i(), "(  1.2300e+06     +0.0000i)"),
    new("%b", 1D + 2D.i(), "(4503599627370496p-52+4503599627370496p-51i)"),
    new("%b", (complex64)(1F + 2F.i()), "(8388608p-23+8388608p-22i)"),
    // The sharp flag has no effect for binary complex format.
    new("%#b", 1D + 2D.i(), "(4503599627370496p-52+4503599627370496p-51i)"),
    // Precision has no effect for binary complex format.
    new("%.4b", 1D + 2D.i(), "(4503599627370496p-52+4503599627370496p-51i)"),
    new("%.4b", (complex64)(1F + 2F.i()), "(8388608p-23+8388608p-22i)"),
    // complex infinites and NaNs
    new("%f", complex(posInf, posInf), "(+Inf+Infi)"),
    new("%f", complex(negInf, negInf), "(-Inf-Infi)"),
    new("%f", complex(NaN, NaN), "(NaN+NaNi)"),
    new("%.1f", complex(posInf, posInf), "(+Inf+Infi)"),
    new("% f", complex(posInf, posInf), "( Inf+Infi)"),
    new("% f", complex(negInf, negInf), "(-Inf-Infi)"),
    new("% f", complex(NaN, NaN), "( NaN+NaNi)"),
    new("%8e", complex(posInf, posInf), "(    +Inf    +Infi)"),
    new("%8x", complex(posInf, posInf), "(    +Inf    +Infi)"),
    new("% 8E", complex(posInf, posInf), "(     Inf    +Infi)"),
    new("% 8X", complex(posInf, posInf), "(     Inf    +Infi)"),
    new("%+8f", complex(negInf, negInf), "(    -Inf    -Infi)"),
    new("% +8g", complex(negInf, negInf), "(    -Inf    -Infi)"),
    new("% -8G", complex(NaN, NaN), "( NaN    +NaN    i)"),
    new("%+-8b", complex(NaN, NaN), "(+NaN    +NaN    i)"),
    // Zero padding does not apply to infinities and NaN.
    new("%08f", complex(posInf, posInf), "(    +Inf    +Infi)"),
    new("%-08g", complex(negInf, negInf), "(-Inf    -Inf    i)"),
    new("%-08G", complex(NaN, NaN), "(NaN     +NaN    i)"),

    // old test/fmt_test.go
    new("%e", 1.0D, "1.000000e+00"),
    new("%e", 1234.5678e3D, "1.234568e+06"),
    new("%e", 1234.5678e-8D, "1.234568e-05"),
    new("%e", -7.0D, "-7.000000e+00"),
    new("%e", -1e-9D, "-1.000000e-09"),
    new("%f", 1234.5678e3D, "1234567.800000"),
    new("%f", 1234.5678e-8D, "0.000012"),
    new("%f", -7.0D, "-7.000000"),
    new("%f", -1e-9D, "-0.000000"),
    new("%g", 1234.5678e3D, "1.2345678e+06"),
    new("%g", (float32)1234.5678e3F, "1.2345678e+06"),
    new("%g", 1234.5678e-8D, "1.2345678e-05"),
    new("%g", -7.0D, "-7"),
    new("%g", -1e-9D, "-1e-09"),
    new("%g", (float32)(-1e-9F), "-1e-09"),
    new("%E", 1.0D, "1.000000E+00"),
    new("%E", 1234.5678e3D, "1.234568E+06"),
    new("%E", 1234.5678e-8D, "1.234568E-05"),
    new("%E", -7.0D, "-7.000000E+00"),
    new("%E", -1e-9D, "-1.000000E-09"),
    new("%G", 1234.5678e3D, "1.2345678E+06"),
    new("%G", (float32)1234.5678e3F, "1.2345678E+06"),
    new("%G", 1234.5678e-8D, "1.2345678E-05"),
    new("%G", -7.0D, "-7"),
    new("%G", -1e-9D, "-1E-09"),
    new("%G", (float32)(-1e-9F), "-1E-09"),
    new("%20.5s", (@string)"qwertyuiop", "               qwert"),
    new("%.5s", (@string)"qwertyuiop", "qwert"),
    new("%-20.5s", (@string)"qwertyuiop", "qwert               "),
    new("%20c", (rune)'x', "                   x"),
    new("%-20c", (rune)'x', "x                   "),
    new("%20.6e", 1.2345e3D, "        1.234500e+03"),
    new("%20.6e", 1.2345e-3D, "        1.234500e-03"),
    new("%20e", 1.2345e3D, "        1.234500e+03"),
    new("%20e", 1.2345e-3D, "        1.234500e-03"),
    new("%20.8e", 1.2345e3D, "      1.23450000e+03"),
    new("%20f", 1.23456789e3D, "         1234.567890"),
    new("%20f", 1.23456789e-3D, "            0.001235"),
    new("%20f", 12345678901.23456789D, "  12345678901.234568"),
    new("%-20f", 1.23456789e3D, "1234.567890         "),
    new("%20.8f", 1.23456789e3D, "       1234.56789000"),
    new("%20.8f", 1.23456789e-3D, "          0.00123457"),
    new("%g", 1.23456789e3D, "1234.56789"),
    new("%g", 1.23456789e-3D, "0.00123456789"),
    new("%g", 1.23456789e20D, "1.23456789e+20"),

    // arrays
    new("%v", Δarray.Clone(), "[1 2 3 4 5]"),
    new("%v", iarray.Clone(), "[1 hello 2.5 <nil>]"),
    new("%v", barray.Clone(), "[1 2 3 4 5]"),
    new("%v", ᏑΔarray, "&[1 2 3 4 5]"),
    new("%v", Ꮡiarray, "&[1 hello 2.5 <nil>]"),
    new("%v", Ꮡbarray, "&[1 2 3 4 5]"),

    // slices
    new("%v", Δslice, "[1 2 3 4 5]"),
    new("%v", islice, "[1 hello 2.5 <nil>]"),
    new("%v", bslice, "[1 2 3 4 5]"),
    new("%v", ᏑΔslice, "&[1 2 3 4 5]"),
    new("%v", Ꮡislice, "&[1 hello 2.5 <nil>]"),
    new("%v", Ꮡbslice, "&[1 2 3 4 5]"),

    // byte arrays and slices with %b,%c,%d,%o,%U and %v
    new("%b", new byte[]{65, 66, 67}.array(), "[1000001 1000010 1000011]"),
    new("%c", new byte[]{65, 66, 67}.array(), "[A B C]"),
    new("%d", new byte[]{65, 66, 67}.array(), "[65 66 67]"),
    new("%o", new byte[]{65, 66, 67}.array(), "[101 102 103]"),
    new("%U", new byte[]{65, 66, 67}.array(), "[U+0041 U+0042 U+0043]"),
    new("%v", new byte[]{65, 66, 67}.array(), "[65 66 67]"),
    new("%v", new byte[]{123}.array(), "[123]"),
    new("%012v", slice<byte>([]), "[]"),
    new("%#012v", slice<byte>([]), "[]byte{}"),
    new("%6v", slice<byte>([1, 11, 111]), "[     1     11    111]"),
    new("%06v", slice<byte>([1, 11, 111]), "[000001 000011 000111]"),
    new("%-6v", slice<byte>([1, 11, 111]), "[1      11     111   ]"),
    new("%-06v", slice<byte>([1, 11, 111]), "[1      11     111   ]"),
    new("%#v", slice<byte>([1, 11, 111]), "[]byte{0x1, 0xb, 0x6f}"),
    new("%#6v", slice<byte>([1, 11, 111]), "[]byte{   0x1,    0xb,   0x6f}"),
    new("%#06v", slice<byte>([1, 11, 111]), "[]byte{0x000001, 0x00000b, 0x00006f}"),
    new("%#-6v", slice<byte>([1, 11, 111]), "[]byte{0x1   , 0xb   , 0x6f  }"),
    new("%#-06v", slice<byte>([1, 11, 111]), "[]byte{0x1   , 0xb   , 0x6f  }"),
    // f.space should and f.plus should not have an effect with %v.
    new("% v", slice<byte>([1, 11, 111]), "[ 1  11  111]"),
    new("%+v", new byte[]{1, 11, 111}.array(), "[1 11 111]"),
    new("%# -6v", slice<byte>([1, 11, 111]), "[]byte{ 0x1  ,  0xb  ,  0x6f }"),
    new("%#+-6v", new byte[]{1, 11, 111}.array(), "[3]uint8{0x1   , 0xb   , 0x6f  }"),
    // f.space and f.plus should have an effect with %d.
    new("% d", slice<byte>([1, 11, 111]), "[ 1  11  111]"),
    new("%+d", new byte[]{1, 11, 111}.array(), "[+1 +11 +111]"),
    new("%# -6d", slice<byte>([1, 11, 111]), "[ 1      11     111  ]"),
    new("%#+-6d", new byte[]{1, 11, 111}.array(), "[+1     +11    +111  ]"),

    // floates with %v
    new("%v", 1.2345678D, "1.2345678"),
    new("%v", (float32)1.2345678F, "1.2345678"),

    // complexes with %v
    new("%v", 1D + 2D.i(), "(1+2i)"),
    new("%v", (complex64)(1F + 2F.i()), "(1+2i)"),

    // structs
    new("%v", new A(1, 2, "a", slice<nint>([1, 2])), @"{1 2 a [1 2]}"),
    new("%+v", new A(1, 2, "a", slice<nint>([1, 2])), @"{i:1 j:2 s:a x:[1 2]}"),

    // +v on structs with Stringable items
    new("%+v", new B(1, 2), @"{I:<1> j:2}"),
    new("%+v", new C(1, new B(2, 3)), @"{i:1 B:{I:<2> j:3}}"),

    // other formats on Stringable items
    new("%s", (I)23, @"<23>"),
    new("%q", (I)23, @"""<23>"""),
    new("%x", (I)23, @"3c32333e"),
    new("%#x", (I)23, @"0x3c32333e"),
    new("%# x", (I)23, @"0x3c 0x32 0x33 0x3e"),
    // Stringer applies only to string formats.
    new("%d", (I)23, @"23"),
    // Stringer applies to the extracted value.
    new("%s", reflect.ValueOf((I)23), @"<23>"),

    // go syntax
    new("%#v", new A(1, 2, "a", slice<nint>([1, 2])), @"fmt_test.A{i:1, j:0x2, s:""a"", x:[]int{1, 2}}"),
    new("%#v", @new<byte>(), "(*uint8)(0xPTR)"),
    new("%#v", new channel<nint>(0), "(chan int)(0xPTR)"),
    new("%#v", (uint64)(18446744073709551615UL), "0xffffffffffffffff"),
    new("%#v", (nint)1000000000, "1000000000"),
    new("%#v", new map<@string, nint>{["a"] = 1}, @"map[string]int{""a"":1}"),
    new("%#v", new map<@string, B>{["a"] = new(1, 2)}, @"map[string]fmt_test.B{""a"":fmt_test.B{I:1, j:2}}"),
    new("%#v", slice<@string>(["a", "b"]), @"[]string{""a"", ""b""}"),
    new("%#v", new SI(), @"fmt_test.SI{I:interface {}(nil)}"),
    new("%#v", slice<nint>(default), @"[]int(nil)"),
    new("%#v", slice<nint>([]), @"[]int{}"),
    new("%#v", Δarray.Clone(), @"[5]int{1, 2, 3, 4, 5}"),
    new("%#v", ᏑΔarray, @"&[5]int{1, 2, 3, 4, 5}"),
    new("%#v", iarray.Clone(), @"[4]interface {}{1, ""hello"", 2.5, interface {}(nil)}"),
    new("%#v", Ꮡiarray, @"&[4]interface {}{1, ""hello"", 2.5, interface {}(nil)}"),
    new("%#v", (map<nint, byte>)default, @"map[int]uint8(nil)"),
    new("%#v", new map<nint, byte>{}, @"map[int]uint8{}"),
    new("%#v", (@string)"foo", @"""foo"""),
    new("%#v", barray.Clone(), @"[5]fmt_test.renamedUint8{0x1, 0x2, 0x3, 0x4, 0x5}"),
    new("%#v", bslice, @"[]fmt_test.renamedUint8{0x1, 0x2, 0x3, 0x4, 0x5}"),
    new("%#v", slice<int32>(default), "[]int32(nil)"),
    new("%#v", 1.2345678D, "1.2345678"),
    new("%#v", (float32)1.2345678F, "1.2345678"),

    // functions
    new("%v", TestFmtInterface, "0xPTR"), // simple function
    new("%v", reflect.ValueOf(TestFmtInterface), "0xPTR"),
    new("%v", ((Func<G, @string>)(GoString)), "0xPTR"), // method expression
    new("%v", reflect.ValueOf(((Func<G, @string>)(GoString))), "0xPTR"),
    new("%v", () => ((G)23).GoString(), "0xPTR"), // method value
    new("%v", reflect.ValueOf(() => ((G)23).GoString()), "0xPTR"),
    new("%v", reflect.ValueOf((G)23).Method(0), "0xPTR"),
    new("%v", ((Func<Fn, @string>)(String)), "0xPTR"), // method of function type
    new("%v", reflect.ValueOf(((Func<Fn, @string>)(String))), "0xPTR"),
    new("%v", (fnValue).OrTypedNilFunc(), "String(fn)"), // variable of function type with String method
    new("%v", reflect.ValueOf((fnValue).OrTypedNilFunc()), "String(fn)"),
    new("%v", new Fn[]{fnValue}.array(), "[String(fn)]"), // array of function type with String method
    new("%v", reflect.ValueOf(new Fn[]{fnValue}.array()), "[String(fn)]"),
    new("%v", () => fnValue.String(), "0xPTR"), // method value from function type
    new("%v", reflect.ValueOf(() => fnValue.String()), "0xPTR"),
    new("%v", reflect.ValueOf((fnValue).OrTypedNilFunc()).Method(0), "0xPTR"),
    new("%v", (new U().u).OrTypedNilFunc(), "<nil>"), // unexported function field
    new("%v", reflect.ValueOf((new U().u).OrTypedNilFunc()), "<nil>"),
    new("%v", reflect.ValueOf(new U()).Field(0), "<nil>"),
    new("%v", (new U(fn: fnValue).fn).OrTypedNilFunc(), "String(fn)"), // unexported field of function type with String method
    new("%v", reflect.ValueOf((new U(fn: fnValue).fn).OrTypedNilFunc()), "String(fn)"),
    new("%v", reflect.ValueOf(new U(fn: fnValue)).Field(1), "<nil>"),

    // functions with go syntax
    new("%#v", TestFmtInterface, "(func(*testing.T))(0xPTR)"), // simple function
    new("%#v", reflect.ValueOf(TestFmtInterface), "(func(*testing.T))(0xPTR)"),
    new("%#v", ((Func<G, @string>)(GoString)), "(func(fmt_test.G) string)(0xPTR)"), // method expression
    new("%#v", reflect.ValueOf(((Func<G, @string>)(GoString))), "(func(fmt_test.G) string)(0xPTR)"),
    new("%#v", () => ((G)23).GoString(), "(func() string)(0xPTR)"), // method value
    new("%#v", reflect.ValueOf(() => ((G)23).GoString()), "(func() string)(0xPTR)"),
    new("%#v", reflect.ValueOf((G)23).Method(0), "(func() string)(0xPTR)"),
    new("%#v", ((Func<Fn, @string>)(String)), "(func(fmt_test.Fn) string)(0xPTR)"), // method of function type
    new("%#v", reflect.ValueOf(((Func<Fn, @string>)(String))), "(func(fmt_test.Fn) string)(0xPTR)"),
    new("%#v", (fnValue).OrTypedNilFunc(), "(fmt_test.Fn)(nil)"), // variable of function type with String method
    new("%#v", reflect.ValueOf((fnValue).OrTypedNilFunc()), "(fmt_test.Fn)(nil)"),
    new("%#v", new Fn[]{fnValue}.array(), "[1]fmt_test.Fn{(fmt_test.Fn)(nil)}"), // array of function type with String method
    new("%#v", reflect.ValueOf(new Fn[]{fnValue}.array()), "[1]fmt_test.Fn{(fmt_test.Fn)(nil)}"),
    new("%#v", () => fnValue.String(), "(func() string)(0xPTR)"), // method value from function type
    new("%#v", reflect.ValueOf(() => fnValue.String()), "(func() string)(0xPTR)"),
    new("%#v", reflect.ValueOf((fnValue).OrTypedNilFunc()).Method(0), "(func() string)(0xPTR)"),
    new("%#v", (new U().u).OrTypedNilFunc(), "(func() string)(nil)"), // unexported function field
    new("%#v", reflect.ValueOf((new U().u).OrTypedNilFunc()), "(func() string)(nil)"),
    new("%#v", reflect.ValueOf(new U()).Field(0), "(func() string)(nil)"),
    new("%#v", (new U(fn: fnValue).fn).OrTypedNilFunc(), "(fmt_test.Fn)(nil)"), // unexported field of function type with String method
    new("%#v", reflect.ValueOf((new U(fn: fnValue).fn).OrTypedNilFunc()), "(fmt_test.Fn)(nil)"),
    new("%#v", reflect.ValueOf(new U(fn: fnValue)).Field(1), "(fmt_test.Fn)(nil)"),

    // Whole number floats are printed without decimals. See Issue 27634.
    new("%#v", 1.0D, "1"),
    new("%#v", 1000000.0D, "1e+06"),
    new("%#v", (float32)1.0F, "1"),
    new("%#v", (float32)1000000.0F, "1e+06"),

    // Only print []byte and []uint8 as type []byte if they appear at the top level.
    new("%#v", slice<byte>(default), "[]byte(nil)"),
    new("%#v", slice<uint8>(default), "[]byte(nil)"),
    new("%#v", slice<byte>([]), "[]byte{}"),
    new("%#v", slice<uint8>([]), "[]byte{}"),
    new("%#v", reflect.ValueOf(slice<byte>([])), "[]uint8{}"),
    new("%#v", reflect.ValueOf(slice<uint8>([])), "[]uint8{}"),
    new("%#v", Ꮡ(slice<byte>([])), "&[]uint8{}"),
    new("%#v", Ꮡ(slice<byte>([])), "&[]uint8{}"),
    new("%#v", new byte[]{}.array(3), "[3]uint8{0x0, 0x0, 0x0}"),
    new("%#v", new uint8[]{}.array(3), "[3]uint8{0x0, 0x0, 0x0}"),

    // slices with other formats
    new("%#x", slice<nint>([1, 2, 15]), @"[0x1 0x2 0xf]"),
    new("%x", slice<nint>([1, 2, 15]), @"[1 2 f]"),
    new("%d", slice<nint>([1, 2, 15]), @"[1 2 15]"),
    new("%d", slice<byte>([1, 2, 15]), @"[1 2 15]"),
    new("%q", slice<@string>(["a", "b"]), @"[""a"" ""b""]"),
    new("% 02x", slice<byte>([1]), "01"),
    new("% 02x", slice<byte>([1, 2, 3]), "01 02 03"),

    // Padding with byte slices.
    new("%2x", slice<byte>([]), "  "),
    new("%#2x", slice<byte>([]), "  "),
    new("% 02x", slice<byte>([]), "00"),
    new("%# 02x", slice<byte>([]), "00"),
    new("%-2x", slice<byte>([]), "  "),
    new("%-02x", slice<byte>([]), "  "),
    new("%8x", slice<byte>([0xab]), "      ab"),
    new("% 8x", slice<byte>([0xab]), "      ab"),
    new("%#8x", slice<byte>([0xab]), "    0xab"),
    new("%# 8x", slice<byte>([0xab]), "    0xab"),
    new("%08x", slice<byte>([0xab]), "000000ab"),
    new("% 08x", slice<byte>([0xab]), "000000ab"),
    new("%#08x", slice<byte>([0xab]), "00000xab"),
    new("%# 08x", slice<byte>([0xab]), "00000xab"),
    new("%10x", slice<byte>([0xab, 0xcd]), "      abcd"),
    new("% 10x", slice<byte>([0xab, 0xcd]), "     ab cd"),
    new("%#10x", slice<byte>([0xab, 0xcd]), "    0xabcd"),
    new("%# 10x", slice<byte>([0xab, 0xcd]), " 0xab 0xcd"),
    new("%010x", slice<byte>([0xab, 0xcd]), "000000abcd"),
    new("% 010x", slice<byte>([0xab, 0xcd]), "00000ab cd"),
    new("%#010x", slice<byte>([0xab, 0xcd]), "00000xabcd"),
    new("%# 010x", slice<byte>([0xab, 0xcd]), "00xab 0xcd"),
    new("%-10X", slice<byte>([0xab]), "AB        "),
    new("% -010X", slice<byte>([0xab]), "AB        "),
    new("%#-10X", slice<byte>([0xab, 0xcd]), "0XABCD    "),
    new("%# -010X", slice<byte>([0xab, 0xcd]), "0XAB 0XCD "),
    // Same for strings
    new("%2x", (@string)""u8, "  "),
    new("%#2x", (@string)""u8, "  "),
    new("% 02x", (@string)""u8, "00"),
    new("%# 02x", (@string)""u8, "00"),
    new("%-2x", (@string)""u8, "  "),
    new("%-02x", (@string)""u8, "  "),
    new("%8x", ((@string)(new byte[]{0xab})), "      ab"),
    new("% 8x", ((@string)(new byte[]{0xab})), "      ab"),
    new("%#8x", ((@string)(new byte[]{0xab})), "    0xab"),
    new("%# 8x", ((@string)(new byte[]{0xab})), "    0xab"),
    new("%08x", ((@string)(new byte[]{0xab})), "000000ab"),
    new("% 08x", ((@string)(new byte[]{0xab})), "000000ab"),
    new("%#08x", ((@string)(new byte[]{0xab})), "00000xab"),
    new("%# 08x", ((@string)(new byte[]{0xab})), "00000xab"),
    new("%10x", ((@string)(new byte[]{0xab, 0xcd})), "      abcd"),
    new("% 10x", ((@string)(new byte[]{0xab, 0xcd})), "     ab cd"),
    new("%#10x", ((@string)(new byte[]{0xab, 0xcd})), "    0xabcd"),
    new("%# 10x", ((@string)(new byte[]{0xab, 0xcd})), " 0xab 0xcd"),
    new("%010x", ((@string)(new byte[]{0xab, 0xcd})), "000000abcd"),
    new("% 010x", ((@string)(new byte[]{0xab, 0xcd})), "00000ab cd"),
    new("%#010x", ((@string)(new byte[]{0xab, 0xcd})), "00000xabcd"),
    new("%# 010x", ((@string)(new byte[]{0xab, 0xcd})), "00xab 0xcd"),
    new("%-10X", ((@string)(new byte[]{0xab})), "AB        "),
    new("% -010X", ((@string)(new byte[]{0xab})), "AB        "),
    new("%#-10X", ((@string)(new byte[]{0xab, 0xcd})), "0XABCD    "),
    new("%# -010X", ((@string)(new byte[]{0xab, 0xcd})), "0XAB 0XCD "),

    // renamings
    new("%v", (renamedBool)true, "true"),
    new("%d", (renamedBool)true, "%!d(fmt_test.renamedBool=true)"),
    new("%o", (renamedInt)8, "10"),
    new("%d", (renamedInt8)(-9), "-9"),
    new("%v", (renamedInt16)10, "10"),
    new("%v", (renamedInt32)(-11), "-11"),
    new("%X", (renamedInt64)255, "FF"),
    new("%v", (renamedUint)13, "13"),
    new("%o", (renamedUint8)14, "16"),
    new("%X", (renamedUint16)15, "F"),
    new("%d", (renamedUint32)16, "16"),
    new("%X", (renamedUint64)17, "11"),
    new("%o", (renamedUintptr)18, "22"),
    new("%x", (renamedString)(@string)"thing", "7468696e67"),
    new("%d", (renamedBytes)slice<byte>([1, 2, 15]), @"[1 2 15]"),
    new("%q", (renamedBytes)slice<byte>("hello"u8), @"""hello"""),
    new("%x", slice<renamedUint8>([(rune)'h', (rune)'e', (rune)'l', (rune)'l', (rune)'o']), "68656c6c6f"),
    new("%X", slice<renamedUint8>([(rune)'h', (rune)'e', (rune)'l', (rune)'l', (rune)'o']), "68656C6C6F"),
    new("%s", slice<renamedUint8>([(rune)'h', (rune)'e', (rune)'l', (rune)'l', (rune)'o']), "hello"),
    new("%q", slice<renamedUint8>([(rune)'h', (rune)'e', (rune)'l', (rune)'l', (rune)'o']), @"""hello"""),
    new("%v", (renamedFloat32)22F, "22"),
    new("%v", (renamedFloat64)33D, "33"),
    new("%v", (renamedComplex64)(3F + 4F.i()), "(3+4i)"),
    new("%v", (renamedComplex128)(4D + -3D.i()), "(4-3i)"),

    // Formatter
    new("%x", (F)1, "<x=F(1)>"),
    new("%x", (G)2, "2"),
    new("%+v", new S((F)4, (G)5), "{F:<v=F(4)> G:5}"),

    // GoStringer
    new("%#v", (G)6, "GoString(6)"),
    new("%#v", new S((F)7, (G)8), "fmt_test.S{F:<v=F(7)>, G:GoString(8)}"),

    // %T
    new("%T", (byte)0, "uint8"),
    new("%T", reflect.ValueOf(default), "reflect.Value"),
    new("%T", (4D + -3D.i()), "complex128"),
    new("%T", (renamedComplex128)(4D + -3D.i()), "fmt_test.renamedComplex128"),
    new("%T", intVar, "int"),
    new("%6T", ᏑintVar, "  *int"),
    new("%10T", default, "     <nil>"),
    new("%-10T", default, "<nil>     "),

    // %p with pointers
    new("%p", (ж<nint>)nil, "0x0"),
    new("%#p", (ж<nint>)nil, "0"),
    new("%p", ᏑintVar, "0xPTR"),
    new("%#p", ᏑintVar, "PTR"),
    new("%p", ᏑΔarray, "0xPTR"),
    new("%p", ᏑΔslice, "0xPTR"),
    new("%8.2p", (ж<nint>)nil, "    0x00"),
    new("%-20.16p", ᏑintVar, "0xPTR  "),
    // %p on non-pointers
    new("%p", new channel<nint>(0), "0xPTR"),
    new("%p", new map<nint, nint>(), "0xPTR"),
    new("%p", () => {
    }, "0xPTR"),
    new("%p", (nint)27, "%!p(int=27)"), // not a pointer at all
    new("%p", default, "%!p(<nil>)"),   // nil on its own has no type ...
    new("%#p", default, "%!p(<nil>)"),  // ... and hence is not a pointer type.
    // pointers with specified base
    new("%b", ᏑintVar, "PTR_b"),
    new("%d", ᏑintVar, "PTR_d"),
    new("%o", ᏑintVar, "PTR_o"),
    new("%x", ᏑintVar, "PTR_x"),
    new("%X", ᏑintVar, "PTR_X"),
    // %v on pointers
    new("%v", default, "<nil>"),
    new("%#v", default, "<nil>"),
    new("%v", (ж<nint>)nil, "<nil>"),
    new("%#v", (ж<nint>)nil, "(*int)(nil)"),
    new("%v", ᏑintVar, "0xPTR"),
    new("%#v", ᏑintVar, "(*int)(0xPTR)"),
    new("%8.2v", (ж<nint>)nil, "   <nil>"),
    new("%-20.16v", ᏑintVar, "0xPTR  "),
    // string method on pointer
    new("%s", ᏑpValue, "String(p)"), // String method...
    new("%p", ᏑpValue, "0xPTR"),     // ... is not called with %p.

    // %d on Stringer should give integer if possible
    new("%s", new time.Time().Month(), "January"),
    new("%d", new time.Time().Month(), "1"),

    // erroneous things
    new("", default, "%!(EXTRA <nil>)"),
    new("", (nint)2, "%!(EXTRA int=2)"),
    new("no args", (@string)"hello", "no args%!(EXTRA string=hello)"),
    new("%s %", (@string)"hello", "hello %!(NOVERB)"),
    new("%s %.2", (@string)"hello", "hello %!(NOVERB)"),
    new("%017091901790959340919092959340919017929593813360", (nint)0, "%!(NOVERB)%!(EXTRA int=0)"),
    new("%184467440737095516170v", (nint)0, "%!(NOVERB)%!(EXTRA int=0)"),
    // Extra argument errors should format without flags set.
    new("%010.2", (@string)"12345", "%!(NOVERB)%!(EXTRA string=12345)"),

    // Test that maps with non-reflexive keys print all keys and values.
    new("%v", new map<float64, nint>{[NaN] = 1, [NaN] = 1}, "map[NaN:1 NaN:1]"),

    // Comparison of padding rules with C printf.
    /*
		C program:
		#include <stdio.h>

		char *format[] = {
			"[%.2f]",
			"[% .2f]",
			"[%+.2f]",
			"[%7.2f]",
			"[% 7.2f]",
			"[%+7.2f]",
			"[% +7.2f]",
			"[%07.2f]",
			"[% 07.2f]",
			"[%+07.2f]",
			"[% +07.2f]"
		};

		int main(void) {
			int i;
			for(i = 0; i < 11; i++) {
				printf("%s: ", format[i]);
				printf(format[i], 1.0);
				printf(" ");
				printf(format[i], -1.0);
				printf("\n");
			}
		}

		Output:
			[%.2f]: [1.00] [-1.00]
			[% .2f]: [ 1.00] [-1.00]
			[%+.2f]: [+1.00] [-1.00]
			[%7.2f]: [   1.00] [  -1.00]
			[% 7.2f]: [   1.00] [  -1.00]
			[%+7.2f]: [  +1.00] [  -1.00]
			[% +7.2f]: [  +1.00] [  -1.00]
			[%07.2f]: [0001.00] [-001.00]
			[% 07.2f]: [ 001.00] [-001.00]
			[%+07.2f]: [+001.00] [-001.00]
			[% +07.2f]: [+001.00] [-001.00]

	*/
    new("%.2f", 1.0D, "1.00"),
    new("%.2f", -1.0D, "-1.00"),
    new("% .2f", 1.0D, " 1.00"),
    new("% .2f", -1.0D, "-1.00"),
    new("%+.2f", 1.0D, "+1.00"),
    new("%+.2f", -1.0D, "-1.00"),
    new("%7.2f", 1.0D, "   1.00"),
    new("%7.2f", -1.0D, "  -1.00"),
    new("% 7.2f", 1.0D, "   1.00"),
    new("% 7.2f", -1.0D, "  -1.00"),
    new("%+7.2f", 1.0D, "  +1.00"),
    new("%+7.2f", -1.0D, "  -1.00"),
    new("% +7.2f", 1.0D, "  +1.00"),
    new("% +7.2f", -1.0D, "  -1.00"),
    new("%07.2f", 1.0D, "0001.00"),
    new("%07.2f", -1.0D, "-001.00"),
    new("% 07.2f", 1.0D, " 001.00"),
    new("% 07.2f", -1.0D, "-001.00"),
    new("%+07.2f", 1.0D, "+001.00"),
    new("%+07.2f", -1.0D, "-001.00"),
    new("% +07.2f", 1.0D, "+001.00"),
    new("% +07.2f", -1.0D, "-001.00"),

    // Complex numbers: exhaustively tested in TestComplexFormatting.
    new("%7.2f", 1D + 2D.i(), "(   1.00  +2.00i)"),
    new("%+07.2f", -1D + -2D.i(), "(-001.00-002.00i)"),

    // Use spaces instead of zero if padding to the right.
    new("%0-5s", (@string)"abc", "abc  "),
    new("%-05.1f", 1.0D, "1.0  "),

    // float and complex formatting should not change the padding width
    // for other elements. See issue 14642.
    new("%06v", slice<any>([+10.0D, (nint)10]), "[000010 000010]"),
    new("%06v", slice<any>([-10.0D, (nint)10]), "[-00010 000010]"),
    new("%06v", slice<any>([10D + 10D.i(), (nint)10]), "[(000010+00010i) 000010]"),
    new("%06v", slice<any>([-10D + 10D.i(), (nint)10]), "[(-00010+00010i) 000010]"),

    // integer formatting should not alter padding for other elements.
    new("%03.6v", slice<any>([(nint)1, 2.0D, (@string)"x"]), "[000001 002 00x]"),
    new("%03.0v", slice<any>([(nint)0, 2.0D, (@string)"x"]), "[    002 000]"),

    // Complex fmt used to leave the plus flag set for future entries in the array
    // causing +2+0i and +3+0i instead of 2+0i and 3+0i.
    new("%v", slice<complex64>([1F, 2F, 3F]), "[(1+0i) (2+0i) (3+0i)]"),
    new("%v", slice<complex128>([1D, 2D, 3D]), "[(1+0i) (2+0i) (3+0i)]"),

    // Incomplete format specification caused crash.
    new("%.", (nint)3, "%!.(int=3)"),

    // Padding for complex numbers. Has been bad, then fixed, then bad again.
    new("%+10.2f", 104.66D + 440.51D.i(), "(   +104.66   +440.51i)"),
    new("%+10.2f", -104.66D + 440.51D.i(), "(   -104.66   +440.51i)"),
    new("%+10.2f", 104.66D + -440.51D.i(), "(   +104.66   -440.51i)"),
    new("%+10.2f", -104.66D + -440.51D.i(), "(   -104.66   -440.51i)"),
    new("%+010.2f", 104.66D + 440.51D.i(), "(+000104.66+000440.51i)"),
    new("%+010.2f", -104.66D + 440.51D.i(), "(-000104.66+000440.51i)"),
    new("%+010.2f", 104.66D + -440.51D.i(), "(+000104.66-000440.51i)"),
    new("%+010.2f", -104.66D + -440.51D.i(), "(-000104.66-000440.51i)"),

    // []T where type T is a byte with a Stringer method.
    new("%v", byteStringerSlice, "[X X X X X]"),
    new("%s", byteStringerSlice, "hello"),
    new("%q", byteStringerSlice, "\"hello\""),
    new("%x", byteStringerSlice, "68656c6c6f"),
    new("%X", byteStringerSlice, "68656C6C6F"),
    new("%#v", byteStringerSlice, "[]fmt_test.byteStringer{0x68, 0x65, 0x6c, 0x6c, 0x6f}"),

    // And the same for Formatter.
    new("%v", byteFormatterSlice, "[X X X X X]"),
    new("%s", byteFormatterSlice, "hello"),
    new("%q", byteFormatterSlice, "\"hello\""),
    new("%x", byteFormatterSlice, "68656c6c6f"),
    new("%X", byteFormatterSlice, "68656C6C6F"),
    // This next case seems wrong, but the docs say the Formatter wins here.
    new("%#v", byteFormatterSlice, "[]fmt_test.byteFormatter{X, X, X, X, X}"),

    // pp.WriteString
    new("%s", (writeStringFormatter)(@string)""u8, "******"),
    new("%s", (writeStringFormatter)(@string)"xyz", "***xyz***"),
    new("%s", (writeStringFormatter)(@string)"⌘/⌘", "***⌘/⌘***"),

    // reflect.Value handled specially in Go 1.5, making it possible to
    // see inside non-exported fields (which cannot be accessed with Interface()).
    // Issue 8965.
    new("%v", reflect.ValueOf(new A()).Field(0).String(), "<int Value>"), // Equivalent to the old way.
    new("%v", reflect.ValueOf(new A()).Field(0), "0"),                    // Sees inside the field.

    // verbs apply to the extracted value too.
    new("%s", reflect.ValueOf("hello"), "hello"),
    new("%q", reflect.ValueOf("hello"), @"""hello"""),
    new("%#04x", reflect.ValueOf((nint)256), "0x0100"),

    // invalid reflect.Value doesn't crash.
    new("%v", new reflectꓸValue(), "<invalid reflect.Value>"),
    new("%v", Ꮡ(new reflectꓸValue()), "<invalid Value>"),
    new("%v", new SI(new reflectꓸValue()), "{<invalid Value>}"),

    // Tests to check that not supported verbs generate an error string.
    new("%☠", default, "%!☠(<nil>)"),
    new("%☠", (any)default, "%!☠(<nil>)"),
    new("%☠", (nint)0, "%!☠(int=0)"),
    new("%☠", (nuint)0, "%!☠(uint=0)"),
    new("%☠", slice<byte>([0, 1]), "[%!☠(uint8=0) %!☠(uint8=1)]"),
    new("%☠", slice<uint8>([0, 1]), "[%!☠(uint8=0) %!☠(uint8=1)]"),
    new("%☠", new byte[]{0}.array(), "[%!☠(uint8=0)]"),
    new("%☠", new uint8[]{0}.array(), "[%!☠(uint8=0)]"),
    new("%☠", (@string)"hello", "%!☠(string=hello)"),
    new("%☠", 1.2345678D, "%!☠(float64=1.2345678)"),
    new("%☠", (float32)1.2345678F, "%!☠(float32=1.2345678)"),
    new("%☠", 1.2345678D + 1.2345678D.i(), "%!☠(complex128=(1.2345678+1.2345678i))"),
    new("%☠", (complex64)(1.2345678F + 1.2345678F.i()), "%!☠(complex64=(1.2345678+1.2345678i))"),
    new("%☠", ᏑintVar, "%!☠(*int=0xPTR)"),
    new("%☠", new channel<nint>(0), "%!☠(chan int=0xPTR)"),
    new("%☠", () => {
    }, "%!☠(func()=0xPTR)"),
    new("%☠", reflect.ValueOf((renamedInt)0), "%!☠(fmt_test.renamedInt=0)"),
    new("%☠", new SI((renamedInt)0), "{%!☠(fmt_test.renamedInt=0)}"),
    new("%☠", Ꮡ(slice<any>([(I)1, (G)2])), "&[%!☠(fmt_test.I=1) %!☠(fmt_test.G=2)]"),
    new("%☠", new SI(Ꮡ(slice<any>([(I)1, (G)2]))), "{%!☠(*[]interface {}=&[1 2])}"),
    new("%☠", new reflectꓸValue(), "<invalid reflect.Value>"),
    new("%☠", new map<float64, nint>{[NaN] = 1}, "map[%!☠(float64=NaN):%!☠(int=1)]")
]);

// zeroFill generates zero-filled strings of the specified width. The length
// of the suffix (but not the prefix) is compensated for in the width calculation.
internal static @string zeroFill(@string prefix, nint width, @string suffix) {
    return prefix + strings.Repeat("0", width - len(suffix)) + suffix;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string ptrˢ => ref ptrˢᵇ;
private static readonly @string ptrˢᵇ = "PTR"u8;
internal static ref readonly @string ptrBˢ => ref ptrBˢᵇ;
private static readonly @string ptrBˢᵇ = "PTR_b"u8;
internal static ref readonly @string ptrOˢ => ref ptrOˢᵇ;
private static readonly @string ptrOˢᵇ = "PTR_o"u8;
internal static ref readonly @string ptrDˢ => ref ptrDˢᵇ;
private static readonly @string ptrDˢᵇ = "PTR_d"u8;
internal static ref readonly @string ptrXˢ => ref ptrXˢᵇ;
private static readonly @string ptrXˢᵇ = "PTR_x"u8;
internal static ref readonly @string ptrXˢ2 => ref ptrXˢ2ᵇ;
private static readonly @string ptrXˢ2ᵇ = "PTR_X"u8;

public static void TestSprintf(ж<Δtesting.T> t) {
    foreach (var (_, tt) in fmtTests) {
        @string s = Sprintf(tt.fmt, tt.val);
        nint i = strings.Index(tt.@out, ptrˢ);
        if (i >= 0 && i < len(s)) {
            @string pattern = default;
            @string chars = default;
            switch (ᐧ) {
            case {} when strings.HasPrefix(tt.@out.slice(i), ptrBˢ): {
                pattern = ptrBˢ;
                chars = "01";
                break;
            }
            case {} when strings.HasPrefix(tt.@out.slice(i), ptrOˢ): {
                pattern = ptrOˢ;
                chars = "01234567";
                break;
            }
            case {} when strings.HasPrefix(tt.@out.slice(i), ptrDˢ): {
                pattern = ptrDˢ;
                chars = "0123456789";
                break;
            }
            case {} when strings.HasPrefix(tt.@out.slice(i), ptrXˢ): {
                pattern = ptrXˢ;
                chars = "0123456789abcdef";
                break;
            }
            case {} when strings.HasPrefix(tt.@out.slice(i), ptrXˢ2): {
                pattern = ptrXˢ2;
                chars = "0123456789ABCDEF";
                break;
            }
            default: {
                pattern = ptrˢ;
                chars = "0123456789abcdefABCDEF";
                break;
            }}
            @string p = s.slice(0, i) + pattern;
            for (nint j = i; j < len(s); j++) {
                if (!strings.ContainsRune(chars, (rune)s[j])) {
                    p += s.slice(j);
                    break;
                }
            }
            s = p;
        }
        if (s != tt.@out) {
            {
                var (_, ok) = tt.val._<@string>(ᐧ); if (ok) {
                    // Don't requote the already-quoted strings.
                    // It's too confusing to read the errors.
                    t.Errorf("Sprintf(%q, %q) = <%s> want <%s>", tt.fmt, tt.val, s, tt.@out);
                } else {
                    t.Errorf("Sprintf(%q, %v) = %q want %q", tt.fmt, tt.val, s, tt.@out);
                }
            }
        }
    }
}

// TestComplexFormatting checks that a complex always formats to the same
// thing as if done by hand with two singleton prints.
public static void TestComplexFormatting(ж<Δtesting.T> t) {
    slice<bool> yesNo = slice<bool>([true, false]);
    slice<float64> values = slice<float64>([1D, 0D, -1D, posInf, negInf, NaN]);
    foreach (var (_, plus) in yesNo) {
        foreach (var (_, zero) in yesNo) {
            foreach (var (_, space) in yesNo) {
                foreach (var (_, @char) in (@string)"fFeEgG") {
                    @string realFmt = "%";
                    if (zero) {
                        realFmt += "0"u8;
                    }
                    if (space) {
                        realFmt += " "u8;
                    }
                    if (plus) {
                        realFmt += "+"u8;
                    }
                    realFmt += "10.2"u8;
                    realFmt += ((@string)@char);
                    // Imaginary part always has a sign, so force + and ignore space.
                    @string imagFmt = "%";
                    if (zero) {
                        imagFmt += "0"u8;
                    }
                    imagFmt += "+"u8;
                    imagFmt += "10.2"u8;
                    imagFmt += ((@string)@char);
                    foreach (var (_, realValue) in values) {
                        foreach (var (_, imagValue) in values) {
                            @string one = Sprintf(realFmt, complex(realValue, imagValue));
                            @string two = Sprintf("("u8 + realFmt + imagFmt + "i)"u8, realValue, imagValue);
                            if (one != two) {
                                t.Error(f, one, two);
                            }
                        }
                    }
                }
            }
        }
    }
}

partial struct SE /*[]any*/; // slice of empty; notational compactness.

partial struct reorderTestsᴛ1 /*dyn*/ {
    internal @string fmt;
    internal SE val;
    internal @string @out;
}
internal static slice<reorderTestsᴛ1> reorderTests = slice<reorderTestsᴛ1>([
    new("%[1]d", new SE(new any[]{(nint)1}.slice()), "1"),
    new("%[2]d", new SE(new any[]{(nint)2, (nint)1}.slice()), "1"),
    new("%[2]d %[1]d", new SE(new any[]{(nint)1, (nint)2}.slice()), "2 1"),
    new("%[2]*[1]d", new SE(new any[]{(nint)2, (nint)5}.slice()), "    2"),
    new("%6.2f", new SE(new any[]{12.0D}.slice()), " 12.00"), // Explicit version of next line.
    new("%[3]*.[2]*[1]f", new SE(new any[]{12.0D, (nint)2, (nint)6}.slice()), " 12.00"),
    new("%[1]*.[2]*[3]f", new SE(new any[]{(nint)6, (nint)2, 12.0D}.slice()), " 12.00"),
    new("%10f", new SE(new any[]{12.0D}.slice()), " 12.000000"),
    new("%[1]*[3]f", new SE(new any[]{(nint)10, (nint)99, 12.0D}.slice()), " 12.000000"),
    new("%.6f", new SE(new any[]{12.0D}.slice()), "12.000000"), // Explicit version of next line.
    new("%.[1]*[3]f", new SE(new any[]{(nint)6, (nint)99, 12.0D}.slice()), "12.000000"),
    new("%6.f", new SE(new any[]{12.0D}.slice()), "    12"), //  // Explicit version of next line; empty precision means zero.
    new("%[1]*.[3]f", new SE(new any[]{(nint)6, (nint)3, 12.0D}.slice()), "    12"),
    // An actual use! Print the same arguments twice.
    new("%d %d %d %#[1]o %#o %#o", new SE(new any[]{(nint)11, (nint)12, (nint)13}.slice()), "11 12 13 013 014 015"),

    // Erroneous cases.
    new("%[d", new SE(new any[]{(nint)2, (nint)1}.slice()), "%!d(BADINDEX)"),
    new("%]d", new SE(new any[]{(nint)2, (nint)1}.slice()), "%!](int=2)d%!(EXTRA int=1)"),
    new("%[]d", new SE(new any[]{(nint)2, (nint)1}.slice()), "%!d(BADINDEX)"),
    new("%[-3]d", new SE(new any[]{(nint)2, (nint)1}.slice()), "%!d(BADINDEX)"),
    new("%[99]d", new SE(new any[]{(nint)2, (nint)1}.slice()), "%!d(BADINDEX)"),
    new("%[3]", new SE(new any[]{(nint)2, (nint)1}.slice()), "%!(NOVERB)"),
    new("%[1].2d", new SE(new any[]{(nint)5, (nint)6}.slice()), "%!d(BADINDEX)"),
    new("%[1]2d", new SE(new any[]{(nint)2, (nint)1}.slice()), "%!d(BADINDEX)"),
    new("%3.[2]d", new SE(new any[]{(nint)7}.slice()), "%!d(BADINDEX)"),
    new("%.[2]d", new SE(new any[]{(nint)7}.slice()), "%!d(BADINDEX)"),
    new("%d %d %d %#[1]o %#o %#o %#o", new SE(new any[]{(nint)11, (nint)12, (nint)13}.slice()), "11 12 13 013 014 015 %!o(MISSING)"),
    new("%[5]d %[2]d %d", new SE(new any[]{(nint)1, (nint)2, (nint)3}.slice()), "%!d(BADINDEX) 2 3"),
    new("%d %[3]d %d", new SE(new any[]{(nint)1, (nint)2}.slice()), "1 %!d(BADINDEX) 2"), // Erroneous index does not affect sequence.
    new("%.[]", new SE(new any[]{}.slice()), "%!](BADINDEX)"),                            // Issue 10675
    new("%.-3d", new SE(new any[]{(nint)42}.slice()), "%!-(int=42)3d"),                   // TODO: Should this set return better error messages?
    new("%2147483648d", new SE(new any[]{(nint)42}.slice()), "%!(NOVERB)%!(EXTRA int=42)"),
    new("%-2147483648d", new SE(new any[]{(nint)42}.slice()), "%!(NOVERB)%!(EXTRA int=42)"),
    new("%.2147483648d", new SE(new any[]{(nint)42}.slice()), "%!(NOVERB)%!(EXTRA int=42)")
]);

public static void TestReorder(ж<Δtesting.T> t) {
    foreach (var (_, tt) in reorderTests) {
        @string s = Sprintf(tt.fmt, tt.val.ꓸꓸꓸ);
        if (s != tt.@out) {
            t.Errorf("Sprintf(%q, %v) = <%s> want <%s>", tt.fmt, tt.val, s, tt.@out);
        }
    }
}

public static void BenchmarkSprintfPadding(ж<Δtesting.B> b) {
    b.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("%16f"u8, 1.0D);
        }
    });
}

public static void BenchmarkSprintfEmpty(ж<Δtesting.B> b) {
    b.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf(""u8);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object helloˢ = (@string)"hello"u8;

public static void BenchmarkSprintfString(ж<Δtesting.B> b) {
    b.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("%s"u8, helloˢ);
        }
    });
}

public static void BenchmarkSprintfTruncateString(ж<Δtesting.B> b) {
    b.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("%.3s"u8, "日本語日本語日本語日本語");
        }
    });
}

public static void BenchmarkSprintfTruncateBytes(ж<Δtesting.B> b) {
    any bytes = slice<byte>("日本語日本語日本語日本語"u8);
    var bytesʗ1 = bytes;
    b.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("%.3s"u8, bytesʗ1);
        }
    });
}

public static void BenchmarkSprintfSlowParsingPath(ж<Δtesting.B> b) {
    b.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("%.v"u8, (any)(default));
        }
    });
}

public static void BenchmarkSprintfQuoteString(ж<Δtesting.B> b) {
    b.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("%q"u8, "日本語日本語日本語");
        }
    });
}

public static void BenchmarkSprintfInt(ж<Δtesting.B> b) {
    b.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("%d"u8, (nint)5);
        }
    });
}

public static void BenchmarkSprintfIntInt(ж<Δtesting.B> b) {
    b.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("%d %d"u8, (nint)5, (nint)6);
        }
    });
}

public static void BenchmarkSprintfPrefixedInt(ж<Δtesting.B> b) {
    b.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("This is some meaningless prefix text that needs to be scanned %d"u8, (nint)6);
        }
    });
}

public static void BenchmarkSprintfFloat(ж<Δtesting.B> b) {
    b.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("%g"u8, 5.23184D);
        }
    });
}

public static void BenchmarkSprintfComplex(ж<Δtesting.B> b) {
    b.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("%f"u8, 5.23184D + 5.23184D.i());
        }
    });
}

public static void BenchmarkSprintfBoolean(ж<Δtesting.B> b) {
    b.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("%t"u8, true);
        }
    });
}

public static void BenchmarkSprintfHexString(ж<Δtesting.B> b) {
    b.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("% #x"u8, "0123456789abcdef");
        }
    });
}

public static void BenchmarkSprintfHexBytes(ж<Δtesting.B> b) {
    var data = slice<byte>("0123456789abcdef"u8);
    var dataʗ1 = data;
    b.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("% #x"u8, dataʗ1);
        }
    });
}

public static void BenchmarkSprintfBytes(ж<Δtesting.B> b) {
    var data = slice<byte>("0123456789abcdef"u8);
    var dataʗ1 = data;
    b.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("%v"u8, dataʗ1);
        }
    });
}

public static void BenchmarkSprintfStringer(ж<Δtesting.B> b) {
    I stringer = (I)12345;
    b.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("%v"u8, stringer);
        }
    });
}

public static void BenchmarkSprintfStructure(ж<Δtesting.B> b) {
    var s = Ꮡ(slice<any>([new SI((nint)12345), new map<nint, @string>{[0] = "hello"}]));
    var sʗ1 = s;
    b.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("%#v"u8, sʗ1.OrTypedNil());
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object worldˢ = (@string)"world"u8;

public static void BenchmarkManyArgs(ж<Δtesting.B> b) {
    b.RunParallel((ж<Δtesting.PB> pb) => {
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        while (pb.Next()) {
            buf.Reset();
            Fprintf(new fmt_test_package.bytes_BufferжWriter(Ꮡbuf), "%2d/%2d/%2d %d:%d:%d %s %s\n"u8, (nint)3, (nint)4, (nint)5, (nint)11, (nint)12, (nint)13, helloˢ, worldˢ);
        }
    });
}

public static void BenchmarkFprintInt(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    for (nint i = 0; i < b.N; i++) {
        buf.Reset();
        Fprint(new fmt_test_package.bytes_BufferжWriter(Ꮡbuf), (nint)123456);
    }
}

public static void BenchmarkFprintfBytes(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var data = slice<byte>((@string)"0123456789"u8);
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    for (nint i = 0; i < b.N; i++) {
        buf.Reset();
        Fprintf(new fmt_test_package.bytes_BufferжWriter(Ꮡbuf), "%s"u8, data);
    }
}

public static void BenchmarkFprintIntNoAlloc(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    any x = (nint)123456;
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    for (nint i = 0; i < b.N; i++) {
        buf.Reset();
        Fprint(new fmt_test_package.bytes_BufferжWriter(Ꮡbuf), x);
    }
}

internal static readonly ж<bytes.Buffer> ᏑmallocBuf = new StaticBox<bytes.Buffer>(default(bytes.Buffer));
internal static ref bytes.Buffer mallocBuf => ref ((StaticBox<bytes.Buffer>)ᏑmallocBuf).ValueSlot;

internal static ж<nint> mallocPointer; // A pointer so we know the interface value won't allocate.

internal static any sink;

partial struct mallocTestᴛ1 /*dyn*/ {
    internal nint count;
    internal @string desc;
    internal Action fn;
}

internal partial struct mallocTest_P /*dyn*/ {
    internal nint x, y;
}

internal partial struct mallocTest_Pᴛ1 /*dyn*/ {
    internal nint x, y;
}
internal static slice<mallocTestᴛ1> mallocTest = slice<mallocTestᴛ1>([
    new(0, @"Sprintf("""")", () => {
        _ = Sprintf(""u8);
    }),
    new(1, @"Sprintf(""xxx"")", () => {
        _ = Sprintf("xxx"u8);
    }),
    new(0, @"Sprintf(""%x"")", () => {
        _ = Sprintf("%x"u8, (nint)7);
    }),
    new(1, @"Sprintf(""%x"")", () => {
        _ = Sprintf("%x"u8, (nint)(1 << 16));
    }),
    new(3, @"Sprintf(""%80000s"")", () => {
        _ = Sprintf("%80000s"u8, "hello");
    }), // large buffer (>64KB)
    new(1, @"Sprintf(""%s"")", () => {
        _ = Sprintf("%s"u8, "hello");
    }),
    new(1, @"Sprintf(""%x %x"")", () => {
        _ = Sprintf("%x %x"u8, (nint)7, (nint)112);
    }),
    new(1, @"Sprintf(""%g"")", () => {
        _ = Sprintf("%g"u8, (float32)3.14159F);
    }),
    new(0, @"Fprintf(buf, ""%s"")", () => {
        mallocBuf.Reset();
        Fprintf(new fmt_test_package.bytes_BufferжWriter(ᏑmallocBuf), "%s"u8, "hello");
    }),
    new(0, @"Fprintf(buf, ""%s"")", () => {
        mallocBuf.Reset();
        @string s = "hello";
        Fprintf(new fmt_test_package.bytes_BufferжWriter(ᏑmallocBuf), "%s"u8, s);
    }),
    new(1, @"Fprintf(buf, ""%s"")", () => {
        mallocBuf.Reset();
        @string s = "hello";
        Fprintf(new fmt_test_package.bytes_BufferжWriter(ᏑmallocBuf), "%s"u8, noliteral(s));
    }),
    new(0, @"Fprintf(buf, ""%x"")", () => {
        mallocBuf.Reset();
        Fprintf(new fmt_test_package.bytes_BufferжWriter(ᏑmallocBuf), "%x"u8, (nint)7);
    }),
    new(0, @"Fprintf(buf, ""%x"")", () => {
        mallocBuf.Reset();
        Fprintf(new fmt_test_package.bytes_BufferжWriter(ᏑmallocBuf), "%x"u8, (nint)(1 << 16));
    }),
    new(0, @"Fprintf(buf, ""%x"")", () => {
        mallocBuf.Reset();
        nint i = 1 << 16;
        Fprintf(new fmt_test_package.bytes_BufferжWriter(ᏑmallocBuf), "%x"u8, i);
    }),
    new(1, @"Fprintf(buf, ""%x"")", () => {
        mallocBuf.Reset();
        nint i = 1 << 16;
        Fprintf(new fmt_test_package.bytes_BufferжWriter(ᏑmallocBuf), "%x"u8, noliteral(i));
    }),
    new(4, @"Fprintf(buf, ""%v"")", () => {
        mallocBuf.Reset();
        var s = slice<nint>([1, 2]);
        Fprintf(new fmt_test_package.bytes_BufferжWriter(ᏑmallocBuf), "%v"u8, s);
    }),
    new(0, @"Fprintf(buf, ""%v"")", () => {
        mallocBuf.Reset();
        Fprintf(new fmt_test_package.bytes_BufferжWriter(ᏑmallocBuf), "%v"u8, new mallocTest_P(1, 2));
    }),
    new(1, @"Fprintf(buf, ""%v"")", () => {
        mallocBuf.Reset();
        Fprintf(new fmt_test_package.bytes_BufferжWriter(ᏑmallocBuf), "%v"u8, noliteral(new mallocTest_Pᴛ1(1, 2)));
    }),
    new(2, @"Fprintf(buf, ""%80000s"")", () => {
        mallocBuf.Reset();
        Fprintf(new fmt_test_package.bytes_BufferжWriter(ᏑmallocBuf), "%80000s"u8, "hello");
    }), // large buffer (>64KB)
    // If the interface value doesn't need to allocate, amortized allocation overhead should be zero.
    new(0, @"Fprintf(buf, ""%x %x %x"")", () => {
        mallocBuf.Reset();
        Fprintf(new fmt_test_package.bytes_BufferжWriter(ᏑmallocBuf), "%x %x %x"u8, mallocPointer.OrTypedNil(), mallocPointer.OrTypedNil(), mallocPointer.OrTypedNil());
    }),
    new(0, @"Errorf(""hello"")", () => {
        _ = Errorf("hello"u8);
    }),
    new(2, @"Errorf(""hello: %x"")", () => {
        _ = Errorf("hello: %x"u8, mallocPointer.OrTypedNil());
    }),
    new(1, @"sink = Errorf(""hello"")", () => {
        sink = Errorf("hello"u8);
    }),
    new(2, @"sink = Errorf(""hello: %x"")", () => {
        sink = Errorf("hello: %x"u8, mallocPointer.OrTypedNil());
    })
]);

internal static bytes.Buffer _ᴛ1ʗ;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingMallocCountInˢ = (@string)"skipping malloc count in short mode"u8;
internal static readonly object skippingMallocCountUnderˢ = (@string)"skipping malloc count under race detector"u8;

public static void TestCountMallocs(ж<Δtesting.T> t) {
    switch (ᐧ) {
    case {} when Δtesting.Short(): {
        t.Skip(skippingMallocCountInˢ);
        break;
    }
    case {} when race.Enabled: {
        t.Skip(skippingMallocCountUnderˢ);
        break;
    }}
    foreach (var (_, mt) in mallocTest) {
        var mallocs = Δtesting.AllocsPerRun(100, mt.fn);
        {
            var (got, max) = (mallocs, (float64)mt.count); if (got != max) {
                t.Errorf("%s: got %v allocs, want %v", mt.desc, got, max);
            }
        }
    }
}

partial struct flagPrinter {
}

internal static void Format(this flagPrinter _, fmt.State f, rune c) {
    @string s = "%";
    for (nint i = 0; i < 128; i++) {
        if (f.Flag(i)) {
            s += ((@string)(rune)i);
        }
    }
    {
        var (w, ok) = f.Width(); if (ok) {
            s += Sprintf("%d"u8, w);
        }
    }
    {
        var (p, ok) = f.Precision(); if (ok) {
            s += Sprintf(".%d"u8, p);
        }
    }
    s += ((@string)c);
    Δio.WriteString(new fmt_test_package.fmt_StateᴠWriter(f), "["u8 + s + "]"u8);
}

partial struct flagtestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal @string @out;
}
internal static slice<flagtestsᴛ1> flagtests = slice<flagtestsᴛ1>([
    new("%a", "[%a]"),
    new("%-a", "[%-a]"),
    new("%+a", "[%+a]"),
    new("%#a", "[%#a]"),
    new("% a", "[% a]"),
    new("%0a", "[%0a]"),
    new("%1.2a", "[%1.2a]"),
    new("%-1.2a", "[%-1.2a]"),
    new("%+1.2a", "[%+1.2a]"),
    new("%-+1.2a", "[%+-1.2a]"),
    new("%-+1.2abc", "[%+-1.2a]bc"),
    new("%-1.2abc", "[%-1.2a]bc"),
    new("%-0abc", "[%-0a]bc")
]);

public static void TestFlagParser(ж<Δtesting.T> t) {
    ref var flagprinter = ref heap(new flagPrinter(), out var Ꮡflagprinter);
    foreach (var (_, tt) in flagtests) {
        @string s = Sprintf(tt.@in, Ꮡflagprinter);
        if (s != tt.@out) {
            t.Errorf("Sprintf(%q, &flagprinter) => %q, want %q", tt.@in, s, tt.@out);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string abcˢ => ref abcˢᵇ;
private static readonly @string abcˢᵇ = "abc"u8;
internal static ref readonly @string defˢ => ref defˢᵇ;
private static readonly @string defˢᵇ = "def"u8;

internal partial struct TestStructPrinter_T /*dyn*/ {
    internal @string a;
    internal @string b;
    internal nint c;
}

internal partial struct TestStructPrinter_type /*dyn*/ {
    internal @string fmt;
    internal @string @out;
}

public static void TestStructPrinter(ж<Δtesting.T> t) {
    ref var s = ref heap(new TestStructPrinter_T(), out var Ꮡs);
    s.a = abcˢ;
    s.b = defˢ;
    s.c = 123;
    slice<TestStructPrinter_type> tests = slice<TestStructPrinter_type>([
        new("%v", "{abc def 123}"),
        new("%+v", "{a:abc b:def c:123}"),
        new("%#v", @"fmt_test.T{a:""abc"", b:""def"", c:123}")
    ]);
    foreach (var (_, tt) in tests) {
        @string @out = Sprintf(tt.fmt, s);
        if (@out != tt.@out) {
            t.Errorf("Sprintf(%q, s) = %#q, want %#q", tt.fmt, @out, tt.@out);
        }
        // The same but with a pointer.
        @out = Sprintf(tt.fmt, Ꮡs);
        if (@out != "&"u8 + tt.@out) {
            t.Errorf("Sprintf(%q, &s) = %#q, want %#q", tt.fmt, @out, "&" + tt.@out);
        }
    }
}

public static void TestSlicePrinter(ж<Δtesting.T> t) {
    ref var Δslice = ref heap<slice<nint>>(slice<nint>([]), out var Ꮡslice);
    @string s = Sprint(Δslice);
    if (s != "[]"u8) {
        t.Errorf("empty slice printed as %q not %q", s, (@string)"[]");
    }
    Δslice = slice<nint>([1, 2, 3]);
    s = Sprint(Δslice);
    if (s != "[1 2 3]"u8) {
        t.Errorf("slice: got %q expected %q", s, (@string)"[1 2 3]");
    }
    s = Sprint(Ꮡslice);
    if (s != "&[1 2 3]"u8) {
        t.Errorf("&slice: got %q expected %q", s, (@string)"&[1 2 3]");
    }
}

// presentInMap checks map printing using substrings so we don't depend on the
// print order.
internal static void presentInMap(@string s, slice<@string> a, ж<Δtesting.T> t) {
    for (nint i = 0; i < len(a); i++) {
        nint loc = strings.Index(s, a[i]);
        if (loc < 0) {
            t.Errorf("map print: expected to find %q in %q", a[i], s);
        }
        // make sure the match ends here
        loc += len(a[i]);
        if (loc >= len(s) || (s[loc] != ' ' && s[loc] != ']')) {
            t.Errorf("map print: %q not properly terminated in %q", a[i], s);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object mapˢ = (@string)"map[]"u8;

public static void TestMapPrinter(ж<Δtesting.T> t) {
    var m0 = new map<nint, @string>();
    @string s = Sprint(m0);
    if (s != "map[]"u8) {
        t.Errorf("empty map printed as %q not %q", s, mapˢ);
    }
    ref var m1 = ref heap<map<nint, @string>>(new map<nint, @string>{[1] = "one", [2] = "two", [3] = "three"}, out var Ꮡm1);
    var a = slice<@string>(["1:one", "2:two", "3:three"]);
    presentInMap(Sprintf("%v"u8, m1), a, t);
    presentInMap(Sprint(m1), a, t);
    // Pointer to map prints the same but with initial &.
    if (!strings.HasPrefix(Sprint(Ꮡm1), "&")) {
        t.Errorf("no initial & for address of map");
    }
    presentInMap(Sprintf("%v"u8, Ꮡm1), a, t);
    presentInMap(Sprint(Ꮡm1), a, t);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string emptyMapStrᶜ => ref emptyMapStrᶜᵇ;
private static readonly @string emptyMapStrᶜᵇ = "map[]"u8;

public static void TestEmptyMap(ж<Δtesting.T> t) {
    @string emptyMapStr = emptyMapStrᶜ;
    map<@string, nint> m = default;
    @string s = Sprint(m);
    if (s != emptyMapStr) {
        t.Errorf("nil map printed as %q not %q", s, emptyMapStr);
    }
    m = new map<@string, nint>();
    s = Sprint(m);
    if (s != emptyMapStr) {
        t.Errorf("empty map printed as %q not %q", s, emptyMapStr);
    }
}

// TestBlank checks that Sprint (and hence Print, Fprint) puts spaces in the
// right places, that is, between arg pairs in which neither is a string.
public static void TestBlank(ж<Δtesting.T> t) {
    @string got = Sprint("<", (nint)1, ">:", (nint)1, (nint)2, (nint)3, "!");
    @string expect = "<1>:1 2 3!";
    if (got != expect) {
        t.Errorf("got %q expected %q", got, expect);
    }
}

// TestBlankln checks that Sprintln (and hence Println, Fprintln) puts spaces in
// the right places, that is, between all arg pairs.
public static void TestBlankln(ж<Δtesting.T> t) {
    @string got = Sprintln("<", (nint)1, ">:", (nint)1, (nint)2, (nint)3, "!");
    @string expect = "< 1 >: 1 2 3 !\n";
    if (got != expect) {
        t.Errorf("got %q expected %q", got, expect);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string vF1ˢ => ref vF1ˢᵇ;
private static readonly @string vF1ˢᵇ = "<v=F(1)>\n"u8;

// TestFormatterPrintln checks Formatter with Sprint, Sprintln, Sprintf.
public static void TestFormatterPrintln(ж<Δtesting.T> t) {
    F f = (F)1;
    @string expect = vF1ˢ;
    @string s = Sprint(f, "\n");
    if (s != expect) {
        t.Errorf("Sprint wrong with Formatter: expected %q got %q", expect, s);
    }
    s = Sprintln(f);
    if (s != expect) {
        t.Errorf("Sprintln wrong with Formatter: expected %q got %q", expect, s);
    }
    s = Sprintf("%v\n"u8, f);
    if (s != expect) {
        t.Errorf("Sprintf wrong with Formatter: expected %q got %q", expect, s);
    }
}

internal static slice<any> args(params ꓸꓸꓸany aʗp) {
    var a = aʗp.slice();

    return a;
}

partial struct startestsᴛ1 /*dyn*/ {
    internal @string fmt;
    internal slice<any> @in;
    internal @string @out;
}
internal static slice<startestsᴛ1> startests = slice<startestsᴛ1>([
    new("%*d", args((nint)4, (nint)42), "  42"),
    new("%-*d", args((nint)4, (nint)42), "42  "),
    new("%*d", args((nint)(-4), (nint)42), "42  "),
    new("%-*d", args((nint)(-4), (nint)42), "42  "),
    new("%.*d", args((nint)4, (nint)42), "0042"),
    new("%*.*d", args((nint)8, (nint)4, (nint)42), "    0042"),
    new("%0*d", args((nint)4, (nint)42), "0042"),
    // Some non-int types for width. (Issue 10732).
    new("%0*d", args((nuint)4, (nint)42), "0042"),
    new("%0*d", args((uint64)4, (nint)42), "0042"),
    new("%0*d", args((rune)'\x04', (nint)42), "0042"),
    new("%0*d", args((uintptr)4, (nint)42), "0042"),

    // erroneous
    new("%*d", args((any)(default), (nint)42), "%!(BADWIDTH)42"),
    new("%*d", args((nint)10000000, (nint)42), "%!(BADWIDTH)42"),
    new("%*d", args((nint)(-10000000), (nint)42), "%!(BADWIDTH)42"),
    new("%.*d", args((any)(default), (nint)42), "%!(BADPREC)42"),
    new("%.*d", args((nint)(-1), (nint)42), "%!(BADPREC)42"),
    new("%.*d", args((nint)10000000, (nint)42), "%!(BADPREC)42"),
    new("%.*d", args((nuint)10000000, (nint)42), "%!(BADPREC)42"),
    new("%.*d", args((uint64)((uint64)1 << 63), (nint)42), "%!(BADPREC)42"),        // Huge negative (-inf).
    new("%.*d", args((uint64)(18446744073709551615UL), (nint)42), "%!(BADPREC)42"), // Small negative (-1).
    new("%*d", args((nint)5, "foo"), "%!d(string=  foo)"),
    new("%*% %d", args((nint)20, (nint)5), "% 5"),
    new("%*", args((nint)4), "%!(NOVERB)")
]);

public static void TestWidthAndPrecision(ж<Δtesting.T> t) {
    foreach (var (i, tt) in startests) {
        @string s = Sprintf(tt.fmt, tt.@in.ꓸꓸꓸ);
        if (s != tt.@out) {
            t.Errorf("#%d: %q: got %q expected %q", i, tt.fmt, s, tt.@out);
        }
    }
}

// PanicS is a type that panics in String.
partial struct PanicS {
    internal any message;
}

// Value receiver.
public static @string String(this PanicS p) {
    throw panic(p.message);
}

// PanicGo is a type that panics in GoString.
partial struct PanicGo {
    internal any message;
}

// Value receiver.
public static @string GoString(this PanicGo p) {
    throw panic(p.message);
}

// PanicF is a type that panics in Format.
partial struct PanicF {
    internal any message;
}

// Value receiver.
public static void Format(this PanicF p, fmt.State f, rune c) {
    throw panic(p.message);
}

partial struct panictestsᴛ1 /*dyn*/ {
    internal @string fmt;
    internal any @in;
    internal @string @out;
}
internal static slice<panictestsᴛ1> panictests = slice<panictestsᴛ1>([
    // String
    new("%s", (ж<PanicS>)nil, "<nil>"), // nil pointer special case
    new("%s", new PanicS(Δio.ErrUnexpectedEOF), "%!s(PANIC=String method: unexpected EOF)"),
    new("%s", new PanicS((nint)3), "%!s(PANIC=String method: 3)"),
    // GoString
    new("%#v", (ж<PanicGo>)nil, "<nil>"), // nil pointer special case
    new("%#v", new PanicGo(Δio.ErrUnexpectedEOF), "%!v(PANIC=GoString method: unexpected EOF)"),
    new("%#v", new PanicGo((nint)3), "%!v(PANIC=GoString method: 3)"),
    // Issue 18282. catchPanic should not clear fmtFlags permanently.
    new("%#v", slice<any>([new PanicGo((nint)3), new PanicGo((nint)3)]), "[]interface {}{%!v(PANIC=GoString method: 3), %!v(PANIC=GoString method: 3)}"),
    // Format
    new("%s", (ж<PanicF>)nil, "<nil>"), // nil pointer special case
    new("%s", new PanicF(Δio.ErrUnexpectedEOF), "%!s(PANIC=Format method: unexpected EOF)"),
    new("%s", new PanicF((nint)3), "%!s(PANIC=Format method: 3)")
]);

public static void TestPanics(ж<Δtesting.T> t) {
    foreach (var (i, tt) in panictests) {
        @string s = Sprintf(tt.fmt, tt.@in);
        if (s != tt.@out) {
            t.Errorf("%d: %q: got %q expected %q", i, tt.fmt, s, tt.@out);
        }
    }
}

// recurCount tests that erroneous String routine doesn't cause fatal recursion.
internal static nint recurCount = 0;

partial struct Recur {
    internal nint i;
    internal ж<bool> failed;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string failˢ => ref failˢᵇ;
private static readonly @string failˢᵇ = "FAIL"u8;

public static @string String(this ж<Recur> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    {
        fmt_test_package.recurCount++; if (fmt_test_package.recurCount > 10) {
            r.failed.Value = true;
            return failˢ;
        }
    }
    // This will call badVerb. Before the fix, that would cause us to recur into
    // this routine to print %!p(value). Now we don't call the user's method
    // during an error.
    return Sprintf("recur@%p value: %d"u8, Ꮡr.OrTypedNil(), r.i);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object failWithPointerˢ = (@string)"fail with pointer"u8;
internal static readonly object failWithValueˢ = (@string)"fail with value"u8;

public static void TestBadVerbRecursion(ж<Δtesting.T> t) {
    ref var failed = ref heap<bool>(false, out var Ꮡfailed);
    ref var r = ref heap<ж<Recur>>(Ꮡ(new Recur(3, Ꮡfailed)), out var Ꮡr);
    _ = Sprintf("recur@%p value: %d\n"u8, Ꮡr, r.i);
    if (failed) {
        t.Error(failWithPointerˢ);
    }
    failed = false;
    r = Ꮡ(new Recur(4, Ꮡfailed));
    _ = Sprintf("recur@%p, value: %d\n"u8, r.OrTypedNil(), r.i);
    if (failed) {
        t.Error(failWithValueˢ);
    }
}

public static void TestIsSpace(ж<Δtesting.T> t) {
    // This tests the internal isSpace function.
    // IsSpace = isSpace is defined in export_test.go.
    for (var i = (rune)0; i <= Δunicode.MaxRune; i++) {
        if (fmt_internal_test_package.IsSpace(i) != Δunicode.IsSpace(i)) {
            t.Errorf("isSpace(%U) = %v, want %v", i, fmt_internal_test_package.IsSpace(i), Δunicode.IsSpace(i));
        }
    }
}

internal static @string hideFromVet(@string s) {
    return s;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string sSSSSˢ => ref sSSSSˢᵇ;
private static readonly @string sSSSSˢᵇ = "%s %s %s %s %s"u8;

internal partial struct TestNilDoesNotBecomeTyped_A /*dyn*/ {
}

internal partial struct TestNilDoesNotBecomeTyped_B /*dyn*/ {
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string expectᶜ => ref expectᶜᵇ;
private static readonly @string expectᶜᵇ = "%!s(<nil>) %!s(*fmt_test.A=<nil>) %!s(<nil>) {} %!s(<nil>)"u8;

public static void TestNilDoesNotBecomeTyped(ж<Δtesting.T> t) {
    ж<TestNilDoesNotBecomeTyped_A> a = default;
    TestNilDoesNotBecomeTyped_B b = new TestNilDoesNotBecomeTyped_B();
    @string got = Sprintf(hideFromVet(sSSSSˢ), (any)(default), a.OrTypedNil(), (any)(default), b, (any)(default));
    @string expect = expectᶜ;
    if (got != expect) {
        t.Errorf("expected:\n\t%q\ngot:\n\t%q", expect, got);
    }
}

partial struct formatterFlagTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal any val;
    internal @string @out;
}
internal static slice<formatterFlagTestsᴛ1> formatterFlagTests = slice<formatterFlagTestsᴛ1>([
    // scalar values with the (unused by fmt) 'a' verb.
    new("%a", new flagPrinter(), "[%a]"),
    new("%-a", new flagPrinter(), "[%-a]"),
    new("%+a", new flagPrinter(), "[%+a]"),
    new("%#a", new flagPrinter(), "[%#a]"),
    new("% a", new flagPrinter(), "[% a]"),
    new("%0a", new flagPrinter(), "[%0a]"),
    new("%1.2a", new flagPrinter(), "[%1.2a]"),
    new("%-1.2a", new flagPrinter(), "[%-1.2a]"),
    new("%+1.2a", new flagPrinter(), "[%+1.2a]"),
    new("%-+1.2a", new flagPrinter(), "[%+-1.2a]"),
    new("%-+1.2abc", new flagPrinter(), "[%+-1.2a]bc"),
    new("%-1.2abc", new flagPrinter(), "[%-1.2a]bc"),
    new("%-0abc", new flagPrinter(), "[%-0a]bc"),

    // composite values with the 'a' verb
    new("%a", new flagPrinter[]{}.array(1), "[[%a]]"),
    new("%-a", new flagPrinter[]{}.array(1), "[[%-a]]"),
    new("%+a", new flagPrinter[]{}.array(1), "[[%+a]]"),
    new("%#a", new flagPrinter[]{}.array(1), "[[%#a]]"),
    new("% a", new flagPrinter[]{}.array(1), "[[% a]]"),
    new("%0a", new flagPrinter[]{}.array(1), "[[%0a]]"),
    new("%1.2a", new flagPrinter[]{}.array(1), "[[%1.2a]]"),
    new("%-1.2a", new flagPrinter[]{}.array(1), "[[%-1.2a]]"),
    new("%+1.2a", new flagPrinter[]{}.array(1), "[[%+1.2a]]"),
    new("%-+1.2a", new flagPrinter[]{}.array(1), "[[%+-1.2a]]"),
    new("%-+1.2abc", new flagPrinter[]{}.array(1), "[[%+-1.2a]]bc"),
    new("%-1.2abc", new flagPrinter[]{}.array(1), "[[%-1.2a]]bc"),
    new("%-0abc", new flagPrinter[]{}.array(1), "[[%-0a]]bc"),

    // simple values with the 'v' verb
    new("%v", new flagPrinter(), "[%v]"),
    new("%-v", new flagPrinter(), "[%-v]"),
    new("%+v", new flagPrinter(), "[%+v]"),
    new("%#v", new flagPrinter(), "[%#v]"),
    new("% v", new flagPrinter(), "[% v]"),
    new("%0v", new flagPrinter(), "[%0v]"),
    new("%1.2v", new flagPrinter(), "[%1.2v]"),
    new("%-1.2v", new flagPrinter(), "[%-1.2v]"),
    new("%+1.2v", new flagPrinter(), "[%+1.2v]"),
    new("%-+1.2v", new flagPrinter(), "[%+-1.2v]"),
    new("%-+1.2vbc", new flagPrinter(), "[%+-1.2v]bc"),
    new("%-1.2vbc", new flagPrinter(), "[%-1.2v]bc"),
    new("%-0vbc", new flagPrinter(), "[%-0v]bc"),

    // composite values with the 'v' verb.
    new("%v", new flagPrinter[]{}.array(1), "[[%v]]"),
    new("%-v", new flagPrinter[]{}.array(1), "[[%-v]]"),
    new("%+v", new flagPrinter[]{}.array(1), "[[%+v]]"),
    new("%#v", new flagPrinter[]{}.array(1), "[1]fmt_test.flagPrinter{[%#v]}"),
    new("% v", new flagPrinter[]{}.array(1), "[[% v]]"),
    new("%0v", new flagPrinter[]{}.array(1), "[[%0v]]"),
    new("%1.2v", new flagPrinter[]{}.array(1), "[[%1.2v]]"),
    new("%-1.2v", new flagPrinter[]{}.array(1), "[[%-1.2v]]"),
    new("%+1.2v", new flagPrinter[]{}.array(1), "[[%+1.2v]]"),
    new("%-+1.2v", new flagPrinter[]{}.array(1), "[[%+-1.2v]]"),
    new("%-+1.2vbc", new flagPrinter[]{}.array(1), "[[%+-1.2v]]bc"),
    new("%-1.2vbc", new flagPrinter[]{}.array(1), "[[%-1.2v]]bc"),
    new("%-0vbc", new flagPrinter[]{}.array(1), "[[%-0v]]bc")
]);

public static void TestFormatterFlags(ж<Δtesting.T> t) {
    foreach (var (_, tt) in formatterFlagTests) {
        @string s = Sprintf(tt.@in, tt.val);
        if (s != tt.@out) {
            t.Errorf("Sprintf(%q, %T) = %q, want %q", tt.@in, tt.val, s, tt.@out);
        }
    }
}

internal partial struct TestParsenum_testCases /*dyn*/ {
    internal @string s;
    internal nint start, end;
    internal nint num;
    internal bool isnum;
    internal nint newi;
}

public static void TestParsenum(ж<Δtesting.T> t) {
    var testCases = slice<TestParsenum_testCases>([
        new("a123", 0, 4, 0, false, 0),
        new("1234", 1, 1, 0, false, 1),
        new("123a", 0, 4, 123, true, 3),
        new("12a3", 0, 4, 12, true, 2),
        new("1234", 0, 4, 1234, true, 4),
        new("1a234", 1, 3, 0, false, 1)
    ]);
    foreach (var (_, tt) in testCases) {
        var (num, isnum, newi) = fmt_internal_test_package.Parsenum(tt.s, tt.start, tt.end);
        if (num != tt.num || isnum != tt.isnum || newi != tt.newi) {
            t.Errorf("parsenum(%q, %d, %d) = %d, %v, %d, want %d, %v, %d", tt.s, tt.start, tt.end, num, isnum, newi, tt.num, tt.isnum, tt.newi);
        }
    }
}

// Test the various Append printers. The details are well tested above;
// here we just make sure the byte slice is updated.
internal static ref readonly @string appendResult => ref appendResultᵇ;
private static readonly @string appendResultᵇ = "hello world, 23"u8;
internal static ref readonly @string hello => ref helloᵇ;
private static readonly @string helloᵇ = "hello "u8;

public static void TestAppendf(ж<Δtesting.T> t) {
    var b = new slice<byte>(100);
    b = b.slice(0, copy(b, hello));
    var got = Appendf(b, "world, %d"u8, (nint)23);
    if (((sstring)got) != appendResult) {
        t.Fatalf("Appendf returns %q not %q", got, appendResult);
    }
    if (Ꮡelem(b, 0) != Ꮡelem(got, 0)) {
        t.Fatalf("Appendf allocated a new slice");
    }
}

public static void TestAppend(ж<Δtesting.T> t) {
    var b = new slice<byte>(100);
    b = b.slice(0, copy(b, hello));
    var got = Append(b, worldˢ, ", ", (nint)23);
    if (((sstring)got) != appendResult) {
        t.Fatalf("Append returns %q not %q", got, appendResult);
    }
    if (Ꮡelem(b, 0) != Ꮡelem(got, 0)) {
        t.Fatalf("Append allocated a new slice");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object worldˢ2 = (@string)"world,"u8;

public static void TestAppendln(ж<Δtesting.T> t) {
    var b = new slice<byte>(100);
    b = b.slice(0, copy(b, hello));
    var got = Appendln(b, worldˢ2, (nint)23);
    if (((@string)got) != appendResult + "\n") {
        t.Fatalf("Appendln returns %q not %q", got, appendResult + "\n");
    }
    if (Ꮡelem(b, 0) != Ꮡelem(got, 0)) {
        t.Fatalf("Appendln allocated a new slice");
    }
}

// noliteral prevents escape analysis from recognizing a literal value.
//
//go:noinline
internal static partial T noliteral<T>(T t) {
    return t;
}

} // end fmt_test_package
