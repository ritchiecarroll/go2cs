// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using reflect = reflect_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using FieldOffsetAttribute = global::System.Runtime.InteropServices.FieldOffsetAttribute;
using LayoutKind = global::System.Runtime.InteropServices.LayoutKind;
using StructLayoutAttribute = global::System.Runtime.InteropServices.StructLayoutAttribute;
using static go.encoding.xml_package;
using ꓸꓸꓸany = Span<any>;

partial class xml_internal_test_package {

public partial struct DriveType /*num:nint*/;

public static DriveType HyperDrive => /* iota */ 0;
public static DriveType ImprobabilityDrive => 1;

public partial struct Passenger {
    public slice<@string> Name; /*`xml:"name"`*/
    public float32 Weight; /*`xml:"weight"`*/
}

public partial struct Ship {
    public EmptyStruct XMLName; /*`xml:"spaceship"`*/
    public @string Name; /*`xml:"name,attr"`*/
    public @string Pilot; /*`xml:"pilot,attr"`*/
    public DriveType Drive; /*`xml:"drive"`*/
    public nuint Age; /*`xml:"age"`*/
    public slice<ж<Passenger>> Passenger; /*`xml:"passenger"`*/
    internal @string secret;
}

public partial struct NamedType /*@string*/;

public partial struct Port {
    public EmptyStruct XMLName; /*`xml:"port"`*/
    public @string Type; /*`xml:"type,attr,omitempty"`*/
    public @string Comment; /*`xml:",comment"`*/
    public @string Number; /*`xml:",chardata"`*/
}

public partial struct Domain {
    public EmptyStruct XMLName; /*`xml:"domain"`*/
    public @string Country; /*`xml:",attr,omitempty"`*/
    public slice<byte> Name; /*`xml:",chardata"`*/
    public slice<byte> Comment; /*`xml:",comment"`*/
}

public partial struct Book {
    public EmptyStruct XMLName; /*`xml:"book"`*/
    public @string Title; /*`xml:",chardata"`*/
}

[StructLayout(LayoutKind.Explicit, Size = 8)] public partial struct Event {
    [FieldOffset(0)] public readonly EmptyStruct XMLName; /*`xml:"event"`*/
    [FieldOffset(0)] public nint Year; /*`xml:",chardata"`*/
}

[StructLayout(LayoutKind.Explicit, Size = 8)] public partial struct Movie {
    [FieldOffset(0)] public readonly EmptyStruct XMLName; /*`xml:"movie"`*/
    [FieldOffset(0)] public nuint Length; /*`xml:",chardata"`*/
}

[StructLayout(LayoutKind.Explicit, Size = 4)] public partial struct Pi {
    [FieldOffset(0)] public readonly EmptyStruct XMLName; /*`xml:"pi"`*/
    [FieldOffset(0)] public float32 Approximation; /*`xml:",chardata"`*/
}

[StructLayout(LayoutKind.Explicit, Size = 8)] public partial struct Universe {
    [FieldOffset(0)] public readonly EmptyStruct XMLName; /*`xml:"universe"`*/
    [FieldOffset(0)] public float64 Visible; /*`xml:",chardata"`*/
}

[StructLayout(LayoutKind.Explicit, Size = 1)] public partial struct Particle {
    [FieldOffset(0)] public readonly EmptyStruct XMLName; /*`xml:"particle"`*/
    [FieldOffset(0)] public bool HasMass; /*`xml:",chardata"`*/
}

public partial struct Departure {
    public EmptyStruct XMLName; /*`xml:"departure"`*/
    public time.Time When; /*`xml:",chardata"`*/
}

public partial struct SecretAgent {
    public EmptyStruct XMLName; /*`xml:"agent"`*/
    public @string Handle; /*`xml:"handle,attr"`*/
    public @string Identity;
    public @string Obfuscate; /*`xml:",innerxml"`*/
}

public partial struct NestedItems {
    public EmptyStruct XMLName; /*`xml:"result"`*/
    public slice<@string> Items; /*`xml:">item"`*/
    public slice<@string> Item1; /*`xml:"Items>item1"`*/
}

public partial struct NestedOrder {
    public EmptyStruct XMLName; /*`xml:"result"`*/
    public @string Field1; /*`xml:"parent>c"`*/
    public @string Field2; /*`xml:"parent>b"`*/
    public @string Field3; /*`xml:"parent>a"`*/
}

public partial struct MixedNested {
    public EmptyStruct XMLName; /*`xml:"result"`*/
    public @string A; /*`xml:"parent1>a"`*/
    public @string B; /*`xml:"b"`*/
    public @string C; /*`xml:"parent1>parent2>c"`*/
    public @string D; /*`xml:"parent1>d"`*/
}

public partial struct NilTest {
    public any A; /*`xml:"parent1>parent2>a"`*/
    public any B; /*`xml:"parent1>b"`*/
    public any C; /*`xml:"parent1>parent2>c"`*/
}

public partial struct Service {
    public EmptyStruct XMLName; /*`xml:"service"`*/
    public ж<Domain> Domain; /*`xml:"host>domain"`*/
    public ж<Port> Port; /*`xml:"host>port"`*/
    public any Extra1;
    public any Extra2; /*`xml:"host>extra2"`*/
}

internal static ж<Ship> nilStruct;

public partial struct EmbedA {
    /*embed*/ public EmbedC EmbedC;
    public EmbedB EmbedB;
    public @string FieldA;
    /*embed*/ internal embedD embedD;
}

public partial struct EmbedB {
    public @string FieldB;
    /*embed*/ public ж<EmbedC> EmbedC;
}

public partial struct EmbedC {
    public @string FieldA1; /*`xml:"FieldA>A1"`*/
    public @string FieldA2; /*`xml:"FieldA>A2"`*/
    public @string FieldB;
    public @string FieldC;
}

internal partial struct embedD {
    internal @string fieldD;
    public @string FieldE; // Promoted and visible when embedD is embedded.
}

public partial struct NameCasing {
    public EmptyStruct XMLName; /*`xml:"casing"`*/
    public @string Xy;
    public @string XY;
    public @string XyA; /*`xml:"Xy,attr"`*/
    public @string XYA; /*`xml:"XY,attr"`*/
}

public partial struct NamePrecedence {
    public global::go.encoding.xml_package.Name XMLName; /*`xml:"Parent"`*/
    public XMLNameWithoutTag FromTag; /*`xml:"InTag"`*/
    public XMLNameWithoutTag FromNameVal;
    public XMLNameWithTag FromNameTag;
    public @string InFieldName;
}

public partial struct XMLNameWithTag {
    public global::go.encoding.xml_package.Name XMLName; /*`xml:"InXMLNameTag"`*/
    public @string Value; /*`xml:",chardata"`*/
}

public partial struct XMLNameWithoutTag {
    public global::go.encoding.xml_package.Name XMLName;
    public @string Value; /*`xml:",chardata"`*/
}

public partial struct NameInField {
    public global::go.encoding.xml_package.Name Foo; /*`xml:"ns foo"`*/
}

public partial struct AttrTest {
    public nint Int; /*`xml:",attr"`*/
    public nint Named; /*`xml:"int,attr"`*/
    public float64 Float; /*`xml:",attr"`*/
    public uint8 Uint8; /*`xml:",attr"`*/
    public bool Bool; /*`xml:",attr"`*/
    public @string Str; /*`xml:",attr"`*/
    public slice<byte> Bytes; /*`xml:",attr"`*/
}

public partial struct AttrsTest {
    public slice<global::go.encoding.xml_package.Attr> Attrs; /*`xml:",any,attr"`*/
    public nint Int; /*`xml:",attr"`*/
    public nint Named; /*`xml:"int,attr"`*/
    public float64 Float; /*`xml:",attr"`*/
    public uint8 Uint8; /*`xml:",attr"`*/
    public bool Bool; /*`xml:",attr"`*/
    public @string Str; /*`xml:",attr"`*/
    public slice<byte> Bytes; /*`xml:",attr"`*/
}

public partial struct OmitAttrTest {
    public nint Int; /*`xml:",attr,omitempty"`*/
    public nint Named; /*`xml:"int,attr,omitempty"`*/
    public float64 Float; /*`xml:",attr,omitempty"`*/
    public uint8 Uint8; /*`xml:",attr,omitempty"`*/
    public bool Bool; /*`xml:",attr,omitempty"`*/
    public @string Str; /*`xml:",attr,omitempty"`*/
    public slice<byte> Bytes; /*`xml:",attr,omitempty"`*/
    public ж<@string> PStr; /*`xml:",attr,omitempty"`*/
}

public partial struct OmitFieldTest {
    public nint Int; /*`xml:",omitempty"`*/
    public nint Named; /*`xml:"int,omitempty"`*/
    public float64 Float; /*`xml:",omitempty"`*/
    public uint8 Uint8; /*`xml:",omitempty"`*/
    public bool Bool; /*`xml:",omitempty"`*/
    public @string Str; /*`xml:",omitempty"`*/
    public slice<byte> Bytes; /*`xml:",omitempty"`*/
    public ж<@string> PStr; /*`xml:",omitempty"`*/
    public ж<PresenceTest> Ptr; /*`xml:",omitempty"`*/
}

public partial struct AnyTest {
    public EmptyStruct XMLName; /*`xml:"a"`*/
    public @string Nested; /*`xml:"nested>value"`*/
    public AnyHolder AnyField; /*`xml:",any"`*/
}

public partial struct AnyOmitTest {
    public EmptyStruct XMLName; /*`xml:"a"`*/
    public @string Nested; /*`xml:"nested>value"`*/
    public ж<AnyHolder> AnyField; /*`xml:",any,omitempty"`*/
}

public partial struct AnySliceTest {
    public EmptyStruct XMLName; /*`xml:"a"`*/
    public @string Nested; /*`xml:"nested>value"`*/
    public slice<AnyHolder> AnyField; /*`xml:",any"`*/
}

public partial struct AnyHolder {
    public global::go.encoding.xml_package.Name XMLName;
    public @string XML; /*`xml:",innerxml"`*/
}

public partial struct RecurseA {
    public @string A;
    public ж<RecurseB> B;
}

public partial struct RecurseB {
    public ж<RecurseA> A;
    public @string B;
}

public partial struct PresenceTest {
    public ж<EmptyStruct> Exists;
}

public partial struct IgnoreTest {
    public @string PublicSecret; /*`xml:"-"`*/
}

public partial struct MyBytes /*[]byte*/;

public partial struct Data {
    public slice<byte> Bytes;
    public slice<byte> Attr; /*`xml:",attr"`*/
    public MyBytes Custom;
}

public partial struct Plain {
    public any V;
}

public partial struct MyInt /*num:nint*/;

public partial struct EmbedInt {
    /*embed*/ public MyInt MyInt;
}

public partial struct Strings {
    public slice<@string> X; /*`xml:"A>B,omitempty"`*/
}

public partial struct PointerFieldsTest {
    public global::go.encoding.xml_package.Name XMLName; /*`xml:"dummy"`*/
    public ж<@string> Name; /*`xml:"name,attr"`*/
    public ж<nuint> Age; /*`xml:"age,attr"`*/
    public ж<@string> Empty; /*`xml:"empty,attr"`*/
    public ж<@string> Contents; /*`xml:",chardata"`*/
}

public partial struct ChardataEmptyTest {
    public global::go.encoding.xml_package.Name XMLName; /*`xml:"test"`*/
    public ж<@string> Contents; /*`xml:",chardata"`*/
}

public partial struct PointerAnonFields {
    /*embed*/ public ж<MyInt> MyInt;
    /*embed*/ public ж<NamedType> NamedType;
}

public partial struct MyMarshalerTest {
}

internal static global::go.encoding.xml_package.Marshaler _ᴛ1ʗ = new xml_internal_test_package.MyMarshalerTestжMarshaler(((ж<MyMarshalerTest>)nil));

public static error MarshalXML(this ref MyMarshalerTest m, ж<global::go.encoding.xml_package.Encoder> e, global::go.encoding.xml_package.StartElement start) {
    e.EncodeToken(start);
    e.EncodeToken((global::go.encoding.xml_package.CharData)slice<byte>("hello world"u8));
    e.EncodeToken(new EndElement(start.Name));
    return default;
}

public partial struct MyMarshalerAttrTest {
}

internal static global::go.encoding.xml_package.MarshalerAttr _ᴛ2ʗ = new xml_internal_test_package.MyMarshalerAttrTestжMarshalerAttr(((ж<MyMarshalerAttrTest>)nil));

public static (global::go.encoding.xml_package.Attr, error) MarshalXMLAttr(this ref MyMarshalerAttrTest m, global::go.encoding.xml_package.Name name) {
    return (new Attr(name, "hello world"), default);
}

public static error UnmarshalXMLAttr(this ref MyMarshalerAttrTest m, global::go.encoding.xml_package.Attr attr) {
    return default;
}

public partial struct MarshalerStruct {
    public MyMarshalerAttrTest Foo; /*`xml:",attr"`*/
}

public partial struct InnerStruct {
    public global::go.encoding.xml_package.Name XMLName; /*`xml:"testns outer"`*/
}

public partial struct OuterStruct {
    /*embed*/ public InnerStruct InnerStruct;
    public nint IntAttr; /*`xml:"int,attr"`*/
}

public partial struct OuterNamedStruct {
    /*embed*/ public InnerStruct InnerStruct;
    public global::go.encoding.xml_package.Name XMLName; /*`xml:"outerns test"`*/
    public nint IntAttr; /*`xml:"int,attr"`*/
}

public partial struct OuterNamedOrderedStruct {
    public global::go.encoding.xml_package.Name XMLName; /*`xml:"outerns test"`*/
    /*embed*/ public InnerStruct InnerStruct;
    public nint IntAttr; /*`xml:"int,attr"`*/
}

public partial struct OuterOuterStruct {
    /*embed*/ public OuterStruct OuterStruct;
}

public partial struct NestedAndChardata {
    public slice<@string> AB; /*`xml:"A>B"`*/
    public @string Chardata; /*`xml:",chardata"`*/
}

public partial struct NestedAndComment {
    public slice<@string> AB; /*`xml:"A>B"`*/
    public @string Comment; /*`xml:",comment"`*/
}

public partial struct CDataTest {
    public @string Chardata; /*`xml:",cdata"`*/
}

public partial struct NestedAndCData {
    public slice<@string> AB; /*`xml:"A>B"`*/
    public @string CDATA; /*`xml:",cdata"`*/
}

internal static any ifaceptr(any xʗp) {
    ref var x = ref heap(xʗp, out var Ꮡx);

    return Ꮡx;
}

internal static ж<@string> stringptr(@string xʗp) {
    ref var x = ref heap(xʗp, out var Ꮡx);

    return Ꮡx;
}

public partial struct T1 {
}

public partial struct T2 {
}

public partial struct IndirComment {
    public T1 T1;
    public ж<@string> Comment; /*`xml:",comment"`*/
    public T2 T2;
}

public partial struct DirectComment {
    public T1 T1;
    public @string Comment; /*`xml:",comment"`*/
    public T2 T2;
}

public partial struct IfaceComment {
    public T1 T1;
    public any Comment; /*`xml:",comment"`*/
    public T2 T2;
}

public partial struct IndirChardata {
    public T1 T1;
    public ж<@string> Chardata; /*`xml:",chardata"`*/
    public T2 T2;
}

public partial struct DirectChardata {
    public T1 T1;
    public @string Chardata; /*`xml:",chardata"`*/
    public T2 T2;
}

public partial struct IfaceChardata {
    public T1 T1;
    public any Chardata; /*`xml:",chardata"`*/
    public T2 T2;
}

public partial struct IndirCDATA {
    public T1 T1;
    public ж<@string> CDATA; /*`xml:",cdata"`*/
    public T2 T2;
}

public partial struct DirectCDATA {
    public T1 T1;
    public @string CDATA; /*`xml:",cdata"`*/
    public T2 T2;
}

public partial struct IfaceCDATA {
    public T1 T1;
    public any CDATA; /*`xml:",cdata"`*/
    public T2 T2;
}

public partial struct IndirInnerXML {
    public T1 T1;
    public ж<@string> InnerXML; /*`xml:",innerxml"`*/
    public T2 T2;
}

public partial struct DirectInnerXML {
    public T1 T1;
    public @string InnerXML; /*`xml:",innerxml"`*/
    public T2 T2;
}

public partial struct IfaceInnerXML {
    public T1 T1;
    public any InnerXML; /*`xml:",innerxml"`*/
    public T2 T2;
}

public partial struct IndirElement {
    public T1 T1;
    public ж<@string> Element;
    public T2 T2;
}

public partial struct DirectElement {
    public T1 T1;
    public @string Element;
    public T2 T2;
}

public partial struct IfaceElement {
    public T1 T1;
    public any Element;
    public T2 T2;
}

public partial struct IndirOmitEmpty {
    public T1 T1;
    public ж<@string> OmitEmpty; /*`xml:",omitempty"`*/
    public T2 T2;
}

public partial struct DirectOmitEmpty {
    public T1 T1;
    public @string OmitEmpty; /*`xml:",omitempty"`*/
    public T2 T2;
}

public partial struct IfaceOmitEmpty {
    public T1 T1;
    public any OmitEmpty; /*`xml:",omitempty"`*/
    public T2 T2;
}

public partial struct IndirAny {
    public T1 T1;
    public ж<@string> Any; /*`xml:",any"`*/
    public T2 T2;
}

public partial struct DirectAny {
    public T1 T1;
    public @string Any; /*`xml:",any"`*/
    public T2 T2;
}

public partial struct IfaceAny {
    public T1 T1;
    public any Any; /*`xml:",any"`*/
    public T2 T2;
}

public partial struct Generic<T> {
    public T X;
}

internal static readonly ж<@string> ᏑnameAttr = new StaticBox<@string>("Sarah");
internal static ref @string nameAttr => ref ((StaticBox<@string>)ᏑnameAttr).ValueSlot;
internal static readonly ж<nuint> ᏑageAttr = new StaticBox<nuint>((nuint)12);
internal static ref nuint ageAttr => ref ((StaticBox<nuint>)ᏑageAttr).ValueSlot;
internal static readonly ж<@string> ᏑcontentsAttr = new StaticBox<@string>("lorem ipsum");
internal static ref @string contentsAttr => ref ((StaticBox<@string>)ᏑcontentsAttr).ValueSlot;
internal static readonly ж<@string> Ꮡempty = new StaticBox<@string>("");
internal static ref @string empty => ref ((StaticBox<@string>)Ꮡempty).ValueSlot;

// Unless explicitly stated as such (or *Plain), all of the
// tests below are two-way tests. When introducing new tests,
// please try to make them two-way as well to ensure that
// marshaling and unmarshaling are as symmetrical as feasible.
partial struct marshalTestsᴛ1 /*dyn*/ {
    public any Value;
    public @string ExpectXML;
    public bool MarshalOnly;
    public @string MarshalError;
    public bool UnmarshalOnly;
    public @string UnmarshalError;
}
        partial struct Δtype /*dyn*/ {
            public EmptyStruct XMLName; /*`xml:"space top"`*/
            public @string A; /*`xml:"x>a"`*/
            public @string B; /*`xml:"x>b"`*/
            public @string C; /*`xml:"space x>c"`*/
            public @string C1; /*`xml:"space1 x>c"`*/
            public @string D1; /*`xml:"space1 x>d"`*/
        }
        partial struct Δtypeᴛ1 /*dyn*/ {
            public global::go.encoding.xml_package.Name XMLName;
            public @string A; /*`xml:"x>a"`*/
            public @string B; /*`xml:"x>b"`*/
            public @string C; /*`xml:"space x>c"`*/
            public @string C1; /*`xml:"space1 x>c"`*/
            public @string D1; /*`xml:"space1 x>d"`*/
        }
        partial struct Δtypeᴛ2 /*dyn*/ {
            public EmptyStruct XMLName; /*`xml:"top"`*/
            public @string B; /*`xml:"space x>b"`*/
            public @string B1; /*`xml:"space1 x>b"`*/
        }
internal static slice<marshalTestsᴛ1> marshalTests;
internal static void initᴛmarshalTests() { marshalTests = slice<marshalTestsᴛ1>([
    // Test nil marshals to nothing
    new(Value: default, ExpectXML: @"", MarshalOnly: true),
    new(Value: nilStruct.OrTypedNil(), ExpectXML: @"", MarshalOnly: true),

    // Test value types
    new(Value: Ꮡ(new Plain(true)), ExpectXML: @"<Plain><V>true</V></Plain>"),
    new(Value: Ꮡ(new Plain(false)), ExpectXML: @"<Plain><V>false</V></Plain>"),
    new(Value: Ꮡ(new Plain((nint)42)), ExpectXML: @"<Plain><V>42</V></Plain>"),
    new(Value: Ꮡ(new Plain((int8)42)), ExpectXML: @"<Plain><V>42</V></Plain>"),
    new(Value: Ꮡ(new Plain((int16)42)), ExpectXML: @"<Plain><V>42</V></Plain>"),
    new(Value: Ꮡ(new Plain((int32)42)), ExpectXML: @"<Plain><V>42</V></Plain>"),
    new(Value: Ꮡ(new Plain((nuint)42)), ExpectXML: @"<Plain><V>42</V></Plain>"),
    new(Value: Ꮡ(new Plain((uint8)42)), ExpectXML: @"<Plain><V>42</V></Plain>"),
    new(Value: Ꮡ(new Plain((uint16)42)), ExpectXML: @"<Plain><V>42</V></Plain>"),
    new(Value: Ꮡ(new Plain((uint32)42)), ExpectXML: @"<Plain><V>42</V></Plain>"),
    new(Value: Ꮡ(new Plain((float32)1.25F)), ExpectXML: @"<Plain><V>1.25</V></Plain>"),
    new(Value: Ꮡ(new Plain((float64)1.25D)), ExpectXML: @"<Plain><V>1.25</V></Plain>"),
    new(Value: Ꮡ(new Plain((uintptr)0xFFDD)), ExpectXML: @"<Plain><V>65501</V></Plain>"),
    new(Value: Ꮡ(new Plain((@string)"gopher")), ExpectXML: @"<Plain><V>gopher</V></Plain>"),
    new(Value: Ꮡ(new Plain(slice<byte>("gopher"u8))), ExpectXML: @"<Plain><V>gopher</V></Plain>"),
    new(Value: Ꮡ(new Plain((@string)"</>")), ExpectXML: @"<Plain><V>&lt;/&gt;</V></Plain>"),
    new(Value: Ꮡ(new Plain(slice<byte>("</>"u8))), ExpectXML: @"<Plain><V>&lt;/&gt;</V></Plain>"),
    new(Value: Ꮡ(new Plain(new byte[]{(byte)'<', (byte)'/', (byte)'>'}.array())), ExpectXML: @"<Plain><V>&lt;/&gt;</V></Plain>"),
    new(Value: Ꮡ(new Plain((NamedType)(@string)"potato")), ExpectXML: @"<Plain><V>potato</V></Plain>"),
    new(Value: Ꮡ(new Plain(slice<nint>([1, 2, 3]))), ExpectXML: @"<Plain><V>1</V><V>2</V><V>3</V></Plain>"),
    new(Value: Ꮡ(new Plain(new nint[]{1, 2, 3}.array())), ExpectXML: @"<Plain><V>1</V><V>2</V><V>3</V></Plain>"),
    new(Value: ifaceptr(true), MarshalOnly: true, ExpectXML: @"<bool>true</bool>"),

    // Test time.
    new(
        Value: Ꮡ(new Plain(time.Unix(1000000000, 123456789).UTC())),
        ExpectXML: @"<Plain><V>2001-09-09T01:46:40.123456789Z</V></Plain>"
    ),

    // A pointer to struct{} may be used to test for an element's presence.
    new(
        Value: Ꮡ(new PresenceTest(@new<EmptyStruct>())),
        ExpectXML: @"<PresenceTest><Exists></Exists></PresenceTest>"
    ),
    new(
        Value: Ꮡ(new PresenceTest()),
        ExpectXML: @"<PresenceTest></PresenceTest>"
    ),

    // A []byte field is only nil if the element was not found.
    new(
        Value: Ꮡ(new Data()),
        ExpectXML: @"<Data></Data>",
        UnmarshalOnly: true
    ),
    new(
        Value: Ꮡ(new Data(Bytes: slice<byte>([]), Custom: new MyBytes(new byte[]{}.slice()), Attr: slice<byte>([]))),
        ExpectXML: @"<Data Attr=""""><Bytes></Bytes><Custom></Custom></Data>",
        UnmarshalOnly: true
    ),

    // Check that []byte works, including named []byte types.
    new(
        Value: Ꮡ(new Data(Bytes: slice<byte>("ab"u8), Custom: ((MyBytes)slice<byte>((@string)"cd"u8)), Attr: slice<byte>([(byte)'v']))),
        ExpectXML: @"<Data Attr=""v""><Bytes>ab</Bytes><Custom>cd</Custom></Data>"
    ),

    // Test innerxml
    new(
        Value: Ꮡ(new SecretAgent(
            Handle: "007",
            Identity: "James Bond",
            Obfuscate: "<redacted/>"
        )),
        ExpectXML: @"<agent handle=""007""><Identity>James Bond</Identity><redacted/></agent>",
        MarshalOnly: true
    ),
    new(
        Value: Ꮡ(new SecretAgent(
            Handle: "007",
            Identity: "James Bond",
            Obfuscate: "<Identity>James Bond</Identity><redacted/>"
        )),
        ExpectXML: @"<agent handle=""007""><Identity>James Bond</Identity><redacted/></agent>",
        UnmarshalOnly: true
    ),

    // Test structs
    new(Value: Ꮡ(new Port(Type: "ssl", Number: "443")), ExpectXML: @"<port type=""ssl"">443</port>"),
    new(Value: Ꮡ(new Port(Number: "443")), ExpectXML: @"<port>443</port>"),
    new(Value: Ꮡ(new Port(Type: "<unix>")), ExpectXML: @"<port type=""&lt;unix&gt;""></port>"),
    new(Value: Ꮡ(new Port(Number: "443", Comment: "https")), ExpectXML: @"<port><!--https-->443</port>"),
    new(Value: Ꮡ(new Port(Number: "443", Comment: "add space-")), ExpectXML: @"<port><!--add space- -->443</port>", MarshalOnly: true),
    new(Value: Ꮡ(new Domain(Name: slice<byte>("google.com&friends"u8))), ExpectXML: @"<domain>google.com&amp;friends</domain>"),
    new(Value: Ꮡ(new Domain(Name: slice<byte>("google.com"u8), Comment: slice<byte>(" &friends "u8))), ExpectXML: @"<domain>google.com<!-- &friends --></domain>"),
    new(Value: Ꮡ(new Book(Title: "Pride & Prejudice")), ExpectXML: @"<book>Pride &amp; Prejudice</book>"),
    new(Value: Ꮡ(new Event(Year: -3114)), ExpectXML: @"<event>-3114</event>"),
    new(Value: Ꮡ(new Movie(Length: 13440)), ExpectXML: @"<movie>13440</movie>"),
    new(Value: Ꮡ(new Pi(Approximation: 3.14159265F)), ExpectXML: @"<pi>3.1415927</pi>"),
    new(Value: Ꮡ(new Universe(Visible: 9.3e13D)), ExpectXML: @"<universe>9.3e+13</universe>"),
    new(Value: Ꮡ(new Particle(HasMass: true)), ExpectXML: @"<particle>true</particle>"),
    new(Value: Ꮡ(new Departure(When: ParseTime("2013-01-09T00:15:00-09:00"))), ExpectXML: @"<departure>2013-01-09T00:15:00-09:00</departure>"),
    new(Value: atomValue.OrTypedNil(), ExpectXML: atomXML),
    new(Value: Ꮡ(new Generic<nint>(1)), ExpectXML: @"<Generic><X>1</X></Generic>"),
    new(
        Value: Ꮡ(new Ship(
            Name: "Heart of Gold",
            Pilot: "Computer",
            Age: 1,
            Drive: ImprobabilityDrive,
            Passenger: slice<ж<Passenger>>([
                Ꮡ(new Passenger(
                    Name: slice<@string>(["Zaphod", "Beeblebrox"]),
                    Weight: 7.25F)),
                Ꮡ(new Passenger(
                    Name: slice<@string>(["Trisha", "McMillen"]),
                    Weight: 5.5F)),
                Ꮡ(new Passenger(
                    Name: slice<@string>(["Ford", "Prefect"]),
                    Weight: 7F)),
                Ꮡ(new Passenger(
                    Name: slice<@string>(["Arthur", "Dent"]),
                    Weight: 6.75F))
            ])
        )),
        ExpectXML: @"<spaceship name=""Heart of Gold"" pilot=""Computer"">"u8 + @"<drive>"u8 + strconv.Itoa((nint)ImprobabilityDrive) + @"</drive>"u8 + @"<age>1</age>"u8 + @"<passenger>"u8 + @"<name>Zaphod</name>"u8 + @"<name>Beeblebrox</name>"u8 + @"<weight>7.25</weight>"u8 + @"</passenger>"u8 + @"<passenger>"u8 + @"<name>Trisha</name>"u8 + @"<name>McMillen</name>"u8 + @"<weight>5.5</weight>"u8 + @"</passenger>"u8 + @"<passenger>"u8 + @"<name>Ford</name>"u8 + @"<name>Prefect</name>"u8 + @"<weight>7</weight>"u8 + @"</passenger>"u8 + @"<passenger>"u8 + @"<name>Arthur</name>"u8 + @"<name>Dent</name>"u8 + @"<weight>6.75</weight>"u8 + @"</passenger>"u8 + @"</spaceship>"u8
    ),

    // Test a>b
    new(
        Value: Ꮡ(new NestedItems(Items: default, Item1: default)),
        ExpectXML: @"<result>"u8 + @"<Items>"u8 + @"</Items>"u8 + @"</result>"u8
    ),
    new(
        Value: Ꮡ(new NestedItems(Items: slice<@string>([]), Item1: slice<@string>([]))),
        ExpectXML: @"<result>"u8 + @"<Items>"u8 + @"</Items>"u8 + @"</result>"u8,
        MarshalOnly: true
    ),
    new(
        Value: Ꮡ(new NestedItems(Items: default, Item1: slice<@string>(["A"]))),
        ExpectXML: @"<result>"u8 + @"<Items>"u8 + @"<item1>A</item1>"u8 + @"</Items>"u8 + @"</result>"u8
    ),
    new(
        Value: Ꮡ(new NestedItems(Items: slice<@string>(["A", "B"]), Item1: default)),
        ExpectXML: @"<result>"u8 + @"<Items>"u8 + @"<item>A</item>"u8 + @"<item>B</item>"u8 + @"</Items>"u8 + @"</result>"u8
    ),
    new(
        Value: Ꮡ(new NestedItems(Items: slice<@string>(["A", "B"]), Item1: slice<@string>(["C"]))),
        ExpectXML: @"<result>"u8 + @"<Items>"u8 + @"<item>A</item>"u8 + @"<item>B</item>"u8 + @"<item1>C</item1>"u8 + @"</Items>"u8 + @"</result>"u8
    ),
    new(
        Value: Ꮡ(new NestedOrder(Field1: "C", Field2: "B", Field3: "A")),
        ExpectXML: @"<result>"u8 + @"<parent>"u8 + @"<c>C</c>"u8 + @"<b>B</b>"u8 + @"<a>A</a>"u8 + @"</parent>"u8 + @"</result>"u8
    ),
    new(
        Value: Ꮡ(new NilTest(A: (@string)"A", B: default, C: (@string)"C")),
        ExpectXML: @"<NilTest>"u8 + @"<parent1>"u8 + @"<parent2><a>A</a></parent2>"u8 + @"<parent2><c>C</c></parent2>"u8 + @"</parent1>"u8 + @"</NilTest>"u8,
        MarshalOnly: true // Uses interface{}

    ),
    new(
        Value: Ꮡ(new MixedNested(A: "A", B: "B", C: "C", D: "D")),
        ExpectXML: @"<result>"u8 + @"<parent1><a>A</a></parent1>"u8 + @"<b>B</b>"u8 + @"<parent1>"u8 + @"<parent2><c>C</c></parent2>"u8 + @"<d>D</d>"u8 + @"</parent1>"u8 + @"</result>"u8
    ),
    new(
        Value: Ꮡ(new Service(Port: Ꮡ(new Port(Number: "80")))),
        ExpectXML: @"<service><host><port>80</port></host></service>"
    ),
    new(
        Value: Ꮡ(new Service()),
        ExpectXML: @"<service></service>"
    ),
    new(
        Value: Ꮡ(new Service(Port: Ꮡ(new Port(Number: "80")), Extra1: (@string)"A", Extra2: (@string)"B")),
        ExpectXML: @"<service>"u8 + @"<host><port>80</port></host>"u8 + @"<Extra1>A</Extra1>"u8 + @"<host><extra2>B</extra2></host>"u8 + @"</service>"u8,
        MarshalOnly: true
    ),
    new(
        Value: Ꮡ(new Service(Port: Ꮡ(new Port(Number: "80")), Extra2: (@string)"example")),
        ExpectXML: @"<service>"u8 + @"<host><port>80</port></host>"u8 + @"<host><extra2>example</extra2></host>"u8 + @"</service>"u8,
        MarshalOnly: true
    ),
    new(
        Value: Ꮡ(new Δtype(
            A: "a",
            B: "b",
            C: "c",
            C1: "c1",
            D1: "d1"
        )),
        ExpectXML: @"<top xmlns=""space"">"u8 + @"<x><a>a</a><b>b</b><c xmlns=""space"">c</c>"u8 + @"<c xmlns=""space1"">c1</c>"u8 + @"<d xmlns=""space1"">d1</d>"u8 + @"</x>"u8 + @"</top>"u8
    ),
    new(
        Value: Ꮡ(new Δtypeᴛ1(
            XMLName: new Name(
                Space: "space0",
                Local: "top"
            ),
            A: "a",
            B: "b",
            C: "c",
            C1: "c1",
            D1: "d1"
        )),
        ExpectXML: @"<top xmlns=""space0"">"u8 + @"<x><a>a</a><b>b</b>"u8 + @"<c xmlns=""space"">c</c>"u8 + @"<c xmlns=""space1"">c1</c>"u8 + @"<d xmlns=""space1"">d1</d>"u8 + @"</x>"u8 + @"</top>"u8
    ),
    new(
        Value: Ꮡ(new Δtypeᴛ2(
            B: "b",
            B1: "b1"
        )),
        ExpectXML: @"<top>"u8 + @"<x><b xmlns=""space"">b</b>"u8 + @"<b xmlns=""space1"">b1</b></x>"u8 + @"</top>"u8
    ),

    // Test struct embedding
    new(
        Value: Ꮡ(new EmbedA(
            EmbedC: new EmbedC(
                FieldA1: "", // Shadowed by A.A
                FieldA2: "", // Shadowed by A.A
                FieldB: "A.C.B",
                FieldC: "A.C.C"
            ),
            EmbedB: new EmbedB(
                FieldB: "A.B.B",
                EmbedC: Ꮡ(new EmbedC(
                    FieldA1: "A.B.C.A1",
                    FieldA2: "A.B.C.A2",
                    FieldB: "", // Shadowed by A.B.B
                    FieldC: "A.B.C.C"
                ))
            ),
            FieldA: "A.A",
            embedD: new embedD(
                FieldE: "A.D.E"
            )
        )),
        ExpectXML: @"<EmbedA>"u8 + @"<FieldB>A.C.B</FieldB>"u8 + @"<FieldC>A.C.C</FieldC>"u8 + @"<EmbedB>"u8 + @"<FieldB>A.B.B</FieldB>"u8 + @"<FieldA>"u8 + @"<A1>A.B.C.A1</A1>"u8 + @"<A2>A.B.C.A2</A2>"u8 + @"</FieldA>"u8 + @"<FieldC>A.B.C.C</FieldC>"u8 + @"</EmbedB>"u8 + @"<FieldA>A.A</FieldA>"u8 + @"<FieldE>A.D.E</FieldE>"u8 + @"</EmbedA>"u8
    ),

    // Anonymous struct pointer field which is nil
    new(
        Value: Ꮡ(new EmbedB()),
        ExpectXML: @"<EmbedB><FieldB></FieldB></EmbedB>"
    ),

    // Other kinds of nil anonymous fields
    new(
        Value: Ꮡ(new PointerAnonFields()),
        ExpectXML: @"<PointerAnonFields></PointerAnonFields>"
    ),

    // Test that name casing matters
    new(
        Value: Ꮡ(new NameCasing(Xy: "mixed", XY: "upper", XyA: "mixedA", XYA: "upperA")),
        ExpectXML: @"<casing Xy=""mixedA"" XY=""upperA""><Xy>mixed</Xy><XY>upper</XY></casing>"
    ),

    // Test the order in which the XML element name is chosen
    new(
        Value: Ꮡ(new NamePrecedence(
            FromTag: new XMLNameWithoutTag(Value: "A"),
            FromNameVal: new XMLNameWithoutTag(XMLName: new Name(Local: "InXMLName"), Value: "B"),
            FromNameTag: new XMLNameWithTag(Value: "C"),
            InFieldName: "D"
        )),
        ExpectXML: @"<Parent>"u8 + @"<InTag>A</InTag>"u8 + @"<InXMLName>B</InXMLName>"u8 + @"<InXMLNameTag>C</InXMLNameTag>"u8 + @"<InFieldName>D</InFieldName>"u8 + @"</Parent>"u8,
        MarshalOnly: true
    ),
    new(
        Value: Ꮡ(new NamePrecedence(
            XMLName: new Name(Local: "Parent"),
            FromTag: new XMLNameWithoutTag(XMLName: new Name(Local: "InTag"), Value: "A"),
            FromNameVal: new XMLNameWithoutTag(XMLName: new Name(Local: "FromNameVal"), Value: "B"),
            FromNameTag: new XMLNameWithTag(XMLName: new Name(Local: "InXMLNameTag"), Value: "C"),
            InFieldName: "D"
        )),
        ExpectXML: @"<Parent>"u8 + @"<InTag>A</InTag>"u8 + @"<FromNameVal>B</FromNameVal>"u8 + @"<InXMLNameTag>C</InXMLNameTag>"u8 + @"<InFieldName>D</InFieldName>"u8 + @"</Parent>"u8,
        UnmarshalOnly: true
    ),

    // xml.Name works in a plain field as well.
    new(
        Value: Ꮡ(new NameInField(new Name(Space: "ns", Local: "foo"))),
        ExpectXML: @"<NameInField><foo xmlns=""ns""></foo></NameInField>"
    ),
    new(
        Value: Ꮡ(new NameInField(new Name(Space: "ns", Local: "foo"))),
        ExpectXML: @"<NameInField><foo xmlns=""ns""><ignore></ignore></foo></NameInField>",
        UnmarshalOnly: true
    ),

    // Marshaling zero xml.Name uses the tag or field name.
    new(
        Value: Ꮡ(new NameInField()),
        ExpectXML: @"<NameInField><foo xmlns=""ns""></foo></NameInField>",
        MarshalOnly: true
    ),

    // Test attributes
    new(
        Value: Ꮡ(new AttrTest(
            Int: 8,
            Named: 9,
            Float: 23.5D,
            Uint8: 255,
            Bool: true,
            Str: "str",
            Bytes: slice<byte>("byt"u8)
        )),
        ExpectXML: @"<AttrTest Int=""8"" int=""9"" Float=""23.5"" Uint8=""255"""u8 + @" Bool=""true"" Str=""str"" Bytes=""byt""></AttrTest>"u8
    ),
    new(
        Value: Ꮡ(new AttrTest(Bytes: slice<byte>([]))),
        ExpectXML: @"<AttrTest Int=""0"" int=""0"" Float=""0"" Uint8=""0"""u8 + @" Bool=""false"" Str="""" Bytes=""""></AttrTest>"u8
    ),
    new(
        Value: Ꮡ(new AttrsTest(
            Attrs: slice<global::go.encoding.xml_package.Attr>([
                new(Name: new Name(Local: "Answer"), Value: "42"),
                new(Name: new Name(Local: "Int"), Value: "8"),
                new(Name: new Name(Local: "int"), Value: "9"),
                new(Name: new Name(Local: "Float"), Value: "23.5"),
                new(Name: new Name(Local: "Uint8"), Value: "255"),
                new(Name: new Name(Local: "Bool"), Value: "true"),
                new(Name: new Name(Local: "Str"), Value: "str"),
                new(Name: new Name(Local: "Bytes"), Value: "byt")
            ])
        )),
        ExpectXML: @"<AttrsTest Answer=""42"" Int=""8"" int=""9"" Float=""23.5"" Uint8=""255"" Bool=""true"" Str=""str"" Bytes=""byt"" Int=""0"" int=""0"" Float=""0"" Uint8=""0"" Bool=""false"" Str="""" Bytes=""""></AttrsTest>",
        MarshalOnly: true
    ),
    new(
        Value: Ꮡ(new AttrsTest(
            Attrs: slice<global::go.encoding.xml_package.Attr>([
                new(Name: new Name(Local: "Answer"), Value: "42")
            ]),
            Int: 8,
            Named: 9,
            Float: 23.5D,
            Uint8: 255,
            Bool: true,
            Str: "str",
            Bytes: slice<byte>("byt"u8)
        )),
        ExpectXML: @"<AttrsTest Answer=""42"" Int=""8"" int=""9"" Float=""23.5"" Uint8=""255"" Bool=""true"" Str=""str"" Bytes=""byt""></AttrsTest>"
    ),
    new(
        Value: Ꮡ(new AttrsTest(
            Attrs: slice<global::go.encoding.xml_package.Attr>([
                new(Name: new Name(Local: "Int"), Value: "0"),
                new(Name: new Name(Local: "int"), Value: "0"),
                new(Name: new Name(Local: "Float"), Value: "0"),
                new(Name: new Name(Local: "Uint8"), Value: "0"),
                new(Name: new Name(Local: "Bool"), Value: "false"),
                new(Name: new Name(Local: "Str")),
                new(Name: new Name(Local: "Bytes"))
            ]),
            Bytes: slice<byte>([])
        )),
        ExpectXML: @"<AttrsTest Int=""0"" int=""0"" Float=""0"" Uint8=""0"" Bool=""false"" Str="""" Bytes="""" Int=""0"" int=""0"" Float=""0"" Uint8=""0"" Bool=""false"" Str="""" Bytes=""""></AttrsTest>",
        MarshalOnly: true
    ),
    new(
        Value: Ꮡ(new OmitAttrTest(
            Int: 8,
            Named: 9,
            Float: 23.5D,
            Uint8: 255,
            Bool: true,
            Str: "str",
            Bytes: slice<byte>("byt"u8),
            PStr: Ꮡempty
        )),
        ExpectXML: @"<OmitAttrTest Int=""8"" int=""9"" Float=""23.5"" Uint8=""255"""u8 + @" Bool=""true"" Str=""str"" Bytes=""byt"" PStr=""""></OmitAttrTest>"u8
    ),
    new(
        Value: Ꮡ(new OmitAttrTest()),
        ExpectXML: @"<OmitAttrTest></OmitAttrTest>"
    ),

    // pointer fields
    new(
        Value: Ꮡ(new PointerFieldsTest(Name: ᏑnameAttr, Age: ᏑageAttr, Contents: ᏑcontentsAttr)),
        ExpectXML: @"<dummy name=""Sarah"" age=""12"">lorem ipsum</dummy>",
        MarshalOnly: true
    ),

    // empty chardata pointer field
    new(
        Value: Ꮡ(new ChardataEmptyTest()),
        ExpectXML: @"<test></test>",
        MarshalOnly: true
    ),

    // omitempty on fields
    new(
        Value: Ꮡ(new OmitFieldTest(
            Int: 8,
            Named: 9,
            Float: 23.5D,
            Uint8: 255,
            Bool: true,
            Str: "str",
            Bytes: slice<byte>("byt"u8),
            PStr: Ꮡempty,
            Ptr: Ꮡ(new PresenceTest())
        )),
        ExpectXML: @"<OmitFieldTest>"u8 + @"<Int>8</Int>"u8 + @"<int>9</int>"u8 + @"<Float>23.5</Float>"u8 + @"<Uint8>255</Uint8>"u8 + @"<Bool>true</Bool>"u8 + @"<Str>str</Str>"u8 + @"<Bytes>byt</Bytes>"u8 + @"<PStr></PStr>"u8 + @"<Ptr></Ptr>"u8 + @"</OmitFieldTest>"u8
    ),
    new(
        Value: Ꮡ(new OmitFieldTest()),
        ExpectXML: @"<OmitFieldTest></OmitFieldTest>"
    ),

    // Test ",any"
    new(
        ExpectXML: @"<a><nested><value>known</value></nested><other><sub>unknown</sub></other></a>",
        Value: Ꮡ(new AnyTest(
            Nested: "known",
            AnyField: new AnyHolder(
                XMLName: new Name(Local: "other"),
                XML: "<sub>unknown</sub>"
            )
        ))
    ),
    new(
        Value: Ꮡ(new AnyTest(Nested: "known",
            AnyField: new AnyHolder(
                XML: "<unknown/>",
                XMLName: new Name(Local: "AnyField")
            )
        )),
        ExpectXML: @"<a><nested><value>known</value></nested><AnyField><unknown/></AnyField></a>"
    ),
    new(
        ExpectXML: @"<a><nested><value>b</value></nested></a>",
        Value: Ꮡ(new AnyOmitTest(
            Nested: "b"
        ))
    ),
    new(
        ExpectXML: @"<a><nested><value>b</value></nested><c><d>e</d></c><g xmlns=""f""><h>i</h></g></a>",
        Value: Ꮡ(new AnySliceTest(
            Nested: "b",
            AnyField: slice<AnyHolder>([
                new(
                    XMLName: new Name(Local: "c"),
                    XML: "<d>e</d>"
                ),
                new(
                    XMLName: new Name(Space: "f", Local: "g"),
                    XML: "<h>i</h>"
                )
            ])
        ))
    ),
    new(
        ExpectXML: @"<a><nested><value>b</value></nested></a>",
        Value: Ꮡ(new AnySliceTest(
            Nested: "b"
        ))
    ),

    // Test recursive types.
    new(
        Value: Ꮡ(new RecurseA(
            A: "a1",
            B: Ꮡ(new RecurseB(
                A: Ꮡ(new RecurseA("a2", nil)),
                B: "b1"
            ))
        )),
        ExpectXML: @"<RecurseA><A>a1</A><B><A><A>a2</A></A><B>b1</B></B></RecurseA>"
    ),

    // Test ignoring fields via "-" tag
    new(
        ExpectXML: @"<IgnoreTest></IgnoreTest>",
        Value: Ꮡ(new IgnoreTest())
    ),
    new(
        ExpectXML: @"<IgnoreTest></IgnoreTest>",
        Value: Ꮡ(new IgnoreTest(PublicSecret: "can't tell")),
        MarshalOnly: true
    ),
    new(
        ExpectXML: @"<IgnoreTest><PublicSecret>ignore me</PublicSecret></IgnoreTest>",
        Value: Ꮡ(new IgnoreTest()),
        UnmarshalOnly: true
    ),

    // Test escaping.
    new(
        ExpectXML: @"<a><nested><value>dquote: &#34;; squote: &#39;; ampersand: &amp;; less: &lt;; greater: &gt;;</value></nested><empty></empty></a>",
        Value: Ꮡ(new AnyTest(
            Nested: @"dquote: ""; squote: '; ampersand: &; less: <; greater: >;",
            AnyField: new AnyHolder(XMLName: new Name(Local: "empty"))
        ))
    ),
    new(
        ExpectXML: @"<a><nested><value>newline: &#xA;; cr: &#xD;; tab: &#x9;;</value></nested><AnyField></AnyField></a>",
        Value: Ꮡ(new AnyTest(
            Nested: "newline: \n; cr: \r; tab: \t;",
            AnyField: new AnyHolder(XMLName: new Name(Local: "AnyField"))
        ))
    ),
    new(
        ExpectXML: "<a><nested><value>1\r2\r\n3\n\r4\n5</value></nested></a>",
        Value: Ꮡ(new AnyTest(
            Nested: "1\n2\n3\n\n4\n5"
        )),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<EmbedInt><MyInt>42</MyInt></EmbedInt>",
        Value: Ꮡ(new EmbedInt(
            MyInt: 42
        ))
    ),
    // Test outputting CDATA-wrapped text.
    new(
        ExpectXML: @"<CDataTest></CDataTest>",
        Value: Ꮡ(new CDataTest())
    ),
    new(
        ExpectXML: @"<CDataTest><![CDATA[http://example.com/tests/1?foo=1&bar=baz]]></CDataTest>",
        Value: Ꮡ(new CDataTest(
            Chardata: "http://example.com/tests/1?foo=1&bar=baz"
        ))
    ),
    new(
        ExpectXML: @"<CDataTest><![CDATA[Literal <![CDATA[Nested]]]]><![CDATA[>!]]></CDataTest>",
        Value: Ꮡ(new CDataTest(
            Chardata: "Literal <![CDATA[Nested]]>!"
        ))
    ),
    new(
        ExpectXML: @"<CDataTest><![CDATA[<![CDATA[Nested]]]]><![CDATA[> Literal!]]></CDataTest>",
        Value: Ꮡ(new CDataTest(
            Chardata: "<![CDATA[Nested]]> Literal!"
        ))
    ),
    new(
        ExpectXML: @"<CDataTest><![CDATA[<![CDATA[Nested]]]]><![CDATA[> Literal! <![CDATA[Nested]]]]><![CDATA[> Literal!]]></CDataTest>",
        Value: Ꮡ(new CDataTest(
            Chardata: "<![CDATA[Nested]]> Literal! <![CDATA[Nested]]> Literal!"
        ))
    ),
    new(
        ExpectXML: @"<CDataTest><![CDATA[<![CDATA[<![CDATA[Nested]]]]><![CDATA[>]]]]><![CDATA[>]]></CDataTest>",
        Value: Ꮡ(new CDataTest(
            Chardata: "<![CDATA[<![CDATA[Nested]]>]]>"
        ))
    ),

    // Test omitempty with parent chain; see golang.org/issue/4168.
    new(
        ExpectXML: @"<Strings><A></A></Strings>",
        Value: Ꮡ(new Strings())
    ),
    // Custom marshalers.
    new(
        ExpectXML: @"<MyMarshalerTest>hello world</MyMarshalerTest>",
        Value: Ꮡ(new MyMarshalerTest())
    ),
    new(
        ExpectXML: @"<MarshalerStruct Foo=""hello world""></MarshalerStruct>",
        Value: Ꮡ(new MarshalerStruct())
    ),
    new(
        ExpectXML: @"<outer xmlns=""testns"" int=""10""></outer>",
        Value: Ꮡ(new OuterStruct(IntAttr: 10))
    ),
    new(
        ExpectXML: @"<test xmlns=""outerns"" int=""10""></test>",
        Value: Ꮡ(new OuterNamedStruct(XMLName: new Name(Space: "outerns", Local: "test"), IntAttr: 10))
    ),
    new(
        ExpectXML: @"<test xmlns=""outerns"" int=""10""></test>",
        Value: Ꮡ(new OuterNamedOrderedStruct(XMLName: new Name(Space: "outerns", Local: "test"), IntAttr: 10))
    ),
    new(
        ExpectXML: @"<outer xmlns=""testns"" int=""10""></outer>",
        Value: Ꮡ(new OuterOuterStruct(new OuterStruct(IntAttr: 10)))
    ),
    new(
        ExpectXML: @"<NestedAndChardata><A><B></B><B></B></A>test</NestedAndChardata>",
        Value: Ꮡ(new NestedAndChardata(AB: new slice<@string>(2), Chardata: "test"))
    ),
    new(
        ExpectXML: @"<NestedAndComment><A><B></B><B></B></A><!--test--></NestedAndComment>",
        Value: Ꮡ(new NestedAndComment(AB: new slice<@string>(2), Comment: "test"))
    ),
    new(
        ExpectXML: @"<NestedAndCData><A><B></B><B></B></A><![CDATA[test]]></NestedAndCData>",
        Value: Ꮡ(new NestedAndCData(AB: new slice<@string>(2), CDATA: "test"))
    ),
    // Test pointer indirection in various kinds of fields.
    // https://golang.org/issue/19063
    new(
        ExpectXML: @"<IndirComment><T1></T1><!--hi--><T2></T2></IndirComment>",
        Value: Ꮡ(new IndirComment(Comment: stringptr("hi"))),
        MarshalOnly: true
    ),
    new(
        ExpectXML: @"<IndirComment><T1></T1><T2></T2></IndirComment>",
        Value: Ꮡ(new IndirComment(Comment: stringptr(""))),
        MarshalOnly: true
    ),
    new(
        ExpectXML: @"<IndirComment><T1></T1><T2></T2></IndirComment>",
        Value: Ꮡ(new IndirComment(Comment: nil)),
        MarshalError: "xml: bad type for comment field of xml.IndirComment"
    ),
    new(
        ExpectXML: @"<IndirComment><T1></T1><!--hi--><T2></T2></IndirComment>",
        Value: Ꮡ(new IndirComment(Comment: nil)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceComment><T1></T1><!--hi--><T2></T2></IfaceComment>",
        Value: Ꮡ(new IfaceComment(Comment: (@string)"hi")),
        MarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceComment><T1></T1><!--hi--><T2></T2></IfaceComment>",
        Value: Ꮡ(new IfaceComment(Comment: default)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceComment><T1></T1><T2></T2></IfaceComment>",
        Value: Ꮡ(new IfaceComment(Comment: default)),
        MarshalError: "xml: bad type for comment field of xml.IfaceComment"
    ),
    new(
        ExpectXML: @"<IfaceComment><T1></T1><T2></T2></IfaceComment>",
        Value: Ꮡ(new IfaceComment(Comment: default)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<DirectComment><T1></T1><!--hi--><T2></T2></DirectComment>",
        Value: Ꮡ(new DirectComment(Comment: ((@string)"hi"u8)))
    ),
    new(
        ExpectXML: @"<DirectComment><T1></T1><T2></T2></DirectComment>",
        Value: Ꮡ(new DirectComment(Comment: ((@string)""u8)))
    ),
    new(
        ExpectXML: @"<IndirChardata><T1></T1>hi<T2></T2></IndirChardata>",
        Value: Ꮡ(new IndirChardata(Chardata: stringptr("hi")))
    ),
    new(
        ExpectXML: @"<IndirChardata><T1></T1><![CDATA[hi]]><T2></T2></IndirChardata>",
        Value: Ꮡ(new IndirChardata(Chardata: stringptr("hi"))),
        UnmarshalOnly: true // marshals without CDATA

    ),
    new(
        ExpectXML: @"<IndirChardata><T1></T1><T2></T2></IndirChardata>",
        Value: Ꮡ(new IndirChardata(Chardata: stringptr("")))
    ),
    new(
        ExpectXML: @"<IndirChardata><T1></T1><T2></T2></IndirChardata>",
        Value: Ꮡ(new IndirChardata(Chardata: nil)),
        MarshalOnly: true // unmarshal leaves Chardata=stringptr("")

    ),
    new(
        ExpectXML: @"<IfaceChardata><T1></T1>hi<T2></T2></IfaceChardata>",
        Value: Ꮡ(new IfaceChardata(Chardata: ((@string)"hi"u8))),
        UnmarshalError: "cannot unmarshal into interface {}"
    ),
    new(
        ExpectXML: @"<IfaceChardata><T1></T1><![CDATA[hi]]><T2></T2></IfaceChardata>",
        Value: Ꮡ(new IfaceChardata(Chardata: ((@string)"hi"u8))),
        UnmarshalOnly: true, // marshals without CDATA
        UnmarshalError: "cannot unmarshal into interface {}"
    ),
    new(
        ExpectXML: @"<IfaceChardata><T1></T1><T2></T2></IfaceChardata>",
        Value: Ꮡ(new IfaceChardata(Chardata: ((@string)""u8))),
        UnmarshalError: "cannot unmarshal into interface {}"
    ),
    new(
        ExpectXML: @"<IfaceChardata><T1></T1><T2></T2></IfaceChardata>",
        Value: Ꮡ(new IfaceChardata(Chardata: default)),
        UnmarshalError: "cannot unmarshal into interface {}"
    ),
    new(
        ExpectXML: @"<DirectChardata><T1></T1>hi<T2></T2></DirectChardata>",
        Value: Ꮡ(new DirectChardata(Chardata: ((@string)"hi"u8)))
    ),
    new(
        ExpectXML: @"<DirectChardata><T1></T1><![CDATA[hi]]><T2></T2></DirectChardata>",
        Value: Ꮡ(new DirectChardata(Chardata: ((@string)"hi"u8))),
        UnmarshalOnly: true // marshals without CDATA

    ),
    new(
        ExpectXML: @"<DirectChardata><T1></T1><T2></T2></DirectChardata>",
        Value: Ꮡ(new DirectChardata(Chardata: ((@string)""u8)))
    ),
    new(
        ExpectXML: @"<IndirCDATA><T1></T1><![CDATA[hi]]><T2></T2></IndirCDATA>",
        Value: Ꮡ(new IndirCDATA(CDATA: stringptr("hi")))
    ),
    new(
        ExpectXML: @"<IndirCDATA><T1></T1>hi<T2></T2></IndirCDATA>",
        Value: Ꮡ(new IndirCDATA(CDATA: stringptr("hi"))),
        UnmarshalOnly: true // marshals with CDATA

    ),
    new(
        ExpectXML: @"<IndirCDATA><T1></T1><T2></T2></IndirCDATA>",
        Value: Ꮡ(new IndirCDATA(CDATA: stringptr("")))
    ),
    new(
        ExpectXML: @"<IndirCDATA><T1></T1><T2></T2></IndirCDATA>",
        Value: Ꮡ(new IndirCDATA(CDATA: nil)),
        MarshalOnly: true // unmarshal leaves CDATA=stringptr("")

    ),
    new(
        ExpectXML: @"<IfaceCDATA><T1></T1><![CDATA[hi]]><T2></T2></IfaceCDATA>",
        Value: Ꮡ(new IfaceCDATA(CDATA: ((@string)"hi"u8))),
        UnmarshalError: "cannot unmarshal into interface {}"
    ),
    new(
        ExpectXML: @"<IfaceCDATA><T1></T1>hi<T2></T2></IfaceCDATA>",
        Value: Ꮡ(new IfaceCDATA(CDATA: ((@string)"hi"u8))),
        UnmarshalOnly: true, // marshals with CDATA
        UnmarshalError: "cannot unmarshal into interface {}"
    ),
    new(
        ExpectXML: @"<IfaceCDATA><T1></T1><T2></T2></IfaceCDATA>",
        Value: Ꮡ(new IfaceCDATA(CDATA: ((@string)""u8))),
        UnmarshalError: "cannot unmarshal into interface {}"
    ),
    new(
        ExpectXML: @"<IfaceCDATA><T1></T1><T2></T2></IfaceCDATA>",
        Value: Ꮡ(new IfaceCDATA(CDATA: default)),
        UnmarshalError: "cannot unmarshal into interface {}"
    ),
    new(
        ExpectXML: @"<DirectCDATA><T1></T1><![CDATA[hi]]><T2></T2></DirectCDATA>",
        Value: Ꮡ(new DirectCDATA(CDATA: ((@string)"hi"u8)))
    ),
    new(
        ExpectXML: @"<DirectCDATA><T1></T1>hi<T2></T2></DirectCDATA>",
        Value: Ꮡ(new DirectCDATA(CDATA: ((@string)"hi"u8))),
        UnmarshalOnly: true // marshals with CDATA

    ),
    new(
        ExpectXML: @"<DirectCDATA><T1></T1><T2></T2></DirectCDATA>",
        Value: Ꮡ(new DirectCDATA(CDATA: ((@string)""u8)))
    ),
    new(
        ExpectXML: @"<IndirInnerXML><T1></T1><hi/><T2></T2></IndirInnerXML>",
        Value: Ꮡ(new IndirInnerXML(InnerXML: stringptr("<hi/>"))),
        MarshalOnly: true
    ),
    new(
        ExpectXML: @"<IndirInnerXML><T1></T1><T2></T2></IndirInnerXML>",
        Value: Ꮡ(new IndirInnerXML(InnerXML: stringptr(""))),
        MarshalOnly: true
    ),
    new(
        ExpectXML: @"<IndirInnerXML><T1></T1><T2></T2></IndirInnerXML>",
        Value: Ꮡ(new IndirInnerXML(InnerXML: nil))
    ),
    new(
        ExpectXML: @"<IndirInnerXML><T1></T1><hi/><T2></T2></IndirInnerXML>",
        Value: Ꮡ(new IndirInnerXML(InnerXML: nil)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceInnerXML><T1></T1><hi/><T2></T2></IfaceInnerXML>",
        Value: Ꮡ(new IfaceInnerXML(InnerXML: (@string)"<hi/>")),
        MarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceInnerXML><T1></T1><hi/><T2></T2></IfaceInnerXML>",
        Value: Ꮡ(new IfaceInnerXML(InnerXML: default)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceInnerXML><T1></T1><T2></T2></IfaceInnerXML>",
        Value: Ꮡ(new IfaceInnerXML(InnerXML: default))
    ),
    new(
        ExpectXML: @"<IfaceInnerXML><T1></T1><T2></T2></IfaceInnerXML>",
        Value: Ꮡ(new IfaceInnerXML(InnerXML: default)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<DirectInnerXML><T1></T1><hi/><T2></T2></DirectInnerXML>",
        Value: Ꮡ(new DirectInnerXML(InnerXML: ((@string)"<hi/>"u8))),
        MarshalOnly: true
    ),
    new(
        ExpectXML: @"<DirectInnerXML><T1></T1><hi/><T2></T2></DirectInnerXML>",
        Value: Ꮡ(new DirectInnerXML(InnerXML: ((@string)"<T1></T1><hi/><T2></T2>"u8))),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<DirectInnerXML><T1></T1><T2></T2></DirectInnerXML>",
        Value: Ꮡ(new DirectInnerXML(InnerXML: ((@string)""u8))),
        MarshalOnly: true
    ),
    new(
        ExpectXML: @"<DirectInnerXML><T1></T1><T2></T2></DirectInnerXML>",
        Value: Ꮡ(new DirectInnerXML(InnerXML: ((@string)"<T1></T1><T2></T2>"u8))),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IndirElement><T1></T1><Element>hi</Element><T2></T2></IndirElement>",
        Value: Ꮡ(new IndirElement(Element: stringptr("hi")))
    ),
    new(
        ExpectXML: @"<IndirElement><T1></T1><Element></Element><T2></T2></IndirElement>",
        Value: Ꮡ(new IndirElement(Element: stringptr("")))
    ),
    new(
        ExpectXML: @"<IndirElement><T1></T1><T2></T2></IndirElement>",
        Value: Ꮡ(new IndirElement(Element: nil))
    ),
    new(
        ExpectXML: @"<IfaceElement><T1></T1><Element>hi</Element><T2></T2></IfaceElement>",
        Value: Ꮡ(new IfaceElement(Element: (@string)"hi")),
        MarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceElement><T1></T1><Element>hi</Element><T2></T2></IfaceElement>",
        Value: Ꮡ(new IfaceElement(Element: default)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceElement><T1></T1><T2></T2></IfaceElement>",
        Value: Ꮡ(new IfaceElement(Element: default))
    ),
    new(
        ExpectXML: @"<IfaceElement><T1></T1><T2></T2></IfaceElement>",
        Value: Ꮡ(new IfaceElement(Element: default)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<DirectElement><T1></T1><Element>hi</Element><T2></T2></DirectElement>",
        Value: Ꮡ(new DirectElement(Element: ((@string)"hi"u8)))
    ),
    new(
        ExpectXML: @"<DirectElement><T1></T1><Element></Element><T2></T2></DirectElement>",
        Value: Ꮡ(new DirectElement(Element: ((@string)""u8)))
    ),
    new(
        ExpectXML: @"<IndirOmitEmpty><T1></T1><OmitEmpty>hi</OmitEmpty><T2></T2></IndirOmitEmpty>",
        Value: Ꮡ(new IndirOmitEmpty(OmitEmpty: stringptr("hi")))
    ),
    new(
        // Note: Changed in Go 1.8 to include <OmitEmpty> element (because x.OmitEmpty != nil).
        ExpectXML: @"<IndirOmitEmpty><T1></T1><OmitEmpty></OmitEmpty><T2></T2></IndirOmitEmpty>",
        Value: Ꮡ(new IndirOmitEmpty(OmitEmpty: stringptr(""))),
        MarshalOnly: true
    ),
    new(
        ExpectXML: @"<IndirOmitEmpty><T1></T1><OmitEmpty></OmitEmpty><T2></T2></IndirOmitEmpty>",
        Value: Ꮡ(new IndirOmitEmpty(OmitEmpty: stringptr(""))),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IndirOmitEmpty><T1></T1><T2></T2></IndirOmitEmpty>",
        Value: Ꮡ(new IndirOmitEmpty(OmitEmpty: nil))
    ),
    new(
        ExpectXML: @"<IfaceOmitEmpty><T1></T1><OmitEmpty>hi</OmitEmpty><T2></T2></IfaceOmitEmpty>",
        Value: Ꮡ(new IfaceOmitEmpty(OmitEmpty: (@string)"hi")),
        MarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceOmitEmpty><T1></T1><OmitEmpty>hi</OmitEmpty><T2></T2></IfaceOmitEmpty>",
        Value: Ꮡ(new IfaceOmitEmpty(OmitEmpty: default)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceOmitEmpty><T1></T1><T2></T2></IfaceOmitEmpty>",
        Value: Ꮡ(new IfaceOmitEmpty(OmitEmpty: default))
    ),
    new(
        ExpectXML: @"<IfaceOmitEmpty><T1></T1><T2></T2></IfaceOmitEmpty>",
        Value: Ꮡ(new IfaceOmitEmpty(OmitEmpty: default)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<DirectOmitEmpty><T1></T1><OmitEmpty>hi</OmitEmpty><T2></T2></DirectOmitEmpty>",
        Value: Ꮡ(new DirectOmitEmpty(OmitEmpty: ((@string)"hi"u8)))
    ),
    new(
        ExpectXML: @"<DirectOmitEmpty><T1></T1><T2></T2></DirectOmitEmpty>",
        Value: Ꮡ(new DirectOmitEmpty(OmitEmpty: ((@string)""u8)))
    ),
    new(
        ExpectXML: @"<IndirAny><T1></T1><Any>hi</Any><T2></T2></IndirAny>",
        Value: Ꮡ(new IndirAny(Any: stringptr("hi")))
    ),
    new(
        ExpectXML: @"<IndirAny><T1></T1><Any></Any><T2></T2></IndirAny>",
        Value: Ꮡ(new IndirAny(Any: stringptr("")))
    ),
    new(
        ExpectXML: @"<IndirAny><T1></T1><T2></T2></IndirAny>",
        Value: Ꮡ(new IndirAny(Any: nil))
    ),
    new(
        ExpectXML: @"<IfaceAny><T1></T1><Any>hi</Any><T2></T2></IfaceAny>",
        Value: Ꮡ(new IfaceAny(Any: (@string)"hi")),
        MarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceAny><T1></T1><Any>hi</Any><T2></T2></IfaceAny>",
        Value: Ꮡ(new IfaceAny(Any: default)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceAny><T1></T1><T2></T2></IfaceAny>",
        Value: Ꮡ(new IfaceAny(Any: default))
    ),
    new(
        ExpectXML: @"<IfaceAny><T1></T1><T2></T2></IfaceAny>",
        Value: Ꮡ(new IfaceAny(Any: default)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<DirectAny><T1></T1><Any>hi</Any><T2></T2></DirectAny>",
        Value: Ꮡ(new DirectAny(Any: ((@string)"hi"u8)))
    ),
    new(
        ExpectXML: @"<DirectAny><T1></T1><Any></Any><T2></T2></DirectAny>",
        Value: Ꮡ(new DirectAny(Any: ((@string)""u8)))
    ),
    new(
        ExpectXML: @"<IndirFoo><T1></T1><Foo>hi</Foo><T2></T2></IndirFoo>",
        Value: Ꮡ(new IndirAny(Any: stringptr("hi"))),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IndirFoo><T1></T1><Foo></Foo><T2></T2></IndirFoo>",
        Value: Ꮡ(new IndirAny(Any: stringptr(""))),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IndirFoo><T1></T1><T2></T2></IndirFoo>",
        Value: Ꮡ(new IndirAny(Any: nil)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceFoo><T1></T1><Foo>hi</Foo><T2></T2></IfaceFoo>",
        Value: Ꮡ(new IfaceAny(Any: default)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceFoo><T1></T1><T2></T2></IfaceFoo>",
        Value: Ꮡ(new IfaceAny(Any: default)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceFoo><T1></T1><T2></T2></IfaceFoo>",
        Value: Ꮡ(new IfaceAny(Any: default)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<DirectFoo><T1></T1><Foo>hi</Foo><T2></T2></DirectFoo>",
        Value: Ꮡ(new DirectAny(Any: ((@string)"hi"u8))),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<DirectFoo><T1></T1><Foo></Foo><T2></T2></DirectFoo>",
        Value: Ꮡ(new DirectAny(Any: ((@string)""u8))),
        UnmarshalOnly: true
    )
]); }

public static void TestMarshal(ж<testing.T> t) {
    foreach (var (idx, vᴛ1) in marshalTests) {
        ref var test = ref heap(new marshalTestsᴛ1(), out var Ꮡtest);
        test = vᴛ1;

        if (test.UnmarshalOnly) {
            continue;
        }

        var testʗ1 = test;
        t.Run(fmt.Sprintf("%d"u8, idx), (ж<testing.T> tΔ1) => {
            var (data, err) = Marshal(testʗ1.Value);
            if (err != nil) {
                if (testʗ1.MarshalError == ""u8) {
                    tΔ1.Errorf("marshal(%#v): %s", testʗ1.Value, err);
                    return;
                }
                if (!strings.Contains(err.Error(), testʗ1.MarshalError)) {
                    tΔ1.Errorf("marshal(%#v): %s, want %q", testʗ1.Value, err, testʗ1.MarshalError);
                }
                return;
            }
            if (testʗ1.MarshalError != ""u8) {
                tΔ1.Errorf("Marshal succeeded, want error %q", testʗ1.MarshalError);
                return;
            }
            {
                @string got = (@string)data;
                @string want = testʗ1.ExpectXML; if (got != want) {
                    if (strings.Contains(want, "\n")) {
                        tΔ1.Errorf("marshal(%#v):\nHAVE:\n%s\nWANT:\n%s", testʗ1.Value, got, want);
                    } else {
                        tΔ1.Errorf("marshal(%#v):\nhave %#q\nwant %#q", testʗ1.Value, got, want);
                    }
                }
            }
        });
    }
}

public partial struct AttrParent {
    public @string X; /*`xml:"X>Y,attr"`*/
}

public partial struct BadAttr {
    public map<@string, @string> Name; /*`xml:"name,attr"`*/
}

partial struct marshalErrorTestsᴛ1 /*dyn*/ {
    public any Value;
    public @string Err;
    public reflectꓸKind Kind;
}
internal static slice<marshalErrorTestsᴛ1> marshalErrorTests = slice<marshalErrorTestsᴛ1>([
    new(
        Value: new channel<bool>(0),
        Err: "xml: unsupported type: chan bool",
        Kind: reflect.Chan
    ),
    new(
        Value: new map<@string, @string>{
            ["question"] = "What do you get when you multiply six by nine?",
            ["answer"] = "42"
        },
        Err: "xml: unsupported type: map[string]string",
        Kind: reflect.Map
    ),
    new(
        Value: new map<ж<Ship>, bool>{[default] = false},
        Err: "xml: unsupported type: map[*xml.Ship]bool",
        Kind: reflect.Map
    ),
    new(
        Value: Ꮡ(new Domain(Comment: slice<byte>("f--bar"u8))),
        Err: @"xml: comments must not contain ""--"""
    ),
    // Reject parent chain with attr, never worked; see golang.org/issue/5033.
    new(
        Value: Ꮡ(new AttrParent()),
        Err: @"xml: X>Y chain not valid with attr flag"
    ),
    new(
        Value: new BadAttr(new map<@string, @string>{["X"] = "Y"}),
        Err: @"xml: unsupported type: map[string]string"
    )
]);

partial struct marshalIndentTestsᴛ1 /*dyn*/ {
    public any Value;
    public @string Prefix;
    public @string Indent;
    public @string ExpectXML;
}
internal static slice<marshalIndentTestsᴛ1> marshalIndentTests = slice<marshalIndentTestsᴛ1>([
    new(
        Value: Ꮡ(new SecretAgent(
            Handle: "007",
            Identity: "James Bond",
            Obfuscate: "<redacted/>"
        )),
        Prefix: "",
        Indent: "\t",
        ExpectXML: "<agent handle=\"007\">\n\t<Identity>James Bond</Identity><redacted/>\n</agent>"
    )
]);

public static void TestMarshalErrors(ж<testing.T> t) {
    foreach (var (idx, test) in marshalErrorTests) {
        var (data, err) = Marshal(test.Value);
        if (err == nil) {
            t.Errorf("#%d: marshal(%#v) = [success] %q, want error %v", idx, test.Value, data, test.Err);
            continue;
        }
        if (err.Error() != test.Err) {
            t.Errorf("#%d: marshal(%#v) = [error] %v, want %v", idx, test.Value, err, test.Err);
        }
        if (test.Kind != reflect.Invalid) {
            {
                reflectꓸKind kind = err._<ж<global::go.encoding.xml_package.UnsupportedTypeError>>().Type.Kind(); if (kind != test.Kind) {
                    t.Errorf("#%d: marshal(%#v) = [error kind] %s, want %s", idx, test.Value, kind, test.Kind);
                }
            }
        }
    }
}

// Do invertibility testing on the various structures that we test
public static void TestUnmarshal(ж<testing.T> t) {
    foreach (var (i, vᴛ1) in marshalTests) {
        ref var test = ref heap(new marshalTestsᴛ1(), out var Ꮡtest);
        test = vᴛ1;

        if (test.MarshalOnly) {
            continue;
        }
        {
            var (_, ok) = test.Value._<ж<Plain>>(ᐧ); if (ok) {
                continue;
            }
        }
        if (test.ExpectXML == @"<top>"u8 + @"<x><b xmlns=""space"">b</b>"u8 + @"<b xmlns=""space1"">b1</b></x>"u8 + @"</top>"u8) {
            // TODO(rogpeppe): re-enable this test in
            // https://go-review.googlesource.com/#/c/5910/
            continue;
        }

        var vt = reflect.TypeOf(test.Value);
        var dest = reflect.New(vt.Elem()).Interface();
        var err = Unmarshal(slice<byte>(test.ExpectXML), dest);

        var destʗ1 = dest;
        var errʗ1 = err;
        var testʗ1 = test;
        t.Run(fmt.Sprintf("%d"u8, i), (ж<testing.T> tΔ1) => {
            switch (destʗ1.type()) {
            case ж<Feed> fix: {
                fix.Author.InnerXML = ""u8;
                foreach (var iΔ1 in range(fix.Entry)) {
                    fix.Entry[iΔ1].Author.InnerXML = ""u8;
                }
                break;
            }}

            if (errʗ1 != nil) {
                if (testʗ1.UnmarshalError == ""u8) {
                    tΔ1.Errorf("unmarshal(%#v): %s", testʗ1.ExpectXML, errʗ1);
                    return;
                }
                if (!strings.Contains(errʗ1.Error(), testʗ1.UnmarshalError)) {
                    tΔ1.Errorf("unmarshal(%#v): %s, want %q", testʗ1.ExpectXML, errʗ1, testʗ1.UnmarshalError);
                }
                return;
            }
            {
                var (got, want) = (destʗ1, testʗ1.Value); if (!reflect.DeepEqual(got, want)) {
                    tΔ1.Errorf("unmarshal(%q):\nhave %#v\nwant %#v", testʗ1.ExpectXML, got, want);
                }
            }
        });
    }
}

public static void TestMarshalIndent(ж<testing.T> t) {
    foreach (var (i, test) in marshalIndentTests) {
        var (data, err) = MarshalIndent(test.Value, test.Prefix, test.Indent);
        if (err != nil) {
            t.Errorf("#%d: Error: %s", i, err);
            continue;
        }
        {
            @string got = (@string)data;
            @string want = test.ExpectXML; if (got != want) {
                t.Errorf("#%d: MarshalIndent:\nGot:%s\nWant:\n%s", i, got, want);
            }
        }
    }
}

internal partial struct limitedBytesWriter {
    internal io.Writer w;
    internal nint remain; // until writes fail
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string writeLimitHitˢ => ref writeLimitHitˢᵇ;
private static readonly @string writeLimitHitˢᵇ = "write limit hit"u8;

internal static (nint n, error err) Write(this ref limitedBytesWriter lw, slice<byte> p) {
    nint n = default;
    error err = default;

    if (lw.remain <= 0) {
        println((@string)"error");
        return (0, errors.New(writeLimitHitˢ));
    }
    if (len(p) > lw.remain) {
        p = p.slice(0, lw.remain);
        (n, _) = lw.w.Write(p);
        lw.remain = 0;
        return (n, errors.New(writeLimitHitˢ));
    }
    (n, err) = lw.w.Write(p);
    lw.remain -= n;
    return (n, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedAnErrorˢ = (@string)"expected an error"u8;

public static void TestMarshalWriteErrors(ж<testing.T> t) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    const nint writeCap = 1024;
    var w = Ꮡ(new limitedBytesWriter(new xml_test_package.bytes_BufferжWriter(Ꮡbuf), writeCap));
    var enc = NewEncoder(new xml_internal_test_package.limitedBytesWriterжWriter(w));
    error err = default;
    nint i = default;
    const nint n = 4000;
    for (i = 1; i <= n; i++) {
        err = enc.Encode(Ꮡ(new Passenger(
            Name: slice<@string>(["Alice", "Bob"]),
            Weight: 5F
        )));
        if (err != nil) {
            break;
        }
    }
    if (err == nil) {
        t.Error(expectedAnErrorˢ);
    }
    if (i == n) {
        t.Errorf("expected to fail before the end");
    }
    if (buf.Len() != writeCap) {
        t.Errorf("buf.Len() = %d; want %d", buf.Len(), (nint)(writeCap));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unwritableˢ => ref unwritableˢᵇ;
private static readonly @string unwritableˢᵇ = "unwritable"u8;

public static void TestMarshalWriteIOErrors(ж<testing.T> t) {
    var enc = NewEncoder(new errWriter());

    @string expectErr = unwritableˢ;
    var err = enc.Encode(Ꮡ(new Passenger()));
    if (err == nil || err.Error() != expectErr) {
        t.Errorf("EscapeTest = [error] %v, want %v", err, expectErr);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object helloWorldˢ = (@string)"hello world"u8;

public static void TestMarshalFlush(ж<testing.T> t) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    var enc = NewEncoder(new xml_test_package.strings_BuilderжWriter(Ꮡbuf));
    {
        var err = enc.EncodeToken((global::go.encoding.xml_package.CharData)slice<byte>((@string)"hello world"u8)); if (err != nil) {
            t.Fatalf("enc.EncodeToken: %v", err);
        }
    }
    if (buf.Len() > 0) {
        t.Fatalf("enc.EncodeToken caused actual write: %q", buf.String());
    }
    {
        var err = enc.Flush(); if (err != nil) {
            t.Fatalf("enc.Flush: %v", err);
        }
    }
    if (buf.String() != "hello world"u8) {
        t.Fatalf("after enc.Flush, buf.String() = %q, want %q", buf.String(), helloWorldˢ);
    }
}

public static void BenchmarkMarshal(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            Marshal(atomValue.OrTypedNil());
        }
    });
}

public static void BenchmarkUnmarshal(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var xml = slice<byte>(atomXML);
    var xmlʗ1 = xml;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            Unmarshal(xmlʗ1, Ꮡ(new Feed()));
        }
    });
}

internal partial struct TestStructPointerMarshal_A /*dyn*/ {
    public @string XMLName; /*`xml:"a"`*/
    public slice<any> B;
}

internal partial struct TestStructPointerMarshal_C /*dyn*/ {
    public global::go.encoding.xml_package.Name XMLName;
    public @string Value; /*`xml:"value"`*/
}

// golang.org/issue/6556
public static void TestStructPointerMarshal(ж<testing.T> t) {
    var a = @new<TestStructPointerMarshal_A>();
    a.B = append(a.B, (any)(Ꮡ(new TestStructPointerMarshal_C(
        XMLName: new Name(Local: "c"),
        Value: "x"
    ))));

    var (b, err) = Marshal(a.OrTypedNil());
    if (err != nil) {
        t.Fatal(err);
    }
    {
        @string x = (@string)b; if (x != "<a><c><value>x</value></c></a>"u8) {
            t.Fatal(x);
        }
    }
    ref var v = ref heap(new TestStructPointerMarshal_A(), out var Ꮡv);
    err = Unmarshal(b, Ꮡv);
    if (err != nil) {
        t.Fatal(err);
    }
}

partial struct encodeTokenTestsᴛ1 /*dyn*/ {
    internal @string desc;
    internal slice<ΔToken> toks;
    internal @string want;
    internal @string err;
}
internal static slice<encodeTokenTestsᴛ1> encodeTokenTests;
internal static void initᴛencodeTokenTests() { encodeTokenTests = slice<encodeTokenTestsᴛ1>([new(
    desc: "start element with name space",
    toks: slice<ΔToken>([
        new StartElement(new Name("space", "local"), default)
    ]),
    want: @"<local xmlns=""space"">"
), new(
    desc: "start element with no name",
    toks: slice<ΔToken>([
        new StartElement(new Name("space", ""), default)
    ]),
    err: "xml: start tag with no name"
), new(
    desc: "end element with no name",
    toks: slice<ΔToken>([
        new EndElement(new Name("space", ""))
    ]),
    err: "xml: end tag with no name"
), new(
    desc: "char data",
    toks: slice<ΔToken>([
        (global::go.encoding.xml_package.CharData)slice<byte>((@string)"foo"u8)
    ]),
    want: @"foo"
), new(
    desc: "char data with escaped chars",
    toks: slice<ΔToken>([
        (global::go.encoding.xml_package.CharData)slice<byte>((@string)" \t\n"u8)
    ]),
    want: " &#x9;\n"
), new(
    desc: "comment",
    toks: slice<ΔToken>([
        (global::go.encoding.xml_package.Comment)slice<byte>((@string)"foo"u8)
    ]),
    want: @"<!--foo-->"
), new(
    desc: "comment with invalid content",
    toks: slice<ΔToken>([
        (global::go.encoding.xml_package.Comment)slice<byte>((@string)"foo-->"u8)
    ]),
    err: "xml: EncodeToken of Comment containing --> marker"
), new(
    desc: "proc instruction",
    toks: slice<ΔToken>([
        new ProcInst("Target", slice<byte>("Instruction"u8))
    ]),
    want: @"<?Target Instruction?>"
), new(
    desc: "proc instruction with empty target",
    toks: slice<ΔToken>([
        new ProcInst("", slice<byte>("Instruction"u8))
    ]),
    err: "xml: EncodeToken of ProcInst with invalid Target"
), new(
    desc: "proc instruction with bad content",
    toks: slice<ΔToken>([
        new ProcInst("", slice<byte>("Instruction?>"u8))
    ]),
    err: "xml: EncodeToken of ProcInst with invalid Target"
), new(
    desc: "directive",
    toks: slice<ΔToken>([
        (global::go.encoding.xml_package.Directive)slice<byte>((@string)"foo"u8)
    ]),
    want: @"<!foo>"
), new(
    desc: "more complex directive",
    toks: slice<ΔToken>([
        (global::go.encoding.xml_package.Directive)slice<byte>((@string)"DOCTYPE doc [ <!ELEMENT doc '>'> <!-- com>ment --> ]"u8)
    ]),
    want: @"<!DOCTYPE doc [ <!ELEMENT doc '>'> <!-- com>ment --> ]>"
), new(
    desc: "directive instruction with bad name",
    toks: slice<ΔToken>([
        (global::go.encoding.xml_package.Directive)slice<byte>((@string)"foo>"u8)
    ]),
    err: "xml: EncodeToken of Directive containing wrong < or > markers"
), new(
    desc: "end tag without start tag",
    toks: slice<ΔToken>([
        new EndElement(new Name("foo", "bar"))
    ]),
    err: "xml: end tag </bar> without start tag"
), new(
    desc: "mismatching end tag local name",
    toks: slice<ΔToken>([
        new StartElement(new Name("", "foo"), default),
        new EndElement(new Name("", "bar"))
    ]),
    err: "xml: end tag </bar> does not match start tag <foo>",
    want: @"<foo>"
), new(
    desc: "mismatching end tag namespace",
    toks: slice<ΔToken>([
        new StartElement(new Name("space", "foo"), default),
        new EndElement(new Name("another", "foo"))
    ]),
    err: "xml: end tag </foo> in namespace another does not match start tag <foo> in namespace space",
    want: @"<foo xmlns=""space"">"
), new(
    desc: "start element with explicit namespace",
    toks: slice<ΔToken>([
        new StartElement(new Name("space", "local"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("xmlns", "x"), "space"),
            new(new Name("space", "foo"), "value")
        ])
        )
    ]),
    want: @"<local xmlns=""space"" xmlns:_xmlns=""xmlns"" _xmlns:x=""space"" xmlns:space=""space"" space:foo=""value"">"
), new(
    desc: "start element with explicit namespace and colliding prefix",
    toks: slice<ΔToken>([
        new StartElement(new Name("space", "local"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("xmlns", "x"), "space"),
            new(new Name("space", "foo"), "value"),
            new(new Name("x", "bar"), "other")
        ])
        )
    ]),
    want: @"<local xmlns=""space"" xmlns:_xmlns=""xmlns"" _xmlns:x=""space"" xmlns:space=""space"" space:foo=""value"" xmlns:x=""x"" x:bar=""other"">"
), new(
    desc: "start element using previously defined namespace",
    toks: slice<ΔToken>([
        new StartElement(new Name("", "local"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("xmlns", "x"), "space")
        ])
        ),
        new StartElement(new Name("space", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("space", "x"), "y")
        ])
        )
    ]),
    want: @"<local xmlns:_xmlns=""xmlns"" _xmlns:x=""space""><foo xmlns=""space"" xmlns:space=""space"" space:x=""y"">"
), new(
    desc: "nested name space with same prefix",
    toks: slice<ΔToken>([
        new StartElement(new Name("", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("xmlns", "x"), "space1")
        ])
        ),
        new StartElement(new Name("", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("xmlns", "x"), "space2")
        ])
        ),
        new StartElement(new Name("", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("space1", "a"), "space1 value"),
            new(new Name("space2", "b"), "space2 value")
        ])
        ),
        new EndElement(new Name("", "foo")),
        new EndElement(new Name("", "foo")),
        new StartElement(new Name("", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("space1", "a"), "space1 value"),
            new(new Name("space2", "b"), "space2 value")
        ])
        )
    ]),
    want: @"<foo xmlns:_xmlns=""xmlns"" _xmlns:x=""space1""><foo _xmlns:x=""space2""><foo xmlns:space1=""space1"" space1:a=""space1 value"" xmlns:space2=""space2"" space2:b=""space2 value""></foo></foo><foo xmlns:space1=""space1"" space1:a=""space1 value"" xmlns:space2=""space2"" space2:b=""space2 value"">"
), new(
    desc: "start element defining several prefixes for the same name space",
    toks: slice<ΔToken>([
        new StartElement(new Name("space", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("xmlns", "a"), "space"),
            new(new Name("xmlns", "b"), "space"),
            new(new Name("space", "x"), "value")
        ])
        )
    ]),
    want: @"<foo xmlns=""space"" xmlns:_xmlns=""xmlns"" _xmlns:a=""space"" _xmlns:b=""space"" xmlns:space=""space"" space:x=""value"">"
), new(
    desc: "nested element redefines name space",
    toks: slice<ΔToken>([
        new StartElement(new Name("", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("xmlns", "x"), "space")
        ])
        ),
        new StartElement(new Name("space", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("xmlns", "y"), "space"),
            new(new Name("space", "a"), "value")
        ])
        )
    ]),
    want: @"<foo xmlns:_xmlns=""xmlns"" _xmlns:x=""space""><foo xmlns=""space"" _xmlns:y=""space"" xmlns:space=""space"" space:a=""value"">"
), new(
    desc: "nested element creates alias for default name space",
    toks: slice<ΔToken>([
        new StartElement(new Name("space", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("", "xmlns"), "space")
        ])
        ),
        new StartElement(new Name("space", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("xmlns", "y"), "space"),
            new(new Name("space", "a"), "value")
        ])
        )
    ]),
    want: @"<foo xmlns=""space"" xmlns=""space""><foo xmlns=""space"" xmlns:_xmlns=""xmlns"" _xmlns:y=""space"" xmlns:space=""space"" space:a=""value"">"
), new(
    desc: "nested element defines default name space with existing prefix",
    toks: slice<ΔToken>([
        new StartElement(new Name("", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("xmlns", "x"), "space")
        ])
        ),
        new StartElement(new Name("space", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("", "xmlns"), "space"),
            new(new Name("space", "a"), "value")
        ])
        )
    ]),
    want: @"<foo xmlns:_xmlns=""xmlns"" _xmlns:x=""space""><foo xmlns=""space"" xmlns=""space"" xmlns:space=""space"" space:a=""value"">"
), new(
    desc: "nested element uses empty attribute name space when default ns defined",
    toks: slice<ΔToken>([
        new StartElement(new Name("space", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("", "xmlns"), "space")
        ])
        ),
        new StartElement(new Name("space", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("", "attr"), "value")
        ])
        )
    ]),
    want: @"<foo xmlns=""space"" xmlns=""space""><foo xmlns=""space"" attr=""value"">"
), new(
    desc: "redefine xmlns",
    toks: slice<ΔToken>([
        new StartElement(new Name("", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("foo", "xmlns"), "space")
        ])
        )
    ]),
    want: @"<foo xmlns:foo=""foo"" foo:xmlns=""space"">"
), new(
    desc: "xmlns with explicit name space #1",
    toks: slice<ΔToken>([
        new StartElement(new Name("space", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("xml", "xmlns"), "space")
        ])
        )
    ]),
    want: @"<foo xmlns=""space"" xmlns:_xml=""xml"" _xml:xmlns=""space"">"
), new(
    desc: "xmlns with explicit name space #2",
    toks: slice<ΔToken>([
        new StartElement(new Name("space", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name(xmlURL, "xmlns"), "space")
        ])
        )
    ]),
    want: @"<foo xmlns=""space"" xml:xmlns=""space"">"
), new(
    desc: "empty name space declaration is ignored",
    toks: slice<ΔToken>([
        new StartElement(new Name("", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("xmlns", "foo"), "")
        ])
        )
    ]),
    want: @"<foo xmlns:_xmlns=""xmlns"" _xmlns:foo="""">"
), new(
    desc: "attribute with no name is ignored",
    toks: slice<ΔToken>([
        new StartElement(new Name("", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("", ""), "value")
        ])
        )
    ]),
    want: @"<foo>"
), new(
    desc: "namespace URL with non-valid name",
    toks: slice<ΔToken>([
        new StartElement(new Name("/34", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("/34", "x"), "value")
        ])
        )
    ]),
    want: @"<foo xmlns=""/34"" xmlns:_=""/34"" _:x=""value"">"
), new(
    desc: "nested element resets default namespace to empty",
    toks: slice<ΔToken>([
        new StartElement(new Name("space", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("", "xmlns"), "space")
        ])
        ),
        new StartElement(new Name("", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("", "xmlns"), ""),
            new(new Name("", "x"), "value"),
            new(new Name("space", "x"), "value")
        ])
        )
    ]),
    want: @"<foo xmlns=""space"" xmlns=""space""><foo xmlns="""" x=""value"" xmlns:space=""space"" space:x=""value"">"
), new(
    desc: "nested element requires empty default name space",
    toks: slice<ΔToken>([
        new StartElement(new Name("space", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("", "xmlns"), "space")
        ])
        ),
        new StartElement(new Name("", "foo"), default)
    ]),
    want: @"<foo xmlns=""space"" xmlns=""space""><foo>"
), new(
    desc: "attribute uses name space from xmlns",
    toks: slice<ΔToken>([
        new StartElement(new Name("some/space", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("", "attr"), "value"),
            new(new Name("some/space", "other"), "other value")
        ])
        )
    ]),
    want: @"<foo xmlns=""some/space"" attr=""value"" xmlns:space=""some/space"" space:other=""other value"">"
), new(
    desc: "default name space should not be used by attributes",
    toks: slice<ΔToken>([
        new StartElement(new Name("space", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("", "xmlns"), "space"),
            new(new Name("xmlns", "bar"), "space"),
            new(new Name("space", "baz"), "foo")
        ])
        ),
        new StartElement(new Name("space", "baz"), default),
        new EndElement(new Name("space", "baz")),
        new EndElement(new Name("space", "foo"))
    ]),
    want: @"<foo xmlns=""space"" xmlns=""space"" xmlns:_xmlns=""xmlns"" _xmlns:bar=""space"" xmlns:space=""space"" space:baz=""foo""><baz xmlns=""space""></baz></foo>"
), new(
    desc: "default name space not used by attributes, not explicitly defined",
    toks: slice<ΔToken>([
        new StartElement(new Name("space", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("", "xmlns"), "space"),
            new(new Name("space", "baz"), "foo")
        ])
        ),
        new StartElement(new Name("space", "baz"), default),
        new EndElement(new Name("space", "baz")),
        new EndElement(new Name("space", "foo"))
    ]),
    want: @"<foo xmlns=""space"" xmlns=""space"" xmlns:space=""space"" space:baz=""foo""><baz xmlns=""space""></baz></foo>"
), new(
    desc: "impossible xmlns declaration",
    toks: slice<ΔToken>([
        new StartElement(new Name("", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("", "xmlns"), "space")
        ])
        ),
        new StartElement(new Name("space", "bar"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("space", "attr"), "value")
        ])
        )
    ]),
    want: @"<foo xmlns=""space""><bar xmlns=""space"" xmlns:space=""space"" space:attr=""value"">"
), new(
    desc: "reserved namespace prefix -- all lower case",
    toks: slice<ΔToken>([
        new StartElement(new Name("", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("http://www.w3.org/2001/xmlSchema-instance", "nil"), "true")
        ])
        )
    ]),
    want: @"<foo xmlns:_xmlSchema-instance=""http://www.w3.org/2001/xmlSchema-instance"" _xmlSchema-instance:nil=""true"">"
), new(
    desc: "reserved namespace prefix -- all upper case",
    toks: slice<ΔToken>([
        new StartElement(new Name("", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("http://www.w3.org/2001/XMLSchema-instance", "nil"), "true")
        ])
        )
    ]),
    want: @"<foo xmlns:_XMLSchema-instance=""http://www.w3.org/2001/XMLSchema-instance"" _XMLSchema-instance:nil=""true"">"
), new(
    desc: "reserved namespace prefix -- all mixed case",
    toks: slice<ΔToken>([
        new StartElement(new Name("", "foo"), slice<global::go.encoding.xml_package.Attr>([
            new(new Name("http://www.w3.org/2001/XmLSchema-instance", "nil"), "true")
        ])
        )
    ]),
    want: @"<foo xmlns:_XmLSchema-instance=""http://www.w3.org/2001/XmLSchema-instance"" _XmLSchema-instance:nil=""true"">"
)
]); }

public static void TestEncodeToken(ж<testing.T> t) {
loop:
    foreach (var (i, vᴛ1) in encodeTokenTests) {
        ref var tt = ref heap(new encodeTokenTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        var enc = NewEncoder(new xml_test_package.strings_BuilderжWriter(Ꮡbuf));
        error err = default;
        foreach (var (j, tok) in tt.toks) {
            err = enc.EncodeToken(tok);
            if (err != nil && j < len(tt.toks) - 1) {
                t.Errorf("#%d %s token #%d: %v", i, tt.desc, j, err);
                goto continue_loop;
            }
        }
        var ttʗ1 = tt;
        void errorf(@string f, params ꓸꓸꓸany aʗp) {
            var a = aʗp.sslice();
            t.Errorf("#%d %s token #%d:%s", i, ttʗ1.desc, len(ttʗ1.toks) - 1, fmt.Sprintf(f, a.ꓸꓸꓸ));
        }
        switch (ᐧ) {
        case {} when tt.err != ""u8 && err == nil: {
            errorf(" expected error; got none");
            continue;
            break;
        }
        case {} when tt.err == ""u8 && err != nil: {
            errorf(" got error: %v", err);
            continue;
            break;
        }
        case {} when tt.err != ""u8 && err != nil && tt.err != err.Error(): {
            errorf(" error mismatch; got %v, want %v", err, tt.err);
            continue;
            break;
        }}
        {
            var errΔ1 = enc.Flush(); if (errΔ1 != nil) {
                errorf(" %v", errΔ1);
                continue;
            }
        }
        {
            @string got = buf.String(); if (got != tt.want) {
                errorf("\ngot  %v\nwant %v", got, tt.want);
                continue;
            }
        }
continue_loop:;
    }
break_loop:;
}

public static void TestProcInstEncodeToken(ж<testing.T> t) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var enc = NewEncoder(new xml_test_package.bytes_BufferжWriter(Ꮡbuf));

    {
        var err = enc.EncodeToken(new ProcInst("xml", slice<byte>("Instruction"u8))); if (err != nil) {
            t.Fatalf("enc.EncodeToken: expected to be able to encode xml target ProcInst as first token, %s", err);
        }
    }

    {
        var err = enc.EncodeToken(new ProcInst("Target", slice<byte>("Instruction"u8))); if (err != nil) {
            t.Fatalf("enc.EncodeToken: expected to be able to add non-xml target ProcInst");
        }
    }

    {
        var err = enc.EncodeToken(new ProcInst("xml", slice<byte>("Instruction"u8))); if (err == nil) {
            t.Fatalf("enc.EncodeToken: expected to not be allowed to encode xml target ProcInst when not first token");
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string xmlVersion10EncodingUtf8ˢ => ref xmlVersion10EncodingUtf8ˢᵇ;
private static readonly @string xmlVersion10EncodingUtf8ˢᵇ = """
<?xml version="1.0" encoding="UTF-8"?>
<?Target Instruction?>
<root>
</root>

"""u8.Raw();

public static void TestDecodeEncode(ж<testing.T> t) {
    ref var @in = ref heap(new bytes.Buffer(), out var Ꮡin);
    ref var @out = ref heap(new bytes.Buffer(), out var Ꮡout);
    @in.WriteString(xmlVersion10EncodingUtf8ˢ);
    var dec = NewDecoder(new xml_test_package.bytes_BufferжReader(Ꮡin));
    var enc = NewEncoder(new xml_test_package.bytes_BufferжWriter(Ꮡout));
    for (var (tok, err) = dec.Token(); err == nil; (tok, err) = dec.Token()) {
        err = enc.EncodeToken(tok);
        if (err != nil) {
            t.Fatalf("enc.EncodeToken: Unable to encode token (%#v), %v", tok, err);
        }
    }
}

internal partial struct TestRace9796_A /*dyn*/ {
}

internal partial struct TestRace9796_B /*dyn*/ {
    public slice<TestRace9796_A> C; /*`xml:"X>Y"`*/
}

// Issue 9796. Used to fail with GORACE="halt_on_error=1" -race.
public static partial void TestRace9796(ж<testing.T> t) {
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    for (nint i = 0; i < 2; i++) {
        Ꮡwg.Add(1);
        goǃ(() => {
            Marshal(new TestRace9796_B(slice<TestRace9796_A>([new()])));
            Ꮡwg.Done();
        });
    }
    Ꮡwg.Wait();
}

public static void TestIsValidDirective(ж<testing.T> t) {
    var testOK = slice<@string>([
        "<>",
        "< < > >",
        "<!DOCTYPE '<' '>' '>' <!--nothing-->>",
        "<!DOCTYPE doc [ <!ELEMENT doc ANY> <!ELEMENT doc ANY> ]>",
        "<!DOCTYPE doc [ <!ELEMENT doc \"ANY> '<' <!E\" LEMENT '>' doc ANY> ]>",
        "<!DOCTYPE doc <!-- just>>>> a < comment --> [ <!ITEM anything> ] >"
    ]);
    var testKO = slice<@string>([
        "<",
        ">",
        "<!--",
        "-->",
        "< > > < < >",
        "<!dummy <!-- > -->",
        "<!DOCTYPE doc '>",
        "<!DOCTYPE doc '>'",
        "<!DOCTYPE doc <!--comment>"
    ]);
    foreach (var (_, s) in testOK) {
        if (!isValidDirective((global::go.encoding.xml_package.Directive)slice<byte>(s))) {
            t.Errorf("Directive %q is expected to be valid", s);
        }
    }
    foreach (var (_, s) in testKO) {
        if (isValidDirective((global::go.encoding.xml_package.Directive)slice<byte>(s))) {
            t.Errorf("Directive %q is expected to be invalid", s);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string object2Object2ˢ => ref object2Object2ˢᵇ;
private static readonly @string object2Object2ˢᵇ = "<object2></object2>"u8;

// Issue 11719. EncodeToken used to silently eat tokens with an invalid type.
public static void TestSimpleUseOfEncodeToken(ж<testing.T> t) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    var enc = NewEncoder(new xml_test_package.strings_BuilderжWriter(Ꮡbuf));
    {
        var err = enc.EncodeToken(Ꮡ(new StartElement(Name: new Name("", "object1")))); if (err == nil) {
            t.Errorf("enc.EncodeToken: pointer type should be rejected");
        }
    }
    {
        var err = enc.EncodeToken(Ꮡ(new EndElement(Name: new Name("", "object1")))); if (err == nil) {
            t.Errorf("enc.EncodeToken: pointer type should be rejected");
        }
    }
    {
        var err = enc.EncodeToken(new StartElement(Name: new Name("", "object2"))); if (err != nil) {
            t.Errorf("enc.EncodeToken: StartElement %s", err);
        }
    }
    {
        var err = enc.EncodeToken(new EndElement(Name: new Name("", "object2"))); if (err != nil) {
            t.Errorf("enc.EncodeToken: EndElement %s", err);
        }
    }
    {
        var err = enc.EncodeToken(new Universe()); if (err == nil) {
            t.Errorf("enc.EncodeToken: invalid type not caught");
        }
    }
    {
        var err = enc.Flush(); if (err != nil) {
            t.Errorf("enc.Flush: %s", err);
        }
    }
    if (buf.Len() == 0) {
        t.Errorf("enc.EncodeToken: empty buffer");
    }
    @string want = object2Object2ˢ;
    if (buf.String() != want) {
        t.Errorf("enc.EncodeToken: expected %q; got %q", want, buf.String());
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string dataᶜ => ref dataᶜᵇ;
private static readonly @string dataᶜᵇ = @"<foo b=""HELLOWORLD""></foo>"u8;

internal partial struct TestIssue16158_type /*dyn*/ {
    public byte B; /*`xml:"b,attr,omitempty"`*/
}

// Issue 16158. Decoder.unmarshalAttr ignores the return value of copyValue.
public static void TestIssue16158(ж<testing.T> t) {
    @string data = dataᶜ;
    var err = Unmarshal(slice<byte>(data), Ꮡ(new TestIssue16158_type()));
    if (err == nil) {
        t.Errorf("Unmarshal: expected error, got nil");
    }
}

partial struct InvalidXMLName_Type /*dyn*/ {
    public global::go.encoding.xml_package.Name XMLName; /*`xml:"type,attr"`*/
}

// Issue 20953. Crash on invalid XMLName attribute.
public partial struct InvalidXMLName {
    public global::go.encoding.xml_package.Name XMLName; /*`xml:"error"`*/
    public InvalidXMLName_Type Type;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unexpectedSuccessˢ = (@string)"unexpected success"u8;
internal static ref readonly @string invalidTagˢ => ref invalidTagˢᵇ;
private static readonly @string invalidTagˢᵇ = "invalid tag"u8;

public static void TestInvalidXMLName(ж<testing.T> t) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var enc = NewEncoder(new xml_test_package.bytes_BufferжWriter(Ꮡbuf));
    {
        var err = enc.Encode(new InvalidXMLName()); if (err == nil) {
            t.Error(unexpectedSuccessˢ);
        } else {
            @string want = invalidTagˢ; if (!strings.Contains(err.Error(), want)) {
                t.Errorf("error %q does not contain %q", err, want);
            }
        }
    }
}

// Issue 50164. Crash on zero value XML attribute.
public partial struct LayerOne {
    public global::go.encoding.xml_package.Name XMLName; /*`xml:"l1"`*/
    public ж<float64> Value; /*`xml:"value,omitempty"`*/
    /*embed*/ public ж<LayerTwo> LayerTwo; /*`xml:",omitempty"`*/
}

public partial struct LayerTwo {
    public ж<nint> ValueTwo; /*`xml:"value_two,attr,omitempty"`*/
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string l1Value12345ValueL1ˢ => ref l1Value12345ValueL1ˢᵇ;
private static readonly @string l1Value12345ValueL1ˢᵇ = @"<l1><value>1.2345</value></l1>"u8;

public static void TestMarshalZeroValue(ж<testing.T> t) {
    @string proofXml = l1Value12345ValueL1ˢ;
    ref var l1 = ref heap(new LayerOne(), out var Ꮡl1);
    var err = Unmarshal(slice<byte>(proofXml), Ꮡl1);
    if (err != nil) {
        t.Fatalf("unmarshal XML error: %v", err);
    }
    var want = (float64)1.2345D;
    var got = l1.Value.Value;
    if (got != want) {
        t.Fatalf("unexpected unmarshal result, want %f but got %f", want, got);
    }

    // Marshal again (or Encode again)
    // In issue 50164, here `Marshal(l1)` will panic because of the zero value of xml attribute ValueTwo `value_two`.
    (var anotherXML, err) = Marshal(l1);
    if (err != nil) {
        t.Fatalf("marshal XML error: %v", err);
    }
    if (((sstring)anotherXML) != proofXml) {
        t.Fatalf("unexpected unmarshal result, want %q but got %q", proofXml, anotherXML);
    }
}

internal static slice<encodeTokenTestsᴛ1> closeTests = slice<encodeTokenTestsᴛ1>([new(
    desc: "unclosed start element",
    toks: slice<ΔToken>([
        new StartElement(new Name("", "foo"), default)
    ]),
    want: @"<foo>",
    err: "unclosed tag <foo>"
), new(
    desc: "closed element",
    toks: slice<ΔToken>([
        new StartElement(new Name("", "foo"), default),
        new EndElement(new Name("", "foo"))
    ]),
    want: @"<foo></foo>"
), new(
    desc: "directive",
    toks: slice<ΔToken>([
        (global::go.encoding.xml_package.Directive)slice<byte>((@string)"foo"u8)
    ]),
    want: @"<!foo>"
)
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorGotNoneˢ = (@string)" expected error; got none"u8;

public static void TestClose(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in closeTests) {
        ref var tt = ref heap(new encodeTokenTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.desc, (ж<testing.T> tΔ1) => {
            ref var @out = ref heap(new strings.Builder(), out var Ꮡout);
            var enc = NewEncoder(new xml_test_package.strings_BuilderжWriter(Ꮡout));
            foreach (var (j, tok) in ttʗ1.toks) {
                {
                    var errΔ1 = enc.EncodeToken(tok); if (errΔ1 != nil) {
                        tΔ1.Fatalf("token #%d: %v", j, errΔ1);
                    }
                }
            }
            var err = enc.Close();
            switch (ᐧ) {
            case {} when ttʗ1.err != ""u8 && err == nil: {
                tΔ1.Error(expectedErrorGotNoneˢ);
                break;
            }
            case {} when ttʗ1.err == ""u8 && err != nil: {
                tΔ1.Errorf(" got error: %v", err);
                break;
            }
            case {} when ttʗ1.err != ""u8 && err != nil && ttʗ1.err != err.Error(): {
                tΔ1.Errorf(" error mismatch; got %v, want %v", err, ttʗ1.err);
                break;
            }}
            {
                @string got = @out.String(); if (got != ttʗ1.want) {
                    tΔ1.Errorf("\ngot  %v\nwant %v", got, ttʗ1.want);
                }
            }
            tΔ1.Log(enc.p.closed);
            {
                var errΔ1 = enc.EncodeToken((global::go.encoding.xml_package.Directive)slice<byte>((@string)"foo"u8)); if (errΔ1 == nil) {
                    tΔ1.Errorf("unexpected success when encoding after Close");
                }
            }
        });
    }
}

} // end xml_internal_test_package
