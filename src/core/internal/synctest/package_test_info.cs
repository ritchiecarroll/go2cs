// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.@internal.synctest_package;

// <ImportedTypeAliases>
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using runtimeꓸError = go.runtime_package.ΔError;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static global::go.@internal.synctest_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
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
[assembly: go.GoPositionMap("internal/synctest/synctest_test.go", "synctest_test.cs", "AB80gpKkgIKkpICyyIKAgt6ikpKCgIK2goKAgqSAgtqCuIKCuIKCkryigoKCgpKCgpSCpoKCgoLKgoKCgoKCkoKClIKmgoKCgtyCgoKCgoCC2oKCgoKCgoKCyoKCgoKAgraCgIK2goKCgILagoKClIKCgqaCgoCS2oKCgoKmgriCgpKUgryigoKSgoKCtLSkgpSUvKKCgpKCgoKClIKClIKCgKS0tLSClJS8ooKCgqSCxgAFELKCgoKCgqSCxt6igoKClILOooKClKKCtPyCgoKCAAkIgoLcosaixqLGpAAMDqSCAA4MkoKilKIADwyCrtzc+KSSkoKC0oKikpKUuJKSgoLSgoKitJKSlAAMDoKu3Nz4kpKCgtKCopKSgpQACAyigoK01KKCgoLGpKKCgoKCgtikgoKCgoKUpIKCgqaCgoK4goKCgILIgpKCgoKUgoKCgoKUgILagoKCgqaCkrKCgrjKgoKCyoKCgoKmgrKCkoKStJLKgoKCyoKC7pKCppKCyoKCgoKCgoKUgoKAgtySgoKCpLiCgoKCtoKUtNaC1oKCgoKCyoKCgoKU6IKCgoKUgsqCgoKClJSUgriCgoKCgqaUlILsgu6SuJSCpoKCgrqSgoKCgoKCgpKmgoK4poaigoKCpoKUgpSUgqaCkoKUkoKUlIKCpoKUgpSUgqaCkoKUkoKUlIKCpoKCgqaCgsqAkqSAksqSgoKCksrCgIKAgrbE1IKUgoKmgoKCgoKUgg==", "28-44:1;33-38:1.1;51-65:1;69-70:1;74-76:1;80-83:1;81-81:1.1;89-108:1;93-101:1.1;113-132:1;117-125:1.1;137-144:1;148-157:1;161-179:1;183-198:1;185-192:1.1;188-191:1.1.1;203-207:1;204-206:1.1;215-217:1;226-244:1;250-279:1;255-258:1.1;259-262:1.2;285-296:1;303-316:1;322-330:1;324-326:1.1;336-355:1;340-348:1.1;366-366:1;367-367:2;370-370:3;371-371:4;374-374:5;375-375:6;378-378:7;379-384:8;387-387:9;388-394:10;396-404:11;398-400:11.1;401-403:11.2;415-417:1;421-423:2;427-429:3;432-466:4;434-447:4.1;435-446:4.1.1;438-441:4.1.1.1;442-444:4.1.1.2;449-465:4.2;450-464:4.2.1;453-459:4.2.1.1;456-458:4.2.1.1.1;460-462:4.2.1.2;477-479:1;483-485:2;489-491:3;494-508:4;495-507:4.1;498-501:4.1.1;502-505:4.1.2;514-516:1;521-525:1;522-524:1.1;530-537:1;531-536:1.1;541-587:1;547-559:1.1;571-576:1.2;591-616:1;592-596:1.1;598-605:1.2;620-645:1;621-625:1.1;627-634:1.2;649-656:1;657-661:2;662-666:3;670-683:1;674-677:1.1;688-694:1;691-691:1.1;701-706:1;717-723:1;720-722:1.1;727-733:1;729-731:1.1;737-745:1;739-741:1.1;742-744:1.2;750-755:1;756-760:2;765-772:1;773-777:2;783-792:1;790-790:1.1;798-805:1;812-825:1;813-824:1.1;817-819:1.1.1;835-894:1;840-842:1.1;843-845:1.2;853-856:1.3;857-860:1.4;868-870:1.5;871-873:1.6;881-884:1.7;885-888:1.8;896-907:2;897-901:2.1;898-900:2.1.1;902-906:2.2;903-905:2.2.1;919-925:1", "", "799=Now/1/3/1,Now/2/3/2,Add/1/2/2,Now/3/3/3,Add/2/2/3;836=Now/1/3/1,Now/2/3/2,Add/1/2/2,Now/3/3/3,Add/2/2/3;851=Itoa/1/1/2,TypeOf/1/1/3,StructOf/1/1/1")]
// </GoSourcePositionMaps>

namespace go.@internal;

[GoPackage("synctest_test")]
public static partial class synctest_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct TestChannelFromOutsideBubble_type {}
    internal partial struct TestChannelMovedOutOfBubble_type {}
    internal partial struct TestTimerFromInsideBubble_type {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsynctest() => builtin.initPackage(typeof(go.@internal.synctest_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(go.@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸiter() => builtin.initPackage(typeof(iter_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(go.strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(go.sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸweak() => builtin.initPackage(typeof(weak_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.@internal.synctest_package));
    }
}
