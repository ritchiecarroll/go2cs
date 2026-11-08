// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
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

// The go statements whose goroutine the converter proved cannot recurse, by the file, line and column of
// their goǃ call: go2cs-gen starts each on golib's bounded stack reservation rather than the Go-scale default.
[assembly: GoBoundedGoroutine("exec_test.cs", 755, 5, 0)]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("html/template/clone_test.go", "clone_test.cs", "AB0igpKCgpSSgoKClJIAKgiM0qa2kpK2goCCpICSyJKStoKAgqSAksiSkraStpKAgraAgraAgsiCgIKkgJLIgILIgIKkgILIgoCCpICSABAIgoSYkoKClIKCgoKCpoIACgySgpIACgyykpKSgqaCggAJCpK6kriSkpSCABAKkpiSxIKCgrKCgoLGpNamkpKCAAkIgpKSkoKUggAQCpKIlKSCkpKCggAMDJKSgpKCgpSS", "181-183:1;212-219:1")]
[assembly: GoPositionMap("html/template/content_test.go", "content_test.cs", "ACccggANHADeAsYFgpKCgoKCgoKClJKCAAoWgu6CAAgGgoKCkoCCpIKClIKCgIKkgoIACwqSpIKCpoKClII=")]
[assembly: GoPositionMap("html/template/css_test.go", "css_test.cs", "ABEaggAKHIKCggAJCoIAFTKCgoIACAqCABw+goKClIKCyoKSgoCCpIKAggAJCoIADB6CgoLKggABFAABGIKCpoKC+IIAMGaCgoIACAqigviigriigoKCuKKCgoLoooLoooI=")]
[assembly: GoPositionMap("html/template/escape_test.go", "escape_test.cs", "ABkolNqCABcGggAPMpQAvwWqC7KigqSClIKAgqSAkqSCgIKkgJKkggAJDIK4AA0gkoKCgpSSggASCoIACiIAWboBkoKCpraCgoKUgoKClJSCgpSCAAkKggDxAeoDgoKCgoKUgoKClIKClJSCgqaCAAkKggDzBO4JgoKCgoKUgoIADQqCAFq8AYKSgoKClIKCgoKCgsqC3IKCgoKUgoIACAqCgoKCgoKC+IKCgoKUgoKCggAJCIIACSaCgoKCgoKCAAoOgoKCgoKCkoKCgoKClIKCgoKCAAkKkpKC3IIACg7CkrKCgoKmgoLEgoKCggAKELKUgoKCggANCpKUlIKUgoKCgpSClLiCgriCggAICKKSgoKCggAVCpKSpIKCgIKCgIK0goCCpIKCABMKkrySgIKkkoCCpICCpJKClIIACwiigoKApJKCgpSCgrQACQSCkoKClIKCAAoIggAfSrKSkoKClIIADAyCABk+spKSgoKUgg==", "785-809:1;958-965:1;2145-2155:1;2146-2151:1.1;2276-2276:1;2340-2349:1;2386-2395:1", "", "2265=Parse/1/1/1;2267=Parse/1/1/1", "", "2211:25,2212:17,2427:21,2428:13")]
[assembly: GoPositionMap("html/template/exec_test.go", "exec_test.cs", "ABgsAEiSAYIADhaigpQACg6igpSmpAAsVLLGyO6msqaipqKCgoKokqaCpoKmgqaCgoKmgoKClKaWkoKUqJLmgoKUpoIADR6C1gCgA8AGgqaCpoKmoqiSgpSCkoKUlOiS6JKokqaigoKUpoKmooKUgoKUpoKmgqaCggAPIIKCgoKUgoKClJSCgpSCgpSCtILGgraCgsqCpgAUDoKCgoKCgoKCgoKUgpSUlJSCgpSCgoKUggAKDJKCgoKClIKCgoKClAAXGpKCgpSCgoKUgoKC+IIACRiCgoIAJjiCACNIgoKUgpSCgpSCgqaCgoKUgoL4lAAVFpSCgoKClIKCgriCgoKUgoKClIKCpoKC+IKCgpSCgoIACRQAgAHuAYKCAAoigoKCgpSCgoKClIKClIIADgqCpoKClJSCgpSCgoKmgoKCgpSCgqaCgoKClIKCgqaCgoKmgoKCAAkMooKClIKC7LiCAAkGlIKClIKClIKmgoKUgoKUgoKUgoK4gqaC7oK6gsqCyoK4ooKClN6iABUEgryCgpSSgqaCgpSAgraCgpSAggAQCIIAKFqykpKCgoKUggAODIKClJKCgoKUgoLo3LiCkoKCgpSCAA0K7gAgSIKSggAHFIKCgpSUgoKUggALDJKUpgAfSoKCgoKUgoKCgoKUABIMkpaSgoKClIKClIK4goKCgqiSgoKCgpSCABYegoKCgpSCgoKCuIKCgrKCgoKCxqQADAaClJKCgpSmuIKClIKClIIACRSCgoKU5oKCgoKUgoKUuIIACQqSgoK8soKClII=", "713-718:1;1400-1402:1;1493-1503:1;1593-1595:1;1622-1624:1;1701-1701:1;1749-1757:1;1765-1771:1;1828-1830:1", "", "143=NewBuffer/1/1/36,New/1/2/37,newInt/1/1/38,newString/1/1/39,newIntSlice/1/1/40,New/2/2/47,Parse/1/1/47,Must/1/1/47;313=NewBufferString/1/1/121;2086=New/2/2/4", "", "1552:21,1553:13")]
[assembly: GoPositionMap("html/template/html_test.go", "html_test.cs", "AAwaggABFgABGoKCpoKCgviCAA4igoLKooLoooK4ooK4ooI=")]
[assembly: GoPositionMap("html/template/js_test.go", "js_test.cs", "ABUcggBMoAGCgIKkgILIgqaCAAoMggAQBoIAL2iCgIKkgriCgoCCABAKggAqXIKCggAMCoIAH0aCgoIACgqCAAEUAAxKgoCCgrqigqaAgoIACgqCAAkagoLKooK4ooLoooIACAiivoIACAiivoK4ooK4ooK4ooK4ooI=", "", "", "148=Copysign/1/2/25,Copysign/2/2/31")]
[assembly: GoPositionMap("html/template/multi_test.go", "multi_test.cs", "ABQiACA2lIKClIKClAALBoKCgpSCgoKU9oKCgpSCgpSCgoKUAAwGgpSCgoK4goKCgpSmgoKCgpTMAAkIgoKClOaCgoKUAAgGgoKClIKClAARFpKCgoKClIIADAaCgoKClIKUgvqSggAKCJKCgoKUgoKUgoIACQimgoCCpICCpICCAAoIggAIGoKUmIKCgqaCgoKUggALELr4goKCgpSCgoKmgoKC")]
[assembly: GoPositionMap("html/template/transition_test.go", "transition_test.cs", "ABIaggARKoKCAA4KpJiCgpSCgpKClA==")]
[assembly: GoPositionMap("html/template/url_test.go", "url_test.cs", "AA4WggANIIKClIIACgqCAAEUAAxCgoKCAAoKggAgSoKC+qKCuKKC6KKCuKKC6KKCuKKC")]
// </GoSourcePositionMaps>

namespace go.html;

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
    [GoLocalName("dataItem")] partial struct TestEscapeSet_dataItem {}
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
