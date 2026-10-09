// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.math.big_package;
global using static global::go.math.big_internal_test_package;

// <ImportedTypeAliases>
global using bigꓸInt = go.math.big_package.ΔInt;
global using bigꓸRat = go.math.big_package.ΔRat;
global using execꓸError = go.os.exec_package.ΔError;
global using flagꓸErrorHandling = go.flag_package.ΔErrorHandling;
global using jsonꓸToken = object;
global using jsonꓸΔToken = object;
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
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
global using xmlꓸToken = object;
global using xmlꓸΔToken = object;
using testing = go.testing_package;
using Δrand = go.math.rand_package;
// </ImportedTypeAliases>

using go;
using static global::go.math.big_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b6120696e7436343b206220696e7436343b206f757420737472696e677d", "setFrac64Testsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6120696e7436343b206220696e7436343b2070726f6420737472696e677d", "mulRangesZᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b612075696e7436343b20622075696e7436343b2070726f6420737472696e677d", "mulRangesNᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6420737472696e673b207820737472696e673b207920737472696e673b206120737472696e673b206220737472696e677d", "gcdTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b656c656d656e7420737472696e673b206d6f64756c757320737472696e677d", "modInverseTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f757420696e747d", "bitLenTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f757420737472696e673b206261736520696e743b2076616c20696e7436343b206f6b20626f6f6c7d", "stringTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f757420737472696e677d", "notTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f75742075696e747d", "tzbTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b207072656320696e743b206f757420737472696e677d", "floatStringTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e70757420737472696e673b20666f726d617420737472696e673b206f757470757420737472696e673b2072656d61696e696e6720696e747d", "scanTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e70757420737472696e673b20666f726d617420737472696e673b206f757470757420737472696e677d", "formatTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7261743120737472696e673b207261743220737472696e673b206f757420696e747d", "ratCmpTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7320737472696e673b206261736520696e743b206672616320626f6f6c3b2078206d6174682f6269672e6e61743b206220696e743b20636f756e7420696e743b20657272206572726f723b206e6578742072756e657d", "natScanTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7320737472696e673b2062617365326f6b20626f6f6c3b207365704f6b20626f6f6c3b207820696e7436343b206220696e743b20657272206572726f723b206e6578742072756e657d", "exponentTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820696e7436343b207920696e7436343b207120696e7436343b207220696e7436343b206420696e7436343b206d20696e7436347d", "divisionSignsTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b78206d6174682f6269672e576f72643b2079206d6174682f6269672e576f72643b2063206d6174682f6269672e576f72643b2071206d6174682f6269672e576f72643b2072206d6174682f6269672e576f72647d", "mulAddWWWTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b78206d6174682f6269672e576f72643b2079206d6174682f6269672e576f72643b2071206d6174682f6269672e576f72643b2072206d6174682f6269672e576f72647d", "mulWWTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b78206d6174682f6269672e6e61743b206220696e743b207320737472696e677d", "strTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b78206d6174682f6269672e6e61743b2079206d6174682f6269672e6e61743b207220696e747d", "cmpTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820737472696e673b206920696e743b20622075696e747d", "bitsetTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820737472696e673b20692075696e743b2077616e742075696e747d", "bitTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820737472696e673b207920737472696e673b20616e6420737472696e673b206f7220737472696e673b20786f7220737472696e673b20616e644e6f7420737472696e677d", "bitwiseTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820737472696e673b207920737472696e673b206d20737472696e673b206b302075696e7436343b206f7574333220737472696e673b206f7574363420737472696e677d", "montgomeryTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820737472696e673b207920737472696e673b206d20737472696e673b206f757420737472696e677d", "expTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820737472696e673b207920737472696e673b206e2075696e743b207a20737472696e677d", "subMod2NTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820737472696e673b207920737472696e673b207120737472696e673b207220737472696e677d", "quoTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820737472696e673b207920737472696e673b2073756d20737472696e673b2070726f6420737472696e677d", "ratBinTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7831206d6174682f6269672e576f72643b207830206d6174682f6269672e576f72643b2079206d6174682f6269672e576f72643b2071206d6174682f6269672e576f72643b2072206d6174682f6269672e576f72647d", "divWWTestsᴛ1")]
[assembly: GoTypeAlias("Bits", "ΔBits")]
[assembly: GoTypeAlias("Int", "ΔInt")]
[assembly: GoTypeAlias("Rat", "ΔRat")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.ByteScanner>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<global::go.math.big_package.ErrNaN, error>]
[assembly: GoImplement<go.math.rand_package.Rand, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<strings_package.Reader, io_package.ByteScanner>(Pointer = true)]
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<global::go.math.big_package.ΔInt, ж<global::go.math.big_package.ΔInt>>(Indirect = true)]
[assembly: GoImplicitConv<global::go.math.big_package.ΔRat, ж<global::go.math.big_package.ΔRat>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("math/big/alias_test.go", "alias_test.cs", "ABIggu6igoKCpqKCgpTuooKClIKU7oKC7qKCgpTuou6iAAIYAAwCloCCuICCgrgAAhwADwKWgKaSgriCgIKCtoKAgoLcgoKAgpKCuICCgraCgIKCuKaCABsiuroABBSCooK4gqKCuAAQTAAEEAAHHJKSlKSkpKSApg==", "183-185:1;186-188:2;189-191:3;192-194:4;195-197:5;198-202:6;199-201:6.1;203-207:7;204-206:7.1;208-212:8;209-211:8.1;213-217:9;214-216:9.1;218-224:10;220-223:10.1;225-231:11;227-230:11.1;232-236:12;233-235:12.1;237-239:13;240-242:14;243-245:15;246-248:16;249-251:17;252-254:18;255-257:19;258-260:20;261-266:21;262-265:21.1;267-269:22;270-274:23;271-273:23.1;275-277:24;278-282:25;279-281:25.1;283-285:26;286-288:27;289-291:28;293-310:29")]
[assembly: go.GoPositionMap("math/big/example_rat_test.go", "example_rat_test.cs", "AAwoAAgCgoKUloK6hKyygro=")]
// </GoSourcePositionMaps>

namespace go.math;

[GoPackage("big_test")]
public static partial class big_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct bigInt {}
    internal partial struct notZeroInt {}
    internal partial struct positiveInt {}
    internal partial struct prime {}
    internal partial struct smallUint {}
    internal partial struct zeroOrOne {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸgob() => builtin.initPackage(typeof(encoding.gob_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸxml() => builtin.initPackage(typeof(encoding.xml_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(go.math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(go.math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(go.math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸquick() => builtin.initPackage(typeof(go.testing.quick_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.math.big_package));
    }
}
