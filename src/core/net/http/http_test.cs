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

public static void TestForeachHeaderElement(ж<testing.T> Ꮡt) {
    var tests = new TestForeachHeaderElement_tests[]{
        new("Foo"u8, new @string[]{"Foo"u8}.slice()),
        new(" Foo"u8, new @string[]{"Foo"u8}.slice()),
        new("Foo "u8, new @string[]{"Foo"u8}.slice()),
        new(" Foo "u8, new @string[]{"Foo"u8}.slice()),
        new("foo"u8, new @string[]{"foo"u8}.slice()),
        new("anY-cAsE"u8, new @string[]{"anY-cAsE"u8}.slice()),
        new(""u8, default!),
        new(",,,,  ,  ,,   ,,, ,"u8, default!),
        new(" Foo,Bar, Baz,lower,,Quux "u8, new @string[]{"Foo"u8, "Bar"u8, "Baz"u8, "lower"u8, "Quux"u8}.slice())
    }.slice();
    foreach (var (_, tt) in tests) {
        ref var got = ref heap<slice<@string>>(out var Ꮡgot);
        foreachHeaderElement(tt.@in, (@string v) => {
            Ꮡgot.ValueSlot = append(Ꮡgot.ValueSlot, v);
        });
        if (!slices.Equal<slice<@string>, @string>(got, tt.want)) {
            Ꮡt.Errorf("foreachHeaderElement(%q) = %q; want %q"u8, tt.@in, got, tt.want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string toolˢ = "tool"u8;

// Test that cmd/go doesn't link in the HTTP server.
//
// This catches accidental dependencies between the HTTP transport and
// server code.
public static void TestCmdGoNoHTTPServer(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Parallel();
    @string goBin = testenv.GoToolPath(new http_test_package.testing_TжTB(Ꮡt));
    var (@out, err) = testenv.Command(new http_test_package.testing_TжTB(Ꮡt), goBin, toolˢ, "nm", goBin).CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("go tool nm: %v: %s"u8, err, @out);
    }
    var wantSym = new map<@string, bool>{ // Verify these exist: (sanity checking this test)

        ["net/http.(*Client).do"u8] = true,
        ["net/http.(*Transport).RoundTrip"u8] = true, // Verify these don't exist:

        ["net/http.http2Server"u8] = false,
        ["net/http.(*Server).Serve"u8] = false,
        ["net/http.(*ServeMux).ServeHTTP"u8] = false,
        ["net/http.DefaultServeMux"u8] = false
    };
    foreach (var (sym, want) in wantSym) {
        var got = bytes.Contains(@out, slice<byte>(sym));
        if (!want && got) {
            Ꮡt.Errorf("cmd/go unexpectedly links in HTTP server code; found symbol %q in cmd/go"u8, sym);
        }
        if (want && !got) {
            Ꮡt.Errorf("expected to find symbol %q in cmd/go; not found"u8, sym);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"skipping in short mode"u8;
internal static readonly @string testˢ = "test"u8;
internal static readonly @string shortˢ = "-short"u8;
internal static readonly @string tagsNethttpomithttp2ˢ = "-tags=nethttpomithttp2"u8;
internal static readonly @string netHttpˢ2 = "net/http"u8;

// Tests that the nethttpomithttp2 build tag doesn't rot too much,
// even if there's not a regular builder on it.
public static void TestOmitHTTP2(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    Ꮡt.Parallel();
    @string goTool = testenv.GoToolPath(new http_test_package.testing_TжTB(Ꮡt));
    var (@out, err) = testenv.Command(new http_test_package.testing_TжTB(Ꮡt), goTool, testˢ, shortˢ, tagsNethttpomithttp2ˢ, netHttpˢ2).CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("go test -short failed: %v, %s"u8, err, @out);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string vetˢ = "vet"u8;

// Tests that the nethttpomithttp2 build tag at least type checks
// in short mode.
// The TestOmitHTTP2 test above actually runs tests (in long mode).
public static void TestOmitHTTP2Vet(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    @string goTool = testenv.GoToolPath(new http_test_package.testing_TжTB(Ꮡt));
    var (@out, err) = testenv.Command(new http_test_package.testing_TжTB(Ꮡt), goTool, vetˢ, tagsNethttpomithttp2ˢ, netHttpˢ2).CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("go vet failed: %v, %s"u8, err, @out);
    }
}

internal static nint valuesCount;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object benchmarkWasnTRunˢ = (@string)"Benchmark wasn't run"u8;

public static void BenchmarkCopyValues(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var src = new url.Values(new map<@string, slice<@string>>{
        ["a"u8] = new @string[]{"1"u8, "2"u8, "3"u8, "4"u8, "5"u8}.slice(),
        ["b"u8] = new @string[]{"2"u8, "2"u8, "3"u8, "4"u8, "5"u8}.slice(),
        ["c"u8] = new @string[]{"3"u8, "2"u8, "3"u8, "4"u8, "5"u8}.slice(),
        ["d"u8] = new @string[]{"4"u8, "2"u8, "3"u8, "4"u8, "5"u8}.slice(),
        ["e"u8] = new @string[]{"1"u8, "1"u8, "2"u8, "3"u8, "4"u8, "5"u8, "6"u8, "7"u8, "abcdef"u8, "l"u8, "a"u8, "b"u8, "c"u8, "d"u8, "z"u8}.slice(),
        ["j"u8] = new @string[]{"1"u8, "2"u8}.slice(),
        ["m"u8] = default!
    });
    for (nint i = 0; i < b.N; i++) {
        var dst = new url.Values(new map<@string, slice<@string>>{["a"u8] = new @string[]{"b"u8}.slice(), ["b"u8] = new @string[]{"2"u8}.slice(), ["c"u8] = new @string[]{"3"u8}.slice(), ["d"u8] = new @string[]{"4"u8}.slice(), ["j"u8] = default!, ["m"u8] = new @string[]{"x"u8}.slice()});
        copyValues(dst, src);
        {
            http_internal_test_package.valuesCount = builtin.len(dst["a"u8]); if (http_internal_test_package.valuesCount != 6) {
                Ꮡb.Fatalf(@"%d items in dst[""a""] but expected 6"u8, http_internal_test_package.valuesCount);
            }
        }
    }
    if (http_internal_test_package.valuesCount == 0) {
        Ꮡb.Fatal(benchmarkWasnTRunˢ);
    }
}

// Functions that use Unicode-aware case folding.
// Functions that use Unicode-aware spaces.
internal static map<@string, bool> forbiddenStringsFunctions = new map<@string, bool>{
    ["EqualFold"u8] = true,
    ["Title"u8] = true,
    ["ToLower"u8] = true,
    ["ToLowerSpecial"u8] = true,
    ["ToTitle"u8] = true,
    ["ToTitleSpecial"u8] = true,
    ["ToUpper"u8] = true,
    ["ToUpperSpecial"u8] = true,
    ["Fields"u8] = true,
    ["TrimSpace"u8] = true
};

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stringsBytesAZaZˢ = @"(strings|bytes).([A-Za-z]+)"u8;
internal static readonly @string testGoˢ = "_test.go"u8;

// TestNoUnicodeStrings checks that nothing in net/http uses the Unicode-aware
// strings and bytes package functions. HTTP is mostly ASCII based, and doing
// Unicode-aware case folding or space stripping can introduce vulnerabilities.
public static void TestNoUnicodeStrings(ж<testing.T> Ꮡt) {
    testenv.MustHaveSource(new http_test_package.testing_TжTB(Ꮡt));
    var re = regexp.MustCompile(stringsBytesAZaZˢ);
    {
        var reʗ1 = re;
        var err = fs.WalkDir(os.DirFS("."u8), "."u8, error (@string path, fs.DirEntry d, error errΔ1) => {
            if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
            if (path == "internal/ascii"u8) {
                return fs.SkipDir;
            }
            if (!strings.HasSuffix(path, ".go"u8) || strings.HasSuffix(path, testGoˢ) || path == "h2_bundle.go"u8 || d.IsDir()) {
                return default!;
            }
            (var contents, errΔ1) = os.ReadFile(path);
            if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
            foreach (var (lineNum, line) in strings.Split(((@string)contents), "\n"u8)) {
                foreach (var (_, match) in reʗ1.FindAllStringSubmatch(line, -1)) {
                    if (!forbiddenStringsFunctions[match[2]]) {
                        continue;
                    }
                    Ꮡt.Errorf("disallowed call to %s at %s:%d"u8, match[0], path, lineNum + 1);
                }
            }
            return default!;
        }); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
}

public static void TestProtocols(ж<testing.T> Ꮡt) {
    global::go.net.http_package.Protocols p = default!;
    if (p.HTTP1()) {
        Ꮡt.Errorf("zero-value protocols: p.HTTP1() = true, want false"u8);
    }
    p.SetHTTP1(true);
    p.SetHTTP2(true);
    if (!p.HTTP1()) {
        Ꮡt.Errorf("initialized protocols: p.HTTP1() = false, want true"u8);
    }
    if (!p.HTTP2()) {
        Ꮡt.Errorf("initialized protocols: p.HTTP2() = false, want true"u8);
    }
    p.SetHTTP1(false);
    if (p.HTTP1()) {
        Ꮡt.Errorf("after unsetting HTTP1: p.HTTP1() = true, want false"u8);
    }
    if (!p.HTTP2()) {
        Ꮡt.Errorf("after unsetting HTTP1: p.HTTP2() = false, want true"u8);
    }
}

internal static readonly @string redirectURL = "/thisaredirect细雪withasciilettersのけぶabcdefghijk.html"u8;

public static void BenchmarkHexEscapeNonASCII(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        hexEscapeNonASCII(redirectURL);
    }
}

} // end http_internal_test_package
