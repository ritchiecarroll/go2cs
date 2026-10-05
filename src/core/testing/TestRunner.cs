// TestRunner.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using go.golib;

// go2cs HAND-OWNED (whole file) — part of the Phase-4 test host, a structural replacement for Go's
// testing package rather than a conversion of it (the rationale and the measured clobber are in
// testing.cs). No converted source emits at this path, so this marker declares ownership rather than
// resolving a collision; the mechanical guards are the -stdlib skip list (isNonConvertedStdLibPackage)
// and testConversion.go's -tests refusal (requireConvertibleTestTarget).
[module: go.GoManualConversion]

namespace go.testing_runtime;
/// <summary>
/// Executes a <see cref="TestRegistry"/>: selection and ordering, <c>-count</c> repetition,
/// <c>-shuffle</c>, the serial/parallel interleaving Go performs, and the failure accounting that
/// becomes the process exit code.
/// </summary>
/// <remarks>
/// <para>
/// The interesting part is the parallel handshake, and it is Go's semantics rather than a scheduling
/// choice. A Go test runs SERIALLY until it calls <c>t.Parallel()</c>, at which point it pauses and
/// its parent proceeds; the paused tests are released together once every serial test has finished.
/// So each test is started and then awaited on whichever comes first — completion, or reaching
/// <c>t.Parallel()</c> — and reaching parallel merely parks it on a list. The release happens per
/// <c>-count</c> ITERATION, not once at the end of the whole run, because that is where Go releases
/// them: iterations interleave serial and parallel phases rather than batching every iteration's
/// parallel tests to the very end.
/// </para>
/// <para>
/// Failures are counted in two separate buckets. A test that FAILED is a result the differential
/// oracle compares against Go's; a test that failed for an INFRASTRUCTURE reason (the host could not
/// run it correctly) is not a Go-comparable verdict at all, and conflating the two would let a host
/// defect be recorded as a genuine behavioral difference. Both make the exit code non-zero.
/// </para>
/// </remarks>
public sealed class TestRunner
{
    private readonly TestRegistry m_registry;
    private readonly TestOptions m_options;
    private readonly TestReporter m_reporter;
    private readonly SemaphoreSlim m_parallelLimiter;
    private int m_failures;
    private int m_infrastructureFailures;

    internal TestRunner(TestRegistry registry, TestOptions options, TestReporter reporter, string workingDirectory, string runRoot)
    {
        m_registry = registry;
        m_options = options;
        m_reporter = reporter;
        m_parallelLimiter = new SemaphoreSlim(options.Parallel);
        WorkingDirectory = workingDirectory;
        RunRoot = runRoot;
    }

    public bool HasRun { get; private set; }

    public nint ExitCode => m_listStatus ?? (m_failures == 0 && m_infrastructureFailures == 0 ? 0 : 1);

    // Set only by ListTests: a listing run's status is the listing's, never a test verdict's.
    private int? m_listStatus;

    internal string Package => m_registry.Package;

    // -test.list was given: M.Run lists instead of running.
    internal bool Listing => m_options.ListPattern.Length > 0;

    // -test.v (or --json, which implies it, as `go test -json` passes -test.v): Go's chatty output.
    internal bool Verbose => m_options.Verbose;

    internal string WorkingDirectory { get; }

    /// <summary>
    /// Gets the run sandbox's root — the private directory holding the package's whole staged
    /// ancestry. This is where per-test temp directories live, deliberately OUTSIDE the staged
    /// <c>src</c> tree; see <see cref="TestExecution.TempDir"/>.
    /// </summary>
    internal string RunRoot { get; }

    public nint RunAll()
    {
        HasRun = true;
        Stopwatch packageTimer = Stopwatch.StartNew();
        m_reporter.ReportPackage("run", output: m_options.ShuffleSeed is int reportedSeed ? $"shuffle seed: {reportedSeed}" : null);

        for (int count = 0; count < m_options.Count; count++)
        {
            // GO'S ORDER, not the names' (D4). cmd/go's loadTestFuncs (load/test.go) lists the package's
            // internal _test files, then its external (package x_test) files, each list by file name (go/build
            // reads the directory sorted by name), and a file's tests in declaration order. An order-dependent
            // suite reads differently in any other order: google/uuid's TestRandPool leaves the package's random
            // source exhausted, and Go runs it AFTER TestRandomUUID. The name stays as the last key, so a
            // registration without a Go source position keeps the name order it always had.
            List<RegisteredTest> tests = InGoOrder(m_registry.Tests.Where(test => m_options.ShouldRun(test.Name))).ToList();

            if (m_options.ShuffleSeed is int seed)
                Shuffle(tests, unchecked(seed + count));

            // Go releases top-level parallel tests at the end of EACH -count iteration, so
            // iterations interleave serial and parallel phases rather than batching every
            // iteration's parallel tests to the very end of the run.
            List<TestExecution> parallel = [];

            foreach (RegisteredTest test in tests)
            {
                // A test may lower the process's descriptor limit and leave it lowered, as Go's is free
                // to (syscall's TestPrlimitFileLimit): the next test starts from the run's own limit,
                // because the CLR cannot start a thread inside Go's 43 (HostDescriptorLimit.cs).
                HostDescriptorLimit.RLimit? descriptorLimit = HostDescriptorLimit.Snapshot();

                TestExecution execution = Start(test.Name, test.Action, null, test.Source, test.Line);
                WaitForSerialBoundary(execution, parallel.Add);

                if (HostDescriptorLimit.Restore(descriptorLimit) is int errno)
                    Console.Error.WriteLine($"go2cs test host: restoring RLIMIT_NOFILE after {test.Name} was refused (errno {errno}); later tests run under the lowered limit");
            }

            foreach (TestExecution execution in parallel)
                execution.ReleaseParallel();
            foreach (TestExecution execution in parallel)
                execution.Wait();
        }

        packageTimer.Stop();
        m_reporter.ReportPackage(ExitCode == 0 ? "pass" : "fail", packageTimer.Elapsed.TotalSeconds);
        return ExitCode;
    }

    /// <summary>
    /// <c>-test.list</c>: prints each registered test whose name matches the pattern, one per line, and
    /// runs nothing -- Go's M.Run hands the same pattern to listTests and returns 0 before a test starts.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The pattern is matched unanchored against the WHOLE name, as Go's <c>matchString</c> does (it is
    /// not -run's <c>/</c>-split), and in the order the run would use, which is Go's m.tests order.
    /// An invalid pattern is Go's message and status 1. Go exits there; the status is returned instead,
    /// so the in-process tier survives it and a real binary exits with the same code.
    /// </para>
    /// <para>
    /// Go also lists benchmarks, fuzz targets and examples. The registry carries none of them (Phase 4D
    /// registers tests only), so a listing here names tests only. A pattern that matches nothing, which
    /// is x/sync/singleflight's control case (<c>-test.list=^$</c>), prints nothing on both sides.
    /// </para>
    /// </remarks>
    public nint ListTests()
    {
        HasRun = true;

        Regex pattern;

        try
        {
            pattern = new Regex(m_options.ListPattern, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1.0D));
        }
        catch (ArgumentException ex)
        {
            Console.Error.Write($"testing: invalid regexp in -test.list (\"{m_options.ListPattern}\"): {ex.Message}\n");
            m_listStatus = 1;
            return 1;
        }

        foreach (RegisteredTest test in InGoOrder(m_registry.Tests))
        {
            if (pattern.IsMatch(test.Name))
                TestReporter.WriteEventLine(test.Name);
        }

        m_listStatus = 0;
        return 0;
    }

    // GO'S ORDER, not the names' (D4). See RunAll, the reason this order exists; ListTests takes the same one.
    private static IEnumerable<RegisteredTest> InGoOrder(IEnumerable<RegisteredTest> tests) => tests
        .OrderBy(test => IsExternalTest(test) ? 1 : 0)
        .ThenBy(test => test.Source, StringComparer.Ordinal)
        .ThenBy(test => test.Line)
        .ThenBy(test => test.Name, StringComparer.Ordinal);

    internal bool RunChild(TestExecution parent, string requestedName, Action<ж<testing_package.T>> action)
    {
        string name = parent.NextSubtestName(requestedName);
        if (!m_options.ShouldRun(name))
            return true;
        TestExecution child = Start(name, action, parent, parent.Source, parent.Line);
        WaitForSerialBoundary(child, parent.AddParallelChild);
        return !child.Failed;
    }

    /// <summary>
    /// A <c>testing.RunTests</c> root: Go's fresh test context, which owns its own <c>-test.parallel</c>
    /// slots and the names its top-level tests have taken.
    /// </summary>
    /// <remarks>
    /// Go's root writes to <c>os.Stdout</c> AS IT IS when RunTests is called (<c>w: os.Stdout</c>), and
    /// only output on the process's stdout reaches test2json. So while the program has redirected it
    /// (testify's TestSuiteLogging pipes it to read its own suite's log lines), <see cref="Redirected"/>
    /// writes Go's text output to that file and the root reports nothing to the run.
    /// </remarks>
    internal sealed class NestedRoot(int parallel, Action<string>? redirected)
    {
        internal SemaphoreSlim ParallelLimiter { get; } = new(parallel);

        internal Dictionary<string, int> Names { get; } = new(StringComparer.Ordinal);

        internal Action<string>? Redirected { get; } = redirected;
    }

    /// <summary>
    /// Go's runTests for <c>testing.RunTests</c>, entered from inside a running test: per <c>-count</c>
    /// iteration a fresh root runs the list as TOP-LEVEL tests -- each named as Go names a root's
    /// subtest, selected by -run/-skip through the caller's matcher, serial until it calls Parallel,
    /// the parked ones released when the list is done -- and the result is whether all of them passed.
    /// </summary>
    internal bool RunTests(IReadOnlyList<(string Name, Action<ж<testing_package.T>> F)> tests, Func<string, string, bool> matchString, Action<string>? redirectedStdout, out bool ran)
    {
        TestExecution? caller = TestExecution.Current;
        string source = caller?.Source ?? "";
        int line = caller?.Line ?? 0;
        bool ok = true;
        ran = false;

        for (int count = 0; count < m_options.Count; count++)
        {
            if (count > 0 && !ran)
                break;

            NestedRoot root = new(m_options.Parallel, redirectedStdout);
            List<TestExecution> started = [];
            List<TestExecution> parallel = [];

            foreach ((string requested, Action<ж<testing_package.T>> action) in tests)
            {
                string name = RootTestName(root, requested);

                if (!m_options.ShouldRun(name, matchString))
                    continue;

                ran = true;
                TestExecution execution = Start(name, action, null, source, line, root);
                started.Add(execution);
                WaitForSerialBoundary(execution, parallel.Add);
            }

            foreach (TestExecution execution in parallel)
                execution.ReleaseParallel();
            foreach (TestExecution execution in parallel)
                execution.Wait();

            ok = ok && started.All(execution => !execution.Failed);
        }

        return ok;
    }

    // A root's subtest name: Go's rewrite of the requested name, unique among the root's own names.
    private static string RootTestName(NestedRoot root, string requested)
    {
        string baseName = TestExecution.SanitizeName(requested);
        int sequence = root.Names.TryGetValue(baseName, out int current) ? current + 1 : 0;
        root.Names[baseName] = sequence;

        return requested.Length == 0
            ? $"#{sequence:00}"
            : sequence == 0 ? baseName : $"{baseName}#{sequence:00}";
    }

    private TestExecution Start(string name, Action<ж<testing_package.T>> action, TestExecution? parent, string source, int line, NestedRoot? nestedRoot = null)
    {
        TestExecution execution = new(this, name, parent, source, line, nestedRoot);
        execution.Start(action);
        return execution;
    }

    // The parallel sink is a CALLBACK because the two callers have different concurrency stories,
    // and passing a bare List to both hid that. The top-level loop owns a local list and runs on
    // one thread, so a plain Add is correct there. RunChild can be entered CONCURRENTLY on one
    // parent (Go permits t.Run from any goroutine -- go.dev/issue/64402), so its sink must be the
    // parent's lock-guarded writer.
    private static void WaitForSerialBoundary(TestExecution execution, Action<TestExecution> onParallel)
    {
        Task completed = Task.WhenAny(execution.Completion, execution.ParallelReached).GetAwaiter().GetResult();
        if (completed == execution.ParallelReached && !execution.Completion.IsCompleted)
            onParallel(execution);
        else
            execution.Wait();
    }

    // A failed test under a testing.RunTests root is the caller's ok == false, never the package's
    // verdict: Go's m.Run passes on its own list's results. An infrastructure failure still counts --
    // that is the HOST failing, wherever the test was started from.
    internal void Completed(TestExecution execution)
    {
        if (execution.InfrastructureFailed)
            Interlocked.Increment(ref m_infrastructureFailures);
        else if (execution.Failed && execution.NestedRoot is null)
            Interlocked.Increment(ref m_failures);
    }

    /// <summary>
    /// The goroutine-root containment policy for this run: an unhandled NON-panic exception escaping
    /// a goroutine fails the test that started it, and the run continues.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Without containment, ANY such exception on a goroutine thread reached golib's AppDomain
    /// backstop and killed the host mid-run: no result files, and every test after the crash reported
    /// no result at all — a single defect read as a mass infrastructure wall across the package. It is
    /// the same failure mode the owner-check comment below describes, arriving from converted code
    /// rather than from testing.T misuse.
    /// </para>
    /// <para>
    /// A panic is deliberately NOT contained (golib never offers one): Go's own behavior for an
    /// unrecovered panic in a goroutine is process death, and the differential oracle must keep
    /// observing that. This containment is a property of the HOST — many independent Go programs in
    /// one process — never of converted-program semantics.
    /// </para>
    /// <para>
    /// If the failed goroutine was the one that would have unblocked its test, that test now waits
    /// rather than dying instantly, and the package timeout ends it — which still writes every result
    /// gathered so far, where the crash wrote none.
    /// </para>
    /// </remarks>
    internal void ContainGoroutineException(Exception ex)
    {
        // The test whose goroutine this is: an AsyncLocal flows with the ExecutionContext that
        // ThreadPool.QueueUserWorkItem captures — exactly how golib dispatches a goroutine — so the
        // attribution survives any depth of goroutine spawning goroutines.
        string owner = TestExecution.Current?.Name ?? "";

        if (TestExecution.Current is TestExecution execution)
            execution.RecordGoroutineFailure(ex);
        else
            RecordInfrastructureFailure("", $"unhandled exception on a goroutine outside any test: {ex}");

        // The package's terminal event, for the SAME reason ReportGoroutinePanic writes one: the
        // caller flushes the evidence and exits, so RunAll never reaches its own terminal event.
        //
        // Without this line the non-panic death is the one truncation in the family that leaves NO
        // MARKER AT ALL. The other two announce themselves — a panic writes "died on an unrecovered
        // panic in a goroutine", a package deadline writes an "action":"timeout" event — but an
        // unhandled .NET exception escaping a goroutine simply STOPPED the results stream mid-test,
        // with no timeout and no death event, and the only tell was that the stream ended. Measured
        // in the runtime/pprof walls census (2026-09-03): a `pprof_goroutineProfileWithLabels` stub
        // throwing inside Goroutine.Run took the host down during TestGoroutineProfileLabelRace, and
        // a slice reading 2 of 7 verdicts was indistinguishable from a mass-empty conversion failure
        // until its log was read by hand. The mass-empty family's diagnostic rule — read the results
        // tail first, because a kill states itself — was simply FALSE for this member.
        //
        // It names the exception TYPE and the test that owned the goroutine, because those are the
        // two facts that turn "the stream stopped" into an attributable failure.
        m_reporter.ReportPackage("fail", output: owner.Length > 0
            ? $"test binary died on an unhandled {ex.GetType().Name} on a goroutine started by {owner}"
            : $"test binary died on an unhandled {ex.GetType().Name} on a goroutine outside any test");
    }

    /// <summary>
    /// Reports a PANIC that escaped a goroutine root. The process is about to end on it (Go's own
    /// behavior for an unrecovered panic in any goroutine), so this is the run's only chance to say
    /// which test it belonged to and where it faulted.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The panic is a genuine, Go-comparable VERDICT — converted code panicked where Go's did not —
    /// so it is recorded as a test FAILURE, not as an infrastructure error. The distinction is the
    /// one this class draws throughout: infrastructure means the host could not run the test, and the
    /// host ran this one exactly as asked.
    /// </para>
    /// <para>
    /// The traceback comes from <see cref="PanicException.StackTrace"/>, which prefers the panic's
    /// ORIGIN over the frames it unwound through — without it the report names whichever machinery
    /// re-raised the panic last, which is the same as naming nothing.
    /// </para>
    /// </remarks>
    internal void ReportGoroutinePanic(PanicException panic)
    {
        // Go's own shape: the panic value first, then the traceback.
        string report = $"panic: {panic.Message}{Environment.NewLine}{panic.StackTrace}";

        if (TestExecution.Current is TestExecution execution)
            execution.RecordGoroutinePanic(report);
        else
            RecordInfrastructureFailure("", $"panic on a goroutine outside any test{Environment.NewLine}{report}");

        // The package's terminal event, because RunAll will never reach its own.
        m_reporter.ReportPackage("fail", output: "test binary died on an unrecovered panic in a goroutine");
    }

    /// <summary>
    /// Records a host-level infrastructure failure that cannot be attached to a live execution —
    /// e.g. testing.T misuse observed after its test already completed, or an unexpected exception
    /// escaping an execution thread. Counted toward the exit code and disclosed as an event so the
    /// failure can never silently pass.
    /// </summary>
    internal void RecordInfrastructureFailure(string name, string output)
    {
        Interlocked.Increment(ref m_infrastructureFailures);
        m_reporter.Report(new TestEvent(Package, name, "infrastructure-error", Output: output));
    }

    // A parallel test holds one slot while it RUNS (acquired after its serial-phase gate opens,
    // released before it waits on its own parallel children — Go's tRunner does the same, so a
    // parallel parent never starves its children under a small -parallel cap).
    internal void AcquireParallelSlot(NestedRoot? root) => (root?.ParallelLimiter ?? m_parallelLimiter).Wait();

    internal void ReleaseParallelSlot(NestedRoot? root) => (root?.ParallelLimiter ?? m_parallelLimiter).Release();

    internal void Report(TestEvent testEvent) => m_reporter.Report(testEvent);

    // Whether a registered test comes from the package's EXTERNAL (package x_test) files. Go's own definition is the
    // package clause: an external test file's clause is `<name>_test`. The converter stamps every test class with
    // its clause as [GoPackage("<clause>")], and a converted host registers each test by method group, so the
    // declaring class's stamp says which list Go put the test in. The class NAME cannot: a package named `x_internal`
    // declares its external tests in `x_internal_test_package`. A host-side lambda (GolibTests) or any unstamped
    // class reads as internal, and its relative order is unchanged.
    //
    // Only Test functions are registered: examples and benchmarks are deferred by kind (Phase 4D) and fuzz targets
    // are compile-only. When examples register, they need a kind key AHEAD of Source/Line, because testing.M runs
    // every test, then every fuzz target's seeds, then every example.
    private static bool IsExternalTest(RegisteredTest test) =>
        test.Action.Method.DeclaringType?.GetCustomAttribute<GoPackageAttribute>()?.PackageName
            .EndsWith("_test", StringComparison.Ordinal) ?? false;

    private static void Shuffle<T>(IList<T> values, int seed)
    {
        Random random = new(seed);
        for (int i = values.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (values[i], values[j]) = (values[j], values[i]);
        }
    }
}
