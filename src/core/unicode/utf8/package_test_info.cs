// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.unicode.utf8_package;

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static global::go.unicode.utf8_test_package;

// <ExportedTypeAliases>
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
[assembly: go.GoPositionMap("unicode/utf8/utf8_test.go", "utf8_test.cs", "AA0gkoKUgrqSgpSCAAoSACRGygAIEoKCgoKUgoKUgoKUgoKmgoKClILKgoKCgoKCgsqCgoCCpICC2oKCgoKClIKCgqiCgpSCgoKogoKUgoKUgoKCqIKUlIKClIKCgsyCgoKCgpSCgoLOooKCgtyCrsKCgoCCgqaCgIKCpIKCgpQADgoAJ3CCgpSmgoKCgIKCpIKAgoKkgoKUgoKCAAkKgoqCgoKCgoKUgoKCgoKUgoKClIKClJSCgoKCgoKUgoKUgoKUgoKClJSCupKCgoKCggAIEgAIEoKCgIKkgILagoCCgpQACRIADBqCgoCCABEUABQqgoKClIIACRQADh6igoCC2qKCgriigoK4ooK4ooK4lKKCgriigoK4ooKCtqKCgriigoK4ooK4ooK4ooK4ooK4ooL+goKCgoKUpoKmooKCuKKCgriigoK4ooKCuKKCgriigoK4ooKCuKKCgriigoK4ooKCuKKCgriigoK4ooKCuKKCgriigoK4ooKCAAwQggAEELKSgg==", "442-445:1;776-780:1")]
// </GoSourcePositionMaps>

namespace go.unicode;

[GoPackage("utf8_test")]
public static partial class utf8_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct BenchmarkFullRune_benchmarks {}
    [GoLocalName("info")] internal partial struct testSequence_info {}
    public partial struct RuneCountTest {}
    public partial struct RuneLenTest {}
    public partial struct Utf8Map {}
    public partial struct ValidRuneTest {}
    public partial struct ValidTest {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.unicode.utf8_package));
    }
}
