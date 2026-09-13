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
using static go.main_package;

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
[assembly: GoTypeAlias("ArrayType", "ΔArrayType")]
[assembly: GoTypeAlias("ChanDir", "ΔChanDir")]
[assembly: GoTypeAlias("FuncType", "ΔFuncType")]
[assembly: GoTypeAlias("InterfaceType", "ΔInterfaceType")]
[assembly: GoTypeAlias("Kind", "ΔKind")]
[assembly: GoTypeAlias("MapType", "ΔMapType")]
[assembly: GoTypeAlias("Name", "ΔName")]
[assembly: GoTypeAlias("String", "const:ΔString")]
[assembly: GoTypeAlias("StructType", "ΔStructType")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("abi.go", "abi.cs", "ABFcgoKClIKCgpSCgoKUAAIYgoKUgoKUAAUQggACEoI=")]
[assembly: go.GoPositionMap("compiletype.go", "compiletype.cs", "AAgegKaAqICmgKaA")]
[assembly: go.GoPositionMap("escape.go", "escape.cs", "AAomgoLugoKU")]
[assembly: go.GoPositionMap("main.go", "main.cs", "7oKCgoSEgoI=")]
[assembly: go.GoPositionMap("switch.go", "switch.cs", "ABtAgoKYlKQ=")]
[assembly: go.GoPositionMap("type.go", "type.cs", "AEiIAoKClAAgRKKMqIKCgIKkpoCkgqiApoKogqaCABQuooKU1qKClAACFIIADiSigpSmggAUMqKCgpQAKgiigpSUpKysrKysrKwABBKilIKkgqSCpIKkgqSoooKUqKKClKiigpSoooKUqKKClKiApoCkgAAIEIKCgpSmooKClKiAAA4igqSCpIKkgqSCpqKClAANMoKmgqaCpoLWooKClIKU1KKCgpSCgpSmggANHIIADFKC2oKogqiCqILagoKCgoKCzIKClIKsgoKCgoKClOqCgpSCqIKClIKCpoKClIKUgoKChIKCgpSCgpSCloKCgoKCgoKW")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("main")]
[GoArchExclusive("amd64")]
[GoTestMatchingConsoleOutput]
public static partial class main_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("u")] internal partial struct Uncommon_u {}
    [GoLocalName("u")] internal partial struct Uncommon_uᴛ1 {}
    [GoLocalName("u")] internal partial struct Uncommon_uᴛ2 {}
    [GoLocalName("u")] internal partial struct Uncommon_uᴛ3 {}
    [GoLocalName("u")] internal partial struct Uncommon_uᴛ4 {}
    [GoLocalName("u")] internal partial struct Uncommon_uᴛ5 {}
    [GoLocalName("u")] internal partial struct Uncommon_uᴛ6 {}
    [GoLocalName("u")] internal partial struct Uncommon_uᴛ7 {}
    internal partial struct structTypeUncommon {}
    public partial struct ArchFamilyType {}
    public partial struct ChanType {}
    public partial struct EmptyInterface {}
    public partial struct FuncFlag {}
    public partial struct FuncID {}
    [GoValueClone("Fun")] public partial struct ITab {}
    public partial struct Imethod {}
    public partial struct IntArgRegBitmap {}
    [GoValueClone("Cases")] public partial struct InterfaceSwitch {}
    [GoValueClone("Entries")] public partial struct InterfaceSwitchCache {}
    public partial struct InterfaceSwitchCacheEntry {}
    public partial struct Method {}
    public partial struct NameOff {}
    public partial struct PtrType {}
    public partial struct RF_State {}
    [GoValueClone("Ints", "Floats", "Ptrs", "ReturnIsPtr")] public partial struct RegArgs {}
    public partial struct SliceType {}
    public partial struct StructField {}
    public partial struct TFlag {}
    public partial struct TextOff {}
    public partial struct Type {}
    public partial struct TypeAssert {}
    [GoValueClone("Entries")] public partial struct TypeAssertCache {}
    public partial struct TypeAssertCacheEntry {}
    public partial struct TypeOff {}
    public partial struct UncommonType {}
    public partial struct ΔArrayType {}
    public partial struct ΔChanDir {}
    public partial struct ΔFuncType {}
    public partial struct ΔInterfaceType {}
    public partial struct ΔKind {}
    public partial struct ΔMapType {}
    public partial struct ΔName {}
    public partial struct ΔStructType {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    // </ImportInitializers>
}
