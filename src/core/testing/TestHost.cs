// TestHost.cs - Gbtc
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
using System.Runtime.CompilerServices;
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
/// Entry point for a converted package's test binary: parses the command line, builds the isolated
/// environment one <c>go test</c> invocation gets, runs the suite, and writes the results out.
/// </summary>
/// <remarks>
/// <para>
/// This type owns the RUN, not the tests. <see cref="TestRunner"/> decides what executes and in what
/// order; <see cref="TestExecution"/> is one running test. What lives here is everything that has to
/// happen exactly once around them: the isolated working directory and its fixture staging, the
/// process-wide state <c>testing.Short()</c>/<c>Verbose()</c>/<c>T.Deadline()</c> report, the
/// package deadline, and the result and JUnit files.
/// </para>
/// <para>
/// The process-wide statics are correct rather than convenient: one host run executes per test
/// process, which is exactly Go's model — <c>go test</c> builds and runs one binary per package.
/// </para>
/// <para>
/// The isolated run directory reproduces the SHAPE <c>go test</c> gives a package and not merely a
/// scratch space, because tests observe it. Its last segment is the package's own directory name and
/// its parent holds nothing else, so a test that walks out and back in by name (io/fs's TestGlob
/// globs <c>*/glob.go</c> against <c>os.DirFS("..")</c> and expects <c>fs/glob.go</c>) sees what Go
/// shows it. Culture and TZ are pinned to invariant/UTC for the same reason: Go's own runs are not
/// locale-dependent, and a differential comparison against them cannot be either. All of it is
/// restored in the <c>finally</c>, whatever the run did.
/// </para>
/// </remarks>
public static class TestHost
{
    /// <summary>
    /// Gets whether the current run was started with -short — the value testing.Short() reports.
    /// One host run executes per test process, so process-wide state matches Go's model.
    /// </summary>
    public static bool ShortMode { get; private set; }

    /// <summary>
    /// Gets whether the current run was started with -v — the value testing.Verbose() reports.
    /// </summary>
    public static bool VerboseMode { get; private set; }

    /// <summary>
    /// Gets the UTC instant at which the package deadline (-timeout) expires, or null when no
    /// deadline is in effect. This is what testing.T.Deadline() reports, so it is measured from the
    /// same moment the host starts counting against options.Timeout — not from process start.
    /// </summary>
    public static DateTime? PackageDeadlineUtc { get; private set; }

    public static int Run(TestRegistry registry, string[] args)
    {
        // The go2cs runtime allocation counter is off by default, and this is the ONLY thing that
        // turns it on: testing.AllocsPerRun is its only reader, so a converted application that
        // never runs a test must not pay for it. Enabled at the very top of the run rather than
        // lazily, so that everything a test can observe is inside the counting window.
        AllocationCounter.Enable();

        // The run's own command line, announced once so nothing downstream has to ask the PROCESS
        // for it. See TestFlagBridge.HostCommandLine: in a real converted test binary this array
        // already IS os.Args[1:], so this is inert there; in the in-process MSTest tier it is the
        // difference between parsing this run's arguments and parsing the test RUNNER's.
        TestFlagBridge.HostCommandLine = args;

        TestOptions options;

        try
        {
            options = TestOptions.Parse(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 2;
        }

        ShortMode = options.Short;
        VerboseMode = options.Verbose;

        CultureInfo previousCulture = CultureInfo.CurrentCulture;
        CultureInfo previousUICulture = CultureInfo.CurrentUICulture;
        string previousDirectory = Environment.CurrentDirectory;
        string? previousTimezone = Environment.GetEnvironmentVariable("TZ");

        // A RE-EXEC'D HELPER of an outer host run keeps the state its parent assigned instead of
        // sandboxing again. Go's re-exec'd test binary performs no chdir of its own, and the
        // helper protocol depends on cmd.Dir surviving into the child — os/exec's
        // TestImplicitPWD/TestExplicitPWD assert the child's os.Getwd() against the directory the
        // TEST chose, and a fresh GUID sandbox here answered with its own root instead. The outer
        // host plants this marker in its own environment immediately after creating its sandbox,
        // so every descendant — including one spawned with a cmd.Environ()-derived environment —
        // identifies itself here and leaves the inherited working directory, fixtures and TZ
        // alone. Its working directory is previousDirectory by definition: that IS the directory
        // the parent spawned it in.
        string? inheritedSandbox = Environment.GetEnvironmentVariable(SandboxMarkerVariable);

        // The environment is the PRIMARY transport, and for a spawner that INHERITS one it is the
        // whole story. It is not the whole story for a spawner that FILTERS one. net/http/cgi
        // builds its child's environment from scratch — CGI meta-variables plus, on Windows, only
        // SystemRoot, COMSPEC, PATHEXT and WINDIR carried over from the host — so the marker above
        // never arrives, the child fails to recognize itself, sandboxes, and chdirs away from the
        // cmd.Dir the test chose. cgi's TestDir and TestEnvOverride are the measured witnesses
        // (2026-08-29): both assert the child's os.Getwd() against a directory the test named, and
        // both got a fresh sandbox GUID instead while every other cgi verdict passed, because the
        // CGI behavior itself was right. os/exec could never expose this — exec.Cmd inherits the
        // environment by default — so cgi is simply the first witness, not a special case: any
        // package spawning through an environment-filtering API meets the same wall.
        //
        // So the same question is asked a second way, of a marker FILE that no environment filter
        // can reach. It is consulted ONLY when the variable is absent, which is what keeps this
        // strictly additive: every inheriting spawner answers on the line above and never reaches
        // the file at all.
        if (string.IsNullOrEmpty(inheritedSandbox))
            inheritedSandbox = InheritedSandboxFromMarkerFile(previousDirectory);

        bool helperReExec = !string.IsNullOrEmpty(inheritedSandbox);

        // The isolated run directory reproduces the SHAPE `go test` gives a package, not just a
        // scratch space: its own last segment is the package's directory name, and its parent holds
        // nothing else. A test may walk out of the working directory and back in by name — io/fs's
        // TestGlob globs `*/glob.go` against os.DirFS("..") and expects `fs/glob.go` — which a bare
        // GUID directory answers with the GUID (and, worse, with every SIBLING run still on disk).
        (string runRoot, string workingDirectory) = helperReExec
            ? (inheritedSandbox!, previousDirectory)
            : CreateRunDirectory(registry.Package);

        // Held open for the whole run, and released in the finally: while this handle lives, the
        // file it owns asserts that this run is live and its sandbox is real.
        IDisposable? sandboxMarkerFile = null;

        if (!helperReExec)
        {
            PublishSandboxMarker(runRoot);
            sandboxMarkerFile = PublishSandboxMarkerFile(runRoot);
        }

        options.ResolveOutputPaths(previousDirectory);

        // The results-flush latch is PER RUN: the in-process guard tier runs many hosts in one process,
        // and a latch left true by the previous run would silence this run's exit flush.
        s_resultsWritten = false;
        EventHandler? flushOnProcessExit = null;

        try
        {
            if (!helperReExec)
            {
                // The ancestry goes in FIRST, so the fixture staging that follows can replace any linked
                // component it needs to write into with a real one. GOROOT is what the pipeline exports to
                // both sides; when there is none, staging is skipped and the sandbox is what it always was.
                PackageAncestry.TryStage(Environment.GetEnvironmentVariable("GOROOT"), registry.Package, runRoot, workingDirectory);

                CreateFixtureDirectories(registry.FixtureDirectories, workingDirectory, runRoot);

                // AFTER the run-directory shape and BEFORE the copies. After, because a link at
                // `testdata` replaces the empty directory that pass just created; before, because
                // every copy target's parent is made writable on its way in, and a link is exactly
                // what must never be made writable — staging first is what puts the refusal in
                // front of the write instead of behind it.
                PackageAncestry.StageFixtureLinks(registry.FixtureLinks, Environment.GetEnvironmentVariable("GOROOT"),
                    registry.Package, workingDirectory, runRoot);

                CopyFixtures(registry.Fixtures, workingDirectory, runRoot);
                Environment.CurrentDirectory = workingDirectory;

                // PWD follows the chdir, because on Unix it is the SHELL's job to keep them equal
                // and nothing else will do it here. `go test` starts its binary in the package
                // directory with PWD already naming it, so Go's tests may assume PWD == cwd — and
                // os/exec's TestImplicitPWD asserts exactly that, comparing the PWD entries
                // Cmd.Environ() derives against the working directory it expects. Leaving the
                // inherited value in place points it at whatever directory the pipeline was
                // invoked from, which is neither this run's cwd nor anything a Go test could
                // predict. Published to the converted environment as well as the CLR's, for the
                // reason PublishSandboxMarker gives: Cmd.Environ() reads syscall.envs.
                PublishEnvironmentVariable("PWD", workingDirectory);

                // The helper skips this with the rest: a parent test that hands its child an
                // explicit TZ through cmd.Env must see that value in the child, exactly as Go's
                // helper — which has no TZ logic at all — would show it.
                //
                // DELIBERATELY the CLR-only form, not PublishEnvironmentVariable (2026-08-29). The
                // publishing variant rode into 83ea02659 unannounced alongside the parse
                // relocation, and it is an UNMEASURED behavior change: it would make this pin
                // actually reach converted code on linux/darwin, where the pipeline hands the
                // `go test` side no TZ at all — so the two sides could start disagreeing about the
                // local zone rather than agreeing. That is the TZ arc's own question, with its own
                // measurement in flight; this merge unit carries only what its gates measured.
                Environment.SetEnvironmentVariable("TZ", "UTC");
            }

            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

            // Go's package-level variable initializers run BEFORE main, so a package that declares
            // `var mode = flag.Bool("bogo-mode", ...)` has already put that name on
            // flag.CommandLine by the time its test binary parses anything. The converted analogue
            // is the test package class's static constructor, which the CLR would not run until the
            // first test body executes — long after the host had to decide what its command line
            // meant. Forcing it here restores Go's ORDER, and it is done only when the parse
            // actually met a name the host does not own: every other run keeps the initialization
            // exactly where it was.
            if (options.UnrecognizedFlag is not null)
                InitializePackageUnderTest(registry);

            // Declare this run's command line on the converted flag package, the way testing.Init()
            // declares -test.* before a Go test binary's TestMain runs — otherwise a converted
            // TestMain calling flag.Parse() rejects the host's own arguments ("flag provided but not
            // defined: -json") before a single test executes. Placed HERE, after the isolated
            // environment is in place and before anything from the package under test is touched,
            // because that is the same moment Go's testing.Init() occupies: the test binary is
            // already running where `go test` put it, and no test code has run yet.
            TestFlagBridge.Register(options);

            // NO VERDICT HERE — the unrecognized flag PASSES THROUGH to the converted flag package,
            // which is the only party entitled to rule on it. This block used to answer the name
            // itself ("flag provided but not defined: -x", return 2), reproducing flag's wording and
            // flag's exit code — and that looked right precisely because the code matched. It was
            // still wrong, because it took the decision in the WRONG PLACE and TOO EARLY: Go's test
            // binary reaches exactly one flag.Parse(), by which time the package under test has
            // installed whatever flag.Usage it wants, and Usage is entitled to do something other
            // than exit 2.
            //
            // crypto/tls is the measured witness (2026-08-28). Its TestMain sets
            // `flag.Usage = func() { …; if *bogoMode { os.Exit(89) } }`, and BoGo's runner reads
            // exit 89 as errUnimplemented → SKIP (runner.go:1685, 20380). Go's shim defines ~45 of
            // the ~100 flags the runner uses and INTENDS the rest to exit 89 and skip; answering
            // here turned 1,902 of those into hard failures instead. The class is wider than BoGo —
            // any package whose Usage does anything but flag's default diverged the same way.
            //
            // This is the FIRST of the two ordering defects; the second was the parse itself
            // happening here rather than in M.Run, and the pair is why NEITHER fix alone produced a
            // single 89 (i9's re-run of the first was byte-identical, 1,340/1,902/0). Nothing is
            // lost by deferring: TestFlagBridge.Register above put the host's own vocabulary on the
            // converted flag package and InitializePackageUnderTest put the package's there, so the
            // one parse M.Run performs sees exactly the combined flag set Go's single parse sees —
            // and rejects a genuinely undefined name with Go's message, Go's Usage and Go's status,
            // decided by Go's code.


            TestReporter reporter = new(registry.Package, options.Json, options.Verbose);
            TestRunner runner = new(registry, options, reporter, workingDirectory, runRoot);

            // TEST-HOST-ONLY: the results file is written on EVERY way out of the process, a converted
            // os.Exit included. Go's TestMain convention ends in os.Exit(m.Run()) -- net/http's does on
            // every path (main_test.go:24-29) -- and os.Exit is syscall.Exit is Environment.Exit, which
            // never returns to the completion path below: the verdicts reached stdout (the comparison
            // reads that stream) but the host's own results file was never written, so a row ending
            // in os.Exit had no results tail for the sweep to read (measured 2026-09-04, net/http:
            // both arms of the goroutine-leak pair preserved a comparison record and no results file).
            // Environment.Exit runs AppDomain.ProcessExit on the shutdown thread with
            // Environment.ExitCode set to the code it was given (measured on net10.0), so the flush
            // hangs there. It is a NO-OP on every path that already wrote -- the latch inside
            // WriteResults -- which covers completion, the package timeout, and both fatal-goroutine
            // paths (whose own Environment.Exit(2) would otherwise reach this handler and double-write).
            // Unsubscribed in the finally, so a later run in the same process (the guard tier) cannot
            // flush THIS run's reporter.
            flushOnProcessExit = (_, _) => FlushResultsOnProcessExit(reporter, registry, options);
            AppDomain.CurrentDomain.ProcessExit += flushOnProcessExit;

            // TEST-HOST-ONLY: an unhandled exception escaping a goroutine is attributed to the test
            // that started it, the run's evidence is flushed, and the process then DIES — Go's own
            // semantics, which golib already keeps for a converted program and which the host must
            // not soften.
            //
            // It used to be contained and the run allowed to continue, on the reasoning that one
            // test's stray exception should not cost the whole run. Measured, that reasoning does not
            // hold: recording the failure cannot UNBLOCK whatever the dead goroutine was going to
            // signal. A goroutine that dies before its `wg.Done()` leaves the test parked in
            // `sync.Wait` forever, the package deadline eventually fires, and the timeout path
            // discards every verdict — so containment did not save one test, it lost the entire
            // package AND took the evidence with it.
            //
            // Measured instance (reflect, 2026-08-30): TestOffsetLock's four goroutines each threw
            // `NotImplementedException: addReflectOff` in under a second. Contained, that presented as
            // an UNBOUNDED HANG which ate a 40-minute deadline and truncated reflect's suite to a
            // meaningless 99 pass / 93 fail / 1 skip. The exception was recorded correctly the whole
            // time and nobody could see it. Dying loudly names the defect in one second instead.
            //
            // Go has no quietly-dead goroutine, so there is nothing here to be faithful TO by
            // surviving. Same attribute-then-flush shape as the panic path below; the panic path does
            // not exit explicitly because the unhandled panic itself ends the process, while a
            // contained exception has already been caught and must be ended deliberately.
            Goroutine.ContainUnhandledExceptions(ex => ReportFatalGoroutineException(runner, reporter, registry, options, ex));

            // TEST-HOST-ONLY: a PANIC escaping a goroutine still kills the process — that is Go's
            // behavior and the oracle must keep observing it — but it no longer takes the run's
            // evidence with it. The panic is attributed to the test that started the goroutine and
            // reported WITH its traceback, and every verdict gathered so far is flushed to the result
            // files, which the fatal path would otherwise discard whole (a package that had already
            // passed six tests recorded zero).
            Goroutine.ObserveUnhandledPanic(panic => ReportFatalGoroutinePanic(runner, reporter, registry, options, panic));

            // Set immediately before the clock starts, so testing.T.Deadline() and the Wait below
            // are answering about the same instant.
            PackageDeadlineUtc = options.Timeout > TimeSpan.Zero ? DateTime.UtcNow + options.Timeout : null;

            Task<nint> run = Task.Run(() => RunTests(registry, runner));

            if (!run.Wait(options.Timeout))
            {
                reporter.ReportPackage("timeout", options.Timeout.TotalSeconds, $"package timeout after {options.Timeout}");
                WriteResults(options.ResultFile, registry.Package, options, reporter.Events);
                WriteJUnit(options.JUnitFile, registry.Package, reporter.Events);
                return 1;
            }

            int exitCode = checked((int)run.Result);
            WriteResults(options.ResultFile, registry.Package, options, reporter.Events);
            WriteJUnit(options.JUnitFile, registry.Package, reporter.Events);
            return exitCode;
        }
        catch (Exception ex) when (CrashReport.TryUnwrapPanic(ex, out PanicException? panic, out Exception? thrown))
        {
            // A panic that escaped the run is NOT an infrastructure failure. Go's test binary dies
            // on one with a crash report and status 2, and the oracle has to be able to observe
            // exactly that: runtime/debug's TestSetCrashOutput re-executes this binary, panics
            // inside TestMain, and reads the report back from BOTH the child's stderr and the file
            // debug.SetCrashOutput configured. Reporting the CLR exception instead is what made it
            // read `System.AggregateException: One or more errors occurred. (oops) --->
            // go.PanicException: oops` over a frame list — docs/phase4/DESIGN-crash-report.md.
            //
            // TestMain's panic arrives WRAPPED, from Task.Wait above, which is why the filter
            // unwraps rather than testing the exception's own type.
            //
            // `return 2` rather than Environment.Exit: Go's status for a panicking test binary is 2
            // either way, and returning lets the finally below tear down the isolated run directory.
            CrashReport.Report(panic, thrown);
            return 2;
        }
        catch (Exception ex)
        {
            TestEvent infrastructureError = new(registry.Package, "", "infrastructure-error", Output: ex.ToString());
            if (options.Json)
                Console.WriteLine(JsonSerializer.Serialize(infrastructureError, TestReporter.JsonOptions));
            else
                Console.Error.WriteLine(ex);
            return 2;
        }
        finally
        {
            if (flushOnProcessExit is not null)
                AppDomain.CurrentDomain.ProcessExit -= flushOnProcessExit;

            Environment.CurrentDirectory = previousDirectory;
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUICulture;
            // The CLR-only form, matching the pin above — see its note: the publishing variant is
            // the TZ arc's unmeasured half and does not ride in this merge unit.
            Environment.SetEnvironmentVariable("TZ", previousTimezone);

            try
            {
                // Released BEFORE the sandbox is torn down, and in its own guard so that a failure
                // here cannot cost the tree below: the file's whole meaning is "this run is live
                // and its sandbox exists", and the moment either stops being true it must stop
                // saying so. On Windows the delete is a kernel property of the handle, so it rides
                // out an abnormal exit too; elsewhere a killed host can strand the file, which is
                // exactly the case the reader's liveness check exists to answer.
                sandboxMarkerFile?.Dispose();
            }
            catch
            {
                // Best effort, like the teardown below it: process exit closes the handle anyway.
            }

            try
            {
                // The whole run root, so the package-named directory's private parent goes with it.
                // Junction-aware: a recursive delete does not FOLLOW a link (which is what keeps the
                // real GOROOT safe) but it does not remove one either, so the ancestry's links are
                // unlinked first. A helper re-exec never deletes: its runRoot is the PARENT run's
                // sandbox, which the parent is still using and owns.
                if (!helperReExec)
                {
                    // The write refusal must not outlive the sandbox it guards: a host run in a
                    // process that runs another (the guard tier does exactly that) would otherwise
                    // inherit a protected path that no longer exists.
                    PackageAncestry.ReleaseFixtureLinks();
                    PackageAncestry.Delete(runRoot);
                }
            }
            catch
            {
                // Per-test cleanup failures are reported; final process cleanup is best effort.
            }
        }
    }

    /// <summary>
    /// Runs the package under test's own initialization — the converted form of Go's package-level
    /// variable initializers, which run before main.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The registered tests ARE the package: each is a delegate over one of its converted methods,
    /// so their declaring types are exactly the classes whose static state the package declares
    /// (the internal test variant recompiles the production class, the external one adds
    /// <c>&lt;pkg&gt;_test</c>). Deriving them from the registry rather than taking them as a
    /// parameter keeps the generated host unchanged.
    /// </para>
    /// <para>
    /// Failures are NOT caught. A Go package whose initializer panics dies before main with that
    /// panic, and here the exception reaches Run's handler as an infrastructure error — which says
    /// the same thing about the same moment. Swallowing it would move the failure to whichever test
    /// happened to touch the class first.
    /// </para>
    /// </remarks>
    private static void InitializePackageUnderTest(TestRegistry registry)
    {
        IEnumerable<Type> declaringTypes = registry.Tests
            .Select(test => test.Action.Method.DeclaringType)
            .Append(registry.TestMain?.Method.DeclaringType)
            .OfType<Type>()
            .Distinct();

        foreach (Type declaringType in declaringTypes)
            RuntimeHelpers.RunClassConstructor(declaringType.TypeHandle);
    }

    private static nint RunTests(TestRegistry registry, TestRunner runner)
    {
        // This thread IS the main goroutine for the run. Go's testing.(*M).Run -- and the TestMain
        // that calls it -- execute on the main goroutine, and the package deadline is a timer
        // (testing.go's startAlarm). Run above inverts the threads: it parks the process's main
        // thread on the deadline and hands the run to this one. Without adopting the identity here
        // TestMain ran as `goroutine 0` (a thread with no identity; an id Go never mints) while the
        // REAL goroutine 1 -- registered by golib's module initializer on the parked host thread,
        // which runs no Go code -- was rendered by every runtime.Stack(all) as a frameless foreign
        // block that no leak filter can drop by its text. That is how net/http's TestMain counted
        // the host itself as a leaked goroutine and exited 1 over a 1,345/1,345 record (measured
        // 2026-09-04, Release + tiered, Linux). The deadline path is untouched: the identity is the
        // property, not which thread waits.
        using Goroutine.Scope main = Goroutine.EnterAsMain();

        testing_package.M m = new() { Runner = runner };

        // No TestMain: Go's generated main is `os.Exit(m.Run())`, so this goes through M.Run for
        // the same reason it does there -- M.Run is where the flag parse lives, and a package with
        // custom test flags and no TestMain has nothing else to populate them.
        if (registry.TestMain is null)
            return m.Run();

        registry.TestMain(new StandardBox<testing_package.M>(m));
        return runner.HasRun ? runner.ExitCode : 0;
    }

    // Output-directory folder holding fixtures that reach ABOVE the package. MUST match the
    // converter's SharedFixtureStagingRoot, which emits the matching csproj <Link>.
    private const string SharedFixtureStagingRoot = "go2cs_shared_fixtures";

    // Planted in the host's own environment the moment its sandbox exists, so every descendant
    // process can tell it is a RE-EXEC'D HELPER of this run rather than a fresh host — the value
    // is the outer run root, and its presence is what Run's helper gate keys on. Environment
    // inheritance is the delivery mechanism: it survives cmd.Environ()-derived child environments
    // by construction, which is how the os/exec helpers build theirs.
    private const string SandboxMarkerVariable = "GO2CS_TEST_SANDBOX";

    // The SECONDARY transport's file, written beside the test executable — the one location a child
    // can name with no environment at all, because it derives from the process image rather than
    // from anything a spawner is free to rewrite. See InheritedSandboxFromMarkerFile for why this
    // location, and what stops it from being read by a run it does not belong to.
    private const string SandboxMarkerFileName = ".go2cs-test-sandbox";

    // The converted syscall package: type `go.syscall_package` in assembly `syscall`. Resolved by
    // name, for the reason TestFlagBridge gives at length — the generated test projects set
    // DisableTransitiveProjectReferences, so a `testing` -> `syscall` reference would not deploy
    // syscall.dll beside a host whose own package does not import it.
    private const string SyscallPackageTypeName = "go.syscall_package, syscall";

    /// <summary>
    /// Publishes the sandbox marker so a re-exec'd HELPER of this run can recognize itself.
    /// </summary>
    /// <remarks>
    /// <para>
    /// It has to be published TWICE, and the second half is the one that matters. Setting it with
    /// <see cref="Environment.SetEnvironmentVariable"/> alone reaches this process — which is what
    /// the helper gate reads on the way IN — but it does not reach a child, because a child's
    /// environment is built by <c>Cmd.Environ()</c> from the converted <c>os.Environ()</c>, and
    /// that reads <c>syscall.envs</c>: a slice initialized ONCE from a static field initializer
    /// (<c>envs = runtime_envs()</c>) when the syscall package's static constructor runs, which is
    /// long before this method is reached. A variable set in the CLR's environment afterwards is
    /// therefore invisible to every converted child — measured, not assumed: with only the CLR
    /// half published, os/exec's nine PWD subtests kept failing with the child reporting its own
    /// fresh sandbox GUID.
    /// </para>
    /// <para>
    /// The converted <c>syscall.Setenv</c> is what updates that slice (Go's own implementation
    /// appends the pair to <c>envs</c> and indexes it in <c>env</c>), so calling it is what makes
    /// the marker inheritable. Absent syscall.dll there is nothing to publish into and nothing that
    /// could spawn a child to inherit it, so doing nothing is correct rather than merely safe —
    /// the same argument the flag bridge makes for its own late binding.
    /// </para>
    /// </remarks>
    private static void PublishSandboxMarker(string runRoot) =>
        PublishEnvironmentVariable(SandboxMarkerVariable, runRoot);

    /// <summary>
    /// Publishes the sandbox marker a second way — as a FILE beside the test executable — so a
    /// helper re-exec'd by a spawner that builds its child a clean environment can still recognize
    /// itself.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The file lives in <see cref="AppContext.BaseDirectory"/> because that is the one directory
    /// both sides can name with NO environment: it derives from the running image, and a re-exec'd
    /// helper is by construction the same image. The working directory cannot serve — cgi's own
    /// TestDir spawns one child with <c>Dir</c> inside the parent's sandbox and another with no
    /// <c>Dir</c> at all, which cgi resolves to the executable's directory — and argv cannot serve
    /// either, because the child's argv is the CGI contract's, not the host's.
    /// </para>
    /// <para>
    /// The handle is held for the run's whole life with <see cref="FileOptions.DeleteOnClose"/>, so
    /// on Windows the file is a kernel-level assertion that its owner is still running: it goes
    /// away when the last handle closes, including on a kill, which is the failure mode this
    /// repository treats as routine. That is the STALENESS half. Elsewhere DeleteOnClose is managed
    /// cleanup and a killed host can strand the file, so the reader validates liveness rather than
    /// trusting existence.
    /// </para>
    /// <para>
    /// <c>CreateNew</c>, not <c>Create</c>: if a marker is already there the honest reading is that
    /// another host of this same executable owns it, and truncating its claim would be worse than
    /// declining to publish — a run without this file simply behaves as every run did before it
    /// existed. The one exception is a marker whose owner is demonstrably gone, which is reclaimed
    /// exactly the way <c>PackageAncestry</c> reclaims an abandoned sandbox, so a stranded file on a
    /// platform without kernel-backed delete cannot disable the transport forever.
    /// </para>
    /// </remarks>
    private static IDisposable? PublishSandboxMarkerFile(string runRoot)
    {
        string marker = Path.Combine(AppContext.BaseDirectory, SandboxMarkerFileName);

        try
        {
            FileStream? stream = TryCreateMarker(marker, runRoot);

            if (stream is null && !MarkerOwnerIsLive(marker))
            {
                // Only a marker whose owner is provably gone is cleared, and only then is the
                // create retried — once. Everything uncertain leaves the file alone.
                File.Delete(marker);
                stream = TryCreateMarker(marker, runRoot);
            }

            return stream;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            // Losing this costs the file transport and nothing else: the environment variable is
            // still published, so every inheriting spawner behaves exactly as before.
            return null;
        }

        static FileStream? TryCreateMarker(string path, string runRoot)
        {
            try
            {
                using Process self = Process.GetCurrentProcess();

                // Shared for read and delete so a child can read it while the owner holds it, and
                // so the DeleteOnClose teardown is not blocked by a reader.
                FileStream stream = new(path, FileMode.CreateNew, FileAccess.Write,
                    FileShare.ReadWrite | FileShare.Delete, bufferSize: 1, FileOptions.DeleteOnClose);

                // Line-separated, because a run root may contain spaces and a PID and a process name
                // may not.
                byte[] payload = Encoding.UTF8.GetBytes($"{self.Id}\n{self.ProcessName}\n{runRoot}\n");
                stream.Write(payload, 0, payload.Length);
                stream.Flush();
                return stream;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Reads the sandbox marker FILE, and answers with the owning run's root only when this process
    /// is genuinely one of that run's re-exec'd helpers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the half that must not be generous. A marker honored by a run it does not belong to
    /// would make that run skip its sandbox entirely and execute in whatever directory it was
    /// started in — un-sandboxing a whole package rather than failing loudly — so every one of the
    /// checks below has to hold, and anything unreadable, unparseable or merely uncertain answers
    /// null and lets the caller sandbox normally. That is the opposite polarity from
    /// <c>PackageAncestry</c>'s liveness test, which resolves doubt towards "alive" because there
    /// the risk runs the other way (deleting a running sibling's tree).
    /// </para>
    /// <para>
    /// <b>Staleness</b> is answered twice: the owner's handle carries DeleteOnClose, and the owner
    /// is separately checked to be a live process whose name still matches, so neither a crashed
    /// host on a platform without kernel-backed delete nor a recycled PID can vouch for a run that
    /// has ended.
    /// </para>
    /// <para>
    /// <b>Collision</b> — a second, genuinely fresh host of the SAME executable, running at the same
    /// time — is answered by containment. A helper is spawned into a directory its parent chose, and
    /// the only two the parent controls are its own sandbox and the directory holding the
    /// executable; those are exactly the two cgi produces (<c>Dir</c> set, and <c>Dir</c> unset,
    /// which cgi resolves to the executable's directory). A fresh host is started by the pipeline in
    /// the package's own source directory, which is neither, so it is rejected. The residue this
    /// leaves is narrow and worth stating plainly: a second host of the same published executable,
    /// started CONCURRENTLY with the first, from inside the first's sandbox or from the publish
    /// directory itself, would be taken for a helper. The pipeline cannot produce that — it launches
    /// from the package directory — so it takes a deliberate hand-run second instance, and what it
    /// costs is that the second run does not sandbox, which is visible rather than silent.
    /// </para>
    /// </remarks>
    private static string? InheritedSandboxFromMarkerFile(string workingDirectory)
    {
        string markerDirectory = AppContext.BaseDirectory;
        string marker = Path.Combine(markerDirectory, SandboxMarkerFileName);

        try
        {
            if (!TryReadMarker(marker, out int ownerId, out string ownerName, out string runRoot))
                return null;

            // A host must never read its OWN marker back. Within one run the read happens before the
            // write so it cannot, but a process that runs one host after another (the guard tier
            // does exactly that) could otherwise meet a handle its own earlier run had not yet
            // released.
            using (Process self = Process.GetCurrentProcess())
            {
                if (ownerId == self.Id)
                    return null;
            }

            if (!OwnerIsLive(ownerId, ownerName) || !Directory.Exists(runRoot))
                return null;

            // The containment test: this process was started somewhere the owner controls.
            if (!IsWithin(workingDirectory, runRoot) && !IsWithin(workingDirectory, markerDirectory))
                return null;

            return runRoot;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return null;
        }
    }

    // Reads the marker's three lines. Every malformed shape answers false, because a marker that
    // cannot be understood is a marker that cannot be obeyed.
    private static bool TryReadMarker(string marker, out int ownerId, out string ownerName, out string runRoot)
    {
        ownerId = 0;
        ownerName = "";
        runRoot = "";

        if (!File.Exists(marker))
            return false;

        string text;

        try
        {
            // FileShare.Delete matters as much as the read share: the owner holds this open with
            // DeleteOnClose, and without it the open is refused.
            using FileStream stream = new(marker, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete);
            using StreamReader reader = new(stream, Encoding.UTF8);
            text = reader.ReadToEnd();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return false;
        }

        string[] lines = text.Split('\n');

        if (lines.Length < 3 || !int.TryParse(lines[0].Trim(), out ownerId))
            return false;

        ownerName = lines[1].Trim();
        runRoot = lines[2].Trim();

        return ownerName.Length > 0 && runRoot.Length > 0;
    }

    private static bool MarkerOwnerIsLive(string marker) =>
        TryReadMarker(marker, out int ownerId, out string ownerName, out _) && OwnerIsLive(ownerId, ownerName);

    // The process NAME is compared alongside the id for the reason PackageAncestry gives: a recycled
    // PID must not be able to vouch for a run that has ended. Uncertainty answers false here —
    // see InheritedSandboxFromMarkerFile on why this side resolves doubt towards "sandbox normally".
    private static bool OwnerIsLive(int ownerId, string ownerName)
    {
        try
        {
            using Process owner = Process.GetProcessById(ownerId);
            return string.Equals(owner.ProcessName, ownerName, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception)
        {
            return false;
        }
    }

    // Path containment, asked the way the rest of this file asks it: case-insensitively, over fully
    // resolved paths, with the root's own directory counting as inside itself.
    private static bool IsWithin(string candidate, string root)
    {
        try
        {
            string full = Path.TrimEndingDirectorySeparator(Path.GetFullPath(candidate));
            string parent = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));

            return full.Equals(parent, StringComparison.OrdinalIgnoreCase) ||
                   full.StartsWith(parent + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or NotSupportedException)
        {
            return false;
        }
    }

    /// <summary>
    /// Sets an environment variable in BOTH environments this process has — the CLR's, which the
    /// host itself reads, and the converted <c>syscall</c> package's, which is what a child
    /// inherits.
    /// </summary>
    /// <inheritdoc cref="PublishSandboxMarker" path="/remarks"/>
    private static void PublishEnvironmentVariable(string name, string? value)
    {
        // A null value CLEARS on both sides: that is what the TZ restore asks for when the run
        // inherited no TZ at all, and leaving a stale "UTC" behind would be a different bug from
        // the one this method exists to fix.
        Environment.SetEnvironmentVariable(name, value);

        try
        {
            Type? syscallPackage = Type.GetType(SyscallPackageTypeName, throwOnError: false);

            if (syscallPackage is null)
                return;

            if (value is null)
            {
                MethodInfo? unsetenv = syscallPackage.GetMethod(
                    "Unsetenv",
                    BindingFlags.Public | BindingFlags.Static,
                    binder: null,
                    types: [typeof(@string)],
                    modifiers: null);

                unsetenv?.Invoke(null, [(@string)name]);
                return;
            }

            MethodInfo? setenv = syscallPackage.GetMethod(
                "Setenv",
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: [typeof(@string), typeof(@string)],
                modifiers: null);

            setenv?.Invoke(null, [(@string)name, (@string)value]);
        }
        catch (Exception ex)
        {
            // A failure here costs this one variable and nothing else: the run continues, and
            // whatever reads it behaves exactly as it did before this was published.
            Console.Error.WriteLine($"testing: could not publish {name} to the converted environment: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates this run's isolated directory pair — the run root and the package working directory
    /// inside it — under the first base that will actually accept one.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The temp directory is the right FIRST choice and the wrong ONLY choice, because it is under
    /// test control: a Go test may repoint TMP/TEMP at anything, including somewhere that does not
    /// exist. os's TestRootDirAsTemp re-execs the test binary with TMP and TEMP set to an
    /// intentionally UNMOUNTED drive root — <c>findUnusedDriveLetter</c> picks a letter precisely
    /// because <c>os.Stat</c> says it is not there — to check what <c>os.TempDir</c> reports. Go's
    /// test binary needs no scratch space of its own and does not care; this host does, and it died
    /// in startup with <c>DirectoryNotFoundException</c> before running a single test, which the
    /// parent then read as a child that produced no output.
    /// </para>
    /// <para>
    /// So the host's own isolation must not depend on an environment variable the suite it is
    /// running is free to rewrite. The executable's own directory is the fallback: it exists by
    /// construction (the host is running out of it) and is writable wherever the build put it.
    /// </para>
    /// <para>
    /// The temp path is also DECLINED outright when it resolves inside the Windows directory. That
    /// is not a hypothetical: <c>GetTempPath</c> falls back to <c>%TMP%</c>, then <c>%TEMP%</c>,
    /// then <c>%USERPROFILE%</c>, and finally to the Windows directory itself, so a child spawned
    /// with a filtered environment — cgi's carries neither TMP nor TEMP — asks for a temp directory
    /// and is handed <c>C:\WINDOWS</c>. Sandboxes then accumulate as <c>C:\WINDOWS\go2cs-tests\…</c>,
    /// which succeeds or fails purely on whether the run happens to be elevated; the census that
    /// found this counted 56 such roots, and cgi's own TestDir reported one in its failure message
    /// (2026-08-29). The last resort of that chain is not a scratch directory in any useful sense —
    /// it is shared, elevation-gated, and the wrong place for a test's private tree — so the
    /// executable's directory, which this method already trusts, is strictly the better answer.
    /// </para>
    /// </remarks>
    private static (string runRoot, string workingDirectory) CreateRunDirectory(string package)
    {
        Exception? firstFailure = null;

        foreach (string root in CandidateRunRoots())
        {
            // The isolated run directory reproduces the SHAPE `go test` gives a package, not just a
            // scratch space: its own last segment is the package's directory name, and its parent
            // holds nothing else. A test may walk out of the working directory and back in by name —
            // io/fs's TestGlob globs `*/glob.go` against os.DirFS("..") and expects `fs/glob.go` —
            // which a bare GUID directory answers with the GUID (and, worse, with every SIBLING run
            // still on disk).
            string runRoot = Path.Combine(root, "go2cs-tests", SanitizePath(package), Guid.NewGuid().ToString("N"));
            string workingDirectory = Path.Combine(runRoot, PackageDirectoryPath(package));

            try
            {
                Directory.CreateDirectory(workingDirectory);
                return (runRoot, workingDirectory);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException)
            {
                firstFailure ??= ex;
            }
        }

        throw new InvalidOperationException(
            "testing: could not create an isolated run directory under the temp path or the test binary's own directory",
            firstFailure);
    }

    // The bases CreateRunDirectory will try, in order. The temp path leads because it is the right
    // first choice; it is omitted entirely when it is the Windows directory, for the reason
    // CreateRunDirectory states at length. Dropping it rather than ranking it lower is deliberate:
    // there is no condition under which writing a test sandbox into the Windows directory is the
    // answer, and the executable's own directory is always available.
    private static IEnumerable<string> CandidateRunRoots()
    {
        string temp = Path.GetTempPath();

        if (!IsWindowsDirectory(temp))
            yield return temp;

        yield return AppContext.BaseDirectory;
    }

    // True only when this is Windows AND the path is the Windows directory or inside it. Every
    // uncertainty answers false, which leaves the candidate list exactly as it was before this
    // check existed.
    private static bool IsWindowsDirectory(string path)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return false;

        try
        {
            string windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

            return windows.Length > 0 && IsWithin(path, windows);
        }
        catch (Exception)
        {
            return false;
        }
    }

    // Reproduces the package directory's own SHAPE: `go test` runs a package where the sibling
    // packages nested under it are present as subdirectories, so a test that asks what its working
    // directory contains — os's TestReadDir looks for the `exec` directory beside `read_test.go` —
    // must find them here too. Names only, created empty: the name is what such a test observes, and
    // a sibling package's files are staged by that package's own run. (testdata, when the suite has
    // one, is created here too and then filled in by CopyFixtures — CreateDirectory is idempotent.)
    private static void CreateFixtureDirectories(IReadOnlyList<string> directories, string workingDirectory, string runRoot)
    {
        foreach (string name in directories)
        {
            string target = Path.GetFullPath(Path.Combine(workingDirectory, name));

            // A directory NAME never escapes the package directory; anything that resolves outside
            // it did not come from the converter's enumeration and is not created.
            if (!target.StartsWith(workingDirectory, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"fixture directory escapes run root: {name}");

            PackageAncestry.EnsureWritable(target, runRoot);
        }
    }

    private static void CopyFixtures(IReadOnlyList<string> fixtures, string workingDirectory, string runRoot)
    {
        int staged = 0;

        foreach (string relativePath in fixtures)
        {
            string normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);

            // Go shares large fixtures between sibling packages rather than duplicating them:
            // compress/{flate,zlib,lzw} all read ../testdata/{e,pi,gettysburg}.txt, and flate also
            // reads ../../testdata/Isaac.Newton-Opticks.txt. Such a path cannot keep its shape under
            // the build output, so the csproj links it to <root>/up<N>/<tail>; restore the true
            // relative path here. The TARGET lands inside runRoot rather than the working directory
            // — which is exactly why the working directory mirrors the whole import path.
            string source = SharedFixtureStagingParts(relativePath) is var (up, tail) && up > 0
                ? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, SharedFixtureStagingRoot, $"up{up}", tail.Replace('/', Path.DirectorySeparatorChar)))
                : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, normalized));

            string target = Path.GetFullPath(Path.Combine(workingDirectory, normalized));

            // The run root is the containment boundary, not the working directory: a shared fixture
            // legitimately resolves to a SIBLING of the package directory inside the sandbox.
            if (!source.StartsWith(AppContext.BaseDirectory, StringComparison.OrdinalIgnoreCase) ||
                !target.StartsWith(runRoot, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"fixture escapes run root: {relativePath}");

            // A RELOCATED copy of the single-file host has no fixture sources beside it — the
            // os/exec-style tests copy the lone executable to a temp directory and re-exec it,
            // exactly as they do Go's statically linked test binary, which carries no fixtures
            // either. Go's shape is that the binary STARTS and a test missing its testdata fails
            // at its own read with a file-not-found; a startup throw here instead killed the
            // helper-process reentry before TestMain ever ran. Skip what is absent (after the
            // containment check above — an escaping path is still a defect) and let each test
            // meet the same ENOENT Go's copy would hand it.
            if (!File.Exists(source))
                continue;

            // The ancestry view may hold a LINK at the fixture's parent — compress/{flate,zlib,lzw}
            // all stage into `../testdata` — and writing through one would put staged fixtures inside
            // the real Go installation. EnsureWritable makes every component below the run root a
            // real directory first.
            PackageAncestry.EnsureWritable(Path.GetDirectoryName(target)!, runRoot);
            File.Copy(source, target, true);
            staged++;
        }

        // …but a suite that declares fixtures and stages NONE of them is not that case, and it is
        // not a shrug either: it means the staging path itself is broken, and every test that reads
        // a fixture is about to fail its own read for a reason no gate would attribute. That is
        // exactly how time's banked TestLoadLocationFromTZDataSlim (pass/pass) reached master
        // failing on the published path — R found it by A/B, because nothing in the harness said a
        // word (2026-08-29). The per-file skip above stays for the relocated lone copy; the
        // ALL-of-them case becomes the gate failure it should always have been.
        //
        // The discriminator is the host's own directory, because both situations reach here. A
        // relocated copy is one executable someone copied out on its own; a published or built host
        // sits among its dependencies and staged sources. So "alone" means alone: anything more
        // than the executable itself says the fixtures were supposed to be here and are not.
        if (fixtures.Count > 0 && staged == 0 && !HostIsLoneRelocatedCopy())
        {
            throw new InvalidOperationException(
                $"fixture staging found none of the {fixtures.Count} fixture(s) this suite declares, " +
                $"under '{AppContext.BaseDirectory}' — the run would proceed with an empty testdata and " +
                "fail each reader with a bare file-not-found. This is a broken build/publish staging " +
                "path, not a missing test input.");
        }
    }

    // Whether this host is a lone executable someone copied out — os/exec's TestCommand and
    // TestLookPathWindows do exactly that, mirroring what they do to Go's statically linked test
    // binary, and such a copy legitimately carries no fixtures. Counting entries rather than
    // probing for a marker keeps it honest for both the single-file shape (one file) and any
    // future one, and it never mistakes a real host for a copy: a published host's directory holds
    // its dependencies and staged sources, a built one holds its whole output.
    private static bool HostIsLoneRelocatedCopy()
    {
        try
        {
            return Directory.EnumerateFileSystemEntries(AppContext.BaseDirectory).Take(2).Count() <= 1;
        }
        catch (Exception)
        {
            // Unreadable base directory: say NOT a lone copy, so the louder branch wins. A wrong
            // guess here fails a run that would otherwise have failed each test individually — the
            // safer direction of the two.
            return false;
        }
    }

    // Splits a fixture path that reaches above the package into the levels it ascends and the
    // remainder ("../testdata/e.txt" -> (1, "testdata/e.txt")); (0, path) for anything at or below
    // the package. The level count is part of the staging key because two different ancestors can
    // hold a same-named file. Mirrors the converter's sharedFixtureStagingParts.
    private static (int Up, string Tail) SharedFixtureStagingParts(string fixture)
    {
        int up = 0;
        string tail = fixture;

        while (tail.StartsWith("../", StringComparison.Ordinal))
        {
            up++;
            tail = tail["../".Length..];
        }

        return (up, tail);
    }

    /// <summary>
    /// Says what died and writes the run's evidence out, in the moment between a panic escaping a
    /// goroutine root and the process ending on it.
    /// </summary>
    /// <remarks>
    /// Ordering is the whole point: the attribution has to be recorded BEFORE the files are written,
    /// or the panic itself is the one verdict the files do not carry. Everything here is best-effort
    /// — the process is already dying, and a failure to write must not replace the panic's own report
    /// with a report about the writer.
    /// </remarks>
    private static void ReportFatalGoroutinePanic(TestRunner runner, TestReporter reporter, TestRegistry registry, TestOptions options, PanicException panic)
    {
        try
        {
            runner.ReportGoroutinePanic(panic);
            WriteResults(options.ResultFile, registry.Package, options, reporter.Events);
            WriteJUnit(options.JUnitFile, registry.Package, reporter.Events);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"go2cs test host: could not record the goroutine panic: {ex}");
        }
    }

    /// <summary>
    /// Ends the run on an unhandled NON-panic exception that escaped a goroutine: attribute it to the
    /// owning test, flush the evidence, say so in Go's shape, and exit.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The exit is the point. A goroutine that dies takes with it whatever it was going to signal —
    /// most often a <c>WaitGroup.Done</c> — so any goroutine failure that is merely RECORDED can still
    /// park the test forever. That is not a hypothetical: it is how one sub-second
    /// <c>NotImplementedException</c> became a 40-minute deadline burn and truncated a whole package's
    /// verdicts (see the install site).
    /// </para>
    /// <para>
    /// Exit code 2 matches Go's status for an unrecovered panic, which is the nearest thing Go has to
    /// this situation — in Go a failure of this kind IS a panic, and the process dies. The message
    /// goes to stderr in Go's shape so a comparison run reads it the way it reads Go's.
    /// </para>
    /// <para>
    /// Flushing BEFORE exiting is what the panic path had to learn: the fatal route otherwise discards
    /// the whole run's evidence, and a package that had already passed six tests recorded zero.
    /// </para>
    /// </remarks>
    /// <remarks>
    /// <para>
    /// <paramref name="exit"/> exists so the DEATH is assertable. The other two thirds —
    /// attribution and the flush — are observable from their own artifacts, but "and then the
    /// process ended with 2" is the half that actually converts a hang into a red, and a test that
    /// called <see cref="Environment.Exit"/> would take its own host down. Production passes null
    /// and gets <c>Environment.Exit</c>, so the shipped path is byte-identical; only a guard supplies
    /// anything else. Test-visible seams are a cost, and this one is paid deliberately rather than
    /// leaving the exit code resting on a manual repro.
    /// </para>
    /// </remarks>
    private static void ReportFatalGoroutineException(TestRunner runner, TestReporter reporter, TestRegistry registry, TestOptions options, Exception failure, Action<int>? exit = null)
    {
        try
        {
            // Attribution first — this is the same recording the contained path always did, and it is
            // what names the owning test in the results.
            runner.ContainGoroutineException(failure);
            WriteResults(options.ResultFile, registry.Package, options, reporter.Events);
            WriteJUnit(options.JUnitFile, registry.Package, reporter.Events);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"go2cs test host: could not record the fatal goroutine exception: {ex}");
        }

        try
        {
            // Go's shape: the headline, a blank line, then the detail. A converted program's
            // unrecovered panic reaches stderr the same way.
            Console.Error.WriteLine($"panic: {failure.Message}");
            Console.Error.WriteLine();
            Console.Error.WriteLine(failure.ToString());
            Console.Error.Flush();
            Console.Out.Flush();
        }
        catch
        {
            // Reporting the death must never be what prevents it.
        }

        (exit ?? Environment.Exit)(2);
    }

    /// <summary>
    /// The guard seam for <see cref="ReportFatalGoroutineException"/>: same path, with the process
    /// death handed to <paramref name="exit"/> instead of taken.
    /// </summary>
    /// <remarks>
    /// Internal rather than public, and named for what it is. A guard that reached the real overload
    /// would kill the test host on its first assertion; one that reimplemented the sequence would
    /// guard a copy rather than the shipped code, which is the failure this whole arc was about.
    /// </remarks>
    internal static void ReportFatalGoroutineExceptionForGuard(TestRunner runner, TestReporter reporter, TestRegistry registry, TestOptions options, Exception failure, Action<int> exit) =>
        ReportFatalGoroutineException(runner, reporter, registry, options, failure, exit);

    // Set by WriteResults the moment the results file is written, reset by Run at its start. Read
    // by the ProcessExit flush, which must never write over a file a completing, timing-out or
    // dying run has already written -- those paths are the record; the flush is only for an exit
    // that bypassed them (a converted os.Exit).
    private static volatile bool s_resultsWritten;

    /// <summary>
    /// Writes the run's evidence on a process exit that reached none of the host's own write paths
    /// -- a converted <c>os.Exit</c> -- stating the exit in the record.
    /// </summary>
    /// <remarks>
    /// The terminal event is Go's own shape for this exit: <c>go test -json</c> carries the PASS line
    /// <c>testing.M.Run</c> printed AND the <c>fail</c> action <c>go test</c> appends when the binary's
    /// status is non-zero, so a package that reported <c>pass</c> and then exited 1 carries both, in
    /// that order. Best-effort throughout: the process is already ending, and a failure to write
    /// must not become the thing that is reported about it.
    /// </remarks>
    private static void FlushResultsOnProcessExit(TestReporter reporter, TestRegistry registry, TestOptions options, int? exitCode = null)
    {
        if (s_resultsWritten)
            return;

        int code = exitCode ?? Environment.ExitCode;

        try
        {
            // RECORDED, never printed: the binary's stdout must stay what the converted program left
            // there. A helper process re-executed by os/exec's tests has its stdout read back by the
            // test that spawned it, and the first form of this flush -- ReportPackage, which prints --
            // appended `PASS ... exit status 0 ...` to every helper's output and failed twenty of
            // os/exec's verdicts on a control run (2026-09-04). Go's binary prints nothing on os.Exit;
            // the `fail` action a non-zero status implies is `go test`'s to append, and here the
            // comparison derives it from the exit status exactly as `go test` does.
            reporter.RecordPackage(code == 0 ? "pass" : "fail", output: $"exit status {code}: the process ended before the host completed (os.Exit)");
            WriteResults(options.ResultFile, registry.Package, options, reporter.Events);
            WriteJUnit(options.JUnitFile, registry.Package, reporter.Events);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"go2cs test host: could not write the results on process exit: {ex}");
        }
    }

    /// <summary>
    /// The guard seam for <see cref="FlushResultsOnProcessExit"/>: the same path with the exit
    /// status handed in, since a guard cannot end its own process to observe the real one.
    /// </summary>
    internal static void FlushResultsOnProcessExitForGuard(TestReporter reporter, TestRegistry registry, TestOptions options, int exitCode) =>
        FlushResultsOnProcessExit(reporter, registry, options, exitCode);

    /// <summary>
    /// Resets the per-run results latch for a guard that drives the flush without a Run of its own.
    /// </summary>
    internal static void ResetResultsLatchForGuard() => s_resultsWritten = false;

    private static void WriteResults(string? path, string package, TestOptions options, IReadOnlyList<TestEvent> events)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            object result = new
            {
                schemaVersion = 1,
                package,
                environment = new
                {
                    dotnetRuntime = RuntimeInformation.FrameworkDescription,
                    culture = CultureInfo.CurrentCulture.Name,
                    timezone = Environment.GetEnvironmentVariable("TZ"),
                    shuffleSeed = options.ShuffleSeed,
                    // The Go-side comparison record carries the SAME two fields (testEnvironmentRecord,
                    // testConversion.go), derived from the options that chose this publish/run
                    // configuration. This side records what the host actually IS and actually ran
                    // with, observed rather than assumed -- a compile-time constant baked in by which
                    // `dotnet publish -c` built this binary, and a runtime read of the exact
                    // environment variable testHostRunEnv sets, so the two readings can never silently
                    // drift apart even if something outside the pipeline changes the environment.
#if DEBUG
                    configuration = "Debug",
#else
                    configuration = "Release",
#endif
                    tiered = Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") != "0"
                },
                events
            };
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
            File.WriteAllText(path, JsonSerializer.Serialize(result, TestReporter.JsonOptions));
            s_resultsWritten = true;
        }
    }

    private static void WriteJUnit(string? path, string package, IReadOnlyList<TestEvent> events)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        TestEvent[] terminal = events
            .Where(testEvent => testEvent.Test.Length > 0 && testEvent.Action is "pass" or "fail" or "skip" or "timeout" or "infrastructure-error")
            .ToArray();
        int failures = terminal.Count(testEvent => testEvent.Action is "fail" or "timeout");
        int errors = terminal.Count(testEvent => testEvent.Action == "infrastructure-error");
        int skipped = terminal.Count(testEvent => testEvent.Action == "skip");

        XElement suite = new("testsuite",
            new XAttribute("name", package),
            new XAttribute("tests", terminal.Length),
            new XAttribute("failures", failures),
            new XAttribute("errors", errors),
            new XAttribute("skipped", skipped),
            new XAttribute("time", terminal.Sum(testEvent => testEvent.Elapsed).ToString("0.######", CultureInfo.InvariantCulture)));

        foreach (TestEvent testEvent in terminal)
        {
            XElement testCase = new("testcase",
                new XAttribute("classname", package),
                new XAttribute("name", XmlSanitize(testEvent.Test)),
                new XAttribute("time", testEvent.Elapsed.ToString("0.######", CultureInfo.InvariantCulture)));

            if (testEvent.Action == "skip")
                testCase.Add(new XElement("skipped", new XAttribute("message", XmlSanitize(testEvent.Output ?? "skipped"))));
            else if (testEvent.Action is "fail" or "timeout")
                testCase.Add(new XElement("failure", new XAttribute("message", testEvent.Action), XmlSanitize(testEvent.Output ?? "")));
            else if (testEvent.Action == "infrastructure-error")
                testCase.Add(new XElement("error", new XAttribute("message", "infrastructure-error"), XmlSanitize(testEvent.Output ?? "")));

            suite.Add(testCase);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        new XDocument(new XElement("testsuites", suite)).Save(path);
    }

    /// <summary>
    /// Replaces characters XML 1.0 cannot carry with visible \uXXXX escape text. Real Go test
    /// logs legitimately contain them — unicode/utf8's own tests log U+FFFE/U+FFFF data — and an
    /// unsanitized XDocument.Save throws AFTER the run completed, downgrading a finished suite to
    /// an infrastructure error with no JUnit file at all.
    /// </summary>
    private static string XmlSanitize(string value)
    {
        StringBuilder? sanitized = null;

        for (int i = 0; i < value.Length; i++)
        {
            char ch = value[i];
            bool valid;

            if (char.IsHighSurrogate(ch))
                valid = i + 1 < value.Length && char.IsLowSurrogate(value[i + 1]);
            else if (char.IsLowSurrogate(ch))
                valid = i > 0 && char.IsHighSurrogate(value[i - 1]);
            else
                valid = ch is '\t' or '\n' or '\r' || (ch >= 0x20 && ch <= 0xD7FF) || (ch >= 0xE000 && ch <= 0xFFFD);

            if (valid)
            {
                sanitized?.Append(ch);
                continue;
            }

            sanitized ??= new StringBuilder(value[..i], value.Length + 8);
            sanitized.Append(CultureInfo.InvariantCulture, $"\\u{(int)ch:x4}");
        }

        return sanitized?.ToString() ?? value;
    }

    // The package's directory path under the run root, mirroring its whole Go import path BENEATH a
    // `src` level ("compress/flate" -> "src\compress\flate"). The last element is what `go test`
    // makes the working directory's base name; the ANCESTORS matter too, because a package that reads
    // a shared fixture ("../testdata/e.txt", "../../testdata/Isaac.Newton-Opticks.txt") needs those
    // levels to exist INSIDE the sandbox for the relative open() to resolve. Mirroring the import
    // path gives exactly Go's own layout, so the depth is always sufficient and never guessed.
    //
    // `src` is a level of GOROOT, so it is a level here: without it, a package's climb out of its own
    // tree lands one short. internal/godebugs reads ../../../doc/godebug.md, which is GOROOT's `doc`
    // beside `src` and not a fourth level above the package; internal/testenv stats ../../../bin/go
    // the same way. It is also where the Go toolchain's module walk finds `module std`, which is what
    // internal/coverage/cfile needs for `go test` inside a testdata directory to resolve at all.
    // PackageAncestry fills these levels with the real GOROOT's content.
    private static string PackageDirectoryPath(string importPath)
    {
        string[] segments = importPath.TrimEnd('/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(SanitizePath)
            .Where(segment => !string.IsNullOrEmpty(segment))
            .ToArray();

        return Path.Combine("src", segments.Length == 0 ? "pkg" : Path.Combine(segments));
    }

    private static string SanitizePath(string value) =>
        string.Concat(value.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) || ch is '/' or '\\' ? '_' : ch));
}
