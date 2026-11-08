// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !fips140v1.0
namespace go.crypto;

using cipher = go.crypto.cipher_package;
using cryptotest = go.crypto.@internal.cryptotest_package;
using fips140 = go.crypto.@internal.fips140_package;
using fipsaes = go.crypto.@internal.fips140.aes_package;
using gcm = go.crypto.@internal.fips140.aes.gcm_package;
using binary = encoding.binary_package;
using testenv = go.@internal.testenv_package;
using math = math_package;
using testing = testing_package;
using aes = go.crypto.@internal.fips140.aes_package;
using encoding;
using exec = go.os.exec_package;
using go.@internal;
using go.crypto;
using go.crypto.@internal;
using go.crypto.@internal.fips140.aes;
using go.os;

partial class cipher_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string testRunˢ => ref testRunˢᵇ;
private static readonly @string testRunˢᵇ = "-test.run=^TestGCMNoncesFIPSV126$"u8;
private static ref readonly @string testVˢ => ref testVˢᵇ;
private static readonly @string testVˢᵇ = "-test.v"u8;
private static ref readonly @string ffffffffffffffffffffffffˢ => ref ffffffffffffffffffffffffˢᵇ;
private static readonly @string ffffffffffffffffffffffffˢᵇ = "ffffffffffffffffffffffff"u8;
private static ref readonly @string aabbccddeeff001122334455ˢ => ref aabbccddeeff001122334455ˢᵇ;
private static readonly @string aabbccddeeff001122334455ˢᵇ = "aabbccddeeff001122334455"u8;

public static void TestGCMNoncesFIPSV126(ж<testing.T> t) {
    cryptotest.MustSupportFIPS140(t);
    if (!fips140.Enabled) {
        var cmd = testenv.Command(new testing_TжTB(t), testenv.Executable(new testing_TжTB(t)), testRunˢ, testVˢ);
        cmd.Env = append(cmd.Environ(), (@string)"GODEBUG=fips140=on");
        var (@out, err) = cmd.CombinedOutput();
        t.Logf("running with GODEBUG=fips140=on:\n%s", @out);
        if (err != nil) {
            t.Errorf("fips140=on subprocess failed: %v", err);
        }
        return;
    }

    bool tryNonce(cipher.AEAD aead, slice<byte> nonce) {
        fips140.ResetServiceIndicator();
        aead.Seal(default, nonce, slice<byte>("x"u8), default);
        return fips140.ServiceIndicator();
    }
    var tryNonceʗ1 = tryNonce;
    void expectOK(ж<testing.T> tΔ1, cipher.AEAD aead, slice<byte> nonce) {
        tΔ1.Helper();
        if (!tryNonceʗ1(aead, nonce)) {
            tΔ1.Errorf("expected service indicator true for %x", nonce);
        }
    }
    var tryNonceʗ2 = tryNonce;
    void expectPanic(ж<testing.T> tΔ2, cipher.AEAD aead, slice<byte> nonce) {
        GoFrame ᒐ = default;
        try {
            tΔ2.Helper();
            var nonceʗ1 = nonce;
            defer(() => {
                tΔ2.Helper();
                if (recover() == nil) {
                    tΔ2.Errorf("expected panic for %x", nonceʗ1);
                }
            }, ref ᒐ);
            tryNonceʗ2(aead, nonce);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }

    var expectOKʗ1 = expectOK;
    var expectPanicʗ1 = expectPanic;
    t.Run("NewGCMWithXORCounterNonce", (ж<testing.T> tΔ3) => {
        ж<gcm.GCMWithXORCounterNonce> newGCM() {
            var key = new slice<byte>(16);
            var (block, _) = fipsaes.New(key);
            var (aead, _) = gcm.NewGCMWithXORCounterNonce(block);
            return aead;
        }
        slice<byte> nonce(slice<byte> mask, uint64 counter) {
            var nonceΔ1 = new slice<byte>(12);
            copy(nonceΔ1, mask);
            var n = binary.BigEndian.AppendUint64(default, counter);
            foreach (var (i, b) in range(n)) {
                nonceΔ1[4 + i] ^= (byte)(b);
            }
            return nonceΔ1;
        }

        foreach (var (_, mask) in range(slice<slice<byte>>([
            decodeHex(tΔ3, ffffffffffffffffffffffffˢ),
            decodeHex(tΔ3, aabbccddeeff001122334455ˢ),
            decodeHex(tΔ3, "000000000000000000000000")
        ]))) {
            var g = newGCM();
            // Mask is derived from first invocation with zero nonce.
            expectOKʗ1(tΔ3, new gcm_GCMWithXORCounterNonceжAEAD(g), nonce(mask, 0));
            expectOKʗ1(tΔ3, new gcm_GCMWithXORCounterNonceжAEAD(g), nonce(mask, 1));
            expectOKʗ1(tΔ3, new gcm_GCMWithXORCounterNonceжAEAD(g), nonce(mask, 100));
            expectPanicʗ1(tΔ3, new gcm_GCMWithXORCounterNonceжAEAD(g), nonce(mask, 100));
            expectPanicʗ1(tΔ3, new gcm_GCMWithXORCounterNonceжAEAD(g), nonce(mask, 99));
            expectOKʗ1(tΔ3, new gcm_GCMWithXORCounterNonceжAEAD(g), nonce(mask, math.MaxUint64 - 2));
            expectOKʗ1(tΔ3, new gcm_GCMWithXORCounterNonceжAEAD(g), nonce(mask, math.MaxUint64 - 1));
            expectPanicʗ1(tΔ3, new gcm_GCMWithXORCounterNonceжAEAD(g), nonce(mask, math.MaxUint64));
            expectPanicʗ1(tΔ3, new gcm_GCMWithXORCounterNonceжAEAD(g), nonce(mask, 0));

            g = newGCM();
            g.SetNoncePrefixAndMask(mask);
            expectOKʗ1(tΔ3, new gcm_GCMWithXORCounterNonceжAEAD(g), nonce(mask, 0xFFFFFFFFU));
            expectOKʗ1(tΔ3, new gcm_GCMWithXORCounterNonceжAEAD(g), nonce(mask, math.MaxUint64 - 2));
            expectOKʗ1(tΔ3, new gcm_GCMWithXORCounterNonceжAEAD(g), nonce(mask, math.MaxUint64 - 1));
            expectPanicʗ1(tΔ3, new gcm_GCMWithXORCounterNonceжAEAD(g), nonce(mask, math.MaxUint64));
            expectPanicʗ1(tΔ3, new gcm_GCMWithXORCounterNonceжAEAD(g), nonce(mask, 0));

            g = newGCM();
            g.SetNoncePrefixAndMask(mask);
            expectOKʗ1(tΔ3, new gcm_GCMWithXORCounterNonceжAEAD(g), nonce(mask, math.MaxUint64 - 1));
            expectPanicʗ1(tΔ3, new gcm_GCMWithXORCounterNonceжAEAD(g), nonce(mask, math.MaxUint64));
            expectPanicʗ1(tΔ3, new gcm_GCMWithXORCounterNonceжAEAD(g), nonce(mask, 0));
        }
    });
}

} // end cipher_test_package
