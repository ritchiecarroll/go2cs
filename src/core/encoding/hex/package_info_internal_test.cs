// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.encoding.hex_package;
using static go.encoding.hex_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f757420737472696e673b20657272206572726f727d", "errTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<TestEncoderDecoder_r, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<TestEncoderDecoder_r, io_package.Reader>]
[assembly: GoImplement<TestEncoderDecoder_w, io_package.Writer>(Promoted = true)]
[assembly: GoImplement<TestEncoderDecoder_w, io_package.Writer>]
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
[assembly: go.GoPositionMap("encoding/hex/hex_test.go", "hex_test.cs", "ABMoAAkUgoKCgoKUgpSCgoLKpoKCgoKCpJSCgoKkyoKCgoLKgoKCgoKUggALCgALIIKCgoKCyoKCgoIADAqCgoKChIKCgoCCgqaAgoKmgoKCgIKmgoLcgoKCgpSClILKgoKCloKCgoKCgoKUgpaCgvqCgoSCgoKChIKCuIKChIKEgoK4goKCloKCuAAIDoKCgoSSgoLcooKChJKCgtyCgoKCgoLcgoKEkoKC", "255-260:1;269-274:1;281-286:1;294-299:1")]
// </GoSourcePositionMaps>

namespace go.encoding;

[GoPackage("hex")]
public static partial class hex_internal_test_package
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
    // </ImportInitializers>
}
