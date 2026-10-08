// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (!arm64 && !s390x && !ppc64 && !ppc64le) || !gc || purego
namespace go.vendor.golang.org.x.crypto;

partial class chacha20_package {

internal static UntypedInt bufSize => /* blockSize */ 64;

internal static void xorKeyStreamBlocks(this ref Cipher s, slice<byte> dst, slice<byte> src) {
    s.xorKeyStreamBlocksGeneric(dst, src);
}

} // end chacha20_package
