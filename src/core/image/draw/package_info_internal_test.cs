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
[assembly: go.GoPositionMap("image/draw/bench_test.go", "bench_test.cs", "ABMmAAcQ0oSClIKCggAHEKSCgoIABxCoooKCgqaUuIKUpIKCggAHEKSCgoLKpIKCggAHEKSCgoIABxCkgoKCAAcQpIKCgoKCgpQAChSmgriSgoKmgrikpoKUgoS8kqaCpoKmgqaCpoKmgqaCpoKmgqaCpoKmgqaCpoKmgqqSpoKmgqaC")]
[assembly: go.GoPositionMap("image/draw/clip_test.go", "clip_test.cs", "ABYqAIQBkAKCgoKCgoKC0oKUqIKClIKClIKCgsyCpoKClJSCgg==")]
[assembly: go.GoPositionMap("image/draw/draw_test.go", "draw_test.cs", "ABw0gKSApIKmgoKUgoKCgoKCAAcQgoKUgoKCgoKCpoKmgoCC7IKC7oKCgIIADhyApICkgqaCgpSCgoKCgoIABxCCgpSCgoKCgoKmgoKUgoKCgoKmgqaCgILsgoLugoKAgsiCgoKmgqaCpoKCgoKmpoKCgoKmpoKCgoKmpoIACBKCgqamgoKCgqamgoKCgqamgoKCgqamgoKCgqYANBYATO4BpoKCgoKUgoKCgoKClIKClIKCloKygpSCgoKUggAHEKaCAAoWgoKCgriUtMiCgoKUpoKmgpS4goKClAAKEoKCgoKCgoKClIKCgqaUgoKCggAKFJKCgoKCgoKCggAJCIIADh6ygpKSgrKCgoKC3IKCppSCgoKUlIKCgpSUgoKCuIIAFziCgoKCgoKEgoSAgqSAgqSAgsqCgoKCgoKAggAIENKUgoKCgoKCgoKCgoCCABAewoKClJKCgpSEnJiagoKCgoKCgoKCgpQACQykzIKCgpSUlAAQIoKCgILIgII=", "586-595:1;774-782:1")]
// </GoSourcePositionMaps>

namespace go.image;

[GoPackage("draw")]
public static partial class draw_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸimage() => builtin.initPackage(typeof(image_package));
    [GoInit] internal static void initᴛᴛimportꓸimageꓸcolor() => builtin.initPackage(typeof(go.image.color_package));
    [GoInit] internal static void initᴛᴛimportꓸimageꓸpng() => builtin.initPackage(typeof(go.image.png_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸquick() => builtin.initPackage(typeof(go.testing.quick_package));
    // </ImportInitializers>
}
