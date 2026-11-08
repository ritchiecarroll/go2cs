// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Tests of internal functions and things with no better homes.
namespace go.net;

using bytes = bytes_package;
using testenv = global::go.@internal.testenv_package;
using fs = global::go.io.fs_package;
using url = global::go.net.url_package;
using os = os_package;
using regexp = regexp_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using exec = global::go.os.exec_package;
using global::go.@internal;
using global::go.io;
using global::go.net;
using global::go.os;
using static global::go.net.http_package;

partial class http_internal_test_package {

internal partial struct TestForeachHeaderElement_tests /*dyn*/ {
    internal @string @in;
    internal slice<@string> want;
}

public static void TestForeachHeaderElement(ж<testing.T> t) {
    var tests = slice<TestForeachHeaderElement_tests>([
        new("Foo", ["Foo"]),
        new(" Foo", ["Foo"]),
        new("Foo ", ["Foo"]),
        new(" Foo ", ["Foo"]),

        new("foo", ["foo"]),
        new("anY-cAsE", ["anY-cAsE"]),

        new("", default),
        new(",,,,  ,  ,,   ,,, ,", default),

        new(" Foo,Bar, Baz,lower,,Quux ", ["Foo", "Bar", "Baz", "lower", "Quux"])
    ]);
    foreach (var (_, tt) in range(tests)) {
        ref var got = ref heap<slice<@string>>(out var Ꮡgot);
        foreachHeaderElement(tt.@in, (@string v) => {
            Ꮡgot.ValueSlot = append(Ꮡgot.ValueSlot, v);
        });
        if (!slices.Equal<slice<@string>, @string>(got, tt.want)) {
            t.Errorf("foreachHeaderElement(%q) = %q; want %q", tt.@in, got, tt.want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string toolˢ => ref toolˢᵇ;
private static readonly @string toolˢᵇ = "tool"u8;

// Test that cmd/go doesn't link in the HTTP server.
//
// This catches accidental dependencies between the HTTP transport and
// server code.
public static void TestCmdGoNoHTTPServer(ж<testing.T> t) {
    t.Parallel();
    @string goBin = testenv.GoToolPath(new http_test_package.testing_TжTB(t));
    var (@out, err) = testenv.Command(new http_test_package.testing_TжTB(t), goBin, toolˢ, "nm", goBin).CombinedOutput();
    if (err != nil) {
        t.Fatalf("go tool nm: %v: %s", err, @out);
    }
    var wantSym = new map<@string, bool>{
        // Verify these exist: (sanity checking this test)
        ["net/http.(*Client).do"] = true,
        ["net/http.(*Transport).RoundTrip"] = true,

        // Verify these don't exist:
        ["net/http.http2Server"] = false,
        ["net/http.(*Server).Serve"] = false,
        ["net/http.(*ServeMux).ServeHTTP"] = false,
        ["net/http.DefaultServeMux"] = false
    };
    foreach (var (sym, want) in wantSym) {
        var got = bytes.Contains(@out, slice<byte>(sym));
        if (!want && got) {
            t.Errorf("cmd/go unexpectedly links in HTTP server code; found symbol %q in cmd/go", sym);
        }
        if (want && !got) {
            t.Errorf("expected to find symbol %q in cmd/go; not found", sym);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"skipping in short mode"u8;
internal static ref readonly @string testˢ => ref testˢᵇ;
private static readonly @string testˢᵇ = "test"u8;
internal static ref readonly @string shortˢ => ref shortˢᵇ;
private static readonly @string shortˢᵇ = "-short"u8;
internal static ref readonly @string tagsNethttpomithttp2ˢ => ref tagsNethttpomithttp2ˢᵇ;
private static readonly @string tagsNethttpomithttp2ˢᵇ = "-tags=nethttpomithttp2"u8;
internal static ref readonly @string netHttpˢ2 => ref netHttpˢ2ᵇ;
private static readonly @string netHttpˢ2ᵇ = "net/http"u8;

// Tests that the nethttpomithttp2 build tag doesn't rot too much,
// even if there's not a regular builder on it.
public static void TestOmitHTTP2(ж<testing.T> t) {
    if (testing.Short()) {
        t.Skip(skippingInShortModeˢ);
    }
    t.Parallel();
    @string goTool = testenv.GoToolPath(new http_test_package.testing_TжTB(t));
    var (@out, err) = testenv.Command(new http_test_package.testing_TжTB(t), goTool, testˢ, shortˢ, tagsNethttpomithttp2ˢ, netHttpˢ2).CombinedOutput();
    if (err != nil) {
        t.Fatalf("go test -short failed: %v, %s", err, @out);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string vetˢ => ref vetˢᵇ;
private static readonly @string vetˢᵇ = "vet"u8;

// Tests that the nethttpomithttp2 build tag at least type checks
// in short mode.
// The TestOmitHTTP2 test above actually runs tests (in long mode).
public static void TestOmitHTTP2Vet(ж<testing.T> t) {
    t.Parallel();
    @string goTool = testenv.GoToolPath(new http_test_package.testing_TжTB(t));
    var (@out, err) = testenv.Command(new http_test_package.testing_TжTB(t), goTool, vetˢ, tagsNethttpomithttp2ˢ, netHttpˢ2).CombinedOutput();
    if (err != nil) {
        t.Fatalf("go vet failed: %v, %s", err, @out);
    }
}

internal static nint valuesCount;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object benchmarkWasnTRunˢ = (@string)"Benchmark wasn't run"u8;

public static void BenchmarkCopyValues(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var src = new url.Values(new map<@string, slice<@string>>{
        ["a"] = ["1", "2", "3", "4", "5"],
        ["b"] = ["2", "2", "3", "4", "5"],
        ["c"] = ["3", "2", "3", "4", "5"],
        ["d"] = ["4", "2", "3", "4", "5"],
        ["e"] = ["1", "1", "2", "3", "4", "5", "6", "7", "abcdef", "l", "a", "b", "c", "d", "z"],
        ["j"] = ["1", "2"],
        ["m"] = default
    });
    for (nint i = 0; i < b.N; i++) {
        var dst = new url.Values(new map<@string, slice<@string>>{["a"] = ["b"], ["b"] = ["2"], ["c"] = ["3"], ["d"] = ["4"], ["j"] = default, ["m"] = ["x"]});
        copyValues(dst, src);
        http_internal_test_package.valuesCount = builtin.len(dst["a"]); if (http_internal_test_package.valuesCount != 6) {
            Ꮡb.Fatalf(@"%d items in dst[""a""] but expected 6", http_internal_test_package.valuesCount);
        }
    }
    if (http_internal_test_package.valuesCount == 0) {
        Ꮡb.Fatal(benchmarkWasnTRunˢ);
    }
}

internal static map<@string, bool> forbiddenStringsFunctions = new map<@string, bool>{
    // Functions that use Unicode-aware case folding.
    ["EqualFold"] = true,
    ["Title"] = true,
    ["ToLower"] = true,
    ["ToLowerSpecial"] = true,
    ["ToTitle"] = true,
    ["ToTitleSpecial"] = true,
    ["ToUpper"] = true,
    ["ToUpperSpecial"] = true,

    // Functions that use Unicode-aware spaces.
    ["Fields"] = true,
    ["TrimSpace"] = true
};

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string stringsBytesAZaZˢ => ref stringsBytesAZaZˢᵇ;
private static readonly @string stringsBytesAZaZˢᵇ = @"(strings|bytes).([A-Za-z]+)"u8;
internal static ref readonly @string testGoˢ => ref testGoˢᵇ;
private static readonly @string testGoˢᵇ = "_test.go"u8;

// TestNoUnicodeStrings checks that nothing in net/http uses the Unicode-aware
// strings and bytes package functions. HTTP is mostly ASCII based, and doing
// Unicode-aware case folding or space stripping can introduce vulnerabilities.
public static void TestNoUnicodeStrings(ж<testing.T> t) {
    testenv.MustHaveSource(new http_test_package.testing_TжTB(t));

    var re = regexp.MustCompile(stringsBytesAZaZˢ);
    {
        var reʗ1 = re;
        var err = fs.WalkDir(os.DirFS("."), ".", error (@string path, fs.DirEntry d, error errΔ1) => {
            if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }

            if (path == "internal/ascii"u8) {
                return fs.SkipDir;
            }
            if (!strings.HasSuffix(path, ".go") || strings.HasSuffix(path, testGoˢ) || path == "h2_bundle.go"u8 || path == "internal/httpcommon/httpcommon.go"u8 || d.IsDir()) {
                return default;
            }

            (var contents, errΔ1) = os.ReadFile(path);
            if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
            foreach (var (lineNum, line) in range(strings.Split((@string)contents, "\n"))) {
                foreach (var (_, match) in range(reʗ1.FindAllStringSubmatch(line, -1))) {
                    if (!forbiddenStringsFunctions[match[2]]) {
                        continue;
                    }
                    t.Errorf("disallowed call to %s at %s:%d", match[0], path, lineNum + 1);
                }
            }

            return default;
        }); if (err != nil) {
            t.Fatal(err);
        }
    }
}

public static void TestProtocols(ж<testing.T> t) {
    global::go.net.http_package.Protocols p = default;
    if (p.HTTP1()) {
        t.Errorf("zero-value protocols: p.HTTP1() = true, want false");
    }
    p.SetHTTP1(true);
    p.SetHTTP2(true);
    if (!p.HTTP1()) {
        t.Errorf("initialized protocols: p.HTTP1() = false, want true");
    }
    if (!p.HTTP2()) {
        t.Errorf("initialized protocols: p.HTTP2() = false, want true");
    }
    p.SetHTTP1(false);
    if (p.HTTP1()) {
        t.Errorf("after unsetting HTTP1: p.HTTP1() = true, want false");
    }
    if (!p.HTTP2()) {
        t.Errorf("after unsetting HTTP1: p.HTTP2() = false, want true");
    }
}

internal static ref readonly @string redirectURL => ref redirectURLᵇ;
private static readonly @string redirectURLᵇ = "/thisaredirect细雪withasciilettersのけぶabcdefghijk.html"u8;

public static void BenchmarkHexEscapeNonASCII(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();

    for (nint i = 0; i < b.N; i++) {
        hexEscapeNonASCII(redirectURL);
    }
}

} // end http_internal_test_package
