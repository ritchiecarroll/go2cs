// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using bufio = bufio_package;
using bigmod = go.crypto.@internal.fips140.bigmod_package;
using hex = encoding.hex_package;
using fmt = fmt_package;
using big = math.big_package;
using os = os_package;
using strings = strings_package;
using testing = testing_package;
using encoding;
using go.crypto.@internal.fips140;
using io = io_package;
using math;
using static go.crypto.@internal.fips140.rsa_package;

partial class rsa_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataMillerRabinTestsˢ = "testdata/miller_rabin_tests.txt"u8;

public static void TestMillerRabin(ж<testing.T> Ꮡt) {
    var (f, err) = os.Open(testdataMillerRabinTestsˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }

    bool expected = default!;
    @string W = default!;
    @string B = default!;
    nint lineNum = default!;
    var scanner = bufio.NewScanner(new rsa_internal_test_package.os_FileжReader(f));
    while (scanner.Scan()) {
        lineNum++;
        @string line = scanner.Text();
        if (len(line) == 0 || line[0] == (rune)'#') {
            continue;
        }

        var (k, v, _) = strings.Cut(line, " = "u8);
        var exprᴛ1 = k;
        if (exprᴛ1 == "Result"u8) {
            var exprᴛ2 = v;
            if (exprᴛ2 == "Composite"u8) {
                expected = millerRabinCOMPOSITE;
            }
            else if (exprᴛ2 == "PossiblyPrime"u8) {
                expected = millerRabinPOSSIBLYPRIME;
            }
            else { /* default: */
                Ꮡt.Fatalf("unknown result %q on line %d"u8, v, lineNum);
            }
        }
        else if (exprᴛ1 == "W"u8) {
            W = v;
        }
        else if (exprᴛ1 == "B"u8) {
            B = v;

            Ꮡt.Run(fmt.Sprintf("line %d"u8, lineNum), (ж<testing.T> tΔ2) => {
                if (len(W) % 2 != 0) {
                    W = "0"u8 + W;
                }
                while (len(B) < len(W)) {
                    B = "0"u8 + B;
                }

                var (mr, errΔ2) = millerRabinSetup(decodeHex(tΔ2, W));
                if (errΔ2 != default!) {
                    tΔ2.Logf("W = %s"u8, W);
                    tΔ2.Logf("B = %s"u8, B);
                    tΔ2.Fatalf("failed to set up Miller-Rabin test: %v"u8, errΔ2);
                }

                (var result, errΔ2) = millerRabinIteration(ref (mr).DerefOrNull(), decodeHex(tΔ2, B));
                if (errΔ2 != default!) {
                    tΔ2.Logf("W = %s"u8, W);
                    tΔ2.Logf("B = %s"u8, B);
                    tΔ2.Fatalf("failed to run Miller-Rabin test: %v"u8, errΔ2);
                }

                if (result != expected) {
                    tΔ2.Logf("W = %s"u8, W);
                    tΔ2.Logf("B = %s"u8, B);
                    tΔ2.Fatalf("unexpected result: got %v, want %v"u8, result, expected);
                }
            });
        }
        else { /* default: */
            Ꮡt.Fatalf("unknown key %q on line %d"u8, k, lineNum);
        }
    }
    {
        var errΔ3 = scanner.Err(); if (errΔ3 != default!) {
            Ꮡt.Fatal(errΔ3);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataGcdLcmTestsTxtˢ = "testdata/gcd_lcm_tests.txt"u8;
internal static readonly object skippingTestWithZeroˢ = (@string)"skipping test with zero input"u8;
internal static readonly object skippingTestWithLcm1ˢ = (@string)"skipping test with LCM=1"u8;
internal static readonly object gcdTooLargeˢ = (@string)"GCD too large"u8;

public static void TestTotient(ж<testing.T> Ꮡt) {
    var (f, err) = os.Open(testdataGcdLcmTestsTxtˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }

    @string GCD = default!;
    @string A = default!;
    @string B = default!;
    @string LCM = default!;
    nint lineNum = default!;
    var scanner = bufio.NewScanner(new rsa_internal_test_package.os_FileжReader(f));
    while (scanner.Scan()) {
        lineNum++;
        @string line = scanner.Text();
        if (len(line) == 0 || line[0] == (rune)'#') {
            continue;
        }

        var (k, v, _) = strings.Cut(line, " = "u8);
        var exprᴛ1 = k;
        if (exprᴛ1 == "GCD"u8) {
            GCD = v;
        }
        else if (exprᴛ1 == "A"u8) {
            A = v;
        }
        else if (exprᴛ1 == "B"u8) {
            B = v;
        }
        else if (exprᴛ1 == "LCM"u8) {
            LCM = v;

            Ꮡt.Run(fmt.Sprintf("line %d"u8, lineNum), (ж<testing.T> tΔ2) => {
                if (A == "0"u8 || B == "0"u8) {
                    tΔ2.Skip(skippingTestWithZeroˢ);
                }
                if (LCM == "1"u8) {
                    tΔ2.Skip(skippingTestWithLcm1ˢ);
                }

                var (p, _) = bigmod.NewModulus(addOne(decodeHex(tΔ2, A)));
                var (a, _) = bigmod.NewNat().SetBytes(decodeHex(tΔ2, A), p);
                var (q, _) = bigmod.NewModulus(addOne(decodeHex(tΔ2, B)));
                var (b, _) = bigmod.NewNat().SetBytes(decodeHex(tΔ2, B), q);

                var (gcd, errΔ2) = bigmod.NewNat().GCDVarTime(a, b);
                // GCD doesn't work if a and b are both even, but LCM handles it.
                if (errΔ2 == default!) {
                    {
                        @string got = strings.TrimLeft(hex.EncodeToString(gcd.Bytes(p)), "0"u8); if (got != GCD) {
                            tΔ2.Fatalf("unexpected GCD: got %s, want %s"u8, got, GCD);
                        }
                    }
                }

                (var lcm, errΔ2) = totient(p, q);
                if (oddDivisorLargerThan32Bits(decodeHex(tΔ2, GCD))) {
                    if (!AreEqual(errΔ2, errDivisorTooLarge)) {
                        tΔ2.Fatalf("expected divisor too large error, got %v"u8, errΔ2);
                    }
                    tΔ2.Skip(gcdTooLargeˢ);
                }
                if (errΔ2 != default!) {
                    tΔ2.Fatalf("failed to calculate totient: %v"u8, errΔ2);
                }
                {
                    @string got = strings.TrimLeft(hex.EncodeToString(lcm.Nat().Bytes(lcm)), "0"u8); if (got != LCM) {
                        tΔ2.Fatalf("unexpected LCM: got %s, want %s"u8, got, LCM);
                    }
                }
            });
        }
        else { /* default: */
            Ꮡt.Fatalf("unknown key %q on line %d"u8, k, lineNum);
        }
    }
    {
        var errΔ3 = scanner.Err(); if (errΔ3 != default!) {
            Ꮡt.Fatal(errΔ3);
        }
    }
}

internal static bool oddDivisorLargerThan32Bits(slice<byte> b) {
    var x = @new<bigꓸInt>().SetBytes(b);
    x.Rsh(x, x.TrailingZeroBits());
    return x.BitLen() > 32;
}

internal static slice<byte> addOne(slice<byte> b) {
    var x = @new<bigꓸInt>().SetBytes(b);
    x.Add(x, big.NewInt(1));
    return x.Bytes();
}

internal static slice<byte> decodeHex(ж<testing.T> Ꮡt, @string s) {
    Ꮡt.Helper();
    if (len(s) % 2 != 0) {
        s = "0"u8 + s;
    }
    var (b, err) = hex.DecodeString(s);
    if (err != default!) {
        Ꮡt.Fatalf("failed to decode hex %q: %v"u8, s, err);
    }
    return b;
}

} // end rsa_internal_test_package
