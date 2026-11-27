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
using iter = go.iter_package;
// </ImportedTypeAliases>

using go;
using static go.reflect_package;
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
[assembly: GoPositionMap("reflect/abi.go", "abi.cs", "AFS4AYKClIKClIKCgqyygoKClJSu9IKCAA4ekriCpoKClAACEgAJBIKSgoIABxCClIKClAAElgEACQKClIKUgpSCgoKU3JQAAhLigpSClILclK7CgtwAIEiCgoKCgoKCgoKCgoKCgqaigoKClLgACBKmuJaSgoKCgpSmpoKCgpSCgoKCyriCuJbsgoKCgpSCgu6CrNKs0qzSlLS0zrKUtLQ=")]
[assembly: GoPositionMap("reflect/badlinkname.go", "badlinkname.cs", "AAw6ABICAAIU9qampqampqampqampqampqampqampqampqampqam")]
[assembly: GoPositionMap("reflect/float32reg_generic.go", "float32reg_generic.cs", "AAogwoKmgg==")]
[assembly: GoPositionMap("reflect/iter.go", "iter.cs", "AAoWtoKUgqaClIIABRrygoKGppSkpKSkpKSkpKSkpIKUgoKCgtqCgoLagoKC2oKCgoLagoKCygACENKCgoamlIKUgoKCgtqCgoLagoKC2oKCgoLK", "14-28:1;40-45:1;41-43:1.1;74-81:2;83-89:3;91-97:4;99-106:5;108-114:6;126-131:1;127-129:1.1;138-145:2;147-153:3;155-161:4;163-170:5")]
[assembly: GoPositionMap("reflect/makefunc.go", "makefunc.cs", "ABueAQAPJPKCuIKCppSmggANHJSmggACEAATLAALAu6Cgso=")]
[assembly: GoPositionMap("reflect/map.go", "map.cs", "ABO2AQAIEoKUgqYACRSUAAkUgqYAA1jSgoKmgqzSgoKmABv2A9I=", "", "", "48=TypeFor/1/1/3,ArrayOf/1/1/7")]
[assembly: GoPositionMap("reflect/type.go", "type.cs", "ANUCxgWCpoIADlCCpoKmgqaCpoKmggAXKoKClIKCgoKUpoKCpoIAFjSSqJKClKYAHUYACA4ACA4ACCAAEQiiqqKssqaipqKmoqaCAAQWlKKClIKClKaUlJSCgoKUAAqwAYIABCyCAAQWgqaCgoKUAAo+ooKUggAQdqKClKSUpqKClKSUpqKClIKClKaigpSCgpSmopSkpJSmooKUgoKUgoKUgqailKSklKaigpSCgpSCgpSCpqKClIKCgtyCgoKC3KKClIKCgtyigpSCgoIABTAAEgIACgaClKSklKjCgpSCgoKCgoKmgoKogJbSgpSCgoKCpgARKpIADCTSggACEgAJCJSCgpSCgu6CgpSClIKmgoKClJSClIKUgoKClKaosoKUgoKCgoKUgpS4goKCgpQACBKUAAIQ7OKCgoKCgpSUlAAIFgAOEoIAARAADA6UgoKC3IKCuJSCgpSCgpSCgsqUlJSCgoKCgtyClIKCgpSClIKClIKCgqaCpqr0goKCgoKUgriClKqiAAQUkgAGHgAIAAADEKKCgriCuIKCgoKUgqrCgpSC7pSUgqaCqLKClAAEGoKClIKmgoKUpoIABLgBAAgKAAIQAAkEgsqCpoK4prKCpoKmAAr6AQAJAgACJAARAoKUgriSgpSClAAHEIKCgpSmACO4AaKCgoKCgpSCpgAPFAAGwAGCqMKCgoKClIKClKaCgoKCgpSCgpSUgpSqwpSkpIKkgoKCprbKspTapIKkgoKCprbKspSkgqSCgoKmpLzCgoKCgoKCggASdJIAAhLigoKmgriokpSCgpSkpIKUpIKClKSCgoKmtqqigoKUAASwBYKssoKmpoKCuILKrNKUlIKCgoKmgpSCpgAG7AGCgpSCABJgAAwCgpSClIKCgrimAAgSgqaCgpSUxqbKggAIFJK4gqaC", "964-970:1;974-980:1;987-993:1;1000-1006:1;1378-1378:1;2871-2873:1", "", "1540=TypeOf/1/2/3,TypeOf/2/2/7,ArrayOf/1/1/7;1826=IsExported/1/1/1,newName/1/1/1,common/1/1/2;1918=align/1/1/6")]
[assembly: GoPositionMap("reflect/value.go", "value.cs", "AFGsAYKmgoKUrAAIDKyygpSClKiSAAUQooKUgqaCgoKClMi2AA00goKUABnUAYKmAAMagpSCpqSClIKUgoKUAAQm0gACENIAFUiUgqqCgoKClKaCpoKCgoKClIKUgqaClIKUgqaCgqaCgqaUgoKCgoKClJSCgoKmgoKUlraWkoKCuKaUgoLcgri0pIKkpKSUuKKCuIKCgqaCgpSmxoKCuJSUlJTYgpSCtAAIDIKmlIK4grimgqaCgoKCpri4goKCpoKUgoC4gsrIpoKSlJIACxiCgpSCtIK0grS0trgACi4AFwLclIKUpoKCgoKClIKCgsqCgpSUpqaCgoKUgrSCtIK0tOyCkpSmuIKCgriCgoKCloKWggAQIoKCgqbKgqamxoKC7tiClIK0AAoQ3MoAAhQACwKCgoKClIKClIKClIKClIKCgpSCgpSCgpSuwoKUgoKClLyiAAwsABUCggAJFIKSuIKm2LaCpKSkuIKCpoKClAAMGoCClIKClKamgoK2tqSkpraAgoKCprS0tLa2uJSCgoKUtoKkpKTKpoKmygAKFoKUgoKUysqCpsrosoKCgpQABBqCgpSkpIKUpAAEJJKUpAAJ3AGygpSCgoKCgpSmlK7CgpSCgoKCgpSmlKyygoKUrsKClKiSlKQABSIAA2KSlKQABTKSgpQADOQB1oKUAAZ00gAEpgGigpSUgoKUgoKmgoKUlIKUgoKClJSCAAAQlJQABHaiAAQagqSCpKS2pIKUpKrCpoKClAAEPtKClIKUAAIYAAkCgpSClIKClAAEGqKClKSUqqKClKSUpoKClKqigpSCgpSqooKUgoKUAAiSAdKCggAEPLKCggAY6gKygoIADpYCgoK4AAIQ0oKCrsKCggAEGJKmgoKUpoKCpoKCyoKUgoKUgqaCgpSCqJKUpAAN2AHigoKUooKCAAUiAAsCkoKCggAgQoKCAAcW8gAEIJKClLS0ggAXoAIALawC9gAMhgGygpQADogB8oKmuIKCpriUgoKUgpSUuAAEJqKCgriUgsaCgrauwoKUppSCgqaUpqaCgqamAAMYAAgCgpSCpoKmgsqUtKSkpKSkpLaClIKUlIKCpraCgoKmpKSq0pSUpKTIlKSkyJSkpMiUyIKUpNqClKTKgriCyILagriGpoKClKaqooKClLS0tKSqooKClLSkqJKCgoKqooKClLSkpoKCgoKmgoKCgqaCgoKCAAIS4qiSqJKokqiSqJKokriUqJKokoKClKiSgoKUqJKokqiSqJKokoKClIKokoKClIKCgoKClKiSgoKClIKCgpSokoKCgpSUqJKCgoKUrMamAAIUAAkGpIKCpoKWpqYAAhgACgKCgqikooKCqKampAABOAAbBJrKyszc3NaklqaqsoIACRrCgg==", "2649-2655:1;2670-2677:1", "", "120=typ/1/1/1,packEfaceData/1/1/2;978=add/1/2/1,add/2/2/2;2195=Kind/2/2/1,nameFor/2/2/1,elem/1/2/2,elem/2/2/2,haveIdenticalUnderlyingType/1/1/2")]
[assembly: GoPositionMap("reflect/visiblefields.go", "visiblefields.cs", "AAcgAAkCgpSClNy4goKCgpSmlJQACR7SgpSCgoKCgoKCyoKClKampoKClIKClIKmlA==", "", "", "24=NumField/1/1/3")]
// </GoSourcePositionMaps>

namespace go;

using static go.reflect_package;
using abi = go.@internal.abi_package;
using iter = go.iter_package;

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
[GoMethodHost(typeof(reflect_package.ChanDir)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class reflectᴛChanDir;
[GoMethodHost(typeof(reflect_package.Kind)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class reflectᴛKind;
[GoMethodHost(typeof(reflect_package.MapIter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class reflectᴛMapIter;
[GoMethodHost(typeof(reflect_package.Method)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class reflectᴛMethod;
[GoMethodHost(typeof(reflect_package.StructField)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class reflectᴛStructField;
[GoMethodHost(typeof(reflect_package.StructTag)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class reflectᴛStructTag;
[GoMethodHost(typeof(reflect_package.Value)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class reflectᴛValue;
[GoMethodHost(typeof(reflect_package.ValueError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class reflectᴛValueError;
[GoMethodHost(typeof(reflect_package.abiDesc)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class reflectᴛabiDesc;
[GoMethodHost(typeof(reflect_package.abiSeq)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class reflectᴛabiSeq;
[GoMethodHost(typeof(reflect_package.bitVector)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class reflectᴛbitVector;
[GoMethodHost(typeof(reflect_package.flag)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class reflectᴛflag;
[GoMethodHost(typeof(reflect_package.interfaceType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class reflectᴛinterfaceType;
[GoMethodHost(typeof(reflect_package.rtype)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class reflectᴛrtype;
[GoMethodHost(typeof(reflect_package.structType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class reflectᴛstructType;
[GoMethodHost(typeof(reflect_package.visibleFieldsWalker)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class reflectᴛvisibleFieldsWalker;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class reflectᴛChanDir {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ChanDir d) => reflect_package.String⁔(d);
}
partial class reflectᴛKind {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Kind k) => reflect_package.String⁔(k);
}
partial class reflectᴛMethod {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsExported(this Method m) => reflect_package.IsExported⁔(m);
}
partial class reflectᴛStructField {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsExported(this StructField f) => reflect_package.IsExported⁔(f);
}
partial class reflectᴛStructTag {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Get(this StructTag tag, @string key) => reflect_package.Get⁔(tag, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (@string value, bool ok) Lookup(this StructTag tag, @string key) => reflect_package.Lookup⁔(tag, key);
}
partial class reflectᴛValue {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static iter.Seq<Value> Seq(this Value v) => reflect_package.Seq⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static iter.Seq2<Value, Value> Seq2(this Value v) => reflect_package.Seq2⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<abi.Type> typ(this Value v) => reflect_package.typ⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static unsafe_package.Pointer pointer(this Value v) => reflect_package.pointer⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void panicNotBool(this Value v) => reflect_package.panicNotBool⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> bytesSlow(this Value v) => reflect_package.bytesSlow⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool CanAddr(this Value v) => reflect_package.CanAddr⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool CanSet(this Value v) => reflect_package.CanSet⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<Value> call(this Value v, @string op, slice<Value> @in) => reflect_package.call⁔(v, op, @in);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint capNonSlice(this Value v) => reflect_package.capNonSlice⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool CanComplex(this Value v) => reflect_package.CanComplex⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Value FieldByIndex(this Value v, slice<nint> index) => reflect_package.FieldByIndex⁔(v, index);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (Value, error) FieldByIndexErr(this Value v, slice<nint> index) => reflect_package.FieldByIndexErr⁔(v, index);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Value FieldByName(this Value v, @string name) => reflect_package.FieldByName⁔(v, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Value FieldByNameFunc(this Value v, Func<@string, bool> match) => reflect_package.FieldByNameFunc⁔(v, match);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool CanFloat(this Value v) => reflect_package.CanFloat⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool CanInt(this Value v) => reflect_package.CanInt⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool CanInterface(this Value v) => reflect_package.CanInterface⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsValid(this Value v) => reflect_package.IsValid⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Kind Kind(this Value v) => reflect_package.Kind⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint lenNonSlice(this Value v) => reflect_package.lenNonSlice⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint NumMethod(this Value v) => reflect_package.NumMethod⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Value MethodByName(this Value v, @string name) => reflect_package.MethodByName⁔(v, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool OverflowComplex(this Value v, complex128 x) => reflect_package.OverflowComplex⁔(v, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool OverflowFloat(this Value v, float64 x) => reflect_package.OverflowFloat⁔(v, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool OverflowInt(this Value v, int64 x) => reflect_package.OverflowInt⁔(v, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool OverflowUint(this Value v, uint64 x) => reflect_package.OverflowUint⁔(v, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (Value x, bool ok) Recv(this Value v) => reflect_package.Recv⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Send(this Value v, Value x) => reflect_package.Send⁔(v, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetPointer(this Value v, unsafe_package.Pointer x) => reflect_package.SetPointer⁔(v, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string stringNonString(this Value v) => reflect_package.stringNonString⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (Value x, bool ok) TryRecv(this Value v) => reflect_package.TryRecv⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool TrySend(this Value v, Value x) => reflect_package.TrySend⁔(v, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type typeSlow(this Value v) => reflect_package.typeSlow⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<abi.Type> abiType(this Value v) => reflect_package.abiType⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<abi.Type> abiTypeSlow(this Value v) => reflect_package.abiTypeSlow⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool CanUint(this Value v) => reflect_package.CanUint⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static iter.Seq2<StructField, Value> Fields(this Value v) => reflect_package.Fields⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static iter.Seq2<Method, Value> Methods(this Value v) => reflect_package.Methods⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void grow(this Value v, nint n) => reflect_package.grow⁔(v, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Value assignTo(this Value v, @string context, ж<abi.Type> Ꮡdst, unsafe_package.Pointer target) => reflect_package.assignTo⁔(v, context, Ꮡdst, target);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool CanConvert(this Value v, Type t) => reflect_package.CanConvert⁔(v, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Comparable(this Value v) => reflect_package.Comparable⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Equal(this Value v, Value u) => reflect_package.Equal⁔(v, u);
}
partial class reflectᴛValueError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref ValueError e) => reflect_package.Error⁔(ref e);
}
partial class reflectᴛabiDesc {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void dump(this ref abiDesc a) => reflect_package.dump⁔(ref a);
}
partial class reflectᴛabiSeq {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void dump(this ref abiSeq a) => reflect_package.dump⁔(ref a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<abiStep> stepsForValue(this ref abiSeq a, nint i) => reflect_package.stepsForValue⁔(ref a, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<abiStep> addArg(this ref abiSeq a, ж<abi.Type> Ꮡt) => reflect_package.addArg⁔(ref a, Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<abiStep>, bool) addRcvr(this ref abiSeq a, ж<abi.Type> Ꮡrcvr) => reflect_package.addRcvr⁔(ref a, Ꮡrcvr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool assignIntN(this ref abiSeq a, uintptr offset, uintptr size, nint n, uint8 ptrMap) => reflect_package.assignIntN⁔(ref a, offset, size, n, ptrMap);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool assignFloatN(this ref abiSeq a, uintptr offset, uintptr size, nint n) => reflect_package.assignFloatN⁔(ref a, offset, size, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stackAssign(this ref abiSeq a, uintptr size, uintptr alignment) => reflect_package.stackAssign⁔(ref a, size, alignment);
}
partial class reflectᴛbitVector {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void append(this ref bitVector bv, uint8 bit) => reflect_package.append⁔(ref bv, bit);
}
partial class reflectᴛflag {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void panicNotMap(this flag f) => reflect_package.panicNotMap⁔(f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Kind kind(this flag f) => reflect_package.kind⁔(f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static flag ro(this flag f) => reflect_package.ro⁔(f);
}
partial class reflectᴛinterfaceType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static abi.Name nameOff(this ж<interfaceType> Ꮡt, aNameOff off) => reflect_package.nameOff⁔(Ꮡt, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<abi.Type> typeOff(this ж<interfaceType> Ꮡt, aTypeOff off) => reflect_package.typeOff⁔(Ꮡt, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<abi.Type> common(this ж<interfaceType> Ꮡt) => reflect_package.common⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<abi.UncommonType> uncommon(this ж<interfaceType> Ꮡt) => reflect_package.uncommon⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Method /*m*/ Method(this ж<interfaceType> Ꮡt, nint i) => reflect_package.Method⁔(Ꮡt, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint NumMethod(this ref interfaceType t) => reflect_package.NumMethod⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (Method m, bool ok) MethodByName(this ж<interfaceType> Ꮡt, @string name) => reflect_package.MethodByName⁔(Ꮡt, name);
}
partial class reflectᴛrtype {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<abi.Type> common(this ж<rtype> Ꮡt) => reflect_package.common⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<abi.UncommonType> uncommon(this ж<rtype> Ꮡt) => reflect_package.uncommon⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static abi.Name nameOff(this ж<rtype> Ꮡt, aNameOff off) => reflect_package.nameOff⁔(Ꮡt, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<abi.Type> typeOff(this ж<rtype> Ꮡt, aTypeOff off) => reflect_package.typeOff⁔(Ꮡt, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static unsafe_package.Pointer textOff(this ж<rtype> Ꮡt, aTextOff off) => reflect_package.textOff⁔(Ꮡt, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr Size(this ref rtype t) => reflect_package.Size⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Bits(this ж<rtype> Ꮡt) => reflect_package.Bits⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Align(this ref rtype t) => reflect_package.Align⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint FieldAlign(this ref rtype t) => reflect_package.FieldAlign⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Kind Kind(this ref rtype t) => reflect_package.Kind⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<abi.Method> exportedMethods(this ж<rtype> Ꮡt) => reflect_package.exportedMethods⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (StructField, bool) FieldByNameFunc(this ж<rtype> Ꮡt, Func<@string, bool> match) => reflect_package.FieldByNameFunc⁔(Ꮡt, match);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool OverflowComplex(this ж<rtype> Ꮡt, complex128 x) => reflect_package.OverflowComplex⁔(Ꮡt, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool OverflowFloat(this ж<rtype> Ꮡt, float64 x) => reflect_package.OverflowFloat⁔(Ꮡt, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool OverflowInt(this ж<rtype> Ꮡt, int64 x) => reflect_package.OverflowInt⁔(Ꮡt, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool OverflowUint(this ж<rtype> Ꮡt, uint64 x) => reflect_package.OverflowUint⁔(Ꮡt, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool CanSeq(this ж<rtype> Ꮡt) => reflect_package.CanSeq⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool CanSeq2(this ж<rtype> Ꮡt) => reflect_package.CanSeq2⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static iter.Seq<StructField> Fields(this ж<rtype> Ꮡt) => reflect_package.Fields⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static iter.Seq<Method> Methods(this ж<rtype> Ꮡt) => reflect_package.Methods⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static iter.Seq<Type> Ins(this ж<rtype> Ꮡt) => reflect_package.Ins⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static iter.Seq<Type> Outs(this ж<rtype> Ꮡt) => reflect_package.Outs⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<abi.Type> ptrTo(this ж<rtype> Ꮡt) => reflect_package.ptrTo⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool AssignableTo(this ж<rtype> Ꮡt, Type u) => reflect_package.AssignableTo⁔(Ꮡt, u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool ConvertibleTo(this ж<rtype> Ꮡt, Type u) => reflect_package.ConvertibleTo⁔(Ꮡt, u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Comparable(this ref rtype t) => reflect_package.Comparable⁔(ref t);
}
partial class reflectᴛstructType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static StructField /*f*/ Field(this ref structType t, nint i) => reflect_package.Field⁔(ref t, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static StructField /*f*/ FieldByIndex(this ж<structType> Ꮡt, slice<nint> index) => reflect_package.FieldByIndex⁔(Ꮡt, index);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (StructField result, bool ok) FieldByNameFunc(this ж<structType> Ꮡt, Func<@string, bool> match) => reflect_package.FieldByNameFunc⁔(Ꮡt, match);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (StructField f, bool present) FieldByName(this ж<structType> Ꮡt, @string name) => reflect_package.FieldByName⁔(Ꮡt, name);
}
partial class reflectᴛvisibleFieldsWalker {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void walk(this ref visibleFieldsWalker w, Type t) => reflect_package.walk⁔(ref w, t);
}
// </MethodForwarders>
