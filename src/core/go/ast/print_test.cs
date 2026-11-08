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

partial struct testsᴛ1 /*dyn*/ {
    internal any x; // x is printed as s
    internal @string s;
}
    partial struct Δtype /*dyn*/ {
        internal nint x;
    }
    partial struct Δtypeᴛ1 /*dyn*/ {
        public nint X;
        internal nint y;
    }
    partial struct Δtypeᴛ2 /*dyn*/ {
        public nint X, Y;
    }
internal static slice<testsᴛ1> tests = slice<testsᴛ1>([
    // basic types
    new(default, "0  nil"),
    new(true, "0  true"),
    new((nint)42, "0  42"),
    new(3.14D, "0  3.14"),
    new(1D + 2.718D.i(), "0  (1+2.718i)"),
    new((@string)"foobar", "0  \"foobar\""),

    // maps
    new(new map<global::go.go.ast_package.Expr, @string>{}, @"0  map[ast.Expr]string (len = 0) {}"),
    new(new map<@string, nint>{["a"] = 1},
        """
0  map[string]int (len = 1) {
		1  .  "a": 1
		2  }
"""u8.Raw()),

    // pointers
    new(@new<nint>(), "0  *0"),

    // arrays
    new(new nint[]{}.array(), @"0  [0]int {}"),
    new(new nint[]{1, 2, 3}.array(),
        """
0  [3]int {
		1  .  0: 1
		2  .  1: 2
		3  .  2: 3
		4  }
"""u8.Raw()),
    new(new nint[]{42}.array(),
        """
0  [1]int {
		1  .  0: 42
		2  }
"""u8.Raw()),

    // slices
    new(slice<nint>([]), @"0  []int (len = 0) {}"),
    new(slice<nint>([1, 2, 3]),
        """
0  []int (len = 3) {
		1  .  0: 1
		2  .  1: 2
		3  .  2: 3
		4  }
"""u8.Raw()),

    // structs
    new(new EmptyStruct(), @"0  struct {} {}"),
    new(new Δtype(7), @"0  struct { x int } {}"),
    new(new Δtypeᴛ1(42, 991),
        """
0  struct { X int; y int } {
		1  .  X: 42
		2  }
"""u8.Raw()),
    new(new Δtypeᴛ2(42, 991),
        """
0  struct { X int; Y int } {
		1  .  X: 42
		2  .  Y: 991
		3  }
"""u8.Raw())
]);

// Split s into lines, trim whitespace from all lines, and return
// the concatenated non-empty lines.
internal static @string trim(@string s) {
    var lines = strings.Split(s, "\n");
    nint i = 0;
    foreach (var (_, vᴛ1) in range(lines)) {
        var line = vᴛ1;

        line = strings.TrimSpace(line);
        if (line != ""u8) {
            lines[i] = line;
            i++;
        }
    }
    return strings.Join(lines.slice(0, i), "\n");
}

public static void TestPrint(ж<testing.T> t) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    foreach (var (_, test) in range(tests)) {
        buf.Reset();
        var err = Fprint(new ast_test_package.strings_BuilderжWriter(Ꮡbuf), nil, test.x, default); if (err != nil) {
            t.Errorf("Fprint failed: %s", err);
        }
        @string s = trim(buf.String());
        @string ts = trim(test.s); if (s != ts) {
            t.Errorf("got:\n%s\nexpected:\n%s\n", s, ts);
        }
    }
}

} // end ast_internal_test_package
