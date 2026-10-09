// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using rand = go.crypto.rand_package;
using fmt = fmt_package;
using testing = testing_package;
using go.crypto;
using static go.crypto.rand_internal_test_package;

partial class rand_test_package {

public static void TestText(ж<testing.T> Ꮡt) {
    var set = new map<@string, EmptyStruct>(); // hold every string produced
    array<map<rune, nint>> indexSet = new(26);                             // hold every char produced at every position
    foreach (var (i, _) in indexSet) {
        indexSet[i] = new map<rune, nint>();
    }
    // not getting a char in a position: (31/32)¹⁰⁰⁰ = 1.6e-14
    // test completion within 1000 rounds: (1-(31/32)¹⁰⁰⁰)²⁶ = 0.9999999999996
    // empirically, this should complete within 400 rounds = 0.999921
    nint rounds = 1000;
    bool done = default!;
    foreach (var _ᴛ1 in range(rounds)) {
        @string s = rand.Text();
        if (len(s) != 26) {
            Ꮡt.Errorf("len(Text()) = %d, want = 26"u8, len(s));
        }
        foreach (var (i, r) in s) {
            if (((rune)'A' > r || r > (rune)'Z') && ((rune)'2' > r || r > (rune)'7')) {
                Ꮡt.Errorf("Text()[%d] = %v, outside of base32 alphabet"u8, i, r);
            }
        }
        {
            var (_, ok) = set[s, ꟷ]; if (ok) {
                Ꮡt.Errorf("Text() = %s, duplicate of previously produced string"u8, s);
            }
        }
        set[s] = new EmptyStruct();
        done = true;
        foreach (var (i, r) in s) {
            indexSet[i][r]++;
            if (len(indexSet[i]) != 32) {
                done = false;
            }
        }
        if (done) {
            break;
        }
    }
    if (!done) {
        Ꮡt.Errorf("failed to produce every char at every index after %d rounds"u8, rounds);
        indexSetTable(Ꮡt, indexSet);
    }
}

internal static void indexSetTable(ж<testing.T> Ꮡt, /*[26]*/ array<map<rune, nint>> indexSet) {
    indexSet = indexSet.Clone();

    @string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567"u8;
    @string line = "   "u8;
    foreach (var (_, r) in alphabet) {
        line += fmt.Sprintf(" %3s"u8, ((@string)r));
    }
    Ꮡt.Log(line);
    foreach (var (i, set) in indexSet.ΔRangeSnapshot()) {
        line = fmt.Sprintf("%2d:"u8, i);
        foreach (var (_, r) in alphabet) {
            line += fmt.Sprintf(" %3d"u8, set[r]);
        }
        Ꮡt.Log(line);
    }
}

} // end rand_test_package
