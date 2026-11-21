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
using context = go.context_package;
using cryptobyte = go.vendor.golang.org.x.crypto.cryptobyte_package;
using hash = go.hash_package;
using io = go.io_package;
using net = go.net_package;
using time = go.time_package;
using tls13 = go.crypto.@internal.fips140.tls13_package;
using Δx509 = go.crypto.x509_package;
// </ImportedTypeAliases>

using go;
using static go.crypto.tls_package;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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
[assembly: GoImplement<finishedHash, transcriptHash>(Pointer = true)]
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
[assembly: GoImplement<net_package.Error, error>]
[assembly: GoImplement<newSessionTicketMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<newSessionTicketMsgTLS13, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<permanentError, error>(Pointer = true)]
[assembly: GoImplement<prefixNonceAEAD, aead>(Pointer = true)]
[assembly: GoImplement<rsaKeyAgreement, keyAgreement>]
[assembly: GoImplement<serverHelloDoneMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<serverHelloMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<serverKeyExchangeMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<xorNonceAEAD, aead>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<AlertError, alert>(Inverted = true, ValueType = "uint8")]
[assembly: GoImplicitConv<alert, AlertError>(Inverted = true, ValueType = "uint8")]
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(Certificate))]
[assembly: GoFieldAccess(typeof(CertificateRequestInfo))]
[assembly: GoFieldAccess(typeof(CipherSuite))]
[assembly: GoFieldAccess(typeof(ClientSessionState))]
[assembly: GoFieldAccess(typeof(Config))]
[assembly: GoFieldAccess(typeof(Conn))]
[assembly: GoFieldAccess(typeof(QUICEvent))]
[assembly: GoFieldAccess(typeof(SessionState))]
[assembly: GoFieldAccess(typeof(certificateMsg))]
[assembly: GoFieldAccess(typeof(certificateMsgTLS13))]
[assembly: GoFieldAccess(typeof(certificateRequestMsg))]
[assembly: GoFieldAccess(typeof(certificateRequestMsgTLS13))]
[assembly: GoFieldAccess(typeof(certificateStatusMsg))]
[assembly: GoFieldAccess(typeof(certificateVerifyMsg))]
[assembly: GoFieldAccess(typeof(cipherSuite))]
[assembly: GoFieldAccess(typeof(cipherSuiteTLS13))]
[assembly: GoFieldAccess(typeof(clientHandshakeState))]
[assembly: GoFieldAccess(typeof(clientHelloMsg))]
[assembly: GoFieldAccess(typeof(clientKeyExchangeMsg))]
[assembly: GoFieldAccess(typeof(ecdheKeyAgreement))]
[assembly: GoFieldAccess(typeof(echCipher))]
[assembly: GoFieldAccess(typeof(echClientContext))]
[assembly: GoFieldAccess(typeof(echConfig))]
[assembly: GoFieldAccess(typeof(echExtension))]
[assembly: GoFieldAccess(typeof(echServerContext))]
[assembly: GoFieldAccess(typeof(encryptedExtensionsMsg))]
[assembly: GoFieldAccess(typeof(finishedMsg))]
[assembly: GoFieldAccess(typeof(go.crypto.ecdsa_package.PrivateKey))]
[assembly: GoFieldAccess(typeof(go.crypto.ecdsa_package.PublicKey))]
[assembly: GoFieldAccess(typeof(go.crypto.elliptic_package.CurveParams))]
[assembly: GoFieldAccess(typeof(go.crypto.rsa_package.PrivateKey))]
[assembly: GoFieldAccess(typeof(go.crypto.rsa_package.PublicKey))]
[assembly: GoFieldAccess(typeof(go.crypto.x509_package.Certificate))]
[assembly: GoFieldAccess(typeof(go.encoding.pem_package.Block))]
[assembly: GoFieldAccess(typeof(keyShare))]
[assembly: GoFieldAccess(typeof(keySharePrivateKeys))]
[assembly: GoFieldAccess(typeof(listener))]
[assembly: GoFieldAccess(typeof(lruSessionCacheEntry))]
[assembly: GoFieldAccess(typeof(newSessionTicketMsg))]
[assembly: GoFieldAccess(typeof(newSessionTicketMsgTLS13))]
[assembly: GoFieldAccess(typeof(prefixNonceAEAD))]
[assembly: GoFieldAccess(typeof(pskIdentity))]
[assembly: GoFieldAccess(typeof(quicState))]
[assembly: GoFieldAccess(typeof(serverHandshakeState))]
[assembly: GoFieldAccess(typeof(serverHelloMsg))]
[assembly: GoFieldAccess(typeof(serverKeyExchangeMsg))]
[assembly: GoFieldAccess(typeof(xorNonceAEAD))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("crypto/tls/alert.go", "alert.cs", "AA8eggArYAAkSoKCgpSmgg==")]
[assembly: GoPositionMap("crypto/tls/auth.go", "auth.cs", "AB0skoKCgpSUgoKUgsaCgpSCxoKClICC1oKClIKAgtaUqqKUgoKUgsaCgpSCxpQABxAAChqikoKCgoKq0pS0tLS0lJS0tLS0tJSsspSk7KT4AA0igpSm7pSkpKTGgoKCgqaktAAJDtKCgpSCgryGgpTKgpS4goKmqqKktqaCgrikAAoKxuamgqY=", "239-241:1;245-247:2", "", "227=Size/1/7/6,Size/2/7/7,Size/3/7/8,Size/4/7/11,Size/5/7/12,Size/6/7/13,Size/7/7/14")]
[assembly: GoPositionMap("crypto/tls/cache.go", "cache.cs", "ABQmgoCCgsiCgqaCgIKgkoKUgqDGpg==", "33-33:1;38-38:2")]
[assembly: GoPositionMap("crypto/tls/cipher_suites.go", "cipher_suites.cs", "ACxYgoIAARTyABEu9gAPJrKCgqaCgqYAI0wAGDaigoKCpoKCuAAVLABFjAEAHj4AFzAADRwACxjegoKmlgAKHLKClIKAgqSAgramgoKmgoKClKaCgoKUqJKmgpSqogAWMoKClIKCpoKCAAkUgoKUgoKUgoKmpoKClIKCpqaCgpSCgpSCgpSCpIKmgoIAAhoACgKClIKClIKClIKkgqaCgqaCgpSCgqaCggAOHIKCgoKUgoKskoKCgoKCgpSmgqaC3IIABRCigoKmpoKCgqamgoKCpqaCgoKm", "608-610:1")]
[assembly: GoPositionMap("crypto/tls/common.go", "common.cs", "ADhespSkpKSkpABTxAGClKS4gpSkAC1wAFfkAQAKAgAcQqKUpABm8AGyABQ2sgCRA/IG4oiygoKCAAYgAA4CgpSCggAqUJgACgaEuIKCgoKCgtyCgrYAAhYAEwaCgoKClIKCgoKUpoKCgpSCgqaCuIKCgpSCgoKUgoK0gqaUAAUgAA8CgqaCoqaCgqaCgoKUpoKCgpSmooKClIKYgpiqoqYACxiYwoKCgpSCgqaClIKUgpSUpoKCgpSssoKCgpSUpqKCgpiCmIKUpoKqooKCgqYACxiYwoSCgriCppSmgoKAgqSCgoKCgsqygsoADhwAEwyCgpSCgsqCgoKUggAHEKSCyoKCpriCgpSClJSClMqCgu6CuIKClIKmgoCCpIKUtLS0lIKCgoKmgpS0gpT2puyCgpSCgqaCpoKUlIKm7NKAgraCpoKmgoKCuIKCuAACFPKCgoKCgriClIIADxqCgqaUgoKUACNKooKUAB5GspSClAAGEvKClIKCgpSCgpSmgoKCpoKCgoKCgqKo8oKUgoKUAAgKgqaCAAQQsoKClKqUgoIACBKCyoKCgpSCgoK4qqKCqoIACRSCpoIABhLigqaCgoK4gqamgoKmgoK4rsKCgpSGAAgSgoKClIKCpoKCuA==", "1189-1191:1;1194-1196:2;1273-1275:1;1278-1280:2;1402-1432:1;1419-1427:1.1;1500-1517:2;1757-1759:1;1800-1802:1;1872-1874:1", "", "992=HasPrefix/1/1/1;1692=New/1/1/2")]
[assembly: GoPositionMap("crypto/tls/common_string.go", "common_string.cs", "/oaigoKCgoKCgoKCgoIAFSCWgpSkpKSkpKSkpIKkpoaigoKCgoKCAAkUgpaClIKkpIKkpoaigoKCgtqUgoKU")]
[assembly: GoPositionMap("crypto/tls/conn.go", "conn.cs", "AHOGAtKokqyyqqKssqyyABY2goKClIKClJSqooKCqqKClIKCgoKCrNKCgoKCqJKCgoLurLKCppSktoKUtM7igqaClKaUgqaClIKCyoKCggAKFpSCpoIACRaigoK4gqaClJSClLSClIKClJSCgpSCgoKmgoKCxoKCgqaCgpQABxDEtoKClIKmgoKCgpSCyqaCgoKmgoKCgoIACBKCgqamgqzigpSClIKqooKmgoKCAAkUlILKgpSCgoK0goKmgqaClIKClKaCgsaCgoKCgoKCgpSClMTIgoKClAAQIqCkooKCgqaCpoIADCIAEAKClKaClJSCuLiClIKUlILcgoKmgoKmlIKCgoKUyoKmgoKClIKClLiCgoKUgriCppS4gqaUpoKUgpSClNyUlJSmpKTIgqaC7oKUgpSCyIK4gsq2gpS26sKCgoKUAAoWgoKUgoKClIKUqqKClLiCgqiygqaUtKSUgpSmqMKCggARQgAQAoKmgriCgpS0tKamxLaCuIKCgqaCgpSmgoKCpoKCpoKCpoKCgoK4AAgQ4oKClIKCgqamgoLsgqaCgoKCpoKCgqaCppSCgoKUgoKClIKUgqaCgrjc4oKUgoKUgqba0oKCgtiygpSCgqas0oCCpJS4uKaCgoKUgIKkgqaigpS0tLSClMaClMaClOq0tLS02LS0tLTclIKmgqaoAAESAAsEgoKClIKmlIKmgpSCpoKmggALGIKCgoKClLiCAAsI4oKmgoKmgoKCpoKmlKSCAAgKgqaClIKClAAJCsKCpoKClIKCgqaUpNyC5uKCgqaCgqaCgpSCgoKUgpSCpoKmgoCCtgAFEgALAoCCpKamgpSCgIKkgoLKAAgShIK42LSSgoKClIKm7qaCgoK4gIKkppqygqam0oKUlIKClJQABSAADQIAAhoADAbmAAgIgqa4pIKCgpSClJKUyoKUgIKkgqaClIKCuKaClIKmgoK4gqaCgoKUypSCpujigoLWlKKCgoKCgoKCgoKCgoKCgoKCgpSmgoKCgoKClKaUgqrygpQADQwACAKCgoKUgpSClAAJDAAICIKClJSUgqyy", "996-1004:1;1538-1540:1;1541-1546:2;1642-1648:1", "", "1593=Bytes/1/1/1", "1736:21", "1067:21,1073:13,1747:25,1748:17")]
[assembly: GoPositionMap("crypto/tls/defaults.go", "defaults.cs", "AA4ggpiiprakAAYWwgAPIIKUgoKUuIKCABAqAA8g", "71-75:1")]
[assembly: GoPositionMap("crypto/tls/defaults_fips140.go", "defaults_fips140.cs", "ABo6uAAHEAALGAAHEMyClKSktA==")]
[assembly: GoPositionMap("crypto/tls/ech.go", "ech.cs", "ACdWAAkMgoKUpqKCgoKUgpSClIKCgpSClIKUgpSCgpSCgoKUgpSUgpSCgpSCgoKUgoKClIKUpqyygoKClIKUgoKClIKCgpSCgqamgrKClIK4gqaClIKClIKmlLiCgpSCgpSmpqKCgpSUgoKUlJSmooKClKaigoKUAA0QooK4lIKCgqaCgoKElJQADQYACBKCsoKM3IKCuIKCpoKCkoKClIKUgpSSgoKCgoSClIKCgpSCgoSCnMKCgoKClISClIKCgpSCpoKSlKaCgu6CgpSCgqaCpoLcgriCgpS4gqamooKmgoKCgoKCkJKQkqaigoKUgoLKgoKClIKClIKCgpSCgpSuwoKUgoKUgoKClIKClIK4ABAagqaCAAYS8oKCgoKCgpSCgoKClJSCgpSCgpSCgpSCgpSCgpSCgsqmsqKCgpSmpoKmgqaCgoKClIKUgoKClIKCgpSCgoKUgoKClIKClKaClAAIEoKCgqaU7qaCgoKSgoKUgqaClA==", "299-371:1;301-303:1.1;304-306:1.2;307-309:1.3;310-370:1.4;358-360:1.4.1;365-367:1.4.2;424-424:1;425-425:2;651-659:1", "", "258=skipUint8LengthPrefixed/1/2/1,skipUint16LengthPrefixed/1/1/2,skipUint8LengthPrefixed/2/2/3;273=ReadUint16LengthPrefixed/1/1/1;307=readUint8LengthPrefixed/1/2/1,readUint16LengthPrefixed/1/1/3,readUint8LengthPrefixed/2/2/4,ReadUint16LengthPrefixed/1/1/5;345=ReadUint16LengthPrefixed/1/1/1;356=Empty/2/2/1")]
[assembly: GoPositionMap("crypto/tls/handshake_client.go", "handshake_client.cs", "AENWooKCpoKCgpSmgqaCgriClAARJIKmgqaUgqqCggAHEIKCgriCgqaClIKmgoKClKaCyoKCgpSCgqaCuIKCgpSClKaCgoKUgpSCgpSCgpSCuIKCgpSCgoK4ABIe4oLKgpSCgqaCgpSC7oKC3JS4lIKCgu6CuJSCpoKCgoKClIKUgriCgqaCgoKmgtyCgoKEgqaCAAoWpu4ACAqU9IKmuJSm3IK4goKUgoKUpoKCgoKmgqaUgpSClJSmlMqUgrimgriCgpSmgriCgsqCgpSCgoKCgqaCpoKCyoKCgoLugriCpoKCgoCCtqaygoKmgoKCpoKCgpTqwgAJFIKCpoKCptyCpoKUgqaCgoKClIKUgpS4goKCpoKUgqaClIKUgpSClIKClIKmgqaClOaCgoKmgoKUgoKmggAMBqKUgoKUgoKCpoKCpoK4yoKmlIKCuKaCAAgSgoK4lIKCgoKClIKCpoKCuIKCgoKUgoKCpoKCuIKCgtyCgoKCuIKCgpSCgriCgqaCgoKUlKaCgqaClIKCgqaCgoKClJKClIKCgoKUgoKUgoKUgoKCpoKCgpSCgoKCuIK4lKaClISSkoKCgoKUgqaCgqamABIIgpSAgraCgqaCgoKClKaCpoKCgoK4goKCgoKCuICCgqSUlIKmgoKmgoK4goK4goKCgqaClJQACgqigpSUlIKUgoKm5qKUgtyCgpSCgoKmgoSCpoKmguaigpSUgoKmgoKUgoKCpoKmgoKUlIKCpoKClIKCpqKUgIK2goKAgqSC7pSCgoKClKaqwoKCgoKClIKCgoKmpoKCgoKCpu6ClIKCgqaCgoKmgu6ClIKCgqaCgoK4tLSCppSCgoK4goKCuKqi3JKClLTI3JTq2MjKgoKCgpSUgsaC2qaigqaygpS4qqKClIKUrLKCgpSClIKUgpSmooKClIKC", "99-101:1;241-253:1", "501=crypto.Hash.New", "97=hostnameInSNI/1/1/7,curvePreferences/1/1/8;463=time/1/1/1;847=Sum/1/1/1;931=keysFromMasterSecret/1/1/1;954=Equal/1/1/1;1120=ConstantTimeCompare/1/1/1;1254=time/1/1/2,NewCertPool/1/1/4;1277=time/1/1/2,NewCertPool/1/1/4", "", "276:25,283:17")]
[assembly: GoPositionMap("crypto/tls/handshake_client_tls13.go", "handshake_client_tls13.cs", "ADZcwriCgriCpoCCtpSAgraCgoK4goCCpICCyIKCgoKCkoKCgpSCgoKCgpSCgqaCgqa4gIK2goCCpICCpICCpICCpICCpICCpICCpICCpICCtoKCppQAEgqilIKCpoKCpoKCpo6CpoKCpoKCpoKCgpSCgpSClKqigpSClJQAEgqiyoKCgoKCpoKCgoKCgpSCgoKmgoKCgoKSgoKClIKCgoKCuIKUlILcgoKmgqaCgtyAgoKClIaClIKCgpSCgoKmtoKCgpSUgpSCgoKCpoCCyIK4goKmyoKCgqaCpqaCuIKCpoKCgpSUgqaCABAGopSCgqaCgqaCgqaCgpSGgqaCpoKCpoKUgoKUgoKmgoKCgoKC5oKUgoKClIKCgpSUgoK2lIKCgoKmgoKCuIKCgpSCgoKmlAAQBoKUgoKmgoKCpsqClJSClIKUlIKCuIKClIKUgoKClIKCpoKCgoKCuAAIBoK4uIKCgqamgoKmgoKUgoK4goKClIKCpoKUgtyCgqaCgoLchIKUgoKUgpSClIKUlIKm5oLKgoKmgoKCpoKCgqaCyoKCgqaCgoKUgoKCppSmgpSCpoKClKbcgqaUgoKUgriCpoKUgqaCppKCpoKCgpSCgoKUlIKmpoKUuIKmlIKmgqYADgaygoKmgriClIKCgqaCgriCgqaCgqamgoKCgoKCgoKUgoKm", "316-318:1;433-435:1", "92=crypto.Hash.New;267=crypto.Hash.New;487=crypto.Hash.New;868=crypto.Hash.New", "766=signatureSchemesForPublicKey/1/1/1,isSupportedSignatureAlgorithm/2/2/1;925=finishedHash/1/1/1;997=Size/1/1/1")]
[assembly: GoPositionMap("crypto/tls/handshake_messages.go", "handshake_messages.cs", "ABMqgqqikoKUgrqygqrCkoKUgqrCqsKqwgAjSLKClIKCgoKCypSCgoK4lIKCppSCgoK4lIKUlIKUlIKUlIKCpoKCgq7ClIKUgoKCgriUgpSCgoKC3JSClIKCgoLclIKUgoKCgtyUgpSCgoKCgu6UgpSCgoKC3JSClIKCgsqUgpSCgoKygpLulIKUgoKCyoKCkpKC3JSCgoKykpSmgoKS3IKCpoKCkoKCgoKmgoKmgqaCksqmgqzSgoKCpoKClIKCgqbqooKUgoKmlKaigpSWpoKClIKCgoKClIKUpoKmlKaCgqaCgoKChKaClIKUlpKClIKCgoaUgpSUlJSC2pKChpS2koKUgoKClNiE2IK2koKUgoKClNqSgpSCgoKU6oKUxraSgpSCgoKU2LaSgpSCgoKU2ITIkoKUgoKGlNjGgsaCgtiClIKClIKChpSUgoKUgoKElMaC2LaCuKaCpoIAOXaigoKClIKClIKCgoK4goKUgoKCgoLKgoKCgoKS3IKCgqaCgoKCgriCgoK4goKCgriCgoKmgoKCgriCgoKmgoKmgoKmgoKSgoKClIKUgpLKpqKClJqmlKaCgqaCgoKChKaClJSUtLSClLS0goKUgoSUtIKClIKChJTGgsaE6oKCpoTYgoLYhMaCgsaClMa2grimggAKFoKCgoKCgoKCgoLKlIKCppSClIKCgqaCgsqmgoKUgpSmgoKCgoSmgpSUlIKClIKElLSCgti0goLGgpTGtoK42oKCgqaC7oKCgoKClLimgpSClJSUtLSUAAoWgoKCgoKCgpSCpoKCgoLcpqKClIKepoKCgoSmlILYtoK4AAoWgoKCppSCgoKU3IKUgoKCgoLKgoKCgoLKgoKCgoKSAAgSpoKClJKYpoKCgoSmlLS0goKUgoKClNiCgpSCgoKU2IKClIKCgpTYtoK47oKCgqaCgoKCgpSCgoKUgoKCgoKCpqaCgqaCgqaCgoKClIKClIKCpoKCgoKCpgAIEoKCgoKUgoKUgpSmpoKSgpKUkpSUgoKSgpK4goKSkoKSAAoWooKUgpimgpSmsoKClIKCgoSUgoKCgoSUlKaUgobGgoKUgoKElOq2grjugoKCgoKCgpSmgoKUgu6CgoKCgoK4pqKUgpiU2oKCgqaC7oKCgoKCgoKUpoKClIKClILugoKCgqamgoIACx6UgoKClJSCpoKCgoKUlIKUgoKCgoKCgoK4goKCgoKCgoKmpoKCpoKCpoKCgqaCgqaUgoKUgoKClIKUgoKCgriClIKCgpSCgpSCgoKUgpSCpoKmAAgSgoKCgoKUgrimopSClIKCpu6UgoKCgoKCgoKClKaCgqaCgqaCgqaU3IKmggAGJAALAoKCgriCgpSC", "28-34:1;110-117:1;111-116:1.1;113-115:1.1.1;122-126:2;123-125:2.1;131-133:3;138-142:4;139-141:4.1;162-164:5;168-170:6;183-187:7;196-202:8;197-201:8.1;211-217:9;212-216:9.1;226-232:10;227-231:10.1;241-249:11;242-248:11.1;244-246:11.1.1;258-264:12;259-263:12.1;273-277:13;274-276:13.1;286-295:14;287-294:14.1;290-292:14.1.1;304-308:15;305-307:15.1;313-319:16;314-318:16.1;325-341:17;326-333:17.1;328-330:17.1.1;334-340:17.2;336-338:17.2.1;350-372:18;353-357:18.1;358-362:18.2;363-365:18.3;368-370:18.4;760-764:1;761-763:1.1;772-778:2;773-777:2.1;774-776:2.1.1;782-790:3;783-789:3.1;785-787:3.1.1;794-796:4;800-805:5;802-804:5.1;809-811:6;816-820:7;817-819:7.1;824-826:8;830-834:9;831-833:9.1;838-840:10;854-868:11;857-859:11.1;864-866:11.2;1016-1051:1;1017-1050:1.1;1020-1026:1.1.1;1021-1025:1.1.1.1;1022-1024:1.1.1.1.1;1031-1033:1.1.2;1042-1044:1.1.3;1142-1148:1;1183-1201:1;1186-1188:1.1;1189-1191:1.2;1193-1200:1.3;1196-1198:1.3.1;1258-1310:1;1263-1309:1.1;1279-1285:1.1.1;1280-1284:1.1.1.1;1289-1295:1.1.2;1290-1294:1.1.2.1;1299-1307:1.1.3;1300-1306:1.1.3.1;1302-1304:1.1.3.1.1;1470-1481:1;1487-1520:1;1489-1491:1.1;1492-1518:1.2;1499-1504:1.2.1;1501-1503:1.2.1.1;1508-1516:1.2.2;1509-1515:1.2.2.1;1511-1513:1.2.2.1.1;1632-1637:1;1634-1636:1.1;1702-1704:1;1859-1866:1;1863-1865:1.1", "", "459=ReadUint16/1/1/1,ReadBytes/1/1/1,readUint8LengthPrefixed/1/1/2;499=ReadUint16LengthPrefixed/1/1/1;519=ReadUint16LengthPrefixed/1/1/1,Empty/1/1/2;541=ReadUint16LengthPrefixed/1/2/1,ReadUint16LengthPrefixed/2/2/2;670=readUint16LengthPrefixed/1/1/1;707=ReadUint32/1/1/1;750=Clone/1/20/1,Clone/2/20/3,Clone/3/20/4,Clone/4/20/5,Clone/5/20/6,Clone/6/20/9,Clone/7/20/10,Clone/8/20/12,Clone/9/20/13,Clone/10/20/14,Clone/11/20/16,Clone/12/20/18,Clone/13/20/20,Clone/14/20/21,Clone/15/20/22,Clone/16/20/24,Clone/17/20/25,Clone/18/20/26,Clone/19/20/27,Clone/20/20/28;944=ReadUint16/1/2/1,ReadBytes/1/1/1,readUint8LengthPrefixed/1/1/2,ReadUint16/2/2/3,ReadUint8/1/1/4;963=ReadUint16LengthPrefixed/1/1/1;998=Empty/1/2/1,Empty/2/2/1;1038=readUint16LengthPrefixed/1/1/1;1146=ReadUint16LengthPrefixed/1/1/1,Empty/1/1/1;1155=ReadUint16LengthPrefixed/1/1/1;1171=Empty/1/2/1,Empty/2/2/1;1252=ReadUint8/1/1/1,Empty/1/1/1;1312=ReadUint32/1/2/1,ReadUint32/2/2/2,readUint8LengthPrefixed/1/1/3,readUint16LengthPrefixed/1/1/4,ReadUint16LengthPrefixed/1/1/5,Empty/1/1/6;1320=ReadUint16LengthPrefixed/1/1/1;1420=ReadUint8LengthPrefixed/1/1/1,Empty/1/2/1,ReadUint16LengthPrefixed/1/1/2,Empty/2/2/3;1428=ReadUint16LengthPrefixed/1/1/1;1646=ReadUint8LengthPrefixed/1/1/1,Empty/1/2/1,unmarshalCertificate/1/1/2,Empty/2/2/3;1668=ReadUint16LengthPrefixed/1/1/1;1675=ReadUint16LengthPrefixed/1/1/1;1686=readUint24LengthPrefixed/1/1/1;1767=ReadUint8/1/1/1,readUint24LengthPrefixed/1/1/2,Empty/1/1/3;1831=readUint24LengthPrefixed/1/1/1,Empty/1/1/2")]
[assembly: GoPositionMap("crypto/tls/handshake_server.go", "handshake_server.cs", "ACxUsoKCpoLcpsqmopSAgsiCgIKklICCpICCpICCpICCpICCpIKAgtqAgqSAgqSAgqSAgqSCgoCCpICCpICCyIKU6NaCgpSCgoKqooKCgoKCgqaCgriCgoKCgoKCgqaUgu6CgpSCgoKUgoIACBKCgqaCgqbmgpSClJSCgoK4goKmgpSCgoKUlJSCgoKmgoKmgoKCgqaCgoKUgpSCgoKUlJSCpoKCgqbcpoCClLS0tIK2gIKUtILIrLKClJSUgoKCgpSCAAcQgpTqooKCgoK4goKCgpTugoKCpuailJSCgoKmlIKClIKCpoKUgoKUuKaigoKUgoKUgpSClIKU6LKUgqaCgoKClIKUlIKClIKClNyCgriCppSCgoKmgriUgqaCgoKUgpSClMqEuIKUppSClJSmgoKCgoKCgoKCpqKUgqa4goKCgIKkgIK2goKCuJTmopSCpoKmgpSCppSClIKmgoKCpoKCgoK4goKCgpSCgoKUgriClIK4goIABxCClIK4goKmgqaUgoLKgoKCgqamlIKmgoKmgoKCyoKCgqaCgoKUgoKmgoKClJSmgoIACBK4goKUgoKCpoKCgoKClIKClIKClIKClIKCpoKCgpSCgoK4lIK4lKaClJaSpIKCgoKUgqaClOailILcgoKUgoKCpoKEgqaCpoKmooKmgpSCgqaUgoKCgqaCgpSCgriAgramopSAgraCgoCCtpTqwoKCgoKCgpSCgoKCyoKClJSmgu6CpoKCgoKClJSmgoKCuIKClILmgriCgoK4poKCgqY=", "", "", "587=supportedCipherSuites/1/1/1;604=time/1/1/1;609=anyValidVerifiedChain/1/1/1;830=Sum/1/1/1;919=keysFromMasterSecret/1/1/1;969=ConstantTimeCompare/1/1/1;1085=time/1/1/2,NewCertPool/1/1/3")]
[assembly: GoPositionMap("crypto/tls/handshake_server_tls13.go", "handshake_server_tls13.cs", "AEaEAaKmgIKkgIKkgIKkgoCCpICCpICC2oCCpICCpICCtpQAFgailLiClIKCAAsYgqaCgpS4hIKmgoKCpoKCpoKCgpTugqaClIKClIKUgoKCpoKCpoKCAAkUgoaCgpSCgoKmlKaWlIKygoKmgoKClJSUgoKClIKCgqaCgoKUlJSCgoK4goKUlIKCuIIACgaClIKmgoKCgqaCpoKClIKmgoKmgoKCgoKUgqaCgpSCgoK4gqaCgqaCgtyCgoKUgpSClMqEpoKCuJKUgoKClIKCgpSCgoKCpoaUgoKUgoK4goKCgpSCgoKmAAgQ0oKCqIqClIKClIKCgpSClKaCpoK4gqaCgoKUlJSCpoKUlKqigpSClJQAGAaCyoKCyoKUgoKClAAJFJSCgoKUkoKCgpSCpoKmgriCgqaCgoKmgoKCpoKCgqaCgqaCgoKmgoKCpoKCgqa4goKUlIKCpoKCpoKCpoKCrLKMlIKCpoKCpoKCpoKCpoKCpoKCpgACIIKUgoKCgoKmkoKCgpSCpoKmgqaCpoKCpJSCgoKCpoKCgriCgoKUgoKCpoKUgoKClIKmgsqCgoKCgqaCgoKCuIKmpoKmgqaCppSCgoKCgoKmgriUgoKUgqaCgpSSgqaCgoKUgoKChJSUlJSCpqaClLiCypSCgpSCpoKCgpSCgoKmyoKCuKaCgriCuKaClIKmgqaUgpTmooKCuKaUgoKCgoKCgoKmgoKClIKCpsqCgIKklJSmgIK2poKUpoKCgqbcgoKmgoKCpoKmgoKCuLiCgqaCgoLchIKUgoKUgpSClIKUlILcgqbmgqaCgqaCgoKmgoKmgqY=", "210-212:1;217-224:2;225-227:3;228-230:4", "389=crypto.Hash.New;560=crypto.Hash.New;724=crypto.Hash.New;748=crypto.Hash.New;978=crypto.Hash.New", "427=time/1/1/1;432=anyValidVerifiedChain/1/1/1;768=Equal/1/8/1,Equal/2/8/2,Equal/3/8/3,Equal/4/8/6,Equal/5/8/8,Equal/6/8/10,Equal/7/8/12,Equal/8/8/13;953=finishedHash/1/1/1;1044=Size/1/1/1;1155=signatureSchemesForPublicKey/1/1/1,isSupportedSignatureAlgorithm/2/2/1")]
[assembly: GoPositionMap("crypto/tls/key_agreement.go", "key_agreement.cs", "ACdOguyC5sKClIKClJSCgqaCggAHEOaC5sKCgoKCgqaCgpSCgpSCgoKCgqiSgoKUqqKCgoKUgoKqwoKUABou0oKCgriClIKmgoKUpoKCgoKCgpSCgqaCgoKClJKClIKClIKClIKClIKCpoKClIKClIKCuIKCgpSCgoKCgoKUgoKUpqKCpoKClIKCpgAIBuKClIKUlIKClIKUgoKUgoKCgqaCgpSUgqaCpoKClJSCgpSCgqaCgoKClIKCgoKUgoKUgoKUgpSCgqaCgpSClIKCuOaCgqbqooKCpqaClKSkpKQ=")]
[assembly: GoPositionMap("crypto/tls/key_schedule.go", "key_schedule.cs", "ABcw8vjSkpLs0pKSgqrCggAbNoKCpIKkgqSCpJSkpKSkyMjIAAgSgoKClKaCgoKUgoKUgoKUpqKCgpSCgpQADB6iqoKUgpSCgpSCgpS4gpSU5qKClJKCgpSClKqSlIKUgoKUgoKCgpSClILmsoKUkoKClIKUmJKUgpSCgpSCgpSU", "49-51:1;77-79:1;80-82:2;83-85:3;86-88:4;174-176:1;217-219:1;256-258:1", "25=crypto.Hash.New;30=crypto.Hash.New;31=crypto.Hash.New;39=crypto.Hash.New;40=crypto.Hash.New")]
[assembly: GoPositionMap("crypto/tls/prf.go", "prf.cs", "ABYuwoKCqJKCgpSCgoKCgoKClIKCupKCgpSCgpSCgoKUgqaokgAQIIKUpIKUpMiCgqqigoKUqqKCpqSsAAoCgoKUgoKCgoKCgoKCgoKCgqaCgoKmgoKmABAogoKUgoKmgqamgoKmgoKqoqqiqqKCpqqi7LLssqiSsqamgoKUlIKUgoKUgqY=", "74-76:1;256-281:1", "126=crypto.Hash.New")]
[assembly: GoPositionMap("crypto/tls/quic.go", "quic.cs", "ACAwgpSkpKSkAIIBrALiruKmosqCAAoS4oKUgoKUgoKUqqKCppSCgoKUgoKUgoKClIKCgpSCgoKokoKUgoKm6uKCgpSCgoKUpoKSgoKCgoKCgpSClIKmgpQAExiygoKUgpSClIKUgu7CgoKUgoKUgoKokq7CgpSCgoK8ooKUgpSCgrimooKCpqbqgoKU3KaCAAcQgoKClIK4lKaiuIKCgqamgtyC3KKCuIKCpqaCyoIABBQADAaCuLiClJSCgg==", "", "", "", "233:21")]
[assembly: GoPositionMap("crypto/tls/ticket.go", "ticket.cs", "AGreAQAIAoKCgpSUgoKClIKCkriClJSClJTKgoKkgoKUgpLcgoKmgoKCppSmgoKClAAKCJKCgqKCggAAFJSCgoKUlJS0tJSUtLSUlLS0lIKCgpSUgpSCgoKClIKCgpSCgpSCgoKClIKClJSUgoKClJSCgoK4gqaqogAOIsKCgoKU5oKCpoKCgoKUgpSCgoKUlIKClK7igoKClIKClKaCgqaCgoKUooKClIKmgoKUgpSmAAgc8oKUAAIQ8oI=", "121-123:1;124-130:2;126-128:2.1;146-161:3;148-159:3.1;155-157:3.1.1;163-165:4", "", "146=certificatesToBytesSlice/1/1/1;210=ReadUint8/1/3/1,ReadUint16/2/2/2,readUint64/1/1/3,readUint8LengthPrefixed/1/1/4,ReadUint24LengthPrefixed/1/1/5,ReadUint8/2/3/6,ReadUint8/3/3/7,unmarshalCertificate/1/1/9;319=time/1/1/3,Unix/1/1/3")]
[assembly: GoPositionMap("crypto/tls/tls.go", "tls.cs", "ACxcwriSrsLKkgAQFqKCgpSu4oKCgu70hJSCgpQACQqCggABGAAKAqbSgoKCtoKCgraCgqaCgpSUgriUgoKmgoKClAAFEuIAEDLypoKClAACGAAJAoKUlAACFgAIAoKClIKClKYAFRDilIKCgoKCgpSClLiCgpSClKaCgoKCgoKUgpSUgpTKgoKmgpSmgoKmlIKClILGgoKUgsaCgpSC1qYACAyygIKkgILEtLaAgrY=", "267-267:1", "", "", "168:25,175:25")]
// </GoSourcePositionMaps>

namespace go.crypto;

using static go.crypto.tls_package;
using context = go.context_package;
using cryptobyte = go.vendor.golang.org.x.crypto.cryptobyte_package;
using hash = go.hash_package;
using io = go.io_package;
using net = go.net_package;
using time = go.time_package;
using tls13 = go.crypto.@internal.fips140.tls13_package;
using Δx509 = go.crypto.x509_package;

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
    internal partial struct finishedHash {}
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
    public partial struct ConnectionState {}
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

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(tls_package.AlertError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛAlertError;
[GoMethodHost(typeof(tls_package.Certificate)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛCertificate;
[GoMethodHost(typeof(tls_package.CertificateRequestInfo)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛCertificateRequestInfo;
[GoMethodHost(typeof(tls_package.CertificateVerificationError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛCertificateVerificationError;
[GoMethodHost(typeof(tls_package.ClientAuthType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛClientAuthType;
[GoMethodHost(typeof(tls_package.ClientHelloInfo)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛClientHelloInfo;
[GoMethodHost(typeof(tls_package.ClientSessionState)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛClientSessionState;
[GoMethodHost(typeof(tls_package.Config)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛConfig;
[GoMethodHost(typeof(tls_package.Conn)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛConn;
[GoMethodHost(typeof(tls_package.ConnectionState)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛConnectionState;
[GoMethodHost(typeof(tls_package.CurveID)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛCurveID;
[GoMethodHost(typeof(tls_package.Dialer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛDialer;
[GoMethodHost(typeof(tls_package.ECHRejectionError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛECHRejectionError;
[GoMethodHost(typeof(tls_package.QUICConn)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛQUICConn;
[GoMethodHost(typeof(tls_package.QUICEncryptionLevel)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛQUICEncryptionLevel;
[GoMethodHost(typeof(tls_package.RecordHeaderError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛRecordHeaderError;
[GoMethodHost(typeof(tls_package.SessionState)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛSessionState;
[GoMethodHost(typeof(tls_package.SignatureScheme)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛSignatureScheme;
[GoMethodHost(typeof(tls_package.alert)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛalert;
[GoMethodHost(typeof(tls_package.atLeastReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛatLeastReader;
[GoMethodHost(typeof(tls_package.certificateMsg)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛcertificateMsg;
[GoMethodHost(typeof(tls_package.certificateMsgTLS13)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛcertificateMsgTLS13;
[GoMethodHost(typeof(tls_package.certificateRequestMsg)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛcertificateRequestMsg;
[GoMethodHost(typeof(tls_package.certificateRequestMsgTLS13)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛcertificateRequestMsgTLS13;
[GoMethodHost(typeof(tls_package.certificateStatusMsg)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛcertificateStatusMsg;
[GoMethodHost(typeof(tls_package.certificateVerifyMsg)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛcertificateVerifyMsg;
[GoMethodHost(typeof(tls_package.cipherSuiteTLS13)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛcipherSuiteTLS13;
[GoMethodHost(typeof(tls_package.clientHandshakeState)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛclientHandshakeState;
[GoMethodHost(typeof(tls_package.clientHandshakeStateTLS13)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛclientHandshakeStateTLS13;
[GoMethodHost(typeof(tls_package.clientHelloMsg)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛclientHelloMsg;
[GoMethodHost(typeof(tls_package.clientKeyExchangeMsg)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛclientKeyExchangeMsg;
[GoMethodHost(typeof(tls_package.cthWrapper)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛcthWrapper;
[GoMethodHost(typeof(tls_package.ecdhKeyExchange)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛecdhKeyExchange;
[GoMethodHost(typeof(tls_package.ecdheKeyAgreement)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛecdheKeyAgreement;
[GoMethodHost(typeof(tls_package.echConfigErr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛechConfigErr;
[GoMethodHost(typeof(tls_package.encryptedExtensionsMsg)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛencryptedExtensionsMsg;
[GoMethodHost(typeof(tls_package.endOfEarlyDataMsg)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛendOfEarlyDataMsg;
[GoMethodHost(typeof(tls_package.finishedHash)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛfinishedHash;
[GoMethodHost(typeof(tls_package.finishedMsg)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛfinishedMsg;
[GoMethodHost(typeof(tls_package.halfConn)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛhalfConn;
[GoMethodHost(typeof(tls_package.helloRequestMsg)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛhelloRequestMsg;
[GoMethodHost(typeof(tls_package.hybridKeyExchange)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛhybridKeyExchange;
[GoMethodHost(typeof(tls_package.keyUpdateMsg)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛkeyUpdateMsg;
[GoMethodHost(typeof(tls_package.listener)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛlistener;
[GoMethodHost(typeof(tls_package.lruSessionCache)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛlruSessionCache;
[GoMethodHost(typeof(tls_package.marshalingFunction)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛmarshalingFunction;
[GoMethodHost(typeof(tls_package.newSessionTicketMsg)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛnewSessionTicketMsg;
[GoMethodHost(typeof(tls_package.newSessionTicketMsgTLS13)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛnewSessionTicketMsgTLS13;
[GoMethodHost(typeof(tls_package.permanentError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛpermanentError;
[GoMethodHost(typeof(tls_package.prefixNonceAEAD)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛprefixNonceAEAD;
[GoMethodHost(typeof(tls_package.rsaKeyAgreement)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛrsaKeyAgreement;
[GoMethodHost(typeof(tls_package.serverHandshakeState)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛserverHandshakeState;
[GoMethodHost(typeof(tls_package.serverHandshakeStateTLS13)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛserverHandshakeStateTLS13;
[GoMethodHost(typeof(tls_package.serverHelloDoneMsg)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛserverHelloDoneMsg;
[GoMethodHost(typeof(tls_package.serverHelloMsg)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛserverHelloMsg;
[GoMethodHost(typeof(tls_package.serverKeyExchangeMsg)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛserverKeyExchangeMsg;
[GoMethodHost(typeof(tls_package.timeoutError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛtimeoutError;
[GoMethodHost(typeof(tls_package.weakCertCache)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛweakCertCache;
[GoMethodHost(typeof(tls_package.xorNonceAEAD)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tlsᴛxorNonceAEAD;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class tlsᴛAlertError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this AlertError e) => tls_package.Error⁔(e);
}
partial class tlsᴛCertificate {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Δx509.Certificate>, error) leaf(this ref Certificate c) => tls_package.leaf⁔(ref c);
}
partial class tlsᴛCertificateRequestInfo {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static context.Context Context(this ref CertificateRequestInfo c) => tls_package.Context⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SupportsCertificate(this ref CertificateRequestInfo cri, ж<Certificate> Ꮡc) => tls_package.SupportsCertificate⁔(ref cri, Ꮡc);
}
partial class tlsᴛCertificateVerificationError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref CertificateVerificationError e) => tls_package.Error⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Unwrap(this ref CertificateVerificationError e) => tls_package.Unwrap⁔(ref e);
}
partial class tlsᴛClientAuthType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ClientAuthType i) => tls_package.String⁔(i);
}
partial class tlsᴛClientHelloInfo {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static context.Context Context(this ref ClientHelloInfo c) => tls_package.Context⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SupportsCertificate(this ж<ClientHelloInfo> Ꮡchi, ж<Certificate> Ꮡc) => tls_package.SupportsCertificate⁔(Ꮡchi, Ꮡc);
}
partial class tlsᴛClientSessionState {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte> ticket, ж<SessionState> state, error err) ResumptionState(this ж<ClientSessionState> Ꮡcs) => tls_package.ResumptionState⁔(Ꮡcs);
}
partial class tlsᴛConfig {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ticketKey /*key*/ ticketKeyFromBytes(this ref Config c, /*[32]*/ array<byte> b) => tls_package.ticketKeyFromBytes⁔(ref c, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Config> Clone(this ж<Config> Ꮡc) => tls_package.Clone⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void initLegacySessionTicketKeyRLocked(this ж<Config> Ꮡc) => tls_package.initLegacySessionTicketKeyRLocked⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<ticketKey> ticketKeys(this ж<Config> Ꮡc, ж<Config> ᏑconfigForClient) => tls_package.ticketKeys⁔(Ꮡc, ᏑconfigForClient);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetSessionTicketKeys(this ж<Config> Ꮡc, slice<array<byte>> keys) => tls_package.SetSessionTicketKeys⁔(Ꮡc, keys);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static io.Reader rand(this ref Config c) => tls_package.rand⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static time.Time time(this ref Config c) => tls_package.time⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<uint16> cipherSuites(this ж<Config> Ꮡc, bool aesGCMPreferred) => tls_package.cipherSuites⁔(Ꮡc, aesGCMPreferred);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<uint16> supportedCipherSuites(this ж<Config> Ꮡc) => tls_package.supportedCipherSuites⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<uint16> supportedVersions(this ж<Config> Ꮡc, bool isClient) => tls_package.supportedVersions⁔(Ꮡc, isClient);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint16 maxSupportedVersion(this ж<Config> Ꮡc, bool isClient) => tls_package.maxSupportedVersion⁔(Ꮡc, isClient);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<CurveID> curvePreferences(this ж<Config> Ꮡc, uint16 version) => tls_package.curvePreferences⁔(Ꮡc, version);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool supportsCurve(this ж<Config> Ꮡc, uint16 version, CurveID curve) => tls_package.supportsCurve⁔(Ꮡc, version, curve);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uint16, bool) mutualVersion(this ж<Config> Ꮡc, bool isClient, slice<uint16> peerVersions) => tls_package.mutualVersion⁔(Ꮡc, isClient, peerVersions);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Certificate>, error) getCertificate(this ref Config c, ж<ClientHelloInfo> ᏑclientHello) => tls_package.getCertificate⁔(ref c, ᏑclientHello);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void BuildNameToCertificate(this ref Config c) => tls_package.BuildNameToCertificate⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeKeyLog(this ref Config c, @string label, slice<byte> clientRandom, slice<byte> secret) => tls_package.writeKeyLog⁔(ref c, label, clientRandom, secret);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) EncryptTicket(this ж<Config> Ꮡc, ConnectionState cs, ж<SessionState> Ꮡss) => tls_package.EncryptTicket⁔(Ꮡc, cs, Ꮡss);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) encryptTicket(this ref Config c, slice<byte> state, slice<ticketKey> ticketKeys) => tls_package.encryptTicket⁔(ref c, state, ticketKeys);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<SessionState>, error) DecryptTicket(this ж<Config> Ꮡc, slice<byte> identity, ConnectionState cs) => tls_package.DecryptTicket⁔(Ꮡc, identity, cs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> decryptTicket(this ref Config c, slice<byte> encrypted, slice<ticketKey> ticketKeys) => tls_package.decryptTicket⁔(ref c, encrypted, ticketKeys);
}
partial class tlsᴛConn {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static net.Addr LocalAddr(this ref Conn c) => tls_package.LocalAddr⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static net.Addr RemoteAddr(this ref Conn c) => tls_package.RemoteAddr⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SetDeadline(this ref Conn c, time.Time t) => tls_package.SetDeadline⁔(ref c, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SetReadDeadline(this ref Conn c, time.Time t) => tls_package.SetReadDeadline⁔(ref c, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SetWriteDeadline(this ref Conn c, time.Time t) => tls_package.SetWriteDeadline⁔(ref c, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static net.Conn NetConn(this ref Conn c) => tls_package.NetConn⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static RecordHeaderError /*err*/ newRecordHeaderError(this ref Conn c, net.Conn conn, @string msg) => tls_package.newRecordHeaderError⁔(ref c, conn, msg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readRecord(this ж<Conn> Ꮡc) => tls_package.readRecord⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readChangeCipherSpec(this ж<Conn> Ꮡc) => tls_package.readChangeCipherSpec⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readRecordOrCCS(this ж<Conn> Ꮡc, bool expectChangeCipherSpec) => tls_package.readRecordOrCCS⁔(Ꮡc, expectChangeCipherSpec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error retryReadRecord(this ж<Conn> Ꮡc, bool expectChangeCipherSpec) => tls_package.retryReadRecord⁔(Ꮡc, expectChangeCipherSpec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readFromUntil(this ref Conn c, io.Reader r, nint n) => tls_package.readFromUntil⁔(ref c, r, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error sendAlertLocked(this ж<Conn> Ꮡc, alert err) => tls_package.sendAlertLocked⁔(Ꮡc, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error sendAlert(this ж<Conn> Ꮡc, alert err) => tls_package.sendAlert⁔(Ꮡc, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint maxPayloadSizeForWrite(this ref Conn c, recordType typ) => tls_package.maxPayloadSizeForWrite⁔(ref c, typ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) write(this ref Conn c, slice<byte> data) => tls_package.write⁔(ref c, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) flush(this ref Conn c) => tls_package.flush⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) writeRecordLocked(this ж<Conn> Ꮡc, recordType typ, slice<byte> data) => tls_package.writeRecordLocked⁔(Ꮡc, typ, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) writeHandshakeRecord(this ж<Conn> Ꮡc, handshakeMessage msg, transcriptHash transcript) => tls_package.writeHandshakeRecord⁔(Ꮡc, msg, transcript);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeChangeCipherRecord(this ж<Conn> Ꮡc) => tls_package.writeChangeCipherRecord⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readHandshakeBytes(this ж<Conn> Ꮡc, nint n) => tls_package.readHandshakeBytes⁔(Ꮡc, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (any, error) readHandshake(this ж<Conn> Ꮡc, transcriptHash transcript) => tls_package.readHandshake⁔(Ꮡc, transcript);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (handshakeMessage, error) unmarshalHandshakeMessage(this ж<Conn> Ꮡc, slice<byte> data, transcriptHash transcript) => tls_package.unmarshalHandshakeMessage⁔(Ꮡc, data, transcript);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) Write(this ж<Conn> Ꮡc, slice<byte> b) => tls_package.Write⁔(Ꮡc, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error handleRenegotiation(this ж<Conn> Ꮡc) => tls_package.handleRenegotiation⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error handlePostHandshakeMessage(this ж<Conn> Ꮡc) => tls_package.handlePostHandshakeMessage⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error handleKeyUpdate(this ж<Conn> Ꮡc, ж<keyUpdateMsg> ᏑkeyUpdate) => tls_package.handleKeyUpdate⁔(Ꮡc, ᏑkeyUpdate);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) Read(this ж<Conn> Ꮡc, slice<byte> b) => tls_package.Read⁔(Ꮡc, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this ж<Conn> Ꮡc) => tls_package.Close⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error CloseWrite(this ж<Conn> Ꮡc) => tls_package.CloseWrite⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error closeNotify(this ж<Conn> Ꮡc) => tls_package.closeNotify⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Handshake(this ж<Conn> Ꮡc) => tls_package.Handshake⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error HandshakeContext(this ж<Conn> Ꮡc, context.Context ctx) => tls_package.HandshakeContext⁔(Ꮡc, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error /*ret*/ handshakeContext(this ж<Conn> Ꮡc, context.Context ctx) => tls_package.handshakeContext⁔(Ꮡc, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ConnectionState ConnectionState(this ж<Conn> Ꮡc) => tls_package.ConnectionState⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ConnectionState connectionStateLocked(this ж<Conn> Ꮡc) => tls_package.connectionStateLocked⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> OCSPResponse(this ж<Conn> Ꮡc) => tls_package.OCSPResponse⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error VerifyHostname(this ж<Conn> Ꮡc, @string host) => tls_package.VerifyHostname⁔(Ꮡc, host);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error setReadTrafficSecret(this ж<Conn> Ꮡc, ж<cipherSuiteTLS13> Ꮡsuite, QUICEncryptionLevel level, slice<byte> secret, bool locked) => tls_package.setReadTrafficSecret⁔(Ꮡc, Ꮡsuite, level, secret, locked);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setWriteTrafficSecret(this ref Conn c, ж<cipherSuiteTLS13> Ꮡsuite, QUICEncryptionLevel level, slice<byte> secret) => tls_package.setWriteTrafficSecret⁔(ref c, Ꮡsuite, level, secret);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<clientHelloMsg>, ж<echServerContext>, error) processECHClientHello(this ж<Conn> Ꮡc, ж<clientHelloMsg> Ꮡouter, slice<EncryptedClientHelloKey> echKeys) => tls_package.processECHClientHello⁔(Ꮡc, Ꮡouter, echKeys);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<clientHelloMsg>, ж<keySharePrivateKeys>, ж<echClientContext>, error) makeClientHello(this ж<Conn> Ꮡc) => tls_package.makeClientHello⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error /*err*/ clientHandshake(this ж<Conn> Ꮡc, context.Context ctx) => tls_package.clientHandshake⁔(Ꮡc, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<SessionState> session, ж<tls13.EarlySecret> earlySecret, slice<byte> binderKey, error err) loadSession(this ж<Conn> Ꮡc, ж<clientHelloMsg> Ꮡhello) => tls_package.loadSession⁔(Ꮡc, Ꮡhello);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error pickTLSVersion(this ж<Conn> Ꮡc, ж<serverHelloMsg> ᏑserverHello) => tls_package.pickTLSVersion⁔(Ꮡc, ᏑserverHello);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error verifyServerCertificate(this ж<Conn> Ꮡc, slice<slice<byte>> certificates) => tls_package.verifyServerCertificate⁔(Ꮡc, certificates);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Certificate>, error) getClientCertificate(this ref Conn c, ж<CertificateRequestInfo> Ꮡcri) => tls_package.getClientCertificate⁔(ref c, Ꮡcri);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string clientSessionCacheKey(this ref Conn c) => tls_package.clientSessionCacheKey⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error handleNewSessionTicket(this ж<Conn> Ꮡc, ж<newSessionTicketMsgTLS13> Ꮡmsg) => tls_package.handleNewSessionTicket⁔(Ꮡc, Ꮡmsg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error serverHandshake(this ж<Conn> Ꮡc, context.Context ctx) => tls_package.serverHandshake⁔(Ꮡc, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<clientHelloMsg>, ж<echServerContext>, error) readClientHello(this ж<Conn> Ꮡc, context.Context ctx) => tls_package.readClientHello⁔(Ꮡc, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error processCertsFromClient(this ж<Conn> Ꮡc, Certificate certificate) => tls_package.processCertsFromClient⁔(Ꮡc, certificate);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error sendSessionTicket(this ж<Conn> Ꮡc, bool earlyData, slice<slice<byte>> extra) => tls_package.sendSessionTicket⁔(Ꮡc, earlyData, extra);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error quicReadHandshakeBytes(this ж<Conn> Ꮡc, nint n) => tls_package.quicReadHandshakeBytes⁔(Ꮡc, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error quicSetReadSecret(this ж<Conn> Ꮡc, QUICEncryptionLevel level, uint16 suite, slice<byte> secret) => tls_package.quicSetReadSecret⁔(Ꮡc, level, suite, secret);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void quicSetWriteSecret(this ref Conn c, QUICEncryptionLevel level, uint16 suite, slice<byte> secret) => tls_package.quicSetWriteSecret⁔(ref c, level, suite, secret);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void quicWriteCryptoData(this ref Conn c, QUICEncryptionLevel level, slice<byte> data) => tls_package.quicWriteCryptoData⁔(ref c, level, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error quicResumeSession(this ж<Conn> Ꮡc, ж<SessionState> Ꮡsession) => tls_package.quicResumeSession⁔(Ꮡc, Ꮡsession);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void quicStoreSession(this ref Conn c, ж<SessionState> Ꮡsession) => tls_package.quicStoreSession⁔(ref c, Ꮡsession);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void quicSetTransportParameters(this ref Conn c, slice<byte> @params) => tls_package.quicSetTransportParameters⁔(ref c, @params);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) quicGetTransportParameters(this ж<Conn> Ꮡc) => tls_package.quicGetTransportParameters⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void quicHandshakeComplete(this ref Conn c) => tls_package.quicHandshakeComplete⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void quicRejectedEarlyData(this ref Conn c) => tls_package.quicRejectedEarlyData⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error quicWaitForSignal(this ж<Conn> Ꮡc) => tls_package.quicWaitForSignal⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<SessionState> sessionState(this ref Conn c) => tls_package.sessionState⁔(ref c);
}
partial class tlsᴛConnectionState {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) ExportKeyingMaterial(this ref ConnectionState cs, @string label, slice<byte> context, nint length) => tls_package.ExportKeyingMaterial⁔(ref cs, label, context, length);
}
partial class tlsᴛCurveID {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this CurveID i) => tls_package.String⁔(i);
}
partial class tlsᴛDialer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (net.Conn, error) Dial(this ref Dialer d, @string network, @string addr) => tls_package.Dial⁔(ref d, network, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<net.Dialer> netDialer(this ref Dialer d) => tls_package.netDialer⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (net.Conn, error) DialContext(this ref Dialer d, context.Context ctx, @string network, @string addr) => tls_package.DialContext⁔(ref d, ctx, network, addr);
}
partial class tlsᴛECHRejectionError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref ECHRejectionError e) => tls_package.Error⁔(ref e);
}
partial class tlsᴛQUICConn {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Start(this ж<QUICConn> Ꮡq, context.Context ctx) => tls_package.Start⁔(Ꮡq, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static QUICEvent NextEvent(this ref QUICConn q) => tls_package.NextEvent⁔(ref q);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this ref QUICConn q) => tls_package.Close⁔(ref q);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error HandleData(this ж<QUICConn> Ꮡq, QUICEncryptionLevel level, slice<byte> data) => tls_package.HandleData⁔(Ꮡq, level, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SendSessionTicket(this ref QUICConn q, QUICSessionTicketOptions opts) => tls_package.SendSessionTicket⁔(ref q, opts);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error StoreSession(this ref QUICConn q, ж<SessionState> Ꮡsession) => tls_package.StoreSession⁔(ref q, Ꮡsession);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ConnectionState ConnectionState(this ref QUICConn q) => tls_package.ConnectionState⁔(ref q);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetTransportParameters(this ref QUICConn q, slice<byte> @params) => tls_package.SetTransportParameters⁔(ref q, @params);
}
partial class tlsᴛQUICEncryptionLevel {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this QUICEncryptionLevel l) => tls_package.String⁔(l);
}
partial class tlsᴛRecordHeaderError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this RecordHeaderError e) => tls_package.Error⁔(e);
}
partial class tlsᴛSessionState {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) Bytes(this ж<SessionState> Ꮡs) => tls_package.Bytes⁔(Ꮡs);
}
partial class tlsᴛSignatureScheme {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this SignatureScheme i) => tls_package.String⁔(i);
}
partial class tlsᴛalert {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this alert e) => tls_package.String⁔(e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this alert e) => tls_package.Error⁔(e);
}
partial class tlsᴛatLeastReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this ref atLeastReader r, slice<byte> p) => tls_package.Read⁔(ref r, p);
}
partial class tlsᴛcertificateMsg {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) marshal(this ref certificateMsg m) => tls_package.marshal⁔(ref m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool unmarshal(this ref certificateMsg m, slice<byte> data) => tls_package.unmarshal⁔(ref m, data);
}
partial class tlsᴛcertificateMsgTLS13 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) marshal(this ж<certificateMsgTLS13> Ꮡm) => tls_package.marshal⁔(Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool unmarshal(this ж<certificateMsgTLS13> Ꮡm, slice<byte> data) => tls_package.unmarshal⁔(Ꮡm, data);
}
partial class tlsᴛcertificateRequestMsg {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) marshal(this ref certificateRequestMsg m) => tls_package.marshal⁔(ref m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool unmarshal(this ref certificateRequestMsg m, slice<byte> data) => tls_package.unmarshal⁔(ref m, data);
}
partial class tlsᴛcertificateRequestMsgTLS13 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) marshal(this ж<certificateRequestMsgTLS13> Ꮡm) => tls_package.marshal⁔(Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool unmarshal(this ref certificateRequestMsgTLS13 m, slice<byte> data) => tls_package.unmarshal⁔(ref m, data);
}
partial class tlsᴛcertificateStatusMsg {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) marshal(this ж<certificateStatusMsg> Ꮡm) => tls_package.marshal⁔(Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool unmarshal(this ж<certificateStatusMsg> Ꮡm, slice<byte> data) => tls_package.unmarshal⁔(Ꮡm, data);
}
partial class tlsᴛcertificateVerifyMsg {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) marshal(this ж<certificateVerifyMsg> Ꮡm) => tls_package.marshal⁔(Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool unmarshal(this ж<certificateVerifyMsg> Ꮡm, slice<byte> data) => tls_package.unmarshal⁔(Ꮡm, data);
}
partial class tlsᴛcipherSuiteTLS13 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> nextTrafficSecret(this ж<cipherSuiteTLS13> Ꮡc, slice<byte> trafficSecret) => tls_package.nextTrafficSecret⁔(Ꮡc, trafficSecret);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte> key, slice<byte> iv) trafficKey(this ж<cipherSuiteTLS13> Ꮡc, slice<byte> trafficSecret) => tls_package.trafficKey⁔(Ꮡc, trafficSecret);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> finishedHash(this ж<cipherSuiteTLS13> Ꮡc, slice<byte> baseKey, hash.Hash transcript) => tls_package.finishedHash⁔(Ꮡc, baseKey, transcript);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Func<@string, slice<byte>, nint, (slice<byte>, error)> exportKeyingMaterial(this ref cipherSuiteTLS13 c, ж<tls13.MasterSecret> Ꮡs, hash.Hash transcript) => tls_package.exportKeyingMaterial⁔(ref c, Ꮡs, transcript);
}
partial class tlsᴛclientHandshakeState {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error handshake(this ж<clientHandshakeState> Ꮡhs) => tls_package.handshake⁔(Ꮡhs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error pickCipherSuite(this ref clientHandshakeState hs) => tls_package.pickCipherSuite⁔(ref hs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error doFullHandshake(this ж<clientHandshakeState> Ꮡhs) => tls_package.doFullHandshake⁔(Ꮡhs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error establishKeys(this ref clientHandshakeState hs) => tls_package.establishKeys⁔(ref hs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool serverResumedSession(this ref clientHandshakeState hs) => tls_package.serverResumedSession⁔(ref hs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (bool, error) processServerHello(this ref clientHandshakeState hs) => tls_package.processServerHello⁔(ref hs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readFinished(this ж<clientHandshakeState> Ꮡhs, slice<byte> @out) => tls_package.readFinished⁔(Ꮡhs, @out);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readSessionTicket(this ж<clientHandshakeState> Ꮡhs) => tls_package.readSessionTicket⁔(Ꮡhs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error saveSessionTicket(this ref clientHandshakeState hs) => tls_package.saveSessionTicket⁔(ref hs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error sendFinished(this ж<clientHandshakeState> Ꮡhs, slice<byte> @out) => tls_package.sendFinished⁔(Ꮡhs, @out);
}
partial class tlsᴛclientHandshakeStateTLS13 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error handshake(this ж<clientHandshakeStateTLS13> Ꮡhs) => tls_package.handshake⁔(Ꮡhs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error checkServerHelloOrHRR(this ref clientHandshakeStateTLS13 hs) => tls_package.checkServerHelloOrHRR⁔(ref hs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error sendDummyChangeCipherSpec(this ref clientHandshakeStateTLS13 hs) => tls_package.sendDummyChangeCipherSpec⁔(ref hs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error processHelloRetryRequest(this ref clientHandshakeStateTLS13 hs) => tls_package.processHelloRetryRequest⁔(ref hs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error processServerHello(this ж<clientHandshakeStateTLS13> Ꮡhs) => tls_package.processServerHello⁔(Ꮡhs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error establishHandshakeKeys(this ref clientHandshakeStateTLS13 hs) => tls_package.establishHandshakeKeys⁔(ref hs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readServerParameters(this ref clientHandshakeStateTLS13 hs) => tls_package.readServerParameters⁔(ref hs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readServerCertificate(this ref clientHandshakeStateTLS13 hs) => tls_package.readServerCertificate⁔(ref hs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readServerFinished(this ref clientHandshakeStateTLS13 hs) => tls_package.readServerFinished⁔(ref hs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error sendClientCertificate(this ref clientHandshakeStateTLS13 hs) => tls_package.sendClientCertificate⁔(ref hs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error sendClientFinished(this ref clientHandshakeStateTLS13 hs) => tls_package.sendClientFinished⁔(ref hs);
}
partial class tlsᴛclientHelloMsg {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) marshalMsg(this ж<clientHelloMsg> Ꮡm, bool echInner) => tls_package.marshalMsg⁔(Ꮡm, echInner);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) marshal(this ж<clientHelloMsg> Ꮡm) => tls_package.marshal⁔(Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) marshalWithoutBinders(this ж<clientHelloMsg> Ꮡm) => tls_package.marshalWithoutBinders⁔(Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error updateBinders(this ref clientHelloMsg m, slice<slice<byte>> pskBinders) => tls_package.updateBinders⁔(ref m, pskBinders);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool unmarshal(this ж<clientHelloMsg> Ꮡm, slice<byte> data) => tls_package.unmarshal⁔(Ꮡm, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> originalBytes(this ref clientHelloMsg m) => tls_package.originalBytes⁔(ref m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<clientHelloMsg> clone(this ref clientHelloMsg m) => tls_package.clone⁔(ref m);
}
partial class tlsᴛclientKeyExchangeMsg {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) marshal(this ref clientKeyExchangeMsg m) => tls_package.marshal⁔(ref m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool unmarshal(this ref clientKeyExchangeMsg m, slice<byte> data) => tls_package.unmarshal⁔(ref m, data);
}
partial class tlsᴛcthWrapper {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Size(this ref cthWrapper c) => tls_package.Size⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint BlockSize(this ref cthWrapper c) => tls_package.BlockSize⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Reset(this ref cthWrapper c) => tls_package.Reset⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Write(this ref cthWrapper c, slice<byte> p) => tls_package.Write⁔(ref c, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> Sum(this ref cthWrapper c, slice<byte> b) => tls_package.Sum⁔(ref c, b);
}
partial class tlsᴛecdhKeyExchange {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<keySharePrivateKeys>, slice<keyShare>, error) keyShares(this ref ecdhKeyExchange ke, io.Reader rand) => tls_package.keyShares⁔(ref ke, rand);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, keyShare, error) serverSharedSecret(this ref ecdhKeyExchange ke, io.Reader rand, slice<byte> clientKeyShare) => tls_package.serverSharedSecret⁔(ref ke, rand, clientKeyShare);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) clientSharedSecret(this ref ecdhKeyExchange ke, ж<keySharePrivateKeys> Ꮡpriv, slice<byte> serverKeyShare) => tls_package.clientSharedSecret⁔(ref ke, Ꮡpriv, serverKeyShare);
}
partial class tlsᴛecdheKeyAgreement {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<serverKeyExchangeMsg>, error) generateServerKeyExchange(this ref ecdheKeyAgreement ka, ж<Config> Ꮡconfig, ж<Certificate> Ꮡcert, ж<clientHelloMsg> ᏑclientHello, ж<serverHelloMsg> Ꮡhello) => tls_package.generateServerKeyExchange⁔(ref ka, Ꮡconfig, Ꮡcert, ᏑclientHello, Ꮡhello);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) processClientKeyExchange(this ref ecdheKeyAgreement ka, ж<Config> Ꮡconfig, ж<Certificate> Ꮡcert, ж<clientKeyExchangeMsg> Ꮡckx, uint16 version) => tls_package.processClientKeyExchange⁔(ref ka, Ꮡconfig, Ꮡcert, Ꮡckx, version);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error processServerKeyExchange(this ref ecdheKeyAgreement ka, ж<Config> Ꮡconfig, ж<clientHelloMsg> ᏑclientHello, ж<serverHelloMsg> ᏑserverHello, ж<Δx509.Certificate> Ꮡcert, ж<serverKeyExchangeMsg> Ꮡskx) => tls_package.processServerKeyExchange⁔(ref ka, Ꮡconfig, ᏑclientHello, ᏑserverHello, Ꮡcert, Ꮡskx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, ж<clientKeyExchangeMsg>, error) generateClientKeyExchange(this ref ecdheKeyAgreement ka, ж<Config> Ꮡconfig, ж<clientHelloMsg> ᏑclientHello, ж<Δx509.Certificate> Ꮡcert) => tls_package.generateClientKeyExchange⁔(ref ka, Ꮡconfig, ᏑclientHello, Ꮡcert);
}
partial class tlsᴛechConfigErr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this ref echConfigErr e) => tls_package.Error⁔(ref e);
}
partial class tlsᴛencryptedExtensionsMsg {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) marshal(this ж<encryptedExtensionsMsg> Ꮡm) => tls_package.marshal⁔(Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool unmarshal(this ref encryptedExtensionsMsg m, slice<byte> data) => tls_package.unmarshal⁔(ref m, data);
}
partial class tlsᴛendOfEarlyDataMsg {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) marshal(this ref endOfEarlyDataMsg m) => tls_package.marshal⁔(ref m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool unmarshal(this ref endOfEarlyDataMsg m, slice<byte> data) => tls_package.unmarshal⁔(ref m, data);
}
partial class tlsᴛfinishedHash {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this ref finishedHash h, slice<byte> msg) => tls_package.Write⁔(ref h, msg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> Sum(this finishedHash h) => tls_package.Sum⁔(h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> clientSum(this finishedHash h, slice<byte> masterSecret) => tls_package.clientSum⁔(h, masterSecret);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> serverSum(this finishedHash h, slice<byte> masterSecret) => tls_package.serverSum⁔(h, masterSecret);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> hashForClientCertificate(this finishedHash h, uint8 sigType) => tls_package.hashForClientCertificate⁔(h, sigType);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void discardHandshakeBuffer(this ref finishedHash h) => tls_package.discardHandshakeBuffer⁔(ref h);
}
partial class tlsᴛfinishedMsg {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) marshal(this ж<finishedMsg> Ꮡm) => tls_package.marshal⁔(Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool unmarshal(this ж<finishedMsg> Ꮡm, slice<byte> data) => tls_package.unmarshal⁔(Ꮡm, data);
}
partial class tlsᴛhalfConn {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error setErrorLocked(this ref halfConn hc, error err) => tls_package.setErrorLocked⁔(ref hc, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void prepareCipherSpec(this ref halfConn hc, uint16 version, any cipher, hash.Hash mac) => tls_package.prepareCipherSpec⁔(ref hc, version, cipher, mac);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error changeCipherSpec(this ref halfConn hc) => tls_package.changeCipherSpec⁔(ref hc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setTrafficSecret(this ref halfConn hc, ж<cipherSuiteTLS13> Ꮡsuite, QUICEncryptionLevel level, slice<byte> secret) => tls_package.setTrafficSecret⁔(ref hc, Ꮡsuite, level, secret);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void incSeq(this ref halfConn hc) => tls_package.incSeq⁔(ref hc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint explicitNonceLen(this ref halfConn hc) => tls_package.explicitNonceLen⁔(ref hc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, recordType, error) decrypt(this ref halfConn hc, slice<byte> record) => tls_package.decrypt⁔(ref hc, record);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) encrypt(this ref halfConn hc, slice<byte> record, slice<byte> payload, io.Reader rand) => tls_package.encrypt⁔(ref hc, record, payload, rand);
}
partial class tlsᴛhelloRequestMsg {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) marshal(this ref helloRequestMsg _) => tls_package.marshal⁔(ref _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool unmarshal(this ref helloRequestMsg _, slice<byte> data) => tls_package.unmarshal⁔(ref _, data);
}
partial class tlsᴛhybridKeyExchange {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<keySharePrivateKeys>, slice<keyShare>, error) keyShares(this ж<hybridKeyExchange> Ꮡke, io.Reader rand) => tls_package.keyShares⁔(Ꮡke, rand);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, keyShare, error) serverSharedSecret(this ж<hybridKeyExchange> Ꮡke, io.Reader rand, slice<byte> clientKeyShare) => tls_package.serverSharedSecret⁔(Ꮡke, rand, clientKeyShare);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) clientSharedSecret(this ж<hybridKeyExchange> Ꮡke, ж<keySharePrivateKeys> Ꮡpriv, slice<byte> serverKeyShare) => tls_package.clientSharedSecret⁔(Ꮡke, Ꮡpriv, serverKeyShare);
}
partial class tlsᴛkeyUpdateMsg {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) marshal(this ж<keyUpdateMsg> Ꮡm) => tls_package.marshal⁔(Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool unmarshal(this ref keyUpdateMsg m, slice<byte> data) => tls_package.unmarshal⁔(ref m, data);
}
partial class tlsᴛlistener {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static net.Addr Addr(this listener recvᴛ) => tls_package.Addr⁔(recvᴛ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this listener recvᴛ) => tls_package.Close⁔(recvᴛ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (net.Conn, error) Accept(this ref listener l) => tls_package.Accept⁔(ref l);
}
partial class tlsᴛlruSessionCache {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Put(this ж<lruSessionCache> Ꮡc, @string sessionKey, ж<ClientSessionState> Ꮡcs) => tls_package.Put⁔(Ꮡc, sessionKey, Ꮡcs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<ClientSessionState>, bool) Get(this ж<lruSessionCache> Ꮡc, @string sessionKey) => tls_package.Get⁔(Ꮡc, sessionKey);
}
partial class tlsᴛmarshalingFunction {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Marshal(this marshalingFunction f, ж<cryptobyte.Builder> Ꮡb) => tls_package.Marshal⁔(f, Ꮡb);
}
partial class tlsᴛnewSessionTicketMsg {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) marshal(this ref newSessionTicketMsg m) => tls_package.marshal⁔(ref m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool unmarshal(this ref newSessionTicketMsg m, slice<byte> data) => tls_package.unmarshal⁔(ref m, data);
}
partial class tlsᴛnewSessionTicketMsgTLS13 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) marshal(this ж<newSessionTicketMsgTLS13> Ꮡm) => tls_package.marshal⁔(Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool unmarshal(this ж<newSessionTicketMsgTLS13> Ꮡm, slice<byte> data) => tls_package.unmarshal⁔(Ꮡm, data);
}
partial class tlsᴛpermanentError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this ref permanentError e) => tls_package.Error⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Unwrap(this ref permanentError e) => tls_package.Unwrap⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Timeout(this ref permanentError e) => tls_package.Timeout⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Temporary(this ref permanentError e) => tls_package.Temporary⁔(ref e);
}
partial class tlsᴛprefixNonceAEAD {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint NonceSize(this ref prefixNonceAEAD f) => tls_package.NonceSize⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Overhead(this ref prefixNonceAEAD f) => tls_package.Overhead⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint explicitNonceLen(this ref prefixNonceAEAD f) => tls_package.explicitNonceLen⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> Seal(this ref prefixNonceAEAD f, slice<byte> @out, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData) => tls_package.Seal⁔(ref f, @out, nonce, plaintext, additionalData);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) Open(this ref prefixNonceAEAD f, slice<byte> @out, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData) => tls_package.Open⁔(ref f, @out, nonce, ciphertext, additionalData);
}
partial class tlsᴛrsaKeyAgreement {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<serverKeyExchangeMsg>, error) generateServerKeyExchange(this rsaKeyAgreement ka, ж<Config> Ꮡconfig, ж<Certificate> Ꮡcert, ж<clientHelloMsg> ᏑclientHello, ж<serverHelloMsg> Ꮡhello) => tls_package.generateServerKeyExchange⁔(ka, Ꮡconfig, Ꮡcert, ᏑclientHello, Ꮡhello);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) processClientKeyExchange(this rsaKeyAgreement ka, ж<Config> Ꮡconfig, ж<Certificate> Ꮡcert, ж<clientKeyExchangeMsg> Ꮡckx, uint16 version) => tls_package.processClientKeyExchange⁔(ka, Ꮡconfig, Ꮡcert, Ꮡckx, version);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error processServerKeyExchange(this rsaKeyAgreement ka, ж<Config> Ꮡconfig, ж<clientHelloMsg> ᏑclientHello, ж<serverHelloMsg> ᏑserverHello, ж<Δx509.Certificate> Ꮡcert, ж<serverKeyExchangeMsg> Ꮡskx) => tls_package.processServerKeyExchange⁔(ka, Ꮡconfig, ᏑclientHello, ᏑserverHello, Ꮡcert, Ꮡskx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, ж<clientKeyExchangeMsg>, error) generateClientKeyExchange(this rsaKeyAgreement ka, ж<Config> Ꮡconfig, ж<clientHelloMsg> ᏑclientHello, ж<Δx509.Certificate> Ꮡcert) => tls_package.generateClientKeyExchange⁔(ka, Ꮡconfig, ᏑclientHello, Ꮡcert);
}
partial class tlsᴛserverHandshakeState {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error handshake(this ж<serverHandshakeState> Ꮡhs) => tls_package.handshake⁔(Ꮡhs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error processClientHello(this ref serverHandshakeState hs) => tls_package.processClientHello⁔(ref hs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error pickCipherSuite(this ж<serverHandshakeState> Ꮡhs) => tls_package.pickCipherSuite⁔(Ꮡhs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool cipherSuiteOk(this ref serverHandshakeState hs, ж<cipherSuite> Ꮡc) => tls_package.cipherSuiteOk⁔(ref hs, Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error checkForResumption(this ж<serverHandshakeState> Ꮡhs) => tls_package.checkForResumption⁔(Ꮡhs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error doResumeHandshake(this ж<serverHandshakeState> Ꮡhs) => tls_package.doResumeHandshake⁔(Ꮡhs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error doFullHandshake(this ж<serverHandshakeState> Ꮡhs) => tls_package.doFullHandshake⁔(Ꮡhs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error establishKeys(this ref serverHandshakeState hs) => tls_package.establishKeys⁔(ref hs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readFinished(this ж<serverHandshakeState> Ꮡhs, slice<byte> @out) => tls_package.readFinished⁔(Ꮡhs, @out);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error sendSessionTicket(this ж<serverHandshakeState> Ꮡhs) => tls_package.sendSessionTicket⁔(Ꮡhs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error sendFinished(this ж<serverHandshakeState> Ꮡhs, slice<byte> @out) => tls_package.sendFinished⁔(Ꮡhs, @out);
}
partial class tlsᴛserverHandshakeStateTLS13 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error handshake(this ж<serverHandshakeStateTLS13> Ꮡhs) => tls_package.handshake⁔(Ꮡhs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error processClientHello(this ж<serverHandshakeStateTLS13> Ꮡhs) => tls_package.processClientHello⁔(Ꮡhs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error checkForResumption(this ref serverHandshakeStateTLS13 hs) => tls_package.checkForResumption⁔(ref hs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error pickCertificate(this ref serverHandshakeStateTLS13 hs) => tls_package.pickCertificate⁔(ref hs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error sendDummyChangeCipherSpec(this ref serverHandshakeStateTLS13 hs) => tls_package.sendDummyChangeCipherSpec⁔(ref hs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<keyShare>, error) doHelloRetryRequest(this ref serverHandshakeStateTLS13 hs, CurveID selectedGroup) => tls_package.doHelloRetryRequest⁔(ref hs, selectedGroup);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error sendServerParameters(this ref serverHandshakeStateTLS13 hs) => tls_package.sendServerParameters⁔(ref hs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool requestClientCert(this ref serverHandshakeStateTLS13 hs) => tls_package.requestClientCert⁔(ref hs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error sendServerCertificate(this ref serverHandshakeStateTLS13 hs) => tls_package.sendServerCertificate⁔(ref hs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error sendServerFinished(this ref serverHandshakeStateTLS13 hs) => tls_package.sendServerFinished⁔(ref hs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool shouldSendSessionTickets(this ref serverHandshakeStateTLS13 hs) => tls_package.shouldSendSessionTickets⁔(ref hs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error sendSessionTickets(this ref serverHandshakeStateTLS13 hs) => tls_package.sendSessionTickets⁔(ref hs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readClientCertificate(this ref serverHandshakeStateTLS13 hs) => tls_package.readClientCertificate⁔(ref hs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readClientFinished(this ref serverHandshakeStateTLS13 hs) => tls_package.readClientFinished⁔(ref hs);
}
partial class tlsᴛserverHelloDoneMsg {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) marshal(this ref serverHelloDoneMsg m) => tls_package.marshal⁔(ref m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool unmarshal(this ref serverHelloDoneMsg m, slice<byte> data) => tls_package.unmarshal⁔(ref m, data);
}
partial class tlsᴛserverHelloMsg {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) marshal(this ж<serverHelloMsg> Ꮡm) => tls_package.marshal⁔(Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool unmarshal(this ж<serverHelloMsg> Ꮡm, slice<byte> data) => tls_package.unmarshal⁔(Ꮡm, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> originalBytes(this ref serverHelloMsg m) => tls_package.originalBytes⁔(ref m);
}
partial class tlsᴛserverKeyExchangeMsg {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) marshal(this ref serverKeyExchangeMsg m) => tls_package.marshal⁔(ref m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool unmarshal(this ref serverKeyExchangeMsg m, slice<byte> data) => tls_package.unmarshal⁔(ref m, data);
}
partial class tlsᴛtimeoutError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this timeoutError _) => tls_package.Error⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Timeout(this timeoutError _) => tls_package.Timeout⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Temporary(this timeoutError _) => tls_package.Temporary⁔(_);
}
partial class tlsᴛweakCertCache {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Δx509.Certificate>, error) newCert(this ж<weakCertCache> Ꮡwcc, slice<byte> der) => tls_package.newCert⁔(Ꮡwcc, der);
}
partial class tlsᴛxorNonceAEAD {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint NonceSize(this ref xorNonceAEAD f) => tls_package.NonceSize⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Overhead(this ref xorNonceAEAD f) => tls_package.Overhead⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint explicitNonceLen(this ref xorNonceAEAD f) => tls_package.explicitNonceLen⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> Seal(this ref xorNonceAEAD f, slice<byte> @out, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData) => tls_package.Seal⁔(ref f, @out, nonce, plaintext, additionalData);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) Open(this ref xorNonceAEAD f, slice<byte> @out, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData) => tls_package.Open⁔(ref f, @out, nonce, ciphertext, additionalData);
}
// </MethodForwarders>
