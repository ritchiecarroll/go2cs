// Copyright 2026 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Entropy generation in FIPS 140-3 mode uses a scratch buffer in the BSS
// section (see below), which usually doesn't cost much, except on Wasm, due to
// the way the linear memory works. FIPS 140-3 mode is not supported on Wasm, so
// we just use a build tag to exclude it. (Could also exclude other platforms
// that does not support FIPS 140-3 mode, but as the BSS variable doesn't cost
// much, don't bother.)
//
//go:build !wasm
namespace go.crypto.@internal.fips140;

using entropy = go.crypto.@internal.entropy.v1_0_0.entropy_package;

partial class drbg_package {

// memory is a scratch buffer that is accessed between samples by the entropy
// source to expose it to memory access timings.
//
// We reuse it and share it between Seed calls to avoid the significant (~500µs)
// cost of zeroing a new allocation every time. The entropy source accesses it
// using atomics (and doesn't care about its contents).
//
// It should end up in the .noptrbss section, and become backed by physical pages
// at first use. This ensures that programs that do not use the FIPS 140-3 module
// do not incur any memory use or initialization penalties.
internal static readonly ж<entropy.ScratchBuffer> Ꮡmemory = new StaticBox<entropy.ScratchBuffer>(default(entropy.ScratchBuffer));
internal static ref entropy.ScratchBuffer memory => ref ((StaticBox<entropy.ScratchBuffer>)Ꮡmemory).ValueSlot;

internal static ж<array<byte>> getEntropy() {
    nint retries = default!;
    ref var seed = ref heap<array<byte>>(out var Ꮡseed);
    (seed, var err) = entropy.Seed(Ꮡmemory);
    while (err != default!) {
        // The CPU jitter-based SP 800-90B entropy source has a non-negligible
        // chance of failing the startup health tests.
        //
        // Each time it does, it enters a permanent failure state, and we
        // restart it anew. This is not expected to happen more than a few times
        // in a row.
        {
            retries++; if (retries > 100) {
                throw panic("fips140/drbg: failed to obtain initial entropy");
            }
        }
        (seed, err) = entropy.Seed(Ꮡmemory);
    }
    return Ꮡseed;
}

} // end drbg_package
