// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using testing = testing_package;
using static global::go.go.token_package;

partial class token_internal_test_package {

internal partial struct TestIsIdentifier_tests /*dyn*/ {
    internal @string name;
    internal @string @in;
    internal bool want;
}

public static void TestIsIdentifier(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestIsIdentifier_tests[]{
        new("Empty"u8, ""u8, false),
        new("Space"u8, " "u8, false),
        new("SpaceSuffix"u8, "foo "u8, false),
        new("Number"u8, "123"u8, false),
        new("Keyword"u8, "func"u8, false),
        new("LettersASCII"u8, "foo"u8, true),
        new("MixedASCII"u8, "_bar123"u8, true),
        new("UppercaseKeyword"u8, "Func"u8, true),
        new("LettersUnicode"u8, "fóö"u8, true)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new TestIsIdentifier_tests(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.name, (ж<testing.T> tΔ1) => {
            {
                var got = IsIdentifier(testʗ1.@in); if (got != testʗ1.want) {
                    tΔ1.Fatalf("IsIdentifier(%q) = %t, want %v"u8, testʗ1.@in, got, testʗ1.want);
                }
            }
        });
    }
}

} // end token_internal_test_package
