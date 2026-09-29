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
// </ImportedTypeAliases>

using go;
using static go.image_package;

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
[assembly: GoTypeAlias("Opaque", "const:ΔOpaque")]
[assembly: GoTypeAlias("RGBA", "ΔRGBA")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<Alpha, Image>(Pointer = true)]
[assembly: GoImplement<Alpha, RGBA64Image>(Pointer = true)]
[assembly: GoImplement<Alpha16, Image>(Pointer = true)]
[assembly: GoImplement<Alpha16, RGBA64Image>(Pointer = true)]
[assembly: GoImplement<CMYK, Image>(Pointer = true)]
[assembly: GoImplement<CMYK, RGBA64Image>(Pointer = true)]
[assembly: GoImplement<Gray, Image>(Pointer = true)]
[assembly: GoImplement<Gray, RGBA64Image>(Pointer = true)]
[assembly: GoImplement<Gray16, Image>(Pointer = true)]
[assembly: GoImplement<Gray16, RGBA64Image>(Pointer = true)]
[assembly: GoImplement<NRGBA, Image>(Pointer = true)]
[assembly: GoImplement<NRGBA, RGBA64Image>(Pointer = true)]
[assembly: GoImplement<NRGBA64, Image>(Pointer = true)]
[assembly: GoImplement<NRGBA64, RGBA64Image>(Pointer = true)]
[assembly: GoImplement<NYCbCrA, Image>(Pointer = true)]
[assembly: GoImplement<Paletted, Image>(Pointer = true)]
[assembly: GoImplement<Paletted, PalettedImage>(Pointer = true)]
[assembly: GoImplement<Paletted, RGBA64Image>(Pointer = true)]
[assembly: GoImplement<RGBA64, Image>(Pointer = true)]
[assembly: GoImplement<RGBA64, RGBA64Image>(Pointer = true)]
[assembly: GoImplement<Rectangle, Image>(Pointer = true)]
[assembly: GoImplement<Rectangle, RGBA64Image>(Pointer = true)]
[assembly: GoImplement<Rectangle, RGBA64Image>]
[assembly: GoImplement<Uniform, Image>(Pointer = true)]
[assembly: GoImplement<Uniform, RGBA64Image>(Pointer = true)]
[assembly: GoImplement<Uniform, go.image.color_package.Model>(Pointer = true)]
[assembly: GoImplement<YCbCr, Image>(Pointer = true)]
[assembly: GoImplement<YCbCr, RGBA64Image>(Pointer = true)]
[assembly: GoImplement<bufio_package.Reader, reader>(Pointer = true)]
[assembly: GoImplement<ΔRGBA, Image>(Pointer = true)]
[assembly: GoImplement<ΔRGBA, RGBA64Image>(Pointer = true)]
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
[assembly: go.GoPositionMap("image/format.go", "format.cs", "AA8gAA8q4oKCggAJFJKAgqSokoKUgoKmqJKCgoKCpq7CgoKClIKuwoKCgpSC")]
[assembly: go.GoPositionMap("image/geom.go", "geom.cs", "ABImkqiSqJKokqiSqJKsopKCgoKUgoKUqJIABxKSAA4gkqiSqJKokt6S3pIABRKygoKUgpSCgpSClKqigpSClIKUgtyClKiSgpSClIKUgpSClIKUqJKqoqiSrJKCuKyigpSClKiSgpSokoKUqJKokgAHFrKClIKUqqKClIKClIKClIKClKqigpSCgpQ=")]
[assembly: go.GoPositionMap("image/image.go", "image.cs", "AFi6AfKCgpQADRyApICkgqaCgpSCgoKCgoIABxCCgpSCgqqipoKClIKCgoKCgqaCgpSCgoKCgqaCgpSCgoKCgqqiuIKUggAGEJKClJKCgoKmgpTYkgARJICkgKSCpoKClIKCAAcUoqaCgpSCgoKCgoKCgoKCpoKClIKCgoKCgoKCgqqiuIKUggAGEJKClJKCgoKmgpTYkgARJICkgKSCpoKCpoKClIKCqqKmgoKUgoKCgoKCpoKClIKCgoKUgoKCgoKmgoKUgoKCgoKqoriClIIABhCSgpSSgoKCpoKU2JIAESSApICkgqaCgqaCgpSCggAHFKKmgoKUgoKCgoKCgoKCgqaCgpSCgoKClIKCgoKCgoKCgqaCgpSCgoKCgoKCgoKqoriClIIABhCSgpSSgoKCpoKU2JIAESSApICkgqaCgoKmgoKUgqqipoKClIKmgoKUgqaCgpSCqqK4gpSCAAYQkoKUkoKCgqaClNiSABEkgKSApIKmgoKmgoKUgqqipoKClIKCgqaCgpSCgqaCgpSCgqqiuIKUggAGEJKClJKCgoKmgpTYkgARJICkgKSCpoKCgqaCgpSCqqKmgoKUgqaCgqaCgqaCgpSCqqK4gpSCAAYQktiSABEkgKSApIKmgoKmgoKUgqqipoKClIKCgqaCgqaCgoKmgoKUgoKqoriClIIABhCS2JIAESSApICkgqaCgqaCgpSCgqqipoKClIKCgoKCgqaCgpSCgoKCgoKmgoKUgoKCgoKqoriClIIABhCS2JIAEyiApICkgoKUgpSCpoKClIKClIKUggAHFKKmgoKUgqaCgpSCpoKClIKmgoKUgqqiuIK4ggAHEpKCkoKClIKUgoKUgoKm2qI=")]
[assembly: go.GoPositionMap("image/names.go", "names.cs", "AAoahISEAAcSgqaCpoKmgKSApIKCqJKCqJI=")]
[assembly: go.GoPositionMap("image/ycbcr.go", "ycbcr.cs", "ABsugpSkpKSkpKQAGDKCpoKmgqaCgqaCgpSCggAGEqKqopSkpKSktqqiuIK4goIAChaCptKClIKkgqSCpIKkgqaCtKqitqiCloKCgoIAEyiCpoKmgoKmgoKUgoKCAAkYoqqiuILcgoKCAA4gkoKUkoKCgqaClKqipqiCloKCgoKC")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("image")]
public static partial class image_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface reader {}
    internal partial struct format {}
    public partial interface Image {}
    public partial interface PalettedImage {}
    public partial interface RGBA64Image {}
    public partial struct Alpha {}
    public partial struct Alpha16 {}
    public partial struct CMYK {}
    public partial struct Config {}
    public partial struct Gray {}
    public partial struct Gray16 {}
    public partial struct NRGBA {}
    public partial struct NRGBA64 {}
    public partial struct NYCbCrA {}
    public partial struct Paletted {}
    public partial struct Point {}
    public partial struct RGBA64 {}
    public partial struct Rectangle {}
    public partial struct Uniform {}
    public partial struct YCbCr {}
    public partial struct YCbCrSubsampleRatio {}
    public partial struct ΔRGBA {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸimageꓸcolor() => builtin.initPackage(typeof(image.color_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    // </ImportInitializers>
}
