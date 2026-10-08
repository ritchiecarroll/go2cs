// go2cs code converter defines `global using` statements here for imported type
// aliases as package references are encountered via `import' statements. Exported
// type aliases that need a `global using` declaration will be loaded from the
// referenced package by parsing its 'package_info.cs' source file and reading its
// defined `GoTypeAlias` attributes.

// Package name separator "dot" used in imported type aliases is extended Unicode
// character '\uA4F8' which is a valid character in a C# identifier name. This is
// used to simulate Go's package level type aliases since C# does not yet support
// importing type aliases at a namespace level.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.main_package;

// For encountered type alias declarations, e.g., `type Table = map[string]int`,
// go2cs code converter will generate a `global using` statement for the alias in
// the converted source, e.g.: `global using Table = go.map<go.@string, nint>;`.
// Although scope of `global using` is available to all files in the project, all
// converted Go code for the project targets the same package, so `global using`
// statements will effectively have package level scope.

// Additionally, `GoTypeAlias` attributes will be generated here for exported type
// aliases. This allows the type alias to be imported and used from other packages
// when referenced.

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("696e746572666163657b436c6f73652829206572726f727d", "Δtype")]
[assembly: GoDynamicTypeLift("696e746572666163657b4973286572726f722920626f6f6c7d", "testTypeAssertion_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b52656164285b5d62797465292028696e742c206572726f72297d", "WithInlineField_R")]
[assembly: GoDynamicTypeLift("696e746572666163657b556e777261702829206572726f727d", "testTypeSwitch_type")]
[assembly: GoDynamicTypeLift("7374727563747b696f2e5772697465727d", "fill_dst")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<byteRepeat, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<embeddedImpl, InlineEmbed>]
[assembly: GoImplement<fakeError, error>]
[assembly: GoImplement<fakeReader, WithInlineField_R>]
[assembly: GoImplement<fakeReader, takesReader_r>]
[assembly: GoImplement<fill_dst, io_package.Writer>(Promoted = true)]
[assembly: GoImplement<fill_dst, io_package.Writer>]
[assembly: GoImplement<tally, io_package.Writer>(Pointer = true)]
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
[assembly: go.GoPositionMap("AnonymousInterfaces.go", "AnonymousInterfaces.cs", "AA8SgpTEAAwKgoCClAALCoKCgtiCgoKCAA0QgoKCggAOFoCigNSCgoLegoIACAqAooCigOiCggAGEILqgoKUgoKClIKClAAIEoKCgpSmgoKCgoKChIKChII=")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("main")]
[GoTestMatchingConsoleOutput]
public static partial class main_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface takesReader_r {}
    internal partial interface testTypeAssertion_type {}
    internal partial interface testTypeSwitch_type {}
    internal partial struct byteRepeat {}
    internal partial struct embeddedImpl {}
    internal partial struct fakeError {}
    internal partial struct fakeReader {}
    internal partial struct fill_dst {}
    [GoValueClone("data")] internal partial struct frame {}
    internal partial struct quad {}
    internal partial struct tally {}
    public partial interface InlineEmbed {}
    public partial interface WithInlineField_R {}
    public partial interface Δtype {}
    public partial struct WithInlineField {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    // </ImportInitializers>
}
