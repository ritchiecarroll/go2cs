// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.image.gif_package;

// <ImportedTypeAliases>
global using colorꓸRGBA = go.image.color_package.ΔRGBA;
global using imageꓸRGBA = go.image_package.ΔRGBA;
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
using static global::go.image.gif_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b66696c656e616d6520737472696e673b20746f6c6572616e636520696e7436347d", "testCaseᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<global::go.image.gif_package.blockReader, io_package.ByteReader>(Pointer = true)]
[assembly: GoImplement<global::go.image.gif_package.blockWriter, global::go.image.gif_package.writer>]
[assembly: GoImplement<image_package.Paletted, go.image.draw_package.Image>(Pointer = true)]
[assembly: GoImplement<image_package.ΔRGBA, go.image.draw_package.Image>(Pointer = true)]
[assembly: GoImplement<offsetImage, image_package.Image>(Promoted = true)]
[assembly: GoImplement<offsetImage, image_package.Image>]
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
[assembly: go.GoPositionMap("image/gif/fuzz_test.go", "fuzz_test.cs", "ABggooKWgoKUgoKUgoKUloKCgpSClIKClKKCgoKUgoKUgoKC", "36-64:1")]
[assembly: go.GoPositionMap("image/gif/reader_test.go", "reader_test.cs", "ACRClpKCgoCCpICCpAAPBoi0ABM4goKCyoKCgoKUqJaolpSClIKEgoKogpQABhKC3IKCgoKClIKCpoKCgpSCgpSEgoKUgoKCyoKUgoKCAA0MAA0ggoKCgpSC6JSClIKCqIKCgpaCgpSmgqiClpaCgoKEhNaCgpaClpaCgoKEloKClLiCloIABxKWloKCgoSEAAwGggAcRLKSgoKUgoKClIKClIKUggAJDIKCgoKUgoKUgvyygoLKgoKUgoCCpIKCgoKAgqSCgILE1KKCgpSCgpSCgoKCuIKCgpSCgpSCgg==", "357-377:1")]
[assembly: go.GoPositionMap("image/gif/writer_test.go", "writer_test.cs", "ABcqooKClJKC1qKCgpSS1oKCgpSqooKqopKCgoKCgoKCgoKmyuQABRKCgoKCgpSCgoKClIKCgpSCgqaCgoLKgoKClIKCgoKUgoKUgpSCvKKCgpSCgqaCgoKmgoKCpqb6gpTKgoKClIKCpu6CgpSCgoKUypaCgIKkgoKClIKCloKUgpSAkqaClIKUgpSClIKWgoKClJKClIKClILKgKKAooDkgoKClriAgqbKgpSAgviCgILIgsqCAAgSgoKCpoLcou6CuIKAgoKkgoKClICC3IKCuIKCgoKChIKAgoKkgoKClICCgqaSlLqAgqSAgtoACRSCpriCgoKmgtyCkgAPJgAOIIKAgqSCgpSClIKAgtqikoKCgoKCqAALGoKCggAJDIKCgoCCyoKEgoKCgIKkgIKkgpSCuKaCAAoWgoKCgIKCpICCAAoUgqaCgoKogoKCgpSCgpSCgqgACRSCgoKClIKClgAJFIKCuKKCgoKWgoKCgriigoKCgoIABxKCgoKC6KKCgpSCgoSCgoKCuKKCgpTKgoSCgoKC", "398-402:1")]
// </GoSourcePositionMaps>

namespace go.image;

[GoPackage("gif")]
public static partial class gif_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸcompressꓸlzw() => builtin.initPackage(typeof(compress.lzw_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸimage() => builtin.initPackage(typeof(image_package));
    [GoInit] internal static void initᴛᴛimportꓸimageꓸcolor() => builtin.initPackage(typeof(go.image.color_package));
    [GoInit] internal static void initᴛᴛimportꓸimageꓸcolorꓸpalette() => builtin.initPackage(typeof(go.image.color.palette_package));
    [GoInit] internal static void initᴛᴛimportꓸimageꓸdraw() => builtin.initPackage(typeof(go.image.draw_package));
    [GoInit] internal static void initᴛᴛimportꓸimageꓸpng() => builtin.initPackage(typeof(go.image.png_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸruntimeꓸdebug() => builtin.initPackage(typeof(go.runtime.debug_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.image.gif_package));
    }
}
