// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using bytes = bytes_package;
using bigmod = go.crypto.@internal.fips140.bigmod_package;
using rand = go.crypto.rand_package;
using io = io_package;
using testing = testing_package;
using go.crypto;
using go.crypto.@internal.fips140;
using static go.crypto.@internal.fips140.ecdsa_package;
using Δnistec = go.crypto.@internal.fips140.nistec_package;

partial class ecdsa_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string p224ˢ => ref p224ˢᵇ;
private static readonly @string p224ˢᵇ = "P-224"u8;
internal static ref readonly @string p256ˢ => ref p256ˢᵇ;
private static readonly @string p256ˢᵇ = "P-256"u8;
internal static ref readonly @string p384ˢ => ref p384ˢᵇ;
private static readonly @string p384ˢᵇ = "P-384"u8;
internal static ref readonly @string p521ˢ => ref p521ˢᵇ;
private static readonly @string p521ˢᵇ = "P-521"u8;

public static void TestRandomPoint(ж<testing.T> t) {
    t.Run(p224ˢ, (ж<testing.T> tΔ1) => {
        testRandomPoint<P224PointжPoint>(tΔ1, P224());
    });
    t.Run(p256ˢ, (ж<testing.T> tΔ2) => {
        testRandomPoint<P256PointжPoint>(tΔ2, P256());
    });
    t.Run(p384ˢ, (ж<testing.T> tΔ3) => {
        testRandomPoint<P384PointжPoint>(tΔ3, P384());
    });
    t.Run(p521ˢ, (ж<testing.T> tΔ4) => {
        testRandomPoint<P521PointжPoint>(tΔ4, P521());
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object kIsZeroˢ = (@string)"k is zero"u8;
internal static readonly object pIsInfinityˢ = (@string)"p is infinity"u8;
internal static readonly object overflowWasNotRejectedˢ = (@string)"overflow was not rejected"u8;
internal static readonly object zeroWasNotRejectedˢ = (@string)"zero was not rejected"u8;
internal static readonly object unexpectedRejectionˢ = (@string)"unexpected rejection"u8;

internal static void testRandomPoint<P>(ж<testing.T> t, ж<global::go.crypto.@internal.fips140.ecdsa_package.Curve<P>> Ꮡc)
    where P : global::go.crypto.@internal.fips140.ecdsa_package.Point<P>
{
    ref var c = ref Ꮡc.DerefOrNull();

    t.Cleanup(() => {
        testingOnlyRejectionSamplingLooped = default;
    });
    nint loopCount = default;
    testingOnlyRejectionSamplingLooped = () => {
        loopCount++;
    };

    // A sequence of all ones will generate 2^N-1, which should be rejected.
    // (Unless, for example, we are masking too many bits.)
    ref var r = ref heap<io.Reader>(io.MultiReader(new ecdsa_internal_test_package.bytes_ReaderжReader(bytes.NewReader(bytes.Repeat(slice<byte>([0xff]), 100))), rand.Reader), out var Ꮡr);
    {
        var (k, p, err) = randomPoint(ref (Ꮡc).DerefOrNull(), (slice<byte> b) => {
            var (_, errΔ1) = Ꮡr.ValueSlot.Read(b);
            return errΔ1;
        }); if (err != nil) {
            t.Fatal(err);
        } else if (k.IsZero() == 1) {
            t.Error(kIsZeroˢ);
        } else if (p.Bytes()[0] != 4) {
            t.Error(pIsInfinityˢ);
        }
    }
    if (loopCount == 0) {
        t.Error(overflowWasNotRejectedˢ);
    }
    loopCount = 0;

    // A sequence of all zeroes will generate zero, which should be rejected.
    r = io.MultiReader(new ecdsa_internal_test_package.bytes_ReaderжReader(bytes.NewReader(bytes.Repeat(slice<byte>([0]), 100))), rand.Reader);
    {
        var (k, p, err) = randomPoint(ref (Ꮡc).DerefOrNull(), (slice<byte> b) => {
            var (_, errΔ1) = Ꮡr.ValueSlot.Read(b);
            return errΔ1;
        }); if (err != nil) {
            t.Fatal(err);
        } else if (k.IsZero() == 1) {
            t.Error(kIsZeroˢ);
        } else if (p.Bytes()[0] != 4) {
            t.Error(pIsInfinityˢ);
        }
    }
    if (loopCount == 0) {
        t.Error(zeroWasNotRejectedˢ);
    }
    loopCount = 0;

    // P-256 has a 2⁻³² chance of randomly hitting a rejection. For P-224 it's
    // 2⁻¹¹², for P-384 it's 2⁻¹⁹⁴, and for P-521 it's 2⁻²⁶², so if we hit in
    // tests, something is horribly wrong. (For example, we are masking the
    // wrong bits.)
    if (c.curve == p256) {
        return;
    }
    {
        var (k, p, err) = randomPoint(ref (Ꮡc).DerefOrNull(), (slice<byte> b) => {
            var (_, errΔ1) = rand.Reader.Read(b);
            return errΔ1;
        }); if (err != nil) {
            t.Fatal(err);
        } else if (k.IsZero() == 1) {
            t.Error(kIsZeroˢ);
        } else if (p.Bytes()[0] != 4) {
            t.Error(pIsInfinityˢ);
        }
    }
    if (loopCount > 0) {
        t.Error(unexpectedRejectionˢ);
    }
}

public static void TestHashToNat(ж<testing.T> t) {
    t.Run(p224ˢ, (ж<testing.T> tΔ1) => {
        testHashToNat<P224PointжPoint>(tΔ1, P224());
    });
    t.Run(p256ˢ, (ж<testing.T> tΔ2) => {
        testHashToNat<P256PointжPoint>(tΔ2, P256());
    });
    t.Run(p384ˢ, (ж<testing.T> tΔ3) => {
        testHashToNat<P384PointжPoint>(tΔ3, P384());
    });
    t.Run(p521ˢ, (ж<testing.T> tΔ4) => {
        testHashToNat<P521PointжPoint>(tΔ4, P521());
    });
}

internal static void testHashToNat<P>(ж<testing.T> t, ж<global::go.crypto.@internal.fips140.ecdsa_package.Curve<P>> c)
    where P : global::go.crypto.@internal.fips140.ecdsa_package.Point<P>
{
    for (nint l = 0; l < 600; l++) {
        var h = bytes.Repeat(slice<byte>([0xff]), l);
        hashToNat(ref (c).DerefOrNull(), bigmod.NewNat(), h);
    }
}

} // end ecdsa_internal_test_package
