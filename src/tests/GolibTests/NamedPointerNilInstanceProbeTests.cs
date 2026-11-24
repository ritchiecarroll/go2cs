using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// R7 seat 2 (P3's NIT 4): reflection's typed nil of a NAMED pointer type is the wrapper's canonical NilInstance, which
/// go2cs-gen declares as ΔNilInstance when the type is itself named NilInstance (Common.PointerWrapperMember). The probe
/// reads both names: a generic type's CLR name carries its arity (NilInstance`1), so a test of the CLR name missed
/// ΔNilInstance there and the typed nil lost its type, silently.
/// </summary>
[TestClass]
public class NamedPointerNilInstanceProbeTests
{
    // Stand-ins for go2cs-gen's named-pointer wrappers: a plain one, one named NilInstance, and a GENERIC one named NilInstance.
    private sealed class plainNamed
    {
        internal static plainNamed NilInstance { get; } = new();
    }

    private sealed class NilInstance
    {
        internal static NilInstance ΔNilInstance { get; } = new();
    }

    private sealed class NilInstance<T>
    {
        internal static NilInstance<T> ΔNilInstance { get; } = new();
    }

    [TestMethod]
    public void TheProbeReadsTheWrappersOwnNilInstance() =>
        Assert.AreSame(plainNamed.NilInstance, GoReflect.CanonicalNilPointer(typeof(plainNamed)));

    [TestMethod]
    public void ATypeNamedNilInstanceAnswersItsDeltaNilInstance() =>
        Assert.AreSame(NilInstance.ΔNilInstance, GoReflect.CanonicalNilPointer(typeof(NilInstance)));

    [TestMethod]
    public void AGenericTypeNamedNilInstanceAnswersItsDeltaNilInstance() =>
        Assert.AreSame(NilInstance<int>.ΔNilInstance, GoReflect.CanonicalNilPointer(typeof(NilInstance<int>)));
}
