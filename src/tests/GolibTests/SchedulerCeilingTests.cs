using System;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

/// <summary>
/// M:N N5 (DESIGN-mn-n5-invisible-blocking.md 9, Q1): runtime/debug.SetMaxThreads sets the scheduler's thread ceiling
/// (GoScheduler.SetMaxThreads) and returns the old one, and a value past int32 is clamped to it, as Go's setMaxThreads
/// does (Go's TestSetMaxThreadsOvf). With the scheduler off this process has made no carriers, so no ceiling here is
/// below the threads made. The fatal path past the ceiling is read end to end by behavioral MnInvisible (MN_INV_ONLY=V7,
/// and GO2CS_MN_MAXTHREADS on V2).
/// </summary>
[TestClass]
[DoNotParallelize]
public class SchedulerCeilingTests
{
    [TestMethod]
    public void SetMaxThreadsReturnsTheOldCeiling()
    {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GO2CS_MN_MAXTHREADS")))
            Assert.Inconclusive("GO2CS_MN_MAXTHREADS sets the ceiling for this run: NOT MEASURED");

        nint old = GoScheduler.SetMaxThreads(5000);

        try
        {
            Assert.AreEqual((nint)10_000, old, "the default ceiling is Go's sched.maxmcount");
        }
        finally
        {
            Assert.AreEqual((nint)5000, GoScheduler.SetMaxThreads(old), "and the set value reads back");
        }
    }

    [TestMethod]
    public void AValuePastInt32IsClamped()
    {
        nint old = GoScheduler.SetMaxThreads(unchecked((nint)2147483648L));
        Assert.AreEqual((nint)int.MaxValue, GoScheduler.SetMaxThreads(old), "1<<31 is clamped to 0x7fffffff");
    }
}
