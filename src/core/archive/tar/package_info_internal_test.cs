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
[assembly: go.GoPositionMap("archive/tar/reader_test.go", "reader_test.cs", "ACs0ggDHBLgJsrKCgpSUgoKY/IKCgoKClJSEgpSCgoKUloKClIKmgpaCgpSCqIKUogAOCIIAEDCysoKClJSCgoKClIKAgqSCqICCxPiigoKUlIKCgrYAEgyAABAKsoLcgoKUlrKChAAwXoKCgpSUgrSCtIK0grSCtIK2goKCgIKkgoKAgsiCpoIACRLCgoKUlIKCgoKClIKClJaCgsyClIKCgoLGAAgEggAkUoKCgoKUgIIACwqCABI4goKCgpSAggAPCoKCgoKUgpSWtJKCgoKCgpSWgoKClJaSgoKCgoKUlgA0dKKCgoKCgoKUgpSCAA0KgogAkQGyAoKClMiClLiCgoKCgoKClIKUgpSClIIADRKCgpQAJgaCAK8BpAOCgpSStIKUgpKCxKaClIKCgILWgoKAgpKCpILGgIKkgILmAA0MgoL6goKmgqaEgoKCgpSCpoKClIIACgqCgoKCgqaCgoKClII=", "632-698:1;728-753:1;1037-1046:1;1048-1067:2;1069-1078:3;1157-1159:1;1313-1319:2;1321-1326:3", "", "45=Unix/1/55/13,Unix/2/55/24,Unix/3/55/42,Unix/4/55/56,Unix/5/55/75,Unix/6/55/95,Unix/7/55/115,Unix/8/55/139,Unix/9/55/143,Unix/10/55/144,Unix/11/55/151,Unix/12/55/155,Unix/13/55/156,Unix/14/55/166,Unix/15/55/174,Unix/16/55/187,Unix/17/55/188,Unix/18/55/189,Unix/19/55/206,Unix/20/55/207,Unix/21/55/208,Unix/22/55/236,Repeat/1/4/253,Unix/23/55/254,Repeat/2/4/258,Unix/24/55/272,Unix/25/55/278,Unix/26/55/288,Unix/27/55/293,Unix/28/55/305,Unix/29/55/322,Unix/30/55/326,Unix/31/55/327,Unix/32/55/349,Unix/33/55/353,Unix/34/55/354,Unix/35/55/372,Unix/36/55/387,Unix/37/55/391,Unix/38/55/392,Unix/39/55/400,Unix/40/55/404,Unix/41/55/405,Unix/42/55/413,Unix/43/55/417,Unix/44/55/418,Unix/45/55/427,Unix/46/55/444,Unix/47/55/461,Unix/48/55/479,Unix/49/55/517,Unix/50/55/527,Unix/51/55/539,Unix/52/55/549,Unix/53/55/559,Unix/54/55/576,Repeat/3/4/590,Unix/55/55/591,Repeat/4/4/593;1025=Unix/1/1/13;1170=Invoke/1/21/6,Invoke/2/21/9,Invoke/3/21/13,Invoke/4/21/16,Invoke/5/21/20,Invoke/6/21/19,Invoke/7/21/25,Invoke/8/21/24,Invoke/9/21/30,Invoke/10/21/29,Invoke/11/21/36,Invoke/12/21/38,Invoke/13/21/34,Invoke/14/21/42,Invoke/15/21/41,Invoke/16/21/46,Invoke/17/21/45,Invoke/18/21/50,Invoke/19/21/49,Invoke/20/21/54,Invoke/21/21/53")]
[assembly: go.GoPositionMap("archive/tar/strconv_test.go", "strconv_test.cs", "ABMcggAQLIKCggAKCoIAJViCgoKCgoKUpoIACgqCACxmgoKCgoKCgpSmggAKCoIAGDyCgoIACgqCADt+goKCgoKUpoIACgyCAB5GgoKCAA0MgoKEABpEgoKCgoKUpoKmggAMDIKChAAOKoKCgoKClKaC", "", "", "225=Unix/1/45/5,Unix/2/45/6,Unix/3/45/7,Unix/4/45/8,Unix/5/45/9,Unix/6/45/10,Unix/7/45/11,Unix/8/45/12,Unix/9/45/13,Unix/10/45/14,Unix/11/45/15,Unix/12/45/16,Unix/13/45/17,Unix/14/45/18,Unix/15/45/19,Unix/16/45/20,Unix/17/45/21,Unix/18/45/22,Unix/19/45/23,Unix/20/45/24,Unix/21/45/25,Unix/22/45/26,Unix/23/45/27,Unix/24/45/28,Unix/25/45/29,Unix/26/45/30,Unix/27/45/31,Unix/28/45/32,Unix/29/45/33,Unix/30/45/34,Unix/31/45/35,Unix/32/45/37,Unix/33/45/38,Unix/34/45/39,Unix/35/45/41,Unix/36/45/42,Unix/37/45/43,Unix/38/45/44,Unix/39/45/45,Unix/40/45/46,Unix/41/45/47,Unix/42/45/48,Unix/43/45/49,Unix/44/45/50,Unix/45/45/51;296=Unix/1/2/1,Nanosecond/1/2/1,Unix/2/2/1,Nanosecond/2/2/1")]
[assembly: go.GoPositionMap("archive/tar/tar_test.go", "tar_test.cs", "ACxKgoKUgpSCgpaCgpSUgtaCgpSClIKCloKUgpSUgtaCgpSClIKCloKUgoIACgaCAE6sAYKCgpSClIKClIKCAAgKgoKClIKClICSpICCpICCpICCtoCCAAgIgoKClIKClICStoCCpICCpICC+IKEhIKCgIKkgoKWgoKUgJKkgJKkgILIgoSCggAJEoCCpICCpICCuIKCgpSClIKClIIACBKiAHTqAYKCgoKClIKUgoKUgJKkgIKkgJKkgJKkgJKkgJKkgJKkgIKCpICCpICCpICCpICCpICCpICCAAsKggDbAcADgoKClIKUgpSCABIKggAfSpKykoKmgoKAgqSAgraAgu6SgoKWgoKClIKClIKCgoCCpICCAAgS+IKmgqaCpoKmgqaCpoKmgqaCgoKClIKUgg==", "795-817:1;797-815:1.1;819-846:2;831-844:2.1", "", "344=Now/1/1/4,Round/1/1/4;395=Unix/1/11/6,Unix/2/11/16,Unix/3/11/26,Unix/4/11/36,Unix/5/11/46,Unix/6/11/56,Unix/7/11/66,Unix/8/11/76,Unix/9/11/86,Unix/10/11/97,Unix/11/11/109;613=Repeat/1/5/74,Repeat/2/5/77,Repeat/3/5/78,Repeat/4/5/85,Repeat/5/5/86,Unix/1/24/122,Unix/2/24/125,Unix/3/24/128,Unix/4/24/132,Unix/5/24/136,Unix/6/24/140,Unix/7/24/144,Unix/8/24/148,Unix/9/24/151,Unix/10/24/154,Unix/11/24/158,Unix/12/24/162,Unix/13/24/166,Unix/14/24/170,Unix/15/24/174,Unix/16/24/178,Unix/17/24/182,Unix/18/24/186,Unix/19/24/190,Unix/20/24/194,Unix/21/24/198,Unix/22/24/202,Unix/23/24/206,Unix/24/24/210")]
[assembly: go.GoPositionMap("archive/tar/writer_test.go", "writer_test.cs", "ACgygqqCgoKCgpSClJSCgpSCgpQAIwaCAIsDzAaCgoKClJSyooKChIKkgoLGgoLGgoKAgpKCpILGgoLWuIKCgpSCggAKDpSCgpSCgqaCgoKCgoCCpICCpICCtoKmgoKClILolIKClIKClJSChIKCgoCCpICCtoKmgoKClIIACAimgoKWgoKogoKEgoKEhIKCgIKkgIKkgIK2gqaCgoKUgpSClILogrqCgpSCgpSCgoKCgIKkgIKkgIK2goKClIL6goKClIKClITegoKAgqSAgqSAgraCqNyC6JSCgpSCgpSmgoSCgoKAgqSAgraCgoKUgriCgoSUyoCCpICCpISEgoKClIKUggAGEIKClIIACwaCgoKCgIKkgIK4goKCgIK4goKAgriCgoKAgqSAgqSAgqSAgqSAgriCgoKAgqSAgriCgoKAgqSAgriCgoCCpICCpICCpICCpICCAAwKgoQADyyCgoIABRKyAAcSsoSCgMikgIKokoKCgoKWgoKClIIACQqCABAmgoCCAA4SgoKUACYGggDHAdQDgoKCkoKUgrSClIKCgsSmgpSCgsaCgoCCkoKkgsaAgqSAgua4gIIACAqi7oKCgIKkgIK4loSCgpSEgoKEgoKUgoKUgpaCgpSCqIKogpaCgpSCgriC6IK4goKAgg==", "477-484:1;486-534:2;841-850:1;852-858:2;860-865:3;867-885:4;887-896:5;898-907:6;909-926:7", "", "821=Bytes/1/4/1,Index/1/4/1,Bytes/2/4/2,Index/2/4/2,Bytes/3/4/3,Index/3/4/3,Bytes/4/4/4,Index/4/4/4;1057=Invoke/1/19/9,Invoke/2/19/10,Invoke/3/19/11,Invoke/4/19/11,Invoke/5/19/12,Invoke/6/19/13,Invoke/7/19/13,Invoke/8/19/14,Invoke/9/19/15,Invoke/10/19/15,Invoke/11/19/16,Invoke/12/19/16,Invoke/13/19/17,Invoke/14/19/17,Invoke/15/19/18,Invoke/16/19/18,Invoke/17/19/19,Invoke/18/19/19,Invoke/19/19/19;1554=Mode/1/2/1,FileInfo/1/1/1,Mode/2/2/1")]
// </GoSourcePositionMaps>

namespace go.archive;

[GoPackage("tar")]
public static partial class tar_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("file")] partial struct Benchmark_file {}
    [GoLocalName("file")] partial struct FuzzReader_file {}
    [GoLocalName("makeReg")] partial struct TestFileReader_makeReg {}
    [GoLocalName("makeSparse")] partial struct TestFileReader_makeSparse {}
    [GoLocalName("testRead")] partial struct TestFileReader_testRead {}
    [GoLocalName("testRemaining")] partial struct TestFileReader_testRemaining {}
    [GoLocalName("testWriteTo")] partial struct TestFileReader_testWriteTo {}
    [GoLocalName("makeReg")] partial struct TestFileWriter_makeReg {}
    [GoLocalName("makeSparse")] partial struct TestFileWriter_makeSparse {}
    [GoLocalName("testReadFrom")] partial struct TestFileWriter_testReadFrom {}
    [GoLocalName("testRemaining")] partial struct TestFileWriter_testRemaining {}
    [GoLocalName("testWrite")] partial struct TestFileWriter_testWrite {}
    [GoLocalName("testCase")] partial struct TestPartialRead_testCase {}
    [GoLocalName("testClose")] partial struct TestWriter_testClose {}
    [GoLocalName("testHeader")] partial struct TestWriter_testHeader {}
    [GoLocalName("testReadFrom")] partial struct TestWriter_testReadFrom {}
    [GoLocalName("testWrite")] partial struct TestWriter_testWrite {}
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
