using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        TypeExtensions.ResetExtensionScanForTest();

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

    // A load landing mid-scan may leave the scan's pass without the new assembly: it must not be what the scan
    // publishes. The scan keeps what it has already visited (an assembly's extension methods never change after it
    // loads) and visits only the assembly that loaded meanwhile before it publishes.
    [TestMethod]
    public void AScanThatALoadInterruptedVisitsTheNewAssemblyBeforeItPublishes()
    {
        Assembly first = typeof(go.sync.atomic_package).Assembly;
        Assembly late = typeof(go.sort_package).Assembly;
        Assembly[] source = [first];
        ConcurrentDictionary<Assembly, int> visits = [];
        using var scanInFlight = new ManualResetEventSlim();
        using var handlerDone = new ManualResetEventSlim();

        TypeExtensions.ResetExtensionScanForTest();
        TypeExtensions.AssemblySourceForTest = () => Volatile.Read(ref source);
        TypeExtensions.ScanProbeForTest = assembly =>
        {
            visits.AddOrUpdate(assembly, 1, static (_, count) => count + 1);

            if (assembly == first && !scanInFlight.IsSet)
            {
                scanInFlight.Set();
                handlerDone.Wait(ProbeLimit);
            }
        };

        try
        {
            Task<(MethodInfo, Type)[]> scan = Task.Run(TypeExtensions.GetExtensionMethods);
            Assert.IsTrue(scanInFlight.Wait(ProbeLimit), "control: the scan must reach the probe");

            // The runtime loads `late` while the scan is in flight, and raises AssemblyLoad.
            Volatile.Write(ref source, [first, late]);

            Task.Run(() =>
            {
                TypeExtensions.ClearTypeCaches(null, EventArgs.Empty);
                handlerDone.Set();
            }).Wait(HandlerLimit);

            handlerDone.Set();
            Assert.IsTrue(scan.Wait(ProbeLimit * 2), "the scan must finish");

            Assert.AreEqual(1, visits.GetValueOrDefault(late), "a scan interrupted by a load must visit the new assembly before it publishes");
            Assert.AreEqual(1, visits.GetValueOrDefault(first), "the assembly already visited must not be visited again");
            Assert.IsTrue(scan.Result.Any(entry => entry.Item1.DeclaringType?.Assembly == late),
                "the published scan must hold the extension methods of the assembly that loaded mid-scan");
        }
        finally
        {
            TypeExtensions.ScanProbeForTest = null;
            TypeExtensions.AssemblySourceForTest = null;
            TypeExtensions.ResetExtensionScanForTest();
        }
    }

    // A load that lands after a scan published visits only the new assembly; the scan published before it is kept.
    [TestMethod]
    public void ALoadAfterAScanVisitsOnlyTheNewAssembly()
    {
        Assembly first = typeof(go.sync.atomic_package).Assembly;
        Assembly late = typeof(go.sort_package).Assembly;
        Assembly[] source = [first];
        ConcurrentDictionary<Assembly, int> visits = [];

        TypeExtensions.ResetExtensionScanForTest();
        TypeExtensions.AssemblySourceForTest = () => Volatile.Read(ref source);
        TypeExtensions.ScanProbeForTest = assembly => visits.AddOrUpdate(assembly, 1, static (_, count) => count + 1);

        try
        {
            (MethodInfo, Type)[] before = TypeExtensions.GetExtensionMethods();

            Volatile.Write(ref source, [first, late]);
            TypeExtensions.ClearTypeCaches(null, EventArgs.Empty);
            (MethodInfo, Type)[] after = TypeExtensions.GetExtensionMethods();

            Assert.AreEqual(1, visits.GetValueOrDefault(first), "a load must not make the scan visit an assembly it has already visited");
            Assert.AreEqual(1, visits.GetValueOrDefault(late), "the newly loaded assembly must be visited once");
            CollectionAssert.AreEqual(before, after.Take(before.Length).ToArray(), "the methods found before the load must be kept, in order");
            Assert.IsTrue(after.Skip(before.Length).Any(), "the newly loaded assembly's extension methods must be appended");
        }
        finally
        {
            TypeExtensions.ScanProbeForTest = null;
            TypeExtensions.AssemblySourceForTest = null;
            TypeExtensions.ResetExtensionScanForTest();
        }
    }
}
