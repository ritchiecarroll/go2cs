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
using Δio = go.io_package;
// </ImportedTypeAliases>

using go;
using static go.log_package;
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
[assembly: GoFieldAccess(typeof(Logger))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("log/log.go", "log.cs", "AEKOAdKCgoKCqOKCgoKipJaAlpSSgoKCgoKCpoIAAhDSgpSCgpSCgoKCgoKClIKCgoKCgoKCgpSmgoKCgoKCppSCgoKUgrikgoKCpgAIDoKUAAIS4oK84oKmuIKUgoKCgoKCgoKmgoKCgpS4goKCgoKmgoKC1oKCAAQQwr7CvsK8spaospaospaosoKGqLKChqiygoaqoqqiqJKClKiyqOKCgtiSqqKqoqiSqJKokq7ivsK+wryylqiylqiylqiygoaosoKGqLKChgACFPKC", "194-196:1;249-253:1;250-252:1.1;259-261:1;267-269:1;275-277:1;282-284:1;290-292:1;298-300:1;307-309:1;316-318:1;325-327:1;400-402:1;408-410:1;416-418:1;423-425:1;431-433:1;439-441:1;448-450:1;457-459:1;466-468:1;480-482:1")]
// </GoSourcePositionMaps>

namespace go;

using static go.log_package;
using Δio = go.io_package;
using ꓸꓸꓸany = Span<any>;

[GoPackage("log")]
public static partial class log_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    public partial struct Logger {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
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
[GoMethodHost(typeof(log_package.Logger)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class logᴛLogger;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class logᴛLogger {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetOutput(this ж<Logger> Ꮡl, Δio.Writer w) => log_package.SetOutput⁔(Ꮡl, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Output(this ж<Logger> Ꮡl, nint calldepth, @string s) => log_package.Output⁔(Ꮡl, calldepth, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error output(this ж<Logger> Ꮡl, uintptr pc, nint calldepth, Func<slice<byte>, slice<byte>> appendOutput) => log_package.output⁔(Ꮡl, pc, calldepth, appendOutput);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Print(this ж<Logger> Ꮡl, params ꓸꓸꓸany vʗp) => log_package.Print⁔(Ꮡl, vʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Printf(this ж<Logger> Ꮡl, @string format, params ꓸꓸꓸany vʗp) => log_package.Printf⁔(Ꮡl, format, vʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Println(this ж<Logger> Ꮡl, params ꓸꓸꓸany vʗp) => log_package.Println⁔(Ꮡl, vʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Fatal(this ж<Logger> Ꮡl, params ꓸꓸꓸany vʗp) => log_package.Fatal⁔(Ꮡl, vʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Fatalf(this ж<Logger> Ꮡl, @string format, params ꓸꓸꓸany vʗp) => log_package.Fatalf⁔(Ꮡl, format, vʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Fatalln(this ж<Logger> Ꮡl, params ꓸꓸꓸany vʗp) => log_package.Fatalln⁔(Ꮡl, vʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Panic(this ж<Logger> Ꮡl, params ꓸꓸꓸany vʗp) => log_package.Panic⁔(Ꮡl, vʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Panicf(this ж<Logger> Ꮡl, @string format, params ꓸꓸꓸany vʗp) => log_package.Panicf⁔(Ꮡl, format, vʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Panicln(this ж<Logger> Ꮡl, params ꓸꓸꓸany vʗp) => log_package.Panicln⁔(Ꮡl, vʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Flags(this ж<Logger> Ꮡl) => log_package.Flags⁔(Ꮡl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetFlags(this ж<Logger> Ꮡl, nint flag) => log_package.SetFlags⁔(Ꮡl, flag);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Prefix(this ж<Logger> Ꮡl) => log_package.Prefix⁔(Ꮡl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetPrefix(this ж<Logger> Ꮡl, @string prefixʗp) => log_package.SetPrefix⁔(Ꮡl, prefixʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Δio.Writer Writer(this ж<Logger> Ꮡl) => log_package.Writer⁔(Ꮡl);
}
// </MethodForwarders>
