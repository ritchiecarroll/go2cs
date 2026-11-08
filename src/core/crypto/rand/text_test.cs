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

public static void TestText(ж<testing.T> t) {
    var set = new map<@string, EmptyStruct>(); // hold every string produced
    array<map<rune, nint>> indexSet = new(26); // hold every char produced at every position
    foreach (var i in range(len(indexSet))) {
        indexSet[i] = new map<rune, nint>();
    }

    // not getting a char in a position: (31/32)¹⁰⁰⁰ = 1.6e-14
    // test completion within 1000 rounds: (1-(31/32)¹⁰⁰⁰)²⁶ = 0.9999999999996
    // empirically, this should complete within 400 rounds = 0.999921
    nint rounds = 1000;
    bool done = default;
    foreach (var _ᴛ1 in range(rounds)) {
        @string s = rand.Text();
        if (len(s) != 26) {
            t.Errorf("len(Text()) = %d, want = 26", len(s));
        }
        foreach (var (i, r) in s) {
            if (('A' > r || r > 'Z') && ('2' > r || r > '7')) {
                t.Errorf("Text()[%d] = %v, outside of base32 alphabet", i, r);
            }
        }
        var (_, ok) = set[s, ꟷ]; if (ok) {
            t.Errorf("Text() = %s, duplicate of previously produced string", s);
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
        t.Errorf("failed to produce every char at every index after %d rounds", rounds);
        indexSetTable(t, indexSet);
    }
}

internal static void indexSetTable(ж<testing.T> t, /*[26]*/ array<map<rune, nint>> indexSet) {
    indexSet = indexSet.Clone();

    @string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
    @string line = "   ";
    foreach (var (_, r) in alphabet) {
        line += fmt.Sprintf(" %3s"u8, (@string)r);
    }
    t.Log(line);
    foreach (var (i, set) in range(indexSet.ΔRangeSnapshot())) {
        line = fmt.Sprintf("%2d:"u8, i);
        foreach (var (_, r) in alphabet) {
            line += fmt.Sprintf(" %3d"u8, set[r]);
        }
        t.Log(line);
    }
}

} // end rand_test_package
