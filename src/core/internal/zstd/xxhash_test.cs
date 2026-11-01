// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bytes = bytes_package;
using os = os_package;
using exec = go.os.exec_package;
using strconv = go.strconv_package;
using testing = testing_package;
using go;
using go.os;
using static go.@internal.zstd_package;

partial class zstd_internal_test_package {

partial struct xxHashTestsᴛ1 /*dyn*/ {
    internal @string data;
    internal uint64 hash;
}
internal static slice<xxHashTestsᴛ1> xxHashTests = slice<xxHashTestsᴛ1>([
    new(
        "hello, world",
        0xb33a384e6d1b1242UL
    ),
    new(
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789$",
        0x1032d841e824f998UL
    )
]);

public static void TestXXHash(ж<testing.T> t) {
    global::go.@internal.zstd_package.xxhash64 xh = new();
    foreach (var (i, test) in xxHashTests) {
        xh.reset();
        xh.update(slice<byte>(test.data));
        {
            var got = xh.digest(); if (got != test.hash) {
                t.Errorf("#%d: got %#x want %#x", i, got, test.hash);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingExpensiveTestInˢ = (@string)"skipping expensive test in short mode"u8;
internal static ref readonly @string testdataIsaacNewtonˢ => ref testdataIsaacNewtonˢᵇ;
private static readonly @string testdataIsaacNewtonˢᵇ = "../../testdata/Isaac.Newton-Opticks.txt"u8;

public static void TestLargeXXHash(ж<testing.T> t) {
    if (testing.Short()) {
        t.Skip(skippingExpensiveTestInˢ);
    }

    var (data, err) = os.ReadFile(testdataIsaacNewtonˢ);
    if (err != nil) {
        t.Fatal(err);
    }

    global::go.@internal.zstd_package.xxhash64 xh = new();
    xh.reset();
    nint i = 0;
    while (i < builtin.len(data)) {
        // Write varying amounts to test buffering.
        nint c = i % 4094 + 1;
        if (i + c > builtin.len(data)) {
            c = builtin.len(data) - i;
        }
        xh.update(data.slice(i, i + c));
        i += c;
    }

    var got = xh.digest();
    var want = (uint64)0xf0dd39fd7e063f82UL;
    if (got != want) {
        t.Errorf("got %#x want %#x", got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string xxhsumˢ => ref xxhsumˢᵇ;
private static readonly @string xxhsumˢᵇ = "xxhsum"u8;
internal static readonly object skippingBecauseXxhsumNotˢ = (@string)"skipping because xxhsum not found"u8;

internal static @string findXxhsum(testing.TB t) {
    var (xxhsum, err) = exec.LookPath(xxhsumˢ);
    if (err != nil) {
        t.Skip(skippingBecauseXxhsumNotˢ);
    }
    return xxhsum;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string h64ˢ => ref h64ˢᵇ;
private static readonly @string h64ˢᵇ = "-H64"u8;

public static void FuzzXXHash(ж<testing.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    @string xxhsum = findXxhsum(new zstd_internal_test_package.testing_FжTB(Ꮡf));

    foreach (var (_, test) in xxHashTests) {
        f.Add(slice<byte>(test.data));
    }
    f.Add(bytes.Repeat(slice<byte>("abcdefghijklmnop"u8), 256));
    bytes.Buffer buf = default;
    for (nint i = 0; i < 256; i++) {
        buf.WriteByte((byte)i);
    }
    f.Add(bytes.Repeat(buf.Bytes(), 64));
    f.Add(bigData(new zstd_internal_test_package.testing_FжTB(Ꮡf)));

    Ꮡf.Fuzz((ж<testing.T> t, slice<byte> b) => {
        var cmd = exec.Command(xxhsum, h64ˢ);
        cmd.Stdin = new zstd_internal_test_package.bytes_ReaderжReader(bytes.NewReader(b));
        ref var hhsumHash = ref heap(new bytes.Buffer(), out var ᏑhhsumHash);
        cmd.Stdout = new zstd_internal_test_package.bytes_BufferжWriter(ᏑhhsumHash);
        {
            var errΔ1 = cmd.Run(); if (errΔ1 != nil) {
                t.Fatalf("running hhsum failed: %v", errΔ1);
            }
        }
        var hhHashBytes = bytes.Fields(bytes.TrimSpace(hhsumHash.Bytes()))[0];
        var (hhHash, err) = strconv.ParseUint((@string)hhHashBytes, 16, 64);
        if (err != nil) {
            t.Fatalf("could not parse hash %q: %v", hhHashBytes, err);
        }

        global::go.@internal.zstd_package.xxhash64 xh = new();
        xh.reset();
        xh.update(b);
        var goHash = xh.digest();

        if (goHash != hhHash) {
            t.Errorf("Go hash %#x != xxhsum hash %#x", goHash, hhHash);
        }
    });
}

} // end zstd_internal_test_package
