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
    public partial ref EmbedC EmbedC { get; }
    public EmbedB EmbedB;
    public @string FieldA;
    internal partial ref embedD embedD { get; }
}

public partial struct EmbedB {
    public @string FieldB;
    public partial ref ж<EmbedC> EmbedC { get; }
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
    public partial ref MyInt MyInt { get; }
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
    public partial ref ж<MyInt> MyInt { get; }
    public partial ref ж<NamedType> NamedType { get; }
}

public partial struct MyMarshalerTest {
}

internal static global::go.encoding.xml_package.Marshaler _ᴛ1ʗ = new xml_internal_test_package.MyMarshalerTestжMarshaler(((ж<MyMarshalerTest>)nil));

public static error MarshalXML(this ref MyMarshalerTest m, ж<global::go.encoding.xml_package.Encoder> Ꮡe, global::go.encoding.xml_package.StartElement start) {
    Ꮡe.EncodeToken(start);
    Ꮡe.EncodeToken(((global::go.encoding.xml_package.CharData)slice<byte>("hello world"u8)));
    Ꮡe.EncodeToken(new EndElement(start.Name));
    return default!;
}

public partial struct MyMarshalerAttrTest {
}

internal static global::go.encoding.xml_package.MarshalerAttr _ᴛ2ʗ = new xml_internal_test_package.MyMarshalerAttrTestжMarshalerAttr(((ж<MyMarshalerAttrTest>)nil));

public static (global::go.encoding.xml_package.Attr, error) MarshalXMLAttr(this ref MyMarshalerAttrTest m, global::go.encoding.xml_package.Name name) {
    return (new Attr(name, "hello world"u8), default!);
}

public static error UnmarshalXMLAttr(this ref MyMarshalerAttrTest m, global::go.encoding.xml_package.Attr attr) {
    return default!;
}

public partial struct MarshalerStruct {
    public MyMarshalerAttrTest Foo; /*`xml:",attr"`*/
}

public partial struct InnerStruct {
    public global::go.encoding.xml_package.Name XMLName; /*`xml:"testns outer"`*/
}

public partial struct OuterStruct {
    public partial ref InnerStruct InnerStruct { get; }
    public nint IntAttr; /*`xml:"int,attr"`*/
}

public partial struct OuterNamedStruct {
    public partial ref InnerStruct InnerStruct { get; }
    public global::go.encoding.xml_package.Name XMLName; /*`xml:"outerns test"`*/
    public nint IntAttr; /*`xml:"int,attr"`*/
}

public partial struct OuterNamedOrderedStruct {
    public global::go.encoding.xml_package.Name XMLName; /*`xml:"outerns test"`*/
    public partial ref InnerStruct InnerStruct { get; }
    public nint IntAttr; /*`xml:"int,attr"`*/
}

public partial struct OuterOuterStruct {
    public partial ref OuterStruct OuterStruct { get; }
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

internal static ж<@string> ᏑnameAttr = new StandardBox<@string>("Sarah"u8);
internal static ref @string nameAttr => ref ᏑnameAttr.Value;
internal static ж<nuint> ᏑageAttr = new StandardBox<nuint>((nuint)12);
internal static ref nuint ageAttr => ref ᏑageAttr.Value;
internal static ж<@string> ᏑcontentsAttr = new StandardBox<@string>("lorem ipsum"u8);
internal static ref @string contentsAttr => ref ᏑcontentsAttr.Value;
internal static ж<@string> Ꮡempty = new StandardBox<@string>(""u8);
internal static ref @string empty => ref Ꮡempty.Value;

// Test nil marshals to nothing
// Test value types
// Test time.
// A pointer to struct{} may be used to test for an element's presence.
// A []byte field is only nil if the element was not found.
// Check that []byte works, including named []byte types.
// Test innerxml
// Test structs
// Test a>b
// Uses interface{}
// Test struct embedding
// Shadowed by A.A
// Shadowed by A.A
// Shadowed by A.B.B
// Anonymous struct pointer field which is nil
// Other kinds of nil anonymous fields
// Test that name casing matters
// Test the order in which the XML element name is chosen
// xml.Name works in a plain field as well.
// Marshaling zero xml.Name uses the tag or field name.
// Test attributes
// pointer fields
// empty chardata pointer field
// omitempty on fields
// Test ",any"
// Test recursive types.
// Test ignoring fields via "-" tag
// Test escaping.
// Test outputting CDATA-wrapped text.
// Test omitempty with parent chain; see golang.org/issue/4168.
// Custom marshalers.
// Test pointer indirection in various kinds of fields.
// https://golang.org/issue/19063
// marshals without CDATA
// unmarshal leaves Chardata=stringptr("")
// marshals without CDATA
// marshals without CDATA
// marshals with CDATA
// unmarshal leaves CDATA=stringptr("")
// marshals with CDATA
// marshals with CDATA
// Note: Changed in Go 1.8 to include <OmitEmpty> element (because x.OmitEmpty != nil).
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
internal static void initᴛmarshalTests() { marshalTests = new marshalTestsᴛ1[]{
    new(Value: default!, ExpectXML: @""u8, MarshalOnly: true),
    new(Value: nilStruct.OrTypedNil(), ExpectXML: @""u8, MarshalOnly: true),
    new(Value: Ꮡ(new Plain(true)), ExpectXML: @"<Plain><V>true</V></Plain>"u8),
    new(Value: Ꮡ(new Plain(false)), ExpectXML: @"<Plain><V>false</V></Plain>"u8),
    new(Value: Ꮡ(new Plain((nint)42)), ExpectXML: @"<Plain><V>42</V></Plain>"u8),
    new(Value: Ꮡ(new Plain((int8)42)), ExpectXML: @"<Plain><V>42</V></Plain>"u8),
    new(Value: Ꮡ(new Plain((int16)42)), ExpectXML: @"<Plain><V>42</V></Plain>"u8),
    new(Value: Ꮡ(new Plain((int32)42)), ExpectXML: @"<Plain><V>42</V></Plain>"u8),
    new(Value: Ꮡ(new Plain((nuint)42)), ExpectXML: @"<Plain><V>42</V></Plain>"u8),
    new(Value: Ꮡ(new Plain((uint8)42)), ExpectXML: @"<Plain><V>42</V></Plain>"u8),
    new(Value: Ꮡ(new Plain((uint16)42)), ExpectXML: @"<Plain><V>42</V></Plain>"u8),
    new(Value: Ꮡ(new Plain((uint32)42)), ExpectXML: @"<Plain><V>42</V></Plain>"u8),
    new(Value: Ꮡ(new Plain((float32)1.25F)), ExpectXML: @"<Plain><V>1.25</V></Plain>"u8),
    new(Value: Ꮡ(new Plain((float64)1.25D)), ExpectXML: @"<Plain><V>1.25</V></Plain>"u8),
    new(Value: Ꮡ(new Plain((uintptr)0xFFDD)), ExpectXML: @"<Plain><V>65501</V></Plain>"u8),
    new(Value: Ꮡ(new Plain((@string)"gopher"u8)), ExpectXML: @"<Plain><V>gopher</V></Plain>"u8),
    new(Value: Ꮡ(new Plain(slice<byte>("gopher"u8))), ExpectXML: @"<Plain><V>gopher</V></Plain>"u8),
    new(Value: Ꮡ(new Plain((@string)"</>"u8)), ExpectXML: @"<Plain><V>&lt;/&gt;</V></Plain>"u8),
    new(Value: Ꮡ(new Plain(slice<byte>("</>"u8))), ExpectXML: @"<Plain><V>&lt;/&gt;</V></Plain>"u8),
    new(Value: Ꮡ(new Plain(new byte[]{(rune)'<', (rune)'/', (rune)'>'}.array())), ExpectXML: @"<Plain><V>&lt;/&gt;</V></Plain>"u8),
    new(Value: Ꮡ(new Plain(((NamedType)(@string)"potato"u8))), ExpectXML: @"<Plain><V>potato</V></Plain>"u8),
    new(Value: Ꮡ(new Plain(new nint[]{1, 2, 3}.slice())), ExpectXML: @"<Plain><V>1</V><V>2</V><V>3</V></Plain>"u8),
    new(Value: Ꮡ(new Plain(new nint[]{1, 2, 3}.array())), ExpectXML: @"<Plain><V>1</V><V>2</V><V>3</V></Plain>"u8),
    new(Value: ifaceptr(true), MarshalOnly: true, ExpectXML: @"<bool>true</bool>"u8),
    new(
        Value: Ꮡ(new Plain(time.Unix(1000000000, 123456789).UTC())),
        ExpectXML: @"<Plain><V>2001-09-09T01:46:40.123456789Z</V></Plain>"u8
    ),
    new(
        Value: Ꮡ(new PresenceTest(@new<EmptyStruct>())),
        ExpectXML: @"<PresenceTest><Exists></Exists></PresenceTest>"u8
    ),
    new(
        Value: Ꮡ(new PresenceTest(nil)),
        ExpectXML: @"<PresenceTest></PresenceTest>"u8
    ),
    new(
        Value: Ꮡ(new Data(nil)),
        ExpectXML: @"<Data></Data>"u8,
        UnmarshalOnly: true
    ),
    new(
        Value: Ꮡ(new Data(Bytes: new byte[]{}.slice(), Custom: new MyBytes(new byte[]{}.slice()), Attr: new byte[]{}.slice())),
        ExpectXML: @"<Data Attr=""""><Bytes></Bytes><Custom></Custom></Data>"u8,
        UnmarshalOnly: true
    ),
    new(
        Value: Ꮡ(new Data(Bytes: slice<byte>("ab"u8), Custom: ((MyBytes)slice<byte>((@string)"cd"u8)), Attr: new byte[]{(rune)'v'}.slice())),
        ExpectXML: @"<Data Attr=""v""><Bytes>ab</Bytes><Custom>cd</Custom></Data>"u8
    ),
    new(
        Value: Ꮡ(new SecretAgent(
            Handle: "007"u8,
            Identity: "James Bond"u8,
            Obfuscate: "<redacted/>"u8
        )),
        ExpectXML: @"<agent handle=""007""><Identity>James Bond</Identity><redacted/></agent>"u8,
        MarshalOnly: true
    ),
    new(
        Value: Ꮡ(new SecretAgent(
            Handle: "007"u8,
            Identity: "James Bond"u8,
            Obfuscate: "<Identity>James Bond</Identity><redacted/>"u8
        )),
        ExpectXML: @"<agent handle=""007""><Identity>James Bond</Identity><redacted/></agent>"u8,
        UnmarshalOnly: true
    ),
    new(Value: Ꮡ(new Port(Type: "ssl"u8, Number: "443"u8)), ExpectXML: @"<port type=""ssl"">443</port>"u8),
    new(Value: Ꮡ(new Port(Number: "443"u8)), ExpectXML: @"<port>443</port>"u8),
    new(Value: Ꮡ(new Port(Type: "<unix>"u8)), ExpectXML: @"<port type=""&lt;unix&gt;""></port>"u8),
    new(Value: Ꮡ(new Port(Number: "443"u8, Comment: "https"u8)), ExpectXML: @"<port><!--https-->443</port>"u8),
    new(Value: Ꮡ(new Port(Number: "443"u8, Comment: "add space-"u8)), ExpectXML: @"<port><!--add space- -->443</port>"u8, MarshalOnly: true),
    new(Value: Ꮡ(new Domain(Name: slice<byte>("google.com&friends"u8))), ExpectXML: @"<domain>google.com&amp;friends</domain>"u8),
    new(Value: Ꮡ(new Domain(Name: slice<byte>("google.com"u8), Comment: slice<byte>(" &friends "u8))), ExpectXML: @"<domain>google.com<!-- &friends --></domain>"u8),
    new(Value: Ꮡ(new Book(Title: "Pride & Prejudice"u8)), ExpectXML: @"<book>Pride &amp; Prejudice</book>"u8),
    new(Value: Ꮡ(new Event(Year: -3114)), ExpectXML: @"<event>-3114</event>"u8),
    new(Value: Ꮡ(new Movie(Length: 13440)), ExpectXML: @"<movie>13440</movie>"u8),
    new(Value: Ꮡ(new Pi(Approximation: 3.14159265F)), ExpectXML: @"<pi>3.1415927</pi>"u8),
    new(Value: Ꮡ(new Universe(Visible: 9.3e13D)), ExpectXML: @"<universe>9.3e+13</universe>"u8),
    new(Value: Ꮡ(new Particle(HasMass: true)), ExpectXML: @"<particle>true</particle>"u8),
    new(Value: Ꮡ(new Departure(When: ParseTime("2013-01-09T00:15:00-09:00"u8))), ExpectXML: @"<departure>2013-01-09T00:15:00-09:00</departure>"u8),
    new(Value: atomValue.OrTypedNil(), ExpectXML: atomXML),
    new(Value: Ꮡ(new Generic<nint>(1)), ExpectXML: @"<Generic><X>1</X></Generic>"u8),
    new(
        Value: Ꮡ(new Ship(
            Name: "Heart of Gold"u8,
            Pilot: "Computer"u8,
            Age: 1,
            Drive: ImprobabilityDrive,
            Passenger: new ж<Passenger>[]{
                Ꮡ(new Passenger(
                    Name: new @string[]{"Zaphod"u8, "Beeblebrox"u8}.slice(),
                    Weight: 7.25F)),
                Ꮡ(new Passenger(
                    Name: new @string[]{"Trisha"u8, "McMillen"u8}.slice(),
                    Weight: 5.5F)),
                Ꮡ(new Passenger(
                    Name: new @string[]{"Ford"u8, "Prefect"u8}.slice(),
                    Weight: 7F)),
                Ꮡ(new Passenger(
                    Name: new @string[]{"Arthur"u8, "Dent"u8}.slice(),
                    Weight: 6.75F))
            }.slice()
        )),
        ExpectXML: @"<spaceship name=""Heart of Gold"" pilot=""Computer"">"u8 + @"<drive>"u8 + strconv.Itoa((nint)ImprobabilityDrive) + @"</drive>"u8 + @"<age>1</age>"u8 + @"<passenger>"u8 + @"<name>Zaphod</name>"u8 + @"<name>Beeblebrox</name>"u8 + @"<weight>7.25</weight>"u8 + @"</passenger>"u8 + @"<passenger>"u8 + @"<name>Trisha</name>"u8 + @"<name>McMillen</name>"u8 + @"<weight>5.5</weight>"u8 + @"</passenger>"u8 + @"<passenger>"u8 + @"<name>Ford</name>"u8 + @"<name>Prefect</name>"u8 + @"<weight>7</weight>"u8 + @"</passenger>"u8 + @"<passenger>"u8 + @"<name>Arthur</name>"u8 + @"<name>Dent</name>"u8 + @"<weight>6.75</weight>"u8 + @"</passenger>"u8 + @"</spaceship>"u8
    ),
    new(
        Value: Ꮡ(new NestedItems(Items: default!, Item1: default!)),
        ExpectXML: @"<result>"u8 + @"<Items>"u8 + @"</Items>"u8 + @"</result>"u8
    ),
    new(
        Value: Ꮡ(new NestedItems(Items: new @string[]{}.slice(), Item1: new @string[]{}.slice())),
        ExpectXML: @"<result>"u8 + @"<Items>"u8 + @"</Items>"u8 + @"</result>"u8,
        MarshalOnly: true
    ),
    new(
        Value: Ꮡ(new NestedItems(Items: default!, Item1: new @string[]{"A"u8}.slice())),
        ExpectXML: @"<result>"u8 + @"<Items>"u8 + @"<item1>A</item1>"u8 + @"</Items>"u8 + @"</result>"u8
    ),
    new(
        Value: Ꮡ(new NestedItems(Items: new @string[]{"A"u8, "B"u8}.slice(), Item1: default!)),
        ExpectXML: @"<result>"u8 + @"<Items>"u8 + @"<item>A</item>"u8 + @"<item>B</item>"u8 + @"</Items>"u8 + @"</result>"u8
    ),
    new(
        Value: Ꮡ(new NestedItems(Items: new @string[]{"A"u8, "B"u8}.slice(), Item1: new @string[]{"C"u8}.slice())),
        ExpectXML: @"<result>"u8 + @"<Items>"u8 + @"<item>A</item>"u8 + @"<item>B</item>"u8 + @"<item1>C</item1>"u8 + @"</Items>"u8 + @"</result>"u8
    ),
    new(
        Value: Ꮡ(new NestedOrder(Field1: "C"u8, Field2: "B"u8, Field3: "A"u8)),
        ExpectXML: @"<result>"u8 + @"<parent>"u8 + @"<c>C</c>"u8 + @"<b>B</b>"u8 + @"<a>A</a>"u8 + @"</parent>"u8 + @"</result>"u8
    ),
    new(
        Value: Ꮡ(new NilTest(A: (@string)"A"u8, B: default!, C: (@string)"C"u8)),
        ExpectXML: @"<NilTest>"u8 + @"<parent1>"u8 + @"<parent2><a>A</a></parent2>"u8 + @"<parent2><c>C</c></parent2>"u8 + @"</parent1>"u8 + @"</NilTest>"u8,
        MarshalOnly: true
    ),
    new(
        Value: Ꮡ(new MixedNested(A: "A"u8, B: "B"u8, C: "C"u8, D: "D"u8)),
        ExpectXML: @"<result>"u8 + @"<parent1><a>A</a></parent1>"u8 + @"<b>B</b>"u8 + @"<parent1>"u8 + @"<parent2><c>C</c></parent2>"u8 + @"<d>D</d>"u8 + @"</parent1>"u8 + @"</result>"u8
    ),
    new(
        Value: Ꮡ(new Service(Port: Ꮡ(new Port(Number: "80"u8)))),
        ExpectXML: @"<service><host><port>80</port></host></service>"u8
    ),
    new(
        Value: Ꮡ(new Service(nil)),
        ExpectXML: @"<service></service>"u8
    ),
    new(
        Value: Ꮡ(new Service(Port: Ꮡ(new Port(Number: "80"u8)), Extra1: (@string)"A"u8, Extra2: (@string)"B"u8)),
        ExpectXML: @"<service>"u8 + @"<host><port>80</port></host>"u8 + @"<Extra1>A</Extra1>"u8 + @"<host><extra2>B</extra2></host>"u8 + @"</service>"u8,
        MarshalOnly: true
    ),
    new(
        Value: Ꮡ(new Service(Port: Ꮡ(new Port(Number: "80"u8)), Extra2: (@string)"example"u8)),
        ExpectXML: @"<service>"u8 + @"<host><port>80</port></host>"u8 + @"<host><extra2>example</extra2></host>"u8 + @"</service>"u8,
        MarshalOnly: true
    ),
    new(
        Value: Ꮡ(new Δtype(
            A: "a"u8,
            B: "b"u8,
            C: "c"u8,
            C1: "c1"u8,
            D1: "d1"u8
        )),
        ExpectXML: @"<top xmlns=""space"">"u8 + @"<x><a>a</a><b>b</b><c xmlns=""space"">c</c>"u8 + @"<c xmlns=""space1"">c1</c>"u8 + @"<d xmlns=""space1"">d1</d>"u8 + @"</x>"u8 + @"</top>"u8
    ),
    new(
        Value: Ꮡ(new Δtypeᴛ1(
            XMLName: new Name(
                Space: "space0"u8,
                Local: "top"u8
            ),
            A: "a"u8,
            B: "b"u8,
            C: "c"u8,
            C1: "c1"u8,
            D1: "d1"u8
        )),
        ExpectXML: @"<top xmlns=""space0"">"u8 + @"<x><a>a</a><b>b</b>"u8 + @"<c xmlns=""space"">c</c>"u8 + @"<c xmlns=""space1"">c1</c>"u8 + @"<d xmlns=""space1"">d1</d>"u8 + @"</x>"u8 + @"</top>"u8
    ),
    new(
        Value: Ꮡ(new Δtypeᴛ2(
            B: "b"u8,
            B1: "b1"u8
        )),
        ExpectXML: @"<top>"u8 + @"<x><b xmlns=""space"">b</b>"u8 + @"<b xmlns=""space1"">b1</b></x>"u8 + @"</top>"u8
    ),
    new(
        Value: Ꮡ(new EmbedA(
            EmbedC: new EmbedC(
                FieldA1: ""u8,
                FieldA2: ""u8,
                FieldB: "A.C.B"u8,
                FieldC: "A.C.C"u8
            ),
            EmbedB: new EmbedB(
                FieldB: "A.B.B"u8,
                EmbedC: Ꮡ(new EmbedC(
                    FieldA1: "A.B.C.A1"u8,
                    FieldA2: "A.B.C.A2"u8,
                    FieldB: ""u8,
                    FieldC: "A.B.C.C"u8
                ))
            ),
            FieldA: "A.A"u8,
            embedD: new embedD(
                FieldE: "A.D.E"u8
            )
        )),
        ExpectXML: @"<EmbedA>"u8 + @"<FieldB>A.C.B</FieldB>"u8 + @"<FieldC>A.C.C</FieldC>"u8 + @"<EmbedB>"u8 + @"<FieldB>A.B.B</FieldB>"u8 + @"<FieldA>"u8 + @"<A1>A.B.C.A1</A1>"u8 + @"<A2>A.B.C.A2</A2>"u8 + @"</FieldA>"u8 + @"<FieldC>A.B.C.C</FieldC>"u8 + @"</EmbedB>"u8 + @"<FieldA>A.A</FieldA>"u8 + @"<FieldE>A.D.E</FieldE>"u8 + @"</EmbedA>"u8
    ),
    new(
        Value: Ꮡ(new EmbedB(nil)),
        ExpectXML: @"<EmbedB><FieldB></FieldB></EmbedB>"u8
    ),
    new(
        Value: Ꮡ(new PointerAnonFields(nil)),
        ExpectXML: @"<PointerAnonFields></PointerAnonFields>"u8
    ),
    new(
        Value: Ꮡ(new NameCasing(Xy: "mixed"u8, XY: "upper"u8, XyA: "mixedA"u8, XYA: "upperA"u8)),
        ExpectXML: @"<casing Xy=""mixedA"" XY=""upperA""><Xy>mixed</Xy><XY>upper</XY></casing>"u8
    ),
    new(
        Value: Ꮡ(new NamePrecedence(
            FromTag: new XMLNameWithoutTag(Value: "A"u8),
            FromNameVal: new XMLNameWithoutTag(XMLName: new Name(Local: "InXMLName"u8), Value: "B"u8),
            FromNameTag: new XMLNameWithTag(Value: "C"u8),
            InFieldName: "D"u8
        )),
        ExpectXML: @"<Parent>"u8 + @"<InTag>A</InTag>"u8 + @"<InXMLName>B</InXMLName>"u8 + @"<InXMLNameTag>C</InXMLNameTag>"u8 + @"<InFieldName>D</InFieldName>"u8 + @"</Parent>"u8,
        MarshalOnly: true
    ),
    new(
        Value: Ꮡ(new NamePrecedence(
            XMLName: new Name(Local: "Parent"u8),
            FromTag: new XMLNameWithoutTag(XMLName: new Name(Local: "InTag"u8), Value: "A"u8),
            FromNameVal: new XMLNameWithoutTag(XMLName: new Name(Local: "FromNameVal"u8), Value: "B"u8),
            FromNameTag: new XMLNameWithTag(XMLName: new Name(Local: "InXMLNameTag"u8), Value: "C"u8),
            InFieldName: "D"u8
        )),
        ExpectXML: @"<Parent>"u8 + @"<InTag>A</InTag>"u8 + @"<FromNameVal>B</FromNameVal>"u8 + @"<InXMLNameTag>C</InXMLNameTag>"u8 + @"<InFieldName>D</InFieldName>"u8 + @"</Parent>"u8,
        UnmarshalOnly: true
    ),
    new(
        Value: Ꮡ(new NameInField(new Name(Space: "ns"u8, Local: "foo"u8))),
        ExpectXML: @"<NameInField><foo xmlns=""ns""></foo></NameInField>"u8
    ),
    new(
        Value: Ꮡ(new NameInField(new Name(Space: "ns"u8, Local: "foo"u8))),
        ExpectXML: @"<NameInField><foo xmlns=""ns""><ignore></ignore></foo></NameInField>"u8,
        UnmarshalOnly: true
    ),
    new(
        Value: Ꮡ(new NameInField(nil)),
        ExpectXML: @"<NameInField><foo xmlns=""ns""></foo></NameInField>"u8,
        MarshalOnly: true
    ),
    new(
        Value: Ꮡ(new AttrTest(
            Int: 8,
            Named: 9,
            Float: 23.5D,
            Uint8: 255,
            Bool: true,
            Str: "str"u8,
            Bytes: slice<byte>("byt"u8)
        )),
        ExpectXML: @"<AttrTest Int=""8"" int=""9"" Float=""23.5"" Uint8=""255"""u8 + @" Bool=""true"" Str=""str"" Bytes=""byt""></AttrTest>"u8
    ),
    new(
        Value: Ꮡ(new AttrTest(Bytes: new byte[]{}.slice())),
        ExpectXML: @"<AttrTest Int=""0"" int=""0"" Float=""0"" Uint8=""0"""u8 + @" Bool=""false"" Str="""" Bytes=""""></AttrTest>"u8
    ),
    new(
        Value: Ꮡ(new AttrsTest(
            Attrs: new global::go.encoding.xml_package.Attr[]{
                new(Name: new Name(Local: "Answer"u8), Value: "42"u8),
                new(Name: new Name(Local: "Int"u8), Value: "8"u8),
                new(Name: new Name(Local: "int"u8), Value: "9"u8),
                new(Name: new Name(Local: "Float"u8), Value: "23.5"u8),
                new(Name: new Name(Local: "Uint8"u8), Value: "255"u8),
                new(Name: new Name(Local: "Bool"u8), Value: "true"u8),
                new(Name: new Name(Local: "Str"u8), Value: "str"u8),
                new(Name: new Name(Local: "Bytes"u8), Value: "byt"u8)
            }.slice()
        )),
        ExpectXML: @"<AttrsTest Answer=""42"" Int=""8"" int=""9"" Float=""23.5"" Uint8=""255"" Bool=""true"" Str=""str"" Bytes=""byt"" Int=""0"" int=""0"" Float=""0"" Uint8=""0"" Bool=""false"" Str="""" Bytes=""""></AttrsTest>"u8,
        MarshalOnly: true
    ),
    new(
        Value: Ꮡ(new AttrsTest(
            Attrs: new global::go.encoding.xml_package.Attr[]{
                new(Name: new Name(Local: "Answer"u8), Value: "42"u8)
            }.slice(),
            Int: 8,
            Named: 9,
            Float: 23.5D,
            Uint8: 255,
            Bool: true,
            Str: "str"u8,
            Bytes: slice<byte>("byt"u8)
        )),
        ExpectXML: @"<AttrsTest Answer=""42"" Int=""8"" int=""9"" Float=""23.5"" Uint8=""255"" Bool=""true"" Str=""str"" Bytes=""byt""></AttrsTest>"u8
    ),
    new(
        Value: Ꮡ(new AttrsTest(
            Attrs: new global::go.encoding.xml_package.Attr[]{
                new(Name: new Name(Local: "Int"u8), Value: "0"u8),
                new(Name: new Name(Local: "int"u8), Value: "0"u8),
                new(Name: new Name(Local: "Float"u8), Value: "0"u8),
                new(Name: new Name(Local: "Uint8"u8), Value: "0"u8),
                new(Name: new Name(Local: "Bool"u8), Value: "false"u8),
                new(Name: new Name(Local: "Str"u8)),
                new(Name: new Name(Local: "Bytes"u8))
            }.slice(),
            Bytes: new byte[]{}.slice()
        )),
        ExpectXML: @"<AttrsTest Int=""0"" int=""0"" Float=""0"" Uint8=""0"" Bool=""false"" Str="""" Bytes="""" Int=""0"" int=""0"" Float=""0"" Uint8=""0"" Bool=""false"" Str="""" Bytes=""""></AttrsTest>"u8,
        MarshalOnly: true
    ),
    new(
        Value: Ꮡ(new OmitAttrTest(
            Int: 8,
            Named: 9,
            Float: 23.5D,
            Uint8: 255,
            Bool: true,
            Str: "str"u8,
            Bytes: slice<byte>("byt"u8),
            PStr: Ꮡempty
        )),
        ExpectXML: @"<OmitAttrTest Int=""8"" int=""9"" Float=""23.5"" Uint8=""255"""u8 + @" Bool=""true"" Str=""str"" Bytes=""byt"" PStr=""""></OmitAttrTest>"u8
    ),
    new(
        Value: Ꮡ(new OmitAttrTest(nil)),
        ExpectXML: @"<OmitAttrTest></OmitAttrTest>"u8
    ),
    new(
        Value: Ꮡ(new PointerFieldsTest(Name: ᏑnameAttr, Age: ᏑageAttr, Contents: ᏑcontentsAttr)),
        ExpectXML: @"<dummy name=""Sarah"" age=""12"">lorem ipsum</dummy>"u8,
        MarshalOnly: true
    ),
    new(
        Value: Ꮡ(new ChardataEmptyTest(nil)),
        ExpectXML: @"<test></test>"u8,
        MarshalOnly: true
    ),
    new(
        Value: Ꮡ(new OmitFieldTest(
            Int: 8,
            Named: 9,
            Float: 23.5D,
            Uint8: 255,
            Bool: true,
            Str: "str"u8,
            Bytes: slice<byte>("byt"u8),
            PStr: Ꮡempty,
            Ptr: Ꮡ(new PresenceTest(nil))
        )),
        ExpectXML: @"<OmitFieldTest>"u8 + @"<Int>8</Int>"u8 + @"<int>9</int>"u8 + @"<Float>23.5</Float>"u8 + @"<Uint8>255</Uint8>"u8 + @"<Bool>true</Bool>"u8 + @"<Str>str</Str>"u8 + @"<Bytes>byt</Bytes>"u8 + @"<PStr></PStr>"u8 + @"<Ptr></Ptr>"u8 + @"</OmitFieldTest>"u8
    ),
    new(
        Value: Ꮡ(new OmitFieldTest(nil)),
        ExpectXML: @"<OmitFieldTest></OmitFieldTest>"u8
    ),
    new(
        ExpectXML: @"<a><nested><value>known</value></nested><other><sub>unknown</sub></other></a>"u8,
        Value: Ꮡ(new AnyTest(
            Nested: "known"u8,
            AnyField: new AnyHolder(
                XMLName: new Name(Local: "other"u8),
                XML: "<sub>unknown</sub>"u8
            )
        ))
    ),
    new(
        Value: Ꮡ(new AnyTest(Nested: "known"u8,
            AnyField: new AnyHolder(
                XML: "<unknown/>"u8,
                XMLName: new Name(Local: "AnyField"u8)
            )
        )),
        ExpectXML: @"<a><nested><value>known</value></nested><AnyField><unknown/></AnyField></a>"u8
    ),
    new(
        ExpectXML: @"<a><nested><value>b</value></nested></a>"u8,
        Value: Ꮡ(new AnyOmitTest(
            Nested: "b"u8
        ))
    ),
    new(
        ExpectXML: @"<a><nested><value>b</value></nested><c><d>e</d></c><g xmlns=""f""><h>i</h></g></a>"u8,
        Value: Ꮡ(new AnySliceTest(
            Nested: "b"u8,
            AnyField: new AnyHolder[]{
                new(
                    XMLName: new Name(Local: "c"u8),
                    XML: "<d>e</d>"u8
                ),
                new(
                    XMLName: new Name(Space: "f"u8, Local: "g"u8),
                    XML: "<h>i</h>"u8
                )
            }.slice()
        ))
    ),
    new(
        ExpectXML: @"<a><nested><value>b</value></nested></a>"u8,
        Value: Ꮡ(new AnySliceTest(
            Nested: "b"u8
        ))
    ),
    new(
        Value: Ꮡ(new RecurseA(
            A: "a1"u8,
            B: Ꮡ(new RecurseB(
                A: Ꮡ(new RecurseA("a2"u8, nil)),
                B: "b1"u8
            ))
        )),
        ExpectXML: @"<RecurseA><A>a1</A><B><A><A>a2</A></A><B>b1</B></B></RecurseA>"u8
    ),
    new(
        ExpectXML: @"<IgnoreTest></IgnoreTest>"u8,
        Value: Ꮡ(new IgnoreTest(nil))
    ),
    new(
        ExpectXML: @"<IgnoreTest></IgnoreTest>"u8,
        Value: Ꮡ(new IgnoreTest(PublicSecret: "can't tell"u8)),
        MarshalOnly: true
    ),
    new(
        ExpectXML: @"<IgnoreTest><PublicSecret>ignore me</PublicSecret></IgnoreTest>"u8,
        Value: Ꮡ(new IgnoreTest(nil)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<a><nested><value>dquote: &#34;; squote: &#39;; ampersand: &amp;; less: &lt;; greater: &gt;;</value></nested><empty></empty></a>"u8,
        Value: Ꮡ(new AnyTest(
            Nested: @"dquote: ""; squote: '; ampersand: &; less: <; greater: >;"u8,
            AnyField: new AnyHolder(XMLName: new Name(Local: "empty"u8))
        ))
    ),
    new(
        ExpectXML: @"<a><nested><value>newline: &#xA;; cr: &#xD;; tab: &#x9;;</value></nested><AnyField></AnyField></a>"u8,
        Value: Ꮡ(new AnyTest(
            Nested: "newline: \n; cr: \r; tab: \t;"u8,
            AnyField: new AnyHolder(XMLName: new Name(Local: "AnyField"u8))
        ))
    ),
    new(
        ExpectXML: "<a><nested><value>1\r2\r\n3\n\r4\n5</value></nested></a>"u8,
        Value: Ꮡ(new AnyTest(
            Nested: "1\n2\n3\n\n4\n5"u8
        )),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<EmbedInt><MyInt>42</MyInt></EmbedInt>"u8,
        Value: Ꮡ(new EmbedInt(
            MyInt: 42
        ))
    ),
    new(
        ExpectXML: @"<CDataTest></CDataTest>"u8,
        Value: Ꮡ(new CDataTest(nil))
    ),
    new(
        ExpectXML: @"<CDataTest><![CDATA[http://example.com/tests/1?foo=1&bar=baz]]></CDataTest>"u8,
        Value: Ꮡ(new CDataTest(
            Chardata: "http://example.com/tests/1?foo=1&bar=baz"u8
        ))
    ),
    new(
        ExpectXML: @"<CDataTest><![CDATA[Literal <![CDATA[Nested]]]]><![CDATA[>!]]></CDataTest>"u8,
        Value: Ꮡ(new CDataTest(
            Chardata: "Literal <![CDATA[Nested]]>!"u8
        ))
    ),
    new(
        ExpectXML: @"<CDataTest><![CDATA[<![CDATA[Nested]]]]><![CDATA[> Literal!]]></CDataTest>"u8,
        Value: Ꮡ(new CDataTest(
            Chardata: "<![CDATA[Nested]]> Literal!"u8
        ))
    ),
    new(
        ExpectXML: @"<CDataTest><![CDATA[<![CDATA[Nested]]]]><![CDATA[> Literal! <![CDATA[Nested]]]]><![CDATA[> Literal!]]></CDataTest>"u8,
        Value: Ꮡ(new CDataTest(
            Chardata: "<![CDATA[Nested]]> Literal! <![CDATA[Nested]]> Literal!"u8
        ))
    ),
    new(
        ExpectXML: @"<CDataTest><![CDATA[<![CDATA[<![CDATA[Nested]]]]><![CDATA[>]]]]><![CDATA[>]]></CDataTest>"u8,
        Value: Ꮡ(new CDataTest(
            Chardata: "<![CDATA[<![CDATA[Nested]]>]]>"u8
        ))
    ),
    new(
        ExpectXML: @"<Strings><A></A></Strings>"u8,
        Value: Ꮡ(new Strings(nil))
    ),
    new(
        ExpectXML: @"<MyMarshalerTest>hello world</MyMarshalerTest>"u8,
        Value: Ꮡ(new MyMarshalerTest(nil))
    ),
    new(
        ExpectXML: @"<MarshalerStruct Foo=""hello world""></MarshalerStruct>"u8,
        Value: Ꮡ(new MarshalerStruct(nil))
    ),
    new(
        ExpectXML: @"<outer xmlns=""testns"" int=""10""></outer>"u8,
        Value: Ꮡ(new OuterStruct(IntAttr: 10))
    ),
    new(
        ExpectXML: @"<test xmlns=""outerns"" int=""10""></test>"u8,
        Value: Ꮡ(new OuterNamedStruct(XMLName: new Name(Space: "outerns"u8, Local: "test"u8), IntAttr: 10))
    ),
    new(
        ExpectXML: @"<test xmlns=""outerns"" int=""10""></test>"u8,
        Value: Ꮡ(new OuterNamedOrderedStruct(XMLName: new Name(Space: "outerns"u8, Local: "test"u8), IntAttr: 10))
    ),
    new(
        ExpectXML: @"<outer xmlns=""testns"" int=""10""></outer>"u8,
        Value: Ꮡ(new OuterOuterStruct(new OuterStruct(IntAttr: 10)))
    ),
    new(
        ExpectXML: @"<NestedAndChardata><A><B></B><B></B></A>test</NestedAndChardata>"u8,
        Value: Ꮡ(new NestedAndChardata(AB: new slice<@string>(2), Chardata: "test"u8))
    ),
    new(
        ExpectXML: @"<NestedAndComment><A><B></B><B></B></A><!--test--></NestedAndComment>"u8,
        Value: Ꮡ(new NestedAndComment(AB: new slice<@string>(2), Comment: "test"u8))
    ),
    new(
        ExpectXML: @"<NestedAndCData><A><B></B><B></B></A><![CDATA[test]]></NestedAndCData>"u8,
        Value: Ꮡ(new NestedAndCData(AB: new slice<@string>(2), CDATA: "test"u8))
    ),
    new(
        ExpectXML: @"<IndirComment><T1></T1><!--hi--><T2></T2></IndirComment>"u8,
        Value: Ꮡ(new IndirComment(Comment: stringptr("hi"u8))),
        MarshalOnly: true
    ),
    new(
        ExpectXML: @"<IndirComment><T1></T1><T2></T2></IndirComment>"u8,
        Value: Ꮡ(new IndirComment(Comment: stringptr(""u8))),
        MarshalOnly: true
    ),
    new(
        ExpectXML: @"<IndirComment><T1></T1><T2></T2></IndirComment>"u8,
        Value: Ꮡ(new IndirComment(Comment: nil)),
        MarshalError: "xml: bad type for comment field of xml.IndirComment"u8
    ),
    new(
        ExpectXML: @"<IndirComment><T1></T1><!--hi--><T2></T2></IndirComment>"u8,
        Value: Ꮡ(new IndirComment(Comment: nil)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceComment><T1></T1><!--hi--><T2></T2></IfaceComment>"u8,
        Value: Ꮡ(new IfaceComment(Comment: (@string)"hi"u8)),
        MarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceComment><T1></T1><!--hi--><T2></T2></IfaceComment>"u8,
        Value: Ꮡ(new IfaceComment(Comment: default!)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceComment><T1></T1><T2></T2></IfaceComment>"u8,
        Value: Ꮡ(new IfaceComment(Comment: default!)),
        MarshalError: "xml: bad type for comment field of xml.IfaceComment"u8
    ),
    new(
        ExpectXML: @"<IfaceComment><T1></T1><T2></T2></IfaceComment>"u8,
        Value: Ꮡ(new IfaceComment(Comment: default!)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<DirectComment><T1></T1><!--hi--><T2></T2></DirectComment>"u8,
        Value: Ꮡ(new DirectComment(Comment: ((@string)"hi"u8)))
    ),
    new(
        ExpectXML: @"<DirectComment><T1></T1><T2></T2></DirectComment>"u8,
        Value: Ꮡ(new DirectComment(Comment: ((@string)""u8)))
    ),
    new(
        ExpectXML: @"<IndirChardata><T1></T1>hi<T2></T2></IndirChardata>"u8,
        Value: Ꮡ(new IndirChardata(Chardata: stringptr("hi"u8)))
    ),
    new(
        ExpectXML: @"<IndirChardata><T1></T1><![CDATA[hi]]><T2></T2></IndirChardata>"u8,
        Value: Ꮡ(new IndirChardata(Chardata: stringptr("hi"u8))),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IndirChardata><T1></T1><T2></T2></IndirChardata>"u8,
        Value: Ꮡ(new IndirChardata(Chardata: stringptr(""u8)))
    ),
    new(
        ExpectXML: @"<IndirChardata><T1></T1><T2></T2></IndirChardata>"u8,
        Value: Ꮡ(new IndirChardata(Chardata: nil)),
        MarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceChardata><T1></T1>hi<T2></T2></IfaceChardata>"u8,
        Value: Ꮡ(new IfaceChardata(Chardata: ((@string)"hi"u8))),
        UnmarshalError: "cannot unmarshal into interface {}"u8
    ),
    new(
        ExpectXML: @"<IfaceChardata><T1></T1><![CDATA[hi]]><T2></T2></IfaceChardata>"u8,
        Value: Ꮡ(new IfaceChardata(Chardata: ((@string)"hi"u8))),
        UnmarshalOnly: true,
        UnmarshalError: "cannot unmarshal into interface {}"u8
    ),
    new(
        ExpectXML: @"<IfaceChardata><T1></T1><T2></T2></IfaceChardata>"u8,
        Value: Ꮡ(new IfaceChardata(Chardata: ((@string)""u8))),
        UnmarshalError: "cannot unmarshal into interface {}"u8
    ),
    new(
        ExpectXML: @"<IfaceChardata><T1></T1><T2></T2></IfaceChardata>"u8,
        Value: Ꮡ(new IfaceChardata(Chardata: default!)),
        UnmarshalError: "cannot unmarshal into interface {}"u8
    ),
    new(
        ExpectXML: @"<DirectChardata><T1></T1>hi<T2></T2></DirectChardata>"u8,
        Value: Ꮡ(new DirectChardata(Chardata: ((@string)"hi"u8)))
    ),
    new(
        ExpectXML: @"<DirectChardata><T1></T1><![CDATA[hi]]><T2></T2></DirectChardata>"u8,
        Value: Ꮡ(new DirectChardata(Chardata: ((@string)"hi"u8))),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<DirectChardata><T1></T1><T2></T2></DirectChardata>"u8,
        Value: Ꮡ(new DirectChardata(Chardata: ((@string)""u8)))
    ),
    new(
        ExpectXML: @"<IndirCDATA><T1></T1><![CDATA[hi]]><T2></T2></IndirCDATA>"u8,
        Value: Ꮡ(new IndirCDATA(CDATA: stringptr("hi"u8)))
    ),
    new(
        ExpectXML: @"<IndirCDATA><T1></T1>hi<T2></T2></IndirCDATA>"u8,
        Value: Ꮡ(new IndirCDATA(CDATA: stringptr("hi"u8))),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IndirCDATA><T1></T1><T2></T2></IndirCDATA>"u8,
        Value: Ꮡ(new IndirCDATA(CDATA: stringptr(""u8)))
    ),
    new(
        ExpectXML: @"<IndirCDATA><T1></T1><T2></T2></IndirCDATA>"u8,
        Value: Ꮡ(new IndirCDATA(CDATA: nil)),
        MarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceCDATA><T1></T1><![CDATA[hi]]><T2></T2></IfaceCDATA>"u8,
        Value: Ꮡ(new IfaceCDATA(CDATA: ((@string)"hi"u8))),
        UnmarshalError: "cannot unmarshal into interface {}"u8
    ),
    new(
        ExpectXML: @"<IfaceCDATA><T1></T1>hi<T2></T2></IfaceCDATA>"u8,
        Value: Ꮡ(new IfaceCDATA(CDATA: ((@string)"hi"u8))),
        UnmarshalOnly: true,
        UnmarshalError: "cannot unmarshal into interface {}"u8
    ),
    new(
        ExpectXML: @"<IfaceCDATA><T1></T1><T2></T2></IfaceCDATA>"u8,
        Value: Ꮡ(new IfaceCDATA(CDATA: ((@string)""u8))),
        UnmarshalError: "cannot unmarshal into interface {}"u8
    ),
    new(
        ExpectXML: @"<IfaceCDATA><T1></T1><T2></T2></IfaceCDATA>"u8,
        Value: Ꮡ(new IfaceCDATA(CDATA: default!)),
        UnmarshalError: "cannot unmarshal into interface {}"u8
    ),
    new(
        ExpectXML: @"<DirectCDATA><T1></T1><![CDATA[hi]]><T2></T2></DirectCDATA>"u8,
        Value: Ꮡ(new DirectCDATA(CDATA: ((@string)"hi"u8)))
    ),
    new(
        ExpectXML: @"<DirectCDATA><T1></T1>hi<T2></T2></DirectCDATA>"u8,
        Value: Ꮡ(new DirectCDATA(CDATA: ((@string)"hi"u8))),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<DirectCDATA><T1></T1><T2></T2></DirectCDATA>"u8,
        Value: Ꮡ(new DirectCDATA(CDATA: ((@string)""u8)))
    ),
    new(
        ExpectXML: @"<IndirInnerXML><T1></T1><hi/><T2></T2></IndirInnerXML>"u8,
        Value: Ꮡ(new IndirInnerXML(InnerXML: stringptr("<hi/>"u8))),
        MarshalOnly: true
    ),
    new(
        ExpectXML: @"<IndirInnerXML><T1></T1><T2></T2></IndirInnerXML>"u8,
        Value: Ꮡ(new IndirInnerXML(InnerXML: stringptr(""u8))),
        MarshalOnly: true
    ),
    new(
        ExpectXML: @"<IndirInnerXML><T1></T1><T2></T2></IndirInnerXML>"u8,
        Value: Ꮡ(new IndirInnerXML(InnerXML: nil))
    ),
    new(
        ExpectXML: @"<IndirInnerXML><T1></T1><hi/><T2></T2></IndirInnerXML>"u8,
        Value: Ꮡ(new IndirInnerXML(InnerXML: nil)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceInnerXML><T1></T1><hi/><T2></T2></IfaceInnerXML>"u8,
        Value: Ꮡ(new IfaceInnerXML(InnerXML: (@string)"<hi/>"u8)),
        MarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceInnerXML><T1></T1><hi/><T2></T2></IfaceInnerXML>"u8,
        Value: Ꮡ(new IfaceInnerXML(InnerXML: default!)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceInnerXML><T1></T1><T2></T2></IfaceInnerXML>"u8,
        Value: Ꮡ(new IfaceInnerXML(InnerXML: default!))
    ),
    new(
        ExpectXML: @"<IfaceInnerXML><T1></T1><T2></T2></IfaceInnerXML>"u8,
        Value: Ꮡ(new IfaceInnerXML(InnerXML: default!)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<DirectInnerXML><T1></T1><hi/><T2></T2></DirectInnerXML>"u8,
        Value: Ꮡ(new DirectInnerXML(InnerXML: ((@string)"<hi/>"u8))),
        MarshalOnly: true
    ),
    new(
        ExpectXML: @"<DirectInnerXML><T1></T1><hi/><T2></T2></DirectInnerXML>"u8,
        Value: Ꮡ(new DirectInnerXML(InnerXML: ((@string)"<T1></T1><hi/><T2></T2>"u8))),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<DirectInnerXML><T1></T1><T2></T2></DirectInnerXML>"u8,
        Value: Ꮡ(new DirectInnerXML(InnerXML: ((@string)""u8))),
        MarshalOnly: true
    ),
    new(
        ExpectXML: @"<DirectInnerXML><T1></T1><T2></T2></DirectInnerXML>"u8,
        Value: Ꮡ(new DirectInnerXML(InnerXML: ((@string)"<T1></T1><T2></T2>"u8))),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IndirElement><T1></T1><Element>hi</Element><T2></T2></IndirElement>"u8,
        Value: Ꮡ(new IndirElement(Element: stringptr("hi"u8)))
    ),
    new(
        ExpectXML: @"<IndirElement><T1></T1><Element></Element><T2></T2></IndirElement>"u8,
        Value: Ꮡ(new IndirElement(Element: stringptr(""u8)))
    ),
    new(
        ExpectXML: @"<IndirElement><T1></T1><T2></T2></IndirElement>"u8,
        Value: Ꮡ(new IndirElement(Element: nil))
    ),
    new(
        ExpectXML: @"<IfaceElement><T1></T1><Element>hi</Element><T2></T2></IfaceElement>"u8,
        Value: Ꮡ(new IfaceElement(Element: (@string)"hi"u8)),
        MarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceElement><T1></T1><Element>hi</Element><T2></T2></IfaceElement>"u8,
        Value: Ꮡ(new IfaceElement(Element: default!)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceElement><T1></T1><T2></T2></IfaceElement>"u8,
        Value: Ꮡ(new IfaceElement(Element: default!))
    ),
    new(
        ExpectXML: @"<IfaceElement><T1></T1><T2></T2></IfaceElement>"u8,
        Value: Ꮡ(new IfaceElement(Element: default!)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<DirectElement><T1></T1><Element>hi</Element><T2></T2></DirectElement>"u8,
        Value: Ꮡ(new DirectElement(Element: ((@string)"hi"u8)))
    ),
    new(
        ExpectXML: @"<DirectElement><T1></T1><Element></Element><T2></T2></DirectElement>"u8,
        Value: Ꮡ(new DirectElement(Element: ((@string)""u8)))
    ),
    new(
        ExpectXML: @"<IndirOmitEmpty><T1></T1><OmitEmpty>hi</OmitEmpty><T2></T2></IndirOmitEmpty>"u8,
        Value: Ꮡ(new IndirOmitEmpty(OmitEmpty: stringptr("hi"u8)))
    ),
    new(
        ExpectXML: @"<IndirOmitEmpty><T1></T1><OmitEmpty></OmitEmpty><T2></T2></IndirOmitEmpty>"u8,
        Value: Ꮡ(new IndirOmitEmpty(OmitEmpty: stringptr(""u8))),
        MarshalOnly: true
    ),
    new(
        ExpectXML: @"<IndirOmitEmpty><T1></T1><OmitEmpty></OmitEmpty><T2></T2></IndirOmitEmpty>"u8,
        Value: Ꮡ(new IndirOmitEmpty(OmitEmpty: stringptr(""u8))),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IndirOmitEmpty><T1></T1><T2></T2></IndirOmitEmpty>"u8,
        Value: Ꮡ(new IndirOmitEmpty(OmitEmpty: nil))
    ),
    new(
        ExpectXML: @"<IfaceOmitEmpty><T1></T1><OmitEmpty>hi</OmitEmpty><T2></T2></IfaceOmitEmpty>"u8,
        Value: Ꮡ(new IfaceOmitEmpty(OmitEmpty: (@string)"hi"u8)),
        MarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceOmitEmpty><T1></T1><OmitEmpty>hi</OmitEmpty><T2></T2></IfaceOmitEmpty>"u8,
        Value: Ꮡ(new IfaceOmitEmpty(OmitEmpty: default!)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceOmitEmpty><T1></T1><T2></T2></IfaceOmitEmpty>"u8,
        Value: Ꮡ(new IfaceOmitEmpty(OmitEmpty: default!))
    ),
    new(
        ExpectXML: @"<IfaceOmitEmpty><T1></T1><T2></T2></IfaceOmitEmpty>"u8,
        Value: Ꮡ(new IfaceOmitEmpty(OmitEmpty: default!)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<DirectOmitEmpty><T1></T1><OmitEmpty>hi</OmitEmpty><T2></T2></DirectOmitEmpty>"u8,
        Value: Ꮡ(new DirectOmitEmpty(OmitEmpty: ((@string)"hi"u8)))
    ),
    new(
        ExpectXML: @"<DirectOmitEmpty><T1></T1><T2></T2></DirectOmitEmpty>"u8,
        Value: Ꮡ(new DirectOmitEmpty(OmitEmpty: ((@string)""u8)))
    ),
    new(
        ExpectXML: @"<IndirAny><T1></T1><Any>hi</Any><T2></T2></IndirAny>"u8,
        Value: Ꮡ(new IndirAny(Any: stringptr("hi"u8)))
    ),
    new(
        ExpectXML: @"<IndirAny><T1></T1><Any></Any><T2></T2></IndirAny>"u8,
        Value: Ꮡ(new IndirAny(Any: stringptr(""u8)))
    ),
    new(
        ExpectXML: @"<IndirAny><T1></T1><T2></T2></IndirAny>"u8,
        Value: Ꮡ(new IndirAny(Any: nil))
    ),
    new(
        ExpectXML: @"<IfaceAny><T1></T1><Any>hi</Any><T2></T2></IfaceAny>"u8,
        Value: Ꮡ(new IfaceAny(Any: (@string)"hi"u8)),
        MarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceAny><T1></T1><Any>hi</Any><T2></T2></IfaceAny>"u8,
        Value: Ꮡ(new IfaceAny(Any: default!)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceAny><T1></T1><T2></T2></IfaceAny>"u8,
        Value: Ꮡ(new IfaceAny(Any: default!))
    ),
    new(
        ExpectXML: @"<IfaceAny><T1></T1><T2></T2></IfaceAny>"u8,
        Value: Ꮡ(new IfaceAny(Any: default!)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<DirectAny><T1></T1><Any>hi</Any><T2></T2></DirectAny>"u8,
        Value: Ꮡ(new DirectAny(Any: ((@string)"hi"u8)))
    ),
    new(
        ExpectXML: @"<DirectAny><T1></T1><Any></Any><T2></T2></DirectAny>"u8,
        Value: Ꮡ(new DirectAny(Any: ((@string)""u8)))
    ),
    new(
        ExpectXML: @"<IndirFoo><T1></T1><Foo>hi</Foo><T2></T2></IndirFoo>"u8,
        Value: Ꮡ(new IndirAny(Any: stringptr("hi"u8))),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IndirFoo><T1></T1><Foo></Foo><T2></T2></IndirFoo>"u8,
        Value: Ꮡ(new IndirAny(Any: stringptr(""u8))),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IndirFoo><T1></T1><T2></T2></IndirFoo>"u8,
        Value: Ꮡ(new IndirAny(Any: nil)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceFoo><T1></T1><Foo>hi</Foo><T2></T2></IfaceFoo>"u8,
        Value: Ꮡ(new IfaceAny(Any: default!)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceFoo><T1></T1><T2></T2></IfaceFoo>"u8,
        Value: Ꮡ(new IfaceAny(Any: default!)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<IfaceFoo><T1></T1><T2></T2></IfaceFoo>"u8,
        Value: Ꮡ(new IfaceAny(Any: default!)),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<DirectFoo><T1></T1><Foo>hi</Foo><T2></T2></DirectFoo>"u8,
        Value: Ꮡ(new DirectAny(Any: ((@string)"hi"u8))),
        UnmarshalOnly: true
    ),
    new(
        ExpectXML: @"<DirectFoo><T1></T1><Foo></Foo><T2></T2></DirectFoo>"u8,
        Value: Ꮡ(new DirectAny(Any: ((@string)""u8))),
        UnmarshalOnly: true
    )
}.slice(); }

public static void TestMarshal(ж<testing.T> Ꮡt) {
    foreach (var (idx, vᴛ1) in marshalTests) {
        ref var test = ref heap(new marshalTestsᴛ1(), out var Ꮡtest);
        test = vᴛ1;

        if (test.UnmarshalOnly) {
            continue;
        }
        var testʗ1 = test;
        Ꮡt.Run(fmt.Sprintf("%d"u8, idx), (ж<testing.T> tΔ1) => {
            var (data, err) = Marshal(testʗ1.Value);
            if (err != default!) {
                if (testʗ1.MarshalError == ""u8) {
                    tΔ1.Errorf("marshal(%#v): %s"u8, testʗ1.Value, err);
                    return;
                }
                if (!strings.Contains(err.Error(), testʗ1.MarshalError)) {
                    tΔ1.Errorf("marshal(%#v): %s, want %q"u8, testʗ1.Value, err, testʗ1.MarshalError);
                }
                return;
            }
            if (testʗ1.MarshalError != ""u8) {
                tΔ1.Errorf("Marshal succeeded, want error %q"u8, testʗ1.MarshalError);
                return;
            }
            {
                @string got = ((@string)data);
                @string want = testʗ1.ExpectXML; if (got != want) {
                    if (strings.Contains(want, "\n"u8)){
                        tΔ1.Errorf("marshal(%#v):\nHAVE:\n%s\nWANT:\n%s"u8, testʗ1.Value, got, want);
                    } else {
                        tΔ1.Errorf("marshal(%#v):\nhave %#q\nwant %#q"u8, testʗ1.Value, got, want);
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

// Reject parent chain with attr, never worked; see golang.org/issue/5033.

partial struct marshalErrorTestsᴛ1 /*dyn*/ {
    public any Value;
    public @string Err;
    public reflectꓸKind Kind;
}
internal static slice<marshalErrorTestsᴛ1> marshalErrorTests = new marshalErrorTestsᴛ1[]{
    new(
        Value: new channel<bool>(0),
        Err: "xml: unsupported type: chan bool"u8,
        Kind: reflect.Chan
    ),
    new(
        Value: new map<@string, @string>{
            ["question"u8] = "What do you get when you multiply six by nine?"u8,
            ["answer"u8] = "42"u8
        },
        Err: "xml: unsupported type: map[string]string"u8,
        Kind: reflect.Map
    ),
    new(
        Value: new map<ж<Ship>, bool>{[default!] = false},
        Err: "xml: unsupported type: map[*xml.Ship]bool"u8,
        Kind: reflect.Map
    ),
    new(
        Value: Ꮡ(new Domain(Comment: slice<byte>("f--bar"u8))),
        Err: @"xml: comments must not contain ""--"""u8
    ),
    new(
        Value: Ꮡ(new AttrParent(nil)),
        Err: @"xml: X>Y chain not valid with attr flag"u8
    ),
    new(
        Value: new BadAttr(new map<@string, @string>{["X"u8] = "Y"u8}),
        Err: @"xml: unsupported type: map[string]string"u8
    )
}.slice();


partial struct marshalIndentTestsᴛ1 /*dyn*/ {
    public any Value;
    public @string Prefix;
    public @string Indent;
    public @string ExpectXML;
}
internal static slice<marshalIndentTestsᴛ1> marshalIndentTests = new marshalIndentTestsᴛ1[]{
    new(
        Value: Ꮡ(new SecretAgent(
            Handle: "007"u8,
            Identity: "James Bond"u8,
            Obfuscate: "<redacted/>"u8
        )),
        Prefix: ""u8,
        Indent: "\t"u8,
        ExpectXML: "<agent handle=\"007\">\n\t<Identity>James Bond</Identity><redacted/>\n</agent>"u8
    )
}.slice();

public static void TestMarshalErrors(ж<testing.T> Ꮡt) {
    foreach (var (idx, test) in marshalErrorTests) {
        var (data, err) = Marshal(test.Value);
        if (err == default!) {
            Ꮡt.Errorf("#%d: marshal(%#v) = [success] %q, want error %v"u8, idx, test.Value, data, test.Err);
            continue;
        }
        if (err.Error() != test.Err) {
            Ꮡt.Errorf("#%d: marshal(%#v) = [error] %v, want %v"u8, idx, test.Value, err, test.Err);
        }
        if (test.Kind != reflect.Invalid) {
            {
                reflectꓸKind kind = (~err._<ж<global::go.encoding.xml_package.UnsupportedTypeError>>()).Type.Kind(); if (kind != test.Kind) {
                    Ꮡt.Errorf("#%d: marshal(%#v) = [error kind] %s, want %s"u8, idx, test.Value, kind, test.Kind);
                }
            }
        }
    }
}

// Do invertibility testing on the various structures that we test
public static void TestUnmarshal(ж<testing.T> Ꮡt) {
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
        Ꮡt.Run(fmt.Sprintf("%d"u8, i), (ж<testing.T> tΔ1) => {
            switch (destʗ1.type()) {
            case ж<Feed> fix: {
                fix.Value.Author.InnerXML = ""u8;
                foreach (var (iΔ1, _) in (~fix).Entry) {
                    (~fix).Entry[iΔ1].Author.InnerXML = ""u8;
                }
                break;
            }}
            if (errʗ1 != default!) {
                if (testʗ1.UnmarshalError == ""u8) {
                    tΔ1.Errorf("unmarshal(%#v): %s"u8, testʗ1.ExpectXML, errʗ1);
                    return;
                }
                if (!strings.Contains(errʗ1.Error(), testʗ1.UnmarshalError)) {
                    tΔ1.Errorf("unmarshal(%#v): %s, want %q"u8, testʗ1.ExpectXML, errʗ1, testʗ1.UnmarshalError);
                }
                return;
            }
            {
                var (got, want) = (destʗ1, testʗ1.Value); if (!reflect.DeepEqual(got, want)) {
                    tΔ1.Errorf("unmarshal(%q):\nhave %#v\nwant %#v"u8, testʗ1.ExpectXML, got, want);
                }
            }
        });
    }
}

public static void TestMarshalIndent(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in marshalIndentTests) {
        var (data, err) = MarshalIndent(test.Value, test.Prefix, test.Indent);
        if (err != default!) {
            Ꮡt.Errorf("#%d: Error: %s"u8, i, err);
            continue;
        }
        {
            @string got = ((@string)data);
            @string want = test.ExpectXML; if (got != want) {
                Ꮡt.Errorf("#%d: MarshalIndent:\nGot:%s\nWant:\n%s"u8, i, got, want);
            }
        }
    }
}

internal partial struct limitedBytesWriter {
    internal io.Writer w;
    internal nint remain; // until writes fail
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string writeLimitHitˢ = "write limit hit"u8;

internal static (nint n, error err) Write(this ref limitedBytesWriter lw, slice<byte> p) {
    nint n = default!;
    error err = default!;

    if (lw.remain <= 0) {
        println((@string)"error"u8);
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

public static void TestMarshalWriteErrors(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    const nint writeCap = 1024;
    var w = Ꮡ(new limitedBytesWriter(new xml_test_package.bytes_BufferжWriter(Ꮡbuf), writeCap));
    var enc = NewEncoder(new xml_internal_test_package.limitedBytesWriterжWriter(w));
    error err = default!;
    nint i = default!;
    const nint n = 4000;
    for (i = 1; i <= n; i++) {
        err = enc.Encode(Ꮡ(new Passenger(
            Name: new @string[]{"Alice"u8, "Bob"u8}.slice(),
            Weight: 5F
        )));
        if (err != default!) {
            break;
        }
    }
    if (err == default!) {
        Ꮡt.Error(expectedAnErrorˢ);
    }
    if (i == n) {
        Ꮡt.Errorf("expected to fail before the end"u8);
    }
    if (buf.Len() != writeCap) {
        Ꮡt.Errorf("buf.Len() = %d; want %d"u8, buf.Len(), (nint)(writeCap));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unwritableˢ = "unwritable"u8;

public static void TestMarshalWriteIOErrors(ж<testing.T> Ꮡt) {
    var enc = NewEncoder(new errWriter(nil));
    @string expectErr = unwritableˢ;
    var err = enc.Encode(Ꮡ(new Passenger(nil)));
    if (err == default! || err.Error() != expectErr) {
        Ꮡt.Errorf("EscapeTest = [error] %v, want %v"u8, err, expectErr);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object helloWorldˢ = (@string)"hello world"u8;

public static void TestMarshalFlush(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    var enc = NewEncoder(new xml_test_package.strings_BuilderжWriter(Ꮡbuf));
    {
        var err = enc.EncodeToken(((global::go.encoding.xml_package.CharData)slice<byte>((@string)"hello world"u8))); if (err != default!) {
            Ꮡt.Fatalf("enc.EncodeToken: %v"u8, err);
        }
    }
    if (buf.Len() > 0) {
        Ꮡt.Fatalf("enc.EncodeToken caused actual write: %q"u8, buf.String());
    }
    {
        var err = enc.Flush(); if (err != default!) {
            Ꮡt.Fatalf("enc.Flush: %v"u8, err);
        }
    }
    if (buf.String() != "hello world"u8) {
        Ꮡt.Fatalf("after enc.Flush, buf.String() = %q, want %q"u8, buf.String(), helloWorldˢ);
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
            Unmarshal(xmlʗ1, Ꮡ(new Feed(nil)));
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
public static void TestStructPointerMarshal(ж<testing.T> Ꮡt) {
    var a = @new<TestStructPointerMarshal_A>();
    a.Value.B = append((~a).B, (any)(Ꮡ(new TestStructPointerMarshal_C(
        XMLName: new Name(Local: "c"u8),
        Value: "x"u8
    ))));
    var (b, err) = Marshal(a.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        @string x = ((@string)b); if (x != "<a><c><value>x</value></c></a>"u8) {
            Ꮡt.Fatal(x);
        }
    }
    ref var v = ref heap(new TestStructPointerMarshal_A(), out var Ꮡv);
    err = Unmarshal(b, Ꮡv);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
}


partial struct encodeTokenTestsᴛ1 /*dyn*/ {
    internal @string desc;
    internal slice<ΔToken> toks;
    internal @string want;
    internal @string err;
}
internal static slice<encodeTokenTestsᴛ1> encodeTokenTests;
internal static void initᴛencodeTokenTests() { encodeTokenTests = new encodeTokenTestsᴛ1[]{new(
    desc: "start element with name space"u8,
    toks: new ΔToken[]{
        new StartElement(new Name("space"u8, "local"u8), default!)
    }.slice(),
    want: @"<local xmlns=""space"">"u8
), new(
    desc: "start element with no name"u8,
    toks: new ΔToken[]{
        new StartElement(new Name("space"u8, ""u8), default!)
    }.slice(),
    err: "xml: start tag with no name"u8
), new(
    desc: "end element with no name"u8,
    toks: new ΔToken[]{
        new EndElement(new Name("space"u8, ""u8))
    }.slice(),
    err: "xml: end tag with no name"u8
), new(
    desc: "char data"u8,
    toks: new ΔToken[]{
        ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"foo"u8))
    }.slice(),
    want: @"foo"u8
), new(
    desc: "char data with escaped chars"u8,
    toks: new ΔToken[]{
        ((global::go.encoding.xml_package.CharData)slice<byte>((@string)" \t\n"u8))
    }.slice(),
    want: " &#x9;\n"u8
), new(
    desc: "comment"u8,
    toks: new ΔToken[]{
        ((global::go.encoding.xml_package.Comment)slice<byte>((@string)"foo"u8))
    }.slice(),
    want: @"<!--foo-->"u8
), new(
    desc: "comment with invalid content"u8,
    toks: new ΔToken[]{
        ((global::go.encoding.xml_package.Comment)slice<byte>((@string)"foo-->"u8))
    }.slice(),
    err: "xml: EncodeToken of Comment containing --> marker"u8
), new(
    desc: "proc instruction"u8,
    toks: new ΔToken[]{
        new ProcInst("Target"u8, slice<byte>("Instruction"u8))
    }.slice(),
    want: @"<?Target Instruction?>"u8
), new(
    desc: "proc instruction with empty target"u8,
    toks: new ΔToken[]{
        new ProcInst(""u8, slice<byte>("Instruction"u8))
    }.slice(),
    err: "xml: EncodeToken of ProcInst with invalid Target"u8
), new(
    desc: "proc instruction with bad content"u8,
    toks: new ΔToken[]{
        new ProcInst(""u8, slice<byte>("Instruction?>"u8))
    }.slice(),
    err: "xml: EncodeToken of ProcInst with invalid Target"u8
), new(
    desc: "directive"u8,
    toks: new ΔToken[]{
        ((global::go.encoding.xml_package.Directive)slice<byte>((@string)"foo"u8))
    }.slice(),
    want: @"<!foo>"u8
), new(
    desc: "more complex directive"u8,
    toks: new ΔToken[]{
        ((global::go.encoding.xml_package.Directive)slice<byte>((@string)"DOCTYPE doc [ <!ELEMENT doc '>'> <!-- com>ment --> ]"u8))
    }.slice(),
    want: @"<!DOCTYPE doc [ <!ELEMENT doc '>'> <!-- com>ment --> ]>"u8
), new(
    desc: "directive instruction with bad name"u8,
    toks: new ΔToken[]{
        ((global::go.encoding.xml_package.Directive)slice<byte>((@string)"foo>"u8))
    }.slice(),
    err: "xml: EncodeToken of Directive containing wrong < or > markers"u8
), new(
    desc: "end tag without start tag"u8,
    toks: new ΔToken[]{
        new EndElement(new Name("foo"u8, "bar"u8))
    }.slice(),
    err: "xml: end tag </bar> without start tag"u8
), new(
    desc: "mismatching end tag local name"u8,
    toks: new ΔToken[]{
        new StartElement(new Name(""u8, "foo"u8), default!),
        new EndElement(new Name(""u8, "bar"u8))
    }.slice(),
    err: "xml: end tag </bar> does not match start tag <foo>"u8,
    want: @"<foo>"u8
), new(
    desc: "mismatching end tag namespace"u8,
    toks: new ΔToken[]{
        new StartElement(new Name("space"u8, "foo"u8), default!),
        new EndElement(new Name("another"u8, "foo"u8))
    }.slice(),
    err: "xml: end tag </foo> in namespace another does not match start tag <foo> in namespace space"u8,
    want: @"<foo xmlns=""space"">"u8
), new(
    desc: "start element with explicit namespace"u8,
    toks: new ΔToken[]{
        new StartElement(new Name("space"u8, "local"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name("xmlns"u8, "x"u8), "space"u8),
            new(new Name("space"u8, "foo"u8), "value"u8)
        }.slice()
        )
    }.slice(),
    want: @"<local xmlns=""space"" xmlns:_xmlns=""xmlns"" _xmlns:x=""space"" xmlns:space=""space"" space:foo=""value"">"u8
), new(
    desc: "start element with explicit namespace and colliding prefix"u8,
    toks: new ΔToken[]{
        new StartElement(new Name("space"u8, "local"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name("xmlns"u8, "x"u8), "space"u8),
            new(new Name("space"u8, "foo"u8), "value"u8),
            new(new Name("x"u8, "bar"u8), "other"u8)
        }.slice()
        )
    }.slice(),
    want: @"<local xmlns=""space"" xmlns:_xmlns=""xmlns"" _xmlns:x=""space"" xmlns:space=""space"" space:foo=""value"" xmlns:x=""x"" x:bar=""other"">"u8
), new(
    desc: "start element using previously defined namespace"u8,
    toks: new ΔToken[]{
        new StartElement(new Name(""u8, "local"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name("xmlns"u8, "x"u8), "space"u8)
        }.slice()
        ),
        new StartElement(new Name("space"u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name("space"u8, "x"u8), "y"u8)
        }.slice()
        )
    }.slice(),
    want: @"<local xmlns:_xmlns=""xmlns"" _xmlns:x=""space""><foo xmlns=""space"" xmlns:space=""space"" space:x=""y"">"u8
), new(
    desc: "nested name space with same prefix"u8,
    toks: new ΔToken[]{
        new StartElement(new Name(""u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name("xmlns"u8, "x"u8), "space1"u8)
        }.slice()
        ),
        new StartElement(new Name(""u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name("xmlns"u8, "x"u8), "space2"u8)
        }.slice()
        ),
        new StartElement(new Name(""u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name("space1"u8, "a"u8), "space1 value"u8),
            new(new Name("space2"u8, "b"u8), "space2 value"u8)
        }.slice()
        ),
        new EndElement(new Name(""u8, "foo"u8)),
        new EndElement(new Name(""u8, "foo"u8)),
        new StartElement(new Name(""u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name("space1"u8, "a"u8), "space1 value"u8),
            new(new Name("space2"u8, "b"u8), "space2 value"u8)
        }.slice()
        )
    }.slice(),
    want: @"<foo xmlns:_xmlns=""xmlns"" _xmlns:x=""space1""><foo _xmlns:x=""space2""><foo xmlns:space1=""space1"" space1:a=""space1 value"" xmlns:space2=""space2"" space2:b=""space2 value""></foo></foo><foo xmlns:space1=""space1"" space1:a=""space1 value"" xmlns:space2=""space2"" space2:b=""space2 value"">"u8
), new(
    desc: "start element defining several prefixes for the same name space"u8,
    toks: new ΔToken[]{
        new StartElement(new Name("space"u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name("xmlns"u8, "a"u8), "space"u8),
            new(new Name("xmlns"u8, "b"u8), "space"u8),
            new(new Name("space"u8, "x"u8), "value"u8)
        }.slice()
        )
    }.slice(),
    want: @"<foo xmlns=""space"" xmlns:_xmlns=""xmlns"" _xmlns:a=""space"" _xmlns:b=""space"" xmlns:space=""space"" space:x=""value"">"u8
), new(
    desc: "nested element redefines name space"u8,
    toks: new ΔToken[]{
        new StartElement(new Name(""u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name("xmlns"u8, "x"u8), "space"u8)
        }.slice()
        ),
        new StartElement(new Name("space"u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name("xmlns"u8, "y"u8), "space"u8),
            new(new Name("space"u8, "a"u8), "value"u8)
        }.slice()
        )
    }.slice(),
    want: @"<foo xmlns:_xmlns=""xmlns"" _xmlns:x=""space""><foo xmlns=""space"" _xmlns:y=""space"" xmlns:space=""space"" space:a=""value"">"u8
), new(
    desc: "nested element creates alias for default name space"u8,
    toks: new ΔToken[]{
        new StartElement(new Name("space"u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name(""u8, "xmlns"u8), "space"u8)
        }.slice()
        ),
        new StartElement(new Name("space"u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name("xmlns"u8, "y"u8), "space"u8),
            new(new Name("space"u8, "a"u8), "value"u8)
        }.slice()
        )
    }.slice(),
    want: @"<foo xmlns=""space"" xmlns=""space""><foo xmlns=""space"" xmlns:_xmlns=""xmlns"" _xmlns:y=""space"" xmlns:space=""space"" space:a=""value"">"u8
), new(
    desc: "nested element defines default name space with existing prefix"u8,
    toks: new ΔToken[]{
        new StartElement(new Name(""u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name("xmlns"u8, "x"u8), "space"u8)
        }.slice()
        ),
        new StartElement(new Name("space"u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name(""u8, "xmlns"u8), "space"u8),
            new(new Name("space"u8, "a"u8), "value"u8)
        }.slice()
        )
    }.slice(),
    want: @"<foo xmlns:_xmlns=""xmlns"" _xmlns:x=""space""><foo xmlns=""space"" xmlns=""space"" xmlns:space=""space"" space:a=""value"">"u8
), new(
    desc: "nested element uses empty attribute name space when default ns defined"u8,
    toks: new ΔToken[]{
        new StartElement(new Name("space"u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name(""u8, "xmlns"u8), "space"u8)
        }.slice()
        ),
        new StartElement(new Name("space"u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name(""u8, "attr"u8), "value"u8)
        }.slice()
        )
    }.slice(),
    want: @"<foo xmlns=""space"" xmlns=""space""><foo xmlns=""space"" attr=""value"">"u8
), new(
    desc: "redefine xmlns"u8,
    toks: new ΔToken[]{
        new StartElement(new Name(""u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name("foo"u8, "xmlns"u8), "space"u8)
        }.slice()
        )
    }.slice(),
    want: @"<foo xmlns:foo=""foo"" foo:xmlns=""space"">"u8
), new(
    desc: "xmlns with explicit name space #1"u8,
    toks: new ΔToken[]{
        new StartElement(new Name("space"u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name("xml"u8, "xmlns"u8), "space"u8)
        }.slice()
        )
    }.slice(),
    want: @"<foo xmlns=""space"" xmlns:_xml=""xml"" _xml:xmlns=""space"">"u8
), new(
    desc: "xmlns with explicit name space #2"u8,
    toks: new ΔToken[]{
        new StartElement(new Name("space"u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name(xmlURL, "xmlns"u8), "space"u8)
        }.slice()
        )
    }.slice(),
    want: @"<foo xmlns=""space"" xml:xmlns=""space"">"u8
), new(
    desc: "empty name space declaration is ignored"u8,
    toks: new ΔToken[]{
        new StartElement(new Name(""u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name("xmlns"u8, "foo"u8), ""u8)
        }.slice()
        )
    }.slice(),
    want: @"<foo xmlns:_xmlns=""xmlns"" _xmlns:foo="""">"u8
), new(
    desc: "attribute with no name is ignored"u8,
    toks: new ΔToken[]{
        new StartElement(new Name(""u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name(""u8, ""u8), "value"u8)
        }.slice()
        )
    }.slice(),
    want: @"<foo>"u8
), new(
    desc: "namespace URL with non-valid name"u8,
    toks: new ΔToken[]{
        new StartElement(new Name("/34"u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name("/34"u8, "x"u8), "value"u8)
        }.slice()
        )
    }.slice(),
    want: @"<foo xmlns=""/34"" xmlns:_=""/34"" _:x=""value"">"u8
), new(
    desc: "nested element resets default namespace to empty"u8,
    toks: new ΔToken[]{
        new StartElement(new Name("space"u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name(""u8, "xmlns"u8), "space"u8)
        }.slice()
        ),
        new StartElement(new Name(""u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name(""u8, "xmlns"u8), ""u8),
            new(new Name(""u8, "x"u8), "value"u8),
            new(new Name("space"u8, "x"u8), "value"u8)
        }.slice()
        )
    }.slice(),
    want: @"<foo xmlns=""space"" xmlns=""space""><foo xmlns="""" x=""value"" xmlns:space=""space"" space:x=""value"">"u8
), new(
    desc: "nested element requires empty default name space"u8,
    toks: new ΔToken[]{
        new StartElement(new Name("space"u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name(""u8, "xmlns"u8), "space"u8)
        }.slice()
        ),
        new StartElement(new Name(""u8, "foo"u8), default!)
    }.slice(),
    want: @"<foo xmlns=""space"" xmlns=""space""><foo>"u8
), new(
    desc: "attribute uses name space from xmlns"u8,
    toks: new ΔToken[]{
        new StartElement(new Name("some/space"u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name(""u8, "attr"u8), "value"u8),
            new(new Name("some/space"u8, "other"u8), "other value"u8)
        }.slice()
        )
    }.slice(),
    want: @"<foo xmlns=""some/space"" attr=""value"" xmlns:space=""some/space"" space:other=""other value"">"u8
), new(
    desc: "default name space should not be used by attributes"u8,
    toks: new ΔToken[]{
        new StartElement(new Name("space"u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name(""u8, "xmlns"u8), "space"u8),
            new(new Name("xmlns"u8, "bar"u8), "space"u8),
            new(new Name("space"u8, "baz"u8), "foo"u8)
        }.slice()
        ),
        new StartElement(new Name("space"u8, "baz"u8), default!),
        new EndElement(new Name("space"u8, "baz"u8)),
        new EndElement(new Name("space"u8, "foo"u8))
    }.slice(),
    want: @"<foo xmlns=""space"" xmlns=""space"" xmlns:_xmlns=""xmlns"" _xmlns:bar=""space"" xmlns:space=""space"" space:baz=""foo""><baz xmlns=""space""></baz></foo>"u8
), new(
    desc: "default name space not used by attributes, not explicitly defined"u8,
    toks: new ΔToken[]{
        new StartElement(new Name("space"u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name(""u8, "xmlns"u8), "space"u8),
            new(new Name("space"u8, "baz"u8), "foo"u8)
        }.slice()
        ),
        new StartElement(new Name("space"u8, "baz"u8), default!),
        new EndElement(new Name("space"u8, "baz"u8)),
        new EndElement(new Name("space"u8, "foo"u8))
    }.slice(),
    want: @"<foo xmlns=""space"" xmlns=""space"" xmlns:space=""space"" space:baz=""foo""><baz xmlns=""space""></baz></foo>"u8
), new(
    desc: "impossible xmlns declaration"u8,
    toks: new ΔToken[]{
        new StartElement(new Name(""u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name(""u8, "xmlns"u8), "space"u8)
        }.slice()
        ),
        new StartElement(new Name("space"u8, "bar"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name("space"u8, "attr"u8), "value"u8)
        }.slice()
        )
    }.slice(),
    want: @"<foo xmlns=""space""><bar xmlns=""space"" xmlns:space=""space"" space:attr=""value"">"u8
), new(
    desc: "reserved namespace prefix -- all lower case"u8,
    toks: new ΔToken[]{
        new StartElement(new Name(""u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name("http://www.w3.org/2001/xmlSchema-instance"u8, "nil"u8), "true"u8)
        }.slice()
        )
    }.slice(),
    want: @"<foo xmlns:_xmlSchema-instance=""http://www.w3.org/2001/xmlSchema-instance"" _xmlSchema-instance:nil=""true"">"u8
), new(
    desc: "reserved namespace prefix -- all upper case"u8,
    toks: new ΔToken[]{
        new StartElement(new Name(""u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name("http://www.w3.org/2001/XMLSchema-instance"u8, "nil"u8), "true"u8)
        }.slice()
        )
    }.slice(),
    want: @"<foo xmlns:_XMLSchema-instance=""http://www.w3.org/2001/XMLSchema-instance"" _XMLSchema-instance:nil=""true"">"u8
), new(
    desc: "reserved namespace prefix -- all mixed case"u8,
    toks: new ΔToken[]{
        new StartElement(new Name(""u8, "foo"u8), new global::go.encoding.xml_package.Attr[]{
            new(new Name("http://www.w3.org/2001/XmLSchema-instance"u8, "nil"u8), "true"u8)
        }.slice()
        )
    }.slice(),
    want: @"<foo xmlns:_XmLSchema-instance=""http://www.w3.org/2001/XmLSchema-instance"" _XmLSchema-instance:nil=""true"">"u8
)
}.slice(); }

public static void TestEncodeToken(ж<testing.T> Ꮡt) {
loop:
    foreach (var (i, vᴛ1) in encodeTokenTests) {
        ref var tt = ref heap(new encodeTokenTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        var enc = NewEncoder(new xml_test_package.strings_BuilderжWriter(Ꮡbuf));
        error err = default!;
        foreach (var (j, tok) in tt.toks) {
            err = enc.EncodeToken(tok);
            if (err != default! && j < len(tt.toks) - 1) {
                Ꮡt.Errorf("#%d %s token #%d: %v"u8, i, tt.desc, j, err);
                goto continue_loop;
            }
        }
        var ttʗ1 = tt;
        void errorf(@string f, params ꓸꓸꓸany aʗp) {
            var a = aʗp.sslice();
            Ꮡt.Errorf("#%d %s token #%d:%s"u8, i, ttʗ1.desc, len(ttʗ1.toks) - 1, fmt.Sprintf(f, a.ꓸꓸꓸ));
        }
        switch (ᐧ) {
        case {} when tt.err != ""u8 && err == default!: {
            errorf(" expected error; got none"u8);
            continue;
            break;
        }
        case {} when tt.err == ""u8 && err != default!: {
            errorf(" got error: %v"u8, err);
            continue;
            break;
        }
        case {} when tt.err != ""u8 && err != default! && tt.err != err.Error(): {
            errorf(" error mismatch; got %v, want %v"u8, err, tt.err);
            continue;
            break;
        }}

        {
            var errΔ1 = enc.Flush(); if (errΔ1 != default!) {
                errorf(" %v"u8, errΔ1);
                continue;
            }
        }
        {
            @string got = buf.String(); if (got != tt.want) {
                errorf("\ngot  %v\nwant %v"u8, got, tt.want);
                continue;
            }
        }
continue_loop:;
    }
break_loop:;
}

public static void TestProcInstEncodeToken(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var enc = NewEncoder(new xml_test_package.bytes_BufferжWriter(Ꮡbuf));
    {
        var err = enc.EncodeToken(new ProcInst("xml"u8, slice<byte>("Instruction"u8))); if (err != default!) {
            Ꮡt.Fatalf("enc.EncodeToken: expected to be able to encode xml target ProcInst as first token, %s"u8, err);
        }
    }
    {
        var err = enc.EncodeToken(new ProcInst("Target"u8, slice<byte>("Instruction"u8))); if (err != default!) {
            Ꮡt.Fatalf("enc.EncodeToken: expected to be able to add non-xml target ProcInst"u8);
        }
    }
    {
        var err = enc.EncodeToken(new ProcInst("xml"u8, slice<byte>("Instruction"u8))); if (err == default!) {
            Ꮡt.Fatalf("enc.EncodeToken: expected to not be allowed to encode xml target ProcInst when not first token"u8);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xmlVersion10EncodingUtf8ˢ = """
<?xml version="1.0" encoding="UTF-8"?>
<?Target Instruction?>
<root>
</root>

"""u8;

public static void TestDecodeEncode(ж<testing.T> Ꮡt) {
    ref var @in = ref heap(new bytes.Buffer(), out var Ꮡin);
    ref var @out = ref heap(new bytes.Buffer(), out var Ꮡout);
    @in.WriteString(xmlVersion10EncodingUtf8ˢ);
    var dec = NewDecoder(new xml_test_package.bytes_BufferжReader(Ꮡin));
    var enc = NewEncoder(new xml_test_package.bytes_BufferжWriter(Ꮡout));
    for (var (tok, err) = dec.Token(); err == default!; (tok, err) = dec.Token()) {
        err = enc.EncodeToken(tok);
        if (err != default!) {
            Ꮡt.Fatalf("enc.EncodeToken: Unable to encode token (%#v), %v"u8, tok, err);
        }
    }
}

internal partial struct TestRace9796_A /*dyn*/ {
}

internal partial struct TestRace9796_B /*dyn*/ {
    public slice<TestRace9796_A> C; /*`xml:"X>Y"`*/
}

// Issue 9796. Used to fail with GORACE="halt_on_error=1" -race.
public static partial void TestRace9796(ж<testing.T> Ꮡt) {
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    for (nint i = 0; i < 2; i++) {
        Ꮡwg.Add(1);
        goǃ(() => {
            Marshal(new TestRace9796_B(new TestRace9796_A[]{new()}.slice()));
            Ꮡwg.Done();
        });
    }
    Ꮡwg.Wait();
}

public static void TestIsValidDirective(ж<testing.T> Ꮡt) {
    var testOK = new @string[]{
        "<>"u8,
        "< < > >"u8,
        "<!DOCTYPE '<' '>' '>' <!--nothing-->>"u8,
        "<!DOCTYPE doc [ <!ELEMENT doc ANY> <!ELEMENT doc ANY> ]>"u8,
        "<!DOCTYPE doc [ <!ELEMENT doc \"ANY> '<' <!E\" LEMENT '>' doc ANY> ]>"u8,
        "<!DOCTYPE doc <!-- just>>>> a < comment --> [ <!ITEM anything> ] >"u8
    }.slice();
    var testKO = new @string[]{
        "<"u8,
        ">"u8,
        "<!--"u8,
        "-->"u8,
        "< > > < < >"u8,
        "<!dummy <!-- > -->"u8,
        "<!DOCTYPE doc '>"u8,
        "<!DOCTYPE doc '>'"u8,
        "<!DOCTYPE doc <!--comment>"u8
    }.slice();
    foreach (var (_, s) in testOK) {
        if (!isValidDirective(((global::go.encoding.xml_package.Directive)slice<byte>(s)))) {
            Ꮡt.Errorf("Directive %q is expected to be valid"u8, s);
        }
    }
    foreach (var (_, s) in testKO) {
        if (isValidDirective(((global::go.encoding.xml_package.Directive)slice<byte>(s)))) {
            Ꮡt.Errorf("Directive %q is expected to be invalid"u8, s);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string object2Object2ˢ = "<object2></object2>"u8;

// Issue 11719. EncodeToken used to silently eat tokens with an invalid type.
public static void TestSimpleUseOfEncodeToken(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    var enc = NewEncoder(new xml_test_package.strings_BuilderжWriter(Ꮡbuf));
    {
        var err = enc.EncodeToken(Ꮡ(new StartElement(Name: new Name(""u8, "object1"u8)))); if (err == default!) {
            Ꮡt.Errorf("enc.EncodeToken: pointer type should be rejected"u8);
        }
    }
    {
        var err = enc.EncodeToken(Ꮡ(new EndElement(Name: new Name(""u8, "object1"u8)))); if (err == default!) {
            Ꮡt.Errorf("enc.EncodeToken: pointer type should be rejected"u8);
        }
    }
    {
        var err = enc.EncodeToken(new StartElement(Name: new Name(""u8, "object2"u8))); if (err != default!) {
            Ꮡt.Errorf("enc.EncodeToken: StartElement %s"u8, err);
        }
    }
    {
        var err = enc.EncodeToken(new EndElement(Name: new Name(""u8, "object2"u8))); if (err != default!) {
            Ꮡt.Errorf("enc.EncodeToken: EndElement %s"u8, err);
        }
    }
    {
        var err = enc.EncodeToken(new Universe(nil)); if (err == default!) {
            Ꮡt.Errorf("enc.EncodeToken: invalid type not caught"u8);
        }
    }
    {
        var err = enc.Flush(); if (err != default!) {
            Ꮡt.Errorf("enc.Flush: %s"u8, err);
        }
    }
    if (buf.Len() == 0) {
        Ꮡt.Errorf("enc.EncodeToken: empty buffer"u8);
    }
    @string want = object2Object2ˢ;
    if (buf.String() != want) {
        Ꮡt.Errorf("enc.EncodeToken: expected %q; got %q"u8, want, buf.String());
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string dataᶜ = @"<foo b=""HELLOWORLD""></foo>"u8;

internal partial struct TestIssue16158_type /*dyn*/ {
    public byte B; /*`xml:"b,attr,omitempty"`*/
}

// Issue 16158. Decoder.unmarshalAttr ignores the return value of copyValue.
public static void TestIssue16158(ж<testing.T> Ꮡt) {
    @string data = dataᶜ;
    var err = Unmarshal(slice<byte>(data), Ꮡ(new TestIssue16158_type()));
    if (err == default!) {
        Ꮡt.Errorf("Unmarshal: expected error, got nil"u8);
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
internal static readonly @string invalidTagˢ = "invalid tag"u8;

public static void TestInvalidXMLName(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var enc = NewEncoder(new xml_test_package.bytes_BufferжWriter(Ꮡbuf));
    {
        var err = enc.Encode(new InvalidXMLName(nil)); if (err == default!){
            Ꮡt.Error(unexpectedSuccessˢ);
        } else 
        {
            @string want = invalidTagˢ; if (!strings.Contains(err.Error(), want)) {
                Ꮡt.Errorf("error %q does not contain %q"u8, err, want);
            }
        }
    }
}

// Issue 50164. Crash on zero value XML attribute.
public partial struct LayerOne {
    public global::go.encoding.xml_package.Name XMLName; /*`xml:"l1"`*/
    public ж<float64> Value; /*`xml:"value,omitempty"`*/
    public partial ref ж<LayerTwo> LayerTwo { get; } /*`xml:",omitempty"`*/
}

public partial struct LayerTwo {
    public ж<nint> ValueTwo; /*`xml:"value_two,attr,omitempty"`*/
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string l1Value12345ValueL1ˢ = @"<l1><value>1.2345</value></l1>"u8;

public static void TestMarshalZeroValue(ж<testing.T> Ꮡt) {
    @string proofXml = l1Value12345ValueL1ˢ;
    ref var l1 = ref heap(new LayerOne(), out var Ꮡl1);
    var err = Unmarshal(slice<byte>(proofXml), Ꮡl1);
    if (err != default!) {
        Ꮡt.Fatalf("unmarshal XML error: %v"u8, err);
    }
    var want = (float64)1.2345D;
    var got = l1.Value.Value;
    if (got != want) {
        Ꮡt.Fatalf("unexpected unmarshal result, want %f but got %f"u8, want, got);
    }
    // Marshal again (or Encode again)
    // In issue 50164, here `Marshal(l1)` will panic because of the zero value of xml attribute ValueTwo `value_two`.
    (var anotherXML, err) = Marshal(l1);
    if (err != default!) {
        Ꮡt.Fatalf("marshal XML error: %v"u8, err);
    }
    if (((sstring)anotherXML) != proofXml) {
        Ꮡt.Fatalf("unexpected unmarshal result, want %q but got %q"u8, proofXml, anotherXML);
    }
}

internal static slice<encodeTokenTestsᴛ1> closeTests = new encodeTokenTestsᴛ1[]{new(
    desc: "unclosed start element"u8,
    toks: new ΔToken[]{
        new StartElement(new Name(""u8, "foo"u8), default!)
    }.slice(),
    want: @"<foo>"u8,
    err: "unclosed tag <foo>"u8
), new(
    desc: "closed element"u8,
    toks: new ΔToken[]{
        new StartElement(new Name(""u8, "foo"u8), default!),
        new EndElement(new Name(""u8, "foo"u8))
    }.slice(),
    want: @"<foo></foo>"u8
), new(
    desc: "directive"u8,
    toks: new ΔToken[]{
        ((global::go.encoding.xml_package.Directive)slice<byte>((@string)"foo"u8))
    }.slice(),
    want: @"<!foo>"u8
)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorGotNoneˢ = (@string)" expected error; got none"u8;

public static void TestClose(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in closeTests) {
        ref var ttΔ1 = ref heap<encodeTokenTestsᴛ1>(out var ᏑttΔ1);
        ttΔ1 = tt;
        var ttʗ1 = ttΔ1;
        Ꮡt.Run(ttΔ1.desc, (ж<testing.T> tΔ1) => {
            ref var @out = ref heap(new strings.Builder(), out var Ꮡout);
            var enc = NewEncoder(new xml_test_package.strings_BuilderжWriter(Ꮡout));
            foreach (var (j, tok) in ttʗ1.toks) {
                {
                    var errΔ1 = enc.EncodeToken(tok); if (errΔ1 != default!) {
                        tΔ1.Fatalf("token #%d: %v"u8, j, errΔ1);
                    }
                }
            }
            var err = enc.Close();
            switch (ᐧ) {
            case {} when ttʗ1.err != ""u8 && err == default!: {
                tΔ1.Error(expectedErrorGotNoneˢ);
                break;
            }
            case {} when ttʗ1.err == ""u8 && err != default!: {
                tΔ1.Errorf(" got error: %v"u8, err);
                break;
            }
            case {} when ttʗ1.err != ""u8 && err != default! && ttʗ1.err != err.Error(): {
                tΔ1.Errorf(" error mismatch; got %v, want %v"u8, err, ttʗ1.err);
                break;
            }}

            {
                @string got = @out.String(); if (got != ttʗ1.want) {
                    tΔ1.Errorf("\ngot  %v\nwant %v"u8, got, ttʗ1.want);
                }
            }
            tΔ1.Log((~enc).p.closed);
            {
                var errΔ1 = enc.EncodeToken(((global::go.encoding.xml_package.Directive)slice<byte>((@string)"foo"u8))); if (errΔ1 == default!) {
                    tΔ1.Errorf("unexpected success when encoding after Close"u8);
                }
            }
        });
    }
}

} // end xml_internal_test_package
