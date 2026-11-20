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
using syscall = go.syscall_package;
// </ImportedTypeAliases>

using go;
using static go.golang.org.x.sys.unix_package;

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
[assembly: GoDynamicTypeLift("7374727563747b6e756d2073797363616c6c2e4572726e6f3b206e616d6520737472696e673b206465736320737472696e677d", "errorListᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e756d2073797363616c6c2e5369676e616c3b206e616d6520737472696e673b206465736320737472696e677d", "signalListᴛ1")]
[assembly: GoTypeAlias("Errno", "go.syscall_package.Errno")]
[assembly: GoTypeAlias("Signal", "go.syscall_package.Signal")]
[assembly: GoTypeAlias("SysProcAttr", "go.syscall_package.SysProcAttr")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<SockaddrALG, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrCAN, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrCANJ1939, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrHCI, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrIUCV, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrInet4, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrInet6, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrL2, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrL2TPIP, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrL2TPIP6, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrLinklayer, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrNFC, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrNFCLLCP, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrNetlink, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrPPPoE, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrRFCOMM, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrTIPC, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrUnix, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrVM, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrXDP, Sockaddr>(Pointer = true)]
[assembly: GoImplement<TIPCServiceName, TIPCAddr>(Pointer = true)]
[assembly: GoImplement<TIPCServiceRange, TIPCAddr>(Pointer = true)]
[assembly: GoImplement<TIPCSocketAddr, TIPCAddr>(Pointer = true)]
// </InterfaceImplementations>

// The struct types whose arrays this package holds inline (the go.arrays family), as the converter
// admitted them: a `GoInlineArrayType` record per named type, a `GoInlineArrayKeys` record with the
// count and a digest of every admitted type (anonymous ones included), a `GoInlineArrayAnonymous`
// record per admitted anonymous type, a `GoInlineArraySeed` record per -inline-arrays seed, and a
// `GoInlineArrays` record naming the rule when it is not the default. A -tests conversion applies the
// same rule and seeds rather than deciding again, and refuses a production set that admits differently.
// The section exists only while there is a record to hold.
// <InlineArrays>
[assembly: go.golib.GoInlineArrayKeys(39, "bb8d34185e027d04")]
[assembly: go.golib.GoInlineArrayType("EthtoolDrvinfo")]
[assembly: go.golib.GoInlineArrayType("EthtoolTsInfo")]
[assembly: go.golib.GoInlineArrayType("Flock_t")]
[assembly: go.golib.GoInlineArrayType("Fsid")]
[assembly: go.golib.GoInlineArrayType("ICMPv6Filter")]
[assembly: go.golib.GoInlineArrayType("IPMreq")]
[assembly: go.golib.GoInlineArrayType("IPMreqn")]
[assembly: go.golib.GoInlineArrayType("IPv6Mreq")]
[assembly: go.golib.GoInlineArrayType("Inet4Pktinfo")]
[assembly: go.golib.GoInlineArrayType("Inet6Pktinfo")]
[assembly: go.golib.GoInlineArrayType("LoopConfig")]
[assembly: go.golib.GoInlineArrayType("LoopInfo64")]
[assembly: go.golib.GoInlineArrayType("PacketMreq")]
[assembly: go.golib.GoInlineArrayType("PtpClockCaps")]
[assembly: go.golib.GoInlineArrayType("PtpClockTime")]
[assembly: go.golib.GoInlineArrayType("PtpExttsRequest")]
[assembly: go.golib.GoInlineArrayType("PtpPinDesc")]
[assembly: go.golib.GoInlineArrayType("PtpSysOffsetExtended")]
[assembly: go.golib.GoInlineArrayType("PtpSysOffsetPrecise")]
[assembly: go.golib.GoInlineArrayType("RawSockaddr")]
[assembly: go.golib.GoInlineArrayType("Siginfo")]
[assembly: go.golib.GoInlineArrayType("Sigset_t")]
[assembly: go.golib.GoInlineArrayType("SockaddrStorage")]
[assembly: go.golib.GoInlineArrayType("Stat_t")]
[assembly: go.golib.GoInlineArrayType("Statfs_t")]
[assembly: go.golib.GoInlineArrayType("StatxTimestamp")]
[assembly: go.golib.GoInlineArrayType("Statx_t")]
[assembly: go.golib.GoInlineArrayType("Sysinfo_t")]
[assembly: go.golib.GoInlineArrayType("SysvIpcPerm")]
[assembly: go.golib.GoInlineArrayType("SysvShmDesc")]
[assembly: go.golib.GoInlineArrayType("TCPInfo")]
[assembly: go.golib.GoInlineArrayType("TCPMD5Sig")]
[assembly: go.golib.GoInlineArrayType("Termios")]
[assembly: go.golib.GoInlineArrayType("Timespec")]
[assembly: go.golib.GoInlineArrayType("Timeval")]
[assembly: go.golib.GoInlineArrayType("Timex")]
[assembly: go.golib.GoInlineArrayType("Ustat_t")]
[assembly: go.golib.GoInlineArrayType("Utsname")]
[assembly: go.golib.GoInlineArrayType("WatchdogInfo")]
// </InlineArrays>

// <ImplicitConversions>
[assembly: GoImplicitConv<WaitStatus, syscall.Signal>(Inverted = true, ValueType = "uint32")]
[assembly: GoImplicitConv<_C_int, WaitStatus>(Inverted = true, ValueType = "int32")]
// </ImplicitConversions>

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(Cmsghdr))]
[assembly: GoFieldAccess(typeof(Ifreq))]
[assembly: GoFieldAccess(typeof(RawFileDedupeRange))]
[assembly: GoFieldAccess(typeof(RawFileDedupeRangeInfo))]
[assembly: GoFieldAccess(typeof(RawSockaddrCAN))]
[assembly: GoFieldAccess(typeof(RawSockaddrInet4))]
[assembly: GoFieldAccess(typeof(RawSockaddrInet6))]
[assembly: GoFieldAccess(typeof(RawSockaddrL2))]
[assembly: GoFieldAccess(typeof(RawSockaddrL2TPIP))]
[assembly: GoFieldAccess(typeof(RawSockaddrL2TPIP6))]
[assembly: GoFieldAccess(typeof(RawSockaddrLinklayer))]
[assembly: GoFieldAccess(typeof(RawSockaddrNFC))]
[assembly: GoFieldAccess(typeof(RawSockaddrNFCLLCP))]
[assembly: GoFieldAccess(typeof(RawSockaddrNetlink))]
[assembly: GoFieldAccess(typeof(RawSockaddrRFCOMM))]
[assembly: GoFieldAccess(typeof(RawSockaddrTIPC))]
[assembly: GoFieldAccess(typeof(RawSockaddrUnix))]
[assembly: GoFieldAccess(typeof(RawSockaddrVM))]
[assembly: GoFieldAccess(typeof(RawSockaddrXDP))]
[assembly: GoFieldAccess(typeof(SockaddrALG))]
[assembly: GoFieldAccess(typeof(SockaddrCAN))]
[assembly: GoFieldAccess(typeof(SockaddrCANJ1939))]
[assembly: GoFieldAccess(typeof(SockaddrHCI))]
[assembly: GoFieldAccess(typeof(SockaddrIUCV))]
[assembly: GoFieldAccess(typeof(SockaddrInet4))]
[assembly: GoFieldAccess(typeof(SockaddrInet6))]
[assembly: GoFieldAccess(typeof(SockaddrL2))]
[assembly: GoFieldAccess(typeof(SockaddrL2TPIP))]
[assembly: GoFieldAccess(typeof(SockaddrL2TPIP6))]
[assembly: GoFieldAccess(typeof(SockaddrLinklayer))]
[assembly: GoFieldAccess(typeof(SockaddrNFC))]
[assembly: GoFieldAccess(typeof(SockaddrNFCLLCP))]
[assembly: GoFieldAccess(typeof(SockaddrNetlink))]
[assembly: GoFieldAccess(typeof(SockaddrPPPoE))]
[assembly: GoFieldAccess(typeof(SockaddrRFCOMM))]
[assembly: GoFieldAccess(typeof(SockaddrTIPC))]
[assembly: GoFieldAccess(typeof(SockaddrUnix))]
[assembly: GoFieldAccess(typeof(SockaddrVM))]
[assembly: GoFieldAccess(typeof(SockaddrXDP))]
[assembly: GoFieldAccess(typeof(fileHandle))]
[assembly: GoFieldAccess(typeof(mremapMmapper))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/affinity_linux.go", "affinity_linux.cs", "ABU2goKClKqCqoLIgqyCxoKmgqaCgriCgoK4goKCuIKCgpSmgoKClMiCqIKogqiCyoKCgpTIgqyCqIKogqiCqIKmgqaCgpQABBCCqoI=")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/auxv.go", "auxv.cs", "AAsenIKClIKmgqaCgg==")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/dev_linux.go", "dev_linux.cs", "AAgqgoKCqIKCgqqCgoKCgg==")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/dirent.go", "dirent.cs", "AAoYgoKUgpSmgpSkgqSCpIKm2IKUpIKkgqSCpgAFEKKCgoKCgpSCgoKClIKUgoKClIKCgoKogpSCgpQ=")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/env_unix.go", "env_unix.cs", "AAoagqaCpoKmgqaC")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/fcntl.go", "fcntl.cs", "AA0egoKCgpSogqiCgoKU")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/fdset.go", "fdset.cs", "AAwUgqiCqIKogg==")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/ifreq_linux.go", "ifreq_linux.cs", "ABBIhIKmgpTMggACFIKChKasgoKmgpSCAAQQqIKogoKogqiCgqqCAAwoog==")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/ioctl_linux.go", "ioctl_linux.cs", "AAsYgoKClKaCgoKmgoKCpoKmgoKCpoKqgoKCpoKUgqqCgoKmgpSCqoKCgqaClIKqgoKClIKsloKCgqqCgoKsgoKCqoKCgqqCqoKqgqyCgoKshKyCrIIAEi6ihIKCgoKClIKGgoKCgqaWgoaCgoKCpqaCpoKCgqaCgoKmgoKCpoKCgqqGroasgoKCpqqiqKKqgoKClKqCqIKsgg==")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/ioctl_unsigned.go", "ioctl_unsigned.cs", "AAoggq6CgqqCgqyGrIQAAhCCgoKmgoKCpoKCgg==")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/mremap.go", "mremap.cs", "ABEgAAoS4oKmgoKCgoKUgoKUgoKClIIACAaCpqKC")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/pagesize_unix.go", "pagesize_unix.cs", "AAoagg==")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/race0.go", "race0.cs", "AAwepqam")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/readdirent_getdents.go", "readdirent_getdents.cs", "AAgUgg==")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/readv_unix.go", "readv_unix.cs", "AAwoooKCgoKUlJS6goKUgoKCgs6CgpSCgoKCpoK4soKCgoKmsoKCgoKmsoKCgpSCgqaygoKClIKC")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/sockcmsg_linux.go", "sockcmsg_linux.cs", "AAocooKCgoKCgqyigpSClIKoooKCgoKCgqiigoKCgoKCrKKUgoKCgoKmgoKCgoKCpg==")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/sockcmsg_unix.go", "sockcmsg_unix.cs", "AAwigqqCxoIACRaCgoKCgoKUgoKUrLKCgpSClKaCgoKUqqKCgoKCgoKClKqigpSClIKCgpQ=")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/sockcmsg_unix_other.go", "sockcmsg_unix_other.cs", "AAocgpimuoLIgpaCyrY=")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/syscall.go", "syscall.cs", "AAxIgoKUgoKsgoKClKqCgpSsooKUgqiCgqY=")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/syscall_linux.go", "syscall_linux.cs", "ABE2gqaCpoKmgqaCgpSsooKUgoKUrIaCgoaCgoKmlKaCAAIkgqaCpoKmgqqCqoKmgqqCgpSCgoKClKqCgpSmgoKCgpSqgqaCpoKqgqaCqoKCgoKUlIKUgoKCgoKUqoKmgoKUgpSmgoKUgpSmhs6igoKCloKagqamsoKClIKogqaCgoKUgoKUpoKCpoKClAAMLJSUlJSUgoKUpoKClKaCgpSmgoKUysKCgoKUqoKmgsaigpSCgoKCgsaigpSCgoKCgoLGooKCgpSCgpaCgpSEhKYAEByigpSCgoKCgoKCAA4YooKCgoIADBaigoKCAA4aooKCgoKClIKCgoIADEqigoKCAA02ooKUgoKCgpSCgpQADh6igpSCgoKClIKClIIADpQBpIKUgqaCgpSClAANJqKCgoKUAA4WooKCgoKUAA8mooKUgqYAABKGgoKCggARPKKCgqa0ooKCprSigoKmtKKClIKCgoIADBSigoKCAA0WooKCgoIADBSiiIKCgpSClIKUgpQADRSigoKCggAQGqKCgoKCgoKClIKC1piilIKCgoKCgqaCgoKCgoKCgqaCgowAAhCCgpSCpoKCppSCgoKCpIKCgoKC2IKCppSCgoKCgqSCgoKCgoLYgsqkgoKWlILcpIK41oLcpIKClLiCgoKmpJS8lLS0tKaklIKUgoKmuKaCgqaUlKaCgpSCgpSCgqSmgoKUgoKU1oKClJSCyqSCgpQABxCk1qbCgpKCgpSCgoKUpsKCkoKClIKUgoKClKaygpKClKaCgoKCpoKCgoKmgoKCggACEoKCgoKCAAISgoKCgoIAAhKCgoKCgqqCgoKCgoKClIKmpoKCgoKmgoKCgqaCpoKqgqaCgoKUpoKmgqaCgpSmggACJoqChIKCqISoAAMSgoKClAACEIKCAAIQggACEIIAAhSCAAIcggACKoKClKzSgoKCgoKCgoKCloKCgoKmgpSCgpSClIKCprKCgoKCgoKCgoKCgpaCgoKCpoKUgoKUgpSClKiCrK6egoKCgpSCqIaCgpSCgqamgqaCpoKmqoKCgoKClIKCgoKUqIKCgoKUgqiCgoKClIKCgoKUpqaCpoKmggAEEIKCgoKmgoKCgqaCprKCgoKmgqaCppSUlJSYgqaCpoKmgoKClKqmgpSCgpQAAhKCpoKClAACOIIAAjaigoKUAAIQggACEqKCgpQAAhKCgpQAAhSigoKUrKKCgpQAAhKCroKugqy2goKCgpSmqqKCroKCgoKUlIKClIKClAACVLSGrIKCgpSmgqaCpoKmgqaCpoKsgqyCpoKCpoKCpoIAAjyCgqaCgqaygoKCgoKmgoKmsoKCgpSCgoIABiyCgoKmgoKmpoKCgqamgoKClKyCgqaCAAISgqaCgqaCgqaCgoKIpqaChJSElKaCgpSCgpSmgpS4gqYACzCCgoKCgoKCxoKCgoKClMrChoKCgoKCgoKChJSCgpSClLyCqoKCgoKUAAc4ggALJoKCgqauooKCpqqihJSsgqKCpoKigqqogoKCqoKCmpSC3AACEKKClIKqgoKClAACEIKmgg==", "", "", "1013=Uint16/1/1/1;2042=Nanoseconds/1/2/1,NsecToTimeval/1/2/1,Nanoseconds/2/2/2,NsecToTimeval/2/2/2")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/syscall_linux_amd64.go", "syscall_linux_amd64.cs", "AAsyggACFKKCgpQAAhKEAAI0goKClKaigoKClIKUrIKmgsaUtILGgqaCxoLGgsqCgoiU")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/syscall_linux_amd64_gc.go", "syscall_linux_amd64_gc.cs", "AAoY")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/syscall_linux_gc.go", "syscall_linux_gc.cs", "AAgUyA==")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/syscall_unix.go", "syscall_unix.cs", "AB5MgpSkpKSkqIKGgpSogoaClKyCgoKCpqiCgoKUAAsY4oKogoKoloKCgoLm0oKogoKCgoKogpSCAAgGgqaCpqKCpoKmsoKCgpSCpqaygpSCgpSmsoKCgpSCpqaygpSCgpQAG0KigoKUpqKCgpSmsoKSgpSmooKCgqaigoKCprKCgqaCgoKCpoKCgoKmgoKCgqaCgoKCpoKCgoKmgoKCgqaigoKCpsKCkoKUgpQAAiDigoKClIKCloKUrOKCgoKClKaCgpSClKyiggACNqKCgoKUgoKCgoKmrKKCgoKClKaCgoKCgqamgqaigoKCgoKmpqKmgqKmoqaCpoKmgqaCpoKCgpSmgqaiprKClIKmsoKCgoKU2pSigoKUgpSClJSCAAIQggACEIKClIKUuKiCgoKmqIY=", "54-56:1;65-67:1;78-83:1", "", "611=TimevalToNsec/1/2/1,NsecToTimespec/1/2/1,TimevalToNsec/2/2/2,NsecToTimespec/2/2/2")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/syscall_unix_gc.go", "syscall_unix_gc.cs", "AAoWsrKy")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/sysvshm_linux.go", "sysvshm_linux.cs", "AAoagoSm")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/sysvshm_unix.go", "sysvshm_unix.cs", "AAoagoKCqJSCiIKogqyCgqaqgg==")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/timestruct.go", "timestruct.cs", "AAsYloKCgoKClK6CgoKagpSoloKCgoKCgpTIgsiCyILIgg==")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/vgetrandom_linux.go", "vgetrandom_linux.cs", "AAoa")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/zerrors_linux_amd64.go", "zerrors_linux_amd64.cs", "ALoFiAsAjAGYAg==")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/zptrace_x86_linux.go", "zptrace_x86_linux.cs", "ABo+gqiCACBGgqiC")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/zsyscall_linux.go", "zsyscall_linux.cs", "AAccsoKCgpSqooKCgpSCgpSqooKCgpSCgpSqooKClKqigoKUqqKCgoKUgoKClIKClKqygoKClIKCgpSqsoKCgpSCgoKUqqKCgpSqsoKCgpSqsoKCgpSCgpSUgoKClKqigoKClIKCgpSCgpSqooKCgpSCgpSqooKCgpSCgpSqsoKClJSCgoKUqrKCgoKUqqKCgpSqsoKCgpSqsoKClJSCgoKUqrKCgoKUgoKClKqygoKClIKCgpSCgoKUqqKCgpSUgoKUqrKCgpSUgoKClKqigoKClIKCgpSCgpSqooKClKqigoKUqqKCgpSqooKCgpSCgpSqooKCgpSCgoKUgoKClIKClKqigoKClIKClKqigoKClIKClKqygoKClIKCgpSCgpSUgoKClKqygoKClKqigoKUqqKCgpSqooKCgpSCgpSqooKCgpSCgpSqsoKCgpSqooKClKqigoKUqqKCgpSqooKClKqigoKUqqKCgpSqsoKCgpSqooKCgpSCgpSqsoKCgpSqooKClKqygoKClKqigoKUqrKCgpSUgoKClKqygoKClKqCgqqigoKUqqKCgpSqooKCgpSCgpSqooKClKqygoKClIKClJSCgoKUqqKCgoKUgoKUqrKCgpSUgoKClKqigoKUqqKCgoKUgoKUqqKCgoKUgoKUlIKClKqigoKUqrKCgoKUqrKCgoKUgoKClKqygoKClIKCgpSqooKClKqygoKUlIKCgpSqsoKCgpSqooKCqqKCgqqygoKClKqigoKUqrKCgoKUqqKCgqqygoKClIKCgpSCgpSUgoKClKqigoKUlIKCgpSCgpSqsoKCgpSCgoKUqrKCgoKUqrKCgoKUqqKCgpSqsoKClJSCgoKUqrKCgoKUgoKClIKClJSCgoKUqrKCgpSUgoKClKqygoKClIKClJSCgoKUqrKCgoKUgoKUlIKCgpSqooKCgpSCgoKUgoKUqqKCgoKUgoKClIKClJSCgpSqsoKCgpSCgoKUqqKCgoKUgoKUqqKCgoKUgoKUqqKCgoKUgoKClIKClKqigoKUqrKCgoKUgoKClKqygoKClKqigoKClIKCgpSCgpSqooKClKqigoKUqrKCgoKUqrKCgpSUgoKClKqigoKClIKCgpSCgpSqooKCgpSCgoKUgoKUqrKCgoKUgoKClIKCgpSCgoKUqqKCgpSUgoKUqqKCgpSUgoKUqqKCgpSqsoKCgpSqooKClKqigoKUqqKCgpSqooKCgpSCgoKUgoKUlIKClKqygoKClKqigoKClIKClKqCgqqigoKUqqKCgpSqsoKCgpSqooKClKqigoKUqqKCgpSqsoKCgpSqooKCqqKCgpSqooKCgpSCgpSqooKClKqygoKUlIKCgpSqooKClKqygoKUlIKCgpSqsoKClJSCgoKUqrKCgpSUgoKClKqygoKUlIKCgpSqsoKClJSCgoKUqrKCgpSUgoKClKqigoKUqrKCgoKUqqKCgpSUgoKUqqKCgpSUgoKUqqKCgpSUgoKUqqKCgpSqooKClJSCgpSqooKClJSCgpSqooKClKqigoKClIKClKqigoKClIKClKqigoKClIKClKqygoKClKqygoKUlIKClJSCgoKUqrKCgpSUgoKUlIKCgpSqsoKCgpSqsoKCgpSqooKClKqygoKClKqygoKClKqigoKUqrKCgoKUqqKCgpSqooKClKqigoKUqoKCqoKCqqKCgpSqooKClKqigoKUqqKCgpSUgoKUqqKCgpQ=")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/unix/zsyscall_linux_amd64.go", "zsyscall_linux_amd64.cs", "AAkiooKClKqigoKUqrKCgoKUqqKCgpSqooKClKqigoKUqqKCgoKUgoKUqqKCgpSqooKClKqigoKqooKCqqKCgqqigoKUqqKCgqqigoKUqqKCgpSqooKCgpSCgpSqooKClKqygoKClKqigoKUqrKCgpSUgoKClKqygoKUlIKCgpSqooKCgpSCgoKUgoKUqrKCgoKUqrKCgoKUqrKCgoKUqrKCgoKUqqKCgpSqsoKCgpSqooKCgpSCgpSqooKClKqigoKClIKClKqigoKUqrKCgoKUqqKCgpSqooKClKqygoKClKqigoKUqqKCgpSqooKClKqygoKClKqigoKUqqKCgpSqooKClKqygoKUlIKCgpSqooKClJSCgpSqsoKCgpSqsoKCgpSqsoKCgpSqooKCgpSCgpSqooKCgpSCgpSqooKCgpSCgpSqooKCgpSCgpSqsoKCgpQ=")]
// </GoSourcePositionMaps>

namespace go.golang.org.x.sys;

[GoPackage("unix", ImportPath = "golang.org/x/sys/unix")]
public static partial class unix_package
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
    internal partial struct errorListᴛ1 {}
    internal partial struct fileHandle {}
    [GoValueClone("Ifrn", "Ifru")] internal partial struct ifreq {}
    [GoValueClone("name")] internal partial struct ifreqData {}
    internal partial struct mmapper {}
    internal partial struct mremapMmapper {}
    internal partial struct signalListᴛ1 {}
    internal partial struct sigset_argpack {}
    public partial interface Sockaddr {}
    public partial interface TIPCAddr {}
    public partial struct BlkpgIoctlArg {}
    [GoValueClone("Devname", "Volname")] public partial struct BlkpgPartition {}
    public partial struct CANBitTiming {}
    [GoValueClone("Name")] public partial struct CANBitTimingConst {}
    public partial struct CANBusErrorCounters {}
    public partial struct CANClock {}
    public partial struct CANCtrlMode {}
    public partial struct CANDeviceStats {}
    public partial struct CGroupStats {}
    public partial struct CPUSet {}
    public partial struct CPUSetDynamic {}
    public partial struct CachestatRange {}
    public partial struct Cachestat_t {}
    public partial struct CanFilter {}
    public partial struct CapUserData {}
    public partial struct CapUserHeader {}
    public partial struct Cmsghdr {}
    [GoValueClone("Type", "Geniv")] public partial struct CryptoReportAEAD {}
    [GoValueClone("Type")] public partial struct CryptoReportAKCipher {}
    [GoValueClone("Type")] public partial struct CryptoReportAcomp {}
    [GoValueClone("Type", "Geniv")] public partial struct CryptoReportBlkCipher {}
    [GoValueClone("Type")] public partial struct CryptoReportCipher {}
    [GoValueClone("Type")] public partial struct CryptoReportComp {}
    [GoValueClone("Type")] public partial struct CryptoReportHash {}
    [GoValueClone("Type")] public partial struct CryptoReportKPP {}
    [GoValueClone("Type")] public partial struct CryptoReportLarval {}
    [GoValueClone("Type")] public partial struct CryptoReportRNG {}
    [GoValueClone("Type")] public partial struct CryptoStatAEAD {}
    [GoValueClone("Type")] public partial struct CryptoStatAKCipher {}
    [GoValueClone("Type")] public partial struct CryptoStatCipher {}
    [GoValueClone("Type")] public partial struct CryptoStatCompress {}
    [GoValueClone("Type")] public partial struct CryptoStatHash {}
    [GoValueClone("Type")] public partial struct CryptoStatKPP {}
    [GoValueClone("Type")] public partial struct CryptoStatLarval {}
    [GoValueClone("Type")] public partial struct CryptoStatRNG {}
    [GoValueClone("Name", "Driver_name", "Module_name")] public partial struct CryptoUserAlg {}
    [GoValueClone("Name")] public partial struct Dirent {}
    [GoValueClone("Version", "Name", "Uuid", "Data")] public partial struct DmIoctl {}
    public partial struct DmNameList {}
    public partial struct DmTargetDeps {}
    public partial struct DmTargetMsg {}
    [GoValueClone("Target_type")] public partial struct DmTargetSpec {}
    [GoValueClone("Version")] public partial struct DmTargetVersions {}
    public partial struct EpollEvent {}
    public partial struct EraseInfo {}
    public partial struct EraseInfo64 {}
    public partial struct EthtoolDrvinfo {}
    public partial struct EthtoolTsInfo {}
    public partial struct FanotifyEventMetadata {}
    public partial struct FanotifyResponse {}
    [GoValueClone("Bits")] public partial struct FdSet {}
    public partial struct FileCloneRange {}
    public partial struct FileDedupeRange {}
    public partial struct FileDedupeRangeInfo {}
    public partial struct FileHandle {}
    public partial struct Flock_t {}
    [GoValueClone("Key_spec")] public partial struct FscryptAddKeyArg {}
    [GoValueClone("Key_spec")] public partial struct FscryptGetKeyStatusArg {}
    [GoValueClone("Policy")] public partial struct FscryptGetPolicyExArg {}
    [GoValueClone("Raw")] public partial struct FscryptKey {}
    [GoValueClone("U")] public partial struct FscryptKeySpecifier {}
    [GoValueClone("Master_key_descriptor")] public partial struct FscryptPolicy {}
    [GoValueClone("Master_key_descriptor")] public partial struct FscryptPolicyV1 {}
    [GoValueClone("Master_key_identifier")] public partial struct FscryptPolicyV2 {}
    [GoValueClone("Key_spec")] public partial struct FscryptRemoveKeyArg {}
    public partial struct Fsid {}
    public partial struct FsverityDigest {}
    public partial struct FsverityEnableArg {}
    [GoValueClone("Name", "Label")] public partial struct GPIOChipInfo {}
    public partial struct GPIOV2LineAttribute {}
    [GoValueClone("Attrs")] public partial struct GPIOV2LineConfig {}
    public partial struct GPIOV2LineConfigAttribute {}
    public partial struct GPIOV2LineEvent {}
    [GoValueClone("Name", "Consumer", "Attrs")] public partial struct GPIOV2LineInfo {}
    [GoValueClone("Info")] public partial struct GPIOV2LineInfoChanged {}
    [GoValueClone("Offsets", "Consumer", "Config")] public partial struct GPIOV2LineRequest {}
    public partial struct GPIOV2LineValues {}
    public partial struct Genlmsghdr {}
    public partial struct HDDriveCmdHdr {}
    [GoValueClone("Serial_no", "Fw_rev", "Model", "Words69_70", "Words71_74", "Words76_79", "Words104_125", "Words130_155", "Words157_159", "Words161_175", "Words176_205", "Words206_254")] public partial struct HDDriveID {}
    public partial struct HDGeometry {}
    public partial struct HIDRawDevInfo {}
    [GoValueClone("Value")] public partial struct HIDRawReportDescriptor {}
    public partial struct HwTstampConfig {}
    public partial struct ICMPv6Filter {}
    public partial struct IPMIMsg {}
    public partial struct IPMIRecv {}
    public partial struct IPMIReq {}
    public partial struct IPMISystemInterfaceAddr {}
    public partial struct IPMreq {}
    public partial struct IPMreqn {}
    [GoValueClone("Addr")] public partial struct IPv6MTUInfo {}
    public partial struct IPv6Mreq {}
    public partial struct IfAddrlblmsg {}
    public partial struct IfAddrmsg {}
    public partial struct IfInfomsg {}
    public partial struct IfaCacheinfo {}
    [GoValueClone("raw")] public partial struct Ifreq {}
    public partial struct Inet4Pktinfo {}
    public partial struct Inet6Pktinfo {}
    public partial struct InotifyEvent {}
    public partial struct Iovec {}
    public partial struct ItimerSpec {}
    public partial struct ItimerWhich {}
    public partial struct Itimerval {}
    public partial struct KCMAttach {}
    public partial struct KCMClone {}
    public partial struct KCMUnattach {}
    public partial struct KernelTimespec {}
    public partial struct KeyctlDHParams {}
    public partial struct LandlockPathBeneathAttr {}
    public partial struct LandlockRulesetAttr {}
    public partial struct Linger {}
    public partial struct LoopConfig {}
    [GoValueClone("Name", "Encrypt_key", "Init", "Reserved")] public partial struct LoopInfo {}
    public partial struct LoopInfo64 {}
    public partial struct Mmsghdr {}
    public partial struct MountAttr {}
    public partial struct Msghdr {}
    public partial struct MtdEccStats {}
    public partial struct MtdInfo {}
    public partial struct MtdOobBuf {}
    public partial struct MtdOobBuf64 {}
    public partial struct MtdWriteReq {}
    [GoValueClone("Eccpos", "Oobfree")] public partial struct NandEcclayout {}
    public partial struct NandOobfree {}
    [GoValueClone("Oobfree", "Eccpos")] public partial struct NandOobinfo {}
    public partial struct NdMsg {}
    public partial struct NdUseroptmsg {}
    public partial struct NexthopGrp {}
    public partial struct Nfgenmsg {}
    public partial struct Nhmsg {}
    public partial struct NlAttr {}
    public partial struct NlMsgerr {}
    public partial struct NlMsghdr {}
    public partial struct NsIdReq {}
    public partial struct OpenHow {}
    public partial struct OtpInfo {}
    public partial struct PPSFData {}
    public partial struct PPSKInfo {}
    public partial struct PPSKParams {}
    public partial struct PPSKTime {}
    public partial struct PacketMreq {}
    public partial struct PerfEventAttr {}
    public partial struct PerfEventMmapPage {}
    public partial struct PidfdInfo {}
    public partial struct PollFd {}
    public partial struct PrefixCacheinfo {}
    public partial struct Prefixmsg {}
    public partial struct PtpClockCaps {}
    public partial struct PtpClockTime {}
    [GoValueClone("Rsv")] public partial struct PtpExttsEvent {}
    public partial struct PtpExttsRequest {}
    public partial struct PtpPeroutRequest {}
    public partial struct PtpPinDesc {}
    [GoValueClone("Rsv", "Ts")] public partial struct PtpSysOffset {}
    public partial struct PtpSysOffsetExtended {}
    public partial struct PtpSysOffsetPrecise {}
    public partial struct PtraceRegs {}
    public partial struct PtraceRegs386 {}
    public partial struct PtraceRegsAmd64 {}
    public partial struct RTCPLLInfo {}
    public partial struct RTCTime {}
    public partial struct RTCWkAlrm {}
    public partial struct RawFileDedupeRange {}
    public partial struct RawFileDedupeRangeInfo {}
    public partial struct RawSockaddr {}
    [GoValueClone("Type", "Name")] public partial struct RawSockaddrALG {}
    [GoValueClone("Pad")] public partial struct RawSockaddrAny {}
    [GoValueClone("Addr")] public partial struct RawSockaddrCAN {}
    public partial struct RawSockaddrHCI {}
    [GoValueClone("Nodeid", "User_id", "Name")] public partial struct RawSockaddrIUCV {}
    [GoValueClone("Addr", "Zero")] public partial struct RawSockaddrInet4 {}
    [GoValueClone("Addr")] public partial struct RawSockaddrInet6 {}
    [GoValueClone("Bdaddr")] public partial struct RawSockaddrL2 {}
    [GoValueClone("Addr")] public partial struct RawSockaddrL2TPIP {}
    [GoValueClone("Addr")] public partial struct RawSockaddrL2TPIP6 {}
    [GoValueClone("Addr")] public partial struct RawSockaddrLinklayer {}
    public partial struct RawSockaddrNFC {}
    [GoValueClone("Service_name")] public partial struct RawSockaddrNFCLLCP {}
    public partial struct RawSockaddrNetlink {}
    public partial struct RawSockaddrPPPoX {}
    [GoValueClone("Bdaddr")] public partial struct RawSockaddrRFCOMM {}
    [GoValueClone("Addr")] public partial struct RawSockaddrTIPC {}
    [GoValueClone("Path")] public partial struct RawSockaddrUnix {}
    [GoValueClone("Zero")] public partial struct RawSockaddrVM {}
    public partial struct RawSockaddrXDP {}
    public partial struct RegionInfo {}
    public partial struct RemoteIovec {}
    public partial struct Rlimit {}
    public partial struct RtAttr {}
    public partial struct RtGenmsg {}
    public partial struct RtMsg {}
    public partial struct RtNexthop {}
    public partial struct Rusage {}
    public partial struct SchedAttr {}
    [GoValueClone("Ts")] public partial struct ScmTimestamping {}
    public partial struct Siginfo {}
    public partial struct SignalfdSiginfo {}
    public partial struct Sigset_t {}
    public partial struct SockDiagReq {}
    public partial struct SockExtendedErr {}
    public partial struct SockFilter {}
    public partial struct SockFprog {}
    [GoValueClone("raw")] public partial struct SockaddrALG {}
    [GoValueClone("raw")] public partial struct SockaddrCAN {}
    [GoValueClone("raw")] public partial struct SockaddrCANJ1939 {}
    public partial struct SockaddrHCI {}
    [GoValueClone("raw")] public partial struct SockaddrIUCV {}
    [GoValueClone("Addr", "raw")] public partial struct SockaddrInet4 {}
    [GoValueClone("Addr", "raw")] public partial struct SockaddrInet6 {}
    [GoValueClone("Addr", "raw")] public partial struct SockaddrL2 {}
    [GoValueClone("Addr", "raw")] public partial struct SockaddrL2TPIP {}
    [GoValueClone("Addr", "raw")] public partial struct SockaddrL2TPIP6 {}
    [GoValueClone("Addr", "raw")] public partial struct SockaddrLinklayer {}
    public partial struct SockaddrNFC {}
    [GoValueClone("raw")] public partial struct SockaddrNFCLLCP {}
    public partial struct SockaddrNetlink {}
    [GoValueClone("raw")] public partial struct SockaddrPPPoE {}
    [GoValueClone("Addr", "raw")] public partial struct SockaddrRFCOMM {}
    public partial struct SockaddrStorage {}
    [GoValueClone("raw")] public partial struct SockaddrTIPC {}
    [GoValueClone("raw")] public partial struct SockaddrUnix {}
    [GoValueClone("raw")] public partial struct SockaddrVM {}
    public partial struct SockaddrXDP {}
    public partial struct SocketControlMessage {}
    public partial struct Stat_t {}
    public partial struct Statfs_t {}
    public partial struct StatxTimestamp {}
    public partial struct Statx_t {}
    public partial struct Sysinfo_t {}
    public partial struct SysvIpcPerm {}
    public partial struct SysvShmDesc {}
    public partial struct TCPBBRInfo {}
    public partial struct TCPDCTCPInfo {}
    public partial struct TCPInfo {}
    public partial struct TCPMD5Sig {}
    public partial struct TCPRepairOpt {}
    public partial struct TCPVegasInfo {}
    [GoValueClone("S")] public partial struct TIPCEvent {}
    public partial struct TIPCGroupReq {}
    [GoValueClone("Linkname")] public partial struct TIPCSIOCLNReq {}
    [GoValueClone("Id")] public partial struct TIPCSIOCNodeIDReq {}
    public partial struct TIPCServiceName {}
    public partial struct TIPCServiceRange {}
    public partial struct TIPCSocketAddr {}
    [GoValueClone("Handle")] public partial struct TIPCSubscr {}
    [GoValueClone("Ac_comm", "Ac_pad")] public partial struct Taskstats {}
    public partial struct Termios {}
    public partial struct Time_t {}
    public partial struct Timespec {}
    public partial struct Timeval {}
    public partial struct Timex {}
    public partial struct Tms {}
    public partial struct Tpacket2Hdr {}
    public partial struct Tpacket3Hdr {}
    public partial struct TpacketAuxdata {}
    public partial struct TpacketBDTS {}
    [GoValueClone("Hdr")] public partial struct TpacketBlockDesc {}
    public partial struct TpacketHdr {}
    public partial struct TpacketHdrV1 {}
    public partial struct TpacketHdrVariant1 {}
    public partial struct TpacketReq {}
    public partial struct TpacketReq3 {}
    public partial struct TpacketStats {}
    public partial struct TpacketStatsV3 {}
    public partial struct Ucred {}
    public partial struct Ustat_t {}
    public partial struct Utimbuf {}
    public partial struct Utsname {}
    public partial struct WaitStatus {}
    public partial struct WatchdogInfo {}
    public partial struct Winsize {}
    public partial struct XDPDesc {}
    public partial struct XDPMmapOffsets {}
    public partial struct XDPRingOffset {}
    public partial struct XDPStatistics {}
    public partial struct XDPUmemReg {}
    public partial struct _Socklen {}
    public partial struct cpuMask {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyscall() => builtin.initPackage(typeof(syscall_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(unix_package.CPUSet)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛCPUSet;
[GoMethodHost(typeof(unix_package.CPUSetDynamic)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛCPUSetDynamic;
[GoMethodHost(typeof(unix_package.Cmsghdr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛCmsghdr;
[GoMethodHost(typeof(unix_package.FdSet)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛFdSet;
[GoMethodHost(typeof(unix_package.FileHandle)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛFileHandle;
[GoMethodHost(typeof(unix_package.Ifreq)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛIfreq;
[GoMethodHost(typeof(unix_package.Iovec)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛIovec;
[GoMethodHost(typeof(unix_package.Msghdr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛMsghdr;
[GoMethodHost(typeof(unix_package.PtraceRegs)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛPtraceRegs;
[GoMethodHost(typeof(unix_package.RawSockaddrNFCLLCP)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛRawSockaddrNFCLLCP;
[GoMethodHost(typeof(unix_package.SockaddrALG)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛSockaddrALG;
[GoMethodHost(typeof(unix_package.SockaddrCAN)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛSockaddrCAN;
[GoMethodHost(typeof(unix_package.SockaddrCANJ1939)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛSockaddrCANJ1939;
[GoMethodHost(typeof(unix_package.SockaddrHCI)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛSockaddrHCI;
[GoMethodHost(typeof(unix_package.SockaddrIUCV)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛSockaddrIUCV;
[GoMethodHost(typeof(unix_package.SockaddrInet4)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛSockaddrInet4;
[GoMethodHost(typeof(unix_package.SockaddrInet6)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛSockaddrInet6;
[GoMethodHost(typeof(unix_package.SockaddrL2)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛSockaddrL2;
[GoMethodHost(typeof(unix_package.SockaddrL2TPIP)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛSockaddrL2TPIP;
[GoMethodHost(typeof(unix_package.SockaddrL2TPIP6)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛSockaddrL2TPIP6;
[GoMethodHost(typeof(unix_package.SockaddrLinklayer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛSockaddrLinklayer;
[GoMethodHost(typeof(unix_package.SockaddrNFC)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛSockaddrNFC;
[GoMethodHost(typeof(unix_package.SockaddrNFCLLCP)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛSockaddrNFCLLCP;
[GoMethodHost(typeof(unix_package.SockaddrNetlink)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛSockaddrNetlink;
[GoMethodHost(typeof(unix_package.SockaddrPPPoE)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛSockaddrPPPoE;
[GoMethodHost(typeof(unix_package.SockaddrRFCOMM)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛSockaddrRFCOMM;
[GoMethodHost(typeof(unix_package.SockaddrTIPC)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛSockaddrTIPC;
[GoMethodHost(typeof(unix_package.SockaddrUnix)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛSockaddrUnix;
[GoMethodHost(typeof(unix_package.SockaddrVM)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛSockaddrVM;
[GoMethodHost(typeof(unix_package.SockaddrXDP)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛSockaddrXDP;
[GoMethodHost(typeof(unix_package.TIPCServiceName)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛTIPCServiceName;
[GoMethodHost(typeof(unix_package.TIPCServiceRange)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛTIPCServiceRange;
[GoMethodHost(typeof(unix_package.TIPCSocketAddr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛTIPCSocketAddr;
[GoMethodHost(typeof(unix_package.Timespec)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛTimespec;
[GoMethodHost(typeof(unix_package.Timeval)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛTimeval;
[GoMethodHost(typeof(unix_package.WaitStatus)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛWaitStatus;
[GoMethodHost(typeof(unix_package.mmapper)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛmmapper;
[GoMethodHost(typeof(unix_package.mremapMmapper)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class unixᴛmremapMmapper;
// </MethodHolders>
