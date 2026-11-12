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
using static go.net_package;

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
[assembly: GoPositionMap("net/addrselect.go", "addrselect.cs", "AA0cgoKUpoKClIKCgu6Cgv6ygoKCgoKCgoKUpgAIEoKClIIADSTygoKCgoKCgpSC3IKUgpSC7oKUggAVLISUhNyClIIADBqClIIACxiCgpSClILuAAwaAD1+xIKUgoKmAA4egoKUgoKCuIKUAAIaAAwCgpSCgqaCgpSCgoKCgpSCgoKCgoKCgrg=", "", "", "28=ipAttrOf/1/2/2,ipAttrOf/2/2/4;76=classifyScope/1/1/1;225=AddrFrom16/1/9/3,PrefixFrom/1/9/3,AddrFrom16/2/9/10,PrefixFrom/2/9/10,AddrFrom16/3/9/16,PrefixFrom/3/9/16,AddrFrom16/4/9/23,PrefixFrom/4/9/23,AddrFrom16/5/9/30,PrefixFrom/5/9/30,AddrFrom16/6/9/36,PrefixFrom/6/9/36,AddrFrom16/7/9/42,PrefixFrom/7/9/42,AddrFrom16/8/9/48,PrefixFrom/8/9/48,AddrFrom16/9/9/54,PrefixFrom/9/9/54")]
[assembly: GoPositionMap("net/cgo_unix.go", "cgo_unix.cs", "ACRGgoKWAAkO8oKCpoKCloqCsoKCgqKktKSC+MKCgpSylLaCgtaCtIK0lJS0tqrCgoKmgpSCgoKUgpSkpKaUgpSCgqSCgqbmwoKCgoKUtLaCgpSCgoKkAAcQlKSk3JSkqJSUgpSUgoK0goK25oIADSSigoKUgoKmuqKCgoKCgoKmgpSClKSkpoKUpoKClIKUprKCgpSCgpSqouqigoKCgoKCgpSClAAJFIKClKaCgqaCgoKClIK4goKmgoKUgoKClA==", "64-69:1;111-113:1;228-230:1;256-258:1;315-317:1;345-348:1", "", "", "", "416:21,418:13")]
[assembly: GoPositionMap("net/cgo_unix_syscall.go", "cgo_unix_syscall.cs", "ACZYlIKClKaCgoKCgpSCpoKmgqaCgqaCgqaCpoKmgoKCpoKCgg==")]
[assembly: GoPositionMap("net/conf.go", "conf.cs", "AEaUAZiSggAMCsKCgoKUgoKClIKUlIKUxoKUxoKCgpQAChSmgriCgrik7oKCgsqCgrSoogAIEMrKpt7SgqYABxCCuKwACQKCgqbsAAkCgoKmABIGpKSCuIKClIKUyqa4grikAAgSlJSmpsq4gqaCypSUlKSCgqaUpIKCppS2AAcQlIKmgrimlKamgpSSgoKClJSClIKClIKUpoK4gpSCgpS0yqyylIKUlLS0pIKUxriCgoKCggAHEIKCgu6UgpTGpLjGAAEcAA8CgoKClIKUpoKCgpSCAAwKouqi6qI=", "92-121:1;222-224:1;234-236:1;500-509:1", "", "", "", "100:25,101:17,250:25,251:17,272:25,273:17")]
[assembly: GoPositionMap("net/dial.go", "dial.cs", "AClUAAwagpSkuIKUpqaCgpQAEB6ClKTKgpSmpoKClABu7AG0goKUgpQABBIACAKClIKUpoKClMqigpSCgqaEkoKClKbGgoKU2LKCggAICIL4lJSCpIKCgoKCppTMsoKClIKUpIKClIKUlIKClLyUgrSCtIKkgoKClJSClLSClLSClLaClM7CAAIS4gAEZgAwAoIAAhoACgKCAAsi8gAGKgAWAoK2goKCgoKmgoKm3JKClKbWgoKUgpKCgoKCgqaAgoKytOikooKUggAEGgAMAoKSAAUWAAwCgpIABRYADAKCkgAFFgAMAoKSABAO4oKmgoQAARDCgoKUguaC2raCkraCpIK0gpLGgpSClJSClMoACRDilIKk2IKAgoKUgpSUgoKC2IKClIK4gpTaAAgCgoKCgpSCkLaClIKCpNaCxILEgtSUgpQALVrCAAIS4gAGFPKCgpTKgoKUgqTW1JSClAACFPKCgpTKgoKUxMTUlIKUAAo8ABUCggACOAAZAoI=", "576-582:1;585-592:2;671-684:1;781-781:1", "", "", "576:21,675:21,697:21,719:21,741:21,771:21,804:21,811:21,820:29,886:37", "926:29,927:21")]
[assembly: GoPositionMap("net/dnsclient.go", "dnsclient.cs", "ABEoxIKmgqzSgoKUgqaUgoLcgqaygpSCgoKClIKUgqYAAiAADgSCAAoWgoKmgoKCgoKUpILGxoKUgsaClIKUpJSCpgACFgAIAoKUAA8gooKClIKCgoKCgoKUpoK6koKClJSCgoKCpgAPGpKCgpQ=", "200-205:1;231-233:1")]
[assembly: GoPositionMap("net/dnsclient_unix.go", "dnsclient_unix.cs", "ACtWgoKCgoLKqJTigoKClIKmgqaClIKClIK4goKUgoKCgqaygpSClIKUpqKAgraCgoKClLiCgpSCgpS4ooKmgoKUgoKUgoKUgoKClIKClIKUyNKCgoKUgoKUlIKCpIKClIKUgoKClJSCgpSCAAgSgpSU+LKUgqaCgsqCptyClKamooKCgpSClIKUggAEELKCgoKCgoKUgoKUgu7CgoKUgoKU3IKClJKCpoKUgqaCppSCpoKmlIKmpgAVIoKCAAgI1oK4rAAIApSCuIKUlIKClJQAChCCgpSCyIKipIKkpLiCxqLcpoKmqoKCgpSmpoKUuJQACA7CgpSCyKSCgoK4goKUpoKCppSCpoKCgriClAAQIgAJEIKClMaylIKCpoKmgoKUgrKUyMKCgoKCgqaCytKCAAwG0oKClIKCgoKUpoK4lJSMgqaCgoKUlLSkgoKCkqKCgoLWkoKSkoKmqIKCgpSCgoKClIKClJQAESSCgoKC3IKUlIKCypSCgsiCgsqUgoLIgoLKlILIgsqU6LiClIKmuJSCgoKCgoKCgoKUpoKm6JKCqLKCgoKmgriCgpSSgoKCgoK4lIKCgoKUgtyCgoLclIKC3Kg=", "656-656:1;657-662:2;664-671:3;666-670:3.1;672-674:4", "", "829=Error/1/1/1;842=Error/1/1/1;858=Error/1/1/1;874=Error/1/1/1;888=Error/1/1/1;989=Error/1/1/1;998=Error/1/1/1;1008=Error/1/1/1", "205:25")]
[assembly: GoPositionMap("net/dnsconfig.go", "dnsconfig.cs", "ABo0lAAVMOKClA==")]
[assembly: GoPositionMap("net/dnsconfig_unix.go", "dnsconfig_unix.cs", "ABomssqCgoKCgpSSgpSCgoKUgpSUgoKUpLiC2oLIgoKCgpTIgpSCgoKClLSCgpS0goKUtAAIDgAJELQACAq0AAkQtraClIKU1oKClJSClKaCgpQ=")]
[assembly: GoPositionMap("net/error_posix.go", "error_posix.cs", "AAsgooKU")]
[assembly: GoPositionMap("net/error_unix.go", "error_unix.cs", "AAoWgoKU")]
[assembly: GoPositionMap("net/fd_posix.go", "fd_posix.cs", "AB06gpKClIKUpqKClKaUguaCgoKmgqaCprKCgqbCgoKUsoKCprKCgqbigoKm0oKCptKCgqaygoKmsoKCprKCgqaygoKmwoKCpsKCgqbCgoKmgqaCpoI=")]
[assembly: GoPositionMap("net/fd_unix.go", "fd_unix.cs", "AB40ggAJFMaiAAgGAAkIgNakxoKUgv6ClKSkgIK2goKAgoLagsiClJLu7gAIEoCCpMakgoKUgNbagsak+MKCgoKUpoKClIKClIKCyOSygoKClKY=", "88-94:1;95-107:2", "", "", "", "132:25,133:17")]
[assembly: GoPositionMap("net/file.go", "file.cs", "ABIckIK8AAgCgoKUrgAIAoKClK4ACAKCgpQ=")]
[assembly: GoPositionMap("net/file_posix.go", "file_posix.cs", "ABQegoKClIKCgoKUgoKClIKUtLS0gpSCgoKUgoKCgoKUgqaCgoKUlKSkpJSCpoKCgpSUpJSCpoKCgpSUpKSUgg==")]
[assembly: GoPositionMap("net/file_unix.go", "file_unix.cs", "ABMigoKCgpSUgoKU")]
[assembly: GoPositionMap("net/hook.go", "hook.cs", "AAweAAAQkuw=")]
[assembly: GoPositionMap("net/hook_unix.go", "hook_unix.cs", "AAoY2oKCgg==")]
[assembly: GoPositionMap("net/hosts.go", "hosts.cs", "ABMkgoKClAAbOqKClIKUgoKCpoKUgoKCuIKSgoCUpIKClIKCpoKCgoKClIKmlIK4pgAHEIKCgoKCoqaygoKCgoKCgpSCgoKm2LKCgoKCgpSCgoKCpg==")]
[assembly: GoPositionMap("net/interface.go", "interface.cs", "ABMsgoKCggAVMgAKEoKCgoKClKaClMqigpSCgpSqooKUgoKUyJKCgpSClAACENKCgpQAAhDSgpSCgpSCgpSmgrKCpqiSgpSCgpSClLKCpgAPIAAHEAAIAoKCgoKUgoKCgqaCgoKCgoK45oKClIKCgoKCgoKClIKUpoKClIKCgoKCgoKClIKU")]
[assembly: GoPositionMap("net/interface_bsd.go", "interface_bsd.cs", "AA0isoKClIKClIKCgpSClIKCgoKClIKCgsimgoKClIKUgpSClIKUgpSs0oKClIKClIKClIKUgpSCgoKCgoK2gpSCgoKCgoK2gsg=")]
[assembly: GoPositionMap("net/interface_darwin.go", "interface_darwin.cs", "AAwYgqqigoKUgoKUgpSClIKCgoKCgraCyA==")]
[assembly: GoPositionMap("net/ip.go", "ip.cs", "ADFqooKCgoKCgqaYooKCgoKCrLKClIKUgoKCgoKCgpSClLqSkpK6kpKSkpLKoqiSgpSqou68qJKClKqiqqKClKqigpQAAhYACAIABBKSgoKmyqKClIiUqqKClIKU2oKCvLKClJSkpNiCgoKmyJKClIKUgoKUgoKUAAYS4oKmgqaClIK0gqSCxoKCgpSqooKUyrSClIKssoKUgqasxIKClKqigoKUgoKClIKssoKUgpSClKaCyqKCsoKCuIKCpoKUgoKmlMzigoKU6JKClMaygoKCpoKUgsaCxpTIkoKClIKClIKCpuiAAAUQ4oKUgoKUgoKUAAQS4oKUpoKCgpQAAhYACAKCgqaCgqaCgpSCgqaCgoI=", "", "", "198=Equal/1/1/1,IsUnspecified/1/1/2,IsLoopback/1/1/3,IsMulticast/1/1/4,IsLinkLocalUnicast/1/1/5;221=isZeros/1/1/1")]
[assembly: GoPositionMap("net/iprawsock.go", "iprawsock.cs", "AA1EABICAA0agNSigpSCgpSmooKUpoKClAAEIAANAoKUgoKU1pSCgpQAChbCgpSosoKUgoKUqLKClIKClIKUAAIUAA4CgpSCgpSosoKUgoKUqLKClIKClIKClAACEgALAoKUgoKUxgABEvKmooKUgoKUgoKUAAIUAAkCgpSCgoKU", "", "", "33=AsSlice/1/1/1,Zone/1/1/2")]
[assembly: GoPositionMap("net/iprawsock_posix.go", "iprawsock_posix.cs", "AAwcgpSklMaigpSClKaigpSmgsaGooKUgrSkxoKClIKClIKUgsbigoKUtKSmgoKUgpSCgpSmooKUgpSCgpQACAaigoKU1pSCgpiCgpQACAaigoKU1pSCgpiCgpQ=", "127-129:1;150-152:1")]
[assembly: GoPositionMap("net/ipsock.go", "ipsock.cs", "AB9CooKqooKs1qSmgt6SlKSklKiAurKCtsakgpSqooKCpgACEgAJAoKCgoKClKbO4oKCgqaClLiSqJIAEhgADAKYgpS2goKmlIKClKYACAyClJSClIKCpoKUgqaCptaClJQAAhD2gpTOwrqkgoKUgtiCxpSSpKSkpLaCuIKC3IKmgoKUgpQABBwACwKClA==", "170-172:1;272-283:1")]
[assembly: GoPositionMap("net/ipsock_posix.go", "ipsock_posix.cs", "ABk4AA4CtoKCgqaCxoKkAAUS2qSCgoKUgoKCgpSClIKUxgAEagAzApSkpoKClIKUpoSUpoKkgraCpoKClIKClIKCpgAKFoK4goKUgoIAAhwACwKUkoKUpJKClJSm7oKClLim7oLK", "", "", "61=ParseIP/1/1/5,IPv4/1/1/7;173=family/2/2/1;273=As4/1/1/1,Port/1/1/2;288=As16/1/1/1,Port/1/1/2,Zone/1/1/3,index/1/1/3")]
[assembly: GoPositionMap("net/lookup.go", "lookup.cs", "ABw0AA0cABo4goKCgoKClAANFIKkgpSkpJSmgoKCgoKClJSqooKUgoKUygAnVsCioKSCgpQABBDSyrSClIKUyqKCgpSCgpTKoq7CgoKU1qaClIKCpoKClK4ACAqCgpSCgoKmABcUtpKkpAAFEMLKtIKUgIKkgoLKgoIABxCUgoKitoKCgpQAChKCgrSkgoKUpIKCgoKCpoKClOyigpSCgoKClKiSgoKUrsLM4oKC1rSUgoKmgpQABCYAEAIABCAADQKCgpSClAAEIgAOAgAEIgAOAoKClIKUgoKClIKUlIKUAAQYAAkCAAQS4oKClIKCgpSmgoKmlIKUAAQYAAkCAAQS4oKClIKCgpSClJSClAAEFPLOwgAEHgAMAgAEEuKCgpSCgoKmgpQACxbc0oKClIKUgpQAAhgADQKClJSSgpSCgoKClILcgoLclIKUgoLclIKokpKClIKCgoKUgtyCgtyUgoLcloKospKClIKCgoKUgtyCgtyUgoLclLiSkoKUgoKCgpSC3IKC3JSCggAIEoKClIKClIKUlAAKBoKClIKClA==", "334-336:1;338-342:2;354-354:3")]
[assembly: GoPositionMap("net/lookup_unix.go", "lookup_unix.cs", "ABIiooKClKSUgpSCgpSCgpSCguqoooLGgoKClKaigoKUgqamgoKmgqaUpoKCgoKmpoKmgqaCpoKmgoKClA==")]
[assembly: GoPositionMap("net/mac.go", "mac.cs", "ABEYooKUgoKClIKUAAQgAA8CgqaCgpSCgpSCgoKClJSCgpSCgpSCgoKClIKUpoKmgoKmgoKCgpSmlII=")]
[assembly: GoPositionMap("net/mptcpsock_stub.go", "mptcpsock_stub.cs", "AA0agsaCxoI=")]
[assembly: GoPositionMap("net/net.go", "net.cs", "ALkB9gKgqtKClIKClKiygpSCgpSosoKUgoKUrNKClKzSgpSosoKUgpSosoKUgpSosoKUgpSqwoKUgpSqwoKUgpQAAhoADQKCgpQAR6oBAA0CgJIAHkCmpoIADA6UuKKUpKQAH0TUooKUgoKUgpSCgpSUlILsuIL+goKClIIACg6mgqaCgpSCAA8alIIADg6igpSCgpSmgviCgviCggAOGgAKCIKClIIADBKCgoLIggAJDgALDIKCABEmoryCgsqCppIADBiA1KKClIKClIKsoJqg2O4ACBCiAAwatAAJEqIADBikAAwagoKUtKTIggARJoIAAxIACQKClIKCgoKCpoIAAhIACQKCgoKClIKUpoKCgoKClIKC", "405-405:1;810-812:1", "", "794=Error/1/1/2")]
[assembly: GoPositionMap("net/nss.go", "nss.cs", "ACFCgoKCgoIACAiSgoKo4qaClJSCgpSUgoKUgqaCgoKipIKmgqSkuIIAEiKigoKmABQcsoKUgqS0xpSClMaigoKUkoKCpoKCAAsGooKCgoKUgoKClIKCgoKClIKCgoKUgpSUgoKCgpSCgoKClJSClNwACAjCgoKCgpSClIKClIKCgpTKlA==", "223-247:1")]
[assembly: GoPositionMap("net/parse.go", "parse.cs", "ABoulLKCgoKCgpSCgoKCppSCgpSmsoKUgoKCgpSCpoKmooKClMaCgoKUpqKCgpSokoKCgqaokoKCgoKCgoKUpoKClKbO0oKCgoKmgpSq0oKCgoKCgoKCgoKUlIKmgpSuwoKUgqiSgoKmqJKCgsySgpSokoKUgpSokqqigpSqooKCgoKUgoKmlKqiqqKClIKCpg==")]
[assembly: GoPositionMap("net/pipe.go", "pipe.cs", "ABQqggAEEgALAoKUgpSmgoKClLiAgoKUgpTIgrSm4oKC9oKkpAAMDIIAFDjSgoKCgoKU7u7GgpSCgoKUpqKUpKSm1IKCpKSkyIKCgpSm4pSkpKaCgoLUgoK0pKSm5qKClIKCpqKClIKmooKUgqaCgJI=", "54-56:1;236-236:1", "", "150=makePipeDeadline/1/2/4,makePipeDeadline/2/2/5;157=makePipeDeadline/1/2/4,makePipeDeadline/2/2/5")]
[assembly: GoPositionMap("net/port.go", "port.cs", "AAceAAoCppSYgoKCgoKUgqKClJSCgpSCgoKClJSCgoKUlIKU")]
[assembly: GoPositionMap("net/port_unix.go", "port_unix.cs", "ABQkooKClKSUgIKkgoKUgoKClIKCgoKUgoLYppKC")]
[assembly: GoPositionMap("net/rawconn.go", "rawconn.cs", "ABo0oKSigpSCgoKUpqKClIKCgpSmooKUgoKClAACFAAJAoKUxoIABBTyAAoOgqaCxoI=")]
[assembly: GoPositionMap("net/rlimit_unix.go", "rlimit_unix.cs", "AAoqAAoCgoKUgoKCgpQ=")]
[assembly: GoPositionMap("net/sendfile.go", "sendfile.cs", "ABMwAAsCgpSCgoKCgsqCgqaCgqaCgoKUgqaCpg==", "49-52:1")]
[assembly: GoPositionMap("net/sendfile_nonwindows.go", "sendfile_nonwindows.cs", "AAgUkg==")]
[assembly: GoPositionMap("net/sock_bsd.go", "sock_bsd.cs", "ABMcgpiktPikgsqClA==")]
[assembly: GoPositionMap("net/sock_posix.go", "sock_posix.cs", "AA8k0oKClIKClIKCABgygpSCgpSkgoKUpoKClMaCpJSUlIKU5qKCgoKCgoKClIK4goKCgoKCgriCgoKClIKUlIIACBKCgoKAgpSk5qKCgpSCgqaCgoK4gpSClIKUgoKmogAIEoKClIKUtKS2goKCpoKCgqaClIKUgoI=")]
[assembly: GoPositionMap("net/sockaddr_posix.go", "sockaddr_posix.cs", "ACFIgpSUpKTGlKSktg==", "57-57:1")]
[assembly: GoPositionMap("net/sockopt_bsd.go", "sockopt_bsd.cs", "ABAegsqUtLa4lJSUppSmpoLc")]
[assembly: GoPositionMap("net/sockopt_posix.go", "sockopt_posix.cs", "AAwekoKUpoKClIKClIKUgsaCyKaCgoKmgoKCpoKCgqaCgoKClIKUgoI=")]
[assembly: GoPositionMap("net/sockoptip4_bsdvar.go", "sockoptip4_bsdvar.cs", "AAwcgoKClIKCgoKmgoKC")]
[assembly: GoPositionMap("net/sockoptip4_posix_nonlinux.go", "sockoptip4_posix_nonlinux.cs", "AA0egoKClIKCpoKClIKClIKUgIKC1oCCgtiCgpQ=")]
[assembly: GoPositionMap("net/sockoptip6_posix.go", "sockoptip6_posix.cs", "AAwcooKClIKCpoKCgqaigoKClIKC")]
[assembly: GoPositionMap("net/splice_stub.go", "splice_stub.cs", "AAoWgqaC")]
[assembly: GoPositionMap("net/sys_cloexec.go", "sys_cloexec.cs", "ABMotIKCgpSCgpSCgpQ=")]
[assembly: GoPositionMap("net/tcpsock.go", "tcpsock.cs", "AB5A8oKUgoLogNSigpSCgpSmooKUpoKClAAEJAAPAta0lIKClKyyAC5kwoKUqLKClIKClKiygpSCgpSqwoKUgpSqwoKUgpQAAiIAEAKClIKUqsKClIKUAAIQ8oKUgpSu4oKUgpQAAhgACwKClMaCgoLKgoKClIKCpgACFPKmotaUgpSCmIKUgpSUgpQACx7ygpSqwoKUgoKUqsKClIKClKrCgpSClKygmMKClAACGgAOAoKUgoKUAAQYAAsC1pSClIKYgpSUgpSokg==", "", "", "128=Addr/1/2/1,AsSlice/1/1/1,Addr/2/2/2,Zone/1/1/2,Port/1/1/3")]
[assembly: GoPositionMap("net/tcpsock_posix.go", "tcpsock_posix.cs", "AA8ggpSklMaigpSClKaigpSmgsaigIKkgIKkpqKClMaigIKkgIKkpoKmsoKCmAAZNIKClKaClMaUggAKFoKUgoKmgoKUgpTGoKSCgoKUpoKmgoKClMaCpqKCgpiCgpQ=", "81-83:1;185-187:1")]
[assembly: GoPositionMap("net/tcpsock_unix.go", "tcpsock_unix.cs", "AA4YsoKmgIKkgIKkgIKkgIK2")]
[assembly: GoPositionMap("net/tcpsockopt_darwin.go", "tcpsockopt_darwin.cs", "ABAmgoKCgriCgoKmgoKCgriCgoKmgoKCgqaCgg==")]
[assembly: GoPositionMap("net/tcpsockopt_posix.go", "tcpsockopt_posix.cs", "AAwcgoKC")]
[assembly: GoPositionMap("net/udpsock.go", "udpsock.cs", "AB1A8oKUgoLogNSigpSCgpSmooKUpoKClAAEJAAPAta0lIKClKyyABEYAAsUwoKUqNqowoKUgoKUqJKClJQAAhAACgKClIKClAACFAANAoKCgpSoAAgCgpSCgpSosoKUgoKUqLKClIKClKiygpSCgpSCgpQAAhYADQKClIKClKjigpSCgpTGAAES8qai1pSClIKClIKClAACGAALAtaUgpSCgoKUAAIuABYC1pSClIKCgpQ=", "", "", "127=Addr/1/2/1,AsSlice/1/1/1,Addr/2/2/2,Zone/1/1/2,Port/1/1/3")]
[assembly: GoPositionMap("net/udpsock_posix.go", "udpsock_posix.cs", "AA4egpSklMaigpSClKaigpSmgsaigoKUgoKCgsaCgoKCtpSUpsKCgpSCgoKCxoKCgoK2gpSm4pSCgoK0goKCpKaigpSCppSSgpSkkoKUpLiCgpSCppSSgpSkkoKUpLiigpSClIKClKaCgpSCppSCgpKClJSkgoKSgpSUpNiigoKYgoKUxqKCgpiCgpSmsoKCmIKClIKCgoKmgoKmxoKCgqaAgqSAgqSmgoKCpoCCpICCpA==", "216-218:1;230-232:1;244-246:1")]
[assembly: GoPositionMap("net/unixsock.go", "unixsock.cs", "ABs6ouaigpSmoqaCgpQABBLipKQACxjCgpSqwoKUgpSqwoKUgpSosoKUgoKUqLKClIKClIKUAAIUAA4CgpSCgpSosoKUgoKUqLKClIKClIKClAACEgALAoKUgoKUxgABEOKmotaUgoKUgoKUAA4aoK7ygpSqwoKUgoKUqsKClIKClKrCgpSClKygmMKClAACGgAOAoKUgoKUzLLWlIKUgoKClKyy1pSClIKCgpQ=")]
[assembly: GoPositionMap("net/unixsock_posix.go", "unixsock_posix.cs", "AA4ggoKktLS0pqSClIKUgvimgoKUpoKClKaCgpSmgoKUAAoGgpSkpKTogqaigpSmgsaCgoKUgram4oKCgqaUgramooKUgpSClIKmooKUgoKClJTGooKCmIKClMaCgoKUpgANGIKCpqaCgoKUAAIWAAgCxrKCgpiCgpSmooKCmIKClA==", "160-162:1;191-195:1;222-224:1;236-238:1")]
[assembly: GoPositionMap("net/unixsock_readmsg_cloexec.go", "unixsock_readmsg_cloexec.cs", "AAwagoKCprKCgoKUgg==")]
[assembly: GoPositionMap("net/writev_unix.go", "writev_unix.cs", "AA8cooKUgoKUAAgGsoKC")]
// </GoSourcePositionMaps>

namespace go;

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
[GoMethodHost(typeof(net_package.AddrError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛAddrError;
[GoMethodHost(typeof(net_package.Buffers)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛBuffers;
[GoMethodHost(typeof(net_package.DNSConfigError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛDNSConfigError;
[GoMethodHost(typeof(net_package.DNSError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛDNSError;
[GoMethodHost(typeof(net_package.Dialer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛDialer;
[GoMethodHost(typeof(net_package.Flags)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛFlags;
[GoMethodHost(typeof(net_package.HardwareAddr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛHardwareAddr;
[GoMethodHost(typeof(net_package.IP)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛIP;
[GoMethodHost(typeof(net_package.IPAddr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛIPAddr;
[GoMethodHost(typeof(net_package.IPConn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛIPConn;
[GoMethodHost(typeof(net_package.IPMask)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛIPMask;
[GoMethodHost(typeof(net_package.IPNet)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛIPNet;
[GoMethodHost(typeof(net_package.Interface)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛInterface;
[GoMethodHost(typeof(net_package.InvalidAddrError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛInvalidAddrError;
[GoMethodHost(typeof(net_package.ListenConfig)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛListenConfig;
[GoMethodHost(typeof(net_package.OpError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛOpError;
[GoMethodHost(typeof(net_package.ParseError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛParseError;
[GoMethodHost(typeof(net_package.Resolver)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛResolver;
[GoMethodHost(typeof(net_package.TCPAddr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛTCPAddr;
[GoMethodHost(typeof(net_package.TCPConn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛTCPConn;
[GoMethodHost(typeof(net_package.TCPListener)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛTCPListener;
[GoMethodHost(typeof(net_package.UDPAddr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛUDPAddr;
[GoMethodHost(typeof(net_package.UDPConn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛUDPConn;
[GoMethodHost(typeof(net_package.UnixAddr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛUnixAddr;
[GoMethodHost(typeof(net_package.UnixConn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛUnixConn;
[GoMethodHost(typeof(net_package.UnixListener)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛUnixListener;
[GoMethodHost(typeof(net_package.UnknownNetworkError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛUnknownNetworkError;
[GoMethodHost(typeof(net_package.addrList)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛaddrList;
[GoMethodHost(typeof(net_package.addrPortUDPAddr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛaddrPortUDPAddr;
[GoMethodHost(typeof(net_package.addrinfoErrno)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛaddrinfoErrno;
[GoMethodHost(typeof(net_package.byPref)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛbyPref;
[GoMethodHost(typeof(net_package.byPriorityWeight)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛbyPriorityWeight;
[GoMethodHost(typeof(net_package.canceledError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛcanceledError;
[GoMethodHost(typeof(net_package.conf)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛconf;
[GoMethodHost(typeof(net_package.conn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛconn;
[GoMethodHost(typeof(net_package.dnsConfig)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛdnsConfig;
[GoMethodHost(typeof(net_package.@file)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛfile;
[GoMethodHost(typeof(net_package.fileAddr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛfileAddr;
[GoMethodHost(typeof(net_package.hostLookupOrder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛhostLookupOrder;
[GoMethodHost(typeof(net_package.ipStackCapabilities)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛipStackCapabilities;
[GoMethodHost(typeof(net_package.ipv6ZoneCache)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛipv6ZoneCache;
[GoMethodHost(typeof(net_package.mptcpStatusDial)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛmptcpStatusDial;
[GoMethodHost(typeof(net_package.mptcpStatusListen)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛmptcpStatusListen;
[GoMethodHost(typeof(net_package.netFD)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛnetFD;
[GoMethodHost(typeof(net_package.noReadFrom)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛnoReadFrom;
[GoMethodHost(typeof(net_package.noWriteTo)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛnoWriteTo;
[GoMethodHost(typeof(net_package.notFoundError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛnotFoundError;
[GoMethodHost(typeof(net_package.nssCriterion)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛnssCriterion;
[GoMethodHost(typeof(net_package.nssSource)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛnssSource;
[GoMethodHost(typeof(net_package.nsswitchConfig)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛnsswitchConfig;
[GoMethodHost(typeof(net_package.onlyValuesCtx)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛonlyValuesCtx;
[GoMethodHost(typeof(net_package.pipe)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛpipe;
[GoMethodHost(typeof(net_package.pipeAddr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛpipeAddr;
[GoMethodHost(typeof(net_package.pipeDeadline)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛpipeDeadline;
[GoMethodHost(typeof(net_package.policyTable)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛpolicyTable;
[GoMethodHost(typeof(net_package.rawConn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛrawConn;
[GoMethodHost(typeof(net_package.rawListener)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛrawListener;
[GoMethodHost(typeof(net_package.resolverConfig)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛresolverConfig;
[GoMethodHost(typeof(net_package.sysDialer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛsysDialer;
[GoMethodHost(typeof(net_package.sysListener)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛsysListener;
[GoMethodHost(typeof(net_package.temporaryError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛtemporaryError;
[GoMethodHost(typeof(net_package.timeoutError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class netᴛtimeoutError;
// </MethodHolders>
