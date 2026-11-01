// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δruntime = runtime_package;
using testing = testing_package;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

internal partial struct TestPrintQuoted_type /*dyn*/ {
    internal @string @in, expected;
}

public static void TestPrintQuoted(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in slice<TestPrintQuoted_type>([
        new(@in: "baz", expected: @"""baz"""),
        new(@in: "foobar", expected: @"""foobar"""),
        // make sure newlines get escaped
        new(@in: "baz\n", expected: @"""baz\n"""),
        // make sure null and escape bytes are properly escaped
        new(@in: "b\u001bit", expected: @"""b\x1bit"""),
        new(@in: "b\u0000ar", expected: @"""b\x00ar"""),
        // verify that simple 16-bit unicode runes are escaped with \u, including a greek upper-case sigma and an arbitrary unicode character.
        new(@in: "\u1234Σ", expected: @"""\u1234\u03a3"""),
        // verify that 32-bit unicode runes are escaped with \U along with tabs
        new(@in: "fizz\tle", expected: @"""fizz\tle"""),
        new(@in: "\U00045678boop", expected: @"""\U00045678boop"""),
        // verify carriage returns and backslashes get escaped along with our nulls, newlines and a 32-bit unicode character
        new(@in: "fiz\\zl\re", expected: @"""fiz\\zl\re""")
    ])) {
        ref var tbl = ref heap(new TestPrintQuoted_type(), out var Ꮡtbl);
        tbl = vᴛ1;

        var tblʗ1 = tbl;
        t.Run(tbl.@in, (ж<testing.T> tΔ1) => {
            @string @out = runtime_internal_test_package.DumpPrintQuoted(tblʗ1.@in);
            if (@out != tblʗ1.expected) {
                tΔ1.Errorf("unexpected output for print(escaped(%q));\n got: %s\nwant: %s", tblʗ1.@in, @out, tblʗ1.expected);
            }
        });
    }
}

} // end runtime_test_package
