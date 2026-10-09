// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using testing = testing_package;
using static global::go.net.http_package;

partial class http_internal_test_package {

// Match Apache laxity:

partial struct ParseRangeTestsᴛ1 /*dyn*/ {
    internal @string s;
    internal int64 length;
    internal slice<global::go.net.http_package.httpRange> r;
}
public static slice<ParseRangeTestsᴛ1> ParseRangeTests = new ParseRangeTestsᴛ1[]{
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
    new("bytes=   1 -2   ,  4- 5, 7 - 8 , ,,"u8, 11, new global::go.net.http_package.httpRange[]{new(1, 2), new(4, 2), new(7, 2)}.slice())
}.slice();

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
        foreach (var (i, _) in r) {
            if (ranges[i].start != r[i].start) {
                Ꮡt.Errorf("parseRange(%q)[%d].start = %d, want %d"u8, test.s, i, ranges[i].start, r[i].start);
            }
            if (ranges[i].length != r[i].length) {
                Ꮡt.Errorf("parseRange(%q)[%d].length = %d, want %d"u8, test.s, i, ranges[i].length, r[i].length);
            }
        }
    }
}

} // end http_internal_test_package
