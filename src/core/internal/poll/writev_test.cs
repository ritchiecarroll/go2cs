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

public static void TestConsume(ж<testing.T> Ꮡt) {
    var tests = new TestConsume_tests[]{
        new(
            @in: new slice<byte>[]{slice<byte>("foo"u8), slice<byte>("bar"u8)}.slice(),
            consume: 0,
            want: new slice<byte>[]{slice<byte>("foo"u8), slice<byte>("bar"u8)}.slice()
        ),
        new(
            @in: new slice<byte>[]{slice<byte>("foo"u8), slice<byte>("bar"u8)}.slice(),
            consume: 2,
            want: new slice<byte>[]{slice<byte>("o"u8), slice<byte>("bar"u8)}.slice()
        ),
        new(
            @in: new slice<byte>[]{slice<byte>("foo"u8), slice<byte>("bar"u8)}.slice(),
            consume: 3,
            want: new slice<byte>[]{slice<byte>("bar"u8)}.slice()
        ),
        new(
            @in: new slice<byte>[]{slice<byte>("foo"u8), slice<byte>("bar"u8)}.slice(),
            consume: 4,
            want: new slice<byte>[]{slice<byte>("ar"u8)}.slice()
        ),
        new(
            @in: new slice<byte>[]{default!, default!, default!, slice<byte>("bar"u8)}.slice(),
            consume: 1,
            want: new slice<byte>[]{slice<byte>("ar"u8)}.slice()
        ),
        new(
            @in: new slice<byte>[]{default!, default!, default!, slice<byte>("foo"u8)}.slice(),
            consume: 0,
            want: new slice<byte>[]{slice<byte>("foo"u8)}.slice()
        ),
        new(
            @in: new slice<byte>[]{default!, default!, default!}.slice(),
            consume: 0,
            want: new slice<byte>[]{}.slice()
        )
    }.slice();
    foreach (var (i, tt) in tests) {
        ref var @in = ref heap<slice<slice<byte>>>(out var Ꮡin);
        @in = tt.@in;
        poll_internal_test_package.Consume(Ꮡin, tt.consume);
        if (!reflect.DeepEqual(@in, tt.want)) {
            Ꮡt.Errorf("%d. after consume(%d) = %+v, want %+v"u8, i, tt.consume, @in, tt.want);
        }
    }
}

} // end poll_test_package
