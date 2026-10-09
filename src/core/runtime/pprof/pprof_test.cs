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

internal static ж<nint> Ꮡsalt1 = new StandardBox<nint>(0);
internal static ref nint salt1 => ref Ꮡsalt1.Value;
internal static ж<nint> Ꮡsalt2 = new StandardBox<nint>(0);
internal static ref nint salt2 => ref Ꮡsalt2.Value;

// The actual CPU hogging function.
// Must not call other functions nor access heap/globals in the loop,
// otherwise under race detector the samples will be in the race runtime.
internal static nint cpuHog1(nint x) {
    return cpuHog0(x, 100000);
}

internal static nint cpuHog0(nint x, nint n) {
    nint foo = x;
    for (nint i = 0; i < n; i++) {
        if (foo > 0){
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
        if (foo > 0){
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
        return new @string[]{"runtime.sigprof"u8}.slice();
    }
    return default!;
}

public static void TestCPUProfile(ж<testing.T> Ꮡt) {
    var matches = matchAndAvoidStacks(new Func<@string, uintptr, slice<ж<profile.Location>>, map<@string, slice<@string>>, bool>(stackContains), new @string[]{"runtime/pprof.cpuHog1"u8}.slice(), avoidFunctions());
    testCPUProfile(Ꮡt, matches, (time.Duration dur) => {
        cpuHogger(cpuHog1, Ꮡsalt1, dur);
    });
}

public static void TestCPUProfileMultithreaded(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(runtime.GOMAXPROCS, runtime.GOMAXPROCS(2), ref ᒐ);
        var matches = matchAndAvoidStacks(new Func<@string, uintptr, slice<ж<profile.Location>>, map<@string, slice<@string>>, bool>(stackContains), new @string[]{"runtime/pprof.cpuHog1"u8, "runtime/pprof.cpuHog2"u8}.slice(), avoidFunctions());
        testCPUProfile(Ꮡt, matches, [MethodImpl(MethodImplOptions.NoInlining)] (time.Duration dur) => {
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

public static void TestCPUProfileMultithreadMagnitude(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (runtime.GOOS != "linux"u8) {
            Ꮡt.Skip(issue35057IsOnlyˢ);
        }
        // Linux [5.9,5.16) has a kernel bug that can break CPU timers on newly
        // created threads, breaking our CPU accounting.
        var (major, minor) = unix.KernelVersion();
        Ꮡt.Logf("Running on Linux %d.%d"u8, major, minor);
        defer(() => {
            if (Ꮡt.Failed()) {
                Ꮡt.Logf("Failure of this test may indicate that your system suffers from a known Linux kernel bug fixed on newer kernels. See https://golang.org/issue/49065."u8);
            }
        }, ref ᒐ);
        // Disable on affected builders to avoid flakiness, but otherwise keep
        // it enabled to potentially warn users that they are on a broken
        // kernel.
        if (testenv.Builder() != ""u8 && (runtime.GOARCH == "386"u8 || runtime.GOARCH == "amd64"u8)) {
            var have59 = major > 5 || (major == 5 && minor >= 9);
            var have516 = major > 5 || (major == 5 && minor >= 16);
            if (have59 && !have516) {
                testenv.SkipFlaky(new pprof_internal_test_package.testing_TжTB(Ꮡt), 49065);
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
            var diff = (float64)(int64)(a - b) / (float64)(int64)a;
            if (diff > maxDiffΔ1) {
                return fmt.Errorf("CPU usage reports are too different (limit -%.1f%%, got -%.1f%%)"u8, maxDiffΔ1 * 100D, diff * 100D);
            }
            return default!;
        }
        foreach (var (_, vᴛ1) in new TestCPUProfileMultithreadMagnitude_type[]{
            new(
                name: "serial"u8,
                workers: 1
            ),
            new(
                name: "parallel"u8,
                workers: runtime.GOMAXPROCS(0)
            )
        }.slice()) {
            ref var tc = ref heap(new TestCPUProfileMultithreadMagnitude_type(), out var Ꮡtc);
            tc = vᴛ1;

            // check that the OS's perspective matches what the Go runtime measures.
            var compareʗ1 = compare;
            var tcʗ1 = tc;
            Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
                tΔ1.Logf("Running with %d workers"u8, tcʗ1.workers);
                time.Duration userTime = default!;
                time.Duration systemTime = default!;
                var matches = matchAndAvoidStacks(new Func<@string, uintptr, slice<ж<profile.Location>>, map<@string, slice<@string>>, bool>(stackContains), new @string[]{"runtime/pprof.cpuHog1"u8}.slice(), avoidFunctions());
                var compareʗ2 = compareʗ1;
                var matchesʗ1 = matches;
                var acceptProfile = (ж<testing.T> tΔ2, ж<profile.Profile> p) => {
                    if (!matchesʗ1(tΔ2, p)) {
                        return false;
                    }
                    var ok = true;
                    foreach (var (i, unit) in new @string[]{"count"u8, "nanoseconds"u8}.slice()) {
                        {
                            @string have = (~p).SampleType[i].Value.Unit;
                            @string want = unit; if (have != want) {
                                tΔ2.Logf("pN SampleType[%d]; %q != %q"u8, i, have, want);
                                ok = false;
                            }
                        }
                    }
                    // cpuHog1 called below is the primary source of CPU
                    // load, but there may be some background work by the
                    // runtime. Since the OS rusage measurement will
                    // include all work done by the process, also compare
                    // against all samples in our profile.
                    time.Duration value = default!;
                    foreach (var (_, sample) in (~p).Sample) {
                        value += ((time.Duration)(~sample).Value[1]) * time.ΔNanosecond;
                    }
                    var totalTime = userTime + systemTime;
                    tΔ2.Logf("compare %s user + %s system = %s vs %s"u8, userTime, systemTime, totalTime, value);
                    {
                        var err = compareʗ2(totalTime, value, maxDiff); if (err != default!) {
                            tΔ2.Logf("compare got %v want nil"u8, err);
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

public static void TestCPUProfileInlining(ж<testing.T> Ꮡt) {
    if (!containsInlinedCall(inlinedCaller, (4 << (int)(10)))) {
        Ꮡt.Skip(canTDetermineWhetherˢ);
    }
    var matches = matchAndAvoidStacks(new Func<@string, uintptr, slice<ж<profile.Location>>, map<@string, slice<@string>>, bool>(stackContains), new @string[]{"runtime/pprof.inlinedCallee"u8, "runtime/pprof.inlinedCaller"u8}.slice(), avoidFunctions());
    var p = testCPUProfile(Ꮡt, matches, (time.Duration dur) => {
        cpuHogger(inlinedCaller, Ꮡsalt1, dur);
    });
    // Check if inlined function locations are encoded correctly. The inlinedCalee and inlinedCaller should be in one location.
    foreach (var (_, loc) in (~p).Location) {
        var (hasInlinedCallerAfterInlinedCallee, hasInlinedCallee) = (false, false);
        foreach (var (_, line) in (~loc).Line) {
            if ((~line.Function).Name == "runtime/pprof.inlinedCallee"u8) {
                hasInlinedCallee = true;
            }
            if (hasInlinedCallee && (~line.Function).Name == "runtime/pprof.inlinedCaller"u8) {
                hasInlinedCallerAfterInlinedCallee = true;
            }
        }
        if (hasInlinedCallee != hasInlinedCallerAfterInlinedCallee) {
            Ꮡt.Fatalf("want inlinedCallee followed by inlinedCaller, got separate Location entries:\n%v"u8, p.OrTypedNil());
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
    if (pcs == default!) {
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
    inlineWrapperInterface h = default!;
    h = new pprof_internal_test_package.inlineWrapperжinlineWrapperInterface(Ꮡ(new inlineWrapper(nil)));
    h.dump(pcs);
}

public static void TestCPUProfileRecursion(ж<testing.T> Ꮡt) {
    var matches = matchAndAvoidStacks(new Func<@string, uintptr, slice<ж<profile.Location>>, map<@string, slice<@string>>, bool>(stackContains), new @string[]{"runtime/pprof.inlinedCallee"u8, "runtime/pprof.recursionCallee"u8, "runtime/pprof.recursionCaller"u8}.slice(), avoidFunctions());
    var p = testCPUProfile(Ꮡt, matches, (time.Duration dur) => {
        cpuHogger(recursionCaller, Ꮡsalt1, dur);
    });
    // check the Location encoding was not confused by recursive calls.
    foreach (var (i, loc) in (~p).Location) {
        nint recursionFunc = 0;
        foreach (var (_, line) in (~loc).Line) {
            {
                @string name = line.Function.Value.Name; if (name == "runtime/pprof.recursionCaller"u8 || name == "runtime/pprof.recursionCallee"u8) {
                    recursionFunc++;
                }
            }
        }
        if (recursionFunc > 1) {
            Ꮡt.Fatalf("want at most one recursionCaller or recursionCallee in one Location, got a violating Location (index: %d):\n%v"u8, i, p.OrTypedNil());
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

internal static ж<profile.Profile> parseProfile(ж<testing.T> Ꮡt, slice<byte> valBytes, Action<uintptr, slice<ж<profile.Location>>, map<@string, slice<@string>>> f) {
    var (p, err) = profile.Parse(new pprof_internal_test_package.bytes_ReaderжReader(bytes.NewReader(valBytes)));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    foreach (var (_, sample) in (~p).Sample) {
        var count = (uintptr)(~sample).Value[0];
        f(count, (~sample).Location, (~sample).Label);
    }
    return p;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unameˢ = "uname"u8;
internal static readonly object skippingOnPlan9ˢ = (@string)"skipping on plan9"u8;
internal static readonly object skippingOnWasip1ˢ = (@string)"skipping on wasip1"u8;
internal static readonly @string inQemuˢ = "IN_QEMU"u8;
internal static readonly object ignoreTheFailureInQemuˢ = (@string)"ignore the failure in QEMU; see golang.org/issue/9605"u8;

// testCPUProfile runs f under the CPU profiler, checking for some conditions specified by need,
// as interpreted by matches, and returns the parsed profile.
internal static ж<profile.Profile> testCPUProfile(ж<testing.T> Ꮡt, Func<ж<testing.T>, ж<profile.Profile>, bool> matches, Action<time.Duration> f) {
    ref var t = ref Ꮡt.DerefOrNull();

    var exprᴛ1 = runtime.GOOS;
    if (exprᴛ1 == "darwin"u8) {
        var (@out, err) = testenv.Command(new pprof_internal_test_package.testing_TжTB(Ꮡt), unameˢ, "-a"u8).CombinedOutput();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        @string vers = ((@string)@out);
        Ꮡt.Logf("uname -a: %v"u8, vers);
    }
    else if (exprᴛ1 == "plan9"u8) {
        Ꮡt.Skip(skippingOnPlan9ˢ);
    }
    else if (exprᴛ1 == "wasip1"u8) {
        Ꮡt.Skip(skippingOnWasip1ˢ);
    }

    var broken = testenv.CPUProfilingBroken();
    var (deadline, ok) = t.Deadline();
    if (broken || !ok) {
        if (broken && testing.Short()){
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
    while (ᐧ) {
        ref var prof = ref heap(new bytes.Buffer(), out var Ꮡprof);
        {
            var err = StartCPUProfile(new pprof_internal_test_package.bytes_BufferжWriter(Ꮡprof)); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
        f(duration);
        StopCPUProfile();
        {
            var (p, okΔ1) = profileOk(Ꮡt, matches, prof, duration); if (okΔ1) {
                return p;
            }
        }
        duration *= 2;
        if (time.Until(deadline) < duration) {
            break;
        }
        Ꮡt.Logf("retrying with %s duration"u8, duration);
    }
    if (broken) {
        Ꮡt.Skipf("ignoring failure on %s/%s; see golang.org/issue/13841"u8, runtime.GOOS, runtime.GOARCH);
    }
    // Ignore the failure if the tests are running in a QEMU-based emulator,
    // QEMU is not perfect at emulating everything.
    // IN_QEMU environmental variable is set by some of the Go builders.
    // IN_QEMU=1 indicates that the tests are running in QEMU. See issue 9605.
    if (os.Getenv(inQemuˢ) == "1"u8) {
        Ꮡt.Skip(ignoreTheFailureInQemuˢ);
    }
    Ꮡt.FailNow();
    return default!;
}

internal static Func<Action, (time.Duration, time.Duration)> diffCPUTimeImpl;

internal static (time.Duration user, time.Duration system) diffCPUTime(ж<testing.T> Ꮡt, Action f) {
    {
        var fn = diffCPUTimeImpl; if (fn != default!) {
            return fn(f);
        }
    }
    Ꮡt.Fatalf("cannot measure CPU time on GOOS=%s GOARCH=%s"u8, runtime.GOOS, runtime.GOARCH);
    return (0, 0);
}

// stackContains matches if a function named spec appears anywhere in the stack trace.
internal static bool stackContains(@string spec, uintptr count, slice<ж<profile.Location>> stk, map<@string, slice<@string>> labels) {
    foreach (var (_, loc) in stk) {
        foreach (var (_, line) in (~loc).Line) {
            if (strings_package.Contains((~line.Function).Name, spec)) {
                return true;
            }
        }
    }
    return false;
}

// type sampleMatchFunc is a methodless func type — rendered inline as its base delegate

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object tooFewSamplesOnWindowsˢ = (@string)"too few samples on Windows (golang.org/issue/10842)"u8;

internal static (ж<profile.Profile>, bool ok) profileOk(ж<testing.T> Ꮡt, Func<ж<testing.T>, ж<profile.Profile>, bool> matches, bytes.Buffer prof, time.Duration duration) {
    bool ok = default!;

    ok = true;
    uintptr samples = default!;
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    var p = parseProfile(Ꮡt, prof.Bytes(), (uintptr count, slice<ж<profile.Location>> stk, map<@string, slice<@string>> labels) => {
        fmt.Fprintf(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡbuf), "%d:"u8, count);
        fprintStack(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡbuf), stk);
        fmt.Fprintf(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡbuf), " labels: %v\n"u8, labels);
        samples += count;
        fmt.Fprintf(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡbuf), "\n"u8);
    });
    Ꮡt.Logf("total %d CPU profile samples collected:\n%s"u8, samples, buf.String());
    if (samples < 10 && runtime.GOOS == "windows"u8) {
        // On some windows machines we end up with
        // not enough samples due to coarse timer
        // resolution. Let it go.
        Ꮡt.Log(tooFewSamplesOnWindowsˢ);
        return (p, false);
    }
    // Check that we got a reasonable number of samples.
    // We used to always require at least ideal/4 samples,
    // but that is too hard to guarantee on a loaded system.
    // Now we accept 10 or more samples, which we take to be
    // enough to show that at least some profiling is occurring.
    {
        var ideal = (uintptr)(int64)(duration * 100 / time.ΔSecond); if (samples == 0 || (samples < ideal / 4 && samples < 10)) {
            Ꮡt.Logf("too few samples; got %d, want at least %d, ideally %d"u8, samples, ideal / 4, ideal);
            ok = false;
        }
    }
    if (matches != default! && !matches(Ꮡt, p)) {
        ok = false;
    }
    return (p, ok);
}

// type profileMatchFunc is a methodless func type — rendered inline as its base delegate

internal static Func<ж<testing.T>, ж<profile.Profile>, bool> matchAndAvoidStacks(Func<@string, uintptr, slice<ж<profile.Location>>, map<@string, slice<@string>>, bool> matches, slice<@string> need, slice<@string> avoid) {
    var avoidʗ1 = avoid;
    var needʗ1 = need;
    return (ж<testing.T> t, ж<profile.Profile> p) => {
        bool ok = default!;
        ok = true;
        // Check that profile is well formed, contains 'need', and does not contain
        // anything from 'avoid'.
        var have = new slice<uintptr>(len(needʗ1));
        var avoidSamples = new slice<uintptr>(len(avoidʗ1));
        foreach (var (_, sample) in (~p).Sample) {
            var count = (uintptr)(~sample).Value[0];
            foreach (var (i, spec) in needʗ1) {
                if (matches(spec, count, (~sample).Location, (~sample).Label)) {
                    have[i] += count;
                }
            }
            foreach (var (i, name) in avoidʗ1) {
                foreach (var (_, loc) in (~sample).Location) {
                    foreach (var (_, line) in (~loc).Line) {
                        if (strings_package.Contains((~line.Function).Name, name)) {
                            avoidSamples[i] += count;
                        }
                    }
                }
            }
        }
        foreach (var (i, name) in avoidʗ1) {
            var bad = avoidSamples[i];
            if (bad != 0) {
                t.Logf("found %d samples in avoid-function %s\n"u8, bad, name);
                ok = false;
            }
        }
        if (len(needʗ1) == 0) {
            return ok;
        }
        uintptr total = default!;
        foreach (var (i, name) in needʗ1) {
            total += have[i];
            t.Logf("found %d samples in expected function %s\n"u8, have[i], name);
        }
        if (total == 0) {
            t.Logf("no samples in expected functions"u8);
            ok = false;
        }
        // We'd like to check a reasonable minimum, like
        // total / len(have) / smallconstant, but this test is
        // pretty flaky (see bug 7095).  So we'll just test to
        // make sure we got at least one sample.
        var min = (uintptr)1;
        foreach (var (i, name) in needʗ1) {
            if (have[i] < min) {
                t.Logf("%s has %d samples out of %d, want at least %d, ideally %d"u8, name, have[i], total, min, total / (uintptr)len(have));
                ok = false;
            }
        }
        return ok;
    };
}

// Fork can hang if preempted with signals frequently enough (see issue 5517).
// Ensure that we do not do this.
public static partial void TestCPUProfileWithFork(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        testenv.MustHaveExec(new pprof_internal_test_package.testing_TжTB(Ꮡt));
        var (exe, err) = os.Executable();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        nint heap = (1 << (int)(30));
        if (runtime.GOOS == "android"u8) {
            // Use smaller size for Android to avoid crash.
            heap = (100 << (int)(20));
        }
        if (runtime.GOOS == "windows"u8 && runtime.GOARCH == "arm"u8) {
            // Use smaller heap for Windows/ARM to avoid crash.
            heap = (100 << (int)(20));
        }
        if (testing.Short()) {
            heap = (100 << (int)(20));
        }
        // This makes fork slower.
        var garbage = new slice<byte>(heap);
        // Need to touch the slice, otherwise it won't be paged in.
        var done = new channel<bool>(0);
        var doneʗ1 = done;
        var garbageʗ1 = garbage;
        goǃ(() => {
            foreach (var (i, _) in garbageʗ1) {
                garbageʗ1[i] = 42;
            }
            doneʗ1.ᐸꟷ(true);
        });
        ᐸꟷ(done);
        ref var prof = ref builtin.heap(new bytes.Buffer(), out var Ꮡprof);
        {
            var errΔ1 = StartCPUProfile(new pprof_internal_test_package.bytes_BufferжWriter(Ꮡprof)); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        defer(StopCPUProfile, ref ᒐ);
        for (nint i = 0; i < 10; i++) {
            testenv.Command(new pprof_internal_test_package.testing_TжTB(Ꮡt), exe, "-h"u8).CombinedOutput();
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
public static void TestGoroutineSwitch(ж<testing.T> Ꮡt) {
    if (runtime.Compiler == "gccgo") {
        Ꮡt.Skip(notApplicableForGccgoˢ);
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
            var err = StartCPUProfile(new pprof_internal_test_package.bytes_BufferжWriter(Ꮡprof)); if (err != default!) {
                Ꮡt.Fatal(err);
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
        parseProfile(Ꮡt, prof.Bytes(), (uintptr countΔ1, slice<ж<profile.Location>> stk, map<@string, slice<@string>> _) => {
            // An entry with two frames with 'System' in its top frame
            // exists to record a PC without a traceback. Those are okay.
            if (len(stk) == 2) {
                @string nameΔ1 = (~stk[1]).Line[0].Function.Value.Name;
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
            @string name = (~stk[0]).Line[0].Function.Value.Name;
            if (name == "gogo"u8) {
                ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
                fprintStack(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡbuf), stk);
                Ꮡt.Fatalf("found profile entry for gogo:\n%s"u8, buf.String());
            }
        });
    }
}

internal static void fprintStack(io.Writer w, slice<ж<profile.Location>> stk) {
    if (len(stk) == 0) {
        fmt.Fprintf(w, " (stack empty)"u8);
    }
    foreach (var (_, loc) in stk) {
        fmt.Fprintf(w, " %#x"u8, (~loc).Address);
        fmt.Fprintf(w, " ("u8);
        foreach (var (i, line) in (~loc).Line) {
            if (i > 0) {
                fmt.Fprintf(w, " "u8);
            }
            fmt.Fprintf(w, "%s:%d"u8, (~line.Function).Name, line.ΔLine);
        }
        fmt.Fprintf(w, ")"u8);
    }
}

// Test that profiling of division operations is okay, especially on ARM. See issue 6681.
public static void TestMathBigDivide(ж<testing.T> Ꮡt) {
    testCPUProfile(Ꮡt, default!, (time.Duration duration) => {
        var tΔ1 = time.After(duration);
        var pi = @new<bigꓸInt>();
        while (ᐧ) {
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
    foreach (var (_, f) in strings_package.Split(spec, ","u8)) {
        if (!stackContains(f, count, stk, labels)) {
            return false;
        }
    }
    return true;
}

public static void TestMorestack(ж<testing.T> Ꮡt) {
    var matches = matchAndAvoidStacks(new Func<@string, uintptr, slice<ж<profile.Location>>, map<@string, slice<@string>>, bool>(stackContainsAll), new @string[]{"runtime.newstack,runtime/pprof.growstack"u8}.slice(), avoidFunctions());
    testCPUProfile(Ꮡt, matches, [MethodImpl(MethodImplOptions.NoInlining)] (time.Duration duration) => {
        var tΔ1 = time.After(duration);
        var c = new channel<bool>(0);
        while (ᐧ) {
            var cʗ1 = c;
            goǃ(() => {
                growstack1();
                cʗ1.ᐸꟷ(true);
            });
            var selᴛ2 = tΔ1;
            var selᴛ3 = c;
            switch (select(ᐸꟷ(selᴛ2, ꓸꓸꓸ), ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
            case 0 when selᴛ2.ꟷᐳ(out _): {
                return;
            }
            case 1 when selᴛ3.ꟷᐳ(out _): {
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
    array<byte> buf = new(2097152); /* (8 << (int)(18)) */
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
internal static readonly @string blockˢ = "block"u8;
internal static readonly @string contentionCyclesSecondˢ = "--- contention:\ncycles/second="u8;

internal partial struct TestBlockProfile_TestCase /*dyn*/ {
    internal @string name;
    internal Action<ж<testing.T>> f;
    internal slice<@string> stk;
    internal @string re;
}

public static void TestBlockProfile(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        ref var tests = ref heap<array<TestBlockProfile_TestCase>>(out var Ꮡtests);
        tests = new TestBlockProfile_TestCase[]{
            new(
                name: "chan recv"u8,
                f: blockChanRecv,
                stk: new @string[]{
                    "runtime.chanrecv1"u8,
                    "runtime/pprof.blockChanRecv"u8,
                    "runtime/pprof.TestBlockProfile"u8
                }.slice(),
                re: """

[0-9]+ [0-9]+ @( 0x[[:xdigit:]]+)+
#	0x[0-9a-f]+	runtime\.chanrecv1\+0x[0-9a-f]+	.*runtime/chan.go:[0-9]+
#	0x[0-9a-f]+	runtime/pprof\.blockChanRecv\+0x[0-9a-f]+	.*runtime/pprof/pprof_test.go:[0-9]+
#	0x[0-9a-f]+	runtime/pprof\.TestBlockProfile\+0x[0-9a-f]+	.*runtime/pprof/pprof_test.go:[0-9]+

"""u8),
            new(
                name: "chan send"u8,
                f: blockChanSend,
                stk: new @string[]{
                    "runtime.chansend1"u8,
                    "runtime/pprof.blockChanSend"u8,
                    "runtime/pprof.TestBlockProfile"u8
                }.slice(),
                re: """

[0-9]+ [0-9]+ @( 0x[[:xdigit:]]+)+
#	0x[0-9a-f]+	runtime\.chansend1\+0x[0-9a-f]+	.*runtime/chan.go:[0-9]+
#	0x[0-9a-f]+	runtime/pprof\.blockChanSend\+0x[0-9a-f]+	.*runtime/pprof/pprof_test.go:[0-9]+
#	0x[0-9a-f]+	runtime/pprof\.TestBlockProfile\+0x[0-9a-f]+	.*runtime/pprof/pprof_test.go:[0-9]+

"""u8),
            new(
                name: "chan close"u8,
                f: blockChanClose,
                stk: new @string[]{
                    "runtime.chanrecv1"u8,
                    "runtime/pprof.blockChanClose"u8,
                    "runtime/pprof.TestBlockProfile"u8
                }.slice(),
                re: """

[0-9]+ [0-9]+ @( 0x[[:xdigit:]]+)+
#	0x[0-9a-f]+	runtime\.chanrecv1\+0x[0-9a-f]+	.*runtime/chan.go:[0-9]+
#	0x[0-9a-f]+	runtime/pprof\.blockChanClose\+0x[0-9a-f]+	.*runtime/pprof/pprof_test.go:[0-9]+
#	0x[0-9a-f]+	runtime/pprof\.TestBlockProfile\+0x[0-9a-f]+	.*runtime/pprof/pprof_test.go:[0-9]+

"""u8),
            new(
                name: "select recv async"u8,
                f: blockSelectRecvAsync,
                stk: new @string[]{
                    "runtime.selectgo"u8,
                    "runtime/pprof.blockSelectRecvAsync"u8,
                    "runtime/pprof.TestBlockProfile"u8
                }.slice(),
                re: """

[0-9]+ [0-9]+ @( 0x[[:xdigit:]]+)+
#	0x[0-9a-f]+	runtime\.selectgo\+0x[0-9a-f]+	.*runtime/select.go:[0-9]+
#	0x[0-9a-f]+	runtime/pprof\.blockSelectRecvAsync\+0x[0-9a-f]+	.*runtime/pprof/pprof_test.go:[0-9]+
#	0x[0-9a-f]+	runtime/pprof\.TestBlockProfile\+0x[0-9a-f]+	.*runtime/pprof/pprof_test.go:[0-9]+

"""u8),
            new(
                name: "select send sync"u8,
                f: blockSelectSendSync,
                stk: new @string[]{
                    "runtime.selectgo"u8,
                    "runtime/pprof.blockSelectSendSync"u8,
                    "runtime/pprof.TestBlockProfile"u8
                }.slice(),
                re: """

[0-9]+ [0-9]+ @( 0x[[:xdigit:]]+)+
#	0x[0-9a-f]+	runtime\.selectgo\+0x[0-9a-f]+	.*runtime/select.go:[0-9]+
#	0x[0-9a-f]+	runtime/pprof\.blockSelectSendSync\+0x[0-9a-f]+	.*runtime/pprof/pprof_test.go:[0-9]+
#	0x[0-9a-f]+	runtime/pprof\.TestBlockProfile\+0x[0-9a-f]+	.*runtime/pprof/pprof_test.go:[0-9]+

"""u8),
            new(
                name: "mutex"u8,
                f: blockMutex,
                stk: new @string[]{
                    "sync.(*Mutex).Lock"u8,
                    "runtime/pprof.blockMutex"u8,
                    "runtime/pprof.TestBlockProfile"u8
                }.slice(),
                re: """

[0-9]+ [0-9]+ @( 0x[[:xdigit:]]+)+
#	0x[0-9a-f]+	sync\.\(\*Mutex\)\.Lock\+0x[0-9a-f]+	.*sync/mutex\.go:[0-9]+
#	0x[0-9a-f]+	runtime/pprof\.blockMutex\+0x[0-9a-f]+	.*runtime/pprof/pprof_test.go:[0-9]+
#	0x[0-9a-f]+	runtime/pprof\.TestBlockProfile\+0x[0-9a-f]+	.*runtime/pprof/pprof_test.go:[0-9]+

"""u8),
            new(
                name: "cond"u8,
                f: blockCond,
                stk: new @string[]{
                    "sync.(*Cond).Wait"u8,
                    "runtime/pprof.blockCond"u8,
                    "runtime/pprof.TestBlockProfile"u8
                }.slice(),
                re: """

[0-9]+ [0-9]+ @( 0x[[:xdigit:]]+)+
#	0x[0-9a-f]+	sync\.\(\*Cond\)\.Wait\+0x[0-9a-f]+	.*sync/cond\.go:[0-9]+
#	0x[0-9a-f]+	runtime/pprof\.blockCond\+0x[0-9a-f]+	.*runtime/pprof/pprof_test.go:[0-9]+
#	0x[0-9a-f]+	runtime/pprof\.TestBlockProfile\+0x[0-9a-f]+	.*runtime/pprof/pprof_test.go:[0-9]+

"""u8)
        }.array();
        // Generate block profile
        runtime.SetBlockProfileRate(1);
        defer(runtime.SetBlockProfileRate, (nint)(0), ref ᒐ);
        foreach (var (_, test) in tests.ΔRangeSnapshot()) {
            test.f(Ꮡt);
        }
        var testsʗ1 = tests;
        Ꮡt.Run(debug1ˢ, (ж<testing.T> tΔ1) => {
            ref var w = ref heap(new strings.Builder(), out var Ꮡw);
            Lookup(blockˢ).WriteTo(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡw), 1);
            @string prof = w.String();
            if (!strings_package.HasPrefix(prof, contentionCyclesSecondˢ)) {
                tΔ1.Fatalf("Bad profile header:\n%v"u8, prof);
            }
            if (strings_package.HasSuffix(prof, "#\t0x0\n\n"u8)) {
                tΔ1.Errorf("Useless 0 suffix:\n%v"u8, prof);
            }
            foreach (var (_, test) in testsʗ1.ΔRangeSnapshot()) {
                if (!regexp.MustCompile(strings_package.ReplaceAll(test.re, "\t"u8, "\t+"u8)).MatchString(prof)) {
                    tΔ1.Errorf("Bad %v entry, expect:\n%v\ngot:\n%v"u8, test.name, test.re, prof);
                }
            }
        });
        var testsʗ2 = tests;
        Ꮡt.Run(protoˢ, (ж<testing.T> tΔ2) => {
            // proto format
            ref var w = ref heap(new bytes.Buffer(), out var Ꮡw);
            Lookup(blockˢ).WriteTo(new pprof_internal_test_package.bytes_BufferжWriter(Ꮡw), 0);
            var (p, err) = profile.Parse(new pprof_internal_test_package.bytes_BufferжReader(Ꮡw));
            if (err != default!) {
                tΔ2.Fatalf("failed to parse profile: %v"u8, err);
            }
            tΔ2.Logf("parsed proto: %s"u8, p.OrTypedNil());
            {
                var errΔ1 = p.CheckValid(); if (errΔ1 != default!) {
                    tΔ2.Fatalf("invalid profile: %v"u8, errΔ1);
                }
            }
            var stks = profileStacks(p);
            foreach (var (_, test) in testsʗ2.ΔRangeSnapshot()) {
                if (!containsStack(stks, test.stk)) {
                    tΔ2.Errorf("No matching stack entry for %v, want %+v"u8, test.name, test.stk);
                }
            }
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static slice<slice<@string>> /*res*/ profileStacks(ж<profile.Profile> Ꮡp) {
    slice<slice<@string>> res = default!;

    ref var p = ref Ꮡp.DerefOrNull();
    foreach (var (_, s) in p.Sample) {
        slice<@string> stk = default!;
        foreach (var (_, l) in (~s).Location) {
            foreach (var (_, line) in (~l).Line) {
                stk = append(stk, (~line.Function).Name);
            }
        }
        res = append(res, stk);
    }
    return res;
}

internal static partial slice<slice<@string>> /*res*/ blockRecordStacks(slice<runtime.BlockProfileRecord> records) {
    slice<slice<@string>> res = default!;

    foreach (var (_, vᴛ1) in records) {
        var record = vᴛ1;

        var frames = runtime.CallersFrames(record.StackRecord.Stack());
        slice<@string> stk = default!;
        while (ᐧ) {
            var (frame, more) = frames.Next();
            stk = append(stk, frame.Function);
            if (!more) {
                break;
            }
        }
        res = append(res, stk);
    }
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
internal static readonly @string allˢ = "all"u8;

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
        var buf = new slice<byte>((64 << (int)(10)));
        while (ᐧ) {
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
internal static readonly @string chanReceiveˢ = "chan receive"u8;
internal static readonly @string blockChanRecvˢ = "blockChanRecv"u8;

internal static partial void blockChanRecv(ж<testing.T> Ꮡt) {
    var c = new channel<bool>(0);
    var cʗ1 = c;
    goǃ(() => {
        awaitBlockedGoroutine(Ꮡt, chanReceiveˢ, blockChanRecvˢ, 1);
        cʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(c);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string chanSendˢ = "chan send"u8;
internal static readonly @string blockChanSendˢ = "blockChanSend"u8;

internal static partial void blockChanSend(ж<testing.T> Ꮡt) {
    var c = new channel<bool>(0);
    var cʗ1 = c;
    goǃ(() => {
        awaitBlockedGoroutine(Ꮡt, chanSendˢ, blockChanSendˢ, 1);
        ᐸꟷ(cʗ1);
    });
    c.ᐸꟷ(true);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string blockChanCloseˢ = "blockChanClose"u8;

internal static partial void blockChanClose(ж<testing.T> Ꮡt) {
    var c = new channel<bool>(0);
    var cʗ1 = c;
    goǃ(() => {
        awaitBlockedGoroutine(Ꮡt, chanReceiveˢ, blockChanCloseˢ, 1);
        close(cʗ1);
    });
    ᐸꟷ(c);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string selectˢ = "select"u8;
internal static readonly @string blockSelectRecvAsyncˢ = "blockSelectRecvAsync"u8;

internal static partial void blockSelectRecvAsync(ж<testing.T> Ꮡt) {
    const nint numTries = 3;
    var c = new channel<bool>(1);
    var c2 = new channel<bool>(1);
    var cʗ1 = c;
    goǃ(() => {
        for (nint i = 0; i < numTries; i++) {
            awaitBlockedGoroutine(Ꮡt, selectˢ, blockSelectRecvAsyncˢ, 1);
            cʗ1.ᐸꟷ(true);
        }
    });
    for (nint i = 0; i < numTries; i++) {
        var selᴛ4 = c;
        var selᴛ5 = c2;
        switch (select(ᐸꟷ(selᴛ4, ꓸꓸꓸ), ᐸꟷ(selᴛ5, ꓸꓸꓸ))) {
        case 0 when selᴛ4.ꟷᐳ(out _): {
            break;
        }
        case 1 when selᴛ5.ꟷᐳ(out _): {
            break;
        }}
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string blockSelectSendSyncˢ = "blockSelectSendSync"u8;

internal static partial void blockSelectSendSync(ж<testing.T> Ꮡt) {
    var c = new channel<bool>(0);
    var c2 = new channel<bool>(0);
    var cʗ1 = c;
    goǃ(() => {
        awaitBlockedGoroutine(Ꮡt, selectˢ, blockSelectSendSyncˢ, 1);
        ᐸꟷ(cʗ1);
    });
    var selᴛ6 = c.ᐸꟷ(true, ꓸꓸꓸ);
    var selᴛ7 = c2.ᐸꟷ(true, ꓸꓸꓸ);
    switch (select(selᴛ6, selᴛ7)) {
    case 0: {
        break;
    }
    case 1: {
        break;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string syncMutexLockˢ = "sync.Mutex.Lock"u8;
internal static readonly @string blockMutexˢ = "blockMutex"u8;

internal static partial void blockMutex(ж<testing.T> Ꮡt) {
    ref var mu = ref heap(new sync.Mutex(), out var Ꮡmu);
    Ꮡmu.Lock();
    goǃ(() => {
        awaitBlockedGoroutine(Ꮡt, syncMutexLockˢ, blockMutexˢ, 1);
        Ꮡmu.Unlock();
    });
    // Note: Unlock releases mu before recording the mutex event,
    // so it's theoretically possible for this to proceed and
    // capture the profile before the event is recorded. As long
    // as this is blocked before the unlock happens, it's okay.
    Ꮡmu.Lock();
}

internal static partial void blockMutexN(ж<testing.T> Ꮡt, nint n, time.Duration d) {
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    ref var mu = ref heap(new sync.Mutex(), out var Ꮡmu);
    Ꮡmu.Lock();
    goǃ(() => {
        awaitBlockedGoroutine(Ꮡt, syncMutexLockˢ, blockMutexˢ, n);
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
internal static readonly @string syncCondWaitˢ = "sync.Cond.Wait"u8;
internal static readonly @string blockCondˢ = "blockCond"u8;

internal static partial void blockCond(ж<testing.T> Ꮡt) {
    ref var mu = ref heap(new sync.Mutex(), out var Ꮡmu);
    var c = sync.NewCond(new sync.MutexжLocker(Ꮡmu));
    Ꮡmu.Lock();
    var cʗ1 = c;
    goǃ(() => {
        awaitBlockedGoroutine(Ꮡt, syncCondWaitˢ, blockCondˢ, 1);
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
public static void TestBlockProfileBias(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        nint rate = (nint)1000; // arbitrary value
        runtime.SetBlockProfileRate(rate);
        defer(runtime.SetBlockProfileRate, (nint)(0), ref ᒐ);
        // simulate blocking events
        blockFrequentShort(rate);
        blockInfrequentLong(rate);
        ref var w = ref heap(new bytes.Buffer(), out var Ꮡw);
        Lookup(blockˢ).WriteTo(new pprof_internal_test_package.bytes_BufferжWriter(Ꮡw), 0);
        var (p, err) = profile.Parse(new pprof_internal_test_package.bytes_BufferжReader(Ꮡw));
        if (err != default!) {
            Ꮡt.Fatalf("failed to parse profile: %v"u8, err);
        }
        Ꮡt.Logf("parsed proto: %s"u8, p.OrTypedNil());
        var il = (float64)(-1D); // blockInfrequentLong duration
        var fs = (float64)(-1D); // blockFrequentShort duration
        foreach (var (_, s) in (~p).Sample) {
            foreach (var (_, l) in (~s).Location) {
                foreach (var (_, line) in (~l).Line) {
                    if (len((~s).Value) < 2) {
                        Ꮡt.Fatal(blockProfileHasLessThan2ˢ);
                    }
                    if ((~line.Function).Name == "runtime/pprof.blockInfrequentLong"u8){
                        il = (float64)(~s).Value[1];
                    } else 
                    if ((~line.Function).Name == "runtime/pprof.blockFrequentShort"u8) {
                        fs = (float64)(~s).Value[1];
                    }
                }
            }
        }
        if (il == -1D || fs == -1D) {
            Ꮡt.Fatal(blockProfileIsMissingˢ);
        }
        // stddev of bias from 100 runs on local machine multiplied by 10x
        const float64 threshold = 0.2;
        {
            var bias = (il - fs) / il; if (math.Abs(bias) > threshold){
                Ꮡt.Fatalf("bias: abs(%f) > %f"u8, bias, (float64)(threshold));
            } else {
                Ꮡt.Logf("bias: abs(%f) < %f"u8, bias, (float64)(threshold));
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
internal static readonly @string mutexCyclesSecondˢ = "--- mutex:\ncycles/second="u8;
internal static readonly @string dD0xXdigitˢ = @"^\d+ \d+ @(?: 0x[[:xdigit:]]+)+"u8;
internal static readonly @string runtimePprofBlockMutexˢ = "^#.*runtime/pprof.blockMutex.*$"u8;
internal static readonly @string recordsˢ = "records"u8;

public static void TestMutexProfile(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        // Generate mutex profile
        nint old = runtime.SetMutexProfileFraction(1);
        defer(runtime.SetMutexProfileFraction, old, ref ᒐ);
        if (old != 0) {
            Ꮡt.Fatalf("need MutexProfileRate 0, got %d"u8, old);
        }
        UntypedInt N = 100;
        time.Duration D = /* 100 * time.Millisecond */ 100000000;
        var start = time.Now();
        blockMutexN(Ꮡt, N, D);
        var blockMutexNTime = time.Since(start);
        Ꮡt.Run(debug1ˢ, (ж<testing.T> tΔ1) => {
            ref var w = ref heap(new strings.Builder(), out var Ꮡw);
            Lookup(mutexˢ).WriteTo(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡw), 1);
            @string prof = w.String();
            tΔ1.Logf("received profile: %v"u8, prof);
            if (!strings_package.HasPrefix(prof, mutexCyclesSecondˢ)) {
                tΔ1.Errorf("Bad profile header:\n%v"u8, prof);
            }
            prof = strings_package.Trim(prof, "\n"u8);
            var lines = strings_package.Split(prof, "\n"u8);
            if (len(lines) < 6) {
                tΔ1.Fatalf("expected >=6 lines, got %d %q\n%s"u8, len(lines), prof, prof);
            }
            // checking that the line is like "35258904 1 @ 0x48288d 0x47cd28 0x458931"
            @string r2 = dD0xXdigitˢ;
            {
                var (ok, err) = regexp.MatchString(r2, lines[3]); if (err != default! || !ok) {
                    tΔ1.Errorf("%q didn't match %q"u8, lines[3], r2);
                }
            }
            @string r3 = runtimePprofBlockMutexˢ;
            {
                var (ok, err) = regexp.MatchString(r3, lines[5]); if (err != default! || !ok) {
                    tΔ1.Errorf("%q didn't match %q"u8, lines[5], r3);
                }
            }
            tΔ1.Log(prof);
        });
        Ꮡt.Run(protoˢ, (ж<testing.T> tΔ2) => {
            // proto format
            ref var w = ref heap(new bytes.Buffer(), out var Ꮡw);
            Lookup(mutexˢ).WriteTo(new pprof_internal_test_package.bytes_BufferжWriter(Ꮡw), 0);
            var (p, err) = profile.Parse(new pprof_internal_test_package.bytes_BufferжReader(Ꮡw));
            if (err != default!) {
                tΔ2.Fatalf("failed to parse profile: %v"u8, err);
            }
            tΔ2.Logf("parsed proto: %s"u8, p.OrTypedNil());
            {
                var errΔ1 = p.CheckValid(); if (errΔ1 != default!) {
                    tΔ2.Fatalf("invalid profile: %v"u8, errΔ1);
                }
            }
            var stks = profileStacks(p);
            foreach (var (_, want) in new slice<@string>[]{
                new @string[]{"sync.(*Mutex).Unlock"u8, "runtime/pprof.blockMutexN.func1"u8}.slice()
            }.slice()) {
                if (!containsStack(stks, want)) {
                    tΔ2.Errorf("No matching stack entry for %+v"u8, want);
                }
            }
            nint i = 0;
            for (; i < len((~p).SampleType); i++) {
                if ((~(~p).SampleType[i]).Unit == "nanoseconds"u8) {
                    break;
                }
            }
            if (i >= len((~p).SampleType)) {
                tΔ2.Fatalf("profile did not contain nanoseconds sample"u8);
            }
            var total = (int64)0;
            foreach (var (_, s) in (~p).Sample) {
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
            var d = ((time.Duration)total);
            var lo = (time.Duration)(9000000000L);
            var hi = ((time.Duration)N) * blockMutexNTime * 11 / 10;
            if (d < lo || d > hi) {
                foreach (var (_, s) in (~p).Sample) {
                    tΔ2.Logf("sample: %s"u8, ((time.Duration)(~s).Value[i]));
                }
                tΔ2.Fatalf("profile samples total %v, want within range [%v, %v] (target: %v)"u8, d, lo, hi, (time.Duration)(10000000000L));
            }
        });
        Ꮡt.Run(recordsˢ, (ж<testing.T> tΔ3) => {
            // Record a mutex profile using the structured record API.
            slice<runtime.BlockProfileRecord> records = default!;
            while (ᐧ) {
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
            var want = new @string[]{"sync.(*Mutex).Unlock"u8, "runtime/pprof.blockMutexN.func1"u8, "runtime.goexit"u8}.slice();
            if (!containsStack(stks, want)) {
                tΔ3.Errorf("No matching stack entry for %+v"u8, want);
            }
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object didNotSeeExpectedˢ = (@string)"did not see expected function in profile"u8;

public static void TestMutexProfileRateAdjust(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        nint old = runtime.SetMutexProfileFraction(1);
        defer(runtime.SetMutexProfileFraction, old, ref ᒐ);
        if (old != 0) {
            Ꮡt.Fatalf("need MutexProfileRate 0, got %d"u8, old);
        }
        (int64 contentions, int64 delay) readProfile() {
            int64 contentionsΔ1 = default!;
            int64 delayΔ1 = default!;
            ref var w = ref heap(new bytes.Buffer(), out var Ꮡw);
            Lookup(mutexˢ).WriteTo(new pprof_internal_test_package.bytes_BufferжWriter(Ꮡw), 0);
            var (p, err) = profile.Parse(new pprof_internal_test_package.bytes_BufferжReader(Ꮡw));
            if (err != default!) {
                Ꮡt.Fatalf("failed to parse profile: %v"u8, err);
            }
            Ꮡt.Logf("parsed proto: %s"u8, p.OrTypedNil());
            {
                var errΔ1 = p.CheckValid(); if (errΔ1 != default!) {
                    Ꮡt.Fatalf("invalid profile: %v"u8, errΔ1);
                }
            }
            foreach (var (_, s) in (~p).Sample) {
                bool match = default!;
                bool runtimeInternal = default!;
                foreach (var (_, l) in (~s).Location) {
                    foreach (var (_, line) in (~l).Line) {
                        if ((~line.Function).Name == "runtime/pprof.blockMutex.func1"u8) {
                            match = true;
                        }
                        if ((~line.Function).Name == "runtime.unlock"u8) {
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
        blockMutex(Ꮡt);
        var (contentions, delay) = readProfile();
        if (contentions == 0) {
            // low-resolution timers can have delay of 0 in mutex profile
            Ꮡt.Fatal(didNotSeeExpectedˢ);
        }
        runtime.SetMutexProfileFraction(0);
        var (newContentions, newDelay) = readProfile();
        if (newContentions != contentions || newDelay != delay) {
            Ꮡt.Fatalf("sample value changed: got [%d, %d], want [%d, %d]"u8, newContentions, newDelay, contentions, delay);
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
internal static readonly @string labelˢ = "label"u8;
internal static readonly @string selfLabelˢ = "self-label"u8;
internal static readonly @string selfValueˢ = "self-value"u8;
internal static readonly @string fingLabelˢ = "fing-label"u8;
internal static readonly @string fingValueˢ = "fing-value"u8;

public static partial void TestGoroutineCounts(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
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
        garbage = default!;
        for (nint i = 0; i < 2; i++) {
            runtime.GC();
        }
        ᐸꟷ(fingReady);
        ref var w = ref heap(new bytes.Buffer(), out var Ꮡw);
        var goroutineProf = Lookup(goroutineˢ);
        // Check debug profile
        goroutineProf.WriteTo(new pprof_internal_test_package.bytes_BufferжWriter(Ꮡw), 1);
        @string prof = Ꮡw.String();
        ref var labels = ref heap<global::go.runtime.pprof_package.labelMap>(out var Ꮡlabels);
        labels = new labelMap(Labels(labelˢ, valueˢ));
        @string labelStr = "\n# labels: "u8 + Ꮡlabels.String();
        ref var selfLabel = ref heap<global::go.runtime.pprof_package.labelMap>(out var ᏑselfLabel);
        selfLabel = new labelMap(Labels(selfLabelˢ, selfValueˢ));
        @string selfLabelStr = "\n# labels: "u8 + ᏑselfLabel.String();
        ref var fingLabel = ref heap<global::go.runtime.pprof_package.labelMap>(out var ᏑfingLabel);
        fingLabel = new labelMap(Labels(fingLabelˢ, fingValueˢ));
        @string fingLabelStr = "\n# labels: "u8 + ᏑfingLabel.String();
        var orderedPrefix = new @string[]{
            "\n50 @ "u8,
            "\n44 @"u8, labelStr,
            "\n40 @"u8,
            "\n36 @"u8, labelStr,
            "\n10 @"u8,
            "\n9 @"u8, labelStr,
            "\n1 @"u8}.slice();
        if (!containsInOrder(prof, append(orderedPrefix, selfLabelStr).ꓸꓸꓸ)) {
            Ꮡt.Errorf("expected sorted goroutine counts with Labels:\n%s"u8, prof);
        }
        if (!containsInOrder(prof, append(orderedPrefix, fingLabelStr).ꓸꓸꓸ)) {
            Ꮡt.Errorf("expected sorted goroutine counts with Labels:\n%s"u8, prof);
        }
        // Check proto profile
        w.Reset();
        goroutineProf.WriteTo(new pprof_internal_test_package.bytes_BufferжWriter(Ꮡw), 0);
        var (p, err) = profile.Parse(new pprof_internal_test_package.bytes_BufferжReader(Ꮡw));
        if (err != default!) {
            Ꮡt.Errorf("error parsing protobuf profile: %v"u8, err);
        }
        {
            var errΔ1 = p.CheckValid(); if (errΔ1 != default!) {
                Ꮡt.Errorf("protobuf profile is invalid: %v"u8, errΔ1);
            }
        }
        var expectedLabels = new map<int64, map<@string, @string>>{
            [50] = new map<@string, @string>{},
            [44] = new map<@string, @string>{["label"u8] = "value"u8},
            [40] = new map<@string, @string>{},
            [36] = new map<@string, @string>{["label"u8] = "value"u8},
            [10] = new map<@string, @string>{},
            [9] = new map<@string, @string>{["label"u8] = "value"u8},
            [1] = new map<@string, @string>{["self-label"u8] = "self-value"u8, ["fing-label"u8] = "fing-value"u8}
        };
        if (!containsCountsLabels(p, expectedLabels)) {
            Ꮡt.Errorf("expected count profile to contain goroutines with counts and labels %v, got %v"u8,
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
        bool ok = default!;
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
        foreach (var (k, vs) in (~s).Label) {
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimePprofRuntimeˢ = "\truntime/pprof.runtime_goroutineProfileWithLabels+"u8;
internal static readonly @string runtimeRunfinqˢ = "runtime.runfinq"u8;
internal static readonly @string overlappingProfileˢ = "overlapping profile requests"u8;
internal static readonly @string finalizerNotPresentˢ = "finalizer not present"u8;
internal static readonly @string finalizerPresentˢ = "finalizer present"u8;
internal static readonly @string goroutineLaunchesˢ = "goroutine launches"u8;

internal partial class TestGoroutineProfileConcurrency_T /*ж<byte>*/;

public static void TestGoroutineProfileConcurrency(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    testenv.MustHaveParallelism(new pprof_internal_test_package.testing_TжTB(Ꮡt));
    var goroutineProf = Lookup(goroutineˢ);
    nint profilerCalls(@string s) => strings_package.Count(s, runtimePprofRuntimeˢ);
    bool includesFinalizer(@string s) => strings_package.Contains(s, runtimeRunfinqˢ);
    // Concurrent calls to the goroutine profiler should not trigger data races
    // or corruption.
    var goroutineProfʗ1 = goroutineProf;
    var profilerCallsʗ1 = profilerCalls;
    Ꮡt.Run(overlappingProfileˢ, (ж<testing.T> tΔ1) => {
        GoFrame ᒐ = default;
        try {
            ref var ctx = ref heap<context.Context>(out var Ꮡctx);
            Ꮡctx.ValueSlot = context.Background();
            (Ꮡctx.ValueSlot, var cancel) = context.WithTimeout(Ꮡctx.ValueSlot, (time.Duration)(10000000000L));
            var cancelʗ1 = cancel;
            defer(() => cancelʗ1(), ref ᒐ);
            ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
            for (nint i = 0; i < 2; i++) {
                Ꮡwg.Add(1);
                var cancelʗ2 = cancel;
                var goroutineProfʗ2 = goroutineProfʗ1;
                var profilerCallsʗ2 = profilerCallsʗ1;
                Do(Ꮡctx.ValueSlot, Labels("i"u8, fmt.Sprint(i)), [MethodImpl(MethodImplOptions.NoInlining)] (context.Context _) => {
                    var cancelʗ3 = cancelʗ2;
                    var goroutineProfʗ3 = goroutineProfʗ2;
                    var profilerCallsʗ3 = profilerCallsʗ2;
                    goǃ(() => {
                        GoFrame ᒐ = default;
                        try {
                            defer(Ꮡwg.Done, ref ᒐ);
                            while (Ꮡctx.ValueSlot.Err() == default!) {
                                ref var w = ref heap(new strings.Builder(), out var Ꮡw);
                                goroutineProfʗ3.WriteTo(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡw), 1);
                                @string prof = w.String();
                                nint count = profilerCallsʗ3(prof);
                                if (count >= 2) {
                                    tΔ1.Logf("prof %d\n%s"u8, count, prof);
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
    var includesFinalizerʗ1 = includesFinalizer;
    Ꮡt.Run(finalizerNotPresentˢ, (ж<testing.T> tΔ2) => {
        ref var w = ref heap(new strings.Builder(), out var Ꮡw);
        goroutineProfʗ4.WriteTo(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡw), 1);
        @string prof = w.String();
        if (includesFinalizerʗ1(prof)) {
            tΔ2.Errorf("profile includes finalizer (but finalizer should be marked as system):\n%s"u8, prof);
        }
    });
    // The finalizer goroutine should show up when it's running user code.
    var goroutineProfʗ5 = goroutineProf;
    var includesFinalizerʗ2 = includesFinalizer;
    Ꮡt.Run(finalizerPresentˢ, (ж<testing.T> tΔ3) => {
        GoFrame ᒐ = default;
        try {
            var obj = @new<TestGoroutineProfileConcurrency_T>();
            var (ch1, ch2) = (new channel<nint>(0), new channel<nint>(0));
            defer(ᴛ1 => close(ᴛ1), ch2, ref ᒐ);
            var ch1ʗ1 = ch1;
            var ch2ʗ1 = ch2;
            runtime.SetFinalizer(obj.OrTypedNil(), (any _) => {
                close(ch1ʗ1);
                ᐸꟷ(ch2ʗ1);
            });
            obj = default!;
            for (nint i = 10; i >= 0; i--) {
                var selᴛ8 = ch1;
                switch (trySelect(ᐸꟷ(selᴛ8, ꓸꓸꓸ))) {
                case 0 when selᴛ8.ꟷᐳ(out _): {
                    break;
                }
                default: {
                    if (i == 0) {
                        tΔ3.Fatalf("finalizer did not run"u8);
                    }
                    runtime.GC();
                    break;
                }}
            }
            ref var w = ref heap(new strings.Builder(), out var Ꮡw);
            goroutineProfʗ5.WriteTo(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡw), 1);
            @string prof = w.String();
            if (!includesFinalizerʗ2(prof)) {
                tΔ3.Errorf("profile does not include finalizer (and it should be marked as user):\n%s"u8, prof);
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
            ref var ctx = ref heap<context.Context>(out var Ꮡctx);
            Ꮡctx.ValueSlot = context.Background();
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
                    for (nint i = 0; Ꮡctx.ValueSlot.Err() == default!; i++) {
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
                if (i == 0){
                    Ꮡready.Done();
                } else 
                if (i % 16 == 0) {
                    // Yield on occasion so this sequence of goroutine launches
                    // doesn't monopolize a P. See issue #52934.
                    runtime.Gosched();
                }
                if (Ꮡctx.ValueSlot.Err() == default!) {
                    goǃ(Ꮡchurn.ValueSlot, i + 1);
                }
            };
            goǃ(() => {
                Ꮡchurn.ValueSlot(0);
            });
            Ꮡready.Wait();
            ref var w = ref heap(new array<bytes.Buffer>(3), out var Ꮡw);
            foreach (var (i, _) in w) {
                goroutineProfʗ6.WriteTo(new pprof_internal_test_package.bytes_BufferжWriter(Ꮡw.at<bytes.Buffer>(i)), 0);
            }
            foreach (var (i, _) in w) {
                var (p, err) = profile.Parse(new pprof_internal_test_package.bytes_ReaderжReader(bytes.NewReader(w[i].Bytes())));
                if (err != default!) {
                    tΔ4.Errorf("error parsing protobuf profile: %v"u8, err);
                }
                // High-numbered loop-i goroutines imply that every lower-numbered
                // loop-i goroutine should be present in the profile too.
                var counts = new map<@string, nint>();
                foreach (var (_, s) in (~p).Sample) {
                    var label = (~s).Label[tΔ4.Name() + "-loop-i"u8];
                    if (len(label) > 0) {
                        counts[label[0]]++;
                    }
                }
                for ((nint j, nint max) = (0, len(counts) - 1); j <= max; j++) {
                    nint n = counts[fmt.Sprint(j)];
                    if (n == 1 || (n == 2 && j == max)) {
                        continue;
                    }
                    tΔ4.Errorf("profile #%d's goroutines with label loop-i:%d; %d != 1 (or 2 for the last entry, %d)"u8,
                        i + 1, j, n, max);
                    tΔ4.Logf("counts %v"u8, counts);
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
        Ꮡt.Run(goroutineLaunchesˢ, testLaunches);
    }
}

// Regression test for #69998.
public static partial void TestGoroutineProfileCoro(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    testenv.MustHaveParallelism(new pprof_internal_test_package.testing_TжTB(Ꮡt));
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
            while (ᐧ) {
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
            while (ᐧ) {
                iterFuncʗ1();
                var selᴛ9 = doneʗ1;
                switch (trySelect(ᐸꟷ(selᴛ9, ꓸꓸꓸ))) {
                case 0 when selᴛ9.ꟷᐳ(out _): {
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

internal partial class TestGoroutineProfileIssue74090_T /*ж<byte>*/;

// This test tries to provoke a situation wherein the finalizer goroutine is
// erroneously inspected by the goroutine profiler in such a way that could
// cause a crash. See go.dev/issue/74090.
public static void TestGoroutineProfileIssue74090(ж<testing.T> Ꮡt) {
    testenv.MustHaveParallelism(new pprof_internal_test_package.testing_TжTB(Ꮡt));
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
        slice<ж<TestGoroutineProfileIssue74090_T>> objs = default!;
        foreach (var _ᴛ2 in range(10000)) {
            var obj = @new<TestGoroutineProfileIssue74090_T>();
            runtime.SetFinalizer(obj.OrTypedNil(), (any _) => {
            });
            objs = append(objs, obj);
        }
        objs = default!;
        // Queue up all the finalizers.
        runtime.GC();
        // Try to run a goroutine profile concurrently with finalizer execution
        // to trigger the bug.
        ref var w = ref heap(new strings.Builder(), out var Ꮡw);
        goroutineProf.WriteTo(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡw), 1);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string churnIˢ = "churn-i"u8;
internal static readonly @string concurrentLaunchesOpˢ = "concurrent_launches/op"u8;

public static void BenchmarkGoroutine(ж<testing.B> Ꮡb) {
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
                ref var ctx = ref heap<context.Context>(out var Ꮡctx);
                Ꮡctx.ValueSlot = context.Background();
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
                    if (Ꮡctx.ValueSlot.Err() == default!) {
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
    foreach (var (_, n) in new nint[]{50, 500, 5000}.slice()) {
        Ꮡb.Run(fmt.Sprintf("Profile.WriteTo idle %d"u8, n), withIdle(n, benchWriteTo));
        Ꮡb.Run(fmt.Sprintf("Profile.WriteTo churn %d"u8, n), withIdle(n, withChurn(benchWriteTo)));
        Ꮡb.Run(fmt.Sprintf("runtime.GoroutineProfile churn %d"u8, n), withIdle(n, withChurn(benchGoroutineProfile)));
    }
}

internal static int64 emptyCallStackTestRun;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lostProfileEventˢ = "lostProfileEvent"u8;

// Issue 18836.
public static void TestEmptyCallStack(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    @string name = fmt.Sprintf("test18836_%d"u8, emptyCallStackTestRun);
    emptyCallStackTestRun++;
    Ꮡt.Parallel();
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    var p = NewProfile(name);
    p.Add(fooˢ, 47674);
    p.WriteTo(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡbuf), 1);
    p.Remove(fooˢ);
    @string got = buf.String();
    @string prefix = name + " profile: total 1\n"u8;
    if (!strings_package.HasPrefix(got, prefix)) {
        Ꮡt.Fatalf("got:\n\t%q\nwant prefix:\n\t%q\n"u8, got, prefix);
    }
    @string lostevent = lostProfileEventˢ;
    if (!strings_package.Contains(got, lostevent)) {
        Ꮡt.Fatalf("got:\n\t%q\ndoes not contain:\n\t%q\n"u8, got, lostevent);
    }
}

// stackContainsLabeled takes a spec like funcname;key=value and matches if the stack has that key
// and value and has funcname somewhere in the stack.
internal static bool stackContainsLabeled(@string spec, uintptr count, slice<ж<profile.Location>> stk, map<@string, slice<@string>> labels) {
    var (@base, kv, ok) = strings_package.Cut(spec, ";"u8);
    if (!ok) {
        throw panic("no semicolon in key/value spec");
    }
    (var k, var v, ok) = strings_package.Cut(kv, "="u8);
    if (!ok) {
        throw panic("missing = in key/value spec");
    }
    if (!slices.Contains(labels[k], v)) {
        return false;
    }
    return stackContains(@base, count, stk, labels);
}

public static void TestCPUProfileLabel(ж<testing.T> Ꮡt) {
    var matches = matchAndAvoidStacks(new Func<@string, uintptr, slice<ж<profile.Location>>, map<@string, slice<@string>>, bool>(stackContainsLabeled), new @string[]{"runtime/pprof.cpuHogger;key=value"u8}.slice(), avoidFunctions());
    testCPUProfile(Ꮡt, matches, (time.Duration dur) => {
        Do(context.Background(), Labels(keyˢ, valueˢ), (context.Context _) => {
            cpuHogger(cpuHog1, Ꮡsalt1, dur);
        });
    });
}

public static void TestLabelRace(ж<testing.T> Ꮡt) {
    testenv.MustHaveParallelism(new pprof_internal_test_package.testing_TжTB(Ꮡt));
    // Test the race detector annotations for synchronization
    // between setting labels and consuming them from the
    // profile.
    var matches = matchAndAvoidStacks(new Func<@string, uintptr, slice<ж<profile.Location>>, map<@string, slice<@string>>, bool>(stackContainsLabeled), new @string[]{"runtime/pprof.cpuHogger;key=value"u8}.slice(), default!);
    testCPUProfile(Ꮡt, matches, [MethodImpl(MethodImplOptions.NoInlining)] (time.Duration dur) => {
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
internal static readonly @string resetˢ = "reset"u8;
internal static readonly @string loopIˢ = "loop-i"u8;
internal static readonly @string churnˢ = "churn"u8;

public static void TestGoroutineProfileLabelRace(ж<testing.T> Ꮡt) {
    testenv.MustHaveParallelism(new pprof_internal_test_package.testing_TжTB(Ꮡt));
    // Test the race detector annotations for synchronization
    // between setting labels and consuming them from the
    // goroutine profile. See issue #50292.
    Ꮡt.Run(resetˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
        GoFrame ᒐ = default;
        try {
            ref var ctx = ref heap<context.Context>(out var Ꮡctx);
            Ꮡctx.ValueSlot = context.Background();
            (Ꮡctx.ValueSlot, var cancel) = context.WithCancel(Ꮡctx.ValueSlot);
            var cancelʗ1 = cancel;
            defer(() => cancelʗ1(), ref ᒐ);
            var cancelʗ2 = cancel;
            goǃ(() => {
                var goroutineProf = Lookup(goroutineˢ);
                while (Ꮡctx.ValueSlot.Err() == default!) {
                    ref var w = ref heap(new strings.Builder(), out var Ꮡw);
                    goroutineProf.WriteTo(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡw), 1);
                    @string prof = w.String();
                    if (strings_package.Contains(prof, loopIˢ)) {
                        cancelʗ2();
                    }
                }
            });
            for (nint i = 0; Ꮡctx.ValueSlot.Err() == default!; i++) {
                Do(Ꮡctx.ValueSlot, Labels(loopIˢ, fmt.Sprint(i)), (context.Context ctxΔ1) => {
                });
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    Ꮡt.Run(churnˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ2) => {
        GoFrame ᒐ = default;
        try {
            ref var ctx = ref heap<context.Context>(out var Ꮡctx);
            Ꮡctx.ValueSlot = context.Background();
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
                if (Ꮡctx.ValueSlot.Err() == default!) {
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
internal static readonly @string sampleContainsBothSWhichˢ = "sample contains both %s, which must be labeled, and %s, which must not be labeled"u8;
internal static readonly @string sampleMustBeLabeledˢ = "sample must be labeled because of %s, but is not"u8;
internal static readonly @string sampleMustNotBeLabeledˢ = "sample must not be labeled because of %s, but is"u8;

// TestLabelSystemstack makes sure CPU profiler samples of goroutines running
// on systemstack include the correct pprof labels. See issue #48577
public static void TestLabelSystemstack(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Grab and re-set the initial value before continuing to ensure
    // GOGC doesn't actually change following the test.
    nint gogc = debug.SetGCPercent(100);
    debug.SetGCPercent(gogc);
    var matches = matchAndAvoidStacks(new Func<@string, uintptr, slice<ж<profile.Location>>, map<@string, slice<@string>>, bool>(stackContainsLabeled), new @string[]{"runtime.systemstack;key=value"u8}.slice(), avoidFunctions());
    var p = testCPUProfile(Ꮡt, matches, (time.Duration dur) => {
        Do(context.Background(), Labels(keyˢ, valueˢ), [MethodImpl(MethodImplOptions.NoInlining)] (context.Context ctx) => {
            parallelLabelHog(ctx, dur, gogc);
        });
    });
    // Two conditions to check:
    // * labelHog should always be labeled.
    // * The label should _only_ appear on labelHog and the Do call above.
    foreach (var (_, s) in (~p).Sample) {
        var isLabeled = (~s).Label != default! && slices.Contains((~s).Label[keyˢ], valueˢ);
        bool mayBeLabeled = default!;
        @string mustBeLabeled = default!;
        @string mustNotBeLabeled = default!;
        foreach (var (_, loc) in (~s).Location) {
            foreach (var (_, l) in (~loc).Line) {
                var exprᴛ1 = (~l.Function).Name;
                if (exprᴛ1 == "runtime/pprof.labelHog"u8 || exprᴛ1 == "runtime/pprof.parallelLabelHog"u8 || exprᴛ1 == "runtime/pprof.parallelLabelHog.func1"u8) {
                    mustBeLabeled = l.Function.Value.Name;
                }
                else if (exprᴛ1 == "runtime/pprof.Do"u8) {
                    mayBeLabeled = true;
                }
                else if (exprᴛ1 == "runtime.bgsweep"u8 || exprᴛ1 == "runtime.bgscavenge"u8 || exprᴛ1 == "runtime.forcegchelper"u8 || exprᴛ1 == "runtime.gcBgMarkWorker"u8 || exprᴛ1 == "runtime.runfinq"u8 || exprᴛ1 == "runtime.sysmon"u8) {
                    mustNotBeLabeled = l.Function.Value.Name;
                }
                else if (exprᴛ1 == "gogo"u8 || exprᴛ1 == "gosave_systemstack_switch"u8 || exprᴛ1 == "racecall"u8) {
                    mayBeLabeled = true;
                }

                // Do sets the labels, so samples may
                // or may not be labeled depending on
                // which part of the function they are
                // at.
                // Runtime system goroutines or threads
                // (such as those identified by
                // runtime.isSystemGoroutine). These
                // should never be labeled.
                // These are context switch/race
                // critical that we can't do a full
                // traceback from. Typically this would
                // be covered by the runtime check
                // below, but these symbols don't have
                // the package name.
                if (strings_package.HasPrefix((~l.Function).Name, runtimeˢ)) {
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
            fprintStack(new pprof_internal_test_package.strings_BuilderжWriter(Ꮡbuf), (~sʗ1).Location);
            Ꮡt.Errorf("%s: %s"u8, fmt.Sprintf(f, args.ꓸꓸꓸ), buf.String());
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
    for (nint i = 0; ᐧ ; i++) {
        var selᴛ10 = stop;
        switch (trySelect(ᐸꟷ(selᴛ10, ꓸꓸꓸ))) {
        case 0 when selᴛ10.ꟷᐳ(out _): {
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
internal static readonly @string profatomicˢ = "profatomic"u8;

// Check that there is no deadlock when the program receives SIGPROF while in
// 64bit atomics' critical section. Used to happen on mips{,le}. See #20146.
public static partial void TestAtomicLoadStore64(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.CreateTemp(""u8, profatomicˢ);
        if (err != default!) {
            Ꮡt.Fatalf("TempFile: %v"u8, err);
        }
        defer(os.Remove, f.Name(), ref ᒐ);
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        {
            var errΔ1 = StartCPUProfile(new os.FileжWriter(f)); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
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
internal static readonly @string proftracebackˢ = "proftraceback"u8;

public static partial void TestTracebackAll(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // With gccgo, if a profiling signal arrives at the wrong time
        // during traceback, it may crash or hang. See issue #29448.
        var (f, err) = os.CreateTemp(""u8, proftracebackˢ);
        if (err != default!) {
            Ꮡt.Fatalf("TempFile: %v"u8, err);
        }
        defer(os.Remove, f.Name(), ref ᒐ);
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        {
            var errΔ1 = StartCPUProfile(new os.FileжWriter(f)); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
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
    internal slice<uint64> input;     // following the input format assumed by profileBuilder.addCPUData.
    internal nint count;              // number of records in input.
    internal slice<slice<@string>> wantLocs; // ordered location entries with function names.
    internal slice<ж<profile.Sample>> wantSamples; // ordered samples, we care only about Value and the profile location IDs.
}

// TestTryAdd tests the cases that are hard to test with real program execution.
//
// For example, the current go compilers may not always inline functions
// involved in recursion but that may not be true in the future compilers. This
// tests such cases by using fake call sequences and forcing the profile build
// utilizing translateCPUProfile defined in proto_test.go
public static void TestTryAdd(ж<testing.T> Ꮡt) {
    {
        var (_, found) = findInlinedCall(inlinedCallerDump, (4 << (int)(10))); if (!found) {
            Ꮡt.Skip(canTDetermineWhetherˢ2);
        }
    }
    // inlinedCallerDump
    //   inlinedCalleeDump
    var pcs = new slice<uintptr>(2);
    inlinedCallerDump(pcs);
    var inlinedCallerStack = new slice<uint64>(2);
    foreach (var (i, _) in pcs) {
        inlinedCallerStack[i] = (uint64)pcs[i];
    }
    var wrapperPCs = new slice<uintptr>(1);
    inlinedWrapperCallerDump(wrapperPCs);
    {
        var (_, found) = findInlinedCall(recursionChainBottom, (4 << (int)(10))); if (!found) {
            Ꮡt.Skip(canTDetermineWhetherˢ3);
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
    foreach (var (i, _) in pcs) {
        recursionStack[i] = (uint64)pcs[i];
    }
    var period = (int64)(2000 * 1000); // 1/500*1e9 nanosec.
    var testCases = new TestTryAdd_testCases[]{new(
        name: "full_stack_trace"u8, // Sanity test for a normal, complete stack trace.

        input: new uint64[]{
            3, 0, 500, // hz = 500. Must match the period.

            5, 0, 50, inlinedCallerStack[0], inlinedCallerStack[1]
        }.slice(),
        count: 2,
        wantLocs: new slice<@string>[]{
            new @string[]{"runtime/pprof.inlinedCalleeDump"u8, "runtime/pprof.inlinedCallerDump"u8}.slice()
        }.slice(),
        wantSamples: new ж<profile.Sample>[]{
            Ꮡ(new profile.Sample(Value: new int64[]{50, 50 * period}.slice(), Location: new ж<profile.Location>[]{Ꮡ(new profile.Location(ID: 1))}.slice()))
        }.slice()
    ), new(
        name: "bug35538"u8,
        input: new uint64[]{
            3, 0, 500, // hz = 500. Must match the period.
 // Fake frame: tryAdd will have inlinedCallerDump
 // (stack[1]) on the deck when it encounters the next
 // inline function. It should accept this.

            7, 0, 10, inlinedCallerStack[0], inlinedCallerStack[1], inlinedCallerStack[0], inlinedCallerStack[1],
            5, 0, 20, inlinedCallerStack[0], inlinedCallerStack[1]
        }.slice(),
        count: 3,
        wantLocs: new slice<@string>[]{new @string[]{"runtime/pprof.inlinedCalleeDump"u8, "runtime/pprof.inlinedCallerDump"u8}.slice()}.slice(),
        wantSamples: new ж<profile.Sample>[]{
            Ꮡ(new profile.Sample(Value: new int64[]{10, 10 * period}.slice(), Location: new ж<profile.Location>[]{Ꮡ(new profile.Location(ID: 1)), Ꮡ(new profile.Location(ID: 1))}.slice())),
            Ꮡ(new profile.Sample(Value: new int64[]{20, 20 * period}.slice(), Location: new ж<profile.Location>[]{Ꮡ(new profile.Location(ID: 1))}.slice()))
        }.slice()
    ), new(
        name: "bug38096"u8,
        input: new uint64[]{
            3, 0, 500, // hz = 500. Must match the period.
 // count (data[2]) == 0 && len(stk) == 1 is an overflow
 // entry. The "stk" entry is actually the count.

            4, 0, 0, 4242
        }.slice(),
        count: 2,
        wantLocs: new slice<@string>[]{new @string[]{"runtime/pprof.lostProfileEvent"u8}.slice()}.slice(),
        wantSamples: new ж<profile.Sample>[]{
            Ꮡ(new profile.Sample(Value: new int64[]{4242, 4242 * period}.slice(), Location: new ж<profile.Location>[]{Ꮡ(new profile.Location(ID: 1))}.slice()))
        }.slice()
    ), new(
        name: "directly_recursive_func_is_not_inlined"u8, // If a function is directly called recursively then it must
 // not be inlined in the caller.
 //
 // N.B. We're generating an impossible profile here, with a
 // recursive inlineCalleeDump call. This is simulating a non-Go
 // function that looks like an inlined Go function other than
 // its recursive property. See pcDeck.tryAdd.

        input: new uint64[]{
            3, 0, 500, // hz = 500. Must match the period.

            5, 0, 30, inlinedCallerStack[0], inlinedCallerStack[0],
            4, 0, 40, inlinedCallerStack[0]
        }.slice(),
        count: 3, // inlinedCallerDump shows up here because
 // runtime_expandFinalInlineFrame adds it to the stack frame.

        wantLocs: new slice<@string>[]{new @string[]{"runtime/pprof.inlinedCalleeDump"u8}.slice(), new @string[]{"runtime/pprof.inlinedCallerDump"u8}.slice()}.slice(),
        wantSamples: new ж<profile.Sample>[]{
            Ꮡ(new profile.Sample(Value: new int64[]{30, 30 * period}.slice(), Location: new ж<profile.Location>[]{Ꮡ(new profile.Location(ID: 1)), Ꮡ(new profile.Location(ID: 1)), Ꮡ(new profile.Location(ID: 2))}.slice())),
            Ꮡ(new profile.Sample(Value: new int64[]{40, 40 * period}.slice(), Location: new ж<profile.Location>[]{Ꮡ(new profile.Location(ID: 1)), Ꮡ(new profile.Location(ID: 2))}.slice()))
        }.slice()
    ), new(
        name: "recursion_chain_inline"u8,
        input: new uint64[]{
            3, 0, 500, // hz = 500. Must match the period.

            9, 0, 10, recursionStack[0], recursionStack[1], recursionStack[2], recursionStack[3], recursionStack[4], recursionStack[5]
        }.slice(),
        count: 2,
        wantLocs: new slice<@string>[]{
            new @string[]{"runtime/pprof.recursionChainBottom"u8}.slice(),
            new @string[]{
                "runtime/pprof.recursionChainMiddle"u8,
                "runtime/pprof.recursionChainTop"u8,
                "runtime/pprof.recursionChainBottom"u8}.slice(),
            new @string[]{
                "runtime/pprof.recursionChainMiddle"u8,
                "runtime/pprof.recursionChainTop"u8,
                "runtime/pprof.TestTryAdd"u8}.slice()
        }.slice(), // inlined into the test.

        wantSamples: new ж<profile.Sample>[]{
            Ꮡ(new profile.Sample(Value: new int64[]{10, 10 * period}.slice(), Location: new ж<profile.Location>[]{Ꮡ(new profile.Location(ID: 1)), Ꮡ(new profile.Location(ID: 2)), Ꮡ(new profile.Location(ID: 3))}.slice()))
        }.slice()
    ), new(
        name: "truncated_stack_trace_later"u8,
        input: new uint64[]{
            3, 0, 500, // hz = 500. Must match the period.

            5, 0, 50, inlinedCallerStack[0], inlinedCallerStack[1],
            4, 0, 60, inlinedCallerStack[0]
        }.slice(),
        count: 3,
        wantLocs: new slice<@string>[]{new @string[]{"runtime/pprof.inlinedCalleeDump"u8, "runtime/pprof.inlinedCallerDump"u8}.slice()}.slice(),
        wantSamples: new ж<profile.Sample>[]{
            Ꮡ(new profile.Sample(Value: new int64[]{50, 50 * period}.slice(), Location: new ж<profile.Location>[]{Ꮡ(new profile.Location(ID: 1))}.slice())),
            Ꮡ(new profile.Sample(Value: new int64[]{60, 60 * period}.slice(), Location: new ж<profile.Location>[]{Ꮡ(new profile.Location(ID: 1))}.slice()))
        }.slice()
    ), new(
        name: "truncated_stack_trace_first"u8,
        input: new uint64[]{
            3, 0, 500, // hz = 500. Must match the period.

            4, 0, 70, inlinedCallerStack[0],
            5, 0, 80, inlinedCallerStack[0], inlinedCallerStack[1]
        }.slice(),
        count: 3,
        wantLocs: new slice<@string>[]{new @string[]{"runtime/pprof.inlinedCalleeDump"u8, "runtime/pprof.inlinedCallerDump"u8}.slice()}.slice(),
        wantSamples: new ж<profile.Sample>[]{
            Ꮡ(new profile.Sample(Value: new int64[]{70, 70 * period}.slice(), Location: new ж<profile.Location>[]{Ꮡ(new profile.Location(ID: 1))}.slice())),
            Ꮡ(new profile.Sample(Value: new int64[]{80, 80 * period}.slice(), Location: new ж<profile.Location>[]{Ꮡ(new profile.Location(ID: 1))}.slice()))
        }.slice()
    ), new(
        name: "truncated_stack_trace_only"u8, // We can recover the inlined caller from a truncated stack.

        input: new uint64[]{
            3, 0, 500, // hz = 500. Must match the period.

            4, 0, 70, inlinedCallerStack[0]
        }.slice(),
        count: 2,
        wantLocs: new slice<@string>[]{new @string[]{"runtime/pprof.inlinedCalleeDump"u8, "runtime/pprof.inlinedCallerDump"u8}.slice()}.slice(),
        wantSamples: new ж<profile.Sample>[]{
            Ꮡ(new profile.Sample(Value: new int64[]{70, 70 * period}.slice(), Location: new ж<profile.Location>[]{Ꮡ(new profile.Location(ID: 1))}.slice()))
        }.slice()
    ), new(
        name: "truncated_stack_trace_twice"u8, // The same location is used for duplicated stacks.

        input: new uint64[]{
            3, 0, 500, // hz = 500. Must match the period.

            4, 0, 70, inlinedCallerStack[0], // Fake frame: add a fake call to
 // inlinedCallerDump to prevent this sample
 // from getting merged into above.

            5, 0, 80, inlinedCallerStack[1], inlinedCallerStack[0]
        }.slice(),
        count: 3,
        wantLocs: new slice<@string>[]{
            new @string[]{"runtime/pprof.inlinedCalleeDump"u8, "runtime/pprof.inlinedCallerDump"u8}.slice(),
            new @string[]{"runtime/pprof.inlinedCallerDump"u8}.slice()
        }.slice(),
        wantSamples: new ж<profile.Sample>[]{
            Ꮡ(new profile.Sample(Value: new int64[]{70, 70 * period}.slice(), Location: new ж<profile.Location>[]{Ꮡ(new profile.Location(ID: 1))}.slice())),
            Ꮡ(new profile.Sample(Value: new int64[]{80, 80 * period}.slice(), Location: new ж<profile.Location>[]{Ꮡ(new profile.Location(ID: 2)), Ꮡ(new profile.Location(ID: 1))}.slice()))
        }.slice()
    ), new(
        name: "expand_wrapper_function"u8,
        input: new uint64[]{
            3, 0, 500, // hz = 500. Must match the period.

            4, 0, 50, (uint64)wrapperPCs[0]
        }.slice(),
        count: 2,
        wantLocs: new slice<@string>[]{new @string[]{"runtime/pprof.inlineWrapper.dump"u8}.slice()}.slice(),
        wantSamples: new ж<profile.Sample>[]{
            Ꮡ(new profile.Sample(Value: new int64[]{50, 50 * period}.slice(), Location: new ж<profile.Location>[]{Ꮡ(new profile.Location(ID: 1))}.slice()))
        }.slice()
    )
    }.slice();
    foreach (var (_, vᴛ1) in testCases) {
        ref var tc = ref heap(new TestTryAdd_testCases(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
            var (p, err) = translateCPUProfile(tcʗ1.input, tcʗ1.count);
            if (err != default!) {
                tΔ1.Fatalf("translating profile: %v"u8, err);
            }
            tΔ1.Logf("Profile: %v\n"u8, p.OrTypedNil());
            // One location entry with all inlined functions.
            slice<slice<@string>> gotLoc = default!;
            foreach (var (_, loc) in (~p).Location) {
                slice<@string> names = default!;
                foreach (var (_, line) in (~loc).Line) {
                    names = append(names, (~line.Function).Name);
                }
                gotLoc = append(gotLoc, names);
            }
            {
                @string got = fmtJSON(gotLoc);
                @string want = fmtJSON(tcʗ1.wantLocs); if (got != want) {
                    tΔ1.Errorf("Got Location = %+v\n\twant %+v"u8, got, want);
                }
            }
            // All samples should point to one location.
            slice<ж<profile.Sample>> gotSamples = default!;
            foreach (var (_, sample) in (~p).Sample) {
                slice<ж<profile.Location>> locs = default!;
                foreach (var (_, loc) in (~sample).Location) {
                    locs = append(locs, Ꮡ(new profile.Location(ID: (~loc).ID)));
                }
                gotSamples = append(gotSamples, Ꮡ(new profile.Sample(Value: (~sample).Value, Location: locs)));
            }
            {
                @string got = fmtJSON(gotSamples);
                @string want = fmtJSON(tcʗ1.wantSamples); if (got != want) {
                    tΔ1.Errorf("Got Samples = %+v\n\twant %+v"u8, got, want);
                }
            }
        });
    }
}

public static void TestTimeVDSO(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Test that time functions have the right stack trace. In particular,
    // it shouldn't be recursive.
    if (runtime.GOOS == "android"u8) {
        // Flaky on Android, issue 48655. VDSO may not be enabled.
        testenv.SkipFlaky(new pprof_internal_test_package.testing_TжTB(Ꮡt), 48655);
    }
    var matches = matchAndAvoidStacks(new Func<@string, uintptr, slice<ж<profile.Location>>, map<@string, slice<@string>>, bool>(stackContains), new @string[]{"time.now"u8}.slice(), avoidFunctions());
    var p = testCPUProfile(Ꮡt, matches, (time.Duration dur) => {
        var t0 = time.Now();
        while (ᐧ) {
            var tΔ1 = time.Now();
            if (tΔ1.Sub(t0) >= dur) {
                return;
            }
        }
    });
    // Check for recursive time.now sample.
    foreach (var (_, sample) in (~p).Sample) {
        bool seenNow = default!;
        foreach (var (_, loc) in (~sample).Location) {
            foreach (var (_, line) in (~loc).Line) {
                if ((~line.Function).Name == "time.now"u8) {
                    if (seenNow) {
                        Ꮡt.Fatalf("unexpected recursive time.now"u8);
                    }
                    seenNow = true;
                }
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goroutineDeepˢ = "goroutineDeep"u8;
internal static readonly @string runtimePprofˢ = "runtime/pprof.produceProfileEvents"u8;

internal partial struct TestProfilerStackDepth_tests /*dyn*/ {
    internal @string profiler;
    internal slice<@string> prefix;
}

public static partial void TestProfilerStackDepth(ж<testing.T> Ꮡt) {
    Ꮡt.Cleanup(disableSampling());
    UntypedInt depth = 128;
    goǃ(produceProfileEvents, Ꮡt, (nint)(depth));
    awaitBlockedGoroutine(Ꮡt, chanReceiveˢ, goroutineDeepˢ, 1);
    var tests = new TestProfilerStackDepth_tests[]{
        new("heap"u8, new @string[]{"runtime/pprof.allocDeep"u8}.slice()),
        new("block"u8, new @string[]{"runtime.chanrecv1"u8, "runtime/pprof.blockChanDeep"u8}.slice()),
        new("mutex"u8, new @string[]{"sync.(*Mutex).Unlock"u8, "runtime/pprof.blockMutexDeep"u8}.slice()),
        new("goroutine"u8, new @string[]{"runtime.gopark"u8, "runtime.chanrecv"u8, "runtime.chanrecv1"u8, "runtime/pprof.goroutineDeep"u8}.slice())
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new TestProfilerStackDepth_tests(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.profiler, (ж<testing.T> tΔ1) => {
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            {
                var errΔ1 = Lookup(testʗ1.profiler).WriteTo(new pprof_internal_test_package.bytes_BufferжWriter(Ꮡbuf), 0); if (errΔ1 != default!) {
                    tΔ1.Fatalf("failed to write heap profile: %v"u8, errΔ1);
                }
            }
            var (p, err) = profile.Parse(new pprof_internal_test_package.bytes_BufferжReader(Ꮡbuf));
            if (err != default!) {
                tΔ1.Fatalf("failed to parse heap profile: %v"u8, err);
            }
            tΔ1.Logf("Profile = %v"u8, p.OrTypedNil());
            var stks = profileStacks(p);
            slice<slice<@string>> matchedStacks = default!;
            foreach (var (_, stk) in stks) {
                if (!hasPrefix(stk, testʗ1.prefix)) {
                    continue;
                }
                // We may get multiple stacks which contain the prefix we want, but
                // which might not have enough frames, e.g. if the profiler hides
                // some leaf frames that would count against the stack depth limit.
                // Check for at least one match
                matchedStacks = append(matchedStacks, stk);
                if (len(stk) != depth) {
                    continue;
                }
                {
                    @string rootFn = stk[depth - 1];
                    @string wantFn = runtimePprofˢ; if (rootFn != wantFn) {
                        continue;
                    }
                }
                // Found what we wanted
                return;
            }
            foreach (var (_, stk) in matchedStacks) {
                tΔ1.Logf("matched stack=%s"u8, stk);
                if (len(stk) != depth) {
                    tΔ1.Errorf("want stack depth = %d, got %d"u8, (nint)(depth), len(stk));
                }
                {
                    @string rootFn = stk[depth - 1];
                    @string wantFn = runtimePprofˢ; if (rootFn != wantFn) {
                        tΔ1.Errorf("want stack stack root %s, got %v"u8, wantFn, rootFn);
                    }
                }
            }
        });
    }
}

internal static bool hasPrefix(slice<@string> stk, slice<@string> prefix) {
    if (len(prefix) > len(stk)) {
        return false;
    }
    foreach (var (i, _) in prefix) {
        if (stk[i] != prefix[i]) {
            return false;
        }
    }
    return true;
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
    memSink = new slice<byte>((1 << (int)(20)));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string blockChanDeepˢ = "blockChanDeep"u8;

// blockChanDeep produces a block profile event at stack depth n, including the
// caller.
internal static partial void blockChanDeep(ж<testing.T> Ꮡt, nint n) {
    if (n > 1) {
        blockChanDeep(Ꮡt, n - 1);
        return;
    }
    var ch = new channel<EmptyStruct>(0);
    var chʗ1 = ch;
    goǃ(() => {
        awaitBlockedGoroutine(Ꮡt, chanReceiveˢ, blockChanDeepˢ, 1);
        chʗ1.ᐸꟷ(new EmptyStruct());
    });
    ᐸꟷ(ch);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string blockMutexDeepˢ = "blockMutexDeep"u8;

// blockMutexDeep produces a block profile event at stack depth n, including the
// caller.
internal static partial void blockMutexDeep(ж<testing.T> Ꮡt, nint n) {
    if (n > 1) {
        blockMutexDeep(Ꮡt, n - 1);
        return;
    }
    ref var mu = ref heap(new sync.Mutex(), out var Ꮡmu);
    goǃ(() => {
        Ꮡmu.Lock();
        Ꮡmu.Lock();
    });
    awaitBlockedGoroutine(Ꮡt, syncMutexLockˢ, blockMutexDeepˢ, 1);
    Ꮡmu.Unlock();
}

// goroutineDeep blocks at stack depth n, including the caller until the test is
// finished.
internal static void goroutineDeep(ж<testing.T> Ꮡt, nint n) {
    if (n > 1) {
        goroutineDeep(Ꮡt, n - 1);
        return;
    }
    var wait = new channel<EmptyStruct>(1);
    var waitʗ1 = wait;
    Ꮡt.Cleanup(() => {
        waitʗ1.ᐸꟷ(new EmptyStruct());
    });
    ᐸꟷ(wait);
}

// produceProfileEvents produces pprof events at the given stack depth and then
// blocks in goroutineDeep until the test completes. The stack traces are
// guaranteed to have exactly the desired depth with produceProfileEvents as
// their root frame which is expected by TestProfilerStackDepth.
internal static void produceProfileEvents(ж<testing.T> Ꮡt, nint depth) {
    allocDeep(depth - 1); // -1 for produceProfileEvents, **
    blockChanDeep(Ꮡt, depth - 2); // -2 for produceProfileEvents, **, chanrecv1
    blockMutexDeep(Ꮡt, depth - 2); // -2 for produceProfileEvents, **, Unlock
    memSink = default!;
    runtime.GC();
    goroutineDeep(Ꮡt, depth - 4); // -4 for produceProfileEvents, **, chanrecv1, chanrev, gopark
}

internal static partial slice<@string> getProfileStacks(Func<slice<runtime.BlockProfileRecord>, (nint, bool)> collect, bool fileLine) {
    nint n = default!;
    bool ok = default!;
    slice<runtime.BlockProfileRecord> p = default!;
    while (ᐧ) {
        p = new slice<runtime.BlockProfileRecord>(n, () => new(nil));
        (n, ok) = collect(p);
        if (ok) {
            p = p.slice(0, n);
            break;
        }
    }
    slice<@string> stacks = default!;
    foreach (var (_, vᴛ1) in p) {
        var r = vᴛ1;

        ref var stack = ref heap(new strings.Builder(), out var Ꮡstack);
        foreach (var (i, pc) in r.StackRecord.Stack()) {
            if (i > 0) {
                Ꮡstack.WriteByte((rune)'\n');
            }
            // Use FuncForPC instead of CallersFrames,
            // because we want to see the info for exactly
            // the PCs returned by the mutex profile to
            // ensure inlined calls have already been properly
            // expanded.
            var f = runtime.FuncForPC(pc - 1);
            Ꮡstack.WriteString(f.Name());
            if (fileLine) {
                Ꮡstack.WriteByte((rune)' ');
                var (@file, line) = f.FileLine(pc - 1);
                Ꮡstack.WriteString(@file);
                Ꮡstack.WriteByte((rune)':');
                Ꮡstack.WriteString(strconv.Itoa(line));
            }
        }
        stacks = append(stacks, stack.String());
    }
    return stacks;
}

public static partial void TestMutexBlockFullAggregation(ж<testing.T> Ꮡt) {
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
        defer(runtime.SetBlockProfileRate, (nint)(0), ref ᒐ);
        ref var wg = ref heap<sync.WaitGroup>(out var Ꮡwg);
        wg = new sync.WaitGroup(nil);
        Ꮡwg.Add(workers);
        for (nint j = 0; j < workers; j++) {
            goǃ(() => {
                for (nint i = 0; i < iters; i++) {
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
            var stacks = getProfileStacks(collect, true);
            var seen = new map<@string, EmptyStruct>();
            foreach (var (_, s) in stacks) {
                {
                    var (_, ok) = seen[s, ꟷ]; if (ok) {
                        Ꮡt.Errorf("saw duplicate entry in %s profile with stack:\n%s"u8, name, s);
                    }
                }
                seen[s] = new EmptyStruct();
            }
            if (len(seen) == 0) {
                Ꮡt.Errorf("did not see any samples in %s profile for this test"u8, name);
            }
        }
        var assertNoDuplicatesʗ1 = assertNoDuplicates;
        Ꮡt.Run(mutexˢ, (ж<testing.T> tΔ1) => {
            assertNoDuplicatesʗ1(mutexˢ, runtime.MutexProfile);
        });
        var assertNoDuplicatesʗ2 = assertNoDuplicates;
        Ꮡt.Run(blockˢ, (ж<testing.T> tΔ2) => {
            assertNoDuplicatesʗ2(blockˢ, runtime.BlockProfile);
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void inlineA(ж<sync.Mutex> Ꮡmu, ж<sync.WaitGroup> Ꮡwg) {
    inlineB(Ꮡmu, Ꮡwg);
}

internal static void inlineB(ж<sync.Mutex> Ꮡmu, ж<sync.WaitGroup> Ꮡwg) {
    inlineC(Ꮡmu, Ꮡwg);
}

internal static void inlineC(ж<sync.Mutex> Ꮡmu, ж<sync.WaitGroup> Ꮡwg) {
    GoFrame ᒐ = default;
    try {
        defer(Ꮡwg.Done, ref ᒐ);
        Ꮡmu.Lock();
        Ꮡmu.Unlock();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void inlineD(ж<sync.Mutex> Ꮡmu, ж<sync.WaitGroup> Ꮡwg) {
    inlineE(Ꮡmu, Ꮡwg);
}

internal static void inlineE(ж<sync.Mutex> Ꮡmu, ж<sync.WaitGroup> Ꮡwg) {
    inlineF(Ꮡmu, Ꮡwg);
}

internal static void inlineF(ж<sync.Mutex> Ꮡmu, ж<sync.WaitGroup> Ꮡwg) {
    GoFrame ᒐ = default;
    try {
        defer(Ꮡwg.Done, ref ᒐ);
        Ꮡmu.Unlock();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string inlineCˢ = "inlineC"u8;
internal static readonly object didNotSeeExpectedStackˢ = (@string)"did not see expected stack"u8;

internal partial struct TestBlockMutexProfileInlineExpansion_tcs /*dyn*/ {
    public @string Name;
    public Func<slice<runtime.BlockProfileRecord>, (nint, bool)> Collect;
    public @string SubStack;
}

public static partial void TestBlockMutexProfileInlineExpansion(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        runtime.SetBlockProfileRate(1);
        defer(runtime.SetBlockProfileRate, (nint)(0), ref ᒐ);
        nint prev = runtime.SetMutexProfileFraction(1);
        defer(runtime.SetMutexProfileFraction, prev, ref ᒐ);
        ref var mu = ref heap(new sync.Mutex(), out var Ꮡmu);
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(2);
        Ꮡmu.Lock();
        goǃ(inlineA, Ꮡmu, Ꮡwg);
        awaitBlockedGoroutine(Ꮡt, syncMutexLockˢ, inlineCˢ, 1);
        // inlineD will unblock inlineA
        goǃ(inlineD, Ꮡmu, Ꮡwg);
        Ꮡwg.Wait();
        var tcs = new TestBlockMutexProfileInlineExpansion_tcs[]{
            new(
                Name: "mutex"u8,
                Collect: runtime.MutexProfile,
                SubStack: """
sync.(*Mutex).Unlock
runtime/pprof.inlineF
runtime/pprof.inlineE
runtime/pprof.inlineD
"""u8
            ),
            new(
                Name: "block"u8,
                Collect: runtime.BlockProfile,
                SubStack: """
sync.(*Mutex).Lock
runtime/pprof.inlineC
runtime/pprof.inlineB
runtime/pprof.inlineA
"""u8
            )
        }.slice();
        foreach (var (_, vᴛ1) in tcs) {
            ref var tc = ref heap(new TestBlockMutexProfileInlineExpansion_tcs(), out var Ꮡtc);
            tc = vᴛ1;

            var tcʗ1 = tc;
            Ꮡt.Run(tc.Name, (ж<testing.T> tΔ1) => {
                var stacks = getProfileStacks(tcʗ1.Collect, false);
                foreach (var (_, s) in stacks) {
                    if (strings_package.Contains(s, tcʗ1.SubStack)) {
                        return;
                    }
                }
                tΔ1.Error(didNotSeeExpectedStackˢ);
                tΔ1.Logf("wanted:\n%s"u8, tcʗ1.SubStack);
                tΔ1.Logf("got: %s"u8, stacks);
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mutexProfileˢ = "MutexProfile"u8;
internal static readonly @string goroutineProfileˢ = "GoroutineProfile"u8;
internal static readonly @string blockProfileˢ = "BlockProfile"u8;
internal static readonly @string memProfileInUseZeroTrueˢ = "MemProfile/inUseZero=true"u8;
internal static readonly @string memProfileInUseZeroFalseˢ = "MemProfile/inUseZero=false"u8;

public static void TestProfileRecordNullPadding(ж<testing.T> Ꮡt) {
    // Produce events for the different profile types.
    Ꮡt.Cleanup(disableSampling());
    memSink = new slice<byte>(1); // MemProfile
    ᐸꟷ(time.After(time.Millisecond)); // BlockProfile
    blockMutex(Ꮡt); // MutexProfile
    runtime.GC();
    // Test that all profile records are null padded.
    testProfileRecordNullPadding<runtime.BlockProfileRecord>(Ꮡt, mutexProfileˢ, runtime.MutexProfile);
    testProfileRecordNullPadding<runtime.StackRecord>(Ꮡt, goroutineProfileˢ, runtime.GoroutineProfile);
    testProfileRecordNullPadding<runtime.BlockProfileRecord>(Ꮡt, blockProfileˢ, runtime.BlockProfile);
    testProfileRecordNullPadding(Ꮡt, memProfileInUseZeroTrueˢ, (slice<runtime.MemProfileRecord> p) => runtime.MemProfile(p, true));
    testProfileRecordNullPadding(Ꮡt, memProfileInUseZeroFalseˢ, (slice<runtime.MemProfileRecord> p) => runtime.MemProfile(p, false));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object noRecordsFoundˢ = (@string)"no records found"u8;

// Not testing ThreadCreateProfile because it is broken, see issue 6104.
internal static void testProfileRecordNullPadding<T>(ж<testing.T> Ꮡt, @string name, Func<slice<T>, (nint, bool)> fn)
    where T : /* runtime.StackRecord | runtime.MemProfileRecord | runtime.BlockProfileRecord */ new()
{
    ж<array<uintptr>> stack0(ж<T> sr) {
        var switchᴛ1 = ((any)sr.OrTypedNil());
        switch (switchᴛ1.type()) {
        case ж<runtime.StackRecord> tΔ1: {
            return tΔ1.of(runtime.StackRecord.ᏑStack0);
        }
        case ж<runtime.MemProfileRecord> tΔ1: {
            return tΔ1.of(runtime.MemProfileRecord.ᏑStack0);
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
    Ꮡt.Run(name, (ж<testing.T> tΔ2) => {
        slice<T> p = default!;
        while (ᐧ) {
            var (n, ok) = fn(p);
            if (ok) {
                p = p.slice(0, n);
                break;
            }
            p = new slice<T>(n * 2);
            foreach (var (i, _) in p) {
                var s0 = stack0ʗ1(Ꮡ(p, i));
                foreach (var (j, _) in s0.Value) {
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
                    tΔ2.Fatalf("record p[%d].Stack0 is not null padded: %+v"u8, i, sr);
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
