// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.iter_package;

// <ImportedTypeAliases>
global using runtimeꓸError = go.runtime_package.ΔError;
// </ImportedTypeAliases>

using go;
using static global::go.iter_test_package;

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
[assembly: go.GoPositionMap("iter/pull_test.go", "pull_test.cs", "AA8cgoKCgtyCgoKC3IKSgoKCgIKCtoKCgoKCgpSUgoKClIKCgpSUhIKCgsqCkoKCgoCCgraCgoKCgoKUlIKCgpSCgoKUlISCgoLOAA0UhIKCgoKClIKUlJSUAAgGgoKCgoLcgqKCgqaitoKCgoKC3IKigoKmovaigoKClJKClJSCgrSkgoKCgu6igoKClJKClJSCgrSkgoKCggAODqKCgpCSpoCCtpSCgoKClJCSpoCCtriCgriCgoKC3KKCgpCSpoCCtpSCgoKClJCSpoCCtriCgriCgoKC3LKCgIKClLaCAAsGooKCgoKClJSAgqSUgoKCgpSSlKaAgra4goK4goKCggAIDKKCgoKCgpSUgIKklIKCgoKUkpSmgIK2uIKCuIKCgoLcgoSCsoKSlIKiktaigpSAgsiigpSAgg==", "15-21:1;25-31:1;36-72:1;38-43:1.1;78-114:1;80-85:1.1;163-170:1;164-168:1.1;185-192:1;186-190:1.1;201-206:1;214-219:1;230-235:1;243-248:1;254-265:1;256-256:1.1;266-281:2;272-272:2.1;285-287:1;291-297:1;301-312:1;303-303:1.1;313-328:2;319-319:2.1;332-334:1;338-344:1;348-355:1;361-374:1;364-367:1.1;375-392:2;381-383:2.1;396-398:1;402-408:1;412-425:1;415-418:1.1;426-443:2;432-434:2.1;447-449:1;453-459:1;466-473:1;468-470:1.1")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("iter_test")]
public static partial class iter_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸiter() => builtin.initPackage(typeof(iter_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.iter_package));
    }
}
