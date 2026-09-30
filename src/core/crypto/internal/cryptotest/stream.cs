// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using bytes = bytes_package;
using cipher = go.crypto.cipher_package;
using subtle = go.crypto.subtle_package;
using fmt = fmt_package;
using strings = strings_package;
using testing = testing_package;
using go.crypto;
using io = io_package;

partial class cryptotest_package {

// Each test is executed with each of the buffer lengths in bufLens.
internal static slice<nint> bufLens = new nint[]{0, 1, 3, 4, 8, 10, 15, 16, 20, 32, 50, 4096, 5000}.slice();

internal static nint bufCap = 10000;

// type MakeStream is a methodless func type — rendered inline as its base delegate

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string xorSemanticsˢ = "XORSemantics"u8;
private static readonly @string testCFBStreamˢ = "TestCFBStream"u8;
private static readonly object cfbImplementsCipherˢ = (@string)"CFB implements cipher.Stream but does not follow XOR semantics"u8;
private static readonly @string directXORˢ = "DirectXOR"u8;

// TestStream performs a set of tests on cipher.Stream implementations,
// checking the documented requirements of XORKeyStream.
public static void TestStream(ж<testing.T> Ꮡt, Func<cipher.Stream> ms) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Run(xorSemanticsˢ, (ж<testing.T> tΔ1) => {
        if (strings.Contains(tΔ1.Name(), testCFBStreamˢ)) {
            // This is ugly, but so is CFB's abuse of cipher.Stream.
            // Don't want to make it easier for anyone else to do that.
            tΔ1.Skip(cfbImplementsCipherˢ);
        }
        // Test that XORKeyStream inverts itself for encryption/decryption.
        tΔ1.Run(roundtripˢ, (ж<testing.T> tΔ2) => {
            foreach (var (_, length) in bufLens) {
                tΔ2.Run(fmt.Sprintf("BuffLength=%d"u8, length), (ж<testing.T> tΔ3) => {
                    var rng = newRandReader(tΔ3);
                    var plaintext = new slice<byte>(length);
                    rng.Read(plaintext);
                    var ciphertext = new slice<byte>(length);
                    var decrypted = new slice<byte>(length);
                    ms().XORKeyStream(ciphertext, plaintext); // Encrypt plaintext
                    ms().XORKeyStream(decrypted, ciphertext); // Decrypt ciphertext
                    if (!bytes.Equal(decrypted, plaintext)) {
                        tΔ3.Errorf("plaintext is different after an encrypt/decrypt cycle; got %s, want %s"u8, truncateHex(decrypted), truncateHex(plaintext));
                    }
                });
            }
        });
        // Test that XORKeyStream behaves the same as directly XORing
        // plaintext with the stream.
        tΔ1.Run(directXORˢ, (ж<testing.T> tΔ4) => {
            foreach (var (_, length) in bufLens) {
                tΔ4.Run(fmt.Sprintf("BuffLength=%d"u8, length), (ж<testing.T> tΔ5) => {
                    var rng = newRandReader(tΔ5);
                    var plaintext = new slice<byte>(length);
                    rng.Read(plaintext);
                    // Encrypting all zeros should reveal the stream itself
                    var (stream, directXOR) = (new slice<byte>(length), new slice<byte>(length));
                    ms().XORKeyStream(stream, stream);
                    // Encrypt plaintext by directly XORing the stream
                    subtle.XORBytes(directXOR, stream, plaintext);
                    // Encrypt plaintext with XORKeyStream
                    var ciphertext = new slice<byte>(length);
                    ms().XORKeyStream(ciphertext, plaintext);
                    if (!bytes.Equal(ciphertext, directXOR)) {
                        tΔ5.Errorf("xor semantics were not preserved; got %s, want %s"u8, truncateHex(ciphertext), truncateHex(directXOR));
                    }
                });
            }
        });
    });
    Ꮡt.Run(emptyInputˢ, (ж<testing.T> tΔ6) => {
        var rng = newRandReader(tΔ6);
        var (src, dst) = (new slice<byte>(100), new slice<byte>(100));
        rng.Read(dst);
        var before = bytes.Clone(dst);
        ms().XORKeyStream(dst, src[..0]);
        if (!bytes.Equal(dst, before)) {
            tΔ6.Errorf("XORKeyStream modified dst on empty input; got %s, want %s"u8, truncateHex(dst), truncateHex(before));
        }
    });
    Ꮡt.Run(alterInputˢ, (ж<testing.T> tΔ7) => {
        var rng = newRandReader(tΔ7);
        var (src, dst, before) = (new slice<byte>(bufCap), new slice<byte>(bufCap), new slice<byte>(bufCap));
        rng.Read(src);
        foreach (var (_, length) in bufLens) {
            var beforeʗ1 = before;
            var dstʗ1 = dst;
            var srcʗ1 = src;
            tΔ7.Run(fmt.Sprintf("BuffLength=%d"u8, length), (ж<testing.T> tΔ8) => {
                copy(beforeʗ1, srcʗ1);
                ms().XORKeyStream(dstʗ1.slice(0, length), srcʗ1.slice(0, length));
                if (!bytes.Equal(srcʗ1, beforeʗ1)) {
                    tΔ8.Errorf("XORKeyStream modified src; got %s, want %s"u8, truncateHex(srcʗ1), truncateHex(beforeʗ1));
                }
            });
        }
    });
    Ꮡt.Run(aliasingˢ, (ж<testing.T> tΔ9) => {
        var rng = newRandReader(tΔ9);
        var (buff, expectedOutput) = (new slice<byte>(bufCap), new slice<byte>(bufCap));
        foreach (var (_, length) in bufLens) {
            // Record what output is when src and dst are different
            rng.Read(buff);
            ms().XORKeyStream(expectedOutput.slice(0, length), buff.slice(0, length));
            // Check that the same output is generated when src=dst alias to the same
            // memory
            ms().XORKeyStream(buff.slice(0, length), buff.slice(0, length));
            if (!bytes.Equal(buff.slice(0, length), expectedOutput.slice(0, length))) {
                tΔ9.Errorf("block cipher produced different output when dst = src; got %x, want %x"u8, buff.slice(0, length), expectedOutput.slice(0, length));
            }
        }
    });
    Ꮡt.Run(outOfBoundsWriteˢ, (ж<testing.T> tΔ10) => {
        // Issue 21104
        var rng = newRandReader(tΔ10);
        var plaintext = new slice<byte>(bufCap);
        rng.Read(plaintext);
        var ciphertext = new slice<byte>(bufCap);
        foreach (var (_, length) in bufLens) {
            copy(ciphertext, plaintext); // Reset ciphertext buffer
            var ciphertextʗ1 = ciphertext;
            var plaintextʗ1 = plaintext;
            tΔ10.Run(fmt.Sprintf("BuffLength=%d"u8, length), (ж<testing.T> tΔ11) => {
                var ciphertextʗ2 = ciphertextʗ1;
                var plaintextʗ2 = plaintextʗ1;
                mustPanic(tΔ11, outputSmallerThanInputˢ, () => {
                    ms().XORKeyStream(ciphertextʗ2.slice(0, length), plaintextʗ2);
                });
                if (!bytes.Equal(ciphertextʗ1.slice(length), plaintextʗ1.slice(length))) {
                    tΔ11.Errorf("XORKeyStream did out of bounds write; got %s, want %s"u8, truncateHex(ciphertextʗ1.slice(length)), truncateHex(plaintextʗ1.slice(length)));
                }
            });
        }
    });
    Ꮡt.Run(bufferOverlapˢ, (ж<testing.T> tΔ12) => {
        var rng = newRandReader(tΔ12);
        var buff = new slice<byte>(bufCap);
        rng.Read(buff);
        foreach (var (_, length) in bufLens) {
            if (length == 0 || length == 1) {
                continue;
            }
            var buffʗ1 = buff;
            tΔ12.Run(fmt.Sprintf("BuffLength=%d"u8, length), (ж<testing.T> tΔ13) => {
                // Make src and dst slices point to same array with inexact overlap
                ref var src = ref heap<slice<byte>>(out var Ꮡsrc);
                Ꮡsrc.ValueSlot = buffʗ1.slice(0, length);
                ref var dst = ref heap<slice<byte>>(out var Ꮡdst);
                Ꮡdst.ValueSlot = buffʗ1.slice(1, length + 1);
                mustPanic(tΔ13, invalidBufferOverlapˢ, () => {
                    ms().XORKeyStream(Ꮡdst.ValueSlot, Ꮡsrc.ValueSlot);
                });
                // Only overlap on one byte
                Ꮡsrc.ValueSlot = buffʗ1.slice(0, length);
                Ꮡdst.ValueSlot = buffʗ1.slice(length - 1, 2 * length - 1);
                mustPanic(tΔ13, invalidBufferOverlapˢ, () => {
                    ms().XORKeyStream(Ꮡdst.ValueSlot, Ꮡsrc.ValueSlot);
                });
                // src comes after dst with one byte overlap
                Ꮡsrc.ValueSlot = buffʗ1.slice(length - 1, 2 * length - 1);
                Ꮡdst.ValueSlot = buffʗ1.slice(0, length);
                mustPanic(tΔ13, invalidBufferOverlapˢ, () => {
                    ms().XORKeyStream(Ꮡdst.ValueSlot, Ꮡsrc.ValueSlot);
                });
            });
        }
    });
    Ꮡt.Run(keepStateˢ, (ж<testing.T> tΔ14) => {
        var rng = newRandReader(tΔ14);
        var plaintext = new slice<byte>(bufCap);
        rng.Read(plaintext);
        var ciphertext = new slice<byte>(bufCap);
        // Make one long call to XORKeyStream
        ms().XORKeyStream(ciphertext, plaintext);
        foreach (var (_, step) in bufLens) {
            if (step == 0) {
                continue;
            }
            @string stepMsg = fmt.Sprintf("step %d: "u8, step);
            var dst = new slice<byte>(bufCap);
            // Make a bunch of small calls to (stateful) XORKeyStream
            var stream = ms();
            nint i = 0;
            while (i + step < len(plaintext)) {
                stream.XORKeyStream(dst.slice(i), plaintext.slice(i, i + step));
                i += step;
            }
            stream.XORKeyStream(dst.slice(i), plaintext.slice(i));
            if (!bytes.Equal(dst, ciphertext)) {
                tΔ14.Errorf(stepMsg + "successive XORKeyStream calls returned a different result than a single one; got %s, want %s"u8, truncateHex(dst), truncateHex(ciphertext));
            }
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object seeIssue68377ˢ = (@string)"see Issue 68377"u8;
private static readonly @string blockModeStreamˢ = "BlockModeStream"u8;

// TestStreamFromBlock creates a Stream from a cipher.Block used in a
// cipher.BlockMode. It addresses Issue 68377 by checking for a panic when the
// BlockMode uses an IV with incorrect length.
// For a valid IV, it also runs all TestStream tests on the resulting stream.
public static void TestStreamFromBlock(ж<testing.T> Ꮡt, cipher.Block block, Func<cipher.Block, slice<byte>, cipher.Stream> blockMode) {
    Ꮡt.Run(wrongIVLenˢ, (ж<testing.T> tΔ1) => {
        tΔ1.Skip(seeIssue68377ˢ);
        var rng = newRandReader(tΔ1);
        var iv = new slice<byte>(block.BlockSize() + 1);
        rng.Read(iv);
        var ivʗ1 = iv;
        mustPanic(tΔ1, ivLengthMustEqualBlockˢ, () => {
            blockMode(block, ivʗ1);
        });
    });
    Ꮡt.Run(blockModeStreamˢ, (ж<testing.T> tΔ2) => {
        var rng = newRandReader(tΔ2);
        var iv = new slice<byte>(block.BlockSize());
        rng.Read(iv);
        var ivʗ2 = iv;
        TestStream(tΔ2, () => blockMode(block, ivʗ2));
    });
}

internal static @string truncateHex(slice<byte> b) {
    nint numVals = 50;
    if (len(b) <= numVals) {
        return fmt.Sprintf("%x"u8, b);
    }
    return fmt.Sprintf("%x..."u8, b.slice(0, numVals));
}

} // end cryptotest_package
