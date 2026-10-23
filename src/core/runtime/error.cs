// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using bytealg = @internal.bytealg_package;
using sys = @internal.runtime.sys_package;
using @internal;
using @internal.runtime;
using @unsafe = unsafe_package;

partial class runtime_package {

// Error identifies a runtime error used in panic.
//
// The Go runtime triggers panics for a variety of cases, as described by the
// Go Language Spec, such as out-of-bounds slice/array access, close of nil
// channels, type assertion failures, etc.
//
// When these cases occur, the Go runtime panics with an error that implements
// Error. This can be useful when recovering from panics to distinguish between
// custom application panics and fundamental runtime panics.
//
// Packages outside of the Go standard library should not implement Error.
partial interface ΔError :
    error
{
    // RuntimeError is a no-op function but
    // serves to distinguish types that are runtime
    // errors from ordinary errors: a type is a
    // runtime error if it has a RuntimeError method.
    void RuntimeError();
}

// A TypeAssertionError explains a failed type assertion.
partial struct TypeAssertionError {
    internal ж<_type> _interface;
    internal ж<_type> concrete;
    internal ж<_type> asserted;
    internal @string missingMethod; // one method needed by Interface, missing from Concrete
}

public static void RuntimeError(this ref TypeAssertionError _) {
}

public static @string Error(this ref TypeAssertionError e) {
    @string inter = interfaceˢ;
    if (e._interface != nil) {
        inter = toRType(e._interface).@string();
    }
    @string @as = toRType(e.asserted).@string();
    if (e.concrete == nil) {
        return "interface conversion: "u8 + inter + " is nil, not "u8 + @as;
    }
    @string cs = toRType(e.concrete).@string();
    if (e.missingMethod == ""u8) {
        @string msg = "interface conversion: "u8 + inter + " is "u8 + cs + ", not "u8 + @as;
        if (cs == @as) {
            // provide slightly clearer error message
            if (toRType(e.concrete).pkgpath() != toRType(e.asserted).pkgpath()){
                msg += " (types from different packages)"u8;
            } else {
                msg += " (types from different scopes)"u8;
            }
        }
        return msg;
    }
    return "interface conversion: "u8 + cs + " is not "u8 + @as + ": missing method "u8 + e.missingMethod;
}

// itoa converts val to a decimal representation. The result is
// written somewhere within buf and the location of the result is returned.
// buf must be at least 20 bytes.
//
//go:nosplit
internal static slice<byte> itoa(slice<byte> buf, uint64 val) {
    nint i = len(buf) - 1;
    while (val >= 10) {
        buf[i] = (byte)(val % 10 + (rune)'0');
        i--;
        val /= 10;
    }
    buf[i] = (byte)(val + (rune)'0');
    return buf.slice(i);
}

// An errorString represents a runtime error described by a single string.
partial struct errorString /*@string*/;

internal static void RuntimeError(this errorString e) {
}

internal static @string Error(this errorString e) {
    return "runtime error: "u8 + ((@string)e);
}

partial struct errorAddressString {
    internal @string msg; // error message
    internal uintptr addr; // memory address where the error occurred
}

internal static void RuntimeError(this errorAddressString e) {
}

internal static @string Error(this errorAddressString e) {
    return "runtime error: "u8 + e.msg;
}

// Addr returns the memory address where a fault occurred.
// The address provided is best-effort.
// The veracity of the result may depend on the platform.
// Errors providing this method will only be returned as
// a result of using [runtime/debug.SetPanicOnFault].
internal static uintptr Addr(this errorAddressString e) {
    return e.addr;
}

// plainError represents a runtime error described a string without
// the prefix "runtime error: " after invoking errorString.Error().
// See Issue #14965.
partial struct plainError /*@string*/;

internal static void RuntimeError(this plainError e) {
}

internal static @string Error(this plainError e) {
    return (@string)e;
}

// A boundsError represents an indexing or slicing operation gone wrong.
partial struct boundsError {
    internal int64 x;
    internal nint y;
    // Values in an index or slice expression can be signed or unsigned.
    // That means we'd need 65 bits to encode all possible indexes, from -2^63 to 2^64-1.
    // Instead, we keep track of whether x should be interpreted as signed or unsigned.
    // y is known to be nonnegative and to fit in an int.
    internal bool signed;
    internal abi.BoundsErrorCode code;
}

// boundsErrorFmts provide error text for various out-of-bounds panics.
// Note: if you change these strings, you should adjust the size of the buffer
// in boundsError.Error below as well.
[GoLazy] partial class lazyᴛboundsErrorFmts { internal static array<@string> boundsErrorFmts = new golib.SparseArray<@string>{
    [abi.BoundsIndex] = "index out of range [%x] with length %y"u8,
    [abi.BoundsSliceAlen] = "slice bounds out of range [:%x] with length %y"u8,
    [abi.BoundsSliceAcap] = "slice bounds out of range [:%x] with capacity %y"u8,
    [abi.BoundsSliceB] = "slice bounds out of range [%x:%y]"u8,
    [abi.BoundsSlice3Alen] = "slice bounds out of range [::%x] with length %y"u8,
    [abi.BoundsSlice3Acap] = "slice bounds out of range [::%x] with capacity %y"u8,
    [abi.BoundsSlice3B] = "slice bounds out of range [:%x:%y]"u8,
    [abi.BoundsSlice3C] = "slice bounds out of range [%x:%y:]"u8,
    [abi.BoundsConvert] = "cannot convert slice with length %y to array or pointer to array with length %x"u8
}.array(); }

// boundsNegErrorFmts are overriding formats if x is negative. In this case there's no need to report y.
[GoLazy] partial class lazyᴛboundsNegErrorFmts { internal static array<@string> boundsNegErrorFmts = new golib.SparseArray<@string>{
    [abi.BoundsIndex] = "index out of range [%x]"u8,
    [abi.BoundsSliceAlen] = "slice bounds out of range [:%x]"u8,
    [abi.BoundsSliceAcap] = "slice bounds out of range [:%x]"u8,
    [abi.BoundsSliceB] = "slice bounds out of range [%x:]"u8,
    [abi.BoundsSlice3Alen] = "slice bounds out of range [::%x]"u8,
    [abi.BoundsSlice3Acap] = "slice bounds out of range [::%x]"u8,
    [abi.BoundsSlice3B] = "slice bounds out of range [:%x:]"u8,
    [abi.BoundsSlice3C] = "slice bounds out of range [%x::]"u8
}.array(); }

internal static void RuntimeError(this boundsError e) {
}

internal static slice<byte> appendIntStr(slice<byte> b, int64 v, bool signed) {
    if (signed && v < 0) {
        b = append(b, (byte)((rune)'-'));
        v = -v;
    }
    array<byte> buf = new(20);
    b = appendꓸꓸꓸ(b, itoa(buf[..], (uint64)v));
    return b;
}

internal static @string Error(this boundsError e) {
    var bᴷ = new stackAppend<byte>();

    @string fmt = boundsErrorFmts[e.code];
    if (e.signed && e.x < 0) {
        fmt = boundsNegErrorFmts[e.code];
    }
    // max message length is 99: "runtime error: slice bounds out of range [::%x] with capacity %y"
    // x can be at most 20 characters. y can be at most 19.
    var b = new slice<byte>(0, 100);
    b = append(b, ((@string)"runtime error: "u8).ꓸꓸꓸ);
    for (nint i = 0; i < len(fmt); i++) {
        var c = fmt[i];
        if (c != (rune)'%') {
            b = append(bᴷ, b, c);
            continue;
        }
        i++;
        switch (fmt[i]) {
        case (rune)'x': {
            b = appendIntStr(b, e.x, e.signed);
            break;
        }
        case (rune)'y': {
            b = appendIntStr(b, (int64)e.y, true);
            break;
        }}
    }
    return (@string)b;
}

partial interface stringer {
    @string String();
}

// printpanicval prints an argument passed to panic.
// If panic is called with a value that has a String or Error method,
// it has already been converted into a string by preprintpanics.
//
// To ensure that the traceback can be unambiguously parsed even when
// the panic value contains "\ngoroutine" and other stack-like
// strings, newlines in the string representation of v are replaced by
// "\n\t".
internal static void printpanicval(any v) {
    switch (v.type()) {
    case null: {
        print((@string)"nil"u8);
        break;
    }
    case bool vΔ1: {
        print(vΔ1);
        break;
    }
    case nint vΔ1: {
        print(vΔ1);
        break;
    }
    case int8 vΔ1: {
        print(vΔ1);
        break;
    }
    case int16 vΔ1: {
        print(vΔ1);
        break;
    }
    case int32 vΔ1: {
        print(vΔ1);
        break;
    }
    case int64 vΔ1: {
        print(vΔ1);
        break;
    }
    case nuint vΔ1: {
        print(vΔ1);
        break;
    }
    case uint8 vΔ1: {
        print(vΔ1);
        break;
    }
    case uint16 vΔ1: {
        print(vΔ1);
        break;
    }
    case uint32 vΔ1: {
        print(vΔ1);
        break;
    }
    case uint64 vΔ1: {
        print(vΔ1);
        break;
    }
    case uintptr vΔ1: {
        print(vΔ1);
        break;
    }
    case float32 vΔ1: {
        print(vΔ1);
        break;
    }
    case float64 vΔ1: {
        print(vΔ1);
        break;
    }
    case complex64 vΔ1: {
        print(vΔ1);
        break;
    }
    case complex128 vΔ1: {
        print(vΔ1);
        break;
    }
    case @string vΔ1: {
        printindented(vΔ1);
        break;
    }
    default: {
        var vΔ1 = v;
        printanycustomtype(vΔ1);
        break;
    }}
}

// Invariant: each newline in the string representation is followed by a tab.
internal static void printanycustomtype(any iʗp) {
    ref var i = ref heap(iʗp, out var Ꮡi);

    var eface = efaceOf(Ꮡi);
    @string typestring = toRType((~eface)._type).@string();

    var exprᴛ1 = (~eface)._type.Kind();
    if (exprᴛ1 == abi.ΔString) {
        print(typestring, (@string)@"("""u8);
        printindented(~(ж<@string>)(uintptr)((~eface).data));
        print((@string)@""")"u8);
    }
    else if (exprᴛ1 == abi.Bool) {
        print(typestring, (@string)"("u8, ~(ж<bool>)(uintptr)((~eface).data), (@string)")"u8);
    }
    else if (exprᴛ1 == abi.Int) {
        print(typestring, (@string)"("u8, ~(ж<nint>)(uintptr)((~eface).data), (@string)")"u8);
    }
    else if (exprᴛ1 == abi.Int8) {
        print(typestring, (@string)"("u8, ~(ж<int8>)(uintptr)((~eface).data), (@string)")"u8);
    }
    else if (exprᴛ1 == abi.Int16) {
        print(typestring, (@string)"("u8, ~(ж<int16>)(uintptr)((~eface).data), (@string)")"u8);
    }
    else if (exprᴛ1 == abi.Int32) {
        print(typestring, (@string)"("u8, ~(ж<int32>)(uintptr)((~eface).data), (@string)")"u8);
    }
    else if (exprᴛ1 == abi.Int64) {
        print(typestring, (@string)"("u8, ~(ж<int64>)(uintptr)((~eface).data), (@string)")"u8);
    }
    else if (exprᴛ1 == abi.Uint) {
        print(typestring, (@string)"("u8, ~(ж<nuint>)(uintptr)((~eface).data), (@string)")"u8);
    }
    else if (exprᴛ1 == abi.Uint8) {
        print(typestring, (@string)"("u8, ~(ж<uint8>)(uintptr)((~eface).data), (@string)")"u8);
    }
    else if (exprᴛ1 == abi.Uint16) {
        print(typestring, (@string)"("u8, ~(ж<uint16>)(uintptr)((~eface).data), (@string)")"u8);
    }
    else if (exprᴛ1 == abi.Uint32) {
        print(typestring, (@string)"("u8, ~(ж<uint32>)(uintptr)((~eface).data), (@string)")"u8);
    }
    else if (exprᴛ1 == abi.Uint64) {
        print(typestring, (@string)"("u8, ~(ж<uint64>)(uintptr)((~eface).data), (@string)")"u8);
    }
    else if (exprᴛ1 == abi.Uintptr) {
        print(typestring, (@string)"("u8, ~(ж<uintptr>)(uintptr)((~eface).data), (@string)")"u8);
    }
    else if (exprᴛ1 == abi.Float32) {
        print(typestring, (@string)"("u8, ~(ж<float32>)(uintptr)((~eface).data), (@string)")"u8);
    }
    else if (exprᴛ1 == abi.Float64) {
        print(typestring, (@string)"("u8, ~(ж<float64>)(uintptr)((~eface).data), (@string)")"u8);
    }
    else if (exprᴛ1 == abi.Complex64) {
        print(typestring, ~(ж<complex64>)(uintptr)((~eface).data));
    }
    else if (exprᴛ1 == abi.Complex128) {
        print(typestring, ~(ж<complex128>)(uintptr)((~eface).data));
    }
    else { /* default: */
        print((@string)"("u8, typestring, (@string)") "u8, @unsafe.Pointer.OrTypedNil((~eface).data));
    }
}

// printindented prints s, replacing "\n" with "\n\t".
internal static void printindented(@string s) {
    while (ᐧ) {
        nint i = bytealg.IndexByteString(s, (rune)'\n');
        if (i < 0) {
            break;
        }
        i += len("\n");
        print(s.slice(0, i));
        print((@string)"\t"u8);
        s = s.slice(i);
    }
    print(s);
}

// panicwrap generates a panic for a call to a wrapped value method
// with a nil pointer receiver.
//
// It is called from the generated wrapper code.
internal static void panicwrap() {
    var pc = sys.GetCallerPC();
    @string name = funcNameForPrint(funcname(findfunc(pc)));
    // name is something like "main.(*T).F".
    // We want to extract pkg ("main"), typ ("T"), and meth ("F").
    // Do it by finding the parens.
    nint i = bytealg.IndexByteString(name, (rune)'(');
    if (i < 0) {
        @throw("panicwrap: no ( in "u8 + name);
    }
    @string pkg = name.slice(0, i - 1);
    if (i + 2 >= len(name) || name.slice(i - 1, i + 2) != ".(*") {
        @throw("panicwrap: unexpected string after package name: "u8 + name);
    }
    name = name.slice(i + 2);
    i = bytealg.IndexByteString(name, (rune)')');
    if (i < 0) {
        @throw("panicwrap: no ) in "u8 + name);
    }
    if (i + 2 >= len(name) || name.slice(i, i + 2) != ").") {
        @throw("panicwrap: unexpected string after type name: "u8 + name);
    }
    @string typ = name.slice(0, i);
    @string meth = name.slice(i + 2);
    throw panic(((plainError)("value method "u8 + pkg + "."u8 + typ + "."u8 + meth + " called using nil *"u8 + typ + " pointer"u8)));
}

} // end runtime_package
