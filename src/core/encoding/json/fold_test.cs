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
        new @string[]{"", ""}.array(),
        new @string[]{"123abc", "123ABC"}.array(),
        new @string[]{"αβδ", "ΑΒΔ"}.array(),
        new @string[]{"abc", "xyz"}.array(),
        new @string[]{"abc", "XYZ"}.array(),
        new @string[]{"1", "2"}.array(),
        new @string[]{"hello, world!", "hello, world!"}.array(),
        new @string[]{"hello, world!", "Hello, World!"}.array(),
        new @string[]{"hello, world!", "HELLO, WORLD!"}.array(),
        new @string[]{"hello, world!", "jello, world!"}.array(),
        new @string[]{"γειά, κόσμε!", "γειά, κόσμε!"}.array(),
        new @string[]{"γειά, κόσμε!", "Γειά, Κόσμε!"}.array(),
        new @string[]{"γειά, κόσμε!", "ΓΕΙΆ, ΚΌΣΜΕ!"}.array(),
        new @string[]{"γειά, κόσμε!", "ΛΕΙΆ, ΚΌΣΜΕ!"}.array(),
        new @string[]{"AESKey", "aesKey"}.array(),
        new @string[]{"AESKEY", "aes_key"}.array(),
        new @string[]{"aes_key", "AES_KEY"}.array(),
        new @string[]{"AES_KEY", "aes-key"}.array(),
        new @string[]{"aes-key", "AES-KEY"}.array(),
        new @string[]{"AES-KEY", "aesKey"}.array(),
        new @string[]{"aesKey", "AesKey"}.array(),
        new @string[]{"AesKey", "AESKey"}.array(),
        new @string[]{"AESKey", "aeskey"}.array(),
        new @string[]{"DESKey", "aeskey"}.array(),
        new @string[]{"AES Key", "aeskey"}.array()
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
            t.Errorf("equalFold(%q, %q) = %v, want %v", x, y, got, want);
        }
    });
}

} // end json_internal_test_package
