// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using context = context_package;
using fmt = fmt_package;
using io = io_package;
using log = log_package;
using Δhttp = global::go.net.http_package;
using os = os_package;
using signal = global::go.os.signal_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using bufio = bufio_package;
using global::go.net;
using global::go.os;
using net = net_package;
using static global::go.net.http_internal_test_package;

partial class http_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string hijackˢ => ref hijackˢᵇ;
private static readonly @string hijackˢᵇ = "/hijack"u8;
internal static ref readonly @string webserverDoesnTSupportˢ => ref webserverDoesnTSupportˢᵇ;
private static readonly @string webserverDoesnTSupportˢᵇ = "webserver doesn't support hijacking"u8;
internal static ref readonly @string nowWeReSpeakingRawTcpSayˢ => ref nowWeReSpeakingRawTcpSayˢᵇ;
private static readonly @string nowWeReSpeakingRawTcpSayˢᵇ = "Now we're speaking raw TCP. Say hi: "u8;

public static void ExampleHijacker() {
    Δhttp.HandleFunc(hijackˢ, (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        GoFrame ᒐ = default;
        try {
            var (hj, ok) = w._<Δhttp.Hijacker>(ᐧ);
            if (!ok) {
                Δhttp.Error(w, webserverDoesnTSupportˢ, Δhttp.StatusInternalServerError);
                return;
            }
            var (conn, bufrw, err) = hj.Hijack();
            if (err != nil) {
                Δhttp.Error(w, err.Error(), Δhttp.StatusInternalServerError);
                return;
            }
            // Don't forget to close the connection:
            var connʗ1 = conn;
            defer(() => connʗ1.Close(), ref ᒐ);
            bufrw.Value.Writer.Value.WriteString(nowWeReSpeakingRawTcpSayˢ);
            bufrw.Value.Writer.Value.Flush();
            (var s, err) = bufrw.Value.Reader.Value.ReadString((byte)'\n');
            if (err != nil) {
                log.Printf("error reading string: %v", err);
                return;
            }
            fmt.Fprintf(new http_test_package.bufio_ReadWriterжWriter(bufrw), "You said: %q\nBye.\n"u8, s);
            bufrw.Value.Writer.Value.Flush();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpWwwGoogleComRobotsˢ => ref httpWwwGoogleComRobotsˢᵇ;
private static readonly @string httpWwwGoogleComRobotsˢᵇ = "http://www.google.com/robots.txt"u8;

public static void ExampleGet() {
    var (res, err) = Δhttp.Get(httpWwwGoogleComRobotsˢ);
    if (err != nil) {
        log.Fatal(err);
    }
    (var body, err) = io.ReadAll(res.Body);
    res.Body.Close();
    if (res.StatusCode > 299) {
        log.Fatalf("Response failed with status code: %d and\nbody: %s\n", res.StatusCode, body);
    }
    if (err != nil) {
        log.Fatal(err);
    }
    fmt.Printf("%s"u8, body);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string usrShareDocˢ => ref usrShareDocˢᵇ;
private static readonly @string usrShareDocˢᵇ = "/usr/share/doc"u8;

public static void ExampleFileServer() {
    // Simple static webserver:
    log.Fatal(Δhttp.ListenAndServe(":8080", Δhttp.FileServer((Δhttp.Dir)(@string)usrShareDocˢ)));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string tmpfilesˢ => ref tmpfilesˢᵇ;
private static readonly @string tmpfilesˢᵇ = "/tmpfiles/"u8;
internal static ref readonly @string tmpˢ => ref tmpˢᵇ;
private static readonly @string tmpˢᵇ = "/tmp"u8;

public static void ExampleFileServer_stripPrefix() {
    // To serve a directory on disk (/tmp) under an alternate URL
    // path (/tmpfiles/), use StripPrefix to modify the request
    // URL's path before the FileServer sees it:
    Δhttp.Handle(tmpfilesˢ, Δhttp.StripPrefix(tmpfilesˢ, Δhttp.FileServer((Δhttp.Dir)(@string)tmpˢ)));
}

public static void ExampleStripPrefix() {
    // To serve a directory on disk (/tmp) under an alternate URL
    // path (/tmpfiles/), use StripPrefix to modify the request
    // URL's path before the FileServer sees it:
    Δhttp.Handle(tmpfilesˢ, Δhttp.StripPrefix(tmpfilesˢ, Δhttp.FileServer((Δhttp.Dir)(@string)tmpˢ)));
}

partial struct apiHandler {
}

internal static void ServeHTTP(this apiHandler _Δp0, Δhttp.ResponseWriter _Δp1, ж<Δhttp.Request> _Δp2) {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string apiˢ => ref apiˢᵇ;
private static readonly @string apiˢᵇ = "/api/"u8;

public static void ExampleServeMux_Handle() {
    var mux = Δhttp.NewServeMux();
    mux.Handle(apiˢ, new apiHandler());
    mux.HandleFunc("/", (Δhttp.ResponseWriter w, ж<Δhttp.Request> req) => {
        // The "/" pattern matches everything, so we need to check
        // that we're at the root here.
        if (req.URL.Path != "/"u8) {
            Δhttp.NotFound(w, req);
            return;
        }
        fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "Welcome to the home page!"u8);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string sendstrailersˢ => ref sendstrailersˢᵇ;
private static readonly @string sendstrailersˢᵇ = "/sendstrailers"u8;
internal static ref readonly @string atEnd1AtEnd2ˢ => ref atEnd1AtEnd2ˢᵇ;
private static readonly @string atEnd1AtEnd2ˢᵇ = "AtEnd1, AtEnd2"u8;
internal static ref readonly @string atEnd3ˢ => ref atEnd3ˢᵇ;
private static readonly @string atEnd3ˢᵇ = "AtEnd3"u8;
internal static ref readonly @string textPlainCharsetUtf8ˢ => ref textPlainCharsetUtf8ˢᵇ;
private static readonly @string textPlainCharsetUtf8ˢᵇ = "text/plain; charset=utf-8"u8;
internal static ref readonly @string atEnd1ˢ => ref atEnd1ˢᵇ;
private static readonly @string atEnd1ˢᵇ = "AtEnd1"u8;
internal static ref readonly @string value1ˢ => ref value1ˢᵇ;
private static readonly @string value1ˢᵇ = "value 1"u8;
internal static ref readonly @string thisHttpResponseHasBothˢ => ref thisHttpResponseHasBothˢᵇ;
private static readonly @string thisHttpResponseHasBothˢᵇ = "This HTTP response has both headers before this text and trailers at the end.\n"u8;
internal static ref readonly @string atEnd2ˢ => ref atEnd2ˢᵇ;
private static readonly @string atEnd2ˢᵇ = "AtEnd2"u8;
internal static ref readonly @string value2ˢ => ref value2ˢᵇ;
private static readonly @string value2ˢᵇ = "value 2"u8;
internal static ref readonly @string value3ˢ => ref value3ˢᵇ;
private static readonly @string value3ˢᵇ = "value 3"u8;

// HTTP Trailers are a set of key/value pairs like headers that come
// after the HTTP response, instead of before.
public static void ExampleResponseWriter_trailers() {
    var mux = Δhttp.NewServeMux();
    mux.HandleFunc(sendstrailersˢ, (Δhttp.ResponseWriter w, ж<Δhttp.Request> req) => {
        // Before any call to WriteHeader or Write, declare
        // the trailers you will set during the HTTP
        // response. These three headers are actually sent in
        // the trailer.
        w.Header().Set(trailerˢ, atEnd1AtEnd2ˢ);
        w.Header().Add(trailerˢ, atEnd3ˢ);

        w.Header().Set(contentTypeˢ, textPlainCharsetUtf8ˢ); // normal header
        w.WriteHeader(Δhttp.StatusOK);

        w.Header().Set(atEnd1ˢ, value1ˢ);
        io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), thisHttpResponseHasBothˢ);
        w.Header().Set(atEnd2ˢ, value2ˢ);
        w.Header().Set(atEnd3ˢ, value3ˢ); // These will appear as trailers.
    });
}

public static partial void ExampleServer_Shutdown() {
    ref var srv = ref heap(new Δhttp.Server(), out var Ꮡsrv);

    var idleConnsClosed = new channel<EmptyStruct>(0);
    var idleConnsClosedʗ1 = idleConnsClosed;
    goǃ(() => {
        var sigint = new channel<osꓸSignal>(1);
        signal.Notify(sigint.AsSendOnly(), os.Interrupt);
        ᐸꟷ(sigint);

        // We received an interrupt signal, shut down.
        {
            var err = Ꮡsrv.Shutdown(context_package.Background()); if (err != nil) {
                // Error from closing listeners, or context timeout:
                log.Printf("HTTP server Shutdown: %v", err);
            }
        }
        builtin.close(idleConnsClosedʗ1);
    });

    {
        var err = Ꮡsrv.ListenAndServe(); if (!AreEqual(err, Δhttp.ErrServerClosed)) {
            // Error starting or closing listener:
            log.Fatalf("HTTP server ListenAndServe: %v", err);
        }
    }

    ᐸꟷ(idleConnsClosed);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloTlsˢ => ref helloTlsˢᵇ;
private static readonly @string helloTlsˢᵇ = "Hello, TLS!\n"u8;
internal static ref readonly @string certPemˢ => ref certPemˢᵇ;
private static readonly @string certPemˢᵇ = "cert.pem"u8;
internal static ref readonly @string keyPemˢ => ref keyPemˢᵇ;
private static readonly @string keyPemˢᵇ = "key.pem"u8;

public static void ExampleListenAndServeTLS() {
    Δhttp.HandleFunc("/", (Δhttp.ResponseWriter w, ж<Δhttp.Request> req) => {
        io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), helloTlsˢ);
    });

    // One can use generate_cert.go in crypto/tls to generate cert.pem and key.pem.
    log.Printf("About to listen on 8443. Go to https://127.0.0.1:8443/");
    var err = Δhttp.ListenAndServeTLS(":8443", certPemˢ, keyPemˢ, default);
    log.Fatal(err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloWorldˢ2 => ref helloWorldˢ2ᵇ;
private static readonly @string helloWorldˢ2ᵇ = "Hello, world!\n"u8;
internal static ref readonly @string helloˢ4 => ref helloˢ4ᵇ;
private static readonly @string helloˢ4ᵇ = "/hello"u8;

public static void ExampleListenAndServe() {
    // Hello world, the web server

    var helloHandler = (Δhttp.ResponseWriter w, ж<Δhttp.Request> req) => {
        io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), helloWorldˢ2);
    };

    Δhttp.HandleFunc(helloˢ4, helloHandler);
    log.Fatal(Δhttp.ListenAndServe(":8080", default));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloFromAHandleFunc1ˢ => ref helloFromAHandleFunc1ˢᵇ;
private static readonly @string helloFromAHandleFunc1ˢᵇ = "Hello from a HandleFunc #1!\n"u8;
internal static ref readonly @string helloFromAHandleFunc2ˢ => ref helloFromAHandleFunc2ˢᵇ;
private static readonly @string helloFromAHandleFunc2ˢᵇ = "Hello from a HandleFunc #2!\n"u8;
internal static ref readonly @string endpointˢ => ref endpointˢᵇ;
private static readonly @string endpointˢᵇ = "/endpoint"u8;

public static void ExampleHandleFunc() {
    var h1 = (Δhttp.ResponseWriter w, ж<Δhttp.Request> _) => {
        io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), helloFromAHandleFunc1ˢ);
    };
    var h2 = (Δhttp.ResponseWriter w, ж<Δhttp.Request> _) => {
        io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), helloFromAHandleFunc2ˢ);
    };

    Δhttp.HandleFunc("/", h1);
    Δhttp.HandleFunc(endpointˢ, h2);

    log.Fatal(Δhttp.ListenAndServe(":8080", default));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object thisIsThePeopleHandlerˢ = (@string)"This is the people handler."u8;

internal static httpꓸHandler newPeopleHandler() {
    return new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        fmt.Fprintln(new http_test_package.http_ResponseWriterᴠWriter(w), thisIsThePeopleHandlerˢ);
    }));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string resourcesˢ => ref resourcesˢᵇ;
private static readonly @string resourcesˢᵇ = "/resources"u8;
internal static ref readonly @string resourcesPeopleˢ => ref resourcesPeopleˢᵇ;
private static readonly @string resourcesPeopleˢᵇ = "/resources/people/"u8;

public static void ExampleNotFoundHandler() {
    var mux = Δhttp.NewServeMux();

    // Create sample handler to returns 404
    mux.Handle(resourcesˢ, Δhttp.NotFoundHandler());

    // Create sample handler that returns 200
    mux.Handle(resourcesPeopleˢ, newPeopleHandler());

    log.Fatal(Δhttp.ListenAndServe(":8080", new Δhttp.ServeMuxжΔHandler(mux)));
}

public static void ExampleProtocols_http1() {
    ref var srv = ref heap<Δhttp.Server>(new Δhttp.Server(
        Addr: ":8443"
    ), out var Ꮡsrv);

    // Serve only HTTP/1.
    srv.Protocols = @new<Δhttp.Protocols>();
    srv.Protocols.SetHTTP1(true);

    log.Fatal(Ꮡsrv.ListenAndServeTLS(certPemˢ, keyPemˢ));
}

public static void ExampleProtocols_http1or2() {
    var t = Δhttp.DefaultTransport._<ж<Δhttp.Transport>>().Clone();

    // Use either HTTP/1 and HTTP/2.
    t.Protocols = @new<Δhttp.Protocols>();
    t.Protocols.SetHTTP1(true);
    t.Protocols.SetHTTP2(true);

    var cli = Ꮡ(new Δhttp.Client(Transport: new Δhttp.TransportжRoundTripper(t)));
    var (res, err) = cli.Get(httpWwwGoogleComRobotsˢ);
    if (err != nil) {
        log.Fatal(err);
    }
    res.Body.Close();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string requestAllowedˢ => ref requestAllowedˢᵇ;
private static readonly @string requestAllowedˢᵇ = "request allowed\n"u8;

public static void ExampleCrossOriginProtection() {
    var mux = Δhttp.NewServeMux();

    mux.HandleFunc(helloˢ4, (Δhttp.ResponseWriter w, ж<Δhttp.Request> req) => {
        io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), requestAllowedˢ);
    });

    ref var srv = ref heap<Δhttp.Server>(new Δhttp.Server(
        Addr: ":8080",
        ReadTimeout: (time.Duration)(15000000000L),
        WriteTimeout: (time.Duration)(15000000000L),
        // Use CrossOriginProtection.Handler to block all non-safe cross-origin
        // browser requests to mux.
        Handler: Δhttp.NewCrossOriginProtection().Handler(new Δhttp.ServeMuxжΔHandler(mux))
    ), out var Ꮡsrv);

    log.Fatal(Ꮡsrv.ListenAndServe());
}

} // end http_test_package
