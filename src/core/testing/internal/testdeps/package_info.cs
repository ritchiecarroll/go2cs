// go2cs code converter defines `global using` statements here for imported type
// aliases as package references are encountered via `import' statements. Exported
// type aliases that need a `global using` declaration will be loaded from the
// referenced package by parsing its 'package_info.cs' source file and reading its
// defined `GoTypeAlias` attributes.

// Package name separator "dot" used in imported type aliases is extended Unicode
// character '\uA4F8' which is a valid character in a C# identifier name. This is
// used to simulate Go's package level type aliases since C# does not yet support
// importing type aliases at a namespace level.

// <ImportedTypeAliases>
global using fuzzꓸCorpusEntry = go.@internal.fuzz_package.CorpusEntryᴛ1;
using io = go.io_package;
using reflect = go.reflect_package;
using time = go.time_package;
// </ImportedTypeAliases>

using go;
using static go.testing.@internal.testdeps_package;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

// For encountered type alias declarations, e.g., `type Table = map[string]int`,
// go2cs code converter will generate a `global using` statement for the alias in
// the converted source, e.g.: `global using Table = go.map<go.@string, nint>;`.
// Although scope of `global using` is available to all files in the project, all
// converted Go code for the project targets the same package, so `global using`
// statements will effectively have package level scope.

// Additionally, `GoTypeAlias` attributes will be generated here for exported type
// aliases. This allows the type alias to be imported and used from other packages
// when referenced.

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<testLog, go.@internal.testlog_package.Interface>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(testLog))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("testing/internal/testdeps/deps.go", "deps.cs", "ACtOsoKCgoKmpoKmgqaC7ILaggANFILmguaC5oKo4oKmgoKClIKCgqIACQiCgoLKgoKUpqKCgoKC2JKmAAYagpIACxiClNYACA6CkoKClNaCpoKmgqaCABQiwoKUAAoGooKCgoKUlIKCgpQ=", "", "", "", "198:21,231:21")]
// </GoSourcePositionMaps>

namespace go.testing.@internal;

using static global::go.testing.@internal.testdeps_package;
using io = global::go.io_package;
using reflect = global::go.reflect_package;
using time = global::go.time_package;

[GoPackage("testdeps")]
public static partial class testdeps_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct testLog {}
    public partial struct TestDeps {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸfuzz() => builtin.initPackage(typeof(global::go.@internal.fuzz_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestlog() => builtin.initPackage(typeof(global::go.@internal.testlog_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸsignal() => builtin.initPackage(typeof(global::go.os.signal_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸruntimeꓸpprof() => builtin.initPackage(typeof(runtime.pprof_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(testdeps_package.TestDeps)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class testdepsᴛTestDeps;
[GoMethodHost(typeof(testdeps_package.testLog)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class testdepsᴛtestLog;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class testdepsᴛTestDeps {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (bool result, error err) MatchString(this TestDeps _, @string pat, @string str) => testdeps_package.MatchString⁔(_, pat, str);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error StartCPUProfile(this TestDeps _, io.Writer w) => testdeps_package.StartCPUProfile⁔(_, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void StopCPUProfile(this TestDeps _) => testdeps_package.StopCPUProfile⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error WriteProfileTo(this TestDeps _, @string name, io.Writer w, nint debug) => testdeps_package.WriteProfileTo⁔(_, name, w, debug);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string ImportPath(this TestDeps _) => testdeps_package.ImportPath⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string ModulePath(this TestDeps _) => testdeps_package.ModulePath⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void StartTestLog(this TestDeps _, io.Writer w) => testdeps_package.StartTestLog⁔(_, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error StopTestLog(this TestDeps _) => testdeps_package.StopTestLog⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetPanicOnExit0(this TestDeps _, bool v) => testdeps_package.SetPanicOnExit0⁔(_, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error /*err*/ CoordinateFuzzing(this TestDeps _, time.Duration timeout, int64 limit, time.Duration minimizeTimeout, int64 minimizeLimit, nint parallel, slice<fuzzꓸCorpusEntry> seed, slice<reflect.Type> types, @string corpusDir, @string cacheDir) => testdeps_package.CoordinateFuzzing⁔(_, timeout, limit, minimizeTimeout, minimizeLimit, parallel, seed, types, corpusDir, cacheDir);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error RunFuzzWorker(this TestDeps _, Func<fuzzꓸCorpusEntry, error> fn) => testdeps_package.RunFuzzWorker⁔(_, fn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<fuzzꓸCorpusEntry>, error) ReadCorpus(this TestDeps _, @string dir, slice<reflect.Type> types) => testdeps_package.ReadCorpus⁔(_, dir, types);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error CheckCorpus(this TestDeps _, slice<any> vals, slice<reflect.Type> types) => testdeps_package.CheckCorpus⁔(_, vals, types);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void ResetCoverage(this TestDeps _) => testdeps_package.ResetCoverage⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SnapshotCoverage(this TestDeps _) => testdeps_package.SnapshotCoverage⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (@string mode, Func<@string, @string, (@string, error)> tearDown, Func<float64> snapcov) InitRuntimeCoverage(this TestDeps _) => testdeps_package.InitRuntimeCoverage⁔(_);
}
partial class testdepsᴛtestLog {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Getenv(this ж<testLog> Ꮡl, @string key) => testdeps_package.Getenv⁔(Ꮡl, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Open(this ж<testLog> Ꮡl, @string name) => testdeps_package.Open⁔(Ꮡl, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Stat(this ж<testLog> Ꮡl, @string name) => testdeps_package.Stat⁔(Ꮡl, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Chdir(this ж<testLog> Ꮡl, @string name) => testdeps_package.Chdir⁔(Ꮡl, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void add(this ж<testLog> Ꮡl, @string op, @string name) => testdeps_package.add⁔(Ꮡl, op, name);
}
// </MethodForwarders>
