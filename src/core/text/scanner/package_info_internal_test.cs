// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static go.text.scanner_package;
using static go.text.scanner_internal_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<StringReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<countReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<errReader, io_package.Reader>]
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
[assembly: go.GoPositionMap("text/scanner/scanner_test.go", "scanner_test.cs", "ABYssoKCgpSUpoKCgoKCgoKUlIIADh6CggCbAcwCgoKClKaigpSClIKCpoKCyqKCgoK4goKCgqamooKCgoKCgoKUlKaCggAIBoKCgoKCgqaCgoKCgoKCgriigoKCgoKCgpSClIKUgoKmgriigoKCgoKCgoKUlIKUgriCgoKCgoKClJSCuKKCuIKCggAJBoKCuIaCgoKCgoKCgoKCpoKCgoKCgoKCgoKCgoKCgoKCgoKCuIKClIKClIKEgoKCgoK4goKCgpSAgqSClKaCgpSClIIAEQiCgoKEgoKEgoKEgoKEgoSCgoKCgoKCgoSCgoKCguyCpoKCgoKCgIKkpoKClIK4goLKooCCpIKmooKCgIKCgrbmtIKCgpaCgpSClIKogoKUgpSCqIKCgoKCgoKCgoKCgoKCgpSClIKogoKCgoKCgoKCgoKClIKUgtyCguaCloSCgpaCgpaCuIKWhIKCloKCloLoooKCgoKCpgAICIIAbooCgoKCgqiCgoKCgoKUgqaCuoCCAAoKggAHFoKCyqKCgoKCgKSClKQADAqCgoKEgoKCgpaCgoI=", "296-301:1;395-397:1;458-469:1;530-537:1;704-710:1;851-855:1;920-920:1")]
// </GoSourcePositionMaps>

namespace go.text;

[GoPackage("scanner")]
public static partial class scanner_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    // </ImportInitializers>
}
