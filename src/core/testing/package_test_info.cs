// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.testing_package;

// <ImportedTypeAliases>
global using execꓸError = go.os.exec_package.ΔError;
global using flagꓸErrorHandling = go.flag_package.ΔErrorHandling;
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
using static global::go.testing_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("696e746572666163657b4973426f6f6c466c6167282920626f6f6c7d", "testFlagHelper_type")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20666e2066756e6328293b20616c6c6f637320666c6f617436347d", "allocsPerRunTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
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
[assembly: go.GoPositionMap("testing/allocs_test.go", "allocs_test.cs", "ABEWmqKioqK2goKAgg==")]
[assembly: go.GoPositionMap("testing/example_loop_test.go", "example_loop_test.cs", "AAsgAAkIgoKoyr6igoKUpoI=")]
[assembly: go.GoPositionMap("testing/flag_test.go", "flag_test.cs", "AA8e6KKCgpaEgoKCgoKClIKCuJSCAA8OkoKCloKCpJaCgpSEgpSkpKSmgg==", "29-46:1")]
[assembly: go.GoPositionMap("testing/helper_test.go", "helper_test.cs", "ACkegoKogoKWhIKCgoQAACaC+IKCgpaEgoKChISKtIKUgoCCyKKClIKUgoKCgpQ=", "89-91:1;92-94:2")]
[assembly: go.GoPositionMap("testing/helperfuncs_test.go", "helperfuncs_test.cs", "AAwckqaCgqaCpoKCpoKCppSC9saCgoKWgoKUhIKCpoK6goKUgoK6qISC1oKCgoKCgqbWgoKCgriigoKCgIK2oqSCgg==", "52-55:1;58-65:2;69-72:3;73-76:4;94-97:1;104-107:1;112-117:1")]
[assembly: go.GoPositionMap("testing/panic_test.go", "panic_test.cs", "ABMmkpKSAAoEgoQAgQHiAbKSgoKCgoKCgoCCAAsMgoKCgtaCgpSCgpS2pIKCgqaCgoKCgoKCgriClIKCgoKCgrgACwyihAAOJIKCgoKCgoKAggAJCoKCloKCyoKCloCSgg==", "141-152:1;176-181:1;184-209:2;187-192:2.1;200-205:2.2;252-256:1;253-255:1.1;264-264:1")]
[assembly: go.GoPositionMap("testing/testing_test.go", "testing_test.cs", "ACI8soKWAAUcAAoChIKClJaCgpSC6KKClIKmAA4KgoKCgoKCgoKCggAIBoKCpKSClJSClLSCyoKClIKClIKUgoKClIKUgoKUgpaCgIIADAiiABY8soKAgraWkoKCqIKClILKooKAgsSkooSCoqSihIKi1IKEooSi5oKEgqKEogAICIKmgqaCpoKmgqaCpoKmgqaCpoIACwaigoKUloKClIKmlgAZNpKClIKWhIKClIKWuICSyIKogoKUgsbslpKCABgiooKUgpSCloKUhIKAgqaCgoKWgoIACAyihISCgoKChKqigoKSgpSC5oKUgpSWloKCgpSC+pKCgpaEggAICIKCgoKSkoKClJKCgqaClsyCgoKUgrqCgoKUguiCgoKCgqaWloKCgpSCuoKCgpSCAAgIgoKCgoKmlJaEgpSClIK4goKCgoKSgoKWkpKCgpSSgoKoloSClIKUguiCgoKClJaEgpSClILogoKCgoKEhIKClILogoKElIKUguiCgoSUgpSCuKKClIK4goKUguiCgoS4gpSCAAgIgoKWgoKClJaCgqiCgqjWgryCgoKCgoKCggAGEIKCgoKCgoCC7t6CgpSClJSCuKKEgoKogoKogoLegoKCgoKCgIIABxC6goKUgpSUgriigoKUgIKAgoLaloKopqiCkoKCgpKCgqaCggAIBsiCgpIACgqCgoCCpoKCgoCCtoKCqJKCAAkKgoKEgoKogoKCloKC6IKCqIKCuoKCgriCgoK4ooKC", "52-57:1;53-55:1.1;69-78:1;70-75:1.1;96-114:1;189-194:1;230-234:1;240-246:1;241-245:1.1;335-365:1;457-460:1;468-470:1;505-516:1;506-510:1.1;511-515:1.2;552-555:1;585-592:1;586-590:1.1;587-589:1.1.1;614-618:1;620-631:2;621-625:2.1;626-630:2.2;651-654:1;756-762:1;764-768:2;770-774:3;788-798:1;791-796:1.1;842-857:1;846-850:1.1;853-855:1.2;928-932:1;943-948:1;945-947:1.1;958-963:1;964-968:2;970-974:3")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("testing_test")]
public static partial class testing_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface testFlagHelper_type {}
    internal partial struct TestChdir_type {}
    internal partial struct TestMorePanic_testCases {}
    internal partial struct TestPanic_testCases {}
    internal partial struct TestSetenv_tests {}
    internal partial struct allocsPerRunTestsᴛ1 {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸrace() => builtin.initPackage(typeof(@internal.race_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrandꓸv2() => builtin.initPackage(typeof(math.rand.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.testing_package));
    }
}
