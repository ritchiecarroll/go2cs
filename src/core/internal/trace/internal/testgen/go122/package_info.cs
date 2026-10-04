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
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
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
using static go.@internal.trace.@internal.testgen.go122.testkit_package;

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
[assembly: GoTypeAlias("Batch", "ΔBatch")]
[assembly: GoTypeAlias("Generation", "ΔGeneration")]
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

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("internal/trace/internal/testgen/go122/trace.go", "trace.cs", "ABsutIKClKaWloCCxAAXMpKCAAYSooKokqzSrLKu4tyCqJSSgoKogqiCqIKCqAAHEKKCgoCCtgANHgANJtKClMqCrsKClICCpIKCrsKClIKUgoKCgIKkgoKoxIKogoKWgoKigoKClJaCgqaWgoKCloKCpqaCAAse0oKClIKCgoKUlJSCgIKkgpSmgoKCgpSClKSkpKSkpKSkpKSkrNKWgoKClIKCqKiyyoI=")]
// </GoSourcePositionMaps>

namespace go.@internal.trace.@internal.testgen.go122;

[GoPackage("testkit", ImportPath = "internal/trace/internal/testgen/go122")]
public static partial class testkit_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoValueClone("stk")] internal partial struct stack {}
    public partial struct Seq {}
    public partial struct Time {}
    public partial struct Trace {}
    public partial struct ΔBatch {}
    public partial struct ΔGeneration {}
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
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtraceꓸevent() => builtin.initPackage(typeof(go.@internal.trace.event_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtraceꓸeventꓸgo122() => builtin.initPackage(typeof(go.@internal.trace.@event.go122_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtraceꓸraw() => builtin.initPackage(typeof(go.@internal.trace.raw_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtraceꓸversion() => builtin.initPackage(typeof(go.@internal.trace.version_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtxtar() => builtin.initPackage(typeof(go.@internal.txtar_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    // </ImportInitializers>
}
