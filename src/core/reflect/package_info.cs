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
global using abiꓸArrayType = go.@internal.abi_package.ΔArrayType;
global using abiꓸChanDir = go.@internal.abi_package.ΔChanDir;
global using abiꓸFuncType = go.@internal.abi_package.ΔFuncType;
global using abiꓸInterfaceType = go.@internal.abi_package.ΔInterfaceType;
global using abiꓸKind = go.@internal.abi_package.ΔKind;
global using abiꓸName = go.@internal.abi_package.ΔName;
global using abiꓸStructType = go.@internal.abi_package.ΔStructType;
global using mapsꓸMap = go.@internal.runtime.maps_package.ΔMap;
global using runtimeꓸError = go.runtime_package.ΔError;
using abi = go.@internal.abi_package;
// </ImportedTypeAliases>

using go;
using static go.reflect_package;

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
[assembly: GoDynamicTypeLift("7374727563747b6220626f6f6c3b207820616e797d", "dummyᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73796e632e4d757465783b206d2073796e632e4d61707d", "funcLookupCacheᴛ1")]
[assembly: GoTypeAlias("Bool", "const:ΔBool")]
[assembly: GoTypeAlias("ChanDir", "ΔChanDir")]
[assembly: GoTypeAlias("Int", "const:ΔInt")]
[assembly: GoTypeAlias("Interface", "const:ΔInterface")]
[assembly: GoTypeAlias("Kind", "ΔKind")]
[assembly: GoTypeAlias("Method", "ΔMethod")]
[assembly: GoTypeAlias("Pointer", "const:ΔPointer")]
[assembly: GoTypeAlias("Slice", "const:ΔSlice")]
[assembly: GoTypeAlias("String", "const:ΔString")]
[assembly: GoTypeAlias("Type", "ΔType")]
[assembly: GoTypeAlias("Uint", "const:ΔUint")]
[assembly: GoTypeAlias("UnsafePointer", "const:ΔUnsafePointer")]
[assembly: GoTypeAlias("Value", "ΔValue")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<rtype, ΔType>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<abi.Type, ж<abi.Type>>(Indirect = true)]
[assembly: GoImplicitConv<flag, abiꓸKind>(Inverted = false, ValueType = "uint8")]
[assembly: GoImplicitConv<flag, ΔKind>(Inverted = true, ValueType = "uintptr")]
[assembly: GoImplicitConv<ΔKind, abiꓸKind>(Inverted = false, ValueType = "uint8")]
[assembly: GoImplicitConv<ΔKind, flag>(Inverted = true, ValueType = "nuint")]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("reflect/abi.go", "abi.cs", "AFW4AYKClIKClIKCgqyygoKClJSu9IKCAA4egriCpoKClAACEgAJBIKSgoIABxCClIKClAAElgEACQKClIKUgpSCgoKU3JQAAhLigpSClILclK7CguwAIEiCgoKCgoKCgoKCgoKCgqaigoKClLgACBKWqIaSgoKCgpSmpoKCgpSCgoKCyqiCqIbsgoKCgpSCgu6CrNKs0qzSlKSkzrKUpKQ=")]
[assembly: go.GoPositionMap("reflect/badlinkname.go", "badlinkname.cs", "AAw6ABACAAIU9qampqampqampqampqampqampqampqampqampqam")]
[assembly: go.GoPositionMap("reflect/float32reg_generic.go", "float32reg_generic.cs", "AAogwoKmgg==")]
[assembly: go.GoPositionMap("reflect/iter.go", "iter.cs", "AAoWtoKUgqaClIIABRrygoKGppSkpKSkpKSkpKSktIKUgoKCguqCgoLagoKC2oKCgoLagoKC2gACENKCgoampIKUgoKCguqCgoLagoKC2oKCgoLa", "14-28:1;40-45:1;41-43:1.1;74-81:2;83-89:3;91-97:4;99-106:5;108-114:6;126-131:1;127-129:1.1;138-145:2;147-153:3;155-161:4;163-170:5")]
[assembly: go.GoPositionMap("reflect/makefunc.go", "makefunc.cs", "ABqcAQAPJPKCqIKCpoSWggAMHISmggACEAAPLAAOAqaCyg==")]
[assembly: go.GoPositionMap("reflect/map_swiss.go", "map_swiss.cs", "ABfEAQAHEoKUgpYACRSEAAkUgqYAA1jSgoKWgqzSgoKWABv+A9I=")]
[assembly: go.GoPositionMap("reflect/type.go", "type.cs", "AJ8ChgWCpoIADVCCpoKmgqaCpoKmggAVKoKClIKCgoKUpoKCpoIAFjSSqJKClKYAHUYACA4ACA4ACCAAEQiiqqKssqaipqKmoqaCAAQWgKSigpSCgpSmgKSApICkgoKClAAKsAGCAAQsggAEFoKmgoKClAAKPqKClIIAEHaigpSkpKaigpSkpKaigpSCgqSmooKUgoKkpqKUpKSkpqKClIKClIKClIKmopSkpKSmooKUgoKUgoKUggACKgASAvaClKSkpKjCgpSCgoKCgoKmgoKokKbSgpSCgoKCpgARKpIABCTSggACEgAICJSCgpSCgt6CgpSClIKWgoKClJSClIKEgoKClKaosoKUgoKCgoKUgIKkqIKCgoKUAAgSlAACEOzigoKCgoKUlJQACBYADRKCAAAQAAsOhIKCgsyCgriUgoKUgoKUgoK6lJSUgoKCgoLMgpSCgoKUgpSCgpSCgoKmgqaq9IKCgoKClIK4gpSqoqiSAAYa8AAEEKKCgqiAgriCgoKClIKawoKUgt6EhIKmgqiygpQABBqCgpSCpoKClKaCAAS4AQALCgACEAAJBIK6gpaCqKaygpaClgAJ+gEACQIAAiQADwKChIKokoKUgpQABhCCgoKUpgAkuAGigoKCgoKUgpYACRQAB8ABgqiygoKCgpSCgpSmgoKCpJSCgpSUgpSqwpSkpIKkgoKCpqbK0pSqpIKkgrKCpqbK0pSkgqSCgoKmpMzCgoKCgoKCggASdJIAAhLigoKWgqiokpSCgpSkpIKUpIKClKSCgoKmxqqigoKUAASyBYKssoKWpoKCqILKrNKUhIKSgoKmgpSCpgAG7gGCgpSCABJgAAwCgpSClIKAgoK4lgAIEoKWgoKUlJamyoIACBSSuIKmggAEVJKCgIKk", "1297-1297:1;2784-2786:1")]
[assembly: go.GoPositionMap("reflect/value.go", "value.cs", "AFCqAYKmgoKUrAAIDKyygpSClKiSgoKUlIKWgpKCgpS4tgAKDIIADDSCgpQAGNQBgqYAAxqClIKWpJKUgpSCgqQABCbSAAIQ0gARRJSCqoKCpJSWgpaCgoKCgpSClIKmgpSClIKmgoKmgoCCtpSCgoKCgoCCpJSCgoKWgoKUhqaGkoKCuKaEgoK8griAtKSCpKSktKiigriCgoKWkoKUlqaigriUlJSUuJKUgqQACAyCloSCqIKoloKWgoKCgqa4qIKCgqaClIKAuILKuKaCgpSCAAoYgoKUgqSCpIKkpLaoAAcuABcC3JSChJaCgoKCgpSCgoCCyoKClJSmpoKCgpSCpIKkgqSk7IKClLaogoKCqIKCgoKWgpaCAA8igoKClprCppamooLuuJKUgqQACRDMugACFAALAoKCgoKUgoKUgoKUgoKUgoKClIKClJKClK7CgpSCpJS8ogAILAAWAoIACBSCkqiCloC4xpKkpKTIgoKWgoKUAAoagIKUgoKUpqaCgqa2tKS2toCCgoKWpLSktra4lIKCgpSmgqSkpNqmgpa6AAkWgpSAgoKUyrqClrrYsoKCgpQABBqCgpSkpIKUpAAEJJKUpAAK1AGygpSCgoKCgpSmlK7CgpSCgoKCgpSmlKyygoCCpK7CgIKkqJKUpAAGIgADYpKUpAAGMpKClAAKwAHSAASaAaKClJSCgpSCgqaCgpSUgpSCgoKUlIIAABCUlAAEdqIABBqCgKSCpKSmpJKUtKrCpoKClAAEONKClIKUAAIQ0oKUgpSCgpQABBqigpSkpKqigpSkpKaCgpSqooKUgoKkqqKClIKCpAAIkgHSgoIABDyygoIAGOoCsoKCAA2WAoKCuAACENKCgq7CgoIABBiSpoKClKaCgpaCgrqClIKClIKmgoKUgqiSlKQABkYACQKClIKmggAfqgGCggAGFvIABCCSgpS0tIIAGKACACysAvYADIYBsoKUAA6IAfKClpiCgsa4lIKClIKUlLgABCaigoK4lILGgoLGrsKClKaUgoKmpKamgoKmpgAEGAAIAoKUgpaCloK6mKSkpKSkpLaCpIKUlIKCpraCkoKmtAADUQACVqrSlJSkpMiUpKTIlKSkyJTIgpSk2oKUpLqimKK4gsqCqIaWgoKUlqqigoKUpKSkpKqigoKUpKSokoKCgqqigoKUpKSmgoKCgqaCgoKCpoKCgoIAAhLiqJKokqiSqJKokqiSuJSokqiSgoCCpKiSgoCCpKiSqJKokqiSqJKCgpSCqJKCgpSCgoKCgoSokoKCgpSCgoKUqJKCgoKUlKiSgoKClKzGpgACFAAJBqSCgqaSlqamAAIYAAoCgoKopKKCgqimpqQAATgAGwSaysrM3NzWpJamqrKCAAoawoI=")]
[assembly: go.GoPositionMap("reflect/visiblefields.go", "visiblefields.cs", "AAcgAAkCgpSClNy4goKCgpSmlJQACR7SgpSCgoKCgoCCgsqCtqa2poKClIKClIKmlA==")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("reflect")]
public static partial class reflect_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoValueClone("inRegPtrs", "outRegPtrs")] internal partial struct abiDesc {}
    internal partial struct abiSeq {}
    internal partial struct abiStep {}
    internal partial struct abiStepKind {}
    internal partial struct bitVector {}
    internal partial struct cacheKey {}
    internal partial struct dummyᴛ1 {}
    internal partial struct fieldScan {}
    internal partial struct flag {}
    internal partial struct funcLookupCacheᴛ1 {}
    internal partial struct interfaceType {}
    internal partial struct layoutKey {}
    [GoValueClone("abid")] internal partial struct layoutType {}
    [GoValueClone("regPtrs")] internal partial struct makeFuncCtxt {}
    internal partial struct makeFuncImpl {}
    internal partial struct methodValue {}
    internal partial struct nonEmptyInterface {}
    internal partial struct ptrType {}
    internal partial struct rtype {}
    internal partial struct runtimeSelect {}
    internal partial struct sliceType {}
    internal partial struct structType {}
    internal partial struct structTypeUncommon {}
    internal partial struct visibleFieldsWalker {}
    internal partial struct visit {}
    internal partial struct Δcommon {}
    public partial interface ΔType {}
    public partial struct MapIter {}
    public partial struct SelectCase {}
    public partial struct SelectDir {}
    public partial struct SliceHeader {}
    public partial struct StringHeader {}
    public partial struct StructField {}
    public partial struct StructTag {}
    public partial struct ValueError {}
    public partial struct ΔChanDir {}
    public partial struct ΔKind {}
    public partial struct ΔMethod {}
    public partial struct ΔValue {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸabi() => builtin.initPackage(typeof(@internal.abi_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸbytealg() => builtin.initPackage(typeof(@internal.bytealg_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸrace() => builtin.initPackage(typeof(@internal.race_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸruntimeꓸmaps() => builtin.initPackage(typeof(@internal.runtime.maps_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸruntimeꓸsys() => builtin.initPackage(typeof(@internal.runtime.sys_package));
    [GoInit] internal static void initᴛᴛimportꓸiter() => builtin.initPackage(typeof(iter_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    // </ImportInitializers>
}
