// PointerReinterpretRoundTripTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// A pointer conversion through <c>Reinterpret</c> and back is the SAME pointer in Go: <c>(*Employee)((*Person)(e)) == e</c>,
/// and through <c>unsafe.Pointer</c> as well. Pointer identity is equality, so these arms read <c>==</c>, the hash that a
/// pointer-keyed map uses, and the address <c>unsafe.Pointer</c> takes.
/// </summary>
/// <remarks>
/// <c>Reinterpret</c> returns a reinterpret VIEW over its source. Before the round-trip collapse a reinterpret of such a view
/// built a view over the VIEW, which no equality rule tied back to the original box, so the round trip read false while
/// writes through it still aliased (the PointerConversionAliasing probe, claude/c1-ptrconv-probe, at hop 5f774114a5). The
/// collapse re-views the ROOT box instead, and answers the root itself when the target is the root's own type.
/// </remarks>
[TestClass]
public class PointerReinterpretRoundTripTests
{
    // Two Go struct types with one underlying type: `type Employee struct{ Name string; Age int }` and Person alike.
    public struct Employee
    {
        public @string Name;
        public nint Age;
    }

    public struct Person
    {
        public @string Name;
        public nint Age;
    }

    public struct Staff
    {
        public @string Name;
        public nint Age;
    }

    // A struct holding an Employee, so `&pair.Second` is a field reference.
    public struct Pair
    {
        public Employee First;
        public Employee Second;
    }

    // A single-field wrapper over Employee: Wrapper -> Employee aliases (the wrapped field is at offset 0) and Employee ->
    // Person aliases, but Wrapper -> Person does not (neither is a wrapper of the other and the field sequences differ).
    public struct Wrapper
    {
        public Employee Inner;
    }

    private static ref Employee ᏑSecond(object p) => ref ((ж<Pair>)p).Value.Second;

    [TestMethod]
    public void ARoundTripOfAStandardBoxIsTheBoxItself()
    {
        ж<Employee> e = Ꮡ(new Employee { Name = "ann", Age = 30 });
        ж<Person> p = e.Reinterpret<Employee, Person>();

        Assert.IsTrue(p.Reinterpret<Person, Employee>() == e, "(*Employee)((*Person)(e)) == e");
        Assert.IsTrue(e == p.Reinterpret<Person, Employee>(), "e == (*Employee)((*Person)(e))");
    }

    [TestMethod]
    public void ARoundTripOfAFieldReferenceIsTheFieldReference()
    {
        ж<Pair> pair = Ꮡ(new Pair());
        ж<Employee> second = pair.of(ᏑSecond);
        ж<Person> p = second.Reinterpret<Employee, Person>();

        Assert.IsTrue(p.Reinterpret<Person, Employee>() == second, "(*Employee)((*Person)(&pair.Second)) == &pair.Second");
        Assert.IsTrue(p.Reinterpret<Person, Employee>() == pair.of(ᏑSecond), "the round trip equals a fresh &pair.Second");
    }

    [TestMethod]
    public void ARoundTripOfAnElementReferenceIsTheElementReference()
    {
        slice<Employee> s = new Employee[] { new() { Name = "bob", Age = 1 }, new() { Name = "cat", Age = 2 } }.slice();
        ж<Employee> element = Ꮡ(s, 1);
        ж<Person> p = element.Reinterpret<Employee, Person>();

        Assert.IsTrue(p.Reinterpret<Person, Employee>() == element, "(*Employee)((*Person)(&s[1])) == &s[1]");
        Assert.IsTrue(p.Reinterpret<Person, Employee>() == Ꮡ(s, 1), "the round trip equals a fresh &s[1]");
    }

    [TestMethod]
    public void AChainEqualsTheDirectReinterpretAndHashesAlike()
    {
        ж<Employee> e = Ꮡ(new Employee { Name = "dot", Age = 4 });
        ж<Staff> viaPerson = e.Reinterpret<Employee, Person>().Reinterpret<Person, Staff>();
        ж<Staff> direct = e.Reinterpret<Employee, Staff>();

        Assert.IsTrue(viaPerson == direct, "(*Staff)((*Person)(e)) == (*Staff)(e)");
        Assert.AreEqual(direct.GetHashCode(), viaPerson.GetHashCode(), "equal pointers must hash alike");
    }

    [TestMethod]
    public void TwoReinterpretsOfOneBoxAreEqualAndHashAlike()
    {
        ж<Employee> e = Ꮡ(new Employee { Name = "eve", Age = 5 });
        ж<Person> first = e.Reinterpret<Employee, Person>();
        ж<Person> second = e.Reinterpret<Employee, Person>();

        Assert.IsTrue(first == second, "(*Person)(e) == (*Person)(e)");
        Assert.AreEqual(first.GetHashCode(), second.GetHashCode(), "equal pointers must hash alike");
    }

    [TestMethod]
    public void UnsafePointerOfAReinterpretIsTheSourceAddress()
    {
        ж<Employee> e = Ꮡ(new Employee { Name = "fay", Age = 6 });
        ж<Person> p = e.Reinterpret<Employee, Person>();

        Assert.IsTrue(@unsafe.Pointer.FromPinnedBox(p) == @unsafe.Pointer.FromPinnedBox(e), "unsafe.Pointer((*Person)(e)) == unsafe.Pointer(e)");
        Assert.IsTrue(@unsafe.Pointer.FromPinnedBox(p.Reinterpret<Person, Employee>()) == @unsafe.Pointer.FromPinnedBox(e),
            "unsafe.Pointer of the round trip == unsafe.Pointer(e)");
    }

    // The converter's OTHER mint for unsafe.Pointer(p): FromPinnedBox for a plain pointer operand, FromBox on the
    // unsafePointerBoxEmission branch (a pointer parameter's box). Both spellings must agree with Go.
    [TestMethod]
    public void UnsafePointerFromBoxOfAReinterpretIsTheSourceAddress()
    {
        ж<Employee> e = Ꮡ(new Employee { Name = "jan", Age = 11 });
        ж<Person> p = e.Reinterpret<Employee, Person>();

        Assert.IsTrue(@unsafe.Pointer.FromBox(p) == @unsafe.Pointer.FromBox(e), "unsafe.Pointer((*Person)(e)) == unsafe.Pointer(e), FromBox");
        Assert.IsTrue(@unsafe.Pointer.FromBox(p.Reinterpret<Person, Employee>()) == @unsafe.Pointer.FromBox(e),
            "unsafe.Pointer of the round trip == unsafe.Pointer(e), FromBox");
        Assert.IsTrue(@unsafe.Pointer.FromBox(p) == @unsafe.Pointer.FromPinnedBox(e), "the two mints agree for one address");
    }

    [TestMethod]
    public void ANilSourceReinterpretsToNil()
    {
        ж<Employee> e = null!;

        Assert.IsTrue(e.Reinterpret<Employee, Person>() == nil, "(*Person)((*Employee)(nil)) == nil");
        Assert.IsTrue(e.Reinterpret<Employee, Person>().Reinterpret<Person, Employee>() == nil, "the round trip of nil is nil");
    }

    [TestMethod]
    public void AChainTheRootCannotAliasStillAliasesThroughTheView()
    {
        ж<Wrapper> w = Ꮡ(new Wrapper { Inner = new Employee { Name = "gus", Age = 7 } });
        ж<Person> p = w.Reinterpret<Wrapper, Employee>().Reinterpret<Employee, Person>();

        p.Value.Age = 8;
        Assert.AreEqual((nint)8, w.Value.Inner.Age, "a write through (*Person)((*Employee)(w)) reaches w.Inner");

        w.Value.Inner.Name = "hal";
        Assert.AreEqual("hal", p.Value.Name.ToString(), "a write through w is seen through the chain");
    }

    [TestMethod]
    public void WritesThroughTheRoundTripReachTheSource()
    {
        ж<Employee> e = Ꮡ(new Employee { Name = "ivy", Age = 9 });
        ж<Employee> back = e.Reinterpret<Employee, Person>().Reinterpret<Person, Employee>();

        back.Value.Age = 10;
        Assert.AreEqual((nint)10, e.Value.Age, "a write through the round trip reaches e");
    }
}
