#nullable enable

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

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

// golib's binder, the route for a method no generated arm answers, reads a value method's pointee through *T at each
// call, as Go's reflect does (checked against Go 1.26.10): a write after the bind is seen, and a nil *T binds and then
// panics at the call, with panicwrap's text for the type's own method and the nil-pointer fault for a promoted one.
[TestClass]
public class MethodValuePointeeTests
{
    private const string Panicwrap = "value method nilrecv.Cell.Get called using nil *Cell pointer";
    private const string Fault = "runtime error: invalid memory address or nil pointer dereference";

    private static int Index(Type t, string name) => GoReflect.GoMethodIndex(t, name);

    [TestMethod]
    public void AWriteAfterTheBindIsSeenAtTheCall()
    {
        ж<nilrecv_package.Cell> box = Ꮡ(new nilrecv_package.Cell { n = 3 });
        Func<nint> get = (Func<nint>)GoReflect.GoMethodValue(typeof(ж<nilrecv_package.Cell>), Index(typeof(ж<nilrecv_package.Cell>), "Get"), box);

        ж<nilrecv_package.Outer> outer = Ꮡ(new nilrecv_package.Outer { Inner = new nilrecv_package.Inner { i = 4 } });
        Func<nint> twice = (Func<nint>)GoReflect.GoMethodValue(typeof(ж<nilrecv_package.Outer>), Index(typeof(ж<nilrecv_package.Outer>), "Twice"), outer);

        box.Value.n = 5;
        outer.Value.Inner.i = 5;

        Assert.AreEqual((nint)5, get());
        Assert.AreEqual((nint)10, twice());
    }

    [TestMethod]
    public void ANilPointerBindsAndPanicsAtTheCallWithGosText()
    {
        Type cell = typeof(ж<nilrecv_package.Cell>);
        Type outer = typeof(ж<nilrecv_package.Outer>);

        Assert.AreEqual(Panicwrap, CallMessage(() => GoReflect.GoMethodValue(cell, Index(cell, "Get"), null), static d => ((Func<nint>)d)()), "bound, null");
        Assert.AreEqual(Panicwrap, CallMessage(() => GoReflect.GoMethodValue(cell, Index(cell, "Get"), global::go.ж<nilrecv_package.Cell>.NilBox), static d => ((Func<nint>)d)()), "bound, nil box");
        Assert.AreEqual(Panicwrap, CallMessage(() => GoReflect.GoMethodFunc(cell, Index(cell, "Get"))!, static d => ((Func<ж<nilrecv_package.Cell>, nint>)d)(null!)), "unbound");
        Assert.AreEqual(Fault, CallMessage(() => GoReflect.GoMethodValue(outer, Index(outer, "Twice"), null), static d => ((Func<nint>)d)()), "promoted, bound");
        Assert.AreEqual(Fault, CallMessage(() => GoReflect.GoMethodFunc(outer, Index(outer, "Twice"))!, static d => ((Func<ж<nilrecv_package.Outer>, nint>)d)(null!)), "promoted, unbound");
    }

    [TestMethod]
    public void ADeclaredMethodShadowsTheEmbedsAndPanicsWithPanicwrap()
    {
        const string ShadowPanicwrap = "value method nilrecv.Shadow.Get called using nil *Shadow pointer";
        Type shadow = typeof(ж<nilrecv_package.Shadow>);

        Assert.AreEqual(ShadowPanicwrap, CallMessage(() => GoReflect.GoMethodValue(shadow, Index(shadow, "Get"), null), static d => ((Func<nint>)d)()), "bound");
        Assert.AreEqual(ShadowPanicwrap, CallMessage(() => GoReflect.GoMethodFunc(shadow, Index(shadow, "Get"))!, static d => ((Func<ж<nilrecv_package.Shadow>, nint>)d)(null!)), "unbound");
    }

    [TestMethod]
    public void APanicwrapPackagePathIsEscapedAsGoSpellsItsSymbols()
    {
        Type d = typeof(ж<vdotx_package.D>);

        Assert.AreEqual("value method example.com/probe/v%2ex.D.M called using nil *D pointer",
            CallMessage(() => GoReflect.GoMethodValue(d, Index(d, "M"), null), static f => ((Func<nint>)f)()));
    }

    // The panic's message when the bind succeeds and the call panics; anything else is named.
    private static string CallMessage(Func<Delegate> bind, Func<Delegate, nint> call)
    {
        Delegate bound;

        try { bound = bind(); }
        catch (Exception ex) { return "bind threw " + ex.GetType().Name + ": " + ex.Message; }

        try { return "no panic: " + call(bound); }
        catch (PanicException ex) { return ex.Message; }
        catch (Exception ex) { return "call threw " + ex.GetType().Name + ": " + ex.Message; }
    }
}

public static class nilrecv_package
{
    public struct Cell { public nint n; }

    public struct Inner { public nint i; }

    // Outer promotes Inner's Twice through the forwarder go2cs-gen writes.
    public struct Outer
    {
        [GoEmbedded] public Inner Inner;
        public nint o;
    }

    public static nint Get(this Cell c) => c.n;

    public static nint Twice(this Inner x) => x.i * 2;

    [GoPromoted]
    public static nint Twice(this Outer target) => target.Inner.Twice();

    // Shadow declares Get and embeds Cell, which has Get too: its own method shadows the embed's, as in Go, so
    // go2cs-gen writes no forwarder and the method carries no [GoPromoted].
    public struct Shadow
    {
        [GoEmbedded] public Cell Cell;
    }

    public static nint Get(this Shadow s) => -s.Cell.n;
}

// A package whose last path element holds a '.', which Go's symbol names escape (gopkg.in/yaml.v3's shape).
[GoPackage("vdotx", ImportPath = "example.com/probe/v.x")]
public static class vdotx_package
{
    public struct D { public nint n; }

    public static nint M(this D d) => d.n;
}
