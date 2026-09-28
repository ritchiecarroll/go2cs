// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.@internal.synctest_package;

// <ImportedTypeAliases>
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static global::go.@internal.synctest_test_package;

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
[assembly: go.GoPositionMap("internal/synctest/synctest_test.go", "synctest_test.cs", "ABMmgpKkgIKkpICyyIKAgtqCuIKCuIKCkryigoKCgpKCgpSCpoKCgoLKgoKCgoKCkoKClIKmgoKCgtyCgoKCgoCC2oKCgoKCgoKCyoKCgoKAgqaCgIKmgoKCgILagoKClIKCgqaCgoCS2oKCkpSCAAkIgoLcosaixqLGpAAMDqSCAA4MkoKilKIACwyCrtzc+JKCgtKCgsSSgpT6ooKC+KKCgoIACAqCgoKCgoSkgoKCloKCgqiCgoKAgriCkoKCgpSCgoKCgpSAgtqCgoKCpoKSsoKCuMqCgoLKgoKCgqaCsoKSgpLWyoKCgsqCgu6SgqaSgsqCgoKCgoKClIKCgILaooCCgIK2", "21-37:1;26-31:1.1;41-42:1;46-48:1;52-55:1;53-53:1.1;61-80:1;65-73:1.1;85-104:1;89-97:1.1;109-116:1;120-129:1;133-151:1;155-170:1;157-164:1.1;160-163:1.1.1;175-177:1;191-191:1;192-192:2;195-195:3;196-196:4;199-199:5;200-200:6;203-203:7;204-209:8;212-212:9;213-219:10;221-229:11;223-225:11.1;226-228:11.2;240-242:1;246-248:2;252-254:3;257-270:4;260-264:4.1;265-268:4.2;276-278:1;283-287:1;284-286:1.1;291-337:1;297-309:1.1;321-326:1.2;341-366:1;342-346:1.1;348-355:1.2;370-395:1;371-375:1.1;377-384:1.2;399-406:1;407-411:2;412-416:3;420-433:1;424-427:1.1")]
// </GoSourcePositionMaps>

namespace go.@internal;

[GoPackage("synctest_test")]
public static partial class synctest_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct TestChannelFromOutsideBubble_type {}
    internal partial struct TestTimerFromInsideBubble_type {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸiter() => builtin.initPackage(typeof(iter_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(go.sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.@internal.synctest_package));
    }
}
