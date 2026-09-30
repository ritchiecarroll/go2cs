// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using bytes = bytes_package;
using cipher = go.crypto.cipher_package;
using testing = testing_package;
using go.crypto;
using io = io_package;

partial class cryptotest_package {

// type MakeBlockMode is a methodless func type — rendered inline as its base delegate

// TestBlockMode performs a set of tests on cipher.BlockMode implementations,
// checking the documented requirements of CryptBlocks.
public static void TestBlockMode(ж<testing.T> Ꮡt, cipher.Block block, Func<cipher.Block, slice<byte>, cipher.BlockMode> makeEncrypter, Func<cipher.Block, slice<byte>, cipher.BlockMode> makeDecrypter) {
    var rng = newRandReader(Ꮡt);
    var iv = new slice<byte>(block.BlockSize());
    rng.Read(iv);
    testBlockModePair(Ꮡt, block, makeEncrypter, makeDecrypter, iv);
}

internal static void testBlockModePair(ж<testing.T> Ꮡt, cipher.Block b, Func<cipher.Block, slice<byte>, cipher.BlockMode> enc, Func<cipher.Block, slice<byte>, cipher.BlockMode> dec, slice<byte> iv) {
    var ivʗ1 = iv;
    Ꮡt.Run(encryptionˢ, (ж<testing.T> tΔ1) => {
        testBlockMode(tΔ1, enc, b, ivʗ1);
    });
    var ivʗ2 = iv;
    Ꮡt.Run(decryptionˢ, (ж<testing.T> tΔ2) => {
        testBlockMode(tΔ2, dec, b, ivʗ2);
    });
    var ivʗ3 = iv;
    Ꮡt.Run(roundtripˢ, (ж<testing.T> tΔ3) => {
        var rng = newRandReader(tΔ3);
        nint blockSize = enc(b, ivʗ3).BlockSize();
        {
            nint decBlockSize = dec(b, ivʗ3).BlockSize(); if (decBlockSize != blockSize) {
                tΔ3.Errorf("decryption blocksize different than encryption's; got %d, want %d"u8, decBlockSize, blockSize);
            }
        }
        var (before, dst, after) = (new slice<byte>(blockSize * 2), new slice<byte>(blockSize * 2), new slice<byte>(blockSize * 2));
        rng.Read(before);
        enc(b, ivʗ3).CryptBlocks(dst, before);
        dec(b, ivʗ3).CryptBlocks(after, dst);
        if (!bytes.Equal(after, before)) {
            tΔ3.Errorf("plaintext is different after an encrypt/decrypt cycle; got %x, want %x"u8, after, before);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string wrongIVLenˢ = "WrongIVLen"u8;
private static readonly @string ivLengthMustEqualBlockˢ = "IV length must equal block size"u8;
private static readonly @string emptyInputˢ = "EmptyInput"u8;
private static readonly @string outputSmallerThanInputˢ = "output smaller than input"u8;
private static readonly @string partialBlocksˢ = "PartialBlocks"u8;
private static readonly @string inputNotFullBlocksˢ = "input not full blocks"u8;
private static readonly @string keepStateˢ = "KeepState"u8;

internal static void testBlockMode(ж<testing.T> Ꮡt, Func<cipher.Block, slice<byte>, cipher.BlockMode> bm, cipher.Block b, slice<byte> iv) {
    ref var t = ref Ꮡt.DerefOrNull();

    nint blockSize = bm(b, iv).BlockSize();
    Ꮡt.Run(wrongIVLenˢ, (ж<testing.T> tΔ1) => {
        var ivΔ1 = new slice<byte>(b.BlockSize() + 1);
        var ivʗ1 = ivΔ1;
        mustPanic(tΔ1, ivLengthMustEqualBlockˢ, () => {
            bm(b, ivʗ1);
        });
    });
    var ivʗ2 = iv;
    Ꮡt.Run(emptyInputˢ, (ж<testing.T> tΔ2) => {
        var rng = newRandReader(tΔ2);
        var (src, dst) = (new slice<byte>(blockSize), new slice<byte>(blockSize));
        rng.Read(dst);
        var before = bytes.Clone(dst);
        bm(b, ivʗ2).CryptBlocks(dst, src[..0]);
        if (!bytes.Equal(dst, before)) {
            tΔ2.Errorf("CryptBlocks modified dst on empty input; got %x, want %x"u8, dst, before);
        }
    });
    var ivʗ3 = iv;
    Ꮡt.Run(alterInputˢ, (ж<testing.T> tΔ3) => {
        var rng = newRandReader(tΔ3);
        var (src, dst, before) = (new slice<byte>(blockSize * 2), new slice<byte>(blockSize * 2), new slice<byte>(blockSize * 2));
        foreach (var (_, length) in new nint[]{0, blockSize, blockSize * 2}.slice()) {
            rng.Read(src);
            copy(before, src);
            bm(b, ivʗ3).CryptBlocks(dst.slice(0, length), src.slice(0, length));
            if (!bytes.Equal(src, before)) {
                tΔ3.Errorf("CryptBlocks modified src; got %x, want %x"u8, src, before);
            }
        }
    });
    var ivʗ4 = iv;
    Ꮡt.Run(aliasingˢ, (ж<testing.T> tΔ4) => {
        var rng = newRandReader(tΔ4);
        var (buff, expectedOutput) = (new slice<byte>(blockSize * 2), new slice<byte>(blockSize * 2));
        foreach (var (_, length) in new nint[]{0, blockSize, blockSize * 2}.slice()) {
            // Record what output is when src and dst are different
            rng.Read(buff);
            bm(b, ivʗ4).CryptBlocks(expectedOutput.slice(0, length), buff.slice(0, length));
            // Check that the same output is generated when src=dst alias to the same
            // memory
            bm(b, ivʗ4).CryptBlocks(buff.slice(0, length), buff.slice(0, length));
            if (!bytes.Equal(buff.slice(0, length), expectedOutput.slice(0, length))) {
                tΔ4.Errorf("block cipher produced different output when dst = src; got %x, want %x"u8, buff.slice(0, length), expectedOutput.slice(0, length));
            }
        }
    });
    var ivʗ5 = iv;
    Ꮡt.Run(outOfBoundsWriteˢ, (ж<testing.T> tΔ5) => {
        // Issue 21104
        var rng = newRandReader(tΔ5);
        ref var src = ref heap<slice<byte>>(out var Ꮡsrc);
        Ꮡsrc.ValueSlot = new slice<byte>(blockSize);
        rng.Read(Ꮡsrc.ValueSlot);
        // Make a buffer with dst in the middle and data on either end
        var buff = new slice<byte>(blockSize * 3);
        nint endOfPrefix = blockSize;
        nint startOfSuffix = blockSize * 2;
        rng.Read(buff.slice(0, endOfPrefix));
        rng.Read(buff.slice(startOfSuffix));
        ref var dst = ref heap<slice<byte>>(out var Ꮡdst);
        Ꮡdst.ValueSlot = buff.slice(endOfPrefix, startOfSuffix);
        // Record the prefix and suffix data to make sure they aren't written to
        var (initPrefix, initSuffix) = (new slice<byte>(blockSize), new slice<byte>(blockSize));
        copy(initPrefix, buff.slice(0, endOfPrefix));
        copy(initSuffix, buff.slice(startOfSuffix));
        // Write to dst (the middle of the buffer) and make sure it doesn't write
        // beyond the dst slice on a valid CryptBlocks call
        bm(b, ivʗ5).CryptBlocks(Ꮡdst.ValueSlot, Ꮡsrc.ValueSlot);
        if (!bytes.Equal(buff.slice(startOfSuffix), initSuffix)) {
            tΔ5.Errorf("block cipher did out of bounds write after end of dst slice; got %x, want %x"u8, buff.slice(startOfSuffix), initSuffix);
        }
        if (!bytes.Equal(buff.slice(0, endOfPrefix), initPrefix)) {
            tΔ5.Errorf("block cipher did out of bounds write before beginning of dst slice; got %x, want %x"u8, buff.slice(0, endOfPrefix), initPrefix);
        }
        // Check that dst isn't written to beyond len(src) even if there is room in
        // the slice
        Ꮡdst.ValueSlot = buff.slice(endOfPrefix); // Extend dst to include suffix
        bm(b, ivʗ5).CryptBlocks(Ꮡdst.ValueSlot, Ꮡsrc.ValueSlot);
        if (!bytes.Equal(buff.slice(startOfSuffix), initSuffix)) {
            tΔ5.Errorf("CryptBlocks modified dst past len(src); got %x, want %x"u8, buff.slice(startOfSuffix), initSuffix);
        }
        // Issue 21104: Shouldn't write to anything outside of dst even if src is bigger
        Ꮡsrc.ValueSlot = new slice<byte>(blockSize * 3);
        rng.Read(Ꮡsrc.ValueSlot);
        var ivʗ6 = ivʗ5;
        mustPanic(tΔ5, outputSmallerThanInputˢ, () => {
            bm(b, ivʗ6).CryptBlocks(Ꮡdst.ValueSlot, Ꮡsrc.ValueSlot);
        });
        if (!bytes.Equal(buff.slice(startOfSuffix), initSuffix)) {
            tΔ5.Errorf("block cipher did out of bounds write after end of dst slice; got %x, want %x"u8, buff.slice(startOfSuffix), initSuffix);
        }
        if (!bytes.Equal(buff.slice(0, endOfPrefix), initPrefix)) {
            tΔ5.Errorf("block cipher did out of bounds write before beginning of dst slice; got %x, want %x"u8, buff.slice(0, endOfPrefix), initPrefix);
        }
    });
    // Check that output of cipher isn't affected by adjacent data beyond input
    // slice scope
    var ivʗ7 = iv;
    Ꮡt.Run(outOfBoundsReadˢ, (ж<testing.T> tΔ6) => {
        var rng = newRandReader(tΔ6);
        var src = new slice<byte>(blockSize);
        rng.Read(src);
        var expectedDst = new slice<byte>(blockSize);
        bm(b, ivʗ7).CryptBlocks(expectedDst, src);
        // Make a buffer with src in the middle and data on either end
        var buff = new slice<byte>(blockSize * 3);
        nint endOfPrefix = blockSize;
        nint startOfSuffix = blockSize * 2;
        copy(buff.slice(endOfPrefix, startOfSuffix), src);
        rng.Read(buff.slice(0, endOfPrefix));
        rng.Read(buff.slice(startOfSuffix));
        var testDst = new slice<byte>(blockSize);
        bm(b, ivʗ7).CryptBlocks(testDst, buff.slice(endOfPrefix, startOfSuffix));
        if (!bytes.Equal(testDst, expectedDst)) {
            tΔ6.Errorf("CryptBlocks affected by data outside of src slice bounds; got %x, want %x"u8, testDst, expectedDst);
        }
    });
    var ivʗ8 = iv;
    Ꮡt.Run(bufferOverlapˢ, (ж<testing.T> tΔ7) => {
        var rng = newRandReader(tΔ7);
        var buff = new slice<byte>(blockSize * 2);
        rng.Read(buff);
        // Make src and dst slices point to same array with inexact overlap
        ref var src = ref heap<slice<byte>>(out var Ꮡsrc);
        Ꮡsrc.ValueSlot = buff.slice(0, blockSize);
        ref var dst = ref heap<slice<byte>>(out var Ꮡdst);
        Ꮡdst.ValueSlot = buff.slice(1, blockSize + 1);
        var ivʗ9 = ivʗ8;
        mustPanic(tΔ7, invalidBufferOverlapˢ, () => {
            bm(b, ivʗ9).CryptBlocks(Ꮡdst.ValueSlot, Ꮡsrc.ValueSlot);
        });
        // Only overlap on one byte
        Ꮡsrc.ValueSlot = buff.slice(0, blockSize);
        Ꮡdst.ValueSlot = buff.slice(blockSize - 1, 2 * blockSize - 1);
        var ivʗ10 = ivʗ8;
        mustPanic(tΔ7, invalidBufferOverlapˢ, () => {
            bm(b, ivʗ10).CryptBlocks(Ꮡdst.ValueSlot, Ꮡsrc.ValueSlot);
        });
        // src comes after dst with one byte overlap
        Ꮡsrc.ValueSlot = buff.slice(blockSize - 1, 2 * blockSize - 1);
        Ꮡdst.ValueSlot = buff.slice(0, blockSize);
        var ivʗ11 = ivʗ8;
        mustPanic(tΔ7, invalidBufferOverlapˢ, () => {
            bm(b, ivʗ11).CryptBlocks(Ꮡdst.ValueSlot, Ꮡsrc.ValueSlot);
        });
    });
    // Input to CryptBlocks should be a multiple of BlockSize
    var ivʗ12 = iv;
    Ꮡt.Run(partialBlocksˢ, (ж<testing.T> tΔ8) => {
        // Check a few cases of not being a multiple of BlockSize
        foreach (var (_, srcSize) in new nint[]{blockSize - 1, blockSize + 1, 2 * blockSize - 1, 2 * blockSize + 1}.slice()) {
            var src = new slice<byte>(srcSize);
            var dst = new slice<byte>(3 * blockSize); // Make a dst large enough for all src
            var dstʗ1 = dst;
            var ivʗ13 = ivʗ12;
            var srcʗ1 = src;
            mustPanic(tΔ8, inputNotFullBlocksˢ, () => {
                bm(b, ivʗ13).CryptBlocks(dstʗ1, srcʗ1);
            });
        }
    });
    var ivʗ14 = iv;
    Ꮡt.Run(keepStateˢ, (ж<testing.T> tΔ9) => {
        var rng = newRandReader(tΔ9);
        var (src, serialDst, compositeDst) = (new slice<byte>(blockSize * 4), new slice<byte>(blockSize * 4), new slice<byte>(blockSize * 4));
        rng.Read(src);
        nint length = 2 * blockSize;
        var block = bm(b, ivʗ14);
        block.CryptBlocks(serialDst, src.slice(0, length));
        block.CryptBlocks(serialDst.slice(length), src.slice(length));
        bm(b, ivʗ14).CryptBlocks(compositeDst, src);
        if (!bytes.Equal(serialDst, compositeDst)) {
            tΔ9.Errorf("two successive CryptBlocks calls returned a different result than a single one; got %x, want %x"u8, serialDst, compositeDst);
        }
    });
}

} // end cryptotest_package
