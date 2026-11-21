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
using static go.math.rand.rand_package;
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
[assembly: GoImplement<ChaCha8, Source>(Pointer = true)]
[assembly: GoImplement<PCG, Source>(Pointer = true)]
[assembly: GoImplement<Rand, Source>(Pointer = true)]
[assembly: GoImplement<Zipf, Source>(Pointer = true)]
[assembly: GoImplement<runtimeSource, Source>]
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
[assembly: GoFieldAccess(typeof(ChaCha8))]
[assembly: GoFieldAccess(typeof(Zipf))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("math/rand/v2/chacha8.go", "chacha8.cs", "ABUwsoKCqMKCgqiygoKClAADFAAKAoKCgpSCgoKUgoKClOiygoKCgoKUlKaCgpSmgoKUqLKCgoKUqKQ=")]
[assembly: GoPositionMap("math/rand/v2/exp.go", "exp.cs", "ABI88oKCgoKCgpSClILKADVsAEGEAQ==")]
[assembly: GoPositionMap("math/rand/v2/normal.go", "normal.cs", "ABIugoKUAAIU8oKCgoKClKaUgoKCgqaClJSCygAbOAAhRA==")]
[assembly: GoPositionMap("math/rand/v2/pcg.go", "pcg.cs", "ABcwkqiSgqiSgoKCqJKmlpKClIKCpgADEgANEIKCgoKCgqiSAAsakoKCgoI=")]
[assembly: GoPositionMap("math/rand/v2/rand.go", "rand.cs", "ACZQoqiAloCWgJaAloCWgJiigpSqooKUqJKClIIAIEKCgoKCpqqiggAQIoKCgoKCgoKCgoKCgoKmqqKClKqigpTOooKUqqKClKikqKSqooKClJCSrLKCAAgSgoIACRSW6oKqkJiQmqCaoJiQmJCWgJaAmqCaoJqgmqCa0oKUAAcUkJiQmJCaoAABFPAAARLg", "226-226:1")]
[assembly: GoPositionMap("math/rand/v2/zipf.go", "zipf.cs", "ABo2gqaCruKCgpSCgoKCgoKCgoKqwoKUlIKCgoKCgpSCpg==")]
// </GoSourcePositionMaps>

namespace go.math.rand;

using static go.math.rand.rand_package;

[GoPackage("rand", ImportPath = "math/rand/v2")]
public static partial class rand_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface intType<ΔT> {}
    internal partial struct runtimeSource {}
    public partial interface Source {}
    [GoValueClone("state", "readBuf")] public partial struct ChaCha8 {}
    public partial struct PCG {}
    public partial struct Rand {}
    public partial struct Zipf {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(go.math.bits_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(rand_package.ChaCha8)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class randᴛChaCha8;
[GoMethodHost(typeof(rand_package.PCG)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class randᴛPCG;
[GoMethodHost(typeof(rand_package.Rand)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class randᴛRand;
[GoMethodHost(typeof(rand_package.Zipf)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class randᴛZipf;
[GoMethodHost(typeof(rand_package.runtimeSource)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class randᴛruntimeSource;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class randᴛChaCha8 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Seed(this ж<ChaCha8> Ꮡc, /*[32]*/ array<byte> seed) => rand_package.Seed⁔(Ꮡc, seed);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint64 Uint64(this ж<ChaCha8> Ꮡc) => rand_package.Uint64⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) Read(this ж<ChaCha8> Ꮡc, slice<byte> p) => rand_package.Read⁔(Ꮡc, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnmarshalBinary(this ж<ChaCha8> Ꮡc, slice<byte> data) => rand_package.UnmarshalBinary⁔(Ꮡc, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) AppendBinary(this ж<ChaCha8> Ꮡc, slice<byte> b) => rand_package.AppendBinary⁔(Ꮡc, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) MarshalBinary(this ж<ChaCha8> Ꮡc) => rand_package.MarshalBinary⁔(Ꮡc);
}
partial class randᴛPCG {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Seed(this ref PCG p, uint64 seed1, uint64 seed2) => rand_package.Seed⁔(ref p, seed1, seed2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) AppendBinary(this ref PCG p, slice<byte> b) => rand_package.AppendBinary⁔(ref p, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) MarshalBinary(this ref PCG p) => rand_package.MarshalBinary⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnmarshalBinary(this ref PCG p, slice<byte> data) => rand_package.UnmarshalBinary⁔(ref p, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uint64 hi, uint64 lo) next(this ref PCG p) => rand_package.next⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint64 Uint64(this ref PCG p) => rand_package.Uint64⁔(ref p);
}
partial class randᴛRand {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static float64 ExpFloat64(this ref Rand r) => rand_package.ExpFloat64⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static float64 NormFloat64(this ref Rand r) => rand_package.NormFloat64⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 Int64(this ref Rand r) => rand_package.Int64⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint32 Uint32(this ref Rand r) => rand_package.Uint32⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint64 Uint64(this ref Rand r) => rand_package.Uint64⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int32 Int32(this ref Rand r) => rand_package.Int32⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Int(this ref Rand r) => rand_package.Int⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nuint Uint(this ref Rand r) => rand_package.Uint⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 Int64N(this ref Rand r, int64 n) => rand_package.Int64N⁔(ref r, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint64 Uint64N(this ref Rand r, uint64 n) => rand_package.Uint64N⁔(ref r, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 uint64n(this ref Rand r, uint64 n) => rand_package.uint64n⁔(ref r, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 uint32n(this ref Rand r, uint32 n) => rand_package.uint32n⁔(ref r, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int32 Int32N(this ref Rand r, int32 n) => rand_package.Int32N⁔(ref r, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint32 Uint32N(this ref Rand r, uint32 n) => rand_package.Uint32N⁔(ref r, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint IntN(this ref Rand r, nint n) => rand_package.IntN⁔(ref r, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nuint UintN(this ref Rand r, nuint n) => rand_package.UintN⁔(ref r, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static float64 Float64(this ref Rand r) => rand_package.Float64⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static float32 Float32(this ref Rand r) => rand_package.Float32⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<nint> Perm(this ref Rand r, nint n) => rand_package.Perm⁔(ref r, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Shuffle(this ref Rand r, nint n, Action<nint, nint> swap) => rand_package.Shuffle⁔(ref r, n, swap);
}
partial class randᴛZipf {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static float64 h(this ref Zipf z, float64 x) => rand_package.h⁔(ref z, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static float64 hinv(this ref Zipf z, float64 x) => rand_package.hinv⁔(ref z, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint64 Uint64(this ж<Zipf> Ꮡz) => rand_package.Uint64⁔(Ꮡz);
}
partial class randᴛruntimeSource {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 Uint64(this runtimeSource _) => rand_package.Uint64⁔(_);
}
// </MethodForwarders>
