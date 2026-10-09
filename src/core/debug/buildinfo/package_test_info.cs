// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.debug.buildinfo_package;
global using static global::go.debug.buildinfo_internal_test_package;

// <ImportedTypeAliases>
global using BuildInfo = go.runtime.debug_package.BuildInfo;
global using buildinfoꓸBuildInfo = go.runtime.debug_package.BuildInfo;
global using elfꓸData = go.debug.elf_package.ΔData;
global using elfꓸSection = go.debug.elf_package.ΔSection;
global using execꓸError = go.os.exec_package.ΔError;
global using flagꓸErrorHandling = go.flag_package.ΔErrorHandling;
global using machoꓸSection = go.debug.macho_package.ΔSection;
global using machoꓸSegment = go.debug.macho_package.ΔSegment;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using peꓸSection = go.debug.pe_package.ΔSection;
global using plan9objꓸSection = go.debug.plan9obj_package.ΔSection;
global using runtimeꓸError = go.runtime_package.ΔError;
global using xcoffꓸSection = go.@internal.xcoff_package.ΔSection;
// </ImportedTypeAliases>

using go;
using static global::go.debug.buildinfo_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
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
[assembly: go.GoPositionMap("debug/buildinfo/buildinfo_test.go", "buildinfo_test.cs", "ACI0ABoM4oKUhAAIFJKCgoKCpoKWgoKoiJKCgoKAgqSCgoCCpIKCgoKCgoCCgIKkpJaSgoKAgqSCgoCCpIKCgoKCgoCCgIKkpJaCgoKUgoKUgoCCuIKCgpSCgpSCgoCCuIKC6oKCgpSUlgASIIKCgIKkAAsegoLugoIACxyCgsyCktKClIKCsoKSsoKCgIKCkoCCtoKUgoCCAA8YkoKCloKCloKUgpSC+rKCgpaCgrqCAAMQ9IKCAAMQ4oSYhpSClISEhMqEhIKChICCuKSClqKUgoKEhIKCkoCC7KKCgpSEgoKUhII=", "66-68:1;70-95:2;97-121:3;123-136:4;138-152:5;156-169:6;162-167:6.1;179-181:7;186-193:8;206-210:9;215-219:10;231-235:11;242-273:12;248-271:12.1;251-269:12.1.1;326-328:1;380-395:1;412-414:1", "", "425=Len/1/1/3")]
// </GoSourcePositionMaps>

namespace go.debug;

[GoPackage("buildinfo_test")]
public static partial class buildinfo_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct TestReadFile_cases {}
    [GoLocalName("platform")] internal partial struct TestReadFile_platform {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸdebugꓸbuildinfo() => builtin.initPackage(typeof(go.debug.buildinfo_package));
    [GoInit] internal static void initᴛᴛimportꓸdebugꓸpe() => builtin.initPackage(typeof(go.debug.pe_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸobscuretestdata() => builtin.initPackage(typeof(@internal.obscuretestdata_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸpath() => builtin.initPackage(typeof(path_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(go.path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.debug.buildinfo_package));
    }
}
