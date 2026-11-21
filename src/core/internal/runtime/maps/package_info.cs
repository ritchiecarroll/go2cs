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
using static go.@internal.runtime.maps_package;

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
[assembly: GoImplement<unhashableTypeError, error>]
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
[assembly: GoFieldAccess(typeof(Map))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.EmptyInterface))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.ITab))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.InterfaceType))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.NonEmptyInterface))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.StructType))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.Type))]
[assembly: GoFieldAccess(typeof(table))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("internal/runtime/maps/group.go", "group.cs", "AC1gws7CyqKokszEgs7CzLLKosyyyKQAFCSSgpSokoKClKiSqqLOAAwSgsiSzgAKDoLKos4ACQyCyJKopM4ACQyCABo8kqyygpSCgpTIkqiylKiylAAQJrIACBDWlOqigpSCgoKUgoKmlIKCgpSCgg==", "", "", "360=newarray/1/1/2")]
[assembly: GoPositionMap("internal/runtime/maps/map.go", "map.cs", "ABLuAgCnAQKssgA1eoKClAACEAAIAoKmlAAOHu6CgqaCgoK4goKUgoK4gpSUlKaClKaCgpTGgoKUpoKmgqbGgpS4wqaCgoKCyoKmgpSCyoKUgoKmgqqi5qKCpoKmlIKmgqaigqaCppSCgqaCpqK4lIKUgoKmgoKClKbKpqKC7vKCpriUgqaCgpSClJTupoKCgoKmgpSU+KK4poKUgoKUgoKmgoKmlNyCgoKmlIKCgoKUlIKCgoKmgpSmgoKUpqailLiClKaCgqaCgqaUppSUgpSCprKCgpSmgqa4lIKUgoK4uKaClKaiuJSCgoKCgpSClJSClKaClO7KgpS6soKmgpSUgpSCgoKClIKUgqa4lIKUpqK4gpTmtIK4goKmlJSCgoKmgoKCgoKUlKbGooKUpqKClJSkgoKCgoKCgpSUgoKUgqaCpoKUxoKCgqakgoKClIKmtgAJDJS6")]
[assembly: GoPositionMap("internal/runtime/maps/runtime.go", "runtime.cs", "ABAmxqampqYAFCwACgKCgoKClIKUgqaCgpSmgqaUgoKClLiCpoKCgpSUgpSCgoKUgoKClJSmgqbMwoKCgoKUgpSCpoKClKaCppSCgoKUuIKmgoKClJSClIKCgpSCgoKUlKaCpgAIDMKClIKCgoKUgpSClIKmuJSCpoKClIKUlLimgoKUgpSaspSCgoKmgpSCgoKUgoKmgoKmgpTKgri4goKCpriCgoKCgoKUlIKCgoKmgoKClIKmggAHEKaCgoL8gpSU")]
[assembly: GoPositionMap("internal/runtime/maps/runtime_fast32.go", "runtime_fast32.cs", "AA8ewoKCgqaCpoKCpoKmgoKCgoKClIKUpoK2gqaCgoKUlIKUgoKClKaCpszCgoKCpoKmgoKmgqaCgoKCgoKUgpSmgraCpoKCgpSUgpSCgoKUpoKm6oK4poKUgoKClNyCgqaUgpSUgpTIwoKUgoKClIKmgsiUgqaCgpSClJS4poKClIKUmrKUgoKCpoKUgoKUgpTKgoKUgqaCgpTugoKCuIK4goKUlIKCgpSCpoLYgpSUAAIQAAgCgpSCgoKUgqaCyJSCpoKClIKUlLimgoKUgpSYopSCgoKmgpSCgpSClMqCgpSCpoKClO6CgoK4goKUlIKCgpSCpoLYgpSUqMKCgoKmgqY=")]
[assembly: GoPositionMap("internal/runtime/maps/runtime_fast64.go", "runtime_fast64.cs", "AA8ewoKCgqaCpoKCpoKmgoKCgoKClIKUpoK2gqaCgoKUlIKUgoKClKaCpszCgoKCpoKmgoKmgqaCgoKCgoKUgpSmgraCppSCgpSUgpSCgoKUpoKm6oK4poKUgoKClNyCgqaUgpSUgpTIwoKUgoKClIKmgsiUgqaCgpSClJS4poKClIKUmrKUgoKCpoKUgoKUgpTKgoKUgqaCgpTugoKCuIK4goKUlIKCgpSCpoLYgpSUxoK4poKUgoKClNyCgqaUgpSUgpTM4oKUgoKClIKmgsiUgqaCgpSClJS4poKClIKUmrKUgoKCpoKUgoKUgpTKgoKUgqaCgpTugoKCuIKClJSCgoKUgqaC2IKUlKjCgoKCpoKm")]
[assembly: GoPositionMap("internal/runtime/maps/runtime_faststr.go", "runtime_faststr.cs", "ABIesriCgtyUgoKCppSUgpSUpoKClKaEooKClIKClIKUzLKClKSCpoKCgpTkogACEMKCgoKmgqaCgqaCgoKUpoK2gqaCgoKUlIKUgoKClKaCpszCgoKCpoKmgoKmgoKClKaCtoKmgoKClJSClIKCgpSmgqbqgrimgpSClIKClNyCgqaUgpSUgpTIwoKUgoKClIKmgsiUgqaCgpSClJS4poKClIKUmrKUgoKCpoKUgqaClIKUyoKClIKmgoKU7oKCgriCuIKClJSCgoKUgqaC2IKUlKjCgoKCpoKm")]
[assembly: GoPositionMap("internal/runtime/maps/table.go", "table.cs", "AEWSAYKCpsqCyoKCppTKooKCgpSCgryipoLulIKUuoKqAAsOgoIAAhgAJjiCgoKUlIKUgoKUgoKClJSmgqbKooKCgpSUgpSCgpSCgoKUlKaCpgAEGAALApiilIKCgqaClIKClIKCpoKCpoKUyoKClIKmgoKU7oKCgriCuIKCgoKClJSCgoKCpoKCgpSCpoIAAygAEgKC7oKClIKClIKClKaCgpSmgoKCzMKCgoKClIKUgoKCpoKClJSCpqaClO4AAhYACAKCgpSCpoKUpoKmAAQmABEClKi2goKCgoKCgoKUuLiCgoKUgoKUuKbugoKClIKClIK4goKUgoKCgoKCAAYUsqiygoIADRyClJSCgoKClKaCgoKmggAjUMKUgqaCgpSCpoKCgoKCgqaCqJKssq7CpgAaNoKCgoKqooIAFzCCgoKUuKYABhTylIKCpoKCppSClJSmgoLugoKCgoKUgoKCpqaCpoKCuIKCgpSCgqYAHTyCgqa4lIKCggALGIKClO6ClAANHIKCyoKmpoKCpoKCgoK4lIKmgoK4goKCpoIAEiaCgoKUyoKmgpSmyqaCpoKClJSCpoKCAA0cgoKCgqaCuIK4goKUgqaCgsqCuKaCpoKC3IKCzAAUIIKCgqbIkoKUyNKCpoKmlIKCgpSmgoKmgoKmgoKClJS4gq7ylIKCgoKUpoKCpoKCppTKgoIAFyyCAAgOgoKCxrSCgqaCgoKCgpSU")]
[assembly: GoPositionMap("internal/runtime/maps/table_debug.go", "table_debug.cs", "ABAcsoKqooKCgoKCgpS0tJSCgsqCpoKCgoKCguyCgoKmgoKCgpSCgoKmgoKCpqIAFBKClIKCgpSCgpS0tLaCgoKCgu6igoKC")]
// </GoSourcePositionMaps>

namespace go.@internal.runtime;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using abi = go.@internal.abi_package;
using goarch = go.@internal.goarch_package;
using sys = go.@internal.runtime.sys_package;
using @unsafe = unsafe_package;
using go.@internal;
using go.@internal.runtime;
using static maps_package;
using math = go.@internal.runtime.math_package;
using race = go.@internal.race_package;
// </GlyphForwarderUsings>

[GoPackage("maps")]
public static partial class maps_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct bitset {}
    internal partial struct ctrl {}
    internal partial struct ctrlGroup {}
    internal partial struct groupReference {}
    internal partial struct groupsReference {}
    internal partial struct probeSeq {}
    [GoLocalName("stringStruct")] internal partial struct stringPtr_stringStruct {}
    internal partial struct unhashableTypeError {}
    public partial struct Iter {}
    public partial struct Map {}
    public partial struct table {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸabi() => builtin.initPackage(typeof(go.@internal.abi_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸrace() => builtin.initPackage(typeof(go.@internal.race_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸruntimeꓸsys() => builtin.initPackage(typeof(go.@internal.runtime.sys_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(maps_package.Iter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mapsᴛIter;
[GoMethodHost(typeof(maps_package.Map)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mapsᴛMap;
[GoMethodHost(typeof(maps_package.bitset)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mapsᴛbitset;
[GoMethodHost(typeof(maps_package.ctrlGroup)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mapsᴛctrlGroup;
[GoMethodHost(typeof(maps_package.groupReference)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mapsᴛgroupReference;
[GoMethodHost(typeof(maps_package.groupsReference)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mapsᴛgroupsReference;
[GoMethodHost(typeof(maps_package.probeSeq)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mapsᴛprobeSeq;
[GoMethodHost(typeof(maps_package.table)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mapsᴛtable;
[GoMethodHost(typeof(maps_package.unhashableTypeError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mapsᴛunhashableTypeError;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class mapsᴛbitset {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr first(this bitset b) => maps_package.firstᴍ(b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bitset removeFirst(this bitset b) => maps_package.removeFirstᴍ(b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bitset removeBelow(this bitset b, uintptr i) => maps_package.removeBelowᴍ(b, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool lowestSet(this bitset b) => maps_package.lowestSetᴍ(b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bitset shiftOutLowest(this bitset b) => maps_package.shiftOutLowestᴍ(b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint count(this bitset b) => maps_package.countᴍ(b);
}
partial class mapsᴛctrlGroup {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ctrl get(this ж<ctrlGroup> Ꮡg, uintptr i) => maps_package.getᴍ(Ꮡg, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void set(this ж<ctrlGroup> Ꮡg, uintptr i, ctrl c) => maps_package.setᴍ(Ꮡg, i, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setEmpty(this ref ctrlGroup g) => maps_package.setEmptyᴍ(ref g);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bitset matchH2(this ctrlGroup g, uintptr h) => maps_package.matchH2ᴍ(g, h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bitset matchEmpty(this ctrlGroup g) => maps_package.matchEmptyᴍ(g);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bitset matchEmptyOrDeleted(this ctrlGroup g) => maps_package.matchEmptyOrDeletedᴍ(g);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bitset matchFull(this ctrlGroup g) => maps_package.matchFullᴍ(g);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool anyFull(this ctrlGroup g) => maps_package.anyFullᴍ(g);
}
partial class mapsᴛgroupReference {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ctrlGroup> ctrls(this ref groupReference g) => maps_package.ctrlsᴍ(ref g);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @unsafe.Pointer key(this ref groupReference g, ж<abi.MapType> Ꮡtyp, uintptr i) => maps_package.keyᴍ(ref g, Ꮡtyp, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @unsafe.Pointer elem(this ref groupReference g, ж<abi.MapType> Ꮡtyp, uintptr i) => maps_package.elemᴍ(ref g, Ꮡtyp, i);
}
partial class mapsᴛgroupsReference {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static groupReference group(this ref groupsReference g, ж<abi.MapType> Ꮡtyp, uint64 i) => maps_package.groupᴍ(ref g, Ꮡtyp, i);
}
partial class mapsᴛMap {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr directoryIndex(this ref Map m, uintptr hash) => maps_package.directoryIndexᴍ(ref m, hash);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<table> directoryAt(this ref Map m, uintptr i) => maps_package.directoryAtᴍ(ref m, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void directorySet(this ref Map m, uintptr i, ж<table> nt) => maps_package.directorySetᴍ(ref m, i, nt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void replaceTable(this ref Map m, ж<table> Ꮡnt) => maps_package.replaceTableᴍ(ref m, Ꮡnt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void installTableSplit(this ref Map m, ж<table> Ꮡold, ж<table> Ꮡleft, ж<table> Ꮡright) => maps_package.installTableSplitᴍ(ref m, Ꮡold, Ꮡleft, Ꮡright);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static uint64 Used(this ref Map m) => maps_package.Usedᴍ(ref m);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (@unsafe.Pointer, bool) Get(this ref Map m, ж<abi.MapType> typ, @unsafe.Pointer key) => maps_package.Getᴍ(ref m, typ, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@unsafe.Pointer, @unsafe.Pointer, bool) getWithKey(this ref Map m, ж<abi.MapType> Ꮡtyp, @unsafe.Pointer key) => maps_package.getWithKeyᴍ(ref m, Ꮡtyp, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@unsafe.Pointer, bool) getWithoutKey(this ref Map m, ж<abi.MapType> Ꮡtyp, @unsafe.Pointer key) => maps_package.getWithoutKeyᴍ(ref m, Ꮡtyp, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@unsafe.Pointer, @unsafe.Pointer, bool) getWithKeySmall(this ref Map m, ж<abi.MapType> Ꮡtyp, uintptr hash, @unsafe.Pointer key) => maps_package.getWithKeySmallᴍ(ref m, Ꮡtyp, hash, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Put(this ж<Map> Ꮡm, ж<abi.MapType> Ꮡtyp, @unsafe.Pointer key, @unsafe.Pointer elem) => maps_package.Putᴍ(Ꮡm, Ꮡtyp, key, elem);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @unsafe.Pointer PutSlot(this ж<Map> Ꮡm, ж<abi.MapType> Ꮡtyp, @unsafe.Pointer key) => maps_package.PutSlotᴍ(Ꮡm, Ꮡtyp, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @unsafe.Pointer putSlotSmall(this ref Map m, ж<abi.MapType> Ꮡtyp, uintptr hash, @unsafe.Pointer key) => maps_package.putSlotSmallᴍ(ref m, Ꮡtyp, hash, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void growToSmall(this ref Map m, ж<abi.MapType> typ) => maps_package.growToSmallᴍ(ref m, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void growToTable(this ref Map m, ж<abi.MapType> Ꮡtyp) => maps_package.growToTableᴍ(ref m, Ꮡtyp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Delete(this ж<Map> Ꮡm, ж<abi.MapType> Ꮡtyp, @unsafe.Pointer key) => maps_package.Deleteᴍ(Ꮡm, Ꮡtyp, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void deleteSmall(this ref Map m, ж<abi.MapType> Ꮡtyp, uintptr hash, @unsafe.Pointer key) => maps_package.deleteSmallᴍ(ref m, Ꮡtyp, hash, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Clear(this ж<Map> Ꮡm, ж<abi.MapType> typ) => maps_package.Clearᴍ(Ꮡm, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void clearSmall(this ref Map m, ж<abi.MapType> Ꮡtyp) => maps_package.clearSmallᴍ(ref m, Ꮡtyp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Map> Clone(this ж<Map> Ꮡm, ж<abi.MapType> typ) => maps_package.Cloneᴍ(Ꮡm, typ);
}
partial class mapsᴛunhashableTypeError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void RuntimeError(this unhashableTypeError _) => maps_package.RuntimeErrorᴍ(_);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this unhashableTypeError e) => maps_package.Errorᴍ(e);
}
partial class mapsᴛMap {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @unsafe.Pointer putSlotSmallFast32(this ref Map m, ж<abi.MapType> typ, uintptr hash, uint32 key) => maps_package.putSlotSmallFast32ᴍ(ref m, typ, hash, key);
}
partial class mapsᴛMap {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @unsafe.Pointer putSlotSmallFast64(this ref Map m, ж<abi.MapType> typ, uintptr hash, uint64 key) => maps_package.putSlotSmallFast64ᴍ(ref m, typ, hash, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @unsafe.Pointer putSlotSmallFastPtr(this ref Map m, ж<abi.MapType> typ, uintptr hash, @unsafe.Pointer key) => maps_package.putSlotSmallFastPtrᴍ(ref m, typ, hash, key);
}
partial class mapsᴛMap {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @unsafe.Pointer getWithoutKeySmallFastStr(this ref Map m, ж<abi.MapType> Ꮡtyp, @string keyʗp) => maps_package.getWithoutKeySmallFastStrᴍ(ref m, Ꮡtyp, keyʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @unsafe.Pointer putSlotSmallFastStr(this ref Map m, ж<abi.MapType> typ, uintptr hash, @string key) => maps_package.putSlotSmallFastStrᴍ(ref m, typ, hash, key);
}
partial class mapsᴛtable {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void reset(this ref table t, ж<abi.MapType> typ, uint16 capacity) => maps_package.resetᴍ(ref t, typ, capacity);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint16 maxGrowthLeft(this ref table t) => maps_package.maxGrowthLeftᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static uint64 Used(this ref table t) => maps_package.Usedᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (@unsafe.Pointer, bool) Get(this ref table t, ж<abi.MapType> Ꮡtyp, ж<Map> Ꮡm, @unsafe.Pointer key) => maps_package.Getᴍ(ref t, Ꮡtyp, Ꮡm, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@unsafe.Pointer, @unsafe.Pointer, bool) getWithKey(this ref table t, ж<abi.MapType> Ꮡtyp, uintptr hash, @unsafe.Pointer key) => maps_package.getWithKeyᴍ(ref t, Ꮡtyp, hash, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@unsafe.Pointer, bool) getWithoutKey(this ref table t, ж<abi.MapType> Ꮡtyp, uintptr hash, @unsafe.Pointer key) => maps_package.getWithoutKeyᴍ(ref t, Ꮡtyp, hash, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (@unsafe.Pointer, bool) PutSlot(this ж<table> Ꮡt, ж<abi.MapType> Ꮡtyp, ж<Map> Ꮡm, uintptr hash, @unsafe.Pointer key) => maps_package.PutSlotᴍ(Ꮡt, Ꮡtyp, Ꮡm, hash, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void uncheckedPutSlot(this ref table t, ж<abi.MapType> Ꮡtyp, uintptr hash, @unsafe.Pointer key, @unsafe.Pointer elem) => maps_package.uncheckedPutSlotᴍ(ref t, Ꮡtyp, hash, key, elem);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Delete(this ref table t, ж<abi.MapType> Ꮡtyp, ж<Map> Ꮡm, uintptr hash, @unsafe.Pointer key) => maps_package.Deleteᴍ(ref t, Ꮡtyp, Ꮡm, hash, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void pruneTombstones(this ref table t, ж<abi.MapType> Ꮡtyp, ж<Map> Ꮡm) => maps_package.pruneTombstonesᴍ(ref t, Ꮡtyp, Ꮡm);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint16 tombstones(this ref table t) => maps_package.tombstonesᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Clear(this ref table t, ж<abi.MapType> Ꮡtyp) => maps_package.Clearᴍ(ref t, Ꮡtyp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void rehash(this ж<table> Ꮡt, ж<abi.MapType> typ, ж<Map> m) => maps_package.rehashᴍ(Ꮡt, typ, m);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void split(this ж<table> Ꮡt, ж<abi.MapType> Ꮡtyp, ж<Map> Ꮡm) => maps_package.splitᴍ(Ꮡt, Ꮡtyp, Ꮡm);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void grow(this ref table t, ж<abi.MapType> Ꮡtyp, ж<Map> Ꮡm, uint16 newCapacity) => maps_package.growᴍ(ref t, Ꮡtyp, Ꮡm, newCapacity);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<table> clone(this ж<table> Ꮡt, ж<abi.MapType> typ) => maps_package.cloneᴍ(Ꮡt, typ);
}
partial class mapsᴛIter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Init(this ref Iter it, ж<abi.MapType> Ꮡtyp, ж<Map> Ꮡm) => maps_package.Initᴍ(ref it, Ꮡtyp, Ꮡm);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Initialized(this ref Iter it) => maps_package.Initializedᴍ(ref it);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Map> Map(this ref Iter it) => maps_package.Mapᴍ(ref it);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @unsafe.Pointer Key(this ref Iter it) => maps_package.Keyᴍ(ref it);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @unsafe.Pointer Elem(this ref Iter it) => maps_package.Elemᴍ(ref it);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void nextDirIdx(this ref Iter it) => maps_package.nextDirIdxᴍ(ref it);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@unsafe.Pointer, @unsafe.Pointer, bool) grownKeyElem(this ref Iter it, @unsafe.Pointer key, uintptr slotIdx) => maps_package.grownKeyElemᴍ(ref it, key, slotIdx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Next(this ref Iter it) => maps_package.Nextᴍ(ref it);
}
partial class mapsᴛprobeSeq {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static probeSeq next(this probeSeq s) => maps_package.nextᴍ(s);
}
partial class mapsᴛtable {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void checkInvariants(this ref table t, ж<abi.MapType> Ꮡtyp, ж<Map> Ꮡm) => maps_package.checkInvariantsᴍ(ref t, Ꮡtyp, Ꮡm);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Print(this ref table t, ж<abi.MapType> Ꮡtyp, ж<Map> m) => maps_package.Printᴍ(ref t, Ꮡtyp, m);
}
// </GlyphForwarders>
