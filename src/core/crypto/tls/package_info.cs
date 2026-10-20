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
global using ecdhꓸPublicKey = go.crypto.ecdh_package.ΔPublicKey;
global using hpkeꓸKEM = go.crypto.hpke_package.ΔKEM;
global using hpkeꓸPublicKey = go.crypto.hpke_package.ΔPublicKey;
global using netꓸAddr = go.net_package.ΔAddr;
global using netꓸError = go.net_package.ΔError;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using runtimeꓸError = go.runtime_package.ΔError;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
global using tls13ꓸExporterMasterSecret = go.crypto.@internal.fips140.tls13_package.ΔExporterMasterSecret;
global using tls13ꓸHandshakeSecret = go.crypto.@internal.fips140.tls13_package.ΔHandshakeSecret;
global using tls13ꓸMasterSecret = go.crypto.@internal.fips140.tls13_package.ΔMasterSecret;
global using x509ꓸOID = go.crypto.x509_package.ΔOID;
// </ImportedTypeAliases>

using go;
using static go.crypto.tls_package;

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
[assembly: GoDynamicTypeLift("63727970746f2f746c732e62696e6172794d61727368616c6572", "cloneHash_binaryMarshaler")]
[assembly: GoDynamicTypeLift("696e746572666163657b4d61727368616c42696e6172792829202864617461205b5d627974652c20657272206572726f72293b20556e6d61727368616c42696e6172792864617461205b5d6279746529206572726f727d", "cloneHash_binaryMarshaler")]
[assembly: GoDynamicTypeLift("7374727563747b736368656d652063727970746f2f746c732e5369676e6174757265536368656d653b206d696e4d6f64756c7573427974657320696e747d", "rsaSignatureSchemesᴛ1")]
[assembly: GoTypeAlias("ConnectionState", "ΔConnectionState")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<CertificateVerificationError, error>(Pointer = true)]
[assembly: GoImplement<Conn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<ECHRejectionError, error>(Pointer = true)]
[assembly: GoImplement<RecordHeaderError, error>]
[assembly: GoImplement<alert, error>]
[assembly: GoImplement<atLeastReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<certificateMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<certificateMsgTLS13, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<certificateRequestMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<certificateRequestMsgTLS13, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<certificateStatusMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<certificateVerifyMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<clientHelloMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<clientKeyExchangeMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<cthWrapper, hash_package.Hash>(Pointer = true)]
[assembly: GoImplement<ecdhKeyExchange, keyExchange>(Pointer = true)]
[assembly: GoImplement<ecdheKeyAgreement, keyAgreement>(Pointer = true)]
[assembly: GoImplement<echConfigErr, error>(Pointer = true)]
[assembly: GoImplement<encryptedExtensionsMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<endOfEarlyDataMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<finishedMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<go.crypto.@internal.fips140.aes.gcm_package.GCMWithCounterNonce, go.crypto.cipher_package.AEAD>(Pointer = true)]
[assembly: GoImplement<go.crypto.@internal.fips140.aes.gcm_package.GCMWithXORCounterNonce, go.crypto.cipher_package.AEAD>(Pointer = true)]
[assembly: GoImplement<go.crypto.rsa_package.PSSOptions, crypto_package.SignerOpts>(Pointer = true)]
[assembly: GoImplement<hash_package.Hash, transcriptHash>]
[assembly: GoImplement<helloRequestMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<hybridKeyExchange, keyExchange>(Pointer = true)]
[assembly: GoImplement<keyUpdateMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<listener, net_package.Listener>(Pointer = true)]
[assembly: GoImplement<lruSessionCache, ClientSessionCache>(Pointer = true)]
[assembly: GoImplement<marshalingFunction, go.vendor.golang.org.x.crypto.cryptobyte_package.MarshalingValue>]
[assembly: GoImplement<net_package.Conn, io_package.Reader>]
[assembly: GoImplement<net_package.ΔError, error>]
[assembly: GoImplement<newSessionTicketMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<newSessionTicketMsgTLS13, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<permanentError, error>(Pointer = true)]
[assembly: GoImplement<prefixNonceAEAD, aead>(Pointer = true)]
[assembly: GoImplement<rsaKeyAgreement, keyAgreement>]
[assembly: GoImplement<serverHelloDoneMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<serverHelloMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<serverKeyExchangeMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<xorNonceAEAD, aead>(Pointer = true)]
[assembly: GoImplement<ΔfinishedHash, transcriptHash>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<AlertError, alert>(Inverted = true, ValueType = "uint8")]
[assembly: GoImplicitConv<alert, AlertError>(Inverted = true, ValueType = "uint8")]
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: go.GoHoistedLiterals]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("crypto/tls/alert.go", "alert.cs", "AA8eggArYAAkSoKCgpSmgg==")]
[assembly: go.GoPositionMap("crypto/tls/auth.go", "auth.cs", "ABoskoKCgpSUgoKUgsaCgpSCxoKClICC1oKClIKAgtaUqqKUgoKUgsaCgpSAgtaUAAUQAAoaopKCgoKCqtKUpKSkpJSUpKSkpKSUrLKUpOyk+AANIoKUpu6UpKSkxoKCgoKmpLT+0oKClIKCvIaClMqClLiCgqaqoqS2poKCuKT6xuamgqY=", "239-241:1;245-247:2", "", "215=Size/1/7/6,Size/2/7/7,Size/3/7/8,Size/4/7/11,Size/5/7/12,Size/6/7/13,Size/7/7/14")]
[assembly: go.GoPositionMap("crypto/tls/cache.go", "cache.cs", "ABQmgoCCgILYgoKmkoCCoKKAgpSCoNam", "33-33:1;38-38:2")]
[assembly: go.GoPositionMap("crypto/tls/cipher_suites.go", "cipher_suites.cs", "ACxYgoIAARTyABAu9gAPJrKCgqaCgqYAI0wAGDaigoKCpoKCuAAVLABFjAEAHj4AFzAADRwACxjegoKmlgAKHLKClIKAgqSAgramgoKmgoKClKaCgoKUqJKmgpSqogAWMoCigKKApIKCpoKCAAkUgKKAooCkgoKUgoKmpoKClIKCpqaCgpSCgpSCgpSCpIKmgoIAAhoACgKClIKClIKClIKkgqaCgqaCgpSCgqaCggAOHICigKKAooCigKSCgqySgoKCgoKClKaCpoLcggAFEKKCgqamgoKCpqaCgoKmpoKCgqY=", "608-610:1")]
[assembly: go.GoPositionMap("crypto/tls/common.go", "common.cs", "ADNespSkpKSkpABTxAGClKS4gpSkAC1wAFXkAQAKAgAcQqKUpABm8AGyABQ2sgCRA/IG4oiygoKCAAYgAA4CgpSCggAqUJgACgaEuIKCgoKCgILskoK2AAIWABMGgoKCgpSCgoKClKaCgoKUgoKmgriCgoKUgoKAgqSCgrSCppQABSAADwKCpoKipoKCpoKCgpSmgoKClKaigoKUgpiCmKqipgAMGJjCgoKClIKCpoKUgpSClJSmgoKClKyygoKClJSmooKCmIKYgpSmgqqigoKCpgALGJjChIKCuIKmlKaCgoCCpIKCgoKAgtqygILaAAkcABIMgoKUgoLKgoKClICCAAgQpILKgIKAgrbIgoKUgpSUgpTKgoCC/oK4goKUgqaCgIKkgpSkpKSUgoKCgqaClLSClPam7IKClIKCpoKmgpSUgqbc0oCCtoKmgqaCgoCCyIKCuAACFPKCgoKCgriClIIAChqCgqaUgoKUACNKooKUAB5GspSClAAGEvKClICCgoKUgoKUtoKCgqaCgoKCgoKiqPKClICCgqQACAqCpoIABBCygoKUqpSCggAIEoLKgoKCpIKCgriqooKqggAJFIKmggAFEuKCpoKCgriCpqaCgqaCgriuwoKClIYACBKCgoKUgoCCtoKAgsg=", "1189-1191:1;1194-1196:2;1273-1275:1;1278-1280:2;1402-1432:1;1419-1427:1.1;1500-1517:2;1757-1759:1;1800-1802:1;1872-1874:1", "", "985=HasPrefix/1/1/1;1690=New/1/1/2")]
[assembly: go.GoPositionMap("crypto/tls/common_string.go", "common_string.cs", "/oaigoKCgoKCgoKCgoIADCCWgpSkpKSkpKSkpIKktoaigoKCgoKCAAYUgpaClIKkpIKktoaigoKCgsqUgoKU")]
[assembly: go.GoPositionMap("crypto/tls/conn.go", "conn.cs", "AHOGAtKokqyyqqKssqyyABY2gKKAooCigKSCgIKUpKqigoKqooKUgoKCgoKs0oKCgoKokoKCgu6ssoKmlKS2gpS0zuKCpoKUppSCpoKUgoLKgoKCAAoWlIKmggAJFqKCgriCpoKUlIKUtIKUgoKUlIKClIKCgqaCgoLGgoKCpoKClAAHEMS2goKUgqaCgoKClILKpoKCgqaCgoKCggAIEoKCpqaCrOKAgpSCpIKqooKmgoCCgoAACRSUgIL6gpSCgoK0goKmgqaClIKClKaCgsaCgoKCgoKCgpSClMTIgoKClAAQIqCkooKCgqaCpoIACCIAEAKClKaClJSCuIC4gpSAgqSkgtyCgqaCgqaUgoKCgpTKgqaCgoKUgIKAgqTIgoKClIK4gqaUuIKmmoKUgpSClNyUlJSmpKS4gqaC7oKUgpSAgsiCuILKpoKUAAKVAQACnAHawoKCgpQAChaCgpSCgoKUgpSqooKUuIKCqLKCppSklJSClKaowoKCABFCABACgqaCuIKClLS0pqbEtoK4goKCpoKClKaCgoKmgoKmgoKmgoKCgrgABxDigoKUgoKAgramgpLsgqaCgoKAgraCgoKmkqaUgoKClIKCgpSAgqSCpoKAgsjc4oKUgoKUgqba0oKCgtiygpSCgIK2rNKAgqSUuLimgoKClICCpIKmooKUtLS0gpTGgpTGgpTqtLS0tNi0tLS03JSCpoKmqAABEgALBIKCgpSCppSAgraClICCtoKmggALGIKCgIKCgpTIggAJCOKCpoKCpoKCgqaCppSkguqCpoKUgoCCpAAICsKCpoKClIKCgqaUpNyC1uKCgqaCgqaCgpSCgoKUgpSCpoKmgoCCtgAFEgALAoCCpKamgpSCgIKkgoCC2gAIEoSAgsjYtJKCgoKUgqbupoKCgILIgIKkppqygqam0oKUlIKClJQABSAADQIAAhoADAbWAAgIgqa4pIKCkpSClJKUyoKUgIKkgqaClIKCuKaClIKmgoK4gIK2goKClMqUgqbo4oKC1pSigoKCgoKCgoKCgoKCgoKCgoKUpoKSgoKCgpSmlIKq8oKUAAoMAAgCgoKClIKUgpQACAwACAiCgpSUlIKssg==", "996-1004:1;1538-1540:1;1541-1546:2;1642-1648:1", "", "1628=Bytes/1/1/1")]
[assembly: go.GoPositionMap("crypto/tls/defaults.go", "defaults.cs", "AA4gkpiiprakAAYWwgAPIJKUgoKUuIKCABAqAA8g", "71-75:1")]
[assembly: go.GoPositionMap("crypto/tls/defaults_fips140.go", "defaults_fips140.cs", "ABo6uAAHEAALGAAHEMyClKSktA==")]
[assembly: go.GoPositionMap("crypto/tls/ech.go", "ech.cs", "ACdWAAgMgoKUpqKSgoKUgpSClIKCgpSClIKUgpSCgpSCgoKUgpSUgpSCgpSCgoKUgoKClIKUpqyygoKClIKUgoKClIKCgpSCgqamgrKClIK4gqaClIKClIKmlLiCgpSCgpSmpqKCgpSUgoKUlJSmooKClKaigoKUAAsQgpK4lIKCgqaCgoKElJQACAYACBKSsoKM3IKCuIKCpoKCkoKClIKUgpSSgoKCgoSClIKCgpSCgoSCnMKCgoKClISClIKCgpSCpoKSlKaCgu6CgpSCgqaCpoLcgriCkpS4gqamooKmgoKCgoKCkJKQkqaigoKUgoLKgoKClIKClIKCgpSCgpSuwoKUgoKUgoKClIKClIK4AA8agqaSAAYS8oKCkoKCgpSCgoKClJSCgpSCgpSCgpSCgpSCgpSCgsqmsqKCgpSmpoKmgqaCgoKClIKUgoKClIKCgpSCgoKUgoKClIKClKaClAAIEoKCgqaU7qaCgoKSgoKUgqaClA==", "299-371:1;301-303:1.1;304-306:1.2;307-309:1.3;310-370:1.4;358-360:1.4.1;365-367:1.4.2;424-424:1;425-425:2;651-659:1", "", "255=skipUint8LengthPrefixed/1/2/1,skipUint16LengthPrefixed/1/1/2,skipUint8LengthPrefixed/2/2/3;270=ReadUint16LengthPrefixed/1/1/1;300=readUint8LengthPrefixed/1/2/1,readUint16LengthPrefixed/1/1/3,readUint8LengthPrefixed/2/2/4,ReadUint16LengthPrefixed/1/1/5;338=ReadUint16LengthPrefixed/1/1/1;349=Empty/2/2/1")]
[assembly: go.GoPositionMap("crypto/tls/handshake_client.go", "handshake_client.cs", "ADpWooKCpoKCgIKUtoKmgoK4kpQAESSCpoKmlIKqgoIABxCCgoCCyIKCpoKUgqaCkoKUpoLKgoKClIKCpoK4goKClIKUpoKCgpSClIKClIKClIK4goKClIKCgrgAER7igsqClIKCpoKClILugoCC7JS4lIKCgu6AgsiUgIK2goKCgoKUgIKkgriCgqaCgoKmgILsgoKChIKmggAKFqbuAAgKlPSCpriUptyCuIKClIKClKaCgoKCpoKmlIKUgpSUgKakypSCuKaCuIKClKaCuIKCyoKClIKCgoKCpoKmgoLKgoKCgu6CuIKmgoKCgIK2prKCgqaCgoKmgoKClNrCAAkUgoKmgoKm3IKmgIKkgIK2goKCgIKkgIKkgIKkuIKAgoK2gIKkgIK2gIKkgIKkgIKkgIKkgoCCpICCtoCCtoKU1oKAgoK2goKUgoKmggAIBqKUgoKUgoKCpoKCpoKouoKmlIKCuKaAggAJEoKCuJSCgoKCgpSAgoK2goK4goKCgpSCgIKCtoKCuIKCgtyCgoKAgsiCgoKUgoCCyIKCpoKCgpSUpoCCgraClIKCgqaCgoKClJKClIKCgoKUgoKUgoKUgoKCpoKCgpSCgoKCuICCyJSmgpSEkpKCgoKClIKmgoKmpgALCIKUgIK2goKmgoKCgpSmgqaCgoKCuIKCgoKCgriAgoKklJSCpoKCpoKCuIKCuIKCgoKmgpSU+qKClJSUgpSCgqbWopSAguyCgpSCgoKmgoSCpoCCtoLWooKUlIKCpoKClIKCgqaCpoKClJSCgqaCgpSCgqailICCtoKCgIKkgu6UgoCCgIKClMaqwoKCgoKClIKCgIKCtqaCgoKAgoK27oKUgoKCpoKCgraC7oKUgoKCpoKCgri0tIKmlIKAgoLIgoCCgsiqotySgpSkuNyU6tjIyoKCgoKUlILGgtqmooKmsoCCpLiqooKUgpSssoKClICCpIKUgpSmooKClIKC", "99-101:1;241-253:1", "501=crypto.Hash.New", "91=hostnameInSNI/1/1/7,curvePreferences/1/1/8;471=time/1/1/1;889=Sum/1/1/1;977=keysFromMasterSecret/1/1/1;1000=Equal/1/1/1;1157=ConstantTimeCompare/1/1/1;1300=time/1/1/2,NewCertPool/1/1/4;1324=time/1/1/2,NewCertPool/1/1/4")]
[assembly: go.GoPositionMap("crypto/tls/handshake_client_tls13.go", "handshake_client_tls13.cs", "ADJcwriCgriCpoCCtpSAgraCgoCCyIKAgqSAgsiCgoKCgpKCgoKUgoKCgoKUgoKmgoKmuICCtoKAgqSAgqSAgqSAgqSAgqSAgqSAgqSAgqSAgraCgqaUAAsKopSCgqaCgqaCgqaOgqaCgqaCgqaCgoKUgoKUgpSqooKUgpSUAAsKosqCgoKCgIK2goKCgoKClIKCgqaCgoKCgpKCgoKUgoKCgoK4gIK0lILcgoKmgqaCgtyAgoKClIaClIKCgpSCgoKmtoKCgpSUgpSCgoKAgraAgsiCuIKCpsqCgoCCtoCCtqaAgsiCgqaCgoKUlICCtoIACgailIKCpoKCpoKCpoKClIaCpoKmgoKmgpSCgpSCgqaCgoKCgoLWgpSCgoKUgoKClJSCgraUgoKCgIK2goKAgsiCgoKUgoKCppQACgaClIKCpoKCgqaAyoKklIKUgpSUgoK4goKUgpSCgoKUgoKmgoKSgoK45oK4uIKAgoK2poKCpoKClIKCuIKCgpSCgqaClICC7IKCpoKCgtyEgpSCgpSClIKAlIKklICCttaCyoKCpoKCgqaCgoKmgILagoKAgraCgoKUgoKCppSmgpSCpoKAgqSm3IKmlIKClICCyIKmgpSCpoKmkoKmgoKClIKCgpSUgIK2poKUuICCtpSCpoKmAAkGsoKCpoK4gpSCgoKmgoK4goKmgoKmpoKCgoKCgoKClIKAgrY=", "316-318:1;433-435:1", "92=crypto.Hash.New;267=crypto.Hash.New;487=crypto.Hash.New;868=crypto.Hash.New", "761=signatureSchemesForPublicKey/1/1/1,isSupportedSignatureAlgorithm/2/2/1;933=finishedHash/1/1/1;1002=Size/1/1/1")]
[assembly: go.GoPositionMap("crypto/tls/handshake_messages.go", "handshake_messages.cs", "ABMqgqqikoKUgrqygqrCkoKUgqrCqsKqwgAjSLKClIKCgoKCypSCgoK4lIKCppSCgoK4lIKUlIKUlIKUlIKCpoKCgq7ClIKUgoKCgriUgpSCgoKC3JSClIKCgoLclIKUgoKCgtyUgpSCgoKCgu6UgpSCgoKC3JSClIKCgsqUgpSCgoKygpLulIKUgoKCyoKCkpKC3JSCgoKykpSmgoKS3IKCpoKCkoKCgoKmgoKmgqaCksqmgqzSgoKCpoKClIKCgqbaooKUgoKmlKaigqSWpoKClIKCgoKClIKUpoKmlKaCgqaCgoKChKaClIKUlpKClIKCgoaUgpSUlJSC2pKChpS2koKUgoKClNiE2IK2koKUgoKClNqSgpSCgoKU6oKUxraSgpSCgoKU2LaSgpSCgoKU2ITIkoKUgoKGlNjGgsaCgtiClIKClIKChpSUgoKUgoKElMaC2LaCuKaCpoIAOXaigoKClIKClIKCgoK4goKUgoKCgoLKgoKCgoKS3IKCgqaCgoKCgriCgoK4goKCgriCgoKmgoKCgriCgoKmgoKmgoKmgoKSgoKClIKUgpLKpqKCpJqmlKaCgqaCgoKChKaClJSUtLSClLS0goKUgoSUtIKClIKChJTGgsaE6oKCpoTYgoLYhMaCgsaClMa2grimggAKFoKCgoKCgoKCgoLKlIKCppSClIKCgqaCgsqmgoKUgpSmgoKCgoSmgpSUlIKClIKElLSCgti0goLGgpTGtoK42oKCgqaC7oKCgoKClLimgpSClJSUtLSUAAoWgoKCgoKCgpSCpoKCgoLcpqKCpIKepoKCgoSmlILYtoK4AAoWgoKCppSCgoKU3IKUgoKCgoLKgoKCgoLKgoKCgoKSAAgSpoKClJKYpoKCgoSmlLS0goKUgoKClNiCgpSCgoKU2IKClIKCgpTYtoK47oKCgqaCgoKCgpSCgoKUgoKCgoKCpqaCgqaCgqaCgoKClIKClIKCpoKCgoKCpgAIEoKCgoKUgoKUgpSmpoKSgpKUkpSUgoKSgpK4goKSkoKSAAoWooKkgpimgpSmsoKClIKCgoSUgoKCgoSUlKaUgobGgoKUgoKElOq2grjugoKCgoKCgpSmgoKUgu6CgoKCgoK4pqKkgpiU2oKCgqaC7oKCgoKCgoKUpoKClIKClILugoKCgqamgpIACx6UgoKClJSCpoKCgoKUlIKUgoKCgoKCgoK4goKCgoKCgoKmpoKCpoKCpoKCgqaCgqaUgoKUgoKClIKUgoKCgriClIKCgpSCgpSCgoKUgpSCpoKmAAgSgoKCgoKUgrimoqSClIKCpu6UgoKCgoKCgoKClKaCgqaCgqaCgqaU3IKmggAGJAALAoCCgIKC2IKClII=", "28-34:1;110-117:1;111-116:1.1;113-115:1.1.1;122-126:2;123-125:2.1;131-133:3;138-142:4;139-141:4.1;162-164:5;168-170:6;183-187:7;196-202:8;197-201:8.1;211-217:9;212-216:9.1;226-232:10;227-231:10.1;241-249:11;242-248:11.1;244-246:11.1.1;258-264:12;259-263:12.1;273-277:13;274-276:13.1;286-295:14;287-294:14.1;290-292:14.1.1;304-308:15;305-307:15.1;313-319:16;314-318:16.1;325-341:17;326-333:17.1;328-330:17.1.1;334-340:17.2;336-338:17.2.1;350-372:18;353-357:18.1;358-362:18.2;363-365:18.3;368-370:18.4;760-764:1;761-763:1.1;772-778:2;773-777:2.1;774-776:2.1.1;782-790:3;783-789:3.1;785-787:3.1.1;794-796:4;800-805:5;802-804:5.1;809-811:6;816-820:7;817-819:7.1;824-826:8;830-834:9;831-833:9.1;838-840:10;854-868:11;857-859:11.1;864-866:11.2;1016-1051:1;1017-1050:1.1;1020-1026:1.1.1;1021-1025:1.1.1.1;1022-1024:1.1.1.1.1;1031-1033:1.1.2;1042-1044:1.1.3;1142-1148:1;1183-1201:1;1186-1188:1.1;1189-1191:1.2;1193-1200:1.3;1196-1198:1.3.1;1258-1310:1;1263-1309:1.1;1279-1285:1.1.1;1280-1284:1.1.1.1;1289-1295:1.1.2;1290-1294:1.1.2.1;1299-1307:1.1.3;1300-1306:1.1.3.1;1302-1304:1.1.3.1.1;1470-1481:1;1487-1520:1;1489-1491:1.1;1492-1518:1.2;1499-1504:1.2.1;1501-1503:1.2.1.1;1508-1516:1.2.2;1509-1515:1.2.2.1;1511-1513:1.2.2.1.1;1632-1637:1;1634-1636:1.1;1702-1704:1;1859-1866:1;1863-1865:1.1", "", "459=ReadUint16/1/1/1,ReadBytes/1/1/1,readUint8LengthPrefixed/1/1/2;499=ReadUint16LengthPrefixed/1/1/1;519=ReadUint16LengthPrefixed/1/1/1,Empty/1/1/2;541=ReadUint16LengthPrefixed/1/2/1,ReadUint16LengthPrefixed/2/2/2;670=readUint16LengthPrefixed/1/1/1;707=ReadUint32/1/1/1;750=Clone/1/20/1,Clone/2/20/3,Clone/3/20/4,Clone/4/20/5,Clone/5/20/6,Clone/6/20/9,Clone/7/20/10,Clone/8/20/12,Clone/9/20/13,Clone/10/20/14,Clone/11/20/16,Clone/12/20/18,Clone/13/20/20,Clone/14/20/21,Clone/15/20/22,Clone/16/20/24,Clone/17/20/25,Clone/18/20/26,Clone/19/20/27,Clone/20/20/28;945=ReadUint16/1/2/1,ReadBytes/1/1/1,readUint8LengthPrefixed/1/1/2,ReadUint16/2/2/3,ReadUint8/1/1/4;964=ReadUint16LengthPrefixed/1/1/1;999=Empty/1/2/1,Empty/2/2/1;1039=readUint16LengthPrefixed/1/1/1;1147=ReadUint16LengthPrefixed/1/1/1,Empty/1/1/1;1156=ReadUint16LengthPrefixed/1/1/1;1172=Empty/1/2/1,Empty/2/2/1;1253=ReadUint8/1/1/1,Empty/1/1/1;1314=ReadUint32/1/2/1,ReadUint32/2/2/2,readUint8LengthPrefixed/1/1/3,readUint16LengthPrefixed/1/1/4,ReadUint16LengthPrefixed/1/1/5,Empty/1/1/6;1322=ReadUint16LengthPrefixed/1/1/1;1422=ReadUint8LengthPrefixed/1/1/1,Empty/1/2/1,ReadUint16LengthPrefixed/1/1/2,Empty/2/2/3;1430=ReadUint16LengthPrefixed/1/1/1;1649=ReadUint8LengthPrefixed/1/1/1,Empty/1/2/1,unmarshalCertificate/1/1/2,Empty/2/2/3;1671=ReadUint16LengthPrefixed/1/1/1;1678=ReadUint16LengthPrefixed/1/1/1;1689=readUint24LengthPrefixed/1/1/1;1771=ReadUint8/1/1/1,readUint24LengthPrefixed/1/1/2,Empty/1/1/3;1836=readUint24LengthPrefixed/1/1/1,Empty/1/1/2")]
[assembly: go.GoPositionMap("crypto/tls/handshake_server.go", "handshake_server.cs", "ACxUsoKCpoLsptqmopSAgsiCgIKklICCpICCpICCpICCpICCpIKAgtqAgqSAgqSAgqSAgqSCgoCCpICCpICCyIKU2NaCgpSCgoKqooKCgoKCgqaCgriCgoKCgIKCkoK2lILukoKUgoKClIKCAAgSgoKmgoKm1oKUgpSUgoKCuIKCpoKUgoKClJSUgoKCpoKCpoKCgoKmgoKClIKUgoKClJSUgqaCgoKm3KaAgpS0tLSCtoCClLSCyKyygpSUlIKCgoKUggAHEIKU2qKCgoKCuIKCgoKU7oKSgqbWopSUgoKCppSCgpSCgqaClIKClLimooKClIKCpIKkgpSClNiylIKmgoKCgpSClJSCgpSCgpTcgoK4gqaUgoKCpoK4lIKmgoKClIKUgpTKhLiClKaUgpSUpoKCgoKCgoKCgqailIKmuIKCgoCCpICCtoKAgoLIlNailIKmgqaClIKmlICCpICCtoKCgIK2goKCgILIgoKCgpSCgIKCpICCyIKUgriCggAHEIKUgILIgoCCtoCCtpSCgsqCgoKCpoCmpIKmgoKmgoCCgtqCgoKmgoKClIKCpoKCgpSUpoCCggAJEriCgpSCgoKmgoKCgoKUgoKUgoKUgoKUgIKCtoKCgpSCgIKCyJSAgsiUpoKUlpKkgoKCgpSCpoKU1qKUgILsgoKUgoKCpoKEgqaAgraCpqKCpoKUgoKmlIKCgoKmgoKUgoK4gIK2pqKUgIK2goKAgraU2sKCgoKCgIKCpIKCgIKC2oKClJSmgu6CpoKCgIKSgIKUtKaCgoK4goKUguaCuIKAgoLIpoKCgqY=", "", "", "591=supportedCipherSuites/1/1/1;608=time/1/1/1;613=anyValidVerifiedChain/1/1/1;857=Sum/1/1/1;954=keysFromMasterSecret/1/1/1;1005=ConstantTimeCompare/1/1/1;1126=time/1/1/2,NewCertPool/1/1/3")]
[assembly: go.GoPositionMap("crypto/tls/handshake_server_tls13.go", "handshake_server_tls13.cs", "AEaEAaKmgIKkgIKkgIKkgoCCpICCpICC2oCCpICCpICCtpQADQailLiClIKCAAsYgqaCgpS4hIKmgoCCgraCgqaCgoKk7oKmgpSCgpSClIKCgqaCgqaCggAJFIKGgoKUgoKCppSmlpSCsoKCpoKCgpSUlIKCgpSCgoKmgoKClJSUgoKCuIKClJSCgriC9oKUgqaCgoKCpoKmgoKUgqaCgqaCgoKCgpSCpoKClIKCgriCpoKCpoKC3IKCgpSClIKUyoSmgoCCyJKUgoKClIKCgpSCgoKCpoaUgoCCpIKAgsiCgoKClIKCgqYACBDSgIKAgsiKgpSCgpSCgoKUgIKkpoKmgriCpoKCgpSUlIKmgpSUqqKClIKUlAAOBoLKgoLKgIKkgoKClAAJFJSCgoCCpJKCgoKUgqaAgraAgsiCgqaCgoKmgoKCpoKCgqaCgqaCgoKmgoKCpoKCgqa4goKUlIKCpoKCpoKCpoKCrLKMlIKCpoKCpoKCpoKCpoKCpoKCpgACIIKUgoKCgoCCtpKCgoKUgqaAgraAgraAgraCgqSUgoKCgIK2goKAgsiCgoKUgoKCpoKUgoKClIKmgsqCgoKCgqaCgoKCuICCtqaCpoKmgqaUgoKCgoKCpoCCyJSCgpSAgraCgpSSgqaCgoKUgoKCgISUpJSUgIK2poKUuICC2pSCgpSCpoKCgpSCgoKmyoKAgsimgoK4grimgpSCpoCCtpSClNaigoK4ppSCgoKCgoKCgqaCgoKUgoKmyoKAgqSUlKaAgramgpSmgoCCgrbcgoKmgoKCpoCCtoKAgoLIuIKCpoKCgtyEgpSCgpSClIKAlIKklICC7ICCttaCpoKCpoKCgqaCgqaAgrY=", "210-212:1;217-224:2;225-227:3;228-230:4", "389=crypto.Hash.New;560=crypto.Hash.New;724=crypto.Hash.New;748=crypto.Hash.New;978=crypto.Hash.New", "418=time/1/1/1;423=anyValidVerifiedChain/1/1/1;769=Equal/1/8/1,Equal/2/8/2,Equal/3/8/3,Equal/4/8/6,Equal/5/8/8,Equal/6/8/10,Equal/7/8/12,Equal/8/8/13;976=finishedHash/1/1/1;1072=Size/1/1/1;1189=signatureSchemesForPublicKey/1/1/1,isSupportedSignatureAlgorithm/2/2/1")]
[assembly: go.GoPositionMap("crypto/tls/key_agreement.go", "key_agreement.cs", "ACdOkuyC1sKClIKClJSCgqaCggAHENaC1sKCgoKCgqaCgpSCgpSCgoKCgqiSgoKUqqKCgoKUgoKqwoKUABgu0oKCgriClICCtoKClKaCgoKCgoKUgoKmgoKCgpSSgpSCgpSCgpSCgpSCgqaCgpSCgpSCgriCgoKUgoKCgoKClIKClKaigqaCgpSCgqbm4oKUgpSUgoKUgpSCgpSCgoKCpoKClJSCpoCCtoKClJSCgpSCgqaCgoKClIKCgoKUgoKUgoKUgpSCgIK2goKUgpSCgILI1oKCptqigoKmpoKUpKSkpA==")]
[assembly: go.GoPositionMap("crypto/tls/key_schedule.go", "key_schedule.cs", "ABYw8ujSkpLc0pKSgqrCggAaNoKCpIKkgqSCpJSkpKSkyMjIAAgSgoKClKaCgoKUgoKUgoKUpqKCgpSCgpQADB6iqoKUgpSCgIKkgoKUuIKUlNaigpSSgoKUgpSqkpSClIKClIKCgoKUgpSC1rKClJKCgpSClJiSlIKUgoKUgoKUlA==", "49-51:1;77-79:1;80-82:2;83-85:3;86-88:4;174-176:1;217-219:1;256-258:1", "25=crypto.Hash.New;30=crypto.Hash.New;31=crypto.Hash.New;39=crypto.Hash.New;40=crypto.Hash.New")]
[assembly: go.GoPositionMap("crypto/tls/prf.go", "prf.cs", "ABYuwoKCqJKCgpSCgoKCgoKClIKCupKCgpSCgpSCgoKUgqaokgAPIIKUpIKUpLiCgqqigoKUqqKCpqSsAAoCgoKUgoKCgoKCgoKCgoKCgqaCgoKmgoKmABAogoKUgoKmgqamgoKmgoKqoqqiqqKCpqqi3LLcsqiSsqamgoKUlIKUgoKUgqY=", "74-76:1;256-281:1", "126=crypto.Hash.New")]
[assembly: go.GoPositionMap("crypto/tls/quic.go", "quic.cs", "AB0wgpSkpKSkAIIBrALiruKmosqCAAgS4oKUgoKUgoCCpKqigoCmpIKCgpSAgoKUgoKCpIKCgpSCgoKokoKUgoKm2uKCgpSCgoKUpoKSgoKCgoKCgpSClICCtoKUABAYsoKClIKUgpSClILewoKClIKClIKCqJKuwoKUgoKCvKKClICCpIKCuKaigoCCtqbqgoKU3KaCAAcQgoKClIK4lKaiuIKCgIK2poLcgtyigriCgIK2poLKggAEFAAMBoK4uIKUlIKC")]
[assembly: go.GoPositionMap("crypto/tls/ticket.go", "ticket.cs", "AGneAQAIAoKCgpSUgoKClIKCkriClJSClJTKgoKkgoKUgpLcgoKmgoKCppSmgoKClPiSgpKigoIAABSUgoKClJSUtLSUlLS0lJS0tJSCgoKUlIKUgoKCgpSCgoKUgoKUgoKCgpSCgpSUlIKCgpSUgoKCuIKmqqIADiLCgoKClNaCgqaCgoKClICCpIKCgpSUgoKUruKCgoKUgoKUpoKCpoKCgpSigoKUgqaCgpSClKYACBzygpQAAhDygg==", "121-123:1;124-130:2;126-128:2.1;146-161:3;148-159:3.1;155-157:3.1.1;163-165:4", "", "145=certificatesToBytesSlice/1/1/1;207=ReadUint8/1/3/1,ReadUint16/2/2/2,readUint64/1/1/3,readUint8LengthPrefixed/1/1/4,ReadUint24LengthPrefixed/1/1/5,ReadUint8/2/3/6,ReadUint8/3/3/7,unmarshalCertificate/1/1/9;316=time/1/1/3,Unix/1/1/3")]
[assembly: go.GoPositionMap("crypto/tls/tls.go", "tls.cs", "ACxcwriSrsLKkgAQFqKCgpSu4oKCgt70hJSCgpQACAqAooCigAACGAAKAqbSgoKCtoKCgraCgqaCgpSUgriUgoKmgoCCgqQABRLiABAy8qaCgpQAAhgACQKClJQAAhYACAKCgpSCgpSmAAwQ4pSCgoKCgoKUgpS4goKUgpSmgoKCgoKClIKUlIKUyoKCpoKUpoKCppSCgpSCxoKClILGgoKUgtam7LKAgqSAgsS0toCCtg==", "267-267:1")]
// </GoSourcePositionMaps>

namespace go.crypto;

[GoPackage("tls")]
public static partial class tls_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface aead {}
    internal partial interface cbcMode {}
    [GoLocalName("binaryMarshaler")] internal partial interface cloneHash_binaryMarshaler {}
    internal partial interface constantTimeHash {}
    internal partial interface handshakeMessage {}
    internal partial interface handshakeMessageWithOriginalBytes {}
    internal partial interface keyAgreement {}
    internal partial interface keyExchange {}
    internal partial interface transcriptHash {}
    internal partial struct alert {}
    internal partial struct atLeastReader {}
    internal partial struct certificateMsg {}
    internal partial struct certificateMsgTLS13 {}
    internal partial struct certificateRequestMsg {}
    internal partial struct certificateRequestMsgTLS13 {}
    internal partial struct certificateStatusMsg {}
    internal partial struct certificateVerifyMsg {}
    internal partial struct cipherSuite {}
    internal partial struct cipherSuiteTLS13 {}
    internal partial struct clientHandshakeState {}
    internal partial struct clientHandshakeStateTLS13 {}
    internal partial struct clientHelloMsg {}
    internal partial struct clientKeyExchangeMsg {}
    internal partial struct cthWrapper {}
    internal partial struct ecdhKeyExchange {}
    internal partial struct ecdheKeyAgreement {}
    internal partial struct echClientContext {}
    internal partial struct echConfig {}
    internal partial struct echConfigErr {}
    internal partial struct echExtType {}
    internal partial struct echServerContext {}
    internal partial struct encryptedExtensionsMsg {}
    internal partial struct endOfEarlyDataMsg {}
    internal partial struct finishedMsg {}
    [GoValueClone("seq", "scratchBuf")] internal partial struct halfConn {}
    internal partial struct helloRequestMsg {}
    internal partial struct hybridKeyExchange {}
    internal partial struct keyShare {}
    internal partial struct keySharePrivateKeys {}
    internal partial struct keyUpdateMsg {}
    internal partial struct listener {}
    internal partial struct lruSessionCache {}
    internal partial struct lruSessionCacheEntry {}
    internal partial struct newSessionTicketMsg {}
    internal partial struct newSessionTicketMsgTLS13 {}
    internal partial struct permanentError {}
    [GoValueClone("nonce")] internal partial struct prefixNonceAEAD {}
    internal partial struct pskIdentity {}
    [GoValueClone("eventArr")] internal partial struct quicState {}
    internal partial struct rawExtension {}
    internal partial struct recordType {}
    internal partial struct rsaKeyAgreement {}
    internal partial struct rsaSignatureSchemesᴛ1 {}
    internal partial struct serverHandshakeState {}
    internal partial struct serverHandshakeStateTLS13 {}
    internal partial struct serverHelloDoneMsg {}
    internal partial struct serverHelloMsg {}
    internal partial struct serverKeyExchangeMsg {}
    [GoValueClone("aesKey", "hmacKey")] internal partial struct ticketKey {}
    internal partial struct timeoutError {}
    internal partial struct weakCertCache {}
    [GoValueClone("nonceMask")] internal partial struct xorNonceAEAD {}
    internal partial struct ΔfinishedHash {}
    public partial interface ClientSessionCache {}
    public partial struct AlertError {}
    [GoMemberRecord("PrivateKey", GoMemberFact.Descriptor, typeof(go.crypto_package.PrivateKeyᴅ))] public partial struct Certificate {}
    public partial struct CertificateRequestInfo {}
    public partial struct CertificateVerificationError {}
    public partial struct CipherSuite {}
    public partial struct ClientAuthType {}
    public partial struct ClientHelloInfo {}
    public partial struct ClientSessionState {}
    [GoValueClone("SessionTicketKey")] public partial struct Config {}
    [GoValueClone("clientFinished", "serverFinished", "@in", "@out", "tmp")] public partial struct Conn {}
    public partial struct CurveID {}
    public partial struct Dialer {}
    public partial struct ECHRejectionError {}
    public partial struct EncryptedClientHelloKey {}
    public partial struct QUICConfig {}
    public partial struct QUICConn {}
    public partial struct QUICEncryptionLevel {}
    public partial struct QUICEvent {}
    public partial struct QUICEventKind {}
    public partial struct QUICSessionTicketOptions {}
    [GoValueClone("RecordHeader")] public partial struct RecordHeaderError {}
    public partial struct RenegotiationSupport {}
    public partial struct SessionState {}
    public partial struct SignatureScheme {}
    public partial struct echCipher {}
    public partial struct echExtension {}
    public partial struct ΔConnectionState {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸcrypto() => builtin.initPackage(typeof(crypto_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸaes() => builtin.initPackage(typeof(go.crypto.aes_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸcipher() => builtin.initPackage(typeof(go.crypto.cipher_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸdes() => builtin.initPackage(typeof(go.crypto.des_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸecdh() => builtin.initPackage(typeof(go.crypto.ecdh_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸecdsa() => builtin.initPackage(typeof(go.crypto.ecdsa_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸed25519() => builtin.initPackage(typeof(go.crypto.ed25519_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸelliptic() => builtin.initPackage(typeof(go.crypto.elliptic_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸfips140() => builtin.initPackage(typeof(go.crypto.fips140_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸhkdf() => builtin.initPackage(typeof(go.crypto.hkdf_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸhmac() => builtin.initPackage(typeof(go.crypto.hmac_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸhpke() => builtin.initPackage(typeof(go.crypto.hpke_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸboring() => builtin.initPackage(typeof(go.crypto.@internal.boring_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸaes() => builtin.initPackage(typeof(go.crypto.@internal.fips140.aes_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸaesꓸgcm() => builtin.initPackage(typeof(go.crypto.@internal.fips140.aes.gcm_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸtls12() => builtin.initPackage(typeof(go.crypto.@internal.fips140.tls12_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸtls13() => builtin.initPackage(typeof(go.crypto.@internal.fips140.tls13_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸmd5() => builtin.initPackage(typeof(go.crypto.md5_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸmlkem() => builtin.initPackage(typeof(go.crypto.mlkem_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(go.crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrc4() => builtin.initPackage(typeof(go.crypto.rc4_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrsa() => builtin.initPackage(typeof(go.crypto.rsa_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha1() => builtin.initPackage(typeof(go.crypto.sha1_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha256() => builtin.initPackage(typeof(go.crypto.sha256_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha512() => builtin.initPackage(typeof(go.crypto.sha512_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsubtle() => builtin.initPackage(typeof(go.crypto.subtle_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸtlsꓸinternalꓸfips140tls() => builtin.initPackage(typeof(go.crypto.tls.@internal.fips140tls_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸx509() => builtin.initPackage(typeof(go.crypto.x509_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸpem() => builtin.initPackage(typeof(encoding.pem_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸhash() => builtin.initPackage(typeof(hash_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(go.@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸcryptoꓸchacha20poly1305() => builtin.initPackage(typeof(vendor.golang.org.x.crypto.chacha20poly1305_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸcryptoꓸcryptobyte() => builtin.initPackage(typeof(vendor.golang.org.x.crypto.cryptobyte_package));
    [GoInit] internal static void initᴛᴛimportꓸweak() => builtin.initPackage(typeof(weak_package));
    // </ImportInitializers>
}
