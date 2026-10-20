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
global using dnsmessageꓸAAAAResource = go.vendor.golang.org.x.net.dns.dnsmessage_package.ΔAAAAResource;
global using dnsmessageꓸAResource = go.vendor.golang.org.x.net.dns.dnsmessage_package.ΔAResource;
global using dnsmessageꓸCNAMEResource = go.vendor.golang.org.x.net.dns.dnsmessage_package.ΔCNAMEResource;
global using dnsmessageꓸHTTPSResource = go.vendor.golang.org.x.net.dns.dnsmessage_package.ΔHTTPSResource;
global using dnsmessageꓸMXResource = go.vendor.golang.org.x.net.dns.dnsmessage_package.ΔMXResource;
global using dnsmessageꓸNSResource = go.vendor.golang.org.x.net.dns.dnsmessage_package.ΔNSResource;
global using dnsmessageꓸOPTResource = go.vendor.golang.org.x.net.dns.dnsmessage_package.ΔOPTResource;
global using dnsmessageꓸPTRResource = go.vendor.golang.org.x.net.dns.dnsmessage_package.ΔPTRResource;
global using dnsmessageꓸQuestion = go.vendor.golang.org.x.net.dns.dnsmessage_package.ΔQuestion;
global using dnsmessageꓸSOAResource = go.vendor.golang.org.x.net.dns.dnsmessage_package.ΔSOAResource;
global using dnsmessageꓸSRVResource = go.vendor.golang.org.x.net.dns.dnsmessage_package.ΔSRVResource;
global using dnsmessageꓸSVCBResource = go.vendor.golang.org.x.net.dns.dnsmessage_package.ΔSVCBResource;
global using dnsmessageꓸTXTResource = go.vendor.golang.org.x.net.dns.dnsmessage_package.ΔTXTResource;
global using dnsmessageꓸUnknownResource = go.vendor.golang.org.x.net.dns.dnsmessage_package.ΔUnknownResource;
global using netipꓸAddr = go.net.netip_package.ΔAddr;
global using netipꓸPrefix = go.net.netip_package.ΔPrefix;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using runtimeꓸError = go.runtime_package.ΔError;
global using strconvꓸError = go.@internal.strconv_package.ΔError;
global using syscallꓸSignal = go.syscall_package.ΔSignal;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
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
[assembly: GoImplement<addrinfoErrno, ΔError>]
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

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("net/addrselect.go", "addrselect.cs", "AAwcgoKUpoKClIKCgu6Cgu6ygpKCgoKCgoCCpKYACBKCgpSCAA0k8oKCgoKCgoKUgtyClIKUgu6ClIIAFSyElITcgpSCAAwagpSCAAsYgoKUgpSC7gAMGgA7fsSClIKCpgAMHoKClIKCgriClAACGgAMAoCCpIKCpoKClIKCgoKClIKCgoKCgoKCuA==", "", "", "27=ipAttrOf/1/2/2,ipAttrOf/2/2/4;77=classifyScope/1/1/1;226=AddrFrom16/1/9/3,PrefixFrom/1/9/3,AddrFrom16/2/9/10,PrefixFrom/2/9/10,AddrFrom16/3/9/16,PrefixFrom/3/9/16,AddrFrom16/4/9/23,PrefixFrom/4/9/23,AddrFrom16/5/9/30,PrefixFrom/5/9/30,AddrFrom16/6/9/36,PrefixFrom/6/9/36,AddrFrom16/7/9/42,PrefixFrom/7/9/42,AddrFrom16/8/9/48,PrefixFrom/8/9/48,AddrFrom16/9/9/54,PrefixFrom/9/9/54")]
[assembly: go.GoPositionMap("net/cgo_unix.go", "cgo_unix.cs", "ACNGgKKAooCmAAgO8oCCgraCgpaKgrKCgoKipLSkgviygoKUspSmgoK2gqSCpJSUtLaqwoKCpoKUgoKClIKUpKSmlIKUgoKkgoKm5sKCgoKClLS2goKUgoKCpAAHEJSkpNyUpKiUlIKUlIKCpIKCpuaCAA0kooKClIKCprqigoKCgoKCpoKUgpSkpKaAgqSmgoCCpICCpKaygoKUgoKUqqLaooKCgoKCgoKUgIKkAAkUgpKUpoKCpoKCgoKUgriCgqaCgIKkgoKClA==", "64-69:1;111-113:1;228-230:1;256-258:1;315-317:1;345-348:1")]
[assembly: go.GoPositionMap("net/cgo_unix_syscall.go", "cgo_unix_syscall.cs", "ACZYgKSCgpSmgKKAooCigKKAooCkgqaCpoKmgoKmgoKmgqaCpoKSgqaCkoI=")]
[assembly: go.GoPositionMap("net/conf.go", "conf.cs", "AEWUAZiSggAICsKCgoKUgoKClIKUlIKUxoKUxoKSgpQAChSmgriCgriU7oKCgsqCgrSoogAHEMrKpr7SgqYABxCCuKwACQKCgqbsAAkCgoKmAA0GpKSCuIKSlJKUuqa4griUAAgSlJSmpsq4gqaCypSUlJSCgqaUpIKCppS2AAcQlIKmgrimlKamgpSSgoKClJSClIKClIKUpoK4gpSCgpS0yqyylIKUlKSklIKUxriCgoKCggAHEIKCgu6UgpTGpLimAAEcAA8CgoKClIKUpoCCgoKkggAICqLaotqi", "92-121:1;222-224:1;234-236:1;500-509:1")]
[assembly: go.GoPositionMap("net/dial.go", "dial.cs", "AClUAAoagpSkuIKUpqaCgpQADB6ClKTKgpSmpoKClABq7AGApIKClIKUAAISAAgCgpSAgqSmgoKUqqKClIKCpoSSgoKUpqaCgpS4soKC2ILYlJSClIKCgoKCppSssoKClIKUlIKClIKUlIKClLyUgrSCtIKkgoKClJSClLSClLSClLaClK7CAAIS4gACZgAwAoIAAhoACgKSAAki8gAFKgAWAoK2goCCkoKCtoKCptySgpSm1oKClIKSgoKAgoKCtoCCgrK06KSigpSCAAQaAAwCgpIABRYADAKCkgAFFgAMAoKSAAUWAAwCgpIADA7igqaChAABEMKCgpSC5oLatoKStoKkgrSCksaClIKUlIKUygAJEOKUgqTYgoCCgpSClJSCgoLYgoKUgriClNoACAKCgoKClIKQtoKUgoKk1oLEgsSC1JSClAApWsIAAhLiAAUU8oKClMqCgpSCpNbUlIKUAAIU8oKClMqCgpTExNSUgpQACDwAFQKCAAI4ABkCgg==", "576-582:1;585-592:2;671-684:1;781-781:1")]
[assembly: go.GoPositionMap("net/dnsclient.go", "dnsclient.cs", "ABAoxIKmgqzSgoKUgqaUgoLcgqaygpSCgoKClIKUgqYAAiAADgSCAAoWgoKmgoKCgoKUpILGxoKUgsaClIKUpJSCpgACFgAIAoKUAA0gooKClIKCgoKCgoKUpoK6koKAgqSUgoKCgqYACxqSgoKU", "200-205:1;231-233:1")]
[assembly: go.GoPositionMap("net/dnsclient_unix.go", "dnsclient_unix.cs", "AClWgoKCgoLKqJTigpKAgqSAgraCpoCCpIKAgqSAgsiCgpSCgoKCprKClIKUgpSmooCCtoKCgoKUuIKClIKClLiigIK2goCCpIKClIKClIKCgpSCgpSClKjSgoKClIKClJSCgqSCgpSAgqSCgoCClKSCgpSAggAJEoKUlNiylIKmgoLKgqbcgpSmpqKCgoKUgpSClICCAAUQsoKCgoKCgpSCgpSAgt7CgoKUgoKU3IKClJKCpoCCpIKmgIKmlIK2gIKmlIK2pgASIoKCqNaCuKwACAKUgriClJSCgpSUAAgQgoCCpIK4gqKkgqSkuIKmotymgqaqgoKClICmtoKUgLik3sKClIKopIKCgriCgpSmgoKmlIKmgoKCuIKUAA4iAAcQgoCCpKaylIKCpoKmgoKUgrKUqMKCgoKAgoK2gqrSggAIBtKCgpSCgoKClKaCuJSUjIKmgoKClJS0pIKCgpKigoKC1pKCkpKCpqiCgoKUgoKCgoCUgpKUpAARJIKCgoLcgpSUgoLKlIKCuIKCypSCgriCgsqUgriAgsqk2LiClIKmgLikgoKCgoKCgoKClKaCptiSgqiSgoKCpoK4goKUkoKAgoKCgsiUgoKCgpSC3IKCgtyUgoLcqA==", "656-656:1;657-662:2;664-671:3;666-670:3.1;672-674:4", "", "832=Error/1/1/1;845=Error/1/1/1;860=Error/1/1/1;875=Error/1/1/1;889=Error/1/1/1;991=Error/1/1/1;1000=Error/1/1/1;1010=Error/1/1/1")]
[assembly: go.GoPositionMap("net/dnsconfig.go", "dnsconfig.cs", "ABk0lAATMOKClA==")]
[assembly: go.GoPositionMap("net/dnsconfig_unix.go", "dnsconfig_unix.cs", "ABcmssqCgoKCgpSSgIKUgoKCpIKUlIKClJS4gILagriCgoKClLiClIKCkoKUtIKClLSCgpS0AAgOAAkQtAAICrQACBCmpoKUgpTWgoKUlICCpKaCgpQ=")]
[assembly: go.GoPositionMap("net/error_posix.go", "error_posix.cs", "AAsgooCCpA==")]
[assembly: go.GoPositionMap("net/error_unix.go", "error_unix.cs", "AAoWgoCCpA==")]
[assembly: go.GoPositionMap("net/fd_posix.go", "fd_posix.cs", "ABo6gpKClIKUpqKClKaUgtaCgoKmgqaCprKCgqbCgoKksoKCprKCgqbigoKm0oKCptKCgqaygoKmsoKCprKCgqaygoKmwoKCpsKCgqbCgoKmgqaCpoI=")]
[assembly: go.GoPositionMap("net/fd_unix.go", "fd_unix.cs", "ABc0ggAJFKai5gAJCIDWpMaAgqSC/oKUpKSAgraCgoCCgtqCyIKUku7uAAgSgIKkxqSCgpSAxtqAgsak+MKCgoKUpoCCgqSAgoKkgoKoxLKCgoKUpg==", "88-94:1;95-107:2")]
[assembly: go.GoPositionMap("net/file.go", "file.cs", "AA4coKKArAAIAoKClK4ACAKCgpSuAAgCgoKU")]
[assembly: go.GoPositionMap("net/file_posix.go", "file_posix.cs", "ABEegoKClIKCgoKUgoKClIKUtLS0gpSCgoKUgoKCgIKCpIKmgoKClJSkpKSUgqaCgoKUlKSUgqaCgoKUlKSklII=")]
[assembly: go.GoPositionMap("net/file_unix.go", "file_unix.cs", "ABIigoKCgpSUgIKCpA==")]
[assembly: go.GoPositionMap("net/hook.go", "hook.cs", "AAweAAAQkuw=")]
[assembly: go.GoPositionMap("net/hook_unix.go", "hook_unix.cs", "AAoY2oKCgg==")]
[assembly: go.GoPositionMap("net/hosts.go", "hosts.cs", "ABMkgoKClAAbOqKClIKUgoKCpoKUgoKCuIKSgoCUpIKClIKCpoKCgoKClIKmlICCuLYABxCCgoKCgqKmsoKCgoKCgoKUgIKCgrbYsoKCgoKClIKAgoKCtg==")]
[assembly: go.GoPositionMap("net/interface.go", "interface.cs", "ABIsgoKCggAVMgAIEoKCgoKClKaClKqigpSCgpSqooKUgoKUqJKCgpSClAACENKCgpQAAhDSgpSCgpSCgpSmgrKCpqiSgpSCgpSClLKCpgAPIAAFEAAIAoKCgoKUgoKCgIK2goKCgoKAgsjmgoKUgoKCgoKCgoKUgpSmgoKUgoKCgoKCgoKUgpQ=")]
[assembly: go.GoPositionMap("net/interface_bsd.go", "interface_bsd.cs", "AA0isoKClIKClIKCgpSClIKCgoCCgqSCgoLIpoKCgpSClIKUgpSClIKUrNKCgpSCgpSCgpSClIKUgoKSgoKCtoKUgoKSgoKCtoLI")]
[assembly: go.GoPositionMap("net/interface_darwin.go", "interface_darwin.cs", "AAwYgqqigoKUgoKUgpSClIKCkoKCgraCyA==")]
[assembly: go.GoPositionMap("net/ip.go", "ip.cs", "ADBqooKCgoKCgqaYooKCgoKCrLKClIKUgoKCgoKCgpSClLqCgoKqgoKCgoKaoqiSgIKkqqKA7syokoCCpKqiqqKAgqSqooCCpAACFgAIAgACEpKCgqaqooKUiJSqooKUgpS6goKcsoCCpJSkpLiCgoKmqJKClIKUgoKUgoKUAAUS4oKmgqaClIKkgpSCpoKCgpSqooKUqrSAgqSCrLKClIKmrMSCgpSqooKClJKCgpSCrLKClIKUgpSmgqqigrKCgriCgqaClIKCppSs4oKClKiSgpSmsoCCgoK2gpSCtoK2lKiSgoCCpIKClIKCptiQAAIQ4oKUgoKUgoKUAAIS4oCCpKaCgoKUAAIWAAgCgoKmgoKmgoKUgoKmgoKC", "", "", "195=Equal/1/1/1,IsUnspecified/1/1/2,IsLoopback/1/1/3,IsMulticast/1/1/4,IsLinkLocalUnicast/1/1/5;214=isZeros/1/1/1")]
[assembly: go.GoPositionMap("net/iprawsock.go", "iprawsock.cs", "AAxEABICAAsakKSigpSCgpSmooKUpoKClAACIAANAoKUgoKUtpSCgpQACBbCgpSosoKUgoKUqLKClIKClIKUAAIUAA4CgpSCgpSosoKUgoKUqLKClIKClIKClAACEgALAoKUgoKUpoAAAhLypqKClIKClIKClAACFAAJAoKUgoKClA==", "", "", "32=AsSlice/1/1/1,Zone/1/1/2")]
[assembly: go.GoPositionMap("net/iprawsock_posix.go", "iprawsock_posix.cs", "AAscgpSklKaigpSClKaigpSmgqaGooKUgrSkpoKClIKClIKUgqbigoKUtKSmgoKUgpSCgpSmooKUgpSCgpSmooKClLaUgoKYgoKUpqKCgpS2lIKCmIKClA==", "127-129:1;150-152:1")]
[assembly: go.GoPositionMap("net/ipsock.go", "ipsock.cs", "AB5CooKqooKs1pSmgt6SlKSklKiQqrKCpraUgpSqooKCpgACEgAJAoKCgoKClKauwoKCgqaClKiSqJIADRgADAKYgpS2goKmlIKClKb8gpSUgpSCgqaClIKmgqbWgIKUpAACEPaClK7CupSCgIKkgILYgraUkpSkpKSmgriCgtyCpoKClIKUAAIcAAsCgpQ=", "170-172:1;272-283:1")]
[assembly: go.GoPositionMap("net/ipsock_posix.go", "ipsock_posix.cs", "ABY4AA4CpoKCgqaCtoKUAAUSypSCgoKUgoKCgpSAgqSClMYAAmoAMwKUpKaCgpSClKaElKaClIKmgqaCgpSCgpSCgqYAChaCuIKClIKCAAIcAAsClJKClKSSgpSUpu6CgpS4pu6Cyg==", "", "", "55=ParseIP/1/1/5,IPv4/1/1/7;165=family/2/2/1;263=As4/1/1/1,Port/1/1/2;278=As16/1/1/1,Port/1/1/2,Zone/1/1/3,index/1/1/3")]
[assembly: go.GoPositionMap("net/lookup.go", "lookup.cs", "ABw0AA0cABo4goKCgoKClAAMFIKUgIKkpKSUpoKAgoKCgoCCpKSqooKUgoKUygAlVsCioKSCgpQAAhDSqrSClICCpKqigoKUgoKUqqKuwoKClLamgpSCgqaCgpSuAAgKgoKUgoKAgrYAFRSWkqSkAAMQwqq0gpSAgqSCgsqCgIIACBCUgoKitoKCgpQAChKCgrSkgoKUpIKCgoKAgraCgpTMooKUgoKCgpSokoKClK7CrOKCgraklIKCpoKUAAImABACAAIgAA0CgoKUgpQAAiIADgIAAiIADgKCgpSClIKCgpSClJSClAACGAAJAgACEuKCgpSCgoKUpoKCppSClAACGAAJAgACEuKCgpSCgoKUgpSUgpQAAhTyrsIAAh4ADAIAAhLigoKUgoKCpoKUAAcW3NKCgpSClIKUAAIYAA0CgpSUkoKUgoKCgpSC3IKAguyUgpSCgtyUgqiSkoKUgoKCgpSC3IKAguyUgoLcloKokpKClIKCgoKUgtyCgILslIKC3JSokpKClIKCgoKUgtyCgILslIKCAAgSgoKUgoKUgpSU5oKClIKClA==", "334-336:1;338-342:2;354-354:3")]
[assembly: go.GoPositionMap("net/lookup_unix.go", "lookup_unix.cs", "ABEiooKClKSUgIKkgoKUgIKAgqSCgIIACAqoooKmgoKClKaigoKUgqamgoKmgIK2lKaCgoKAgramgqaCpoKmgqaCgoKU")]
[assembly: go.GoPositionMap("net/mac.go", "mac.cs", "AAwYooKUgoKClIKUAAIgAA8CgqaCgpSCgpSCgoKAgqSkgoKUgoKUgoKCgIKkgIKkpoKmgoKmgoKCgIKkppSC")]
[assembly: go.GoPositionMap("net/mptcpsock_stub.go", "mptcpsock_stub.cs", "AAoagqaCpoI=")]
[assembly: go.GoPositionMap("net/net.go", "net.cs", "AKkB9gKgqtKClIKClKiygpSCgpSosoKUgoKUrNKClKzSgpSosoKUgIKkqLKClICCpKiygpSAgqSqwoKUgIKkqsKClICCpAACGgANAoKClABCqgEADQKAkgAbQKamggAJDoCkgKiilKSkAB1EgKSigpSCgpSClIKClJSUgsy4gt6CgIKCpILupoKmgIKCpIIACxqApICigP6igpSCgpSmgKKAyICigKKAyICigKKAAA0a+ICigKKApIIACBKAooCigKKAuIL+gAAIDICigKKAABAmoryAgoLagqaSAAoYkKSigpSCgpSCrLCqsMjuAAYQogAKGrQABxKiAAoYpAAMGoKClLSkyIIAESaCAAESAAkCgIKkgoKCgoKmggACEgAJAoKCgoKUgpSmgoKCgoKUgoI=", "405-405:1;810-812:1", "", "791=Error/1/1/2")]
[assembly: go.GoPositionMap("net/nss.go", "nss.cs", "AB9CgoKCgoLYkoKCqOKmgpSUgoKUlIKAgqSCpoKCgqKkgqaCpKS4ggAOIqKCgqYADhyygpSClKS2lIKUpqKCgpSSgoKmgoIACQaigoKCgpSCgoKUgoKCgoKUgoKCgpSClJSCgoKClIKCgoKUlIKU3OjCgoKCgpSClIKClIKCgpTKlA==", "223-247:1")]
[assembly: go.GoPositionMap("net/parse.go", "parse.cs", "ABcugKSygoKCgoKUgoKCgqaUgoKUprKAgqSCgoKClIKmgqaigoKUpoKCgpSmooKClKiSgoKCpqiSgoKCgoKCgpSmgoKUpoDe0oKCgoKmgpSq0oKCgoKSgoKSgoKUlIKmgpSuwoKUgqiSgoKmqJKCgsySgpSokoKUgpSokqqigIKkqqKCgoKClICCgILGlKqiqqKClIKCpg==")]
[assembly: go.GoPositionMap("net/pipe.go", "pipe.cs", "ABMqggACEgALAoKUgpSmgoKClLiAgoKUgpTIgrSm4oKC1oKkpAAJDICigAATONKCgoKCgpTu7qaAooCkgoKClKailKSkptSCgqSkpMiCgoKUpuKUpKSmgoKC1IKCtKSkpuaigpSCgqaigpSCpqKClIKmgoCS", "54-56:1;236-236:1", "", "145=makePipeDeadline/1/2/4,makePipeDeadline/2/2/5;152=makePipeDeadline/1/2/4,makePipeDeadline/2/2/5")]
[assembly: go.GoPositionMap("net/port.go", "port.cs", "AAceAAoCppSYgoKSgoKUgqKClJSCgpSCgoKClJSCkoKUlIKU")]
[assembly: go.GoPositionMap("net/port_unix.go", "port_unix.cs", "ABMkooKClKSUgIKkgoKUgoKClIKCgoKUgoLYppKC")]
[assembly: go.GoPositionMap("net/rawconn.go", "rawconn.cs", "ABc0oKSigpSCgoKUpqKClIKCgpSmooKUgoKClAACFAAJAoKUpoIAAhTy7oKmgqaC")]
[assembly: go.GoPositionMap("net/rlimit_unix.go", "rlimit_unix.cs", "AAoqAAoCgoCCpIKCkoKU")]
[assembly: go.GoPositionMap("net/sendfile.go", "sendfile.cs", "ABIwAAsCgpSCgoKCgsqCgqaCgqaCgoKUgqaCpg==", "49-52:1")]
[assembly: go.GoPositionMap("net/sendfile_nonwindows.go", "sendfile_nonwindows.cs", "AAgUkg==")]
[assembly: go.GoPositionMap("net/sock_bsd.go", "sock_bsd.cs", "ABAcgpiUpNiUgsqClA==")]
[assembly: go.GoPositionMap("net/sock_posix.go", "sock_posix.cs", "AA4k0oKClICCgqSAgoIAGTKClICCgqSkgIKCpKaAgoKkpoKUlJSUgpTWooKCgoKCkoKUgILIgoKCgIKSgoCC2IKCgoCCpICCpJSAggAJEoKCkoCClKSmooKAgqSCgIK2goKAgsiAgqSAgqSAgqSCgqaiAAgSgoCCpJKUpJS2goKAgraCgoCCtoCCpICCpIKC")]
[assembly: go.GoPositionMap("net/sockaddr_posix.go", "sockaddr_posix.cs", "AB5IgpSUpKS2lKSkpg==", "57-57:1")]
[assembly: go.GoPositionMap("net/sockopt_bsd.go", "sockopt_bsd.cs", "AA8egsqUpKa4lJSUppSmpoCC7A==")]
[assembly: go.GoPositionMap("net/sockopt_posix.go", "sockopt_posix.cs", "AAwekoKUpoKClIKClIKUgsaCyKaCgoKmgoKCpoKCgqaCgoKClIKUgoI=")]
[assembly: go.GoPositionMap("net/sockoptip4_bsdvar.go", "sockoptip4_bsdvar.cs", "AAwcgoKClIKCgoKmgoKC")]
[assembly: go.GoPositionMap("net/sockoptip4_posix_nonlinux.go", "sockoptip4_posix_nonlinux.cs", "AA0egoKAgqSCgqaCgpSCgpSClICCgtaAgoLYgoKU")]
[assembly: go.GoPositionMap("net/sockoptip6_posix.go", "sockoptip6_posix.cs", "AAwcooKClIKCpoKCgqaigoKClIKC")]
[assembly: go.GoPositionMap("net/splice_stub.go", "splice_stub.cs", "AAoWgqaC")]
[assembly: go.GoPositionMap("net/sys_cloexec.go", "sys_cloexec.cs", "ABIotIKCgpSCgpSAgoKk")]
[assembly: go.GoPositionMap("net/tcpsock.go", "tcpsock.cs", "ABtA8oKUgoKokKSigpSCgpSmooKUpoKClAACJAAPAraklIKClKyyACxkwoKUqLKClIKClKiygpSCgpSqwoKUgIKkqsKClICCpAACIgAQAoKUgIKkqsKClICCpAACEPKClICCpK7igpSAgqQAAhgACwKClKaCgoLKgoKClIKCpgACFPKmoraUgpSCmIKUgpSUgpQACR7ygpSqwoKUgoKUqsKClIKClKrCgpSAgqSssKjCgpQAAhoADgKClIKClAACGAALAraUgpSCmIKUlIKUqJI=", "", "", "114=Addr/1/2/1,AsSlice/1/1/1,Addr/2/2/2,Zone/1/1/2,Port/1/1/3")]
[assembly: go.GoPositionMap("net/tcpsock_posix.go", "tcpsock_posix.cs", "AA4ggpSklKaigpSClKaigpSmgqaigIKkgIKkpqKAgqSmooCCpICCpKaCprKCgpgAGTSCgpSmgpSmlIIAChaClIKCpoKAgqSAgqSmoKSCgoKUpoKmgoKClKaCpqKCgpiCgpQ=", "81-83:1;185-187:1")]
[assembly: go.GoPositionMap("net/tcpsock_unix.go", "tcpsock_unix.cs", "AAsYsoKmgIKkgIKkgIKkgIK2")]
[assembly: go.GoPositionMap("net/tcpsockopt_darwin.go", "tcpsockopt_darwin.cs", "ABAmgoKSgriCgoKmgoKSgriCgoKmgoKSgqaCgg==")]
[assembly: go.GoPositionMap("net/tcpsockopt_posix.go", "tcpsockopt_posix.cs", "AAwcgoKC")]
[assembly: go.GoPositionMap("net/udpsock.go", "udpsock.cs", "ABpA8oKUgoKokKSigpSCgpSmooKUpoKClAACJAAPAraklIKClKyyAAsYgAAIFMKClKjaqMKClIKClKiSgpSUAAIQAAoCgpSCgpQAAhQADQKCgoKUqAAIAoKUgoKUqLKClIKClKiygpSCgpSosoKUgoKUgoKUAAIWAA0CgpSCgpSo4oKUgoKUpoAAAhLypqK2lIKUgoKUgoKUAAIYAAsCtpSClIKCgpQAAi4AFgK2lIKUgoKClA==", "", "", "113=Addr/1/2/1,AsSlice/1/1/1,Addr/2/2/2,Zone/1/1/2,Port/1/1/3")]
[assembly: go.GoPositionMap("net/udpsock_posix.go", "udpsock_posix.cs", "AA0egpSklKaigpSClKaigpSmgqaigoKUgoKCgraCgoKCppSUpsKCgpSCgoKCtoKCgoKmgpSm4pSCgoKkgoKClKaigpSCppSSgpSkkoKUpLiCgpSCppSSgpSkkoKUpLiigpSClIKClKaCgpSCppSCgpKClJSkgoKSgpSUpLiigoKYgoKUpqKCgpiCgpSmsoKCmIKClIKAgoCCgraAgoLGpoKCgIK2gIKkgIKkpoKCgIK2gIKkgIKk", "216-218:1;230-232:1;244-246:1")]
[assembly: go.GoPositionMap("net/unixsock.go", "unixsock.cs", "ABg6oqaigpSmoqaCgpQAAhLilKQACRjCgpSqwoKUgIKkqsKClICCpKiygpSCgpSosoKUgoKUgpQAAhQADgKClIKClKiygpSCgpSosoKUgoKUgoKUAAISAAsCgpSCgpSmgAACEOKmoraUgoKUgoKUAAwaoK7ygpSqwoKUgoKUqsKClIKClKrCgpSAgqSssKjCgpQAAhoADgKClIKClKyytpSClIKCgpSssraUgpSCgoKU")]
[assembly: go.GoPositionMap("net/unixsock_posix.go", "unixsock_posix.cs", "AA0ggoKUpKSkppSClIKUgtimgoKUpoKAgqSmgoCCpKaCgIKk9oKUpKSkuIKmooKUpoKmgoKClIK2puKCgoKmlIK2pqKClIKUgpSCpqKClIKCgpSUpqKCgpiCgpSmgoKClKYADRiCgqamgoKClAACFgAIAqaygoKYgoKUpqKCgpiCgpQ=", "160-162:1;191-195:1;222-224:1;236-238:1")]
[assembly: go.GoPositionMap("net/unixsock_readmsg_cloexec.go", "unixsock_readmsg_cloexec.cs", "AAwagoKCprKCgoKUgg==")]
[assembly: go.GoPositionMap("net/writev_unix.go", "writev_unix.cs", "AAwcooKUgoKU1rKCgg==")]
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
