// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.strconv_package;
global using static global::go.strconv_internal_test_package;

// <ImportedTypeAliases>
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
// </ImportedTypeAliases>

using go;
using static global::go.strconv_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b426f6f6c20626f6f6c3b20496e7420696e743b20496e74363420696e7436343b2055696e7436342075696e7436343b20466c6f6174363420666c6f617436343b20436f6d706c657831323820636f6d706c65783132383b204572726f72206572726f723b204279746573205b5d627974657d", "Sinkᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b636f756e7420696e743b206465736320737472696e673b20666e2066756e6328297d", "mallocTestᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6620666c6f617436343b207320737472696e677d", "roundTripCasesᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e2075696e7436343b206f757420737472696e677d", "varlenUintsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20666c6f617420666c6f617436343b20666d7420627974653b207072656320696e743b2062697453697a6520696e747d", "ftoaBenchesᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<os_package.File, io_package.Reader>(Pointer = true)]
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
[assembly: go.GoPositionMap("strconv/atob_test.go", "atob_test.cs", "ACRIooKClIKmgriClIIAChaCgoCCABAggoKCgg==")]
[assembly: go.GoPositionMap("strconv/atoc_test.go", "atoc_test.cs", "AB9CggCSAbYCgoKClIKClIKWgoKClIKCAAgOkoKEgoKClII=")]
[assembly: go.GoPositionMap("strconv/atof_test.go", "atof_test.cs", "AMwD+AaCpqaCgoKmgoKCuoKUlIKCgoKWgoKCloKCuIKCgoLKgoKCgpSC3IKCgoKCgoKCqIKCgoKClIKCyoKCgoKClIKCuKaApICkgoKCgpQACgoAEiKCgoKCgpSCgpSCgoKUgoKUupKCgpSCgoKClISCgpS0tLSU2qKChIKCgpSCyqKCuKKCuKKCuKKCuKKCgoK4ooKCgriigoKClIKCgoKCgsqigriigriigriigoKCgpSCgriigoKCgpSCgg==")]
[assembly: go.GoPositionMap("strconv/atoi_test.go", "atoi_test.cs", "AMQC/gSmgoKCpoKCgqaCgoKmgoKCpoKCgqaCgoLKgoKCgoLcgoKCgoLcgoKCgoLcgoKCgoLcgoKCgoLcgoKCgoLcgpSCgoKC2oKCgoL+gpSCgoKC2oKCgoL+gpSCgoKCgpSC2oKCgoKClIL+gqaCpoIAGTCCgpSClNaCgoKCgoIACAyCgoKCgoLcgoKCgoKC3IKCgoKCggAIDIKCyoCCAAgKgoKC+IKClIIACBKC7rKSgoKC3IKClIK4gsqCyrKSgoKC", "615-617:1;618-620:2;637-643:1;648-650:1;651-653:2;669-675:1")]
[assembly: go.GoPositionMap("strconv/ctoa_test.go", "ctoa_test.cs", "ABMYggAOMIKCgtyigoCCtg==", "47-51:1")]
[assembly: go.GoPositionMap("strconv/decimal_test.go", "decimal_test.cs", "ACBAgoKCgoKCggAdPoKCgoKCgoKmgoKCgqaCgoKCABgygoKCgoKCgg==")]
[assembly: go.GoPositionMap("strconv/fp_test.go", "fp_test.cs", "ABAggpSkpKSqooCCgoKUgoKClKaCgoKCgpSUgoKCgoKUlKSCgpSqooCCgoKClIKCgpSkgoKClAANBqKCgpSUhIKCgpSCgoKUgoKUgoKCgpSkgoKClIKkgriC")]
[assembly: go.GoPositionMap("strconv/ftoa_test.go", "ftoa_test.cs", "ABUqgACPAbgCgoKCgoKUgoKUgoKClIKC3IKCgoKCgIK2goKCgILsgoKClIKCgoSCgoKCgpaCgoKCgoLKooKAgrYAMmSCspKC3IKCsqKC", "250-254:1;307-311:1;318-322:1")]
[assembly: go.GoPositionMap("strconv/ftoaryu_test.go", "ftoaryu_test.cs", "AAwagoKCgoLKgoKCgoI=")]
[assembly: go.GoPositionMap("strconv/itoa_test.go", "itoa_test.cs", "ADx8ooKCgqaCgqiCgoKmgoK6goKC3oKAgrYAFCSCgoKCpoKCACFCgoKCgsqigoKCyqKCgoKCyqKCgoLKooKCgoLKgoKCgoKC3KKCgoKCuIKykoKCgg==", "98-102:1;213-218:1;233-239:1")]
[assembly: go.GoPositionMap("strconv/quote_test.go", "quote_test.cs", "AA0esoKCgoKCgt6ygoKCgoKCABgygoKAgqSAgtqCgoCCpICC2oKCgIKkgILaooK4ooLcooLcooIAGziCgoCCpICC2oKCgIKkgILagoKAgqSAggA5cIKCgIIAT6IBgoKUgpSCAAoKsgAGGoK4tIKCzIKUgoKUgoKCgpSC6KKC6KKC")]
[assembly: go.GoPositionMap("strconv/strconv_test.go", "strconv_test.cs", "ABswgqSkgqSmoqKkpgAJDoKClIKmgoKUgoKAggAhIoKC7gAHEpKUkpSSlJKUkpSSlJKUkpSSlJIACwiCgoKCgoQABhaCgoKClICC", "90-97:1;91-96:1.1;99-101:2;102-104:3;105-107:4;108-110:5;111-113:6;114-116:7;117-119:8;120-122:9;123-125:10;126-128:11")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("strconv_test")]
public static partial class strconv_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct TestAllocationsFromBytes_bytes {}
    internal partial struct TestErrorPrefixes_vectors {}
    internal partial struct TestFormatComplex_tests {}
    internal partial struct TestUnquoteInvalidUTF8_tests {}
    internal partial struct appendBoolTest {}
    internal partial struct atobTest {}
    internal partial struct atocTest {}
    internal partial struct atofSimpleTest {}
    internal partial struct atofTest {}
    internal partial struct benchCase {}
    internal partial struct canBackquoteTest {}
    internal partial struct ftoaBenchesᴛ1 {}
    internal partial struct ftoaTest {}
    internal partial struct itob64Test {}
    internal partial struct mallocTestᴛ1 {}
    internal partial struct numErrorTest {}
    internal partial struct parseErrorTest {}
    internal partial struct parseInt32Test {}
    internal partial struct parseInt64BaseTest {}
    internal partial struct parseInt64Test {}
    internal partial struct parseUint32Test {}
    internal partial struct parseUint64BaseTest {}
    internal partial struct parseUint64Test {}
    internal partial struct quoteRuneTest {}
    internal partial struct quoteTest {}
    internal partial struct roundIntTest {}
    internal partial struct roundTest {}
    internal partial struct roundTripCasesᴛ1 {}
    internal partial struct shiftTest {}
    internal partial struct uitob64Test {}
    internal partial struct unQuoteTest {}
    internal partial struct varlenUintsᴛ1 {}
    public partial struct Sinkᴛ1 {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸcmplx() => builtin.initPackage(typeof(go.math.cmplx_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(go.math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.strconv_package));
    }
}
