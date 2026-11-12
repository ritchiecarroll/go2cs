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
[assembly: GoPositionMap("internal/runtime/maps/group.go", "group.cs", "AC1gws7CyqKokszEgs7CzLLKosyyyKQAFCSygpSosoKClKiSqqLOAAwSgsiSzgAKDoLKos4ACQyCyJKopM4ACQyCABo8kqyygpSCgpTIkqiylKiylAAQJrIACBDWlOqigpSCgoKUgoKmlIKCgpSCgg==", "", "", "364=newarray/1/1/2")]
[assembly: GoPositionMap("internal/runtime/maps/map.go", "map.cs", "ABLuAgCnAQKssgA1eoKClAACEAAIAoKmlAAOHu6CgqaCgoK4goKUgoK4gpSUlKaClKaCgpTGgoKUpoKmgqbGgpS4wqaCgoKCyoKmgpSCyoKUgoKmgqqi5qKCpoKmlIKmgqaigqaCppSCgqaCpqK4lIKUgoKmgoKClKbKpqKC7vKCpriUgqaCgpSClJTupoKCgoKmgpSU+KK4poKUgoKUgoKmgoKmlNyCgoKmlIKCgoKUlIKCgoKmgpSmgoKUpqailLiClKaCgqaCgqaUppSUgpSCprKCgpSmgqa4lIKUgoK4uKaClKaiuJSCgoKCgpSClJSClKaClO7KgpS6soKmgpSUgpSCgoKClIKUgqa4lIKUpqK4gpTmtIK4goKmlJSCgoKmgoKCgoKUlKbGooKUpqKClJSkgoKCgoKCgpSUgoKUgqaCpoKUxoKCgqakgoKClIKmtgAJDJS6")]
[assembly: GoPositionMap("internal/runtime/maps/runtime.go", "runtime.cs", "ABAmxqampqYAFCwACgKCgoKClIKUgqaCgpSmgqaUgoKClLiCpoKCgpSUgpSCgoKUgoKClJSmgqbMwoKCgoKUgpSCpoKClKaCppSCgoKUuIKmgoKClJSClIKCgpSCgoKUlKaCpgAIDMKClIKCgoKUgpSClIKmuJSCpoKClIKUlLimgoKUgpSaspSCgoKmgpSCgoKUgoKmgoKmgpTKgri4goKCpriCgoKCgoKUlIKCgoKmgoKClIKmggAHEKaCgoL8gpSU")]
[assembly: GoPositionMap("internal/runtime/maps/runtime_fast32.go", "runtime_fast32.cs", "AA8ewoKCgqaCpoKCpoKmgoKCgoKClIKUpoK2gqaCgoKUlIKUgoKClKaCpszCgoKCpoKmgoKmgqaCgoKCgoKUgpSmgraCpoKCgpSUgpSCgoKUpoKm6oK4poKUgoKClNyCgqaUgpSUgpTIwoKUgoKClIKmgsiUgqaCgpSClJS4poKClIKUmrKUgoKCpoKUgoKUgpTKgoKUgqaCgpTugoKCuIK4goKUlIKCgpSCpoLYgpSUAAIQAAgCgpSCgoKUgqaCyJSCpoKClIKUlLimgoKUgpSYopSCgoKmgpSCgpSClMqCgpSCpoKClO6CgoK4goKUlIKCgpSCpoLYgpSUqMKCgoKmgqY=")]
[assembly: GoPositionMap("internal/runtime/maps/runtime_fast64.go", "runtime_fast64.cs", "AA8ewoKCgqaCpoKCpoKmgoKCgoKClIKUpoK2gqaCgoKUlIKUgoKClKaCpszCgoKCpoKmgoKmgqaCgoKCgoKUgpSmgraCppSCgpSUgpSCgoKUpoKm6oK4poKUgoKClNyCgqaUgpSUgpTIwoKUgoKClIKmgsiUgqaCgpSClJS4poKClIKUmrKUgoKCpoKUgoKUgpTKgoKUgqaCgpTugoKCuIK4goKUlIKCgpSCpoLYgpSUxoK4poKUgoKClNyCgqaUgpSUgpTM4oKUgoKClIKmgsiUgqaCgpSClJS4poKClIKUmrKUgoKCpoKUgoKUgpTKgoKUgqaCgpTugoKCuIKClJSCgoKUgqaC2IKUlKjCgoKCpoKm")]
[assembly: GoPositionMap("internal/runtime/maps/runtime_faststr.go", "runtime_faststr.cs", "ABIesriCgtyUgoKCppSUgpSUpoKClKaEooKClIKClIKUzLKClKSCpoKCgpTkogACEMKCgoKmgqaCgqaCgoKUpoK2gqaCgoKUlIKUgoKClKaCpszCgoKCpoKmgoKmgoKClKaCtoKmgoKClJSClIKCgpSmgqbqgrimgpSClIKClNyCgqaUgpSUgpTIwoKUgoKClIKmgsiUgqaCgpSClJS4poKClIKUmrKUgoKCpoKUgqaClIKUyoKClIKmgoKU7oKCgriCuIKClJSCgoKUgqaC2IKUlKjCgoKCpoKm")]
[assembly: GoPositionMap("internal/runtime/maps/table.go", "table.cs", "AEWSAYKCpsqCyoKCppTKooKCgpSCgryipoLulIKUuoKqAAsOgoIAAhgAJjiCgoKUlIKUgoKUgoKClJSmgqbKooKCgpSUgpSCgpSCgoKUlKaCpgAEGAALApiilIKCgqaClIKClIKCpoKCpoKUyoKClIKmgoKU7oKCgriCuIKCgoKClJSCgoKCpoKCgpSCpoIAAygAEgKC7oKClIKClIKClKaCgpSmgoKCzMKCgoKClIKUgoKCpoKClJSCpqaClO4AAhYACAKCgpSCpoKUpoKmAAQmABEClKi2goKCgoKCgoKUuLiCgoKUgoKUuKbugoKClIKClIK4goKUgoKCgoKCAAYUsqiygoIADRyClJSCgoKClKaCgoKmggAjUMKUgqaCgpSCpoKCgoKCgqaCqJKssq7CpgAaNoKCgoKqooIAFzCCgoKUuKYABhTylIKCpoKCppSClJSmgoLugoKCgoKUgoKCpqaCpoKCuIKCgpSCgqYAHTyCgqa4lIKCggALGIKClO6ClAANHIKCyoKmpoKCpoKCgoK4lIKmgoK4goKCpoIAEiaCgoKUyoKmgpSmyqaCpoKClJSCpoKCAA0cgoKCgqaCuIK4goKUgqaCgsqCuKaCpoKC3IKCzAAUIIKCgqbIkoKUyNKCpoKmlIKCgpSmgoKmgoKmgoKClJS4gq7ylIKCgoKUpoKCpoKCppTKgoIAFyyCAAgOgoKCxrSCgqaCgoKCgpSU")]
[assembly: GoPositionMap("internal/runtime/maps/table_debug.go", "table_debug.cs", "ABAcsoKqooKCgoKCgpS0tJSCgsqCpoKCgoKCguyCgoKmgoKCgpSCgoKmgoKCpqIAFBKClIKCgpSCgpS0tLaCgoKCgu6igoKC")]
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
