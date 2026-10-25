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

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: go.GoHoistedLiterals]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("testing/allocs_test.go", "allocs_test.cs", "ABAWmqKioqK2goKAgg==")]
[assembly: go.GoPositionMap("testing/example_loop_test.go", "example_loop_test.cs", "AAsg+IKCuMrugoKClKaC")]
[assembly: go.GoPositionMap("testing/flag_test.go", "flag_test.cs", "AA8e6IKCgqaUgoKCgoKUgoK4lIIADw6SgoKmgoKSgqaCgpSUgpSkpKSmgg==", "28-45:1")]
[assembly: go.GoPositionMap("testing/helper_test.go", "helper_test.cs", "ACkegoK4goKmlIKCgpQAACaC+IKCgqaUgoKClJSaxIKUgoCCyKKClIKUgoKCgpQ=", "89-91:1;92-94:2")]
[assembly: go.GoPositionMap("testing/helperfuncs_test.go", "helperfuncs_test.cs", "AAwckqaCgqaCpoKCpoKCppSC9qaCgoKmgoKUlIKCpoLKgoKUgoLKuJSC1oKCgoKCgqbWgoKCgriigoKCgIK2oqSCgg==", "52-55:1;58-65:2;69-72:3;73-76:4;94-97:1;104-107:1;112-117:1")]
[assembly: go.GoPositionMap("testing/panic_test.go", "panic_test.cs", "ABQmkpKSAAoEgpQAkgGEArKSgoKCgoKCgoCCAAsMgoKCgtaCgpSCgoKUtpSCgoKmkoKCgoKCgriClIKCgoKCgoK4AAsMopQADySCgoKCgoKCgIIACQqCgqaCgsqCgqaAkoI=", "158-169:1;194-199:1;201-227:2;204-209:2.1;218-223:2.2;270-274:1;271-273:1.1;282-282:1")]
[assembly: go.GoPositionMap("testing/testing_test.go", "testing_test.cs", "ACQ8soKmABAcgpSCgpSmgoKUguiCgpSCpgAOCoKCgoKCgoKCgoKCAAgGgoKkpIKUlIKUtILagoKUgoKUgpSCgoKUgpSCgpSCpoKAggAICAAIDoKAgqSUlIKCpoKmgoKUgrQACgSiABc8soKAgramkoKCuIKClILKwoKAgsSkopSCoqSilIKi1IKUopSi5oKUgqKUogAICIKmgqaCpoKmgqaCpoKmgqaCpoIACwaigoKUpoKClIKmpgAaNpKClIKmlIKClIKmyICCyIK4goKUgsbslpKCABgiooKUgpSCpoKUlIKAgraCgoKmgoIACAzClJSCgoKCgpSqooKCkoKUguaClIKUpqaCgoKUgvqSgoKmlIIACAiCgoKCkpKCgpSSgoKmgqbcgoKClILKgoKClILogoKCgoKmpqaCgoKUgsqCgoKUggAICIKCgoKCppSmlIKUgpSCuIKCgoKCkoKCppKSgoKUkoKCuKaUgpSClILogoKCgpSmlIKUgpSC6IKCgoKClJSCgpSC6IKClJSClILogoKUlIKUgriigpSCuIKClILogoKUuIKUggAICIKCpoKCgpSmgoK4goK41oLcgoKCgoKCgoIABxCCgoKCgoKAgv7ugoKUgpSUgriClIKCuIKCuIKC7oKCgoKCgoCCAAgQyoKClIKUlIK4ooKClICCgIKC6qaCuKa4gpKCgoKSgoKmgoIACAa4goKCAAoKgoKAgraCgoKAgraCgriSgv6i5oKUgoIACwiCAAgYgrKSpqaUgoKCAAkOguaCgoKCpoKCgtaCgoKClIKClILogoK4goKCpoKCgriCgoKmooKCgriClIKCgpSmgoKUgqaCgoK4goIACAiCgpSCgriCgoKmgoLogoK4goLKgoKCuIKCgriigoK4gg==", "52-57:1;53-55:1.1;69-78:1;70-75:1.1;97-115:1;223-228:1;264-268:1;274-280:1;275-279:1.1;369-399:1;492-495:1;503-505:1;540-551:1;541-545:1.1;546-550:1.2;587-590:1;620-627:1;621-625:1.1;622-624:1.1.1;649-653:1;655-666:2;656-660:2.1;661-665:2.2;686-689:1;791-797:1;799-803:2;805-809:3;823-833:1;826-831:1.1;877-892:1;881-885:1.1;888-890:1.2;963-967:1;978-983:1;980-982:1.1;993-998:1;999-1003:2;1005-1009:3;1042-1044:1;1094-1096:1;1107-1109:1")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("testing_test")]
public static partial class testing_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface testFlagHelper_type {}
    internal partial struct TestAttrInvalid_tests {}
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
