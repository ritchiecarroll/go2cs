// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.expvar_package;

// <ImportedTypeAliases>
global using httpꓸCookie = go.net.http_package.ΔCookie;
global using httpꓸHandler = go.net.http_package.ΔHandler;
global using httpꓸHeader = go.net.http_package.ΔHeader;
global using jsonꓸToken = object;
global using jsonꓸΔToken = object;
global using netꓸAddr = go.net_package.ΔAddr;
global using netꓸError = go.net_package.ΔError;
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
// </ImportedTypeAliases>

using go;
using static global::go.expvar_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoTypeAlias("String", "ΔString")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<global::go.expvar_package.ΔString, global::go.expvar_package.Var>(Pointer = true)]
[assembly: GoImplement<go.net.http.httptest_package.ResponseRecorder, go.net.http_package.ResponseWriter>(Pointer = true)]
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
[assembly: go.GoPositionMap("expvar/expvar_test.go", "expvar_test.cs", "AB0wwoKCgpSi1IKCgoLogoKCgIKkgpaCgoCCpoCCpoKAgsiChIKCyoKEgoL6goKCgpSCloKCgIKmgIKmgoCCyIKEgoLKgoSCggAKCoKCgoCCpoKAkqSAkriCgJL4goSCggAJCoKCgoKChIKAkoKWhIKAkoLogoKEgoKEgoCSgpaCgIKkgoCSgpaCgoCSgpaCgoCCpIKAkoIACAiigoSCgoKCgIKkgIKkgILKgoKCgpSCgpSCgoKUggAJCKKCgoKCgoKAgqSCgpSCgpSCuIKEhKKCyqKCgoKClJaCgoSCsoKEgoIABRLSgoKWgoSCgoL6goSChKKCyoKCgoKCgoLKooKCgoKUloSCkoKEgoKCAAUS0oKCloSCgoLKgoKSgsqigoKCgpSWgoSCooKEgoLcgoKigoCSpICCpoKAkgAPCIKCgoKCgoKUgoKCioCCAAoIoqKCgoKCgpSSgoKCgoSCgujCAAUUhIKCloKCgoKUkoKygoKCgoKUlJSCgoKClJSCloSCgpSigoKCgoKUgoKCgpa0ppaygoKChIKCloK0prKCgoKCgpSCgoKClpSipqKCgoKEgoKWlKKkoqSigoKUgoKKgg==", "70-74:1;80-84:1;116-120:1;126-130:1;158-162:1;173-173:1;181-181:2;196-196:1;206-206:2;213-213:3;224-224:4;298-302:1;320-329:1;358-362:1;366-374:1;390-400:1;424-428:1;445-454:1;460-460:1;544-555:1;574-591:2;597-611:3;614-632:4;635-649:5")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("expvar")]
public static partial class expvar_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha1() => builtin.initPackage(typeof(crypto.sha1_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸhttptest() => builtin.initPackage(typeof(go.net.http.httptest_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.expvar_package));
    }
}
