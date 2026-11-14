// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
using System;

// Hand-finished conversion (the reflection bridge — Phase 4). Go's Swapper reads the slice
// header through unsafe.Pointer and swaps flat memory by element size — no managed form (the
// auto conversion NREs unpacking the eface; sort.Slice → Swapper was the first operational
// hit, sort's TestSlice). Swap through golib's non-generic ISlice indexer instead — the
// indexer applies the slice window offset, so swaps land on the shared backing store exactly
// like Go's. The converter skips the auto form via the manualConversionFuncs registry
// (go2cs/manualTypeOperations.go); this module marker also keeps go2cs from re-converting
// this file wholesale. See docs/phase4/DESIGN-reflection-bridge.md.

[module: go.GoManualConversion]

namespace go.@internal;

partial class reflectlite_package {

// Swapper returns a function that swaps the elements in the provided
// slice.
//
// Swapper panics if the provided interface is not a slice.
//
// The slice is read through ValueOf, as Go's body reads it, so a slice held behind an interface
// shell is the slice (makeReflectValue's unwrap), not the shell.
public static Action<nint, nint> Swapper(any Δslice) {
    if (ValueOf(Δslice).live is not ISlice s) {
        throw panic(Ꮡ(new ValueError(Method: "Swapper"u8, Kind: ValueOf(Δslice).Kind())));
    }
    // Fast path for slices of size 0 and 1. Nothing to swap.
    switch (s.Length) {
    case 0: {
        return (nint _, nint _) => {
            throw panic("reflect: slice index out of range");
        };
    }
    case 1: {
        return (nint i, nint j) => {
            if (i != 0 || j != 0) {
                throw panic("reflect: slice index out of range");
            }
        };
    }}
    return (nint i, nint j) => {
        if (!s.IndexIsValid(i) || !s.IndexIsValid(j)) {
            throw panic("reflect: slice index out of range");
        }
        (s[i], s[j]) = (s[j], s[i]);
    };
}

} // end reflectlite_package
