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
using static go.main_package;
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

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(of))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("main.go", "main.cs", "AAkigoKCgoKCgoKCgoK6goKCgoKCurq6goK8gri4uAAXNoKCqOSCgoKCgoKChgAIApaCpLSkgqS0pKS0pJaCgoKCgoKmgoKC0oKCgpSCgoKUkoKSgoKClIKSlJKCkoKClIKClIKCgoKCkoKCgoKmgoKCgoKCgoKCgoKCgoKCpLSkgoKCmIKCgoKCgoKUgoKCgoKCgoKIgoKCgoKCgoKCgoKCgoKCgoKCgg==", "", "120=main.Pipe.Capacity;120=main.Pipe.IsClosed;120=main.Pipe.Length;159=main.Counts.IsNil;159=main.Counts.Length;172=main.Level.CompareTo;179=main.Gauge.Value;184=main.Name.Length")]
// </GoSourcePositionMaps>

namespace go;

using static go.main_package;

[GoTestMatchingConsoleOutput]
[GoPackage("main")]
public static partial class main_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial class at {}
    internal partial class of {}
    internal partial class strPtr {}
    internal partial struct point {}
    public partial class IsNilPointer {}
    public partial class IsNull {}
    public partial class NilInstance {}
    public partial class Value {}
    public partial class ValueSlot {}
    public partial struct Capacity {}
    public partial struct CompareTo {}
    public partial struct Count {}
    public partial struct Counts {}
    public partial struct Gauge {}
    public partial struct Label {}
    public partial struct Length {}
    public partial struct Level {}
    public partial struct Name {}
    public partial struct Pipe {}
    public partial struct Rank {}
    public partial struct Receive {}
    public partial struct Send {}
    public partial struct Tally {}
    public partial struct Wire {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(main_package.Counts)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛCounts;
[GoMethodHost(typeof(main_package.Gauge)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛGauge;
[GoMethodHost(typeof(main_package.Level)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛLevel;
[GoMethodHost(typeof(main_package.Name)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛName;
[GoMethodHost(typeof(main_package.Pipe)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛPipe;
[GoMethodHost(typeof(main_package.Tally)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛTally;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class mainᴛCounts {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Length(this Counts c) => main_package.Length⁔(c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string IsNil(this Counts c) => main_package.IsNil⁔(c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Count(this Counts c) => main_package.Count⁔(c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Make(this Counts c) => main_package.Make⁔(c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GetEnumerator(this Counts c) => main_package.GetEnumerator⁔(c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Add(this Counts c, @string k) => main_package.Add⁔(c, k);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Remove(this Counts c, @string k) => main_package.Remove⁔(c, k);
}
partial class mainᴛGauge {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static float64 Value(this Gauge g) => main_package.Value⁔(g);
}
partial class mainᴛLevel {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint CompareTo(this Level l, Level o) => main_package.CompareTo⁔(l, o);
}
partial class mainᴛName {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Length(this Name n) => main_package.Length⁔(n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint CompareTo(this Name n, Name o) => main_package.CompareTo⁔(n, o);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GetEnumerator(this Name n) => main_package.GetEnumerator⁔(n);
}
partial class mainᴛPipe {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Capacity(this Pipe p) => main_package.Capacity⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Length(this Pipe p) => main_package.Length⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string IsClosed(this Pipe p) => main_package.IsClosed⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string IsUnbuffered(this Pipe p) => main_package.IsUnbuffered⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Receive(this Pipe p) => main_package.Receive⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Receiving(this Pipe p) => main_package.Receiving⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Sending(this Pipe p, nint v) => main_package.Sending⁔(p, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Received(this Pipe p, nint v) => main_package.Received⁔(p, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Send(this Pipe p, nint v) => main_package.Send⁔(p, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Sent(this Pipe p, nint v) => main_package.Sent⁔(p, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Pipe Make(this Pipe p, nint n) => main_package.Make⁔(p, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint GetEnumerator(this Pipe p) => main_package.GetEnumerator⁔(p);
}
partial class mainᴛTally {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Counts(this Tally t) => main_package.Counts⁔(t);
}
// </MethodForwarders>
