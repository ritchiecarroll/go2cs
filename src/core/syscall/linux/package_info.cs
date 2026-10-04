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
global using runtimeꓸError = go.runtime_package.ΔError;
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
[assembly: GoTypeAlias("Signal", "ΔSignal")]
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
[assembly: GoImplicitConv<SysProcAttr, ж<SysProcAttr>>(Indirect = true)]
[assembly: GoImplicitConv<WaitStatus, ΔSignal>(Inverted = true, ValueType = "uint32")]
[assembly: GoImplicitConv<_C_int, WaitStatus>(Inverted = true, ValueType = "int32")]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("syscall/dirent.go", "dirent.cs", "AA4ikoKUgpSmgpSkpKSkyIKUpKSkpAAEEOKCgoKCgpSCgoKCuJKUgoKClIKCgoK4gpSCgpQ=")]
[assembly: go.GoPositionMap("syscall/env_unix.go", "env_unix.cs", "ABE6lrSSgoKCgoKAgtyk3KKEgoSAgoKkgtbCgoKWgoSCgpSCgoKm5qKCgpSCgriCgoK6goSCgoKUgpSCgtaihIKEgpSCoqSigoKCgoKCpg==")]
[assembly: go.GoPositionMap("syscall/exec_linux.go", "exec_linux.cs", "AIcB6AGSzKKSAAEaABAGgoKUgqiCgpaCgqaCgIK2opYAFCqQppAAETgAEQQAAyYAMCKEhIKCloKChIKUlKi6goKCgpSUqIKAgoK4goKUgqaCuIKClILMgoKCpoKUlKbuvIKCgrqCgIKkooKUgoKUgoK6goKCupSCgqiCgoKEqKKCzJaCgoKWgoKAoqSigpSAgqaAoqSigpSAgriCgoCipKKClICCAAoWgsKCzIKigrqAgoKCgpSCgoKmgoKUgoK4poSAwqbGgpaAwqaigoLMgqKCuoKCgsyCgoKCgt6CgoKUgpSCgpKUgoKUgrqCgoKUpoKClLiCggAGEIKogoKCuoKCgroAChaigv6CgoK66oSygriCgoKUqJKCgpaAgoKmgIKmrsKCgoKWgoKUloCCgqaqooKCgIK4lICCpIKAgriokoKCAAYQ4pKCgpaogoLKgoKCpoKWuoSCgoKCpoKWAAUWAAsCgpSUlO6Wgg==")]
[assembly: go.GoPositionMap("syscall/forkpipe2.go", "forkpipe2.cs", "AAoaogAGHAAFFgALAoKElIKCqgALGoSChJiCqKKowoKEgpaElLQ=")]
[assembly: go.GoPositionMap("syscall/lsf_linux.go", "lsf_linux.cs", "AAockqiSqJKmgoKUgoKCgoKCgpQABxKygoKUgoKCooKUgpSUooKU2JKCgoKokoI=")]
[assembly: go.GoPositionMap("syscall/netlink_linux.go", "netlink_linux.cs", "AAsekqqiAAkUgoKCgoKCgoKmgoKCgoKCgqaCgrrCgoKUgoKAgqSCgIKkgoKUgoKUhIKCgoKCgoKUgpSCgoKClIKClIKUgtgACxaigoKCgpSCgpSmgoKCgpQACBjSgpSkpKSkgoKCgpSCgpSmgoKClA==")]
[assembly: go.GoPositionMap("syscall/rlimit.go", "rlimit.cs", "AA88AA4CgoCCggAHEISCyIKmlA==")]
[assembly: go.GoPositionMap("syscall/rlimit_stub.go", "rlimit_stub.cs", "AAgU")]
[assembly: go.GoPositionMap("syscall/sockcmsg_linux.go", "sockcmsg_linux.cs", "AAoc0oKCgoKCgqzSgpSClIKUkg==")]
[assembly: go.GoPositionMap("syscall/sockcmsg_unix.go", "sockcmsg_unix.cs", "AAwioqqipqIACBaigoKCgoKUgoKUpoKCgpSqwoKCgoKCgoKUqsKClIKUgoKClA==")]
[assembly: go.GoPositionMap("syscall/sockcmsg_unix_other.go", "sockcmsg_unix_other.cs", "AAockqiWqsK4kpaSuA==")]
[assembly: go.GoPositionMap("syscall/syscall.go", "syscall.cs", "ACBK8oKClKyygpSCggACENCqsoKClAAHEJKokqiSqJKqopai")]
[assembly: go.GoPositionMap("syscall/syscall_linux.go", "syscall_linux.cs", "ABU6AAkGAAIsABQCrgAIAoKCgqzyAAwagoKs8oKCgqaSnLKmgqaCpoKmgoKUpoKCgpaCgqamgoKChMSsooIACBSCgIIACBSCloKAgqaCgpiSgoK4ppaClJSUlJaCgpSCgpSWgpSogpasxoKCpoKSgqaUqpKmgqaCpoKqkqaCqpKClIKCgoKUqpKmgqaCqpKmgqqSpoKClKqSgpSmgoKUpqbOsoKCgqaCyoKWprKCgpSCqIKWgoKClIKClAAWboCkgKSApICkgKSCgpSmgoKUpoKClKaCgpSq0oKCgpSmggAGOKKCgoKUgpSCgqaCgpSUlJYADBqigpSCgoKCgoKCAAoWooKCgoIABIoBggAGPLKSggAIMIKCkoIABBSCgpKCAAZWsoKCgoKCgpSCgoKCgoKmgoKmgpSCgoCCpIKUqJKsAAgO3oKCgoKUgqimgoKUgoKWpoKmgqbagoKCgoKUgoKCgpSogoKCgpSCqIKCgoKUgoKCgpSWpoKmgs6igoKCpqKCgoKmgqaygoKCpoKmgqaApICkgKiSpoKmgqaCpoKCgpSq1oKUgoKUAAI8ABsCggACQgAfJgAUAoKUgq7ygpSCrAAIDKKCgIK0gIKkyqKCgIK0gIKkyqKCgIK0gIKkyqKCgIK0gIKkyqKCgIK0gIKkyqKCgIK0gIKkyqKCgIK0gIKkyqKCgIK0gIKkABMq3IKmggACFPSqogACFAAJAoKClA==")]
[assembly: go.GoPositionMap("syscall/syscall_linux_amd64.go", "syscall_linux_amd64.cs", "AAx4AC0CpoKmgqzEgoKClKaigoKClIKUrKKmgqaApICkgqaCpoI=")]
[assembly: go.GoPositionMap("syscall/syscall_unix.go", "syscall_unix.cs", "ABtEkoCCpAAKGOKCqIKCqJaCgoKC5tKCqIKCgoKCqICCpIIACCaCgoKCpqaClKSkpKSmgqaCAAoaopSkpKSkzqSCgoKCpqaygoKClIKmgpSClKaygpSCgoKmlIKUgpSClKaygoKClIKmgpSClKaygpSCgpSClIKUACJsooKSggAEHrKCkoCCpIKCgoKmsoKSgIKkgoKCgoKm0oKCgpSCgoKCptKCgoKUgoKCgoKm4oKUgpSmooIABB6igoKUpqKCgpSmooKClKaigoKUAAQioqaCoqaiAAgegqaCgoKUpqKmsoKUgqaygoKCgpSmgoKU")]
[assembly: go.GoPositionMap("syscall/time_nofake.go", "time_nofake.cs", "AAoWlA==")]
[assembly: go.GoPositionMap("syscall/timestruct.go", "timestruct.cs", "AAgUkKaSgoKCgpSokKaSgoKCgoKU")]
[assembly: go.GoPositionMap("syscall/zerrors_linux_amd64.go", "zerrors_linux_amd64.cs", "AOwLmhUAhQGMAg==")]
[assembly: go.GoPositionMap("syscall/zsyscall_linux_amd64.go", "zsyscall_linux_amd64.cs", "AAkYsoKCgpSigpSqsoKCgpSigpSqsoKCgpSigpSqsoKCgpSigpSqsoKCgpSCgoKUwoKUqsKCgoKUooKClKqyooKUqsKCgoKUgoKUlMKCgpSqsoKCgpSCgoKUwoKUqrKCgoKUooKUqrKCgoKUwoKUqsKCgpSUooKClAAFILKCgpSqsqKClKqygoKClKKClKqygoKClIKCgpSCgoKUAAgCgpSqsoKCgpSigpQABSCygoKClKKClKqygoKClKKClKqygoKUqsKCgoKUqrKCgpSqwoKCgpSqsqKClKqygoKUqrKCgpSqsoKClKqygoKClKKClKrCgoKClKqygoKUqrKCgpSqsoKClKrCgoKUlKKCgpSqwoKCgpSqsoKCqrKCgqrCgoKClKqyooKUqrKCgqrCgoKClIKCgpSCgpSU4oKClKrCgoKClKKCgpSqwoKCgpSqwoKCgpSqsoKClKrCgoKUlKKCgpSqwoKCgpSCgpSUwoKClKqygoKClKKClKqygoKClKKClKqywoKUqrKCgoKUgoKClMKClKqywoKUqsKCgpSUooKClKqygoKClIKCgpTCgpSqsoKClJSigpSqsoKClJSigpSqsoKClKrCgoKClKqyooKUqrKCgpSqsoKCgpSCgoKUgoKUlOKClKqSggAFHsKCgoKUqrKCgpSqwqKCgpSqsoKCAAUesoKCgpSigpSqsoKClKrCgoKUlKKCgpSqsoKClKrCooKClKqygoKUqrKCgpSUooKUqrKCgpSUooKUqrKCgpSUooKUqrKCgpSUooKUqrKCgpSqsoKClKqygoKUqrKCgpQACDKygoKUqrKCgqqygoKqsoKCqrKCgqrCgoKClKqygoKUqrKCgpSqsoKClKqygoKUqsKCgpSUooKClKrCgoKUlKKCgpSqsoKCgpSCgoKUwoKUqsKCgoKUAAUgwqKCgpSqsoKClKqygoKUqrKCgpSqwsKCgpQABSiygoKUqrKCgoKUooKUqrKigpSqwsKCgpSqsqKClKqyooKUAAUowqKCgpSqssKClKqyooKUqsKCgoKUqrKigpSqssKClKqywoKUqsKCgpSU4oKClKqygoKUlMKClKrCooKClKrCooKClKrCgoKClKrCgoKUlKKCgpSqsoKCgpTCgpSqsoKCgpTCgpSqsoKCgpTCgpQ=")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("syscall")]
public static partial class syscall_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
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
    public partial struct ΔSignal {}
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
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    // </ImportInitializers>
}
