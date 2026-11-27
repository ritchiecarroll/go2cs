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
using io = go.io_package;
using time = go.time_package;
// </ImportedTypeAliases>

using go;
using static go.runtime.trace_package;
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
[assembly: GoFieldAccess(typeof(FlightRecorder))]
[assembly: GoFieldAccess(typeof(Task))]
[assembly: GoFieldAccess(typeof(traceMultiplexer))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("runtime/trace/annotation.go", "annotation.cs", "ABFMABYCgoKCggAVLIKClAAHFLLalKaqsoKosqaCAAYgABgggoKCogACFAAIAoKUgoIACBKWsoKUrLIAAhDmpqY=")]
[assembly: GoPositionMap("runtime/trace/batch.go", "batch.cs", "ABQuooKUgqaAgoKUpICCpIKmgoKUgqaCgpSCpoKClIKmgoKUgpSCgoKUpg==")]
[assembly: GoPositionMap("runtime/trace/encoding.go", "encoding.cs", "AAwegpiSgoKCgoKCgpSUgpSssoKCgoKUgg==")]
[assembly: GoPositionMap("runtime/trace/flightrecorder.go", "flightrecorder.cs", "AClgkoKClKaClJSu4oKUgoKCuIKmgqiSgpSCpoKsoAABEAAMAoKUppSmAAcQgoKmgoKUpoKUgoKCuAAbNMY=")]
[assembly: GoPositionMap("runtime/trace/recorder.go", "recorder.cs", "ABIq8pSSlILKgoKUgoKUgpSCgpSmgpSCpIKU5oKmpgAHEIKCuIKUgpSCgqYACRSCqNySABcykg==", "24-31:1", "", "", "", "30:57,31:13")]
[assembly: GoPositionMap("runtime/trace/subscribe.go", "subscribe.cs", "AB060oKUgoKClIKUgoKCgpTW0oKUlIKCgpSClILW0oKUgoKClIKUgoKCgpTW0oKUlIKCgpSClILWopSUgqaCpqKClIKCgqa4gpS4ooIACBKCgoKUgoKClIKmgoKClIKUgpSCgriCgoKCgpSClILKgqg=", "157-198:1")]
[assembly: GoPositionMap("runtime/trace/trace.go", "trace.cs", "AHLsAbKqog==")]
// </GoSourcePositionMaps>

namespace go.runtime;

using static go.runtime.trace_package;
using io = go.io_package;
using time = go.time_package;

[GoPackage("trace")]
public static partial class trace_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct batch {}
    internal partial struct eventTime {}
    internal partial struct frequency {}
    internal partial struct rawGeneration {}
    internal partial struct recorder {}
    internal partial struct timestamp {}
    internal partial struct traceContextKey {}
    internal partial struct traceMultiplexer {}
    [GoValueClone("header")] public partial struct FlightRecorder {}
    public partial struct FlightRecorderConfig {}
    public partial struct Region {}
    public partial struct Task {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtraceꓸtracev2() => builtin.initPackage(typeof(@internal.trace.tracev2_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(trace_package.FlightRecorder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class traceᴛFlightRecorder;
[GoMethodHost(typeof(trace_package.Region)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class traceᴛRegion;
[GoMethodHost(typeof(trace_package.Task)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class traceᴛTask;
[GoMethodHost(typeof(trace_package.eventTime)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class traceᴛeventTime;
[GoMethodHost(typeof(trace_package.frequency)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class traceᴛfrequency;
[GoMethodHost(typeof(trace_package.recorder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class traceᴛrecorder;
[GoMethodHost(typeof(trace_package.traceMultiplexer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class traceᴛtraceMultiplexer;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class traceᴛFlightRecorder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Start(this ж<FlightRecorder> Ꮡfr) => trace_package.Start⁔(Ꮡfr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Stop(this ref FlightRecorder fr) => trace_package.Stop⁔(ref fr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Enabled(this ref FlightRecorder fr) => trace_package.Enabled⁔(ref fr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (int64 n, error err) WriteTo(this ж<FlightRecorder> Ꮡfr, io.Writer w) => trace_package.WriteTo⁔(Ꮡfr, w);
}
partial class traceᴛRegion {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void End(this ж<Region> Ꮡr) => trace_package.End⁔(Ꮡr);
}
partial class traceᴛTask {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void End(this ref Task t) => trace_package.End⁔(ref t);
}
partial class traceᴛeventTime {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static time.Duration Sub(this eventTime t, eventTime t0) => trace_package.Sub⁔(t, t0);
}
partial class traceᴛfrequency {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static eventTime mul(this frequency f, timestamp t) => trace_package.mul⁔(f, t);
}
partial class traceᴛrecorder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this ж<recorder> Ꮡw, slice<byte> b) => trace_package.Write⁔(Ꮡw, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void endGeneration(this ref recorder w) => trace_package.endGeneration⁔(ref w);
}
partial class traceᴛtraceMultiplexer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error subscribeFlightRecorder(this ж<traceMultiplexer> Ꮡt, ж<recorder> Ꮡr) => trace_package.subscribeFlightRecorder⁔(Ꮡt, Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error unsubscribeFlightRecorder(this ж<traceMultiplexer> Ꮡt) => trace_package.unsubscribeFlightRecorder⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error subscribeTraceStartWriter(this ж<traceMultiplexer> Ꮡt, io.Writer w) => trace_package.subscribeTraceStartWriter⁔(Ꮡt, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void unsubscribeTraceStartWriter(this ж<traceMultiplexer> Ꮡt) => trace_package.unsubscribeTraceStartWriter⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error addedSubscriber(this ж<traceMultiplexer> Ꮡt) => trace_package.addedSubscriber⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void removingSubscriber(this ж<traceMultiplexer> Ꮡt) => trace_package.removingSubscriber⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void removedSubscriber(this ref traceMultiplexer t) => trace_package.removedSubscriber⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error startLocked(this ж<traceMultiplexer> Ꮡt) => trace_package.startLocked⁔(Ꮡt);
}
// </MethodForwarders>
