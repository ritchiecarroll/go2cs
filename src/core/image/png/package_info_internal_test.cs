// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.image.png_package;
using static go.image.png_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b66696c6520737472696e673b2065727220737472696e677d", "readerErrorsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<pool, global::go.image.png_package.EncoderBufferPool>(Pointer = true)]
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
[assembly: go.GoPositionMap("image/png/fuzz_test.go", "fuzz_test.cs", "ABggooKWgoKUgoKUgoKUloKCgpSClIKClNyygoKCgoKUgoKClIKCgg==", "36-73:1")]
[assembly: go.GoPositionMap("image/png/paeth_test.go", "paeth_test.cs", "AA0agoKUqqKCgoKCgqSUqJKClIK4goKCgoKCgu6igriCgoKCgoKCgoKCgoKUgoKCgoKCgoI=")]
[assembly: go.GoPositionMap("image/png/reader_test.go", "reader_test.cs", "ABYoACVMAAcQ3KKCgpSSAAgMABAiAA0cABQWsoKCgoKUpKSCgoKUtLS0tKiCgoCClJS0tLS01rqAgpS4goKCgoKylLTEpIKUlIKAgqSCgoKCgpS2gILIlIKCgpjEpMaCgoKYxKTcgoKUgoLGgoLGgoLGgoLGgoKUpIKU+oKClKSClPqSgoKCgoKCpoKCgpTGlKLUooKClJSCgoKWpoKClJaCgoKmgoKClJKWgoKCgpSCgpSCAAgUgoCCuIKCpoKUgsb0AAYUgoKCgoKUgpSCyqKCgoKClJKCgoKUgoKClIKCxuSCgoKUgoKUggAPCIj6goIAFQiGAAgMgoKapIKClIIAEggAABoAFA6CgoKCgoKUgoSCgpSCgpS0goKUtIKUtICC2qLKgoLolIKCgqiCgIIACgiUAA4egpaCgoKCgoCCzO6WAAAaggANCKKChAA9kAGCgoKCooKkpIKkgqSCloDcAAgQuICCuIK4AAoWABk2goKigqaCgqKCyKKCgpSCgpSCgoKC6ILWgtaC1oLWgtaC")]
[assembly: go.GoPositionMap("image/png/writer_test.go", "writer_test.cs", "ABUmgoKClIKCgoKCgoKCgrimgoKCgpSmgoKCgqaUgoKUgpSCgoKmgoKClIKCgqaCgoIACgqClAAaSrKkgoLuhIKCgoKqkoCCgqSCgoSCgoSUgoK4gpKClIKClIK4AAoOgoSSgIKkgoCCpoKUgIKkgILIgoKCgqaCgoKClIKCggAICIKSgoKCgoKCgoKCgoK6AAUUspKCgoKUgoLcooKCgoKCAAcQgqaipqKCtoKCgoLoopSCgoKmgpSCgoKC6KKCgpSCgoKCuKKYgoKCgriilIKCgqaClIKCgoK4opKCgoKClMTE6IKUgoKCgg==", "131-190:1;267-277:1")]
// </GoSourcePositionMaps>

namespace go.image;

[GoPackage("png")]
public static partial class png_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcompressꓸzlib() => builtin.initPackage(typeof(compress.zlib_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbase64() => builtin.initPackage(typeof(encoding.base64_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸimage() => builtin.initPackage(typeof(image_package));
    [GoInit] internal static void initᴛᴛimportꓸimageꓸcolor() => builtin.initPackage(typeof(go.image.color_package));
    [GoInit] internal static void initᴛᴛimportꓸimageꓸdraw() => builtin.initPackage(typeof(go.image.draw_package));
    [GoInit] internal static void initᴛᴛimportꓸimageꓸpng() => builtin.initPackage(typeof(go.image.png_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
}
