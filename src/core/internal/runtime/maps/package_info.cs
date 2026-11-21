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
using abi = go.@internal.abi_package;
// </ImportedTypeAliases>

using go;
using static go.@internal.runtime.maps_package;
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
[assembly: GoPositionMap("internal/runtime/maps/group.go", "group.cs", "ACpgwq7CqqKokqzEgq7CrLKqoqyyqKQAECSSgpSokoKClKiSqqKuAAwSgqiSrgAKDoKqoq4ACQyCqJKopK4ACQyCABo8kqyygpSCgpSokqiylKiylAAOJrIABhDWlMqigpSCgoKUgoKmlIKCgpSCgg==", "", "", "319=newarray/1/1/2")]
[assembly: GoPositionMap("internal/runtime/maps/map.go", "map.cs", "ABHuAgCnAQKssgA1eoKClAACEAAIAoKmlAAOHu6CgqaCgoK4goKUgoK4gpSUlKaClKaCgpSmgoKUpoKmgqbGgpS4wqaCgoKCyoKmgpSCyoKUgoKmgqqi5qKCpoKmlIKmgqaigqaCppSCgqaCpqK4lIKUgoKmgoKClKbKpqKC7vKCpriUgqaCgpSClJTupoKCgoKmgpSU+KK4poKUgoKUgoKmgoKmlNyCgoKmlIKCgoKUlIKCgoKmgpSmgoKUpqailLiClKaCgqaCgqaUppSUgpSCprKCgpSmgqa4lIKUgoK4uKaClKaiuJSCgoKCgpSClJSClKaClO7KgpS6soKmgpSUgpSCgoKClIKUgqa4lIKUpqK4gpTmtIK4goKmlJSCgoKmgoKCgoKUlKamooKUpqKClJSkgoKCgoKCgpSUgoKUgqaCpoKUxoKCgqakgoKClIKmtvyUmg==")]
[assembly: GoPositionMap("internal/runtime/maps/runtime.go", "runtime.cs", "ABAmxqampqYAECwACgKCgoKClIKUgqaCgpSmgqaUgoKClLiCpoKCgpSUgpSCgoKUgoKClJSmgqbMwoKCgoKUgpSCpoKClKaCppSCgoKUuIKmgoKClJSClIKCgpSCgoKUlKaCpszCgpSCgoKClIKUgpSCpriUgqaCgpSClJS4poKClIKUmrKUgoKCpoKUgoKClIKCpoKCpoKUyoK4uIKCgqa4goKCgoKClJSCgoKCpoKCgpSCpoIABxCmgoKC/IKUlA==")]
[assembly: GoPositionMap("internal/runtime/maps/runtime_fast32.go", "runtime_fast32.cs", "AA4ewoKCgqaCpoKCpoKmgoKCgoKClIKUpoK2gqaCgoKUlIKUgoKClKaCpszCgoKCpoKmgoKmgqaCgoKCgoKUgpSmgraCpoKCgpSUgpSCgoKUpoKmyoK4poKUgoKClNyCgqaUgpSUgpSowoKUgoKClIKmgsiUgqaCgpSClJS4poKClIKUmrKUgoKCpoKUgoKUgpTKgoKUgqaCgpTugoKCuIK4goKUlIKCgpSCpoLYgpSUAAIQAAgCgpSCgoKUgqaCyJSCpoKClIKUlLimgoKUgpSYopSCgoKmgpSCgpSClMqCgpSCpoKClO6CgoK4goKUlIKCgpSCpoLYgpSUqMKCgoKmgqY=")]
[assembly: GoPositionMap("internal/runtime/maps/runtime_fast64.go", "runtime_fast64.cs", "AA4ewoKCgqaCpoKCpoKmgoKCgoKClIKUpoK2gqaCgoKUlIKUgoKClKaCpszCgoKCpoKmgoKmgqaCgoKCgoKUgpSmgraCppSCgpSUgpSCgoKUpoKmyoK4poKUgoKClNyCgqaUgpSUgpSowoKUgoKClIKmgsiUgqaCgpSClJS4poKClIKUmrKUgoKCpoKUgoKUgpTKgoKUgqaCgpTugoKCuIK4goKUlIKCgpSCpoLYgpSUpoK4poKUgoKClNyCgqaUgpSUgpSs4oKUgoKClIKmgsiUgqaCgpSClJS4poKClIKUmrKUgoKCpoKUgoKUgpTKgoKUgqaCgpTugoKCuIKClJSCgoKUgqaC2IKUlKjCgoKCpoKm")]
[assembly: GoPositionMap("internal/runtime/maps/runtime_faststr.go", "runtime_faststr.cs", "AA8esriCgtyUgoKCppSUgpSUpoKClKaEooKClIKClIKUrLKClKSCpoKCgpTkogACEMKCgoKmgqaCgqaCgoKUpoK2gqaCgoKUlIKUgoKClKaCpszCgoKCpoKmgoKmgoKClKaCtoKmgoKClJSClIKCgpSmgqbKgrimgpSClIKClNyCgqaUgpSUgpSowoKUgoKClIKmgsiUgqaCgpSClJS4poKClIKUmrKUgoKCpoKUgqaClIKUyoKClIKmgoKU7oKCgriCuIKClJSCgoKUgqaC2IKUlKjCgoKCpoKm")]
[assembly: GoPositionMap("internal/runtime/maps/table.go", "table.cs", "AESSAYKCpsqCyoKCppSqooKCgpSCgryipoLulIKUuoKqAAsOgoIAAhgAJjiCgoKUlIKUgoKUgoKClJSmgqbKooKCgpSUgpSCgpSCgoKUlKaCpgAEGAALApiilIKCgqaClIKClIKCpoKCpoKUyoKClIKmgoKU7oKCgriCuIKCgoKClJSCgoKCpoKCgpSCpoIAAygAEgKC7oKClIKClIKClKaCgpSmgoKCzMKCgoKClIKUgoKCpoKClJSCpqaClO4AAhYACAKCgpSCpoKUpoKmAAQmABEClKi2goKCgoKCgoKUuLiCgoKUgoKUuKbugoKClIKClIK4goKUgoKCgoKCAAYUsqiygoIADRyClJSCgoKClKaCgoKmggAfUMKUgqaCgpSCpoKCgoKCgqaCqJKssq7CpgAaNoKCgoKqooIAFzCCgoKUuKYABhTylIKCpoKCppSClJSmgoLugoKCgoKUgoKCpqaCpoKCuIKCgpSCgqYAHTyCgqa4lIKCggALGIKClO6ClAANHIKCyoKmpoKCpoKCgoK4lIKmgoK4goKCpoIAEiaCgoKUyoKmgpSmyqaCpoKClJSCpoKCAA0cgoKCgqaCuIK4goKUgqaCgsqCuKaCpoKC3IKCrAAUIIKCgqaokoKUqNKCpoKmlIKCgpSmgoKmgoKmgoKClJS4gq7ylIKCgoKUpoKCpoKCppTKgoIAFSyC7oKCgqa0goKmgoKCgoKUlA==")]
[assembly: GoPositionMap("internal/runtime/maps/table_debug.go", "table_debug.cs", "AA0csoKqooKCgoKCgpS0tJSCgsqCpoKCgoKCguyCgoKmgoKCgpSCgoKmgoKCpqIAFBKClIKCgpSCgpS0tLaCgoKCgs6igoKC")]
// </GoSourcePositionMaps>

namespace go.@internal.runtime;

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
[GoMethodHost(typeof(maps_package.Iter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mapsᴛIter;
[GoMethodHost(typeof(maps_package.Map)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mapsᴛMap;
[GoMethodHost(typeof(maps_package.bitset)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mapsᴛbitset;
[GoMethodHost(typeof(maps_package.ctrlGroup)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mapsᴛctrlGroup;
[GoMethodHost(typeof(maps_package.groupReference)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mapsᴛgroupReference;
[GoMethodHost(typeof(maps_package.groupsReference)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mapsᴛgroupsReference;
[GoMethodHost(typeof(maps_package.probeSeq)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mapsᴛprobeSeq;
[GoMethodHost(typeof(maps_package.table)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mapsᴛtable;
[GoMethodHost(typeof(maps_package.unhashableTypeError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mapsᴛunhashableTypeError;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class mapsᴛIter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Init(this ref maps_package.Iter it, ж<abi.MapType> Ꮡtyp, ж<maps_package.Map> Ꮡm) => maps_package.Init⁔(ref it, Ꮡtyp, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Initialized(this ref maps_package.Iter it) => maps_package.Initialized⁔(ref it);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<maps_package.Map> Map(this ref maps_package.Iter it) => maps_package.Map⁔(ref it);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @unsafe.Pointer Key(this ref maps_package.Iter it) => maps_package.Key⁔(ref it);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @unsafe.Pointer Elem(this ref maps_package.Iter it) => maps_package.Elem⁔(ref it);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void nextDirIdx(this ref maps_package.Iter it) => maps_package.nextDirIdx⁔(ref it);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@unsafe.Pointer, @unsafe.Pointer, bool) grownKeyElem(this ref maps_package.Iter it, @unsafe.Pointer key, uintptr slotIdx) => maps_package.grownKeyElem⁔(ref it, key, slotIdx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Next(this ref maps_package.Iter it) => maps_package.Next⁔(ref it);
}
partial class mapsᴛMap {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr directoryIndex(this ref maps_package.Map m, uintptr hash) => maps_package.directoryIndex⁔(ref m, hash);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<maps_package.table> directoryAt(this ref maps_package.Map m, uintptr i) => maps_package.directoryAt⁔(ref m, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void directorySet(this ref maps_package.Map m, uintptr i, ж<maps_package.table> Ꮡnt) => maps_package.directorySet⁔(ref m, i, Ꮡnt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void replaceTable(this ref maps_package.Map m, ж<maps_package.table> Ꮡnt) => maps_package.replaceTable⁔(ref m, Ꮡnt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void installTableSplit(this ref maps_package.Map m, ж<maps_package.table> Ꮡold, ж<maps_package.table> Ꮡleft, ж<maps_package.table> Ꮡright) => maps_package.installTableSplit⁔(ref m, Ꮡold, Ꮡleft, Ꮡright);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint64 Used(this ref maps_package.Map m) => maps_package.Used⁔(ref m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (@unsafe.Pointer, bool) Get(this ref maps_package.Map m, ж<abi.MapType> Ꮡtyp, @unsafe.Pointer key) => maps_package.Get⁔(ref m, Ꮡtyp, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@unsafe.Pointer, @unsafe.Pointer, bool) getWithKey(this ref maps_package.Map m, ж<abi.MapType> Ꮡtyp, @unsafe.Pointer key) => maps_package.getWithKey⁔(ref m, Ꮡtyp, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@unsafe.Pointer, bool) getWithoutKey(this ref maps_package.Map m, ж<abi.MapType> Ꮡtyp, @unsafe.Pointer key) => maps_package.getWithoutKey⁔(ref m, Ꮡtyp, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@unsafe.Pointer, @unsafe.Pointer, bool) getWithKeySmall(this ref maps_package.Map m, ж<abi.MapType> Ꮡtyp, uintptr hash, @unsafe.Pointer key) => maps_package.getWithKeySmall⁔(ref m, Ꮡtyp, hash, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Put(this ж<maps_package.Map> Ꮡm, ж<abi.MapType> Ꮡtyp, @unsafe.Pointer key, @unsafe.Pointer elem) => maps_package.Put⁔(Ꮡm, Ꮡtyp, key, elem);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @unsafe.Pointer PutSlot(this ж<maps_package.Map> Ꮡm, ж<abi.MapType> Ꮡtyp, @unsafe.Pointer key) => maps_package.PutSlot⁔(Ꮡm, Ꮡtyp, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @unsafe.Pointer putSlotSmall(this ref maps_package.Map m, ж<abi.MapType> Ꮡtyp, uintptr hash, @unsafe.Pointer key) => maps_package.putSlotSmall⁔(ref m, Ꮡtyp, hash, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void growToSmall(this ref maps_package.Map m, ж<abi.MapType> Ꮡtyp) => maps_package.growToSmall⁔(ref m, Ꮡtyp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void growToTable(this ref maps_package.Map m, ж<abi.MapType> Ꮡtyp) => maps_package.growToTable⁔(ref m, Ꮡtyp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Delete(this ж<maps_package.Map> Ꮡm, ж<abi.MapType> Ꮡtyp, @unsafe.Pointer key) => maps_package.Delete⁔(Ꮡm, Ꮡtyp, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void deleteSmall(this ref maps_package.Map m, ж<abi.MapType> Ꮡtyp, uintptr hash, @unsafe.Pointer key) => maps_package.deleteSmall⁔(ref m, Ꮡtyp, hash, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Clear(this ж<maps_package.Map> Ꮡm, ж<abi.MapType> Ꮡtyp) => maps_package.Clear⁔(Ꮡm, Ꮡtyp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void clearSmall(this ref maps_package.Map m, ж<abi.MapType> Ꮡtyp) => maps_package.clearSmall⁔(ref m, Ꮡtyp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<maps_package.Map> Clone(this ж<maps_package.Map> Ꮡm, ж<abi.MapType> Ꮡtyp) => maps_package.Clone⁔(Ꮡm, Ꮡtyp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @unsafe.Pointer putSlotSmallFast32(this ref maps_package.Map m, ж<abi.MapType> Ꮡtyp, uintptr hash, uint32 key) => maps_package.putSlotSmallFast32⁔(ref m, Ꮡtyp, hash, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @unsafe.Pointer putSlotSmallFast64(this ref maps_package.Map m, ж<abi.MapType> Ꮡtyp, uintptr hash, uint64 key) => maps_package.putSlotSmallFast64⁔(ref m, Ꮡtyp, hash, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @unsafe.Pointer putSlotSmallFastPtr(this ref maps_package.Map m, ж<abi.MapType> Ꮡtyp, uintptr hash, @unsafe.Pointer key) => maps_package.putSlotSmallFastPtr⁔(ref m, Ꮡtyp, hash, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @unsafe.Pointer getWithoutKeySmallFastStr(this ref maps_package.Map m, ж<abi.MapType> Ꮡtyp, @string keyʗp) => maps_package.getWithoutKeySmallFastStr⁔(ref m, Ꮡtyp, keyʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @unsafe.Pointer putSlotSmallFastStr(this ref maps_package.Map m, ж<abi.MapType> Ꮡtyp, uintptr hash, @string key) => maps_package.putSlotSmallFastStr⁔(ref m, Ꮡtyp, hash, key);
}
partial class mapsᴛbitset {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr first(this maps_package.bitset b) => maps_package.first⁔(b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static maps_package.bitset removeFirst(this maps_package.bitset b) => maps_package.removeFirst⁔(b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static maps_package.bitset removeBelow(this maps_package.bitset b, uintptr i) => maps_package.removeBelow⁔(b, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool lowestSet(this maps_package.bitset b) => maps_package.lowestSet⁔(b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static maps_package.bitset shiftOutLowest(this maps_package.bitset b) => maps_package.shiftOutLowest⁔(b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint count(this maps_package.bitset b) => maps_package.count⁔(b);
}
partial class mapsᴛctrlGroup {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static maps_package.ctrl get(this ж<maps_package.ctrlGroup> Ꮡg, uintptr i) => maps_package.get⁔(Ꮡg, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void set(this ж<maps_package.ctrlGroup> Ꮡg, uintptr i, maps_package.ctrl c) => maps_package.set⁔(Ꮡg, i, c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setEmpty(this ref maps_package.ctrlGroup g) => maps_package.setEmpty⁔(ref g);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static maps_package.bitset matchH2(this maps_package.ctrlGroup g, uintptr h) => maps_package.matchH2⁔(g, h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static maps_package.bitset matchEmpty(this maps_package.ctrlGroup g) => maps_package.matchEmpty⁔(g);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static maps_package.bitset matchEmptyOrDeleted(this maps_package.ctrlGroup g) => maps_package.matchEmptyOrDeleted⁔(g);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static maps_package.bitset matchFull(this maps_package.ctrlGroup g) => maps_package.matchFull⁔(g);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool anyFull(this maps_package.ctrlGroup g) => maps_package.anyFull⁔(g);
}
partial class mapsᴛgroupReference {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<maps_package.ctrlGroup> ctrls(this ref maps_package.groupReference g) => maps_package.ctrls⁔(ref g);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @unsafe.Pointer key(this ref maps_package.groupReference g, ж<abi.MapType> Ꮡtyp, uintptr i) => maps_package.key⁔(ref g, Ꮡtyp, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @unsafe.Pointer elem(this ref maps_package.groupReference g, ж<abi.MapType> Ꮡtyp, uintptr i) => maps_package.elem⁔(ref g, Ꮡtyp, i);
}
partial class mapsᴛgroupsReference {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static maps_package.groupReference group(this ref maps_package.groupsReference g, ж<abi.MapType> Ꮡtyp, uint64 i) => maps_package.group⁔(ref g, Ꮡtyp, i);
}
partial class mapsᴛprobeSeq {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static maps_package.probeSeq next(this maps_package.probeSeq s) => maps_package.next⁔(s);
}
partial class mapsᴛtable {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void reset(this ref maps_package.table t, ж<abi.MapType> Ꮡtyp, uint16 capacity) => maps_package.reset⁔(ref t, Ꮡtyp, capacity);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint16 maxGrowthLeft(this ref maps_package.table t) => maps_package.maxGrowthLeft⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint64 Used(this ref maps_package.table t) => maps_package.Used⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (@unsafe.Pointer, bool) Get(this ref maps_package.table t, ж<abi.MapType> Ꮡtyp, ж<maps_package.Map> Ꮡm, @unsafe.Pointer key) => maps_package.Get⁔(ref t, Ꮡtyp, Ꮡm, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@unsafe.Pointer, @unsafe.Pointer, bool) getWithKey(this ref maps_package.table t, ж<abi.MapType> Ꮡtyp, uintptr hash, @unsafe.Pointer key) => maps_package.getWithKey⁔(ref t, Ꮡtyp, hash, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@unsafe.Pointer, bool) getWithoutKey(this ref maps_package.table t, ж<abi.MapType> Ꮡtyp, uintptr hash, @unsafe.Pointer key) => maps_package.getWithoutKey⁔(ref t, Ꮡtyp, hash, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (@unsafe.Pointer, bool) PutSlot(this ж<maps_package.table> Ꮡt, ж<abi.MapType> Ꮡtyp, ж<maps_package.Map> Ꮡm, uintptr hash, @unsafe.Pointer key) => maps_package.PutSlot⁔(Ꮡt, Ꮡtyp, Ꮡm, hash, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void uncheckedPutSlot(this ref maps_package.table t, ж<abi.MapType> Ꮡtyp, uintptr hash, @unsafe.Pointer key, @unsafe.Pointer elem) => maps_package.uncheckedPutSlot⁔(ref t, Ꮡtyp, hash, key, elem);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Delete(this ref maps_package.table t, ж<abi.MapType> Ꮡtyp, ж<maps_package.Map> Ꮡm, uintptr hash, @unsafe.Pointer key) => maps_package.Delete⁔(ref t, Ꮡtyp, Ꮡm, hash, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void pruneTombstones(this ref maps_package.table t, ж<abi.MapType> Ꮡtyp, ж<maps_package.Map> Ꮡm) => maps_package.pruneTombstones⁔(ref t, Ꮡtyp, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint16 tombstones(this ref maps_package.table t) => maps_package.tombstones⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Clear(this ref maps_package.table t, ж<abi.MapType> Ꮡtyp) => maps_package.Clear⁔(ref t, Ꮡtyp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void rehash(this ж<maps_package.table> Ꮡt, ж<abi.MapType> Ꮡtyp, ж<maps_package.Map> Ꮡm) => maps_package.rehash⁔(Ꮡt, Ꮡtyp, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void split(this ж<maps_package.table> Ꮡt, ж<abi.MapType> Ꮡtyp, ж<maps_package.Map> Ꮡm) => maps_package.split⁔(Ꮡt, Ꮡtyp, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void grow(this ref maps_package.table t, ж<abi.MapType> Ꮡtyp, ж<maps_package.Map> Ꮡm, uint16 newCapacity) => maps_package.grow⁔(ref t, Ꮡtyp, Ꮡm, newCapacity);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<maps_package.table> clone(this ж<maps_package.table> Ꮡt, ж<abi.MapType> Ꮡtyp) => maps_package.clone⁔(Ꮡt, Ꮡtyp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void checkInvariants(this ref maps_package.table t, ж<abi.MapType> Ꮡtyp, ж<maps_package.Map> Ꮡm) => maps_package.checkInvariants⁔(ref t, Ꮡtyp, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Print(this ref maps_package.table t, ж<abi.MapType> Ꮡtyp, ж<maps_package.Map> Ꮡm) => maps_package.Print⁔(ref t, Ꮡtyp, Ꮡm);
}
partial class mapsᴛunhashableTypeError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void RuntimeError(this maps_package.unhashableTypeError _) => maps_package.RuntimeError⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this maps_package.unhashableTypeError e) => maps_package.Error⁔(e);
}
// </MethodForwarders>
