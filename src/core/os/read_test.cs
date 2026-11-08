// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using errors = errors_package;
using static os_package;
using filepath = go.path.filepath_package;
using Δruntime = runtime_package;
using syscall = syscall_package;
using Δtesting = testing_package;
using fs = go.io.fs_package;
using go.io;
using go.path;
using static go.os_internal_test_package;
using Δos = os_package;

partial class os_test_package {

internal static void checkNamedSize(ж<Δtesting.T> t, @string path, int64 size) {
    var (dir, err) = Stat(path);
    if (err != nil) {
        t.Fatalf("Stat %q (looking for size %d): %s", path, size, err);
    }
    if (dir.Size() != size) {
        t.Errorf("Stat %q: size %d want %d", path, dir.Size(), size);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string rumpelstilzchenˢ => ref rumpelstilzchenˢᵇ;
private static readonly @string rumpelstilzchenˢᵇ = "rumpelstilzchen"u8;
internal static ref readonly @string readTestGoˢ => ref readTestGoˢᵇ;
private static readonly @string readTestGoˢᵇ = "read_test.go"u8;

public static void TestReadFile(ж<Δtesting.T> t) {
    t.Parallel();

    @string filename = rumpelstilzchenˢ;
    var (contents, err) = ReadFile(filename);
    if (err == nil) {
        t.Fatalf("ReadFile %s: error expected, none found", filename);
    }

    filename = readTestGoˢ;
    (contents, err) = ReadFile(filename);
    if (err != nil) {
        t.Fatalf("ReadFile %s: %v", filename, err);
    }

    checkNamedSize(t, filename, (int64)len(contents));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string osTestˢ => ref osTestˢᵇ;
private static readonly @string osTestˢᵇ = "os-test"u8;

public static void TestWriteFile(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        t.Parallel();

        var (f, err) = CreateTemp(""u8, osTestˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        defer(Remove, f.Name(), ref ᒐ);

        @string msg = "Programming today is a race between software engineers striving to "u8 + "build bigger and better idiot-proof programs, and the Universe trying "u8 + "to produce bigger and better idiots. So far, the Universe is winning."u8;

        var errΔ1 = WriteFile(f.Name(), slice<byte>(msg), 420); if (errΔ1 != nil) {
            t.Fatalf("WriteFile %s: %v", f.Name(), errΔ1);
        }

        (var data, err) = ReadFile(f.Name());
        if (err != nil) {
            t.Fatalf("ReadFile %s: %v", f.Name(), err);
        }

        if (((sstring)data) != msg) {
            t.Fatalf("ReadFile: wrong data:\nhave %q\nwant %q", (@string)data, msg);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string blurpTxtˢ => ref blurpTxtˢᵇ;
private static readonly @string blurpTxtˢᵇ = "blurp.txt"u8;

public static void TestReadOnlyWriteFile(ж<Δtesting.T> t) {
    if (Getuid() == 0) {
        t.Skipf("Root can write to read-only files anyway, so skip the read-only test.");
    }
    if (Δruntime.GOOS == "wasip1"u8) {
        t.Skip("no support for file permissions on " + Δruntime.GOOS);
    }
    t.Parallel();

    // We don't want to use CreateTemp directly, since that opens a file for us as 0600.
    @string filename = filepath.Join(t.TempDir(), blurpTxtˢ);

    var shmorp = slice<byte>("shmorp"u8);
    var florp = slice<byte>("florp"u8);
    var err = WriteFile(filename, shmorp, 292);
    if (err != nil) {
        t.Fatalf("WriteFile %s: %v", filename, err);
    }
    err = WriteFile(filename, florp, 292);
    if (err == nil) {
        t.Fatalf("Expected an error when writing to read-only file %s", filename);
    }
    (var got, err) = ReadFile(filename);
    if (err != nil) {
        t.Fatalf("ReadFile %s: %v", filename, err);
    }
    if (!bytes.Equal(got, shmorp)) {
        t.Fatalf("want %s, got %s", shmorp, got);
    }
}

public static void TestReadDir(ж<Δtesting.T> t) {
    t.Parallel();

    @string dirname = rumpelstilzchenˢ;
    var (_, errΔ1) = ReadDir(dirname); if (errΔ1 == nil) {
        t.Fatalf("ReadDir %s: error expected, none found", dirname);
    }

    @string filename = filepath.Join(t.TempDir(), fooˢ);
    var (f, err) = Create(filename);
    if (err != nil) {
        t.Fatal(err);
    }
    f.Close();
    var (listΔ1, errΔ2) = ReadDir(filename); if (listΔ1 != nil || !errors.Is(errΔ2, syscall.ENOTDIR)) {
        t.Fatalf("ReadDir %s: (nil, ENOTDIR) expected, got (%v, %v)", filename, listΔ1, errΔ2);
    }

    dirname = ".";
    (var list, err) = ReadDir(dirname);
    if (err != nil) {
        t.Fatalf("ReadDir %s: %v", dirname, err);
    }

    var foundFile = false;
    var foundSubDir = false;
    foreach (var (_, dir) in range(list)) {
        switch (ᐧ) {
        case {} when !dir.IsDir() && dir.Name() == "read_test.go"u8: {
            foundFile = true;
            break;
        }
        case {} when dir.IsDir() && dir.Name() == "exec"u8: {
            foundSubDir = true;
            break;
        }}
    }
    if (!foundFile) {
        t.Fatalf("ReadDir %s: read_test.go file not found", dirname);
    }
    if (!foundSubDir) {
        t.Fatalf("ReadDir %s: exec directory not found", dirname);
    }
}

} // end os_test_package
