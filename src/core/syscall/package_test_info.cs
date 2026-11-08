// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.syscall_package;
global using static global::go.syscall_internal_test_package;

// <ImportedTypeAliases>
global using syscallꓸHandle = go.syscall_package.ΔHandle;
// </ImportedTypeAliases>

using go;
using static global::go.syscall_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b73747220737472696e673b2077737472205b5d75696e7431367d", "wtf8testsᴛ1")]
[assembly: GoTypeAlias("Handle", "ΔHandle")]
[assembly: GoTypeAlias("Signal", "ΔSignal")]
[assembly: GoTypeAlias("Sockaddr", "ΔSockaddr")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(go.os.exec_package.Cmd))]
[assembly: GoFieldAccess(typeof(os_package.Process))]
[assembly: GoFieldAccess(typeof(syscall_package.Timeval))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("syscall/exec_windows_test.go", "exec_windows_test.cs", "ABsmggAWMoKCAAkKggASLIKCggAOCqK4gqaUgoKClIKCgpTKgoKCgpSSgsqCgoKkgpSUgqaCgoKUgoKUkrQ=", "115-118:1", "", "", "", "137:21,138:13")]
[assembly: GoPositionMap("syscall/syscall_test.go", "syscall_test.cs", "AA8egoKClIKClIIACQiClKqilIKCguiCgpSCgpSC")]
[assembly: GoPositionMap("syscall/syscall_windows_test.go", "syscall_windows_test.cs", "ABskgpSCgoKClJQACyCCgoKUgvqCgoKUggANCIKUgoKClISOlIKCgpSCgqaCuIIADAaigoKUgoKClIKCgoKipIKCADcIgoKClJYAAR6CgoKUgoKCgqgAARaCgoKCgpSCgqaCgqaCgoKCuJSCyoKUgoKUgoKmpoKCgpQACQiiyoKClIKCgqaCgoKCpoLKgoKClJSCgoKUtKSigoKCgoKClIKCgoKUlJSClILKooKC", "247-258:1;248-257:1.1;249-253:1.1.1;260-266:2;267-273:3;285-301:1")]
[assembly: GoPositionMap("syscall/wtf8_windows_test.go", "wtf8_windows_test.cs", "ABUiAHHqAYKykoKCgtyCspKCgtyigpSUgoK4goLcooKClIKCgriCgriCgg==", "136-142:1;148-153:1;161-174:1;182-199:1")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("syscall_test")]
public static partial class syscall_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct TestEnvBlockSorted_tests {}
    internal partial struct TestEscapeArg_type {}
    internal partial struct TestOpen_tests {}
    [GoLocalName("X")] [GoValueClone("fd", "pad")] internal partial struct TestWin32finddata_X {}
    internal partial struct wtf8testsᴛ1 {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsyscall() => builtin.initPackage(typeof(syscall_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(unicode.utf8_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.syscall_package));
    }
}
