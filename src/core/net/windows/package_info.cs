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
global using dnsmessageꓸCNAMEResource = go.vendor.golang.org.x.net.dns.dnsmessage_package.ΔCNAMEResource;
global using dnsmessageꓸOPTResource = go.vendor.golang.org.x.net.dns.dnsmessage_package.ΔOPTResource;
global using dnsmessageꓸQuestion = go.vendor.golang.org.x.net.dns.dnsmessage_package.ΔQuestion;
global using netipꓸAddr = go.net.netip_package.ΔAddr;
global using netipꓸPrefix = go.net.netip_package.ΔPrefix;
global using syscallꓸHandle = go.syscall_package.ΔHandle;
global using syscallꓸSockaddr = go.syscall_package.ΔSockaddr;
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
[assembly: GoTypeAlias("Addr", "ΔAddr")]
[assembly: GoTypeAlias("Error", "ΔError")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<AddrError, error>(Pointer = true)]
[assembly: GoImplement<AddrError, ΔError>(Pointer = true)]
[assembly: GoImplement<Buffers, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<Buffers, io_package.WriterTo>(Pointer = true)]
[assembly: GoImplement<Conn, io_package.Reader>]
[assembly: GoImplement<DNSConfigError, ΔError>(Pointer = true)]
[assembly: GoImplement<DNSError, error>(Pointer = true)]
[assembly: GoImplement<DNSError, ΔError>(Pointer = true)]
[assembly: GoImplement<IPAddr, ΔAddr>(Pointer = true)]
[assembly: GoImplement<IPAddr, Δsockaddr>(Pointer = true)]
[assembly: GoImplement<IPConn, Conn>(Pointer = true)]
[assembly: GoImplement<IPConn, PacketConn>(Pointer = true)]
[assembly: GoImplement<IPNet, ΔAddr>(Pointer = true)]
[assembly: GoImplement<InvalidAddrError, ΔError>(Pointer = true)]
[assembly: GoImplement<InvalidAddrError, ΔError>]
[assembly: GoImplement<OpError, error>(Pointer = true)]
[assembly: GoImplement<OpError, ΔError>(Pointer = true)]
[assembly: GoImplement<ParseError, error>(Pointer = true)]
[assembly: GoImplement<ParseError, ΔError>(Pointer = true)]
[assembly: GoImplement<TCPAddr, ΔAddr>(Pointer = true)]
[assembly: GoImplement<TCPAddr, Δsockaddr>(Pointer = true)]
[assembly: GoImplement<TCPConn, Conn>(Pointer = true)]
[assembly: GoImplement<TCPListener, Listener>(Pointer = true)]
[assembly: GoImplement<UDPAddr, ΔAddr>(Pointer = true)]
[assembly: GoImplement<UDPAddr, Δsockaddr>(Pointer = true)]
[assembly: GoImplement<UDPConn, Conn>(Pointer = true)]
[assembly: GoImplement<UDPConn, PacketConn>(Pointer = true)]
[assembly: GoImplement<UnixAddr, ΔAddr>(Pointer = true)]
[assembly: GoImplement<UnixAddr, Δsockaddr>(Pointer = true)]
[assembly: GoImplement<UnixConn, Conn>(Pointer = true)]
[assembly: GoImplement<UnixConn, PacketConn>(Pointer = true)]
[assembly: GoImplement<UnixListener, Listener>(Pointer = true)]
[assembly: GoImplement<UnknownNetworkError, ΔError>(Pointer = true)]
[assembly: GoImplement<UnknownNetworkError, ΔError>]
[assembly: GoImplement<addrPortUDPAddr, ΔAddr>]
[assembly: GoImplement<canceledError, error>]
[assembly: GoImplement<dialParallel_dialResult, Conn>(Promoted = true)]
[assembly: GoImplement<dialParallel_dialResult, error>(Promoted = true)]
[assembly: GoImplement<fileAddr, ΔAddr>]
[assembly: GoImplement<goLookupIPCNAMEOrder_result, error>(Promoted = true)]
[assembly: GoImplement<notFoundError, error>(Pointer = true)]
[assembly: GoImplement<onlyValuesCtx, context_package.Context>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<pipe, Conn>(Pointer = true)]
[assembly: GoImplement<pipeAddr, ΔAddr>]
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
[assembly: GoFieldAccess(typeof(byRFC6724Info))]
[assembly: GoFieldAccess(typeof(conf))]
[assembly: GoFieldAccess(typeof(dnsConfig))]
[assembly: GoFieldAccess(typeof(go.@internal.nettrace_package.Trace))]
[assembly: GoFieldAccess(typeof(go.@internal.poll_package.FD))]
[assembly: GoFieldAccess(typeof(go.@internal.syscall.windows_package.IpAdapterAddresses))]
[assembly: GoFieldAccess(typeof(go.@internal.syscall.windows_package.IpAdapterAnycastAddress))]
[assembly: GoFieldAccess(typeof(go.@internal.syscall.windows_package.IpAdapterDnsServerAdapter))]
[assembly: GoFieldAccess(typeof(go.@internal.syscall.windows_package.IpAdapterMulticastAddress))]
[assembly: GoFieldAccess(typeof(go.@internal.syscall.windows_package.IpAdapterUnicastAddress))]
[assembly: GoFieldAccess(typeof(go.vendor.golang.org.x.net.dns.dnsmessage_package.ΔCNAMEResource))]
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
[assembly: GoFieldAccess(typeof(syscall_package.AddrinfoW))]
[assembly: GoFieldAccess(typeof(syscall_package.DNSMXData))]
[assembly: GoFieldAccess(typeof(syscall_package.DNSPTRData))]
[assembly: GoFieldAccess(typeof(syscall_package.DNSRecord))]
[assembly: GoFieldAccess(typeof(syscall_package.DNSSRVData))]
[assembly: GoFieldAccess(typeof(syscall_package.DNSTXTData))]
[assembly: GoFieldAccess(typeof(syscall_package.IPv6Mreq))]
[assembly: GoFieldAccess(typeof(syscall_package.Protoent))]
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
[assembly: GoPositionMap("net/conf.go", "conf.cs", "AEWUAZiSggAMCsKCgoKUgoKClIKUlIKUxoKUxoKCgpQAChSmgriCgrik7oKCgsqCgrSoogAIEMrKpr7SgqYABxCCuKwACQKCgqbsAAkCgoKmABIGpKSCuIKClIKUyqa4grikAAgSlJSmpsq4gqaCypSUlKSCgqaUpIKCppS2AAcQlIKmgrimlKamgpSSgoKClJSClIKClIKUpoK4gpSCgpS0yqyylIKUlLS0pIKUxriCgoKCggAHEIKCgu6UgpTGpLimAAEcAA8CgoKClIKUpoKCgpSCAAwKouqi6qI=", "92-121:1;222-224:1;234-236:1;500-509:1", "", "", "", "99:25,100:17,247:25,248:17,269:25,270:17")]
[assembly: GoPositionMap("net/dial.go", "dial.cs", "AChUAAoagpSkuIKUpqaCgpQADB6ClKTKgpSmpoKClABq7AGUgoKUgpQAAhIACAKClIKUpoKClKqigpSCgqaEkoKClKamgoKUuLKCggAICIL4lJSCpIKCgoKCppSssoKClIKUpIKClIKUlIKClLyUgrSCtIKkgoKClJSClLSClLSClLaClK7CAAIS4gACZgAwAoIAAhoACgKCAAki8gAGKgAWAoK2goKCgoKmgoKm3JKClKbWgoKUgpKCgoKCgqaAgoKytOikooKUggAEGgAMAoKSAAUWAAwCgpIABRYADAKCkgAFFgAMAoKSAAwO4oKmgoQAARDCgoKUguaC2raCkraCpIK0gpLGgpSClJSClMoACRDilIKk2IKAgoKUgpSUgoKC2IKClIK4gpTaAAgCgoKCgpSCkLaClIKCpNaCxILEgtSUgpQAKVrCAAIS4gAGFPKCgpTKgoKUgqTW1JSClAACFPKCgpTKgoKUxMTUlIKUAAg8ABUCggACOAAZAoI=", "576-582:1;585-592:2;671-684:1;781-781:1", "", "", "547:21,646:21,668:21,690:21,712:21,738:21,771:21,778:21,787:29,853:37", "893:29,894:21")]
[assembly: GoPositionMap("net/dnsclient.go", "dnsclient.cs", "ABAoxIKmgqzSgoKUgqaUgoLcgqaygpSCgoKClIKUgqYAAiAADgSCAAoWgoKmgoKCgoKUpILGxoKUgsaClIKUpJSCpgACFgAIAoKUAA0gooKClIKCgoKCgoKUpoK6koKClJSCgoKCpgALGpKCgpQ=", "200-205:1;231-233:1")]
[assembly: GoPositionMap("net/dnsclient_unix.go", "dnsclient_unix.cs", "AClWgoKCgoLKqJTigoKClIKmgqaClIKClIK4goKUgoKCgqaygpSClIKUpqKAgraCgoKClLiCgpSCgpS4ooKmgoKUgoKUgoKUgoKClIKClIKUqNKCgoKUgoKUlIKCpIKClIKUgoKClJSCgpSCAAgSgpSU2LKUgqaCgsqCptyClKamooKCgpSClIKUggAEELKCgoKCgoKUgoKUgs7CgoKUgoKU3IKClJKCpoKUgqaCppSCpoKmlIKmpgATIoKCqNaCuKwACAKUgriClJSCgpSUAAoQgoKUgsiCoqSCpKS4gqai3KaCpqqCgoKUpqaClLiU7sKClIKopIKCgriCgpSmgoKmlIKmgoKCuIKUAA4iAAcQgoKUprKUgoKmgqaCgpSCspSowoKCgoKCpoKq0oIACAbSgoKUgoKCgpSmgriUlIyCpoKCgpSUtKSCgoKSooKCgtaSgpKSgqaogoKClIKCgoKUgoKUlAARJIKCgoLcgpSUgoLKlIKCyIKCypSCgsiCgsqUgsiCypTouIKUgqa4lIKCgoKCgoKCgpSmgqbokoKosoKCgqaCuIKClJKCgoKCgriUgoKCgpSC3IKCgtyUgoLcqA==", "656-656:1;657-662:2;664-671:3;666-670:3.1;672-674:4", "", "793=Error/1/1/1;806=Error/1/1/1;822=Error/1/1/1;838=Error/1/1/1;852=Error/1/1/1;953=Error/1/1/1;962=Error/1/1/1;972=Error/1/1/1", "201:25")]
[assembly: GoPositionMap("net/dnsconfig.go", "dnsconfig.cs", "ABk0lAATMOKClA==")]
[assembly: GoPositionMap("net/dnsconfig_windows.go", "dnsconfig_windows.cs", "AAwassqCgqaCgqaUgriCpoKCgpSClLSCgtzopKY=", "19-23:1", "", "", "", "22:21,23:13")]
[assembly: GoPositionMap("net/error_posix.go", "error_posix.cs", "AAsgooKU")]
[assembly: GoPositionMap("net/error_windows.go", "error_windows.cs", "AAkSgoKU")]
[assembly: GoPositionMap("net/fd_posix.go", "fd_posix.cs", "ABo6gpKClIKUpqKClKaUguaCgoKmgqaCprKCgqbCgoKUsoKCprKCgqbigoKm0oKCptKCgqaygoKmsoKCprKCgqaygoKmwoKCpsKCgqbCgoKmgqaCpoI=")]
[assembly: GoPositionMap("net/fd_windows.go", "fd_windows.cs", "AB80gqqipKamggAJFOaigIKkyIKCgoKCuIKCgoKCtgAMCAAICICCtsqUgpSCpoK2gpSiptyCgqaClLS0pIK4gpS0xKSmuJSUpriAgqSkgpTI1qKClIKClOaCgoKmopiCgpS4goKClIKCqKKSlIKUgqjE2IKUgoKClIKUgso=", "107-112:1;113-119:2;202-204:1;248-250:1", "", "", "", "149:29,150:21")]
[assembly: GoPositionMap("net/file.go", "file.cs", "AA8ckIKcAAgCgoKUrgAIAoKClK4ACAKCgpQ=")]
[assembly: GoPositionMap("net/file_posix.go", "file_posix.cs", "ABQegoKClIKCgoKUgoKClIKUtLS0gpSCgoKUgoKCgoKUgqaCgoKUlKSkpJSCpoKCgpSUpJSCpoKCgpSUpKSUgg==")]
[assembly: GoPositionMap("net/file_windows.go", "file_windows.cs", "AA4egoKCgpSm3JSCgqaCgoKUgpSClA==", "39-41:1")]
[assembly: GoPositionMap("net/hook.go", "hook.cs", "AAweAAAQkuw=")]
[assembly: GoPositionMap("net/hook_windows.go", "hook_windows.cs", "AAsapoKC")]
[assembly: GoPositionMap("net/hosts.go", "hosts.cs", "ABMkgoKClAAbOqKClIKUgoKCpoKUgoKCuIKSgoCUpIKClIKCpoKCgoKClIKmlIK4pgAHEIKCgoKCoqaygoKCgoKCgpSCgoKm2LKCgoKCgpSCgoKCpg==")]
[assembly: GoPositionMap("net/interface.go", "interface.cs", "ABIsgoKCggAVMgAIEoKCgoKClKaClKqigpSCgpSqooKUgoKUqJKCgpSClAACENKCgpQAAhDSgpSCgpSCgpSmgrKCpqiSgpSCgpSClLKCpgAPIAAFEAAIAoKCgoKUgoKCgqaCgoKCgoK45oKClIKCgoKCgoKClIKUpoKClIKCgoKCgoKClIKU")]
[assembly: GoPositionMap("net/interface_windows.go", "interface_windows.cs", "AA9gsoKClIKCgoKUgriCgtyUtLS0pIKUlIKClIKCuOzSgoKUgoKCgpSCgoKClJS0goK2goKClJS0goLaqsKCgpSCgoKClIKCgoKUlLSCgto=", "", "", "31=UTF16PtrToString/1/1/2")]
[assembly: GoPositionMap("net/ip.go", "ip.cs", "ADBqooKCgoKCgqaYooKCgoKCrLKClIKUgoKCgoKCgpSClLqSkpK6kpKSkpKqoqiSgpSqou68qJKClKqiqqKClKqigpQAAhYACAIAAhKSgoKmqqKClIiUqqKClIKUuoKCnLKClJSkpLiCgoKmqJKClIKUgoKUgoKUAAYS4oKmgqaClIK0gqSCpoKCgpSqooKUqrSClIKssoKUgqasxIKClKqigoKUgoKClIKssoKUgpSClKaCqqKCsoKCuIKCpoKUgoKmlKzigoKUqJKClKaygoKCpoKUgsaCxpSokoKClIKClIKCpuiAAAEQ4oKUgoKUgoKUAAIS4oKUpoKCgpQAAhYACAKCgqaCgqaCgpSCgqaCgoI=", "", "", "195=Equal/1/1/1,IsUnspecified/1/1/2,IsLoopback/1/1/3,IsMulticast/1/1/4,IsLinkLocalUnicast/1/1/5;214=isZeros/1/1/1")]
[assembly: GoPositionMap("net/iprawsock.go", "iprawsock.cs", "AAxEABICAAsagJSigpSCgpSmooKUpoKClAACIAANAoKUgoKU1pSCgpQACBbCgpSosoKUgoKUqLKClIKClIKUAAIUAA4CgpSCgpSosoKUgoKUqLKClIKClIKClAACEgALAoKUgoKUpgABEvKmooKUgoKUgoKUAAIUAAkCgpSCgoKU", "", "", "32=AsSlice/1/1/1,Zone/1/1/2")]
[assembly: GoPositionMap("net/iprawsock_posix.go", "iprawsock_posix.cs", "AAscgpSklKaigpSClKaigpSmgqaGooKUgrSkpoKClIKClIKUgqbigoKUtKSmgoKUgpSCgpSmooKUgpSCgpSmooKClNaUgoKYgoKUpqKCgpTWlIKCmIKClA==", "127-129:1;150-152:1")]
[assembly: GoPositionMap("net/ipsock.go", "ipsock.cs", "AB5CooKqooKs1qSmgt6SlKSklKiAmrKCtsakgpSqooKCpgACEgAJAoKCgoKClKau4oKCgqaClLiSqJIAEhgADAKYgpS2goKmlIKClKYACAyClJSClIKCpoKUgqaCptaClJQAAhD2gpSuwrqkgoKUgtiCxpSSpKSkpLaCuIKC3IKmgoKUgpQAAhwACwKClA==", "170-172:1;272-283:1")]
[assembly: GoPositionMap("net/ipsock_posix.go", "ipsock_posix.cs", "ABY4AA4CtoKCgqaCxoKkAAUS2qSCgoKUgoKCgpSClIKUxgACagAzApSkpoKClIKUpoSUpoKkgraCpoKClIKClIKCpgAKFoK4goKUgoIAAhwACwKUkoKUpJKClJSm7oKClLim7oLK", "", "", "58=ParseIP/1/1/5,IPv4/1/1/7;168=family/2/2/1;268=As4/1/1/1,Port/1/1/2;283=As16/1/1/1,Port/1/1/2,Zone/1/1/3,index/1/1/3")]
[assembly: GoPositionMap("net/lookup.go", "lookup.cs", "ABs0AA0cABo4goKCgoKClAANFIKkgpSkpJSmgoKCgoKClJSqooKUgoKUygAlVsCioKSCgpQAAhDSqrSClIKUqqKCgpSCgpSqoq7CgoKU1qaClIKCpoKClK4ACAqCgpSCgoKmABUUlpKkpAADEMKqtIKUgIKkgoLKgoIABxCUgoKitoKCgpQAChKCgrSkgoKUpIKCgoKCpoKClMyigpSCgoKClKiSgoKUrsKs4oKC1rSUgoKmgpQAAiYAEAIAAiAADQKCgpSClAACIgAOAgACIgAOAoKClIKUgoKClIKUlIKUAAIYAAkCAAIS4oKClIKCgpSmgoKmlIKUAAIYAAkCAAIS4oKClIKCgpSClJSClAACFPKuwgACHgAMAgACEuKCgpSCgoKmgpQABxbc0oKClIKUgpQAAhgADQKClJSSgpSCgoKClILcgoLclIKUgoLclIKokpKClIKCgoKUgtyCgtyUgoLcloKospKClIKCgoKUgtyCgtyUgoLclLiSkoKUgoKCgpSC3IKC3JSCggAIEoKClIKClIKUlAAIBoKClIKClA==", "334-336:1;338-342:2;354-354:3")]
[assembly: GoPositionMap("net/mac.go", "mac.cs", "AA0YooKUgoKClIKUAAIgAA8CgqaCgpSCgpSCgoKClJSCgpSCgpSCgoKClIKUpoKmgoKmgoKCgpSmlII=")]
[assembly: GoPositionMap("net/mptcpsock_stub.go", "mptcpsock_stub.cs", "AAoagqaCpoI=")]
[assembly: GoPositionMap("net/net.go", "net.cs", "ALYB9gKgqtKClIKClKiygpSCgpSosoKUgoKUrNKClKzSgpSosoKUgpSosoKUgpSosoKUgpSqwoKUgpSqwoKUgpQAAhoADQKCgpQARaoBAA0CgJIAHkCmpoIACg6UmKKUpKQAHUSUooKUgoKUgpSCgpSUlILMuILegoKClILupoKmgoKUggALGpSC7qKClIKClKaCuIKCuIKCAAwaAAgIgoKUggAIEoKCgqiC/vyCggAPJqK8goLKgqaSAAoYgJSigpSCgpSCrKCaoLjuAAYQogAKGrQABxKiAAoYpAAMGoKClLSkyIIAESaCAAESAAkCgpSCgoKCgqaCAAISAAkCgoKCgpSClKaCgoKCgpSCgg==", "405-405:1;810-812:1", "", "733=Error/1/1/2")]
[assembly: GoPositionMap("net/nss.go", "nss.cs", "ACBCgoKCgoLokoKCqOKmgpSUgoKUlIKClIKmgoKCoqSCpoKkpLiCAA4iooKCpgAQHLKClIKktMaUgpSmooKClJKCgqaCggALBqKCgoKClIKCgpSCgoKCgpSCgoKClIKUlIKCgoKUgoKCgpSUgpTcAAgIwoKCgoKUgpSCgpSCgoKUypQ=", "223-247:1")]
[assembly: GoPositionMap("net/parse.go", "parse.cs", "ABculLKCgoKCgpSCgoKCppSCgpSmsoKUgoKCgpSCpoKmooKClKaCgoKUpqKCgpSokoKCgqaokoKCgoKCgoKUpoKClKbO0oKCgoKmgpSq0oKCgoKCgoKCgoKUlIKmgpSuwoKUgqiSgoKmqJKCgsySgpSokoKUgpSokqqigpSqooKCgoKUgoKmlKqiqqKClIKCpg==")]
[assembly: GoPositionMap("net/pipe.go", "pipe.cs", "ABMqggACEgALAoKUgpSmgoKClLiAgoKUgpTIgrSm4oKC1oKkpAAKDIIAEjjSgoKCgoKU7u6mgpSCgoKUpqKUpKSm1IKCpKSkyIKCgpSm4pSkpKaCgoLUgoK0pKSm5qKClIKCpqKClIKmooKUgqaCgJI=", "54-56:1;236-236:1", "", "141=makePipeDeadline/1/2/4,makePipeDeadline/2/2/5;148=makePipeDeadline/1/2/4,makePipeDeadline/2/2/5")]
[assembly: GoPositionMap("net/port.go", "port.cs", "AAceAAoCppSYgoKCgoKUgqKClJSCgpSCgoKClJSCgoKUlIKU")]
[assembly: GoPositionMap("net/rawconn.go", "rawconn.cs", "ABc0oKSigpSCgoKUpqKClIKCgpSmooKUgoKClAACFAAJAoKUpoIAAhTy7oKmgqaC")]
[assembly: GoPositionMap("net/sendfile.go", "sendfile.cs", "ABMwAAsCgpSCgoKCgsqCgqaCgqaCgoKUgqaCpg==", "49-52:1")]
[assembly: GoPositionMap("net/sendfile_windows.go", "sendfile_windows.cs", "AAoc0oKU")]
[assembly: GoPositionMap("net/sock_posix.go", "sock_posix.cs", "AA4k0oKClIKClIKCABgygpSCgpSkgoKUpoKClKaCpJSUlIKUpqKCgoKCgoKClIK4goKCgoKCgriCgoKClIKUlIIACBKCgoKAgpSkpqKCgpSCgqaCgoK4gpSClIKUgoKmogAIEoKClIKUtKS2goKCpoKCgqaClIKUgoI=")]
[assembly: GoPositionMap("net/sock_windows.go", "sock_windows.cs", "AAwauOaClIKU")]
[assembly: GoPositionMap("net/sockaddr_posix.go", "sockaddr_posix.cs", "AB5IgpSUpKTGlKSktg==", "57-57:1")]
[assembly: GoPositionMap("net/sockopt_posix.go", "sockopt_posix.cs", "AAwekoKUpoKClIKClIKUgsaCyKaCgoKmgoKCpoKCgqaCgoKClIKUgoI=")]
[assembly: GoPositionMap("net/sockopt_windows.go", "sockopt_windows.cs", "AAoYgriUlJSm7qam")]
[assembly: GoPositionMap("net/sockoptip4_posix_nonlinux.go", "sockoptip4_posix_nonlinux.cs", "AA0egoKClIKCpoKClIKClIKUgIKC1oCCgtiCgpQ=")]
[assembly: GoPositionMap("net/sockoptip4_windows.go", "sockoptip4_windows.cs", "AAwagoKClIKCgoKmgoKC")]
[assembly: GoPositionMap("net/sockoptip6_posix.go", "sockoptip6_posix.cs", "AAwcooKClIKCpoKCgqaigoKClIKC")]
[assembly: GoPositionMap("net/splice_stub.go", "splice_stub.cs", "AAoWgqaC")]
[assembly: GoPositionMap("net/tcpsock.go", "tcpsock.cs", "ABtA8oKUgoKogJSigpSCgpSmooKUpoKClAACJAAPAta0lIKClKyyACxkwoKUqLKClIKClKiygpSCgpSqwoKUgpSqwoKUgpQAAiIAEAKClIKUqsKClIKUAAIQ8oKUgpSu4oKUgpQAAhgACwKClKaCgoLKgoKClIKCpgACFPKmotaUgpSCmIKUgpSUgpQACR7ygpSqwoKUgoKUqsKClIKClKrCgpSClKygmMKClAACGgAOAoKUgoKUAAIYAAsC1pSClIKYgpSUgpSokg==", "", "", "115=Addr/1/2/1,AsSlice/1/1/1,Addr/2/2/2,Zone/1/1/2,Port/1/1/3")]
[assembly: GoPositionMap("net/tcpsock_posix.go", "tcpsock_posix.cs", "AA4ggpSklKaigpSClKaigpSmgqaigIKkgIKkpqKClKaigIKkgIKkpoKmsoKCmAAZNIKClKaClKaUggAKFoKUgoKmgoKUgpSmoKSCgoKUpoKmgoKClKaCpqKCgpiCgpQ=", "81-83:1;185-187:1")]
[assembly: GoPositionMap("net/tcpsock_windows.go", "tcpsock_windows.cs", "AAwasoKmgIKkgoCCpICCpIKUgIK2")]
[assembly: GoPositionMap("net/tcpsockopt_posix.go", "tcpsockopt_posix.cs", "AAwcgoKC")]
[assembly: GoPositionMap("net/tcpsockopt_windows.go", "tcpsockopt_windows.cs", "ABUugoKmgoKCpoKCgqaCgqaCgoKmgoKCpoKCgoKmgoKoAAgQuP7G6oKUgsqCgsqCgoKC")]
[assembly: GoPositionMap("net/udpsock.go", "udpsock.cs", "ABpA8oKUgoKogJSigpSCgpSmooKUpoKClAACJAAPAta0lIKClKyyAAsYAAcUwoKUqNqowoKUgoKUqJKClJQAAhAACgKClIKClAACFAANAoKCgpSoAAgCgpSCgpSosoKUgoKUqLKClIKClKiygpSCgpSCgpQAAhYADQKClIKClKjigpSCgpSmAAES8qai1pSClIKClIKClAACGAALAtaUgpSCgoKUAAIuABYC1pSClIKCgpQ=", "", "", "114=Addr/1/2/1,AsSlice/1/1/1,Addr/2/2/2,Zone/1/1/2,Port/1/1/3")]
[assembly: GoPositionMap("net/udpsock_posix.go", "udpsock_posix.cs", "AA0egpSklKaigpSClKaigpSmgqaigoKUgoKCgsaCgoKCtpSUpsKCgpSCgoKCxoKCgoK2gpSm4pSCgoK0goKCpKaigpSCppSSgpSkkoKUpLiCgpSCppSSgpSkkoKUpLiigpSClIKClKaCgpSCppSCgpKClJSkgoKSgpSUpLiigoKYgoKUpqKCgpiCgpSmsoKCmIKClIKCgoKmgoKmpoKCgqaAgqSAgqSmgoKCpoCCpICCpA==", "216-218:1;230-232:1;244-246:1")]
[assembly: GoPositionMap("net/unixsock.go", "unixsock.cs", "ABg6oqaigpSmoqaCgpQAAhLipKQACRjCgpSqwoKUgpSqwoKUgpSosoKUgoKUqLKClIKClIKUAAIUAA4CgpSCgpSosoKUgoKUqLKClIKClIKClAACEgALAoKUgoKUpgABEOKmotaUgoKUgoKUAAwaoK7ygpSqwoKUgoKUqsKClIKClKrCgpSClKygmMKClAACGgAOAoKUgoKUrLLWlIKUgoKClKyy1pSClIKCgpQ=")]
[assembly: GoPositionMap("net/unixsock_posix.go", "unixsock_posix.cs", "AA0ggoKktLS0pqSClIKUgvimgoKUpoKClKaCgpSmgoKUAAoGgpSkpKTIgqaigpSmgqaCgoKUgram4oKCgqaUgramooKUgpSClIKmooKUgoKClJSmooKCmIKClKaCgoKUpgANGIKCpqaCgoKUAAIWAAgCprKCgpiCgpSmooKCmIKClA==", "160-162:1;191-195:1;222-224:1;236-238:1")]
[assembly: GoPositionMap("net/unixsock_readmsg_other.go", "unixsock_readmsg_other.cs", "AAoW")]
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
    internal partial interface temporary {}
    internal partial interface timeout {}
    internal partial interface Δsockaddr {}
    internal partial struct addrList {}
    internal partial struct addrPortUDPAddr {}
    internal partial struct byName {}
    internal partial struct byPref {}
    internal partial struct byPriorityWeight {}
    internal partial struct byRFC6724Info {}
    internal partial struct canceledError {}
    internal partial struct conf {}
    internal partial struct conn {}
    [GoLocalName("dialResult")] internal partial struct dialParallel_dialResult {}
    internal partial struct dnsConfig {}
    internal partial struct fileAddr {}
    [GoLocalName("result")] internal partial struct goLookupIPCNAMEOrder_result {}
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
    internal partial struct Δfile {}
    internal partial struct ΔhostLookupOrder {}
    public partial interface Conn {}
    public partial interface Listener {}
    public partial interface PacketConn {}
    public partial interface ΔAddr {}
    public partial interface ΔError {}
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
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsingleflight() => builtin.initPackage(typeof(@internal.singleflight_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸstrconv() => builtin.initPackage(typeof(@internal.strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸstringslite() => builtin.initPackage(typeof(@internal.stringslite_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyscallꓸwindows() => builtin.initPackage(typeof(@internal.syscall.windows_package));
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
