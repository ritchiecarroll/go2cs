// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.context_package;
global using static global::go.context_internal_test_package;

// <ImportedTypeAliases>
global using netꓸError = go.net_package.ΔError;
// </ImportedTypeAliases>

using go;
using static global::go.context_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("696e746572666163657b54696d656f7574282920626f6f6c7d", "TestDeadlineExceededSupportsTimeout_type")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<afterFuncContext, context_package.Context>(Pointer = true)]
[assembly: GoImplement<customCauseContext, context_package.Context>(Pointer = true)]
[assembly: GoImplement<customContext, context_package.Context>(Pointer = true)]
[assembly: GoImplement<customDoneContext, context_package.Context>(Pointer = true)]
[assembly: GoImplement<otherContext, context_package.Context>(Promoted = true)]
[assembly: GoImplement<otherContext, context_package.Context>]
[assembly: GoImplement<testing_package.T, global::go.context_internal_test_package.testingT>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(afterFuncContext))]
[assembly: GoFieldAccess(typeof(customContext))]
[assembly: GoFieldAccess(typeof(global::go.context_package.afterFuncCtx))]
[assembly: GoFieldAccess(typeof(global::go.context_package.cancelCtx))]
[assembly: GoFieldAccess(typeof(global::go.context_package.timerCtx))]
[assembly: GoFieldAccess(typeof(sync_package.Cond))]
[assembly: GoFieldAccess(typeof(time_package.Timer))]

// The go statements whose goroutine the converter proved cannot recurse, by the file, line and column of
// their goǃ call: go2cs-gen starts each on golib's bounded stack reservation rather than the Go-scale default.
[assembly: GoBoundedGoroutine("x_test.cs", 513, 9, 1)]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("context/afterfunc_test.go", "afterfunc_test.cs", "ABYulIKm0oKCgpTW0oKC1oKmwoKCgoKUgrKCgoKCAAkI0oKCgpSCgqSipKKCgpKCoqSigoKSgqKkooKCkpSSgqKkgoKCgoCSpIKCgJLIgoKCgJKkgoCSyIKCkoCSpIKAkg==", "56-62:1;97-99:1;133-133:1", "", "", "122:21,136:21,154:21")]
[assembly: GoPositionMap("context/benchmark_test.go", "benchmark_test.cs", "ABkgwoKCpIKSgoKCgpSClIKC2KSCkoKCyqKCgpaSgoKCwoKCgpSipJSCkoKCgpSCuJSCggAMCoKCgoKCgqaCgoKCpoKCgoLugoIACQiCgoKSgqaSggALEKKCpJKCAAsMpIKCgoKmgoKCgqKCopLcgoKCgqaCgtyigpKCgsakgoKCgoI=", "22-37:1;43-45:1;59-66:1;71-81:2;94-114:1;95-99:1.1;100-106:1.2;107-113:1.3;127-131:1;132-139:2;146-153:1;163-173:1;167-170:1.1;184-188:1", "", "", "34:21,197:21,263:21")]
[assembly: GoPositionMap("context/example_test.go", "example_test.cs", "ABEgmgAKDIKCgpKCtKTIpoKkgoKCAAoS2MKCyqS0tPqo5oKktLT6AAwIooaCgpSmgpSCAAYSoqLYggAJFKKS2oKCgrjWlIKCgsKUgqSCpIKipAAOFsKigpKClIKmgoKUpoKCgpSkgoKClKSCpIKC2AAICOaCgpKUooK4gqSUgqSCgtg=", "27-41:1;30-39:1.1;106-112:1;128-159:1;129-145:1.1;166-177:2;175-175:2.1;191-206:1;193-196:1.1;238-247:1;240-242:1.1;243-246:1.2", "", "", "54:21,88:21,118:21,200:25,232:29,291:21,299:21,303:21,351:21")]
[assembly: GoPositionMap("context/net_test.go", "net_test.cs", "AA8agoKClII=")]
[assembly: GoPositionMap("context/x_test.go", "x_test.cs", "ABcqkpSClIKUggAOJsKCgqaC1IKCgpSk1pL4goKClKTWkviClJKmgoKUgoKUgqak+oKC1qSCyqKCgoKStNaCtAAQBIKUgpKUlIKClIKCgpSClIIACgaClIKSlJSCgpSCgoKmgoKCgoLWpILuABUMgoKAgqSAgqSAgsiClIKUgJK2gpSAkraClIKUgJK2gpSClIKUgpSCAAwGgoKUggAKEP6CAAcQggAHEIKCAAcQgoLcgqaUgoKUgsrCgoKUgoKCgoKCqJKCgoKCuIKCkJK2guaCgsiCkoKU5oKCxKSigoKigpSCgoKS5oKCxKSCpoIADgbSlIKCkpSGvoKUgoK0goK0goKClIKCtrKCgriCpOiSlIKCgoKCkuaklIKC1qS0pIKCgpSC1qSAgqSAgsiUgoKCpIKCgIKkgIIACwqCgJKClICSkuiCgJKClICSgpSAkoK4soCSggANBoKGgpSCAAkGgs4ANSqCggAHEIKCAAcQgoIABxCCgoKCAAcQgoKCggAHEIKCgoIABxCCgoKCAAcQgoKCggAHEIKCgoIABxCCggAHEIKCAAcQgoIABxCCggAHEIKCggAHEIKCggANIIKCggAHEIKCggAHEIKCggAHEIKCgoIABxCCgoKCAAwUkoKCgIKkgIIACgyCgoKilKaCgpSUuIKSgoKClIKUgpSCABQSgqaigoKCuIKkgpSCgIKkgILEAAoWsqbSgoLW0oKC1oKmooKCgoKUgriC5pSmgoCCpICC/qaCgoKAgqSAgqSAgqSAguyCgIKkgIKkgIKkgILapoKCgoCCpICCpICCpICCyIKCgpKUtNaC5qSCuKKCkoKSlObEpIKCgoKSlObIgoKCkpSClIK01oK6soKCpJSSlObEAAoWgriSkoKmkpKCyoKmgqaCpKS4gqikgoKCkoKCptY=", "212-222:1;280-280:1;286-289:2;295-298:3;304-308:4;314-318:5;357-360:1;365-365:2;380-383:3;396-399:1;426-428:1;460-466:2;539-539:1;543-543:2;550-550:1;554-554:2;558-558:3;565-565:1;609-613:1;619-623:2;629-633:3;639-645:4;651-657:5;663-669:6;675-681:7;687-693:8;699-705:9;711-715:10;721-725:11;731-735:12;741-745:13;751-756:14;762-767:15;773-775:16;781-786:17;792-797:18;803-808:19;814-820:20;826-832:21;841-850:22;857-859:1;1046-1048:1;1069-1071:1;1083-1085:1;1096-1098:1;1117-1120:1;1145-1150:1;1146-1149:1.1;1151-1156:2;1152-1155:2.1", "", "", "170:21,493:21,549:21,663:25,1215:21,1446:21,1525:21,1604:19", "794:21,795:13")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("context_test")]
public static partial class context_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface TestDeadlineExceededSupportsTimeout_type {}
    [GoLocalName("favContextKey")] internal partial struct ExampleWithValue_favContextKey {}
    internal partial struct TestAllocs_type {}
    internal partial struct TestCause_type {}
    internal partial struct afterFuncContext {}
    internal partial struct customCauseContext {}
    internal partial struct customContext {}
    internal partial struct customDoneContext {}
    internal partial struct key1 {}
    internal partial struct key2 {}
    internal partial struct otherContext {}
    [GoLocalName("value")] internal partial struct testLayers_value {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.context_package));
    }
}
