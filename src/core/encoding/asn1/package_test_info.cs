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
[assembly: GoDynamicTypeLift("7374727563747b696e205b5d627974653b206f6b20626f6f6c3b2062617365313020737472696e677d", "bigIntTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e205b5d627974653b206f757420616e797d", "unmarshalTestDataᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e205b5d627974653b206f757420656e636f64696e672f61736e312e6578706c6963697454616767656454696d65546573747d", "explicitTaggedTimeTestDataᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b206465636f64656420737472696e673b20656e636f64656448657820737472696e673b20696e76616c696420626f6f6c7d", "bmpStringTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: go.GoHoistedLiterals]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("encoding/asn1/asn1_test.go", "asn1_test.cs", "ABw2AAcQgoKCgpSCAAoWAA4egoKCgpSCAAoWAA4egoKCgpSCAAkKAAsggoKCgpSCgpSCgoKUgoKCAAwaAAgSgoKCgpSCggANDIKCgpSClIKUgpSClIIACRQACBKCgoKCggAKFgAIEoKCgoKUgoLKgIIAChQAJESCgoKCgpSUgoKUgoKCgsoAHDqCgoKClIKCAAsYABw6goKCgpSCAAkUoKSgpKCkoKQAEiaCgoKCACZGABg4goKCgoKClIIACgqCAAUUgoKCgpSAkgArVoSSgIKkgui4goCCAAkSgoKUgoKClIKClIKUggAJFAARJKKCgILaADZuAC1cAH+AAu6CgoKCpoKCgqaCAAwSAAYUpoKCgoKUggAIEsqCgoCCpICCAAsQlO6CgIIADhCCgoKUgoKSgviCuIKAggAPHoKUgoKmgoKUgoKCuIKCgpSCppKAgraClKaCAAgIgoqCgqaCgIK2goKCABAIgo6UAAcWgoKAgriSgIIACwoADySCspKCgqaUgpKCpoLcgoKClIKCgriigoIADAiCAAcggoKUgoCCtoIACQjKgoKUlIKCgpSKgoKmgriCAAoIgoaCpoKCgpSCgpSmkoKCgILIkoKCgoKUgJIACRCyioKmgoKClIKClKaSgoKAgsiSgoKCgpSAkg==", "1151-1168:1;1266-1276:1;1278-1284:2;1286-1296:3;1312-1322:1;1324-1330:2;1332-1342:3", "", "282=FixedZone/1/2/1,Date/1/5/1,FixedZone/2/2/2,Date/2/5/2,Date/3/5/3,Date/4/5/4,Date/5/5/5;341=Date/1/4/1,Date/2/4/3,FixedZone/1/2/7,Date/3/4/7,FixedZone/2/2/8,Date/4/4/8;460=newInt64/1/3/12,newInt/1/3/13,newInt64/2/3/14,newInt/2/3/14,newInt64/3/3/15,newInt/3/3/15;522=newInt/1/2/4,newInt/2/2/9,newString/1/5/10,newString/2/5/11,newString/3/5/13,newBool/1/2/19,newBool/2/2/20,NewInt/1/1/22,newString/4/5/24,newString/5/5/25;717=Date/1/2/14,Date/2/2/15;980=Date/1/2/5,FixedZone/1/1/7,Date/2/2/7")]
[assembly: go.GoPositionMap("encoding/asn1/marshal_test.go", "marshal_test.cs", "AGXEAe6CgoKUpgA3kAGigoKClIKCAAoY3KKCgoKUgoIACRbugoKCgoKmgsqCgoK4ou6CgoKUgoLKgoKUgoKClIKCpoKCgqaCgqaCgoKClIK4goKCgoKmgoKCgpSCyqKUgoIACwqCAAYSgoKmgoaCgpSClILYgsyCgqaCgoKClIKUggAICKKEioKCgoKU3IKCgg==", "", "", "117=NewInt/1/1/7,Unix/1/4/15,UTC/1/3/15,Unix/2/4/16,UTC/2/3/16,Unix/3/4/17,In/1/1/17,farFuture/1/1/18,Unix/4/4/19,UTC/3/3/19")]
// </GoSourcePositionMaps>

namespace go.encoding;

[GoPackage("asn1")]
public static partial class asn1_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("testCase")] partial struct BenchmarkUnmarshal_testCase {}
    [GoLocalName("foo")] partial struct TestExplicitTagRawValueStruct_foo {}
    [GoLocalName("tagged")] partial struct TestImplicitTypeRoundtrip_tagged {}
    [GoLocalName("testSetSET")] partial struct TestSetEncoderSETSliceSuffix_testSetSET {}
    [GoLocalName("taggedRawValue")] partial struct TestTaggedRawValue_taggedRawValue {}
    [GoLocalName("untaggedRawValue")] partial struct TestTaggedRawValue_untaggedRawValue {}
    [GoLocalName("Recursive")] partial struct TestUnmarshalNestingLimitSlice_Recursive {}
    [GoLocalName("Recursive")] partial struct TestUnmarshalNestingLimitStruct_Recursive {}
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
