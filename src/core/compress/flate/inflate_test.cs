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

public static void TestReset(ж<testing.T> Ꮡt) {
    var ss = new @string[]{
        "lorem ipsum izzle fo rizzle"u8,
        "the quick brown fox jumped over"u8
    }.slice();
    var deflated = new slice<bytes.Buffer>(2);
    foreach (var (i, s) in ss) {
        var (w, _) = NewWriter(new flate_test_package.bytes_BufferжWriter(Ꮡ(deflated, i)), 1);
        w.Write(slice<byte>(s));
        w.Close();
    }
    var inflated = new slice<bytes.Buffer>(2);
    var f = NewReader(new flate_test_package.bytes_BufferжReader(Ꮡ(deflated, 0)));
    io.Copy(new flate_test_package.bytes_BufferжWriter(Ꮡ(inflated, 0)), f);
    f._<Resetter>().Reset(new flate_test_package.bytes_BufferжReader(Ꮡ(deflated, 1)), default!);
    io.Copy(new flate_test_package.bytes_BufferжWriter(Ꮡ(inflated, 1)), f);
    f.Close();
    foreach (var (i, s) in ss) {
        if (s != Ꮡ(inflated, i).String()) {
            Ꮡt.Errorf("inflated[%d]:\ngot  %q\nwant %q"u8, i, inflated[i], s);
        }
    }
}

[GoType("dyn")] internal partial struct TestReaderTruncated_vectors {
    internal @string input, output;
}

public static void TestReaderTruncated(ж<testing.T> Ꮡt) {
    var vectors = new TestReaderTruncated_vectors[]{
        new("\x00"u8, ""u8),
        new("\x00\f"u8, ""u8),
        new("\x00\f\x00"u8, ""u8),
        new(((@string)(new byte[]{0x00, 0x0c, 0x00, 0xf3, 0xff})), ""u8),
        new(((@string)(new byte[]{0x00, 0x0c, 0x00, 0xf3, 0xff, 0x68, 0x65, 0x6c, 0x6c, 0x6f})), "hello"u8),
        new(((@string)(new byte[]{0x00, 0x0c, 0x00, 0xf3, 0xff, 0x68, 0x65, 0x6c, 0x6c, 0x6f, 0x2c, 0x20, 0x77, 0x6f, 0x72, 0x6c, 0x64})), "hello, world"u8),
        new("\x02"u8, ""u8),
        new(((@string)(new byte[]{0xf2, 0x48, 0xcd})), "He"u8),
        new(((@string)(new byte[]{0xf2, 0x48, 0xcd, 0x99, 0x30, 0x61, 0xc2, 0x84, 0x09})), ((@string)(new byte[]{0x48, 0x65, 0x6c, 0x90, 0x90, 0x90, 0x90, 0x90}))),
        new(((@string)(new byte[]{0xf2, 0x48, 0xcd, 0x99, 0x30, 0x61, 0xc2, 0x84, 0x09, 0x00})), ((@string)(new byte[]{0x48, 0x65, 0x6c, 0x90, 0x90, 0x90, 0x90, 0x90})))
    }.slice();
    foreach (var (i, v) in vectors) {
        var r = strings.NewReader(v.input);
        var zr = NewReader(new flate_test_package.strings_ReaderжReader(r));
        var (b, err) = io.ReadAll(zr);
        if (!AreEqual(err, io.ErrUnexpectedEOF)) {
            Ꮡt.Errorf("test %d, error mismatch: got %v, want io.ErrUnexpectedEOF"u8, i, err);
        }
        if (((sstring)b) != v.output) {
            Ꮡt.Errorf("test %d, output mismatch: got %q, want %q"u8, i, b, v.output);
        }
    }
}

public static void TestResetDict(ж<testing.T> Ꮡt) {
    var dict = slice<byte>("the lorem fox"u8);
    var ss = new @string[]{
        "lorem ipsum izzle fo rizzle"u8,
        "the quick brown fox jumped over"u8
    }.slice();
    var deflated = new slice<bytes.Buffer>(len(ss));
    foreach (var (i, s) in ss) {
        var (w, _) = NewWriterDict(new flate_test_package.bytes_BufferжWriter(Ꮡ(deflated, i)), DefaultCompression, dict);
        w.Write(slice<byte>(s));
        w.Close();
    }
    var inflated = new slice<bytes.Buffer>(len(ss));
    var f = NewReader(default!);
    foreach (var (i, _) in inflated) {
        f._<Resetter>().Reset(new flate_test_package.bytes_BufferжReader(Ꮡ(deflated, i)), dict);
        io.Copy(new flate_test_package.bytes_BufferжWriter(Ꮡ(inflated, i)), f);
    }
    f.Close();
    foreach (var (i, s) in ss) {
        if (s != Ꮡ(inflated, i).String()) {
            Ꮡt.Errorf("inflated[%d]:\ngot  %q\nwant %q"u8, i, inflated[i], s);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string bufferIsReusedˢ = "BufferIsReused"u8;

[GoType("dyn")] internal partial struct TestReaderReusesReaderBuffer_encodedNotByteReader {
    [GoEmbedded] public io_package.Reader Reader;
}

public static void TestReaderReusesReaderBuffer(ж<testing.T> Ꮡt) {
    var encodedReader = bytes.NewReader(new byte[]{}.slice());
    ref var encodedNotByteReader = ref heap<TestReaderReusesReaderBuffer_encodedNotByteReader>(out var ᏑencodedNotByteReader);
    encodedNotByteReader = new TestReaderReusesReaderBuffer_encodedNotByteReader(new flate_test_package.bytes_Readerжio_Reader(encodedReader));
    var encodedNotByteReaderʗ1 = encodedNotByteReader;
    Ꮡt.Run(bufferIsReusedˢ, (ж<testing.T> tΔ1) => {
        var f = NewReader(encodedNotByteReaderʗ1)._<ж<global::go.compress.flate_package.decompressor>>();
        var (bufioR, ok) = (~f).r._<ж<bufio.Reader>>(ᐧ);
        if (!ok) {
            tΔ1.Fatalf("bufio.Reader should be created"u8);
        }
        f.Reset(encodedNotByteReaderʗ1, default!);
        if (!AreEqual(bufioR, (~f).r)) {
            tΔ1.Fatalf("bufio.Reader was not reused"u8);
        }
    });
    var encodedNotByteReaderʗ2 = encodedNotByteReader;
    var encodedReaderʗ1 = encodedReader;
    Ꮡt.Run("BufferIsNotReusedWhenGotByteReader"u8, (ж<testing.T> tΔ2) => {
        var f = NewReader(encodedNotByteReaderʗ2)._<ж<global::go.compress.flate_package.decompressor>>();
        {
            var (_, ok) = (~f).r._<ж<bufio.Reader>>(ᐧ); if (!ok) {
                tΔ2.Fatalf("bufio.Reader should be created"u8);
            }
        }
        f.Reset(new flate_test_package.bytes_Readerжio_Reader(encodedReaderʗ1), default!);
        if (!AreEqual((~f).r, encodedReaderʗ1)) {
            tΔ2.Fatalf("provided io.ByteReader should be used directly"u8);
        }
    });
    var encodedNotByteReaderʗ3 = encodedNotByteReader;
    var encodedReaderʗ2 = encodedReader;
    Ꮡt.Run("BufferIsCreatedAfterByteReader"u8, (ж<testing.T> tΔ3) => {
        foreach (var (i, r) in new io.Reader[]{new flate_test_package.bytes_Readerжio_Reader(encodedReaderʗ2), new flate_test_package.bufio_ReaderжReader(bufio.NewReader(new flate_test_package.bytes_Readerжio_Reader(encodedReaderʗ2)))}.slice()) {
            var f = NewReader(r)._<ж<global::go.compress.flate_package.decompressor>>();
            if (!AreEqual((~f).r, r)) {
                tΔ3.Fatalf("provided io.ByteReader should be used directly, i=%d"u8, i);
            }
            f.Reset(encodedNotByteReaderʗ3, default!);
            {
                var (_, ok) = (~f).r._<ж<bufio.Reader>>(ᐧ); if (!ok) {
                    tΔ3.Fatalf("bufio.Reader should be created, i=%d"u8, i);
                }
            }
        }
    });
}

} // end flate_internal_test_package
