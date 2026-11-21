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
using static go.container.list_package;
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
[assembly: GoFieldAccess(typeof(Element))]
[assembly: GoFieldAccess(typeof(List))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("container/list/list.go", "list.cs", "AB0+koKUqJKClAAJFrKCgoKogJiQlpKClKiSgpSosoK60oKCgoKCgqiSqLKCgoKCgqjCgpSClIKCgqzippSokoKosoKs0oKmrNKCpqzigqas4oKmrPKClKzygpSq0oKCvMKCgg==")]
// </GoSourcePositionMaps>

namespace go.container;

using static go.container.list_package;

[GoPackage("list")]
public static partial class list_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    public partial struct Element {}
    public partial struct List {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(list_package.Element)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class listᴛElement;
[GoMethodHost(typeof(list_package.List)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class listᴛList;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class listᴛElement {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Element> Next(this ref Element e) => list_package.Next⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Element> Prev(this ref Element e) => list_package.Prev⁔(ref e);
}
partial class listᴛList {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<List> Init(this ж<List> Ꮡl) => list_package.Init⁔(Ꮡl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Len(this ref List l) => list_package.Len⁔(ref l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Element> Front(this ref List l) => list_package.Front⁔(ref l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Element> Back(this ref List l) => list_package.Back⁔(ref l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void lazyInit(this ж<List> Ꮡl) => list_package.lazyInit⁔(Ꮡl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Element> insert(this ж<List> Ꮡl, ж<Element> Ꮡe, ж<Element> Ꮡat) => list_package.insert⁔(Ꮡl, Ꮡe, Ꮡat);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Element> insertValue(this ж<List> Ꮡl, any v, ж<Element> Ꮡat) => list_package.insertValue⁔(Ꮡl, v, Ꮡat);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void remove(this ref List l, ж<Element> Ꮡe) => list_package.remove⁔(ref l, Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void move(this ref List l, ж<Element> Ꮡe, ж<Element> Ꮡat) => list_package.move⁔(ref l, Ꮡe, Ꮡat);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static any Remove(this ж<List> Ꮡl, ж<Element> Ꮡe) => list_package.Remove⁔(Ꮡl, Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Element> PushFront(this ж<List> Ꮡl, any v) => list_package.PushFront⁔(Ꮡl, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Element> PushBack(this ж<List> Ꮡl, any v) => list_package.PushBack⁔(Ꮡl, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Element> InsertBefore(this ж<List> Ꮡl, any v, ж<Element> Ꮡmark) => list_package.InsertBefore⁔(Ꮡl, v, Ꮡmark);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Element> InsertAfter(this ж<List> Ꮡl, any v, ж<Element> Ꮡmark) => list_package.InsertAfter⁔(Ꮡl, v, Ꮡmark);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void MoveToFront(this ж<List> Ꮡl, ж<Element> Ꮡe) => list_package.MoveToFront⁔(Ꮡl, Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void MoveToBack(this ж<List> Ꮡl, ж<Element> Ꮡe) => list_package.MoveToBack⁔(Ꮡl, Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void MoveBefore(this ж<List> Ꮡl, ж<Element> Ꮡe, ж<Element> Ꮡmark) => list_package.MoveBefore⁔(Ꮡl, Ꮡe, Ꮡmark);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void MoveAfter(this ж<List> Ꮡl, ж<Element> Ꮡe, ж<Element> Ꮡmark) => list_package.MoveAfter⁔(Ꮡl, Ꮡe, Ꮡmark);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void PushBackList(this ж<List> Ꮡl, ж<List> Ꮡother) => list_package.PushBackList⁔(Ꮡl, Ꮡother);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void PushFrontList(this ж<List> Ꮡl, ж<List> Ꮡother) => list_package.PushFrontList⁔(Ꮡl, Ꮡother);
}
// </MethodForwarders>
