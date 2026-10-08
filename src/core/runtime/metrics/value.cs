// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using math = math_package;
using @unsafe = unsafe_package;

partial class metrics_package {

partial struct ValueKind /*num:nint*/;

public static ValueKind KindBad => /* iota */ 0;
public static ValueKind KindUint64 => 1;
public static ValueKind KindFloat64 => 2;
public static ValueKind KindFloat64Histogram => 3;

// Value represents a metric value returned by the runtime.
partial struct Value {
    internal ValueKind kind;
    internal uint64 scalar;         // contains scalar values for scalar Kinds.
    internal @unsafe.Pointer pointer; // contains non-scalar values.
}

// Kind returns the tag representing the kind of value this is.
public static ValueKind Kind(this Value v) {
    return v.kind;
}

// Uint64 returns the internal uint64 value for the metric.
//
// If v.Kind() != KindUint64, this method panics.
public static uint64 Uint64(this Value v) {
    if (v.kind != KindUint64) {
        throw panic("called Uint64 on non-uint64 metric value");
    }
    return v.scalar;
}

// Float64 returns the internal float64 value for the metric.
//
// If v.Kind() != KindFloat64, this method panics.
public static float64 Float64(this Value v) {
    if (v.kind != KindFloat64) {
        throw panic("called Float64 on non-float64 metric value");
    }
    return math.Float64frombits(v.scalar);
}

// Float64Histogram returns the internal *Float64Histogram value for the metric.
//
// If v.Kind() != KindFloat64Histogram, this method panics.
public static ж<ΔFloat64Histogram> Float64Histogram(this Value v) {
    if (v.kind != KindFloat64Histogram) {
        throw panic("called Float64Histogram on non-Float64Histogram metric value");
    }
    return (ж<ΔFloat64Histogram>)(uintptr)(v.pointer);
}

} // end metrics_package
