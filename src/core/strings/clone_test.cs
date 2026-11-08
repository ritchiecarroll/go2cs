// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using strings = strings_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using static go.strings_internal_test_package;

partial class strings_test_package {

internal static @string emptyString;

public static void TestClone(ж<testing.T> t) {
    slice<@string> cloneTests = slice<@string>([
        "",
        strings.Clone(""u8),
        strings.Repeat("a", 42)[..0],
        "short",
        strings.Repeat("a", 42)
    ]);
    foreach (var (_, input) in range(cloneTests)) {
        @string clone = strings.Clone(input);
        if (clone != input) {
            t.Errorf("Clone(%q) = %q; want %q", input, clone, input);
        }

        if (len(input) != 0 && @unsafe.StringData(clone) == @unsafe.StringData(input)) {
            t.Errorf("Clone(%q) return value should not reference inputs backing memory.", input);
        }

        if (len(input) == 0 && @unsafe.StringData(clone) != @unsafe.StringData(emptyString)) {
            t.Errorf("Clone(%#v) return value should be equal to empty string.", @unsafe.StringData(input).OrTypedNil());
        }
    }
}

public static void BenchmarkClone(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string str = strings.Repeat("a", 42);
    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        stringSink = strings.Clone(str);
    }
}

} // end strings_test_package
