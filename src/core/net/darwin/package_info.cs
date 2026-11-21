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
[assembly: GoPositionMap("net/cgo_unix.go", "cgo_unix.cs", "ACRGgoKWAAkO8oKCpoKCloqCsoKCgqKktKSC+MKCgpSylLaCgtaCtIK0lJS0tqrCgoKmgpSCgoKUgpSkpKaUgpSCgqSCgqbmwoKCgoKUtLaCgpSCgoKUAAcQlKSk3JSkqJSUgpSUgoK0goK25oIADSSigoKUgoKmuqKCgoKCgoKmgpSClKSkpoKUpoKClIKUprKCgpSCgpSqouqigoKCgoKCgpSClAAJFIKClKaCgqaCgoKClIK4goKmgoKUgoKClA==", "64-69:1;111-113:1;228-230:1;256-258:1;315-317:1;345-348:1", "", "", "", "415:21,417:13")]
[assembly: GoPositionMap("net/cgo_unix_syscall.go", "cgo_unix_syscall.cs", "ACZYlIKClKaCgoKCgpSCpoKmgqaCgqaCgqaCpoKmgoKCpoKCgg==")]
[assembly: GoPositionMap("net/conf.go", "conf.cs", "AEaUAZiSggAMCsKCgoKUgoKClIKUlIKUxoKUxoKCgpQAChSmgriCgrik7oKCgsqCgrSoogAIEMrKpt7SgqYABxCCuKwACQKCgqbsAAkCgoKmABIGpKSCuIKClIKUyqa4grikAAgSlJSmpsq4gqaCypSUlKSCgqaUpIKCppS2AAcQlIKmgrimlKamgpSSgoKClJSClIKClIKUpoK4gpSCgpS0yqyylIKUlLS0pIKUxriCgoKCggAHEIKCgu6UgpTGpLjGAAEcAA8CgoKClIKUpoKCgpSCAAwKouqi6qI=", "92-121:1;222-224:1;234-236:1;500-509:1", "", "", "", "100:25,101:17,250:25,251:17,272:25,273:17")]
[assembly: GoPositionMap("net/dial.go", "dial.cs", "AClUAAwagpSkuIKUpqaCgpQAEB6ClKTKgpSmpoKClABu7AG0goKUgpQABBIACAKClIKUpoKClMqigpSCgqaEkoKClKbGgoKU2LKCgviC+JSUgpSCgoKCgqaUzLKCgpSClJSCgpSClJSCgpS8lIK0grSCpIKCgpSUgpS0gpS0gpS2gpTOwgACEuIABGYAMAKCAAIaAAoCggALIvIABioAFgKCtoKCgoKCpoKCptySgpSm1oKClIKSgoKCgoKmgIKCsrTopKKClIIABBoADAKCkpKClIKUAAUWAAwCgpKSgpSClAAFFgAMAoKSkoKUgpQABRYADAKCkgAQDuKCpoKEAAEQwoKClILmgtq2gpK2gqSCtIKSxoKUgpSUgpTKAAkQ4pSCpNiCgIKClIKUlIKCgtiCgpSCuIKU2gAIAoKCgoKUgpC2gpSCgqTWgsSCxILUlIKUAC1awgACEuIABhTygoKUyoKClIKk1tSUgpQAAhTygoKUyoKClMTE1JSClAAKPAAVAoIAAjgAGQKC", "576-582:1;585-592:2;692-705:1;802-802:1", "", "", "573:21,672:21,702:21,732:21,762:21,792:21,825:21,832:21,841:29,907:37", "947:29,948:21")]
[assembly: GoPositionMap("net/dnsclient.go", "dnsclient.cs", "ABEoxIKmgqzSgoKUgqaUgoLcgqaygpSCgoKClIKUgqYAAiAADgSCAAoWgoKmgoKCgoKUpILGxoKUgsaClIKUpJSCpgACFgAIAoKUAA8gooKClIKCgoKCgoKUpoK6koKClJSCgoKCpgAPGpKCgpQ=", "200-205:1;231-233:1")]
[assembly: GoPositionMap("net/dnsclient_unix.go", "dnsclient_unix.cs", "ACtWgoKCgoLKqJTigoKClIKmgqaClIKClIK4goKUgoKCgqaygpSClIKUpqKAgraCgoKClLiCgpSCgpS4ooKmgoKUgoKUgoKUgoKClIKClIKUyNKCgoKUgoKUlIKCpIKClIKUgoKClJSCgpSCAAgSgpSU+LKUgqaCgsqCptyClKamooKCgpSClIKUggAEELKCgoKCgoKUgoKUgu7CgoKUgoKU3IKClJKCpoKUgqaCppSCpoKmlIKmpgAVIoKCAAgI1oK4rAAIApSCuIKUlIKClJQAChCCgpSCyIKipIKkpLiCxqLcpoKmqoKCgpSmpoKUuJQACA7CgpSCyKSCgoK4goKUpoKCppSCpoKCgriClAAQIgAJEIKClMaylIKCpoKmgoKUgrKUyMKCgoKCgqaCytKCAAwG0oKClIKCkoKUpoK4lJSMgqaCgoKUlLSkgoKCkqKCgoLWkoKSkoKmqIKCgpSCgoKClIKClJQAESSCgoKC3IKUlIKCypSCgsiCgsqUgoLIgoLKlILIgsqU6LiClIKmuJSCgoKCgoKCkoKUpoKm6JKCqLKCgoKmgriCgpSSgoKCgoK4lIKCgoKUgtyCgoLclIKC3Kg=", "656-656:1;657-662:2;664-671:3;666-670:3.1;672-674:4", "", "830=Error/1/1/1;843=Error/1/1/1;859=Error/1/1/1;875=Error/1/1/1;889=Error/1/1/1;991=Error/1/1/1;1000=Error/1/1/1;1010=Error/1/1/1", "205:25")]
[assembly: GoPositionMap("net/dnsconfig.go", "dnsconfig.cs", "ABo0lAAVMOKClA==")]
[assembly: GoPositionMap("net/dnsconfig_unix.go", "dnsconfig_unix.cs", "ABomssqCgoKCgpSSgpSCgoKUgpSUgoKUpLiC2oLIgoKCgpTIgpSCgoKClLSCgpS0goKUtAAIDgAJELQACAq0AAkQtraClIKU1oKClJSClKaCgpQ=")]
[assembly: GoPositionMap("net/error_posix.go", "error_posix.cs", "AAsgooKU")]
[assembly: GoPositionMap("net/error_unix.go", "error_unix.cs", "AAoWgoKU")]
[assembly: GoPositionMap("net/fd_posix.go", "fd_posix.cs", "AB06gpKClIKUpqKClKaUguaCgoKmgqaCprKCgqbCgoKUsoKCprKCgqbigoKm0oKCptKCgqaygoKmsoKCprKCgqaygoKmwoKCpsKCgqbCgoKmgqaCpoI=")]
[assembly: GoPositionMap("net/fd_unix.go", "fd_unix.cs", "AB40ggAJFMaiAAgGAAkIgMakxoKUgv6ClKSkgIK2goKAgoLagsiClJLu7gAIEoCCpMakgoKUgNbagsak+MKCgoKUpoKClIKClIKCyOSygoKClKY=", "88-94:1;95-107:2", "", "", "", "131:25,132:17")]
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
[assembly: GoPositionMap("net/iprawsock.go", "iprawsock.cs", "AA1EABICAA0agNSigpSCgpSmooKUpoKClAAEIAANAoKUgoKUxpSCgpQAChbCgpSosoKUgoKUqLKClIKClIKUAAIUAA4CgpSCgpSosoKUgoKUqLKClIKClIKClAACEgALAoKUgoKUxgABEvKmooKUgoKUgoKUAAIUAAkCgpSCgoKU", "", "", "33=AsSlice/1/1/1,Zone/1/1/2")]
[assembly: GoPositionMap("net/iprawsock_posix.go", "iprawsock_posix.cs", "AAwcgpSklMaigpSClKaigpSmgsaGooKUgrSkxoKClIKClIKUgsbigoKUtKSmgoKUgpSCgpSmooKUgpSCgpQACAaigoKUxpSCgpiCgpQACAaigoKUxpSCgpiCgpQ=", "127-129:1;150-152:1")]
[assembly: GoPositionMap("net/ipsock.go", "ipsock.cs", "AB9CooKqooKs1qSmgt6SlKSklKiAurKCpsakgpSqooKCpgACEgAJAoKCgoKClKbO4oKCgqaClLiSqJIAEhgADAKYgpS2goKmlIKClKYACAyClJSClIKCpoKUgqaCptaClJQAAhD2gpTOwrqkgoKUgtiCxpSSpKSkpLaCuIKC3IKmgoKUgpQABBwACwKClA==", "170-172:1;272-283:1")]
[assembly: GoPositionMap("net/ipsock_posix.go", "ipsock_posix.cs", "ABk4AA4CtoKCgqaCxoKkAAUS2qSCgoKUgoKCgpSClIKUxgAEagAzApSkpoKClIKUpoSUpoKkgraCpoKClIKClIKCpgAKFoK4goKUgoIAAhwACwKUkoKUpJKClJSm7oKClLim7oLK", "", "", "61=ParseIP/1/1/5,IPv4/1/1/7;173=family/2/2/1;273=As4/1/1/1,Port/1/1/2;288=As16/1/1/1,Port/1/1/2,Zone/1/1/3,index/1/1/3")]
[assembly: GoPositionMap("net/lookup.go", "lookup.cs", "ABw0AA0cABo4goKCgoKClAANFIKUgpSkpJSmgoKCgoKClJSqooKUgoKUygAnVsCioKSCgpQABBDSyrSClIKUyqKCgpSCgpTKoq7CgoKUxqaClIKCpoKClK4ACAqCgpSCgoKmABsUtpKkpAAFEMLKtIKUgIKkgoLKgoIABxCUgoKitoKCgpQAChKCgrSkgoKUpIKCgoKCpoKClOyigpSCgoKClKiSgoKUrsLM4oKCxrSUgoKmgpQABCYAEAIABCAADQKCgpSClAAEIgAOAgAEIgAOAoKClIKUgoKClIKUlIKUAAQYAAkCAAQS4oKClIKCgpSmgoKmlIKUAAQYAAkCAAQS4oKClIKCgpSClJSClAAEFPLOwgAEHgAMAgAEEuKCgpSCgoKmgpQACxbc0oKClIKUgpQAAhgADQKClJSSgpSCgoKClILcgoLclIKUgoLclIKokpKClIKCgoKUgtyCgtyUgoLcloKospKClIKCgoKUgtyCgtyUgoLclLiSkoKUgoKCgpSC3IKC3JSCggAIEoKClIKClIKUlAAKBoKClIKClA==", "334-336:1;338-342:2;354-354:3")]
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
[assembly: GoPositionMap("net/tcpsock.go", "tcpsock.cs", "AB5A8oKUgoLogNSigpSCgpSmooKUpoKClAAEJAAPAsa0lIKClKyyAC5kwoKUqLKClIKClKiygpSCgpSqwoKUgpSqwoKUgpQAAiIAEAKClIKUqsKClIKUAAIQ8oKUgpSu4oKUgpQAAhgACwKClMaCgoLKgoKClIKCpgACFPKmosaUgpSCmIKUgpSUgpQACx7ygpSqwoKUgoKUqsKClIKClKrCgpSClKygmMKClAACGgAOAoKUgoKUAAQYAAsCxpSClIKYgpSUgpSokg==", "", "", "127=Addr/1/2/1,AsSlice/1/1/1,Addr/2/2/2,Zone/1/1/2,Port/1/1/3")]
[assembly: GoPositionMap("net/tcpsock_posix.go", "tcpsock_posix.cs", "AA8ggpSklMaigpSClKaigpSmgsaigIKkgIKkpqKClMaigIKkgIKkpoKmsoKCmAAZNIKClKaClMaUggAKFoKUgoKmgoKUgpTGoKSCgoKUpoKmgoKClMaCpqKCgpiCgpQ=", "81-83:1;185-187:1")]
[assembly: GoPositionMap("net/tcpsock_unix.go", "tcpsock_unix.cs", "AA4YsoKmgIKkgIKkgIKkgIK2")]
[assembly: GoPositionMap("net/tcpsockopt_darwin.go", "tcpsockopt_darwin.cs", "ABAmgoKCgriCgoKmgoKCgriCgoKmgoKCgqaCgg==")]
[assembly: GoPositionMap("net/tcpsockopt_posix.go", "tcpsockopt_posix.cs", "AAwcgoKC")]
[assembly: GoPositionMap("net/udpsock.go", "udpsock.cs", "AB1A8oKUgoLogNSigpSCgpSmooKUpoKClAAEJAAPAsa0lIKClKyyABEYAAsUwoKUqNqowoKUgoKUqJKClJQAAhAACgKClIKClAACFAANAoKCgpSoAAgCgpSCgpSosoKUgoKUqLKClIKClKiygpSCgpSCgpQAAhYADQKClIKClKjigpSCgpTGAAES8qaixpSClIKClIKClAACGAALAsaUgpSCgoKUAAIuABYCxpSClIKCgpQ=", "", "", "126=Addr/1/2/1,AsSlice/1/1/1,Addr/2/2/2,Zone/1/1/2,Port/1/1/3")]
[assembly: GoPositionMap("net/udpsock_posix.go", "udpsock_posix.cs", "AA4egpSklMaigpSClKaigpSmgsaigoKUgoKCgsaCgoKCtpSUpsKCgpSCgoKCxoKCgoK2gpSm4pSCgoK0goKCpKaigpSCppSSgpSkkoKUpLiCgpSCppSSgpSkkoKUpLiigpSClIKClKaCgpSCppSCgpKClJSkgoKSgpSUpNiigoKYgoKUxqKCgpiCgpSmsoKCmIKClIKCgoKmgoKmxoKCgqaAgqSAgqSmgoKCpoCCpICCpA==", "216-218:1;230-232:1;244-246:1")]
[assembly: GoPositionMap("net/unixsock.go", "unixsock.cs", "ABs6ouaigpSmoqaCgpQABBLilKQACxjCgpSqwoKUgpSqwoKUgpSosoKUgoKUqLKClIKClIKUAAIUAA4CgpSCgpSosoKUgoKUqLKClIKClIKClAACEgALAoKUgoKUxgABEOKmosaUgoKUgoKUAA4aoK7ygpSqwoKUgoKUqsKClIKClKrCgpSClKygmMKClAACGgAOAoKUgoKUzLLGlIKUgoKClKyyxpSClIKCgpQ=")]
[assembly: GoPositionMap("net/unixsock_posix.go", "unixsock_posix.cs", "AA4ggoKUtLS0ppSClIKUgvimgoKUpoKClKaCgpSmgoKUAAoGgpSkpKTogqaigpSmgsaCgoKUgram4oKCgqaUgramooKUgpSClIKmooKUgoKClJTGooKCmIKClMaCgoKUpgANGIKCpqaCgoKUAAIWAAgCxrKCgpiCgpSmooKCmIKClA==", "160-162:1;191-195:1;222-224:1;236-238:1")]
[assembly: GoPositionMap("net/unixsock_readmsg_cloexec.go", "unixsock_readmsg_cloexec.cs", "AAwagoKCprKCgoKUgg==")]
[assembly: GoPositionMap("net/writev_unix.go", "writev_unix.cs", "AA8cooKUgoKUAAgGsoKC")]
// </GoSourcePositionMaps>

namespace go;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using netip = net.netip_package;
using net;
using static net_package;
using context = context_package;
using syscall = syscall_package;
using dnsmessage = vendor.golang.org.x.net.dns.dnsmessage_package;
using @internal;
using vendor.golang.org.x.net.dns;
using os = os_package;
using go.io;
using time = time_package;
using Δio = io_package;
using go.sync;
using poll = @internal.poll_package;
using singleflight = @internal.singleflight_package;
// </GlyphForwarderUsings>

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

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class netᴛpolicyTable {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static policyTableEntry Classify(this policyTable t, netip.Addr ip) => net_package.Classifyᴍ(t, ip);
}
partial class netᴛaddrinfoErrno {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this addrinfoErrno eai) => net_package.Errorᴍ(eai);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Temporary(this addrinfoErrno eai) => net_package.Temporaryᴍ(eai);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Timeout(this addrinfoErrno eai) => net_package.Timeoutᴍ(eai);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void isAddrinfoErrno(this addrinfoErrno eai) => net_package.isAddrinfoErrnoᴍ(eai);
}
partial class netᴛconf {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool mustUseGoResolver(this ref conf c, ж<Resolver> Ꮡr) => net_package.mustUseGoResolverᴍ(ref c, Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (hostLookupOrder ret, ж<dnsConfig> dnsConf) addrLookupOrder(this ж<conf> Ꮡc, ж<Resolver> r, @string addr) => net_package.addrLookupOrderᴍ(Ꮡc, r, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (hostLookupOrder ret, ж<dnsConfig> dnsConf) hostLookupOrder(this ж<conf> Ꮡc, ж<Resolver> r, @string hostname) => net_package.hostLookupOrderᴍ(Ꮡc, r, hostname);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (hostLookupOrder ret, ж<dnsConfig> dnsConf) lookupOrder(this ref conf c, ж<Resolver> r, @string hostname) => net_package.lookupOrderᴍ(ref c, r, hostname);
}
partial class netᴛmptcpStatusDial {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool get(this ref mptcpStatusDial m) => net_package.getᴍ(ref m);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void set(this ref mptcpStatusDial m, bool use) => net_package.setᴍ(ref m, use);
}
partial class netᴛmptcpStatusListen {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool get(this ref mptcpStatusListen m) => net_package.getᴍ(ref m);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void set(this ref mptcpStatusListen m, bool use) => net_package.setᴍ(ref m, use);
}
partial class netᴛDialer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool dualStack(this ref Dialer d) => net_package.dualStackᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static time.Time /*earliest*/ deadline(this ref Dialer d, context.Context ctx, time.Time now) => net_package.deadlineᴍ(ref d, ctx, now);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Resolver> resolver(this ref Dialer d) => net_package.resolverᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static time.Duration fallbackDelay(this ref Dialer d) => net_package.fallbackDelayᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool MultipathTCP(this ref Dialer d) => net_package.MultipathTCPᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetMultipathTCP(this ref Dialer d, bool use) => net_package.SetMultipathTCPᴍ(ref d, use);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (Conn, error) Dial(this ж<Dialer> Ꮡd, @string network, @string address) => net_package.Dialᴍ(Ꮡd, network, address);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (Conn, error) DialContext(this ж<Dialer> Ꮡd, context.Context ctx, @string network, @string address) => net_package.DialContextᴍ(Ꮡd, ctx, network, address);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (context.Context, Action) dialCtx(this ref Dialer d, context.Context ctx) => net_package.dialCtxᴍ(ref d, ctx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<TCPConn>, error) DialTCP(this ж<Dialer> Ꮡd, context.Context ctx, @string network, netip.AddrPort laddr, netip.AddrPort raddr) => net_package.DialTCPᴍ(Ꮡd, ctx, network, laddr, raddr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<UDPConn>, error) DialUDP(this ж<Dialer> Ꮡd, context.Context ctx, @string network, netip.AddrPort laddr, netip.AddrPort raddr) => net_package.DialUDPᴍ(Ꮡd, ctx, network, laddr, raddr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<IPConn>, error) DialIP(this ж<Dialer> Ꮡd, context.Context ctx, @string network, netip.Addr laddr, netip.Addr raddr) => net_package.DialIPᴍ(Ꮡd, ctx, network, laddr, raddr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<UnixConn>, error) DialUnix(this ж<Dialer> Ꮡd, context.Context ctx, @string network, ж<UnixAddr> laddr, ж<UnixAddr> raddr) => net_package.DialUnixᴍ(Ꮡd, ctx, network, laddr, raddr);
}
partial class netᴛResolver {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (addrList, error) resolveAddrList(this ж<Resolver> Ꮡr, context.Context ctx, @string op, @string network, @string addr, Addr hint) => net_package.resolveAddrListᴍ(Ꮡr, ctx, op, network, addr, hint);
}
partial class netᴛsysDialer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (Conn, error) dialParallel(this ж<sysDialer> Ꮡsd, context.Context ctx, addrList primaries, addrList fallbacks) => net_package.dialParallelᴍ(Ꮡsd, ctx, primaries, fallbacks);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (Conn, error) dialSerial(this ж<sysDialer> Ꮡsd, context.Context ctx, addrList ras) => net_package.dialSerialᴍ(Ꮡsd, ctx, ras);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (Conn c, error err) dialSingle(this ж<sysDialer> Ꮡsd, context.Context ctx, Addr ra) => net_package.dialSingleᴍ(Ꮡsd, ctx, ra);
}
partial class netᴛListenConfig {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool MultipathTCP(this ref ListenConfig lc) => net_package.MultipathTCPᴍ(ref lc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetMultipathTCP(this ref ListenConfig lc, bool use) => net_package.SetMultipathTCPᴍ(ref lc, use);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (Listener, error) Listen(this ref ListenConfig lc, context.Context ctx, @string network, @string address) => net_package.Listenᴍ(ref lc, ctx, network, address);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (PacketConn, error) ListenPacket(this ref ListenConfig lc, context.Context ctx, @string network, @string address) => net_package.ListenPacketᴍ(ref lc, ctx, network, address);
}
partial class netᴛbyPriorityWeight {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void shuffleByWeight(this byPriorityWeight addrs) => net_package.shuffleByWeightᴍ(addrs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void sort(this byPriorityWeight addrs) => net_package.sortᴍ(addrs);
}
partial class netᴛbyPref {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void sort(this byPref s) => net_package.sortᴍ(s);
}
partial class netᴛResolver {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (dnsmessage.Parser, dnsmessage.Header, error) exchange(this ж<Resolver> Ꮡr, context.Context ctx, @string server, dnsmessage.Question q, time.Duration timeout, bool useTCP, bool ad) => net_package.exchangeᴍ(Ꮡr, ctx, server, q, timeout, useTCP, ad);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (dnsmessage.Parser, @string, error) tryOneName(this ж<Resolver> Ꮡr, context.Context ctx, ж<dnsConfig> Ꮡcfg, @string name, dnsmessage.Type qtype) => net_package.tryOneNameᴍ(Ꮡr, ctx, Ꮡcfg, name, qtype);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (dnsmessage.Parser, @string, error) lookup(this ж<Resolver> Ꮡr, context.Context ctx, @string name, dnsmessage.Type qtype, ж<dnsConfig> Ꮡconf) => net_package.lookupᴍ(Ꮡr, ctx, name, qtype, Ꮡconf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<@string> addrs, error err) goLookupHostOrder(this ж<Resolver> Ꮡr, context.Context ctx, @string name, hostLookupOrder order, ж<dnsConfig> conf) => net_package.goLookupHostOrderᴍ(Ꮡr, ctx, name, order, conf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<IPAddr> addrs, error err) goLookupIP(this ж<Resolver> Ꮡr, context.Context ctx, @string network, @string host, hostLookupOrder order, ж<dnsConfig> conf) => net_package.goLookupIPᴍ(Ꮡr, ctx, network, host, order, conf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<IPAddr> addrs, dnsmessage.Name cname, error err) goLookupIPCNAMEOrder(this ж<Resolver> Ꮡr, context.Context ctx, @string network, @string name, hostLookupOrder order, ж<dnsConfig> Ꮡconf) => net_package.goLookupIPCNAMEOrderᴍ(Ꮡr, ctx, network, name, order, Ꮡconf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string, error) goLookupCNAME(this ж<Resolver> Ꮡr, context.Context ctx, @string host, hostLookupOrder order, ж<dnsConfig> conf) => net_package.goLookupCNAMEᴍ(Ꮡr, ctx, host, order, conf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<@string>, error) goLookupPTR(this ж<Resolver> Ꮡr, context.Context ctx, @string addr, hostLookupOrder order, ж<dnsConfig> conf) => net_package.goLookupPTRᴍ(Ꮡr, ctx, addr, order, conf);
}
partial class netᴛresolverConfig {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ж<resolverConfig> Ꮡconf) => net_package.initᴍ(Ꮡconf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void tryUpdate(this ж<resolverConfig> Ꮡconf, @string name) => net_package.tryUpdateᴍ(Ꮡconf, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool tryAcquireSema(this ref resolverConfig conf) => net_package.tryAcquireSemaᴍ(ref conf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void releaseSema(this ref resolverConfig conf) => net_package.releaseSemaᴍ(ref conf);
}
partial class netᴛdnsConfig {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<@string> nameList(this ref dnsConfig conf, @string name) => net_package.nameListᴍ(ref conf, name);
}
partial class netᴛhostLookupOrder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this hostLookupOrder o) => net_package.Stringᴍ(o);
}
partial class netᴛdnsConfig {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint32 serverOffset(this ж<dnsConfig> Ꮡc) => net_package.serverOffsetᴍ(Ꮡc);
}
partial class netᴛnetFD {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string name(this ref netFD fd) => net_package.nameᴍ(ref fd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setAddr(this ж<netFD> Ꮡfd, Addr laddr, Addr raddr) => net_package.setAddrᴍ(Ꮡfd, laddr, raddr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ж<netFD> Ꮡfd) => net_package.Closeᴍ(Ꮡfd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error shutdown(this ж<netFD> Ꮡfd, nint how) => net_package.shutdownᴍ(Ꮡfd, how);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error closeRead(this ж<netFD> Ꮡfd) => net_package.closeReadᴍ(Ꮡfd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error closeWrite(this ж<netFD> Ꮡfd) => net_package.closeWriteᴍ(Ꮡfd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Read(this ж<netFD> Ꮡfd, slice<byte> p) => net_package.Readᴍ(Ꮡfd, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, syscall.Sockaddr sa, error err) readFrom(this ж<netFD> Ꮡfd, slice<byte> p) => net_package.readFromᴍ(Ꮡfd, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) readFromInet4(this ж<netFD> Ꮡfd, slice<byte> p, ж<syscall.SockaddrInet4> from) => net_package.readFromInet4ᴍ(Ꮡfd, p, from);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) readFromInet6(this ж<netFD> Ꮡfd, slice<byte> p, ж<syscall.SockaddrInet6> from) => net_package.readFromInet6ᴍ(Ꮡfd, p, from);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, nint oobn, nint retflags, syscall.Sockaddr sa, error err) readMsg(this ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags) => net_package.readMsgᴍ(Ꮡfd, p, oob, flags);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, nint oobn, nint retflags, error err) readMsgInet4(this ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags, ж<syscall.SockaddrInet4> sa) => net_package.readMsgInet4ᴍ(Ꮡfd, p, oob, flags, sa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, nint oobn, nint retflags, error err) readMsgInet6(this ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags, ж<syscall.SockaddrInet6> sa) => net_package.readMsgInet6ᴍ(Ꮡfd, p, oob, flags, sa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint nn, error err) Write(this ж<netFD> Ꮡfd, slice<byte> p) => net_package.Writeᴍ(Ꮡfd, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) writeTo(this ж<netFD> Ꮡfd, slice<byte> p, syscall.Sockaddr sa) => net_package.writeToᴍ(Ꮡfd, p, sa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) writeToInet4(this ж<netFD> Ꮡfd, slice<byte> p, ж<syscall.SockaddrInet4> sa) => net_package.writeToInet4ᴍ(Ꮡfd, p, sa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) writeToInet6(this ж<netFD> Ꮡfd, slice<byte> p, ж<syscall.SockaddrInet6> sa) => net_package.writeToInet6ᴍ(Ꮡfd, p, sa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, nint oobn, error err) writeMsg(this ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, syscall.Sockaddr sa) => net_package.writeMsgᴍ(Ꮡfd, p, oob, sa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, nint oobn, error err) writeMsgInet4(this ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, ж<syscall.SockaddrInet4> sa) => net_package.writeMsgInet4ᴍ(Ꮡfd, p, oob, sa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, nint oobn, error err) writeMsgInet6(this ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, ж<syscall.SockaddrInet6> sa) => net_package.writeMsgInet6ᴍ(Ꮡfd, p, oob, sa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error SetDeadline(this ж<netFD> Ꮡfd, time.Time t) => net_package.SetDeadlineᴍ(Ꮡfd, t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error SetReadDeadline(this ж<netFD> Ꮡfd, time.Time t) => net_package.SetReadDeadlineᴍ(Ꮡfd, t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error SetWriteDeadline(this ж<netFD> Ꮡfd, time.Time t) => net_package.SetWriteDeadlineᴍ(Ꮡfd, t);
}
partial class netᴛnetFD {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error init(this ж<netFD> Ꮡfd) => net_package.initᴍ(Ꮡfd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (syscall.Sockaddr rsa, error ret) connect(this ж<netFD> Ꮡfd, context.Context ctx, syscall.Sockaddr la, syscall.Sockaddr ra) => net_package.connectᴍ(Ꮡfd, ctx, la, ra);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<netFD> netfd, error err) accept(this ж<netFD> Ꮡfd) => net_package.acceptᴍ(Ꮡfd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<os.File> f, error err) dup(this ж<netFD> Ꮡfd) => net_package.dupᴍ(Ꮡfd);
}
partial class netᴛfileAddr {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Network(this fileAddr _) => net_package.Networkᴍ(_);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this fileAddr f) => net_package.Stringᴍ(f);
}
partial class netᴛFlags {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Flags f) => net_package.Stringᴍ(f);
}
partial class netᴛInterface {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<Addr>, error) Addrs(this ж<Interface> Ꮡifi) => net_package.Addrsᴍ(Ꮡifi);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<Addr>, error) MulticastAddrs(this ж<Interface> Ꮡifi) => net_package.MulticastAddrsᴍ(Ꮡifi);
}
partial class netᴛipv6ZoneCache {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool /*updated*/ update(this ж<ipv6ZoneCache> Ꮡzc, slice<Interface> ift, bool force) => net_package.updateᴍ(Ꮡzc, ift, force);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string name(this ref ipv6ZoneCache zc, nint index) => net_package.nameᴍ(ref zc, index);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint index(this ref ipv6ZoneCache zc, @string name) => net_package.indexᴍ(ref zc, name);
}
partial class netᴛIP {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsUnspecified(this IP ip) => net_package.IsUnspecifiedᴍ(ip);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsLoopback(this IP ip) => net_package.IsLoopbackᴍ(ip);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsPrivate(this IP ip) => net_package.IsPrivateᴍ(ip);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsMulticast(this IP ip) => net_package.IsMulticastᴍ(ip);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsInterfaceLocalMulticast(this IP ip) => net_package.IsInterfaceLocalMulticastᴍ(ip);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsLinkLocalMulticast(this IP ip) => net_package.IsLinkLocalMulticastᴍ(ip);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsLinkLocalUnicast(this IP ip) => net_package.IsLinkLocalUnicastᴍ(ip);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsGlobalUnicast(this IP ip) => net_package.IsGlobalUnicastᴍ(ip);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static IP To4(this IP ip) => net_package.To4ᴍ(ip);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static IP To16(this IP ip) => net_package.To16ᴍ(ip);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static IPMask DefaultMask(this IP ip) => net_package.DefaultMaskᴍ(ip);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static IP Mask(this IP ip, IPMask mask) => net_package.Maskᴍ(ip, mask);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this IP ip) => net_package.Stringᴍ(ip);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> appendTo(this IP ip, slice<byte> b) => net_package.appendToᴍ(ip, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte>, error) AppendText(this IP ip, slice<byte> b) => net_package.AppendTextᴍ(ip, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte>, error) MarshalText(this IP ip) => net_package.MarshalTextᴍ(ip);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error UnmarshalText(this ref IP ip, slice<byte> text) => net_package.UnmarshalTextᴍ(ref ip, text);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Equal(this IP ip, IP x) => net_package.Equalᴍ(ip, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool matchAddrFamily(this IP ip, IP x) => net_package.matchAddrFamilyᴍ(ip, x);
}
partial class netᴛIPMask {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint ones, nint bits) Size(this IPMask m) => net_package.Sizeᴍ(m);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this IPMask m) => net_package.Stringᴍ(m);
}
partial class netᴛIPNet {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Contains(this ж<IPNet> Ꮡn, IP ip) => net_package.Containsᴍ(Ꮡn, ip);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Network(this ref IPNet n) => net_package.Networkᴍ(ref n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<IPNet> Ꮡn) => net_package.Stringᴍ(Ꮡn);
}
partial class netᴛIPAddr {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Network(this ref IPAddr a) => net_package.Networkᴍ(ref a);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<IPAddr> Ꮡa) => net_package.Stringᴍ(Ꮡa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isWildcard(this ж<IPAddr> Ꮡa) => net_package.isWildcardᴍ(Ꮡa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Addr opAddr(this ж<IPAddr> Ꮡa) => net_package.opAddrᴍ(Ꮡa);
}
partial class netᴛIPConn {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (syscall.RawConn, error) SyscallConn(this ж<IPConn> Ꮡc) => net_package.SyscallConnᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, ж<IPAddr>, error) ReadFromIP(this ж<IPConn> Ꮡc, slice<byte> b) => net_package.ReadFromIPᴍ(Ꮡc, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, Addr, error) ReadFrom(this ж<IPConn> Ꮡc, slice<byte> b) => net_package.ReadFromᴍ(Ꮡc, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint n, nint oobn, nint flags, ж<IPAddr> addr, error err) ReadMsgIP(this ж<IPConn> Ꮡc, slice<byte> b, slice<byte> oob) => net_package.ReadMsgIPᴍ(Ꮡc, b, oob);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) WriteToIP(this ж<IPConn> Ꮡc, slice<byte> b, ж<IPAddr> addr) => net_package.WriteToIPᴍ(Ꮡc, b, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) WriteTo(this ж<IPConn> Ꮡc, slice<byte> b, Addr addr) => net_package.WriteToᴍ(Ꮡc, b, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint n, nint oobn, error err) WriteMsgIP(this ж<IPConn> Ꮡc, slice<byte> b, slice<byte> oob, ж<IPAddr> addr) => net_package.WriteMsgIPᴍ(Ꮡc, b, oob, addr);
}
partial class netᴛIPAddr {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint family(this ж<IPAddr> Ꮡa) => net_package.familyᴍ(Ꮡa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (syscall.Sockaddr, error) sockaddr(this ж<IPAddr> Ꮡa, nint family) => net_package.sockaddrᴍ(Ꮡa, family);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static sockaddr toLocal(this ref IPAddr a, @string net) => net_package.toLocalᴍ(ref a, net);
}
partial class netᴛIPConn {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, ж<IPAddr>, error) readFrom(this ref IPConn c, slice<byte> b) => net_package.readFromᴍ(ref c, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, nint oobn, nint flags, ж<IPAddr> addr, error err) readMsg(this ref IPConn c, slice<byte> b, slice<byte> oob) => net_package.readMsgᴍ(ref c, b, oob);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) writeTo(this ref IPConn c, slice<byte> b, ж<IPAddr> addr) => net_package.writeToᴍ(ref c, b, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, nint oobn, error err) writeMsg(this ref IPConn c, slice<byte> b, slice<byte> oob, ж<IPAddr> addr) => net_package.writeMsgᴍ(ref c, b, oob, addr);
}
partial class netᴛsysDialer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<IPConn>, error) dialIP(this ж<sysDialer> Ꮡsd, context.Context ctx, ж<IPAddr> laddr, ж<IPAddr> raddr) => net_package.dialIPᴍ(Ꮡsd, ctx, laddr, raddr);
}
partial class netᴛsysListener {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<IPConn>, error) listenIP(this ж<sysListener> Ꮡsl, context.Context ctx, ж<IPAddr> laddr) => net_package.listenIPᴍ(Ꮡsl, ctx, laddr);
}
partial class netᴛaddrList {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Addr forResolve(this addrList addrs, @string network, @string addr) => net_package.forResolveᴍ(addrs, network, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Addr first(this addrList addrs, Func<Addr, bool> strategy) => net_package.firstᴍ(addrs, strategy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (addrList primaries, addrList fallbacks) partition(this addrList addrs, Func<Addr, bool> strategy) => net_package.partitionᴍ(addrs, strategy);
}
partial class netᴛResolver {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (addrList, error) internetAddrList(this ж<Resolver> Ꮡr, context.Context ctx, @string net, @string addr) => net_package.internetAddrListᴍ(Ꮡr, ctx, net, addr);
}
partial class netᴛipStackCapabilities {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void probe(this ж<ipStackCapabilities> Ꮡp) => net_package.probeᴍ(Ꮡp);
}
partial class netᴛResolver {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool preferGo(this ж<Resolver> Ꮡr) => net_package.preferGoᴍ(Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool strictErrors(this ж<Resolver> Ꮡr) => net_package.strictErrorsᴍ(Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<singleflight.Group> getLookupGroup(this ж<Resolver> Ꮡr) => net_package.getLookupGroupᴍ(Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<@string> addrs, error err) LookupHost(this ж<Resolver> Ꮡr, context.Context ctx, @string host) => net_package.LookupHostᴍ(Ꮡr, ctx, host);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<IPAddr>, error) LookupIPAddr(this ж<Resolver> Ꮡr, context.Context ctx, @string host) => net_package.LookupIPAddrᴍ(Ꮡr, ctx, host);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<IP>, error) LookupIP(this ж<Resolver> Ꮡr, context.Context ctx, @string network, @string host) => net_package.LookupIPᴍ(Ꮡr, ctx, network, host);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<netip.Addr>, error) LookupNetIP(this ж<Resolver> Ꮡr, context.Context ctx, @string network, @string host) => net_package.LookupNetIPᴍ(Ꮡr, ctx, network, host);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<IPAddr>, error) lookupIPAddr(this ж<Resolver> Ꮡr, context.Context ctx, @string network, @string host) => net_package.lookupIPAddrᴍ(Ꮡr, ctx, network, host);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint port, error err) LookupPort(this ж<Resolver> Ꮡr, context.Context ctx, @string network, @string service) => net_package.LookupPortᴍ(Ꮡr, ctx, network, service);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (@string, error) LookupCNAME(this ж<Resolver> Ꮡr, context.Context ctx, @string host) => net_package.LookupCNAMEᴍ(Ꮡr, ctx, host);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (@string, slice<ж<SRV>>, error) LookupSRV(this ж<Resolver> Ꮡr, context.Context ctx, @string service, @string proto, @string name) => net_package.LookupSRVᴍ(Ꮡr, ctx, service, proto, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<ж<MX>>, error) LookupMX(this ж<Resolver> Ꮡr, context.Context ctx, @string name) => net_package.LookupMXᴍ(Ꮡr, ctx, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<ж<NS>>, error) LookupNS(this ж<Resolver> Ꮡr, context.Context ctx, @string name) => net_package.LookupNSᴍ(Ꮡr, ctx, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<@string>, error) LookupTXT(this ж<Resolver> Ꮡr, context.Context ctx, @string name) => net_package.LookupTXTᴍ(Ꮡr, ctx, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<@string>, error) LookupAddr(this ж<Resolver> Ꮡr, context.Context ctx, @string addr) => net_package.LookupAddrᴍ(Ꮡr, ctx, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (Conn, error) dial(this ж<Resolver> Ꮡr, context.Context ctx, @string network, @string server) => net_package.dialᴍ(Ꮡr, ctx, network, server);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string target, slice<ж<SRV>> srvs, error err) goLookupSRV(this ж<Resolver> Ꮡr, context.Context ctx, @string service, @string proto, @string name) => net_package.goLookupSRVᴍ(Ꮡr, ctx, service, proto, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<ж<MX>>, error) goLookupMX(this ж<Resolver> Ꮡr, context.Context ctx, @string name) => net_package.goLookupMXᴍ(Ꮡr, ctx, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<ж<NS>>, error) goLookupNS(this ж<Resolver> Ꮡr, context.Context ctx, @string name) => net_package.goLookupNSᴍ(Ꮡr, ctx, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<@string>, error) goLookupTXT(this ж<Resolver> Ꮡr, context.Context ctx, @string name) => net_package.goLookupTXTᴍ(Ꮡr, ctx, name);
}
partial class netᴛonlyValuesCtx {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (time.Time, bool) Deadline(this onlyValuesCtx recvᴛ) => net_package.Deadlineᴍ(recvᴛ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static /*<-*/channel<EmptyStruct> Done(this onlyValuesCtx recvᴛ) => net_package.Doneᴍ(recvᴛ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Err(this onlyValuesCtx recvᴛ) => net_package.Errᴍ(recvᴛ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static any Value(this ref onlyValuesCtx ovc, any key) => net_package.Valueᴍ(ref ovc, key);
}
partial class netᴛResolver {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<@string> addrs, error err) lookupHost(this ж<Resolver> Ꮡr, context.Context ctx, @string host) => net_package.lookupHostᴍ(Ꮡr, ctx, host);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<IPAddr> addrs, error err) lookupIP(this ж<Resolver> Ꮡr, context.Context ctx, @string network, @string host) => net_package.lookupIPᴍ(Ꮡr, ctx, network, host);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) lookupPort(this ж<Resolver> Ꮡr, context.Context ctx, @string network, @string service) => net_package.lookupPortᴍ(Ꮡr, ctx, network, service);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string, error) lookupCNAME(this ж<Resolver> Ꮡr, context.Context ctx, @string name) => net_package.lookupCNAMEᴍ(Ꮡr, ctx, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string, slice<ж<SRV>>, error) lookupSRV(this ж<Resolver> Ꮡr, context.Context ctx, @string service, @string proto, @string name) => net_package.lookupSRVᴍ(Ꮡr, ctx, service, proto, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<ж<MX>>, error) lookupMX(this ж<Resolver> Ꮡr, context.Context ctx, @string name) => net_package.lookupMXᴍ(Ꮡr, ctx, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<ж<NS>>, error) lookupNS(this ж<Resolver> Ꮡr, context.Context ctx, @string name) => net_package.lookupNSᴍ(Ꮡr, ctx, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<@string>, error) lookupTXT(this ж<Resolver> Ꮡr, context.Context ctx, @string name) => net_package.lookupTXTᴍ(Ꮡr, ctx, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<@string>, error) lookupAddr(this ж<Resolver> Ꮡr, context.Context ctx, @string addr) => net_package.lookupAddrᴍ(Ꮡr, ctx, addr);
}
partial class netᴛHardwareAddr {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this HardwareAddr a) => net_package.Stringᴍ(a);
}
partial class netᴛsysDialer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<TCPConn>, error) dialMPTCP(this ж<sysDialer> Ꮡsd, context.Context ctx, ж<TCPAddr> laddr, ж<TCPAddr> raddr) => net_package.dialMPTCPᴍ(Ꮡsd, ctx, laddr, raddr);
}
partial class netᴛsysListener {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<TCPListener>, error) listenMPTCP(this ж<sysListener> Ꮡsl, context.Context ctx, ж<TCPAddr> laddr) => net_package.listenMPTCPᴍ(Ꮡsl, ctx, laddr);
}
partial class netᴛconn {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool ok(this ж<conn> Ꮡc) => net_package.okᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Read(this ж<conn> Ꮡc, slice<byte> b) => net_package.Readᴍ(Ꮡc, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Write(this ж<conn> Ꮡc, slice<byte> b) => net_package.Writeᴍ(Ꮡc, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ж<conn> Ꮡc) => net_package.Closeᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Addr LocalAddr(this ж<conn> Ꮡc) => net_package.LocalAddrᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Addr RemoteAddr(this ж<conn> Ꮡc) => net_package.RemoteAddrᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error SetDeadline(this ж<conn> Ꮡc, time.Time t) => net_package.SetDeadlineᴍ(Ꮡc, t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error SetReadDeadline(this ж<conn> Ꮡc, time.Time t) => net_package.SetReadDeadlineᴍ(Ꮡc, t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error SetWriteDeadline(this ж<conn> Ꮡc, time.Time t) => net_package.SetWriteDeadlineᴍ(Ꮡc, t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error SetReadBuffer(this ж<conn> Ꮡc, nint bytes) => net_package.SetReadBufferᴍ(Ꮡc, bytes);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error SetWriteBuffer(this ж<conn> Ꮡc, nint bytes) => net_package.SetWriteBufferᴍ(Ꮡc, bytes);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<os.File> f, error err) File(this ref conn c) => net_package.Fileᴍ(ref c);
}
partial class netᴛcanceledError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this canceledError _) => net_package.Errorᴍ(_);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Is(this canceledError _, error err) => net_package.Isᴍ(_, err);
}
partial class netᴛOpError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Unwrap(this ref OpError e) => net_package.Unwrapᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this ж<OpError> Ꮡe) => net_package.Errorᴍ(Ꮡe);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Timeout(this ref OpError e) => net_package.Timeoutᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Temporary(this ref OpError e) => net_package.Temporaryᴍ(ref e);
}
partial class netᴛParseError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this ref ParseError e) => net_package.Errorᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Timeout(this ref ParseError e) => net_package.Timeoutᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Temporary(this ref ParseError e) => net_package.Temporaryᴍ(ref e);
}
partial class netᴛAddrError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this ж<AddrError> Ꮡe) => net_package.Errorᴍ(Ꮡe);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Timeout(this ref AddrError e) => net_package.Timeoutᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Temporary(this ref AddrError e) => net_package.Temporaryᴍ(ref e);
}
partial class netᴛUnknownNetworkError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this UnknownNetworkError e) => net_package.Errorᴍ(e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Timeout(this UnknownNetworkError e) => net_package.Timeoutᴍ(e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Temporary(this UnknownNetworkError e) => net_package.Temporaryᴍ(e);
}
partial class netᴛInvalidAddrError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this InvalidAddrError e) => net_package.Errorᴍ(e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Timeout(this InvalidAddrError e) => net_package.Timeoutᴍ(e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Temporary(this InvalidAddrError e) => net_package.Temporaryᴍ(e);
}
partial class netᴛtimeoutError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this ref timeoutError e) => net_package.Errorᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Timeout(this ref timeoutError e) => net_package.Timeoutᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Temporary(this ref timeoutError e) => net_package.Temporaryᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Is(this ref timeoutError e, error err) => net_package.Isᴍ(ref e, err);
}
partial class netᴛDNSConfigError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Unwrap(this ref DNSConfigError e) => net_package.Unwrapᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this ref DNSConfigError e) => net_package.Errorᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Timeout(this ref DNSConfigError e) => net_package.Timeoutᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Temporary(this ref DNSConfigError e) => net_package.Temporaryᴍ(ref e);
}
partial class netᴛnotFoundError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this ref notFoundError e) => net_package.Errorᴍ(ref e);
}
partial class netᴛtemporaryError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this ref temporaryError e) => net_package.Errorᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Temporary(this ref temporaryError e) => net_package.Temporaryᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Timeout(this ref temporaryError e) => net_package.Timeoutᴍ(ref e);
}
partial class netᴛDNSError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Unwrap(this ref DNSError e) => net_package.Unwrapᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this ж<DNSError> Ꮡe) => net_package.Errorᴍ(Ꮡe);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Timeout(this ref DNSError e) => net_package.Timeoutᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Temporary(this ref DNSError e) => net_package.Temporaryᴍ(ref e);
}
partial class netᴛnoReadFrom {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64, error) ReadFrom(this noReadFrom _Δp0, Δio.Reader _Δp1) => net_package.ReadFromᴍ(_Δp0, _Δp1);
}
partial class netᴛnoWriteTo {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64, error) WriteTo(this noWriteTo _Δp0, Δio.Writer _Δp1) => net_package.WriteToᴍ(_Δp0, _Δp1);
}
partial class netᴛBuffers {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (int64 n, error err) WriteTo(this ж<Buffers> Ꮡv, Δio.Writer w) => net_package.WriteToᴍ(Ꮡv, w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint n, error err) Read(this ref Buffers v, slice<byte> p) => net_package.Readᴍ(ref v, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void consume(this ref Buffers v, int64 n) => net_package.consumeᴍ(ref v, n);
}
partial class netᴛnsswitchConfig {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ref nsswitchConfig conf) => net_package.initᴍ(ref conf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void tryUpdate(this ж<nsswitchConfig> Ꮡconf) => net_package.tryUpdateᴍ(Ꮡconf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void acquireSema(this ref nsswitchConfig conf) => net_package.acquireSemaᴍ(ref conf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool tryAcquireSema(this ref nsswitchConfig conf) => net_package.tryAcquireSemaᴍ(ref conf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void releaseSema(this ref nsswitchConfig conf) => net_package.releaseSemaᴍ(ref conf);
}
partial class netᴛnssSource {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool standardCriteria(this nssSource s) => net_package.standardCriteriaᴍ(s);
}
partial class netᴛnssCriterion {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool standardStatusAction(this nssCriterion c, bool last) => net_package.standardStatusActionᴍ(c, last);
}
partial class netᴛfile {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void close(this ref @file f) => net_package.closeᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string s, bool ok) getLineFromData(this ref @file f) => net_package.getLineFromDataᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string s, bool ok) readLine(this ref @file f) => net_package.readLineᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (time.Time mtime, int64 size, error err) stat(this ref @file f) => net_package.statᴍ(ref f);
}
partial class netᴛpipeDeadline {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void set(this ж<pipeDeadline> Ꮡd, time.Time t) => net_package.setᴍ(Ꮡd, t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static channel<EmptyStruct> wait(this ж<pipeDeadline> Ꮡd) => net_package.waitᴍ(Ꮡd);
}
partial class netᴛpipeAddr {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Network(this pipeAddr _) => net_package.Networkᴍ(_);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this pipeAddr _) => net_package.Stringᴍ(_);
}
partial class netᴛpipe {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Addr LocalAddr(this ref pipe _) => net_package.LocalAddrᴍ(ref _);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Addr RemoteAddr(this ref pipe _) => net_package.RemoteAddrᴍ(ref _);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Read(this ж<pipe> Ꮡp, slice<byte> b) => net_package.Readᴍ(Ꮡp, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) read(this ж<pipe> Ꮡp, slice<byte> b) => net_package.readᴍ(Ꮡp, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Write(this ж<pipe> Ꮡp, slice<byte> b) => net_package.Writeᴍ(Ꮡp, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) write(this ж<pipe> Ꮡp, slice<byte> b) => net_package.writeᴍ(Ꮡp, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error SetDeadline(this ж<pipe> Ꮡp, time.Time t) => net_package.SetDeadlineᴍ(Ꮡp, t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error SetReadDeadline(this ж<pipe> Ꮡp, time.Time t) => net_package.SetReadDeadlineᴍ(Ꮡp, t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error SetWriteDeadline(this ж<pipe> Ꮡp, time.Time t) => net_package.SetWriteDeadlineᴍ(Ꮡp, t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ж<pipe> Ꮡp) => net_package.Closeᴍ(Ꮡp);
}
partial class netᴛrawConn {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool ok(this ж<rawConn> Ꮡc) => net_package.okᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Control(this ж<rawConn> Ꮡc, Action<uintptr> f) => net_package.Controlᴍ(Ꮡc, f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Read(this ж<rawConn> Ꮡc, Func<uintptr, bool> f) => net_package.Readᴍ(Ꮡc, f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Write(this ж<rawConn> Ꮡc, Func<uintptr, bool> f) => net_package.Writeᴍ(Ꮡc, f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<poll.FD> PollFD(this ж<rawConn> Ꮡc) => net_package.PollFDᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static poll.String Network(this ref rawConn c) => net_package.Networkᴍ(ref c);
}
partial class netᴛrawListener {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Read(this ref rawListener l, Func<uintptr, bool> _) => net_package.Readᴍ(ref l, _);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Write(this ref rawListener l, Func<uintptr, bool> _) => net_package.Writeᴍ(ref l, _);
}
partial class netᴛnetFD {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string ctrlNetwork(this ref netFD fd) => net_package.ctrlNetworkᴍ(ref fd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error dial(this ж<netFD> Ꮡfd, context.Context ctx, sockaddr laddr, sockaddr raddr, Func<context.Context, @string, @string, syscall.RawConn, error> ctrlCtxFn) => net_package.dialᴍ(Ꮡfd, ctx, laddr, raddr, ctrlCtxFn);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error listenStream(this ж<netFD> Ꮡfd, context.Context ctx, sockaddr laddr, nint backlog, Func<context.Context, @string, @string, syscall.RawConn, error> ctrlCtxFn) => net_package.listenStreamᴍ(Ꮡfd, ctx, laddr, backlog, ctrlCtxFn);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error listenDatagram(this ж<netFD> Ꮡfd, context.Context ctx, sockaddr laddr, Func<context.Context, @string, @string, syscall.RawConn, error> ctrlCtxFn) => net_package.listenDatagramᴍ(Ꮡfd, ctx, laddr, ctrlCtxFn);
}
partial class netᴛnetFD {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Func<syscall.Sockaddr, Addr> addrFunc(this ref netFD fd) => net_package.addrFuncᴍ(ref fd);
}
partial class netᴛTCPAddr {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static netip.AddrPort AddrPort(this ж<TCPAddr> Ꮡa) => net_package.AddrPortᴍ(Ꮡa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Network(this ref TCPAddr a) => net_package.Networkᴍ(ref a);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<TCPAddr> Ꮡa) => net_package.Stringᴍ(Ꮡa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isWildcard(this ж<TCPAddr> Ꮡa) => net_package.isWildcardᴍ(Ꮡa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Addr opAddr(this ж<TCPAddr> Ꮡa) => net_package.opAddrᴍ(Ꮡa);
}
partial class netᴛTCPConn {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (syscall.RawConn, error) SyscallConn(this ж<TCPConn> Ꮡc) => net_package.SyscallConnᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (int64, error) ReadFrom(this ж<TCPConn> Ꮡc, Δio.Reader r) => net_package.ReadFromᴍ(Ꮡc, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (int64, error) WriteTo(this ж<TCPConn> Ꮡc, Δio.Writer w) => net_package.WriteToᴍ(Ꮡc, w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error CloseRead(this ж<TCPConn> Ꮡc) => net_package.CloseReadᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error CloseWrite(this ж<TCPConn> Ꮡc) => net_package.CloseWriteᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SetLinger(this ж<TCPConn> Ꮡc, nint sec) => net_package.SetLingerᴍ(Ꮡc, sec);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SetKeepAlive(this ж<TCPConn> Ꮡc, bool keepalive) => net_package.SetKeepAliveᴍ(Ꮡc, keepalive);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SetKeepAlivePeriod(this ж<TCPConn> Ꮡc, time.Duration d) => net_package.SetKeepAlivePeriodᴍ(Ꮡc, d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SetNoDelay(this ж<TCPConn> Ꮡc, bool noDelay) => net_package.SetNoDelayᴍ(Ꮡc, noDelay);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (bool, error) MultipathTCP(this ж<TCPConn> Ꮡc) => net_package.MultipathTCPᴍ(Ꮡc);
}
partial class netᴛTCPListener {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (syscall.RawConn, error) SyscallConn(this ж<TCPListener> Ꮡl) => net_package.SyscallConnᴍ(Ꮡl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<TCPConn>, error) AcceptTCP(this ж<TCPListener> Ꮡl) => net_package.AcceptTCPᴍ(Ꮡl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (Conn, error) Accept(this ж<TCPListener> Ꮡl) => net_package.Acceptᴍ(Ꮡl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Close(this ж<TCPListener> Ꮡl) => net_package.Closeᴍ(Ꮡl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Addr Addr(this ref TCPListener l) => net_package.Addrᴍ(ref l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SetDeadline(this ж<TCPListener> Ꮡl, time.Time t) => net_package.SetDeadlineᴍ(Ꮡl, t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<os.File> f, error err) File(this ж<TCPListener> Ꮡl) => net_package.Fileᴍ(Ꮡl);
}
partial class netᴛTCPAddr {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint family(this ж<TCPAddr> Ꮡa) => net_package.familyᴍ(Ꮡa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (syscall.Sockaddr, error) sockaddr(this ж<TCPAddr> Ꮡa, nint family) => net_package.sockaddrᴍ(Ꮡa, family);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static sockaddr toLocal(this ref TCPAddr a, @string net) => net_package.toLocalᴍ(ref a, net);
}
partial class netᴛTCPConn {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64, error) readFrom(this ж<TCPConn> Ꮡc, Δio.Reader r) => net_package.readFromᴍ(Ꮡc, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64, error) writeTo(this ж<TCPConn> Ꮡc, Δio.Writer w) => net_package.writeToᴍ(Ꮡc, w);
}
partial class netᴛsysDialer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<TCPConn>, error) dialTCP(this ж<sysDialer> Ꮡsd, context.Context ctx, ж<TCPAddr> laddr, ж<TCPAddr> raddr) => net_package.dialTCPᴍ(Ꮡsd, ctx, laddr, raddr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<TCPConn>, error) doDialTCP(this ж<sysDialer> Ꮡsd, context.Context ctx, ж<TCPAddr> laddr, ж<TCPAddr> raddr) => net_package.doDialTCPᴍ(Ꮡsd, ctx, laddr, raddr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<TCPConn>, error) doDialTCPProto(this ж<sysDialer> Ꮡsd, context.Context ctx, ж<TCPAddr> Ꮡladdr, ж<TCPAddr> raddr, nint proto) => net_package.doDialTCPProtoᴍ(Ꮡsd, ctx, Ꮡladdr, raddr, proto);
}
partial class netᴛTCPListener {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool ok(this ж<TCPListener> Ꮡln) => net_package.okᴍ(Ꮡln);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<TCPConn>, error) accept(this ref TCPListener ln) => net_package.acceptᴍ(ref ln);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error close(this ref TCPListener ln) => net_package.closeᴍ(ref ln);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<os.File>, error) @file(this ref TCPListener ln) => net_package.@fileᴍ(ref ln);
}
partial class netᴛsysListener {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<TCPListener>, error) listenTCP(this ж<sysListener> Ꮡsl, context.Context ctx, ж<TCPAddr> laddr) => net_package.listenTCPᴍ(Ꮡsl, ctx, laddr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<TCPListener>, error) listenTCPProto(this ж<sysListener> Ꮡsl, context.Context ctx, ж<TCPAddr> laddr, nint proto) => net_package.listenTCPProtoᴍ(Ꮡsl, ctx, laddr, proto);
}
partial class netᴛTCPConn {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SetKeepAliveConfig(this ж<TCPConn> Ꮡc, KeepAliveConfig config) => net_package.SetKeepAliveConfigᴍ(Ꮡc, config);
}
partial class netᴛUDPAddr {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static netip.AddrPort AddrPort(this ж<UDPAddr> Ꮡa) => net_package.AddrPortᴍ(Ꮡa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Network(this ref UDPAddr a) => net_package.Networkᴍ(ref a);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<UDPAddr> Ꮡa) => net_package.Stringᴍ(Ꮡa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isWildcard(this ж<UDPAddr> Ꮡa) => net_package.isWildcardᴍ(Ꮡa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Addr opAddr(this ж<UDPAddr> Ꮡa) => net_package.opAddrᴍ(Ꮡa);
}
partial class netᴛaddrPortUDPAddr {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Network(this addrPortUDPAddr _) => net_package.Networkᴍ(_);
}
partial class netᴛUDPConn {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (syscall.RawConn, error) SyscallConn(this ж<UDPConn> Ꮡc) => net_package.SyscallConnᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint n, ж<UDPAddr> addr, error err) ReadFromUDP(this ж<UDPConn> Ꮡc, slice<byte> b) => net_package.ReadFromUDPᴍ(Ꮡc, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, ж<UDPAddr>, error) readFromUDP(this ж<UDPConn> Ꮡc, slice<byte> b, ж<UDPAddr> Ꮡaddr) => net_package.readFromUDPᴍ(Ꮡc, b, Ꮡaddr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, Addr, error) ReadFrom(this ж<UDPConn> Ꮡc, slice<byte> b) => net_package.ReadFromᴍ(Ꮡc, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint n, netip.AddrPort addr, error err) ReadFromUDPAddrPort(this ж<UDPConn> Ꮡc, slice<byte> b) => net_package.ReadFromUDPAddrPortᴍ(Ꮡc, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint n, nint oobn, nint flags, ж<UDPAddr> addr, error err) ReadMsgUDP(this ж<UDPConn> Ꮡc, slice<byte> b, slice<byte> oob) => net_package.ReadMsgUDPᴍ(Ꮡc, b, oob);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint n, nint oobn, nint flags, netip.AddrPort addr, error err) ReadMsgUDPAddrPort(this ж<UDPConn> Ꮡc, slice<byte> b, slice<byte> oob) => net_package.ReadMsgUDPAddrPortᴍ(Ꮡc, b, oob);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) WriteToUDP(this ж<UDPConn> Ꮡc, slice<byte> b, ж<UDPAddr> addr) => net_package.WriteToUDPᴍ(Ꮡc, b, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) WriteToUDPAddrPort(this ж<UDPConn> Ꮡc, slice<byte> b, netip.AddrPort addr) => net_package.WriteToUDPAddrPortᴍ(Ꮡc, b, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) WriteTo(this ж<UDPConn> Ꮡc, slice<byte> b, Addr addr) => net_package.WriteToᴍ(Ꮡc, b, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint n, nint oobn, error err) WriteMsgUDP(this ж<UDPConn> Ꮡc, slice<byte> b, slice<byte> oob, ж<UDPAddr> addr) => net_package.WriteMsgUDPᴍ(Ꮡc, b, oob, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint n, nint oobn, error err) WriteMsgUDPAddrPort(this ж<UDPConn> Ꮡc, slice<byte> b, slice<byte> oob, netip.AddrPort addr) => net_package.WriteMsgUDPAddrPortᴍ(Ꮡc, b, oob, addr);
}
partial class netᴛUDPAddr {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint family(this ж<UDPAddr> Ꮡa) => net_package.familyᴍ(Ꮡa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (syscall.Sockaddr, error) sockaddr(this ж<UDPAddr> Ꮡa, nint family) => net_package.sockaddrᴍ(Ꮡa, family);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static sockaddr toLocal(this ref UDPAddr a, @string net) => net_package.toLocalᴍ(ref a, net);
}
partial class netᴛUDPConn {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, ж<UDPAddr>, error) readFrom(this ref UDPConn c, slice<byte> b, ж<UDPAddr> Ꮡaddr) => net_package.readFromᴍ(ref c, b, Ꮡaddr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, netip.AddrPort addr, error err) readFromAddrPort(this ref UDPConn c, slice<byte> b) => net_package.readFromAddrPortᴍ(ref c, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, nint oobn, nint flags, netip.AddrPort addr, error err) readMsg(this ref UDPConn c, slice<byte> b, slice<byte> oob) => net_package.readMsgᴍ(ref c, b, oob);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) writeTo(this ref UDPConn c, slice<byte> b, ж<UDPAddr> Ꮡaddr) => net_package.writeToᴍ(ref c, b, Ꮡaddr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) writeToAddrPort(this ref UDPConn c, slice<byte> b, netip.AddrPort addr) => net_package.writeToAddrPortᴍ(ref c, b, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, nint oobn, error err) writeMsg(this ref UDPConn c, slice<byte> b, slice<byte> oob, ж<UDPAddr> addr) => net_package.writeMsgᴍ(ref c, b, oob, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, nint oobn, error err) writeMsgAddrPort(this ref UDPConn c, slice<byte> b, slice<byte> oob, netip.AddrPort addr) => net_package.writeMsgAddrPortᴍ(ref c, b, oob, addr);
}
partial class netᴛsysDialer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<UDPConn>, error) dialUDP(this ж<sysDialer> Ꮡsd, context.Context ctx, ж<UDPAddr> laddr, ж<UDPAddr> raddr) => net_package.dialUDPᴍ(Ꮡsd, ctx, laddr, raddr);
}
partial class netᴛsysListener {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<UDPConn>, error) listenUDP(this ж<sysListener> Ꮡsl, context.Context ctx, ж<UDPAddr> laddr) => net_package.listenUDPᴍ(Ꮡsl, ctx, laddr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<UDPConn>, error) listenMulticastUDP(this ж<sysListener> Ꮡsl, context.Context ctx, ж<Interface> ifi, ж<UDPAddr> Ꮡgaddr) => net_package.listenMulticastUDPᴍ(Ꮡsl, ctx, ifi, Ꮡgaddr);
}
partial class netᴛUnixAddr {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Network(this ref UnixAddr a) => net_package.Networkᴍ(ref a);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<UnixAddr> Ꮡa) => net_package.Stringᴍ(Ꮡa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isWildcard(this ж<UnixAddr> Ꮡa) => net_package.isWildcardᴍ(Ꮡa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Addr opAddr(this ж<UnixAddr> Ꮡa) => net_package.opAddrᴍ(Ꮡa);
}
partial class netᴛUnixConn {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (syscall.RawConn, error) SyscallConn(this ж<UnixConn> Ꮡc) => net_package.SyscallConnᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error CloseRead(this ж<UnixConn> Ꮡc) => net_package.CloseReadᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error CloseWrite(this ж<UnixConn> Ꮡc) => net_package.CloseWriteᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, ж<UnixAddr>, error) ReadFromUnix(this ж<UnixConn> Ꮡc, slice<byte> b) => net_package.ReadFromUnixᴍ(Ꮡc, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, Addr, error) ReadFrom(this ж<UnixConn> Ꮡc, slice<byte> b) => net_package.ReadFromᴍ(Ꮡc, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint n, nint oobn, nint flags, ж<UnixAddr> addr, error err) ReadMsgUnix(this ж<UnixConn> Ꮡc, slice<byte> b, slice<byte> oob) => net_package.ReadMsgUnixᴍ(Ꮡc, b, oob);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) WriteToUnix(this ж<UnixConn> Ꮡc, slice<byte> b, ж<UnixAddr> addr) => net_package.WriteToUnixᴍ(Ꮡc, b, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) WriteTo(this ж<UnixConn> Ꮡc, slice<byte> b, Addr addr) => net_package.WriteToᴍ(Ꮡc, b, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint n, nint oobn, error err) WriteMsgUnix(this ж<UnixConn> Ꮡc, slice<byte> b, slice<byte> oob, ж<UnixAddr> addr) => net_package.WriteMsgUnixᴍ(Ꮡc, b, oob, addr);
}
partial class netᴛUnixListener {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool ok(this ж<UnixListener> Ꮡln) => net_package.okᴍ(Ꮡln);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (syscall.RawConn, error) SyscallConn(this ж<UnixListener> Ꮡl) => net_package.SyscallConnᴍ(Ꮡl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<UnixConn>, error) AcceptUnix(this ж<UnixListener> Ꮡl) => net_package.AcceptUnixᴍ(Ꮡl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (Conn, error) Accept(this ж<UnixListener> Ꮡl) => net_package.Acceptᴍ(Ꮡl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Close(this ж<UnixListener> Ꮡl) => net_package.Closeᴍ(Ꮡl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Addr Addr(this ref UnixListener l) => net_package.Addrᴍ(ref l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SetDeadline(this ж<UnixListener> Ꮡl, time.Time t) => net_package.SetDeadlineᴍ(Ꮡl, t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<os.File> f, error err) File(this ж<UnixListener> Ꮡl) => net_package.Fileᴍ(Ꮡl);
}
partial class netᴛUnixAddr {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint family(this ref UnixAddr a) => net_package.familyᴍ(ref a);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (syscall.Sockaddr, error) sockaddr(this ж<UnixAddr> Ꮡa, nint family) => net_package.sockaddrᴍ(Ꮡa, family);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static sockaddr toLocal(this ж<UnixAddr> Ꮡa, @string net) => net_package.toLocalᴍ(Ꮡa, net);
}
partial class netᴛUnixConn {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, ж<UnixAddr>, error) readFrom(this ref UnixConn c, slice<byte> b) => net_package.readFromᴍ(ref c, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, nint oobn, nint flags, ж<UnixAddr> addr, error err) readMsg(this ref UnixConn c, slice<byte> b, slice<byte> oob) => net_package.readMsgᴍ(ref c, b, oob);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) writeTo(this ref UnixConn c, slice<byte> b, ж<UnixAddr> Ꮡaddr) => net_package.writeToᴍ(ref c, b, Ꮡaddr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, nint oobn, error err) writeMsg(this ref UnixConn c, slice<byte> b, slice<byte> oob, ж<UnixAddr> Ꮡaddr) => net_package.writeMsgᴍ(ref c, b, oob, Ꮡaddr);
}
partial class netᴛsysDialer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<UnixConn>, error) dialUnix(this ж<sysDialer> Ꮡsd, context.Context ctx, ж<UnixAddr> laddr, ж<UnixAddr> raddr) => net_package.dialUnixᴍ(Ꮡsd, ctx, laddr, raddr);
}
partial class netᴛUnixListener {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<UnixConn>, error) accept(this ref UnixListener ln) => net_package.acceptᴍ(ref ln);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error close(this ж<UnixListener> Ꮡln) => net_package.closeᴍ(Ꮡln);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<os.File>, error) @file(this ref UnixListener ln) => net_package.@fileᴍ(ref ln);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetUnlinkOnClose(this ref UnixListener l, bool unlink) => net_package.SetUnlinkOnCloseᴍ(ref l, unlink);
}
partial class netᴛsysListener {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<UnixListener>, error) listenUnix(this ж<sysListener> Ꮡsl, context.Context ctx, ж<UnixAddr> Ꮡladdr) => net_package.listenUnixᴍ(Ꮡsl, ctx, Ꮡladdr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<UnixConn>, error) listenUnixgram(this ж<sysListener> Ꮡsl, context.Context ctx, ж<UnixAddr> laddr) => net_package.listenUnixgramᴍ(Ꮡsl, ctx, laddr);
}
partial class netᴛconn {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64, error) writeBuffers(this ж<conn> Ꮡc, ж<Buffers> v) => net_package.writeBuffersᴍ(Ꮡc, v);
}
partial class netᴛnetFD {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64 n, error err) writeBuffers(this ж<netFD> Ꮡfd, ж<Buffers> v) => net_package.writeBuffersᴍ(Ꮡfd, v);
}
// </GlyphForwarders>
