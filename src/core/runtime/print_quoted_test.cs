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

public static void TestPrintQuoted(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in new TestPrintQuoted_type[]{
        new(@in: "baz"u8, expected: @"""baz"""u8),
        new(@in: "foobar"u8, expected: @"""foobar"""u8),
        // make sure newlines get escaped
        new(@in: "baz\n"u8, expected: @"""baz\n"""u8),
        // make sure null and escape bytes are properly escaped
        new(@in: "b\u001bit"u8, expected: @"""b\x1bit"""u8),
        new(@in: "b\u0000ar"u8, expected: @"""b\x00ar"""u8),
        // verify that simple 16-bit unicode runes are escaped with \u, including a greek upper-case sigma and an arbitrary unicode character.
        new(@in: "\u1234Σ"u8, expected: @"""\u1234\u03a3"""u8),
        // verify that 32-bit unicode runes are escaped with \U along with tabs
        new(@in: "fizz\tle"u8, expected: @"""fizz\tle"""u8),
        new(@in: "\U00045678boop"u8, expected: @"""\U00045678boop"""u8),
        // verify carriage returns and backslashes get escaped along with our nulls, newlines and a 32-bit unicode character
        new(@in: "fiz\\zl\re"u8, expected: @"""fiz\\zl\re"""u8)
    }.slice()) {
        ref var tbl = ref heap(new TestPrintQuoted_type(), out var Ꮡtbl);
        tbl = vᴛ1;

        var tblʗ1 = tbl;
        Ꮡt.Run(tbl.@in, (ж<testing.T> tΔ1) => {
            @string @out = runtime_internal_test_package.DumpPrintQuoted(tblʗ1.@in);
            if (@out != tblʗ1.expected) {
                tΔ1.Errorf("unexpected output for print(escaped(%q));\n got: %s\nwant: %s"u8, tblʗ1.@in, @out, tblʗ1.expected);
            }
        });
    }
}

} // end runtime_test_package
