// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.testing;

using strings = strings_package;
using testing = testing_package;
using io = io_package;
using static go.testing.iotest_package;

partial class iotest_internal_test_package {

partial struct truncateWriterTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal @string want;
    internal int64 trunc;
    internal nint n;
}
internal static slice<truncateWriterTestsᴛ1> truncateWriterTests = slice<truncateWriterTestsᴛ1>([
    new("hello", "", -1, 5),
    new("world", "", 0, 5),
    new("abcde", "abc", 3, 5),
    new("edcba", "edcba", 7, 5)
]);

public static void TestTruncateWriter(ж<testing.T> t) {
    foreach (var (_, tt) in truncateWriterTests) {
        var buf = @new<strings.Builder>();
        var tw = TruncateWriter(new iotest_test_package.strings_BuilderжWriter(buf), tt.trunc);
        var (n, err) = tw.Write(slice<byte>(tt.@in));
        if (err != nil) {
            t.Errorf("Unexpected error %v for\n\t%+v", err, tt);
        }
        {
            @string g = buf.String();
            @string w = tt.want; if (g != w) {
                t.Errorf("got %q, expected %q", g, w);
            }
        }
        {
            nint g = n;
            nint w = tt.n; if (g != w) {
                t.Errorf("read %d bytes, but expected to have read %d bytes for\n\t%+v", g, w, tt);
            }
        }
    }
}

} // end iotest_internal_test_package
