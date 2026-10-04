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
using net = go.net_package;
using Δx509 = go.crypto.x509_package;
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
[assembly: GoDynamicTypeLift("7374727563747b736368656d652063727970746f2f746c732e5369676e6174757265536368656d653b206d696e4d6f64756c7573427974657320696e743b206d617856657273696f6e2075696e7431367d", "rsaSignatureSchemesᴛ1")]
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
[assembly: GoImplement<ecdheKeyAgreement, keyAgreement>(Pointer = true)]
[assembly: GoImplement<encryptedExtensionsMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<endOfEarlyDataMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<finishedMsg, handshakeMessage>(Pointer = true)]
[assembly: GoImplement<go.crypto.@internal.fips140.aes.gcm_package.GCMForTLS12, go.crypto.cipher_package.AEAD>(Pointer = true)]
[assembly: GoImplement<go.crypto.@internal.fips140.aes.gcm_package.GCMForTLS13, go.crypto.cipher_package.AEAD>(Pointer = true)]
[assembly: GoImplement<go.crypto.rsa_package.PSSOptions, crypto_package.SignerOpts>(Pointer = true)]
[assembly: GoImplement<hash_package.Hash, go.crypto.@internal.fips140_package.Hash>]
[assembly: GoImplement<hash_package.Hash, transcriptHash>]
[assembly: GoImplement<helloRequestMsg, handshakeMessage>(Pointer = true)]
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
[assembly: GoImplicitConv<Certificate, ж<Certificate>>(Indirect = true)]
[assembly: GoImplicitConv<Config, ж<Config>>(Indirect = true)]
[assembly: GoImplicitConv<QUICConfig, ж<QUICConfig>>(Indirect = true)]
[assembly: GoImplicitConv<SessionState, ж<SessionState>>]
[assembly: GoImplicitConv<alert, AlertError>(Inverted = true, ValueType = "uint8")]
[assembly: GoImplicitConv<clientHelloMsg, ж<clientHelloMsg>>(Indirect = true)]
[assembly: GoImplicitConv<clientKeyExchangeMsg, ж<clientKeyExchangeMsg>>(Indirect = true)]
[assembly: GoImplicitConv<echClientContext, ж<echClientContext>>(Indirect = true)]
[assembly: GoImplicitConv<serverHelloMsg, ж<serverHelloMsg>>(Indirect = true)]
[assembly: GoImplicitConv<serverKeyExchangeMsg, ж<serverKeyExchangeMsg>>(Indirect = true)]
[assembly: GoImplicitConv<Δx509.Certificate, ж<Δx509.Certificate>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("crypto/tls/alert.go", "alert.cs", "AAseggAqYAAkSoKCgpSmgg==")]
[assembly: go.GoPositionMap("crypto/tls/auth.go", "auth.cs", "ABouopSCgpSCxoKClILGgoKUgILWgoKUgoCC1qQABRAAChqigoKCgoKUgoKCgqrSlKSkpKSklKSkpKSkpKyylKSspAATCAAJMuKCgpaCpKbclJSkpKTWgoKCgtjEloKCgoKmlNyygoKUpriCgpSCpqqipLamgoKopPrW5paClg==")]
[assembly: go.GoPositionMap("crypto/tls/cache.go", "cache.cs", "ACpUAAgkAAsCgoKCgqaosq7CgIKmgoKWgoCCpA==", "63-67:1")]
[assembly: go.GoPositionMap("crypto/tls/cipher_suites.go", "cipher_suites.cs", "ACxYgoIAARTyABAu9gAPJrKCgqaCgqYAGEwAGDaigoKCloKCuAAWLABMjAEAFD4AFDAAChwACxjOgoKWtgAIGqKCgIKkgIK2poKCpoKCgpSmgoKClKiSpoKUqqIAFjKAooCigKSCgqaCggAJFICigKKApIKClIKClqaCgpSCgpamgoKUgoKUgoKUgqSCloKCAAIaAAoCgpSCgpSCgpSCpIKWgoKmgoKUgoKWgoIADhyAooCigKKAooCkgoKskoKCgoKCgpSmgqaC3IIABRCigoKmpoKCgqamgoKCpqaCgoKm", "604-606:1")]
[assembly: go.GoPositionMap("crypto/tls/common.go", "common.cs", "ADFespSkpKSkpABtwAGCpoIAMm4ATdgBAAoCAApCopSkAFjoAbIAFDayANICpgbiiLKCgoIABiAADgKClIKCAClOmAAKBoSogoKCgoKAguyktgACFgATBoKCgpSCgoKClJaCgoKUgoKmgqiCgoKUgoKAgqSCgrSCppQABSAADwKCloKiloKCpoKCgpSmgoKClKaCgoKUlIKCmKYADBiUooKCgpSCgqaClIKUgpSUpoKCgpSssoKCgpSUpqKCgpSUgpiClKaCgoKmqqKCgoKCuAALGJjChIKCqIKWlJaCgoCCpIKCgoKAgsqygILKAAkcABIMgoKUgoK6goKClICCAAcQpILKgIKAgrbIgoKUgpSUgpS6goCC7oKogpaCgIKkgpSkpKSkgoKCgqaClLSClPam3IKClIKCpoKmgpSUgpbc0oCCpoKWgqaCgoCCuIKCuAACFPKCgoKCgriClIIAChqCgpaEgoKEAB9AooKUAB5GsoSClAAGEvKChICCgoKUgoKUpoKCgpaCgoKCgoKiqPKChICCgqQACAqCpoKokoKUpoKCgqYACRSCpoIABRLigpaCgoKogpamgoKWgoKopqaUgqSWrsKCgpSGAAgSgoKClIKAgraCgILI", "1130-1132:1;1206-1208:1;1337-1367:1;1354-1362:1.1;1431-1448:2;1767-1769:1")]
[assembly: go.GoPositionMap("crypto/tls/common_string.go", "common_string.cs", "/oaigoKCgoKCgoKCgoIADCCWgpSkpKSkpKSkpIKkxoaigoKCggAGFJaClIKkpKTGhqKCgoKCypSCgpQ=")]
[assembly: go.GoPositionMap("crypto/tls/conn.go", "conn.cs", "AHaKAtKokqyyqqKssqyyABY2gKKAooCigKSCgIKUpKqigoKqooKUgoKCgoKUrNKCgoKCgrqSgoKC3qyygpaUpKaSlLTO4oKWgpSWlIKWgpSCgrqCgoIACRaEgqaCAAkWooKCqIKWgoSEgpS0gpSCgpSEgoKUgoKCloKCgsaCgoKWgoKUAAAQxAAIBoKClIKmgoKCgpSCypaCgoKWgoKCgoIABxKCgpaWgqzigIKUgqSCqqKCloKAgoKAAAkUlICC6oKUgoKCtIKCloKWgoSCgoSmgoLGgoKCgoKCgoKUgpTEuIKCgoQAECKgpKKCgoKmgqaCAAgiABACgpSWgpSEgqiAuIKUgIKkpILMgoKWgoKmlIKCgoKUyoKmgoKClICCgIKkuIKCgpSCqIKWlKiClpqClIKUgpSClJaktMiClpKe0pSClICCyIKYopqmspQAAoMBAAKKAdrCgoKClAAKFoKClIKCgpSClKqigpS4goKosoKWlKSkhIKUlqjCgoIACEIAEAKCloKogoKUtLSGhsT2gqiCgoKWgoKUpoKCgpaCgqaCgpaCgoKCuAAHEOKCgpSCgoCCtpaCkuyCloKCgoCCpoKCgqbIlIKCgoSCgoKUgIKkgpaCgIK43OKChIKClIKW2tKCgoLYsoKUgoCCtqzSgIKkhLi4loKCgpSAgqSCpqKClLS0tIKUxoKUxoKU6rS0tLTYtLS0tNyEgpaClqgAARIACwSCgoKUgqaEgIKmgoSAgqaCloIACRiCgoCCgoKUuIIACQjigpaCgpaCgoKWgpaUpILagraChIKAgqQACArCgpaCgpSCgoKWlKTcgtbigoKWgoKWgoKEgoKClIKUgpaCloKAgqYABRIACwKAgqSmloKEgoCCpIKAgsoABxKEgIK42LSSgoKClIKm7paCgoCCuICCpKaasoKWptKChJSCgpSUAAUgAA0CAAIaAAwG1gAICIKWuJSCgv6CgqKCgJS2sraCtNqChICCpIKWgoSCgriWgpSCloKCuICCtoKCgpTKlIKW6OKCgtaUooKCgoKClIKCgoKCgoKCgoKUpoKkgoKClKaUgqrygoQACgwACAKCgoKUgpSClAAIDAAICIKClIKssg==", "993-1001:1;1541-1547:1;1548-1557:2;1653-1659:1")]
[assembly: go.GoPositionMap("crypto/tls/defaults.go", "defaults.cs", "AA4gmKKClO4ADh6SlIKCABAqAA8gAAkW7rgADBoABxA=", "51-55:1")]
[assembly: go.GoPositionMap("crypto/tls/ech.go", "ech.cs", "ABcugoKUABk6lKKSgoKUgpSClIKCgpSClIKUgpSCgpSCgoKUgpSUgpSCgpSCgoKUgoKClIKUltyygoKClIKUgoKClIKCgpSCgqamgrKAgqSCgoCCpICCpIKUgpSClIK4gqaClJTWgriAgqSAgqSUpqKCgpSEgoKUlISmooKClKaigoKUAAsQgpLIlIKCgpaCgoKElJQACAYAChKSsoKMzIKCqIKCloKCkoKClIKUgpSSgoKChIKUgoKClIKCgoKClIKClIKCgpSCpoKSuIKC3oKClIKCloKWgtyCqIK2qIKWpqKCpoKCgoKCgpCSkJKmooKClIKCyoKCgpSCgpSCgoKUgoKUrsKClIKClIKCgpSCgpSCuAAPGoKmkgAGEvKCgpKCgoKUgoKCgpSUgoKUgoKUgoKUgoKUgoKUgoK6poKCkoKmprKigoKUlpaCloKWgoKCgpSClIKCgpSCgpSWgpQABhKCgoKWhN6mgoKCkoKClIKmgpQ=", "311-369:1;313-315:1.1;316-318:1.2;319-321:1.3;322-368:1.4;357-359:1.4.1;363-365:1.4.2;422-422:1;423-423:2;562-566:1;644-652:1")]
[assembly: go.GoPositionMap("crypto/tls/handshake_client.go", "handshake_client.cs", "AEFeooKCloKCgIKUtoKWgoKUlAAQJIKWgpaCgpSChIKCgriClJaCggAGEIKCgIK4gpSCloKUgpSCpJSWgpSSpoKCgpSCgIKkgoKUgoLcgqaAgqSCgpSogoKClIKUloKCgpSClIKClIKClIK4goKChIKClIKClIKCgoKoABEe4oK6hIKCloKClILugoCC3JSolIKCgs6AgriEgIKmgoKCgoKUgIKkgqiCgpaCgoKWgILcgoKChIKWggAKFpbu5vSClqiEpsyCqIKClIKClJaCgoKCpoKWlIKUgpSUgKakypSCqKaCloKogoK6goKUgoKCgoKmgpaCgrqCgoKC3oK4gpaCgoKAgqamsoKCloKCgpaCgoKEqsKEgoKWzIKWgIKkgIKmgoKCgIKkgIKkgIKkuIKAgoK2gIKkgIK2gIKkgIKkgIKkgIKkgoCCpICCtoCCpoKE1oKAgoKmgoKUgoKWguaihIKClIKCgpaCgpaCqLqCloSCgqimgIIACRKCgqiEgoKCgoKUkpaCgqiCgoKChIKAgoKmgoKogoKCzIKCgoCCuIKCgpSCgIK4goKmpoCCgqaChIKCgpaCgoKCgoKUgoKUgpSCgoKogoKClIKCgpaAgriEpoKEhJKSgpKCkpSCloKCpqYACgiChICCpoKCloKCgoKogoKCgoKCqICCgqSEhIKWgoKWgoKogoKogoKCgoKmgpb6ooKUlJSClIKCptaihICC3IKClIKCgpaChIKWgIKmgtaigpSEgoKWgoKUgoKCloKmgoKUhIKCloKChIKCpqKEgIKmgoKAgqSC7pSCgIKAgoKUxqrCgoKCgoKClIKCgIKCtoKWgoKCgIKCtt6ClIKCgpaCgoLI3oKUgoKCloKCgqi0tIKmgoSCgIKCuIKAgoK4qqLMkoKUpLjclOrY2LqCgoKClJSCxoLapqKClrKAgqSoqqKClIKUrLKCgpSAgqSClIKUpqKCgpSCgg==", "285-297:1", "533=crypto.Hash.New")]
[assembly: go.GoPositionMap("crypto/tls/handshake_client_tls13.go", "handshake_client_tls13.cs", "ADVewqiCgqiCloCCpoSAgqaCgoCCuIKAgqSAgriCgoKCgvyCgoKChIKCloKCpqiAgqaCgIKkgIKkgIKkgIKkgIKkgIKkgIKkgIKkgIKmgoKWhAAKCqKEgoKWgoKWgoKWjoKWgoKWgoKWgoKClIKClIKEqqKClIKUhAALCqK6goKCgoCCpoKCgoKCgoSCgoKWgoKCgoL8goKCgqiAgtiCzIKCloKWgoLMgJKCgpSWgu6AgoKkgoKClIKmgoKClJSChIKCgoCCpoCCyIKogoKWyoKCgIKmgIK2loCCuIKCloKCgpSEgIKmggAKBqKEgoKWgoKWgoKWgoKUhoKWgpaCgpaClIKClIKCloKCgoKCgoL2goSCgoKClJSCgoKUgoKClIKClIKCgoKUlISCgqaEgoKCgIKmgoKAgriCgoKUgoKCloQACgaChIKCloKCgpaAyoKkhIKUgpSUgoKogoKUgpSCgoKUgoKmgoKkgqjmgqi4goCCgraWgoKWgoKEgoKogoKClIKCloKEgILcgoKWgoKCqIKClIKClIKClIKAlIKmgIKm1oK6goKWgoKCloKCgpaAgrqCgoCCpoKCgpSCgoKWhKaChIKWgoCCpJbcgpaEgoKEgIK4gpaChIKmgpaSgpaCgoKUgoKClISAgqamgoSogIKmhIKWgpYACAaygoKWgqiClIKCgqiCgpaCgpaWgoKCgoKCgoKUgoCCpg==", "315-317:1;436-438:1", "93=crypto.Hash.New;94=crypto.Hash.New;267=crypto.Hash.New;268=crypto.Hash.New;511=crypto.Hash.New;884=crypto.Hash.New")]
[assembly: go.GoPositionMap("crypto/tls/handshake_messages.go", "handshake_messages.cs", "ABAogqqikoKUgrqygqrCkoKUgqrCqsKqwgAjSKKClIKCgoKCypSCgoK4lIKCppSCgoK4lIKUlIKUlIKUpIKCpoKCgq7ClIKUgoKCgriUgpSCgoKC3JSClIKCgoLclIKUgoKCgtyUgpSCgoKCgu6UgpSCgoKC3JSClIKCgsqUgpSCgoKygpLulIKUgoKCyoKCkpKCyqSCgoKykpSmgoKS3IKCloKCkoKCgoKmgoKmgpaCkrqmgqzSgoKCloKClIKCgqbaooKUgoKmhKaigpSWloKClIKCgoKClIKUloKWlJaCgpaCgoKChJaClIKElpKClIKCgoaUgpSUlJSC2pKChpS2koKUgoKClMiUyILGkoKUgoKClNqSgpSCgoKU2pKUtraygpSCgoKUyLaygpSCgoKUyJTIkoKUgoKGlMi2osaCgsiSlIKClIKChpSUgoKUgoKElMaCyMaCqKaCpoIAOXaigoKClIKClIKCgoK4goKUgoKCgoLKgoKCgoKS3IKCgqaCgoKCgriCgoKogoKCgriCgoKmgoKCgriCgoKmgoKWgoKWgoKSgoKClIKEgpK6pqKClJqWlJaCgpaCgoKChJaClISUtLSClLS0goKUgoSUtIKClIKChJTGgsaEyqKCpoTYgoLIlMaCksaClLbGgqimggAJFIKCgoKCgoKCgoLKpIKCppSClIKCgsymgoKEgpSWgoKChJaUgoKUgoSUtIKCyLSCksjGgqjagoKCpoLugoKCgoKUqKaChIKUlJS0tKQAChaCgoKCgoKClIKWgoKCgsymooKUgp6WgoKChJaUgsjGgqgAChaCgoKmhIKCgpTcgpSCgoKCgsqCgoKCgsqCgoKCgpIABxKmgoKEkpiWgoKChJaUtLSCgpSCgoKU2IKClIKCgpTYgoKUgoKClMjGgqjugoKCloKCgoKChIKCgoSCgoKCgoKWpoKCloKCloKCgoKUgoKUgoKWgoKCgoKWAAgSgoKCgoSCgpSClJamgpKCkpSSlJSCgpKCkriCgpKSgpIAChaigpSCmJaChKaygoKUgoKChJSCgoKChJSUlpSChsaCgpSCgoSU2saCuO6CgoKCgoKChKaCgpSC7oKCgoKCgqimopSCmJTagoKCpoLugoKCgoKCgoSmgoKUgoKUgu6CgoKClqaCkgALHpSCgoKUhIKWgoKCgoSEgoSCgoKCgoKCgqiCgoKCgoKCgpamgoKWgoKWgoKCloKCloSCgpSCgoKUgpSCgoKCqIKUgoKClIKChIKCgpSChIKWgpYACBKCgoKCgpSCqKailJKUgoKm7pSCgoKCgoKCgoKEpoKCloKCloKCloTcgqaCAAYkAAsCgIKAgoLIgoKUgg==", "27-33:1;109-116:1;110-115:1.1;112-114:1.1.1;121-125:2;122-124:2.1;130-132:3;137-141:4;138-140:4.1;161-163:5;167-169:6;182-186:7;195-201:8;196-200:8.1;210-216:9;211-215:9.1;225-231:10;226-230:10.1;240-248:11;241-247:11.1;243-245:11.1.1;257-263:12;258-262:12.1;272-276:13;273-275:13.1;285-294:14;286-293:14.1;289-291:14.1.1;303-307:15;304-306:15.1;312-318:16;313-317:16.1;323-339:17;324-331:17.1;326-328:17.1.1;332-338:17.2;334-336:17.2.1;348-370:18;351-355:18.1;356-360:18.2;361-363:18.3;366-368:18.4;758-762:1;759-761:1.1;770-776:2;771-775:2.1;772-774:2.1.1;780-788:3;781-787:3.1;783-785:3.1.1;792-794:4;798-803:5;800-802:5.1;807-809:6;814-818:7;815-817:7.1;822-824:8;828-832:9;829-831:9.1;836-838:10;852-866:11;855-857:11.1;862-864:11.2;1013-1044:1;1014-1043:1.1;1017-1023:1.1.1;1018-1022:1.1.1.1;1019-1021:1.1.1.1.1;1028-1030:1.1.2;1039-1041:1.1.3;1124-1130:1;1165-1183:1;1168-1170:1.1;1171-1173:1.2;1175-1182:1.3;1178-1180:1.3.1;1240-1292:1;1245-1291:1.1;1261-1267:1.1.1;1262-1266:1.1.1.1;1271-1277:1.1.2;1272-1276:1.1.2.1;1281-1289:1.1.3;1282-1288:1.1.3.1;1284-1286:1.1.3.1.1;1452-1463:1;1469-1502:1;1471-1473:1.1;1474-1500:1.2;1481-1486:1.2.1;1483-1485:1.2.1.1;1490-1498:1.2.2;1491-1497:1.2.2.1;1493-1495:1.2.2.1.1;1614-1619:1;1616-1618:1.1;1684-1686:1;1841-1848:1;1845-1847:1.1")]
[assembly: go.GoPositionMap("crypto/tls/handshake_server.go", "handshake_server.cs", "AC1WsoKCloLsltqmooSAgriCgIKklICCpICCpICCpICCpICCpIKAgtqAgqSAgqSAgqSAgqSCgoCCpICCpICCuIKE2NaCgpSCgoKaooKCgqiCgoKCgIKCpLaEgoKUgoKClIKCAAcSgoKWgoKW1oKEgoSUgoKCqIKCloKUgoKClJSUgoKCloKCloKCgoKWgoKClIKEgoKClJSUgpaE3JaAgpS0tLSCxoCClLSCyKyygpSUlIKCgoKUggAHEIKUqqKCgoKCqIKCgoLugpbmooSCgpaCgoKCgoK6goKClISCgpSCgpaClIKClKimooKClIKCtraUgpTYsoSCloKCgoKUgpSUgoKUgoKUzIKCqIKWlIKCgqaCqJSCloKCgpSClIKUyoSogpSmloKCgoKCgoKCpqKEgqa4goKCgIKkgIKmgoCCgriE1qKEgpaChIKmlICCpICCpoKCgIKmgoKCgIK4goKCgpSCkpSAgriClIK4goIABhCClICCuIKAgqaAgqaEgoK6goKCgpaApqSCloKCpoKAgoLKgoKCloKCgpSCgqamgIKCAAgSuIKClIKCgpaCgoKCgpSCgqaCgoKogoCCgqaAgriEpoKEhpKUgpKCkpSCloKE1qKEgILcgoKUgoKCloKEgpaAgqaCpqKCloKEgoKmlIKCgoKmgoKUgoKogIKmpqKEgIKmgoKAgqaE2sKCgoKCgIKCpIKCgIKCyoKClJSWgt6CloKCgoKklJSWgoKCqIKChILmgriCgIKCuKaCgoKW")]
[assembly: go.GoPositionMap("crypto/tls/handshake_server_tls13.go", "handshake_server_tls13.cs", "AEmEAaKWgIKkgIKkgIKkgoCCpICCpICC2oCCpICCpICCpoQADgaihKiChIKCAAoYgqaCgpSohIKWgoCCgqaCgpaCgoIACRKCloKEgoKUgpSCgoKmgoKUgoIACBSChoKClIKCgqaUppaEgrKCgqaCgoKUlISCgoKCgoKUlICCgqSCgoKUgoKCgpSCgoKUgoKCgpTK3JaCgoKUhJSCgoK4goKUlIKCqIL2goSCloKCgoKmgpaCgpSCloKCloKCgoKClIKmgoKUgoKCqIKWgoKWgoLMgoKClIKUgpTKhJaCgIK4kpSCgoKUgoKClIKCgoKWhoSCgIKkgoCCuIKCgoKEgoKClgAIDLSKgpSCgpSCgoKUgIKkpoKWgqiCloKCgpSUlIKmgpSEqqKClIKUhAAOBoK6goK6gIKkgoKChAAIFJSCgoCCpPyWgIKmgIK4goKWgoKCloKCgpaCgoKWgoKWgoKCloKCgpaCgoKWqIKClISCgpaCgpaCgpaCgqyyjJSCgqaCgqaCgqaCgqaCgqaCgqYAAiCChIKCgoKAgrb8loCCpoCCpoCCpoKCpISCgoKAgqaCgoCCuIKCgpSCgoKWgoSCgoKUgrqCgoKCqICCpqaCpoKWgpaUgoKCgoKWgIK4hIKChICCpoKChJKCloKCgpSCgoKAlJSklISAgqamgoSogIK6hIKChIKWgoKClIKCgpa6goCCuKaCgqiCqIKCpqaChIKmgIKmhIKU1qKCgriWhIKCgoKCgoKCpoKCgpSCgqa6goCCpISUloCCpqaChKaCgIKCtryCgpaCgoKWgIKmgoCCgri4goKWgoKCqIKClIKClIKClIKAlIKmgILcgIKm1oKWgoKWgoKCloKCloCCpg==", "209-211:1;216-223:2;224-226:3;227-229:4", "427=crypto.Hash.New;591=crypto.Hash.New;592=crypto.Hash.New;754=crypto.Hash.New;755=crypto.Hash.New;777=crypto.Hash.New;999=crypto.Hash.New")]
[assembly: go.GoPositionMap("crypto/tls/key_agreement.go", "key_agreement.cs", "ACVMkuyC1sKClIKClISCgqaCggAHENaC1sKCgoKCgpaCgpSCgpSCgoKCgqiSgoKUqqKCgoKUgoKu4oKCgpSUgoKClIKUgpQAFCTSgoKCgqiClICCpoKClJaCgoKCgoKEgoKWgoKCgoKClIKCpoKCpoKWhIKClIKCloKCgpSCgoKCgoKUgoKEpqKCloKClIKCltbigpSSlISCgpSChIKCloCCpoKClISCgpSCgpaCgoKChIKCgoKCgpaClIKCpoKCpoKWgoKUhIKAgqTWgoKW")]
[assembly: go.GoPositionMap("crypto/tls/key_schedule.go", "key_schedule.cs", "ABUs8ujSkpLc0pKSgqrCggAOHqKCgpamgpSkpKSkyIKUpKSkpA==", "47-49:1", "23=crypto.Hash.New;28=crypto.Hash.New;29=crypto.Hash.New;37=crypto.Hash.New;38=crypto.Hash.New")]
[assembly: go.GoPositionMap("crypto/tls/prf.go", "prf.cs", "ABcuwoKCqJKCgoSCgoKCgoKChIKCupKCgoSCgoSCgoKEgpaokgAPIIKUpIKUpMiCgqqigoKEqqKCpqSsAAoCgoKEgoKCgoKCgoKCgoKCgqaCgoKWgoKWABAogoKEgoKWgpamgoKWgoKqoqqiqqKCloKWgoKCloKWqqLcstyyqJKylraCgpSEgoSCgpSClg==", "74-76:1;270-295:1", "126=crypto.Hash.New")]
[assembly: go.GoPositionMap("crypto/tls/quic.go", "quic.cs", "ABswgpSkpKSkAEyYAuKu4qaiyoIACBLigpSCgpSCgIKkqqKCgKakgoKClIKCgpSCgoKokoKUgqba4oKClIKCgpSmgpKCgoKCgoKClIKUgIK2gpQAEBiygoKUgpSClILewoKClIKClIKCqJKuwoKUgoKCvKKClIKClIKCuKaigoCCtqbqgoKU3KaCAAcQgoKClIK4lKaiuIKCgIK2poLcgtyigriCgIK2poLKggAEFAAMBoK45sq0grSU")]
[assembly: go.GoPositionMap("crypto/tls/ticket.go", "ticket.cs", "AGfYAQAIAoKCgpSUgoKClIKCkriClJSClJTKgoKkgoKUgpLcgoKmgoKCpqaCgoKU6JKCkqKCggAAFpSCgoKUlJS0tKSUtLSkgoKClIKUgoKCgpSCgoKUgoKUgoKCgpSCgpSClJSCgoKUlICCgpSkgoKUgoKUlIKUqqIADiLCgoKClNaCgpaCgoKChICCpIKCgpSEgoKEruKCgoKUgoKUpoKCloKCgoSigoKEgpaCgpSChJYACBzygpQAAhDygg==", "118-120:1;121-127:2;123-125:2.1;143-158:3;145-156:3.1;152-154:3.1.1;160-162:4")]
[assembly: go.GoPositionMap("crypto/tls/tls.go", "tls.cs", "ACNGwriSrsLKkgAQFqKCgpSu4oKCgt70hJSCgpQACAqAooCigAACGAAKAqbSgoKCpoKCgqaCgpaCgpSEgriUgoKWgoCCgqQABRLiABAy8qaCgpQAAhgACQKClJQAAhYACAKCgpSCgpSmAAwQ4oSCgoKCgoKUgpSogoKUgpSWgoKCgoKClIKUlIKUuoKCloKUloKClpSCgpSCxoKClILGgoKUgtaW7LKAgqSAgsS0toCCpg==", "256-256:1")]
// </GoSourcePositionMaps>

namespace go.crypto;

[GoPackage("tls")]
public static partial class tls_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
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
    internal partial interface transcriptHash {}
    internal partial struct activeCert {}
    internal partial struct alert {}
    internal partial struct atLeastReader {}
    internal partial struct cacheEntry {}
    internal partial struct certCache {}
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
    internal partial struct ecdheKeyAgreement {}
    internal partial struct echClientContext {}
    internal partial struct echConfig {}
    internal partial struct echExtType {}
    internal partial struct echServerContext {}
    internal partial struct encryptedExtensionsMsg {}
    internal partial struct endOfEarlyDataMsg {}
    internal partial struct finishedMsg {}
    [GoValueClone("seq", "scratchBuf")] internal partial struct halfConn {}
    internal partial struct helloRequestMsg {}
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
    [GoValueClone("nonceMask")] internal partial struct xorNonceAEAD {}
    internal partial struct ΔfinishedHash {}
    public partial interface ClientSessionCache {}
    public partial struct AlertError {}
    public partial struct Certificate {}
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
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸhmac() => builtin.initPackage(typeof(go.crypto.hmac_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸboring() => builtin.initPackage(typeof(go.crypto.@internal.boring_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸaes() => builtin.initPackage(typeof(go.crypto.@internal.fips140.aes_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸaesꓸgcm() => builtin.initPackage(typeof(go.crypto.@internal.fips140.aes.gcm_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸhkdf() => builtin.initPackage(typeof(go.crypto.@internal.fips140.hkdf_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸmlkem() => builtin.initPackage(typeof(go.crypto.@internal.fips140.mlkem_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸtls12() => builtin.initPackage(typeof(go.crypto.@internal.fips140.tls12_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸtls13() => builtin.initPackage(typeof(go.crypto.@internal.fips140.tls13_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸhpke() => builtin.initPackage(typeof(go.crypto.@internal.hpke_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸmd5() => builtin.initPackage(typeof(go.crypto.md5_package));
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
    // </ImportInitializers>
}
