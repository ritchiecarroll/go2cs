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
using static go.crypto.@internal.fips140.bigmod_package;
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
[assembly: GoFieldAccess(typeof(Modulus))]
[assembly: GoFieldAccess(typeof(Nat))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("crypto/internal/fips140/bigmod/nat.go", "nat.cs", "ACFGzICYtIKCABIssoKosoKUgoKCgpSCgoKosoKCpoKCAAIQ8oKClKjWgoKUlKjCgoKssq7igoKigoKCgpSUgqYABhIACQKCgpSClAAGEAAIAoKCyoKUgqqigpSmgpKCgoKUgoKUgpSs0oKCAAIQAAgEgoKUgoKUrMSClIKClKzEgpSCpoKClAACEgAIAoKCqJKClKiSgoKCgoKUgpQAAhAACASCgpSCgrgAAhAACQSCgpSCgpQAAhAACQSCgpSClAACEAAJBIKClIKUAAIQAAgEgpSClIKCpoKCgqaCgriu1IKUgoKmrLKmgoKUABIqsoKCgriUggAKFriCgpSCyoKCgoK4rgAKDoKClAACENKCrsKCgoKClOaCgoKUgoKClKiSqJKotoKCAAIQAAgCpoKCggAIEoKCgoKCgoKCypQAAhQACwLcpoKClIKCpoKClKzSrNIAAhwADQKCpoIAAhIACQKUgoKCrLKCpgACEgAIAoKCAAIUAAsGrviCggACFAALAoKCgpSmgoKUnMKCABImgsrcggALGIL+goKCgoKCgoKUgraCgoKCgoKCgpSCtoKCgoKCgoKClIK2AAIQ8oKCgqaCgoKClAACEgAKAqaCpoKClKaCgpSmgrjKgoKClKSCgoKUpIKCgpQAAxgACwKCAAcQ3IKCpoKCgoKCpoKCgqaCgriCuAACEgAIAoLKgoKCgoKmAAIWAAoCgoKUgpSuwoKClAAIEAAiMoKUgqaCgpSCgoKCggAOHqaCgoKClIKCyoK4goKCgpSCpoKCgpSCuILMkoKUgoKClAAEFNKCpoKClA==", "", "", "1089=NewNat/1/15/2,NewNat/2/15/2,NewNat/3/15/2,NewNat/4/15/2,NewNat/5/15/2,NewNat/6/15/3,NewNat/7/15/3,NewNat/8/15/3,NewNat/9/15/3,NewNat/10/15/3,NewNat/11/15/4,NewNat/12/15/4,NewNat/13/15/4,NewNat/14/15/4,NewNat/15/15/4")]
[assembly: GoPositionMap("crypto/internal/fips140/bigmod/nat_noasm.go", "nat_noasm.cs", "AAoWgqaCpoI=")]
// </GoSourcePositionMaps>

namespace go.crypto.@internal.fips140;

using static go.crypto.@internal.fips140.bigmod_package;

[GoPackage("bigmod")]
public static partial class bigmod_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    public partial struct Modulus {}
    public partial struct Nat {}
    public partial struct choice {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸcheck() => builtin.initPackage(typeof(go.crypto.@internal.fips140.check_package));
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
[GoMethodHost(typeof(bigmod_package.Modulus)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bigmodᴛModulus;
[GoMethodHost(typeof(bigmod_package.Nat)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bigmodᴛNat;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class bigmodᴛModulus {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Size(this ref Modulus m) => bigmod_package.Size⁔(ref m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint BitLen(this ref Modulus m) => bigmod_package.BitLen⁔(ref m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Nat> Nat(this ref Modulus m) => bigmod_package.Nat⁔(ref m);
}
partial class bigmodᴛNat {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Nat> expand(this ж<Nat> Ꮡx, nint n) => bigmod_package.expand⁔(Ꮡx, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Nat> reset(this ж<Nat> Ꮡx, nint n) => bigmod_package.reset⁔(Ꮡx, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Nat> resetToBytes(this ж<Nat> Ꮡx, slice<byte> b) => bigmod_package.resetToBytes⁔(Ꮡx, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Nat> trim(this ж<Nat> Ꮡx) => bigmod_package.trim⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Nat> set(this ж<Nat> Ꮡx, ж<Nat> Ꮡy) => bigmod_package.set⁔(Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<nuint> Bits(this ref Nat x) => bigmod_package.Bits⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Bytes(this ref Nat x, ж<Modulus> Ꮡm) => bigmod_package.Bytes⁔(ref x, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Nat>, error) SetBytes(this ж<Nat> Ꮡx, slice<byte> b, ж<Modulus> Ꮡm) => bigmod_package.SetBytes⁔(Ꮡx, b, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Nat>, error) SetOverflowingBytes(this ж<Nat> Ꮡx, slice<byte> b, ж<Modulus> Ꮡm) => bigmod_package.SetOverflowingBytes⁔(Ꮡx, b, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error setBytes(this ref Nat x, slice<byte> b) => bigmod_package.setBytes⁔(ref x, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Nat> SetUint(this ж<Nat> Ꮡx, nuint y) => bigmod_package.SetUint⁔(Ꮡx, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static choice Equal(this ref Nat x, ж<Nat> Ꮡy) => bigmod_package.Equal⁔(ref x, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static choice IsZero(this ref Nat x) => bigmod_package.IsZero⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static choice IsOne(this ref Nat x) => bigmod_package.IsOne⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static choice IsMinusOne(this ref Nat x, ж<Modulus> Ꮡm) => bigmod_package.IsMinusOne⁔(ref x, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static choice IsOdd(this ref Nat x) => bigmod_package.IsOdd⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nuint TrailingZeroBitsVarTime(this ref Nat x) => bigmod_package.TrailingZeroBitsVarTime⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static choice cmpGeq(this ref Nat x, ж<Nat> Ꮡy) => bigmod_package.cmpGeq⁔(ref x, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Nat> assign(this ж<Nat> Ꮡx, choice on, ж<Nat> Ꮡy) => bigmod_package.assign⁔(Ꮡx, on, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nuint /*c*/ add(this ref Nat x, ж<Nat> Ꮡy) => bigmod_package.add⁔(ref x, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nuint /*c*/ sub(this ref Nat x, ж<Nat> Ꮡy) => bigmod_package.sub⁔(ref x, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Nat> ShiftRightVarTime(this ж<Nat> Ꮡx, nuint n) => bigmod_package.ShiftRightVarTime⁔(Ꮡx, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint BitLenVarTime(this ref Nat x) => bigmod_package.BitLenVarTime⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Nat> shiftIn(this ж<Nat> Ꮡx, nuint y, ж<Modulus> Ꮡm) => bigmod_package.shiftIn⁔(Ꮡx, y, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Nat> Mod(this ж<Nat> Ꮡout, ж<Nat> Ꮡx, ж<Modulus> Ꮡm) => bigmod_package.Mod⁔(Ꮡout, Ꮡx, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Nat> ExpandFor(this ж<Nat> Ꮡx, ж<Modulus> Ꮡm) => bigmod_package.ExpandFor⁔(Ꮡx, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Nat> resetFor(this ж<Nat> Ꮡout, ж<Modulus> Ꮡm) => bigmod_package.resetFor⁔(Ꮡout, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void maybeSubtractModulus(this ж<Nat> Ꮡx, choice always, ж<Modulus> Ꮡm) => bigmod_package.maybeSubtractModulus⁔(Ꮡx, always, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Nat> Sub(this ж<Nat> Ꮡx, ж<Nat> Ꮡy, ж<Modulus> Ꮡm) => bigmod_package.Sub⁔(Ꮡx, Ꮡy, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Nat> SubOne(this ж<Nat> Ꮡx, ж<Modulus> Ꮡm) => bigmod_package.SubOne⁔(Ꮡx, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Nat> Add(this ж<Nat> Ꮡx, ж<Nat> Ꮡy, ж<Modulus> Ꮡm) => bigmod_package.Add⁔(Ꮡx, Ꮡy, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Nat> montgomeryRepresentation(this ж<Nat> Ꮡx, ж<Modulus> Ꮡm) => bigmod_package.montgomeryRepresentation⁔(Ꮡx, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Nat> montgomeryReduction(this ж<Nat> Ꮡx, ж<Modulus> Ꮡm) => bigmod_package.montgomeryReduction⁔(Ꮡx, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Nat> montgomeryMul(this ж<Nat> Ꮡx, ж<Nat> Ꮡa, ж<Nat> Ꮡb, ж<Modulus> Ꮡm) => bigmod_package.montgomeryMul⁔(Ꮡx, Ꮡa, Ꮡb, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Nat> Mul(this ж<Nat> Ꮡx, ж<Nat> Ꮡy, ж<Modulus> Ꮡm) => bigmod_package.Mul⁔(Ꮡx, Ꮡy, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Nat> Exp(this ж<Nat> Ꮡout, ж<Nat> Ꮡx, slice<byte> e, ж<Modulus> Ꮡm) => bigmod_package.Exp⁔(Ꮡout, Ꮡx, e, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Nat> ExpShortVarTime(this ж<Nat> Ꮡout, ж<Nat> Ꮡx, nuint e, ж<Modulus> Ꮡm) => bigmod_package.ExpShortVarTime⁔(Ꮡout, Ꮡx, e, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Nat>, bool) InverseVarTime(this ж<Nat> Ꮡx, ж<Nat> Ꮡa, ж<Modulus> Ꮡm) => bigmod_package.InverseVarTime⁔(Ꮡx, Ꮡa, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Nat>, error) GCDVarTime(this ж<Nat> Ꮡx, ж<Nat> Ꮡa, ж<Nat> Ꮡb) => bigmod_package.GCDVarTime⁔(Ꮡx, Ꮡa, Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nuint DivShortVarTime(this ref Nat x, nuint y) => bigmod_package.DivShortVarTime⁔(ref x, y);
}
// </MethodForwarders>
