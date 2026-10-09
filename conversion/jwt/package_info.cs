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
global using bigꓸInt = go.math.big_package.ΔInt;
global using bigꓸRat = go.math.big_package.ΔRat;
global using cryptoꓸDecrypterOpts = object;
global using cryptoꓸPrivateKey = object;
global using cryptoꓸPublicKey = object;
global using jsonꓸToken = object;
global using jsonꓸΔToken = object;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static go.github.com.golang_jwt.jwt.jwt_package;

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
[assembly: GoDynamicTypeLift("696e746572666163657b63727970746f2e5075626c69634b6579207c205b5d75696e74387d", "VerificationKeyᴛ1")]
[assembly: GoTypeAlias("VerificationKey", "object")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<MapClaims, Claims>(Pointer = true)]
[assembly: GoImplement<MapClaims, Claims>]
[assembly: GoImplement<RegisteredClaims, Claims>(Pointer = true)]
[assembly: GoImplement<RegisteredClaims, Claims>]
[assembly: GoImplement<SigningMethodECDSA, SigningMethod>(Pointer = true)]
[assembly: GoImplement<SigningMethodEd25519, SigningMethod>(Pointer = true)]
[assembly: GoImplement<SigningMethodHMAC, SigningMethod>(Pointer = true)]
[assembly: GoImplement<SigningMethodRSA, SigningMethod>(Pointer = true)]
[assembly: GoImplement<SigningMethodRSAPSS, SigningMethod>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<joinedError, error>(Pointer = true)]
[assembly: GoImplement<signingMethodNone, SigningMethod>(Pointer = true)]
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
[assembly: go.GoPositionMap("github.com/golang-jwt/jwt/v5@v5.3.1/ecdsa.go", "ecdsa.cs", "AA8aAAwmhIKKgoqCqoLahIKUxJaCloKGgpSChoCCptqEgpTEmIKWgoaAgoSCloKCnIKChJQ=", "35-37:1;41-43:2;47-49:3")]
[assembly: go.GoPositionMap("github.com/golang-jwt/jwt/v5@v5.3.1/ecdsa_utils.go", "ecdsa_utils.cs", "AAsWgpiChoKAgqiCgIKAgsiCgoCCpqiChoKAgqiCgIKAgpTIgoKAgqY=")]
[assembly: go.GoPositionMap("github.com/golang-jwt/jwt/v5@v5.3.1/ed25519.go", "ed25519.cs", "AAsWAAYYgoLagtqCgoSAgqaCmIKW2oKChICCpoCCrIKClg==", "25-27:1")]
[assembly: go.GoPositionMap("github.com/golang-jwt/jwt/v5@v5.3.1/ed25519_utils.go", "ed25519_utils.cs", "AAwYgpiChoKAgqiCgIKmgoKAgqaogoaCgIKogoCCpoKCgIKm")]
[assembly: go.GoPositionMap("github.com/golang-jwt/jwt/v5@v5.3.1/errors.go", "errors.cs", "AAkUgoKCgoKCgoKCgoKCgoKCAAUUgoKClqqizoIAAiCigoKCgpSCloKCloI=")]
[assembly: go.GoPositionMap("github.com/golang-jwt/jwt/v5@v5.3.1/hmac.go", "hmac.cs", "ABIqloSCioKKgqqCAAUapIKCmIKckoKCmAAFFqKAgoKWkoSm", "27-29:1;33-35:2;39-41:3", "73=crypto.Hash.New;97=crypto.Hash.New")]
[assembly: go.GoPositionMap("github.com/golang-jwt/jwt/v5@v5.3.1/map_claims.go", "map_claims.cs", "AA0agtiC2ILYgtiC2IKsgoKClpSClqSElqqCgpS0tIKCgpS4rIKqgoKWgoKW")]
[assembly: go.GoPositionMap("github.com/golang-jwt/jwt/v5@v5.3.1/none.go", "none.cs", "AA8cgoKE2oLYhoCCpoKYqIKAgqY=", "18-20:1")]
[assembly: go.GoPositionMap("github.com/golang-jwt/jwt/v5@v5.3.1/parser.go", "parser.cs", "ABc+oqqClqqCAAcUgoKCmIKCgoKCgqaEqoSWgoKYgpSCmoKAgvikgpiEgpaAgrqEAA0S0oKCgpaGgoCCpICCqISCgpyEgJKUtoKEgJKUtoKYgIKAgraogoKWrIKCgoKUgoKClIiCgpSErIKEgoCCpJaClAACFKIAAhSi")]
[assembly: go.GoPositionMap("github.com/golang-jwt/jwt/v5@v5.3.1/parser_option.go", "parser_option.cs", "AAkagpK8goK8goK6goK+goK8goK8goK8goIAAxiikgADHKKSggADGIKCAAMYgoIAAxCCgr6Cgg==", "14-16:1;22-24:1;30-32:1;37-39:1;46-48:1;54-56:1;62-64:1;70-72:1;84-86:1;100-103:1;115-117:1;129-131:1;139-141:1;148-150:1")]
[assembly: go.GoPositionMap("github.com/golang-jwt/jwt/v5@v5.3.1/registered_claims.go", "registered_claims.cs", "AA5IgqiCqIKogqiCqII=")]
[assembly: go.GoPositionMap("github.com/golang-jwt/jwt/v5@v5.3.1/rsa.go", "rsa.cs", "ABQuhIKKgoqCqoLagoKEgIKogpSChtqCgoaAgqiCloKGgIKU", "26-28:1;32-34:2;38-40:3")]
[assembly: go.GoPositionMap("github.com/golang-jwt/jwt/v5@v5.3.1/rsa_pss.go", "rsa_pss.cs", "ABU2hAALGIoACxiKAAsY3oKClMSYgpSChIKCltqChJTEmIKWgoaAgpQ=", "41-43:1;58-60:2;75-77:3")]
[assembly: go.GoPositionMap("github.com/golang-jwt/jwt/v5@v5.3.1/rsa_utils.go", "rsa_utils.cs", "AAsWgoKYgoaCgIKmgoCCgILIgoKAgqYAAhCChoKAgqaEgoCCpoCCgILIgoKAgqaogoaCgIKogoCCgIKUgILqgoKAgqY=")]
[assembly: go.GoPositionMap("github.com/golang-jwt/jwt/v5@v5.3.1/signing_method.go", "signing_method.cs", "7pIABxyigoSiprKChICCpNiygoSClA==")]
[assembly: go.GoPositionMap("github.com/golang-jwt/jwt/v5@v5.3.1/token.go", "token.cs", "AB9SoqqiAAkegoKCloKCloSsgoKCloKClq6C", "", "", "41=Alg/1/1/3")]
[assembly: go.GoPositionMap("github.com/golang-jwt/jwt/v5@v5.3.1/types.go", "types.cs", "ABNMgqqCgqqCgoKUAAAYgoSErqKagIKmgIKmgoQABBCihICCpoSUtLSCgoKUxrSWhKaMgpY=")]
[assembly: go.GoPositionMap("github.com/golang-jwt/jwt/v5@v5.3.1/validator.go", "validator.cs", "ABqoAaKCAAISgq6ClJyAgqyAgqiCgIK6goCCuoKAgrqCgIK8goKAgriClgACFoKCgpaClgACFoKCgpaClgACFoKCgpaClgACFIKCgpqCgpaChIKomoKCqAACFIKCgpaClgACFIKCgpaClqqCgpS8goKU")]
// </GoSourcePositionMaps>

namespace go.github.com.golang_jwt.jwt;

[GoPackage("jwt", ImportPath = "github.com/golang-jwt/jwt/v5")]
public static partial class jwt_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct joinedError {}
    public partial interface Claims {}
    public partial interface ClaimsValidator {}
    public partial interface SigningMethod {}
    [GoLocalName("VerificationKey")] public partial interface VerificationKeyᴅ {}
    public partial interface VerificationKeyᴛ1 {}
    public partial struct ClaimStrings {}
    public partial struct MapClaims {}
    public partial struct NumericDate {}
    public partial struct Parser {}
    public partial struct RegisteredClaims {}
    public partial struct SigningMethodECDSA {}
    public partial struct SigningMethodEd25519 {}
    public partial struct SigningMethodHMAC {}
    public partial struct SigningMethodRSA {}
    public partial struct SigningMethodRSAPSS {}
    public partial struct Token {}
    public partial struct Validator {}
    public partial struct VerificationKeySet {}
    public partial struct signingMethodNone {}
    public partial struct unsafeNoneMagicConstant {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcrypto() => builtin.initPackage(typeof(crypto_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸecdsa() => builtin.initPackage(typeof(go.crypto.ecdsa_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸed25519() => builtin.initPackage(typeof(go.crypto.ed25519_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸhmac() => builtin.initPackage(typeof(go.crypto.hmac_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(go.crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrsa() => builtin.initPackage(typeof(go.crypto.rsa_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸx509() => builtin.initPackage(typeof(go.crypto.x509_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbase64() => builtin.initPackage(typeof(encoding.base64_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸpem() => builtin.initPackage(typeof(encoding.pem_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(go.math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}
