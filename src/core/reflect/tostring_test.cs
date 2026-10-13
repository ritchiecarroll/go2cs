// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Formatting of reflection types and values for debugging.
// Not defined as methods so they do not need to be linked into most binaries;
// the functions are not used by the library itself, only in tests.
namespace go;

using static reflect_package;
using strconv = strconv_package;
using static global::go.reflect_internal_test_package;
using Δreflect = reflect_package;

partial class reflect_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string zeroValueˢ = "<zero Value>"u8;
internal static readonly @string trueˢ = "true"u8;
internal static readonly @string falseˢ = "false"u8;

// valueToString returns a textual representation of the reflection value val.
// For debugging only.
internal static @string valueToString(reflectꓸValue val) {
    @string str = default!;
    if (!val.IsValid()) {
        return zeroValueˢ;
    }
    var typ = val.Type();
    var exprᴛ1 = val.Kind();
    if (exprᴛ1 == ΔInt || exprᴛ1 == Int8 || exprᴛ1 == Int16 || exprᴛ1 == Int32 || exprᴛ1 == Int64) {
        return strconv.FormatInt(val.Int(), 10);
    }
    if (exprᴛ1 == ΔUint || exprᴛ1 == Uint8 || exprᴛ1 == Uint16 || exprᴛ1 == Uint32 || exprᴛ1 == Uint64 || exprᴛ1 == Uintptr) {
        return strconv.FormatUint(val.Uint(), 10);
    }
    if (exprᴛ1 == Float32 || exprᴛ1 == Float64) {
        return strconv.FormatFloat(val.Float(), (rune)'g', -1, 64);
    }
    if (exprᴛ1 == Complex64 || exprᴛ1 == Complex128) {
        var c = val.Complex();
        return strconv.FormatFloat(real(c), (rune)'g', -1, 64) + "+"u8 + strconv.FormatFloat(imag(c), (rune)'g', -1, 64) + "i"u8;
    }
    if (exprᴛ1 == ΔString) {
        return val.String();
    }
    if (exprᴛ1 == ΔBool) {
        if (val.Bool()){
            return trueˢ;
        } else {
            return falseˢ;
        }
    }
    if (exprᴛ1 == ΔPointer) {
        var v = val;
        str = typ.String() + "("u8;
        if (v.IsNil()){
            str += "0"u8;
        } else {
            str += "&"u8 + valueToString(v.Elem());
        }
        str += ")"u8;
        return str;
    }
    if (exprᴛ1 == Array || exprᴛ1 == ΔSlice) {
        var v = val;
        str += typ.String();
        str += "{"u8;
        for (nint i = 0; i < v.Len(); i++) {
            if (i > 0) {
                str += ", "u8;
            }
            str += valueToString(v.Index(i));
        }
        str += "}"u8;
        return str;
    }
    if (exprᴛ1 == Map) {
        var t = typ;
        str = t.String();
        str += "{"u8;
        str += "<can't iterate on maps>"u8;
        str += "}"u8;
        return str;
    }
    if (exprᴛ1 == Chan) {
        str = typ.String();
        return str;
    }
    if (exprᴛ1 == Struct) {
        var t = typ;
        var v = val;
        str += t.String();
        str += "{"u8;
        for ((nint i, nint n) = (0, v.NumField()); i < n; i++) {
            if (i > 0) {
                str += ", "u8;
            }
            str += valueToString(v.Field(i));
        }
        str += "}"u8;
        return str;
    }
    if (exprᴛ1 == ΔInterface) {
        return typ.String() + "("u8 + valueToString(val.Elem()) + ")"u8;
    }
    if (exprᴛ1 == Func) {
        var v = val;
        return typ.String() + "("u8 + strconv.FormatUint((uint64)v.Pointer(), 10) + ")"u8;
    }
    { /* default: */
        throw panic("valueToString: can't print type " + typ.String());
    }
}

} // end reflect_test_package
