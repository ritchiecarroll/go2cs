// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.runtime.debug_package;

// <ImportedTypeAliases>
global using execꓸError = go.os.exec_package.ΔError;
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
using static global::go.runtime.debug_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<G<G<nint>>, I>]
[assembly: GoImplement<G<nint>, I>]
[assembly: GoImplement<os_package.File, io_package.Reader>(Pointer = true)]
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
[assembly: go.GoPositionMap("runtime/debug/example_monitor_test.go", "example_monitor_test.cs", "AA8mwgACIAANApaCAAoS4oKUgoSCgpSUqIKClICCpICCpLqCgpSCgoKCgoKUgoCC")]
[assembly: go.GoPositionMap("runtime/debug/garbage_test.go", "garbage_test.cs", "ABAgooSCgriCgpaChIKUgpSClIKClIKUgoKCgpSClIKUqIKCgpaCgrqClIKCgoKUtMi6gpaEgqgACBSEgrqCAAkWgoKClJSUggAGEqKWgoKCqIKClIKEkoKCgoKCgpaSgIK2gpSUgoKAgriClJSCgpSCgoKCtKSCgpSmAAgSgg==", "169-172:1")]
[assembly: go.GoPositionMap("runtime/debug/heapdump_test.go", "heapdump_test.cs", "ABMcooKUgoKUgpKCgoKUgoCCxOyo0oKUgoKUgqaCgoKCloKWgoKiAAkQtJKUooKUgoKUgpKCgqI=")]
[assembly: go.GoPositionMap("runtime/debug/mod_test.go", "mod_test.cs", "AA0ekoKClAAhBrSclgABEJ6MgoKUgrqCgoKWgg==", "55-74:1")]
[assembly: go.GoPositionMap("runtime/debug/stack_test.go", "stack_test.cs", "AB8sgpSCpoKClICCpIK4yoKkggANKgAUAoKCgt6CgNyCgoKClIKCgoKUuKSCgpaCkoSCgpSEhIKClJSEgoKCguaCgoKCloSCgoKCgoKUAAoYgoKUgoKWyoKUgrqCgpSC", "113-129:1")]
// </GoSourcePositionMaps>

namespace go.runtime;

[GoPackage("debug_test")]
public static partial class debug_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    public partial interface I {}
    public partial struct G<T> {}
    public partial struct Obj {}
    public partial struct T {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸruntimeꓸdebug() => builtin.initPackage(typeof(go.runtime.debug_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.runtime.debug_package));
    }
}
