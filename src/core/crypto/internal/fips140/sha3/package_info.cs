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
using static go.crypto.@internal.fips140.sha3_package;
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
[assembly: GoFieldAccess(typeof(Digest))]
[assembly: GoFieldAccess(typeof(SHAKE))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("crypto/internal/fips140/sha3/cast.go", "cast.cs", "AAwagoK43IKCgpQ=", "14-31:1")]
[assembly: GoPositionMap("crypto/internal/fips140/sha3/hashes.go", "hashes.cs", "AAcQkqiSqJKokgAPKKKqog==")]
[assembly: GoPositionMap("crypto/internal/fips140/sha3/keccakf.go", "keccakf.cs", "AA8e")]
[assembly: GoPositionMap("crypto/internal/fips140/sha3/sha3.go", "sha3.cs", "ADNwgJaAlqSCgqaCgqiygqoACAq4lIKogILCgqaUgoKCpoK4qOSCpqaUgqaCgqaqooKUlKaCgsqCgoIADRiCpoKUtLS0tLaCgoIACgaCgqaCggANDKaCgoKmgpSSgpSCgpSU")]
[assembly: GoPositionMap("crypto/internal/fips140/sha3/sha3_noasm.go", "sha3_noasm.cs", "AAgSgqaClIKUgg==")]
[assembly: GoPositionMap("crypto/internal/fips140/sha3/shake.go", "shake.cs", "ABo0goKCgoKCuJSCgqaCgoKCpqKCgoKCgoKCpoKcsJiQlIKmqLKCgrqSgqaCpoKCgpSCpoKClIKUgqiSqJIAAhDkgqaigoKUAAIQ5IKmooKClA==")]
// </GoSourcePositionMaps>

namespace go.crypto.@internal.fips140;

using static go.crypto.@internal.fips140.sha3_package;

[GoPackage("sha3")]
public static partial class sha3_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct spongeDirection {}
    [GoValueClone("a")] public partial struct Digest {}
    [GoValueClone("d")] public partial struct SHAKE {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140() => builtin.initPackage(typeof(go.crypto.@internal.fips140_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140depsꓸcpu() => builtin.initPackage(typeof(go.crypto.@internal.fips140deps.cpu_package));
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
[GoMethodHost(typeof(sha3_package.Digest)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class sha3ᴛDigest;
[GoMethodHost(typeof(sha3_package.SHAKE)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class sha3ᴛSHAKE;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class sha3ᴛDigest {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint BlockSize(this ref Digest d) => sha3_package.BlockSize⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Size(this ref Digest d) => sha3_package.Size⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Reset(this ref Digest d) => sha3_package.Reset⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Digest> Clone(this ref Digest d) => sha3_package.Clone⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void permute(this ж<Digest> Ꮡd) => sha3_package.permute⁔(Ꮡd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void padAndPermute(this ж<Digest> Ꮡd) => sha3_package.padAndPermute⁔(Ꮡd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) Write(this ж<Digest> Ꮡd, slice<byte> p) => sha3_package.Write⁔(Ꮡd, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) writeGeneric(this ж<Digest> Ꮡd, slice<byte> p) => sha3_package.writeGeneric⁔(Ꮡd, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) readGeneric(this ж<Digest> Ꮡd, slice<byte> @out) => sha3_package.readGeneric⁔(Ꮡd, @out);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Sum(this ref Digest d, slice<byte> b) => sha3_package.Sum⁔(ref d, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> sumGeneric(this ref Digest d, slice<byte> b) => sha3_package.sumGeneric⁔(ref d, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) MarshalBinary(this ref Digest d) => sha3_package.MarshalBinary⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) AppendBinary(this ref Digest d, slice<byte> b) => sha3_package.AppendBinary⁔(ref d, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnmarshalBinary(this ref Digest d, slice<byte> b) => sha3_package.UnmarshalBinary⁔(ref d, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) write(this ж<Digest> Ꮡd, slice<byte> p) => sha3_package.write⁔(Ꮡd, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) read(this ж<Digest> Ꮡd, slice<byte> @out) => sha3_package.read⁔(Ꮡd, @out);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> sum(this ref Digest d, slice<byte> b) => sha3_package.sum⁔(ref d, b);
}
partial class sha3ᴛSHAKE {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint BlockSize(this ref SHAKE s) => sha3_package.BlockSize⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Size(this ref SHAKE s) => sha3_package.Size⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Sum(this ref SHAKE s, slice<byte> @in) => sha3_package.Sum⁔(ref s, @in);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) Write(this ж<SHAKE> Ꮡs, slice<byte> p) => sha3_package.Write⁔(Ꮡs, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) Read(this ж<SHAKE> Ꮡs, slice<byte> @out) => sha3_package.Read⁔(Ꮡs, @out);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Reset(this ж<SHAKE> Ꮡs) => sha3_package.Reset⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<SHAKE> Clone(this ref SHAKE s) => sha3_package.Clone⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) MarshalBinary(this ref SHAKE s) => sha3_package.MarshalBinary⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) AppendBinary(this ref SHAKE s, slice<byte> b) => sha3_package.AppendBinary⁔(ref s, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnmarshalBinary(this ref SHAKE s, slice<byte> b) => sha3_package.UnmarshalBinary⁔(ref s, b);
}
// </MethodForwarders>
