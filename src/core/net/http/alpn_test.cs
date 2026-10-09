// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using bufio = bufio_package;
using bytes = bytes_package;
using tls = crypto.tls_package;
using x509 = crypto.x509_package;
using fmt = fmt_package;
using io = io_package;
using static global::go.net.http_package;
using httptest = global::go.net.http.httptest_package;
using strings = strings_package;
using testing = testing_package;
using crypto;
using global::go.net;
using global::go.net.http;
using net = net_package;
using static global::go.net.http_internal_test_package;
using Δhttp = global::go.net.http_package;

partial class http_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object requestWithNoRemoteAddrˢ = (@string)"request with no RemoteAddr"u8;
internal static readonly @string pathProtoˢ = "path=/,proto="u8;
internal static readonly @string tcpˢ = "tcp"u8;
internal static readonly @string pathFooProtoTls09ˢ = "path=/foo,proto=tls-0.9"u8;

public static void TestNextProtoUpgrade(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        setParallel(Ꮡt);
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        var ts = httptest.NewUnstartedServer(new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "path=%s,proto="u8, (~(~r).URL).Path);
            if ((~r).TLS != nil) {
                w.Write(slice<byte>((~(~r).TLS).NegotiatedProtocol));
            }
            if ((~r).RemoteAddr == ""u8) {
                Ꮡt.Error(requestWithNoRemoteAddrˢ);
            }
            if ((~r).Body == default!) {
                Ꮡt.Errorf("request with nil Body"u8);
            }
        })));
        ts.Value.TLS = Ꮡ(new tls.Config(
            NextProtos: new @string[]{"unhandled-proto"u8, "tls-0.9"u8}.slice()
        ));
        ts.Value.Config.Value.TLSNextProto = new map<@string, Action<ж<Δhttp.Server>, ж<tls.Conn>, httpꓸHandler>>{
            ["tls-0.9"u8] = handleTLSProtocol09
        };
        ts.StartTLS();
        var tsʗ1 = ts;
        defer(tsʗ1.Close, ref ᒐ);
        // Normal request, without NPN.
        {
            var c = ts.Client();
            var (res, err) = c.Get((~ts).URL);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            (var body, err) = io.ReadAll(new http_test_package.io_ReadCloserᴠReader((~res).Body));
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            {
                @string want = pathProtoˢ; if (((sstring)body) != want) {
                    Ꮡt.Errorf("plain request = %q; want %q"u8, body, want);
                }
            }
        }
        // Request to an advertised but unhandled NPN protocol.
        // Server will hang up.
        {
            var certPool = x509.NewCertPool();
            certPool.AddCert(ts.Certificate());
            var tr = Ꮡ(new Transport(
                TLSClientConfig: Ꮡ(new tls.Config(
                    RootCAs: certPool,
                    NextProtos: new @string[]{"unhandled-proto"u8}.slice()
                ))
            ));
            var trʗ1 = tr;
            defer(trʗ1.CloseIdleConnections, ref ᒐ);
            var c = Ꮡ(new Client(
                Transport: new Δhttp.TransportжRoundTripper(tr)
            ));
            var (res, err) = c.Get((~ts).URL);
            if (err == default!) {
                var resʗ1 = res;
                defer(() => (~resʗ1).Body.Close(), ref ᒐ);
                ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
                res.Write(new http_test_package.bytes_BufferжWriter(Ꮡbuf));
                Ꮡt.Errorf("expected error on unhandled-proto request; got: %s"u8, buf.Bytes());
            }
        }
        // Request using the "tls-0.9" protocol, which we register here.
        // It is HTTP/0.9 over TLS.
        {
            var c = ts.Client();
            var tlsConfig = (~c).Transport._<ж<Δhttp.Transport>>().Value.TLSClientConfig;
            tlsConfig.Value.NextProtos = new @string[]{"tls-0.9"u8}.slice();
            var (conn, err) = tls.Dial(tcpˢ, (~ts).Listener.Addr().String(), tlsConfig);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            conn.Write(slice<byte>("GET /foo\n"u8));
            (var body, err) = io.ReadAll(new http_test_package.tls_ConnжReader(conn));
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            {
                @string want = pathFooProtoTls09ˢ; if (((sstring)body) != want) {
                    Ꮡt.Errorf("plain request = %q; want %q"u8, body, want);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getˢ = "GET "u8;
internal static readonly @string getˢ2 = "GET"u8;
internal static readonly @string http09ˢ = "HTTP/0.9"u8;

// handleTLSProtocol09 implements the HTTP/0.9 protocol over TLS, for the
// TestNextProtoUpgrade test.
internal static void handleTLSProtocol09(ж<Δhttp.Server> Ꮡsrv, ж<tls.Conn> Ꮡconn, httpꓸHandler h) {
    var br = bufio.NewReader(new http_test_package.tls_ConnжReader(Ꮡconn));
    var (line, err) = br.ReadString((rune)'\n');
    if (err != default!) {
        return;
    }
    line = strings.TrimSpace(line);
    @string path = strings.TrimPrefix(line, getˢ);
    if (path == line) {
        return;
    }
    var (req, _) = NewRequest(getˢ2, path, default!);
    req.Value.Proto = http09ˢ;
    req.Value.ProtoMajor = 0;
    req.Value.ProtoMinor = 9;
    var rw = Ꮡ(new http09Writer(new http_test_package.tls_ConnжWriter(Ꮡconn), new httpꓸHeader(0)));
    h.ServeHTTP(new http_test_package.http09WriterжResponseWriter(rw), req);
}

partial struct http09Writer {
    /*embed*/ public io_package.Writer Writer;
    internal httpꓸHeader h;
}

internal static httpꓸHeader Header(this http09Writer w) {
    return w.h;
}

internal static void WriteHeader(this http09Writer w, nint _) {
}

// no headers

} // end http_test_package
