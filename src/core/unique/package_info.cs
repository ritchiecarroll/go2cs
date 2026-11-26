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
using weak = go.weak_package;
// </ImportedTypeAliases>

using go;
using static go.unique_package;
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
[assembly: GoArrayDimsCompanion("Make", 0)]
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
[assembly: GoFieldAccess(typeof(canonMap<>))]
[assembly: GoFieldAccess(typeof(entry<>))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.MapType))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.StructField))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.StructType))]
[assembly: GoFieldAccess(typeof(indirect<>))]
[assembly: GoFieldAccess(typeof(node<>))]
[assembly: GoFieldAccess(typeof(uniqueMap<>))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("unique/canonmap.go", "canonmap.cs", "ABo0goKUgoKCgqaypIKCgpSCgpSCgpSUptK0goKCgpSCgoKClIKClIKUuIKUgpSUgriCgpSm7qSCgoKUpsiClNyUkpTc5IKmgqaCgoKClIKCgoKCgpSCgpSuwoKCgoKUgoKCgpSCgpSUgpSUgpSUgriCgsqCgpQABxCCgpSmgoKCgoKUgqYACRSigpSmooKUABMsgqaCgoKmAAoWgoKCggAGFNKCgoKUlKiygoKUlKzkgoKUlIK4goKCgoKUlJSu", "136-138:1")]
[assembly: GoPositionMap("unique/handle.go", "handle.cs", "ABk0oqz0goKUgoKClKaClJQ=")]
// </GoSourcePositionMaps>

namespace go;

using static go.unique_package;
using weak = go.weak_package;

[GoPackage("unique")]
public static partial class unique_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct canonMap<T> {}
    internal partial struct entry<T> {}
    [GoValueClone("children")] internal partial struct indirect<T> {}
    internal partial struct node<T> {}
    internal partial struct uniqueMap<T> {}
    public partial struct Handle<T> {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸabi() => builtin.initPackage(typeof(@internal.abi_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸstringslite() => builtin.initPackage(typeof(@internal.stringslite_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsync() => builtin.initPackage(typeof(@internal.sync_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸweak() => builtin.initPackage(typeof(weak_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(unique_package.Handle<>)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class uniqueᴛHandle;
[GoMethodHost(typeof(unique_package.canonMap<>)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class uniqueᴛcanonMap;
[GoMethodHost(typeof(unique_package.entry<>)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class uniqueᴛentry;
[GoMethodHost(typeof(unique_package.indirect<>)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class uniqueᴛindirect;
[GoMethodHost(typeof(unique_package.node<>)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class uniqueᴛnode;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class uniqueᴛHandle {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static T Value<T>(this Handle<T> h) => unique_package.Value⁔<T>(h);
}
partial class uniqueᴛcanonMap {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<T> Load<T>(this ж<canonMap<T>> Ꮡm, T keyʗp) => unique_package.Load⁔<T>(Ꮡm, keyʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<T> LoadOrStore<T>(this ж<canonMap<T>> Ꮡm, T keyʗp) => unique_package.LoadOrStore⁔<T>(Ꮡm, keyʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<node<T>> expand<T>(this ref canonMap<T> m, ж<entry<T>> ᏑoldEntry, ж<entry<T>> ᏑnewEntry, uintptr newHash, nuint hashShift, ж<indirect<T>> Ꮡparent) => unique_package.expand⁔<T>(ref m, ᏑoldEntry, ᏑnewEntry, newHash, hashShift, Ꮡparent);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void cleanup<T>(this ж<canonMap<T>> Ꮡm, uintptr hash, weak.Pointer<T> wp) => unique_package.cleanup⁔<T>(Ꮡm, hash, wp);
}
partial class uniqueᴛentry {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<T>, weak.Pointer<T>) lookup<T>(this ж<entry<T>> Ꮡe, T key) => unique_package.lookup⁔<T>(Ꮡe, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool hasWeakPointer<T>(this ж<entry<T>> Ꮡe, weak.Pointer<T> wp) => unique_package.hasWeakPointer⁔<T>(Ꮡe, wp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<entry<T>> prune<T>(this ж<entry<T>> Ꮡe) => unique_package.prune⁔<T>(Ꮡe);
}
partial class uniqueᴛindirect {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool empty<T>(this ref indirect<T> i) => unique_package.empty⁔<T>(ref i);
}
partial class uniqueᴛnode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<entry<T>> entry<T>(this ж<node<T>> Ꮡn) => unique_package.entry⁔<T>(Ꮡn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<indirect<T>> indirect<T>(this ж<node<T>> Ꮡn) => unique_package.indirect⁔<T>(Ꮡn);
}
// </MethodForwarders>
