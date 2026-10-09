// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.net_package;
global using static global::go.net_internal_test_package;

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
global using execꓸError = go.os.exec_package.ΔError;
global using flagꓸErrorHandling = go.flag_package.ΔErrorHandling;
global using jsonꓸToken = object;
global using jsonꓸΔToken = object;
global using netipꓸAddr = go.net.netip_package.ΔAddr;
global using netipꓸPrefix = go.net.netip_package.ΔPrefix;
global using netꓸAddr = go.net_package.ΔAddr;
global using netꓸError = go.net_package.ΔError;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using runtimeꓸError = go.runtime_package.ΔError;
global using socktestꓸSockets = go.net.@internal.socktest_package.ΔSockets;
global using syscallꓸHandle = go.syscall_package.ΔHandle;
global using syscallꓸSignal = go.syscall_package.ΔSignal;
global using syscallꓸSockaddr = go.syscall_package.ΔSockaddr;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static global::go.net_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("696e746572666163657b46696c65282920282a6f732e46696c652c206572726f72297d", "startTestSocketPeer_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b697341646472696e666f4572726e6f28297d", "parseDialError_type")]
[assembly: GoDynamicTypeLift("7374727563747b4164647220737472696e673b205265766572736520737472696e673b2045727250726566697820737472696e677d", "revAddrTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6469616c2066756e6328737472696e672c20737472696e672920286e65742e436f6e6e2c206572726f72293b20756e726561636861626c654e6574776f726b20737472696e673b206e6574776f726b73205b5d737472696e673b206164647273205b5d737472696e677d", "dialGoogleTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b66696c7465722066756e63286e65742e4950416464722920626f6f6c3b20697073205b5d6e65742e4950416464723b20696e6574616464722066756e63286e65742e49504164647229206e65742e416464723b206669727374206e65742e416464723b207072696d6172696573206e65742e616464724c6973743b2066616c6c6261636b73206e65742e616464724c6973743b20657272206572726f727d", "addrListTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e202a6e65742e49504e65743b206f757420737472696e677d", "ipNetStringTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e206e65742e49503b2061663420626f6f6c3b2061663620626f6f6c7d", "ipAddrFamilyTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e206e65742e49503b206d61736b206e65742e49504d61736b3b206f7574206e65742e49507d", "ipMaskTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e206e65742e49503b2073747220737472696e673b20627974205b5d627974653b206572726f727d", "ipStringTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e206e65742e49504d61736b3b206f757420737472696e677d", "ipMaskStringTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e206e65742e49504e65743b206f7574206e65742e49504e65747d", "networkNumberAndMaskTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206970206e65742e49503b206e6574202a6e65742e49504e65743b20657272206572726f727d", "parseCIDRTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f7574206e65742e4861726477617265416464723b2065727220737472696e677d", "parseMACTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f7574206e65742e49507d", "parseIPTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e697469616c54696d656f75742074696d652e4475726174696f6e3b20696e697469616c44656c74612074696d652e4475726174696f6e7d", "dialTimeoutTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6970206e65742e49503b206e6574202a6e65742e49504e65743b206f6b20626f6f6c7d", "ipNetContainsTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f6f6b757020737472696e673b2072657320737472696e677d", "lookupStaticHostAliasesTestᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20636e616d6520737472696e677d", "lookupCNAMETestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20656e7473205b5d6e65742e737461746963486f7374456e7472797d", "lookupStaticHostTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20686f737420737472696e677d", "lookupGmailMXTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b2074787420737472696e673b20686f737420737472696e677d", "lookupGmailTXTTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e677d", "lookupGoogleHostTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e657420737472696e673b206761646472202a6e65742e554450416464727d", "ipv4MulticastListenerTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e657420737472696e673b206c61646472202a6e65742e4950416464727d", "ipConnLocalNameTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e657420737472696e673b206c61646472202a6e65742e544350416464727d", "tcpListenerNameTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e657420737472696e673b206c61646472202a6e65742e554450416464727d", "udpConnLocalNameTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e6574776f726b20737472696e673b206164647265737320737472696e677d", "prohibitionaryDialArgTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e6574776f726b20737472696e673b2076657273696f6e20627974657d", "ipVersionTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e6574776f726b20737472696e677d", "fileConnTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e6574776f726b3120737472696e673b20616464726573733120737472696e673b206e6574776f726b3220737472696e673b20616464726573733220737472696e673b2078657272206572726f727d", "dualStackTCPListenerTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6f6e657320696e743b206269747320696e743b206f7574206e65742e49504d61736b7d", "cidrMaskTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73636f70652066756e63286e65742e49502920626f6f6c3b20696e206e65742e49503b206f6b20626f6f6c7d", "ipAddrScopeTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7365727669636520737472696e673b20706f727420696e743b206e656564734c6f6f6b757020626f6f6c7d", "parsePortTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7365727669636520737472696e673b2070726f746f20737472696e673b206e616d6520737472696e673b20636e616d6520737472696e673b2074617267657420737472696e677d", "lookupGoogleSRVTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b736e657420737472696e673b20736164647220737472696e673b20746e657420737472696e673b20746164647220737472696e673b206469616c20626f6f6c7d", "udpServerTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b736e657420737472696e673b20736164647220737472696e673b20746e657420737472696e673b20746164647220737472696e677d", "tcpServerTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b74696d656f75742074696d652e4475726174696f6e3b207865727273205b325d6572726f727d", "readTimeoutTestsᴛ1")]
[assembly: GoTypeAlias("Addr", "ΔAddr")]
[assembly: GoTypeAlias("Error", "ΔError")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<global::go.net_package.Conn, io_package.Reader>]
[assembly: GoImplement<global::go.net_package.Conn, io_package.Writer>]
[assembly: GoImplement<global::go.net_package.InvalidAddrError, global::go.net_package.ΔError>]
[assembly: GoImplement<global::go.net_package.TCPConn, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<global::go.net_package.TCPConn, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<global::go.net_package.UnknownNetworkError, error>]
[assembly: GoImplement<global::go.net_package.UnknownNetworkError, global::go.net_package.ΔError>]
[assembly: GoImplement<global::go.net_package.canceledError, error>]
[assembly: GoImplement<global::go.net_package.timeoutError, global::go.net_package.ΔError>(Pointer = true)]
[assembly: GoImplement<go.math.rand_package.Rand, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<testing_package.B, testing_package.TB>(Pointer = true)]
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<global::go.net_package.Resolver, ж<global::go.net_package.Resolver>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("net/pipe_test.go", "pipe_test.cs", "AA8ggsKCgoKUuIKChICCpICCpICCpICCpICCpICC", "17-24:1;19-22:1.1")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("net_test")]
public static partial class net_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha256() => builtin.initPackage(typeof(crypto.sha256_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸpoll() => builtin.initPackage(typeof(@internal.poll_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyscallꓸwindows() => builtin.initPackage(typeof(@internal.syscall.windows_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸinternalꓸsocktest() => builtin.initPackage(typeof(net.@internal.socktest_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸnetip() => builtin.initPackage(typeof(net.netip_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸsyscall() => builtin.initPackage(typeof(syscall_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸnetꓸdnsꓸdnsmessage() => builtin.initPackage(typeof(vendor.golang.org.x.net.dns.dnsmessage_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸnetꓸnettest() => builtin.initPackage(typeof(vendor.golang.org.x.net.nettest_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.net_package));
    }
}
