// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.

// <ImportedTypeAliases>
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
// </ImportedTypeAliases>

using go;
using static global::go.@internal.coverage.test_internal_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<ctrVis, go.@internal.coverage.encodecounter_package.CounterVisitor>(Pointer = true)]
[assembly: GoImplement<go.@internal.coverage.slicewriter_package.WriteSeeker, io_package.WriteSeeker>(Pointer = true)]
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
[assembly: go.GoPositionMap("internal/coverage/test/counter_test.go", "counter_test.cs", "ABgsgoKAgramggAJDqK6goKCppbKhISWgoKCgqiCgpSCgoCCpICCpIaSgpKAgriClICCpIKCgoKUgoKUgoCCkoKkgoKCpoKAgpKC1tSigoKCgpaWgoKCgoKCgoKCgoKCgpSEhJSCgpSCgIK2gILIgIKokoKSgIK4gpSAgqSCgpSCloKAgpSmgoCCkoLIgoKCgIKSgqSCgoLY", "45-52:1;91-95:2;187-191:1")]
[assembly: go.GoPositionMap("internal/coverage/test/roundtrip_test.go", "roundtrip_test.cs", "ABYmgoKCgpTmxoKCgoKClIKCgoKClIKClIKClIKClIKCAAgIlIKCgoKClAAHEIKClgAIEoKCqIKWgoKClIKCloKClIKCloKCgoCCpIKCyoKCgoKCgriUypTWgoKCgpaCgpSCgqaCgoKU1qKWgoKCpoKCgoKCgpSAgsqChIKClpSCgpSCgIKkgIK4goKUgoKUgoKClIKCgpaCgoKCgoKUgoKCpoKCgIK2goK4uKiCgoKCgpSCgoKCgoIABxCCqIKWgoKClIKClIKCgIKkgoI=")]
// </GoSourcePositionMaps>

namespace go.@internal.coverage;

[GoPackage("test")]
public static partial class test_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸcoverage() => builtin.initPackage(typeof(go.@internal.coverage_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸcoverageꓸdecodecounter() => builtin.initPackage(typeof(go.@internal.coverage.decodecounter_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸcoverageꓸdecodemeta() => builtin.initPackage(typeof(go.@internal.coverage.decodemeta_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸcoverageꓸencodecounter() => builtin.initPackage(typeof(go.@internal.coverage.encodecounter_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸcoverageꓸencodemeta() => builtin.initPackage(typeof(go.@internal.coverage.encodemeta_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸcoverageꓸslicewriter() => builtin.initPackage(typeof(go.@internal.coverage.slicewriter_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
}
