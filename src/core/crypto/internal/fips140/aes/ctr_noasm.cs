// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (!amd64 && !arm64 && !s390x && !ppc64 && !ppc64le) || purego
namespace go.crypto.@internal.fips140;

partial class aes_package {

internal static void ctrBlocks1(ref Block b, /*[16]*/ ж<array<byte>> Ꮡdst, /*[16]*/ ж<array<byte>> Ꮡsrc, uint64 ivlo, uint64 ivhi) {
    ref var dst = ref Ꮡdst.DerefOrNull();
    ref var src = ref Ꮡsrc.DerefOrNull();

    ctrBlocks(ref b, dst[..], src[..], ivlo, ivhi);
}

internal static void ctrBlocks2(ref Block b, /*[32]*/ ж<array<byte>> Ꮡdst, /*[32]*/ ж<array<byte>> Ꮡsrc, uint64 ivlo, uint64 ivhi) {
    ref var dst = ref Ꮡdst.DerefOrNull();
    ref var src = ref Ꮡsrc.DerefOrNull();

    ctrBlocks(ref b, dst[..], src[..], ivlo, ivhi);
}

internal static void ctrBlocks4(ref Block b, /*[64]*/ ж<array<byte>> Ꮡdst, /*[64]*/ ж<array<byte>> Ꮡsrc, uint64 ivlo, uint64 ivhi) {
    ref var dst = ref Ꮡdst.DerefOrNull();
    ref var src = ref Ꮡsrc.DerefOrNull();

    ctrBlocks(ref b, dst[..], src[..], ivlo, ivhi);
}

internal static void ctrBlocks8(ref Block b, /*[128]*/ ж<array<byte>> Ꮡdst, /*[128]*/ ж<array<byte>> Ꮡsrc, uint64 ivlo, uint64 ivhi) {
    ref var dst = ref Ꮡdst.DerefOrNull();
    ref var src = ref Ꮡsrc.DerefOrNull();

    ctrBlocks(ref b, dst[..], src[..], ivlo, ivhi);
}

} // end aes_package
