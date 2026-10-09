// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using bytes = bytes_package;
using context = context_package;
using synctest = global::go.@internal.synctest_package;
using io = io_package;
using math = math_package;
using net = net_package;
using netip = global::go.net.netip_package;
using os = os_package;
using sync = sync_package;
using time = time_package;
using global::go.@internal;
using global::go.net;
using static global::go.net.http_internal_test_package;

partial class http_test_package {

internal static ж<fakeNetListener> fakeNetListen() {
    var li = Ꮡ(new fakeNetListener(
        setc: new channel<EmptyStruct>(1),
        unsetc: new channel<EmptyStruct>(1),
        addr: netip.MustParseAddrPort("127.0.0.1:8000"u8),
        locPort: 10000
    ));
    (~li).unsetc.ᐸꟷ(new EmptyStruct());
    return li;
}

partial struct fakeNetListener {
    internal channel<EmptyStruct> setc, unsetc;
    internal slice<net.Conn> queue;
    internal bool closed;
    internal netip.AddrPort addr;
    internal uint16 locPort;
    internal Action onDial; // called when making a new connection
    internal bool trackConns; // set this to record all created conns
    internal slice<ж<fakeNetConn>> conns;
}

internal static void @lock(this ref fakeNetListener li) {
    var selᴛ16 = li.setc;
    var selᴛ17 = li.unsetc;
    switch (select(ᐸꟷ(selᴛ16, ꓸꓸꓸ), ᐸꟷ(selᴛ17, ꓸꓸꓸ))) {
    case 0 when selᴛ16.ꟷᐳ(out _): {
        break;
    }
    case 1 when selᴛ17.ꟷᐳ(out _): {
        break;
    }}
}

internal static void unlock(this ref fakeNetListener li) {
    if (li.closed || len(li.queue) > 0){
        li.setc.ᐸꟷ(new EmptyStruct());
    } else {
        li.unsetc.ᐸꟷ(new EmptyStruct());
    }
}

internal static ж<fakeNetConn> connect(this ж<fakeNetListener> Ꮡli) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var li = ref Ꮡli.DerefOrNull();

        if (li.onDial != default!) {
            li.onDial();
        }
        li.@lock();
        ᒐd1 = true;
        var locAddr = netip.AddrPortFrom(netip.AddrFrom4(new byte[]{127, 0, 0, 1}.array()), li.locPort);
        li.locPort++;
        var (c0, c1) = fakeNetPipe(li.addr, locAddr);
        li.queue = append(li.queue, (net.Conn)(new http_test_package.fakeNetConnжConn(c0)));
        if (li.trackConns) {
            li.conns = append(li.conns, c0);
        }
        return c1;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡli.DerefOrNull().unlock(); ᒐ.Run(); }
}

internal static (net.Conn, error) Accept(this ж<fakeNetListener> Ꮡli) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var li = ref Ꮡli.DerefOrNull();

        ᐸꟷ(li.setc);
        ᒐd1 = true;
        if (li.closed) {
            return (default!, net.ErrClosed);
        }
        var c = li.queue[0];
        li.queue = li.queue[1..];
        return (c, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡli.DerefOrNull().unlock(); ᒐ.Run(); }
}

internal static error Close(this ж<fakeNetListener> Ꮡli) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var li = ref Ꮡli.DerefOrNull();

        li.@lock();
        ᒐd1 = true;
        li.closed = true;
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡli.DerefOrNull().unlock(); ᒐ.Run(); }
}

internal static netꓸAddr Addr(this ref fakeNetListener li) {
    return new net.TCPAddrжΔAddr(net.TCPAddrFromAddrPort(li.addr));
}

// fakeNetPipe creates an in-memory, full duplex network connection.
//
// Unlike net.Pipe, the connection is not synchronous.
// Writes are made to a buffer, and return immediately.
// By default, the buffer size is unlimited.
internal static (ж<fakeNetConn> r, ж<fakeNetConn> w) fakeNetPipe(netip.AddrPort s1ap, netip.AddrPort s2ap) {
    var s1addr = net.TCPAddrFromAddrPort(s1ap);
    var s2addr = net.TCPAddrFromAddrPort(s2ap);
    var s1 = newSynctestNetConnHalf(new net.TCPAddrжΔAddr(s1addr));
    var s2 = newSynctestNetConnHalf(new net.TCPAddrжΔAddr(s2addr));
    var c1 = Ꮡ(new fakeNetConn(loc: s1, rem: s2));
    var c2 = Ꮡ(new fakeNetConn(loc: s2, rem: s1));
    c1.Value.peer = c2;
    c2.Value.peer = c1;
    return (c1, c2);
}

// A fakeNetConn is one endpoint of the connection created by fakeNetPipe.
partial struct fakeNetConn {
    // local and remote connection halves.
    // Each half contains a buffer.
    // Reads pull from the local buffer, and writes push to the remote buffer.
    internal ж<fakeNetConnHalf> loc, rem;
    // When set, synctest.Wait is automatically called before reads and after writes.
    internal bool autoWait;
    // peer is the other endpoint.
    internal ж<fakeNetConn> peer;
    internal Action onClose; // called when closing
}

// Read reads data from the connection.
internal static (nint n, error err) Read(this ref fakeNetConn c, slice<byte> b) {
    if (c.autoWait) {
        synctest.Wait();
    }
    return c.loc.read(b);
}

// Peek returns the available unread read buffer,
// without consuming its contents.
internal static slice<byte> Peek(this ref fakeNetConn c) {
    if (c.autoWait) {
        synctest.Wait();
    }
    return c.loc.peek();
}

// Write writes data to the connection.
internal static (nint n, error err) Write(this ж<fakeNetConn> Ꮡc, slice<byte> b) {
    nint n = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        if (c.autoWait) {
            defer(synctest.Wait, ref ᒐ);
        }
        (n, err) = c.rem.write(b);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (n, err);
}

// IsClosed reports whether the peer has closed its end of the connection.
internal static bool IsClosedByPeer(this ж<fakeNetConn> Ꮡc) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        if (c.autoWait) {
            synctest.Wait();
        }
        c.rem.@lock();
        ᒐd1 = true;
        // If the remote half of the conn is returning ErrClosed,
        // the peer has closed the connection.
        return AreEqual((~c.rem).readErr, net.ErrClosed);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡc.DerefOrNull().rem.unlock(); ᒐ.Run(); }
}

// Close closes the connection.
internal static error Close(this ref fakeNetConn c) {
    if (c.onClose != default!) {
        c.onClose();
    }
    // Local half of the conn is now closed.
    c.loc.@lock();
    c.loc.Value.writeErr = net.ErrClosed;
    c.loc.Value.readErr = net.ErrClosed;
    c.loc.of(fakeNetConnHalf.Ꮡbuf).Reset();
    c.loc.unlock();
    // Remote half of the connection reads EOF after reading any remaining data.
    c.rem.@lock();
    if ((~c.rem).readErr != default!) {
        c.rem.Value.readErr = io.EOF;
    }
    c.rem.unlock();
    if (c.autoWait) {
        synctest.Wait();
    }
    return default!;
}

// LocalAddr returns the (fake) local network address.
internal static netꓸAddr LocalAddr(this ref fakeNetConn c) {
    return (~c.loc).addr;
}

// LocalAddr returns the (fake) remote network address.
internal static netꓸAddr RemoteAddr(this ref fakeNetConn c) {
    return (~c.rem).addr;
}

// SetDeadline sets the read and write deadlines for the connection.
internal static error SetDeadline(this ref fakeNetConn c, time.Time t) {
    c.SetReadDeadline(t);
    c.SetWriteDeadline(t);
    return default!;
}

// SetReadDeadline sets the read deadline for the connection.
internal static error SetReadDeadline(this ref fakeNetConn c, time.Time t) {
    c.loc.of(fakeNetConnHalf.Ꮡrctx).setDeadline(t);
    return default!;
}

// SetWriteDeadline sets the write deadline for the connection.
internal static error SetWriteDeadline(this ref fakeNetConn c, time.Time t) {
    c.rem.of(fakeNetConnHalf.Ꮡwctx).setDeadline(t);
    return default!;
}

// SetReadBufferSize sets the read buffer limit for the connection.
// Writes by the peer will block so long as the buffer is full.
internal static void SetReadBufferSize(this ref fakeNetConn c, nint size) {
    c.loc.setReadBufferSize(size);
}

// fakeNetConnHalf is one data flow in the connection created by fakeNetPipe.
// Each half contains a buffer. Writes to the half push to the buffer, and reads pull from it.
partial struct fakeNetConnHalf {
    internal netꓸAddr addr;
    // Read and write timeouts.
    internal deadlineContext rctx, wctx;
    // A half can be readable and/or writable.
    //
    // These four channels act as a lock,
    // and allow waiting for readability/writability.
    // When the half is unlocked, exactly one channel contains a value.
    // When the half is locked, all channels are empty.
    internal channel<EmptyStruct> lockr; // readable
    internal channel<EmptyStruct> lockw; // writable
    internal channel<EmptyStruct> lockrw; // readable and writable
    internal channel<EmptyStruct> lockc; // neither readable nor writable
    internal nint bufMax; // maximum buffer size
    internal bytes.Buffer buf;
    internal error readErr; // error returned by reads
    internal error writeErr; // error returned by writes
}

internal static ж<fakeNetConnHalf> newSynctestNetConnHalf(netꓸAddr addr) {
    var h = Ꮡ(new fakeNetConnHalf(
        addr: addr,
        lockw: new channel<EmptyStruct>(1),
        lockr: new channel<EmptyStruct>(1),
        lockrw: new channel<EmptyStruct>(1),
        lockc: new channel<EmptyStruct>(1),
        bufMax: math.MaxInt
    ));
    // unlimited
    h.unlock();
    return h;
}

// lock locks h.
internal static void @lock(this ref fakeNetConnHalf h) {
    var selᴛ18 = h.lockw;
    var selᴛ19 = h.lockr;
    var selᴛ20 = h.lockrw;
    var selᴛ21 = h.lockc;
    switch (select(ᐸꟷ(selᴛ18, ꓸꓸꓸ), ᐸꟷ(selᴛ19, ꓸꓸꓸ), ᐸꟷ(selᴛ20, ꓸꓸꓸ), ᐸꟷ(selᴛ21, ꓸꓸꓸ))) {
    case 0 when selᴛ18.ꟷᐳ(out _): {
        break;
    }
    case 1 when selᴛ19.ꟷᐳ(out _): {
        break;
    }
    case 2 when selᴛ20.ꟷᐳ(out _): {
        break;
    }
    case 3 when selᴛ21.ꟷᐳ(out _): {
        break;
    }}
}

// writable
// readable
// readable and writable
// neither readable nor writable

// h unlocks h.
internal static void unlock(this ref fakeNetConnHalf h) {
    var canRead = h.readErr != default! || h.buf.Len() > 0;
    var canWrite = h.writeErr != default! || h.bufMax > h.buf.Len();
    switch (ᐧ) {
    case {} when canRead && canWrite: {
        h.lockrw.ᐸꟷ(new EmptyStruct()); // readable and writable
        break;
    }
    case {} when canRead: {
        h.lockr.ᐸꟷ(new EmptyStruct()); // readable
        break;
    }
    case {} when canWrite: {
        h.lockw.ᐸꟷ(new EmptyStruct()); // writable
        break;
    }
    default: {
        h.lockc.ᐸꟷ(new EmptyStruct()); // neither readable nor writable
        break;
    }}

}

// waitAndLockForRead waits until h is readable and locks it.
internal static error waitAndLockForRead(this ж<fakeNetConnHalf> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    // First a non-blocking select to see if we can make immediate progress.
    // This permits using a canceled context for a non-blocking operation.
    var selᴛ22 = h.lockr;
    var selᴛ23 = h.lockrw;
    switch (trySelect(ᐸꟷ(selᴛ22, ꓸꓸꓸ), ᐸꟷ(selᴛ23, ꓸꓸꓸ))) {
    case 0 when selᴛ22.ꟷᐳ(out _): {
        return default!; // readable
    }
    case 1 when selᴛ23.ꟷᐳ(out _): {
        return default!; // readable and writable
    }
    default: {
        break;
    }}
    var ctx = Ꮡh.of(fakeNetConnHalf.Ꮡrctx).context();
    var selᴛ24 = h.lockr;
    var selᴛ25 = h.lockrw;
    var selᴛ26 = ctx.Done();
    switch (select(ᐸꟷ(selᴛ24, ꓸꓸꓸ), ᐸꟷ(selᴛ25, ꓸꓸꓸ), ᐸꟷ(selᴛ26, ꓸꓸꓸ))) {
    case 0 when selᴛ24.ꟷᐳ(out _): {
        return default!; // readable
    }
    case 1 when selᴛ25.ꟷᐳ(out _): {
        return default!; // readable and writable
    }
    case 2 when selᴛ26.ꟷᐳ(out _): {
        return context_package.Cause(ctx);
    }}
    return default!;
}

// waitAndLockForWrite waits until h is writable and locks it.
internal static error waitAndLockForWrite(this ж<fakeNetConnHalf> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    // First a non-blocking select to see if we can make immediate progress.
    // This permits using a canceled context for a non-blocking operation.
    var selᴛ27 = h.lockw;
    var selᴛ28 = h.lockrw;
    switch (trySelect(ᐸꟷ(selᴛ27, ꓸꓸꓸ), ᐸꟷ(selᴛ28, ꓸꓸꓸ))) {
    case 0 when selᴛ27.ꟷᐳ(out _): {
        return default!; // writable
    }
    case 1 when selᴛ28.ꟷᐳ(out _): {
        return default!; // readable and writable
    }
    default: {
        break;
    }}
    var ctx = Ꮡh.of(fakeNetConnHalf.Ꮡwctx).context();
    var selᴛ29 = h.lockw;
    var selᴛ30 = h.lockrw;
    var selᴛ31 = ctx.Done();
    switch (select(ᐸꟷ(selᴛ29, ꓸꓸꓸ), ᐸꟷ(selᴛ30, ꓸꓸꓸ), ᐸꟷ(selᴛ31, ꓸꓸꓸ))) {
    case 0 when selᴛ29.ꟷᐳ(out _): {
        return default!; // writable
    }
    case 1 when selᴛ30.ꟷᐳ(out _): {
        return default!; // readable and writable
    }
    case 2 when selᴛ31.ꟷᐳ(out _): {
        return context_package.Cause(ctx);
    }}
    return default!;
}

internal static slice<byte> peek(this ж<fakeNetConnHalf> Ꮡh) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var h = ref Ꮡh.DerefOrNull();

        h.@lock();
        ᒐd1 = true;
        return h.buf.Bytes();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡh.DerefOrNull().unlock(); ᒐ.Run(); }
}

internal static (nint n, error err) read(this ж<fakeNetConnHalf> Ꮡh, slice<byte> b) {
    nint n = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var h = ref Ꮡh.DerefOrNull();

        {
            var errΔ1 = Ꮡh.waitAndLockForRead(); if (errΔ1 != default!) {
                (n, err) = (0, errΔ1); goto ᒐdone;
            }
        }
        defer(Ꮡh.unlock, ref ᒐ);
        if (h.buf.Len() == 0 && h.readErr != default!) {
            (n, err) = (0, h.readErr); goto ᒐdone;
        }
        (n, err) = h.buf.Read(b);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (n, err);
}

internal static void setReadBufferSize(this ж<fakeNetConnHalf> Ꮡh, nint size) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var h = ref Ꮡh.DerefOrNull();

        h.@lock();
        ᒐd1 = true;
        h.bufMax = size;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡh.DerefOrNull().unlock(); ᒐ.Run(); }
}

internal static (nint n, error err) write(this ж<fakeNetConnHalf> Ꮡh, slice<byte> b) {
    nint n = default!;

    while (n < len(b)) {
        var (nn, errΔ1) = Ꮡh.writePartial(b.slice(n));
        n += nn;
        if (errΔ1 != default!) {
            return (n, errΔ1);
        }
    }
    return (n, default!);
}

internal static (nint n, error err) writePartial(this ж<fakeNetConnHalf> Ꮡh, slice<byte> b) {
    nint n = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var h = ref Ꮡh.DerefOrNull();

        {
            var errΔ1 = Ꮡh.waitAndLockForWrite(); if (errΔ1 != default!) {
                (n, err) = (0, errΔ1); goto ᒐdone;
            }
        }
        defer(Ꮡh.unlock, ref ᒐ);
        if (h.writeErr != default!) {
            (n, err) = (0, h.writeErr); goto ᒐdone;
        }
        nint writeMax = h.bufMax - h.buf.Len();
        if (writeMax < len(b)) {
            b = b.slice(0, writeMax);
        }
        (n, err) = h.buf.Write(b);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (n, err);
}

// deadlineContext converts a changable deadline (as in net.Conn.SetDeadline) into a Context.
partial struct deadlineContext {
    internal sync.Mutex mu;
    internal context.Context ctx;
    internal Action<error> cancel;
    internal ж<time.Timer> timer;
}

// context returns a Context which expires when the deadline does.
internal static context.Context context(this ж<deadlineContext> Ꮡt) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        t.mu.Lock();
        ᒐd1 = true;
        if (t.ctx == default!) {
            (t.ctx, t.cancel) = context_package.WithCancelCause(context_package.Background());
        }
        return t.ctx;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡt.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

// setDeadline sets the current deadline.
internal static void setDeadline(this ж<deadlineContext> Ꮡt, time.Time deadline) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        t.mu.Lock();
        defer(Ꮡt.of(deadlineContext.Ꮡmu).Unlock, ref ᒐ);
        // If t.ctx is non-nil and t.cancel is nil, then t.ctx was canceled
        // and we should create a new one.
        if (t.ctx == default! || t.cancel == default!) {
            (t.ctx, t.cancel) = context_package.WithCancelCause(context_package.Background());
        }
        // Stop any existing deadline from expiring.
        if (t.timer != nil) {
            t.timer.Stop();
        }
        if (deadline.IsZero()) {
            // No deadline.
            return;
        }
        var now = time.Now();
        if (!deadline.After(now)) {
            // Deadline has already expired.
            t.cancel(os.ErrDeadlineExceeded);
            t.cancel = default!;
            return;
        }
        if (t.timer != nil) {
            // Reuse existing deadline timer.
            t.timer.Reset(deadline.Sub(now));
            return;
        }
        // Create a new timer to cancel the context at the deadline.
        t.timer = time.AfterFunc(deadline.Sub(now), () => {
            GoFrame ᒐ = default;
            try {
                Ꮡt.Value.mu.Lock();
                defer(Ꮡt.of(deadlineContext.Ꮡmu).Unlock, ref ᒐ);
                Ꮡt.Value.cancel(os.ErrDeadlineExceeded);
                Ꮡt.Value.cancel = default!;
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end http_test_package
