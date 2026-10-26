using System;
using System.IO;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.testing_runtime;

namespace GolibTests;

/// <summary>
/// The converted test host is launched with exactly the argv <c>go test -json</c> gives a Go test
/// binary, because that argv is the converted program's <c>os.Args</c>: cobra's <c>Execute</c> with no
/// <c>SetArgs</c> parses it, and pflag skips every <c>-test.</c> flag, so Go's run of
/// <c>TestCalledAs</c> sees no arguments. The pipeline's own result and JUnit paths have no Go flag and
/// arrive in the environment instead.
/// </summary>
[TestClass]
public class HostGoTestArgvTests
{
    [TestMethod]
    public void GoTestJsonSpellingIsTheHostJsonMode()
    {
        TestOptions options = TestOptions.Parse(["-test.v=test2json", "-test.timeout=20m0s", "-test.run=^TestX$"]);

        Assert.IsTrue(options.Json);
        Assert.IsTrue(options.Verbose);
        Assert.AreEqual(TimeSpan.FromMinutes(20.0D), options.Timeout);
        Assert.IsNull(options.UnrecognizedFlag);
    }

    [TestMethod]
    public void ResultPathsAreTakenFromTheEnvironmentOnceAndRemoved()
    {
        Environment.SetEnvironmentVariable(TestOptions.ResultFileEnvironmentVariable, "results.json");
        Environment.SetEnvironmentVariable(TestOptions.JUnitFileEnvironmentVariable, "results.xml");

        try
        {
            TestOptions options = TestOptions.Parse([]);

            Assert.AreEqual("results.json", options.ResultFile);
            Assert.AreEqual("results.xml", options.JUnitFile);

            // Removed, so a test that re-executes the host does not hand the child the parent's files.
            Assert.IsNull(Environment.GetEnvironmentVariable(TestOptions.ResultFileEnvironmentVariable));
            Assert.IsNull(Environment.GetEnvironmentVariable(TestOptions.JUnitFileEnvironmentVariable));
        }
        finally
        {
            Environment.SetEnvironmentVariable(TestOptions.ResultFileEnvironmentVariable, null);
            Environment.SetEnvironmentVariable(TestOptions.JUnitFileEnvironmentVariable, null);
        }
    }

    // THE CONVERTED HALF (2026-10-25, COORD's ruling). The arm above reads the CLR environment, which is
    // not what a converted child inherits: exec.Command(os.Args[0]) builds the child's environment from
    // syscall.envs, the snapshot syscall's static initializer took, and when that ran BEFORE Parse the
    // snapshot still held the parent's absolute GO2CS_TEST_RESULT. Each re-executed helper's exit flush
    // then truncated the parent's results file (4 helpers of the linux syscall row; on darwin a child's
    // record survived). So this arm plants all three settings on BOTH sides, as an early syscall init
    // leaves them, and reads what a child spawned from a test body would inherit.
    //
    // UNIX ONLY in effect: windows' converted syscall reads and writes the process environment itself
    // (env_windows: GetEnvironmentVariable/SetEnvironmentVariable), so the CLR clear already cleared the
    // child's view and this arm passes there with or without the fix -- measured, and the windows os/exec
    // row's results file has one writer. Unix's is the envs snapshot (env_unix).
    //
    // RED-FIRST, 2026-10-25, on linux: without TestHost.Run's converted-environment clear after Parse, this
    // arm fails naming all three; with it, it passes.
    private static (bool Result, bool JUnit, bool Deadline)? s_convertedDuringRun;

    [TestMethod]
    public void AChildStartedByATestInheritsNoneOfThePipelineSettings()
    {
        string scratch = Path.Combine(Path.GetTempPath(), "g2cs-pipeline-env-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(scratch);
        (string Name, string Value)[] planted =
        [
            (TestOptions.ResultFileEnvironmentVariable, Path.Combine(scratch, "results.json")),
            (TestOptions.JUnitFileEnvironmentVariable, Path.Combine(scratch, "results.xml")),
            (TestOptions.TestDeadlineEnvironmentVariable, "0"),
        ];

        try
        {
            foreach ((string name, string value) in planted)
                TestHost.PublishEnvironmentVariable(name, value);

            s_convertedDuringRun = null;
            TestRegistry registry = new("guard/pipelineenv", []);
            registry.Add("TestProbe", _ => s_convertedDuringRun = (
                syscall_package.Getenv(TestOptions.ResultFileEnvironmentVariable).found,
                syscall_package.Getenv(TestOptions.JUnitFileEnvironmentVariable).found,
                syscall_package.Getenv(TestOptions.TestDeadlineEnvironmentVariable).found), "guard_test.go", 1);

            Assert.AreEqual(0, TestHost.Run(registry, []));
            Assert.IsNotNull(s_convertedDuringRun, "the probe test did not run");

            (bool result, bool junit, bool deadline) = s_convertedDuringRun.Value;
            Assert.IsFalse(result || junit || deadline,
                $"a converted child started by a test would inherit the parent's pipeline settings: {TestOptions.ResultFileEnvironmentVariable}={result}, " +
                $"{TestOptions.JUnitFileEnvironmentVariable}={junit}, {TestOptions.TestDeadlineEnvironmentVariable}={deadline} -- each re-executed helper writes the parent's results file");

            // The run itself still wrote where the pipeline told it to: the clear never costs the parent its record.
            Assert.IsTrue(File.Exists(Path.Combine(scratch, "results.json")), "the host lost its own results path");
        }
        finally
        {
            foreach ((string name, _) in planted)
                TestHost.PublishEnvironmentVariable(name, null);

            try { Directory.Delete(scratch, recursive: true); } catch (IOException) { }
        }
    }

    [TestMethod]
    public void AnExplicitFlagStillWinsOverTheEnvironment()
    {
        // The in-process tier passes --result directly; that must keep working.
        Environment.SetEnvironmentVariable(TestOptions.ResultFileEnvironmentVariable, "from-env.json");

        try
        {
            Assert.AreEqual("from-flag.json", TestOptions.Parse(["--result", "from-flag.json"]).ResultFile);
        }
        finally
        {
            Environment.SetEnvironmentVariable(TestOptions.ResultFileEnvironmentVariable, null);
        }
    }
}
