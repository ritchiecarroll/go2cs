using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// The member record (docs/PLAN-marker-comment-parity.md, 5.5 and 5.6): converted code marks a Go embedded field
// with a `/*embed*/` comment and carries a field's Go struct tag in a comment at the end of its line, which
// go2cs-gen records on the declaring type as [GoMemberRecord(member, Embedded)] and
// [GoMemberRecord(member, Tag, tag)]. golib reads the member's attribute ([GoEmbedded], [GoTag]; hand-written
// files keep them) OR that record, and refuses BY NAME a record naming a member the type does not declare.
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

    [TestMethod]
    public void TheRecordGivesTheFieldItsTag()
    {
        Assert.AreEqual("json:\"method\"", Field(typeof(memberrecord_package.Tagged), "Method").Tag);
        Assert.AreEqual("a:\"`*/\"", Field(typeof(memberrecord_package.Tagged), "Odd").Tag, "a tag only the quoted comment can hold");
        Assert.AreEqual("", Field(typeof(memberrecord_package.Tagged), "Plain").Tag, "an unrecorded field is untagged");
    }

    [TestMethod]
    public void TheRecordGivesAnEmbeddedFieldItsTagThroughItsProperty()
    {
        GoReflect.GoFieldInfo embedded = Field(typeof(memberrecord_package.EmbedTagged), "Inner");

        Assert.IsTrue(embedded.Embedded);
        Assert.AreEqual("json:\"inner\"", embedded.Tag);
    }

    [TestMethod]
    public void TheHandWrittenAttributeStillGivesTheFieldItsTag()
    {
        Assert.AreEqual("xml:\"v\"", Field(typeof(memberrecord_package.Stamped), "v").Tag);
    }

    [TestMethod]
    public void ATagRecordNamingAMemberTheTypeDoesNotDeclareIsRefusedByName()
    {
        InvalidOperationException refusal = Assert.ThrowsException<InvalidOperationException>(() => GoReflect.GoFields(typeof(memberrecord_package.MistaggedRecord)));

        StringAssert.Contains(refusal.Message, "'gone'");
        StringAssert.Contains(refusal.Message, "MistaggedRecord");
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
        [System.ComponentModel.Description("xml:\"v\"")] public nint v; // [GoTag] in converted and hand-written go2cs code
    }

    [GoMemberRecord("Method", GoMemberFact.Tag, "json:\"method\"")]
    [GoMemberRecord("Odd", GoMemberFact.Tag, "a:\"`*/\"")]
    public struct Tagged
    {
        public nint Method;
        public nint Odd;
        public nint Plain;
    }

    public struct Inner
    {
        public nint n;
    }

    // The converter's shape for an embedded struct: a partial property named for the embed (here a plain
    // one, which reflection reads the same way) over the backing field go2cs-gen mints for it.
    [GoMemberRecord("Inner", GoMemberFact.Tag, "json:\"inner\"")]
    public struct EmbedTagged
    {
        public Inner ʗInner;
        public Inner Inner => ʗInner;
    }

    [GoMemberRecord("gone", GoMemberFact.Tag, "json:\"gone\"")]
    public struct MistaggedRecord
    {
        public nint present;
    }

    [GoMemberRecord("missing", GoMemberFact.Embedded)]
    public struct Misrecorded
    {
        public nint present;
    }
}
