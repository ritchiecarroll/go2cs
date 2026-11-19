using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

// reflect's kinded setters (src/core/reflect/value_impl.cs, the Set{Bool,Int,Uint,Float,Complex,String}
// family over setKinded).
//
// (a) COST. reflect's TestMapAlloc block 2 calls val.SetInt 500 times per AllocsPerRun run and wants at
// most 10 allocations. Measured on the i7 (docs/phase4/briefs/reflect-alloc-readings-2026-09-27.md
// section 3): 1,000 of its 1,002 counted objects per run were setKinded's op name, a @string copied
// from the "SetInt"u8 literal (and its System.String conversion for mustBeAssignable) on EVERY
// successful call. Go's setters allocate nothing; the name is only needed to build a panic.
//
// (b) KIND CHECK. Go's SetUint panics on a non-uint kind, like its siblings; without the check it stored
// the number into an int slot.
[TestClass]
public class ReflectSetterTests
{
    [ClassInitialize]
    public static void EnableCounting(TestContext _) => AllocationCounter.Enable();

    // The objects golib charged to this thread for one call of `action`, after one warm-up call (as
    // AllocsPerRun warms up).
    private static long Charge(Action action)
    {
        action();

        long before = AllocationCounter.CurrentThreadCount;

        action();

        return AllocationCounter.CurrentThreadCount - before;
    }

    private static reflect_package.Value Settable(object zero) =>
        reflect_package.New(reflect_package.TypeOf(zero)).Elem();

    [TestMethod]
    public void TheCounterSeesALiteralCopiedIntoAString()
    {
        // CONTROL: the copy the setters used to make on every call is a counted object, so a zero below
        // is a reading and not a blind counter.
        Assert.IsTrue(Charge(() => { @string op = "SetInt"u8; GC.KeepAlive(op); }) >= 1);
    }

    [TestMethod]
    public void ASuccessfulKindedSetChargesNoCountedObject()
    {
        reflect_package.Value b = Settable(false);
        reflect_package.Value i = Settable((nint)0);
        reflect_package.Value u = Settable((nuint)0);
        reflect_package.Value f = Settable(0.0);
        reflect_package.Value c = Settable(new complex128(0, 0));
        reflect_package.Value s = Settable((@string)"");
        @string text = "go";

        long setBool = Charge(() => b.SetBool(true));
        long setInt = Charge(() => i.SetInt(7));
        long setUint = Charge(() => u.SetUint(7));
        long setFloat = Charge(() => f.SetFloat(1.5));
        long setComplex = Charge(() => c.SetComplex(new complex128(1, 2)));
        long setString = Charge(() => s.SetString(text));

        Assert.AreEqual(7L, i.Int(), "SetInt stored");
        Assert.AreEqual(7UL, u.Uint(), "SetUint stored");

        Assert.AreEqual("SetBool 0, SetInt 0, SetUint 0, SetFloat 0, SetComplex 0, SetString 0",
            $"SetBool {setBool}, SetInt {setInt}, SetUint {setUint}, SetFloat {setFloat}, SetComplex {setComplex}, SetString {setString}",
            "counted objects per successful call");
    }

    [TestMethod]
    public void SetUintOnAnIntValuePanicsWithGosText()
    {
        reflect_package.Value v = Settable((nint)0);
        string? text = null;

        try { v.SetUint(5); }
        catch (PanicException ex) { text = ex.Message; }

        Assert.AreEqual("reflect: call of reflect.Value.SetUint on int Value", text);
        Assert.AreEqual(0L, v.Int(), "the int slot is not written");
    }

    [TestMethod]
    public void SetIntOnAUintValuePanicsWithGosText()
    {
        // CONTROL: the sibling that already checks, in the same text shape.
        reflect_package.Value v = Settable((nuint)0);
        string? text = null;

        try { v.SetInt(5); }
        catch (PanicException ex) { text = ex.Message; }

        Assert.AreEqual("reflect: call of reflect.Value.SetInt on uint Value", text);
    }
}
