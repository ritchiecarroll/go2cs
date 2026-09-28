// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.flag_package;
global using static global::go.flag_internal_test_package;

// <ImportedTypeAliases>
global using execꓸError = go.os.exec_package.ΔError;
global using flagꓸErrorHandling = go.flag_package.ΔErrorHandling;
global using netꓸAddr = go.net_package.ΔAddr;
global using netꓸError = go.net_package.ΔError;
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
global using urlꓸError = go.net.url_package.ΔError;
// </ImportedTypeAliases>

using go;
using static global::go.flag_test_package;

// <ExportedTypeAliases>
[assembly: GoTypeAlias("ErrorHandling", "ΔErrorHandling")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<URLValue, flag_package.Value>(Pointer = true)]
[assembly: GoImplement<boolFlagVar, flag_package.Value>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<flagVar, flag_package.Value>(Pointer = true)]
[assembly: GoImplement<interval, flag_package.Value>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
[assembly: GoImplement<zeroPanicker, flag_package.Value>(Pointer = true)]
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
[assembly: go.GoPositionMap("flag/example_test.go", "example_test.cs", "ABkwgpiCAAQQotwACAyClIKCgpSUAAgSpqY=")]
[assembly: go.GoPositionMap("flag/example_value_test.go", "example_value_test.cs", "ABEigoKUpoKAgpSkAAkKgoKEgg==")]
[assembly: go.GoPositionMap("flag/flag_test.go", "flag_test.cs", "AB4wgoKUABwGgoKCgoKCgoKCgoKEkoKCgoKClLS0tLS0griCgoKCpoKCgoKCuIKCgoKCgoKCgoKCgoKCgqiSgJKCuIKCgoKCgoKCgoSCgoKCgpSUpKSkpKSkpKSCuOaCgoCSgpSCABwIooKUgoKCgoKCgoKCggALGICCpIKUgpSClIKUgpSClIKUgpSClIKUgqTogoCS1oLMgqaCguaCgoKCgoKAgqSClIKC+IKCgoKCgpSAgqSClIKAgqaSgoKAgraCgpiAgraAgqKCAAgIgoKCgJKCggAIEoKmgoKUpoKmgoKCgoKCgoCCgriCloLogoKCgoKCgpSCgoKClJSCgoKC6IKCgoKCgoCCAAgMwoCSgpCSgoKAgqSCgoKChIIAFQqSgoKAkoKUgoKUgpSCgqaCgpSClIKWkoKCgoKUggAKFoKCpoKClAA8TIKCgoKCgoKCgoKCgoKCgoKCgoKCgoL6koKUgoKCgIKkgIL6soKCgoCSgoKCgIIACwiCgoKChIKUgpSCloKCgoKEgpSClIIACAiCgoKCgoKCgoKClIKCgoKUgvqC7oKCgoKCgoKUgoKClIIADwqChIKCgoKClIKWABc6ooK6gpSClILcooKCpLSAgua2AAoGggAJHLKEgoKEooKUgIIADQqCAAkcgoSCgoSChJKUgIIACQqCgoKCgoKUgIKkgpSCgIKmkoKCgIK2goKYgIK2gIKiggAMCKKUlpI=", "41-41:1;42-42:2;46-66:3;103-103:4;120-149:1;155-155:1;231-231:1;273-276:1;297-299:2;315-315:1;403-403:1;405-405:2;426-426:1;566-566:1;725-736:1;762-765:1;797-799:1;810-813:1;834-836:2;855-857:1")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("flag_test")]
public static partial class flag_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct TestExitCode_tests {}
    internal partial struct TestInvalidFlags_tests {}
    internal partial struct TestRedefinedFlags_tests {}
    internal partial struct boolFlagVar {}
    internal partial struct flagVar {}
    internal partial struct interval {}
    internal partial struct zeroPanicker {}
    public partial struct URLValue {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸurl() => builtin.initPackage(typeof(go.net.url_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.flag_package));
    }
}
