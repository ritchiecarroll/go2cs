// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.encoding.asn1_package;

// <ImportedTypeAliases>
global using bigꓸInt = go.math.big_package.ΔInt;
global using bigꓸRat = go.math.big_package.ΔRat;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using runtimeꓸError = go.runtime_package.ΔError;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static global::go.encoding.asn1_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b6465636f64656420737472696e673b20656e636f64656448657820737472696e677d", "bmpStringTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e205b5d627974653b206f6b20626f6f6c3b2062617365313020737472696e677d", "bigIntTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e205b5d627974653b206f757420616e797d", "unmarshalTestDataᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e205b5d627974653b206f757420656e636f64696e672f61736e312e6578706c6963697454616767656454696d65546573747d", "explicitTaggedTimeTestDataᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
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
[assembly: go.GoPositionMap("encoding/asn1/asn1_test.go", "asn1_test.cs", "ACRGgoKCgpSCABk0goKCgpSCABk0goKCgpSCABYqgoKCgpSCgpSCgoKUgoKCABUsgoKCgpSCggANDIKCgpSClIKUgpSClIIAEiaCgoKCggATKIKCgoKUgoK6gIIAL1iCgoKCgpSUgoKUgoKCggAhRIKCgoKUgoIAKFKCgoKClIIACRSgpKCkoKSgABUqgoKCggBDfIKCgoKCgpSCAAoKggAEFIKCgoKUgJIALFaEkoCCpILoyIKAggAJEoKChIKCgpSCgpSClIIAGziigoCCAPEB4gOCgoKCloKCgpaCABUmpoKCgoKUggAJEsqCgoCCpICCAAwQlAAJDoKAggAPEIKCgoSCgqT4griCgIIADx6ChIKCloKClIKCgriCgoKUgpaSgIKmgpSWggAKCIKKgoKWgoCCpoKCggARCIKOhAAGFoKCgIKokoCCABIggoKCgpaEgoKWgoLKgoKClIKCgriigoIACgjKgoKUhIKCgoSKgoKWgqiC")]
[assembly: go.GoPositionMap("encoding/asn1/marshal_test.go", "marshal_test.cs", "AHrSAYKCgpQAPJYBooKCgpSCggAQJKKCgoKUgoIAECSCgoKCgpaCyoKCgriiAAoOgoKClIKCyoKClIKCgpSCgpaCgoKWgoKWgoKCgpSCuIKCgoKCloKCgoKUgsqihIKCAAwKggAEEoKCloKGgoKUgpSC2IKsgoKWgoKCgpSClIIACAiihIqCgoKChMyCgoI=")]
// </GoSourcePositionMaps>

namespace go.encoding;

[GoPackage("asn1")]
public static partial class asn1_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(go.encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(go.math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.encoding.asn1_package));
    }
}
