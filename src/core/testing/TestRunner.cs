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

    public nint ExitCode => m_failures == 0 && m_infrastructureFailures == 0 ? 0 : 1;

    internal string Package => m_registry.Package;

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
            List<RegisteredTest> tests = m_registry.Tests
                .Where(test => m_options.ShouldRun(test.Name))
                .OrderBy(test => test.Name, StringComparer.Ordinal)
                .ToList();

            if (m_options.ShuffleSeed is int seed)
                Shuffle(tests, unchecked(seed + count));

            // Go releases top-level parallel tests at the end of EACH -count iteration, so
            // iterations interleave serial and parallel phases rather than batching every
            // iteration's parallel tests to the very end of the run.
            List<TestExecution> parallel = [];

            foreach (RegisteredTest test in tests)
            {
                TestExecution execution = Start(test.Name, test.Action, null, test.Source, test.Line);
                WaitForSerialBoundary(execution, parallel.Add);
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

    internal bool RunChild(TestExecution parent, string requestedName, Action<ж<testing_package.T>> action)
    {
        string name = parent.NextSubtestName(requestedName);
        if (!m_options.ShouldRun(name))
            return true;
        TestExecution child = Start(name, action, parent, parent.Source, parent.Line);
        WaitForSerialBoundary(child, parent.AddParallelChild);
        return !child.Failed;
    }

    private TestExecution Start(string name, Action<ж<testing_package.T>> action, TestExecution? parent, string source, int line)
    {
        TestExecution execution = new(this, name, parent, source, line);
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

    internal void Completed(TestExecution execution)
    {
        if (execution.InfrastructureFailed)
            Interlocked.Increment(ref m_infrastructureFailures);
        else if (execution.Failed)
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
    internal void AcquireParallelSlot() => m_parallelLimiter.Wait();

    internal void ReleaseParallelSlot() => m_parallelLimiter.Release();

    internal void Report(TestEvent testEvent) => m_reporter.Report(testEvent);

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
