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
using fmt = fmt_package;
using static go.fmt_internal_test_package;
using ꓸꓸꓸany = Span<any>;

partial class fmt_test_package {

[GoType("bool")] partial struct renamedBool;

[GoType("num:nint")] partial struct renamedInt;

[GoType("num:int8")] partial struct renamedInt8;

[GoType("num:int16")] partial struct renamedInt16;

[GoType("num:int32")] partial struct renamedInt32;

[GoType("num:int64")] partial struct renamedInt64;

[GoType("num:nuint")] partial struct renamedUint;

[GoType("num:uint8")] partial struct renamedUint8;

[GoType("num:uint16")] partial struct renamedUint16;

[GoType("num:uint32")] partial struct renamedUint32;

[GoType("num:uint64")] partial struct renamedUint64;

[GoType("num:uintptr")] partial struct renamedUintptr;

[GoType("@string")] partial struct renamedString;

[GoType("[]byte")] partial struct renamedBytes;

[GoType("num:float32")] partial struct renamedFloat32;

[GoType("num:float64")] partial struct renamedFloat64;

[GoType("num:complex64")] partial struct renamedComplex64;

[GoType("num:complex128")] partial struct renamedComplex128;

public static void TestFmtInterface(ж<Δtesting.T> Ꮡt) {
    any i1 = default!;
    i1 = (@string)"abc"u8;
    @string s = Sprintf("%s"u8, i1);
    if (s != "abc"u8) {
        Ꮡt.Errorf(@"Sprintf(""%%s"", empty(""abc"")) = %q want %q"u8, s, (@string)"abc"u8);
    }
}

public static float64 NaN = Δmath.NaN();
internal static float64 posInf = Δmath.Inf(1);
internal static float64 negInf = Δmath.Inf(-1);
internal static ж<nint> ᏑintVar = new StandardBox<nint>(0);
internal static ref nint intVar => ref ᏑintVar.Value;
internal static ж<array<nint>> ᏑΔarray = new StandardBox<array<nint>>(new nint[]{1, 2, 3, 4, 5}.array());
internal static ref array<nint> Δarray => ref ᏑΔarray.Value;
internal static ж<array<any>> Ꮡiarray = new StandardBox<array<any>>(new any[]{(nint)(1), (@string)"hello"u8, 2.5D, default!}.array());
internal static ref array<any> iarray => ref Ꮡiarray.Value;
internal static ж<slice<nint>> ᏑΔslice = new StandardBox<slice<nint>>(Δarray[..]);
internal static ref slice<nint> Δslice => ref ᏑΔslice.ValueSlot;
internal static ж<slice<any>> Ꮡislice = new StandardBox<slice<any>>(iarray[..]);
internal static ref slice<any> islice => ref Ꮡislice.ValueSlot;

[GoType] partial struct A {
    internal nint i;
    internal nuint j;
    internal @string s;
    internal slice<nint> x;
}

[GoType("num:nint")] partial struct I;

public static @string String(this I i) {
    return Sprintf("<%d>"u8, (nint)i);
}

[GoType] partial struct B {
    public I I;
    internal nint j;
}

[GoType] partial struct C {
    internal nint i;
    public partial ref B B { get; }
}

[GoType("num:nint")] partial struct F;

public static void Format(this F f, fmt.State s, rune c) {
    Fprintf(new fmt_test_package.fmt_StateᴠWriter(s), "<%c=F(%d)>"u8, c, (nint)f);
}

[GoType("num:nint")] partial struct G;

public static @string GoString(this G g) {
    return Sprintf("GoString(%d)"u8, (nint)g);
}

[GoType] partial struct S {
    public F F; // a struct field that Formats
    public G G; // a struct field that GoStrings
}

[GoType] partial struct SI {
    public any I;
}

[GoType("num:nint")] partial struct P;

internal static ж<P> ᏑpValue = new StandardBox<P>(default(P));
internal static ref P pValue => ref ᏑpValue.Value;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stringPˢ = "String(p)"u8;

[GoRecv] public static @string String(this ref P p) {
    return stringPˢ;
}

public delegate nint Fn();

public static @string String(this Fn fn) {
    return "String(fn)"u8;
}

internal static Fn fnValue;

// U is a type with two unexported function fields.
[GoType] partial struct U {
    internal Func<@string> u;
    internal Fn fn;
}

internal static ж<array<renamedUint8>> Ꮡbarray = new StandardBox<array<renamedUint8>>(new renamedUint8[]{1, 2, 3, 4, 5}.array());
internal static ref array<renamedUint8> barray => ref Ꮡbarray.Value;

internal static ж<slice<renamedUint8>> Ꮡbslice = new StandardBox<slice<renamedUint8>>(barray[..]);
internal static ref slice<renamedUint8> bslice => ref Ꮡbslice.ValueSlot;

[GoType("num:byte")] partial struct byteStringer;

internal static @string String(this byteStringer _) {
    return "X"u8;
}

internal static slice<byteStringer> byteStringerSlice = new byteStringer[]{(rune)'h', (rune)'e', (rune)'l', (rune)'l', (rune)'o'}.slice();

[GoType("num:byte")] partial struct byteFormatter;

internal static void Format(this byteFormatter _Δp0, fmt.State f, rune _Δp2) {
    Fprint(new fmt_test_package.fmt_StateᴠWriter(f), (@string)"X"u8);
}

internal static slice<byteFormatter> byteFormatterSlice = new byteFormatter[]{(rune)'h', (rune)'e', (rune)'l', (rune)'l', (rune)'o'}.slice();

[GoType("@string")] partial struct writeStringFormatter;

internal static void Format(this writeStringFormatter sf, fmt.State f, rune c) {
    {
        var (sw, ok) = f._<Δio.StringWriter>(ᐧ); if (ok) {
            sw.WriteString("***"u8 + ((@string)sf) + "***"u8);
        }
    }
}

// basic string
// basic bytes
// escaped strings
// The space modifier should have no effect.
// 0 has no effect when - is present.
// Runes that are not printable.
// Runes that are not valid.
// characters
// Specifying precision should have no effect.
// Runes that are not printable.
// Runes that are not valid.
// escaped characters
// The space modifier should have no effect.
// Specifying precision should have no effect.
// 0 has no effect when - is present.
// Runes that are not printable.
// Runes that are not valid.
// width
// integers
// Test correct f.intbuf overflow checks.
// unicode format
// Plus flag should have no effect.
// Space flag should have no effect.
// Precisions below 4 should print 4 digits.
// floats
// Test sharp flag used with floats.
// The sharp flag has no effect for binary float format.
// Precision has no effect for binary float format.
// Test correct f.intbuf boundary checks.
// float infinites and NaNs
// Zero padding does not apply to infinities and NaN.
// complex values
// The sharp flag has no effect for binary complex format.
// Precision has no effect for binary complex format.
// complex infinites and NaNs
// Zero padding does not apply to infinities and NaN.
// old test/fmt_test.go
// arrays
// slices
// byte arrays and slices with %b,%c,%d,%o,%U and %v
// f.space should and f.plus should not have an effect with %v.
// f.space and f.plus should have an effect with %d.
// floates with %v
// complexes with %v
// structs
// +v on structs with Stringable items
// other formats on Stringable items
// Stringer applies only to string formats.
// Stringer applies to the extracted value.
// go syntax
// functions
// simple function
// method expression
// method value
// method of function type
// variable of function type with String method
// array of function type with String method
// method value from function type
// unexported function field
// unexported field of function type with String method
// functions with go syntax
// simple function
// method expression
// method value
// method of function type
// variable of function type with String method
// array of function type with String method
// method value from function type
// unexported function field
// unexported field of function type with String method
// Whole number floats are printed without decimals. See Issue 27634.
// Only print []byte and []uint8 as type []byte if they appear at the top level.
// slices with other formats
// Padding with byte slices.
// Same for strings
// renamings
// Formatter
// GoStringer
// %T
// %p with pointers
// %p on non-pointers
// not a pointer at all
// nil on its own has no type ...
// ... and hence is not a pointer type.
// pointers with specified base
// %v on pointers
// string method on pointer
// String method...
// ... is not called with %p.
// %d on Stringer should give integer if possible
// erroneous things
// Extra argument errors should format without flags set.
// Test that maps with non-reflexive keys print all keys and values.
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
// Complex numbers: exhaustively tested in TestComplexFormatting.
// Use spaces instead of zero if padding to the right.
// float and complex formatting should not change the padding width
// for other elements. See issue 14642.
// integer formatting should not alter padding for other elements.
// Complex fmt used to leave the plus flag set for future entries in the array
// causing +2+0i and +3+0i instead of 2+0i and 3+0i.
// Incomplete format specification caused crash.
// Padding for complex numbers. Has been bad, then fixed, then bad again.
// []T where type T is a byte with a Stringer method.
// And the same for Formatter.
// This next case seems wrong, but the docs say the Formatter wins here.
// pp.WriteString
// reflect.Value handled specially in Go 1.5, making it possible to
// see inside non-exported fields (which cannot be accessed with Interface()).
// Issue 8965.
// Equivalent to the old way.
// Sees inside the field.
// verbs apply to the extracted value too.
// invalid reflect.Value doesn't crash.
// Tests to check that not supported verbs generate an error string.

[GoType("dyn")] partial struct fmtTestsᴛ1 {
    internal @string fmt;
    internal any val;
    internal @string @out;
}
internal static slice<fmtTestsᴛ1> fmtTests = new fmtTestsᴛ1[]{
    new("%d"u8, (nint)(12345), "12345"u8),
    new("%v"u8, (nint)(12345), "12345"u8),
    new("%t"u8, true, "true"u8),
    new("%s"u8, (@string)"abc"u8, "abc"u8),
    new("%q"u8, (@string)"abc"u8, @"""abc"""u8),
    new("%x"u8, (@string)"abc"u8, "616263"u8),
    new("%x"u8, ((@string)(new byte[]{0xff, 0xf0, 0x0f, 0xff})), "fff00fff"u8),
    new("%X"u8, ((@string)(new byte[]{0xff, 0xf0, 0x0f, 0xff})), "FFF00FFF"u8),
    new("%x"u8, (@string)""u8, ""u8),
    new("% x"u8, (@string)""u8, ""u8),
    new("%#x"u8, (@string)""u8, ""u8),
    new("%# x"u8, (@string)""u8, ""u8),
    new("%x"u8, (@string)"xyz"u8, "78797a"u8),
    new("%X"u8, (@string)"xyz"u8, "78797A"u8),
    new("% x"u8, (@string)"xyz"u8, "78 79 7a"u8),
    new("% X"u8, (@string)"xyz"u8, "78 79 7A"u8),
    new("%#x"u8, (@string)"xyz"u8, "0x78797a"u8),
    new("%#X"u8, (@string)"xyz"u8, "0X78797A"u8),
    new("%# x"u8, (@string)"xyz"u8, "0x78 0x79 0x7a"u8),
    new("%# X"u8, (@string)"xyz"u8, "0X78 0X79 0X7A"u8),
    new("%s"u8, slice<byte>("abc"u8), "abc"u8),
    new("%s"u8, new byte[]{(rune)'a', (rune)'b', (rune)'c'}.array(), "abc"u8),
    new("%s"u8, Ꮡ(new byte[]{(rune)'a', (rune)'b', (rune)'c'}.array()), "&abc"u8),
    new("%q"u8, slice<byte>("abc"u8), @"""abc"""u8),
    new("%x"u8, slice<byte>("abc"u8), "616263"u8),
    new("%x"u8, slice<byte>(((@string)(new byte[]{0xff, 0xf0, 0x0f, 0xff}))), "fff00fff"u8),
    new("%X"u8, slice<byte>(((@string)(new byte[]{0xff, 0xf0, 0x0f, 0xff}))), "FFF00FFF"u8),
    new("%x"u8, slice<byte>(""u8), ""u8),
    new("% x"u8, slice<byte>(""u8), ""u8),
    new("%#x"u8, slice<byte>(""u8), ""u8),
    new("%# x"u8, slice<byte>(""u8), ""u8),
    new("%x"u8, slice<byte>("xyz"u8), "78797a"u8),
    new("%X"u8, slice<byte>("xyz"u8), "78797A"u8),
    new("% x"u8, slice<byte>("xyz"u8), "78 79 7a"u8),
    new("% X"u8, slice<byte>("xyz"u8), "78 79 7A"u8),
    new("%#x"u8, slice<byte>("xyz"u8), "0x78797a"u8),
    new("%#X"u8, slice<byte>("xyz"u8), "0X78797A"u8),
    new("%# x"u8, slice<byte>("xyz"u8), "0x78 0x79 0x7a"u8),
    new("%# X"u8, slice<byte>("xyz"u8), "0X78 0X79 0X7A"u8),
    new("%q"u8, (@string)""u8, @""""""u8),
    new("%#q"u8, (@string)""u8, "``"u8),
    new("%q"u8, (@string)"\""u8, @"""\"""""u8),
    new("%#q"u8, (@string)"\""u8, "`\"`"u8),
    new("%q"u8, (@string)"`"u8, @""""u8 + "`"u8 + @""""u8),
    new("%#q"u8, (@string)"`"u8, @""""u8 + "`"u8 + @""""u8),
    new("%q"u8, (@string)"\n"u8, @"""\n"""u8),
    new("%#q"u8, (@string)"\n"u8, @"""\n"""u8),
    new("%q"u8, (@string)@"\n"u8, @"""\\n"""u8),
    new("%#q"u8, (@string)@"\n"u8, "`\\n`"u8),
    new("%q"u8, (@string)"abc"u8, @"""abc"""u8),
    new("%#q"u8, (@string)"abc"u8, "`abc`"u8),
    new("%q"u8, (@string)"日本語"u8, @"""日本語"""u8),
    new("%+q"u8, (@string)"日本語"u8, @"""\u65e5\u672c\u8a9e"""u8),
    new("%#q"u8, (@string)"日本語"u8, "`日本語`"u8),
    new("%#+q"u8, (@string)"日本語"u8, "`日本語`"u8),
    new("%q"u8, (@string)"\a\b\f\n\r\t\v\"\\"u8, @"""\a\b\f\n\r\t\v\""\\"""u8),
    new("%+q"u8, (@string)"\a\b\f\n\r\t\v\"\\"u8, @"""\a\b\f\n\r\t\v\""\\"""u8),
    new("%#q"u8, (@string)"\a\b\f\n\r\t\v\"\\"u8, @"""\a\b\f\n\r\t\v\""\\"""u8),
    new("%#+q"u8, (@string)"\a\b\f\n\r\t\v\"\\"u8, @"""\a\b\f\n\r\t\v\""\\"""u8),
    new("%q"u8, (@string)"☺"u8, @"""☺"""u8),
    new("% q"u8, (@string)"☺"u8, @"""☺"""u8),
    new("%+q"u8, (@string)"☺"u8, @"""\u263a"""u8),
    new("%#q"u8, (@string)"☺"u8, "`☺`"u8),
    new("%#+q"u8, (@string)"☺"u8, "`☺`"u8),
    new("%10q"u8, (@string)"⌘"u8, @"       ""⌘"""u8),
    new("%+10q"u8, (@string)"⌘"u8, @"  ""\u2318"""u8),
    new("%-10q"u8, (@string)"⌘"u8, @"""⌘""       "u8),
    new("%+-10q"u8, (@string)"⌘"u8, @"""\u2318""  "u8),
    new("%010q"u8, (@string)"⌘"u8, @"0000000""⌘"""u8),
    new("%+010q"u8, (@string)"⌘"u8, @"00""\u2318"""u8),
    new("%-010q"u8, (@string)"⌘"u8, @"""⌘""       "u8),
    new("%+-010q"u8, (@string)"⌘"u8, @"""\u2318""  "u8),
    new("%#8q"u8, (@string)"\n"u8, @"    ""\n"""u8),
    new("%#+8q"u8, (@string)"\r"u8, @"    ""\r"""u8),
    new("%#-8q"u8, (@string)"\t"u8, "`	`     "u8),
    new("%#+-8q"u8, (@string)"\b"u8, @"""\b""    "u8),
    new("%q"u8, ((@string)(new byte[]{0x61, 0x62, 0x63, 0xff, 0x64, 0x65, 0x66})), @"""abc\xffdef"""u8),
    new("%+q"u8, ((@string)(new byte[]{0x61, 0x62, 0x63, 0xff, 0x64, 0x65, 0x66})), @"""abc\xffdef"""u8),
    new("%#q"u8, ((@string)(new byte[]{0x61, 0x62, 0x63, 0xff, 0x64, 0x65, 0x66})), @"""abc\xffdef"""u8),
    new("%#+q"u8, ((@string)(new byte[]{0x61, 0x62, 0x63, 0xff, 0x64, 0x65, 0x66})), @"""abc\xffdef"""u8),
    new("%q"u8, (@string)"\U0010ffff"u8, @"""\U0010ffff"""u8),
    new("%+q"u8, (@string)"\U0010ffff"u8, @"""\U0010ffff"""u8),
    new("%#q"u8, (@string)"\U0010ffff"u8, "`􏿿`"u8),
    new("%#+q"u8, (@string)"\U0010ffff"u8, "`􏿿`"u8),
    new("%q"u8, ((@string)(rune)0x110000), @"""�"""u8),
    new("%+q"u8, ((@string)(rune)0x110000), @"""\ufffd"""u8),
    new("%#q"u8, ((@string)(rune)0x110000), "`�`"u8),
    new("%#+q"u8, ((@string)(rune)0x110000), "`�`"u8),
    new("%c"u8, (nuint)(rune)'x', "x"u8),
    new("%c"u8, (nint)(0xe4), "ä"u8),
    new("%c"u8, (nint)(0x672c), "本"u8),
    new("%c"u8, (rune)'日', "日"u8),
    new("%.0c"u8, (rune)'⌘', "⌘"u8),
    new("%3c"u8, (rune)'⌘', "  ⌘"u8),
    new("%-3c"u8, (rune)'⌘', "⌘  "u8),
    new("%c"u8, (uint64)0x100000000UL, "\ufffd"u8),
    new("%c"u8, (rune)'\U00000e00', "\u0e00"u8),
    new("%c"u8, (rune)0x10FFFF, "\U0010ffff"u8),
    new("%c"u8, (nint)(-1), "�"u8),
    new("%c"u8, (nint)(0xDC80), "�"u8),
    new("%c"u8, (rune)0x110000, "�"u8),
    new("%c"u8, (int64)0xFFFFFFFFFL, "�"u8),
    new("%c"u8, (uint64)0xFFFFFFFFFUL, "�"u8),
    new("%q"u8, (nuint)0, @"'\x00'"u8),
    new("%+q"u8, (nuint)0, @"'\x00'"u8),
    new("%q"u8, (rune)'"', @"'""'"u8),
    new("%+q"u8, (rune)'"', @"'""'"u8),
    new("%q"u8, (rune)'\'', @"'\''"u8),
    new("%+q"u8, (rune)'\'', @"'\''"u8),
    new("%q"u8, (rune)'`', "'`'"u8),
    new("%+q"u8, (rune)'`', "'`'"u8),
    new("%q"u8, (rune)'x', @"'x'"u8),
    new("%+q"u8, (rune)'x', @"'x'"u8),
    new("%q"u8, (rune)'ÿ', @"'ÿ'"u8),
    new("%+q"u8, (rune)'ÿ', @"'\u00ff'"u8),
    new("%q"u8, (rune)'\n', @"'\n'"u8),
    new("%+q"u8, (rune)'\n', @"'\n'"u8),
    new("%q"u8, (rune)'☺', @"'☺'"u8),
    new("%+q"u8, (rune)'☺', @"'\u263a'"u8),
    new("% q"u8, (rune)'☺', @"'☺'"u8),
    new("%.0q"u8, (rune)'☺', @"'☺'"u8),
    new("%10q"u8, (rune)'⌘', @"       '⌘'"u8),
    new("%+10q"u8, (rune)'⌘', @"  '\u2318'"u8),
    new("%-10q"u8, (rune)'⌘', @"'⌘'       "u8),
    new("%+-10q"u8, (rune)'⌘', @"'\u2318'  "u8),
    new("%010q"u8, (rune)'⌘', @"0000000'⌘'"u8),
    new("%+010q"u8, (rune)'⌘', @"00'\u2318'"u8),
    new("%-010q"u8, (rune)'⌘', @"'⌘'       "u8),
    new("%+-010q"u8, (rune)'⌘', @"'\u2318'  "u8),
    new("%q"u8, (rune)'\U00000e00', @"'\u0e00'"u8),
    new("%q"u8, (rune)0x10FFFF, @"'\U0010ffff'"u8),
    new("%q"u8, (int32)(-1), @"'�'"u8),
    new("%q"u8, (nint)(0xDC80), @"'�'"u8),
    new("%q"u8, (rune)0x110000, @"'�'"u8),
    new("%q"u8, (int64)0xFFFFFFFFFL, @"'�'"u8),
    new("%q"u8, (uint64)0xFFFFFFFFFUL, @"'�'"u8),
    new("%5s"u8, (@string)"abc"u8, "  abc"u8),
    new("%5s"u8, slice<byte>("abc"u8), "  abc"u8),
    new("%2s"u8, (@string)"\u263a"u8, " ☺"u8),
    new("%2s"u8, slice<byte>("\u263a"u8), " ☺"u8),
    new("%-5s"u8, (@string)"abc"u8, "abc  "u8),
    new("%-5s"u8, slice<byte>("abc"u8), "abc  "u8),
    new("%05s"u8, (@string)"abc"u8, "00abc"u8),
    new("%05s"u8, slice<byte>("abc"u8), "00abc"u8),
    new("%5s"u8, (@string)"abcdefghijklmnopqrstuvwxyz"u8, "abcdefghijklmnopqrstuvwxyz"u8),
    new("%5s"u8, slice<byte>("abcdefghijklmnopqrstuvwxyz"u8), "abcdefghijklmnopqrstuvwxyz"u8),
    new("%.5s"u8, (@string)"abcdefghijklmnopqrstuvwxyz"u8, "abcde"u8),
    new("%.5s"u8, slice<byte>("abcdefghijklmnopqrstuvwxyz"u8), "abcde"u8),
    new("%.0s"u8, (@string)"日本語日本語"u8, ""u8),
    new("%.0s"u8, slice<byte>("日本語日本語"u8), ""u8),
    new("%.5s"u8, (@string)"日本語日本語"u8, "日本語日本"u8),
    new("%.5s"u8, slice<byte>("日本語日本語"u8), "日本語日本"u8),
    new("%.10s"u8, (@string)"日本語日本語"u8, "日本語日本語"u8),
    new("%.10s"u8, slice<byte>("日本語日本語"u8), "日本語日本語"u8),
    new("%08q"u8, (@string)"abc"u8, @"000""abc"""u8),
    new("%08q"u8, slice<byte>("abc"u8), @"000""abc"""u8),
    new("%-8q"u8, (@string)"abc"u8, @"""abc""   "u8),
    new("%-8q"u8, slice<byte>("abc"u8), @"""abc""   "u8),
    new("%.5q"u8, (@string)"abcdefghijklmnopqrstuvwxyz"u8, @"""abcde"""u8),
    new("%.5q"u8, slice<byte>("abcdefghijklmnopqrstuvwxyz"u8), @"""abcde"""u8),
    new("%.5x"u8, (@string)"abcdefghijklmnopqrstuvwxyz"u8, "6162636465"u8),
    new("%.5x"u8, slice<byte>("abcdefghijklmnopqrstuvwxyz"u8), "6162636465"u8),
    new("%.3q"u8, (@string)"日本語日本語"u8, @"""日本語"""u8),
    new("%.3q"u8, slice<byte>("日本語日本語"u8), @"""日本語"""u8),
    new("%.1q"u8, (@string)"日本語"u8, @"""日"""u8),
    new("%.1q"u8, slice<byte>("日本語"u8), @"""日"""u8),
    new("%.1x"u8, (@string)"日本語"u8, "e6"u8),
    new("%.1X"u8, slice<byte>("日本語"u8), "E6"u8),
    new("%10.1q"u8, (@string)"日本語日本語"u8, @"       ""日"""u8),
    new("%10.1q"u8, slice<byte>("日本語日本語"u8), @"       ""日"""u8),
    new("%10v"u8, default!, "     <nil>"u8),
    new("%-10v"u8, default!, "<nil>     "u8),
    new("%d"u8, (nuint)12345, "12345"u8),
    new("%d"u8, (nint)(-12345), "-12345"u8),
    new("%d"u8, unchecked((uint8)(~(uint8)0)), "255"u8),
    new("%d"u8, unchecked((uint16)(~(uint16)0)), "65535"u8),
    new("%d"u8, ~(uint32)0, "4294967295"u8),
    new("%d"u8, ~(uint64)0, "18446744073709551615"u8),
    new("%d"u8, (int8)((int8)(-1 << (int)(7))), "-128"u8),
    new("%d"u8, (int16)((int16)(-1 << (int)(15))), "-32768"u8),
    new("%d"u8, (int32)((int32)(-1 << (int)(31))), "-2147483648"u8),
    new("%d"u8, (int64)(-9223372036854775808L), "-9223372036854775808"u8),
    new("%.d"u8, (nint)(0), ""u8),
    new("%.0d"u8, (nint)(0), ""u8),
    new("%6.0d"u8, (nint)(0), "      "u8),
    new("%06.0d"u8, (nint)(0), "      "u8),
    new("% d"u8, (nint)(12345), " 12345"u8),
    new("%+d"u8, (nint)(12345), "+12345"u8),
    new("%+d"u8, (nint)(-12345), "-12345"u8),
    new("%b"u8, (nint)(7), "111"u8),
    new("%b"u8, (nint)(-6), "-110"u8),
    new("%#b"u8, (nint)(7), "0b111"u8),
    new("%#b"u8, (nint)(-6), "-0b110"u8),
    new("%b"u8, ~(uint32)0, "11111111111111111111111111111111"u8),
    new("%b"u8, ~(uint64)0, "1111111111111111111111111111111111111111111111111111111111111111"u8),
    new("%b"u8, (int64)(-9223372036854775808L), zeroFill("-1"u8, 63, ""u8)),
    new("%o"u8, (nint)(668), "1234"u8),
    new("%o"u8, (nint)(-668), "-1234"u8),
    new("%#o"u8, (nint)(668), "01234"u8),
    new("%#o"u8, (nint)(-668), "-01234"u8),
    new("%O"u8, (nint)(668), "0o1234"u8),
    new("%O"u8, (nint)(-668), "-0o1234"u8),
    new("%o"u8, ~(uint32)0, "37777777777"u8),
    new("%o"u8, ~(uint64)0, "1777777777777777777777"u8),
    new("%#X"u8, (nint)(0), "0X0"u8),
    new("%x"u8, (nint)(0x12abcdef), "12abcdef"u8),
    new("%X"u8, (nint)(0x12abcdef), "12ABCDEF"u8),
    new("%x"u8, ~(uint32)0, "ffffffff"u8),
    new("%X"u8, ~(uint64)0, "FFFFFFFFFFFFFFFF"u8),
    new("%.20b"u8, (nint)(7), "00000000000000000111"u8),
    new("%10d"u8, (nint)(12345), "     12345"u8),
    new("%10d"u8, (nint)(-12345), "    -12345"u8),
    new("%+10d"u8, (nint)(12345), "    +12345"u8),
    new("%010d"u8, (nint)(12345), "0000012345"u8),
    new("%010d"u8, (nint)(-12345), "-000012345"u8),
    new("%20.8d"u8, (nint)(1234), "            00001234"u8),
    new("%20.8d"u8, (nint)(-1234), "           -00001234"u8),
    new("%020.8d"u8, (nint)(1234), "            00001234"u8),
    new("%020.8d"u8, (nint)(-1234), "           -00001234"u8),
    new("%-20.8d"u8, (nint)(1234), "00001234            "u8),
    new("%-20.8d"u8, (nint)(-1234), "-00001234           "u8),
    new("%-#20.8x"u8, (nint)(0x1234abc), "0x01234abc          "u8),
    new("%-#20.8X"u8, (nint)(0x1234abc), "0X01234ABC          "u8),
    new("%-#20.8o"u8, (nint)(668), "00001234            "u8),
    new("%068d"u8, (nint)(1), zeroFill(""u8, 68, "1"u8)),
    new("%068d"u8, (nint)(-1), zeroFill("-"u8, 67, "1"u8)),
    new("%#.68x"u8, (nint)(42), zeroFill("0x"u8, 68, "2a"u8)),
    new("%.68d"u8, (nint)(-42), zeroFill("-"u8, 68, "42"u8)),
    new("%+.68d"u8, (nint)(42), zeroFill("+"u8, 68, "42"u8)),
    new("% .68d"u8, (nint)(42), zeroFill(" "u8, 68, "42"u8)),
    new("% +.68d"u8, (nint)(42), zeroFill("+"u8, 68, "42"u8)),
    new("%U"u8, (nint)(0), "U+0000"u8),
    new("%U"u8, (nint)(-1), "U+FFFFFFFFFFFFFFFF"u8),
    new("%U"u8, (rune)'\n', @"U+000A"u8),
    new("%#U"u8, (rune)'\n', @"U+000A"u8),
    new("%+U"u8, (rune)'x', @"U+0078"u8),
    new("%# U"u8, (rune)'x', @"U+0078 'x'"u8),
    new("%#.2U"u8, (rune)'x', @"U+0078 'x'"u8),
    new("%U"u8, (rune)'\u263a', @"U+263A"u8),
    new("%#U"u8, (rune)'\u263a', @"U+263A '☺'"u8),
    new("%U"u8, (rune)0x1D6C2, @"U+1D6C2"u8),
    new("%#U"u8, (rune)0x1D6C2, @"U+1D6C2 '𝛂'"u8),
    new("%#14.6U"u8, (rune)'⌘', "  U+002318 '⌘'"u8),
    new("%#-14.6U"u8, (rune)'⌘', "U+002318 '⌘'  "u8),
    new("%#014.6U"u8, (rune)'⌘', "  U+002318 '⌘'"u8),
    new("%#-014.6U"u8, (rune)'⌘', "U+002318 '⌘'  "u8),
    new("%.68U"u8, (nuint)42, zeroFill("U+"u8, 68, "2A"u8)),
    new("%#.68U"u8, (rune)'日', zeroFill("U+"u8, 68, "65E5"u8) + " '日'"u8),
    new("%+.3e"u8, 0.0D, "+0.000e+00"u8),
    new("%+.3e"u8, 1.0D, "+1.000e+00"u8),
    new("%+.3x"u8, 0.0D, "+0x0.000p+00"u8),
    new("%+.3x"u8, 1.0D, "+0x1.000p+00"u8),
    new("%+.3f"u8, -1.0D, "-1.000"u8),
    new("%+.3F"u8, -1.0D, "-1.000"u8),
    new("%+.3F"u8, (float32)(-1.0F), "-1.000"u8),
    new("%+07.2f"u8, 1.0D, "+001.00"u8),
    new("%+07.2f"u8, -1.0D, "-001.00"u8),
    new("%-07.2f"u8, 1.0D, "1.00   "u8),
    new("%-07.2f"u8, -1.0D, "-1.00  "u8),
    new("%+-07.2f"u8, 1.0D, "+1.00  "u8),
    new("%+-07.2f"u8, -1.0D, "-1.00  "u8),
    new("%-+07.2f"u8, 1.0D, "+1.00  "u8),
    new("%-+07.2f"u8, -1.0D, "-1.00  "u8),
    new("%+10.2f"u8, +1.0D, "     +1.00"u8),
    new("%+10.2f"u8, -1.0D, "     -1.00"u8),
    new("% .3E"u8, -1.0D, "-1.000E+00"u8),
    new("% .3e"u8, 1.0D, " 1.000e+00"u8),
    new("% .3X"u8, -1.0D, "-0X1.000P+00"u8),
    new("% .3x"u8, 1.0D, " 0x1.000p+00"u8),
    new("%+.3g"u8, 0.0D, "+0"u8),
    new("%+.3g"u8, 1.0D, "+1"u8),
    new("%+.3g"u8, -1.0D, "-1"u8),
    new("% .3g"u8, -1.0D, "-1"u8),
    new("% .3g"u8, 1.0D, " 1"u8),
    new("%b"u8, (float32)1.0F, "8388608p-23"u8),
    new("%b"u8, 1.0D, "4503599627370496p-52"u8),
    new("%#g"u8, 1e-323D, "1.00000e-323"u8),
    new("%#g"u8, -1.0D, "-1.00000"u8),
    new("%#g"u8, 1.1D, "1.10000"u8),
    new("%#g"u8, 123456.0D, "123456."u8),
    new("%#g"u8, 1234567.0D, "1.234567e+06"u8),
    new("%#g"u8, 1230000.0D, "1.23000e+06"u8),
    new("%#g"u8, 1000000.0D, "1.00000e+06"u8),
    new("%#.0f"u8, 1.0D, "1."u8),
    new("%#.0e"u8, 1.0D, "1.e+00"u8),
    new("%#.0x"u8, 1.0D, "0x1.p+00"u8),
    new("%#.0g"u8, 1.0D, "1."u8),
    new("%#.0g"u8, 1100000.0D, "1.e+06"u8),
    new("%#.4f"u8, 1.0D, "1.0000"u8),
    new("%#.4e"u8, 1.0D, "1.0000e+00"u8),
    new("%#.4x"u8, 1.0D, "0x1.0000p+00"u8),
    new("%#.4g"u8, 1.0D, "1.000"u8),
    new("%#.4g"u8, 100000.0D, "1.000e+05"u8),
    new("%#.4g"u8, 1.234D, "1.234"u8),
    new("%#.4g"u8, 0.1234D, "0.1234"u8),
    new("%#.4g"u8, 1.23D, "1.230"u8),
    new("%#.4g"u8, 0.123D, "0.1230"u8),
    new("%#.4g"u8, 1.2D, "1.200"u8),
    new("%#.4g"u8, 0.12D, "0.1200"u8),
    new("%#.4g"u8, 10.2D, "10.20"u8),
    new("%#.4g"u8, 0.0D, "0.000"u8),
    new("%#.4g"u8, 0.012D, "0.01200"u8),
    new("%#.0f"u8, 123.0D, "123."u8),
    new("%#.0e"u8, 123.0D, "1.e+02"u8),
    new("%#.0x"u8, 123.0D, "0x1.p+07"u8),
    new("%#.0g"u8, 123.0D, "1.e+02"u8),
    new("%#.4f"u8, 123.0D, "123.0000"u8),
    new("%#.4e"u8, 123.0D, "1.2300e+02"u8),
    new("%#.4x"u8, 123.0D, "0x1.ec00p+06"u8),
    new("%#.4g"u8, 123.0D, "123.0"u8),
    new("%#.4g"u8, 123000.0D, "1.230e+05"u8),
    new("%#9.4g"u8, 1.0D, "    1.000"u8),
    new("%#b"u8, 1.0D, "4503599627370496p-52"u8),
    new("%.4b"u8, (float32)1.0F, "8388608p-23"u8),
    new("%.4b"u8, -1.0D, "-4503599627370496p-52"u8),
    new("%.68f"u8, 1.0D, zeroFill("1."u8, 68, ""u8)),
    new("%.68f"u8, -1.0D, zeroFill("-1."u8, 68, ""u8)),
    new("%f"u8, posInf, "+Inf"u8),
    new("%.1f"u8, negInf, "-Inf"u8),
    new("% f"u8, NaN, " NaN"u8),
    new("%20f"u8, posInf, "                +Inf"u8),
    new("% 20F"u8, posInf, "                 Inf"u8),
    new("% 20e"u8, negInf, "                -Inf"u8),
    new("% 20x"u8, negInf, "                -Inf"u8),
    new("%+20E"u8, negInf, "                -Inf"u8),
    new("%+20X"u8, negInf, "                -Inf"u8),
    new("% +20g"u8, negInf, "                -Inf"u8),
    new("%+-20G"u8, posInf, "+Inf                "u8),
    new("%20e"u8, NaN, "                 NaN"u8),
    new("%20x"u8, NaN, "                 NaN"u8),
    new("% +20E"u8, NaN, "                +NaN"u8),
    new("% +20X"u8, NaN, "                +NaN"u8),
    new("% -20g"u8, NaN, " NaN                "u8),
    new("%+-20G"u8, NaN, "+NaN                "u8),
    new("%+020e"u8, posInf, "                +Inf"u8),
    new("%+020x"u8, posInf, "                +Inf"u8),
    new("%-020f"u8, negInf, "-Inf                "u8),
    new("%-020E"u8, NaN, "NaN                 "u8),
    new("%-020X"u8, NaN, "NaN                 "u8),
    new("%.f"u8, 0D.i(), "(0+0i)"u8),
    new("% .f"u8, 0D.i(), "( 0+0i)"u8),
    new("%+.f"u8, 0D.i(), "(+0+0i)"u8),
    new("% +.f"u8, 0D.i(), "(+0+0i)"u8),
    new("%+.3e"u8, 0D.i(), "(+0.000e+00+0.000e+00i)"u8),
    new("%+.3x"u8, 0D.i(), "(+0x0.000p+00+0x0.000p+00i)"u8),
    new("%+.3f"u8, 0D.i(), "(+0.000+0.000i)"u8),
    new("%+.3g"u8, 0D.i(), "(+0+0i)"u8),
    new("%+.3e"u8, 1D + 2D.i(), "(+1.000e+00+2.000e+00i)"u8),
    new("%+.3x"u8, 1D + 2D.i(), "(+0x1.000p+00+0x1.000p+01i)"u8),
    new("%+.3f"u8, 1D + 2D.i(), "(+1.000+2.000i)"u8),
    new("%+.3g"u8, 1D + 2D.i(), "(+1+2i)"u8),
    new("%.3e"u8, 0D.i(), "(0.000e+00+0.000e+00i)"u8),
    new("%.3x"u8, 0D.i(), "(0x0.000p+00+0x0.000p+00i)"u8),
    new("%.3f"u8, 0D.i(), "(0.000+0.000i)"u8),
    new("%.3F"u8, 0D.i(), "(0.000+0.000i)"u8),
    new("%.3F"u8, (complex64)0F.i(), "(0.000+0.000i)"u8),
    new("%.3g"u8, 0D.i(), "(0+0i)"u8),
    new("%.3e"u8, 1D + 2D.i(), "(1.000e+00+2.000e+00i)"u8),
    new("%.3x"u8, 1D + 2D.i(), "(0x1.000p+00+0x1.000p+01i)"u8),
    new("%.3f"u8, 1D + 2D.i(), "(1.000+2.000i)"u8),
    new("%.3g"u8, 1D + 2D.i(), "(1+2i)"u8),
    new("%.3e"u8, -1D + -2D.i(), "(-1.000e+00-2.000e+00i)"u8),
    new("%.3x"u8, -1D + -2D.i(), "(-0x1.000p+00-0x1.000p+01i)"u8),
    new("%.3f"u8, -1D + -2D.i(), "(-1.000-2.000i)"u8),
    new("%.3g"u8, -1D + -2D.i(), "(-1-2i)"u8),
    new("% .3E"u8, -1D + -2D.i(), "(-1.000E+00-2.000E+00i)"u8),
    new("% .3X"u8, -1D + -2D.i(), "(-0X1.000P+00-0X1.000P+01i)"u8),
    new("%+.3g"u8, 1D + 2D.i(), "(+1+2i)"u8),
    new("%+.3g"u8, (complex64)(1F + 2F.i()), "(+1+2i)"u8),
    new("%#g"u8, 1D + 2D.i(), "(1.00000+2.00000i)"u8),
    new("%#g"u8, 123456D + 789012D.i(), "(123456.+789012.i)"u8),
    new("%#g"u8, 1e-10D.i(), "(0.00000+1.00000e-10i)"u8),
    new("%#g"u8, -1e+10D + -1.11e+100D.i(), "(-1.00000e+10-1.11000e+100i)"u8),
    new("%#.0f"u8, 1.23D + 1D.i(), "(1.+1.i)"u8),
    new("%#.0e"u8, 1.23D + 1D.i(), "(1.e+00+1.e+00i)"u8),
    new("%#.0x"u8, 1.23D + 1D.i(), "(0x1.p+00+0x1.p+00i)"u8),
    new("%#.0g"u8, 1.23D + 1D.i(), "(1.+1.i)"u8),
    new("%#.0g"u8, 100000D.i(), "(0.+1.e+05i)"u8),
    new("%#.0g"u8, 1.23e+06D + 0D.i(), "(1.e+06+0.i)"u8),
    new("%#.4f"u8, 1D + 1.23D.i(), "(1.0000+1.2300i)"u8),
    new("%#.4e"u8, 123D + 1D.i(), "(1.2300e+02+1.0000e+00i)"u8),
    new("%#.4x"u8, 123D + 1D.i(), "(0x1.ec00p+06+0x1.0000p+00i)"u8),
    new("%#.4g"u8, 123D + 1.23D.i(), "(123.0+1.230i)"u8),
    new("%#12.5g"u8, 100000D.i(), "(      0.0000 +1.0000e+05i)"u8),
    new("%#12.5g"u8, 1.23e+06D + 0D.i(), "(  1.2300e+06     +0.0000i)"u8),
    new("%b"u8, 1D + 2D.i(), "(4503599627370496p-52+4503599627370496p-51i)"u8),
    new("%b"u8, (complex64)(1F + 2F.i()), "(8388608p-23+8388608p-22i)"u8),
    new("%#b"u8, 1D + 2D.i(), "(4503599627370496p-52+4503599627370496p-51i)"u8),
    new("%.4b"u8, 1D + 2D.i(), "(4503599627370496p-52+4503599627370496p-51i)"u8),
    new("%.4b"u8, (complex64)(1F + 2F.i()), "(8388608p-23+8388608p-22i)"u8),
    new("%f"u8, complex(posInf, posInf), "(+Inf+Infi)"u8),
    new("%f"u8, complex(negInf, negInf), "(-Inf-Infi)"u8),
    new("%f"u8, complex(NaN, NaN), "(NaN+NaNi)"u8),
    new("%.1f"u8, complex(posInf, posInf), "(+Inf+Infi)"u8),
    new("% f"u8, complex(posInf, posInf), "( Inf+Infi)"u8),
    new("% f"u8, complex(negInf, negInf), "(-Inf-Infi)"u8),
    new("% f"u8, complex(NaN, NaN), "( NaN+NaNi)"u8),
    new("%8e"u8, complex(posInf, posInf), "(    +Inf    +Infi)"u8),
    new("%8x"u8, complex(posInf, posInf), "(    +Inf    +Infi)"u8),
    new("% 8E"u8, complex(posInf, posInf), "(     Inf    +Infi)"u8),
    new("% 8X"u8, complex(posInf, posInf), "(     Inf    +Infi)"u8),
    new("%+8f"u8, complex(negInf, negInf), "(    -Inf    -Infi)"u8),
    new("% +8g"u8, complex(negInf, negInf), "(    -Inf    -Infi)"u8),
    new("% -8G"u8, complex(NaN, NaN), "( NaN    +NaN    i)"u8),
    new("%+-8b"u8, complex(NaN, NaN), "(+NaN    +NaN    i)"u8),
    new("%08f"u8, complex(posInf, posInf), "(    +Inf    +Infi)"u8),
    new("%-08g"u8, complex(negInf, negInf), "(-Inf    -Inf    i)"u8),
    new("%-08G"u8, complex(NaN, NaN), "(NaN     +NaN    i)"u8),
    new("%e"u8, 1.0D, "1.000000e+00"u8),
    new("%e"u8, 1234.5678e3D, "1.234568e+06"u8),
    new("%e"u8, 1234.5678e-8D, "1.234568e-05"u8),
    new("%e"u8, -7.0D, "-7.000000e+00"u8),
    new("%e"u8, -1e-9D, "-1.000000e-09"u8),
    new("%f"u8, 1234.5678e3D, "1234567.800000"u8),
    new("%f"u8, 1234.5678e-8D, "0.000012"u8),
    new("%f"u8, -7.0D, "-7.000000"u8),
    new("%f"u8, -1e-9D, "-0.000000"u8),
    new("%g"u8, 1234.5678e3D, "1.2345678e+06"u8),
    new("%g"u8, (float32)1234.5678e3F, "1.2345678e+06"u8),
    new("%g"u8, 1234.5678e-8D, "1.2345678e-05"u8),
    new("%g"u8, -7.0D, "-7"u8),
    new("%g"u8, -1e-9D, "-1e-09"u8),
    new("%g"u8, (float32)(-1e-9F), "-1e-09"u8),
    new("%E"u8, 1.0D, "1.000000E+00"u8),
    new("%E"u8, 1234.5678e3D, "1.234568E+06"u8),
    new("%E"u8, 1234.5678e-8D, "1.234568E-05"u8),
    new("%E"u8, -7.0D, "-7.000000E+00"u8),
    new("%E"u8, -1e-9D, "-1.000000E-09"u8),
    new("%G"u8, 1234.5678e3D, "1.2345678E+06"u8),
    new("%G"u8, (float32)1234.5678e3F, "1.2345678E+06"u8),
    new("%G"u8, 1234.5678e-8D, "1.2345678E-05"u8),
    new("%G"u8, -7.0D, "-7"u8),
    new("%G"u8, -1e-9D, "-1E-09"u8),
    new("%G"u8, (float32)(-1e-9F), "-1E-09"u8),
    new("%20.5s"u8, (@string)"qwertyuiop"u8, "               qwert"u8),
    new("%.5s"u8, (@string)"qwertyuiop"u8, "qwert"u8),
    new("%-20.5s"u8, (@string)"qwertyuiop"u8, "qwert               "u8),
    new("%20c"u8, (rune)'x', "                   x"u8),
    new("%-20c"u8, (rune)'x', "x                   "u8),
    new("%20.6e"u8, 1.2345e3D, "        1.234500e+03"u8),
    new("%20.6e"u8, 1.2345e-3D, "        1.234500e-03"u8),
    new("%20e"u8, 1.2345e3D, "        1.234500e+03"u8),
    new("%20e"u8, 1.2345e-3D, "        1.234500e-03"u8),
    new("%20.8e"u8, 1.2345e3D, "      1.23450000e+03"u8),
    new("%20f"u8, 1.23456789e3D, "         1234.567890"u8),
    new("%20f"u8, 1.23456789e-3D, "            0.001235"u8),
    new("%20f"u8, 12345678901.23456789D, "  12345678901.234568"u8),
    new("%-20f"u8, 1.23456789e3D, "1234.567890         "u8),
    new("%20.8f"u8, 1.23456789e3D, "       1234.56789000"u8),
    new("%20.8f"u8, 1.23456789e-3D, "          0.00123457"u8),
    new("%g"u8, 1.23456789e3D, "1234.56789"u8),
    new("%g"u8, 1.23456789e-3D, "0.00123456789"u8),
    new("%g"u8, 1.23456789e20D, "1.23456789e+20"u8),
    new("%v"u8, Δarray.Clone(), "[1 2 3 4 5]"u8),
    new("%v"u8, iarray.Clone(), "[1 hello 2.5 <nil>]"u8),
    new("%v"u8, barray.Clone(), "[1 2 3 4 5]"u8),
    new("%v"u8, ᏑΔarray, "&[1 2 3 4 5]"u8),
    new("%v"u8, Ꮡiarray, "&[1 hello 2.5 <nil>]"u8),
    new("%v"u8, Ꮡbarray, "&[1 2 3 4 5]"u8),
    new("%v"u8, Δslice, "[1 2 3 4 5]"u8),
    new("%v"u8, islice, "[1 hello 2.5 <nil>]"u8),
    new("%v"u8, bslice, "[1 2 3 4 5]"u8),
    new("%v"u8, ᏑΔslice, "&[1 2 3 4 5]"u8),
    new("%v"u8, Ꮡislice, "&[1 hello 2.5 <nil>]"u8),
    new("%v"u8, Ꮡbslice, "&[1 2 3 4 5]"u8),
    new("%b"u8, new byte[]{65, 66, 67}.array(), "[1000001 1000010 1000011]"u8),
    new("%c"u8, new byte[]{65, 66, 67}.array(), "[A B C]"u8),
    new("%d"u8, new byte[]{65, 66, 67}.array(), "[65 66 67]"u8),
    new("%o"u8, new byte[]{65, 66, 67}.array(), "[101 102 103]"u8),
    new("%U"u8, new byte[]{65, 66, 67}.array(), "[U+0041 U+0042 U+0043]"u8),
    new("%v"u8, new byte[]{65, 66, 67}.array(), "[65 66 67]"u8),
    new("%v"u8, new byte[]{123}.array(), "[123]"u8),
    new("%012v"u8, new byte[]{}.slice(), "[]"u8),
    new("%#012v"u8, new byte[]{}.slice(), "[]byte{}"u8),
    new("%6v"u8, new byte[]{1, 11, 111}.slice(), "[     1     11    111]"u8),
    new("%06v"u8, new byte[]{1, 11, 111}.slice(), "[000001 000011 000111]"u8),
    new("%-6v"u8, new byte[]{1, 11, 111}.slice(), "[1      11     111   ]"u8),
    new("%-06v"u8, new byte[]{1, 11, 111}.slice(), "[1      11     111   ]"u8),
    new("%#v"u8, new byte[]{1, 11, 111}.slice(), "[]byte{0x1, 0xb, 0x6f}"u8),
    new("%#6v"u8, new byte[]{1, 11, 111}.slice(), "[]byte{   0x1,    0xb,   0x6f}"u8),
    new("%#06v"u8, new byte[]{1, 11, 111}.slice(), "[]byte{0x000001, 0x00000b, 0x00006f}"u8),
    new("%#-6v"u8, new byte[]{1, 11, 111}.slice(), "[]byte{0x1   , 0xb   , 0x6f  }"u8),
    new("%#-06v"u8, new byte[]{1, 11, 111}.slice(), "[]byte{0x1   , 0xb   , 0x6f  }"u8),
    new("% v"u8, new byte[]{1, 11, 111}.slice(), "[ 1  11  111]"u8),
    new("%+v"u8, new byte[]{1, 11, 111}.array(), "[1 11 111]"u8),
    new("%# -6v"u8, new byte[]{1, 11, 111}.slice(), "[]byte{ 0x1  ,  0xb  ,  0x6f }"u8),
    new("%#+-6v"u8, new byte[]{1, 11, 111}.array(), "[3]uint8{0x1   , 0xb   , 0x6f  }"u8),
    new("% d"u8, new byte[]{1, 11, 111}.slice(), "[ 1  11  111]"u8),
    new("%+d"u8, new byte[]{1, 11, 111}.array(), "[+1 +11 +111]"u8),
    new("%# -6d"u8, new byte[]{1, 11, 111}.slice(), "[ 1      11     111  ]"u8),
    new("%#+-6d"u8, new byte[]{1, 11, 111}.array(), "[+1     +11    +111  ]"u8),
    new("%v"u8, 1.2345678D, "1.2345678"u8),
    new("%v"u8, (float32)1.2345678F, "1.2345678"u8),
    new("%v"u8, 1D + 2D.i(), "(1+2i)"u8),
    new("%v"u8, (complex64)(1F + 2F.i()), "(1+2i)"u8),
    new("%v"u8, new A(1, 2, "a"u8, new nint[]{1, 2}.slice()), @"{1 2 a [1 2]}"u8),
    new("%+v"u8, new A(1, 2, "a"u8, new nint[]{1, 2}.slice()), @"{i:1 j:2 s:a x:[1 2]}"u8),
    new("%+v"u8, new B(1, 2), @"{I:<1> j:2}"u8),
    new("%+v"u8, new C(1, new B(2, 3)), @"{i:1 B:{I:<2> j:3}}"u8),
    new("%s"u8, ((I)23), @"<23>"u8),
    new("%q"u8, ((I)23), @"""<23>"""u8),
    new("%x"u8, ((I)23), @"3c32333e"u8),
    new("%#x"u8, ((I)23), @"0x3c32333e"u8),
    new("%# x"u8, ((I)23), @"0x3c 0x32 0x33 0x3e"u8),
    new("%d"u8, ((I)23), @"23"u8),
    new("%s"u8, reflect.ValueOf(((I)23)), @"<23>"u8),
    new("%#v"u8, new A(1, 2, "a"u8, new nint[]{1, 2}.slice()), @"fmt_test.A{i:1, j:0x2, s:""a"", x:[]int{1, 2}}"u8),
    new("%#v"u8, @new<byte>(), "(*uint8)(0xPTR)"u8),
    new("%#v"u8, new channel<nint>(0), "(chan int)(0xPTR)"u8),
    new("%#v"u8, (uint64)(18446744073709551615UL), "0xffffffffffffffff"u8),
    new("%#v"u8, (nint)(1000000000), "1000000000"u8),
    new("%#v"u8, new map<@string, nint>{["a"u8] = 1}, @"map[string]int{""a"":1}"u8),
    new("%#v"u8, new map<@string, B>{["a"u8] = new(1, 2)}, @"map[string]fmt_test.B{""a"":fmt_test.B{I:1, j:2}}"u8),
    new("%#v"u8, new @string[]{"a"u8, "b"u8}.slice(), @"[]string{""a"", ""b""}"u8),
    new("%#v"u8, new SI(nil), @"fmt_test.SI{I:interface {}(nil)}"u8),
    new("%#v"u8, slice<nint>(default!), @"[]int(nil)"u8),
    new("%#v"u8, new nint[]{}.slice(), @"[]int{}"u8),
    new("%#v"u8, Δarray.Clone(), @"[5]int{1, 2, 3, 4, 5}"u8),
    new("%#v"u8, ᏑΔarray, @"&[5]int{1, 2, 3, 4, 5}"u8),
    new("%#v"u8, iarray.Clone(), @"[4]interface {}{1, ""hello"", 2.5, interface {}(nil)}"u8),
    new("%#v"u8, Ꮡiarray, @"&[4]interface {}{1, ""hello"", 2.5, interface {}(nil)}"u8),
    new("%#v"u8, ((map<nint, byte>)default!), @"map[int]uint8(nil)"u8),
    new("%#v"u8, new map<nint, byte>{}, @"map[int]uint8{}"u8),
    new("%#v"u8, (@string)"foo"u8, @"""foo"""u8),
    new("%#v"u8, barray.Clone(), @"[5]fmt_test.renamedUint8{0x1, 0x2, 0x3, 0x4, 0x5}"u8),
    new("%#v"u8, bslice, @"[]fmt_test.renamedUint8{0x1, 0x2, 0x3, 0x4, 0x5}"u8),
    new("%#v"u8, slice<int32>(default!), "[]int32(nil)"u8),
    new("%#v"u8, 1.2345678D, "1.2345678"u8),
    new("%#v"u8, (float32)1.2345678F, "1.2345678"u8),
    new("%v"u8, TestFmtInterface, "0xPTR"u8),
    new("%v"u8, reflect.ValueOf(TestFmtInterface), "0xPTR"u8),
    new("%v"u8, ((Func<G, @string>)(GoString)), "0xPTR"u8),
    new("%v"u8, reflect.ValueOf(((Func<G, @string>)(GoString))), "0xPTR"u8),
    new("%v"u8, () => ((G)23).GoString(), "0xPTR"u8),
    new("%v"u8, reflect.ValueOf(() => ((G)23).GoString()), "0xPTR"u8),
    new("%v"u8, reflect.ValueOf(((G)23)).Method(0), "0xPTR"u8),
    new("%v"u8, ((Func<Fn, @string>)(String)), "0xPTR"u8),
    new("%v"u8, reflect.ValueOf(((Func<Fn, @string>)(String))), "0xPTR"u8),
    new("%v"u8, (fnValue).OrTypedNilFunc(), "String(fn)"u8),
    new("%v"u8, reflect.ValueOf((fnValue).OrTypedNilFunc()), "String(fn)"u8),
    new("%v"u8, new Fn[]{fnValue}.array(), "[String(fn)]"u8),
    new("%v"u8, reflect.ValueOf(new Fn[]{fnValue}.array()), "[String(fn)]"u8),
    new("%v"u8, () => fnValue.String(), "0xPTR"u8),
    new("%v"u8, reflect.ValueOf(() => fnValue.String()), "0xPTR"u8),
    new("%v"u8, reflect.ValueOf((fnValue).OrTypedNilFunc()).Method(0), "0xPTR"u8),
    new("%v"u8, (new U(nil).u).OrTypedNilFunc(), "<nil>"u8),
    new("%v"u8, reflect.ValueOf((new U(nil).u).OrTypedNilFunc()), "<nil>"u8),
    new("%v"u8, reflect.ValueOf(new U(nil)).Field(0), "<nil>"u8),
    new("%v"u8, (new U(fn: fnValue).fn).OrTypedNilFunc(), "String(fn)"u8),
    new("%v"u8, reflect.ValueOf((new U(fn: fnValue).fn).OrTypedNilFunc()), "String(fn)"u8),
    new("%v"u8, reflect.ValueOf(new U(fn: fnValue)).Field(1), "<nil>"u8),
    new("%#v"u8, TestFmtInterface, "(func(*testing.T))(0xPTR)"u8),
    new("%#v"u8, reflect.ValueOf(TestFmtInterface), "(func(*testing.T))(0xPTR)"u8),
    new("%#v"u8, ((Func<G, @string>)(GoString)), "(func(fmt_test.G) string)(0xPTR)"u8),
    new("%#v"u8, reflect.ValueOf(((Func<G, @string>)(GoString))), "(func(fmt_test.G) string)(0xPTR)"u8),
    new("%#v"u8, () => ((G)23).GoString(), "(func() string)(0xPTR)"u8),
    new("%#v"u8, reflect.ValueOf(() => ((G)23).GoString()), "(func() string)(0xPTR)"u8),
    new("%#v"u8, reflect.ValueOf(((G)23)).Method(0), "(func() string)(0xPTR)"u8),
    new("%#v"u8, ((Func<Fn, @string>)(String)), "(func(fmt_test.Fn) string)(0xPTR)"u8),
    new("%#v"u8, reflect.ValueOf(((Func<Fn, @string>)(String))), "(func(fmt_test.Fn) string)(0xPTR)"u8),
    new("%#v"u8, (fnValue).OrTypedNilFunc(), "(fmt_test.Fn)(nil)"u8),
    new("%#v"u8, reflect.ValueOf((fnValue).OrTypedNilFunc()), "(fmt_test.Fn)(nil)"u8),
    new("%#v"u8, new Fn[]{fnValue}.array(), "[1]fmt_test.Fn{(fmt_test.Fn)(nil)}"u8),
    new("%#v"u8, reflect.ValueOf(new Fn[]{fnValue}.array()), "[1]fmt_test.Fn{(fmt_test.Fn)(nil)}"u8),
    new("%#v"u8, () => fnValue.String(), "(func() string)(0xPTR)"u8),
    new("%#v"u8, reflect.ValueOf(() => fnValue.String()), "(func() string)(0xPTR)"u8),
    new("%#v"u8, reflect.ValueOf((fnValue).OrTypedNilFunc()).Method(0), "(func() string)(0xPTR)"u8),
    new("%#v"u8, (new U(nil).u).OrTypedNilFunc(), "(func() string)(nil)"u8),
    new("%#v"u8, reflect.ValueOf((new U(nil).u).OrTypedNilFunc()), "(func() string)(nil)"u8),
    new("%#v"u8, reflect.ValueOf(new U(nil)).Field(0), "(func() string)(nil)"u8),
    new("%#v"u8, (new U(fn: fnValue).fn).OrTypedNilFunc(), "(fmt_test.Fn)(nil)"u8),
    new("%#v"u8, reflect.ValueOf((new U(fn: fnValue).fn).OrTypedNilFunc()), "(fmt_test.Fn)(nil)"u8),
    new("%#v"u8, reflect.ValueOf(new U(fn: fnValue)).Field(1), "(fmt_test.Fn)(nil)"u8),
    new("%#v"u8, 1.0D, "1"u8),
    new("%#v"u8, 1000000.0D, "1e+06"u8),
    new("%#v"u8, (float32)1.0F, "1"u8),
    new("%#v"u8, (float32)1000000.0F, "1e+06"u8),
    new("%#v"u8, slice<byte>(default!), "[]byte(nil)"u8),
    new("%#v"u8, slice<uint8>(default!), "[]byte(nil)"u8),
    new("%#v"u8, new byte[]{}.slice(), "[]byte{}"u8),
    new("%#v"u8, new uint8[]{}.slice(), "[]byte{}"u8),
    new("%#v"u8, reflect.ValueOf(new byte[]{}.slice()), "[]uint8{}"u8),
    new("%#v"u8, reflect.ValueOf(new uint8[]{}.slice()), "[]uint8{}"u8),
    new("%#v"u8, Ꮡ(new byte[]{}.slice()), "&[]uint8{}"u8),
    new("%#v"u8, Ꮡ(new byte[]{}.slice()), "&[]uint8{}"u8),
    new("%#v"u8, new byte[]{}.array(3), "[3]uint8{0x0, 0x0, 0x0}"u8),
    new("%#v"u8, new uint8[]{}.array(3), "[3]uint8{0x0, 0x0, 0x0}"u8),
    new("%#x"u8, new nint[]{1, 2, 15}.slice(), @"[0x1 0x2 0xf]"u8),
    new("%x"u8, new nint[]{1, 2, 15}.slice(), @"[1 2 f]"u8),
    new("%d"u8, new nint[]{1, 2, 15}.slice(), @"[1 2 15]"u8),
    new("%d"u8, new byte[]{1, 2, 15}.slice(), @"[1 2 15]"u8),
    new("%q"u8, new @string[]{"a"u8, "b"u8}.slice(), @"[""a"" ""b""]"u8),
    new("% 02x"u8, new byte[]{1}.slice(), "01"u8),
    new("% 02x"u8, new byte[]{1, 2, 3}.slice(), "01 02 03"u8),
    new("%2x"u8, new byte[]{}.slice(), "  "u8),
    new("%#2x"u8, new byte[]{}.slice(), "  "u8),
    new("% 02x"u8, new byte[]{}.slice(), "00"u8),
    new("%# 02x"u8, new byte[]{}.slice(), "00"u8),
    new("%-2x"u8, new byte[]{}.slice(), "  "u8),
    new("%-02x"u8, new byte[]{}.slice(), "  "u8),
    new("%8x"u8, new byte[]{0xab}.slice(), "      ab"u8),
    new("% 8x"u8, new byte[]{0xab}.slice(), "      ab"u8),
    new("%#8x"u8, new byte[]{0xab}.slice(), "    0xab"u8),
    new("%# 8x"u8, new byte[]{0xab}.slice(), "    0xab"u8),
    new("%08x"u8, new byte[]{0xab}.slice(), "000000ab"u8),
    new("% 08x"u8, new byte[]{0xab}.slice(), "000000ab"u8),
    new("%#08x"u8, new byte[]{0xab}.slice(), "00000xab"u8),
    new("%# 08x"u8, new byte[]{0xab}.slice(), "00000xab"u8),
    new("%10x"u8, new byte[]{0xab, 0xcd}.slice(), "      abcd"u8),
    new("% 10x"u8, new byte[]{0xab, 0xcd}.slice(), "     ab cd"u8),
    new("%#10x"u8, new byte[]{0xab, 0xcd}.slice(), "    0xabcd"u8),
    new("%# 10x"u8, new byte[]{0xab, 0xcd}.slice(), " 0xab 0xcd"u8),
    new("%010x"u8, new byte[]{0xab, 0xcd}.slice(), "000000abcd"u8),
    new("% 010x"u8, new byte[]{0xab, 0xcd}.slice(), "00000ab cd"u8),
    new("%#010x"u8, new byte[]{0xab, 0xcd}.slice(), "00000xabcd"u8),
    new("%# 010x"u8, new byte[]{0xab, 0xcd}.slice(), "00xab 0xcd"u8),
    new("%-10X"u8, new byte[]{0xab}.slice(), "AB        "u8),
    new("% -010X"u8, new byte[]{0xab}.slice(), "AB        "u8),
    new("%#-10X"u8, new byte[]{0xab, 0xcd}.slice(), "0XABCD    "u8),
    new("%# -010X"u8, new byte[]{0xab, 0xcd}.slice(), "0XAB 0XCD "u8),
    new("%2x"u8, (@string)""u8, "  "u8),
    new("%#2x"u8, (@string)""u8, "  "u8),
    new("% 02x"u8, (@string)""u8, "00"u8),
    new("%# 02x"u8, (@string)""u8, "00"u8),
    new("%-2x"u8, (@string)""u8, "  "u8),
    new("%-02x"u8, (@string)""u8, "  "u8),
    new("%8x"u8, ((@string)(new byte[]{0xab})), "      ab"u8),
    new("% 8x"u8, ((@string)(new byte[]{0xab})), "      ab"u8),
    new("%#8x"u8, ((@string)(new byte[]{0xab})), "    0xab"u8),
    new("%# 8x"u8, ((@string)(new byte[]{0xab})), "    0xab"u8),
    new("%08x"u8, ((@string)(new byte[]{0xab})), "000000ab"u8),
    new("% 08x"u8, ((@string)(new byte[]{0xab})), "000000ab"u8),
    new("%#08x"u8, ((@string)(new byte[]{0xab})), "00000xab"u8),
    new("%# 08x"u8, ((@string)(new byte[]{0xab})), "00000xab"u8),
    new("%10x"u8, ((@string)(new byte[]{0xab, 0xcd})), "      abcd"u8),
    new("% 10x"u8, ((@string)(new byte[]{0xab, 0xcd})), "     ab cd"u8),
    new("%#10x"u8, ((@string)(new byte[]{0xab, 0xcd})), "    0xabcd"u8),
    new("%# 10x"u8, ((@string)(new byte[]{0xab, 0xcd})), " 0xab 0xcd"u8),
    new("%010x"u8, ((@string)(new byte[]{0xab, 0xcd})), "000000abcd"u8),
    new("% 010x"u8, ((@string)(new byte[]{0xab, 0xcd})), "00000ab cd"u8),
    new("%#010x"u8, ((@string)(new byte[]{0xab, 0xcd})), "00000xabcd"u8),
    new("%# 010x"u8, ((@string)(new byte[]{0xab, 0xcd})), "00xab 0xcd"u8),
    new("%-10X"u8, ((@string)(new byte[]{0xab})), "AB        "u8),
    new("% -010X"u8, ((@string)(new byte[]{0xab})), "AB        "u8),
    new("%#-10X"u8, ((@string)(new byte[]{0xab, 0xcd})), "0XABCD    "u8),
    new("%# -010X"u8, ((@string)(new byte[]{0xab, 0xcd})), "0XAB 0XCD "u8),
    new("%v"u8, ((renamedBool)true), "true"u8),
    new("%d"u8, ((renamedBool)true), "%!d(fmt_test.renamedBool=true)"u8),
    new("%o"u8, ((renamedInt)8), "10"u8),
    new("%d"u8, ((renamedInt8)(-9)), "-9"u8),
    new("%v"u8, ((renamedInt16)10), "10"u8),
    new("%v"u8, ((renamedInt32)(-11)), "-11"u8),
    new("%X"u8, ((renamedInt64)255), "FF"u8),
    new("%v"u8, ((renamedUint)13), "13"u8),
    new("%o"u8, ((renamedUint8)14), "16"u8),
    new("%X"u8, ((renamedUint16)15), "F"u8),
    new("%d"u8, ((renamedUint32)16), "16"u8),
    new("%X"u8, ((renamedUint64)17), "11"u8),
    new("%o"u8, ((renamedUintptr)18), "22"u8),
    new("%x"u8, ((renamedString)(@string)"thing"u8), "7468696e67"u8),
    new("%d"u8, ((renamedBytes)new byte[]{1, 2, 15}.slice()), @"[1 2 15]"u8),
    new("%q"u8, ((renamedBytes)slice<byte>("hello"u8)), @"""hello"""u8),
    new("%x"u8, new renamedUint8[]{(rune)'h', (rune)'e', (rune)'l', (rune)'l', (rune)'o'}.slice(), "68656c6c6f"u8),
    new("%X"u8, new renamedUint8[]{(rune)'h', (rune)'e', (rune)'l', (rune)'l', (rune)'o'}.slice(), "68656C6C6F"u8),
    new("%s"u8, new renamedUint8[]{(rune)'h', (rune)'e', (rune)'l', (rune)'l', (rune)'o'}.slice(), "hello"u8),
    new("%q"u8, new renamedUint8[]{(rune)'h', (rune)'e', (rune)'l', (rune)'l', (rune)'o'}.slice(), @"""hello"""u8),
    new("%v"u8, ((renamedFloat32)22F), "22"u8),
    new("%v"u8, ((renamedFloat64)33D), "33"u8),
    new("%v"u8, ((renamedComplex64)(3F + 4F.i())), "(3+4i)"u8),
    new("%v"u8, ((renamedComplex128)(4D + -3D.i())), "(4-3i)"u8),
    new("%x"u8, ((F)1), "<x=F(1)>"u8),
    new("%x"u8, ((G)2), "2"u8),
    new("%+v"u8, new S(((F)4), ((G)5)), "{F:<v=F(4)> G:5}"u8),
    new("%#v"u8, ((G)6), "GoString(6)"u8),
    new("%#v"u8, new S(((F)7), ((G)8)), "fmt_test.S{F:<v=F(7)>, G:GoString(8)}"u8),
    new("%T"u8, (byte)0, "uint8"u8),
    new("%T"u8, reflect.ValueOf(default!), "reflect.Value"u8),
    new("%T"u8, (4D + -3D.i()), "complex128"u8),
    new("%T"u8, ((renamedComplex128)(4D + -3D.i())), "fmt_test.renamedComplex128"u8),
    new("%T"u8, intVar, "int"u8),
    new("%6T"u8, ᏑintVar, "  *int"u8),
    new("%10T"u8, default!, "     <nil>"u8),
    new("%-10T"u8, default!, "<nil>     "u8),
    new("%p"u8, ((ж<nint>)nil), "0x0"u8),
    new("%#p"u8, ((ж<nint>)nil), "0"u8),
    new("%p"u8, ᏑintVar, "0xPTR"u8),
    new("%#p"u8, ᏑintVar, "PTR"u8),
    new("%p"u8, ᏑΔarray, "0xPTR"u8),
    new("%p"u8, ᏑΔslice, "0xPTR"u8),
    new("%8.2p"u8, ((ж<nint>)nil), "    0x00"u8),
    new("%-20.16p"u8, ᏑintVar, "0xPTR  "u8),
    new("%p"u8, new channel<nint>(0), "0xPTR"u8),
    new("%p"u8, new map<nint, nint>(), "0xPTR"u8),
    new("%p"u8, () => {
    }, "0xPTR"u8),
    new("%p"u8, (nint)(27), "%!p(int=27)"u8),
    new("%p"u8, default!, "%!p(<nil>)"u8),
    new("%#p"u8, default!, "%!p(<nil>)"u8),
    new("%b"u8, ᏑintVar, "PTR_b"u8),
    new("%d"u8, ᏑintVar, "PTR_d"u8),
    new("%o"u8, ᏑintVar, "PTR_o"u8),
    new("%x"u8, ᏑintVar, "PTR_x"u8),
    new("%X"u8, ᏑintVar, "PTR_X"u8),
    new("%v"u8, default!, "<nil>"u8),
    new("%#v"u8, default!, "<nil>"u8),
    new("%v"u8, ((ж<nint>)nil), "<nil>"u8),
    new("%#v"u8, ((ж<nint>)nil), "(*int)(nil)"u8),
    new("%v"u8, ᏑintVar, "0xPTR"u8),
    new("%#v"u8, ᏑintVar, "(*int)(0xPTR)"u8),
    new("%8.2v"u8, ((ж<nint>)nil), "   <nil>"u8),
    new("%-20.16v"u8, ᏑintVar, "0xPTR  "u8),
    new("%s"u8, ᏑpValue, "String(p)"u8),
    new("%p"u8, ᏑpValue, "0xPTR"u8),
    new("%s"u8, new time.Time(nil).Month(), "January"u8),
    new("%d"u8, new time.Time(nil).Month(), "1"u8),
    new(""u8, default!, "%!(EXTRA <nil>)"u8),
    new(""u8, (nint)(2), "%!(EXTRA int=2)"u8),
    new("no args"u8, (@string)"hello"u8, "no args%!(EXTRA string=hello)"u8),
    new("%s %"u8, (@string)"hello"u8, "hello %!(NOVERB)"u8),
    new("%s %.2"u8, (@string)"hello"u8, "hello %!(NOVERB)"u8),
    new("%017091901790959340919092959340919017929593813360"u8, (nint)(0), "%!(NOVERB)%!(EXTRA int=0)"u8),
    new("%184467440737095516170v"u8, (nint)(0), "%!(NOVERB)%!(EXTRA int=0)"u8),
    new("%010.2"u8, (@string)"12345"u8, "%!(NOVERB)%!(EXTRA string=12345)"u8),
    new("%v"u8, new map<float64, nint>{[NaN] = 1, [NaN] = 1}, "map[NaN:1 NaN:1]"u8),
    new("%.2f"u8, 1.0D, "1.00"u8),
    new("%.2f"u8, -1.0D, "-1.00"u8),
    new("% .2f"u8, 1.0D, " 1.00"u8),
    new("% .2f"u8, -1.0D, "-1.00"u8),
    new("%+.2f"u8, 1.0D, "+1.00"u8),
    new("%+.2f"u8, -1.0D, "-1.00"u8),
    new("%7.2f"u8, 1.0D, "   1.00"u8),
    new("%7.2f"u8, -1.0D, "  -1.00"u8),
    new("% 7.2f"u8, 1.0D, "   1.00"u8),
    new("% 7.2f"u8, -1.0D, "  -1.00"u8),
    new("%+7.2f"u8, 1.0D, "  +1.00"u8),
    new("%+7.2f"u8, -1.0D, "  -1.00"u8),
    new("% +7.2f"u8, 1.0D, "  +1.00"u8),
    new("% +7.2f"u8, -1.0D, "  -1.00"u8),
    new("%07.2f"u8, 1.0D, "0001.00"u8),
    new("%07.2f"u8, -1.0D, "-001.00"u8),
    new("% 07.2f"u8, 1.0D, " 001.00"u8),
    new("% 07.2f"u8, -1.0D, "-001.00"u8),
    new("%+07.2f"u8, 1.0D, "+001.00"u8),
    new("%+07.2f"u8, -1.0D, "-001.00"u8),
    new("% +07.2f"u8, 1.0D, "+001.00"u8),
    new("% +07.2f"u8, -1.0D, "-001.00"u8),
    new("%7.2f"u8, 1D + 2D.i(), "(   1.00  +2.00i)"u8),
    new("%+07.2f"u8, -1D + -2D.i(), "(-001.00-002.00i)"u8),
    new("%0-5s"u8, (@string)"abc"u8, "abc  "u8),
    new("%-05.1f"u8, 1.0D, "1.0  "u8),
    new("%06v"u8, new any[]{+10.0D, (nint)(10)}.slice(), "[000010 000010]"u8),
    new("%06v"u8, new any[]{-10.0D, (nint)(10)}.slice(), "[-00010 000010]"u8),
    new("%06v"u8, new any[]{10D + 10D.i(), (nint)(10)}.slice(), "[(000010+00010i) 000010]"u8),
    new("%06v"u8, new any[]{-10D + 10D.i(), (nint)(10)}.slice(), "[(-00010+00010i) 000010]"u8),
    new("%03.6v"u8, new any[]{(nint)(1), 2.0D, (@string)"x"u8}.slice(), "[000001 002 00x]"u8),
    new("%03.0v"u8, new any[]{(nint)(0), 2.0D, (@string)"x"u8}.slice(), "[    002 000]"u8),
    new("%v"u8, new complex64[]{1F, 2F, 3F}.slice(), "[(1+0i) (2+0i) (3+0i)]"u8),
    new("%v"u8, new complex128[]{1D, 2D, 3D}.slice(), "[(1+0i) (2+0i) (3+0i)]"u8),
    new("%."u8, (nint)(3), "%!.(int=3)"u8),
    new("%+10.2f"u8, 104.66D + 440.51D.i(), "(   +104.66   +440.51i)"u8),
    new("%+10.2f"u8, -104.66D + 440.51D.i(), "(   -104.66   +440.51i)"u8),
    new("%+10.2f"u8, 104.66D + -440.51D.i(), "(   +104.66   -440.51i)"u8),
    new("%+10.2f"u8, -104.66D + -440.51D.i(), "(   -104.66   -440.51i)"u8),
    new("%+010.2f"u8, 104.66D + 440.51D.i(), "(+000104.66+000440.51i)"u8),
    new("%+010.2f"u8, -104.66D + 440.51D.i(), "(-000104.66+000440.51i)"u8),
    new("%+010.2f"u8, 104.66D + -440.51D.i(), "(+000104.66-000440.51i)"u8),
    new("%+010.2f"u8, -104.66D + -440.51D.i(), "(-000104.66-000440.51i)"u8),
    new("%v"u8, byteStringerSlice, "[X X X X X]"u8),
    new("%s"u8, byteStringerSlice, "hello"u8),
    new("%q"u8, byteStringerSlice, "\"hello\""u8),
    new("%x"u8, byteStringerSlice, "68656c6c6f"u8),
    new("%X"u8, byteStringerSlice, "68656C6C6F"u8),
    new("%#v"u8, byteStringerSlice, "[]fmt_test.byteStringer{0x68, 0x65, 0x6c, 0x6c, 0x6f}"u8),
    new("%v"u8, byteFormatterSlice, "[X X X X X]"u8),
    new("%s"u8, byteFormatterSlice, "hello"u8),
    new("%q"u8, byteFormatterSlice, "\"hello\""u8),
    new("%x"u8, byteFormatterSlice, "68656c6c6f"u8),
    new("%X"u8, byteFormatterSlice, "68656C6C6F"u8),
    new("%#v"u8, byteFormatterSlice, "[]fmt_test.byteFormatter{X, X, X, X, X}"u8),
    new("%s"u8, ((writeStringFormatter)(@string)""u8), "******"u8),
    new("%s"u8, ((writeStringFormatter)(@string)"xyz"u8), "***xyz***"u8),
    new("%s"u8, ((writeStringFormatter)(@string)"⌘/⌘"u8), "***⌘/⌘***"u8),
    new("%v"u8, reflect.ValueOf(new A(nil)).Field(0).String(), "<int Value>"u8),
    new("%v"u8, reflect.ValueOf(new A(nil)).Field(0), "0"u8),
    new("%s"u8, reflect.ValueOf((@string)"hello"u8), "hello"u8),
    new("%q"u8, reflect.ValueOf((@string)"hello"u8), @"""hello"""u8),
    new("%#04x"u8, reflect.ValueOf((nint)(256)), "0x0100"u8),
    new("%v"u8, new reflectꓸValue(nil), "<invalid reflect.Value>"u8),
    new("%v"u8, Ꮡ(new reflectꓸValue(nil)), "<invalid Value>"u8),
    new("%v"u8, new SI(new reflectꓸValue(nil)), "{<invalid Value>}"u8),
    new("%☠"u8, default!, "%!☠(<nil>)"u8),
    new("%☠"u8, ((any)default!), "%!☠(<nil>)"u8),
    new("%☠"u8, (nint)0, "%!☠(int=0)"u8),
    new("%☠"u8, (nuint)0, "%!☠(uint=0)"u8),
    new("%☠"u8, new byte[]{0, 1}.slice(), "[%!☠(uint8=0) %!☠(uint8=1)]"u8),
    new("%☠"u8, new uint8[]{0, 1}.slice(), "[%!☠(uint8=0) %!☠(uint8=1)]"u8),
    new("%☠"u8, new byte[]{0}.array(), "[%!☠(uint8=0)]"u8),
    new("%☠"u8, new uint8[]{0}.array(), "[%!☠(uint8=0)]"u8),
    new("%☠"u8, (@string)"hello"u8, "%!☠(string=hello)"u8),
    new("%☠"u8, 1.2345678D, "%!☠(float64=1.2345678)"u8),
    new("%☠"u8, (float32)1.2345678F, "%!☠(float32=1.2345678)"u8),
    new("%☠"u8, 1.2345678D + 1.2345678D.i(), "%!☠(complex128=(1.2345678+1.2345678i))"u8),
    new("%☠"u8, (complex64)(1.2345678F + 1.2345678F.i()), "%!☠(complex64=(1.2345678+1.2345678i))"u8),
    new("%☠"u8, ᏑintVar, "%!☠(*int=0xPTR)"u8),
    new("%☠"u8, new channel<nint>(0), "%!☠(chan int=0xPTR)"u8),
    new("%☠"u8, () => {
    }, "%!☠(func()=0xPTR)"u8),
    new("%☠"u8, reflect.ValueOf(((renamedInt)0)), "%!☠(fmt_test.renamedInt=0)"u8),
    new("%☠"u8, new SI(((renamedInt)0)), "{%!☠(fmt_test.renamedInt=0)}"u8),
    new("%☠"u8, Ꮡ(new any[]{((I)1), ((G)2)}.slice()), "&[%!☠(fmt_test.I=1) %!☠(fmt_test.G=2)]"u8),
    new("%☠"u8, new SI(Ꮡ(new any[]{((I)1), ((G)2)}.slice())), "{%!☠(*[]interface {}=&[1 2])}"u8),
    new("%☠"u8, new reflectꓸValue(nil), "<invalid reflect.Value>"u8),
    new("%☠"u8, new map<float64, nint>{[NaN] = 1}, "map[%!☠(float64=NaN):%!☠(int=1)]"u8)
}.slice();

// zeroFill generates zero-filled strings of the specified width. The length
// of the suffix (but not the prefix) is compensated for in the width calculation.
internal static @string zeroFill(@string prefix, nint width, @string suffix) {
    return prefix + strings.Repeat("0"u8, width - len(suffix)) + suffix;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ptrˢ = "PTR"u8;
internal static readonly @string ptrBˢ = "PTR_b"u8;
internal static readonly @string ptrOˢ = "PTR_o"u8;
internal static readonly @string ptrDˢ = "PTR_d"u8;
internal static readonly @string ptrXˢ = "PTR_x"u8;
internal static readonly @string ptrXˢ2 = "PTR_X"u8;

public static void TestSprintf(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, tt) in fmtTests) {
        @string s = Sprintf(tt.fmt, tt.val);
        nint i = strings.Index(tt.@out, ptrˢ);
        if (i >= 0 && i < len(s)) {
            @string pattern = default!;
            @string chars = default!;
            switch (ᐧ) {
            case {} when strings.HasPrefix(tt.@out.slice(i), ptrBˢ): {
                pattern = ptrBˢ;
                chars = "01"u8;
                break;
            }
            case {} when strings.HasPrefix(tt.@out.slice(i), ptrOˢ): {
                pattern = ptrOˢ;
                chars = "01234567"u8;
                break;
            }
            case {} when strings.HasPrefix(tt.@out.slice(i), ptrDˢ): {
                pattern = ptrDˢ;
                chars = "0123456789"u8;
                break;
            }
            case {} when strings.HasPrefix(tt.@out.slice(i), ptrXˢ): {
                pattern = ptrXˢ;
                chars = "0123456789abcdef"u8;
                break;
            }
            case {} when strings.HasPrefix(tt.@out.slice(i), ptrXˢ2): {
                pattern = ptrXˢ2;
                chars = "0123456789ABCDEF"u8;
                break;
            }
            default: {
                pattern = ptrˢ;
                chars = "0123456789abcdefABCDEF"u8;
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
                var (_, ok) = tt.val._<@string>(ᐧ); if (ok){
                    // Don't requote the already-quoted strings.
                    // It's too confusing to read the errors.
                    Ꮡt.Errorf("Sprintf(%q, %q) = <%s> want <%s>"u8, tt.fmt, tt.val, s, tt.@out);
                } else {
                    Ꮡt.Errorf("Sprintf(%q, %v) = %q want %q"u8, tt.fmt, tt.val, s, tt.@out);
                }
            }
        }
    }
}

// TestComplexFormatting checks that a complex always formats to the same
// thing as if done by hand with two singleton prints.
public static void TestComplexFormatting(ж<Δtesting.T> Ꮡt) {
    slice<bool> yesNo = new bool[]{true, false}.slice();
    slice<float64> values = new float64[]{1D, 0D, -1D, posInf, negInf, NaN}.slice();
    foreach (var (_, plus) in yesNo) {
        foreach (var (_, zero) in yesNo) {
            foreach (var (_, space) in yesNo) {
                foreach (var (_, @char) in (@string)"fFeEgG"u8) {
                    @string realFmt = "%"u8;
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
                    @string imagFmt = "%"u8;
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
                                Ꮡt.Error(f, one, two);
                            }
                        }
                    }
                }
            }
        }
    }
}

[GoType("[]any")] partial struct SE; // slice of empty; notational compactness.

// Explicit version of next line.
// Explicit version of next line.
//  // Explicit version of next line; empty precision means zero.
// An actual use! Print the same arguments twice.
// Erroneous cases.
// Erroneous index does not affect sequence.
// Issue 10675
// TODO: Should this set return better error messages?

[GoType("dyn")] partial struct reorderTestsᴛ1 {
    internal @string fmt;
    internal SE val;
    internal @string @out;
}
internal static slice<reorderTestsᴛ1> reorderTests = new reorderTestsᴛ1[]{
    new("%[1]d"u8, new SE(new any[]{(nint)(1)}.slice()), "1"u8),
    new("%[2]d"u8, new SE(new any[]{(nint)(2), (nint)(1)}.slice()), "1"u8),
    new("%[2]d %[1]d"u8, new SE(new any[]{(nint)(1), (nint)(2)}.slice()), "2 1"u8),
    new("%[2]*[1]d"u8, new SE(new any[]{(nint)(2), (nint)(5)}.slice()), "    2"u8),
    new("%6.2f"u8, new SE(new any[]{12.0D}.slice()), " 12.00"u8),
    new("%[3]*.[2]*[1]f"u8, new SE(new any[]{12.0D, (nint)(2), (nint)(6)}.slice()), " 12.00"u8),
    new("%[1]*.[2]*[3]f"u8, new SE(new any[]{(nint)(6), (nint)(2), 12.0D}.slice()), " 12.00"u8),
    new("%10f"u8, new SE(new any[]{12.0D}.slice()), " 12.000000"u8),
    new("%[1]*[3]f"u8, new SE(new any[]{(nint)(10), (nint)(99), 12.0D}.slice()), " 12.000000"u8),
    new("%.6f"u8, new SE(new any[]{12.0D}.slice()), "12.000000"u8),
    new("%.[1]*[3]f"u8, new SE(new any[]{(nint)(6), (nint)(99), 12.0D}.slice()), "12.000000"u8),
    new("%6.f"u8, new SE(new any[]{12.0D}.slice()), "    12"u8),
    new("%[1]*.[3]f"u8, new SE(new any[]{(nint)(6), (nint)(3), 12.0D}.slice()), "    12"u8),
    new("%d %d %d %#[1]o %#o %#o"u8, new SE(new any[]{(nint)(11), (nint)(12), (nint)(13)}.slice()), "11 12 13 013 014 015"u8),
    new("%[d"u8, new SE(new any[]{(nint)(2), (nint)(1)}.slice()), "%!d(BADINDEX)"u8),
    new("%]d"u8, new SE(new any[]{(nint)(2), (nint)(1)}.slice()), "%!](int=2)d%!(EXTRA int=1)"u8),
    new("%[]d"u8, new SE(new any[]{(nint)(2), (nint)(1)}.slice()), "%!d(BADINDEX)"u8),
    new("%[-3]d"u8, new SE(new any[]{(nint)(2), (nint)(1)}.slice()), "%!d(BADINDEX)"u8),
    new("%[99]d"u8, new SE(new any[]{(nint)(2), (nint)(1)}.slice()), "%!d(BADINDEX)"u8),
    new("%[3]"u8, new SE(new any[]{(nint)(2), (nint)(1)}.slice()), "%!(NOVERB)"u8),
    new("%[1].2d"u8, new SE(new any[]{(nint)(5), (nint)(6)}.slice()), "%!d(BADINDEX)"u8),
    new("%[1]2d"u8, new SE(new any[]{(nint)(2), (nint)(1)}.slice()), "%!d(BADINDEX)"u8),
    new("%3.[2]d"u8, new SE(new any[]{(nint)(7)}.slice()), "%!d(BADINDEX)"u8),
    new("%.[2]d"u8, new SE(new any[]{(nint)(7)}.slice()), "%!d(BADINDEX)"u8),
    new("%d %d %d %#[1]o %#o %#o %#o"u8, new SE(new any[]{(nint)(11), (nint)(12), (nint)(13)}.slice()), "11 12 13 013 014 015 %!o(MISSING)"u8),
    new("%[5]d %[2]d %d"u8, new SE(new any[]{(nint)(1), (nint)(2), (nint)(3)}.slice()), "%!d(BADINDEX) 2 3"u8),
    new("%d %[3]d %d"u8, new SE(new any[]{(nint)(1), (nint)(2)}.slice()), "1 %!d(BADINDEX) 2"u8),
    new("%.[]"u8, new SE(new any[]{}.slice()), "%!](BADINDEX)"u8),
    new("%.-3d"u8, new SE(new any[]{(nint)(42)}.slice()), "%!-(int=42)3d"u8),
    new("%2147483648d"u8, new SE(new any[]{(nint)(42)}.slice()), "%!(NOVERB)%!(EXTRA int=42)"u8),
    new("%-2147483648d"u8, new SE(new any[]{(nint)(42)}.slice()), "%!(NOVERB)%!(EXTRA int=42)"u8),
    new("%.2147483648d"u8, new SE(new any[]{(nint)(42)}.slice()), "%!(NOVERB)%!(EXTRA int=42)"u8)
}.slice();

public static void TestReorder(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, tt) in reorderTests) {
        @string s = Sprintf(tt.fmt, tt.val.ꓸꓸꓸ);
        if (s != tt.@out) {
            Ꮡt.Errorf("Sprintf(%q, %v) = <%s> want <%s>"u8, tt.fmt, tt.val, s, tt.@out);
        }
    }
}

public static void BenchmarkSprintfPadding(ж<Δtesting.B> Ꮡb) {
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("%16f"u8, 1.0D);
        }
    });
}

public static void BenchmarkSprintfEmpty(ж<Δtesting.B> Ꮡb) {
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf(""u8);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object helloˢ = (@string)"hello"u8;

public static void BenchmarkSprintfString(ж<Δtesting.B> Ꮡb) {
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("%s"u8, helloˢ);
        }
    });
}

public static void BenchmarkSprintfTruncateString(ж<Δtesting.B> Ꮡb) {
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("%.3s"u8, (@string)"日本語日本語日本語日本語"u8);
        }
    });
}

public static void BenchmarkSprintfTruncateBytes(ж<Δtesting.B> Ꮡb) {
    any bytes = slice<byte>("日本語日本語日本語日本語"u8);
    var bytesʗ1 = bytes;
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("%.3s"u8, bytesʗ1);
        }
    });
}

public static void BenchmarkSprintfSlowParsingPath(ж<Δtesting.B> Ꮡb) {
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("%.v"u8, (any)(default!));
        }
    });
}

public static void BenchmarkSprintfQuoteString(ж<Δtesting.B> Ꮡb) {
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("%q"u8, (@string)"日本語日本語日本語"u8);
        }
    });
}

public static void BenchmarkSprintfInt(ж<Δtesting.B> Ꮡb) {
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("%d"u8, (nint)(5));
        }
    });
}

public static void BenchmarkSprintfIntInt(ж<Δtesting.B> Ꮡb) {
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("%d %d"u8, (nint)(5), (nint)(6));
        }
    });
}

public static void BenchmarkSprintfPrefixedInt(ж<Δtesting.B> Ꮡb) {
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("This is some meaningless prefix text that needs to be scanned %d"u8, (nint)(6));
        }
    });
}

public static void BenchmarkSprintfFloat(ж<Δtesting.B> Ꮡb) {
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("%g"u8, 5.23184D);
        }
    });
}

public static void BenchmarkSprintfComplex(ж<Δtesting.B> Ꮡb) {
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("%f"u8, 5.23184D + 5.23184D.i());
        }
    });
}

public static void BenchmarkSprintfBoolean(ж<Δtesting.B> Ꮡb) {
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("%t"u8, true);
        }
    });
}

public static void BenchmarkSprintfHexString(ж<Δtesting.B> Ꮡb) {
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("% #x"u8, (@string)"0123456789abcdef"u8);
        }
    });
}

public static void BenchmarkSprintfHexBytes(ж<Δtesting.B> Ꮡb) {
    var data = slice<byte>("0123456789abcdef"u8);
    var dataʗ1 = data;
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("% #x"u8, dataʗ1);
        }
    });
}

public static void BenchmarkSprintfBytes(ж<Δtesting.B> Ꮡb) {
    var data = slice<byte>("0123456789abcdef"u8);
    var dataʗ1 = data;
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("%v"u8, dataʗ1);
        }
    });
}

public static void BenchmarkSprintfStringer(ж<Δtesting.B> Ꮡb) {
    I stringer = ((I)12345);
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("%v"u8, stringer);
        }
    });
}

public static void BenchmarkSprintfStructure(ж<Δtesting.B> Ꮡb) {
    var s = Ꮡ(new any[]{new SI((nint)(12345)), new map<nint, @string>{[0] = "hello"u8}}.slice());
    var sʗ1 = s;
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            _ = Sprintf("%#v"u8, sʗ1.OrTypedNil());
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object worldˢ = (@string)"world"u8;

public static void BenchmarkManyArgs(ж<Δtesting.B> Ꮡb) {
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        while (pb.Next()) {
            buf.Reset();
            Fprintf(new fmt_test_package.bytes_BufferжWriter(Ꮡbuf), "%2d/%2d/%2d %d:%d:%d %s %s\n"u8, (nint)(3), (nint)(4), (nint)(5), (nint)(11), (nint)(12), (nint)(13), helloˢ, worldˢ);
        }
    });
}

public static void BenchmarkFprintInt(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    for (nint i = 0; i < b.N; i++) {
        buf.Reset();
        Fprint(new fmt_test_package.bytes_BufferжWriter(Ꮡbuf), (nint)(123456));
    }
}

public static void BenchmarkFprintfBytes(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var data = slice<byte>(((@string)"0123456789"u8));
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    for (nint i = 0; i < b.N; i++) {
        buf.Reset();
        Fprintf(new fmt_test_package.bytes_BufferжWriter(Ꮡbuf), "%s"u8, data);
    }
}

public static void BenchmarkFprintIntNoAlloc(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    any x = (nint)(123456);
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    for (nint i = 0; i < b.N; i++) {
        buf.Reset();
        Fprint(new fmt_test_package.bytes_BufferжWriter(Ꮡbuf), x);
    }
}

internal static ж<bytes.Buffer> ᏑmallocBuf = new StandardBox<bytes.Buffer>(default(bytes.Buffer));
internal static ref bytes.Buffer mallocBuf => ref ᏑmallocBuf.Value;

internal static ж<nint> mallocPointer; // A pointer so we know the interface value won't allocate.

// large buffer (>64KB)
// not constant
// large buffer (>64KB)
// If the interface value doesn't need to allocate, amortized allocation overhead should be zero.

[GoType("dyn")] partial struct mallocTestᴛ1 {
    internal nint count;
    internal @string desc;
    internal Action fn;
}

[GoType("dyn")] internal partial struct mallocTest_P {
    internal nint x, y;
}
internal static slice<mallocTestᴛ1> mallocTest = new mallocTestᴛ1[]{
    new(0, @"Sprintf("""")"u8, () => {
        _ = Sprintf(""u8);
    }),
    new(1, @"Sprintf(""xxx"")"u8, () => {
        _ = Sprintf("xxx"u8);
    }),
    new(0, @"Sprintf(""%x"")"u8, () => {
        _ = Sprintf("%x"u8, (nint)(7));
    }),
    new(1, @"Sprintf(""%x"")"u8, () => {
        _ = Sprintf("%x"u8, (nint)((1 << (int)(16))));
    }),
    new(3, @"Sprintf(""%80000s"")"u8, () => {
        _ = Sprintf("%80000s"u8, (@string)"hello"u8);
    }),
    new(1, @"Sprintf(""%s"")"u8, () => {
        _ = Sprintf("%s"u8, (@string)"hello"u8);
    }),
    new(1, @"Sprintf(""%x %x"")"u8, () => {
        _ = Sprintf("%x %x"u8, (nint)(7), (nint)(112));
    }),
    new(1, @"Sprintf(""%g"")"u8, () => {
        _ = Sprintf("%g"u8, (float32)3.14159F);
    }),
    new(0, @"Fprintf(buf, ""%s"")"u8, () => {
        mallocBuf.Reset();
        Fprintf(new fmt_test_package.bytes_BufferжWriter(ᏑmallocBuf), "%s"u8, (@string)"hello"u8);
    }),
    new(0, @"Fprintf(buf, ""%x"")"u8, () => {
        mallocBuf.Reset();
        Fprintf(new fmt_test_package.bytes_BufferжWriter(ᏑmallocBuf), "%x"u8, (nint)(7));
    }),
    new(0, @"Fprintf(buf, ""%x"")"u8, () => {
        mallocBuf.Reset();
        Fprintf(new fmt_test_package.bytes_BufferжWriter(ᏑmallocBuf), "%x"u8, (nint)((1 << (int)(16))));
    }),
    new(1, @"Fprintf(buf, ""%x"")"u8, () => {
        mallocBuf.Reset();
        nint i = (1 << (int)(16));
        Fprintf(new fmt_test_package.bytes_BufferжWriter(ᏑmallocBuf), "%x"u8, i);
    }),
    new(4, @"Fprintf(buf, ""%v"")"u8, () => {
        mallocBuf.Reset();
        var s = new nint[]{1, 2}.slice();
        Fprintf(new fmt_test_package.bytes_BufferжWriter(ᏑmallocBuf), "%v"u8, s);
    }),
    new(1, @"Fprintf(buf, ""%v"")"u8, () => {
        mallocBuf.Reset();
        Fprintf(new fmt_test_package.bytes_BufferжWriter(ᏑmallocBuf), "%v"u8, new mallocTest_P(1, 2));
    }),
    new(2, @"Fprintf(buf, ""%80000s"")"u8, () => {
        mallocBuf.Reset();
        Fprintf(new fmt_test_package.bytes_BufferжWriter(ᏑmallocBuf), "%80000s"u8, (@string)"hello"u8);
    }),
    new(0, @"Fprintf(buf, ""%x %x %x"")"u8, () => {
        mallocBuf.Reset();
        Fprintf(new fmt_test_package.bytes_BufferжWriter(ᏑmallocBuf), "%x %x %x"u8, mallocPointer.OrTypedNil(), mallocPointer.OrTypedNil(), mallocPointer.OrTypedNil());
    })
}.slice();

internal static bytes.Buffer _ᴛ1ʗ;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingMallocCountInˢ = (@string)"skipping malloc count in short mode"u8;
internal static readonly object skippingMallocCountUnderˢ = (@string)"skipping malloc count under race detector"u8;

public static void TestCountMallocs(ж<Δtesting.T> Ꮡt) {
    switch (ᐧ) {
    case {} when Δtesting.Short(): {
        Ꮡt.Skip(skippingMallocCountInˢ);
        break;
    }
    case {} when race.Enabled: {
        Ꮡt.Skip(skippingMallocCountUnderˢ);
        break;
    }}

    foreach (var (_, mt) in mallocTest) {
        var mallocs = Δtesting.AllocsPerRun(100, mt.fn);
        {
            var (got, max) = (mallocs, (float64)mt.count); if (got > max) {
                Ꮡt.Errorf("%s: got %v allocs, want <=%v"u8, mt.desc, got, max);
            }
        }
    }
}

[GoType] partial struct flagPrinter {
}

internal static void Format(this flagPrinter _, fmt.State f, rune c) {
    @string s = "%"u8;
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


[GoType("dyn")] partial struct flagtestsᴛ1 {
    internal @string @in;
    internal @string @out;
}
internal static slice<flagtestsᴛ1> flagtests = new flagtestsᴛ1[]{
    new("%a"u8, "[%a]"u8),
    new("%-a"u8, "[%-a]"u8),
    new("%+a"u8, "[%+a]"u8),
    new("%#a"u8, "[%#a]"u8),
    new("% a"u8, "[% a]"u8),
    new("%0a"u8, "[%0a]"u8),
    new("%1.2a"u8, "[%1.2a]"u8),
    new("%-1.2a"u8, "[%-1.2a]"u8),
    new("%+1.2a"u8, "[%+1.2a]"u8),
    new("%-+1.2a"u8, "[%+-1.2a]"u8),
    new("%-+1.2abc"u8, "[%+-1.2a]bc"u8),
    new("%-1.2abc"u8, "[%-1.2a]bc"u8),
    new("%-0abc"u8, "[%-0a]bc"u8)
}.slice();

public static void TestFlagParser(ж<Δtesting.T> Ꮡt) {
    ref var flagprinter = ref heap(new flagPrinter(), out var Ꮡflagprinter);
    foreach (var (_, tt) in flagtests) {
        @string s = Sprintf(tt.@in, Ꮡflagprinter);
        if (s != tt.@out) {
            Ꮡt.Errorf("Sprintf(%q, &flagprinter) => %q, want %q"u8, tt.@in, s, tt.@out);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string abcˢ = "abc"u8;
internal static readonly @string defˢ = "def"u8;

[GoType("dyn")] internal partial struct TestStructPrinter_T {
    internal @string a;
    internal @string b;
    internal nint c;
}

[GoType("dyn")] internal partial struct TestStructPrinter_type {
    internal @string fmt;
    internal @string @out;
}

public static void TestStructPrinter(ж<Δtesting.T> Ꮡt) {
    ref var s = ref heap(new TestStructPrinter_T(), out var Ꮡs);
    s.a = abcˢ;
    s.b = defˢ;
    s.c = 123;
    slice<TestStructPrinter_type> tests = new TestStructPrinter_type[]{
        new("%v"u8, "{abc def 123}"u8),
        new("%+v"u8, "{a:abc b:def c:123}"u8),
        new("%#v"u8, @"fmt_test.T{a:""abc"", b:""def"", c:123}"u8)
    }.slice();
    foreach (var (_, tt) in tests) {
        @string @out = Sprintf(tt.fmt, s);
        if (@out != tt.@out) {
            Ꮡt.Errorf("Sprintf(%q, s) = %#q, want %#q"u8, tt.fmt, @out, tt.@out);
        }
        // The same but with a pointer.
        @out = Sprintf(tt.fmt, Ꮡs);
        if (@out != "&"u8 + tt.@out) {
            Ꮡt.Errorf("Sprintf(%q, &s) = %#q, want %#q"u8, tt.fmt, @out, "&" + tt.@out);
        }
    }
}

public static void TestSlicePrinter(ж<Δtesting.T> Ꮡt) {
    ref var Δslice = ref heap<slice<nint>>(out var Ꮡslice);
    Δslice = new nint[]{}.slice();
    @string s = Sprint(Δslice);
    if (s != "[]"u8) {
        Ꮡt.Errorf("empty slice printed as %q not %q"u8, s, (@string)"[]"u8);
    }
    Δslice = new nint[]{1, 2, 3}.slice();
    s = Sprint(Δslice);
    if (s != "[1 2 3]"u8) {
        Ꮡt.Errorf("slice: got %q expected %q"u8, s, (@string)"[1 2 3]"u8);
    }
    s = Sprint(Ꮡslice);
    if (s != "&[1 2 3]"u8) {
        Ꮡt.Errorf("&slice: got %q expected %q"u8, s, (@string)"&[1 2 3]"u8);
    }
}

// presentInMap checks map printing using substrings so we don't depend on the
// print order.
internal static void presentInMap(@string s, slice<@string> a, ж<Δtesting.T> Ꮡt) {
    for (nint i = 0; i < len(a); i++) {
        nint loc = strings.Index(s, a[i]);
        if (loc < 0) {
            Ꮡt.Errorf("map print: expected to find %q in %q"u8, a[i], s);
        }
        // make sure the match ends here
        loc += len(a[i]);
        if (loc >= len(s) || (s[loc] != (rune)' ' && s[loc] != (rune)']')) {
            Ꮡt.Errorf("map print: %q not properly terminated in %q"u8, a[i], s);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object mapˢ = (@string)"map[]"u8;

public static void TestMapPrinter(ж<Δtesting.T> Ꮡt) {
    var m0 = new map<nint, @string>();
    @string s = Sprint(m0);
    if (s != "map[]"u8) {
        Ꮡt.Errorf("empty map printed as %q not %q"u8, s, mapˢ);
    }
    ref var m1 = ref heap<map<nint, @string>>(out var Ꮡm1);
    m1 = new map<nint, @string>{[1] = "one"u8, [2] = "two"u8, [3] = "three"u8};
    var a = new @string[]{"1:one"u8, "2:two"u8, "3:three"u8}.slice();
    presentInMap(Sprintf("%v"u8, m1), a, Ꮡt);
    presentInMap(Sprint(m1), a, Ꮡt);
    // Pointer to map prints the same but with initial &.
    if (!strings.HasPrefix(Sprint(Ꮡm1), "&"u8)) {
        Ꮡt.Errorf("no initial & for address of map"u8);
    }
    presentInMap(Sprintf("%v"u8, Ꮡm1), a, Ꮡt);
    presentInMap(Sprint(Ꮡm1), a, Ꮡt);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string emptyMapStrᶜ = "map[]"u8;

public static void TestEmptyMap(ж<Δtesting.T> Ꮡt) {
    @string emptyMapStr = emptyMapStrᶜ;
    map<@string, nint> m = default!;
    @string s = Sprint(m);
    if (s != emptyMapStr) {
        Ꮡt.Errorf("nil map printed as %q not %q"u8, s, emptyMapStr);
    }
    m = new map<@string, nint>();
    s = Sprint(m);
    if (s != emptyMapStr) {
        Ꮡt.Errorf("empty map printed as %q not %q"u8, s, emptyMapStr);
    }
}

// TestBlank checks that Sprint (and hence Print, Fprint) puts spaces in the
// right places, that is, between arg pairs in which neither is a string.
public static void TestBlank(ж<Δtesting.T> Ꮡt) {
    @string got = Sprint((@string)"<"u8, (nint)(1), (@string)">:"u8, (nint)(1), (nint)(2), (nint)(3), (@string)"!"u8);
    @string expect = "<1>:1 2 3!"u8;
    if (got != expect) {
        Ꮡt.Errorf("got %q expected %q"u8, got, expect);
    }
}

// TestBlankln checks that Sprintln (and hence Println, Fprintln) puts spaces in
// the right places, that is, between all arg pairs.
public static void TestBlankln(ж<Δtesting.T> Ꮡt) {
    @string got = Sprintln((@string)"<"u8, (nint)(1), (@string)">:"u8, (nint)(1), (nint)(2), (nint)(3), (@string)"!"u8);
    @string expect = "< 1 >: 1 2 3 !\n"u8;
    if (got != expect) {
        Ꮡt.Errorf("got %q expected %q"u8, got, expect);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string vF1ˢ = "<v=F(1)>\n"u8;

// TestFormatterPrintln checks Formatter with Sprint, Sprintln, Sprintf.
public static void TestFormatterPrintln(ж<Δtesting.T> Ꮡt) {
    F f = ((F)1);
    @string expect = vF1ˢ;
    @string s = Sprint(f, (@string)"\n"u8);
    if (s != expect) {
        Ꮡt.Errorf("Sprint wrong with Formatter: expected %q got %q"u8, expect, s);
    }
    s = Sprintln(f);
    if (s != expect) {
        Ꮡt.Errorf("Sprintln wrong with Formatter: expected %q got %q"u8, expect, s);
    }
    s = Sprintf("%v\n"u8, f);
    if (s != expect) {
        Ꮡt.Errorf("Sprintf wrong with Formatter: expected %q got %q"u8, expect, s);
    }
}

internal static slice<any> args(params ꓸꓸꓸany aʗp) {
    var a = aʗp.slice();

    return a;
}

// Some non-int types for width. (Issue 10732).
// erroneous
// Huge negative (-inf).
// Small negative (-1).

[GoType("dyn")] partial struct startestsᴛ1 {
    internal @string fmt;
    internal slice<any> @in;
    internal @string @out;
}
internal static slice<startestsᴛ1> startests = new startestsᴛ1[]{
    new("%*d"u8, args((nint)(4), (nint)(42)), "  42"u8),
    new("%-*d"u8, args((nint)(4), (nint)(42)), "42  "u8),
    new("%*d"u8, args((nint)(-4), (nint)(42)), "42  "u8),
    new("%-*d"u8, args((nint)(-4), (nint)(42)), "42  "u8),
    new("%.*d"u8, args((nint)(4), (nint)(42)), "0042"u8),
    new("%*.*d"u8, args((nint)(8), (nint)(4), (nint)(42)), "    0042"u8),
    new("%0*d"u8, args((nint)(4), (nint)(42)), "0042"u8),
    new("%0*d"u8, args((nuint)4, (nint)(42)), "0042"u8),
    new("%0*d"u8, args((uint64)4, (nint)(42)), "0042"u8),
    new("%0*d"u8, args((rune)'\x04', (nint)(42)), "0042"u8),
    new("%0*d"u8, args((uintptr)4, (nint)(42)), "0042"u8),
    new("%*d"u8, args((any)(default!), (nint)(42)), "%!(BADWIDTH)42"u8),
    new("%*d"u8, args((nint)10000000, (nint)(42)), "%!(BADWIDTH)42"u8),
    new("%*d"u8, args((nint)(-10000000), (nint)(42)), "%!(BADWIDTH)42"u8),
    new("%.*d"u8, args((any)(default!), (nint)(42)), "%!(BADPREC)42"u8),
    new("%.*d"u8, args((nint)(-1), (nint)(42)), "%!(BADPREC)42"u8),
    new("%.*d"u8, args((nint)10000000, (nint)(42)), "%!(BADPREC)42"u8),
    new("%.*d"u8, args((nuint)10000000, (nint)(42)), "%!(BADPREC)42"u8),
    new("%.*d"u8, args((uint64)(((uint64)1 << (int)(63))), (nint)(42)), "%!(BADPREC)42"u8),
    new("%.*d"u8, args((uint64)(18446744073709551615UL), (nint)(42)), "%!(BADPREC)42"u8),
    new("%*d"u8, args((nint)(5), (@string)"foo"u8), "%!d(string=  foo)"u8),
    new("%*% %d"u8, args((nint)(20), (nint)(5)), "% 5"u8),
    new("%*"u8, args((nint)(4)), "%!(NOVERB)"u8)
}.slice();

public static void TestWidthAndPrecision(ж<Δtesting.T> Ꮡt) {
    foreach (var (i, tt) in startests) {
        @string s = Sprintf(tt.fmt, tt.@in.ꓸꓸꓸ);
        if (s != tt.@out) {
            Ꮡt.Errorf("#%d: %q: got %q expected %q"u8, i, tt.fmt, s, tt.@out);
        }
    }
}

// PanicS is a type that panics in String.
[GoType] partial struct PanicS {
    internal any message;
}

// Value receiver.
public static @string String(this PanicS p) {
    throw panic(p.message);
}

// PanicGo is a type that panics in GoString.
[GoType] partial struct PanicGo {
    internal any message;
}

// Value receiver.
public static @string GoString(this PanicGo p) {
    throw panic(p.message);
}

// PanicF is a type that panics in Format.
[GoType] partial struct PanicF {
    internal any message;
}

// Value receiver.
public static void Format(this PanicF p, fmt.State f, rune c) {
    throw panic(p.message);
}

// String
// nil pointer special case
// GoString
// nil pointer special case
// Issue 18282. catchPanic should not clear fmtFlags permanently.
// Format
// nil pointer special case

[GoType("dyn")] partial struct panictestsᴛ1 {
    internal @string fmt;
    internal any @in;
    internal @string @out;
}
internal static slice<panictestsᴛ1> panictests = new panictestsᴛ1[]{
    new("%s"u8, ((ж<PanicS>)nil), "<nil>"u8),
    new("%s"u8, new PanicS(Δio.ErrUnexpectedEOF), "%!s(PANIC=String method: unexpected EOF)"u8),
    new("%s"u8, new PanicS((nint)(3)), "%!s(PANIC=String method: 3)"u8),
    new("%#v"u8, ((ж<PanicGo>)nil), "<nil>"u8),
    new("%#v"u8, new PanicGo(Δio.ErrUnexpectedEOF), "%!v(PANIC=GoString method: unexpected EOF)"u8),
    new("%#v"u8, new PanicGo((nint)(3)), "%!v(PANIC=GoString method: 3)"u8),
    new("%#v"u8, new any[]{new PanicGo((nint)(3)), new PanicGo((nint)(3))}.slice(), "[]interface {}{%!v(PANIC=GoString method: 3), %!v(PANIC=GoString method: 3)}"u8),
    new("%s"u8, ((ж<PanicF>)nil), "<nil>"u8),
    new("%s"u8, new PanicF(Δio.ErrUnexpectedEOF), "%!s(PANIC=Format method: unexpected EOF)"u8),
    new("%s"u8, new PanicF((nint)(3)), "%!s(PANIC=Format method: 3)"u8)
}.slice();

public static void TestPanics(ж<Δtesting.T> Ꮡt) {
    foreach (var (i, tt) in panictests) {
        @string s = Sprintf(tt.fmt, tt.@in);
        if (s != tt.@out) {
            Ꮡt.Errorf("%d: %q: got %q expected %q"u8, i, tt.fmt, s, tt.@out);
        }
    }
}

// recurCount tests that erroneous String routine doesn't cause fatal recursion.
internal static nint recurCount = 0;

[GoType] partial struct Recur {
    internal nint i;
    internal ж<bool> failed;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string failˢ = "FAIL"u8;

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

public static void TestBadVerbRecursion(ж<Δtesting.T> Ꮡt) {
    ref var failed = ref heap<bool>(out var Ꮡfailed);
    failed = false;
    ref var r = ref heap<ж<Recur>>(out var Ꮡr);
    r = Ꮡ(new Recur(3, Ꮡfailed));
    _ = Sprintf("recur@%p value: %d\n"u8, Ꮡr, (~r).i);
    if (failed) {
        Ꮡt.Error(failWithPointerˢ);
    }
    failed = false;
    r = Ꮡ(new Recur(4, Ꮡfailed));
    _ = Sprintf("recur@%p, value: %d\n"u8, r.OrTypedNil(), (~r).i);
    if (failed) {
        Ꮡt.Error(failWithValueˢ);
    }
}

public static void TestIsSpace(ж<Δtesting.T> Ꮡt) {
    // This tests the internal isSpace function.
    // IsSpace = isSpace is defined in export_test.go.
    for (var i = (rune)0; i <= Δunicode.MaxRune; i++) {
        if (fmt_internal_test_package.IsSpace(i) != Δunicode.IsSpace(i)) {
            Ꮡt.Errorf("isSpace(%U) = %v, want %v"u8, i, fmt_internal_test_package.IsSpace(i), Δunicode.IsSpace(i));
        }
    }
}

internal static @string hideFromVet(@string s) {
    return s;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sSSSSˢ = "%s %s %s %s %s"u8;

[GoType("dyn")] internal partial struct TestNilDoesNotBecomeTyped_A {
}

[GoType("dyn")] internal partial struct TestNilDoesNotBecomeTyped_B {
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string expectᶜ = "%!s(<nil>) %!s(*fmt_test.A=<nil>) %!s(<nil>) {} %!s(<nil>)"u8;

public static void TestNilDoesNotBecomeTyped(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ж<TestNilDoesNotBecomeTyped_A> a = default!;
    TestNilDoesNotBecomeTyped_B b = new TestNilDoesNotBecomeTyped_B(nil);
    @string got = Sprintf(hideFromVet(sSSSSˢ), (any)(default!), a.OrTypedNil(), (any)(default!), b, (any)(default!));
    @string expect = expectᶜ;
    if (got != expect) {
        Ꮡt.Errorf("expected:\n\t%q\ngot:\n\t%q"u8, expect, got);
    }
}

// scalar values with the (unused by fmt) 'a' verb.
// composite values with the 'a' verb
// simple values with the 'v' verb
// composite values with the 'v' verb.

[GoType("dyn")] partial struct formatterFlagTestsᴛ1 {
    internal @string @in;
    internal any val;
    internal @string @out;
}
internal static slice<formatterFlagTestsᴛ1> formatterFlagTests = new formatterFlagTestsᴛ1[]{
    new("%a"u8, new flagPrinter(nil), "[%a]"u8),
    new("%-a"u8, new flagPrinter(nil), "[%-a]"u8),
    new("%+a"u8, new flagPrinter(nil), "[%+a]"u8),
    new("%#a"u8, new flagPrinter(nil), "[%#a]"u8),
    new("% a"u8, new flagPrinter(nil), "[% a]"u8),
    new("%0a"u8, new flagPrinter(nil), "[%0a]"u8),
    new("%1.2a"u8, new flagPrinter(nil), "[%1.2a]"u8),
    new("%-1.2a"u8, new flagPrinter(nil), "[%-1.2a]"u8),
    new("%+1.2a"u8, new flagPrinter(nil), "[%+1.2a]"u8),
    new("%-+1.2a"u8, new flagPrinter(nil), "[%+-1.2a]"u8),
    new("%-+1.2abc"u8, new flagPrinter(nil), "[%+-1.2a]bc"u8),
    new("%-1.2abc"u8, new flagPrinter(nil), "[%-1.2a]bc"u8),
    new("%-0abc"u8, new flagPrinter(nil), "[%-0a]bc"u8),
    new("%a"u8, new flagPrinter[]{}.array(1), "[[%a]]"u8),
    new("%-a"u8, new flagPrinter[]{}.array(1), "[[%-a]]"u8),
    new("%+a"u8, new flagPrinter[]{}.array(1), "[[%+a]]"u8),
    new("%#a"u8, new flagPrinter[]{}.array(1), "[[%#a]]"u8),
    new("% a"u8, new flagPrinter[]{}.array(1), "[[% a]]"u8),
    new("%0a"u8, new flagPrinter[]{}.array(1), "[[%0a]]"u8),
    new("%1.2a"u8, new flagPrinter[]{}.array(1), "[[%1.2a]]"u8),
    new("%-1.2a"u8, new flagPrinter[]{}.array(1), "[[%-1.2a]]"u8),
    new("%+1.2a"u8, new flagPrinter[]{}.array(1), "[[%+1.2a]]"u8),
    new("%-+1.2a"u8, new flagPrinter[]{}.array(1), "[[%+-1.2a]]"u8),
    new("%-+1.2abc"u8, new flagPrinter[]{}.array(1), "[[%+-1.2a]]bc"u8),
    new("%-1.2abc"u8, new flagPrinter[]{}.array(1), "[[%-1.2a]]bc"u8),
    new("%-0abc"u8, new flagPrinter[]{}.array(1), "[[%-0a]]bc"u8),
    new("%v"u8, new flagPrinter(nil), "[%v]"u8),
    new("%-v"u8, new flagPrinter(nil), "[%-v]"u8),
    new("%+v"u8, new flagPrinter(nil), "[%+v]"u8),
    new("%#v"u8, new flagPrinter(nil), "[%#v]"u8),
    new("% v"u8, new flagPrinter(nil), "[% v]"u8),
    new("%0v"u8, new flagPrinter(nil), "[%0v]"u8),
    new("%1.2v"u8, new flagPrinter(nil), "[%1.2v]"u8),
    new("%-1.2v"u8, new flagPrinter(nil), "[%-1.2v]"u8),
    new("%+1.2v"u8, new flagPrinter(nil), "[%+1.2v]"u8),
    new("%-+1.2v"u8, new flagPrinter(nil), "[%+-1.2v]"u8),
    new("%-+1.2vbc"u8, new flagPrinter(nil), "[%+-1.2v]bc"u8),
    new("%-1.2vbc"u8, new flagPrinter(nil), "[%-1.2v]bc"u8),
    new("%-0vbc"u8, new flagPrinter(nil), "[%-0v]bc"u8),
    new("%v"u8, new flagPrinter[]{}.array(1), "[[%v]]"u8),
    new("%-v"u8, new flagPrinter[]{}.array(1), "[[%-v]]"u8),
    new("%+v"u8, new flagPrinter[]{}.array(1), "[[%+v]]"u8),
    new("%#v"u8, new flagPrinter[]{}.array(1), "[1]fmt_test.flagPrinter{[%#v]}"u8),
    new("% v"u8, new flagPrinter[]{}.array(1), "[[% v]]"u8),
    new("%0v"u8, new flagPrinter[]{}.array(1), "[[%0v]]"u8),
    new("%1.2v"u8, new flagPrinter[]{}.array(1), "[[%1.2v]]"u8),
    new("%-1.2v"u8, new flagPrinter[]{}.array(1), "[[%-1.2v]]"u8),
    new("%+1.2v"u8, new flagPrinter[]{}.array(1), "[[%+1.2v]]"u8),
    new("%-+1.2v"u8, new flagPrinter[]{}.array(1), "[[%+-1.2v]]"u8),
    new("%-+1.2vbc"u8, new flagPrinter[]{}.array(1), "[[%+-1.2v]]bc"u8),
    new("%-1.2vbc"u8, new flagPrinter[]{}.array(1), "[[%-1.2v]]bc"u8),
    new("%-0vbc"u8, new flagPrinter[]{}.array(1), "[[%-0v]]bc"u8)
}.slice();

public static void TestFormatterFlags(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, tt) in formatterFlagTests) {
        @string s = Sprintf(tt.@in, tt.val);
        if (s != tt.@out) {
            Ꮡt.Errorf("Sprintf(%q, %T) = %q, want %q"u8, tt.@in, tt.val, s, tt.@out);
        }
    }
}

[GoType("dyn")] internal partial struct TestParsenum_testCases {
    internal @string s;
    internal nint start, end;
    internal nint num;
    internal bool isnum;
    internal nint newi;
}

public static void TestParsenum(ж<Δtesting.T> Ꮡt) {
    var testCases = new TestParsenum_testCases[]{
        new("a123"u8, 0, 4, 0, false, 0),
        new("1234"u8, 1, 1, 0, false, 1),
        new("123a"u8, 0, 4, 123, true, 3),
        new("12a3"u8, 0, 4, 12, true, 2),
        new("1234"u8, 0, 4, 1234, true, 4),
        new("1a234"u8, 1, 3, 0, false, 1)
    }.slice();
    foreach (var (_, tt) in testCases) {
        var (num, isnum, newi) = fmt_internal_test_package.Parsenum(tt.s, tt.start, tt.end);
        if (num != tt.num || isnum != tt.isnum || newi != tt.newi) {
            Ꮡt.Errorf("parsenum(%q, %d, %d) = %d, %v, %d, want %d, %v, %d"u8, tt.s, tt.start, tt.end, num, isnum, newi, tt.num, tt.isnum, tt.newi);
        }
    }
}

// Test the various Append printers. The details are well tested above;
// here we just make sure the byte slice is updated.
internal static readonly @string appendResult = "hello world, 23"u8;
internal static readonly @string hello = "hello "u8;

public static void TestAppendf(ж<Δtesting.T> Ꮡt) {
    var b = new slice<byte>(100);
    b = b.slice(0, copy(b, hello));
    var got = Appendf(b, "world, %d"u8, (nint)(23));
    if (((sstring)got) != appendResult) {
        Ꮡt.Fatalf("Appendf returns %q not %q"u8, got, appendResult);
    }
    if (Ꮡ(b, 0) != Ꮡ(got, 0)) {
        Ꮡt.Fatalf("Appendf allocated a new slice"u8);
    }
}

public static void TestAppend(ж<Δtesting.T> Ꮡt) {
    var b = new slice<byte>(100);
    b = b.slice(0, copy(b, hello));
    var got = Append(b, worldˢ, (@string)", "u8, (nint)(23));
    if (((sstring)got) != appendResult) {
        Ꮡt.Fatalf("Append returns %q not %q"u8, got, appendResult);
    }
    if (Ꮡ(b, 0) != Ꮡ(got, 0)) {
        Ꮡt.Fatalf("Append allocated a new slice"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object worldˢ2 = (@string)"world,"u8;

public static void TestAppendln(ж<Δtesting.T> Ꮡt) {
    var b = new slice<byte>(100);
    b = b.slice(0, copy(b, hello));
    var got = Appendln(b, worldˢ2, (nint)(23));
    if (((@string)got) != appendResult + "\n") {
        Ꮡt.Fatalf("Appendln returns %q not %q"u8, got, appendResult + "\n");
    }
    if (Ꮡ(b, 0) != Ꮡ(got, 0)) {
        Ꮡt.Fatalf("Appendln allocated a new slice"u8);
    }
}

} // end fmt_test_package
