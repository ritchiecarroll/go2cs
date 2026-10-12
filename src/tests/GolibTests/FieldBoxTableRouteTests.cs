using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// reflect's addressable <c>Value.Field(i)</c> takes a struct's generated field-box table first (IGoFieldBoxSource; trim
/// stage 3d, family E): the table builds the very view converted code builds for <c>&amp;s.A</c>, <c>s.of(S.ᏑA)</c>, in
/// compiled code, where golib emitted a DynamicMethod that Native AOT cannot run. The DynamicMethod stays for a struct
/// with no table and for a name the table does not know, where only the JIT can run it.
/// </summary>
/// <remarks>
/// The route is read by IDENTITY: the table's view is cached per (box, accessor), so reflect's box IS converted code's
/// box for the same field, the same object, where the DynamicMethod route mints its own box per call.
/// </remarks>
[TestClass]
public class FieldBoxTableRouteTests
{
    // The shape go2cs-gen emits for a struct: its fields, their Ꮡ accessors, a blank field's own accessor, and the table.
    private struct S : IGoFieldBoxSource
    {
        internal nint A;
        internal nint B;
        internal nint _;

        internal static ref nint ᏑA(ref S instance) => ref instance.A;
        internal static ref nint ᏑB(ref S instance) => ref instance.B;
        private static ref nint goref__(object box) => ref ((ж<S>)box).ValueSlot._;

        // B is left out on purpose: a name the table does not know answers null.
        object? IGoFieldBoxSource.FieldBox(string field, object parentBox) => field switch
        {
            "A" => ((ж<S>)parentBox).of(ᏑA),
            "_" => ((ж<S>)parentBox).of(goref__),
            _ => null
        };
    }

    // The same fields with no table.
    private struct Untabled
    {
        internal nint A;

        internal static ref nint ᏑA(ref Untabled instance) => ref instance.A;
    }

    private static object ReflectField<T>(ж<T> box, string name) =>
        GoReflect.FieldAliasBox(box, Array.Find(GoReflect.GoFields(typeof(T)), field => field.Name == name));

    [TestMethod]
    public void AFieldBoxIsTheStructsTabledViewTheConvertersOwn()
    {
        ж<S> s = Ꮡ(new S());

        object viaReflect = ReflectField(s, "A");

        Assert.AreSame(s.of(S.ᏑA), viaReflect, "reflect's box for &s.A IS converted code's: the table's cached view");
        Assert.AreSame(viaReflect, ReflectField(s, "A"), "and the same box on every call");
    }

    [TestMethod]
    public void ATabledFieldBoxWritesThroughToTheStruct()
    {
        ж<S> s = Ꮡ(new S());

        GoReflect.WritePointerSlot(ReflectField(s, "A"), (nint)41);

        Assert.AreEqual((nint)41, s.Value.A, "the write lands in the struct the parent box holds");
        Assert.AreEqual((nint)41, GoReflect.ReadPointerSlot(ReflectField(s, "A")), "and reads back through reflect");
    }

    [TestMethod]
    public void ABlankFieldIsTabledByItsCSharpName()
    {
        ж<S> s = Ꮡ(new S());

        object blank = ReflectField(s, "_");

        Assert.AreSame(blank, ReflectField(s, "_"), "the blank field's view, cached like any other");
        GoReflect.WritePointerSlot(blank, (nint)3);
        Assert.AreEqual((nint)3, s.Value._, "it writes through to the blank field");
    }

    [TestMethod]
    public void ANameTheTableDoesNotKnowFallsBackToTheDynamicMethod()
    {
        ж<S> s = Ꮡ(new S());

        object first = ReflectField(s, "B");
        object second = ReflectField(s, "B");

        Assert.AreNotSame(first, second, "the DynamicMethod route mints a box per call");
        Assert.IsTrue((ж<nint>)first == (ж<nint>)second, "and they are still one Go pointer");
        GoReflect.WritePointerSlot(first, (nint)7);
        Assert.AreEqual((nint)7, s.Value.B, "the fallback still writes through");
    }

    [TestMethod]
    public void AStructWithNoTableKeepsTheDynamicMethod()
    {
        ж<Untabled> u = Ꮡ(new Untabled());

        object viaReflect = ReflectField(u, "A");

        Assert.AreNotSame(u.of(Untabled.ᏑA), viaReflect, "no table: the DynamicMethod's own box");
        Assert.IsTrue(u.of(Untabled.ᏑA) == (ж<nint>)viaReflect, "equal to converted code's by field name, as before");
    }
}
