// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (!s390x && !ppc64 && !ppc64le) || purego
namespace go.crypto.@internal.fips140;

partial class aes_package {

internal static void cryptBlocksEnc(ref Block b, /*[16]*/ ж<array<byte>> Ꮡciv, slice<byte> dst, slice<byte> src) {
    cryptBlocksEncGeneric(ref b, Ꮡciv, dst, src);
}

internal static void cryptBlocksDec(ref Block b, /*[16]*/ ж<array<byte>> Ꮡciv, slice<byte> dst, slice<byte> src) {
    cryptBlocksDecGeneric(ref b, Ꮡciv, dst, src);
}

} // end aes_package
