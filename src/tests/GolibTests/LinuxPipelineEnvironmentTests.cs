using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.testing_runtime;

namespace GolibTests;

// A CHILD STARTED BY A TEST INHERITS NONE OF THE PIPELINE'S SETTINGS (2026-10-25, COORD's ruling).
//
// The pipeline hands the host GO2CS_TEST_RESULT, GO2CS_TEST_JUNIT and GO2CS_TEST_DEADLINE in the
// environment, and TestOptions.Parse takes them -- clearing the CLR environment, which is what
// HostGoTestArgvTests.ResultPathsAreTakenFromTheEnvironmentOnceAndRemoved reads. A CONVERTED child does
// not read that: exec.Command(os.Args[0]) builds its environment from syscall.envs, the snapshot unix
// syscall's static initializer took, and when that ran BEFORE Parse the snapshot still held the parent's
// absolute GO2CS_TEST_RESULT. Each re-executed helper's exit flush then truncated the parent's results
// file: 4 helpers of the linux syscall row wrote it before the parent's final write, and on darwin a
// helper's 3-event record survived. TestHost.Run now clears the three from the converted environment too.
//
// LINUX FLAVOR ONLY (GolibTests.csproj excludes this file unless $(GoTargetOS) is linux, as it does
// LinuxSpawnSeamTests): windows' converted syscall reads and writes the process environment itself, so
// the CLR clear already covered the child there (the windows os/exec row's results file has one writer),
// and on a linux host the default build binds the WINDOWS flavor, whose static initializer cannot run.
//
// The arm plants all three settings on BOTH sides, as an early syscall init leaves them, and reads what
// a child spawned from a test body would inherit. RED-FIRST, 2026-10-25, on linux with
// -p:GoTargetOS=linux: without TestHost.Run's converted-environment clear after Parse it fails naming all
// three; with it, it passes.
[TestClass]
public class LinuxPipelineEnvironmentTests
{
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

            // The precondition: the plant reached the converted side, or the verdict below would measure nothing.
            Assert.IsTrue(syscall_package.Getenv(TestOptions.ResultFileEnvironmentVariable).found, "the plant did not reach the converted environment");

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
}
