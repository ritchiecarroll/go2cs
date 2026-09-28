// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
using parse = go.text.template.parse_package;
// </ImportedTypeAliases>

using go;
using static go.html.template_package;
using static go.html.template_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b6120696e743b206220737472696e677d", "Δtype")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<ErrorWriter, io_package.Writer>]
[assembly: GoImplement<S, I>]
[assembly: GoImplement<T, I>(Pointer = true)]
[assembly: GoImplement<badMarshaler, go.encoding.json_package.Marshaler>(Pointer = true)]
[assembly: GoImplement<goodMarshaler, go.encoding.json_package.Marshaler>(Pointer = true)]
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
[assembly: go.GoPositionMap("html/template/clone_test.go", "clone_test.cs", "ABkiopKCgpSSgoKClICSABwIjNKWppKSpoKAgqSAkriSkqaCgIKkgJK4kpKmkqaSgIK2gIK2gIK4goCCpICSuICCuICCpICCuIKAgqSAkgAPCIKEmJKCgpSCgoKCgqaCAAgMkoKSAAgM0pKSkoKmgoL6kqqSqJKShIIADQqSmJK0goKCsoKCgIIACArWxpKSgviCkpKSgpSCAA4KkoiElIKSkoKCAAsMkpKSkoKAgqSAkg==", "181-183:1;212-219:1")]
[assembly: go.GoPositionMap("html/template/content_test.go", "content_test.cs", "AB4cogAMHADdAsYFgpKCgoKCgoCCgqSAkoIACxaC7oLmgoKCkoCCpIKClIKCgIKkgoIACwqSlIKCloKChII=")]
[assembly: go.GoPositionMap("html/template/css_test.go", "css_test.cs", "ABEaogAKHIKCggAJCqIAFTKCgoIACAqiABw+goKClIKAgtqCkoKAgqSCgIIACQqiAAwegoKCyqIAABQAABiCgpaCgviCADBmgoKC+qKC6KKCuKKCgoK4ooKCguiiguiigg==")]
[assembly: go.GoPositionMap("html/template/escape_test.go", "escape_test.cs", "ABkolNqCABYGogAQMoQAjgXQCrKigqSClIKAgqSAkqSCgIKkgJKkggAJDKK4AA0gkoKAgoKkgJKCABIKogAJIgBYugGSgoCCtqaCgoKUgoKClISAgoKkgIIACgyiANoBwgOCgoKCgpSCgoKUgoKUlIKCpoCCAAoMogDqBN4JgoKCgoKUgoIADAqiAFq8AYKSgoKClIKCgoKCgsqC3IKCgoKUgoL6goKCgoKk6KKCgoKUgoKk+KIACCaCgoKCgoKAggAKDqKSgpKSgpKCgoKklIKCgqQACAqSkoCC7IIACA7CkrKCgoKmgoLEgoKCggAIELKEgoKkAAkKspSUgpSCgoKClIKUuIKCuIKC+KKSgoKCggAQCrKSlIKCgIKigrSCgIKkgoCCABAKkrySgIKkkoCCpICCpJKClII=", "740-764:1;913-920:1;2074-2084:1;2075-2080:1.1")]
[assembly: go.GoPositionMap("html/template/exec_test.go", "exec_test.cs", "AGG+AYIADRaigpQACQ6igpQANV6yxgAOHLKmoqaigoKCqJKmgqaCpoKmgoKCpoKCgpTMkoKUqJLWgoKUpoIAnAPmBoKmgqaCpqKokoKUgpKClJTYktiSqJKmooKClKaCpqKClIKClKaCpoKmooIADyCCgoKClIKCgpSUgoKUgoKUgrSCtpLGgoLKggAYFIKCgoKCgoKCgpKUkpSUlJSCgpSCgoKUggAIDJKCgoKClIKCpIKUABQakoKClIKCgpSCgoL4ggAJGIKCggAjOIIAI0iCgpSClIKClIKCpoKCgpSCguiUABEWlIKCgoKUgoKCqIKCgpSCgoKUgoKmgoLogoKClIKCggCXAYICgoIACiKCgoKClIKCgoKUgoKUggALCoKmgoKUlIKClIKCgqaCgoKClIKCpoKCgoKUgoKCpoKCgqaCgoL8ooKClIKCAAkUgva0goKUgoKUgqaCgpSCgpSCgpSCgriCpoLugrqCyoLKgriigoKU3gAUBoK8goKUkoKWgoCCpICCpoKAgqSAggAOCKIAKFqykpKCgoKUggAMDIKClJKCgoKUgoLo3LiCkoKCgpSCAAwK7gAfSIKSggAHFIKCgpSUgoKUggALDLKUpgAfSoKCgoKUgoKkgpQADwyShpKCgoKUgoKUgqiCgoKCmJKCgoKClIIAEx6CgoKClIKCgoKogoKCsoKCgoCCAAgKAAgGgoSSgoCCpJaogoKUgoKUgIIAChSCgoCCpNaCgoKClIKClLiAggAICpKCgqyygoCCpICC", "713-718:1;1400-1402:1;1493-1503:1;1593-1595:1;1622-1624:1;1701-1701:1;1749-1757:1;1765-1771:1;1828-1830:1")]
[assembly: go.GoPositionMap("html/template/html_test.go", "html_test.cs", "AAwaogAAFgAAGoKCloKCgviCAA0igoCC2qKC6KKCuKKCuKKC")]
[assembly: go.GoPositionMap("html/template/js_test.go", "js_test.cs", "ABUcogBGlAGCgIKkgIK4gpaCAAkMggAMBqIAL2iCgIKkgriCgoCCAAoKogApXIKCggAJCqIAHkaCgoIACgqiAAAUAAtKgoCCgqqigpaAgoIACgqCAAgagoLKooK4ooLoooIACAiivoIACAiivoK4ooK4ooK4ooK4ooI=")]
[assembly: go.GoPositionMap("html/template/multi_test.go", "multi_test.cs", "ADJYlIKClIKClAAIBoKCgpSCgoKU5oKCgpSCgpSCgoKUAAgGgoSCgoKogoKCgpSWgoKCgpQADBSCgoKU1oKCgpTmgoKClIKClAALFpKCgoKClIIACAaCgoKAgqSAgqSAgvqSggAJCJKCgoKUgoKUgoL4poKAgqSAgqSAggAKCIIACBqChJiCgoKmgoCCgqSCAAkQ2viCgoKClIKCgqaCgoI=")]
[assembly: go.GoPositionMap("html/template/transition_test.go", "transition_test.cs", "ABIaogARKoKAggANCqSIgoKEgoKSgIKk")]
[assembly: go.GoPositionMap("html/template/url_test.go", "url_test.cs", "AA4WogANIIKAgqSCAAoKogAAFAALQoKAgoIACwqCAB9KgoCCAAgKooK4ooLoooK4ooLoooK4ooI=")]
// </GoSourcePositionMaps>

namespace go.html;

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
    [GoInit] internal static void initᴛᴛimportꓸarchiveꓸzip() => builtin.initPackage(typeof(archive.zip_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸhtml() => builtin.initPackage(typeof(html_package));
    [GoInit] internal static void initᴛᴛimportꓸhtmlꓸtemplate() => builtin.initPackage(typeof(go.html.template_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtextꓸtemplate() => builtin.initPackage(typeof(text.template_package));
    [GoInit] internal static void initᴛᴛimportꓸtextꓸtemplateꓸparse() => builtin.initPackage(typeof(text.template.parse_package));
    // </ImportInitializers>
}
