// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.io;

using bytes = bytes_package;
using static go.io.ioutil_package;
using os = os_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
using testing = testing_package;
using fs = go.io.fs_package;
using go.io;
using path;

partial class ioutil_test_package {

internal static void checkSize(ж<testing.T> Ꮡt, @string path, int64 size) {
    var (dir, err) = os.Stat(path);
    if (err != default!) {
        Ꮡt.Fatalf("Stat %q (looking for size %d): %s"u8, path, size, err);
    }
    if (dir.Size() != size) {
        Ꮡt.Errorf("Stat %q: size %d want %d"u8, path, dir.Size(), size);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string rumpelstilzchenˢ = "rumpelstilzchen"u8;
private static readonly @string ioutilTestGoˢ = "ioutil_test.go"u8;

public static void TestReadFile(ж<testing.T> Ꮡt) {
    @string filename = rumpelstilzchenˢ;
    var (contents, err) = ReadFile(filename);
    if (err == default!) {
        Ꮡt.Fatalf("ReadFile %s: error expected, none found"u8, filename);
    }

    filename = ioutilTestGoˢ;
    (contents, err) = ReadFile(filename);
    if (err != default!) {
        Ꮡt.Fatalf("ReadFile %s: %v"u8, filename, err);
    }

    checkSize(Ꮡt, filename, (int64)len(contents));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string ioutilTestˢ = "ioutil-test"u8;

public static void TestWriteFile(ж<testing.T> Ꮡt) {
    var (f, err) = TempFile(""u8, ioutilTestˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    @string filename = f.Name();
    @string data = "Programming today is a race between software engineers striving to "u8 + "build bigger and better idiot-proof programs, and the Universe trying "u8 + "to produce bigger and better idiots. So far, the Universe is winning."u8;

    {
        var errΔ1 = WriteFile(filename, slice<byte>(data), 420); if (errΔ1 != default!) {
            Ꮡt.Fatalf("WriteFile %s: %v"u8, filename, errΔ1);
        }
    }

    (var contents, err) = ReadFile(filename);
    if (err != default!) {
        Ꮡt.Fatalf("ReadFile %s: %v"u8, filename, err);
    }

    if (((sstring)contents) != data) {
        Ꮡt.Fatalf("contents = %q\nexpected = %q"u8, (@string)contents, data);
    }

    // cleanup
    f.Close();
    os.Remove(filename); // ignore error
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object filePermissionsAreNotˢ = (@string)"file permissions are not supported by wasip1"u8;
private static readonly @string blurpTxtˢ = "blurp.txt"u8;

public static void TestReadOnlyWriteFile(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (os.Getuid() == 0) {
            Ꮡt.Skipf("Root can write to read-only files anyway, so skip the read-only test."u8);
        }
        if (runtime.GOOS == "wasip1"u8) {
            Ꮡt.Skip(filePermissionsAreNotˢ);
        }

        // We don't want to use TempFile directly, since that opens a file for us as 0600.
        var (tempDir, err) = TempDir(""u8, Ꮡt.Name());
        if (err != default!) {
            Ꮡt.Fatalf("TempDir %s: %v"u8, Ꮡt.Name(), err);
        }
        defer(os.RemoveAll, tempDir, ref ᒐ);
        @string filename = filepath.Join(tempDir, blurpTxtˢ);

        var shmorp = slice<byte>("shmorp"u8);
        var florp = slice<byte>("florp"u8);
        err = WriteFile(filename, shmorp, 292);
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
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestReadDir(ж<testing.T> Ꮡt) {
    @string dirname = rumpelstilzchenˢ;
    var (_, err) = ReadDir(dirname);
    if (err == default!) {
        Ꮡt.Fatalf("ReadDir %s: error expected, none found"u8, dirname);
    }

    dirname = ".."u8;
    (var list, err) = ReadDir(dirname);
    if (err != default!) {
        Ꮡt.Fatalf("ReadDir %s: %v"u8, dirname, err);
    }

    var foundFile = false;
    var foundSubDir = false;
    foreach (var (_, dir) in list) {
        switch (ᐧ) {
        case {} when !dir.IsDir() && dir.Name() == "io_test.go"u8: {
            foundFile = true;
            break;
        }
        case {} when dir.IsDir() && dir.Name() == "ioutil"u8: {
            foundSubDir = true;
            break;
        }}
    }
    if (!foundFile) {
        Ꮡt.Fatalf("ReadDir %s: io_test.go file not found"u8, dirname);
    }
    if (!foundSubDir) {
        Ꮡt.Fatalf("ReadDir %s: ioutil directory not found"u8, dirname);
    }
}

} // end ioutil_test_package
