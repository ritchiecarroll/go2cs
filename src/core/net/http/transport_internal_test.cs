// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// White-box tests for transport.go (in package http instead of http_test).
namespace go.net;

using bytes = bytes_package;
using context = context_package;
using tls = crypto.tls_package;
using errors = errors_package;
using io = io_package;
using net = net_package;
using testcert = global::go.net.http.@internal.testcert_package;
using strings = strings_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using crypto;
using global::go.net.http.@internal;
using static global::go.net.http_package;
using time = time_package;

partial class http_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testOverˢ => ref testOverˢᵇ;
private static readonly @string testOverˢᵇ = "test over"u8;

// Issue 15446: incorrect wrapping of errors when server closes an idle connection.
public static partial void TestTransportPersistConnReadLoopEOF(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var ln = newLocalListener(t);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);

        var connc = new channel<net.Conn>(1);
        var conncʗ1 = connc;
        var lnʗ2 = ln;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => builtin.close(ᴛ1), conncʗ1, ref ᒐ);
                var (c, errΔ1) = lnʗ2.Accept();
                if (errΔ1 != nil) {
                    t.Error(errΔ1);
                    return;
                }
                conncʗ1.ᐸꟷ(c);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });

        var tr = @new<global::go.net.http_package.Transport>();
        var (req, _) = NewRequest(getˢ, "http://"u8 + ln.Addr().String(), default);
        req = req.WithT(t);
        var (ctx, cancel) = context_package.WithCancelCause(context_package.Background());
        var treq = Ꮡ(new transportRequest(Request: req, ctx: ctx, cancel: cancel));
        var cm = new connectMethod(targetScheme: "http", targetAddr: ln.Addr().String());
        var (pc, err) = tr.getConn(treq, cm);
        if (err != nil) {
            t.Fatal(err);
        }
        var pcʗ1 = pc;
        defer(pcʗ1.close, errors.New(testOverˢ), ref ᒐ);

        var conn = ᐸꟷ(connc);
        if (conn == nil) {
            // Already called t.Error in the accept goroutine.
            return;
        }
        conn.Close(); // simulate the server hanging up on the client

        (_, err) = pc.roundTrip(treq);
        if (!isNothingWrittenError(err) && !isTransportReadFromServerError(err) && !AreEqual(err, errServerClosedIdle)) {
            t.Errorf("roundTrip = %#v, %v; want errServerClosedIdle, transportReadFromServerError, or nothingWrittenError", err, err);
        }

        ᐸꟷ(pc.closech);
        err = pc.closed;
        if (!isNothingWrittenError(err) && !isTransportReadFromServerError(err) && !AreEqual(err, errServerClosedIdle)) {
            t.Errorf("pc.closed = %#v, %v; want errServerClosedIdle or transportReadFromServerError, or nothingWrittenError", err, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static bool isNothingWrittenError(error err) {
    var (_, ok) = err._<nothingWrittenError>(ᐧ);
    return ok;
}

internal static bool isTransportReadFromServerError(error err) {
    var (_, ok) = err._<transportReadFromServerError>(ᐧ);
    return ok;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string tcp6ˢ => ref tcp6ˢᵇ;
private static readonly @string tcp6ˢᵇ = "tcp6"u8;

internal static net.Listener newLocalListener(ж<testing.T> t) {
    var (ln, err) = net.Listen(tcpˢ, "127.0.0.1:0");
    if (err != nil) {
        (ln, err) = net.Listen(tcp6ˢ, "[::1]:0");
    }
    if (err != nil) {
        t.Fatal(err);
    }
    return ln;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpFakeTldˢ => ref httpFakeTldˢᵇ;
private static readonly @string httpFakeTldˢᵇ = "http://fake.tld/"u8;

internal static ж<global::go.net.http_package.Request> dummyRequest(@string method) {
    var (req, err) = NewRequest(method, httpFakeTldˢ, default);
    if (err != nil) {
        throw panic(err);
    }
    return req;
}

internal static ж<global::go.net.http_package.Request> dummyRequestWithBody(@string method) {
    var (req, err) = NewRequest(method, httpFakeTldˢ, new http_test_package.strings_ReaderжReader(strings.NewReader(fooˢ)));
    if (err != nil) {
        throw panic(err);
    }
    return req;
}

internal static ж<global::go.net.http_package.Request> dummyRequestWithBodyNoGetBody(@string method) {
    var req = dummyRequestWithBody(method);
    req.GetBody = default;
    return req;
}

// issue22091Error acts like a golang.org/x/net/http2.ErrNoCachedConn.
internal partial struct issue22091Error {
}

internal static void IsHTTP2NoCachedConnError(this issue22091Error _) {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string issue22091Errorˢ => ref issue22091Errorˢᵇ;
private static readonly @string issue22091Errorˢᵇ = "issue22091Error"u8;

internal static @string Error(this issue22091Error _) {
    return issue22091Errorˢ;
}

internal partial struct TestTransportShouldRetryRequest_tests /*dyn*/ {
    internal ж<global::go.net.http_package.persistConn> pc;
    internal ж<global::go.net.http_package.Request> req;
    internal error err;
    internal bool want;
}

public static void TestTransportShouldRetryRequest(ж<testing.T> t) {
    var tests = new slice<TestTransportShouldRetryRequest_tests>(10){
        [0] = new(
            pc: Ꮡ(new persistConn(reused: false)),
            req: dummyRequest(postˢ),
            err: new nothingWrittenError(),
            want: false
        ),
        [1] = new(
            pc: Ꮡ(new persistConn(reused: true)),
            req: dummyRequest(postˢ),
            err: new nothingWrittenError(),
            want: true
        ),
        [2] = new(
            pc: Ꮡ(new persistConn(reused: true)),
            req: dummyRequest(postˢ),
            err: http2ErrNoCachedConn,
            want: true
        ),
        [3] = new(
            pc: nil,
            req: nil,
            err: new issue22091Error(), // like an external http2ErrNoCachedConn
            want: true
        ),
        [4] = new(
            pc: Ꮡ(new persistConn(reused: true)),
            req: dummyRequest(postˢ),
            err: errMissingHost,
            want: false
        ),
        [5] = new(
            pc: Ꮡ(new persistConn(reused: true)),
            req: dummyRequest(postˢ),
            err: new transportReadFromServerError(),
            want: false
        ),
        [6] = new(
            pc: Ꮡ(new persistConn(reused: true)),
            req: dummyRequest(getˢ),
            err: new transportReadFromServerError(),
            want: true
        ),
        [7] = new(
            pc: Ꮡ(new persistConn(reused: true)),
            req: dummyRequest(getˢ),
            err: errServerClosedIdle,
            want: true
        ),
        [8] = new(
            pc: Ꮡ(new persistConn(reused: true)),
            req: dummyRequestWithBody(postˢ),
            err: new nothingWrittenError(),
            want: true
        ),
        [9] = new(
            pc: Ꮡ(new persistConn(reused: true)),
            req: dummyRequestWithBodyNoGetBody(postˢ),
            err: new nothingWrittenError(),
            want: false
        )
    };
    foreach (var (i, tt) in tests) {
        var got = tt.pc.shouldRetryRequest(tt.req, tt.err);
        if (got != tt.want) {
            t.Errorf("%d. shouldRetryRequest = %v; want %v", i, got, tt.want);
        }
    }
}

internal delegate (ж<global::go.net.http_package.Response>, error) roundTripFunc(ж<global::go.net.http_package.Request> r);

internal static partial (ж<global::go.net.http_package.Response>, error) RoundTrip(this roundTripFunc f, ж<global::go.net.http_package.Request> r) {
    return f(r);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpsExampleOrgˢ => ref httpsExampleOrgˢᵇ;
private static readonly @string httpsExampleOrgˢᵇ = "https://example.org/"u8;
internal static ref readonly @string requestˢ => ref requestˢᵇ;
private static readonly @string requestˢᵇ = "request"u8;
internal static readonly object bodyLengthIsZeroˢ = (@string)"body length is zero"u8;

// Issue 25009
public static partial void TestTransportBodyAltRewind(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        ref var cert = ref heap<tls.Certificate>(out var Ꮡcert);
        (cert, var err) = tls.X509KeyPair(testcert.LocalhostCert, testcert.LocalhostKey);
        if (err != nil) {
            t.Fatal(err);
        }
        var ln = newLocalListener(t);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);

        var certʗ1 = cert;
        var lnʗ2 = ln;
        goǃ(() => {
            var tln = tls.NewListener(lnʗ2, Ꮡ(new tls.Config(
                NextProtos: slice<@string>(["foo"]),
                Certificates: slice<tls.Certificate>([certʗ1])
            )));
            for (nint i = 0; i < 2; i++) {
                var (sc, errΔ1) = tln.Accept();
                if (errΔ1 != nil) {
                    t.Error(errΔ1);
                    return;
                }
                {
                    var errΔ2 = sc._<ж<tls.Conn>>().Handshake(); if (errΔ2 != nil) {
                        t.Error(errΔ2);
                        return;
                    }
                }
                sc.Close();
            }
        });

        @string addr = ln.Addr().String();
        var (req, _) = NewRequest(postˢ, httpsExampleOrgˢ, new http_test_package.bytes_BufferжReader(bytes.NewBufferString(requestˢ)));
        var roundTripped = false;

        var tr = Ꮡ(new Transport(
            DisableKeepAlives: true,
            TLSNextProto: new map<@string, Func<@string, ж<tls.Conn>, global::go.net.http_package.RoundTripper>>{
                ["foo"] = (@string authority, ж<tls.Conn> cΔ1) => new http_internal_test_package.roundTripFuncᴠRoundTripper(new roundTripFunc((ж<global::go.net.http_package.Request> r) => {
                        var (n, _) = io.Copy(io.Discard, r.Body);
                        if (n == 0) {
                            t.Error(bodyLengthIsZeroˢ);
                        }
                        if (roundTripped) {
                            return (Ꮡ(new Response(
                                Body: NoBody,
                                StatusCode: 200
                            )), default);
                        }
                        roundTripped = true;
                        return (default, new http2noCachedConnError());
                    }))
            },
            DialTLS: (@string _Δp0, @string _Δp1) => {
                var (tc, errΔ3) = tls.Dial(tcpˢ, addr, Ꮡ(new tls.Config(
                    InsecureSkipVerify: true,
                    NextProtos: slice<@string>(["foo"])
                )));
                if (errΔ3 != nil) {
                    return (default, errΔ3);
                }
                {
                    var errΔ4 = tc.Handshake(); if (errΔ4 != nil) {
                        return (default, errΔ4);
                    }
                }
                return (new tls.ConnжConn(tc), default);
            }
        ));
        var c = Ꮡ(new Client(Transport: new global::go.net.http_package.TransportжRoundTripper(tr)));
        (_, err) = c.Do(req);
        if (err != nil) {
            t.Error(err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end http_internal_test_package
