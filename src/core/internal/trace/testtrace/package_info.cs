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
using trace = go.@internal.trace_package;
// </ImportedTypeAliases>

using go;
using static go.@internal.trace.testtrace_package;
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
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(Expectation))]
[assembly: GoFieldAccess(typeof(go.@internal.trace_package.ClockSnapshot))]
[assembly: GoFieldAccess(typeof(go.@internal.txtar_package.Archive))]
[assembly: GoFieldAccess(typeof(goState))]
[assembly: GoFieldAccess(typeof(procState))]
[assembly: GoFieldAccess(typeof(schedContext))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("internal/trace/testtrace/expectation.go", "expectation.cs", "ABYukgACENKClIKUgqSokoKCgoLWgoKUgoKUtJSUpoKCgpSCgpQ=")]
[assembly: GoPositionMap("internal/trace/testtrace/format.go", "format.cs", "ABAikoKClIKUgpSClIKClIKCgpSCgoKUgpSCpoKClA==")]
[assembly: GoPositionMap("internal/trace/testtrace/helpers.go", "helpers.cs", "ABookoKUuILulIK4uIKUgoKUgoKClIKCgpSClIKmpqKUgoKClJKClA==", "", "", "", "93:21")]
[assembly: GoPositionMap("internal/trace/testtrace/platforms.go", "platforms.cs", "AA0cooKUqqLalA==")]
[assembly: GoPositionMap("internal/trace/testtrace/validation.go", "validation.cs", "ADFgkgAJHMKs4qaCgpSmuJSUgoLugoKUgpSClIKUgoKUgpSCyrSClKaClKbWpoK4gpSUgoLGgoLGgoIAEhyCgqaCkoKUgpSClIKCgpSUgpSCpoKCgoKkgpSCppSCgoKkgpTqgpKClIKUgpSCgoKUlIKUgqaCgoKCpIKUgoKCgpSClAAIDIKUgpS0gsaClOiClJSUpuqCgoKUAAkQpKaCgqaCgoKmgoKmgoKClIKClLbCyIKUgpSCgpSClIKClIKCgoKUprSCuJSCAAgSoraC", "368-384:1")]
// </GoSourcePositionMaps>

namespace go.@internal.trace;

using static go.@internal.trace.testtrace_package;
using trace = go.@internal.trace_package;
using ꓸꓸꓸany = Span<any>;

[GoPackage("testtrace")]
public static partial class testtrace_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct errAccumulator {}
    internal partial struct goState {}
    internal partial struct procState {}
    internal partial struct schedContext {}
    public partial struct Expectation {}
    public partial struct Validator {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(go.@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtrace() => builtin.initPackage(typeof(go.@internal.trace_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtraceꓸraw() => builtin.initPackage(typeof(go.@internal.trace.raw_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtraceꓸversion() => builtin.initPackage(typeof(go.@internal.trace.version_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtxtar() => builtin.initPackage(typeof(go.@internal.txtar_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(go.strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(testtrace_package.Expectation)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class testtraceᴛExpectation;
[GoMethodHost(typeof(testtrace_package.Validator)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class testtraceᴛValidator;
[GoMethodHost(typeof(testtrace_package.errAccumulator)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class testtraceᴛerrAccumulator;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class testtraceᴛExpectation {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Check(this ref Expectation e, error err) => testtrace_package.Check⁔(ref e, err);
}
partial class testtraceᴛValidator {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SkipClockSnapshotChecks(this ref Validator v) => testtrace_package.SkipClockSnapshotChecks⁔(ref v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Event(this ж<Validator> Ꮡv, trace.Event ev) => testtrace_package.Event⁔(Ꮡv, ev);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool hasRange(this ref Validator v, trace.ResourceID r, @string name) => testtrace_package.hasRange⁔(ref v, r, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addRange(this ref Validator v, trace.ResourceID r, @string name) => testtrace_package.addRange⁔(ref v, r, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool hasAnyRange(this ref Validator v, trace.ResourceID r) => testtrace_package.hasAnyRange⁔(ref v, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void deleteRange(this ref Validator v, trace.ResourceID r, @string name) => testtrace_package.deleteRange⁔(ref v, r, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<schedContext> getOrCreateThread(this ж<Validator> Ꮡv, ж<errAccumulator> Ꮡe, trace.Event ev, trace.ThreadID m) => testtrace_package.getOrCreateThread⁔(Ꮡv, Ꮡe, ev, m);
}
partial class testtraceᴛerrAccumulator {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Errorf(this ref errAccumulator e, @string f, params ꓸꓸꓸany argsʗp) => testtrace_package.Errorf⁔(ref e, f, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Errors(this ref errAccumulator e) => testtrace_package.Errors⁔(ref e);
}
// </MethodForwarders>
