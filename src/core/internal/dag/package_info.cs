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
using static go.@internal.dag_package;
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
[assembly: GoImplement<syntaxError, error>]
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
[assembly: GoFieldAccess(typeof(Graph))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("internal/dag/alg.go", "alg.cs", "AAcQkpSCgqaCgsyygpSCkoKUgpSClIKUgpSqtIKCgoKC", "29-38:1")]
[assembly: GoPositionMap("internal/dag/parse.go", "parse.cs", "ADlygqaCgpSCgoKmgqaCpoKmooKClIassoKUgoKokpKUgoKClIKCgpSClIKClIKU7oKCgtyCgoKCuJTugoKCgtyCpgALGIIACAjSgoKUpMS2lIKCgoKCgpSUgpSCgoKClIKUlgAMFpLosoKClIKUpJSUgoIACA6igoKUpIKCpoKUgqaCgpSCpoKkgqaCgpSCgoI=", "88-90:1;108-110:1;196-206:1", "", "", "", "208:21,209:13")]
// </GoSourcePositionMaps>

namespace go.@internal;

using static go.@internal.dag_package;

[GoPackage("dag")]
public static partial class dag_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct rule {}
    internal partial struct rulesParser {}
    internal partial struct syntaxError {}
    public partial struct Graph {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(dag_package.Graph)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dagᴛGraph;
[GoMethodHost(typeof(dag_package.rulesParser)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dagᴛrulesParser;
[GoMethodHost(typeof(dag_package.syntaxError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dagᴛsyntaxError;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class dagᴛGraph {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Transpose(this ref Graph g) => dag_package.Transpose⁔(ref g);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<@string> Topo(this ж<Graph> Ꮡg) => dag_package.Topo⁔(Ꮡg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void TransitiveReduction(this ref Graph g) => dag_package.TransitiveReduction⁔(ref g);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool addNode(this ref Graph g, @string label) => dag_package.addNode⁔(ref g, label);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddEdge(this ref Graph g, @string from, @string to) => dag_package.AddEdge⁔(ref g, from, to);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void DelEdge(this ref Graph g, @string from, @string to) => dag_package.DelEdge⁔(ref g, from, to);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool HasEdge(this ref Graph g, @string from, @string to) => dag_package.HasEdge⁔(ref g, from, to);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<@string> Edges(this ж<Graph> Ꮡg, @string from) => dag_package.Edges⁔(Ꮡg, from);
}
partial class dagᴛrulesParser {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void syntaxError(this ref rulesParser p, @string msg) => dag_package.syntaxError⁔(ref p, msg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<@string> list, @string token) nextList(this ref rulesParser p) => dag_package.nextList⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string nextToken(this ref rulesParser p) => dag_package.nextToken⁔(ref p);
}
partial class dagᴛsyntaxError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this syntaxError e) => dag_package.Error⁔(e);
}
// </MethodForwarders>
