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
using static go.@internal.runtime.gc.@internal.gen_package;

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
[assembly: GoImplement<@void, Value>]
[assembly: GoImplement<Mask64, Value>(Pointer = true)]
[assembly: GoImplement<Mask64, Value>]
[assembly: GoImplement<Mask8, Value>(Pointer = true)]
[assembly: GoImplement<Mask8, Value>]
[assembly: GoImplement<Ptr<@void>, Word>]
[assembly: GoImplement<Uint64x8, Value>(Pointer = true)]
[assembly: GoImplement<Uint64x8, Value>]
[assembly: GoImplement<Uint8x64, Value>(Pointer = true)]
[assembly: GoImplement<Uint8x64, Value>]
[assembly: GoImplement<locMem, loc>]
[assembly: GoImplement<locReg, loc>]
[assembly: GoImplement<opHeap, go.container.heap_package.Interface>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: go.GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: go.GoFieldAccess(typeof(go.@internal.runtime.gc.@internal.gen_package.Func))]
[assembly: go.GoFieldAccess(typeof(op))]
[assembly: go.GoFieldAccess(typeof(Δkind))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("internal/runtime/gc/internal/gen/gen.go", "gen.cs", "ABgqogAIEoKmggAHELgACRSCgqrkgoKCgoK4goKCptKCgoKCgqaigoKUggARKIKmgqaigpSCgqamgoKClIKUgpSClKaCgoIACgjG6oKmgoKUgqaClIKCgpSCgpSUgqaClJS6pIKCgpSClIKAgqSClKaCooKCgqaCuIKCgoKCgriokoKCkoKUgoK4gqTIgpSCgoKmgsoACRb6goKCgtyUlIKCgoKU7oKCgoKm5qKClLS07MqAooCigoKUlJSUlICSpKaCpoKCgoKCAAgI/oKCgoKClJSCppSCgoKUgoKCgoKCuIKCgoKCgoKm1qKmgoKCuKaCtqSktqaUgriUgoKUlIKUpoC4pJSCpoKUgoKCgpSmgriCgoKCgoKClKY=", "173-179:1;230-239:1;259-267:1;366-376:1")]
[assembly: go.GoPositionMap("internal/runtime/gc/internal/gen/gp.go", "gp.cs", "AAsWlKKCpoKmgoKC")]
[assembly: go.GoPositionMap("internal/runtime/gc/internal/gen/regalloc.go", "regalloc.cs", "AB9EgpSkpKSUpoKmggAIEoKClIKUgpSmgqaCgpQACBIADCCS3KKClIKClIKUgpSmgoKUgoKUgpSClKa0poKCAAgSgoKUgpTKgoKUgoKU7oKCgsqCgriClKaUgtKAgoKklIKmuIKCpIKUtIKUgoKCgoKUtIKUlJKClLSCtILo2oKCpoCUtoKCgsqCppSCuICCyIKCgriClLSCxoLGgraCpg==", "215-227:1")]
[assembly: go.GoPositionMap("internal/runtime/gc/internal/gen/simd.go", "simd.cs", "AAsWlLKCpoKmgoKCpqKCpqLclILKgoKmooKCpqKCgq7kgoKmtIKCprSCgu6UxIKmooKmooKmgqaCgoKmooKmtIKmooKmtIKmooKmtILulKKCgoKmgqaCgoKmooKmooKmooKUlO6UooKCgqaCpoKCgqaigqaigqaigpSUpqKClJQ=")]
[assembly: go.GoPositionMap("internal/runtime/gc/internal/gen/val.go", "val.cs", "ACpMlIAACQ6UpKKCgoKCgpSmooKCpsKCgoKUgqaygoKCgqaCAAgO5oKmpJLu7KKClIKUpoKAgoLagg==")]
// </GoSourcePositionMaps>

namespace go.@internal.runtime.gc.@internal;

[GoPackage("gen")]
public static partial class gen_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface loc {}
    internal partial struct @void {}
    internal partial struct fileConst {}
    internal partial struct locMem {}
    internal partial struct locReg {}
    internal partial struct opHeap {}
    internal partial struct regClass {}
    [GoValueClone("inUse")] internal partial struct regSet {}
    internal partial struct valAny {}
    internal partial struct valGP {}
    public partial interface Value {}
    public partial interface Word {}
    public partial interface Δwrap<T> {}
    public partial struct Array<T> {}
    public partial struct File {}
    public partial struct Func {}
    public partial struct Mask64 {}
    public partial struct Mask8 {}
    public partial struct Ptr<T> {}
    public partial struct Uint64 {}
    public partial struct Uint64x8 {}
    public partial struct Uint8x64 {}
    public partial struct op {}
    public partial struct Δkind {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸcontainerꓸheap() => builtin.initPackage(typeof(container.heap_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸhashꓸmaphash() => builtin.initPackage(typeof(hash.maphash_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(go.sync_package));
    // </ImportInitializers>
}
