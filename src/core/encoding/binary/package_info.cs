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
using reflect = go.reflect_package;
// </ImportedTypeAliases>

using go;
using static go.encoding.binary_package;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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
[assembly: GoPositionMap("encoding/binary/binary.go", "binary.cs", "ACBCACVIkoKokoKCqJLOkoKokoKCgoKokgAGEpKCqpKCgoKCgoKCgqiSAA4Y1MqSgqiSgoKoks6SgqiSgoKCgqiSAAYSkoKqkoKCgoKCgoKCqJIADhjU1NQAASQAEQSCgoKmgsqCgpSCtKSCpoKAgqSCrsKCgqaCyoKClIK0pIKmgpSCgqaClLS0tLS0tLS0tLS0gsaCxrSCxoLGgsaCxoLGgsaCxoLWlAACFgAJBIKCgqaCuIKCgqaCgoKCrtSCgqaCuIKCgqaClIKCrtSCgoK4goKCpoKCgqaClIKUxoKUxoKClNi0tILGtLS0tLSCxrS0gsa0tILGtLSCxrS0gsa0tILGtLSCxrS0ggAFELLEpIKUpIKUpIKUpKSkxKSClKSClKSkxKSClKSClKSkxKSClKSClKSkpKSClKSkgpSkpJQABRLClIKAgraCgoKUyIKAgqSCgqaCyKiSlICC2IKCgoKUlKymAAsYgoKCpoKClJSmgoKCpoKCpoKCgqaCgqaCgoKmgoKmgoKCpoKCppSUlJSUlJSUgpSCgsiCgtyClNqCgsi2tLS0trS0tLa0ttrugpSCgsiCgpSClNqCgsi2tLS0trS0tLa0toKCtIKCyIKmgoKCrLL0pKSk5KSk5KSk5KSkxMSkpJSqooKC", "", "", "1104=uint32/1/2/1,Float32frombits/1/2/1,uint32/2/2/2,Float32frombits/2/2/2;1110=uint64/1/2/1,Float64frombits/1/2/1,uint64/2/2/2,Float64frombits/2/2/2")]
[assembly: GoPositionMap("encoding/binary/varint.go", "varint.cs", "ACJSooKClKqigoKCgpSCAAIS4oKCgqaUgoKUlIKUqqKCgpSqooKClAACEuKCgoKUppzCgoKCgoKClJSCgpSUgpSuwoKCgpQ=")]
// </GoSourcePositionMaps>

namespace go.encoding;

using static go.encoding.binary_package;
using reflect = go.reflect_package;

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
[GoMethodHost(typeof(binary_package.bigEndian)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class binaryᴛbigEndian;
[GoMethodHost(typeof(binary_package.decoder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class binaryᴛdecoder;
[GoMethodHost(typeof(binary_package.encoder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class binaryᴛencoder;
[GoMethodHost(typeof(binary_package.littleEndian)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class binaryᴛlittleEndian;
[GoMethodHost(typeof(binary_package.nativeEndian)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class binaryᴛnativeEndian;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class binaryᴛbigEndian {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint16 Uint16(this bigEndian _Δp0, slice<byte> b) => binary_package.Uint16⁔(_Δp0, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void PutUint16(this bigEndian _Δp0, slice<byte> b, uint16 v) => binary_package.PutUint16⁔(_Δp0, b, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> AppendUint16(this bigEndian _, slice<byte> b, uint16 v) => binary_package.AppendUint16⁔(_, b, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint32 Uint32(this bigEndian _Δp0, slice<byte> b) => binary_package.Uint32⁔(_Δp0, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void PutUint32(this bigEndian _Δp0, slice<byte> b, uint32 v) => binary_package.PutUint32⁔(_Δp0, b, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> AppendUint32(this bigEndian _, slice<byte> b, uint32 v) => binary_package.AppendUint32⁔(_, b, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint64 Uint64(this bigEndian _Δp0, slice<byte> b) => binary_package.Uint64⁔(_Δp0, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void PutUint64(this bigEndian _Δp0, slice<byte> b, uint64 v) => binary_package.PutUint64⁔(_Δp0, b, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> AppendUint64(this bigEndian _, slice<byte> b, uint64 v) => binary_package.AppendUint64⁔(_, b, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this bigEndian _) => binary_package.String⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this bigEndian _) => binary_package.GoString⁔(_);
}
partial class binaryᴛdecoder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool @bool(this ref decoder d) => binary_package.bool⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint8 uint8(this ref decoder d) => binary_package.uint8⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint16 uint16(this ref decoder d) => binary_package.uint16⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 uint32(this ref decoder d) => binary_package.uint32⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 uint64(this ref decoder d) => binary_package.uint64⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int8 int8(this ref decoder d) => binary_package.int8⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int16 int16(this ref decoder d) => binary_package.int16⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int32 int32(this ref decoder d) => binary_package.int32⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 int64(this ref decoder d) => binary_package.int64⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void value(this ref decoder d, reflect.Value v) => binary_package.value⁔(ref d, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void skip(this ref decoder d, reflect.Value v) => binary_package.skip⁔(ref d, v);
}
partial class binaryᴛencoder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void @bool(this ref encoder e, bool x) => binary_package.bool⁔(ref e, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void uint8(this ref encoder e, uint8 x) => binary_package.uint8⁔(ref e, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void uint16(this ref encoder e, uint16 x) => binary_package.uint16⁔(ref e, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void uint32(this ref encoder e, uint32 x) => binary_package.uint32⁔(ref e, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void uint64(this ref encoder e, uint64 x) => binary_package.uint64⁔(ref e, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void int8(this ref encoder e, int8 x) => binary_package.int8⁔(ref e, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void int16(this ref encoder e, int16 x) => binary_package.int16⁔(ref e, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void int32(this ref encoder e, int32 x) => binary_package.int32⁔(ref e, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void int64(this ref encoder e, int64 x) => binary_package.int64⁔(ref e, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void value(this ref encoder e, reflect.Value v) => binary_package.value⁔(ref e, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void skip(this ref encoder e, reflect.Value v) => binary_package.skip⁔(ref e, v);
}
partial class binaryᴛlittleEndian {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint16 Uint16(this littleEndian _Δp0, slice<byte> b) => binary_package.Uint16⁔(_Δp0, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void PutUint16(this littleEndian _Δp0, slice<byte> b, uint16 v) => binary_package.PutUint16⁔(_Δp0, b, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> AppendUint16(this littleEndian _, slice<byte> b, uint16 v) => binary_package.AppendUint16⁔(_, b, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint32 Uint32(this littleEndian _Δp0, slice<byte> b) => binary_package.Uint32⁔(_Δp0, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void PutUint32(this littleEndian _Δp0, slice<byte> b, uint32 v) => binary_package.PutUint32⁔(_Δp0, b, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> AppendUint32(this littleEndian _, slice<byte> b, uint32 v) => binary_package.AppendUint32⁔(_, b, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint64 Uint64(this littleEndian _Δp0, slice<byte> b) => binary_package.Uint64⁔(_Δp0, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void PutUint64(this littleEndian _Δp0, slice<byte> b, uint64 v) => binary_package.PutUint64⁔(_Δp0, b, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> AppendUint64(this littleEndian _, slice<byte> b, uint64 v) => binary_package.AppendUint64⁔(_, b, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this littleEndian _) => binary_package.String⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this littleEndian _) => binary_package.GoString⁔(_);
}
partial class binaryᴛnativeEndian {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this nativeEndian _) => binary_package.String⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this nativeEndian _) => binary_package.GoString⁔(_);
}
// </MethodForwarders>
