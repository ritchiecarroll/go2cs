// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net.http;

using bytes = bytes_package;
using base64 = encoding.base64_package;
using fmt = fmt_package;
using profile = go.@internal.profile_package;
using testenv = go.@internal.testenv_package;
using io = io_package;
using http = go.net.http_package;
using httptest = go.net.http.httptest_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
using pprof = go.runtime.pprof_package;
using strings = strings_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using encoding;
using exec = go.os.exec_package;
using go.@internal;
using go.net;
using go.net.http;
using go.os;
using go.runtime;
using go.sync;
using path;
using static go.net.http.pprof_package;

partial class pprof_internal_test_package {

// TestDescriptions checks that the profile names under runtime/pprof package
// have a key in the description map.
public static void TestDescriptions(ж<testing.T> Ꮡt) {
    foreach (var (_, p) in pprof.Profiles()) {
        var (_, ok) = profileDescriptions[p.Name(), ꟷ];
        if (ok != true) {
            Ꮡt.Errorf("%s does not exist in profileDescriptions map\n"u8, p.Name());
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getˢ = "GET"u8;

[GoType("dyn")] internal partial struct TestHandlers_testCases {
    internal @string path;
    internal http.HandlerFunc handler;
    internal nint statusCode;
    internal @string contentType;
    internal @string contentDisposition;
    internal slice<byte> resp;
}

public static void TestHandlers(ж<testing.T> Ꮡt) {
    var testCases = new TestHandlers_testCases[]{
        new("/debug/pprof/<script>scripty<script>"u8, Index, http.StatusNotFound, "text/plain; charset=utf-8"u8, ""u8, slice<byte>("Unknown profile\n"u8)),
        new("/debug/pprof/heap"u8, Index, http.StatusOK, "application/octet-stream"u8, @"attachment; filename=""heap"""u8, default!),
        new("/debug/pprof/heap?debug=1"u8, Index, http.StatusOK, "text/plain; charset=utf-8"u8, ""u8, default!),
        new("/debug/pprof/cmdline"u8, Cmdline, http.StatusOK, "text/plain; charset=utf-8"u8, ""u8, default!),
        new("/debug/pprof/profile?seconds=1"u8, Profile, http.StatusOK, "application/octet-stream"u8, @"attachment; filename=""profile"""u8, default!),
        new("/debug/pprof/symbol"u8, Symbol, http.StatusOK, "text/plain; charset=utf-8"u8, ""u8, default!),
        new("/debug/pprof/trace"u8, Trace, http.StatusOK, "application/octet-stream"u8, @"attachment; filename=""trace"""u8, default!),
        new("/debug/pprof/mutex"u8, Index, http.StatusOK, "application/octet-stream"u8, @"attachment; filename=""mutex"""u8, default!),
        new("/debug/pprof/block?seconds=1"u8, Index, http.StatusOK, "application/octet-stream"u8, @"attachment; filename=""block-delta"""u8, default!),
        new("/debug/pprof/goroutine?seconds=1"u8, Index, http.StatusOK, "application/octet-stream"u8, @"attachment; filename=""goroutine-delta"""u8, default!),
        new("/debug/pprof/"u8, Index, http.StatusOK, "text/html; charset=utf-8"u8, ""u8, slice<byte>("Types of profiles available:"u8))
    }.slice();
    foreach (var (_, vᴛ1) in testCases) {
        ref var tc = ref heap(new TestHandlers_testCases(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.path, (ж<testing.T> tΔ1) => {
            var req = httptest.NewRequest(getˢ, "http://example.com"u8 + tcʗ1.path, default!);
            var w = httptest.NewRecorder();
            tcʗ1.handler(new pprof_internal_test_package.httptest_ResponseRecorderжResponseWriter(w), req);
            var resp = w.Result();
            {
                nint got = resp.Value.StatusCode;
                nint want = tcʗ1.statusCode; if (got != want) {
                    tΔ1.Errorf("status code: got %d; want %d"u8, got, want);
                }
            }
            var (body, err) = io.ReadAll(new pprof_internal_test_package.io_ReadCloserᴠReader((~resp).Body));
            if (err != default!) {
                tΔ1.Errorf("when reading response body, expected non-nil err; got %v"u8, err);
            }
            {
                @string got = (~resp).Header.Get(xContentTypeOptionsˢ);
                @string want = nosniffˢ; if (got != want) {
                    tΔ1.Errorf("X-Content-Type-Options: got %q; want %q"u8, got, want);
                }
            }
            {
                @string got = (~resp).Header.Get(contentTypeˢ);
                @string want = tcʗ1.contentType; if (got != want) {
                    tΔ1.Errorf("Content-Type: got %q; want %q"u8, got, want);
                }
            }
            {
                @string got = (~resp).Header.Get(contentDispositionˢ);
                @string want = tcʗ1.contentDisposition; if (got != want) {
                    tΔ1.Errorf("Content-Disposition: got %q; want %q"u8, got, want);
                }
            }
            if ((~resp).StatusCode == http.StatusOK) {
                return;
            }
            {
                @string got = (~resp).Header.Get(xGoPprofˢ);
                @string want = "1"u8; if (got != want) {
                    tΔ1.Errorf("X-Go-Pprof: got %q; want %q"u8, got, want);
                }
            }
            if (!bytes.Equal(body, tcʗ1.resp)) {
                tΔ1.Errorf("response: got %q; want %q"u8, body, tcʗ1.resp);
            }
        });
    }
}

public static ж<uint32> ᏑSink = new StandardBox<uint32>(default(uint32));
public static ref uint32 Sink => ref ᏑSink.Value;

internal static void mutexHog1(ж<sync.Mutex> Ꮡmu1, ж<sync.Mutex> Ꮡmu2, time.Time start, time.Duration dt) {
    atomic.AddUint32(ᏑSink, 1);
    while (time.Since(start) < dt) {
        // When using gccgo the loop of mutex operations is
        // not preemptible. This can cause the loop to block a GC,
        // causing the time limits in TestDeltaContentionz to fail.
        // Since this loop is not very realistic, when using
        // gccgo add preemption points 100 times a second.
        var t1 = time.Now();
        while (time.Since(start) < dt && time.Since(t1) < 10 * time.Millisecond) {
            Ꮡmu1.Lock();
            Ꮡmu2.Lock();
            Ꮡmu1.Unlock();
            Ꮡmu2.Unlock();
        }
        if (runtime.Compiler == "gccgo") {
            runtime.Gosched();
        }
    }
}

// mutexHog2 is almost identical to mutexHog but we keep them separate
// in order to distinguish them with function names in the stack trace.
// We make them slightly different, using Sink, because otherwise
// gccgo -c opt will merge them.
internal static void mutexHog2(ж<sync.Mutex> Ꮡmu1, ж<sync.Mutex> Ꮡmu2, time.Time start, time.Duration dt) {
    atomic.AddUint32(ᏑSink, 2);
    while (time.Since(start) < dt) {
        // See comment in mutexHog.
        var t1 = time.Now();
        while (time.Since(start) < dt && time.Since(t1) < 10 * time.Millisecond) {
            Ꮡmu1.Lock();
            Ꮡmu2.Lock();
            Ꮡmu1.Unlock();
            Ꮡmu2.Unlock();
        }
        if (runtime.Compiler == "gccgo") {
            runtime.Gosched();
        }
    }
}

// mutexHog starts multiple goroutines that runs the given hogger function for the specified duration.
// The hogger function will be given two mutexes to lock & unlock.
internal static partial void mutexHog(time.Duration duration, Action<ж<sync.Mutex>, ж<sync.Mutex>, time.Time, time.Duration> hogger) {
    ref var start = ref heap<time.Time>(out var Ꮡstart);
    start = time.Now();
    var mu1 = @new<sync.Mutex>();
    var mu2 = @new<sync.Mutex>();
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(10);
    for (nint i = 0; i < 10; i++) {
        var mu1ʗ1 = mu1;
        var mu2ʗ1 = mu2;
        var startʗ1 = start;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                hogger(mu1ʗ1, mu2ʗ1, startʗ1, duration);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string armˢ = "arm"u8;
internal static readonly @string debugPprofMutexˢ = "/debug/pprof/mutex"u8;
internal static readonly @string mutexHog1ˢ = "mutexHog1"u8;
internal static readonly @string mutexHog2ˢ = "mutexHog2"u8;

public static partial void TestDeltaProfile(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (strings.HasPrefix(runtime.GOARCH, armˢ)) {
            testenv.SkipFlaky(new pprof_internal_test_package.testing_TжTB(Ꮡt), 50218);
        }
        nint rate = runtime.SetMutexProfileFraction(1);
        defer(() => {
            runtime.SetMutexProfileFraction(rate);
        }, ref ᒐ);
        // mutexHog1 will appear in non-delta mutex profile
        // if the mutex profile works.
        mutexHog(20 * time.Millisecond, mutexHog1);
        // If mutexHog1 does not appear in the mutex profile,
        // skip this test. Mutex profile is likely not working,
        // so is the delta profile.
        var (p, err) = query(debugPprofMutexˢ);
        if (err != default!) {
            Ꮡt.Skipf("mutex profile is unsupported: %v"u8, err);
        }
        if (!seen(p, mutexHog1ˢ)) {
            Ꮡt.Skipf("mutex profile is not working: %v"u8, p.OrTypedNil());
        }
        // causes mutexHog2 call stacks to appear in the mutex profile.
        var done = new channel<bool>(0);
        var doneʗ1 = done;
        goǃ(() => {
            while (ᐧ) {
                mutexHog(20 * time.Millisecond, mutexHog2);
                var selᴛ1 = doneʗ1;
                switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
                case 0 when selᴛ1.ꟷᐳ(out _): {
                    doneʗ1.ᐸꟷ(true);
                    return;
                }
                default: {
                    time.Sleep(10 * time.Millisecond);
                    break;
                }}
            }
        });
        var doneʗ2 = done;
        defer(() => {
            // cleanup the above goroutine.
            doneʗ2.ᐸꟷ(true);
            ᐸꟷ(doneʗ2); // wait for the goroutine to exit.
        }, ref ᒐ);
        foreach (var (_, d) in new nint[]{1, 4, 16, 32}.slice()) {
            @string endpoint = fmt.Sprintf("/debug/pprof/mutex?seconds=%d"u8, d);
            var (pΔ1, errΔ1) = query(endpoint);
            if (errΔ1 != default!) {
                Ꮡt.Fatalf("failed to query %q: %v"u8, endpoint, errΔ1);
            }
            if (!seen(pΔ1, mutexHog1ˢ) && seen(pΔ1, mutexHog2ˢ) && (~pΔ1).DurationNanos > 0) {
                break; // pass
            }
            if (d == 32) {
                Ꮡt.Errorf("want mutexHog2 but no mutexHog1 in the profile, and non-zero p.DurationNanos, got %v"u8, pΔ1.OrTypedNil());
            }
        }
        (p, err) = query(debugPprofMutexˢ);
        if (err != default!) {
            Ꮡt.Fatalf("failed to query mutex profile: %v"u8, err);
        }
        if (!seen(p, mutexHog1ˢ) || !seen(p, mutexHog2ˢ)) {
            Ꮡt.Errorf("want both mutexHog1 and mutexHog2 in the profile, got %v"u8, p.OrTypedNil());
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static ж<httptest.Server> srv = httptest.NewServer(default!);

internal static (ж<profile.Profile>, error) query(@string endpoint) {
    @string url = (~srv).URL + endpoint;
    var (r, err) = http.Get(url);
    if (err != default!) {
        return (default!, fmt.Errorf("failed to fetch %q: %v"u8, url, err));
    }
    if ((~r).StatusCode != http.StatusOK) {
        return (default!, fmt.Errorf("failed to fetch %q: %v"u8, url, (~r).Status));
    }
    (var b, err) = io.ReadAll(new pprof_internal_test_package.io_ReadCloserᴠReader((~r).Body));
    (~r).Body.Close();
    if (err != default!) {
        return (default!, fmt.Errorf("failed to read and parse the result from %q: %v"u8, url, err));
    }
    return profile.Parse(new pprof_internal_test_package.bytes_BufferжReader(bytes.NewBuffer(b)));
}

// seen returns true if the profile includes samples whose stacks include
// the specified function name (fname).
internal static bool seen(ж<profile.Profile> Ꮡp, @string fname) {
    ref var p = ref Ꮡp.DerefOrNull();

    var locIDs = new map<ж<profile.Location>, bool>{};
    foreach (var (_, loc) in p.Location) {
        foreach (var (_, l) in (~loc).Line) {
            if (strings.Contains((~l.Function).Name, fname)) {
                locIDs[loc] = true;
                break;
            }
        }
    }
    foreach (var (_, sample) in p.Sample) {
        foreach (var (_, loc) in (~sample).Location) {
            if (locIDs[loc]) {
                return true;
            }
        }
    }
    return false;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"skipping in -short mode"u8;
internal static readonly @string runˢ = "run"u8;
internal static readonly @string testdataˢ = "testdata"u8;
internal static readonly @string deltaMutexGoˢ = "delta_mutex.go"u8;
internal static readonly object pPeriodTypeGotNilWantNotˢ = (@string)"p.PeriodType got nil want not nil"u8;

// TestDeltaProfileEmptyBase validates that we still receive a valid delta
// profile even if the base contains no samples.
//
// Regression test for https://go.dev/issue/64566.
public static void TestDeltaProfileEmptyBase(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        // Delta profile collection has a 1s minimum.
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    testenv.MustHaveGoRun(new pprof_internal_test_package.testing_TжTB(Ꮡt));
    var (gotool, err) = testenv.GoTool();
    if (err != default!) {
        Ꮡt.Fatalf("error finding go tool: %v"u8, err);
    }
    (var @out, err) = testenv.Command(new pprof_internal_test_package.testing_TжTB(Ꮡt), gotool, runˢ, filepath.Join(testdataˢ, deltaMutexGoˢ)).CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("error running profile collection: %v\noutput: %s"u8, err, @out);
    }
    // Log the binary output for debugging failures.
    var b64 = new slice<byte>(base64.StdEncoding.EncodedLen(len(@out)));
    base64.StdEncoding.Encode(b64, @out);
    Ꮡt.Logf("Output in base64.StdEncoding: %s"u8, b64);
    (var p, err) = profile.Parse(new pprof_internal_test_package.bytes_ReaderжReader(bytes.NewReader(@out)));
    if (err != default!) {
        Ꮡt.Fatalf("Parse got err %v want nil"u8, err);
    }
    Ꮡt.Logf("Output as parsed Profile: %s"u8, p.OrTypedNil());
    if (len((~p).SampleType) != 2) {
        Ꮡt.Errorf("len(p.SampleType) got %d want 2"u8, len((~p).SampleType));
    }
    if ((~(~p).SampleType[0]).Type != "contentions"u8) {
        Ꮡt.Errorf(@"p.SampleType[0].Type got %q want ""contentions"""u8, (~(~p).SampleType[0]).Type);
    }
    if ((~(~p).SampleType[0]).Unit != "count"u8) {
        Ꮡt.Errorf(@"p.SampleType[0].Unit got %q want ""count"""u8, (~(~p).SampleType[0]).Unit);
    }
    if ((~(~p).SampleType[1]).Type != "delay"u8) {
        Ꮡt.Errorf(@"p.SampleType[1].Type got %q want ""delay"""u8, (~(~p).SampleType[1]).Type);
    }
    if ((~(~p).SampleType[1]).Unit != "nanoseconds"u8) {
        Ꮡt.Errorf(@"p.SampleType[1].Unit got %q want ""nanoseconds"""u8, (~(~p).SampleType[1]).Unit);
    }
    if ((~p).PeriodType == nil) {
        Ꮡt.Fatal(pPeriodTypeGotNilWantNotˢ);
    }
    if ((~(~p).PeriodType).Type != "contentions"u8) {
        Ꮡt.Errorf(@"p.PeriodType.Type got %q want ""contentions"""u8, (~(~p).PeriodType).Type);
    }
    if ((~(~p).PeriodType).Unit != "count"u8) {
        Ꮡt.Errorf(@"p.PeriodType.Unit got %q want ""count"""u8, (~(~p).PeriodType).Unit);
    }
}

} // end pprof_internal_test_package
