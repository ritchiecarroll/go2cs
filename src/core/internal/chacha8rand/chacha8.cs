// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package chacha8rand implements a pseudorandom generator
// based on ChaCha8. It is used by both runtime and math/rand/v2
// and must have minimal dependencies.
namespace go.@internal;

using byteorder = go.@internal.byteorder_package;
using go.@internal;

partial class chacha8rand_package {

internal static UntypedInt ctrInc => 4; // increment counter by 4 between block calls
internal static UntypedInt ctrMax => 16; // reseed when counter reaches 16
internal static UntypedInt chunk => 32; // each chunk produced by block is 32 uint64s
internal static UntypedInt reseed => 4; // reseed with 4 words

// block is the chacha8rand block function.
internal static partial void block([GoArrayDims(4)] ж<array<uint64>> seed, [GoArrayDims(32)] ж<array<uint64>> blocks, uint32 counter);

// A State holds the state for a single random generator.
// It must be used from one goroutine at a time.
// If used by multiple goroutines at a time, the goroutines
// may see the same random values, but the code will not
// crash or cause out-of-bounds memory accesses.
[GoType] partial struct State {
    internal array<uint64> buf = new(32);
    internal array<uint64> seed = new(4);
    internal uint32 i;
    internal uint32 n;
    internal uint32 c;
}

// Next returns the next random value, along with a boolean
// indicating whether one was available.
// If one is not available, the caller should call Refill
// and then repeat the call to Next.
//
// Next is //go:nosplit to allow its use in the runtime
// with per-m data without holding the per-m lock.
//
//go:nosplit
[GoRecv] public static (uint64, bool) Next(this ref State s) {
    var i = s.i;
    if (i >= s.n) {
        return (0, false);
    }
    s.i = i + 1;
    return (s.buf[(nint)((uint32)(i & 31))], true); // i&31 eliminates bounds check
}

// Init seeds the State with the given seed value.
public static void Init(this ж<State> Ꮡs, [GoArrayDims(32)] array<byte> seed) {
    seed = seed.Clone();

    Ꮡs.Init64(new uint64[]{
        byteorder.LeUint64(seed[(int)(0 * 8)..]),
        byteorder.LeUint64(seed[(int)(1 * 8)..]),
        byteorder.LeUint64(seed[(int)(2 * 8)..]),
        byteorder.LeUint64(seed[(int)(3 * 8)..])
    }.array());
}

// Init64 seeds the state with the given seed value.
public static void Init64(this ж<State> Ꮡs, [GoArrayDims(4)] array<uint64> seed) {
    seed = seed.Clone();

    ref var s = ref Ꮡs.DerefOrNull();
    s.seed = seed.Clone();
    block(Ꮡs.of(State.Ꮡseed), Ꮡs.of(State.Ꮡbuf), 0);
    s.c = 0;
    s.i = 0;
    s.n = chunk;
}

// Refill refills the state with more random values.
// After a call to Refill, an immediate call to Next will succeed
// (unless multiple goroutines are incorrectly sharing a state).
public static void Refill(this ж<State> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    s.c += ctrInc;
    if (s.c == ctrMax) {
        // Reseed with generated uint64s for forward secrecy.
        // Normally this is done immediately after computing a block,
        // but we do it immediately before computing the next block,
        // to allow a much smaller serialized state (just the seed plus offset).
        // This gives a delayed benefit for the forward secrecy
        // (you can reconstruct the recent past given a memory dump),
        // which we deem acceptable in exchange for the reduced size.
        s.seed[0] = s.buf[(nint)(len(s.buf) - (nint)reseed) + 0];
        s.seed[1] = s.buf[(nint)(len(s.buf) - (nint)reseed) + 1];
        s.seed[2] = s.buf[(nint)(len(s.buf) - (nint)reseed) + 2];
        s.seed[3] = s.buf[(nint)(len(s.buf) - (nint)reseed) + 3];
        s.c = 0;
    }
    block(Ꮡs.of(State.Ꮡseed), Ꮡs.of(State.Ꮡbuf), s.c);
    s.i = 0;
    s.n = (uint32)len(s.buf);
    if (s.c == ctrMax - ctrInc) {
        s.n = (uint32)len(s.buf) - (uint32)reseed;
    }
}

// Reseed reseeds the state with new random values.
// After a call to Reseed, any previously returned random values
// have been erased from the memory of the state and cannot be
// recovered.
public static void Reseed(this ж<State> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    array<uint64> seed = new(4);
    foreach (var (i, _) in seed) {
        while (ᐧ) {
            var (x, ok) = s.Next();
            if (ok) {
                seed[i] = x;
                break;
            }
            Ꮡs.Refill();
        }
    }
    Ꮡs.Init64(seed);
}

// Marshal marshals the state into a byte slice.
// Marshal and Unmarshal are functions, not methods,
// so that they will not be linked into the runtime
// when it uses the State struct, since the runtime
// does not need these.
public static slice<byte> Marshal(ж<State> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    var data = new slice<byte>(6 * 8);
    copy(data, "chacha8:"u8);
    var used = (s.c / (uint32)ctrInc) * (uint32)chunk + s.i;
    byteorder.BePutUint64(data[(int)(1 * 8)..], (uint64)used);
    foreach (var (i, seed) in s.seed.ΔRangeSnapshot()) {
        byteorder.LePutUint64(data[(int)((2 + i) * 8)..], seed);
    }
    return data;
}

[GoType] partial struct errUnmarshalChaCha8 {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidChaCha8Encodingˢ = "invalid ChaCha8 encoding"u8;

[GoRecv] internal static @string Error(this ref errUnmarshalChaCha8 _) {
    return invalidChaCha8Encodingˢ;
}

// Unmarshal unmarshals the state from a byte slice.
public static error Unmarshal(ж<State> Ꮡs, slice<byte> data) {
    ref var s = ref Ꮡs.DerefOrNull();

    if (len(data) != 6 * 8 || ((sstring)(data[..8])) != "chacha8:"u8) {
        return new errUnmarshalChaCha8жerror(@new<errUnmarshalChaCha8>());
    }
    var used = byteorder.BeUint64(data[(int)(1 * 8)..]);
    if (used > (ctrMax / ctrInc) * chunk - reseed) {
        return new errUnmarshalChaCha8жerror(@new<errUnmarshalChaCha8>());
    }
    foreach (var (i, _) in s.seed) {
        s.seed[i] = byteorder.LeUint64(data[(int)((2 + i) * 8)..]);
    }
    s.c = (uint32)ctrInc * ((uint32)used / (uint32)chunk);
    block(Ꮡs.of(State.Ꮡseed), Ꮡs.of(State.Ꮡbuf), s.c);
    s.i = (uint32)used % (uint32)chunk;
    s.n = chunk;
    if (s.c == ctrMax - ctrInc) {
        s.n = chunk - reseed;
    }
    return default!;
}

} // end chacha8rand_package
