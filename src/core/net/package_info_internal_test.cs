// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
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
// </ImplicitConversions>

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(dualStackServer))]
[assembly: GoFieldAccess(typeof(localPacketServer))]
[assembly: GoFieldAccess(typeof(localServer))]
[assembly: GoFieldAccess(typeof(lookupCustomResolver))]
[assembly: GoFieldAccess(typeof(resolverDialHandler))]
[assembly: GoFieldAccess(typeof(resolverFuncConn))]
[assembly: GoFieldAccess(typeof(routeStats))]

// The go statements whose goroutine the converter proved cannot recurse, by the file, line and column of
// their goǃ call: go2cs-gen starts each on golib's bounded stack reservation rather than the Go-scale default.
[assembly: GoBoundedGoroutine("dnsname_test.cs", 67, 5, 0)]
[assembly: GoBoundedGoroutine("net_test.cs", 379, 9, 0)]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("net/conn_test.go", "conn_test.cs", "ABIogoKigqaCkoKQkoCCpIKmgoKUkoKUgoKUgIKkgoKmgrQ=", "22-61:1;30-30:1.1", "", "", "30:29,50:29")]
[assembly: GoPositionMap("net/dial_test.go", "dial_test.cs", "ACEwAAcQopSkpIKmgoKUpIKCpoKCgoLGpKKCkoKClIKClKLUoqS0tKSCpoKCkJKCgoKCgpSmgoKUgoKmgoKCgoLCgoKCgpSipIKCgoK0AAoa0oKClJSm3IKClIKUgoKClIKCABIGgoKUgoKClKYAGD6SgoKCgpSUtrKikoKClIKUgoKUpoKCpoKClILcgpSCpoKCgqaCuIKCgpKCgriCgpTKgrbaAA0IopSCpoKQkpSCkJKUAAgagoKCgpSmgoKUkoKmgpSCgoKCgoKUgoKClILGpMKCpoKCgpSmgoKkgoIACBKCuIKCgpSClIKClJKCppSCgoKQkriCyoKmptySgoKUuIKClKaiAAoEgoIADyyCgoKUgtz0ooKWACxkgoKCuMqCkJKCgoKCgpSmgoKCkoK4goKCgoKUgpSCgriClKaCgpSUtKSilIKmlIKQkoKCgoKClLiisoKClJKCpoKCgoKClKS0pMb0ooK2goKCgpSmpoKSgIK2AAYUgoCkgoKCgoKUgoLG5KKUgoKmgqSClIKCgoKCooKUpoKCxIKClILGgrSClKbcgoKCuKaClAAOCqKCppSCgpKCuJKCgoKUgoKCgoKUgpSUprKCgsqClIKUppKCuIKCgpSClIK0toK05IKUlIKigsqCgoKCgpTIlMqCgoKCgpSCgpS0AAgQ4oIADBqCgpSkgoKCAAsYgoKClIKiAAgEgqS0tqKCgpSCkoKCgoKUtJKigoKUgoKUkoKCgoKUtLaCpLSkgoKigqaCkoKCgqSCgpSClKLagqS0tqKCgoKUgpKCgoKkgqSCgoKUxIKCgpS0goKUgpS0kqKCgoKUgoKUkoKCgqSCpIKCgpTEgoKClLSCgpSClLS8soKCgoLKlO6UpqKCkoKCpoKCgoKUog==", "92-92:1;94-102:2;117-125:3;183-189:1;222-232:2;235-300:3;236-249:3.1;288-292:3.2;325-325:1;329-329:2;345-353:3;400-425:1;440-440:2;441-453:3;464-470:4;589-589:1;591-599:2;651-651:1;653-661:2;693-695:1;694-694:1.1;697-705:2;726-726:3;761-767:1;822-825:1;828-847:2;849-883:3;897-935:1;996-1011:1;1012-1030:2;1040-1064:1;1042-1062:1.1;1050-1053:1.1.1;1075-1114:1;1084-1087:1.1;1115-1157:2;1127-1130:2.1", "", "257=Invoke/1/1/27;378=ParseIP/1/2/1,ParseIP/2/2/2;595=Add/1/13/8,Add/2/13/8,Add/3/13/9,Add/4/13/9,Add/5/13/10,Add/6/13/10,Add/7/13/12,Add/8/13/12,Add/9/13/14,Add/10/13/14,Add/11/13/18,Add/12/13/19,Add/13/13/20;685=ParseIP/1/1/1;689=ParseIP/1/1/1", "62:21,86:21,444:21,525:21,713:25,793:25,850:21,1041:29,1102:29,1163:21,1219:29,1244:29,1281:33,1326:29,1382:29,1459:21", "128:21,129:13,409:21,410:13,416:21,417:13,536:21,537:13,696:21,697:13,767:21,768:13,995:21,996:13")]
[assembly: GoPositionMap("net/dnsclient_test.go", "dnsclient_test.cs", "AAoWgoKCppSCgoKCgoKmgoKCgoKUgriCgoKUpoKCgoKmgsqmgg==")]
[assembly: GoPositionMap("net/dnsname_test.go", "dnsname_test.cs", "ABAiABAiooKCgpSClIKmgsqUlKKkgoKCgoLKopS4goKC", "", "", "80=Repeat/1/2/1,Repeat/2/2/2", "37:21")]
[assembly: GoPositionMap("net/error_posix_test.go", "error_posix_test.cs", "ABIeggAKHII=")]
[assembly: GoPositionMap("net/error_test.go", "error_test.cs", "ABgsooKUgpSCxoLGgsaCxoLGgsaCxoLWpoKU7LKCppSClIKklIKClAAIBKSkgrSCpJaUlIKClKYAEyyipLaCkJKGhpSCspKCgoKClIKCgpSClIKmgpSC2KSCpLaCgqS0tLSkgoKUgoKUAAkIgqS2gqYAChiCgpSSgoKCgoKUkpSCgoKUgpSCgoKUgoKUgtwADiKipLaCkJKGhpSykoKCgoKUgoKClIKUgqaClILYpAALHKKktoKQkpiykoKCgoKUgpSC2KSCpLaCgqS0tLS0pIKClIKCAAQQsoKmlIKUgqSClJSCgpSUgqSUlJSCgpSssoKmlIKUgqSUgoKUAAgEpKSCpJSUlIKClOyyggAHEIKCpoKmlIKUgqSUgoKUlIK0gqSUlJSCgpSUlKaCooKSgoKUpIKCgqaCgoKmgoKAgqSCgILWpKKCpoKClKSCgoLGvLKCppSClIKklIKClJSCtIKklJSUgoKUpoKCgoKCgpSCgpSClJSmgoCCgraCrLKCppSClIKklIKClJSCtIK0gqSUlJSCgpT2ooKClIKkgoKClIKmgpSCgoKUgqaClIKCgpSCpoKmlIKCgoKmlLSkgoKmpKSU5oKClII=", "147-147:1;148-150:2;151-153:3;158-183:4;243-274:1;302-302:1;303-305:2;306-308:3;312-337:4;362-362:1;363-365:2;368-381:3;565-596:1;598-615:2;662-680:1", "", "156=JoinHostPort/1/4/11,JoinHostPort/2/4/12,JoinHostPort/3/4/13,JoinHostPort/4/4/14;343=JoinHostPort/1/2/10,JoinHostPort/2/2/11;414=JoinHostPort/1/2/10,JoinHostPort/2/2/11", "686:25,692:25,737:25,896:21", "188:21,189:13,370:21,371:13,438:21,439:13")]
[assembly: GoPositionMap("net/error_windows_test.go", "error_windows_test.cs", "AA0eloKCprg=")]
[assembly: GoPositionMap("net/external_test.go", "external_test.cs", "ABMegpSCpoKCgoKUlKSkAAsKABEugpSCpoKCgpSCgoKCgqbMAAoWAAoQwoKClIKUkoKClIKUgqaCgoKmlIKCgqaUpqKCgpSSgoKUgoKClIKU", "", "", "", "170:21")]
[assembly: GoPositionMap("net/file_test.go", "file_test.cs", "ABkqAAYSgqS2srKCppKkgpKCtKKCgpSSgqKSgpKClIK2goKClJSUgpS0tKSCgpSUgoKUpoKClIKClJSkgIKClKSCtMgABRCCpLaysoKmgqSklJiUtKSkxIK2goKUpoKClIKClJSkgoKygoKCgpSClKKSgoKClJSCgoK0yM6CpLaysoKmgqSklJiUtKSCgpSUgoKUpoKClIKClJSkgoKUlIK0+rKkpIKmooKClJKCoqSCkoCCtoKCgoKUlIKCsoKCgrSSsoKikrQ=", "37-120:1;50-58:1.1;139-214:1;190-201:1.1;232-288:1;302-310:1;328-334:2;335-338:3", "", "", "60:33,74:41,83:33,139:29,209:33,236:29,346:29,389:29,399:21")]
[assembly: GoPositionMap("net/hosts_test.go", "hosts_test.cs", "ABUkACdWooCkgoKCxqSCgoKCgsoALGCigKSCgoLGpIKCgpSCAAkIxoCkgoKUgoKUlIKClIKClKLUABUoooCkgoK0pIKCgoKC", "62-62:1;131-131:1;154-154:1;198-198:1", "", "", "", "65:33,66:13,138:33,139:13,174:33,175:13,230:33,231:13")]
[assembly: GoPositionMap("net/interface_test.go", "interface_test.cs", "AA0isoKClLKCpqyygpSCgpSCgoK4poKCgpSygoKUpILGgraCgoKUgqa4goKClIKCgpSCgpSCuIKCgpSCgpSCsoKClIKClIKUgriCgoKUgoKClIKClIKygoKUgoKUgpSCAAgSgoKCgoKUuO7ugoKUgpSClIKCgpSClJSCgpSClMaClIKUgpSC1qamgoKClIKUgpSClILWpqbEgsqClKbCABEegramooKUgoL6ooKUgoKUgoLKooKUgoKUgoLKooKUgoLKooKUgoKUgoLKooKUgoKUgoI=")]
[assembly: GoPositionMap("net/ip_test.go", "ip_test.cs", "ABQeACBIgoKClJSUgoLKgoKClIKCyoKCgoKUuIKCuIKCgpSClILcopSCgsqilIL4opSCupKCgoKmgoKClIKmgoKClIIACggAVtYBgoKAgqSAgqSCgILalIKSgpSCAAwKgoKCgoKCpoKCgoKCpoKClIKC7oKUgqaCuKKCgoKCggALDAAIGoKCggAICgAHFoKCgsqilIKCAAsKAB5IgoKCgpSCAAoKAAoegoKCAAkKAAcWgoKCAAkKAA4mgoKCzIKCgoKCgoKC1gARKoKygoKCAA8KggAgSoK4ABg4gpSCgpSCAAsMggAeRoIACgoAESyCgoCCpICCAAsKAEKOAYKmgoKAgqSCgpSAgtqCgpSCuKKCgoKmgoKCpoKCgg==", "288-291:1;299-306:1;307-314:2;315-323:3;331-333:1;335-337:2;813-815:1;816-818:2", "", "21=IPv4/1/6/4,IPv4/2/6/5,IPv4/3/6/6,IPv4/4/6/7,IPv4/5/6/8,IPv4/6/6/9;375=IPv4/1/8/5,IPv4Mask/1/2/5,IPv4/2/8/5,IPv4/3/8/6,ParseIP/1/8/6,IPv4/4/8/6,IPv4/5/8/7,ParseIP/2/8/7,IPv4/6/8/7,IPv4/7/8/8,IPv4Mask/2/2/8,IPv4/8/8/8,ParseIP/3/8/9,ParseIP/4/8/9,ParseIP/5/8/9,ParseIP/6/8/10,ParseIP/7/8/10,ParseIP/8/8/10;396=IPv4Mask/1/2/4,IPv4Mask/2/2/5,ParseIP/1/2/6,ParseIP/2/2/7;431=IPv4/1/10/6,IPv4/2/10/6,IPv4Mask/1/5/6,IPv4/3/10/7,IPv4/4/10/7,IPv4Mask/2/5/7,IPv4/5/10/8,IPv4/6/10/8,IPv4Mask/3/5/8,IPv4/7/10/9,IPv4/8/10/9,IPv4Mask/4/5/9,IPv4/9/10/10,IPv4/10/10/10,IPv4Mask/5/5/10,ParseIP/1/36/11,ParseIP/2/36/11,ParseIP/3/36/11,ParseIP/4/36/12,ParseIP/5/36/12,ParseIP/6/36/12,ParseIP/7/36/13,ParseIP/8/36/13,ParseIP/9/36/13,ParseIP/10/36/14,ParseIP/11/36/14,ParseIP/12/36/14,ParseIP/13/36/15,ParseIP/14/36/15,ParseIP/15/36/15,ParseIP/16/36/16,ParseIP/17/36/16,ParseIP/18/36/16,ParseIP/19/36/17,ParseIP/20/36/17,ParseIP/21/36/17,ParseIP/22/36/18,ParseIP/23/36/18,ParseIP/24/36/18,ParseIP/25/36/19,ParseIP/26/36/19,ParseIP/27/36/19,ParseIP/28/36/20,ParseIP/29/36/20,ParseIP/30/36/20,ParseIP/31/36/21,ParseIP/32/36/21,ParseIP/33/36/21,ParseIP/34/36/22,ParseIP/35/36/22,ParseIP/36/36/22;480=IPv4/1/8/5,IPv4/2/8/5,CIDRMask/1/4/5,IPv4/3/8/6,IPv4/4/8/6,CIDRMask/2/4/6,IPv4/5/8/7,IPv4/6/8/7,IPv4Mask/1/2/7,IPv4/7/8/8,IPv4/8/8/8,IPv4Mask/2/2/8,ParseIP/1/10/9,ParseIP/2/10/9,CIDRMask/3/4/9,ParseIP/3/10/10,ParseIP/4/10/10,CIDRMask/4/4/10,ParseIP/5/10/11,ParseIP/6/10/11,ParseIP/7/10/11,ParseIP/8/10/12,ParseIP/9/10/12,ParseIP/10/10/12;504=IPv4/1/2/4,CIDRMask/1/2/4,IPv4/2/2/5,IPv4Mask/1/1/5,ParseIP/1/3/6,CIDRMask/2/2/6,ParseIP/2/3/7,ParseIP/3/3/7;525=IPv4Mask/1/4/5,IPv4Mask/2/4/6,IPv4Mask/3/4/7,IPv4Mask/4/4/8;732=IPv4/1/3/9,IPv4/2/3/10,IPv4/3/3/11,ParseIP/1/3/17,ParseIP/2/3/18,ParseIP/3/3/19;771=IPv4/1/34/6,IPv4/2/34/10,IPv4/3/34/11,IPv4/4/34/12,IPv4/5/34/16,IPv4/6/34/17,IPv4/7/34/18,IPv4/8/34/23,IPv4/9/34/24,IPv4/10/34/27,IPv4/11/34/28,IPv4/12/34/29,IPv4/13/34/33,IPv4/14/34/34,IPv4/15/34/35,IPv4/16/34/39,IPv4/17/34/40,IPv4/18/34/41,IPv4/19/34/48,IPv4/20/34/49,IPv4/21/34/50,IPv4/22/34/51,IPv4/23/34/52,IPv4/24/34/53,IPv4/25/34/54,IPv4/26/34/55,IPv4/27/34/56,IPv4/28/34/57,IPv4/29/34/58,IPv4/30/34/59,IPv4/31/34/60,IPv4/32/34/61,IPv4/33/34/62,IPv4/34/34/63")]
[assembly: GoPositionMap("net/iprawsock_test.go", "iprawsock_test.cs", "ABs0ACI8ooKmgpCSlIKCgoKUgoKC2PQACBKigoKClIKUgoKClJKCxuSigoKmgoKUgoKUkoK0AAwEogASLoKC3oKClIKUgriCooKClIKClIKCgpSCgpSCgg==", "62-62:1", "", "28=IPv4/1/4/1,IPv4/2/4/2,IPv4/3/4/3,ParseIP/1/7/5,ParseIP/2/7/6,ParseIP/3/7/7,ParseIP/4/7/8,ParseIP/5/7/10,ParseIP/6/7/11,IPv4/4/4/13,ParseIP/7/7/14,Error/1/6/22,Error/2/6/23,Error/3/6/24,Error/4/6/25,Error/5/6/26,Error/6/6/27;100=IPv4/1/1/4", "126:25,157:21", "72:21,73:13")]
[assembly: GoPositionMap("net/ipsock_test.go", "ipsock_test.cs", "AAsYAAsEALIBlgOCgqaCgoKUgoKUlIKClIKClIKUgoIAEgqCmgAFFMSCkoKUgpSC", "270-273:1", "", "24=IPv4/1/40/12,IPv4/2/40/16,IPv4/3/40/17,IPv4/4/40/25,IPv4/5/40/28,IPv4/6/40/30,IPv4/7/40/36,IPv4/8/40/37,IPv4/9/40/40,IPv4/10/40/42,IPv4/11/40/43,ParseIP/1/10/52,ParseIP/2/10/58,IPv4/12/40/66,IPv4/13/40/67,ParseIP/3/10/69,IPv4/14/40/72,IPv4/15/40/74,IPv4/16/40/75,ParseIP/4/10/79,ParseIP/5/10/87,IPv4/17/40/88,IPv4/18/40/89,IPv4/19/40/92,ParseIP/6/10/95,IPv4/20/40/98,IPv4/21/40/99,IPv4/22/40/106,IPv4/23/40/108,ParseIP/7/10/109,IPv4/24/40/112,IPv4/25/40/114,IPv4/26/40/115,ParseIP/8/10/119,IPv4/27/40/127,ParseIP/9/10/128,IPv4/28/40/129,IPv4/29/40/132,ParseIP/10/10/135,IPv4/30/40/138,IPv4/31/40/139,IPv4/32/40/147,IPv4/33/40/151,IPv4/34/40/152,IPv4/35/40/160,IPv4/36/40/163,IPv4/37/40/164,IPv4/38/40/172,IPv4/39/40/185,Error/1/5/194,Error/2/5/196,Error/3/5/197,Error/4/5/199,IPv4/40/40/200,Error/5/5/200;252=ParseIP/1/3/1,ParseIP/2/3/2,ParseIP/3/3/3")]
[assembly: GoPositionMap("net/listen_test.go", "listen_test.cs", "ABImgoKClKaCgoKUpgAWOrKktoKCgqaCgpSCgpSCgpSCgpS4ABY6sqS2goKCpoKClIKClIKClIKClAAICAA3hgHipKSCpqKCgqaClJKCgoKUgoKUgoKClIKClIKClIKClIKUgpSCygAWQuKkpIKmooKCpoKUkoKCgpSCgpSCgoKUgoKUgoKUgoKUgpSClILKgoKUgpSmgqSCgILWgoLGgoLGgoCC1oKCxoKCxpSmgqSCxoLGlKaCpILGgsaUpqLcgoKmggAJEIKCpoLolOailKS2goK4gIKkgIKkgIKkgIKkgILE9AAIGLKUpKSCpoKCgsrcgpSCgoKClIKClIKClIKClMoAEyyylKSkgqaCpoKCgsrcgpSCgoKClIKClIKClIKClMqCgIKCgqSCgIKkpoKkpIK2goKUsoKClIKCuKiSgpSSgoKClO6UlIKClAAIBoKktoKCgpSCpoKCgpSC", "495-499:1;541-547:1;618-624:1;703-711:1;733-741:1;742-750:2", "", "582=IPv4/1/2/4,IPv4/2/2/6;647=ParseIP/1/12/4,ParseIP/2/12/5,ParseIP/3/12/6,ParseIP/4/12/7,ParseIP/5/12/8,ParseIP/6/12/9,ParseIP/7/12/11,ParseIP/8/12/12,ParseIP/9/12/13,ParseIP/10/12/14,ParseIP/11/12/15,ParseIP/12/12/16", "", "541:21,542:13")]
[assembly: GoPositionMap("net/lookup_test.go", "lookup_test.cs", "ABwupIKmgqTaAA0WABQw5IKClIKmgqaCgoKCgoKCgoKCgoKUlIKUgpSCggAIDM6CgpSCpoKmgoKCgoKCgoKCgoKClJSClIKC3M6CgpSCpoKmgoKCgoKCgoKCgoKClJSClIKCAAgM/oKClIKUgqaCpoKCgoKCgoKCgoKCgpSUgqaWygAMDqKUgqaUgoKClIKUgoLY9KKCppSCgpSClIK05IKCpoKClIKSuIKClIKS+gAHFKKClIKmlIKCgoKCgoKCgoKCgpSUgsbUzqKClIKmlIKCgpSClIKC2OSigpSUgoKUgriCtKTOooKUgqaUgoKClIKUgoLY9AANJKKCgoKCgpSClIKUgsYADASigqaUggAHEKaCgpKCpIKCgqKClKKCpoqCgrS0lLSCgpSClILWggANGKIACASigqaUlIKCgpSCgoKClIKClIKCgoKClKamgoL45KKClIKUgqaCppSAgoKkgIKCxAAKBIKCgpSCgoLKgoKCgqaCgpSCgoLKgoKUgoKCyoKClIKUgoKC3IKCgoKCgpSCpoKCgoKCgpSCpoKCgoKCgpSCAA4GjAAWOqSCxraCgoKUgoIACxCiAAkegoKCAAkKggAIGoKCggAIDKKUgIIACA6CgpSClIK0pKKClIKSgqaCyqwAChCCgqKCgoK0kraC3IKClIKmgoKUogAOCKKCgqaCgoKCgoKCgoIACgqigoKUvIKCpoKCgoKCAAoWpoKCAAkUgoKCgoK8wsi2gpSUgoKCgqaCgoKigoKCtKSUgqaCgsbkAA0igoKCABAQwoKQpM6CgqbKooKCgqaUlMqCgoKUgsamsoKQpO4ACBiCgtiClOaUpoKSgoKCgoLCgoKClIKCtKSi1pKCgpSCuIKmkrimgJLIpoCCypKCggAOBqKUgpSU7rKCgpSkgoKUtLiSgoKCpoKCgoKClKbKgoKmgoLKgpSC2MQACQaygpCSlIKCgpSUgoKCgoKCgoKCyoKClqKCgqKCopKigqKSgoKCpoKCgrKCopKygqKSgoKCgoKiAAsEgoKmlJSigqKkooKi9oKCpoKCgqaCgoKCgqaCgqaCuIKCgoKCgqaC+IKCgoLulIKUpoKCgoLKgoKCgsqCgoKCABYKgpSClL6C3ILcgtyC3ILcgtyC3ILcgtyCyrKSkoKCgoKClIKCgoKUgoKUgqaCgoKCgoKUggANDoKUlJaogoKmgoKmgoKCgg==", "250-252:1;529-532:1;533-536:2;882-885:1;893-932:2;909-915:2.1;1011-1016:1;1044-1050:1;1095-1095:1;1114-1122:2;1144-1144:1;1167-1180:2;1189-1199:3;1262-1321:1;1270-1319:1.1;1328-1328:1;1332-1335:2;1338-1349:3;1360-1363:4;1364-1367:5;1377-1380:6;1381-1384:7;1402-1405:1;1407-1410:2;1457-1462:1;1476-1481:1;1485-1490:1;1494-1499:1;1514-1517:1;1521-1524:2;1528-1531:3;1535-1538:4;1542-1545:5;1549-1552:6;1556-1559:7;1563-1566:8;1570-1573:9;1577-1580:10;1585-1620:11;1586-1619:11.1;1629-1631:1;1632-1634:2", "", "40=IPv4/1/1/1;482=Repeat/1/1/1;1319=IPv4/1/1/1;1373=IPv4/1/2/2,IPv4/2/2/6", "608:21,611:21,1407:21", "1038:21,1039:13,1305:21,1306:13,1362:21,1363:13,1608:21,1609:13")]
[assembly: GoPositionMap("net/lookup_windows_test.go", "lookup_windows_test.cs", "ABkugpSCguaCgoKCgpSUgoKCgpQACAyClIKCgpSClIKClIKClJSCgoLKgpSCgoKUgpSCgpSCyoKUgoKClIKUgoKUhoKCgsqClIKCgpSClIKClIKCgsqClIKClIKClIKUgoKUgoKCuIKUgoKKwoKUlIKUgoKClIKCggAKCoKCgoKCgoKUpoKUAAgGsoKClKaCgoK4goKClOaygoKUlIKClAAIBrKCgqaUgoKUAAgGsoKClAAHEIKClOaCgoKClILmsoKClIKCgpTmwoKClKSU", "40-47:1;55-78:1;67-72:1.1;84-99:1;105-125:1;117-119:1.1;131-148:1", "", "", "394:21")]
[assembly: GoPositionMap("net/mac_test.go", "mac_test.cs", "ABEaAE2kAYKCgpSmgoKClJSCgoKClII=", "96-101:1")]
[assembly: GoPositionMap("net/main_conf_test.go", "main_conf_test.cs", "AAwagoLYgqKSooKiuqKCgoKSgpSCgqqigoKCkoKUgoIACAaigpSCgoKUgoKUgrTUooKClLiCgoKUgoKUgrQ=", "15-21:1;22-25:2;34-37:1;49-52:1;80-82:1")]
[assembly: GoPositionMap("net/main_posix_test.go", "main_posix_test.cs", "AA4egqaCgoKkgsaCxoLGgsaCxoK2", "21-49:1")]
[assembly: GoPositionMap("net/main_test.go", "main_test.cs", "AB44lO7uloKClJSCgoKClIKssoKCgoKUAAscgoK4uNyCgoK4goKmgoKCuLjcgoKCprimpqS46rgACBCCgoKUgoKU6JKCgoKCgoKClJSCpoKCgpSCgpSmgoKClIKClA==", "", "", "89=IPv4/1/2/1,IPv4/2/2/2;93=IPv4/1/2/1,IPv4/2/2/2;97=IPv4/1/2/1,IPv4/2/2/2;118=ParseIP/1/2/1,name/1/1/1,ParseIP/2/2/2;122=ParseIP/1/2/1,name/1/1/1,ParseIP/2/2/2;126=ParseIP/1/2/1,name/1/1/1,ParseIP/2/2/2")]
[assembly: GoPositionMap("net/main_windows_test.go", "main_windows_test.cs", "ABIegpKCkoKWgoKCgoKCpoKCgoKCgqiSgrjqgpQ=")]
[assembly: GoPositionMap("net/mockserver_test.go", "mockserver_test.cs", "AB0uyIKClIKCptaigpS0tIK2koKCgpSmpIKUgsaCxoLGpoKCAAsGgr6CgoKygoKClIKUgoKUlJQACRSCgoKUptKCgoKCgoKCgqaCgqS21oKCggAIEoIAChiigoKCpqaigoKCgoKmgqaigoKCgqaCgoKClIKCpoKCgpSSgoKClAAIEsKUlLSkgoKClIKUlIKCgpSCgpSCgoKClIKUgoKUgrSkopSCgpSCgoKUgpSClIKCgoKUgpSCtAAIBKKClLS0graSgoKClKaCpIKUgsaCxoLGpoL2gr6CgoKygoKClIKUgoKUlJQACBKCgoKUpqKCgoKCgoKCpLaCpoKCgu6CpqKUgoKUgoKCgpSClIKkxLSCgqaCgpSCtKSilIKClIKCgpSClIKUgoKCgpSClIK0pMKUgqaCkpKCooKUgoKCgpSUgoKUlAANBoKCgoKmgu6UgoKUlIKUgqaCsoKCgqaSgoLKooKUlIKCgoKUlJSCpIKCpoKClJSCgqaCgLTEpMTGgoKCpoKCxg==", "31-35:1;51-58:1;124-127:1;180-183:1;330-337:1;402-405:1;520-523:1;572-577:1;579-584:2", "", "121=IPv4/1/1/4;282=Addr/1/2/2,String/1/2/2,Addr/2/2/3,String/2/2/3;457=IPv4/1/1/4;680=Itoa/1/2/3,Itoa/2/2/4,Getenv/1/1/5", "296:21,350:21,539:21,587:21,634:21,742:21,772:29")]
[assembly: GoPositionMap("net/net_test.go", "net_test.cs", "ABYmgqSklIKigpSUgqSkpIKClKSkpJS0pIKClJSCgoK0yKKktoKSlKaCsoKUlLKCgoIACBKCpoKUpIKCgoKUlLSkgoKUgpSCgoK0pIKSgqaCgpSClKSkpJS0pIKClJSCgoKUgoK0yIKCgqKClJSCpKSkgoKUpKSkgoKUlIKCgrTIgoKCooKUlIKktoKClJSCgoIAChLIgoKCooKUlIKkpKSCgpSUgoKCtMqypLaCsoKCtJKSgpSUlIKygoKCgpSClKKSgpKCpoKClJSCtKSCgoKigpSUgoKigoKCuMKCgoKUopKCgpSSgoKUpLimgoKmlJSCgrTO0oKCkoKigoKClIKClKKCgoKUgoKUgoLG7MKCsoKCsoK4ppSCgoK0krSkgoKCAAgMkoKUqJKClIKClIKCgpSUqLKCmKKCgqKSgoKUsoKCgpSCopKClKS4gpSi5rKUgoKC0pSCgpSCgqKSooKmgoKClKSCgoKmgpSUgoKCtNaSAAoagg==", "27-67:1;85-179:1;91-137:1.1;186-220:1;227-257:1;264-288:1;300-305:1;306-311:2;315-326:3;347-402:1;355-362:1.1;363-370:1.2;414-423:1;424-433:2;447-477:1;450-466:1.1;452-460:1.1.1;480-483:2;490-495:1;496-502:2;513-517:1;522-530:2;531-533:3;553-563:1;564-567:2", "", "", "49:29,62:29,143:37,179:29,198:29,250:29,263:29,346:29,382:29,404:29,420:21,467:37,482:29,488:29,494:37,504:37,526:21,576:25,582:33,687:21,721:29,747:21", "454:29,455:21,654:21,655:13,736:21,737:13")]
[assembly: GoPositionMap("net/net_windows_test.go", "net_windows_test.cs", "ABougoKClIKClIKClAAKDNKigpSCgpSUpIKCgpSCgpTWooKClKSCgoKUgpTWlIKClIKCpoKClLaCgoKClIKClIKCgoKUgqakgoKCpoKygoKClJSSgoLKgoKU+qTIgoKUgrQACgTCgoKUlIKClIKCgoKCgpSCgoKUgpSUgoKUAAsGgsqUgoKUgpSCAAsIgoKCAAEWAAkCgoKCgoKUgqS0tKSCgpSCpgAIBoKUgoKUpoKClIKClJSCgoKUgoKUgoKUlIK4ABImgpKCgoKClIKUgpSClIKCgoKCgoKCgtyCgoKCuOYAFzKSgoKCgoKUgpSCgpSClIKUgoKmuIKCppTmgpSCgpSCgqaCgpTEgpSCgoKUgpSClJTGgpSUyJSCgoKUggAKDqKCgoKUlIIADQiClIKClIKClJSmgoIAFzCCgoKCgpSClJSigpSClJSClJSCgoKUgoKClIKClJSUgoKClKaCgoCCgpT+gIKCgsg=", "43-65:1;67-83:2;132-139:3;140-145:4;173-178:1;273-278:1;555-564:1;565-583:2", "", "", "70:29,95:29,127:21,151:21", "172:21,173:13")]
[assembly: GoPositionMap("net/packetconn_test.go", "packetconn_test.cs", "AA4q0oKApAAIBqIABBKCgqSCyLKCgoKmgoKUkpSCgpSSgpSClIKUgpSCgsYACASiAAQSgoKkgsiygoKCgqaCgpSSlIKClJKClIKUgoKUgqS0pIKUgoLG", "23-23:1;38-45:1;95-102:1", "", "38=testUnixAddr/1/2/6,testUnixAddr/2/2/6;109=testUnixAddr/1/2/6,testUnixAddr/2/2/6", "149:25")]
[assembly: GoPositionMap("net/parse_test.go", "parse_test.cs", "ABIcxqSklIKUlJKUgoKUpIKCgoKClIKClIKUgrQACQSCAAcagoI=", "", "", "", "38:21")]
[assembly: GoPositionMap("net/platform_test.go", "platform_test.cs", "ABAiooLWpKSC6KSkxqSktqSCxoK2qqLGgraqooKmgoK0xMTElIKUgoKUgrSCtILIgsqClIKUgoKClILcpoKUgrimoqakgoLGgpQ=")]
[assembly: GoPositionMap("net/platform_windows_test.go", "platform_windows_test.cs", "AAsSgg==")]
[assembly: GoPositionMap("net/port_test.go", "port_test.cs", "AA8SAB9IlIKC")]
[assembly: GoPositionMap("net/protoconn_test.go", "protoconn_test.cs", "ABEw8qS2goKUgoKUkoKUgIKCpqSAgoKmtoKUtKSigoKUgoKUgpCSgpKCpoKClIKClJKCgoKCgoKCgpSClIKCpoK0pKKCgpSCgpSSgoKCgoKClIKCgpSClIKUgqaClKaCgriCogAJBKKCpoKClIKmgoKUkoKCgoKCgpSClKaCgriCgqLUooKmgoKClIKClJKCgpSAgoKmpICCgqa2gpS01KKCprSCgpSCgpSSgoKCgoKCgpSCgpSCgpSSgoKCgoKCgpSCgpSCgpSSgoKCgoKCgpSCgoKCgpSClIKUgpSClIKUgpSCpoKUpoKCuIKCgqI=", "74-74:1;152-156:1;194-198:1;342-346:1", "", "", "42:21,93:21,107:21,146:21,216:21,265:21,322:21,341:21,360:21", "176:21,177:13,231:21,232:13,405:21,406:13")]
[assembly: GoPositionMap("net/rawconn_test.go", "rawconn_test.cs", "ABocgqS2oqKCgoKUpIKClIKCgpSCgpSClIKCgpSCpoKCgoKUgoK0koKSgqaCgpSkgoKUgoCCpIKCgpSCtJKipLaCpIKClKSCgpSUgoKUgpSAgqSClIKUgIKkgqaCgpSClICCpIKmgoKUgpSAgqSCtOaCpLaigqSCgpSCpoKClKSCgpSCpoKClIKCtA==", "20-93:1;21-64:1.1;34-37:1.1.1;94-165:2;174-208:1", "", "", "47:33,90:25,100:25,136:25,143:25,231:25,246:25")]
[assembly: GoPositionMap("net/rawconn_windows_test.go", "rawconn_windows_test.cs", "AAwagoKCkoKCgoKCgoKUgpSmgoKSgoKCgoKUgpSmgoKCkoKCgpQAFSyCgoLIgpSmgoKCpKSCgsaUgsaCxraClA==", "16-25:1;34-41:1;50-85:1;99-102:1;106-108:2;110-112:3")]
[assembly: GoPositionMap("net/resolverdialfunc_test.go", "resolverdialfunc_test.cs", "ACUugriCttyCgqaCgqaCgriClKKCgpSCuKKCgpQACxyCgoLcgoKClIKmgoLcgoKmABMogoIABxgACg6ClOyigoKC/KKCgoIABxCigoKUgtyClKiCAAwegoKCgoKUgqbCgpSCgoKmgoKUlIKCgpSCpoKCgoKUlIKCgoKCgoKCgoKUgpSCxoLGgsiCgqaCgoKUAAkKwpSClKSkpA==", "27-30:1;31-34:2;37-41:3;42-46:4;47-51:5;57-65:6;67-92:7;105-118:1", "", "39=newResolverDialFunc/1/1/2;48=String/1/1/1")]
[assembly: GoPositionMap("net/sendfile_test.go", "sendfile_test.cs", "ACY+goKCgpIAChTygoKCgpSCkpS8goKUgoKClIKCgpSCgpSCtKSCgpSC1LiCpoLKpsSigqSCpIKCgoKmwoKkkoKClKiiooKCgqamgoKmgoK02oKClKSCgoKmgqaCpoK05KKCpIKkgoKCgqbCgqSSgoKUpKailIKCxraCgpSkgpSSpoK0pKKCpIKUgqSCgoKCpsKCpJKCgpSSgoKmopSCgrS2goKUpIKUgqaCtNaytraUgqSCgpSSpJSCgvaCgoKClNqCgoKmoqSCxIKCgrSkgtaCgoKClJKiyKaCpoLWgoKiyIKCgoKmoqaygqSCwpKCpoKClMiCpoKClOyCgqaClOaCgpSkgoKUgqaCtOTCupSCpIKClKSUgoKilIKCgpSCxqSClIKylIKCgpSkgoKClKSCgqaCgvqCgpSkgoKUgqaiAAgEgqamgJKAtoKCgrgACRLCgpSCpIKCgpSkgoKmgoKUgrTkgoKClJKmgpSCpqaCgoI=", "34-34:1;35-37:2;54-56:1;63-71:2;117-160:1;126-159:1.1;140-149:1.1.1;194-226:1;203-225:1.1;217-219:1.1.1;254-286:1;263-285:1.1;278-280:1.1.1;330-351:1;354-361:2;364-375:3;387-393:4;413-450:1;414-417:1.1;492-504:1;510-537:2;563-563:1;564-564:2;616-618:1", "574=net.sendFileBench.benchSendFile", "", "143:21,161:33,163:33,172:33,211:21,244:21,262:33,264:33,273:33,299:21,322:21,343:33,345:33,354:33,380:21,416:21,423:21,425:21,446:29,493:29,545:21,564:29,577:29,603:21,638:21,645:21,686:29,694:29,716:21,780:21", "72:21,73:13,554:29,555:21")]
[assembly: GoPositionMap("net/server_test.go", "server_test.cs", "ABMaACdSkpSy0oKmgoKClKaCgoKCpoKCgpSCgpCSgriCgoKClIKCgoKUlJKCppKCppKCxszyAAcUlIKCgqaCgoKUpoKCgoKmgoKClIKCkJKCuIKCgoKCgpSmgqaSgqaSgqaSgtj0AChagrKygpSUgoKClKaCkoKAkoKmgoKClIKCgoKClJSSlIKCgpSUkoKClKaCtIKUgsaClILoAAoIggAGFrKygpSUgoKClKaCkoKAkoKmgoKCgoKClJSCkpSCgoKUlIKSpoK0gpSCxoKUgug=", "58-120:1;73-77:1.1;85-85:1.2;155-159:1;167-167:2;253-325:1;270-270:1.1;342-408:1;359-359:1.1", "", "156=testUnixAddr/1/2/3,testUnixAddr/2/2/6;392=testUnixAddr/1/4/5,testUnixAddr/2/4/5,testUnixAddr/3/4/6,testUnixAddr/4/4/6", "124:33,219:29,312:29,336:33,347:33,422:29,443:33,455:33", "88:29,89:21,182:25,183:17")]
[assembly: GoPositionMap("net/tcpconn_keepalive_conf_posix_test.go", "tcpconn_keepalive_conf_posix_test.cs", "AAsW")]
[assembly: GoPositionMap("net/tcpconn_keepalive_conf_windows_test.go", "tcpconn_keepalive_conf_windows_test.cs", "ABcuyoI=")]
[assembly: GoPositionMap("net/tcpconn_keepalive_posix_test.go", "tcpconn_keepalive_posix_test.cs", "AA0esoKClIKClIKClIKClNymgoKUgpSClIKUgpSCpoKClIKmgoKUgqaCgpSCpoKClII=")]
[assembly: GoPositionMap("net/tcpconn_keepalive_test.go", "tcpconn_keepalive_test.cs", "AA4copSCpJiCpoKCgoKUpqaCkoCCtrKmgoKUpIKmgoKUgJKU1qSilIKkmIKmgpKCgpSUsqaCkoCCpIKCgpSkgpKClIKClICSlNakopSCgoKClKaCkoCCpLKCgoKUpIKCppiAgpSkgqaCgriCpriAkpTW", "17-19:1;18-18:1.1;24-26:2;28-36:3;64-66:4;75-77:1;76-76:1.1;82-84:2;87-93:3;119-121:4;130-138:1;161-163:2;184-186:3", "", "", "44:21,63:25,120:25,132:25,136:25,174:21,190:25")]
[assembly: GoPositionMap("net/tcpsock_test.go", "tcpsock_test.cs", "ABcqgqaCpoKmgtaCgpSmgoKUpoKClKaCgpSmwpSCgoKCgoKCgpSCpoKCgoKUlIKCgoKCgqaUgoKUkpTCgoKClJTSkoKUgpSCgoLGtoKClOKSlIKCgpSSgpSCgoLGpIKCtKSCpoKClKbC7pSCgqaCgoKClJKCsoKCgoKUlJSCgoKUlJSUgoKUooKCgoKClIKCgoLGtqaygoKCgoKClLS2soKCgoKClIKCgoKmoraigoKCgoKCpqKkogAKEgAcOqKCkJKUgoKCgpSCgoLY9AAFEqKUgoKClJKCgsakopSCpoKCpoKUgpKCkJKClICCtoKClJKAgqSAgraClIKCpoLGpKKCgoKUgoKCgpKCgoKUlKaCgoKCgoKUpoKCgpSCtKSi2siUkoKUkoKCooKClIKClJKClJSCooKClIKCpoKmgoKCsoKCpsKCgpSCpoK0pIKCgoKCpoKCgoKUlIKCgoKCgqamgoKUlOKSlIKCgqbCgoKCgsbEkpTikpSCgoKUkoKCgsakgpSC2qKCpoKigqSCgoLCgoKCgpSCgoKCgpSUgpSipIKClIKCgpSCosiipLaCpIKSgqbCgoKClKSCgoKCpqKkgoKUpIKClKSCkoK4pLKCyoKCgpSCgoK0pMKCkoKCooKClIKClJKClIKCgoKCgrSkooKkgoCSgLSCgpSkgrSk6oKkgpSCgoLSlIKCgrS2goKUlIKCgpSCgoKUtA==", "82-89:1;90-100:2;108-132:3;116-130:3.1;117-120:3.1.1;137-156:4;138-140:4.1;198-208:1;224-239:2;245-256:3;259-275:4;278-289:5;332-332:1;393-393:1;437-446:1;488-492:1;504-513:2;521-526:3;527-535:4;549-556:1;557-567:2;575-595:3;576-578:3.1;585-593:3.2;598-614:4;599-601:4.1;631-671:1;638-658:1.1;685-689:1;690-706:2;721-725:3;729-732:4;753-757:1;781-781:1;782-782:2;782-782:2.1;809-817:1", "", "373=IPv4/1/3/1,IPv4/2/3/2,ParseIP/1/11/4,ParseIP/2/11/5,ParseIP/3/11/7,ParseIP/4/11/8,IPv4/3/3/10,ParseIP/5/11/11,ParseIP/6/11/17,ParseIP/7/11/18,ParseIP/8/11/19,ParseIP/9/11/20,ParseIP/10/11/21,ParseIP/11/11/22,Error/1/3/24,Error/2/3/25,Error/3/3/26;435=IPv4/1/1/4", "116:21,178:33,239:21,452:25,482:25,502:25,601:21,616:21,620:21,641:21,728:37,763:29,798:29,808:37,862:21,882:29,902:21,909:21,920:21,954:21,969:21,973:21,991:21,1007:21,1026:21", "138:37,139:29,169:33,170:25,407:21,408:13,714:25,715:17,754:29,755:21,866:21,867:13,913:21,914:13,997:21,998:13")]
[assembly: GoPositionMap("net/timeout_test.go", "timeout_test.cs", "ABQm7oKCggAICgAMIKKktpSCkoK4suqClKqCgqaUgoKUggAMGoKUgpSCgpSCgriCgoK4gpSC2PSigpKCuAAHEKKCgpSCgpSC2NSClKS2yoKilIKk7oLcooKm0oKCggAHEIKUgqKkpoKUlIKClJSClIK0yKKUpLaCpIKUgpSCgqaClIK0pKKUpLaCpIKCsoKUgpSCgqaCpLSClLSCxOQABxiyooKCgpSCspKCkoKmgoKUpKKClIKCgoKCgpSCpoKClIKU2KaylKS2gqSCgpSkgoKygpSClIKUgoKCpoKktICCpLSCgoCCpILWpAAHFqKCgpKCpoKSgqaCgpSCgpSSlKKClIKCgoKCgpSCpoKClIKU2KQABxiylIKkooKClKSClIKCgoKClIKmgoKUgpTYprKUpLaCpIKClKSCgrKClIKUgpSCgoKCgsqCpLSAgqS0goKAgqSC1qSilIKkgoKmyoKygoKUpIKUgoKCgoKmgoKUgriCgpSClIKUgpSClLTEABIsogAJEriqwoLKgqiygqSCgpSkgoKCgoKCgpSCgpSClIKUgqaCgpSClIKCgriCgqa0prKCpIKClKSCgoKCgoKClIKClIKUgpSCpoKClIKUgoKCuIKCprSkoqS2gqSCgpSkgoKCgoKClIKCgoKCgoKmlIKUgpSCpoKClIKUgsqCuJSCuIKUprSmkoLYxIKUgqKmxIKUgqKkooKCgoKUgqaCkoCCtgAUKoKCgoKmgoKUgoKmgoKUgoKUgpTYqMKUpLaigoKClKSCgrKCgoKCgpSClIKClOiygoKCgoKUgpSCgpToopKCkoKmgoKUpIKCgrSmspSCgqaCpIKClKSCgrKCgpKCgoKUlIKClJS0krKCgoK0krKCgoK0kqKmsoKkgoKUgpKCgoKUtoKCgoLCpoKCgsakoqqygoKUgpSClA==", "26-31:1;59-63:1;66-133:2;139-143:1;153-165:2;183-247:1;203-206:1.1;208-225:1.2;294-304:1;334-342:1;404-418:1;455-459:1;578-596:1;636-681:1;956-965:1;1036-1085:1;1046-1064:1.1;1065-1083:1.2;1125-1144:1;1145-1151:2;1152-1158:3;1185-1195:1", "", "", "222:29,251:41,313:21,351:21,372:21,420:29,427:21,437:21,488:21,495:21,520:21,564:21,573:21,587:21,640:21,650:25,697:21,704:21,733:21,772:21,794:33,901:21,908:21,965:21,972:21,1035:21,1042:21,1166:21,1253:29,1314:21,1324:21,1349:21,1356:21,1425:21,1440:25", "67:21,68:13,161:21,162:13,240:33,241:25")]
[assembly: GoPositionMap("net/udpsock_test.go", "udpsock_test.cs", "ABoqwpSClIKClIKCpoKClJKCgpSkgoKClILGAAoSABw6ooKQkpSCgoKClIKCgtikoqS2gqaCgpSkgqKkooKClKSCgpSUgoKCuIKCgoKCgoKCgpSCgoKClIKCgoK0pKKCgpSkgoKmgoKClIKClIKClIKClIKUgoK09AAFEoKUsrKCpoKClJKCgrQACQiigqaCgoKUpIKCgoKmgoKUpAAFEoKC2KSilIKmgoKmgpSCkoKQkoKUgIK2goKUkoCCpICCtoKUgoKmgsYACASipLSkgqaCpIKCgpSClIKCgoKClIKUgsakoqSkgqaCpIKCgoKUgpSCgpSUxoLopKKkpIKmgqSCgpSkgoKCgpSClIKClJSClILGqMKCpoKClIKClKSCgoKClIKUgrTkora0praClIK4lIKClJKCgpSygoKUgoKmgIK2soKClIKCpoCCtrKCgpSCgqaAgsSkwoKClJKCgoKCgoKClIKCxqTCgoKUkoKCgoKCgoKUgoLGpMKCgpSSgoKCgoKCgpSCgsbkoqSkgqaCgpSSgoKCgpSCgpSClIKClIKClIK06tKClIKm7LaCgpSkgoKClIKCpoKCpoKCtKrSpLaCpoKClKSCgoKUgqaCpoK0rvKktoKmgoKUpIKClIKClIKmgoKUgrQ=", "96-96:1;147-152:1;230-244:1;308-308:1;521-530:1;535-544:2;549-558:3", "", "81=IPv4/1/3/1,IPv4/2/3/2,ParseIP/1/11/4,ParseIP/2/11/5,ParseIP/3/11/7,ParseIP/4/11/8,IPv4/3/3/10,ParseIP/5/11/11,ParseIP/6/11/17,ParseIP/7/11/18,ParseIP/8/11/19,ParseIP/9/11/20,ParseIP/10/11/21,ParseIP/11/11/22,Error/1/3/24,Error/2/3/25,Error/3/3/26;266=IPv4/1/1/4;338=LocalAddr/1/2/4,RemoteAddr/1/2/5,LocalAddr/2/2/6,RemoteAddr/2/2/7", "51:21,57:21,157:21,174:21,225:21,292:29,322:25,336:25,374:25,394:25,448:21,492:21,540:21,547:21,594:21,650:21,726:21,756:21,786:21,828:21,891:21,939:21,986:21", "115:21,116:13")]
[assembly: GoPositionMap("net/unixsock_test.go", "unixsock_test.cs", "ABQoooKUpKSCpoKCgpSCgpSigqaCgtKQkoKCgpSCgoKCtKSCgoKCgpSClIK0pKKCpoKCpIKClIKkgoKClIKUgoKCgpSUlILGguikooLKgoKkgoKUgqSCgoKClIKUgoKClJSUgsaC6KSigqaCgoKUgoKUgqSCoqSigoKUpIKCgoKUgoKClIKCgpSCtKSigoKClIKkgoKUgpSClIK05KKCppKygoKClIKClIKSgqaCgpSCgpTSgoK0koKmpIK2yoKC2OSigqaygoKClIKClJKCpoKCgqaCgpTCgoK0pKSCyNyCgtgAIASCgpSktpSCgoKUlIKCgpSCgpSUgoKmgoLKsoKCgrjCgoKCgoKCgoK4soKCgoKClIKCgsqygoKCgqaigoKCgoKmwoKCgoKCgoKCprKCgoKCgoKCgg==", "41-44:1;48-61:2;49-49:2.1;248-248:1;273-278:2;317-320:1;332-337:2;371-377:1;378-388:2;389-393:3;394-398:4;401-406:5;409-419:6;422-434:7;438-444:8;446-453:9;455-465:10;467-477:11", "", "369=Addr/1/1/1,LocalAddr/1/1/2,RemoteAddr/1/1/3;448=LocalAddr/1/2/1,RemoteAddr/1/2/2,LocalAddr/2/2/3,RemoteAddr/2/2/4", "109:21,117:21,166:21,174:21,229:21,246:21,282:21,330:25", "48:21,49:13,62:29,63:21,346:25,347:17,410:25,411:17,427:25,428:17")]
[assembly: GoPositionMap("net/unixsock_windows_test.go", "unixsock_windows_test.cs", "ABYiooKUkrKCgoKUgoKUgpKCpoKClIKClNKCgrSSgqaClMqCgtikgoLKgpSCgpSCgpSmooKmlIKClKSCgqaCgrQ=", "21-21:1;46-51:2", "", "80=Addr/1/1/1,LocalAddr/1/1/2,RemoteAddr/1/1/3", "46:25,131:21", "62:25,63:17")]
[assembly: GoPositionMap("net/writev_test.go", "writev_test.cs", "ABckgoIABxCCgpSClIIACQiCACRSgoKCgsqCgoKCAAoMooKQkoiCgoKUgoKmkoKClIKCgpSUgpSClIKUlIKCpoKCgqaCpIKCgoKClILGgoKClIK2gpS05KKCppSCooKCuMKCgoKClKKSgoKUkoKClMqCgoKUgrQ=", "94-96:1;103-103:1;108-112:2;118-140:3;140-179:4;190-195:1;197-205:2", "", "", "233:29,249:21", "119:21,120:13,221:21,222:13")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("net")]
public static partial class net_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("test")] partial struct TestDialListenIPArgs_test {}
    [GoValueClone("args")] partial struct TestDialListenIPArgs_type {}
    [GoLocalName("test")] partial struct TestDialerLocalAddr_test {}
    [GoLocalName("test")] partial struct TestLookupPort_Minimal_test {}
    [GoLocalName("test")] partial struct TestLookupPort_test {}
    [GoLocalName("test")] partial struct TestLookupProtocol_Minimal_test {}
    [GoValueClone("xerrs")] partial struct readTimeoutTestsᴛ1 {}
    [GoValueClone("q")] partial struct resolverFuncConn {}
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
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸrace() => builtin.initPackage(typeof(@internal.race_package));
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
