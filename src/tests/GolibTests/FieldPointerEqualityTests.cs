using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// A pointer to one struct field is ONE pointer in Go however it was taken: <c>&amp;s.A</c> in converted
/// code and <c>reflect.ValueOf(&amp;s).Elem().Field(0).Addr()</c> compare equal and find each other as map
/// keys. The two paths build their <see cref="FieldRefBox{T}"/> with different identity delegates -- the
/// generated <c>ᏑA</c> accessor and reflect's <c>goref_A</c> -- which made them unequal (the box-equality
/// census, ruled 2026-09-29: == false and a map miss, silently).
/// </summary>
[TestClass]
public class FieldPointerEqualityTests
{
    // The shape go2cs-gen emits: a Go struct and its generated field accessors.
    private struct S
    {
        internal nint A;
        internal nint B;

        internal static ref nint ᏑA(ref S instance) => ref instance.A;
        internal static ref nint ᏑB(ref S instance) => ref instance.B;
    }

    private static ж<nint> ReflectField(ж<S> box, string name) =>
        (ж<nint>)GoReflect.FieldAliasBox(box, Array.Find(GoReflect.GoFields(typeof(S)), field => field.Name == name));

    [TestMethod]
    public void AFieldPointerTakenThroughReflectEqualsTheConvertersOwn()
    {
        ж<S> s = Ꮡ(new S());

        ж<nint> converted = s.of(S.ᏑA);
        ж<nint> viaReflect = ReflectField(s, "A");

        Assert.IsTrue(viaReflect == converted, "reflect's Field(0).Addr() == &s.A, as in Go");
        Assert.IsTrue(converted == viaReflect, "and symmetrically");
        Assert.AreEqual(converted.GetHashCode(), viaReflect.GetHashCode(), "equal pointers hash alike");
    }

    [TestMethod]
    public void AConvertedFieldPointerKeyIsFoundByItsReflectTwin()
    {
        ж<S> s = Ꮡ(new S());
        Dictionary<ж<nint>, string> keys = new() { [s.of(S.ᏑA)] = "field A" };

        Assert.IsTrue(keys.TryGetValue(ReflectField(s, "A"), out string? found), "the map lookup through reflect's pointer hits");
        Assert.AreEqual("field A", found);
    }

    [TestMethod]
    public void DifferentFieldsOrDifferentStructsStayDifferentPointers()
    {
        ж<S> s = Ꮡ(new S());
        ж<S> t = Ꮡ(new S());

        Assert.IsFalse(ReflectField(s, "B") == s.of(S.ᏑA), "another field of the same struct is another pointer");
        Assert.IsFalse(ReflectField(t, "A") == s.of(S.ᏑA), "the same field of another struct is another pointer");
    }
}
