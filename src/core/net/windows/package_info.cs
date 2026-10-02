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
global using dnsmessageꓸMXResource = go.vendor.golang.org.x.net.dns.dnsmessage_package.ΔMXResource;
global using dnsmessageꓸNSResource = go.vendor.golang.org.x.net.dns.dnsmessage_package.ΔNSResource;
global using dnsmessageꓸOPTResource = go.vendor.golang.org.x.net.dns.dnsmessage_package.ΔOPTResource;
global using dnsmessageꓸPTRResource = go.vendor.golang.org.x.net.dns.dnsmessage_package.ΔPTRResource;
global using dnsmessageꓸQuestion = go.vendor.golang.org.x.net.dns.dnsmessage_package.ΔQuestion;
global using dnsmessageꓸSOAResource = go.vendor.golang.org.x.net.dns.dnsmessage_package.ΔSOAResource;
global using dnsmessageꓸSRVResource = go.vendor.golang.org.x.net.dns.dnsmessage_package.ΔSRVResource;
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
global using syscallꓸHandle = go.syscall_package.ΔHandle;
global using syscallꓸSignal = go.syscall_package.ΔSignal;
global using syscallꓸSockaddr = go.syscall_package.ΔSockaddr;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
using syscall = go.syscall_package;
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
[assembly: GoImplicitConv<Interface, ж<Interface>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("net/addrselect.go", "addrselect.cs", "AAwcgoKUpoKClIKCgu6Cgu6ygpKCgoKCgoCCpKYACBKCgpSCAA0k8oKCgoKCgoKEgsyClIKUgt6ClIIAECyElITMgpSCAAkagpSCAAoYgoKEgpSC3gAYGgAvfsSClIKCpgALHoKClIKCgriClAACGgAMAoCCpIKCpoKClIKCgoKClIKCgoKCgoKCuA==")]
[assembly: go.GoPositionMap("net/conf.go", "conf.cs", "AESUAZiSggAICsKCgoKEgoKClIKUlIKUxoKUxoKklAAJFJaCqIKCqJTugoKCuoKCtKiiAAEQqqqmzgAQAoKWAAcQgqisAAkCgoKm7AAJAoKCpgANBqSUgriCtra6pqiCqJQABxKElJamuriCloLKlJSUlIKCppSkgoKmlKYABxCEgqaCuJaUppaChJKCgoKUlIKUgoKUgpSWgpiilIKClLTKnLKUgpSUpKSkgpS2yIKCgoKCAAYQgoKC3pSClMakuKYAARwADwKCgoKUgpSmgIKCgqSCAAgKotqi2qI=", "92-121:1;222-224:1;234-236:1;500-509:1")]
[assembly: go.GoPositionMap("net/dial.go", "dial.cs", "ABxSAAcagpSkuIKElqaCgpQACR6ClKTKgoSWpoKClABq7AGApIKClIKUAAISAAgCkpSAgqSmgoKUqqKClIKCpoSSgoKUpqaCgpS4soKS2ILYpJSClIKCgoKCpqSssoKClIKUlIKClIKUpIKClLyUgrSCtIKkgoKClJSClLSClLSClLaClK7CAAIS4gACZgAwAoIAAhoACgKSAAki8gAFKgAWAoKUgoKCgIKCkraAgoKSsrTouIKAgpKCgqaCgpbMkoKUlgAMDuKCloKEAAAQwoKClILmgsqmgpKmgpSCtIKSxoKUgpSUgpTKAAkQ4oSCpMiCgIKClIKUlIKCgsiCgpSCqIKU2gAIAoKCgoKUgpC2gpSCgqTWgsSCxILUlIKUAClawgACEuIABRTygoKUyoKClIKk1tSUgpQAAhTygoKUyoKClMTE1JSClAAIPAAVAoIAAjgAGQKC", "541-547:1;601-614:1;711-711:1")]
[assembly: go.GoPositionMap("net/dnsclient.go", "dnsclient.cs", "ABAoxIKmgqyygoKUgqaUgoLcgqaygpSCgoKClIKUgqYAAiAADgSCAAkWgoKWgoKCgoKUpIK2tqKUgraSlIKUtJSClgACFgAIAoKUAAwgooKClIKCgoKCgoKUpoK6koKAgqSUgoKCgqYAChqSgoKU", "200-205:1;231-233:1")]
[assembly: go.GoPositionMap("net/dnsclient_unix.go", "dnsclient_unix.cs", "ACVWgoKCgoKKqJTigpKAgqSAgqaCpoCCpIKAgqSAgriCgpSCgoKCprKClIKUgpSmooCCpoKCgoKUuIKClIKClLiigIKmgoCCpIKClIKClIKCgpSCgpSClKjSgoKClIKClJSCgpSCgpSAgqSCgoCClKSCgpSAggAJEoKUlNiyhIKWgoK6gpbcgpSWpqKCgoKUgpSClICCAAUQsoKCgoKCgpSCgpSAgt7CgoKEgoKUzIKChJKCpoCCpIKWgIKmlIKmgIKmlIKmpgASIoKCqNaCqKwACAKEgqiClISCgpSEAAMQ0oCCpIK4gqKkgqSkuIKmotyWgpaqgoKClICmtoKUgLik3sKClIKopIKCgqiCgpSWgoKWlIKmgoKCuIKUAAoiAAcQgoCCpKaylIKCloKmgoKUgrKUqMKCgoKAgoK2gqrSggAIBsKCgoSCgoKClJaCqJSUjpaCgoKUlLS0goKCkqKCgoLWkoKSkoKmqIKCgpSCgoKCgJSCtqQADySCgoKC3IKUlIKCypSCgriCgsqUgoK4goLKlIK4gILKpOi4gpSCpoC4pIKCgoKCgoKCgpSmgqbYkoKokoKCgpaCqIKClJKCgoKCgoK4lIKCgoKUgtyCgoLclIKC3Jg=", "656-656:1;657-662:2;664-671:3;666-670:3.1;672-674:4")]
[assembly: go.GoPositionMap("net/dnsconfig.go", "dnsconfig.cs", "ABo2lAATMOKClA==")]
[assembly: go.GoPositionMap("net/dnsconfig_windows.go", "dnsconfig_windows.cs", "AAwassqCgqaCgpaUgqiCloKCgpSClLSCgtzYtKY=", "19-23:1")]
[assembly: go.GoPositionMap("net/error_posix.go", "error_posix.cs", "AAsgooCCpA==")]
[assembly: go.GoPositionMap("net/error_windows.go", "error_windows.cs", "AAkSgoCCpA==")]
[assembly: go.GoPositionMap("net/fd_posix.go", "fd_posix.cs", "ABo6ooKCpoKC1oKCgqaCpoKmsoKCpsKCgqSygoKmsoKCpuKCgqbSgoKm0oKCprKCgqaygoKmsoKCprKCgqbCgoKmwoKCpsKCgqaCpoKmgg==")]
[assembly: go.GoPositionMap("net/fd_windows.go", "fd_windows.cs", "ABk0gqqilLamggAJFKaigoKUAAgIAAgIgIKmyoSClICCpoK2gpSipsyCgqaClLS0tICCuIKUtMSkpuiUlKaogIKkpICCpMjWooKUgoKU1oKCgqaiiIKClKiCgoKUgIKCqKKSlIKEgqqk", "87-92:1;93-99:2;182-184:1")]
[assembly: go.GoPositionMap("net/file.go", "file.cs", "AA4coKKArAAIAoKClK4ACAKCgpSuAAgCgoKU")]
[assembly: go.GoPositionMap("net/file_windows.go", "file_windows.cs", "AAoYlKaUppQ=")]
[assembly: go.GoPositionMap("net/hook.go", "hook.cs", "AAoeAAAQkpw=")]
[assembly: go.GoPositionMap("net/hook_windows.go", "hook_windows.cs", "AAsahoKC")]
[assembly: go.GoPositionMap("net/hosts.go", "hosts.cs", "ABMkgoKClAAcOqKChIKUgoKCloKEgoKCqIKSgoCUpIKClIKCloKCgoKChIKWhICCuKYABxCCgoKCgqKmsoKCgoKCgoKUgIKCgrbYsoKCgoKClIKAgoKCtg==")]
[assembly: go.GoPositionMap("net/interface.go", "interface.cs", "ABtAgoKCggAVMgAIEoKCgoKClKaClKqigpSCgpSqooKUgoKUqJKCgpSClAACENKCgpQAAhDSgpSCgpSCgpSmgrKCpqiSgpSCgpSClLKCpgAPIAAFEAAIAoKCgoKUgoKCgIK2goKCgoCCtuaCgpSCgoKCgoKCgpSSlKaCgpSCgoKCgoKCgpSSlA==")]
[assembly: go.GoPositionMap("net/interface_windows.go", "interface_windows.cs", "AA9gsoKClIKCgpKUgriCgtyUpKSkpIKUlIKClIKCuNzSgoKUgoKCkpSCgoKClJS0goK2goKClJS0goLaqsKCgpSCgoKSlIKCgoKUlLSCgto=")]
[assembly: go.GoPositionMap("net/ip.go", "ip.cs", "ACRqooKCgoKCgqaYooKCgoKCrLKClIKUgoKCgoKCgpSClLqSkpKqkpKSkpKaoqiSgIKkqqKA7syokoCCpKqiqqKAgqSqooCCpAACFgAIAgACEpKCgqaqooKUiJSqooKUgpS6kpKcsoCCpJSkpMiCgoKmqJKClIKUgoKUgoKUAAUS4oKWgpaClIKkgqSCpoKCgpSqooKUqrSAgqSCrLKClIKWrMSCgpSqooKClJKCgpSCrLKClIKUgpSmgqqigrKCgriCgqaClIKCppSs4oKClKiSgpSmsoCCgoK2gpSCtoK2pKiSgoCCpIKClIKCptiQAAIQ4oKUgoKUgoKUAAIS4oCCpKaCgoKUAAIWAAgCgoKWgoKWgoKUgoKmgoKC")]
[assembly: go.GoPositionMap("net/iprawsock.go", "iprawsock.cs", "ACBKkKSigpSCgpSmooKUpoKClAACIAANApKUgoKUtqSCgpQACBbCgpSosoKUgoKUqLKClIKClIKUAAIUAA4CgpSCgpSosoKUgoKUqLKClIKClIKClAACEgALAoKUgoKUpoAAAhLygpSCgoKUAAIUAAkCgpSCgoKU")]
[assembly: go.GoPositionMap("net/iprawsock_posix.go", "iprawsock_posix.cs", "AAscgpSklKaigpSClKaigpSmgqaGooKUgrSkpoKClIKClIKUgqbigoKUtKSmgoKUgpSCgpSmooKUgpSCgpSmooKClLakgoKYgoKUpqKCgpS2pIKCmIKClA==", "127-129:1;150-152:1")]
[assembly: go.GoPositionMap("net/ipsock.go", "ipsock.cs", "AB5CooKqooKs1pSmgs6SlKSklKiQqrKClra0gpSqooKCpgACEgAJAoKCgoKClKauwoKCgqaClKiSqJIADRgADAKYgpSmgoKWlIKClJbcspSkgpSCgqaClIKWgqbWgIKUpAACEPaClK7CupSCgIKkgILYgrakkpSkpKS2gqiCgtyCloKClIKUAAIcAAsCgpQ=", "170-172:1;272-283:1")]
[assembly: go.GoPositionMap("net/ipsock_posix.go", "ipsock_posix.cs", "ABY4AA4CloKCgraCtoKkAAUSmtSCgoKUgoKCgpSAgqSClMYAAmoAMwKUpKaCgpSClJaElKaClIK2gqaCgpSCgpSCgqYAChaCuIKClIKCAAIcAAsClJKClKSSgpSkpu6CgpS4pu6CgpTK")]
[assembly: go.GoPositionMap("net/lookup.go", "lookup.cs", "ABsyAA4cABo4goKCgoKClAAMFIKUgJKkpKSkpoKAgoKCgoCCpKSqooKUgoKUygAlVsCioKSCgpQAAhDSqrSClICCpKqigoKUgoKUqqKuwoKClLamgpSCgpaCgpSuAAgKgoKUgoKAgrYAFRSWkqSkAAMQwqq0gpSAgqSCgsqCgIIABxCEgoKipoKCgpQAAxLygrSkgoKUpIKCgoKAgraCgpTMooKUgoKCgpSokoKClK7CrOKCgraktIKCpoKUAAImABACAAIgAA0CgoKUgpQAAiIADgIAAiIADgKCgpSClIKCgpSClJSClAACGAAJAgACEuKCgpSCgoKUgpSUgpQAAhgACQIAAhLigoKUgoKClIKUlIKUAAIU8q7CAAIeAAwCAAIS4oKClIKCgqaClAAHFtzSgoKUgpSClAACGAANAoKUlJKClIKCgoKUgtyCgILslIKUgoLclIKokpKClIKCgoKUgtyCgILslIKC3JaCqJKSgpSCgoKClILcgoCC7JSCgtyUqJKSgpSCgoKClILcgoCC7JSCggAIEoKClIKClIKUlOaCgpSCgpQ=", "333-335:1;337-341:2;353-353:3")]
[assembly: go.GoPositionMap("net/mac.go", "mac.cs", "AAsYgoKUgoKClIKUAAIeAA4CgpaCgpSCgpSCgoKAgqS2gpSCgpSCgoKAgqSAgqSmlISC")]
[assembly: go.GoPositionMap("net/mptcpsock_stub.go", "mptcpsock_stub.cs", "AAoagqaCpoI=")]
[assembly: go.GoPositionMap("net/net.go", "net.cs", "AKkB8gKgqtKClIKClKiygpSCgpSosoKUgoKUrNKClKzSgpSosoKUgIKkqLKClICCpKiygpSAgqSqwoKUgIKkqsKClICCpAACFAAKAoKClABDqgEADQKAkgAaQJaWkgAJDoCkgKiilKSkAB5EgKSigpSCgpSClIKClJSUgqyIgt6CgIKCpILupoKWgIKCpIIACxqApICigP6igpSCgpSmgKKAyICigKKAyICigKKAAA0a+ICigKKApIIACBKAooCigKKAuJL+gAAIDICigKKAABAmoqyAgoLKgpaSAAoYkKSigpSCgpSCrLCqsMjuAAYQogAKGrQABxKiAAoYpAAMGoKClLSkyIIADCaCAAESAAkCgIKkgoKCgoKmggACEgAJAoKCgoKUgpSmgoKCgoKUgoI=", "400-400:1;805-807:1")]
[assembly: go.GoPositionMap("net/nss.go", "nss.cs", "AB9CgoKCgoLYkoKCqOKWgpSEgoKUhIKAgqSCloKCgqKkgqaCpKS4ggAOIqKCgqYADhyygpSClKSmtIKUpqKCgpSSgoKWgoIACQaigoKCgpSCgoKUgoKCgoKUgoKCgpSClJSCgoKClIKCgoKUlIKU3OjCgoKCgpSClIKClIKCgpTKlA==", "223-247:1")]
[assembly: go.GoPositionMap("net/parse.go", "parse.cs", "ABcugKSygoKCgoKUgoKCgqaUgoKUprKAgqSCgoKClIKmgqaigoKUpoKCgpSmooKClKiSgoKCpqiSgoKCgoKCgpSmgoKUpoDe0oKCgoKmgpSq0oKCgoKkgqSClJSCpoKUrsKClIKokoKCpqiSgoLMkoKUqJKClIKUqJKqooCCpKqigoKCgpSAgoCCxpSqoqqigpSCgqY=")]
[assembly: go.GoPositionMap("net/pipe.go", "pipe.cs", "ABMqggACEgALAoKEgpSWgoKClKiAgoKUgpS4grSm4oKC1oKkpAAJDICigAATONKCgoKCgoTu7qaAooCkgoKClKailKSkptSCgqSkpMiCgoKUpuKUpKSmgoKC1IKCtKSkpuaigpSCgqaigpSCpqKClIKmgoCS", "54-56:1;236-236:1")]
[assembly: go.GoPositionMap("net/port.go", "port.cs", "AAceAAoCppSYgoKkgpSCooKUlIKClIKCgoKUlIKklJSClA==")]
[assembly: go.GoPositionMap("net/rawconn.go", "rawconn.cs", "ABc0oKSigpSCgoKUpqKClIKCgpSmooKUgoKClAACFAAJAoKUpoIAAhTy7oKmgqaC")]
[assembly: go.GoPositionMap("net/sendfile_windows.go", "sendfile_windows.cs", "ABIuAAsChIKCgoKogoKWgoK6hA==")]
[assembly: go.GoPositionMap("net/sock_posix.go", "sock_posix.cs", "AA4k0oKClICCgqSAgoIAFzKClICCgqSkgIKCpLaAgoKkpoKUpJSkgpSmooKCgoKCpJSAgriCgoKAgqSAgtiCgoKAgqSAgqSUgIIACRKCgqKClKSmooKAgqSCgIKmgoKAgriAgqSAgqSAgqSCgqaiAAES8oCCpJKUpKS2goKAgqaCgoCCtoCCpICCpIKC")]
[assembly: go.GoPositionMap("net/sock_windows.go", "sock_windows.cs", "AAwauNaClIKU")]
[assembly: go.GoPositionMap("net/sockaddr_posix.go", "sockaddr_posix.cs", "AB5IgpSUpKTGlKSkxg==", "57-57:1")]
[assembly: go.GoPositionMap("net/sockopt_posix.go", "sockopt_posix.cs", "AA0gkoKUpoKClIKClIKUgsaCyKaCgpSCgpSClICCgtaAgoLYgoKUpoKCgqaCgoKmgoKCpoKCgoKUgpSCgg==")]
[assembly: go.GoPositionMap("net/sockopt_windows.go", "sockopt_windows.cs", "AAoYgriUlJSm7qam")]
[assembly: go.GoPositionMap("net/sockoptip_posix.go", "sockoptip_posix.cs", "AAwcgoKAgqSCgqaigoKUgoKmgoKCpqKCgoKUgoI=")]
[assembly: go.GoPositionMap("net/sockoptip_windows.go", "sockoptip_windows.cs", "AAwagoKClIKCgoKmgoKC")]
[assembly: go.GoPositionMap("net/splice_stub.go", "splice_stub.cs", "AAoWgqaC")]
[assembly: go.GoPositionMap("net/tcpsock.go", "tcpsock.cs", "ABtA8oKUgoKokKSigpSCgpSmooKUpoKClAACJAAPAraktIKClKyyACxkwoKUqLKClIKClKiygpSCgpSqwoKUgIKkqsKClICCpAACIgAQAoKUgIKkqsKClICCpAACEPKClICCpK7igpSAgqQAAhgACwKClKaCgoLKgoKClIKCpgACFPK2pIKUgpiClJSClAAJHvKClKrCgpSCgpSqwoKUgoKUqsKClICCpKywqMKClAACFAALAoKUgoKUAAIYAAsCtqSClIKYgpSUgpSokg==")]
[assembly: go.GoPositionMap("net/tcpsock_posix.go", "tcpsock_posix.cs", "AA4ggpSklKaigpSClKaigpSmgqaigIKkgIKkpqKAgqSmooCCpICCpKaCprKCgpgAGDSCgpSWgpSmlIIACRaClIKCpoKAgqSAgqSmoKSCgoKUpoKmgoKClKaCpqKCgpiCgpQ=", "81-83:1;185-187:1")]
[assembly: go.GoPositionMap("net/tcpsock_windows.go", "tcpsock_windows.cs", "AAwasoKWgIKkgoCCpICCxIKkgIKm")]
[assembly: go.GoPositionMap("net/tcpsockopt_posix.go", "tcpsockopt_posix.cs", "AAwcgoKC")]
[assembly: go.GoPositionMap("net/tcpsockopt_windows.go", "tcpsockopt_windows.cs", "ABYugoKWgqSmgoKCpoKCloKkpoKCgqaCgqSWgoLYAAgQmK62AA4KgpSCuoKC2pKCgoI=")]
[assembly: go.GoPositionMap("net/udpsock.go", "udpsock.cs", "ABxG8oKUgoKokKSigpSCgpSmooKUpoKClAACJAAPAraktIKClKyyAAsYgAAIFMKClKjaqMKClIKClKiSgpSUAAIQAAoCgpSCgpQAAhQADQKCgoKUqAAIAoKUgoKUqLKClIKClKiygpSCgpSosoKUgoKUgoKUAAIWAA0CgpSCgpSo4oKUgoKUpoAAAhLytqSClIKCgpQAAhgACwK2pIKUgoKClAACLgAWArakgpSCgoKU")]
[assembly: go.GoPositionMap("net/udpsock_posix.go", "udpsock_posix.cs", "AA0egpSklKaigpSClKaigpSmgqaigoKUgoKCgraCgoKCtpSUpsKCgpSCgoKCtoKCgoK2gpSm4pSCgoKkgoKCpKaigpSClpSSgpSkkoKUpMiCgpSClpSSgpSkkoKUpMiigpSClIKClKaCgpSClpSSgpSkkoKUpMiigoKYgoKUpqKCgpiCgpSmsoKCmIKClIKAgoCCgraAgoLGpoKCgIK2gIKkgIKkpoKCgIK2gIKkgIKk", "208-210:1;222-224:1;236-238:1")]
[assembly: go.GoPositionMap("net/unixsock.go", "unixsock.cs", "ABg6oqaigpSmoqaCgpQAAhLilKQAChjCgpSqwoKUgIKkqsKClICCpKiygpSCgpSosoKUgoKUgpQAAhQADgKClIKClKiygpSCgpSosoKUgoKUgoKUAAISAAsCgpSCgpSmgAACEOK2pIKCgpQADBqgrvKClKrCgpSCgpSqwoKUgoKUqsKClICCpKywqMKClAACFAALAoKUgoKUrLK2pIKUgoKClKyytqSClIKCgpQ=")]
[assembly: go.GoPositionMap("net/unixsock_posix.go", "unixsock_posix.cs", "AA0ggoKUpKSkppSClIKUgtimgoKUpoKAgqSmgoCCpKaCgIKk9oKUpKSkyIKmooKUpoKmgoKClIK2puKCgoKWlIK2pqKClIKUgpSCpqKClIKCgpSUpqKCgpiCgpSmgoKClKYADRiCgqamgoKClAACFgAIAqaygoKYgoKUpqKCgpiCgpQ=", "160-162:1;191-195:1;222-224:1;236-238:1")]
[assembly: go.GoPositionMap("net/unixsock_readmsg_other.go", "unixsock_readmsg_other.cs", "AAoW")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("net")]
public static partial class net_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
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
