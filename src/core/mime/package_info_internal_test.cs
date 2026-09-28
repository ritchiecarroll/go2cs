// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.mime_package;
using static go.mime_internal_test_package;

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
[assembly: go.GoPositionMap("mime/encodedword_test.go", "encodedword_test.cs", "ABccopIADyqCgIIACgqCAAcYgoKCgoKCloKCAAsOggAOJoKCgoKClIKClIIACAqiABxAgoKCgpSCAAsKggAMJrKCpIKUgoKUgoKUhKaCgpSCAAkKgqyAgviiguiihIK4ooSC", "180-195:1;209-211:1")]
[assembly: go.GoPositionMap("mime/mediatype_test.go", "mediatype_test.cs", "AA0agtyigoKCgrbcggALGKKCgoKCttyCAAwaooKCgoKCtrYAORi0koKClJaCAKIC4gSCgoKCgpSUgJKUpIKUggAfRIKCgoKClIKUgpSClIIAIUKCgoKClIKUgoKUgpSigoI=", "101-107:1")]
[assembly: go.GoPositionMap("mime/type_test.go", "type_test.cs", "AA8egoKCgoK4gqaCgpSAgsiC7oSCgoIADArCgoKCgpSUAAcQgoKCABIKooKEgoKClKaAgqSAgriAggAUCKKCgoKCgoKUlAAGFoKCgpSCgpSCgpSCAAwKgoKUgoKUgriigoTKgoKC7qKChMqCgoKAggANEKKClJSUroKCgoKUgg==", "18-21:1;56-61:1;84-88:1;106-113:1;150-153:1;168-174:1;169-173:1.1;187-195:1;188-194:1.1;200-204:1")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("mime")]
public static partial class mime_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmaps() => builtin.initPackage(typeof(maps_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
}
