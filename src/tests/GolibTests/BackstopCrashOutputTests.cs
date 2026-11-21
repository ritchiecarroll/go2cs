// BackstopCrashOutputTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.SafeHandles;

namespace GolibTests;

// Go writes EVERY fatal report to the descriptor runtime/debug.SetCrashOutput configured, as well as to stderr: a
// panic, a throw, a deadlock, main's Goexit. golib's CrashReport, FatalReport and MainGoexit tee to it. The process
// backstop did not when what escaped a goroutine was NOT a Go panic (a managed exception: a defect in converted code or
// in golib), so the one report an operator most needs from a crash file was missing from it (i9, run 42969401661).
//
// The backstop ends the process, so it is exercised in a CHILD: the parent re-runs this assembly's test host on one
// method with a crash file named in the environment; the child points the crash output at it and throws a .NET
// exception on a real goroutine; the backstop reports and exits 2. The test platform does not relay a crashed host's
// stderr, so the child also points Console.Error at a file: the backstop writes there, and nothing else does.
//
// The child holds the crash file's handle in a STATIC field. golib keeps only the descriptor's NUMBER, so a handle held
// in a local was unrooted once the method had read that number: a GC plus the finalizer closed the descriptor before
// the backstop wrote, the write failed silently, and the crash file was empty (one red in a full Release run at the
// 312 union; forced on demand, red 6 of 6). The child now forces that GC itself, so losing the root reds every run.
[TestClass]
public class BackstopCrashOutputTests
{
    private const string CrashFileVariable = "GO2CS_TEST_BACKSTOP_CRASH_FILE";
    private const string StdErrFileVariable = "GO2CS_TEST_BACKSTOP_STDERR_FILE";
    private const string Marker = "a managed exception escaping a goroutine (BackstopCrashOutputTests)";

    // The crash output's handle, rooted for the child's life. Never read: it exists to keep the descriptor open.
    private static SafeFileHandle? s_crashOutput;

    // The CHILD. Under the parent it never returns: golib's backstop exits the process. Run on its own, it does nothing.
    [TestMethod]
    public void ChildThrowsAManagedExceptionOnAGoroutine()
    {
        string? path = Environment.GetEnvironmentVariable(CrashFileVariable);

        if (string.IsNullOrEmpty(path))
            return;

        // Held open for the process's life, as debug.SetCrashOutput's duplicated descriptor is. A local is NOT held: the JIT
        // may treat it as dead after DangerousGetHandle, and its finalizer then closes the descriptor golib writes to.
        s_crashOutput = File.OpenHandle(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        CrashReport.SetCrashOutputFd(unchecked((nuint)(nint)s_crashOutput.DangerousGetHandle()));

        Console.SetError(new StreamWriter(Environment.GetEnvironmentVariable(StdErrFileVariable)!) { AutoFlush = true });

        // The tripwire: run the collection and the finalizers that could close an unrooted descriptor, every time, before the
        // backstop writes. With the handle rooted this changes nothing; with it unrooted the crash file is empty on every run.
        GC.Collect();
        GC.WaitForPendingFinalizers();

        builtin.goǃ(() => throw new InvalidOperationException(Marker));

        Thread.Sleep(TimeSpan.FromSeconds(60));
        Assert.Fail("golib's backstop did not end the process within 60 s");
    }

    [TestMethod]
    public void AManagedExceptionEscapingAGoroutineIsCopiedToTheCrashOutput()
    {
        string path = Path.Combine(Path.GetTempPath(), $"go2cs-backstop-{Guid.NewGuid():N}.out");
        string stderrPath = path + ".stderr";
        string filter = $"FullyQualifiedName={typeof(BackstopCrashOutputTests).FullName}.{nameof(ChildThrowsAManagedExceptionOnAGoroutine)}";

        ProcessStartInfo start = new("dotnet", ["test", typeof(BackstopCrashOutputTests).Assembly.Location, "--filter", filter])
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        start.Environment[CrashFileVariable] = path;
        start.Environment[StdErrFileVariable] = stderrPath;

        try
        {
            using Process process = Process.Start(start)!;
            Task<string> stdout = process.StandardOutput.ReadToEndAsync();
            Task<string> stderr = process.StandardError.ReadToEndAsync();

            if (!process.WaitForExit(180_000))
            {
                process.Kill(entireProcessTree: true);
                Assert.Fail("the backstop child did not finish in 180 s");
            }

            string child = stdout.Result + stderr.Result;
            string crash = File.Exists(path) ? File.ReadAllText(path) : "";
            string reported = File.Exists(stderrPath) ? File.ReadAllText(stderrPath) : "";

            Assert.AreNotEqual(0, process.ExitCode, $"the child's host should have crashed:\n{child}");
            // stderr's copy says which half failed: the report there and not here means the backstop ran and the crash output
            // lost the write; neither means the child never reached the backstop.
            Assert.IsTrue(crash.Contains($"System.InvalidOperationException: {Marker}"), $"the crash file lacks the report.\ncrash file:\n{crash}\nstderr file:\n{reported}\nchild:\n{child}");

            // The SAME report stderr got, byte for byte: the program wrote nothing else to either.
            Assert.AreEqual(reported, crash, "the crash file must hold exactly the report written to stderr");
        }
        finally
        {
            File.Delete(path);
            File.Delete(stderrPath);
        }
    }
}
