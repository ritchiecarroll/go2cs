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
using static go.crypto.cipher_test_package;

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
[assembly: GoDynamicTypeLift("7374727563747b6b657920737472696e673b20697620737472696e673b20706c61696e7465787420737472696e673b206369706865727465787420737472696e677d", "cfbTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6b657920737472696e673b206e6f6e636520737472696e673b20706c61696e7465787420737472696e673b20616420737472696e673b20726573756c7420737472696e677d", "aesGCMTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b206b6579205b5d627974653b206976205b5d627974653b20696e205b5d627974653b206f7574205b5d627974657d", "cbcAESTestsᴛ1")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<aesCtrWrapper, Stream>]
[assembly: GoImplement<cbcDecrypter, BlockMode>(Pointer = true, Production = true)]
[assembly: GoImplement<cbcEncrypter, BlockMode>(Pointer = true, Production = true)]
[assembly: GoImplement<cfb, Stream>(Pointer = true, Production = true)]
[assembly: GoImplement<ctr, Stream>(Pointer = true, Production = true)]
[assembly: GoImplement<gcmFallback, AEAD>(Pointer = true, Production = true)]
[assembly: GoImplement<gcmWithRandomNonce, AEAD>]
[assembly: GoImplement<go.crypto.@internal.fips140.aes.gcm_package.GCM, AEAD>(Pointer = true, Production = true)]
[assembly: GoImplement<go.crypto.@internal.fips140.aes_package.CBCDecrypter, BlockMode>(Pointer = true, Production = true)]
[assembly: GoImplement<go.crypto.@internal.fips140.aes_package.CBCEncrypter, BlockMode>(Pointer = true, Production = true)]
[assembly: GoImplement<ofb, Stream>(Pointer = true, Production = true)]
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
[assembly: GoFieldAccess(typeof(go.os.exec_package.Cmd))]
[assembly: GoFieldAccess(typeof(ofb))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("crypto/cipher/benchmark_test.go", "benchmark_test.cs", "AA0cooKUgoKCgoKUgoK4ooKUgoKCgoKUlIKCuIKCgpSCpoKUgsqilIKCgpSCggALFoKCgoKClIKUgtyigpSCgoKCgriigpSCgoKCgg==", "52-54:1;55-57:2;59-61:3;62-64:4;92-102:1;93-95:1.1;96-98:1.2;99-101:1.3")]
[assembly: GoPositionMap("crypto/cipher/cbc.go", "cbc.cs", "ABs6ggAQKLKClICCpIKUgIKkrsKClKaApIKClIKUgpSAgraUlIKmgoK4poKClAALHrKClICCpIKUgIKkrsKClKaApIKClIKUgpSAgqSCyoKCpqaCgpSCgriCpqaCgpQ=", "", "", "29=BlockSize/1/2/2,Clone/1/1/3,BlockSize/2/2/4")]
[assembly: GoPositionMap("crypto/cipher/cbc_aes_test.go", "cbc_aes_test.cs", "ABsqACtcgqaCgoKCgqaUgpSCgsqCpoKCgoKCppSClIKC")]
[assembly: GoPositionMap("crypto/cipher/cbc_test.go", "cbc_test.cs", "ABkokoKCgpSClIKCpsqClIKUgoKm+IKCgoKUgqaCgoI=", "21-37:1;23-35:1.1;39-51:2")]
[assembly: GoPositionMap("crypto/cipher/cfb.go", "cfb.cs", "ABcwgoKUgpSCgoKmypSCgpSCggADGgAJAoKUAAIYAAkCgpSmgoKUlO6U")]
[assembly: GoPositionMap("crypto/cipher/cfb_test.go", "cfb_test.cs", "ABssABo6goKCgpSCgpSCgpSCgqaCgqaCgpSCpoKClILKgoKCgqaCgoKCgoKUgoKClIIACQiEhIKUgpSCgqaSlJLKgpSClIKCppKUkg==", "122-139:1;133-135:1.1;136-138:1.2;142-159:2;153-155:2.1;156-158:2.2")]
[assembly: GoPositionMap("crypto/cipher/common_test.go", "common_test.cs", "AAgS7pTKyg==")]
[assembly: GoPositionMap("crypto/cipher/ctr.go", "ctr.cs", "ACVSooCCpIKUgIKkgpSCgpQADBqCpoKCgoKCgoKmgoKCuIKmgoKUgpSAgqSCgpSCgoI=", "", "", "61=Clone/1/1/2")]
[assembly: GoPositionMap("crypto/cipher/ctr_aes_test.go", "ctr_aes_test.cs", "AB46pAAnXIKmgoKUgoKCpoKCgoKCgILIgoKCgoKAgsiCyoIACwyygoKClICCtpQABhaysoKClJSClIKUggALGoKCgoKClIKUAAsYsoKClIKigoKClJSmgsqSwpLCgoKCgoKCAAkUgoKClAAWDLKUgpQADBqCsoKCgoKmsoKypoKUgoKCggAMFpKCpoKClIKygpSCgpSCpoKUhJKCgpSCgpSCgoLegtyCgoKClA==", "148-171:1;203-236:1;221-234:1.1;223-232:1.1.1;271-298:1;279-296:1.1;281-294:1.1.1;313-352:1", "", "176=extractCounters/1/2/6,extractCounters/2/2/7;305=parseHex/1/10/1,parseHex/2/10/2,parseHex/3/10/3,parseHex/4/10/4,parseHex/5/10/5,parseHex/6/10/6,parseHex/7/10/7,parseHex/8/10/8,parseHex/9/10/9,parseHex/10/10/10")]
[assembly: GoPositionMap("crypto/cipher/ctr_test.go", "ctr_test.cs", "ABQmgKKAooCkgoKCgsqCgriCgoKCgoKUgoKCgoKUgoKCyoKCgoKUgpSCgqbKgpSClIKCpriCgoKC", "62-78:1;64-76:1.1;80-92:2")]
[assembly: GoPositionMap("crypto/cipher/gcm.go", "gcm.cs", "AB084oKUAAIU8oKUAAIWAAgCgpTmgoKCgpS4goKUAAYaAAoCgoKUgoKU7oKmgqaCgqaCgpSClIIAGDKCgqaCpoKClIKmgoKUgtyCgoKClIKmgoKUABEUgoKUgpSAgqSClAALGIKmgqaCgpSClIKmgoKUgqaigoKUlIKClKaUgoKUgqaClIKmgoKUgqaigoKUgpSCgsqCppSmooKClIKCgriigoKClIKClIKCgriigqaigoKCgq7ygIKUgqSC")]
[assembly: GoPositionMap("crypto/cipher/gcm_fips140v1.26_test.go", "gcm_fips140v1.26_test.cs", "ACQqgoKCgoKCgoKUpoKCgpSSgoKmsoKSgoKmoqSigoKCgpSCgoKCgpSmypSCgoKCgoKCgpSCgoKCgoKUgoKCgg==", "34-38:1;39-44:2;45-54:3;47-52:3.1;56-104:4;57-62:4.1;63-71:4.2")]
[assembly: GoPositionMap("crypto/cipher/gcm_test.go", "gcm_test.cs", "ACA03ICigKKAppIACAaCgoKCgpSmgoKCgpT6AOACxgWC1oKCgpSCgoKUgoKmgoLagoKUyIKCyIKCyIKAgoK2goKCpoKCpoKCgIKkpoKAgqSUgoCCpLiCpoKClIKCgsqC9tyCgpSClIKCpoKCpoKmgoLKguaUAAgWgoKCgoKCgpSCgpSCggARCsaygoKUgoKUgoKUqJKCgpSCmISCgoKClIKCgoKUypKCgIKkgoKUgIKkgIKkgoCCpIKCgpSCgpSClJSCgoKCgoKCgIKCgoKCggAHEJIACQaClJSClIKUuLaSyIKS2pKAgqQAEAyCgoKSlIKSlIKSlIKCgIKkkgAMCoKCgoKCgoKClKaCgoKUkoKCprKCkoKCpqKkooKCgoKmgoKCgoKCgoKCgpSUgpSUgoKUlIKClIKUlIKUpqKCgoKCpoKCgoLKgoKCgoKCgoKmgoKCgoKCgoKUgoKCgoKCgoKCgoKCuIKCgoKU", "42-50:1;43-49:1.1;51-59:2;52-58:2.1;596-610:1;642-674:2;705-736:1;715-715:1.1;718-720:1.2;719-719:1.2.1;724-726:1.3;725-725:1.3.1;730-735:1.4;734-734:1.4.1;741-762:1;742-745:1.1;746-749:1.2;750-753:1.3;754-761:1.4;778-782:1;783-788:2;789-798:3;791-796:3.1;800-846:4;801-806:4.1;848-899:5;849-854:5.1;856-863:5.2")]
[assembly: GoPositionMap("crypto/cipher/io.go", "io.cs", "ABMmsoKCAA0csoKCgoKUqqKAgqQ=")]
[assembly: GoPositionMap("crypto/cipher/modes_test.go", "modes_test.cs", "ABQsgu6CpoKCgoCCAAgQgqaCpoKCgoCCpIKAggAIEIKmgoKCgpSAgsyipoKUgoKAgraCgoCCpIKAgraCgoCCyKKCgoKCpg==")]
[assembly: GoPositionMap("crypto/cipher/ofb.go", "ofb.cs", "ABY+AAkCgqaCgpSCgpTugqaCgoKClIKCgoKClIKmgoKUgpSCgpSCgoI=")]
[assembly: GoPositionMap("crypto/cipher/ofb_test.go", "ofb_test.cs", "AB8+ACdQgoKUgoKCpoKCgoKCgriCgoKCgoK4gsqEhIKUgpSCgqa4gpSClIKCpg==", "111-123:1;126-138:2")]
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
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸaes() => builtin.initPackage(typeof(go.crypto.aes_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸdes() => builtin.initPackage(typeof(go.crypto.des_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸboring() => builtin.initPackage(typeof(go.crypto.@internal.boring_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸcryptotest() => builtin.initPackage(typeof(go.crypto.@internal.cryptotest_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140() => builtin.initPackage(typeof(go.crypto.@internal.fips140_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140only() => builtin.initPackage(typeof(go.crypto.@internal.fips140only_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸaes() => builtin.initPackage(typeof(go.crypto.@internal.fips140.aes_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸaesꓸgcm() => builtin.initPackage(typeof(go.crypto.@internal.fips140.aes.gcm_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(go.crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsubtle() => builtin.initPackage(typeof(go.crypto.subtle_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(go.@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(go.math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}

[GoPackage("cipher_test")]
public static partial class cipher_test_package
{
}
