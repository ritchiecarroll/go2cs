#nullable enable

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// golib takes a generated method-set arm (IGoMethodSet) only when its delegate is of the type golib's own binder builds,
// on the unbound path (Type.Method(i).Func) and the bound one (Value.Method(i)) alike, so a method value's reflected
// type never depends on which route answered. Skewed's arms are of another type; the binder answers instead.
[TestClass]
public class MethodSetArmGuardTests
{
    [TestMethod]
    public void AMethodValueArmOfAnotherTypeIsNotTaken()
    {
        Type t = typeof(armguard_package.Skewed);
        Delegate value = GoReflect.GoMethodValue(t, GoReflect.GoMethodIndex(t, "Get"), new armguard_package.Skewed { n = 7 });

        Assert.AreEqual(typeof(Func<nint>), value.GetType(), "the bound arm's Func<object> is refused for the binder's Func<nint>");
        Assert.AreEqual((nint)7, ((Func<nint>)value)());
    }

    [TestMethod]
    public void AnUnboundArmOfAnotherTypeIsNotTaken()
    {
        Type t = typeof(armguard_package.Skewed);
        Delegate func = GoReflect.GoMethodFunc(t, GoReflect.GoMethodIndex(t, "Get"))!;

        Assert.AreEqual(typeof(Func<armguard_package.Skewed, nint>), func.GetType(), "the unbound arm's Func<Skewed, object> is refused");
        Assert.AreEqual((nint)8, ((Func<armguard_package.Skewed, nint>)func)(new armguard_package.Skewed { n = 8 }));
    }
}

public static class armguard_package
{
    // A method set whose arms answer Get with delegates golib's binder would not build: object where Go's signature
    // returns int.
    public struct Skewed : IGoMethodSet
    {
        public nint n;

        readonly Delegate? IGoMethodSet.ᴛMethod(string method, bool pointer, bool bound, object? receiver) => (method, pointer, bound) switch
        {
            ("Get", false, false) => new Func<Skewed, object>(static s => s.n),
            ("Get", false, true) => new Func<object>(() => ((Skewed)receiver!).n),
            _ => null
        };
    }

    public static nint Get(this Skewed s) => s.n;
}
