// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.compress;

using bufio = bufio_package;
using bytes = bytes_package;
using io = io_package;
using strings = strings_package;
using testing = testing_package;
using static go.compress.flate_package;

partial class flate_internal_test_package {

public static void TestReset(ж<testing.T> t) {
    var ss = slice<@string>([
        "lorem ipsum izzle fo rizzle",
        "the quick brown fox jumped over"
    ]);

    var deflated = new slice<bytes.Buffer>(2);
    foreach (var (i, s) in range(ss)) {
        var (w, _) = NewWriter(new flate_test_package.bytes_BufferжWriter(Ꮡ(deflated, i)), 1);
        w.Write(slice<byte>(s));
        w.Close();
    }

    var inflated = new slice<bytes.Buffer>(2);

    var f = NewReader(new flate_test_package.bytes_BufferжReader(Ꮡ(deflated, 0)));
    io.Copy(new flate_test_package.bytes_BufferжWriter(Ꮡ(inflated, 0)), f);
    f._<Resetter>().Reset(new flate_test_package.bytes_BufferжReader(Ꮡ(deflated, 1)), default);
    io.Copy(new flate_test_package.bytes_BufferжWriter(Ꮡ(inflated, 1)), f);
    f.Close();

    foreach (var (i, s) in range(ss)) {
        if (s != Ꮡ(inflated, i).String()) {
            t.Errorf("inflated[%d]:\ngot  %q\nwant %q", i, Ꮡ(inflated, i), s);
        }
    }
}

internal partial struct TestReaderTruncated_vectors /*dyn*/ {
    internal @string input, output;
}

public static void TestReaderTruncated(ж<testing.T> t) {
    var vectors = slice<TestReaderTruncated_vectors>([
        new("\x00", ""),
        new("\x00\f", ""),
        new("\x00\f\x00", ""),
        new(((@string)(new byte[]{0x00, 0x0c, 0x00, 0xf3, 0xff})), ""),
        new(((@string)(new byte[]{0x00, 0x0c, 0x00, 0xf3, 0xff, 0x68, 0x65, 0x6c, 0x6c, 0x6f})), "hello"),
        new(((@string)(new byte[]{0x00, 0x0c, 0x00, 0xf3, 0xff, 0x68, 0x65, 0x6c, 0x6c, 0x6f, 0x2c, 0x20, 0x77, 0x6f, 0x72, 0x6c, 0x64})), "hello, world"),
        new("\x02", ""),
        new(((@string)(new byte[]{0xf2, 0x48, 0xcd})), "He"),
        new(((@string)(new byte[]{0xf2, 0x48, 0xcd, 0x99, 0x30, 0x61, 0xc2, 0x84, 0x09})), ((@string)(new byte[]{0x48, 0x65, 0x6c, 0x90, 0x90, 0x90, 0x90, 0x90}))),
        new(((@string)(new byte[]{0xf2, 0x48, 0xcd, 0x99, 0x30, 0x61, 0xc2, 0x84, 0x09, 0x00})), ((@string)(new byte[]{0x48, 0x65, 0x6c, 0x90, 0x90, 0x90, 0x90, 0x90})))
    ]);

    foreach (var (i, v) in range(vectors)) {
        var r = strings.NewReader(v.input);
        var zr = NewReader(new flate_test_package.strings_ReaderжReader(r));
        var (b, err) = io.ReadAll(zr);
        if (!AreEqual(err, io.ErrUnexpectedEOF)) {
            t.Errorf("test %d, error mismatch: got %v, want io.ErrUnexpectedEOF", i, err);
        }
        if (((sstring)b) != v.output) {
            t.Errorf("test %d, output mismatch: got %q, want %q", i, b, v.output);
        }
    }
}

public static void TestResetDict(ж<testing.T> t) {
    var dict = slice<byte>("the lorem fox"u8);
    var ss = slice<@string>([
        "lorem ipsum izzle fo rizzle",
        "the quick brown fox jumped over"
    ]);

    var deflated = new slice<bytes.Buffer>(len(ss));
    foreach (var (i, s) in range(ss)) {
        var (w, _) = NewWriterDict(new flate_test_package.bytes_BufferжWriter(Ꮡ(deflated, i)), DefaultCompression, dict);
        w.Write(slice<byte>(s));
        w.Close();
    }

    var inflated = new slice<bytes.Buffer>(len(ss));

    var f = NewReader(default);
    foreach (var i in range(len(inflated))) {
        f._<Resetter>().Reset(new flate_test_package.bytes_BufferжReader(Ꮡ(deflated, i)), dict);
        io.Copy(new flate_test_package.bytes_BufferжWriter(Ꮡ(inflated, i)), f);
    }
    f.Close();

    foreach (var (i, s) in range(ss)) {
        if (s != Ꮡ(inflated, i).String()) {
            t.Errorf("inflated[%d]:\ngot  %q\nwant %q", i, Ꮡ(inflated, i), s);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string bufferIsReusedˢ => ref bufferIsReusedˢᵇ;
private static readonly @string bufferIsReusedˢᵇ = "BufferIsReused"u8;

internal partial struct TestReaderReusesReaderBuffer_encodedNotByteReader /*dyn*/ {
    /*embed*/ public io_package.Reader Reader;
}

public static void TestReaderReusesReaderBuffer(ж<testing.T> t) {
    var encodedReader = bytes.NewReader([]);
    ref var encodedNotByteReader = ref heap<TestReaderReusesReaderBuffer_encodedNotByteReader>(new TestReaderReusesReaderBuffer_encodedNotByteReader(new flate_test_package.bytes_Readerжio_Reader(encodedReader)), out var ᏑencodedNotByteReader);

    var encodedNotByteReaderʗ1 = encodedNotByteReader;
    t.Run(bufferIsReusedˢ, (ж<testing.T> tΔ1) => {
        var f = NewReader(encodedNotByteReaderʗ1)._<ж<global::go.compress.flate_package.decompressor>>();
        var (bufioR, ok) = f.r._<ж<bufio.Reader>>(ᐧ);
        if (!ok) {
            tΔ1.Fatalf("bufio.Reader should be created");
        }
        f.Reset(encodedNotByteReaderʗ1, default);
        if (!AreEqual(bufioR, f.r)) {
            tΔ1.Fatalf("bufio.Reader was not reused");
        }
    });
    var encodedNotByteReaderʗ2 = encodedNotByteReader;
    var encodedReaderʗ1 = encodedReader;
    t.Run("BufferIsNotReusedWhenGotByteReader", (ж<testing.T> tΔ2) => {
        var f = NewReader(encodedNotByteReaderʗ2)._<ж<global::go.compress.flate_package.decompressor>>();
        var (_, ok) = f.r._<ж<bufio.Reader>>(ᐧ); if (!ok) {
            tΔ2.Fatalf("bufio.Reader should be created");
        }
        f.Reset(new flate_test_package.bytes_Readerжio_Reader(encodedReaderʗ1), default);
        if (!AreEqual(f.r, encodedReaderʗ1)) {
            tΔ2.Fatalf("provided io.ByteReader should be used directly");
        }
    });
    var encodedNotByteReaderʗ3 = encodedNotByteReader;
    var encodedReaderʗ2 = encodedReader;
    t.Run("BufferIsCreatedAfterByteReader", (ж<testing.T> tΔ3) => {
        foreach (var (i, r) in range(slice<io.Reader>([new flate_test_package.bytes_Readerжio_Reader(encodedReaderʗ2), new flate_test_package.bufio_ReaderжReader(bufio.NewReader(new flate_test_package.bytes_Readerжio_Reader(encodedReaderʗ2)))]))) {
            var f = NewReader(r)._<ж<global::go.compress.flate_package.decompressor>>();
            if (!AreEqual(f.r, r)) {
                tΔ3.Fatalf("provided io.ByteReader should be used directly, i=%d", i);
            }
            f.Reset(encodedNotByteReaderʗ3, default);
            var (_, ok) = f.r._<ж<bufio.Reader>>(ᐧ); if (!ok) {
                tΔ3.Fatalf("bufio.Reader should be created, i=%d", i);
            }
        }
    });
}

} // end flate_internal_test_package
