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
using buffer = go.log.slog.@internal.buffer_package;
using context = go.context_package;
using slog = go.log.slog_package;
using time = go.time_package;
// </ImportedTypeAliases>

using go;
using static go.log.slog.@internal.benchmarks_package;
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
[assembly: GoImplement<fastTextHandler, go.log.slog_package.Handler>(Pointer = true)]
[assembly: GoImplement<go.log.slog.@internal.buffer_package.Buffer, io_package.Writer>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("log/slog/internal/benchmarks/benchmarks.go", "benchmarks.cs", "AClSuIKW", "", "", "49=String/1/1/1,Int/1/1/2,Duration/1/1/3,Time/1/1/4,Any/1/1/5")]
[assembly: GoPositionMap("log/slog/internal/benchmarks/handlers.go", "handlers.cs", "AB08gqYACQTSgqSCgoKUgpKCgoKSgoKCgpSCgtailLTExMTE1LSClMTWyKK2gqaCAA8ggqaUopKCpoKmguyClIKmgg==", "50-56:1")]
// </GoSourcePositionMaps>

namespace go.log.slog.@internal;

using static go.log.slog.@internal.benchmarks_package;
using buffer = go.log.slog.@internal.buffer_package;
using context = go.context_package;
using slog = go.log.slog_package;
using time = go.time_package;

[GoPackage("benchmarks")]
public static partial class benchmarks_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct @event {}
    [GoValueClone("ringBuffer")] internal partial struct asyncHandler {}
    internal partial struct disabledHandler {}
    internal partial struct fastTextHandler {}
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
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlogꓸslog() => builtin.initPackage(typeof(go.log.slog_package));
    [GoInit] internal static void initᴛᴛimportꓸlogꓸslogꓸinternalꓸbuffer() => builtin.initPackage(typeof(go.log.slog.@internal.buffer_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(benchmarks_package.asyncHandler)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class benchmarksᴛasyncHandler;
[GoMethodHost(typeof(benchmarks_package.disabledHandler)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class benchmarksᴛdisabledHandler;
[GoMethodHost(typeof(benchmarks_package.fastTextHandler)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class benchmarksᴛfastTextHandler;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class benchmarksᴛasyncHandler {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Enabled(this ref asyncHandler _Δp0, context.Context _Δp1, slog.Level _Δp2) => benchmarks_package.Enabled⁔(ref _Δp0, _Δp1, _Δp2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Handle(this ref asyncHandler h, context.Context _, slog.Record r) => benchmarks_package.Handle⁔(ref h, _, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slog.Handler WithAttrs(this ref asyncHandler _Δp0, slice<slog.Attr> _Δp1) => benchmarks_package.WithAttrs⁔(ref _Δp0, _Δp1);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slog.Handler WithGroup(this ref asyncHandler _Δp0, @string _Δp1) => benchmarks_package.WithGroup⁔(ref _Δp0, _Δp1);
}
partial class benchmarksᴛdisabledHandler {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Enabled(this disabledHandler _Δp0, context.Context _Δp1, slog.Level _Δp2) => benchmarks_package.Enabled⁔(_Δp0, _Δp1, _Δp2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Handle(this disabledHandler _Δp0, context.Context _Δp1, slog.Record _Δp2) => benchmarks_package.Handle⁔(_Δp0, _Δp1, _Δp2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slog.Handler WithAttrs(this disabledHandler _Δp0, slice<slog.Attr> _Δp1) => benchmarks_package.WithAttrs⁔(_Δp0, _Δp1);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slog.Handler WithGroup(this disabledHandler _Δp0, @string _Δp1) => benchmarks_package.WithGroup⁔(_Δp0, _Δp1);
}
partial class benchmarksᴛfastTextHandler {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Enabled(this ref fastTextHandler h, context.Context _Δp1, slog.Level _Δp2) => benchmarks_package.Enabled⁔(ref h, _Δp1, _Δp2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Handle(this ж<fastTextHandler> Ꮡh, context.Context _Δp1, slog.Record r) => benchmarks_package.Handle⁔(Ꮡh, _Δp1, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void appendValue(this ref fastTextHandler h, ж<buffer.Buffer> Ꮡbuf, slog.Value v) => benchmarks_package.appendValue⁔(ref h, Ꮡbuf, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void appendTime(this ref fastTextHandler h, ж<buffer.Buffer> Ꮡbuf, time.Time t) => benchmarks_package.appendTime⁔(ref h, Ꮡbuf, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slog.Handler WithAttrs(this ref fastTextHandler h, slice<slog.Attr> _) => benchmarks_package.WithAttrs⁔(ref h, _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slog.Handler WithGroup(this ref fastTextHandler _Δp0, @string _Δp1) => benchmarks_package.WithGroup⁔(ref _Δp0, _Δp1);
}
// </MethodForwarders>
