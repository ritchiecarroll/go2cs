// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static go.net_package;
using static go.net_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("696e746572666163657b46696c65282920282a6f732e46696c652c206572726f72297d", "startTestSocketPeer_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b697341646472696e666f4572726e6f28297d", "parseDialError_type")]
[assembly: GoDynamicTypeLift("7374727563747b4164647220737472696e673b205265766572736520737472696e673b2045727250726566697820737472696e677d", "revAddrTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6469616c2066756e6328737472696e672c20737472696e672920286e65742e436f6e6e2c206572726f72293b20756e726561636861626c654e6574776f726b20737472696e673b206e6574776f726b73205b5d737472696e673b206164647273205b5d737472696e677d", "dialGoogleTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b66696c7465722066756e63286e65742e4950416464722920626f6f6c3b20697073205b5d6e65742e4950416464723b20696e6574616464722066756e63286e65742e49504164647229206e65742e416464723b206669727374206e65742e416464723b207072696d6172696573206e65742e616464724c6973743b2066616c6c6261636b73206e65742e616464724c6973743b20657272206572726f727d", "addrListTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e202a6e65742e49504e65743b206f757420737472696e677d", "ipNetStringTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e206e65742e49503b2061663420626f6f6c3b2061663620626f6f6c7d", "ipAddrFamilyTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e206e65742e49503b206d61736b206e65742e49504d61736b3b206f7574206e65742e49507d", "ipMaskTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e206e65742e49503b2073747220737472696e673b20627974205b5d627974653b206572726f727d", "ipStringTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e206e65742e49504d61736b3b206f757420737472696e677d", "ipMaskStringTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e206e65742e49504e65743b206f7574206e65742e49504e65747d", "networkNumberAndMaskTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206970206e65742e49503b206e6574202a6e65742e49504e65743b20657272206572726f727d", "parseCIDRTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f7574206e65742e4861726477617265416464723b2065727220737472696e677d", "parseMACTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f7574206e65742e49507d", "parseIPTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e697469616c54696d656f75742074696d652e4475726174696f6e3b20696e697469616c44656c74612074696d652e4475726174696f6e7d", "dialTimeoutTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6970206e65742e49503b206e6574202a6e65742e49504e65743b206f6b20626f6f6c7d", "ipNetContainsTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f6f6b757020737472696e673b2072657320737472696e677d", "lookupStaticHostAliasesTestᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20636e616d6520737472696e677d", "lookupCNAMETestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20656e7473205b5d6e65742e737461746963486f7374456e7472797d", "lookupStaticHostTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20686f737420737472696e677d", "lookupGmailMXTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b2074787420737472696e673b20686f737420737472696e677d", "lookupGmailTXTTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e677d", "lookupGoogleHostTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e657420737472696e673b206761646472202a6e65742e554450416464727d", "ipv4MulticastListenerTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e657420737472696e673b206c61646472202a6e65742e4950416464727d", "ipConnLocalNameTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e657420737472696e673b206c61646472202a6e65742e544350416464727d", "tcpListenerNameTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e657420737472696e673b206c61646472202a6e65742e554450416464727d", "udpConnLocalNameTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e6574776f726b20737472696e673b206164647265737320737472696e677d", "prohibitionaryDialArgTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e6574776f726b20737472696e673b2076657273696f6e20627974657d", "ipVersionTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e6574776f726b20737472696e677d", "fileConnTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e6574776f726b3120737472696e673b20616464726573733120737472696e673b206e6574776f726b3220737472696e673b20616464726573733220737472696e673b2078657272206572726f727d", "dualStackTCPListenerTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6f6e657320696e743b206269747320696e743b206f7574206e65742e49504d61736b7d", "cidrMaskTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73636f70652066756e63286e65742e49502920626f6f6c3b20696e206e65742e49503b206f6b20626f6f6c7d", "ipAddrScopeTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7365727669636520737472696e673b20706f727420696e743b206e656564734c6f6f6b757020626f6f6c7d", "parsePortTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7365727669636520737472696e673b2070726f746f20737472696e673b206e616d6520737472696e673b20636e616d6520737472696e673b2074617267657420737472696e677d", "lookupGoogleSRVTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b736e657420737472696e673b20736164647220737472696e673b20746e657420737472696e673b20746164647220737472696e673b206469616c20626f6f6c7d", "udpServerTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b736e657420737472696e673b20736164647220737472696e673b20746e657420737472696e673b20746164647220737472696e677d", "tcpServerTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b74696d656f75742074696d652e4475726174696f6e3b207865727273205b325d6572726f727d", "readTimeoutTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<TestDialerLocalAddr_test, error>(Promoted = true)]
[assembly: GoImplement<TestSpuriousENOTAVAIL_type, error>(Promoted = true)]
[assembly: GoImplement<contextWithNonZeroDeadline, context_package.Context>(Promoted = true)]
[assembly: GoImplement<contextWithNonZeroDeadline, context_package.Context>]
[assembly: GoImplement<ipStringTestsᴛ1, error>(Promoted = true)]
[assembly: GoImplement<localPacketServer, global::go.net_package.PacketConn>(Promoted = true)]
[assembly: GoImplement<localServer, global::go.net_package.Listener>(Promoted = true)]
[assembly: GoImplement<packetListener, global::go.net_package.PacketConn>(Promoted = true)]
[assembly: GoImplement<resolverFuncConn, global::go.net_package.Conn>(Pointer = true)]
[assembly: GoImplement<someaddr, global::go.net_package.ΔAddr>]
[assembly: GoImplement<streamListener, global::go.net_package.Listener>(Promoted = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<routeStats, ж<routeStats>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("net/conn_test.go", "conn_test.cs", "ABIogoKSooKWgpKCkJKAgqSCloKClJKClIKChICCpIKAgqaCtA==", "23-62:1;31-31:1.1")]
[assembly: go.GoPositionMap("net/dial_test.go", "dial_test.cs", "ACAuAAcQooSUpIKWgoKUlIKCloKCgoLGpKKCkoKClIKClKLUopSkpKSCloKCkJKCgoKCgpSmgoKUgIKCpoKCgoKCwoKCgoKUoqSCgoKCtAAJGtKCgpSUpsyCgpSChIKCgpSCggAPBoKChIKCgpSWABc+goKCgoKUlJaCoqKSgoKUgpSAgoKkloKCtoKClILcgoSCloKSgpaCqIKCgpKCgriCgpTKgpbqAA0IooSCloKQkoSCkJKEAAcagoKCgpSmgoKUkoCCpoKEgoKCgpKClIKCgpSCxqTCgpaCgIKClKaCgqSCggAHEoKogoKAgqSClIKClJKAgqaEgoKCkJK4grqClrbMkoKClKiCgpSWogAKBIKCAA8sgoKClILc9KKClgAoZIKCgri6gpCSgoKCgoKUpoKCgpKAgriCgoKCgpSClIKCuIKUpoKAgqSUtKSihIKWhIKQkoKCgoKClKiisoKClJKAgqaCgoKCgpSApLTExvSigqaCgoKClKa2gpKAgqYABRSCgJSCgpKCgpSCgsbkooSCgpaClIKEgoKCgoKigIKUtoKCxIKClILGgrSAgqSm3IKCgqimgIKkAA0KooKWhIKCkoKokoKCgpSCgoKCgpSAgqSUlrKCgrqChIKUlpKAgriCgoKUgpSAgsSmgrTkopSUgqKCyoKCgoKClMiEyoKCgoKClIKClLQABxAACAKCAAsagoKUlIKCggAKGIKCgpSCouSClKSmooKClIKSkoKCgpS0kqKCgpSCgpSSkoKCgpS0toKUpKSCgqKCloKSgpKCpIKClIKUogAFENKCgoKCypTulKaigpKCgpaCgoKClKI=", "91-91:1;93-101:2;116-124:3;182-188:1;221-231:2;235-300:3;236-249:3.1;288-292:3.2;325-325:1;329-329:2;345-353:3;400-425:1;440-440:2;441-453:3;464-470:4;589-589:1;591-599:2;651-651:1;653-661:2;693-695:1;694-694:1.1;697-705:2;726-726:3;761-767:1;822-825:1;828-847:2;849-883:3;897-935:1;996-1011:1;1012-1030:2;1040-1064:1;1042-1062:1.1;1050-1053:1.1.1")]
[assembly: go.GoPositionMap("net/dnsclient_test.go", "dnsclient_test.cs", "AAoWgoKCloSCgoKCgoKWgoKCgoKUgriCgoKUpoKCgoKmgsqmgg==")]
[assembly: go.GoPositionMap("net/dnsname_test.go", "dnsname_test.cs", "ABMiAA8iooKCgpSChIKWgrqUlKKkgoKCgoLKooS4goKC")]
[assembly: go.GoPositionMap("net/error_posix_test.go", "error_posix_test.cs", "ABIeggAJHICC")]
[assembly: go.GoPositionMap("net/error_test.go", "error_test.cs", "ABgsooKUgpSCxoLGgsaCxoLGgsaCxoLWpoKU7LKClpSAgqSCpISCgpQACASkpIK0grSWpISCgpSmABEsopSmgpCShoaEkoKikoKCgoKUgoKAgqSAgqSCpoKUgILopIKUpoKClKSkpKSCgpSAgoKkAAkIopSmgpYAChiCgpSSgoKCgoKUkpSCgoKUgIKkgoKClIKClILcAAwiopSmgpCShoaEspKCgoKClIKCgIKkgIKkgqaClICC6KQAChyilKaCkJKIspKCgoKClIKUgILopIKUpoKClKSkpKSkgoKUgIKCAAUQsoKWlICCpIKkgpSEgoKUlIKklKSEgoKUrLKClpSAgqSCpISCgpQACASkpIKklKSEgoKU3LKCAAYQgoKWgpaUgIKkgqSEgoKUlIK0grSUpISCgpSUtKaCooKSgoKUlIKCgIK2goKAgraCgoCCpIKAgtaUooKWgoKUlIKCgILWvLKClpSAgqSCpISCgpSUgrSCtJSkhIKClKaCgoKCgoCCpIKClICCpJSmgoCCgqaCrLKClpSAgqSCpISCgpSUgrSCtIKklKSEgoKU5qKUpoKClIKUgoKClICCtoKUgoKClICCtoKUgoKClICCtoKWhIKCgoCCtpS0pIKClqSktOaCgpSC", "147-147:1;148-150:2;151-153:3;159-184:4;244-275:1;303-303:1;304-306:2;307-309:3;313-338:4;363-363:1;364-366:2;369-382:3;566-597:1;599-616:2;663-681:1")]
[assembly: go.GoPositionMap("net/error_windows_test.go", "error_windows_test.cs", "AAweloKCprg=")]
[assembly: go.GoPositionMap("net/external_test.go", "external_test.cs", "ABIegoSCloKCgoKUlKSkAA0KABEugoSCloKCgpSCgoKCgIK2zAALGAAJEMKCgpSClJKCgpSClIKmgoKCppSCgoKmlKaigoKUkoKAgqSCgoKUgpQ=")]
[assembly: go.GoPositionMap("net/file_test.go", "file_test.cs", "ABoqAAYSopSmgoKClpKUgpKCpKKCgpSSgqKSgpKAgqSCpoKCgIKklISClLS0pICCgIKkpIKAgqSWgoCCpIKAgqSUlICCgIKkpILGpAAFEKKUpoKCgpaClKSEmJS0pJS0gILGgoCCpJaCgIKkgoCCpJSUgoKygoKCgIKkgpSikoKCgIKklIKCgsakzqKUpoKCgpaClKSEmJS0pICCgIKkpIKAgqSWgoCCpIKAgqSUlICCgIKkpILG1rKUpIKWooKClJKCopSCkoCCpoKCgoKUhIKCsoKCgrSSsoKikrQ=", "50-58:1;189-200:1;299-307:1;325-331:2;332-335:3")]
[assembly: go.GoPositionMap("net/hosts_test.go", "hosts_test.cs", "ABkkACdWooCUgoKCxqSCgoKCgvoALGCigJSCgoLGpIKCgpSC+OaAlIKClIKClISCgpSCgpSiAAsEAA8oooCUgoK0pIKCgoKC", "62-62:1;131-131:1;154-154:1;198-198:1")]
[assembly: go.GoPositionMap("net/interface_test.go", "interface_test.cs", "AA0isoKClLKCpqyygpSCgpSCgIKCyKaCgoKUsoKClJSCtoK2goKUgpS4goKClIKCgpSCgpSAgsiCgoKUgoKUgrKCgpSCgpSClICCyIKCgpSCgoKUgoKUgrKCgpSCgpSClICCAAkSgoKCgoKUuO7ugoKUgpSClIKCgpSSlJSCgpSSlMaClIKUgpSC1qamgoKClIKUgpSClILWpqbEgsqClKbCAAMeAAwCtqaigoSCgIIACAqigoSCgpSCgILaooKEgoKUgoCC2qKChIKAgtqigoSCgpSCgILaooKEgoKUgoCC")]
[assembly: go.GoPositionMap("net/ip_test.go", "ip_test.cs", "ABgeABxIgoKAgqSUlIKAgtqigoKUgoK6goKCgqS4goKogoKCpIKUgtyihIKCyqKEguiihIK6koKCgIK2goKClIKWgoKClIIAEAgATdYBgoKAgqSAgqSCgILalIKSgpSCAAkKgoKCgoKCpoKCgoKCpoKClIKC7oKEgpaCuKKCgoKCggAMDAAIGoKCgIIACgoABxaCgoCC2qKEgoIADAoAHkiCgoKClIIACwoACh6CgoCCAAsKAAcWgoKAggALCgAOJoKCgILcgoKCgoKCgoLmABEqgrKCgoIADwqiACJKgIK4ABI4gIKUgoKUggAMDIIAHkaAggAMCgARLIKCgIKkgIIADAoAQo4BgqaCgoCCpIKClICC2oKClIK4ooKCgqaCgoKmgoKC", "288-291:1;299-306:1;307-314:2;315-323:3;331-333:1;335-337:2;813-815:1;816-818:2")]
[assembly: go.GoPositionMap("net/iprawsock_test.go", "iprawsock_test.cs", "AB00ABs8ooKWgpCShIKCgoKUgoKC2AAIBAAIEqKCgoKUgpSCkoKUkoCC1tSigoKWgoKUkoKUkoK0AA0EogASLoKC3oKClIKUgqiCooKClIKClIKCgpSCgpSCgg==", "62-62:1")]
[assembly: go.GoPositionMap("net/ipsock_test.go", "ipsock_test.cs", "AAsYAAwEAKwBlgOCgpaCgoKUgoKUlIKClIKClIKUgoIADwqCmgAFFMSCkoKUgpSC", "270-273:1")]
[assembly: go.GoPositionMap("net/listen_test.go", "listen_test.cs", "ABQmgoKClKaCgoKUpgAROrKUpoKCgpaCgpSAgoKkgoKUgIKCpLgAETqylKaCgoKWgoKUgIKCpIKClICCgqQAKggAE4YB4pSkgpaigoKWgpSSgoKClIKClIKCgpSAgoKkgoKUgIKCpIKUgpSCygATQuKUpIKWooKCloKUkoKCgpSCgpSCgoKUgIKCpIKClICCgqSClIKUgsqCgpSClKaClIKAgsaCgraCgraCgILGgoK2goK2pKaClIK2grakpoKUgraCtqSmopzCgqaCAAUQwoKmguiU1qKElKaCgIK4gIKkgIKkgIKkgIKkgILEAAgEAAcYsoSUpIKWgoKCutyClIKCgoCCpICCgqSAgoKkgIKCpMoAEiyyhJSkgpSCloKCgrrcgpSCgoKAgqSAgoKkgIKCpICCgqTKgoCCkoKkgoCCpKaClKSCtoKClLKCgpSCgriosoKEkoKCgpTehISCgpTmgpSmgoKClIKmgoKClII=", "495-499:1;541-547:1;617-623:1;702-710:1;732-740:1;741-749:2")]
[assembly: go.GoPositionMap("net/lookup_test.go", "lookup_test.cs", "ABwupIKmgpTaABAWABIw5IKChIKWgpaCgoKCgoKCgoKCgoKUlIKUgpSCggAJDM6CgoSCloKWgoKCgoKCgoKCgoKClJSClIKC3M6CgoSCloKWgoKCgoKCgoKCgoKClJSClIKCAAkM/oKClIKEgpaCloKCkoKCgoKCgoKCgpSUgpaWygAKDqKEgpaEgoKClIKUgoLY5KKCloSCgpSClICCxNSCgpaCgpSCgJK4goKUgoCSAAkKAAcUooKEgpaEgoKCgoKCgoKCgoKClJSCxuTOooKEgpaEgoKClIKUgoLY1KKChISCgpSCuIK0pM6igoSCloSCgoKUgpSCgtgACAQADSSigoKCgoKUgpSClILGAAwEooKWhIIABxCWgoKSgpSCgoKigpSigqaKgoK0tJS0goKUgpSC1oIADBii5KKCloSEgoKClIKCgoKUgoKUgoKSgoKUpqaSkvjUooKUgoSCloKWhICCgqSAgoLE9IKCgpSCgoK6goKSgpaCgpSCgoK6goKUgoKCuoKClIKUgoKC3IKCgoKCgpSCpoKCgoKCgpSCpoKCgoKCgpSCAA4GrAASOJSCtqaCgoKUgoCCAAwQwgAJHoKCggAJCqIACBqCgoL8ooSAgv6CgpSClIK0pMKChIKSgpaCuqwABRCCgvKCgoK0kqaC3ICCgqSCloKAgqSiAAkIwoKCloKCgoKCgoKCggAICsKCgoSsgoKWgoKCgoIACBaWgoIACRSCgoKCgrzCmMaChISCgpKCloKCgqKCgoK0pISCloKCxvQADSKCgoCCAA0QwoKQlM6Cgpa6ooKCgqaUhLqCgoKUgsamsoKQlO4ACBiSgtiClOaUloKSkoKCgoLCgoKClIKCtKSi1pKCgpSC6IKWkqiWgJK4loCCyrKCggANBsKEgoSE7rKCgpSUgoKUtLiSgoKCloKCgoCCgpSmyoKCpoKCuoKUgtjE9tKCkJKEkoKClISCgoKSgIKSgpKAgtqCgoaigoKigqKSooKikoKCgpaCgoKygqKSsoKikoKCgoKCogAIBIKCloSEooKilKKCouaigqaCgoKWgoKCgoKWgoKWgqiCgoKCgoKWguiCgoKCgv6EgpSmgoKCgoLKgoKCgsqCgoKCABMKgoSChL6C3ILcgtyC3ILcgtyC3ILcgtyCurKSkoKCgoKClICCgoKClIKClIK2goKCgoKClIIACw6ilJSWmIKCloKCloKCgoI=", "250-252:1;529-532:1;533-536:2;881-884:1;892-931:2;908-914:2.1;1010-1015:1;1043-1049:1;1094-1094:1;1113-1121:2;1143-1143:1;1166-1179:2;1188-1198:3;1261-1320:1;1269-1318:1.1;1327-1327:1;1331-1334:2;1337-1348:3;1359-1362:4;1363-1366:5;1376-1379:6;1380-1383:7;1401-1404:1;1406-1409:2;1456-1462:1;1476-1482:1;1486-1491:1;1495-1500:1;1515-1518:1;1522-1525:2;1529-1532:3;1536-1539:4;1543-1546:5;1550-1553:6;1557-1560:7;1564-1567:8;1571-1574:9;1578-1581:10;1586-1621:11;1587-1620:11.1;1630-1632:1;1633-1635:2")]
[assembly: go.GoPositionMap("net/lookup_windows_test.go", "lookup_windows_test.cs", "ABkukpSCgtaCgoKCgoKClJSCgoKClAAIDIKEgoKClIKUgoKUgoCCpJSCgoLKgoSCgoKUgpSCgpSCyoKEgoKClIKUgoKUhoKCgsqChIKCgpSClIKClIKCgsqChIKClIKClIKUgoKUgoKCuIKEgoKKwoKUlIKUgoKClIKCggAICoKCgoKCgoCCpKaClOaygoCCpKaCgoK4goKClNaygoCCpJSCgpTmsoKAgraUgoKU5rKCgIKkAAYQgoKU1oKCgoKUgtaygoCCpIKCgpTWwoKClJSE", "42-49:1;57-80:1;69-74:1.1;86-101:1;107-127:1;119-121:1.1;133-150:1")]
[assembly: go.GoPositionMap("net/mac_test.go", "mac_test.cs", "ABUaADqMAYKCgpSWgoKClJSCgoKClII=", "84-89:1")]
[assembly: go.GoPositionMap("net/main_conf_test.go", "main_conf_test.cs", "AAwagoLYgqKSooKiuqKCgoKSgpSCgqqigoKCkoKUgoL2ooKUgoKClIKClIK01KKCgpS4goKClIKClIK0", "15-21:1;22-25:2;34-37:1;49-52:1;80-82:1")]
[assembly: go.GoPositionMap("net/main_posix_test.go", "main_posix_test.cs", "AA4egqaCgoKUgraCtoK2graCtoK2", "21-49:1")]
[assembly: go.GoPositionMap("net/main_test.go", "main_test.cs", "AB04hI6OloKChISCgoKClIKssoKCgoKUAAscgoK4uMyCgoKogoKWgoKSuLjMgoKCprimppS42rgACBCCgoKUgoKU2JKCgoKCgoKClJSCpoKCgpSCgpSmgoKClIKClA==")]
[assembly: go.GoPositionMap("net/main_windows_test.go", "main_windows_test.cs", "AA8WgpKCkoKWgoKCgoKCpoKCgoKCgqiSgg==")]
[assembly: go.GoPositionMap("net/mockserver_test.go", "mockserver_test.cs", "AB4uyIKClIKAgrbWooKUtLSCtpKCgoKUlpSClIK2graCtqaCggAKBoK+goKCsoKCgpSClIKClJSUAAkUgoKClKbSgoKCgoKCgoCCtoKClLbWgoKCAAgSggAKGKKCgoKmpqKCgoKCgqaCpqKCgoKCpoKCgoKUgoKmgoKClJKCgoKUAAgSwoSUtKSCgoCCpIKUhIKCgpSCgoSCgoKAgqSClICCgIKkgsSkooSCgoSCgoCCpIKUgpSCgoKAgqSClIK05KKClLS0graSgoKClJaClIKUgraCtoK2poL2gr6CgoKygoKClIKUgoKUlJQACBKCgoKUpqKCgoKCgoKClLaCpoKCgu6CpqKEgoKEgoKCgIKkgpSSlLS0goKmgIKAgqSCxKSihIKChIKCgIKkgpSClIKCgoCCpIKUgrSkwoSCkpKCooKUgoKCgpSUgoKUlAAMBoKEppaCgpaC7oKChICCpoKygoKClpKCgsqigpSEgpSCgpaCgIKkhIKAgqaCgKS0lLS2goKCloKAgtY=", "31-35:1;51-58:1;124-127:1;180-183:1;330-337:1;402-405:1;516-519:1;568-573:1;575-580:2")]
[assembly: go.GoPositionMap("net/net_test.go", "net_test.cs", "ABQkgpSkhIKCooKUhIKUpJSCgpSUpJSUtKSCgIKklIKCgrTIopSmgpKUloKCsoKUhLKCgoIABxKCloKUlIKCgoKUlLSkgoCCpIKUgoKCtJSCkoCCpoKClIKUlKSUlLSkgoCCpJSCgoKUgoK0yIKCgoKigpSEgpSklIKClJSklICCgIKkpIKCgrTIgoKCgqKClISClKaAgoCCpKSCgoIAAxLI4oKCgqKClISClKSUgIKAgqSkgoKCtMqylKaCsoKCtJKSgIKklISCsoKCgoCCpIKUopKCkoCCpoKClISCtKSCgoKCooKUhIKCooKCgrjCgoKClKKSgoKUkoKClJS4loKClpSUgoK0ztKCgpKCooKCgpSCgpSigoKClIKClIKAgtbcwoKygoKygriWlIKCgrSStKSCgoIACAySgpSokoKUgoKUgoKClJTosoSCgoLShIKClIKCopKigpaCgoKUlIKCkqaClJSAgpKCxNayAAkagII=", "27-67:1;86-180:1;92-138:1.1;188-222:1;230-260:1;268-292:1;304-309:1;310-315:2;319-330:3;352-407:1;360-367:1.1;368-375:1.2;419-428:1;429-438:2;452-482:1;455-471:1.1;457-465:1.1.1;485-488:2;495-500:1;501-507:2;518-528:1;529-532:2")]
[assembly: go.GoPositionMap("net/net_windows_test.go", "net_windows_test.cs", "ABsugoKClIKClIKClPzSooKUgoKUlJSCgoKUgoKUxqKCgpSUgoKClIKUxoCUgoKUgoKmgoKUpoKCgoKUgoKUgoKCgpSCppSCgoKWgrKCgoKUlJKCgrqCgpS6pNiCgpSCtPTCgoKUlIKClIKCgoKCgpSCgoKUgpSUgoKUAAkGgsqUgoKUgpSCAAgIgoKCAAEWAAkCgoKCgoKClIKUpKSkgoCCpIKm5oKEgoKUloKClIKClISCgoKUgoKUgoKUhIK4ABImgpKCgoKCgpSClIKUgpSCgoKCgoKCgoLcgoKCgrjWABYykoKCgoKCgpSClIKClIKUgpSCgqaogoKWlNaChIKClIKCloKClMSClIKCgpSClIKUlMaClJTIhIKCgoSCAAgOooKCgpSUggAJCIKEgoKUgoKUlJaCggAXMIKSgoKClIKUlKKClIKUlIKUlIKCgpSCgoKClIKClJSEgoKAgqSWgoKAgoKU/oCCgoCC2A==", "43-65:1;67-83:2;132-139:3;140-145:4;173-178:1;274-279:1;558-567:1;568-586:2")]
[assembly: go.GoPositionMap("net/packetconn_test.go", "packetconn_test.cs", "AA4q0oKApAAIBqIAAxKCgpSCuLKCgoKWgoKUkoSCgpSSgoSAgqSAgqSAgqSCgILWAAgEogADEoKClIK4soKCgoKWgoKUkoSCgpSSgoSAgqSCgIKkgpSkpICCpIKAgtY=", "23-23:1;38-45:1;95-102:1")]
[assembly: go.GoPositionMap("net/parse_test.go", "parse_test.cs", "ABEcxpSkhIKUlJKEgoKUlIKCgoKAgqSCgpSClIK0AAkEggAHGoKC")]
[assembly: go.GoPositionMap("net/platform_test.go", "platform_test.cs", "ABUmgsqCkoKCzqKCtpSkgtiUpMaUxpSCtoK2qqKAppLGqqKCloKCgKS0tLS0gpSCgpSCtIK0griCuoKUgpSCgoKUgsymkpSSqKaippSCgraCpA==")]
[assembly: go.GoPositionMap("net/port_test.go", "port_test.cs", "ABISABtItIKAgg==")]
[assembly: go.GoPositionMap("net/protoconn_test.go", "protoconn_test.cs", "ABEw8pSmgoKUgoKUkoKEgIKCpqSAgoKmpoCClMSkooKClIKClIKQkoKSgIKmgoKUgoKUkoKCgoKCgoKChICCpIKAgqaCtKSigoKUgoKUkoKCgoKCgoSCgoCCpICCpICCpICCpoCClKaCgIK4gqL0ooKWgoKUgqaSgpSSgoKCgoKChICClKaCgIK4goKi1KKCloKCgpSCgpSSgoKEgIKCpqSAgoKmpoCClMTUooKWpIKClIKClJKCgoKCgoKChIKClIKClJKCgoKCgoKChIKClIKClJKCgoKCgoKChIKCgoKAgqSAgqSAgqSAgqSAgqSAgqSAgqSAgqaAgpSmgoCCuIKCgqI=", "74-74:1;152-156:1;194-198:1;342-346:1")]
[assembly: go.GoPositionMap("net/rawconn_test.go", "rawconn_test.cs", "ABgcgpSmoqKCgoKUlIKClIKCgpSCgpSClIKCgpSCloKCgoKUgIKCxJKCkoCCpoKClJSCgpSCgIKkgoKClIK0kqKUpoKUgoKUlIKClISCgIKkgIKkgIKkgpSAgqSAgqSCloKAgqSAgqSAgqSCloKAgqSAgqSAgqSCtOaClKaigpSCgpSAgqaCgpSUgoKUgIKmgoCCpIKAgsQ=", "20-93:1;21-64:1.1;34-37:1.1.1;94-165:2;174-208:1")]
[assembly: go.GoPositionMap("net/rawconn_windows_test.go", "rawconn_windows_test.cs", "AAwagoKCkoKCgoKCgoKUgpSmgoKSgoKCgoKUgpSmgoKCkoKCgpQAASwAFAKSgsiAgqSmgoKClKSUgsaCxsaAgqQ=", "16-25:1;34-41:1;50-85:1;101-103:1;105-107:2")]
[assembly: go.GoPositionMap("net/resolverdialfunc_test.go", "resolverdialfunc_test.cs", "AB8uoriCttyCgqaCgqaCgriChKKCgpSAgriigoKUAAscgoKC3IKCgpSCpoKC7IKAgrYAEyiCggAHGAAKDoKU7KKCgoL8ooKCggAHEKKCgpSC3IKUqIIADB6AooCigKKAooCigKSCpsKClIKCgpaCgpSUgoKClIKWgoKCgpSUgoKCkoKCgoKCgpSClIK2graCyIKCloKCgoQACAqA0oCkgpSkpKQ=", "27-30:1;31-34:2;37-41:3;42-46:4;47-51:5;57-65:6;67-92:7;105-118:1")]
[assembly: go.GoPositionMap("net/sendfile_test.go", "sendfile_test.cs", "ACpI8oKCgpSCkpS8goKUgoKClIKCgpSCgpSCtKSAooKkguS4gqaCuqbUooKUgqSCgoKClsKClJKCgpSYopjEooKCgqbIgoKWgoK0yoKClJSCgoKWgpaCloK01KKClIKkgoKCgpbCgpSSgoKUlKailIKCxqaCgpSUgoSAkqaCtKSigpSChIKkgoKCgpbCgpSSgoKUkoCCgqailIKCtKaCgpSUgoSCloK01tKWtoSClIKClJKUhIKC9oKCgoKU2oKCgqailILEgoKCtJSC1oKCgoKUkqK4loCCpoLWgoKiuIKCkoKWoqbSgpSCwpKCloKClLiAgqaCgpTcgoKWgpTWgoKUlIKClIKWgILE1MKqhIKUgoKUlISCgqKEgoCCgqSCxpSChIKyhIKCgpSUgoKClJSCgpaAgoL6goKUlIKClIKWouSCppaAkoC2goKCuAAJEsKChIKUgoKClJSCgpaCgpSCtNSCgoKUkpaAgqSAgqamgoKC", "43-45:1;52-60:2;106-156:1;115-155:1.1;135-144:1.1.1;190-222:1;199-221:1.1;213-215:1.1.1;250-282:1;259-281:1.1;274-276:1.1.1;326-347:1;350-357:2;360-371:3;383-389:4;409-446:1;410-413:1.1;488-500:1;506-533:2;559-559:1;560-560:2;612-614:1", "570=net.sendFileBench.benchSendFile")]
[assembly: go.GoPositionMap("net/server_test.go", "server_test.cs", "ABQaAB9SkoSysoKWgoKAgqSWgoKCgqaCgoKUgoKQkoCCuIKCgoKUkoKCgIKklJKCloKCpoKCxszCAAUUhIKCgpaCgoCCpJaCgoKCpoKCgpSCgpCSgIK4goKSgoKAgqSWgIKmkoKWgoKmgoLYAAgEAB5agoKisoKUhIKCgIKkloKSkoCSgIKmgoKClIKSgoKAgqSUkpSCgoCCpJSSgoKUloK0gpSCxoKUgugACgiCAAQWgqKygpSEgoKAgqSWgpKSgJKAgqaCgpKCgoCCpJSCkpSCgoCCpJSCkpaCtIKUgsaClILo", "58-120:1;73-77:1.1;85-85:1.2;155-159:1;167-167:2;254-326:1;271-271:1.1;344-410:1;361-361:1.1")]
[assembly: go.GoPositionMap("net/tcpconn_keepalive_conf_posix_test.go", "tcpconn_keepalive_conf_posix_test.cs", "AAsW")]
[assembly: go.GoPositionMap("net/tcpconn_keepalive_conf_windows_test.go", "tcpconn_keepalive_conf_windows_test.cs", "ABcu6oI=")]
[assembly: go.GoPositionMap("net/tcpconn_keepalive_posix_test.go", "tcpconn_keepalive_posix_test.cs", "AA0esoKClIKClIKClIKClNymooKUgpSClIKUgpSCloKClIKWgoKUgpaCgpSCloKClII=")]
[assembly: go.GoPositionMap("net/tcpconn_keepalive_test.go", "tcpconn_keepalive_test.cs", "AA4cooSCpJiCloKCgoKUpraCkoCCprLGgoKUlIKWgoKUgJKU1qSihIKkmIKWgpKCgpSUsraCkoCCpJKCgpSUgpKClIKClICSlNakooSCgoKClKaCkoCCpLKSgoKUlIKClpiAgpSkgpaCgriCpqiAkpTW", "17-19:1;18-18:1.1;24-26:2;28-36:3;64-66:4;75-77:1;76-76:1.1;82-84:2;87-93:3;119-121:4;130-138:1;161-163:2;184-186:3")]
[assembly: go.GoPositionMap("net/tcpsock_test.go", "tcpsock_test.cs", "ABcqgqaCpoKmgtaCgpSmgoKUpoKClKaCgpSmwoSCgoKCgoKCgpSCpoKCgoKUlIKCgoKCgqaUgoKUkpTCgoKClJTSkoKUgpSCgoLGtoKClOKSlIKCgpSSgpSCgoLGpIKCtKSCpoKClKbCzoSCgpaCgoKClJKCsoKCgoKUlJSCgoKUlISEgoKUooKCgoKClIKCgoLGppaygoKCgoKClLSmsoKCgoKClIKCgoKmoqaigoKCgoKCpqKkogAMEgAVOqKCkJKEgoKCgpSCgoLYAAgEAAUSooSCgoKUkoKAgtakooSCloKCpoKUgpKCkJKAgqSAgqaCgpSSgIKkgIKmgIKkgoCCpoLGpKKCgoKUgoKCgpKCgoKUlKaCgoKCgoKUpoKCkpSCtKSimtaSgpSSgoKigoKUgoKUkoCCpISCooKClIKCpoKWgoKCsoKCpsKCgIKkgIK2grSkgoKCgoKWgoKCgpSUgoKCgoKCppaCgpSU4pKUgoKCpsKCgoKCxsSSlOKSlIKCgpSSgoKCxqSClILaooKWgqKClIKCgsKCgoKClIKCgoKClJSClKKUgoKUgoKClIKiyKKUpoKUgpKAgrbCgoKClJSCgoKClqKUgoKUlIKClJSCkoCCuJSygrqCgoKUgoKCtKTCgpKCgqKCgpSCgpSSgIKkgoKCgoKCtKSigpSCgJKApIKClJSCtKTagpSChIKCgtKEgoKCtKaCgpSUgoKChIKCgpS0", "82-89:1;90-100:2;108-132:3;116-130:3.1;117-120:3.1.1;137-156:4;138-140:4.1;198-208:1;224-239:2;245-256:3;259-275:4;278-289:5;332-332:1;393-393:1;437-446:1;485-489:1;501-510:2;518-523:3;524-532:4;546-553:1;554-564:2;572-592:3;573-575:3.1;582-590:3.2;595-611:4;596-598:4.1;628-668:1;635-655:1.1;682-686:1;687-703:2;718-722:3;726-729:4;750-754:1;778-778:1;779-779:2;779-779:2.1;806-814:1")]
[assembly: go.GoPositionMap("net/timeout_test.go", "timeout_test.cs", "ABQm7oKCggAMCgAKIMKUpoSCkoCCuLLqkoSqgoKWhIKChIIACxqChIKUgoKUgoKogoKCqICCpICC6PSigpKAgrgABxCikoKUgoKUgILo1KKElKa6goKihIKU7oLMooKW0oKSggAHEIKUgqKUloCCpISCgpSEgIKkgrTIooSUpoKUgIKkgIKkgoKWgIKkgrSkooSUpoKUgoKygIKkgIKkgoKWgpS0gIKktILEAAkEAAQYsqKCgoKUgrKSgpKAgqaCgpSUooCCpIKCgoKCgIKkgqaCgpSClNimsoSUpoKUgoKUlIKCsoCCpICCpICCpIKCgpaClLSAgqS0goKAgqSAgubEAAQWooKCkoCCtoKSgIKmgoKUgoKUkoSigIKkgoKCgoKAgqSCpoKClICCpNjEAAQYsoSClKKCgpSUgIKkgoKCgoCCpIKmgoKUgpTYprKElKaClIKClJSCgrKAgqSAgqSAgqSCgoKAgoLKgpS0gIKktIKCgIKkgILmpKKEgpSCgpa6grKCgpSUgIKkgoKCgoKmgoKUgriCgpSAgqSClIKUgpS0xAAFLKIAARIACwiqwoLKgoKUqLKClIKClJSCgoKCgoKAgqSCgoSClICCpIKWgoKUgpSAgoKCuIKCprSmsoKUgoKUlIKCgoKCgoCCpIKChIKUgIKkgpaCgpSClICCgoK4goKmtKTClKaClIKClJSCgoKCgoCCpIKCgoKCgoKmlIKUgIKkgpaCgpSClICCyoK4pICCuIKkprSmkoLY5IKUgqKm5IKUgqKkooKCgoKUgqaCkoCCpgAUKoKCgoKmgoKEgoKWgoCCpIKChICClOiowoSUpqKCgoKUlIKCsoKCgoCCgIKkgqSAgoCCpPiygoKCgIKAgqSCpICCgIKk+KKSgpKAgqaCgpSUgoKCtKayhIKCloKUgoKUlIKCsoKCkoKAgoCCpKSAgoCCpKS0krKCgoK0krKCgoK0kqKmsoKUgoKEgpKCgoKUpoKCkoLCpoKCgsakoqqygoKUgpSClA==", "26-31:1;59-63:1;66-133:2;139-143:1;153-165:2;184-248:1;204-207:1.1;209-226:1.2;295-305:1;335-343:1;405-419:1;456-460:1;579-597:1;637-682:1;960-969:1;1040-1089:1;1050-1068:1.1;1069-1087:1.2;1129-1148:1;1149-1155:2;1156-1162:3;1189-1199:1")]
[assembly: go.GoPositionMap("net/udpsock_test.go", "udpsock_test.cs", "ABkowoSClIKClIKCloKClJKCgpSUgoKAgqSAgtYADBIAFTqigpCShIKCgoKUgoKC2KSilKaCloKClJSCoqSigoKUlIKCloKCgpSClIKClIKUgoKUgoKUgpSCgrSkooKClJSCgpaCgoKUgoKUgoKUgoKUgpSCgrQACAQABRKChLKygpaCgpSSgoCCxAAJCKKCloKCgpSUgoKCgIK2goKUlAAFEoKAguikooSCloKCpoKUgpKCkJKAgqSAgqaCgpSSgIKkgIKmgIKkgoCCpoLG5KKUpKSCloKUgoKClIKUgoKCgoKUgpSCxqSilKSCloKUgoKCgpSClIKClJS2gLL4pKKUpIKWgpSCgpSUgoKCgpSClIKClJSSlILGqMKCloKClIKClJSCgoKClIKUgrTUopa0gpSCqISCgpSSgpKEsoKClIKCpoCCprKCgpSCgqaAgqaygoKUgoKmgILEpMKCgpSSgoKCgoKCgpSCgsakwoKClJKCgoKCgoKClIKCxqTCgoKUkoKCgoKCgoKUgoLG5KKUpIKWgoKUkoKCgoKUgoKUgpSCgpSCgpSCtNrSgpSClpzmgoKUlIKCgoSCgpaCgpaCgrSq0pSmgpaCgpSUgoKChICCpoCCpoCCxA==", "95-95:1;228-242:1;306-306:1;513-522:1;527-536:2;541-550:3")]
[assembly: go.GoPositionMap("net/unixsock_windows_test.go", "unixsock_windows_test.cs", "ABUgooKUkoKSgoKClIKClIKSgIKmgoKUgoKUwoKCtJKAgqaClMqCgtikooKWhIKClJSCgpaCgrQ=", "20-20:1;46-51:2")]
[assembly: go.GoPositionMap("net/writev_test.go", "writev_test.cs", "ABgkgoIACBCCgpSClIIACQiCACRSgoKCgsqCgoKCAAoMooKQkoiCgoKUgoKWkpKClIKCgpSUgpSClIKUlIKCloKCgpaClIKCgoKClIK2goKClIK2gpS05MKCloSCooKCqMKCgoKClKKSgoKUkoKClLqCkoKUgILE", "94-96:1;103-103:1;108-112:2;118-140:3;140-179:4;190-195:1;197-205:2")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("net")]
public static partial class net_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha256() => builtin.initPackage(typeof(crypto.sha256_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸpoll() => builtin.initPackage(typeof(@internal.poll_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyscallꓸwindows() => builtin.initPackage(typeof(@internal.syscall.windows_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸinternalꓸsocktest() => builtin.initPackage(typeof(net.@internal.socktest_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸnetip() => builtin.initPackage(typeof(net.netip_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸsyscall() => builtin.initPackage(typeof(syscall_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸnetꓸdnsꓸdnsmessage() => builtin.initPackage(typeof(vendor.golang.org.x.net.dns.dnsmessage_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸnetꓸnettest() => builtin.initPackage(typeof(vendor.golang.org.x.net.nettest_package));
    // </ImportInitializers>
}
