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
[assembly: GoTypeAlias("LineReader", "ΔLineReader")]
[assembly: GoTypeAlias("Reader", "ΔReader")]
[assembly: GoTypeAlias("Type", "ΔType")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<AddrType, ΔType>(Pointer = true)]
[assembly: GoImplement<ArrayType, ΔType>(Pointer = true)]
[assembly: GoImplement<BoolType, ΔType>(Pointer = true)]
[assembly: GoImplement<CharType, ΔType>(Pointer = true)]
[assembly: GoImplement<ComplexType, ΔType>(Pointer = true)]
[assembly: GoImplement<DecodeError, error>]
[assembly: GoImplement<DotDotDotType, ΔType>(Pointer = true)]
[assembly: GoImplement<EnumType, ΔType>(Pointer = true)]
[assembly: GoImplement<FloatType, ΔType>(Pointer = true)]
[assembly: GoImplement<FuncType, ΔType>(Pointer = true)]
[assembly: GoImplement<IntType, ΔType>(Pointer = true)]
[assembly: GoImplement<PtrType, ΔType>(Pointer = true)]
[assembly: GoImplement<QualType, ΔType>(Pointer = true)]
[assembly: GoImplement<StructType, ΔType>(Pointer = true)]
[assembly: GoImplement<TypedefType, ΔType>(Pointer = true)]
[assembly: GoImplement<UcharType, ΔType>(Pointer = true)]
[assembly: GoImplement<UintType, ΔType>(Pointer = true)]
[assembly: GoImplement<UnspecifiedType, ΔType>(Pointer = true)]
[assembly: GoImplement<UnsupportedType, ΔType>(Pointer = true)]
[assembly: GoImplement<VoidType, ΔType>(Pointer = true)]
[assembly: GoImplement<typeUnit, dataFormat>(Pointer = true)]
[assembly: GoImplement<typeUnitReader, typeReader>(Pointer = true)]
[assembly: GoImplement<unit, dataFormat>(Pointer = true)]
[assembly: GoImplement<unknownFormat, dataFormat>]
[assembly: GoImplement<ΔReader, typeReader>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<Entry, ж<Entry>>(Indirect = true)]
[assembly: GoImplicitConv<typeUnit, ж<typeUnit>>(Indirect = true)]
[assembly: GoImplicitConv<unit, ж<unit>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("debug/dwarf/attr_string.go", "attr_string.cs", "/oaigoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCygB7+AGCgIKk")]
[assembly: go.GoPositionMap("debug/dwarf/buf.go", "buf.cs", "ACdUgqaCpoKmotaCgoKUgoKCpoKCgpSCgoKmgKSCgoKCloKCgqaCgoKUpoKCgpSClLiCgoKUpoKCgpSq0oKCgoKCgoKmqJKCqJKCgoKU2JKUpKSkpILWsoKCgpKClKaCgoIACRSC")]
[assembly: go.GoPositionMap("debug/dwarf/class_string.go", "class_string.cs", "/oaigoKCgoKCgoKCgoKCgoKCgoKCgsqUgoKU")]
[assembly: go.GoPositionMap("debug/dwarf/const.go", "const.cs", "AIUBmgKCgIKkAHuWAoKClA==")]
[assembly: go.GoPositionMap("debug/dwarf/entry.go", "entry.cs", "ACVOwoCCpoKClJS4gpSCgqiCgoKCgoKCgpSClJSCmJKCgoKCgoKCgqaChJSClILcABkyABYw0pymAAIS4pQAAhCA0qSmpqamrICypKampqamAAKDAYIAQKgDkgACFPKAgqSqooKCpgAhEtKSgoKUgoKClAAFFI6CgoKWgoKCgpSUgoKUgpSCgoKClJaCgoKUlIKCloKCgpSUgoKUgpSWgoKCgoKClIKctJKUpKSkpKSClIKewoKSgoKWgoKCgpTKpKSkqKSkpKSkpKSoqqiC8pKClIKCkoKUyKSkpKSopJKCgpKClJSClIKUgoKUgoKUlIKCgoLGgpSkpKSkpIKUgoKUgpSClJ7CgpKCgpa2goKSgpS+grKSgpS8rKSkqLiMAAwCgpKCgpYAA/EDAAL4A5SCloKUgoKCgpSkgoKCtoKCgsoAEiqigoKqoqiS6qKCgoKCgpSCgoKCloKCgpSClIKCqJKCupKCgoKu4oKUgoKUgoKCgpSCgoKClIKCppSs0oLegoKWgoKWgoKClILOoqqiAAIeAA4CgoKClIKCgoKCgoKClIKUgoKUooKmlKzihISCgoKClKSCgoLKgpaCgIKmlIKClJSCgpSCgpSmgoKUgoKUpsqCgoKClJYABRAACAKCgpSCgpSCkoKCqICCkoCCttaygpSCgoKEgpaClKiq0oKUgoKWgoKCgpSClKaCgoKCuIKEgoKUgoKUpoKCgoKUpoKCpqaCgqaCggAIDLKEgpaCgoKClA==", "438-466:1;468-495:2")]
[assembly: go.GoPositionMap("debug/dwarf/line.go", "line.cs", "AHyeAvKUqIKUlIK4loKUAAcQgIK4hKrCloKCgoKUgtyUgoKUgpSCgpSUgoKUgoKUlJSCgpaClIKUgqiCgoKogpSCgIK4lIKCgoKUgpSmlLqCgoCCkoLIgoKCgoKClJSCgoKCooKUqIQAChiSgoKCgpSo0oKCgoKUpIKClJSClIKClJSCgoK4kpS4pKSkpKSkpKSkpKS2lKSClKSk7IKWrLKSgpSClIKCgoKUlJLugoKUgqaCqqKClAADFAAIAoK6goKUgoKUggAJDgAMJtKElIKCgoKWloKChJSCgqaUtLS0tNiAkoKSgoKkqKaEkrqmpqaCpqampqaopqaqwraEgoKCgoKCqqKCggAOIJKuwoKCgoKqtIKWlqi2AAwaggACHgAMAsoAARwADgKAgqSUgoCCpJSCuoKCgoCCgpSkgpS4gpQAAxTigqqigsqClKaCgqa4goKClJSqooKAgraUlIKUgoK4")]
[assembly: go.GoPositionMap("debug/dwarf/open.go", "open.cs", "ADNqAAEUAAgCAA8igpSCgoKUpoKUpIK0grSmgoKUgq7CrsKClKSkpLY=")]
[assembly: go.GoPositionMap("debug/dwarf/tag_string.go", "tag_string.cs", "/oaigoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoIACRqCgoKWgpSCpKSCpKSCpIKk")]
[assembly: go.GoPositionMap("debug/dwarf/type.go", "type.cs", "AB46gKSAAA8egKSCgpQAOHKApIAAChSCpoKClAAKEIAACBCAAF68AYKClKaCgpSmgoKClIKClIKCgpSCgoKCpoIAFCSCgoKUgoKClJSCAAwUgoKCgpSUgoKUAAcQgAAIEICkgAAJEoKClAAOILIABxCigpSCgriCgpSCABIQAAkCgIKkgpKClIKCAAYQgoKClJy0lpKC7oKCgoKUgoKUgoKClJSClIKUzJKCgpSAgtaAguiklgABFIKCgoAACAKkhpKmlIKUgoKSgqakgraUloKCAAMegoIACwKClJykpIK4gKS0+KSkpKQAAjmCAAI8goaCgoKCgoKCggADJoKCgpSkpKSCgoKCgoKCgpSCgIKkmKKCgpSCgoLGpoKCgoKCgoKClIKEppSCgpSCgpQABBCCgoKAsqSUpKQABBiCgoKCgoKCgoKCgoKCgpSCAAUSgoKCwoKUAAMYgoKCgAAJAqSCgoKYgILGqwACEAADEIKCgoKsgoKCrIKCgoIADAaCloKCgoKa5LaUhIiy1oKCgqaSgg==", "428-430:1;442-476:2;480-497:3")]
[assembly: go.GoPositionMap("debug/dwarf/typeunit.go", "typeunit.cs", "ABs0soKCgoKCgpSCgoKClIKClJSCgpSChIKClIKCgpSWggAMGoKmqLKCgpSClpKCgoKWggAKGJKCgoKClKiSqLKClIKUgoKClKiSAAYQkg==")]
[assembly: go.GoPositionMap("debug/dwarf/unit.go", "unit.cs", "ABc4kqaCpoLWtIKCgoKCgpSCgqaCqIKCgoKCgoKUgpSCgoKClIKCgpSCgpSUgoKClJSCgpaUpIKSlLiClIKUqtSGgpSCgpQ=", "126-128:1")]
// </GoSourcePositionMaps>

namespace go.debug;

[GoPackage("dwarf")]
public static partial class dwarf_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
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
    [GoLocalName("delayed")] internal partial struct entry_delayed {}
    internal partial struct format {}
    internal partial struct lnctForm {}
    internal partial struct typeFixer {}
    internal partial struct typeUnit {}
    internal partial struct typeUnitReader {}
    internal partial struct unit {}
    internal partial struct unknownFormat {}
    public partial interface ΔType {}
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
    public partial struct LineReaderPos {}
    public partial struct Offset {}
    public partial struct PtrType {}
    public partial struct QualType {}
    public partial struct StructField {}
    public partial struct StructType {}
    public partial struct Tag {}
    public partial struct TypedefType {}
    public partial struct UcharType {}
    public partial struct UintType {}
    public partial struct UnspecifiedType {}
    public partial struct UnsupportedType {}
    public partial struct VoidType {}
    public partial struct ΔLineReader {}
    public partial struct ΔReader {}
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
