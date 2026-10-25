// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.testing;

using bufio = bufio_package;
using bytes = bytes_package;
using context = context_package;
using io = io_package;
using net = net_package;
using http = go.net.http_package;
using strings = strings_package;
using testing = testing_package;
using synctest = go.testing.synctest_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.net;
using go.testing;

partial class synctest_test_package {

// Keep the following tests in sync with the package documentation.
public static void TestTime(ж<testing.T> Ꮡt) {
    synctest.Test(Ꮡt, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
        ref var start = ref heap<time.Time>(out var Ꮡstart);
        start = time.Now(); // always midnight UTC 2000-01-01
        var startʗ1 = start;
        goǃ(() => {
            time.Sleep(1 * time.ΔNanosecond);
            tΔ1.Log(time.Since(startʗ1)); // always logs "1ns"
        });
        time.Sleep(2 * time.ΔNanosecond); // the AfterFunc will run before this Sleep returns
        tΔ1.Log(time.Since(start)); // always logs "2ns"
    });
}

public static void TestWait(ж<testing.T> Ꮡt) {
    synctest.Test(Ꮡt, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
        var done = false;
        goǃ(() => {
            done = true;
        });
        // Wait will block until the goroutine above has finished.
        synctest.Wait();
        tΔ1.Log(done); // always logs "true"
    });
}

public static void TestContextAfterFunc(ж<testing.T> Ꮡt) {
    synctest.Test(Ꮡt, (ж<testing.T> tΔ1) => {
        // Create a context.Context which can be canceled.
        var (ctx, cancel) = context.WithCancel(tΔ1.Context());

        // context.AfterFunc registers a function to be called
        // when a context is canceled.
        var afterFuncCalled = false;
        context.AfterFunc(ctx, () => {
            afterFuncCalled = true;
        });

        // The context has not been canceled, so the AfterFunc is not called.
        synctest.Wait();
        if (afterFuncCalled) {
            tΔ1.Fatalf("before context is canceled: AfterFunc called"u8);
        }

        // Cancel the context and wait for the AfterFunc to finish executing.
        // Verify that the AfterFunc ran.
        cancel();
        synctest.Wait();
        if (!afterFuncCalled) {
            tΔ1.Fatalf("after context is canceled: AfterFunc not called"u8);
        }
    });
}

public static void TestContextWithTimeout(ж<testing.T> Ꮡt) {
    synctest.Test(Ꮡt, (ж<testing.T> tΔ1) => {
        GoFrame ᒐ = default;
        try {
            // Create a context.Context which is canceled after a timeout.
            time.Duration timeout = /* 5 * time.Second */ 5000000000;
            var (ctx, cancel) = context.WithTimeout(tΔ1.Context(), timeout);
            var cancelʗ1 = cancel;
            defer(() => cancelʗ1(), ref ᒐ);

            // Wait just less than the timeout.
            time.Sleep((time.Duration)(4999999999L));
            synctest.Wait();
            {
                var err = ctx.Err(); if (err != default!) {
                    tΔ1.Fatalf("before timeout: ctx.Err() = %v, want nil\n"u8, err);
                }
            }

            // Wait the rest of the way until the timeout.
            time.Sleep(time.ΔNanosecond);
            synctest.Wait();
            {
                var err = ctx.Err(); if (!AreEqual(err, context.DeadlineExceeded)) {
                    tΔ1.Fatalf("after timeout: ctx.Err() = %v, want DeadlineExceeded\n"u8, err);
                }
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string requestBodyˢ = "request body"u8;
private static readonly @string putˢ = "PUT"u8;
private static readonly @string httpTestTldˢ = "http://test.tld/"u8;
private static readonly @string expectˢ = "Expect"u8;
private static readonly @string continueˢ = "100-continue"u8;

public static void TestHTTPTransport100Continue(ж<testing.T> Ꮡt) {
    synctest.Test(Ꮡt, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> _Δp0) => {
        GoFrame ᒐ = default;
        try {
            // Create an in-process fake network connection.
            // We cannot use a loopback network connection for this test,
            // because goroutines blocked on network I/O prevent a synctest
            // bubble from becoming idle.
            var (srvConn, cliConn) = net.Pipe();
            var cliConnʗ1 = cliConn;
            defer(() => cliConnʗ1.Close(), ref ᒐ);
            var srvConnʗ1 = srvConn;
            defer(() => srvConnʗ1.Close(), ref ᒐ);

                var cliConnʗ2 = cliConn;
            var tr = Ꮡ(new http.Transport(
                // Use the fake network connection created above.
                DialContext: (context.Context ctx, @string network, @string address) => (cliConnʗ2, default!),
                // Enable "Expect: 100-continue" handling.
                ExpectContinueTimeout: (time.Duration)(5000000000L)
            ));

            // Send a request with the "Expect: 100-continue" header set.
            // Send it in a new goroutine, since it won't complete until the end of the test.
            @string body = requestBodyˢ;
            var trʗ1 = tr;
            goǃ(() => {
                var (reqΔ1, _) = http.NewRequest(putˢ, httpTestTldˢ, new strings_ReaderжReader(strings.NewReader(body)));
                (~reqΔ1).Header.Set(expectˢ, continueˢ);
                var (resp, errΔ1) = trʗ1.RoundTrip(reqΔ1);
                if (errΔ1 != default!){
                    Ꮡt.Errorf("RoundTrip: unexpected error %v\n"u8, errΔ1);
                } else {
                    (~resp).Body.Close();
                }
            });

            // Read the request headers sent by the client.
            var (req, err) = http.ReadRequest(bufio.NewReader(new net_ConnᴠReader(srvConn)));
            if (err != default!) {
                Ꮡt.Fatalf("ReadRequest: %v\n"u8, err);
            }

            // Start a new goroutine copying the body sent by the client into a buffer.
            // Wait for all goroutines in the bubble to block and verify that we haven't
            // read anything from the client yet.
            ref var gotBody = ref heap(new bytes.Buffer(), out var ᏑgotBody);
            goǃ((ᴛ1, ᴛ2) => io.Copy(ᴛ1, ᴛ2), new bytes_BufferжWriter(ᏑgotBody), new io_ReadCloserᴠReader((~req).Body));
            synctest.Wait();
            {
                @string got = ᏑgotBody.String();
                @string want = ""u8; if (got != want) {
                    Ꮡt.Fatalf("before sending 100 Continue, read body: %q, want %q\n"u8, got, want);
                }
            }

            // Write a "100 Continue" response to the client and verify that
            // it sends the request body.
            srvConn.Write(slice<byte>("HTTP/1.1 100 Continue\r\n\r\n"u8));
            synctest.Wait();
            {
                @string got = ᏑgotBody.String();
                @string want = body; if (got != want) {
                    Ꮡt.Fatalf("after sending 100 Continue, read body: %q, want %q\n"u8, got, want);
                }
            }

            // Finish up by sending the "200 OK" response to conclude the request.
            srvConn.Write(slice<byte>("HTTP/1.1 200 OK\r\n\r\n"u8));

            // We started several goroutines during the test.
            // The synctest.Test call will wait for all of them to exit before returning.
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

} // end synctest_test_package
