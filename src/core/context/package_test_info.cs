// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.context_package;
global using static global::go.context_internal_test_package;

// <ImportedTypeAliases>
global using netꓸAddr = go.net_package.ΔAddr;
global using netꓸError = go.net_package.ΔError;
global using reflectliteꓸKind = go.@internal.abi_package.ΔKind;
global using reflectliteꓸType = go.@internal.reflectlite_package.ΔType;
global using runtimeꓸError = go.runtime_package.ΔError;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static global::go.context_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("696e746572666163657b54696d656f7574282920626f6f6c7d", "TestDeadlineExceededSupportsTimeout_type")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<afterFuncContext, context_package.Context>(Pointer = true)]
[assembly: GoImplement<customCauseContext, context_package.Context>(Pointer = true)]
[assembly: GoImplement<customDoneContext, context_package.Context>(Pointer = true)]
[assembly: GoImplement<otherContext, context_package.Context>(Promoted = true)]
[assembly: GoImplement<otherContext, context_package.Context>]
[assembly: GoImplement<testing_package.T, global::go.context_internal_test_package.testingT>(Pointer = true)]
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
[assembly: go.GoPositionMap("context/afterfunc_test.go", "afterfunc_test.cs", "ABgugqaCptKCgoKU1tKCgtaCpsKCgoKClIKygoKCggAJCNKCgoKUgoKkoqSigoKSgqKkooKCkoKipKKCgpKUkoKipIKCgoKAkqSCgoCSyIKCgoCSpIKAksiCgpKAkqSCgJI=", "58-64:1;99-101:1;135-135:1")]
[assembly: go.GoPositionMap("context/benchmark_test.go", "benchmark_test.cs", "ABcgwoKClIKSgoKCgpSClIKC2KSCkoKCyqKCgoaSgoKCwoKCgpSipISCkoKCgpSCuISCggAJCoKCgoKCgqaCgoKCpoKCgoLugoL4goKCkoKmkoIACxCigpSSggALDKSCgpKCloKCgoKigqKS3IKCkoKWgoI=", "22-37:1;43-45:1;59-66:1;71-81:2;94-114:1;95-99:1.1;100-106:1.2;107-113:1.3;127-131:1;132-139:2;146-153:1;163-173:1;167-170:1.1;184-188:1")]
[assembly: go.GoPositionMap("context/example_test.go", "example_test.cs", "ABEgmgAKDIKCgpKCtKTIloKUgoKCAAQS2AALAoK6lLS0yqgACQaClLS0ygAKCNKGgIKCpJaChIIAAhLiotiCAAgUopLKgoKCqMaEgoKCwoSClIKUgqKkAAYWAAoCooKSgpSCpoKClJaCgoKUlIKCgpSUgpSCgqjoAAkGgoKSlKKCqIKUhIKUgoKo", "27-41:1;30-39:1.1;106-112:1;128-159:1;129-145:1.1;166-177:2;175-175:2.1;191-206:1;193-196:1.1;238-247:1;240-242:1.1;243-246:1.2")]
[assembly: go.GoPositionMap("context/net_test.go", "net_test.cs", "AA8agoKClII=")]
[assembly: go.GoPositionMap("context/x_test.go", "x_test.cs", "ABcokqSCpIKkggAOJsKCgpaC1IKCgpSk1oCS+IKCgpSk1oCS+KKEgJKmgoKEgoCCpICCpqTqgoLWpICC2qKCgoKStNaAgsQACgSChIKAkqSEgoKEgoKChIKEgvaChIKAkqSEgoKEgoKCpqKCgoKC1qSAggAIDoAAEQyigoCCpICCpICCuIKEgoSAkqaChICSpoKEgoSAkqaChIKEgoSChIIACQaCggAKEP6CAAcQggAHEIKCAAcQgoLcgqaUgoKUgILaooKClIKCgoKCgqiSgoKCgqiCgpCSpoLmgoLIgpKClOaCgsSkooKCooKUgoKCkuaCgsSkgqaCAAoGooSCgpKUhr6ClIKCtIKCtIKCgpSCgsaygoCCyIKk6ICSpIKCgoKCkuaklIKC1qS0pIKCgoSC1qSAgqSAgsiUgoKClIKCgIKkgIIACgqCgJKClICSgJL4goCSgpSAkoKUgJKCuLKAkoIADQaChoKUggAKBoLOADAqgoIABxCCggAHEIKCAAcQgoKCggAHEIKCgoIABxCCgoKCAAcQgoKCggAHEIKCgoIABxCCgoKCAAcQgoIABxCCggAHEIKCAAcQgoIABxCCgoIABxCCgoIADSCCgoIABxCCgoLckpKCgoCCpICCAAkMgoKCopSmgIKClKS4gpKCgoCCpICCpICCpICCABUSgqaigoKCuIKUgoSCgIKkgILEAAoWsqbSgoLW0oKC1oKmooKCgoKEgriC1pSmgoCCpICC7qaCgoKAgqSAgqSAgqSAgtyCgIKkgIKkgIKkgILKpoKCgoCCpICCpICCpICCyIKCgpKUtNaC5qSCuKKCkoKSlObEpIKCgoKSlObIgoKCkpSClIK01oK6soKCpJSSlObE", "211-221:1;276-276:1;282-285:2;291-294:3;300-304:4;310-314:5;353-356:1;361-361:2;376-379:3;392-395:1;422-424:1;456-462:2;535-535:1;539-539:2;546-546:1;550-550:2;554-554:3;561-561:1;605-609:1;615-619:2;625-629:3;635-641:4;647-653:5;659-665:6;671-677:7;683-689:8;695-701:9;707-711:10;717-721:11;727-731:12;737-741:13;747-752:14;758-763:15;769-771:16;777-782:17;788-793:18;799-808:19;815-817:1;1004-1006:1;1027-1029:1;1041-1043:1;1054-1056:1;1075-1078:1")]
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
