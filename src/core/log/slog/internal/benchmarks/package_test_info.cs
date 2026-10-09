// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.log.slog.@internal.benchmarks_package;

// <ImportedTypeAliases>
global using flagꓸErrorHandling = go.flag_package.ΔErrorHandling;
global using slogꓸHandler = go.log.slog_package.ΔHandler;
global using slogꓸKind = go.log.slog_package.ΔKind;
global using slogꓸLevel = go.log.slog_package.ΔLevel;
global using slogꓸLogValuer = go.log.slog_package.ΔLogValuer;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static global::go.log.slog.@internal.benchmarks_internal_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<global::go.log.slog.@internal.benchmarks_package.asyncHandler, go.log.slog_package.ΔHandler>(Pointer = true)]
[assembly: GoImplement<global::go.log.slog.@internal.benchmarks_package.disabledHandler, go.log.slog_package.ΔHandler>]
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
[assembly: go.GoPositionMap("log/slog/internal/benchmarks/benchmarks_test.go", "benchmarks_test.cs", "ABQiggAVDrKCAAkWgrKClAAPFgAKGAAKGAAQJAAuWpKCkoI=", "39-150:1;53-61:1.1;65-73:1.2;77-90:1.3;95-138:1.4;141-148:1.5;143-147:1.5.1", "", "83=String/1/1/1,Int/1/1/2,Duration/1/1/3,Time/1/1/4,Any/1/1/5;94=String/1/1/1,Int/1/1/2,Duration/1/1/3,Time/1/1/4,Any/1/1/5;105=String/1/2/1,Int/1/2/2,Duration/1/2/3,Time/1/2/4,Any/1/2/5,String/2/2/6,Int/2/2/7,Duration/2/2/8,Time/2/2/9,Any/2/2/10;122=String/1/8/1,Int/1/8/2,Duration/1/8/3,Time/1/8/4,Any/1/8/5,String/2/8/6,Int/2/8/7,Duration/2/8/8,Time/2/8/9,Any/2/8/10,String/3/8/11,Int/3/8/12,Duration/3/8/13,Time/3/8/14,Any/3/8/15,String/4/8/16,Int/4/8/17,Duration/4/8/18,Time/4/8/19,Any/4/8/20,String/5/8/21,Int/5/8/22,Duration/5/8/23,Time/5/8/24,Any/5/8/25,String/6/8/26,Int/6/8/27,Duration/6/8/28,Time/6/8/29,Any/6/8/30,String/7/8/31,Int/7/8/32,Duration/7/8/33,Time/7/8/34,Any/7/8/35,String/8/8/36,Int/8/8/37,Duration/8/8/38,Time/8/8/39,Any/8/8/40")]
[assembly: go.GoPositionMap("log/slog/internal/benchmarks/handlers_test.go", "handlers_test.cs", "ABUegoKSgqKCgoCCpIKCpqKCgIKkgoLKooKAgJI=", "19-29:1;30-39:2;44-44:1")]
// </GoSourcePositionMaps>

namespace go.log.slog.@internal;

[GoPackage("benchmarks")]
public static partial class benchmarks_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸrace() => builtin.initPackage(typeof(go.@internal.race_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlogꓸslog() => builtin.initPackage(typeof(go.log.slog_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.log.slog.@internal.benchmarks_package));
    }
}
