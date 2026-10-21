// AssertInlineHeadTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.builtin;

namespace GolibTests;

internal struct inlineHeadTarget
{
    public nint Field;
}

// Plain C# interfaces: go2cs-gen emits no shell for them, so only a registered (nominal) adapter answers.
internal interface IInlineHeadProbe
{
    nint Probe();
}

internal interface IInlineHeadOther
{
    nint Other();
}

// The Go dynamic value inside the interface-from-interface adapter below: it implements IInlineHeadProbe itself.
internal sealed class inlineHeadInner : IInlineHeadProbe
{
    public nint Probe() => 7;
}

// An interface created from an interface (`var j J = i`): the adapter IMPLEMENTS the target interface too, and
// Go's dynamic value is the one it wraps.
internal sealed class inlineHeadInterfaceAdapter(object inner) : IInlineHeadProbe, IInterfaceAdapter
{
    public object? Value => inner;

    public nint Probe() => -1;
}

// A pointer-sourced interface value (`var p Probe = &t`), as AdapterImplTemplate emits it.
internal sealed class inlineHeadTargetжIInlineHeadProbe(ж<inlineHeadTarget> box) : IInlineHeadProbe, IжAdapter
{
    public object? Box => box;

    public nint Probe() => box.IsNilPointer ? -1 : box.Value.Field;
}

/// <summary>
/// <c>x._&lt;T&gt;()</c> starts with an inlined head that answers only a HIT it can prove cheaply, specialized at the
/// call site: for a concrete T (not <c>object</c>), <c>target is T</c>; for an interface T on a raw value (not an
/// adapter, not a typed nil func), <c>target is T</c> and then the itab slot. Every other value falls through to
/// TryTypeAssert's body, and so does every miss. These arms are the classes that must FALL THROUGH, each pinned to
/// the body's answer: an interface-from-interface adapter that implements the target itself, the same adapter
/// asserted to <c>any</c>, a pointer adapter asserted to its pointer, a raw string to <c>@string</c>, a typed nil
/// func, the nil interface and a typed nil <c>*T</c>; and the head's own hits, which must equal the body's.
/// </summary>
/// <remarks>
/// Each call site below is a non-generic method with T known, so in an optimized build the head is inlined there,
/// which is the shape converted code has. The answers do not depend on inlining.
/// </remarks>
[TestClass]
public class AssertInlineHeadTests
{
    [ClassInitialize]
    public static void RegisterAdapters(TestContext _)
    {
        AdapterRegistry.Register(typeof(ж<inlineHeadTarget>), typeof(IInlineHeadProbe),
            static box => new inlineHeadTargetжIInlineHeadProbe((ж<inlineHeadTarget>)box));
    }

    [TestMethod]
    public void AnAdapterThatImplementsTheTargetStillAnswersTheValueItWraps()
    {
        inlineHeadInner inner = new();
        object adapter = new inlineHeadInterfaceAdapter(inner);

        (IInlineHeadProbe? probe, bool ok) = adapter._<IInlineHeadProbe>(ᐧ);

        Assert.IsTrue(ok);
        Assert.AreSame(inner, probe, "Go's dynamic value is the wrapped one, never the adapter");
    }

    [TestMethod]
    public void AnAdapterAssertedToAnyAnswersTheValueItWraps()
    {
        inlineHeadInner inner = new();
        object adapter = new inlineHeadInterfaceAdapter(inner);

        (object? value, bool ok) = adapter._<object>(ᐧ);

        Assert.IsTrue(ok);
        Assert.AreSame(inner, value);
    }

    [TestMethod]
    public void APointerAdapterAssertedToItsPointerAnswersTheBox()
    {
        ж<inlineHeadTarget> box = new StandardBox<inlineHeadTarget>(new inlineHeadTarget { Field = 3 });
        object adapter = new inlineHeadTargetжIInlineHeadProbe(box);

        (ж<inlineHeadTarget>? pointer, bool ok) = adapter._<ж<inlineHeadTarget>>(ᐧ);

        Assert.IsTrue(ok);
        Assert.AreSame(box, pointer);
    }

    [TestMethod]
    public void ARawStringAssertedToGoStringIsConverted()
    {
        object raw = "go";

        (@string value, bool ok) = raw._<@string>(ᐧ);

        Assert.IsTrue(ok);
        Assert.AreEqual("go", value.ToString());
    }

    [TestMethod]
    public void ATypedNilFuncKeepsItsTypeThroughTheAssertion()
    {
        Action none = null;
        object nilFunc = none.OrTypedNilFunc();

        (Action? func, bool ok) = nilFunc._<Action>(ᐧ);
        Assert.IsTrue(ok, "Go: f, ok := any(nilFunc).(func()) is ok=true");
        Assert.IsNull(func);

        (IInlineHeadProbe? _, bool probeOk) = nilFunc._<IInlineHeadProbe>(ᐧ);
        Assert.IsFalse(probeOk, "an unnamed func type has no methods");
    }

    [TestMethod]
    public void TheNilInterfaceMissesEveryTarget()
    {
        object nilInterface = null;

        (ж<inlineHeadTarget>? pointer, bool ok) = nilInterface._<ж<inlineHeadTarget>>(ᐧ);
        Assert.IsFalse(ok);
        Assert.IsTrue(pointer is null || pointer.IsNilPointer);

        (IInlineHeadProbe? _, bool probeOk) = nilInterface._<IInlineHeadProbe>(ᐧ);
        Assert.IsFalse(probeOk);

        Assert.IsFalse(nilInterface._<nint>(out nint zero));
        Assert.AreEqual((nint)0, zero);
    }

    [TestMethod]
    public void ATypedNilPointerAssertsToItsTypeAndToItsInterfaces()
    {
        object typedNil = global::go.ж<inlineHeadTarget>.NilBox;

        (ж<inlineHeadTarget>? pointer, bool ok) = typedNil._<ж<inlineHeadTarget>>(ᐧ);
        Assert.IsTrue(ok, "Go: p, ok := any((*T)(nil)).(*T) is ok=true");
        Assert.AreSame(typedNil, pointer);

        (IInlineHeadProbe? probe, bool probeOk) = typedNil._<IInlineHeadProbe>(ᐧ);
        Assert.IsTrue(probeOk, "*T's method set is the same for a nil *T");
        Assert.AreSame(typedNil, ((IжAdapter)probe!).Box);
        Assert.AreEqual((nint)(-1), probe!.Probe());
    }

    [TestMethod]
    public void TheHeadsOwnHitsAreTheBodysAnswers()
    {
        ж<inlineHeadTarget> box = new StandardBox<inlineHeadTarget>(new inlineHeadTarget { Field = 5 });
        object pointer = box;
        object value = new inlineHeadTarget { Field = 6 };
        object number = (nint)9;

        Assert.AreSame(box, pointer._<ж<inlineHeadTarget>>(ᐧ).Item1);
        Assert.AreEqual((nint)6, value._<inlineHeadTarget>(ᐧ).Item1.Field);
        Assert.AreEqual((nint)9, number._<nint>());
        Assert.IsTrue(number._<nint>(out nint n) && n == 9);

        // An interface hit through the itab slot: twice, so the second is answered from the filled slot.
        for (int i = 0; i < 2; i++)
        {
            (IInlineHeadProbe? probe, bool ok) = pointer._<IInlineHeadProbe>(ᐧ);
            Assert.IsTrue(ok);
            Assert.AreSame(box, ((IжAdapter)probe!).Box);
            Assert.AreEqual((nint)5, probe!.Probe());
        }

        // A miss still takes the body's answer, and the throwing form still panics.
        Assert.IsFalse(number._<IInlineHeadOther>(ᐧ).Item2);
        Assert.ThrowsException<PanicException>(() => number._<@string>());
    }
}
