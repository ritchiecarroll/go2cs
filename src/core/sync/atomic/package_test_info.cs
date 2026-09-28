// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.sync.atomic_package;

// <ImportedTypeAliases>
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
using static global::go.sync.atomic_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b696e697420616e793b206e657720616e793b206f6c6420616e793b2077616e7420626f6f6c3b2065727220616e797d", "Value_CompareAndSwapTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e697420616e793b206e657720616e793b2077616e7420616e793b2065727220616e797d", "Value_SwapTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b75696e747d", "heapAᴛ1")]
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
[assembly: go.GoPositionMap("sync/atomic/atomic_test.go", "atomic_test.cs", "ACdGgoqCgoKCgoKUlIIACQiCioKCgoKCgpSUggAJCIKKgoKCgoKClJSCAAkIgoqCgoKCgoKUlIIACQiCioKCgoKCgoKUlIIACQiCioKCgoKCgoKUlIIACQiCioKCgoKCgoKUlIIACQiCioKCgoKCgoKUlIIACQiCioKCgoKCgoKClJSCAAkIgoqCgoKCgoKCgpSUguyClIKmlIIACAaCioKCgoKEgoKClJSCAAkIgoqCgoKCgoKCgoKUlIIACQiCioKCgoKCgoKmggAJCIKKgoKCgoKCgqaCAAkIgoqCgoKCgoKCpoIACQiCioKCgoKCgoKmggAJCIKKgoKCgoKCgoKmggAJCIKKgoKCgoKCgoKmggAJCIKKgoKCgoKCgoKmggAJCIKKgoKCgoKCgoKmggAJCIKKgoKCgoKCgoKCpoIACQiCioKCgoKCgoKCgqaCAAkIgoqCgoKCgoKCgoKmggAJCIKKgoKCgoKCgoKCpoIACQiCioKCgoKCgoKCgqaCAAkIgoqCgoKCgoKCgoKmggAJCIKKgoKCgoKCgoKCgqaCAAkIgoqCgoKCgoKCgoKCpoIACQiCioKCgoKCgoKCgoKmggAJCIKKgoKCgoKCgoKCgqaCAAkIgoqCgoKCgoKCgoKCgqaCAAkIgoqCgoKCgoKCgoKCgqaCAAkIgoqCgoKCgoKCgqaCAAkIgoqCgoKCgoKCgqaCAAkIgoqCgoKCgoKCgqaCAAkIgoqCgoKCgoKCgqaCAAkIgoqCgoKCgoKCgoKmggAJCIKKgoKCgoKCgoKCpoIACQiCioKCgoKCgoKCgqaCAAkIgoqCgoKCgoKCgoKmggAJCIKKgoKCgoKCgoKCgqaCAAkIgoqCgoKCgoKCgoKCpoIACQiCioKCgoKClIKUgoKUgqaCAAkIgoqCgoKCgpSClIKClIKmggAJCIKKgoKCgoKUgpSCgpSCpoIACQiCioKCgoKClIKUgoKUgqaCAAkIgoqCgoKCgoKUgpSCgpSCpoIACQiCioKCgoKCgpSClIKClIKmggAJCIKKgoKCgoKClIKUgoKUgqaCuIIACAaCioKCgoKCgpSClIKClIKmggAJCIKKgoKCgoKCgpSClIKClIKmggAJCIKKgoKCgoKCgpSClIKClIKmggAJCIKKgoKCgoKCgoKUgpSClIKmggAJCIKKgoKCgoKCgoKClIKUgpSCpoIACQiCioKCgoKClJSCAAkIgoqCgoKCgoKUgpSCAAkIgoqCgoKCgpSUggAJCIKKgoKCgoKClIKUggAJCIKKgoKCgoKClJSCAAkIgoqCgoKCgoKClIKUggAJCIKKgoKCgoKClJSCAAkIgoqCgoKCgoKClIKUggAJCIKKgoKCgoKCgpSUggAJCIKKgoKCgoKCgoKUgpSCAAkIgoqCgoKCgoKCgqaCAAkIgoqCgoKCgoKCgoKmggAJCIKKgoKCgoKClJSCAAkIgoqCgoKCgoKUlIIACQiCioKCgoKCgpSUggAJCIKKgoKCgoKClJSCAAkIgoqCgoKCgoKClJSCAAkIgoqCgoKCgoKClJSCAAkIgoqCgoKCgoKClJSCAAkIgoqCgoKCgoKClJSCAAkIgoqCgoKCgoKCgpSUggAJCIKKgoKCgoKCgoKUlIIACQiCioKCgoKCgoKmggAJCIKKgoKCgoKCgoKmggAjTIKClIKCgoKCuKKCgoKCgoLKooKCgoKCgsqigoKCgoLKooKCgoKCgsrGgoKCgoKCysaCgoKCgoLKgoKCuIKCgriCgriCgoK4poKCuKaCgriCgoKCgoLcgoKCgoKC3IKCgoKC3IKCgoKCgtymgoKCgoLcpoKCgoKCAAgMooKCgpSEgoKCgsKSgIKklNaClIIAHDaCgpSCgoKCgriigoKCgoKCyqKCgoKCgoLKooKCgoKCyqKCgoKCgoLuxoKCgoKCgoLcxoKCgoKCgoLcgoKCuIKCgriCgriCgoK4poKCuKaCgriCgoKCgoLcgoKCgoKC3IKCgoKC3IKCgoKCgtymgoKCgoLcpoKCgoKC3KKCgoKUhIKCgoLCkoCCpJTWgpSC+oKCgoKCgpSCgpSmgoKCgoKClIKClKaCgoKCgoKUgoKUpoKCgoKCgpSCgpSmgoKCgoKClIKmgoKCgoKClIKmgoKCgoKClIKmgoKCgoKClIKmgoKCgoKCgoKUgoKmgoKClIKUqJKCgoKCgoKClIKCpoKCgpSClK7CgoKCgoKCgpSCgqaCgoKUgpSuwoKCgoKCgoKUgoKmgoKClIKUpsIABxCCgpSCppSCgoKCgqKClKaC+qKClIKCgpSCkpKCkoKCgoKCgoKmgoKClIKClJSmgtaigpSCgoKUgpKSgpKCgoKCgoKCpoKCgpSCgpSUpoIADQaigpSCgoKUggAAEIKSgoKCgpSCgqaCgoKCuKaCAA0GooKUgoKClIIAABCCkoKCgoKUgoKmgoKCgrimggAIBqKUhIKCgqKCtgAPBriCloKEkJKQkpCSkJKQkpCSkJKQAA0GgoiAgqSAgqaIgIKkgILIgpKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKkgrKCgpQ=", "2143-2151:1;2144-2149:1.1;2410-2418:1;2411-2416:1.1;2674-2679:1;2700-2723:1;2742-2765:1;2790-2811:1;2836-2857:1;2864-2875:1;2890-2890:1;2891-2891:2;2892-2892:3;2893-2893:4;2894-2894:5;2895-2895:6;2896-2896:7;2897-2897:8;2926-2926:1;2927-2927:2;2928-2928:3;2929-2929:4;2930-2930:5;2931-2931:6;2932-2932:7;2933-2933:8;2934-2934:9;2935-2935:10;2936-2936:11;2937-2937:12;2938-2938:13;2939-2939:14;2940-2940:15;2941-2941:16;2942-2942:17;2943-2943:18;2944-2944:19;2945-2945:20;2946-2946:21;2947-2947:22;2948-2948:23;2949-2949:24;2950-2950:25;2951-2951:26;2952-2952:27;2953-2953:28;2954-2954:29;2955-2955:30;2956-2956:31;2957-2957:32;2958-2958:33;2959-2959:34;2960-2960:35;2961-2961:36;2962-2962:37;2963-2963:38;2964-2964:39;2965-2965:40;2966-2966:41;2967-2967:42;2968-2968:43;2969-2969:44;2970-2970:45;2971-2971:46;2972-2972:47;2973-2973:48;2974-2974:49;2975-2975:50;2976-2976:51;2977-2977:52;2978-2978:53;2979-2979:54;2980-2980:55;2981-2981:56;2982-2982:57;2983-2983:58;2986-2992:59;2987-2990:59.1")]
[assembly: go.GoPositionMap("sync/atomic/example_test.go", "example_test.cs", "AAwagqaCqqKUgqaCgsqCgoKUAAgQooSClIKCpqKCgoKCgpSC+g==", "27-34:1;38-44:2;56-59:1;61-74:2")]
[assembly: go.GoPositionMap("sync/atomic/value_test.go", "value_test.cs", "ABQigoKClIKCgIKkgoKAggAICIKCgoKAgqSCgoCCAAoIgoKCgqKCgoKmxIKigoKCpsSigoKCpuiC3IKCgoKUgoKCgqKCgoKCgoKCgoKmgoK0poKC3IKCgoKCgoIAEySCsrKCgpSSgpS0xoCCpICCAAkMgoKCgpKCgpSCgoKCgoKAgraCpoKAggAeLoKysoKClJKClLTGgIIACQyCgoKCkoKClIKCgoKCgqamgoCC", "52-60:1;53-58:1.1;62-70:2;63-68:2.1;71-79:3;72-77:3.1;99-117:1;130-137:1;154-174:1;159-167:1.1;190-199:1;228-245:1;233-241:1.1;261-268:1")]
// </GoSourcePositionMaps>

namespace go.sync;

[GoPackage("atomic_test")]
public static partial class atomic_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct ExampleValue_readMostly_Map {}
    internal partial struct TestAddInt32Method_x {}
    internal partial struct TestAddInt32_x {}
    internal partial struct TestAddInt64Method_x {}
    internal partial struct TestAddInt64_x {}
    internal partial struct TestAddUint32Method_x {}
    internal partial struct TestAddUint32_x {}
    internal partial struct TestAddUint64Method_x {}
    internal partial struct TestAddUint64_x {}
    internal partial struct TestAddUintptrMethod_x {}
    internal partial struct TestAddUintptr_x {}
    internal partial struct TestAndInt32Method_x {}
    internal partial struct TestAndInt32_x {}
    internal partial struct TestAndInt64Method_x {}
    internal partial struct TestAndInt64_x {}
    internal partial struct TestAndUint32Method_x {}
    internal partial struct TestAndUint32_x {}
    internal partial struct TestAndUint64Method_x {}
    internal partial struct TestAndUint64_x {}
    internal partial struct TestAndUintptrMethod_x {}
    internal partial struct TestAndUintptr_x {}
    internal partial struct TestAutoAligned64_signed {}
    internal partial struct TestAutoAligned64_unsigned {}
    internal partial struct TestCompareAndSwapInt32Method_x {}
    internal partial struct TestCompareAndSwapInt32_x {}
    internal partial struct TestCompareAndSwapInt64Method_x {}
    internal partial struct TestCompareAndSwapInt64_x {}
    internal partial struct TestCompareAndSwapPointerMethod_x {}
    internal partial struct TestCompareAndSwapPointer_x {}
    internal partial struct TestCompareAndSwapUint32Method_x {}
    internal partial struct TestCompareAndSwapUint32_x {}
    internal partial struct TestCompareAndSwapUint64Method_x {}
    internal partial struct TestCompareAndSwapUintptrMethod_x {}
    internal partial struct TestCompareAndSwapUintptr_x {}
    internal partial struct TestLoadInt32Method_x {}
    internal partial struct TestLoadInt32_x {}
    internal partial struct TestLoadInt64Method_x {}
    internal partial struct TestLoadInt64_x {}
    internal partial struct TestLoadPointerMethod_x {}
    internal partial struct TestLoadPointer_x {}
    internal partial struct TestLoadUint32Method_x {}
    internal partial struct TestLoadUint32_x {}
    internal partial struct TestLoadUint64Method_x {}
    internal partial struct TestLoadUint64_x {}
    internal partial struct TestLoadUintptrMethod_x {}
    internal partial struct TestLoadUintptr_x {}
    internal partial struct TestOrInt32Method_x {}
    internal partial struct TestOrInt32_x {}
    internal partial struct TestOrInt64Method_x {}
    internal partial struct TestOrInt64_x {}
    internal partial struct TestOrUint32Method_x {}
    internal partial struct TestOrUint32_x {}
    internal partial struct TestOrUint64Method_x {}
    internal partial struct TestOrUint64_x {}
    internal partial struct TestOrUintptrMethod_x {}
    internal partial struct TestOrUintptr_x {}
    internal partial struct TestStoreInt32Method_x {}
    internal partial struct TestStoreInt32_x {}
    internal partial struct TestStoreInt64Method_x {}
    internal partial struct TestStoreInt64_x {}
    [GoLocalName("Data")] [GoValueClone("pad1", "pad2")] internal partial struct TestStoreLoadRelAcq32_Data {}
    [GoLocalName("Data")] [GoValueClone("pad1", "pad2")] internal partial struct TestStoreLoadRelAcq64_Data {}
    internal partial struct TestStorePointerMethod_x {}
    internal partial struct TestStorePointer_x {}
    internal partial struct TestStoreUint32Method_x {}
    internal partial struct TestStoreUint32_x {}
    internal partial struct TestStoreUint64Method_x {}
    internal partial struct TestStoreUint64_x {}
    internal partial struct TestStoreUintptrMethod_x {}
    internal partial struct TestStoreUintptr_x {}
    internal partial struct TestSwapInt32Method_x {}
    internal partial struct TestSwapInt32_x {}
    internal partial struct TestSwapInt64Method_x {}
    internal partial struct TestSwapInt64_x {}
    internal partial struct TestSwapPointerMethod_x {}
    internal partial struct TestSwapPointer_x {}
    internal partial struct TestSwapUint32Method_x {}
    internal partial struct TestSwapUint32_x {}
    internal partial struct TestSwapUint64Method_x {}
    internal partial struct TestSwapUint64_x {}
    internal partial struct TestSwapUintptrMethod_x {}
    internal partial struct TestSwapUintptr_x {}
    internal partial struct heapAᴛ1 {}
    internal partial struct testCompareAndSwapUint64_x {}
    public partial struct List {}
    public partial struct Value_CompareAndSwapTestsᴛ1 {}
    public partial struct Value_SwapTestsᴛ1 {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸruntimeꓸdebug() => builtin.initPackage(typeof(go.runtime.debug_package));
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
        builtin.initPackage(typeof(global::go.sync.atomic_package));
    }
}
