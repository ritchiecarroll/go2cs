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
using static go.index.suffixarray_package;

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
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(go.index.suffixarray_package.Index))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("index/suffixarray/sais.go", "sais.cs", "AHmAArKClAACFgAIAoK4gpSCggACEOKCgpTugqaCgoKCgoKCAAgSlJSCpgACFgAIAoKUgqaCgoKUqqKCgoKCgoIAAxDCgoKCgoKCAAMyABUClIKCABo2goKCgoKCpoKCgoIADh6ClAACHAAMBIIAGDKCgoIACBKCgoKUgoKUlJSClNyCgoKmgoKClIIAAyAADQSCABUsgpSCgoKUlIKUgoLugoKCgqaCgoKUggADMAAUAgASJriCgoKCgoKCuIKCgpSCgqaCggAEHgAKAoKCgpSCgpSUlJSCgoKCgqaUgoKCgoKUAAIcAAsCgoKCgoIABBLCAChSgoKUlIKClNyCAAIWAAgCgqaCgoKCgoKmgsqCggADEMKC3IKCgoKUgoKClKaCgoKCggAEFvSCAAsYgoKCuIKCgpSCgpQACxiCgoKCuIKCgpSCuJSClIKUgoKmuIIACBKCgoKCuIKCgpSC")]
[assembly: GoPositionMap("index/suffixarray/sais2.go", "sais2.cs", "AAgSgoKUpoKCuIKUgoIAAhDigoKU7oKmgoKCgoKCggAIEpSUgqamgoK4gpSCggACEOKCgpTugqaCgoKCgoKCAAgSlJSCpqaCgriClIKCAAIQ4oKClO6CpoKCgoKCgoIACBKUlIKmpoKClIKmgoKClKaCgpSCpoKClKaCgpSCpoKClKaCgoKCgoKCuIKCgoKCuIKCgoKCuIKCgoKCgoK4goKCgoK4goKCgoK4gpSCggAaNoKCgoKCgqaCgoKCAA4egpSmgpSCABo2goKCgoKCpoKCgoIADh6ClKaClIIAGjaCgoKCgoKmgoKCggAOHoKUppSCABgygoKCAAgSgoKClIKClJSUgpTcgoKCpoKCgpSCuJQAGDKCgoIACBKCgoKUgoKUlJSClNyCgoKmgoKClIK4lAAYMoKCggAIEoKCgpSCgpSUlIKU3IKCgqaCgoKUgriUggAVLIKUgoKClJSClIKC7oKCgoKmgoKClIK4lAAVLIKUgoKClJSClIKC7oKCgoKmgoKClIK4lAAVLIKUgoKClJSClIKC7oKCgoKmgoKClIK4ggASJriCgoKCgoKCuIKCgpSCgqaCgsqCABUsgoKCgoKCuIKCgpSUgsqCABUsgoKCgoKCuIKCgpSUgsqCgoKClIKClJSUlIKCgoKCppSCgoKCgpSmgoKCgpSCgpSUlJSCgoKCgqaUgoKCgoKUpoKCgoKUgoKUlJSUgoKCgoKmlIKCgoKClKaCgoKCgoLKggAoUoKClJSCgpTcgqaCgqaCgoKCgoKmgsqCgriCgqaCgoKCgoKmgsqCgriCgqaCgoKCgoKmgsqCgriCgtyCgoKClIKCgpSmgoKCgoLKgtyCgoKClIKCgpSmgoKCgoLKgtyCgoKClIKCgpSmgoKCgoLKlIIACxiCgoK4goKClIKClAALGIKCgoK4goKClIK4lAALGIKCgriCgoKUgoKUAAsYgoKCgriCgoKUgriUAAsYgoKCuIKCgpSCgpQACxiCgoKCuIKCgpSCuJSClIKUgoKmuIIACBKCgoKCuIKCgpSCuJSUgpSCgqa4ggAIEoKCgoK4goKClIK4lJSClIKCpriCAAgSgoKCgriCgoKUgg==")]
[assembly: GoPositionMap("index/suffixarray/suffixarray.go", "suffixarray.cs", "ADNggqaCgpSmgoKUuIKClMqigoKClIKUqJKCgqiSgoKq5IKCgrimgqaY1JKCgpSUlKaCuIKCgqbspKaCgpSClKamgoKCgpS4griCuIKCgoKUlKikpoCCyICCyIKCgoKUlKqipoKq5qSSAAIQ8oKCgoKmgoKCgqaCygACEgAMBoKmgsrukoKClIKCgoKCgoKmgoKCgoKCpoKmpoKU7qaSgoKUgoKCgoKUlIKCgoKmpqaClA==", "246-246:1;248-248:2")]
// </GoSourcePositionMaps>

namespace go.index;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using bytes = bytes_package;
using binary = encoding.binary_package;
using errors = errors_package;
using io = io_package;
using math = math_package;
using regexp = regexp_package;
using slices = slices_package;
using sort = sort_package;
using encoding;
using static suffixarray_package;
// </GlyphForwarderUsings>

[GoPackage("suffixarray")]
public static partial class suffixarray_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct ints {}
    public partial struct Index {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(suffixarray_package.Index)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class suffixarrayᴛIndex;
[GoMethodHost(typeof(suffixarray_package.ints)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class suffixarrayᴛints;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class suffixarrayᴛints {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint len(this ref ints a) => suffixarray_package.lenᴍ(ref a);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 get(this ref ints a, nint i) => suffixarray_package.getᴍ(ref a, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void set(this ref ints a, nint i, int64 v) => suffixarray_package.setᴍ(ref a, i, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ints Δslice(this ref ints a, nint i, nint j) => suffixarray_package.Δsliceᴍ(ref a, i, j);
}
partial class suffixarrayᴛIndex {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Read(this ref Index x, io.Reader r) => suffixarray_package.Readᴍ(ref x, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Write(this ref Index x, io.Writer w) => suffixarray_package.Writeᴍ(ref x, w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<byte> Bytes(this ref Index x) => suffixarray_package.Bytesᴍ(ref x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> at(this ref Index x, nint i) => suffixarray_package.atᴍ(ref x, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ints lookupAll(this ж<Index> Ꮡx, slice<byte> s) => suffixarray_package.lookupAllᴍ(Ꮡx, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<nint> /*result*/ Lookup(this ж<Index> Ꮡx, slice<byte> s, nint n) => suffixarray_package.Lookupᴍ(Ꮡx, s, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<slice<nint>> /*result*/ FindAllIndex(this ж<Index> Ꮡx, ж<regexp.Regexp> Ꮡr, nint n) => suffixarray_package.FindAllIndexᴍ(Ꮡx, Ꮡr, n);
}
// </GlyphForwarders>
