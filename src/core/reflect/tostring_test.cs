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
internal static ref readonly @string zeroValueˢ => ref zeroValueˢᵇ;
private static readonly @string zeroValueˢᵇ = "<zero Value>"u8;
internal static ref readonly @string trueˢ => ref trueˢᵇ;
private static readonly @string trueˢᵇ = "true"u8;
internal static ref readonly @string falseˢ => ref falseˢᵇ;
private static readonly @string falseˢᵇ = "false"u8;

// valueToString returns a textual representation of the reflection value val.
// For debugging only.
internal static @string valueToString(reflectꓸValue val) {
    @string str = default;
    if (!val.IsValid()) {
        return zeroValueˢ;
    }
    var typ = val.Type();
    switch (val.Kind()) {
    case var ᴛ when ᴛ == ΔInt || ᴛ == Int8 || ᴛ == Int16 || ᴛ == Int32 || ᴛ == Int64: {
        return strconv.FormatInt(val.Int(), 10);
    }
    case var ᴛ when ᴛ == ΔUint || ᴛ == Uint8 || ᴛ == Uint16 || ᴛ == Uint32 || ᴛ == Uint64 || ᴛ == Uintptr: {
        return strconv.FormatUint(val.Uint(), 10);
    }
    case var ᴛ when ᴛ == Float32 || ᴛ == Float64: {
        return strconv.FormatFloat(val.Float(), (byte)'g', -1, 64);
    }
    case var ᴛ when ᴛ == Complex64 || ᴛ == Complex128: {
        var c = val.Complex();
        return strconv.FormatFloat(real(c), (byte)'g', -1, 64) + "+"u8 + strconv.FormatFloat(imag(c), (byte)'g', -1, 64) + "i"u8;
    }
    case var ᴛ when ᴛ == ΔString: {
        return val.String();
    }
    case var ᴛ when ᴛ == ΔBool: {
        if (val.Bool()) {
            return trueˢ;
        } else {
            return falseˢ;
        }
        break;
    }
    case var ᴛ when ᴛ == ΔPointer: {
        var v = val;
        str = typ.String() + "("u8;
        if (v.IsNil()) {
            str += "0"u8;
        } else {
            str += "&"u8 + valueToString(v.Elem());
        }
        str += ")"u8;
        return str;
    }
    case var ᴛ when ᴛ == Array || ᴛ == ΔSlice: {
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
    case var ᴛ when ᴛ == Map: {
        var t = typ;
        str = t.String();
        str += "{"u8;
        str += "<can't iterate on maps>"u8;
        str += "}"u8;
        return str;
    }
    case var ᴛ when ᴛ == Chan: {
        str = typ.String();
        return str;
    }
    case var ᴛ when ᴛ == Struct: {
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
    case var ᴛ when ᴛ == ΔInterface: {
        return typ.String() + "("u8 + valueToString(val.Elem()) + ")"u8;
    }
    case var ᴛ when ᴛ == Func: {
        var v = val;
        return typ.String() + "("u8 + strconv.FormatUint((uint64)v.Pointer(), 10) + ")"u8;
    }
    default: {
        throw panic("valueToString: can't print type " + typ.String());
        break;
    }}
}

} // end reflect_test_package
