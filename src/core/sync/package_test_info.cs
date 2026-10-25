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
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<go.@internal.sync_package.HashTrieMap<any, any>, mapInterface>(Pointer = true)]
[assembly: GoImplement<sync_package.Map, mapInterface>(Pointer = true)]
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
[assembly: go.GoPositionMap("sync/cond_test.go", "cond_test.cs", "ABQcgoKCgoKCgrKCgoKCpoKUgqTWgoKCgqTWlKaCgoKCgoKCsoKCgoKUgoKCpoKCgviCgoKCgoKCgrKCgoKClKaCgpSCgoKUpNaCgoKCgoKClKak1uaCgoKCooKCgoKUgoKClKKCgoKCgpSCgpSClKKCgoKCgpSUgpSCgpSClIKCpoKCgqaCooKCgpSmgoIAChaCkqaSgoKUuLiCgoK4ooKCgqaSgoKCoqSCpoKmgqaCpoKmgqaCgoKUgqKCgoKClIKCgpSUlIKCgoKmgg==", "21-27:1;59-65:1;87-95:1;135-146:1;147-161:2;162-181:3;194-201:1;217-219:2;221-227:3;242-247:1;285-306:1")]
[assembly: go.GoPositionMap("sync/example_pool_test.go", "example_pool_test.cs", "AA4eysqSpoKClIKCgoKCguaC")]
[assembly: go.GoPositionMap("sync/example_test.go", "example_test.cs", "ABEezKKCypSUuKqigsqUlLSUorbWgoKClIKCooKmggAKEKKCgoKUgpSCgqKCgoKUpoIACg6SgoKUgoKigoKUgqaC", "30-33:1;52-57:1;65-67:1;70-73:2;85-92:1;95-102:2;113-116:1;119-126:2")]
[assembly: go.GoPositionMap("sync/map_bench_test.go", "map_bench_test.cs", "ABUqgqKigoKmgpSCooLcgqSUgqaCyoLcgqSUgqaCyoIACAyCpJSAgqSCpoLKgoKCgIK2gIIACBCClICC2oLcgpS4gtyCpJSAgqSCpoLKgoKClO6ClICC2oLcgpS4goCC/oKUlILKggAFFtKUkoKCgIKCggAHGNKUlILKgpSCgoKU7oKUuILcgpS4gtyCpJSCpoLKgoKClILugqSUgqaCyoKCgpSC7oKUuIKC7oKUgoLugpS4gtyCpJSAgraCpoLKgoKClNyCpJSCpoLKgoKClNyClLiCgu6CpJSAgraCpoLKgoKClILugqSUgqaCyoKCgpSC7oKUgpKC", "23-37:1;33-36:1.1;45-53:1;55-59:2;67-75:1;77-81:2;89-100:1;102-115:2;121-125:1;127-131:2;137-139:1;141-145:2;153-164:1;166-175:2;181-185:1;187-191:2;197-199:1;201-207:2;215-219:1;221-225:2;223-223:2.1;236-246:1;259-263:1;265-277:2;270-273:2.1;283-285:1;287-291:2;297-299:1;301-305:2;313-321:1;323-333:2;341-349:1;351-361:2;367-369:1;371-377:2;383-389:1;395-397:1;399-403:2;411-423:1;425-433:2;441-449:1;451-459:2;465-467:1;469-475:2;483-495:1;497-507:2;515-523:1;525-535:2;541-547:1")]
[assembly: go.GoPositionMap("sync/map_reference_test.go", "map_reference_test.cs", "AB08goIABxLCgoKCpqKCgpSCpsKCgoKCgpSUgqbCgoKmgoKCpsKCgoKClIKCpqKCgqbSgoKCpoKCgpTm0oKCgqaCgoKU5rKCgoKUlIKCgpSCytKClKIAChSygoKmooKCgoKmwoKCgqaUgoKCgoKClIKmwoKCgoKCgqbCgoKCgoKCpqKCgoKCptKCgIK2goKCgoKCgpTm0oKAgraClIKCgoKClOaCgoKCyoKCgoKUptKClKI=")]
[assembly: go.GoPositionMap("sync/map_test.go", "map_test.cs", "ACRCABEkgpSkgqSkpIKkpIKClKSCgIK2pIKkAAgSooKClKaCgpSUpsKCgqaCkoKmtoKmgqaCpoKmgoCCyIKAgsiCgILIopSCgqaCgpKClIKCgtKCgqTGgoKUAAkOgoKUgpSSgoKUgpSCpoLGpILKlLiCgoCSgoL4koKClIKmgILsgIL+goKAgqS4gsqCgoKmgriCgpS4koKCgpiCvKKUkqaCooKiyIKigoCClMTIgqKCoraUlIKUuIKCgoKUgg==", "117-120:1;169-172:1;176-192:2;202-212:3;234-234:1;246-276:1;247-271:1.1;281-284:2;302-306:1;303-305:1.1;322-325:1;330-337:2;342-345:3;352-356:4;362-364:1")]
[assembly: go.GoPositionMap("sync/mutex_test.go", "mutex_test.cs", "ABgqgoKClKaCgoKCgpSCuKKCgqbCgoKCgoKUgoKCoqSCgoKClJSClKaigIKklJSCgpSCgpSUgoKUgrTkvoLcgoKC3ILcgoLcgoKC3ILcgoLcgoKCyoKCsoKygJKikoKmgviCgoKCysKCgoKSgoKCgqTqgpKCgoKUlObE9IKKgoKCyqKCgpSCgoKCgoKCgri4gqaCpoKmgqau4pKCgoKCgoKCgoKUgriSlNyKwpKCgoKCgoKCgg==", "176-179:1;177-177:1.1;202-213:1;215-222:2;235-241:1;249-262:1;291-312:1;306-308:1.1;322-333:1")]
[assembly: go.GoPositionMap("sync/once_test.go", "once_test.cs", "ABAcgqaygJKAgqSmgoKCgoKClIKUgriCgqKCgIK2grSkgriCgpKSgg==", "19-19:1;44-53:1;45-49:1.1;50-52:1.2;55-57:2;62-62:1;63-67:2")]
[assembly: go.GoPositionMap("sync/oncefunc_test.go", "oncefunc_test.cs", "ABUokoKAkpKCgpSClJKUgriCgoKClJKAkoKClIKUgpSSlIK4goKCgpSSgJKCgpSClIKUkpSCuIKCgsrGgoKCooKUgqKSgpSUgriCkoKClKaCkoKClJC2gpKCgpSQtoKSgoKUgqSUuKaCgoKUgoKCsoKAkqKSlILoxpSCgIKkgoKCpqKkguaC1JC2kICSkLaQgJKQtpCAtpCAkpC2kICSkLaCkoKCgJKCgoKUsoCSopKCppSygJKi3JKCqMb+ggAJBoKCgpSmgoK4poK4koKmgoKC3AAHEIKClKaUgoKCgJLIgoKCgJLIgoKCgoCSyIKCggAHFgAIFoKCgpSmlIKCgoCCkoLIgoKCgIKSgsiCgoaCgIKSgsiCgoI=", "22-22:1;31-33:2;41-44:1;46-46:2;57-59:3;67-70:1;72-72:2;83-85:3;92-96:1;105-111:1;106-108:1.1;124-127:1;133-136:1;137-137:2;142-145:1;146-146:2;151-154:1;155-161:2;168-171:1;175-179:2;177-177:2.1;192-201:1;211-213:1;212-212:1.1;214-217:2;215-215:2.1;216-216:2.2;218-221:3;219-219:3.1;220-220:3.2;222-224:4;223-223:4.1;225-228:5;226-226:5.1;227-227:5.2;229-232:6;230-230:6.1;231-231:6.2;235-258:7;238-238:7.1;244-247:7.2;245-245:7.2.1;254-257:7.3;255-255:7.3.1;280-280:1;284-290:1;291-299:2;300-309:3;305-305:3.1;310-315:4;313-313:4.1;328-330:1;336-343:1;344-351:2;352-360:3;354-354:3.1;361-366:4;364-364:4.1;387-390:1;396-405:1;406-415:2;416-428:3;418-420:3.1;429-436:4;432-434:4.1")]
[assembly: go.GoPositionMap("sync/pool_test.go", "pool_test.cs", "ABsqtIKCgsqCgoKAgqSAgqSAgqS4gqaCgIK2goCCxKS0lIKkgqaAgqSAgtqCgoCCpJSAgsSmkqiSpqIACBKmgoKCgpSSgoKAkpSCgri4gqaAgtakgoKCgpSCgoKSgoKClIKCgoKmpoK4gqaCpoKCgoKUgoKCkoKCyoKCkoKCgoKCuICCyMqCgpKClJSCgoKCuJSmgoIABxCCAAkIgqKCtKSCwpKClKKSwpKCoraCgoKCgsqCgoKCgpSCAAUQooKmgoKCgpSCAAwQ5JSClIKEkoKCppSCuIKCgqaCgqLkAAkMgoCmkoKCgoKUkoKmgoKUgqaUgoKmlJSCog==", "68-71:1;126-126:1;158-172:1;196-201:1;206-222:2;228-243:3;263-267:1;270-276:2;277-281:3;286-291:1;296-305:1;316-325:1;371-371:1;376-380:2;383-402:3")]
[assembly: go.GoPositionMap("sync/runtime_sema_test.go", "runtime_sema_test.cs", "ABMagoqCgoLKwoKUkoKCkoKUlJKmgoKCgoKCgqaUgrSkgqaCpoKmgg==", "18-24:1;34-39:1;40-42:2;44-58:3")]
[assembly: go.GoPositionMap("sync/rwmutex_test.go", "rwmutex_test.cs", "ABMoooKCgoKmgoKCgoKCgqaClIKmgriigoKCoqSCgoKCgoKUlIKUpoKCgoKCgpSUgpSmgoSSgoKCgoKUgoKmgriilIKClIKUlIKUlIKUgpSClIKUgoKClIKCgoKCgoKCgqLkgoKCgoKCgrKCgoKCgqaCgoKk1oKCpNYACAiCioKCgoKCgoLKgoKCgoKCgoKUgoKClKa4gqaCpoKmgg==", "163-171:1;196-206:1;211-228:1")]
[assembly: go.GoPositionMap("sync/waitgroup_test.go", "waitgroup_test.cs", "ABokgoKCgoKCkoKCpoKCpNaUgriCgqaC6KKCgoKmgoKCgqLUlIKClIKigqaCooKmgoIACQqCioKClKaCgoKClIKCAAgMooKmgoKCgoKCgpSUgoKUgvaCioKCgoLKgoKCgoKCgoKUlLiCpoKmgoKCgoKCgoKmuIKmgqaigoKCgoKClA==", "24-28:1;55-60:1;75-78:1;81-84:2;100-102:1;109-111:1;137-139:1;149-156:1;161-172:1;185-195:1;208-217:1;212-214:1.1")]
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
