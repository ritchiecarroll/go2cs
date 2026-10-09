// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.text.template.parse_package;

// <ImportedTypeAliases>
global using flagꓸErrorHandling = go.flag_package.ΔErrorHandling;
global using runtimeꓸError = go.runtime_package.ΔError;
// </ImportedTypeAliases>

using go;
using static global::go.text.template.parse_internal_test_package;

// <ExportedTypeAliases>
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
[assembly: go.GoPositionMap("text/template/parse/lex_test.go", "lex_test.cs", "AA0aACJKgoKClAAIEoLegoKCgoKCgoKCgoKmgvYAqALiBMKCyoKCgoKmpoKClIKClIKUgpSCpqaisoKCgpTaABIogpaisoKCAAkKogAJEISCuKIAChKEgrgAMGbCsoKCgpSCgoKCAAgWAAkCgoKCgoKC", "", "", "93=mkItem/1/97/2,mkItem/2/97/3,mkItem/3/97/5,mkItem/4/97/6,mkItem/5/97/7,mkItem/6/97/12,mkItem/7/97/13,mkItem/8/97/14,mkItem/9/97/23,mkItem/10/97/32,mkItem/11/97/39,mkItem/12/97/41,mkItem/13/97/43,mkItem/14/97/45,mkItem/15/97/47,mkItem/16/97/49,mkItem/17/97/51,mkItem/18/97/53,mkItem/19/97/55,mkItem/20/97/57,mkItem/21/97/59,mkItem/22/97/61,mkItem/23/97/63,mkItem/24/97/69,mkItem/25/97/71,mkItem/26/97/73,mkItem/27/97/75,mkItem/28/97/77,mkItem/29/97/79,mkItem/30/97/81,mkItem/31/97/87,mkItem/32/97/89,mkItem/33/97/101,mkItem/34/97/107,mkItem/35/97/111,mkItem/36/97/113,mkItem/37/97/114,mkItem/38/97/115,mkItem/39/97/121,mkItem/40/97/123,mkItem/41/97/125,mkItem/42/97/127,mkItem/43/97/129,mkItem/44/97/135,mkItem/45/97/137,mkItem/46/97/139,mkItem/47/97/141,mkItem/48/97/143,mkItem/49/97/145,mkItem/50/97/147,mkItem/51/97/149,mkItem/52/97/150,mkItem/53/97/152,mkItem/54/97/158,mkItem/55/97/160,mkItem/56/97/165,mkItem/57/97/167,mkItem/58/97/169,mkItem/59/97/171,mkItem/60/97/174,mkItem/61/97/176,mkItem/62/97/178,mkItem/63/97/180,mkItem/64/97/182,mkItem/65/97/187,mkItem/66/97/189,mkItem/67/97/191,mkItem/68/97/197,mkItem/69/97/199,mkItem/70/97/201,mkItem/71/97/203,mkItem/72/97/205,mkItem/73/97/212,mkItem/74/97/214,mkItem/75/97/219,mkItem/76/97/221,mkItem/77/97/223,mkItem/78/97/227,mkItem/79/97/228,mkItem/80/97/229,mkItem/81/97/234,mkItem/82/97/236,mkItem/83/97/240,mkItem/84/97/245,mkItem/85/97/249,mkItem/86/97/253,mkItem/87/97/257,mkItem/88/97/261,mkItem/89/97/266,mkItem/90/97/267,mkItem/91/97/271,mkItem/92/97/272,mkItem/93/97/289,mkItem/94/97/290,mkItem/95/97/293,mkItem/96/97/294,mkItem/97/97/299;450=mkItem/1/7/3,mkItem/2/7/4,mkItem/3/7/5,mkItem/4/7/6,mkItem/5/7/7,mkItem/6/7/8,mkItem/7/7/9;494=mkItem/1/5/1,mkItem/2/5/2,mkItem/3/5/3,mkItem/4/5/4,mkItem/5/5/5;513=mkItem/1/6/1,mkItem/2/6/2,mkItem/3/6/3,mkItem/4/6/4,mkItem/5/6/5,mkItem/6/6/6")]
[assembly: go.GoPositionMap("text/template/parse/parse_test.go", "parse_test.cs", "AA0cABYcADZ6ooaigoKClIKCpoKCgoKUgoKUgoKUlIKUgoKUgqSClIKClIKkgpSCgpSCpIKUgoKUgqSCABoiAI4BtALKwoKAkoKClIK0graSlLSCgpSUgsakgqiSpsKCgJLcspKCgoKClICC6NT4goCUgobGgoKopoKCxuTCgoKAkoKCgoKUgoCCxAAIEAANFqKClIKCgoKUgIIACAqCgoKUgoKCgpSC+gBizAGCspKCgpSCABMMgqqCgoKUgJKkgoKUgJIACAiEkoKCgriCgpSCgpSCgsqigoKCggAKDqKmgoKClIIAGQiiAAAkgoKUgoKClII=", "339-339:1;379-379:1;387-397:2;406-406:1;413-413:2;434-434:1;601-609:1")]
// </GoSourcePositionMaps>

namespace go.text.template;

[GoPackage("parse")]
public static partial class parse_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.text.template.parse_package));
    }
}
