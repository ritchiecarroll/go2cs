// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build purego

// Package alias implements memory aliasing tests.
namespace go.vendor.golang.org.x.crypto.@internal;

// This is the Google App Engine standard variant based on reflect
// because the unsafe package and cgo are disallowed.
using reflect = reflect_package;

partial class alias_package {

// go2cs generated this placeholder — func AnyOverlap is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// InexactOverlap reports whether x and y share memory at any non-corresponding
// index. The memory beyond the slice length is ignored. Note that x and y can
// have different lengths and still not have any inexact overlap.
//
// InexactOverlap can be used to implement the requirements of the crypto/cipher
// AEAD, Block, BlockMode and Stream interfaces.
public static bool InexactOverlap(slice<byte> x, slice<byte> y) {
    if (len(x) == 0 || len(y) == 0 || Ꮡelem(x, 0) == Ꮡelem(y, 0)) {
        return false;
    }
    return AnyOverlap(x, y);
}

} // end alias_package
