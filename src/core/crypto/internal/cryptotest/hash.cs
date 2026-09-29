// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using bytes = bytes_package;
using hash = hash_package;
using io = io_package;
using rand = math.rand_package;
using testing = testing_package;
using time = time_package;
using math;

partial class cryptotest_package {

// type MakeHash is a methodless func type — rendered inline as its base delegate

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string sumAppendˢ = "SumAppend"u8;
private static readonly @string writeWithoutErrorˢ = "WriteWithoutError"u8;
private static readonly @string resetStateˢ = "ResetState"u8;
private static readonly @string statefulWriteˢ = "StatefulWrite"u8;

// TestHash performs a set of tests on hash.Hash implementations, checking the
// documented requirements of Write, Sum, Reset, Size, and BlockSize.
public static void TestHash(ж<testing.T> Ꮡt, Func<hash.Hash> mh) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Test that Sum returns an appended digest matching output of Size
    Ꮡt.Run(sumAppendˢ, (ж<testing.T> tΔ1) => {
        var h = mh();
        var rng = newRandReader(tΔ1);
        var emptyBuff = slice<byte>(""u8);
        var shortBuff = slice<byte>("a"u8);
        var longBuff = new slice<byte>(h.BlockSize() + 1);
        rng.Read(longBuff);
        // Set of example strings to append digest to
        var prefixes = new slice<byte>[]{default!, emptyBuff, shortBuff, longBuff}.slice();
        // Go to each string and check digest gets appended to and is correct size.
        foreach (var (_, prefix) in prefixes) {
            h.Reset();
            var sum = getSum(tΔ1, h, prefix); // Append new digest to prefix
            // Check that Sum didn't alter the prefix
            if (!bytes.Equal(sum.slice(0, len(prefix)), prefix)) {
                tΔ1.Errorf("Sum alters passed buffer instead of appending; got %x, want %x"u8, sum.slice(0, len(prefix)), prefix);
            }
            // Check that the appended sum wasn't affected by the prefix
            {
                var expectedSum = getSum(tΔ1, h, default!); if (!bytes.Equal(sum.slice(len(prefix)), expectedSum)) {
                    tΔ1.Errorf("Sum behavior affected by data in the input buffer; got %x, want %x"u8, sum.slice(len(prefix)), expectedSum);
                }
            }
            // Check size of append
            {
                nint got = len(sum) - len(prefix);
                nint want = h.Size(); if (got != want) {
                    tΔ1.Errorf("Sum appends number of bytes != Size; got %v , want %v"u8, got, want);
                }
            }
        }
    });
    // Test that Hash.Write never returns error.
    Ꮡt.Run(writeWithoutErrorˢ, (ж<testing.T> tΔ2) => {
        var h = mh();
        var rng = newRandReader(tΔ2);
        var emptySlice = slice<byte>(""u8);
        var shortSlice = slice<byte>("a"u8);
        var longSlice = new slice<byte>(h.BlockSize() + 1);
        rng.Read(longSlice);
        // Set of example strings to append digest to
        var slices = new slice<byte>[]{emptySlice, shortSlice, longSlice}.slice();
        foreach (var (_, Δslice) in slices) {
            writeToHash(tΔ2, h, Δslice); // Writes and checks Write doesn't error
        }
    });
    Ꮡt.Run(resetStateˢ, (ж<testing.T> tΔ3) => {
        var h = mh();
        var rng = newRandReader(tΔ3);
        var emptySum = getSum(tΔ3, h, default!);
        // Write to hash and then Reset it and see if Sum is same as emptySum
        var writeEx = new slice<byte>(h.BlockSize());
        rng.Read(writeEx);
        writeToHash(tΔ3, h, writeEx);
        h.Reset();
        var resetSum = getSum(tΔ3, h, default!);
        if (!bytes.Equal(emptySum, resetSum)) {
            tΔ3.Errorf("Reset hash yields different Sum than new hash; got %x, want %x"u8, emptySum, resetSum);
        }
    });
    // Check that Write isn't reading from beyond input slice's bounds
    Ꮡt.Run(outOfBoundsReadˢ, (ж<testing.T> tΔ4) => {
        var h = mh();
        nint blockSize = h.BlockSize();
        var rng = newRandReader(tΔ4);
        var msg = new slice<byte>(blockSize);
        rng.Read(msg);
        writeToHash(tΔ4, h, msg);
        var expectedDigest = getSum(tΔ4, h, default!); // Record control digest
        h.Reset();
        // Make a buffer with msg in the middle and data on either end
        var buff = new slice<byte>(blockSize * 3);
        nint endOfPrefix = blockSize;
        nint startOfSuffix = blockSize * 2;
        copy(buff.slice(endOfPrefix, startOfSuffix), msg);
        rng.Read(buff.slice(0, endOfPrefix));
        rng.Read(buff.slice(startOfSuffix));
        writeToHash(tΔ4, h, buff.slice(endOfPrefix, startOfSuffix));
        var testDigest = getSum(tΔ4, h, default!);
        if (!bytes.Equal(testDigest, expectedDigest)) {
            tΔ4.Errorf("Write affected by data outside of input slice bounds; got %x, want %x"u8, testDigest, expectedDigest);
        }
    });
    // Test that multiple calls to Write is stateful
    Ꮡt.Run(statefulWriteˢ, (ж<testing.T> tΔ5) => {
        var h = mh();
        var rng = newRandReader(tΔ5);
        var (prefix, suffix) = (new slice<byte>(h.BlockSize()), new slice<byte>(h.BlockSize()));
        rng.Read(prefix);
        rng.Read(suffix);
        // Write prefix then suffix sequentially and record resulting hash
        writeToHash(tΔ5, h, prefix);
        writeToHash(tΔ5, h, suffix);
        var serialSum = getSum(tΔ5, h, default!);
        h.Reset();
        // Write prefix and suffix at the same time and record resulting hash
        writeToHash(tΔ5, h, appendꓸꓸꓸ(prefix, suffix));
        var compositeSum = getSum(tΔ5, h, default!);
        // Check that sequential writing results in the same as writing all at once
        if (!bytes.Equal(compositeSum, serialSum)) {
            tΔ5.Errorf("two successive Write calls resulted in a different Sum than a single one; got %x, want %x"u8, compositeSum, serialSum);
        }
    });
}

// Helper function for writing. Verifies that Write does not error.
internal static void writeToHash(ж<testing.T> Ꮡt, hash.Hash h, slice<byte> p) {
    Ꮡt.Helper();
    var before = new slice<byte>(len(p));
    copy(before, p);
    var (n, err) = h.Write(p);
    if (err != default! || n != len(p)) {
        Ꮡt.Errorf("Write returned error; got (%v, %v), want (nil, %v)"u8, err, n, len(p));
    }
    if (!bytes.Equal(p, before)) {
        Ꮡt.Errorf("Write modified input slice; got %x, want %x"u8, p, before);
    }
}

// Helper function for getting Sum. Checks that Sum doesn't change hash state.
internal static slice<byte> getSum(ж<testing.T> Ꮡt, hash.Hash h, slice<byte> buff) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Helper();
    var testBuff = new slice<byte>(len(buff));
    copy(testBuff, buff);
    var sum = h.Sum(buff);
    var testSum = h.Sum(testBuff);
    // Check that Sum doesn't change underlying hash state
    if (!bytes.Equal(sum, testSum)) {
        Ꮡt.Errorf("successive calls to Sum yield different results; got %x, want %x"u8, sum, testSum);
    }
    return sum;
}

internal static io.Reader newRandReader(ж<testing.T> Ꮡt) {
    var seed = time.Now().UnixNano();
    Ꮡt.Logf("Deterministic RNG seed: 0x%x"u8, seed);
    return new rand_RandжReader(rand.New(rand.NewSource(seed)));
}

} // end cryptotest_package
