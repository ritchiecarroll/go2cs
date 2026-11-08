// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using context = context_package;
using fmt = fmt_package;
using io = io_package;
using Δhttp = global::go.net.http_package;
using sync = sync_package;
using atomic = global::go.sync.atomic_package;
using testing = testing_package;
using synctest = global::go.testing.synctest_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using global::go.net;
using global::go.sync;
using global::go.testing;
using net = net_package;
using static global::go.net.http_internal_test_package;
using ꓸꓸꓸany = Span<any>;

partial class http_test_package {

public static void TestTransportNewClientConnRoundTrip(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testTransportNewClientConnRoundTrip(Δp0, Δp1)); }
internal static void testTransportNewClientConnRoundTrip(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> reqΔ1) => {
            io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), reqΔ1.Host);
        })), optFakeNet.OrTypedNil());

        @string scheme = mode.Scheme(); // http or https
        var (cc, err) = cst.tr.NewClientConn(t.Context(), scheme, cst.ts.Listener.Addr().String());
        if (err != nil) {
            t.Fatal(err);
        }
        var ccʗ1 = cc;
        defer(() => ccʗ1.Close(), ref ᒐ);

        // Send requests for a couple different domains.
        // All use the same connection.
        foreach (var (_, host) in range(slice<@string>(["example.tld", "go.dev"]))) {
            var (reqΔ2, _) = Δhttp.NewRequest(getˢ2, fmt.Sprintf("%v://%v/"u8, scheme, host), default);
            var (respΔ1, errΔ1) = cc.RoundTrip(reqΔ2);
            if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
            var (got, _) = io.ReadAll(respΔ1.Body);
            if (((sstring)got) != host) {
                t.Errorf("got response body %q, want %v", got, host);
            }
            respΔ1.Body.Close();

            // CloseIdleConnections does not close connections created by NewClientConn.
            cst.tr.CloseIdleConnections();
        }

        var errΔ2 = cc.Err(); if (errΔ2 != nil) {
            t.Errorf("before close: ClientConn.Err() = %v, want nil", errΔ2);
        }

        cc.Close();
        var errΔ3 = cc.Err(); if (errΔ3 == nil) {
            t.Errorf("after close: ClientConn.Err() = nil, want error");
        }

        var (req, _) = Δhttp.NewRequest(getˢ2, scheme + "://example.tld/"u8, default);
        (var resp, err) = cc.RoundTrip(req);
        if (err == nil) {
            resp.Body.Close();
            t.Errorf("after close: cc.RoundTrip succeeded, want error");
        }
        t.Log(err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static (ж<clientServerTest>, ж<Δhttp.ClientConn>) newClientConnTest(testing.TB t, testMode mode, Δhttp.HandlerFunc h, params ꓸꓸꓸany optsʗp) {
    var opts = optsʗp.sslice();

    if (h == nil) {
        h = (Δhttp.ResponseWriter w, ж<Δhttp.Request> req) => {
        };
    }
    var cst = newClientServerTest(t, mode, new http_test_package.http_HandlerFuncᴠΔHandler(h), opts.ꓸꓸꓸ);
    var (cc, err) = cst.tr.NewClientConn(t.Context(), mode.Scheme(), cst.ts.Listener.Addr().String());
    if (err != nil) {
        t.Fatal(err);
    }
    var ccʗ1 = cc;
    t.Cleanup(() => {
        ccʗ1.Close();
    });
    synctest.Wait();
    return (cst, cc);
}

// TestClientConnReserveAll reserves every concurrency slot on a connection.
public static void TestClientConnReserveAll(ж<testing.T> t) { runSynctest(t, testClientConnReserveAll); }
internal static void testClientConnReserveAll(ж<testing.T> t, testMode mode) {
    var (cst, cc) = newClientConnTest(new http_test_package.testing_TжTB(t), mode, default, optFakeNet.OrTypedNil(), (ж<Δhttp.Server> s) => {
        s.HTTP2 = Ꮡ(new Δhttp.HTTP2Config(
            MaxConcurrentStreams: 3
        ));
    });

    nint want = 1;
    var exprᴛ1 = mode;
    switch (ᐧ) {
    case {} when exprᴛ1 == http2Mode || exprᴛ1 == http2UnencryptedMode: {
        want = cst.ts.Config.HTTP2.MaxConcurrentStreams;
        break;
    }}
    nint available = cc.Available();
    if (available != want) {
        t.Fatalf("cc.Available() = %v, want %v", available, want);
    }

    // Reserve every available concurrency slot on the connection.
    foreach (var i in range(available)) {
        var err = cc.Reserve(); if (err != nil) {
            t.Fatalf("cc.Reserve() #%v = %v, want nil", i, err);
        }
        {
            nint got = cc.Available();
            nint wantΔ1 = available - i - 1; if (got != wantΔ1) {
                t.Fatalf("cc.Available() = %v, want %v", got, wantΔ1);
            }
        }
        {
            nint got = cc.InFlight();
            nint wantΔ2 = i + 1; if (got != wantΔ2) {
                t.Fatalf("cc.InFlight() = %v, want %v", got, wantΔ2);
            }
        }
    }

    // The next reservation attempt should fail, since every slot is consumed.
    {
        var err = cc.Reserve(); if (err == nil) {
            t.Fatalf("cc.Reserve() = nil, want error");
        }
    }
}

// TestClientConnReserveParallel starts concurrent goroutines which reserve every
// concurrency slot on a connection.
public static void TestClientConnReserveParallel(ж<testing.T> t) { runSynctest(t, testClientConnReserveParallel); }
internal static void testClientConnReserveParallel(ж<testing.T> t, testMode mode) {
    var (_, cc) = newClientConnTest(new http_test_package.testing_TжTB(t), mode, default, optFakeNet.OrTypedNil(), (ж<Δhttp.Server> s) => {
        s.HTTP2 = Ꮡ(new Δhttp.HTTP2Config(
            MaxConcurrentStreams: 3
        ));
    });
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    ref var mu = ref heap(new sync.Mutex(), out var Ꮡmu);
    nint success = default;
    nint failure = default;
    nint available = cc.Available();
    const nint extra = 2;
    foreach (var _ᴛ1 in range(available + extra)) {
        var ccʗ1 = cc;
        Ꮡwg.Go(() => {
            GoFrame ᒐ = default;
            try {
                var err = ccʗ1.Reserve();
                Ꮡmu.Lock();
                defer(Ꮡmu.Unlock, ref ᒐ);
                if (err == nil) {
                    success++;
                } else {
                    failure++;
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();

    {
        nint got = success;
        nint want = available; if (got != want) {
            t.Errorf("%v successful reservations, want %v", got, want);
        }
    }
    {
        nint got = failure;
        nint want = extra; if (got != want) {
            t.Errorf("%v failed reservations, want %v", got, want);
        }
    }
}

// TestClientConnReserveRelease repeatedly reserves and releases concurrency slots.
public static void TestClientConnReserveRelease(ж<testing.T> t) { runSynctest(t, testClientConnReserveRelease); }
internal static void testClientConnReserveRelease(ж<testing.T> t, testMode mode) {
    var (_, cc) = newClientConnTest(new http_test_package.testing_TжTB(t), mode, default, optFakeNet.OrTypedNil(), (ж<Δhttp.Server> s) => {
        s.HTTP2 = Ꮡ(new Δhttp.HTTP2Config(
            MaxConcurrentStreams: 3
        ));
    });

    nint available = cc.Available();
    foreach (var i in range(2 * available)) {
        var err = cc.Reserve(); if (err != nil) {
            t.Fatalf("cc.Reserve() #%v = %v, want nil", i, err);
        }
        cc.Release();
    }

    nint got = cc.Available();
    nint want = available; if (got != want) {
        t.Fatalf("cc.Available() = %v, want %v", available, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string invalidFieldNameˢ => ref invalidFieldNameˢᵇ;
private static readonly @string invalidFieldNameˢᵇ = "invalid field name"u8;

internal partial struct TestClientConnReserveAndConsume_type /*dyn*/ {
    internal @string name;
    internal Action<ж<testing.T>, ж<Δhttp.ClientConn>, testMode> consume;
    internal Action<Δhttp.ResponseWriter, ж<Δhttp.Request>, channel<EmptyStruct>> handler;
    internal bool h1Closed;
}

// TestClientConnReserveAndConsume reserves a concurrency slot on a connection,
// and then verifies that various events consume the reservation.
public static void TestClientConnReserveAndConsume(ж<testing.T> t) {


    foreach (var (_, vᴛ1) in range(slice<TestClientConnReserveAndConsume_type>([new(
        // Explicit release.
        name: "release",
        consume: (ж<testing.T> tΔ1, ж<Δhttp.ClientConn> cc, testMode mode) => {
            cc.Release();
        }
    ), new(
        // Invalid request sent to RoundTrip.
        name: "invalid field name",
        consume: (ж<testing.T> tΔ2, ж<Δhttp.ClientConn> cc, testMode mode) => {
            var (req, _) = Δhttp.NewRequest(getˢ2, mode.Scheme() + "://example.tld/"u8, default);
            req.Header[invalidFieldNameˢ] = ["x"];
            var (_, err) = cc.RoundTrip(req);
            if (err == nil) {
                tΔ2.Fatalf("RoundTrip succeeded, want failure");
            }
        }
    ), new(
        // Successful request/response cycle.
        name: "body close",
        consume: (ж<testing.T> tΔ3, ж<Δhttp.ClientConn> cc, testMode mode) => {
            var (req, _) = Δhttp.NewRequest(getˢ2, mode.Scheme() + "://example.tld/"u8, default);
            var (resp, err) = cc.RoundTrip(req);
            if (err != nil) {
                tΔ3.Fatalf("RoundTrip: %v", err);
            }
            resp.Body.Close();
        }
    ), new(
        // Request context canceled before headers received.
        name: "cancel",
        consume: [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ4, ж<Δhttp.ClientConn> cc, testMode mode) => {
            var (ctx, cancel) = context_package.WithCancel(tΔ4.Context());
            var ctxʗ1 = ctx;
            goǃ(() => {
                var (req, _) = Δhttp.NewRequestWithContext(ctxʗ1, getˢ2, mode.Scheme() + "://example.tld/"u8, default);
                var (_, err) = cc.RoundTrip(req);
                if (err == nil) {
                    tΔ4.Errorf("RoundTrip succeeded, want failure");
                }
            });
            synctest.Wait();
            cancel();
        },
        handler: (Δhttp.ResponseWriter w, ж<Δhttp.Request> req, channel<EmptyStruct> donec) => {
            ᐸꟷ(donec);
        },
        // An HTTP/1 connection is closed after a request is canceled on it.
        h1Closed: true
    ), new(
        // Response body closed before full response received.
        name: "early body close",
        consume: (ж<testing.T> tΔ5, ж<Δhttp.ClientConn> cc, testMode mode) => {
            var (req, _) = Δhttp.NewRequest(getˢ2, mode.Scheme() + "://example.tld/"u8, default);
            var (resp, err) = cc.RoundTrip(req);
            if (err != nil) {
                tΔ5.Fatalf("RoundTrip: %v", err);
            }
            tΔ5.Logf("%T", resp.Body);
            resp.Body.Close();
        },
        handler: (Δhttp.ResponseWriter w, ж<Δhttp.Request> req, channel<EmptyStruct> donec) => {
            w.WriteHeader(200);
            Δhttp.NewResponseController(w).Flush();
            ᐸꟷ(donec);
        },
        // An HTTP/1 connection is closed after a request is canceled on it.
        h1Closed: true
    )
    ]))) {
        ref var test = ref heap(new TestClientConnReserveAndConsume_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        t.Run(test.name, (ж<testing.T> tΔ6) => {
            var testʗ2 = testʗ1;
            runSynctest(tΔ6, (ж<testing.T> tΔ7, testMode mode) => {
                GoFrame ᒐ = default;
                try {
                    var donec = new channel<EmptyStruct>(0);
                    defer(ᴛ1 => builtin.close(ᴛ1), donec, ref ᒐ);
                    var donecʗ1 = donec;
                    var testʗ3 = testʗ2;
                    var handler = (Δhttp.ResponseWriter w, ж<Δhttp.Request> req) => {
                        if (testʗ3.handler != nil) {
                            testʗ3.handler(w, req, donecʗ1);
                        }
                    };

                    var (_, cc) = newClientConnTest(new http_test_package.testing_TжTB(tΔ7), mode, new Δhttp.HandlerFunc(handler), optFakeNet.OrTypedNil());
                    nint stateHookCalls = 0;
                    cc.SetStateHook((ж<Δhttp.ClientConn> ccΔ1) => {
                        stateHookCalls++;
                    });
                    synctest.Wait();
                    stateHookCalls = 0; // ignore any initial update call

                    nint avail = cc.Available();
                    {
                        var err = cc.Reserve(); if (err != nil) {
                            tΔ7.Fatalf("cc.Reserve() = %v, want nil", err);
                        }
                    }
                    synctest.Wait();
                    {
                        nint got = stateHookCalls;
                        nint want = 0; if (got != want) {
                            tΔ7.Errorf("connection state hook calls: %v, want %v", got, want);
                        }
                    }

                    testʗ2.consume(tΔ7, cc, mode);
                    synctest.Wait();

                    // State hook should be called, either to report the
                    // connection availability increasing or the connection closing,
                    // or both.
                    if (stateHookCalls == 0) {
                        tΔ7.Errorf("connection state hook calls: %v, want >1", stateHookCalls);
                    }

                    if (testʗ2.h1Closed && (mode == http1Mode || mode == https1Mode)) {
                        {
                            nint got = cc.Available();
                            nint want = 0; if (got != want) {
                                tΔ7.Errorf("cc.Available() = %v, want %v", got, want);
                            }
                        }
                        {
                            nint got = cc.InFlight();
                            nint want = 0; if (got != want) {
                                tΔ7.Errorf("cc.InFlight() = %v, want %v", got, want);
                            }
                        }
                        var err = cc.Err(); if (err == nil) {
                            tΔ7.Errorf("cc.Err() = nil, want closed connection");
                        }
                    } else {
                        {
                            nint got = cc.Available();
                            nint want = avail; if (got != want) {
                                tΔ7.Errorf("cc.Available() = %v, want %v", got, want);
                            }
                        }
                        {
                            nint got = cc.InFlight();
                            nint want = 0; if (got != want) {
                                tΔ7.Errorf("cc.InFlight() = %v, want %v", got, want);
                            }
                        }
                        var err = cc.Err(); if (err != nil) {
                            tΔ7.Errorf("cc.Err() = %v, want nil", err);
                        }
                    }

                    if (cc.Available() > 0) {
                        var err = cc.Reserve(); if (err != nil) {
                            tΔ7.Errorf("cc.Reserve() = %v, want success", err);
                        }
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        });
    }
}

// TestClientConnRoundTripBlocks verifies that RoundTrip blocks until a concurrency
// slot is available on a connection.
public static void TestClientConnRoundTripBlocks(ж<testing.T> t) { runSynctest(t, testClientConnRoundTripBlocks); }
internal static partial void testClientConnRoundTripBlocks(ж<testing.T> t, testMode mode) {
    ref var handlerCalls = ref heap(new atomic.Int64(), out var ᏑhandlerCalls);
    var requestc = new channel<EmptyStruct>(0);
    var requestcʗ1 = requestc;
    var handler = (Δhttp.ResponseWriter w, ж<Δhttp.Request> req) => {
        ᏑhandlerCalls.Add(1);
        ᐸꟷ(requestcʗ1);
    };
    var (_, cc) = newClientConnTest(new http_test_package.testing_TжTB(t), mode, new Δhttp.HandlerFunc(handler), optFakeNet.OrTypedNil(), (ж<Δhttp.Server> s) => {
        s.HTTP2 = Ꮡ(new Δhttp.HTTP2Config(
            MaxConcurrentStreams: 3
        ));
    });

    nint available = cc.Available();
    ref var responses = ref heap(new atomic.Int64(), out var Ꮡresponses);
    const nint extra = 2;
    foreach (var _ᴛ1 in range(available + extra)) {
        var ccʗ1 = cc;
        goǃ(() => {
            var (req, _) = Δhttp.NewRequest(getˢ2, mode.Scheme() + "://example.tld/"u8, default);
            var (resp, err) = ccʗ1.RoundTrip(req);
            Ꮡresponses.Add(1);
            if (err != nil) {
                t.Errorf("RoundTrip: %v", err);
                return;
            }
            resp.Body.Close();
        });
    }

    synctest.Wait();
    {
        nint got = (nint)ᏑhandlerCalls.Load();
        nint want = available; if (got != want) {
            t.Errorf("got %v handler calls, want %v", got, want);
        }
    }
    {
        nint got = (nint)Ꮡresponses.Load();
        nint want = 0; if (got != want) {
            t.Errorf("got %v responses, want %v", got, want);
        }
    }

    foreach (var i in range(available + extra)) {
        requestc.ᐸꟷ(new EmptyStruct());
        synctest.Wait();
        nint got = (nint)Ꮡresponses.Load();
        nint want = i + 1; if (got != want) {
            t.Errorf("got %v responses, want %v", got, want);
        }
    }
}

} // end http_test_package
