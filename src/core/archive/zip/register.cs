// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.archive;

using flate = compress.flate_package;
using errors = errors_package;
using io = io_package;
using sync = sync_package;
using compress;

partial class zip_package {

// type Compressor is a methodless func type — rendered inline as its base delegate

// type Decompressor is a methodless func type — rendered inline as its base delegate

internal static ж<sync.Pool> ᏑflateWriterPool = new StandardBox<sync.Pool>(default(sync.Pool));
internal static ref sync.Pool flateWriterPool => ref ᏑflateWriterPool.Value;

internal static io.WriteCloser newFlateWriter(io.Writer w) {
    var (fw, ok) = ᏑflateWriterPool.Get()._<ж<flate.Writer>>(ᐧ);
    if (ok){
        fw.Reset(w);
    } else {
        (fw, _) = flate.NewWriter(w, 5);
    }
    return new pooledFlateWriterжWriteCloser(Ꮡ(new pooledFlateWriter(fw: fw)));
}

partial struct pooledFlateWriter {
    internal sync.Mutex mu; // guards Close and Write
    internal ж<flate.Writer> fw;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string writeAfterCloseˢ = "Write after Close"u8;

internal static (nint n, error err) Write(this ж<pooledFlateWriter> Ꮡw, slice<byte> p) {
    nint n = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var w = ref Ꮡw.DerefOrNull();

        w.mu.Lock();
        defer(Ꮡw.of(pooledFlateWriter.Ꮡmu).Unlock, ref ᒐ);
        if (w.fw == nil) {
            (n, err) = (0, errors.New(writeAfterCloseˢ)); goto ᒐdone;
        }
        (n, err) = w.fw.Write(p);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (n, err);
}

internal static error Close(this ж<pooledFlateWriter> Ꮡw) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var w = ref Ꮡw.DerefOrNull();

        w.mu.Lock();
        ᒐd1 = true;
        error err = default!;
        if (w.fw != nil) {
            err = w.fw.Close();
            ᏑflateWriterPool.Put(w.fw.OrTypedNil());
            w.fw = default!;
        }
        return err;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡw.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static ж<sync.Pool> ᏑflateReaderPool = new StandardBox<sync.Pool>(default(sync.Pool));
internal static ref sync.Pool flateReaderPool => ref ᏑflateReaderPool.Value;

internal static io.ReadCloser newFlateReader(io.Reader r) {
    var (fr, ok) = ᏑflateReaderPool.Get()._<io.ReadCloser>(ᐧ);
    if (ok){
        fr._<flate.Resetter>().Reset(r, default!);
    } else {
        fr = flate.NewReader(r);
    }
    return new pooledFlateReaderжReadCloser(Ꮡ(new pooledFlateReader(fr: fr)));
}

partial struct pooledFlateReader {
    internal sync.Mutex mu; // guards Close and Read
    internal io.ReadCloser fr;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string readAfterCloseˢ = "Read after Close"u8;

internal static (nint n, error err) Read(this ж<pooledFlateReader> Ꮡr, slice<byte> p) {
    nint n = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var r = ref Ꮡr.DerefOrNull();

        r.mu.Lock();
        defer(Ꮡr.of(pooledFlateReader.Ꮡmu).Unlock, ref ᒐ);
        if (r.fr == default!) {
            (n, err) = (0, errors.New(readAfterCloseˢ)); goto ᒐdone;
        }
        (n, err) = r.fr.Read(p);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (n, err);
}

internal static error Close(this ж<pooledFlateReader> Ꮡr) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var r = ref Ꮡr.DerefOrNull();

        r.mu.Lock();
        ᒐd1 = true;
        error err = default!;
        if (r.fr != default!) {
            err = r.fr.Close();
            ᏑflateReaderPool.Put(r.fr);
            r.fr = default!;
        }
        return err;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡr.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static ж<sync.Map> Ꮡcompressors = new StandardBox<sync.Map>(default(sync.Map));
internal static ref sync.Map compressors => ref Ꮡcompressors.Value; // map[uint16]Compressor
internal static ж<sync.Map> Ꮡdecompressors = new StandardBox<sync.Map>(default(sync.Map));
internal static ref sync.Map decompressors => ref Ꮡdecompressors.Value; // map[uint16]Decompressor

[GoInit] internal static void init() {
    Ꮡcompressors.Store(Store, new Func<io.Writer, (io.WriteCloser, error)>((io.Writer w) => (new nopCloserжWriteCloser(Ꮡ(new nopCloser(w))), default!)));
    Ꮡcompressors.Store(Deflate, new Func<io.Writer, (io.WriteCloser, error)>((io.Writer w) => (newFlateWriter(w), default!)));
    Ꮡdecompressors.Store(Store, NilSafeDelegateConversion<Func<io.Reader, io.ReadCloser>, Func<io.Reader, io.ReadCloser>>(io.NopCloser));
    Ꮡdecompressors.Store(Deflate, new Func<io.Reader, io.ReadCloser>(newFlateReader));
}

// RegisterDecompressor allows custom decompressors for a specified method ID.
// The common methods [Store] and [Deflate] are built in.
public static void RegisterDecompressor(uint16 method, Func<io.Reader, io.ReadCloser> dcomp) {
    {
        var (_, dup) = Ꮡdecompressors.LoadOrStore(method, (dcomp).OrTypedNilFunc()); if (dup) {
            throw panic("decompressor already registered");
        }
    }
}

// RegisterCompressor registers custom compressors for a specified method ID.
// The common methods [Store] and [Deflate] are built in.
public static void RegisterCompressor(uint16 method, Func<io.Writer, (io.WriteCloser, error)> comp) {
    {
        var (_, dup) = Ꮡcompressors.LoadOrStore(method, (comp).OrTypedNilFunc()); if (dup) {
            throw panic("compressor already registered");
        }
    }
}

internal static Func<io.Writer, (io.WriteCloser, error)> compressor(uint16 method) {
    var (ci, ok) = Ꮡcompressors.Load(method);
    if (!ok) {
        return default!;
    }
    return ci._<Func<io.Writer, (io.WriteCloser, error)>>();
}

internal static Func<io.Reader, io.ReadCloser> decompressor(uint16 method) {
    var (di, ok) = Ꮡdecompressors.Load(method);
    if (!ok) {
        return default!;
    }
    return di._<Func<io.Reader, io.ReadCloser>>();
}

} // end zip_package
