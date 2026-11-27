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
using static go.@internal.routebsd_package;
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
[assembly: GoImplement<InetAddr, Addr>(Pointer = true)]
[assembly: GoImplement<InterfaceAddrMessage, Message>(Pointer = true)]
[assembly: GoImplement<InterfaceAddrMessage, Message>]
[assembly: GoImplement<InterfaceMessage, Message>(Pointer = true)]
[assembly: GoImplement<InterfaceMessage, Message>]
[assembly: GoImplement<InterfaceMulticastAddrMessage, Message>(Pointer = true)]
[assembly: GoImplement<InterfaceMulticastAddrMessage, Message>]
[assembly: GoImplement<LinkAddr, Addr>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(InterfaceAddrMessage))]
[assembly: GoFieldAccess(typeof(InterfaceMessage))]
[assembly: GoFieldAccess(typeof(InterfaceMulticastAddrMessage))]
[assembly: GoFieldAccess(typeof(LinkAddr))]
[assembly: GoFieldAccess(typeof(wireFormat))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("internal/routebsd/address.go", "address.cs", "ABo6gJSCgpSCgpSCqgARIKKClIKUgpSCgpSCgoKCgpSCgpQABxCCgpS6kryUgpSCgqaCgpSUpqSClIKCgoKClILKgoLcpqS8ABUogqaCppSCmLiUpqSCgpSUpqSmpIKClJSmuIKCgoKClIKUgoKUgoKClLSCgoKUgoKClLSCgpSCgoKU2oKClO4=", "", "", "125=AddrFrom4/1/1/1;155=AddrFrom16/1/1/1;206=AddrFrom16/1/1/1;218=AddrFrom16/1/1/1;224=AddrFrom4/1/1/1;236=AddrFrom4/1/1/1")]
[assembly: GoPositionMap("internal/routebsd/binary.go", "binary.cs", "ABw8gqaCpoLagqaCpoI=")]
[assembly: GoPositionMap("internal/routebsd/interface.go", "interface.cs", "AClekoI=")]
[assembly: GoPositionMap("internal/routebsd/interface_classic.go", "interface_classic.cs", "AAscgoKUgoKUlAARGoKCgpSCuIK4poKClIKClPyClJSCgoKU", "", "", "22=Uint32/1/1/4,Uint16/1/1/5;66=Uint32/1/1/3")]
[assembly: GoPositionMap("internal/routebsd/interface_multicast.go", "interface_multicast.cs", "AAgSgoKUgoKUAAoOgoKClA==", "", "", "17=Uint32/1/1/3,Uint16/1/1/4")]
[assembly: GoPositionMap("internal/routebsd/message.go", "message.cs", "AA0gwoKSgoKCgpSClIKClIKUgoKUgpSmuIKU")]
[assembly: GoPositionMap("internal/routebsd/route.go", "route.cs", "AA8kgoKCnsKCgoKasoKmvrKCgpSCgpQ=")]
[assembly: GoPositionMap("internal/routebsd/sys.go", "sys.cs", "ABJEgoKU")]
[assembly: GoPositionMap("internal/routebsd/sys_darwin.go", "sys_darwin.cs", "AAkUktyCgpKCkoKSgpKCpA==")]
// </GoSourcePositionMaps>

namespace go.@internal;

using static go.@internal.routebsd_package;

[GoPackage("routebsd")]
public static partial class routebsd_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface binaryByteOrder {}
    internal partial struct binaryBigEndian {}
    internal partial struct binaryLittleEndian {}
    internal partial struct wireFormat {}
    public partial interface Addr {}
    public partial interface Message {}
    public partial struct InetAddr {}
    public partial struct InterfaceAddrMessage {}
    public partial struct InterfaceMessage {}
    public partial struct InterfaceMulticastAddrMessage {}
    public partial struct LinkAddr {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸnetip() => builtin.initPackage(typeof(net.netip_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸsyscall() => builtin.initPackage(typeof(syscall_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(routebsd_package.InetAddr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class routebsdᴛInetAddr;
[GoMethodHost(typeof(routebsd_package.InterfaceAddrMessage)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class routebsdᴛInterfaceAddrMessage;
[GoMethodHost(typeof(routebsd_package.InterfaceMessage)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class routebsdᴛInterfaceMessage;
[GoMethodHost(typeof(routebsd_package.InterfaceMulticastAddrMessage)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class routebsdᴛInterfaceMulticastAddrMessage;
[GoMethodHost(typeof(routebsd_package.LinkAddr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class routebsdᴛLinkAddr;
[GoMethodHost(typeof(routebsd_package.binaryBigEndian)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class routebsdᴛbinaryBigEndian;
[GoMethodHost(typeof(routebsd_package.binaryLittleEndian)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class routebsdᴛbinaryLittleEndian;
[GoMethodHost(typeof(routebsd_package.wireFormat)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class routebsdᴛwireFormat;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class routebsdᴛInetAddr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Family(this ref InetAddr a) => routebsd_package.Family⁔(ref a);
}
partial class routebsdᴛInterfaceAddrMessage {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void message(this InterfaceAddrMessage _) => routebsd_package.message⁔(_);
}
partial class routebsdᴛInterfaceMessage {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void message(this InterfaceMessage _) => routebsd_package.message⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint MTU(this ref InterfaceMessage m) => routebsd_package.MTU⁔(ref m);
}
partial class routebsdᴛInterfaceMulticastAddrMessage {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void message(this InterfaceMulticastAddrMessage _) => routebsd_package.message⁔(_);
}
partial class routebsdᴛLinkAddr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Family(this ref LinkAddr a) => routebsd_package.Family⁔(ref a);
}
partial class routebsdᴛbinaryBigEndian {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint16 Uint16(this binaryBigEndian _, slice<byte> b) => routebsd_package.Uint16⁔(_, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 Uint32(this binaryBigEndian _, slice<byte> b) => routebsd_package.Uint32⁔(_, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 Uint64(this binaryBigEndian _, slice<byte> b) => routebsd_package.Uint64⁔(_, b);
}
partial class routebsdᴛbinaryLittleEndian {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint16 Uint16(this binaryLittleEndian _, slice<byte> b) => routebsd_package.Uint16⁔(_, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 Uint32(this binaryLittleEndian _, slice<byte> b) => routebsd_package.Uint32⁔(_, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 Uint64(this binaryLittleEndian _, slice<byte> b) => routebsd_package.Uint64⁔(_, b);
}
partial class routebsdᴛwireFormat {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (Message, error) parseInterfaceMessage(this ref wireFormat w, slice<byte> b) => routebsd_package.parseInterfaceMessage⁔(ref w, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (Message, error) parseInterfaceAddrMessage(this ref wireFormat w, slice<byte> b) => routebsd_package.parseInterfaceAddrMessage⁔(ref w, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (Message, error) parseInterfaceMulticastAddrMessage(this ref wireFormat w, slice<byte> b) => routebsd_package.parseInterfaceMulticastAddrMessage⁔(ref w, b);
}
// </MethodForwarders>
