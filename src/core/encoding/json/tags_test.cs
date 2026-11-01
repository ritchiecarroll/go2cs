// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !goexperiment.jsonv2
namespace go.encoding;

using testing = testing_package;
using static go.encoding.json_package;

partial class json_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fieldFoobarFooˢ => ref fieldFoobarFooˢᵇ;
private static readonly @string fieldFoobarFooˢᵇ = "field,foobar,foo"u8;

internal partial struct TestTagParsing_type /*dyn*/ {
    internal @string opt;
    internal bool want;
}

public static void TestTagParsing(ж<testing.T> t) {
    var (name, opts) = parseTag(fieldFoobarFooˢ);
    if (name != "field"u8) {
        t.Fatalf("name = %q, want field", name);
    }
    foreach (var (_, tt) in slice<TestTagParsing_type>([
        new("foobar", true),
        new("foo", true),
        new("bar", false)
    ])) {
        if (opts.Contains(tt.opt) != tt.want) {
            t.Errorf("Contains(%q) = %v, want %v", tt.opt, !tt.want, tt.want);
        }
    }
}

} // end json_internal_test_package
