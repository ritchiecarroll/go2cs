// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.@internal.strconv_package;
global using static global::go.@internal.strconv_internal_test_package;

// <ImportedTypeAliases>
global using flagꓸErrorHandling = go.flag_package.ΔErrorHandling;
global using httpꓸCookie = go.net.http_package.ΔCookie;
global using httpꓸHandler = go.net.http_package.ΔHandler;
global using httpꓸHeader = go.net.http_package.ΔHeader;
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
global using strconvꓸError = go.@internal.strconv_package.ΔError;
// </ImportedTypeAliases>

using go;
using static global::go.@internal.strconv_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b657870313020696e743b206d616e7420696e7465726e616c2f737472636f6e765f746573742e75696e743132383b206578703220696e743b206f6b20626f6f6c7d", "pow10Testsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6620666c6f617436343b207320737472696e677d", "roundTripCasesᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e2075696e7436343b206f757420737472696e677d", "varlenUintsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20666c6f617420666c6f617436343b20666d7420627974653b207072656320696e743b2062697453697a6520696e747d", "ftoaBenchesᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b782075696e7436343b207920696e7465726e616c2f737472636f6e765f746573742e75696e743132383b2068692075696e7436343b206d69642075696e7436343b206c6f2075696e7436347d", "umul192Testsᴛ1")]
[assembly: GoTypeAlias("Error", "ΔError")]
[assembly: GoTypeAlias("Uint128", "global::go.@internal.strconv_package.uint128")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<io_package.ReadCloser, io_package.Reader>]
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
[assembly: go.GoPositionMap("internal/strconv/atob_test.go", "atob_test.cs", "ABImABAigoKCgsrKgoKAggALFsqCgoKC")]
[assembly: go.GoPositionMap("internal/strconv/atoc_test.go", "atoc_test.cs", "AA4egoKClIKCggAHEoIAmgG2AoKCgoKUgoKCgtyC2JKClIKCgpSC", "", "", "32=NaN/1/4/26,NaN/2/4/27,NaN/3/4/28,NaN/4/4/28")]
[assembly: go.GoPositionMap("internal/strconv/atof_test.go", "atof_test.cs", "ABguAMYCjAUAXb4BgqaUgpSUgoKCgqaCgoKmgoK4goKCgsqCgoKClILcgoKCgoKCgoK4goKCgoKUgoLKgoKCgoKUgoK4poCkgKSCgoKClAAJCuYAChyCgoKCgpSCgpSCgoKUgoKUupKCgpSCgoKClJSCgpS0tKSU2qKClIKCgpSCyqKCuKKCuKKCuKKCuKKCgoK4ooKCgriigoKClIKCgoKCgsqigriigriigriigoKCgpSCgriigoKCgpSCgg==", "", "", "25=Repeat/1/4/257,Repeat/2/4/260,Repeat/3/4/274,Repeat/4/4/275;352=Repeat/1/2/19,Repeat/2/2/20")]
[assembly: go.GoPositionMap("internal/strconv/atoi_test.go", "atoi_test.cs", "ABMoABs2AF68AQAjRgBRogEAFy4AIEDcgoKCgoLcgoKCgoLcgoKCgoLcgoKCgoLcgoKCgoLcgoKCgoLcgpSCgoKC2oKCgoLugpSCgoKC2oKCgoLugpSCgoKCyIKCgoIAChb+AAkSgoKUgpSmgoKCgoLcgoKCgoLcgoKCgoLcgoKCgoIACQyCgpSCAAgSgu6ykoKCgtyCgpSCuILKgsqykoKCgg==", "527-529:1;530-532:2;549-555:1;560-562:1;563-565:2;581-587:1")]
[assembly: go.GoPositionMap("internal/strconv/ctoa_test.go", "ctoa_test.cs", "ABMYggARMIKCgtyigoCCtqI=", "47-51:1")]
[assembly: go.GoPositionMap("internal/strconv/decimal_test.go", "decimal_test.cs", "ABMkAA0cgoKCgoKCggAMGgASJIKCgoKCgoKmgoKCgqaCgoKCAAsYAAwagoKCgoKCgg==")]
[assembly: go.GoPositionMap("internal/strconv/fp_test.go", "fp_test.cs", "ABUmgpSkpJSqooCCgoKUgoKClKaCgoKCgpSUgoKCgoKUlKSCgpSqooCCgoKClIKCgpSkgoKClNyCgpKCgoKClIKCgpSCgpSCgoKClKSCgoKUgpSC7gAPIvKClKSUuIKAgsiCgoKUgpSCgoKUgIKk1pSCgoKCgoKClIKCgoKUgpSCgoCCpJSCgoCCAAkMlIKCgoKCgoKUgpSClIKCgoKCgoKAgg==")]
[assembly: go.GoPositionMap("internal/strconv/ftoa_test.go", "ftoa_test.cs", "ABUqgP4AygGEA4KCgoKClIKClIKCpoKUgoKUgoLcgoKCgoKAgraCgoKAguyCgoKUgoKClIKCgoKCpoKCgoKCgsqigoCCtqIACQQAMnKCspKC3IKCsqKC", "302-306:1;369-373:1;380-384:1", "", "31=fdiv/1/2/109,fdiv/2/2/110,NaN/1/2/118,NaN/2/2/119,Inf/1/3/120,Inf/2/3/121,Inf/3/3/122;340=Ldexp/1/5/22,Ldexp/2/5/23,Ldexp/3/5/34,Ldexp/4/5/35,Ldexp/5/5/36")]
[assembly: go.GoPositionMap("internal/strconv/import_test.go", "import_test.cs", "ABAigoKCgoKCgoI=")]
[assembly: go.GoPositionMap("internal/strconv/itoa_test.go", "itoa_test.cs", "ABImAC5YooKCgqaCgriCgoKmgoLKgoKC7oKAgraiAAgQAAgSgoKCgqaCggAJDgAWNIKCgoLKooKCgsqigoKCgsqigoKCyqKCgoKCyoKCgoKCgtyigoKCguiCspKCgoI=", "99-103:1;214-219:1;234-240:1")]
[assembly: go.GoPositionMap("internal/strconv/math_test.go", "math_test.cs", "ABMaAAcagoKCgtyCgoKCpoKCgsyCAAkGAAQWgoKCgtyCgoKCgsqCgoKCgsqCgoKUpoKCgoKUgpSClIKUgpSClILKgoKCuIK4goLKgqKCgoKClIKU", "", "", "64=u128/1/2/7,u128/2/2/8")]
// </GoSourcePositionMaps>

namespace go.@internal;

[GoPackage("strconv_test")]
public static partial class strconv_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct TestFormatComplex_tests {}
    internal partial struct appendBoolTest {}
    internal partial struct atobTest {}
    internal partial struct atocTest {}
    internal partial struct atofSimpleTest {}
    internal partial struct atofTest {}
    internal partial struct benchCase {}
    internal partial struct ftoaBenchesᴛ1 {}
    internal partial struct ftoaTest {}
    internal partial struct itob64Test {}
    internal partial struct numErrorTest {}
    internal partial struct parseErrorTest {}
    internal partial struct parseInt32Test {}
    internal partial struct parseInt64BaseTest {}
    internal partial struct parseInt64Test {}
    internal partial struct parseUint32Test {}
    internal partial struct parseUint64BaseTest {}
    internal partial struct parseUint64Test {}
    internal partial struct pow10Testsᴛ1 {}
    internal partial struct roundIntTest {}
    internal partial struct roundTest {}
    internal partial struct roundTripCasesᴛ1 {}
    internal partial struct shiftTest {}
    internal partial struct uitob64Test {}
    internal partial struct umul192Testsᴛ1 {}
    internal partial struct varlenUintsᴛ1 {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸembed() => builtin.initPackage(typeof(embed_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸstrconv() => builtin.initPackage(typeof(go.@internal.strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸcmplx() => builtin.initPackage(typeof(go.math.cmplx_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(go.math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttp() => builtin.initPackage(typeof(net.http_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(go.sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.@internal.strconv_package));
    }
}
