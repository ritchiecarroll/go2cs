// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Random number generation
namespace go;

using chacha8rand = @internal.chacha8rand_package;
using goarch = @internal.goarch_package;
using Δmath = runtime.@internal.math_package;
using @unsafe = unsafe_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for go:linkname
using @internal;
using runtime.@internal;

partial class runtime_package {

// OS-specific startup can set startupRand if the OS passes
// random data to the process at startup time.
// For example Linux passes 16 bytes in the auxv vector.
internal static slice<byte> startupRand;

// globalRand holds the global random state.
// It is only used at startup and for creating new m's.
// Otherwise the per-m random state should be used
// by calling goodrand.

[GoType("dyn")] partial struct globalRandᴛ1 {
    internal mutex @lock;
    internal array<byte> seed = new(32);
    internal chacha8rand.State state;
    internal bool init;
}
internal static ж<globalRandᴛ1> ᏑglobalRand = new StandardBox<globalRandᴛ1>(new globalRandᴛ1());
internal static ref globalRandᴛ1 globalRand => ref ᏑglobalRand.Value;

internal static ж<bool> ᏑreadRandomFailed = new StandardBox<bool>(default(bool));
internal static ref bool readRandomFailed => ref ᏑreadRandomFailed.Value;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string randinitTwiceˢ = "randinit twice"u8;

// randinit initializes the global random state.
// It must be called before any use of grand.
internal static void randinit() {
    @lock(ᏑglobalRand.of(globalRandᴛ1.Ꮡlock));
    if (globalRand.init) {
        fatal(randinitTwiceˢ);
    }
    var seed = ᏑglobalRand.of(globalRandᴛ1.Ꮡseed);
    if (startupRand != default!){
        foreach (var (i, c) in startupRand) {
            seed.Value[i % 32] ^= (byte)(c);
        }
        builtin.clear(startupRand);
        startupRand = default!;
    } else {
        if (readRandom((~seed)[..]) != 32) {
            // readRandom should never fail, but if it does we'd rather
            // not make Go binaries completely unusable, so make up
            // some random data based on the current time.
            readRandomFailed = true;
            readTimeRandom((~seed)[..]);
        }
    }
    ᏑglobalRand.of(globalRandᴛ1.Ꮡstate).Init(seed.Value);
    builtin.clear((~seed)[..]);
    globalRand.init = true;
    unlock(ᏑglobalRand.of(globalRandᴛ1.Ꮡlock));
}

// readTimeRandom stretches any entropy in the current time
// into entropy the length of r and XORs it into r.
// This is a fallback for when readRandom does not read
// the full requested amount.
// Whatever entropy r already contained is preserved.
internal static void readTimeRandom(slice<byte> r) {
    // Inspired by wyrand.
    // An earlier version of this code used getg().m.procid as well,
    // but note that this is called so early in startup that procid
    // is not initialized yet.
    var v = (uint64)nanotime();
    while (len(r) > 0) {
        v ^= (uint64)(0xa0761d6478bd642fUL);
        v *= 0xe7037ed1a0b428dbUL;
        nint size = 8;
        if (len(r) < 8) {
            size = len(r);
        }
        for (nint i = 0; i < size; i++) {
            r[i] ^= (byte)((byte)(v.Rsh((uint64)((8 * i)))));
        }
        r = r[(int)(size)..];
        v = (uint64)((v >> (int)(32)) | (v << (int)(32)));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string randinitMissedˢ = "randinit missed"u8;

// bootstrapRand returns a random uint64 from the global random generator.
internal static uint64 bootstrapRand() {
    @lock(ᏑglobalRand.of(globalRandᴛ1.Ꮡlock));
    if (!globalRand.init) {
        fatal(randinitMissedˢ);
    }
    while (ᐧ) {
        {
            var (x, ok) = ᏑglobalRand.of(globalRandᴛ1.Ꮡstate).Next(); if (ok) {
                unlock(ᏑglobalRand.of(globalRandᴛ1.Ꮡlock));
                return x;
            }
        }
        ᏑglobalRand.of(globalRandᴛ1.Ꮡstate).Refill();
    }
}

// bootstrapRandReseed reseeds the bootstrap random number generator,
// clearing from memory any trace of previously returned random numbers.
internal static void bootstrapRandReseed() {
    @lock(ᏑglobalRand.of(globalRandᴛ1.Ꮡlock));
    if (!globalRand.init) {
        fatal(randinitMissedˢ);
    }
    ᏑglobalRand.of(globalRandᴛ1.Ꮡstate).Reseed();
    unlock(ᏑglobalRand.of(globalRandᴛ1.Ꮡlock));
}

// rand32 is uint32(rand()), called from compiler-generated code.
//
//go:nosplit
internal static uint32 rand32() {
    return (uint32)rand();
}

// rand returns a random uint64 from the per-m chacha8 state.
// Do not change signature: used via linkname from other packages.
//
//go:nosplit
//go:linkname rand
internal static uint64 rand() {
    // Note: We avoid acquirem here so that in the fast path
    // there is just a getg, an inlined c.Next, and a return.
    // The performance difference on a 16-core AMD is
    // 3.7ns/call this way versus 4.3ns/call with acquirem (+16%).
    var mp = getg().Value.m;
    var c = mp.of(m.Ꮡchacha8);
    while (ᐧ) {
        // Note: c.Next is marked nosplit,
        // so we don't need to use mp.locks
        // on the fast path, which is that the
        // first attempt succeeds.
        var (x, ok) = c.Next();
        if (ok) {
            return x;
        }
        mp.Value.locks++; // hold m even though c.Refill may do stack split checks
        c.Refill();
        mp.Value.locks--;
    }
}

// mrandinit initializes the random state of an m.
internal static void mrandinit(ж<m> Ꮡmp) {
    ref var mp = ref Ꮡmp.DerefOrNull();

    array<uint64> seed = new(4);
    foreach (var (i, _) in seed) {
        seed[i] = bootstrapRand();
    }
    bootstrapRandReseed(); // erase key we just extracted
    Ꮡmp.of(m.Ꮡchacha8).Init64(seed);
    mp.cheaprand = rand();
}

// randn is like rand() % n but faster.
// Do not change signature: used via linkname from other packages.
//
//go:nosplit
//go:linkname randn
internal static uint32 randn(uint32 n) {
    // See https://lemire.me/blog/2016/06/27/a-fast-alternative-to-the-modulo-reduction/
    return (uint32)((((uint64)(uint32)rand() * (uint64)n) >> (int)(32)));
}

// cheaprand is a non-cryptographic-quality 32-bit random generator
// suitable for calling at very high frequency (such as during scheduling decisions)
// and at sensitive moments in the runtime (such as during stack unwinding).
// it is "cheap" in the sense of both expense and quality.
//
// cheaprand must not be exported to other packages:
// the rule is that other packages using runtime-provided
// randomness must always use rand.
//
// cheaprand should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/gopkg
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname cheaprand
//go:nosplit
internal static uint32 cheaprand() {
    var mp = getg().Value.m;
    // Implement wyrand: https://github.com/wangyi-fudan/wyhash
    // Only the platform that math.Mul64 can be lowered
    // by the compiler should be in this list.
    if ((UntypedInt)((UntypedInt)((UntypedInt)((UntypedInt)((UntypedInt)((UntypedInt)((UntypedInt)((UntypedInt)(goarch.IsAmd64 | goarch.IsArm64) | goarch.IsPpc64) | goarch.IsPpc64le) | goarch.IsMips64) | goarch.IsMips64le) | goarch.IsS390x) | goarch.IsRiscv64) | goarch.IsLoong64) == 1) {
        mp.Value.cheaprand += 0xa0761d6478bd642fUL;
        var (hi, lo) = Δmath.Mul64((~mp).cheaprand, (uint64)((~mp).cheaprand ^ 0xe7037ed1a0b428dbUL));
        return (uint32)((uint64)(hi ^ lo));
    }
    // Implement xorshift64+: 2 32-bit xorshift sequences added together.
    // Shift triplet [17,7,16] was calculated as indicated in Marsaglia's
    // Xorshift paper: https://www.jstatsoft.org/article/view/v008i14/xorshift.pdf
    // This generator passes the SmallCrush suite, part of TestU01 framework:
    // http://simul.iro.umontreal.ca/testu01/tu01.html
    var t = (ж<array<uint32>>)(uintptr)(@unsafe.Pointer.FromPinnedBox(mp.of(m.Ꮡcheaprand)));
    var (s1, s0) = (t.Value[0], t.Value[1]);
    s1 ^= (uint32)((s1 << (int)(17)));
    s1 = (uint32)((uint32)((uint32)(s1 ^ s0) ^ (s1 >> (int)(7))) ^ (s0 >> (int)(16)));
    (t.Value[0], t.Value[1]) = (s0, s1);
    return s0 + s1;
}

// cheaprand64 is a non-cryptographic-quality 63-bit random generator
// suitable for calling at very high frequency (such as during sampling decisions).
// it is "cheap" in the sense of both expense and quality.
//
// cheaprand64 must not be exported to other packages:
// the rule is that other packages using runtime-provided
// randomness must always use rand.
//
// cheaprand64 should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/zhangyunhao116/fastrand
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname cheaprand64
//go:nosplit
internal static int64 cheaprand64() {
    return (int64)(((int64)cheaprand() << (int)(31)) ^ (int64)cheaprand());
}

// cheaprandn is like cheaprand() % n but faster.
//
// cheaprandn must not be exported to other packages:
// the rule is that other packages using runtime-provided
// randomness must always use randn.
//
// cheaprandn should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/phuslu/log
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname cheaprandn
//go:nosplit
internal static uint32 cheaprandn(uint32 n) {
    // See https://lemire.me/blog/2016/06/27/a-fast-alternative-to-the-modulo-reduction/
    return (uint32)((((uint64)cheaprand() * (uint64)n) >> (int)(32)));
}

// Too much legacy code has go:linkname references
// to runtime.fastrand and friends, so keep these around for now.
// Code should migrate to math/rand/v2.Uint64,
// which is just as fast, but that's only available in Go 1.22+.
// It would be reasonable to remove these in Go 1.24.
// Do not call these from package runtime.

//go:linkname legacy_fastrand runtime.fastrand
internal static uint32 legacy_fastrand() {
    return (uint32)rand();
}

//go:linkname legacy_fastrandn runtime.fastrandn
internal static uint32 legacy_fastrandn(uint32 n) {
    return randn(n);
}

//go:linkname legacy_fastrand64 runtime.fastrand64
internal static uint64 legacy_fastrand64() {
    return rand();
}

} // end runtime_package
