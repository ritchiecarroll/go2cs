// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.math.cmplx_package;
using static go.math.cmplx_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b696e20636f6d706c65783132383b2077616e7420636f6d706c65783132387d", "acosSCᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
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
[assembly: go.GoPositionMap("math/cmplx/cmath_test.go", "cmath_test.cs", "AA4eAAwaABEmAAwaAAwYAAwYAAwYAAwYAAwYAAwYAAwYAAwYAAwYAAwYAAwYABAiAAwYAAwYAAwYAAwYAAwYAAwYAA0cAA4eAA0cgJS2AAsGACJIAB9AABk0ABg0ABs4ABg4tgAIBgAjQAAkQAAgSAALFgANGAAeQAAcQLa2yOgAI0IAH0IAGjgAHTgAHUQAChiSgoK6goKCpqSAooKUpLSmgoKCgoKmpICigKKCkoKmlIKmlKSmpoKCgIK2goCC2IKCgIK2goCCpJSmgIK2ooCC2IKCgIK2goCCpJSmgIK4ooCC2IKCgIK2goCCpJSmgIKklKaAgraigILYgoKAgraCgIKklKaAgqSUpoCCtqKAgtiCgoCCtoKAgqSUpoCCpJSmgIK2ooCC2IKCgIK2goCCpJSmgIKklKaAgraigILYgoKAgraCgILYgoKAgraCgIKklKaAgqSUpoCC2IKCgIK2goCCpJSmgIKklKaAgtiCgoCCtoKAgqSUpoCC2IKCgILYgoKAgraCgIKklKaAgraigILYgoKAgraCgIKklKaAgtiCgoCCtoKAgtiEktyigIK2goKAgraCgIK2ooCC2IKCgIK2goCC2IKCgIK2goCCpJSmgIKklKaAgtiCgoCCtoKAgqSUpoCCpJSmgILYgoKAgraCgIKklKaAgraigILYgoKAgraCgIKklKaAgqSUpoCC2IKCgIK2goCCpJSmgIKklKaAgtySgoCCyKKCtqKCtqKCtqKCtqKCtqKCtqKCtqKCtqKCtqKCtqKCtqKCtqKCtqKCtqKCtqKCtqKCtqKCtqKCtqKCtqKCtqKC", "", "", "343=NaN/1/1/1;347=NaN/1/1/1;359=NaN/1/4/16,NaN/2/4/30,NaN/3/4/33,NaN/4/4/34;394=NaN/1/4/12,NaN/2/4/26,NaN/3/4/29,NaN/4/4/30;426=NaN/1/5/10,NaN/2/5/18,NaN/3/5/20,NaN/4/5/23,NaN/5/5/24;452=NaN/1/4/10,NaN/2/4/20,NaN/3/4/23,NaN/4/4/24;477=NaN/1/5/8,NaN/2/5/14,NaN/3/5/22,NaN/4/5/25,NaN/5/5/26;505=NaN/1/4/14,NaN/2/4/22,NaN/3/4/25,NaN/4/4/26;530=NaN/1/1/1;534=NaN/1/1/1;543=NaN/1/6/14,NaN/2/6/18,NaN/3/6/22,NaN/4/6/26,NaN/5/6/29,NaN/6/6/30;579=NaN/1/6/12,NaN/2/6/14,Cos/1/1/18,Sin/1/1/18,NaN/3/6/26,NaN/4/6/28,NaN/5/6/29,NaN/6/6/30;616=NaN/1/6/10,NaN/2/6/12,Cos/1/2/16,Copysign/1/2/16,Sin/1/2/16,Copysign/2/2/16,Cos/2/2/18,Sin/2/2/18,NaN/3/6/30,NaN/4/6/32,NaN/5/6/33,NaN/6/6/34;649=Inf/1/8/1,Inf/2/8/1,Inf/3/8/2,NaN/1/8/2,NaN/2/8/3,Inf/4/8/3,NaN/3/8/4,NaN/4/8/5,Inf/5/8/6,Inf/6/8/6,Inf/7/8/7,NaN/5/8/7,NaN/6/8/8,Inf/8/8/8,NaN/7/8/9,NaN/8/8/9;675=NaN/1/4/12,NaN/2/4/26,NaN/3/4/29,NaN/4/4/30;706=NaN/1/4/12,NaN/2/4/26,NaN/3/4/29,NaN/4/4/30;735=NaN/1/1/1;739=NaN/1/2/1,NaN/2/2/1;743=NaN/1/3/1,NaN/2/3/1,NaN/3/3/2;748=NaN/1/2/1,NaN/2/2/2;755=NaN/1/6/14,NaN/2/6/18,NaN/3/6/22,NaN/4/6/26,NaN/5/6/29,NaN/6/6/30;791=NaN/1/6/12,NaN/2/6/14,Cos/1/1/18,Sin/1/1/18,NaN/3/6/26,NaN/4/6/28,NaN/5/6/29,NaN/6/6/30;823=NaN/1/4/14,NaN/2/4/24,NaN/3/4/25,NaN/4/4/26;850=NaN/1/7/12,NaN/2/7/14,NaN/3/7/18,NaN/4/7/20,NaN/5/7/22,NaN/6/7/25,NaN/7/7/26;880=NaN/1/6/8,NaN/2/6/10,Sin/1/1/12,Copysign/1/1/12,NaN/3/6/20,NaN/4/6/22,NaN/5/6/23,NaN/6/6/24;1482=Inf/1/2/3,Inf/2/2/4")]
[assembly: go.GoPositionMap("math/cmplx/huge_test.go", "huge_test.cs", "AA0ggoKAgg==")]
// </GoSourcePositionMaps>

namespace go.math;

[GoPackage("cmplx")]
public static partial class cmplx_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
}
