using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.testing_runtime;

namespace GolibTests;

[TestClass]
public class AllocsPerRunParallelTests
{
    // WHY THIS EXISTS.
    //
    // Go 1.25 made testing.AllocsPerRun panic while parallel tests run (allocs.go:21-23):
    //
    //   if parallelStart.Load() != parallelStop.Load() {
    //       panic("testing: AllocsPerRun called during parallel test")
    //   }
    //
    // The two counters are process-wide (testing.go:429-430). parallelStart counts a parallel test once
    // it is released from its parent and holds a -parallel slot (testing.go:1805, after waitParallel);
    // parallelStop counts it at the very end of its run, after its cleanups and parallel subtests and
    // just before it signals its parent (testing.go:1949). Go's own suites therefore never call
    // AllocsPerRun from a parallel test, and the parity risk runs the other way: a host whose
    // accounting differs from Go's panics where Go does not, or stays silent where Go panics. So the
    // guard pins all three readings, each by name.
    //
    // The host has no _test.go for this, so it takes the route TestExecutionOutputCapTests documents:
    // an MSTest guard binding the converted package directly. MSTest runs this assembly serially (no
    // [Parallelize]), which is what makes reading the process-wide counters safe here.

    private const string GoPanicText = "testing: AllocsPerRun called during parallel test";

    private static string? s_serialPanic = "not run";
    private static string? s_parallelPanic = "not run";

    private static string? CallAllocsPerRun()
    {
        try
        {
            testing_package.AllocsPerRun(1, static () => { });
            return null;
        }
        catch (PanicException ex)
        {
            return ex.Message;
        }
    }

    [TestMethod]
    public void AllocsPerRunPanicsInsideAParallelTestAndNowhereElse()
    {
        s_serialPanic = "not run";
        s_parallelPanic = "not run";

        TestRegistry registry = new("allocsguard", []);

        // Go's order puts the serial test first; top-level parallel tests run after every serial one.
        registry.Add("TestSerialAllocs", static t => s_serialPanic = CallAllocsPerRun(), "allocs_guard_test.go", 1);
        registry.Add("TestParallelAllocs", static t =>
        {
            t.Value.Parallel();
            s_parallelPanic = CallAllocsPerRun();
        }, "allocs_guard_test.go", 2);

        TestRunner runner = new(registry, new TestOptions(), new TestReporter("allocsguard", json: false, verbose: false), ".", ".");
        runner.RunAll();

        Assert.IsNull(s_serialPanic,
            "AllocsPerRun from a serial test, with no parallel test started, must not panic");
        Assert.AreEqual(GoPanicText, s_parallelPanic,
            "AllocsPerRun from a running parallel test must panic with Go's text (allocs.go:22)");
        Assert.IsNull(CallAllocsPerRun(),
            "after every parallel test has finished, parallelStart == parallelStop and AllocsPerRun must not panic");
    }
}
