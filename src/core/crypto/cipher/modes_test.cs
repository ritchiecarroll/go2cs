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
    return new specialCTR();
}

public static void TestCTRAble(ж<testing.T> t) {
    var b = new block();
    var s = NewCTR(b, new slice<byte>(16));
    {
        var (_, ok) = s._<specialCTR>(ᐧ); if (!ok) {
            t.Errorf("NewCTR did not return specialCTR");
        }
    }
}

partial struct specialCBC {
    /*embed*/ public go.crypto.cipher_package.BlockMode BlockMode;
}

internal static cipher.BlockMode ΔNewCBCEncrypter(this block _, slice<byte> iv) {
    return new specialCBC();
}

internal static cipher.BlockMode ΔNewCBCDecrypter(this block _, slice<byte> iv) {
    return new specialCBC();
}

public static void TestCBCAble(ж<testing.T> t) {
    var b = new block();
    var s = NewCBCEncrypter(b, new slice<byte>(16));
    {
        var (_, ok) = s._<specialCBC>(ᐧ); if (!ok) {
            t.Errorf("NewCBCEncrypter did not return specialCBC");
        }
    }
    s = NewCBCDecrypter(b, new slice<byte>(16));
    {
        var (_, ok) = s._<specialCBC>(ᐧ); if (!ok) {
            t.Errorf("NewCBCDecrypter did not return specialCBC");
        }
    }
}

partial struct specialGCM {
    /*embed*/ public go.crypto.cipher_package.AEAD AEAD;
}

internal static (cipher.AEAD, error) ΔNewGCM(this block _, nint nonceSize, nint tagSize) {
    return (new specialGCM(), default);
}

public static void TestGCM(ж<testing.T> t) {
    var b = new block();
    var (s, err) = NewGCM(b);
    if (err != nil) {
        t.Errorf("NewGCM failed: %v", err);
    }
    {
        var (_, ok) = s._<specialGCM>(ᐧ); if (!ok) {
            t.Errorf("NewGCM did not return specialGCM");
        }
    }
}

// TestNoExtraMethods makes sure we don't accidentally expose methods on the
// underlying implementations of modes.
public static void TestNoExtraMethods(ж<testing.T> t) {
    testAllImplementations(t, testNoExtraMethods);
}

internal static void testNoExtraMethods(ж<testing.T> t, Func<slice<byte>, cipher.Block> newBlock) {
    var b = newBlock(new slice<byte>(16));

    var ctr = NewCTR(b, new slice<byte>(16));
    var ctrExpected = slice<@string>(["XORKeyStream"]);
    {
        var got = exportedMethods(ctr); if (!reflect.DeepEqual(got, ctrExpected)) {
            t.Errorf("CTR: got %v, want %v", got, ctrExpected);
        }
    }

    var cbc = NewCBCEncrypter(b, new slice<byte>(16));
    var cbcExpected = slice<@string>(["BlockSize", "CryptBlocks", "SetIV"]);
    {
        var got = exportedMethods(cbc); if (!reflect.DeepEqual(got, cbcExpected)) {
            t.Errorf("CBC: got %v, want %v", got, cbcExpected);
        }
    }
    cbc = NewCBCDecrypter(b, new slice<byte>(16));
    {
        var got = exportedMethods(cbc); if (!reflect.DeepEqual(got, cbcExpected)) {
            t.Errorf("CBC: got %v, want %v", got, cbcExpected);
        }
    }

    var (gcm, _) = NewGCM(b);
    var gcmExpected = slice<@string>(["NonceSize", "Open", "Overhead", "Seal"]);
    {
        var got = exportedMethods(gcm); if (!reflect.DeepEqual(got, gcmExpected)) {
            t.Errorf("GCM: got %v, want %v", got, gcmExpected);
        }
    }
}

internal static slice<@string> exportedMethods(any x) {
    var methodsᴷ = new stackAppend<@string>(moves: true);

    slice<@string> methods = default;
    var v = reflect.ValueOf(x);
    for (nint i = 0; i < v.NumMethod(); i++) {
        if (v.Type().Method(i).IsExported()) {
            methods = append(methodsᴷ, methods, v.Type().Method(i).Name);
        }
    }
    methods = move2heap(methodsᴷ, methods);
    return methods;
}

} // end cipher_test_package
