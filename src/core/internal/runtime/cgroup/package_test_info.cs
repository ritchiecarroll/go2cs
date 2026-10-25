// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.@internal.runtime.cgroup_package;
global using static global::go.@internal.runtime.cgroup_internal_test_package;

// <ImportedTypeAliases>
global using strconvꓸError = go.@internal.strconv_package.ΔError;
// </ImportedTypeAliases>

using go;
using static global::go.@internal.runtime.cgroup_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20636f6e74656e747320737472696e673b2077616e74205b5d696e7465726e616c2f72756e74696d652f6367726f75705f746573742e6e6578744c696e657d", "readerTestsᴛ1")]
[assembly: GoTypeAlias("LineReader", "global::go.@internal.runtime.cgroup_package.lineReader")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: go.GoHoistedLiterals]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("internal/runtime/cgroup/cgroup_test.go", "cgroup_test.cs", "ABYeggAgTLKSgoKClJSCpoIADQyCADN0spKCgoKUlIKmgqaC3IKCkoKCgpQACwiCgpQALF6ykoKCgoKmgqaC3ILugoKCgoIACgyCAB5EspKCggARDIiygoKmgoKUgpSmAPMBsgOykoKCgpSCpoLcggAMGoKCgoKCAAUU4oKClLS2AAwGggAtYJKykoKC3JKykoKClIKCpqKCgpSigtyCAAgSgpKCgg==", "55-70:1;134-153:1;159-166:1;221-236:1;248-255:1;295-300:1;542-555:1;573-580:1;650-659:1;652-657:1.1;661-683:2;663-672:2.1;673-677:2.2;678-681:2.3;697-702:1")]
[assembly: go.GoPositionMap("internal/runtime/cgroup/line_reader_test.go", "line_reader_test.cs", "ABMkgqSCAAkKAGbCAYKykoKUgoKCgpKCkoKUuIKCgqaCgpSC7qKClIKCgpKClIKCgpKClKaCpoKmgoI=", "126-158:1;166-195:1", "", "35=complete/1/13/13,incomplete/1/5/20,complete/2/13/27,incomplete/2/5/34,complete/3/13/43,complete/4/13/44,complete/5/13/55,complete/6/13/56,complete/7/13/57,complete/8/13/58,complete/9/13/66,incomplete/3/5/67,complete/10/13/77,incomplete/4/5/78,complete/11/13/79,complete/12/13/90,incomplete/5/5/91,complete/13/13/92")]
// </GoSourcePositionMaps>

namespace go.@internal.runtime;

[GoPackage("cgroup_test")]
public static partial class cgroup_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct TestContainsCPU_tests {}
    internal partial struct TestEscapePath_tests {}
    internal partial struct TestParseCPUCgroup_tests {}
    internal partial struct TestParseCPUMount_tests {}
    internal partial struct TestParseV1Number_tests {}
    internal partial struct TestParseV2Limit_tests {}
    internal partial struct nextLine {}
    internal partial struct readerTestsᴛ1 {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸruntimeꓸcgroup() => builtin.initPackage(typeof(go.@internal.runtime.cgroup_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.@internal.runtime.cgroup_package));
    }
}
