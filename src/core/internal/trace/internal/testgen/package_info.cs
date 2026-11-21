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
using raw = go.@internal.trace.raw_package;
using time = go.time_package;
using trace = go.@internal.trace_package;
using tracev2 = go.@internal.trace.tracev2_package;
// </ImportedTypeAliases>

using go;
using static go.@internal.trace.@internal.testgen_package;

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
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(Batch))]
[assembly: GoFieldAccess(typeof(Generation))]
[assembly: GoFieldAccess(typeof(Trace))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("internal/trace/internal/testgen/trace.go", "trace.cs", "ABoutIKClLampoK0ABg0kgAHFKKCqJKs0qyyruLugqiUkoKCuIK4griCgrgABxCigoKCpgAOHgAPKNK4goKuwoKUgpSCgq7CgpSClIKCgoKUgoKuwoKUABIg1IKUgoKCgoKCuIK4goKCgriCgqKCgoKUpoKCpqaCgoKmgoKmpoLuooKCAAse0oKClIKCgoKClJSUgoKUgpSmgoKCgpSClLS0tLS0tLS0tLSkrNKmgoKClIKCuKiyyoK6koKC", "", "", "77=Specs/1/2/1,EventNames/1/1/1,Specs/2/2/2;157=Bytes/1/1/3", "36:21")]
// </GoSourcePositionMaps>

namespace go.@internal.trace.@internal;

using static go.@internal.trace.@internal.testgen_package;
using raw = go.@internal.trace.raw_package;
using time = go.time_package;
using trace = go.@internal.trace_package;
using tracev2 = go.@internal.trace.tracev2_package;
using ꓸꓸꓸany = Span<any>;
using ꓸꓸꓸuint64 = Span<uint64>;

[GoPackage("testgen")]
public static partial class testgen_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoValueClone("stk")] internal partial struct stack {}
    internal partial struct sync {}
    public partial struct Batch {}
    public partial struct Generation {}
    public partial struct Seq {}
    public partial struct Time {}
    public partial struct Trace {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtrace() => builtin.initPackage(typeof(go.@internal.trace_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtraceꓸraw() => builtin.initPackage(typeof(go.@internal.trace.raw_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtraceꓸtracev2() => builtin.initPackage(typeof(go.@internal.trace.tracev2_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtraceꓸversion() => builtin.initPackage(typeof(go.@internal.trace.version_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtxtar() => builtin.initPackage(typeof(go.@internal.txtar_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(testgen_package.Batch)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class testgenᴛBatch;
[GoMethodHost(typeof(testgen_package.Generation)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class testgenᴛGeneration;
[GoMethodHost(typeof(testgen_package.Trace)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class testgenᴛTrace;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class testgenᴛBatch {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Event(this ref Batch b, @string name, params ꓸꓸꓸany argsʗp) => testgen_package.Event⁔(ref b, name, argsʗp);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 uintArgFor(this ref Batch b, any arg, @string argSpec) => testgen_package.uintArgFor⁔(ref b, arg, argSpec);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void RawEvent(this ref Batch b, tracev2.EventType typ, slice<byte> data, params ꓸꓸꓸuint64 argsʗp) => testgen_package.RawEvent⁔(ref b, typ, data, argsʗp);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeEventsTo(this ref Batch b, ж<raw.TextWriter> Ꮡtw) => testgen_package.writeEventsTo⁔(ref b, Ꮡtw);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setTimestamp(this ref Batch b, Time t) => testgen_package.setTimestamp⁔(ref b, t);
}
partial class testgenᴛGeneration {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Batch> Batch(this ж<Generation> Ꮡg, trace.ThreadID thread, Time time) => testgen_package.Batch⁔(Ꮡg, thread, time);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static uint64 String(this ref Generation g, @string s) => testgen_package.String⁔(ref g, s);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static uint64 Stack(this ref Generation g, slice<trace.StackFrame> stk) => testgen_package.Stack⁔(ref g, stk);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Sync(this ref Generation g, uint64 freq, Time timeΔ1, uint64 mono, time.Time wall) => testgen_package.Sync⁔(ref g, freq, timeΔ1, mono, wall);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeEventsTo(this ж<Generation> Ꮡg, ж<raw.TextWriter> Ꮡtw) => testgen_package.writeEventsTo⁔(Ꮡg, Ꮡtw);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Batch> newStructuralBatch(this ж<Generation> Ꮡg) => testgen_package.newStructuralBatch⁔(Ꮡg);
}
partial class testgenᴛTrace {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void ExpectFailure(this ref Trace t, @string pattern) => testgen_package.ExpectFailure⁔(ref t, pattern);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void ExpectSuccess(this ref Trace t) => testgen_package.ExpectSuccess⁔(ref t);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void RawEvent(this ref Trace t, tracev2.EventType typ, slice<byte> data, params ꓸꓸꓸuint64 argsʗp) => testgen_package.RawEvent⁔(ref t, typ, data, argsʗp);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void DisableTimestamps(this ref Trace t) => testgen_package.DisableTimestamps⁔(ref t);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Generation> Generation(this ж<Trace> Ꮡt, uint64 gen) => testgen_package.Generation⁔(Ꮡt, gen);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<byte> Generate(this ref Trace t) => testgen_package.Generate⁔(ref t);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static raw.Event createEvent(this ref Trace t, tracev2.EventType ev, slice<byte> data, params ꓸꓸꓸuint64 argsʗp) => testgen_package.createEvent⁔(ref t, ev, data, argsʗp);
}
// </MethodForwarders>
