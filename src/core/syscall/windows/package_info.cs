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
[assembly: GoDynamicTypeLift("7374727563747b6f6e63652073796e632e4f6e63653b20616464722075696e747074723b20657272206572726f727d", "connectExFuncᴛ1")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<DLLError, error>(Pointer = true)]
[assembly: GoImplement<Errno, error>]
[assembly: GoImplement<SockaddrInet4, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrInet6, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrUnix, Sockaddr>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(ProcessEntry32))]
[assembly: GoFieldAccess(typeof(ProcessInformation))]
[assembly: GoFieldAccess(typeof(SockaddrUnix))]
[assembly: GoFieldAccess(typeof(_STARTUPINFOEXW))]
[assembly: GoFieldAccess(typeof(mountPointReparseBuffer))]
[assembly: GoFieldAccess(typeof(procThreadAttributeListContainer))]
[assembly: GoFieldAccess(typeof(reparseDataBuffer))]
[assembly: GoFieldAccess(typeof(symbolicLinkReparseBuffer))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("syscall/env_windows.go", "env_windows.cs", "AAoagoKClIKCgoKClILKgoKClIKClIKClIKmgoKClIKClIKmgriCgoLcooKClJSCgpSCgqaCgpQ=")]
[assembly: GoPositionMap("syscall/syscall.go", "syscall.cs", "ACFK8oKClKyygpSCggACEMCasoKClAAIEJLIksiSyJLKkpaSmA==")]
[assembly: GoPositionMap("syscall/syscall_windows.go", "syscall_windows.cs", "ABg80oKClK7CggAHEIKCrLKCgoKClJS0AAkQtoKqooKUgoKCgpQAAhLQnMKCgpQACxicwvqUgoKmgpSClLaCgpSmhJKCgoKCgriUAAsUgpSorKqepoKmgsgAAhDiAAIS4gAC4AEAagKCgoKmooKUgoKUgoKUtLSkgpS4gqaUgoKClIKC3IKUpgALFKSCntKUgrS0pIKClIKCppSEkoKClIKCuISCuIKmgoKmpoKCgoKUlJSmgoKCgpSmooKCgpSUgpSClKaigpSCgpSClIKU6ryigoLGtIKUprKClLS0pIKmgsiiopaCgsqC3IKCgpSClLiigoKUpqKCgpSmooKClKaigoKUpqKCgpSCgpSmgpKCgoKU5oKIgoKmooKCgqaCgpSSgoKUgoKmsoKUgoKUpoKUgoKCgpSClAAJDLKClIKClKaClIKCgoKUgpTmgqaCgoKUgoKUgpSUpoKmggBt8gGigoKCuLiClIKCuIKmgpSmAAhigoKUpoKCAApKgqaCAAQuooKClAAKAoKClKamooKClAAKAoKClKamggAMOqIACAKCgpSmABI2lJSUlJSUlJQAChKUooKCqoCCgpSCABo6goKCgqaigqaiAAQSlAADEgAIAoKClKaigoKClKaigoKUgoKCgpSCgpSCgvqCgoKUpoKYgoKCgpSClJSmooKCggAKFoKUqICClIKCgpSCgoKC2JSCgoKCpsaCqNKCgpSUgpSUgoKCgqaCgpSCgoKCgoLYAA0SgoKCggAIDpSU6JKosoKCgoKUgoKCgqaokuiSgoKCgpSUgoKCpoKCgpSCyJKCqJKCgoKokgAENgAYAqaCgqaygqaU")]
[assembly: GoPositionMap("syscall/time_nofake.go", "time_nofake.cs", "AAoWlA==")]
[assembly: GoPositionMap("syscall/types_windows.go", "types_windows.cs", "AEaaAQDzAf4DkpIAEiKCxqKCggAePrSUlILGtJSUgoIAHk7ygoKCgoKCgqaCAOEEgAo=")]
[assembly: GoPositionMap("syscall/wtf8_windows.go", "wtf8_windows.cs", "ACZYoqaClIKCgoKmgpSqooKCtsiC6oKClIKklA==")]
[assembly: GoPositionMap("syscall/zerrors_windows.go", "zerrors_windows.cs", "AJMBrgI=")]
[assembly: GoPositionMap("syscall/zsyscall_windows.go", "zsyscall_windows.cs", "ABIyopSkyqiCgoKCgoKCgoKClIKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoIABSaiwoKUpqKCgpQAEAKClKai4oKUpqKigpSmooKClKaiooKmosKClKaiAA4CgpSmogAOAoKUpqKigpSmooKClKaiAAwCgpSmosKClKaiABYCgpSmogAKAoKUAAQWooKClKayooKClKayooKClAAIMLKCgoKUprKigoKUAAQWosKCpqKCgoKUAAYgosKClKaiooKUpqKCgpQABBaigoKUpqLCgpSmssKCgpSmssKCgpSmosKClKaygoKClKai4oKUpqKCgpQAEAKClKaiwoKUprKCgoKUpqKigpSmgqKmogAIAoKUpqKCgpSigpSmgoKmooKClAAGKKKCgpSmooKClKaygoKUwoKClKaiooKUpqKCgpQABBKiwoKUpqKigpSmsqKCgpSmsoKCgpSmooKCprKCgoKUprLCgoKUpqKigpSmosKClKayooKClKaiooKUprKCgoKUprKigoKUprLigoKUpqKCgpSmssKCgpSmsoKCgpSmsqKCgpSmogAIAoKUpqLigpSmssKCgpQABBCygoKClKaCoqayooKClAAEGLKCgoKUpqLCgpSmsoKCgpSmsqKCgpSmsoKCgpSmsoKCgpSmsoKCgpSmosKClKaygoKUgoKClKaiooKUAAYmouKClKaigoKU4oKUAAQeoqKClKaiooKUpqKCgpSmosKClKaiooKUpqKCgpSmoqKClKayooKClKai4oKUpqKCgpSmooKClKaigoKUpqIACAKClKaigoKUpqKCgpSmsoKCgpSmouKClAAKSKKigpQABiaiwoKUpqLigpSmssKCgpSmosKClAAGIKKCgpQABBiiAAgCgpQACDaiAAoCgpQABBaiooKUpqKCgpSmoqKClKaygoKClAAEGKLCgpSmsoKCgpQABBiygoKClIKCgpQABBiiwoKUpqLCgpSmooKClKaigoKmoqKClKaigoKUprKCgoKU")]
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
    [GoLocalName("_FILE_END_OF_FILE_INFO")] internal partial struct Ftruncate__FILE_END_OF_FILE_INFO {}
    internal partial struct _STARTUPINFOEXW {}
    internal partial struct connectExFuncᴛ1 {}
    [GoValueClone("PathBuffer")] internal partial struct mountPointReparseBuffer {}
    internal partial struct procThreadAttributeListContainer {}
    internal partial struct reparseDataBuffer {}
    [GoValueClone("PathBuffer")] internal partial struct symbolicLinkReparseBuffer {}
    internal partial struct sysLinger {}
    [GoValueClone("FileName", "AlternateFileName")] internal partial struct win32finddata1 {}
    public partial class Pointer {}
    public partial interface Conn {}
    public partial interface RawConn {}
    public partial interface Sockaddr {}
    public partial struct AddrinfoW {}
    public partial struct ByHandleFileInformation {}
    public partial struct CertChainContext {}
    public partial struct CertChainElement {}
    public partial struct CertChainPara {}
    public partial struct CertChainPolicyPara {}
    public partial struct CertChainPolicyStatus {}
    public partial struct CertContext {}
    public partial struct CertEnhKeyUsage {}
    public partial struct CertInfo {}
    public partial struct CertRevocationCrlInfo {}
    public partial struct CertRevocationInfo {}
    public partial struct CertSimpleChain {}
    public partial struct CertTrustListInfo {}
    public partial struct CertTrustStatus {}
    public partial struct CertUsageMatch {}
    public partial struct DNSMXData {}
    public partial struct DNSPTRData {}
    [GoValueClone("Data")] public partial struct DNSRecord {}
    public partial struct DNSSRVData {}
    [GoValueClone("StringArray")] public partial struct DNSTXTData {}
    public partial struct Errno {}
    public partial struct FileNotifyInformation {}
    public partial struct Filetime {}
    [GoValueClone("Data4")] public partial struct GUID {}
    public partial struct Handle {}
    public partial struct Hostent {}
    [GoValueClone("Multiaddr", "Interface")] public partial struct IPMreq {}
    [GoValueClone("Multiaddr")] public partial struct IPv6Mreq {}
    [GoValueClone("Address", "BroadcastAddress", "Netmask")] public partial struct InterfaceInfo {}
    [GoValueClone("AdapterName", "Description", "Address", "IpAddressList", "GatewayList", "DhcpServer", "PrimaryWinsServer", "SecondaryWinsServer")] public partial struct IpAdapterInfo {}
    [GoValueClone("IpAddress", "IpMask")] public partial struct IpAddrString {}
    [GoValueClone("String")] public partial struct IpAddressString {}
    [GoValueClone("Value")] public partial struct IpMaskString {}
    public partial struct Linger {}
    [GoValueClone("Name", "PhysAddr", "Descr")] public partial struct MibIfRow {}
    public partial struct Overlapped {}
    [GoValueClone("ExeFile")] public partial struct ProcessEntry32 {}
    public partial struct ProcessInformation {}
    public partial struct Protoent {}
    [GoValueClone("Data")] public partial struct RawSockaddr {}
    [GoValueClone("Addr", "Pad")] public partial struct RawSockaddrAny {}
    [GoValueClone("Addr", "Zero")] public partial struct RawSockaddrInet4 {}
    [GoValueClone("Addr")] public partial struct RawSockaddrInet6 {}
    [GoValueClone("Path")] public partial struct RawSockaddrUnix {}
    public partial struct Rusage {}
    public partial struct SSLExtraCertChainPolicyPara {}
    public partial struct SecurityAttributes {}
    public partial struct Servent {}
    public partial struct Signal {}
    public partial struct SockaddrGen {}
    [GoValueClone("Addr", "raw")] public partial struct SockaddrInet4 {}
    [GoValueClone("Addr", "raw")] public partial struct SockaddrInet6 {}
    [GoValueClone("raw")] public partial struct SockaddrUnix {}
    public partial struct StartupInfo {}
    public partial struct Systemtime {}
    public partial struct TCPKeepalive {}
    public partial struct Timespec {}
    public partial struct Timeval {}
    [GoValueClone("StandardName", "DaylightName")] public partial struct Timezoneinformation {}
    public partial struct TransmitFileBuffers {}
    public partial struct WSABuf {}
    [GoValueClone("Description", "SystemStatus")] public partial struct WSAData {}
    [GoValueClone("ChainEntries")] public partial struct WSAProtocolChain {}
    [GoValueClone("ProviderId", "ProtocolChain", "ProtocolName")] public partial struct WSAProtocolInfo {}
    public partial struct WaitStatus {}
    public partial struct Win32FileAttributeData {}
    [GoValueClone("FileName", "AlternateFileName")] public partial struct Win32finddata {}
    public partial struct _PROC_THREAD_ATTRIBUTE_LIST {}
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
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyscallꓸwindowsꓸsysdll() => builtin.initPackage(typeof(@internal.syscall.windows.sysdll_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(unicode.utf8_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(syscall_package.DLL)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛDLL;
[GoMethodHost(typeof(syscall_package.DLLError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛDLLError;
[GoMethodHost(typeof(syscall_package.Errno)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛErrno;
[GoMethodHost(typeof(syscall_package.Filetime)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛFiletime;
[GoMethodHost(typeof(syscall_package.LazyDLL)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛLazyDLL;
[GoMethodHost(typeof(syscall_package.LazyProc)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛLazyProc;
[GoMethodHost(typeof(syscall_package.Proc)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛProc;
[GoMethodHost(typeof(syscall_package.RawSockaddrAny)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛRawSockaddrAny;
[GoMethodHost(typeof(syscall_package.SID)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛSID;
[GoMethodHost(typeof(syscall_package.Signal)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛSignal;
[GoMethodHost(typeof(syscall_package.SockaddrInet4)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛSockaddrInet4;
[GoMethodHost(typeof(syscall_package.SockaddrInet6)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛSockaddrInet6;
[GoMethodHost(typeof(syscall_package.SockaddrUnix)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛSockaddrUnix;
[GoMethodHost(typeof(syscall_package.Timespec)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛTimespec;
[GoMethodHost(typeof(syscall_package.Timeval)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛTimeval;
[GoMethodHost(typeof(syscall_package.Token)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛToken;
[GoMethodHost(typeof(syscall_package.WaitStatus)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛWaitStatus;
[GoMethodHost(typeof(syscall_package.procThreadAttributeListContainer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syscallᴛprocThreadAttributeListContainer;
// </MethodHolders>
