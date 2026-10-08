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

    // Face lift D (5.4): a field's dims comment is recorded as [GoMemberRecord(member, Dims, dims)], a parameter's as
    // [GoParamDims(method, parameter types, position, dims)] on the method's declaring type.

    [TestMethod]
    public void TheRecordGivesAFieldItsDims()
    {
        CollectionAssert.AreEqual(new nint[] { 3 }, Field(typeof(memberrecord_package.DimsRecorded), "p").ArrayDims);
        Assert.IsNull(Field(typeof(memberrecord_package.DimsRecorded), "q").ArrayDims, "an unrecorded pointer field has none");
    }

    [TestMethod]
    public void ADimsRecordNamingAFieldTheTypeDoesNotDeclareIsRefusedByName()
    {
        InvalidOperationException refusal = Assert.ThrowsException<InvalidOperationException>(() => GoReflect.GoFields(typeof(memberrecord_package.DimsMisrecorded)));

        StringAssert.Contains(refusal.Message, "'absent'");
    }

    [TestMethod]
    public void TheRecordGivesEachParameterItsDims()
    {
        nint[]?[]? hash = GoReflect.FuncParamDims((Func<array<byte>, nint>)paramdims_package.hash);
        CollectionAssert.AreEqual(new nint[] { 32 }, hash![0]);

        nint[]?[]? fill = GoReflect.FuncParamDims((Action<nint, ж<array<int>>, array<array<byte>>>)paramdims_package.fill);
        Assert.IsNull(fill![0], "a parameter with no record");
        CollectionAssert.AreEqual(new nint[] { 2 }, fill[1], "a pointer to an array carries its pointee's dims");
        CollectionAssert.AreEqual(new nint[] { 4, 8 }, fill[2], "nested arrays, outermost first");

        Assert.IsNull(GoReflect.FuncParamDims((Func<array<byte>, nint>)paramdims_package.unrecorded), "a method with no record");
    }

    [TestMethod]
    public void AGenericMethodsParameterDimsAreReadThroughItsOpenKey()
    {
        // Face lift D2: a type built from a type parameter is keyed by its open definition, a bare type parameter
        // by null, and a func value bound to a constructed method finds the record of its definition.
        CollectionAssert.AreEqual(new nint[] { 2 }, GoReflect.FuncParamDims((Func<array<int>, int>)paramdims_generic.first<int>)![0]);

        nint[]?[]? pick = GoReflect.FuncParamDims((Func<@string, array<@string>, @string>)paramdims_generic.pick<@string>);
        Assert.IsNull(pick![0]);
        CollectionAssert.AreEqual(new nint[] { 3 }, pick[1]);
    }

    [TestMethod]
    public void TheHandWrittenAttributeStillGivesAParameterItsDims()
    {
        CollectionAssert.AreEqual(new nint[] { 4 }, GoReflect.FuncParamDims((Action<array<byte>>)paramdims_package.stamped)![0]);
    }

    [TestMethod]
    public void AParameterRecordThatCannotBeResolvedIsRefusedByName()
    {
        InvalidOperationException none = Assert.ThrowsException<InvalidOperationException>(() => GoReflect.FuncParamDims((Action<array<byte>>)paramdims_nomethod.present));
        StringAssert.Contains(none.Message, "'missing'");

        InvalidOperationException notArray = Assert.ThrowsException<InvalidOperationException>(() => GoReflect.FuncParamDims((Action<nint>)paramdims_notarray.count));
        StringAssert.Contains(notArray.Message, "'count'");
        StringAssert.Contains(notArray.Message, "not an array");

        InvalidOperationException outOfRange = Assert.ThrowsException<InvalidOperationException>(() => GoReflect.FuncParamDims((Action<array<byte>>)paramdims_position.only));
        StringAssert.Contains(outOfRange.Message, "parameter 3 of 'only'");

        InvalidOperationException ambiguous = Assert.ThrowsException<InvalidOperationException>(() => GoReflect.FuncParamDims((Action<array<byte>>)paramdims_twice.twin));
        StringAssert.Contains(ambiguous.Message, "more than one");
    }

    // Section 11 (docs/PLAN-marker-comment-parity.md): a field whose Go type is a defined type over an interface,
    // erased to a `using` alias, has its descriptor carrier recorded on the declaring type as
    // [GoMemberRecord(member, Descriptor, typeof(carrier))], which the converter writes in the package's metadata
    // file. A hand-written field keeps [GoDescriptorType(Self = ...)].

    [TestMethod]
    public void TheRecordGivesAFieldItsDescriptorCarrier()
    {
        Assert.AreEqual(typeof(memberrecord_package.Tokenᴅ), Field(typeof(memberrecord_package.DescriptorRecorded), "tok").DescriptorSelf);
        Assert.IsNull(Field(typeof(memberrecord_package.DescriptorRecorded), "plain").DescriptorSelf, "an unrecorded field has none");
    }

    [TestMethod]
    public void TheHandWrittenAttributeStillGivesAFieldItsDescriptorCarrier()
    {
        Assert.AreEqual(typeof(memberrecord_package.Tokenᴅ), Field(typeof(memberrecord_package.DescriptorStamped), "tok").DescriptorSelf);
    }

    [TestMethod]
    public void ADescriptorRecordThatCannotHoldItsFactIsRefusedByName()
    {
        InvalidOperationException missing = Assert.ThrowsException<InvalidOperationException>(() => GoReflect.GoFields(typeof(memberrecord_package.DescriptorMisrecorded)));
        StringAssert.Contains(missing.Message, "'vanished'");
        StringAssert.Contains(missing.Message, "DescriptorMisrecorded");

        InvalidOperationException notInterface = Assert.ThrowsException<InvalidOperationException>(() => GoReflect.GoFields(typeof(memberrecord_package.DescriptorNotInterface)));
        StringAssert.Contains(notInterface.Message, "'tok'");
        StringAssert.Contains(notInterface.Message, "not an interface");
    }
}

// "package paramdims" -- converted funcs whose array parameters' dims are recorded on the package class, one
// not recorded, and one hand-written with the attribute. The other classes each carry one record go2cs-gen
// could never write, which the reader refuses by name.
[GoParamDims("hash", new[] { typeof(array<byte>) }, 0, 32)]
[GoParamDims("fill", new[] { typeof(nint), typeof(ж<array<int>>), typeof(array<array<byte>>) }, 1, 2)]
[GoParamDims("fill", new[] { typeof(nint), typeof(ж<array<int>>), typeof(array<array<byte>>) }, 2, 4, 8)]
public static class paramdims_package
{
    public static nint hash(array<byte> b) => 0;

    public static void fill(nint n, ж<array<int>> p, array<array<byte>> grid) { }

    public static nint unrecorded(array<byte> b) => 0;

    public static void stamped([GoArrayDims(4)] array<byte> b) { }
}

[GoParamDims("first", new[] { typeof(array<>) }, 0, 2)]
[GoParamDims("pick", new Type?[] { null, typeof(array<>) }, 1, 3)]
public static class paramdims_generic
{
    public static T first<T>(array<T> a) => default!;

    public static T pick<T>(T x, array<T> a) => x;
}

[GoParamDims("missing", new[] { typeof(array<byte>) }, 0, 32)]
public static class paramdims_nomethod
{
    public static void present(array<byte> b) { }
}

[GoParamDims("count", new[] { typeof(nint) }, 0, 32)]
public static class paramdims_notarray
{
    public static void count(nint n) { }
}

[GoParamDims("only", new[] { typeof(array<byte>) }, 3, 32)]
public static class paramdims_position
{
    public static void only(array<byte> b) { }
}

[GoParamDims("twin", new[] { typeof(array<byte>) }, 0, 32)]
public static class paramdims_twice
{
    public static void twin(array<byte> b) { }

    public static void twin<T>(array<byte> b) { }
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

    [GoMemberRecord("p", GoMemberFact.Dims, 3)]
    public struct DimsRecorded
    {
        public ж<array<byte>> p;
        public ж<array<byte>> q;
    }

    [GoMemberRecord("absent", GoMemberFact.Dims, 3)]
    public struct DimsMisrecorded
    {
        public ж<array<byte>> present;
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

    // The converter's descriptor carrier for `type Token any`: uninhabited, carrying the Go name.
    [GoLocalName("Token")]
    public interface Tokenᴅ { }

    [GoMemberRecord("tok", GoMemberFact.Descriptor, typeof(Tokenᴅ))]
    public struct DescriptorRecorded
    {
        public object tok;
        public object plain;
    }

    public struct DescriptorStamped
    {
        [GoDescriptorType(Self = typeof(Tokenᴅ))] public object tok;
    }

    [GoMemberRecord("vanished", GoMemberFact.Descriptor, typeof(Tokenᴅ))]
    public struct DescriptorMisrecorded
    {
        public object present;
    }

    [GoMemberRecord("tok", GoMemberFact.Descriptor, typeof(Tagged))]
    public struct DescriptorNotInterface
    {
        public object tok;
    }
}
