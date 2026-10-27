using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.testing_runtime;

namespace GolibTests;

[TestClass]
public class TestChdirLifecycleTests
{
    // WHY THIS EXISTS.
    //
    // Go 1.24's TB.Chdir was UNEXERCISED, and I announced it as uncoverable here on the reasoning
    // that a working-directory change is process-global and so cannot be asserted by a test running
    // beside others. THAT REASONING WAS AN ASSUMPTION AND IT WAS WRONG: this project declares no
    // [Parallelize] attribute and ships no .runsettings, so MSTest runs its tests SERIALLY within
    // the assembly -- and HostEnvironmentVisibilityTests already mutates process-global environment
    // state here on exactly that basis. Checking it cost one command; asserting it cost a member of
    // the Go 1.24 bill its only coverage.
    //
    // Two members of this bill had already turned out to be invented rather than read (b.Loop's
    // cursor reset, Context's owner check). This is the third assumption in the same sitting that
    // did not survive being checked, which is the argument for checking rather than for confidence.
    //
    // THE ASSERTIONS ARE SEMANTIC, NOT STRING COMPARISONS. Comparing Directory.GetCurrentDirectory()
    // against the target path is fragile on Windows, where GetTempPath can hand back an 8.3 short
    // form while GetCurrentDirectory returns the long one -- a guard that fails for that reason
    // teaches nothing. Instead a sentinel file is dropped in the target and the test asks whether a
    // RELATIVE path resolves to it, which is what a working directory actually IS.

    private const string Sentinel = "chdir-guard-probe.txt";

    private static TestExecution NewExecution(string name)
    {
        TestReporter reporter = new("guard", json: false, verbose: false);
        TestRunner runner = new(new TestRegistry("guard", []), new TestOptions(), reporter, ".", ".");

        return new TestExecution(runner, name, null, "guard.go", 1);
    }

    private static string NewTargetDirectory()
    {
        string target = Path.Combine(Path.GetTempPath(), "r-chdir-guard-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(target);
        File.WriteAllText(Path.Combine(target, Sentinel), "probe");

        return target;
    }

    // Both halves of Go's contract in one arm, because they are one contract: Chdir moves the
    // process into the directory FOR THE DURATION OF THE TEST, and Cleanup puts it back.
    [TestMethod]
    public void ChdirEntersTheDirectoryAndTheCleanupRestoresIt()
    {
        string original = Directory.GetCurrentDirectory();
        string target = NewTargetDirectory();

        try
        {
            // Chdir refuses off the test goroutine, so it must be driven from INSIDE a running test
            // rather than on a freshly constructed execution whose owner thread is not yet set.
            TestExecution parent = NewExecution("TestChdirGuard");
            bool sentinelVisibleInBody = false;
            bool bodyRan = false;

            parent.Run("child", t =>
            {
                bodyRan = true;
                t.Value.Execution.Chdir(target);
                sentinelVisibleInBody = File.Exists(Sentinel);
            });

            Assert.IsTrue(bodyRan, "the test body must have run, or neither assertion below means anything");
            Assert.IsTrue(sentinelVisibleInBody,
                "a RELATIVE path must resolve inside the target directory while the test runs -- that is what Chdir does");
            Assert.IsFalse(File.Exists(Sentinel),
                "the cleanup must have restored the working directory -- the sentinel must NOT resolve relatively any more");
            Assert.AreEqual(original, Directory.GetCurrentDirectory(),
                "and the restored directory must be the one the test started in");
        }
        finally
        {
            // Restore unconditionally: a working directory left moved by a FAILING assertion would
            // poison every test that runs after this one in the same process.
            Directory.SetCurrentDirectory(original);
            TryDelete(target);
        }
    }

    // Go's Chdir calls c.Fatal on a directory it cannot enter. The host's Fatal is Log + FailNow,
    // which throws and ends the test -- so the observable is a FAILED test whose working directory
    // never moved.
    [TestMethod]
    public void ChdirToAMissingDirectoryFailsTheTestAndDoesNotMove()
    {
        string original = Directory.GetCurrentDirectory();
        string missing = Path.Combine(Path.GetTempPath(), "r-chdir-absent-" + Guid.NewGuid().ToString("N"));

        Assert.IsFalse(Directory.Exists(missing), "PRECONDITION: the target must not exist");

        try
        {
            TestExecution parent = NewExecution("TestChdirMissing");
            TestExecution? child = null;

            parent.Run("child", t =>
            {
                child = t.Value.Execution;
                t.Value.Execution.Chdir(missing);
            });

            Assert.IsNotNull(child, "the subtest body must have run and exposed its execution");
            Assert.IsTrue(child!.Failed, "Chdir into a directory that cannot be entered must FAIL the test, as Go's Fatal does");
            Assert.AreEqual(original, Directory.GetCurrentDirectory(),
                "a failed Chdir must leave the working directory exactly where it was");
        }
        finally
        {
            Directory.SetCurrentDirectory(original);
        }
    }

    // Go 1.24's Chdir sets PWD on POSIX and DELIBERATELY DOES NOT on windows/plan9 -- its switch
    // says so in a comment ("Windows and Plan 9 do not use the PWD variable"). This arm asserts that
    // omission, which is a decision and not an accident.
    //
    // ⚠ CORRECTED 2026-09-16, and the correction is this commit's own subject. This comment used to
    // claim a second consequence: "Setenv is what enforces 'cannot be used in parallel tests', so on
    // Windows Go performs NO parallel check in Chdir at all." That is FALSE at 1.24.13, measured:
    // T.Chdir is `t.checkParallel(); t.common.Chdir(dir)` (testing.go:1628), so the check runs
    // UNCONDITIONALLY, on every GOOS, BEFORE the directory is touched and independently of the PWD
    // write. And T.Chdir DOES NOT EXIST at 1.23.12 -- zero declarations in that GOROOT's testing.go --
    // so the sentence was never describing an older Go: it described THIS HOST's own shape, where the
    // only parallel check Chdir reached was the one inside its non-Windows Setenv("PWD", ...) call,
    // and it attributed that shape to Go. The host is now Go's shape, so the claim is wrong twice over.
    //
    // ⚠ The ARM below is unaffected and is NOT edited: it asserts PWD behaviour, which was right then
    // and is right now. What was wrong is the justification beside it -- and prose is what the next
    // reader believes when deciding whether an arm still covers what it was written for.
    //
    // ⚠ IT ASSERTS SOMETHING REAL ON BOTH PLATFORMS RATHER THAN SKIPPING ON ONE. A guard that
    // skipped on Windows would be inert on the only host that runs it today, and the Windows half is
    // the half that encodes a DECISION: PWD must come back untouched. The POSIX half is written to
    // be meaningful the moment a linux leg runs it -- which is exactly the coverage class C1 and i9
    // are measuring on train 37 right now, arriving here as a runtime branch rather than a per-GOOS
    // file.
    [TestMethod]
    public void ChdirTouchesPwdOnPosixAndLeavesItAloneOnWindows()
    {
        string original = Directory.GetCurrentDirectory();
        string? pwdBefore = Environment.GetEnvironmentVariable("PWD");
        string target = NewTargetDirectory();

        try
        {
            TestExecution parent = NewExecution("TestChdirPwd");
            string? pwdDuring = null;
            bool bodyRan = false;

            parent.Run("child", t =>
            {
                bodyRan = true;
                t.Value.Execution.Chdir(target);
                pwdDuring = Environment.GetEnvironmentVariable("PWD");
            });

            Assert.IsTrue(bodyRan, "the test body must have run, or nothing below means anything");

            if (OperatingSystem.IsWindows())
            {
                Assert.AreEqual(pwdBefore, pwdDuring,
                    "Go does NOT set PWD on windows -- Chdir must leave it exactly as it found it");
            }
            else
            {
                Assert.IsNotNull(pwdDuring, "Go sets PWD on POSIX, so Chdir must have published one");
                Assert.AreEqual(new DirectoryInfo(target).FullName, new DirectoryInfo(pwdDuring!).FullName,
                    "PWD must name the directory Chdir entered, resolved rather than string-compared");
            }

            // Both flavors agree here: whatever Chdir did to PWD, the cleanup must undo.
            Assert.AreEqual(pwdBefore, Environment.GetEnvironmentVariable("PWD"),
                "the cleanup must restore PWD to its pre-test value on every platform");
        }
        finally
        {
            Directory.SetCurrentDirectory(original);
            Environment.SetEnvironmentVariable("PWD", pwdBefore);
            TryDelete(target);
        }
    }

    // Go's Chdir and its artifact root read the working directory through os.Getwd, which answers $PWD
    // when it names the same file as "." -- the LOGICAL spelling. getcwd answers the PHYSICAL one. They
    // part under a symlinked directory, which is darwin's $TMPDIR (/var -> /private/var), where the
    // host's sandbox sits: testing's TestChdir ("current (relative) with extra os.Chdir") and
    // TestArtifactDirDefault both read /private/var/... there while Go reads /var/... (BOARD 2026-10-26,
    // fix B). This arm makes the symlink itself, so linux runs the darwin shape. Windows does not use
    // PWD, which the arm above already asserts, so there is nothing to assert here.
    [TestMethod]
    public void TheWorkingDirectoryKeepsPwdsLogicalSpellingUnderASymlink()
    {
        if (OperatingSystem.IsWindows())
            Assert.Inconclusive("windows does not use PWD; ChdirTouchesPwdOnPosixAndLeavesItAloneOnWindows covers it");

        string original = Directory.GetCurrentDirectory();
        string? pwdBefore = Environment.GetEnvironmentVariable("PWD");
        var (syscallPwdBefore, syscallHadPwd) = go.syscall_package.Getenv("PWD");
        string root = Path.Combine(Path.GetTempPath(), "c1-chdir-symlink-" + Guid.NewGuid().ToString("N"));
        string real = Path.Combine(root, "real");
        string link = Path.Combine(root, "link");

        try
        {
            Directory.CreateDirectory(real);
            Directory.CreateSymbolicLink(link, real);
            Directory.SetCurrentDirectory(link);

            // What a shell, and the host's own run setup, do on entering a directory: PWD names it as
            // entered, in both env stores (the converted os reads the syscall one).
            go.syscall_package.Setenv("PWD", link);
            Environment.SetEnvironmentVariable("PWD", link);

            Assert.AreNotEqual(link, Directory.GetCurrentDirectory(),
                "PRECONDITION: getcwd must answer the physical spelling, or this arm cannot tell the two apart");

            TestExecution parent = NewExecution("TestChdirSymlink");
            string? pwdDuring = null;
            bool bodyRan = false;

            parent.Run("child", t =>
            {
                bodyRan = true;
                t.Value.Execution.Chdir(".");
                pwdDuring = go.syscall_package.Getenv("PWD").Item1.ToString();
            });

            Assert.IsTrue(bodyRan, "the test body must have run, or nothing below means anything");
            Assert.AreEqual(link, pwdDuring,
                "t.Chdir(\".\") records PWD from os.Getwd, which keeps PWD's logical spelling -- not getcwd's physical one");

            TestReporter reporter = new("guard", json: false, verbose: false);
            TestRunner runner = new(new TestRegistry("guard", []), TestOptions.Parse(["-test.artifacts"]), reporter, ".", ".");
            string artifactDir = runner.MakeArtifactDir();

            Assert.AreEqual(Path.Combine(link, "_artifacts"), Path.GetDirectoryName(artifactDir),
                "the artifact root is filepath.Abs(\"_artifacts\"), joined to os.Getwd's logical spelling");
        }
        finally
        {
            Directory.SetCurrentDirectory(original);
            Environment.SetEnvironmentVariable("PWD", pwdBefore);

            if (syscallHadPwd)
                go.syscall_package.Setenv("PWD", syscallPwdBefore);
            else
                go.syscall_package.Unsetenv("PWD");

            TryDelete(root);
        }
    }

    private static void TryDelete(string directory)
    {
        try
        {
            Directory.Delete(directory, recursive: true);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
