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

// type MakeBlock is a methodless func type — rendered inline as its base delegate

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string encryptionˢ = "Encryption"u8;
private static readonly @string decryptionˢ = "Decryption"u8;

// TestBlock performs a set of tests on cipher.Block implementations, checking
// the documented requirements of BlockSize, Encrypt, and Decrypt.
public static void TestBlock(ж<testing.T> Ꮡt, nint keySize, Func<slice<byte>, (cipher.Block, error)> mb) {
    // Generate random key
    var key = new slice<byte>(keySize);
    newRandReader(Ꮡt).Read(key);
    Ꮡt.Logf("Cipher key: 0x%x"u8, key);
    var (block, err) = mb(key);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    nint blockSize = block.BlockSize();
    var blockʗ1 = block;
    Ꮡt.Run(encryptionˢ, (ж<testing.T> tΔ1) => {
        testCipher(tΔ1, blockʗ1.Encrypt, blockSize);
    });
    var blockʗ2 = block;
    Ꮡt.Run(decryptionˢ, (ж<testing.T> tΔ2) => {
        testCipher(tΔ2, blockʗ2.Decrypt, blockSize);
    });
    // Checks baseline Encrypt/Decrypt functionality.  More thorough
    // implementation-specific characterization/golden tests should be done
    // for each block cipher implementation.
    var blockʗ3 = block;
    Ꮡt.Run(roundtripˢ, (ж<testing.T> tΔ3) => {
        var rng = newRandReader(tΔ3);
        // Check Decrypt inverts Encrypt
        var (before, ciphertext, after) = (new slice<byte>(blockSize), new slice<byte>(blockSize), new slice<byte>(blockSize));
        rng.Read(before);
        blockʗ3.Encrypt(ciphertext, before);
        blockʗ3.Decrypt(after, ciphertext);
        if (!bytes.Equal(after, before)) {
            tΔ3.Errorf("plaintext is different after an encrypt/decrypt cycle; got %x, want %x"u8, after, before);
        }
        // Check Encrypt inverts Decrypt (assumes block ciphers are deterministic)
        before = new slice<byte>(blockSize);
        var plaintext = new slice<byte>(blockSize);
        after = new slice<byte>(blockSize);
        rng.Read(before);
        blockʗ3.Decrypt(plaintext, before);
        blockʗ3.Encrypt(after, plaintext);
        if (!bytes.Equal(after, before)) {
            tΔ3.Errorf("ciphertext is different after a decrypt/encrypt cycle; got %x, want %x"u8, after, before);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string alterInputˢ = "AlterInput"u8;
private static readonly @string aliasingˢ = "Aliasing"u8;
private static readonly @string outOfBoundsWriteˢ = "OutOfBoundsWrite"u8;
private static readonly @string outOfBoundsReadˢ = "OutOfBoundsRead"u8;
private static readonly @string nonZeroDstˢ = "NonZeroDst"u8;
private static readonly @string shortBlockˢ = "ShortBlock"u8;
private static readonly @string inputNotFullBlockˢ = "input not full block"u8;
private static readonly @string outputNotFullBlockˢ = "output not full block"u8;

internal static void testCipher(ж<testing.T> Ꮡt, Action<slice<byte>, slice<byte>> cipher, nint blockSize) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Run(alterInputˢ, (ж<testing.T> tΔ1) => {
        var rng = newRandReader(tΔ1);
        // Make long src that shouldn't be modified at all, within block
        // size scope or beyond it
        var (src, before) = (new slice<byte>(blockSize * 2), new slice<byte>(blockSize * 2));
        rng.Read(src);
        copy(before, src);
        var dst = new slice<byte>(blockSize);
        cipher(dst, src);
        if (!bytes.Equal(src, before)) {
            tΔ1.Errorf("block cipher modified src; got %x, want %x"u8, src, before);
        }
    });
    Ꮡt.Run(aliasingˢ, (ж<testing.T> tΔ2) => {
        var rng = newRandReader(tΔ2);
        var (buff, expectedOutput) = (new slice<byte>(blockSize), new slice<byte>(blockSize));
        // Record what output is when src and dst are different
        rng.Read(buff);
        cipher(expectedOutput, buff);
        // Check that the same output is generated when src=dst alias to the same
        // memory
        cipher(buff, buff);
        if (!bytes.Equal(buff, expectedOutput)) {
            tΔ2.Errorf("block cipher produced different output when dst = src; got %x, want %x"u8, buff, expectedOutput);
        }
    });
    Ꮡt.Run(outOfBoundsWriteˢ, (ж<testing.T> tΔ3) => {
        var rng = newRandReader(tΔ3);
        var src = new slice<byte>(blockSize);
        rng.Read(src);
        // Make a buffer with dst in the middle and data on either end
        var buff = new slice<byte>(blockSize * 3);
        nint endOfPrefix = blockSize;
        nint startOfSuffix = blockSize * 2;
        rng.Read(buff.slice(0, endOfPrefix));
        rng.Read(buff.slice(startOfSuffix));
        var dst = buff.slice(endOfPrefix, startOfSuffix);
        // Record the prefix and suffix data to make sure they aren't written to
        var (initPrefix, initSuffix) = (new slice<byte>(blockSize), new slice<byte>(blockSize));
        copy(initPrefix, buff.slice(0, endOfPrefix));
        copy(initSuffix, buff.slice(startOfSuffix));
        // Write to dst (the middle of the buffer) and make sure it doesn't write
        // beyond the dst slice
        cipher(dst, src);
        if (!bytes.Equal(buff.slice(startOfSuffix), initSuffix)) {
            tΔ3.Errorf("block cipher did out of bounds write after end of dst slice; got %x, want %x"u8, buff.slice(startOfSuffix), initSuffix);
        }
        if (!bytes.Equal(buff.slice(0, endOfPrefix), initPrefix)) {
            tΔ3.Errorf("block cipher did out of bounds write before beginning of dst slice; got %x, want %x"u8, buff.slice(0, endOfPrefix), initPrefix);
        }
        // Check that dst isn't written to beyond BlockSize even if there is room
        // in the slice
        dst = buff.slice(endOfPrefix); // Extend dst to include suffix
        cipher(dst, src);
        if (!bytes.Equal(buff.slice(startOfSuffix), initSuffix)) {
            tΔ3.Errorf("block cipher modified dst past BlockSize bytes; got %x, want %x"u8, buff.slice(startOfSuffix), initSuffix);
        }
    });
    // Check that output of cipher isn't affected by adjacent data beyond input
    // slice scope
    // For encryption, this assumes block ciphers encrypt deterministically
    Ꮡt.Run(outOfBoundsReadˢ, (ж<testing.T> tΔ4) => {
        var rng = newRandReader(tΔ4);
        var src = new slice<byte>(blockSize);
        rng.Read(src);
        var expectedDst = new slice<byte>(blockSize);
        cipher(expectedDst, src);
        // Make a buffer with src in the middle and data on either end
        var buff = new slice<byte>(blockSize * 3);
        nint endOfPrefix = blockSize;
        nint startOfSuffix = blockSize * 2;
        copy(buff.slice(endOfPrefix, startOfSuffix), src);
        rng.Read(buff.slice(0, endOfPrefix));
        rng.Read(buff.slice(startOfSuffix));
        var testDst = new slice<byte>(blockSize);
        cipher(testDst, buff.slice(endOfPrefix, startOfSuffix));
        if (!bytes.Equal(testDst, expectedDst)) {
            tΔ4.Errorf("block cipher affected by data outside of src slice bounds; got %x, want %x"u8, testDst, expectedDst);
        }
        // Check that src isn't read from beyond BlockSize even if the slice is
        // longer and contains data in the suffix
        cipher(testDst, buff.slice(endOfPrefix)); // Input long src
        if (!bytes.Equal(testDst, expectedDst)) {
            tΔ4.Errorf("block cipher affected by src data beyond BlockSize bytes; got %x, want %x"u8, buff.slice(startOfSuffix), expectedDst);
        }
    });
    Ꮡt.Run(nonZeroDstˢ, (ж<testing.T> tΔ5) => {
        var rng = newRandReader(tΔ5);
        // Record what the cipher writes into a destination of zeroes
        var src = new slice<byte>(blockSize);
        rng.Read(src);
        var expectedDst = new slice<byte>(blockSize);
        cipher(expectedDst, src);
        // Make nonzero dst
        var dst = new slice<byte>(blockSize * 2);
        rng.Read(dst);
        // Remember the random suffix which shouldn't be written to
        expectedDst = appendꓸꓸꓸ(expectedDst, dst.slice(blockSize));
        cipher(dst, src);
        if (!bytes.Equal(dst, expectedDst)) {
            tΔ5.Errorf("block cipher behavior differs when given non-zero dst; got %x, want %x"u8, dst, expectedDst);
        }
    });
    Ꮡt.Run(bufferOverlapˢ, (ж<testing.T> tΔ6) => {
        var rng = newRandReader(tΔ6);
        var buff = new slice<byte>(blockSize * 2);
        rng.Read((buff));
        // Make src and dst slices point to same array with inexact overlap
        ref var src = ref heap<slice<byte>>(out var Ꮡsrc);
        Ꮡsrc.ValueSlot = buff.slice(0, blockSize);
        ref var dst = ref heap<slice<byte>>(out var Ꮡdst);
        Ꮡdst.ValueSlot = buff.slice(1, blockSize + 1);
        mustPanic(tΔ6, invalidBufferOverlapˢ, () => {
            cipher(Ꮡdst.ValueSlot, Ꮡsrc.ValueSlot);
        });
        // Only overlap on one byte
        Ꮡsrc.ValueSlot = buff.slice(0, blockSize);
        Ꮡdst.ValueSlot = buff.slice(blockSize - 1, 2 * blockSize - 1);
        mustPanic(tΔ6, invalidBufferOverlapˢ, () => {
            cipher(Ꮡdst.ValueSlot, Ꮡsrc.ValueSlot);
        });
        // src comes after dst with one byte overlap
        Ꮡsrc.ValueSlot = buff.slice(blockSize - 1, 2 * blockSize - 1);
        Ꮡdst.ValueSlot = buff.slice(0, blockSize);
        mustPanic(tΔ6, invalidBufferOverlapˢ, () => {
            cipher(Ꮡdst.ValueSlot, Ꮡsrc.ValueSlot);
        });
    });
    // Test short input/output.
    // Assembly used to not notice.
    // See issue 7928.
    Ꮡt.Run(shortBlockˢ, (ж<testing.T> tΔ7) => {
        // Returns slice of n bytes of an n+1 length array.  Lets us test that a
        // slice is still considered too short even if the underlying array it
        // points to is large enough
        slice<byte> byteSlice(nint n) => new slice<byte>(n + 1).slice(0, n);
        // Off by one byte
        var byteSliceʗ1 = byteSlice;
        mustPanic(tΔ7, inputNotFullBlockˢ, () => {
            cipher(byteSliceʗ1(blockSize), byteSliceʗ1(blockSize - 1));
        });
        var byteSliceʗ2 = byteSlice;
        mustPanic(tΔ7, outputNotFullBlockˢ, () => {
            cipher(byteSliceʗ2(blockSize - 1), byteSliceʗ2(blockSize));
        });
        // Small slices
        var byteSliceʗ3 = byteSlice;
        mustPanic(tΔ7, inputNotFullBlockˢ, () => {
            cipher(byteSliceʗ3(1), byteSliceʗ3(1));
        });
        var byteSliceʗ4 = byteSlice;
        mustPanic(tΔ7, inputNotFullBlockˢ, () => {
            cipher(byteSliceʗ4(100), byteSliceʗ4(1));
        });
        var byteSliceʗ5 = byteSlice;
        mustPanic(tΔ7, outputNotFullBlockˢ, () => {
            cipher(byteSliceʗ5(1), byteSliceʗ5(100));
        });
    });
}

internal static void mustPanic(ж<testing.T> Ꮡt, @string msg, Action f) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Helper();
        defer(() => {
            Ꮡt.Helper();
            var err = recover();
            if (err == default!) {
                Ꮡt.Errorf("function did not panic for %q"u8, msg);
            }
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end cryptotest_package
