// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.@internal.runtime.atomic_package;

// <ImportedTypeAliases>
global using runtimeꓸError = go.runtime_package.ΔError;
// </ImportedTypeAliases>

using go;
using static global::go.@internal.runtime.atomic_test_package;

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
[assembly: go.GoPositionMap("internal/runtime/atomic/atomic_andor_test.go", "atomic_andor_test.cs", "ABAclJKCgoKAgsqCgqiCgoKigpSmgqiCgsqUkoKCgoKAgsqCgqiCgoKigpSmgqiCgsqUkoKCgoCCypaCgoKigpSmgqiCgsqUkoKCgoKAgsqWgoKCooKUpoKogoLKooKCgriCgoKCgoKCyqKCgoK4goKCgoKCgsqigoKCuIKCgoKCgoLKooKCgriCgoKCgoKC", "35-40:1;76-81:1;113-118:1;151-156:1;181-187:1;201-207:1;221-227:1;241-247:1")]
[assembly: go.GoPositionMap("internal/runtime/atomic/atomic_test.go", "atomic_test.cs", "ABEeooKCgpKClKaCtKSCgoKCgpSCkoKUgIKkgoKClILsosqUgpKCguiilISCgoKSgIK2ogALBsqCloKEgoSQkpCSkJKQkpCSkLaUkoKCgILKgoKogoKCooKUpoKogoLKlJKCgoCCyoKCqIKCgqKClKaCqIKCypSSgoKAgsqWgoKCooKUpoKogoLKlJKCgoCCypaCgoKigpSmgqiCgsqUloKCqIKCgqKCgoKClIKCuKaCqIKCypSWgoKogoKCooKCgoKUgoK4poKogoIADAqCggAAEIKCgoKCgoKWgpaC+oKCgpSC", "19-24:1;40-42:1;47-50:2;75-86:1;106-106:1;107-107:2;108-108:3;109-109:4;110-110:5;111-111:6;134-139:1;173-178:1;209-214:1;245-250:1;278-292:1;320-334:1")]
[assembly: go.GoPositionMap("internal/runtime/atomic/bench_test.go", "bench_test.cs", "AA0cooKCgriigoKCuKKCgoK4ooKCgriigoKCuKKCgoK4ooKCgriigoKCuIKCgoKCgoLKgoKCgoKCgsqigoKCuKKCgoK4goKCgoKCgsqCgoKCgoKCyoKCgpKCyoKCgpKCyoKCgoKSgoLKgoKCgpKCgsiCgoKCkoKCgoLKgoKCgpKCgoKC", "81-87:1;93-99:1;121-127:1;133-139:1;145-149:1;155-159:1;166-171:1;178-183:1;189-196:1;203-210:1")]
[assembly: go.GoPositionMap("internal/runtime/atomic/xchg8_test.go", "xchg8_test.cs", "AAwcgoKCgpSogoKCgoKClIKUgsqigoKCuIKCgoKCgoI=", "52-58:1")]
// </GoSourcePositionMaps>

namespace go.@internal.runtime;

[GoPackage("atomic_test")]
public static partial class atomic_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct TestCasRel_x {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.@internal.runtime.atomic_package));
    }
}
