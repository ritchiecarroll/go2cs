// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using fmt = fmt_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using io = io_package;
using static global::go.net.http_package;

partial class http_internal_test_package {

public static void TestIndex(ж<testing.T> Ꮡt) {
    // Generate every kind of pattern up to some number of segments,
    // and compare conflicts found during indexing with those found
    // by exhaustive comparison.
    var patterns = generatePatterns();
    ref var idx = ref heap(new global::go.net.http_package.routingIndex(), out var Ꮡidx);
    foreach (var (i, pat) in patterns) {
        var got = indexConflicts(pat, Ꮡidx);
        var want = trueConflicts(pat, patterns.slice(0, i));
        if (!slices.Equal<slice<@string>, @string>(got, want)) {
            Ꮡt.Fatalf("%q:\ngot  %q\nwant %q"u8, pat.OrTypedNil(), got, want);
        }
        idx.addPattern(pat);
    }
}

internal static slice<@string> trueConflicts(ж<global::go.net.http_package.pattern> Ꮡpat, slice<ж<global::go.net.http_package.pattern>> pats) {
    ref var pat = ref Ꮡpat.DerefOrNull();

    slice<@string> s = default!;
    foreach (var (_, p) in pats) {
        if (pat.conflictsWith(p)) {
            s = append(s, p.String());
        }
    }
    slices.Sort<slice<@string>, @string>(s);
    return s;
}

internal static slice<@string> indexConflicts(ж<global::go.net.http_package.pattern> Ꮡpat, ж<global::go.net.http_package.routingIndex> Ꮡidx) {
    ref var idx = ref Ꮡidx.DerefOrNull();

    ref var s = ref heap<slice<@string>>(out var Ꮡs);
    idx.possiblyConflictingPatterns(Ꮡpat, (ж<global::go.net.http_package.pattern> p) => {
        if (Ꮡpat.Value.conflictsWith(p)) {
            Ꮡs.ValueSlot = append(Ꮡs.ValueSlot, p.String());
        }
        return default!;
    });
    slices.Sort<slice<@string>, @string>(s);
    return slices.Compact<slice<@string>, @string>(s);
}

// generatePatterns generates all possible patterns using a representative
// sample of parts.
internal static slice<ж<global::go.net.http_package.pattern>> generatePatterns() {
    ref var pats = ref heap<slice<ж<global::go.net.http_package.pattern>>>(out var Ꮡpats);
    var collect = (@string s) => {
        // Replace duplicate wildcards with unique ones.
        ref var b = ref heap(new strings.Builder(), out var Ꮡb);
        nint wc = 0;
        while (ᐧ) {
            nint i = strings.Index(s, "{x}"u8);
            if (i < 0) {
                Ꮡb.WriteString(s);
                break;
            }
            Ꮡb.WriteString(s.slice(0, i));
            fmt.Fprintf(new http_test_package.strings_BuilderжWriter(Ꮡb), "{x%d}"u8, wc);
            wc++;
            s = s.slice(i + 3);
        }
        var (pat, err) = parsePattern(b.String());
        if (err != default!) {
            throw panic(err);
        }
        Ꮡpats.ValueSlot = append(Ꮡpats.ValueSlot, pat);
    };
    slice<@string> methods = new @string[]{""u8, "GET "u8, "HEAD "u8, "POST "u8}.slice();
    slice<@string> hosts = new @string[]{""u8, "h1"u8, "h2"u8}.slice();
    slice<@string> segs = new @string[]{"/a"u8, "/b"u8, "/{x}"u8}.slice();
    slice<@string> finalSegs = new @string[]{"/a"u8, "/b"u8, "/{f}"u8, "/{m...}"u8, "/{$}"u8}.slice();
    var g = genConcat(
        genChoice(methods),
        genChoice(hosts),
        genStar(3, genChoice(segs)),
        genChoice(finalSegs));
    g(collect);
    return pats;
}

// type generator is a methodless func type — rendered inline as its base delegate

// genConst generates a single constant string.
internal static Action<Action<@string>> genConst(@string s) {
    return (Action<@string> collect) => {
        collect(s);
    };
}

// genChoice generates all the strings in its argument.
internal static Action<Action<@string>> genChoice(slice<@string> choices) {
    var choicesʗ1 = choices;
    return (Action<@string> collect) => {
        foreach (var (_, c) in choicesʗ1) {
            collect(c);
        }
    };
}

// genConcat2 generates the cross product of the strings of g1 concatenated
// with those of g2.
internal static Action<Action<@string>> genConcat2(Action<Action<@string>> g1, Action<Action<@string>> g2) {
    return (Action<@string> collect) => {
        g1((@string s1) => {
            g2((@string s2) => {
                collect(s1 + s2);
            });
        });
    };
}

// genConcat generalizes genConcat2 to any number of generators.
internal static Action<Action<@string>> genConcat(params Span<Action<Action<@string>>> gsʗp) {
    var gs = gsʗp.slice();

    if (builtin.len(gs) == 0) {
        return genConst(""u8);
    }
    return genConcat2(gs[0], genConcat(gs[1..].ꓸꓸꓸ));
}

// genRepeat generates strings of exactly n copies of g's strings.
internal static Action<Action<@string>> genRepeat(nint n, Action<Action<@string>> g) {
    if (n == 0) {
        return genConst(""u8);
    }
    return genConcat(g, genRepeat(n - 1, g));
}

// genStar (named after the Kleene star) generates 0, 1, 2, ..., max
// copies of the strings of g.
internal static Action<Action<@string>> genStar(nint max, Action<Action<@string>> g) {
    return (Action<@string> collect) => {
        for (nint i = 0; i <= max; i++) {
            genRepeat(i, g)(collect);
        }
    };
}

public static void BenchmarkMultiConflicts(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    // How fast is indexing if the corpus is all multis?
    const nint nMultis = 1000;
    slice<ж<global::go.net.http_package.pattern>> pats = default!;
    for (nint i = 0; i < nMultis; i++) {
        pats = append(pats, mustParsePattern(new http_test_package.testing_BжTB(Ꮡb), fmt.Sprintf("/a/b/{x}/d%d/"u8, i)));
    }
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        ref var idx = ref heap(new global::go.net.http_package.routingIndex(), out var Ꮡidx);
        foreach (var (_, p) in pats) {
            var got = indexConflicts(p, Ꮡidx);
            if (builtin.len(got) != 0) {
                Ꮡb.Fatalf("got %d conflicts, want 0"u8, builtin.len(got));
            }
            idx.addPattern(p);
        }
        if (i == 0) {
            // Confirm that all the multis ended up where they belong.
            {
                nint g = builtin.len(idx.multis);
                nint w = nMultis; if (g != w) {
                    Ꮡb.Fatalf("got %d multis, want %d"u8, g, w);
                }
            }
        }
    }
}

} // end http_internal_test_package
