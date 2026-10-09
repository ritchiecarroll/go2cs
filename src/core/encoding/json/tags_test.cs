// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using testing = testing_package;
using static go.encoding.json_package;

partial class json_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fieldFoobarFooˢ = "field,foobar,foo"u8;

internal partial struct TestTagParsing_type /*dyn*/ {
    internal @string opt;
    internal bool want;
}

public static void TestTagParsing(ж<testing.T> Ꮡt) {
    var (name, opts) = parseTag(fieldFoobarFooˢ);
    if (name != "field"u8) {
        Ꮡt.Fatalf("name = %q, want field"u8, name);
    }
    foreach (var (_, tt) in new TestTagParsing_type[]{
        new("foobar"u8, true),
        new("foo"u8, true),
        new("bar"u8, false)
    }.slice()) {
        if (opts.Contains(tt.opt) != tt.want) {
            Ꮡt.Errorf("Contains(%q) = %v, want %v"u8, tt.opt, !tt.want, tt.want);
        }
    }
}

} // end json_internal_test_package
