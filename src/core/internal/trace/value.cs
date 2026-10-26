// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class trace_package {

// Value is a dynamically-typed value obtained from a trace.
partial struct Value {
    internal ValueKind kind;
    internal @unsafe.Pointer pointer;
    internal uint64 scalar;
}

// ValueKind is the type of a dynamically-typed value from a trace.
partial struct ValueKind /*num:uint8*/;

public static ValueKind ValueBad => /* iota */ 0;
public static ValueKind ValueUint64 => 1;
public static ValueKind ValueString => 2;

// Kind returns the ValueKind of the value.
//
// It represents the underlying structure of the value.
//
// New ValueKinds may be added in the future. Users of this type must be robust
// to that possibility.
public static ValueKind Kind(this Value v) {
    return v.kind;
}

// Uint64 returns the uint64 value for a ValueUint64.
//
// Panics if this Value's Kind is not ValueUint64.
public static uint64 Uint64(this Value v) {
    if (v.kind != ValueUint64) {
        throw panic("Uint64 called on Value of a different Kind");
    }
    return v.scalar;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string valueBadˢ = "Value{Bad}"u8;

// String returns the string value for a ValueString, and otherwise
// a string representation of the value for other kinds of values.
public static @string String(this Value v) {
    if (v.kind == ValueString) {
        return @unsafe.String((ж<byte>)(uintptr)(v.pointer), (nint)v.scalar);
    }
    var exprᴛ1 = v.kind;
    if (exprᴛ1 == ValueUint64) {
        return fmt.Sprintf("Value{Uint64(%d)}"u8, v.Uint64());
    }
    return valueBadˢ;
}

// Uint64Value creates a value of kind ValueUint64.
public static Value Uint64Value(uint64 x) {
    return new Value(kind: ValueUint64, scalar: x);
}

// StringValue creates a value of kind ValueString.
public static Value StringValue(@string s) {
    return new Value(kind: ValueString, scalar: (uint64)len(s), pointer: @unsafe.Pointer.FromPinnedBox(@unsafe.StringData(s)));
}

} // end trace_package
