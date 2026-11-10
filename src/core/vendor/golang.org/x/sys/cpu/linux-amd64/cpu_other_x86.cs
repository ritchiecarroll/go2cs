// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build 386 || amd64p32 || (amd64 && (!darwin || !gc))
namespace go.vendor.golang.org.x.sys;

partial class cpu_package {

internal static bool darwinSupportsAVX512() {
    throw panic("only implemented for gc && amd64 && darwin");
}

} // end cpu_package
