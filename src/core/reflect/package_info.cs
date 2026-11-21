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
[assembly: GoDynamicTypeLift("696e746572666163657b4d28297d", "packIfaceValueIntoEmptyIface_type")]
[assembly: GoDynamicTypeLift("7374727563747b6220626f6f6c3b207820616e797d", "dummyᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73796e632e4d757465783b206d2073796e632e4d61707d", "funcLookupCacheᴛ1")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<rtype, go.reflect_package.Type>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<Kind, abi.Kind>(Inverted = false, ValueType = "uint8")]
[assembly: GoImplicitConv<Kind, flag>(Inverted = true, ValueType = "nuint")]
[assembly: GoImplicitConv<flag, Kind>(Inverted = true, ValueType = "uintptr")]
[assembly: GoImplicitConv<flag, abi.Kind>(Inverted = false, ValueType = "uint8")]
// </ImplicitConversions>

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(StructField))]
[assembly: GoFieldAccess(typeof(abiStep))]
[assembly: GoFieldAccess(typeof(bitVector))]
[assembly: GoFieldAccess(typeof(funcType))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.FuncType))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.ITab))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.Imethod))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.RegArgs))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.StructField))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.Type))]
[assembly: GoFieldAccess(typeof(go.@internal.unsafeheader_package.Slice))]
[assembly: GoFieldAccess(typeof(interfaceType))]
[assembly: GoFieldAccess(typeof(makeFuncCtxt))]
[assembly: GoFieldAccess(typeof(methodValue))]
[assembly: GoFieldAccess(typeof(nonEmptyInterface))]
[assembly: GoFieldAccess(typeof(rtype))]
[assembly: GoFieldAccess(typeof(structType))]
[assembly: GoFieldAccess(typeof(visibleFieldsWalker))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("reflect/abi.go", "abi.cs", "AFe4AYKClIKClIKCgqyygoKClJSu9IKCAA4egriCpoKClAACEgAJBIKSgoIABxCClIKClAAElgEACQKClIKUgpSCgoKU3JQAAhLigpSClILclK7CgtwAJEiCgoKCgoKCgoKCgoKCgsaigoKClLgACBKmuJaSgoKCgpSmpoKCgpSCgoKCyriCuJbsgoKCgpSCgu6CrNKs0qzSlLS0zrKUtLQ=")]
[assembly: GoPositionMap("reflect/badlinkname.go", "badlinkname.cs", "AAw6ABICAAIU9qampqampqampqampqampqampqampqampqampqam")]
[assembly: GoPositionMap("reflect/float32reg_generic.go", "float32reg_generic.cs", "AAogwoKmgg==")]
[assembly: GoPositionMap("reflect/iter.go", "iter.cs", "AAwWtoKUgqaClIIABxrygoKGppSkpKSkpKSkpKSkpIKUgoKCgtqCgoLagoKC2oKCgoLagoKCygACENKCgoamlIKUgoKCgtqCgoLagoKC2oKCgoLK", "14-28:1;40-45:1;41-43:1.1;74-81:2;83-89:3;91-97:4;99-106:5;108-114:6;126-131:1;127-129:1.1;138-145:2;147-153:3;155-161:4;163-170:5")]
[assembly: GoPositionMap("reflect/makefunc.go", "makefunc.cs", "ABueAQAPJPKCuIKCppSmggANHJSmggACEAATLAALAu6Cgso=")]
[assembly: GoPositionMap("reflect/map.go", "map.cs", "ABi2AQAIEoKUgqYACRSUAAkUgqYAB1jSgoKmgqzSgoKmAC32A9I=", "", "", "53=TypeFor/1/1/3,ArrayOf/1/1/7")]
[assembly: GoPositionMap("reflect/type.go", "type.cs", "ANkCxgWCpoIAElCCxoKmgsaCpoKmggAZKoKClIKCgoKUpoKCpoIAGDSSyJKClMYAHUYACA4ACA4ACCAAEQiiqqKsssaipqKmosaCAAYWlKKClIKClKaUlJSCgoKUAAywAYIACCyCAAgWgqaCgoKUAAw+ooKUggAQdqKClKSUpqKClKSUpqKClIKClKaigpSCgpSmopSkpJTGooKUgoKUgoKUgsailKSklMaigpSCgpSCgpSCxqKClIKCgtyCgoKC3KKClIKCgtyigpSCgoIABzAAEgIADAaClKSklMjCgpSCgoKCgoKmgoKogJbSgpSCgoKCpgAVKpIAECTSggACEgAJCJSCgpSCgu6CgpSClIKmgoKClJSClIKUgoKClKbIsoKUgoKCgoKUgpS4goKCgpQACBKUAAQQAAgM4oKCgoKClJSUAAwWAA4SggABEAAMDpSCgoLcgoK4lIKClIKClIKCypSUlIKCgoKC3IKUgoKClIKUgoKUgoKCpoKmqvSCgoKCgpSCuIKUyqIABBSSAAYeAAgAAAUQooKCuIK4goKCgpSCqsKClILulJSCxoKosoKUAAYagoKUgqaCgpSmggAGuAEACAoAAhAACQSCyoKmgrimsoKmgqYACvoBAAkCAAIkABECgpSCuJKClIKUAAcQgoKClKYAI7gBooKCgoKClIKmAAkUAAbAAYKowoKCgoKUgoKUpoKCgoKClIKClJSClKrClKSkgqSCgoKmtsqylNqkgqSCgoKmtsqylKSCpIKCgqakvMKCgoKCgoKCABJ0kgACEuKCgqaCuKiSlIKClKSkgpSkgoKUpIKCgqa2qqKCgpQABLAFgqyygqamgoK4gsqs0pSUgoKCgqaClIKmAAbsAYKClIIAEmAADAKClIKUgoKCuKYACBKCpoKClJSmpsqCAAoUkriCpoI=", "964-970:1;974-980:1;987-993:1;1000-1006:1;1378-1378:1;2871-2873:1", "", "1620=TypeOf/1/2/3,TypeOf/2/2/7,ArrayOf/1/1/7;1900=IsExported/1/1/1,newName/1/1/1,common/1/1/2;1992=align/1/1/6")]
[assembly: GoPositionMap("reflect/value.go", "value.cs", "AFWsAYKmgoKUzAAIDKyygpSClMiSAAUQooKUgqaCgoKClMi2AA80goKUAB7UAYLGAAUagpSCpqSClIKUgoKUAAQm0gACENIAGUiUgqqCgoKClKaCpoKCgoKClIKUgqaClIKUgqaCgqaCgqaUgoKCgoKClJSCgoKmgoKUlraWkoKCuKaUgoLcgri0pIKkpKSUuKKCuIKCgqaCgpSmxoKCuJSUlJTYgpSCtAAIDIKmlIK4grimgqaCgoKCpri4goKCpoKUgoC4gsrIpoKClIIACxiCgpSCtIK0grS0trgAEC4AFwLclIKUpoKCgoKClIKCgsqCgpSUpqaCgoKUgrSCtIK0tOyCgpSmuIKCgriCgoKCloKWggAQIoKCgqbKgqamxoKC7tiClIK0AAoQ3MoAAhQACwKCgoKClIKClIKClIKClIKCgpSCgpSCgpSuwoKUgoKClLyiAAwsABUCggAJFIKSuIKm2LaCpKSkuIKCpoKClAAMGoCClIKClKamgoK2tqSkpraAgoKCprS0tLa2uJSCgoKUtoKkpKTKpoKmygAKFoKUgoKUysqCpsrosoKCgpQABhqCgpSkpIKUpAAEJJKUpAAJ3AGygpSCgoKCgpSmlK7CgpSCgoKCgpSmlKyygoKUrsKClKiSlKQAByIABWKSlKQABTKSgpQADuQB1oKUAAh00gAGpgGigpSUgoKUgoKmgoKUlIKUgoKClJSCAAAQlJQABnaiAAQagqSCpKS2pIKUpMrCpoKClAAGPtKClIKUAAIYAAkCgpSClIKClAAEGqKClKSUqqKClKSUxoKClMqigpSCgpSqooKUgoKUAAiSAdKCggAEPLKCggAY6gKygoIADpYCgoK4AAIQ0oKCrsKCggAEGJKmgoKUpoKCpoKCyoKUgoKUgqaCgpSCqJKUpAAN2AHigoKUooKCAAUiAAsCkoKCggAiQoKCAAcW8gAGIJKClLS0ggAZoAIALawC9gAMhgGygpQAEIgB8oKmuIKCpriUgoKUgpSUuAAEJqKCgriUgsaCgrauwoKUppSCgqaUpqaCgqamAAMYAAgCgpSCpoKmgsqUtKSkpKSkpLaClIKUlIKCpraCgoKmpKTK0pSUpKTIlKSkyJSkpMiUyIKUpNqClKTKgriCyILagriGpoKClKaqooKClLS0tKSqooKClLSkqJKCgoKqooKClLSkpoKCgoKmgoKCgqaCgoKCAAIS4qiSqJKokqiSqJKokriUqJKokoKClKiSgoKUqJKokqiSqJKokoKClIKokoKClIKCgoKClKiSgoKClIKCgpSokoKCgpSUqJKCgoKUrMamAAIUAAkGpIKCpoKWpqYAAhgACgKCgqikooKCqKampAABOAAbBJrKyszc3NaklqaqsoIACRrCgg==", "2649-2655:1;2670-2677:1", "", "128=typ/1/1/1,packEfaceData/1/1/2;1004=add/1/2/1,add/2/2/2;2253=Kind/2/2/1,nameFor/2/2/1,elem/1/2/2,elem/2/2/2,haveIdenticalUnderlyingType/1/1/2")]
[assembly: GoPositionMap("reflect/visiblefields.go", "visiblefields.cs", "AAkgAAkCgpSClNy4goKCgpSmlJQACx7SgpSCgoKCgoKCyoKClKampoKClIKClIKmlA==", "", "", "26=NumField/1/1/3")]
// </GoSourcePositionMaps>

namespace go;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using @unsafe = unsafe_package;
using @internal;
using static reflect_package;
using iter = iter_package;
using reflect = reflect_package;
using race = @internal.race_package;
using maps = @internal.runtime.maps_package;
using sys = @internal.runtime.sys_package;
using @internal.runtime;
using runtime = runtime_package;
using strconv = strconv_package;
using Δsync = sync_package;
using Δunicode = unicode_package;
using utf8 = go.unicode.utf8_package;
using go.unicode;
using ꓸꓸꓸbyte = Span<byte>;
// </GlyphForwarderUsings>

[GoPackage("reflect")]
public static partial class reflect_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface packIfaceValueIntoEmptyIface_type {}
    [GoValueClone("inRegPtrs", "outRegPtrs")] internal partial struct abiDesc {}
    internal partial struct abiSeq {}
    internal partial struct abiStep {}
    internal partial struct abiStepKind {}
    internal partial struct bitVector {}
    internal partial struct cacheKey {}
    internal partial struct common {}
    internal partial struct dummyᴛ1 {}
    internal partial struct fieldScan {}
    internal partial struct flag {}
    internal partial struct funcLookupCacheᴛ1 {}
    internal partial struct interfaceType {}
    internal partial struct layoutKey {}
    [GoValueClone("abid")] internal partial struct layoutType {}
    [GoValueClone("regPtrs")] internal partial struct makeFuncCtxt {}
    [GoValueClone("makeFuncCtxt")] internal partial struct makeFuncImpl {}
    [GoValueClone("makeFuncCtxt")] internal partial struct methodValue {}
    internal partial struct nonEmptyInterface {}
    internal partial struct ptrType {}
    internal partial struct rtype {}
    internal partial struct runtimeSelect {}
    internal partial struct sliceType {}
    internal partial struct structType {}
    internal partial struct structTypeUncommon {}
    internal partial struct visibleFieldsWalker {}
    internal partial struct visit {}
    public partial interface Type {}
    public partial struct ChanDir {}
    public partial struct Kind {}
    public partial struct MapIter {}
    public partial struct Method {}
    public partial struct SelectCase {}
    public partial struct SelectDir {}
    public partial struct SliceHeader {}
    public partial struct StringHeader {}
    public partial struct StructField {}
    public partial struct StructTag {}
    public partial struct Value {}
    public partial struct ValueError {}
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
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸstrconv() => builtin.initPackage(typeof(@internal.strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸiter() => builtin.initPackage(typeof(iter_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(reflect_package.ChanDir)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class reflectᴛChanDir;
[GoMethodHost(typeof(reflect_package.Kind)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class reflectᴛKind;
[GoMethodHost(typeof(reflect_package.MapIter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class reflectᴛMapIter;
[GoMethodHost(typeof(reflect_package.Method)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class reflectᴛMethod;
[GoMethodHost(typeof(reflect_package.StructField)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class reflectᴛStructField;
[GoMethodHost(typeof(reflect_package.StructTag)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class reflectᴛStructTag;
[GoMethodHost(typeof(reflect_package.Value)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class reflectᴛValue;
[GoMethodHost(typeof(reflect_package.ValueError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class reflectᴛValueError;
[GoMethodHost(typeof(reflect_package.abiDesc)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class reflectᴛabiDesc;
[GoMethodHost(typeof(reflect_package.abiSeq)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class reflectᴛabiSeq;
[GoMethodHost(typeof(reflect_package.bitVector)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class reflectᴛbitVector;
[GoMethodHost(typeof(reflect_package.flag)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class reflectᴛflag;
[GoMethodHost(typeof(reflect_package.interfaceType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class reflectᴛinterfaceType;
[GoMethodHost(typeof(reflect_package.rtype)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class reflectᴛrtype;
[GoMethodHost(typeof(reflect_package.structType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class reflectᴛstructType;
[GoMethodHost(typeof(reflect_package.visibleFieldsWalker)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class reflectᴛvisibleFieldsWalker;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class reflectᴛabiSeq {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void dump(this ref abiSeq a) => reflect_package.dumpᴍ(ref a);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<abiStep> stepsForValue(this ref abiSeq a, nint i) => reflect_package.stepsForValueᴍ(ref a, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<abiStep> addArg(this ref abiSeq a, ж<abi.Type> Ꮡt) => reflect_package.addArgᴍ(ref a, Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<abiStep>, bool) addRcvr(this ref abiSeq a, ж<abi.Type> Ꮡrcvr) => reflect_package.addRcvrᴍ(ref a, Ꮡrcvr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool assignIntN(this ref abiSeq a, uintptr offset, uintptr size, nint n, uint8 ptrMap) => reflect_package.assignIntNᴍ(ref a, offset, size, n, ptrMap);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool assignFloatN(this ref abiSeq a, uintptr offset, uintptr size, nint n) => reflect_package.assignFloatNᴍ(ref a, offset, size, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void stackAssign(this ref abiSeq a, uintptr size, uintptr alignment) => reflect_package.stackAssignᴍ(ref a, size, alignment);
}
partial class reflectᴛabiDesc {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void dump(this ref abiDesc a) => reflect_package.dumpᴍ(ref a);
}
partial class reflectᴛValue {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static iter.Seq<Value> Seq(this Value v) => reflect_package.Seqᴍ(v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static iter.Seq2<Value, Value> Seq2(this Value v) => reflect_package.Seq2ᴍ(v);
}
partial class reflectᴛflag {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void panicNotMap(this flag f) => reflect_package.panicNotMapᴍ(f);
}
partial class reflectᴛrtype {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<abi.Type> common(this ж<rtype> Ꮡt) => reflect_package.commonᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<abi.UncommonType> uncommon(this ж<rtype> Ꮡt) => reflect_package.uncommonᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static abi.Name nameOff(this ж<rtype> Ꮡt, aNameOff off) => reflect_package.nameOffᴍ(Ꮡt, off);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<abi.Type> typeOff(this ж<rtype> Ꮡt, aTypeOff off) => reflect_package.typeOffᴍ(Ꮡt, off);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @unsafe.Pointer textOff(this ж<rtype> Ꮡt, aTextOff off) => reflect_package.textOffᴍ(Ꮡt, off);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr Size(this ref rtype t) => reflect_package.Sizeᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Bits(this ж<rtype> Ꮡt) => reflect_package.Bitsᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Align(this ref rtype t) => reflect_package.Alignᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint FieldAlign(this ref rtype t) => reflect_package.FieldAlignᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Kind Kind(this ref rtype t) => reflect_package.Kindᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<abi.Method> exportedMethods(this ж<rtype> Ꮡt) => reflect_package.exportedMethodsᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (StructField, bool) FieldByNameFunc(this ж<rtype> Ꮡt, Func<@string, bool> match) => reflect_package.FieldByNameFuncᴍ(Ꮡt, match);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool OverflowComplex(this ж<rtype> Ꮡt, complex128 x) => reflect_package.OverflowComplexᴍ(Ꮡt, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool OverflowFloat(this ж<rtype> Ꮡt, float64 x) => reflect_package.OverflowFloatᴍ(Ꮡt, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool OverflowInt(this ж<rtype> Ꮡt, int64 x) => reflect_package.OverflowIntᴍ(Ꮡt, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool OverflowUint(this ж<rtype> Ꮡt, uint64 x) => reflect_package.OverflowUintᴍ(Ꮡt, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool CanSeq(this ж<rtype> Ꮡt) => reflect_package.CanSeqᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool CanSeq2(this ж<rtype> Ꮡt) => reflect_package.CanSeq2ᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static iter.Seq<StructField> Fields(this ж<rtype> Ꮡt) => reflect_package.Fieldsᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static iter.Seq<Method> Methods(this ж<rtype> Ꮡt) => reflect_package.Methodsᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static iter.Seq<Type> Ins(this ж<rtype> Ꮡt) => reflect_package.Insᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static iter.Seq<Type> Outs(this ж<rtype> Ꮡt) => reflect_package.Outsᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<abi.Type> ptrTo(this ж<rtype> Ꮡt) => reflect_package.ptrToᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool AssignableTo(this ж<rtype> Ꮡt, Type u) => reflect_package.AssignableToᴍ(Ꮡt, u);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool ConvertibleTo(this ж<rtype> Ꮡt, Type u) => reflect_package.ConvertibleToᴍ(Ꮡt, u);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Comparable(this ref rtype t) => reflect_package.Comparableᴍ(ref t);
}
partial class reflectᴛinterfaceType {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static abi.Name nameOff(this ж<interfaceType> Ꮡt, aNameOff off) => reflect_package.nameOffᴍ(Ꮡt, off);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<abi.Type> typeOff(this ж<interfaceType> Ꮡt, aTypeOff off) => reflect_package.typeOffᴍ(Ꮡt, off);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<abi.Type> common(this ж<interfaceType> Ꮡt) => reflect_package.commonᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<abi.UncommonType> uncommon(this ж<interfaceType> Ꮡt) => reflect_package.uncommonᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Method /*m*/ Method(this ж<interfaceType> Ꮡt, nint i) => reflect_package.Methodᴍ(Ꮡt, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint NumMethod(this ref interfaceType t) => reflect_package.NumMethodᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (Method m, bool ok) MethodByName(this ж<interfaceType> Ꮡt, @string name) => reflect_package.MethodByNameᴍ(Ꮡt, name);
}
partial class reflectᴛMethod {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsExported(this Method m) => reflect_package.IsExportedᴍ(m);
}
partial class reflectᴛKind {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Kind k) => reflect_package.Stringᴍ(k);
}
partial class reflectᴛChanDir {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ChanDir d) => reflect_package.Stringᴍ(d);
}
partial class reflectᴛStructField {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsExported(this StructField f) => reflect_package.IsExportedᴍ(f);
}
partial class reflectᴛStructTag {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Get(this StructTag tag, @string key) => reflect_package.Getᴍ(tag, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (@string value, bool ok) Lookup(this StructTag tag, @string key) => reflect_package.Lookupᴍ(tag, key);
}
partial class reflectᴛstructType {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static StructField /*f*/ Field(this ref structType t, nint i) => reflect_package.Fieldᴍ(ref t, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static StructField /*f*/ FieldByIndex(this ж<structType> Ꮡt, slice<nint> index) => reflect_package.FieldByIndexᴍ(Ꮡt, index);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (StructField result, bool ok) FieldByNameFunc(this ж<structType> Ꮡt, Func<@string, bool> match) => reflect_package.FieldByNameFuncᴍ(Ꮡt, match);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (StructField f, bool present) FieldByName(this ж<structType> Ꮡt, @string name) => reflect_package.FieldByNameᴍ(Ꮡt, name);
}
partial class reflectᴛbitVector {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void append(this ref bitVector bv, uint8 bit) => reflect_package.appendᴍ(ref bv, bit);
}
partial class reflectᴛvisibleFieldsWalker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void walk(this ref visibleFieldsWalker w, Type t) => reflect_package.walkᴍ(ref w, t);
}
// </GlyphForwarders>
