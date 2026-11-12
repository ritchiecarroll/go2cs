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
using static go.debug.dwarf_package;

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
[assembly: GoPositionMap("debug/dwarf/attr_string.go", "attr_string.cs", "AAgOhqKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoLaAH34AYKClA==")]
[assembly: GoPositionMap("debug/dwarf/buf.go", "buf.cs", "ACpUgqaCpoLGogAIBoKCgpSCgoKmgoKClIKCgqaUgoKCgqaCgoKmgoKClKaCgoKUgpS4goKClKaCgoKUqtKCgoKCgoKCpqiSgqiSgoKClOiSlKSkpJSC5rKCgoKCgpSmgoKCAA0Ugg==")]
[assembly: GoPositionMap("debug/dwarf/class_string.go", "class_string.cs", "AAgOhqKCgoKCgoKCgoKCgoKCgoKCgoKC2rSCgpQ=")]
[assembly: GoPositionMap("debug/dwarf/const.go", "const.cs", "AI4BmgKCgpQAigGWAoKClA==")]
[assembly: GoPositionMap("debug/dwarf/entry.go", "entry.cs", "ACpOwoKmgoKUlLiClIKCuIKCgoKCgoKClIKUlIKokoKCgoKCgoKCpoKUlIKUgvwAGDIAGDDSlIKmpgAIEoKUAAcQgIKkpqamptyAgqSmpqampgCQAaYCkgAEFPKClKqigoKmADAS0qKCgoKUgoKClO6CgoKmgoKCgpSUgoKUgpSCgoKClKaCgoKUlIKCpoKCgpSUgoKUgpSmgoKCgoKClIKUyLSClLS0tLSkgpSCpoKCgoKClNq0tLTItLS0tLS0tMjqyIKCgoKUgoKCgpTYtLS0tMi0goKCgoKUlIKUgpSCgpSCgpSUgoKCgsaClLS0tLSkgpSCgpSClIKUpoK2goKCgpT+goKCgpTs7LS0yMiUgraUgpQAFiqigoLKoqiSAAgKooKCgoKClIKCgoKCpoKCgpSClIKCgqiSgrqSgoKCgqaigpSCggADEOKClIKClIKCgoKUgoKCgpSCgqaUrNKC7oKCpoKCpoKCgpSCzqKqogACHgAOAoKCgpSCgoKCgoKCgoKUgpSCgpSigqaUzOKUlIKCgoKUtIKCgtqCpoKCppSCgpSUgoKUgoKUpoKClIKClKbKgoKCgpSmAAgQAAgCgoKUgoKUgoKCgriCgoKm5sKClIKCgpSCpoKUuPrSgpSCgqaCgoKClIKUpoKCgoLIgpSCgpSCgpS2goKCgpS2goK2toKCtoKCAAsMspSCpoKCgoKU", "429-457:1;459-486:2")]
[assembly: GoPositionMap("debug/dwarf/line.go", "line.cs", "AIEBngLylLiClJSCuKaClAAHEIK4lMrCpoKCgoKUgtyUgoKUgpSCgpSUgoKUgoKUlJSCgqaClIKUgriCgoK4gpSCgriUgoKCgpSClKaUyoKCgoKCuIKCgoKCgpSUgoKCgqKClLiUAA4YkoKCgoKUAAgI0oKCgoKUtIKClJSClIKClJSCgoLYgpTYtLS0tLS0tLS0tLS2lLSClLS0AAgMgqassoKClIKUgoKCgpSUgu6CgpSCpoKqooKUAAMU4oLKgoKUgoKUggAIDgAVJtKUlIKCgoKmpoKClJSCgraUtLS0tNiCgoKCgpTItpSC2ra2toK2tra2tsi2ttqCtpSCgoKCgoKqooKCABIgkq7CgoKCgqq0gqamqLYADBqCAAIeAAwC6gADHAAOAoCCpJSCgpSUgsqCgoKCgpSUgpS4gpQABRTigqqigsqClKaCgqa4goKClJSqooKCppSUgpSCgrg=")]
[assembly: GoPositionMap("debug/dwarf/open.go", "open.cs", "ADRqAAEUAAgCABAigpSCgoKUpoKUpIK0grSmgoKUgs7CrsKCpLS0tLY=")]
[assembly: GoPositionMap("debug/dwarf/tag_string.go", "tag_string.cs", "AAgOhqKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCggAPGoKCgraClIKkpIKkpIKkgqQ=")]
[assembly: GoPositionMap("debug/dwarf/type.go", "type.cs", "ACE6lAASHpSCgpQAPHKUAA0UgqaCgpQADxAACxAAYbwBgoKUxoKClMaCgoKUgoKUgoKClIKCgoKmggAZJIKCgpSCgoKUlIIAERSCgoKClJSCgpQACxAACxCUAAwSgoKUABIgsgALEKKClIKCuIKClIIAHxAACQKAgqSCkoKUgoIABxCCgoKUrMSmkoLugoKCgpSCgpSCgoKUlIKUgpTckoKClILGguiUpgAJFIKCgoKUlpKmlIKUgoKCgqa0graUpoKCAA8egoKCgpSUgra0tIK4pLTotLS0tKSChoKCgoKCgoKCABMmgoKClLS0pIKCgoKCgoKClIKClLiCgoKUgoKCxraCgoKCgoKCgpSClKaUgoKUgoKUAAgQgoKCgpSUtLQADRiCgoKCgoKCgoKCgoKCgpSCAAkSgoKCgoKUAAwYgoKCgpSCgoKUtILGpAAIEIKCgoLsgoKC7IKCgoK2gqaCgoKCyrS2lJSIsvaCgoKmgoI=", "428-430:1;442-476:2;480-497:3", "", "746=Basic/1/1/2", "", "541:25,542:17")]
[assembly: GoPositionMap("debug/dwarf/typeunit.go", "typeunit.cs", "ACA0soKCgoKCgpSCgoKClIKClJSCgpSClIKClIKCgpSmggAMGoKmqLKCgpSCpoKCgoKmggAOGJKCgoKClKiSqLKClIKUgoKClKiSAAYQkg==", "", "", "77=bytes/1/1/4;168=makeBuf/1/1/3")]
[assembly: GoPositionMap("debug/dwarf/unit.go", "unit.cs", "ACJIkqaCpoKmgoKUpoKClKaCgpSmgoKUAAgGtIKCgoKCgpSCgqaCuIKCgoKCgoKUgpSCgoKClIKCgpSCgpSUgoKClJSCgqaUtIKClMiClIKUqtSGgpSCgpSmooKUgoK4lIKUgpSClIKU", "162-164:1")]
// </GoSourcePositionMaps>

namespace go.debug;

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
[GoMethodHost(typeof(dwarf_package.ArrayType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dwarfᴛArrayType;
[GoMethodHost(typeof(dwarf_package.Attr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dwarfᴛAttr;
[GoMethodHost(typeof(dwarf_package.BasicType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dwarfᴛBasicType;
[GoMethodHost(typeof(dwarf_package.Class)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dwarfᴛClass;
[GoMethodHost(typeof(dwarf_package.CommonType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dwarfᴛCommonType;
[GoMethodHost(typeof(dwarf_package.Data)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dwarfᴛData;
[GoMethodHost(typeof(dwarf_package.DecodeError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dwarfᴛDecodeError;
[GoMethodHost(typeof(dwarf_package.DotDotDotType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dwarfᴛDotDotDotType;
[GoMethodHost(typeof(dwarf_package.Entry)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dwarfᴛEntry;
[GoMethodHost(typeof(dwarf_package.EnumType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dwarfᴛEnumType;
[GoMethodHost(typeof(dwarf_package.FuncType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dwarfᴛFuncType;
[GoMethodHost(typeof(dwarf_package.LineReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dwarfᴛLineReader;
[GoMethodHost(typeof(dwarf_package.PtrType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dwarfᴛPtrType;
[GoMethodHost(typeof(dwarf_package.QualType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dwarfᴛQualType;
[GoMethodHost(typeof(dwarf_package.Reader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dwarfᴛReader;
[GoMethodHost(typeof(dwarf_package.StructField)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dwarfᴛStructField;
[GoMethodHost(typeof(dwarf_package.StructType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dwarfᴛStructType;
[GoMethodHost(typeof(dwarf_package.Tag)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dwarfᴛTag;
[GoMethodHost(typeof(dwarf_package.TypedefType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dwarfᴛTypedefType;
[GoMethodHost(typeof(dwarf_package.UnsupportedType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dwarfᴛUnsupportedType;
[GoMethodHost(typeof(dwarf_package.VoidType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dwarfᴛVoidType;
[GoMethodHost(typeof(dwarf_package.buf)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dwarfᴛbuf;
[GoMethodHost(typeof(dwarf_package.typeFixer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dwarfᴛtypeFixer;
[GoMethodHost(typeof(dwarf_package.typeUnitReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dwarfᴛtypeUnitReader;
[GoMethodHost(typeof(dwarf_package.unit)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dwarfᴛunit;
[GoMethodHost(typeof(dwarf_package.unknownFormat)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dwarfᴛunknownFormat;
// </MethodHolders>
