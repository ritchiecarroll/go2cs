// p256_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.
//
// This file is inspired by Go, whose notice it carries (license of record: LICENSES/BSD-3-Clause.txt):
//
// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//
// PROVENANCE:
// - init: INSPIRED BY src/crypto/internal/fips140/nistec/p256.go, Go 1.26.11, func init. The same generator tables,
//   decoded from the embedded bytes by go2cs's own loop rather than aliased.

using System;
using System.Buffers.Binary;
using go;
using go.golib;

// The marker every manualConversionFuncs companion carries: crypto/internal/fips140/nistec's init
// is suppressed at emission and provided here. A displaced init emits only the placeholder, so this
// file carries the [GoInit] module initializer itself.
[module: GoManualConversion]

namespace go.crypto.@internal.fips140;

using fiat = go.crypto.@internal.fips140.nistec.fiat_package;

// Hand-written body for crypto/internal/fips140/nistec's init (R, COORD f6acfe252d).
//
// WHAT WENT WRONG. Go 1.24 replaced a runtime-computed generator table with an EMBEDDED one and
// takes a free view over it:
//
//     p256GeneratorTables = (*[43]p256AffineTable)(unsafe.Pointer(&p256PrecomputedEmbed))
//
// The converter emitted that literally, as `(ж<array<p256AffineTable>>)(uintptr)(...FromPinnedBox
// (Ꮡp256PrecomputedEmbed))`. golib's `array<T>` is a `readonly struct` whose FIRST field is
// `T[] m_array`, and a generated `[GoType("[N]E")]` type is a struct holding a `StrongBox<array<E>>`
// -- so the raw-address route lays a MANAGED HEADER over the table's own DATA and the first eight
// bytes become a reference. It is not latent: MEASURED at the version tip, SystemCertVerify dies
// `0xC0000005` on the first `.at<>()`, ten frames deep, in `ecdsa.GenerateKey` -> `ScalarBaseMult`
// -> `nistec.Select` -> `DerefOrNull` -> `ElemRefBox<p256AffineTable>.get_ValueSlot`, and the
// program never reaches the certificate seam it exists to test.
//
// ⚠ WHY NOT THE HOUSE SEAM. q97 cured the same SHAPE in sha3 with `MemoryMarshal.Cast` over the
// array's own span, and that does not work here. `Cast` is constrained `where T : struct`, NOT
// `where T : unmanaged`: it compiles for any struct and does a RUNTIME reference check. MEASURED --
// `MemoryMarshal.Cast<byte, p256AffineTable>` builds with 0 errors and throws
// `ArgumentException: Only value types without pointers or references are supported` inside the
// module initializer. Every generated Go array type is a managed struct by construction, so no
// AGGREGATE Go array type can be cast into; q97's worked because its destination was a PRIMITIVE
// (`byte` -> `uint64`). The general cure -- the converter emitting a typed managed-backed view for
// this shape -- is post-hop work and is NOT what this file is.
//
// SO THIS IS A COPY, AND A COPY IS CORRECT HERE. MEASURED: `p256GeneratorTables` is written once,
// in this init, and read at exactly two sites (`ScalarBaseMult`'s two table lookups); nothing else
// in the corpus touches it. q97's sponge REQUIRED a genuine alias because absorb and squeeze share
// the buffer between permutations -- this table does not, and Go aliases it purely to avoid copying
// 88,064 bytes once at startup. The copy happens one time, in a module initializer.
//
// ⚠ BOTH ARMS, and the endianness branch is GONE rather than skipped. Go's function has a
// `cpu.BigEndian` arm that rebuilds the table through `byteorder.LEUint64` -- it exists because
// Go's alias is HOST-NATIVE, so on a big-endian host the embedded little-endian limbs would be
// misread. This decode reads every limb with an EXPLICIT little-endian read, so it is already
// correct on every host and the branch has nothing left to do. That is how `:577`, the big-endian
// arm's own `(ж<array<array<byte>>>)(uintptr)` reinterpret, is cured: not left behind, but
// subsumed. (`cpu.BigEndian` is a `const bool = false` in this corpus, so the arm was also
// unreachable -- but the decode does not depend on that being true.)
//
// The view is taken over the BOX'S OWN WINDOW (`ToSpan()` honours `m_low`/`m_length`), never a raw
// address, so a table that ever changed size surfaces as a bounds failure here rather than as
// corruption at first use.
//
// ⚠ SCOPE: this is a SITE cure, not the class cure. C1's census at the version tip found a whole
// class of pinned-box reinterpretations at a different pointee, with the crash site itself among
// them -- so the campaign keeps looking, and this file fixes exactly one table.
//
// ⚠ AND THE BOUND WITHIN THAT SCOPE (C2): `p256_asm.go` carries the IDENTICAL reinterpret at its
// own line 332, and it is not cured here because it is not in this corpus -- under the corpus's own
// default tags (`purego,math_big_pure_go`) this package selects only `p256.go`,
// `p256_ordinv_noasm.go` and `p256_table.go`. So the cure is complete FOR THE CORPUS AS SELECTED,
// and a tag-set change that admits the asm variant brings a second site of the same shape with it.
//
// COST, stated because nobody had: the alias viewed 88,064 bytes in place, and this decode allocates
// 2,752 `P256Element`s each holding an `array<uint64>(4)` -- roughly twice the bytes plus per-object
// headers, once, in a module initializer. Worth knowing; not worth trading the crash for.

partial class nistec_package {

// The embedded table's shape, from Go's own declaration `*[43]p256AffineTable` over
// `[43 * 32 * 2 * 4]uint64`: 43 tables of 32 affine points, each point two field elements, each
// element four 64-bit limbs.
internal const int p256GeneratorTableCount = 43;
internal const int p256AffineTablePoints = 32;
private const int p256LimbsPerElement = 4;
private const int p256ElementBytes = p256LimbsPerElement * sizeof(ulong);
private const int p256EmbeddedTableBytes =
    p256GeneratorTableCount * p256AffineTablePoints * 2 * p256ElementBytes;

[GoInit] internal static void init() {
    ReadOnlySpan<byte> src = Ꮡp256PrecomputedEmbed.Value.ToSpan();
    if (src.Length != p256EmbeddedTableBytes) {
        throw panic("nistec: internal error: p256 precomputed table has the wrong length");
    }
    var tables = new array<p256AffineTable>(p256GeneratorTableCount);
    // ⚠ EVERY HOP BELOW MUST BE BY REF, AND A BY-VALUE HOP FAILS SILENTLY (C2). The plain length
    // constructor leaves all 43 generated wrappers with a NULL backing, so every element is virgin
    // storage on first touch -- and the generator's own template warns that a by-value `m_value`
    // "would lazily allocate on the returned temp, silently dropping every write on virgin storage".
    // `array<T>`'s indexer returns `ref T`, so `ref var` materializes THIS element's backing and the
    // writes land; change any one of these to `var` and the decode still compiles, still runs, still
    // exits 0 -- and writes a table of ZEROS. Wrong curve points, no exception, and the crash this
    // file cures would come back as a silently wrong answer instead.
    int off = 0;
    for (int t = 0; t < p256GeneratorTableCount; t++) {
        ref var table = ref tables[t];
        for (int i = 0; i < p256AffineTablePoints; i++) {
            ref var point = ref table[i];
            fiat.SetMontgomeryLimbs(ref point.x,
                BinaryPrimitives.ReadUInt64LittleEndian(src[off..]),
                BinaryPrimitives.ReadUInt64LittleEndian(src[(off + 8)..]),
                BinaryPrimitives.ReadUInt64LittleEndian(src[(off + 16)..]),
                BinaryPrimitives.ReadUInt64LittleEndian(src[(off + 24)..]));
            off += p256ElementBytes;
            fiat.SetMontgomeryLimbs(ref point.y,
                BinaryPrimitives.ReadUInt64LittleEndian(src[off..]),
                BinaryPrimitives.ReadUInt64LittleEndian(src[(off + 8)..]),
                BinaryPrimitives.ReadUInt64LittleEndian(src[(off + 16)..]),
                BinaryPrimitives.ReadUInt64LittleEndian(src[(off + 24)..]));
            off += p256ElementBytes;
        }
    }
    p256GeneratorTables = Ꮡ(tables);
}

} // end nistec_package
