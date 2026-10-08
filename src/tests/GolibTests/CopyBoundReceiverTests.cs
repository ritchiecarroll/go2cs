using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.builtin;

namespace GolibTests;

// A `this ref X` extension method marked [GoCopyBound] is a method of X's VALUE method set whose
// emitted receiver happens to be by reference: the generated forwarder of a pointer-receiver method
// promoted through an embedded POINTER (bufio.ReadWriter's ReadString, through *Reader). Go puts that
// method in the outer type's value set, and a by-ref receiver fits no delegate, so every run-time
// binder refused it — the reflect method table (Type.Method(i)) threw building its func type, a method
// value threw binding it, and the interface shell skipped it while the structural probe counted it.
//
// The rule under test (TypeExtensions.IsCopyBoundReceiver): such a method is bound through a COPY of
// its receiver, which is exactly Go's value-receiver semantics. Every OTHER by-ref receiver is a
// POINTER-set method and is never copied: an unmarked one, which is how converted code emits a Go
// pointer receiver since face lift A (docs/PLAN-marker-comment-parity.md, 5.1), and a [GoRecv] one,
// which hand-written files keep.
//
// The fixture is shaped like the emission: `Bump` is the [GoCopyBound] by-ref forwarder beside its ж
// twin, and it deliberately writes BOTH through the embedded pointer (shared by a copy) and to its own
// receiver (not shared), so a test can tell a copy from a reference.
[TestClass]
public class CopyBoundReceiverTests
{
    private static copybound_package.Holder NewHolder() =>
        new() { Inner = Ꮡ(new copybound_package.Inner { n = 10 }) };

    private static string[] Names(Type t) =>
        [.. Enumerable.Range(0, GoReflect.GoMethodCount(t)).Select(i => GoReflect.GoMethodName(t, i))];

    // Go: the value set holds Bump (pointer hop) and neither Poke ([GoRecv]) nor Prod (unmarked), both
    // pointer set only; the pointer set holds all three.
    [TestMethod]
    public void TheValueSetHoldsTheCopyBoundMethodAndNeitherPointerSetOne()
    {
        CollectionAssert.AreEqual(new[] { "Bump" }, Names(typeof(copybound_package.Holder)));
        CollectionAssert.AreEqual(new[] { "Bump", "Poke", "Prod" }, Names(typeof(ж<copybound_package.Holder>)));
    }

    // Type.Method(i).Type: func(Holder) int, the receiver BY VALUE. This is the call the reflect walk
    // died in ("Go method signature (Holder&) -> IntPtr has no Func<>/Action<> delegate form").
    [TestMethod]
    public void TheMethodTablesFuncTypeTakesTheReceiverByValue()
    {
        Type funcType = GoReflect.GoMethodFuncType(typeof(copybound_package.Holder), 0);
        ParameterInfo[] parameters = funcType.GetMethod("Invoke")!.GetParameters();

        Assert.AreEqual(1, parameters.Length);
        Assert.AreEqual(typeof(copybound_package.Holder), parameters[0].ParameterType);
    }

    // Type.Method(i).Func, called with a value: the method runs on a COPY. The embedded pointer is
    // shared, so its target moves; the receiver's own field does not.
    [TestMethod]
    public void TheUnboundFuncRunsOnACopyOfTheReceiver()
    {
        copybound_package.Holder holder = NewHolder();
        Delegate func = GoReflect.GoMethodFunc(typeof(copybound_package.Holder), 0)!;

        Assert.AreEqual((nint)11, func.DynamicInvoke(holder));
        Assert.AreEqual((nint)11, holder.Inner.Value.n, "the embedded pointer is shared with the copy");
        Assert.AreEqual((nint)0, holder.own, "the receiver itself was copied");
    }

    // Value.Method(i): the bound method value, called twice — a fresh copy per call.
    [TestMethod]
    public void TheMethodValueBindsThroughACopy()
    {
        copybound_package.Holder holder = NewHolder();
        Delegate bound = GoReflect.GoMethodValue(typeof(copybound_package.Holder), 0, holder);

        Assert.AreEqual((nint)11, bound.DynamicInvoke());
        Assert.AreEqual((nint)12, bound.DynamicInvoke());
        Assert.AreEqual((nint)12, holder.Inner.Value.n);
        Assert.AreEqual((nint)0, holder.own);
    }

    // Through *Holder the ж twin is the bound shape, so the call lands on the pointee itself.
    [TestMethod]
    public void ThroughThePointerTheTwinIsBoundAndNothingIsCopied()
    {
        ж<copybound_package.Holder> box = Ꮡ(NewHolder());
        Type pointer = typeof(ж<copybound_package.Holder>);
        Delegate bound = GoReflect.GoMethodValue(pointer, GoReflect.GoMethodIndex(pointer, "Bump"), box);

        Assert.AreEqual((nint)11, bound.DynamicInvoke());
        Assert.AreEqual((nint)1, box.Value.own, "the pointer set's method runs on the pointee");
    }

    // The interface probe and the shell binder must agree: the probe counts the copy-bound method for
    // a VALUE source, so the binder hands it over as the by-value form and a by-value delegate over it
    // can be created.
    [TestMethod]
    public void TheShellBinderHandsTheCopyBoundMethodOverAsTheByValueForm()
    {
        Assert.IsTrue(typeof(copybound_package.Holder).StructurallyImplements(typeof(copybound_package.Bumper)));

        AdapterBinder.ResolveReceiverMethods(typeof(copybound_package.Holder), "Bump", out MethodInfo? byPtr, out MethodInfo? byVal);

        Assert.IsNotNull(byPtr, "the ж twin is the pointer form");
        Assert.IsNotNull(byVal, "the copy-bound method is the value form");
        Assert.IsTrue(byVal!.GetParameters()[0].ParameterType.IsByRef);

        copybound_package.BumpByVal? shellForm = byVal.CreateStaticDelegate(typeof(copybound_package.BumpByVal)) as copybound_package.BumpByVal;

        Assert.IsNotNull(shellForm);

        copybound_package.Holder holder = NewHolder();

        Assert.AreEqual((nint)11, shellForm!(holder));
        Assert.AreEqual((nint)0, holder.own);
    }

    // The unmarked by-ref method — a converted Go pointer receiver — never becomes a value form either:
    // Go rejects the assertion, and so do the probe and the binder.
    [TestMethod]
    public void AnUnmarkedByRefMethodIsPointerSetAndNeverBoundThroughACopy()
    {
        Assert.IsFalse(typeof(copybound_package.Holder).StructurallyImplements(typeof(copybound_package.Prodder)));
        Assert.IsTrue(typeof(ж<copybound_package.Holder>).StructurallyImplements(typeof(copybound_package.Prodder)));

        AdapterBinder.ResolveReceiverMethods(typeof(copybound_package.Holder), "Prod", out MethodInfo? byPtr, out MethodInfo? byVal);

        Assert.IsNotNull(byPtr);
        Assert.IsNull(byVal, "an unmarked by-ref receiver has no value form");
        Assert.IsTrue(GoReflect.GoMethodIndex(typeof(copybound_package.Holder), "Prod") < 0, "the value method table does not list it");

        ж<copybound_package.Holder> box = Ꮡ(NewHolder());
        Type pointer = typeof(ж<copybound_package.Holder>);
        Delegate bound = GoReflect.GoMethodValue(pointer, GoReflect.GoMethodIndex(pointer, "Prod"), box);

        Assert.AreEqual((nint)1000, bound.DynamicInvoke());
        Assert.AreEqual((nint)1000, box.Value.own, "the pointer set's method runs on the pointee");
    }

    // The [GoRecv] method never becomes a value form: Go rejects the assertion, and so does the probe.
    [TestMethod]
    public void AGoRecvMethodIsNeverBoundThroughACopy()
    {
        Assert.IsFalse(typeof(copybound_package.Holder).StructurallyImplements(typeof(copybound_package.Poker)));
        Assert.IsTrue(typeof(ж<copybound_package.Holder>).StructurallyImplements(typeof(copybound_package.Poker)));

        AdapterBinder.ResolveReceiverMethods(typeof(copybound_package.Holder), "Poke", out MethodInfo? byPtr, out MethodInfo? byVal);

        Assert.IsNotNull(byPtr);
        Assert.IsNull(byVal, "a [GoRecv] by-ref receiver has no value form");
        Assert.IsNull(typeof(copybound_package).GetMethod("Poke", [typeof(copybound_package.Holder).MakeByRefType()])!
            .CreateStaticDelegate(typeof(copybound_package.PokeByVal)), "and no by-value delegate is minted over it");
    }

    // A [GoRecv] by-ref method with NO ж twin has no bindable shape at all. The refusal is a Go
    // PANIC — what a deferred recover() adopts — where it was a NotImplementedException no Go frame
    // sees.
    [TestMethod]
    public void AGoRecvMethodWithNoTwinIsRefusedAsARecoverablePanic()
    {
        Type pointer = typeof(ж<copybound_package.Lone>);
        int index = GoReflect.GoMethodIndex(pointer, "Only");

        Assert.IsTrue(index >= 0, "the pointer set lists the method");

        PanicException panic = Assert.ThrowsException<PanicException>(
            () => GoReflect.GoMethodValue(pointer, index, Ꮡ(new copybound_package.Lone())));

        StringAssert.Contains(panic.Message, "takes its receiver by reference");
    }

    // The hand-owned testing package opts out of the receiver rule ([assembly: GoHandOwnedPackage]), so
    // inside it a method is a pointer receiver by [GoRecv] alone; a converted package says nothing.
    [TestMethod]
    public void TheHandOwnedTestingPackageOptsOutAndAConvertedPackageDoesNot()
    {
        Assert.IsTrue(typeof(testing_package).Assembly.IsDefined(typeof(GoHandOwnedPackageAttribute), false));
        Assert.IsFalse(typeof(runtime_package).Assembly.IsDefined(typeof(GoHandOwnedPackageAttribute), false));
    }

    // The committed by-ref receivers that are NOT Go methods (the allowlist the corpus guard
    // TestByRefReceiversFollowTheReceiverRule keeps) never surface in a Go method-set walk, each for its
    // class.
    [TestMethod]
    public void TheAllowlistedHelpersNeverSurfaceInAMethodSetWalk()
    {
        // golib's slicing helpers: `in slice<T>`, unexported name — a method table is exported-only.
        foreach (Type t in new[] { typeof(slice<nint>), typeof(ж<slice<nint>>) })
        {
            Assert.IsTrue(GoReflect.GoMethodIndex(t, "slice") < 0);
            CollectionAssert.DoesNotContain(Names(t), "slice");
        }

        // ToUTF8Bytes: its receiver is a ref struct, which can never be boxed into a Go value, so no
        // dynamic type reaches the method table with it.
        MethodInfo toUtf8 = typeof(builtin).GetMethods().Single(m => m.Name == "ToUTF8Bytes" && m.GetParameters()[0].ParameterType.IsByRef);

        Assert.IsTrue(toUtf8.GetParameters()[0].ParameterType.GetElementType()!.IsByRefLike);
    }
}

// "package copybound" — a pointer-embed-shaped holder and the two by-ref receiver shapes.
public static class copybound_package
{
    public struct Inner
    {
        public nint n;
    }

    public struct Holder
    {
        public ж<Inner> Inner;
        public nint own;
    }

    public struct Lone;

    public interface Bumper
    {
        nint Bump();
    }

    public interface Poker
    {
        nint Poke();
    }

    public interface Prodder
    {
        nint Prod();
    }

    public delegate nint BumpByVal(Holder target);

    public delegate nint PokeByVal(Holder target);

    // The generated forwarder's shape for a pointer-receiver method promoted through *Inner: by-ref,
    // [GoCopyBound], with its ж twin beside it.
    [GoCopyBound] public static nint Bump(this ref Holder target)
    {
        target.own++;
        ref Inner inner = ref target.Inner.Value;
        inner.n++;
        return inner.n;
    }

    public static nint Bump(this ж<Holder> Ꮡtarget)
    {
        ref Holder target = ref Ꮡtarget.Value;
        return target.Bump();
    }

    // A pointer-receiver method of Holder itself: [GoRecv], with its ж twin.
    [GoRecv] public static nint Poke(this ref Holder target)
    {
        target.own += 100;
        return target.own;
    }

    public static nint Poke(this ж<Holder> Ꮡtarget)
    {
        ref Holder target = ref Ꮡtarget.Value;
        return target.Poke();
    }

    // A pointer-receiver method of Holder as converted code emits it: by-ref and UNMARKED, with its ж twin.
    public static nint Prod(this ref Holder target)
    {
        target.own += 1000;
        return target.own;
    }

    public static nint Prod(this ж<Holder> Ꮡtarget)
    {
        ref Holder target = ref Ꮡtarget.Value;
        return target.Prod();
    }

    // [GoRecv] with NO twin: nothing a delegate can bind.
    [GoRecv] public static nint Only(this ref Lone target) => 1;
}
