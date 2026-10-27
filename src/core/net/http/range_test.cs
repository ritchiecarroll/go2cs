// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using metrics = global::go.runtime.metrics_package;
using strings = strings_package;
using testing = testing_package;
using global::go.runtime;
using static global::go.net.http_package;

partial class http_internal_test_package {

partial struct ParseRangeTestsᴛ1 /*dyn*/ {
    internal @string s;
    internal int64 length;
    internal slice<global::go.net.http_package.httpRange> r;
}
public static readonly ж<slice<ParseRangeTestsᴛ1>> ᏑParseRangeTests = new StaticBox<slice<ParseRangeTestsᴛ1>>(new ParseRangeTestsᴛ1[]{
    new(""u8, 0, default!),
    new(""u8, 1000, default!),
    new("foo"u8, 0, default!),
    new("bytes="u8, 0, default!),
    new("bytes=7"u8, 10, default!),
    new("bytes= 7 "u8, 10, default!),
    new("bytes=1-"u8, 0, default!),
    new("bytes=5-4"u8, 10, default!),
    new("bytes=0-2,5-4"u8, 10, default!),
    new("bytes=2-5,4-3"u8, 10, default!),
    new("bytes=--5,4--3"u8, 10, default!),
    new("bytes=A-"u8, 10, default!),
    new("bytes=A- "u8, 10, default!),
    new("bytes=A-Z"u8, 10, default!),
    new("bytes= -Z"u8, 10, default!),
    new("bytes=5-Z"u8, 10, default!),
    new("bytes=Ran-dom, garbage"u8, 10, default!),
    new("bytes=0x01-0x02"u8, 10, default!),
    new("bytes=         "u8, 10, default!),
    new("bytes= , , ,   "u8, 10, default!),

    new("bytes=0-9"u8, 10, new global::go.net.http_package.httpRange[]{new(0, 10)}.slice()),
    new("bytes=0-"u8, 10, new global::go.net.http_package.httpRange[]{new(0, 10)}.slice()),
    new("bytes=5-"u8, 10, new global::go.net.http_package.httpRange[]{new(5, 5)}.slice()),
    new("bytes=0-20"u8, 10, new global::go.net.http_package.httpRange[]{new(0, 10)}.slice()),
    new("bytes=15-,0-5"u8, 10, new global::go.net.http_package.httpRange[]{new(0, 6)}.slice()),
    new("bytes=1-2,5-"u8, 10, new global::go.net.http_package.httpRange[]{new(1, 2), new(5, 5)}.slice()),
    new("bytes=-2 , 7-"u8, 11, new global::go.net.http_package.httpRange[]{new(9, 2), new(7, 4)}.slice()),
    new("bytes=0-0 ,2-2, 7-"u8, 11, new global::go.net.http_package.httpRange[]{new(0, 1), new(2, 1), new(7, 4)}.slice()),
    new("bytes=-5"u8, 10, new global::go.net.http_package.httpRange[]{new(5, 5)}.slice()),
    new("bytes=-15"u8, 10, new global::go.net.http_package.httpRange[]{new(0, 10)}.slice()),
    new("bytes=0-499"u8, 10000, new global::go.net.http_package.httpRange[]{new(0, 500)}.slice()),
    new("bytes=500-999"u8, 10000, new global::go.net.http_package.httpRange[]{new(500, 500)}.slice()),
    new("bytes=-500"u8, 10000, new global::go.net.http_package.httpRange[]{new(9500, 500)}.slice()),
    new("bytes=9500-"u8, 10000, new global::go.net.http_package.httpRange[]{new(9500, 500)}.slice()),
    new("bytes=0-0,-1"u8, 10000, new global::go.net.http_package.httpRange[]{new(0, 1), new(9999, 1)}.slice()),
    new("bytes=500-600,601-999"u8, 10000, new global::go.net.http_package.httpRange[]{new(500, 101), new(601, 399)}.slice()),
    new("bytes=500-700,601-999"u8, 10000, new global::go.net.http_package.httpRange[]{new(500, 201), new(601, 399)}.slice()),

    // Match Apache laxity:
    new("bytes=   1 -2   ,  4- 5, 7 - 8 , ,,"u8, 11, new global::go.net.http_package.httpRange[]{new(1, 2), new(4, 2), new(7, 2)}.slice())
}.slice());
public static ref slice<ParseRangeTestsᴛ1> ParseRangeTests => ref ((StaticBox<slice<ParseRangeTestsᴛ1>>)ᏑParseRangeTests).ValueSlot;

public static void TestParseRange(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in ParseRangeTests) {
        var r = test.r;
        var (ranges, err) = parseRange(test.s, test.length);
        if (err != default! && r != default!) {
            Ꮡt.Errorf("parseRange(%q) returned error %q"u8, test.s, err);
        }
        if (builtin.len(ranges) != builtin.len(r)) {
            Ꮡt.Errorf("len(parseRange(%q)) = %d, want %d"u8, test.s, builtin.len(ranges), builtin.len(r));
            continue;
        }
        foreach (var i in range(r)) {
            if (ranges[i].start != r[i].start) {
                Ꮡt.Errorf("parseRange(%q)[%d].start = %d, want %d"u8, test.s, i, ranges[i].start, r[i].start);
            }
            if (ranges[i].length != r[i].length) {
                Ꮡt.Errorf("parseRange(%q)[%d].length = %d, want %d"u8, test.s, i, ranges[i].length, r[i].length);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string godebugNonDefaultˢ = "/godebug/non-default-behavior/httpservecontentmaxranges:events"u8;

internal partial struct TestParseRangeLimit_type /*dyn*/ {
    internal @string name;
    internal @string godebug;
    internal nint numRanges;
    internal nint want;
    internal bool wantNonDef;
}

public static void TestParseRangeLimit(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in new TestParseRangeLimit_type[]{
        new(
            name: "default limit not exceeded"u8,
            numRanges: defaultMaxContentRanges,
            want: defaultMaxContentRanges
        ),
        new(
            name: "default limit exceeded"u8,
            numRanges: defaultMaxContentRanges + 1,
            want: 0
        ),
        new(
            name: "small limit exceeded"u8,
            godebug: "httpservecontentmaxranges=10"u8,
            numRanges: 11,
            want: 0,
            wantNonDef: true
        ),
        new(
            name: "small limit not exceeded"u8,
            godebug: "httpservecontentmaxranges=10"u8,
            numRanges: 10,
            want: 10
        ),
        new(
            name: "disabled limit"u8,
            godebug: "httpservecontentmaxranges=0"u8,
            numRanges: 2 * defaultMaxContentRanges,
            want: 2 * defaultMaxContentRanges,
            wantNonDef: true
        ),
        new(
            name: "large limit exceeded"u8,
            godebug: "httpservecontentmaxranges=300"u8,
            numRanges: 301,
            want: 0
        ),
        new(
            name: "large limit not exceeded"u8,
            godebug: "httpservecontentmaxranges=300"u8,
            numRanges: 300,
            want: 300,
            wantNonDef: true
        ),
        new(
            name: "large limit not exceeded by small input"u8,
            godebug: "httpservecontentmaxranges=300"u8,
            numRanges: 10,
            want: 10
        ),
        new(
            name: "invalid limit negative"u8,
            godebug: "httpservecontentmaxranges=-5"u8,
            numRanges: defaultMaxContentRanges + 1,
            want: 0
        ),
        new(
            name: "invalid limit non-numeric"u8,
            godebug: "httpservecontentmaxranges=abc"u8,
            numRanges: defaultMaxContentRanges + 1,
            want: 0
        )
    }.slice()) {
        ref var tc = ref heap(new TestParseRangeLimit_type(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
            tΔ1.Setenv(godebugˢ, tcʗ1.godebug);
            array<metrics.Sample> m = new(1);
            m[0].Name = godebugNonDefaultˢ;
            metrics.Read(m[..]);
            var before = m[0].Value.Uint64();

            @string rangeHeader = "bytes="u8 + strings.Repeat("0-0,"u8, tcʗ1.numRanges - 1) + "0-0"u8;
            var (ranges, err) = parseRange(rangeHeader, 10_000_000);
            if (err != default!) {
                tΔ1.Fatalf("parseRange(%q): %v"u8, rangeHeader, err);
            }
            {
                nint got = builtin.len(ranges); if (got != tcʗ1.want) {
                    tΔ1.Errorf("len(ranges) = %v, want %v"u8, got, tcʗ1.want);
                }
            }

            metrics.Read(m[..]);
            var after = m[0].Value.Uint64();
            if (tcʗ1.wantNonDef && after <= before){
                tΔ1.Errorf("metric did not increment: before=%d, after=%d"u8, before, after);
            } else 
            if (!tcʗ1.wantNonDef && after != before) {
                tΔ1.Errorf("metric unexpectedly incremented: before=%d, after=%d"u8, before, after);
            }
        });
    }
}

} // end http_internal_test_package
