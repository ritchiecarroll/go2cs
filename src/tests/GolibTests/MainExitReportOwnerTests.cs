// MainExitReportOwnerTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.golib;

namespace GolibTests;

// Go's paniclk, as golib.MainExit keeps it: the first fatal start owns the crash report, and every other fatal path
// waits behind it for good. A process race cannot show a second report reliably: on G-LAPTOP a goroutine's fatal error
// racing main's panic printed one report in 30 of 30 runs both before and after C1's F1, because the owner's exit
// usually ends the process within the window. So the decision itself is driven here: a thread that did not claim
// the report, arriving once the owner has reported, must never come back to print a second one. MSTest runs this
// assembly serially (no [Parallelize]), which is what makes the process-wide report state safe to set and reset.
[TestClass]
public class MainExitReportOwnerTests
{
    [TestCleanup]
    public void ForgetFatalStart() => MainExit.ResetFatalStartForTest();

    [TestMethod]
    public void ANonOwnerNeverComesBackToPrintASecondReport()
    {
        MainExit.ResetFatalStartForTest();

        // Another thread starts a fatal report (a Go throw on FatalReport's path, say) and writes it: it owns the report.
        Thread owner = new(() =>
        {
            MainExit.BeginFatal(2);
            MainExit.FatalReported();
        });

        owner.Start();
        owner.Join();

        // A later fatal path (main's own panic reaching the backstop) parks for good; the owner's exit ends the
        // process. Background, so the parked thread cannot hold the test host's own exit.
        bool cameBack = false;
        Thread late = new(() =>
        {
            MainExit.AwaitReportOwner();
            Volatile.Write(ref cameBack, true);
        }) { IsBackground = true };

        late.Start();

        Assert.IsFalse(late.Join(500), "a non-owner woke and went on to print beside the owner's report");
        Assert.IsFalse(Volatile.Read(ref cameBack));
    }

    [TestMethod]
    public void TheFirstFatalPathOwnsTheReportAndGoesOnToPrint()
    {
        MainExit.ResetFatalStartForTest();

        // The control: nothing owns the report, so this thread claims it and returns at once to print.
        bool cameBack = false;
        Thread first = new(() =>
        {
            MainExit.AwaitReportOwner();
            Volatile.Write(ref cameBack, true);
        }) { IsBackground = true };

        first.Start();

        Assert.IsTrue(first.Join(5000), "the first fatal path did not go on to print");
        Assert.IsTrue(Volatile.Read(ref cameBack));
    }
}
