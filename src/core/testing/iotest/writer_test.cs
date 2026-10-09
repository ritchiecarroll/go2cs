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
internal static slice<truncateWriterTestsᴛ1> truncateWriterTests = new truncateWriterTestsᴛ1[]{
    new("hello"u8, ""u8, -1, 5),
    new("world"u8, ""u8, 0, 5),
    new("abcde"u8, "abc"u8, 3, 5),
    new("edcba"u8, "edcba"u8, 7, 5)
}.slice();

public static void TestTruncateWriter(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, tt) in truncateWriterTests) {
        var buf = @new<strings.Builder>();
        var tw = TruncateWriter(new iotest_test_package.strings_BuilderжWriter(buf), tt.trunc);
        var (n, err) = tw.Write(slice<byte>(tt.@in));
        if (err != default!) {
            Ꮡt.Errorf("Unexpected error %v for\n\t%+v"u8, err, tt);
        }
        {
            @string g = buf.String();
            @string w = tt.want; if (g != w) {
                Ꮡt.Errorf("got %q, expected %q"u8, g, w);
            }
        }
        {
            nint g = n;
            nint w = tt.n; if (g != w) {
                Ꮡt.Errorf("read %d bytes, but expected to have read %d bytes for\n\t%+v"u8, g, w, tt);
            }
        }
    }
}

} // end iotest_internal_test_package
