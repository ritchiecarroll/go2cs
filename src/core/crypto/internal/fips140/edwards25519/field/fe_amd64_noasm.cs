// Copyright (c) 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !amd64 || purego
namespace go.crypto.@internal.fips140.edwards25519;

partial class field_package {

internal static void feMul(ref Element v, ref Element x, ref Element y) {
    feMulGeneric(ref v, ref x, ref y);
}

internal static void feSquare(ref Element v, ref Element x) {
    feSquareGeneric(ref v, ref x);
}

} // end field_package
