// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using fmt = fmt_package;

partial class trace_package {

// Value is a dynamically-typed value obtained from a trace.
partial struct Value {
    internal ValueKind kind;
    internal uint64 scalar;
}

partial struct ValueKind /*num:uint8*/;

public static ValueKind ValueBad => /* iota */ 0;
public static ValueKind ValueUint64 => 1;

// Kind returns the ValueKind of the value.
//
// It represents the underlying structure of the value.
//
// New ValueKinds may be added in the future. Users of this type must be robust
// to that possibility.
public static ValueKind Kind(this Value v) {
    return v.kind;
}

// Uint64 returns the uint64 value for a MetricSampleUint64.
//
// Panics if this metric sample's Kind is not MetricSampleUint64.
public static uint64 Uint64(this Value v) {
    if (v.kind != ValueUint64) {
        throw panic("Uint64 called on Value of a different Kind");
    }
    return v.scalar;
}

// valueAsString produces a debug string value.
//
// This isn't just Value.String because we may want to use that to store
// string values in the future.
internal static @string valueAsString(Value v) {
    var exprᴛ1 = v.Kind();
    if (exprᴛ1 == ValueUint64) {
        return fmt.Sprintf("Uint64(%d)"u8, v.scalar);
    }

    return badˢ;
}

} // end trace_package
