// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.sync_package;
global using static global::go.sync_internal_test_package;

// <ImportedTypeAliases>
global using execꓸError = go.os.exec_package.ΔError;
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
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static global::go.sync_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20662066756e6328297d", "misuseTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<DeepCopyMap, mapInterface>(Pointer = true)]
[assembly: GoImplement<RWMutexMap, mapInterface>(Pointer = true)]
[assembly: GoImplement<go.@internal.sync_package.HashTrieMap<any, any>, mapInterface>(Pointer = true)]
[assembly: GoImplement<sync_package.Map, mapInterface>(Pointer = true)]
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
[assembly: go.GoPositionMap("sync/cond_test.go", "cond_test.cs", "ABQcgoKCgoKCgrKCgoKCpoKUgqTWgoKCgqTWlKaCgoKCgoKCsoKCgoKUgoKCpoKCgviCgoKCgoKCgrKCgoKClKaCgpSCgoKUpNaCgoKCgoKClKak1uaCgoKCooKCgoKUgoKClKKCgoKCgpSCgpSClKKCgoKCgpSUgpSCgpSClIKCpqKCgpaCooKCgoSWgoIACRaCkpaSgoKUqKiCgoK4ooKCgqaSgoKCoqSCpoKmgqaCpoKmgqaCgoKEgqKCgoKClIKCgpSUlIKCgoKmgg==", "21-27:1;59-65:1;87-95:1;135-146:1;147-161:2;162-181:3;194-201:1;217-219:2;221-227:3;242-247:1;285-306:1")]
[assembly: go.GoPositionMap("sync/example_pool_test.go", "example_pool_test.cs", "ABEeAAQUkqaCgpSCgoKCgoLmgg==")]
[assembly: go.GoPositionMap("sync/example_test.go", "example_test.cs", "ABEezKKCypSUtJSittaCgoKUgoKigqaCAAgQ0oKCgpSClIKCooKCgpSmggAIDsKCgpSCgqKCgpSCpoI=", "32-37:1;45-47:1;50-53:2;65-72:1;75-82:2;93-96:1;99-106:2")]
[assembly: go.GoPositionMap("sync/map_bench_test.go", "map_bench_test.cs", "ABUqgqKigoKWgoSCooLcgpSUgqaCuoLcgpSUgqaCuoIACAyClJSAgqSCpoK6goKCgIK2gIIACBCClICCyoLcgpSogtyClJSAgqSCpoK6goKClO6ClICCyoLcgpSogoCC/oKElIK6ggAFFtKUkoKCgIKCggAHGNKElIK6goSCgoKU7oKUqILcgpSogtyClJSCpoK6goKClILugpSUgqaCuoKCgpSC7oKUqIKC7oKUgoLugpSogtyClJSAgqaCpoK6goKClNyClJSCpoK6goKClNyClKiCgu6ClJSAgqaCpoK6goKClILugpSUgqaCuoKCgpSC7oKUgpKC", "23-37:1;33-36:1.1;45-53:1;55-59:2;67-75:1;77-81:2;89-100:1;102-115:2;121-125:1;127-131:2;137-139:1;141-145:2;153-164:1;166-175:2;181-185:1;187-191:2;197-199:1;201-207:2;215-219:1;221-225:2;223-223:2.1;236-246:1;259-263:1;265-277:2;270-273:2.1;283-285:1;287-291:2;297-299:1;301-305:2;313-321:1;323-333:2;341-349:1;351-361:2;367-369:1;371-377:2;383-389:1;395-397:1;399-403:2;411-423:1;425-433:2;441-449:1;451-459:2;465-467:1;469-475:2;483-495:1;497-507:2;515-523:1;525-535:2;541-547:1")]
[assembly: go.GoPositionMap("sync/map_reference_test.go", "map_reference_test.cs", "AB08goIABxLCgoKCpqKCgpSCpsKCgoKCgpSUgqbCgoKWgoKCpsKCgoKClIKCpqKCgqbSgoKCloKCgpTm0oKCgpaCgoKU5qKCgoKUhIKCgpSCytKChKIAChSygoKmooKCgoKmwoKCgpaUgoKCgoKClIKmwoKCgoKCgqbCgoKCgoKCpqKCgoKCptKCgIKmgoKCgoKCgpTm0oKAgqaChIKCgoKClOaCgoKCyoKCgoKUptKChKI=")]
[assembly: go.GoPositionMap("sync/map_test.go", "map_test.cs", "ACRCABEkgpSkgqSkpIKkpIKClKSCgIK2pIKkAAkSooKClKaCgpSkprKCgpaCkoKWpoKmgqaCpoKmgoCCyIKAgsiCgILIooSCgpaCgpKClIKCgtKCgqTGgoKUAAgOgoKUgoSSgoKUgpSCloLGpIK6hKiCgoKUgoL4soKClIKmgILcgILugoKAgqSogrqCgoKWgriCgpS4koKCgpiCvKKEkpaCooKiuIKigoCClMS4gqKCoqaEhIKEuIKCgoKUgg==", "117-120:1;169-172:1;176-192:2;202-212:3;234-236:1;248-278:1;249-273:1.1;283-286:2;304-308:1;305-307:1.1;324-327:1;332-339:2;344-347:3;354-358:4;364-366:1")]
[assembly: go.GoPositionMap("sync/mutex_test.go", "mutex_test.cs", "ABgqgoKClKaCgoKCgpSCuKKCgqbCgoKCgoKEgoKCoqSCgoKClJSClKaigIKkhISCgpSCgpSEgoKUgrT0voLcgoKC3ILcgoLcgoKC3ILcgoLcgoKCyoKCsoKygJKikoKmgviCgoKCgsrCgoKCkoKCgoKk6oKSgoKClJTmxPSCioKCgsqigoKUgoKCgoKCgoK4uIKmgqaCpoKmruKSgoKCgoKCgoKClIK4kpTcisKSgoKCgoKCgoI=", "176-179:1;177-177:1.1;203-214:1;216-223:2;236-242:1;250-263:1;292-313:1;307-309:1.1;323-334:1")]
[assembly: go.GoPositionMap("sync/once_test.go", "once_test.cs", "ABAcgqaygJKAgqSmgoKCgoKClIKUgriCgqKCgIK2grSUgriCgpKSgg==", "19-19:1;44-53:1;45-49:1.1;50-52:1.2;55-57:2;62-62:1;63-67:2")]
[assembly: go.GoPositionMap("sync/oncefunc_test.go", "oncefunc_test.cs", "ABUokoKAkoKClIK4goKCgpSQkoKClIKUgriCgoKClJCSgoKUgpSCuIKCgsrGgoKCooKUgqKSgpSUgriCkoKClKaCkoKClJC2gpKCgpSQtoKSgoKUgqSkuMaCgoKUgoKCsoKAkqKSlILoxoSCgIKkgoKCpqKkguaCpJC2kICSkLaQgJKQtoKSgoKClIKCgpSCgqaUzJKCqMbKggAIBoKCgpSmgoK4poK4koLMzIKClKaUgoKCgJLIgoKCgJLIgoKCgoCS", "22-22:1;34-37:1;38-38:2;53-56:1;57-57:2;71-75:1;84-90:1;85-87:1.1;103-106:1;112-115:1;116-116:2;121-124:1;125-125:2;130-133:1;134-140:2;147-150:1;154-158:2;156-156:2.1;171-180:1;190-192:1;191-191:1.1;193-196:2;194-194:2.1;195-195:2.2;197-200:3;198-198:3.1;199-199:3.2;203-222:4;206-208:4.1;242-242:1;246-252:1;253-261:2;262-271:3;267-267:3.1;282-284:1;290-297:1;298-305:2;306-314:3;308-308:3.1")]
[assembly: go.GoPositionMap("sync/pool_test.go", "pool_test.cs", "ABsq1IKCgrqCgoKAgqSAgqSAgqSogqaCgIK2goCCxKTUhIKkgqaAgqSAgsqCgoCCpISAgsSmkqiSpoKCgoKCgpSSgoKClJSCgqaCgpSAgrbYgoKCgpSCgoKSgoKClIKCgoKmpoK4gqaCpoKCgoKUgoKCkoKCuoKCkoKCgoKCuICCyLqCgpKClJSCgoKCuJSWgoIABxCCAAkIgqKCtJSCwpKClKKSwpKCoraCgoKCgsqCgoKCgpSCAAUQooKmgoKCgpSCAAwQ1ISChIKEkoKCppSCqIKCgqaCgqLkAAgMgoCWkoKCgoKUkoKmgoKUgqaUgoKmlISCog==", "68-71:1;115-117:1;146-160:1;184-189:1;194-210:2;216-231:3;251-255:1;258-264:2;265-269:3;274-279:1;284-293:1;304-313:1;359-359:1;364-368:2;371-390:3")]
[assembly: go.GoPositionMap("sync/runtime_sema_test.go", "runtime_sema_test.cs", "ABMagoqCgoLKwoKUkoKCkoKUlJKmgoKCgoKCgqaUgrSkgqaCpoKmgg==", "18-24:1;34-39:1;40-42:2;44-58:3")]
[assembly: go.GoPositionMap("sync/rwmutex_test.go", "rwmutex_test.cs", "ABMoooKCgoKmgoKCgoKCgqaClIKmgriigoKCoqSCgoKCgoKUlIKUpoKCgoKCgpSUgpSmgoSSgoKCgoKUgoKmgriihIKClIKUhIKUhIKUgpSClIKEgoKClIKCgoKCgoKCgqLkgoKCgoKCgrKCgoKCgqaCgoKk1oKCpNYACAiCioKCgoKCgoLKgoKCgoKCgoKUgoKClKa4gqaCpoKmgg==", "163-171:1;196-206:1;211-228:1")]
[assembly: go.GoPositionMap("sync/waitgroup_test.go", "waitgroup_test.cs", "ABMagoKCgoKCkoKCpoKCpNaUgriCgpaC6KKCgoKmgoKCgqLUlIKClIKigqaCooKmgoIACQqCioKClPaCioKCgoLKgoKCgoKCgoKUlLiCpoKmgoKCgoKCgoKmuIKmgqaigoKCgoKClA==", "19-23:1;50-55:1;70-73:1;76-79:2;95-97:1;106-113:1;118-129:1;142-152:1;165-174:1;169-171:1.1")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("sync_test")]
public static partial class sync_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface mapInterface {}
    [GoLocalName("PaddedMutex")] [GoValueClone("pad")] internal partial struct BenchmarkMutexUncontended_PaddedMutex {}
    [GoLocalName("PaddedRWMutex")] [GoValueClone("pad")] internal partial struct BenchmarkRWMutexUncontended_PaddedRWMutex {}
    [GoLocalName("PaddedSem")] [GoValueClone("pad")] internal partial struct BenchmarkSemaUncontended_PaddedSem {}
    [GoLocalName("PaddedWaitGroup")] [GoValueClone("pad")] internal partial struct BenchmarkWaitGroupUncontended_PaddedWaitGroup {}
    [GoLocalName("X")] internal partial struct TestWaitGroupAlign_X {}
    internal partial struct bench {}
    internal partial struct httpPkg {}
    internal partial struct mapCall {}
    internal partial struct mapOp {}
    internal partial struct mapResult {}
    internal partial struct misuseTestsᴛ1 {}
    internal partial struct one {}
    public partial struct DeepCopyMap {}
    public partial struct RWMutexMap {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsync() => builtin.initPackage(typeof(@internal.sync_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(go.math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸruntimeꓸdebug() => builtin.initPackage(typeof(go.runtime.debug_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸquick() => builtin.initPackage(typeof(go.testing.quick_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.sync_package));
    }
}
