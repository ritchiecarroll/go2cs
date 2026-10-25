// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
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
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("regexp/all_test.go", "all_test.cs", "ABEgABgyABUegoKClIKSgpSmgoK4goK4soKClIKCpoKCuIKyuLKCgpSCuIKyuLKCgpSCgoLKgrIABxAAW7gBABgy3IKCgoKClIKCuIKC3JSCgpSCgoKUgoK4goLcgoKCgpSCgriCgtyCsoKCgpSCgriSggAKFgAIEgATIIKUgoKUyoKCgoKUgoKCgoLuopSCgoKUggAQIpQADRyCgoKCgoKUgoKClIKCgriCgoIACwwAGT6CgoKCgqaCgqaCgoIACg6ygpS+goKUgoKUgIIACAySgoKUgoKUgryigqaC1qKCgoKCgoKCgoLKooKCgoKCgoLKooKCgoKCgoKCggAICqKCgoKCgoKCgoKUgsqigoKCgoKCgoKClILKooKCgoKCgsqigoKCgoKCAAgKooKCgoKCgvqipoKCgoKC+qKCgoKCguiigoKCgoK4ooKCgpSCgoLoooKCgoKCuKKCgoKUgoKC6KKCgoKCguiigoKCgoLoooKCgoKC6KKCgoKCgriigoKCgoK4ooKCgoKC6KKCgoKigsqigoKCooKC7rKCgoKmgoKCgriigoKCgujcgrKSgoKAggALDoKCgoKmgoKmgoKmgoL4AA4kgoKCgoIACAqCgoKCgoKClICCgqSCpoKCgoKUgoCCgqSCpoKCgoI=", "344-344:1;837-841:1;848-853:1;890-897:1;984-990:1", "", "43=Repeat/1/1/12;1024=Repeat/1/2/3,Repeat/2/2/3")]
[assembly: go.GoPositionMap("regexp/exec2_test.go", "exec2_test.cs", "ABEeooKU")]
[assembly: go.GoPositionMap("regexp/exec_test.go", "exec_test.cs", "AB6EAQArAtbCgoKUkoKCgoKUlIKCAAYSgoKUtMaCtIK0tIKUlIKCpoKUgoKUlIKAgqSUgoKUlMaClJSClIKC3JSCgpSCgoKCgoCCpJSCgoKAgqTatoCCpIKUorTuooKmooKmooKmooK27qKCpqKCpqKCpqKCpoKCgqam1IKAgrbmlIKmgoKCpoKCgoKUgoKClIKCgoKUgpSCptyygoKUgoLIlKKCgoKUkoKCgoKCgoKCgpQACRSClIKCgoKUgoKmggA+foK4goLGgoCCgtaCyIKCuIKCgIKkgoCC/oKUpqaCgoLKhJKCgpQACAq2gpTIgoKClJSCgpSCgoKUgoKClIKUggAJCKQAFB6mgoKCtoKCgraCgpSUgoKCgoKClIKUgoKUgoKUgoKCgoKCpoKUgoKUgoKSyoKClIKCgoKCgpSClKbmgpSCgrKClIKygoKCAAsQgoKCgpSygpSCsoKCgoLuAAwSAAgYgoKClICSpIKAkgAJDKKCgpSC", "266-270:1;656-663:1;679-687:1")]
[assembly: go.GoPositionMap("regexp/find_test.go", "find_test.cs", "ABkwgqYAbuIBwoKCgoKCgoKCpqqSgoKClILYtLSCgpSC7IKCgtjGgsaCguyi2LS0goLagrK4grK4grK8koKC2LS0goKUgoKClIKC/oKCgti0tIKClIKCgv6i2LS0goKUgoLsgrK4grK8soKClIKCgpSUgoKClIKCgsqCsoLYtLTaooKClIKCgpSUgoKCyoKygti0tNqCgoKUgoLKoti0tMiCsriCsriCsrySsoLYtLS0guyCsoLYtLS0guyi2LS0tILagrK4grI=", "", "", "30=build/1/70/1,build/2/70/2,build/3/70/3,build/4/70/4,build/5/70/5,build/6/70/7,build/7/70/8,build/8/70/9,build/9/70/10,build/10/70/11,build/11/70/12,build/12/70/15,build/13/70/16,build/14/70/17,build/15/70/18,build/16/70/19,build/17/70/20,build/18/70/21,build/19/70/22,build/20/70/23,build/21/70/24,build/22/70/25,build/23/70/26,build/24/70/27,build/25/70/28,build/26/70/29,build/27/70/30,build/28/70/31,build/29/70/32,build/30/70/33,build/31/70/34,build/32/70/36,build/33/70/37,build/34/70/38,build/35/70/39,build/36/70/43,build/37/70/44,build/38/70/45,build/39/70/46,build/40/70/47,build/41/70/50,build/42/70/52,build/43/70/53,build/44/70/54,build/45/70/55,build/46/70/56,build/47/70/58,build/48/70/59,build/49/70/60,build/50/70/62,build/51/70/64,build/52/70/65,build/53/70/66,build/54/70/67,build/55/70/69,build/56/70/71,build/57/70/72,build/58/70/73,build/59/70/74,build/60/70/87,build/61/70/89,build/62/70/90,build/63/70/91,build/64/70/93,build/65/70/94,build/66/70/95,build/67/70/96,build/68/70/97,build/69/70/98,build/70/70/104")]
[assembly: go.GoPositionMap("regexp/onepass_test.go", "onepass_test.cs", "ABMcAGreAYKygoKUggAICgAnVoKqgoCCgraCgIKCpIKCAAkMvoKCgoKClIKClII=", "", "", "146=Repeat/1/1/40")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("regexp")]
public static partial class regexp_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
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
