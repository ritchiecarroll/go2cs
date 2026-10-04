// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.runtime.pprof_package;

// <ImportedTypeAliases>
global using abiꓸArrayType = go.@internal.abi_package.ΔArrayType;
global using abiꓸChanDir = go.@internal.abi_package.ΔChanDir;
global using abiꓸFuncType = go.@internal.abi_package.ΔFuncType;
global using abiꓸInterfaceType = go.@internal.abi_package.ΔInterfaceType;
global using abiꓸKind = go.@internal.abi_package.ΔKind;
global using abiꓸName = go.@internal.abi_package.ΔName;
global using abiꓸStructType = go.@internal.abi_package.ΔStructType;
global using bigꓸInt = go.math.big_package.ΔInt;
global using bigꓸRat = go.math.big_package.ΔRat;
global using execꓸError = go.os.exec_package.ΔError;
global using jsonꓸToken = object;
global using jsonꓸΔToken = object;
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
global using syscallꓸHandle = go.syscall_package.ΔHandle;
global using syscallꓸSignal = go.syscall_package.ΔSignal;
global using syscallꓸSockaddr = go.syscall_package.ΔSockaddr;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static global::go.runtime.pprof_internal_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<inlineWrapper, inlineWrapperInterface>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
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
[assembly: go.GoPositionMap("runtime/pprof/label_test.go", "label_test.cs", "AA8ggpKCgpSCAAwGlIKCgqiUgoKUgoKCqIKCgpSCgoKogoKClIKCgqiCgoKUgoKCABEIggAYNoCCAA8KgoKCgoK4goSCgoK4goSCgoK4goKClIKWgoKCgoK4goSCgoK4goSCgoI=", "18-21:1;22-22:2;119-125:1;123-123:1.1;127-135:2;133-133:2.1;137-145:3;143-143:3.1;156-184:4;157-163:4.1;161-161:4.1.1;165-173:4.2;171-171:4.2.1;175-183:4.3;181-181:4.3.1")]
[assembly: go.GoPositionMap("runtime/pprof/mprof_test.go", "mprof_test.cs", "AB0ugoK6kqaCAAkUgpSCvJKmgoIADwrCgqiCgoKogqiCgoKCgoSEhAAoRJKCgIKmgoK6koKAgqSCgpSEgoKCqIKCqIKCgoKUgqaC2A==", "74-76:1;129-140:2;142-180:3")]
[assembly: go.GoPositionMap("runtime/pprof/pprof_test.go", "pprof_test.cs", "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", "96-98:1;104-112:1;106-109:1.1;124-128:1;157-172:2;188-242:3;193-224:3.1;226-241:3.2;227-240:3.2.1;232-237:3.2.1.1;236-236:3.2.1.1.1;289-291:1;357-359:1;523-529:1;560-620:1;649-654:1;697-721:1;744-759:1;774-788:1;778-781:1.1;923-941:1;943-962:2;1021-1024:1;1047-1050:1;1056-1059:1;1065-1068:1;1076-1081:1;1093-1096:1;1106-1109:1;1121-1125:1;1132-1136:2;1145-1150:1;1241-1265:1;1266-1318:2;1320-1340:3;1350-1380:1;1423-1438:1;1445-1450:2;1446-1449:2.1;1576-1578:1;1580-1582:2;1586-1611:3;1594-1608:3.1;1595-1607:3.1.1;1615-1622:4;1625-1654:5;1633-1636:5.1;1657-1755:6;1675-1699:6.1;1682-1685:6.1.1;1706-1718:6.2;1719-1721:6.3;1774-1791:1;1776-1782:1.1;1795-1804:2;1835-1835:1;1851-1877:1;1852-1876:1.1;1855-1858:1.1.1;1863-1867:1.1.2;1879-1907:2;1880-1906:2.1;1889-1898:2.1.1;1899-1901:2.1.2;1909-1916:3;1918-1925:4;1981-1985:1;1982-1984:1.1;1994-2010:1;2001-2006:1.1;2002-2004:1.1.1;2019-2040:1;2024-2034:1.1;2037-2038:1.2;2042-2068:2;2050-2058:2.1;2059-2061:2.2;2080-2084:1;2081-2083:1.1;2134-2138:2;2183-2186:1;2212-2217:1;2243-2245:1;2465-2496:1;2510-2518:1;2554-2595:1;2632-2635:1;2647-2650:1;2663-2665:1;2742-2751:1;2755-2767:2;2768-2770:3;2771-2773:4;2831-2841:1;2857-2859:1;2860-2862:2;2867-2878:1;2880-2909:2;2919-2923:1")]
[assembly: go.GoPositionMap("runtime/pprof/proto_test.go", "proto_test.cs", "ABw64oKCgoCCpIKssoKmhJSCgoKUgIKmgoKogrqokKaQ2PKWgpKUgpKCAAcQlKaUgoKCpIKEwoKWAAcQAAkSgqSChqKkpoKEAAkMgoKUgoK4AAkUpqKEgpSClIKUgriCgqaSgoKCpoKmAGa8AYSCgoKClIKUgoKUgrqSlpIACBbygoSogoKCgpSEgoKWgoKUhIKCgoKUpoKWgoKClIKCqLiCggAHEKKClIKCgpTcsoKAgqSCgpSCgpaCgoKClKaCgoKUgoLOotiCgg==", "94-104:1;314-331:1;324-326:1.1;333-335:2;337-339:3;358-410:1")]
[assembly: go.GoPositionMap("runtime/pprof/protomem_test.go", "protomem_test.cs", "ABwogsyCgsyC3AAXNu7SgoCCpoKClsqipqKCgpSmooKCgpSmooKClKiygpSCgoKUgpSCloKCgIKkgoKWgt6CgsYACxaCpoKClAAPCKKClIKWgoKCgpaChISCgIKkgoKWgoKCgoKCgqaClIKCgrSkgoKCAAgMwoKCgpaEgoKAgqSCgpaWgoKClJaClIKUgpSSqIK0", "76-88:1;128-130:1;194-197:1;245-247:1")]
[assembly: go.GoPositionMap("runtime/pprof/runtime_test.go", "runtime_test.cs", "ABAcgoSSgIKkkoCCpJSEgoKCgIKkkoCCpJSEgoKCgIKkkoCCpJSmgoKAgqaCgoCCpoKSgoCCpJSYgoCCgoLIgoKClIKClA==", "21-26:1;35-40:2;49-54:3;64-80:1;71-77:1.1")]
// </GoSourcePositionMaps>

namespace go.runtime;

[GoPackage("pprof")]
public static partial class pprof_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
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
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸabi() => builtin.initPackage(typeof(@internal.abi_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸprofile() => builtin.initPackage(typeof(@internal.profile_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸiter() => builtin.initPackage(typeof(iter_package));
    [GoInit] internal static void initᴛᴛimportꓸmaps() => builtin.initPackage(typeof(maps_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(go.math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸruntimeꓸdebug() => builtin.initPackage(typeof(go.runtime.debug_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.runtime.pprof_package));
    }
}
