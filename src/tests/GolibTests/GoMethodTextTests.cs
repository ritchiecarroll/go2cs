// GoMethodTextTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Reflection;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.builtin;

namespace GolibTests;

// Method-set stand-ins, written the way the converter emits Go methods: extension methods over the receiver.
// None of them is a [GoType], so only golib's runtime path (ж<T>.ToString) reaches them; the generated
// value-type ToString is measured on real converted types below.

// G's review case: a VALUE-receiver Error() with a POINTER-receiver String(). Both are in *T's method set,
// and fmt's %v of &t picks by KIND, Error first, so the pointer prints "mix-error".
internal struct testMixValueError;

// The mirror: a VALUE-receiver String() with a POINTER-receiver Error(). *T prints the Error.
internal struct testMixPointerError;

// A pointer-receiver Error() that answers a nil receiver itself, as Go code may.
internal struct testNilAware;

// A String() that panics.
internal struct testBoom;

internal static class GoMethodTextTestExtensions
{
    internal static @string Error(this testMixValueError _) => "mix-error";

    internal static @string String(this ж<testMixValueError> _) => "mix-string";

    internal static @string String(this testMixPointerError _) => "mix2-string";

    internal static @string Error(this ж<testMixPointerError> _) => "mix2-error";

    internal static @string Error(this ж<testNilAware> p) => p == nil ? "nil receiver" : "set receiver";

    internal static @string String(this ж<testBoom> _) => throw panic("boom");
}

/// <summary>
/// A converted Go value's C# <c>ToString()</c> is its Go <c>%v</c> text: <c>Error()</c>, else <c>String()</c>,
/// chosen by method KIND across the method set Go gives the operand (GoMethodText). Before, a value printed its
/// fields or underlying value and a pointer printed its address.
/// </summary>
[TestClass]
public class GoMethodTextTests
{
    [TestMethod]
    public void AValueTypesStringMethodIsItsToString()
    {
        time_package.Duration twoSeconds = new(2_000_000_000);

        Assert.AreEqual("2s", twoSeconds.ToString(), "time.Duration has a value-receiver String()");
        Assert.AreEqual("January", time_package.January.ToString(), "time.Month's String()");
    }

    [TestMethod]
    public void AValueTypesErrorMethodIsItsToString()
    {
        syscall_package.Errno errno = new(2);
        string expected = syscall_package.Error(errno).ToString();

        Assert.AreNotEqual("2", expected, "the fixture: Errno(2) has error text");
        Assert.AreEqual(expected, errno.ToString(), "syscall.Errno has a value-receiver Error()");
    }

    [TestMethod]
    public void APointersMethodIsItsToString()
    {
        Assert.AreEqual("42", go.math.big_package.NewInt(42).ToString(), "*big.Int has a pointer-receiver String()");

        ж<reflect_package.ValueError> pointer = Ꮡ(new reflect_package.ValueError { Method = "M" });

        Assert.AreEqual("reflect: call of M on zero Value", pointer.ToString(), "*reflect.ValueError has a pointer-receiver Error()");
        Assert.AreNotEqual("reflect: call of M on zero Value", pointer.Value.ToString(),
            "a pointer-receiver Error() is NOT in the value's method set, so the value keeps its structural text");
    }

    [TestMethod]
    public void AnErrorHeldAsAnInterfacePrintsItsErrorText()
    {
        // errors.New returns a *errorString inside an `error`: the interface value's ToString is its dynamic value's.
        error diskFull = errors_package.New("disk full");

        Assert.AreEqual("disk full", diskFull.ToString());
        Assert.AreEqual("disk full after 1.5s", $"{diskFull} after {new time_package.Duration(1_500_000_000)}");
    }

    [TestMethod]
    public void APointerChoosesByMethodKindAcrossItsWholeMethodSet()
    {
        Assert.AreEqual("mix-error", Ꮡ(new testMixValueError()).ToString(),
            "a value-receiver Error() wins over a pointer-receiver String(): Error before String, whatever the receivers");
        Assert.AreEqual("mix2-error", Ꮡ(new testMixPointerError()).ToString(),
            "a pointer-receiver Error() wins over a value-receiver String()");
    }

    [TestMethod]
    public void ANilPointerCallsItsMethodAndPrintsNilOnlyWhenItPanics()
    {
        Assert.AreEqual("nil receiver", go.ж<testNilAware>.NilBox.ToString(), "a pointer-receiver method answers a nil receiver itself");
        Assert.AreEqual("<nil>", go.ж<testMixValueError>.NilBox.ToString(),
            "a value-receiver Error() through a nil pointer panics, and fmt's catchPanic prints <nil>");
    }

    [TestMethod]
    public void AMethodThatPanicsPrintsGosPanicText()
    {
        Assert.AreEqual("%!v(PANIC=String method: boom)", Ꮡ(new testBoom()).ToString());
    }

    [TestMethod]
    public void PrintAndPrintlnStillPrintAPointersAddress()
    {
        MethodInfo printArg = typeof(builtin).GetMethod("printArg", BindingFlags.NonPublic | BindingFlags.Static)!;
        ж<go.math.big_package.ΔInt> pointer = go.math.big_package.NewInt(42);
        string printed = (string)printArg.Invoke(null, [pointer])!;

        Assert.IsTrue(printed.StartsWith("0x"), $"println prints the address, never String(): {printed}");
    }
}
