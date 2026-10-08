// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net.http;

using bufio = bufio_package;
using errors = errors_package;
using io = io_package;
using net = net_package;
using http = go.net.http_package;
using textproto = go.net.textproto_package;
using sync = sync_package;
using go.net;
using time = time_package;

partial class httputil_package {

public static ж<http.ProtocolError> ErrPersistEOF = Ꮡ(new http.ProtocolError(ErrorString: "persistent connection closed"u8));
public static ж<http.ProtocolError> ErrClosed = Ꮡ(new http.ProtocolError(ErrorString: "connection closed by user"u8));
public static ж<http.ProtocolError> ErrPipeline = Ꮡ(new http.ProtocolError(ErrorString: "pipeline error"u8));

// This is an API usage error - the local side is closed.
// ErrPersistEOF (above) reports that the remote side is closed.
internal static error errClosed = errors.New("i/o operation on closed connection"u8);

// ServerConn is an artifact of Go's early HTTP implementation.
// It is low-level, old, and unused by Go's current HTTP stack.
// We should have deleted it before Go 1.
//
// Deprecated: Use the Server in package [net/http] instead.
partial struct ServerConn {
    internal sync.Mutex mu; // read-write protects the following fields
    internal net.Conn c;
    internal ж<bufio.Reader> r;
    internal error re, we; // read/write errors
    internal io.ReadCloser lastbody;
    internal nint nread, nwritten;
    internal map<ж<http.Request>, nuint> pipereq;
    internal textproto.Pipeline pipe;
}

// NewServerConn is an artifact of Go's early HTTP implementation.
// It is low-level, old, and unused by Go's current HTTP stack.
// We should have deleted it before Go 1.
//
// Deprecated: Use the Server in package [net/http] instead.
public static ж<ServerConn> NewServerConn(net.Conn c, ж<bufio.Reader> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    if (Ꮡr == nil) {
        Ꮡr = bufio.NewReader(new net_ConnᴠReader(c)); r = ref Ꮡr.DerefOrNull();
    }
    return Ꮡ(new ServerConn(c: c, r: Ꮡr, pipereq: new map<ж<http.Request>, nuint>()));
}

// Hijack detaches the [ServerConn] and returns the underlying connection as well
// as the read-side bufio which may have some left over data. Hijack may be
// called before Read has signaled the end of the keep-alive logic. The user
// should not call Hijack while [ServerConn.Read] or [ServerConn.Write] is in progress.
public static (net.Conn, ж<bufio.Reader>) Hijack(this ж<ServerConn> Ꮡsc) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var sc = ref Ꮡsc.DerefOrNull();

        sc.mu.Lock();
        ᒐd1 = true;
        var c = sc.c;
        var r = sc.r;
        sc.c = default!;
        sc.r = default!;
        return (c, r);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡsc.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

// Close calls [ServerConn.Hijack] and then also closes the underlying connection.
public static error Close(this ж<ServerConn> Ꮡsc) {
    var (c, _) = Ꮡsc.Hijack();
    if (c != default!) {
        return c.Close();
    }
    return default!;
}

// Read returns the next request on the wire. An [ErrPersistEOF] is returned if
// it is gracefully determined that there are no more requests (e.g. after the
// first request on an HTTP/1.0 connection, or after a Connection:close on a
// HTTP/1.1 connection).
public static (ж<http.Request>, error) Read(this ж<ServerConn> Ꮡsc) {
    GoFrame ᒐ = default;
    try {
        ref var sc = ref Ꮡsc.DerefOrNull();

        ref var req = ref heap<ж<http.Request>>(out var Ꮡreq);
        error err = default!;
        // Ensure ordered execution of Reads and Writes
        nuint id = Ꮡsc.of(ServerConn.Ꮡpipe).Next();
        Ꮡsc.of(ServerConn.Ꮡpipe).StartRequest(id);
        defer(() => {
            Ꮡsc.of(ServerConn.Ꮡpipe).EndRequest(id);
            if (Ꮡreq.ValueSlot == nil){
                Ꮡsc.of(ServerConn.Ꮡpipe).StartResponse(id);
                Ꮡsc.of(ServerConn.Ꮡpipe).EndResponse(id);
            } else {
                // Remember the pipeline id of this request
                Ꮡsc.Value.mu.Lock();
                Ꮡsc.Value.pipereq[Ꮡreq.ValueSlot] = id;
                Ꮡsc.Value.mu.Unlock();
            }
        }, ref ᒐ);
        sc.mu.Lock();
        if (sc.we != default!) {
            // no point receiving if write-side broken or closed
            defer(Ꮡsc.of(ServerConn.Ꮡmu).Unlock, ref ᒐ);
            return (default!, sc.we);
        }
        if (sc.re != default!) {
            defer(Ꮡsc.of(ServerConn.Ꮡmu).Unlock, ref ᒐ);
            return (default!, sc.re);
        }
        if (sc.r == nil) {
            // connection closed by user in the meantime
            defer(Ꮡsc.of(ServerConn.Ꮡmu).Unlock, ref ᒐ);
            return (default!, errClosed);
        }
        var r = sc.r;
        var lastbody = sc.lastbody;
        sc.lastbody = default!;
        sc.mu.Unlock();
        // Make sure body is fully consumed, even if user does not call body.Close
        if (lastbody != default!) {
            // body.Close is assumed to be idempotent and multiple calls to
            // it should return the error that its first invocation
            // returned.
            err = lastbody.Close();
            if (err != default!) {
                sc.mu.Lock();
                defer(Ꮡsc.of(ServerConn.Ꮡmu).Unlock, ref ᒐ);
                sc.re = err;
                return (default!, err);
            }
        }
        (req, err) = http.ReadRequest(r);
        sc.mu.Lock();
        defer(Ꮡsc.of(ServerConn.Ꮡmu).Unlock, ref ᒐ);
        if (err != default!) {
            if (AreEqual(err, io.ErrUnexpectedEOF)){
                // A close from the opposing client is treated as a
                // graceful close, even if there was some unparse-able
                // data before the close.
                sc.re = new http.ProtocolErrorжerror(ErrPersistEOF);
                return (default!, sc.re);
            } else {
                sc.re = err;
                return (req, err);
            }
        }
        sc.lastbody = req.Value.Body;
        sc.nread++;
        if ((~req).Close) {
            sc.re = new http.ProtocolErrorжerror(ErrPersistEOF);
            return (req, sc.re);
        }
        return (req, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Pending returns the number of unanswered requests
// that have been received on the connection.
public static nint Pending(this ж<ServerConn> Ꮡsc) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var sc = ref Ꮡsc.DerefOrNull();

        sc.mu.Lock();
        ᒐd1 = true;
        return sc.nread - sc.nwritten;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡsc.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string persistServerPipeCountˢ = "persist server pipe count"u8;

// Write writes resp in response to req. To close the connection gracefully, set the
// Response.Close field to true. Write should be considered operational until
// it returns an error, regardless of any errors returned on the [ServerConn.Read] side.
public static error Write(this ж<ServerConn> Ꮡsc, ж<http.Request> Ꮡreq, ж<http.Response> Ꮡresp) {
    GoFrame ᒐ = default;
    try {
        ref var sc = ref Ꮡsc.DerefOrNull();
        ref var resp = ref Ꮡresp.DerefOrNull();

        // Retrieve the pipeline ID of this request/response pair
        sc.mu.Lock();
        var (id, ok) = sc.pipereq[Ꮡreq, ꟷ];
        delete(sc.pipereq, Ꮡreq);
        if (!ok) {
            sc.mu.Unlock();
            return new http.ProtocolErrorжerror(ErrPipeline);
        }
        sc.mu.Unlock();
        // Ensure pipeline order
        Ꮡsc.of(ServerConn.Ꮡpipe).StartResponse(id);
        defer(Ꮡsc.of(ServerConn.Ꮡpipe).EndResponse, id, ref ᒐ);
        sc.mu.Lock();
        if (sc.we != default!) {
            defer(Ꮡsc.of(ServerConn.Ꮡmu).Unlock, ref ᒐ);
            return sc.we;
        }
        if (sc.c == default!) {
            // connection closed by user in the meantime
            defer(Ꮡsc.of(ServerConn.Ꮡmu).Unlock, ref ᒐ);
            return new http.ProtocolErrorжerror(ErrClosed);
        }
        var c = sc.c;
        if (sc.nread <= sc.nwritten) {
            defer(Ꮡsc.of(ServerConn.Ꮡmu).Unlock, ref ᒐ);
            return errors.New(persistServerPipeCountˢ);
        }
        if (resp.Close) {
            // After signaling a keep-alive close, any pipelined unread
            // requests will be lost. It is up to the user to drain them
            // before signaling.
            sc.re = new http.ProtocolErrorжerror(ErrPersistEOF);
        }
        sc.mu.Unlock();
        var err = resp.Write(new net_ConnᴠWriter(c));
        sc.mu.Lock();
        defer(Ꮡsc.of(ServerConn.Ꮡmu).Unlock, ref ᒐ);
        if (err != default!) {
            sc.we = err;
            return err;
        }
        sc.nwritten++;
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// ClientConn is an artifact of Go's early HTTP implementation.
// It is low-level, old, and unused by Go's current HTTP stack.
// We should have deleted it before Go 1.
//
// Deprecated: Use Client or Transport in package [net/http] instead.
partial struct ClientConn {
    internal sync.Mutex mu; // read-write protects the following fields
    internal net.Conn c;
    internal ж<bufio.Reader> r;
    internal error re, we; // read/write errors
    internal io.ReadCloser lastbody;
    internal nint nread, nwritten;
    internal map<ж<http.Request>, nuint> pipereq;
    internal textproto.Pipeline pipe;
    internal Func<ж<http.Request>, io.Writer, error> writeReq;
}

// NewClientConn is an artifact of Go's early HTTP implementation.
// It is low-level, old, and unused by Go's current HTTP stack.
// We should have deleted it before Go 1.
//
// Deprecated: Use the Client or Transport in package [net/http] instead.
public static ж<ClientConn> NewClientConn(net.Conn c, ж<bufio.Reader> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    if (Ꮡr == nil) {
        Ꮡr = bufio.NewReader(new net_ConnᴠReader(c)); r = ref Ꮡr.DerefOrNull();
    }
    return Ꮡ(new ClientConn(
        c: c,
        r: Ꮡr,
        pipereq: new map<ж<http.Request>, nuint>(),
        writeReq: ((Func<ж<http.Request>, io.Writer, error>)(http.Write))
    ));
}

// NewProxyClientConn is an artifact of Go's early HTTP implementation.
// It is low-level, old, and unused by Go's current HTTP stack.
// We should have deleted it before Go 1.
//
// Deprecated: Use the Client or Transport in package [net/http] instead.
public static ж<ClientConn> NewProxyClientConn(net.Conn c, ж<bufio.Reader> Ꮡr) {
    var cc = NewClientConn(c, Ꮡr);
    cc.Value.writeReq = ((Func<ж<http.Request>, io.Writer, error>)(http.WriteProxy));
    return cc;
}

// Hijack detaches the [ClientConn] and returns the underlying connection as well
// as the read-side bufio which may have some left over data. Hijack may be
// called before the user or Read have signaled the end of the keep-alive
// logic. The user should not call Hijack while [ClientConn.Read] or ClientConn.Write is in progress.
public static (net.Conn c, ж<bufio.Reader> r) Hijack(this ж<ClientConn> Ꮡcc) {
    net.Conn c = default!;
    ж<bufio.Reader> r = default!;
    GoFrame ᒐ = default;
    try {
        ref var cc = ref Ꮡcc.DerefOrNull();

        cc.mu.Lock();
        defer(Ꮡcc.of(ClientConn.Ꮡmu).Unlock, ref ᒐ);
        c = cc.c;
        r = cc.r;
        cc.c = default!;
        cc.r = default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (c, r);
}

// Close calls [ClientConn.Hijack] and then also closes the underlying connection.
public static error Close(this ж<ClientConn> Ꮡcc) {
    var (c, _) = Ꮡcc.Hijack();
    if (c != default!) {
        return c.Close();
    }
    return default!;
}

// Write writes a request. An [ErrPersistEOF] error is returned if the connection
// has been closed in an HTTP keep-alive sense. If req.Close equals true, the
// keep-alive connection is logically closed after this request and the opposing
// server is informed. An ErrUnexpectedEOF indicates the remote closed the
// underlying TCP connection, which is usually considered as graceful close.
public static error Write(this ж<ClientConn> Ꮡcc, ж<http.Request> Ꮡreq) {
    GoFrame ᒐ = default;
    try {
        ref var cc = ref Ꮡcc.DerefOrNull();
        ref var req = ref Ꮡreq.DerefOrNull();

        ref var err = ref heap<error>(out var Ꮡerr);
        // Ensure ordered execution of Writes
        nuint id = Ꮡcc.of(ClientConn.Ꮡpipe).Next();
        Ꮡcc.of(ClientConn.Ꮡpipe).StartRequest(id);
        defer(() => {
            Ꮡcc.of(ClientConn.Ꮡpipe).EndRequest(id);
            if (Ꮡerr.ValueSlot != default!){
                Ꮡcc.of(ClientConn.Ꮡpipe).StartResponse(id);
                Ꮡcc.of(ClientConn.Ꮡpipe).EndResponse(id);
            } else {
                // Remember the pipeline id of this request
                Ꮡcc.Value.mu.Lock();
                Ꮡcc.Value.pipereq[Ꮡreq] = id;
                Ꮡcc.Value.mu.Unlock();
            }
        }, ref ᒐ);
        cc.mu.Lock();
        if (cc.re != default!) {
            // no point sending if read-side closed or broken
            defer(Ꮡcc.of(ClientConn.Ꮡmu).Unlock, ref ᒐ);
            return cc.re;
        }
        if (cc.we != default!) {
            defer(Ꮡcc.of(ClientConn.Ꮡmu).Unlock, ref ᒐ);
            return cc.we;
        }
        if (cc.c == default!) {
            // connection closed by user in the meantime
            defer(Ꮡcc.of(ClientConn.Ꮡmu).Unlock, ref ᒐ);
            return errClosed;
        }
        var c = cc.c;
        if (req.Close) {
            // We write the EOF to the write-side error, because there
            // still might be some pipelined reads
            cc.we = new http.ProtocolErrorжerror(ErrPersistEOF);
        }
        cc.mu.Unlock();
        err = cc.writeReq(Ꮡreq, new net_ConnᴠWriter(c));
        cc.mu.Lock();
        defer(Ꮡcc.of(ClientConn.Ꮡmu).Unlock, ref ᒐ);
        if (err != default!) {
            cc.we = err;
            return err;
        }
        cc.nwritten++;
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Pending returns the number of unanswered requests
// that have been sent on the connection.
public static nint Pending(this ж<ClientConn> Ꮡcc) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var cc = ref Ꮡcc.DerefOrNull();

        cc.mu.Lock();
        ᒐd1 = true;
        return cc.nwritten - cc.nread;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡcc.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

// Read reads the next response from the wire. A valid response might be
// returned together with an [ErrPersistEOF], which means that the remote
// requested that this be the last request serviced. Read can be called
// concurrently with [ClientConn.Write], but not with another Read.
public static (ж<http.Response> resp, error err) Read(this ж<ClientConn> Ꮡcc, ж<http.Request> Ꮡreq) {
    ж<http.Response> resp = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var cc = ref Ꮡcc.DerefOrNull();

        // Retrieve the pipeline ID of this request/response pair
        cc.mu.Lock();
        var (id, ok) = cc.pipereq[Ꮡreq, ꟷ];
        delete(cc.pipereq, Ꮡreq);
        if (!ok) {
            cc.mu.Unlock();
            (resp, err) = (default!, new http.ProtocolErrorжerror(ErrPipeline)); goto ᒐdone;
        }
        cc.mu.Unlock();
        // Ensure pipeline order
        Ꮡcc.of(ClientConn.Ꮡpipe).StartResponse(id);
        defer(Ꮡcc.of(ClientConn.Ꮡpipe).EndResponse, id, ref ᒐ);
        cc.mu.Lock();
        if (cc.re != default!) {
            defer(Ꮡcc.of(ClientConn.Ꮡmu).Unlock, ref ᒐ);
            (resp, err) = (default!, cc.re); goto ᒐdone;
        }
        if (cc.r == nil) {
            // connection closed by user in the meantime
            defer(Ꮡcc.of(ClientConn.Ꮡmu).Unlock, ref ᒐ);
            (resp, err) = (default!, errClosed); goto ᒐdone;
        }
        var r = cc.r;
        var lastbody = cc.lastbody;
        cc.lastbody = default!;
        cc.mu.Unlock();
        // Make sure body is fully consumed, even if user does not call body.Close
        if (lastbody != default!) {
            // body.Close is assumed to be idempotent and multiple calls to
            // it should return the error that its first invocation
            // returned.
            err = lastbody.Close();
            if (err != default!) {
                cc.mu.Lock();
                defer(Ꮡcc.of(ClientConn.Ꮡmu).Unlock, ref ᒐ);
                cc.re = err;
                (resp, err) = (default!, err); goto ᒐdone;
            }
        }
        (resp, err) = http.ReadResponse(r, Ꮡreq);
        cc.mu.Lock();
        defer(Ꮡcc.of(ClientConn.Ꮡmu).Unlock, ref ᒐ);
        if (err != default!) {
            cc.re = err;
            goto ᒐdone;
        }
        cc.lastbody = resp.Value.Body;
        cc.nread++;
        if ((~resp).Close) {
            cc.re = new http.ProtocolErrorжerror(ErrPersistEOF); // don't send any more requests
            (resp, err) = (resp, cc.re); goto ᒐdone;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (resp, err);
}

// Do is convenience method that writes a request and reads a response.
public static (ж<http.Response>, error) Do(this ж<ClientConn> Ꮡcc, ж<http.Request> Ꮡreq) {
    var err = Ꮡcc.Write(Ꮡreq);
    if (err != default!) {
        return (default!, err);
    }
    return Ꮡcc.Read(Ꮡreq);
}

} // end httputil_package
