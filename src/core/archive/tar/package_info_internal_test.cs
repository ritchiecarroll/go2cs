// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.archive.tar_package;
using static go.archive.tar_internal_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<failOnceWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<fileInfoNames, go.io.fs_package.FileInfo>(Pointer = true)]
[assembly: GoImplement<readBadSeeker, io_package.ReadSeeker>(Pointer = true)]
[assembly: GoImplement<readBadSeeker, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<readSeeker, io_package.ReadSeeker>(Promoted = true)]
[assembly: GoImplement<readSeeker, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<reader, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<testError, error>(Promoted = true)]
[assembly: GoImplement<testError, error>]
[assembly: GoImplement<testFile, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<testFile, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<testNonEmptyReader, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<testNonEmptyReader, io_package.Reader>]
[assembly: GoImplement<testNonEmptyWriter, io_package.Writer>(Promoted = true)]
[assembly: GoImplement<testNonEmptyWriter, io_package.Writer>]
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
[assembly: go.GoPositionMap("archive/tar/fuzz_test.go", "fuzz_test.cs", "ABEaooKCgsqClIKClICCpISCgoqCgoKUgpSCgIKkuoKWgoKCgIKkgIK2gII=", "34-79:1")]
[assembly: go.GoPositionMap("archive/tar/reader_test.go", "reader_test.cs", "ACs0ggDHBLgJsrKCgpSUgoKY/IKCgoKClJSEgpSCgoKUloKClIKmgpaCgpSCqIKUogAOCIIAEDCysoKClJSCgoKClIKAgqSCqICCxPiigoKUlIKCgrYAEgyAABAKsoLcgoKUlrKChAAwXoKCgpSUgrSCtIK0grSCtIK2goKCgIKkgoKAgsiCpoIACRLCgoKUlIKCgoKClIKClJaCgsyClIKCgoLGAAgEggAkUoKCgoKUgIIACwqCABI4goKCgpSAggAPCoKCgoKUgpSWtJKCgoKCgpSWgoKClJaSgoKCgoKUlgA0dKKCgoKCgoKUgpSCAA0KgogAkQGyAoKClMiClLiCgoKCgoKClIKUgpSClIIADRKCgpQAJgaCAK8BpAOCgpSStIKUgpKCxKaClIKCgILWgoKAgqSkgsaAgqSAguYADQyCgvqCgqaCpoSCgoKClIKmgoKUggAKCoKCgoKCpoKCgoKUgg==", "632-698:1;728-753:1;1037-1046:1;1048-1067:2;1069-1078:3;1157-1159:1;1313-1319:2;1321-1326:3")]
[assembly: go.GoPositionMap("archive/tar/strconv_test.go", "strconv_test.cs", "ABMcggAQLIKCggAKCoIAJViCgoKCgoKUpoIACgqCACxmgoKCgoKCgpSmggAKCoIAGDyCgoIACgqCADt+goKCgoKUpoIACgyCAB5GgoKCAA0MgoKEABpEgoKCgoKUpoKmggAMDIKChAAOKoKCgoKClKaC")]
[assembly: go.GoPositionMap("archive/tar/tar_test.go", "tar_test.cs", "ACxKgoKUgpSCgpaCgpSUgtaCgpSClIKCloKUgpSUgtaCgpSClIKCloKUgoIACgaCAE6sAYKCgpSClIKClIKCAAgKgoKClIKClICSpICCpICCpICCtoCCAAgIgoKClIKClICStoCCpICCpICC+IKEhIKCgIKkgoKWgoKUgJKkgJKkgILIgoSCggAJEoCCpICCpICCuIKCgpSClIKClIIACBKiAHTqAYKCgoKClIKUgoKUgJKkgIKkgJKkgJKkgJKkgJKkgJKkgIKCpICCpICCpICCpICCpICCpICCAAsKggDbAcADgoKClIKUgpSCABIKggAfSpKykoKmgoKAgqSAgraAgu6SgoKWgoKClIKClIKCgoCCpICCAAgS+IKmgqaCpoKmgqaCpoKmgqaCgoKClIKUgg==", "795-817:1;797-815:1.1;819-846:2;831-844:2.1")]
[assembly: go.GoPositionMap("archive/tar/writer_test.go", "writer_test.cs", "ACgygqqCgoKCgpSClJSCgpSCgpQAIwaCAIsDzAaCgoKClJSyooKChIKkgoLGgoLGgoKAgqSkgsaCgta4goKClIKCAAoOlIKClIKCpoKCgoKCgIKkgIKkgIK2gqaCgoKUguiUgoKUgoKUlIKEgoKCgIKkgIK2gqaCgoKUggAICKaCgpaCgqiCgoSCgoSEgoKAgqSAgqSAgraCpoKCgpSClIKUguiCuoKClIKClIKCgoKAgqSAgqSAgraCgoKUgvqCgoKUgoKUhN6CgoCCpICCpICCtoKo3ILolIKClIKClKaChIKCgoCCpICCtoKCgpSCuIKChJTKgIKkgIKkhISCgoKUgpSCAAYQgoKUggALBoKCgoKAgqSAgriCgoKAgriCgoCCuIKCgoCCpICCpICCpICCpICCuIKCgoCCpICCuIKCgoCCpICCuIKCgIKkgIKkgIKkgIKkgIIADAqChAAPLIKCggAFErIABxKyhIKAyKSAgqiSgoKCgpaCgoKUggAJCoIAECaCgIIADhKCgpQAJgaCAMcB1AOCgoKSgpSCtIKUgoKCxKaClIKCxoKCgIKkpILGgIKkgILmuICCAAgKou6CgoCCpICCuJaEgoKUhIKChIKClIKClIKWgoKUgqiCqIKWgoKUgoK4guiCuIKCgII=", "477-484:1;486-534:2;841-850:1;852-858:2;860-865:3;867-885:4;887-896:5;898-907:6;909-926:7")]
// </GoSourcePositionMaps>

namespace go.archive;

[GoPackage("tar")]
public static partial class tar_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcompressꓸbzip2() => builtin.initPackage(typeof(compress.bzip2_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸhashꓸcrc32() => builtin.initPackage(typeof(hash.crc32_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸmaps() => builtin.initPackage(typeof(maps_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpath() => builtin.initPackage(typeof(path_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(go.path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸfstest() => builtin.initPackage(typeof(go.testing.fstest_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸiotest() => builtin.initPackage(typeof(go.testing.iotest_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}
