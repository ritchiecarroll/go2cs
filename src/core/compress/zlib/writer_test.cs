// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.compress;

using bytes = bytes_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using io = io_package;
using os = os_package;
using testing = testing_package;
using @internal;
using static go.compress.zlib_package;

partial class zlib_internal_test_package {

internal static slice<@string> filenames = [
    "../testdata/gettysburg.txt",
    "../testdata/e.txt",
    "../testdata/pi.txt"
];

internal static slice<@string> data = [
    "test a reasonable sized string that can be compressed"
];

// Tests that compressing and then decompressing the given file at the given compression level and dictionary
// yields equivalent bytes to the original file.
internal static void testFileLevelDict(ж<testing.T> t, @string fn, nint level, @string d) {
    GoFrame ᒐ = default;
    try {
        // Read the file, as golden output.
        var (golden, err) = os.Open(fn);
        if (err != nil) {
            t.Errorf("%s (level=%d, dict=%q): %v", fn, level, d, err);
            return;
        }
        var goldenʗ1 = golden;
        defer(() => goldenʗ1.Close(), ref ᒐ);
        var (b0, err0) = io.ReadAll(new zlib_test_package.os_FileжReader(golden));
        if (err0 != nil) {
            t.Errorf("%s (level=%d, dict=%q): %v", fn, level, d, err0);
            return;
        }
        testLevelDict(t, fn, b0, level, d);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void testLevelDict(ж<testing.T> t, @string fn, slice<byte> b0, nint level, @string d) {
    GoFrame ᒐ = default;
    try {
        // Make dictionary, if given.
        slice<byte> dict = default;
        if (d != ""u8) {
            dict = slice<byte>(d);
        }

        // Push data through a pipe that compresses at the write end, and decompresses at the read end.
        var (piper, pipew) = io.Pipe();
        var piperʗ1 = piper;
        defer(() => piperʗ1.Close(), ref ᒐ);
        var b0ʗ1 = b0;
        var dictʗ1 = dict;
        var pipewʗ1 = pipew;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                var pipewʗ2 = pipewʗ1;
                defer(() => pipewʗ2.Close(), ref ᒐ);
                var (zlibw, errΔ1) = NewWriterLevelDict(new io.PipeWriterжWriter(pipewʗ1), level, dictʗ1);
                if (errΔ1 != nil) {
                    t.Errorf("%s (level=%d, dict=%q): %v", fn, level, d, errΔ1);
                    return;
                }
                var zlibwʗ1 = zlibw;
                defer(() => zlibwʗ1.Close(), ref ᒐ);
                (_, errΔ1) = zlibw.Write(b0ʗ1);
                if (errΔ1 != nil) {
                    t.Errorf("%s (level=%d, dict=%q): %v", fn, level, d, errΔ1);
                    return;
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        var (zlibr, err) = NewReaderDict(new io.PipeReaderжReader(piper), dict);
        if (err != nil) {
            t.Errorf("%s (level=%d, dict=%q): %v", fn, level, d, err);
            return;
        }
        var zlibrʗ1 = zlibr;
        defer(() => zlibrʗ1.Close(), ref ᒐ);

        // Compare the decompressed data.
        var (b1, err1) = io.ReadAll(zlibr);
        if (err1 != nil) {
            t.Errorf("%s (level=%d, dict=%q): %v", fn, level, d, err1);
            return;
        }
        if (len(b0) != len(b1)) {
            t.Errorf("%s (level=%d, dict=%q): length mismatch %d versus %d", fn, level, d, len(b0), len(b1));
            return;
        }
        for (nint i = 0; i < len(b0); i++) {
            if (b0[i] != b1[i]) {
                t.Errorf("%s (level=%d, dict=%q): mismatch at %d, 0x%02x versus 0x%02x\n", fn, level, d, i, b0[i], b1[i]);
                return;
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testFileLevelDictReset(ж<testing.T> t, @string fn, nint level, slice<byte> dict) {
    slice<byte> b0 = default;
    error err = default;
    if (fn != ""u8) {
        (b0, err) = os.ReadFile(fn);
        if (err != nil) {
            t.Errorf("%s (level=%d): %v", fn, level, err);
            return;
        }
    }

    // Compress once.
    var buf = @new<bytes.Buffer>();
    ж<global::go.compress.zlib_package.Writer> zlibw = default;
    if (dict == nil) {
        (zlibw, err) = NewWriterLevel(new zlib_test_package.bytes_BufferжWriter(buf), level);
    } else {
        (zlibw, err) = NewWriterLevelDict(new zlib_test_package.bytes_BufferжWriter(buf), level, dict);
    }
    if (err == nil) {
        (_, err) = zlibw.Write(b0);
    }
    if (err == nil) {
        err = zlibw.Close();
    }
    if (err != nil) {
        t.Errorf("%s (level=%d): %v", fn, level, err);
        return;
    }
    @string @out = buf.String();

    // Reset and compress again.
    var buf2 = @new<bytes.Buffer>();
    zlibw.Reset(new zlib_test_package.bytes_BufferжWriter(buf2));
    (_, err) = zlibw.Write(b0);
    if (err == nil) {
        err = zlibw.Close();
    }
    if (err != nil) {
        t.Errorf("%s (level=%d): %v", fn, level, err);
        return;
    }
    @string out2 = buf2.String();

    if (out2 != @out) {
        t.Errorf("%s (level=%d): different output after reset (got %d bytes, expected %d",
            fn, level, len(out2), len(@out));
    }
}

public static void TestWriter(ж<testing.T> t) {
    foreach (var (i, s) in range(data)) {
        var b = slice<byte>(s);
        @string tag = fmt.Sprintf("#%d"u8, i);
        testLevelDict(t, tag, b, DefaultCompression, ""u8);
        testLevelDict(t, tag, b, NoCompression, ""u8);
        testLevelDict(t, tag, b, HuffmanOnly, ""u8);
        for (nint level = BestSpeed; level <= BestCompression; level++) {
            testLevelDict(t, tag, b, level, ""u8);
        }
    }
}

public static void TestWriterBig(ж<testing.T> t) {
    foreach (var (i, fn) in range(filenames)) {
        testFileLevelDict(t, fn, DefaultCompression, ""u8);
        testFileLevelDict(t, fn, NoCompression, ""u8);
        testFileLevelDict(t, fn, HuffmanOnly, ""u8);
        for (nint level = BestSpeed; level <= BestCompression; level++) {
            testFileLevelDict(t, fn, level, ""u8);
            if (level >= 1 && testing.Short() && testenv.Builder() == ""u8) {
                break;
            }
        }
        if (i == 0 && testing.Short() && testenv.Builder() == ""u8) {
            break;
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string dictionaryᶜ => ref dictionaryᶜᵇ;
private static readonly @string dictionaryᶜᵇ = "0123456789."u8;

public static void TestWriterDict(ж<testing.T> t) {
    @string dictionary = dictionaryᶜ;
    foreach (var (i, fn) in range(filenames)) {
        testFileLevelDict(t, fn, DefaultCompression, dictionary);
        testFileLevelDict(t, fn, NoCompression, dictionary);
        testFileLevelDict(t, fn, HuffmanOnly, dictionary);
        for (nint level = BestSpeed; level <= BestCompression; level++) {
            testFileLevelDict(t, fn, level, dictionary);
            if (level >= 1 && testing.Short() && testenv.Builder() == ""u8) {
                break;
            }
        }
        if (i == 0 && testing.Short() && testenv.Builder() == ""u8) {
            break;
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string dictionaryᶜ1 => ref dictionaryᶜ1ᵇ;
private static readonly @string dictionaryᶜ1ᵇ = "0123456789."u8;

public static void TestWriterReset(ж<testing.T> t) {
    @string dictionary = dictionaryᶜ1;
    foreach (var (_, fn) in range(filenames)) {
        testFileLevelDictReset(t, fn, NoCompression, default);
        testFileLevelDictReset(t, fn, DefaultCompression, default);
        testFileLevelDictReset(t, fn, HuffmanOnly, default);
        testFileLevelDictReset(t, fn, NoCompression, slice<byte>(dictionary));
        testFileLevelDictReset(t, fn, DefaultCompression, slice<byte>(dictionary));
        testFileLevelDictReset(t, fn, HuffmanOnly, slice<byte>(dictionary));
        if (testing.Short()) {
            break;
        }
        for (nint level = BestSpeed; level <= BestCompression; level++) {
            testFileLevelDictReset(t, fn, level, default);
        }
    }
}

public static void TestWriterDictIsUsed(ж<testing.T> t) {
    slice<byte> input = slice<byte>("Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."u8);
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var (compressor, err) = NewWriterLevelDict(new zlib_test_package.bytes_BufferжWriter(Ꮡbuf), BestCompression, input);
    if (err != nil) {
        t.Errorf("error in NewWriterLevelDict: %s", err);
        return;
    }
    compressor.Write(input);
    compressor.Close();
    const nint expectedMaxSize = 25;
    var output = buf.Bytes();
    if (len(output) > expectedMaxSize) {
        t.Errorf("result too large (got %d, want <= %d bytes). Is the dictionary being used?", len(output), (nint)(expectedMaxSize));
    }
}

} // end zlib_internal_test_package
