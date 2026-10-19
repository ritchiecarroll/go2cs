// PointerReinterpretUnaliasedTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// <c>ReinterpretUnaliased</c> is Go's <c>(*U)(p)</c> for two struct types identical ignoring tags whose C# forms cannot
/// alias: a nested unnamed type held through a reference differs in its tags, so C# lifts it twice, as two classes.
/// The conversion stands -- nil stays nil, a round trip is the source, comparison is a reinterpret view's -- and only a
/// read or a write through it refuses, by name (G's ruling on the pointer-conversion seat).
/// </summary>
/// <remarks>
/// The shape is the TypeConversion behavioral test's: <c>(*Person)(data)</c> where Person's <c>Address *struct{...}</c>
/// and data's carry different tags. Before, the converter emitted a value copy, so a write through the result was
/// silently lost; refusing on golib's address route would have named "native memory", which is wrong for a managed box.
/// </remarks>
[TestClass]
public class PointerReinterpretUnaliasedTests
{
    // The two lifts of one nested unnamed struct, distinct C# types because their Go tags differ.
    public struct PersonAddress
    {
        public @string Street;
    }

    public struct DataAddress
    {
        public @string Street;
    }

    public struct Person
    {
        public @string Name;
        public ж<PersonAddress> Address;
    }

    public struct Data
    {
        public @string Name;
        public ж<DataAddress> Address;
    }

    [TestMethod]
    public void ANilSourceConvertsToNil()
    {
        ж<Data> none = null!;

        Assert.IsTrue(none.ReinterpretUnaliased<Data, Person>() == nil, "(*Person)((*data)(nil)) == nil");
    }

    [TestMethod]
    public void ARoundTripIsTheSource()
    {
        ж<Data> d = Ꮡ(new Data { Name = "lee" });
        ж<Person> p = d.ReinterpretUnaliased<Data, Person>();

        Assert.IsTrue(p.ReinterpretUnaliased<Person, Data>() == d, "(*data)((*Person)(d)) == d");
        Assert.AreSame(d, p.ReinterpretUnaliased<Person, Data>(), "the round trip returns the source box itself");
    }

    [TestMethod]
    public void TwoConversionsOfOnePointerAreEqualAndHashAlike()
    {
        ж<Data> d = Ꮡ(new Data { Name = "max" });
        ж<Person> first = d.ReinterpretUnaliased<Data, Person>();
        ж<Person> second = d.ReinterpretUnaliased<Data, Person>();

        Assert.IsTrue(first == second, "(*Person)(d) == (*Person)(d)");
        Assert.AreEqual(first.GetHashCode(), second.GetHashCode(), "equal pointers must hash alike");
        Assert.IsFalse(first == nil, "the conversion of a non-nil pointer is not nil");
    }

    [TestMethod]
    public void AReadRefusesByName()
    {
        ж<Data> d = Ꮡ(new Data { Name = "ned" });
        ж<Person> p = d.ReinterpretUnaliased<Data, Person>();

        PanicException panic = Assert.ThrowsException<PanicException>(() => _ = p.Value.Name, "a read through the conversion answered");
        AssertNamesTheConversion(panic);
    }

    [TestMethod]
    public void AWriteRefusesByNameAndLeavesTheSourceAlone()
    {
        ж<Data> d = Ꮡ(new Data { Name = "ora" });
        ж<Person> p = d.ReinterpretUnaliased<Data, Person>();

        PanicException panic = Assert.ThrowsException<PanicException>(() => p.Value.Name = "pat", "a write through the conversion answered");
        AssertNamesTheConversion(panic);
        Assert.AreEqual("ora", d.Value.Name.ToString(), "a refused write changed the source");
    }

    // Go never panics here, so the refusal is a go2cs limitation: a plain panic carrying a Go string, never a
    // runtime.Error, or a recovering handler (net/http, fmt's catchPanic) would print it as Go's own behaviour.
    private static void AssertNamesTheConversion(PanicException panic)
    {
        string message = panic.Message;

        StringAssert.StartsWith(message, "go2cs: pointer conversion (*", "the refusal does not name itself a go2cs limitation and the conversion");
        StringAssert.Contains(message, "two C# classes, which cannot alias", "the refusal does not say why");
        Assert.IsInstanceOfType(panic.State, typeof(@string), "the panic value is not a plain Go string");
        Assert.IsFalse(message.Contains("runtime error"), $"the refusal reads as a Go runtime error: {message}");
        Assert.IsFalse(message.Contains("native memory"), $"the refusal speaks of native memory for a managed box: {message}");
    }
}
