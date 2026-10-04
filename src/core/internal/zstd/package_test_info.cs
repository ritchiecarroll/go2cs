// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.@internal.zstd_package;

// <ImportedTypeAliases>
global using execꓸError = go.os.exec_package.ΔError;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
// </ImportedTypeAliases>

using go;
using static global::go.@internal.zstd_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b6461746120737472696e673b20686173682075696e7436347d", "xxHashTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20756e636f6d7072657373656420737472696e673b20636f6d7072657373656420737472696e677d", "testsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<global::go.@internal.zstd_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<testing_package.B, testing_package.TB>(Pointer = true)]
[assembly: GoImplement<testing_package.F, testing_package.TB>(Pointer = true)]
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
[assembly: go.GoPositionMap("internal/zstd/fse_test.go", "fse_test.cs", "AA0cAAcQ7gAOFsIAFjqCkpKCgoCCpoKAgqaC", "72-87:1")]
[assembly: go.GoPositionMap("internal/zstd/fuzz_test.go", "fuzz_test.cs", "ABAgAA4gsoKUgpSCggADEOKEgqiCgoKUgoSCgoKCgoKAgqaCgoKUgv7ihIKogoCUgoKEgoKCgoKChIKCAAsagoKUgoKCgoKC2A==", "39-42:1;65-83:1;97-97:1;99-139:2")]
[assembly: go.GoPositionMap("internal/zstd/window_test.go", "window_test.cs", "AAwaooKUpoKSgoKCgoKEsgAHFKKChIKChIKCgoSCloKWkoKCgoSC", "29-31:1")]
[assembly: go.GoPositionMap("internal/zstd/xxhash_test.go", "xxhash_test.cs", "ABQeAAocgoKCgoKAggAJCoKCloKCloKCgpSCgpSCloKCgviCgoKU1qKEgpSCgoKUgoSCgoKCgoCCpIKCgpaCgoKEgg==", "92-114:1")]
[assembly: go.GoPositionMap("internal/zstd/zstd_test.go", "zstd_test.cs", "ACMuACC0AYKCkpKCgoKUgoLcgoKCgpKygoKCgpSCggAKGpKCgoKmgpTmgoKClAAHGLKEhJKCgoKCgoCCgqaUgpSqooKWgoSEgoKCloLqkoKClIKCgpSCgoKU+oKCgpaCgoKygoKUgviCgoKWgoKCloKCgpaCgoCCpISCggAIDIKCgoKC3KKChISEgoSCgoKC", "116-126:1;135-146:1;158-163:1;192-204:1;264-268:1;286-303:1;309-314:1")]
// </GoSourcePositionMaps>

namespace go.@internal;

[GoPackage("zstd")]
public static partial class zstd_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha256() => builtin.initPackage(typeof(crypto.sha256_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸrace() => builtin.initPackage(typeof(go.@internal.race_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(go.@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(go.sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.@internal.zstd_package));
    }
}
