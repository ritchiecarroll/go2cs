// TestingRuntimeTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using go;
using go.testing_runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BehavioralTests;

[TestClass]
public class TestingRuntimeTests
{
    [TestMethod]
    public void PassingCleanupAndParallelSubtestsComplete()
    {
        ConcurrentQueue<string> events = new();
        TestRegistry registry = new("runtime/pass", []);
        registry.Add("TestPass", pointer =>
        {
            ref testing_package.T test = ref pointer.Value;
            test.Cleanup(() => events.Enqueue("cleanup"));
            test.Run("first", child =>
            {
                ref testing_package.T subtest = ref child.Value;
                subtest.Parallel();
                events.Enqueue("first");
            });
            test.Run("second", child =>
            {
                ref testing_package.T subtest = ref child.Value;
                subtest.Parallel();
                events.Enqueue("second");
            });
            events.Enqueue("parent");
        }, "runtime_test.go", 1);

        Assert.AreEqual(0, TestHost.Run(registry, []));
        CollectionAssert.Contains(events.ToArray(), "first");
        CollectionAssert.Contains(events.ToArray(), "second");
        Assert.AreEqual("cleanup", events.Last());
    }

    [TestMethod]
    public void ErrorFatalPanicAndSkipProduceExpectedExitCodes()
    {
        Assert.AreEqual(1, RunSingle("TestError", (ref testing_package.T test) => test.Error("nonfatal")));
        Assert.AreEqual(1, RunSingle("TestFatal", (ref testing_package.T test) => test.Fatal("fatal")));
        Assert.AreEqual(1, RunSingle("TestPanic", (ref testing_package.T _) => throw new PanicException("boom")));
        Assert.AreEqual(1, RunSingle("TestRuntimePanic", (ref testing_package.T unused) =>
        {
            int zero = 0;
            _ = 1 / zero;
        }));
        Assert.AreEqual(0, RunSingle("TestSkip", (ref testing_package.T test) => test.Skip("not applicable")));
    }

    [TestMethod]
    public void TestMainControlsRegistryExecution()
    {
        bool ran = false;
        TestRegistry registry = new("runtime/main", []);
        registry.Add("TestThroughMain", _ => ran = true, "runtime_test.go", 1);
        registry.SetTestMain(pointer =>
        {
            ref testing_package.M testMain = ref pointer.Value;
            testMain.Run();
        });

        Assert.AreEqual(0, TestHost.Run(registry, []));
        Assert.IsTrue(ran);
    }

    [TestMethod]
    public void FatalStillRunsCleanupInLifoOrder()
    {
        ConcurrentQueue<string> cleanupOrder = new();
        TestRegistry registry = new("runtime/cleanup", []);
        registry.Add("TestCleanup", pointer =>
        {
            ref testing_package.T test = ref pointer.Value;
            test.Cleanup(() => cleanupOrder.Enqueue("first"));
            test.Cleanup(() => cleanupOrder.Enqueue("second"));
            test.Fatal("stop now");
        }, "runtime_test.go", 1);

        Assert.AreEqual(1, TestHost.Run(registry, []));
        CollectionAssert.AreEqual(new[] { "second", "first" }, cleanupOrder.ToArray());
    }

    [TestMethod]
    public void SetenvTempDirAndFixturesAreIsolated()
    {
        string key = $"GO2CS_TEST_{Guid.NewGuid():N}";
        string fixtureRelativePath = Path.Combine("testdata", "runtime-fixture.txt");
        string fixtureSourcePath = Path.Combine(AppContext.BaseDirectory, fixtureRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fixtureSourcePath)!);
        File.WriteAllText(fixtureSourcePath, "original");

        string? tempDirectory = null;
        bool observedEnvironment = false;
        bool observedFixture = false;

        try
        {
            TestRegistry registry = new("runtime/isolation", [fixtureRelativePath]);
            registry.Add("TestIsolation", pointer =>
            {
                ref testing_package.T test = ref pointer.Value;
                test.Setenv(key, "value");
                observedEnvironment = Environment.GetEnvironmentVariable(key) == "value";
                tempDirectory = test.TempDir().ToString();
                observedFixture = File.ReadAllText(fixtureRelativePath) == "original";
                File.WriteAllText(fixtureRelativePath, "working-copy");
            }, "runtime_test.go", 1);

            Assert.AreEqual(0, TestHost.Run(registry, []));
            Assert.IsTrue(observedEnvironment);
            Assert.IsTrue(observedFixture);
            Assert.IsNull(Environment.GetEnvironmentVariable(key));
            Assert.IsNotNull(tempDirectory);
            Assert.IsFalse(Directory.Exists(tempDirectory));
            Assert.AreEqual("original", File.ReadAllText(fixtureSourcePath));
        }
        finally
        {
            File.Delete(fixtureSourcePath);
        }
    }

    [TestMethod]
    public void JsonAndJUnitResultFilesAreWritten()
    {
        string resultPath = Path.Combine(Path.GetTempPath(), $"go2cs-results-{Guid.NewGuid():N}.json");
        string junitPath = Path.ChangeExtension(resultPath, ".xml");

        try
        {
            TestRegistry registry = new("runtime/results", []);
            registry.Add("TestPass", _ => { }, "runtime_test.go", 1);

            Assert.AreEqual(0, TestHost.Run(registry, ["--result", resultPath, "--junit", junitPath]));
            StringAssert.Contains(File.ReadAllText(resultPath), "\"action\":\"pass\"");
            StringAssert.Contains(File.ReadAllText(junitPath), "<testsuite");
            StringAssert.Contains(File.ReadAllText(junitPath), "TestPass");
        }
        finally
        {
            File.Delete(resultPath);
            File.Delete(junitPath);
        }
    }

    [TestMethod]
    public void HierarchicalFilterSelectsNamedSubtest()
    {
        ConcurrentQueue<string> ran = new();
        TestRegistry registry = new("runtime/filter", []);
        registry.Add("TestParent", pointer =>
        {
            ref testing_package.T test = ref pointer.Value;
            test.Run("wanted", _ => ran.Enqueue("wanted"));
            test.Run("other", _ => ran.Enqueue("other"));
        }, "runtime_test.go", 1);

        Assert.AreEqual(0, TestHost.Run(registry, ["-run", "TestParent/wanted"]));
        CollectionAssert.AreEqual(new[] { "wanted" }, ran.ToArray());
    }

    // testing.RunTests, called from INSIDE a running test (testify's suite tests): Go runs the list on a
    // FRESH root, so each entry is a top-level test under its own name -- serial until it calls Parallel,
    // the parked ones released when the list is done -- and returns ok == false exactly when one failed.
    // The root has no parent, so the CALLER is not failed, and the package still passes (m.Run's ok is
    // its own list's).
    [TestMethod]
    public void RunTestsRunsItsListAsTopLevelTestsFromInsideARunningTest()
    {
        string resultPath = Path.Combine(Path.GetTempPath(), $"go2cs-results-{Guid.NewGuid():N}.json");
        ConcurrentQueue<string> events = new();
        bool okFailing = true, okPassing = false, callerFailed = true;

        try
        {
            TestRegistry registry = new("runtime/runtests", []);
            registry.Add("TestOuter", pointer =>
            {
                ref testing_package.T test = ref pointer.Value;
                okFailing = testing_package.RunTests((_, _) => (true, null!), new testing_package.InternalTest[]
                {
                    new(Name: "TestOuter/Fails", F: t => { events.Enqueue("fails"); t.Error("expected"); }),
                    new(Name: "TestOuter/Parallel", F: t => { t.Value.Parallel(); events.Enqueue("parallel"); }),
                    new(Name: "TestOuter/Serial", F: _ => events.Enqueue("serial")),
                }.slice());
                okPassing = testing_package.RunTests((_, _) => (true, null!), new testing_package.InternalTest[]
                {
                    new(Name: "TestOuter/Passes", F: _ => events.Enqueue("passes")),
                }.slice());
                events.Enqueue("after");
                callerFailed = test.Failed();
            }, "runtime_test.go", 1);

            Assert.AreEqual(0, TestHost.Run(registry, ["--result", resultPath]));
            Assert.IsFalse(okFailing);
            Assert.IsTrue(okPassing);
            Assert.IsFalse(callerFailed);
            CollectionAssert.AreEqual(new[] { "fails", "serial", "parallel", "passes", "after" }, events.ToArray());

            string results = File.ReadAllText(resultPath);
            StringAssert.Contains(results, "\"test\":\"TestOuter/Fails\",\"action\":\"fail\"");
            StringAssert.Contains(results, "\"test\":\"TestOuter/Passes\",\"action\":\"pass\"");
        }
        finally
        {
            File.Delete(resultPath);
        }
    }

    // RunTests' matcher is Go's newMatcher(matchString, *match, "-test.run", *skip): each -run ELEMENT goes
    // to the CALLER's matchString, so testify's always-true filter runs the whole list whatever -run says,
    // and a real matcher selects by element.
    [TestMethod]
    public void RunTestsHandsEachRunElementToTheCallersMatchString()
    {
        ConcurrentQueue<string> ran = new();
        ConcurrentQueue<string> patterns = new();
        TestRegistry registry = new("runtime/runtests-match", []);
        registry.Add("TestOuter", _ =>
        {
            testing_package.RunTests((pattern, name) =>
            {
                patterns.Enqueue($"{pattern}~{name}");
                return (System.Text.RegularExpressions.Regex.IsMatch(name.ToString(), pattern.ToString()), null!);
            }, new testing_package.InternalTest[]
            {
                new(Name: "TestOuter/Wanted", F: _ => ran.Enqueue("wanted")),
                new(Name: "TestOuter/Other", F: _ => ran.Enqueue("other")),
            }.slice());
            testing_package.RunTests((_, _) => (true, null!), new testing_package.InternalTest[]
            {
                new(Name: "TestOuter/Always", F: _ => ran.Enqueue("always")),
            }.slice());
        }, "runtime_test.go", 1);

        Assert.AreEqual(0, TestHost.Run(registry, ["-run", "TestOuter/Wanted"]));
        CollectionAssert.AreEqual(new[] { "wanted", "always" }, ran.ToArray());
        CollectionAssert.Contains(patterns.ToArray(), "Wanted~Other");
    }

    [TestMethod]
    public void CrossGoroutineFatalRecordsInfrastructureFailureWithoutKillingProcess()
    {
        // F8 guard: golib goroutines run on the bare thread pool with no non-panic exception
        // handler, and golib's AppDomain backstop responds to an unhandled exception by printing
        // the report to stderr and exiting 2 (like Go) — killing the whole run with no result
        // files — so an ownership violation must be ROUTED to an infrastructure failure on the
        // owning execution, never thrown into the foreign thread.
        string resultPath = Path.Combine(Path.GetTempPath(), $"go2cs-ownership-{Guid.NewGuid():N}.json");

        try
        {
            TestRegistry registry = new("runtime/ownership", []);
            registry.Add("TestCrossGoroutineFatal", pointer =>
            {
                Thread goroutine = new(() => pointer.Fatal("cross-goroutine fatal")) { IsBackground = true };
                goroutine.Start();
                goroutine.Join();
            }, "runtime_test.go", 1);

            Assert.AreEqual(1, TestHost.Run(registry, ["--result", resultPath]));
            StringAssert.Contains(File.ReadAllText(resultPath), "\"action\":\"infrastructure-error\"");
        }
        finally
        {
            File.Delete(resultPath);
        }
    }

    [TestMethod]
    public void FailNowInsideCleanupRecordsFailure()
    {
        // F9 guard: a FailNow issued from within a cleanup must mark the test failed (the abort
        // exception itself is contained by the cleanup loop, and later cleanups still run).
        ConcurrentQueue<string> cleanupOrder = new();
        TestRegistry registry = new("runtime/cleanupfailnow", []);
        registry.Add("TestCleanupFailNow", pointer =>
        {
            ref testing_package.T test = ref pointer.Value;
            test.Cleanup(() => cleanupOrder.Enqueue("outer"));
            test.Cleanup(() => testing_package.FailNow(ref pointer.Value));
        }, "runtime_test.go", 1);

        Assert.AreEqual(1, TestHost.Run(registry, []));
        CollectionAssert.AreEqual(new[] { "outer" }, cleanupOrder.ToArray());
    }

    [TestMethod]
    public void CleanupRegisteredAfterCompletionIsRejected()
    {
        // F9 guard: a cleanup registered after the test completed can never run — it must be
        // rejected loudly (Go panics on late testing.T use) instead of silently dropped.
        ж<testing_package.T>? captured = null;
        TestRegistry registry = new("runtime/latecleanup", []);
        registry.Add("TestCapture", pointer => captured = pointer, "runtime_test.go", 1);

        Assert.AreEqual(0, TestHost.Run(registry, []));
        Assert.IsNotNull(captured);
        Assert.ThrowsException<InvalidOperationException>(() => testing_package.Cleanup(ref captured.Value, () => { }));
    }

    [TestMethod]
    public void ParallelTestsAreReleasedPerCountIteration()
    {
        // F10 guard: Go releases top-level parallel tests at the end of EACH -count iteration
        // (serial, parallel, serial, parallel) — not batched after every iteration completes.
        ConcurrentQueue<string> order = new();
        TestRegistry registry = new("runtime/countiterations", []);
        registry.Add("TestAParallel", pointer =>
        {
            ref testing_package.T test = ref pointer.Value;
            test.Parallel();
            order.Enqueue("parallel");
        }, "runtime_test.go", 1);
        registry.Add("TestSerial", _ => order.Enqueue("serial"), "runtime_test.go", 1);

        Assert.AreEqual(0, TestHost.Run(registry, ["-count", "2", "-timeout", "30s"]));
        CollectionAssert.AreEqual(new[] { "serial", "parallel", "serial", "parallel" }, order.ToArray());
    }

    [TestMethod]
    public void ParallelCapLimitsConcurrentParallelTests()
    {
        // F10 guard: -parallel caps simultaneously RUNNING parallel tests (Go semantics; the
        // default is the processor count, matching go test's GOMAXPROCS default).
        int current = 0;
        int observedMax = 0;

        TestRegistry registry = new("runtime/parallelcap", []);

        for (int i = 1; i <= 4; i++)
        {
            registry.Add($"TestParallel{i}", pointer =>
            {
                ref testing_package.T test = ref pointer.Value;
                test.Parallel();
                int now = Interlocked.Increment(ref current);
                int seenMax;
                do
                {
                    seenMax = Volatile.Read(ref observedMax);
                }
                while (now > seenMax && Interlocked.CompareExchange(ref observedMax, now, seenMax) != seenMax);
                Thread.Sleep(50);
                Interlocked.Decrement(ref current);
            }, "runtime_test.go", 1);
        }

        Assert.AreEqual(0, TestHost.Run(registry, ["-parallel", "1", "-timeout", "30s"]));
        Assert.AreEqual(1, observedMax);
    }

    [TestMethod]
    public void ParallelParentReleasesItsSlotForParallelChildren()
    {
        // F10 guard: a parallel parent must give its -parallel slot back before waiting on its
        // parallel children (Go's tRunner ordering) or a cap of 1 would deadlock this shape.
        bool childRan = false;
        TestRegistry registry = new("runtime/parallelparent", []);
        registry.Add("TestParent", pointer =>
        {
            ref testing_package.T test = ref pointer.Value;
            test.Parallel();
            test.Run("child", childPointer =>
            {
                ref testing_package.T child = ref childPointer.Value;
                child.Parallel();
                childRan = true;
            });
        }, "runtime_test.go", 1);

        Assert.AreEqual(0, TestHost.Run(registry, ["-parallel", "1", "-timeout", "30s"]));
        Assert.IsTrue(childRan);
    }

    [TestMethod]
    public void ShortAndVerboseFlagsReachTheTestingShim()
    {
        // F4 support guard: testing.Short()/testing.Verbose() report the host's -short/-v flags
        // (go test defaults: both false when absent).
        bool shortSeen = true;
        bool verboseSeen = true;

        TestRegistry registry = new("runtime/flags", []);
        registry.Add("TestFlags", _ =>
        {
            shortSeen = testing_package.Short();
            verboseSeen = testing_package.Verbose();
        }, "runtime_test.go", 1);

        Assert.AreEqual(0, TestHost.Run(registry, ["-short", "-v"]));
        Assert.IsTrue(shortSeen);
        Assert.IsTrue(verboseSeen);

        TestRegistry defaults = new("runtime/flags", []);
        defaults.Add("TestDefaults", _ =>
        {
            shortSeen = testing_package.Short();
            verboseSeen = testing_package.Verbose();
        }, "runtime_test.go", 1);

        Assert.AreEqual(0, TestHost.Run(defaults, []));
        Assert.IsFalse(shortSeen);
        Assert.IsFalse(verboseSeen);
    }

    [TestMethod]
    public void FlagParsingStopsAtTheFirstNonFlagAndLeavesTheRestToTheProgram()
    {
        // Root-A guard: a Go test binary is a program, and its TestMain may take arguments. Go's
        // flag.Parse stops at the first non-flag token; the host used to throw on it, which killed
        // every os/exec helper child (`exec.Command(exePath(t), "cat")`) at startup with exit 2
        // before TestMain could dispatch — 26 comparison rows failing for one host defect.

        // Recognized flags still parse, and BOTH dash spellings name the same flag the way Go's
        // one-or-two-dash stripping does (TestFlagBridge already republishes them undashed).
        Assert.AreEqual((0, true, true), RunArgv("-v"));
        Assert.AreEqual((0, true, true), RunArgv("--v"));
        Assert.AreEqual((0, true, true), RunArgv("--json"), "--json implies -v");
        Assert.AreEqual((0, true, true), RunArgv("-json"));

        // The first non-flag STOPS the parse — the run proceeds instead of dying at startup.
        Assert.AreEqual((0, true, false), RunArgv("cat"));

        // Flags BEFORE the stop are parsed; tokens AFTER it are left alone. A trailing `-v` is the
        // sharp version of that claim: consuming it would flip Verbose and rejecting it would exit
        // 2, and the host must do neither, because that token belongs to the child.
        Assert.AreEqual((0, true, true), RunArgv("-v", "cat", "-n"));
        Assert.AreEqual((0, true, false), RunArgv("cat", "-v"), "a token after the stop must not be parsed as a host flag");
        Assert.AreEqual((0, true, false), RunArgv("cat", "-nosuchflag"), "a token after the stop must not be rejected either");

        // A lone `-` is a non-flag by Go's length test, and `--` terminates the flags; both stop
        // the parse, so the `-v` behind them stays the program's.
        Assert.AreEqual((0, true, false), RunArgv("-", "-v"));
        Assert.AreEqual((0, true, false), RunArgv("--", "-v"));

        // An unrecognized -flag BEFORE any non-flag is the HOST's own command line being wrong, and
        // Go errors there too — the stopping rule must not be widened into ignoring unknown flags.
        Assert.AreEqual((2, false, false), RunArgv("-nosuchflag"));
        Assert.AreEqual((2, false, false), RunArgv("-nosuchflag", "cat"));
        Assert.AreEqual((2, false, false), RunArgv("---json"), "bad flag syntax stays an error");

        // A non-boolean flag takes the NEXT token as its value even when that token looks like a
        // flag, so the stopping rule never sees it: `-run -v` filters on "-v" (matching no test)
        // rather than setting Verbose or erroring for want of an argument.
        Assert.AreEqual((0, false, false), RunArgv("-run", "-v"));

        // ...and a value that does match still runs the test, so the filter really was applied.
        Assert.AreEqual((0, true, false), RunArgv("-run", "TestFlags"));
    }

    [TestMethod]
    public void APackageRegisteredFlagParticipatesAndAnUndefinedOneIsStillRejected()
    {
        // Root guard for the flag LIFECYCLE. Go's test binary reaches exactly one flag.Parse(), by
        // which time testing.Init() has defined -test.* AND the package's own package-level
        // flag.Bool variables have initialized — so a package flag on the command line is simply
        // one of the names in scope. The host's own parse necessarily runs earlier than the
        // package's initialization, and it used to REJECT anything it did not personally define:
        // crypto/tls's BoGo runner re-executes the test binary as its TLS shim with `-bogo-mode`
        // (a flag.Bool in handshake_test.go), and every one of the 3,242 BoGo cases died at
        // startup with `flag provided but not defined: -bogo-mode` before TestMain ran.

        // The package's flag is declared where Go declares it — in package-level variable
        // initialization, which converted C# performs in the class's static constructor — and the
        // host must run that before deciding, exactly as Go runs package init before main.
        Assert.AreEqual(0, TestHost.Run(BogoLikeRegistry(), ["-harness-package-mode"]));
        Assert.IsTrue(BogoLikePackage.Ran, "the run must proceed past a flag the PACKAGE defines");
        Assert.IsNotNull(BogoLikePackage.Mode, "the package's own initializer is what defines it");

        // The package flag STOPS the host's parse, because nothing here knows a foreign flag's
        // arity. Flags before it are the host's; everything after belongs to the program, and the
        // sharp form of that claim is a `-v` on each side of it.
        Assert.AreEqual(0, TestHost.Run(BogoLikeRegistry(), ["-v", "-harness-package-mode"]));
        Assert.IsTrue(BogoLikePackage.Verbose, "a flag BEFORE the package's is still parsed here");

        Assert.AreEqual(0, TestHost.Run(BogoLikeRegistry(), ["-harness-package-mode", "-v"]));
        Assert.IsFalse(BogoLikePackage.Verbose, "a flag AFTER the package's belongs to the program");

        // And the deferral is a MOVE, not a removal. With the converted flag package present and
        // initialized, a name neither vocabulary defines is still the command line being wrong.
        Assert.AreEqual(2, TestHost.Run(BogoLikeRegistry(), ["-nosuchflag"]));
        Assert.AreEqual(2, TestHost.Run(BogoLikeRegistry(), ["-harness-package-mode-typo"]));
    }

    private static TestRegistry BogoLikeRegistry()
    {
        TestRegistry registry = new("runtime/packageflag", []);
        registry.Add("TestFlags", BogoLikePackage.TestFlags, "runtime_test.go", 1);
        return registry;
    }

    // Stands in for the package under test: a class whose STATIC CONSTRUCTOR declares a flag, which
    // is what a converted package-level `var mode = flag.Bool(...)` compiles to. Written with an
    // explicit static constructor rather than a field initializer so the CLR's precise (non
    // beforefieldinit) rules apply — the guard is about WHO runs it and WHEN, and a beforefieldinit
    // class the runtime may initialize early would let it pass without the host doing anything.
    private static class BogoLikePackage
    {
        internal static bool Ran;
        internal static bool Verbose;
        internal static ж<bool>? Mode;

        static BogoLikePackage() =>
            Mode = flag_package.Bool((@string)"harness-package-mode", false, (@string)"stands in for a package's own flag");

        internal static void TestFlags(ж<testing_package.T> _)
        {
            Ran = true;
            Verbose = testing_package.Verbose();
        }
    }

    // Runs a one-test registry with the given command line, reporting the host's exit code, whether
    // the test ran at all, and what testing.Verbose() saw from inside it. Verbose is the probe
    // because a trailing `-v` the host wrongly consumed shows up here as a true the child should
    // have owned; `ran` separates "the filter applied" from "the host rejected the command line".
    private static (int ExitCode, bool Ran, bool Verbose) RunArgv(params string[] args)
    {
        bool verbose = false;
        bool ran = false;

        TestRegistry registry = new("runtime/argv", []);
        registry.Add("TestFlags", _ =>
        {
            verbose = testing_package.Verbose();
            ran = true;
        }, "runtime_test.go", 1);

        return (TestHost.Run(registry, args), ran, verbose);
    }

    [TestMethod]
    public void AllocsPerRunMapsZeroExactlyAndReportsBytesWhenAllocating()
    {
        // F4 support guard: .NET has no malloc counter, so testing.AllocsPerRun measures
        // allocated BYTES on the calling thread — 0 bytes ⟺ 0 allocations makes the zero case
        // exact (the contract the stdlib's assert-zero tests rely on); a nonzero result is a
        // positive byte-derived value, never rounded down to a silent zero pass.
        double zeroAllocs = double.NaN;
        double someAllocs = double.NaN;
        byte[]? sink = null;

        TestRegistry registry = new("runtime/allocs", []);
        registry.Add("TestAllocs", _ =>
        {
            zeroAllocs = testing_package.AllocsPerRun(100, static () => { });

            // The capturing delegate is allocated at this call site, before the measurement
            // window opens; the captured sink keeps each per-run array observable so no
            // future JIT escape analysis can elide the allocation under test.
            someAllocs = testing_package.AllocsPerRun(100, () => sink = new byte[64]);
        }, "runtime_test.go", 1);

        Assert.AreEqual(0, TestHost.Run(registry, []));
        Assert.AreEqual(0.0D, zeroAllocs);
        Assert.IsTrue(someAllocs >= 64.0D, $"expected byte-scaled result >= 64, got {someAllocs}");
        Assert.IsNotNull(sink);
    }

    [TestMethod]
    public void BenchmarkCompileSurfaceIsNoOpAndCoverModeReportsCoverageOff()
    {
        // B6 guard (strings/bytes blocker map): capability-excluded benchmark bodies still
        // COMPILE — exclusion gates the run registry, not emission — so every B member the
        // strings/bytes suites reference must exist on the compile-only shim, through both
        // receiver shapes converted code binds (the ж<B> box and the ref-local value). The timer and
        // reporting members are no-ops; the failure and skip members are Go's ZERO B (measured on
        // go1.24.13): Errorf sets Failed, Skip sets Skipped, and Fatal, Fatalf and Skip end the
        // calling goroutine (runtime.Goexit).
        ж<testing_package.B> benchmark = new StandardBox<testing_package.B>(new testing_package.B());

        benchmark.ReportAllocs();
        benchmark.SetBytes(1024L);
        benchmark.ResetTimer();
        benchmark.StopTimer();
        benchmark.StartTimer();
        benchmark.Errorf("errorf %d", 1);
        Assert.IsTrue(benchmark.Failed());
        Assert.ThrowsException<GoexitException>(() => benchmark.Fatal("fatal"));
        Assert.ThrowsException<GoexitException>(() => benchmark.Fatalf("fatalf %d", 2));
        Assert.ThrowsException<GoexitException>(() => benchmark.Skip("skip"));
        Assert.IsTrue(benchmark.Skipped());
        Assert.IsTrue(benchmark.Run("sub", _ => { }));

        ref testing_package.B direct = ref benchmark.Value;
        direct.ReportAllocs();
        direct.SetBytes(2048L);
        direct.ResetTimer();
        direct.StopTimer();
        direct.StartTimer();
        direct.Errorf("errorf");
        ExpectGoexit(ref direct, (ref testing_package.B b) => b.Fatal("fatal"));
        ExpectGoexit(ref direct, (ref testing_package.B b) => b.Fatalf("fatalf"));
        ExpectGoexit(ref direct, (ref testing_package.B b) => b.Skip("skip"));
        Assert.AreEqual((nint)0, direct.N);

        // strings TestIndexRune branches on `testing.CoverMode() == ""` — the shim must report
        // Go's coverage-off value so the test takes the same path as an uncovered `go test` run.
        Assert.IsTrue(testing_package.CoverMode() == "");
    }

    [TestMethod]
    public void FuzzCompileSurfaceIsNoOpAndAcceptsAnyTargetSignature()
    {
        // Fuzz declarations are disclosed-unsupported exactly as benchmarks are, but their bodies
        // still COMPILE — math/big's `func FuzzExpMont(f *testing.F)` failed the whole package
        // build with CS0426 before F existed. Every member must be present on both receiver shapes
        // converted code binds. The rest are no-ops; the failure and skip members are Go's ZERO F
        // (measured on go1.24.13): Error, Errorf and Fail set Failed, the Skip family sets Skipped,
        // and Fatal, Fatalf, FailNow and the Skip family end the calling goroutine (runtime.Goexit).
        ж<testing_package.F> fuzz = new StandardBox<testing_package.F>(new testing_package.F());

        fuzz.Add(1, "seed");
        fuzz.Log("log");
        fuzz.Logf("logf %d", 2);
        fuzz.Helper();
        fuzz.Cleanup(() => { });
        fuzz.Setenv("K", "V");
        Assert.IsFalse(fuzz.Failed());
        Assert.IsFalse(fuzz.Skipped());
        fuzz.Error("error");
        fuzz.Errorf("errorf %d", 1);
        fuzz.Fail();
        Assert.IsTrue(fuzz.Failed());
        Assert.ThrowsException<GoexitException>(() => fuzz.Fatal("fatal"));
        Assert.ThrowsException<GoexitException>(() => fuzz.Fatalf("fatalf %d", 3));
        Assert.ThrowsException<GoexitException>(() => fuzz.FailNow());
        Assert.ThrowsException<GoexitException>(() => fuzz.Skip("skip"));
        Assert.ThrowsException<GoexitException>(() => fuzz.Skipf("skipf %d", 4));
        Assert.ThrowsException<GoexitException>(() => fuzz.SkipNow());
        Assert.IsTrue(fuzz.Skipped());
        Assert.IsTrue(fuzz.Name() == "");
        Assert.IsTrue(fuzz.TempDir() == "");

        ref testing_package.F direct = ref fuzz.Value;
        direct.Add(2);
        direct.Errorf("errorf");
        ExpectGoexit(ref direct, (ref testing_package.F f) => f.Fatal("fatal"));
        ExpectGoexit(ref direct, (ref testing_package.F f) => f.Skip("skip"));
        direct.Helper();

        // Fuzz takes a System.Delegate because a Go fuzz target's signature is arbitrary — the
        // converted body is an explicitly-typed lambda, so C# infers its natural Action<…>. Both
        // a wide arity (math/big's target takes *testing.T plus nine uints) and a minimal one must
        // bind, and neither target may be invoked: there is no fuzzing engine.
        bool invoked = false;

        fuzz.Fuzz((ж<testing_package.T> t, nuint a, nuint b, nuint c, nuint d,
            nuint e, nuint f, nuint g, nuint h, nuint i) => invoked = true);

        direct.Fuzz((ж<testing_package.T> t, byte[] data) => invoked = true);

        Assert.IsFalse(invoked, "a compile-only fuzz surface must never invoke its target");
    }

    [TestMethod]
    public void AllocsPerRunNotesItsUnitOnlyWhenTheAnswerIsNonzero()
    {
        // r56d: the value AllocsPerRun returns is rendered by Go's own "got %v allocs" format,
        // so a byte figure was reaching the page wearing the word "allocs". A NONZERO result
        // must therefore name its unit at the seam. The ZERO case must stay silent — there the
        // two units agree exactly (0 bytes ⟺ 0 allocations), and a passing test's output has to
        // remain byte-identical to what it was before this seam existed, which is what keeps
        // every already-banked suite's proof page from moving.
        const string Marker = "go2cs: testing.AllocsPerRun measured";

        Assert.IsTrue(RunAndCaptureJUnit("runtime/allocnote-nonzero", static () =>
        {
            byte[]? sink = null;
            testing_package.AllocsPerRun(100, () => sink = new byte[64]);
            GC.KeepAlive(sink);
        }).Contains(Marker, StringComparison.Ordinal), "a nonzero AllocsPerRun must disclose that its figure is in BYTES");

        Assert.IsFalse(RunAndCaptureJUnit("runtime/allocnote-zero", static () =>
            testing_package.AllocsPerRun(100, static () => { })
        ).Contains(Marker, StringComparison.Ordinal), "a zero AllocsPerRun must add nothing — the units agree there, so passing output must not move");
    }

    [TestMethod]
    public void AllocsPerRunReportsAGolibObjectCountAndFallsBackToBytesWhenNothingIsCounted()
    {
        // r58a: AllocsPerRun reports go2cs's OWN allocation COUNT — golib's runtime counter, the
        // structural mirror of Go's runtime.MemStats.Mallocs — instead of the byte figure the CLR
        // is the only thing able to supply. Two arms, and the SECOND is the one that keeps the
        // first honest.
        double boxAllocs = double.NaN;
        double rawAllocs = double.NaN;
        double zeroAllocs = double.NaN;
        ж<long>? boxSink = null;
        byte[]? byteSink = null;

        TestRegistry registry = new("runtime/alloccount", []);
        registry.Add("TestAllocs", _ =>
        {
            // COUNTED: a ж<long> over an unmanaged T allocates exactly two managed objects — the
            // box and the eager one-element pinnable slot ж.cs's m_slot commentary explains cannot
            // be deferred. Two is therefore the exact expected COUNT; the same run allocates on the
            // order of 72-80 BYTES, so this assert distinguishes the units outright and fails if
            // the counter is ever removed or stops being read.
            boxAllocs = testing_package.AllocsPerRun(100, () => boxSink = new StandardBox<long>(1L));

            // UNCOUNTED: a raw byte[64] is allocated by the C# compiler in this assembly, never
            // through golib, so the counter charges nothing for it. Reporting the count's zero here
            // would be a FALSE PASS on any assert-zero test, so the shim must fall back to bytes.
            rawAllocs = testing_package.AllocsPerRun(100, () => byteSink = new byte[64]);

            // Zero stays exactly zero in both units.
            zeroAllocs = testing_package.AllocsPerRun(100, static () => { });
        }, "runtime_test.go", 1);

        Assert.AreEqual(0, TestHost.Run(registry, []));

        Assert.AreEqual(2.0D, boxAllocs, $"a ж<long> per run is exactly 2 counted objects; got {boxAllocs} (a byte figure would be ~72-80)");
        Assert.IsTrue(rawAllocs >= 64.0D, $"an allocation golib never sees must fall back to BYTES rather than report a false zero; got {rawAllocs}");
        Assert.AreEqual(0.0D, zeroAllocs);

        Assert.IsNotNull(boxSink);
        Assert.IsNotNull(byteSink);
    }

    [TestMethod]
    public void AllocsPerRunNoteNamesTheCountWhenCountedAndBytesWhenNot()
    {
        // The r56d seam, now carrying two shapes: the note must say which quantity produced the
        // number, because the value is about to be rendered by Go's own "got %v allocs" format and
        // a disclosure decision has to be able to see whether it read a count or a byte total.
        const string CountedMarker = "go2cs: testing.AllocsPerRun counted";
        const string BytesMarker = "go2cs: testing.AllocsPerRun measured";

        string counted = RunAndCaptureJUnit("runtime/alloccount-counted", static () =>
        {
            ж<long>? sink = null;
            testing_package.AllocsPerRun(100, () => sink = new StandardBox<long>(1L));
            GC.KeepAlive(sink);
        });

        Assert.IsTrue(counted.Contains(CountedMarker, StringComparison.Ordinal), "a counted result must name itself an allocation COUNT");
        Assert.IsTrue(counted.Contains("LOWER BOUND", StringComparison.Ordinal), "a counted result must disclose that golib's census is not total");
        Assert.IsFalse(counted.Contains(BytesMarker, StringComparison.Ordinal), "a counted result must not also claim to be a byte figure");

        string uncounted = RunAndCaptureJUnit("runtime/alloccount-uncounted", static () =>
        {
            byte[]? sink = null;
            testing_package.AllocsPerRun(100, () => sink = new byte[64]);
            GC.KeepAlive(sink);
        });

        Assert.IsTrue(uncounted.Contains(BytesMarker, StringComparison.Ordinal), "an uncounted result must disclose that it fell back to BYTES");
        Assert.IsFalse(uncounted.Contains(CountedMarker, StringComparison.Ordinal), "an uncounted result must never present itself as a count");
    }

    private static string RunAndCaptureJUnit(string package, Action body)
    {
        string junitPath = Path.Combine(Path.GetTempPath(), $"go2cs-junit-{Guid.NewGuid():N}.xml");

        try
        {
            TestRegistry registry = new(package, []);

            // The test fails deliberately: a JUnit record carries a test's captured output, and
            // failing is the state the real rows this guards are in (nistec's TestAllocations).
            registry.Add("TestAllocs", pointer =>
            {
                body();
                pointer.Error("deliberate failure so the record carries its output");
            }, "runtime_test.go", 1);

            Assert.AreEqual(1, TestHost.Run(registry, ["--junit", junitPath]));
            return File.ReadAllText(junitPath);
        }
        finally
        {
            File.Delete(junitPath);
        }
    }

    [TestMethod]
    public void JUnitOutputSurvivesXmlInvalidCharacters()
    {
        // Real Go test logs legitimately contain XML-invalid characters (unicode/utf8's own
        // tests log U+FFFE/U+FFFF data); an unsanitized XDocument.Save threw AFTER the suite
        // completed, downgrading the whole run to an infrastructure error with no JUnit file.
        string junitPath = Path.Combine(Path.GetTempPath(), $"go2cs-junit-{Guid.NewGuid():N}.xml");

        try
        {
            TestRegistry registry = new("runtime/xmlchars", []);
            registry.Add("TestWeirdOutput", pointer => pointer.Error("bad ￾￿ chars"), "runtime_test.go", 1);

            Assert.AreEqual(1, TestHost.Run(registry, ["--junit", junitPath]));
            string junit = File.ReadAllText(junitPath);
            StringAssert.Contains(junit, "TestWeirdOutput");
            StringAssert.Contains(junit, "\\ufffe");
        }
        finally
        {
            File.Delete(junitPath);
        }
    }

    [TestMethod]
    public void SubtestNamesEscapeNonPrintableRunesTheWayGoDoes()
    {
        // A subtest's NAME is what the differential oracle pairs results by, so a name that renders
        // differently from Go's produces a one-sided row on BOTH sides — `Go="pass" C#=""` beside
        // `Go="" C#="pass"`. os's TestReadStdin feeds two inputs containing U+001A across 462
        // subtests, which folding a non-printable to U+FFFD turned into 924 lines that read exactly
        // like a mass failure, on a top-level test that AGREED. Go's testing.rewrite folds
        // whitespace to `_` and escapes anything unprintable as strconv.QuoteRune's body.
        ConcurrentQueue<string> names = new();
        TestRegistry registry = new("runtime/names", []);
        registry.Add("TestNames", pointer =>
        {
            ref testing_package.T test = ref pointer.Value;

            foreach (string requested in new[] { "helloworld", "a b", "tab\there", "soft­hyphen", "plain" })
            {
                test.Run(requested, child => names.Enqueue(child.Value.Name().ToString()));
            }
        }, "runtime_test.go", 1);

        Assert.AreEqual(0, TestHost.Run(registry, []));

        // Every expectation below is what `go test -v` prints for the same five subtest names.
        string[] reported = names.ToArray();
        CollectionAssert.Contains(reported, @"TestNames/hello\x1aworld");
        CollectionAssert.Contains(reported, "TestNames/a_b");
        CollectionAssert.Contains(reported, "TestNames/tab_here");
        // U+00AD SOFT HYPHEN is a FORMAT character — unprintable to Go, so it escapes rather than
        // passing through, and the escape is the four-hex \u form rather than the two-hex \x one.
        CollectionAssert.Contains(reported, @"TestNames/soft\u00adhyphen");
        CollectionAssert.Contains(reported, "TestNames/plain");
    }

    [TestMethod]
    public void ByteSliceFormatsAsItsBytesUnderTheStringVerbs()
    {
        // Go renders a []byte under %s/%q/%x as the BYTES, and only under %v/%d as the list of
        // numbers — so `%q` of an empty []byte is `""`, never `"[]"`. Quoting the default rendering
        // instead made os's TestExecutable report a child that produced nothing as
        // `Child returned "[]"`, naming the formatter's own gap in place of the failure. (core/fmt
        // has always been right here; this shim is the host's separate fmt-free formatter.)
        string resultPath = Path.Combine(Path.GetTempPath(), $"go2cs-verbs-{Guid.NewGuid():N}.json");

        try
        {
            slice<byte> nilBytes = default;
            slice<byte> text = new byte[] { (byte)'a', (byte)'b', (byte)'\n' };

            TestRegistry registry = new("runtime/verbs", []);
            registry.Add("TestVerbs", pointer =>
            {
                ref testing_package.T test = ref pointer.Value;
                test.Errorf("q=%q|%q x=%x X=%X s=|%s| v=%v", nilBytes, text, text, text, text, text);
            }, "runtime_test.go", 1);

            Assert.AreEqual(1, TestHost.Run(registry, ["--result", resultPath]));

            string reported = File.ReadAllText(resultPath);

            // JSON-escaped, so the quotes and the newline arrive as \" and \\n.
            StringAssert.Contains(reported, @"q=\u0022\u0022|\u0022ab\\n\u0022");
            StringAssert.Contains(reported, "x=61620a");
            StringAssert.Contains(reported, "X=61620A");
            StringAssert.Contains(reported, @"s=|ab\n|");
            // %v keeps the sequence reading — the divergence is confined to the string verbs.
            StringAssert.Contains(reported, "v=[");
        }
        finally
        {
            File.Delete(resultPath);
        }
    }

    [TestMethod]
    public void FieldPointerEqualityPairsAcrossOfCalls()
    {
        // Guards the golib pointer-identity fix the Phase-4 test runtime depends on: the typed
        // ж.of(...) overload wraps its accessor in a per-call closure, and comparing the wrappers
        // made every distinct `&x.field` box unequal — `&x.f == &x.f` was FALSE (violating Go
        // pointer identity) and the address-keyed runtime semaphores in the hand-owned
        // sync/internal-poll implementations never paired Semrelease with Semacquire (the
        // ConvertedTestHarness os.ReadFile close hang). Equality now compares the field's
        // identity token: same box + same accessor = equal pointer, across call sites.
        ж<SemaHolder> box = new StandardBox<SemaHolder>(new SemaHolder());
        ж<uint> first = box.of<uint>(SemaField);
        ж<uint> second = box.of<uint>(SemaField);

        Assert.IsTrue(first.Equals(second));
        Assert.AreEqual(first.GetHashCode(), second.GetHashCode());

        ж<SemaHolder> otherBox = new StandardBox<SemaHolder>(new SemaHolder());
        Assert.IsFalse(first.Equals(otherBox.of<uint>(SemaField)));
    }

    private struct SemaHolder
    {
        public uint Sema;
    }

    private static ref uint SemaField(ref SemaHolder instance) => ref instance.Sema;

    private static int RunSingle(string name, ActionRef action)
    {
        TestRegistry registry = new("runtime/failure", []);
        registry.Add(name, pointer =>
        {
            ref testing_package.T test = ref pointer.Value;
            action(ref test);
        }, "runtime_test.go", 1);
        return TestHost.Run(registry, []);
    }

    private delegate void ActionRef(ref testing_package.T test);

    private delegate void RefAction<TValue>(ref TValue value);

    // A terminating member called on a ref-local receiver (which a lambda cannot capture): it must end
    // the calling goroutine.
    private static void ExpectGoexit<TValue>(ref TValue value, RefAction<TValue> action)
    {
        try
        {
            action(ref value);
        }
        catch (GoexitException)
        {
            return;
        }

        Assert.Fail("expected runtime.Goexit");
    }
}
