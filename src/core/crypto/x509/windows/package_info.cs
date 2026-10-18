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
global using ecdhꓸCurve = go.crypto.ecdh_package.ΔCurve;
global using ecdhꓸPublicKey = go.crypto.ecdh_package.ΔPublicKey;
global using netipꓸAddr = go.net.netip_package.ΔAddr;
global using netipꓸPrefix = go.net.netip_package.ΔPrefix;
global using netꓸAddr = go.net_package.ΔAddr;
global using netꓸError = go.net_package.ΔError;
global using runtimeꓸError = go.runtime_package.ΔError;
global using syscallꓸHandle = go.syscall_package.ΔHandle;
global using syscallꓸSignal = go.syscall_package.ΔSignal;
global using syscallꓸSockaddr = go.syscall_package.ΔSockaddr;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
global using urlꓸError = go.net.url_package.ΔError;
using syscall = go.syscall_package;
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
[assembly: GoTypeAlias("OID", "ΔOID")]
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
[assembly: GoImplicitConv<VerifyOptions, ж<VerifyOptions>>(Indirect = true)]
[assembly: GoImplicitConv<syscall.CertChainContext, ж<syscall.CertChainContext>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("crypto/x509/cert_pool.go", "cert_pool.cs", "ADuAAZIABRDCgpSokoKoktyCgoKUopSCAAIcAAsCgIK2AAcU0oIAAhIACAKCgoKUgpS2tMiCgpSCgoKCprKClKiygpQAAhTygriCpoLKAAcSAAgCgoKCgpSCppKCgpSIgpSClJSmAAIQ8oKClKjCgpSClKKCpgACEPKClA==", "184-186:1;239-246:1;240-244:1.1;291-293:1")]
[assembly: go.GoPositionMap("crypto/x509/constraints.go", "constraints.cs", "AEeUAdKCppSClIKCgoKmAAIQAAkCgqaUgqaCpoKClJSClKaigpSCgpSmsoKClKaipoIAEByGgpSSgoKUppKCppSCppSmgoKClJSClAACHAALAoKCgpSCgoKCgpSClIKUpoKm7riCgpSUAAwaAAIWAAgCgpSCgoKCgoKUgoKUpoKCkoKUABUqgoKUgoKmlO6UgoKigoKClJSCuKaCgqaCgqaCgoKAgrYAEh6GgpSCgoKCgpSCypSClKaClKaCgoCCtoKUggAQEqKCgIK2goCCtgALGoKCgIK2goKUgIK2goKUgIK2goKUgIK2poKCgrKClO6CgpKCgqaCpoKClIKCgpS4goKUgoKmgoCC2gAHEIKmgoKCgoKUgoKCgu6AgraUpoKCgoKClIKUpoKClJQ=", "", "", "568=newIPNetConstraints/1/2/1,newIPNetConstraints/2/2/1,newDNSConstraints/1/4/2,newDNSConstraints/2/4/2,newDNSConstraints/3/4/3,newDNSConstraints/4/4/3,newEmailConstraints/1/2/4,newEmailConstraints/2/2/4")]
[assembly: go.GoPositionMap("crypto/x509/oid.go", "oid.cs", "ABImAAYSkoK2goKmgriClIK4qJKCpoKCpoKCgpSmgoKUpoKCgoKClJSmooKUpoKCpoKCgoKUlKiSqJKokqa4goKCuKqCgoKUlKqAgqSAgraCpoKUlIKCgoKClKaCqJKokqiSgoKUgqi2psKCgqaCgpSCpoKClIKCgpSClKaCrLKClIKmlIKSgqaSgriCgoKmlIK4qJKCgqrOgoKCgoKmgoKUgpSCgoKCgpSCgoKCgpSUgoKCgoKCgpSCppSCpqaClLyUgoKmgpSCgoKClIKUgpSCuNiSgoKClJQ=")]
[assembly: go.GoPositionMap("crypto/x509/parser.go", "parser.cs", "AChGogASMMIAChaClJSkgoKmpIKUAAkUgriAgraCgriGlIKmpIKClKSCgqaUAAgKwoKmgoKCgoKUgoKClIKClIKCgpSCgoKUpqbmgpKClIKUgoKClIKC9qKClILGgsaUpqKCgpSCgqb2goKClIKCpoKClIIAEwaCgoKCuIKmkoKClIKUgqaClIKmuKSCgoKUgoKU2IKUgpTIgpS0goKClAAHEJKIlISUpOiCgoKmgoKCptaigoKUgoK4goKCgpSm5qKClIKCgoKUgILI9uKClIKAgqSkgoCCpKSCgIKkgoKUgpSklKS4puaUlJSCgoKUgoKUlNaigoKClIKCgpSAgpS21qKCgoKUgoKCgpSClIKCgpSUqJKUgoKCpqaU5rjmABAggqKSiqbKpsKCkoKEps6UgoCCtoKUpoKklIK2gramgqamgoCC2oKAgraCpqaCgIK2gqaClKa4poCCpICCpJQADwaigoKUgpSCgsaCgpSCtIKCppTIgoIADyCSgpSCgoKUgoKClIKUgpSCgpSCgpTagoLGkoKUgoKClJSClJSCgoKUlIKUxoKCxpSUgoKClLSCgpSCgoCC6JKClIKCkoaUxoKClMa0lJSUkoKUgoKClIKClIKUgpSUtNqmgrimABYEgpTIgpSCgqamgpSCgqaClIK4goKmgoKUgoKUppSCgpqygpSClIKClJSCgpSCgoKUlIKClIKCpoKClIKCgpSUgoKUgoKUgoKUkoKUgoKClILIgriCgpSClIKCgoKUgoKClIKCgpSCgpSCgpSClIKC3IKClJQABRDSgoKUgpSqooKCgoKUgpQADBKilMiClIKCpqaClIKCpoKClIKUgqaCgpqygpSClIKClJSCgpSUgoKUgoKClJSCgpSCgoK4goKClIKkgoKUgoKmgoKUgoKUgoKClIKCgoKUgoKUgoKCpriCyriCgoKUgoKUgoKClIKClIKCgqSCgoKCpriqAAwYgsqCAAgSgoKmgpSUgoKClILcuA==", "401-438:1;568-659:1", "", "395=ReadASN1Integer/1/3/1,ReadASN1Integer/2/3/2,ReadASN1Integer/3/3/3;398=Sign/3/4/1,Sign/4/4/1;681=Empty/1/2/1,ContextSpecific/1/2/2,Constructed/1/2/2,ReadOptionalASN1/1/2/2,ContextSpecific/2/2/3,Constructed/2/2/3,ReadOptionalASN1/2/2/3,Empty/2/2/4;702=ReadAnyASN1/1/1/1;1001=ReadASN1/2/3/1,ReadASN1/3/3/2")]
[assembly: go.GoPositionMap("crypto/x509/pem_decrypt.go", "pem_decrypt.cs", "ACxcACFIsoKClIKCgoKCgpQABRIACAKCuAAJHAAOAoKCpoKCpoKClIKClILKgoKCpoKmgoIABxCCgpSCgpSClIKCpgACFPKCgpSCgILIgoKClIKCuJSClJQACRSCgoKCpqaCgoKCpg==", "", "", "241=EncodeToString/1/1/4")]
[assembly: go.GoPositionMap("crypto/x509/pkcs1.go", "pkcs1.cs", "AC5cAAgQ4oKCgpSCgIKkgIKkpoKmiKaCyoKCgoKCgoKCgpTKgoCmgoKCgoKAgoLItgACGAALApSCgqYACxiCgoKCpoL8soKCgoCCpJSCpoKUgqYABRLSuA==", "", "", "86=Sign/5/7/1,Sign/6/7/2,Sign/7/7/3")]
[assembly: go.GoPositionMap("crypto/x509/pkcs8.go", "pkcs8.cs", "ACJMAA8CgoCCgIKkgIKkpJSCgpSmgoKAgqSCgpSmgIKkgoCCpICCpKaAgqSCgIKktgAGGgAJApSUuIKAgqS2goKUgoKU3ICC2KaCgpS2gqaCgIK2goKUgoKU3ICC+qY=")]
[assembly: go.GoPositionMap("crypto/x509/root.go", "root.cs", "AB4+ooKCgtaigpSCgoKmgqaUgqaCpqKkAAEeABECgqaClIKU3IKCgqaApIKUgqaCpqI=", "103-103:1")]
[assembly: go.GoPositionMap("crypto/x509/root_windows.go", "root_windows.cs", "AA4eggACFAALApSCgpSUgoKUlIKCpoKCgoKUgoKmgoKCygAICLKCpoKCgpSCgoKCgpSmqqKCgpSktqaqooKClLiUppSSgoLcgpSkpKS42pSCgujSgoKmgoKCuIKClIIABxCCgpSAlLaqAAgCgoKUlIKUgoKUgoKCgpSAgraCgoKUgoKmgoKSrMaSgoKUlIKCpoCCgoKCgtqUpg==")]
[assembly: go.GoPositionMap("crypto/x509/sec1.go", "sec1.cs", "ACJIsgAFEPKCgqaqooKClIKClAAHFMIACRbigoCCgIKkgIKkpIKmgoKUlIKmgriCgpTclA==", "", "", "85=Bytes/1/2/2,PublicKey/1/1/3,Bytes/2/2/3")]
[assembly: go.GoPositionMap("crypto/x509/verify.go", "verify.cs", "AFKQAYKUpKSkpKSkpKSkgoKUlAAMFIKClIKmgoCUgpSClIKClKaClLaClAAQHIKCgoKCgpSmlAAKEIKCgpSmgMgAOIYBgq7igqaUAAoWgoKCgpSClJTIgpSIgpQACyS26oKUlO6CgpQAAhSCptiC7oKGuIKUyoCCtoKCqsKCgoCCgpSCgqbalKaClKaClMra0oKmgoKCuIKClILagu6CggAUKoKmgoKCuAACRgAlBoKUgoKClILKpoKClIK4gsqCgoK4goKmgoKCuIKClIKCuIKClIK4gqaCgoKCgoK4goKUgIKClKSmgoKUgoKClJSClIKmpoKCgoIABhDSirKCgriCgsqClIKygoKmgpKClIKmAAgS5MKqkoKUgoKCpoKmgIKCgpS2goKCgpSmgoCCgoKUyJSkgoK4gr6CgoLqgpSCpqaAooCqsoKUgpSmpoKUlLiUgoKUgpSClIKUppS4poKClKaClIKmlIKmgoKUgrisxIKCpoKUgoK4gqaCgoKmAAIYAAwEgoKUgKaCgqa2goKU3IKCpoLKpoKmgoKUgqbcgoKClKaClLiUgoKCpoKCuIKCggAICqaCgoKUAAoYgtyCgpQAChSUguzcoqaCgpSmgoKUpoKClKaCpoKmgoKClIKUgpSmgoKCgoKmgoKC3KaCgoKCgpTcgoKCgriCpgAJFoK4lIKigpSClIKmgoK4gqaClKaCuIKmgpSmgpSCuIKUgIK2ggAIEoKCgoKCgsqCypSUlJSCgpSUpsqUlICCkoCUgoIACA6UgoKUgpSCyoKUgqaC3IK4gqikgpSAgtqCgqaUlIKCuIKCyoKm", "622-640:1;731-780:1", "", "391=Contains/1/1/2;485=Format/1/2/3,Format/2/2/3,Sprintf/1/1/3;492=Format/1/2/3,Format/2/2/3,Sprintf/1/1/3")]
[assembly: go.GoPositionMap("crypto/x509/x509.go", "x509.cs", "AEiQAQAIAoKAgoCCpJKCpKbClLiClKa0goKUgoKUgoKCgpS0grSCgpSCgpSCgoKClNamAAIYAAkCgoKUgIK2AAcQggA/hgGCgoKmpoKCgqamgoKCpgAKGu6CgpQAMmiCgoKCgoKCgoKCgoKUgoKUygALBgASNgAIFIKCAA0egqaCuIKCgqbcgoCCtoKAggAIEIqmlKSkpgALGoLKyoKcspSkpKSUABIogoKCloKUpKSklKaClKSkpKamgpSkpKSmAB9IgoKCgoKCgoKCgoKCggAZMgAQKLKCgqamsoKCpqiSgoKUgoKUAK0BhAPcggALEIKmsoKUpoKuAAsMhKaCpoKmAAIU8qaCpoKCgqamgurCgpSCgoKCuKSCtraClKSClIKCppSClIKUxoKUgpSkgpSClJSs0oIACAqCACROgoKCgqyylIKUgoKUuKiCgoKCgoKCgoKCgpiCmqKCgqaqooKCgIKklIKAgqSUlIKClJSCgoCCpJSmgpSCuKaUspKUhIKClKaEgoKUpoKCgpSmgoKCgpSmgoKCgpSmhIKCgsqCyoKClKaEuIKCgpSmgoSCgpSmioKUgoKCgoKmopSCgIK2goLKgqKiyoKAgraCgsqCgIK2goLKpoKCpoKCpoKigpK4gpLKgoKUpoSUgoLupoKClO6mgpSCgpSCgqaCgoLWgpSCgoCClMiUgoKmgriClIKC1oKUgqKCgrKSkoKClMqCktyCgqaClISCgqbcpoKCpgAIDLKCgpSkgraClKSkpMiCxqaCpoKCgpSCpu6mgqSCgtyCgriAgrbKAAmCAQBCAoKCpoK4goCC7IIABxCCpoKmgqaCgqaCgpSCpoKCpoKCpoKCpoKCgsqC3IKqhoKSgraCgqaCAAoWgoKUlIKCpu64AAQW4oKCgqbcsoKAgpKCpAACEPKCgqaCgriCooKmAAkUgoKCgoKUpoKClJSCgqYAPo4ByKKCgoKUgoKUgpSokoKCgqaCpgALCqSMgoKCgJS2gqaCgIKkgoKClJSmAAcsABYCgoKmgoKmgoKCgqaCgriCgoKC3IKUgoLKlIKCuIKUgqamAAcQgoK4goLKggAEEIKCpoKAgramgoKCgrgADBqCgpSUgoKmAAYSopSCgpKCptaCAAsegoKCgriCgIKSgraUgIK2gpSCgtqokgB+lAIAEAKClIKUgpSClIKUgqaCgqamlIKigsqCgoKUgqbugoKClMqCgoKm3IKUuIKCpoCCpIKCuIKCpgAQIoKmgqaCgsqUgoKmAAYSwoSmgqaCpg==", "1302-1308:1;1310-1358:2;1318-1322:2.1;1319-1321:2.1.1;1326-1330:2.2;1327-1329:2.2.1;1338-1342:2.3;1339-1341:2.3.1;1350-1354:2.4;1351-1353:2.4.1;1371-1383:3;1373-1375:3.1;1379-1381:3.2;1476-1497:1;1480-1488:1.1;1481-1487:1.1.1;1492-1494:1.2", "", "478=Equal/2/4/1,Equal/3/4/2,Equal/4/4/3;1246=oidInExtensions/1/1/1;1254=oidInExtensions/1/1/1;1288=oidInExtensions/1/1/1;1310=oidInExtensions/1/1/1;1324=oidInExtensions/1/1/1;1332=oidInExtensions/1/1/4;1442=oidInExtensions/1/1/1;1572=oidInExtensions/1/1/1;1875=UTC/1/2/5,UTC/2/2/5;1975=ToRDNSequence/1/1/3,UTC/1/2/4,UTC/2/2/5;2343=RightAlign/1/1/6,getSignatureAlgorithmFromAI/1/1/7,getPublicKeyAlgorithmFromOID/1/1/9,parseRawAttributes/1/1/12;2589=UTC/1/1/2;2646=UTC/1/2/4,UTC/2/2/5")]
[assembly: go.GoPositionMap("crypto/x509/x509_string.go", "x509_string.cs", "/oaigoKCgoKCgoIACx6WgpSCpKSkpKSkpKS2hqKCgoKCgoKCgoKCgoKCypSCgoKU")]
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
    public partial struct ΔOID {}
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
    [GoInit] internal static void initᴛᴛimportꓸsyscall() => builtin.initPackage(typeof(syscall_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸcryptoꓸcryptobyte() => builtin.initPackage(typeof(vendor.golang.org.x.crypto.cryptobyte_package));
    // </ImportInitializers>
}
