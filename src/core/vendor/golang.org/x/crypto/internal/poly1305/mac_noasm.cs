// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (!amd64 && !ppc64le && !ppc64 && !s390x) || !gc || purego
namespace go.vendor.golang.org.x.crypto.@internal;

partial class poly1305_package {

partial struct mac {
    internal partial ref macGeneric macGeneric { get; }
}

} // end poly1305_package
