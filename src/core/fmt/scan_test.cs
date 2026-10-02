// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bufio = bufio_package;
using bytes = bytes_package;
using errors = errors_package;
using static fmt_package;
using Δio = io_package;
using Δmath = math_package;
using reflect = reflect_package;
using Δregexp = regexp_package;
using strings = strings_package;
using Δtesting = testing_package;
using iotest = go.testing.iotest_package;
using utf8 = go.unicode.utf8_package;
using fmt = fmt_package;
using go.testing;
using go.unicode;
using static go.fmt_internal_test_package;

partial class fmt_test_package {

[GoType] partial struct ScanTest {
    internal @string text;
    internal any @in;
    internal any @out;
}

[GoType] partial struct ScanfTest {
    internal @string format;
    internal @string text;
    internal any @in;
    internal any @out;
}

[GoType] partial struct ScanfMultiTest {
    internal @string format;
    internal @string text;
    internal slice<any> @in;
    internal slice<any> @out;
    internal @string err;
}

internal static ж<bool> ᏑboolVal = new StandardBox<bool>(default(bool));
internal static ref bool boolVal => ref ᏑboolVal.Value;
internal static ж<nint> ᏑintVal = new StandardBox<nint>(default(nint));
internal static ref nint intVal => ref ᏑintVal.Value;
internal static ж<int8> Ꮡint8Val = new StandardBox<int8>(default(int8));
internal static ref int8 int8Val => ref Ꮡint8Val.Value;
internal static ж<int16> Ꮡint16Val = new StandardBox<int16>(default(int16));
internal static ref int16 int16Val => ref Ꮡint16Val.Value;
internal static ж<int32> Ꮡint32Val = new StandardBox<int32>(default(int32));
internal static ref int32 int32Val => ref Ꮡint32Val.Value;
internal static ж<int64> Ꮡint64Val = new StandardBox<int64>(default(int64));
internal static ref int64 int64Val => ref Ꮡint64Val.Value;
internal static ж<nuint> ᏑuintVal = new StandardBox<nuint>(default(nuint));
internal static ref nuint uintVal => ref ᏑuintVal.Value;
internal static ж<uint8> Ꮡuint8Val = new StandardBox<uint8>(default(uint8));
internal static ref uint8 uint8Val => ref Ꮡuint8Val.Value;
internal static ж<uint16> Ꮡuint16Val = new StandardBox<uint16>(default(uint16));
internal static ref uint16 uint16Val => ref Ꮡuint16Val.Value;
internal static ж<uint32> Ꮡuint32Val = new StandardBox<uint32>(default(uint32));
internal static ref uint32 uint32Val => ref Ꮡuint32Val.Value;
internal static ж<uint64> Ꮡuint64Val = new StandardBox<uint64>(default(uint64));
internal static ref uint64 uint64Val => ref Ꮡuint64Val.Value;
internal static ж<uintptr> ᏑuintptrVal = new StandardBox<uintptr>(default(uintptr));
internal static ref uintptr uintptrVal => ref ᏑuintptrVal.Value;
internal static ж<float32> Ꮡfloat32Val = new StandardBox<float32>(default(float32));
internal static ref float32 float32Val => ref Ꮡfloat32Val.Value;
internal static ж<float64> Ꮡfloat64Val = new StandardBox<float64>(default(float64));
internal static ref float64 float64Val => ref Ꮡfloat64Val.Value;
internal static ж<@string> ᏑstringVal = new StandardBox<@string>(default(@string));
internal static ref @string stringVal => ref ᏑstringVal.Value;
internal static ж<slice<byte>> ᏑbytesVal = new StandardBox<slice<byte>>(default(slice<byte>));
internal static ref slice<byte> bytesVal => ref ᏑbytesVal.ValueSlot;
internal static ж<rune> ᏑruneVal = new StandardBox<rune>(default(rune));
internal static ref rune runeVal => ref ᏑruneVal.Value;
internal static ж<complex64> Ꮡcomplex64Val = new StandardBox<complex64>(default(complex64));
internal static ref complex64 complex64Val => ref Ꮡcomplex64Val.Value;
internal static ж<complex128> Ꮡcomplex128Val = new StandardBox<complex128>(default(complex128));
internal static ref complex128 complex128Val => ref Ꮡcomplex128Val.Value;
internal static ж<renamedBool> ᏑrenamedBoolVal = new StandardBox<renamedBool>(default(renamedBool));
internal static ref renamedBool renamedBoolVal => ref ᏑrenamedBoolVal.Value;
internal static ж<renamedInt> ᏑrenamedIntVal = new StandardBox<renamedInt>(default(renamedInt));
internal static ref renamedInt renamedIntVal => ref ᏑrenamedIntVal.Value;
internal static ж<renamedInt8> ᏑrenamedInt8Val = new StandardBox<renamedInt8>(default(renamedInt8));
internal static ref renamedInt8 renamedInt8Val => ref ᏑrenamedInt8Val.Value;
internal static ж<renamedInt16> ᏑrenamedInt16Val = new StandardBox<renamedInt16>(default(renamedInt16));
internal static ref renamedInt16 renamedInt16Val => ref ᏑrenamedInt16Val.Value;
internal static ж<renamedInt32> ᏑrenamedInt32Val = new StandardBox<renamedInt32>(default(renamedInt32));
internal static ref renamedInt32 renamedInt32Val => ref ᏑrenamedInt32Val.Value;
internal static ж<renamedInt64> ᏑrenamedInt64Val = new StandardBox<renamedInt64>(default(renamedInt64));
internal static ref renamedInt64 renamedInt64Val => ref ᏑrenamedInt64Val.Value;
internal static ж<renamedUint> ᏑrenamedUintVal = new StandardBox<renamedUint>(default(renamedUint));
internal static ref renamedUint renamedUintVal => ref ᏑrenamedUintVal.Value;
internal static ж<renamedUint8> ᏑrenamedUint8Val = new StandardBox<renamedUint8>(default(renamedUint8));
internal static ref renamedUint8 renamedUint8Val => ref ᏑrenamedUint8Val.Value;
internal static ж<renamedUint16> ᏑrenamedUint16Val = new StandardBox<renamedUint16>(default(renamedUint16));
internal static ref renamedUint16 renamedUint16Val => ref ᏑrenamedUint16Val.Value;
internal static ж<renamedUint32> ᏑrenamedUint32Val = new StandardBox<renamedUint32>(default(renamedUint32));
internal static ref renamedUint32 renamedUint32Val => ref ᏑrenamedUint32Val.Value;
internal static ж<renamedUint64> ᏑrenamedUint64Val = new StandardBox<renamedUint64>(default(renamedUint64));
internal static ref renamedUint64 renamedUint64Val => ref ᏑrenamedUint64Val.Value;
internal static ж<renamedUintptr> ᏑrenamedUintptrVal = new StandardBox<renamedUintptr>(default(renamedUintptr));
internal static ref renamedUintptr renamedUintptrVal => ref ᏑrenamedUintptrVal.Value;
internal static ж<renamedString> ᏑrenamedStringVal = new StandardBox<renamedString>(default(renamedString));
internal static ref renamedString renamedStringVal => ref ᏑrenamedStringVal.Value;
internal static ж<renamedBytes> ᏑrenamedBytesVal = new StandardBox<renamedBytes>(default(renamedBytes));
internal static ref renamedBytes renamedBytesVal => ref ᏑrenamedBytesVal.ValueSlot;
internal static ж<renamedFloat32> ᏑrenamedFloat32Val = new StandardBox<renamedFloat32>(default(renamedFloat32));
internal static ref renamedFloat32 renamedFloat32Val => ref ᏑrenamedFloat32Val.Value;
internal static ж<renamedFloat64> ᏑrenamedFloat64Val = new StandardBox<renamedFloat64>(default(renamedFloat64));
internal static ref renamedFloat64 renamedFloat64Val => ref ᏑrenamedFloat64Val.Value;
internal static ж<renamedComplex64> ᏑrenamedComplex64Val = new StandardBox<renamedComplex64>(default(renamedComplex64));
internal static ref renamedComplex64 renamedComplex64Val => ref ᏑrenamedComplex64Val.Value;
internal static ж<renamedComplex128> ᏑrenamedComplex128Val = new StandardBox<renamedComplex128>(default(renamedComplex128));
internal static ref renamedComplex128 renamedComplex128Val => ref ᏑrenamedComplex128Val.Value;

[GoType("@string")] partial struct Xs;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string syntaxErrorForXsˢ = "syntax error for xs"u8;

[GoRecv] public static error Scan(this ref Xs x, fmt.ScanState state, rune verb) {
    var (tok, err) = state.Token(true, (rune r) => r == verb);
    if (err != default!) {
        return err;
    }
    @string s = ((@string)tok);
    if (!Δregexp.MustCompile("^"u8 + ((@string)verb) + "+$"u8).MatchString(s)) {
        return errors.New(syntaxErrorForXsˢ);
    }
    x = ((Xs)s);
    return default!;
}

internal static ж<Xs> ᏑxVal = new StandardBox<Xs>(default(Xs));
internal static ref Xs xVal => ref ᏑxVal.Value;

// IntString accepts an integer followed immediately by a string.
// It tests the embedding of a scan within a scan.
[GoType] partial struct IntString {
    internal nint i;
    internal @string s;
}

public static error Scan(this ж<IntString> Ꮡs, fmt.ScanState state, rune verb) {
    ref var s = ref Ꮡs.DerefOrNull();

    {
        var (_, errΔ1) = Fscan(new fmt_test_package.fmt_ScanStateᴠReader(state), Ꮡs.of(IntString.Ꮡi)); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    var (tok, err) = state.Token(true, default!);
    if (err != default!) {
        return err;
    }
    s.s = ((@string)tok);
    return default!;
}

internal static ж<IntString> ᏑintStringVal = new StandardBox<IntString>(default(IntString));
internal static ref IntString intStringVal => ref ᏑintStringVal.Value;

// Basic types
// boolean test vals toggle to be sure they are written
// restored to zero value
// Carriage-return followed by newline. (We treat \r\n as \n always.)
// Renamed types
// Custom scanners.
// Fixed bugs
// was: integer overflow
internal static slice<ScanTest> scanTests = new ScanTest[]{
    new("T\n"u8, ᏑboolVal, true),
    new("F\n"u8, ᏑboolVal, false),
    new("21\n"u8, ᏑintVal, (nint)(21)),
    new("2_1\n"u8, ᏑintVal, (nint)(21)),
    new("0\n"u8, ᏑintVal, (nint)(0)),
    new("000\n"u8, ᏑintVal, (nint)(0)),
    new("0x10\n"u8, ᏑintVal, (nint)(0x10)),
    new("0x_1_0\n"u8, ᏑintVal, (nint)(0x10)),
    new("-0x10\n"u8, ᏑintVal, (nint)(-0x10)),
    new("0377\n"u8, ᏑintVal, (nint)(255)),
    new("0_3_7_7\n"u8, ᏑintVal, (nint)(255)),
    new("0o377\n"u8, ᏑintVal, (nint)(255)),
    new("0o_3_7_7\n"u8, ᏑintVal, (nint)(255)),
    new("-0377\n"u8, ᏑintVal, (nint)(-255)),
    new("-0o377\n"u8, ᏑintVal, (nint)(-255)),
    new("0\n"u8, ᏑuintVal, (nuint)0),
    new("000\n"u8, ᏑuintVal, (nuint)0),
    new("0x10\n"u8, ᏑuintVal, (nuint)0x10),
    new("0377\n"u8, ᏑuintVal, (nuint)255),
    new("22\n"u8, Ꮡint8Val, (int8)22),
    new("23\n"u8, Ꮡint16Val, (int16)23),
    new("24\n"u8, Ꮡint32Val, (int32)24),
    new("25\n"u8, Ꮡint64Val, (int64)25),
    new("127\n"u8, Ꮡint8Val, (int8)127),
    new("-21\n"u8, ᏑintVal, (nint)(-21)),
    new("-22\n"u8, Ꮡint8Val, (int8)(-22)),
    new("-23\n"u8, Ꮡint16Val, (int16)(-23)),
    new("-24\n"u8, Ꮡint32Val, (int32)(-24)),
    new("-25\n"u8, Ꮡint64Val, (int64)(-25)),
    new("-128\n"u8, Ꮡint8Val, (int8)(-128)),
    new("+21\n"u8, ᏑintVal, (nint)(+21)),
    new("+22\n"u8, Ꮡint8Val, (int8)(+22)),
    new("+23\n"u8, Ꮡint16Val, (int16)(+23)),
    new("+24\n"u8, Ꮡint32Val, (int32)(+24)),
    new("+25\n"u8, Ꮡint64Val, (int64)(+25)),
    new("+127\n"u8, Ꮡint8Val, (int8)(+127)),
    new("26\n"u8, ᏑuintVal, (nuint)26),
    new("27\n"u8, Ꮡuint8Val, (uint8)27),
    new("28\n"u8, Ꮡuint16Val, (uint16)28),
    new("29\n"u8, Ꮡuint32Val, (uint32)29),
    new("30\n"u8, Ꮡuint64Val, (uint64)30),
    new("31\n"u8, ᏑuintptrVal, (uintptr)31),
    new("255\n"u8, Ꮡuint8Val, (uint8)255),
    new("32767\n"u8, Ꮡint16Val, (int16)32767),
    new("2.3\n"u8, Ꮡfloat64Val, 2.3D),
    new("2.3e1\n"u8, Ꮡfloat32Val, (float32)2.3e1F),
    new("2.3e2\n"u8, Ꮡfloat64Val, 2.3e2D),
    new("2.3p2\n"u8, Ꮡfloat64Val, 2.3D * 4D),
    new("2.3p+2\n"u8, Ꮡfloat64Val, 2.3D * 4D),
    new("2.3p+66\n"u8, Ꮡfloat64Val, 2.3D * (73786976294838206464D)),
    new("2.3p-66\n"u8, Ꮡfloat64Val, 2.3D / (73786976294838206464D)),
    new("0x2.3p-66\n"u8, Ꮡfloat64Val, (float64)35D / (1180591620717411303424D)),
    new("2_3.4_5\n"u8, Ꮡfloat64Val, 23.45D),
    new("2.35\n"u8, ᏑstringVal, (@string)"2.35"u8),
    new("2345678\n"u8, ᏑbytesVal, slice<byte>("2345678"u8)),
    new("(3.4e1-2i)\n"u8, Ꮡcomplex128Val, 34D + -2D.i()),
    new("-3.45e1-3i\n"u8, Ꮡcomplex64Val, (complex64)(-34.5F + -3F.i())),
    new("-.45e1-1e2i\n"u8, Ꮡcomplex128Val, (complex128)(-4.5D + -100D.i())),
    new("-.4_5e1-1E2i\n"u8, Ꮡcomplex128Val, (complex128)(-4.5D + -100D.i())),
    new("0x1.0p1+0x1.0P2i\n"u8, Ꮡcomplex128Val, (complex128)(2D + 4D.i())),
    new("-0x1p1-0x1p2i\n"u8, Ꮡcomplex128Val, (complex128)(-2D + -4D.i())),
    new("-0x1ep-1-0x1p2i\n"u8, Ꮡcomplex128Val, (complex128)(-15D + -4D.i())),
    new("-0x1_Ep-1-0x1p0_2i\n"u8, Ꮡcomplex128Val, (complex128)(-15D + -4D.i())),
    new("hello\n"u8, ᏑstringVal, (@string)"hello"u8),
    new("hello\r\n"u8, ᏑstringVal, (@string)"hello"u8),
    new("27\r\n"u8, Ꮡuint8Val, (uint8)27),
    new("true\n"u8, ᏑrenamedBoolVal, ((renamedBool)true)),
    new("F\n"u8, ᏑrenamedBoolVal, ((renamedBool)false)),
    new("101\n"u8, ᏑrenamedIntVal, ((renamedInt)101)),
    new("102\n"u8, ᏑrenamedIntVal, ((renamedInt)102)),
    new("103\n"u8, ᏑrenamedUintVal, ((renamedUint)103)),
    new("104\n"u8, ᏑrenamedUintVal, ((renamedUint)104)),
    new("105\n"u8, ᏑrenamedInt8Val, ((renamedInt8)105)),
    new("106\n"u8, ᏑrenamedInt16Val, ((renamedInt16)106)),
    new("107\n"u8, ᏑrenamedInt32Val, ((renamedInt32)107)),
    new("108\n"u8, ᏑrenamedInt64Val, ((renamedInt64)108)),
    new("109\n"u8, ᏑrenamedUint8Val, ((renamedUint8)109)),
    new("110\n"u8, ᏑrenamedUint16Val, ((renamedUint16)110)),
    new("111\n"u8, ᏑrenamedUint32Val, ((renamedUint32)111)),
    new("112\n"u8, ᏑrenamedUint64Val, ((renamedUint64)112)),
    new("113\n"u8, ᏑrenamedUintptrVal, ((renamedUintptr)113)),
    new("114\n"u8, ᏑrenamedStringVal, ((renamedString)(@string)"114"u8)),
    new("115\n"u8, ᏑrenamedBytesVal, ((renamedBytes)slice<byte>("115"u8))),
    new("  vvv "u8, ᏑxVal, ((Xs)(@string)"vvv"u8)),
    new(" 1234hello"u8, ᏑintStringVal, new IntString(1234, "hello"u8)),
    new("2147483648\n"u8, Ꮡint64Val, (int64)2147483648L)
}.slice();

// only %v takes underscores
// only %v takes underscores
// only %v takes underscores
// Strings
// Byte slices
// Renamed types
// Interesting formats
// %% at end of string.
// Corner cases
// Custom scanner.
// Fixed bugs
// ok
// was: "unexpected newline"
// was: "EOF"; 0 was taken as base prefix and not counted.
// was: "EOF"; 0 was taken as base prefix and not counted.
// %c must accept a blank.
// %c must accept any space.
// %c must accept any space.
// %% matches literal %.
// %% matches literal %.
// space handling
// expected space in input to match format
// expected space in input to match format
// input does not match format
// input does not match format
// expected space in input to match format
// expected space in input to match format
// expected space in input to match format
// expected space in input to match format
// expected space in input to match format
// expected space in input to match format
// expected space in input to match format
// unexpected EOF
// unexpected EOF
// input does not match format
// unexpected EOF
// input does not match format
// input does not match format
// input does not match format
// input does not match format
// input does not match format
// expected space in input to match format
// newline in input does not match format
// expected space in input to match format
// expected space in input to match format
// input does not match format
// input does not match format
// expected space in input to match format
// expected space in input to match format
internal static slice<ScanfTest> scanfTests = new ScanfTest[]{
    new("%v"u8, "TRUE\n"u8, ᏑboolVal, true),
    new("%t"u8, "false\n"u8, ᏑboolVal, false),
    new("%v"u8, "-71\n"u8, ᏑintVal, (nint)(-71)),
    new("%v"u8, "-7_1\n"u8, ᏑintVal, (nint)(-71)),
    new("%v"u8, "0b111\n"u8, ᏑintVal, (nint)(7)),
    new("%v"u8, "0b_1_1_1\n"u8, ᏑintVal, (nint)(7)),
    new("%v"u8, "0377\n"u8, ᏑintVal, (nint)(255)),
    new("%v"u8, "0_3_7_7\n"u8, ᏑintVal, (nint)(255)),
    new("%v"u8, "0o377\n"u8, ᏑintVal, (nint)(255)),
    new("%v"u8, "0o_3_7_7\n"u8, ᏑintVal, (nint)(255)),
    new("%v"u8, "0x44\n"u8, ᏑintVal, (nint)(0x44)),
    new("%v"u8, "0x_4_4\n"u8, ᏑintVal, (nint)(0x44)),
    new("%d"u8, "72\n"u8, ᏑintVal, (nint)(72)),
    new("%c"u8, "a\n"u8, ᏑruneVal, (rune)'a'),
    new("%c"u8, "\u5072\n"u8, ᏑruneVal, (rune)'\u5072'),
    new("%c"u8, "\u1234\n"u8, ᏑruneVal, (rune)'\u1234'),
    new("%d"u8, "73\n"u8, Ꮡint8Val, (int8)73),
    new("%d"u8, "+74\n"u8, Ꮡint16Val, (int16)74),
    new("%d"u8, "75\n"u8, Ꮡint32Val, (int32)75),
    new("%d"u8, "76\n"u8, Ꮡint64Val, (int64)76),
    new("%b"u8, "1001001\n"u8, ᏑintVal, (nint)(73)),
    new("%o"u8, "075\n"u8, ᏑintVal, (nint)(61)),
    new("%x"u8, "a75\n"u8, ᏑintVal, (nint)(0xa75)),
    new("%v"u8, "71\n"u8, ᏑuintVal, (nuint)71),
    new("%d"u8, "72\n"u8, ᏑuintVal, (nuint)72),
    new("%d"u8, "7_2\n"u8, ᏑuintVal, (nuint)7),
    new("%d"u8, "73\n"u8, Ꮡuint8Val, (uint8)73),
    new("%d"u8, "74\n"u8, Ꮡuint16Val, (uint16)74),
    new("%d"u8, "75\n"u8, Ꮡuint32Val, (uint32)75),
    new("%d"u8, "76\n"u8, Ꮡuint64Val, (uint64)76),
    new("%d"u8, "77\n"u8, ᏑuintptrVal, (uintptr)77),
    new("%b"u8, "1001001\n"u8, ᏑuintVal, (nuint)73),
    new("%b"u8, "100_1001\n"u8, ᏑuintVal, (nuint)4),
    new("%o"u8, "075\n"u8, ᏑuintVal, (nuint)61),
    new("%o"u8, "07_5\n"u8, ᏑuintVal, (nuint)7),
    new("%x"u8, "a75\n"u8, ᏑuintVal, (nuint)0xa75),
    new("%x"u8, "A75\n"u8, ᏑuintVal, (nuint)0xa75),
    new("%x"u8, "A7_5\n"u8, ᏑuintVal, (nuint)0xa7),
    new("%U"u8, "U+1234\n"u8, ᏑintVal, (nint)0x1234),
    new("%U"u8, "U+4567\n"u8, ᏑuintVal, (nuint)0x4567),
    new("%e"u8, "2.3\n"u8, Ꮡfloat64Val, 2.3D),
    new("%E"u8, "2.3e1\n"u8, Ꮡfloat32Val, (float32)2.3e1F),
    new("%f"u8, "2.3e2\n"u8, Ꮡfloat64Val, 2.3e2D),
    new("%g"u8, "2.3p2\n"u8, Ꮡfloat64Val, 2.3D * 4D),
    new("%G"u8, "2.3p+2\n"u8, Ꮡfloat64Val, 2.3D * 4D),
    new("%v"u8, "2.3p+66\n"u8, Ꮡfloat64Val, 2.3D * (73786976294838206464D)),
    new("%f"u8, "2.3p-66\n"u8, Ꮡfloat64Val, 2.3D / (73786976294838206464D)),
    new("%G"u8, "0x2.3p-66\n"u8, Ꮡfloat64Val, (float64)35D / (1180591620717411303424D)),
    new("%E"u8, "2_3.4_5\n"u8, Ꮡfloat64Val, 23.45D),
    new("%s"u8, "using-%s\n"u8, ᏑstringVal, (@string)"using-%s"u8),
    new("%x"u8, "7573696e672d2578\n"u8, ᏑstringVal, (@string)"using-%x"u8),
    new("%X"u8, "7573696E672D2558\n"u8, ᏑstringVal, (@string)"using-%X"u8),
    new("%q"u8, @"""quoted\twith\\do\u0075bl\x65s"""u8 + "\n"u8, ᏑstringVal, (@string)"quoted\twith\\doubles"u8),
    new("%q"u8, "`quoted with backs`\n"u8, ᏑstringVal, (@string)"quoted with backs"u8),
    new("%s"u8, "bytes-%s\n"u8, ᏑbytesVal, slice<byte>("bytes-%s"u8)),
    new("%x"u8, "62797465732d2578\n"u8, ᏑbytesVal, slice<byte>("bytes-%x"u8)),
    new("%X"u8, "62797465732D2558\n"u8, ᏑbytesVal, slice<byte>("bytes-%X"u8)),
    new("%q"u8, @"""bytes\rwith\vdo\u0075bl\x65s"""u8 + "\n"u8, ᏑbytesVal, slice<byte>("bytes\rwith\vdoubles"u8)),
    new("%q"u8, "`bytes with backs`\n"u8, ᏑbytesVal, slice<byte>("bytes with backs"u8)),
    new("%v\n"u8, "true\n"u8, ᏑrenamedBoolVal, ((renamedBool)true)),
    new("%t\n"u8, "F\n"u8, ᏑrenamedBoolVal, ((renamedBool)false)),
    new("%v"u8, "101\n"u8, ᏑrenamedIntVal, ((renamedInt)101)),
    new("%c"u8, "\u0101\n"u8, ᏑrenamedIntVal, ((renamedInt)(rune)'\u0101')),
    new("%o"u8, "0146\n"u8, ᏑrenamedIntVal, ((renamedInt)102)),
    new("%v"u8, "103\n"u8, ᏑrenamedUintVal, ((renamedUint)103)),
    new("%d"u8, "104\n"u8, ᏑrenamedUintVal, ((renamedUint)104)),
    new("%d"u8, "105\n"u8, ᏑrenamedInt8Val, ((renamedInt8)105)),
    new("%d"u8, "106\n"u8, ᏑrenamedInt16Val, ((renamedInt16)106)),
    new("%d"u8, "107\n"u8, ᏑrenamedInt32Val, ((renamedInt32)107)),
    new("%d"u8, "108\n"u8, ᏑrenamedInt64Val, ((renamedInt64)108)),
    new("%x"u8, "6D\n"u8, ᏑrenamedUint8Val, ((renamedUint8)109)),
    new("%o"u8, "0156\n"u8, ᏑrenamedUint16Val, ((renamedUint16)110)),
    new("%d"u8, "111\n"u8, ᏑrenamedUint32Val, ((renamedUint32)111)),
    new("%d"u8, "112\n"u8, ᏑrenamedUint64Val, ((renamedUint64)112)),
    new("%d"u8, "113\n"u8, ᏑrenamedUintptrVal, ((renamedUintptr)113)),
    new("%s"u8, "114\n"u8, ᏑrenamedStringVal, ((renamedString)(@string)"114"u8)),
    new("%q"u8, "\"1155\"\n"u8, ᏑrenamedBytesVal, ((renamedBytes)slice<byte>("1155"u8))),
    new("%g"u8, "116e1\n"u8, ᏑrenamedFloat32Val, ((renamedFloat32)116e1F)),
    new("%g"u8, "-11.7e+1"u8, ᏑrenamedFloat64Val, ((renamedFloat64)(-11.7e+1D))),
    new("%g"u8, "11+6e1i\n"u8, ᏑrenamedComplex64Val, ((renamedComplex64)(11F + 60F.i()))),
    new("%g"u8, "-11.+7e+1i"u8, ᏑrenamedComplex128Val, ((renamedComplex128)(-11D + 70D.i()))),
    new("here is\tthe value:%d"u8, "here is   the\tvalue:118\n"u8, ᏑintVal, (nint)(118)),
    new("%% %%:%d"u8, "% %:119\n"u8, ᏑintVal, (nint)(119)),
    new("%d%%"u8, "42%"u8, ᏑintVal, (nint)(42)),
    new("%x"u8, "FFFFFFFF\n"u8, Ꮡuint32Val, (uint32)0xFFFFFFFFU),
    new("%s"u8, "  sss "u8, ᏑxVal, ((Xs)(@string)"sss"u8)),
    new("%2s"u8, "sssss"u8, ᏑxVal, ((Xs)(@string)"ss"u8)),
    new("%d\n"u8, "27\n"u8, ᏑintVal, (nint)(27)),
    new("%d\n"u8, "28 \n"u8, ᏑintVal, (nint)(28)),
    new("%v"u8, "0"u8, ᏑintVal, (nint)(0)),
    new("%v"u8, "0"u8, ᏑuintVal, (nuint)0),
    new("%c"u8, " "u8, ᏑuintVal, (nuint)(rune)' '),
    new("%c"u8, "\t"u8, ᏑuintVal, (nuint)(rune)'\t'),
    new("%c"u8, "\n"u8, ᏑuintVal, (nuint)(rune)'\n'),
    new("%d%%"u8, "23%\n"u8, ᏑuintVal, (nuint)23),
    new("%%%d"u8, "%23\n"u8, ᏑuintVal, (nuint)23),
    new("%d"u8, "27"u8, ᏑintVal, (nint)(27)),
    new("%d"u8, "27 "u8, ᏑintVal, (nint)(27)),
    new("%d"u8, " 27"u8, ᏑintVal, (nint)(27)),
    new("%d"u8, " 27 "u8, ᏑintVal, (nint)(27)),
    new("X%d"u8, "X27"u8, ᏑintVal, (nint)(27)),
    new("X%d"u8, "X27 "u8, ᏑintVal, (nint)(27)),
    new("X%d"u8, "X 27"u8, ᏑintVal, (nint)(27)),
    new("X%d"u8, "X 27 "u8, ᏑintVal, (nint)(27)),
    new("X %d"u8, "X27"u8, ᏑintVal, default!),
    new("X %d"u8, "X27 "u8, ᏑintVal, default!),
    new("X %d"u8, "X 27"u8, ᏑintVal, (nint)(27)),
    new("X %d"u8, "X 27 "u8, ᏑintVal, (nint)(27)),
    new("%dX"u8, "27X"u8, ᏑintVal, (nint)(27)),
    new("%dX"u8, "27 X"u8, ᏑintVal, default!),
    new("%dX"u8, " 27X"u8, ᏑintVal, (nint)(27)),
    new("%dX"u8, " 27 X"u8, ᏑintVal, default!),
    new("%d X"u8, "27X"u8, ᏑintVal, default!),
    new("%d X"u8, "27 X"u8, ᏑintVal, (nint)(27)),
    new("%d X"u8, " 27X"u8, ᏑintVal, default!),
    new("%d X"u8, " 27 X"u8, ᏑintVal, (nint)(27)),
    new("X %d X"u8, "X27X"u8, ᏑintVal, default!),
    new("X %d X"u8, "X27 X"u8, ᏑintVal, default!),
    new("X %d X"u8, "X 27X"u8, ᏑintVal, default!),
    new("X %d X"u8, "X 27 X"u8, ᏑintVal, (nint)(27)),
    new("X %s X"u8, "X27X"u8, ᏑstringVal, default!),
    new("X %s X"u8, "X27 X"u8, ᏑstringVal, default!),
    new("X %s X"u8, "X 27X"u8, ᏑstringVal, default!),
    new("X %s X"u8, "X 27 X"u8, ᏑstringVal, (@string)"27"u8),
    new("X%sX"u8, "X27X"u8, ᏑstringVal, default!),
    new("X%sX"u8, "X27 X"u8, ᏑstringVal, default!),
    new("X%sX"u8, "X 27X"u8, ᏑstringVal, default!),
    new("X%sX"u8, "X 27 X"u8, ᏑstringVal, default!),
    new("X%s"u8, "X27"u8, ᏑstringVal, (@string)"27"u8),
    new("X%s"u8, "X27 "u8, ᏑstringVal, (@string)"27"u8),
    new("X%s"u8, "X 27"u8, ᏑstringVal, (@string)"27"u8),
    new("X%s"u8, "X 27 "u8, ᏑstringVal, (@string)"27"u8),
    new("X%dX"u8, "X27X"u8, ᏑintVal, (nint)(27)),
    new("X%dX"u8, "X27 X"u8, ᏑintVal, default!),
    new("X%dX"u8, "X 27X"u8, ᏑintVal, (nint)(27)),
    new("X%dX"u8, "X 27 X"u8, ᏑintVal, default!),
    new("X%dX"u8, "X27X"u8, ᏑintVal, (nint)(27)),
    new("X%dX"u8, "X27X "u8, ᏑintVal, (nint)(27)),
    new("X%dX"u8, " X27X"u8, ᏑintVal, default!),
    new("X%dX"u8, " X27X "u8, ᏑintVal, default!),
    new("X%dX\n"u8, "X27X"u8, ᏑintVal, (nint)(27)),
    new("X%dX \n"u8, "X27X "u8, ᏑintVal, (nint)(27)),
    new("X%dX\n"u8, "X27X\n"u8, ᏑintVal, (nint)(27)),
    new("X%dX\n"u8, "X27X \n"u8, ᏑintVal, (nint)(27)),
    new("X%dX \n"u8, "X27X"u8, ᏑintVal, (nint)(27)),
    new("X%dX \n"u8, "X27X "u8, ᏑintVal, (nint)(27)),
    new("X%dX \n"u8, "X27X\n"u8, ᏑintVal, (nint)(27)),
    new("X%dX \n"u8, "X27X \n"u8, ᏑintVal, (nint)(27)),
    new("X%c"u8, "X\n"u8, ᏑruneVal, (rune)'\n'),
    new("X%c"u8, "X \n"u8, ᏑruneVal, (rune)' '),
    new("X %c"u8, "X!"u8, ᏑruneVal, default!),
    new("X %c"u8, "X\n"u8, ᏑruneVal, default!),
    new("X %c"u8, "X !"u8, ᏑruneVal, (rune)'!'),
    new("X %c"u8, "X \n"u8, ᏑruneVal, (rune)'\n'),
    new(" X%dX"u8, "X27X"u8, ᏑintVal, default!),
    new(" X%dX"u8, "X27X "u8, ᏑintVal, default!),
    new(" X%dX"u8, " X27X"u8, ᏑintVal, (nint)(27)),
    new(" X%dX"u8, " X27X "u8, ᏑintVal, (nint)(27)),
    new("X%dX "u8, "X27X"u8, ᏑintVal, (nint)(27)),
    new("X%dX "u8, "X27X "u8, ᏑintVal, (nint)(27)),
    new("X%dX "u8, " X27X"u8, ᏑintVal, default!),
    new("X%dX "u8, " X27X "u8, ᏑintVal, default!),
    new(" X%dX "u8, "X27X"u8, ᏑintVal, default!),
    new(" X%dX "u8, "X27X "u8, ᏑintVal, default!),
    new(" X%dX "u8, " X27X"u8, ᏑintVal, (nint)(27)),
    new(" X%dX "u8, " X27X "u8, ᏑintVal, (nint)(27)),
    new("%d\nX"u8, "27\nX"u8, ᏑintVal, (nint)(27)),
    new("%dX\n X"u8, "27X\n X"u8, ᏑintVal, (nint)(27))
}.slice();

internal static slice<ScanTest> overflowTests = new ScanTest[]{
    new("128"u8, Ꮡint8Val, (nint)(0)),
    new("32768"u8, Ꮡint16Val, (nint)(0)),
    new("-129"u8, Ꮡint8Val, (nint)(0)),
    new("-32769"u8, Ꮡint16Val, (nint)(0)),
    new("256"u8, Ꮡuint8Val, (nint)(0)),
    new("65536"u8, Ꮡuint16Val, (nint)(0)),
    new("1e100"u8, Ꮡfloat32Val, (nint)(0)),
    new("1e500"u8, Ꮡfloat64Val, (nint)(0)),
    new("(1e100+0i)"u8, Ꮡcomplex64Val, (nint)(0)),
    new("(1+1e100i)"u8, Ꮡcomplex64Val, (nint)(0)),
    new("(1-1e500i)"u8, Ꮡcomplex128Val, (nint)(0))
}.slice();

internal static ж<bool> Ꮡtruth = new StandardBox<bool>(default(bool));
internal static ref bool truth => ref Ꮡtruth.Value;

internal static ж<nint> Ꮡi = new StandardBox<nint>(default(nint));
internal static ref nint i => ref Ꮡi.Value;
internal static ж<nint> Ꮡj = new StandardBox<nint>(default(nint));
internal static ref nint j => ref Ꮡj.Value;
internal static ж<nint> Ꮡk = new StandardBox<nint>(default(nint));
internal static ref nint k => ref Ꮡk.Value;

internal static ж<float64> Ꮡf = new StandardBox<float64>(default(float64));
internal static ref float64 f => ref Ꮡf.Value;

internal static ж<@string> Ꮡs = new StandardBox<@string>(default(@string));
internal static ref @string s => ref Ꮡs.Value;
internal static ж<@string> Ꮡt = new StandardBox<@string>(default(@string));
internal static ref @string t => ref Ꮡt.Value;

internal static ж<complex128> Ꮡc = new StandardBox<complex128>(default(complex128));
internal static ref complex128 c => ref Ꮡc.Value;

internal static ж<Xs> Ꮡx = new StandardBox<Xs>(default(Xs));
internal static ref Xs x => ref Ꮡx.Value;
internal static ж<Xs> Ꮡy = new StandardBox<Xs>(default(Xs));
internal static ref Xs y => ref Ꮡy.Value;

internal static ж<IntString> Ꮡz = new StandardBox<IntString>(default(IntString));
internal static ref IntString z => ref Ꮡz.Value;

internal static ж<rune> Ꮡr1 = new StandardBox<rune>(default(rune));
internal static ref rune r1 => ref Ꮡr1.Value;
internal static ж<rune> Ꮡr2 = new StandardBox<rune>(default(rune));
internal static ref rune r2 => ref Ꮡr2.Value;
internal static ж<rune> Ꮡr3 = new StandardBox<rune>(default(rune));
internal static ref rune r3 => ref Ꮡr3.Value;

// Custom scanners.
// Errors
// Slightly odd error, but correct.
// Bad UTF-8: should see every byte.
// Fixed bugs
internal static slice<ScanfMultiTest> multiTests = new ScanfMultiTest[]{
    new(""u8, ""u8, new any[]{}.slice(), new any[]{}.slice(), ""u8),
    new("%d"u8, "23"u8, args(Ꮡi), args((nint)(23)), ""u8),
    new("%2s%3s"u8, "22333"u8, args(Ꮡs, Ꮡt), args((@string)"22"u8, (@string)"333"u8), ""u8),
    new("%2d%3d"u8, "44555"u8, args(Ꮡi, Ꮡj), args((nint)(44), (nint)(555)), ""u8),
    new("%2d.%3d"u8, "66.777"u8, args(Ꮡi, Ꮡj), args((nint)(66), (nint)(777)), ""u8),
    new("%d, %d"u8, "23, 18"u8, args(Ꮡi, Ꮡj), args((nint)(23), (nint)(18)), ""u8),
    new("%3d22%3d"u8, "33322333"u8, args(Ꮡi, Ꮡj), args((nint)(333), (nint)(333)), ""u8),
    new("%6vX=%3fY"u8, "3+2iX=2.5Y"u8, args(Ꮡc, Ꮡf), args((3D + 2D.i()), 2.5D), ""u8),
    new("%d%s"u8, "123abc"u8, args(Ꮡi, Ꮡs), args((nint)(123), (@string)"abc"u8), ""u8),
    new("%c%c%c"u8, "2\u50c2X"u8, args(Ꮡr1, Ꮡr2, Ꮡr3), args((rune)'2', (rune)'\u50c2', (rune)'X'), ""u8),
    new("%5s%d"u8, " 1234567 "u8, args(Ꮡs, Ꮡi), args((@string)"12345"u8, (nint)(67)), ""u8),
    new("%5s%d"u8, " 12 34 567 "u8, args(Ꮡs, Ꮡi), args((@string)"12"u8, (nint)(34)), ""u8),
    new("%e%f"u8, "eefffff"u8, args(Ꮡx, Ꮡy), args(((Xs)(@string)"ee"u8), ((Xs)(@string)"fffff"u8)), ""u8),
    new("%4v%s"u8, "12abcd"u8, args(Ꮡz, Ꮡs), args(new IntString(12, "ab"u8), (@string)"cd"u8), ""u8),
    new("%t"u8, "23 18"u8, args(Ꮡi), default!, "bad verb"u8),
    new("%d %d %d"u8, "23 18"u8, args(Ꮡi, Ꮡj), args((nint)(23), (nint)(18)), "too few operands"u8),
    new("%d %d"u8, "23 18 27"u8, args(Ꮡi, Ꮡj, Ꮡk), args((nint)(23), (nint)(18)), "too many operands"u8),
    new("%c"u8, "\u0100"u8, args(Ꮡint8Val), default!, "overflow"u8),
    new("X%d"u8, "10X"u8, args(ᏑintVal), default!, "input does not match format"u8),
    new("%d%"u8, "42%"u8, args(ᏑintVal), args((nint)(42)), "missing verb: % at end of format string"u8),
    new("%d% "u8, "42%"u8, args(ᏑintVal), args((nint)(42)), "too few operands for format '% '"u8),
    new("%%%d"u8, "xxx 42"u8, args(ᏑintVal), args((nint)(42)), "missing literal %"u8),
    new("%%%d"u8, "x42"u8, args(ᏑintVal), args((nint)(42)), "missing literal %"u8),
    new("%%%d"u8, "42"u8, args(ᏑintVal), args((nint)(42)), "missing literal %"u8),
    new("%c%c%c"u8, ((@string)(new byte[]{0xc2, 0x58, 0xc2})), args(Ꮡr1, Ꮡr2, Ꮡr3), args((int32)(utf8.RuneError), (rune)'X', (int32)(utf8.RuneError)), ""u8),
    new("%v%v"u8, "FALSE23"u8, args(Ꮡtruth, Ꮡi), args(false, (nint)(23)), ""u8)
}.slice();


[GoType("dyn")] partial struct readersᴛ1 {
    internal @string name;
    internal Func<@string, Δio.Reader> f;
}

[GoType("dyn")] internal partial struct readers_type {
    [GoEmbedded] public io_package.Reader Reader;
}
internal static slice<readersᴛ1> readers = new readersᴛ1[]{
    new("StringReader"u8, (@string s) => new fmt_test_package.strings_ReaderжReader(strings.NewReader(s))),
    new("ReaderOnly"u8, (@string s) => new readers_type(new fmt_test_package.strings_ReaderжReader(strings.NewReader(s)))),
    new("OneByteReader"u8, (@string s) => iotest.OneByteReader(new fmt_test_package.strings_ReaderжReader(strings.NewReader(s)))),
    new("DataErrReader"u8, (@string s) => iotest.DataErrReader(new fmt_test_package.strings_ReaderжReader(strings.NewReader(s))))
}.slice();

internal static void testScan(ж<Δtesting.T> Ꮡt, Func<@string, Δio.Reader> f, Funcꓸꓸꓸ<Δio.Reader, any, (nint, error)> scan) {
    foreach (var (_, test) in scanTests) {
        var r = f(test.text);
        var (n, err) = scan(r, test.@in);
        if (err != default!) {
            @string m = ""u8;
            if (n > 0) {
                m = Sprintf(" (%d fields ok)"u8, n);
            }
            Ꮡt.Errorf("got error scanning %q: %s%s"u8, test.text, err, m);
            continue;
        }
        if (n != 1) {
            Ꮡt.Errorf("count error on entry %q: got %d"u8, test.text, n);
            continue;
        }
        // The incoming value may be a pointer
        var v = reflect.ValueOf(test.@in);
        {
            var p = v; if (p.Kind() == reflect.ΔPointer) {
                v = p.Elem();
            }
        }
        var val = v.Interface();
        if (!reflect.DeepEqual(val, test.@out)) {
            Ꮡt.Errorf("scanning %q: expected %#v got %#v, type %T"u8, test.text, test.@out, val, val);
        }
    }
}

public static void TestScan(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, vᴛ1) in readers) {
        ref var r = ref heap(new readersᴛ1(), out var Ꮡr);
        r = vᴛ1;

        var rʗ1 = r;
        Ꮡt.Run(r.name, (ж<Δtesting.T> tΔ1) => {
            testScan(tΔ1, rʗ1.f, Fscan);
        });
    }
}

public static void TestScanln(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, vᴛ1) in readers) {
        ref var r = ref heap(new readersᴛ1(), out var Ꮡr);
        r = vᴛ1;

        var rʗ1 = r;
        Ꮡt.Run(r.name, (ж<Δtesting.T> tΔ1) => {
            testScan(tΔ1, rʗ1.f, Fscanln);
        });
    }
}

public static void TestScanf(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, test) in scanfTests) {
        var (n, err) = Sscanf(test.text, test.format, test.@in);
        if (err != default!) {
            if (test.@out != default!) {
                Ꮡt.Errorf("Sscanf(%q, %q): unexpected error: %v"u8, test.text, test.format, err);
            }
            continue;
        }
        if (test.@out == default!) {
            Ꮡt.Errorf("Sscanf(%q, %q): unexpected success"u8, test.text, test.format);
            continue;
        }
        if (n != 1) {
            Ꮡt.Errorf("Sscanf(%q, %q): parsed %d field, want 1"u8, test.text, test.format, n);
            continue;
        }
        // The incoming value may be a pointer
        var v = reflect.ValueOf(test.@in);
        {
            var p = v; if (p.Kind() == reflect.ΔPointer) {
                v = p.Elem();
            }
        }
        var val = v.Interface();
        if (!reflect.DeepEqual(val, test.@out)) {
            Ꮡt.Errorf("Sscanf(%q, %q): parsed value %T(%#v), want %T(%#v)"u8, test.text, test.format, val, val, test.@out, test.@out);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string overflowTooLargeOutOfˢ = "overflow|too large|out of range|not representable"u8;

public static void TestScanOverflow(ж<Δtesting.T> Ꮡt) {
    // different machines and different types report errors with different strings.
    var re = Δregexp.MustCompile(overflowTooLargeOutOfˢ);
    foreach (var (_, test) in overflowTests) {
        var (_, err) = Sscan(test.text, test.@in);
        if (err == default!) {
            Ꮡt.Errorf("expected overflow scanning %q"u8, test.text);
            continue;
        }
        if (!re.MatchString(err.Error())) {
            Ꮡt.Errorf("expected overflow error scanning %q: %s"u8, test.text, err);
        }
    }
}

internal static void verifyNaN(@string str, ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var f = ref heap(new float64(), out var Ꮡf);
    ref var f32 = ref heap(new float32(), out var Ꮡf32);
    ref var f64 = ref heap(new float64(), out var Ꮡf64);
    @string text = str + " "u8 + str + " "u8 + str;
    var (n, err) = Fscan(new fmt_test_package.strings_ReaderжReader(strings.NewReader(text)), Ꮡf, Ꮡf32, Ꮡf64);
    if (err != default!) {
        Ꮡt.Errorf("got error scanning %q: %s"u8, text, err);
    }
    if (n != 3) {
        Ꮡt.Errorf("count error scanning %q: got %d"u8, text, n);
    }
    if (!Δmath.IsNaN((float64)f) || !Δmath.IsNaN((float64)f32) || !Δmath.IsNaN(f64)) {
        Ꮡt.Errorf("didn't get NaNs scanning %q: got %g %g %g"u8, text, f, f32, f64);
    }
}

public static void TestNaN(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, s) in new @string[]{"nan"u8, "NAN"u8, "NaN"u8}.slice()) {
        verifyNaN(s, Ꮡt);
    }
}

internal static void verifyInf(@string str, ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var f = ref heap(new float64(), out var Ꮡf);
    ref var f32 = ref heap(new float32(), out var Ꮡf32);
    ref var f64 = ref heap(new float64(), out var Ꮡf64);
    @string text = str + " "u8 + str + " "u8 + str;
    var (n, err) = Fscan(new fmt_test_package.strings_ReaderжReader(strings.NewReader(text)), Ꮡf, Ꮡf32, Ꮡf64);
    if (err != default!) {
        Ꮡt.Errorf("got error scanning %q: %s"u8, text, err);
    }
    if (n != 3) {
        Ꮡt.Errorf("count error scanning %q: got %d"u8, text, n);
    }
    nint sign = 1;
    if (str[0] == (rune)'-') {
        sign = -1;
    }
    if (!Δmath.IsInf((float64)f, sign) || !Δmath.IsInf((float64)f32, sign) || !Δmath.IsInf(f64, sign)) {
        Ꮡt.Errorf("didn't get right Infs scanning %q: got %g %g %g"u8, text, f, f32, f64);
    }
}

public static void TestInf(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, s) in new @string[]{"inf"u8, "+inf"u8, "-inf"u8, "INF"u8, "-INF"u8, "+INF"u8, "Inf"u8, "-Inf"u8, "+Inf"u8}.slice()) {
        verifyInf(s, Ꮡt);
    }
}

internal static void testScanfMulti(ж<Δtesting.T> Ꮡt, Func<@string, Δio.Reader> f) {
    var sliceType = reflect.TypeOf(new slice<any>(1));
    foreach (var (_, test) in multiTests) {
        var r = f(test.text);
        var (n, err) = Fscanf(r, test.format, test.@in.ꓸꓸꓸ);
        if (err != default!) {
            if (test.err == ""u8){
                Ꮡt.Errorf("got error scanning (%q, %q): %q"u8, test.format, test.text, err);
            } else 
            if (!strings.Contains(err.Error(), test.err)) {
                Ꮡt.Errorf("got wrong error scanning (%q, %q): %q; expected %q"u8, test.format, test.text, err, test.err);
            }
            continue;
        }
        if (test.err != ""u8) {
            Ꮡt.Errorf("expected error %q error scanning (%q, %q)"u8, test.err, test.format, test.text);
        }
        if (n != len(test.@out)) {
            Ꮡt.Errorf("count error on entry (%q, %q): expected %d got %d"u8, test.format, test.text, len(test.@out), n);
            continue;
        }
        // Convert the slice of pointers into a slice of values
        var resultVal = reflect.MakeSlice(sliceType, n, n);
        for (nint i = 0; i < n; i++) {
            var v = reflect.ValueOf(test.@in[i]).Elem();
            resultVal.Index(i).Set(v);
        }
        var result = resultVal.Interface();
        if (!reflect.DeepEqual(result, test.@out)) {
            Ꮡt.Errorf("scanning (%q, %q): expected %#v got %#v"u8, test.format, test.text, test.@out, result);
        }
    }
}

public static void TestScanfMulti(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, vᴛ1) in readers) {
        ref var r = ref heap(new readersᴛ1(), out var Ꮡr);
        r = vᴛ1;

        var rʗ1 = r;
        Ꮡt.Run(r.name, (ж<Δtesting.T> tΔ1) => {
            testScanfMulti(tΔ1, rʗ1.f);
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string asdfˢ = "asdf"u8;

public static void TestScanMultiple(ж<Δtesting.T> Ꮡt) {
    ref var a = ref heap(new nint(), out var Ꮡa);
    ref var s = ref heap(new @string(), out var Ꮡs);
    var (n, err) = Sscan("123abc"u8, Ꮡa, Ꮡs);
    if (n != 2) {
        Ꮡt.Errorf("Sscan count error: expected 2: got %d"u8, n);
    }
    if (err != default!) {
        Ꮡt.Errorf("Sscan expected no error; got %s"u8, err);
    }
    if (a != 123 || s != "abc"u8) {
        Ꮡt.Errorf("Sscan wrong values: got (%d %q) expected (123 \"abc\")"u8, a, s);
    }
    (n, err) = Sscan(asdfˢ, Ꮡs, Ꮡa);
    if (n != 1) {
        Ꮡt.Errorf("Sscan count error: expected 1: got %d"u8, n);
    }
    if (err == default!) {
        Ꮡt.Errorf("Sscan expected error; got none: %s"u8, err);
    }
    if (s != "asdf"u8) {
        Ꮡt.Errorf("Sscan wrong values: got %q expected \"asdf\""u8, s);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object sscanOneItemExpectedˢ = (@string)"Sscan <one item> expected error; got none"u8;
internal static readonly object sscanEmptyExpectedErrorˢ = (@string)"Sscan <empty> expected error; got none"u8;

// Empty strings are not valid input when scanning a string.
public static void TestScanEmpty(ж<Δtesting.T> Ꮡt) {
    ref var s1 = ref heap(new @string(), out var Ꮡs1);
    ref var s2 = ref heap(new @string(), out var Ꮡs2);
    var (n, err) = Sscan(abcˢ, Ꮡs1, Ꮡs2);
    if (n != 1) {
        Ꮡt.Errorf("Sscan count error: expected 1: got %d"u8, n);
    }
    if (err == default!) {
        Ꮡt.Error(sscanOneItemExpectedˢ);
    }
    if (s1 != "abc"u8) {
        Ꮡt.Errorf("Sscan wrong values: got %q expected \"abc\""u8, s1);
    }
    (n, err) = Sscan(""u8, Ꮡs1, Ꮡs2);
    if (n != 0) {
        Ꮡt.Errorf("Sscan count error: expected 0: got %d"u8, n);
    }
    if (err == default!) {
        Ꮡt.Error(sscanEmptyExpectedErrorˢ);
    }
    // Quoted empty string is OK.
    (n, err) = Sscanf(@""""""u8, "%q"u8, Ꮡs1);
    if (n != 1) {
        Ꮡt.Errorf("Sscanf count error: expected 1: got %d"u8, n);
    }
    if (err != default!) {
        Ꮡt.Errorf("Sscanf <empty> expected no error with quoted string; got %s"u8, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorScanningNonˢ = (@string)"expected error scanning non-pointer"u8;
internal static readonly @string pointerˢ = "pointer"u8;

public static void TestScanNotPointer(ж<Δtesting.T> Ꮡt) {
    var r = strings.NewReader("1"u8);
    nint a = default!;
    var (_, err) = Fscan(new fmt_test_package.strings_ReaderжReader(r), a);
    if (err == default!){
        Ꮡt.Error(expectedErrorScanningNonˢ);
    } else 
    if (!strings.Contains(err.Error(), pointerˢ)) {
        Ꮡt.Errorf("expected pointer error scanning non-pointer, got: %s"u8, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorScanningˢ = (@string)"expected error scanning string missing newline"u8;
internal static readonly @string newlineˢ = "newline"u8;

public static void TestScanlnNoNewline(ж<Δtesting.T> Ꮡt) {
    ref var a = ref heap(new nint(), out var Ꮡa);
    var (_, err) = Sscanln("1 x\n"u8, Ꮡa);
    if (err == default!){
        Ꮡt.Error(expectedErrorScanningˢ);
    } else 
    if (!strings.Contains(err.Error(), newlineˢ)) {
        Ꮡt.Errorf("expected newline error scanning string missing newline, got: %s"u8, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorScanningˢ2 = (@string)"expected error scanning string with extra newline"u8;

public static void TestScanlnWithMiddleNewline(ж<Δtesting.T> Ꮡt) {
    var r = strings.NewReader("123\n456\n"u8);
    ref var a = ref heap(new nint(), out var Ꮡa);
    ref var b = ref heap(new nint(), out var Ꮡb);
    var (_, err) = Fscanln(new fmt_test_package.strings_ReaderжReader(r), Ꮡa, Ꮡb);
    if (err == default!){
        Ꮡt.Error(expectedErrorScanningˢ2);
    } else 
    if (!strings.Contains(err.Error(), newlineˢ)) {
        Ꮡt.Errorf("expected newline error scanning string with extra newline, got: %s"u8, err);
    }
}

// eofCounter is a special Reader that counts reads at end of file.
[GoType] partial struct eofCounter {
    internal ж<strings.Reader> reader;
    internal nint eofCount;
}

[GoRecv] internal static (nint n, error err) Read(this ref eofCounter ec, slice<byte> b) {
    nint n = default!;
    error err = default!;

    (n, err) = ec.reader.Read(b);
    if (n == 0) {
        ec.eofCount++;
    }
    return (n, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unexpectedErrorˢ = (@string)"unexpected error"u8;
internal static readonly object expectedToScanOneItemGotˢ = (@string)"expected to scan one item, got"u8;
internal static readonly object expectedZeroEOFsˢ = (@string)"expected zero EOFs"u8;
internal static readonly object expectedErrorScanningˢ3 = (@string)"expected error scanning empty string"u8;
internal static readonly object expectedToScanZeroItemsˢ = (@string)"expected to scan zero items, got"u8;
internal static readonly object expectedOneEofGotˢ = (@string)"expected one EOF, got"u8;

// TestEOF verifies that when we scan, we see at most EOF once per call to a
// Scan function, and then only when it's really an EOF.
public static void TestEOF(ж<Δtesting.T> Ꮡt) {
    var ec = Ꮡ(new eofCounter(strings.NewReader("123\n"u8), 0));
    ref var a = ref heap(new nint(), out var Ꮡa);
    var (n, err) = Fscanln(new fmt_test_package.eofCounterжReader(ec), Ꮡa);
    if (err != default!) {
        Ꮡt.Error(unexpectedErrorˢ, err);
    }
    if (n != 1) {
        Ꮡt.Error(expectedToScanOneItemGotˢ, n);
    }
    if ((~ec).eofCount != 0) {
        Ꮡt.Error(expectedZeroEOFsˢ, (~ec).eofCount);
        ec.Value.eofCount = 0; // reset for next test
    }
    (n, err) = Fscanln(new fmt_test_package.eofCounterжReader(ec), Ꮡa);
    if (err == default!) {
        Ꮡt.Error(expectedErrorScanningˢ3);
    }
    if (n != 0) {
        Ꮡt.Error(expectedToScanZeroItemsˢ, n);
    }
    if ((~ec).eofCount != 1) {
        Ꮡt.Error(expectedOneEofGotˢ, (~ec).eofCount);
    }
}

// TestEOFAtEndOfInput verifies that we see an EOF error if we run out of input.
// This was a buglet: we used to get "expected integer".
public static void TestEOFAtEndOfInput(ж<Δtesting.T> Ꮡt) {
    ref var i = ref heap(new nint(), out var Ꮡi);
    ref var j = ref heap(new nint(), out var Ꮡj);
    var (n, err) = Sscanf("23"u8, "%d %d"u8, Ꮡi, Ꮡj);
    if (n != 1 || i != 23) {
        Ꮡt.Errorf("Sscanf expected one value of 23; got %d %d"u8, n, i);
    }
    if (!AreEqual(err, Δio.EOF)) {
        Ꮡt.Errorf("Sscanf expected EOF; got %q"u8, err);
    }
    (n, err) = Sscan("234"u8, Ꮡi, Ꮡj);
    if (n != 1 || i != 234) {
        Ꮡt.Errorf("Sscan expected one value of 234; got %d %d"u8, n, i);
    }
    if (!AreEqual(err, Δio.EOF)) {
        Ꮡt.Errorf("Sscan expected EOF; got %q"u8, err);
    }
    // Trailing space is tougher.
    (n, err) = Sscan("234 "u8, Ꮡi, Ꮡj);
    if (n != 1 || i != 234) {
        Ꮡt.Errorf("Sscan expected one value of 234; got %d %d"u8, n, i);
    }
    if (!AreEqual(err, Δio.EOF)) {
        Ꮡt.Errorf("Sscan expected EOF; got %q"u8, err);
    }
}


[GoType("dyn")] partial struct eofTestsᴛ1 {
    internal @string format;
    internal any v;
}
internal static slice<eofTestsᴛ1> eofTests = new eofTestsᴛ1[]{
    new("%s"u8, ᏑstringVal),
    new("%q"u8, ᏑstringVal),
    new("%x"u8, ᏑstringVal),
    new("%v"u8, ᏑstringVal),
    new("%v"u8, ᏑbytesVal),
    new("%v"u8, ᏑintVal),
    new("%v"u8, ᏑuintVal),
    new("%v"u8, ᏑboolVal),
    new("%v"u8, Ꮡfloat32Val),
    new("%v"u8, Ꮡcomplex64Val),
    new("%v"u8, ᏑrenamedStringVal),
    new("%v"u8, ᏑrenamedBytesVal),
    new("%v"u8, ᏑrenamedIntVal),
    new("%v"u8, ᏑrenamedUintVal),
    new("%v"u8, ᏑrenamedBoolVal),
    new("%v"u8, ᏑrenamedFloat32Val),
    new("%v"u8, ᏑrenamedComplex64Val)
}.slice();

public static void TestEOFAllTypes(ж<Δtesting.T> Ꮡt) {
    foreach (var (i, test) in eofTests) {
        {
            var (_, err) = Sscanf(""u8, test.format, test.v); if (!AreEqual(err, Δio.EOF)) {
                Ꮡt.Errorf("#%d: %s %T not eof on empty string: %s"u8, i, test.format, test.v, err);
            }
        }
        {
            var (_, err) = Sscanf("   "u8, test.format, test.v); if (!AreEqual(err, Δio.EOF)) {
                Ꮡt.Errorf("#%d: %s %T not eof on trailing blanks: %s"u8, i, test.format, test.v, err);
            }
        }
    }
}

// TestUnreadRuneWithBufio verifies that, at least when using bufio, successive
// calls to Fscan do not lose runes.
public static void TestUnreadRuneWithBufio(ж<Δtesting.T> Ꮡt) {
    var r = bufio.NewReader(new fmt_test_package.strings_ReaderжReader(strings.NewReader("123αb"u8)));
    ref var i = ref heap(new nint(), out var Ꮡi);
    ref var a = ref heap(new @string(), out var Ꮡa);
    var (n, err) = Fscanf(new fmt_test_package.bufio_ReaderжReader(r), "%d"u8, Ꮡi);
    if (n != 1 || err != default!) {
        Ꮡt.Errorf("reading int expected one item, no errors; got %d %q"u8, n, err);
    }
    if (i != 123) {
        Ꮡt.Errorf("expected 123; got %d"u8, i);
    }
    (n, err) = Fscanf(new fmt_test_package.bufio_ReaderжReader(r), "%s"u8, Ꮡa);
    if (n != 1 || err != default!) {
        Ꮡt.Errorf("reading string expected one item, no errors; got %d %q"u8, n, err);
    }
    if (a != "αb"u8) {
        Ꮡt.Errorf("expected αb; got %q"u8, a);
    }
}

[GoType("@string")] partial struct TwoLines;

// Scan attempts to read two lines into the object. Scanln should prevent this
// because it stops at newline; Scan and Scanf should be fine.
[GoRecv] public static error Scan(this ref TwoLines t, fmt.ScanState state, rune verb) {
    var chars = new slice<rune>(0, 100);
    for (nint nlCount = 0; nlCount < 2; ) {
        var (c, _, err) = state.ReadRune();
        if (err != default!) {
            return err;
        }
        chars = append(chars, c);
        if (c == (rune)'\n') {
            nlCount++;
        }
    }
    t = ((TwoLines)((@string)chars));
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string abcDefˢ = "abc\ndef\n"u8;
internal static readonly object sscanlnExpectedErrorGotˢ = (@string)"Sscanln: expected error; got none"u8;

public static void TestMultiLine(ж<Δtesting.T> Ꮡt) {
    @string input = abcDefˢ;
    // Sscan should work
    ref var tscan = ref heap(new TwoLines(), out var Ꮡtscan);
    var (n, err) = Sscan(input, Ꮡtscan);
    if (n != 1) {
        Ꮡt.Errorf("Sscan: expected 1 item; got %d"u8, n);
    }
    if (err != default!) {
        Ꮡt.Errorf("Sscan: expected no error; got %s"u8, err);
    }
    if (((@string)tscan) != input) {
        Ꮡt.Errorf("Sscan: expected %q; got %q"u8, input, tscan);
    }
    // Sscanf should work
    ref var tscanf = ref heap(new TwoLines(), out var Ꮡtscanf);
    (n, err) = Sscanf(input, "%s"u8, Ꮡtscanf);
    if (n != 1) {
        Ꮡt.Errorf("Sscanf: expected 1 item; got %d"u8, n);
    }
    if (err != default!) {
        Ꮡt.Errorf("Sscanf: expected no error; got %s"u8, err);
    }
    if (((@string)tscanf) != input) {
        Ꮡt.Errorf("Sscanf: expected %q; got %q"u8, input, tscanf);
    }
    // Sscanln should not work
    ref var tscanln = ref heap(new TwoLines(), out var Ꮡtscanln);
    (n, err) = Sscanln(input, Ꮡtscanln);
    if (n != 0) {
        Ꮡt.Errorf("Sscanln: expected 0 items; got %d: %q"u8, n, tscanln);
    }
    if (err == default!){
        Ꮡt.Error(sscanlnExpectedErrorGotˢ);
    } else 
    if (!AreEqual(err, Δio.ErrUnexpectedEOF)) {
        Ꮡt.Errorf("Sscanln: expected io.ErrUnexpectedEOF (ha!); got %s"u8, err);
    }
}

[GoType("dyn")] internal partial struct TestLineByLineFscanf_r {
    [GoEmbedded] public io_package.Reader Reader;
}

// TestLineByLineFscanf tests that Fscanf does not read past newline. Issue
// 3481.
public static void TestLineByLineFscanf(ж<Δtesting.T> Ꮡt) {
    var r = new TestLineByLineFscanf_r(new fmt_test_package.strings_ReaderжReader(strings.NewReader("1\n2\n"u8)));
    ref var i = ref heap(new nint(), out var Ꮡi);
    ref var j = ref heap(new nint(), out var Ꮡj);
    var (n, err) = Fscanf(r, "%v\n"u8, Ꮡi);
    if (n != 1 || err != default!) {
        Ꮡt.Fatalf("first read: %d %q"u8, n, err);
    }
    (n, err) = Fscanf(r, "%v\n"u8, Ꮡj);
    if (n != 1 || err != default!) {
        Ꮡt.Fatalf("second read: %d %q"u8, n, err);
    }
    if (i != 1 || j != 2) {
        Ꮡt.Errorf("wrong values; wanted 1 2 got %d %d"u8, i, j);
    }
}

// TestScanStateCount verifies the correct byte count is returned. Issue 8512.

// runeScanner implements the Scanner interface for TestScanStateCount.
[GoType] partial struct runeScanner {
    internal rune rune;
    internal nint size;
}

[GoRecv] internal static error Scan(this ref runeScanner rs, fmt.ScanState state, rune verb) {
    var (r, size, err) = state.ReadRune();
    rs.rune = r;
    rs.size = size;
    return err;
}

public static void TestScanStateCount(ж<Δtesting.T> Ꮡt) {
    ref var a = ref heap(new runeScanner(), out var Ꮡa);
    ref var b = ref heap(new runeScanner(), out var Ꮡb);
    ref var c = ref heap(new runeScanner(), out var Ꮡc);
    var (n, err) = Sscanf("12➂"u8, "%c%c%c"u8, Ꮡa, Ꮡb, Ꮡc);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (n != 3) {
        Ꮡt.Fatalf("expected 3 items consumed, got %d"u8, n);
    }
    if (a.rune != (rune)'1' || b.rune != (rune)'2' || c.rune != (rune)'➂') {
        Ꮡt.Errorf("bad scan rune: %q %q %q should be '1' '2' '➂'"u8, a.rune, b.rune, c.rune);
    }
    if (a.size != 1 || b.size != 1 || c.size != 3) {
        Ꮡt.Errorf("bad scan size: %q %q %q should be 1 1 3"u8, a.size, b.size, c.size);
    }
}

// RecursiveInt accepts a string matching %d.%d.%d....
// and parses it into a linked list.
// It allows us to benchmark recursive descent style scanners.
[GoType] partial struct RecursiveInt {
    internal nint i;
    internal ж<RecursiveInt> next;
}

public static error /*err*/ Scan(this ж<RecursiveInt> Ꮡr, fmt.ScanState state, rune verb) {
    error err = default!;

    ref var r = ref Ꮡr.DerefOrNull();
    (_, err) = Fscan(new fmt_test_package.fmt_ScanStateᴠReader(state), Ꮡr.of(RecursiveInt.Ꮡi));
    if (err != default!) {
        return err;
    }
    var next = @new<RecursiveInt>();
    (_, err) = Fscanf(new fmt_test_package.fmt_ScanStateᴠReader(state), ".%v"u8, next.OrTypedNil());
    if (err != default!) {
        if (AreEqual(err, Δio.ErrUnexpectedEOF)) {
            err = default!;
        }
        return err;
    }
    r.next = next;
    return err;
}

// scanInts performs the same scanning task as RecursiveInt.Scan
// but without recurring through scanner, so we can compare
// performance more directly.
internal static error /*err*/ scanInts(ж<RecursiveInt> Ꮡr, ж<bytes.Buffer> Ꮡb) {
    error err = default!;

    ref var r = ref Ꮡr.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();
    r.next = default!;
    (_, err) = Fscan(new fmt_test_package.bytes_BufferжReader(Ꮡb), Ꮡr.of(RecursiveInt.Ꮡi));
    if (err != default!) {
        return err;
    }
    (var c, _, err) = b.ReadRune();
    if (err != default!) {
        if (AreEqual(err, Δio.EOF)) {
            err = default!;
        }
        return err;
    }
    if (c != (rune)'.') {
        return err;
    }
    var next = @new<RecursiveInt>();
    err = scanInts(next, Ꮡb);
    if (err == default!) {
        r.next = next;
    }
    return err;
}

internal static slice<byte> makeInts(nint n) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    Fprintf(new fmt_test_package.bytes_BufferжWriter(Ꮡbuf), "1"u8);
    for (nint i = 1; i < n; i++) {
        Fprintf(new fmt_test_package.bytes_BufferжWriter(Ꮡbuf), ".%d"u8, i + 1);
    }
    return buf.Bytes();
}

public static void TestScanInts(ж<Δtesting.T> Ꮡt) {
    testScanInts(Ꮡt, scanInts);
    testScanInts(Ꮡt, (ж<RecursiveInt> r, ж<bytes.Buffer> b) => {
        error err = default!;
        (_, err) = Fscan(new fmt_test_package.bytes_BufferжReader(b), r.OrTypedNil());
        return err;
    });
}

// 800 is small enough to not overflow the stack when using gccgo on a
// platform that does not support split stack.
internal static UntypedInt intCount => 800;

internal static void testScanInts(ж<Δtesting.T> Ꮡt, Func<ж<RecursiveInt>, ж<bytes.Buffer>, error> scan) {
    var r = @new<RecursiveInt>();
    var ints = makeInts(intCount);
    var buf = bytes.NewBuffer(ints);
    var err = scan(r, buf);
    if (err != default!) {
        Ꮡt.Error(unexpectedErrorˢ, err);
    }
    nint i = 1;
    for (; r != nil; r = r.Value.next) {
        if ((~r).i != i) {
            Ꮡt.Fatalf("bad scan: expected %d got %d"u8, i, (~r).i);
        }
        i++;
    }
    if (i - 1 != intCount) {
        Ꮡt.Fatalf("bad scan count: expected %d got %d"u8, (nint)(intCount), i - 1);
    }
}

public static void BenchmarkScanInts(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    var ints = makeInts(intCount);
    ref var r = ref heap(new RecursiveInt(), out var Ꮡr);
    for (nint i = 0; i < b.N; i++) {
        var buf = bytes.NewBuffer(ints);
        b.StartTimer();
        scanInts(Ꮡr, buf);
        b.StopTimer();
    }
}

public static void BenchmarkScanRecursiveInt(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    var ints = makeInts(intCount);
    ref var r = ref heap(new RecursiveInt(), out var Ꮡr);
    for (nint i = 0; i < b.N; i++) {
        var buf = bytes.NewBuffer(ints);
        b.StartTimer();
        Fscan(new fmt_test_package.bytes_BufferжReader(buf), Ꮡr);
        b.StopTimer();
    }
}

[GoType("dyn")] internal partial struct BenchmarkScanRecursiveIntReaderWrapper_buf {
    [GoEmbedded] public io_package.Reader Reader;
}

public static void BenchmarkScanRecursiveIntReaderWrapper(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    var ints = makeInts(intCount);
    ref var r = ref heap(new RecursiveInt(), out var Ꮡr);
    for (nint i = 0; i < b.N; i++) {
        var buf = new BenchmarkScanRecursiveIntReaderWrapper_buf(new fmt_test_package.strings_ReaderжReader(strings.NewReader(((@string)ints))));
        b.StartTimer();
        Fscan(buf, Ꮡr);
        b.StopTimer();
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string simpleˢ = "simple"u8;
internal static readonly @string simplePairAˢ = "simple pair a"u8;
internal static readonly @string simplePairBˢ = "simple pair b"u8;
internal static readonly @string colonˢ = "colon"u8;
internal static readonly @string colonPairAˢ = "colon pair a"u8;
internal static readonly @string colonPairBˢ = "colon pair b"u8;

// Issue 9124.
// %x on bytes couldn't handle non-space bytes terminating the scan.
public static void TestHexBytes(ж<Δtesting.T> Ꮡt) {
    ref var a = ref heap<slice<byte>>(out var Ꮡa);
    ref var b = ref heap<slice<byte>>(out var Ꮡb);
    var (n, err) = Sscanf("00010203"u8, "%x"u8, Ꮡa);
    if (n != 1 || err != default!) {
        Ꮡt.Errorf("simple: got count, err = %d, %v; expected 1, nil"u8, n, err);
    }
    void check(@string msg, slice<byte> x) {
        if (len(x) != 4) {
            Ꮡt.Errorf("%s: bad length %d"u8, msg, len(x));
        }
        foreach (var (i, bΔ1) in x) {
            if ((nint)bΔ1 != i) {
                Ꮡt.Errorf("%s: bad x[%d] = %x"u8, msg, i, x[i]);
            }
        }
    }
    check(simpleˢ, a);
    a = default!;
    (n, err) = Sscanf("00010203 00010203"u8, "%x %x"u8, Ꮡa, Ꮡb);
    if (n != 2 || err != default!) {
        Ꮡt.Errorf("simple pair: got count, err = %d, %v; expected 2, nil"u8, n, err);
    }
    check(simplePairAˢ, a);
    check(simplePairBˢ, b);
    a = default!;
    b = default!;
    (n, err) = Sscanf("00010203:"u8, "%x"u8, Ꮡa);
    if (n != 1 || err != default!) {
        Ꮡt.Errorf("colon: got count, err = %d, %v; expected 1, nil"u8, n, err);
    }
    check(colonˢ, a);
    a = default!;
    (n, err) = Sscanf("00010203:00010203"u8, "%x:%x"u8, Ꮡa, Ꮡb);
    if (n != 2 || err != default!) {
        Ꮡt.Errorf("colon pair: got count, err = %d, %v; expected 2, nil"u8, n, err);
    }
    check(colonPairAˢ, a);
    check(colonPairBˢ, b);
    a = default!;
    b = default!;
    // This one fails because there is a hex byte after the data,
    // that is, an odd number of hex input bytes.
    (n, err) = Sscanf("000102034:"u8, "%x"u8, Ꮡa);
    if (n != 0 || err == default!) {
        Ꮡt.Errorf("odd count: got count, err = %d, %v; expected 0, error"u8, n, err);
    }
}

[GoType("dyn")] internal partial struct TestScanNewlinesAreSpaces_type {
    internal @string name;
    internal @string text;
    internal nint count;
}

public static void TestScanNewlinesAreSpaces(ж<Δtesting.T> Ꮡt) {
    ref var a = ref heap(new nint(), out var Ꮡa);
    ref var b = ref heap(new nint(), out var Ꮡb);
    slice<TestScanNewlinesAreSpaces_type> tests = new TestScanNewlinesAreSpaces_type[]{
        new("newlines"u8, "1\n2\n"u8, 2),
        new("no final newline"u8, "1\n2"u8, 2),
        new("newlines with spaces "u8, "1  \n  2  \n"u8, 2),
        new("no final newline with spaces"u8, "1  \n  2"u8, 2)
    }.slice();
    foreach (var (_, test) in tests) {
        var (n, err) = Sscan(test.text, Ꮡa, Ꮡb);
        if (n != test.count) {
            Ꮡt.Errorf("%s: expected to scan %d item(s), scanned %d"u8, test.name, test.count, n);
        }
        if (err != default!) {
            Ꮡt.Errorf("%s: unexpected error: %s"u8, test.name, err);
        }
    }
}

[GoType("dyn")] internal partial struct TestScanlnNewlinesTerminate_type {
    internal @string name;
    internal @string text;
    internal nint count;
    internal bool ok;
}

public static void TestScanlnNewlinesTerminate(ж<Δtesting.T> Ꮡt) {
    ref var a = ref heap(new nint(), out var Ꮡa);
    ref var b = ref heap(new nint(), out var Ꮡb);
    slice<TestScanlnNewlinesTerminate_type> tests = new TestScanlnNewlinesTerminate_type[]{
        new("one line one item"u8, "1\n"u8, 1, false),
        new("one line two items with spaces "u8, "   1 2    \n"u8, 2, true),
        new("one line two items no newline"u8, "   1 2"u8, 2, true),
        new("two lines two items"u8, "1\n2\n"u8, 1, false)
    }.slice();
    foreach (var (_, test) in tests) {
        var (n, err) = Sscanln(test.text, Ꮡa, Ꮡb);
        if (n != test.count) {
            Ꮡt.Errorf("%s: expected to scan %d item(s), scanned %d"u8, test.name, test.count, n);
        }
        if (test.ok && err != default!) {
            Ꮡt.Errorf("%s: unexpected error: %s"u8, test.name, err);
        }
        if (!test.ok && err == default!) {
            Ꮡt.Errorf("%s: expected error; got none"u8, test.name);
        }
    }
}

[GoType("dyn")] internal partial struct TestScanfNewlineMatchFormat_type {
    internal @string name;
    internal @string text;
    internal @string format;
    internal nint count;
    internal bool ok;
}

public static void TestScanfNewlineMatchFormat(ж<Δtesting.T> Ꮡt) {
    ref var a = ref heap(new nint(), out var Ꮡa);
    ref var b = ref heap(new nint(), out var Ꮡb);
// fails: space after nl in input but not pattern
// fails: space after nl in input but not pattern
// fails: space after nl in input but not pattern
// fails: space after nl in input but not pattern
// fails: space after nl in input but not pattern
    slice<TestScanfNewlineMatchFormat_type> tests = new TestScanfNewlineMatchFormat_type[]{
        new("newline in both"u8, "1\n2"u8, "%d\n%d\n"u8, 2, true),
        new("newline in input"u8, "1\n2"u8, "%d %d"u8, 1, false),
        new("space-newline in input"u8, "1 \n2"u8, "%d %d"u8, 1, false),
        new("newline in format"u8, "1 2"u8, "%d\n%d"u8, 1, false),
        new("space-newline in format"u8, "1 2"u8, "%d \n%d"u8, 1, false),
        new("space-newline in both"u8, "1 \n2"u8, "%d \n%d"u8, 2, true),
        new("extra space in format"u8, "1\n2"u8, "%d\n %d"u8, 2, true),
        new("two extra spaces in format"u8, "1\n2"u8, "%d \n %d"u8, 2, true),
        new("space vs newline 0000"u8, "1\n2"u8, "%d\n%d"u8, 2, true),
        new("space vs newline 0001"u8, "1\n2"u8, "%d\n %d"u8, 2, true),
        new("space vs newline 0010"u8, "1\n2"u8, "%d \n%d"u8, 2, true),
        new("space vs newline 0011"u8, "1\n2"u8, "%d \n %d"u8, 2, true),
        new("space vs newline 0100"u8, "1\n 2"u8, "%d\n%d"u8, 2, true),
        new("space vs newline 0101"u8, "1\n 2"u8, "%d\n%d "u8, 2, true),
        new("space vs newline 0110"u8, "1\n 2"u8, "%d \n%d"u8, 2, true),
        new("space vs newline 0111"u8, "1\n 2"u8, "%d \n %d"u8, 2, true),
        new("space vs newline 1000"u8, "1 \n2"u8, "%d\n%d"u8, 2, true),
        new("space vs newline 1001"u8, "1 \n2"u8, "%d\n %d"u8, 2, true),
        new("space vs newline 1010"u8, "1 \n2"u8, "%d \n%d"u8, 2, true),
        new("space vs newline 1011"u8, "1 \n2"u8, "%d \n %d"u8, 2, true),
        new("space vs newline 1100"u8, "1 \n 2"u8, "%d\n%d"u8, 2, true),
        new("space vs newline 1101"u8, "1 \n 2"u8, "%d\n %d"u8, 2, true),
        new("space vs newline 1110"u8, "1 \n 2"u8, "%d \n%d"u8, 2, true),
        new("space vs newline 1111"u8, "1 \n 2"u8, "%d \n %d"u8, 2, true),
        new("space vs newline no-percent 0000"u8, "1\n2"u8, "1\n2"u8, 0, true),
        new("space vs newline no-percent 0001"u8, "1\n2"u8, "1\n 2"u8, 0, true),
        new("space vs newline no-percent 0010"u8, "1\n2"u8, "1 \n2"u8, 0, true),
        new("space vs newline no-percent 0011"u8, "1\n2"u8, "1 \n 2"u8, 0, true),
        new("space vs newline no-percent 0100"u8, "1\n 2"u8, "1\n2"u8, 0, false),
        new("space vs newline no-percent 0101"u8, "1\n 2"u8, "1\n2 "u8, 0, false),
        new("space vs newline no-percent 0110"u8, "1\n 2"u8, "1 \n2"u8, 0, false),
        new("space vs newline no-percent 0111"u8, "1\n 2"u8, "1 \n 2"u8, 0, true),
        new("space vs newline no-percent 1000"u8, "1 \n2"u8, "1\n2"u8, 0, true),
        new("space vs newline no-percent 1001"u8, "1 \n2"u8, "1\n 2"u8, 0, true),
        new("space vs newline no-percent 1010"u8, "1 \n2"u8, "1 \n2"u8, 0, true),
        new("space vs newline no-percent 1011"u8, "1 \n2"u8, "1 \n 2"u8, 0, true),
        new("space vs newline no-percent 1100"u8, "1 \n 2"u8, "1\n2"u8, 0, false),
        new("space vs newline no-percent 1101"u8, "1 \n 2"u8, "1\n 2"u8, 0, true),
        new("space vs newline no-percent 1110"u8, "1 \n 2"u8, "1 \n2"u8, 0, false),
        new("space vs newline no-percent 1111"u8, "1 \n 2"u8, "1 \n 2"u8, 0, true)
    }.slice();
    foreach (var (_, test) in tests) {
        nint n = default!;
        error err = default!;
        if (strings.Contains(test.format, "%"u8)){
            (n, err) = Sscanf(test.text, test.format, Ꮡa, Ꮡb);
        } else {
            (n, err) = Sscanf(test.text, test.format);
        }
        if (n != test.count) {
            Ꮡt.Errorf("%s: expected to scan %d item(s), scanned %d"u8, test.name, test.count, n);
        }
        if (test.ok && err != default!) {
            Ꮡt.Errorf("%s: unexpected error: %s"u8, test.name, err);
        }
        if (!test.ok && err == default!) {
            Ꮡt.Errorf("%s: expected error; got none"u8, test.name);
        }
    }
}

[GoType("[2]byte")] partial struct hexBytes;

// Test for issue 12090: Was unreading at EOF, double-scanning a byte.
[GoRecv] internal static error Scan(this ref hexBytes h, fmt.ScanState ss, rune verb) {
    ref var b = ref heap<slice<byte>>(out var Ꮡb);
    var (_, err) = Fscanf(new fmt_test_package.fmt_ScanStateᴠReader(ss), "%4x"u8, Ꮡb);
    if (err != default!) {
        throw panic(err); // Really shouldn't happen.
    }
    copy((h)[..], b);
    return err;
}

public static void TestHexByte(ж<Δtesting.T> Ꮡt) {
    ref var h = ref heap(new hexBytes(), out var Ꮡh);
    var (n, err) = Sscanln("0123\n"u8, Ꮡh);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (n != 1) {
        Ꮡt.Fatalf("expected 1 item; scanned %d"u8, n);
    }
    if (h[0] != 0x01 || h[1] != 0x23) {
        Ꮡt.Fatalf("expected 0123 got %x"u8, h);
    }
}

} // end fmt_test_package
