// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Pipe adapter to connect code expecting an io.Reader
// with code expecting an io.Writer.
namespace go;

using errors = errors_package;
using Δsync = sync_package;

partial class io_package {

// onceError is an object that will only store an error once.
partial struct onceError {
    public partial ref sync_package.Mutex Mutex { get; } // guards following
    internal error err;
}

internal static void Store(this ж<onceError> Ꮡa, error err) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var a = ref Ꮡa.DerefOrNull();

        Ꮡa.of(onceError.ᏑMutex).Lock();
        ᒐd1 = true;
        if (a.err != default!) {
            return;
        }
        a.err = err;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡa.of(onceError.ᏑMutex).Unlock(); ᒐ.Run(); }
}

internal static error Load(this ж<onceError> Ꮡa) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var a = ref Ꮡa.DerefOrNull();

        Ꮡa.of(onceError.ᏑMutex).Lock();
        ᒐd1 = true;
        return a.err;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡa.of(onceError.ᏑMutex).Unlock(); ᒐ.Run(); }
}

// ErrClosedPipe is the error used for read or write operations on a closed pipe.
public static error ErrClosedPipe = errors.New("io: read/write on closed pipe"u8);

// A pipe is the shared pipe structure underlying PipeReader and PipeWriter.
partial struct pipe {
    internal Δsync.Mutex wrMu; // Serializes Write operations
    internal channel<slice<byte>> wrCh;
    internal channel<nint> rdCh;
    internal Δsync.Once once; // Protects closing done
    internal channel<EmptyStruct> done;
    internal onceError rerr;
    internal onceError werr;
}

internal static (nint n, error err) read(this ж<pipe> Ꮡp, slice<byte> b) {
    ref var p = ref Ꮡp.DerefOrNull();

    var selᴛ1 = p.done;
    switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out _): {
        return (0, Ꮡp.readCloseError());
    }
    default: {
        break;
    }}
    var selᴛ2 = p.wrCh;
    var selᴛ3 = p.done;
    switch (select(ᐸꟷ(selᴛ2, ꓸꓸꓸ), ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
    case 0 when selᴛ2.ꟷᐳ(out var bw): {
        nint nr = copy(b, bw);
        p.rdCh.ᐸꟷ(nr);
        return (nr, default!);
    }
    case 1 when selᴛ3.ꟷᐳ(out _): {
        return (0, Ꮡp.readCloseError());
    }}
    return default!;
}

internal static error closeRead(this ж<pipe> Ꮡp, error err) {
    if (err == default!) {
        err = ErrClosedPipe;
    }
    Ꮡp.of(pipe.Ꮡrerr).Store(err);
    Ꮡp.of(pipe.Ꮡonce).Do(() => {
        close(Ꮡp.Value.done);
    });
    return default!;
}

internal static (nint n, error err) write(this ж<pipe> Ꮡp, slice<byte> b) {
    nint n = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var p = ref Ꮡp.DerefOrNull();

        var selᴛ4 = p.done;
        switch (trySelect(ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
        case 0 when selᴛ4.ꟷᐳ(out _): {
            (n, err) = (0, Ꮡp.writeCloseError()); goto ᒐdone;
        }
        default: {
            p.wrMu.Lock();
            defer(Ꮡp.of(pipe.ᏑwrMu).Unlock, ref ᒐ);
            break;
        }}
        for (var once = true; once || len(b) > 0; once = false) {
            var selᴛ5 = p.wrCh.ᐸꟷ(b, ꓸꓸꓸ);
            var selᴛ6 = p.done;
            switch (select(selᴛ5, ᐸꟷ(selᴛ6, ꓸꓸꓸ))) {
            case 0: {
                nint nw = ᐸꟷ(p.rdCh);
                b = b.slice(nw);
                n += nw;
                break;
            }
            case 1 when selᴛ6.ꟷᐳ(out _): {
                (n, err) = (n, Ꮡp.writeCloseError()); goto ᒐdone;
            }}
        }
        (n, err) = (n, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (n, err);
}

internal static error closeWrite(this ж<pipe> Ꮡp, error err) {
    if (err == default!) {
        err = EOF;
    }
    Ꮡp.of(pipe.Ꮡwerr).Store(err);
    Ꮡp.of(pipe.Ꮡonce).Do(() => {
        close(Ꮡp.Value.done);
    });
    return default!;
}

// readCloseError is considered internal to the pipe type.
internal static error readCloseError(this ж<pipe> Ꮡp) {
    var rerr = Ꮡp.of(pipe.Ꮡrerr).Load();
    {
        var werr = Ꮡp.of(pipe.Ꮡwerr).Load(); if (rerr == default! && werr != default!) {
            return werr;
        }
    }
    return ErrClosedPipe;
}

// writeCloseError is considered internal to the pipe type.
internal static error writeCloseError(this ж<pipe> Ꮡp) {
    var werr = Ꮡp.of(pipe.Ꮡwerr).Load();
    {
        var rerr = Ꮡp.of(pipe.Ꮡrerr).Load(); if (werr == default! && rerr != default!) {
            return rerr;
        }
    }
    return ErrClosedPipe;
}

// A PipeReader is the read half of a pipe.
partial struct PipeReader {
    internal partial ref pipe pipe { get; }
}

// Read implements the standard Read interface:
// it reads data from the pipe, blocking until a writer
// arrives or the write end is closed.
// If the write end is closed with an error, that error is
// returned as err; otherwise err is EOF.
public static (nint n, error err) Read(this ж<PipeReader> Ꮡr, slice<byte> data) {
    return Ꮡr.of(PipeReader.Ꮡpipe).read(data);
}

// Close closes the reader; subsequent writes to the
// write half of the pipe will return the error [ErrClosedPipe].
public static error Close(this ж<PipeReader> Ꮡr) {
    return Ꮡr.CloseWithError(default!);
}

// CloseWithError closes the reader; subsequent writes
// to the write half of the pipe will return the error err.
//
// CloseWithError never overwrites the previous error if it exists
// and always returns nil.
public static error CloseWithError(this ж<PipeReader> Ꮡr, error err) {
    return Ꮡr.of(PipeReader.Ꮡpipe).closeRead(err);
}

// A PipeWriter is the write half of a pipe.
partial struct PipeWriter {
    internal PipeReader r;
}

// Write implements the standard Write interface:
// it writes data to the pipe, blocking until one or more readers
// have consumed all the data or the read end is closed.
// If the read end is closed with an error, that err is
// returned as err; otherwise err is [ErrClosedPipe].
public static (nint n, error err) Write(this ж<PipeWriter> Ꮡw, slice<byte> data) {
    return Ꮡw.of(PipeWriter.Ꮡr).of(PipeReader.Ꮡpipe).write(data);
}

// Close closes the writer; subsequent reads from the
// read half of the pipe will return no bytes and EOF.
public static error Close(this ж<PipeWriter> Ꮡw) {
    return Ꮡw.CloseWithError(default!);
}

// CloseWithError closes the writer; subsequent reads from the
// read half of the pipe will return no bytes and the error err,
// or EOF if err is nil.
//
// CloseWithError never overwrites the previous error if it exists
// and always returns nil.
public static error CloseWithError(this ж<PipeWriter> Ꮡw, error err) {
    return Ꮡw.of(PipeWriter.Ꮡr).of(PipeReader.Ꮡpipe).closeWrite(err);
}

// Pipe creates a synchronous in-memory pipe.
// It can be used to connect code expecting an [io.Reader]
// with code expecting an [io.Writer].
//
// Reads and Writes on the pipe are matched one to one
// except when multiple Reads are needed to consume a single Write.
// That is, each Write to the [PipeWriter] blocks until it has satisfied
// one or more Reads from the [PipeReader] that fully consume
// the written data.
// The data is copied directly from the Write to the corresponding
// Read (or Reads); there is no internal buffering.
//
// It is safe to call Read and Write in parallel with each other or with Close.
// Parallel calls to Read and parallel calls to Write are also safe:
// the individual calls will be gated sequentially.
public static (ж<PipeReader>, ж<PipeWriter>) Pipe() {
    var pw = Ꮡ(new PipeWriter(r: new PipeReader(pipe: new pipe(
        wrCh: new channel<slice<byte>>(0),
        rdCh: new channel<nint>(0),
        done: new channel<EmptyStruct>(0)
    )
    )
    ));
    return (pw.of(PipeWriter.Ꮡr), pw);
}

} // end io_package
