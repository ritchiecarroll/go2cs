// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using testing = testing_package;
using static go.crypto.@internal.fips140.aes_package;

partial class aes_internal_test_package {

// See const.go for overview of math here.

// Test that powx is initialized correctly.
// (Can adapt this code to generate it too.)
public static void TestPowx(ж<testing.T> Ꮡt) {
    nint p = 1;
    for (nint i = 0; i < len(powx); i++) {
        if (powx[i] != (byte)p) {
            Ꮡt.Errorf("powx[%d] = %#x, want %#x"u8, i, powx[i], p);
        }
        p <<= (int)(1);
        if ((nint)(p & 0x100) != 0) {
            p ^= (nint)(poly);
        }
    }
}

// Multiply b and c as GF(2) polynomials modulo poly
internal static uint32 mul(uint32 b, uint32 c) {
    var i = b;
    var j = c;
    var s = (uint32)0;
    for (var k = (uint32)1; k < 0x100 && j != 0; k <<= (int)(1)) {
        // Invariant: k == 1<<n, i == b * xⁿ
        if ((uint32)(j & k) != 0) {
            // s += i in GF(2); xor in binary
            s ^= (uint32)(i);
            j ^= (uint32)(k); // turn off bit to end loop early
        }
        // i *= x in GF(2) modulo the polynomial
        i <<= (int)(1);
        if ((uint32)(i & 0x100) != 0) {
            i ^= (uint32)(poly);
        }
    }
    return s;
}

// Test all mul inputs against bit-by-bit n² algorithm.
public static void TestMul(ж<testing.T> Ꮡt) {
    for (var i = (uint32)0; i < 256; i++) {
        for (var j = (uint32)0; j < 256; j++) {
            // Multiply i, j bit by bit.
            var s = (uint8)0;
            for (nuint k = (nuint)0; k < 8; k++) {
                for (nuint l = (nuint)0; l < 8; l++) {
                    if ((uint32)(i & (((uint32)1).Lsh(k))) != 0 && (uint32)(j & (((uint32)1).Lsh(l))) != 0) {
                        s ^= (byte)(powx[k + l]);
                    }
                }
            }
            {
                var x = mul(i, j); if (x != (uint32)s) {
                    Ꮡt.Fatalf("mul(%#x, %#x) = %#x, want %#x"u8, i, j, x, s);
                }
            }
        }
    }
}

// Check that S-boxes are inverses of each other.
// They have more structure that we could test,
// but if this sanity check passes, we'll assume
// the cut and paste from the FIPS PDF worked.
public static void TestSboxes(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < 256; i++) {
        {
            var j = sbox0[sbox1[i]]; if (j != (byte)i) {
                Ꮡt.Errorf("sbox0[sbox1[%#x]] = %#x"u8, i, j);
            }
        }
        {
            var j = sbox1[sbox0[i]]; if (j != (byte)i) {
                Ꮡt.Errorf("sbox1[sbox0[%#x]] = %#x"u8, i, j);
            }
        }
    }
}

// Test that encryption tables are correct.
// (Can adapt this code to generate them too.)
public static void TestTe(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < 256; i++) {
        var s = (uint32)sbox0[i];
        var s2 = mul(s, 2);
        var s3 = mul(s, 3);
        var w = (uint32)((uint32)((uint32)((s2 << (int)(24)) | (s << (int)(16))) | (s << (int)(8))) | s3);
        var te = GoReflect.WithElemDims(new array<uint32>[]{te0.Clone(), te1.Clone(), te2.Clone(), te3.Clone()}.slice(), 256);
        for (nint j = 0; j < 4; j++) {
            {
                var x = te[j][i]; if (x != w) {
                    Ꮡt.Fatalf("te[%d][%d] = %#x, want %#x"u8, j, i, x, w);
                }
            }
            w = (uint32)((w << (int)(24)) | (w >> (int)(8)));
        }
    }
}

// Test that decryption tables are correct.
// (Can adapt this code to generate them too.)
public static void TestTd(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < 256; i++) {
        var s = (uint32)sbox1[i];
        var s9 = mul(s, 0x9);
        var sb = mul(s, 0xb);
        var sd = mul(s, 0xd);
        var se = mul(s, 0xe);
        var w = (uint32)((uint32)((uint32)((se << (int)(24)) | (s9 << (int)(16))) | (sd << (int)(8))) | sb);
        var td = GoReflect.WithElemDims(new array<uint32>[]{td0.Clone(), td1.Clone(), td2.Clone(), td3.Clone()}.slice(), 256);
        for (nint j = 0; j < 4; j++) {
            {
                var x = td[j][i]; if (x != w) {
                    Ꮡt.Fatalf("td[%d][%d] = %#x, want %#x"u8, j, i, x, w);
                }
            }
            w = (uint32)((w << (int)(24)) | (w >> (int)(8)));
        }
    }
}

} // end aes_internal_test_package
