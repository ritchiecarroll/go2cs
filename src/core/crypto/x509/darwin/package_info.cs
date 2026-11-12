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
global using cryptoꓸPublicKey = object;
// </ImportedTypeAliases>

using go;
using static go.crypto.x509_package;

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
[assembly: GoDynamicTypeLift("63727970746f2f783530392e7075624b6579457175616c", "alreadyInChain_pubKeyEqual")]
[assembly: GoDynamicTypeLift("696e746572666163657b457175616c2863727970746f2e5075626c69634b65792920626f6f6c7d", "alreadyInChain_pubKeyEqual")]
[assembly: GoDynamicTypeLift("696e746572666163657b71756572792863727970746f2f783530392e726663323832314d61696c626f78292028737472696e672c20626f6f6c297d", "newEmailConstraints_R0")]
[assembly: GoDynamicTypeLift("696e746572666163657b7175657279286e65742e49502920282a6e65742e49504e65742c20626f6f6c297d", "newIPNetConstraints_R0")]
[assembly: GoDynamicTypeLift("696e746572666163657b717565727928737472696e67292028737472696e672c20626f6f6c297d", "newDNSConstraints_R0")]
[assembly: GoDynamicTypeLift("7374727563747b616c676f2063727970746f2f783530392e5369676e6174757265416c676f726974686d3b206e616d6520737472696e673b206f696420656e636f64696e672f61736e312e4f626a6563744964656e7469666965723b20706172616d7320656e636f64696e672f61736e312e52617756616c75653b207075624b6579416c676f2063727970746f2f783530392e5075626c69634b6579416c676f726974686d3b20686173682063727970746f2e486173683b20697352534150535320626f6f6c7d", "signatureAlgorithmDetailsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6578744b657955736167652063727970746f2f783530392e4578744b657955736167653b206f696420656e636f64696e672f61736e312e4f626a6563744964656e7469666965727d", "extKeyUsageOIDsᴛ1")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<CertificateInvalidError, error>]
[assembly: GoImplement<ConstraintViolationError, error>]
[assembly: GoImplement<HostnameError, error>]
[assembly: GoImplement<InsecureAlgorithmError, error>]
[assembly: GoImplement<SystemRootsError, error>]
[assembly: GoImplement<UnhandledCriticalExtension, error>]
[assembly: GoImplement<UnknownAuthorityError, error>]
[assembly: GoImplement<dnsConstraints, newDNSConstraints_R0>(Pointer = true)]
[assembly: GoImplement<emailConstraints, newEmailConstraints_R0>(Pointer = true)]
[assembly: GoImplement<go.crypto.rsa_package.PSSOptions, crypto_package.SignerOpts>(Pointer = true)]
[assembly: GoImplement<ipConstraints, newIPNetConstraints_R0>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(CertPool))]
[assembly: GoFieldAccess(typeof(Certificate))]
[assembly: GoFieldAccess(typeof(CertificateRequest))]
[assembly: GoFieldAccess(typeof(RevocationList))]
[assembly: GoFieldAccess(typeof(RevocationListEntry))]
[assembly: GoFieldAccess(typeof(chainConstraints))]
[assembly: GoFieldAccess(typeof(dnsConstraints))]
[assembly: GoFieldAccess(typeof(emailConstraints))]
[assembly: GoFieldAccess(typeof(go.crypto.dsa_package.PublicKey))]
[assembly: GoFieldAccess(typeof(go.crypto.ecdsa_package.PrivateKey))]
[assembly: GoFieldAccess(typeof(go.crypto.ecdsa_package.PublicKey))]
[assembly: GoFieldAccess(typeof(go.crypto.elliptic_package.CurveParams))]
[assembly: GoFieldAccess(typeof(go.crypto.rsa_package.PrivateKey))]
[assembly: GoFieldAccess(typeof(go.crypto.rsa_package.PublicKey))]
[assembly: GoFieldAccess(typeof(go.crypto.x509.pkix_package.AlgorithmIdentifier))]
[assembly: GoFieldAccess(typeof(go.crypto.x509.pkix_package.AttributeTypeAndValue))]
[assembly: GoFieldAccess(typeof(go.crypto.x509.pkix_package.Extension))]
[assembly: GoFieldAccess(typeof(go.encoding.pem_package.Block))]
[assembly: GoFieldAccess(typeof(go.net.url_package.URL))]
[assembly: GoFieldAccess(typeof(nameConstraintsSet<,>))]
[assembly: GoFieldAccess(typeof(net_package.IPNet))]
[assembly: GoFieldAccess(typeof(pkcs1PublicKey))]
[assembly: GoFieldAccess(typeof(policyGraphNode))]
[assembly: GoFieldAccess(typeof(rfc1423Algo))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("crypto/x509/cert_pool.go", "cert_pool.cs", "ADyAAZIABxDCgpSokoKoktyCgoKUopSCAAQcAAsCgqYACRQACAKCAAISAAgCgoKClIKUtrTIgoKUgoKCgqaygpSosoKUAAIU8oK4gqaCygALEgAIAoKCgoKUgqaCgoKUiIKUgpSUpgACEPKCgpSowoKUgpSigqYAAhDygpQ=", "184-186:1;239-246:1;240-244:1.1;291-293:1")]
[assembly: GoPositionMap("crypto/x509/constraints.go", "constraints.cs", "AEqUAdKCppSClIKCgoKmAAIQAAkCgqaUgqaCpoKClJSClMaigpSCgpSmsoKClKaipoIAEByGgpSSgoKUppKCppSCppTGgoKClJSClAAEHAALAoKCgpSCgoKCgpSClIKUpoKm7riCgpSUAAwaAAIWAAgCgpSCgoKCgoKUgoKUpoKCgoKUABUqgoKUgoKmlO6UgoKigoKClJSCuMaCgqaCgqaCgoKCpgAUHqaClIKCgoKClILKlIKUpoKUxoKCgqaClIIAEhKigoKmgoKmAA0agoKCpoKClIKmgoKUgqaCgpSCpsaCgoKygpTugoKCgoKmgqaCgpSCgoKUuIKClIKCpoKCygAJEILGgoKCgoKUgoKCgu6AgraUpoKCgoKClIKUpoKClJQ=", "", "", "574=newIPNetConstraints/1/2/1,newIPNetConstraints/2/2/1,newDNSConstraints/1/4/2,newDNSConstraints/2/4/2,newDNSConstraints/3/4/3,newDNSConstraints/4/4/3,newEmailConstraints/1/2/4,newEmailConstraints/2/2/4")]
[assembly: GoPositionMap("crypto/x509/oid.go", "oid.cs", "ABMmAAYSkoK2goKmgriClIK4qJKCpoKCpoKCgpSmgoKUpoKCgoKClJSmooKUpoKCpoKCgoKUlMiSqJKokqa4goKCuKqCgoKUlKqAgqSAgraCpoKUlIKCgoKClKaCqJKokqiSgoKUgqi2xsKCgqaCgpSCpoKClIKCgpSClKaCzLKClIKmlIKSgqaSgriCgoKmlIK4qJKCgqrOgoKCgoKmgoKUgpSCgoKCgpSCgoKCgpSUgoKCgoKCgpSCppSCpqaClLyUgoKmgpSCgoKClIKUgpSCuAAICLKCgoKUlA==")]
[assembly: GoPositionMap("crypto/x509/parser.go", "parser.cs", "AChGogAXMOIAChaClJSkgoKmpIKUAAoUgriCpoKCuIaUgqakgoKUpIKCppQADArCgqaCgoKCgpSCgoKUgoKUgoKClIKCgpSmpgAIBoKCgpSClIKCgpSCggAKBqKClILGgsaUpqKCgpSCgqYACgaCgoKUgoKmgoKUggAiBoKCgoK4gqaCgoKUgpSCpoKUgqa4pIKCgpSCgpTYgpSClMiClLSCgoKUAAcQgoiUhJSk+IKCgqaCgoKm5qKCgpSCgriCgoKClKYACAaigpSCgoKClIK4AAoG4oKUgoKUtIKClLSCgpSCgpSClLSUtMimAAgGlJSUgoKClIKClJTmwoKCgpSCgoKUgpSmAAgGsoKCgpSCgoKClIKUgoKClJS4kpSCgoKmppTmuAAIBgASIIKikoqmyqYACAKCkoKEps6UgoKmgpS2gqSUgraCtqaCpraCgsqCgqaCpraCgqaCpoKUtsjmgpSClJQAGgaigoKUgpSCgsaCgpSCtIKCppTIgoIAECCCgpSCgoKUgoKClIKUgpSCgpSCgpTagoLGgoKUgoKClJSClJSCgoKUlIKUxoKCxpSUgoKClLSCgpSCgoLYgoKUgoKShpTGgoKUxqSUlJSCgpSCgoKUgoKUgpSClJS02qaCuKYAKQSClLiClIKCpqaClIKCpoKUgriCgqaCgpSCgpSmlIKCmrKClIKUgoKUlIKClIKCgpSUgoKUgoKmgoKUgoKClJSCgpSCgpSCgpSSgpSCgoKUgsiCuIKClIKUgoKCgpSCgoKUgoKClIKClIKClIKUgoLcgoKUlAAGENKCgpSClKrCgoKCgpSClAAREqKUuIKUgoKmpoKUgoKmgoKUgpSCpoKCmrKClIKUgoKUlIKClJSCgpSCgoKUlIKClIKCgriCgoKUgpSCgpSCgqaCgpSCgpSCgoKUgoKCgpSCgpSCgoKmuILKuIKCgpSCgpSCgoKUgoKUgoKClIKCgoKmuKoADRiCyoIACBKCgqaClJSCgoKUgty4", "401-438:1;568-659:1", "", "425=ReadASN1Integer/1/3/1,ReadASN1Integer/2/3/2,ReadASN1Integer/3/3/3;428=Sign/3/4/1,Sign/4/4/1;727=Empty/1/2/1,ContextSpecific/1/2/2,Constructed/1/2/2,ReadOptionalASN1/1/2/2,ContextSpecific/2/2/3,Constructed/2/2/3,ReadOptionalASN1/2/2/3,Empty/2/2/4;752=ReadAnyASN1/1/1/1;1055=ReadASN1/2/3/1,ReadASN1/3/3/2")]
[assembly: GoPositionMap("crypto/x509/pem_decrypt.go", "pem_decrypt.cs", "AC1cACNIsoKClIKCgoKCgpQACBIACAKCuAAPHAAOAoKCpoKCpoKClIKClILKgoKCpoKmgoIABxCCgpSCgpSClIKCpgACFPKCgpSCgriCgoKUgoK4lIKUlAAJFIKCgoKmpoKCgoKm", "", "", "251=EncodeToString/1/1/4")]
[assembly: GoPositionMap("crypto/x509/pkcs1.go", "pkcs1.cs", "AC5cAA0Q4oKCgpSCgpSClKaCpoimgsqCgoKCgoKCgoKUyoKmgoKCgoKCgrimAAIYAAsClIKCpgALGIKCgoKmggAKDLKCgoKClJSCpoKUgqYABRLSuA==", "", "", "87=Sign/5/7/1,Sign/6/7/2,Sign/7/7/3")]
[assembly: GoPositionMap("crypto/x509/pkcs8.go", "pkcs8.cs", "ACZMAA8CgoKClIKUlJSCgpSmgoKClIKClKaAgqSCgpSAgqSmgIKkgoKUtgAHGgAJApSUuIKAgqS2goKUgoKU3ILIpoKClLaCpoKCpoKClIKClNyC6qY=")]
[assembly: GoPositionMap("crypto/x509/root.go", "root.cs", "AB4+ooKCgtaigpSCgoKmgqaUgqaCpqKkAAEeAA8CgqaClIKU3IKCgqaApIKUgqaCpqI=", "103-103:1")]
[assembly: GoPositionMap("crypto/x509/root_darwin.go", "root_darwin.cs", "ABYa8oKCgoKUgoKCgoKUgoKUuIKCgoKUlIKClJSCgoKCAAgSgpSkpKS4goKClIKCgoKClKaUpoKmlIK4goK4goK4gqYACAiSgoKUpoI=")]
[assembly: GoPositionMap("crypto/x509/sec1.go", "sec1.cs", "ACNIsgAGEPKCgqaqooKClIKClAAHFMIAChbigoKClIKUlIKmgoKUlIKmgriCgpTclA==", "", "", "87=Bytes/1/2/2,PublicKey/1/1/3,Bytes/2/2/3")]
[assembly: GoPositionMap("crypto/x509/verify.go", "verify.cs", "AF+UAYKUpKSkpKSkpKSkgoKUlAARFIKClIKmgpSClIKUgoKUpoKUpoKUABUcgoKCgoKClKaUAA8QgoKClKbYADqGAYLO8oKmlAAKFoKCgoKUgpSUyIKUiIKUAAsktuqClJTugoKUAAIUgqbYgu6ChriClMqCpoKCqsKCgoKClIKCpsqUpoKUpoKUygAICtKCpoKCgriCgpSCyoLugoIAFCqCpoKCgrgAAkYAJgaClIKCgpSCyqaCgpSCuILKgoKCuIKCpoKCgriCgpSCgriCgpSCuIKmgoKCgoKCuIKClIKClJSClKaCgpSCgoKUlIKUgqbGgoKCggAGENKKsoKCuIKCyoKUgrKCgqaCgoKUgqYACBIACATCqpKClIKCgqaCpoKCgpSmgoKCgpSmgoKCgpS4lLSCgsiCvoKCguqClIKmxoKasoKUgpSmpoKUlLiUgoKUgpSClIKUppS4poKClKaClIKmlIKmgoKUgrisxIKCpoKUgoK4gqaCgoKmAAQYAAwEgoKUpoKCpqaCgpTcgoKmgsrGgqaCgpSCptyCgoKUpoKUuJSCgoKmgoK4goKCAAgKpoKCgpQAChiC3IKClAAKFJSC3PyipoKClKaCgpSmgoKUpoKmgqaCgoKUgpSClKaigoKCgqaCgoLctoKCgoKClNyCgoKCuILGAAwWgriUgqKClIKUgqaCgriCpoKUpoK4gqaClKaClIK4gpSCpoIACBKCgoKCgoLKgsqUlJSUgoKUlKbKlJSCgpSCgu6UgoKUgpSCyoKUgqaC3IK4gqikgpSCyoKCppSUgoK4goLKgqbcAAoaooKCgqY=", "624-645:1;736-785:1", "", "422=Contains/1/1/2;515=Format/1/2/3,Format/2/2/3,Sprintf/1/1/3;521=Format/1/2/3,Format/2/2/3,Sprintf/1/1/3")]
[assembly: GoPositionMap("crypto/x509/x509.go", "x509.cs", "AEuQAQAIAoKCgpSCgpSmwpS4gpSmtIKClIKClIKCgoKUtIK0goKUgoKUgoKCgpTWpgACGAAJAoKClIKmAAcQggBBhgGCgoKmpoKCgqamgoKCpgAMGgAIDoKClAA0aIKCgoKCgoKCgoKCgpSCgpTKAAsGABI2AAgUgoIADR6CpoK4goKCptyCgIK2goCCAAgQiqaUpKSmAAsagsrKgpyylKSkpJQAEiiCgoKWgpSkpKSUpoKUpKSkpqaClKSkpKYAH0iCgoKCgoKCgoKCgoKCABkyABAosoKCpqaygoKmyJKCgpSCgpQArwGEA/yCABAQgsaygpSmgq4ACwyEpoKmgqYAAhTypoKmgoKCpsaCAAgKwoKUgoKCgrikgra2gpSkgpSCgqaUgpSClMaClIKUpIKUgpSUzNKCAA0KggAmToKCgoKsspSClIKClLiogoKCgoKCgoKCgoKYgpqigoKmqqKCgoKUlIKClJSUgoKUlIKCgpSUpoKUgrimlLKSlISCgpSmhIKClKaCgoKUpoKCgoKUpoKCgoKUpoSCgoLKgsqCgpSmhLiCgoKUpoKEgoKUpoqClIKCgoKCpqKUgoKmgoLKgqKiyoKCpoKCyoKCpoKCyqaCgqaCgqaCooKSuIKSyoKClKaElIKC7qaCgpTupoKUgoKUgoKmgoKC5oKUgoKClLiUgoKmgriClIKC5oKUgqKCgrKSkoKClMqCktyCgqaClISCgqbcpoKCpgAMDLKCgpSkgraClLS0tMiCxqaCpoKCgpSCpu6mgpSCgtyCgriCpsoAD4IBAEICgoKmgriCgtyCAAcQgqaCpoKmgoKmgoKUgqaCgqaCgqaCgqaCgoLKgtyCqoiCgqaCgqaCAAoWgoKUlIKCpu64AAQW4oKCgqbssoKCgoKUAAgQ8oKCpoKCuIKigqYACRSCgoKCgpSmgoKUlIKCpgBAjgHYooKCgpSCgpSClKiSgoKCpoKmAAwKpIyCgoKAlLaCpoKAgqSCgoKUlKYABywAFgKCgqaCgqaCgoKCpoKCuIKCgoLcgpSCgsqUgoK4gpSCpqYABxCCgriCgsqCAAQQgoKmgoKmpoKCgoK4AAwagoKUlIKCpgAGEqKUgoKCgqbmggAOHoKCgoK4goKCgqaUgqaClIKC2siSAIoBlAIAEAKClIKUgpSClIKUgqaCgqamlIKigsqCgoKUgqbugoKClMqCgoKm3IKUuIKCpoKUgoK4goKmABAigqaCpoKCypSCgqYACBLChKaCpoKm", "1302-1308:1;1310-1358:2;1318-1322:2.1;1319-1321:2.1.1;1326-1330:2.2;1327-1329:2.2.1;1338-1342:2.3;1339-1341:2.3.1;1350-1354:2.4;1351-1353:2.4.1;1371-1383:3;1373-1375:3.1;1379-1381:3.2;1476-1497:1;1480-1488:1.1;1481-1487:1.1.1;1492-1494:1.2", "", "482=Equal/2/4/1,Equal/3/4/2,Equal/4/4/3;1270=oidInExtensions/1/1/1;1278=oidInExtensions/1/1/1;1312=oidInExtensions/1/1/1;1334=oidInExtensions/1/1/1;1348=oidInExtensions/1/1/1;1356=oidInExtensions/1/1/4;1460=oidInExtensions/1/1/1;1590=oidInExtensions/1/1/1;1898=UTC/1/2/5,UTC/2/2/5;2002=ToRDNSequence/1/1/3,UTC/1/2/4,UTC/2/2/5;2372=RightAlign/1/1/6,getSignatureAlgorithmFromAI/1/1/7,getPublicKeyAlgorithmFromOID/1/1/9,parseRawAttributes/1/1/12;2630=UTC/1/1/2;2685=UTC/1/2/4,UTC/2/2/5")]
[assembly: GoPositionMap("crypto/x509/x509_string.go", "x509_string.cs", "AAgOhqKCgoKCgoKCggATHraClIKkpKSkpKSkpNaGooKCgoKCgoKCgoKCgoLatIKCgpQ=")]
// </GoSourcePositionMaps>

namespace go.crypto;

[GoPackage("x509")]
public static partial class x509_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("pubKeyEqual")] internal partial interface alreadyInChain_pubKeyEqual {}
    internal partial interface constraints_permitted<T, V> {}
    internal partial interface newDNSConstraints_R0 {}
    internal partial interface newEmailConstraints_R0 {}
    internal partial interface newIPNetConstraints_R0 {}
    internal partial struct AppendCertsFromPEM_lazyCert {}
    internal partial struct CreateCertificateRequest_attr {}
    internal partial struct authKeyId {}
    internal partial struct authorityInfoAccess {}
    internal partial struct basicConstraints {}
    internal partial struct buildChains_type {}
    internal partial struct certificate {}
    internal partial struct certificateList {}
    internal partial struct certificateRequest {}
    internal partial struct chainConstraints {}
    internal partial struct constraints<T, V> {}
    internal partial struct distributionPoint {}
    internal partial struct dnsConstraints {}
    internal partial struct dsaAlgorithmParameters {}
    internal partial struct ecPrivateKey {}
    internal partial struct emailConstraints {}
    internal partial struct extKeyUsageOIDsᴛ1 {}
    internal partial struct ipConstraints {}
    internal partial struct lazyCert {}
    internal partial struct nameConstraintsSet<T, V> {}
    [GoLocalName("pkcs10Attribute")] internal partial struct parseCSRExtensions_pkcs10Attribute {}
    internal partial struct parsedURI {}
    internal partial struct pkcs1PrivateKey {}
    internal partial struct pkcs1PublicKey {}
    internal partial struct pkcs8 {}
    internal partial struct pkixPublicKey {}
    internal partial struct policyGraph {}
    internal partial struct policyGraphNode {}
    internal partial struct policyInformation {}
    internal partial struct potentialParent {}
    internal partial struct pssParameters {}
    internal partial struct rfc1423Algo {}
    internal partial struct rfc2821Mailbox {}
    internal partial struct signatureAlgorithmDetailsᴛ1 {}
    internal partial struct sum224 {}
    public partial struct CertPool {}
    public partial struct Certificate {}
    public partial struct CertificateInvalidError {}
    public partial struct CertificateRequest {}
    public partial struct ConstraintViolationError {}
    public partial struct ExtKeyUsage {}
    public partial struct HostnameError {}
    public partial struct InsecureAlgorithmError {}
    public partial struct InvalidReason {}
    public partial struct KeyUsage {}
    public partial struct OID {}
    public partial struct PEMCipher {}
    public partial struct PolicyMapping {}
    public partial struct PublicKeyAlgorithm {}
    public partial struct RevocationList {}
    public partial struct RevocationListEntry {}
    public partial struct SignatureAlgorithm {}
    public partial struct SystemRootsError {}
    public partial struct UnhandledCriticalExtension {}
    public partial struct UnknownAuthorityError {}
    public partial struct VerifyOptions {}
    public partial struct distributionPointName {}
    public partial struct pkcs1AdditionalRSAPrime {}
    public partial struct publicKeyInfo {}
    public partial struct tbsCertificate {}
    public partial struct tbsCertificateList {}
    public partial struct tbsCertificateRequest {}
    public partial struct validity {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcrypto() => builtin.initPackage(typeof(crypto_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸaes() => builtin.initPackage(typeof(go.crypto.aes_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸcipher() => builtin.initPackage(typeof(go.crypto.cipher_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸdes() => builtin.initPackage(typeof(go.crypto.des_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸdsa() => builtin.initPackage(typeof(go.crypto.dsa_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸecdh() => builtin.initPackage(typeof(go.crypto.ecdh_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸecdsa() => builtin.initPackage(typeof(go.crypto.ecdsa_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸed25519() => builtin.initPackage(typeof(go.crypto.ed25519_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸelliptic() => builtin.initPackage(typeof(go.crypto.elliptic_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸmd5() => builtin.initPackage(typeof(go.crypto.md5_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrsa() => builtin.initPackage(typeof(go.crypto.rsa_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha1() => builtin.initPackage(typeof(go.crypto.sha1_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha256() => builtin.initPackage(typeof(go.crypto.sha256_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha512() => builtin.initPackage(typeof(go.crypto.sha512_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸx509ꓸinternalꓸmacos() => builtin.initPackage(typeof(go.crypto.x509.@internal.macos_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸx509ꓸpkix() => builtin.initPackage(typeof(go.crypto.x509.pkix_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸasn1() => builtin.initPackage(typeof(encoding.asn1_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸpem() => builtin.initPackage(typeof(encoding.pem_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(go.@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸiter() => builtin.initPackage(typeof(iter_package));
    [GoInit] internal static void initᴛᴛimportꓸmaps() => builtin.initPackage(typeof(maps_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(go.math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(go.math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸnetip() => builtin.initPackage(typeof(go.net.netip_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸurl() => builtin.initPackage(typeof(go.net.url_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸcryptoꓸcryptobyte() => builtin.initPackage(typeof(vendor.golang.org.x.crypto.cryptobyte_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(x509_package.CertPool)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class x509ᴛCertPool;
[GoMethodHost(typeof(x509_package.Certificate)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class x509ᴛCertificate;
[GoMethodHost(typeof(x509_package.CertificateInvalidError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class x509ᴛCertificateInvalidError;
[GoMethodHost(typeof(x509_package.CertificateRequest)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class x509ᴛCertificateRequest;
[GoMethodHost(typeof(x509_package.ConstraintViolationError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class x509ᴛConstraintViolationError;
[GoMethodHost(typeof(x509_package.ExtKeyUsage)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class x509ᴛExtKeyUsage;
[GoMethodHost(typeof(x509_package.HostnameError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class x509ᴛHostnameError;
[GoMethodHost(typeof(x509_package.InsecureAlgorithmError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class x509ᴛInsecureAlgorithmError;
[GoMethodHost(typeof(x509_package.KeyUsage)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class x509ᴛKeyUsage;
[GoMethodHost(typeof(x509_package.OID)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class x509ᴛOID;
[GoMethodHost(typeof(x509_package.PublicKeyAlgorithm)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class x509ᴛPublicKeyAlgorithm;
[GoMethodHost(typeof(x509_package.RevocationList)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class x509ᴛRevocationList;
[GoMethodHost(typeof(x509_package.SignatureAlgorithm)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class x509ᴛSignatureAlgorithm;
[GoMethodHost(typeof(x509_package.SystemRootsError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class x509ᴛSystemRootsError;
[GoMethodHost(typeof(x509_package.UnhandledCriticalExtension)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class x509ᴛUnhandledCriticalExtension;
[GoMethodHost(typeof(x509_package.UnknownAuthorityError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class x509ᴛUnknownAuthorityError;
[GoMethodHost(typeof(x509_package.chainConstraints)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class x509ᴛchainConstraints;
[GoMethodHost(typeof(x509_package.dnsConstraints)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class x509ᴛdnsConstraints;
[GoMethodHost(typeof(x509_package.emailConstraints)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class x509ᴛemailConstraints;
[GoMethodHost(typeof(x509_package.ipConstraints)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class x509ᴛipConstraints;
[GoMethodHost(typeof(x509_package.nameConstraintsSet<,>)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class x509ᴛnameConstraintsSet;
[GoMethodHost(typeof(x509_package.parsedURI)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class x509ᴛparsedURI;
[GoMethodHost(typeof(x509_package.policyGraph)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class x509ᴛpolicyGraph;
[GoMethodHost(typeof(x509_package.rfc1423Algo)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class x509ᴛrfc1423Algo;
[GoMethodHost(typeof(x509_package.rfc2821Mailbox)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class x509ᴛrfc2821Mailbox;
// </MethodHolders>
