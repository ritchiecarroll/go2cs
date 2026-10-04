// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static go.encoding.xml_package;
using static go.encoding.xml_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b4920696e747d", "Child_G")]
[assembly: GoDynamicTypeLift("7374727563747b56616c756520616e793b2045727220737472696e673b204b696e64207265666c6563742e4b696e647d", "marshalErrorTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b56616c756520616e793b20457870656374584d4c20737472696e673b204d61727368616c4f6e6c7920626f6f6c3b204d61727368616c4572726f7220737472696e673b20556e6d61727368616c4f6e6c7920626f6f6c3b20556e6d61727368616c4572726f7220737472696e677d", "marshalTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b56616c756520616e793b2050726566697820737472696e673b20496e64656e7420737472696e673b20457870656374584d4c20737472696e677d", "marshalIndentTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b584d4c4e616d6520656e636f64696e672f786d6c2e4e616d652022786d6c3a5c22747970652c617474725c22227d", "InvalidXMLName_Type")]
[assembly: GoDynamicTypeLift("7374727563747b584d4c4e616d6520656e636f64696e672f786d6c2e4e616d653b204120737472696e672022786d6c3a5c22783e615c22223b204220737472696e672022786d6c3a5c22783e625c22223b204320737472696e672022786d6c3a5c22737061636520783e635c22223b20433120737472696e672022786d6c3a5c2273706163653120783e635c22223b20443120737472696e672022786d6c3a5c2273706163653120783e645c22227d", "Δtypeᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b584d4c4e616d65207374727563747b7d2022786d6c3a5c22737061636520746f705c22223b204120737472696e672022786d6c3a5c22783e615c22223b204220737472696e672022786d6c3a5c22783e625c22223b204320737472696e672022786d6c3a5c22737061636520783e635c22223b20433120737472696e672022786d6c3a5c2273706163653120783e635c22223b20443120737472696e672022786d6c3a5c2273706163653120783e645c22227d", "Δtype")]
[assembly: GoDynamicTypeLift("7374727563747b584d4c4e616d65207374727563747b7d2022786d6c3a5c22746f705c22223b204220737472696e672022786d6c3a5c22737061636520783e625c22223b20423120737472696e672022786d6c3a5c2273706163653120783e625c22227d", "Δtypeᴛ2")]
[assembly: GoDynamicTypeLift("7374727563747b6465736320737472696e673b20746f6b73205b5d656e636f64696e672f786d6c2e546f6b656e3b2077616e7420737472696e673b2065727220737472696e677d", "encodeTokenTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b2065727220737472696e677d", "characterTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e70757420737472696e673b20657870656374205b325d737472696e677d", "procInstTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7620616e793b206520616e797d", "badPathTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b786d6c20737472696e673b2074616220656e636f64696e672f786d6c2e5461626c6541747472733b206e7320737472696e677d", "tableAttrsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b786d6c20737472696e673b2074616220656e636f64696e672f786d6c2e5461626c65733b206e7320737472696e677d", "tablesᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<MyAttr, global::go.encoding.xml_package.UnmarshalerAttr>(Pointer = true)]
[assembly: GoImplement<MyCharData, global::go.encoding.xml_package.Unmarshaler>(Pointer = true)]
[assembly: GoImplement<MyMarshalerAttrTest, global::go.encoding.xml_package.MarshalerAttr>(Pointer = true)]
[assembly: GoImplement<MyMarshalerTest, global::go.encoding.xml_package.Marshaler>(Pointer = true)]
[assembly: GoImplement<downCaser, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<errWriter, io_package.Writer>]
[assembly: GoImplement<limitedBytesWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<mapper, global::go.encoding.xml_package.TokenReader>]
[assembly: GoImplement<tokReader, global::go.encoding.xml_package.TokenReader>]
[assembly: GoImplement<toks, global::go.encoding.xml_package.TokenReader>(Pointer = true)]
[assembly: GoImplement<toksNil, global::go.encoding.xml_package.TokenReader>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("encoding/xml/atom_test.go", "atom_test.cs", "AAoSABNGgoKClKaC")]
[assembly: go.GoPositionMap("encoding/xml/marshal_test.go", "marshal_test.cs", "ALUDgAWUgoKCgtyUgqaCAEBmoqaiAIMCpgIAlAe+EYKygpaSgoKCgpSClJSCgpSAkoKUABgeACdMAAwkgoKCgoKUgpSCgILukrKClICCpKyWgoKEspSCgriCgoKUgpSUgILsgoKCgoKUgJIADRSygoKUgoKCgpSCgtaCgoKCgoKCgoK4gqaClIKUguiChIKCguiCgoKAgqSClICCpIK4ooKCgsqigoKSggAQDJIAABS6goKUgIKkgoKCAAsIAP8CxAWCgrKCgoKCgoKCpqKUlIK0grSCtICCgqSAgoL6goKEgIKmgIKmgIIADQiCkoqCgoKCggAMDJKKgoKCgqamggAHEAAKFoKCpoKC/JKCgoCCpICCpICCpICCpICCpICCpIKUgoIACwqSgoaCABMagoKCgIKiggAXIIKCgoKClIKCgrqCgpSCuAAZNoKCkpKCgoKAgraClLS0tICCpIKAgg==", "1667-1690:1;1798-1820:1;1915-1919:1;1925-1929:1;2330-2332:1;2398-2401:1;2565-2589:1")]
[assembly: go.GoPositionMap("encoding/xml/read_test.go", "read_test.cs", "ABEokoKAgqSCAHbAAQClAbQCAAcQgoKCgIKkggAuRgAGEqKCgoIAEx6CgoCCpIIAEgiCAAAcgoCCpIKkloKAgqSCloKAgqSCpAAQEgAhboKCgoKCgoKUlIKClIKC+oKSgoKUgoKCACIkACmIAYKCgoKCgoKUlIKClIKC+oKSgoKUgoKCloKAgqaCAAcQooKCkpSClICCtqaUgu6CgqYAFBSCjoKAgqaCABAakoKCgoKClIKClJKCAAgSktyCgoKAggAMFpKCgIKkgpSCAEeOAaSCgIKmAAkCABImgroACAIACQLyABgygIKmABQmggA3bpKSgIKmABYuggBNcJKSgIKmABYuggAICpKKhIIACwqCiIKCpAALCKKClIKCgqaGog==", "1118-1123:1")]
[assembly: go.GoPositionMap("encoding/xml/xml_test.go", "xml_test.cs", "ABYsgoKUgoKClAANBoKCABIusrKCoriGgpSAggAMGIKCpoKUlIKC9qKCgoKCgriChoCCAAgulAAmSgAsUgAMEgApXoKCggAPHAAjSIKCggAHELKCgpSmgoKmgoKCgpSU1oKCgoKUgpSCgpSClIKCyqKCgoKCgoKUgpKAgpSkgIKUpKiUtpKUtLSCgsYAEiQAESSChIKCgpSCyoKChIKCgpSCyoKCgoKUgIIAFAqCAAAWAAwagoKAgoKmgoIAGzYAMV6CgoSClIIADBCCgoKigoCCpICCpICCpIKClIKC+IKCgoSC6IKCgoKCgIKkgpSCgpSCuILcooKCgoCCpIKUgoKUggAICoKCgoKClIKC+IKCgoKClIKUgoL4goKCgoKUgoIACAiCgoKClIKClILogoKCgpSCuIKCgoKUguiCgoKClIIACAgACx6EgoKEgpSCgpSCyoL8goIACgoADSSCooCCpICCAA8cAAkUgoSCgoKUggAIEICkgoKEguiCgoSCgIKkhILYgoSClIIAFgiEAAAYgoKAggAKCIIABRaCgIKAkrYADQqEjvaEigAIBoSeggAIBoSMpoLKgoKCgoKCpoCCAAoKggAFEoKCgoKCgoKUpoKClIIAFgqCAAAegoKCgoKWgpSCloKCgpSClIKUgoKClIKUggAMCISEAAwggoKAgoKkpAALDIIACBiCgoKCgoKClKaCgpSCyoIAChqCgoKU1oKCgoIADwiCgoKUgoLGgoK2loSagIKmguyC2oLWooKAgriCoqSCgoKCgoKCgpSClICCpJSAgqaCgoKClIKUgpSCgpSUgriCuIKAABMIooYADyyCgoKCgoKmgoKUpoKClIKCABAegoKCgoKCgoKCgoKU7oIAJ1KCgoKCgoKCgoKUlJSClIKClIKC", "62-76:1;104-118:1;354-359:1;1218-1223:1;1249-1263:1;1294-1298:1;1355-1355:1;1360-1362:1;1423-1439:1")]
// </GoSourcePositionMaps>

namespace go.encoding;

[GoPackage("xml")]
public static partial class xml_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸxml() => builtin.initPackage(typeof(go.encoding.xml_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    // </ImportInitializers>
}
