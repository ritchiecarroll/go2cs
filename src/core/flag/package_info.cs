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
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using runtimeꓸError = go.runtime_package.ΔError;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static go.flag_package;

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
[assembly: GoTypeAlias("ErrorHandling", "ΔErrorHandling")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<boolFuncValue, Value>]
[assembly: GoImplement<boolValue, Value>(Pointer = true)]
[assembly: GoImplement<durationValue, Value>(Pointer = true)]
[assembly: GoImplement<float64Value, Value>(Pointer = true)]
[assembly: GoImplement<funcValue, Value>]
[assembly: GoImplement<int64Value, Value>(Pointer = true)]
[assembly: GoImplement<intValue, Value>(Pointer = true)]
[assembly: GoImplement<stringValue, Value>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<textValue, Getter>]
[assembly: GoImplement<uint64Value, Value>(Pointer = true)]
[assembly: GoImplement<uintValue, Value>(Pointer = true)]
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
[assembly: go.GoPositionMap("flag/flag.go", "flag.cs", "AGbKAbi4lIKCgpSClIKUzKKCpoKCgpSCpoCkgKSAAAwYooKmgoKClIKmgKSAyqKCpoKCgpSCpoCkgMqigqaCgoKUgqaApIDKooKmgoKClIKmgKSAyqKCpoKCpoCkgMqigqaCgoKUgqaApIDKooKmgoKClIKmgKSA+oKCgpSCgpSClIKmgqaCpoKAgoCCxsyApIDKgKSApIAAQ4gBkoKCgoKUhqqigpSokqiSqqKqooK8oqqigryiqJKqoqiSpIKCAAgSgoKClIKUhJSCgpSClIKokqoACwiSgoKUyoKAgoKUtgAOEAAKBIKCgoKCgoKmuIKUgsa01LTUpKzSgoKCgoKCgriSuJSogIKSgoCUlMbKgIKCggAFPgAaAqiygpSUABAggoKokKaQqrKClKyyqJCmkKaQppCooqqiqqKCgqqiqqKqoqqigoKqoqqiqqKqooKCqqKqoqqiqqKCgqqiqqKqoqqigoKqoqqiqqKqooKCqqKqoqqiqqKCgqqirLKssqyygoKssgACENIAAhDSrLKssqyyrLIAAhL0gpKCqIKCgoKClJSUgIKkgpQAAhLiqLKCgqrSgoKqwoKU6rKClIKClIKCgpKCpoKCqIKCgpKCgoKCqIKCkoKUloCSgoCCtoCC2pSClIKUgILGgpSCruKCgoKCgpSClJSkgpSktqiSqrSokgAHEJSClN6mgqyyuJKssoI=", "423-425:1;552-559:1;609-641:1")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("flag")]
public static partial class flag_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface boolFlag {}
    internal partial struct boolValue {}
    internal partial struct durationValue {}
    internal partial struct float64Value {}
    internal partial struct int64Value {}
    internal partial struct intValue {}
    internal partial struct stringValue {}
    internal partial struct textValue {}
    internal partial struct uint64Value {}
    internal partial struct uintValue {}
    public partial interface Getter {}
    public partial interface Value {}
    public partial struct Flag {}
    public partial struct FlagSet {}
    public partial struct ΔErrorHandling {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}
