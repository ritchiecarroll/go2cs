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
using static go.@internal.abi_package;
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
[assembly: GoFieldAccess(typeof(Uncommon_u))]
[assembly: GoFieldAccess(typeof(Uncommon_uᴛ1))]
[assembly: GoFieldAccess(typeof(Uncommon_uᴛ2))]
[assembly: GoFieldAccess(typeof(Uncommon_uᴛ3))]
[assembly: GoFieldAccess(typeof(Uncommon_uᴛ4))]
[assembly: GoFieldAccess(typeof(Uncommon_uᴛ5))]
[assembly: GoFieldAccess(typeof(Uncommon_uᴛ6))]
[assembly: GoFieldAccess(typeof(Uncommon_uᴛ7))]
[assembly: GoFieldAccess(typeof(structTypeUncommon))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("internal/abi/abi.go", "abi.cs", "ACxegoKClIKCgpSCgoKUAAIYAAkCgpSCgpQABxCSAAIS9II=")]
[assembly: GoPositionMap("internal/abi/bounds.go", "bounds.cs", "ABluABgCgoKCgpSClJSClJSCgoKUlIKUlJTygoKCgoKCgoKCgpSClIKClIKUgpQ=")]
[assembly: GoPositionMap("internal/abi/compiletype.go", "compiletype.cs", "AAce4JaAmJCWgJaA")]
[assembly: GoPositionMap("internal/abi/escape.go", "escape.cs", "AAkmAAoCgt6SgpQAAiAADhwADgKC")]
[assembly: GoPositionMap("internal/abi/funcpc.go", "funcpc.cs", "AAgsAA4S")]
[assembly: GoPositionMap("internal/abi/map.go", "map.cs", "AC9qkpSSlJKUkg==")]
[assembly: GoPositionMap("internal/abi/switch.go", "switch.cs", "ACBAppSk")]
[assembly: GoPositionMap("internal/abi/type.go", "type.cs", "AIgBngKSgpSmACFclIKogJaSpoKClAAaLqKClOaigpQAAhTyABIyggA/RLKClJSkrKysrKysrAARlAGAlAAHEIKCgpSmooKClKiAABk+gqaCpoKmggAFMoIADRyCACVSouqiqJKokqiS6qKCgoKCgsySgpSCrLKCgoKCgpT6koKUgqiSgpSCgqaCgpSClIKCgpSCgoKUgoKUgqaCgoKCgoKCpg==")]
// </GoSourcePositionMaps>

namespace go.@internal;

using static go.@internal.abi_package;

[GoPackage("abi")]
public static partial class abi_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("u")] internal partial struct Uncommon_u {}
    [GoLocalName("u")] internal partial struct Uncommon_uᴛ1 {}
    [GoLocalName("u")] internal partial struct Uncommon_uᴛ2 {}
    [GoLocalName("u")] internal partial struct Uncommon_uᴛ3 {}
    [GoLocalName("u")] internal partial struct Uncommon_uᴛ4 {}
    [GoLocalName("u")] internal partial struct Uncommon_uᴛ5 {}
    [GoLocalName("u")] internal partial struct Uncommon_uᴛ6 {}
    [GoLocalName("u")] internal partial struct Uncommon_uᴛ7 {}
    internal partial struct structTypeUncommon {}
    public partial struct ArrayType {}
    public partial struct BoundsErrorCode {}
    public partial struct ChanDir {}
    public partial struct ChanType {}
    public partial struct CommonInterface {}
    public partial struct EmptyInterface {}
    public partial struct FuncFlag {}
    public partial struct FuncID {}
    public partial struct FuncType {}
    [GoValueClone("Fun")] public partial struct ITab {}
    public partial struct Imethod {}
    public partial struct IntArgRegBitmap {}
    [GoValueClone("Cases")] public partial struct InterfaceSwitch {}
    [GoValueClone("Entries")] public partial struct InterfaceSwitchCache {}
    public partial struct InterfaceSwitchCacheEntry {}
    public partial struct InterfaceType {}
    public partial struct Kind {}
    public partial struct MapType {}
    public partial struct Method {}
    public partial struct Name {}
    public partial struct NameOff {}
    public partial struct NonEmptyInterface {}
    public partial struct PCLnTabMagic {}
    public partial struct PtrType {}
    public partial struct RF_State {}
    [GoValueClone("Ints", "Floats", "Ptrs", "ReturnIsPtr")] public partial struct RegArgs {}
    public partial struct SliceType {}
    public partial struct StructField {}
    public partial struct StructType {}
    public partial struct TFlag {}
    public partial struct TextOff {}
    public partial struct Type {}
    public partial struct TypeAssert {}
    [GoValueClone("Entries")] public partial struct TypeAssertCache {}
    public partial struct TypeAssertCacheEntry {}
    public partial struct TypeOff {}
    public partial struct UncommonType {}
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
[GoMethodHost(typeof(abi_package.FuncType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class abiᴛFuncType;
[GoMethodHost(typeof(abi_package.IntArgRegBitmap)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class abiᴛIntArgRegBitmap;
[GoMethodHost(typeof(abi_package.InterfaceType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class abiᴛInterfaceType;
[GoMethodHost(typeof(abi_package.Kind)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class abiᴛKind;
[GoMethodHost(typeof(abi_package.MapType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class abiᴛMapType;
[GoMethodHost(typeof(abi_package.Name)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class abiᴛName;
[GoMethodHost(typeof(abi_package.RegArgs)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class abiᴛRegArgs;
[GoMethodHost(typeof(abi_package.StructField)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class abiᴛStructField;
[GoMethodHost(typeof(abi_package.Type)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class abiᴛType;
[GoMethodHost(typeof(abi_package.UncommonType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class abiᴛUncommonType;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class abiᴛFuncType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Type> In(this ж<FuncType> Ꮡt, nint i) => abi_package.In⁔(Ꮡt, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint NumIn(this ref FuncType t) => abi_package.NumIn⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint NumOut(this ref FuncType t) => abi_package.NumOut⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Type> Out(this ж<FuncType> Ꮡt, nint i) => abi_package.Out⁔(Ꮡt, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsVariadic(this ref FuncType t) => abi_package.IsVariadic⁔(ref t);
}
partial class abiᴛIntArgRegBitmap {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Set(this ref IntArgRegBitmap b, nint i) => abi_package.Set⁔(ref b, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Get(this ж<IntArgRegBitmap> Ꮡb, nint i) => abi_package.Get⁔(Ꮡb, i);
}
partial class abiᴛInterfaceType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint NumMethod(this ref InterfaceType t) => abi_package.NumMethod⁔(ref t);
}
partial class abiᴛKind {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Kind k) => abi_package.String⁔(k);
}
partial class abiᴛMapType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool NeedKeyUpdate(this ref MapType mt) => abi_package.NeedKeyUpdate⁔(ref mt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool HashMightPanic(this ref MapType mt) => abi_package.HashMightPanic⁔(ref mt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IndirectKey(this ref MapType mt) => abi_package.IndirectKey⁔(ref mt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IndirectElem(this ref MapType mt) => abi_package.IndirectElem⁔(ref mt);
}
partial class abiᴛName {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<byte> DataChecked(this Name n, nint off, @string whySafe) => abi_package.DataChecked⁔(n, off, whySafe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<byte> Data(this Name n, nint off) => abi_package.Data⁔(n, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsExported(this Name n) => abi_package.IsExported⁔(n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool HasTag(this Name n) => abi_package.HasTag⁔(n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsEmbedded(this Name n) => abi_package.IsEmbedded⁔(n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, nint) ReadVarint(this Name n, nint off) => abi_package.ReadVarint⁔(n, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsBlank(this Name n) => abi_package.IsBlank⁔(n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Name(this Name n) => abi_package.Name⁔(n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Tag(this Name n) => abi_package.Tag⁔(n);
}
partial class abiᴛRegArgs {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Dump(this ref RegArgs r) => abi_package.Dump⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static unsafe_package.Pointer IntRegArgAddr(this ref RegArgs r, nint reg, uintptr argSize) => abi_package.IntRegArgAddr⁔(ref r, reg, argSize);
}
partial class abiᴛStructField {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Embedded(this ref StructField f) => abi_package.Embedded⁔(ref f);
}
partial class abiᴛType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Kind Kind(this ref Type t) => abi_package.Kind⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool HasName(this ref Type t) => abi_package.HasName⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Pointers(this ref Type t) => abi_package.Pointers⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsDirectIface(this ref Type t) => abi_package.IsDirectIface⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> GcSlice(this ref Type t, uintptr begin, uintptr end) => abi_package.GcSlice⁔(ref t, begin, end);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Type> Common(this ж<Type> Ꮡt) => abi_package.Common⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<UncommonType> Uncommon(this ж<Type> Ꮡt) => abi_package.Uncommon⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Align(this ref Type t) => abi_package.Align⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint FieldAlign(this ref Type t) => abi_package.FieldAlign⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<Method> ExportedMethods(this ж<Type> Ꮡt) => abi_package.ExportedMethods⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint NumMethod(this ж<Type> Ꮡt) => abi_package.NumMethod⁔(Ꮡt);
}
partial class abiᴛUncommonType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static unsafe slice<Method> Methods(this ж<UncommonType> Ꮡt) => abi_package.Methods⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static unsafe slice<Method> ExportedMethods(this ж<UncommonType> Ꮡt) => abi_package.ExportedMethods⁔(Ꮡt);
}
// </MethodForwarders>
