// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.@internal.sync_package;
global using static global::go.@internal.sync_internal_test_package;

// <ImportedTypeAliases>
global using abiꓸArrayType = go.@internal.abi_package.ΔArrayType;
global using abiꓸChanDir = go.@internal.abi_package.ΔChanDir;
global using abiꓸFuncType = go.@internal.abi_package.ΔFuncType;
global using abiꓸInterfaceType = go.@internal.abi_package.ΔInterfaceType;
global using abiꓸKind = go.@internal.abi_package.ΔKind;
global using abiꓸName = go.@internal.abi_package.ΔName;
global using abiꓸStructType = go.@internal.abi_package.ΔStructType;
global using runtimeꓸError = go.runtime_package.ΔError;
// </ImportedTypeAliases>

using go;
using static global::go.@internal.sync_test_package;

// <ExportedTypeAliases>
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
[assembly: go.GoPositionMap("internal/sync/hashtriemap_bench_test.go", "hashtriemap_bench_test.cs", "AA0YgqaCpoKmooKCgpSCkoKCgoKC3IKmgqaigoSSgoKCgoI=", "31-40:1;55-64:1")]
[assembly: go.GoPositionMap("internal/sync/hashtriemap_test.go", "hashtriemap_test.cs", "ABUkgoKCuIKqgsoAFAiigoSCtoKEgpKSkqSCkraChJiCgoSCkpKSpIKCtoKWgpKkgoKCgrKElIK0uIKWloLIgoKEgoKSkpKkgpKCgoKkgsiChIKSkpKkgoKCgoKkyIKEgpKSkqSCgoKUgoKkyIKEkoKUgraChIKCgoKyhIaCgpKSkqSCgpKCpIKCxKSUgpaCkqSCgoKCsoSCgoKkgsSkpoKChIKSkpKkgoKSgoKCtoK2goSCkpKSpIKCgoKCpMiChIKSkpKkgoKClIKCpMqChIKCgoKyhIaCgpKSkqSCgpKCpIKCxKSUgoSCgoKCsoSGgoKSkpKkgoKSgpKCgoKkgoLEpJSCloKSpIKCgoKyhIKCgqSCxKSmgoKEgpKSkqSCgpKStoK2goSCkpKSpJKCgqTIgoSCkpKSpIKkgoKkyIKEgoKCgrKEhoKCkpKSpIKCkpKkgoLEpJSChIKCgoKyhIaCgpKSkqSCgpKSkoKSpIKCxKSUgpaCkqSCgoKCsoSCgqSCxKSmgoKEgoKSkpKkgpKSkqSCyIKEgpKSkqSSkpKSgoKkyIKEgpKSkqSCkpKSpIKCpMiChJKSlIK2goSCgoKCsoSGgoKSkpKkgoKSkqSCgsSklIKWgpKkgoKCgrKEgoKkgsSkyoKCpIKigoKClIKClIKUgoLKgoKChIKUgsqCgqaUgoSClILKgoKChIKUgsqCgoKEgpSCyoKCgoSCyoKCgoSCyoKCgoSCyoKCgoSCyoKCgoSCkoLKgoKChILKgoKChIKSgsqCgoKEgsqCgoKUAAYSgoKUgpSCAAoW8oiKlJKCgoKCgoKUgIK4goK6goSCgoKygoKCgrSk", "19-22:1;26-28:1;32-38:1;42-48:1;49-62:2;63-69:3;66-68:3.1;70-120:4;71-84:4.1;85-119:4.2;97-105:4.2.1;121-258:5;122-143:5.1;144-163:5.2;164-185:5.3;186-196:5.4;189-192:5.4.1;197-230:5.5;204-227:5.5.1;207-209:5.5.1.1;231-257:5.6;243-254:5.6.1;259-424:6;260-281:6.1;282-301:6.2;302-323:6.3;325-358:6.4;332-355:6.4.1;335-337:6.4.1.1;359-396:6.5;366-393:6.5.1;369-371:6.5.1.1;397-423:6.6;409-420:6.6.1;425-581:7;426-445:7.1;446-463:7.2;464-483:7.3;484-517:7.4;491-514:7.4.1;494-496:7.4.1.1;518-554:7.5;525-551:7.5.1;528-530:7.5.1.1;555-580:7.6;567-577:7.6.1;582-719:8;583-603:8.1;604-624:8.2;625-647:8.3;648-658:8.4;651-654:8.4.1;659-692:8.5;666-689:8.5.1;669-671:8.5.1.1;693-718:8.6;705-715:8.6.1;727-739:1;749-758:1;768-777:1;782-791:1;796-805:1;810-816:1;821-827:1;832-838:1;843-849:1;854-862:1;867-873:1;878-886:1;891-897:1;942-944:1;945-964:2;972-979:3")]
// </GoSourcePositionMaps>

namespace go.@internal;

[GoPackage("sync_test")]
public static partial class sync_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("cleanupArg")] internal partial struct TestConcurrentCache_cleanupArg {}
    [GoLocalName("dummy")] internal partial struct TestConcurrentCache_dummy {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸabi() => builtin.initPackage(typeof(go.@internal.abi_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsync() => builtin.initPackage(typeof(go.@internal.sync_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(go.sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸweak() => builtin.initPackage(typeof(weak_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.@internal.sync_package));
    }
}
