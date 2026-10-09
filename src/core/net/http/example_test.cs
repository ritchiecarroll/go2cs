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
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using bufio = bufio_package;
using global::go.net;
using global::go.os;
using net = net_package;
using static global::go.net.http_internal_test_package;

partial class http_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string hijackˢ = "/hijack"u8;
internal static readonly @string webserverDoesnTSupportˢ = "webserver doesn't support hijacking"u8;
internal static readonly @string nowWeReSpeakingRawTcpSayˢ = "Now we're speaking raw TCP. Say hi: "u8;

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
            if (err != default!) {
                Δhttp.Error(w, err.Error(), Δhttp.StatusInternalServerError);
                return;
            }
            // Don't forget to close the connection:
            var connʗ1 = conn;
            defer(() => connʗ1.Close(), ref ᒐ);
            bufrw.Value.Writer.Value.WriteString(nowWeReSpeakingRawTcpSayˢ);
            bufrw.Value.Writer.Value.Flush();
            (var s, err) = bufrw.Value.Reader.Value.ReadString((rune)'\n');
            if (err != default!) {
                log.Printf("error reading string: %v"u8, err);
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
internal static readonly @string httpWwwGoogleComRobotsˢ = "http://www.google.com/robots.txt"u8;

public static void ExampleGet() {
    var (res, err) = Δhttp.Get(httpWwwGoogleComRobotsˢ);
    if (err != default!) {
        log.Fatal(err);
    }
    (var body, err) = io.ReadAll((~res).Body);
    (~res).Body.Close();
    if ((~res).StatusCode > 299) {
        log.Fatalf("Response failed with status code: %d and\nbody: %s\n"u8, (~res).StatusCode, body);
    }
    if (err != default!) {
        log.Fatal(err);
    }
    fmt.Printf("%s"u8, body);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string usrShareDocˢ = "/usr/share/doc"u8;

public static void ExampleFileServer() {
    // Simple static webserver:
    log.Fatal(Δhttp.ListenAndServe(":8080"u8, Δhttp.FileServer(((Δhttp.Dir)(@string)usrShareDocˢ))));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tmpfilesˢ = "/tmpfiles/"u8;
internal static readonly @string tmpˢ = "/tmp"u8;

public static void ExampleFileServer_stripPrefix() {
    // To serve a directory on disk (/tmp) under an alternate URL
    // path (/tmpfiles/), use StripPrefix to modify the request
    // URL's path before the FileServer sees it:
    Δhttp.Handle(tmpfilesˢ, Δhttp.StripPrefix(tmpfilesˢ, Δhttp.FileServer(((Δhttp.Dir)(@string)tmpˢ))));
}

public static void ExampleStripPrefix() {
    // To serve a directory on disk (/tmp) under an alternate URL
    // path (/tmpfiles/), use StripPrefix to modify the request
    // URL's path before the FileServer sees it:
    Δhttp.Handle(tmpfilesˢ, Δhttp.StripPrefix(tmpfilesˢ, Δhttp.FileServer(((Δhttp.Dir)(@string)tmpˢ))));
}

partial struct apiHandler {
}

internal static void ServeHTTP(this apiHandler _Δp0, Δhttp.ResponseWriter _Δp1, ж<Δhttp.Request> _Δp2) {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string apiˢ = "/api/"u8;

public static void ExampleServeMux_Handle() {
    var mux = Δhttp.NewServeMux();
    mux.Handle(apiˢ, new apiHandler(nil));
    mux.HandleFunc("/"u8, (Δhttp.ResponseWriter w, ж<Δhttp.Request> req) => {
        // The "/" pattern matches everything, so we need to check
        // that we're at the root here.
        if ((~(~req).URL).Path != "/"u8) {
            Δhttp.NotFound(w, req);
            return;
        }
        fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "Welcome to the home page!"u8);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sendstrailersˢ = "/sendstrailers"u8;
internal static readonly @string atEnd1AtEnd2ˢ = "AtEnd1, AtEnd2"u8;
internal static readonly @string atEnd3ˢ = "AtEnd3"u8;
internal static readonly @string textPlainCharsetUtf8ˢ = "text/plain; charset=utf-8"u8;
internal static readonly @string atEnd1ˢ = "AtEnd1"u8;
internal static readonly @string value1ˢ = "value 1"u8;
internal static readonly @string thisHttpResponseHasBothˢ = "This HTTP response has both headers before this text and trailers at the end.\n"u8;
internal static readonly @string atEnd2ˢ = "AtEnd2"u8;
internal static readonly @string value2ˢ = "value 2"u8;
internal static readonly @string value3ˢ = "value 3"u8;

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
        signal.Notify(sigint.WithDirection(GoChanDir.Send), os.Interrupt);
        ᐸꟷ(sigint);
        // We received an interrupt signal, shut down.
        {
            var err = Ꮡsrv.Shutdown(context_package.Background()); if (err != default!) {
                // Error from closing listeners, or context timeout:
                log.Printf("HTTP server Shutdown: %v"u8, err);
            }
        }
        builtin.close(idleConnsClosedʗ1);
    });
    {
        var err = Ꮡsrv.ListenAndServe(); if (!AreEqual(err, Δhttp.ErrServerClosed)) {
            // Error starting or closing listener:
            log.Fatalf("HTTP server ListenAndServe: %v"u8, err);
        }
    }
    ᐸꟷ(idleConnsClosed);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string helloTlsˢ = "Hello, TLS!\n"u8;
internal static readonly @string certPemˢ = "cert.pem"u8;
internal static readonly @string keyPemˢ = "key.pem"u8;

public static void ExampleListenAndServeTLS() {
    Δhttp.HandleFunc("/"u8, (Δhttp.ResponseWriter w, ж<Δhttp.Request> req) => {
        io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), helloTlsˢ);
    });
    // One can use generate_cert.go in crypto/tls to generate cert.pem and key.pem.
    log.Printf("About to listen on 8443. Go to https://127.0.0.1:8443/"u8);
    var err = Δhttp.ListenAndServeTLS(":8443"u8, certPemˢ, keyPemˢ, default!);
    log.Fatal(err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string helloWorldˢ = "Hello, world!\n"u8;
internal static readonly @string helloˢ4 = "/hello"u8;

public static void ExampleListenAndServe() {
    // Hello world, the web server
    var helloHandler = (Δhttp.ResponseWriter w, ж<Δhttp.Request> req) => {
        io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), helloWorldˢ);
    };
    Δhttp.HandleFunc(helloˢ4, helloHandler);
    log.Fatal(Δhttp.ListenAndServe(":8080"u8, default!));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string helloFromAHandleFunc1ˢ = "Hello from a HandleFunc #1!\n"u8;
internal static readonly @string helloFromAHandleFunc2ˢ = "Hello from a HandleFunc #2!\n"u8;
internal static readonly @string endpointˢ = "/endpoint"u8;

public static void ExampleHandleFunc() {
    var h1 = (Δhttp.ResponseWriter w, ж<Δhttp.Request> _) => {
        io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), helloFromAHandleFunc1ˢ);
    };
    var h2 = (Δhttp.ResponseWriter w, ж<Δhttp.Request> _) => {
        io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), helloFromAHandleFunc2ˢ);
    };
    Δhttp.HandleFunc("/"u8, h1);
    Δhttp.HandleFunc(endpointˢ, h2);
    log.Fatal(Δhttp.ListenAndServe(":8080"u8, default!));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object thisIsThePeopleHandlerˢ = (@string)"This is the people handler."u8;

internal static httpꓸHandler newPeopleHandler() {
    return new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        fmt.Fprintln(new http_test_package.http_ResponseWriterᴠWriter(w), thisIsThePeopleHandlerˢ);
    }));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string resourcesˢ = "/resources"u8;
internal static readonly @string resourcesPeopleˢ = "/resources/people/"u8;

public static void ExampleNotFoundHandler() {
    var mux = Δhttp.NewServeMux();
    // Create sample handler to returns 404
    mux.Handle(resourcesˢ, Δhttp.NotFoundHandler());
    // Create sample handler that returns 200
    mux.Handle(resourcesPeopleˢ, newPeopleHandler());
    log.Fatal(Δhttp.ListenAndServe(":8080"u8, new Δhttp.ServeMuxжΔHandler(mux)));
}

public static void ExampleProtocols_http1() {
    ref var srv = ref heap<Δhttp.Server>(out var Ꮡsrv);
    srv = new Δhttp.Server(
        Addr: ":8443"u8
    );
    // Serve only HTTP/1.
    srv.Protocols = @new<Δhttp.Protocols>();
    srv.Protocols.SetHTTP1(true);
    log.Fatal(Ꮡsrv.ListenAndServeTLS(certPemˢ, keyPemˢ));
}

public static void ExampleProtocols_http1or2() {
    var t = Δhttp.DefaultTransport._<ж<Δhttp.Transport>>().Clone();
    // Use either HTTP/1 and HTTP/2.
    t.Value.Protocols = @new<Δhttp.Protocols>();
    (~t).Protocols.SetHTTP1(true);
    (~t).Protocols.SetHTTP2(true);
    var cli = Ꮡ(new Δhttp.Client(Transport: new Δhttp.TransportжRoundTripper(t)));
    var (res, err) = cli.Get(httpWwwGoogleComRobotsˢ);
    if (err != default!) {
        log.Fatal(err);
    }
    (~res).Body.Close();
}

} // end http_test_package
