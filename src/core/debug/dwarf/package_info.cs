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
using binary = go.encoding.binary_package;
// </ImportedTypeAliases>

using go;
using static go.debug.dwarf_package;
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
[assembly: GoDynamicTypeLift("696e746572666163657b42617369632829202a64656275672f64776172662e4261736963547970657d", "readType_type")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<AddrType, go.debug.dwarf_package.Type>(Pointer = true)]
[assembly: GoImplement<ArrayType, go.debug.dwarf_package.Type>(Pointer = true)]
[assembly: GoImplement<BoolType, go.debug.dwarf_package.Type>(Pointer = true)]
[assembly: GoImplement<CharType, go.debug.dwarf_package.Type>(Pointer = true)]
[assembly: GoImplement<ComplexType, go.debug.dwarf_package.Type>(Pointer = true)]
[assembly: GoImplement<DecodeError, error>]
[assembly: GoImplement<DotDotDotType, go.debug.dwarf_package.Type>(Pointer = true)]
[assembly: GoImplement<EnumType, go.debug.dwarf_package.Type>(Pointer = true)]
[assembly: GoImplement<FloatType, go.debug.dwarf_package.Type>(Pointer = true)]
[assembly: GoImplement<FuncType, go.debug.dwarf_package.Type>(Pointer = true)]
[assembly: GoImplement<IntType, go.debug.dwarf_package.Type>(Pointer = true)]
[assembly: GoImplement<PtrType, go.debug.dwarf_package.Type>(Pointer = true)]
[assembly: GoImplement<QualType, go.debug.dwarf_package.Type>(Pointer = true)]
[assembly: GoImplement<Reader, typeReader>(Pointer = true)]
[assembly: GoImplement<StructType, go.debug.dwarf_package.Type>(Pointer = true)]
[assembly: GoImplement<TypedefType, go.debug.dwarf_package.Type>(Pointer = true)]
[assembly: GoImplement<UcharType, go.debug.dwarf_package.Type>(Pointer = true)]
[assembly: GoImplement<UintType, go.debug.dwarf_package.Type>(Pointer = true)]
[assembly: GoImplement<UnspecifiedType, go.debug.dwarf_package.Type>(Pointer = true)]
[assembly: GoImplement<UnsupportedType, go.debug.dwarf_package.Type>(Pointer = true)]
[assembly: GoImplement<VoidType, go.debug.dwarf_package.Type>(Pointer = true)]
[assembly: GoImplement<typeUnit, dataFormat>(Pointer = true)]
[assembly: GoImplement<typeUnitReader, typeReader>(Pointer = true)]
[assembly: GoImplement<unit, dataFormat>(Pointer = true)]
[assembly: GoImplement<unknownFormat, dataFormat>]
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
[assembly: GoFieldAccess(typeof(ArrayType))]
[assembly: GoFieldAccess(typeof(BasicType))]
[assembly: GoFieldAccess(typeof(CommonType))]
[assembly: GoFieldAccess(typeof(Data))]
[assembly: GoFieldAccess(typeof(Entry))]
[assembly: GoFieldAccess(typeof(EnumType))]
[assembly: GoFieldAccess(typeof(EnumValue))]
[assembly: GoFieldAccess(typeof(Field))]
[assembly: GoFieldAccess(typeof(FuncType))]
[assembly: GoFieldAccess(typeof(LineReader))]
[assembly: GoFieldAccess(typeof(PtrType))]
[assembly: GoFieldAccess(typeof(QualType))]
[assembly: GoFieldAccess(typeof(Reader))]
[assembly: GoFieldAccess(typeof(StructField))]
[assembly: GoFieldAccess(typeof(StructType))]
[assembly: GoFieldAccess(typeof(TypedefType))]
[assembly: GoFieldAccess(typeof(UnspecifiedType))]
[assembly: GoFieldAccess(typeof(UnsupportedType))]
[assembly: GoFieldAccess(typeof(buf))]
[assembly: GoFieldAccess(typeof(typeUnit))]
[assembly: GoFieldAccess(typeof(typeUnitReader))]
[assembly: GoFieldAccess(typeof(unit))]
[assembly: GoFieldAccess(typeof(unit5))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("debug/dwarf/attr_string.go", "attr_string.cs", "/oaigoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKC2gB7+AGCgpQ=")]
[assembly: GoPositionMap("debug/dwarf/buf.go", "buf.cs", "ACdUgqaCpoKmouaCgoKUgoKCpoKCgpSCgoKmlIKCgoKmgoKCpoKCgpSmgoKClIKUuIKCgpSmgoKClKrSgoKCgoKCgqaokoKokoKCgpTokpSkpKSUguaygoKCgoKUpoKCggAJFII=")]
[assembly: GoPositionMap("debug/dwarf/class_string.go", "class_string.cs", "/oaigoKCgoKCgoKCgoKCgoKCgoKCgtqUgoKU")]
[assembly: GoPositionMap("debug/dwarf/const.go", "const.cs", "AIoBmgKCgpQAhgGWAoKClA==")]
[assembly: GoPositionMap("debug/dwarf/entry.go", "entry.cs", "ACdOwoKmgoKUlLiClIKCuIKCgoKCgoKClIKUlIKokoKCgoKCgoKCpoKUlIKUgtwAGDIAGDDSlIKmpgAIEoKUAAcQgIKkpqamptyAgqSmpqampgCOAaYCkgACFPKClKqigoKmACwS0qKCgoKUgoKClO6CgoKmgoKCgpSUgoKUgpSCgoKClKaCgoKUlIKCpoKCgpSUgoKUgpSmgoKCgoKCpIKU2LSClLS0tLSkgpSCpoKCgoKClNrExMTYtLS0tLS0tMjqyIKCgoKUgoKCgpTYtLS0tMi0goKCgoKUlIKUgpSCgpSCgpSUgoKCgsaClLS0tLSkgpSCgpSClIKUpoK2goKCgpT+goKSgpTs/LS0yMiUgraUgpQAEiqigoKqoqiSAAgKooKCgoKClIKCgoKCpoKCgpSClIKCgqiSgrqSgoKCgqaigpSCggADEOKClIKClIKCgoKUgoKCgpSCgqaUrNKC7oKCpoKCpoKCgpSCzqKqogACHgAOAoKCgpSCgoKCgoKCgoKUgpSCgpSigqaUrOKUlIKCgoKUtIKCgtqCpoKCppSCgpSUgoKUgoKUpoKClIKClKbKgoKCgpSmAAYQAAgCgoKUgoKUgoKCgriCgoKm5sKClIKCgpSCpoKUuLrSgpSCgqaCgoKClIKUpoKCgoLIgpSCgpSCgpS2goKCgpS2goK2toKCtoKCAAkMspSCpoKCgoKU", "429-457:1;459-486:2")]
[assembly: GoPositionMap("debug/dwarf/line.go", "line.cs", "AH6eAvKUuIKUlIK4ppKUAAcQgriUqsKmgoKCgpSC3JSCgpSClIKClJSCgpSCgpSUlIKCpoKUgpSCuIKCgsiClIKCuJSCgoKClIKUppTKgoKCgoK4goKCgoKClJSCgoKCooKUuJQAChiSgoKCgpSo0oKCgoKUtIKClJSClIKClJSCgoLogpTYtLS0tLS0tLS0tLS2lLSClLS0AAgMgqassoKClIKUgoKCgpSUgu6CgpSCpoKqooKUAAMU4oLKgoKUgoKUgu4ADybSlJSCgoKCpqaCgpSUgoK2lLS0tLTYgoKCgoKUyLaUgtq2tsaCtra2trbYtrbagraUgoKCgoKCqqKCggAOIJKuwoKCgoKqtIKmpqi2AAwaggACHgAMAsoAARwADgKAgqSUgoKUlILKgoKCgoKUlIKUuIKUAAMU4oKqooLKgpSmgoKmuIKCgpSUqqKCgqaUlIKUgoK4")]
[assembly: GoPositionMap("debug/dwarf/open.go", "open.cs", "ADNqAAEUAAgCABAigpSCgoKUpoKUpIK0grSmgoKUgq7CrsKClLS0tLY=")]
[assembly: GoPositionMap("debug/dwarf/tag_string.go", "tag_string.cs", "/oaigoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoIADxqCgoKWgpSCpKSCpKSCpIKk")]
[assembly: GoPositionMap("debug/dwarf/type.go", "type.cs", "AB46lAAOHpSCgpQAOHKAtAAJFIKmgoKUAAwQAAcQAF28AYKClKaCgpSmgoKClIKClIKCgpSSgoKCpoIAFSSCgoKUgoKClKSCAA0UgoKCgpSUgoKUAAcQAAcQlAAIEoKClAAPILIABxCigpSCgriCgpSCABsQAAkCgIKkgpKClIKCAAcQgoKClKzEppKC7oKCgoKUgpKUgoKClJSClIKU3JKCgpSCxoLolKYACRSCgoKClJaSppSClIKCgoKmtIK2lKaCggAPHoKCgoKUlIK2tLSCuKS06LS0tLSkgoaCgoKCgoKCggATJoKCgpS0tKSCgoKCgoKCgpSCgpS4goKClIKCgsa2goKCgoKCgoKUgpSmlIKClIKClAAIEIKCgoKUlLS0AA0YgoKCgoKCgoKCgoKCgoKUggAJEoKCgoKClAAMGIKCgoKUgoKClLSCxqQACBCCgoKC7IKCguyCgoKCtoKmgoKCgsq0tpSUiLLWgoKCpoKC", "428-430:1;442-476:2;480-497:3", "", "691=Basic/1/1/2", "", "485:60,486:17")]
[assembly: GoPositionMap("debug/dwarf/typeunit.go", "typeunit.cs", "AB00soKCgoKCgpSCgoKClIKClJSCgpSClIKClIKCgpSmggANGoKmqLKCgpSCpoKCgoKmggAKGJKCgoKClKiSqLKClIKUgoKClKiSAAcQkg==", "", "", "74=bytes/1/1/4;162=makeBuf/1/1/3")]
[assembly: GoPositionMap("debug/dwarf/unit.go", "unit.cs", "AB9IkqaCpoKmgoKUpoKClKaCgpSmgoKU5rSCgoKCgoKUgoKmgriCgoKCgoKClIKUgoKCgpSCgoKUgoKUlIKCgpSUgoKmlLSCgpTIgpSClKrUhoKUgoKUpqKClIKCuJSClIKUgpSClA==", "162-164:1")]
// </GoSourcePositionMaps>

namespace go.debug;

using static go.debug.dwarf_package;
using binary = go.encoding.binary_package;

[GoPackage("dwarf")]
public static partial class dwarf_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface dataFormat {}
    internal partial interface readType_type {}
    internal partial interface typeReader {}
    internal partial struct abbrev {}
    internal partial struct abbrevTable {}
    internal partial struct afield {}
    internal partial struct buf {}
    internal partial struct format {}
    internal partial struct lnctForm {}
    internal partial struct typeFixer {}
    internal partial struct typeUnit {}
    internal partial struct typeUnitReader {}
    internal partial struct unit {}
    internal partial struct unit5 {}
    internal partial struct unknownFormat {}
    public partial interface Type {}
    public partial struct AddrType {}
    public partial struct ArrayType {}
    public partial struct Attr {}
    public partial struct BasicType {}
    public partial struct BoolType {}
    public partial struct CharType {}
    public partial struct Class {}
    public partial struct CommonType {}
    public partial struct ComplexType {}
    public partial struct Data {}
    public partial struct DecodeError {}
    public partial struct DotDotDotType {}
    public partial struct Entry {}
    public partial struct EnumType {}
    public partial struct EnumValue {}
    public partial struct Field {}
    public partial struct FloatType {}
    public partial struct FuncType {}
    public partial struct IntType {}
    public partial struct LineEntry {}
    public partial struct LineFile {}
    public partial struct LineReader {}
    public partial struct LineReaderPos {}
    public partial struct Offset {}
    public partial struct PtrType {}
    public partial struct QualType {}
    public partial struct Reader {}
    public partial struct StructField {}
    public partial struct StructType {}
    public partial struct Tag {}
    public partial struct TypedefType {}
    public partial struct UcharType {}
    public partial struct UintType {}
    public partial struct UnspecifiedType {}
    public partial struct UnsupportedType {}
    public partial struct VoidType {}
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
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸpath() => builtin.initPackage(typeof(path_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(dwarf_package.ArrayType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dwarfᴛArrayType;
[GoMethodHost(typeof(dwarf_package.Attr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dwarfᴛAttr;
[GoMethodHost(typeof(dwarf_package.BasicType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dwarfᴛBasicType;
[GoMethodHost(typeof(dwarf_package.Class)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dwarfᴛClass;
[GoMethodHost(typeof(dwarf_package.CommonType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dwarfᴛCommonType;
[GoMethodHost(typeof(dwarf_package.Data)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dwarfᴛData;
[GoMethodHost(typeof(dwarf_package.DecodeError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dwarfᴛDecodeError;
[GoMethodHost(typeof(dwarf_package.DotDotDotType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dwarfᴛDotDotDotType;
[GoMethodHost(typeof(dwarf_package.Entry)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dwarfᴛEntry;
[GoMethodHost(typeof(dwarf_package.EnumType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dwarfᴛEnumType;
[GoMethodHost(typeof(dwarf_package.FuncType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dwarfᴛFuncType;
[GoMethodHost(typeof(dwarf_package.LineReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dwarfᴛLineReader;
[GoMethodHost(typeof(dwarf_package.PtrType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dwarfᴛPtrType;
[GoMethodHost(typeof(dwarf_package.QualType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dwarfᴛQualType;
[GoMethodHost(typeof(dwarf_package.Reader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dwarfᴛReader;
[GoMethodHost(typeof(dwarf_package.StructField)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dwarfᴛStructField;
[GoMethodHost(typeof(dwarf_package.StructType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dwarfᴛStructType;
[GoMethodHost(typeof(dwarf_package.Tag)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dwarfᴛTag;
[GoMethodHost(typeof(dwarf_package.TypedefType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dwarfᴛTypedefType;
[GoMethodHost(typeof(dwarf_package.UnsupportedType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dwarfᴛUnsupportedType;
[GoMethodHost(typeof(dwarf_package.VoidType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dwarfᴛVoidType;
[GoMethodHost(typeof(dwarf_package.buf)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dwarfᴛbuf;
[GoMethodHost(typeof(dwarf_package.typeFixer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dwarfᴛtypeFixer;
[GoMethodHost(typeof(dwarf_package.typeUnitReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dwarfᴛtypeUnitReader;
[GoMethodHost(typeof(dwarf_package.unit)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dwarfᴛunit;
[GoMethodHost(typeof(dwarf_package.unknownFormat)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dwarfᴛunknownFormat;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class dwarfᴛArrayType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref ArrayType t) => dwarf_package.String⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 Size(this ref ArrayType t) => dwarf_package.Size⁔(ref t);
}
partial class dwarfᴛAttr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Attr i) => dwarf_package.String⁔(i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this Attr a) => dwarf_package.GoString⁔(a);
}
partial class dwarfᴛBasicType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<BasicType> Basic(this ж<BasicType> Ꮡb) => dwarf_package.Basic⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref BasicType t) => dwarf_package.String⁔(ref t);
}
partial class dwarfᴛClass {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Class i) => dwarf_package.String⁔(i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this Class i) => dwarf_package.GoString⁔(i);
}
partial class dwarfᴛCommonType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<CommonType> Common(this ж<CommonType> Ꮡc) => dwarf_package.Common⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 Size(this ref CommonType c) => dwarf_package.Size⁔(ref c);
}
partial class dwarfᴛData {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (abbrevTable, error) parseAbbrev(this ж<Data> Ꮡd, uint64 off, nint vers) => dwarf_package.parseAbbrev⁔(Ꮡd, off, vers);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Reader> Reader(this ж<Data> Ꮡd) => dwarf_package.Reader⁔(Ꮡd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<array<uint64>>, error) Ranges(this ж<Data> Ꮡd, ж<Entry> Ꮡe) => dwarf_package.Ranges⁔(Ꮡd, Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Entry>, uint64, error) baseAddressForEntry(this ж<Data> Ꮡd, ж<Entry> Ꮡe) => dwarf_package.baseAddressForEntry⁔(Ꮡd, Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<array<uint64>>, error) dwarf2Ranges(this ж<Data> Ꮡd, ж<unit> Ꮡu, uint64 @base, int64 ranges, slice<array<uint64>> ret) => dwarf_package.dwarf2Ranges⁔(Ꮡd, Ꮡu, @base, ranges, ret);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<array<uint64>>, error) dwarf5Ranges(this ж<Data> Ꮡd, ж<unit> Ꮡu, ж<Entry> Ꮡcu, uint64 @base, int64 ranges, slice<array<uint64>> ret) => dwarf_package.dwarf5Ranges⁔(Ꮡd, Ꮡu, Ꮡcu, @base, ranges, ret);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uint64, error) debugAddr(this ж<Data> Ꮡd, dataFormat format, uint64 addrBase, uint64 idx) => dwarf_package.debugAddr⁔(Ꮡd, format, addrBase, idx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<LineReader>, error) LineReader(this ж<Data> Ꮡd, ж<Entry> Ꮡcu) => dwarf_package.LineReader⁔(Ꮡd, Ꮡcu);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error AddTypes(this ж<Data> Ꮡd, @string name, slice<byte> types) => dwarf_package.AddTypes⁔(Ꮡd, name, types);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error AddSection(this ref Data d, @string name, slice<byte> contents) => dwarf_package.AddSection⁔(ref d, name, contents);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (Type, error) Type(this ж<Data> Ꮡd, Offset off) => dwarf_package.Type⁔(Ꮡd, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (Type, error) readType(this ж<Data> Ꮡd, @string name, typeReader r, Offset off, map<Offset, Type> typeCache, ж<typeFixer> Ꮡfixups) => dwarf_package.readType⁔(Ꮡd, name, r, off, typeCache, Ꮡfixups);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error parseTypes(this ж<Data> Ꮡd, @string name, slice<byte> types) => dwarf_package.parseTypes⁔(Ꮡd, name, types);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (Type, error) sigToType(this ж<Data> Ꮡd, uint64 sig) => dwarf_package.sigToType⁔(Ꮡd, sig);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<unit>, error) parseUnits(this ж<Data> Ꮡd) => dwarf_package.parseUnits⁔(Ꮡd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint offsetToUnit(this ж<Data> Ꮡd, Offset off) => dwarf_package.offsetToUnit⁔(Ꮡd, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error collectDwarf5BaseOffsets(this ж<Data> Ꮡd, ж<unit> Ꮡu) => dwarf_package.collectDwarf5BaseOffsets⁔(Ꮡd, Ꮡu);
}
partial class dwarfᴛDecodeError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this DecodeError e) => dwarf_package.Error⁔(e);
}
partial class dwarfᴛDotDotDotType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref DotDotDotType t) => dwarf_package.String⁔(ref t);
}
partial class dwarfᴛEntry {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static any Val(this ref Entry e, Attr a) => dwarf_package.Val⁔(ref e, a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Field> AttrField(this ref Entry e, Attr a) => dwarf_package.AttrField⁔(ref e, a);
}
partial class dwarfᴛEnumType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref EnumType t) => dwarf_package.String⁔(ref t);
}
partial class dwarfᴛFuncType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref FuncType t) => dwarf_package.String⁔(ref t);
}
partial class dwarfᴛLineReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readHeader(this ж<LineReader> Ꮡr, @string compDir) => dwarf_package.readHeader⁔(Ꮡr, compDir);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<lnctForm> readLNCTFormat(this ref LineReader r) => dwarf_package.readLNCTFormat⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string path, uint64 mtime, uint64 size, error err) readLNCT(this ref LineReader r, slice<lnctForm> s, bool dwarf64) => dwarf_package.readLNCT⁔(ref r, s, dwarf64);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (bool, error) readFileEntry(this ref LineReader r) => dwarf_package.readFileEntry⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void updateFile(this ref LineReader r) => dwarf_package.updateFile⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Next(this ref LineReader r, ж<LineEntry> Ꮡentry) => dwarf_package.Next⁔(ref r, Ꮡentry);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool step(this ref LineReader r, ж<LineEntry> Ꮡentry) => dwarf_package.step⁔(ref r, Ꮡentry);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void advancePC(this ref LineReader r, nint opAdvance) => dwarf_package.advancePC⁔(ref r, opAdvance);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static LineReaderPos Tell(this ref LineReader r) => dwarf_package.Tell⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Seek(this ref LineReader r, LineReaderPos pos) => dwarf_package.Seek⁔(ref r, pos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Reset(this ref LineReader r) => dwarf_package.Reset⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void resetState(this ref LineReader r) => dwarf_package.resetState⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<ж<LineFile>> Files(this ref LineReader r) => dwarf_package.Files⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SeekPC(this ref LineReader r, uint64 pc, ж<LineEntry> Ꮡentry) => dwarf_package.SeekPC⁔(ref r, pc, Ꮡentry);
}
partial class dwarfᴛPtrType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref PtrType t) => dwarf_package.String⁔(ref t);
}
partial class dwarfᴛQualType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref QualType t) => dwarf_package.String⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 Size(this ref QualType t) => dwarf_package.Size⁔(ref t);
}
partial class dwarfᴛReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint AddressSize(this ref Reader r) => dwarf_package.AddressSize⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static binary.ByteOrder ByteOrder(this ref Reader r) => dwarf_package.ByteOrder⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Seek(this ref Reader r, Offset off) => dwarf_package.Seek⁔(ref r, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void maybeNextUnit(this ref Reader r) => dwarf_package.maybeNextUnit⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void nextUnit(this ref Reader r) => dwarf_package.nextUnit⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void collectDwarf5BaseOffsets(this ref Reader r, ж<unit> Ꮡu) => dwarf_package.collectDwarf5BaseOffsets⁔(ref r, Ꮡu);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Entry>, error) Next(this ж<Reader> Ꮡr) => dwarf_package.Next⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SkipChildren(this ж<Reader> Ꮡr) => dwarf_package.SkipChildren⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static typeReader clone(this ref Reader r) => dwarf_package.clone⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Offset offset(this ref Reader r) => dwarf_package.offset⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Entry>, error) SeekPC(this ж<Reader> Ꮡr, uint64 pc) => dwarf_package.SeekPC⁔(Ꮡr, pc);
}
partial class dwarfᴛStructField {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 bitOffset(this ref StructField f) => dwarf_package.bitOffset⁔(ref f);
}
partial class dwarfᴛStructType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref StructType t) => dwarf_package.String⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Defn(this ref StructType t) => dwarf_package.Defn⁔(ref t);
}
partial class dwarfᴛTag {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this Tag t) => dwarf_package.GoString⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Tag i) => dwarf_package.String⁔(i);
}
partial class dwarfᴛTypedefType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref TypedefType t) => dwarf_package.String⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 Size(this ref TypedefType t) => dwarf_package.Size⁔(ref t);
}
partial class dwarfᴛUnsupportedType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref UnsupportedType t) => dwarf_package.String⁔(ref t);
}
partial class dwarfᴛVoidType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref VoidType t) => dwarf_package.String⁔(ref t);
}
partial class dwarfᴛbuf {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint8 uint8(this ref buf b) => dwarf_package.uint8⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> bytes(this ref buf b, nint n) => dwarf_package.bytes⁔(ref b, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void skip(this ref buf b, nint n) => dwarf_package.skip⁔(ref b, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string @string(this ref buf b) => dwarf_package.string⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint16 uint16(this ref buf b) => dwarf_package.uint16⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 uint24(this ref buf b) => dwarf_package.uint24⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 uint32(this ref buf b) => dwarf_package.uint32⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 uint64(this ref buf b) => dwarf_package.uint64⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uint64 c, nuint bits) varint(this ref buf b) => dwarf_package.varint⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 @uint(this ref buf b) => dwarf_package.uint⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 @int(this ref buf b) => dwarf_package.int⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 addr(this ref buf b) => dwarf_package.addr⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (Offset length, bool dwarf64) unitLength(this ref buf b) => dwarf_package.unitLength⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void error(this ref buf b, @string s) => dwarf_package.error⁔(ref b, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Entry> entry(this ж<buf> Ꮡb, ж<Entry> Ꮡcu, ж<unit> Ꮡu) => dwarf_package.entry⁔(Ꮡb, Ꮡcu, Ꮡu);
}
partial class dwarfᴛtypeFixer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void recordArrayType(this ref typeFixer tf, ж<Type> Ꮡt) => dwarf_package.recordArrayType⁔(ref tf, Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void apply(this ref typeFixer tf) => dwarf_package.apply⁔(ref tf);
}
partial class dwarfᴛtypeUnitReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Seek(this ref typeUnitReader tur, Offset off) => dwarf_package.Seek⁔(ref tur, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint AddressSize(this ref typeUnitReader tur) => dwarf_package.AddressSize⁔(ref tur);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Entry>, error) Next(this ж<typeUnitReader> Ꮡtur) => dwarf_package.Next⁔(Ꮡtur);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static typeReader clone(this ref typeUnitReader tur) => dwarf_package.clone⁔(ref tur);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Offset offset(this ref typeUnitReader tur) => dwarf_package.offset⁔(ref tur);
}
partial class dwarfᴛunit {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint version(this ref unit u) => dwarf_package.version⁔(ref u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (bool, bool) dwarf64(this ref unit u) => dwarf_package.dwarf64⁔(ref u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint addrsize(this ref unit u) => dwarf_package.addrsize⁔(ref u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 addrBase(this ref unit u) => dwarf_package.addrBase⁔(ref u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 strOffsetsBase(this ref unit u) => dwarf_package.strOffsetsBase⁔(ref u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 rngListsBase(this ref unit u) => dwarf_package.rngListsBase⁔(ref u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 locListsBase(this ref unit u) => dwarf_package.locListsBase⁔(ref u);
}
partial class dwarfᴛunknownFormat {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint version(this unknownFormat u) => dwarf_package.version⁔(u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (bool, bool) dwarf64(this unknownFormat u) => dwarf_package.dwarf64⁔(u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint addrsize(this unknownFormat u) => dwarf_package.addrsize⁔(u);
}
// </MethodForwarders>
