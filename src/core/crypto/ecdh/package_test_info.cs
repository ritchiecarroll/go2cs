// go2cs code converter defines `global using` statements here for imported type
// aliases as package references are encountered via `import' statements. Exported
// type aliases that need a `global using` declaration will be loaded from the
// referenced package by parsing its 'package_info.cs' source file and reading its
// defined `GoTypeAlias` attributes.

// Package name separator "dot" used in imported type aliases is extended Unicode
// character '\uA4F8' which is a valid character in a C# identifier name. This is
// used to simulate Go's package level type aliases since C# does not yet support
// importing type aliases at a namespace level.

// <ImportedTypeAliases>
global using cryptoꓸDecrypterOpts = object;
global using cryptoꓸPrivateKey = object;
global using cryptoꓸPublicKey = object;
global using ecdhꓸCurve = go.crypto.ecdh_package.ΔCurve;
global using ecdhꓸPublicKey = go.crypto.@internal.fips140.ecdh_package.ΔPublicKey;
global using execꓸError = go.os.exec_package.ΔError;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
// </ImportedTypeAliases>

using go;
using static go.crypto.ecdh_package;
using static go.crypto.ecdh_test_package;

// For encountered type alias declarations, e.g., `type Table = map[string]int`,
// go2cs code converter will generate a `global using` statement for the alias in
// the converted source, e.g.: `global using Table = go.map<go.@string, nint>;`.
// Although scope of `global using` is available to all files in the project, all
// converted Go code for the project targets the same package, so `global using`
// statements will effectively have package level scope.

// Additionally, `GoTypeAlias` attributes will be generated here for exported type
// aliases. This allows the type alias to be imported and used from other packages
// when referenced.

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("696e746572666163657b457175616c28782063727970746f2e507269766174654b65792920626f6f6c3b205075626c696328292063727970746f2e5075626c69634b65797d", "_ᴛ2")]
[assembly: GoDynamicTypeLift("696e746572666163657b457175616c28782063727970746f2e5075626c69634b65792920626f6f6c7d", "_ᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b507269766174654b657920737472696e673b205075626c69634b657920737472696e673b20506565725075626c69634b657920737472696e673b2053686172656453656372657420737472696e677d", "vectorsᴛ1")]
[assembly: GoTypeAlias("Curve", "ΔCurve")]
[assembly: GoTypeAlias("PublicKey", "ΔPublicKey")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<PrivateKey, _ᴛ2>(Pointer = true)]
[assembly: GoImplement<nistCurve, ΔCurve>(Pointer = true, Production = true)]
[assembly: GoImplement<x25519Curve, ΔCurve>(Pointer = true, Production = true)]
[assembly: GoImplement<ΔPublicKey, _ᴛ1>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<ΔPublicKey, ж<ΔPublicKey>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("crypto/ecdh/ecdh.go", "ecdh.cs", "AEKIAZaiAAIU8oKClKiCABI2AA4CgpSolqIAAhTygoKUqIKmgqqi", "", "", "87=ConstantTimeCompare/1/1/1;151=ConstantTimeCompare/1/1/1")]
[assembly: go.GoPositionMap("crypto/ecdh/ecdh_test.go", "ecdh_test.cs", "ACc85uqCgoKClIKCloKClIKUgpaCgpSClIKWgoKUgoKWggAJFIKCgqaCgoKCggAGEICSABAKAB1UgoKCgoKUgpSCgpSCgpSCAAgMgoKClKaCgoKCAAgKgoKCgoSgkqDmgoKClIKClIKClIIADQgAK2yCgoKCgpKCkoIAEQwAIVyCgoKCgpKCkoIACwyCgJKAkoCSgLaCgoKClKiCgpSCgoSEgoKClIKCgpSCgpTKgoCSgJKAkoDskoKmACUwsoKUhIKCgoKWkoKCgoKUloKCgIKqooKCgoKUgqaCAAsIggAFFIKCgpaygpSigoKUgoKC", "39-83:1;98-113:1;159-180:1;192-197:1;206-206:1;207-207:2;283-294:1;344-355:1;359-359:1;360-360:2;361-361:3;362-362:4;366-401:1;405-405:1;406-406:2;407-407:3;408-408:4;456-464:1;511-521:1", "", "141=P256/1/1/6,P384/1/1/14,P521/1/1/25,X25519/1/1/34;190=hexDecode/1/1/1,Sum256/2/2/1;268=P256/1/1/1,Repeat/1/4/7,P384/1/1/15,Repeat/2/4/21,P521/1/1/29,Repeat/3/4/35,X25519/1/1/44,Repeat/4/4/50;341=P256/1/1/1,Repeat/1/3/5,P384/1/1/15,Repeat/2/3/19,P521/1/1/29,Repeat/3/3/33,X25519/1/1/43;569=P256/1/1/4,P384/1/1/5,P521/1/1/6,X25519/1/1/7")]
[assembly: go.GoPositionMap("crypto/ecdh/nist.go", "nist.cs", "ABkwgtaigoKClIKClNyWgpaCgpYACRSCgoKUgoKUgpTWooKCgpSCgpTcloKClAAJFNbGgpS4goKClJSCgpSUpgAJEIKUAAIQ0KQABRgAAxLQpAAFGAADEtCkAAUY", "", "", "45=Bytes/1/1/3;60=Bytes/1/2/2,PublicKey/1/2/6,Bytes/2/2/6,PublicKey/2/2/7;100=Clone/1/1/2,Bytes/1/1/3;112=Clone/1/1/2,PublicKey/1/2/6,Bytes/1/1/6,PublicKey/2/2/7;136=Clone/1/1/2")]
[assembly: go.GoPositionMap("crypto/ecdh/x25519.go", "x25519.cs", "ABQ20KT4gtaCgpSCgoCCpNaCgpSClIKCuO6CgpSClAAIDLKCgoKUpoKEgoKChOKCgoKEgoKCgoKCgoSCgoKCgoKCgoKCgoKEgoKCgpaChIKCqJKCgpQ=", "", "", "75=Clone/1/1/2;89=Clone/1/1/2")]
// </GoSourcePositionMaps>

namespace go.crypto;

[GoPackage("ecdh")]
public static partial class ecdh_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct nistCurve {}
    internal partial struct x25519Curve {}
    public partial interface ΔCurve {}
    public partial struct PrivateKey {}
    public partial struct ΔPublicKey {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcrypto() => builtin.initPackage(typeof(crypto_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸcipher() => builtin.initPackage(typeof(go.crypto.cipher_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸboring() => builtin.initPackage(typeof(go.crypto.@internal.boring_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140only() => builtin.initPackage(typeof(go.crypto.@internal.fips140only_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸecdh() => builtin.initPackage(typeof(go.crypto.@internal.fips140.ecdh_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸedwards25519ꓸfield() => builtin.initPackage(typeof(go.crypto.@internal.fips140.edwards25519.field_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸrandutil() => builtin.initPackage(typeof(go.crypto.@internal.randutil_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(go.crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha256() => builtin.initPackage(typeof(go.crypto.sha256_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsubtle() => builtin.initPackage(typeof(go.crypto.subtle_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(go.@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸcryptoꓸchacha20() => builtin.initPackage(typeof(vendor.golang.org.x.crypto.chacha20_package));
    // </ImportInitializers>
}

[GoPackage("ecdh_test")]
public static partial class ecdh_test_package
{
}
