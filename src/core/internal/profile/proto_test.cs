// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using slices = slices_package;
using testing = testing_package;
using static go.@internal.profile_package;

partial class profile_internal_test_package {

internal partial struct TestPackedEncoding_testcase /*dyn*/ {
    internal slice<uint64> uint64s;
    internal slice<int64> int64s;
    internal slice<byte> encoded;
}

public static void TestPackedEncoding(ж<testing.T> Ꮡt) {
    foreach (var (i, tc) in new TestPackedEncoding_testcase[]{
        new(
            new uint64[]{0, 1, 10, 100, 1000, 10000}.slice(),
            new int64[]{1000, 0, 1000}.slice(),
            new byte[]{10, 8, 0, 1, 10, 100, 232, 7, 144, 78, 18, 5, 232, 7, 0, 232, 7}.slice()
        ),
        new(
            new uint64[]{10000}.slice(),
            default!,
            new byte[]{8, 144, 78}.slice()
        ),
        new(
            default!,
            new int64[]{-10000}.slice(),
            new byte[]{16, 240, 177, 255, 255, 255, 255, 255, 255, 255, 1}.slice()
        )
    }.slice()) {
        var source = Ꮡ(new packedInts(tc.uint64s, tc.int64s));
        {
            var (got, want) = (marshal(new profile_internal_test_package.packedIntsжmessage(source)), tc.encoded); if (!slices.Equal<slice<byte>, byte>(got, want)) {
                Ꮡt.Errorf("failed encode %d, got %v, want %v"u8, i, got, want);
            }
        }
        var dest = @new<packedInts>();
        {
            var err = unmarshal(tc.encoded, new profile_internal_test_package.packedIntsжmessage(dest)); if (err != default!) {
                Ꮡt.Errorf("failed decode %d: %v"u8, i, err);
                continue;
            }
        }
        {
            var (got, want) = (dest.Value.uint64s, tc.uint64s); if (!slices.Equal<slice<uint64>, uint64>(got, want)) {
                Ꮡt.Errorf("failed decode uint64s %d, got %v, want %v"u8, i, got, want);
            }
        }
        {
            var (got, want) = (dest.Value.int64s, tc.int64s); if (!slices.Equal<slice<int64>, int64>(got, want)) {
                Ꮡt.Errorf("failed decode int64s %d, got %v, want %v"u8, i, got, want);
            }
        }
    }
}

internal partial struct packedInts {
    internal slice<uint64> uint64s;
    internal slice<int64> int64s;
}

internal static slice<Func<ж<global::go.@internal.profile_package.buffer>, global::go.@internal.profile_package.message, error>> decoder(this ref packedInts u) {
    return new Func<ж<global::go.@internal.profile_package.buffer>, global::go.@internal.profile_package.message, error>[]{
        default!,
        (ж<global::go.@internal.profile_package.buffer> b, global::go.@internal.profile_package.message m) => decodeUint64s(ref (b).DerefOrNull(), m._<ж<packedInts>>().of(packedInts.Ꮡuint64s)),
        (ж<global::go.@internal.profile_package.buffer> b, global::go.@internal.profile_package.message m) => decodeInt64s(ref (b).DerefOrNull(), m._<ж<packedInts>>().of(packedInts.Ꮡint64s))
    }.slice();
}

internal static void encode(this ref packedInts u, ж<global::go.@internal.profile_package.buffer> Ꮡb) {
    encodeUint64s(ref (Ꮡb).DerefOrNull(), 1, u.uint64s);
    encodeInt64s(ref (Ꮡb).DerefOrNull(), 2, u.int64s);
}

} // end profile_internal_test_package
