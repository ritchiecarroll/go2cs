// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.go.@internal.gccgoimporter_package;

// <ImportedTypeAliases>
global using constantꓸKind = go.go.constant_package.ΔKind;
global using elfꓸData = go.debug.elf_package.ΔData;
global using elfꓸSection = go.debug.elf_package.ΔSection;
global using execꓸError = go.os.exec_package.ΔError;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using tokenꓸFile = go.go.token_package.ΔFile;
global using tokenꓸPos = go.go.token_package.ΔPos;
global using tokenꓸPosition = go.go.token_package.ΔPosition;
global using typesꓸError = go.go.types_package.ΔError;
global using typesꓸInfo = go.go.types_package.ΔInfo;
global using typesꓸScope = go.go.types_package.ΔScope;
global using typesꓸSignature = go.go.types_package.ΔSignature;
global using typesꓸTerm = go.go.types_package.ΔTerm;
global using typesꓸType = go.go.types_package.ΔType;
global using xcoffꓸSection = go.@internal.xcoff_package.ΔSection;
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static global::go.go.@internal.gccgoimporter_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b696420737472696e673b2074797020737472696e673b2077616e7420737472696e673b20756e6465726c79696e6720737472696e673b206d6574686f647320737472696e677d", "typeParserTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<importerTest, ж<importerTest>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: global::go.GoPositionMap("go/internal/gccgoimporter/gccgoinstallation_test.go", "gccgoinstallation_test.cs", "ABYiAIMBigK0goKWgoKClKiCgoKCqIKCgroAChA=")]
[assembly: global::go.GoPositionMap("go/internal/gccgoimporter/importer_test.go", "importer_test.cs", "AB4wooKCgpaCgoKCloKCloKCgrqCgpSCgoKoggAMGgAZNIKCgoSy/rKCgpSAgqTmlIKCloKCgpSCgpSCgpSCgpSEgoKEgoKEspSCloKAgqSChIKCgoKWhIKClIKCgoKWhICCpICC")]
[assembly: global::go.GoPositionMap("go/internal/gccgoimporter/parser_test.go", "parser_test.cs", "ABUeABQogoKCgoKCgoKEgqiAgqaCgpaCgoKogoKCgpSCgg==")]
// </GoSourcePositionMaps>

namespace go.go.@internal;

[GoPackage("gccgoimporter")]
public static partial class gccgoimporter_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
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
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtypes() => builtin.initPackage(typeof(global::go.go.types_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(global::go.@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(global::go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtextꓸscanner() => builtin.initPackage(typeof(text.scanner_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.go.@internal.gccgoimporter_package));
    }
}
