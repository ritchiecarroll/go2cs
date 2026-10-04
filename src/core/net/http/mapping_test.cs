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

public static void TestMapping(ж<testing.T> Ꮡt) {
    ref var m = ref heap(new global::go.net.http_package.mapping<nint, @string>(), out var Ꮡm);
    for (nint i = 0; i < maxSlice; i++) {
        m.add(i, strconv.Itoa(i));
    }
    if (m.m != default!) {
        Ꮡt.Fatal(mMNilˢ);
    }
    for (nint i = 0; i < maxSlice; i++) {
        var (gΔ1, _) = Ꮡm.find(i);
        @string w = strconv.Itoa(i);
        if (gΔ1 != w) {
            Ꮡt.Fatalf("%d: got %s, want %s"u8, i, gΔ1, w);
        }
    }
    m.add(4, "4"u8);
    if (m.s != default!) {
        Ꮡt.Fatal(mSNilˢ);
    }
    if (m.m == default!) {
        Ꮡt.Fatal(mMNilˢ2);
    }
    var (g, _) = Ꮡm.find(4);
    {
        @string w = "4"u8; if (g != w) {
            Ꮡt.Fatalf("got %s, want %s"u8, g, w);
        }
    }
}

public static void TestMappingEachPair(ж<testing.T> Ꮡt) {
    ref var m = ref heap(new global::go.net.http_package.mapping<nint, @string>(), out var Ꮡm);
    slice<global::go.net.http_package.entry<nint, @string>> want = default!;
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
        Ꮡt.Errorf("got %v, want %v"u8, got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string articlesˢ = "articles"u8;
internal static readonly @string repLinearˢ = "rep=linear"u8;
internal static readonly @string repMapˢ = "rep=map"u8;

public static void BenchmarkFindChild(ж<testing.B> Ꮡb) {
    @string key = articlesˢ;
    var children = new @string[]{
        "*"u8,
        "cmd.html"u8,
        "code.html"u8,
        "contrib.html"u8,
        "contribute.html"u8,
        "debugging_with_gdb.html"u8,
        "docs.html"u8,
        "effective_go.html"u8,
        "files.log"u8,
        "gccgo_contribute.html"u8,
        "gccgo_install.html"u8,
        "go-logo-black.png"u8,
        "go-logo-blue.png"u8,
        "go-logo-white.png"u8,
        "go1.1.html"u8,
        "go1.2.html"u8,
        "go1.html"u8,
        "go1compat.html"u8,
        "go_faq.html"u8,
        "go_mem.html"u8,
        "go_spec.html"u8,
        "help.html"u8,
        "ie.css"u8,
        "install-source.html"u8,
        "install.html"u8,
        "logo-153x55.png"u8,
        "Makefile"u8,
        "root.html"u8,
        "share.png"u8,
        "sieve.gif"u8,
        "tos.html"u8,
        "articles"u8
    }.slice();
    if (builtin.len(children) != 32) {
        throw panic("bad len");
    }
    foreach (var (_, n) in new nint[]{2, 4, 8, 16, 32}.slice()) {
        var list = children.slice(0, n);
        var listʗ1 = list;
        Ꮡb.Run(fmt.Sprintf("n=%d"u8, n), (ж<testing.B> bΔ1) => {
            var listʗ2 = listʗ1;
            bΔ1.Run(repLinearˢ, (ж<testing.B> bΔ2) => {
                slice<global::go.net.http_package.entry<@string, any>> entries = default!;
                foreach (var (_, c) in listʗ2) {
                    entries = append(entries, new entry<@string, any>(c, default!));
                }
                bΔ2.ResetTimer();
                for (nint i = 0; i < (~bΔ2).N; i++) {
                    findChildLinear(key, entries);
                }
            });
            var listʗ3 = listʗ1;
            bΔ1.Run(repMapˢ, (ж<testing.B> bΔ3) => {
                var m = new map<@string, any>{};
                foreach (var (_, c) in listʗ3) {
                    m[c] = default!;
                }
                any x = default!;
                bΔ3.ResetTimer();
                for (nint i = 0; i < (~bΔ3).N; i++) {
                    x = m[key];
                }
                _ = x;
            });
            var listʗ4 = listʗ1;
            bΔ1.Run(fmt.Sprintf("rep=hybrid%d"u8, maxSlice), (ж<testing.B> bΔ4) => {
                ref var h = ref heap(new global::go.net.http_package.mapping<@string, any>(), out var Ꮡh);
                foreach (var (_, c) in listʗ4) {
                    h.add(c, default!);
                }
                any x = default!;
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
    foreach (var (_, e) in entries) {
        if (key == e.key) {
            return e.value;
        }
    }
    return default!;
}

} // end http_internal_test_package
