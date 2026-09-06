// TestExecution.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
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
/// One running test or subtest — the state behind the <c>*testing.T</c> a converted test body holds.
/// </summary>
/// <remarks>
/// <para>
/// Every <c>t.</c>-something a Go test can call lands here: <c>Fail</c>/<c>FailNow</c>,
/// <c>Skip</c>, <c>Log</c>, <c>Helper</c>, <c>Cleanup</c>, <c>Run</c> (subtests), <c>Parallel</c>,
/// <c>TempDir</c> and <c>Setenv</c>. Instances form a tree — a subtest keeps its parent — because
/// Go's naming, failure propagation and parallel release are all defined over that tree.
/// </para>
/// <para>
/// Each test gets its OWN thread rather than a pool work item, for two independent reasons. A
/// parallel test PARKS its thread until the serial phase completes, and dozens-to-hundreds of
/// parked thread-pool threads would starve the pool — stalling the suite and every converted
/// goroutine the tests spawn, since golib queues those on that same pool. And a dedicated thread
/// can be created with a deliberately huge stack reservation (see <c>TestThreadStackSize</c>):
/// Go goroutine stacks GROW, so deeply recursive test code is legal Go, while a .NET thread's
/// stack is a fixed reservation whose overflow is uncatchable and kills the entire host.
/// </para>
/// <para>
/// <c>FailNow</c> and <c>SkipNow</c> must abandon the test body immediately, which in Go is a
/// <c>runtime.Goexit</c> on the test's goroutine. Here that is <see cref="TestAbortException"/>,
/// thrown on the test's own thread and caught at its root — which is also why several members check
/// they are on the OWNING thread first: Go says those calls are only valid from the test's own
/// goroutine, and honoring them from another one would unwind a stack that is not the test's.
/// </para>
/// <para>
/// The current execution flows to goroutines through an <see cref="AsyncLocal{T}"/>, so a failure
/// raised inside a goroutine the test started can be attributed to the test that started it rather
/// than to whatever happened to be running.
/// </para>
/// </remarks>
public sealed class TestExecution
{
    // Go goroutine stacks GROW (to ~1GB on 64-bit), so deeply recursive test code is legal Go —
    // io/fs's TestCVE202230630 legitimately recurses 10,001 frames through globWithLimit before
    // its own depth guard fires. A .NET thread's stack is a FIXED reservation (default ~1MB) and
    // an overflow is uncatchable, killing the whole host. The reservation costs address space only
    // (pages commit on demand), so it is sized at Go's OWN ceiling rather than at a fraction of it.
    //
    // 256MB was not Go-scale, and go/parser is the proof: its parser guards recursion with
    // maxNestLev = 1e5 and TestParseDepthLimit feeds it maxNestLev+1 deliberately, so Go recurses
    // 100,001 levels — about four converted frames each through
    // parseType -> parseFieldDecl -> parseStructType -> tryIdentOrType — before its own guard fires.
    // That is ~400k frames, which 256MB serves only if every frame fits in 671 bytes; the converted
    // frames do not, and the host died with an uncatchable "Stack overflow." partway through the
    // suite, taking every later verdict with it. Matching Go's 1GB maximum makes the host's headroom
    // the same headroom the code was written against.
    private const int TestThreadStackSize = 1024 * 1024 * 1024;

    // A test's log output is a diagnostic, and a diagnostic nobody can read is not one. These two
    // bounds are what stop a runaway test from turning its own output into the thing that kills the
    // host: MaxLogCharacters bounds what ONE execution accumulates, MaxRecordCharacters bounds any
    // single record within it, so neither a storm of small records nor one pathological record can
    // grow the report without limit.
    //
    // Both sit far above any real test's output (Go's own suites report bytes to kilobytes) and far
    // below what breaks a consumer. That gap is not theoretical: System.Text.Json refuses a value
    // near 700 MB, which is exactly how sync/atomic's TestHammerStoreLoad killed this host — a
    // late-goroutine Fatalf storm drove m_logs to a 693 MB join, the join went OutOfMemory inside
    // the fatal path, and the run lost 72 of 108 verdicts because the process died instead of
    // reporting them. Whether the storm won that race was HOST-DEPENDENT, so the package read
    // 104/108 on one machine and 35/108 on another — a false host-dependence the harness owes
    // nobody (coordinator Ruling A, 2026-08-20, which queued this as a bounded robustness fix).
    internal const int MaxLogCharacters = 1 << 20;

    internal const int MaxRecordCharacters = 1 << 16;

    internal const string RecordTruncatedSuffix = " [record truncated]";

    // The test a goroutine belongs to. Set on the test's own thread, it flows into every goroutine
    // that thread starts (and into theirs, transitively) because ThreadPool.QueueUserWorkItem —
    // golib's goroutine dispatch — captures the ExecutionContext an AsyncLocal lives in.
    private static readonly AsyncLocal<TestExecution?> s_current = new();

    private readonly TestRunner m_runner;
    private readonly TestExecution? m_parent;
    private readonly object m_syncRoot = new();
    private readonly Stack<Action> m_cleanups = new();
    private readonly List<string> m_logs = [];
    private readonly Dictionary<string, int> m_subtestNames = new(StringComparer.Ordinal);
    private readonly ManualResetEventSlim m_parallelGate = new(false);
    private int m_logCharacters;
    private int m_logsDropped;
    private int m_ownerThread;
    private int m_tempDirSequence;
    private bool m_parallel;
    private bool m_envSet;
    private bool m_holdsParallelSlot;
    private bool m_finished;
    private bool m_failed;
    private bool m_skipped;
    private bool m_measurementUnitNoted;

    internal TestExecution(TestRunner runner, string name, TestExecution? parent, string source, int line)
    {
        m_runner = runner;
        m_parent = parent;
        Name = name;
        Source = source;
        Line = line;
    }

    public string Name { get; }

    public string Source { get; }

    public int Line { get; }

    public bool Failed { get { lock (m_syncRoot) return m_failed; } }

    public bool Skipped { get { lock (m_syncRoot) return m_skipped; } }

    public bool InfrastructureFailed { get; private set; }

    internal Task Completion { get; private set; } = Task.CompletedTask;

    private TaskCompletionSource ParallelSource { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal Task ParallelReached => ParallelSource.Task;

    // Go keeps a parent's sub-test bookkeeping under t.mu because t.Run is callable from ANY
    // goroutine (go.dev/issue/64402, whose regression tests are TestConcurrentRun and TestParentRun).
    // This list is that bookkeeping, and it was the ONE member of the per-parent set that was not
    // already guarded: NextSubtestName locks m_syncRoot for the naming counter, TestExecution's own
    // failed/finished accounting locks it, and the child objects are per-child. A bare List<T> whose
    // Add runs on the CALLING goroutine while the parent's wait enumerates it is a torn read on one
    // side and a lost child on the other, so both ends move under the lock the rest of the state
    // already uses. AddParallelChild is the only writer; the wait takes a SNAPSHOT rather than
    // holding the lock across child.Wait(), which would serialise exactly what this exists to allow.
    private readonly List<TestExecution> m_parallelChildren = [];

    internal void AddParallelChild(TestExecution child)
    {
        lock (m_syncRoot)
            m_parallelChildren.Add(child);
    }

    internal TestExecution[] ParallelChildrenSnapshot()
    {
        lock (m_syncRoot)
            return m_parallelChildren.ToArray();
    }

    internal void Start(Action<ж<testing_package.T>> action)
    {
        TaskCompletionSource completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Completion = completion.Task;

        // Each execution gets a DEDICATED thread rather than Task.Run: a parallel test parks its
        // thread on m_parallelGate (and then on the -parallel limiter) until the serial phase
        // completes, and dozens-to-hundreds of parked thread-pool threads would starve the pool
        // (injection is ~1 thread/s) — stalling both the suite and any converted goroutines the
        // tests spawn, which golib queues on that same pool. Dedicated threads keep test parking
        // and goroutine scheduling independent at stdlib-suite scale.
        Thread thread = new(() =>
        {
            // This dedicated thread IS the test's goroutine (Go's tRunner runs every test in one),
            // so mark it as such: runtime.Goexit — which testing.T.FailNow is specified in terms
            // of — is supported from a goroutine and gated from the main goroutine.
            using Goroutine.Scope goroutine = Goroutine.Enter();

            try
            {
                Execute(action);
            }
            catch (Exception ex)
            {
                // Execute contains its own handling; anything escaping it is a host defect. Contain
                // it here — an unhandled exception on a background thread would hit golib's
                // AppDomain backstop, which prints the report to stderr and exits 2 (like Go):
                // the whole run dies mid-flight with NO result files and every unrelated test
                // killed, instead of one attributed infrastructure failure.
                ReportHostFailure(ex);
            }
            finally
            {
                completion.TrySetResult();
            }
        }, TestThreadStackSize)
        {
            IsBackground = true,
            Name = $"go2cs test: {Name}"
        };

        thread.Start();
    }

    // The containment report is this thread's LAST WORD, so it must not be lost to its own
    // machinery: the interpolation below allocates through the shared char pool, and an exception
    // it raises would escape the containment catch to the AppDomain backstop wearing the REPORT
    // path's stack — the primary exception masked entirely (measured under compare load,
    // 2026-08-28: nine crashes reported only the reporter's own SharedArrayPool NRE; nine
    // primaries unknown). Each fallback asks strictly less of the runtime than the one before it,
    // and the primary's TYPE is written before anything that formats a stack.
    private void ReportHostFailure(Exception primary)
    {
        try
        {
            m_runner.RecordInfrastructureFailure(Name, $"test host failure: {primary}");
        }
        catch (Exception reporterEx)
        {
            try
            {
                Console.Error.Write("test host failure (report path failed): primary ");
                Console.Error.WriteLine(primary.GetType().FullName);
                Console.Error.WriteLine(primary.StackTrace);
                Console.Error.Write("reporter ");
                Console.Error.WriteLine(reporterEx.GetType().FullName);
                Console.Error.WriteLine(reporterEx.StackTrace);
            }
            catch
            {
                // Nothing further can be said on stderr; the record attempt below still runs.
            }

            try
            {
                m_runner.RecordInfrastructureFailure(Name,
                    "test host failure: report path failed; primary exception type and stack are on stderr");
            }
            catch
            {
                // The runner itself cannot record; Completion still resolves in the finally.
            }
        }
    }

    internal void Wait() => Completion.GetAwaiter().GetResult();

    internal void ReleaseParallel() => m_parallelGate.Set();

    public void Fail()
    {
        lock (m_syncRoot)
        {
            // RESIDUAL, deliberately NOT changed with Log's fix above and recorded here rather than
            // left to be rediscovered. Go's Fail diverges from this in TWO ways and only ONE of them
            // is the same defect Log had:
            //
            //   func (c *common) Fail() {
            //       if c.parent != nil { c.parent.Fail() }        // propagate FIRST, unconditionally
            //       ...
            //       if c.done { panic("Fail in goroutine after " + c.name + " has completed") }
            //       c.failed = true
            //   }
            //
            // The KIND is the same divergence Log carried -- Go panics, this throws a .NET exception
            // that a converted recover() cannot see. The ORDER is not: Go marks the whole ancestor
            // chain failed BEFORE it can panic, and its recursive parent.Fail() would itself panic on
            // a done ancestor, where FailFromChild never does. Fixing the kind alone would leave the
            // order wrong; fixing the order needs FailFromChild's semantics decided against Go's
            // recursion, which is a separate question from the one Log's fix answers. Neither is
            // reached by the late-log path this cut is for.
            if (m_finished)
                throw new InvalidOperationException($"Fail called after {Name} completed");
            m_failed = true;
        }
        m_parent?.FailFromChild();
    }

    public void FailNow()
    {
        if (!TryEnsureOwner(nameof(FailNow)))
            return;
        Fail();
        throw new TestAbortException();
    }

    public void SkipNow()
    {
        if (!TryEnsureOwner(nameof(SkipNow)))
            return;
        lock (m_syncRoot)
            m_skipped = true;
        throw new TestAbortException();
    }

    /// <summary>
    /// Go's <c>common.logDepth</c> (testing.go:1015-1032): append one record to this test's output,
    /// and when this test has already FINISHED, hand the record to the nearest LIVE ancestor instead
    /// of refusing it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This host refused every late record until 2026-09-06, and it diverged from Go's contract in
    /// TWO ways at once. Go's own text:
    /// </para>
    /// <code>
    /// if c.done {
    ///     for parent := c.parent; parent != nil; parent = parent.parent {
    ///         parent.mu.Lock()
    ///         defer parent.mu.Unlock()
    ///         if !parent.done {
    ///             parent.output = append(parent.output, parent.decorate(s, depth+1)...)
    ///             return
    ///         }
    ///     }
    ///     panic("Log in goroutine after " + c.name + " has completed: " + s)
    /// }
    /// </code>
    /// <para>
    /// REACHABILITY. Go panics only when NO live ancestor exists, and <c>runTests</c> runs every
    /// top-level test as <c>t.Run</c> on a root <c>T</c> (testing.go:2155-2169, the child's parent
    /// set at :1724), so mid-run there is ALWAYS a live ancestor and Go's panic is effectively
    /// unreachable until the whole run ends. A refusal here therefore fired on a path Go structurally
    /// never takes — and it fires routinely, because <c>internal/testenv.Command</c> installs a
    /// <c>cmd.Cancel</c> closure that calls <c>t.Logf</c> (exec.go:186,199) from <c>os/exec</c>'s
    /// <c>watchCtx</c> goroutine, which can outlive the test that started it.
    /// </para>
    /// <para>
    /// KIND. Go PANICS, so <c>recover()</c> can see it and golib's backstop reports it Go-style and
    /// exits 2. A raw <see cref="InvalidOperationException"/> is invisible to a converted
    /// <c>recover()</c> and lands in the host's INFRASTRUCTURE bucket — which by TestRunner's own
    /// definition means "the host could not run the test", the opposite of what happened. So the
    /// refusal, when it is genuinely owed, is <c>builtin.panic</c> carrying Go's text composed exactly
    /// as testing.go composes it.
    /// </para>
    /// <para>
    /// Two deliberate differences from Go's letter, neither observable. Go holds every ancestor lock
    /// it takes until it returns (its <c>defer</c>s); this walk releases each one before moving on,
    /// which cannot matter because the record is appended INSIDE the lock that proved the ancestor
    /// live. And the appended record keeps this host's existing trailing-newline trim while the PANIC
    /// text carries the caller's string untrimmed, which is what Go's <c>s</c> is.
    /// </para>
    /// </remarks>
    public void Log(string text)
    {
        lock (m_syncRoot)
        {
            if (!m_finished)
            {
                AppendLog(text.TrimEnd('\r', '\n'));
                return;
            }

            // Child lock, then ancestor locks, outermost-first — the same order FailFromChild takes
            // and the same order Go takes, so this walk cannot invert against either.
            for (TestExecution? ancestor = m_parent; ancestor is not null; ancestor = ancestor.m_parent)
            {
                lock (ancestor.m_syncRoot)
                {
                    if (ancestor.m_finished)
                        continue;

                    ancestor.AppendLog(text.TrimEnd('\r', '\n'));
                    return;
                }
            }

            throw builtin.panic(LogAfterCompleteText(Name, text));
        }
    }

    /// <summary>
    /// Go's panic text for a late log with no live ancestor, composed exactly as testing.go:1029
    /// composes it: <c>"Log in goroutine after " + c.name + " has completed: " + s</c>.
    /// </summary>
    /// <remarks>
    /// Composed rather than a constant because Go's own text carries the test's name and the record,
    /// and quoted rather than paraphrased for the reason the Setenv/Parallel texts below are: a
    /// recovered value is compared against the whole string.
    /// </remarks>
    internal static string LogAfterCompleteText(string name, string record) =>
        $"Log in goroutine after {name} has completed: {record}";

    public void Helper()
    {
        // Helper-frame elision is staged; declaration source identity is still reported.
    }

    /// <summary>
    /// Records — at most once per test — that a number this test is about was measured in a
    /// different UNIT than the Go declaration reporting it assumes. The note rides the test's
    /// own log output, so it lands beside the assert's own message rather than replacing it.
    /// </summary>
    /// <remarks>
    /// The one caller is <see cref="testing_package.AllocsPerRun"/>. Silently letting a byte
    /// figure be printed by Go's own <c>"got %v allocs"</c> format would present bytes AS a
    /// count, which is the thing that must not happen: a reader — or a later disclosure
    /// decision — has to be able to see which unit the number is in. Attaching it here rather
    /// than at the measurement keeps the seam observable in <c>results.json</c> too, since a
    /// test's log output is carried on its <see cref="TestEvent"/> whatever the verdict.
    /// </remarks>
    internal void NoteMeasurementUnitOnce(string note)
    {
        lock (m_syncRoot)
        {
            if (m_finished || m_measurementUnitNoted)
                return;
            m_measurementUnitNoted = true;
            AppendLog(note);
        }
    }

    public void Cleanup(Action cleanup)
    {
        ArgumentNullException.ThrowIfNull(cleanup);
        lock (m_syncRoot)
        {
            // A cleanup registered after the test completed can never run (the cleanup phase is
            // already over) — reject it instead of silently dropping it. Go panics here too.
            if (m_finished)
                throw new InvalidOperationException($"Cleanup called after {Name} completed");
            m_cleanups.Push(cleanup);
        }
    }

    /// <summary>
    /// Go's <c>T.Run</c>. Callable from ANY goroutine, deliberately.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>There is no owner-goroutine check here, and its absence is the contract.</b> Go's
    /// <c>testing</c> restricts <c>FailNow</c> and <c>Fatal</c> to the test's own goroutine — a
    /// restriction this host keeps, because those unwind the frame — but it places NO such
    /// restriction on <c>Run</c>. Two of Go's own tests pin that, both regression tests for
    /// go.dev/issue/64402: <c>TestConcurrentRun</c> starts two goroutines that each call
    /// <c>t.Run</c> on the SAME parent, and <c>TestParentRun</c> calls <c>t1.Run</c> from inside
    /// <c>t2</c>'s body.
    /// </para>
    /// <para>
    /// This host refused both until 2026-09-04, and the two refusals failed differently, which is
    /// why only one of them was visible: <c>TestParentRun</c> merely reported an infrastructure
    /// error and lost its inner subtest, while <c>TestConcurrentRun</c> DEADLOCKED — the refused
    /// <c>Run</c> never runs the body, so the body's <c>ready.Done()</c> never happens and the
    /// parent's <c>ready.Wait()</c> blocks until the package deadline kills the whole run. One
    /// defect, one row unmeasurable.
    /// </para>
    /// <para>
    /// What made the check safe to remove is not the reading but the audit behind it: Go keeps a
    /// parent's sub-test bookkeeping under <c>t.mu</c>, and every member of this host's equivalent
    /// set was already guarded except one. <see cref="NextSubtestName"/> locks for the naming
    /// counter, the failed/finished accounting locks, the child objects are per-child — and the
    /// parallel-children list was a bare <c>List&lt;T&gt;</c> written by the calling goroutine and
    /// enumerated by the parent's wait. That list now moves under the same lock
    /// (<see cref="AddParallelChild"/> / <see cref="ParallelChildrenSnapshot"/>); removing the check
    /// without it would have traded a deadlock for a race.
    /// </para>
    /// </remarks>
    public bool Run(string name, Action<ж<testing_package.T>> action)
    {
        return m_runner.RunChild(this, name, action);
    }

    /// <summary>
    /// Go's <c>common.isParallel</c> for this execution, read under the lock
    /// <see cref="Parallel"/> writes it under.
    /// </summary>
    private bool IsParallel
    {
        get
        {
            lock (m_syncRoot)
                return m_parallel;
        }
    }

    /// <summary>
    /// Whether this execution or ANY ancestor has called <c>t.Parallel</c>.
    /// </summary>
    /// <remarks>
    /// Go's <c>T.Setenv</c> walks the whole chain — <c>for c := &amp;t.common; c != nil; c = c.parent</c>
    /// (testing.go:1515) — and its own comment says why: a NON-parallel subtest of a PARALLEL parent
    /// still runs concurrently with tests outside that parent, so it is only serial with respect to
    /// its siblings. <c>Setenv</c> mutates the whole process, so the ancestor's parallelism disqualifies
    /// the child just as the child's own would. Checking only this execution's flag — which is what
    /// this host did until 2026-09-03 — accepts exactly the two cases Go's own suite pins,
    /// <c>TestSetenvWithParallelParentBeforeSetenv</c> and its grand-parent sibling.
    /// </remarks>
    private bool HasParallelSelfOrAncestor()
    {
        for (TestExecution? execution = this; execution is not null; execution = execution.m_parent)
        {
            if (execution.IsParallel)
                return true;
        }

        return false;
    }

    // Go's three panic texts for the Setenv/Parallel contract, VERBATIM from testing.go (1444-1448,
    // 1523) at the pinned go1.23.12. They are quoted rather than composed because Go's own tests
    // compare the recovered value against the whole string with ==, so a paraphrase, a truncation at
    // the semicolon (what this host shipped), or an interpolated test name all read as "no panic".
    private const string ParallelCalledMultipleTimesText =
        "testing: t.Parallel called multiple times";
    private const string ParallelAfterSetenvText =
        "testing: t.Parallel called after t.Setenv; cannot set environment variables in parallel tests";
    private const string SetenvAfterParallelText =
        "testing: t.Setenv called after t.Parallel; cannot set environment variables in parallel tests";

    public void Parallel()
    {
        if (!TryEnsureOwner(nameof(Parallel)))
            return;
        lock (m_syncRoot)
        {
            // Go's order, which is observable: isParallel is tested before isEnvSet, so a test that
            // is both already-parallel and env-set recovers the "multiple times" text.
            if (m_parallel)
                throw builtin.panic(ParallelCalledMultipleTimesText);
            if (m_envSet)
                throw builtin.panic(ParallelAfterSetenvText);
            m_parallel = true;
        }
        ParallelSource.TrySetResult();
        m_parallelGate.Wait();

        // Released from the serial-phase gate; now compete for a -parallel slot so at most
        // Options.Parallel parallel tests RUN simultaneously (Go's -parallel semantics).
        m_runner.AcquireParallelSlot();
        m_holdsParallelSlot = true;
    }

    /// <summary>
    /// Go's <c>t.TempDir()</c>: a per-test directory, removed when the test finishes.
    /// </summary>
    /// <remarks>
    /// It lives at the run sandbox's ROOT rather than under the working directory, and the
    /// difference is observable. Go's <c>TempDir</c> goes through <c>os.MkdirTemp("")</c>, so it
    /// lands in the system temp — a location with no <c>go.mod</c> anywhere above it and outside the
    /// package's own directory. Under the working directory it would be neither: the staged ancestry
    /// puts <c>src/go.mod</c> (module std) and a <c>vendor</c> directory above every path inside the
    /// package tree, so a test that runs the toolchain in its own temp directory would have the
    /// module walk terminate at the standard library instead of finding nothing — go/build's
    /// TestImportPackageOutsideModule wants exactly "go.mod file not found in current directory or
    /// any parent directory" and got the vendor-mode error instead. Hoisting it to the run root
    /// restores Go's property, and incidentally keeps <c>.tmp</c> out of the working directory a
    /// test may enumerate.
    /// </remarks>
    public @string TempDir()
    {
        string path = Path.Combine(m_runner.RunRoot, ".tmp", TempDirName(Name), Interlocked.Increment(ref m_tempDirSequence).ToString(CultureInfo.InvariantCulture));
        Directory.CreateDirectory(path);
        Cleanup(() => RemoveAllWithWindowsRetry(path));
        return path;
    }

    /// <summary>
    /// <see cref="RemoveAll"/>, retrying the two Windows errors that mean "something still has this
    /// open, briefly" — mirroring Go's own <c>testing.removeAll</c>, which its <c>TempDir</c> cleanup
    /// uses for exactly this reason.
    /// </summary>
    /// <remarks>
    /// On Windows a handle outlives the process that closed it by a short, unpredictable interval —
    /// an antivirus scan, an indexer, or a child process the test spawned and waited for. Go
    /// accommodates that with a bounded retry (go.dev/issue/50051 and 51442) rather than failing the
    /// cleanup, and a host that does not is REPRODUCIBLY less tolerant than the runtime it is
    /// standing in for: os's TestStatLxSymLink drives a WSL child inside its TempDir and its cleanup
    /// failed 3 runs of 3 with <c>ERROR_SHARING_VIOLATION</c> on the directory, while `go test`
    /// passed the same test every time. The timeout and the jittered backoff are Go's.
    /// </remarks>
    private static void RemoveAllWithWindowsRetry(string path)
    {
        TimeSpan timeout = TimeSpan.FromSeconds(2);
        TimeSpan nextSleep = TimeSpan.FromMilliseconds(1);
        DateTime start = default;

        while (true)
        {
            try
            {
                RemoveAll(path);
                return;
            }
            catch (Exception ex) when (IsWindowsRetryable(ex))
            {
                if (start == default)
                {
                    start = DateTime.UtcNow;
                }
                else if (DateTime.UtcNow - start + nextSleep >= timeout)
                {
                    throw;
                }

                Thread.Sleep(nextSleep);
                nextSleep += TimeSpan.FromTicks(Random.Shared.NextInt64(nextSleep.Ticks + 1));
            }
        }
    }

    // ERROR_SHARING_VIOLATION (32) and ERROR_ACCESS_DENIED (5) — the two Go's isWindowsRetryable
    // names. .NET surfaces the first as IOException and the second as UnauthorizedAccessException,
    // both carrying the Win32 code in HResult.
    private static bool IsWindowsRetryable(Exception ex)
    {
        const int ErrorAccessDenied = unchecked((int)0x80070005);
        const int ErrorSharingViolation = unchecked((int)0x80070020);

        return ex is IOException or UnauthorizedAccessException &&
               ex.HResult is ErrorAccessDenied or ErrorSharingViolation;
    }

    // Go's os.RemoveAll semantics for the TempDir cleanup: a reparse point (junction or symlink)
    // is removed as the link itself and never traversed, and a read-only attribute is cleared
    // before a retry the way os.Remove does on Windows. .NET's Directory.Delete(path, true)
    // instead opens some junction targets during its recursive walk -- a junction to an
    // NT-namespace volume root (filepath's TestNTNamespaceSymlink) fails it with
    // UnauthorizedAccessException where Go's cleanup succeeds.
    private static void RemoveAll(string path)
    {
        if (File.Exists(path))
        {
            DeleteEntry(new FileInfo(path));
            return;
        }

        DirectoryInfo directory = new(path);

        if (!directory.Exists)
            return;

        if ((directory.Attributes & FileAttributes.ReparsePoint) == 0)
        {
            foreach (FileSystemInfo entry in directory.GetFileSystemInfos())
            {
                if (entry is DirectoryInfo)
                    RemoveAll(entry.FullName);
                else
                    DeleteEntry(entry);
            }
        }

        DeleteEntry(directory);
    }

    private static void DeleteEntry(FileSystemInfo entry)
    {
        try
        {
            entry.Delete();
        }
        catch (UnauthorizedAccessException) when ((entry.Attributes & FileAttributes.ReadOnly) != 0)
        {
            entry.Attributes &= ~FileAttributes.ReadOnly;
            entry.Delete();
        }
    }

    public void Setenv(string key, string value)
    {
        if (!TryEnsureOwner(nameof(Setenv)))
            return;

        if (HasParallelSelfOrAncestor())
            throw builtin.panic(SetenvAfterParallelText);

        // Set BEFORE the environment is written, as Go does (testing.go:1525, ahead of the
        // common.Setenv call): a later t.Parallel must be refused whether or not the write below
        // succeeded, because the test has already declared an intent to mutate process state.
        lock (m_syncRoot)
            m_envSet = true;

        // Write BOTH env stores the corpus reads through. The corpus has two disjoint env-reader
        // paths on the Linux flavor, and a foundational test host must feed both or it fixes one
        // consumer while breaking the other:
        //   - syscall.Getenv reads golib's PRIVATE `envs` copy (syscall/linux/env_unix.cs). This is
        //     what time.LoadLocation reads for ZONEINFO — and t.Setenv writing ONLY the managed
        //     store left ZONEINFO invisible there, so time.TestEnvVarUsage read empty on Linux (R6;
        //     it passed on Windows because there syscall.Getenv reads the same Win32 block the
        //     managed store updates — the two coincide on Windows and diverge on Linux).
        //   - Environment.GetEnvironmentVariable is what internal/godebug's GODEBUG reader observes.
        //     Writing ONLY the syscall store regressed every GODEBUG-driven test (archive/tar's
        //     tarinsecurepath, mime/multipart's multipartmax*, go/types' gotypesalias) — measured.
        // Writing both keeps every reader consistent; each store's previous value is captured and
        // restored on cleanup. (Go itself needs only os.Setenv because its runtime and os share one
        // environ; the managed split is a go2cs-specific fact of the two-store corpus.)
        var (syscallPrevious, syscallHad) = go.syscall_package.Getenv(key);
        string? managedPrevious = Environment.GetEnvironmentVariable(key);

        go.syscall_package.Setenv(key, value);
        Environment.SetEnvironmentVariable(key, value);

        Cleanup(() =>
        {
            if (syscallHad)
                go.syscall_package.Setenv(key, syscallPrevious);
            else
                go.syscall_package.Unsetenv(key);
            Environment.SetEnvironmentVariable(key, managedPrevious);
        });
    }

    internal string NextSubtestName(string requested)
    {
        string baseName = SanitizeName(requested);
        lock (m_syncRoot)
        {
            int sequence = m_subtestNames.TryGetValue(baseName, out int current) ? current + 1 : 0;
            m_subtestNames[baseName] = sequence;
            // Go names EVERY empty-name subtest by its per-parent sequence alone (#00, #01, …,
            // matching Go's testing.tRunner naming) — the sequence IS the name, never a dedup
            // suffix on a shared "#00" (the old form emitted #00, #00#01, … and every subtest
            // row keyed one-sided against `go test -json`). Duplicate NON-empty names keep the
            // Go dedup form name#NN, byte-identical to before (sort.TestFind's ab/ab#01/ab#02).
            string unique = requested.Length == 0
                ? $"#{sequence:00}"
                : sequence == 0 ? baseName : $"{baseName}#{sequence:00}";
            return $"{Name}/{unique}";
        }
    }

    internal static TestExecution? Current => s_current.Value;

    // Q54: a test body, or a goroutine it started, that dies while holding a converted runtime lock
    // would leave that lock's key orphaned — the next locker would poll forever and the package
    // deadline would be consumed (measured eight times on the runtime row). The protocol poisons
    // every lock the dying THREAD holds so the next locker dies by name instead
    // (runtime/lock_managed_impl.cs); each seam below runs on the dying goroutine's own thread.
    private static void AbandonRuntimeLocks(Exception ex)
    {
        global::go.runtime_package.GoAbandonRuntimeLocksHeldByCurrentThread($"{ex.GetType().Name}: {ex.Message}");
    }

    internal void RecordGoroutineFailure(Exception ex)
    {
        AbandonRuntimeLocks(ex);
        string message = $"unhandled exception on a goroutine started by {Name}: {ex}";
        bool completed;

        lock (m_syncRoot)
        {
            completed = m_finished;

            if (!completed)
            {
                InfrastructureFailed = true;
                AppendLog(message);
            }
        }

        if (completed)
        {
            // The test already reported its terminal event and was counted; record the late failure
            // at the runner level so it still fails the run and is disclosed as an event.
            m_runner.RecordInfrastructureFailure(Name, message);
        }
        else
        {
            FailFromInfrastructure();
        }
    }

    /// <summary>
    /// Records the panic that is about to end the process against this test, and reports the terminal
    /// event its own thread will now never reach.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="RecordGoroutineFailure"/> there is no "and the run continues" here: the
    /// caller is inside the fatal path, so marking the execution finished and reporting its verdict
    /// IS the completion. A panic that arrives after the test already reported is recorded at the
    /// runner level instead, exactly as a late infrastructure failure is.
    /// </remarks>
    internal void RecordGoroutinePanic(string report)
    {
        string message = $"panic on a goroutine started by {Name}{Environment.NewLine}{report}";
        bool completed;
        string output;

        lock (m_syncRoot)
        {
            completed = m_finished;

            if (!completed)
            {
                // Claim the terminal event here: Execute's finally is on a thread that will be
                // killed mid-test, so nothing else will ever report this execution.
                m_finished = true;
                m_failed = true;
                AppendLog(message, terminal: true);
            }

            output = LogOutput();
        }

        if (completed)
        {
            m_runner.RecordInfrastructureFailure(Name, message);
            return;
        }

        m_parent?.FailFromChild();
        m_runner.Report(new TestEvent(m_runner.Package, Name, "fail", 0.0D, output, Source, Line));
        m_runner.Completed(this);
    }

    /// <summary>
    /// Appends one record to this execution's log, bounded. Callers must hold <c>m_syncRoot</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Truncation keeps the HEAD, deliberately. A disclosure is matched by a <c>Contains</c> over
    /// this very text (testConversion.go's signature matching), and the message a signature pins is
    /// the FIRST failure — so dropping the tail of a storm keeps every signature that could ever
    /// have matched, while dropping the head would silently unpin disclosed rows.
    /// </para>
    /// <para>
    /// <paramref name="terminal"/> marks the one record the head-keeping policy must not apply to:
    /// the goroutine-panic report is appended LAST and is the whole reason the report is being
    /// built at all. It still passes <see cref="MaxRecordCharacters"/>, so the worst case stays
    /// bounded at the aggregate plus one record plus the notice.
    /// </para>
    /// </remarks>
    private void AppendLog(string text, bool terminal = false)
    {
        if (text.Length > MaxRecordCharacters)
            text = string.Concat(text.AsSpan(0, MaxRecordCharacters), RecordTruncatedSuffix);

        if (!terminal && m_logCharacters >= MaxLogCharacters)
        {
            m_logsDropped++;
            return;
        }

        m_logs.Add(text);
        m_logCharacters += text.Length + Environment.NewLine.Length;
    }

    /// <summary>
    /// This execution's log as one string — null when it logged nothing, and stating its own
    /// truncation when the cap dropped anything. Callers must hold <c>m_syncRoot</c>.
    /// </summary>
    /// <remarks>
    /// The notice is part of the output rather than a side channel because the output is what every
    /// consumer sees: a reader diagnosing a failure, and results.json. A truncated report that does
    /// not say so is worse than a long one.
    /// </remarks>
    private string? LogOutput()
    {
        if (m_logs.Count == 0)
            return null;

        string output = string.Join(Environment.NewLine, m_logs);

        if (m_logsDropped == 0)
            return output;

        return $"{output}{Environment.NewLine}testing: {m_logsDropped} further log record(s) dropped after {MaxLogCharacters} characters";
    }

    private void Execute(Action<ж<testing_package.T>> action)
    {
        Stopwatch timer = Stopwatch.StartNew();
        m_ownerThread = Environment.CurrentManagedThreadId;
        s_current.Value = this;
        m_runner.Report(new TestEvent(m_runner.Package, Name, "run", Source: Source, Line: Line));

        testing_package.T t = new() { Execution = this };
        try
        {
            action(new StandardBox<testing_package.T>(t));
        }
        catch (TestAbortException)
        {
        }
        catch (GoexitException)
        {
            // The test's goroutine ended without the test function completing — Go's tRunner
            // detects the same state and reports errNilPanicOrGoexit against the test. FailNow's
            // contract IS this (Go: mark failed, then runtime.Goexit) and it arrives above as a
            // TestAbortException, so a GoexitException here means the test body called
            // runtime.Goexit itself. Go fails the test and then panics the whole binary; failing
            // this one test and letting the run finish keeps the rest of the package measurable.
            Log("test executed panic(nil) or runtime.Goexit");
            Fail();
        }
        catch (PanicException ex)
        {
            AbandonRuntimeLocks(ex);
            Log($"panic: {ex.Message}\n{ex.StackTrace}");
            Fail();
        }
        catch (Exception ex) when (RuntimeErrorPanic.TryAsPanic(ex, out PanicException? panic))
        {
            AbandonRuntimeLocks(panic!);
            Log($"panic: {panic!.Message}\n{panic.StackTrace}");
            Fail();
        }
        catch (Exception ex)
        {
            AbandonRuntimeLocks(ex);
            InfrastructureFailed = true;
            Log(ex.ToString());
            FailFromInfrastructure();
        }
        finally
        {
            // Give the -parallel slot back BEFORE waiting on parallel children (Go's ordering):
            // a parallel parent that held its slot while waiting would deadlock its own children
            // under a small -parallel cap.
            if (m_holdsParallelSlot)
            {
                m_holdsParallelSlot = false;
                m_runner.ReleaseParallelSlot();
            }

            // ONE snapshot for both passes: a child added between them would be released and
            // never waited for, which is the lost-child half of the race above.
            TestExecution[] parallelChildren = ParallelChildrenSnapshot();

            foreach (TestExecution child in parallelChildren)
                child.ReleaseParallel();
            foreach (TestExecution child in parallelChildren)
                child.Wait();

            RunCleanups();
            timer.Stop();

            lock (m_syncRoot)
                m_finished = true;

            string terminal = InfrastructureFailed ? "infrastructure-error" : Failed ? "fail" : Skipped ? "skip" : "pass";
            string? output;
            lock (m_syncRoot)
                output = LogOutput();

            m_runner.Report(new TestEvent(m_runner.Package, Name, terminal, timer.Elapsed.TotalSeconds, output, Source, Line));
            m_runner.Completed(this);
        }
    }

    private void RunCleanups()
    {
        while (true)
        {
            Action cleanup;
            lock (m_syncRoot)
            {
                if (m_cleanups.Count == 0)
                    return;
                cleanup = m_cleanups.Pop();
            }

            try
            {
                cleanup();
            }
            catch (TestAbortException)
            {
            }
            catch (Exception ex) when (RuntimeErrorPanic.TryAsPanic(ex, out PanicException? panic))
            {
                lock (m_syncRoot)
                    AppendLog($"cleanup panic: {panic!.Message}\n{panic.StackTrace}");
                FailFromInfrastructure();
            }
            catch (Exception ex)
            {
                InfrastructureFailed = true;
                lock (m_syncRoot)
                    AppendLog($"cleanup failed: {ex}");
                FailFromInfrastructure();
            }
        }
    }

    private void FailFromChild()
    {
        lock (m_syncRoot)
            m_failed = true;
        m_parent?.FailFromChild();
    }

    private void FailFromInfrastructure()
    {
        lock (m_syncRoot)
            m_failed = true;
        m_parent?.FailFromChild();
    }

    /// <summary>
    /// Verifies the caller is the test's own goroutine for operations Go restricts to it
    /// (FailNow/SkipNow/Run/Parallel/Setenv). A violation is recorded as an infrastructure
    /// failure on THIS execution and the operation becomes a no-op — it must never throw.
    /// </summary>
    /// <remarks>
    /// Throwing here would surface inside foreign converted code: golib's GoFunc exception filter
    /// only captures panic-convertible exceptions (its comment: "Non-panic exceptions fail the
    /// filter and propagate unchanged"), converted goroutines are queued on the bare thread pool
    /// (builtin.goǃ), and golib's AppDomain.UnhandledException backstop prints the report to
    /// stderr and exits 2 (like Go) — terminating the whole run mid-flight with NO result files
    /// and every unrelated test killed. Routing to an infrastructure failure keeps the misuse
    /// disclosed (req §6.1) without throwing into code that cannot handle it. (Fail/Error/Log
    /// intentionally have no owner check — Go permits them from any goroutine.)
    /// </remarks>
    private bool TryEnsureOwner(string operation)
    {
        if (Environment.CurrentManagedThreadId == m_ownerThread)
            return true;

        string message = $"testing: {operation} called from a goroutine other than the test goroutine for {Name}";
        bool completed;

        lock (m_syncRoot)
        {
            completed = m_finished;

            if (!completed)
            {
                InfrastructureFailed = true;
                AppendLog(message);
            }
        }

        if (completed)
        {
            // The owning execution already reported its terminal event and was counted — record
            // the late misuse at the runner level so it still fails the run and emits an event.
            m_runner.RecordInfrastructureFailure(Name, message);
        }
        else
        {
            FailFromInfrastructure();
        }

        return false;
    }

    /// <summary>
    /// Rewrites a subtest name the way Go's <c>testing.rewrite</c> does: whitespace folds to
    /// <c>_</c>, and any rune Go does not consider printable becomes its Go ESCAPE \u2014 the body of
    /// <c>strconv.QuoteRune</c>, so U+001A reads <c>\x1a</c>.
    /// </summary>
    /// <remarks>
    /// The escape is not cosmetic; it is the NAME, and the differential oracle pairs results by name.
    /// Folding a non-printable to U+FFFD instead \u2014 which this did \u2014 made every such subtest a matched
    /// pair of one-sided rows (<c>Go="pass" C#=""</c> beside <c>Go="" C#="pass"</c>), and os's
    /// TestReadStdin has two inputs containing U+001A across 462 subtests: 924 lines that read exactly
    /// like a mass failure and were none, on a top-level test that AGREED. Go's own rule is in
    /// testing/match.go (rewrite/isSpace) and strconv/quote.go (IsPrint/appendEscapedRune).
    /// </remarks>
    private static string SanitizeName(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "#00";

        StringBuilder rewritten = new(value.Length);

        for (int i = 0; i < value.Length; i++)
        {
            int rune = char.IsHighSurrogate(value[i]) && i + 1 < value.Length && char.IsLowSurrogate(value[i + 1])
                ? char.ConvertToUtf32(value[i], value[++i])
                : value[i];

            if (IsGoSpace(rune))
                rewritten.Append('_');
            else if (IsGoPrintable(rune))
                rewritten.Append(char.ConvertFromUtf32(rune));
            else
                AppendGoEscape(rewritten, rune);
        }

        return rewritten.ToString();
    }

    // testing/match.go's isSpace \u2014 deliberately its own list rather than unicode.IsSpace, so the
    // rewrite folds exactly the runes Go folds.
    private static bool IsGoSpace(int rune)
    {
        if (rune < 0x2000)
        {
            return rune is '\t' or '\n' or '\v' or '\f' or '\r' or ' ' or 0x85 or 0xA0 or 0x1680;
        }

        if (rune <= 0x200A)
            return true;

        return rune is 0x2028 or 0x2029 or 0x202F or 0x205F or 0x3000;
    }

    // unicode.IsPrint: letters, marks, numbers, punctuation, symbols, and the ASCII space. Space
    // never reaches here (isSpace claims it first), so the ASCII-space clause is omitted.
    private static bool IsGoPrintable(int rune)
    {
        if (rune > 0x10FFFF)
            return false;

        return CharUnicodeInfo.GetUnicodeCategory(char.ConvertFromUtf32(rune), 0) switch
        {
            UnicodeCategory.UppercaseLetter or UnicodeCategory.LowercaseLetter or
            UnicodeCategory.TitlecaseLetter or UnicodeCategory.ModifierLetter or
            UnicodeCategory.OtherLetter or
            UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark or
            UnicodeCategory.EnclosingMark or
            UnicodeCategory.DecimalDigitNumber or UnicodeCategory.LetterNumber or
            UnicodeCategory.OtherNumber or
            UnicodeCategory.ConnectorPunctuation or UnicodeCategory.DashPunctuation or
            UnicodeCategory.OpenPunctuation or UnicodeCategory.ClosePunctuation or
            UnicodeCategory.InitialQuotePunctuation or UnicodeCategory.FinalQuotePunctuation or
            UnicodeCategory.OtherPunctuation or
            UnicodeCategory.MathSymbol or UnicodeCategory.CurrencySymbol or
            UnicodeCategory.ModifierSymbol or UnicodeCategory.OtherSymbol => true,
            _ => false
        };
    }

    // strconv's appendEscapedRune, minus the quote/backslash cases a non-printable rune cannot be.
    private static void AppendGoEscape(StringBuilder builder, int rune)
    {
        switch (rune)
        {
            case '\a': builder.Append("\\a"); return;
            case '\b': builder.Append("\\b"); return;
            case '\f': builder.Append("\\f"); return;
            case '\n': builder.Append("\\n"); return;
            case '\r': builder.Append("\\r"); return;
            case '\t': builder.Append("\\t"); return;
            case '\v': builder.Append("\\v"); return;
        }

        if (rune < ' ' || rune == 0x7F)
            builder.Append("\\x").Append(rune.ToString("x2", CultureInfo.InvariantCulture));
        else if (rune > 0x10FFFF)
            builder.Append("\\ufffd");
        else if (rune < 0x10000)
            builder.Append("\\u").Append(rune.ToString("x4", CultureInfo.InvariantCulture));
        else
            builder.Append("\\U").Append(rune.ToString("x8", CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Builds the per-test directory component of a <see cref="TempDir"/> path -- the sanitized test
    /// name plus a deterministic discriminator that makes the component unique per EXACT name.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="SanitizeName"/> alone is not a safe path component. It is case-PRESERVING but the
    /// filesystem underneath is case-INSENSITIVE on Windows and macOS, so two tests whose names
    /// differ only by case resolve to ONE directory -- <c>TestFileReaddir</c> and
    /// <c>TestFileReadDir</c> in os's suite, both parallel, each one's <c>t.Cleanup</c> deleting the
    /// other's tree. The whitespace folding has the same shape without any help from the filesystem:
    /// <c>"a b"</c> and <c>"a_b"</c> sanitize to the same string.
    /// </para>
    /// <para>
    /// The discriminator is a hash of the exact name rather than a counter because a temp directory
    /// must be STABLE run to run: the same test always gets the same path, so a failed run's tree is
    /// findable afterwards, and the whole <c>.tmp</c> layout is reproducible. A shared counter would
    /// depend on thread interleaving under <c>-parallel</c> and change every run. Go does not need
    /// this at all -- its <c>TempDir</c> goes through <c>os.MkdirTemp</c>, whose random suffix is
    /// unique by construction but deliberately NOT reproducible.
    /// </para>
    /// <para>
    /// A subtest's name carries <c>/</c> separators, which stay separators here: the component
    /// nests, and the discriminator lands on the leaf, which is the only part a sibling can collide
    /// with. Two names that share a case-folded prefix therefore share the parent directory (already
    /// true, and harmless -- cleanup only ever removes its own leaf) and never the leaf.
    /// </para>
    /// </remarks>
    /// <summary>
    /// The characters Go's <c>TempDir</c> mapper admits beyond ASCII alphanumerics -- its
    /// <c>allowed</c> constant, verbatim from <c>testing.go</c> at the pinned go1.23.12. The
    /// trailing SPACE is Go's and is deliberate.
    /// </summary>
    private const string TempDirAllowedAscii = "!#$%&()+,-.=@^_{}~ ";

    /// <summary>
    /// Go's <c>TempDir</c> name mapper: everything the filesystem or a glob cannot carry is
    /// DROPPED, not escaped.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is <c>strings.Map(mapper, c.Name())</c> from <c>(*common).TempDir</c>, and Go's own
    /// comment says why it exists: <i>"Drop unusual characters (such as path separators or
    /// characters interacting with globs) from the directory name to avoid surprising
    /// os.MkdirTemp behavior."</i> An ASCII rune survives only if it is alphanumeric or appears in
    /// <see cref="TempDirAllowedAscii"/>; a non-ASCII rune survives only if it is a letter or a
    /// number. Everything else disappears, <c>/</c> included, so a subtest's temp directory is
    /// FLAT exactly as Go's is.
    /// </para>
    /// <para>
    /// The host used <see cref="SanitizeName"/> here, which is Go's <c>testing.rewrite</c> -- the
    /// mapper for a test NAME, which ESCAPES a non-printable rather than dropping it and leaves
    /// <c>:</c>, <c>*</c>, <c>[</c> and <c>]</c> untouched because a name may legitimately carry
    /// them. Two mappers for two jobs, and using the name one for a PATH is what broke four of
    /// Go's own <c>TestTempDir</c> leaves (measured 2026-09-04): <c>test*</c> and
    /// <c>test:subtest</c> could not be created on Windows at all, and <c>test[]</c> was created
    /// fine and then failed the test's own <c>filepath.Glob</c> with "syntax error in pattern",
    /// because the surviving brackets read as a character class in the DIRECTORY part of the
    /// pattern.
    /// </para>
    /// </remarks>
    internal static string GoTempDirPattern(string value)
    {
        StringBuilder builder = new(value.Length);

        foreach (Rune rune in value.EnumerateRunes())
        {
            int codePoint = rune.Value;

            if (codePoint < 0x80)
            {
                if (codePoint is >= '0' and <= '9' or >= 'a' and <= 'z' or >= 'A' and <= 'Z' ||
                    TempDirAllowedAscii.Contains((char)codePoint))
                {
                    builder.Append(rune);
                }

                continue;
            }

            if (Rune.IsLetter(rune) || Rune.IsNumber(rune))
                builder.Append(rune);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Go's mapped pattern plus a deterministic hash of the ORIGINAL name.
    /// </summary>
    /// <remarks>
    /// The hash is this host's, not Go's, and it does the job Go gets from <c>os.MkdirTemp</c>'s
    /// random suffix. The mapper is LOSSY, so two different subtest names can map to one pattern
    /// (<c>test/..</c> and <c>test..</c> both give <c>TestTempDirtest..</c>) and without the hash
    /// they would share a directory; it is taken over the ORIGINAL value for that reason. A
    /// backslash needs no special handling any more -- the mapper drops it, along with every other
    /// separator.
    /// </remarks>
    internal static string TempDirName(string value)
    {
        return $"{GoTempDirPattern(value)}-{Fnv1a32(value):x8}";
    }

    /// <summary>
    /// FNV-1a over the UTF-16 code units of <paramref name="value"/>, low byte first.
    /// </summary>
    /// <remarks>
    /// Chosen for being short, dependency-free and identical on every platform and every run --
    /// <see cref="string.GetHashCode()"/> is randomized per process and would give a test a new temp
    /// directory on every run. Eight hex digits keep the path short enough not to crowd Windows'
    /// path limit, which a long subtest name already presses against.
    /// </remarks>
    private static uint Fnv1a32(string value)
    {
        const uint OffsetBasis = 2166136261;
        const uint Prime = 16777619;

        uint hash = OffsetBasis;

        unchecked
        {
            foreach (char ch in value)
            {
                hash = (hash ^ (byte)ch) * Prime;
                hash = (hash ^ (byte)(ch >> 8)) * Prime;
            }
        }

        return hash;
    }
}

/// <summary>
/// Control flow, not an error: thrown to abandon a test body at <c>t.FailNow()</c> / <c>t.SkipNow()</c>,
/// and caught at the root of that test's own thread.
/// </summary>
/// <remarks>
/// Go implements both with <c>runtime.Goexit</c>, which unwinds the test's goroutine while still
/// running its deferred calls. An exception is the managed equivalent — it unwinds and runs
/// <c>finally</c> blocks — and it stays on the test's OWN thread, which is why the members that
/// throw it verify thread ownership first. It carries no message because nothing reports it: the
/// verdict was already recorded before the throw, and reaching a handler that treats this as a
/// failure REASON means it escaped the thread it belongs to.
/// </remarks>
internal sealed class TestAbortException : Exception;
