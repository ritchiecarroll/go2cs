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
[assembly: GoFieldAccess(typeof(ChanType))]
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
[assembly: GoPositionMap("abi.go", "abi.cs", "ABFcgoKClIKCgpSCgoKUAAIYgoKUgoKUAAUQggACEoI=")]
[assembly: GoPositionMap("compiletype.go", "compiletype.cs", "AAgelpiWlg==")]
[assembly: GoPositionMap("escape.go", "escape.cs", "AAomgoLegoKU")]
[assembly: GoPositionMap("main.go", "main.cs", "7oKCgpSUgoI=")]
[assembly: GoPositionMap("switch.go", "switch.cs", "ABtAgoKYlKQ=")]
[assembly: GoPositionMap("type.go", "type.cs", "AEiIAoKClKYAHT6ijKiCgoKUppSCqJaCqIKmggARLqKClKaigpQAAhSCAA4kooKUpoIAFDKigoKUACoIooKUlKSsrKysrKysAAQSopSCpIKkgqSCpIKkqKKClKiigpSoooKUqKKClKiigpSolpQABxCCgoKUpqKCgpSoAA0igpSClIKUgpSCpqKClAANMoKmgqaCpoKmooKClIKUlKKCgpSCgpSmggANHIIADFKCqoKogqiCqIKqgoKCgoKCzIKClIKsgoKCgoKClLqCgpSCqIKClIKCpoKClIKUgoKClIKCgpSCgpSCpoKCgoKCgoKm")]
// </GoSourcePositionMaps>

namespace go;

using static go.main_package;

[GoPackage("main")]
[GoArchExclusive("amd64")]
[GoTestMatchingConsoleOutput]
public static partial class main_package
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
    public partial struct ArchFamilyType {}
    public partial struct ArrayType {}
    public partial struct ChanDir {}
    public partial struct ChanType {}
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
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(main_package.Kind)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛKind;
[GoMethodHost(typeof(main_package.Name)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛName;
[GoMethodHost(typeof(main_package.Type)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛType;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class mainᴛKind {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Kind k) => main_package.String⁔(k);
}
partial class mainᴛName {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<byte> DataChecked(this Name n, nint off, @string whySafe) => main_package.DataChecked⁔(n, off, whySafe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<byte> Data(this Name n, nint off) => main_package.Data⁔(n, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsExported(this Name n) => main_package.IsExported⁔(n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool HasTag(this Name n) => main_package.HasTag⁔(n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsEmbedded(this Name n) => main_package.IsEmbedded⁔(n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, nint) ReadVarint(this Name n, nint off) => main_package.ReadVarint⁔(n, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsBlank(this Name n) => main_package.IsBlank⁔(n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Name(this Name n) => main_package.Name⁔(n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Tag(this Name n) => main_package.Tag⁔(n);
}
partial class mainᴛType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Kind Kind(this ref Type t) => main_package.Kind⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool HasName(this ref Type t) => main_package.HasName⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Pointers(this ref Type t) => main_package.Pointers⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IfaceIndir(this ref Type t) => main_package.IfaceIndir⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsDirectIface(this ref Type t) => main_package.IsDirectIface⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> GcSlice(this ref Type t, uintptr begin, uintptr end) => main_package.GcSlice⁔(ref t, begin, end);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Len(this ж<Type> Ꮡt) => main_package.Len⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Type> Common(this ж<Type> Ꮡt) => main_package.Common⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ChanDir ChanDir(this ж<Type> Ꮡt) => main_package.ChanDir⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<UncommonType> Uncommon(this ж<Type> Ꮡt) => main_package.Uncommon⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Type> Elem(this ж<Type> Ꮡt) => main_package.Elem⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<StructType> StructType(this ж<Type> Ꮡt) => main_package.StructType⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<MapType> MapType(this ж<Type> Ꮡt) => main_package.MapType⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<ArrayType> ArrayType(this ж<Type> Ꮡt) => main_package.ArrayType⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<FuncType> FuncType(this ж<Type> Ꮡt) => main_package.FuncType⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<InterfaceType> InterfaceType(this ж<Type> Ꮡt) => main_package.InterfaceType⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uintptr Size(this ref Type t) => main_package.Size⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Align(this ref Type t) => main_package.Align⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint FieldAlign(this ref Type t) => main_package.FieldAlign⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<Method> ExportedMethods(this ж<Type> Ꮡt) => main_package.ExportedMethods⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint NumMethod(this ж<Type> Ꮡt) => main_package.NumMethod⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Type> Key(this ж<Type> Ꮡt) => main_package.Key⁔(Ꮡt);
}
// </MethodForwarders>
