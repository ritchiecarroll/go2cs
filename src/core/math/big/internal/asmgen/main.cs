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

[GoLazy] partial class lazyᴛarches { internal static slice<ж<Arch>> arches = slice<ж<Arch>>([
    lazyᴛArch386.initial,
    lazyᴛArchAMD64.initial,
    lazyᴛArchARM.initial,
    lazyᴛArchARM64.initial,
    lazyᴛArchLoong64.initial,
    lazyᴛArchMIPS.initial,
    lazyᴛArchMIPS64x.initial,
    lazyᴛArchPPC64x.initial,
    lazyᴛArchRISCV64.initial,
    lazyᴛArchS390X.initial
]); }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string addVVˢ = "addVV"u8;
internal static readonly @string subVVˢ = "subVV"u8;
internal static readonly @string lshVUˢ = "lshVU"u8;
internal static readonly @string rshVUˢ = "rshVU"u8;

// generate returns the file name and content of the generated assembly for the given architecture.
internal static (@string @file, slice<byte> data) generate(ж<Arch> Ꮡarch) {
    @string @file = default;
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
