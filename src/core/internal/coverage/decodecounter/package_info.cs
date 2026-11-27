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
// </ImportedTypeAliases>

using go;
using static go.@internal.coverage.decodecounter_package;
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
[assembly: GoFieldAccess(typeof(CounterDataReader))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("internal/coverage/decodecounter/decodecounterfile.go", "decodecounterfile.cs", "AChOgtyAgqSClIKUgriAgraCgIK2gIKkpqKCpqKCgIKkgIKkgpSClKrUgIKkgoLKgIKkgIK2gIKUgoKCgsimgoKCgpSClIKCggAKBqKCgoKUgpSCkoKClJSCgoKCgpSCgpSClJSCgoKUgoKSpoKUgpQAAhLirsKuwgAKGJKu4oKUgpSCgIK2gIKkqqIABBQACAKClIKCgoKCgoKCgpSCgoKUlJSCgoKCgpSClKaCgoKUgpS4AAIQ0oKCgoKCgoKUgriUgriCgpSCgpSClIKCgoKUlA==", "157-163:1;281-297:1;300-309:2;311-320:3")]
// </GoSourcePositionMaps>

namespace go.@internal.coverage;

using static go.@internal.coverage.decodecounter_package;

[GoPackage("decodecounter")]
public static partial class decodecounter_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoValueClone("hdr", "ftr")] public partial struct CounterDataReader {}
    public partial struct FuncPayload {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸcoverage() => builtin.initPackage(typeof(go.@internal.coverage_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸcoverageꓸslicereader() => builtin.initPackage(typeof(go.@internal.coverage.slicereader_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸcoverageꓸstringtab() => builtin.initPackage(typeof(go.@internal.coverage.stringtab_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(go.strconv_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(decodecounter_package.CounterDataReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class decodecounterᴛCounterDataReader;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class decodecounterᴛCounterDataReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readFooter(this ж<CounterDataReader> Ꮡcdr) => decodecounter_package.readFooter⁔(Ꮡcdr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readSegmentPreamble(this ж<CounterDataReader> Ꮡcdr) => decodecounter_package.readSegmentPreamble⁔(Ꮡcdr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readStringTable(this ref CounterDataReader cdr) => decodecounter_package.readStringTable⁔(ref cdr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readArgs(this ж<CounterDataReader> Ꮡcdr) => decodecounter_package.readArgs⁔(Ꮡcdr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<@string> OsArgs(this ref CounterDataReader cdr) => decodecounter_package.OsArgs⁔(ref cdr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Goos(this ref CounterDataReader cdr) => decodecounter_package.Goos⁔(ref cdr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Goarch(this ref CounterDataReader cdr) => decodecounter_package.Goarch⁔(ref cdr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint32 NumSegments(this ref CounterDataReader cdr) => decodecounter_package.NumSegments⁔(ref cdr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (bool, error) BeginNextSegment(this ж<CounterDataReader> Ꮡcdr) => decodecounter_package.BeginNextSegment⁔(Ꮡcdr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint32 NumFunctionsInSegment(this ref CounterDataReader cdr) => decodecounter_package.NumFunctionsInSegment⁔(ref cdr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (bool, error) NextFunc(this ж<CounterDataReader> Ꮡcdr, ж<FuncPayload> Ꮡp) => decodecounter_package.NextFunc⁔(Ꮡcdr, Ꮡp);
}
// </MethodForwarders>
