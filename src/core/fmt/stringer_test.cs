// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static fmt_package;
using Δtesting = testing_package;
using static go.fmt_internal_test_package;

partial class fmt_test_package {

partial struct TI /*num:nint*/;

partial struct TI8 /*num:int8*/;

partial struct TI16 /*num:int16*/;

partial struct TI32 /*num:int32*/;

partial struct TI64 /*num:int64*/;

partial struct TU /*num:nuint*/;

partial struct TU8 /*num:uint8*/;

partial struct TU16 /*num:uint16*/;

partial struct TU32 /*num:uint32*/;

partial struct TU64 /*num:uint64*/;

partial struct TUI /*num:uintptr*/;

partial struct TF /*num:float64*/;

partial struct TF32 /*num:float32*/;

partial struct TF64 /*num:float64*/;

partial struct TB /*bool*/;

partial struct TS /*@string*/;

public static @string String(this TI v) {
    return Sprintf("I: %d"u8, (nint)v);
}

public static @string String(this TI8 v) {
    return Sprintf("I8: %d"u8, (int8)v);
}

public static @string String(this TI16 v) {
    return Sprintf("I16: %d"u8, (int16)v);
}

public static @string String(this TI32 v) {
    return Sprintf("I32: %d"u8, (int32)v);
}

public static @string String(this TI64 v) {
    return Sprintf("I64: %d"u8, (int64)v);
}

public static @string String(this TU v) {
    return Sprintf("U: %d"u8, (nuint)v);
}

public static @string String(this TU8 v) {
    return Sprintf("U8: %d"u8, (uint8)v);
}

public static @string String(this TU16 v) {
    return Sprintf("U16: %d"u8, (uint16)v);
}

public static @string String(this TU32 v) {
    return Sprintf("U32: %d"u8, (uint32)v);
}

public static @string String(this TU64 v) {
    return Sprintf("U64: %d"u8, (uint64)v);
}

public static @string String(this TUI v) {
    return Sprintf("UI: %d"u8, (uintptr)v);
}

public static @string String(this TF v) {
    return Sprintf("F: %f"u8, (float64)v);
}

public static @string String(this TF32 v) {
    return Sprintf("F32: %f"u8, (float32)v);
}

public static @string String(this TF64 v) {
    return Sprintf("F64: %f"u8, (float64)v);
}

public static @string String(this TB v) {
    return Sprintf("B: %t"u8, (bool)v);
}

public static @string String(this TS v) {
    return Sprintf("S: %q"u8, ((@string)v));
}

internal static void check(ж<Δtesting.T> Ꮡt, @string got, @string want) {
    if (got != want) {
        Ꮡt.Error(got, (@string)"!="u8, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string i0I81I162I323I644ˢ = "I: 0 I8: 1 I16: 2 I32: 3 I64: 4"u8;
internal static readonly @string u5U86U167U328U649Ui10ˢ = "U: 5 U8: 6 U16: 7 U32: 8 U64: 9 UI: 10"u8;
internal static readonly @string f1000000F322000000F643ˢ = "F: 1.000000 F32: 2.000000 F64: 3.000000"u8;
internal static readonly @string bTrueSXˢ = "B: true S: \"x\""u8;

public static void TestStringer(ж<Δtesting.T> Ꮡt) {
    @string s = Sprintf("%v %v %v %v %v"u8, ((TI)0), ((TI8)1), ((TI16)2), ((TI32)3), ((TI64)4));
    check(Ꮡt, s, i0I81I162I323I644ˢ);
    s = Sprintf("%v %v %v %v %v %v"u8, ((TU)5), ((TU8)6), ((TU16)7), ((TU32)8), ((TU64)9), ((TUI)10));
    check(Ꮡt, s, u5U86U167U328U649Ui10ˢ);
    s = Sprintf("%v %v %v"u8, ((TF)1.0D), ((TF32)2.0F), ((TF64)3.0D));
    check(Ꮡt, s, f1000000F322000000F643ˢ);
    s = Sprintf("%v %v"u8, ((TB)true), ((TS)(@string)"x"u8));
    check(Ꮡt, s, bTrueSXˢ);
}

} // end fmt_test_package
