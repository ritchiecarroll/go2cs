#nullable enable

using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests;

// FROZEN SURFACE of go.sync and go.time from .5 (COORD's ruling, docs/phase4/inbox/G/20261122T114603Z-COORD.md; row 7
// pieces 2+3): generated code in a twinned user assembly names these two hand-written twins, so their exact signatures
// hold. A rename, a changed receiver or a lost `public` reds HERE, read off the built sync and time assemblies, never in
// a user's build.
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

    [TestMethod]
    public void TimesSleepTwinTakesADuration()
    {
        MethodInfo? twin = typeof(time_package).GetMethod("Sleepꞏtwin", BindingFlags.Public | BindingFlags.Static, [typeof(time_package.Duration)]);

        AssertFrozenTwin(twin, "time's Sleepꞏtwin(Duration)");
    }
}
