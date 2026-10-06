// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static go.regexp_package;
using static go.regexp_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b52656765787020737472696e673b206d696e20696e747d", "minInputLenTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6c656674205b5d72756e653b207269676874205b5d72756e653b206d6572676564205b5d72756e653b206e657874205b5d75696e7433323b206c65667450432075696e7433323b20726967687450432075696e7433327d", "runeMergeTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b206e20696e747d", "benchSizesᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20726520737472696e677d", "compileBenchDataᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b726520737472696e673b2069734f6e655061737320626f6f6c7d", "onePassTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b726520737472696e673b206d6174636820737472696e677d", "onePassTests1ᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7320737472696e673b207220737472696e673b206e20696e743b206f7574205b5d737472696e677d", "splitTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<FindTest, ж<FindTest>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("regexp/all_test.go", "all_test.cs", "ABEgABgyABUegoKClIKSgpSmgoK4goK4soKClIKCpoKCuIKyuLKCgpSCuIKyuLKCgpSCgoLKgrIAEBAAS7gBABcy3IKCgoKClIKCuIKC3JSCgpSCgoKUgoK4goLMgoKCgpSCgriCgtyCsoKCgpSCgriSggAMFgAKEgARIIKUgoKUuoKCgoKUgoKCgoLuopSCgoKUggAQIpQADRyCgoKCgoKUgoKClIKCgriCgoIADAwAGT6CgoKCgpaCgpaCgoIACg6ygoS+goKUgoKUgIIACAySgoKUgoKUgryigpaC1qKCgoKCgoKCgoLKooKCgoKCgoLKooKCgoKCgoKCggAICqKCgoKCgoKCgoKUgsqigoKCgoKCgoKClILKooKCgoKCgsqigoKCgoKCAAgKooKCgoKCgvqipoKCgoKC+qKCgoKCguiigoKCgoK4ooKCgpSCgoLoooKCgoKCuKKCgoKUgoKC6KKCgoKCguiigoKCgoLoooKCgoKC6KKCgoKCgriigoKCgoK4ooKCgoKC6KKCgoKigsqigoKCooKC7qKCgoKmgoKCgriigoKCgvjcgrKSgoKAggALDoKCgpKWgoKWgoKWgoIACAgADiSCgoKCggAICoKCgoKCgoKUgIKCpIKWgoKCgpSCgIKCpIKmgoKCgg==", "344-344:1;837-841:1;848-853:1;890-897:1;984-990:1")]
[assembly: go.GoPositionMap("regexp/exec2_test.go", "exec2_test.cs", "AA8eooKU")]
[assembly: go.GoPositionMap("regexp/exec_test.go", "exec_test.cs", "AB6EAQArAtbCgoKUkoKCgoKUlIKCAAYSgoKUtLaStIK0tIKUlIKClpKUgoKUlIKAgqSUgoKUlLaCpJSClIKC3JSCgpSCgoKCgoCCpJSCgoKAgqTaxoCCpIKUorTuooKmooKmooKmooK27qKCpqKCpqKCpqKCpoKCgqam1IKAgrbmlIKmgoKCpoKCgoKUgoKClIKCgoKUgpSCptyygoKUgoLIlMKCgoKUkoKCgoKCgoKCgpQACBSClIKCgoKUgoKmggA9foKYgqLGgoCCgtaCyIKCqIKCgIKkgoCC7oKUlpaCgoKqhJKCgpTq1oKUyIKCgpSUgoKUgoKClIKCgpSClIIACQikABMeloKCgqaCgoKmgoLUhIKCgoKCgpSClIKClIKClIKCgoKCgqaClIKClIKCgsqCgpSCgoKCgoKUgpSm5oKEgoKygpSCsoKCggALEIKCgoKUsoKUgrKCgoKC7gANEgAIGIKCgpSAkqSCgJIACQyigoKUgg==", "266-270:1;656-663:1;679-687:1")]
[assembly: go.GoPositionMap("regexp/find_test.go", "find_test.cs", "ABkwgvYAYN4BwoKCgoKCgoKCpqqSgoKClILIxLSCgpSC/IKCgsjGksaCgvyiyMS0goLqgrK4grK4grK8koKCyMS0goKUgoKClIKCAAgOgoKCyMS0goKUgoKCAAgOosjEtIKClIKC/IKyuIKyvLKCgpSCgoKUlIKCgpSCgoLKgrKCyMS06qKCgpSCgoKUlIKCgsqCsoLIxLTqgoKClIKCyqLIxLTYgrK4grK4grK8krKCyMS0tIL8grKCyMS0tIL8osjEtLSC6oKyuIKy")]
[assembly: go.GoPositionMap("regexp/onepass_test.go", "onepass_test.cs", "ACEcAF3eAYKygoKUggAJCgAnVoKqgoCCgraCgIKCpIKCAAsMvoKCgoKClIKClII=")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("regexp")]
public static partial class regexp_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸcompressꓸbzip2() => builtin.initPackage(typeof(compress.bzip2_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸregexpꓸsyntax() => builtin.initPackage(typeof(regexp.syntax_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    // </ImportInitializers>
}
