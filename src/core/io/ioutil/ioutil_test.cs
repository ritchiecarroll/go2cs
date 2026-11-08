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

internal static void checkSize(ж<testing.T> t, @string path, int64 size) {
    var (dir, err) = os.Stat(path);
    if (err != nil) {
        t.Fatalf("Stat %q (looking for size %d): %s", path, size, err);
    }
    if (dir.Size() != size) {
        t.Errorf("Stat %q: size %d want %d", path, dir.Size(), size);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string rumpelstilzchenˢ => ref rumpelstilzchenˢᵇ;
private static readonly @string rumpelstilzchenˢᵇ = "rumpelstilzchen"u8;
private static ref readonly @string ioutilTestGoˢ => ref ioutilTestGoˢᵇ;
private static readonly @string ioutilTestGoˢᵇ = "ioutil_test.go"u8;

public static void TestReadFile(ж<testing.T> t) {
    @string filename = rumpelstilzchenˢ;
    var (contents, err) = ReadFile(filename);
    if (err == nil) {
        t.Fatalf("ReadFile %s: error expected, none found", filename);
    }

    filename = ioutilTestGoˢ;
    (contents, err) = ReadFile(filename);
    if (err != nil) {
        t.Fatalf("ReadFile %s: %v", filename, err);
    }

    checkSize(t, filename, (int64)len(contents));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string ioutilTestˢ => ref ioutilTestˢᵇ;
private static readonly @string ioutilTestˢᵇ = "ioutil-test"u8;

public static void TestWriteFile(ж<testing.T> t) {
    var (f, err) = TempFile(""u8, ioutilTestˢ);
    if (err != nil) {
        t.Fatal(err);
    }
    @string filename = f.Name();
    @string data = "Programming today is a race between software engineers striving to "u8 + "build bigger and better idiot-proof programs, and the Universe trying "u8 + "to produce bigger and better idiots. So far, the Universe is winning."u8;

    var errΔ1 = WriteFile(filename, slice<byte>(data), 420); if (errΔ1 != nil) {
        t.Fatalf("WriteFile %s: %v", filename, errΔ1);
    }

    (var contents, err) = ReadFile(filename);
    if (err != nil) {
        t.Fatalf("ReadFile %s: %v", filename, err);
    }

    if (((sstring)contents) != data) {
        t.Fatalf("contents = %q\nexpected = %q", (@string)contents, data);
    }

    // cleanup
    f.Close();
    os.Remove(filename); // ignore error
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object filePermissionsAreNotˢ = (@string)"file permissions are not supported by wasip1"u8;
private static ref readonly @string blurpTxtˢ => ref blurpTxtˢᵇ;
private static readonly @string blurpTxtˢᵇ = "blurp.txt"u8;

public static void TestReadOnlyWriteFile(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (os.Getuid() == 0) {
            t.Skipf("Root can write to read-only files anyway, so skip the read-only test.");
        }
        if (runtime.GOOS == "wasip1"u8) {
            t.Skip(filePermissionsAreNotˢ);
        }

        // We don't want to use TempFile directly, since that opens a file for us as 0600.
        var (tempDir, err) = TempDir(""u8, t.Name());
        if (err != nil) {
            t.Fatalf("TempDir %s: %v", t.Name(), err);
        }
        defer(os.RemoveAll, tempDir, ref ᒐ);
        @string filename = filepath.Join(tempDir, blurpTxtˢ);

        var shmorp = slice<byte>("shmorp"u8);
        var florp = slice<byte>("florp"u8);
        err = WriteFile(filename, shmorp, 292);
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
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestReadDir(ж<testing.T> t) {
    @string dirname = rumpelstilzchenˢ;
    var (_, err) = ReadDir(dirname);
    if (err == nil) {
        t.Fatalf("ReadDir %s: error expected, none found", dirname);
    }

    dirname = "..";
    (var list, err) = ReadDir(dirname);
    if (err != nil) {
        t.Fatalf("ReadDir %s: %v", dirname, err);
    }

    var foundFile = false;
    var foundSubDir = false;
    foreach (var (_, dir) in range(list)) {
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
        t.Fatalf("ReadDir %s: io_test.go file not found", dirname);
    }
    if (!foundSubDir) {
        t.Fatalf("ReadDir %s: ioutil directory not found", dirname);
    }
}

} // end ioutil_test_package
