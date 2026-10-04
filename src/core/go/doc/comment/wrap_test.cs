// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.doc;

using flag = flag_package;
using fmt = fmt_package;
using rand = math.rand_package;
using testing = testing_package;
using time = time_package;
using utf8 = global::go.unicode.utf8_package;
using global::go.unicode;
using math;
using static global::go.go.doc.comment_package;

partial class comment_internal_test_package {

internal static ж<int64> wrapSeed = flag.Int64("wrapseed"u8, 0, "use `seed` for wrap test (default auto-seeds)"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cdefghijklmnopqrstuvwxyzˢ = "1234567890αβcdefghijklmnopqrstuvwxyz"u8;
internal static readonly object maxˢ = (@string)"max="u8;

public static void TestWrap(ж<testing.T> Ꮡt) {
    if (wrapSeed.Value == 0) {
        wrapSeed.Value = time.Now().UnixNano();
    }
    Ꮡt.Logf("-wrapseed=%#x\n"u8, wrapSeed.Value);
    var r = rand.New(rand.NewSource(wrapSeed.Value));
    // Generate words of random length.
    @string s = cdefghijklmnopqrstuvwxyzˢ;
    nint sN = utf8.RuneCountInString(s);
    slice<@string> words = default!;
    for (nint i = 0; i < 100; i++) {
        nint n = 1 + r.Intn(sN - 1);
        if (n >= 12) {
            n++; // extra byte for β
        }
        if (n >= 11) {
            n++; // extra byte for α
        }
        words = append(words, s.slice(0, n));
    }
    for (nint nᴛ1 = 1; nᴛ1 <= len(words) && !Ꮡt.Failed(); nᴛ1++) {
        var n = nᴛ1;
        var wordsʗ1 = words;
        Ꮡt.Run(fmt.Sprint((@string)"n="u8, n), (ж<testing.T> tΔ1) => {
            var wordsΔ1 = wordsʗ1.slice(0, n);
            tΔ1.Logf("words: %v"u8, wordsΔ1);
            for (nint maxᴛ1 = 1; maxᴛ1 < 100 && !tΔ1.Failed(); maxᴛ1++) {
                var max = maxᴛ1;
                var wordsʗ2 = wordsΔ1;
                tΔ1.Run(fmt.Sprint(maxˢ, max), (ж<testing.T> tΔ2) => {
                    var seq = wrap(wordsʗ2, max);
                    // Compute score for seq.
                    nint start = 0;
                    var score = (int64)0;
                    if (len(seq) == 0) {
                        tΔ2.Fatalf("wrap seq is empty"u8);
                    }
                    if (seq[0] != 0) {
                        tΔ2.Fatalf("wrap seq does not start with 0"u8);
                    }
                    foreach (var (_, nΔ1) in seq[1..]) {
                        if (nΔ1 <= start) {
                            tΔ2.Fatalf("wrap seq is non-increasing: %v"u8, seq);
                        }
                        if (nΔ1 > len(wordsʗ2)) {
                            tΔ2.Fatalf("wrap seq contains %d > %d: %v"u8, nΔ1, len(wordsʗ2), seq);
                        }
                        nint size = -1;
                        foreach (var (_, sΔ1) in wordsʗ2.slice(start, nΔ1)) {
                            size += 1 + utf8.RuneCountInString(sΔ1);
                        }
                        if (nΔ1 - start == 1 && size >= max){
                        } else 
                        if (size > max){
                            // no score
                            tΔ2.Fatalf("wrap used overlong line %d:%d: %v"u8, start, nΔ1, wordsʗ2.slice(start, nΔ1));
                        } else 
                        if (nΔ1 != len(wordsʗ2)) {
                            score += (int64)(max - size) * (int64)(max - size) + wrapPenalty(wordsʗ2[nΔ1 - 1]);
                        }
                        start = nΔ1;
                    }
                    if (start != len(wordsʗ2)) {
                        tΔ2.Fatalf("wrap seq does not use all words (%d < %d): %v"u8, start, len(wordsʗ2), seq);
                    }
                    // Check that score matches slow reference implementation.
                    var (slowSeq, slowScore) = wrapSlow(wordsʗ2, max);
                    if (score != slowScore) {
                        tΔ2.Fatalf("wrap score = %d != wrapSlow score %d\nwrap: %v\nslow: %v"u8, score, slowScore, seq, slowSeq);
                    }
                });
            }
        });
    }
}

// wrapSlow is an O(n²) reference implementation for wrap.
// It returns a minimal-score sequence along with the score.
// It is OK if wrap returns a different sequence as long as that
// sequence has the same score.
internal static (slice<nint> seq, int64 score) wrapSlow(slice<@string> words, nint max) {
    slice<nint> seq = default!;

    // Quadratic dynamic programming algorithm for line wrapping problem.
    // best[i] tracks the best score possible for words[:i],
    // assuming that for i < len(words) the line breaks after those words.
    // bestleft[i] tracks the previous line break for best[i].
    var best = new slice<int64>(len(words) + 1);
    var bestleft = new slice<nint>(len(words) + 1);
    best[0] = 0;
    foreach (var (i, w) in words) {
        if (utf8.RuneCountInString(w) >= max) {
            // Overlong word must appear on line by itself. No effect on score.
            best[i + 1] = best[i];
            continue;
        }
        best[i + 1] = 1000000000000000000;
        var p = wrapPenalty(w);
        nint nΔ1 = -1;
        for (nint j = i; j >= 0; j--) {
            nΔ1 += 1 + utf8.RuneCountInString(words[j]);
            if (nΔ1 > max) {
                break;
            }
            var line = (int64)(nΔ1 - max) * (int64)(nΔ1 - max) + p;
            if (i == len(words) - 1) {
                line = 0; // no score for final line being too short
            }
            var s = best[j] + line;
            if (best[i + 1] > s) {
                best[i + 1] = s;
                bestleft[i + 1] = j;
            }
        }
    }
    // Recover least weight sequence from bestleft.
    nint n = 1;
    for (nint m = len(words); m > 0; m = bestleft[m]) {
        n++;
    }
    seq = new slice<nint>(n);
    for (nint m = len(words); m > 0; m = bestleft[m]) {
        n--;
        seq[n] = m;
    }
    return (seq, best[len(words)]);
}

} // end comment_internal_test_package
