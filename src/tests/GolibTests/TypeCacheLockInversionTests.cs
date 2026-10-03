using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TypeExtensions = go.golib.TypeExtensions;

namespace GolibTests;

// THE LOCK INVERSION (rooted 2026-10-03 from six hung published programs): golib's extension-method scan
// (TypeExtensions.GetExtensionMethods) held a lock across every assembly's GetTypes(), which can wait on
// the runtime's type-load lock; the AppDomain.AssemblyLoad handler that invalidates the scan
// (ClearTypeCaches) took the SAME lock -- and the runtime raises AssemblyLoad synchronously while
// holding its type-load lock. Thread A: scan holds the golib lock, waits on the runtime. Thread B: the
// runtime holds its lock, the handler waits on golib's. A ReadyToRun publish pinned to one CPU hung in 20
// of 23 runs.
//
// The arms hold a scan in flight through the internal scan probe and play the runtime's part by calling
// the handler from another thread. MSTest runs this assembly serially (no [Parallelize]), which is what
// makes setting the probe safe for the length of an arm.
[TestClass]
public class TypeCacheLockInversionTests
{
    private static readonly TimeSpan HandlerLimit = TimeSpan.FromSeconds(2);

    private static readonly TimeSpan ProbeLimit = TimeSpan.FromSeconds(5);

    [TestMethod]
    public void TheAssemblyLoadHandlerNeverWaitsForAnInFlightScan()
    {
        TypeExtensions.ClearTypeCaches(null, EventArgs.Empty);

        using var scanInFlight = new ManualResetEventSlim();
        using var handlerDone = new ManualResetEventSlim();

        TypeExtensions.ScanProbeForTest = _ =>
        {
            if (!scanInFlight.IsSet)
            {
                scanInFlight.Set();
                handlerDone.Wait(ProbeLimit);
            }
        };

        try
        {
            Task scan = Task.Run(() => TypeExtensions.GetExtensionMethods());
            Assert.IsTrue(scanInFlight.Wait(ProbeLimit), "control: the scan must reach the probe");

            Task handler = Task.Run(() =>
            {
                TypeExtensions.ClearTypeCaches(null, EventArgs.Empty);
                handlerDone.Set();
            });

            bool handlerFinished = handler.Wait(HandlerLimit);
            handlerDone.Set();
            scan.Wait(ProbeLimit * 2);

            Assert.IsTrue(handlerFinished,
                "the AssemblyLoad handler waited for an in-flight scan -- the runtime raises AssemblyLoad while holding its " +
                "type-load lock, so a handler that waits on a lock the scan holds across GetTypes() deadlocks the process");
        }
        finally
        {
            TypeExtensions.ScanProbeForTest = null;
        }
    }

    [TestMethod]
    public void AScanThatALoadInterruptedRescansRatherThanPublishingAStaleResult()
    {
        TypeExtensions.ClearTypeCaches(null, EventArgs.Empty);

        Assembly? first = null;
        int passes = 0;
        using var scanInFlight = new ManualResetEventSlim();
        using var handlerDone = new ManualResetEventSlim();

        TypeExtensions.ScanProbeForTest = assembly =>
        {
            first ??= assembly;

            if (assembly != first)
                return;

            passes++;

            if (!scanInFlight.IsSet)
            {
                scanInFlight.Set();
                handlerDone.Wait(ProbeLimit);
            }
        };

        try
        {
            Task scan = Task.Run(() => TypeExtensions.GetExtensionMethods());
            Assert.IsTrue(scanInFlight.Wait(ProbeLimit), "control: the scan must reach the probe");

            Task.Run(() =>
            {
                TypeExtensions.ClearTypeCaches(null, EventArgs.Empty);
                handlerDone.Set();
            }).Wait(HandlerLimit);

            handlerDone.Set();
            Assert.IsTrue(scan.Wait(ProbeLimit * 2), "the scan must finish");

            // The load landed mid-scan, so the first pass's result may be missing an assembly: it must not
            // be what the scan publishes. One more pass, started after the load, is the fresh answer.
            Assert.AreEqual(2, passes, "a scan interrupted by a load must scan again before it publishes");
        }
        finally
        {
            TypeExtensions.ScanProbeForTest = null;
            TypeExtensions.ClearTypeCaches(null, EventArgs.Empty);
        }
    }
}
