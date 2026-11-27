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
using static go.math.rand_package;
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
[assembly: GoImplement<Rand, Source64>(Pointer = true)]
[assembly: GoImplement<Rand, Source>(Pointer = true)]
[assembly: GoImplement<lockedSource, Source>(Pointer = true)]
[assembly: GoImplement<rngSource, Source>(Pointer = true)]
[assembly: GoImplement<runtimeSource, Source64>(Pointer = true)]
[assembly: GoImplement<runtimeSource, Source>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(Rand))]
[assembly: GoFieldAccess(typeof(Zipf))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("math/rand/exp.go", "exp.cs", "ABI88oKCgoKClIKUosoANWwAQYQB")]
[assembly: GoPositionMap("math/rand/normal.go", "normal.cs", "ABIugoKUAAIU8oKCgoKUppSCgoKCpoKUlKLKABs4ACFE")]
[assembly: GoPositionMap("math/rand/rand.go", "rand.cs", "ADFmwqaCgoIAECiigqrCgoKmgqiAloCWkoKUqICWkoKqooKUgpSCgoKUqqKClIKUgoKClAACGAAJAoKCgoKCgoKCpqqigpSClKgAESKCgoKUqMiCgoKUqqLcgoKClKyyggAIEoKCgpSCgr7SlKSUprKCgoKCgoKUlJSCgpSCggANGqaYooKokoKCgpTc7qaoAAkSgqaCpoKmwoKCggACLAATAoKUlKaCgoIAChaClJS8kJiQmJCYkJaAmqCaoJqgmJCYkJiQmqAAARDQAAEU8AABEuDusoKCgqaygoKCpqKCgqjCgoKCqqKClLrSgoKC")]
[assembly: GoPositionMap("math/rand/rng.go", "rng.cs", "ABQwAKABxgKSvIKCgoKUqJKClIKClIKmgoKCgoKCgoKCgoLMkqiSgoKmgoKmgoI=")]
[assembly: GoPositionMap("math/rand/zipf.go", "zipf.cs", "ABo2graCvuKCgpSCgoKCgoKCgqKqwoKUlIKCgoKCgpSCpg==")]
// </GoSourcePositionMaps>

namespace go.math;

using static go.math.rand_package;

[GoPackage("rand")]
public static partial class rand_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct lockedSource {}
    [GoValueClone("vec")] internal partial struct rngSource {}
    internal partial struct runtimeSource {}
    public partial interface Source {}
    public partial interface Source64 {}
    public partial struct Rand {}
    public partial struct Zipf {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(rand_package.Rand)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class randᴛRand;
[GoMethodHost(typeof(rand_package.Zipf)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class randᴛZipf;
[GoMethodHost(typeof(rand_package.lockedSource)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class randᴛlockedSource;
[GoMethodHost(typeof(rand_package.rngSource)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class randᴛrngSource;
[GoMethodHost(typeof(rand_package.runtimeSource)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class randᴛruntimeSource;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class randᴛRand {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static float64 ExpFloat64(this ref Rand r) => rand_package.ExpFloat64⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static float64 NormFloat64(this ref Rand r) => rand_package.NormFloat64⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Seed(this ж<Rand> Ꮡr, int64 seed) => rand_package.Seed⁔(Ꮡr, seed);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 Int63(this ref Rand r) => rand_package.Int63⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint32 Uint32(this ref Rand r) => rand_package.Uint32⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint64 Uint64(this ref Rand r) => rand_package.Uint64⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int32 Int31(this ref Rand r) => rand_package.Int31⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Int(this ref Rand r) => rand_package.Int⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 Int63n(this ref Rand r, int64 n) => rand_package.Int63n⁔(ref r, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int32 Int31n(this ref Rand r, int32 n) => rand_package.Int31n⁔(ref r, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int32 int31n(this ref Rand r, int32 n) => rand_package.int31n⁔(ref r, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Intn(this ref Rand r, nint n) => rand_package.Intn⁔(ref r, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static float64 Float64(this ref Rand r) => rand_package.Float64⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static float32 Float32(this ref Rand r) => rand_package.Float32⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<nint> Perm(this ref Rand r, nint n) => rand_package.Perm⁔(ref r, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Shuffle(this ref Rand r, nint n, Action<nint, nint> swap) => rand_package.Shuffle⁔(ref r, n, swap);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) Read(this ж<Rand> Ꮡr, slice<byte> p) => rand_package.Read⁔(Ꮡr, p);
}
partial class randᴛZipf {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static float64 h(this ref Zipf z, float64 x) => rand_package.h⁔(ref z, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static float64 hinv(this ref Zipf z, float64 x) => rand_package.hinv⁔(ref z, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint64 Uint64(this ж<Zipf> Ꮡz) => rand_package.Uint64⁔(Ꮡz);
}
partial class randᴛlockedSource {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 /*n*/ Int63(this ж<lockedSource> Ꮡr) => rand_package.Int63⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 /*n*/ Uint64(this ж<lockedSource> Ꮡr) => rand_package.Uint64⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Seed(this ж<lockedSource> Ꮡr, int64 seed) => rand_package.Seed⁔(Ꮡr, seed);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void seedPos(this ж<lockedSource> Ꮡr, int64 seed, ж<int8> ᏑreadPos) => rand_package.seedPos⁔(Ꮡr, seed, ᏑreadPos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void seed(this ref lockedSource r, int64 seed) => rand_package.seed⁔(ref r, seed);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) read(this ж<lockedSource> Ꮡr, slice<byte> p, ж<int64> ᏑreadVal, ж<int8> ᏑreadPos) => rand_package.read⁔(Ꮡr, p, ᏑreadVal, ᏑreadPos);
}
partial class randᴛrngSource {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Seed(this ref rngSource rng, int64 seed) => rand_package.Seed⁔(ref rng, seed);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 Int63(this ref rngSource rng) => rand_package.Int63⁔(ref rng);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 Uint64(this ref rngSource rng) => rand_package.Uint64⁔(ref rng);
}
partial class randᴛruntimeSource {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 Int63(this ref runtimeSource _) => rand_package.Int63⁔(ref _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Seed(this ref runtimeSource _Δp0, int64 _Δp1) => rand_package.Seed⁔(ref _Δp0, _Δp1);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 Uint64(this ref runtimeSource _) => rand_package.Uint64⁔(ref _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) read(this ж<runtimeSource> Ꮡfs, slice<byte> p, ж<int64> ᏑreadVal, ж<int8> ᏑreadPos) => rand_package.read⁔(Ꮡfs, p, ᏑreadVal, ᏑreadPos);
}
// </MethodForwarders>
