// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.compress;

using testenv = @internal.testenv_package;
using fs = go.io.fs_package;
using os = os_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using go.io;
using io = io_package;
using path;
using static go.compress.gzip_package;
using time = time_package;

partial class gzip_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestOnNonBuilderˢ = (@string)"skipping test on non-builder"u8;
internal static readonly object errorEvaluatingGorootˢ = (@string)"error evaluating GOROOT: "u8;
internal static readonly object errorCollectingListOfGzˢ = (@string)"error collecting list of .gz files in GOROOT: "u8;
internal static readonly object expectedToFindSomeGzˢ = (@string)"expected to find some .gz files under GOROOT"u8;

// TestGZIPFilesHaveZeroMTimes checks that every .gz file in the tree
// has a zero MTIME. This is a requirement for the Debian maintainers
// to be able to have deterministic packages.
//
// To patch a .gz file, use the following command:
//
//	$ dd if=/dev/zero bs=1 seek=4 count=4 conv=notrunc of=filename.gz
//
// See https://golang.org/issue/14937.
public static void TestGZIPFilesHaveZeroMTimes(ж<testing.T> Ꮡt) {
    // To avoid spurious false positives due to untracked GZIP files that
    // may be in the user's GOROOT (Issue 18604), we only run this test on
    // the builders, which should have a clean checkout of the tree.
    if (testenv.Builder() == ""u8) {
        Ꮡt.Skip(skippingTestOnNonBuilderˢ);
    }
    testenv.MustHaveSource(new gzip_test_package.testing_TжTB(Ꮡt));

    var (goroot, err) = filepath.EvalSymlinks(runtime.GOROOT());
    if (err != default!) {
        Ꮡt.Fatal(errorEvaluatingGorootˢ, err);
    }
    ref var files = ref heap<slice<@string>>(out var Ꮡfiles);
    err = filepath.WalkDir(goroot, error (@string path, fs.DirEntry info, error errΔ1) => {
        if (errΔ1 != default!) {
            return errΔ1;
        }
        if (!info.IsDir() && strings.HasSuffix(path, ".gz"u8)) {
            Ꮡfiles.ValueSlot = append(Ꮡfiles.ValueSlot, path);
        }
        return default!;
    });
    if (err != default!) {
        if (os.IsNotExist(err)) {
            Ꮡt.Skipf("skipping: GOROOT directory not found: %s"u8, runtime.GOROOT());
        }
        Ꮡt.Fatal(errorCollectingListOfGzˢ, err);
    }
    if (len(files) == 0) {
        Ꮡt.Fatal(expectedToFindSomeGzˢ);
    }
    foreach (var (_, path) in files) {
        checkZeroMTime(Ꮡt, path);
    }
}

internal static void checkZeroMTime(ж<testing.T> Ꮡt, @string path) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.Open(path);
        if (err != default!) {
            Ꮡt.Error(err);
            return;
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (var gz, err) = NewReader(new gzip_test_package.os_FileжReader(f));
        if (err != default!) {
            Ꮡt.Errorf("cannot read gzip file %s: %s"u8, path, err);
            return;
        }
        var gzʗ1 = gz;
        defer(() => gzʗ1.Close(), ref ᒐ);
        if (!(~gz).ModTime.IsZero()) {
            Ꮡt.Errorf("gzip file %s has non-zero mtime (%s)"u8, path, (~gz).ModTime);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end gzip_internal_test_package
