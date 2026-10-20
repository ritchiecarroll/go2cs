// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.crypto.tls_package;
using static go.crypto.tls_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b41637475616c20737472696e6720226a736f6e3a5c2261637475616c5c22223b20457870656374656420737472696e6720226a736f6e3a5c2265787065637465645c22223b204973556e657870656374656420626f6f6c20226a736f6e3a5c2269735f756e65787065637465645c22223b204572726f7220737472696e6720226a736f6e3a5c226572726f722c6f6d6974656d7074795c22227d", "bogoResults_Tests")]
[assembly: GoDynamicTypeLift("7374727563747b616c676f20737472696e673b206365727420737472696e673b206b657920737472696e677d", "keyPairTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e205b5d627974653b20676f6f6420626f6f6c3b2065787065637465644c656e20696e747d", "paddingTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f757420737472696e677d", "hostnameInSNITestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6d752073796e632e4d757465783b2061646472206e65742e416464723b206368206368616e206e65742e436f6e6e7d", "localListenerᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73657475702066756e63282a63727970746f2f746c732e436f6e6669672c202a63727970746f2f746c732e436f6e666967293b206578706563746564436c69656e744572726f7220737472696e673b207665726966792066756e63282a74657374696e672e542c20696e742c202a63727970746f2f746c732e436f6e6e656374696f6e5374617465297d", "getClientCertificateTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73657475702066756e6328636f6e666967202a63727970746f2f746c732e436f6e666967293b2063616c6c6261636b2066756e6328636c69656e7448656c6c6f202a63727970746f2f746c732e436c69656e7448656c6c6f496e666f2920282a63727970746f2f746c732e436f6e6669672c206572726f72293b206572726f72537562737472696e6720737472696e673b207665726966792066756e6328636f6e666967202a63727970746f2f746c732e436f6e66696729206572726f727d", "getConfigForClientTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<base64Flag, flag_package.Value>(Pointer = true)]
[assembly: GoImplement<brokenConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<brokenSigner, crypto_package.Signer>(Promoted = true)]
[assembly: GoImplement<changeImplConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<discardConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<go.crypto.tls_internal_test_package.zeroSource, io_package.Reader>]
[assembly: GoImplement<hairpinConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<intSlice, flag_package.Value>(Pointer = true)]
[assembly: GoImplement<messageOnlySigner, crypto_package.Signer>(Promoted = true)]
[assembly: GoImplement<opensslInput, io_package.Reader>]
[assembly: GoImplement<opensslOutputSink, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<readerFunc, io_package.Reader>]
[assembly: GoImplement<recordingConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<replayingConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<serializingClientCache, global::go.crypto.tls_package.ClientSessionCache>(Pointer = true)]
[assembly: GoImplement<slowConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<stringSlice, flag_package.Value>(Pointer = true)]
[assembly: GoImplement<writeCountingConn, net_package.Conn>(Pointer = true)]
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
[assembly: go.GoPositionMap("crypto/tls/auth_test.go", "auth_test.cs", "ACIcgrjKuMoAFkCCgoKUgpSCgpSClIKClIKUgqam3AAeRoKCgsqCgoKUgIKkgIK2goKUgIKkgILIgoK8ooKCgpSClII=")]
[assembly: go.GoPositionMap("crypto/tls/bogo_shim_notunix_test.go", "bogo_shim_notunix_test.cs", "AAoSgg==")]
[assembly: go.GoPositionMap("crypto/tls/bogo_shim_test.go", "bogo_shim_test.cs", "AC5MgpSUgoKClJSCgpSCgoKUlJSUgpSCgpSClIKCgoKCgoKCgoKCgoKCgoKClJSCgoKUgoKCgoKUlIKUuoKCgqaCpoKCyoKCgqaCpoKCgpSCyoKCgqaCpoKClIKClIIADgaigoKmlpKCpgAGGISUlIKCgpSmgqa4goKCgoKCgsiCgoKCgoKC3IKCgoKCuIKmgpSCpoKmgpKClIKUuIKCgpSCgriUgoKClIKCuKaCgoKClIKCgpSCpoKUgqaCgqaCgriCgriCgqaCgoKUgoKmAAcQgoKmgoKUtoKCgIK2goKUpoKAguyCpoKCgoKClIKClICCtsqUgoKUgpSCgoKUgqamgoKCpoKmgpSClIKSgpKCkoKmgqaCpoKmgriCgoKUgsqCgoKUgtiuAAgElIKCpoKCgoK4AAoGgoKUgpTcgIK2goKClKaCgqaUAAkUgqaCgu6CgoKUpoKAgraCgIL+ABEkxKKClIKUgIKkgoK4lIKCAAwW0pSAgoKmgpKCtpSCgpSUgoKAgraCgoCCttai3JKCgpSk", "202-223:1;286-298:2;299-311:3;641-655:1;660-662:2", "", "214=NewLRUClientSessionCache/1/1/6;619=Join/1/1/3,Sprintf/1/3/3,Executable/1/1/4,Sprintf/2/3/4,Sprintf/3/3/8;783=Unix/1/1/2,Format/1/1/2")]
[assembly: go.GoPositionMap("crypto/tls/cache_test.go", "cache_test.cs", "ABUagpKCgqaCgpSCgpSCpoCCtoKCgoKktIKCAAsW7oKClMqCgpQ=", "36-59:1")]
[assembly: go.GoPositionMap("crypto/tls/conn_test.go", "conn_test.cs", "ABMcgorKpuQADiaCgoKUgoKCgpSClIIAEBaCAA4clIKCgqamgqaAgoKUyICCpICCpICCpICCyrKUgoKUgoKQkgAICIKkgoCCgqSUgoKUgoKClIKCpoKCpoKCgqamoqSAgqSm3ICCpICCtoKCyriCgoKClIKkgriCtKSClIKCgqaClIKCgqaCgoKCpoKCACUUgoKmxoKSpIKUpqai1KKCkqSClKKCgIKCpIKmgoCCgraUgoK0qMKCkqSSgoCCgqSCgoKUgoKCpoKAgqSCgILE", "92-99:1;101-111:2;137-137:1;138-181:2;283-285:1;301-309:1;332-348:1", "", "21=roundUp/2/5/1,roundUp/3/5/2,roundUp/4/5/3,roundUp/5/5/4;85=fromHex/1/3/3,fromHex/2/3/6,fromHex/3/3/9;227=Repeat/1/3/1,Repeat/2/3/2,Repeat/3/3/3")]
[assembly: go.GoPositionMap("crypto/tls/ech_test.go", "ech_test.cs", "ABUcgr6CgpSCgpSC/IKCgpSCgpSCggAJCIIACBKUgoKCgoKClIKUgoKCgpSCgoK4ppKCkpKC3AAOENzu7u6C7oKCggAIEoKCAAgMsoKCgoKkgg==", "64-89:1;69-71:1.1;72-74:1.2;75-87:1.3;78-80:1.3.1;82-86:1.3.2;83-85:1.3.2.1;91-100:2;93-99:2.1;94-98:2.1.1;109-111:3;115-117:4;122-124:5;129-131:6;136-139:7;144-151:8;146-150:8.1;147-149:8.1.1;156-161:9;158-160:9.1;165-175:10")]
[assembly: go.GoPositionMap("crypto/tls/fips140_test.go", "fips140_test.cs", "ACo+goKClKaCgoKmpoKCgpSCgpTWgoKAgqSCgoKCgoIADAyCgoKCgoKCgoKCgqaClILckoKCgqaSgoKCpoK4gvaCgoKUpKS2mriUlKSkpMiClLakpLiCgoKmpoIAARS2pKTogoKUgoKClIKUgpIAChaCpqKmooKClMqCuIKCgpSCkoKUooCC2qKCgpKC3IK4soKCgoKygpSCgoLWooKUgpSUgoKClJSClIKCtIKCtIKCpKaUkoKCypKCgoKmgu6CtKSCpsiCkqSUgoKClKKCgoKUgoKmgpSCgqaCgqaCgqaCgsYAEgSmgsqCgpSClIKCgpSClIKmgoKCgpSCgpSUgoKUpoKU3IKCgpSCgpSUgoKUpoKU7oKU0oKm0oKmgqaCgqaCkoKUgoKCkoKigoKCgpSCuIKCgoKClIKCgoKCsoKCgoKUgriCgpSygqayggAKGoKCgpSmgoKClAAMGqKSkoCCgqQAChiClIKmgoKCgpSCpoKClIK0gsS2goKUgoKmkqKCuAAHFIIAFSiUAB04goKC", "78-99:1;79-98:1.1;101-106:2;108-113:3;217-244:1;233-235:1.1;237-243:1.2;258-277:1;262-266:1.1;269-276:1.2;290-293:1;301-303:1;304-306:2;310-356:3;336-341:3.1;344-355:3.2;447-472:1;475-499:2;506-509:3;511-514:4;535-546:5;559-570:6;575-578:7;580-583:8;680-684:1", "", "255=defaultCurvePreferences/1/1/5,generateKeyShare/1/1/6;749=NewInt/1/1/1,Unix/1/2/5,Unix/2/2/6")]
[assembly: go.GoPositionMap("crypto/tls/handshake_client_test.go", "handshake_client_test.cs", "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", "300-304:1;346-373:2;390-405:3;628-633:1;799-804:1;844-844:1;845-845:2;879-894:1;896-898:2;899-902:3;903-905:4;906-912:5;923-923:6;931-931:7;954-954:8;958-958:9;966-966:10;1172-1180:1;1189-1205:2;1223-1231:1;1240-1246:2;1262-1268:1;1281-1288:1;1341-1356:1;1372-1380:1;1423-1435:1;1445-1452:1;1485-1487:1;1524-1530:1;1568-1568:1;1569-1569:2;1573-1597:1;1606-1618:2;1608-1617:2.1;1619-1641:3;1620-1640:3.1;1645-1658:4;1648-1657:4.1;1659-1682:5;1661-1681:5.1;1686-1692:6;1688-1691:6.1;1693-1698:7;1694-1697:7.1;1702-1708:8;1704-1707:8.1;1709-1732:9;1711-1731:9.1;1770-1788:10;1795-1795:1;1796-1796:2;1811-1820:1;1821-1833:2;1841-1846:3;1843-1845:3.1;1847-1852:4;1849-1851:4.1;1853-1866:5;1869-1874:6;1871-1873:6.1;1875-1877:7;1878-1882:8;1885-1887:9;1888-1892:10;1889-1891:10.1;1893-1897:11;1900-1902:12;1903-1918:13;1905-1917:13.1;1919-1929:14;1932-1937:15;1934-1936:15.1;1938-1943:16;1940-1942:16.1;1944-1957:17;1960-1965:18;1962-1964:18.1;1966-1969:19;1970-1974:20;1977-1980:21;1981-1986:22;1983-1985:22.1;1987-1991:23;1994-2002:24;1996-1998:24.1;1999-2001:24.2;2003-2007:25;2008-2015:26;2018-2022:27;2023-2031:28;2025-2027:28.1;2028-2030:28.2;2032-2039:29;2049-2066:30;2114-2118:1;2145-2145:1;2146-2146:2;2156-2162:1;2206-2210:1;2240-2253:1;2260-2264:2;2266-2274:3;2369-2369:1;2370-2370:2;2410-2420:1;2487-2491:1;2504-2504:1;2553-2553:1;2554-2554:2;2643-2647:1;2696-2699:1;2706-2721:1;2860-2862:1;2867-2869:2;2876-2878:3;2883-2885:4;2889-2912:5;2893-2897:5.1", "", "234=BEUint16/1/1/1,Sprintf/1/1/1;276=IPv4/1/1/1;1014=NewLRUClientSessionCache/1/1/3;1663=Clone/1/1/2;2048=NewLRUClientSessionCache/1/1/2;3081=NewLRUClientSessionCache/1/1/2;3400=NewInt/1/1/1,Invoke/1/2/4,Add/1/2/4,Invoke/2/2/5,Add/2/2/5")]
[assembly: go.GoPositionMap("crypto/tls/handshake_messages_test.go", "handshake_messages_test.cs", "ABcoAAQsgoKCgpSmgpSCgrKCgpSCgoKCpoKCgoKmgNyClAAIEJS0toKCptyCgoIACBKCgoKCgpTKgoKAgqSmgoKmooKCgoKCgoKClJSCgoKCpoKCgoKUgoKClKaClIKUgpSClIKClIKUgpSClIKCgoKUlLTsgpSClIKCgoKUgpSCpqaigoKCgoKClIKUgpSCpoKmgoKUgpSClIKUgoKCpIKUgoKUgpSCpqailIKUgqamooKCgoKUpqKCgoKUpqKCgoKCpqKCgqaigoKmgoKCpqKCgsqCgoKUgoKClKaigoKClJSCgoKCgpSClIKUgoKUpoKUgoKmgoKClLiClIKCgqaUpoCigoKClIKmgoKmooKCpqKCgoKCgoKUpqKCgpSClIKUgpSCgoKmpqKCgqaCgpSCgoK49paCgsqUgoK4goKmgpSCpoKCpoKUguiogsqUgoL4goKClIKCpoKClIKCABEIgpQAK1yilIKClIKClIKCpoKClIKmgpSC", "56-118:1;646-677:1", "", "531=Intn/1/1/1,randomBytes/1/1/1;541=Intn/1/1/1,randomBytes/1/1/1")]
[assembly: go.GoPositionMap("crypto/tls/handshake_server_test.go", "handshake_server_test.cs", "AC1CgoKqooKmgoKSgoCCpICCpJSCgoKC7IKUgpSCptqClIKmgoKCgqSC6IKmxIKCgrLK7oLc1oLcpoLcpoLWgu6mAAcQgoKCgoKUuIKmpgAHEIKUuIKCgoKmgpQABxCClLKCgoCCtoKCgpSCpoKUgsqUpoKmgriiuAAPIIKCsoKCgIKkgoKClKaCgoKCgoCCpIKClICCABAIxgAGGsSCgpSSAAgSgoKygoKCgoCCpIKCgpSmgoKCgoKCgIKkgoKUgoKmgu6CgpKCpoKCgILIgoKkgoKCuIK4uIKClIKmgoKCggAICIKU3IKUgpSmuIKClIK4goCSgLaCggAIEKaCgpSCgpSCgsqCgJKAtoLKAAkUgoKCuIKClIK4goKClIK4goKUgriCgoKUggAgQsoACgK4gpSklIKCgpSCgoKCgoKCgIK2gqKCgoKUpoK0gIKktKa45oKm4oKClJLmwoKClIKCgoKUgpKCuIKClJSCgpSUgoKCkoCCtoKmlIKCgoKCgrimgoCCyIKCgoKClJKCgoKmgoK0pLSSgpSCgpSClKaCpoKmgqaCpoK4goKmgriCpoK4gqaCuKaCuKaCuKSCuKSCuKaCgoKCgpTKgoKmgoKUyoKmgoKUyoLmgoKUgoKSgoKCgqbKgpSClIKUpuyygpiigpKCgoKCpsqClIKUgpSmpoKClAAHEIKUpoKmgoKUAAcQgqaCgpTugpSmgqaCgpTugpSmgqyyuKqipoKCkoKUyq7ClIbKAAgMsoa0grKSggAMGv6CgriUgoKClJSUuILsopSCmO4ACAqilIKGlNy4lNzWooKUyriClLiClIKUgpTcgpS4otSigpSU3MqClLiCgoKUgoKCoqSCyO6mgsqAgpKCpKaCgqaCuKbYuIKUyqaCgoKCgpTKgqaigoKCgoKClIKCkoKCgoKUgoCCpIKUgoKCgoCCAAwKgoKmgoKmgriCgqaCuIKCpoK4goKUgqaC3KLkgoKCgoKCgoKCgoKClKaClMqClO6ClO6ClO6ClO6iAAkEopQABxCUlIKigoKAgqSUgoKC2oKUgpSkgqaCgqaCtAAJBAAPMqaC7oKUpoKCgriClMqCgqamgoK4gpTKgoKClLKCpoKSgoKUgpTSkqKkgpSUgoKUgoCCyIKSgtyCgoKigpSCgoKAgrbogoKCgoKCgriCgpKCABIIggAMJIKmgoKCsoKCgoKCgpSCgqaCgoKUggAIEIKClICCAAwIwpSCgKQAZdgBspKCAA8ggoKmgtgACQTClIKApAA6gAGykoKCAA0cgoKmgtjq0oKCgoKygoKUpoKClIKUgpSCgrSu8oKCgoKCkpKC8oKSgoKSgIKkgsqCgoKClNbEkoKCgpKAgqSCyoKCgpTWpICCxOSCgpSCAAgIgqSS7oKClIKCpu6CgpSCgoKmlKLClIKCyIKCgpaClIKCyIKCgoKCgqiCgoKClIKUgriClKSCyJaClLiCpoKClIK4gpLugoKUgoKUgoKUgoKm7oKClIKCpoKCyJaCgoKCooKUgoKClIKUgoLIgoKCloKUgoKCgpSClIK4gpS4lIKmgoKUgriCku6CgpSCgpSCgpSCgqbugoKUgoKmgoLIloKCgoKUgoLIgoKCloKUgoKCgpSClIK4gpSCgpSUgg==", "47-56:1;229-243:1;291-304:1;344-394:1;356-371:1.1;400-403:1;456-464:1;480-480:1;481-481:2;513-513:1;514-514:2;641-648:1;693-697:1;913-917:1;923-934:2;952-956:1;962-973:2;988-994:1;1026-1031:1;1047-1052:1;1077-1080:1;1096-1098:1;1118-1158:1;1149-1156:1.1;1172-1174:1;1192-1194:1;1230-1235:1;1252-1257:2;1279-1284:1;1318-1325:1;1386-1392:1;1411-1414:1;1415-1424:2;1416-1419:2.1;1420-1423:2.2;1425-1434:3;1426-1429:3.1;1430-1433:3.2;1435-1444:4;1436-1439:4.1;1440-1443:4.2;1445-1457:5;1449-1452:5.1;1453-1456:5.2;1547-1554:1;1675-1679:1;1683-1686:2;1715-1718:1;1775-1783:1;1816-1816:1;1927-1953:2;1961-1961:1;2028-2053:2;2065-2069:1;2103-2129:1;2108-2117:1.1;2133-2142:2;2159-2161:1;2162-2164:2;2170-2205:1;2206-2273:2;2207-2272:2.1;2219-2221:2.1.1;2237-2239:2.1.2;2241-2253:2.1.3;2265-2267:2.1.4;2280-2282:1;2283-2285:2;2336-2338:1;2343-2351:2;2364-2366:3;2370-2382:4;2398-2400:1;2401-2403:2;2454-2456:1;2472-2474:2;2478-2490:3", "", "612=NewLRUClientSessionCache/1/1/3;712=IPv4/1/1/1;1283=PublicKey/1/1/6,Bytes/1/1/6;1500=Clone/1/1/3;2020=testingKey/1/1/9;2329=PublicKey/1/1/9,Bytes/1/1/9;2529=Add/1/1/2;2545=Add/1/1/3;2656=Add/1/2/2,Add/2/2/3;2680=Add/1/2/3,Add/2/2/4;2777=Add/1/2/2,Add/2/2/3;2801=Add/1/2/3,Add/2/2/4")]
[assembly: go.GoPositionMap("crypto/tls/handshake_test.go", "handshake_test.cs", "ADNcgoKCgpSWlJSCgpSmgoIACg6igqaCgoKmgoKmgoKCgoKCgoKUACQa4oCCpJSClICCgoKUpILW4oCCpJSClICCgoKUpILYpIKCgpKClIKCgpSCgoKClIKClJTWopSCgqaCgoKU3IKClJSCgpSUgoKCgpS4gqYAChaU4oKUgoKmgoKCgoKUpubigpSCgqaCgpSCgoKU5tKClIKClNaAooCigKKAooDWkoKClIKCggAOGoKCgoKUgoKUuJSigpSUgoSSgpKCgoKmgpSUgoKUgrSCgraCgJKQkpSC+oIACQyCgqaCgoKmyoKCgoKCuJSCgqbWxqaCgpSCgpSCgpKUgIKC2pQACRSCgoKCgoKCgpSCtpSU2gAIBsKCgoLCgoKCgoKUkJKCgoKUgILaopKCgoKCgIKkgIK2gpSCpoKC3JSUlJSU6pSUlJSUpLSkABIiACNAABouAAkK3oKSgoKCgqbagIKkgIK2goKmgIKkpoKCgIKSgsiWgpKUgoKmgoKmgIKkpoKCgIKSgsi6lJKUgoKUlIKCpoCCpKaCgoCCkoLMwoKCgoKUlIKCgoKCgoKCguaispSCgoKCgpSCsoCkgqaAgqSUgpSCgqaCgIKklIKCkoI=", "59-64:1;67-69:2;376-376:1;377-377:2;410-416:1;457-457:1;496-517:1;504-504:1.1;660-689:1;704-721:1;739-757:1;793-835:1;803-808:1.1", "", "596=allCipherSuites/1/1/5")]
[assembly: go.GoPositionMap("crypto/tls/key_schedule_test.go", "key_schedule_test.cs", "ABQilIKCgoKCpoKCgoKCgoK4lJSUgIKkgIK2lJSAgqSAgraUlICCpICCpICCtpSAgsySgoKUlIKCgoKUAA0GgpaWpoKCgpSC", "86-91:1")]
[assembly: go.GoPositionMap("crypto/tls/link_test.go", "link_test.cs", "ACAgkoKUlAAwYoKCgrKSgIKkgoKCgIK2goKCgpSCgqaCgg==", "75-102:1")]
[assembly: go.GoPositionMap("crypto/tls/prf_test.go", "prf_test.cs", "ABAgAAcQgoKCgoKCggATIoKCgoKUgoCCgraCgoKCgoimgoKCpoKCpoTs", "", "", "86=EncodeToString/2/2/1;94=cipherSuiteByID/1/3/3,cipherSuiteByID/2/3/19,cipherSuiteByID/3/3/35")]
[assembly: go.GoPositionMap("crypto/tls/quic_test.go", "quic_test.cs", "AB4+griSlKaCuJKUAAcQgoCCpIKUgIKkgpSYpLiCgIKkgpSYpLjkooKCgoCCyIKCgoKUgpSUgoKClJSkpKSAgsaCgrakgoKAgtiClKSCtqSClJSCyoKClIKUgpSAgraAgqSAgqSAgqSAgqSCgIKkgIKkgpSC+oKCgoKUgpSCgoKCgIKkgqaCgoKCgIKkgqaCgoKCgoCCpIK4goKCgpSClIKCgoKClIKAgoK2lJSAgsiUgoKCgoKCgIK2goKCgoKClIC4+JSCgoKCgoKAgraCgoKUgoCCkoLIgoKCgoKCgoCCtoKA3PiCgoKClIKUgoKCgoKClICCtoKClILqkoKCgoKWgoKUgIKkgoCCpIKUgIKkgr6ygoKCgoKCgqKCgIKAksaCgIKAksaUgIIACgiCgoKCgoKCgoKCgpSUgoKCgoCCpIK4goKCgoKCgoaispSC5oKCgoKUgpSClIKCgIKkgoCCpIKAgraAkqSAksiCgpSCgoKCgIK2gpSClIKUgriCgoKCgoKUgoK4goKCgoKUgoK4goKCgoKUgoKUgoKCgoKAgqSCpoKCgoKClJSUgIKkgpSCgpSCgpSCuIKClIK4goKCgoKClIKCgpSCgoKCgoCCpIKmgoKCgoKUgpSUgIKkgpSCgoKUgqaAgg==", "36-38:1;47-49:1;272-284:1;401-403:1;437-451:1;464-471:1;495-497:1;616-622:1;643-645:1;646-648:2;680-682:1", "", "32=QUICClient/1/1/2;44=QUICServer/1/1/2")]
[assembly: go.GoPositionMap("crypto/tls/ticket_test.go", "ticket_test.cs", "AAkOkg==")]
[assembly: go.GoPositionMap("crypto/tls/tls_test.go", "tls_test.cs", "AEFsAA4aAB00ABIYAAUUgoKCgoKAgqSCgIIAEwqigoKUgIK2goKUgIK2nIKClICCAAgIgoCCpICCyIKCgoKUgpTWooKCgpS4gqLkooKClKaCgoKUuIKi1IKCuIKCppKCgoKigoKCgpS4gqaAgoKSgrbugqaCuIKClIKmggAJCKKCpoKklKKCgoKUgoCCgqSmgoKCgpSkgoK4goCCpICCyICCpICCyICCpICCyICCpIK0yICq0oKkgoLCgoKUkqKkgv6CyIKCtKSCgIKkrAAJDoKUgoKAgramooKkgoKigoKCgpSCgoCCgoKkpqaCgoKUpIKCppSCgoKmgoKCgoKUgpQADAaigqSCgoKC4oKCgoKClIKCgoCCgqS0+KSCgoKCpoK02IKUgpSUgoKUkoKmtNiClIK09IKUgoKUgIKkgIK2goKUgIIACgiClMqCgoKCpoKClIKCgpSClIK41qKCpIKCgqKCgoKCgpSCgoCCgoKkpoKClKS4goKAgraCgpSUgpKCpoKCsoKCpoKygoKmgoKmgoCCxNSigqSUwoKClKSCgoCCpKSCgpSCpoCC/oLWwpSCgoKUgIKkpICCtoCCtoKClIKU1pSgkqCkgrSCxuyCgqSCgpSSlICC1gAIBKKCpLKCgpSkgoKAgqSkgoKUgoCCttaCoKSCgoKClJKAgraCpoCCxKSCgpSkgqaCpoKmgqaCpoKmgqaCpoKmgriUgoKCgoKCgoKClIK4goKUgoKmgKQACA7IpKSkpKSkpKSkpKSkpKTIpMiClIKCuIKCgIIAJBiCgpSmgoKUpsKCpJi0koKCgqaUgoKCgoCCpICC2oKCgoKUgoKCgoKUgoKClIKCprSkgoKSgoKClIIAJxiCgpSCgoKCgoKUgoKCgrimwoKklJKCgqaUgoKCgIKkuIKClIKUgoKCpoCCpICCpICCpICCpLSkgoKCgoKClILugoKCggANCIKCgpSClIKCgpSigqaCpoKmgqaCpoKmgqaCpoKSgqaCkoKSgpKCuIKCkoKSgpSCpoKUgriCgqaCyoKClIKmgpSClIKUgpSClIKUgpSClILKsoKUgpSkpJTiAAsYgoKClKKCprCSsKToooKmgqawkrAABhCiuLiClIKCgoK4gAAIBKKUuMrKygCEAZoCgoKUtLQADQqCgoKClKaCpoKCgpSmgriCgoKmgoKmpoKCgoKmgIKSgraCgqaCuICCtoKCgoKmgpSCpoCCyICCtoKUgriCgoKCgpSCyoaygoKCgqaCkoKmgpKCyoKCpIKSgqaCkoKmgpKCpoKSgqaCkoKmlIKUpJSClLiCkoKmgpKClIKUgsqCgJKkgJLMogABIqT8lNqigsiCgqaAgra6gILIgqaAkoAAEwaCACVYgoKUgpSyopSSpIKCgoKCgoKClIKClIKClIKClIKCpoKCgqaAgraCgqaClIKUgqaCgoKClIKmgpSClIKUggAZDIKClJSCAAgiAAcQAAcQAAgSAAcQAAgSAAcQpgAHEAAHEAAHEAAHEAAHEAAHEO6CgrKigpSClJSCgpSSgoaClJSCgpSCgpSClIKUgoKUgqaClIIADQ6CgoKUgoKUgriCgpSUooKCgpSCpqKCgoKUgqaigoKUggAICoKUkoCCyIKCgoKUgriCgoKUAAsYgoKmgoLcgIIADgiCgoKU3IKClIKClIKCgpSCgqaCgoKSgoKSlIKClIKClKamgoKmlIKSlJSCgoKCgoKCgoKCgoKCgoKCuLiygoKUgpSClIKUgpSClIK4lKaCgpSCuKaCgJKAkoDmgoKCgoKCgsjagoKCpoK4yNqCgoKmggAIDoLWgqaCgoKC", "185-191:1;199-201:1;206-212:2;233-242:1;294-306:1;368-375:1;379-387:2;431-446:1;492-513:1;625-641:1;666-669:2;673-677:3;680-684:4;703-735:1;737-766:2;770-770:3;771-771:4;807-831:1;834-834:2;861-864:1;865-868:2;869-872:3;873-876:4;877-880:5;881-884:6;885-888:7;889-892:8;893-896:9;897-900:10;1029-1049:1;1082-1089:1;1083-1085:1.1;1086-1088:1.2;1128-1144:1;1179-1186:1;1180-1182:1.1;1183-1185:1.2;1212-1291:1;1293-1329:2;1344-1386:3;1367-1367:3.1;1368-1368:3.2;1370-1385:3.3;1383-1383:3.3.1;1384-1384:3.3.2;1616-1628:1;1726-1789:2;1869-1869:1;1870-1870:2;1926-2006:1;1942-1945:1.1;1946-1949:1.2;1950-1953:1.3;1954-1957:1.4;2033-2035:1;2041-2043:2;2049-2051:3;2058-2060:4;2066-2068:5;2075-2077:6;2083-2085:7;2086-2088:8;2094-2096:9;2102-2104:10;2110-2112:11;2118-2120:12;2126-2128:13;2134-2136:14;2145-2197:15;2158-2167:15.1;2160-2162:15.1.1;2221-2230:1;2231-2240:2;2241-2249:3;2255-2259:1;2336-2357:1;2339-2354:1.1;2342-2344:1.1.1;2345-2348:1.1.2;2350-2352:1.1.3;2367-2369:2;2396-2419:3;2423-2425:4;2439-2439:1;2440-2440:2;2441-2441:3", "", "781=NewLRUClientSessionCache/1/1/2;1790=NewLRUClientSessionCache/1/1/10;2766=NewInt/1/1/1;2845=NewInt/1/1/1;2892=NewInt/1/1/1,Now/1/2/3,Add/1/2/3,Now/2/2/4,Add/2/2/4;2975=Bytes/1/1/1;3017=Bytes/1/1/1")]
// </GoSourcePositionMaps>

namespace go.crypto;

[GoPackage("tls")]
public static partial class tls_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("serverResult")] partial struct testGetClientCertificate_serverResult {}
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
    [GoInit] internal static void initᴛᴛimportꓸcrypto() => builtin.initPackage(typeof(crypto_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸecdh() => builtin.initPackage(typeof(go.crypto.ecdh_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸecdsa() => builtin.initPackage(typeof(go.crypto.ecdsa_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸed25519() => builtin.initPackage(typeof(go.crypto.ed25519_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸelliptic() => builtin.initPackage(typeof(go.crypto.elliptic_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸfips140() => builtin.initPackage(typeof(go.crypto.fips140_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸboring() => builtin.initPackage(typeof(go.crypto.@internal.boring_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸcryptotest() => builtin.initPackage(typeof(go.crypto.@internal.cryptotest_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140() => builtin.initPackage(typeof(go.crypto.@internal.fips140_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸtls13() => builtin.initPackage(typeof(go.crypto.@internal.fips140.tls13_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(go.crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrsa() => builtin.initPackage(typeof(go.crypto.rsa_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha256() => builtin.initPackage(typeof(go.crypto.sha256_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸtlsꓸinternalꓸfips140tls() => builtin.initPackage(typeof(go.crypto.tls.@internal.fips140tls_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸx509() => builtin.initPackage(typeof(go.crypto.x509_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸx509ꓸpkix() => builtin.initPackage(typeof(go.crypto.x509.pkix_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸasn1() => builtin.initPackage(typeof(encoding.asn1_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbase64() => builtin.initPackage(typeof(encoding.base64_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸpem() => builtin.initPackage(typeof(encoding.pem_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸhtmlꓸtemplate() => builtin.initPackage(typeof(html.template_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸobscuretestdata() => builtin.initPackage(typeof(go.@internal.obscuretestdata_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(go.@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(go.math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(go.math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
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
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸquick() => builtin.initPackage(typeof(go.testing.quick_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸcryptoꓸcryptobyte() => builtin.initPackage(typeof(vendor.golang.org.x.crypto.cryptobyte_package));
    // </ImportInitializers>
}
