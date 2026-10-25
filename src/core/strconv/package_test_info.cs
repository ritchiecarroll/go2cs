// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.strconv_package;
global using static global::go.strconv_internal_test_package;

// <ImportedTypeAliases>
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using runtimeꓸError = go.runtime_package.ΔError;
global using strconvꓸError = go.@internal.strconv_package.ΔError;
// </ImportedTypeAliases>

using go;
using static global::go.strconv_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b426f6f6c20626f6f6c3b20496e7420696e743b20496e74363420696e7436343b2055696e7436342075696e7436343b20466c6f6174363420666c6f617436343b20436f6d706c657831323820636f6d706c65783132383b204572726f72206572726f723b204279746573205b5d627974657d", "Sinkᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b636f756e7420696e743b206465736320737472696e673b20666e2066756e6328297d", "mallocTestᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e2075696e7436343b206f757420737472696e677d", "varlenUintsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
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
[assembly: go.GoPositionMap("strconv/import_test.go", "import_test.cs", "AAsUgg==")]
[assembly: go.GoPositionMap("strconv/number_test.go", "number_test.cs", "AB02AAcQooKClIKmgriClILcyoKCgIIACxbKgoKCgsyCgoKUgoKCAAcSggCaAbYCgoKClIKClIKmgoKClIKCAAgOkoKUgoKClIIAChYAFy6mgoKCyoKCgoKCgriCgoKCgpSCggAMGgARIgATJgARIgAUKAASJAAQINymgoKCpoKCgqaCgoKmgoKCpoKCgqaCgoLKgoKCgoLcgoKCgoLcgoKCgoLcgoKCgoLcgoKCgoLcgoKCgoLcgpSCgoKC2oKCgoLugpSCgoKC2oKCgoLugpSCgoKCgpSC2oKCgoKClILugqaCpoIACRAACA4ACBKCgpSClNaCgoKCgoIACAyCgoKCgoLcgoKCgoKC3IKCgoKCggAIDIKCyoCCAAgKgoKCAAsIggARMIKCgtyigoCCtqIACBAADRaigoKCpoKCuIKCgqaCgsqCgoLugoCCtqIACBAACBKCgoKCpoKCAAkOABY0goKCgg==", "823-827:1;884-888:1", "", "116=NaN/1/4/26,NaN/2/4/27,NaN/3/4/28,NaN/4/4/28;801=New/1/1/3")]
[assembly: go.GoPositionMap("strconv/quote_test.go", "quote_test.cs", "AA0esoKCgoKCgt6ygoKCgoKCAAwaAAsYgoKAgqSAgtqCgoCCpICC2oKCgIKkgILaooK4ooLcooLcooIAChYAECKCgoCCpICC2oKCgIKkgILagoKAgqSAggAKFAAtXIKCgIIAChQAJ1AAHj6CgpSClIIACgqyAAYagriUgoLcgpSCgpSCgoKClILoooLoooI=")]
[assembly: go.GoPositionMap("strconv/strconv_test.go", "strconv_test.cs", "AA8g9JyCpKSCpMaioqSmAAkOgoKUgqaCgpSCgoCCACAigoL+AAgSkpSSlJKUkpSSlJKUkpSSlJKUkgALCIKCgoKClAAHFoKCgoKUgII=", "90-97:1;91-96:1.1;99-101:2;102-104:3;105-107:4;108-110:5;111-113:6;114-116:7;117-119:8;120-122:9;123-125:10;126-128:11")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("strconv_test")]
public static partial class strconv_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
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
    internal partial struct atofTest {}
    internal partial struct canBackquoteTest {}
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
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸcmplx() => builtin.initPackage(typeof(go.math.cmplx_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
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
