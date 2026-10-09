// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using context = context_package;
using errors = errors_package;
using fmt = fmt_package;
using Δio = io_package;
using Δos = os_package;
using Δruntime = runtime_package;
using Δsync = sync_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.net_package;

partial class net_internal_test_package {

[GoInit] internal static void initΔ2() {
    // Install a hook to ensure that a 1ns timeout will always
    // be exceeded by the time Dial gets to the relevant system call.
    //
    // Without this, systems with a very large timer granularity — such as
    // Windows — may be able to accept connections without measurably exceeding
    // even an implausibly short deadline.
    testHookStepTime = () => {
        var now = time.Now();
        while (time.Since(now) == 0) {
            time.Sleep(1 * time.ΔNanosecond);
        }
    };
}

// Tests that dial timeouts, deadlines in the past work.
// timeout over deadline
// timeout over deadline

partial struct dialTimeoutTestsᴛ1 /*dyn*/ {
    internal time.Duration initialTimeout;
    internal time.Duration initialDelta; // for deadline
}
internal static slice<dialTimeoutTestsᴛ1> dialTimeoutTests = new dialTimeoutTestsᴛ1[]{
    new((time.Duration)(-5000000000L), 0),
    new(0, (time.Duration)(-5000000000L)),
    new((time.Duration)(-5000000000L), (time.Duration)(5000000000L)),
    new((time.Duration)(-9223372036854775808L), 0),
    new(0, (time.Duration)(-9223372036854775808L)),
    new(1 * time.Millisecond, 0),
    new(0, 1 * time.Millisecond),
    new(1 * time.Millisecond, (time.Duration)(5000000000L))
}.slice();

public static void TestDialTimeout(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8) {
            Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
        }

        Ꮡt.Parallel();
        var ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var lnʗ1 = ln;
        defer(() => {
            {
                var err = lnʗ1.Close(); if (err != default!) {
                    Ꮡt.Error(err);
                }
            }
        }, ref ᒐ);
        foreach (var (_, vᴛ1) in dialTimeoutTests) {
            ref var tt = ref heap(new dialTimeoutTestsᴛ1(), out var Ꮡtt);
            tt = vᴛ1;

            var lnʗ2 = ln;
            var ttʗ1 = tt;
            Ꮡt.Run(fmt.Sprintf("%v/%v"u8, tt.initialTimeout, tt.initialDelta), (ж<testing.T> tΔ1) => {
                // We don't run these subtests in parallel because we don't know how big
                // the kernel's accept queue is, and we don't want to accidentally saturate
                // it with concurrent calls. (That could cause the Dial to fail with
                // ECONNREFUSED or ECONNRESET instead of a timeout error.)
                ref var d = ref heap<global::go.net_package.Dialer>(out var Ꮡd);
                d = new Dialer(Timeout: ttʗ1.initialTimeout);
                var delta = ttʗ1.initialDelta;
                time.Time beforeDial = default!;
                time.Time afterDial = default!;
                error err = default!;
                while (ᐧ) {
                    if (delta != 0) {
                        d.Deadline = time.Now().Add(delta);
                    }
                    beforeDial = time.Now();
                    global::go.net_package.Conn c = default!;
                    (c, err) = Ꮡd.Dial(lnʗ2.Addr().Network(), lnʗ2.Addr().String());
                    afterDial = time.Now();
                    if (err != default!) {
                        break;
                    }
                    // Even though we're not calling Accept on the Listener, the kernel may
                    // spuriously accept connections on its behalf. If that happens, we will
                    // close the connection (to try to get it out of the kernel's accept
                    // queue) and try a shorter timeout.
                    //
                    // We assume that we will reach a point where the call actually does
                    // time out, although in theory (since this socket is on a loopback
                    // address) a sufficiently clever kernel could notice that no Accept
                    // call is pending and bypass both the queue and the timeout to return
                    // another error immediately.
                    tΔ1.Logf("closing spurious connection from Dial"u8);
                    c.Close();
                    if (delta <= 1 && d.Timeout <= 1) {
                        tΔ1.Fatalf("can't reduce Timeout or Deadline"u8);
                    }
                    if (delta > 1) {
                        delta /= 2;
                        tΔ1.Logf("reducing Deadline delta to %v"u8, delta);
                    }
                    if (d.Timeout > 1) {
                        d.Timeout /= 2;
                        tΔ1.Logf("reducing Timeout to %v"u8, d.Timeout);
                    }
                }
                if (d.Deadline.IsZero() || afterDial.Before(d.Deadline)) {
                    var delay = afterDial.Sub(beforeDial);
                    if (delay < d.Timeout) {
                        tΔ1.Errorf("Dial returned after %v; want ≥%v"u8, delay, d.Timeout);
                    }
                }
                {
                    var perr = parseDialError(err); if (perr != default!) {
                        tΔ1.Errorf("unexpected error from Dial: %v"u8, perr);
                    }
                }
                {
                    var (nerr, ok) = err._<ΔError>(ᐧ); if (!ok || !nerr.Timeout()) {
                        tΔ1.Errorf("Dial: %v, want timeout"u8, err);
                    }
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestDialTimeoutMaxDuration_type /*dyn*/ {
    internal time.Duration timeout;
    internal time.Duration delta; // for deadline
}

public static void TestDialTimeoutMaxDuration(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var lnʗ1 = ln;
        defer(() => {
            {
                var err = lnʗ1.Close(); if (err != default!) {
                    Ꮡt.Error(err);
                }
            }
        }, ref ᒐ);
        foreach (var (_, vᴛ1) in new TestDialTimeoutMaxDuration_type[]{ // Large timeouts that will overflow an int64 unix nanos.

            new((time.Duration)(9223372036854775807L), 0),
            new(0, (time.Duration)(9223372036854775807L))
        }.slice()) {
            ref var tt = ref heap(new TestDialTimeoutMaxDuration_type(), out var Ꮡtt);
            tt = vᴛ1;

            var lnʗ2 = ln;
            var ttʗ1 = tt;
            Ꮡt.Run(fmt.Sprintf("timeout=%s/delta=%s"u8, tt.timeout, tt.delta), (ж<testing.T> tΔ1) => {
                ref var d = ref heap<global::go.net_package.Dialer>(out var Ꮡd);
                d = new Dialer(Timeout: ttʗ1.timeout);
                if (ttʗ1.delta != 0) {
                    d.Deadline = time.Now().Add(ttʗ1.delta);
                }
                var (c, err) = Ꮡd.Dial(lnʗ2.Addr().Network(), lnʗ2.Addr().String());
                if (err != default!) {
                    tΔ1.Fatal(err);
                }
                {
                    var errΔ1 = c.Close(); if (errΔ1 != default!) {
                        tΔ1.Error(errΔ1);
                    }
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object wantedDeadlineExceededˢ = (@string)"wanted deadline exceeded"u8;

public static void TestAcceptTimeout(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Parallel();
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "plan9"u8) {
        Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
    }

    var timeouts = new time.Duration[]{
        (time.Duration)(-5000000000L),
        10 * time.Millisecond
    }.slice();
    foreach (var (_, timeout) in timeouts) {
        var timeoutΔ1 = timeout;
        Ꮡt.Run(fmt.Sprintf("%v"u8, timeoutΔ1), [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                tΔ1.Parallel();
                var ln = newLocalListener(new net_test_package.testing_TжTB(tΔ1), tcpˢ);
                var lnʗ1 = ln;
                defer(() => lnʗ1.Close(), ref ᒐ);
                if (timeoutΔ1 >= 0){
                } else {
                    // Don't dial the listener at all, so that Accept will hang.
                    // A deadline in the past should cause Accept to fail even if there are
                    // incoming connections available. Try to make one available before the
                    // call to Accept happens. (It's ok if the timing doesn't always work
                    // out that way, though: the test should pass regardless.)
                    var (ctx, cancel) = context.WithCancel(context.Background());
                    var dialDone = new channel<EmptyStruct>(0);
                    // Ensure that our background Dial returns before we close the listener.
                    // Otherwise, the listener's port could be reused immediately and we
                    // might spuriously Dial some completely unrelated socket, causing some
                    // other test to see an unexpected extra connection.
                    var cancelʗ1 = cancel;
                    var dialDoneʗ1 = dialDone;
                    defer(() => {
                        cancelʗ1();
                        ᐸꟷ(dialDoneʗ1);
                    }, ref ᒐ);
                    var ctxʗ1 = ctx;
                    var dialDoneʗ2 = dialDone;
                    var lnʗ2 = ln;
                    goǃ(() => {
                        GoFrame ᒐ = default;
                        try {
                            defer(ᴛ1 => builtin.close(ᴛ1), dialDoneʗ2, ref ᒐ);
                            ref var d = ref heap<global::go.net_package.Dialer>(out var Ꮡd);
                            d = new Dialer(nil);
                            var (cΔ1, errΔ1) = Ꮡd.DialContext(ctxʗ1, lnʗ2.Addr().Network(), lnʗ2.Addr().String());
                            if (errΔ1 != default!) {
                                // If the timing didn't work out, it is possible for this Dial
                                // to return an error (depending on the kernel's buffering behavior).
                                // In https://go.dev/issue/65240 we saw failures with ECONNREFUSED
                                // and ECONNRESET.
                                //
                                // What this test really cares about is the behavior of Accept, not
                                // Dial, so just log the error and ignore it.
                                tΔ1.Logf("DialContext: %v"u8, errΔ1);
                                return;
                            }
                            tΔ1.Logf("Dialed %v -> %v"u8, cΔ1.LocalAddr(), cΔ1.RemoteAddr());
                            cΔ1.Close();
                        }
                        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                        finally { ᒐ.Run(); }
                    });
                    time.Sleep(10 * time.Millisecond);
                }
                {
                    var errΔ2 = ln._<ж<global::go.net_package.TCPListener>>().SetDeadline(time.Now().Add(timeoutΔ1)); if (errΔ2 != default!) {
                        tΔ1.Fatal(errΔ2);
                    }
                }
                tΔ1.Logf("ln.SetDeadline(time.Now().Add(%v))"u8, timeoutΔ1);
                var (c, err) = ln.Accept();
                if (err == default!) {
                    c.Close();
                }
                tΔ1.Logf("ln.Accept: %v"u8, err);
                {
                    var perr = parseAcceptError(err); if (perr != default!) {
                        tΔ1.Error(perr);
                    }
                }
                if (!isDeadlineExceeded(err)) {
                    tΔ1.Error(wantedDeadlineExceededˢ);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

public static void TestAcceptTimeoutMustReturn(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8) {
            Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
        }

        var ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        {
            var errΔ1 = ln._<ж<global::go.net_package.TCPListener>>().SetDeadline(noDeadline); if (errΔ1 != default!) {
                Ꮡt.Error(errΔ1);
            }
        }
        {
            var errΔ2 = ln._<ж<global::go.net_package.TCPListener>>().SetDeadline(time.Now().Add(10 * time.Millisecond)); if (errΔ2 != default!) {
                Ꮡt.Error(errΔ2);
            }
        }
        var (c, err) = ln.Accept();
        if (err == default!) {
            c.Close();
        }
        {
            var perr = parseAcceptError(err); if (perr != default!) {
                Ꮡt.Error(perr);
            }
        }
        if (!isDeadlineExceeded(err)) {
            Ꮡt.Fatal(err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static partial void TestAcceptTimeoutMustNotReturn(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8) {
            Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
        }

        var ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var maxch = new channel<ж<time.Timer>>(0);
        var ch = new channel<error>(0);
        var chʗ1 = ch;
        var lnʗ2 = ln;
        var maxchʗ1 = maxch;
        goǃ(() => {
            {
                var errΔ1 = lnʗ2._<ж<global::go.net_package.TCPListener>>().SetDeadline(time.Now().Add((time.Duration)(-5000000000L))); if (errΔ1 != default!) {
                    Ꮡt.Error(errΔ1);
                }
            }
            {
                var errΔ2 = lnʗ2._<ж<global::go.net_package.TCPListener>>().SetDeadline(noDeadline); if (errΔ2 != default!) {
                    Ꮡt.Error(errΔ2);
                }
            }
            maxchʗ1.ᐸꟷ(time.NewTimer(100 * time.Millisecond));
            var (_, err) = lnʗ2.Accept();
            chʗ1.ᐸꟷ(err);
        });
        var max = ᐸꟷ(maxch);
        var maxʗ1 = max;
        defer(() => maxʗ1.Stop(), ref ᒐ);
        var selᴛ15 = ch;
        var selᴛ16 = (~max).C;
        switch (select(ᐸꟷ(selᴛ15, ꓸꓸꓸ), ᐸꟷ(selᴛ16, ꓸꓸꓸ))) {
        case 0 when selᴛ15.ꟷᐳ(out var err): {
            {
                var perr = parseAcceptError(err); if (perr != default!) {
                    Ꮡt.Error(perr);
                }
            }
            Ꮡt.Fatalf("expected Accept to not return, but it returned with %v"u8, err);
            break;
        }
        case 1 when selᴛ16.ꟷᐳ(out _): {
            ln.Close();
            ᐸꟷ(ch); // wait for tester goroutine to stop
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Tests that read deadlines work, even if there's data ready
// to be read.

partial struct readTimeoutTestsᴛ1 /*dyn*/ {
    internal time.Duration timeout;
    internal array<error> xerrs = new(2); // expected errors in transition
}
internal static slice<readTimeoutTestsᴛ1> readTimeoutTests = new readTimeoutTestsᴛ1[]{
    new((time.Duration)(-5000000000L), new error[]{Δos.ErrDeadlineExceeded, Δos.ErrDeadlineExceeded}.array()),
    new(50 * time.Millisecond, new error[]{default!, Δos.ErrDeadlineExceeded}.array())
}.slice();

// There is a very similar copy of this in os/timeout_test.go.
public static void TestReadTimeout(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var handler = (ж<localServer> lsΔ1, global::go.net_package.Listener ln) => {
            GoFrame ᒐ = default;
            try {
                var (cΔ1, errΔ1) = ln.Accept();
                if (errΔ1 != default!) {
                    Ꮡt.Error(errΔ1);
                    return;
                }
                cΔ1.Write(slice<byte>("READ TIMEOUT TEST"u8));
                var cʗ1 = cΔ1;
                defer(() => cʗ1.Close(), ref ᒐ);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        };
        var ls = newLocalServer(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var lsʗ1 = ls;
        defer(() => lsʗ1.teardown(), ref ᒐ);
        {
            var errΔ2 = ls.buildup(handler); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
        var (c, err) = Dial((~ls).Listener.Addr().Network(), (~ls).Listener.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cʗ2 = c;
        defer(() => cʗ2.Close(), ref ᒐ);
        foreach (var (i, vᴛ1) in readTimeoutTests) {
            var tt = vᴛ1.ΔClone();

            {
                var errΔ3 = c.SetReadDeadline(time.Now().Add(tt.timeout)); if (errΔ3 != default!) {
                    Ꮡt.Fatalf("#%d: %v"u8, i, errΔ3);
                }
            }
            array<byte> b = new(1);
            foreach (var (j, xerr) in tt.xerrs.ΔRangeSnapshot()) {
                while (ᐧ) {
                    var (n, errΔ4) = c.Read(b[..]);
                    if (xerr != default!) {
                        {
                            var perr = parseReadError(errΔ4); if (perr != default!) {
                                Ꮡt.Errorf("#%d/%d: %v"u8, i, j, perr);
                            }
                        }
                        if (!isDeadlineExceeded(errΔ4)) {
                            Ꮡt.Fatalf("#%d/%d: %v"u8, i, j, errΔ4);
                        }
                    }
                    if (errΔ4 == default!) {
                        time.Sleep(tt.timeout / 3);
                        continue;
                    }
                    if (n != 0) {
                        Ꮡt.Fatalf("#%d/%d: read %d; want 0"u8, i, j, n);
                    }
                    break;
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// There is a very similar copy of this in os/timeout_test.go.
public static partial void TestReadTimeoutMustNotReturn(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8) {
            Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
        }

        var ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var (c, err) = Dial(ln.Addr().Network(), ln.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        var maxch = new channel<ж<time.Timer>>(0);
        var ch = new channel<error>(0);
        var cʗ2 = c;
        var chʗ1 = ch;
        var maxchʗ1 = maxch;
        goǃ(() => {
            {
                var errΔ1 = cʗ2.SetDeadline(time.Now().Add((time.Duration)(-5000000000L))); if (errΔ1 != default!) {
                    Ꮡt.Error(errΔ1);
                }
            }
            {
                var errΔ2 = cʗ2.SetWriteDeadline(time.Now().Add((time.Duration)(-5000000000L))); if (errΔ2 != default!) {
                    Ꮡt.Error(errΔ2);
                }
            }
            {
                var errΔ3 = cʗ2.SetReadDeadline(noDeadline); if (errΔ3 != default!) {
                    Ꮡt.Error(errΔ3);
                }
            }
            maxchʗ1.ᐸꟷ(time.NewTimer(100 * time.Millisecond));
            ref var b = ref heap(new array<byte>(1), out var Ꮡb);
            var (_, errΔ4) = cʗ2.Read(b[..]);
            chʗ1.ᐸꟷ(errΔ4);
        });
        var max = ᐸꟷ(maxch);
        var maxʗ1 = max;
        defer(() => maxʗ1.Stop(), ref ᒐ);
        var selᴛ17 = ch;
        var selᴛ18 = (~max).C;
        switch (select(ᐸꟷ(selᴛ17, ꓸꓸꓸ), ᐸꟷ(selᴛ18, ꓸꓸꓸ))) {
        case 0 when selᴛ17.ꟷᐳ(out var errΔ5): {
            {
                var perr = parseReadError(errΔ5); if (perr != default!) {
                    Ꮡt.Error(perr);
                }
            }
            Ꮡt.Fatalf("expected Read to not return, but it returned with %v"u8, errΔ5);
            break;
        }
        case 1 when selᴛ18.ꟷᐳ(out _): {
            c.Close();
            var errΔ6 = ᐸꟷ(ch); // wait for tester goroutine to stop
            {
                var perr = parseReadError(errΔ6); if (perr != default!) {
                    Ꮡt.Error(perr);
                }
            }
            {
                var (nerr, ok) = errΔ6._<ΔError>(ᐧ); if (!ok || nerr.Timeout() || nerr.Temporary()) {
                    Ꮡt.Fatal(errΔ6);
                }
            }
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Tests that read deadlines work, even if there's data ready
// to be read.
internal static slice<readTimeoutTestsᴛ1> readFromTimeoutTests = new readTimeoutTestsᴛ1[]{
    new((time.Duration)(-5000000000L), new error[]{Δos.ErrDeadlineExceeded, Δos.ErrDeadlineExceeded}.array()),
    new(50 * time.Millisecond, new error[]{default!, Δos.ErrDeadlineExceeded}.array())
}.slice();

public static void TestReadFromTimeout(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var ch = new channel<global::go.net_package.ΔAddr>(0);
        defer(ᴛ1 => builtin.close(ᴛ1), ch, ref ᒐ);
        var chʗ1 = ch;
        var handler = (ж<localPacketServer> lsΔ1, global::go.net_package.PacketConn cΔ1) => {
            {
                var (dst, ok) = ᐸꟷ(chʗ1, ꟷ); if (ok) {
                    cΔ1.WriteTo(slice<byte>("READFROM TIMEOUT TEST"u8), dst);
                }
            }
        };
        var ls = newLocalPacketServer(new net_test_package.testing_TжTB(Ꮡt), udpˢ);
        var lsʗ1 = ls;
        defer(() => lsʗ1.teardown(), ref ᒐ);
        {
            var errΔ1 = ls.buildup(handler); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        var (host, _, err) = SplitHostPort((~ls).PacketConn.LocalAddr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var c, err) = ListenPacket((~ls).PacketConn.LocalAddr().Network(), JoinHostPort(host, "0"u8));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        ch.ᐸꟷ(c.LocalAddr());
        foreach (var (i, vᴛ1) in readFromTimeoutTests) {
            var tt = vᴛ1.ΔClone();

            {
                var errΔ2 = c.SetReadDeadline(time.Now().Add(tt.timeout)); if (errΔ2 != default!) {
                    Ꮡt.Fatalf("#%d: %v"u8, i, errΔ2);
                }
            }
            array<byte> b = new(1);
            foreach (var (j, xerr) in tt.xerrs.ΔRangeSnapshot()) {
                while (ᐧ) {
                    var (n, _, errΔ3) = c.ReadFrom(b[..]);
                    if (xerr != default!) {
                        {
                            var perr = parseReadError(errΔ3); if (perr != default!) {
                                Ꮡt.Errorf("#%d/%d: %v"u8, i, j, perr);
                            }
                        }
                        if (!isDeadlineExceeded(errΔ3)) {
                            Ꮡt.Fatalf("#%d/%d: %v"u8, i, j, errΔ3);
                        }
                    }
                    if (errΔ3 == default!) {
                        time.Sleep(tt.timeout / 3);
                        continue;
                    }
                    {
                        var (nerr, ok) = errΔ3._<ΔError>(ᐧ); if (ok && nerr.Timeout() && n != 0) {
                            Ꮡt.Fatalf("#%d/%d: read %d; want 0"u8, i, j, n);
                        }
                    }
                    break;
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Tests that write deadlines work, even if there's buffer
// space available to write.
internal static slice<readTimeoutTestsᴛ1> writeTimeoutTests = new readTimeoutTestsᴛ1[]{
    new((time.Duration)(-5000000000L), new error[]{Δos.ErrDeadlineExceeded, Δos.ErrDeadlineExceeded}.array()),
    new(10 * time.Millisecond, new error[]{default!, Δos.ErrDeadlineExceeded}.array())
}.slice();

// There is a very similar copy of this in os/timeout_test.go.
public static void TestWriteTimeout(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();
        var ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        foreach (var (i, vᴛ1) in writeTimeoutTests) {
            var tt = vᴛ1.ΔClone();

            var (c, err) = Dial(ln.Addr().Network(), ln.Addr().String());
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            var cʗ1 = c;
            defer(() => cʗ1.Close(), ref ᒐ);
            {
                var errΔ1 = c.SetWriteDeadline(time.Now().Add(tt.timeout)); if (errΔ1 != default!) {
                    Ꮡt.Fatalf("#%d: %v"u8, i, errΔ1);
                }
            }
            foreach (var (j, xerr) in tt.xerrs.ΔRangeSnapshot()) {
                while (ᐧ) {
                    var (n, errΔ2) = c.Write(slice<byte>("WRITE TIMEOUT TEST"u8));
                    if (xerr != default!) {
                        {
                            var perr = parseWriteError(errΔ2); if (perr != default!) {
                                Ꮡt.Errorf("#%d/%d: %v"u8, i, j, perr);
                            }
                        }
                        if (!isDeadlineExceeded(errΔ2)) {
                            Ꮡt.Fatalf("#%d/%d: %v"u8, i, j, errΔ2);
                        }
                    }
                    if (errΔ2 == default!) {
                        time.Sleep(tt.timeout / 3);
                        continue;
                    }
                    if (n != 0) {
                        Ꮡt.Fatalf("#%d/%d: wrote %d; want 0"u8, i, j, n);
                    }
                    break;
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// There is a very similar copy of this in os/timeout_test.go.
public static partial void TestWriteTimeoutMustNotReturn(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8) {
            Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
        }

        var ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var (c, err) = Dial(ln.Addr().Network(), ln.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        var maxch = new channel<ж<time.Timer>>(0);
        var ch = new channel<error>(0);
        var cʗ2 = c;
        var chʗ1 = ch;
        var maxchʗ1 = maxch;
        goǃ(() => {
            {
                var errΔ1 = cʗ2.SetDeadline(time.Now().Add((time.Duration)(-5000000000L))); if (errΔ1 != default!) {
                    Ꮡt.Error(errΔ1);
                }
            }
            {
                var errΔ2 = cʗ2.SetReadDeadline(time.Now().Add((time.Duration)(-5000000000L))); if (errΔ2 != default!) {
                    Ꮡt.Error(errΔ2);
                }
            }
            {
                var errΔ3 = cʗ2.SetWriteDeadline(noDeadline); if (errΔ3 != default!) {
                    Ꮡt.Error(errΔ3);
                }
            }
            maxchʗ1.ᐸꟷ(time.NewTimer(100 * time.Millisecond));
            ref var b = ref heap(new array<byte>(1024), out var Ꮡb);
            while (ᐧ) {
                {
                    var (_, errΔ4) = cʗ2.Write(b[..]); if (errΔ4 != default!) {
                        chʗ1.ᐸꟷ(errΔ4);
                        break;
                    }
                }
            }
        });
        var max = ᐸꟷ(maxch);
        var maxʗ1 = max;
        defer(() => maxʗ1.Stop(), ref ᒐ);
        var selᴛ19 = ch;
        var selᴛ20 = (~max).C;
        switch (select(ᐸꟷ(selᴛ19, ꓸꓸꓸ), ᐸꟷ(selᴛ20, ꓸꓸꓸ))) {
        case 0 when selᴛ19.ꟷᐳ(out var errΔ5): {
            {
                var perr = parseWriteError(errΔ5); if (perr != default!) {
                    Ꮡt.Error(perr);
                }
            }
            Ꮡt.Fatalf("expected Write to not return, but it returned with %v"u8, errΔ5);
            break;
        }
        case 1 when selᴛ20.ꟷᐳ(out _): {
            c.Close();
            var errΔ6 = ᐸꟷ(ch); // wait for tester goroutine to stop
            {
                var perr = parseWriteError(errΔ6); if (perr != default!) {
                    Ꮡt.Error(perr);
                }
            }
            {
                var (nerr, ok) = errΔ6._<ΔError>(ᐧ); if (!ok || nerr.Timeout() || nerr.Temporary()) {
                    Ꮡt.Fatal(errΔ6);
                }
            }
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestWriteToTimeout(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();
        var c1 = newLocalPacketListener(new net_test_package.testing_TжTB(Ꮡt), udpˢ);
        var c1ʗ1 = c1;
        defer(() => c1ʗ1.Close(), ref ᒐ);
        var (host, _, err) = SplitHostPort(c1.LocalAddr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var timeouts = new time.Duration[]{
            (time.Duration)(-5000000000L),
            10 * time.Millisecond
        }.slice();
        foreach (var (_, timeout) in timeouts) {
            var c1ʗ2 = c1;
            Ꮡt.Run(fmt.Sprint(timeout), (ж<testing.T> tΔ1) => {
                GoFrame ᒐ = default;
                try {
                    var (c2, errΔ1) = ListenPacket(c1ʗ2.LocalAddr().Network(), JoinHostPort(host, "0"u8));
                    if (errΔ1 != default!) {
                        tΔ1.Fatal(errΔ1);
                    }
                    var c2ʗ1 = c2;
                    defer(() => c2ʗ1.Close(), ref ᒐ);
                    {
                        var errΔ2 = c2.SetWriteDeadline(time.Now().Add(timeout)); if (errΔ2 != default!) {
                            tΔ1.Fatalf("SetWriteDeadline: %v"u8, errΔ2);
                        }
                    }
                    var backoff = 1 * time.Millisecond;
                    nint nDeadlineExceeded = 0;
                    for (nint j = 0; nDeadlineExceeded < 2; j++) {
                        var (n, errΔ3) = c2.WriteTo(slice<byte>("WRITETO TIMEOUT TEST"u8), c1ʗ2.LocalAddr());
                        tΔ1.Logf("#%d: WriteTo: %d, %v"u8, j, n, errΔ3);
                        if (errΔ3 == default! && timeout >= 0 && nDeadlineExceeded == 0) {
                            // If the timeout is nonnegative, some number of WriteTo calls may
                            // succeed before the timeout takes effect.
                            tΔ1.Logf("WriteTo succeeded; sleeping %v"u8, timeout / 3);
                            time.Sleep(timeout / 3);
                            continue;
                        }
                        if (isENOBUFS(errΔ3)) {
                            tΔ1.Logf("WriteTo: %v"u8, errΔ3);
                            // We're looking for a deadline exceeded error, but if the kernel's
                            // network buffers are saturated we may see ENOBUFS instead (see
                            // https://go.dev/issue/49930). Give it some time to unsaturate.
                            time.Sleep(backoff);
                            backoff *= 2;
                            continue;
                        }
                        {
                            var perr = parseWriteError(errΔ3); if (perr != default!) {
                                tΔ1.Errorf("failed to parse error: %v"u8, perr);
                            }
                        }
                        if (!isDeadlineExceeded(errΔ3)) {
                            tΔ1.Errorf("error is not 'deadline exceeded'"u8);
                        }
                        if (n != 0) {
                            tΔ1.Errorf("unexpectedly wrote %d bytes"u8, n);
                        }
                        if (!tΔ1.Failed()) {
                            tΔ1.Logf("WriteTo timed out as expected"u8);
                        }
                        nDeadlineExceeded++;
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static time.Duration minDynamicTimeout => /* 1 * time.Millisecond */ 1000000;
internal static time.Duration maxDynamicTimeout => /* 4 * time.Second */ 4000000000;

// timeoutUpperBound returns the maximum time that we expect a timeout of
// duration d to take to return the caller.
internal static time.Duration timeoutUpperBound(time.Duration d) {
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "openbsd"u8 || exprᴛ1 == "netbsd"u8) {
        return d * 3 / 2;
    }

    // NetBSD and OpenBSD seem to be unable to reliably hit deadlines even when
    // the absolute durations are long.
    // In https://build.golang.org/log/c34f8685d020b98377dd4988cd38f0c5bd72267e,
    // we observed that an openbsd-amd64-68 builder took 4.090948779s for a
    // 2.983020682s timeout (37.1% overhead).
    // (See https://go.dev/issue/50189 for further detail.)
    // Give them lots of slop to compensate.
    // Other platforms seem to hit their deadlines more reliably,
    // at least when they are long enough to cover scheduling jitter.
    return d * 11 / 10;
}

// nextTimeout returns the next timeout to try after an operation took the given
// actual duration with a timeout shorter than that duration.
internal static (time.Duration next, bool ok) nextTimeout(time.Duration actual) {
    time.Duration next = default!;

    if (actual >= maxDynamicTimeout) {
        return (maxDynamicTimeout, false);
    }
    // Since the previous attempt took actual, we can't expect to beat that
    // duration by any significant margin. Try the next attempt with an arbitrary
    // factor above that, so that our growth curve is at least exponential.
    next = actual * 5 / 4;
    if (next > maxDynamicTimeout) {
        return (maxDynamicTimeout, true);
    }
    return (next, true);
}

// There is a very similar copy of this in os/timeout_test.go.
public static void TestReadTimeoutFluctuation(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var (c, err) = Dial(ln.Addr().Network(), ln.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        var d = minDynamicTimeout;
        var b = new slice<byte>(256);
        while (ᐧ) {
            Ꮡt.Logf("SetReadDeadline(+%v)"u8, d);
            var t0 = time.Now();
            var deadline = t0.Add(d);
            {
                err = c.SetReadDeadline(deadline); if (err != default!) {
                    Ꮡt.Fatalf("SetReadDeadline(%v): %v"u8, deadline, err);
                }
            }
            nint n = default!;
            (n, err) = c.Read(b);
            var t1 = time.Now();
            if (n != 0 || err == default! || !err._<ΔError>().Timeout()) {
                Ꮡt.Errorf("Read did not return (0, timeout): (%d, %v)"u8, n, err);
            }
            {
                var perr = parseReadError(err); if (perr != default!) {
                    Ꮡt.Error(perr);
                }
            }
            if (!isDeadlineExceeded(err)) {
                Ꮡt.Errorf("Read error is not DeadlineExceeded: %v"u8, err);
            }
            var actual = t1.Sub(t0);
            if (t1.Before(deadline)) {
                Ꮡt.Errorf("Read took %s; expected at least %s"u8, actual, d);
            }
            if (Ꮡt.Failed()) {
                return;
            }
            {
                var want = timeoutUpperBound(d); if (actual > want) {
                    var (next, ok) = nextTimeout(actual);
                    if (!ok) {
                        Ꮡt.Fatalf("Read took %s; expected at most %v"u8, actual, want);
                    }
                    // Maybe this machine is too slow to reliably schedule goroutines within
                    // the requested duration. Increase the timeout and try again.
                    Ꮡt.Logf("Read took %s (expected %s); trying with longer timeout"u8, actual, d);
                    d = next;
                    continue;
                }
            }
            break;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// There is a very similar copy of this in os/timeout_test.go.
public static void TestReadFromTimeoutFluctuation(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var c1 = newLocalPacketListener(new net_test_package.testing_TжTB(Ꮡt), udpˢ);
        var c1ʗ1 = c1;
        defer(() => c1ʗ1.Close(), ref ᒐ);
        var (c2, err) = Dial(c1.LocalAddr().Network(), c1.LocalAddr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var c2ʗ1 = c2;
        defer(() => c2ʗ1.Close(), ref ᒐ);
        var d = minDynamicTimeout;
        var b = new slice<byte>(256);
        while (ᐧ) {
            Ꮡt.Logf("SetReadDeadline(+%v)"u8, d);
            var t0 = time.Now();
            var deadline = t0.Add(d);
            {
                err = c2.SetReadDeadline(deadline); if (err != default!) {
                    Ꮡt.Fatalf("SetReadDeadline(%v): %v"u8, deadline, err);
                }
            }
            nint n = default!;
            (n, _, err) = c2._<PacketConn>().ReadFrom(b);
            var t1 = time.Now();
            if (n != 0 || err == default! || !err._<ΔError>().Timeout()) {
                Ꮡt.Errorf("ReadFrom did not return (0, timeout): (%d, %v)"u8, n, err);
            }
            {
                var perr = parseReadError(err); if (perr != default!) {
                    Ꮡt.Error(perr);
                }
            }
            if (!isDeadlineExceeded(err)) {
                Ꮡt.Errorf("ReadFrom error is not DeadlineExceeded: %v"u8, err);
            }
            var actual = t1.Sub(t0);
            if (t1.Before(deadline)) {
                Ꮡt.Errorf("ReadFrom took %s; expected at least %s"u8, actual, d);
            }
            if (Ꮡt.Failed()) {
                return;
            }
            {
                var want = timeoutUpperBound(d); if (actual > want) {
                    var (next, ok) = nextTimeout(actual);
                    if (!ok) {
                        Ꮡt.Fatalf("ReadFrom took %s; expected at most %s"u8, actual, want);
                    }
                    // Maybe this machine is too slow to reliably schedule goroutines within
                    // the requested duration. Increase the timeout and try again.
                    Ꮡt.Logf("ReadFrom took %s (expected %s); trying with longer timeout"u8, actual, d);
                    d = next;
                    continue;
                }
            }
            break;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestWriteTimeoutFluctuation(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8) {
            Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
        }

        var ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var (c, err) = Dial(ln.Addr().Network(), ln.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        var d = minDynamicTimeout;
        while (ᐧ) {
            Ꮡt.Logf("SetWriteDeadline(+%v)"u8, d);
            var t0 = time.Now();
            var deadline = t0.Add(d);
            {
                var errΔ1 = c.SetWriteDeadline(deadline); if (errΔ1 != default!) {
                    Ꮡt.Fatalf("SetWriteDeadline(%v): %v"u8, deadline, errΔ1);
                }
            }
            int64 n = default!;
            error errΔ2 = default!;
            while (ᐧ) {
                nint dn = default!;
                (dn, errΔ2) = c.Write(slice<byte>("TIMEOUT TRANSMITTER"u8));
                n += (int64)dn;
                if (errΔ2 != default!) {
                    break;
                }
            }
            var t1 = time.Now();
            // Inv: err != nil
            if (!errΔ2._<ΔError>().Timeout()) {
                Ꮡt.Fatalf("Write did not return (any, timeout): (%d, %v)"u8, n, errΔ2);
            }
            {
                var perr = parseWriteError(errΔ2); if (perr != default!) {
                    Ꮡt.Error(perr);
                }
            }
            if (!isDeadlineExceeded(errΔ2)) {
                Ꮡt.Errorf("Write error is not DeadlineExceeded: %v"u8, errΔ2);
            }
            var actual = t1.Sub(t0);
            if (t1.Before(deadline)) {
                Ꮡt.Errorf("Write took %s; expected at least %s"u8, actual, d);
            }
            if (Ꮡt.Failed()) {
                return;
            }
            {
                var want = timeoutUpperBound(d); if (actual > want) {
                    if (n > 0){
                        // SetWriteDeadline specifies a time “after which I/O operations fail
                        // instead of blocking”. However, the kernel's send buffer is not yet
                        // full, we may be able to write some arbitrary (but finite) number of
                        // bytes to it without blocking.
                        Ꮡt.Logf("Wrote %d bytes into send buffer; retrying until buffer is full"u8, n);
                        if (d <= maxDynamicTimeout / 2) {
                            // We don't know how long the actual write loop would have taken if
                            // the buffer were full, so just guess and double the duration so that
                            // the next attempt can make twice as much progress toward filling it.
                            d *= 2;
                        }
                    } else 
                    {
                        var (next, ok) = nextTimeout(actual); if (!ok){
                            Ꮡt.Fatalf("Write took %s; expected at most %s"u8, actual, want);
                        } else {
                            // Maybe this machine is too slow to reliably schedule goroutines within
                            // the requested duration. Increase the timeout and try again.
                            Ꮡt.Logf("Write took %s (expected %s); trying with longer timeout"u8, actual, d);
                            d = next;
                        }
                    }
                    continue;
                }
            }
            break;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// There is a very similar copy of this in os/timeout_test.go.
public static void TestVariousDeadlines(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    testVariousDeadlines(Ꮡt);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"skipping in short mode"u8;

// There is a very similar copy of this in os/timeout_test.go.
public static void TestVariousDeadlines1Proc(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        // Cannot use t.Parallel - modifies global GOMAXPROCS.
        if (testing.Short()) {
            Ꮡt.Skip(skippingInShortModeˢ);
        }
        defer(Δruntime.GOMAXPROCS, Δruntime.GOMAXPROCS(1), ref ᒐ);
        testVariousDeadlines(Ꮡt);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// There is a very similar copy of this in os/timeout_test.go.
public static void TestVariousDeadlines4Proc(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        // Cannot use t.Parallel - modifies global GOMAXPROCS.
        if (testing.Short()) {
            Ꮡt.Skip(skippingInShortModeˢ);
        }
        defer(Δruntime.GOMAXPROCS, Δruntime.GOMAXPROCS(4), ref ᒐ);
        testVariousDeadlines(Ꮡt);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testVariousDeadlines(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var handler = (ж<localServer> lsΔ1, global::go.net_package.Listener ln) => {
            while (ᐧ) {
                var (c, err) = ln.Accept();
                if (err != default!) {
                    break;
                }
                c.Read(new slice<byte>(1)); // wait for client to close connection
                c.Close();
            }
        };
        var ls = newLocalServer(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var lsʗ1 = ls;
        defer(() => lsʗ1.teardown(), ref ᒐ);
        {
            var err = ls.buildup(handler); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
        foreach (var (_, timeout) in new time.Duration[]{
            1 * time.ΔNanosecond,
            2 * time.ΔNanosecond,
            5 * time.ΔNanosecond,
            50 * time.ΔNanosecond,
            100 * time.ΔNanosecond,
            200 * time.ΔNanosecond,
            500 * time.ΔNanosecond,
            750 * time.ΔNanosecond,
            1 * time.Microsecond,
            5 * time.Microsecond,
            25 * time.Microsecond,
            250 * time.Microsecond,
            500 * time.Microsecond,
            1 * time.Millisecond,
            5 * time.Millisecond,
            100 * time.Millisecond,
            250 * time.Millisecond,
            500 * time.Millisecond,
            1 * time.ΔSecond
        }.slice()) {
            nint numRuns = 3;
            if (testing.Short()) {
                numRuns = 1;
                if (timeout > 500 * time.Microsecond) {
                    continue;
                }
            }
            for (nint run = 0; run < numRuns; run++) {
                @string name = fmt.Sprintf("%v %d/%d"u8, timeout, run, numRuns);
                Ꮡt.Log(name);
                var (c, err) = Dial((~ls).Listener.Addr().Network(), (~ls).Listener.Addr().String());
                if (err != default!) {
                    Ꮡt.Fatal(err);
                }
                var t0 = time.Now();
                {
                    var errΔ1 = c.SetDeadline(t0.Add(timeout)); if (errΔ1 != default!) {
                        Ꮡt.Error(errΔ1);
                    }
                }
                (var n, err) = Δio.Copy(Δio.Discard, new net_test_package.net_ConnᴠReader(c));
                var dt = time.Since(t0);
                c.Close();
                {
                    var (nerr, ok) = err._<ΔError>(ᐧ); if (ok && nerr.Timeout()){
                        Ꮡt.Logf("%v: good timeout after %v; %d bytes"u8, name, dt, n);
                    } else {
                        Ꮡt.Fatalf("%v: Copy = %d, %v; want timeout"u8, name, n, err);
                    }
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// TestReadWriteProlongedTimeout tests concurrent deadline
// modification. Known to cause data races in the past.
public static void TestReadWriteProlongedTimeout(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8) {
            Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
        }

        var handler = [MethodImpl(MethodImplOptions.NoInlining)] (ж<localServer> lsΔ1, global::go.net_package.Listener ln) => {
            GoFrame ᒐ = default;
            try {
                var (cΔ1, errΔ1) = ln.Accept();
                if (errΔ1 != default!) {
                    Ꮡt.Error(errΔ1);
                    return;
                }
                var cʗ1 = cΔ1;
                defer(() => cʗ1.Close(), ref ᒐ);
                ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
                Ꮡwg.Add(2);
                var cʗ2 = cΔ1;
                goǃ(() => {
                    GoFrame ᒐ = default;
                    try {
                        defer(Ꮡwg.Done, ref ᒐ);
                        ref var bΔ1 = ref heap(new array<byte>(1), out var ᏑbΔ1);
                        while (ᐧ) {
                            {
                                var errΔ2 = cʗ2.SetReadDeadline(time.Now().Add(time.ΔHour)); if (errΔ2 != default!) {
                                    {
                                        var perr = parseCommonError(errΔ2); if (perr != default!) {
                                            Ꮡt.Error(perr);
                                        }
                                    }
                                    Ꮡt.Error(errΔ2);
                                    return;
                                }
                            }
                            {
                                var (_, errΔ3) = cʗ2.Read(bΔ1[..]); if (errΔ3 != default!) {
                                    {
                                        var perr = parseReadError(errΔ3); if (perr != default!) {
                                            Ꮡt.Error(perr);
                                        }
                                    }
                                    return;
                                }
                            }
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                });
                var cʗ3 = cΔ1;
                goǃ(() => {
                    GoFrame ᒐ = default;
                    try {
                        defer(Ꮡwg.Done, ref ᒐ);
                        ref var bΔ2 = ref heap(new array<byte>(1), out var ᏑbΔ2);
                        while (ᐧ) {
                            {
                                var errΔ4 = cʗ3.SetWriteDeadline(time.Now().Add(time.ΔHour)); if (errΔ4 != default!) {
                                    {
                                        var perr = parseCommonError(errΔ4); if (perr != default!) {
                                            Ꮡt.Error(perr);
                                        }
                                    }
                                    Ꮡt.Error(errΔ4);
                                    return;
                                }
                            }
                            {
                                var (_, errΔ5) = cʗ3.Write(bΔ2[..]); if (errΔ5 != default!) {
                                    {
                                        var perr = parseWriteError(errΔ5); if (perr != default!) {
                                            Ꮡt.Error(perr);
                                        }
                                    }
                                    return;
                                }
                            }
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                });
                Ꮡwg.Wait();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        };
        var ls = newLocalServer(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var lsʗ1 = ls;
        defer(() => lsʗ1.teardown(), ref ᒐ);
        {
            var errΔ6 = ls.buildup(handler); if (errΔ6 != default!) {
                Ꮡt.Fatal(errΔ6);
            }
        }
        var (c, err) = Dial((~ls).Listener.Addr().Network(), (~ls).Listener.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cʗ4 = c;
        defer(() => cʗ4.Close(), ref ᒐ);
        array<byte> b = new(1);
        for (nint i = 0; i < 1000; i++) {
            c.Write(b[..]);
            c.Read(b[..]);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// There is a very similar copy of this in os/timeout_test.go.
public static partial void TestReadWriteDeadlineRace(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();
        nint N = 1000;
        if (testing.Short()) {
            N = 50;
        }
        var ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var (c, err) = Dial(ln.Addr().Network(), ln.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(3);
        var cʗ2 = c;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                var tic = time.NewTicker(2 * time.Microsecond);
                var ticʗ1 = tic;
                defer(ticʗ1.Stop, ref ᒐ);
                for (nint i = 0; i < N; i++) {
                    {
                        var errΔ1 = cʗ2.SetReadDeadline(time.Now().Add(2 * time.Microsecond)); if (errΔ1 != default!) {
                            {
                                var perr = parseCommonError(errΔ1); if (perr != default!) {
                                    Ꮡt.Error(perr);
                                }
                            }
                            break;
                        }
                    }
                    {
                        var errΔ2 = cʗ2.SetWriteDeadline(time.Now().Add(2 * time.Microsecond)); if (errΔ2 != default!) {
                            {
                                var perr = parseCommonError(errΔ2); if (perr != default!) {
                                    Ꮡt.Error(perr);
                                }
                            }
                            break;
                        }
                    }
                    ᐸꟷ((~tic).C);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        var cʗ3 = c;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                ref var b = ref heap(new array<byte>(1), out var Ꮡb);
                for (nint i = 0; i < N; i++) {
                    cʗ3.Read(b[..]); // ignore possible timeout errors
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        var cʗ4 = c;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                ref var b = ref heap(new array<byte>(1), out var Ꮡb);
                for (nint i = 0; i < N; i++) {
                    cʗ4.Write(b[..]); // ignore possible timeout errors
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        Ꮡwg.Wait(); // wait for tester goroutine to stop
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 35367.
public static partial void TestConcurrentSetDeadline(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        const nint goroutines = 8;
        UntypedInt conns = 10;
        const nint tries = 100;
        ref var c = ref heap(new array<global::go.net_package.Conn>(10), out var Ꮡc);
        for (nint iᴛ1 = 0; iᴛ1 < conns; iᴛ1++) {
            var i = iᴛ1;
            error err = default!;
            (c[i], err) = Dial(ln.Addr().Network(), ln.Addr().String());
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            var cʗ1 = c;
            defer(() => cʗ1[i].Close(), ref ᒐ);
        }
        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(goroutines);
        ref var now = ref heap<time.Time>(out var Ꮡnow);
        now = time.Now();
        for (nint i = 0; i < goroutines; i++) {
            var cʗ2 = c;
            var nowʗ1 = now;
            goǃ((nint iΔ1) => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    // Make the deadlines steadily earlier,
                    // to trigger runtime adjusttimers calls.
                    for (nint j = tries; j > 0; j--) {
                        for (nint k = 0; k < conns; k++) {
                            cʗ2[k].SetReadDeadline(nowʗ1.Add((time.Duration)(7200000000000L) + ((time.Duration)(int64)(iΔ1 * j * k)) * time.ΔSecond));
                            cʗ2[k].SetWriteDeadline(nowʗ1.Add((time.Duration)(3600000000000L) + ((time.Duration)(int64)(iΔ1 * j * k)) * time.ΔSecond));
                        }
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            }, i);
        }
        Ꮡwg.Wait();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// isDeadlineExceeded reports whether err is or wraps os.ErrDeadlineExceeded.
// We also check that the error implements net.Error, and that the
// Timeout method returns true.
internal static bool isDeadlineExceeded(error err) {
    var (nerr, ok) = err._<ΔError>(ᐧ);
    if (!ok) {
        return false;
    }
    if (!nerr.Timeout()) {
        return false;
    }
    if (!errors.Is(err, Δos.ErrDeadlineExceeded)) {
        return false;
    }
    return true;
}

} // end net_internal_test_package
