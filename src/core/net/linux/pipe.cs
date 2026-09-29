// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δio = io_package;
using os = os_package;
using Δsync = sync_package;
using time = time_package;

partial class net_package {

// pipeDeadline is an abstraction for handling timeouts.
[GoType] partial struct pipeDeadline {
    internal Δsync.Mutex mu; // Guards timer and cancel
    internal ж<time.Timer> timer;
    internal channel<EmptyStruct> cancel; // Must be non-nil
}

internal static pipeDeadline makePipeDeadline() {
    return new pipeDeadline(cancel: new channel<EmptyStruct>(0));
}

// set sets the point in time when the deadline will time out.
// A timeout event is signaled by closing the channel returned by waiter.
// Once a timeout has occurred, the deadline can be refreshed by specifying a
// t value in the future.
//
// A zero value for t prevents timeout.
internal static void set(this ж<pipeDeadline> Ꮡd, time.Time t) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var d = ref Ꮡd.DerefOrNull();

        d.mu.Lock();
        ᒐd1 = true;
        if (d.timer != nil && !d.timer.Stop()) {
            ᐸꟷ(d.cancel); // Wait for the timer callback to finish and close cancel
        }
        d.timer = default!;
        // Time is zero, then there is no deadline.
        var closed = isClosedChan(d.cancel.WithDirection(GoChanDir.Recv));
        if (t.IsZero()) {
            if (closed) {
                d.cancel = new channel<EmptyStruct>(0);
            }
            return;
        }
        // Time in the future, setup a timer to cancel in the future.
        {
            var dur = time.Until(t); if (dur > 0) {
                if (closed) {
                    d.cancel = new channel<EmptyStruct>(0);
                }
                d.timer = time.AfterFunc(dur, () => {
                    builtin.close(Ꮡd.Value.cancel);
                });
                return;
            }
        }
        // Time in the past, so close immediately.
        if (!closed) {
            builtin.close(d.cancel);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡd.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

// wait returns a channel that is closed when the deadline is exceeded.
internal static channel<EmptyStruct> wait(this ж<pipeDeadline> Ꮡd) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var d = ref Ꮡd.DerefOrNull();

        d.mu.Lock();
        ᒐd1 = true;
        return d.cancel;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡd.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static bool isClosedChan(/*<-*/channel<EmptyStruct> c) {
    var selᴛ19 = c;
    switch (trySelect(ᐸꟷ(selᴛ19, ꓸꓸꓸ))) {
    case 0 when selᴛ19.ꟷᐳ(out _): {
        return true;
    }
    default: {
        return false;
    }}
}

[GoType] partial struct pipeAddr {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pipeˢ = "pipe"u8;

internal static @string Network(this pipeAddr _) {
    return pipeˢ;
}

internal static @string String(this pipeAddr _) {
    return pipeˢ;
}

[GoType] partial struct pipe {
    internal Δsync.Mutex wrMu; // Serialize Write operations
    // Used by local Read to interact with remote Write.
    // Successful receive on rdRx is always followed by send on rdTx.
    internal /*<-*/channel<slice<byte>> rdRx = /*<-*/channel<slice<byte>>.RecvOnly;
    internal channel/*<-*/<nint> rdTx = channel/*<-*/<nint>.SendOnly;
    // Used by local Write to interact with remote Read.
    // Successful send on wrTx is always followed by receive on wrRx.
    internal channel/*<-*/<slice<byte>> wrTx = channel/*<-*/<slice<byte>>.SendOnly;
    internal /*<-*/channel<nint> wrRx = /*<-*/channel<nint>.RecvOnly;
    internal Δsync.Once once; // Protects closing localDone
    internal channel<EmptyStruct> localDone;
    internal /*<-*/channel<EmptyStruct> remoteDone = /*<-*/channel<EmptyStruct>.RecvOnly;
    internal pipeDeadline readDeadline;
    internal pipeDeadline writeDeadline;
}

// Pipe creates a synchronous, in-memory, full duplex
// network connection; both ends implement the [Conn] interface.
// Reads on one end are matched with writes on the other,
// copying data directly between the two; there is no internal
// buffering.
public static (Conn, Conn) Pipe() {
    var cb1 = new channel<slice<byte>>(0);
    var cb2 = new channel<slice<byte>>(0);
    var cn1 = new channel<nint>(0);
    var cn2 = new channel<nint>(0);
    var done1 = new channel<EmptyStruct>(0);
    var done2 = new channel<EmptyStruct>(0);
    var p1 = Ꮡ(new pipe(
        rdRx: cb1, rdTx: cn1,
        wrTx: cb2, wrRx: cn2,
        localDone: done1, remoteDone: done2,
        readDeadline: makePipeDeadline(),
        writeDeadline: makePipeDeadline()
    ));
    var p2 = Ꮡ(new pipe(
        rdRx: cb2, rdTx: cn2,
        wrTx: cb1, wrRx: cn1,
        localDone: done2, remoteDone: done1,
        readDeadline: makePipeDeadline(),
        writeDeadline: makePipeDeadline()
    ));
    return (new pipeжConn(p1), new pipeжConn(p2));
}

[GoRecv] internal static ΔAddr LocalAddr(this ref pipe _) {
    return new pipeAddr(nil);
}

[GoRecv] internal static ΔAddr RemoteAddr(this ref pipe _) {
    return new pipeAddr(nil);
}

internal static (nint, error) Read(this ж<pipe> Ꮡp, slice<byte> b) {
    var (n, err) = Ꮡp.read(b);
    if (err != default! && !AreEqual(err, Δio.EOF) && !AreEqual(err, Δio.ErrClosedPipe)) {
        err = new OpErrorжerror(Ꮡ(new OpError(Op: "read"u8, Net: "pipe"u8, Err: err)));
    }
    return (n, err);
}

internal static (nint n, error err) read(this ж<pipe> Ꮡp, slice<byte> b) {
    ref var p = ref Ꮡp.DerefOrNull();

    switch (ᐧ) {
    case {} when isClosedChan(p.localDone.WithDirection(GoChanDir.Recv)): {
        return (0, Δio.ErrClosedPipe);
    }
    case {} when isClosedChan(p.remoteDone): {
        return (0, Δio.EOF);
    }
    case {} when isClosedChan(Ꮡp.of(pipe.ᏑreadDeadline).wait().WithDirection(GoChanDir.Recv)): {
        return (0, os.ErrDeadlineExceeded);
    }}

    var selᴛ20 = p.rdRx;
    var selᴛ21 = p.localDone;
    var selᴛ22 = p.remoteDone;
    var selᴛ23 = Ꮡp.of(pipe.ᏑreadDeadline).wait();
    switch (select(ᐸꟷ(selᴛ20, ꓸꓸꓸ), ᐸꟷ(selᴛ21, ꓸꓸꓸ), ᐸꟷ(selᴛ22, ꓸꓸꓸ), ᐸꟷ(selᴛ23, ꓸꓸꓸ))) {
    case 0 when selᴛ20.ꟷᐳ(out var bw): {
        nint nr = copy(b, bw);
        p.rdTx.ᐸꟷ(nr);
        return (nr, default!);
    }
    case 1 when selᴛ21.ꟷᐳ(out _): {
        return (0, Δio.ErrClosedPipe);
    }
    case 2 when selᴛ22.ꟷᐳ(out _): {
        return (0, Δio.EOF);
    }
    case 3 when selᴛ23.ꟷᐳ(out _): {
        return (0, os.ErrDeadlineExceeded);
    }}
    return default!;
}

internal static (nint, error) Write(this ж<pipe> Ꮡp, slice<byte> b) {
    var (n, err) = Ꮡp.write(b);
    if (err != default! && !AreEqual(err, Δio.ErrClosedPipe)) {
        err = new OpErrorжerror(Ꮡ(new OpError(Op: "write"u8, Net: "pipe"u8, Err: err)));
    }
    return (n, err);
}

internal static (nint n, error err) write(this ж<pipe> Ꮡp, slice<byte> b) {
    nint n = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var p = ref Ꮡp.DerefOrNull();

        switch (ᐧ) {
        case {} when isClosedChan(p.localDone.WithDirection(GoChanDir.Recv)): {
            (n, err) = (0, Δio.ErrClosedPipe); goto ᒐdone;
        }
        case {} when isClosedChan(p.remoteDone): {
            (n, err) = (0, Δio.ErrClosedPipe); goto ᒐdone;
        }
        case {} when isClosedChan(Ꮡp.of(pipe.ᏑwriteDeadline).wait().WithDirection(GoChanDir.Recv)): {
            (n, err) = (0, os.ErrDeadlineExceeded); goto ᒐdone;
        }}

        p.wrMu.Lock(); // Ensure entirety of b is written together
        defer(Ꮡp.of(pipe.ᏑwrMu).Unlock, ref ᒐ);
        for (var once = true; once || len(b) > 0; once = false) {
            var selᴛ24 = p.wrTx.ᐸꟷ(b, ꓸꓸꓸ);
            var selᴛ25 = p.localDone;
            var selᴛ26 = p.remoteDone;
            var selᴛ27 = Ꮡp.of(pipe.ᏑwriteDeadline).wait();
            switch (select(selᴛ24, ᐸꟷ(selᴛ25, ꓸꓸꓸ), ᐸꟷ(selᴛ26, ꓸꓸꓸ), ᐸꟷ(selᴛ27, ꓸꓸꓸ))) {
            case 0: {
                nint nw = ᐸꟷ(p.wrRx);
                b = b.slice(nw);
                n += nw;
                break;
            }
            case 1 when selᴛ25.ꟷᐳ(out _): {
                (n, err) = (n, Δio.ErrClosedPipe); goto ᒐdone;
            }
            case 2 when selᴛ26.ꟷᐳ(out _): {
                (n, err) = (n, Δio.ErrClosedPipe); goto ᒐdone;
            }
            case 3 when selᴛ27.ꟷᐳ(out _): {
                (n, err) = (n, os.ErrDeadlineExceeded); goto ᒐdone;
            }}
        }
        (n, err) = (n, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (n, err);
}

internal static error SetDeadline(this ж<pipe> Ꮡp, time.Time t) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (isClosedChan(p.localDone.WithDirection(GoChanDir.Recv)) || isClosedChan(p.remoteDone)) {
        return Δio.ErrClosedPipe;
    }
    Ꮡp.of(pipe.ᏑreadDeadline).set(t);
    Ꮡp.of(pipe.ᏑwriteDeadline).set(t);
    return default!;
}

internal static error SetReadDeadline(this ж<pipe> Ꮡp, time.Time t) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (isClosedChan(p.localDone.WithDirection(GoChanDir.Recv)) || isClosedChan(p.remoteDone)) {
        return Δio.ErrClosedPipe;
    }
    Ꮡp.of(pipe.ᏑreadDeadline).set(t);
    return default!;
}

internal static error SetWriteDeadline(this ж<pipe> Ꮡp, time.Time t) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (isClosedChan(p.localDone.WithDirection(GoChanDir.Recv)) || isClosedChan(p.remoteDone)) {
        return Δio.ErrClosedPipe;
    }
    Ꮡp.of(pipe.ᏑwriteDeadline).set(t);
    return default!;
}

internal static error Close(this ж<pipe> Ꮡp) {
    Ꮡp.of(pipe.Ꮡonce).Do(() => {
        builtin.close(Ꮡp.Value.localDone);
    });
    return default!;
}

} // end net_package
