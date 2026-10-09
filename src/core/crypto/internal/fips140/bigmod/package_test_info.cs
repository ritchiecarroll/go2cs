// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.crypto.@internal.fips140.bigmod_package;

// <ImportedTypeAliases>
global using bigꓸInt = go.math.big_package.ΔInt;
global using bigꓸRat = go.math.big_package.ΔRat;
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
// </ImportedTypeAliases>

using go;
using static global::go.crypto.@internal.fips140.bigmod_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoTypeAlias("Nat", "ΔNat")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<os_package.File, io_package.Reader>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<global::go.crypto.@internal.fips140.bigmod_package.Modulus, ж<global::go.crypto.@internal.fips140.bigmod_package.Modulus>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("crypto/internal/fips140/bigmod/nat_test.go", "nat_test.cs", "ABs48oKCgpSmgoKClKaCgoKUqsKCgpSmooKCgoKCpoKCgriigoKCgqaCgoK4goKCgoKCgoKCgoKCgpSUggALCIKClAAVMoKCgoCC2sqmlIL2ggApWoKCgoKClJSCgpSAgriCgoKClJaCggAJCIKCAA0kgoKCyoKCgoKCgoK4goKCgoKCgpSCgoK4goKCgoKCgpSCgoK4goKCgoKCgriCgoKCgoKC/sSCgoSCgoSCloKCgoKEguiCgJKAkoCSgLaCgoKCloKUgpaCgpSCgpSCgpaChIKCgoKCgoSCABkIgoKCgqaCgoKogoKClIKCgoKCgoKCgoKCgoKCgoKCgoKCgoKChIKCgpSCuIKCgoKCgoKUgriCgoKUgpKCgoKClIKCppCSkJKQyKKmlIKmgoKCqJKCgoKCpoKmgoKClKaCgoKUpqKCgoSCgriigoKEgoK4ooKEgoK4ooKCgoSCgriigoKEgoK4ooKCgoKCgoSCgriigoKChIKCuIKCgoKUgoKUgoKUgoKUgoK4goKCgpT2uAAEELKSgoKCgoKCgoIACgyCgoKWooKCgoKCgpaClKSkhIKCgpSCgpaCgpSCgpSCyLaAgsiCgoKUgoKU", "95-110:1;232-239:1;369-369:1;370-370:2;371-371:3;372-372:4;419-424:1;425-430:2;492-503:1;504-504:2;505-505:3;506-506:4;683-694:1;723-744:1")]
// </GoSourcePositionMaps>

namespace go.crypto.@internal.fips140;

[GoPackage("bigmod")]
public static partial class bigmod_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(go.crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸquick() => builtin.initPackage(typeof(go.testing.quick_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.crypto.@internal.fips140.bigmod_package));
    }
}
