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
using static go.net.netip_package;
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
[assembly: GoImplement<parseAddrError, error>]
[assembly: GoImplement<parsePrefixError, error>]
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
[assembly: GoFieldAccess(typeof(Addr))]
[assembly: GoFieldAccess(typeof(uint128))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("net/netip/netip.go", "netip.cs", "AEOKAYKakJiQloCWgJaAlrIABRLSAAgYsoKUpMimqqKCgpQACBKCgoKUpoKSgoKCgoKUgoKClLiCpoKUgoKClKaClIKosoKCgpSoktyCgoKClLiCpoKClILKgqaCgoKCgoKCgoKUlJSUlKaUuIKUlJSmgoKUgoKUgoK4goKmgoK4goKClKaCgpSCgoLcgriCgpSCgpSClJSsspSklKqiqsKqwgACEPasoJ7SlKSUqJKClKyykoKUgpSCgpSClIKClIKUgpKClIKmrKCasqyyqqKuwoKUrLKClIKClIKqooKUgqiSqJKCyoK4gpSokoLKgriClKiSgsqCuIKUqsaClKiSgsqCuIKUAAIYAAkClKaCyoKmAAIUwoK4pqqmpq7CAAYQ0oKUgpSkgpS0graCAAIQ8oKCrNKCgpSClKiSlKSCgqSCgoK8ooKClKaUpqqigoKUgpSCAAYeAAwClKSkgpS+spSkpIKUAAgSyIKUgpSoyIKUgpSClKiSpoKCgoKmgoKCgoKCgoKmgoKCgqaCgoKCgpQAAhAABRDygoKmgoKCgoKUgriCgoKCgpSCpqaCgpSs0pSmgoKCgqampoKUqqKssoLGgrSCgoKUgqQAAhDSgoKUgoKoksa0goKkpoKUpKQAAxDC6qKClIKkgqSCpIKUAAgWkJaAloAACw4ACQKCgqaCgpSClIKClIKmrsKCgoKUgoKUgoKClIKCgpSqooKClKqQmrKClOaigpSkgoK0goKCgpSCgoKUpIKCrLKUpLSCgpSUpIKCqqKssoLGgrSCpKyygoKUgoKssoKClKyyqqKClIKCgpSCAA0uAAgCgoKU3oCaoJqglJaAngAICICCtoCCtgAHEIIAAhTygoKUgoKmgqamgqaCgpSCgpSClKqigoKUrsKCAAIU8oKUkpQACBLKAAMS0oKUgpSClIKSlJSCnMKClIKUrLKClIK4gpSCgpS4goKqoqyygsaCtIKkrLKCgpSCgqyygoKUrMSqooKUgoKClILokoKU", "", "", "104=BEUint64/1/2/2,BEUint64/2/2/3;645=IsLoopback/1/1/1,IsMulticast/1/1/2,IsLinkLocalUnicast/1/1/3;661=v4/2/5/1,v4/3/5/1,v4/4/5/2,v4/5/5/2;1394=withoutZone/1/1/1")]
[assembly: GoPositionMap("net/netip/uint128.go", "uint128.cs", "ABMoogACEMCWkqiSqJKokqiSgqiSgq7CqqKqog==")]
// </GoSourcePositionMaps>

namespace go.net;

using static go.net.netip_package;

[GoPackage("netip")]
public static partial class netip_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct addrDetail {}
    internal partial struct parseAddrError {}
    internal partial struct parsePrefixError {}
    internal partial struct uint128 {}
    public partial struct Addr {}
    public partial struct AddrPort {}
    public partial struct Prefix {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸbytealg() => builtin.initPackage(typeof(@internal.bytealg_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(go.math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸunique() => builtin.initPackage(typeof(unique_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(netip_package.Addr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netipᴛAddr;
[GoMethodHost(typeof(netip_package.AddrPort)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netipᴛAddrPort;
[GoMethodHost(typeof(netip_package.Prefix)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netipᴛPrefix;
[GoMethodHost(typeof(netip_package.parseAddrError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netipᴛparseAddrError;
[GoMethodHost(typeof(netip_package.parsePrefixError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netipᴛparsePrefixError;
[GoMethodHost(typeof(netip_package.uint128)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netipᴛuint128;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class netipᴛAddr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint8 v4(this Addr ip, uint8 i) => netip_package.v4⁔(ip, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint8 v6(this Addr ipʗp, uint8 i) => netip_package.v6⁔(ipʗp, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint16 v6u16(this Addr ipʗp, uint8 i) => netip_package.v6u16⁔(ipʗp, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isZero(this Addr ip) => netip_package.isZero⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsValid(this Addr ip) => netip_package.IsValid⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint BitLen(this Addr ip) => netip_package.BitLen⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Zone(this Addr ip) => netip_package.Zone⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Compare(this Addr ip, Addr ip2) => netip_package.Compare⁔(ip, ip2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Less(this Addr ip, Addr ip2) => netip_package.Less⁔(ip, ip2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Is4(this Addr ip) => netip_package.Is4⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Is4In6(this Addr ip) => netip_package.Is4In6⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Is6(this Addr ip) => netip_package.Is6⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Addr Unmap(this Addr ip) => netip_package.Unmap⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Addr WithZone(this Addr ip, @string zone) => netip_package.WithZone⁔(ip, zone);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Addr withoutZone(this Addr ip) => netip_package.withoutZone⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool hasZone(this Addr ip) => netip_package.hasZone⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsLinkLocalUnicast(this Addr ip) => netip_package.IsLinkLocalUnicast⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsLoopback(this Addr ip) => netip_package.IsLoopback⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsMulticast(this Addr ip) => netip_package.IsMulticast⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsInterfaceLocalMulticast(this Addr ip) => netip_package.IsInterfaceLocalMulticast⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsLinkLocalMulticast(this Addr ip) => netip_package.IsLinkLocalMulticast⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsGlobalUnicast(this Addr ip) => netip_package.IsGlobalUnicast⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsPrivate(this Addr ip) => netip_package.IsPrivate⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsUnspecified(this Addr ip) => netip_package.IsUnspecified⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (Prefix, error) Prefix(this Addr ip, nint b) => netip_package.Prefix⁔(ip, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static array<byte> /*a16*/ As16(this Addr ip) => netip_package.As16⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static array<byte> /*a4*/ As4(this Addr ip) => netip_package.As4⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> AsSlice(this Addr ip) => netip_package.AsSlice⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Addr Next(this Addr ip) => netip_package.Next⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Addr Prev(this Addr ip) => netip_package.Prev⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Addr ip) => netip_package.String⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> AppendTo(this Addr ip, slice<byte> b) => netip_package.AppendTo⁔(ip, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string string4(this Addr ip) => netip_package.string4⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> appendTo4(this Addr ip, slice<byte> ret) => netip_package.appendTo4⁔(ip, ret);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string string4In6(this Addr ip) => netip_package.string4In6⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> appendTo4In6(this Addr ip, slice<byte> ret) => netip_package.appendTo4In6⁔(ip, ret);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string string6(this Addr ip) => netip_package.string6⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> appendTo6(this Addr ip, slice<byte> ret) => netip_package.appendTo6⁔(ip, ret);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string StringExpanded(this Addr ip) => netip_package.StringExpanded⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) AppendText(this Addr ip, slice<byte> b) => netip_package.AppendText⁔(ip, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) MarshalText(this Addr ip) => netip_package.MarshalText⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnmarshalText(this ref Addr ip, slice<byte> text) => netip_package.UnmarshalText⁔(ref ip, text);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) AppendBinary(this Addr ip, slice<byte> b) => netip_package.AppendBinary⁔(ip, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint marshalBinarySize(this Addr ip) => netip_package.marshalBinarySize⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) MarshalBinary(this Addr ip) => netip_package.MarshalBinary⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnmarshalBinary(this ref Addr ip, slice<byte> b) => netip_package.UnmarshalBinary⁔(ref ip, b);
}
partial class netipᴛAddrPort {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Addr Addr(this AddrPort p) => netip_package.Addr⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint16 Port(this AddrPort p) => netip_package.Port⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsValid(this AddrPort p) => netip_package.IsValid⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Compare(this AddrPort p, AddrPort p2) => netip_package.Compare⁔(p, p2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this AddrPort p) => netip_package.String⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> AppendTo(this AddrPort p, slice<byte> b) => netip_package.AppendTo⁔(p, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) AppendText(this AddrPort p, slice<byte> b) => netip_package.AppendText⁔(p, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) MarshalText(this AddrPort p) => netip_package.MarshalText⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnmarshalText(this ref AddrPort p, slice<byte> text) => netip_package.UnmarshalText⁔(ref p, text);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) AppendBinary(this AddrPort p, slice<byte> b) => netip_package.AppendBinary⁔(p, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) MarshalBinary(this AddrPort p) => netip_package.MarshalBinary⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnmarshalBinary(this ref AddrPort p, slice<byte> b) => netip_package.UnmarshalBinary⁔(ref p, b);
}
partial class netipᴛPrefix {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Addr Addr(this Prefix p) => netip_package.Addr⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Bits(this Prefix p) => netip_package.Bits⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsValid(this Prefix p) => netip_package.IsValid⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isZero(this Prefix p) => netip_package.isZero⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsSingleIP(this Prefix p) => netip_package.IsSingleIP⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Compare(this Prefix p, Prefix p2) => netip_package.Compare⁔(p, p2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Prefix Masked(this Prefix p) => netip_package.Masked⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Contains(this Prefix p, Addr ip) => netip_package.Contains⁔(p, ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Overlaps(this Prefix p, Prefix o) => netip_package.Overlaps⁔(p, o);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> AppendTo(this Prefix p, slice<byte> b) => netip_package.AppendTo⁔(p, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) AppendText(this Prefix p, slice<byte> b) => netip_package.AppendText⁔(p, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) MarshalText(this Prefix p) => netip_package.MarshalText⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnmarshalText(this ref Prefix p, slice<byte> text) => netip_package.UnmarshalText⁔(ref p, text);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) AppendBinary(this Prefix p, slice<byte> b) => netip_package.AppendBinary⁔(p, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) MarshalBinary(this Prefix p) => netip_package.MarshalBinary⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnmarshalBinary(this ref Prefix p, slice<byte> b) => netip_package.UnmarshalBinary⁔(ref p, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Prefix p) => netip_package.String⁔(p);
}
partial class netipᴛparseAddrError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this parseAddrError err) => netip_package.Error⁔(err);
}
partial class netipᴛparsePrefixError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this parsePrefixError err) => netip_package.Error⁔(err);
}
partial class netipᴛuint128 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isZero(this uint128 u) => netip_package.isZero⁔(u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint128 and(this uint128 u, uint128 m) => netip_package.and⁔(u, m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint128 xor(this uint128 u, uint128 m) => netip_package.xor⁔(u, m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint128 or(this uint128 u, uint128 m) => netip_package.or⁔(u, m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint128 not(this uint128 u) => netip_package.not⁔(u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint128 subOne(this uint128 u) => netip_package.subOne⁔(u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint128 addOne(this uint128 u) => netip_package.addOne⁔(u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static array<ж<uint64>> halves(this ж<uint128> Ꮡu) => netip_package.halves⁔(Ꮡu);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint128 bitsSetFrom(this uint128 u, uint8 bit) => netip_package.bitsSetFrom⁔(u, bit);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint128 bitsClearedFrom(this uint128 u, uint8 bit) => netip_package.bitsClearedFrom⁔(u, bit);
}
// </MethodForwarders>
