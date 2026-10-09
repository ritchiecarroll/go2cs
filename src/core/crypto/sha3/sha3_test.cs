// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using cryptotest = go.crypto.@internal.cryptotest_package;
using fips140 = go.crypto.@internal.fips140_package;
using static go.crypto.sha3_package;
using hex = encoding.hex_package;
using io = io_package;
using rand = math.rand_package;
using strings = strings_package;
using testing = testing_package;
using encoding;
using go.crypto;
using go.crypto.@internal;
using math;
using sha3 = go.crypto.sha3_package;

partial class sha3_test_package {

internal static readonly @string testString = "brekeccakkeccak koax koax"u8;

// testDigests contains functions returning hash.Hash instances
// with output-length equal to the KAT length for SHA-3, Keccak
// and SHAKE instances.
internal static map<@string, Func<ж<sha3.SHA3>>> testDigests = new map<@string, Func<ж<sha3.SHA3>>>{
    ["SHA3-224"u8] = New224,
    ["SHA3-256"u8] = New256,
    ["SHA3-384"u8] = New384,
    ["SHA3-512"u8] = New512
};

// NewCSHAKE without customization produces same result as SHAKE
// testShakes contains functions that return *sha3.SHAKE instances for
// with output-length equal to the KAT length.

partial struct testShakesᴛ1 /*dyn*/ {
    internal Func<slice<byte>, slice<byte>, ж<sha3.SHAKE>> constructor;
    internal @string defAlgoName;
    internal @string defCustomStr;
}
internal static map<@string, testShakesᴛ1> testShakes = new map<@string, testShakesᴛ1>{
    ["SHAKE128"u8] = new(NewCSHAKE128, ""u8, ""u8),
    ["SHAKE256"u8] = new(NewCSHAKE256, ""u8, ""u8),
    ["cSHAKE128"u8] = new(NewCSHAKE128, "CSHAKE128"u8, "CustomString"u8),
    ["cSHAKE256"u8] = new(NewCSHAKE256, "CSHAKE256"u8, "CustomString"u8)
};

// decodeHex converts a hex-encoded string into a raw byte string.
internal static slice<byte> decodeHex(@string s) {
    var (b, err) = hex.DecodeString(s);
    if (err != default!) {
        throw panic(err);
    }
    return b;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string sha3ˢ = "sha3"u8;

// TestUnalignedWrite tests that writing data in an arbitrary pattern with
// small input buffers.
public static void TestUnalignedWrite(ж<testing.T> Ꮡt) {
    cryptotest.TestAllImplementations(Ꮡt, sha3ˢ, testUnalignedWrite);
}

internal static void testUnalignedWrite(ж<testing.T> Ꮡt) {
    var buf = sequentialBytes(0x10000);
    foreach (var (alg, df) in testDigests) {
        var d = df();
        d.Reset();
        d.Write(buf);
        var want = d.Sum(default!);
        d.Reset();
        for (nint i = 0; i < len(buf); ) {
            // Cycle through offsets which make a 137 byte sequence.
            // Because 137 is prime this sequence should exercise all corner cases.
            var offsets = new nint[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 1}.array();
            foreach (var (_, vᴛ1) in offsets.ΔRangeSnapshot()) {
                var j = vᴛ1;

                {
                    nint v = len(buf) - i; if (v < j) {
                        j = v;
                    }
                }
                d.Write(buf.slice(i, i + j));
                i += j;
            }
        }
        var got = d.Sum(default!);
        if (!bytes.Equal(got, want)) {
            Ꮡt.Errorf("Unaligned writes, alg=%s\ngot %q, want %q"u8, alg, got, want);
        }
    }
    // Same for SHAKE
    foreach (var (alg, df) in testShakes) {
        var want = new slice<byte>(16);
        var got = new slice<byte>(16);
        var d = df.constructor(slice<byte>(df.defAlgoName), slice<byte>(df.defCustomStr));
        d.Reset();
        d.Write(buf);
        d.Read(want);
        d.Reset();
        for (nint i = 0; i < len(buf); ) {
            // Cycle through offsets which make a 137 byte sequence.
            // Because 137 is prime this sequence should exercise all corner cases.
            var offsets = new nint[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 1}.array();
            foreach (var (_, vᴛ2) in offsets.ΔRangeSnapshot()) {
                var j = vᴛ2;

                {
                    nint v = len(buf) - i; if (v < j) {
                        j = v;
                    }
                }
                d.Write(buf.slice(i, i + j));
                i += j;
            }
        }
        d.Read(got);
        if (!bytes.Equal(got, want)) {
            Ꮡt.Errorf("Unaligned writes, alg=%s\ngot %q, want %q"u8, alg, got, want);
        }
    }
}

// TestAppend checks that appending works when reallocation is necessary.
public static void TestAppend(ж<testing.T> Ꮡt) {
    cryptotest.TestAllImplementations(Ꮡt, sha3ˢ, testAppend);
}

internal static void testAppend(ж<testing.T> Ꮡt) {
    var d = New224();
    for (nint capacity = 2; capacity <= 66; capacity += 64) {
        // The first time around the loop, Sum will have to reallocate.
        // The second time, it will not.
        var buf = new slice<byte>(2, capacity);
        d.Reset();
        d.Write(new byte[]{0xcc}.slice());
        buf = d.Sum(buf);
        @string expected = "0000DF70ADC49B2E76EEE3A6931B93FA41841C3AF2CDF5B32A18B5478C39"u8;
        {
            @string got = strings.ToUpper(hex.EncodeToString(buf)); if (got != expected) {
                Ꮡt.Errorf("got %s, want %s"u8, got, expected);
            }
        }
    }
}

// TestAppendNoRealloc tests that appending works when no reallocation is necessary.
public static void TestAppendNoRealloc(ж<testing.T> Ꮡt) {
    cryptotest.TestAllImplementations(Ꮡt, sha3ˢ, testAppendNoRealloc);
}

internal static void testAppendNoRealloc(ж<testing.T> Ꮡt) {
    var buf = new slice<byte>(1, 200);
    var d = New224();
    d.Write(new byte[]{0xcc}.slice());
    buf = d.Sum(buf);
    @string expected = "00DF70ADC49B2E76EEE3A6931B93FA41841C3AF2CDF5B32A18B5478C39"u8;
    {
        @string got = strings.ToUpper(hex.EncodeToString(buf)); if (got != expected) {
            Ꮡt.Errorf("got %s, want %s"u8, got, expected);
        }
    }
}

// TestSqueezing checks that squeezing the full output a single time produces
// the same output as repeatedly squeezing the instance.
public static void TestSqueezing(ж<testing.T> Ꮡt) {
    cryptotest.TestAllImplementations(Ꮡt, sha3ˢ, testSqueezing);
}

internal static void testSqueezing(ж<testing.T> Ꮡt) {
    foreach (var (algo, v) in testShakes) {
        var d0 = v.constructor(slice<byte>(v.defAlgoName), slice<byte>(v.defCustomStr));
        d0.Write(slice<byte>(testString));
        var @ref = new slice<byte>(32);
        d0.Read(@ref);
        var d1 = v.constructor(slice<byte>(v.defAlgoName), slice<byte>(v.defCustomStr));
        d1.Write(slice<byte>(testString));
        slice<byte> multiple = default!;
        foreach ((_, _) in @ref) {
            d1.Read(new slice<byte>(0));
            var one = new slice<byte>(1);
            d1.Read(one);
            multiple = appendꓸꓸꓸ(multiple, one);
        }
        if (!bytes.Equal(@ref, multiple)) {
            Ꮡt.Errorf("%s: squeezing %d bytes one at a time failed"u8, algo, len(@ref));
        }
    }
}

// sequentialBytes produces a buffer of size consecutive bytes 0x00, 0x01, ..., used for testing.
//
// The alignment of each slice is intentionally randomized to detect alignment
// issues in the implementation. See https://golang.org/issue/37644.
// Ideally, the compiler should fuzz the alignment itself.
// (See https://golang.org/issue/35128.)
internal static slice<byte> sequentialBytes(nint size) {
    nint alignmentOffset = rand.Intn(8);
    var result = new slice<byte>(size + alignmentOffset).slice(alignmentOffset);
    foreach (var (i, _) in result) {
        result[i] = (byte)i;
    }
    return result;
}

public static void TestReset(ж<testing.T> Ꮡt) {
    cryptotest.TestAllImplementations(Ꮡt, sha3ˢ, testReset);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object expectedˢ = (@string)"\nExpected:\n"u8;
private static readonly object gotˢ = (@string)"\ngot:\n"u8;

internal static void testReset(ж<testing.T> Ꮡt) {
    var out1 = new slice<byte>(32);
    var out2 = new slice<byte>(32);
    foreach (var (_, v) in testShakes) {
        // Calculate hash for the first time
        var c = v.constructor(default!, new byte[]{0x99, 0x98}.slice());
        c.Write(sequentialBytes(0x100));
        c.Read(out1);
        // Calculate hash again
        c.Reset();
        c.Write(sequentialBytes(0x100));
        c.Read(out2);
        if (!bytes.Equal(out1, out2)) {
            Ꮡt.Error(expectedˢ, out1, gotˢ, out2);
        }
    }
}

internal static byte sinkSHA3;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string newˢ = "New"u8;
private static readonly @string newSHAKEˢ = "NewSHAKE"u8;
private static readonly @string sumˢ = "Sum"u8;
private static readonly @string sumSHAKEˢ = "SumSHAKE"u8;

public static void TestAllocations(ж<testing.T> Ꮡt) {
    cryptotest.SkipTestAllocations(Ꮡt);
    Ꮡt.Run(newˢ, (ж<testing.T> tΔ1) => {
        {
            var allocs = testing.AllocsPerRun(10, () => {
                var h = New256();
                var b = slice<byte>("ABC"u8);
                h.Write(b);
                var @out = new slice<byte>(0, 32);
                @out = h.Sum(@out);
                sinkSHA3 ^= (byte)(@out[0]);
            }); if (allocs > 0D) {
                tΔ1.Errorf("expected zero allocations, got %0.1f"u8, allocs);
            }
        }
    });
    Ꮡt.Run(newSHAKEˢ, (ж<testing.T> tΔ2) => {
        {
            var allocs = testing.AllocsPerRun(10, () => {
                var h = NewSHAKE128();
                var b = slice<byte>("ABC"u8);
                h.Write(b);
                var @out = new slice<byte>(32);
                h.Read(@out);
                sinkSHA3 ^= (byte)(@out[0]);
            }); if (allocs > 0D) {
                tΔ2.Errorf("expected zero allocations, got %0.1f"u8, allocs);
            }
        }
    });
    Ꮡt.Run(sumˢ, (ж<testing.T> tΔ3) => {
        {
            var allocs = testing.AllocsPerRun(10, () => {
                var b = slice<byte>("ABC"u8);
                var @out = Sum256(b);
                sinkSHA3 ^= (byte)(@out[0]);
            }); if (allocs > 0D) {
                tΔ3.Errorf("expected zero allocations, got %0.1f"u8, allocs);
            }
        }
    });
    Ꮡt.Run(sumSHAKEˢ, (ж<testing.T> tΔ4) => {
        {
            var allocs = testing.AllocsPerRun(10, () => {
                var b = slice<byte>("ABC"u8);
                var @out = SumSHAKE128(b, 10);
                sinkSHA3 ^= (byte)(@out[0]);
            }); if (allocs > 0D) {
                tΔ4.Errorf("expected zero allocations, got %0.1f"u8, allocs);
            }
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string cSHAKE128ˢ = "cSHAKE128"u8;
private static readonly @string cSHAKE256ˢ = "cSHAKE256"u8;

public static void TestCSHAKEAccumulated(ж<testing.T> Ꮡt) {
    // Generated with pycryptodome@3.20.0
    //
    //    from Crypto.Hash import cSHAKE128
    //    rng = cSHAKE128.new()
    //    acc = cSHAKE128.new()
    //    for n in range(200):
    //        N = rng.read(n)
    //        for s in range(200):
    //            S = rng.read(s)
    //            c = cSHAKE128.cSHAKE_XOF(data=None, custom=S, capacity=256, function=N)
    //            c.update(rng.read(100))
    //            acc.update(c.read(200))
    //            c = cSHAKE128.cSHAKE_XOF(data=None, custom=S, capacity=256, function=N)
    //            c.update(rng.read(168))
    //            acc.update(c.read(200))
    //            c = cSHAKE128.cSHAKE_XOF(data=None, custom=S, capacity=256, function=N)
    //            c.update(rng.read(200))
    //            acc.update(c.read(200))
    //    print(acc.read(32).hex())
    //
    // and with @noble/hashes@v1.5.0
    //
    //    import { bytesToHex } from "@noble/hashes/utils";
    //    import { cshake128 } from "@noble/hashes/sha3-addons";
    //    const rng = cshake128.create();
    //    const acc = cshake128.create();
    //    for (let n = 0; n < 200; n++) {
    //        const N = rng.xof(n);
    //        for (let s = 0; s < 200; s++) {
    //            const S = rng.xof(s);
    //            let c = cshake128.create({ NISTfn: N, personalization: S });
    //            c.update(rng.xof(100));
    //            acc.update(c.xof(200));
    //            c = cshake128.create({ NISTfn: N, personalization: S });
    //            c.update(rng.xof(168));
    //            acc.update(c.xof(200));
    //            c = cshake128.create({ NISTfn: N, personalization: S });
    //            c.update(rng.xof(200));
    //            acc.update(c.xof(200));
    //        }
    //    }
    //    console.log(bytesToHex(acc.xof(32)));
    //
    cryptotest.TestAllImplementations(Ꮡt, sha3ˢ, (ж<testing.T> tΔ1) => {
        tΔ1.Run(cSHAKE128ˢ, (ж<testing.T> tΔ2) => {
            testCSHAKEAccumulated(tΔ2, NewCSHAKE128, (1600 - 256) / 8,
                "bb14f8657c6ec5403d0b0e2ef3d3393497e9d3b1a9a9e8e6c81dbaa5fd809252"u8);
        });
        tΔ1.Run(cSHAKE256ˢ, (ж<testing.T> tΔ3) => {
            testCSHAKEAccumulated(tΔ3, NewCSHAKE256, (1600 - 512) / 8,
                "0baaf9250c6e25f0c14ea5c7f9bfde54c8a922c8276437db28f3895bdf6eeeef"u8);
        });
    });
}

internal static void testCSHAKEAccumulated(ж<testing.T> Ꮡt, Func<slice<byte>, slice<byte>, ж<sha3.SHAKE>> newCSHAKE, int64 rate, @string exp) {
    var rnd = newCSHAKE(default!, default!);
    var acc = newCSHAKE(default!, default!);
    for (nint n = 0; n < 200; n++) {
        var N = new slice<byte>(n);
        rnd.Read(N);
        for (nint s = 0; s < 200; s++) {
            var S = new slice<byte>(s);
            rnd.Read(S);
            var c = newCSHAKE(N, S);
            io.CopyN(new sha3.SHAKEжWriter(c), new sha3.SHAKEжReader(rnd), 100);
            /* < rate */
            io.CopyN(new sha3.SHAKEжWriter(acc), new sha3.SHAKEжReader(c), 200);
            c.Reset();
            io.CopyN(new sha3.SHAKEжWriter(c), new sha3.SHAKEжReader(rnd), rate);
            io.CopyN(new sha3.SHAKEжWriter(acc), new sha3.SHAKEжReader(c), 200);
            c.Reset();
            io.CopyN(new sha3.SHAKEжWriter(c), new sha3.SHAKEжReader(rnd), 200);
            /* > rate */
            io.CopyN(new sha3.SHAKEжWriter(acc), new sha3.SHAKEжReader(c), 200);
        }
    }
    var @out = new slice<byte>(32);
    acc.Read(@out);
    {
        @string got = hex.EncodeToString(@out); if (got != exp) {
            Ꮡt.Errorf("got %s, want %s"u8, got, exp);
        }
    }
}

public static void TestCSHAKELargeS(ж<testing.T> Ꮡt) {
    cryptotest.TestAllImplementations(Ꮡt, sha3ˢ, testCSHAKELargeS);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object skippingTestInShortModeˢ = (@string)"skipping test in short mode."u8;

internal static void testCSHAKELargeS(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingTestInShortModeˢ);
    }
    // See https://go.dev/issue/66232.
    const nint s = /* (1<<32)/8 + 1000 */ 536871912; // s * 8 > 2^32
    var S = new slice<byte>(s);
    var rnd = NewSHAKE128();
    rnd.Read(S);
    var c = NewCSHAKE128(default!, S);
    io.CopyN(new sha3.SHAKEжWriter(c), new sha3.SHAKEжReader(rnd), 1000);
    var @out = new slice<byte>(32);
    c.Read(@out);
    // Generated with pycryptodome@3.20.0
    //
    //    from Crypto.Hash import cSHAKE128
    //    rng = cSHAKE128.new()
    //    S = rng.read(536871912)
    //    c = cSHAKE128.new(custom=S)
    //    c.update(rng.read(1000))
    //    print(c.read(32).hex())
    //
    @string exp = "2cb9f237767e98f2614b8779cf096a52da9b3a849280bbddec820771ae529cf0"u8;
    {
        @string got = hex.EncodeToString(@out); if (got != exp) {
            Ꮡt.Errorf("got %s, want %s"u8, got, exp);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string sha3224ˢ = "SHA3-224"u8;
private static readonly @string sha3256ˢ = "SHA3-256"u8;
private static readonly @string sha3384ˢ = "SHA3-384"u8;
private static readonly @string sha3512ˢ = "SHA3-512"u8;
private static readonly @string shake128ˢ = "SHAKE128"u8;
private static readonly @string shake256ˢ = "SHAKE256"u8;

public static void TestMarshalUnmarshal(ж<testing.T> Ꮡt) {
    cryptotest.TestAllImplementations(Ꮡt, sha3ˢ, (ж<testing.T> tΔ1) => {
        tΔ1.Run(sha3224ˢ, (ж<testing.T> tΔ2) => {
            testMarshalUnmarshal(tΔ2, New224());
        });
        tΔ1.Run(sha3256ˢ, (ж<testing.T> tΔ3) => {
            testMarshalUnmarshal(tΔ3, New256());
        });
        tΔ1.Run(sha3384ˢ, (ж<testing.T> tΔ4) => {
            testMarshalUnmarshal(tΔ4, New384());
        });
        tΔ1.Run(sha3512ˢ, (ж<testing.T> tΔ5) => {
            testMarshalUnmarshal(tΔ5, New512());
        });
        tΔ1.Run(shake128ˢ, (ж<testing.T> tΔ6) => {
            testMarshalUnmarshalSHAKE(tΔ6, NewSHAKE128());
        });
        tΔ1.Run(shake256ˢ, (ж<testing.T> tΔ7) => {
            testMarshalUnmarshalSHAKE(tΔ7, NewSHAKE256());
        });
        tΔ1.Run(cSHAKE128ˢ, (ж<testing.T> tΔ8) => {
            testMarshalUnmarshalSHAKE(tΔ8, NewCSHAKE128(slice<byte>("N"u8), slice<byte>("S"u8)));
        });
        tΔ1.Run(cSHAKE256ˢ, (ж<testing.T> tΔ9) => {
            testMarshalUnmarshalSHAKE(tΔ9, NewCSHAKE256(slice<byte>("N"u8), slice<byte>("S"u8)));
        });
    });
}

// TODO(filippo): move this to crypto/internal/cryptotest.
internal static void testMarshalUnmarshal(ж<testing.T> Ꮡt, ж<sha3.SHA3> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    var buf = new slice<byte>(200);
    rand.Read(buf);
    nint n = rand.Intn(200);
    Ꮡh.Write(buf);
    var want = h.Sum(default!);
    h.Reset();
    Ꮡh.Write(buf.slice(0, n));
    var (b, err) = h.MarshalBinary();
    if (err != default!) {
        Ꮡt.Errorf("MarshalBinary: %v"u8, err);
    }
    Ꮡh.Write(bytes.Repeat(new byte[]{0}.slice(), 200));
    {
        var errΔ1 = h.UnmarshalBinary(b); if (errΔ1 != default!) {
            Ꮡt.Errorf("UnmarshalBinary: %v"u8, errΔ1);
        }
    }
    Ꮡh.Write(buf.slice(n));
    var got = h.Sum(default!);
    if (!bytes.Equal(got, want)) {
        Ꮡt.Errorf("got %x, want %x"u8, got, want);
    }
}

// TODO(filippo): move this to crypto/internal/cryptotest.
internal static void testMarshalUnmarshalSHAKE(ж<testing.T> Ꮡt, ж<sha3.SHAKE> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    var buf = new slice<byte>(200);
    rand.Read(buf);
    nint n = rand.Intn(200);
    Ꮡh.Write(buf);
    var want = new slice<byte>(32);
    Ꮡh.Read(want);
    Ꮡh.Reset();
    Ꮡh.Write(buf.slice(0, n));
    var (b, err) = h.MarshalBinary();
    if (err != default!) {
        Ꮡt.Errorf("MarshalBinary: %v"u8, err);
    }
    Ꮡh.Write(bytes.Repeat(new byte[]{0}.slice(), 200));
    {
        var errΔ1 = h.UnmarshalBinary(b); if (errΔ1 != default!) {
            Ꮡt.Errorf("UnmarshalBinary: %v"u8, errΔ1);
        }
    }
    Ꮡh.Write(buf.slice(n));
    var got = new slice<byte>(32);
    Ꮡh.Read(got);
    if (!bytes.Equal(got, want)) {
        Ꮡt.Errorf("got %x, want %x"u8, got, want);
    }
}

// benchmarkHash tests the speed to hash num buffers of buflen each.
internal static void benchmarkHash(ж<testing.B> Ꮡb, fips140.Hash h, nint size, nint num) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    h.Reset();
    var data = sequentialBytes(size);
    b.SetBytes((int64)(size * num));
    b.StartTimer();
    slice<byte> state = default!;
    for (nint i = 0; i < b.N; i++) {
        for (nint j = 0; j < num; j++) {
            h.Write(data);
        }
        state = h.Sum(state[..0]);
    }
    b.StopTimer();
    h.Reset();
}

// benchmarkShake is specialized to the Shake instances, which don't
// require a copy on reading output.
internal static void benchmarkShake(ж<testing.B> Ꮡb, ж<sha3.SHAKE> Ꮡh, nint size, nint num) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    Ꮡh.Reset();
    var data = sequentialBytes(size);
    var d = new slice<byte>(32);
    b.SetBytes((int64)(size * num));
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        Ꮡh.Reset();
        for (nint j = 0; j < num; j++) {
            Ꮡh.Write(data);
        }
        Ꮡh.Read(d);
    }
}

public static void BenchmarkSha3_512_MTU(ж<testing.B> Ꮡb) {
    benchmarkHash(Ꮡb, new sha3.SHA3жfips140_Hash(New512()), 1350, 1);
}

public static void BenchmarkSha3_384_MTU(ж<testing.B> Ꮡb) {
    benchmarkHash(Ꮡb, new sha3.SHA3жfips140_Hash(New384()), 1350, 1);
}

public static void BenchmarkSha3_256_MTU(ж<testing.B> Ꮡb) {
    benchmarkHash(Ꮡb, new sha3.SHA3жfips140_Hash(New256()), 1350, 1);
}

public static void BenchmarkSha3_224_MTU(ж<testing.B> Ꮡb) {
    benchmarkHash(Ꮡb, new sha3.SHA3жfips140_Hash(New224()), 1350, 1);
}

public static void BenchmarkShake128_MTU(ж<testing.B> Ꮡb) {
    benchmarkShake(Ꮡb, NewSHAKE128(), 1350, 1);
}

public static void BenchmarkShake256_MTU(ж<testing.B> Ꮡb) {
    benchmarkShake(Ꮡb, NewSHAKE256(), 1350, 1);
}

public static void BenchmarkShake256_16x(ж<testing.B> Ꮡb) {
    benchmarkShake(Ꮡb, NewSHAKE256(), 16, 1024);
}

public static void BenchmarkShake256_1MiB(ж<testing.B> Ꮡb) {
    benchmarkShake(Ꮡb, NewSHAKE256(), 1024, 1024);
}

public static void BenchmarkSha3_512_1MiB(ж<testing.B> Ꮡb) {
    benchmarkHash(Ꮡb, new sha3.SHA3жfips140_Hash(New512()), 1024, 1024);
}

} // end sha3_test_package
