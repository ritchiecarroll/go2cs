// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using testing = testing_package;
using utf8 = unicode.utf8_package;
using static go.html_package;
using unicode;

partial class html_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object mapsNotLoadedˢ = (@string)"maps not loaded"u8;

public static void TestEntityLength(ж<testing.T> Ꮡt) {
    var (entity, entity2) = entityMaps();

    if (len(entity) == 0 || len(entity2) == 0) {
        Ꮡt.Fatal(mapsNotLoadedˢ);
    }

    // We verify that the length of UTF-8 encoding of each value is <= 1 + len(key).
    // The +1 comes from the leading "&". This property implies that the length of
    // unescaped text is <= the length of escaped text.
    foreach (var (k, v) in entity) {
        if (1 + len(k) < utf8.RuneLen(v)) {
            Ꮡt.Error("escaped entity &" + k + " is shorter than its UTF-8 encoding " + ((@string)v));
        }
        if (len(k) > longestEntityWithoutSemicolon && k[len(k) - 1] != (rune)';') {
            Ꮡt.Errorf("entity name %s is %d characters, but longestEntityWithoutSemicolon=%d"u8, k, len(k), (nint)(longestEntityWithoutSemicolon));
        }
    }
    foreach (var (k, vᴛ1) in entity2) {
        var v = vᴛ1.Clone();

        if (1 + len(k) < utf8.RuneLen(v[0]) + utf8.RuneLen(v[1])) {
            Ꮡt.Error("escaped entity &" + k + " is shorter than its UTF-8 encoding " + ((@string)v[0]) + ((@string)v[1]));
        }
    }
}

} // end html_internal_test_package
