// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.os;

using io = io_package;
using testing = testing_package;
using static go.os.exec_package;

partial class exec_internal_test_package {

internal partial struct TestPrefixSuffixSaver_tests /*dyn*/ {
    public nint N;
    internal slice<@string> writes;
    internal @string want;
}

public static void TestPrefixSuffixSaver(ж<testing.T> t) {
    var tests = slice<TestPrefixSuffixSaver_tests>([
        new(
            N: 2,
            writes: default,
            want: ""
        ),
        new(
            N: 2,
            writes: ["a"],
            want: "a"
        ),
        new(
            N: 2,
            writes: ["abc", "d"],
            want: "abcd"
        ),
        new(
            N: 2,
            writes: ["abc", "d", "e"],
            want: "ab\n... omitting 1 bytes ...\nde"
        ),
        new(
            N: 2,
            writes: ["ab______________________yz"],
            want: "ab\n... omitting 22 bytes ...\nyz"
        ),
        new(
            N: 2,
            writes: ["ab_______________________y", "z"],
            want: "ab\n... omitting 23 bytes ...\nyz"
        )
    ]);
    foreach (var (i, tt) in range(tests)) {
        var w = Ꮡ(new prefixSuffixSaver(N: tt.N));
        foreach (var (_, s) in range(tt.writes)) {
            var (n, err) = io.WriteString(new global::go.os.exec_package.prefixSuffixSaverжWriter(w), s);
            if (err != nil || n != len(s)) {
                t.Errorf("%d. WriteString(%q) = %v, %v; want %v, %v", i, s, n, err, len(s), (any)(default));
            }
        }
        @string got = (@string)w.Bytes(); if (got != tt.want) {
            t.Errorf("%d. Bytes = %q; want %q", i, got, tt.want);
        }
    }
}

} // end exec_internal_test_package
