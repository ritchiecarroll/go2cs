// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.log.slog_package;
global using static global::go.log.slog_internal_test_package;

// <ImportedTypeAliases>
global using flagꓸErrorHandling = go.flag_package.ΔErrorHandling;
global using httpꓸCookie = go.net.http_package.ΔCookie;
global using httpꓸHandler = go.net.http_package.ΔHandler;
global using httpꓸHeader = go.net.http_package.ΔHeader;
global using jsonꓸToken = object;
global using jsonꓸΔToken = object;
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
global using slogꓸHandler = go.log.slog_package.ΔHandler;
global using slogꓸKind = go.log.slog_package.ΔKind;
global using slogꓸLevel = go.log.slog_package.ΔLevel;
global using slogꓸLogValuer = go.log.slog_package.ΔLogValuer;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static global::go.log.slog_test_package;

// <ExportedTypeAliases>
[assembly: GoTypeAlias("Handler", "ΔHandler")]
[assembly: GoTypeAlias("Kind", "ΔKind")]
[assembly: GoTypeAlias("Level", "ΔLevel")]
[assembly: GoTypeAlias("LogValuer", "ΔLogValuer")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<LevelHandler, go.log.slog_package.ΔHandler>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<global::go.log.slog_package.LevelVar, encoding_package.TextMarshaler>(Pointer = true)]
[assembly: GoImplement<global::go.log.slog_package.LevelVar, encoding_package.TextUnmarshaler>(Pointer = true)]
[assembly: GoImplement<global::go.log.slog_package.ΔLevel, encoding_package.TextMarshaler>]
[assembly: GoImplement<global::go.log.slog_package.ΔLevel, encoding_package.TextUnmarshaler>(Pointer = true)]
[assembly: GoImplement<global::go.log.slog_package.ΔLevel, global::go.log.slog_package.Leveler>]
[assembly: GoImplement<go.log.slog.@internal.buffer_package.Buffer, io_package.Writer>(Pointer = true)]
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
[assembly: go.GoPositionMap("log/slog/example_level_handler_test.go", "example_level_handler_test.cs", "ABcutICCpKqiqLKokqiSqJIABhYACAKCgoI=")]
[assembly: go.GoPositionMap("log/slog/example_wrap_test.go", "example_wrap_test.cs", "ABYm0oKUgoKC1oKUgqaCgpSUgg==", "30-41:1")]
[assembly: go.GoPositionMap("log/slog/slogtest_test.go", "slogtest_test.cs", "ABokggAGEJKCgpKCgpSUgILsgoKCgpSCgpSUpoKCgIKkAAIS4oKCgoKCgpSUgoKCgoKClIKCqJSClA==", "24-24:1;25-25:2;27-40:3;30-36:3.1")]
// </GoSourcePositionMaps>

namespace go.log;

[GoPackage("slog_test")]
public static partial class slog_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct TestSlogtest_type {}
    public partial struct LevelHandler {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸrace() => builtin.initPackage(typeof(go.@internal.race_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(go.@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸlogꓸslog() => builtin.initPackage(typeof(go.log.slog_package));
    [GoInit] internal static void initᴛᴛimportꓸlogꓸslogꓸinternalꓸbuffer() => builtin.initPackage(typeof(go.log.slog.@internal.buffer_package));
    [GoInit] internal static void initᴛᴛimportꓸlogꓸslogꓸinternalꓸslogtest() => builtin.initPackage(typeof(go.log.slog.@internal.slogtest_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸslogtest() => builtin.initPackage(typeof(go.testing.slogtest_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.log.slog_package));
    }
}
