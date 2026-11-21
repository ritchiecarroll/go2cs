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
using abi = go.@internal.abi_package;
// </ImportedTypeAliases>

using go;
using static go.@internal.reflectlite_package;
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
[assembly: GoDynamicTypeLift("7374727563747b6220626f6f6c3b207820616e797d", "dummyᴛ1")]
[assembly: GoTypeAlias("Kind", "go.@internal.abi_package.Kind")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<rtype, go.@internal.reflectlite_package.Type>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<flag, abi.Kind>(Inverted = false, ValueType = "uint8")]
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.EmptyInterface))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.FuncType))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.StructType))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("internal/reflectlite/type.go", "type.cs", "AIgBrAKCpoKmgqaC6qKCgoKCggAICoKClIKmgoKUgoIACAaCgpSCgoKClKaCggACGgALDuSCpoKmggAEFpSCgoKUpoKCgpQABByCgpSCgoKClLSklKaCpoKCgpTOgoKClKaCgoKUpoKCgpSmgoKClKaCgoKUpoKCgpSmgoKClAACFPKqogAEGoKClIKCpoIABL4BAAkEgsqCuKaygqaCpgAE2AHSgpQ=")]
[assembly: GoPositionMap("internal/reflectlite/value.go", "value.cs", "AEykAYKmgoKUptyqooKUgpSokoKClJSCpoKCgoKU2MbsggAMNIKClAAEIKKClIK+soKmgpSCAAMS0gAIvAHSqqKsxgAEMpKClAAELJKCgqYAAhQACRwACQq4goKmgpS4lIKClJS4AAYU8qaayrKC")]
// </GoSourcePositionMaps>

namespace go.@internal;

[GoPackage("reflectlite")]
public static partial class reflectlite_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct dummyᴛ1 {}
    internal partial struct flag {}
    internal partial struct name {}
    internal partial struct rtype {}
    public partial interface Type {}
    public partial struct Value {}
    public partial struct ValueError {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸabi() => builtin.initPackage(typeof(go.@internal.abi_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(reflectlite_package.Value)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class reflectliteᴛValue;
[GoMethodHost(typeof(reflectlite_package.ValueError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class reflectliteᴛValueError;
[GoMethodHost(typeof(reflectlite_package.flag)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class reflectliteᴛflag;
[GoMethodHost(typeof(reflectlite_package.name)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class reflectliteᴛname;
[GoMethodHost(typeof(reflectlite_package.rtype)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class reflectliteᴛrtype;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class reflectliteᴛValue {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<abi.Type> typ(this reflectlite_package.Value v) => reflectlite_package.typ⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @unsafe.Pointer pointer(this reflectlite_package.Value v) => reflectlite_package.pointer⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool CanSet(this reflectlite_package.Value v) => reflectlite_package.CanSet⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsValid(this reflectlite_package.Value v) => reflectlite_package.IsValid⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Kind Kind(this reflectlite_package.Value v) => reflectlite_package.Kind⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint numMethod(this reflectlite_package.Value v) => reflectlite_package.numMethod⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static reflectlite_package.Type Type(this reflectlite_package.Value v) => reflectlite_package.Type⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static reflectlite_package.Value assignTo(this reflectlite_package.Value v, @string context, ж<abi.Type> Ꮡdst, @unsafe.Pointer target) => reflectlite_package.assignTo⁔(v, context, Ꮡdst, target);
}
partial class reflectliteᴛValueError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref reflectlite_package.ValueError e) => reflectlite_package.Error⁔(ref e);
}
partial class reflectliteᴛflag {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Kind kind(this reflectlite_package.flag f) => reflectlite_package.kind⁔(f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static reflectlite_package.flag ro(this reflectlite_package.flag f) => reflectlite_package.ro⁔(f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void mustBeExported(this reflectlite_package.flag f) => reflectlite_package.mustBeExported⁔(f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void mustBeAssignable(this reflectlite_package.flag f) => reflectlite_package.mustBeAssignable⁔(f);
}
partial class reflectliteᴛname {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<byte> data(this reflectlite_package.name n, nint off, @string whySafe) => reflectlite_package.data⁔(n, off, whySafe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isExported(this reflectlite_package.name n) => reflectlite_package.isExported⁔(n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool hasTag(this reflectlite_package.name n) => reflectlite_package.hasTag⁔(n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool embedded(this reflectlite_package.name n) => reflectlite_package.embedded⁔(n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, nint) readVarint(this reflectlite_package.name n, nint off) => reflectlite_package.readVarint⁔(n, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string name(this reflectlite_package.name n) => reflectlite_package.name⁔(n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string tag(this reflectlite_package.name n) => reflectlite_package.tag⁔(n);
}
partial class reflectliteᴛrtype {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static abi.Name nameOff(this reflectlite_package.rtype t, nameOff off) => reflectlite_package.nameOff⁔(t, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<abi.Type> typeOff(this reflectlite_package.rtype t, typeOff off) => reflectlite_package.typeOff⁔(t, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<uncommonType> uncommon(this reflectlite_package.rtype t) => reflectlite_package.uncommon⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<abi.Type> common(this reflectlite_package.rtype t) => reflectlite_package.common⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<abi.Method> exportedMethods(this reflectlite_package.rtype t) => reflectlite_package.exportedMethods⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint NumMethod(this reflectlite_package.rtype t) => reflectlite_package.NumMethod⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Name(this reflectlite_package.rtype t) => reflectlite_package.Name⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static reflectlite_package.Type In(this reflectlite_package.rtype t, nint i) => reflectlite_package.In⁔(t, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static reflectlite_package.Type Key(this reflectlite_package.rtype t) => reflectlite_package.Key⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Len(this reflectlite_package.rtype t) => reflectlite_package.Len⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint NumField(this reflectlite_package.rtype t) => reflectlite_package.NumField⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint NumIn(this reflectlite_package.rtype t) => reflectlite_package.NumIn⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint NumOut(this reflectlite_package.rtype t) => reflectlite_package.NumOut⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static reflectlite_package.Type Out(this reflectlite_package.rtype t, nint i) => reflectlite_package.Out⁔(t, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool AssignableTo(this reflectlite_package.rtype t, reflectlite_package.Type u) => reflectlite_package.AssignableTo⁔(t, u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Comparable(this reflectlite_package.rtype t) => reflectlite_package.Comparable⁔(t);
}
// </MethodForwarders>
