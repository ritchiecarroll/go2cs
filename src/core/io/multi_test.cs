// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using sha1 = crypto.sha1_package;
using errors = errors_package;
using fmt = fmt_package;
using static io_package;
using Δruntime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using crypto;
using hash = hash_package;
using static go.io_internal_test_package;
using Δio = io_package;

partial class io_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooˢ2 => ref fooˢ2ᵇ;
private static readonly @string fooˢ2ᵇ = "foo "u8;
internal static ref readonly @string barˢ => ref barˢᵇ;
private static readonly @string barˢᵇ = "bar"u8;

public static void TestMultiReader(ж<testing.T> t) {
    ref var mr = ref heap<Δio.Reader>(out var Ꮡmr);
    ref var buf = ref heap<slice<byte>>(out var Ꮡbuf);
    nint nread = 0;
    void withFooBar(Action tests) {
        var r1 = strings.NewReader(fooˢ2);
        var r2 = strings.NewReader(""u8);
        var r3 = strings.NewReader(barˢ);
        Ꮡmr.ValueSlot = MultiReader(new io_test_package.strings_ReaderжReader(r1), new io_test_package.strings_ReaderжReader(r2), new io_test_package.strings_ReaderжReader(r3));
        Ꮡbuf.ValueSlot = new slice<byte>(20);
        tests();
    }
    void expectRead(nint size, @string expected, error eerr) {
        nread++;
        var (n, gerr) = Ꮡmr.ValueSlot.Read(Ꮡbuf.ValueSlot.slice(0, size));
        if (n != len(expected)) {
            t.Errorf("#%d, expected %d bytes; got %d",
                nread, len(expected), n);
        }
        @string got = (@string)(Ꮡbuf.ValueSlot.slice(0, n));
        if (got != expected) {
            t.Errorf("#%d, expected %q; got %q",
                nread, expected, got);
        }
        if (!AreEqual(gerr, eerr)) {
            t.Errorf("#%d, expected error %v; got %v",
                nread, eerr, gerr);
        }
        Ꮡbuf.ValueSlot = Ꮡbuf.ValueSlot.slice(n);
    }
    var expectReadʗ1 = expectRead;
    withFooBar(() => {
        expectReadʗ1(2, "fo", default);
        expectReadʗ1(5, "o ", default);
        expectReadʗ1(5, barˢ, default);
        expectReadʗ1(5, ""u8, EOF);
    });
    var expectReadʗ2 = expectRead;
    withFooBar(() => {
        expectReadʗ2(4, fooˢ2, default);
        expectReadʗ2(1, "b", default);
        expectReadʗ2(3, "ar", default);
        expectReadʗ2(1, ""u8, EOF);
    });
    var expectReadʗ3 = expectRead;
    withFooBar(() => {
        expectReadʗ3(5, fooˢ2, default);
    });
}

public static void TestMultiReaderAsWriterTo(ж<testing.T> t) {
    var mr = MultiReader(
        new io_test_package.strings_ReaderжReader(strings.NewReader(fooˢ2)),
        MultiReader( // Tickle the buffer reusing codepath
            new io_test_package.strings_ReaderжReader(strings.NewReader(""u8)),
            new io_test_package.strings_ReaderжReader(strings.NewReader(barˢ))));
    var (mrAsWriterTo, ok) = mr._<WriterTo>(ᐧ);
    if (!ok) {
        t.Fatalf("expected cast to WriterTo to succeed");
    }
    var sink = Ꮡ(new strings.Builder());
    var (n, err) = mrAsWriterTo.WriteTo(new io_test_package.strings_BuilderжWriter(sink));
    if (err != nil) {
        t.Fatalf("expected no error; got %v", err);
    }
    if (n != 7) {
        t.Errorf("expected read 7 bytes; got %d", n);
    }
    {
        @string result = sink.String(); if (result != "foo bar"u8) {
            t.Errorf(@"expected ""foo bar""; got %q", result);
        }
    }
}

internal partial struct TestMultiWriter_sink /*dyn*/ {
    /*embed*/ public io_package.Writer Writer;
    /*embed*/ public fmt_package.Stringer Stringer;
}

public static void TestMultiWriter(ж<testing.T> t) {
    var sink = @new<bytes.Buffer>();
    // Hide bytes.Buffer's WriteString method:
    testMultiWriter(t, new TestMultiWriter_sink(new io_test_package.bytes_BufferжWriter(sink), new io_test_package.bytes_BufferжStringer(sink)));
}

public static void TestMultiWriter_String(ж<testing.T> t) {
    testMultiWriter(t, new bytes_BufferжtestMultiWriter_sink(@new<bytes.Buffer>()));
}

internal partial struct TestMultiWriter_WriteStringSingleAlloc_simpleWriter /*dyn*/ {
// hide bytes.Buffer's WriteString
    /*embed*/ public io_package.Writer Writer;
}

// Test that a multiWriter.WriteString calls results in at most 1 allocation,
// even if multiple targets don't support WriteString.
public static void TestMultiWriter_WriteStringSingleAlloc(ж<testing.T> t) {
    ref var sink1 = ref heap(new bytes.Buffer(), out var Ꮡsink1);
    ref var sink2 = ref heap(new bytes.Buffer(), out var Ꮡsink2);
    var mw = MultiWriter(new TestMultiWriter_WriteStringSingleAlloc_simpleWriter(new io_test_package.bytes_BufferжWriter(Ꮡsink1)), new TestMultiWriter_WriteStringSingleAlloc_simpleWriter(new io_test_package.bytes_BufferжWriter(Ꮡsink2)));
    var mwʗ1 = mw;
    nint allocs = (nint)testing.AllocsPerRun(1000, () => {
        WriteString(mwʗ1, fooˢ);
    });
    if (allocs != 1) {
        t.Errorf("num allocations = %d; want 1", allocs);
    }
}

partial struct writeStringChecker {
    internal bool called;
}

internal static (nint n, error err) ΔWriteString(this ref writeStringChecker c, @string s) {
    c.called = true;
    return (len(s), default);
}

internal static (nint n, error err) Write(this ref writeStringChecker c, slice<byte> p) {
    return (len(p), default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object didNotSeeWriteStringCallˢ = (@string)"did not see WriteString call to writeStringChecker"u8;

public static void TestMultiWriter_StringCheckCall(ж<testing.T> t) {
    ref var c = ref heap(new writeStringChecker(), out var Ꮡc);
    var mw = MultiWriter(new io_test_package.writeStringCheckerжWriter(Ꮡc));
    WriteString(mw, fooˢ);
    if (!c.called) {
        t.Error(didNotSeeWriteStringCallˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string myInputTextˢ => ref myInputTextˢᵇ;
private static readonly @string myInputTextˢᵇ = "My input text."u8;
internal static readonly object incorrectSha1Valueˢ = (@string)"incorrect sha1 value"u8;

internal partial interface testMultiWriter_sink /*dyn*/ :
    Writer,
    fmt.Stringer
{
}

internal static void testMultiWriter(ж<testing.T> t, testMultiWriter_sink sink) {
    var sha1Δ1 = sha1.New();
    var mw = MultiWriter(sha1Δ1, sink);

    @string sourceString = myInputTextˢ;
    var source = strings.NewReader(sourceString);
    var (written, err) = Copy(mw, new io_test_package.strings_ReaderжReader(source));

    if (written != (int64)len(sourceString)) {
        t.Errorf("short write of %d, not %d", written, len(sourceString));
    }

    if (err != nil) {
        t.Errorf("unexpected error: %v", err);
    }

    @string sha1hex = fmt.Sprintf("%x"u8, sha1Δ1.Sum(default));
    if (sha1hex != "01cb303fa8c30a64123067c5aa6284ba7ec2d31b"u8) {
        t.Error(incorrectSha1Valueˢ);
    }

    if (sink.String() != sourceString) {
        t.Errorf("expected %q; got %q", sourceString, sink.String());
    }
}

// writerFunc is a Writer implemented by the underlying func.
internal delegate (nint, error) writerFunc(slice<byte> p);

internal static partial (nint, error) Write(this writerFunc f, slice<byte> p) {
    return f(p);
}

// Test that MultiWriter properly flattens chained multiWriters.
public static partial void TestMultiWriterSingleChainFlatten(ж<testing.T> t) {
    var pc = new slice<uintptr>(1000); // 1000 should fit the full stack
    nint n = Δruntime.Callers(0, pc);
    nint myDepth = callDepth(pc.slice(0, n));
    nint writeDepth = default; // will contain the depth from which writerFunc.Writer was called

    var pcʗ1 = pc;
    Δio.Writer w = MultiWriter(new io_test_package.writerFuncᴠWriter(new writerFunc([MethodImpl(MethodImplOptions.NoInlining)] (slice<byte> p) => {
        nint nΔ1 = Δruntime.Callers(1, pcʗ1);
        writeDepth += callDepth(pcʗ1.slice(0, nΔ1));
        return (0, default);
    })));

    var mw = w;
    // chain a bunch of multiWriters
    for (nint i = 0; i < 100; i++) {
        mw = MultiWriter(w);
    }

    mw = MultiWriter(w, mw, w, mw);
    mw.Write(default); // don't care about errors, just want to check the call-depth for Write

    if (writeDepth != 4 * (myDepth + 2)) { // 2 should be multiWriter.Write and writerFunc.Write
        t.Errorf("multiWriter did not flatten chained multiWriters: expected writeDepth %d, got %d",
            4 * (myDepth + 2), writeDepth);
    }
}

public static void TestMultiWriterError(ж<testing.T> t) {
    var f1 = new writerFunc((slice<byte> p) => (len(p) / 2, ErrShortWrite));
    var f2 = new writerFunc((slice<byte> p) => {
        t.Errorf("MultiWriter called f2.Write");
        return (len(p), default);
    });
    var w = MultiWriter(new io_test_package.writerFuncᴠWriter(f1), new io_test_package.writerFuncᴠWriter(f2));
    var (n, err) = w.Write(new slice<byte>(100));
    if (n != 50 || !AreEqual(err, ErrShortWrite)) {
        t.Errorf("Write = %d, %v, want 50, ErrShortWrite", n, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloWorldˢ2 => ref helloWorldˢ2ᵇ;
private static readonly @string helloWorldˢ2ᵇ = "hello world"u8;

// Test that MultiReader copies the input slice and is insulated from future modification.
public static void TestMultiReaderCopy(ж<testing.T> t) {
    var Δslice = slice<Δio.Reader>([new io_test_package.strings_ReaderжReader(strings.NewReader(helloWorldˢ2))]);
    var r = MultiReader(Δslice.ꓸꓸꓸ);
    Δslice[0] = default;
    var (data, err) = ReadAll(r);
    if (err != nil || ((sstring)data) != "hello world"u8) {
        t.Errorf("ReadAll() = %q, %v, want %q, nil", data, err, helloWorldˢ2);
    }
}

// Test that MultiWriter copies the input slice and is insulated from future modification.
public static void TestMultiWriterCopy(ж<testing.T> t) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    var Δslice = slice<Δio.Writer>([new io_test_package.strings_BuilderжWriter(Ꮡbuf)]);
    var w = MultiWriter(Δslice.ꓸꓸꓸ);
    Δslice[0] = default;
    var (n, err) = w.Write(slice<byte>("hello world"u8));
    if (err != nil || n != 11) {
        t.Errorf("Write(`hello world`) = %d, %v, want 11, nil", n, err);
    }
    if (buf.String() != "hello world"u8) {
        t.Errorf("buf.String() = %q, want %q", buf.String(), helloWorldˢ2);
    }
}

// readerFunc is a Reader implemented by the underlying func.
internal delegate (nint, error) readerFunc(slice<byte> p);

internal static partial (nint, error) Read(this readerFunc f, slice<byte> p) {
    return f(p);
}

// callDepth returns the logical call depth for the given PCs.
internal static nint /*depth*/ callDepth(slice<uintptr> callers) {
    nint depth = default;

    var frames = Δruntime.CallersFrames(callers);
    var more = true;
    while (more) {
        (_, more) = frames.Next();
        depth++;
    }
    return depth;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string irrelevantˢ => ref irrelevantˢᵇ;
private static readonly @string irrelevantˢᵇ = "irrelevant"u8;

// Test that MultiReader properly flattens chained multiReaders when Read is called
public static partial void TestMultiReaderFlatten(ж<testing.T> t) {
    var pc = new slice<uintptr>(1000); // 1000 should fit the full stack
    nint n = Δruntime.Callers(0, pc);
    nint myDepth = callDepth(pc.slice(0, n));
    nint readDepth = default; // will contain the depth from which fakeReader.Read was called

    var pcʗ1 = pc;
    Δio.Reader r = MultiReader(new io_test_package.readerFuncᴠReader(new readerFunc([MethodImpl(MethodImplOptions.NoInlining)] (slice<byte> p) => {
        nint nΔ1 = Δruntime.Callers(1, pcʗ1);
        readDepth = callDepth(pcʗ1.slice(0, nΔ1));
        return (0, errors.New(irrelevantˢ));
    })));

    // chain a bunch of multiReaders
    for (nint i = 0; i < 100; i++) {
        r = MultiReader(r);
    }

    r.Read(default); // don't care about errors, just want to check the call-depth for Read

    if (readDepth != myDepth + 2) { // 2 should be multiReader.Read and fakeReader.Read
        t.Errorf("multiReader did not flatten chained multiReaders: expected readDepth %d, got %d",
            myDepth + 2, readDepth);
    }
}

// byteAndEOFReader is a Reader which reads one byte (the underlying
// byte) and EOF at once in its Read call.
partial struct byteAndEOFReader /*num:byte*/;

internal static (nint n, error err) Read(this byteAndEOFReader b, slice<byte> p) {
    if (len(p) == 0) {
        // Read(0 bytes) is useless. We expect no such useless
        // calls in this test.
        throw panic("unexpected call");
    }
    p[0] = (byte)b;
    return (1, EOF);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ => ref wantᶜᵇ;
private static readonly @string wantᶜᵇ = "ab"u8;

// This used to yield bytes forever; issue 16795.
public static void TestMultiReaderSingleByteWithEOF(ж<testing.T> t) {
    var (got, err) = ReadAll(LimitReader(MultiReader((byteAndEOFReader)(rune)'a', (byteAndEOFReader)(rune)'b'), 10));
    if (err != nil) {
        t.Fatal(err);
    }
    @string want = wantᶜ;
    if (((sstring)got) != want) {
        t.Errorf("got %q; want %q", got, want);
    }
}

// Test that a reader returning (n, EOF) at the end of a MultiReader
// chain continues to return EOF on its final read, rather than
// yielding a (0, EOF).
public static void TestMultiReaderFinalEOF(ж<testing.T> t) {
    var r = MultiReader(new io_test_package.bytes_ReaderжReader(bytes.NewReader(default)), (byteAndEOFReader)(rune)'a');
    var buf = new slice<byte>(2);
    var (n, err) = r.Read(buf);
    if (n != 1 || !AreEqual(err, EOF)) {
        t.Errorf("got %v, %v; want 1, EOF", n, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object timeoutWaitingForˢ = (@string)"timeout waiting for collection of buf1"u8;

public static void TestMultiReaderFreesExhaustedReaders(ж<testing.T> t) {
    ref var mr = ref heap<Δio.Reader>(out var Ꮡmr);
    var closed = new channel<EmptyStruct>(0);
    // The closure ensures that we don't have a live reference to buf1
    // on our stack after MultiReader is inlined (Issue 18819).  This
    // is a work around for a limitation in liveness analysis.
    var closedʗ1 = closed;
    func(() => {
        var buf1 = bytes.NewReader(slice<byte>("foo"u8));
        var buf2 = bytes.NewReader(slice<byte>("bar"u8));
        Ꮡmr.ValueSlot = MultiReader(new io_test_package.bytes_ReaderжReader(buf1), new io_test_package.bytes_ReaderжReader(buf2));
        Δruntime.AddCleanup(buf1, (channel<EmptyStruct> ch) => {
            close(ch);
        }, closedʗ1);
    })();

    var buf = new slice<byte>(4);
    {
        var (n, err) = ReadFull(mr, buf); if (err != nil || ((sstring)buf) != "foob"u8) {
            t.Fatalf(@"ReadFull = %d (%q), %v; want 3, ""foo"", nil", n, buf.slice(0, n), err);
        }
    }

    Δruntime.GC();
    var selᴛ1 = closed;
    var selᴛ2 = time.After((time.Duration)(5000000000L));
    switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out _): {
        break;
    }
    case 1 when selᴛ2.ꟷᐳ(out _): {
        t.Fatal(timeoutWaitingForˢ);
        break;
    }}

    {
        var (n, err) = ReadFull(mr, buf[..2]); if (err != nil || ((sstring)(buf[..2])) != "ar"u8) {
            t.Fatalf(@"ReadFull = %d (%q), %v; want 2, ""ar"", nil", n, buf.slice(0, n), err);
        }
    }
}

public static void TestInterleavedMultiReader(ж<testing.T> t) {
    var r1 = strings.NewReader("123");
    var r2 = strings.NewReader("45678");

    var mr1 = MultiReader(new io_test_package.strings_ReaderжReader(r1), new io_test_package.strings_ReaderжReader(r2));
    var mr2 = MultiReader(mr1);

    var buf = new slice<byte>(4);

    // Have mr2 use mr1's []Readers.
    // Consume r1 (and clear it for GC to handle) and consume part of r2.
    var (n, err) = ReadFull(mr2, buf);
    {
        @string got = (@string)(buf.slice(0, n)); if (got != "1234"u8 || err != nil) {
            t.Errorf(@"ReadFull(mr2) = (%q, %v), want (""1234"", nil)", got, err);
        }
    }

    // Consume the rest of r2 via mr1.
    // This should not panic even though mr2 cleared r1.
    (n, err) = ReadFull(mr1, buf);
    {
        @string got = (@string)(buf.slice(0, n)); if (got != "5678"u8 || err != nil) {
            t.Errorf(@"ReadFull(mr1) = (%q, %v), want (""5678"", nil)", got, err);
        }
    }
}

} // end io_test_package
