// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using cmp = cmp_package;
using fmt = fmt_package;
using slices = slices_package;
using strconv = strconv_package;
using testing = testing_package;
using static global::go.net.http_package;

partial class http_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object mMNilˢ = (@string)"m.m != nil"u8;
internal static readonly object mSNilˢ = (@string)"m.s != nil"u8;
internal static readonly object mMNilˢ2 = (@string)"m.m == nil"u8;

public static void TestMapping(ж<testing.T> t) {
    ref var m = ref heap(new global::go.net.http_package.mapping<nint, @string>(), out var Ꮡm);
    for (nint i = 0; i < maxSlice; i++) {
        m.add(i, strconv.Itoa(i));
    }
    if (m.m != nil) {
        t.Fatal(mMNilˢ);
    }
    for (nint i = 0; i < maxSlice; i++) {
        var (gΔ1, _) = Ꮡm.find(i);
        @string w = strconv.Itoa(i);
        if (gΔ1 != w) {
            t.Fatalf("%d: got %s, want %s", i, gΔ1, w);
        }
    }
    m.add(4, (@string)"4");
    if (m.s != nil) {
        t.Fatal(mSNilˢ);
    }
    if (m.m == nil) {
        t.Fatal(mMNilˢ2);
    }
    var (g, _) = Ꮡm.find(4);
    {
        @string w = "4"; if (g != w) {
            t.Fatalf("got %s, want %s", g, w);
        }
    }
}

public static void TestMappingEachPair(ж<testing.T> t) {
    ref var m = ref heap(new global::go.net.http_package.mapping<nint, @string>(), out var Ꮡm);
    slice<global::go.net.http_package.entry<nint, @string>> want = default;
    for (nint i = 0; i < maxSlice * 2; i++) {
        @string v = strconv.Itoa(i);
        m.add(i, v);
        want = append(want, new entry<nint, @string>(i, v));
    }

    ref var got = ref heap<slice<global::go.net.http_package.entry<nint, @string>>>(out var Ꮡgot);
    Ꮡm.eachPair((nint k, @string v) => {
        Ꮡgot.ValueSlot = append(Ꮡgot.ValueSlot, new entry<nint, @string>(k, v));
        return true;
    });
    slices.SortFunc(got, (global::go.net.http_package.entry<nint, @string> e1, global::go.net.http_package.entry<nint, @string> e2) => cmp.Compare(e1.key, e2.key));
    if (!slices.Equal<slice<global::go.net.http_package.entry<nint, @string>>, global::go.net.http_package.entry<nint, @string>>(got, want)) {
        t.Errorf("got %v, want %v", got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string articlesˢ => ref articlesˢᵇ;
private static readonly @string articlesˢᵇ = "articles"u8;
internal static ref readonly @string repLinearˢ => ref repLinearˢᵇ;
private static readonly @string repLinearˢᵇ = "rep=linear"u8;
internal static ref readonly @string repMapˢ => ref repMapˢᵇ;
private static readonly @string repMapˢᵇ = "rep=map"u8;

public static void BenchmarkFindChild(ж<testing.B> b) {
    @string key = articlesˢ;
    var children = slice<@string>([
        "*",
        "cmd.html",
        "code.html",
        "contrib.html",
        "contribute.html",
        "debugging_with_gdb.html",
        "docs.html",
        "effective_go.html",
        "files.log",
        "gccgo_contribute.html",
        "gccgo_install.html",
        "go-logo-black.png",
        "go-logo-blue.png",
        "go-logo-white.png",
        "go1.1.html",
        "go1.2.html",
        "go1.html",
        "go1compat.html",
        "go_faq.html",
        "go_mem.html",
        "go_spec.html",
        "help.html",
        "ie.css",
        "install-source.html",
        "install.html",
        "logo-153x55.png",
        "Makefile",
        "root.html",
        "share.png",
        "sieve.gif",
        "tos.html",
        "articles"
    ]);
    if (builtin.len(children) != 32) {
        throw panic("bad len");
    }
    foreach (var (_, n) in range(slice<nint>([2, 4, 8, 16, 32]))) {
        var list = children.slice(0, n);
        var listʗ1 = list;
        b.Run(fmt.Sprintf("n=%d"u8, n), (ж<testing.B> bΔ1) => {
            var listʗ2 = listʗ1;
            bΔ1.Run(repLinearˢ, (ж<testing.B> bΔ2) => {
                var entriesᴷ = new stackAppend<global::go.net.http_package.entry<@string, any>>();
                slice<global::go.net.http_package.entry<@string, any>> entries = default;
                foreach (var (_, c) in range(listʗ2)) {
                    entries = append(entriesᴷ, entries, new entry<@string, any>(c, default));
                }
                bΔ2.ResetTimer();
                for (nint i = 0; i < (~bΔ2).N; i++) {
                    findChildLinear(key, entries);
                }
            });
            var listʗ3 = listʗ1;
            bΔ1.Run(repMapˢ, (ж<testing.B> bΔ3) => {
                var m = new map<@string, any>{};
                foreach (var (_, c) in range(listʗ3)) {
                    m[c] = default;
                }
                any x = default;
                bΔ3.ResetTimer();
                for (nint i = 0; i < (~bΔ3).N; i++) {
                    x = m[key];
                }
                _ = x;
            });
            var listʗ4 = listʗ1;
            bΔ1.Run(fmt.Sprintf("rep=hybrid%d"u8, maxSlice), (ж<testing.B> bΔ4) => {
                ref var h = ref heap(new global::go.net.http_package.mapping<@string, any>(), out var Ꮡh);
                foreach (var (_, c) in range(listʗ4)) {
                    h.add(c, default);
                }
                any x = default;
                bΔ4.ResetTimer();
                for (nint i = 0; i < (~bΔ4).N; i++) {
                    (x, _) = Ꮡh.find(key);
                }
                _ = x;
            });
        });
    }
}

internal static any findChildLinear(@string key, slice<global::go.net.http_package.entry<@string, any>> entries) {
    foreach (var (_, e) in range(entries)) {
        if (key == e.key) {
            return e.value;
        }
    }
    return default;
}

} // end http_internal_test_package
