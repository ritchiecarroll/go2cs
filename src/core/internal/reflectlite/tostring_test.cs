// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Formatting of reflection types and values for debugging.
// Not defined as methods so they do not need to be linked into most binaries;
// the functions are not used by the library itself, only in tests.
namespace go.@internal;

using static global::go.@internal.reflectlite_package;
using reflect = reflect_package;
using strconv = global::go.strconv_package;
using global::go;
using reflectlite = global::go.@internal.reflectlite_package;
using static global::go.@internal.reflectlite_internal_test_package;

partial class reflectlite_test_package {

// valueToString returns a textual representation of the reflection value val.
// For debugging only.
internal static @string valueToString(reflectlite.Value v) {
    return valueToStringImpl(reflect.ValueOf(reflectlite_internal_test_package.ToInterface(v)));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string zeroValueˢ = "<zero Value>"u8;
internal static readonly @string trueˢ = "true"u8;
internal static readonly @string falseˢ = "false"u8;

internal static @string valueToStringImpl(reflectꓸValue val) {
    @string str = default!;
    if (!val.IsValid()) {
        return zeroValueˢ;
    }
    var typ = val.Type();
    var exprᴛ1 = val.Kind();
    if (exprᴛ1 == reflect.ΔInt || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64) {
        return strconv.FormatInt(val.Int(), 10);
    }
    if (exprᴛ1 == reflect.ΔUint || exprᴛ1 == reflect.Uint8 || exprᴛ1 == reflect.Uint16 || exprᴛ1 == reflect.Uint32 || exprᴛ1 == reflect.Uint64 || exprᴛ1 == reflect.Uintptr) {
        return strconv.FormatUint(val.Uint(), 10);
    }
    if (exprᴛ1 == reflect.Float32 || exprᴛ1 == reflect.Float64) {
        return strconv.FormatFloat(val.Float(), (rune)'g', -1, 64);
    }
    if (exprᴛ1 == reflect.Complex64 || exprᴛ1 == reflect.Complex128) {
        var c = val.Complex();
        return strconv.FormatFloat(real(c), (rune)'g', -1, 64) + "+"u8 + strconv.FormatFloat(imag(c), (rune)'g', -1, 64) + "i"u8;
    }
    if (exprᴛ1 == reflect.ΔString) {
        return val.String();
    }
    if (exprᴛ1 == reflect.ΔBool) {
        if (val.Bool()){
            return trueˢ;
        } else {
            return falseˢ;
        }
    }
    if (exprᴛ1 == reflect.ΔPointer) {
        var v = val;
        str = typ.String() + "("u8;
        if (v.IsNil()){
            str += "0"u8;
        } else {
            str += "&"u8 + valueToStringImpl(v.Elem());
        }
        str += ")"u8;
        return str;
    }
    if (exprᴛ1 == reflect.Array || exprᴛ1 == reflect.ΔSlice) {
        var v = val;
        str += typ.String();
        str += "{"u8;
        for (nint i = 0; i < v.Len(); i++) {
            if (i > 0) {
                str += ", "u8;
            }
            str += valueToStringImpl(v.Index(i));
        }
        str += "}"u8;
        return str;
    }
    if (exprᴛ1 == reflect.Map) {
        str += typ.String();
        str += "{"u8;
        str += "<can't iterate on maps>"u8;
        str += "}"u8;
        return str;
    }
    if (exprᴛ1 == reflect.Chan) {
        str = typ.String();
        return str;
    }
    if (exprᴛ1 == reflect.Struct) {
        var t = typ;
        var v = val;
        str += t.String();
        str += "{"u8;
        for ((nint i, nint n) = (0, v.NumField()); i < n; i++) {
            if (i > 0) {
                str += ", "u8;
            }
            str += valueToStringImpl(v.Field(i));
        }
        str += "}"u8;
        return str;
    }
    if (exprᴛ1 == reflect.ΔInterface) {
        return typ.String() + "("u8 + valueToStringImpl(val.Elem()) + ")"u8;
    }
    if (exprᴛ1 == reflect.Func) {
        return typ.String() + "(arg)"u8;
    }
    { /* default: */
        throw panic("valueToString: can't print type " + typ.String());
    }
}

} // end reflectlite_test_package
