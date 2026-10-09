// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
using strings = go.strings_package;
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static go.archive.zip_package;
using static go.archive.zip_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b696f2e5772697465727d", "TestWriterFlush_w")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<TestWriterFlush_w, io_package.Writer>(Promoted = true)]
[assembly: GoImplement<TestWriterFlush_w, io_package.Writer>]
[assembly: GoImplement<fakeHash32, hash_package.Hash32>(Promoted = true)]
[assembly: GoImplement<fakeHash32, hash_package.Hash32>]
[assembly: GoImplement<rleBuffer, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<rleBuffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<rleBuffer, sizedReaderAt>(Pointer = true)]
[assembly: GoImplement<suffixSaver, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<suffixSaver, sizedReaderAt>(Pointer = true)]
[assembly: GoImplement<zeros, io_package.Reader>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<WriteTest, ж<WriteTest>>(Indirect = true)]
[assembly: GoImplicitConv<rleBuffer, ж<rleBuffer>>(Indirect = true)]
[assembly: GoImplicitConv<suffixSaver, ж<suffixSaver>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("archive/zip/fuzz_test.go", "fuzz_test.cs", "ABkeooKClIKClIKClJaCgoKWjIKCgpSCgpSCgILcgpaCgoKClICCuICC", "31-80:1")]
[assembly: go.GoPositionMap("archive/zip/reader_test.go", "reader_test.cs", "AFdyAPYDngiCspLKwoKCgoKCgoKAgoK2goKCgoKUgpSCgoKSlIKCgoKmgoKogrqCloKUgqiClIKogoKCssKClKaCtKSCgoKmsoKUgpSCloSCgpKCqIKCgpSCgoKUgoKClIKCgoKCgpaCgoLMgoKUgpaCgoKUgpSEgIKmgoKSgIKCpoKCloKCgsqigoKSgriCgpaCgqiCgoKUgoKogoK4goKClIKmgpS4gqaCAAMaAAkCAB46goKCgpSmgoIAAoYBAEACAFGgAYKCgoKUpoKCgoKClIKClIKCpqaUAAoWgoKCgsySAAAegoKUgoKClIKCgpSCprqShoKClIK6koyCgpSCgpSCgpSokgAcOoKCAAgIggAJGpKygoKClJKAgsQADgiCAA0kkpKCgoKUgoKSgoKUgpSClIKUgpKClIIADQyigoKClJQACRqCgoKUgILW9IKUABYugoKUgoKUgpSClICC+KKUABcygoKClIKCpoKUgoKUgpSClICCxKTYABs4goK6goKCgoKmgIKkgoKUgri4AAcQgoK4ggALGAA8eoKClIKCgoKAgqSAgraClICCpICCpIKClIKCgoKUlIKClICC+KKCgpSUgpaCsoKClJSCgrTEpKKCgoKUlJSEsoSCgpSUgoK01qSCgtyCgoKCgpSEgoKClIKClIKCAAoKgoKCgoKCgpSCgoKUgoKUgILIAAkUACtYgoKUgoKClICC2rQAGziCgILsotSCgoSCksyAgqSEgoKClLiCgoSCkoTMgIKkhIKCgpS4goKEgpLMgIKkhIKCgpQ=", "586-588:1;664-667:1;826-829:1;833-845:1;1217-1227:1;1251-1281:1;1259-1269:1.1;1626-1637:1;1653-1666:1;1826-1830:1", "", "88=timeZone/1/38/8,Date/1/52/8,timeZone/2/38/14,Date/2/52/14,timeZone/3/38/26,Date/3/52/26,timeZone/4/38/32,Date/4/52/32,timeZone/5/38/44,Date/5/52/44,timeZone/6/38/50,Date/6/52/50,timeZone/7/38/62,Date/7/52/62,timeZone/8/38/68,Date/8/52/68,timeZone/9/38/80,Date/9/52/80,timeZone/10/38/86,Date/10/52/86,rZipBytes/1/1/97,Date/11/52/98,timeZone/11/38/109,Date/12/52/109,Date/13/52/127,Date/14/52/139,Date/15/52/145,Date/16/52/151,Date/17/52/157,timeZone/12/38/169,Date/18/52/169,timeZone/13/38/175,Date/19/52/175,timeZone/14/38/181,Date/20/52/181,timeZone/15/38/187,Date/21/52/187,timeZone/16/38/204,Date/22/52/204,timeZone/17/38/210,Date/23/52/210,Date/24/52/223,Date/25/52/229,Date/26/52/241,Date/27/52/248,timeZone/18/38/261,Date/28/52/261,timeZone/19/38/267,Date/29/52/267,timeZone/20/38/281,Date/30/52/281,timeZone/21/38/288,Date/31/52/288,Date/32/52/299,timeZone/22/38/311,Date/33/52/311,Date/34/52/325,timeZone/23/38/337,Date/35/52/337,timeZone/24/38/353,Date/36/52/353,timeZone/25/38/366,Date/37/52/366,timeZone/26/38/377,Date/38/52/377,timeZone/27/38/388,Date/39/52/388,timeZone/28/38/399,Date/40/52/399,timeZone/29/38/411,Date/41/52/411,timeZone/30/38/423,Date/42/52/423,Date/43/52/435,timeZone/31/38/447,Date/44/52/447,timeZone/32/38/459,Date/45/52/459,timeZone/33/38/471,Date/46/52/471,timeZone/34/38/483,Date/47/52/483,Date/48/52/484,timeZone/35/38/495,Date/49/52/495,timeZone/36/38/501,Date/50/52/501,timeZone/37/38/507,Date/51/52/507,timeZone/38/38/513,Date/52/52/513")]
[assembly: go.GoPositionMap("archive/zip/writer_test.go", "writer_test.cs", "ACVAAC9aooKAgqSCgqiChLKWgIK4goKUsrT2kgAFFJSCgoCCgpSUgriAgqaCqIKogoKUggALCoIAKFyChILcgoKUloCCuIKClIKCggAICoKCuIKAgqSAgqaCgpSAgoLIooKAgqSCgqiCgoKChLKWgIK4goKUsrQACgSCgoKCgpSClICCpIL4goKCgpSAgqSAggAICIKCgoDcpICCpISChIKClISSlpKWgoK4lIKCspSAgriCgpSyqIKCgoCCtoCCuIKClLIADQiCAA0ugoSCgoKCgoSCgoKCgpSCgpSCgpSClgAHEIKClIKUlIKogIK4goKUgoKClIKUgpSClIKUgpaCgoKWgoKCloLKsriClIKClIKCuLKClIKCgpSCgpSCgpSCuKKEkoKCgriUlriChJKCgsqCgoLKpqKCgu6CgpSAgriWgoKUsoKUuIKCggANHIKC", "83-85:1;271-273:1;579-590:1;599-604:2", "", "136=Repeat/1/2/6,Repeat/2/2/7;269=timeZone/1/1/2,Date/1/1/2;501=Repeat/1/1/17")]
[assembly: go.GoPositionMap("archive/zip/zip_test.go", "zip_test.cs", "ABwwgoKUgoKCgsiCpoCCpIKCgpSAgqSCgoLKooKCgoKCuLKCgoKUgJKkgIKkgIKkgIKkgIKmgILIgtymgtymgu6CgoKUgIKkgILIgtyCgoKUgIKkgIIAESKCgpSCpoKCgpSAgoKCyoKCgpSmpoKCuIKCuLKClIaCgoKCgoKCgpSmgpSokoKCgoKClIKWgoKCgoKUggAKFoCigNSCgpSCgoKmgoKUjNKCAAsKooKUgoIAGTCSgoKmkoKCAAgMsoKUggAQIpKCgriSgoIADh6ApJSygoKUgoKClKaCgpSCgoKmsoKCgoKCgpSClIKCgoKCpqqigoKCAAgQgoKAgqaCgoKWgoKUgpaCgIKmgoCCpoKClPiSgpSClKYAIEKSgoKmkoKCAA0KgoKUgoK4gpSCgoKUgoKCpoCCgoK2goKClICCuIKClIKCgpSCgoKCpoCCgoK2goKUgpSCgpSCgIK4gIKm2JKCuKKChIKClICCpICCpoKCgpSCguqShIKE6oSmguoACAaCAAwkgoSCuIKCqICCyKLqqqIAChCqwoK4goKCgoLugoKCgoCCpIKAgqSAgqSCgoKCgoKClJSCgoIACBKCgg==", "218-220:1;308-331:1;309-330:1.1;310-314:1.1.1;332-337:2;338-343:3;352-367:1;353-366:1.1;369-374:2;376-381:3;506-538:1;507-537:1.1;508-512:1.1.1;539-544:2;545-550:3;769-775:1;770-774:1.1", "", "37=Sprintf/1/1/1;144=Now/1/1/3,Local/1/1/3;785=Format/1/1/3;814=Repeat/1/2/6,Repeat/2/2/12")]
// </GoSourcePositionMaps>

namespace go.archive;

[GoPackage("zip")]
public static partial class zip_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("file")] partial struct FuzzReader_file {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcompressꓸflate() => builtin.initPackage(typeof(compress.flate_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸhash() => builtin.initPackage(typeof(hash_package));
    [GoInit] internal static void initᴛᴛimportꓸhashꓸcrc32() => builtin.initPackage(typeof(go.hash.crc32_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸobscuretestdata() => builtin.initPackage(typeof(@internal.obscuretestdata_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(go.path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸfstest() => builtin.initPackage(typeof(go.testing.fstest_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}
