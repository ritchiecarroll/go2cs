using System;
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
