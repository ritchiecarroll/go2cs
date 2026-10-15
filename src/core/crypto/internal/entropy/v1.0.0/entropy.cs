// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package entropy implements a CPU jitter-based SP 800-90B entropy source.
namespace go.crypto.@internal.entropy.v1.0.0;

using time = go.crypto.@internal.fips140deps.time_package;
using errors = errors_package;
using atomic = sync.atomic_package;
using @unsafe = unsafe_package;
using go.crypto.@internal.fips140deps;
using sync;

partial class entropy_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string v100ˢ = "v1.0.0"u8;

// Version returns the version of the entropy source.
//
// This is independent of the FIPS 140-3 module version, in order to reuse the
// ESV certificate across module versions.
public static @string Version() {
    return v100ˢ;
}

// ScratchBuffer is a large buffer that will be written to using atomics, to
// generate noise from memory access timings. Its contents do not matter.
/* [(1 << (int)(25))]byte */
partial struct ScratchBuffer /*[33554432]byte*/;

// Seed returns a 384-bit seed with full entropy.
//
// memory is passed in to allow changing the allocation strategy without
// modifying the frozen and certified entropy source in this package.
//
// Seed returns an error if the entropy source startup health tests fail, which
// has a non-negligible chance of happening.
public static (array<byte>, error) Seed(ж<ScratchBuffer> Ꮡmemory) {
    // Collect w = 1024 samples, each certified to provide no less than h = 0.5
    // bits of entropy, for a total of hᵢₙ = w × h = 512 bits of entropy, over
    // nᵢₙ = w × n = 8192 bits of input data.
    ref var samples = ref heap(new array<byte>(1024), out var Ꮡsamples);
    {
        var err = Samples(samples[..], Ꮡmemory); if (err != default!) {
            return (new byte[]{}.array(48), err);
        }
    }

    // Use a vetted unkeyed conditioning component, SHA-384, with nw = 384 and
    // nₒᵤₜ = 384. Per the formula in SP 800-90B Section 3.1.5.1.2, the output
    // entropy hₒᵤₜ is:
    //
    //     sage: n_in = 8192
    //     sage: n_out = 384
    //     sage: nw = 384
    //     sage: h_in = 512
    //     sage: P_high = 2^(-h_in)
    //     sage: P_low = (1 - P_high) / (2^n_in - 1)
    //     sage: n = min(n_out, nw)
    //     sage: ψ = 2^(n_in - n) * P_low + P_high
    //     sage: U = 2^(n_in - n) + sqrt(2 * n * 2^(n_in - n) * ln(2))
    //     sage: ω = U * P_low
    //     sage: h_out = -log(max(ψ, ω), 2)
    //     sage: h_out.n()
    //     384.000000000000
    //
    // According to Implementation Guidance D.K, Resolution 19, since
    //
    //   - the conditioning component is vetted,
    //   - hᵢₙ = 512 ≥ nₒᵤₜ + 64 = 448, and
    //   - nₒᵤₜ ≤ security strength of SHA-384 = 384 (per SP 800-107 Rev. 1, Table 1),
    //
    // we can claim the output has full entropy.
    return (SHA384(Ꮡsamples), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string entropyAtLeast1024ˢ = "entropy: at least 1024 samples are required for startup health tests"u8;

// Samples starts a new entropy source, collects the requested number of
// samples, conducts startup health tests, and returns the samples or an error
// if the health tests fail.
//
// The health tests have a non-negligible chance of failing.
public static error Samples(slice<uint8> samples, ж<ScratchBuffer> Ꮡmemory) {
    if (len(samples) < 1024) {
        return errors.New(entropyAtLeast1024ˢ);
    }
    var s = newSource(Ꮡmemory);
    foreach (var _ᴛ1 in range(4)) {
        // Warm up the source to avoid any initial bias.
        _ = s.Sample();
    }
    foreach (var (i, _) in samples) {
        samples[i] = s.Sample();
    }
    {
        var err = RepetitionCountTest(samples); if (err != default!) {
            return err;
        }
    }
    {
        var err = AdaptiveProportionTest(samples); if (err != default!) {
            return err;
        }
    }
    return default!;
}

partial struct source {
    internal ж<ScratchBuffer> memory;
    internal uint32 lcgState;
    internal int64 previous;
}

internal static ж<source> newSource(ж<ScratchBuffer> Ꮡmemory) {
    return Ꮡ(new source(
        memory: Ꮡmemory,
        lcgState: (uint32)time.HighPrecisionNow(),
        previous: time.HighPrecisionNow()
    ));
}

// touchMemory performs a write to memory at the given index.
//
// The memory slice is passed in and may be shared across sources e.g. to avoid
// the significant (~500µs) cost of zeroing a new allocation on every [Seed] call.
internal static void touchMemory(ж<ScratchBuffer> Ꮡmemory, uint32 idx) {
    idx = idx / 4 * 4; // align to 32 bits
    var u32 = Ꮡmemory.at<byte>((ulong)(idx)).Reinterpret<byte, uint32>();
    var last = atomic.LoadUint32(u32);
    atomic.SwapUint32(u32, last + 13);
}

internal static uint8 Sample(this ref source s) {
    // Perform a few memory accesses in an unpredictable pattern to expose the
    // next measurement to as much system noise as possible.
    var (memory, lcgState) = (s.memory, s.lcgState);
    if (memory == nil) { // remove the nil check from the inlined touchMemory calls
        throw panic("entropy: nil memory buffer");
    }
    foreach (var _ᴛ1 in range(64)) {
        lcgState = 1664525 * lcgState + 1013904223;
        // Discard the lower bits, which tend to fall into short cycles.
        var idx = (uint32)(((lcgState >> (int)(6))) & ((1 << (int)(25)) - 1));
        touchMemory(memory, idx);
    }
    s.lcgState = lcgState;

    var t = time.HighPrecisionNow();
    var sample = t - s.previous;
    s.previous = t;

    // Reduce the symbol space to 256 values, assuming most of the entropy is in
    // the least-significant bits, which represent the highest-resolution timing
    // differences.
    return (uint8)sample;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string entropyRepetitionCountˢ = "entropy: repetition count health test failed"u8;

// RepetitionCountTest implements the repetition count test from SP 800-90B
// Section 4.4.1. It returns an error if any symbol is repeated C = 41 or more
// times in a row.
//
// This C value is calculated from a target failure probability α = 2⁻²⁰ and a
// claimed min-entropy per symbol h = 0.5 bits, using the formula in SP 800-90B
// Section 4.4.1.
//
//	sage: α = 2^-20
//	sage: H = 0.5
//	sage: 1 + ceil(-log(α, 2) / H)
//	41
public static error RepetitionCountTest(slice<uint8> samples) {
    var x = samples[0];
    nint count = 1;
    foreach (var (_, y) in samples[1..]) {
        if (y == x){
            count++;
            if (count >= 41) {
                return errors.New(entropyRepetitionCountˢ);
            }
        } else {
            x = y;
            count = 1;
        }
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string entropyAdaptiveˢ = "entropy: adaptive proportion health test failed"u8;

// AdaptiveProportionTest implements the adaptive proportion test from SP 800-90B
// Section 4.4.2. It returns an error if any symbol appears C = 410 or more
// times in the last W = 512 samples.
//
// This C value is calculated from a target failure probability α = 2⁻²⁰, a
// window size W = 512, and a claimed min-entropy per symbol h = 0.5 bits, using
// the formula in SP 800-90B Section 4.4.2, equivalent to the Microsoft Excel
// formula 1+CRITBINOM(W, power(2,(−H)),1−α).
//
//	sage: from scipy.stats import binom
//	sage: α = 2^-20
//	sage: H = 0.5
//	sage: W = 512
//	sage: C = 1 + binom.ppf(1 - α, W, 2**(-H))
//	sage: ceil(C)
//	410
public static error AdaptiveProportionTest(slice<uint8> samples) {
    array<nint> counts = new(256);
    foreach (var (i, x) in samples) {
        counts[x]++;
        if (i >= 512) {
            counts[samples[i - 512]]--;
        }
        if (counts[x] >= 410) {
            return errors.New(entropyAdaptiveˢ);
        }
    }
    return default!;
}

} // end entropy_package
