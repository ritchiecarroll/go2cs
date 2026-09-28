// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static go.encoding.binary_package;
using static go.encoding.binary_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("656e636f64696e672f62696e6172792e627974654f72646572", "TestByteOrder_byteOrder")]
[assembly: GoDynamicTypeLift("696e746572666163657b656e636f64696e672f62696e6172792e427974654f726465723b20656e636f64696e672f62696e6172792e417070656e64427974654f726465727d", "TestByteOrder_byteOrder")]
[assembly: GoDynamicTypeLift("7374727563747b4120656e636f64696e672f62696e6172792e5374727563747d", "TestSizeStructCache_type")]
[assembly: GoDynamicTypeLift("7374727563747b46205b385d666c6f617433327d", "BlankFieldsProbe_P3")]
[assembly: GoDynamicTypeLift("7374727563747b66205b385d666c6f617433327d", "BlankFields__")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20666e2066756e63286f7264657220656e636f64696e672f62696e6172792e427974654f726465722c206461746120616e792920285b5d627974652c206572726f72297d", "encodersᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20666e2066756e63286f7264657220656e636f64696e672f62696e6172792e427974654f726465722c206461746120616e792c20627566205b5d6279746529206572726f727d", "decodersᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<byteSliceReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<global::go.encoding.binary_package.bigEndian, TestByteOrder_byteOrder>]
[assembly: GoImplement<global::go.encoding.binary_package.littleEndian, TestByteOrder_byteOrder>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<Struct, ж<Struct>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("encoding/binary/binary_test.go", "binary_test.cs", "AG7sAYKCgoKUggANFoKC3ISCgpaCgpQAFS6CgpTKgoKyooKCyoKCsqKCyoCigKKApICigKKA9IKCgoKWgoKCuIKmgrKSgoKCgoKCgoKCzIKykoKCABAigrKyooKSsoKUpIKClKaCgoKUgoKCgpSCggALEoKykoKAgqaCgoKClICCpAAjPoKykpKCgpiSgIK4gqiCgpiSgIKkggAYDLSEgoKCgpSWgpKCgpYACjKCgpaAgtqCABcwgoCCAA8agoKSgIKmsrKCgqaC/IKykpKCgoKClICCtoKSgpIADQqChKKMgoKUpKSmgIKkgILasoKUgoLmsoKUgoIADAaCioKCAAgSgoKAgqSCgIKkgpaCgoCCpIKAgqSCloKCgIKkgoCCpILcooKUgriCgqaCAAocgoKCggAKCoIABRiykoKClIKCAAkMgoKUgoKSlIKUggAYMoKClILIkpSCAAkUgoKCpqKCgoKCgoKCuKKCgoKCkoKCgpSCgriigoKCuKKCgoSCuKKCgoKCgoKClKaigoKCgoKUpqKCgoKCgoKCuKKCgoKCgoKCgoKCgoKCgpSCgoKCgoKCgoKCuKKCgoKCgoKCgoKCgoKClIKCuKKCgoKCgoKCgoKCgoKUgoK4ooKCgoKCgoKUpqKCgoKCgpSmooKCuKKCgriigoK4ooKCuKKCgriigoK4ooKCuKKCgriigoK4ooKCuKKCgriigoK4ooKCgoKCgoKClIKCgoKCgoKCgoKCgoKCgoK4ooKCgoKCgoKUgoK4ooKCgoKCgoK4ooKCgoKCgoKUpqKCgoKCgoKCuKKCgoKCgoKCuIKCkoLcgoKSgoCC", "190-194:1;201-204:1;217-221:1;223-227:2;236-247:1;254-258:1;277-310:1;280-308:1.1;317-335:1;365-397:1;405-412:1;407-410:1.1;415-419:2;505-513:1;506-510:1.1;520-537:1;521-531:1.1;572-574:1;581-583:1;700-709:1;719-721:1;756-766:1;760-762:1.1;1143-1147:1")]
[assembly: go.GoPositionMap("encoding/binary/varint_test.go", "varint_test.cs", "AA0cgoKCgriCgoKmgoKCgoKUgpaCgoKWgoKUgriCgoKCgpSCloKCgpaCgpSCABgygoKClIKCuIKClIK4goKCgoKCloKCgpSCAAsOogADEoKClIIAFzKCkpKCgJKkgILsgoKCloKCgoKUgILIgoKCpoKCgoK6ooKCgoLKooKCgoI=", "152-159:1;185-193:2")]
// </GoSourcePositionMaps>

namespace go.encoding;

[GoPackage("binary")]
public static partial class binary_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
}
