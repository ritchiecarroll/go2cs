// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.os.exec_package;
global using static global::go.os.exec_internal_test_package;

// <ImportedTypeAliases>
global using execꓸError = go.os.exec_package.ΔError;
global using flagꓸErrorHandling = go.flag_package.ΔErrorHandling;
global using httpꓸCookie = go.net.http_package.ΔCookie;
global using httpꓸHandler = go.net.http_package.ΔHandler;
global using httpꓸHeader = go.net.http_package.ΔHeader;
global using jsonꓸToken = object;
global using jsonꓸΔToken = object;
global using netꓸAddr = go.net_package.ΔAddr;
global using netꓸError = go.net_package.ΔError;
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
// </ImportedTypeAliases>

using go;
using static global::go.os.exec_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("696e746572666163657b466428292075696e747074727d", "TestStdinClose_type")]
[assembly: GoTypeAlias("Error", "ΔError")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<badWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<delayedInfiniteReader, io_package.Reader>]
[assembly: GoImplement<go.net.http_package.HandlerFunc, go.net.http_package.ΔHandler>]
[assembly: GoImplement<os_package.File, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<syscall_package.ΔSignal, os_package.ΔSignal>]
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
[assembly: GoImplement<tickReader, io_package.Reader>(Pointer = true)]
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
[assembly: go.GoPositionMap("os/exec/dot_test.go", "dot_test.cs", "ABcmgpQADQailoKAgqaCgpSAgqSChKiCgoKCgoKCgIKkgoKAgriCgoKkpoKogoKCpJSUgsyAgqQAFi6CgoKAgqaAgqaCgqjKgoKAgqQACBSCgoKUgIKkhIKCgoKmgoK6AAsekoKCgqi2goKCgqi2goKCgg==", "49-101:1;52-99:1.1;120-135:2;137-179:3;181-193:4;182-192:4.1;196-201:5;204-212:6;215-223:7")]
[assembly: go.GoPositionMap("os/exec/exec_test.go", "exec_test.cs", "ADJWgoKClIKEgpSClIKCloKCAAoS4oSCgoQACxiWgoKCgIKCyqaClpaCgoKWkoKCgpSCAAIQ0oKUrLKosoKq0oSCgoKClJQAEySigoKUpqKCuMKCgpSCgoKCgpSSptaCgoKCgqSUgoKkgpTKgoKCgpSAgoLIooKmooKCgoK4goKmooKUgoKCggAKCoKEgoKUgJIACQiChKiCgoKUgoKCloKEgoKUgJIACAiCloKCgoKClIKC6IKEgoKClICCpILCgsSAgqQACAaCloKAgqSCgpSClIL4opaCgviCloKCgpSkgIKAgrbIgpaCgoKClIKCloKCgoKUgoKWgoKCgpSCgpaCgoKCgqiCgoKCABQIgoSCgriCgoKCgoKEgoKCgoKUloKEgoKAkqaCgoCSpoKCgJKmgoIADQyyhIKCpoKClICGpISCgoLChIKWgILoAAgSAAoChIKCgpSAgqiCgoSy7say3IKAguiAggAMCrKCqIKCgoKmloKCloKCgoKCgIIACBKCggAOCsKClgANHJaCqISCuoKClKaCgpSSgoKUuKSCgpKCgpaCgpSClIKCgpSCgpaChKaCgIKCuIKApoSCgraCkoKCggAJFJSCgpSCAAoIgoKClISCgoKUlIKCgpSUgoKClKiCgpSCgoKCgoKCgpKSgJKkgJKkgoKClIL+goKClNiShAARIILagtaChIKCgoL4goSCgoKClIKCguiChIKCgoKUgoKUgIKmgIKkgoKClJSAggAICMKCgrqEgpKEgoKUlICCuICCpqiCgoKAgqamgrqCgpSWgIKUAAwKkoSCgoKClICSAAgIgoKUgoKCggAJCIKEgoKUAAQSgoKAggALCoKEgoKWgoKAgviCgoKCAAkOsoSCgoKUgoKWgIKkkoCCuICCpoKigoKUloSCgpSEgoK4ooKCloKCgoKChISCgoKCgoKClISCgIKCgoKkgqaCgoKSgoKmqISCgpSCgoKUlIKCuoKCkoKAgtyCggALGIKmooKAgqSCloKCpqKEgoKCgoKUqIKCgpaCgIK4goCCgoKCpIKWAA8GoorYgoKCgoKEgpSAgqKC7IKClISCgoS4gIKmgoLegpSAgqKCAAgQgoSCgoKCgt6AggAIEoKEgoKCzILegoKUhIKCgoKCqICCAAcQgoKUhIKCgoKChIKUgIKigv6CgpSEgoKCgoKEgIKmgIKilLaCAAwKoqiihIKUgoKCloKigoKUgIKkhIKCggAJEKKEgpSCgoKWgoLMgoKigoKCloCCpoKCgoKEgoKCAAgOooSClIKCgpSUgoKSgoKUgoCCpISCqILMgIIACQ6ihIKUgoKClJSCgoKClIKAgqSEgqiCzICCAAkOooSClIKCgpaCgqKClICCpIKChIKEgIIACBLiAAASAAwKgozChIKyhIKCgoKClJKEgoCCgpSm2IKyhIKEgoCC+oKCqqKEgqKCgpaCAAkGgoKokoKClIKCloKCgpaCgoK6griUgoKCgoKCgg==", "356-359:1;473-477:1;489-495:2;529-533:1;548-558:2;586-595:1;597-608:2;622-630:1;714-714:1;798-804:1;805-811:2;812-819:3;866-881:1;867-880:1.1;1107-1111:1;1118-1124:2;1180-1184:1;1212-1218:2;1263-1265:1;1303-1318:1;1322-1357:2;1363-1381:3;1388-1403:4;1408-1426:5;1432-1453:6;1458-1485:7;1493-1521:1;1506-1510:1.1;1527-1571:2;1549-1554:2.1;1576-1617:3;1591-1595:3.1;1622-1662:4;1636-1640:4.1;1667-1698:5;1681-1684:5.1;1731-1753:1;1757-1767:2;1781-1785:1;1821-1837:1")]
[assembly: go.GoPositionMap("os/exec/exec_windows_test.go", "exec_windows_test.cs", "ABk0gqaigoKCgoKUAAgGgoSCgpSCgoKCgpSCgoKUgoKUgoL4goSCgoKCgpSC/rKChpKCgoKmhIKClII=")]
[assembly: go.GoPositionMap("os/exec/lp_windows_test.go", "lp_windows_test.cs", "ABswgqaCAAIQ0oKClLS0xtqigoSCgIKmgpSClAAEFPKCgpSUgoKUkoCCuIKC7MKCgpSSgIK4gIIAogG2Auy6goKWspKCloKEgoKWgoKCgoKCppSUgoSEpoKCgoKCgpSUtoCCpKiCgoKCpoKUggCxAeQC7ISykoKWgoSCgoSEgoKCgpSogoKAgpSkgpSWgoKCgqaCloKCgoKklKaCAAgMggAHFIKChIKCgoKWgoI=", "90-94:1;109-113:1;288-355:1;540-602:1")]
// </GoSourcePositionMaps>

namespace go.os;

[GoPackage("exec_test")]
public static partial class exec_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface TestStdinClose_type {}
    internal partial struct TestString_tests {}
    internal partial struct badWriter {}
    internal partial struct commandTest {}
    internal partial struct delayedInfiniteReader {}
    internal partial struct lookPathTest {}
    internal partial struct tickReader {}
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
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸpoll() => builtin.initPackage(typeof(@internal.poll_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttp() => builtin.initPackage(typeof(go.net.http_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸhttptest() => builtin.initPackage(typeof(go.net.http.httptest_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸsignal() => builtin.initPackage(typeof(go.os.signal_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸruntimeꓸdebug() => builtin.initPackage(typeof(go.runtime.debug_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸsyscall() => builtin.initPackage(typeof(syscall_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.os.exec_package));
    }
}
