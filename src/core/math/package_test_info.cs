// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.math_package;
global using static global::go.math_internal_test_package;

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static global::go.math_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b7820666c6f617436343b207920666c6f617436343b207a20666c6f617436343b2077616e7420666c6f617436347d", "fmaCᴛ1")]
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
[assembly: go.GoPositionMap("math/all_test.go", "all_test.cs", "AA8cABEkAAwYAAwYAAwYAAwYAAwYAAwYAAwYAAwYAAwYAAwYAA0cAAwaAAwYAAwYAAwYAAwYAAwYAAwYAAwYAAwYAAwYAAwYAAwYABEkAAwYAAwYAAwYAAwYAAwYAAwYAAwYAAwYAAwYAAwYAAwYAAwYAAwYAAwYAAwYAAwYAAwYAAwYAAwYAA0cAAwYAAwYAAwYAA0cAAwYAAwYAAwYAAwYAAwYAAwYAA0c7O7+AAcQ/gAHEP4ABxD+AAcQAAsWAAsWAExOACdQ/gAJEAAPIAAHEAAJFgAJFgAJFtrc2tz+AAcQAAkSAAkU/gAHEAAIEAAIEgAIEAAMEgANIgAQHAAKGgAKFgAJEgAJFP4ABxAAESKSABEiABEiABEiABEkAEZIACRK/gAQEABKqAEAFSoAFSzu7Ozs7O4ADx4ADyAACRIACRQACRIACRTs/gALFAAKFgAIDO4ADxgADBoADxgADhoAmgGsAQBNpAEAICQAFyQAFR4ADyD+AAcQ/gAHEP4ACBAACRIACRT+AAcQ/v7+/gAMHAAKFAAKFgAOGAAMGgAZHAAufAAOHriClIKCuoKCgqakgKKAooCigpSkpKaCgoKUgoK4goKCgIK2goCC2oKCgoCCtoKAgtqCgoKAgraCgILagoKAgraCgILagoKAgraCgILagoKCgIK2goCC2oKCgIK2goCC2oKCgIK2goCC2oKCgIK2goCC2oKCgIK2goCCtoKAgtqCgoCCtoKAgtqCgoCCtoKAgtqCgoKAgraCgILagoKCgIK2goCC2oKCgoCCtoKAgraCgIK2goCC2oKCgoCCtoKAgraCgIK2goCCAAkKgoKmgoKAgraCgILagoKCgIK2goKAgraCgIIACQqCgqaCgoCCtoKAgraCgoKCyoKCgIK2goCC2oKCgIK2goCCtoKAgtqCgoCCtoKAgtqCgoCCtoKAgraCgILagoKAgraCgIK2goCC2oKCgIK2goCCyICCyIKCgIK2goCCtoKAgtqCgoCCtqKCgoKklJSCyoKCgoCCtoKAgtqCgoKAgraCgILagoKCgIK2goCCtoKAgtqCgoCCtoKAgtqCgoCCtoKAgtqCgoCCpICCtoKAgqSAgtqCgoCCtoKAgraCgIK2goCCtoKAgtqCgoCCtoKAgtqCgoKAgraAgqSCgILagoKAgraCgIK2goCC2oKCgoCCtoCCpIKAgtqCgoKAgraCgIKkgoCC2oKCgoCCtoCCpIKAgraCgoKCyoKCgIK2goCC2oKCgoCCtoKAgtqCgoCCtoKAgtqCgoCCtoKAgtqCgoCC2oKCgIK2goCCyICCtoKAgraCgoKCgsqCgoCCtoKAgtqCgoCCtoKAgtqigoCCtoKAgtiCgoCCtoKAgtqCgoCC2oKCgIK2goCC2oKCgoCCpIKAgraCgIKkgILagoKAgsiCgILagoKAgraCgILagoKAgraCgILagoKCgIK2goCC2oKCgoCCtoKAgtqCgoKAgqSAgraCgIKkgIK2gILIlIKCgoKUgoLMkqiSqJKmyoKCgoKUgoKClIKCggAEEsKCgoKCgsqCgoKCgoLKgoKCgoKCyoKCgoKCgs6ilJSCgqaClIKEgIKkgIKkgIKkgoKCAA0e7oKCgoLKgoCCpICCyJaSgoKCgoKUlIIAECKigoKUpqKCgpSmooKClKaigoKUpqKCgpSmooKClKaigoKUpqKCgpSmooKClMqigoKUpqKCgpSmooKClKaigoKUpqKCgpSmooKClKaigoKUpqKCgpSmooKClKaigoKUpqKCgpSmooKClMqigoKUqKKCgpSmooKClKaigoKUpqKCgpSmooKClKaigoKClIKmooKClKaigoKUpqKCgpSmooKClKaigoKUpqKCgpSmooKClKaigoKUpqKCgoKUgqaigoKUpqKCgpSmooKClKaigoKUpqKCgpSmooKCgpSCpqKCgpSmooKClKaigoKUpqKCgpTKooKClMqigoKUyqKCgpSmooKClKaigoKUyqKCgpSmooKClKaigoKClIKmooKClKaigoKClKaigoKUpqKCgoKUpqKCgpSm3IKCpqaigoKUpqKCgpSmooKClKSigoKUpqKCgpSmooKClKaigoKUpqKCgpTKooKClMqigoKUyqKCgpSmooKClA==", "2937-2941:1")]
[assembly: go.GoPositionMap("math/const_test.go", "const_test.cs", "AAsagoCCpICCpICCpICCpICCyIKAgqSAgqSAgqSAgqSAgg==")]
[assembly: go.GoPositionMap("math/huge_test.go", "huge_test.cs", "AAwaABAiAAwaAAwaAAwgsoKCgoKUgoLKgoKCgoKUgoLKgoKCgoKUgoLKgoKCgoKUgoI=")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("math_test")]
public static partial class math_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct fi {}
    internal partial struct floatTest {}
    internal partial struct fmaCᴛ1 {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.math_package));
    }
}
