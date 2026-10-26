// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !goexperiment.jsonv2
namespace go.encoding;

using bytes = bytes_package;
using testing = testing_package;
using static go.encoding.json_package;

partial class json_internal_test_package {

public static void FuzzEqualFold(ж<testing.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    foreach (var (_, vᴛ1) in GoReflect.WithElemDims(new array<@string>[]{
        new @string[]{""u8, ""u8}.array(),
        new @string[]{"123abc"u8, "123ABC"u8}.array(),
        new @string[]{"αβδ"u8, "ΑΒΔ"u8}.array(),
        new @string[]{"abc"u8, "xyz"u8}.array(),
        new @string[]{"abc"u8, "XYZ"u8}.array(),
        new @string[]{"1"u8, "2"u8}.array(),
        new @string[]{"hello, world!"u8, "hello, world!"u8}.array(),
        new @string[]{"hello, world!"u8, "Hello, World!"u8}.array(),
        new @string[]{"hello, world!"u8, "HELLO, WORLD!"u8}.array(),
        new @string[]{"hello, world!"u8, "jello, world!"u8}.array(),
        new @string[]{"γειά, κόσμε!"u8, "γειά, κόσμε!"u8}.array(),
        new @string[]{"γειά, κόσμε!"u8, "Γειά, Κόσμε!"u8}.array(),
        new @string[]{"γειά, κόσμε!"u8, "ΓΕΙΆ, ΚΌΣΜΕ!"u8}.array(),
        new @string[]{"γειά, κόσμε!"u8, "ΛΕΙΆ, ΚΌΣΜΕ!"u8}.array(),
        new @string[]{"AESKey"u8, "aesKey"u8}.array(),
        new @string[]{"AESKEY"u8, "aes_key"u8}.array(),
        new @string[]{"aes_key"u8, "AES_KEY"u8}.array(),
        new @string[]{"AES_KEY"u8, "aes-key"u8}.array(),
        new @string[]{"aes-key"u8, "AES-KEY"u8}.array(),
        new @string[]{"AES-KEY"u8, "aesKey"u8}.array(),
        new @string[]{"aesKey"u8, "AesKey"u8}.array(),
        new @string[]{"AesKey"u8, "AESKey"u8}.array(),
        new @string[]{"AESKey"u8, "aeskey"u8}.array(),
        new @string[]{"DESKey"u8, "aeskey"u8}.array(),
        new @string[]{"AES Key"u8, "aeskey"u8}.array()
    }.slice(), 2)) {
        var ss = vᴛ1.Clone();

        f.Add(slice<byte>(ss[0]), slice<byte>(ss[1]));
    }
    bool equalFold(slice<byte> x, slice<byte> y) => ((sstring)foldName(x)) == ((sstring)foldName(y));
    var equalFoldʗ1 = equalFold;
    Ꮡf.Fuzz((ж<testing.T> t, slice<byte> x, slice<byte> y) => {
        var got = equalFoldʗ1(x, y);
        var want = bytes.EqualFold(x, y);
        if (got != want) {
            t.Errorf("equalFold(%q, %q) = %v, want %v"u8, x, y, got, want);
        }
    });
}

} // end json_internal_test_package
