// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.go.scanner_package;
using static go.go.scanner_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b696e70757420737472696e673b2077616e7420737472696e677d", "semicolonTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73726320737472696e673b20746f6b20676f2f746f6b656e2e546f6b656e3b20706f7320696e743b206c697420737472696e673b2065727220737472696e677d", "errorsᴛ1")]
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
[assembly: global::go.GoPositionMap("go/scanner/scanner_test.go", "scanner_test.cs", "ABIgAAcSgpSkpKQADxIAfqICgoKCgpSmgoKCgqamoqaClIKUgpSCurKWgpiSlt6CgpaUgpSWgoKClIKogqiClpSStqSklIKCpIK2gpaCqIKYgviCAAsYgoIACgqCgoKCloKCgoKCgoKUpoKCpoKUgIIACQgAZ/QBgoKSgqiCggAVFgAcQMy4+pKCgoKUuIKCgpiSgoCSgoKCAAYQguwAChKUkoKYkoKCgoKUgqailoIACQqSloKCgoKUgoKCgqiCgoKClIKCloIACQiCAAQSgoCUgoKCgIK4gpaCgpaCgoKWgoKCAAkUgoKCgoKClIKCgpSClIKClIKUgpSCABEIAD+MAYKCABoKkgAAJoKCgoKCgpSC+qKCgoKCgoKCpoLKooKCgoKCgoKCgoLcptyCgoKCgpSCgoKCgoKCgoKCAAwQggB8sgKCgoKCpoKClpSkpKaCgpSCqIK6goKUgg==", "230-232:1;578-578:1;617-624:1;680-680:1;721-725:1;903-924:1;1084-1088:1")]
// </GoSourcePositionMaps>

namespace go.go;

[GoPackage("scanner")]
public static partial class scanner_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
}
