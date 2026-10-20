// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using static os_package;
using filepath = go.path.filepath_package;
using Δruntime = runtime_package;
using Δtesting = testing_package;
using fs = go.io.fs_package;
using go.io;
using go.path;
using static go.os_internal_test_package;
using Δos = os_package;

partial class os_test_package {

internal static void checkNamedSize(ж<Δtesting.T> Ꮡt, @string path, int64 size) {
    var (dir, err) = Stat(path);
    if (err != default!) {
        Ꮡt.Fatalf("Stat %q (looking for size %d): %s"u8, path, size, err);
    }
    if (dir.Size() != size) {
        Ꮡt.Errorf("Stat %q: size %d want %d"u8, path, dir.Size(), size);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rumpelstilzchenˢ = "rumpelstilzchen"u8;
internal static readonly @string readTestGoˢ = "read_test.go"u8;

public static void TestReadFile(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();

    @string filename = rumpelstilzchenˢ;
    var (contents, err) = ReadFile(filename);
    if (err == default!) {
        Ꮡt.Fatalf("ReadFile %s: error expected, none found"u8, filename);
    }

    filename = readTestGoˢ;
    (contents, err) = ReadFile(filename);
    if (err != default!) {
        Ꮡt.Fatalf("ReadFile %s: %v"u8, filename, err);
    }

    checkNamedSize(Ꮡt, filename, (int64)len(contents));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string osTestˢ = "os-test"u8;

public static void TestWriteFile(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();

        var (f, err) = CreateTemp(""u8, osTestˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        defer(Remove, f.Name(), ref ᒐ);

        @string msg = "Programming today is a race between software engineers striving to "u8 + "build bigger and better idiot-proof programs, and the Universe trying "u8 + "to produce bigger and better idiots. So far, the Universe is winning."u8;

        {
            var errΔ1 = WriteFile(f.Name(), slice<byte>(msg), 420); if (errΔ1 != default!) {
                Ꮡt.Fatalf("WriteFile %s: %v"u8, f.Name(), errΔ1);
            }
        }

        (var data, err) = ReadFile(f.Name());
        if (err != default!) {
            Ꮡt.Fatalf("ReadFile %s: %v"u8, f.Name(), err);
        }

        if (((sstring)data) != msg) {
            Ꮡt.Fatalf("ReadFile: wrong data:\nhave %q\nwant %q"u8, (@string)data, msg);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string blurpTxtˢ = "blurp.txt"u8;

public static void TestReadOnlyWriteFile(ж<Δtesting.T> Ꮡt) {
    if (Getuid() == 0) {
        Ꮡt.Skipf("Root can write to read-only files anyway, so skip the read-only test."u8);
    }
    if (Δruntime.GOOS == "wasip1"u8) {
        Ꮡt.Skip("no support for file permissions on " + Δruntime.GOOS);
    }
    Ꮡt.Parallel();

    // We don't want to use CreateTemp directly, since that opens a file for us as 0600.
    @string filename = filepath.Join(Ꮡt.TempDir(), blurpTxtˢ);

    var shmorp = slice<byte>("shmorp"u8);
    var florp = slice<byte>("florp"u8);
    var err = WriteFile(filename, shmorp, 292);
    if (err != default!) {
        Ꮡt.Fatalf("WriteFile %s: %v"u8, filename, err);
    }
    err = WriteFile(filename, florp, 292);
    if (err == default!) {
        Ꮡt.Fatalf("Expected an error when writing to read-only file %s"u8, filename);
    }
    (var got, err) = ReadFile(filename);
    if (err != default!) {
        Ꮡt.Fatalf("ReadFile %s: %v"u8, filename, err);
    }
    if (!bytes.Equal(got, shmorp)) {
        Ꮡt.Fatalf("want %s, got %s"u8, shmorp, got);
    }
}

public static void TestReadDir(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();

    @string dirname = rumpelstilzchenˢ;
    var (_, err) = ReadDir(dirname);
    if (err == default!) {
        Ꮡt.Fatalf("ReadDir %s: error expected, none found"u8, dirname);
    }

    dirname = "."u8;
    (var list, err) = ReadDir(dirname);
    if (err != default!) {
        Ꮡt.Fatalf("ReadDir %s: %v"u8, dirname, err);
    }

    var foundFile = false;
    var foundSubDir = false;
    foreach (var (_, dir) in list) {
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
        Ꮡt.Fatalf("ReadDir %s: read_test.go file not found"u8, dirname);
    }
    if (!foundSubDir) {
        Ꮡt.Fatalf("ReadDir %s: exec directory not found"u8, dirname);
    }
}

} // end os_test_package
