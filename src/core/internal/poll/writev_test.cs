// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using poll = go.@internal.poll_package;
using reflect = reflect_package;
using testing = testing_package;
using go.@internal;
using static go.@internal.poll_internal_test_package;

partial class poll_test_package {

internal partial struct TestConsume_tests /*dyn*/ {
    internal slice<slice<byte>> @in;
    internal int64 consume;
    internal slice<slice<byte>> want;
}

public static void TestConsume(ж<testing.T> t) {
    var tests = slice<TestConsume_tests>([
        new(
            @in: [slice<byte>("foo"u8), slice<byte>("bar"u8)],
            consume: 0,
            want: [slice<byte>("foo"u8), slice<byte>("bar"u8)]
        ),
        new(
            @in: [slice<byte>("foo"u8), slice<byte>("bar"u8)],
            consume: 2,
            want: [slice<byte>("o"u8), slice<byte>("bar"u8)]
        ),
        new(
            @in: [slice<byte>("foo"u8), slice<byte>("bar"u8)],
            consume: 3,
            want: [slice<byte>("bar"u8)]
        ),
        new(
            @in: [slice<byte>("foo"u8), slice<byte>("bar"u8)],
            consume: 4,
            want: [slice<byte>("ar"u8)]
        ),
        new(
            @in: [default, default, default, slice<byte>("bar"u8)],
            consume: 1,
            want: [slice<byte>("ar"u8)]
        ),
        new(
            @in: [default, default, default, slice<byte>("foo"u8)],
            consume: 0,
            want: [slice<byte>("foo"u8)]
        ),
        new(
            @in: [default, default, default],
            consume: 0,
            want: []
        )
    ]);
    foreach (var (i, tt) in range(tests)) {
        ref var @in = ref heap<slice<slice<byte>>>(tt.@in, out var Ꮡin);
        poll_internal_test_package.Consume(Ꮡin, tt.consume);
        if (!reflect.DeepEqual(@in, tt.want)) {
            t.Errorf("%d. after consume(%d) = %+v, want %+v", i, tt.consume, @in, tt.want);
        }
    }
}

} // end poll_test_package
