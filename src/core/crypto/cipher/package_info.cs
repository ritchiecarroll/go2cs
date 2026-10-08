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
// </ImportedTypeAliases>

using go;
using static go.crypto.cipher_package;

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
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<aesCtrWrapper, Stream>]
[assembly: GoImplement<cbcDecrypter, BlockMode>(Pointer = true)]
[assembly: GoImplement<cbcEncrypter, BlockMode>(Pointer = true)]
[assembly: GoImplement<cfb, Stream>(Pointer = true)]
[assembly: GoImplement<ctr, Stream>(Pointer = true)]
[assembly: GoImplement<gcmFallback, AEAD>(Pointer = true)]
[assembly: GoImplement<gcmWithRandomNonce, AEAD>]
[assembly: GoImplement<go.crypto.@internal.fips140.aes.gcm_package.GCM, AEAD>(Pointer = true)]
[assembly: GoImplement<go.crypto.@internal.fips140.aes_package.CBCDecrypter, BlockMode>(Pointer = true)]
[assembly: GoImplement<go.crypto.@internal.fips140.aes_package.CBCEncrypter, BlockMode>(Pointer = true)]
[assembly: GoImplement<ofb, Stream>(Pointer = true)]
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
[assembly: go.GoPositionMap("crypto/cipher/cbc.go", "cbc.cs", "ABs6ggAQKLKClICCpIKUgIKkrsKClKaApIKClIKUgpSAgqaElIKWgoKopoKClAALHrKClICCpIKUgIKkrsKClKaApIKClIKUgpSAgqSCuoKClpaCgoSCgqiClqaCgpQ=", "", "", "29=BlockSize/1/2/2,Clone/1/1/3,BlockSize/2/2/4")]
[assembly: go.GoPositionMap("crypto/cipher/cfb.go", "cfb.cs", "ABcwgoKUgpSCgoKWypSCgpSCggADGgAJAoKUAAIYAAkCgpSmgpKUlO6E")]
[assembly: go.GoPositionMap("crypto/cipher/ctr.go", "ctr.cs", "ACVSooCCpIKUgIKkgpSCgpQADBqCpoKCgoKCgoKWgoKCuIKmgoKUgpSAgqSCgpSCgoI=", "", "", "61=Clone/1/1/2")]
[assembly: go.GoPositionMap("crypto/cipher/gcm.go", "gcm.cs", "ABw84oKUAAIU8oKUAAIWAAgCgpTWgoKCgpS4goKUAAUaAAoCgoKUgoKU7oKmgqaCgpaCgpSClIIAFzKCgpaCpoKClIKWgoKUgtyCgoKClIKWgoKUAA4UgoKUgpSAgqSClAALGIKmgqaCgpSClIKWgoKUgpaigoKEhIKChKaUgoKUgpaClIKWgoKUgpaigoKEgoSCgsqCloSmooKClIKCgriigoKChIKClIKCgriigqaigoKCgq7ygIKUgqSC")]
[assembly: go.GoPositionMap("crypto/cipher/io.go", "io.cs", "ABMmsoKCAA0csoKCgpKUqqKAgqQ=")]
[assembly: go.GoPositionMap("crypto/cipher/ofb.go", "ofb.cs", "ABY+AAkCgpaCgpSCgpTegqaCgoKClIKCgoKClIKmgoKUgpSCgpSCgoI=")]
// </GoSourcePositionMaps>

namespace go.crypto;

[GoPackage("cipher")]
public static partial class cipher_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface cbcDecAble {}
    internal partial interface cbcEncAble {}
    internal partial interface ctrAble {}
    internal partial interface gcmAble {}
    internal partial struct aesCtrWrapper {}
    internal partial struct cbc {}
    internal partial struct cbcDecrypter {}
    internal partial struct cbcEncrypter {}
    internal partial struct cfb {}
    internal partial struct ctr {}
    internal partial struct gcmFallback {}
    internal partial struct gcmWithRandomNonce {}
    internal partial struct ofb {}
    public partial interface AEAD {}
    public partial interface Block {}
    public partial interface BlockMode {}
    public partial interface Stream {}
    public partial struct StreamReader {}
    public partial struct StreamWriter {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140only() => builtin.initPackage(typeof(go.crypto.@internal.fips140only_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸaes() => builtin.initPackage(typeof(go.crypto.@internal.fips140.aes_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸaesꓸgcm() => builtin.initPackage(typeof(go.crypto.@internal.fips140.aes.gcm_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsubtle() => builtin.initPackage(typeof(go.crypto.subtle_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    // </ImportInitializers>
}
