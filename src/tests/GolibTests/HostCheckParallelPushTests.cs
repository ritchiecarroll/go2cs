using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.testing_runtime;

namespace GolibTests;

/// <summary>
/// Go 1.26's <c>testing.checkParallel</c>, the push testing/cryptotest's <c>SetGlobalRandom</c> pulls through
/// <c>//go:linkname testingCheckParallel testing.checkParallel</c>: Go's parallel-conflict refusal, in Go 1.26's text.
/// </summary>
/// <remarks>
/// Failing-first record (go1.26.10, ed51b5a3bd): testing/cryptotest's TestSetGlobalRandom read seven
/// infrastructure errors ("testingCheckParallel: no implementation reached this compilation") because the
/// hand-owned host had no push target, and the conflict text was still Go 1.24's, which does not name
/// cryptotest.SetGlobalRandom.
/// </remarks>
[TestClass]
public class HostCheckParallelPushTests
{
    private const string Conflict = "testing: test using t.Setenv, t.Chdir, or cryptotest.SetGlobalRandom can not use t.Parallel";

    private static (int exit, string output) Run(TestRegistry registry, params string[] args)
    {
        TextWriter previousOut = Console.Out;
        StringWriter captured = new();
        int exit;

        try
        {
            Console.SetOut(captured);
            exit = TestHost.Run(registry, args);
        }
        finally
        {
            Console.SetOut(previousOut);
        }

        return (exit, captured.ToString().Replace("\r\n", "\n"));
    }

    [TestMethod]
    public void TheConflictTextIsGo126s() =>
        Assert.AreEqual(Conflict, TestExecution.ParallelConflictText, "testing.go:1752 at go1.26.10");

    [TestMethod]
    public void CheckParallelRefusesAParallelTest()
    {
        TestRegistry registry = new("guard", []);
        registry.Add("TestParallelThenCheck", t =>
        {
            t.Parallel();
            testing_package.checkParallel(t);
        }, "a_test.go", 10);

        (int exit, string output) = Run(registry, "-test.run=^TestParallelThenCheck$");

        Assert.AreEqual(1, exit, output);
        StringAssert.Contains(output, "panic: " + Conflict);
    }

    [TestMethod]
    public void CheckParallelDeniesALaterParallel()
    {
        TestRegistry registry = new("guard", []);
        registry.Add("TestCheckThenParallel", t =>
        {
            testing_package.checkParallel(t);
            t.Parallel();
        }, "a_test.go", 10);

        (int exit, string output) = Run(registry, "-test.run=^TestCheckThenParallel$");

        Assert.AreEqual(1, exit, output);
        StringAssert.Contains(output, "panic: " + Conflict);
    }

    [TestMethod]
    public void CheckParallelAloneDoesNotRefuse()
    {
        TestRegistry registry = new("guard", []);
        registry.Add("TestCheckOnly", testing_package.checkParallel, "a_test.go", 10);

        (int exit, string output) = Run(registry, "-test.run=^TestCheckOnly$");

        Assert.AreEqual(0, exit, output);
        Assert.AreEqual("PASS\n", output);
    }

    [TestMethod]
    public void CheckParallelRefusesUnderAParallelAncestor()
    {
        TestRegistry registry = new("guard", []);
        registry.Add("TestParallelParent", t =>
        {
            t.Parallel();
            t.Run("child", testing_package.checkParallel);
        }, "a_test.go", 10);

        (int exit, string output) = Run(registry, "-test.run=^TestParallelParent$");

        Assert.AreEqual(1, exit, output);
        StringAssert.Contains(output, "--- FAIL: TestParallelParent/child");
        StringAssert.Contains(output, "panic: " + Conflict);
    }
}
