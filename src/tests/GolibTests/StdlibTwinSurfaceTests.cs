#nullable enable

using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests;

// FROZEN SURFACE of go.sync and go.time from .5 (COORD's ruling, docs/phase4/inbox/G/20261122T114603Z-COORD.md; row 7
// pieces 2+3; WaitGroup's Wait twin by the owner's .5 ruling of 2026-11-27): generated code in a twinned user assembly
// names these hand-written twins, so their exact signatures hold. A rename, a changed receiver or a lost `public` reds
// HERE, read off the built sync and time assemblies, never in a user's build.
[TestClass]
public class StdlibTwinSurfaceTests
{
    private static void AssertFrozenTwin(MethodInfo? twin, string what)
    {
        Assert.IsNotNull(twin, what + " is public and static, with this exact parameter list");
        Assert.AreEqual(typeof(GoTask), twin!.ReturnType, what + " returns GoTask");
        Assert.AreEqual(EditorBrowsableState.Never, twin.GetCustomAttribute<EditorBrowsableAttribute>()?.State, what + " is hidden from IntelliSense");
        Assert.IsTrue(go.golib.TypeExtensions.IsTwinMember(twin), what + " is a twin, so no Go method set counts it");
    }

    [TestMethod]
    public void SyncsLockTwinTakesTheMutexBox()
    {
        MethodInfo? twin = typeof(syncᴛMutex).GetMethod("Lockꞏtwin", BindingFlags.Public | BindingFlags.Static, [typeof(ж<sync_package.Mutex>)]);

        AssertFrozenTwin(twin, "sync's Lockꞏtwin(this ж<Mutex>)");
        Assert.IsTrue(twin!.IsDefined(typeof(ExtensionAttribute), false), "an extension, so `Ꮡm.Lockꞏtwin()` binds");

        // The own-shape `this ref Mutex` twin is gone: beside it the generator would keep awaiting the old forwarder in
        // place (IsBoxTwinTarget), and a contended Lock would block its carrier again.
        Assert.IsFalse(typeof(syncᴛMutex).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            .Any(method => method.Name == "Lockꞏtwin" && method.GetParameters().FirstOrDefault()?.ParameterType.IsByRef == true),
            "no Lockꞏtwin(this ref Mutex) remains");
    }

    // The owner's .5 ruling (2026-11-27, docs/phase4/DESIGN-mn-waitgroup-twin.md): Wait's twin is frozen surface of go.sync
    // as Lock's is. It carries no GoParkWeak, so a caller that parks only through wg.Wait() becomes a twin.
    [TestMethod]
    public void SyncsWaitTwinTakesTheWaitGroupBox()
    {
        MethodInfo? twin = typeof(syncᴛWaitGroup).GetMethod("Waitꞏtwin", BindingFlags.Public | BindingFlags.Static, [typeof(ж<sync_package.WaitGroup>)]);

        AssertFrozenTwin(twin, "sync's Waitꞏtwin(this ж<WaitGroup>)");
        Assert.IsTrue(twin!.IsDefined(typeof(ExtensionAttribute), false), "an extension, so `Ꮡwg.Waitꞏtwin()` binds");
        Assert.IsFalse(twin.IsDefined(typeof(GoParkWeakAttribute), false), "not weak: a waiter's caller must become a twin to give its carrier back");
    }

    [TestMethod]
    public void TimesSleepTwinTakesADuration()
    {
        MethodInfo? twin = typeof(time_package).GetMethod("Sleepꞏtwin", BindingFlags.Public | BindingFlags.Static, [typeof(time_package.Duration)]);

        AssertFrozenTwin(twin, "time's Sleepꞏtwin(Duration)");
    }

    [TestMethod]
    public void NoOtherPublicTwinShipsInSyncOrTime()
    {
        // The frozen surface is exactly these members: a new public twin in either package is a surface change that needs
        // its own ruling, so it reds here first. Wait's twin joined by the owner's .5 ruling of 2026-11-27.
        string[] twins = [.. new[] { typeof(syncᴛMutex).Assembly, typeof(time_package).Assembly }
            .SelectMany(static assembly => assembly.GetTypes())
            .SelectMany(static type => type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(static method => method.Name.Contains("ꞏtwin", StringComparison.Ordinal) && method.DeclaringType is { IsVisible: true })
            .Select(static method => method.DeclaringType!.Name + "." + method.Name)
            .OrderBy(static name => name, StringComparer.Ordinal)];

        CollectionAssert.AreEqual(new[] { "syncᴛMutex.Lockꞏtwin", "syncᴛWaitGroup.Waitꞏtwin", "time_package.Sleepꞏtwin" }, twins, string.Join(", ", twins));
    }

    // Row 7's review (2026-11-22): a twin decides by the CALLING THREAD (GoScheduler.OnCarrier), not by GO2CS_MN, so off a
    // carrier it takes the plain, blocking path and hands back a completed task. Before the fold it decided by
    // GoScheduler.Enabled, and with MN=1 a twin off a carrier queued a waiter on the gate and then threw in ParkPending,
    // leaving that waiter for a later Unlock to wake while a real waiter hung. This process runs with MN unset; the row
    // holds the decision to the thread, the same answer the scheduler-on case needs.
    [TestMethod]
    public void OffACarrierTheStdlibTwinsTakeTheirPlainPath()
    {
        ж<sync_package.Mutex> box = builtin.Ꮡ(new sync_package.Mutex());
        box.Value.Lock();

        bool onCarrier = true, lockCompleted = false, sleepCompleted = false;
        Exception? failure = null;
        Thread thread = new(() =>
        {
            try
            {
                onCarrier = GoScheduler.OnCarrier;
                GoTask locked = box.Lockꞏtwin();        // contended: blocks in plain Lock until the main thread unlocks
                lockCompleted = locked.GetAwaiter().IsCompleted;
                locked.GetAwaiter().GetResult();
                box.Value.Unlock();

                GoTask slept = time_package.Sleepꞏtwin(new time_package.Duration(1_000_000));
                sleepCompleted = slept.GetAwaiter().IsCompleted;
                slept.GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });

        thread.Start();
        Thread.Sleep(100);
        box.Value.Unlock();

        Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(30)), "both calls returned");
        Assert.IsNull(failure, failure?.ToString());
        Assert.IsFalse(onCarrier, "a plain thread is no carrier");
        Assert.IsTrue(lockCompleted, "off a carrier, the Lock twin took the lock in plain Lock and returned a completed task");
        Assert.IsTrue(sleepCompleted, "off a carrier, the Sleep twin slept in plain Sleep and returned a completed task");
        Assert.IsTrue(box.Value.TryLock(), "the mutex is free afterwards: no waiter was left on its gate");
        box.Value.Unlock();
    }
}
