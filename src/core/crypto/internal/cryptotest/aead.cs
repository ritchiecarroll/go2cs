// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using bytes = bytes_package;
using cipher = go.crypto.cipher_package;
using fmt = fmt_package;
using testing = testing_package;
using go.crypto;
using io = io_package;

partial class cryptotest_package {

internal static slice<nint> lengths = new nint[]{0, 156, 8192, 8193, 8208}.slice();

// type MakeAEAD is a methodless func type — rendered inline as its base delegate

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string roundtripˢ = "Roundtrip"u8;
private static readonly @string inputNotModifiedˢ = "InputNotModified"u8;
private static readonly @string sealˢ = "Seal"u8;
private static readonly @string openˢ = "Open"u8;
private static readonly @string bufferOverlapˢ = "BufferOverlap"u8;
private static readonly @string invalidBufferOverlapˢ = "invalid buffer overlap"u8;
private static readonly @string appendDstˢ = "AppendDst"u8;
private static readonly @string wrongNonceˢ = "WrongNonce"u8;
private static readonly object aeadDoesNotUseANonceˢ = (@string)"AEAD does not use a nonce"u8;
private static readonly @string wrongAddDataˢ = "WrongAddData"u8;
private static readonly @string wrongCiphertextˢ = "WrongCiphertext"u8;

// TestAEAD performs a set of tests on cipher.AEAD implementations, checking
// the documented requirements of NonceSize, Overhead, Seal and Open.
public static void TestAEAD(ж<testing.T> Ꮡt, Func<(cipher.AEAD, error)> mAEAD) {
    ref var t = ref Ꮡt.DerefOrNull();

    var (aead, err) = mAEAD();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var aeadʗ1 = aead;
    Ꮡt.Run(roundtripˢ, (ж<testing.T> tΔ1) => {
        // Test all combinations of plaintext and additional data lengths.
        foreach (var (_, ptLen) in lengths) {
            foreach (var (_, adLen) in lengths) {
                var aeadʗ2 = aeadʗ1;
                tΔ1.Run(fmt.Sprintf("Plaintext-Length=%d,AddData-Length=%d"u8, ptLen, adLen), (ж<testing.T> tΔ2) => {
                    var rng = newRandReader(tΔ2);
                    var nonce = new slice<byte>(aeadʗ2.NonceSize());
                    rng.Read(nonce);
                    var (before, addData) = (new slice<byte>(adLen), new slice<byte>(ptLen));
                    rng.Read(before);
                    rng.Read(addData);
                    var ciphertext = sealMsg(tΔ2, aeadʗ2, default!, nonce, before, addData);
                    var after = openWithoutError(tΔ2, aeadʗ2, default!, nonce, ciphertext, addData);
                    if (!bytes.Equal(after, before)) {
                        tΔ2.Errorf("plaintext is different after a seal/open cycle; got %s, want %s"u8, truncateHex(after), truncateHex(before));
                    }
                });
            }
        }
    });
    var aeadʗ3 = aead;
    Ꮡt.Run(inputNotModifiedˢ, (ж<testing.T> tΔ3) => {
        // Test all combinations of plaintext and additional data lengths.
        foreach (var (_, ptLen) in lengths) {
            foreach (var (_, adLen) in lengths) {
                var aeadʗ4 = aeadʗ3;
                tΔ3.Run(fmt.Sprintf("Plaintext-Length=%d,AddData-Length=%d"u8, ptLen, adLen), (ж<testing.T> tΔ4) => {
                    var aeadʗ5 = aeadʗ4;
                    tΔ4.Run(sealˢ, (ж<testing.T> tΔ5) => {
                        var rng = newRandReader(tΔ5);
                        var nonce = new slice<byte>(aeadʗ5.NonceSize());
                        rng.Read(nonce);
                        var (src, before) = (new slice<byte>(ptLen), new slice<byte>(ptLen));
                        rng.Read(src);
                        copy(before, src);
                        var addData = new slice<byte>(adLen);
                        rng.Read(addData);
                        sealMsg(tΔ5, aeadʗ5, default!, nonce, src, addData);
                        if (!bytes.Equal(src, before)) {
                            tΔ5.Errorf("Seal modified src; got %s, want %s"u8, truncateHex(src), truncateHex(before));
                        }
                    });
                    var aeadʗ6 = aeadʗ4;
                    tΔ4.Run(openˢ, (ж<testing.T> tΔ6) => {
                        var rng = newRandReader(tΔ6);
                        var nonce = new slice<byte>(aeadʗ6.NonceSize());
                        rng.Read(nonce);
                        var (plaintext, addData) = (new slice<byte>(ptLen), new slice<byte>(adLen));
                        rng.Read(plaintext);
                        rng.Read(addData);
                        // Record the ciphertext that shouldn't be modified as the input of
                        // Open.
                        var ciphertext = sealMsg(tΔ6, aeadʗ6, default!, nonce, plaintext, addData);
                        var before = new slice<byte>(len(ciphertext));
                        copy(before, ciphertext);
                        openWithoutError(tΔ6, aeadʗ6, default!, nonce, ciphertext, addData);
                        if (!bytes.Equal(ciphertext, before)) {
                            tΔ6.Errorf("Open modified src; got %s, want %s"u8, truncateHex(ciphertext), truncateHex(before));
                        }
                    });
                });
            }
        }
    });
    var aeadʗ7 = aead;
    Ꮡt.Run(bufferOverlapˢ, (ж<testing.T> tΔ7) => {
        // Test all combinations of plaintext and additional data lengths.
        foreach (var (_, ptLen) in lengths) {
            if (ptLen <= 1) {
                // We need enough room for an inexact overlap to occur.
                continue;
            }
            foreach (var (_, adLen) in lengths) {
                var aeadʗ8 = aeadʗ7;
                tΔ7.Run(fmt.Sprintf("Plaintext-Length=%d,AddData-Length=%d"u8, ptLen, adLen), (ж<testing.T> tΔ8) => {
                    var aeadʗ9 = aeadʗ8;
                    tΔ8.Run(sealˢ, (ж<testing.T> tΔ9) => {
                        var rng = newRandReader(tΔ9);
                        var nonce = new slice<byte>(aeadʗ9.NonceSize());
                        rng.Read(nonce);
                        // Make a buffer that can hold a plaintext and ciphertext as we
                        // overlap their slices to check for panic on inexact overlaps.
                        nint ctLen = ptLen + aeadʗ9.Overhead();
                        var buff = new slice<byte>(ptLen + ctLen);
                        rng.Read(buff);
                        var addData = new slice<byte>(adLen);
                        rng.Read(addData);
                        // Make plaintext and dst slices point to same array with inexact overlap.
                        ref var plaintext = ref heap<slice<byte>>(out var Ꮡplaintext);
                        Ꮡplaintext.ValueSlot = buff.slice(0, ptLen);
                        ref var dst = ref heap<slice<byte>>(out var Ꮡdst);
                        Ꮡdst.ValueSlot = buff[1..1]; // Shift dst to not start at start of plaintext.
                        var addDataʗ1 = addData;
                        var aeadʗ10 = aeadʗ9;
                        var nonceʗ1 = nonce;
                        mustPanic(tΔ9, invalidBufferOverlapˢ, () => {
                            sealMsg(tΔ9, aeadʗ10, Ꮡdst.ValueSlot, nonceʗ1, Ꮡplaintext.ValueSlot, addDataʗ1);
                        });
                        // Only overlap on one byte
                        Ꮡplaintext.ValueSlot = buff.slice(0, ptLen);
                        Ꮡdst.ValueSlot = buff.slice(ptLen - 1, ptLen - 1);
                        var addDataʗ2 = addData;
                        var aeadʗ11 = aeadʗ9;
                        var nonceʗ2 = nonce;
                        mustPanic(tΔ9, invalidBufferOverlapˢ, () => {
                            sealMsg(tΔ9, aeadʗ11, Ꮡdst.ValueSlot, nonceʗ2, Ꮡplaintext.ValueSlot, addDataʗ2);
                        });
                    });
                    var aeadʗ12 = aeadʗ8;
                    tΔ8.Run(openˢ, (ж<testing.T> tΔ10) => {
                        var rng = newRandReader(tΔ10);
                        var nonce = new slice<byte>(aeadʗ12.NonceSize());
                        rng.Read(nonce);
                        // Create a valid ciphertext to test Open with.
                        var plaintext = new slice<byte>(ptLen);
                        rng.Read(plaintext);
                        var addData = new slice<byte>(adLen);
                        rng.Read(addData);
                        var validCT = sealMsg(tΔ10, aeadʗ12, default!, nonce, plaintext, addData);
                        // Make a buffer that can hold a plaintext and ciphertext as we
                        // overlap their slices to check for panic on inexact overlaps.
                        var buff = new slice<byte>(ptLen + len(validCT));
                        // Make ciphertext and dst slices point to same array with inexact overlap.
                        ref var ciphertext = ref heap<slice<byte>>(out var Ꮡciphertext);
                        Ꮡciphertext.ValueSlot = buff.slice(0, len(validCT));
                        copy(Ꮡciphertext.ValueSlot, validCT);
                        ref var dst = ref heap<slice<byte>>(out var Ꮡdst);
                        Ꮡdst.ValueSlot = buff[1..1]; // Shift dst to not start at start of ciphertext.
                        var addDataʗ3 = addData;
                        var aeadʗ13 = aeadʗ12;
                        var nonceʗ3 = nonce;
                        mustPanic(tΔ10, invalidBufferOverlapˢ, () => {
                            aeadʗ13.Open(Ꮡdst.ValueSlot, nonceʗ3, Ꮡciphertext.ValueSlot, addDataʗ3);
                        });
                        // Only overlap on one byte.
                        Ꮡciphertext.ValueSlot = buff.slice(0, len(validCT));
                        copy(Ꮡciphertext.ValueSlot, validCT);
                        // Make sure it is the actual ciphertext being overlapped and not
                        // the hash digest which might be extracted/truncated in some
                        // implementations: Go one byte past the hash digest/tag and into
                        // the ciphertext.
                        nint beforeTag = len(validCT) - aeadʗ12.Overhead();
                        Ꮡdst.ValueSlot = buff.slice(beforeTag - 1, beforeTag - 1);
                        var addDataʗ4 = addData;
                        var aeadʗ14 = aeadʗ12;
                        var nonceʗ4 = nonce;
                        mustPanic(tΔ10, invalidBufferOverlapˢ, () => {
                            aeadʗ14.Open(Ꮡdst.ValueSlot, nonceʗ4, Ꮡciphertext.ValueSlot, addDataʗ4);
                        });
                    });
                });
            }
        }
    });
    var aeadʗ15 = aead;
    Ꮡt.Run(appendDstˢ, (ж<testing.T> tΔ11) => {
        // Test all combinations of plaintext and additional data lengths.
        foreach (var (_, ptLen) in lengths) {
            foreach (var (_, adLen) in lengths) {
                var aeadʗ16 = aeadʗ15;
                tΔ11.Run(fmt.Sprintf("Plaintext-Length=%d,AddData-Length=%d"u8, ptLen, adLen), (ж<testing.T> tΔ12) => {
                    var aeadʗ17 = aeadʗ16;
                    tΔ12.Run(sealˢ, (ж<testing.T> tΔ13) => {
                        var rng = newRandReader(tΔ13);
                        var nonce = new slice<byte>(aeadʗ17.NonceSize());
                        rng.Read(nonce);
                        var shortBuff = slice<byte>("a"u8);
                        var longBuff = new slice<byte>(512);
                        rng.Read(longBuff);
                        var prefixes = new slice<byte>[]{shortBuff, longBuff}.slice();
                        // Check each prefix gets appended to by Seal without altering them.
                        foreach (var (_, prefix) in prefixes) {
                            var (plaintext, addData) = (new slice<byte>(ptLen), new slice<byte>(adLen));
                            rng.Read(plaintext);
                            rng.Read(addData);
                            var @out = sealMsg(tΔ13, aeadʗ17, prefix, nonce, plaintext, addData);
                            // Check that Seal didn't alter the prefix
                            if (!bytes.Equal(@out.slice(0, len(prefix)), prefix)) {
                                tΔ13.Errorf("Seal alters dst instead of appending; got %s, want %s"u8, truncateHex(@out.slice(0, len(prefix))), truncateHex(prefix));
                            }
                            if (isDeterministic(aeadʗ17)) {
                                var ciphertext = @out.slice(len(prefix));
                                // Check that the appended ciphertext wasn't affected by the prefix
                                {
                                    var expectedCT = sealMsg(tΔ13, aeadʗ17, default!, nonce, plaintext, addData); if (!bytes.Equal(ciphertext, expectedCT)) {
                                        tΔ13.Errorf("Seal behavior affected by pre-existing data in dst; got %s, want %s"u8, truncateHex(ciphertext), truncateHex(expectedCT));
                                    }
                                }
                            }
                        }
                    });
                    var aeadʗ18 = aeadʗ16;
                    tΔ12.Run(openˢ, (ж<testing.T> tΔ14) => {
                        var rng = newRandReader(tΔ14);
                        var nonce = new slice<byte>(aeadʗ18.NonceSize());
                        rng.Read(nonce);
                        var shortBuff = slice<byte>("a"u8);
                        var longBuff = new slice<byte>(512);
                        rng.Read(longBuff);
                        var prefixes = new slice<byte>[]{shortBuff, longBuff}.slice();
                        // Check each prefix gets appended to by Open without altering them.
                        foreach (var (_, prefix) in prefixes) {
                            var (before, addData) = (new slice<byte>(adLen), new slice<byte>(ptLen));
                            rng.Read(before);
                            rng.Read(addData);
                            var ciphertext = sealMsg(tΔ14, aeadʗ18, default!, nonce, before, addData);
                            var @out = openWithoutError(tΔ14, aeadʗ18, prefix, nonce, ciphertext, addData);
                            // Check that Open didn't alter the prefix
                            if (!bytes.Equal(@out.slice(0, len(prefix)), prefix)) {
                                tΔ14.Errorf("Open alters dst instead of appending; got %s, want %s"u8, truncateHex(@out.slice(0, len(prefix))), truncateHex(prefix));
                            }
                            var after = @out.slice(len(prefix));
                            // Check that the appended plaintext wasn't affected by the prefix
                            if (!bytes.Equal(after, before)) {
                                tΔ14.Errorf("Open behavior affected by pre-existing data in dst; got %s, want %s"u8, truncateHex(after), truncateHex(before));
                            }
                        }
                    });
                });
            }
        }
    });
    var aeadʗ19 = aead;
    Ꮡt.Run(wrongNonceˢ, (ж<testing.T> tΔ15) => {
        if (aeadʗ19.NonceSize() == 0) {
            tΔ15.Skip(aeadDoesNotUseANonceˢ);
        }
        // Test all combinations of plaintext and additional data lengths.
        foreach (var (_, ptLen) in lengths) {
            foreach (var (_, adLen) in lengths) {
                var aeadʗ20 = aeadʗ19;
                tΔ15.Run(fmt.Sprintf("Plaintext-Length=%d,AddData-Length=%d"u8, ptLen, adLen), (ж<testing.T> tΔ16) => {
                    var rng = newRandReader(tΔ16);
                    var nonce = new slice<byte>(aeadʗ20.NonceSize());
                    rng.Read(nonce);
                    var (plaintext, addData) = (new slice<byte>(ptLen), new slice<byte>(adLen));
                    rng.Read(plaintext);
                    rng.Read(addData);
                    var ciphertext = sealMsg(tΔ16, aeadʗ20, default!, nonce, plaintext, addData);
                    // Perturb the nonce and check for an error when Opening
                    var alterNonce = new slice<byte>(aeadʗ20.NonceSize());
                    copy(alterNonce, nonce);
                    alterNonce[len(alterNonce) - 1] += 1;
                    var (_, errΔ1) = aeadʗ20.Open(default!, alterNonce, ciphertext, addData);
                    if (errΔ1 == default!) {
                        tΔ16.Errorf("Open did not error when given different nonce than Sealed with"u8);
                    }
                });
            }
        }
    });
    var aeadʗ21 = aead;
    Ꮡt.Run(wrongAddDataˢ, (ж<testing.T> tΔ17) => {
        // Test all combinations of plaintext and additional data lengths.
        foreach (var (_, ptLen) in lengths) {
            foreach (var (_, adLen) in lengths) {
                if (adLen == 0) {
                    continue;
                }
                var aeadʗ22 = aeadʗ21;
                tΔ17.Run(fmt.Sprintf("Plaintext-Length=%d,AddData-Length=%d"u8, ptLen, adLen), (ж<testing.T> tΔ18) => {
                    var rng = newRandReader(tΔ18);
                    var nonce = new slice<byte>(aeadʗ22.NonceSize());
                    rng.Read(nonce);
                    var (plaintext, addData) = (new slice<byte>(ptLen), new slice<byte>(adLen));
                    rng.Read(plaintext);
                    rng.Read(addData);
                    var ciphertext = sealMsg(tΔ18, aeadʗ22, default!, nonce, plaintext, addData);
                    // Perturb the Additional Data and check for an error when Opening
                    var alterAD = new slice<byte>(adLen);
                    copy(alterAD, addData);
                    alterAD[len(alterAD) - 1] += 1;
                    var (_, errΔ2) = aeadʗ22.Open(default!, nonce, ciphertext, alterAD);
                    if (errΔ2 == default!) {
                        tΔ18.Errorf("Open did not error when given different Additional Data than Sealed with"u8);
                    }
                });
            }
        }
    });
    var aeadʗ23 = aead;
    Ꮡt.Run(wrongCiphertextˢ, (ж<testing.T> tΔ19) => {
        // Test all combinations of plaintext and additional data lengths.
        foreach (var (_, ptLen) in lengths) {
            foreach (var (_, adLen) in lengths) {
                var aeadʗ24 = aeadʗ23;
                tΔ19.Run(fmt.Sprintf("Plaintext-Length=%d,AddData-Length=%d"u8, ptLen, adLen), (ж<testing.T> tΔ20) => {
                    var rng = newRandReader(tΔ20);
                    var nonce = new slice<byte>(aeadʗ24.NonceSize());
                    rng.Read(nonce);
                    var (plaintext, addData) = (new slice<byte>(ptLen), new slice<byte>(adLen));
                    rng.Read(plaintext);
                    rng.Read(addData);
                    var ciphertext = sealMsg(tΔ20, aeadʗ24, default!, nonce, plaintext, addData);
                    // Perturb the ciphertext and check for an error when Opening
                    var alterCT = new slice<byte>(len(ciphertext));
                    copy(alterCT, ciphertext);
                    alterCT[len(alterCT) - 1] += 1;
                    var (_, errΔ3) = aeadʗ24.Open(default!, nonce, alterCT, addData);
                    if (errΔ3 == default!) {
                        tΔ20.Errorf("Open did not error when given different ciphertext than was produced by Seal"u8);
                    }
                });
            }
        }
    });
}

// Helper function to Seal a plaintext with additional data. Checks that
// ciphertext isn't bigger than the plaintext length plus Overhead()
internal static slice<byte> sealMsg(ж<testing.T> Ꮡt, cipher.AEAD aead, slice<byte> ciphertext, slice<byte> nonce, slice<byte> plaintext, slice<byte> addData) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Helper();
    nint initialLen = len(ciphertext);
    ciphertext = aead.Seal(ciphertext, nonce, plaintext, addData);
    nint lenCT = len(ciphertext) - initialLen;
    // Appended ciphertext shouldn't ever be longer than the length of the
    // plaintext plus Overhead
    if (lenCT > len(plaintext) + aead.Overhead()) {
        Ꮡt.Errorf("length of ciphertext from Seal exceeds length of plaintext by more than Overhead(); got %d, want <=%d"u8, lenCT, len(plaintext) + aead.Overhead());
    }
    return ciphertext;
}

internal static bool isDeterministic(cipher.AEAD aead) {
    // Check if the AEAD is deterministic by checking if the same plaintext
    // encrypted with the same nonce and additional data produces the same
    // ciphertext.
    var nonce = new slice<byte>(aead.NonceSize());
    var addData = slice<byte>("additional data"u8);
    var plaintext = slice<byte>("plaintext"u8);
    var ciphertext1 = aead.Seal(default!, nonce, plaintext, addData);
    var ciphertext2 = aead.Seal(default!, nonce, plaintext, addData);
    return bytes.Equal(ciphertext1, ciphertext2);
}

// Helper function to Open and authenticate ciphertext. Checks that Open
// doesn't error (assuming ciphertext was well-formed with corresponding nonce
// and additional data).
internal static slice<byte> openWithoutError(ж<testing.T> Ꮡt, cipher.AEAD aead, slice<byte> plaintext, slice<byte> nonce, slice<byte> ciphertext, slice<byte> addData) {
    Ꮡt.Helper();
    (plaintext, var err) = aead.Open(plaintext, nonce, ciphertext, addData);
    if (err != default!) {
        Ꮡt.Fatalf("Open returned error on properly formed ciphertext; got \"%s\", want \"nil\""u8, err);
    }
    return plaintext;
}

} // end cryptotest_package
