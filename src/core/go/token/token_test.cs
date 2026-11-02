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

public static void TestIsIdentifier(ж<testing.T> t) {
    var tests = slice<TestIsIdentifier_tests>([
        new("Empty", "", false),
        new("Space", " ", false),
        new("SpaceSuffix", "foo ", false),
        new("Number", "123", false),
        new("Keyword", "func", false),

        new("LettersASCII", "foo", true),
        new("MixedASCII", "_bar123", true),
        new("UppercaseKeyword", "Func", true),
        new("LettersUnicode", "fóö", true)
    ]);
    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new TestIsIdentifier_tests(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        t.Run(test.name, (ж<testing.T> tΔ1) => {
            {
                var got = IsIdentifier(testʗ1.@in); if (got != testʗ1.want) {
                    tΔ1.Fatalf("IsIdentifier(%q) = %t, want %v", testʗ1.@in, got, testʗ1.want);
                }
            }
        });
    }
}

} // end token_internal_test_package
