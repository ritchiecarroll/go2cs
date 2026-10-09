// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.image.color_package;

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static global::go.image.color_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoTypeAlias("RGBA", "ΔRGBA")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<global::go.image.color_package.Alpha, global::go.image.color_package.Color>]
[assembly: GoImplement<global::go.image.color_package.CMYK, global::go.image.color_package.Color>]
[assembly: GoImplement<global::go.image.color_package.Gray, global::go.image.color_package.Color>]
[assembly: GoImplement<global::go.image.color_package.NYCbCrA, global::go.image.color_package.Color>]
[assembly: GoImplement<global::go.image.color_package.YCbCr, global::go.image.color_package.Color>]
[assembly: GoImplement<global::go.image.color_package.ΔRGBA, global::go.image.color_package.Color>]
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
[assembly: go.GoPositionMap("image/color/color_test.go", "color_test.cs", "AAwYlIKCgpSUlAANHIKCgILIgII=", "14-22:1")]
[assembly: go.GoPositionMap("image/color/ycbcr_test.go", "ycbcr_test.cs", "AAsYgoKUpoKCgoKmqqKCgoKCgoKCAAcWsoKCgoKCgoKCAAcSkoKCgoCC3JKCgoKAgtySgoKCgILeooKCgoKCgoIABxaygoKCgoKCgoKCAAgUkoKAgtqCAAMUgoKCyoKCgv64goKmgoKmgoLKuIKCpoKCpoKCyriCgoKmgoKCpoKCgsq4goKCpoKCgqaCgoI=", "182-186:1;187-191:2;192-196:3;203-207:1;208-212:2;213-217:3;224-229:1;230-235:2;236-241:3;248-253:1;254-259:2;260-265:3")]
// </GoSourcePositionMaps>

namespace go.image;

[GoPackage("color")]
public static partial class color_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸquick() => builtin.initPackage(typeof(go.testing.quick_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.image.color_package));
    }
}
