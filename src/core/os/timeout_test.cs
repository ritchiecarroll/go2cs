// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !js && !plan9 && !wasip1
namespace go;

using fmt = fmt_package;
using Δio = io_package;
using rand = math.rand_package;
using Δos = os_package;
using Δruntime = runtime_package;
using Δsync = sync_package;
using Δtesting = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using math;
using static go.os_internal_test_package;

partial class os_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ostestˢ = "ostest"u8;

public static void TestNonpollableDeadline(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // On BSD systems regular files seem to be pollable,
        // so just run this test on Linux and Windows.
        if (Δruntime.GOOS != "linux"u8 && Δruntime.GOOS != "windows"u8) {
            Ꮡt.Skipf("skipping on %s"u8, Δruntime.GOOS);
        }
        Ꮡt.Parallel();

        var (f, err) = Δos.CreateTemp(""u8, ostestˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        defer(Δos.Remove, f.Name(), ref ᒐ);
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        var deadline = time.Now().Add((time.Duration)(10000000000L));
        {
            var errΔ1 = f.SetDeadline(deadline); if (!AreEqual(errΔ1, Δos.ErrNoDeadline)) {
                Ꮡt.Errorf("SetDeadline on file returned %v, wanted %v"u8, errΔ1, Δos.ErrNoDeadline);
            }
        }
        {
            var errΔ2 = f.SetReadDeadline(deadline); if (!AreEqual(errΔ2, Δos.ErrNoDeadline)) {
                Ꮡt.Errorf("SetReadDeadline on file returned %v, wanted %v"u8, errΔ2, Δos.ErrNoDeadline);
            }
        }
        {
            var errΔ3 = f.SetWriteDeadline(deadline); if (!AreEqual(errΔ3, Δos.ErrNoDeadline)) {
                Ꮡt.Errorf("SetWriteDeadline on file returned %v, wanted %v"u8, errΔ3, Δos.ErrNoDeadline);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct pipeDeadlineTest {
    internal @string name;
    internal Func<ж<Δtesting.T>, (ж<Δos.File> r, ж<Δos.File> w)> create;
}

internal static slice<pipeDeadlineTest> pipeDeadlinesTestCases;

// noDeadline is a zero time.Time value, which cancels a deadline.
internal static time.Time noDeadline;

partial struct readTimeoutTestsᴛ1 /*dyn*/ {
    internal time.Duration timeout;
    internal array<error> xerrs = new(2); // expected errors in transition
}
internal static slice<readTimeoutTestsᴛ1> readTimeoutTests = new readTimeoutTestsᴛ1[]{
    // Tests that read deadlines work, even if there's data ready
    // to be read.
    new((time.Duration)(-5000000000L), new error[]{Δos.ErrDeadlineExceeded, Δos.ErrDeadlineExceeded}.array()),

    new(50 * time.Millisecond, new error[]{default!, Δos.ErrDeadlineExceeded}.array())
}.slice();

// There is a very similar copy of this in net/timeout_test.go.
public static void TestReadTimeout(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();

    foreach (var (_, vᴛ1) in pipeDeadlinesTestCases) {
        ref var tc = ref heap(new pipeDeadlineTest(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<Δtesting.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                tΔ1.Parallel();

                var (r, w) = tcʗ1.create(tΔ1);
                var rʗ1 = r;
                defer(() => rʗ1.Close(), ref ᒐ);
                var wʗ1 = w;
                defer(() => wʗ1.Close(), ref ᒐ);

                {
                    var (_, err) = w.Write(slice<byte>("READ TIMEOUT TEST"u8)); if (err != default!) {
                        tΔ1.Fatal(err);
                    }
                }

                foreach (var (i, vᴛ2) in readTimeoutTests) {
                    var tt = vᴛ2.ΔClone();

                    {
                        var err = r.SetReadDeadline(time.Now().Add(tt.timeout)); if (err != default!) {
                            tΔ1.Fatalf("#%d: %v"u8, i, err);
                        }
                    }
                    array<byte> b = new(1);
                    foreach (var (j, xerr) in tt.xerrs.ΔRangeSnapshot()) {
                        while (ᐧ) {
                            var (n, err) = r.Read(b[..]);
                            if (xerr != default!) {
                                if (!isDeadlineExceeded(err)) {
                                    tΔ1.Fatalf("#%d/%d: %v"u8, i, j, err);
                                }
                            }
                            if (err == default!) {
                                time.Sleep(tt.timeout / 3);
                                continue;
                            }
                            if (n != 0) {
                                tΔ1.Fatalf("#%d/%d: read %d; want 0"u8, i, j, n);
                            }
                            break;
                        }
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// There is a very similar copy of this in net/timeout_test.go.
public static void TestReadTimeoutMustNotReturn(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();

    foreach (var (_, vᴛ1) in pipeDeadlinesTestCases) {
        ref var tc = ref heap(new pipeDeadlineTest(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, [MethodImpl(MethodImplOptions.NoInlining)] (ж<Δtesting.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                tΔ1.Parallel();

                var (r, w) = tcʗ1.create(tΔ1);
                var rʗ1 = r;
                defer(() => rʗ1.Close(), ref ᒐ);
                var wʗ1 = w;
                defer(() => wʗ1.Close(), ref ᒐ);

                var max = time.NewTimer(100 * time.Millisecond);
                var maxʗ1 = max;
                defer(() => maxʗ1.Stop(), ref ᒐ);
                var ch = new channel<error>(0);
                var chʗ1 = ch;
                var rʗ2 = r;
                goǃ(() => {
                    {
                        var errΔ1 = rʗ2.SetDeadline(time.Now().Add((time.Duration)(-5000000000L))); if (errΔ1 != default!) {
                            tΔ1.Error(errΔ1);
                        }
                    }
                    {
                        var errΔ2 = rʗ2.SetWriteDeadline(time.Now().Add((time.Duration)(-5000000000L))); if (errΔ2 != default!) {
                            tΔ1.Error(errΔ2);
                        }
                    }
                    {
                        var errΔ3 = rʗ2.SetReadDeadline(noDeadline); if (errΔ3 != default!) {
                            tΔ1.Error(errΔ3);
                        }
                    }
                    ref var b = ref heap(new array<byte>(1), out var Ꮡb);
                    var (_, err) = rʗ2.Read(b[..]);
                    chʗ1.ᐸꟷ(err);
                });

                var selᴛ6 = ch;
                var selᴛ7 = (~max).C;
                switch (select(ᐸꟷ(selᴛ6, ꓸꓸꓸ), ᐸꟷ(selᴛ7, ꓸꓸꓸ))) {
                case 0 when selᴛ6.ꟷᐳ(out var err): {
                    tΔ1.Fatalf("expected Read to not return, but it returned with %v"u8, err);
                    break;
                }
                case 1 when selᴛ7.ꟷᐳ(out _): {
                    w.Close();
                    var errΔ1 = ᐸꟷ(ch); // wait for tester goroutine to stop
                    if (Δos.IsTimeout(errΔ1)) {
                        tΔ1.Fatal(errΔ1);
                    }
                    break;
                }}
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

internal static slice<readTimeoutTestsᴛ1> writeTimeoutTests = new readTimeoutTestsᴛ1[]{
    // Tests that write deadlines work, even if there's buffer
    // space available to write.
    new((time.Duration)(-5000000000L), new error[]{Δos.ErrDeadlineExceeded, Δos.ErrDeadlineExceeded}.array()),

    new(10 * time.Millisecond, new error[]{default!, Δos.ErrDeadlineExceeded}.array())
}.slice();

// There is a very similar copy of this in net/timeout_test.go.
public static void TestWriteTimeout(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();

    foreach (var (_, vᴛ1) in pipeDeadlinesTestCases) {
        ref var tc = ref heap(new pipeDeadlineTest(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<Δtesting.T> tΔ1) => {
            tΔ1.Parallel();

            foreach (var (i, vᴛ2) in writeTimeoutTests) {
                ref var tt = ref heap(new readTimeoutTestsᴛ1(), out var Ꮡtt);
                tt = vᴛ2.ΔClone();

                var tcʗ2 = tcʗ1;
                var ttʗ1 = tt;
                tΔ1.Run(fmt.Sprintf("#%d"u8, i), (ж<Δtesting.T> tΔ2) => {
                    GoFrame ᒐ = default;
                    try {
                        var (r, w) = tcʗ2.create(tΔ2);
                        var rʗ1 = r;
                        defer(() => rʗ1.Close(), ref ᒐ);
                        var wʗ1 = w;
                        defer(() => wʗ1.Close(), ref ᒐ);

                        {
                            var err = w.SetWriteDeadline(time.Now().Add(ttʗ1.timeout)); if (err != default!) {
                                tΔ2.Fatalf("%v"u8, err);
                            }
                        }
                        foreach (var (j, xerr) in ttʗ1.xerrs.ΔRangeSnapshot()) {
                            while (ᐧ) {
                                var (n, err) = w.Write(slice<byte>("WRITE TIMEOUT TEST"u8));
                                if (xerr != default!) {
                                    if (!isDeadlineExceeded(err)) {
                                        tΔ2.Fatalf("%d: %v"u8, j, err);
                                    }
                                }
                                if (err == default!) {
                                    time.Sleep(ttʗ1.timeout / 3);
                                    continue;
                                }
                                if (n != 0) {
                                    tΔ2.Fatalf("%d: wrote %d; want 0"u8, j, n);
                                }
                                break;
                            }
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                });
            }
        });
    }
}

// There is a very similar copy of this in net/timeout_test.go.
public static void TestWriteTimeoutMustNotReturn(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();

    foreach (var (_, vᴛ1) in pipeDeadlinesTestCases) {
        ref var tc = ref heap(new pipeDeadlineTest(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, [MethodImpl(MethodImplOptions.NoInlining)] (ж<Δtesting.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                tΔ1.Parallel();

                var (r, w) = tcʗ1.create(tΔ1);
                var rʗ1 = r;
                defer(() => rʗ1.Close(), ref ᒐ);
                var wʗ1 = w;
                defer(() => wʗ1.Close(), ref ᒐ);

                var max = time.NewTimer(100 * time.Millisecond);
                var maxʗ1 = max;
                defer(() => maxʗ1.Stop(), ref ᒐ);
                var ch = new channel<error>(0);
                var chʗ1 = ch;
                var wʗ2 = w;
                goǃ(() => {
                    {
                        var err = wʗ2.SetDeadline(time.Now().Add((time.Duration)(-5000000000L))); if (err != default!) {
                            tΔ1.Error(err);
                        }
                    }
                    {
                        var err = wʗ2.SetReadDeadline(time.Now().Add((time.Duration)(-5000000000L))); if (err != default!) {
                            tΔ1.Error(err);
                        }
                    }
                    {
                        var err = wʗ2.SetWriteDeadline(noDeadline); if (err != default!) {
                            tΔ1.Error(err);
                        }
                    }
                    ref var b = ref heap(new array<byte>(1), out var Ꮡb);
                    while (ᐧ) {
                        {
                            var (_, err) = wʗ2.Write(b[..]); if (err != default!) {
                                chʗ1.ᐸꟷ(err);
                                break;
                            }
                        }
                    }
                });

                var selᴛ8 = ch;
                var selᴛ9 = (~max).C;
                switch (select(ᐸꟷ(selᴛ8, ꓸꓸꓸ), ᐸꟷ(selᴛ9, ꓸꓸꓸ))) {
                case 0 when selᴛ8.ꟷᐳ(out var err): {
                    tΔ1.Fatalf("expected Write to not return, but it returned with %v"u8, err);
                    break;
                }
                case 1 when selᴛ9.ꟷᐳ(out _): {
                    r.Close();
                    var errΔ1 = ᐸꟷ(ch); // wait for tester goroutine to stop
                    if (Δos.IsTimeout(errΔ1)) {
                        tΔ1.Fatal(errΔ1);
                    }
                    break;
                }}
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// minDynamicTimeout is the minimum timeout to attempt for
// tests that automatically increase timeouts until success.
//
// Lower values may allow tests to succeed more quickly if the value is close
// to the true minimum, but may require more iterations (and waste more time
// and CPU power on failed attempts) if the timeout is too low.
internal static time.Duration minDynamicTimeout => /* 1 * time.Millisecond */ 1000000;

// maxDynamicTimeout is the maximum timeout to attempt for
// tests that automatically increase timeouts until success.
//
// This should be a strict upper bound on the latency required to hit a
// timeout accurately, even on a slow or heavily-loaded machine. If a test
// would increase the timeout beyond this value, the test fails.
internal static time.Duration maxDynamicTimeout => /* 4 * time.Second */ 4000000000;

// timeoutUpperBound returns the maximum time that we expect a timeout of
// duration d to take to return the caller.
internal static time.Duration timeoutUpperBound(time.Duration d) {
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "openbsd"u8 || exprᴛ1 == "netbsd"u8) {
        // NetBSD and OpenBSD seem to be unable to reliably hit deadlines even when
        // the absolute durations are long.
        // In https://build.golang.org/log/c34f8685d020b98377dd4988cd38f0c5bd72267e,
        // we observed that an openbsd-amd64-68 builder took 4.090948779s for a
        // 2.983020682s timeout (37.1% overhead).
        // (See https://go.dev/issue/50189 for further detail.)
        // Give them lots of slop to compensate.
        return d * 3 / 2;
    }
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
    return (min(next, maxDynamicTimeout), true);
}

// There is a very similar copy of this in net/timeout_test.go.
public static void TestReadTimeoutFluctuation(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();

    foreach (var (_, vᴛ1) in pipeDeadlinesTestCases) {
        ref var tc = ref heap(new pipeDeadlineTest(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<Δtesting.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                tΔ1.Parallel();

                var (r, w) = tcʗ1.create(tΔ1);
                var rʗ1 = r;
                defer(() => rʗ1.Close(), ref ᒐ);
                var wʗ1 = w;
                defer(() => wʗ1.Close(), ref ᒐ);

                var d = minDynamicTimeout;
                var b = new slice<byte>(256);
                while (ᐧ) {
                    tΔ1.Logf("SetReadDeadline(+%v)"u8, d);
                    var t0 = time.Now();
                    var deadline = t0.Add(d);
                    {
                        var errΔ1 = r.SetReadDeadline(deadline); if (errΔ1 != default!) {
                            tΔ1.Fatalf("SetReadDeadline(%v): %v"u8, deadline, errΔ1);
                        }
                    }
                    nint n = default!;
                    (n, var err) = r.Read(b);
                    var t1 = time.Now();

                    if (n != 0 || err == default! || !isDeadlineExceeded(err)) {
                        tΔ1.Errorf("Read did not return (0, timeout): (%d, %v)"u8, n, err);
                    }

                    var actual = t1.Sub(t0);
                    if (t1.Before(deadline)) {
                        tΔ1.Errorf("Read took %s; expected at least %s"u8, actual, d);
                    }
                    if (tΔ1.Failed()) {
                        return;
                    }
                    {
                        var want = timeoutUpperBound(d); if (actual > want) {
                            var (next, ok) = nextTimeout(actual);
                            if (!ok) {
                                tΔ1.Fatalf("Read took %s; expected at most %v"u8, actual, want);
                            }
                            // Maybe this machine is too slow to reliably schedule goroutines within
                            // the requested duration. Increase the timeout and try again.
                            tΔ1.Logf("Read took %s (expected %s); trying with longer timeout"u8, actual, d);
                            d = next;
                            continue;
                        }
                    }

                    break;
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// There is a very similar copy of this in net/timeout_test.go.
public static void TestWriteTimeoutFluctuation(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();

    foreach (var (_, vᴛ1) in pipeDeadlinesTestCases) {
        ref var tc = ref heap(new pipeDeadlineTest(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<Δtesting.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                tΔ1.Parallel();

                var (r, w) = tcʗ1.create(tΔ1);
                var rʗ1 = r;
                defer(() => rʗ1.Close(), ref ᒐ);
                var wʗ1 = w;
                defer(() => wʗ1.Close(), ref ᒐ);

                var d = minDynamicTimeout;
                while (ᐧ) {
                    tΔ1.Logf("SetWriteDeadline(+%v)"u8, d);
                    var t0 = time.Now();
                    var deadline = t0.Add(d);
                    {
                        var errΔ1 = w.SetWriteDeadline(deadline); if (errΔ1 != default!) {
                            tΔ1.Fatalf("SetWriteDeadline(%v): %v"u8, deadline, errΔ1);
                        }
                    }
                    int64 n = default!;
                    error err = default!;
                    while (ᐧ) {
                        nint dn = default!;
                        (dn, err) = w.Write(slice<byte>("TIMEOUT TRANSMITTER"u8));
                        n += (int64)dn;
                        if (err != default!) {
                            break;
                        }
                    }
                    var t1 = time.Now();
                    // Inv: err != nil
                    if (!isDeadlineExceeded(err)) {
                        tΔ1.Fatalf("Write did not return (any, timeout): (%d, %v)"u8, n, err);
                    }

                    var actual = t1.Sub(t0);
                    if (t1.Before(deadline)) {
                        tΔ1.Errorf("Write took %s; expected at least %s"u8, actual, d);
                    }
                    if (tΔ1.Failed()) {
                        return;
                    }
                    {
                        var want = timeoutUpperBound(d); if (actual > want) {
                            if (n > 0){
                                // SetWriteDeadline specifies a time “after which I/O operations fail
                                // instead of blocking”. However, the kernel's send buffer is not yet
                                // full, we may be able to write some arbitrary (but finite) number of
                                // bytes to it without blocking.
                                tΔ1.Logf("Wrote %d bytes into send buffer; retrying until buffer is full"u8, n);
                                if (d <= maxDynamicTimeout / 2) {
                                    // We don't know how long the actual write loop would have taken if
                                    // the buffer were full, so just guess and double the duration so that
                                    // the next attempt can make twice as much progress toward filling it.
                                    d *= 2;
                                }
                            } else 
                            {
                                var (next, ok) = nextTimeout(actual); if (!ok){
                                    tΔ1.Fatalf("Write took %s; expected at most %s"u8, actual, want);
                                } else {
                                    // Maybe this machine is too slow to reliably schedule goroutines within
                                    // the requested duration. Increase the timeout and try again.
                                    tΔ1.Logf("Write took %s (expected %s); trying with longer timeout"u8, actual, d);
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
        });
    }
}

// There is a very similar copy of this in net/timeout_test.go.
public static void TestVariousDeadlines(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    foreach (var (_, vᴛ1) in pipeDeadlinesTestCases) {
        ref var tc = ref heap(new pipeDeadlineTest(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<Δtesting.T> tΔ1) => {
            tΔ1.Parallel();
            testVariousDeadlines(tΔ1, tcʗ1.create);
        });
    }
}

// There is a very similar copy of this in net/timeout_test.go.
public static void TestVariousDeadlines1Proc(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Cannot use t.Parallel - modifies global GOMAXPROCS.
        if (Δtesting.Short()) {
            Ꮡt.Skip(skippingInShortModeˢ);
        }
        defer(Δruntime.GOMAXPROCS, Δruntime.GOMAXPROCS(1), ref ᒐ);
        foreach (var (_, vᴛ1) in pipeDeadlinesTestCases) {
            ref var tc = ref heap(new pipeDeadlineTest(), out var Ꮡtc);
            tc = vᴛ1;

            var tcʗ1 = tc;
            Ꮡt.Run(tc.name, (ж<Δtesting.T> tΔ1) => {
                tΔ1.Parallel();
                testVariousDeadlines(tΔ1, tcʗ1.create);
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// There is a very similar copy of this in net/timeout_test.go.
public static void TestVariousDeadlines4Proc(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Cannot use t.Parallel - modifies global GOMAXPROCS.
        if (Δtesting.Short()) {
            Ꮡt.Skip(skippingInShortModeˢ);
        }
        defer(Δruntime.GOMAXPROCS, Δruntime.GOMAXPROCS(4), ref ᒐ);
        foreach (var (_, vᴛ1) in pipeDeadlinesTestCases) {
            ref var tc = ref heap(new pipeDeadlineTest(), out var Ꮡtc);
            tc = vᴛ1;

            var tcʗ1 = tc;
            Ꮡt.Run(tc.name, (ж<Δtesting.T> tΔ1) => {
                tΔ1.Parallel();
                testVariousDeadlines(tΔ1, tcʗ1.create);
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct neverEnding /*num:byte*/;

internal static (nint, error) Read(this neverEnding b, slice<byte> p) {
    foreach (var i in range(p)) {
        p[i] = (byte)b;
    }
    return (len(p), default!);
}

internal partial struct testVariousDeadlines_result /*dyn*/ {
    internal int64 n;
    internal error err;
    internal time.Duration d;
}

internal static void testVariousDeadlines(ж<Δtesting.T> Ꮡt, Func<ж<Δtesting.T>, (ж<Δos.File>, ж<Δos.File>)> create) {
    void handler(ж<Δos.File> w, channel<testVariousDeadlines_result> pasvch) {
        // The writer, with no timeouts of its own,
        // sending bytes to clients as fast as it can.
        var t0 = time.Now();
        var (n, err) = Δio.Copy(new Δos.FileжWriter(w), (neverEnding)(rune)'a');
        var dt = time.Since(t0);
        pasvch.ᐸꟷ(new testVariousDeadlines_result(n, err, dt));
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
        if (Δtesting.Short()) {
            numRuns = 1;
            if (timeout > 500 * time.Microsecond) {
                continue;
            }
        }
        for (nint run = 0; run < numRuns; run++) {
            var handlerʗ1 = handler;
            Ꮡt.Run(fmt.Sprintf("%v-%d"u8, timeout, run + 1), [MethodImpl(MethodImplOptions.NoInlining)] (ж<Δtesting.T> tΔ1) => {
                GoFrame ᒐ = default;
                try {
                    var (r, w) = create(tΔ1);
                    var rʗ1 = r;
                    defer(() => rʗ1.Close(), ref ᒐ);
                    var wʗ1 = w;
                    defer(() => wʗ1.Close(), ref ᒐ);

                    var pasvch = new channel<testVariousDeadlines_result>(0);
                    var handlerʗ2 = handlerʗ1;
                    goǃ(handlerʗ2, w, pasvch);

                    var tooLong = (time.Duration)(5000000000L);
                    var max = time.NewTimer(tooLong);
                    var maxʗ1 = max;
                    defer(() => maxʗ1.Stop(), ref ᒐ);
                    var actvch = new channel<testVariousDeadlines_result>(0);
                    var actvchʗ1 = actvch;
                    var rʗ2 = r;
                    goǃ(() => {
                        ref var t0 = ref heap<time.Time>(out var Ꮡt0);
                        t0 = time.Now();
                        {
                            var errΔ1 = rʗ2.SetDeadline(t0.Add(timeout)); if (errΔ1 != default!) {
                                tΔ1.Error(errΔ1);
                            }
                        }
                        var (n, err) = Δio.Copy(Δio.Discard, new os_test_package.os_FileжReader(rʗ2));
                        var dt = time.Since(t0);
                        rʗ2.Close();
                        actvchʗ1.ᐸꟷ(new testVariousDeadlines_result(n, err, dt));
                    });

                    var selᴛ10 = actvch;
                    var selᴛ11 = (~max).C;
                    switch (select(ᐸꟷ(selᴛ10, ꓸꓸꓸ), ᐸꟷ(selᴛ11, ꓸꓸꓸ))) {
                    case 0 when selᴛ10.ꟷᐳ(out var res): {
                        if (isDeadlineExceeded(res.err)){
                            tΔ1.Logf("good client timeout after %v, reading %d bytes"u8, res.d, res.n);
                        } else {
                            tΔ1.Fatalf("client Copy = %d, %v; want timeout"u8, res.n, res.err);
                        }
                        break;
                    }
                    case 1 when selᴛ11.ꟷᐳ(out _): {
                        tΔ1.Fatalf("timeout (%v) waiting for client to timeout (%v) reading"u8, tooLong, timeout);
                        break;
                    }}

                    var selᴛ12 = pasvch;
                    var selᴛ13 = (~max).C;
                    switch (select(ᐸꟷ(selᴛ12, ꓸꓸꓸ), ᐸꟷ(selᴛ13, ꓸꓸꓸ))) {
                    case 0 when selᴛ12.ꟷᐳ(out var res): {
                        tΔ1.Logf("writer in %v wrote %d: %v"u8, res.d, res.n, res.err);
                        break;
                    }
                    case 1 when selᴛ13.ꟷᐳ(out _): {
                        tΔ1.Fatalf("timeout waiting for writer to finish writing"u8);
                        break;
                    }}
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object readReturnedNonTimeoutˢ = (@string)"Read returned non-timeout error"u8;
internal static readonly object writeReturnedNonTimeoutˢ = (@string)"Write returned non-timeout error"u8;

// There is a very similar copy of this in net/timeout_test.go.
public static void TestReadWriteDeadlineRace(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();

    nint N = 1000;
    if (Δtesting.Short()) {
        N = 50;
    }

    foreach (var (_, vᴛ1) in pipeDeadlinesTestCases) {
        ref var tc = ref heap(new pipeDeadlineTest(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, [MethodImpl(MethodImplOptions.NoInlining)] (ж<Δtesting.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                tΔ1.Parallel();

                var (r, w) = tcʗ1.create(tΔ1);
                var rʗ1 = r;
                defer(() => rʗ1.Close(), ref ᒐ);
                var wʗ1 = w;
                defer(() => wʗ1.Close(), ref ᒐ);

                ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
                Ꮡwg.Add(3);
                var rʗ2 = r;
                var wʗ2 = w;
                goǃ(() => {
                    GoFrame ᒐ = default;
                    try {
                        defer(Ꮡwg.Done, ref ᒐ);
                        var tic = time.NewTicker(2 * time.Microsecond);
                        var ticʗ1 = tic;
                        defer(ticʗ1.Stop, ref ᒐ);
                        for (nint i = 0; i < N; i++) {
                            {
                                var err = rʗ2.SetReadDeadline(time.Now().Add(2 * time.Microsecond)); if (err != default!) {
                                    break;
                                }
                            }
                            {
                                var err = wʗ2.SetWriteDeadline(time.Now().Add(2 * time.Microsecond)); if (err != default!) {
                                    break;
                                }
                            }
                            ᐸꟷ((~tic).C);
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                });
                var rʗ3 = r;
                goǃ(() => {
                    GoFrame ᒐ = default;
                    try {
                        defer(Ꮡwg.Done, ref ᒐ);
                        ref var b = ref heap(new array<byte>(1), out var Ꮡb);
                        for (nint i = 0; i < N; i++) {
                            var (_, err) = rʗ3.Read(b[..]);
                            if (err != default! && !isDeadlineExceeded(err)) {
                                tΔ1.Error(readReturnedNonTimeoutˢ, err);
                            }
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                });
                var wʗ3 = w;
                goǃ(() => {
                    GoFrame ᒐ = default;
                    try {
                        defer(Ꮡwg.Done, ref ᒐ);
                        ref var b = ref heap(new array<byte>(1), out var Ꮡb);
                        for (nint i = 0; i < N; i++) {
                            var (_, err) = wʗ3.Write(b[..]);
                            if (err != default! && !isDeadlineExceeded(err)) {
                                tΔ1.Error(writeReturnedNonTimeoutˢ, err);
                            }
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                });
                Ꮡwg.Wait(); // wait for tester goroutine to stop
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// TestRacyRead tests that it is safe to mutate the input Read buffer
// immediately after cancellation has occurred.
public static void TestRacyRead(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();

    foreach (var (_, vᴛ1) in pipeDeadlinesTestCases) {
        ref var tc = ref heap(new pipeDeadlineTest(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, [MethodImpl(MethodImplOptions.NoInlining)] (ж<Δtesting.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                tΔ1.Parallel();

                var (r, w) = tcʗ1.create(tΔ1);
                var rʗ1 = r;
                defer(() => rʗ1.Close(), ref ᒐ);
                var wʗ1 = w;
                defer(() => wʗ1.Close(), ref ᒐ);

                ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
                defer(Ꮡwg.Wait, ref ᒐ);

                goǃ((ᴛ1, ᴛ2) => Δio.Copy(ᴛ1, ᴛ2), new Δos.FileжWriter(w), new os_test_package.rand_RandжReader(rand.New(rand.NewSource(0))));

                r.SetReadDeadline(time.Now().Add(time.Millisecond));
                for (nint i = 0; i < 10; i++) {
                    Ꮡwg.Add(1);
                    var rʗ2 = r;
                    goǃ(() => {
                        GoFrame ᒐ = default;
                        try {
                            defer(Ꮡwg.Done, ref ᒐ);

                            var b1 = new slice<byte>(1024);
                            var b2 = new slice<byte>(1024);
                            for (nint j = 0; j < 100; j++) {
                                var (_, err) = rʗ2.Read(b1);
                                copy(b1, b2); // Mutate b1 to trigger potential race
                                if (err != default!) {
                                    if (!isDeadlineExceeded(err)) {
                                        tΔ1.Error(err);
                                    }
                                    rʗ2.SetReadDeadline(time.Now().Add(time.Millisecond));
                                }
                            }
                        }
                        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                        finally { ᒐ.Run(); }
                    });
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// TestRacyWrite tests that it is safe to mutate the input Write buffer
// immediately after cancellation has occurred.
public static void TestRacyWrite(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();

    foreach (var (_, vᴛ1) in pipeDeadlinesTestCases) {
        ref var tc = ref heap(new pipeDeadlineTest(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, [MethodImpl(MethodImplOptions.NoInlining)] (ж<Δtesting.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                tΔ1.Parallel();

                var (r, w) = tcʗ1.create(tΔ1);
                var rʗ1 = r;
                defer(() => rʗ1.Close(), ref ᒐ);
                var wʗ1 = w;
                defer(() => wʗ1.Close(), ref ᒐ);

                ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
                defer(Ꮡwg.Wait, ref ᒐ);

                goǃ((ᴛ1, ᴛ2) => Δio.Copy(ᴛ1, ᴛ2), Δio.Discard, new os_test_package.os_FileжReader(r));

                w.SetWriteDeadline(time.Now().Add(time.Millisecond));
                for (nint i = 0; i < 10; i++) {
                    Ꮡwg.Add(1);
                    var wʗ2 = w;
                    goǃ(() => {
                        GoFrame ᒐ = default;
                        try {
                            defer(Ꮡwg.Done, ref ᒐ);

                            var b1 = new slice<byte>(1024);
                            var b2 = new slice<byte>(1024);
                            for (nint j = 0; j < 100; j++) {
                                var (_, err) = wʗ2.Write(b1);
                                copy(b1, b2); // Mutate b1 to trigger potential race
                                if (err != default!) {
                                    if (!isDeadlineExceeded(err)) {
                                        tΔ1.Error(err);
                                    }
                                    wʗ2.SetWriteDeadline(time.Now().Add(time.Millisecond));
                                }
                            }
                        }
                        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                        finally { ᒐ.Run(); }
                    });
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

} // end os_test_package
