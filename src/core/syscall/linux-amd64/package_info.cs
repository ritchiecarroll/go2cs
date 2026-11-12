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
using static go.syscall_package;

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
[assembly: GoImplement<Errno, error>]
[assembly: GoImplement<SockaddrInet4, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrInet6, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrLinklayer, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrNetlink, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrUnix, Sockaddr>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<WaitStatus, Signal>(Inverted = true, ValueType = "uint32")]
[assembly: GoImplicitConv<_C_int, WaitStatus>(Inverted = true, ValueType = "int32")]
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(Cmsghdr))]
[assembly: GoFieldAccess(typeof(NetlinkRouteRequest))]
[assembly: GoFieldAccess(typeof(NlMsghdr))]
[assembly: GoFieldAccess(typeof(RawSockaddrInet4))]
[assembly: GoFieldAccess(typeof(RawSockaddrInet6))]
[assembly: GoFieldAccess(typeof(Rlimit))]
[assembly: GoFieldAccess(typeof(RtAttr))]
[assembly: GoFieldAccess(typeof(SockaddrLinklayer))]
[assembly: GoFieldAccess(typeof(SockaddrNetlink))]
[assembly: GoFieldAccess(typeof(SockaddrUnix))]
[assembly: GoFieldAccess(typeof(SysProcAttr))]
[assembly: GoFieldAccess(typeof(caps))]
[assembly: GoFieldAccess(typeof(cloneArgs))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("syscall/dirent.go", "dirent.cs", "AA4ikoKUgpSmgpSkpKSkyIKUpKSkpAAEEOKCgoKCgpSCgoKCuIKUgoKClIKCgoK4gpSCgpQ=")]
[assembly: GoPositionMap("syscall/env_unix.go", "env_unix.cs", "ABY0lsSCgoKCgoKC3JTcopSClIKClILWwoKCpoKUgoKUgoKCpuaigoKUgoK4goKCyoKUgoKClIKUgoLWopSClJSCoqSigoKCgoKCpg==")]
[assembly: GoPositionMap("syscall/exec_linux.go", "exec_linux.cs", "AG7oAZLckoIAARoAEAaCgpSCuIKCpoKCpoKCpqKmABMqgJaAABA4ABEEAAMmACMilJSCgqaCgpSClJS4gtyCgoKClJS4goKCuIKClIKmgriCgpSC3IKCgqaClJSm7tyCgoLKgoKUooKUgoKUgoLKgoKCypSCgriCgoKUuKKC3KaCgoKmgoKilKKClIKmopSigpSCuIKCopSigpSCAAoWgsKC3IKigsqCgoKClIKCgqaCgpSCgrimlMKmxoKmwqaigoLcgqKCyoKCgtyCgoKCgu6CgoKUgpSCgoKUgoKUgsqCgoKUpoKClLiCggAHEIK4goKCyoKCgsoAChaiggAIDoKCgsr6hLKCuIKCgpSokoKCpoKCpoKmrsKCgoKmgoKUpoKCpqqigoKCuJSAgqSCgILIqJKCggAZEOKCgoKmuIKCyoKCgqaCpsruggAAJIKCgqKCpoKmAAUWAAsCgpSUlO6mgg==")]
[assembly: GoPositionMap("syscall/forkpipe2.go", "forkpipe2.cs", "AAoaogAKHAAFFgALAoKUlIKCygAMGpSClLiCuKKowoKUgqaUlLQ=")]
[assembly: GoPositionMap("syscall/lsf_linux.go", "lsf_linux.cs", "AAockqiSqJKmgoKUgoKCgoKCgpQABxKygoKUgoKCooKUgpSUooKU2JKCgoKokoI=")]
[assembly: GoPositionMap("syscall/netlink_linux.go", "netlink_linux.cs", "AAwekqqiAAsUgoKCgoKCgoLGgoKCgoKCgqaCgrrCgoKUgoKClIKClIKClIKClJSCgoKCgoKClIKUgoKCgpSCgpSClILYAAsWooKCgoKUgoKUpoKCgoKUAAgY4oKUtLS0lIKCgoKUgoKUtoKCgpQ=")]
[assembly: GoPositionMap("syscall/rlimit.go", "rlimit.cs", "AA88AA4CgoKCAAcQlIK4gqaU")]
[assembly: GoPositionMap("syscall/rlimit_stub.go", "rlimit_stub.cs", "AAgU")]
[assembly: GoPositionMap("syscall/sockcmsg_linux.go", "sockcmsg_linux.cs", "AAoc0oKCgoKCgqzSgpSClIKUgg==")]
[assembly: GoPositionMap("syscall/sockcmsg_unix.go", "sockcmsg_unix.cs", "AA0ioqqixqIAChaigoKCgoKUgoKUpoKCgpSqwoKCgoKCgoKUqsKClIKUgoKClA==")]
[assembly: GoPositionMap("syscall/sockcmsg_unix_other.go", "sockcmsg_unix_other.cs", "AAockri26oLYgqaCyA==")]
[assembly: GoPositionMap("syscall/syscall.go", "syscall.cs", "ACFK8oKClKyygpSCggACEMCasoKClAAJEJLIksiSyJLKkpaSmA==")]
[assembly: GoPositionMap("syscall/syscall_linux.go", "syscall_linux.cs", "ABg8AAkGAAIsABQCrgAIAoKCgqzyAAwagoKs8oKCgqaCnLKmgqaCpoKmgoKUpoKCgqamgoKClNSsooIACRSCggAJFIKmgoKmgoKokoKCuKamgpSUlJSmgoKUgoKUpoKUuIKmrMaCgqaCgoKmlKqSpoKmgqaCqpKmgqqSgpSCgoKClKqSpoKmgqqSpoKqkqaCgpSqkoKUpoKClKamzrKCgoKmgsqCpqaygoKUgriCpoKCgpSCgpQAGG6UlJSUlIKClKaCgpSmgoKUpoKClMrSgoKClKaCAAw4ooKCgri4gpSCgriCpoKUpgAQGqKClIKCgoKCgoIADhaigoKCggAGigGCAAY8soKCAAgwgoKCggAEFIKCgoIABlaygoKCgoKClIKCgoKCgqaCgqaClIKCgpSClKiSrAAKDu6CgoKClIK4poKClIKCpqaCpoKm6oKCgoKClIKCgoKUuIKCgoKUgriCgoKClIKCgoKUpqaCpoLOgoKCgqaCgoKCpoKmsoKCgqaCpoKmlJSYkqaCpoKmgqaCgoKUqtaClIKClAACPAAbAoIAAkIAHyYAFAKClIKu8oKUgqwACAyigoKUgpTKooKClIKUyqKCgpSClMqigoKUgpTKooKClIKUyqKCgpSClMqigoKUgpTKooKClIKUABMq3IKmggACFPSqogACFAAJAoKClA==")]
[assembly: GoPositionMap("syscall/syscall_linux_amd64.go", "syscall_linux_amd64.cs", "AA54AC0CpoKmgqzEgoKClKaigoKClIKUrKKmgsaUtILGgsaC")]
[assembly: GoPositionMap("syscall/syscall_unix.go", "syscall_unix.cs", "AB9EkoKUAAwY4oK4goK4poKCgoLm0oK4goKCgoK4gpSCABomgoKCgqamgpSkpKSUpoKmggAKGqKUpKSklAAIDpSCgoKCpsaygoKClIKmgpSClKaygpSCgoKmlIKUgpSClKaygoKClIKmgpSClKaygpSCgpSClIKUACNsooKCggAEHrKCkoKUgoKCgqaygpKClIKCgoKCptKCgoKUgoKCgqbSgoKClIKCgoKCpuKClIKUpqKCAAQeooKCgoKCpqaigoKCgoKmpqKCgpSmooKClAAEIqKmgqKmogAIHoKmgoKClKaCprKClIKmsoKCgoKUpoKClA==")]
[assembly: GoPositionMap("syscall/time_nofake.go", "time_nofake.cs", "AAoWlA==")]
[assembly: GoPositionMap("syscall/timestruct.go", "timestruct.cs", "AAgUgJaSgoKCgpSogJaSgoKCgoKU")]
[assembly: GoPositionMap("syscall/zerrors_linux_amd64.go", "zerrors_linux_amd64.cs", "AMYKmhUAhQGMAg==")]
[assembly: GoPositionMap("syscall/zsyscall_linux_amd64.go", "zsyscall_linux_amd64.cs", "AAkYsoKCgpSigpSqsoKCgpSigpSqsoKCgpSigpSqsoKCgpSigpSqsoKCgpSCgoKUwoKUqsKCgoKUooKClKqyooKUqsKCgoKUgoKUlMKCgpSqsoKCgpSCgoKUwoKUqrKCgoKUooKUqrKCgoKUwoKUqsKCgpSUooKClAAFILKCgpSqsqKClKqygoKClKKClKqygoKClIKCgpSCgoKUAAgCgpSqsoKCgpSigpQABSCygoKClKKClKqygoKClKKClKqygoKUqsKCgoKUqrKCgpSqwoKCgpSqsqKClKqygoKUqrKCgpSqsoKClKqygoKClKKClKrCgoKClKqygoKUqrKCgpSqsoKClKrCgoKUlKKCgpSqwoKCgpSqsoKCqrKCgqrCgoKClKqyooKUqrKCgqrCgoKClIKCgpSCgpSU4oKClKrCgoKClKKCgpSqwoKCgpSqwoKCgpSqsoKClKrCgoKUlKKCgpSqwoKCgpSCgpSUwoKClKqygoKClKKClKqygoKClKKClKqywoKUqrKCgoKUgoKClMKClKqywoKUqsKCgpSUooKClKqygoKClIKCgpTCgpSqsoKClJSigpSqsoKClJSigpSqsoKClKrCgoKClKqyooKUqrKCgpSqsoKCgpSCgoKUgoKUlOKClKqSggAFHsKCgoKUqrKCgpSqwqKCgpSqsoKCAAUesoKCgpSigpSqsoKClKrCgoKUlKKCgpSqsoKClKrCooKClKqygoKUqrKCgpSUooKUqrKCgpSUooKUqrKCgpSUooKUqrKCgpSUooKUqrKCgpSqsoKClKqygoKUqrKCgpQACDKygoKUqrKCgqqygoKqsoKCqrKCgqrCgoKClKqygoKUqrKCgpSqsoKClKqygoKUqsKCgpSUooKClKrCgoKUlKKCgpSqsoKCgpSCgoKUwoKUqsKCgoKUAAUgwqKCgpSqsoKClKqygoKUqrKCgpSqwsKCgpQABSiygoKUqrKCgoKUooKUqrKigpSqwsKCgpSqsqKClKqyooKUAAUowqKCgpSqssKClKqyooKUqsKCgoKUqrKigpSqssKClKqywoKUqsKCgpSU4oKClKqygoKUlMKClKrCooKClKrCooKClKrCgoKClKrCgoKUlKKCgpSqsoKCgpTCgpSqsoKCgpTCgpSqsoKCgpTCgpQ=")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("syscall")]
public static partial class syscall_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct _C_int {}
    internal partial struct _C_long {}
    internal partial struct _C_long_long {}
    internal partial struct _C_short {}
    internal partial struct _Gid_t {}
    internal partial struct capData {}
    internal partial struct capHeader {}
    [GoValueClone("data")] internal partial struct caps {}
    internal partial struct cloneArgs {}
    [GoValueClone("name")] internal partial struct iflags {}
    internal partial struct mmapper {}
    [GoLocalName("sigInfo")] internal partial struct os_checkClonePidfd_sigInfo {}
    internal partial struct os_checkClonePidfd_sigInfo__ {}
    internal partial struct pollFd {}
    public partial interface Conn {}
    public partial interface RawConn {}
    public partial interface Sockaddr {}
    public partial struct Cmsghdr {}
    [GoValueClone("Name", "Pad_cgo_0")] public partial struct Dirent {}
    public partial struct EpollEvent {}
    public partial struct Errno {}
    [GoValueClone("Bits")] public partial struct FdSet {}
    [GoValueClone("Pad_cgo_0", "Pad_cgo_1")] public partial struct Flock_t {}
    [GoValueClone("X__val")] public partial struct Fsid {}
    [GoValueClone("Data")] public partial struct ICMPv6Filter {}
    [GoValueClone("Multiaddr", "Interface")] public partial struct IPMreq {}
    [GoValueClone("Multiaddr", "Address")] public partial struct IPMreqn {}
    [GoValueClone("Addr")] public partial struct IPv6MTUInfo {}
    [GoValueClone("Multiaddr")] public partial struct IPv6Mreq {}
    public partial struct IfAddrmsg {}
    public partial struct IfInfomsg {}
    [GoValueClone("Spec_dst", "Addr")] public partial struct Inet4Pktinfo {}
    [GoValueClone("Addr")] public partial struct Inet6Pktinfo {}
    [GoValueClone("Name")] public partial struct InotifyEvent {}
    public partial struct Iovec {}
    public partial struct Linger {}
    [GoValueClone("Pad_cgo_0", "Pad_cgo_1")] public partial struct Msghdr {}
    public partial struct NetlinkMessage {}
    public partial struct NetlinkRouteAttr {}
    public partial struct NetlinkRouteRequest {}
    public partial struct NlAttr {}
    public partial struct NlMsgerr {}
    public partial struct NlMsghdr {}
    public partial struct PtraceRegs {}
    [GoValueClone("Data")] public partial struct RawSockaddr {}
    [GoValueClone("Addr", "Pad")] public partial struct RawSockaddrAny {}
    [GoValueClone("Addr", "Zero")] public partial struct RawSockaddrInet4 {}
    [GoValueClone("Addr")] public partial struct RawSockaddrInet6 {}
    [GoValueClone("Addr")] public partial struct RawSockaddrLinklayer {}
    public partial struct RawSockaddrNetlink {}
    [GoValueClone("Path")] public partial struct RawSockaddrUnix {}
    public partial struct Rlimit {}
    public partial struct RtAttr {}
    public partial struct RtGenmsg {}
    public partial struct RtMsg {}
    public partial struct RtNexthop {}
    public partial struct Rusage {}
    public partial struct Signal {}
    public partial struct SockFilter {}
    [GoValueClone("Pad_cgo_0")] public partial struct SockFprog {}
    [GoValueClone("Addr", "raw")] public partial struct SockaddrInet4 {}
    [GoValueClone("Addr", "raw")] public partial struct SockaddrInet6 {}
    [GoValueClone("Addr", "raw")] public partial struct SockaddrLinklayer {}
    public partial struct SockaddrNetlink {}
    [GoValueClone("raw")] public partial struct SockaddrUnix {}
    public partial struct SocketControlMessage {}
    [GoValueClone("X__unused")] public partial struct Stat_t {}
    [GoValueClone("Fsid", "Spare")] public partial struct Statfs_t {}
    public partial struct SysProcAttr {}
    public partial struct SysProcIDMap {}
    [GoValueClone("Loads", "Pad_cgo_0", "X_f", "Pad_cgo_1")] public partial struct Sysinfo_t {}
    [GoValueClone("Pad_cgo_0")] public partial struct TCPInfo {}
    [GoValueClone("Cc", "Pad_cgo_0")] public partial struct Termios {}
    public partial struct Time_t {}
    public partial struct Timespec {}
    public partial struct Timeval {}
    [GoValueClone("Pad_cgo_0", "Pad_cgo_1", "Pad_cgo_2", "Pad_cgo_3")] public partial struct Timex {}
    public partial struct Tms {}
    public partial struct Ucred {}
    [GoValueClone("Pad_cgo_0", "Fname", "Fpack", "Pad_cgo_1")] public partial struct Ustat_t {}
    public partial struct Utimbuf {}
    [GoValueClone("Sysname", "Nodename", "Release", "Version", "Machine", "Domainname")] public partial struct Utsname {}
    public partial struct WaitStatus {}
    public partial struct _Socklen {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸbytealg() => builtin.initPackage(typeof(@internal.bytealg_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸoserror() => builtin.initPackage(typeof(@internal.oserror_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸrace() => builtin.initPackage(typeof(@internal.race_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸstrconv() => builtin.initPackage(typeof(@internal.strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
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
[GoMethodHost(typeof(syscall_package.Cmsghdr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛCmsghdr;
[GoMethodHost(typeof(syscall_package.Errno)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛErrno;
[GoMethodHost(typeof(syscall_package.Iovec)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛIovec;
[GoMethodHost(typeof(syscall_package.Msghdr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛMsghdr;
[GoMethodHost(typeof(syscall_package.NetlinkRouteRequest)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛNetlinkRouteRequest;
[GoMethodHost(typeof(syscall_package.PtraceRegs)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛPtraceRegs;
[GoMethodHost(typeof(syscall_package.Signal)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛSignal;
[GoMethodHost(typeof(syscall_package.SockaddrInet4)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛSockaddrInet4;
[GoMethodHost(typeof(syscall_package.SockaddrInet6)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛSockaddrInet6;
[GoMethodHost(typeof(syscall_package.SockaddrLinklayer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛSockaddrLinklayer;
[GoMethodHost(typeof(syscall_package.SockaddrNetlink)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛSockaddrNetlink;
[GoMethodHost(typeof(syscall_package.SockaddrUnix)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛSockaddrUnix;
[GoMethodHost(typeof(syscall_package.Timespec)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛTimespec;
[GoMethodHost(typeof(syscall_package.Timeval)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛTimeval;
[GoMethodHost(typeof(syscall_package.WaitStatus)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛWaitStatus;
[GoMethodHost(typeof(syscall_package.mmapper)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛmmapper;
// </MethodHolders>
