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

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(cfb))]
[assembly: GoFieldAccess(typeof(ofb))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("crypto/cipher/cbc.go", "cbc.cs", "ABw6ggAQKLKClIKUgpSClK7CgpTGlIKClIKUgpSCppSUgqaCgrimgoKUAA0esoKUgpSClIKUrsKClMaUgoKUgpSClIKUgsqCgqamgoKUgoK4gqamgoKU", "", "", "30=BlockSize/1/2/2,Clone/1/1/3,BlockSize/2/2/4")]
[assembly: GoPositionMap("crypto/cipher/cfb.go", "cfb.cs", "ABowgoKUgpSCgoKmypSCgpSCggAFGgAJAoKUAAIYAAkCgpSmgoKUlO6U")]
[assembly: GoPositionMap("crypto/cipher/ctr.go", "ctr.cs", "ACZSooKUgpSAgqSClIKClAAOGoLGgoKCgoKCgqaCgoK4gqaCgpSClIKUgoKUgoKC", "", "", "60=Clone/1/1/2")]
[assembly: GoPositionMap("crypto/cipher/gcm.go", "gcm.cs", "AB484oKUAAIU8oKUAAIWAAgCgpTmgoKCgpS4goKUAAYaAAoCgoKUgoKUAAgOgqaCpoKCpoKClIKUggAYMoKCpoKmgoKUgqaCgpSC3IKCgoKUgqaCgpQAExSCgpSClIKUgpQADRiCpoKmgoKUgpSCpoKClIKmooKClJSCgpTGtIKClIKmgpSCpoKClIKmooKClIKUgoLKgqaUxqKCgpSCgoK4ooKCgpSCgpSCgoK4ooKmooKCgoKu8oKUgpSC")]
[assembly: GoPositionMap("crypto/cipher/io.go", "io.cs", "ABYmsoKCABEcsoKCgoKUqqKClA==")]
[assembly: GoPositionMap("crypto/cipher/ofb.go", "ofb.cs", "ABc+AAkCgqaCgpSCgpTugsaCgoKClIKCgoKClIKmgoKUgpSCgpSCgoI=")]
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

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(cipher_package.StreamReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class cipherᴛStreamReader;
[GoMethodHost(typeof(cipher_package.StreamWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class cipherᴛStreamWriter;
[GoMethodHost(typeof(cipher_package.aesCtrWrapper)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class cipherᴛaesCtrWrapper;
[GoMethodHost(typeof(cipher_package.cbcDecrypter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class cipherᴛcbcDecrypter;
[GoMethodHost(typeof(cipher_package.cbcEncrypter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class cipherᴛcbcEncrypter;
[GoMethodHost(typeof(cipher_package.cfb)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class cipherᴛcfb;
[GoMethodHost(typeof(cipher_package.ctr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class cipherᴛctr;
[GoMethodHost(typeof(cipher_package.gcmFallback)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class cipherᴛgcmFallback;
[GoMethodHost(typeof(cipher_package.gcmWithRandomNonce)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class cipherᴛgcmWithRandomNonce;
[GoMethodHost(typeof(cipher_package.ofb)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class cipherᴛofb;
// </MethodHolders>
