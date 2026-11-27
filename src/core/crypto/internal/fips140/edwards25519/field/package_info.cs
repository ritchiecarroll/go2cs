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
using static go.crypto.@internal.fips140.edwards25519.field_package;
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

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("crypto/internal/fips140/edwards25519/field/fe.go", "fe.cs", "AChMlrKCppaygqiy7oKCgoK4lIKCgoKCgoKUlKjSgoKCgoKo9oKCgoKCqJKstgALBIKCgoKCgpSCgpSUgoKUlIKClJSCgpSUgoKUlIKClJSCgpSUgoKCgpSowoIABhYACgKCuIKUgpSClIKmgpSolqKmooIACBKCgoKUgoKClKiygqiAltKCgoKCgoKosoKCgoKCgoKCgoKCgoKCgqiSqLKokoKokoKowoKCgoKCgoKCgqaowoKCgqiStIKCgoKCgoKCgpSCgoKUgoKClIKCgpSCgoKUgoKClIKCgpSCgoK4AAIQ0qaCgoKUlIKCgpSUlII=")]
[assembly: GoPositionMap("crypto/internal/fips140/edwards25519/field/fe_amd64_noasm.go", "fe_amd64_noasm.cs", "AAgSlA==")]
[assembly: GoPositionMap("crypto/internal/fips140/edwards25519/field/fe_generic.go", "fe_generic.cs", "ABAgkoKokoKCgqikqJKCgoKokoKCgqiSpoKCgoKClIKCgoIAIkaCgoKCpoKCgoKmgoKCgqaCgoKCpoKCgoIAHDqCgoKClIKCgoLcgoKCgqaCgoKCggAYMoKCpoKCpoKCpoKCpoKClIKCgoKUgoKCgpSCgoKCquaCkoKCgpQ=")]
// </GoSourcePositionMaps>

namespace go.crypto.@internal.fips140.edwards25519;

using static go.crypto.@internal.fips140.edwards25519.field_package;

[GoPackage("field")]
public static partial class field_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct uint128 {}
    public partial struct Element {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸcheck() => builtin.initPackage(typeof(go.crypto.@internal.fips140.check_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸsubtle() => builtin.initPackage(typeof(go.crypto.@internal.fips140.subtle_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(math.bits_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(field_package.Element)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fieldᴛElement;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class fieldᴛElement {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Element> Zero(this ж<Element> Ꮡv) => field_package.Zero⁔(Ꮡv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Element> One(this ж<Element> Ꮡv) => field_package.One⁔(Ꮡv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Element> reduce(this ж<Element> Ꮡv) => field_package.reduce⁔(Ꮡv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Element> Add(this ж<Element> Ꮡv, ж<Element> Ꮡa, ж<Element> Ꮡb) => field_package.Add⁔(Ꮡv, Ꮡa, Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Element> Subtract(this ж<Element> Ꮡv, ж<Element> Ꮡa, ж<Element> Ꮡb) => field_package.Subtract⁔(Ꮡv, Ꮡa, Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Element> Negate(this ж<Element> Ꮡv, ж<Element> Ꮡa) => field_package.Negate⁔(Ꮡv, Ꮡa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Element> Invert(this ж<Element> Ꮡv, ж<Element> Ꮡz) => field_package.Invert⁔(Ꮡv, Ꮡz);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Element> Set(this ж<Element> Ꮡv, ж<Element> Ꮡa) => field_package.Set⁔(Ꮡv, Ꮡa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Element>, error) SetBytes(this ж<Element> Ꮡv, slice<byte> x) => field_package.SetBytes⁔(Ꮡv, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Bytes(this ref Element v) => field_package.Bytes⁔(ref v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> bytes(this ref Element v, /*[32]*/ ж<array<byte>> Ꮡout) => field_package.bytes⁔(ref v, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Equal(this ref Element v, ж<Element> Ꮡu) => field_package.Equal⁔(ref v, Ꮡu);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Element> Select(this ж<Element> Ꮡv, ж<Element> Ꮡa, ж<Element> Ꮡb, nint cond) => field_package.Select⁔(Ꮡv, Ꮡa, Ꮡb, cond);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Swap(this ref Element v, ж<Element> Ꮡu, nint cond) => field_package.Swap⁔(ref v, Ꮡu, cond);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint IsNegative(this ref Element v) => field_package.IsNegative⁔(ref v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Element> Absolute(this ж<Element> Ꮡv, ж<Element> Ꮡu) => field_package.Absolute⁔(Ꮡv, Ꮡu);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Element> Multiply(this ж<Element> Ꮡv, ж<Element> Ꮡx, ж<Element> Ꮡy) => field_package.Multiply⁔(Ꮡv, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Element> Square(this ж<Element> Ꮡv, ж<Element> Ꮡx) => field_package.Square⁔(Ꮡv, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Element> Mult32(this ж<Element> Ꮡv, ж<Element> Ꮡx, uint32 y) => field_package.Mult32⁔(Ꮡv, Ꮡx, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Element> Pow22523(this ж<Element> Ꮡv, ж<Element> Ꮡx) => field_package.Pow22523⁔(Ꮡv, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Element> R, nint wasSquare) SqrtRatio(this ж<Element> Ꮡr, ж<Element> Ꮡu, ж<Element> Ꮡv) => field_package.SqrtRatio⁔(Ꮡr, Ꮡu, Ꮡv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Element> carryPropagate(this ж<Element> Ꮡv) => field_package.carryPropagate⁔(Ꮡv);
}
// </MethodForwarders>
