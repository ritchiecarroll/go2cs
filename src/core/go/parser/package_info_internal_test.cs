// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.go.parser_package;
using static go.go.parser_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20666f726d617420737472696e673b2070617273654d756c7469706c69657220696e743b2073636f706520626f6f6c3b2073636f70654d756c7469706c69657220696e747d", "parseDepthTestsᴛ1")]
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
[assembly: global::go.GoPositionMap("go/parser/error_test.go", "error_test.cs", "ACdIurKCgoKUlJSmgoCCpAAIEpiihLiCgoSCgqSkgoKCkoKUlMiSlLSCgoKUlN6igqaCgJSCgoKUgIKCttzKgoKCyoKCgoKCloKCkoKClISCpqjmgoKClIKCkoKCgpQ=", "42-50:1;192-200:1")]
[assembly: global::go.GoPositionMap("go/parser/parser_test.go", "parser_test.cs", "ABEg7oKCgoLKgpSkpKaA1IKCgoLogoKCguiCgoKClICCpIKCgpSAgqSCgvqCgoKCAA0IpoKCgqaAgriCgoKmgIK4goKClIKUgIK4goCCuIKAgqSCgIKokoKCgoCCyoLogoKCqIKCgoKmgoKC+oKCgqiCgoKCpoKCABAKogAAEoKClgAMFpKAgoKCgpSUgpSCgrYAHgiCAAAugpYAEiqSgoKUhIIAGQiCAAAkgpQABxCClIKCgoKUgoKC3KKCgoCCgoCCgIKCgoIACxKosoKCgqamgoKClICCpICCACsIggAAJIKUgoKCgoKChIKCgoL6kgALGIKCgpaSkpS0tpKWgoCSgoKUxgAYCoIAABaCgoKogJK2gJIADQ6y2IKCgoKWgoKWgoKAgqSUgoKUgoKCAA4KgoaCgoKUgoIAKwgALYQBopKClNaCgpSygpKCgpTcloKCgpSUhIKCgoKmgoIAChCCsoKUgpKCgpTcloKCgpSUhIKCgoKmgoIAChKSAAYQgoKCgpaSlIKCtsySzIKCgvqCgoKCloKCgpSCggAIDIKCgoKCuqiCgoIACQiChIKCgpaCgoIACwqSAAUSgoKCgg==", "230-248:1;465-487:1;540-545:1;653-688:1;699-734:1;756-765:1")]
[assembly: global::go.GoPositionMap("go/parser/performance_test.go", "performance_test.cs", "AA4alIKCgpSmooKCgILaooKCgILaooKCgoKCgpSCgg==")]
[assembly: global::go.GoPositionMap("go/parser/resolver_test.go", "resolver_test.cs", "ABk8AAwCgoKCloKSgoKCgoKCvIKChJKmgpSCgIKkpoIABRCigqSAgqSUrLKElIKCgpSCpq7ygoKCgoSCgoKkpIKCgoCCpJSCypKUpNamwoKUhIKClLS0guY=", "38-72:1;55-61:1.1;80-86:1")]
[assembly: global::go.GoPositionMap("go/parser/short_test.go", "short_test.cs", "AA0WAGbYAYKC6pKCAAgGADycAYKC")]
// </GoSourcePositionMaps>

namespace go.go;

[GoPackage("parser")]
public static partial class parser_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
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
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸast() => builtin.initPackage(typeof(global::go.go.ast_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸscanner() => builtin.initPackage(typeof(global::go.go.scanner_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(global::go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
}
