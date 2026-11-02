// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using bytes = bytes_package;
using race = global::go.@internal.race_package;
using reflect = reflect_package;
using runtime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using global::go.@internal;
using io = io_package;
using static global::go.net.http_package;

partial class http_internal_test_package {

partial struct headerWriteTestsᴛ1 /*dyn*/ {
    internal global::go.net.http_package.ΔHeader h;
    internal map<@string, bool> exclude;
    internal @string expected;
}
internal static slice<headerWriteTestsᴛ1> headerWriteTests = slice<headerWriteTestsᴛ1>([
    new(new ΔHeader(new map<@string, slice<@string>>{}), default, ""),
    new(
        new ΔHeader(new map<@string, slice<@string>>{
            ["Content-Type"] = slice<@string>(["text/html; charset=UTF-8"]),
            ["Content-Length"] = slice<@string>(["0"])
        }),
        default,
        "Content-Length: 0\r\nContent-Type: text/html; charset=UTF-8\r\n"
    ),
    new(
        new ΔHeader(new map<@string, slice<@string>>{
            ["Content-Length"] = slice<@string>(["0", "1", "2"])
        }),
        default,
        "Content-Length: 0\r\nContent-Length: 1\r\nContent-Length: 2\r\n"
    ),
    new(
        new ΔHeader(new map<@string, slice<@string>>{
            ["Expires"] = slice<@string>(["-1"]),
            ["Content-Length"] = slice<@string>(["0"]),
            ["Content-Encoding"] = slice<@string>(["gzip"])
        }),
        new map<@string, bool>{["Content-Length"] = true},
        "Content-Encoding: gzip\r\nExpires: -1\r\n"
    ),
    new(
        new ΔHeader(new map<@string, slice<@string>>{
            ["Expires"] = slice<@string>(["-1"]),
            ["Content-Length"] = slice<@string>(["0", "1", "2"]),
            ["Content-Encoding"] = slice<@string>(["gzip"])
        }),
        new map<@string, bool>{["Content-Length"] = true},
        "Content-Encoding: gzip\r\nExpires: -1\r\n"
    ),
    new(
        new ΔHeader(new map<@string, slice<@string>>{
            ["Expires"] = slice<@string>(["-1"]),
            ["Content-Length"] = slice<@string>(["0"]),
            ["Content-Encoding"] = slice<@string>(["gzip"])
        }),
        new map<@string, bool>{["Content-Length"] = true, ["Expires"] = true, ["Content-Encoding"] = true},
        ""
    ),
    new(
        new ΔHeader(new map<@string, slice<@string>>{
            ["Nil"] = default,
            ["Empty"] = slice<@string>([]),
            ["Blank"] = slice<@string>([""]),
            ["Double-Blank"] = slice<@string>(["", ""])
        }),
        default,
        "Blank: \r\nDouble-Blank: \r\nDouble-Blank: \r\n"
    ),
    // Tests header sorting when over the insertion sort threshold side:
    new(
        new ΔHeader(new map<@string, slice<@string>>{
            ["k1"] = slice<@string>(["1a", "1b"]),
            ["k2"] = slice<@string>(["2a", "2b"]),
            ["k3"] = slice<@string>(["3a", "3b"]),
            ["k4"] = slice<@string>(["4a", "4b"]),
            ["k5"] = slice<@string>(["5a", "5b"]),
            ["k6"] = slice<@string>(["6a", "6b"]),
            ["k7"] = slice<@string>(["7a", "7b"]),
            ["k8"] = slice<@string>(["8a", "8b"]),
            ["k9"] = slice<@string>(["9a", "9b"])
        }),
        new map<@string, bool>{["k5"] = true},
        "k1: 1a\r\nk1: 1b\r\nk2: 2a\r\nk2: 2b\r\nk3: 3a\r\nk3: 3b\r\n"u8 + "k4: 4a\r\nk4: 4b\r\nk6: 6a\r\nk6: 6b\r\n"u8 + "k7: 7a\r\nk7: 7b\r\nk8: 8a\r\nk8: 8b\r\nk9: 9a\r\nk9: 9b\r\n"u8
    ),
    // Tests invalid characters in headers.
    new(
        new ΔHeader(new map<@string, slice<@string>>{
            ["Content-Type"] = slice<@string>(["text/html; charset=UTF-8"]),
            ["NewlineInValue"] = slice<@string>(["1\r\nBar: 2"]),
            ["NewlineInKey\r\n"] = slice<@string>(["1"]),
            ["Colon:InKey"] = slice<@string>(["1"]),
            ["Evil: 1\r\nSmuggledValue"] = slice<@string>(["1"])
        }),
        default,
        "Content-Type: text/html; charset=UTF-8\r\n"u8 + "NewlineInValue: 1  Bar: 2\r\n"u8
    )
]);

public static void TestHeaderWrite(ж<testing.T> t) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    foreach (var (i, test) in headerWriteTests) {
        test.h.WriteSubset(new http_test_package.strings_BuilderжWriter(Ꮡbuf), test.exclude);
        if (buf.String() != test.expected) {
            t.Errorf("#%d:\n got: %q\nwant: %q", i, buf.String(), test.expected);
        }
        buf.Reset();
    }
}

partial struct parseTimeTestsᴛ1 /*dyn*/ {
    internal global::go.net.http_package.ΔHeader h;
    internal bool err;
}
internal static slice<parseTimeTestsᴛ1> parseTimeTests = slice<parseTimeTestsᴛ1>([
    new(new ΔHeader(new map<@string, slice<@string>>{["Date"] = slice<@string>([""])}), true),
    new(new ΔHeader(new map<@string, slice<@string>>{["Date"] = slice<@string>(["invalid"])}), true),
    new(new ΔHeader(new map<@string, slice<@string>>{["Date"] = slice<@string>(["1994-11-06T08:49:37Z00:00"])}), true),
    new(new ΔHeader(new map<@string, slice<@string>>{["Date"] = slice<@string>(["Sun, 06 Nov 1994 08:49:37 GMT"])}), false),
    new(new ΔHeader(new map<@string, slice<@string>>{["Date"] = slice<@string>(["Sunday, 06-Nov-94 08:49:37 GMT"])}), false),
    new(new ΔHeader(new map<@string, slice<@string>>{["Date"] = slice<@string>(["Sun Nov  6 08:49:37 1994"])}), false)
]);

public static void TestParseTime(ж<testing.T> t) {
    var expect = time.Date(1994, 11, 6, 8, 49, 37, 0, time.ΔUTC);
    foreach (var (i, test) in parseTimeTests) {
        var (d, err) = ParseTime(test.h.Get(dateˢ));
        if (err != nil) {
            if (!test.err) {
                t.Errorf("#%d:\n got err: %v", i, err);
            }
            continue;
        }
        if (test.err) {
            t.Errorf("#%d:\n  should err", i);
            continue;
        }
        if (!expect.Equal(d)) {
            t.Errorf("#%d:\n got: %v\nwant: %v", i, d, expect);
        }
    }
}

internal partial struct hasTokenTest {
    internal @string header;
    internal @string token;
    internal bool want;
}

internal static slice<hasTokenTest> hasTokenTests = slice<hasTokenTest>([
    new("", "", false),
    new("", "foo", false),
    new("foo", "foo", true),
    new("foo ", "foo", true),
    new(" foo", "foo", true),
    new(" foo ", "foo", true),
    new("foo,bar", "foo", true),
    new("bar,foo", "foo", true),
    new("bar, foo", "foo", true),
    new("bar,foo, baz", "foo", true),
    new("bar, foo,baz", "foo", true),
    new("bar,foo, baz", "foo", true),
    new("bar, foo, baz", "foo", true),
    new("FOO", "foo", true),
    new("FOO ", "foo", true),
    new(" FOO", "foo", true),
    new(" FOO ", "foo", true),
    new("FOO,BAR", "foo", true),
    new("BAR,FOO", "foo", true),
    new("BAR, FOO", "foo", true),
    new("BAR,FOO, baz", "foo", true),
    new("BAR, FOO,BAZ", "foo", true),
    new("BAR,FOO, BAZ", "foo", true),
    new("BAR, FOO, BAZ", "foo", true),
    new("foobar", "foo", false),
    new("barfoo ", "foo", false)
]);

public static void TestHasToken(ж<testing.T> t) {
    foreach (var (_, tt) in hasTokenTests) {
        if (hasToken(tt.header, tt.token) != tt.want) {
            t.Errorf("hasToken(%q, %q) = %v; want %v", tt.header, tt.token, !tt.want, tt.want);
        }
    }
}

public static void TestNilHeaderClone(ж<testing.T> t) {
    var t1 = (global::go.net.http_package.ΔHeader)default;
    var t2 = t1.Clone();
    if (t2 != nil) {
        t.Errorf("cloned header does not match original: got: %+v; want: %+v", t2, (any)(default));
    }
}

internal static global::go.net.http_package.ΔHeader testHeader;
internal static void initᴛtestHeader() { testHeader = new ΔHeader(new map<@string, slice<@string>>{
    ["Content-Length"] = slice<@string>(["123"]),
    ["Content-Type"] = slice<@string>(["text/plain"]),
    ["Date"] = slice<@string>(["some date at some time Z"]),
    ["Server"] = slice<@string>([DefaultUserAgent])
}); }

internal static readonly ж<bytes.Buffer> Ꮡbuf = new StaticBox<bytes.Buffer>(default(bytes.Buffer));
internal static ref bytes.Buffer buf => ref ((StaticBox<bytes.Buffer>)Ꮡbuf).ValueSlot;

public static void BenchmarkHeaderWriteSubset(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        buf.Reset();
        testHeader.WriteSubset(new http_test_package.bytes_BufferжWriter(Ꮡbuf), default);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingAllocTestInShortˢ = (@string)"skipping alloc test in short mode"u8;
internal static readonly object skippingTestUnderRaceˢ = (@string)"skipping test under race detector"u8;
internal static readonly object skippingGomaxprocs1ˢ = (@string)"skipping; GOMAXPROCS>1"u8;

public static void TestHeaderWriteSubsetAllocs(ж<testing.T> t) {
    if (testing.Short()) {
        t.Skip(skippingAllocTestInShortˢ);
    }
    if (race.Enabled) {
        t.Skip(skippingTestUnderRaceˢ);
    }
    if (runtime.GOMAXPROCS(0) > 1) {
        t.Skip(skippingGomaxprocs1ˢ);
    }
    var n = testing.AllocsPerRun(100, () => {
        buf.Reset();
        testHeader.WriteSubset(new http_test_package.bytes_BufferжWriter(Ꮡbuf), default);
    });
    if (n > 0D) {
        t.Errorf("allocs = %g; want 0", n);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unexpectedNilHeaderˢ = (@string)"unexpected nil Header"u8;

internal partial struct TestCloneOrMakeHeader_tests /*dyn*/ {
    internal @string name;
    internal global::go.net.http_package.ΔHeader @in, want;
}

// Issue 34878: test that every call to
// cloneOrMakeHeader never returns a nil Header.
public static void TestCloneOrMakeHeader(ж<testing.T> t) {
    var tests = slice<TestCloneOrMakeHeader_tests>([
        new("nil", default, new ΔHeader(new map<@string, slice<@string>>{})),
        new("empty", new ΔHeader(new map<@string, slice<@string>>{}), new ΔHeader(new map<@string, slice<@string>>{})),
        new(
            name: "non-empty",
            @in: new ΔHeader(new map<@string, slice<@string>>{["foo"] = slice<@string>(["bar"])}),
            want: new ΔHeader(new map<@string, slice<@string>>{["foo"] = slice<@string>(["bar"])})
        ),
        new(
            name: "nil value",
            @in: new ΔHeader(new map<@string, slice<@string>>{["foo"] = default}),
            want: new ΔHeader(new map<@string, slice<@string>>{["foo"] = default})
        )
    ]);

    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestCloneOrMakeHeader_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.name, (ж<testing.T> tΔ1) => {
            var got = cloneOrMakeHeader(ttʗ1.@in);
            if (got == nil) {
                tΔ1.Fatal(unexpectedNilHeaderˢ);
            }
            if (!reflect.DeepEqual(got, ttʗ1.want)) {
                tΔ1.Fatalf("Got:  %#v\nWant: %#v", got, ttʗ1.want);
            }
            got.Add("A", "B");
            got.Get("A");
        });
    }
}

} // end http_internal_test_package
