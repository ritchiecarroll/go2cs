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

public static void TestPrefixSuffixSaver(ж<testing.T> Ꮡt) {
    var tests = new TestPrefixSuffixSaver_tests[]{
        new(
            N: 2,
            writes: default!,
            want: ""u8
        ),
        new(
            N: 2,
            writes: new @string[]{"a"u8}.slice(),
            want: "a"u8
        ),
        new(
            N: 2,
            writes: new @string[]{"abc"u8, "d"u8}.slice(),
            want: "abcd"u8
        ),
        new(
            N: 2,
            writes: new @string[]{"abc"u8, "d"u8, "e"u8}.slice(),
            want: "ab\n... omitting 1 bytes ...\nde"u8
        ),
        new(
            N: 2,
            writes: new @string[]{"ab______________________yz"u8}.slice(),
            want: "ab\n... omitting 22 bytes ...\nyz"u8
        ),
        new(
            N: 2,
            writes: new @string[]{"ab_______________________y"u8, "z"u8}.slice(),
            want: "ab\n... omitting 23 bytes ...\nyz"u8
        )
    }.slice();
    foreach (var (i, tt) in tests) {
        var w = Ꮡ(new prefixSuffixSaver(N: tt.N));
        foreach (var (_, s) in tt.writes) {
            var (n, err) = io.WriteString(new global::go.os.exec_package.prefixSuffixSaverжWriter(w), s);
            if (err != default! || n != len(s)) {
                Ꮡt.Errorf("%d. WriteString(%q) = %v, %v; want %v, %v"u8, i, s, n, err, len(s), (any)(default!));
            }
        }
        {
            @string got = ((@string)w.Bytes()); if (got != tt.want) {
                Ꮡt.Errorf("%d. Bytes = %q; want %q"u8, i, got, tt.want);
            }
        }
    }
}

} // end exec_internal_test_package
