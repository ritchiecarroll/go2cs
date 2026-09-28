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
using bufio = bufio_package;
using global::go.net;
using net = net_package;
using static global::go.net.http_internal_test_package;
using Δhttp = global::go.net.http_package;

partial class http_test_package {

public static void TestResponseControllerFlush(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testResponseControllerFlush(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object oneˢ = (@string)"one"u8;
internal static readonly object twoˢ = (@string)"two"u8;

internal static void testResponseControllerFlush(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var continuec = new channel<EmptyStruct>(0);
        var continuecʗ1 = continuec;
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            var ctl = NewResponseController(w);
            w.Write(slice<byte>("one"u8));
            {
                var errΔ1 = ctl.Flush(); if (errΔ1 != default!) {
                    Ꮡt.Errorf("ctl.Flush() = %v, want nil"u8, errΔ1);
                    return;
                }
            }
            ᐸꟷ(continuecʗ1);
            w.Write(slice<byte>("two"u8));
        })));
        var (res, err) = (~cst).c.Get((~(~cst).ts).URL);
        if (err != default!) {
            Ꮡt.Fatalf("unexpected connection error: %v"u8, err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        var buf = new slice<byte>(16);
        (var n, err) = (~res).Body.Read(buf);
        builtin.close(continuec);
        if (err != default! || ((sstring)(buf[..(int)(n)])) != "one"u8) {
            Ꮡt.Fatalf("Body.Read = %q, %v, want %q, nil"u8, ((@string)(buf[..(int)(n)])), err, oneˢ);
        }
        (var got, err) = io.ReadAll((~res).Body);
        if (err != default! || ((sstring)got) != "two"u8) {
            Ꮡt.Fatalf("Body.Read = %q, %v, want %q, nil"u8, ((@string)got), err, twoˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestResponseControllerHijack(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testResponseControllerHijack(Δp0, Δp1));
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string headerᶜ = "X-Header"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string valueᶜ = "set"u8;

internal static void testResponseControllerHijack(ж<testing.T> Ꮡt, testMode mode) {
    @string header = headerᶜ;
    @string value = valueᶜ;
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        var ctl = NewResponseController(w);
        var (c, _, errΔ1) = ctl.Hijack();
        if (mode == http2Mode) {
            if (errΔ1 == default!) {
                Ꮡt.Errorf("ctl.Hijack = nil, want error"u8);
            }
            w.Header().Set(header, value);
            return;
        }
        if (errΔ1 != default!) {
            Ꮡt.Errorf("ctl.Hijack = _, _, %v, want _, _, nil"u8, errΔ1);
            return;
        }
        fmt.Fprintf(new http_test_package.net_ConnᴠWriter(c), "HTTP/1.0 200 OK\r\n%v: %v\r\nContent-Length: 0\r\n\r\n"u8, header, value);
    })));
    var (res, err) = (~cst).c.Get((~(~cst).ts).URL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        @string got = (~res).Header.Get(header);
        @string want = value; if (got != want) {
            Ꮡt.Errorf("response header %q = %q, want %q"u8, header, got, want);
        }
    }
}

public static void TestResponseControllerSetPastWriteDeadline(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testResponseControllerSetPastWriteDeadline(Δp0, Δp1));
}

internal static void testResponseControllerSetPastWriteDeadline(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            var ctl = NewResponseController(w);
            w.Write(slice<byte>("one"u8));
            {
                var errΔ1 = ctl.Flush(); if (errΔ1 != default!) {
                    Ꮡt.Errorf("before setting deadline: ctl.Flush() = %v, want nil"u8, errΔ1);
                }
            }
            {
                var errΔ2 = ctl.SetWriteDeadline(time.Now().Add((time.Duration)(-10000000000L))); if (errΔ2 != default!) {
                    Ꮡt.Errorf("ctl.SetWriteDeadline() = %v, want nil"u8, errΔ2);
                }
            }
            w.Write(slice<byte>("two"u8));
            {
                var errΔ3 = ctl.Flush(); if (errΔ3 == default!) {
                    Ꮡt.Errorf("after setting deadline: ctl.Flush() = nil, want non-nil"u8);
                }
            }
            // Connection errors are sticky, so resetting the deadline does not permit
            // making more progress. We might want to change this in the future, but verify
            // the current behavior for now. If we do change this, we'll want to make sure
            // to do so only for writing the response body, not headers.
            {
                var errΔ4 = ctl.SetWriteDeadline(time.Now().Add((time.Duration)(3600000000000L))); if (errΔ4 != default!) {
                    Ꮡt.Errorf("ctl.SetWriteDeadline() = %v, want nil"u8, errΔ4);
                }
            }
            w.Write(slice<byte>("three"u8));
            {
                var errΔ5 = ctl.Flush(); if (errΔ5 == default!) {
                    Ꮡt.Errorf("after resetting deadline: ctl.Flush() = nil, want non-nil"u8);
                }
            }
        })));
        var (res, err) = (~cst).c.Get((~(~cst).ts).URL);
        if (err != default!) {
            Ꮡt.Fatalf("unexpected connection error: %v"u8, err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        var (b, _) = io.ReadAll((~res).Body);
        if (((sstring)b) != "one"u8) {
            Ꮡt.Errorf("unexpected body: %q"u8, ((@string)b));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestResponseControllerSetFutureWriteDeadline(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testResponseControllerSetFutureWriteDeadline(Δp0, Δp1));
}

internal static void testResponseControllerSetFutureWriteDeadline(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var errc = new channel<error>(1);
        var startwritec = new channel<EmptyStruct>(0);
        var errcʗ1 = errc;
        var startwritecʗ1 = startwritec;
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            var ctl = NewResponseController(w);
            w.WriteHeader(200);
            {
                var errΔ1 = ctl.Flush(); if (errΔ1 != default!) {
                    Ꮡt.Errorf("ctl.Flush() = %v, want nil"u8, errΔ1);
                }
            }
            ᐸꟷ(startwritecʗ1); // don't set the deadline until the client reads response headers
            {
                var errΔ2 = ctl.SetWriteDeadline(time.Now().Add(1 * time.Millisecond)); if (errΔ2 != default!) {
                    Ꮡt.Errorf("ctl.SetWriteDeadline() = %v, want nil"u8, errΔ2);
                }
            }
            var (_, errΔ3) = io.Copy(new http_test_package.http_ResponseWriterᴠWriter(w), ((neverEnding)(rune)'a'));
            errcʗ1.ᐸꟷ(errΔ3);
        })));
        var (res, err) = (~cst).c.Get((~(~cst).ts).URL);
        builtin.close(startwritec);
        if (err != default!) {
            Ꮡt.Fatalf("unexpected connection error: %v"u8, err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        (_, err) = io.Copy(io.Discard, (~res).Body);
        if (err == default!) {
            Ꮡt.Errorf("client reading from truncated request body: got nil error, want non-nil"u8);
        }
        err = ᐸꟷ(errc); // io.Copy error
        if (!errors.Is(err, os.ErrDeadlineExceeded)) {
            Ꮡt.Errorf("server timed out writing request body: got err %v; want os.ErrDeadlineExceeded"u8, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestResponseControllerSetPastReadDeadline(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testResponseControllerSetPastReadDeadline(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string textFooˢ = "text/foo"u8;

internal static void testResponseControllerSetPastReadDeadline(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var readc = new channel<EmptyStruct>(0);
        var donec = new channel<EmptyStruct>(0);
        var donecʗ1 = donec;
        var readcʗ1 = readc;
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => builtin.close(ᴛ1), donecʗ1, ref ᒐ);
                var ctl = NewResponseController(w);
                var b = new slice<byte>(3);
                var (n, errΔ1) = io.ReadFull((~r).Body, b);
                b = b[..(int)(n)];
                if (errΔ1 != default! || ((sstring)b) != "one"u8) {
                    Ꮡt.Errorf("before setting read deadline: Read = %v, %q, want nil, %q"u8, errΔ1, ((@string)b), oneˢ);
                    return;
                }
                {
                    var errΔ2 = ctl.SetReadDeadline(time.Now()); if (errΔ2 != default!) {
                        Ꮡt.Errorf("ctl.SetReadDeadline() = %v, want nil"u8, errΔ2);
                        return;
                    }
                }
                (b, errΔ1) = io.ReadAll((~r).Body);
                if (errΔ1 == default! || ((sstring)b) != ""u8) {
                    Ꮡt.Errorf("after setting read deadline: Read = %q, nil, want error"u8, ((@string)b));
                }
                builtin.close(readcʗ1);
                // Connection errors are sticky, so resetting the deadline does not permit
                // making more progress. We might want to change this in the future, but verify
                // the current behavior for now.
                {
                    var errΔ3 = ctl.SetReadDeadline(new time.Time(nil)); if (errΔ3 != default!) {
                        Ꮡt.Errorf("ctl.SetReadDeadline() = %v, want nil"u8, errΔ3);
                        return;
                    }
                }
                (b, errΔ1) = io.ReadAll((~r).Body);
                if (errΔ1 == default!) {
                    Ꮡt.Errorf("after resetting read deadline: Read = %q, nil, want error"u8, ((@string)b));
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
                        Ꮡt.Errorf("server handler unexpectedly exited without closing readc"u8);
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
        var (res, err) = (~cst).c.Post((~(~cst).ts).URL, textFooˢ, new io.PipeReaderжReader(pr));
        if (err == default!) {
            var resʗ1 = res;
            defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestResponseControllerSetFutureReadDeadline(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testResponseControllerSetFutureReadDeadline(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string responseBodyˢ = "response body"u8;
internal static readonly @string textApocryphalˢ = "text/apocryphal"u8;

internal static void testResponseControllerSetFutureReadDeadline(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        @string respBody = responseBodyˢ;
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> req) => {
            var ctl = NewResponseController(w);
            {
                var errΔ1 = ctl.SetReadDeadline(time.Now().Add(1 * time.Millisecond)); if (errΔ1 != default!) {
                    Ꮡt.Errorf("ctl.SetReadDeadline() = %v, want nil"u8, errΔ1);
                }
            }
            var (_, errΔ2) = io.Copy(io.Discard, (~req).Body);
            if (!errors.Is(errΔ2, os.ErrDeadlineExceeded)) {
                Ꮡt.Errorf("server timed out reading request body: got err %v; want os.ErrDeadlineExceeded"u8, errΔ2);
            }
            w.Write(slice<byte>(respBody));
        })));
        var (pr, pw) = io.Pipe();
        var (res, err) = (~cst).c.Post((~(~cst).ts).URL, textApocryphalˢ, new io.PipeReaderжReader(pr));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        (var got, err) = io.ReadAll((~res).Body);
        if (((sstring)got) != respBody || err != default!) {
            Ꮡt.Errorf("client read response body: %q, %v; want %q, nil"u8, ((@string)got), err, respBody);
        }
        pw.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] partial struct wrapWriter {
    public global::go.net.http_package.ResponseWriter ResponseWriter;
}

internal static Δhttp.ResponseWriter Unwrap(this wrapWriter w) {
    return w.ResponseWriter;
}

public static void TestWrappedResponseController(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testWrappedResponseController(Δp0, Δp1));
}

internal static void testWrappedResponseController(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            w = new wrapWriter(w);
            var ctl = NewResponseController(w);
            {
                var errΔ1 = ctl.Flush(); if (errΔ1 != default!) {
                    Ꮡt.Errorf("ctl.Flush() = %v, want nil"u8, errΔ1);
                }
            }
            {
                var errΔ2 = ctl.SetReadDeadline(new time.Time(nil)); if (errΔ2 != default!) {
                    Ꮡt.Errorf("ctl.SetReadDeadline() = %v, want nil"u8, errΔ2);
                }
            }
            {
                var errΔ3 = ctl.SetWriteDeadline(new time.Time(nil)); if (errΔ3 != default!) {
                    Ꮡt.Errorf("ctl.SetWriteDeadline() = %v, want nil"u8, errΔ3);
                }
            }
        })));
        var (res, err) = (~cst).c.Get((~(~cst).ts).URL);
        if (err != default!) {
            Ꮡt.Fatalf("unexpected connection error: %v"u8, err);
        }
        io.Copy(io.Discard, (~res).Body);
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestResponseControllerEnableFullDuplex(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testResponseControllerEnableFullDuplex(Δp0, Δp1));
}

internal static void testResponseControllerEnableFullDuplex(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> req) => {
            var ctl = NewResponseController(w);
            {
                var errΔ1 = ctl.EnableFullDuplex(); if (errΔ1 != default!) {
                    // TODO: Drop test for HTTP/2 when x/net is updated to support
                    // EnableFullDuplex. Since HTTP/2 supports full duplex by default,
                    // the rest of the test is fine; it's just the EnableFullDuplex call
                    // that fails.
                    if (mode != http2Mode) {
                        Ꮡt.Errorf("ctl.EnableFullDuplex() = %v, want nil"u8, errΔ1);
                    }
                }
            }
            w.WriteHeader(200);
            ctl.Flush();
            while (ᐧ) {
                array<byte> buf = new(1);
                var (n, errΔ2) = (~req).Body.Read(buf[..]);
                if (n != 1 || errΔ2 != default!) {
                    break;
                }
                w.Write(buf[..]);
                ctl.Flush();
            }
        })));
        var (pr, pw) = io.Pipe();
        var (res, err) = (~cst).c.Post((~(~cst).ts).URL, textApocryphalˢ, new io.PipeReaderжReader(pr));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        for (var i = (byte)0; i < 10; i++) {
            {
                var (_, errΔ3) = pw.Write(new byte[]{i}.slice()); if (errΔ3 != default!) {
                    Ꮡt.Fatalf("Write: %v"u8, errΔ3);
                }
            }
            array<byte> buf = new(1);
            {
                var (n, errΔ4) = (~res).Body.Read(buf[..]); if (n != 1 || errΔ4 != default!) {
                    Ꮡt.Fatalf("Read: %v, %v"u8, n, errΔ4);
                }
            }
            if (buf[0] != i) {
                Ꮡt.Fatalf("read byte %v, want %v"u8, buf[0], i);
            }
        }
        pw.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestIssue58237(ж<testing.T> Ꮡt) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), http2Mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> req) => {
        var ctl = NewResponseController(w);
        {
            var errΔ1 = ctl.SetReadDeadline(time.Now().Add(1 * time.Millisecond)); if (errΔ1 != default!) {
                Ꮡt.Errorf("ctl.SetReadDeadline() = %v, want nil"u8, errΔ1);
            }
        }
        time.Sleep(10 * time.Millisecond);
    })));
    var (res, err) = (~cst).c.Get((~(~cst).ts).URL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~res).Body.Close();
}

} // end http_test_package
