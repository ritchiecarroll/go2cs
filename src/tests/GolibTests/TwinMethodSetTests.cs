#nullable enable

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests;

// COORD's ruling (2026-11-22) on train 3's default-mode regression: an M:N twin member is NEVER part of a Go method set
// as reflect, fmt or the interface checks see it. RootedUsingCapture printed `pointer methods: 4` where Go prints 3,
// because sync's hand-written `Lockꞏtwin(this ref Mutex)` is [GoRecv] with an upper-case name and reflect counted it.
// golib leaves a twin out by its return type (TypeExtensions.IsTwinMember: a GoTask or GoTask<T>), so the fixture
// declares both shapes a twin comes in: the hand-written one with no stamp (sync's), and a generated forwarder with its
// [GoParkForwarder] stamp, each AHEAD of its plain member so a scan meets the twin first.
[TestClass]
public class TwinMethodSetTests
{
    private static string[] Names(Type t) =>
        [.. Enumerable.Range(0, GoReflect.GoMethodCount(t)).Select(i => GoReflect.GoMethodName(t, i))];

    [TestMethod]
    public void NumMethodAndMethodNeverYieldATwin()
    {
        // Go: (*Mu) has Get, Lock, TryLock; Mu has Get.
        CollectionAssert.AreEqual(new[] { "Get", "Lock", "TryLock" }, Names(typeof(ж<twinms_package.Mu>)), "*Mu's NumMethod / Method(i)");
        CollectionAssert.AreEqual(new[] { "Get" }, Names(typeof(twinms_package.Mu)), "Mu's NumMethod / Method(i)");
    }

    [TestMethod]
    public void MethodByNameNeverFindsATwin()
    {
        Assert.AreEqual(-1, GoReflect.GoMethodIndex(typeof(ж<twinms_package.Mu>), "Lockꞏtwin"), "the hand-written twin");
        Assert.AreEqual(-1, GoReflect.GoMethodIndex(typeof(ж<twinms_package.Mu>), "Getꞏtwin"), "the stamped forwarder");
        Assert.AreEqual(1, GoReflect.GoMethodIndex(typeof(ж<twinms_package.Mu>), "Lock"), "the plain member keeps its index");
    }

    [TestMethod]
    public void TheCandidateSourceHoldsNoTwin()
    {
        foreach (bool pointer in new[] { true, false })
        {
            // By the fixture's names, not by IsTwinMember: a predicate the code under test supplies cannot fail.
            string[] twins = [.. TypeExtensions.GetGoMethodSetCandidates(typeof(twinms_package.Mu), pointer)
                .Select(static method => method.Name).Where(static name => name.EndsWith("ꞏtwin", StringComparison.Ordinal))];

            CollectionAssert.AreEqual(Array.Empty<string>(), twins, $"pointer={pointer}: the probe, the binder and fmt read this list");
        }
    }

    [TestMethod]
    public void AnInterfaceTwinMemberIsNotAGoMethod()
    {
        // The opt-in `GoTwinsIface=default` shape writes the twin onto the interface itself, with a body.
        CollectionAssert.AreEqual(new[] { "Lock" }, typeof(twinms_package.Locker).GetInterfaceMethods().Select(static method => method.Name).ToArray());
        Assert.IsTrue(typeof(ж<twinms_package.Mu>).StructurallyImplements(typeof(twinms_package.Locker)), "*Mu implements Locker");
        Assert.IsFalse(typeof(twinms_package.Mu).StructurallyImplements(typeof(twinms_package.Locker)), "Mu does not: Lock is a pointer method");
    }

    [TestMethod]
    public void PercentTOfALiftedInterfaceNeverPrintsATwin()
    {
        // P3's SHOULD on the twin seat: goInterfaceTypeString read GetMethods() itself, so under GoTwinsIface=default an
        // anonymous interface's `%T` printed `Lockꞏtwin() golib.GoTask` beside Go's one method.
        Assert.AreEqual("interface { Lock() }", GoReflect.GoTypeName(typeof(twinms_package.LiftedLocker)));
    }
}

public static class twinms_package
{
    public struct Mu { public nint state; }

    public interface Locker
    {
        void Lock();

        GoTask Lockꞏtwin() { Lock(); return default; }
    }

    // An anonymous interface the converter lifted (`interface{ Lock() }`), in the same opt-in twin shape.
    [GoType("dyn")]
    public interface LiftedLocker
    {
        void Lock();

        GoTask Lockꞏtwin() { Lock(); return default; }
    }

    // sync's shape: hand-written, no stamp, [GoRecv], an upper-case name.
    [GoRecv] internal static GoTask Lockꞏtwin(this ref Mu m) { m.Lock(); return default; }

    [GoRecv] public static void Lock(this ref Mu m) => m.state = 1;

    [GoRecv] public static bool TryLock(this ref Mu m) { if (m.state != 0) return false; m.state = 1; return true; }

    // go2cs-gen's forwarder shape: a value method's twin, stamped.
    [GoParkForwarder] public static GoTask<nint> Getꞏtwin(this Mu m) => new(m.Get());

    public static nint Get(this Mu m) => m.state;
}
