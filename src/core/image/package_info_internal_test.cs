// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.image_package;
using static go.image_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("696e746572666163657b536574524742413634287820696e742c207920696e742c206320696d6167652f636f6c6f722e524742413634297d", "TestRGBA64Image_type")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20696d6167652066756e63282920696d6167652e696d6167657d", "testImagesᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<global::go.image_package.Alpha, image>(Pointer = true)]
[assembly: GoImplement<global::go.image_package.Alpha16, image>(Pointer = true)]
[assembly: GoImplement<global::go.image_package.Gray, image>(Pointer = true)]
[assembly: GoImplement<global::go.image_package.Gray16, image>(Pointer = true)]
[assembly: GoImplement<global::go.image_package.NRGBA, image>(Pointer = true)]
[assembly: GoImplement<global::go.image_package.NRGBA64, image>(Pointer = true)]
[assembly: GoImplement<global::go.image_package.Paletted, image>(Pointer = true)]
[assembly: GoImplement<global::go.image_package.RGBA64, image>(Pointer = true)]
[assembly: GoImplement<global::go.image_package.ΔRGBA, image>(Pointer = true)]
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
[assembly: go.GoPositionMap("image/geom_test.go", "geom_test.cs", "AAsYtIKClIKCgoK4lgAOIIKCgoKC3oKCgoCCpICCpICCtoKCgoKCgpSUggAHEoKCgoCCpICCpJSUgoKCgoKCgg==", "14-27:1", "", "30=Rect/1/11/1,Rect/2/11/2,Rect/3/11/3,Rect/4/11/4,Rect/5/11/5,Rect/6/11/6,Rect/7/11/7,Rect/8/11/8,Rect/9/11/9,Rect/10/11/10,Rect/11/11/11")]
[assembly: go.GoPositionMap("image/image_test.go", "image_test.cs", "ABYogoKCAAgGAAwogoKCgoKUgoKUgoKClIKClIKCgpSCgpSCgpSCgoKmgoKCAAgItLKCgqaC1piioqKioqKioqKioqamgoK4gpSCloKCggAGEIKCgoK4gtyCnIKCgoKCppyCgoKCggAUCpSCgqiCAAEgggABILaCgoK2toKC/qK4goKClIKChKjKgrKSgoKCgtyCkrKigoKCgtyigoSCuKKCgoSCuKKChIK4ooKChIK4ooKEgriigoKEgriigoSCuKKCgoSCuKKChIK4ooKChIK4ooKEgriigoKEgriigoSCuKKCgoSCuKKChIK4ooKChII=", "94-102:1;95-99:1.1;108-108:2;109-109:3;110-110:4;111-111:5;112-112:6;113-113:7;114-114:8;115-115:9;116-116:10;117-117:11;118-118:12;119-119:13;197-201:1;283-290:1;297-304:1", "", "208=Rect/1/4/1,NewRGBA64/1/1/1,Rect/2/4/2,NewNRGBA64/1/1/2,Rect/3/4/3,NewAlpha16/1/1/3,Rect/4/4/4,NewGray16/1/1/4;244=NewAlpha/1/1/1,NewAlpha16/1/1/2,NewCMYK/1/1/3,NewGray/1/1/4,NewGray16/1/1/5,NewNRGBA/1/1/6,NewNRGBA64/1/1/7,NewNYCbCrA/1/1/8,NewPaletted/1/1/9,NewRGBA/1/1/10,NewRGBA64/1/1/11,NewUniform/1/1/12,NewYCbCr/1/1/13")]
[assembly: go.GoPositionMap("image/ycbcr_test.go", "ycbcr_test.cs", "AAwYggAVLAAHENyCgoKmgsqUgpaClLqCgoKCgoK6goKCgoKWgoKCgoKUAAkUgoKCyoKCgqaCgoKC", "", "", "14=Rect/1/20/1,Rect/2/20/2,Rect/3/20/3,Rect/4/20/4,Rect/5/20/5,Rect/6/20/6,Rect/7/20/7,Rect/8/20/8,Rect/9/20/9,Rect/10/20/10,Rect/11/20/11,Rect/12/20/12,Rect/13/20/13,Rect/14/20/14,Rect/15/20/15,Rect/16/20/16,Rect/17/20/17,Rect/18/20/18,Rect/19/20/19,Rect/20/20/20;44=Pt/1/4/1,Pt/2/4/2,Pt/3/4/3,Pt/4/4/4")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("image")]
public static partial class image_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbase64() => builtin.initPackage(typeof(encoding.base64_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸimage() => builtin.initPackage(typeof(image_package));
    [GoInit] internal static void initᴛᴛimportꓸimageꓸcolor() => builtin.initPackage(typeof(go.image.color_package));
    [GoInit] internal static void initᴛᴛimportꓸimageꓸcolorꓸpalette() => builtin.initPackage(typeof(global::go.image.color.palette_package));
    [GoInit] internal static void initᴛᴛimportꓸimageꓸgif() => builtin.initPackage(typeof(go.image.gif_package));
    [GoInit] internal static void initᴛᴛimportꓸimageꓸjpeg() => builtin.initPackage(typeof(go.image.jpeg_package));
    [GoInit] internal static void initᴛᴛimportꓸimageꓸpng() => builtin.initPackage(typeof(go.image.png_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
}
