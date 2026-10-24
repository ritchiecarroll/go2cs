using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.builtin;

namespace GolibTests;

[TestClass]
public class GoroutineRootPanicTests
{
    // The goroutine root has two host hooks and they answer different questions. A NON-panic
    // exception may be CONTAINED — a host running many independent Go programs in one process wants
    // one program's managed defect to fail one program. A PANIC may only be OBSERVED: Go kills the
    // process on an unrecovered panic in any goroutine, converted code must do the same, and the
    // differential oracle depends on it.
    //
    // Observation exists because that fatal path was otherwise FRAMELESS — golib's backstop prints
    // the panic VALUE and exits, which is Go's own report for a program and useless to a host trying
    // to say WHICH test died and WHERE. net/rpc/jsonrpc is the case that made it matter: a panic on
    // an rpc goroutine took the test host down with no frame at all, and the package recorded zero
    // verdicts where six had already passed.
    //
    // Goroutine.Run — the root, minus the queueing — is called directly here for a reason the real
    // path cannot offer: on a pool thread the escaping panic is by definition unhandled, and the
    // process dies before an assertion could run. On the caller's own thread the SAME root runs the
    // same filters and the panic lands in the test's catch instead of in the runtime's.

    private static PanicException s_observed;
    private static Exception s_contained;

    // Installed ONCE for the assembly: both hooks are process-global and neither can be uninstalled
    // (a host installs its policy at startup and keeps it). Routing them to fields that each test
    // clears makes the assertions independent of test order.
    [ClassInitialize]
    public static void InstallHooks(TestContext context)
    {
        Goroutine.ObserveUnhandledPanic(panic => s_observed = panic);
        Goroutine.ContainUnhandledExceptions(ex => s_contained = ex);
    }

    [TestInitialize]
    public void ClearObservations()
    {
        s_observed = null;
        s_contained = null;
    }

    // The root marks Go's `panicking` as a panic escapes it (MainExit). Here the escape is caught on the test's own
    // thread and the process lives on, so the mark is forgotten after each test.
    [TestCleanup]
    public void ForgetFatalStart() => MainExit.ResetFatalStartForTest();

    // Kept out of the inliner's reach: the frame being asserted on has to exist to be found.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void raiseExplicitPanic()
    {
        throw panic("boom");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void dereferenceNilPointer()
    {
        ж<nint> p = nil;
        _ = p.Value;
    }

    [TestMethod]
    public void PanicEscapingTheRootIsObservedAndStillEscapes()
    {
        PanicException escaped = Assert.ThrowsException<PanicException>(
            static () => Goroutine.Run(raiseExplicitPanic));

        Assert.AreEqual("boom", escaped.Message);
        Assert.IsNotNull(s_observed, "the host observer must be handed a panic escaping the root");
        Assert.AreSame(escaped, s_observed, "the observer must see the panic that escaped, not a copy");
        Assert.IsNull(s_contained, "a panic must never be offered to the containment policy");
    }

    [TestMethod]
    public void ObservedPanicCarriesItsFaultSite()
    {
        // The whole point of observing: a report with no frame names nothing. The root adopts the
        // panic, which snapshots its origin, so even a panic that passed through no defer frame at
        // all reports where it faulted.
        Assert.ThrowsException<PanicException>(static () => Goroutine.Run(raiseExplicitPanic));

        StringAssert.Contains(s_observed.StackTrace, nameof(raiseExplicitPanic),
            "the observed panic must name the fault site, not the goroutine root");
    }

    [TestMethod]
    public void RuntimeErrorOnAGoroutineIsObservedAsAPanic()
    {
        // A nil dereference is a Go panic, not a managed defect, so it takes the panic path: raised
        // as one by the pointer itself, reported with Go's message, and NOT contained. (A nil deref
        // that golib does not raise directly — a raw NullReferenceException out of emitted code —
        // reaches the same place through RuntimeErrorPanic's mapping in the root's filter.)
        Assert.ThrowsException<PanicException>(
            static () => Goroutine.Run(dereferenceNilPointer));

        Assert.IsNotNull(s_observed, "a mapped runtime-error panic must reach the observer");
        StringAssert.Contains(s_observed.Message, "nil pointer dereference");
        StringAssert.Contains(s_observed.StackTrace, nameof(dereferenceNilPointer),
            "the synthesized panic was never thrown at the fault; adoption must have snapshotted it");
        Assert.IsNull(s_contained, "a runtime-error panic is a panic — never containment's business");
    }

    [TestMethod]
    public void NonPanicExceptionIsContainedAndNotObserved()
    {
        Goroutine.Run(static () => throw new InvalidOperationException("host defect"));

        Assert.IsInstanceOfType(s_contained, typeof(InvalidOperationException),
            "a non-panic exception is the containment policy's to take");
        Assert.IsNull(s_observed, "the panic observer must not see a managed exception");
    }

    [TestMethod]
    public void GoexitEndsTheGoroutineQuietly()
    {
        // runtime.Goexit ends THIS goroutine and nothing else — not a panic, not a host failure, and
        // the clause that handles it must keep winning over both hooks.
        Goroutine.Run(static () => throw new GoexitException());

        Assert.IsNull(s_observed);
        Assert.IsNull(s_contained);
    }
}
