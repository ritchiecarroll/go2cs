using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// ONE RULE: in interface space a typed nil func IS its func type's canonical carrier, and a shell or
// adapter over a null func stands for it. GoReflect.NilFuncOf states it; the type assertion, the dynamic
// type, the method bind, assignability, the type name and reflect's unwrap all read through it. Three
// shapes hold such a nil, and each used to answer for itself: the carrier (a nil func boxed into `any`),
// a generated value adapter whose wrapped delegate is null (a nil named func converted to an interface;
// ValueAdapterFormatTests' NilDelegateShell is that shape), and go2cs-gen's generic runtime shell closed
// over the delegate type (a nil func asserted to an interface; NilFuncProbeShell below is that shape).
// The Behavioral program NilFuncInterfaceReads holds the same rule against Go, through real emission.
internal interface INilFuncProbe
{
    nint Probe();
}

internal sealed class NilFuncProbeShell<T>(T target) : INilFuncProbe, IInterfaceAdapter
{
    private readonly T m_target = target;

    object? IInterfaceAdapter.Value => m_target;

    public nint Probe() => 1;
}

[TestClass]
public class NilFuncCarrierRuleTests
{
    private static readonly object s_carrier = GoReflect.CanonicalNilFunc(typeof(sample_package.SampleDelegate));

    [TestMethod]
    public void TheThreeShapesOfANilFuncStandForOneCarrier()
    {
        Assert.AreSame(s_carrier, GoReflect.NilFuncOf(s_carrier), "the carrier stands for itself");
        Assert.AreSame(s_carrier, GoReflect.NilFuncOf(new NilDelegateShell()), "a value adapter over a null delegate");
        Assert.AreSame(s_carrier, GoReflect.NilFuncOf(new NilFuncProbeShell<sample_package.SampleDelegate>(null!)), "a generic shell over a null delegate");
    }

    [TestMethod]
    public void AnythingElseStandsForNoNilFunc()
    {
        Assert.IsNull(GoReflect.NilFuncOf(null), "Go's nil interface");
        Assert.IsNull(GoReflect.NilFuncOf("text"), "a value that is no adapter");
        Assert.IsNull(GoReflect.NilFuncOf(new NilFuncProbeShell<sample_package.SampleDelegate>(() => { })), "a shell over a live func");
        Assert.IsNull(GoReflect.NilFuncOf(new NilFuncProbeShell<string>(null!)), "a shell over a null that is no func");
    }

    [TestMethod]
    public void AShellOverANilFuncReadsAsTheCarrierDoes()
    {
        object shell = new NilFuncProbeShell<sample_package.SampleDelegate>(null!);

        Assert.AreSame(typeof(sample_package.SampleDelegate), GoReflect.GoDynamicTypeOf(shell), "the dynamic type is the func type");
        Assert.AreEqual(go.builtin.GetGoTypeName(s_carrier), go.builtin.GetGoTypeName(shell), "the type name is the func type's");

        Assert.IsTrue(go.builtin._<sample_package.SampleDelegate>(shell, out sample_package.SampleDelegate asserted), "asserts to its own func type");
        Assert.IsNull(asserted, "the asserted value is Go's nil func");
        Assert.IsFalse(go.builtin._<Action<bool>>(shell, out _), "any other func type is Go's failed assertion");

        Assert.IsTrue(GoReflect.TryMarshalAssignable(shell, typeof(sample_package.SampleDelegate), out object? stored), "stores into its own slot type");
        Assert.IsNull(stored, "the slot representation of a nil func is null");
        Assert.IsTrue(GoReflect.TryMarshalAssignable(shell, typeof(INilFuncProbe), out object? kept), "an interface slot it already implements takes it");
        Assert.AreSame(shell, kept, "and takes it as itself");
    }

    [TestMethod]
    public void AValueAdapterOverANilFuncAssertsToItsOwnFuncType()
    {
        object adapter = new NilDelegateShell();

        Assert.AreSame(typeof(sample_package.SampleDelegate), GoReflect.GoDynamicTypeOf(adapter), "the dynamic type is the func type");
        Assert.IsTrue(go.builtin._<sample_package.SampleDelegate>(adapter, out sample_package.SampleDelegate asserted), "asserts to its own func type");
        Assert.IsNull(asserted, "the asserted value is Go's nil func");
        Assert.IsFalse(go.builtin._<Action<bool>>(adapter, out _), "any other func type is Go's failed assertion");
    }
}
