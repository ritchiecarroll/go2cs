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
using static go.image.color_package;

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
[assembly: GoTypeAlias("RGBA", "ΔRGBA")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<Alpha, Color>(Pointer = true)]
[assembly: GoImplement<Alpha, Color>]
[assembly: GoImplement<Alpha16, Color>(Pointer = true)]
[assembly: GoImplement<Alpha16, Color>]
[assembly: GoImplement<CMYK, Color>(Pointer = true)]
[assembly: GoImplement<CMYK, Color>]
[assembly: GoImplement<Gray, Color>(Pointer = true)]
[assembly: GoImplement<Gray, Color>]
[assembly: GoImplement<Gray16, Color>(Pointer = true)]
[assembly: GoImplement<Gray16, Color>]
[assembly: GoImplement<NRGBA, Color>(Pointer = true)]
[assembly: GoImplement<NRGBA, Color>]
[assembly: GoImplement<NRGBA64, Color>(Pointer = true)]
[assembly: GoImplement<NRGBA64, Color>]
[assembly: GoImplement<NYCbCrA, Color>(Pointer = true)]
[assembly: GoImplement<NYCbCrA, Color>]
[assembly: GoImplement<Palette, Model>(Pointer = true)]
[assembly: GoImplement<Palette, Model>]
[assembly: GoImplement<RGBA64, Color>(Pointer = true)]
[assembly: GoImplement<RGBA64, Color>]
[assembly: GoImplement<YCbCr, Color>(Pointer = true)]
[assembly: GoImplement<YCbCr, Color>]
[assembly: GoImplement<modelFunc, Model>(Pointer = true)]
[assembly: GoImplement<ΔRGBA, Color>(Pointer = true)]
[assembly: GoImplement<ΔRGBA, Color>]
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
[assembly: go.GoPositionMap("image/color/color.go", "color.cs", "AB880oKCgoKCgoKCAAsYggAHENKCgoKCgoKCgoKCgoKCggAIEtKCgoKCgoKCgoKCAAcQooKCAAcQooIABxCCgoIABxCCggAIFOzugrqSkpKSkpKSloKAgqSCpoKAgqSCpoKAgqSCgpSCpoKCgqaCgIKkgoKUgqaCgoKmgoCCpIKmgoCCpIKmgoCCpAAIFISmgoCCpN6EzpKClKq2gpKCgoKCgpSmrgAaLoK6kpKS")]
[assembly: go.GoPositionMap("image/color/ycbcr.go", "ycbcr.cs", "AAcQ7oKCugAPIoKClKiCgpSWqAA5coKCAA0egoKUloKClJaCgpSWABEkABAkgoIACxqCgpSWgoKUloKClJa4lIKAgqSCggAJFJSCggALGoKClJaCgpSWgoKUqILKlIKUpJSWgoKCloKokoKCgoKClIKUgpSCgoKokoKCgoIAChaogoKCgriUgoCCpIKC")]
// </GoSourcePositionMaps>

namespace go.image;

[GoPackage("color")]
public static partial class color_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct modelFunc {}
    public partial interface Color {}
    public partial interface Model {}
    public partial struct Alpha {}
    public partial struct Alpha16 {}
    public partial struct CMYK {}
    public partial struct Gray {}
    public partial struct Gray16 {}
    public partial struct NRGBA {}
    public partial struct NRGBA64 {}
    public partial struct NYCbCrA {}
    public partial struct Palette {}
    public partial struct RGBA64 {}
    public partial struct YCbCr {}
    public partial struct ΔRGBA {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    // </ImportInitializers>
}
