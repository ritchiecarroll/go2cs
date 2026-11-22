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
using static go.sort_package;

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
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<Float64Slice, Interface>(Pointer = true)]
[assembly: GoImplement<Float64Slice, Interface>]
[assembly: GoImplement<IntSlice, Interface>(Pointer = true)]
[assembly: GoImplement<IntSlice, Interface>]
[assembly: GoImplement<StringSlice, Interface>(Pointer = true)]
[assembly: GoImplement<StringSlice, Interface>]
[assembly: GoImplement<reverse, Interface>(Pointer = true)]
[assembly: GoImplement<reverse, Interface>(Promoted = true)]
[assembly: GoImplement<reverse, Interface>]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("sort/search.go", "search.cs", "AAp0ADMGkoKUgpS4AAI2AB0IkoKUgpS4AAIS4r7CvsLYgLaAtoA=", "124-124:1;132-132:1;140-140:1")]
[assembly: GoPositionMap("sort/slice.go", "slice.cs", "AAwwAAwCgoKCggACGAAJAoKCAAIQ0oKCgoKm")]
[assembly: GoPositionMap("sort/sort.go", "sort.cs", "ACtk4oKClIIADRyCgoKCxoKCABIkksiSrsKCgoKmAAkQgoKWgAAJDAABENCCtpLIgAAICoKCloC+wJywmqCaoJywmqAAAUgAIgI=")]
[assembly: GoPositionMap("sort/zsortfunc.go", "zsortfunc.cs", "AAgUkoKCzqKCgoKClIKUgpSCuIKCgqaCuIKCAAMU4pSqgpSCgriCgriCgqaCgriCuIKC3IKCgqaClJKCgoKClIKCAAQSwoKkgpSClIKClIKClIKClIKUgpSCgpSCqqKCpIKClIKUgpSCgpSokpiCgoKmgqaCpqaCgoKUuIKCgpS4qqKCgoKUgoKClAAEFNKqlM6ClIKCpqaUpKS6koKClKiSgoKCqJKmgoKCgoKCuIKCuIKCkoKCgpSUgoKCgoKUgpQAAy4AFgi4goKCgoKUuIKU3LiCgoKCgpS4gpSmgoKSgoKUgpSUgoKClLiCgpSClIIAAxDCgpSCgoKUgrg=")]
[assembly: GoPositionMap("sort/zsortinterface.go", "zsortinterface.cs", "AAgUkoKCzqKCgoKClIKUgpSCuIKCgqaCuIKCAAMU4pSqgpSCgriCgriCgqaCgriCuIKC3IKCgqaClJKCgoKClIKCAAQSwoKkgpSClIKClIKClIKClIKUgpSCgpSCqqKCpIKClIKUgpSCgpSokpiCgoKmgqaCpqaCgoKUuIKCgpS4qqKCgoKUgoKClAAEFNKqlM6ClIKCpqaUpKS6koKClKiSgoKCqJKmgoKCgoKCuIKCuIKCkoKCgpSUgoKCgoKUgpQAAy4AFgi4goKCgoKUuIKU3LiCgoKCgpS4gpSmgoKSgoKUgpSUgoKClLiCgpSClIIAAxDCgpSCgoKUgrg=")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("sort")]
public static partial class sort_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct lessSwap {}
    internal partial struct reverse {}
    internal partial struct sortedHint {}
    internal partial struct xorshift {}
    public partial interface Interface {}
    public partial struct Float64Slice {}
    public partial struct IntSlice {}
    public partial struct StringSlice {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸreflectlite() => builtin.initPackage(typeof(@internal.reflectlite_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(sort_package.Float64Slice)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sortᴛFloat64Slice;
[GoMethodHost(typeof(sort_package.IntSlice)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sortᴛIntSlice;
[GoMethodHost(typeof(sort_package.StringSlice)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sortᴛStringSlice;
[GoMethodHost(typeof(sort_package.reverse)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sortᴛreverse;
[GoMethodHost(typeof(sort_package.xorshift)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sortᴛxorshift;
// </MethodHolders>
