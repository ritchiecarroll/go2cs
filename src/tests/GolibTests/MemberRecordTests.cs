using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// The member record (docs/PLAN-marker-comment-parity.md, 5.6): converted code marks a Go embedded field with a
// `/*embed*/` comment, which go2cs-gen records on the declaring type as [GoMemberRecord(member, Embedded)].
// golib reads the field's [GoEmbedded] (hand-written files keep it) OR that record, and refuses BY NAME a
// record naming a field the type does not declare.
[TestClass]
public class MemberRecordTests
{
    private static GoReflect.GoFieldInfo Field(Type type, string name) =>
        GoReflect.GoFields(type).Single(field => field.Name == name);

    [TestMethod]
    public void TheRecordMakesTheFieldEmbedded()
    {
        Assert.IsTrue(Field(typeof(memberrecord_package.Recorded), "int").Embedded, "the recorded field is a Go embedded field");
        Assert.IsFalse(Field(typeof(memberrecord_package.Recorded), "named").Embedded, "an unrecorded field is not");
    }

    [TestMethod]
    public void TheHandWrittenAttributeStillMakesTheFieldEmbedded()
    {
        Assert.IsTrue(Field(typeof(memberrecord_package.Stamped), "int").Embedded);
    }

    [TestMethod]
    public void ARecordNamingAFieldTheTypeDoesNotDeclareIsRefusedByName()
    {
        InvalidOperationException refusal = Assert.ThrowsException<InvalidOperationException>(() => GoReflect.GoFields(typeof(memberrecord_package.Misrecorded)));

        StringAssert.Contains(refusal.Message, "'missing'");
        StringAssert.Contains(refusal.Message, "Misrecorded");
    }
}

// "package memberrecord" -- the converted shape (a record on the type), the hand-written shape (the
// attribute on the field), and a record that names nothing.
public static class memberrecord_package
{
    [GoMemberRecord("int", GoMemberFact.Embedded)]
    public struct Recorded
    {
        public nint @int;
        public nint named;
    }

    public struct Stamped
    {
        [GoEmbedded] public nint @int;
    }

    [GoMemberRecord("missing", GoMemberFact.Embedded)]
    public struct Misrecorded
    {
        public nint present;
    }
}
