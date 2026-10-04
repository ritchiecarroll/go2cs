// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using strings = strings_package;
using testing = testing_package;
using io = io_package;
using reflect = reflect_package;
using static global::go.go.ast_package;
using token = global::go.go.token_package;

partial class ast_internal_test_package {

// basic types
// maps
// pointers
// arrays
// slices
// structs

[GoType("dyn")] partial struct testsᴛ1 {
    internal any x; // x is printed as s
    internal @string s;
}

    [GoType("dyn")] partial struct Δtype {
        internal nint x;
    }

    [GoType("dyn")] partial struct Δtypeᴛ1 {
        public nint X;
        internal nint y;
    }

    [GoType("dyn")] partial struct Δtypeᴛ2 {
        public nint X, Y;
    }
internal static slice<testsᴛ1> tests = new testsᴛ1[]{
    new(default!, "0  nil"u8),
    new(true, "0  true"u8),
    new((nint)(42), "0  42"u8),
    new(3.14D, "0  3.14"u8),
    new(1D + 2.718D.i(), "0  (1+2.718i)"u8),
    new((@string)"foobar"u8, "0  \"foobar\""u8),
    new(new map<global::go.go.ast_package.Expr, @string>{}, @"0  map[ast.Expr]string (len = 0) {}"u8),
    new(new map<@string, nint>{["a"u8] = 1},
        """
0  map[string]int (len = 1) {
		1  .  "a": 1
		2  }
"""u8),
    new(@new<nint>(), "0  *0"u8),
    new(new nint[]{}.array(), @"0  [0]int {}"u8),
    new(new nint[]{1, 2, 3}.array(),
        """
0  [3]int {
		1  .  0: 1
		2  .  1: 2
		3  .  2: 3
		4  }
"""u8),
    new(new nint[]{42}.array(),
        """
0  [1]int {
		1  .  0: 42
		2  }
"""u8),
    new(new nint[]{}.slice(), @"0  []int (len = 0) {}"u8),
    new(new nint[]{1, 2, 3}.slice(),
        """
0  []int (len = 3) {
		1  .  0: 1
		2  .  1: 2
		3  .  2: 3
		4  }
"""u8),
    new(new EmptyStruct(), @"0  struct {} {}"u8),
    new(new Δtype(7), @"0  struct { x int } {}"u8),
    new(new Δtypeᴛ1(42, 991),
        """
0  struct { X int; y int } {
		1  .  X: 42
		2  }
"""u8),
    new(new Δtypeᴛ2(42, 991),
        """
0  struct { X int; Y int } {
		1  .  X: 42
		2  .  Y: 991
		3  }
"""u8)
}.slice();

// Split s into lines, trim whitespace from all lines, and return
// the concatenated non-empty lines.
internal static @string trim(@string s) {
    var lines = strings.Split(s, "\n"u8);
    nint i = 0;
    foreach (var (_, vᴛ1) in lines) {
        var line = vᴛ1;

        line = strings.TrimSpace(line);
        if (line != ""u8) {
            lines[i] = line;
            i++;
        }
    }
    return strings.Join(lines.slice(0, i), "\n"u8);
}

public static void TestPrint(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    foreach (var (_, test) in tests) {
        buf.Reset();
        {
            var err = Fprint(new ast_test_package.strings_BuilderжWriter(Ꮡbuf), nil, test.x, default!); if (err != default!) {
                Ꮡt.Errorf("Fprint failed: %s"u8, err);
            }
        }
        {
            @string s = trim(buf.String());
            @string ts = trim(test.s); if (s != ts) {
                Ꮡt.Errorf("got:\n%s\nexpected:\n%s\n"u8, s, ts);
            }
        }
    }
}

} // end ast_internal_test_package
