// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bytes = bytes_package;
using sha256 = crypto.sha256_package;
using fmt = fmt_package;
using race = go.@internal.race_package;
using testenv = go.@internal.testenv_package;
using io = io_package;
using os = os_package;
using exec = go.os.exec_package;
using filepath = path.filepath_package;
using strings = strings_package;
using sync = go.sync_package;
using testing = testing_package;
using crypto;
using fs = go.io.fs_package;
using go;
using go.@internal;
using go.io;
using go.os;
using hash = hash_package;
using path;
using static go.@internal.zstd_package;

partial class zstd_internal_test_package {

// a small compressed .debug_ranges section.
// tests holds some simple test cases, including some found by fuzzing.

partial struct testsᴛ1 /*dyn*/ {
    internal @string name, uncompressed, compressed;
}
internal static slice<testsᴛ1> tests = new testsᴛ1[]{
    new(
        "hello"u8,
        "hello, world\n"u8,
        ((@string)(new byte[]{0x28, 0xb5, 0x2f, 0xfd, 0x24, 0x0d, 0x69, 0x00, 0x00, 0x68, 0x65, 0x6c, 0x6c, 0x6f, 0x2c, 0x20, 0x77, 0x6f, 0x72, 0x6c, 0x64, 0x0a, 0x4c, 0x1f, 0xf9, 0xf1}))
    ),
    new(
        "ranges"u8,
        ((@string)(new byte[]{0xcc, 0x11, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xd5, 0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})) + "\x1c\x14\x00\x00\x00\x00\x00\x00\x72\x14\x00\x00\x00\x00\x00\x00"u8 + ((@string)(new byte[]{0x9d, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xd5, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})) + "\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00"u8 + ((@string)(new byte[]{0xfb, 0x12, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x09, 0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})) + ((@string)(new byte[]{0x0c, 0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xcb, 0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})) + "\x29\x14\x00\x00\x00\x00\x00\x00\x4e\x14\x00\x00\x00\x00\x00\x00"u8 + ((@string)(new byte[]{0x9d, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xd5, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})) + "\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00"u8 + ((@string)(new byte[]{0xfb, 0x12, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x09, 0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})) + ((@string)(new byte[]{0x67, 0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xcb, 0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})) + ((@string)(new byte[]{0x9d, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xd5, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})) + "\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00"u8 + "\x5f\x0b\x00\x00\x00\x00\x00\x00\x6c\x0b\x00\x00\x00\x00\x00\x00"u8 + "\x7d\x0b\x00\x00\x00\x00\x00\x00\x7e\x0c\x00\x00\x00\x00\x00\x00"u8 + "\x38\x0f\x00\x00\x00\x00\x00\x00\x5c\x0f\x00\x00\x00\x00\x00\x00"u8 + "\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00"u8 + ((@string)(new byte[]{0x83, 0x0c, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xfa, 0x0c, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})) + ((@string)(new byte[]{0xfd, 0x0d, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xef, 0x0e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})) + "\x14\x0f\x00\x00\x00\x00\x00\x00\x38\x0f\x00\x00\x00\x00\x00\x00"u8 + ((@string)(new byte[]{0x9f, 0x0f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xac, 0x0f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})) + ((@string)(new byte[]{0xdb, 0x0f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0x0f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})) + "\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00"u8 + ((@string)(new byte[]{0xfd, 0x0d, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xd8, 0x0e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})) + ((@string)(new byte[]{0x9f, 0x0f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xac, 0x0f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})) + ((@string)(new byte[]{0xdb, 0x0f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0x0f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})) + "\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00"u8 + ((@string)(new byte[]{0xfa, 0x0c, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xea, 0x0d, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})) + ((@string)(new byte[]{0xef, 0x0e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x14, 0x0f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})) + ((@string)(new byte[]{0x5c, 0x0f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x9f, 0x0f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})) + ((@string)(new byte[]{0xac, 0x0f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xdb, 0x0f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})) + ((@string)(new byte[]{0xff, 0x0f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2c, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})) + "\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00"u8 + ((@string)(new byte[]{0x60, 0x11, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xd1, 0x16, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})) + "\x40\x0b\x00\x00\x00\x00\x00\x00\x2c\x10\x00\x00\x00\x00\x00\x00"u8 + "\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00"u8 + ((@string)(new byte[]{0x7a, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xb6, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})) + ((@string)(new byte[]{0x9f, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xa7, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})) + "\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00"u8 + ((@string)(new byte[]{0x7a, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xa9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})) + ((@string)(new byte[]{0x9f, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xa7, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})) + "\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00"u8,
        ((@string)(new byte[]{0x28, 0xb5, 0x2f, 0xfd, 0x64, 0xa0, 0x01, 0x2d, 0x05, 0x00, 0xc4, 0x04, 0xcc, 0x11, 0x00, 0xd5})) + ((@string)(new byte[]{0x13, 0x00, 0x1c, 0x14, 0x00, 0x72, 0x9d, 0xd5, 0xfb, 0x12, 0x00, 0x09, 0x0c, 0x13, 0xcb, 0x13})) + ((@string)(new byte[]{0x29, 0x4e, 0x67, 0x5f, 0x0b, 0x6c, 0x0b, 0x7d, 0x0b, 0x7e, 0x0c, 0x38, 0x0f, 0x5c, 0x0f, 0x83})) + ((@string)(new byte[]{0x0c, 0xfa, 0x0c, 0xfd, 0x0d, 0xef, 0x0e, 0x14, 0x38, 0x9f, 0x0f, 0xac, 0x0f, 0xdb, 0x0f, 0xff})) + ((@string)(new byte[]{0x0f, 0xd8, 0x9f, 0xac, 0xdb, 0xff, 0xea, 0x5c, 0x2c, 0x10, 0x60, 0xd1, 0x16, 0x40, 0x0b, 0x7a})) + ((@string)(new byte[]{0x00, 0xb6, 0x00, 0x9f, 0x01, 0xa7, 0x01, 0xa9, 0x36, 0x20, 0xa0, 0x83, 0x14, 0x34, 0x63, 0x4a})) + ((@string)(new byte[]{0x21, 0x70, 0x8c, 0x07, 0x46, 0x03, 0x4e, 0x10, 0x62, 0x3c, 0x06, 0x4e, 0xc8, 0x8c, 0xb0, 0x32})) + ((@string)(new byte[]{0x2a, 0x59, 0xad, 0xb2, 0xf1, 0x02, 0x82, 0x7c, 0x33, 0xcb, 0x92, 0x6f, 0x32, 0x4f, 0x9b, 0xb0})) + ((@string)(new byte[]{0xa2, 0x30, 0xf0, 0xc0, 0x06, 0x1e, 0x98, 0x99, 0x2c, 0x06, 0x1e, 0xd8, 0xc0, 0x03, 0x56, 0xd8})) + ((@string)(new byte[]{0xc0, 0x03, 0x0f, 0x6c, 0xe0, 0x01, 0xf1, 0xf0, 0xee, 0x9a, 0xc6, 0xc8, 0x97, 0x99, 0xd1, 0x6c})) + ((@string)(new byte[]{0xb4, 0x21, 0x45, 0x3b, 0x10, 0xe4, 0x7b, 0x99, 0x4d, 0x8a, 0x36, 0x64, 0x5c, 0x77, 0x08, 0x02})) + ((@string)(new byte[]{0xcb, 0xe0, 0xce}))
    ),
    new(
        "fuzz1"u8,
        ((@string)(new byte[]{0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x31, 0x00, 0x00, 0x00, 0x00, 0x00, 0x30, 0x30, 0x30, 0x30})),
        ((@string)(new byte[]{0x28, 0xb5, 0x2f, 0xfd, 0x04, 0x58, 0x8d, 0x00, 0x00, 0x50, 0x30, 0x00, 0x30, 0x00, 0x31, 0x00, 0x30, 0x30, 0x30, 0x30, 0x03, 0x54, 0x02, 0x00, 0x01, 0x01, 0x6d, 0xf9, 0xb7, 0x47}))
    ),
    new(
        "empty block"u8,
        ""u8,
        ((@string)(new byte[]{0x28, 0xb5, 0x2f, 0xfd, 0x00, 0x00, 0x15, 0x00, 0x00, 0x00, 0x00}))
    ),
    new(
        "single skippable frame"u8,
        ""u8,
        "\x50\x2a\x4d\x18\x00\x00\x00\x00"u8
    ),
    new(
        "two skippable frames"u8,
        ""u8,
        "\x50\x2a\x4d\x18\x00\x00\x00\x00"u8 + "\x50\x2a\x4d\x18\x00\x00\x00\x00"u8
    )
}.slice();

public static void TestSamples(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in tests) {
        ref var testΔ1 = ref heap<testsᴛ1>(out var ᏑtestΔ1);
        testΔ1 = test;
        var testʗ1 = testΔ1;
        Ꮡt.Run(testΔ1.name, (ж<testing.T> tΔ1) => {
            var r = NewReader(new zstd_internal_test_package.strings_ReaderжReader(strings.NewReader(testʗ1.compressed)));
            var (got, err) = io.ReadAll(new zstd_internal_test_package.zstd_ReaderжReader(r));
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            @string gotstr = ((@string)got);
            if (gotstr != testʗ1.uncompressed) {
                tΔ1.Errorf("got %q want %q"u8, gotstr, testʗ1.uncompressed);
            }
        });
    }
}

public static void TestReset(ж<testing.T> Ꮡt) {
    var input = strings.NewReader(""u8);
    var r = NewReader(new zstd_internal_test_package.strings_ReaderжReader(input));
    foreach (var (_, test) in tests) {
        ref var testΔ1 = ref heap<testsᴛ1>(out var ᏑtestΔ1);
        testΔ1 = test;
        var inputʗ1 = input;
        var rʗ1 = r;
        var testʗ1 = testΔ1;
        Ꮡt.Run(testΔ1.name, (ж<testing.T> tΔ1) => {
            inputʗ1.Reset(testʗ1.compressed);
            rʗ1.Reset(new zstd_internal_test_package.strings_ReaderжReader(inputʗ1));
            var (got, err) = io.ReadAll(new zstd_internal_test_package.zstd_ReaderжReader(rʗ1));
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            @string gotstr = ((@string)got);
            if (gotstr != testʗ1.uncompressed) {
                tΔ1.Errorf("got %q want %q"u8, gotstr, testʗ1.uncompressed);
            }
        });
    }
}

internal static ж<sync.Once> ᏑbigDataOnce = new StandardBox<sync.Once>(default(sync.Once));
internal static ref sync.Once bigDataOnce => ref ᏑbigDataOnce.Value;
internal static slice<byte> bigDataBytes;
internal static error bigDataErr;

// bigData returns the contents of our large test file repeated multiple times.
internal static slice<byte> bigData(testing.TB t) {
    ᏑbigDataOnce.Do(() => {
        (bigDataBytes, bigDataErr) = os.ReadFile(testdataIsaacNewtonˢ);
        if (bigDataErr == default!) {
            bigDataBytes = bytes.Repeat(bigDataBytes, 20);
        }
    });
    if (bigDataErr != default!) {
        t.Fatal(bigDataErr);
    }
    return bigDataBytes;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string zstdˢ = "zstd"u8;
internal static readonly object skippingBecauseZstdNotˢ = (@string)"skipping because zstd not found"u8;

internal static @string findZstd(testing.TB t) {
    var (zstd, err) = exec.LookPath(zstdˢ);
    if (err != default!) {
        t.Skip(skippingBecauseZstdNotˢ);
    }
    return zstd;
}

internal static ж<sync.Once> ᏑzstdBigOnce = new StandardBox<sync.Once>(default(sync.Once));
internal static ref sync.Once zstdBigOnce => ref ᏑzstdBigOnce.Value;
internal static slice<byte> zstdBigBytes;
internal static error zstdBigErr;

// zstdBigData returns the compressed contents of our large test file.
// This will only run on Unix systems with zstd installed.
// That's OK as the package is GOOS-independent.
internal static slice<byte> zstdBigData(testing.TB t) {
    var input = bigData(t);
    @string zstd = findZstd(t);
    var inputʗ1 = input;
    ᏑzstdBigOnce.Do(() => {
        var cmd = exec.Command(zstd, "-z"u8);
        cmd.Value.Stdin = new zstd_internal_test_package.bytes_ReaderжReader(bytes.NewReader(inputʗ1));
        ref var compressed = ref heap(new bytes.Buffer(), out var Ꮡcompressed);
        cmd.Value.Stdout = new zstd_internal_test_package.bytes_BufferжWriter(Ꮡcompressed);
        cmd.Value.Stderr = new os.FileжWriter(os.Stderr);
        {
            var err = cmd.Run(); if (err != default!) {
                zstdBigErr = fmt.Errorf("running zstd failed: %v"u8, err);
                return;
            }
        }
        zstdBigBytes = compressed.Bytes();
    });
    if (zstdBigErr != default!) {
        t.Fatal(zstdBigErr);
    }
    return zstdBigBytes;
}

// Test decompressing a large file. We don't have a compressor,
// so this test only runs on systems with zstd installed.
public static void TestLarge(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingExpensiveTestInˢ);
    }
    var data = bigData(new zstd_internal_test_package.testing_TжTB(Ꮡt));
    var compressed = zstdBigData(new zstd_internal_test_package.testing_TжTB(Ꮡt));
    Ꮡt.Logf("zstd compressed %d bytes to %d"u8, builtin.len(data), builtin.len(compressed));
    var r = NewReader(new zstd_internal_test_package.bytes_ReaderжReader(bytes.NewReader(compressed)));
    var (got, err) = io.ReadAll(new zstd_internal_test_package.zstd_ReaderжReader(r));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!bytes.Equal(got, data)) {
        showDiffs(Ꮡt, got, data);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object dataMismatchˢ = (@string)"data mismatch"u8;

// showDiffs reports the first few differences in two []byte.
internal static void showDiffs(ж<testing.T> Ꮡt, slice<byte> got, slice<byte> want) {
    Ꮡt.Error(dataMismatchˢ);
    if (builtin.len(got) != builtin.len(want)) {
        Ꮡt.Errorf("got data length %d, want %d"u8, builtin.len(got), builtin.len(want));
    }
    nint diffs = 0;
    foreach (var (i, b) in got) {
        if (i >= builtin.len(want)) {
            break;
        }
        if (b != want[i]) {
            diffs++;
            if (diffs > 20) {
                break;
            }
            Ꮡt.Logf("%d: %#x != %#x"u8, i, b, want[i]);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingAllocationTestˢ = (@string)"skipping allocation test under race detector"u8;

public static void TestAlloc(ж<testing.T> Ꮡt) {
    testenv.SkipIfOptimizationOff(new zstd_internal_test_package.testing_TжTB(Ꮡt));
    if (race.Enabled) {
        Ꮡt.Skip(skippingAllocationTestˢ);
    }
    var compressed = zstdBigData(new zstd_internal_test_package.testing_TжTB(Ꮡt));
    var input = bytes.NewReader(compressed);
    var r = NewReader(new zstd_internal_test_package.bytes_ReaderжReader(input));
    var compressedʗ1 = compressed;
    var inputʗ1 = input;
    var rʗ1 = r;
    var c = testing.AllocsPerRun(10, () => {
        inputʗ1.Reset(compressedʗ1);
        rʗ1.Reset(new zstd_internal_test_package.bytes_ReaderжReader(inputʗ1));
        io.Copy(io.Discard, new zstd_internal_test_package.zstd_ReaderжReader(rʗ1));
    });
    if (c != 0D) {
        Ꮡt.Errorf("got %v allocs, want 0"u8, c);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataˢ = "testdata"u8;
internal static readonly @string zstˢ = ".zst"u8;

public static void TestFileSamples(ж<testing.T> Ꮡt) {
    var (samples, err) = os.ReadDir(testdataˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    foreach (var (_, sample) in samples) {
        @string name = sample.Name();
        if (!strings.HasSuffix(name, zstˢ)) {
            continue;
        }
        Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
            var (f, errΔ1) = os.Open(filepath.Join(testdataˢ, name));
            if (errΔ1 != default!) {
                tΔ1.Fatal(errΔ1);
            }
            var r = NewReader(new zstd_internal_test_package.os_FileжReader(f));
            var h = sha256.New();
            {
                var (_, errΔ2) = io.Copy(h, new zstd_internal_test_package.zstd_ReaderжReader(r)); if (errΔ2 != default!) {
                    tΔ1.Fatal(errΔ2);
                }
            }
            @string got = fmt.Sprintf("%x"u8, h.Sum(default!))[..8];
            var (want, _, _) = strings.Cut(name, "."u8);
            if (got != want) {
                tΔ1.Errorf("Wrong uncompressed content hash: got %s, want %s"u8, got, want);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorˢ = (@string)"expected error"u8;

public static void TestReaderBad(ж<testing.T> Ꮡt) {
    foreach (var (i, s) in badStrings) {
        Ꮡt.Run(fmt.Sprintf("badStrings#%d"u8, i), (ж<testing.T> tΔ1) => {
            var (_, err) = io.Copy(io.Discard, new zstd_internal_test_package.zstd_ReaderжReader(NewReader(new zstd_internal_test_package.strings_ReaderжReader(strings.NewReader(s)))));
            if (err == default!) {
                tΔ1.Error(expectedErrorˢ);
            }
        });
    }
}

public static void BenchmarkLarge(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    b.ReportAllocs();
    var compressed = zstdBigData(new zstd_internal_test_package.testing_BжTB(Ꮡb));
    b.SetBytes((int64)builtin.len(compressed));
    var input = bytes.NewReader(compressed);
    var r = NewReader(new zstd_internal_test_package.bytes_ReaderжReader(input));
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        input.Reset(compressed);
        r.Reset(new zstd_internal_test_package.bytes_ReaderжReader(input));
        io.Copy(io.Discard, new zstd_internal_test_package.zstd_ReaderжReader(r));
    }
}

} // end zstd_internal_test_package
