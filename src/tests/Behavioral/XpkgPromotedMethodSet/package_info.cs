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
global using jsonꓸToken = object;
global using jsonꓸΔToken = object;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
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
[assembly: GoDynamicTypeLift("696e746572666163657b4572726f72282e2e2e616e79297d", "main_typeᴛ1")]
[assembly: GoDynamicTypeLift("696e746572666163657b50696e67282920696e747d", "main_typeᴛ4")]
[assembly: GoDynamicTypeLift("696e746572666163657b53657428696e74297d", "main_typeᴛ3")]
[assembly: GoDynamicTypeLift("696e746572666163657b546f6b28292058706b6750726f6d6f746564496e6e65724c69622e546f6b656e7d", "main_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b5768657265282920626f6f6c7d", "main_typeᴛ2")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<Broken, io_package.Reader>(Promoted = true)]
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
[assembly: go.GoPositionMap("main.go", "main.cs", "ACRUgABEaoAAHC6ChIKWhKaCgoKoooKAgriiAEIEgoKCgoKCgoKCgoKCgoKCgoKChIKAgJSEhICCpoSAgqaCgoKCgoKCgoKEgoKCiIKChoKChIKChoKCgoKGgoSClpSChJSChIKW", "138-142:1;167-167:1;168-168:2;202-202:3;203-203:4;204-204:5;216-216:6;217-217:7;218-218:8;219-219:9;220-222:10;223-231:11;232-236:12;237-245:13")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("main")]
[GoTestMatchingConsoleOutput]
public static partial class main_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface main_type {}
    internal partial interface main_typeᴛ1 {}
    internal partial interface main_typeᴛ2 {}
    internal partial interface main_typeᴛ3 {}
    internal partial interface main_typeᴛ4 {}
    internal partial struct brokenState {}
    internal partial struct deepTok {}
    internal partial struct loc {}
    internal partial struct local {}
    internal partial struct local2 {}
    internal partial struct localMid {}
    internal partial struct localTok {}
    public partial struct Broken {}
    public partial struct BufP {}
    public partial struct Direct {}
    public partial struct Five {}
    public partial struct GCounter {}
    public partial struct HideC {}
    public partial struct Mixed {}
    public partial struct Outer {}
    public partial struct OuterFD {}
    public partial struct OuterFS {}
    public partial struct OuterS5 {}
    public partial struct PtrDirect {}
    public partial struct PtrStamp {}
    public partial struct S6 {}
    public partial struct Shadow {}
    public partial struct Stamp {}
    public partial struct TT {}
    public partial struct Within {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸXpkgPromotedInnerLib() => builtin.initPackage(typeof(XpkgPromotedInnerLib_package));
    [GoInit] internal static void initᴛᴛimportꓸXpkgPromotedMidLib() => builtin.initPackage(typeof(XpkgPromotedMidLib_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}
