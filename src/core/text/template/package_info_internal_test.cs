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
[assembly: go.GoPositionMap("text/template/exec_test.go", "exec_test.cs", "ABgoAEuYAYIADRaigpQACQ6igpSm5AAtVLLGAAkOAAgOlrKmoqaigoKCqJKmgqaCpoKmgoKCpoKCgpSmlpKClKiS1oKClKaCAA0ekgA/BgDrApIHgoKCgtyCgoKCAAcQooKCgpTWgqaCpoKmoqiSgpSCkoKUlNiS2JKokqaigoKUpoKmooKUgoKUpoKmgqaygsgADRqCgoKClJSCgpSCgpSCtIK2ksaCgsqC9gASEIKCgoKCgoKCgpKUkpSUlJSCgpSCgoKUggAIDJKCgoKClIKCkoKClAAVGpKCgpSCgoKUgoKCAAkMgNaShriChIKC+IIACRiCgoIAJDiCACNIgoKUgpSCgpSCgqaCgoKUgoIACAiUlIKCgoIACwqUgoKCgpSCgoKmgoKUgoKClIKCgqaCguiCgoKUgoKCABgUAIEB9AGCggAKIoKCgoKUgoKCgpSCgpSCAAwKgqaCgpSUgoKUgoKCpoKCgoKUgoKCpoKCgoKUgoKCpoKCgqaCgoL8ooKClIKC3LiCAAgGtIKClIKClIKmgoKUgoKUgoKUgoK4gqaC7oK6gsqCyoK4ooKClN6iABIEgryCgpSSgpaCgIKkgIKmgoCCpICCAA4IogAoWrKSkoKCgpSCAAwMgoKUkoKCgpSCgrjcuIKSgoKClIIADAruAB9IgpKCAAcUgoKClJSCgpSCAAsMspSmAB9KgoKCgpSCgpKCgpQACwqiADBugoKCgpSCgpKCgpQADgyUkoKCgpSCgpSCqIKCgoKYkoKCgoKUggAICpKCkoKCgpKCAAgKkoSCgpaCgpaChIKCwoKCgoKUgoLGpgAOCqKOgpSUgpSC", "737-743:1;747-753:1;789-794:1;850-850:1;996-998:1;1498-1500:1;1591-1601:1;1691-1693:1;1720-1722:1;1871-1871:1;1923-1935:1")]
[assembly: go.GoPositionMap("text/template/multi_test.go", "multi_test.cs", "AB48ABEmooKClIK0graSlLSClJKClIKCgoKUgoL8ABw2lIKClIKClAAIBoKCgpSCgoKU5oKCgpSCgpSCgoKUAAgGgoSCgoKogoKCgpSWgoKCgpTM+IKCgpTWgoKClAAMFJSCgpSCgpSkgoKUgoKmgoKUgqiSgoKUgqaCgoKUguiUgoKUgoKmgoKUgoKWkoKClIL6koKCgoKUggAIBoKCgoCCpICCpICC+pKCAAgIkoKClIKClIKCAAoItIKEgpSClIKWgoKClIKmgvimgoCCpICCpICCAAoIggAHGoKEmIKCgqaCgIKCpIIACRDa+IKCgoKUgoKCpoKCgsySgoKUgg==")]
// </GoSourcePositionMaps>

namespace go.text;

[GoPackage("template")]
public static partial class template_internal_test_package
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
