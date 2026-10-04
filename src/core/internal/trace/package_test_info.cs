// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.@internal.trace_package;
global using static global::go.@internal.trace_internal_test_package;

// <ImportedTypeAliases>
global using flagꓸErrorHandling = go.flag_package.ΔErrorHandling;
global using oldtraceꓸSTWReason = go.@internal.trace.@internal.oldtrace_package.ΔSTWReason;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using runtimeꓸError = go.runtime_package.ΔError;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
global using traceꓸEvent = go.@internal.trace_package.ΔEvent;
global using traceꓸLabel = go.@internal.trace_package.ΔLabel;
global using traceꓸLog = go.@internal.trace_package.ΔLog;
global using traceꓸMetric = go.@internal.trace_package.ΔMetric;
global using traceꓸRange = go.@internal.trace_package.ΔRange;
global using traceꓸRegion = go.@internal.trace_package.ΔRegion;
global using traceꓸStack = go.@internal.trace_package.ΔStack;
global using traceꓸStateTransition = go.@internal.trace_package.ΔStateTransition;
global using traceꓸTask = go.@internal.trace_package.ΔTask;
global using traceꓸTime = go.@internal.trace_package.ΔTime;
// </ImportedTypeAliases>

using go;
using static global::go.@internal.trace_test_package;

// <ExportedTypeAliases>
[assembly: GoTypeAlias("Event", "ΔEvent")]
[assembly: GoTypeAlias("Label", "ΔLabel")]
[assembly: GoTypeAlias("Log", "ΔLog")]
[assembly: GoTypeAlias("Metric", "ΔMetric")]
[assembly: GoTypeAlias("Range", "ΔRange")]
[assembly: GoTypeAlias("Region", "ΔRegion")]
[assembly: GoTypeAlias("Stack", "ΔStack")]
[assembly: GoTypeAlias("StateTransition", "ΔStateTransition")]
[assembly: GoTypeAlias("Task", "ΔTask")]
[assembly: GoTypeAlias("Time", "ΔTime")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>(Pointer = true)]
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
[assembly: go.GoPositionMap("internal/trace/gc_test.go", "gc_test.cs", "ABEkooKUgoIACAaC3gAHEIQACRyAgqTKgpSCgoIACg7GlJSiqIKCgoIABhCAkoKCgoKCgoKCgtiSkoKCgpSCgoKUgoKClIKUpriigIKmqIKCgoKCgpSClJSSgoKUuqaCgpSCgoKUlJSC", "87-118:1;104-104:1.1;119-142:2;154-166:1;167-174:2")]
[assembly: go.GoPositionMap("internal/trace/oldtrace_test.go", "oldtrace_test.cs", "ABcgooKClIKCgoKClKKCgpSUgoKWgoKCgoKClJSAgqaCggAKFpSAgsaCtoKC+qSC", "28-84:1")]
[assembly: go.GoPositionMap("internal/trace/reader_test.go", "reader_test.cs", "ABgwgtaCgoKUgoKCgpSCgoKUyoi0goKClIKCgpaCppSkpKSkgqSkpAAMEqKCgoCCpJSCgoKClIKAgqSUgpSAgraAgsiChIKClIKCgpSCgoKUgpSAgrbmooSCgpSCgoKUkoCCpA==", "39-45:1;55-93:1")]
[assembly: go.GoPositionMap("internal/trace/summary_test.go", "summary_test.cs", "ABgcgoKsgoSCgoKAgqSAgraClIKUggALCIKCAAwggoKCgoKUgqaCAA4IgoIAABCCgpQAR5ABgoKClIKogoKktqiCgpSCgIKUtoKCuoKUgoLMgpSCgoKClILMlIK4goKCptaigoKUhJSClIKCuIKCgqaWgoKmgpKClIKUlOailIK2gpSCgpSCgqaCgpSClICC2KaUgraClIKClIKCpoKClIKUgILYAAsIooKUgpSClIKUgpSCgqaCgsqCgoKCqIKCmJKCgoKUgpSogoLsgoCClLaCgg==", "89-93:1")]
[assembly: go.GoPositionMap("internal/trace/trace_test.go", "trace_test.cs", "ACYwgoIACiCCgpSCgoKUgpSCgpSClIKkgqSCpIKCgriCggAIDILWgoSUpOaipoKCgoKCgoKClIKUgIKkgt6CgoKCgpSCgoKSgpSClIKUgoKCgoKCgqaCpoKmgqQABhCCgoKCqIKCgoKUgriCgoKogoKClAAICqKOAAwMgoKCgpSCgoKUgsqCgpSCAAkUgpSCgpSCgoKUgpSClLaClLaClLaClLaUgvqC1oIAEAaiggABFgBPngHuyJSCABImgoKClICCtpSCgpSCgoKUgpSCgpSClIKClIKkpIKkgqSCgoKCuIKCAAgMgpSk1oKUpNaC1oKUgqTWgqaCkoSCgpSCgoKUgpSWloKEgJKkooKAkoIAEwqCloKEgoKUgoKUgpSClJSUlIzSgpSAgoKUpJaWgqi4goLulMimkpSSgpSUkoKU", "25-82:1;100-209:1;213-302:1;314-496:1;438-448:1.1;533-552:1;578-644:1;645-647:2;648-653:3;654-659:4")]
// </GoSourcePositionMaps>

namespace go.@internal;

[GoPackage("trace_test")]
public static partial class trace_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct TestMMU_type {}
    [GoLocalName("region")] internal partial struct TestSummarizeGoroutinesRegionsTrace_region {}
    [GoLocalName("task")] internal partial struct TestSummarizeTasksTrace_task {}
    [GoLocalName("evDesc")] internal partial struct TestTraceAnnotations_evDesc {}
    [GoLocalName("evDesc")] internal partial struct TestTraceStacks_evDesc {}
    [GoLocalName("frame")] internal partial struct TestTraceStacks_frame {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸrace() => builtin.initPackage(typeof(go.@internal.race_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(go.@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtrace() => builtin.initPackage(typeof(go.@internal.trace_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtraceꓸraw() => builtin.initPackage(typeof(go.@internal.trace.raw_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtraceꓸtesttrace() => builtin.initPackage(typeof(go.@internal.trace.testtrace_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtraceꓸversion() => builtin.initPackage(typeof(go.@internal.trace.version_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(go.math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.@internal.trace_package));
    }
}
