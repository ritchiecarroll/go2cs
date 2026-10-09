// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.index;

using bytes = bytes_package;
using fmt = fmt_package;
using fs = go.io.fs_package;
using rand = go.math.rand_package;
using os = os_package;
using filepath = path.filepath_package;
using regexp = regexp_package;
using slices = slices_package;
using sort = sort_package;
using strings = strings_package;
using testing = testing_package;
using go.io;
using go.math;
using io = io_package;
using path;
using static go.index.suffixarray_package;

partial class suffixarray_internal_test_package {

internal partial struct testCase {
    internal @string name;  // name of test case
    internal @string source;  // source to index
    internal slice<@string> patterns; // patterns to lookup
}

// 10 a's
// 11 a's
internal static slice<testCase> testCases = new testCase[]{
    new(
        "empty string"u8,
        ""u8,
        new @string[]{
            ""u8,
            "foo"u8,
            "(foo)"u8,
            ".*"u8,
            "a*"u8
        }.slice()
    ),
    new(
        "all a's"u8,
        "aaaaaaaaaa"u8,
        new @string[]{
            ""u8,
            "a"u8,
            "aa"u8,
            "aaa"u8,
            "aaaa"u8,
            "aaaaa"u8,
            "aaaaaa"u8,
            "aaaaaaa"u8,
            "aaaaaaaa"u8,
            "aaaaaaaaa"u8,
            "aaaaaaaaaa"u8,
            "aaaaaaaaaaa"u8,
            "."u8,
            ".*"u8,
            "a+"u8,
            "aa+"u8,
            "aaaa[b]?"u8,
            "aaa*"u8
        }.slice()
    ),
    new(
        "abc"u8,
        "abc"u8,
        new @string[]{
            "a"u8,
            "b"u8,
            "c"u8,
            "ab"u8,
            "bc"u8,
            "abc"u8,
            "a.c"u8,
            "a(b|c)"u8,
            "abc?"u8
        }.slice()
    ),
    new(
        "barbara*3"u8,
        "barbarabarbarabarbara"u8,
        new @string[]{
            "a"u8,
            "bar"u8,
            "rab"u8,
            "arab"u8,
            "barbar"u8,
            "bara?bar"u8
        }.slice()
    ),
    new(
        "typing drill"u8,
        "Now is the time for all good men to come to the aid of their country."u8,
        new @string[]{
            "Now"u8,
            "the time"u8,
            "to come the aid"u8,
            "is the time for all good men to come to the aid of their"u8,
            "to (come|the)?"u8
        }.slice()
    ),
    new(
        "godoc simulation"u8,
        "package main\n\nimport(\n    \"rand\"\n    "u8,
        new @string[]{}.slice()
    )
}.slice();

// find all occurrences of s in source; report at most n occurrences
internal static slice<nint> find(@string src, @string s, nint n) {
    slice<nint> res = default!;
    if (s != ""u8 && n != 0) {
        // find at most n occurrences of s in src
        for (nint i = -1; n < 0 || builtin.len(res) < n; ) {
            nint j = strings.Index(src.slice(i + 1), s);
            if (j < 0) {
                break;
            }
            i += j + 1;
            res = append(res, i);
        }
    }
    return res;
}

internal static void testLookup(ж<testing.T> Ꮡt, ж<testCase> Ꮡtc, ж<global::go.index.suffixarray_package.Index> Ꮡx, @string s, nint n) {
    ref var tc = ref Ꮡtc.DerefOrNull();

    var res = Ꮡx.Lookup(slice<byte>(s), n);
    var exp = find(tc.source, s, n);
    // check that the lengths match
    if (builtin.len(res) != builtin.len(exp)) {
        Ꮡt.Errorf("test %q, lookup %q (n = %d): expected %d results; got %d"u8, tc.name, s, n, builtin.len(exp), builtin.len(res));
    }
    // if n >= 0 the number of results is limited --- unless n >= all results,
    // we may obtain different positions from the Index and from find (because
    // Index may not find the results in the same order as find) => in general
    // we cannot simply check that the res and exp lists are equal
    // check that each result is in fact a correct match and there are no duplicates
    slices.Sort<slice<nint>, nint>(res);
    foreach (var (i, r) in res) {
        if (r < 0 || builtin.len(tc.source) <= r){
            Ꮡt.Errorf("test %q, lookup %q, result %d (n = %d): index %d out of range [0, %d["u8, tc.name, s, i, n, r, builtin.len(tc.source));
        } else 
        if (!strings.HasPrefix(tc.source.slice(r), s)) {
            Ꮡt.Errorf("test %q, lookup %q, result %d (n = %d): index %d not a match"u8, tc.name, s, i, n, r);
        }
        if (i > 0 && res[i - 1] == r) {
            Ꮡt.Errorf("test %q, lookup %q, result %d (n = %d): found duplicate index %d"u8, tc.name, s, i, n, r);
        }
    }
    if (n < 0) {
        // all results computed - sorted res and exp must be equal
        foreach (var (i, r) in res) {
            nint e = exp[i];
            if (r != e) {
                Ꮡt.Errorf("test %q, lookup %q, result %d: expected index %d; got %d"u8, tc.name, s, i, e, r);
            }
        }
    }
}

internal static void testFindAllIndex(ж<testing.T> Ꮡt, ж<testCase> Ꮡtc, ж<global::go.index.suffixarray_package.Index> Ꮡx, ж<regexp.Regexp> Ꮡrx, nint n) {
    ref var tc = ref Ꮡtc.DerefOrNull();

    var res = Ꮡx.FindAllIndex(Ꮡrx, n);
    var exp = Ꮡrx.FindAllStringIndex(tc.source, n);
    // check that the lengths match
    if (builtin.len(res) != builtin.len(exp)) {
        Ꮡt.Errorf("test %q, FindAllIndex %q (n = %d): expected %d results; got %d"u8, tc.name, Ꮡrx.OrTypedNil(), n, builtin.len(exp), builtin.len(res));
    }
    // if n >= 0 the number of results is limited --- unless n >= all results,
    // we may obtain different positions from the Index and from regexp (because
    // Index may not find the results in the same order as regexp) => in general
    // we cannot simply check that the res and exp lists are equal
    // check that each result is in fact a correct match and the result is sorted
    foreach (var (i, r) in res) {
        if (r[0] < 0 || r[0] > r[1] || builtin.len(tc.source) < r[1]){
            Ꮡt.Errorf("test %q, FindAllIndex %q, result %d (n == %d): illegal match [%d, %d]"u8, tc.name, Ꮡrx.OrTypedNil(), i, n, r[0], r[1]);
        } else 
        if (!Ꮡrx.MatchString(tc.source.slice(r[0], r[1]))) {
            Ꮡt.Errorf("test %q, FindAllIndex %q, result %d (n = %d): [%d, %d] not a match"u8, tc.name, Ꮡrx.OrTypedNil(), i, n, r[0], r[1]);
        }
    }
    if (n < 0) {
        // all results computed - sorted res and exp must be equal
        foreach (var (i, r) in res) {
            var e = exp[i];
            if (r[0] != e[0] || r[1] != e[1]) {
                Ꮡt.Errorf("test %q, FindAllIndex %q, result %d: expected match [%d, %d]; got [%d, %d]"u8,
                    tc.name, Ꮡrx.OrTypedNil(), i, e[0], e[1], r[0], r[1]);
            }
        }
    }
}

internal static void testLookups(ж<testing.T> Ꮡt, ж<testCase> Ꮡtc, ж<global::go.index.suffixarray_package.Index> Ꮡx, nint n) {
    ref var tc = ref Ꮡtc.DerefOrNull();

    foreach (var (_, pat) in tc.patterns) {
        testLookup(Ꮡt, Ꮡtc, Ꮡx, pat, n);
        {
            var (rx, err) = regexp.Compile(pat); if (err == default!) {
                testFindAllIndex(Ꮡt, Ꮡtc, Ꮡx, rx, n);
            }
        }
    }
}

internal partial struct index /*global::go.index.suffixarray_package.Index*/;

internal static nint Len(this ref index x) {
    return x.sa.len();
}

internal static bool Less(this ref index x, nint i, nint j) {
    return bytes.Compare(x.at(i), x.at(j)) < 0;
}

internal static void Swap(this ref index x, nint i, nint j) {
    if (x.sa.int32 != default!){
        (x.sa.int32[i], x.sa.int32[j]) = (x.sa.int32[j], x.sa.int32[i]);
    } else {
        (x.sa.int64[i], x.sa.int64[j]) = (x.sa.int64[j], x.sa.int64[i]);
    }
}

internal static slice<byte> at(this ref index x, nint i) {
    return x.data.slice((nint)(x.sa.get(i)));
}

internal static void testConstruction(ж<testing.T> Ꮡt, ж<testCase> Ꮡtc, ж<global::go.index.suffixarray_package.Index> Ꮡx) {
    ref var tc = ref Ꮡtc.DerefOrNull();

    if (!sort.IsSorted(new suffixarray_internal_test_package.indexжInterface(Ꮡx.Reinterpret<global::go.index.suffixarray_package.Index, index>()))) {
        Ꮡt.Errorf("failed testConstruction %s"u8, tc.name);
    }
}

internal static bool equal(ж<global::go.index.suffixarray_package.Index> Ꮡx, ж<global::go.index.suffixarray_package.Index> Ꮡy) {
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    if (!bytes.Equal(x.data, y.data)) {
        return false;
    }
    if (x.sa.len() != y.sa.len()) {
        return false;
    }
    nint n = x.sa.len();
    for (nint i = 0; i < n; i++) {
        if (x.sa.get(i) != y.sa.get(i)) {
            return false;
        }
    }
    return true;
}

// returns the serialized index size
internal static nint testSaveRestore(ж<testing.T> Ꮡt, ж<testCase> Ꮡtc, ж<global::go.index.suffixarray_package.Index> Ꮡx) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();
        ref var tc = ref Ꮡtc.DerefOrNull();
        ref var x = ref Ꮡx.DerefOrNull();

        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        {
            var err = x.Write(new suffixarray_test_package.bytes_BufferжWriter(Ꮡbuf)); if (err != default!) {
                Ꮡt.Errorf("failed writing index %s (%s)"u8, tc.name, err);
            }
        }
        nint size = buf.Len();
        ref var y = ref heap(new global::go.index.suffixarray_package.Index(), out var Ꮡy);
        {
            var err = y.Read(new suffixarray_test_package.bytes_ReaderжReader(bytes.NewReader(buf.Bytes()))); if (err != default!) {
                Ꮡt.Errorf("failed reading index %s (%s)"u8, tc.name, err);
            }
        }
        if (!equal(Ꮡx, Ꮡy)) {
            Ꮡt.Errorf("restored index doesn't match saved index %s"u8, tc.name);
        }
        nint old = maxData32;
        defer(() => {
            maxData32 = old;
        }, ref ᒐ);
        // Reread as forced 32.
        y = new Index(nil);
        maxData32 = realMaxData32;
        {
            var err = y.Read(new suffixarray_test_package.bytes_ReaderжReader(bytes.NewReader(buf.Bytes()))); if (err != default!) {
                Ꮡt.Errorf("failed reading index %s (%s)"u8, tc.name, err);
            }
        }
        if (!equal(Ꮡx, Ꮡy)) {
            Ꮡt.Errorf("restored index doesn't match saved index %s"u8, tc.name);
        }
        // Reread as forced 64.
        y = new Index(nil);
        maxData32 = -1;
        {
            var err = y.Read(new suffixarray_test_package.bytes_ReaderжReader(bytes.NewReader(buf.Bytes()))); if (err != default!) {
                Ꮡt.Errorf("failed reading index %s (%s)"u8, tc.name, err);
            }
        }
        if (!equal(Ꮡx, Ꮡy)) {
            Ꮡt.Errorf("restored index doesn't match saved index %s"u8, tc.name);
        }
        return size;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static void testIndex(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in testCases) {
        ref var tc = ref heap(new testCase(), out var Ꮡtc);
        tc = vᴛ1;

        var x = New(slice<byte>(tc.source));
        testConstruction(Ꮡt, Ꮡtc, x);
        testSaveRestore(Ꮡt, Ꮡtc, x);
        testLookups(Ꮡt, Ꮡtc, x, 0);
        testLookups(Ꮡt, Ꮡtc, x, 1);
        testLookups(Ꮡt, Ꮡtc, x, 10);
        testLookups(Ꮡt, Ꮡtc, x, 2000000000);
        testLookups(Ꮡt, Ꮡtc, x, -1);
    }
}

public static void TestIndex32(ж<testing.T> Ꮡt) {
    testIndex(Ꮡt);
}

public static void TestIndex64(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        maxData32 = -1;
        defer(() => {
            maxData32 = realMaxData32;
        }, ref ᒐ);
        testIndex(Ꮡt);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestNew32(ж<testing.T> Ꮡt) {
    test(Ꮡt, (slice<byte> x) => {
        var sa = new slice<int32>(builtin.len(x));
        text_32(x, sa);
        var @out = new slice<nint>(builtin.len(sa));
        foreach (var (i, v) in sa) {
            @out[i] = (nint)v;
        }
        return @out;
    });
}

public static void TestNew64(ж<testing.T> Ꮡt) {
    test(Ꮡt, (slice<byte> x) => {
        var sa = new slice<int64>(builtin.len(x));
        text_64(x, sa);
        var @out = new slice<nint>(builtin.len(sa));
        foreach (var (i, v) in sa) {
            @out[i] = (nint)v;
        }
        return @out;
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string abababˢ = "ababab..."u8;
internal static readonly @string forceallocˢ = "forcealloc"u8;
internal static readonly @string exhaustive2ˢ = "exhaustive2"u8;
internal static readonly @string exhaustive3ˢ = "exhaustive3"u8;

// test tests an arbitrary suffix array construction function.
// Generates many inputs, builds and checks suffix arrays.
internal static void test(ж<testing.T> Ꮡt, Func<slice<byte>, slice<nint>> build) {
    Ꮡt.Run(abababˢ, (ж<testing.T> tΔ1) => {
        // Very repetitive input has numLMS = len(x)/2-1
        // at top level, the largest it can be.
        // But maxID is only two (aba and ab$).
        nint size = 100000;
        if (testing.Short()) {
            size = 10000;
        }
        var x = new slice<byte>(size);
        foreach (var (i, _) in x) {
            x[i] = LiteralByteAt("ab"u8, i % 2);
        }
        testSA(tΔ1, x, build);
    });
    Ꮡt.Run(forceallocˢ, (ж<testing.T> tΔ2) => {
        // Construct a pathological input that forces
        // recurse_32 to allocate a new temporary buffer.
        // The input must have more than N/3 LMS-substrings,
        // which we arrange by repeating an SLSLSLSLSLSL pattern
        // like ababab... above, but then we must also arrange
        // for a large number of distinct LMS-substrings.
        // We use this pattern:
        // 1 255 1 254 1 253 1 ... 1 2 1 255 2 254 2 253 2 252 2 ...
        // This gives approximately 2¹⁵ distinct LMS-substrings.
        // We need to repeat at least one substring, though,
        // or else the recursion can be bypassed entirely.
        var x = new slice<byte>(100000, 100001);
        var lo = (byte)1;
        var hi = (byte)255;
        foreach (var (i, _) in x) {
            if (i % 2 == 0){
                x[i] = lo;
            } else {
                x[i] = hi;
                hi--;
                if (hi <= lo) {
                    lo++;
                    if (lo == 0) {
                        lo = 1;
                    }
                    hi = 255;
                }
            }
        }
        x.slice(0, cap(x))[builtin.len(x)] = 0; // for sais.New
        testSA(tΔ2, x, build);
    });
    Ꮡt.Run(exhaustive2ˢ, (ж<testing.T> tΔ3) => {
        // All inputs over {0,1} up to length 21.
        // Runs in about 10 seconds on my laptop.
        var x = new slice<byte>(30);
        ref var numFail = ref heap<nint>(out var ᏑnumFail);
        numFail = 0;
        for (nint n = 0; n <= 21; n++) {
            if (n > 12 && testing.Short()) {
                break;
            }
            x[n] = 0; // for sais.New
            testRec(tΔ3, x.slice(0, n), 0, 2, ᏑnumFail, build);
        }
    });
    Ꮡt.Run(exhaustive3ˢ, (ж<testing.T> tΔ4) => {
        // All inputs over {0,1,2} up to length 14.
        // Runs in about 10 seconds on my laptop.
        var x = new slice<byte>(30);
        ref var numFail = ref heap<nint>(out var ᏑnumFail);
        numFail = 0;
        for (nint n = 0; n <= 14; n++) {
            if (n > 8 && testing.Short()) {
                break;
            }
            x[n] = 0; // for sais.New
            testRec(tΔ4, x.slice(0, n), 0, 3, ᏑnumFail, build);
        }
    });
}

// testRec fills x[i:] with all possible combinations of values in [1,max]
// and then calls testSA(t, x, build) for each one.
internal static void testRec(ж<testing.T> Ꮡt, slice<byte> x, nint i, nint max, ж<nint> ᏑnumFail, Func<slice<byte>, slice<nint>> build) {
    ref var numFail = ref ᏑnumFail.DerefOrNull();

    if (i < builtin.len(x)) {
        for (x[i] = 1; x[i] <= (byte)max; x[i]++) {
            testRec(Ꮡt, x, i + 1, max, ᏑnumFail, build);
        }
        return;
    }
    if (!testSA(Ꮡt, x, build)) {
        numFail++;
        if (numFail >= 10) {
            Ꮡt.Errorf("stopping after %d failures"u8, numFail);
            Ꮡt.FailNow();
        }
    }
}

// testSA tests the suffix array build function on the input x.
// It constructs the suffix array and then checks that it is correct.
internal static bool testSA(ж<testing.T> Ꮡt, slice<byte> x, Func<slice<byte>, slice<nint>> build) {
    GoFrame ᒐ = default;
    try {
        var xʗ1 = x;
        defer(() => {
            {
                var e = recover(); if (e != default!) {
                    Ꮡt.Logf("build %v"u8, xʗ1);
                    throw panic(e);
                }
            }
        }, ref ᒐ);
        var sa = build(x);
        if (builtin.len(sa) != builtin.len(x)) {
            Ꮡt.Errorf("build %v: len(sa) = %d, want %d"u8, x, builtin.len(sa), builtin.len(x));
            return false;
        }
        for (nint i = 0; i + 1 < builtin.len(sa); i++) {
            if (sa[i] < 0 || sa[i] >= builtin.len(x) || sa[i + 1] < 0 || sa[i + 1] >= builtin.len(x)) {
                Ꮡt.Errorf("build %s: sa out of range: %v\n"u8, x, sa);
                return false;
            }
            if (bytes.Compare(x.slice(sa[i]), x.slice(sa[i + 1])) >= 0) {
                Ꮡt.Errorf("build %v -> %v\nsa[%d:] = %d,%d out of order"u8, x, sa, i, sa[i], sa[i + 1]);
                return false;
            }
        }
        return true;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static slice<byte> benchdata = new slice<byte>(1000000);
internal static slice<byte> benchrand = new slice<byte>(1000000);

// Of all possible inputs, the random bytes have the least amount of substring
// repetition, and the repeated bytes have the most. For most algorithms,
// the running time of every input will be between these two.
internal static void benchmarkNew(ж<testing.B> Ꮡb, bool random) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    b.StopTimer();
    var data = benchdata;
    if (random) {
        data = benchrand;
        if (data[0] == 0) {
            foreach (var (i, _) in data) {
                data[i] = (byte)rand.Intn(256);
            }
        }
    }
    b.StartTimer();
    b.SetBytes((int64)builtin.len(data));
    for (nint i = 0; i < b.N; i++) {
        New(data);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataIsaacNewtonˢ = "../../testdata/Isaac.Newton-Opticks.txt"u8;

internal static (slice<byte>, error) makeText(@string name) {
    ref var data = ref heap<slice<byte>>(out var Ꮡdata);
    var exprᴛ1 = name;
    if (exprᴛ1 == "opticks"u8) {
        error err = default!;
        (data, err) = os.ReadFile(testdataIsaacNewtonˢ);
        if (err != default!) {
            return (default!, err);
        }
    }
    else if (exprᴛ1 == "go"u8) {
        var err = filepath.WalkDir("../.."u8, error (@string path, fs.DirEntry info, error errΔ1) => {
            if (errΔ1 == default! && strings.HasSuffix(path, ".go"u8) && !info.IsDir()) {
                var (@file, errΔ2) = os.ReadFile(path);
                if (errΔ2 != default!) {
                    return errΔ2;
                }
                Ꮡdata.ValueSlot = appendꓸꓸꓸ(Ꮡdata.ValueSlot, @file);
            }
            return default!;
        });
        if (err != default!) {
            return (default!, err);
        }
    }
    else if (exprᴛ1 == "zero"u8) {
        data = new slice<byte>(50000000);
    }
    else if (exprᴛ1 == "rand"u8) {
        data = new slice<byte>(50000000);
        foreach (var (i, _) in data) {
            data[i] = (byte)rand.Intn(256);
        }
    }

    return (data, default!);
}

internal static Action /*cleanup*/ setBits(nint bits) {
    if (bits == 32){
        maxData32 = realMaxData32;
    } else {
        maxData32 = -1; // force use of 64-bit code
    }
    return () => {
        maxData32 = realMaxData32;
    };
}

public static void BenchmarkNew(ж<testing.B> Ꮡb) {
    foreach (var (_, text) in new @string[]{"opticks"u8, "go"u8, "zero"u8, "rand"u8}.slice()) {
        Ꮡb.Run("text="u8 + text, (ж<testing.B> bΔ1) => {
            var (data, err) = makeText(text);
            if (err != default!) {
                bΔ1.Fatal(err);
            }
            if (testing.Short() && builtin.len(data) > 5000000) {
                data = data[..(int)(5000000)];
            }
            foreach (var (_, size) in new nint[]{100000, 500000, 1000000, 5000000, 10000000, 50000000}.slice()) {
                if (builtin.len(data) < size) {
                    continue;
                }
                var dataΔ1 = data.slice(0, size);
                @string name = fmt.Sprintf("%dK"u8, size / 1000);
                if (size >= 1000000) {
                    name = fmt.Sprintf("%dM"u8, size / 1000000);
                }
                var dataʗ1 = dataΔ1;
                bΔ1.Run("size="u8 + name, (ж<testing.B> bΔ2) => {
                    foreach (var (_, bits) in new nint[]{32, 64}.slice()) {
                        if (~(nuint)0 == 0xffffffffU && bits == 64) {
                            continue;
                        }
                        var dataʗ2 = dataʗ1;
                        bΔ2.Run(fmt.Sprintf("bits=%d"u8, bits), (ж<testing.B> bΔ3) => {
                            GoFrame ᒐ = default;
                            try {
                                var cleanup = setBits(bits);
                                var cleanupʗ1 = cleanup;
                                defer(cleanupʗ1, ref ᒐ);
                                bΔ3.SetBytes((int64)builtin.len(dataʗ2));
                                bΔ3.ReportAllocs();
                                for (nint i = 0; i < (~bΔ3).N; i++) {
                                    New(dataʗ2);
                                }
                            }
                            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                            finally { ᒐ.Run(); }
                        });
                    }
                });
            }
        });
    }
}

public static void BenchmarkSaveRestore(ж<testing.B> Ꮡb) {
    var r = rand.New(rand.NewSource(0x5a77a1)); // guarantee always same sequence
    var data = new slice<byte>((1 << (int)(20))); // 1MB of data to index
    foreach (var (i, _) in data) {
        data[i] = (byte)r.Intn(256);
    }
    foreach (var (_, bits) in new nint[]{32, 64}.slice()) {
        if (~(nuint)0 == 0xffffffffU && bits == 64) {
            continue;
        }
        var dataʗ1 = data;
        Ꮡb.Run(fmt.Sprintf("bits=%d"u8, bits), (ж<testing.B> bΔ1) => {
            GoFrame ᒐ = default;
            try {
                var cleanup = setBits(bits);
                var cleanupʗ1 = cleanup;
                defer(cleanupʗ1, ref ᒐ);
                bΔ1.StopTimer();
                var x = New(dataʗ1);
                nint size = testSaveRestore(nil, nil, x); // verify correctness
                var buf = bytes.NewBuffer(new slice<byte>(size)); // avoid growing
                bΔ1.SetBytes((int64)size);
                bΔ1.StartTimer();
                bΔ1.ReportAllocs();
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    buf.Reset();
                    {
                        var err = x.Write(new suffixarray_test_package.bytes_BufferжWriter(buf)); if (err != default!) {
                            bΔ1.Fatal(err);
                        }
                    }
                    global::go.index.suffixarray_package.Index y = default!;
                    {
                        var err = y.Read(new suffixarray_test_package.bytes_BufferжReader(buf)); if (err != default!) {
                            bΔ1.Fatal(err);
                        }
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

} // end suffixarray_internal_test_package
