// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (!amd64 && !s390x && !ppc64 && !ppc64le && !arm64) || purego
namespace go.crypto.@internal.fips140;

partial class aes_package {

partial struct block {
    internal partial ref blockExpanded blockExpanded { get; }
}

internal static ж<Block> newBlock(ж<Block> Ꮡc, slice<byte> key) {
    newBlockExpanded(Ꮡc.of(Block.ᏑblockExpanded), key);
    return Ꮡc;
}

internal static void encryptBlock(ref Block c, slice<byte> dst, slice<byte> src) {
    encryptBlockGeneric(ref nonnil(ref c).blockExpanded, dst, src);
}

internal static void decryptBlock(ref Block c, slice<byte> dst, slice<byte> src) {
    decryptBlockGeneric(ref nonnil(ref c).blockExpanded, dst, src);
}

internal static void checkGenericIsExpected() {
}

} // end aes_package
