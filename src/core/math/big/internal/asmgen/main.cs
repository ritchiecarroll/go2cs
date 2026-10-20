// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Asmgen generates math/big assembly.
//
// Usage:
//
//	cd go/src/math/big
//	go test ./internal/asmgen -generate
//
// Or:
//
//	go generate math/big
namespace go.math.big.@internal;

partial class asmgen_package {

internal static slice<ж<Arch>> arches;
internal static void initᴛarches() { arches = new ж<Arch>[]{
    Arch386,
    ArchAMD64,
    ArchARM,
    ArchARM64,
    ArchLoong64,
    ArchMIPS,
    ArchMIPS64x,
    ArchPPC64x,
    ArchRISCV64,
    ArchS390X
}.slice(); }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string addVVˢ = "addVV"u8;
internal static readonly @string subVVˢ = "subVV"u8;
internal static readonly @string lshVUˢ = "lshVU"u8;
internal static readonly @string rshVUˢ = "rshVU"u8;

// generate returns the file name and content of the generated assembly for the given architecture.
internal static (@string @file, slice<byte> data) generate(ж<Arch> Ꮡarch) {
    @string @file = default!;
    ref var arch = ref Ꮡarch.DerefOrNull();

    @file = "arith_"u8 + arch.Name + ".s"u8;
    var a = NewAsm(Ꮡarch);
    addOrSubVV(a, addVVˢ);
    addOrSubVV(a, subVVˢ);
    shiftVU(a, lshVUˢ);
    shiftVU(a, rshVUˢ);
    mulAddVWW(a);
    addMulVVWW(a);
    return (@file, a.of(Asm.Ꮡout).Bytes());
}

} // end asmgen_package
