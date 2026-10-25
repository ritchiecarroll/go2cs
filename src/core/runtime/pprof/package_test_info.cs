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
global using abiꓸMapType = go.@internal.abi_package.ΔMapType;
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
global using windowsꓸContext = go.@internal.runtime.syscall.windows_package.Context;
global using windowsꓸMemoryBasicInformation = go.@internal.runtime.syscall.windows_package.MemoryBasicInformation;
// </ImportedTypeAliases>

using go;
using static global::go.runtime.pprof_internal_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<errWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<inlineWrapper, inlineWrapperInterface>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: go.GoHoistedLiterals]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("runtime/pprof/label_test.go", "label_test.cs", "ABEigpKCgpSCAAwGlIKCgriUgoKUgoKCuIKCgpSCgoK4goKClIKCgriCgoKUgoKCABEIggAZNoCCAA8KgoKCgoLIgpSCgoLIgpSCgoLIgoKClIKmgoKCgoLIgpSCgoLIgpSCgoI=", "19-22:1;23-23:2;120-126:1;124-124:1.1;128-136:2;134-134:2.1;138-146:3;144-144:3.1;157-185:4;158-164:4.1;162-162:4.1.1;166-174:4.2;172-172:4.2.1;176-184:4.3;182-182:4.3.1")]
[assembly: go.GoPositionMap("runtime/pprof/mprof_test.go", "mprof_test.cs", "AB0ugoK6kqaCAAkUgpSCvJKmgoIADwrCgriCgoK4griCgoKCgpSUlAApRJKCgIK2goLKkoKAgqSCgpSUgoKCuIKCuIKCgoKUgqaC2A==", "74-76:1;129-140:2;142-180:3", "", "116=Sprintf/1/5/5,Sprintf/2/5/11,Sprintf/3/5/17,Sprintf/4/5/23,Sprintf/5/5/29")]
[assembly: go.GoPositionMap("runtime/pprof/pprof_test.go", "pprof_test.cs", "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", "97-99:1;105-113:1;107-110:1.1;121-125:1;154-169:2;185-239:3;190-221:3.1;223-238:3.2;224-237:3.2.1;229-234:3.2.1.1;233-233:3.2.1.1.1;286-288:1;359-361:1;525-531:1;562-622:1;647-652:1;695-719:1;742-757:1;772-790:1;776-783:1.1;925-943:1;945-964:2;1023-1026:1;1049-1052:1;1058-1061:1;1067-1070:1;1078-1083:1;1095-1098:1;1108-1111:1;1123-1127:1;1134-1138:2;1147-1152:1;1243-1267:1;1268-1320:2;1322-1342:3;1352-1382:1;1425-1440:1;1447-1452:2;1448-1451:2.1;1594-1610:1;1616-1629:2;1633-1658:3;1634-1657:3.1;1664-1669:4;1701-1705:5;1707-1713:6;1717-1721:7;1722-1745:8;1730-1742:8.1;1731-1741:8.1.1;1749-1783:9;1757-1780:9.1;1758-1768:9.1.1;1769-1779:9.1.2;1791-1793:1;1795-1797:2;1801-1826:3;1809-1823:3.1;1810-1822:3.1.1;1830-1837:4;1840-1869:5;1848-1851:5.1;1872-1970:6;1890-1914:6.1;1897-1900:6.1.1;1921-1933:6.2;1934-1936:6.3;1989-2006:1;1991-1997:1.1;2010-2019:2;2050-2050:1;2066-2092:1;2067-2091:1.1;2070-2073:1.1.1;2078-2082:1.1.2;2094-2122:2;2095-2121:2.1;2104-2113:2.1.1;2114-2116:2.1.2;2124-2131:3;2133-2140:4;2196-2200:1;2197-2199:1.1;2209-2225:1;2216-2221:1.1;2217-2219:1.1.1;2234-2255:1;2239-2249:1.1;2252-2253:1.2;2257-2283:2;2265-2273:2.1;2274-2276:2.2;2295-2299:1;2296-2298:1.1;2349-2353:2;2398-2401:1;2427-2432:1;2458-2460:1;2680-2711:1;2725-2733:1;2769-2811:1;2840-2843:1;2855-2858:1;2871-2873:1;2953-2962:1;2966-2978:2;2979-2981:3;2982-2984:4;3042-3052:1;3068-3070:1;3071-3073:2;3078-3089:1;3091-3120:2;3130-3134:1")]
[assembly: go.GoPositionMap("runtime/pprof/proto_test.go", "proto_test.cs", "AB084oKCgoCCpICCpKyygqaEpIKCgpSAgraCgriCyqiQppDY8qaCgpSCkoIABxCUppSCgoKkgpTCgqYABxAACRKCpIKmgpSmgpTcgoKUgoK4AAkUpqKUgpSClIKUgriCgqaCgoKCpoKmAGa8AYSCgoKClIKUgoKUgsqSppIACBbygpS4goKCgpSUgoKmgoKUlIKCgoKUpoKmgoKClIKCuLiCggAHEKKClIKCgpTcsoKAgqSCgpSCgqaCgoKClKaCgoKUgoLOoriCgrjIgKSCgoI=", "97-107:1;317-334:1;327-329:1.1;336-338:2;340-342:3;361-413:1")]
[assembly: go.GoPositionMap("runtime/pprof/protomem_test.go", "protomem_test.cs", "ABwogtyCgtyC3AAXNu7SgoCCtoKCpsqiprKCgpS2ooKCgpSmsoKClLiygpSCgoKUgpSCpoKCgIKkgoKmgu6CgsYACxaCpoKClAAPCKKClIKmgoKCgqaClJSCgIKkgoKmgoKCgoKCgqaClIKCgrSkgoKC/MKCgoKmlIKCgIKkgoKmpoKCgpSmgpSClIK4grQ=", "76-88:1;128-130:1;194-197:1;245-247:1")]
[assembly: go.GoPositionMap("runtime/pprof/runtime_test.go", "runtime_test.cs", "ABIcgpSSgIKkkoCCpJSUgoKCgIKkkoCCpJSUgoKCgIKkkoCCpJSmgoKAgraCgoCCtoKSgoCCpJSogoCCgoLIgoKClIKCgpQ=", "21-26:1;35-40:2;49-54:3;64-80:1;71-77:1.1")]
// </GoSourcePositionMaps>

namespace go.runtime;

[GoPackage("pprof")]
public static partial class pprof_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("T")] partial class TestGoroutineProfileConcurrency_T {}
    [GoLocalName("T")] partial class TestGoroutineProfileIssue74090_T {}
    [GoValueClone("pad")] partial struct Obj32 {}
    [GoLocalName("TestCase")] partial struct TestBlockProfile_TestCase {}
    [GoValueClone("x")] partial struct allocateTransient1M_type {}
    [GoLocalName("nkey")] partial struct containsCountsLabels_nkey {}
    [GoValueClone("buf")] partial struct opAlloc {}
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
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
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
