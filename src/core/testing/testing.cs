// testing.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics;
using go.testing_runtime;
using any = System.Object;
using ꓸꓸꓸany = System.Span<System.Object>;

// go2cs HAND-OWNED (whole file). This is the Phase-4 test host's public `testing` shim — the
// structural replacement stdLibConverter.go's skip list names, not a conversion of Go's testing.go.
// Of the ten hand-owned files in this directory this is the ONLY one whose path a converted
// testing.go would land on, so the marker here resolves a real collision: containsManualConversionMarker
// drops a marked file from the convert set and emits a testing.cs.auto review sibling beside it.
//
// Measured 2026-09-03, before the marker existed: a `-tests` run pointed at this package replaced
// this file's 685 lines with Go's converted testing.go (+2622/-560), after which the host's own
// TestHost.cs and TestExecution.cs failed CS0117/CS1929 on M.Runner, M.Run and T.Execution — the
// hand-owned members Go's testing.go does not declare. The marker is necessary but NOT sufficient:
// it saves this file while Go's benchmark.go/match.go/allocs.go/... still emit beside it into the
// same testing_package. The guard that actually holds is requireConvertibleTestTarget in
// testConversion.go, which refuses `-tests` on this package outright.
[module: go.GoManualConversion]

namespace go;

/// <summary>
/// Bootstrap implementation of the Go testing package used only by converted test projects.
/// </summary>
[GoPackage("testing")]
public static partial class testing_package
{
    public struct T
    {
        internal TestExecution? Execution;

        internal readonly TestExecution RequiredExecution =>
            Execution ?? throw new InvalidOperationException("testing.T is not attached to a running test");
    }

    public struct M
    {
        internal TestRunner? Runner;
    }

    /// <summary>
    /// The testing.TB interface — the common surface of T and B that test-support packages
    /// (internal/testenv is the driving consumer) accept as parameters. Declared with Go 1.23's
    /// full public member set so the compiled shape never drifts as more helpers convert.
    /// </summary>
    /// <remarks>
    /// T reaches this interface the way every converted type reaches a foreign one: the converter
    /// emits <c>[assembly: GoImplement&lt;testing_package.T, testing_package.TB&gt;(Pointer = true)]</c>
    /// into the consuming package, and go2cs-gen mints a <c>testing_TжTB</c> adapter over the
    /// <c>ж&lt;T&gt;</c> box that forwards every member below to the package-scope T implementation.
    /// So T needs no base list here, and there is nothing to "wire up" per suite — an earlier note
    /// on this type predicted that work, and the adapter had already made it unnecessary.
    ///
    /// Every member is therefore backed by the same TestExecution the T spelling uses: a
    /// <c>TB.Fatal</c> logs and calls FailNow, which throws TestAbortException and ends the test,
    /// exactly as <c>t.Fatal</c> does. The one asymmetry is B: an adapter built from a
    /// <c>*testing.B</c> forwards to the compile-only no-ops below, which is sound only because
    /// benchmarks are never registered or run (Phase 4D).
    /// </remarks>
    public interface TB
    {
        void Cleanup(Action cleanup);
        void Error(params ꓸꓸꓸany args);
        void Errorf(@string format, params ꓸꓸꓸany args);
        void Fail();
        void FailNow();
        bool Failed();
        void Fatal(params ꓸꓸꓸany args);
        void Fatalf(@string format, params ꓸꓸꓸany args);
        void Helper();
        void Log(params ꓸꓸꓸany args);
        void Logf(@string format, params ꓸꓸꓸany args);
        @string Name();
        void Setenv(@string key, @string value);
        // Go 1.24 additions. Both are declared on `common`, so T, B and F all acquire them and all
        // three must be spelled below or nothing satisfies this interface -- and the 57 committed
        // `[assembly: GoImplement<…, TB>]` registrations across the banked rows all mint their
        // forwarders from THIS member set at compile time, so an omission is a corpus-wide break
        // rather than a local one. Placed in Go's own declaration order.
        void Chdir(@string dir);
        void Skip(params ꓸꓸꓸany args);
        void SkipNow();
        void Skipf(@string format, params ꓸꓸꓸany args);
        bool Skipped();
        @string TempDir();
        context_package.Context Context();
    }

    /// <summary>
    /// Benchmark receiver surface. Top-level BenchmarkXxx DECLARATIONS remain disclosed-unsupported
    /// in the manifest (execution is deferred to Phase 4D) and are never registered with the host,
    /// but their converted BODIES still compile into the test assembly, so the members they
    /// reference must exist. Go's B embeds `common`, so that surface is Run, the timer/allocation
    /// reporters, AND the whole TB member set below; every one stays a safe non-throwing no-op
    /// answering the "nothing went wrong" value — a disclosed declaration is never invoked, so
    /// there is no run to time, fail, skip, name or clean up. N is the exception: it is set by
    /// <see cref="Benchmark"/>, which DOES drive a
    /// closure in-process (a converted Test can legitimately call testing.Benchmark itself —
    /// unicode's TestCalibrate does), so a b.N loop inside such a closure iterates the measured
    /// count rather than zero times.
    /// </summary>
    public struct B
    {
        public nint N;

        // Go 1.24's b.Loop() cursor. Internal, not part of Go's B: Go's Loop keeps its state in
        // unexported fields of the same struct, and this is that state under a name the converted
        // corpus cannot collide with. Benchmark() mints a fresh B per round, so it starts at zero
        // for each one without the driver having to reset it.
        internal nint LoopIteration;
    }

    /// <summary>
    /// The result of one <see cref="Benchmark"/> run — the subset of Go's testing.BenchmarkResult a
    /// converted Test function needs when it drives an in-process benchmark (unicode's TestCalibrate
    /// reads NsPerOp()). N is the final iteration count Benchmark settled on. Go's companion field is
    /// T time.Duration; the shim stays time-package-free (see the file's Sprint remark on why no
    /// second stdlib tree is dragged in), so the elapsed wall-clock time is held here as Nanoseconds,
    /// the int64 nanosecond form of that Duration.
    /// </summary>
    public struct BenchmarkResult
    {
        public nint N;
        public long Nanoseconds;

        /// <summary>
        /// Average nanoseconds per iteration — like Go's BenchmarkResult.NsPerOp(). Zero iterations
        /// yields 0 (Go returns 0 rather than dividing when N is non-positive).
        /// </summary>
        public readonly long NsPerOp()
        {
            if (N <= 0)
                return 0L;
            return Nanoseconds / (long)N;
        }
    }

    /// <summary>
    /// Runs benchmark closure <paramref name="f"/> in-process and reports the measurement — a
    /// minimal stand-in for Go's testing.Benchmark. Like Go, it grows N geometrically (starting at
    /// 1) until the run spends a target time budget or hits an iteration ceiling, times the final
    /// run, and returns a <see cref="BenchmarkResult"/>. This is the ONLY path that executes a
    /// benchmark body: it exists for converted Test functions that call testing.Benchmark directly
    /// (unicode's TestCalibrate). Top-level BenchmarkXxx declarations are still never registered or
    /// run (they are disclosed in the manifest), so adding this changes no existing test's behavior.
    /// </summary>
    /// <remarks>
    /// Go's default benchmark time is 1s; the shim uses a smaller budget so an in-process benchmark
    /// completes quickly. The N-growth mirrors Go's predictNextN shape (scale by target/observed,
    /// grow by at most 100x and at least +20%, clamp to the ceiling). Only N and the run's wall time
    /// are measured — the B timer/allocation reporters are the no-op receiver members above, exactly
    /// as for a disclosed benchmark, so a closure that calls them still behaves consistently.
    /// </remarks>
    public static BenchmarkResult Benchmark(Action<ж<B>> f)
    {
        ArgumentNullException.ThrowIfNull(f);

        const long targetNanoseconds = 100L * 1000L * 1000L;   // 100ms budget per benchmark
        const nint maxIterations = 1_000_000_000;              // Go's benchmark N ceiling (1e9)

        nint n = 1;
        long elapsedNanoseconds = 0L;

        while (true)
        {
            ref B b = ref builtin.heap<B>(out ж<B> box);
            b.N = n;

            Stopwatch timer = Stopwatch.StartNew();
            f(box);
            timer.Stop();
            elapsedNanoseconds = (long)timer.Elapsed.TotalNanoseconds;

            if (elapsedNanoseconds >= targetNanoseconds || n >= maxIterations)
                break;

            // Predict the next N: scale by how far under budget the last run was, bounded to a
            // 100x jump above and a +20% floor below, and never past the ceiling (Go's predictNextN).
            nint next;
            if (elapsedNanoseconds <= 0L)
                next = n * 100;
            else
            {
                next = (nint)((double)n * targetNanoseconds / elapsedNanoseconds);
                next = Math.Min(next, n * 100);
                next = Math.Max(next, n + n / 5);
            }

            n = Math.Min(Math.Max(next, n + 1), maxIterations);
        }

        return new BenchmarkResult { N = n, Nanoseconds = elapsedNanoseconds };
    }

    /// <summary>
    /// Parallel-benchmark body handle — COMPILE-ONLY, for the same reason as B above. A Go
    /// parallel benchmark is written as b.RunParallel(func(pb *testing.PB) { for pb.Next() {…} }),
    /// so both the type and its Next method must exist for the converted body to compile. Next
    /// reports false so the loop it drives would iterate zero times; RunParallel never invokes
    /// the body at all, since benchmarks are not executed by the bootstrap host.
    /// </summary>
    public struct PB
    {
    }

    /// <summary>
    /// Fuzz-target receiver surface — COMPILE-ONLY, for the same reason as B above. Fuzz
    /// declarations are disclosed-unsupported in the manifest (execution is deferred to Phase 4D)
    /// and never registered with the host, but their converted BODIES still compile into the test
    /// assembly, so the members they reference must exist. There is no fuzzing engine, so Fuzz
    /// never invokes the target and Add never records a seed: with no run to perform there is
    /// nothing to time, seed or fail, and every member is a safe non-throwing no-op. The member
    /// set is Go 1.23's full public surface for *testing.F — the TB members it inherits from the
    /// embedded common, plus its own Add and Fuzz — so the compiled shape never drifts as more
    /// fuzz targets convert (the same rule TB above is declared under).
    /// </summary>
    public struct F
    {
    }

    public static void Error(this ref T t, params ꓸꓸꓸany args)
    {
        TestExecution execution = t.RequiredExecution;
        execution.Log(Sprint(args));
        execution.Fail();
    }

    public static void Errorf(this ref T t, @string format, params ꓸꓸꓸany args)
    {
        TestExecution execution = t.RequiredExecution;
        execution.Log(Sprintf(format, args));
        execution.Fail();
    }

    /// <summary>
    /// Reports the time at which the test binary will have exceeded its package deadline
    /// (<c>-timeout</c>), matching Go's <c>func (t *T) Deadline() (deadline time.Time, ok bool)</c>.
    /// <c>ok</c> is false when no deadline is in effect.
    /// </summary>
    /// <remarks>
    /// The FIRST member of this host's surface that needs a converted standard-library type. Go's
    /// callers use the result as a real <c>time.Time</c> — <c>net</c>'s
    /// <c>deadline.Add(-time.Until(deadline)/10)</c> — so no primitive or golib stand-in satisfies it,
    /// and while the converted stdlib lived in a second tree there was no <c>time</c> this host could
    /// name without dragging two <c>go.time_package</c> declarations into one build (BOARD:
    /// "testing.T.Deadline needs a type core/testing cannot name"). One tree, one <c>time</c>, and the
    /// blocker is simply gone: testing.csproj references core\time like any other consumer.
    ///
    /// Note this deliberately does NOT join <see cref="TB"/> — Go's testing.TB has no Deadline either.
    /// </remarks>
    [GoRecv] public static (time_package.Time deadline, bool ok) Deadline(this ref T t)
    {
        // Touch the execution so an unattached T fails the same way every other member does.
        _ = t.RequiredExecution;

        if (TestHost.PackageDeadlineUtc is not { } deadline)
            return (default!, false);

        // Whole seconds + nanosecond remainder, the shape time.Unix takes. DateTime ticks are
        // 100 ns, so the remainder scales by 100 exactly — no precision is invented.
        long ticks = (deadline - DateTime.UnixEpoch).Ticks;
        long seconds = Math.DivRem(ticks, TimeSpan.TicksPerSecond, out long remainder);

        if (remainder < 0)
        {
            seconds--;
            remainder += TimeSpan.TicksPerSecond;
        }

        return (time_package.Unix(seconds, remainder * 100L), true);
    }

    [GoRecv] public static void Fail(this ref T t) => t.RequiredExecution.Fail();

    [GoRecv] public static void FailNow(this ref T t) => t.RequiredExecution.FailNow();

    [GoRecv] public static bool Failed(this ref T t) => t.RequiredExecution.Failed;

    public static void Fatal(this ref T t, params ꓸꓸꓸany args)
    {
        TestExecution execution = t.RequiredExecution;
        execution.Log(Sprint(args));
        execution.FailNow();
    }

    public static void Fatalf(this ref T t, @string format, params ꓸꓸꓸany args)
    {
        TestExecution execution = t.RequiredExecution;
        execution.Log(Sprintf(format, args));
        execution.FailNow();
    }

    public static void Log(this ref T t, params ꓸꓸꓸany args) =>
        t.RequiredExecution.Log(Sprint(args));

    public static void Logf(this ref T t, @string format, params ꓸꓸꓸany args) =>
        t.RequiredExecution.Log(Sprintf(format, args));

    [GoRecv] public static void Helper(this ref T t) => t.RequiredExecution.Helper();

    [GoRecv] public static @string Name(this ref T t) => t.RequiredExecution.Name;

    [GoRecv] public static void Cleanup(this ref T t, Action cleanup) =>
        t.RequiredExecution.Cleanup(cleanup);

    [GoRecv] public static bool Run(this ref T t, @string name, Action<ж<T>> test) =>
        t.RequiredExecution.Run(name.ToString(), test);

    public static void Skip(this ref T t, params ꓸꓸꓸany args)
    {
        TestExecution execution = t.RequiredExecution;
        execution.Log(Sprint(args));
        execution.SkipNow();
    }

    public static void Skipf(this ref T t, @string format, params ꓸꓸꓸany args)
    {
        TestExecution execution = t.RequiredExecution;
        execution.Log(Sprintf(format, args));
        execution.SkipNow();
    }

    [GoRecv] public static void SkipNow(this ref T t) => t.RequiredExecution.SkipNow();

    [GoRecv] public static bool Skipped(this ref T t) => t.RequiredExecution.Skipped;

    [GoRecv] public static @string TempDir(this ref T t) => t.RequiredExecution.TempDir();

    [GoRecv] public static void Setenv(this ref T t, @string key, @string value) =>
        t.RequiredExecution.Setenv(key.ToString(), value.ToString());

    // Go 1.24: both are real on T, backed by the same TestExecution every other T member uses.
    [GoRecv] public static void Chdir(this ref T t, @string dir) =>
        t.RequiredExecution.Chdir(dir.ToString());

    [GoRecv] public static context_package.Context Context(this ref T t) =>
        t.RequiredExecution.Context();

    [GoRecv] public static void Parallel(this ref T t) => t.RequiredExecution.Parallel();

    // RecvGenerator intentionally handles ordinary receiver signatures. C# params
    // collections use Span<T>, which is ref-like and needs explicit pointer receiver
    // overloads for converted closures that retain *testing.T rather than a ref local.
    public static void Error(this ж<T> t, params ꓸꓸꓸany args) => Error(ref t.Value, args);

    public static void Errorf(this ж<T> t, @string format, params ꓸꓸꓸany args) => Errorf(ref t.Value, format, args);

    public static void Fatal(this ж<T> t, params ꓸꓸꓸany args) => Fatal(ref t.Value, args);

    public static void Fatalf(this ж<T> t, @string format, params ꓸꓸꓸany args) => Fatalf(ref t.Value, format, args);

    public static void Log(this ж<T> t, params ꓸꓸꓸany args) => Log(ref t.Value, args);

    public static void Logf(this ж<T> t, @string format, params ꓸꓸꓸany args) => Logf(ref t.Value, format, args);

    public static void Skip(this ж<T> t, params ꓸꓸꓸany args) => Skip(ref t.Value, args);

    public static void Skipf(this ж<T> t, @string format, params ꓸꓸꓸany args) => Skipf(ref t.Value, format, args);

    [GoRecv] public static nint Run(this ref M m)
    {
        TestRunner runner = m.Runner ?? throw new InvalidOperationException("testing.M is not attached to a test registry");

        // Go's M.Run opens with exactly this, under exactly this reasoning -- its comment reads
        // "TestMain may have already called flag.Parse.":
        //
        //     if !flag.Parsed() { flag.Parse() }
        //
        // The POSITION is the contract, not the call. A package with a TestMain sets flag.Usage and
        // parses INSIDE it, before reaching m.Run; a package without one is run by a generated main
        // that calls m.Run directly, and this is the only thing that populates its custom flags.
        // Parsing any earlier takes the decision away from a TestMain written to make it -- which is
        // what happened when this lived in TestHost.Run: crypto/tls's TestMain installs a flag.Usage
        // that exits 89 for the bogo runner, and an earlier parse resolved the same arguments through
        // the DEFAULT Usage and exited 2, so the override never applied and the wall was measured on
        // an accident rather than on Go's contract (i9's bogo re-run, rooted 2026-08-29).
        TestFlagBridge.Parse();

        return runner.RunAll();
    }

    // Compile-only B surface (see struct B above) — never executed, never throwing. The
    // RecvGenerator supplies the ж<B> overloads, as for every ordinary [GoRecv] signature.
    [GoRecv] public static bool Run(this ref B b, @string name, Action<ж<B>> benchmark) => true;

    [GoRecv] public static void ReportAllocs(this ref B b) { }

    [GoRecv] public static void ResetTimer(this ref B b) { }

    [GoRecv] public static void SetBytes(this ref B b, long n) { }

    [GoRecv] public static void SetParallelism(this ref B b, nint p) { }

    [GoRecv] public static void ReportMetric(this ref B b, double n, @string unit) { }

    [GoRecv] public static void StartTimer(this ref B b) { }

    [GoRecv] public static void StopTimer(this ref B b) { }

    // Parallel benchmarks (see struct PB above): RunParallel does not invoke the body — nothing
    // runs, so nothing is scheduled across goroutines — and PB.Next reports "no more work" so the
    // body's for pb.Next() loop would terminate immediately if it ever were invoked.
    [GoRecv] public static void RunParallel(this ref B b, Action<ж<PB>> body) { }

    [GoRecv] public static bool Next(this ref PB pb) => false;

    // Go's testing.B embeds `common`, so a benchmark body may call ANY of the TB members — not
    // just the timer/allocation reporters above. The whole embedded surface is declared here for
    // the same compile-only reason (internal/zstd's benchmarks call Cleanup/Error/Log/…), and
    // every member is a safe non-throwing no-op answering the "nothing went wrong" value: a
    // benchmark is never registered or run, so there is nothing to fail, skip, name or clean up.
    [GoRecv] public static void Cleanup(this ref B b, Action cleanup) { }

    [GoRecv] public static void Fail(this ref B b) { }

    [GoRecv] public static void FailNow(this ref B b) { }

    [GoRecv] public static bool Failed(this ref B b) => false;

    [GoRecv] public static void Helper(this ref B b) { }

    [GoRecv] public static @string Name(this ref B b) => ""u8;

    [GoRecv] public static void Setenv(this ref B b, @string key, @string value) { }

    [GoRecv] public static void SkipNow(this ref B b) { }

    [GoRecv] public static bool Skipped(this ref B b) => false;

    [GoRecv] public static @string TempDir(this ref B b) => ""u8;

    // Go 1.24 TB additions, on the same compile-only footing as the members above.
    [GoRecv] public static void Chdir(this ref B b, @string dir) { }

    // Background() rather than default!: a benchmark body that reaches this and passes the result
    // to anything expecting a usable context gets one that is merely never canceled, instead of a
    // nil dereference. The "nothing went wrong" value for a context is a live one.
    [GoRecv] public static context_package.Context Context(this ref B b) => context_package.Background();

    /// <summary>
    /// Go 1.24's <c>B.Loop</c> — <c>for b.Loop() { … }</c>, the replacement for <c>for i := 0; i &lt; b.N; i++</c>.
    /// </summary>
    /// <remarks>
    /// <b>This one is NOT a no-op, and it is the one member of this block where a no-op would be
    /// wrong in both directions.</b> The block above is sound because benchmarks are never
    /// registered or run — with <c>N</c> as the standing exception, since <see cref="Benchmark"/>
    /// DOES drive a closure in process (unicode's TestCalibrate is a converted Test that calls it),
    /// so a <c>b.N</c> loop inside such a closure iterates a real count. <c>Loop</c> is reached the
    /// same way and therefore inherits the same exception:
    /// <list type="bullet">
    /// <item>returning <c>true</c> unconditionally HANGS the driver — an unbounded loop;</item>
    /// <item>returning <c>false</c> unconditionally runs the body ZERO times and hands back a
    /// timing for work that never happened, which is worse than an error because it is silent.</item>
    /// </list>
    /// So Loop rides the same <c>N</c> the driver sets: true exactly N times, then false. Go's own
    /// Loop instead ramps INTERNALLY against a time budget and explicitly sets <c>b.N = 0</c> to
    /// avoid confusion (benchmark.go loopSlowPath); here the ramp already exists one level up in
    /// <see cref="Benchmark"/> — Go's predictNextN, the 100 ms budget and the 1e9 ceiling — and it
    /// mints a fresh B per round, so riding N ramps ACROSS closure calls where Go ramps within one.
    /// Different mechanism, same observable: a BenchmarkResult whose N is the iteration count. A
    /// never-driven benchmark sits at N=0 — one evaluation, false, no iterations, no hang.
    /// <para>
    /// ⚠ <b>There is deliberately NO cursor reset, and an earlier version of this method had one.</b>
    /// It was written so that a second <c>for b.Loop()</c> range in the same body would run, which
    /// seemed obviously right and is <b>a behaviour Go forbids</b>: measured against the go1.24.13
    /// oracle (arm12_loop, outside the repo), Go FATALS on the second range with
    /// <c>"B.Loop called with timer stopped"</c> — loopSlowPath's first consistency check, because
    /// the completed first range called StopTimer. Resetting would have made the converted side
    /// silently execute a loop body Go never runs. Without the reset the cursor stays at N, the
    /// second range answers false immediately, and neither side does that work. The host has no
    /// meaningful Fatal on a B whose members are compile-only no-ops, so matching Go's REFUSAL
    /// exactly is not available; not doing the work is the part that matters.
    /// </para>
    /// </remarks>
    [GoRecv] public static bool Loop(this ref B b) {
        if (b.LoopIteration >= b.N) {
            return false;
        }
        b.LoopIteration++;
        return true;
    }

    // Params-taking B members need the same explicit ж<B> overloads as T's above (params
    // collections are ref-like Spans the RecvGenerator does not synthesize overloads for).
    // Failure reporting is a no-op: benchmark bodies never execute, so there is no run to fail.
    public static void Error(this ref B b, params ꓸꓸꓸany args) { }

    public static void Errorf(this ref B b, @string format, params ꓸꓸꓸany args) { }

    public static void Fatal(this ref B b, params ꓸꓸꓸany args) { }

    public static void Fatalf(this ref B b, @string format, params ꓸꓸꓸany args) { }

    public static void Log(this ref B b, params ꓸꓸꓸany args) { }

    public static void Logf(this ref B b, @string format, params ꓸꓸꓸany args) { }

    public static void Skip(this ref B b, params ꓸꓸꓸany args) { }

    public static void Skipf(this ref B b, @string format, params ꓸꓸꓸany args) { }

    public static void Error(this ж<B> b, params ꓸꓸꓸany args) => Error(ref b.Value, args);

    public static void Errorf(this ж<B> b, @string format, params ꓸꓸꓸany args) => Errorf(ref b.Value, format, args);

    public static void Fatal(this ж<B> b, params ꓸꓸꓸany args) => Fatal(ref b.Value, args);

    public static void Fatalf(this ж<B> b, @string format, params ꓸꓸꓸany args) => Fatalf(ref b.Value, format, args);

    public static void Log(this ж<B> b, params ꓸꓸꓸany args) => Log(ref b.Value, args);

    public static void Logf(this ж<B> b, @string format, params ꓸꓸꓸany args) => Logf(ref b.Value, format, args);

    public static void Skip(this ж<B> b, params ꓸꓸꓸany args) => Skip(ref b.Value, args);

    public static void Skipf(this ж<B> b, @string format, params ꓸꓸꓸany args) => Skipf(ref b.Value, format, args);

    // Compile-only F surface (see struct F above) — never executed, never throwing. Fuzz takes a
    // System.Delegate because a Go fuzz target's signature is arbitrary (*testing.T followed by
    // the fuzzed argument types, e.g. func(*testing.T, uint, uint, …)); the converted body is an
    // explicitly-typed lambda, so C# infers its natural Action<…> and converts it to Delegate.
    // The target is never invoked and the seed corpus Add collects is discarded — there is no
    // fuzzing engine to consume either.
    [GoRecv] public static void Fuzz(this ref F f, Delegate target) { }

    [GoRecv] public static void Fail(this ref F f) { }

    [GoRecv] public static void FailNow(this ref F f) { }

    [GoRecv] public static bool Failed(this ref F f) => false;

    [GoRecv] public static void Helper(this ref F f) { }

    [GoRecv] public static @string Name(this ref F f) => ""u8;

    [GoRecv] public static void SkipNow(this ref F f) { }

    [GoRecv] public static bool Skipped(this ref F f) => false;

    [GoRecv] public static void Cleanup(this ref F f, Action cleanup) { }

    [GoRecv] public static void Setenv(this ref F f, @string key, @string value) { }

    [GoRecv] public static @string TempDir(this ref F f) => ""u8;

    // Go 1.24 TB additions, same compile-only footing as F's other members. Note Go's own Chdir and
    // Context both begin with checkFuzzFn, which PANICS when called from inside a fuzz target's
    // function -- so on the F path Go's answer is a panic, not a value. A no-op is the deliberate
    // choice here rather than a reproduction of that: fuzz targets are not run at all, so the panic
    // would only ever fire from a declaration that compiled and never executed.
    [GoRecv] public static void Chdir(this ref F f, @string dir) { }

    [GoRecv] public static context_package.Context Context(this ref F f) => context_package.Background();

    // Params-taking F members need the same explicit ж<F> overloads as T's and B's above (params
    // collections are ref-like Spans the RecvGenerator does not synthesize overloads for).
    public static void Add(this ref F f, params ꓸꓸꓸany args) { }

    public static void Error(this ref F f, params ꓸꓸꓸany args) { }

    public static void Errorf(this ref F f, @string format, params ꓸꓸꓸany args) { }

    public static void Fatal(this ref F f, params ꓸꓸꓸany args) { }

    public static void Fatalf(this ref F f, @string format, params ꓸꓸꓸany args) { }

    public static void Log(this ref F f, params ꓸꓸꓸany args) { }

    public static void Logf(this ref F f, @string format, params ꓸꓸꓸany args) { }

    public static void Skip(this ref F f, params ꓸꓸꓸany args) { }

    public static void Skipf(this ref F f, @string format, params ꓸꓸꓸany args) { }

    public static void Add(this ж<F> f, params ꓸꓸꓸany args) => Add(ref f.Value, args);

    public static void Error(this ж<F> f, params ꓸꓸꓸany args) => Error(ref f.Value, args);

    public static void Errorf(this ж<F> f, @string format, params ꓸꓸꓸany args) => Errorf(ref f.Value, format, args);

    public static void Fatal(this ж<F> f, params ꓸꓸꓸany args) => Fatal(ref f.Value, args);

    public static void Fatalf(this ж<F> f, @string format, params ꓸꓸꓸany args) => Fatalf(ref f.Value, format, args);

    public static void Log(this ж<F> f, params ꓸꓸꓸany args) => Log(ref f.Value, args);

    public static void Logf(this ж<F> f, @string format, params ꓸꓸꓸany args) => Logf(ref f.Value, format, args);

    public static void Skip(this ж<F> f, params ꓸꓸꓸany args) => Skip(ref f.Value, args);

    public static void Skipf(this ж<F> f, @string format, params ꓸꓸꓸany args) => Skipf(ref f.Value, format, args);

    /// <summary>
    /// Reports the average allocation cost per run of f — like Go's testing.AllocsPerRun, which
    /// counts MALLOCS (a <c>runtime.MemStats.Mallocs</c> delta). The CLR publishes no malloc
    /// counter, so the count comes from go2cs's OWN runtime instead of the platform's:
    /// <see cref="AllocationCounter"/> charges golib's allocation sites, which is the structural
    /// mirror of what Go's Mallocs is — a counter the runtime keeps at its own sites, not a
    /// facility the platform provides.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Bytes are still measured, as the cross-check that keeps the count honest.</b> The counted
    /// set cannot be total (see <see cref="AllocationCounter"/>'s coverage statement: allocations
    /// the C# compiler emits in converted code and BCL internals are outside it), so the count is
    /// never trusted on its own:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>
    ///     zero BYTES ⟹ zero allocations, exactly, in both units — the dominant assert-zero stdlib
    ///     tests are faithful and their output is unchanged;
    ///   </description></item>
    ///   <item><description>
    ///     nonzero bytes with a nonzero count ⟹ the COUNT is reported, floored at 1;
    ///   </description></item>
    ///   <item><description>
    ///     nonzero bytes with a ZERO count ⟹ allocations happened that the counter did not see, so
    ///     the byte-derived figure is reported instead. Reporting the zero would be a FALSE PASS,
    ///     which is worse than the byte figure it would replace.
    ///   </description></item>
    /// </list>
    /// <para>
    /// The floor at 1 on any nonzero-byte result is inherited from the byte-only shim and kept for
    /// the same reason: amortized sub-one-per-run allocation must never masquerade as the exact-zero
    /// case. It also makes the change MONOTONE — a counted object always costs at least the CLR's
    /// 24-byte object minimum, so the reported value can only fall, never rise, relative to the byte
    /// figure. No test that passed on bytes can fail on the count.
    /// </para>
    /// <para>
    /// Either way a nonzero return NOTES what was measured on the running test (see
    /// <see cref="TestExecution.NoteMeasurementUnitOnce"/>), carrying both numbers, because the
    /// value is about to be rendered by Go's own <c>"got %v allocs"</c> format and a reader — or a
    /// later disclosure decision — has to be able to see what it is.
    /// </para>
    /// <para>
    /// GC.GetAllocatedBytesForCurrentThread is precise and inherently scoped to this thread,
    /// which stands in for Go's GOMAXPROCS(1) pinning: other threads' allocations never pollute
    /// the measurement. Like Go's, f is assumed single-threaded — allocations made by goroutines
    /// f spawns run on other threads (converted goroutines share the thread pool) and are not
    /// observed. See docs/ConversionStrategies-Reference.md "Manually-Converted Declarations".
    /// </para>
    /// <para>
    /// That a COUNT is unavailable is measured, not assumed (r56d, net9.0/9.0.18, x64):
    /// </para>
    /// <list type="bullet">
    ///   <item><description>
    ///     the whole public GC surface exposes byte totals only — GetAllocatedBytesForCurrentThread,
    ///     GetTotalAllocatedBytes — plus CollectionCount/FinalizationPendingCount/PinnedObjectsCount,
    ///     none of which counts objects allocated;
    ///   </description></item>
    ///   <item><description>
    ///     GetAllocatedBytesForCurrentThread is EXACT (40.000 B/object over 1, 10, 1 000 and
    ///     100 000 allocations of a 40-byte type) but cannot separate count from size: one
    ///     byte[40000] and 1 000 40-byte objects both read ≈40 000 B;
    ///   </description></item>
    ///   <item><description>
    ///     GCAllocationTick is a byte-threshold SAMPLE — 378 events for 1 000 000 allocations,
    ///     one per ≈105 820 B — so it measures bytes too, more coarsely;
    ///   </description></item>
    ///   <item><description>
    ///     GCSampledObjectAllocation, whose ObjectCountForTypeSample payload WOULD be a count,
    ///     raises zero events through an in-process EventListener in every configuration tried
    ///     (High 0x200000, Low 0x2000000, both, and all keywords 0xFFFFFFFFFFFF, at Verbose and
    ///     Informational), with the GC keyword's own tick count as the live positive control;
    ///   </description></item>
    ///   <item><description>
    ///     System.Runtime's EventCounters publish 27 counters whose only allocation-shaped member
    ///     is <c>alloc-rate</c> — bytes per interval;
    ///   </description></item>
    ///   <item><description>
    ///     and runtime events reach an in-process listener ASYNCHRONOUSLY — zero visible
    ///     immediately after the measured loop, settling ≈117 ms later — so no event-derived
    ///     figure could be returned by a synchronous call like this one anyway.
    ///   </description></item>
    /// </list>
    /// <para>
    /// That survey is what makes the count come from go2cs's own runtime instead. It is taken now
    /// (r58a) rather than in r56d because a count that silently omits allocation sites is worse
    /// than an honest byte figure: what the census bought is not totality — which is unreachable,
    /// the compiler emitting allocations in converted code that golib never sees — but a STATED
    /// coverage boundary plus the byte cross-check below, so the number can be trusted exactly as
    /// far as it is true and no further.
    /// </para>
    /// </remarks>
    public static double AllocsPerRun(nint runs, Action f)
    {
        // Warmup run outside the measurement window (Go does the same): first-call lazy
        // initialization — and JIT compilation here — must not count against f.
        f();

        long startBytes = GC.GetAllocatedBytesForCurrentThread();
        long startCount = AllocationCounter.CurrentThreadCount;

        for (nint i = 0; i < runs; i++)
            f();

        // Read the COUNT first: the byte read itself allocates nothing, but reading in this order
        // keeps the two windows nested rather than overlapping.
        long counted = AllocationCounter.CurrentThreadCount - startCount;
        long allocated = GC.GetAllocatedBytesForCurrentThread() - startBytes;

        // Zero bytes is zero allocations, exactly, in both units. Returning here keeps every
        // assert-zero test — the dominant stdlib shape — byte-identical in output to the byte-only
        // shim, and is the reason no banked row's terminal text moves.
        if (allocated == 0L)
            return 0.0D;

        // Something allocated. If the counter saw none of it, the census missed this path
        // (compiler-emitted closures in converted code, or a BCL internal)
        // and the count is not usable: reporting its zero would turn a real allocation into a
        // passing assert. Fall back to the byte figure, which at least cannot understate.
        bool countUsable = AllocationCounter.Enabled && counted > 0L;

        // Integer division like Go's (its comment: "do the division as integers"); runs == 0
        // divides by zero — a runtime-error panic exactly where Go's own division panics. The floor
        // at 1 is what stops amortized sub-one-per-run allocation from masquerading as exact zero.
        double average = Math.Max(1L, (countUsable ? counted : allocated) / runs);

        // The note carries BOTH numbers on every nonzero result, so the unit is never in doubt and
        // a disclosure decision can see the count and the byte total together. Only the nonzero
        // case needs it: at zero the units agree and the test's output must not move.
        TestExecution.Current?.NoteMeasurementUnitOnce(countUsable
            ? $"go2cs: testing.AllocsPerRun counted {counted:N0} go2cs-runtime object allocations " +
              $"({allocated:N0} bytes) over {runs:N0} run(s) — the figure reported above is an " +
              "allocation COUNT per run, from go2cs's own runtime counter (golib's allocation " +
              "sites), which is the structural mirror of Go's runtime.MemStats.Mallocs. The CLR " +
              "publishes no in-process malloc counter (see the measured survey on " +
              "testing.AllocsPerRun). The count covers golib's sites only: allocations the C# " +
              "compiler emits in converted code (closures, params arrays, interface boxing) and " +
              "BCL internals are outside it, so this is a LOWER BOUND on the true object count."
            : $"go2cs: testing.AllocsPerRun measured {allocated:N0} allocated BYTES over {runs:N0} " +
              "run(s) — the figure reported above is BYTES PER RUN, not an allocation count. The " +
              "go2cs runtime allocation counter charged none of it, so every object on this path " +
              "was allocated outside golib (compiler-emitted closures in converted code, or a " +
              "BCL internal) and no count is available; bytes is what " +
              "is measurable here. Zero is exact in both units; a nonzero value is not comparable " +
              "to a Go malloc count.");

        return average;
    }

    /// <summary>
    /// Reports what the test coverage mode is set to — like Go's testing.CoverMode(). Coverage
    /// instrumentation does not exist in the shim, so the mode is always "" — exactly Go's value
    /// when the binary is built without -cover, sending callers down the coverage-off path.
    /// </summary>
    public static @string CoverMode() => "";

    /// <summary>
    /// Reports whether the -short flag was set — like Go's testing.Short() (default false).
    /// </summary>
    public static bool Short() => TestHost.ShortMode;

    /// <summary>
    /// Reports whether the program is a test binary — like Go's testing.Testing(). The shim is
    /// referenced ONLY by converted test projects (the go2cs test host is the program), so every
    /// reachable caller is a test binary.
    /// </summary>
    public static bool Testing() => true;

    /// <summary>
    /// Reports whether the -v flag was set — like Go's testing.Verbose() (default false).
    /// </summary>
    public static bool Verbose() => TestHost.VerboseMode;

    // Formatting IS the fmt package's, exactly as Go's own testing composes it: Log is
    // fmt.Sprintln and Logf is fmt.Sprintf. The earlier fmt-free rule and the measurements
    // that retired it (fmt's own suite self-reporting at 63/1/0 under this reference, the
    // graph still acyclic on all three targets, and the closure and build-time cost that was
    // accepted for it) are recorded in TestFormat's remarks.
    private static string Sprint(ReadOnlySpan<any> args) => TestFormat.Sprint(args);

    private static string Sprintf(@string format, ReadOnlySpan<any> args) => TestFormat.Sprintf(format, args);
}
