// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.path.filepath_package;
global using static global::go.path.filepath_internal_test_package;

// <ImportedTypeAliases>
global using execꓸError = go.os.exec_package.ΔError;
global using flagꓸErrorHandling = go.flag_package.ΔErrorHandling;
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
// </ImportedTypeAliases>

using go;
using static global::go.path.filepath_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b7061746820737472696e673b206465737420737472696e673b2062726f6b656e4c696e6b20626f6f6c7d", "globSymlinkTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7061747465726e20737472696e673b20726573756c7420737472696e677d", "globTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
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
[assembly: go.GoPositionMap("path/filepath/match_test.go", "match_test.cs", "ABkwAD12goKUpoKCgoKClJSClIKCAAgKAAYSgoKCgoKClIKCgpSCpoKCgoKUgsq4goK4goKCgILapvYABBCChIKCgoKCgpSAgqSCgpSUlIKClIIACRSCgoKUgqaCgoKClIKCgpSmgoKCgpSCgoKmgoKUpoKCloKClIKWyqaCgoKmgoKCqAAMHKKCgIK2goKAgraCgoKCgILKgqKCgpSCgpSCgvqCgpSCgoKClII=")]
[assembly: go.GoPositionMap("path/filepath/path_test.go", "path_test.cs", "ACM0ACZm/gAoUoKCgoKUlJSCgIKkgIK4gpSCgpaykJKCAAkUABAgACZOuIKCgpSClIKAggAKFAAOHsrKAA4YgoKUpIKCtqSCgoKClILu7oKCgIKkgIIADBgACAwAECyCgoKUgoCCAAkSAAcQAAwagoKCgpSCgIIADhQAEza4ACFEgoKUlIKCgIIACRIABxCCgoCCAAsWABc0ooKCuIKCgoKCgpSUyoCAtIKCgpS+0oKCgqaCgoKUlKqihIKClqaCiqaC9oKEgpKCmIKClIKUgoTagpSClIKogqiClIKCgoKUgqaCqIKUgoKCgoKUgqaCloK4goKClICCAA0IgoSAgqSChIKCgpSClJSSlIKCgoKClIKoopKClKKSggALCIKEgIKkgIKmgoKCgoKEgoKCloiUlpKUgoKCgIKkgqiikoKUopKCAAkIooSCgoKAgqSCgoKUgpKClJSCkoKClIKUAAEQgrQADgSChIKCgIKkhIKAgqaCgIKmgoCCAA0cACFMkpKCgoKUgoKUgpaCgoKU7gANHAAKFoKClIKmlIKAgtoADRy4AAwagoKUgqaUlIKAggAKFAAKFgASJoKCgpSCpoKmloKAggALFAATKgAOIqKmgoKCgpSCuIKCgoKClIL4goSIooKCqIKCgoKUlIK6goSCgpSWloKCqIKClMzoooKEgoKWgoKUhIKCtAAMBIKEhIKCgpSCgoKUgoKClIKCgpSCgoKogoKUgoQABhKCgoKUgsyyloLMgIKkhIKClIKClIKUygAIEgASHqKChIKCgsyCgoKCooKUgpSWgoKWooKCgoKWgoKClIKClIKUggAEENKChIKCloKClIKClIKUggAIEAApWAAJFIKCgoKUlIKCgoKUlIKUggAJFAAtXIKClIKAgtqCgpSCgpSCgoKClIKClIIACgiSgpSCgoKCgoKClpSCpIKUpJSClIK4goKEgoKWgoKClIKClIKUgpSCgoK4goLWgoSCgoKWggAHEoKCgsyShISEgoKWgIKkgIKkgIK4goKCgpaWgoKClsS0tOySgoSCgIKkgIKkgIKmgoCCpICCpoKCgIKSgpQACAiigoKAgraCgIKkgoKCgpSCgoKUlJSClIKCtKSCgoSmgpSCgpSCgpSC+oKCgoKClIKC", "153-153:1;555-566:1;569-569:1;572-577:1;585-589:1;614-618:1;615-617:1.1;632-634:1;646-705:2;728-736:1;737-737:2;738-738:3;740-750:4;752-756:5;753-753:5.1;757-761:6;758-758:6.1;782-794:1;796-796:2;797-797:3;799-808:4;810-814:5;811-811:5.1;815-819:6;816-816:6.1;833-835:1;837-842:2;844-848:3;940-962:1;942-949:1.1;1647-1663:1;1682-1692:1;1834-1846:1")]
[assembly: go.GoPositionMap("path/filepath/path_windows_test.go", "path_windows_test.cs", "AB4sgoKCloIACQiGmpKCgpSAhIKkgoCCgqSAgoKkkoCCgsqEgoKUgtyClIKkgqaSgvyChISClIKUloKCgoKClJSCqISEhLq6qqKC3oKCgoKUgoKClIKCpoKCgoKUgoKmgoKCgpSCggANLgAUBpSClIKCuIKClJSClNaCgoKUuJKUgoKUlIKmpsjCgpSCgpaCgpSCgpSCgoKUgoKmgoKUogATBIKCgoSCloKWgpKUgpaoABAqgoKClIKCkoKohAAbSoKAgqaCgpSEgoKWhISCgoKEgoKChKaCgoSmgpSCgpKCyuaCouSCgoKUgoKClLiCgqaCggAJBqKCgoKCgqa4uoIACRaCgpSClJSmkgAIBKaClISCgpaCgoKEgoKClIKClIK4xoKUhIKCgoKWgoSCgpSC+IKCgpaEgoKClISCgoKWgoKUgoKClKaUgqiEgoKCloSCgoKWgoKWgpSUggAICIIADCKCgoIACQqCAAgYgoI=", "287-296:1;305-326:1;466-472:1;487-501:1")]
// </GoSourcePositionMaps>

namespace go.path;

[GoPackage("filepath_test")]
public static partial class filepath_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct TestAbsWindows_type {}
    internal partial struct TestIssue13582_tests {}
    internal partial struct TestIssue52476_tests {}
    internal partial struct TestToNorm_tests {}
    internal partial struct TestToNorm_testsDir {}
    internal partial struct TestWalkSymlinkRoot_type {}
    internal partial struct globSymlinkTestsᴛ1 {}
    internal partial struct globTest {}
    internal partial struct globTestsᴛ1 {}
    public partial struct EvalSymlinksTest {}
    public partial struct ExtTest {}
    public partial struct IsAbsTest {}
    public partial struct IsLocalTest {}
    public partial struct JoinTest {}
    public partial struct LocalizeTest {}
    public partial struct MatchTest {}
    public partial struct Node {}
    public partial struct PathTest {}
    public partial struct RelTests {}
    public partial struct SplitListTest {}
    public partial struct SplitTest {}
    public partial struct VolumeNameTest {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(go.path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸruntimeꓸdebug() => builtin.initPackage(typeof(go.runtime.debug_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsyscall() => builtin.initPackage(typeof(syscall_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.path.filepath_package));
    }
}
