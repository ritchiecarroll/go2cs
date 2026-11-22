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
using static go.encoding.binary_package;

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
[assembly: GoImplement<bigEndian, AppendByteOrder>]
[assembly: GoImplement<bigEndian, ByteOrder>]
[assembly: GoImplement<littleEndian, AppendByteOrder>]
[assembly: GoImplement<littleEndian, ByteOrder>]
[assembly: GoImplement<nativeEndian, AppendByteOrder>]
[assembly: GoImplement<nativeEndian, ByteOrder>]
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
[assembly: GoPositionMap("encoding/binary/binary.go", "binary.cs", "ACFCACdIkoKokoKCqJLOkoKokoKCgoKokgAGEpKCqpKCgoKCgoKCgqiSAA4Y1AAICpKCqJKCgqiSzpKCqJKCgoKCqJIABhKSgqqSgoKCgoKCgoKokgAOGNT01AADJAARBIKCgqaCyoKClIK0pIKmgoCCpIKuwoKCpoLKgoKUgrSkgqaClIKCpoKUtLS0tLS0tLS0tLSCxoLGtILGgsaCxoLGgsaCxoLGgtaUAAIWAAkEgoKCpoK4goKCpoKCgoKu1IKCpoK4goKCpoKUgoKu1IKCgriCgoKmgoKCpoKUgpTGgpTGgoKU2LS0gsa0tLS0tILGtLSCxrS0gsa0tILGtLSCxrS0gsa0tILGtLSCAAUQssSkgpSkgpSkgpSkpKTEpIKUpIKUpKTEpIKUpIKUpKTEpIKUpIKUpKSkpIKUpKSClKSklAAFEsKUgoCCtoKCgpTIgoCCpIKCpoLIqJKUgILYgoKCgpSUrKYADRiCgoLGgoKUlMaCgoLGgoLGgoKCxoKCxoKCgsaCgsaCgoLGgoLGtLS0tLS0tLSClIKCyIKC3IKU2oKCyLa0tLS2tLS0trS22gAIDoKUgoLIgoKUgpTagoLItrS0tLa0tLS2tLaCgrSCguiCxoKCgsyy9KSkpOSkpOSkpOSkpMTEpKSUqqKCgg==", "", "", "1153=uint32/1/2/1,Float32frombits/1/2/1,uint32/2/2/2,Float32frombits/2/2/2;1159=uint64/1/2/1,Float64frombits/1/2/1,uint64/2/2/2,Float64frombits/2/2/2")]
[assembly: GoPositionMap("encoding/binary/varint.go", "varint.cs", "ACJSooKClKqigoKCgpSCAAIS4oKCgqaUgoKUlIKUqqKCgpSqooKClAACEuKCgoKUppzCgoKCgoKClJSCgpSUgpSuwoKCgpQ=")]
// </GoSourcePositionMaps>

namespace go.encoding;

[GoPackage("binary")]
public static partial class binary_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct coder {}
    internal partial struct decoder {}
    internal partial struct encoder {}
    public partial interface AppendByteOrder {}
    public partial interface ByteOrder {}
    public partial struct bigEndian {}
    public partial struct littleEndian {}
    public partial struct nativeEndian {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(binary_package.bigEndian)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class binaryᴛbigEndian;
[GoMethodHost(typeof(binary_package.decoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class binaryᴛdecoder;
[GoMethodHost(typeof(binary_package.encoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class binaryᴛencoder;
[GoMethodHost(typeof(binary_package.littleEndian)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class binaryᴛlittleEndian;
[GoMethodHost(typeof(binary_package.nativeEndian)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class binaryᴛnativeEndian;
// </MethodHolders>
