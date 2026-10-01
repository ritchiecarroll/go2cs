// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static go.runtime_package;
using static go.runtime_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("696e746572666163657b537472696e67282920737472696e677d", "TestMapInterfaceKey_GrabBag_i1")]
[assembly: GoDynamicTypeLift("7374727563747b6120696e743b206220696e743b206320696e743b206420696e743b206520696e743b206620696e743b206720696e743b206820696e743b206920696e747d", "globstructᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6120696e743b206220696e743b206320696e743b2078205b325d696e747d", "TestTracebackArgs_b")]
[assembly: GoDynamicTypeLift("7374727563747b6c696e654120696e743b206c696e654220696e747d", "compLitᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6d696e20666c6f617436343b206d617820666c6f617436347d", "testsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b206461746120737472696e677d", "stringdataᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b70205b315d2a696e747d", "BenchmarkMallocTypeInfo8_type")]
[assembly: GoDynamicTypeLift("7374727563747b70205b325d2a696e747d", "BenchmarkMallocTypeInfo16_type")]
[assembly: GoDynamicTypeLift("7374727563747b73796e632e4d757465783b2064697220737472696e673b20746172676574206d61705b737472696e675d2a72756e74696d655f746573742e6275696c646578657d", "testprogᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820666c6f617436343b2079205b343339383436353934313530345d2a627974653b207a205b5d737472696e677d", "TestHugeGCInfo_type")]
[assembly: GoDynamicTypeLift("7374727563747b7820666c6f617436343b2079205b343339383436353934313530345d75696e747074723b207a205b5d737472696e677d", "TestHugeGCInfo_typeᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820696e743b2079205b305d696e743b207a205b325d5b305d696e747d", "TestTracebackArgs_x")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
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
[assembly: global::go.GoPositionMap("runtime/align_runtime_test.go", "align_runtime_test.cs", "AA8eABo0")]
[assembly: global::go.GoPositionMap("runtime/export_debuglog_test.go", "export_debuglog_test.cs", "ABIigqaAooCigKKAooCigKKAooCigKSCgoKCgoSmgoKCgoKClKaCgoKClII=")]
[assembly: global::go.GoPositionMap("runtime/export_map_swiss_test.go", "export_map_swiss_test.cs", "AAoS")]
[assembly: global::go.GoPositionMap("runtime/export_test.go", "export_test.cs", "ABskkpKSkpKSkpKkopKSpKSUkpKUoqKkoqSUkqT+kqS8ggAJFIKmgqSCpoKCuKKClPaCgoKCgoCCpIKUgoCCgraAggAICoKCgoKCgoKClIKCgoKUgoKClIKUgoKClJSCgoKmgoL6yoKCgoKCgoKCgoKylIKClJSUgoKCuoKCgoKCgpKSppSigsyykpS2oqKilICigNiasoKmkpSyhISCgqiEpoCigKKAyIKmggAFEIKmgqqCpoKogoSm3sy6lpSEttjSlpSEqIKCgoKCgpqCgpSClICCgpSCgrqShpKCgoKClIKEhIKCloKCgpSClIKClpaCrNKCgJKEgpK4oqqypoKCgu6CpoKmgqaCpoKmgoKCgqaCpgAGRgAKAoKCgoKUlIKCppSmgq6CpoKmoraWkqiSAAQkgoKm3oKCgpbMhIKCuoKCAAocgKKAooCigMqCpICigKKAooCowqSCgpSClIKCgpSUlKiQqKKChIKSgoKClJSClKaClKyygoKClKbMgqSAooKkgqSCqJDugqSAooCigKKAooKkgoKExoKCAAcWgoSStoKClKSChIK2goKUpIKEtoKCtoKkooKSlKSCgoKUqJKCAAcSkqiSqJKokqiS/gAHIgAJApKCAAIQAAwMgoKCgpQABhKigoKUqqLe4oKUqJIABywADgKWgoKClpKCgqiClrqCloKAgqaC7qaClrqSgoKorLKWgoKmgoKUqMyCloKAgpQACxaCgoKUlIKClKqiAAcQspaSloKCgoKClNyCgoKCgpTK2ISUpqKWgqaCqIKmoqiilJSCgraiAAoekoKCgoKssoKCgpTOkoKCgoKUqJKCgoK4goKCgoKCAAoiwoKClIKClKaCppSCgoKClIKmgtyYwgACEuIADiLKgoKCpqKCgpSCgoKCpoKmgqaCpoKmggAKFoKCgoKCgqaCgoKCpoKmgqaCpoLsgoKUqJKmgqbcggAJEoIACRjKgoKCpoKmgqaCpoKmgqaCpoKmgqaCpoIADyKigqiCgoKWgoKCgrSCgoKkgsaCgoKUgqaSAAMUAA0MgoKCgoKClJSokqiSqsKCgoKClIKC7oIACxiCgoKCgqaCgqaCkpSUpoKClLiCkpSUpoKClLiigoKClJSmgqaCpoLcgoLaAAcQggAEGoKmgqaCgoKCgpSUpoKmlIKmoqqiqqLckpaCpILKgoK4goKClKSopKIACBCCpoKmgqaUgoKClIKCpoKCgoKClILugqaCpII=", "92-94:1;98-100:1;186-194:1;227-229:1;313-338:1;352-424:1;435-435:1;439-441:2;609-612:1;769-775:1;789-795:1;802-808:1;814-820:1;827-829:1;988-992:1;1033-1037:2;1117-1153:1;1231-1235:1;1241-1245:1;1550-1569:1;1753-1759:1;1806-1808:1")]
[assembly: global::go.GoPositionMap("runtime/export_windows_test.go", "export_windows_test.cs", "ABAikgAHGoI=")]
[assembly: global::go.GoPositionMap("runtime/proc_runtime_test.go", "proc_runtime_test.cs", "AAoSgoKCgoKUgoKCgoKCgpSClILcgoKmgoKCgoKU")]
[assembly: global::go.GoPositionMap("runtime/symtabinl_test.go", "symtabinl_test.cs", "ABgagoKWgoKCltzegoKCgoAABxCkgoKCgpaEgoKClIKWgoKUgpaClJaChISAgtyCgsqCgsrEkqSigqSigoI=")]
[assembly: global::go.GoPositionMap("runtime/tracebackx_test.go", "tracebackx_test.cs", "AAkOuoKSgpQ=", "13-16:1")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("runtime")]
public static partial class runtime_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸast() => builtin.initPackage(typeof(global::go.go.ast_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸbuild() => builtin.initPackage(typeof(global::go.go.build_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸimporter() => builtin.initPackage(typeof(global::go.go.importer_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸparser() => builtin.initPackage(typeof(global::go.go.parser_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸprinter() => builtin.initPackage(typeof(global::go.go.printer_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtypes() => builtin.initPackage(typeof(global::go.go.types_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸabi() => builtin.initPackage(typeof(@internal.abi_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸplatform() => builtin.initPackage(typeof(@internal.platform_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸprofile() => builtin.initPackage(typeof(@internal.profile_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸrace() => builtin.initPackage(typeof(@internal.race_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸruntimeꓸmaps() => builtin.initPackage(typeof(@internal.runtime.maps_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸruntimeꓸsys() => builtin.initPackage(typeof(@internal.runtime.sys_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸstringslite() => builtin.initPackage(typeof(@internal.stringslite_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyscallꓸwindows() => builtin.initPackage(typeof(@internal.syscall.windows_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyscallꓸwindowsꓸsysdll() => builtin.initPackage(typeof(@internal.syscall.windows.sysdll_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtrace() => builtin.initPackage(typeof(@internal.trace_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(global::go.math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸcmplx() => builtin.initPackage(typeof(global::go.math.cmplx_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(global::go.math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(global::go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸruntimeꓸdebug() => builtin.initPackage(typeof(global::go.runtime.debug_package));
    [GoInit] internal static void initᴛᴛimportꓸruntimeꓸmetrics() => builtin.initPackage(typeof(global::go.runtime.metrics_package));
    [GoInit] internal static void initᴛᴛimportꓸruntimeꓸpprof() => builtin.initPackage(typeof(global::go.runtime.pprof_package));
    [GoInit] internal static void initᴛᴛimportꓸruntimeꓸtrace() => builtin.initPackage(typeof(global::go.runtime.trace_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(global::go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸsyscall() => builtin.initPackage(typeof(syscall_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(unicode.utf8_package));
    [GoInit] internal static void initᴛᴛimportꓸweak() => builtin.initPackage(typeof(weak_package));
    // </ImportInitializers>
}
