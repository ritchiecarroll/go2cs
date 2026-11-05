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
public static void TestDescriptions(ж<testing.T> t) {
    foreach (var (_, p) in pprof.Profiles()) {
        var (_, ok) = profileDescriptions[p.Name(), ꟷ];
        if (ok != true) {
            t.Errorf("%s does not exist in profileDescriptions map\n", p.Name());
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string getˢ => ref getˢᵇ;
private static readonly @string getˢᵇ = "GET"u8;

internal partial struct TestHandlers_testCases /*dyn*/ {
    internal @string path;
    internal http.HandlerFunc handler;
    internal nint statusCode;
    internal @string contentType;
    internal @string contentDisposition;
    internal slice<byte> resp;
}

public static void TestHandlers(ж<testing.T> t) {
    var testCases = slice<TestHandlers_testCases>([
        new("/debug/pprof/<script>scripty<script>", Index, http.StatusNotFound, "text/plain; charset=utf-8", "", slice<byte>("Unknown profile\n"u8)),
        new("/debug/pprof/heap", Index, http.StatusOK, "application/octet-stream", @"attachment; filename=""heap""", default),
        new("/debug/pprof/heap?debug=1", Index, http.StatusOK, "text/plain; charset=utf-8", "", default),
        new("/debug/pprof/cmdline", Cmdline, http.StatusOK, "text/plain; charset=utf-8", "", default),
        new("/debug/pprof/profile?seconds=1", Profile, http.StatusOK, "application/octet-stream", @"attachment; filename=""profile""", default),
        new("/debug/pprof/symbol", Symbol, http.StatusOK, "text/plain; charset=utf-8", "", default),
        new("/debug/pprof/trace", Trace, http.StatusOK, "application/octet-stream", @"attachment; filename=""trace""", default),
        new("/debug/pprof/mutex", Index, http.StatusOK, "application/octet-stream", @"attachment; filename=""mutex""", default),
        new("/debug/pprof/block?seconds=1", Index, http.StatusOK, "application/octet-stream", @"attachment; filename=""block-delta""", default),
        new("/debug/pprof/goroutine?seconds=1", Index, http.StatusOK, "application/octet-stream", @"attachment; filename=""goroutine-delta""", default),
        new("/debug/pprof/", Index, http.StatusOK, "text/html; charset=utf-8", "", slice<byte>("Types of profiles available:"u8))
    ]);
    foreach (var (_, vᴛ1) in testCases) {
        ref var tc = ref heap(new TestHandlers_testCases(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        t.Run(tc.path, (ж<testing.T> tΔ1) => {
            var req = httptest.NewRequest(getˢ, "http://example.com"u8 + tcʗ1.path, default);
            var w = httptest.NewRecorder();
            tcʗ1.handler(new pprof_internal_test_package.httptest_ResponseRecorderжResponseWriter(w), req);

            var resp = w.Result();
            {
                nint got = resp.StatusCode;
                nint want = tcʗ1.statusCode; if (got != want) {
                    tΔ1.Errorf("status code: got %d; want %d", got, want);
                }
            }

            var (body, err) = io.ReadAll(new pprof_internal_test_package.io_ReadCloserᴠReader(resp.Body));
            if (err != nil) {
                tΔ1.Errorf("when reading response body, expected non-nil err; got %v", err);
            }
            {
                @string got = resp.Header.Get(xContentTypeOptionsˢ);
                @string want = nosniffˢ; if (got != want) {
                    tΔ1.Errorf("X-Content-Type-Options: got %q; want %q", got, want);
                }
            }
            {
                @string got = resp.Header.Get(contentTypeˢ);
                @string want = tcʗ1.contentType; if (got != want) {
                    tΔ1.Errorf("Content-Type: got %q; want %q", got, want);
                }
            }
            {
                @string got = resp.Header.Get(contentDispositionˢ);
                @string want = tcʗ1.contentDisposition; if (got != want) {
                    tΔ1.Errorf("Content-Disposition: got %q; want %q", got, want);
                }
            }

            if (resp.StatusCode == http.StatusOK) {
                return;
            }
            {
                @string got = resp.Header.Get(xGoPprofˢ);
                @string want = "1"; if (got != want) {
                    tΔ1.Errorf("X-Go-Pprof: got %q; want %q", got, want);
                }
            }
            if (!bytes.Equal(body, tcʗ1.resp)) {
                tΔ1.Errorf("response: got %q; want %q", body, tcʗ1.resp);
            }
        });
    }
}

public static readonly ж<uint32> ᏑSink = new StaticBox<uint32>(default(uint32));
public static ref uint32 Sink => ref ((StaticBox<uint32>)ᏑSink).ValueSlot;

internal static void mutexHog1(ж<sync.Mutex> mu1, ж<sync.Mutex> mu2, time.Time start, time.Duration dt) {
    atomic.AddUint32(ᏑSink, 1);
    while (time.Since(start) < dt) {
        // When using gccgo the loop of mutex operations is
        // not preemptible. This can cause the loop to block a GC,
        // causing the time limits in TestDeltaContentionz to fail.
        // Since this loop is not very realistic, when using
        // gccgo add preemption points 100 times a second.
        var t1 = time.Now();
        while (time.Since(start) < dt && time.Since(t1) < 10 * time.Millisecond) {
            mu1.Lock();
            mu2.Lock();
            mu1.Unlock();
            mu2.Unlock();
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
internal static void mutexHog2(ж<sync.Mutex> mu1, ж<sync.Mutex> mu2, time.Time start, time.Duration dt) {
    atomic.AddUint32(ᏑSink, 2);
    while (time.Since(start) < dt) {
        // See comment in mutexHog.
        var t1 = time.Now();
        while (time.Since(start) < dt && time.Since(t1) < 10 * time.Millisecond) {
            mu1.Lock();
            mu2.Lock();
            mu1.Unlock();
            mu2.Unlock();
        }
        if (runtime.Compiler == "gccgo") {
            runtime.Gosched();
        }
    }
}

// mutexHog starts multiple goroutines that runs the given hogger function for the specified duration.
// The hogger function will be given two mutexes to lock & unlock.
internal static partial void mutexHog(time.Duration duration, Action<ж<sync.Mutex>, ж<sync.Mutex>, time.Time, time.Duration> hogger) {
    ref var start = ref heap<time.Time>(time.Now(), out var Ꮡstart);
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
internal static ref readonly @string armˢ => ref armˢᵇ;
private static readonly @string armˢᵇ = "arm"u8;
internal static ref readonly @string debugPprofMutexˢ => ref debugPprofMutexˢᵇ;
private static readonly @string debugPprofMutexˢᵇ = "/debug/pprof/mutex"u8;
internal static ref readonly @string mutexHog1ˢ => ref mutexHog1ˢᵇ;
private static readonly @string mutexHog1ˢᵇ = "mutexHog1"u8;
internal static ref readonly @string mutexHog2ˢ => ref mutexHog2ˢᵇ;
private static readonly @string mutexHog2ˢᵇ = "mutexHog2"u8;

public static partial void TestDeltaProfile(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (strings.HasPrefix(runtime.GOARCH, armˢ)) {
            testenv.SkipFlaky(new pprof_internal_test_package.testing_TжTB(t), 50218);
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
        if (err != nil) {
            t.Skipf("mutex profile is unsupported: %v", err);
        }

        if (!seen(p, mutexHog1ˢ)) {
            t.Skipf("mutex profile is not working: %v", p.OrTypedNil());
        }

        // causes mutexHog2 call stacks to appear in the mutex profile.
        var done = new channel<bool>(0);
        var doneʗ1 = done;
        goǃ(() => {
            for (;;) {
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
        defer(() => { // cleanup the above goroutine.
            doneʗ2.ᐸꟷ(true);
            ᐸꟷ(doneʗ2); // wait for the goroutine to exit.
        }, ref ᒐ);

        foreach (var (_, d) in slice<nint>([1, 4, 16, 32])) {
            @string endpoint = fmt.Sprintf("/debug/pprof/mutex?seconds=%d"u8, d);
            var (pΔ1, errΔ1) = query(endpoint);
            if (errΔ1 != nil) {
                t.Fatalf("failed to query %q: %v", endpoint, errΔ1);
            }
            if (!seen(pΔ1, mutexHog1ˢ) && seen(pΔ1, mutexHog2ˢ) && pΔ1.DurationNanos > 0) {
                break; // pass
            }
            if (d == 32) {
                t.Errorf("want mutexHog2 but no mutexHog1 in the profile, and non-zero p.DurationNanos, got %v", pΔ1.OrTypedNil());
            }
        }
        (p, err) = query(debugPprofMutexˢ);
        if (err != nil) {
            t.Fatalf("failed to query mutex profile: %v", err);
        }
        if (!seen(p, mutexHog1ˢ) || !seen(p, mutexHog2ˢ)) {
            t.Errorf("want both mutexHog1 and mutexHog2 in the profile, got %v", p.OrTypedNil());
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static ж<httptest.Server> srv = httptest.NewServer(default);

internal static (ж<profile.Profile>, error) query(@string endpoint) {
    @string url = srv.URL + endpoint;
    var (r, err) = http.Get(url);
    if (err != nil) {
        return (default, fmt.Errorf("failed to fetch %q: %v"u8, url, err));
    }
    if (r.StatusCode != http.StatusOK) {
        return (default, fmt.Errorf("failed to fetch %q: %v"u8, url, r.Status));
    }

    (var b, err) = io.ReadAll(new pprof_internal_test_package.io_ReadCloserᴠReader(r.Body));
    r.Body.Close();
    if (err != nil) {
        return (default, fmt.Errorf("failed to read and parse the result from %q: %v"u8, url, err));
    }
    return profile.Parse(new pprof_internal_test_package.bytes_BufferжReader(bytes.NewBuffer(b)));
}

// seen returns true if the profile includes samples whose stacks include
// the specified function name (fname).
internal static bool seen(ж<profile.Profile> Ꮡp, @string fname) {
    ref var p = ref Ꮡp.DerefOrNull();

    var locIDs = new map<ж<profile.Location>, bool>{};
    foreach (var (_, loc) in p.Location) {
        foreach (var (_, l) in loc.Line) {
            if (strings.Contains(l.Function.Name, fname)) {
                locIDs[loc] = true;
                break;
            }
        }
    }
    foreach (var (_, sample) in p.Sample) {
        foreach (var (_, loc) in sample.Location) {
            if (locIDs[loc]) {
                return true;
            }
        }
    }
    return false;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"skipping in -short mode"u8;
internal static ref readonly @string runˢ => ref runˢᵇ;
private static readonly @string runˢᵇ = "run"u8;
internal static ref readonly @string testdataˢ => ref testdataˢᵇ;
private static readonly @string testdataˢᵇ = "testdata"u8;
internal static ref readonly @string deltaMutexGoˢ => ref deltaMutexGoˢᵇ;
private static readonly @string deltaMutexGoˢᵇ = "delta_mutex.go"u8;
internal static readonly object pPeriodTypeGotNilWantNotˢ = (@string)"p.PeriodType got nil want not nil"u8;

// TestDeltaProfileEmptyBase validates that we still receive a valid delta
// profile even if the base contains no samples.
//
// Regression test for https://go.dev/issue/64566.
public static void TestDeltaProfileEmptyBase(ж<testing.T> t) {
    if (testing.Short()) {
        // Delta profile collection has a 1s minimum.
        t.Skip(skippingInShortModeˢ);
    }

    testenv.MustHaveGoRun(new pprof_internal_test_package.testing_TжTB(t));

    var (gotool, err) = testenv.GoTool();
    if (err != nil) {
        t.Fatalf("error finding go tool: %v", err);
    }

    (var @out, err) = testenv.Command(new pprof_internal_test_package.testing_TжTB(t), gotool, runˢ, filepath.Join(testdataˢ, deltaMutexGoˢ)).CombinedOutput();
    if (err != nil) {
        t.Fatalf("error running profile collection: %v\noutput: %s", err, @out);
    }

    // Log the binary output for debugging failures.
    var b64 = new slice<byte>(base64.StdEncoding.EncodedLen(len(@out)));
    base64.StdEncoding.Encode(b64, @out);
    t.Logf("Output in base64.StdEncoding: %s", b64);

    (var p, err) = profile.Parse(new pprof_internal_test_package.bytes_ReaderжReader(bytes.NewReader(@out)));
    if (err != nil) {
        t.Fatalf("Parse got err %v want nil", err);
    }

    t.Logf("Output as parsed Profile: %s", p.OrTypedNil());

    if (len(p.SampleType) != 2) {
        t.Errorf("len(p.SampleType) got %d want 2", len(p.SampleType));
    }
    if (p.SampleType[0].Type != "contentions"u8) {
        t.Errorf(@"p.SampleType[0].Type got %q want ""contentions""", p.SampleType[0].Type);
    }
    if (p.SampleType[0].Unit != "count"u8) {
        t.Errorf(@"p.SampleType[0].Unit got %q want ""count""", p.SampleType[0].Unit);
    }
    if (p.SampleType[1].Type != "delay"u8) {
        t.Errorf(@"p.SampleType[1].Type got %q want ""delay""", p.SampleType[1].Type);
    }
    if (p.SampleType[1].Unit != "nanoseconds"u8) {
        t.Errorf(@"p.SampleType[1].Unit got %q want ""nanoseconds""", p.SampleType[1].Unit);
    }

    if (p.PeriodType == nil) {
        t.Fatal(pPeriodTypeGotNilWantNotˢ);
    }
    if (p.PeriodType.Type != "contentions"u8) {
        t.Errorf(@"p.PeriodType.Type got %q want ""contentions""", p.PeriodType.Type);
    }
    if (p.PeriodType.Unit != "count"u8) {
        t.Errorf(@"p.PeriodType.Unit got %q want ""count""", p.PeriodType.Unit);
    }
}

} // end pprof_internal_test_package
