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
global using colorꓸRGBA = go.image.color_package.ΔRGBA;
global using imageꓸRGBA = go.image_package.ΔRGBA;
// </ImportedTypeAliases>

using go;
using static go.image.jpeg_package;

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
[assembly: GoDynamicTypeLift("7374727563747b627566205b343039365d627974653b206920696e743b206a20696e743b206e556e7265616461626c6520696e747d", "decoder_bytes")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<FormatError, error>]
[assembly: GoImplement<UnsupportedError, error>]
[assembly: GoImplement<bufio_package.Writer, writer>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<quantIndex, huffIndex>(Inverted = true, ValueType = "nint")]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("image/jpeg/fdct.go", "fdct.cs", "AFqsAdSCgoKCgoKCgoKChIKCgoSCgoKEgoKChIKCgoKChIKCgoKCgoKCgoKCgoKEgoKCgoK4goKCgoSCgoKEgoKChIKEgoKChIKCgoKCgoKCgoKCgoKEgoKCgoI=")]
[assembly: go.GoPositionMap("image/jpeg/huffman.go", "huffman.cs", "ACtgsoKCgoKUlIKCgpSUgqaqooKAgraCgoKCgpQACQqigoKUgIKkgoKUlIKUuoKCgoKUgpSClIKClICCuIKSgoLcgoKClIKqooKCgoKUgoKCgpSm6sKCloKAgoLKgpS2gIKCgoKmgoKCgIK2gpSCgoKUtKaCgoCCtoKCgqaCgoCCtoKCgoI=")]
[assembly: go.GoPositionMap("image/jpeg/idct.go", "idct.cs", "ADiQAQANBIKClISCgoKCgoKCgoKogoKCgoKCgpaCgoKCgpaCgoKCgoKCgpaCgoKCgpaCgoKCgoKCqLiWgoKCgoKCgpaCgoKCgpaCgoKCgoKCgpaCgoKCgpaCgoKCgoKC")]
[assembly: go.GoPositionMap("image/jpeg/reader.go", "reader.cs", "ABQogMqAAC1qAEWUAaKCuIKCgqaCgoKUgpQAAhDSgoKCgoK80oKAgraCgoIABhDUgoKCgoKUgpSCgpaEgoKUgoKWgoKUgoKUqrSCgpSWgoKCgoKUgIK2qKSCgpSWgoKClIKCgpSAgrYACwiSgpSUpKSk1ICCtoKUgoKCloKmgoKogoKWgpKClIKUAAEaAAMUmgAWAsaSxpIABxyUAAkCxoLGgvqClOiSgoKCgoKUgoKUlKSClIKAgqSCxoKUgoCCpIL4gpTYkoKUgIKkgqaCgpSAgqSEhIKUpoKClICCpISCgpaClAAICJKWgIKkgqiCgoKUABQqgoKCpoKUlKaCgqaSlO66gIKkgoKWlIKCgoK2gpS2gpS2gpSkgpS2pKSCtJS2gqiCgIK2gpSCgqSUlAAKFPKC3sqCgoKCgqYAChiChAAFEoKCgoKClIKCgpS4poKClKaUpoKCgoKCgoKCgoKCgqaokoLaooKAgqSU7IKClOzspoI=")]
[assembly: go.GoPositionMap("image/jpeg/scan.go", "scan.cs", "AAkYkoKCgpaCgoKCgpS0tLS0tLS0goSCkoIAFgqSgpSClICCpIKClIqCgoKCgoKmgpTcgoKmloKAgqSCgILaggASKIKCgoKCgoKUgpSCupKCgoKUgoKCgrqCkgAEEoKCgoKCggAZNIKClIKCgoKCuoKUloKAgraCgpSCgpSClIKClIKWgqaCgoKClIKCgoKClIKClJSCgoKCgpSUgpTMlAAHEJSAgsiCuICCpICCxoKCppSUuurUgoKUgoKUgpSogoKCgoKCgpSChJSCgoKCgpSUxoKCgpSCxqaCgpSClILYgoKAgraqwoKCgoKUgpSCgpSClIKUpqamgoKCgpSCgoKCgoCC2trCgoKUgpKClJS0tLS0yIKCgoKCgqSUlKYABRgACQLKhIKCpAAJEriCloCC")]
[assembly: go.GoPositionMap("image/jpeg/writer.go", "writer.cs", "AA4gkoKUuAAePgAyagBAkgGigoKCpoKSgoKCgoKU/oKiABkygoKUpoKClKaCgpSqooKCgoKCgoKUgpSokoKqooKClIKClJSCgrqSgoKCgqiSgoKCgrqSgoKCgoKCgoKClIKUgpSCpqiSgoKUlKKUgqKCgr7SlIKUkoKCgpSCgpSCpoKUqqKCgoKCgoKCgoLMsoKCgoKCgoLMsoKCgoKCgpSCgoKClIKCgoLMsoKCgoKCgpSCgoKUgoKCgs6igoKCgoKCAAwaAAwazJKUtLQABRCClpKCgoLogoKCgoKCgoKCpJSUlIKCgtoADhzCgoKUgoCClLaCgoKCpKiSgpSmgoKCgoKklLiCltaCgpSUlJSUgoKCgg==")]
// </GoSourcePositionMaps>

namespace go.image;

[GoPackage("jpeg")]
public static partial class jpeg_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface writer {}
    internal partial struct applyBlack_translations {}
    internal partial struct bits {}
    internal partial struct block {}
    internal partial struct component {}
    [GoValueClone("bytes", "comp", "progCoeffs", "huff", "quant", "tmp")] internal partial struct decoder {}
    [GoValueClone("buf")] internal partial struct decoder_bytes {}
    [GoValueClone("buf", "quant")] internal partial struct encoder {}
    internal partial struct huffIndex {}
    [GoValueClone("lut", "vals", "minCodes", "maxCodes", "valsIndices")] internal partial struct huffman {}
    internal partial struct huffmanLUT {}
    [GoValueClone("count")] internal partial struct huffmanSpec {}
    internal partial struct processSOS_scan {}
    internal partial struct quantIndex {}
    public partial interface Reader {}
    public partial struct FormatError {}
    public partial struct Options {}
    public partial struct UnsupportedError {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸimage() => builtin.initPackage(typeof(image_package));
    [GoInit] internal static void initᴛᴛimportꓸimageꓸcolor() => builtin.initPackage(typeof(go.image.color_package));
    [GoInit] internal static void initᴛᴛimportꓸimageꓸinternalꓸimageutil() => builtin.initPackage(typeof(go.image.@internal.imageutil_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    // </ImportInitializers>
}
