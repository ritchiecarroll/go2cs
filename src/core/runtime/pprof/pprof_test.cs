// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !js
namespace go.runtime;

using bytes = bytes_package;
using context = context_package;
using fmt = fmt_package;
using abi = @internal.abi_package;
using profile = @internal.profile_package;
using label = @internal.runtime.pprof.label_package;
using unix = @internal.syscall.unix_package;
using testenv = @internal.testenv_package;
using io = io_package;
using iter = iter_package;
using math = math_package;
using big = go.math.big_package;
using os = os_package;
using regexp = regexp_package;
using runtime = runtime_package;
using debug = go.runtime.debug_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
using testing = testing_package;
using time = time_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using @internal;
using @internal.runtime.pprof;
using @internal.syscall;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using exec = go.os.exec_package;
using go.math;
using go.os;
using go.runtime;
using go.sync;
using static go.runtime.pprof_package;
using ꓸꓸꓸany = Span<any>;
using ꓸꓸꓸstring = Span<@string>;

partial class pprof_internal_test_package {

internal static void cpuHogger(Func<nint, nint> f, ж<nint> Ꮡy, time.Duration dur) {
    ref var y = ref Ꮡy.DerefOrNull();

    // We only need to get one 100 Hz clock tick, so we've got
    // a large safety buffer.
    // But do at least 500 iterations (which should take about 100ms),
    // otherwise TestCPUProfileMultithreaded can fail if only one
    // thread is scheduled during the testing period.
    var t0 = time.Now();
    nint accum = y;
    for (nint i = 0; i < 500 || time.Since(t0) < dur; i++) {
        accum = f(accum);
    }
    y = accum;
}

internal static readonly ж<nint> Ꮡsalt1 = new StaticBox<nint>(0);
internal static ref nint salt1 => ref ((StaticBox<nint>)Ꮡsalt1).ValueSlot;
internal static readonly ж<nint> Ꮡsalt2 = new StaticBox<nint>(0);
internal static ref nint salt2 => ref ((StaticBox<nint>)Ꮡsalt2).ValueSlot;

// The actual CPU hogging function.
// Must not call other functions nor access heap/globals in the loop,
// otherwise under race detector the samples will be in the race runtime.
internal static nint cpuHog1(nint x) {
    return cpuHog0(x, 100000);
}

internal static nint cpuHog0(nint x, nint n) {
    nint foo = x;
    for (nint i = 0; i < n; i++) {
        if (foo > 0) {
            foo *= foo;
        } else {
            foo *= foo + 1;
        }
    }
    return foo;
}

internal static nint cpuHog2(nint x) {
    nint foo = x;
    for (nint i = 0; i < 100000; i++) {
        if (foo > 0) {
            foo *= foo;
        } else {
            foo *= foo + 2;
        }
    }
    return foo;
}

// Return a list of functions that we don't want to ever appear in CPU
// profiles. For gccgo, that list includes the sigprof handler itself.
internal static slice<@string> avoidFunctions() {
    if (runtime.Compiler == "gccgo") {
        return slice<@string>(["runtime.sigprof"]);
    }
    return default;
}

public static void TestCPUProfile(ж<testing.T> t) {
    var matches = matchAndAvoidStacks(new Func<@string, uintptr, slice<ж<profile.Location>>, map<@string, slice<@string>>, bool>(stackContains), slice<@string>(["runtime/pprof.cpuHog1"]), avoidFunctions());
    testCPUProfile(t, matches, (time.Duration dur) => {
        cpuHogger(cpuHog1, Ꮡsalt1, dur);
    });
}

public static void TestCPUProfileMultithreaded(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(runtime.GOMAXPROCS, runtime.GOMAXPROCS(2), ref ᒐ);
        var matches = matchAndAvoidStacks(new Func<@string, uintptr, slice<ж<profile.Location>>, map<@string, slice<@string>>, bool>(stackContains), slice<@string>(["runtime/pprof.cpuHog1", "runtime/pprof.cpuHog2"]), avoidFunctions());
        testCPUProfile(t, matches, [MethodImpl(MethodImplOptions.NoInlining)] (time.Duration dur) => {
            var c = new channel<nint>(0);
            var cʗ1 = c;
            goǃ(() => {
                cpuHogger(cpuHog1, Ꮡsalt1, dur);
                cʗ1.ᐸꟷ(1);
            });
            cpuHogger(cpuHog2, Ꮡsalt2, dur);
            ᐸꟷ(c);
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object issue35057IsOnlyˢ = (@string)"issue 35057 is only confirmed on Linux"u8;

internal partial struct TestCPUProfileMultithreadMagnitude_type /*dyn*/ {
    internal @string name;
    internal nint workers;
}

public static void TestCPUProfileMultithreadMagnitude(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (runtime.GOOS != "linux"u8) {
            t.Skip(issue35057IsOnlyˢ);
        }

        defer(() => {
            if (t.Failed()) {
                t.Logf("Failure of this test may indicate that your system suffers from a known Linux kernel bug fixed on newer kernels. See https://golang.org/issue/49065.");
            }
        }, ref ᒐ);

        // Disable on affected builders to avoid flakiness, but otherwise keep
        // it enabled to potentially warn users that they are on a broken
        // kernel.
        if (testenv.Builder() != ""u8 && (runtime.GOARCH == "386"u8 || runtime.GOARCH == "amd64"u8)) {
            // Linux [5.9,5.16) has a kernel bug that can break CPU timers on newly
            // created threads, breaking our CPU accounting.
            if (unix.KernelVersionGE(5, 9) && !unix.KernelVersionGE(5, 16)) {
                testenv.SkipFlaky(new pprof_internal_test_package.testing_TжTB(t), 49065);
            }
        }

        // Run a workload in a single goroutine, then run copies of the same
        // workload in several goroutines. For both the serial and parallel cases,
        // the CPU time the process measures with its own profiler should match the
        // total CPU usage that the OS reports.
        //
        // We could also check that increases in parallelism (GOMAXPROCS) lead to a
        // linear increase in the CPU usage reported by both the OS and the
        // profiler, but without a guarantee of exclusive access to CPU resources
        // that is likely to be a flaky test.

        // Require the smaller value to be within 10%, or 40% in short mode.
        var maxDiff = 0.10D;
        if (testing.Short()) {
            maxDiff = 0.40D;
        }

        error compare(time.Duration a, time.Duration b, float64 maxDiffΔ1) {
            if (a <= 0 || b <= 0) {
                return fmt.Errorf("Expected both time reports to be positive"u8);
            }

            if (a < b) {
                (a, b) = (b, a);
            }

            var diff = (float64)(a - b) / (float64)a;
            if (diff > maxDiffΔ1) {
                return fmt.Errorf("CPU usage reports are too different (limit -%.1f%%, got -%.1f%%)"u8, maxDiffΔ1 * 100D, diff * 100D);
            }

            return default;
        }

        foreach (var (_, vᴛ1) in slice<TestCPUProfileMultithreadMagnitude_type>([
            new(
                name: "serial",
                workers: 1
            ),
            new(
                name: "parallel",
                workers: runtime.GOMAXPROCS(0)
            )
        ])) {
            ref var tc = ref heap(new TestCPUProfileMultithreadMagnitude_type(), out var Ꮡtc);
            tc = vᴛ1;

            // check that the OS's perspective matches what the Go runtime measures.
            var compareʗ1 = compare;
            var tcʗ1 = tc;
            t.Run(tc.name, (ж<testing.T> tΔ1) => {
                tΔ1.Logf("Running with %d workers", tcʗ1.workers);

                time.Duration userTime = default;
                time.Duration systemTime = default;
                var matches = matchAndAvoidStacks(new Func<@string, uintptr, slice<ж<profile.Location>>, map<@string, slice<@string>>, bool>(stackContains), slice<@string>(["runtime/pprof.cpuHog1"]), avoidFunctions());
                var compareʗ2 = compareʗ1;
                var matchesʗ1 = matches;
                var acceptProfile = (ж<testing.T> tΔ2, ж<profile.Profile> p) => {
                    if (!matchesʗ1(tΔ2, p)) {
                        return false;
                    }

                    var ok = true;
                    foreach (var (i, unit) in slice<@string>(["count", "nanoseconds"])) {
                        {
                            @string have = p.SampleType[i].Unit;
                            @string want = unit; if (have != want) {
                                tΔ2.Logf("pN SampleType[%d]; %q != %q", i, have, want);
                                ok = false;
                            }
                        }
                    }

                    // cpuHog1 called below is the primary source of CPU
                    // load, but there may be some background work by the
                    // runtime. Since the OS rusage measurement will
                    // include all work done by the process, also compare
                    // against all samples in our profile.
                    time.Duration value = default;
                    foreach (var (_, sample) in p.Sample) {
                        value += ((time.Duration)(~sample).Value[1]) * time.ΔNanosecond;
                    }

                    var totalTime = userTime + systemTime;
                    tΔ2.Logf("compare %s user + %s system = %s vs %s", userTime, systemTime, totalTime, value);
                    {
                        var err = compareʗ2(totalTime, value, maxDiff); if (err != nil) {
                            tΔ2.Logf("compare got %v want nil", err);
                            ok = false;
                        }
                    }

                    return ok;
                };

                var tcʗ2 = tcʗ1;
                testCPUProfile(tΔ1, new Func<ж<testing.T>, ж<profile.Profile>, bool>(acceptProfile), (time.Duration dur) => {
                    var tcʗ3 = tcʗ2;
                    (userTime, systemTime) = diffCPUTime(tΔ1, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
                        ref var once = ref heap(new sync.Once(), out var Ꮡonce);
                        for (nint i = 0; i < tcʗ3.workers; i++) {
                            Ꮡwg.Add(1);
                            goǃ(() => {
                                GoFrame ᒐ = default;
                                try {
                                    defer(Ꮡwg.Done, ref ᒐ);
                                    ref var salt = ref heap(new nint(), out var Ꮡsalt);
                                    salt = 0;
                                    cpuHogger(cpuHog1, Ꮡsalt, dur);
                                    Ꮡonce.Do(() => {
                                        salt1 = Ꮡsalt.Value;
                                    });
                                }
                                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                                finally { ᒐ.Run(); }
                            });
                        }
                        Ꮡwg.Wait();
                    });
                });
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// containsInlinedCall reports whether the function body for the function f is
// known to contain an inlined function call within the first maxBytes bytes.
internal static bool containsInlinedCall(any f, nint maxBytes) {
    var (_, found) = findInlinedCall(f, maxBytes);
    return found;
}

// findInlinedCall returns the PC of an inlined function call within
// the function body for the function f if any.
internal static (uint64 pc, bool found) findInlinedCall(any f, nint maxBytes) {
    var fFunc = runtime.FuncForPC((uintptr)abi.FuncPCABIInternal(f));
    if (fFunc == nil || fFunc.Entry() == 0) {
        throw panic("failed to locate function entry");
    }

    for (nint offset = 0; offset < maxBytes; offset++) {
        var innerPC = fFunc.Entry() + (uintptr)offset;
        var inner = runtime.FuncForPC(innerPC);
        if (inner == nil) {
            // No function known for this PC value.
            // It might simply be misaligned, so keep searching.
            continue;
        }
        if (inner.Entry() != fFunc.Entry()) {
            // Scanned past f and didn't find any inlined functions.
            break;
        }
        if (inner.Name() != fFunc.Name()) {
            // This PC has f as its entry-point, but is not f. Therefore, it must be a
            // function inlined into f.
            return ((uint64)innerPC, true);
        }
    }

    return (0, false);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object canTDetermineWhetherˢ = (@string)"Can't determine whether inlinedCallee was inlined into inlinedCaller."u8;

public static void TestCPUProfileInlining(ж<testing.T> t) {
    if (!containsInlinedCall(inlinedCaller, 4 << 10)) {
        t.Skip(canTDetermineWhetherˢ);
    }

    var matches = matchAndAvoidStacks(new Func<@string, uintptr, slice<ж<profile.Location>>, map<@string, slice<@string>>, bool>(stackContains), slice<@string>(["runtime/pprof.inlinedCallee", "runtime/pprof.inlinedCaller"]), avoidFunctions());
    var p = testCPUProfile(t, matches, (time.Duration dur) => {
        cpuHogger(inlinedCaller, Ꮡsalt1, dur);
    });

    // Check if inlined function locations are encoded correctly. The inlinedCalee and inlinedCaller should be in one location.
    foreach (var (_, loc) in p.Location) {
        var (hasInlinedCallerAfterInlinedCallee, hasInlinedCallee) = (false, false);
        foreach (var (_, line) in loc.Line) {
            if (line.Function.Name == "runtime/pprof.inlinedCallee"u8) {
                hasInlinedCallee = true;
            }
            if (hasInlinedCallee && line.Function.Name == "runtime/pprof.inlinedCaller"u8) {
                hasInlinedCallerAfterInlinedCallee = true;
            }
        }
        if (hasInlinedCallee != hasInlinedCallerAfterInlinedCallee) {
            t.Fatalf("want inlinedCallee followed by inlinedCaller, got separate Location entries:\n%v", p.OrTypedNil());
        }
    }
}

internal static nint inlinedCaller(nint x) {
    x = inlinedCallee(x, 100000);
    return x;
}

internal static nint inlinedCallee(nint x, nint n) {
    return cpuHog0(x, n);
}

//go:noinline
internal static partial void dumpCallers(slice<uintptr> pcs) {
    if (pcs == nil) {
        return;
    }

    nint skip = 2; // Callers and dumpCallers
    runtime.Callers(skip, pcs);
}

//go:noinline
internal static partial void inlinedCallerDump(slice<uintptr> pcs) {
    inlinedCalleeDump(pcs);
}

internal static partial void inlinedCalleeDump(slice<uintptr> pcs) {
    dumpCallers(pcs);
}

internal partial interface inlineWrapperInterface {
    void dump(slice<uintptr> stack);
}

internal partial struct inlineWrapper {
}

internal static partial void dump(this inlineWrapper h, slice<uintptr> pcs) {
    dumpCallers(pcs);
}

internal static void inlinedWrapperCallerDump(slice<uintptr> pcs) {
    ref var h = ref heap<inlineWrapperInterface>(out var Ꮡh);

    // Take the address of h, such that h.dump() call (below)
    // does not get devirtualized by the compiler.
    _ = Ꮡh;

    h = new pprof_internal_test_package.inlineWrapperжinlineWrapperInterface(Ꮡ(new inlineWrapper()));
    h.dump(pcs);
}

public static void TestCPUProfileRecursion(ж<testing.T> t) {
    var matches = matchAndAvoidStacks(new Func<@string, uintptr, slice<ж<profile.Location>>, map<@string, slice<@string>>, bool>(stackContains), slice<@string>(["runtime/pprof.inlinedCallee", "runtime/pprof.recursionCallee", "runtime/pprof.recursionCaller"]), avoidFunctions());
    var p = testCPUProfile(t, matches, (time.Duration dur) => {
        cpuHogger(recursionCaller, Ꮡsalt1, dur);
    });

    // check the Location encoding was not confused by recursive calls.
    foreach (var (i, loc) in p.Location) {
        nint recursionFunc = 0;
        foreach (var (_, line) in loc.Line) {
            {
                @string name = line.Function.Name; if (name == "runtime/pprof.recursionCaller"u8 || name == "runtime/pprof.recursionCallee"u8) {
                    recursionFunc++;
                }
            }
        }
        if (recursionFunc > 1) {
            t.Fatalf("want at most one recursionCaller or recursionCallee in one Location, got a violating Location (index: %d):\n%v", i, p.OrTypedNil());
        }
    }
}

internal static nint recursionCaller(nint x) {
    nint y = recursionCallee(3, x);
    return y;
}

internal static nint recursionCallee(nint n, nint x) {
    if (n == 0) {
        return 1;
    }
    nint y = inlinedCallee(x, 10000);
    return y * recursionCallee(n - 1, x);
}

internal static void recursionChainTop(nint x, slice<uintptr> pcs) {
    if (x < 0) {
        return;
    }
    recursionChainMiddle(x, pcs);
}

internal static void recursionChainMiddle(nint x, slice<uintptr> pcs) {
    recursionChainBottom(x, pcs);
}

internal static void recursionChainBottom(nint x, slice<uintptr> pcs) {
    // This will be called each time, we only care about the last. We
    // can't make this conditional or this function won't be inlined.
    dumpCallers(pcs);

    recursionChainTop(x - 1, pcs);
}

internal static ж<profile.Profile> parseProfile(ж<testing.T> t, slice<byte> valBytes, Action<uintptr, slice<ж<profile.Location>>, map<@string, slice<@string>>> f) {
    var (p, err) = profile.Parse(new pprof_internal_test_package.bytes_ReaderжReader(bytes.NewReader(valBytes)));
    if (err != nil) {
        t.Fatal(err);
    }
    foreach (var (_, sample) in p.Sample) {
        var count = (uintptr)(~sample).Value[0];
        f(count, sample.Location, sample.Label);
    }
    return p;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unameˢ => ref unameˢᵇ;
private static readonly @string unameˢᵇ = "uname"u8;
internal static readonly object skippingOnPlan9ˢ = (@string)"skipping on plan9"u8;
internal static readonly object skippingOnWasip1ˢ = (@string)"skipping on wasip1"u8;
internal static ref readonly @string inQemuˢ => ref inQemuˢᵇ;
private static readonly @string inQemuˢᵇ = "IN_QEMU"u8;
internal static readonly object ignoreTheFailureInQemuˢ = (@string)"ignore the failure in QEMU; see golang.org/issue/9605"u8;

// testCPUProfile runs f under the CPU profiler, checking for some conditions specified by need,
// as interpreted by matches, and returns the parsed profile.
internal static ж<profile.Profile> testCPUProfile(ж<testing.T> Ꮡt, Func<ж<testing.T>, ж<profile.Profile>, bool> matches, Action<time.Duration> f) {
    ref var t = ref Ꮡt.DerefOrNull();

    var exprᴛ1 = runtime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "darwin"u8: {
        var (@out, err) = testenv.Command(new pprof_internal_test_package.testing_TжTB(Ꮡt), unameˢ, "-a").CombinedOutput();
        if (err != nil) {
            Ꮡt.Fatal(err);
        }
        @string vers = (@string)@out;
        Ꮡt.Logf("uname -a: %v", vers);
        break;
    }
    case {} when exprᴛ1 == "plan9"u8: {
        Ꮡt.Skip(skippingOnPlan9ˢ);
        break;
    }
    case {} when exprᴛ1 == "wasip1"u8: {
        Ꮡt.Skip(skippingOnWasip1ˢ);
        break;
    }}

    var broken = testenv.CPUProfilingBroken();

    var (deadline, ok) = t.Deadline();
    if (broken || !ok) {
        if (broken && testing.Short()) {
            // If it's expected to be broken, no point waiting around.
            deadline = time.Now().Add(1 * time.ΔSecond);
        } else {
            deadline = time.Now().Add((time.Duration)(10000000000L));
        }
    }

    // If we're running a long test, start with a long duration
    // for tests that try to make sure something *doesn't* happen.
    var duration = (time.Duration)(5000000000L);
    if (testing.Short()) {
        duration = 100 * time.Millisecond;
    }

    // Profiling tests are inherently flaky, especially on a
    // loaded system, such as when this test is running with
    // several others under go test std. If a test fails in a way
    // that could mean it just didn't run long enough, try with a
    // longer duration.
    for (;;) {
        ref var prof = ref heap(new bytes.Buffer(), out var Ꮡprof);
        {
            var err = StartCPUProfile(new pprof_internal_test_package.bytes_BufferжWriter(Ꮡprof)); if (err != nil) {
                Ꮡt.Fatal(err);
            }
        }
        f(duration);
        StopCPUProfile();

        {
            var (p, okΔ1) = profileOk(Ꮡt, matches, Ꮡprof, duration); if (okΔ1) {
                return p;
            }
        }

        duration *= 2;
        if (time.Until(deadline) < duration) {
            break;
        }
        Ꮡt.Logf("retrying with %s duration", duration);
    }

    if (broken) {
        Ꮡt.Skipf("ignoring failure on %s/%s; see golang.org/issue/13841", runtime.GOOS, runtime.GOARCH);
    }

    // Ignore the failure if the tests are running in a QEMU-based emulator,
    // QEMU is not perfect at emulating everything.
    // IN_QEMU environmental variable is set by some of the Go builders.
    // IN_QEMU=1 indicates that the tests are running in QEMU. See issue 9605.
    if (os.Getenv(inQemuˢ) == "1"u8) {
        Ꮡt.Skip(ignoreTheFailureInQemuˢ);
    }
    Ꮡt.FailNow();
    return default;
}

internal static Func<Action, (time.Duration, time.Duration)> diffCPUTimeImpl;

internal static (time.Duration user, time.Duration system) diffCPUTime(ж<testing.T> t, Action f) {
    {
        var fn = diffCPUTimeImpl; if (fn != nil) {
            return fn(f);
        }
    }
    t.Fatalf("cannot measure CPU time on GOOS=%s GOARCH=%s", runtime.GOOS, runtime.GOARCH);
    return (0, 0);
}

// stackContains matches if a function named spec appears anywhere in the stack trace.
internal static bool stackContains(@string spec, uintptr count, slice<ж<profile.Location>> stk, map<@string, slice<@string>> labels) {
    foreach (var (_, loc) in stk) {
        foreach (var (_, line) in loc.Line) {
            if (strings_package.Contains(line.Function.Name, spec)) {
                return true;
            }
        }
    }
    return false;
}

// type sampleMatchFunc is a methodless func type — rendered inline as its base delegate

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object tooFewSamplesOnWindowsˢ = (@string)"too few samples on Windows (golang.org/issue/10842)"u8;

internal static (ж<profile.Profile>, bool ok) profileOk(ж<testing.T> t, Func<ж<testing.T>, ж<profile.Profile>, bool> matches, ж<bytes.Buffer> Ꮡprof, time.Duration duration) {
    bool ok = default;
    ref var prof = ref Ꮡprof.DerefOrNull();

    ok = true;

    uintptr samples = default;
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    var p = parseProfile(t, prof.Bytes(), (uintptr count, slice<ж<profile.Location>> stk, map<@string, slice<@string>> labels) => {
        fmt.Fprintf(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡbuf), "%d:"u8, count);
        fprintStack(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡbuf), stk);
        fmt.Fprintf(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡbuf), " labels: %v\n"u8, labels);
        samples += count;
        fmt.Fprintf(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡbuf), "\n"u8);
    });
    t.Logf("total %d CPU profile samples collected:\n%s", samples, buf.String());

    if (samples < 10 && runtime.GOOS == "windows"u8) {
        // On some windows machines we end up with
        // not enough samples due to coarse timer
        // resolution. Let it go.
        t.Log(tooFewSamplesOnWindowsˢ);
        return (p, false);
    }

    // Check that we got a reasonable number of samples.
    // We used to always require at least ideal/4 samples,
    // but that is too hard to guarantee on a loaded system.
    // Now we accept 10 or more samples, which we take to be
    // enough to show that at least some profiling is occurring.
    {
        var ideal = (uintptr)(duration * 100 / time.ΔSecond); if (samples == 0 || (samples < ideal / 4 && samples < 10)) {
            t.Logf("too few samples; got %d, want at least %d, ideally %d", samples, ideal / 4, ideal);
            ok = false;
        }
    }

    if (matches != nil && !matches(t, p)) {
        ok = false;
    }

    return (p, ok);
}

// type profileMatchFunc is a methodless func type — rendered inline as its base delegate

internal static Func<ж<testing.T>, ж<profile.Profile>, bool> matchAndAvoidStacks(Func<@string, uintptr, slice<ж<profile.Location>>, map<@string, slice<@string>>, bool> matches, slice<@string> need, slice<@string> avoid) {
    var avoidʗ1 = avoid;
    var needʗ1 = need;
    return (ж<testing.T> t, ж<profile.Profile> p) => {
        bool ok = default;
        ok = true;

        // Check that profile is well formed, contains 'need', and does not contain
        // anything from 'avoid'.
        var have = new slice<uintptr>(len(needʗ1));
        var avoidSamples = new slice<uintptr>(len(avoidʗ1));

        foreach (var (_, sample) in p.Sample) {
            var count = (uintptr)(~sample).Value[0];
            foreach (var (i, spec) in needʗ1) {
                if (matches(spec, count, sample.Location, sample.Label)) {
                    have[i] += count;
                }
            }
            foreach (var (i, name) in avoidʗ1) {
                foreach (var (_, loc) in sample.Location) {
                    foreach (var (_, line) in loc.Line) {
                        if (strings_package.Contains(line.Function.Name, name)) {
                            avoidSamples[i] += count;
                        }
                    }
                }
            }
        }

        foreach (var (i, name) in avoidʗ1) {
            var bad = avoidSamples[i];
            if (bad != 0) {
                t.Logf("found %d samples in avoid-function %s\n", bad, name);
                ok = false;
            }
        }

        if (len(needʗ1) == 0) {
            return ok;
        }

        uintptr total = default;
        foreach (var (i, name) in needʗ1) {
            total += have[i];
            t.Logf("found %d samples in expected function %s\n", have[i], name);
        }
        if (total == 0) {
            t.Logf("no samples in expected functions");
            ok = false;
        }

        // We'd like to check a reasonable minimum, like
        // total / len(have) / smallconstant, but this test is
        // pretty flaky (see bug 7095).  So we'll just test to
        // make sure we got at least one sample.
        var min = (uintptr)1;
        foreach (var (i, name) in needʗ1) {
            if (have[i] < min) {
                t.Logf("%s has %d samples out of %d, want at least %d, ideally %d", name, have[i], total, min, total / (uintptr)len(have));
                ok = false;
            }
        }
        return ok;
    };
}

// Fork can hang if preempted with signals frequently enough (see issue 5517).
// Ensure that we do not do this.
public static partial void TestCPUProfileWithFork(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveExec(new pprof_internal_test_package.testing_TжTB(t));

        var (exe, err) = os.Executable();
        if (err != nil) {
            t.Fatal(err);
        }

        nint heap = 1 << 30;
        if (runtime.GOOS == "android"u8) {
            // Use smaller size for Android to avoid crash.
            heap = 100 << 20;
        }
        if (testing.Short()) {
            heap = 100 << 20;
        }
        // This makes fork slower.
        var garbage = new slice<byte>(heap);
        // Need to touch the slice, otherwise it won't be paged in.
        var done = new channel<bool>(0);
        var doneʗ1 = done;
        var garbageʗ1 = garbage;
        goǃ(() => {
            foreach (var i in range(garbageʗ1)) {
                garbageʗ1[i] = 42;
            }
            doneʗ1.ᐸꟷ(true);
        });
        ᐸꟷ(done);

        ref var prof = ref builtin.heap(new bytes.Buffer(), out var Ꮡprof);
        {
            var errΔ1 = StartCPUProfile(new pprof_internal_test_package.bytes_BufferжWriter(Ꮡprof)); if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
        }
        defer(StopCPUProfile, ref ᒐ);

        for (nint i = 0; i < 10; i++) {
            testenv.Command(new pprof_internal_test_package.testing_TжTB(t), exe, "-h").CombinedOutput();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object notApplicableForGccgoˢ = (@string)"not applicable for gccgo"u8;

// Test that profiler does not observe runtime.gogo as "user" goroutine execution.
// If it did, it would see inconsistent state and would either record an incorrect stack
// or crash because the stack was malformed.
public static void TestGoroutineSwitch(ж<testing.T> t) {
    if (runtime.Compiler == "gccgo") {
        t.Skip(notApplicableForGccgoˢ);
    }
    // How much to try. These defaults take about 1 seconds
    // on a 2012 MacBook Pro. The ones in short mode take
    // about 0.1 seconds.
    nint tries = 10;
    nint count = 1000000;
    if (testing.Short()) {
        tries = 1;
    }
    for (nint @try = 0; @try < tries; @try++) {
        ref var prof = ref heap(new bytes.Buffer(), out var Ꮡprof);
        {
            var err = StartCPUProfile(new pprof_internal_test_package.bytes_BufferжWriter(Ꮡprof)); if (err != nil) {
                t.Fatal(err);
            }
        }
        for (nint i = 0; i < count; i++) {
            runtime.Gosched();
        }
        StopCPUProfile();

        // Read profile to look for entries for gogo with an attempt at a traceback.
        // "runtime.gogo" is OK, because that's the part of the context switch
        // before the actual switch begins. But we should not see "gogo",
        // aka "gogo<>(SB)", which does the actual switch and is marked SPWRITE.
        parseProfile(t, prof.Bytes(), (uintptr countΔ1, slice<ж<profile.Location>> stk, map<@string, slice<@string>> _) => {
            // An entry with two frames with 'System' in its top frame
            // exists to record a PC without a traceback. Those are okay.
            if (len(stk) == 2) {
                @string nameΔ1 = stk[1].Line[0].Function.Name;
                if (nameΔ1 == "runtime._System"u8 || nameΔ1 == "runtime._ExternalCode"u8 || nameΔ1 == "runtime._GC"u8) {
                    return;
                }
            }

            // An entry with just one frame is OK too:
            // it knew to stop at gogo.
            if (len(stk) == 1) {
                return;
            }

            // Otherwise, should not see gogo.
            // The place we'd see it would be the inner most frame.
            @string name = stk[0].Line[0].Function.Name;
            if (name == "gogo"u8) {
                ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
                fprintStack(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡbuf), stk);
                t.Fatalf("found profile entry for gogo:\n%s", buf.String());
            }
        });
    }
}

internal static void fprintStack(io.Writer w, slice<ж<profile.Location>> stk) {
    if (len(stk) == 0) {
        fmt.Fprintf(w, " (stack empty)"u8);
    }
    foreach (var (_, loc) in stk) {
        fmt.Fprintf(w, " %#x"u8, loc.Address);
        fmt.Fprintf(w, " ("u8);
        foreach (var (i, line) in loc.Line) {
            if (i > 0) {
                fmt.Fprintf(w, " "u8);
            }
            fmt.Fprintf(w, "%s:%d"u8, line.Function.Name, line.ΔLine);
        }
        fmt.Fprintf(w, ")"u8);
    }
}

// Test that profiling of division operations is okay, especially on ARM. See issue 6681.
public static void TestMathBigDivide(ж<testing.T> t) {
    testCPUProfile(t, default, (time.Duration duration) => {
        var tΔ1 = time.After(duration);
        var pi = @new<bigꓸInt>();
        for (;;) {
            for (nint i = 0; i < 100; i++) {
                var n = big.NewInt(2646693125139304345L);
                var d = big.NewInt(842468587426513207L);
                pi.Div(n, d);
            }
            var selᴛ1 = tΔ1;
            switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
            case 0 when selᴛ1.ꟷᐳ(out _): {
                return;
            }
            default: {
                break;
            }}
        }
    });
}

// stackContainsAll matches if all functions in spec (comma-separated) appear somewhere in the stack trace.
internal static bool stackContainsAll(@string spec, uintptr count, slice<ж<profile.Location>> stk, map<@string, slice<@string>> labels) {
    foreach (var (_, f) in strings_package.Split(spec, ",")) {
        if (!stackContains(f, count, stk, labels)) {
            return false;
        }
    }
    return true;
}

public static void TestMorestack(ж<testing.T> t) {
    var matches = matchAndAvoidStacks(new Func<@string, uintptr, slice<ж<profile.Location>>, map<@string, slice<@string>>, bool>(stackContainsAll), slice<@string>(["runtime.newstack,runtime/pprof.growstack"]), avoidFunctions());
    testCPUProfile(t, matches, [MethodImpl(MethodImplOptions.NoInlining)] (time.Duration duration) => {
        var tΔ1 = time.After(duration);
        var c = new channel<bool>(0);
        for (;;) {
            var cʗ1 = c;
            goǃ(() => {
                growstack1();
                // NOTE(vsaioc): This goroutine may leak without this select.
                var selᴛ2 = cʗ1.ᐸꟷ(true, ꓸꓸꓸ);
                var selᴛ3 = time.After(duration);
                switch (select(selᴛ2, ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
                case 0: {
                    break;
                }
                case 1 when selᴛ3.ꟷᐳ(out _): {
                    break;
                }}
            });
            var selᴛ4 = tΔ1;
            var selᴛ5 = c;
            switch (select(ᐸꟷ(selᴛ4, ꓸꓸꓸ), ᐸꟷ(selᴛ5, ꓸꓸꓸ))) {
            case 0 when selᴛ4.ꟷᐳ(out _): {
                return;
            }
            case 1 when selᴛ5.ꟷᐳ(out _): {
                break;
            }}
        }
    });
}

//go:noinline
internal static partial void growstack1() {
    growstack(10);
}

//go:noinline
internal static partial void growstack(nint n) {
    array<byte> buf = new(2097152); /* (8 << 18) */
    use(buf);
    if (n > 0) {
        growstack(n - 1);
    }
}

//go:noinline
internal static partial void use(/*[2097152]*/ array<byte> x) {
    x = x.Clone();

}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string blockˢ => ref blockˢᵇ;
private static readonly @string blockˢᵇ = "block"u8;
internal static ref readonly @string contentionCyclesSecondˢ => ref contentionCyclesSecondˢᵇ;
private static readonly @string contentionCyclesSecondˢᵇ = "--- contention:\ncycles/second="u8;

internal partial struct TestBlockProfile_TestCase /*dyn*/ {
    internal @string name;
    internal Action<ж<testing.T>> f;
    internal slice<@string> stk;
    internal @string re;
}

public static void TestBlockProfile(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        ref var tests = ref heap<array<TestBlockProfile_TestCase>>(new TestBlockProfile_TestCase[]{
            new(
                name: "chan recv",
                f: blockChanRecv,
                stk: slice<@string>([
                    "runtime.chanrecv1",
                    "runtime/pprof.blockChanRecv",
                    "runtime/pprof.TestBlockProfile"
                ]),
                re: """

[0-9]+ [0-9]+ @( 0x[[:xdigit:]]+)+
#	0x[0-9a-f]+	runtime\.chanrecv1\+0x[0-9a-f]+	.*runtime/chan.go:[0-9]+
#	0x[0-9a-f]+	runtime/pprof\.blockChanRecv\+0x[0-9a-f]+	.*runtime/pprof/pprof_test.go:[0-9]+
#	0x[0-9a-f]+	runtime/pprof\.TestBlockProfile\+0x[0-9a-f]+	.*runtime/pprof/pprof_test.go:[0-9]+

"""u8.Raw()),
            new(
                name: "chan send",
                f: blockChanSend,
                stk: slice<@string>([
                    "runtime.chansend1",
                    "runtime/pprof.blockChanSend",
                    "runtime/pprof.TestBlockProfile"
                ]),
                re: """

[0-9]+ [0-9]+ @( 0x[[:xdigit:]]+)+
#	0x[0-9a-f]+	runtime\.chansend1\+0x[0-9a-f]+	.*runtime/chan.go:[0-9]+
#	0x[0-9a-f]+	runtime/pprof\.blockChanSend\+0x[0-9a-f]+	.*runtime/pprof/pprof_test.go:[0-9]+
#	0x[0-9a-f]+	runtime/pprof\.TestBlockProfile\+0x[0-9a-f]+	.*runtime/pprof/pprof_test.go:[0-9]+

"""u8.Raw()),
            new(
                name: "chan close",
                f: blockChanClose,
                stk: slice<@string>([
                    "runtime.chanrecv1",
                    "runtime/pprof.blockChanClose",
                    "runtime/pprof.TestBlockProfile"
                ]),
                re: """

[0-9]+ [0-9]+ @( 0x[[:xdigit:]]+)+
#	0x[0-9a-f]+	runtime\.chanrecv1\+0x[0-9a-f]+	.*runtime/chan.go:[0-9]+
#	0x[0-9a-f]+	runtime/pprof\.blockChanClose\+0x[0-9a-f]+	.*runtime/pprof/pprof_test.go:[0-9]+
#	0x[0-9a-f]+	runtime/pprof\.TestBlockProfile\+0x[0-9a-f]+	.*runtime/pprof/pprof_test.go:[0-9]+

"""u8.Raw()),
            new(
                name: "select recv async",
                f: blockSelectRecvAsync,
                stk: slice<@string>([
                    "runtime.selectgo",
                    "runtime/pprof.blockSelectRecvAsync",
                    "runtime/pprof.TestBlockProfile"
                ]),
                re: """

[0-9]+ [0-9]+ @( 0x[[:xdigit:]]+)+
#	0x[0-9a-f]+	runtime\.selectgo\+0x[0-9a-f]+	.*runtime/select.go:[0-9]+
#	0x[0-9a-f]+	runtime/pprof\.blockSelectRecvAsync\+0x[0-9a-f]+	.*runtime/pprof/pprof_test.go:[0-9]+
#	0x[0-9a-f]+	runtime/pprof\.TestBlockProfile\+0x[0-9a-f]+	.*runtime/pprof/pprof_test.go:[0-9]+

"""u8.Raw()),
            new(
                name: "select send sync",
                f: blockSelectSendSync,
                stk: slice<@string>([
                    "runtime.selectgo",
                    "runtime/pprof.blockSelectSendSync",
                    "runtime/pprof.TestBlockProfile"
                ]),
                re: """

[0-9]+ [0-9]+ @( 0x[[:xdigit:]]+)+
#	0x[0-9a-f]+	runtime\.selectgo\+0x[0-9a-f]+	.*runtime/select.go:[0-9]+
#	0x[0-9a-f]+	runtime/pprof\.blockSelectSendSync\+0x[0-9a-f]+	.*runtime/pprof/pprof_test.go:[0-9]+
#	0x[0-9a-f]+	runtime/pprof\.TestBlockProfile\+0x[0-9a-f]+	.*runtime/pprof/pprof_test.go:[0-9]+

"""u8.Raw()),
            new(
                name: "mutex",
                f: blockMutex,
                stk: slice<@string>([
                    "sync.(*Mutex).Lock",
                    "runtime/pprof.blockMutex",
                    "runtime/pprof.TestBlockProfile"
                ]),
                re: """

[0-9]+ [0-9]+ @( 0x[[:xdigit:]]+)+
#	0x[0-9a-f]+	sync\.\(\*Mutex\)\.Lock\+0x[0-9a-f]+	.*sync/mutex\.go:[0-9]+
#	0x[0-9a-f]+	runtime/pprof\.blockMutex\+0x[0-9a-f]+	.*runtime/pprof/pprof_test.go:[0-9]+
#	0x[0-9a-f]+	runtime/pprof\.TestBlockProfile\+0x[0-9a-f]+	.*runtime/pprof/pprof_test.go:[0-9]+

"""u8.Raw()),
            new(
                name: "cond",
                f: blockCond,
                stk: slice<@string>([
                    "sync.(*Cond).Wait",
                    "runtime/pprof.blockCond",
                    "runtime/pprof.TestBlockProfile"
                ]),
                re: """

[0-9]+ [0-9]+ @( 0x[[:xdigit:]]+)+
#	0x[0-9a-f]+	sync\.\(\*Cond\)\.Wait\+0x[0-9a-f]+	.*sync/cond\.go:[0-9]+
#	0x[0-9a-f]+	runtime/pprof\.blockCond\+0x[0-9a-f]+	.*runtime/pprof/pprof_test.go:[0-9]+
#	0x[0-9a-f]+	runtime/pprof\.TestBlockProfile\+0x[0-9a-f]+	.*runtime/pprof/pprof_test.go:[0-9]+

"""u8.Raw())
        }.array(), out var Ꮡtests);

        // Generate block profile
        runtime.SetBlockProfileRate(1);
        defer(runtime.SetBlockProfileRate, (nint)0, ref ᒐ);
        foreach (var (_, test) in tests.ΔRangeSnapshot()) {
            test.f(t);
        }

        var testsʗ1 = tests;
        t.Run(debug1ˢ, (ж<testing.T> tΔ1) => {
            ref var w = ref heap(new strings.Builder(), out var Ꮡw);
            Lookup(blockˢ).WriteTo(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡw), 1);
            @string prof = w.String();

            if (!strings_package.HasPrefix(prof, contentionCyclesSecondˢ)) {
                tΔ1.Fatalf("Bad profile header:\n%v", prof);
            }

            if (strings_package.HasSuffix(prof, "#\t0x0\n\n")) {
                tΔ1.Errorf("Useless 0 suffix:\n%v", prof);
            }

            foreach (var (_, test) in testsʗ1.ΔRangeSnapshot()) {
                if (!regexp.MustCompile(strings_package.ReplaceAll(test.re, "\t", "\t+")).MatchString(prof)) {
                    tΔ1.Errorf("Bad %v entry, expect:\n%v\ngot:\n%v", test.name, test.re, prof);
                }
            }
        });

        var testsʗ2 = tests;
        t.Run(protoˢ, (ж<testing.T> tΔ2) => {
            // proto format
            ref var w = ref heap(new bytes.Buffer(), out var Ꮡw);
            Lookup(blockˢ).WriteTo(new pprof_internal_test_package.bytes_BufferжWriter(Ꮡw), 0);
            var (p, err) = profile.Parse(new pprof_internal_test_package.bytes_BufferжReader(Ꮡw));
            if (err != nil) {
                tΔ2.Fatalf("failed to parse profile: %v", err);
            }
            tΔ2.Logf("parsed proto: %s", p.OrTypedNil());
            {
                var errΔ1 = p.CheckValid(); if (errΔ1 != nil) {
                    tΔ2.Fatalf("invalid profile: %v", errΔ1);
                }
            }

            var stks = profileStacks(p);
            foreach (var (_, test) in testsʗ2.ΔRangeSnapshot()) {
                if (!containsStack(stks, test.stk)) {
                    tΔ2.Errorf("No matching stack entry for %v, want %+v", test.name, test.stk);
                }
            }
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static slice<slice<@string>> /*res*/ profileStacks(ж<profile.Profile> Ꮡp) {
    slice<slice<@string>> res = default;
    ref var p = ref Ꮡp.DerefOrNull();

    foreach (var (_, s) in p.Sample) {
        slice<@string> stk = default;
        foreach (var (_, l) in s.Location) {
            foreach (var (_, line) in l.Line) {
                stk = append(stk, line.Function.Name);
            }
        }
        res = append(res, stk);
    }
    return res;
}

internal static partial slice<slice<@string>> /*res*/ blockRecordStacks(slice<runtime.BlockProfileRecord> records) {
    slice<slice<@string>> res = default;
    var resᴷ = new stackAppend<slice<@string>>(moves: true);

    foreach (var (_, vᴛ1) in records) {
        var record = vᴛ1;

        var frames = runtime.CallersFrames(record.StackRecord.Stack());
        slice<@string> stk = default;
        for (;;) {
            var (frame, more) = frames.Next();
            stk = append(stk, frame.Function);
            if (!more) {
                break;
            }
        }
        res = append(resᴷ, res, stk);
    }
    res = move2heap(resᴷ, res);
    return res;
}

internal static bool containsStack(slice<slice<@string>> got, slice<@string> want) {
    foreach (var (_, stk) in got) {
        if (len(stk) < len(want)) {
            continue;
        }
        foreach (var (i, f) in want) {
            if (f != stk[i]) {
                break;
            }
            if (i == len(want) - 1) {
                return true;
            }
        }
    }
    return false;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string allˢ => ref allˢᵇ;
private static readonly @string allˢᵇ = "all"u8;

// awaitBlockedGoroutine spins on runtime.Gosched until a runtime stack dump
// shows a goroutine in the given state with a stack frame in
// runtime/pprof.<fName>.
internal static partial void awaitBlockedGoroutine(ж<testing.T> Ꮡt, @string state, @string fName, nint count) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        @string re = fmt.Sprintf(@"(?m)^goroutine \d+ \[%s\]:\n(?:.+\n\t.+\n)*runtime/pprof\.%s"u8, regexp.QuoteMeta(state), fName);
        var r = regexp.MustCompile(re);

        {
            var (deadline, ok) = t.Deadline(); if (ok) {
                {
                    var d = time.Until(deadline); if (d > 1 * time.ΔSecond) {
                        var timer = time.AfterFunc(d - 1 * time.ΔSecond, () => {
                            debug.SetTraceback(allˢ);
                            throw panic(fmt.Sprintf("timed out waiting for %#q"u8, re));
                        });
                        var timerʗ1 = timer;
                        defer(() => timerʗ1.Stop(), ref ᒐ);
                    }
                }
            }
        }

        var buf = new slice<byte>((64 << 10));
        for (;;) {
            runtime.Gosched();
            nint n = runtime.Stack(buf, true);
            if (n == len(buf)) {
                // Buffer wasn't large enough for a full goroutine dump.
                // Resize it and try again.
                buf = new slice<byte>(2 * len(buf));
                continue;
            }
            if (len(r.FindAll(buf.slice(0, n), -1)) >= count) {
                return;
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string chanReceiveˢ => ref chanReceiveˢᵇ;
private static readonly @string chanReceiveˢᵇ = "chan receive"u8;
internal static ref readonly @string blockChanRecvˢ => ref blockChanRecvˢᵇ;
private static readonly @string blockChanRecvˢᵇ = "blockChanRecv"u8;

internal static partial void blockChanRecv(ж<testing.T> t) {
    var c = new channel<bool>(0);
    var cʗ1 = c;
    goǃ(() => {
        awaitBlockedGoroutine(t, chanReceiveˢ, blockChanRecvˢ, 1);
        cʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(c);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string chanSendˢ => ref chanSendˢᵇ;
private static readonly @string chanSendˢᵇ = "chan send"u8;
internal static ref readonly @string blockChanSendˢ => ref blockChanSendˢᵇ;
private static readonly @string blockChanSendˢᵇ = "blockChanSend"u8;

internal static partial void blockChanSend(ж<testing.T> t) {
    var c = new channel<bool>(0);
    var cʗ1 = c;
    goǃ(() => {
        awaitBlockedGoroutine(t, chanSendˢ, blockChanSendˢ, 1);
        ᐸꟷ(cʗ1);
    });
    c.ᐸꟷ(true);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string blockChanCloseˢ => ref blockChanCloseˢᵇ;
private static readonly @string blockChanCloseˢᵇ = "blockChanClose"u8;

internal static partial void blockChanClose(ж<testing.T> t) {
    var c = new channel<bool>(0);
    var cʗ1 = c;
    goǃ(() => {
        awaitBlockedGoroutine(t, chanReceiveˢ, blockChanCloseˢ, 1);
        close(cʗ1);
    });
    ᐸꟷ(c);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string selectˢ => ref selectˢᵇ;
private static readonly @string selectˢᵇ = "select"u8;
internal static ref readonly @string blockSelectRecvAsyncˢ => ref blockSelectRecvAsyncˢᵇ;
private static readonly @string blockSelectRecvAsyncˢᵇ = "blockSelectRecvAsync"u8;

internal static partial void blockSelectRecvAsync(ж<testing.T> t) {
    const nint numTries = 3;
    var c = new channel<bool>(1);
    var c2 = new channel<bool>(1);
    var cʗ1 = c;
    goǃ(() => {
        for (nint i = 0; i < numTries; i++) {
            awaitBlockedGoroutine(t, selectˢ, blockSelectRecvAsyncˢ, 1);
            cʗ1.ᐸꟷ(true);
        }
    });
    for (nint i = 0; i < numTries; i++) {
        var selᴛ6 = c;
        var selᴛ7 = c2;
        switch (select(ᐸꟷ(selᴛ6, ꓸꓸꓸ), ᐸꟷ(selᴛ7, ꓸꓸꓸ))) {
        case 0 when selᴛ6.ꟷᐳ(out _): {
            break;
        }
        case 1 when selᴛ7.ꟷᐳ(out _): {
            break;
        }}
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string blockSelectSendSyncˢ => ref blockSelectSendSyncˢᵇ;
private static readonly @string blockSelectSendSyncˢᵇ = "blockSelectSendSync"u8;

internal static partial void blockSelectSendSync(ж<testing.T> t) {
    var c = new channel<bool>(0);
    var c2 = new channel<bool>(0);
    var cʗ1 = c;
    goǃ(() => {
        awaitBlockedGoroutine(t, selectˢ, blockSelectSendSyncˢ, 1);
        ᐸꟷ(cʗ1);
    });
    var selᴛ8 = c.ᐸꟷ(true, ꓸꓸꓸ);
    var selᴛ9 = c2.ᐸꟷ(true, ꓸꓸꓸ);
    switch (select(selᴛ8, selᴛ9)) {
    case 0: {
        break;
    }
    case 1: {
        break;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string syncMutexLockˢ => ref syncMutexLockˢᵇ;
private static readonly @string syncMutexLockˢᵇ = "sync.Mutex.Lock"u8;
internal static ref readonly @string blockMutexˢ => ref blockMutexˢᵇ;
private static readonly @string blockMutexˢᵇ = "blockMutex"u8;

internal static partial void blockMutex(ж<testing.T> t) {
    ref var mu = ref heap(new sync.Mutex(), out var Ꮡmu);
    Ꮡmu.Lock();
    goǃ(() => {
        awaitBlockedGoroutine(t, syncMutexLockˢ, blockMutexˢ, 1);
        Ꮡmu.Unlock();
    });
    // Note: Unlock releases mu before recording the mutex event,
    // so it's theoretically possible for this to proceed and
    // capture the profile before the event is recorded. As long
    // as this is blocked before the unlock happens, it's okay.
    Ꮡmu.Lock();
}

internal static partial void blockMutexN(ж<testing.T> t, nint n, time.Duration d) {
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    ref var mu = ref heap(new sync.Mutex(), out var Ꮡmu);
    Ꮡmu.Lock();
    goǃ(() => {
        awaitBlockedGoroutine(t, syncMutexLockˢ, blockMutexˢ, n);
        time.Sleep(d);
        Ꮡmu.Unlock();
    });
    // Note: Unlock releases mu before recording the mutex event,
    // so it's theoretically possible for this to proceed and
    // capture the profile before the event is recorded. As long
    // as this is blocked before the unlock happens, it's okay.
    for (nint i = 0; i < n; i++) {
        Ꮡwg.Add(1);
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                Ꮡmu.Lock();
                Ꮡmu.Unlock();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string syncCondWaitˢ => ref syncCondWaitˢᵇ;
private static readonly @string syncCondWaitˢᵇ = "sync.Cond.Wait"u8;
internal static ref readonly @string blockCondˢ => ref blockCondˢᵇ;
private static readonly @string blockCondˢᵇ = "blockCond"u8;

internal static partial void blockCond(ж<testing.T> t) {
    ref var mu = ref heap(new sync.Mutex(), out var Ꮡmu);
    var c = sync.NewCond(new sync.MutexжLocker(Ꮡmu));
    Ꮡmu.Lock();
    var cʗ1 = c;
    goǃ(() => {
        awaitBlockedGoroutine(t, syncCondWaitˢ, blockCondˢ, 1);
        Ꮡmu.Lock();
        cʗ1.Signal();
        Ꮡmu.Unlock();
    });
    c.Wait();
    Ꮡmu.Unlock();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object blockProfileHasLessThan2ˢ = (@string)"block profile has less than 2 sample types"u8;
internal static readonly object blockProfileIsMissingˢ = (@string)"block profile is missing expected functions"u8;

// See http://golang.org/cl/299991.
public static void TestBlockProfileBias(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        nint rate = (nint)1000; // arbitrary value
        runtime.SetBlockProfileRate(rate);
        defer(runtime.SetBlockProfileRate, (nint)0, ref ᒐ);

        // simulate blocking events
        blockFrequentShort(rate);
        blockInfrequentLong(rate);

        ref var w = ref heap(new bytes.Buffer(), out var Ꮡw);
        Lookup(blockˢ).WriteTo(new pprof_internal_test_package.bytes_BufferжWriter(Ꮡw), 0);
        var (p, err) = profile.Parse(new pprof_internal_test_package.bytes_BufferжReader(Ꮡw));
        if (err != nil) {
            t.Fatalf("failed to parse profile: %v", err);
        }
        t.Logf("parsed proto: %s", p.OrTypedNil());

        var il = (float64)(-1D); // blockInfrequentLong duration
        var fs = (float64)(-1D); // blockFrequentShort duration
        foreach (var (_, s) in p.Sample) {
            foreach (var (_, l) in s.Location) {
                foreach (var (_, line) in l.Line) {
                    if (len((~s).Value) < 2) {
                        t.Fatal(blockProfileHasLessThan2ˢ);
                    }

                    if (line.Function.Name == "runtime/pprof.blockInfrequentLong"u8) {
                        il = (float64)(~s).Value[1];
                    } else if (line.Function.Name == "runtime/pprof.blockFrequentShort"u8) {
                        fs = (float64)(~s).Value[1];
                    }
                }
            }
        }
        if (il == -1D || fs == -1D) {
            t.Fatal(blockProfileIsMissingˢ);
        }

        // stddev of bias from 100 runs on local machine multiplied by 10x
        const float64 threshold = 0.2;
        {
            var bias = (il - fs) / il; if (math.Abs(bias) > threshold) {
                t.Fatalf("bias: abs(%f) > %f", bias, (float64)(threshold));
            } else {
                t.Logf("bias: abs(%f) < %f", bias, (float64)(threshold));
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// blockFrequentShort produces 100000 block events with an average duration of
// rate / 10.
internal static void blockFrequentShort(nint rate) {
    for (nint i = 0; i < 100000; i++) {
        blockevent((int64)(rate / 10), 1);
    }
}

// blockInfrequentLong produces 10000 block events with an average duration of
// rate.
internal static void blockInfrequentLong(nint rate) {
    for (nint i = 0; i < 10000; i++) {
        blockevent((int64)rate, 1);
    }
}

// Used by TestBlockProfileBias.
//
//go:linkname blockevent runtime.blockevent
/*linkname*/ internal static partial void blockevent(int64 cycles, nint skip) {
    runtime.blockevent(cycles, skip);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string mutexCyclesSecondˢ => ref mutexCyclesSecondˢᵇ;
private static readonly @string mutexCyclesSecondˢᵇ = "--- mutex:\ncycles/second="u8;
internal static ref readonly @string dD0xXdigitˢ => ref dD0xXdigitˢᵇ;
private static readonly @string dD0xXdigitˢᵇ = @"^\d+ \d+ @(?: 0x[[:xdigit:]]+)+"u8;
internal static ref readonly @string runtimePprofBlockMutexˢ => ref runtimePprofBlockMutexˢᵇ;
private static readonly @string runtimePprofBlockMutexˢᵇ = "^#.*runtime/pprof.blockMutex.*$"u8;
internal static ref readonly @string recordsˢ => ref recordsˢᵇ;
private static readonly @string recordsˢᵇ = "records"u8;

public static void TestMutexProfile(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // Generate mutex profile

        nint old = runtime.SetMutexProfileFraction(1);
        defer(runtime.SetMutexProfileFraction, old, ref ᒐ);
        if (old != 0) {
            t.Fatalf("need MutexProfileRate 0, got %d", old);
        }

        UntypedInt N = 100;
        time.Duration D = /* 100 * time.Millisecond */ 100000000;
        var start = time.Now();
        blockMutexN(t, N, D);
        var blockMutexNTime = time.Since(start);

        t.Run(debug1ˢ, (ж<testing.T> tΔ1) => {
            ref var w = ref heap(new strings.Builder(), out var Ꮡw);
            Lookup(mutexˢ).WriteTo(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡw), 1);
            @string prof = w.String();
            tΔ1.Logf("received profile: %v", prof);

            if (!strings_package.HasPrefix(prof, mutexCyclesSecondˢ)) {
                tΔ1.Errorf("Bad profile header:\n%v", prof);
            }
            prof = strings_package.Trim(prof, "\n");
            var lines = strings_package.Split(prof, "\n");
            if (len(lines) < 6) {
                tΔ1.Fatalf("expected >=6 lines, got %d %q\n%s", len(lines), prof, prof);
            }
            // checking that the line is like "35258904 1 @ 0x48288d 0x47cd28 0x458931"
            @string r2 = dD0xXdigitˢ;
            {
                var (ok, err) = regexp.MatchString(r2, lines[3]); if (err != nil || !ok) {
                    tΔ1.Errorf("%q didn't match %q", lines[3], r2);
                }
            }
            @string r3 = runtimePprofBlockMutexˢ;
            {
                var (ok, err) = regexp.MatchString(r3, lines[5]); if (err != nil || !ok) {
                    tΔ1.Errorf("%q didn't match %q", lines[5], r3);
                }
            }
            tΔ1.Log(prof);
        });
        t.Run(protoˢ, (ж<testing.T> tΔ2) => {
            // proto format
            ref var w = ref heap(new bytes.Buffer(), out var Ꮡw);
            Lookup(mutexˢ).WriteTo(new pprof_internal_test_package.bytes_BufferжWriter(Ꮡw), 0);
            var (p, err) = profile.Parse(new pprof_internal_test_package.bytes_BufferжReader(Ꮡw));
            if (err != nil) {
                tΔ2.Fatalf("failed to parse profile: %v", err);
            }
            tΔ2.Logf("parsed proto: %s", p.OrTypedNil());
            {
                var errΔ1 = p.CheckValid(); if (errΔ1 != nil) {
                    tΔ2.Fatalf("invalid profile: %v", errΔ1);
                }
            }

            var stks = profileStacks(p);
            foreach (var (_, want) in slice<slice<@string>>([
                slice<@string>(["sync.(*Mutex).Unlock", "runtime/pprof.blockMutexN.func1"])
            ])) {
                if (!containsStack(stks, want)) {
                    tΔ2.Errorf("No matching stack entry for %+v", want);
                }
            }

            nint i = 0;
            for (; i < len(p.SampleType); i++) {
                if (p.SampleType[i].Unit == "nanoseconds"u8) {
                    break;
                }
            }
            if (i >= len(p.SampleType)) {
                tΔ2.Fatalf("profile did not contain nanoseconds sample");
            }
            var total = (int64)0;
            foreach (var (_, s) in p.Sample) {
                total += (~s).Value[i];
            }
            // Want d to be at least N*D, but give some wiggle-room to avoid
            // a test flaking. Set an upper-bound proportional to the total
            // wall time spent in blockMutexN. Generally speaking, the total
            // contention time could be arbitrarily high when considering
            // OS scheduler delays, or any other delays from the environment:
            // time keeps ticking during these delays. By making the upper
            // bound proportional to the wall time in blockMutexN, in theory
            // we're accounting for all these possible delays.
            var d = (time.Duration)total;
            var lo = (time.Duration)(9000000000L);
            var hi = ((time.Duration)N) * blockMutexNTime * 11 / 10;
            if (d < lo || d > hi) {
                foreach (var (_, s) in p.Sample) {
                    tΔ2.Logf("sample: %s", (time.Duration)(~s).Value[i]);
                }
                tΔ2.Fatalf("profile samples total %v, want within range [%v, %v] (target: %v)", d, lo, hi, (time.Duration)(10000000000L));
            }
        });

        t.Run(recordsˢ, (ж<testing.T> tΔ3) => {
            // Record a mutex profile using the structured record API.
            slice<runtime.BlockProfileRecord> records = default;
            for (;;) {
                var (n, ok) = runtime.MutexProfile(records);
                if (ok) {
                    records = records.slice(0, n);
                    break;
                }
                records = new slice<runtime.BlockProfileRecord>(n * 2, () => new(nil));
            }

            // Check that we see the same stack trace as the proto profile. For
            // historical reason we expect a runtime.goexit root frame here that is
            // omitted in the proto profile.
            var stks = blockRecordStacks(records);
            var want = slice<@string>(["sync.(*Mutex).Unlock", "runtime/pprof.blockMutexN.func1", "runtime.goexit"]);
            if (!containsStack(stks, want)) {
                tΔ3.Errorf("No matching stack entry for %+v", want);
            }
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object didNotSeeExpectedˢ = (@string)"did not see expected function in profile"u8;

public static void TestMutexProfileRateAdjust(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        nint old = runtime.SetMutexProfileFraction(1);
        defer(runtime.SetMutexProfileFraction, old, ref ᒐ);
        if (old != 0) {
            t.Fatalf("need MutexProfileRate 0, got %d", old);
        }

        (int64 contentions, int64 delay) readProfile() {
            int64 contentionsΔ1 = default;
            int64 delayΔ1 = default;
            ref var w = ref heap(new bytes.Buffer(), out var Ꮡw);
            Lookup(mutexˢ).WriteTo(new pprof_internal_test_package.bytes_BufferжWriter(Ꮡw), 0);
            var (p, err) = profile.Parse(new pprof_internal_test_package.bytes_BufferжReader(Ꮡw));
            if (err != nil) {
                t.Fatalf("failed to parse profile: %v", err);
            }
            t.Logf("parsed proto: %s", p.OrTypedNil());
            {
                var errΔ1 = p.CheckValid(); if (errΔ1 != nil) {
                    t.Fatalf("invalid profile: %v", errΔ1);
                }
            }

            foreach (var (_, s) in p.Sample) {
                bool match = default;
                bool runtimeInternal = default;
                foreach (var (_, l) in s.Location) {
                    foreach (var (_, line) in l.Line) {
                        if (line.Function.Name == "runtime/pprof.blockMutex.func1"u8) {
                            match = true;
                        }
                        if (line.Function.Name == "runtime.unlock"u8) {
                            runtimeInternal = true;
                        }
                    }
                }
                if (match && !runtimeInternal) {
                    contentionsΔ1 += (~s).Value[0];
                    delayΔ1 += (~s).Value[1];
                }
            }
            return (contentionsΔ1, delayΔ1);
        }

        blockMutex(t);
        var (contentions, delay) = readProfile();
        if (contentions == 0) { // low-resolution timers can have delay of 0 in mutex profile
            t.Fatal(didNotSeeExpectedˢ);
        }
        runtime.SetMutexProfileFraction(0);
        var (newContentions, newDelay) = readProfile();
        if (newContentions != contentions || newDelay != delay) {
            t.Fatalf("sample value changed: got [%d, %d], want [%d, %d]", newContentions, newDelay, contentions, delay);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void func1(channel<nint> c) {
    ᐸꟷ(c);
}

internal static void func2(channel<nint> c) {
    ᐸꟷ(c);
}

internal static void func3(channel<nint> c) {
    ᐸꟷ(c);
}

internal static void func4(channel<nint> c) {
    ᐸꟷ(c);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string labelˢ => ref labelˢᵇ;
private static readonly @string labelˢᵇ = "label"u8;
internal static ref readonly @string selfLabelˢ => ref selfLabelˢᵇ;
private static readonly @string selfLabelˢᵇ = "self-label"u8;
internal static ref readonly @string selfValueˢ => ref selfValueˢᵇ;
private static readonly @string selfValueˢᵇ = "self-value"u8;
internal static ref readonly @string fingLabelˢ => ref fingLabelˢᵇ;
private static readonly @string fingLabelˢᵇ = "fing-label"u8;
internal static ref readonly @string fingValueˢ => ref fingValueˢᵇ;
private static readonly @string fingValueˢᵇ = "fing-value"u8;

public static partial void TestGoroutineCounts(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var orderedPrefixᴷ = new stackAppend<@string>();

        // Setting GOMAXPROCS to 1 ensures we can force all goroutines to the
        // desired blocking point.
        defer(runtime.GOMAXPROCS, runtime.GOMAXPROCS(1), ref ᒐ);

        var c = new channel<nint>(0);
        for (nint i = 0; i < 100; i++) {
            switch (ᐧ) {
            case {} when i % 10 is 0: {
                goǃ(func1, c);
                break;
            }
            case {} when i % 2 is 0: {
                goǃ(func2, c);
                break;
            }
            default: {
                goǃ(func3, c);
                break;
            }}
            // Let goroutines block on channel
            for (nint j = 0; j < 5; j++) {
                runtime.Gosched();
            }
        }
        var ctx = context.Background();

        // ... and again, with labels this time (just with fewer iterations to keep
        // sorting deterministic).
        var cʗ1 = c;
        Do(ctx, Labels(labelˢ, valueˢ), [MethodImpl(MethodImplOptions.NoInlining)] (context.Context _) => {
            for (nint i = 0; i < 89; i++) {
                switch (ᐧ) {
                case {} when i % 10 is 0: {
                    goǃ(func1, cʗ1);
                    break;
                }
                case {} when i % 2 is 0: {
                    goǃ(func2, cʗ1);
                    break;
                }
                default: {
                    goǃ(func3, cʗ1);
                    break;
                }}
                // Let goroutines block on channel
                for (nint j = 0; j < 5; j++) {
                    runtime.Gosched();
                }
            }
        });

        SetGoroutineLabels(WithLabels(context.Background(), Labels(selfLabelˢ, selfValueˢ)));
        defer(SetGoroutineLabels, context.Background(), ref ᒐ);

        var garbage = @new<ж<nint>>();
        var fingReady = new channel<EmptyStruct>(0);
        var cʗ2 = c;
        var fingReadyʗ1 = fingReady;
        runtime.SetFinalizer(garbage.OrTypedNil(), (ж<ж<nint>> v) => {
            var cʗ3 = cʗ2;
            var fingReadyʗ2 = fingReadyʗ1;
            Do(context.Background(), Labels(fingLabelˢ, fingValueˢ), (context.Context ctxΔ1) => {
                close(fingReadyʗ2);
                ᐸꟷ(cʗ3);
            });
        });
        garbage = default;
        for (nint i = 0; i < 2; i++) {
            runtime.GC();
        }
        ᐸꟷ(fingReady);

        ref var w = ref heap(new bytes.Buffer(), out var Ꮡw);
        var goroutineProf = Lookup(goroutineˢ);

        // Check debug profile
        goroutineProf.WriteTo(new pprof_internal_test_package.bytes_BufferжWriter(Ꮡw), 1);
        @string prof = Ꮡw.String();

        ref var labels = ref heap<global::go.runtime.pprof_package.labelMap>(new labelMap(label.NewSet(Labels(labelˢ, valueˢ).list)), out var Ꮡlabels);
        @string labelStr = "\n# labels: "u8 + Ꮡlabels.String();
        ref var selfLabel = ref heap<global::go.runtime.pprof_package.labelMap>(new labelMap(label.NewSet(Labels(selfLabelˢ, selfValueˢ).list)), out var ᏑselfLabel);
        @string selfLabelStr = "\n# labels: "u8 + ᏑselfLabel.String();
        ref var fingLabel = ref heap<global::go.runtime.pprof_package.labelMap>(new labelMap(label.NewSet(Labels(fingLabelˢ, fingValueˢ).list)), out var ᏑfingLabel);
        @string fingLabelStr = "\n# labels: "u8 + ᏑfingLabel.String();
        var orderedPrefix = slice<@string>([
            "\n50 @ ",
            "\n44 @", labelStr,
            "\n40 @",
            "\n36 @", labelStr,
            "\n10 @",
            "\n9 @", labelStr,
            "\n1 @"]);
        if (!containsInOrder(prof, append(orderedPrefixᴷ, orderedPrefix, selfLabelStr).ꓸꓸꓸ)) {
            t.Errorf("expected sorted goroutine counts with Labels:\n%s", prof);
        }
        if (!containsInOrder(prof, append(orderedPrefix, fingLabelStr).ꓸꓸꓸ)) {
            t.Errorf("expected sorted goroutine counts with Labels:\n%s", prof);
        }

        // Check proto profile
        w.Reset();
        goroutineProf.WriteTo(new pprof_internal_test_package.bytes_BufferжWriter(Ꮡw), 0);
        var (p, err) = profile.Parse(new pprof_internal_test_package.bytes_BufferжReader(Ꮡw));
        if (err != nil) {
            t.Errorf("error parsing protobuf profile: %v", err);
        }
        {
            var errΔ1 = p.CheckValid(); if (errΔ1 != nil) {
                t.Errorf("protobuf profile is invalid: %v", errΔ1);
            }
        }
        var expectedLabels = new map<int64, map<@string, @string>>{
            [50] = new map<@string, @string>{},
            [44] = new map<@string, @string>{["label"] = "value"},
            [40] = new map<@string, @string>{},
            [36] = new map<@string, @string>{["label"] = "value"},
            [10] = new map<@string, @string>{},
            [9] = new map<@string, @string>{["label"] = "value"},
            [1] = new map<@string, @string>{["self-label"] = "self-value", ["fing-label"] = "fing-value"}
        };
        if (!containsCountsLabels(p, expectedLabels)) {
            t.Errorf("expected count profile to contain goroutines with counts and labels %v, got %v",
                expectedLabels, p.OrTypedNil());
        }

        close(c);

        time.Sleep(10 * time.Millisecond); // let goroutines exit
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static bool containsInOrder(@string s, params ꓸꓸꓸstring allʗp) {
    var all = allʗp.sslice();

    foreach (var (_, t) in all) {
        bool ok = default;
        {
            (_, s, ok) = strings_package.Cut(s, t); if (!ok) {
                return false;
            }
        }
    }
    return true;
}

internal partial struct containsCountsLabels_nkey /*dyn*/ {
    internal int64 count;
    internal @string key, val;
}

internal static bool containsCountsLabels(ж<profile.Profile> Ꮡprof, map<int64, map<@string, @string>> countLabels) {
    ref var prof = ref Ꮡprof.DerefOrNull();

    var m = new map<int64, nint>();
    var n = new map<containsCountsLabels_nkey, nint>();
    foreach (var (c, kv) in countLabels) {
        m[c]++;
        foreach (var (k, v) in kv) {
            n[new containsCountsLabels_nkey(
                count: c,
                key: k,
                val: v
            )]++;
        }
    }
    foreach (var (_, s) in prof.Sample) {
        // The count is the single value in the sample
        if (len((~s).Value) != 1) {
            return false;
        }
        m[(~s).Value[0]]--;
        foreach (var (k, vs) in s.Label) {
            foreach (var (_, v) in vs) {
                n[new containsCountsLabels_nkey(
                    count: (~s).Value[0],
                    key: k,
                    val: v
                )]--;
            }
        }
    }
    foreach (var (_, nΔ1) in m) {
        if (nΔ1 > 0) {
            return false;
        }
    }
    foreach (var (_, ncnt) in n) {
        if (ncnt != 0) {
            return false;
        }
    }
    return true;
}

// Inlining disabled to make identification simpler.
//
//go:noinline
internal static partial void goroutineLeakExample() {
    ᐸꟷ(new channel<EmptyStruct>(0));
    throw panic("unreachable");
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string goroutineleakProfileˢ => ref goroutineleakProfileˢᵇ;
private static readonly @string goroutineleakProfileˢᵇ = "goroutineleak profile: total "u8;
internal static ref readonly @string runtimeGoparkˢ => ref runtimeGoparkˢᵇ;
private static readonly @string runtimeGoparkˢᵇ = "runtime.gopark"u8;
internal static ref readonly @string runtimeChanrecvˢ => ref runtimeChanrecvˢᵇ;
private static readonly @string runtimeChanrecvˢᵇ = "runtime.chanrecv"u8;
internal static ref readonly @string runtimeChanrecv1ˢ => ref runtimeChanrecv1ˢᵇ;
private static readonly @string runtimeChanrecv1ˢᵇ = "runtime.chanrecv1"u8;
internal static ref readonly @string runtimePprofˢ => ref runtimePprofˢᵇ;
private static readonly @string runtimePprofˢᵇ = "runtime/pprof.goroutineLeakExample"u8;
internal static ref readonly @string runtimePprofˢ2 => ref runtimePprofˢ2ᵇ;
private static readonly @string runtimePprofˢ2ᵇ = "runtime/pprof.TestGoroutineLeakProfileConcurrency.func4"u8;
internal static ref readonly @string profileContainsLeakˢ => ref profileContainsLeakˢᵇ;
private static readonly @string profileContainsLeakˢᵇ = "profile contains leak"u8;
internal static ref readonly @string leakPersistsBetweenˢ => ref leakPersistsBetweenˢᵇ;
private static readonly @string leakPersistsBetweenˢᵇ = "leak persists between sequential profiling runs"u8;
internal static ref readonly @string overlappingProfileˢ => ref overlappingProfileˢᵇ;
private static readonly @string overlappingProfileˢᵇ = "overlapping profile requests"u8;
internal static ref readonly @string overlappingGoroutineAndˢ => ref overlappingGoroutineAndˢᵇ;
private static readonly @string overlappingGoroutineAndˢᵇ = "overlapping goroutine and goroutine leak profile requests"u8;

public static partial void TestGoroutineLeakProfileConcurrency(ж<testing.T> t) {
    UntypedInt leakCount = 3;

    testenv.MustHaveParallelism(new pprof_internal_test_package.testing_TжTB(t));
    var regexLeakCount = regexp.MustCompile(goroutineleakProfileˢ);
    var whiteSpace = regexp.MustCompile("\\s+");

    // Regular goroutine profile. Used to check that there is no interference between
    // the two profile types.
    var goroutineProf = Lookup(goroutineˢ);
    var goroutineLeakProf = goroutineLeakProfile;

    // We use this helper to count the total number of leaked goroutines in a text profile.
    var regexLeakCountʗ1 = regexLeakCount;
    var whiteSpaceʗ1 = whiteSpace;
    int64 countLeaks(ж<testing.T> tΔ1, @string profText) {
        tΔ1.Helper();

        // Strip the profile header
        var parts = regexLeakCountʗ1.Split(profText, -1);
        if (len(parts) < 2) {
            tΔ1.Fatalf("goroutineleak profile does not contain 'goroutineleak profile: total ': %s\nparts: %v", profText, parts);
        }

        parts = whiteSpaceʗ1.Split(parts[1], -1);

        var (count, err) = strconv.ParseInt(parts[0], 10, 64);
        if (err != nil) {
            tΔ1.Fatalf("goroutineleak profile count is not a number: %s\nerror: %v", profText, err);
        }
        return count;
    }

    // checkFrame looks for a specific frame in the stack.
    //
    // i is the location index in the profile and j is the location line index for the location.
    // (Inlining may cause aliasing to the same location.)
    void checkFrame(ж<testing.T> tΔ2, nint i, nint j, slice<ж<profile.Location>> locations, @string funcName) {
        if (len(locations) <= i) {
            tΔ2.Errorf("leaked goroutine stack locations: out of range index %d, length %d", i, len(locations));
            return;
        }
        var location = locations[i];
        if (len(location.Line) <= j) {
            tΔ2.Errorf("leaked goroutine stack location lines: out of range index %d, length %d", j, len(location.Line));
            return;
        }
        if (location.Line[j].Function.Name != funcName) {
            tΔ2.Errorf("leaked goroutine stack expected %s as location[%d].Line[%d] but found %s (%s:%d)", funcName, i, j, location.Line[j].Function.Name, location.Line[j].Function.Filename, location.Line[j].ΔLine);
        }
    }

    // checkLeakStack hooks into profile parsing and performs validation, looking for specific stacks for
    // the goroutines we'll leak in this test.
    var checkFrameʗ1 = checkFrame;
    Action<uintptr, slice<ж<profile.Location>>, map<@string, slice<@string>>> checkLeakStack(ж<testing.T> tΔ3) {
        var checkFrameʗ2 = checkFrameʗ1;
        return (uintptr pc, slice<ж<profile.Location>> locations, map<@string, slice<@string>> _) => {
            if (pc != leakCount) {
                tΔ3.Errorf("expected %d leaked goroutines with specific stack configurations, but found %d", (nint)(leakCount), pc);
                return;
            }
            if (len(locations) < 4 || len(locations) > 5) {
                @string message = fmt.Sprintf("leaked goroutine stack expected 4 or 5 locations but found %d"u8, len(locations));
                foreach (var (_, location) in locations) {
                    foreach (var (_, line) in location.Line) {
                        message += fmt.Sprintf("\n%s:%d"u8, line.Function.Name, line.ΔLine);
                    }
                }
                tΔ3.Errorf("%s", message);
                return;
            }
            // We expect a receive operation. This is the typical stack.
            checkFrameʗ2(tΔ3, 0, 0, locations, runtimeGoparkˢ);
            checkFrameʗ2(tΔ3, 1, 0, locations, runtimeChanrecvˢ);
            checkFrameʗ2(tΔ3, 2, 0, locations, runtimeChanrecv1ˢ);
            checkFrameʗ2(tΔ3, 3, 0, locations, runtimePprofˢ);
            if (len(locations) == 5) {
                checkFrameʗ2(tΔ3, 4, 0, locations, runtimePprofˢ2);
            }
        };
    }

    // Leak some goroutines that will feature in the goroutine leak profile
    UntypedInt totalLeaked = /* leakCount * 2 */ 6;
    for (nint i = 0; i < leakCount; i++) {
        goǃ(goroutineLeakExample);
        goǃ(() => {
            // Leak another goroutine that will feature a slightly different stack.
            // This includes the frame runtime/pprof.TestGoroutineLeakProfileConcurrency.func1.
            goroutineLeakExample();
            throw panic("unreachable");
        });
    }

    // Wait for the goroutines to leak. We might wait here until the timeout,
    // but this is better than intermittent flakes because we didn't wait long
    // enough. If we actually time out, then there's likely a bug.
    nint attempts = 0;
    var startTime = time.Now();
    var waitFor = 10 * time.Millisecond;
    for (;;) {
        //
        // If they never get detected, we'll get a timeout.
        time.Sleep(waitFor);

        ref var w = ref heap(new strings.Builder(), out var Ꮡw);
        goroutineLeakProf.WriteTo(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡw), 1);
        var n = countLeaks(t, w.String());
        if (n >= totalLeaked) {
            break;
        }

        // Log some messages so if a timeout is seen
        attempts++;
        t.Logf("waiting for leak: attempt %d (t=%s): found %d leaked goroutines", attempts, time.Since(startTime), n);

        // Wait a little longer to avoid spamming the log.
        waitFor *= 2;
        if (waitFor > time.ΔSecond) {
            waitFor = time.ΔSecond;
        }
    }

    var checkLeakStackʗ1 = checkLeakStack;
    var goroutineLeakProfʗ1 = goroutineLeakProf;
    t.Run(profileContainsLeakˢ, (ж<testing.T> tΔ4) => {
        ref var w = ref heap(new strings.Builder(), out var Ꮡw);
        goroutineLeakProfʗ1.WriteTo(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡw), 0);
        parseProfile(tΔ4, slice<byte>(w.String()), checkLeakStackʗ1(tΔ4));
    });

    var checkLeakStackʗ2 = checkLeakStack;
    var goroutineLeakProfʗ2 = goroutineLeakProf;
    t.Run(leakPersistsBetweenˢ, (ж<testing.T> tΔ5) => {
        for (nint i = 0; i < 2; i++) {
            ref var w = ref heap(new strings.Builder(), out var Ꮡw);
            goroutineLeakProfʗ2.WriteTo(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡw), 0);
            parseProfile(tΔ5, slice<byte>(w.String()), checkLeakStackʗ2(tΔ5));
        }
    });

    // Concurrent calls to the goroutine leak profiler should not trigger data races
    // or corruption.
    void quickCheckForGoroutine(ж<testing.T> tΔ6, @string profType, @string leak, @string profText) {
        if (!strings_package.Contains(profText, leak)) {
            tΔ6.Errorf("%s profile does not contain expected leaked goroutine %s: %s", profType, leak, profText);
        }
    }
    var countLeaksʗ1 = countLeaks;
    var goroutineLeakProfʗ3 = goroutineLeakProf;
    var quickCheckForGoroutineʗ1 = quickCheckForGoroutine;
    t.Run(overlappingProfileˢ, (ж<testing.T> tΔ7) => {
        GoFrame ᒐ = default;
        try {
            ref var ctx = ref heap<context.Context>(context.Background(), out var Ꮡctx);
            (Ꮡctx.ValueSlot, var cancel) = context.WithTimeout(Ꮡctx.ValueSlot, time.ΔSecond);
            var cancelʗ1 = cancel;
            defer(() => cancelʗ1(), ref ᒐ);

            ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
            for (nint i = 0; i < 2; i++) {
                Ꮡwg.Add(1);
                var countLeaksʗ2 = countLeaksʗ1;
                var goroutineLeakProfʗ4 = goroutineLeakProfʗ3;
                var quickCheckForGoroutineʗ2 = quickCheckForGoroutineʗ1;
                Do(Ꮡctx.ValueSlot, Labels("i", fmt.Sprint(i)), [MethodImpl(MethodImplOptions.NoInlining)] (context.Context _) => {
                    var countLeaksʗ3 = countLeaksʗ2;
                    var goroutineLeakProfʗ5 = goroutineLeakProfʗ4;
                    var quickCheckForGoroutineʗ3 = quickCheckForGoroutineʗ2;
                    goǃ(() => {
                        GoFrame ᒐ = default;
                        try {
                            defer(Ꮡwg.Done, ref ᒐ);
                            while (Ꮡctx.ValueSlot.Err() == nil) {
                                ref var w = ref heap(new strings.Builder(), out var Ꮡw);
                                goroutineLeakProfʗ5.WriteTo(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡw), 1);
                                {
                                    var n = countLeaksʗ3(tΔ7, w.String()); if (n != totalLeaked) {
                                        tΔ7.Errorf("expected %d goroutines leaked, got %d: %s", (nint)(totalLeaked), n, w.String());
                                    }
                                }
                                quickCheckForGoroutineʗ3(tΔ7, goroutineleakˢ, runtimePprofˢ, w.String());
                            }
                        }
                        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                        finally { ᒐ.Run(); }
                    });
                });
            }
            Ꮡwg.Wait();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });

    // Concurrent calls to the goroutine leak profiler should not trigger data races
    // or corruption, or interfere with regular goroutine profiles.
    var countLeaksʗ4 = countLeaks;
    var goroutineLeakProfʗ6 = goroutineLeakProf;
    var goroutineProfʗ1 = goroutineProf;
    var quickCheckForGoroutineʗ4 = quickCheckForGoroutine;
    t.Run(overlappingGoroutineAndˢ, (ж<testing.T> tΔ8) => {
        GoFrame ᒐ = default;
        try {
            ref var ctx = ref heap<context.Context>(context.Background(), out var Ꮡctx);
            (Ꮡctx.ValueSlot, var cancel) = context.WithTimeout(Ꮡctx.ValueSlot, time.ΔSecond);
            var cancelʗ2 = cancel;
            defer(() => cancelʗ2(), ref ᒐ);

            ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
            for (nint i = 0; i < 2; i++) {
                Ꮡwg.Add(2);
                var countLeaksʗ5 = countLeaksʗ4;
                var goroutineLeakProfʗ7 = goroutineLeakProfʗ6;
                var goroutineProfʗ2 = goroutineProfʗ1;
                var quickCheckForGoroutineʗ5 = quickCheckForGoroutineʗ4;
                Do(Ꮡctx.ValueSlot, Labels("i", fmt.Sprint(i)), [MethodImpl(MethodImplOptions.NoInlining)] (context.Context _) => {
                    var countLeaksʗ6 = countLeaksʗ5;
                    var goroutineLeakProfʗ8 = goroutineLeakProfʗ7;
                    var quickCheckForGoroutineʗ6 = quickCheckForGoroutineʗ5;
                    goǃ(() => {
                        GoFrame ᒐ = default;
                        try {
                            defer(Ꮡwg.Done, ref ᒐ);
                            while (Ꮡctx.ValueSlot.Err() == nil) {
                                ref var w = ref heap(new strings.Builder(), out var Ꮡw);
                                goroutineLeakProfʗ8.WriteTo(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡw), 1);
                                {
                                    var n = countLeaksʗ6(tΔ8, w.String()); if (n != totalLeaked) {
                                        tΔ8.Errorf("expected %d goroutines leaked, got %d: %s", (nint)(totalLeaked), n, w.String());
                                    }
                                }
                                quickCheckForGoroutineʗ6(tΔ8, goroutineleakˢ, runtimePprofˢ, w.String());
                            }
                        }
                        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                        finally { ᒐ.Run(); }
                    });
                    var goroutineProfʗ3 = goroutineProfʗ2;
                    var quickCheckForGoroutineʗ7 = quickCheckForGoroutineʗ5;
                    goǃ(() => {
                        GoFrame ᒐ = default;
                        try {
                            defer(Ꮡwg.Done, ref ᒐ);
                            while (Ꮡctx.ValueSlot.Err() == nil) {
                                ref var w = ref heap(new strings.Builder(), out var Ꮡw);
                                goroutineProfʗ3.WriteTo(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡw), 1);
                                // The regular goroutine profile should see the leaked
                                // goroutines. We simply check that the goroutine leak
                                // profile does not corrupt the goroutine profile state.
                                quickCheckForGoroutineʗ7(tΔ8, goroutineˢ, runtimePprofˢ, w.String());
                            }
                        }
                        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                        finally { ᒐ.Run(); }
                    });
                });
            }
            Ꮡwg.Wait();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string runtimePprofRuntimeˢ => ref runtimePprofRuntimeˢᵇ;
private static readonly @string runtimePprofRuntimeˢᵇ = "\truntime/pprof.runtime_goroutineProfileWithLabels+"u8;
internal static ref readonly @string runtimeRunFinalizersˢ => ref runtimeRunFinalizersˢᵇ;
private static readonly @string runtimeRunFinalizersˢᵇ = "runtime.runFinalizers"u8;
internal static ref readonly @string runtimeRunCleanupsˢ => ref runtimeRunCleanupsˢᵇ;
private static readonly @string runtimeRunCleanupsˢᵇ = "runtime.runCleanups"u8;
internal static ref readonly @string finalizerNotPresentˢ => ref finalizerNotPresentˢᵇ;
private static readonly @string finalizerNotPresentˢᵇ = "finalizer not present"u8;
internal static ref readonly @string finalizerPresentˢ => ref finalizerPresentˢᵇ;
private static readonly @string finalizerPresentˢᵇ = "finalizer present"u8;
internal static ref readonly @string goroutineLaunchesˢ => ref goroutineLaunchesˢᵇ;
private static readonly @string goroutineLaunchesˢᵇ = "goroutine launches"u8;

// T is a pointer type so it won't be allocated by the tiny
// allocator, which can lead to its finalizer not being called
// during this test
internal partial class TestGoroutineProfileConcurrency_T /*ж<byte>*/;

public static void TestGoroutineProfileConcurrency(ж<testing.T> t) {
    testenv.MustHaveParallelism(new pprof_internal_test_package.testing_TжTB(t));

    var goroutineProf = Lookup(goroutineˢ);

    nint profilerCalls(@string s) => strings_package.Count(s, runtimePprofRuntimeˢ);

    bool includesFinalizerOrCleanup(@string s) => strings_package.Contains(s, runtimeRunFinalizersˢ) || strings_package.Contains(s, runtimeRunCleanupsˢ);

    // Concurrent calls to the goroutine profiler should not trigger data races
    // or corruption.
    var goroutineProfʗ1 = goroutineProf;
    var profilerCallsʗ1 = profilerCalls;
    t.Run(overlappingProfileˢ, (ж<testing.T> tΔ1) => {
        GoFrame ᒐ = default;
        try {
            ref var ctx = ref heap<context.Context>(context.Background(), out var Ꮡctx);
            (Ꮡctx.ValueSlot, var cancel) = context.WithTimeout(Ꮡctx.ValueSlot, (time.Duration)(10000000000L));
            var cancelʗ1 = cancel;
            defer(() => cancelʗ1(), ref ᒐ);

            ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
            for (nint i = 0; i < 2; i++) {
                Ꮡwg.Add(1);
                var cancelʗ2 = cancel;
                var goroutineProfʗ2 = goroutineProfʗ1;
                var profilerCallsʗ2 = profilerCallsʗ1;
                Do(Ꮡctx.ValueSlot, Labels("i", fmt.Sprint(i)), [MethodImpl(MethodImplOptions.NoInlining)] (context.Context _Δp0) => {
                    var cancelʗ3 = cancelʗ2;
                    var goroutineProfʗ3 = goroutineProfʗ2;
                    var profilerCallsʗ3 = profilerCallsʗ2;
                    goǃ(() => {
                        GoFrame ᒐ = default;
                        try {
                            defer(Ꮡwg.Done, ref ᒐ);
                            while (Ꮡctx.ValueSlot.Err() == nil) {
                                ref var w = ref heap(new strings.Builder(), out var Ꮡw);
                                goroutineProfʗ3.WriteTo(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡw), 1);
                                @string prof = w.String();
                                nint count = profilerCallsʗ3(prof);
                                if (count >= 2) {
                                    tΔ1.Logf("prof %d\n%s", count, prof);
                                    cancelʗ3();
                                }
                            }
                        }
                        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                        finally { ᒐ.Run(); }
                    });
                });
            }
            Ꮡwg.Wait();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });

    // The finalizer goroutine should not show up in most profiles, since it's
    // marked as a system goroutine when idle.
    var goroutineProfʗ4 = goroutineProf;
    var includesFinalizerOrCleanupʗ1 = includesFinalizerOrCleanup;
    t.Run(finalizerNotPresentˢ, (ж<testing.T> tΔ2) => {
        ref var w = ref heap(new strings.Builder(), out var Ꮡw);
        goroutineProfʗ4.WriteTo(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡw), 1);
        @string prof = w.String();
        if (includesFinalizerOrCleanupʗ1(prof)) {
            tΔ2.Errorf("profile includes finalizer or cleanup (but should be marked as system):\n%s", prof);
        }
    });

    // The finalizer goroutine should show up when it's running user code.
    var goroutineProfʗ5 = goroutineProf;
    var includesFinalizerOrCleanupʗ2 = includesFinalizerOrCleanup;
    t.Run(finalizerPresentˢ, (ж<testing.T> tΔ3) => {
        GoFrame ᒐ = default;
        try {
            var obj = @new<TestGoroutineProfileConcurrency_T>();
            var (ch1, ch2) = (new channel<nint>(0), new channel<nint>(0));
            defer(ᴛ1 => close(ᴛ1), ch2, ref ᒐ);
            var ch1ʗ1 = ch1;
            var ch2ʗ1 = ch2;
            runtime.SetFinalizer(obj.OrTypedNil(), (any _Δp0) => {
                close(ch1ʗ1);
                ᐸꟷ(ch2ʗ1);
            });
            obj = default;
            for (nint i = 10; i >= 0; i--) {
                var selᴛ10 = ch1;
                switch (trySelect(ᐸꟷ(selᴛ10, ꓸꓸꓸ))) {
                case 0 when selᴛ10.ꟷᐳ(out _): {
                    break;
                }
                default: {
                    if (i == 0) {
                        tΔ3.Fatalf("finalizer did not run");
                    }
                    runtime.GC();
                    break;
                }}
            }
            ref var w = ref heap(new strings.Builder(), out var Ꮡw);
            goroutineProfʗ5.WriteTo(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡw), 1);
            @string prof = w.String();
            if (!includesFinalizerOrCleanupʗ2(prof)) {
                tΔ3.Errorf("profile does not include finalizer (and it should be marked as user):\n%s", prof);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });

    // Check that new goroutines only show up in order.
    var goroutineProfʗ6 = goroutineProf;
    var testLaunches = [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ4) => {
        GoFrame ᒐ = default;
        try {
            ref var done = ref heap(new sync.WaitGroup(), out var Ꮡdone);
            defer(Ꮡdone.Wait, ref ᒐ);

            ref var ctx = ref heap<context.Context>(context.Background(), out var Ꮡctx);
            (Ꮡctx.ValueSlot, var cancel) = context.WithCancel(Ꮡctx.ValueSlot);
            var cancelʗ4 = cancel;
            defer(() => cancelʗ4(), ref ᒐ);

            var ch = new channel<nint>(0);
            defer(ᴛ1 => close(ᴛ1), ch, ref ᒐ);

            ref var ready = ref heap(new sync.WaitGroup(), out var Ꮡready);

            // These goroutines all survive until the end of the subtest, so we can
            // check that a (numbered) goroutine appearing in the profile implies
            // that all older goroutines also appear in the profile.
            Ꮡready.Add(1);
            Ꮡdone.Add(1);
            var chʗ1 = ch;
            goǃ([MethodImpl(MethodImplOptions.NoInlining)] () => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡdone.Done, ref ᒐ);
                    for (nint i = 0; Ꮡctx.ValueSlot.Err() == nil; i++) {
                        // Use SetGoroutineLabels rather than Do we can always expect an
                        // extra goroutine (this one) with most recent label.
                        SetGoroutineLabels(WithLabels(Ꮡctx.ValueSlot, Labels(tΔ4.Name() + "-loop-i"u8, fmt.Sprint(i))));
                        Ꮡdone.Add(1);
                        var chʗ2 = chʗ1;
                        goǃ(() => {
                            ᐸꟷ(chʗ2);
                            Ꮡdone.Done();
                        });
                        for (nint j = 0; j < i; j++) {
                            // Spin for longer and longer as the test goes on. This
                            // goroutine will do O(N^2) work with the number of
                            // goroutines it launches. This should be slow relative to
                            // the work involved in collecting a goroutine profile,
                            // which is O(N) with the high-water mark of the number of
                            // goroutines in this process (in the allgs slice).
                            runtime.Gosched();
                        }
                        if (i == 0) {
                            Ꮡready.Done();
                        }
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });

            // Short-lived goroutines exercise different code paths (goroutines with
            // status _Gdead, for instance). This churn doesn't have behavior that
            // we can test directly, but does help to shake out data races.
            Ꮡready.Add(1);
            ref var churn = ref heap<Action<nint>>(out var Ꮡchurn);
            Ꮡchurn.ValueSlot = [MethodImpl(MethodImplOptions.NoInlining)] (nint i) => {
                SetGoroutineLabels(WithLabels(Ꮡctx.ValueSlot, Labels(tΔ4.Name() + "-churn-i"u8, fmt.Sprint(i))));
                if (i == 0) {
                    Ꮡready.Done();
                } else if (i % 16 == 0) {
                    // Yield on occasion so this sequence of goroutine launches
                    // doesn't monopolize a P. See issue #52934.
                    runtime.Gosched();
                }
                if (Ꮡctx.ValueSlot.Err() == nil) {
                    goǃ(Ꮡchurn.ValueSlot, i + 1);
                }
            };
            goǃ(() => {
                Ꮡchurn.ValueSlot(0);
            });

            Ꮡready.Wait();

            ref var w = ref heap(new array<bytes.Buffer>(3), out var Ꮡw);
            foreach (var i in range(w)) {
                goroutineProfʗ6.WriteTo(new pprof_internal_test_package.bytes_BufferжWriter(Ꮡw.at<bytes.Buffer>(i)), 0);
            }
            foreach (var i in range(w)) {
                var (p, err) = profile.Parse(new pprof_internal_test_package.bytes_ReaderжReader(bytes.NewReader(w[i].Bytes())));
                if (err != nil) {
                    tΔ4.Errorf("error parsing protobuf profile: %v", err);
                }

                // High-numbered loop-i goroutines imply that every lower-numbered
                // loop-i goroutine should be present in the profile too.
                var counts = new map<@string, nint>();
                foreach (var (_, s) in p.Sample) {
                    var label = s.Label[tΔ4.Name() + "-loop-i"u8];
                    if (len(label) > 0) {
                        counts[label[0]]++;
                    }
                }
                for ((nint j, nint max) = (0, len(counts) - 1); j <= max; j++) {
                    nint n = counts[fmt.Sprint(j)];
                    if (n == 1 || (n == 2 && j == max)) {
                        continue;
                    }
                    tΔ4.Errorf("profile #%d's goroutines with label loop-i:%d; %d != 1 (or 2 for the last entry, %d)",
                        i + 1, j, n, max);
                    tΔ4.Logf("counts %v", counts);
                    break;
                }
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    };

    nint runs = 100;
    if (testing.Short()) {
        runs = 5;
    }
    for (nint i = 0; i < runs; i++) {
        // Run multiple times to shake out data races
        t.Run(goroutineLaunchesˢ, testLaunches);
    }
}

// Regression test for #69998.
public static partial void TestGoroutineProfileCoro(ж<testing.T> t) {
    testenv.MustHaveParallelism(new pprof_internal_test_package.testing_TжTB(t));

    var goroutineProf = Lookup(goroutineˢ);

    // Set up a goroutine to just create and run coroutine goroutines all day.
    void iterFunc() {
        GoFrame ᒐ = default;
        try {
            var (p, stop) = iter.Pull2(
                (Func<nint, nint, bool> yield) => {
                    for (nint i = 0; i < 10000; i++) {
                        if (!yield(i, i)) {
                            return;
                        }
                    }
                });
            var stopʗ1 = stop;
            defer(stopʗ1, ref ᒐ);
            for (;;) {
                var (_, _, ok) = p();
                if (!ok) {
                    break;
                }
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    var done = new channel<EmptyStruct>(0);
    Ꮡwg.Add(1);
    var doneʗ1 = done;
    var iterFuncʗ1 = iterFunc;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(Ꮡwg.Done, ref ᒐ);
            for (;;) {
                iterFuncʗ1();
                var selᴛ11 = doneʗ1;
                switch (trySelect(ᐸꟷ(selᴛ11, ꓸꓸꓸ))) {
                case 0 when selᴛ11.ꟷᐳ(out _): {
                    break;
                }
                default: {
                    break;
                }}
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });

    // Take a goroutine profile. If the bug in #69998 is present, this will crash
    // with high probability. We don't care about the output for this bug.
    goroutineProf.WriteTo(io.Discard, 1);
}

// T is a pointer type so it won't be allocated by the tiny
// allocator, which can lead to its finalizer not being called
// during this test.
internal partial class TestGoroutineProfileIssue74090_T /*ж<byte>*/;

// This test tries to provoke a situation wherein the finalizer goroutine is
// erroneously inspected by the goroutine profiler in such a way that could
// cause a crash. See go.dev/issue/74090.
public static void TestGoroutineProfileIssue74090(ж<testing.T> t) {
    var objsᴷ = new stackAppend<ж<TestGoroutineProfileIssue74090_T>>();

    testenv.MustHaveParallelism(new pprof_internal_test_package.testing_TжTB(t));

    var goroutineProf = Lookup(goroutineˢ);
    foreach (var _ᴛ1 in range(10)) {
        // We use finalizers for this test because finalizers transition between
        // system and user goroutine on each call, since there's substantially
        // more work to do to set up a finalizer call. Cleanups, on the other hand,
        // transition once for a whole batch, and so are less likely to trigger
        // the failure. Under stress testing conditions this test fails approximately
        // 5 times every 1000 executions on a 64 core machine without the appropriate
        // fix, which is not ideal but if this test crashes at all, it's a clear
        // signal that something is broken.
        slice<ж<TestGoroutineProfileIssue74090_T>> objs = default;
        foreach (var _ᴛ2 in range(10000)) {
            var obj = @new<TestGoroutineProfileIssue74090_T>();
            runtime.SetFinalizer(obj.OrTypedNil(), (any _) => {
            });
            objs = append(objsᴷ, objs, obj);
        }
        objs = default;

        // Queue up all the finalizers.
        runtime.GC();

        // Try to run a goroutine profile concurrently with finalizer execution
        // to trigger the bug.
        ref var w = ref heap(new strings.Builder(), out var Ꮡw);
        goroutineProf.WriteTo(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡw), 1);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string churnIˢ => ref churnIˢᵇ;
private static readonly @string churnIˢᵇ = "churn-i"u8;
internal static ref readonly @string concurrentLaunchesOpˢ => ref concurrentLaunchesOpˢᵇ;
private static readonly @string concurrentLaunchesOpˢᵇ = "concurrent_launches/op"u8;

public static void BenchmarkGoroutine(ж<testing.B> b) {
    Action<ж<testing.B>> withIdle(nint n, Action<ж<testing.B>> fn) => [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.B> bΔ1) => {
            GoFrame ᒐ = default;
            try {
                var c = new channel<nint>(0);
                ref var ready = ref heap(new sync.WaitGroup(), out var Ꮡready);
                ref var done = ref heap(new sync.WaitGroup(), out var Ꮡdone);
                var cʗ1 = c;
                defer(() => {
                    close(cʗ1);
                    Ꮡdone.Wait();
                }, ref ᒐ);

                for (nint i = 0; i < n; i++) {
                    Ꮡready.Add(1);
                    Ꮡdone.Add(1);
                    var cʗ2 = c;
                    goǃ(() => {
                        Ꮡready.Done();
                        ᐸꟷ(cʗ2);
                        Ꮡdone.Done();
                    });
                }
                // Let goroutines block on channel
                Ꮡready.Wait();
                for (nint i = 0; i < 5; i++) {
                    runtime.Gosched();
                }

                fn(bΔ1);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        };

    Action<ж<testing.B>> withChurn(Action<ж<testing.B>> fn) => [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.B> bΔ2) => {
            GoFrame ᒐ = default;
            try {
                ref var ctx = ref heap<context.Context>(context.Background(), out var Ꮡctx);
                (Ꮡctx.ValueSlot, var cancel) = context.WithCancel(Ꮡctx.ValueSlot);
                var cancelʗ1 = cancel;
                defer(() => cancelʗ1(), ref ᒐ);

                ref var ready = ref heap(new sync.WaitGroup(), out var Ꮡready);
                Ꮡready.Add(1);
                ref var count = ref heap(new int64(), out var Ꮡcount);
                ref var churn = ref heap<Action<nint>>(out var Ꮡchurn);
                Ꮡchurn.ValueSlot = [MethodImpl(MethodImplOptions.NoInlining)] (nint i) => {
                    SetGoroutineLabels(WithLabels(Ꮡctx.ValueSlot, Labels(churnIˢ, fmt.Sprint(i))));
                    atomic.AddInt64(Ꮡcount, 1);
                    if (i == 0) {
                        Ꮡready.Done();
                    }
                    if (Ꮡctx.ValueSlot.Err() == nil) {
                        goǃ(Ꮡchurn.ValueSlot, i + 1);
                    }
                };
                goǃ(() => {
                    Ꮡchurn.ValueSlot(0);
                });
                Ꮡready.Wait();

                fn(bΔ2);
                bΔ2.ReportMetric((float64)atomic.LoadInt64(Ꮡcount) / (float64)(~bΔ2).N, concurrentLaunchesOpˢ);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        };

    var benchWriteTo = (ж<testing.B> bΔ3) => {
        var goroutineProf = Lookup(goroutineˢ);
        bΔ3.ResetTimer();
        for (nint i = 0; i < (~bΔ3).N; i++) {
            goroutineProf.WriteTo(io.Discard, 0);
        }
        bΔ3.StopTimer();
    };

    var benchGoroutineProfile = (ж<testing.B> bΔ4) => {
        var p = new slice<runtime.StackRecord>(10000, () => new());
        bΔ4.ResetTimer();
        for (nint i = 0; i < (~bΔ4).N; i++) {
            runtime.GoroutineProfile(p);
        }
        bΔ4.StopTimer();
    };

    // Note that some costs of collecting a goroutine profile depend on the
    // length of the runtime.allgs slice, which never shrinks. Stay within race
    // detector's 8k-goroutine limit
    foreach (var (_, n) in slice<nint>([50, 500, 5000])) {
        b.Run(fmt.Sprintf("Profile.WriteTo idle %d"u8, n), withIdle(n, benchWriteTo));
        b.Run(fmt.Sprintf("Profile.WriteTo churn %d"u8, n), withIdle(n, withChurn(benchWriteTo)));
        b.Run(fmt.Sprintf("runtime.GoroutineProfile churn %d"u8, n), withIdle(n, withChurn(benchGoroutineProfile)));
    }
}

internal static int64 emptyCallStackTestRun;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string lostProfileEventˢ => ref lostProfileEventˢᵇ;
private static readonly @string lostProfileEventˢᵇ = "lostProfileEvent"u8;

// Issue 18836.
public static void TestEmptyCallStack(ж<testing.T> t) {
    @string name = fmt.Sprintf("test18836_%d"u8, emptyCallStackTestRun);
    emptyCallStackTestRun++;

    t.Parallel();
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    var p = NewProfile(name);

    p.Add(fooˢ, 47674);
    p.WriteTo(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡbuf), 1);
    p.Remove(fooˢ);
    @string got = buf.String();
    @string prefix = name + " profile: total 1\n"u8;
    if (!strings_package.HasPrefix(got, prefix)) {
        t.Fatalf("got:\n\t%q\nwant prefix:\n\t%q\n", got, prefix);
    }
    @string lostevent = lostProfileEventˢ;
    if (!strings_package.Contains(got, lostevent)) {
        t.Fatalf("got:\n\t%q\ndoes not contain:\n\t%q\n", got, lostevent);
    }
}

// stackContainsLabeled takes a spec like funcname;key=value and matches if the stack has that key
// and value and has funcname somewhere in the stack.
internal static bool stackContainsLabeled(@string spec, uintptr count, slice<ж<profile.Location>> stk, map<@string, slice<@string>> labels) {
    var (@base, kv, ok) = strings_package.Cut(spec, ";");
    if (!ok) {
        throw panic("no semicolon in key/value spec");
    }
    (var k, var v, ok) = strings_package.Cut(kv, "=");
    if (!ok) {
        throw panic("missing = in key/value spec");
    }
    if (!slices.Contains(labels[k], v)) {
        return false;
    }
    return stackContains(@base, count, stk, labels);
}

public static void TestCPUProfileLabel(ж<testing.T> t) {
    var matches = matchAndAvoidStacks(new Func<@string, uintptr, slice<ж<profile.Location>>, map<@string, slice<@string>>, bool>(stackContainsLabeled), slice<@string>(["runtime/pprof.cpuHogger;key=value"]), avoidFunctions());
    testCPUProfile(t, matches, (time.Duration dur) => {
        Do(context.Background(), Labels(keyˢ, valueˢ), (context.Context _) => {
            cpuHogger(cpuHog1, Ꮡsalt1, dur);
        });
    });
}

public static void TestLabelRace(ж<testing.T> t) {
    testenv.MustHaveParallelism(new pprof_internal_test_package.testing_TжTB(t));
    // Test the race detector annotations for synchronization
    // between setting labels and consuming them from the
    // profile.
    var matches = matchAndAvoidStacks(new Func<@string, uintptr, slice<ж<profile.Location>>, map<@string, slice<@string>>, bool>(stackContainsLabeled), slice<@string>(["runtime/pprof.cpuHogger;key=value"]), default);
    testCPUProfile(t, matches, [MethodImpl(MethodImplOptions.NoInlining)] (time.Duration dur) => {
        var start = time.Now();
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        while (time.Since(start) < dur) {
            ref var salts = ref heap(new array<nint>(10), out var Ꮡsalts);
            for (nint i = 0; i < 10; i++) {
                Ꮡwg.Add(1);
                goǃ((nint j) => {
                    Do(context.Background(), Labels(keyˢ, valueˢ), (context.Context _) => {
                        cpuHogger(cpuHog1, Ꮡsalts.at<nint>(j), time.Millisecond);
                    });
                    Ꮡwg.Done();
                }, i);
            }
            Ꮡwg.Wait();
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string resetˢ => ref resetˢᵇ;
private static readonly @string resetˢᵇ = "reset"u8;
internal static ref readonly @string loopIˢ => ref loopIˢᵇ;
private static readonly @string loopIˢᵇ = "loop-i"u8;
internal static ref readonly @string churnˢ => ref churnˢᵇ;
private static readonly @string churnˢᵇ = "churn"u8;

public static void TestGoroutineProfileLabelRace(ж<testing.T> t) {
    testenv.MustHaveParallelism(new pprof_internal_test_package.testing_TжTB(t));
    // Test the race detector annotations for synchronization
    // between setting labels and consuming them from the
    // goroutine profile. See issue #50292.

    t.Run(resetˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
        GoFrame ᒐ = default;
        try {
            ref var ctx = ref heap<context.Context>(context.Background(), out var Ꮡctx);
            (Ꮡctx.ValueSlot, var cancel) = context.WithCancel(Ꮡctx.ValueSlot);
            var cancelʗ1 = cancel;
            defer(() => cancelʗ1(), ref ᒐ);

            var cancelʗ2 = cancel;
            goǃ(() => {
                var goroutineProf = Lookup(goroutineˢ);
                while (Ꮡctx.ValueSlot.Err() == nil) {
                    ref var w = ref heap(new strings.Builder(), out var Ꮡw);
                    goroutineProf.WriteTo(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡw), 1);
                    @string prof = w.String();
                    if (strings_package.Contains(prof, loopIˢ)) {
                        cancelʗ2();
                    }
                }
            });

            for (nint i = 0; Ꮡctx.ValueSlot.Err() == nil; i++) {
                Do(Ꮡctx.ValueSlot, Labels(loopIˢ, fmt.Sprint(i)), (context.Context ctxΔ1) => {
                });
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });

    t.Run(churnˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ2) => {
        GoFrame ᒐ = default;
        try {
            ref var ctx = ref heap<context.Context>(context.Background(), out var Ꮡctx);
            (Ꮡctx.ValueSlot, var cancel) = context.WithCancel(Ꮡctx.ValueSlot);
            var cancelʗ3 = cancel;
            defer(() => cancelʗ3(), ref ᒐ);

            ref var ready = ref heap(new sync.WaitGroup(), out var Ꮡready);
            Ꮡready.Add(1);
            ref var churn = ref heap<Action<nint>>(out var Ꮡchurn);
            Ꮡchurn.ValueSlot = [MethodImpl(MethodImplOptions.NoInlining)] (nint i) => {
                SetGoroutineLabels(WithLabels(Ꮡctx.ValueSlot, Labels(churnIˢ, fmt.Sprint(i))));
                if (i == 0) {
                    Ꮡready.Done();
                }
                if (Ꮡctx.ValueSlot.Err() == nil) {
                    goǃ(Ꮡchurn.ValueSlot, i + 1);
                }
            };
            goǃ(() => {
                Ꮡchurn.ValueSlot(0);
            });
            Ꮡready.Wait();

            var goroutineProf = Lookup(goroutineˢ);
            for (nint i = 0; i < 10; i++) {
                goroutineProf.WriteTo(io.Discard, 1);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string sampleContainsBothSWhichˢ => ref sampleContainsBothSWhichˢᵇ;
private static readonly @string sampleContainsBothSWhichˢᵇ = "sample contains both %s, which must be labeled, and %s, which must not be labeled"u8;
internal static ref readonly @string sampleMustBeLabeledˢ => ref sampleMustBeLabeledˢᵇ;
private static readonly @string sampleMustBeLabeledˢᵇ = "sample must be labeled because of %s, but is not"u8;
internal static ref readonly @string sampleMustNotBeLabeledˢ => ref sampleMustNotBeLabeledˢᵇ;
private static readonly @string sampleMustNotBeLabeledˢᵇ = "sample must not be labeled because of %s, but is"u8;

// TestLabelSystemstack makes sure CPU profiler samples of goroutines running
// on systemstack include the correct pprof labels. See issue #48577
public static void TestLabelSystemstack(ж<testing.T> t) {
    // Grab and re-set the initial value before continuing to ensure
    // GOGC doesn't actually change following the test.
    nint gogc = debug.SetGCPercent(100);
    debug.SetGCPercent(gogc);

    var matches = matchAndAvoidStacks(new Func<@string, uintptr, slice<ж<profile.Location>>, map<@string, slice<@string>>, bool>(stackContainsLabeled), slice<@string>(["runtime.systemstack;key=value"]), avoidFunctions());
    var p = testCPUProfile(t, matches, (time.Duration dur) => {
        Do(context.Background(), Labels(keyˢ, valueˢ), [MethodImpl(MethodImplOptions.NoInlining)] (context.Context ctx) => {
            parallelLabelHog(ctx, dur, gogc);
        });
    });

    // Two conditions to check:
    // * labelHog should always be labeled.
    // * The label should _only_ appear on labelHog and the Do call above.
    foreach (var (_, s) in p.Sample) {
        var isLabeled = s.Label != nil && slices.Contains(s.Label[keyˢ], valueˢ);
        bool mayBeLabeled = default;
        @string mustBeLabeled = default;
        @string mustNotBeLabeled = default;
        foreach (var (_, loc) in s.Location) {
            foreach (var (_, l) in loc.Line) {
                var exprᴛ1 = l.Function.Name;
                switch (ᐧ) {
                case {} when exprᴛ1 == "runtime/pprof.labelHog"u8 || exprᴛ1 == "runtime/pprof.parallelLabelHog"u8 || exprᴛ1 == "runtime/pprof.parallelLabelHog.func1"u8: {
                    mustBeLabeled = l.Function.Name;
                    break;
                }
                case {} when exprᴛ1 == "runtime/pprof.Do"u8: {
                    // Do sets the labels, so samples may
                    // or may not be labeled depending on
                    // which part of the function they are
                    // at.
                    mayBeLabeled = true;
                    break;
                }
                case {} when exprᴛ1 == "runtime.bgsweep"u8 || exprᴛ1 == "runtime.bgscavenge"u8 || exprᴛ1 == "runtime.forcegchelper"u8 || exprᴛ1 == "runtime.gcBgMarkWorker"u8 || exprᴛ1 == "runtime.runFinalizers"u8 || exprᴛ1 == "runtime.runCleanups"u8 || exprᴛ1 == "runtime.sysmon"u8: {
                    // Runtime system goroutines or threads
                    // (such as those identified by
                    // runtime.isSystemGoroutine). These
                    // should never be labeled.
                    mustNotBeLabeled = l.Function.Name;
                    break;
                }
                case {} when exprᴛ1 == "gogo"u8 || exprᴛ1 == "gosave_systemstack_switch"u8 || exprᴛ1 == "racecall"u8: {
                    // These are context switch/race
                    // critical that we can't do a full
                    // traceback from. Typically this would
                    // be covered by the runtime check
                    // below, but these symbols don't have
                    // the package name.
                    mayBeLabeled = true;
                    break;
                }}

                if (strings_package.HasPrefix(l.Function.Name, runtimeˢ)) {
                    // There are many places in the runtime
                    // where we can't do a full traceback.
                    // Ideally we'd list them all, but
                    // barring that allow anything in the
                    // runtime, unless explicitly excluded
                    // above.
                    mayBeLabeled = true;
                }
            }
        }
        var sʗ1 = s;
        void errorStack(@string f, params ꓸꓸꓸany argsʗp) {
            var args = argsʗp.sslice();
            ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
            fprintStack(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡbuf), sʗ1.Location);
            t.Errorf("%s: %s", fmt.Sprintf(f, args.ꓸꓸꓸ), buf.String());
        }
        if (mustBeLabeled != ""u8 && mustNotBeLabeled != ""u8) {
            errorStack(sampleContainsBothSWhichˢ, mustBeLabeled, mustNotBeLabeled);
            continue;
        }
        if (mustBeLabeled != ""u8 || mustNotBeLabeled != ""u8) {
            // We found a definitive frame, so mayBeLabeled hints are not relevant.
            mayBeLabeled = false;
        }
        if (mayBeLabeled) {
            // This sample may or may not be labeled, so there's nothing we can check.
            continue;
        }
        if (mustBeLabeled != ""u8 && !isLabeled) {
            errorStack(sampleMustBeLabeledˢ, mustBeLabeled);
        }
        if (mustNotBeLabeled != ""u8 && isLabeled) {
            errorStack(sampleMustNotBeLabeledˢ, mustNotBeLabeled);
        }
    }
}

// labelHog is designed to burn CPU time in a way that a high number of CPU
// samples end up running on systemstack.
internal static void labelHog(channel<EmptyStruct> stop, nint gogc) {
    // Regression test for issue 50032. We must give GC an opportunity to
    // be initially triggered by a labelled goroutine.
    runtime.GC();

    for (nint i = 0; ; i++) {
        var selᴛ12 = stop;
        switch (trySelect(ᐸꟷ(selᴛ12, ꓸꓸꓸ))) {
        case 0 when selᴛ12.ꟷᐳ(out _): {
            return;
        }
        default: {
            debug.SetGCPercent(gogc);
            break;
        }}
    }
}

// parallelLabelHog runs GOMAXPROCS goroutines running labelHog.
internal static partial void parallelLabelHog(context.Context ctx, time.Duration dur, nint gogc) {
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    var stop = new channel<EmptyStruct>(0);
    for (nint i = 0; i < runtime.GOMAXPROCS(0); i++) {
        Ꮡwg.Add(1);
        var stopʗ1 = stop;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                labelHog(stopʗ1, gogc);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }

    time.Sleep(dur);
    close(stop);
    Ꮡwg.Wait();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string profatomicˢ => ref profatomicˢᵇ;
private static readonly @string profatomicˢᵇ = "profatomic"u8;

// Check that there is no deadlock when the program receives SIGPROF while in
// 64bit atomics' critical section. Used to happen on mips{,le}. See #20146.
public static partial void TestAtomicLoadStore64(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.CreateTemp(""u8, profatomicˢ);
        if (err != nil) {
            t.Fatalf("TempFile: %v", err);
        }
        defer(os.Remove, f.Name(), ref ᒐ);
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);

        {
            var errΔ1 = StartCPUProfile(new os.FileжWriter(f)); if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
        }
        defer(StopCPUProfile, ref ᒐ);

        ref var flag = ref heap(new uint64(), out var Ꮡflag);
        var done = new channel<bool>(1);

        var doneʗ1 = done;
        goǃ(() => {
            while (atomic.LoadUint64(Ꮡflag) == 0) {
                runtime.Gosched();
            }
            doneʗ1.ᐸꟷ(true);
        });
        time.Sleep(50 * time.Millisecond);
        atomic.StoreUint64(Ꮡflag, 1);
        ᐸꟷ(done);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string proftracebackˢ => ref proftracebackˢᵇ;
private static readonly @string proftracebackˢᵇ = "proftraceback"u8;

public static partial void TestTracebackAll(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // With gccgo, if a profiling signal arrives at the wrong time
        // during traceback, it may crash or hang. See issue #29448.
        var (f, err) = os.CreateTemp(""u8, proftracebackˢ);
        if (err != nil) {
            t.Fatalf("TempFile: %v", err);
        }
        defer(os.Remove, f.Name(), ref ᒐ);
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);

        {
            var errΔ1 = StartCPUProfile(new os.FileжWriter(f)); if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
        }
        defer(StopCPUProfile, ref ᒐ);

        var ch = new channel<nint>(0);
        defer(ᴛ1 => close(ᴛ1), ch, ref ᒐ);

        nint count = 10;
        for (nint i = 0; i < count; i++) {
            var chʗ1 = ch;
            goǃ(() => {
                ᐸꟷ(chʗ1); // block
            });
        }

        nint N = 10000;
        if (testing.Short()) {
            N = 500;
        }
        var buf = new slice<byte>(10 * 1024);
        for (nint i = 0; i < N; i++) {
            runtime.Stack(buf, true);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object canTDetermineWhetherˢ2 = (@string)"Can't determine whether anything was inlined into inlinedCallerDump."u8;
internal static readonly object canTDetermineWhetherˢ3 = (@string)"Can't determine whether anything was inlined into recursionChainBottom."u8;

internal partial struct TestTryAdd_testCases /*dyn*/ {
    internal @string name;
    internal slice<uint64> input;                  // following the input format assumed by profileBuilder.addCPUData.
    internal nint count;                           // number of records in input.
    internal slice<slice<@string>> wantLocs;       // ordered location entries with function names.
    internal slice<ж<profile.Sample>> wantSamples; // ordered samples, we care only about Value and the profile location IDs.
}

// TestTryAdd tests the cases that are hard to test with real program execution.
//
// For example, the current go compilers may not always inline functions
// involved in recursion but that may not be true in the future compilers. This
// tests such cases by using fake call sequences and forcing the profile build
// utilizing translateCPUProfile defined in proto_test.go
public static void TestTryAdd(ж<testing.T> t) {
    {
        var (_, found) = findInlinedCall(inlinedCallerDump, 4 << 10); if (!found) {
            t.Skip(canTDetermineWhetherˢ2);
        }
    }

    // inlinedCallerDump
    //   inlinedCalleeDump
    var pcs = new slice<uintptr>(2);
    inlinedCallerDump(pcs);
    var inlinedCallerStack = new slice<uint64>(2);
    foreach (var i in range(pcs)) {
        inlinedCallerStack[i] = (uint64)pcs[i];
    }
    var wrapperPCs = new slice<uintptr>(1);
    inlinedWrapperCallerDump(wrapperPCs);

    {
        var (_, found) = findInlinedCall(recursionChainBottom, 4 << 10); if (!found) {
            t.Skip(canTDetermineWhetherˢ3);
        }
    }

    // recursionChainTop
    //   recursionChainMiddle
    //     recursionChainBottom
    //       recursionChainTop
    //         recursionChainMiddle
    //           recursionChainBottom
    pcs = new slice<uintptr>(6);
    recursionChainTop(1, pcs);
    var recursionStack = new slice<uint64>(len(pcs));
    foreach (var i in range(pcs)) {
        recursionStack[i] = (uint64)pcs[i];
    }

    var period = (int64)(2000 * 1000); // 1/500*1e9 nanosec.

    var testCases = slice<TestTryAdd_testCases>([new(
        // Sanity test for a normal, complete stack trace.
        name: "full_stack_trace",
        input: slice<uint64>([
            3, 0, 500, // hz = 500. Must match the period.
            5, 0, 50, inlinedCallerStack[0], inlinedCallerStack[1]
        ]),
        count: 2,
        wantLocs: slice<slice<@string>>([
            slice<@string>(["runtime/pprof.inlinedCalleeDump", "runtime/pprof.inlinedCallerDump"])
        ]),
        wantSamples: slice<ж<profile.Sample>>([
            Ꮡ(new profile.Sample(Value: slice<int64>([50, 50 * period]), Location: slice<ж<profile.Location>>([Ꮡ(new profile.Location(ID: 1))])))
        ])
    ), new(
        name: "bug35538",
        input: slice<uint64>([
            3, 0, 500, // hz = 500. Must match the period.
            // Fake frame: tryAdd will have inlinedCallerDump
            // (stack[1]) on the deck when it encounters the next
            // inline function. It should accept this.
            7, 0, 10, inlinedCallerStack[0], inlinedCallerStack[1], inlinedCallerStack[0], inlinedCallerStack[1],
            5, 0, 20, inlinedCallerStack[0], inlinedCallerStack[1]
        ]),
        count: 3,
        wantLocs: slice<slice<@string>>([slice<@string>(["runtime/pprof.inlinedCalleeDump", "runtime/pprof.inlinedCallerDump"])]),
        wantSamples: slice<ж<profile.Sample>>([
            Ꮡ(new profile.Sample(Value: slice<int64>([10, 10 * period]), Location: slice<ж<profile.Location>>([Ꮡ(new profile.Location(ID: 1)), Ꮡ(new profile.Location(ID: 1))]))),
            Ꮡ(new profile.Sample(Value: slice<int64>([20, 20 * period]), Location: slice<ж<profile.Location>>([Ꮡ(new profile.Location(ID: 1))])))
        ])
    ), new(
        name: "bug38096",
        input: slice<uint64>([
            3, 0, 500, // hz = 500. Must match the period.
            // count (data[2]) == 0 && len(stk) == 1 is an overflow
            // entry. The "stk" entry is actually the count.
            4, 0, 0, 4242
        ]),
        count: 2,
        wantLocs: slice<slice<@string>>([slice<@string>(["runtime/pprof.lostProfileEvent"])]),
        wantSamples: slice<ж<profile.Sample>>([
            Ꮡ(new profile.Sample(Value: slice<int64>([4242, 4242 * period]), Location: slice<ж<profile.Location>>([Ꮡ(new profile.Location(ID: 1))])))
        ])
    ), new(
        // If a function is directly called recursively then it must
        // not be inlined in the caller.
        //
        // N.B. We're generating an impossible profile here, with a
        // recursive inlineCalleeDump call. This is simulating a non-Go
        // function that looks like an inlined Go function other than
        // its recursive property. See pcDeck.tryAdd.
        name: "directly_recursive_func_is_not_inlined",
        input: slice<uint64>([
            3, 0, 500, // hz = 500. Must match the period.
            5, 0, 30, inlinedCallerStack[0], inlinedCallerStack[0],
            4, 0, 40, inlinedCallerStack[0]
        ]),
        count: 3,
        // inlinedCallerDump shows up here because
        // runtime_expandFinalInlineFrame adds it to the stack frame.
        wantLocs: slice<slice<@string>>([slice<@string>(["runtime/pprof.inlinedCalleeDump"]), slice<@string>(["runtime/pprof.inlinedCallerDump"])]),
        wantSamples: slice<ж<profile.Sample>>([
            Ꮡ(new profile.Sample(Value: slice<int64>([30, 30 * period]), Location: slice<ж<profile.Location>>([Ꮡ(new profile.Location(ID: 1)), Ꮡ(new profile.Location(ID: 1)), Ꮡ(new profile.Location(ID: 2))]))),
            Ꮡ(new profile.Sample(Value: slice<int64>([40, 40 * period]), Location: slice<ж<profile.Location>>([Ꮡ(new profile.Location(ID: 1)), Ꮡ(new profile.Location(ID: 2))])))
        ])
    ), new(
        name: "recursion_chain_inline",
        input: slice<uint64>([
            3, 0, 500, // hz = 500. Must match the period.
            9, 0, 10, recursionStack[0], recursionStack[1], recursionStack[2], recursionStack[3], recursionStack[4], recursionStack[5]
        ]),
        count: 2,
        wantLocs: slice<slice<@string>>([
            slice<@string>(["runtime/pprof.recursionChainBottom"]),
            slice<@string>([
                "runtime/pprof.recursionChainMiddle",
                "runtime/pprof.recursionChainTop",
                "runtime/pprof.recursionChainBottom"]),
            slice<@string>([
                "runtime/pprof.recursionChainMiddle",
                "runtime/pprof.recursionChainTop",
                "runtime/pprof.TestTryAdd"])
            // inlined into the test.
        ]),
        wantSamples: slice<ж<profile.Sample>>([
            Ꮡ(new profile.Sample(Value: slice<int64>([10, 10 * period]), Location: slice<ж<profile.Location>>([Ꮡ(new profile.Location(ID: 1)), Ꮡ(new profile.Location(ID: 2)), Ꮡ(new profile.Location(ID: 3))])))
        ])
    ), new(
        name: "truncated_stack_trace_later",
        input: slice<uint64>([
            3, 0, 500, // hz = 500. Must match the period.
            5, 0, 50, inlinedCallerStack[0], inlinedCallerStack[1],
            4, 0, 60, inlinedCallerStack[0]
        ]),
        count: 3,
        wantLocs: slice<slice<@string>>([slice<@string>(["runtime/pprof.inlinedCalleeDump", "runtime/pprof.inlinedCallerDump"])]),
        wantSamples: slice<ж<profile.Sample>>([
            Ꮡ(new profile.Sample(Value: slice<int64>([50, 50 * period]), Location: slice<ж<profile.Location>>([Ꮡ(new profile.Location(ID: 1))]))),
            Ꮡ(new profile.Sample(Value: slice<int64>([60, 60 * period]), Location: slice<ж<profile.Location>>([Ꮡ(new profile.Location(ID: 1))])))
        ])
    ), new(
        name: "truncated_stack_trace_first",
        input: slice<uint64>([
            3, 0, 500, // hz = 500. Must match the period.
            4, 0, 70, inlinedCallerStack[0],
            5, 0, 80, inlinedCallerStack[0], inlinedCallerStack[1]
        ]),
        count: 3,
        wantLocs: slice<slice<@string>>([slice<@string>(["runtime/pprof.inlinedCalleeDump", "runtime/pprof.inlinedCallerDump"])]),
        wantSamples: slice<ж<profile.Sample>>([
            Ꮡ(new profile.Sample(Value: slice<int64>([70, 70 * period]), Location: slice<ж<profile.Location>>([Ꮡ(new profile.Location(ID: 1))]))),
            Ꮡ(new profile.Sample(Value: slice<int64>([80, 80 * period]), Location: slice<ж<profile.Location>>([Ꮡ(new profile.Location(ID: 1))])))
        ])
    ), new(
        // We can recover the inlined caller from a truncated stack.
        name: "truncated_stack_trace_only",
        input: slice<uint64>([
            3, 0, 500, // hz = 500. Must match the period.
            4, 0, 70, inlinedCallerStack[0]
        ]),
        count: 2,
        wantLocs: slice<slice<@string>>([slice<@string>(["runtime/pprof.inlinedCalleeDump", "runtime/pprof.inlinedCallerDump"])]),
        wantSamples: slice<ж<profile.Sample>>([
            Ꮡ(new profile.Sample(Value: slice<int64>([70, 70 * period]), Location: slice<ж<profile.Location>>([Ꮡ(new profile.Location(ID: 1))])))
        ])
    ), new(
        // The same location is used for duplicated stacks.
        name: "truncated_stack_trace_twice",
        input: slice<uint64>([
            3, 0, 500, // hz = 500. Must match the period.
            4, 0, 70, inlinedCallerStack[0],
            // Fake frame: add a fake call to
            // inlinedCallerDump to prevent this sample
            // from getting merged into above.
            5, 0, 80, inlinedCallerStack[1], inlinedCallerStack[0]
        ]),
        count: 3,
        wantLocs: slice<slice<@string>>([
            slice<@string>(["runtime/pprof.inlinedCalleeDump", "runtime/pprof.inlinedCallerDump"]),
            slice<@string>(["runtime/pprof.inlinedCallerDump"])
        ]),
        wantSamples: slice<ж<profile.Sample>>([
            Ꮡ(new profile.Sample(Value: slice<int64>([70, 70 * period]), Location: slice<ж<profile.Location>>([Ꮡ(new profile.Location(ID: 1))]))),
            Ꮡ(new profile.Sample(Value: slice<int64>([80, 80 * period]), Location: slice<ж<profile.Location>>([Ꮡ(new profile.Location(ID: 2)), Ꮡ(new profile.Location(ID: 1))])))
        ])
    ), new(
        name: "expand_wrapper_function",
        input: slice<uint64>([
            3, 0, 500, // hz = 500. Must match the period.
            4, 0, 50, (uint64)wrapperPCs[0]
        ]),
        count: 2,
        wantLocs: slice<slice<@string>>([slice<@string>(["runtime/pprof.inlineWrapper.dump"])]),
        wantSamples: slice<ж<profile.Sample>>([
            Ꮡ(new profile.Sample(Value: slice<int64>([50, 50 * period]), Location: slice<ж<profile.Location>>([Ꮡ(new profile.Location(ID: 1))])))
        ])
    )
    ]);

    foreach (var (_, vᴛ1) in testCases) {
        ref var tc = ref heap(new TestTryAdd_testCases(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        t.Run(tc.name, (ж<testing.T> tΔ1) => {
            var (p, err) = translateCPUProfile(tcʗ1.input, tcʗ1.count);
            if (err != nil) {
                tΔ1.Fatalf("translating profile: %v", err);
            }
            tΔ1.Logf("Profile: %v\n", p.OrTypedNil());

            // One location entry with all inlined functions.
            slice<slice<@string>> gotLoc = default;
            foreach (var (_, loc) in p.Location) {
                slice<@string> names = default;
                foreach (var (_, line) in loc.Line) {
                    names = append(names, line.Function.Name);
                }
                gotLoc = append(gotLoc, names);
            }
            {
                @string got = fmtJSON(gotLoc);
                @string want = fmtJSON(tcʗ1.wantLocs); if (got != want) {
                    tΔ1.Errorf("Got Location = %+v\n\twant %+v", got, want);
                }
            }
            // All samples should point to one location.
            slice<ж<profile.Sample>> gotSamples = default;
            foreach (var (_, sample) in p.Sample) {
                slice<ж<profile.Location>> locs = default;
                foreach (var (_, loc) in sample.Location) {
                    locs = append(locs, Ꮡ(new profile.Location(ID: loc.ID)));
                }
                gotSamples = append(gotSamples, Ꮡ(new profile.Sample(Value: (~sample).Value, Location: locs)));
            }
            {
                @string got = fmtJSON(gotSamples);
                @string want = fmtJSON(tcʗ1.wantSamples); if (got != want) {
                    tΔ1.Errorf("Got Samples = %+v\n\twant %+v", got, want);
                }
            }
        });
    }
}

public static void TestTimeVDSO(ж<testing.T> t) {
    // Test that time functions have the right stack trace. In particular,
    // it shouldn't be recursive.

    if (runtime.GOOS == "android"u8) {
        // Flaky on Android, issue 48655. VDSO may not be enabled.
        testenv.SkipFlaky(new pprof_internal_test_package.testing_TжTB(t), 48655);
    }

    var matches = matchAndAvoidStacks(new Func<@string, uintptr, slice<ж<profile.Location>>, map<@string, slice<@string>>, bool>(stackContains), slice<@string>(["time.now"]), avoidFunctions());
    var p = testCPUProfile(t, matches, (time.Duration dur) => {
        var t0 = time.Now();
        for (;;) {
            var tΔ1 = time.Now();
            if (tΔ1.Sub(t0) >= dur) {
                return;
            }
        }
    });

    // Check for recursive time.now sample.
    foreach (var (_, sample) in p.Sample) {
        bool seenNow = default;
        foreach (var (_, loc) in sample.Location) {
            foreach (var (_, line) in loc.Line) {
                if (line.Function.Name == "time.now"u8) {
                    if (seenNow) {
                        t.Fatalf("unexpected recursive time.now");
                    }
                    seenNow = true;
                }
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string goroutineDeepˢ => ref goroutineDeepˢᵇ;
private static readonly @string goroutineDeepˢᵇ = "goroutineDeep"u8;
internal static ref readonly @string runtimePprofˢ3 => ref runtimePprofˢ3ᵇ;
private static readonly @string runtimePprofˢ3ᵇ = "runtime/pprof.produceProfileEvents"u8;
internal static ref readonly @string runtimePprofAllocDeepˢ => ref runtimePprofAllocDeepˢᵇ;
private static readonly @string runtimePprofAllocDeepˢᵇ = "runtime/pprof.allocDeep"u8;

internal partial struct TestProfilerStackDepth_tests /*dyn*/ {
    internal @string profiler;
    internal slice<@string> prefix;
}

public static partial void TestProfilerStackDepth(ж<testing.T> t) {
    t.Cleanup(disableSampling());

    UntypedInt depth = 128;
    goǃ(produceProfileEvents, t, (nint)(depth));
    awaitBlockedGoroutine(t, chanReceiveˢ, goroutineDeepˢ, 1);

    var tests = slice<TestProfilerStackDepth_tests>([
        new("heap", slice<@string>(["runtime/pprof.allocDeep"])),
        new("block", slice<@string>(["runtime.chanrecv1", "runtime/pprof.blockChanDeep"])),
        new("mutex", slice<@string>(["sync.(*Mutex).Unlock", "runtime/pprof.blockMutexDeep"])),
        new("goroutine", slice<@string>(["runtime.gopark", "runtime.chanrecv", "runtime.chanrecv1", "runtime/pprof.goroutineDeep"]))
    ]);

    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new TestProfilerStackDepth_tests(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        t.Run(test.profiler, (ж<testing.T> tΔ1) => {
            var matchedStacksᴷ = new stackAppend<slice<@string>>(moves: true);
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            {
                var errΔ1 = Lookup(testʗ1.profiler).WriteTo(new pprof_internal_test_package.bytes_BufferжWriter(Ꮡbuf), 0); if (errΔ1 != nil) {
                    tΔ1.Fatalf("failed to write heap profile: %v", errΔ1);
                }
            }
            var (p, err) = profile.Parse(new pprof_internal_test_package.bytes_BufferжReader(Ꮡbuf));
            if (err != nil) {
                tΔ1.Fatalf("failed to parse heap profile: %v", err);
            }
            tΔ1.Logf("Profile = %v", p.OrTypedNil());

            var stks = profileStacks(p);
            slice<slice<@string>> matchedStacks = default;
            foreach (var (_, stk) in stks) {
                if (!hasPrefix(stk, testʗ1.prefix)) {
                    continue;
                }
                // We may get multiple stacks which contain the prefix we want, but
                // which might not have enough frames, e.g. if the profiler hides
                // some leaf frames that would count against the stack depth limit.
                // Check for at least one match
                matchedStacks = append(matchedStacksᴷ, matchedStacks, stk);
                if (len(stk) != depth) {
                    continue;
                }
                {
                    @string rootFn = stk[depth - 1];
                    @string wantFn = runtimePprofˢ3; if (rootFn != wantFn) {
                        continue;
                    }
                }
                // Found what we wanted
                return;
            }
            matchedStacks = move2heap(matchedStacksᴷ, matchedStacks);
            foreach (var (_, stk) in matchedStacks) {
                tΔ1.Logf("matched stack=%s", stk);
                if (len(stk) != depth) {
                    tΔ1.Errorf("want stack depth = %d, got %d", (nint)(depth), len(stk));
                    continue;
                }

                {
                    @string rootFn = stk[depth - 1];
                    @string wantFn = runtimePprofAllocDeepˢ; if (rootFn != wantFn) {
                        tΔ1.Errorf("want stack stack root %s, got %v", wantFn, rootFn);
                    }
                }
            }
        });
    }
}

internal static bool hasPrefix(slice<@string> stk, slice<@string> prefix) {
    return len(prefix) <= len(stk) && slices.Equal<slice<@string>, @string>(stk.slice(0, len(prefix)), prefix);
}

// ensure that stack records are valid map keys (comparable)
internal static map<runtime.MemProfileRecord, EmptyStruct> _ᴛ1ʗ = new map<runtime.MemProfileRecord, EmptyStruct>{};

internal static map<runtime.StackRecord, EmptyStruct> _ᴛ2ʗ = new map<runtime.StackRecord, EmptyStruct>{};

// allocDeep calls itself n times before calling fn.
internal static void allocDeep(nint n) {
    if (n > 1) {
        allocDeep(n - 1);
        return;
    }
    memSink = new slice<byte>((1 << 20));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string blockChanDeepˢ => ref blockChanDeepˢᵇ;
private static readonly @string blockChanDeepˢᵇ = "blockChanDeep"u8;

// blockChanDeep produces a block profile event at stack depth n, including the
// caller.
internal static partial void blockChanDeep(ж<testing.T> t, nint n) {
    if (n > 1) {
        blockChanDeep(t, n - 1);
        return;
    }
    var ch = new channel<EmptyStruct>(0);
    var chʗ1 = ch;
    goǃ(() => {
        awaitBlockedGoroutine(t, chanReceiveˢ, blockChanDeepˢ, 1);
        chʗ1.ᐸꟷ(new EmptyStruct());
    });
    ᐸꟷ(ch);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string blockMutexDeepˢ => ref blockMutexDeepˢᵇ;
private static readonly @string blockMutexDeepˢᵇ = "blockMutexDeep"u8;

// blockMutexDeep produces a block profile event at stack depth n, including the
// caller.
internal static partial void blockMutexDeep(ж<testing.T> t, nint n) {
    if (n > 1) {
        blockMutexDeep(t, n - 1);
        return;
    }
    ref var mu = ref heap(new sync.Mutex(), out var Ꮡmu);
    goǃ(() => {
        Ꮡmu.Lock();
        Ꮡmu.Lock();
    });
    awaitBlockedGoroutine(t, syncMutexLockˢ, blockMutexDeepˢ, 1);
    Ꮡmu.Unlock();
}

// goroutineDeep blocks at stack depth n, including the caller until the test is
// finished.
internal static void goroutineDeep(ж<testing.T> t, nint n) {
    if (n > 1) {
        goroutineDeep(t, n - 1);
        return;
    }
    var wait = new channel<EmptyStruct>(1);
    var waitʗ1 = wait;
    t.Cleanup(() => {
        waitʗ1.ᐸꟷ(new EmptyStruct());
    });
    ᐸꟷ(wait);
}

// produceProfileEvents produces pprof events at the given stack depth and then
// blocks in goroutineDeep until the test completes. The stack traces are
// guaranteed to have exactly the desired depth with produceProfileEvents as
// their root frame which is expected by TestProfilerStackDepth.
internal static void produceProfileEvents(ж<testing.T> t, nint depth) {
    allocDeep(depth - 1);         // -1 for produceProfileEvents, **
    blockChanDeep(t, depth - 2);  // -2 for produceProfileEvents, **, chanrecv1
    blockMutexDeep(t, depth - 2); // -2 for produceProfileEvents, **, Unlock
    memSink = default;
    runtime.GC();
    goroutineDeep(t, depth - 4); // -4 for produceProfileEvents, **, chanrecv1, chanrev, gopark
}

internal static partial slice<@string> getProfileStacks(Func<slice<runtime.BlockProfileRecord>, (nint, bool)> collect, bool fileLine, bool pcs) {
    var stacksᴷ = new stackAppend<@string>(moves: true);

    nint n = default;
    bool ok = default;
    slice<runtime.BlockProfileRecord> p = default;
    for (;;) {
        p = new slice<runtime.BlockProfileRecord>(n, () => new(nil));
        (n, ok) = collect(p);
        if (ok) {
            p = p.slice(0, n);
            break;
        }
    }
    slice<@string> stacks = default;
    foreach (var (_, vᴛ1) in p) {
        var r = vᴛ1;

        ref var stack = ref heap(new strings.Builder(), out var Ꮡstack);
        foreach (var (i, pc) in r.StackRecord.Stack()) {
            if (i > 0) {
                Ꮡstack.WriteByte((byte)'\n');
            }
            if (pcs) {
                fmt.Fprintf(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡstack), "%x "u8, pc);
            }
            // Use FuncForPC instead of CallersFrames,
            // because we want to see the info for exactly
            // the PCs returned by the mutex profile to
            // ensure inlined calls have already been properly
            // expanded.
            var f = runtime.FuncForPC(pc - 1);
            Ꮡstack.WriteString(f.Name());
            if (fileLine) {
                Ꮡstack.WriteByte((byte)' ');
                var (@file, line) = f.FileLine(pc - 1);
                Ꮡstack.WriteString(@file);
                Ꮡstack.WriteByte((byte)':');
                Ꮡstack.WriteString(strconv.Itoa(line));
            }
        }
        stacks = append(stacksᴷ, stacks, stack.String());
    }
    stacks = move2heap(stacksᴷ, stacks);
    return stacks;
}

public static partial void TestMutexBlockFullAggregation(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // This regression test is adapted from
        // https://github.com/grafana/pyroscope-go/issues/103,
        // authored by Tolya Korniltsev

        ref var m = ref heap(new sync.Mutex(), out var Ꮡm);

        nint prev = runtime.SetMutexProfileFraction(-1);
        defer(runtime.SetMutexProfileFraction, prev, ref ᒐ);

        const nint fraction = 1;
        const nint iters = 100;
        const nint workers = 2;

        runtime.SetMutexProfileFraction(fraction);
        runtime.SetBlockProfileRate(1);
        defer(runtime.SetBlockProfileRate, (nint)0, ref ᒐ);

        ref var wg = ref heap<sync.WaitGroup>(new sync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(workers);
        foreach (var _ᴛ1 in range(workers)) {
            goǃ(() => {
                foreach (var _ᴛ2 in range(iters)) {
                    Ꮡm.Lock();
                    // Wait at least 1 millisecond to pass the
                    // starvation threshold for the mutex
                    time.Sleep(time.Millisecond);
                    Ꮡm.Unlock();
                }
                Ꮡwg.Done();
            });
        }
        Ꮡwg.Wait();

        void assertNoDuplicates(@string name, Func<slice<runtime.BlockProfileRecord>, (nint, bool)> collect) {
            var stacks = getProfileStacks(collect, true, true);
            var seen = new map<@string, EmptyStruct>();
            foreach (var (_, s) in stacks) {
                {
                    var (_, ok) = seen[s, ꟷ]; if (ok) {
                        t.Errorf("saw duplicate entry in %s profile with stack:\n%s", name, s);
                    }
                }
                seen[s] = new EmptyStruct();
            }
            if (len(seen) == 0) {
                t.Errorf("did not see any samples in %s profile for this test", name);
            }
        }
        var assertNoDuplicatesʗ1 = assertNoDuplicates;
        t.Run(mutexˢ, (ж<testing.T> tΔ1) => {
            assertNoDuplicatesʗ1(mutexˢ, runtime.MutexProfile);
        });
        var assertNoDuplicatesʗ2 = assertNoDuplicates;
        t.Run(blockˢ, (ж<testing.T> tΔ2) => {
            assertNoDuplicatesʗ2(blockˢ, runtime.BlockProfile);
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void inlineA(ж<sync.Mutex> mu, ж<sync.WaitGroup> wg) {
    inlineB(mu, wg);
}

internal static void inlineB(ж<sync.Mutex> mu, ж<sync.WaitGroup> wg) {
    inlineC(mu, wg);
}

internal static void inlineC(ж<sync.Mutex> mu, ж<sync.WaitGroup> wg) {
    GoFrame ᒐ = default;
    try {
        defer(wg.Done, ref ᒐ);
        mu.Lock();
        mu.Unlock();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void inlineD(ж<sync.Mutex> mu, ж<sync.WaitGroup> wg) {
    inlineE(mu, wg);
}

internal static void inlineE(ж<sync.Mutex> mu, ж<sync.WaitGroup> wg) {
    inlineF(mu, wg);
}

internal static void inlineF(ж<sync.Mutex> mu, ж<sync.WaitGroup> wg) {
    GoFrame ᒐ = default;
    try {
        defer(wg.Done, ref ᒐ);
        mu.Unlock();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string inlineCˢ => ref inlineCˢᵇ;
private static readonly @string inlineCˢᵇ = "inlineC"u8;
internal static readonly object didNotSeeExpectedStackˢ = (@string)"did not see expected stack"u8;

internal partial struct TestBlockMutexProfileInlineExpansion_tcs /*dyn*/ {
    public @string Name;
    public Func<slice<runtime.BlockProfileRecord>, (nint, bool)> Collect;
    public @string SubStack;
}

public static partial void TestBlockMutexProfileInlineExpansion(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        runtime.SetBlockProfileRate(1);
        defer(runtime.SetBlockProfileRate, (nint)0, ref ᒐ);
        nint prev = runtime.SetMutexProfileFraction(1);
        defer(runtime.SetMutexProfileFraction, prev, ref ᒐ);

        ref var mu = ref heap(new sync.Mutex(), out var Ꮡmu);
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(2);
        Ꮡmu.Lock();
        goǃ(inlineA, Ꮡmu, Ꮡwg);
        awaitBlockedGoroutine(t, syncMutexLockˢ, inlineCˢ, 1);
        // inlineD will unblock inlineA
        goǃ(inlineD, Ꮡmu, Ꮡwg);
        Ꮡwg.Wait();

        var tcs = slice<TestBlockMutexProfileInlineExpansion_tcs>([
            new(
                Name: "mutex",
                Collect: runtime.MutexProfile,
                SubStack: """
sync.(*Mutex).Unlock
runtime/pprof.inlineF
runtime/pprof.inlineE
runtime/pprof.inlineD
"""u8.Raw()
            ),
            new(
                Name: "block",
                Collect: runtime.BlockProfile,
                SubStack: """
sync.(*Mutex).Lock
runtime/pprof.inlineC
runtime/pprof.inlineB
runtime/pprof.inlineA
"""u8.Raw()
            )
        ]);

        foreach (var (_, vᴛ1) in tcs) {
            ref var tc = ref heap(new TestBlockMutexProfileInlineExpansion_tcs(), out var Ꮡtc);
            tc = vᴛ1;

            var tcʗ1 = tc;
            t.Run(tc.Name, (ж<testing.T> tΔ1) => {
                var stacks = getProfileStacks(tcʗ1.Collect, false, false);
                foreach (var (_, s) in stacks) {
                    if (strings_package.Contains(s, tcʗ1.SubStack)) {
                        return;
                    }
                }
                tΔ1.Error(didNotSeeExpectedStackˢ);
                tΔ1.Logf("wanted:\n%s", tcʗ1.SubStack);
                tΔ1.Logf("got: %s", stacks);
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string mutexProfileˢ => ref mutexProfileˢᵇ;
private static readonly @string mutexProfileˢᵇ = "MutexProfile"u8;
internal static ref readonly @string goroutineProfileˢ => ref goroutineProfileˢᵇ;
private static readonly @string goroutineProfileˢᵇ = "GoroutineProfile"u8;
internal static ref readonly @string blockProfileˢ => ref blockProfileˢᵇ;
private static readonly @string blockProfileˢᵇ = "BlockProfile"u8;
internal static ref readonly @string memProfileInUseZeroTrueˢ => ref memProfileInUseZeroTrueˢᵇ;
private static readonly @string memProfileInUseZeroTrueˢᵇ = "MemProfile/inUseZero=true"u8;
internal static ref readonly @string memProfileInUseZeroFalseˢ => ref memProfileInUseZeroFalseˢᵇ;
private static readonly @string memProfileInUseZeroFalseˢᵇ = "MemProfile/inUseZero=false"u8;

public static void TestProfileRecordNullPadding(ж<testing.T> t) {
    // Produce events for the different profile types.
    t.Cleanup(disableSampling());
    memSink = new slice<byte>(1);     // MemProfile
    ᐸꟷ(time.After(time.Millisecond)); // BlockProfile
    blockMutex(t);                    // MutexProfile
    runtime.GC();

    // Test that all profile records are null padded.
    testProfileRecordNullPadding<runtime.BlockProfileRecord>(t, mutexProfileˢ, runtime.MutexProfile);
    testProfileRecordNullPadding<runtime.StackRecord>(t, goroutineProfileˢ, runtime.GoroutineProfile);
    testProfileRecordNullPadding<runtime.BlockProfileRecord>(t, blockProfileˢ, runtime.BlockProfile);
    testProfileRecordNullPadding(t, memProfileInUseZeroTrueˢ, (slice<runtime.MemProfileRecord> p) => runtime.MemProfile(p, true));
    testProfileRecordNullPadding(t, memProfileInUseZeroFalseˢ, (slice<runtime.MemProfileRecord> p) => runtime.MemProfile(p, false));
    // Not testing ThreadCreateProfile because it is broken, see issue 6104.
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object noRecordsFoundˢ = (@string)"no records found"u8;

internal static void testProfileRecordNullPadding<T>(ж<testing.T> t, @string name, Func<slice<T>, (nint, bool)> fn)
    where T : /* runtime.StackRecord | runtime.MemProfileRecord | runtime.BlockProfileRecord */ new()
{
    ж<array<uintptr>> stack0(ж<T> sr) {
        var switchᴛ1 = ((any)sr.OrTypedNil());
        switch (switchᴛ1.type()) {
        case ж<runtime.StackRecord> tΔ1: {
            return tΔ1.ᏑStack0;
        }
        case ж<runtime.MemProfileRecord> tΔ1: {
            return tΔ1.ᏑStack0;
        }
        case ж<runtime.BlockProfileRecord> tΔ1: {
            return tΔ1.of(runtime.BlockProfileRecord.ᏑStack0);
        }
        default: {
            var tΔ1 = switchᴛ1;
            throw panic(fmt.Sprintf("unexpected type %T"u8, sr.OrTypedNil()));
            break;
        }}
    }

    var stack0ʗ1 = stack0;
    t.Run(name, (ж<testing.T> tΔ2) => {
        slice<T> p = default;
        for (;;) {
            var (n, ok) = fn(p);
            if (ok) {
                p = p.slice(0, n);
                break;
            }
            p = new slice<T>(n * 2);
            foreach (var i in range(p)) {
                var s0 = stack0ʗ1(Ꮡ(p, i));
                foreach (var j in range(s0, 32)) {
                    // Poison the Stack0 array to identify lack of zero padding
                    s0.Value[j] = ~(uintptr)0;
                }
            }
        }

        if (len(p) == 0) {
            tΔ2.Fatal(noRecordsFoundˢ);
        }

        foreach (var (_, vᴛ1) in p) {
            ref var sr = ref heap<T>(out var Ꮡsr);
            sr = vᴛ1;

            foreach (var (i, v) in stack0ʗ1(Ꮡsr).Value) {
                if (v == ~(uintptr)0) {
                    tΔ2.Fatalf("record p[%d].Stack0 is not null padded: %+v", i, sr);
                }
            }
        }
    });
}

// disableSampling configures the profilers to capture all events, otherwise
// it's difficult to assert anything.
internal static Action disableSampling() {
    nint oldMemRate = runtime.MemProfileRate;
    runtime.MemProfileRate = 1;
    runtime.SetBlockProfileRate(1);
    nint oldMutexRate = runtime.SetMutexProfileFraction(1);
    return () => {
        runtime.MemProfileRate = oldMemRate;
        runtime.SetBlockProfileRate(0);
        runtime.SetMutexProfileFraction(oldMutexRate);
    };
}

} // end pprof_internal_test_package
