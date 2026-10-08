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
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using runtimeꓸError = go.runtime_package.ΔError;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static go.@internal.coverage.cfile_package;

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
[assembly: GoImplement<emitState, go.@internal.coverage.encodecounter_package.CounterVisitor>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.ReadSeeker>(Pointer = true)]
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
[assembly: go.GoPositionMap("internal/coverage/cfile/apis.go", "apis.cs", "ABEikoKUqJKClIKUgqiSgpSokoKUgqaCgpSCloK4qJKCgpSCADl4goKUgoKmgoKmpg==")]
[assembly: go.GoPositionMap("internal/coverage/cfile/emit.go", "emit.cs", "ACNKAESgAbKClIKCgoKmgoKWgoKCloCCgoLagoKUgoKUpoKCgpSsxN6Clt6EgoKCooKClJSCgoK6goKigIKkgoKClIKCuoKWgoKChKqigoKUgpaE3oCCuIKAgrassoKUgIKCgtykgpSWgqiCAAYQgIKkgqiAgqSAgsqAgqaokoCCpAACEAAIBoKCgpSCgoKCpqzSgoKCgoKCgpQAAhwADQKCgpSCloKAgraCgIK2rNKAgqSAgsqAgqasstaihIKigpSmgoSCgoKUloKCgpSCgqiCgoKCuoKCgoKmlIKWgoKCpgAHEoKCgpKCgIKUggAIEJaCgIK4lIKmAAcQ0oKCgpSCgqrCgoCCpAACENKmgoKEgpSCgoKCooKClJSCgg==", "455-461:1", "", "171=Getenv/1/1/2;239=Getenv/1/1/2;291=Getenv/1/1/4")]
[assembly: go.GoPositionMap("internal/coverage/cfile/hooks.go", "hooks.cs", "AAo4ABYIgoKU")]
[assembly: go.GoPositionMap("internal/coverage/cfile/testsupport.go", "testsupport.cs", "AB484pKCqILKgIKkgIIABxCCgpiSgoKCgoKUkoKCzAAIEoKCgoKUgIK4goCCgILagIK4goCCpIKAgrgADBbkgoKUkpSCgoKUgoKUgoCCuJaygoKUkoKCgpSCgoKClIKogoCUgILGgoKU2IKAgsqCgoKCgoKUgoKCgoKAgraCgoKmgpSCgpS4AAkOhqKCgpSAggAHEIKCgIKkgoKAgrYAAhLigpSWgoKCgoKUgqaChIKUgoKCpqaClA==", "69-74:1;138-140:1;159-193:2", "", "87=NewFormatter/1/1/2")]
// </GoSourcePositionMaps>

namespace go.@internal.coverage;

[GoPackage("cfile")]
public static partial class cfile_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct emitState {}
    internal partial struct fileType {}
    internal partial struct pkfunc {}
    internal partial struct tstate {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸhashꓸfnv() => builtin.initPackage(typeof(hash.fnv_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸcoverage() => builtin.initPackage(typeof(go.@internal.coverage_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸcoverageꓸcformat() => builtin.initPackage(typeof(go.@internal.coverage.cformat_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸcoverageꓸcmerge() => builtin.initPackage(typeof(go.@internal.coverage.cmerge_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸcoverageꓸdecodecounter() => builtin.initPackage(typeof(go.@internal.coverage.decodecounter_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸcoverageꓸdecodemeta() => builtin.initPackage(typeof(go.@internal.coverage.decodemeta_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸcoverageꓸencodecounter() => builtin.initPackage(typeof(go.@internal.coverage.encodecounter_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸcoverageꓸencodemeta() => builtin.initPackage(typeof(go.@internal.coverage.encodemeta_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸcoverageꓸpods() => builtin.initPackage(typeof(go.@internal.coverage.pods_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}
