// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.text.template_package;
using static go.text.template_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b6120696e743b206220737472696e677d", "Δtype")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<CustomError, error>(Pointer = true)]
[assembly: GoImplement<ErrorWriter, io_package.Writer>]
[assembly: GoImplement<S, I>]
[assembly: GoImplement<T, I>(Pointer = true)]
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
[assembly: go.GoPositionMap("text/template/exec_test.go", "exec_test.cs", "ABkqAE6eAYIADRaigpQACQ6igpSmtAAvWLLGAAkOAAgOlrKmoqaipqKCgoKokqaCpoKmgqaCgoKmgoKClKaWkoKUqJLWgoKUpoIADR6C5gDMA6AHgoKCgtyCgoKCAAcQooKCgpTWgqaCpoKmoqiSgpSCkoKUlNiS2JKokqaigoKUpoKmooKUgoKUpoKmgqaygsgADRqCgoKClJSCgpSCgpSCtILGgraCgsqCpgASEIKCgoKCgoKCgoKUgpSUlJSCgpSCgoKUggAIDJKCgoKClIKCkoKClAAVGpKCgpSCgoKUgoKCAAkMgNaShsiClICCAAgIggAJGIKCggAkOIIAI0iCgpSClIKClIKCpoKCgpSCggAICJSUgoKCggALCpSCgoKClIKCgqaCgpSCgoKUgoKCpoKC6IKCgpSCgoIACRQAgwH0AYKCAAoigoKCgpSCgoKClIKClIIADAqCpoKClJSCgpSCgoKmgoKCgpSCgoKmgoKCgpSCgoKmgoKCpoKCgvyigoKUgoLcuIIACAaUgoKUgoKUgqaCgpSCgpSCgpSCgriCpoLugrqCyoLKggAICIKCggAXNoKCgsqigoKU3qIAEgSCvIKClJKCpoKAgqSAgraCgIKkgIIADgiiAChaspKSgoKClIIADAyCgpSSgoKClIKCuNy4gpKCgoKUggAMCu4AIEiCkoIABxSCgoKUlIKClIIACwyylKYAH0qCgoKClIKCkoKClAALCqIAMW6CgoKClIKCkoKClAAODJSSgoKClIKClIK4goKCgqiSgoKCgpSCAAgKkoKSgoKCkoIACAqSlIKCpoKCpoKUgoLCgoKCgpSCgsa2AA4Koo6ClJSClII=", "754-760:1;764-770:1;806-811:1;867-867:1;1013-1015:1;1552-1554:1;1645-1655:1;1745-1747:1;1774-1776:1;1925-1925:1;1977-1989:1", "", "149=NewBuffer/1/1/37,New/1/2/38,newInt/1/1/39,newString/1/1/40,newIntSlice/1/1/41,newUnsafePointer/1/1/42,New/2/2/52,Parse/1/1/52,Must/1/1/52;334=NewBufferString/1/1/121,fVal1/1/5/336,fVal1/2/5/337,fVal1/3/5/338,fVal2/1/6/339,fVal2/2/6/340,fVal2/3/6/341,fVal2/4/6/342,fVal2/5/6/343,fVal1/4/5/344,fVal1/5/5/345,fVal2/6/6/346,rangeTestData/1/12/347,rangeTestData/2/12/348,rangeTestData/3/12/349,rangeTestData/4/12/350,rangeTestData/5/12/351,rangeTestData/6/12/352,rangeTestData/7/12/353,rangeTestData/8/12/354,rangeTestData/9/12/355,rangeTestData/10/12/356,rangeTestData/11/12/357,rangeTestData/12/12/359;1119=Parse/1/1/2")]
[assembly: go.GoPositionMap("text/template/multi_test.go", "multi_test.cs", "AB08ABImooKClIK0gsaClKSClIKClIKCgoKUgoLsAB42lIKClIKClAAIBoKCgpSCgoKU5oKCgpSCgpSCgoKUAAgGgpSCgoK4goKCgpSmgoKCgpTM+IKCgpTWgoKClAANFrSCgpSCgpSCgqSCgpSCgqaCgpSCqJKCgpSCpoKCgpSClIKCgoKSgJL4lIKClIKCpoKClIKClpKCgpSC+pKCgoKClIIACAaCgoKAgqSAgqSAgvqSggAICJKCgpSCgpSCggAKCLSClIKUgpSCpoKCgpSCpoL4poKAgqSAgqSAggAKCIIACBqClJiCgoKmgoCCgqSCAAkQuviCgoKClIKCgqaCgoLMkoKClII=")]
// </GoSourcePositionMaps>

namespace go.text;

[GoPackage("template")]
public static partial class template_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoValueClone("AI")] partial struct T {}
    [GoLocalName("A")] partial struct TestIssue48215_A {}
    [GoLocalName("B")] partial struct TestIssue48215_B {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸiter() => builtin.initPackage(typeof(iter_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtextꓸtemplate() => builtin.initPackage(typeof(go.text.template_package));
    [GoInit] internal static void initᴛᴛimportꓸtextꓸtemplateꓸparse() => builtin.initPackage(typeof(go.text.template.parse_package));
    // </ImportInitializers>
}
