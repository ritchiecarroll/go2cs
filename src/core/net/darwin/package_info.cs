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
using context = go.context_package;
using dnsmessage = go.vendor.golang.org.x.net.dns.dnsmessage_package;
using netip = go.net.netip_package;
using os = go.os_package;
using poll = go.@internal.poll_package;
using singleflight = go.@internal.singleflight_package;
using syscall = go.syscall_package;
using time = go.time_package;
using Δio = go.io_package;
// </ImportedTypeAliases>

using go;
using static go.net_package;
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
[assembly: GoDynamicTypeLift("7374727563747b73796e632e4d757465783b2062794e616d65206d61705b737472696e675d6e65742e62794e616d653b20627941646472206d61705b737472696e675d5b5d737472696e673b206578706972652074696d652e54696d653b207061746820737472696e673b206d74696d652074696d652e54696d653b2073697a6520696e7436347d", "hostsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73796e632e4f6e63653b2076616c20696e747d", "listenerBacklogCacheᴛ1")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<AddrError, Error>(Pointer = true)]
[assembly: GoImplement<AddrError, error>(Pointer = true)]
[assembly: GoImplement<Buffers, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<Buffers, io_package.WriterTo>(Pointer = true)]
[assembly: GoImplement<Conn, io_package.Reader>]
[assembly: GoImplement<DNSConfigError, Error>(Pointer = true)]
[assembly: GoImplement<DNSError, Error>(Pointer = true)]
[assembly: GoImplement<DNSError, error>(Pointer = true)]
[assembly: GoImplement<IPAddr, Addr>(Pointer = true)]
[assembly: GoImplement<IPAddr, sockaddr>(Pointer = true)]
[assembly: GoImplement<IPConn, Conn>(Pointer = true)]
[assembly: GoImplement<IPConn, PacketConn>(Pointer = true)]
[assembly: GoImplement<IPNet, Addr>(Pointer = true)]
[assembly: GoImplement<InvalidAddrError, Error>(Pointer = true)]
[assembly: GoImplement<InvalidAddrError, Error>]
[assembly: GoImplement<OpError, Error>(Pointer = true)]
[assembly: GoImplement<OpError, error>(Pointer = true)]
[assembly: GoImplement<ParseError, Error>(Pointer = true)]
[assembly: GoImplement<ParseError, error>(Pointer = true)]
[assembly: GoImplement<TCPAddr, Addr>(Pointer = true)]
[assembly: GoImplement<TCPAddr, sockaddr>(Pointer = true)]
[assembly: GoImplement<TCPConn, Conn>(Pointer = true)]
[assembly: GoImplement<TCPListener, Listener>(Pointer = true)]
[assembly: GoImplement<UDPAddr, Addr>(Pointer = true)]
[assembly: GoImplement<UDPAddr, sockaddr>(Pointer = true)]
[assembly: GoImplement<UDPConn, Conn>(Pointer = true)]
[assembly: GoImplement<UDPConn, PacketConn>(Pointer = true)]
[assembly: GoImplement<UnixAddr, Addr>(Pointer = true)]
[assembly: GoImplement<UnixAddr, sockaddr>(Pointer = true)]
[assembly: GoImplement<UnixConn, Conn>(Pointer = true)]
[assembly: GoImplement<UnixConn, PacketConn>(Pointer = true)]
[assembly: GoImplement<UnixListener, Listener>(Pointer = true)]
[assembly: GoImplement<UnknownNetworkError, Error>(Pointer = true)]
[assembly: GoImplement<UnknownNetworkError, Error>]
[assembly: GoImplement<addrPortUDPAddr, Addr>]
[assembly: GoImplement<addrinfoErrno, Error>]
[assembly: GoImplement<canceledError, error>]
[assembly: GoImplement<dialParallel_dialResult, Conn>(Promoted = true)]
[assembly: GoImplement<dialParallel_dialResult, error>(Promoted = true)]
[assembly: GoImplement<fileAddr, Addr>]
[assembly: GoImplement<goLookupIPCNAMEOrder_result, error>(Promoted = true)]
[assembly: GoImplement<notFoundError, error>(Pointer = true)]
[assembly: GoImplement<onlyValuesCtx, context_package.Context>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<pipe, Conn>(Pointer = true)]
[assembly: GoImplement<pipeAddr, Addr>]
[assembly: GoImplement<rawConn, syscall_package.RawConn>(Pointer = true)]
[assembly: GoImplement<rawListener, syscall_package.RawConn>(Pointer = true)]
[assembly: GoImplement<tcpConnWithoutReadFrom, io_package.Writer>]
[assembly: GoImplement<tcpConnWithoutWriteTo, io_package.Reader>]
[assembly: GoImplement<temporaryError, error>(Pointer = true)]
[assembly: GoImplement<timeoutError, error>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(@file))]
[assembly: GoFieldAccess(typeof(DNSError))]
[assembly: GoFieldAccess(typeof(IPAddr))]
[assembly: GoFieldAccess(typeof(IPNet))]
[assembly: GoFieldAccess(typeof(MX))]
[assembly: GoFieldAccess(typeof(NS))]
[assembly: GoFieldAccess(typeof(OpError))]
[assembly: GoFieldAccess(typeof(Resolver))]
[assembly: GoFieldAccess(typeof(SRV))]
[assembly: GoFieldAccess(typeof(TCPAddr))]
[assembly: GoFieldAccess(typeof(UDPAddr))]
[assembly: GoFieldAccess(typeof(UnixAddr))]
[assembly: GoFieldAccess(typeof(UnixListener))]
[assembly: GoFieldAccess(typeof(_C_struct_addrinfo))]
[assembly: GoFieldAccess(typeof(byRFC6724Info))]
[assembly: GoFieldAccess(typeof(conf))]
[assembly: GoFieldAccess(typeof(dnsConfig))]
[assembly: GoFieldAccess(typeof(go.@internal.nettrace_package.Trace))]
[assembly: GoFieldAccess(typeof(go.@internal.routebsd_package.InetAddr))]
[assembly: GoFieldAccess(typeof(go.@internal.routebsd_package.InterfaceAddrMessage))]
[assembly: GoFieldAccess(typeof(go.@internal.routebsd_package.InterfaceMessage))]
[assembly: GoFieldAccess(typeof(go.@internal.routebsd_package.InterfaceMulticastAddrMessage))]
[assembly: GoFieldAccess(typeof(go.@internal.routebsd_package.LinkAddr))]
[assembly: GoFieldAccess(typeof(go.vendor.golang.org.x.net.dns.dnsmessage_package.CNAMEResource))]
[assembly: GoFieldAccess(typeof(io_package.LimitedReader))]
[assembly: GoFieldAccess(typeof(ipAttr))]
[assembly: GoFieldAccess(typeof(ipStackCapabilities))]
[assembly: GoFieldAccess(typeof(netFD))]
[assembly: GoFieldAccess(typeof(nssConf))]
[assembly: GoFieldAccess(typeof(nsswitchConfig))]
[assembly: GoFieldAccess(typeof(os_package.SyscallError))]
[assembly: GoFieldAccess(typeof(pipe))]
[assembly: GoFieldAccess(typeof(resolverConfig))]
[assembly: GoFieldAccess(typeof(sysDialer))]
[assembly: GoFieldAccess(typeof(sysListener))]
[assembly: GoFieldAccess(typeof(syscall_package.IPv6Mreq))]
[assembly: GoFieldAccess(typeof(syscall_package.RawSockaddrInet4))]
[assembly: GoFieldAccess(typeof(syscall_package.RawSockaddrInet6))]
[assembly: GoFieldAccess(typeof(syscall_package.SockaddrInet4))]
[assembly: GoFieldAccess(typeof(syscall_package.SockaddrInet6))]
[assembly: GoFieldAccess(typeof(syscall_package.SockaddrUnix))]
[assembly: GoFieldAccess(typeof(time_package.Timer))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("net/addrselect.go", "addrselect.cs", "AAwcgoKUpoKClIKCgu6Cgv6ygoKCgoKCgoKUpgAIEoKClIIADSTygoKCgoKCgpSC3IKUgpSC7oKUggAVLISUhNyClIIADBqClIIACxiCgpSClILuAAwaADt+xIKUgoKmAAwegoKUgoKCuIKUAAIaAAwCgpSCgqaCgpSCgoKCgpSCgoKCgoKCgrg=", "", "", "27=ipAttrOf/1/2/2,ipAttrOf/2/2/4;75=classifyScope/1/1/1;224=AddrFrom16/1/9/3,PrefixFrom/1/9/3,AddrFrom16/2/9/10,PrefixFrom/2/9/10,AddrFrom16/3/9/16,PrefixFrom/3/9/16,AddrFrom16/4/9/23,PrefixFrom/4/9/23,AddrFrom16/5/9/30,PrefixFrom/5/9/30,AddrFrom16/6/9/36,PrefixFrom/6/9/36,AddrFrom16/7/9/42,PrefixFrom/7/9/42,AddrFrom16/8/9/48,PrefixFrom/8/9/48,AddrFrom16/9/9/54,PrefixFrom/9/9/54")]
[assembly: GoPositionMap("net/cgo_unix.go", "cgo_unix.cs", "ACFGgoKW/vKCgqaCgpaKgrKCgoKipLSkgvjCgoKUspS2goLWgrSCtJSUtLaqwoKCpoKUgoKClIKUpKSmlIKUgoKkgoKm5sKCgoKClLS2goKUgoKClAAHEJSkpNyUpKiUlIKUlIKCtIKCtuaCAA0kooKClIKCprqigoKCgoKCpoKUgpSkpKaClKaCgpSClKaygoKUgoKUqqLqooKCgoKCgoKUgpQACRSCgpSmgoKmgoKCgpSCuIKCpoKClIKCgpQ=", "64-69:1;111-113:1;228-230:1;256-258:1;315-317:1;345-348:1", "", "", "", "410:21,412:13")]
[assembly: GoPositionMap("net/cgo_unix_syscall.go", "cgo_unix_syscall.cs", "ACZYlIKClKaCgoKCgpSCpoKmgqaCgqaCgqaCpoKmgoKCpoKCgg==")]
[assembly: GoPositionMap("net/conf.go", "conf.cs", "AEWUAZiSggAMCsKCgoKUgoKClIKUlIKUxoKUxoKCgpQAChSmgriCgrik7oKCgsqCgrSoogAIEMrKpr7SgqYABxCCuKwACQKCgqbsAAkCgoKmABIGpKSCuIKClIKUyqa4grikAAgSlJSmpsq4gqaCypSUlKSCgqaUpIKCppS2AAcQlIKmgrimlKamgpSSgoKClJSClIKClIKUpoK4gpSCgpS0yqyylIKUlLS0pIKUxriCgoKCggAHEIKCgu6UgpTGpLimAAEcAA8CgoKClIKUpoKCgpSCAAwKouqi6qI=", "92-121:1;222-224:1;234-236:1;500-509:1", "", "", "", "99:25,100:17,247:25,248:17,269:25,270:17")]
[assembly: GoPositionMap("net/dial.go", "dial.cs", "AChUAAoagpSkuIKUpqaCgpQADB6ClKTKgpSmpoKClABq7AGUgoKUgpQAAhIACAKClIKUpoKClKqigpSCgqaEkoKClKamgoKUuLKCgviC+JSUgpSCgoKCgqaUrLKCgpSClJSCgpSClJSCgpS8lIK0grSCpIKCgpSUgpS0gpS0gpS2gpSuwgACEuIAAmYAMAKCAAIaAAoCggAJIvIABioAFgKCtoKCgoKCpoKCptySgpSm1oKClIKSgoKCgoKmgIKCsrTopKKClIIABBoADAKCkpKClIKUAAUWAAwCgpKSgpSClAAFFgAMAoKSkoKUgpQABRYADAKCkgAMDuKCpoKEAAEQwoKClILmgtq2gpK2gqSCtIKSxoKUgpSUgpTKAAkQ4pSCpNiCgIKClIKUlIKCgtiCgpSCuIKU2gAIAoKCgoKUgpC2gpSCgqTWgsSCxILUlIKUAClawgACEuIABhTygoKUyoKClIKk1tSUgpQAAhTygoKUyoKClMTE1JSClAAIPAAVAoIAAjgAGQKC", "576-582:1;585-592:2;692-705:1;802-802:1", "", "", "544:21,643:21,673:21,703:21,733:21,759:21,792:21,799:21,808:29,874:37", "914:29,915:21")]
[assembly: GoPositionMap("net/dnsclient.go", "dnsclient.cs", "ABAoxIKmgqzSgoKUgqaUgoLcgqaygpSCgoKClIKUgqYAAiAADgSCAAoWgoKmgoKCgoKUpILGxoKUgsaClIKUpJSCpgACFgAIAoKUAA0gooKClIKCgoKCgoKUpoK6koKClJSCgoKCpgALGpKCgpQ=", "200-205:1;231-233:1")]
[assembly: GoPositionMap("net/dnsclient_unix.go", "dnsclient_unix.cs", "AClWgoKCgoLKqJTigoKClIKmgqaClIKClIK4goKUgoKCgqaygpSClIKUpqKAgraCgoKClLiCgpSCgpS4ooKmgoKUgoKUgoKUgoKClIKClIKUqNKCgoKUgoKUlIKCpIKClIKUgoKClJSCgpSCAAgSgpSU2LKUgqaCgsqCptyClKamooKCgpSClIKUggAEELKCgoKCgoKUgoKUgs7CgoKUgoKU3IKClJKCpoKUgqaCppSCpoKmlIKmpgATIoKCqNaCuKwACAKUgriClJSCgpSUAAoQgoKUgsiCoqSCpKS4gqai3KaCpqqCgoKUpqaClLiU7sKClIKopIKCgriCgpSmgoKmlIKmgoKCuIKUAA4iAAcQgoKUprKUgoKmgqaCgpSCspSowoKCgoKCpoKq0oIACAbSgoKUgoKSgpSmgriUlIyCpoKCgpSUtKSCgoKSooKCgtaSgpKSgqaogoKClIKCgoKUgoKUlAARJIKCgoLcgpSUgoLKlIKCyIKCypSCgsiCgsqUgsiCypTouIKUgqa4lIKCgoKCgoKSgpSmgqbokoKosoKCgqaCuIKClJKCgoKCgriUgoKCgpSC3IKCgtyUgoLcqA==", "656-656:1;657-662:2;664-671:3;666-670:3.1;672-674:4", "", "794=Error/1/1/1;807=Error/1/1/1;823=Error/1/1/1;839=Error/1/1/1;853=Error/1/1/1;955=Error/1/1/1;964=Error/1/1/1;974=Error/1/1/1", "201:25")]
[assembly: GoPositionMap("net/dnsconfig.go", "dnsconfig.cs", "ABk0lAATMOKClA==")]
[assembly: GoPositionMap("net/dnsconfig_unix.go", "dnsconfig_unix.cs", "ABomssqCgoKCgpSSgpSCgoKUgpSUgoKUpLiC2oLIgoKCgpTIgpSCgoKClLSCgpS0goKUtAAIDgAJELQACAq0AAkQtraClIKU1oKClJSClKaCgpQ=")]
[assembly: GoPositionMap("net/error_posix.go", "error_posix.cs", "AAsgooKU")]
[assembly: GoPositionMap("net/error_unix.go", "error_unix.cs", "AAoWgoKU")]
[assembly: GoPositionMap("net/fd_posix.go", "fd_posix.cs", "ABo6gpKClIKUpqKClKaUguaCgoKmgqaCprKCgqbCgoKUsoKCprKCgqbigoKm0oKCptKCgqaygoKmsoKCprKCgqaygoKmwoKCpsKCgqbCgoKmgqaCpoI=")]
[assembly: GoPositionMap("net/fd_unix.go", "fd_unix.cs", "AB00ggAJFKaiAAgGAAkIgMakxoKUgv6ClKSkgIK2goKAgoLagsiClJLu7gAIEoCCpMakgoKUgNbagsak+MKCgoKUpoKClIKClIKCqMSygoKClKY=", "88-94:1;95-107:2", "", "", "", "128:25,129:17")]
[assembly: GoPositionMap("net/file.go", "file.cs", "AA8ckIKcAAgCgoKUrgAIAoKClK4ACAKCgpQ=")]
[assembly: GoPositionMap("net/file_posix.go", "file_posix.cs", "ABIegoKClIKCgoKUgoKClIKUtLS0gpSCgoKUgoKCgoKUgqaCgoKUlKSkpJSCpoKCgpSUpJSCpoKCgpSUpKSUgg==")]
[assembly: GoPositionMap("net/file_unix.go", "file_unix.cs", "ABMigoKCgpSUgoKU")]
[assembly: GoPositionMap("net/hook.go", "hook.cs", "AAweAAAQkuw=")]
[assembly: GoPositionMap("net/hook_unix.go", "hook_unix.cs", "AAoY2oKCgg==")]
[assembly: GoPositionMap("net/hosts.go", "hosts.cs", "ABMkgoKClAAbOqKClIKUgoKCpoKUgoKCuIKSgoCUpIKClIKCpoKCgoKClIKmlIK4pgAHEIKCgoKCoqaygoKCgoKCgpSCgoKm2LKCgoKCgpSCgoKCpg==")]
[assembly: GoPositionMap("net/interface.go", "interface.cs", "ABIsgoKCggAVMgAIEoKCgoKClKaClKqigpSCgpSqooKUgoKUqJKCgpSClAACENKCgpQAAhDSgpSCgpSCgpSmgrKCpqiSgpSCgpSClLKCpgAPIAAFEAAIAoKCgoKUgoKCgqaCgoKCgoK45oKClIKCgoKCgoKClIKUpoKClIKCgoKCgoKClIKU")]
[assembly: GoPositionMap("net/interface_bsd.go", "interface_bsd.cs", "AA0isoKClIKClIKCgpSClIKCgoKClIKCgsimgoKClIKUgpSClIKUgpSs0oKClIKClIKClIKUgpSCgoKCgoK2gpSCgoKCgoK2gsg=")]
[assembly: GoPositionMap("net/interface_darwin.go", "interface_darwin.cs", "AAwYgqqigoKUgoKUgpSClIKCgoKCgraCyA==")]
[assembly: GoPositionMap("net/ip.go", "ip.cs", "ADBqooKCgoKCgqaYooKCgoKCrLKClIKUgoKCgoKCgpSClLqSkpK6kpKSkpKqoqiSgpSqou68qJKClKqiqqKClKqigpQAAhYACAIAAhKSgoKmqqKClIiUqqKClIKUuoKCnLKClJSkpLiCgoKmqJKClIKUgoKUgoKUAAYS4oKmgqaClIK0gqSCpoKCgpSqooKUqrSClIKssoKUgqasxIKClKqigoKUgoKClIKssoKUgpSClKaCqqKCsoKCuIKCpoKUgoKmlKzigoKUqJKClKaygoKCpoKUgsaCxpSokoKClIKClIKCpuiAAAEQ4oKUgoKUgoKUAAIS4oKUpoKCgpQAAhYACAKCgqaCgqaCgpSCgqaCgoI=", "", "", "195=Equal/1/1/1,IsUnspecified/1/1/2,IsLoopback/1/1/3,IsMulticast/1/1/4,IsLinkLocalUnicast/1/1/5;214=isZeros/1/1/1")]
[assembly: GoPositionMap("net/iprawsock.go", "iprawsock.cs", "AAxEABICAAsagJSigpSCgpSmooKUpoKClAACIAANAoKUgoKUxpSCgpQACBbCgpSosoKUgoKUqLKClIKClIKUAAIUAA4CgpSCgpSosoKUgoKUqLKClIKClIKClAACEgALAoKUgoKUpgABEvKmooKUgoKUgoKUAAIUAAkCgpSCgoKU", "", "", "32=AsSlice/1/1/1,Zone/1/1/2")]
[assembly: GoPositionMap("net/iprawsock_posix.go", "iprawsock_posix.cs", "AAscgpSklKaigpSClKaigpSmgqaGooKUgrSkpoKClIKClIKUgqbigoKUtKSmgoKUgpSCgpSmooKUgpSCgpSmooKClMaUgoKYgoKUpqKCgpTGlIKCmIKClA==", "127-129:1;150-152:1")]
[assembly: GoPositionMap("net/ipsock.go", "ipsock.cs", "AB5CooKqooKs1qSmgt6SlKSklKiAmrKCpsakgpSqooKCpgACEgAJAoKCgoKClKau4oKCgqaClLiSqJIAEhgADAKYgpS2goKmlIKClKYACAyClJSClIKCpoKUgqaCptaClJQAAhD2gpSuwrqkgoKUgtiCxpSSpKSkpLaCuIKC3IKmgoKUgpQAAhwACwKClA==", "170-172:1;272-283:1")]
[assembly: GoPositionMap("net/ipsock_posix.go", "ipsock_posix.cs", "ABY4AA4CtoKCgqaCxoKkAAUS2qSCgoKUgoKCgpSClIKUxgACagAzApSkpoKClIKUpoSUpoKkgraCpoKClIKClIKCpgAKFoK4goKUgoIAAhwACwKUkoKUpJKClJSm7oKClLim7oLK", "", "", "58=ParseIP/1/1/5,IPv4/1/1/7;168=family/2/2/1;268=As4/1/1/1,Port/1/1/2;283=As16/1/1/1,Port/1/1/2,Zone/1/1/3,index/1/1/3")]
[assembly: GoPositionMap("net/lookup.go", "lookup.cs", "ABs0AA0cABo4goKCgoKClAANFIKUgpSkpJSmgoKCgoKClJSqooKUgoKUygAlVsCioKSCgpQAAhDSqrSClIKUqqKCgpSCgpSqoq7CgoKUxqaClIKCpoKClK4ACAqCgpSCgoKmABUUlpKkpAADEMKqtIKUgIKkgoLKgoIABxCUgoKitoKCgpQAChKCgrSkgoKUpIKCgoKCpoKClMyigpSCgoKClKiSgoKUrsKs4oKCxrSUgoKmgpQAAiYAEAIAAiAADQKCgpSClAACIgAOAgACIgAOAoKClIKUgoKClIKUlIKUAAIYAAkCAAIS4oKClIKCgpSmgoKmlIKUAAIYAAkCAAIS4oKClIKCgpSClJSClAACFPKuwgACHgAMAgACEuKCgpSCgoKmgpQABxbc0oKClIKUgpQAAhgADQKClJSSgpSCgoKClILcgoLclIKUgoLclIKokpKClIKCgoKUgtyCgtyUgoLcloKospKClIKCgoKUgtyCgtyUgoLclLiSkoKUgoKCgpSC3IKC3JSCggAIEoKClIKClIKUlAAIBoKClIKClA==", "334-336:1;338-342:2;354-354:3")]
[assembly: GoPositionMap("net/lookup_unix.go", "lookup_unix.cs", "ABEiooKClKSUgpSCgpSCgpSCguqoooKmgoKClKaigoKUgqamgoKmgqaUpoKCgoKmpoKmgqaCpoKmgoKClA==")]
[assembly: GoPositionMap("net/mac.go", "mac.cs", "AA0YooKUgoKClIKUAAIgAA8CgqaCgpSCgpSCgoKClJSCgpSCgpSCgoKClIKUpoKmgoKmgoKCgpSmlII=")]
[assembly: GoPositionMap("net/mptcpsock_stub.go", "mptcpsock_stub.cs", "AAoagqaCpoI=")]
[assembly: GoPositionMap("net/net.go", "net.cs", "ALYB9gKgqtKClIKClKiygpSCgpSosoKUgoKUrNKClKzSgpSosoKUgpSosoKUgpSosoKUgpSqwoKUgpSqwoKUgpQAAhoADQKCgpQARaoBAA0CgJIAHkCmpoIACg6UmKKUpKQAHUSUooKUgoKUgpSCgpSUlILMuILegoKClILupoKmgoKUggALGpSC7qKClIKClKaCuIKCuIKCAAwaAAgIgoKUggAIEoKCgqiC/vyCggAPJqK8goLKgqaSAAoYgJSigpSCgpSCrKCaoLjuAAYQogAKGrQABxKiAAoYpAAMGoKClLSkyIIAESaCAAESAAkCgpSCgoKCgqaCAAISAAkCgoKCgpSClKaCgoKCgpSCgg==", "405-405:1;810-812:1", "", "733=Error/1/1/2")]
[assembly: GoPositionMap("net/nss.go", "nss.cs", "ACBCgoKCgoLokoKCqOKmgpSUgoKUlIKClIKmgoKCoqSCpoKkpLiCAA4iooKCpgAQHLKClIKktMaUgpSmooKClJKCgqaCggALBqKCgoKClIKCgpSCgoKCgpSCgoKClIKUlIKCgoKUgoKCgpSUgpTcAAgIwoKCgoKUgpSCgpSCgoKUypQ=", "223-247:1")]
[assembly: GoPositionMap("net/parse.go", "parse.cs", "ABculLKCgoKCgpSCgoKCppSCgpSmsoKUgoKCgpSCpoKmooKClKaCgoKUpqKCgpSokoKCgqaokoKCgoKCgoKUpoKClKbO0oKCgoKmgpSq0oKCgoKCgoKCgoKUlIKmgpSuwoKUgqiSgoKmqJKCgsySgpSokoKUgpSokqqigpSqooKCgoKUgoKmlKqiqqKClIKCpg==")]
[assembly: GoPositionMap("net/pipe.go", "pipe.cs", "ABMqggACEgALAoKUgpSmgoKClLiAgoKUgpTIgrSm4oKC1oKkpAAKDIIAEjjSgoKCgoKU7u6mgpSCgoKUpqKUpKSm1IKCpKSkyIKCgpSm4pSkpKaCgoLUgoK0pKSm5qKClIKCpqKClIKmooKUgqaCgJI=", "54-56:1;236-236:1", "", "141=makePipeDeadline/1/2/4,makePipeDeadline/2/2/5;148=makePipeDeadline/1/2/4,makePipeDeadline/2/2/5")]
[assembly: GoPositionMap("net/port.go", "port.cs", "AAceAAoCppSYgoKCgoKUgqKClJSCgpSCgoKClJSCgoKUlIKU")]
[assembly: GoPositionMap("net/port_unix.go", "port_unix.cs", "ABQkooKClKSUgIKkgoKUgoKClIKCgoKUgoLYppKC")]
[assembly: GoPositionMap("net/rawconn.go", "rawconn.cs", "ABc0oKSigpSCgoKUpqKClIKCgpSmooKUgoKClAACFAAJAoKUpoIAAhTy7oKmgqaC")]
[assembly: GoPositionMap("net/rlimit_unix.go", "rlimit_unix.cs", "AAoqAAoCgoKUgoKCgpQ=")]
[assembly: GoPositionMap("net/sendfile.go", "sendfile.cs", "ABMwAAsCgpSCgoKCgsqCgqaCgqaCgoKUgqaCpg==", "49-52:1")]
[assembly: GoPositionMap("net/sendfile_nonwindows.go", "sendfile_nonwindows.cs", "AAgUkg==")]
[assembly: GoPositionMap("net/sock_bsd.go", "sock_bsd.cs", "ABMcgpiktPikgsqClA==")]
[assembly: GoPositionMap("net/sock_posix.go", "sock_posix.cs", "AA4k0oKClIKClIKCABgygpSCgpSkgoKUpoKClKaCpJSUlIKU5qKCgoKCgoKClIK4goKCgoKCgriCgoKClIKUlIIACBKCgoKAgpSkpqKCgpSCgqaCgoK4gpSClIKUgoKmogAIEoKClIKUtKS2goKCpoKCgqaClIKUgoI=")]
[assembly: GoPositionMap("net/sockaddr_posix.go", "sockaddr_posix.cs", "AB5IgpSUpKTGlKSktg==", "57-57:1")]
[assembly: GoPositionMap("net/sockopt_bsd.go", "sockopt_bsd.cs", "ABAegsqUtLa4lJSUppSmpoLc")]
[assembly: GoPositionMap("net/sockopt_posix.go", "sockopt_posix.cs", "AAwekoKUpoKClIKClIKUgsaCyKaCgoKmgoKCpoKCgqaCgoKClIKUgoI=")]
[assembly: GoPositionMap("net/sockoptip4_bsdvar.go", "sockoptip4_bsdvar.cs", "AAwcgoKClIKCgoKmgoKC")]
[assembly: GoPositionMap("net/sockoptip4_posix_nonlinux.go", "sockoptip4_posix_nonlinux.cs", "AA0egoKClIKCpoKClIKClIKUgIKC1oCCgtiCgpQ=")]
[assembly: GoPositionMap("net/sockoptip6_posix.go", "sockoptip6_posix.cs", "AAwcooKClIKCpoKCgqaigoKClIKC")]
[assembly: GoPositionMap("net/splice_stub.go", "splice_stub.cs", "AAoWgqaC")]
[assembly: GoPositionMap("net/sys_cloexec.go", "sys_cloexec.cs", "ABMotIKCgpSCgpSCgpQ=")]
[assembly: GoPositionMap("net/tcpsock.go", "tcpsock.cs", "ABtA8oKUgoKogJSigpSCgpSmooKUpoKClAACJAAPAsa0lIKClKyyACxkwoKUqLKClIKClKiygpSCgpSqwoKUgpSqwoKUgpQAAiIAEAKClIKUqsKClIKUAAIQ8oKUgpSu4oKUgpQAAhgACwKClKaCgoLKgoKClIKCpgACFPKmosaUgpSCmIKUgpSUgpQACR7ygpSqwoKUgoKUqsKClIKClKrCgpSClKygmMKClAACGgAOAoKUgoKUAAIYAAsCxpSClIKYgpSUgpSokg==", "", "", "114=Addr/1/2/1,AsSlice/1/1/1,Addr/2/2/2,Zone/1/1/2,Port/1/1/3")]
[assembly: GoPositionMap("net/tcpsock_posix.go", "tcpsock_posix.cs", "AA4ggpSklKaigpSClKaigpSmgqaigIKkgIKkpqKClKaigIKkgIKkpoKmsoKCmAAZNIKClKaClKaUggAKFoKUgoKmgoKUgpSmoKSCgoKUpoKmgoKClKaCpqKCgpiCgpQ=", "81-83:1;185-187:1")]
[assembly: GoPositionMap("net/tcpsock_unix.go", "tcpsock_unix.cs", "AAsYsoKmgIKkgIKkgIKkgIK2")]
[assembly: GoPositionMap("net/tcpsockopt_darwin.go", "tcpsockopt_darwin.cs", "ABAmgoKCgriCgoKmgoKCgriCgoKmgoKCgqaCgg==")]
[assembly: GoPositionMap("net/tcpsockopt_posix.go", "tcpsockopt_posix.cs", "AAwcgoKC")]
[assembly: GoPositionMap("net/udpsock.go", "udpsock.cs", "ABpA8oKUgoKogJSigpSCgpSmooKUpoKClAACJAAPAsa0lIKClKyyAAsYAAcUwoKUqNqowoKUgoKUqJKClJQAAhAACgKClIKClAACFAANAoKCgpSoAAgCgpSCgpSosoKUgoKUqLKClIKClKiygpSCgpSCgpQAAhYADQKClIKClKjigpSCgpSmAAES8qaixpSClIKClIKClAACGAALAsaUgpSCgoKUAAIuABYCxpSClIKCgpQ=", "", "", "113=Addr/1/2/1,AsSlice/1/1/1,Addr/2/2/2,Zone/1/1/2,Port/1/1/3")]
[assembly: GoPositionMap("net/udpsock_posix.go", "udpsock_posix.cs", "AA0egpSklKaigpSClKaigpSmgqaigoKUgoKCgsaCgoKCtpSUpsKCgpSCgoKCxoKCgoK2gpSm4pSCgoK0goKCpKaigpSCppSSgpSkkoKUpLiCgpSCppSSgpSkkoKUpLiigpSClIKClKaCgpSCppSCgpKClJSkgoKSgpSUpLiigoKYgoKUpqKCgpiCgpSmsoKCmIKClIKCgoKmgoKmpoKCgqaAgqSAgqSmgoKCpoCCpICCpA==", "216-218:1;230-232:1;244-246:1")]
[assembly: GoPositionMap("net/unixsock.go", "unixsock.cs", "ABg6oqaigpSmoqaCgpQAAhLilKQACRjCgpSqwoKUgpSqwoKUgpSosoKUgoKUqLKClIKClIKUAAIUAA4CgpSCgpSosoKUgoKUqLKClIKClIKClAACEgALAoKUgoKUpgABEOKmosaUgoKUgoKUAAwaoK7ygpSqwoKUgoKUqsKClIKClKrCgpSClKygmMKClAACGgAOAoKUgoKUrLLGlIKUgoKClKyyxpSClIKCgpQ=")]
[assembly: GoPositionMap("net/unixsock_posix.go", "unixsock_posix.cs", "AA0ggoKUtLS0ppSClIKUgvimgoKUpoKClKaCgpSmgoKUAAoGgpSkpKTIgqaigpSmgqaCgoKUgram4oKCgqaUgramooKUgpSClIKmooKUgoKClJSmooKCmIKClKaCgoKUpgANGIKCpqaCgoKUAAIWAAgCprKCgpiCgpSmooKCmIKClA==", "160-162:1;191-195:1;222-224:1;236-238:1")]
[assembly: GoPositionMap("net/unixsock_readmsg_cloexec.go", "unixsock_readmsg_cloexec.cs", "AAwagoKCprKCgoKUgg==")]
[assembly: GoPositionMap("net/writev_unix.go", "writev_unix.cs", "AAwcooKUgoKU5rKCgg==")]
// </GoSourcePositionMaps>

namespace go;

using static go.net_package;
using context = go.context_package;
using dnsmessage = go.vendor.golang.org.x.net.dns.dnsmessage_package;
using netip = go.net.netip_package;
using os = go.os_package;
using poll = go.@internal.poll_package;
using singleflight = go.@internal.singleflight_package;
using syscall = go.syscall_package;
using time = go.time_package;
using Δio = go.io_package;

[GoPackage("net")]
public static partial class net_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface buffersWriter {}
    internal partial interface sockaddr {}
    internal partial interface temporary {}
    internal partial interface timeout {}
    internal partial struct @file {}
    internal partial struct addrList {}
    internal partial struct addrPortUDPAddr {}
    internal partial struct addrinfoErrno {}
    internal partial struct byName {}
    internal partial struct byPref {}
    internal partial struct byPriorityWeight {}
    internal partial struct byRFC6724Info {}
    internal partial struct canceledError {}
    internal partial struct conf {}
    internal partial struct conn {}
    [GoLocalName("dialResult")] internal partial struct dialParallel_dialResult {}
    internal partial struct dnsConfig {}
    [GoLocalName("result")] internal partial struct doBlockingWithCtx_result<T> {}
    internal partial struct fileAddr {}
    [GoLocalName("result")] internal partial struct goLookupIPCNAMEOrder_result {}
    internal partial struct hostLookupOrder {}
    internal partial struct hostsᴛ1 {}
    internal partial struct ipAttr {}
    internal partial struct ipStackCapabilities {}
    internal partial struct ipv6ZoneCache {}
    internal partial struct listenerBacklogCacheᴛ1 {}
    internal partial struct mdnsTest {}
    internal partial struct mptcpStatusDial {}
    internal partial struct mptcpStatusListen {}
    internal partial struct netFD {}
    internal partial struct noReadFrom {}
    internal partial struct noWriteTo {}
    internal partial struct notFoundError {}
    internal partial struct nssConf {}
    internal partial struct nssCriterion {}
    internal partial struct nssSource {}
    internal partial struct nsswitchConfig {}
    internal partial struct onlyValuesCtx {}
    internal partial struct pipe {}
    internal partial struct pipeAddr {}
    internal partial struct pipeDeadline {}
    internal partial struct policyTable {}
    internal partial struct policyTableEntry {}
    internal partial struct probe_type {}
    internal partial struct rawConn {}
    internal partial struct rawListener {}
    internal partial struct resolverConfig {}
    internal partial struct sysDialer {}
    internal partial struct sysListener {}
    internal partial struct tcpConnWithoutReadFrom {}
    internal partial struct tcpConnWithoutWriteTo {}
    internal partial struct temporaryError {}
    internal partial struct timeoutError {}
    public partial interface Addr {}
    public partial interface Conn {}
    public partial interface Error {}
    public partial interface Listener {}
    public partial interface PacketConn {}
    public partial struct AddrError {}
    public partial struct Buffers {}
    public partial struct DNSConfigError {}
    public partial struct DNSError {}
    public partial struct Dialer {}
    public partial struct Flags {}
    public partial struct HardwareAddr {}
    public partial struct IP {}
    public partial struct IPAddr {}
    public partial struct IPConn {}
    public partial struct IPMask {}
    public partial struct IPNet {}
    public partial struct Interface {}
    public partial struct InvalidAddrError {}
    public partial struct KeepAliveConfig {}
    public partial struct ListenConfig {}
    public partial struct MX {}
    public partial struct NS {}
    public partial struct OpError {}
    public partial struct ParseError {}
    public partial struct Resolver {}
    public partial struct SRV {}
    public partial struct TCPAddr {}
    public partial struct TCPConn {}
    public partial struct TCPListener {}
    public partial struct UDPAddr {}
    public partial struct UDPConn {}
    public partial struct UnixAddr {}
    public partial struct UnixConn {}
    public partial struct UnixListener {}
    public partial struct UnknownNetworkError {}
    public partial struct scope {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸbytealg() => builtin.initPackage(typeof(@internal.bytealg_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸpoll() => builtin.initPackage(typeof(@internal.poll_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸroutebsd() => builtin.initPackage(typeof(@internal.routebsd_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsingleflight() => builtin.initPackage(typeof(@internal.singleflight_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸstrconv() => builtin.initPackage(typeof(@internal.strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸstringslite() => builtin.initPackage(typeof(@internal.stringslite_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyscallꓸunix() => builtin.initPackage(typeof(@internal.syscall.unix_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸnetip() => builtin.initPackage(typeof(net.netip_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸsyscall() => builtin.initPackage(typeof(syscall_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸnetꓸdnsꓸdnsmessage() => builtin.initPackage(typeof(vendor.golang.org.x.net.dns.dnsmessage_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(net_package.AddrError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛAddrError;
[GoMethodHost(typeof(net_package.Buffers)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛBuffers;
[GoMethodHost(typeof(net_package.DNSConfigError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛDNSConfigError;
[GoMethodHost(typeof(net_package.DNSError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛDNSError;
[GoMethodHost(typeof(net_package.Dialer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛDialer;
[GoMethodHost(typeof(net_package.Flags)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛFlags;
[GoMethodHost(typeof(net_package.HardwareAddr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛHardwareAddr;
[GoMethodHost(typeof(net_package.IP)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛIP;
[GoMethodHost(typeof(net_package.IPAddr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛIPAddr;
[GoMethodHost(typeof(net_package.IPConn)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛIPConn;
[GoMethodHost(typeof(net_package.IPMask)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛIPMask;
[GoMethodHost(typeof(net_package.IPNet)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛIPNet;
[GoMethodHost(typeof(net_package.Interface)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛInterface;
[GoMethodHost(typeof(net_package.InvalidAddrError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛInvalidAddrError;
[GoMethodHost(typeof(net_package.ListenConfig)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛListenConfig;
[GoMethodHost(typeof(net_package.OpError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛOpError;
[GoMethodHost(typeof(net_package.ParseError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛParseError;
[GoMethodHost(typeof(net_package.Resolver)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛResolver;
[GoMethodHost(typeof(net_package.TCPAddr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛTCPAddr;
[GoMethodHost(typeof(net_package.TCPConn)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛTCPConn;
[GoMethodHost(typeof(net_package.TCPListener)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛTCPListener;
[GoMethodHost(typeof(net_package.UDPAddr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛUDPAddr;
[GoMethodHost(typeof(net_package.UDPConn)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛUDPConn;
[GoMethodHost(typeof(net_package.UnixAddr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛUnixAddr;
[GoMethodHost(typeof(net_package.UnixConn)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛUnixConn;
[GoMethodHost(typeof(net_package.UnixListener)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛUnixListener;
[GoMethodHost(typeof(net_package.UnknownNetworkError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛUnknownNetworkError;
[GoMethodHost(typeof(net_package.addrList)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛaddrList;
[GoMethodHost(typeof(net_package.addrPortUDPAddr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛaddrPortUDPAddr;
[GoMethodHost(typeof(net_package.addrinfoErrno)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛaddrinfoErrno;
[GoMethodHost(typeof(net_package.byPref)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛbyPref;
[GoMethodHost(typeof(net_package.byPriorityWeight)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛbyPriorityWeight;
[GoMethodHost(typeof(net_package.canceledError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛcanceledError;
[GoMethodHost(typeof(net_package.conf)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛconf;
[GoMethodHost(typeof(net_package.conn)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛconn;
[GoMethodHost(typeof(net_package.dnsConfig)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛdnsConfig;
[GoMethodHost(typeof(net_package.@file)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛfile;
[GoMethodHost(typeof(net_package.fileAddr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛfileAddr;
[GoMethodHost(typeof(net_package.hostLookupOrder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛhostLookupOrder;
[GoMethodHost(typeof(net_package.ipStackCapabilities)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛipStackCapabilities;
[GoMethodHost(typeof(net_package.ipv6ZoneCache)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛipv6ZoneCache;
[GoMethodHost(typeof(net_package.mptcpStatusDial)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛmptcpStatusDial;
[GoMethodHost(typeof(net_package.mptcpStatusListen)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛmptcpStatusListen;
[GoMethodHost(typeof(net_package.netFD)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛnetFD;
[GoMethodHost(typeof(net_package.noReadFrom)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛnoReadFrom;
[GoMethodHost(typeof(net_package.noWriteTo)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛnoWriteTo;
[GoMethodHost(typeof(net_package.notFoundError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛnotFoundError;
[GoMethodHost(typeof(net_package.nssCriterion)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛnssCriterion;
[GoMethodHost(typeof(net_package.nssSource)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛnssSource;
[GoMethodHost(typeof(net_package.nsswitchConfig)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛnsswitchConfig;
[GoMethodHost(typeof(net_package.onlyValuesCtx)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛonlyValuesCtx;
[GoMethodHost(typeof(net_package.pipe)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛpipe;
[GoMethodHost(typeof(net_package.pipeAddr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛpipeAddr;
[GoMethodHost(typeof(net_package.pipeDeadline)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛpipeDeadline;
[GoMethodHost(typeof(net_package.policyTable)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛpolicyTable;
[GoMethodHost(typeof(net_package.rawConn)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛrawConn;
[GoMethodHost(typeof(net_package.rawListener)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛrawListener;
[GoMethodHost(typeof(net_package.resolverConfig)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛresolverConfig;
[GoMethodHost(typeof(net_package.sysDialer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛsysDialer;
[GoMethodHost(typeof(net_package.sysListener)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛsysListener;
[GoMethodHost(typeof(net_package.temporaryError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛtemporaryError;
[GoMethodHost(typeof(net_package.timeoutError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class netᴛtimeoutError;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class netᴛAddrError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ж<AddrError> Ꮡe) => net_package.Error⁔(Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Timeout(this ref AddrError e) => net_package.Timeout⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Temporary(this ref AddrError e) => net_package.Temporary⁔(ref e);
}
partial class netᴛBuffers {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (int64 n, error err) WriteTo(this ж<Buffers> Ꮡv, Δio.Writer w) => net_package.WriteTo⁔(Ꮡv, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) Read(this ref Buffers v, slice<byte> p) => net_package.Read⁔(ref v, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void consume(this ref Buffers v, int64 n) => net_package.consume⁔(ref v, n);
}
partial class netᴛDNSConfigError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Unwrap(this ref DNSConfigError e) => net_package.Unwrap⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref DNSConfigError e) => net_package.Error⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Timeout(this ref DNSConfigError e) => net_package.Timeout⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Temporary(this ref DNSConfigError e) => net_package.Temporary⁔(ref e);
}
partial class netᴛDNSError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Unwrap(this ref DNSError e) => net_package.Unwrap⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ж<DNSError> Ꮡe) => net_package.Error⁔(Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Timeout(this ref DNSError e) => net_package.Timeout⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Temporary(this ref DNSError e) => net_package.Temporary⁔(ref e);
}
partial class netᴛDialer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool dualStack(this ref Dialer d) => net_package.dualStack⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static time.Time /*earliest*/ deadline(this ref Dialer d, context.Context ctx, time.Time now) => net_package.deadline⁔(ref d, ctx, now);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Resolver> resolver(this ref Dialer d) => net_package.resolver⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static time.Duration fallbackDelay(this ref Dialer d) => net_package.fallbackDelay⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool MultipathTCP(this ref Dialer d) => net_package.MultipathTCP⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetMultipathTCP(this ref Dialer d, bool use) => net_package.SetMultipathTCP⁔(ref d, use);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (Conn, error) Dial(this ж<Dialer> Ꮡd, @string network, @string address) => net_package.Dial⁔(Ꮡd, network, address);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (Conn, error) DialContext(this ж<Dialer> Ꮡd, context.Context ctx, @string network, @string address) => net_package.DialContext⁔(Ꮡd, ctx, network, address);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (context.Context, Action) dialCtx(this ref Dialer d, context.Context ctx) => net_package.dialCtx⁔(ref d, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<TCPConn>, error) DialTCP(this ж<Dialer> Ꮡd, context.Context ctx, @string network, netip.AddrPort laddr, netip.AddrPort raddr) => net_package.DialTCP⁔(Ꮡd, ctx, network, laddr, raddr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<UDPConn>, error) DialUDP(this ж<Dialer> Ꮡd, context.Context ctx, @string network, netip.AddrPort laddr, netip.AddrPort raddr) => net_package.DialUDP⁔(Ꮡd, ctx, network, laddr, raddr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<IPConn>, error) DialIP(this ж<Dialer> Ꮡd, context.Context ctx, @string network, netip.Addr laddr, netip.Addr raddr) => net_package.DialIP⁔(Ꮡd, ctx, network, laddr, raddr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<UnixConn>, error) DialUnix(this ж<Dialer> Ꮡd, context.Context ctx, @string network, ж<UnixAddr> Ꮡladdr, ж<UnixAddr> Ꮡraddr) => net_package.DialUnix⁔(Ꮡd, ctx, network, Ꮡladdr, Ꮡraddr);
}
partial class netᴛFlags {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Flags f) => net_package.String⁔(f);
}
partial class netᴛHardwareAddr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this HardwareAddr a) => net_package.String⁔(a);
}
partial class netᴛIP {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsUnspecified(this IP ip) => net_package.IsUnspecified⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsLoopback(this IP ip) => net_package.IsLoopback⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsPrivate(this IP ip) => net_package.IsPrivate⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsMulticast(this IP ip) => net_package.IsMulticast⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsInterfaceLocalMulticast(this IP ip) => net_package.IsInterfaceLocalMulticast⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsLinkLocalMulticast(this IP ip) => net_package.IsLinkLocalMulticast⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsLinkLocalUnicast(this IP ip) => net_package.IsLinkLocalUnicast⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsGlobalUnicast(this IP ip) => net_package.IsGlobalUnicast⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static IP To4(this IP ip) => net_package.To4⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static IP To16(this IP ip) => net_package.To16⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static IPMask DefaultMask(this IP ip) => net_package.DefaultMask⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static IP Mask(this IP ip, IPMask mask) => net_package.Mask⁔(ip, mask);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this IP ip) => net_package.String⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> appendTo(this IP ip, slice<byte> b) => net_package.appendTo⁔(ip, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) AppendText(this IP ip, slice<byte> b) => net_package.AppendText⁔(ip, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) MarshalText(this IP ip) => net_package.MarshalText⁔(ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnmarshalText(this ref IP ip, slice<byte> text) => net_package.UnmarshalText⁔(ref ip, text);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Equal(this IP ip, IP x) => net_package.Equal⁔(ip, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool matchAddrFamily(this IP ip, IP x) => net_package.matchAddrFamily⁔(ip, x);
}
partial class netᴛIPAddr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Network(this ref IPAddr a) => net_package.Network⁔(ref a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<IPAddr> Ꮡa) => net_package.String⁔(Ꮡa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isWildcard(this ж<IPAddr> Ꮡa) => net_package.isWildcard⁔(Ꮡa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Addr opAddr(this ж<IPAddr> Ꮡa) => net_package.opAddr⁔(Ꮡa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint family(this ж<IPAddr> Ꮡa) => net_package.family⁔(Ꮡa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (syscall.Sockaddr, error) sockaddr(this ж<IPAddr> Ꮡa, nint family) => net_package.sockaddr⁔(Ꮡa, family);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static sockaddr toLocal(this ref IPAddr a, @string net) => net_package.toLocal⁔(ref a, net);
}
partial class netᴛIPConn {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (syscall.RawConn, error) SyscallConn(this ж<IPConn> Ꮡc) => net_package.SyscallConn⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, ж<IPAddr>, error) ReadFromIP(this ж<IPConn> Ꮡc, slice<byte> b) => net_package.ReadFromIP⁔(Ꮡc, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, Addr, error) ReadFrom(this ж<IPConn> Ꮡc, slice<byte> b) => net_package.ReadFrom⁔(Ꮡc, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, nint oobn, nint flags, ж<IPAddr> addr, error err) ReadMsgIP(this ж<IPConn> Ꮡc, slice<byte> b, slice<byte> oob) => net_package.ReadMsgIP⁔(Ꮡc, b, oob);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) WriteToIP(this ж<IPConn> Ꮡc, slice<byte> b, ж<IPAddr> Ꮡaddr) => net_package.WriteToIP⁔(Ꮡc, b, Ꮡaddr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) WriteTo(this ж<IPConn> Ꮡc, slice<byte> b, Addr addr) => net_package.WriteTo⁔(Ꮡc, b, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, nint oobn, error err) WriteMsgIP(this ж<IPConn> Ꮡc, slice<byte> b, slice<byte> oob, ж<IPAddr> Ꮡaddr) => net_package.WriteMsgIP⁔(Ꮡc, b, oob, Ꮡaddr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, ж<IPAddr>, error) readFrom(this ref IPConn c, slice<byte> b) => net_package.readFrom⁔(ref c, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, nint oobn, nint flags, ж<IPAddr> addr, error err) readMsg(this ref IPConn c, slice<byte> b, slice<byte> oob) => net_package.readMsg⁔(ref c, b, oob);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) writeTo(this ref IPConn c, slice<byte> b, ж<IPAddr> Ꮡaddr) => net_package.writeTo⁔(ref c, b, Ꮡaddr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, nint oobn, error err) writeMsg(this ref IPConn c, slice<byte> b, slice<byte> oob, ж<IPAddr> Ꮡaddr) => net_package.writeMsg⁔(ref c, b, oob, Ꮡaddr);
}
partial class netᴛIPMask {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint ones, nint bits) Size(this IPMask m) => net_package.Size⁔(m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this IPMask m) => net_package.String⁔(m);
}
partial class netᴛIPNet {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Contains(this ж<IPNet> Ꮡn, IP ip) => net_package.Contains⁔(Ꮡn, ip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Network(this ref IPNet n) => net_package.Network⁔(ref n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<IPNet> Ꮡn) => net_package.String⁔(Ꮡn);
}
partial class netᴛInterface {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<Addr>, error) Addrs(this ж<Interface> Ꮡifi) => net_package.Addrs⁔(Ꮡifi);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<Addr>, error) MulticastAddrs(this ж<Interface> Ꮡifi) => net_package.MulticastAddrs⁔(Ꮡifi);
}
partial class netᴛInvalidAddrError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this InvalidAddrError e) => net_package.Error⁔(e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Timeout(this InvalidAddrError e) => net_package.Timeout⁔(e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Temporary(this InvalidAddrError e) => net_package.Temporary⁔(e);
}
partial class netᴛListenConfig {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool MultipathTCP(this ref ListenConfig lc) => net_package.MultipathTCP⁔(ref lc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetMultipathTCP(this ref ListenConfig lc, bool use) => net_package.SetMultipathTCP⁔(ref lc, use);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (Listener, error) Listen(this ref ListenConfig lc, context.Context ctx, @string network, @string address) => net_package.Listen⁔(ref lc, ctx, network, address);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (PacketConn, error) ListenPacket(this ref ListenConfig lc, context.Context ctx, @string network, @string address) => net_package.ListenPacket⁔(ref lc, ctx, network, address);
}
partial class netᴛOpError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Unwrap(this ref OpError e) => net_package.Unwrap⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ж<OpError> Ꮡe) => net_package.Error⁔(Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Timeout(this ref OpError e) => net_package.Timeout⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Temporary(this ref OpError e) => net_package.Temporary⁔(ref e);
}
partial class netᴛParseError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref ParseError e) => net_package.Error⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Timeout(this ref ParseError e) => net_package.Timeout⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Temporary(this ref ParseError e) => net_package.Temporary⁔(ref e);
}
partial class netᴛResolver {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (addrList, error) resolveAddrList(this ж<Resolver> Ꮡr, context.Context ctx, @string op, @string network, @string addr, Addr hint) => net_package.resolveAddrList⁔(Ꮡr, ctx, op, network, addr, hint);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (dnsmessage.Parser, dnsmessage.Header, error) exchange(this ж<Resolver> Ꮡr, context.Context ctx, @string server, dnsmessage.Question q, time.Duration timeout, bool useTCP, bool ad) => net_package.exchange⁔(Ꮡr, ctx, server, q, timeout, useTCP, ad);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (dnsmessage.Parser, @string, error) tryOneName(this ж<Resolver> Ꮡr, context.Context ctx, ж<dnsConfig> Ꮡcfg, @string name, dnsmessage.Type qtype) => net_package.tryOneName⁔(Ꮡr, ctx, Ꮡcfg, name, qtype);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (dnsmessage.Parser, @string, error) lookup(this ж<Resolver> Ꮡr, context.Context ctx, @string name, dnsmessage.Type qtype, ж<dnsConfig> Ꮡconf) => net_package.lookup⁔(Ꮡr, ctx, name, qtype, Ꮡconf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<@string> addrs, error err) goLookupHostOrder(this ж<Resolver> Ꮡr, context.Context ctx, @string name, hostLookupOrder order, ж<dnsConfig> Ꮡconf) => net_package.goLookupHostOrder⁔(Ꮡr, ctx, name, order, Ꮡconf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<IPAddr> addrs, error err) goLookupIP(this ж<Resolver> Ꮡr, context.Context ctx, @string network, @string host, hostLookupOrder order, ж<dnsConfig> Ꮡconf) => net_package.goLookupIP⁔(Ꮡr, ctx, network, host, order, Ꮡconf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<IPAddr> addrs, dnsmessage.Name cname, error err) goLookupIPCNAMEOrder(this ж<Resolver> Ꮡr, context.Context ctx, @string network, @string name, hostLookupOrder order, ж<dnsConfig> Ꮡconf) => net_package.goLookupIPCNAMEOrder⁔(Ꮡr, ctx, network, name, order, Ꮡconf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string, error) goLookupCNAME(this ж<Resolver> Ꮡr, context.Context ctx, @string host, hostLookupOrder order, ж<dnsConfig> Ꮡconf) => net_package.goLookupCNAME⁔(Ꮡr, ctx, host, order, Ꮡconf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<@string>, error) goLookupPTR(this ж<Resolver> Ꮡr, context.Context ctx, @string addr, hostLookupOrder order, ж<dnsConfig> Ꮡconf) => net_package.goLookupPTR⁔(Ꮡr, ctx, addr, order, Ꮡconf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (addrList, error) internetAddrList(this ж<Resolver> Ꮡr, context.Context ctx, @string net, @string addr) => net_package.internetAddrList⁔(Ꮡr, ctx, net, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool preferGo(this ж<Resolver> Ꮡr) => net_package.preferGo⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool strictErrors(this ж<Resolver> Ꮡr) => net_package.strictErrors⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<singleflight.Group> getLookupGroup(this ж<Resolver> Ꮡr) => net_package.getLookupGroup⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<@string> addrs, error err) LookupHost(this ж<Resolver> Ꮡr, context.Context ctx, @string host) => net_package.LookupHost⁔(Ꮡr, ctx, host);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<IPAddr>, error) LookupIPAddr(this ж<Resolver> Ꮡr, context.Context ctx, @string host) => net_package.LookupIPAddr⁔(Ꮡr, ctx, host);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<IP>, error) LookupIP(this ж<Resolver> Ꮡr, context.Context ctx, @string network, @string host) => net_package.LookupIP⁔(Ꮡr, ctx, network, host);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<netip.Addr>, error) LookupNetIP(this ж<Resolver> Ꮡr, context.Context ctx, @string network, @string host) => net_package.LookupNetIP⁔(Ꮡr, ctx, network, host);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<IPAddr>, error) lookupIPAddr(this ж<Resolver> Ꮡr, context.Context ctx, @string network, @string host) => net_package.lookupIPAddr⁔(Ꮡr, ctx, network, host);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint port, error err) LookupPort(this ж<Resolver> Ꮡr, context.Context ctx, @string network, @string service) => net_package.LookupPort⁔(Ꮡr, ctx, network, service);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (@string, error) LookupCNAME(this ж<Resolver> Ꮡr, context.Context ctx, @string host) => net_package.LookupCNAME⁔(Ꮡr, ctx, host);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (@string, slice<ж<SRV>>, error) LookupSRV(this ж<Resolver> Ꮡr, context.Context ctx, @string service, @string proto, @string name) => net_package.LookupSRV⁔(Ꮡr, ctx, service, proto, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<ж<MX>>, error) LookupMX(this ж<Resolver> Ꮡr, context.Context ctx, @string name) => net_package.LookupMX⁔(Ꮡr, ctx, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<ж<NS>>, error) LookupNS(this ж<Resolver> Ꮡr, context.Context ctx, @string name) => net_package.LookupNS⁔(Ꮡr, ctx, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<@string>, error) LookupTXT(this ж<Resolver> Ꮡr, context.Context ctx, @string name) => net_package.LookupTXT⁔(Ꮡr, ctx, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<@string>, error) LookupAddr(this ж<Resolver> Ꮡr, context.Context ctx, @string addr) => net_package.LookupAddr⁔(Ꮡr, ctx, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (Conn, error) dial(this ж<Resolver> Ꮡr, context.Context ctx, @string network, @string server) => net_package.dial⁔(Ꮡr, ctx, network, server);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string target, slice<ж<SRV>> srvs, error err) goLookupSRV(this ж<Resolver> Ꮡr, context.Context ctx, @string service, @string proto, @string name) => net_package.goLookupSRV⁔(Ꮡr, ctx, service, proto, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<ж<MX>>, error) goLookupMX(this ж<Resolver> Ꮡr, context.Context ctx, @string name) => net_package.goLookupMX⁔(Ꮡr, ctx, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<ж<NS>>, error) goLookupNS(this ж<Resolver> Ꮡr, context.Context ctx, @string name) => net_package.goLookupNS⁔(Ꮡr, ctx, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<@string>, error) goLookupTXT(this ж<Resolver> Ꮡr, context.Context ctx, @string name) => net_package.goLookupTXT⁔(Ꮡr, ctx, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<@string> addrs, error err) lookupHost(this ж<Resolver> Ꮡr, context.Context ctx, @string host) => net_package.lookupHost⁔(Ꮡr, ctx, host);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<IPAddr> addrs, error err) lookupIP(this ж<Resolver> Ꮡr, context.Context ctx, @string network, @string host) => net_package.lookupIP⁔(Ꮡr, ctx, network, host);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) lookupPort(this ж<Resolver> Ꮡr, context.Context ctx, @string network, @string service) => net_package.lookupPort⁔(Ꮡr, ctx, network, service);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string, error) lookupCNAME(this ж<Resolver> Ꮡr, context.Context ctx, @string name) => net_package.lookupCNAME⁔(Ꮡr, ctx, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string, slice<ж<SRV>>, error) lookupSRV(this ж<Resolver> Ꮡr, context.Context ctx, @string service, @string proto, @string name) => net_package.lookupSRV⁔(Ꮡr, ctx, service, proto, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<ж<MX>>, error) lookupMX(this ж<Resolver> Ꮡr, context.Context ctx, @string name) => net_package.lookupMX⁔(Ꮡr, ctx, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<ж<NS>>, error) lookupNS(this ж<Resolver> Ꮡr, context.Context ctx, @string name) => net_package.lookupNS⁔(Ꮡr, ctx, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<@string>, error) lookupTXT(this ж<Resolver> Ꮡr, context.Context ctx, @string name) => net_package.lookupTXT⁔(Ꮡr, ctx, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<@string>, error) lookupAddr(this ж<Resolver> Ꮡr, context.Context ctx, @string addr) => net_package.lookupAddr⁔(Ꮡr, ctx, addr);
}
partial class netᴛTCPAddr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static netip.AddrPort AddrPort(this ж<TCPAddr> Ꮡa) => net_package.AddrPort⁔(Ꮡa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Network(this ref TCPAddr a) => net_package.Network⁔(ref a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<TCPAddr> Ꮡa) => net_package.String⁔(Ꮡa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isWildcard(this ж<TCPAddr> Ꮡa) => net_package.isWildcard⁔(Ꮡa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Addr opAddr(this ж<TCPAddr> Ꮡa) => net_package.opAddr⁔(Ꮡa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint family(this ж<TCPAddr> Ꮡa) => net_package.family⁔(Ꮡa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (syscall.Sockaddr, error) sockaddr(this ж<TCPAddr> Ꮡa, nint family) => net_package.sockaddr⁔(Ꮡa, family);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static sockaddr toLocal(this ref TCPAddr a, @string net) => net_package.toLocal⁔(ref a, net);
}
partial class netᴛTCPConn {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (syscall.RawConn, error) SyscallConn(this ж<TCPConn> Ꮡc) => net_package.SyscallConn⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (int64, error) ReadFrom(this ж<TCPConn> Ꮡc, Δio.Reader r) => net_package.ReadFrom⁔(Ꮡc, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (int64, error) WriteTo(this ж<TCPConn> Ꮡc, Δio.Writer w) => net_package.WriteTo⁔(Ꮡc, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error CloseRead(this ж<TCPConn> Ꮡc) => net_package.CloseRead⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error CloseWrite(this ж<TCPConn> Ꮡc) => net_package.CloseWrite⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SetLinger(this ж<TCPConn> Ꮡc, nint sec) => net_package.SetLinger⁔(Ꮡc, sec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SetKeepAlive(this ж<TCPConn> Ꮡc, bool keepalive) => net_package.SetKeepAlive⁔(Ꮡc, keepalive);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SetKeepAlivePeriod(this ж<TCPConn> Ꮡc, time.Duration d) => net_package.SetKeepAlivePeriod⁔(Ꮡc, d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SetNoDelay(this ж<TCPConn> Ꮡc, bool noDelay) => net_package.SetNoDelay⁔(Ꮡc, noDelay);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (bool, error) MultipathTCP(this ж<TCPConn> Ꮡc) => net_package.MultipathTCP⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64, error) readFrom(this ж<TCPConn> Ꮡc, Δio.Reader r) => net_package.readFrom⁔(Ꮡc, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64, error) writeTo(this ж<TCPConn> Ꮡc, Δio.Writer w) => net_package.writeTo⁔(Ꮡc, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SetKeepAliveConfig(this ж<TCPConn> Ꮡc, KeepAliveConfig config) => net_package.SetKeepAliveConfig⁔(Ꮡc, config);
}
partial class netᴛTCPListener {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (syscall.RawConn, error) SyscallConn(this ж<TCPListener> Ꮡl) => net_package.SyscallConn⁔(Ꮡl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<TCPConn>, error) AcceptTCP(this ж<TCPListener> Ꮡl) => net_package.AcceptTCP⁔(Ꮡl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (Conn, error) Accept(this ж<TCPListener> Ꮡl) => net_package.Accept⁔(Ꮡl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this ж<TCPListener> Ꮡl) => net_package.Close⁔(Ꮡl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Addr Addr(this ref TCPListener l) => net_package.Addr⁔(ref l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SetDeadline(this ж<TCPListener> Ꮡl, time.Time t) => net_package.SetDeadline⁔(Ꮡl, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<os.File> f, error err) File(this ж<TCPListener> Ꮡl) => net_package.File⁔(Ꮡl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool ok(this ж<TCPListener> Ꮡln) => net_package.ok⁔(Ꮡln);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<TCPConn>, error) accept(this ref TCPListener ln) => net_package.accept⁔(ref ln);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error close(this ref TCPListener ln) => net_package.close⁔(ref ln);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<os.File>, error) @file(this ref TCPListener ln) => net_package.file⁔(ref ln);
}
partial class netᴛUDPAddr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static netip.AddrPort AddrPort(this ж<UDPAddr> Ꮡa) => net_package.AddrPort⁔(Ꮡa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Network(this ref UDPAddr a) => net_package.Network⁔(ref a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<UDPAddr> Ꮡa) => net_package.String⁔(Ꮡa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isWildcard(this ж<UDPAddr> Ꮡa) => net_package.isWildcard⁔(Ꮡa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Addr opAddr(this ж<UDPAddr> Ꮡa) => net_package.opAddr⁔(Ꮡa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint family(this ж<UDPAddr> Ꮡa) => net_package.family⁔(Ꮡa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (syscall.Sockaddr, error) sockaddr(this ж<UDPAddr> Ꮡa, nint family) => net_package.sockaddr⁔(Ꮡa, family);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static sockaddr toLocal(this ref UDPAddr a, @string net) => net_package.toLocal⁔(ref a, net);
}
partial class netᴛUDPConn {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (syscall.RawConn, error) SyscallConn(this ж<UDPConn> Ꮡc) => net_package.SyscallConn⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, ж<UDPAddr> addr, error err) ReadFromUDP(this ж<UDPConn> Ꮡc, slice<byte> b) => net_package.ReadFromUDP⁔(Ꮡc, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, ж<UDPAddr>, error) readFromUDP(this ж<UDPConn> Ꮡc, slice<byte> b, ж<UDPAddr> Ꮡaddr) => net_package.readFromUDP⁔(Ꮡc, b, Ꮡaddr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, Addr, error) ReadFrom(this ж<UDPConn> Ꮡc, slice<byte> b) => net_package.ReadFrom⁔(Ꮡc, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, netip.AddrPort addr, error err) ReadFromUDPAddrPort(this ж<UDPConn> Ꮡc, slice<byte> b) => net_package.ReadFromUDPAddrPort⁔(Ꮡc, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, nint oobn, nint flags, ж<UDPAddr> addr, error err) ReadMsgUDP(this ж<UDPConn> Ꮡc, slice<byte> b, slice<byte> oob) => net_package.ReadMsgUDP⁔(Ꮡc, b, oob);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, nint oobn, nint flags, netip.AddrPort addr, error err) ReadMsgUDPAddrPort(this ж<UDPConn> Ꮡc, slice<byte> b, slice<byte> oob) => net_package.ReadMsgUDPAddrPort⁔(Ꮡc, b, oob);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) WriteToUDP(this ж<UDPConn> Ꮡc, slice<byte> b, ж<UDPAddr> Ꮡaddr) => net_package.WriteToUDP⁔(Ꮡc, b, Ꮡaddr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) WriteToUDPAddrPort(this ж<UDPConn> Ꮡc, slice<byte> b, netip.AddrPort addr) => net_package.WriteToUDPAddrPort⁔(Ꮡc, b, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) WriteTo(this ж<UDPConn> Ꮡc, slice<byte> b, Addr addr) => net_package.WriteTo⁔(Ꮡc, b, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, nint oobn, error err) WriteMsgUDP(this ж<UDPConn> Ꮡc, slice<byte> b, slice<byte> oob, ж<UDPAddr> Ꮡaddr) => net_package.WriteMsgUDP⁔(Ꮡc, b, oob, Ꮡaddr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, nint oobn, error err) WriteMsgUDPAddrPort(this ж<UDPConn> Ꮡc, slice<byte> b, slice<byte> oob, netip.AddrPort addr) => net_package.WriteMsgUDPAddrPort⁔(Ꮡc, b, oob, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, ж<UDPAddr>, error) readFrom(this ref UDPConn c, slice<byte> b, ж<UDPAddr> Ꮡaddr) => net_package.readFrom⁔(ref c, b, Ꮡaddr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, netip.AddrPort addr, error err) readFromAddrPort(this ref UDPConn c, slice<byte> b) => net_package.readFromAddrPort⁔(ref c, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, nint oobn, nint flags, netip.AddrPort addr, error err) readMsg(this ref UDPConn c, slice<byte> b, slice<byte> oob) => net_package.readMsg⁔(ref c, b, oob);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) writeTo(this ref UDPConn c, slice<byte> b, ж<UDPAddr> Ꮡaddr) => net_package.writeTo⁔(ref c, b, Ꮡaddr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) writeToAddrPort(this ref UDPConn c, slice<byte> b, netip.AddrPort addr) => net_package.writeToAddrPort⁔(ref c, b, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, nint oobn, error err) writeMsg(this ref UDPConn c, slice<byte> b, slice<byte> oob, ж<UDPAddr> Ꮡaddr) => net_package.writeMsg⁔(ref c, b, oob, Ꮡaddr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, nint oobn, error err) writeMsgAddrPort(this ref UDPConn c, slice<byte> b, slice<byte> oob, netip.AddrPort addr) => net_package.writeMsgAddrPort⁔(ref c, b, oob, addr);
}
partial class netᴛUnixAddr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Network(this ref UnixAddr a) => net_package.Network⁔(ref a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<UnixAddr> Ꮡa) => net_package.String⁔(Ꮡa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isWildcard(this ж<UnixAddr> Ꮡa) => net_package.isWildcard⁔(Ꮡa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Addr opAddr(this ж<UnixAddr> Ꮡa) => net_package.opAddr⁔(Ꮡa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint family(this ref UnixAddr a) => net_package.family⁔(ref a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (syscall.Sockaddr, error) sockaddr(this ж<UnixAddr> Ꮡa, nint family) => net_package.sockaddr⁔(Ꮡa, family);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static sockaddr toLocal(this ж<UnixAddr> Ꮡa, @string net) => net_package.toLocal⁔(Ꮡa, net);
}
partial class netᴛUnixConn {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (syscall.RawConn, error) SyscallConn(this ж<UnixConn> Ꮡc) => net_package.SyscallConn⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error CloseRead(this ж<UnixConn> Ꮡc) => net_package.CloseRead⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error CloseWrite(this ж<UnixConn> Ꮡc) => net_package.CloseWrite⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, ж<UnixAddr>, error) ReadFromUnix(this ж<UnixConn> Ꮡc, slice<byte> b) => net_package.ReadFromUnix⁔(Ꮡc, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, Addr, error) ReadFrom(this ж<UnixConn> Ꮡc, slice<byte> b) => net_package.ReadFrom⁔(Ꮡc, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, nint oobn, nint flags, ж<UnixAddr> addr, error err) ReadMsgUnix(this ж<UnixConn> Ꮡc, slice<byte> b, slice<byte> oob) => net_package.ReadMsgUnix⁔(Ꮡc, b, oob);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) WriteToUnix(this ж<UnixConn> Ꮡc, slice<byte> b, ж<UnixAddr> Ꮡaddr) => net_package.WriteToUnix⁔(Ꮡc, b, Ꮡaddr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) WriteTo(this ж<UnixConn> Ꮡc, slice<byte> b, Addr addr) => net_package.WriteTo⁔(Ꮡc, b, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, nint oobn, error err) WriteMsgUnix(this ж<UnixConn> Ꮡc, slice<byte> b, slice<byte> oob, ж<UnixAddr> Ꮡaddr) => net_package.WriteMsgUnix⁔(Ꮡc, b, oob, Ꮡaddr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, ж<UnixAddr>, error) readFrom(this ref UnixConn c, slice<byte> b) => net_package.readFrom⁔(ref c, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, nint oobn, nint flags, ж<UnixAddr> addr, error err) readMsg(this ref UnixConn c, slice<byte> b, slice<byte> oob) => net_package.readMsg⁔(ref c, b, oob);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) writeTo(this ref UnixConn c, slice<byte> b, ж<UnixAddr> Ꮡaddr) => net_package.writeTo⁔(ref c, b, Ꮡaddr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, nint oobn, error err) writeMsg(this ref UnixConn c, slice<byte> b, slice<byte> oob, ж<UnixAddr> Ꮡaddr) => net_package.writeMsg⁔(ref c, b, oob, Ꮡaddr);
}
partial class netᴛUnixListener {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool ok(this ж<UnixListener> Ꮡln) => net_package.ok⁔(Ꮡln);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (syscall.RawConn, error) SyscallConn(this ж<UnixListener> Ꮡl) => net_package.SyscallConn⁔(Ꮡl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<UnixConn>, error) AcceptUnix(this ж<UnixListener> Ꮡl) => net_package.AcceptUnix⁔(Ꮡl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (Conn, error) Accept(this ж<UnixListener> Ꮡl) => net_package.Accept⁔(Ꮡl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this ж<UnixListener> Ꮡl) => net_package.Close⁔(Ꮡl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Addr Addr(this ref UnixListener l) => net_package.Addr⁔(ref l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SetDeadline(this ж<UnixListener> Ꮡl, time.Time t) => net_package.SetDeadline⁔(Ꮡl, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<os.File> f, error err) File(this ж<UnixListener> Ꮡl) => net_package.File⁔(Ꮡl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<UnixConn>, error) accept(this ref UnixListener ln) => net_package.accept⁔(ref ln);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error close(this ж<UnixListener> Ꮡln) => net_package.close⁔(Ꮡln);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<os.File>, error) @file(this ref UnixListener ln) => net_package.file⁔(ref ln);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetUnlinkOnClose(this ref UnixListener l, bool unlink) => net_package.SetUnlinkOnClose⁔(ref l, unlink);
}
partial class netᴛUnknownNetworkError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this UnknownNetworkError e) => net_package.Error⁔(e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Timeout(this UnknownNetworkError e) => net_package.Timeout⁔(e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Temporary(this UnknownNetworkError e) => net_package.Temporary⁔(e);
}
partial class netᴛaddrList {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Addr forResolve(this addrList addrs, @string network, @string addr) => net_package.forResolve⁔(addrs, network, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Addr first(this addrList addrs, Func<Addr, bool> strategy) => net_package.first⁔(addrs, strategy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (addrList primaries, addrList fallbacks) partition(this addrList addrs, Func<Addr, bool> strategy) => net_package.partition⁔(addrs, strategy);
}
partial class netᴛaddrPortUDPAddr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Network(this addrPortUDPAddr _) => net_package.Network⁔(_);
}
partial class netᴛaddrinfoErrno {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this addrinfoErrno eai) => net_package.Error⁔(eai);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Temporary(this addrinfoErrno eai) => net_package.Temporary⁔(eai);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Timeout(this addrinfoErrno eai) => net_package.Timeout⁔(eai);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void isAddrinfoErrno(this addrinfoErrno eai) => net_package.isAddrinfoErrno⁔(eai);
}
partial class netᴛbyPref {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void sort(this byPref s) => net_package.sort⁔(s);
}
partial class netᴛbyPriorityWeight {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void shuffleByWeight(this byPriorityWeight addrs) => net_package.shuffleByWeight⁔(addrs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void sort(this byPriorityWeight addrs) => net_package.sort⁔(addrs);
}
partial class netᴛcanceledError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this canceledError _) => net_package.Error⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Is(this canceledError _, error err) => net_package.Is⁔(_, err);
}
partial class netᴛconf {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool mustUseGoResolver(this ref conf c, ж<Resolver> Ꮡr) => net_package.mustUseGoResolver⁔(ref c, Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (hostLookupOrder ret, ж<dnsConfig> dnsConf) addrLookupOrder(this ж<conf> Ꮡc, ж<Resolver> Ꮡr, @string addr) => net_package.addrLookupOrder⁔(Ꮡc, Ꮡr, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (hostLookupOrder ret, ж<dnsConfig> dnsConf) hostLookupOrder(this ж<conf> Ꮡc, ж<Resolver> Ꮡr, @string hostname) => net_package.hostLookupOrder⁔(Ꮡc, Ꮡr, hostname);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (hostLookupOrder ret, ж<dnsConfig> dnsConf) lookupOrder(this ref conf c, ж<Resolver> Ꮡr, @string hostname) => net_package.lookupOrder⁔(ref c, Ꮡr, hostname);
}
partial class netᴛconn {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool ok(this ж<conn> Ꮡc) => net_package.ok⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this ж<conn> Ꮡc, slice<byte> b) => net_package.Read⁔(Ꮡc, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Write(this ж<conn> Ꮡc, slice<byte> b) => net_package.Write⁔(Ꮡc, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ж<conn> Ꮡc) => net_package.Close⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Addr LocalAddr(this ж<conn> Ꮡc) => net_package.LocalAddr⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Addr RemoteAddr(this ж<conn> Ꮡc) => net_package.RemoteAddr⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error SetDeadline(this ж<conn> Ꮡc, time.Time t) => net_package.SetDeadline⁔(Ꮡc, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error SetReadDeadline(this ж<conn> Ꮡc, time.Time t) => net_package.SetReadDeadline⁔(Ꮡc, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error SetWriteDeadline(this ж<conn> Ꮡc, time.Time t) => net_package.SetWriteDeadline⁔(Ꮡc, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error SetReadBuffer(this ж<conn> Ꮡc, nint bytes) => net_package.SetReadBuffer⁔(Ꮡc, bytes);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error SetWriteBuffer(this ж<conn> Ꮡc, nint bytes) => net_package.SetWriteBuffer⁔(Ꮡc, bytes);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<os.File> f, error err) File(this ref conn c) => net_package.File⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64, error) writeBuffers(this ж<conn> Ꮡc, ж<Buffers> Ꮡv) => net_package.writeBuffers⁔(Ꮡc, Ꮡv);
}
partial class netᴛdnsConfig {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<@string> nameList(this ref dnsConfig conf, @string name) => net_package.nameList⁔(ref conf, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 serverOffset(this ж<dnsConfig> Ꮡc) => net_package.serverOffset⁔(Ꮡc);
}
partial class netᴛfile {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void close(this ref @file f) => net_package.close⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string s, bool ok) getLineFromData(this ref @file f) => net_package.getLineFromData⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string s, bool ok) readLine(this ref @file f) => net_package.readLine⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (time.Time mtime, int64 size, error err) stat(this ref @file f) => net_package.stat⁔(ref f);
}
partial class netᴛfileAddr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Network(this fileAddr _) => net_package.Network⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this fileAddr f) => net_package.String⁔(f);
}
partial class netᴛhostLookupOrder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this hostLookupOrder o) => net_package.String⁔(o);
}
partial class netᴛipStackCapabilities {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void probe(this ж<ipStackCapabilities> Ꮡp) => net_package.probe⁔(Ꮡp);
}
partial class netᴛipv6ZoneCache {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool /*updated*/ update(this ж<ipv6ZoneCache> Ꮡzc, slice<Interface> ift, bool force) => net_package.update⁔(Ꮡzc, ift, force);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string name(this ref ipv6ZoneCache zc, nint index) => net_package.name⁔(ref zc, index);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint index(this ref ipv6ZoneCache zc, @string name) => net_package.index⁔(ref zc, name);
}
partial class netᴛmptcpStatusDial {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool get(this ref mptcpStatusDial m) => net_package.get⁔(ref m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void set(this ref mptcpStatusDial m, bool use) => net_package.set⁔(ref m, use);
}
partial class netᴛmptcpStatusListen {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool get(this ref mptcpStatusListen m) => net_package.get⁔(ref m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void set(this ref mptcpStatusListen m, bool use) => net_package.set⁔(ref m, use);
}
partial class netᴛnetFD {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string name(this ref netFD fd) => net_package.name⁔(ref fd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setAddr(this ж<netFD> Ꮡfd, Addr laddr, Addr raddr) => net_package.setAddr⁔(Ꮡfd, laddr, raddr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ж<netFD> Ꮡfd) => net_package.Close⁔(Ꮡfd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error shutdown(this ж<netFD> Ꮡfd, nint how) => net_package.shutdown⁔(Ꮡfd, how);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error closeRead(this ж<netFD> Ꮡfd) => net_package.closeRead⁔(Ꮡfd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error closeWrite(this ж<netFD> Ꮡfd) => net_package.closeWrite⁔(Ꮡfd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this ж<netFD> Ꮡfd, slice<byte> p) => net_package.Read⁔(Ꮡfd, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, syscall.Sockaddr sa, error err) readFrom(this ж<netFD> Ꮡfd, slice<byte> p) => net_package.readFrom⁔(Ꮡfd, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) readFromInet4(this ж<netFD> Ꮡfd, slice<byte> p, ж<syscall.SockaddrInet4> Ꮡfrom) => net_package.readFromInet4⁔(Ꮡfd, p, Ꮡfrom);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) readFromInet6(this ж<netFD> Ꮡfd, slice<byte> p, ж<syscall.SockaddrInet6> Ꮡfrom) => net_package.readFromInet6⁔(Ꮡfd, p, Ꮡfrom);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, nint oobn, nint retflags, syscall.Sockaddr sa, error err) readMsg(this ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags) => net_package.readMsg⁔(Ꮡfd, p, oob, flags);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, nint oobn, nint retflags, error err) readMsgInet4(this ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags, ж<syscall.SockaddrInet4> Ꮡsa) => net_package.readMsgInet4⁔(Ꮡfd, p, oob, flags, Ꮡsa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, nint oobn, nint retflags, error err) readMsgInet6(this ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags, ж<syscall.SockaddrInet6> Ꮡsa) => net_package.readMsgInet6⁔(Ꮡfd, p, oob, flags, Ꮡsa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint nn, error err) Write(this ж<netFD> Ꮡfd, slice<byte> p) => net_package.Write⁔(Ꮡfd, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) writeTo(this ж<netFD> Ꮡfd, slice<byte> p, syscall.Sockaddr sa) => net_package.writeTo⁔(Ꮡfd, p, sa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) writeToInet4(this ж<netFD> Ꮡfd, slice<byte> p, ж<syscall.SockaddrInet4> Ꮡsa) => net_package.writeToInet4⁔(Ꮡfd, p, Ꮡsa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) writeToInet6(this ж<netFD> Ꮡfd, slice<byte> p, ж<syscall.SockaddrInet6> Ꮡsa) => net_package.writeToInet6⁔(Ꮡfd, p, Ꮡsa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, nint oobn, error err) writeMsg(this ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, syscall.Sockaddr sa) => net_package.writeMsg⁔(Ꮡfd, p, oob, sa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, nint oobn, error err) writeMsgInet4(this ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, ж<syscall.SockaddrInet4> Ꮡsa) => net_package.writeMsgInet4⁔(Ꮡfd, p, oob, Ꮡsa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, nint oobn, error err) writeMsgInet6(this ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, ж<syscall.SockaddrInet6> Ꮡsa) => net_package.writeMsgInet6⁔(Ꮡfd, p, oob, Ꮡsa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error SetDeadline(this ж<netFD> Ꮡfd, time.Time t) => net_package.SetDeadline⁔(Ꮡfd, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error SetReadDeadline(this ж<netFD> Ꮡfd, time.Time t) => net_package.SetReadDeadline⁔(Ꮡfd, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error SetWriteDeadline(this ж<netFD> Ꮡfd, time.Time t) => net_package.SetWriteDeadline⁔(Ꮡfd, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error init(this ж<netFD> Ꮡfd) => net_package.init⁔(Ꮡfd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (syscall.Sockaddr rsa, error ret) connect(this ж<netFD> Ꮡfd, context.Context ctx, syscall.Sockaddr la, syscall.Sockaddr ra) => net_package.connect⁔(Ꮡfd, ctx, la, ra);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<netFD> netfd, error err) accept(this ж<netFD> Ꮡfd) => net_package.accept⁔(Ꮡfd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<os.File> f, error err) dup(this ж<netFD> Ꮡfd) => net_package.dup⁔(Ꮡfd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string ctrlNetwork(this ref netFD fd) => net_package.ctrlNetwork⁔(ref fd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error dial(this ж<netFD> Ꮡfd, context.Context ctx, sockaddr laddr, sockaddr raddr, Func<context.Context, @string, @string, syscall.RawConn, error> ctrlCtxFn) => net_package.dial⁔(Ꮡfd, ctx, laddr, raddr, ctrlCtxFn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error listenStream(this ж<netFD> Ꮡfd, context.Context ctx, sockaddr laddr, nint backlog, Func<context.Context, @string, @string, syscall.RawConn, error> ctrlCtxFn) => net_package.listenStream⁔(Ꮡfd, ctx, laddr, backlog, ctrlCtxFn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error listenDatagram(this ж<netFD> Ꮡfd, context.Context ctx, sockaddr laddr, Func<context.Context, @string, @string, syscall.RawConn, error> ctrlCtxFn) => net_package.listenDatagram⁔(Ꮡfd, ctx, laddr, ctrlCtxFn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Func<syscall.Sockaddr, Addr> addrFunc(this ref netFD fd) => net_package.addrFunc⁔(ref fd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64 n, error err) writeBuffers(this ж<netFD> Ꮡfd, ж<Buffers> Ꮡv) => net_package.writeBuffers⁔(Ꮡfd, Ꮡv);
}
partial class netᴛnoReadFrom {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64, error) ReadFrom(this noReadFrom _Δp0, Δio.Reader _Δp1) => net_package.ReadFrom⁔(_Δp0, _Δp1);
}
partial class netᴛnoWriteTo {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64, error) WriteTo(this noWriteTo _Δp0, Δio.Writer _Δp1) => net_package.WriteTo⁔(_Δp0, _Δp1);
}
partial class netᴛnotFoundError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this ref notFoundError e) => net_package.Error⁔(ref e);
}
partial class netᴛnssCriterion {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool standardStatusAction(this nssCriterion c, bool last) => net_package.standardStatusAction⁔(c, last);
}
partial class netᴛnssSource {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool standardCriteria(this nssSource s) => net_package.standardCriteria⁔(s);
}
partial class netᴛnsswitchConfig {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ref nsswitchConfig conf) => net_package.init⁔(ref conf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void tryUpdate(this ж<nsswitchConfig> Ꮡconf) => net_package.tryUpdate⁔(Ꮡconf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void acquireSema(this ref nsswitchConfig conf) => net_package.acquireSema⁔(ref conf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool tryAcquireSema(this ref nsswitchConfig conf) => net_package.tryAcquireSema⁔(ref conf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void releaseSema(this ref nsswitchConfig conf) => net_package.releaseSema⁔(ref conf);
}
partial class netᴛonlyValuesCtx {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (time.Time, bool) Deadline(this onlyValuesCtx recvᴛ) => net_package.Deadline⁔(recvᴛ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static /*<-*/channel<EmptyStruct> Done(this onlyValuesCtx recvᴛ) => net_package.Done⁔(recvᴛ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Err(this onlyValuesCtx recvᴛ) => net_package.Err⁔(recvᴛ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static any Value(this ref onlyValuesCtx ovc, any key) => net_package.Value⁔(ref ovc, key);
}
partial class netᴛpipe {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Addr LocalAddr(this ref pipe _) => net_package.LocalAddr⁔(ref _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Addr RemoteAddr(this ref pipe _) => net_package.RemoteAddr⁔(ref _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this ж<pipe> Ꮡp, slice<byte> b) => net_package.Read⁔(Ꮡp, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) read(this ж<pipe> Ꮡp, slice<byte> b) => net_package.read⁔(Ꮡp, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Write(this ж<pipe> Ꮡp, slice<byte> b) => net_package.Write⁔(Ꮡp, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) write(this ж<pipe> Ꮡp, slice<byte> b) => net_package.write⁔(Ꮡp, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error SetDeadline(this ж<pipe> Ꮡp, time.Time t) => net_package.SetDeadline⁔(Ꮡp, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error SetReadDeadline(this ж<pipe> Ꮡp, time.Time t) => net_package.SetReadDeadline⁔(Ꮡp, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error SetWriteDeadline(this ж<pipe> Ꮡp, time.Time t) => net_package.SetWriteDeadline⁔(Ꮡp, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ж<pipe> Ꮡp) => net_package.Close⁔(Ꮡp);
}
partial class netᴛpipeAddr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Network(this pipeAddr _) => net_package.Network⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this pipeAddr _) => net_package.String⁔(_);
}
partial class netᴛpipeDeadline {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void set(this ж<pipeDeadline> Ꮡd, time.Time t) => net_package.set⁔(Ꮡd, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static channel<EmptyStruct> wait(this ж<pipeDeadline> Ꮡd) => net_package.wait⁔(Ꮡd);
}
partial class netᴛpolicyTable {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static policyTableEntry Classify(this policyTable t, netip.Addr ip) => net_package.Classify⁔(t, ip);
}
partial class netᴛrawConn {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool ok(this ж<rawConn> Ꮡc) => net_package.ok⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Control(this ж<rawConn> Ꮡc, Action<uintptr> f) => net_package.Control⁔(Ꮡc, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Read(this ж<rawConn> Ꮡc, Func<uintptr, bool> f) => net_package.Read⁔(Ꮡc, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Write(this ж<rawConn> Ꮡc, Func<uintptr, bool> f) => net_package.Write⁔(Ꮡc, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<poll.FD> PollFD(this ж<rawConn> Ꮡc) => net_package.PollFD⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static poll.String Network(this ref rawConn c) => net_package.Network⁔(ref c);
}
partial class netᴛrawListener {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Read(this ref rawListener l, Func<uintptr, bool> _) => net_package.Read⁔(ref l, _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Write(this ref rawListener l, Func<uintptr, bool> _) => net_package.Write⁔(ref l, _);
}
partial class netᴛresolverConfig {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ж<resolverConfig> Ꮡconf) => net_package.init⁔(Ꮡconf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void tryUpdate(this ж<resolverConfig> Ꮡconf, @string name) => net_package.tryUpdate⁔(Ꮡconf, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool tryAcquireSema(this ref resolverConfig conf) => net_package.tryAcquireSema⁔(ref conf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void releaseSema(this ref resolverConfig conf) => net_package.releaseSema⁔(ref conf);
}
partial class netᴛsysDialer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (Conn, error) dialParallel(this ж<sysDialer> Ꮡsd, context.Context ctx, addrList primaries, addrList fallbacks) => net_package.dialParallel⁔(Ꮡsd, ctx, primaries, fallbacks);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (Conn, error) dialSerial(this ж<sysDialer> Ꮡsd, context.Context ctx, addrList ras) => net_package.dialSerial⁔(Ꮡsd, ctx, ras);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (Conn c, error err) dialSingle(this ж<sysDialer> Ꮡsd, context.Context ctx, Addr ra) => net_package.dialSingle⁔(Ꮡsd, ctx, ra);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<IPConn>, error) dialIP(this ж<sysDialer> Ꮡsd, context.Context ctx, ж<IPAddr> Ꮡladdr, ж<IPAddr> Ꮡraddr) => net_package.dialIP⁔(Ꮡsd, ctx, Ꮡladdr, Ꮡraddr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<TCPConn>, error) dialMPTCP(this ж<sysDialer> Ꮡsd, context.Context ctx, ж<TCPAddr> Ꮡladdr, ж<TCPAddr> Ꮡraddr) => net_package.dialMPTCP⁔(Ꮡsd, ctx, Ꮡladdr, Ꮡraddr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<TCPConn>, error) dialTCP(this ж<sysDialer> Ꮡsd, context.Context ctx, ж<TCPAddr> Ꮡladdr, ж<TCPAddr> Ꮡraddr) => net_package.dialTCP⁔(Ꮡsd, ctx, Ꮡladdr, Ꮡraddr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<TCPConn>, error) doDialTCP(this ж<sysDialer> Ꮡsd, context.Context ctx, ж<TCPAddr> Ꮡladdr, ж<TCPAddr> Ꮡraddr) => net_package.doDialTCP⁔(Ꮡsd, ctx, Ꮡladdr, Ꮡraddr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<TCPConn>, error) doDialTCPProto(this ж<sysDialer> Ꮡsd, context.Context ctx, ж<TCPAddr> Ꮡladdr, ж<TCPAddr> Ꮡraddr, nint proto) => net_package.doDialTCPProto⁔(Ꮡsd, ctx, Ꮡladdr, Ꮡraddr, proto);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<UDPConn>, error) dialUDP(this ж<sysDialer> Ꮡsd, context.Context ctx, ж<UDPAddr> Ꮡladdr, ж<UDPAddr> Ꮡraddr) => net_package.dialUDP⁔(Ꮡsd, ctx, Ꮡladdr, Ꮡraddr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<UnixConn>, error) dialUnix(this ж<sysDialer> Ꮡsd, context.Context ctx, ж<UnixAddr> Ꮡladdr, ж<UnixAddr> Ꮡraddr) => net_package.dialUnix⁔(Ꮡsd, ctx, Ꮡladdr, Ꮡraddr);
}
partial class netᴛsysListener {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<IPConn>, error) listenIP(this ж<sysListener> Ꮡsl, context.Context ctx, ж<IPAddr> Ꮡladdr) => net_package.listenIP⁔(Ꮡsl, ctx, Ꮡladdr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<TCPListener>, error) listenMPTCP(this ж<sysListener> Ꮡsl, context.Context ctx, ж<TCPAddr> Ꮡladdr) => net_package.listenMPTCP⁔(Ꮡsl, ctx, Ꮡladdr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<TCPListener>, error) listenTCP(this ж<sysListener> Ꮡsl, context.Context ctx, ж<TCPAddr> Ꮡladdr) => net_package.listenTCP⁔(Ꮡsl, ctx, Ꮡladdr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<TCPListener>, error) listenTCPProto(this ж<sysListener> Ꮡsl, context.Context ctx, ж<TCPAddr> Ꮡladdr, nint proto) => net_package.listenTCPProto⁔(Ꮡsl, ctx, Ꮡladdr, proto);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<UDPConn>, error) listenUDP(this ж<sysListener> Ꮡsl, context.Context ctx, ж<UDPAddr> Ꮡladdr) => net_package.listenUDP⁔(Ꮡsl, ctx, Ꮡladdr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<UDPConn>, error) listenMulticastUDP(this ж<sysListener> Ꮡsl, context.Context ctx, ж<Interface> Ꮡifi, ж<UDPAddr> Ꮡgaddr) => net_package.listenMulticastUDP⁔(Ꮡsl, ctx, Ꮡifi, Ꮡgaddr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<UnixListener>, error) listenUnix(this ж<sysListener> Ꮡsl, context.Context ctx, ж<UnixAddr> Ꮡladdr) => net_package.listenUnix⁔(Ꮡsl, ctx, Ꮡladdr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<UnixConn>, error) listenUnixgram(this ж<sysListener> Ꮡsl, context.Context ctx, ж<UnixAddr> Ꮡladdr) => net_package.listenUnixgram⁔(Ꮡsl, ctx, Ꮡladdr);
}
partial class netᴛtemporaryError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this ref temporaryError e) => net_package.Error⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Temporary(this ref temporaryError e) => net_package.Temporary⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Timeout(this ref temporaryError e) => net_package.Timeout⁔(ref e);
}
partial class netᴛtimeoutError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this ref timeoutError e) => net_package.Error⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Timeout(this ref timeoutError e) => net_package.Timeout⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Temporary(this ref timeoutError e) => net_package.Temporary⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Is(this ref timeoutError e, error err) => net_package.Is⁔(ref e, err);
}
// </MethodForwarders>
