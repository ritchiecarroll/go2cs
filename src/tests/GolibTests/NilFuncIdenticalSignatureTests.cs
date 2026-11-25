using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// A NIL func crosses between two func types of identical signature exactly as a live one does. Every defined
// func type is its own delegate, so a named func type and its unnamed form are two managed types of one
// signature: Go assigns between them both ways (one side unnamed) and converts between any two of them, while two
// different NAMED func types stay unassignable. The live-delegate arm in GoReflect.TryConvertTo already holds
// that rule; the canonical nil carrier's arm in TryMarshalAssignable did not, so reflect's TestConvert panicked on
// `{V((func())(nil)), V(MyFunc(nil))}` ("value of type func() cannot be converted to type reflect_test.MyFunc").

// A second package's named func type of the same signature as sample_package.SampleDelegate: nested in its own
// `<pkg>_package` class, as the converter declares a defined func type, so it reads as NAMED.
internal static class other_package
{
    internal delegate void OtherDelegate();
}

[TestClass]
public class NilFuncIdenticalSignatureTests
{
    [TestMethod]
    public void ANilFuncCrossesBetweenANamedFuncTypeAndItsUnnamedFormBothWays()
    {
        object unnamedNil = GoReflect.CanonicalNilFunc(typeof(Action));
        object namedNil = GoReflect.CanonicalNilFunc(typeof(sample_package.SampleDelegate));

        Assert.IsTrue(GoReflect.TryMarshalAssignable(unnamedNil, typeof(sample_package.SampleDelegate), out object? toNamed, GoReflect.GoTypeRelation.Assignable), "func() nil assigns to a named func type of its signature");
        Assert.IsNull(toNamed, "and lands as the slot's nil func");
        Assert.IsTrue(GoReflect.TryMarshalAssignable(namedNil, typeof(Action), out object? toUnnamed, GoReflect.GoTypeRelation.Assignable), "a named func type's nil assigns to its unnamed form");
        Assert.IsNull(toUnnamed, "and lands as the slot's nil func");
        Assert.IsTrue(GoReflect.TryMarshalAssignable(unnamedNil, typeof(sample_package.SampleDelegate), out object? converted), "and converts, the default relation");
        Assert.IsNull(converted, "as a nil func");
    }

    [TestMethod]
    public void TwoNamedFuncTypesConvertButDoNotAssign()
    {
        object namedNil = GoReflect.CanonicalNilFunc(typeof(sample_package.SampleDelegate));

        Assert.IsFalse(GoReflect.TryMarshalAssignable(namedNil, typeof(other_package.OtherDelegate), out _, GoReflect.GoTypeRelation.Assignable), "two different named func types are never assignable");
        Assert.IsTrue(GoReflect.TryMarshalAssignable(namedNil, typeof(other_package.OtherDelegate), out object? converted, GoReflect.GoTypeRelation.Convertible), "but identical underlying types convert");
        Assert.IsNull(converted, "as a nil func");
    }

    [TestMethod]
    public void ANilFuncOfAnotherSignatureStillCrossesToNothing()
    {
        object unnamedNil = GoReflect.CanonicalNilFunc(typeof(Action));

        Assert.IsFalse(GoReflect.TryMarshalAssignable(unnamedNil, typeof(Action<bool>), out _, GoReflect.GoTypeRelation.Convertible), "a different signature never converts");
        Assert.IsFalse(GoReflect.TryMarshalAssignable(unnamedNil, typeof(Func<bool>), out _, GoReflect.GoTypeRelation.Convertible), "nor does a different result");
    }
}
