// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build amd64 || arm64 || loong64 || s390x || ppc64le || ppc64
namespace go.@internal;

partial class bytealg_package {

// Index returns the index of the first instance of b in a, or -1 if b is not present in a.
// Requires 2 <= len(b) <= MaxLen.
//
//go:noescape
public static partial nint Index(slice<byte> a, slice<byte> b);

// IndexString returns the index of the first instance of b in a, or -1 if b is not present in a.
// Requires 2 <= len(b) <= MaxLen.
//
//go:noescape
public static partial nint IndexString(@string a, @string b);

} // end bytealg_package
