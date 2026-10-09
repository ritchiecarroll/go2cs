// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.image.draw_package;
using static go.image.draw_internal_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<embeddedPaletted, global::go.image.draw_package.Image>]
[assembly: GoImplement<slowerRGBA, global::go.image.draw_package.Image>(Pointer = true)]
[assembly: GoImplement<slowerRGBA, image_package.Image>(Pointer = true)]
[assembly: GoImplement<slowestRGBA, global::go.image.draw_package.Image>(Pointer = true)]
[assembly: GoImplement<slowestRGBA, image_package.Image>(Pointer = true)]
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
[assembly: go.GoPositionMap("image/draw/bench_test.go", "bench_test.cs", "ABMmAAcQ0oSClIKCggAHEKSCgoIABxCoooKCgqaUuIKUpIKCggAHEKSCgoLKpIKCggAHEKSCgoIABxCkgoKCAAcQpIKCgoKCgpQAChSmgriSgoKmgrikpoKUgoS8kqaCpoKmgqaCpoKmgqaCpoKmgqaCpoKmgqaCpoKmgqqSpoKmgqaC", "", "", "162=Rect/1/1/7")]
[assembly: go.GoPositionMap("image/draw/clip_test.go", "clip_test.cs", "ABYqAIQBkAKCgoKCgoKC0oKUqIKClIKClIKCgsyCpoKClJSCgg==", "", "", "23=Rect/1/41/4,Rect/2/41/5,Rect/3/41/6,Rect/4/41/11,Rect/5/41/17,Rect/6/41/18,Rect/7/41/19,Rect/8/41/24,Pt/1/17/25,Rect/9/41/30,Rect/10/41/31,Rect/11/41/32,Rect/12/41/37,Pt/2/17/38,Rect/13/41/43,Rect/14/41/44,Rect/15/41/45,Rect/16/41/50,Pt/3/17/51,Rect/17/41/56,Rect/18/41/57,Rect/19/41/58,Pt/4/17/60,Rect/20/41/63,Pt/5/17/64,Rect/21/41/69,Rect/22/41/70,Rect/23/41/71,Pt/6/17/73,Rect/24/41/76,Pt/7/17/77,Rect/25/41/82,Rect/26/41/83,Rect/27/41/84,Pt/8/17/86,Pt/9/17/90,Rect/28/41/95,Rect/29/41/96,Rect/30/41/97,Pt/10/17/99,Rect/31/41/102,Pt/11/17/103,Rect/32/41/110,Rect/33/41/111,Rect/34/41/112,Rect/35/41/113,Rect/36/41/117,Pt/12/17/118,Pt/13/17/119,Rect/37/41/123,Rect/38/41/124,Rect/39/41/125,Rect/40/41/126,Pt/14/17/127,Pt/15/17/128,Rect/41/41/130,Pt/16/17/131,Pt/17/17/132")]
[assembly: go.GoPositionMap("image/draw/draw_test.go", "draw_test.cs", "ABw0gKSApIKmgoKUgoKCgoKCAAcQgoKUgoKCgoKCpoKmgoCC7IKC7oKCgIIADhyApICkgqaCgpSCgoKCgoIABxCCgpSCgoKCgoKmgoKUgoKCgoKmgqaCgILsgoLugoKAgsiCgoKmgqaCpoKCgoKmpoKCgoKmpoKCgoKmpoIACBKCgqamgoKCgqamgoKCgqamgoKCgqamgoKCgqYANBYATO4BpoKCgoKUgoKCgoKClIKClIKCloKygpSCgoKUggAHEKaCAAoWgoKCgriUtMiCgoKUpoKmgpS4goKClAAKEoKCgoKCgoKClIKCgqaUgoKCggAKFJKCgoKCgoKCggAJCIIADh6ygpKSgrKCgoKC3IKCppSCgoKUlIKCgpSUgoKCuIIAFziCgoKCgoKEgoSAgqSAgqSAgsqCgoKCgoKAggAIENKUgoKCgoKCgoKCgoCCABAewoKClJKCgpSEnJiagoKCgoKCgoKCgpQACQykzIKCgpSUlAAQIoKCgILIgII=", "586-595:1;774-782:1", "", "246=Rect/1/1/7;353=vgradGreen/1/9/2,fillAlpha/1/34/2,vgradGreen/2/9/3,fillAlpha/2/34/3,fillBlue/1/12/8,fillAlpha/3/34/8,fillBlue/2/12/9,fillAlpha/4/34/9,fillBlue/3/12/10,fillAlpha/5/34/10,fillBlue/4/12/11,fillAlpha/6/34/11,fillBlue/5/12/12,fillBlue/6/12/13,vgradGreen/3/9/18,fillAlpha/7/34/18,vgradGreen/4/9/19,fillAlpha/8/34/19,vgradGreen/5/9/20,fillAlpha/9/34/20,vgradGreen/6/9/21,fillAlpha/10/34/21,vgradGreen/7/9/22,vgradGreen/8/9/23,vgradGreenNRGBA/1/6/29,fillAlpha/11/34/29,vgradGreenNRGBA/2/6/30,fillAlpha/12/34/30,vgradGreenNRGBA/3/6/31,fillAlpha/13/34/31,vgradGreenNRGBA/4/6/32,fillAlpha/14/34/32,vgradGreenNRGBA/5/6/33,vgradGreenNRGBA/6/6/34,vgradCr/1/6/39,fillAlpha/15/34/39,vgradCr/2/6/40,fillAlpha/16/34/40,vgradCr/3/6/41,fillAlpha/17/34/41,vgradCr/4/6/42,fillAlpha/18/34/42,vgradCr/5/6/43,vgradCr/6/6/44,vgradGray/1/19/49,fillAlpha/19/34/49,vgradGray/2/19/50,fillAlpha/20/34/50,vgradGray/3/19/51,fillAlpha/21/34/51,vgradGray/4/19/52,fillAlpha/22/34/52,vgradGray/5/19/53,vgradGray/6/19/54,vgradGray/7/19/56,convertToSlowerRGBA/1/8/56,fillAlpha/23/34/56,vgradGray/8/19/58,convertToSlowerRGBA/2/8/58,fillAlpha/24/34/58,vgradGray/9/19/60,convertToSlowerRGBA/3/8/60,fillAlpha/25/34/60,vgradGray/10/19/62,convertToSlowerRGBA/4/8/62,fillAlpha/26/34/62,vgradGray/11/19/64,convertToSlowerRGBA/5/8/64,vgradGray/12/19/66,convertToSlowerRGBA/6/8/66,vgradGray/13/19/69,convertToSlowestRGBA/1/8/69,fillAlpha/27/34/69,vgradGray/14/19/71,convertToSlowestRGBA/2/8/71,fillAlpha/28/34/71,vgradGray/15/19/73,convertToSlowestRGBA/3/8/73,fillAlpha/29/34/73,vgradGray/16/19/75,convertToSlowestRGBA/4/8/75,fillAlpha/30/34/75,vgradGray/17/19/77,convertToSlowestRGBA/5/8/77,vgradGray/18/19/79,convertToSlowestRGBA/6/8/79,vgradMagenta/1/6/85,fillAlpha/31/34/85,vgradMagenta/2/6/86,fillAlpha/32/34/86,vgradMagenta/3/6/87,fillAlpha/33/34/87,vgradMagenta/4/6/88,fillAlpha/34/34/88,vgradMagenta/5/6/89,vgradMagenta/6/6/90,fillBlue/7/12/96,vgradAlpha/1/8/96,fillBlue/8/12/97,vgradAlpha/2/8/97,fillBlue/9/12/99,vgradAlpha/3/8/99,convertToSlowerRGBA/7/8/99,fillBlue/10/12/101,vgradAlpha/4/8/101,convertToSlowerRGBA/8/8/101,fillBlue/11/12/104,vgradAlpha/5/8/104,convertToSlowestRGBA/7/8/104,fillBlue/12/12/106,vgradAlpha/6/8/106,convertToSlowestRGBA/8/8/106,vgradGreen/9/9/115,vgradAlpha/7/8/115,vgradGray/19/19/116,vgradAlpha/8/8/116;481=Rect/1/9/1,Rect/2/9/2,Rect/3/9/3,Rect/4/9/4,Rect/5/9/5,Rect/6/9/6,Rect/7/9/7,Rect/8/9/8,Rect/9/9/9;514=Bounds/1/2/1,Bounds/2/2/1;524=At/1/1/1;533=At/1/2/1,At/2/2/1;600=Rect/1/13/1,Rect/2/13/2,Rect/3/13/3,Rect/4/13/4,Rect/5/13/5,Rect/6/13/6,Rect/7/13/7,Rect/8/13/8,Rect/9/13/9,Rect/10/13/10,Rect/11/13/11,Rect/12/13/12,Rect/13/13/13;804=At/1/2/1,At/2/2/1")]
// </GoSourcePositionMaps>

namespace go.image;

[GoPackage("draw")]
public static partial class draw_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸimage() => builtin.initPackage(typeof(image_package));
    [GoInit] internal static void initᴛᴛimportꓸimageꓸcolor() => builtin.initPackage(typeof(go.image.color_package));
    [GoInit] internal static void initᴛᴛimportꓸimageꓸpng() => builtin.initPackage(typeof(go.image.png_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸquick() => builtin.initPackage(typeof(go.testing.quick_package));
    // </ImportInitializers>
}
