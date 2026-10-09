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

// Tests header sorting when over the insertion sort threshold side:
// Tests invalid characters in headers.

partial struct headerWriteTestsᴛ1 /*dyn*/ {
    internal global::go.net.http_package.ΔHeader h;
    internal map<@string, bool> exclude;
    internal @string expected;
}
internal static slice<headerWriteTestsᴛ1> headerWriteTests = new headerWriteTestsᴛ1[]{
    new(new ΔHeader(new map<@string, slice<@string>>{}), default!, ""u8),
    new(
        new ΔHeader(new map<@string, slice<@string>>{
            ["Content-Type"u8] = new @string[]{"text/html; charset=UTF-8"u8}.slice(),
            ["Content-Length"u8] = new @string[]{"0"u8}.slice()
        }),
        default!,
        "Content-Length: 0\r\nContent-Type: text/html; charset=UTF-8\r\n"u8
    ),
    new(
        new ΔHeader(new map<@string, slice<@string>>{
            ["Content-Length"u8] = new @string[]{"0"u8, "1"u8, "2"u8}.slice()
        }),
        default!,
        "Content-Length: 0\r\nContent-Length: 1\r\nContent-Length: 2\r\n"u8
    ),
    new(
        new ΔHeader(new map<@string, slice<@string>>{
            ["Expires"u8] = new @string[]{"-1"u8}.slice(),
            ["Content-Length"u8] = new @string[]{"0"u8}.slice(),
            ["Content-Encoding"u8] = new @string[]{"gzip"u8}.slice()
        }),
        new map<@string, bool>{["Content-Length"u8] = true},
        "Content-Encoding: gzip\r\nExpires: -1\r\n"u8
    ),
    new(
        new ΔHeader(new map<@string, slice<@string>>{
            ["Expires"u8] = new @string[]{"-1"u8}.slice(),
            ["Content-Length"u8] = new @string[]{"0"u8, "1"u8, "2"u8}.slice(),
            ["Content-Encoding"u8] = new @string[]{"gzip"u8}.slice()
        }),
        new map<@string, bool>{["Content-Length"u8] = true},
        "Content-Encoding: gzip\r\nExpires: -1\r\n"u8
    ),
    new(
        new ΔHeader(new map<@string, slice<@string>>{
            ["Expires"u8] = new @string[]{"-1"u8}.slice(),
            ["Content-Length"u8] = new @string[]{"0"u8}.slice(),
            ["Content-Encoding"u8] = new @string[]{"gzip"u8}.slice()
        }),
        new map<@string, bool>{["Content-Length"u8] = true, ["Expires"u8] = true, ["Content-Encoding"u8] = true},
        ""u8
    ),
    new(
        new ΔHeader(new map<@string, slice<@string>>{
            ["Nil"u8] = default!,
            ["Empty"u8] = new @string[]{}.slice(),
            ["Blank"u8] = new @string[]{""u8}.slice(),
            ["Double-Blank"u8] = new @string[]{""u8, ""u8}.slice()
        }),
        default!,
        "Blank: \r\nDouble-Blank: \r\nDouble-Blank: \r\n"u8
    ),
    new(
        new ΔHeader(new map<@string, slice<@string>>{
            ["k1"u8] = new @string[]{"1a"u8, "1b"u8}.slice(),
            ["k2"u8] = new @string[]{"2a"u8, "2b"u8}.slice(),
            ["k3"u8] = new @string[]{"3a"u8, "3b"u8}.slice(),
            ["k4"u8] = new @string[]{"4a"u8, "4b"u8}.slice(),
            ["k5"u8] = new @string[]{"5a"u8, "5b"u8}.slice(),
            ["k6"u8] = new @string[]{"6a"u8, "6b"u8}.slice(),
            ["k7"u8] = new @string[]{"7a"u8, "7b"u8}.slice(),
            ["k8"u8] = new @string[]{"8a"u8, "8b"u8}.slice(),
            ["k9"u8] = new @string[]{"9a"u8, "9b"u8}.slice()
        }),
        new map<@string, bool>{["k5"u8] = true},
        "k1: 1a\r\nk1: 1b\r\nk2: 2a\r\nk2: 2b\r\nk3: 3a\r\nk3: 3b\r\n"u8 + "k4: 4a\r\nk4: 4b\r\nk6: 6a\r\nk6: 6b\r\n"u8 + "k7: 7a\r\nk7: 7b\r\nk8: 8a\r\nk8: 8b\r\nk9: 9a\r\nk9: 9b\r\n"u8
    ),
    new(
        new ΔHeader(new map<@string, slice<@string>>{
            ["Content-Type"u8] = new @string[]{"text/html; charset=UTF-8"u8}.slice(),
            ["NewlineInValue"u8] = new @string[]{"1\r\nBar: 2"u8}.slice(),
            ["NewlineInKey\r\n"u8] = new @string[]{"1"u8}.slice(),
            ["Colon:InKey"u8] = new @string[]{"1"u8}.slice(),
            ["Evil: 1\r\nSmuggledValue"u8] = new @string[]{"1"u8}.slice()
        }),
        default!,
        "Content-Type: text/html; charset=UTF-8\r\n"u8 + "NewlineInValue: 1  Bar: 2\r\n"u8
    )
}.slice();

public static void TestHeaderWrite(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    foreach (var (i, test) in headerWriteTests) {
        test.h.WriteSubset(new http_test_package.strings_BuilderжWriter(Ꮡbuf), test.exclude);
        if (buf.String() != test.expected) {
            Ꮡt.Errorf("#%d:\n got: %q\nwant: %q"u8, i, buf.String(), test.expected);
        }
        buf.Reset();
    }
}


partial struct parseTimeTestsᴛ1 /*dyn*/ {
    internal global::go.net.http_package.ΔHeader h;
    internal bool err;
}
internal static slice<parseTimeTestsᴛ1> parseTimeTests = new parseTimeTestsᴛ1[]{
    new(new ΔHeader(new map<@string, slice<@string>>{["Date"u8] = new @string[]{""u8}.slice()}), true),
    new(new ΔHeader(new map<@string, slice<@string>>{["Date"u8] = new @string[]{"invalid"u8}.slice()}), true),
    new(new ΔHeader(new map<@string, slice<@string>>{["Date"u8] = new @string[]{"1994-11-06T08:49:37Z00:00"u8}.slice()}), true),
    new(new ΔHeader(new map<@string, slice<@string>>{["Date"u8] = new @string[]{"Sun, 06 Nov 1994 08:49:37 GMT"u8}.slice()}), false),
    new(new ΔHeader(new map<@string, slice<@string>>{["Date"u8] = new @string[]{"Sunday, 06-Nov-94 08:49:37 GMT"u8}.slice()}), false),
    new(new ΔHeader(new map<@string, slice<@string>>{["Date"u8] = new @string[]{"Sun Nov  6 08:49:37 1994"u8}.slice()}), false)
}.slice();

public static void TestParseTime(ж<testing.T> Ꮡt) {
    var expect = time.Date(1994, 11, 6, 8, 49, 37, 0, time.ΔUTC);
    foreach (var (i, test) in parseTimeTests) {
        var (d, err) = ParseTime(test.h.Get(dateˢ));
        if (err != default!) {
            if (!test.err) {
                Ꮡt.Errorf("#%d:\n got err: %v"u8, i, err);
            }
            continue;
        }
        if (test.err) {
            Ꮡt.Errorf("#%d:\n  should err"u8, i);
            continue;
        }
        if (!expect.Equal(d)) {
            Ꮡt.Errorf("#%d:\n got: %v\nwant: %v"u8, i, d, expect);
        }
    }
}

internal partial struct hasTokenTest {
    internal @string header;
    internal @string token;
    internal bool want;
}

internal static slice<hasTokenTest> hasTokenTests = new hasTokenTest[]{
    new(""u8, ""u8, false),
    new(""u8, "foo"u8, false),
    new("foo"u8, "foo"u8, true),
    new("foo "u8, "foo"u8, true),
    new(" foo"u8, "foo"u8, true),
    new(" foo "u8, "foo"u8, true),
    new("foo,bar"u8, "foo"u8, true),
    new("bar,foo"u8, "foo"u8, true),
    new("bar, foo"u8, "foo"u8, true),
    new("bar,foo, baz"u8, "foo"u8, true),
    new("bar, foo,baz"u8, "foo"u8, true),
    new("bar,foo, baz"u8, "foo"u8, true),
    new("bar, foo, baz"u8, "foo"u8, true),
    new("FOO"u8, "foo"u8, true),
    new("FOO "u8, "foo"u8, true),
    new(" FOO"u8, "foo"u8, true),
    new(" FOO "u8, "foo"u8, true),
    new("FOO,BAR"u8, "foo"u8, true),
    new("BAR,FOO"u8, "foo"u8, true),
    new("BAR, FOO"u8, "foo"u8, true),
    new("BAR,FOO, baz"u8, "foo"u8, true),
    new("BAR, FOO,BAZ"u8, "foo"u8, true),
    new("BAR,FOO, BAZ"u8, "foo"u8, true),
    new("BAR, FOO, BAZ"u8, "foo"u8, true),
    new("foobar"u8, "foo"u8, false),
    new("barfoo "u8, "foo"u8, false)
}.slice();

public static void TestHasToken(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in hasTokenTests) {
        if (hasToken(tt.header, tt.token) != tt.want) {
            Ꮡt.Errorf("hasToken(%q, %q) = %v; want %v"u8, tt.header, tt.token, !tt.want, tt.want);
        }
    }
}

public static void TestNilHeaderClone(ж<testing.T> Ꮡt) {
    var t1 = ((global::go.net.http_package.ΔHeader)default!);
    var t2 = t1.Clone();
    if (t2 != default!) {
        Ꮡt.Errorf("cloned header does not match original: got: %+v; want: %+v"u8, t2, (any)(default!));
    }
}

internal static global::go.net.http_package.ΔHeader testHeader;
internal static void initᴛtestHeader() { testHeader = new ΔHeader(new map<@string, slice<@string>>{
    ["Content-Length"u8] = new @string[]{"123"u8}.slice(),
    ["Content-Type"u8] = new @string[]{"text/plain"u8}.slice(),
    ["Date"u8] = new @string[]{"some date at some time Z"u8}.slice(),
    ["Server"u8] = new @string[]{DefaultUserAgent}.slice()
}); }

internal static ж<bytes.Buffer> Ꮡbuf = new StandardBox<bytes.Buffer>(default(bytes.Buffer));
internal static ref bytes.Buffer buf => ref Ꮡbuf.Value;

public static void BenchmarkHeaderWriteSubset(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        buf.Reset();
        testHeader.WriteSubset(new http_test_package.bytes_BufferжWriter(Ꮡbuf), default!);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingAllocTestInShortˢ = (@string)"skipping alloc test in short mode"u8;
internal static readonly object skippingTestUnderRaceˢ = (@string)"skipping test under race detector"u8;
internal static readonly object skippingGomaxprocs1ˢ = (@string)"skipping; GOMAXPROCS>1"u8;

public static void TestHeaderWriteSubsetAllocs(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingAllocTestInShortˢ);
    }
    if (race.Enabled) {
        Ꮡt.Skip(skippingTestUnderRaceˢ);
    }
    if (runtime.GOMAXPROCS(0) > 1) {
        Ꮡt.Skip(skippingGomaxprocs1ˢ);
    }
    var n = testing.AllocsPerRun(100, () => {
        buf.Reset();
        testHeader.WriteSubset(new http_test_package.bytes_BufferжWriter(Ꮡbuf), default!);
    });
    if (n > 0D) {
        Ꮡt.Errorf("allocs = %g; want 0"u8, n);
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
public static void TestCloneOrMakeHeader(ж<testing.T> Ꮡt) {
    var tests = new TestCloneOrMakeHeader_tests[]{
        new("nil"u8, default!, new ΔHeader(new map<@string, slice<@string>>{})),
        new("empty"u8, new ΔHeader(new map<@string, slice<@string>>{}), new ΔHeader(new map<@string, slice<@string>>{})),
        new(
            name: "non-empty"u8,
            @in: new ΔHeader(new map<@string, slice<@string>>{["foo"u8] = new @string[]{"bar"u8}.slice()}),
            want: new ΔHeader(new map<@string, slice<@string>>{["foo"u8] = new @string[]{"bar"u8}.slice()})
        ),
        new(
            name: "nil value"u8,
            @in: new ΔHeader(new map<@string, slice<@string>>{["foo"u8] = default!}),
            want: new ΔHeader(new map<@string, slice<@string>>{["foo"u8] = default!})
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestCloneOrMakeHeader_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<testing.T> tΔ1) => {
            var got = cloneOrMakeHeader(ttʗ1.@in);
            if (got == default!) {
                tΔ1.Fatal(unexpectedNilHeaderˢ);
            }
            if (!reflect.DeepEqual(got, ttʗ1.want)) {
                tΔ1.Fatalf("Got:  %#v\nWant: %#v"u8, got, ttʗ1.want);
            }
            got.Add("A"u8, "B"u8);
            got.Get("A"u8);
        });
    }
}

} // end http_internal_test_package
