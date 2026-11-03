// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using static global::go.net.http_package;
using os = os_package;
using sync = sync_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using bufio = bufio_package;
using global::go.net;
using net = net_package;
using static global::go.net.http_internal_test_package;
using Δhttp = global::go.net.http_package;

partial class http_test_package {

public static void TestResponseControllerFlush(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testResponseControllerFlush(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object oneˢ = (@string)"one"u8;
internal static readonly object twoˢ = (@string)"two"u8;

internal static void testResponseControllerFlush(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var continuec = new channel<EmptyStruct>(0);
        var continuecʗ1 = continuec;
        var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            var ctl = NewResponseController(w);
            w.Write(slice<byte>("one"u8));
            {
                var errΔ1 = ctl.Flush(); if (errΔ1 != nil) {
                    t.Errorf("ctl.Flush() = %v, want nil", errΔ1);
                    return;
                }
            }
            ᐸꟷ(continuecʗ1);
            w.Write(slice<byte>("two"u8));
        })));

        var (res, err) = cst.c.Get(cst.ts.URL);
        if (err != nil) {
            t.Fatalf("unexpected connection error: %v", err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);

        var buf = new slice<byte>(16);
        (var n, err) = res.Body.Read(buf);
        builtin.close(continuec);
        if (err != nil || ((sstring)(buf.slice(0, n))) != "one"u8) {
            t.Fatalf("Body.Read = %q, %v, want %q, nil", (@string)(buf.slice(0, n)), err, oneˢ);
        }

        (var got, err) = io.ReadAll(res.Body);
        if (err != nil || ((sstring)got) != "two"u8) {
            t.Fatalf("Body.Read = %q, %v, want %q, nil", (@string)got, err, twoˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestResponseControllerHijack(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testResponseControllerHijack(Δp0, Δp1));
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string headerᶜ => ref headerᶜᵇ;
private static readonly @string headerᶜᵇ = "X-Header"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string valueᶜ => ref valueᶜᵇ;
private static readonly @string valueᶜᵇ = "set"u8;

internal static void testResponseControllerHijack(ж<testing.T> t, testMode mode) {
    @string header = headerᶜ;
    @string value = valueᶜ;
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        var ctl = NewResponseController(w);
        var (c, _, errΔ1) = ctl.Hijack();
        if (mode == http2Mode) {
            if (errΔ1 == nil) {
                t.Errorf("ctl.Hijack = nil, want error");
            }
            w.Header().Set(header, value);
            return;
        }
        if (errΔ1 != nil) {
            t.Errorf("ctl.Hijack = _, _, %v, want _, _, nil", errΔ1);
            return;
        }
        fmt.Fprintf(new http_test_package.net_ConnᴠWriter(c), "HTTP/1.0 200 OK\r\n%v: %v\r\nContent-Length: 0\r\n\r\n"u8, header, value);
    })));
    var (res, err) = cst.c.Get(cst.ts.URL);
    if (err != nil) {
        t.Fatal(err);
    }
    {
        @string got = res.Header.Get(header);
        @string want = value; if (got != want) {
            t.Errorf("response header %q = %q, want %q", header, got, want);
        }
    }
}

public static void TestResponseControllerSetPastWriteDeadline(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testResponseControllerSetPastWriteDeadline(Δp0, Δp1));
}

internal static void testResponseControllerSetPastWriteDeadline(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            var ctl = NewResponseController(w);
            w.Write(slice<byte>("one"u8));
            {
                var errΔ1 = ctl.Flush(); if (errΔ1 != nil) {
                    t.Errorf("before setting deadline: ctl.Flush() = %v, want nil", errΔ1);
                }
            }
            {
                var errΔ2 = ctl.SetWriteDeadline(time.Now().Add((time.Duration)(-10000000000L))); if (errΔ2 != nil) {
                    t.Errorf("ctl.SetWriteDeadline() = %v, want nil", errΔ2);
                }
            }

            w.Write(slice<byte>("two"u8));
            {
                var errΔ3 = ctl.Flush(); if (errΔ3 == nil) {
                    t.Errorf("after setting deadline: ctl.Flush() = nil, want non-nil");
                }
            }
            // Connection errors are sticky, so resetting the deadline does not permit
            // making more progress. We might want to change this in the future, but verify
            // the current behavior for now. If we do change this, we'll want to make sure
            // to do so only for writing the response body, not headers.
            {
                var errΔ4 = ctl.SetWriteDeadline(time.Now().Add((time.Duration)(3600000000000L))); if (errΔ4 != nil) {
                    t.Errorf("ctl.SetWriteDeadline() = %v, want nil", errΔ4);
                }
            }
            w.Write(slice<byte>("three"u8));
            {
                var errΔ5 = ctl.Flush(); if (errΔ5 == nil) {
                    t.Errorf("after resetting deadline: ctl.Flush() = nil, want non-nil");
                }
            }
        })));

        var (res, err) = cst.c.Get(cst.ts.URL);
        if (err != nil) {
            t.Fatalf("unexpected connection error: %v", err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        var (b, _) = io.ReadAll(res.Body);
        if (((sstring)b) != "one"u8) {
            t.Errorf("unexpected body: %q", (@string)b);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestResponseControllerSetFutureWriteDeadline(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testResponseControllerSetFutureWriteDeadline(Δp0, Δp1));
}

internal static void testResponseControllerSetFutureWriteDeadline(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var errc = new channel<error>(1);
        var startwritec = new channel<EmptyStruct>(0);
        var errcʗ1 = errc;
        var startwritecʗ1 = startwritec;
        var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            var ctl = NewResponseController(w);
            w.WriteHeader(200);
            {
                var errΔ1 = ctl.Flush(); if (errΔ1 != nil) {
                    t.Errorf("ctl.Flush() = %v, want nil", errΔ1);
                }
            }
            ᐸꟷ(startwritecʗ1); // don't set the deadline until the client reads response headers
            {
                var errΔ2 = ctl.SetWriteDeadline(time.Now().Add(1 * time.Millisecond)); if (errΔ2 != nil) {
                    t.Errorf("ctl.SetWriteDeadline() = %v, want nil", errΔ2);
                }
            }
            var (_, errΔ3) = io.Copy(new http_test_package.http_ResponseWriterᴠWriter(w), (neverEnding)(rune)'a');
            errcʗ1.ᐸꟷ(errΔ3);
        })));

        var (res, err) = cst.c.Get(cst.ts.URL);
        builtin.close(startwritec);
        if (err != nil) {
            t.Fatalf("unexpected connection error: %v", err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        (_, err) = io.Copy(io.Discard, res.Body);
        if (err == nil) {
            t.Errorf("client reading from truncated request body: got nil error, want non-nil");
        }
        err = ᐸꟷ(errc); // io.Copy error
        if (!errors.Is(err, os.ErrDeadlineExceeded)) {
            t.Errorf("server timed out writing request body: got err %v; want os.ErrDeadlineExceeded", err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestResponseControllerSetPastReadDeadline(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testResponseControllerSetPastReadDeadline(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string textFooˢ => ref textFooˢᵇ;
private static readonly @string textFooˢᵇ = "text/foo"u8;

internal static partial void testResponseControllerSetPastReadDeadline(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var readc = new channel<EmptyStruct>(0);
        var donec = new channel<EmptyStruct>(0);
        var donecʗ1 = donec;
        var readcʗ1 = readc;
        var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => builtin.close(ᴛ1), donecʗ1, ref ᒐ);
                var ctl = NewResponseController(w);
                var b = new slice<byte>(3);
                var (n, errΔ1) = io.ReadFull(r.Body, b);
                b = b.slice(0, n);
                if (errΔ1 != nil || ((sstring)b) != "one"u8) {
                    t.Errorf("before setting read deadline: Read = %v, %q, want nil, %q", errΔ1, (@string)b, oneˢ);
                    return;
                }
                {
                    var errΔ2 = ctl.SetReadDeadline(time.Now()); if (errΔ2 != nil) {
                        t.Errorf("ctl.SetReadDeadline() = %v, want nil", errΔ2);
                        return;
                    }
                }
                (b, errΔ1) = io.ReadAll(r.Body);
                if (errΔ1 == nil || ((sstring)b) != ""u8) {
                    t.Errorf("after setting read deadline: Read = %q, nil, want error", (@string)b);
                }
                builtin.close(readcʗ1);
                // Connection errors are sticky, so resetting the deadline does not permit
                // making more progress. We might want to change this in the future, but verify
                // the current behavior for now.
                {
                    var errΔ3 = ctl.SetReadDeadline(new time.Time()); if (errΔ3 != nil) {
                        t.Errorf("ctl.SetReadDeadline() = %v, want nil", errΔ3);
                        return;
                    }
                }
                (b, errΔ1) = io.ReadAll(r.Body);
                if (errΔ1 == nil) {
                    t.Errorf("after resetting read deadline: Read = %q, nil, want error", (@string)b);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        })));

        var (pr, pw) = io.Pipe();
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(1);
        var donecʗ2 = donec;
        var pwʗ1 = pw;
        var readcʗ2 = readc;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                var pwʗ2 = pwʗ1;
                defer(() => pwʗ2.Close(), ref ᒐ);
                pwʗ1.Write(slice<byte>("one"u8));
                var selᴛ32 = readcʗ2;
                var selᴛ33 = donecʗ2;
                switch (select(ᐸꟷ(selᴛ32, ꓸꓸꓸ), ᐸꟷ(selᴛ33, ꓸꓸꓸ))) {
                case 0 when selᴛ32.ꟷᐳ(out _): {
                    break;
                }
                case 1 when selᴛ33.ꟷᐳ(out _): {
                    var selᴛ34 = readcʗ2;
                    switch (trySelect(ᐸꟷ(selᴛ34, ꓸꓸꓸ))) {
                    case 0 when selᴛ34.ꟷᐳ(out _): {
                        break;
                    }
                    default: {
                        t.Errorf("server handler unexpectedly exited without closing readc");
                        return;
                    }}
                    break;
                }}
                pwʗ1.Write(slice<byte>("two"u8));
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        defer(Ꮡwg.Wait, ref ᒐ);
        var (res, err) = cst.c.Post(cst.ts.URL, textFooˢ, new io.PipeReaderжReader(pr));
        if (err == nil) {
            var resʗ1 = res;
            defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestResponseControllerSetFutureReadDeadline(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testResponseControllerSetFutureReadDeadline(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string responseBodyˢ => ref responseBodyˢᵇ;
private static readonly @string responseBodyˢᵇ = "response body"u8;
internal static ref readonly @string textApocryphalˢ => ref textApocryphalˢᵇ;
private static readonly @string textApocryphalˢᵇ = "text/apocryphal"u8;

internal static void testResponseControllerSetFutureReadDeadline(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        @string respBody = responseBodyˢ;
        var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> req) => {
            var ctl = NewResponseController(w);
            {
                var errΔ1 = ctl.SetReadDeadline(time.Now().Add(1 * time.Millisecond)); if (errΔ1 != nil) {
                    t.Errorf("ctl.SetReadDeadline() = %v, want nil", errΔ1);
                }
            }
            var (_, errΔ2) = io.Copy(io.Discard, req.Body);
            if (!errors.Is(errΔ2, os.ErrDeadlineExceeded)) {
                t.Errorf("server timed out reading request body: got err %v; want os.ErrDeadlineExceeded", errΔ2);
            }
            w.Write(slice<byte>(respBody));
        })));
        var (pr, pw) = io.Pipe();
        var (res, err) = cst.c.Post(cst.ts.URL, textApocryphalˢ, new io.PipeReaderжReader(pr));
        if (err != nil) {
            t.Fatal(err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        (var got, err) = io.ReadAll(res.Body);
        if (((sstring)got) != respBody || err != nil) {
            t.Errorf("client read response body: %q, %v; want %q, nil", (@string)got, err, respBody);
        }
        pw.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct wrapWriter {
    /*embed*/ public global::go.net.http_package.ResponseWriter ResponseWriter;
}

internal static Δhttp.ResponseWriter Unwrap(this wrapWriter w) {
    return w.ResponseWriter;
}

public static void TestWrappedResponseController(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testWrappedResponseController(Δp0, Δp1));
}

internal static void testWrappedResponseController(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            w = new wrapWriter(w);
            var ctl = NewResponseController(w);
            {
                var errΔ1 = ctl.Flush(); if (errΔ1 != nil) {
                    t.Errorf("ctl.Flush() = %v, want nil", errΔ1);
                }
            }
            {
                var errΔ2 = ctl.SetReadDeadline(new time.Time()); if (errΔ2 != nil) {
                    t.Errorf("ctl.SetReadDeadline() = %v, want nil", errΔ2);
                }
            }
            {
                var errΔ3 = ctl.SetWriteDeadline(new time.Time()); if (errΔ3 != nil) {
                    t.Errorf("ctl.SetWriteDeadline() = %v, want nil", errΔ3);
                }
            }
        })));
        var (res, err) = cst.c.Get(cst.ts.URL);
        if (err != nil) {
            t.Fatalf("unexpected connection error: %v", err);
        }
        io.Copy(io.Discard, res.Body);
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestResponseControllerEnableFullDuplex(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testResponseControllerEnableFullDuplex(Δp0, Δp1));
}

internal static void testResponseControllerEnableFullDuplex(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> req) => {
            var ctl = NewResponseController(w);
            {
                var errΔ1 = ctl.EnableFullDuplex(); if (errΔ1 != nil) {
                    // TODO: Drop test for HTTP/2 when x/net is updated to support
                    // EnableFullDuplex. Since HTTP/2 supports full duplex by default,
                    // the rest of the test is fine; it's just the EnableFullDuplex call
                    // that fails.
                    if (mode != http2Mode) {
                        t.Errorf("ctl.EnableFullDuplex() = %v, want nil", errΔ1);
                    }
                }
            }
            w.WriteHeader(200);
            ctl.Flush();
            for (;;) {
                array<byte> buf = new(1);
                var (n, errΔ2) = req.Body.Read(buf[..]);
                if (n != 1 || errΔ2 != nil) {
                    break;
                }
                w.Write(buf[..]);
                ctl.Flush();
            }
        })));
        var (pr, pw) = io.Pipe();
        var (res, err) = cst.c.Post(cst.ts.URL, textApocryphalˢ, new io.PipeReaderжReader(pr));
        if (err != nil) {
            t.Fatal(err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        for (var i = (byte)0; i < 10; i++) {
            {
                var (_, errΔ3) = pw.Write(slice<byte>([i])); if (errΔ3 != nil) {
                    t.Fatalf("Write: %v", errΔ3);
                }
            }
            array<byte> buf = new(1);
            {
                var (n, errΔ4) = res.Body.Read(buf[..]); if (n != 1 || errΔ4 != nil) {
                    t.Fatalf("Read: %v, %v", n, errΔ4);
                }
            }
            if (buf[0] != i) {
                t.Fatalf("read byte %v, want %v", buf[0], i);
            }
        }
        pw.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestIssue58237(ж<testing.T> t) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), http2Mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> req) => {
        var ctl = NewResponseController(w);
        {
            var errΔ1 = ctl.SetReadDeadline(time.Now().Add(1 * time.Millisecond)); if (errΔ1 != nil) {
                t.Errorf("ctl.SetReadDeadline() = %v, want nil", errΔ1);
            }
        }
        time.Sleep(10 * time.Millisecond);
    })));
    var (res, err) = cst.c.Get(cst.ts.URL);
    if (err != nil) {
        t.Fatal(err);
    }
    res.Body.Close();
}

} // end http_test_package
