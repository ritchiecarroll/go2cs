// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using static go.crypto.cipher_package;
using reflect = reflect_package;
using testing = testing_package;
using cipher = go.crypto.cipher_package;

partial class cipher_test_package {

// Historically, crypto/aes's Block would implement some undocumented
// methods for crypto/cipher to use from NewCTR, NewCBCEncrypter, etc.
// This is no longer the case, but for now test that the mechanism is
// still working until we explicitly decide to remove it.
partial struct block {
    /*embed*/ public go.crypto.cipher_package.Block Block;
}

internal static nint BlockSize(this block _) {
    return 16;
}

partial struct specialCTR {
    /*embed*/ public go.crypto.cipher_package.Stream Stream;
}

internal static cipher.Stream ΔNewCTR(this block _, slice<byte> iv) {
    return new specialCTR(nil);
}

public static void TestCTRAble(ж<testing.T> Ꮡt) {
    var b = new block(nil);
    var s = NewCTR(b, new slice<byte>(16));
    {
        var (_, ok) = s._<specialCTR>(ᐧ); if (!ok) {
            Ꮡt.Errorf("NewCTR did not return specialCTR"u8);
        }
    }
}

partial struct specialCBC {
    /*embed*/ public go.crypto.cipher_package.BlockMode BlockMode;
}

internal static cipher.BlockMode ΔNewCBCEncrypter(this block _, slice<byte> iv) {
    return new specialCBC(nil);
}

internal static cipher.BlockMode ΔNewCBCDecrypter(this block _, slice<byte> iv) {
    return new specialCBC(nil);
}

public static void TestCBCAble(ж<testing.T> Ꮡt) {
    var b = new block(nil);
    var s = NewCBCEncrypter(b, new slice<byte>(16));
    {
        var (_, ok) = s._<specialCBC>(ᐧ); if (!ok) {
            Ꮡt.Errorf("NewCBCEncrypter did not return specialCBC"u8);
        }
    }
    s = NewCBCDecrypter(b, new slice<byte>(16));
    {
        var (_, ok) = s._<specialCBC>(ᐧ); if (!ok) {
            Ꮡt.Errorf("NewCBCDecrypter did not return specialCBC"u8);
        }
    }
}

partial struct specialGCM {
    /*embed*/ public go.crypto.cipher_package.AEAD AEAD;
}

internal static (cipher.AEAD, error) ΔNewGCM(this block _, nint nonceSize, nint tagSize) {
    return (new specialGCM(nil), default!);
}

public static void TestGCM(ж<testing.T> Ꮡt) {
    var b = new block(nil);
    var (s, err) = NewGCM(b);
    if (err != default!) {
        Ꮡt.Errorf("NewGCM failed: %v"u8, err);
    }
    {
        var (_, ok) = s._<specialGCM>(ᐧ); if (!ok) {
            Ꮡt.Errorf("NewGCM did not return specialGCM"u8);
        }
    }
}

// TestNoExtraMethods makes sure we don't accidentally expose methods on the
// underlying implementations of modes.
public static void TestNoExtraMethods(ж<testing.T> Ꮡt) {
    testAllImplementations(Ꮡt, testNoExtraMethods);
}

internal static void testNoExtraMethods(ж<testing.T> Ꮡt, Func<slice<byte>, cipher.Block> newBlock) {
    var b = newBlock(new slice<byte>(16));
    var ctr = NewCTR(b, new slice<byte>(16));
    var ctrExpected = new @string[]{"XORKeyStream"u8}.slice();
    {
        var got = exportedMethods(ctr); if (!reflect.DeepEqual(got, ctrExpected)) {
            Ꮡt.Errorf("CTR: got %v, want %v"u8, got, ctrExpected);
        }
    }
    var cbc = NewCBCEncrypter(b, new slice<byte>(16));
    var cbcExpected = new @string[]{"BlockSize"u8, "CryptBlocks"u8, "SetIV"u8}.slice();
    {
        var got = exportedMethods(cbc); if (!reflect.DeepEqual(got, cbcExpected)) {
            Ꮡt.Errorf("CBC: got %v, want %v"u8, got, cbcExpected);
        }
    }
    cbc = NewCBCDecrypter(b, new slice<byte>(16));
    {
        var got = exportedMethods(cbc); if (!reflect.DeepEqual(got, cbcExpected)) {
            Ꮡt.Errorf("CBC: got %v, want %v"u8, got, cbcExpected);
        }
    }
    var (gcm, _) = NewGCM(b);
    var gcmExpected = new @string[]{"NonceSize"u8, "Open"u8, "Overhead"u8, "Seal"u8}.slice();
    {
        var got = exportedMethods(gcm); if (!reflect.DeepEqual(got, gcmExpected)) {
            Ꮡt.Errorf("GCM: got %v, want %v"u8, got, gcmExpected);
        }
    }
}

internal static slice<@string> exportedMethods(any x) {
    slice<@string> methods = default!;
    var v = reflect.ValueOf(x);
    for (nint i = 0; i < v.NumMethod(); i++) {
        if (v.Type().Method(i).IsExported()) {
            methods = append(methods, v.Type().Method(i).Name);
        }
    }
    return methods;
}

} // end cipher_test_package
