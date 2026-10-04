// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
using testing = go.testing_package;
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
[assembly: GoImplement<brokenConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<brokenSigner, crypto_package.Signer>(Promoted = true)]
[assembly: GoImplement<changeImplConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<discardConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<go.crypto.tls_internal_test_package.zeroSource, io_package.Reader>]
[assembly: GoImplement<hairpinConn, net_package.Conn>(Pointer = true)]
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
[assembly: GoImplicitConv<clientTest, ж<clientTest>>(Indirect = true)]
[assembly: GoImplicitConv<fipsCertificate, ж<fipsCertificate>>(Indirect = true)]
[assembly: GoImplicitConv<serverTest, ж<serverTest>>(Indirect = true)]
[assembly: GoImplicitConv<testQUICConn, ж<testQUICConn>>]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("crypto/tls/auth_test.go", "auth_test.cs", "AB0agrjKuLoAEDiCgoKWgoKUgpSCgpSClIKozAAYPIKCgsqCgoKUgIKkgIKmgoKUgIKkgIK4goK8ooKCgpSClII=")]
[assembly: go.GoPositionMap("crypto/tls/bogo_shim_test.go", "bogo_shim_test.cs", "AChCgoSEgoSEgoKEgoKChISEhIKEgoSCgoKCgoKCgoKCgoKCgoKCgoSEgoKChIKCgoKChISCuoKCgqaCpoKCAA4GooKClgAFFISEhIKCgpSWgqaogoKCgoKCgtqCgoKCgoLMgoKCgoKogpaClIKWgpaCgoKUlIKCgoKUgoKClIKWgpSCloKCgpSCloKCgoKUqIKCloKCgpSCgpYABhCCgoKClJaCgpSmgoKAgqaCgpSWgoCCuIKCgoKClIKClICCtoKCgpSClIKCgpSCppaCgoKWgpaClIKUgqSkpJaCloKWgpaCqIKCgpSC2AANBKKClIKUzICCpoKClIKWgoKWhAAKFIKWgoKCgt6CgoKUloKAgty6xKKClICCpIKCuJSCgg==", "134-155:1;482-493:1;498-500:2")]
[assembly: go.GoPositionMap("crypto/tls/cache_test.go", "cache_test.cs", "ABcegpKCgpaCgpSCgpSCloCClICCyIKCgoKktIKCpJaCAAgSgoKEuoKChKaigoKWkoKmkpKCgoKCgoKUgoKCpoKU", "42-62:1;95-115:1")]
[assembly: go.GoPositionMap("crypto/tls/conn_test.go", "conn_test.cs", "ABMcgorKpvQADiaCgoKUgoKCgpSClIIAEBaCAA0chIKCgqaWgqaAgoKUuICCpICCpICCpICCyrKEgoKEgoKQkgAICIKUgoCCgqSEgoKEgoKClIKCloKCloKCgpaWopSAgqSWzICCpICCpoKCuqiCgoKClIK2qIK0pIKEgoKCpoKEgoKCpoKCgoKmgoIAJRSCgqbGgpKUgpSmlqLUooKSlIKEooKAgoKkgpaCgIKCpoSCgrQ=", "92-99:1;101-111:2;137-137:1;138-181:2;283-285:1;301-309:1")]
[assembly: go.GoPositionMap("crypto/tls/ech_test.go", "ech_test.cs", "ABEYgr6CgpSCgpSC/IKCgpSCgpSCgg==")]
[assembly: go.GoPositionMap("crypto/tls/fips_test.go", "fips_test.cs", "ACM0goKClKaCgoKmpoKCgpQACQaCgoKCgoKCgoKCgqaClILMkoKCgpaSgoKCuIKmgp6kpoKUpKaCgoKmpoK0AAMY1oKChIKCgpSClIKSAAoWgpailqKCgpTcgoKChIKSgoSigILKooKCpO6ygoKCgrKClIKCgqbCgpaCgoKEhIKUgoK0goK0goK0poSSgoK6koKCgqaC/KSCpsiCkpSUgoKChKKCgoKUgoKWgpSCgqaCgqaCgqaCgsYAEgTGgrqCgoSChIKCgoSChIKWgoKCgoSCgoSEgoKUpoKUzIKCgoSCgoSEgoKUpoKU3oKE0oKW0oKWgpaCgpaCkoKUgoKCkoKigoKCgpSCuIKCgoKChIKCgoKCsoKCgoKUgriCgoSygpayggAKGoKCgpSmgoKClAAMGqKSkoCCgqQAChiClIKWgoKCgpSCloKClIK0gsSmgoKUgoKWkqKCqAAIFIIAFSiEAB04goKC", "52-73:1;53-72:1.1;75-80:2;82-87:3;155-182:1;171-173:1.1;175-181:1.2;192-211:1;196-200:1.1;203-210:1.2;220-223:1;231-233:1;236-282:2;262-267:2.1;270-281:2.2;369-394:1;397-421:2;428-431:3;433-436:4;457-468:5;481-492:6;497-500:7;502-505:8;602-606:1")]
[assembly: go.GoPositionMap("crypto/tls/handshake_client_test.go", "handshake_client_test.cs", "AD58goKUpKSkuAAMGoIAChaCgoSCgoKWgpSClJamggApVJwADQKCgpSChIKClIKCloKEgoSCgoKM0oSCgoLKgoKWgoKCgoKogqiCgoKCgoKAgq7CgsiClJSCgoKClqgACQaCpuKCgpSS5tKCgoKChIKCgoKUgpKCuIKClJaCgpSClICCgqa2ggAJFoKWhMKEgoSCgoKUloKCloKCloCCxJSCgpSWgoKCloTChIKEgoKWgoK0lLiClISAgoK4goCC3IKAgriCgoKCgpSSgoKCgoKUgrTYooKAgqSAgsamtJKClIKE5oLmguaC5oKmgriCgqaCuKaCuKaCuIKCpoLcgoKmgtymgtymgrimgrimgtymgoKEzIKmgoKEzILWgoKEyoKUqKaCgoTMpoKChAAGEKaCuKSCuKSCuKaCyqaCyoKEgoKEzIKmgoKChMyChAAGEIKChAAGEKaCgoKEzIKChAAGEIKuwoKClIKElLyEAAcQgqaCgoKE3qaCyqaCgJKAAB8GooKogu6CgpaChAAHEpKCgoKUgpSClICSuJaSgpSSlIKCgIKkloKCgoKogoKCgqiCgoKCloKChIKEgoKCgoKUloKClIKCgqiCgoKCgoLcgoKUlIK63ISogoKCloKogoLugpYABxKCgoKCgoKogoKCgpSEgoSCggAHEoKClIKCgpSCgoKUpoKCgpSCgoKUgoKClKaUgoKWgpSCgILKgpSCgILKgoKAgriCgIK4goCCuIKAgqSCgIIACQqClIKChIKChIKE0oSAgoKkopSAgqaChIKClAAKDIKUgqiCpoKUgoSChIKE0oSAgoKkopSAgqaChIKCgoKogqaCgoQABxCClKaC1siChKLKloKAgqSEgoCCptyE7oKEgIL+goSCgrrayoCCpIKAgram7KKC7oKUgpSmpoKChN6mgoKE3qaCgoTugpaClIKUqNaCuICCpKSmgoIADAYADCyCsoSSloKAgqSEgoCCpoKEgoKClIKUgvrIgoSiuJaCgIKkhIKAgsrKhO6ChICCyIKAkoAADQaigoKUgpSClIKUgpSCkpSClJYAEhCCkoKAgqSClLiSgoCCpIKUgriClIKU7oKCkoKAgqSClLiCkoKAgqSClIK4gpSClO6CkoK4koLugpKCuIKSgoCCpIKUgriClIKUypSEgoKUgoSU7oKChAAIEoSigoKUgpSCgpSClIKmgriCgJKAAAwGlJKCloKEhIKAgqSClIKUgoCCpIKUgpSClgAuDoK6grqClIKUgpSC3IK6poLcprqC3KaCgoCC2oCCpIK4gpSClILcgrqCuoKUgpSClILcgrqCpoLcgqaCuoLcgpa6goKmgpSC3IKCpoKWuoKUgsyygoSUwoKCgoKCgoKCgoKChIKCloKCgoKCgoKCgoQAKySUgoKWgqaUsoKEooKCloKCgpSEACYYgoKmgoCSgLaCgoSChKKCgoKCloKClIKEkoKClIKWgIKmgIIACgiCgoSChJaSgsqygoKWgoKWgoCCpIKEgILIgoKUuIKEkoKAgqaCgIKmgpaCgoSCooKClsKCgoCCpIKWgoKUgpQAEQgAAhSCgoKUgpTKgIIABhCCgoKUyoCCAAYQAAcUgoKUuMqC3IKAkoAACAa0goKWgoKCgoKCgoKEgoKEloKCloyE0pKChIKClKKUgoSUgoKigpTGooKUAAgK6gAXLIKUgoKUgILIgoKCooKClIKCgoCCtriigJKEgoKCgoIACAaigIKkgpSAgqSAgqSAgqSAgqSAgqSAgqSCgIKkgIK2uIKAkoC2tIKClIKC7oKCgoKEgoK4gqaCuoKCgoKUgriCpoLMgqiCgoKUgpSCAAkQ0oKCgoKygoKUpoKClIKUgpSCgrT4ogAVMoKSkoLKgqaCgpSClIKAgsim2oCCACUSggA7bIKEhIKCgpaCgoIADgiCgoKU7oKClIKCloLcgoKCgoKEAB5WsoKEsoKCloKCgoSChIKkAAgMgoSChISCgoI=", "300-304:1;346-373:2;390-405:3;628-633:1;799-804:1;844-844:1;845-845:2;879-894:1;896-898:2;899-902:3;903-905:4;906-912:5;923-923:6;931-931:7;954-954:8;958-958:9;966-966:10;1172-1180:1;1189-1205:2;1223-1231:1;1240-1246:2;1262-1268:1;1281-1288:1;1341-1356:1;1372-1380:1;1423-1435:1;1445-1452:1;1485-1487:1;1524-1530:1;1568-1568:1;1569-1569:2;1573-1597:1;1606-1618:2;1608-1617:2.1;1619-1641:3;1620-1640:3.1;1645-1658:4;1648-1657:4.1;1659-1682:5;1661-1681:5.1;1686-1692:6;1688-1691:6.1;1693-1698:7;1694-1697:7.1;1702-1708:8;1704-1707:8.1;1709-1732:9;1711-1731:9.1;1770-1788:10;1795-1795:1;1796-1796:2;1811-1820:1;1821-1833:2;1841-1846:3;1843-1845:3.1;1847-1852:4;1849-1851:4.1;1853-1866:5;1869-1874:6;1871-1873:6.1;1875-1877:7;1878-1882:8;1885-1887:9;1888-1892:10;1889-1891:10.1;1893-1897:11;1900-1902:12;1903-1918:13;1905-1917:13.1;1919-1929:14;1932-1937:15;1934-1936:15.1;1938-1943:16;1940-1942:16.1;1944-1957:17;1960-1965:18;1962-1964:18.1;1966-1969:19;1970-1974:20;1977-1980:21;1981-1986:22;1983-1985:22.1;1987-1991:23;1994-2002:24;1996-1998:24.1;1999-2001:24.2;2003-2007:25;2008-2015:26;2018-2022:27;2023-2031:28;2025-2027:28.1;2028-2030:28.2;2032-2039:29;2049-2066:30;2114-2118:1;2145-2145:1;2146-2146:2;2156-2162:1;2206-2210:1;2240-2253:1;2260-2264:2;2266-2274:3;2369-2369:1;2370-2370:2;2410-2420:1;2487-2491:1;2504-2504:1;2553-2553:1;2554-2554:2;2643-2647:1;2697-2700:1;2707-2722:1;2861-2863:1;2868-2870:2;2877-2879:3;2884-2886:4;2890-2913:5;2894-2898:5.1")]
[assembly: go.GoPositionMap("crypto/tls/handshake_messages_test.go", "handshake_messages_test.cs", "ABcoAAQsgoKCgpSmooSCgrKCgpSCgoKCloKCgoKWgIKmgNyClAAHEJS0poKCltyCgoIACBKCgoKCgpTKgoKAgqSmgoKmooKCgoKCgoKClJSCgoKCpoKCgoKUgoKClKaClIKUgpSClIKClIKUgpSClIKCgoKUlLS0goKCgoKUgpSClIKWpqKCgoKCgoKEgpSClIKWgpaCgpSClIKUgpSCgoK2lIKClIKUgpamooSClIKWpqKCgoKClKaigoKClKaigoKCgqaigoKmooKCpoKCgqaigoLKgoKClIKCgpSmooKCgpSUgoKCgoKUgpSClIKClKaClIKCpoKCgpS4gpSCgoKmpoCigoKClIKmgoKmooKCpqKCgoKCgoKUpqKCgpSClIKUgpSCgoKmpqKCgqaCgpSCgoK49paCgsqEgoKogoKWgpSCloKCloKEguiogsqEgoL4goKClIKCloKClIKC", "56-122:1")]
[assembly: go.GoPositionMap("crypto/tls/handshake_server_test.go", "handshake_server_test.cs", "AC1CgoKqooKmgoKSgoCCpICCpJSCgoKC7IKUgpSCptqClIKmgoKCgrbogqbEgoKCssrugtzWgtymgtymgtaC7sYABxCCgoKCgpSogqbGAAcQgpSogoKCgqaihAAGEIKEsoKCgIKmgoKCgpSCloKEgsqEpoKWgriiqAAOIIKCsoKCgIKkgoKClKaCgoKCgoCCpIKClICCABAIxgAGGsSCgpSSAAcSgoKygoKCgoCCpIKCgpSmgoKCgoKCgIKkgoKUgoKmgu6CgpKCloKCgoK4goKUgoKCuIK4uIKClIKWgoKCggAICIKE3IKUgpSmuIKClIK4goCSgLaCggAIEKaCgpSCgpSCgsqCgJKAtoLKAAgUgoKCqIKClIKogoKClIKogoKUgqiCgoKUggAgQsoACgK4gpSUhIKCgpSCgoKCgoKCgIKmgqKCgoKUloK0gIKktJao5oKm4oKClJLmwoKChIKCgoKUgpKCuIKClJSCgpSEgoKCooK2gqaEgoKCgoKCuJaCgIK4goKCgoKUkoKCgqaCgrSktJKClIKClIKEpoKmgqaCpoKmgriCgqaCuIKmgriCpoK4poK4poK4pIK4pIK4poKCgoKChMqCgqaCgoTKgqaCgoTKgqaCgoTKgpSm2qKChMqClKamgoKEAAcQgpSmgqaCgoQABxCCpoKChO6ClKaCpoKChO6ClKaCrLK4qqKWgoKSgpTKrsKEhsoACAyyhqSCspKCAAsa7oKCqISCgoKUhJSoguyihIKI7gAICqKEgoaE3KiE3NaigoTKuIKUqIKEgoSChNyClKii1KKChITcyoKUqIKCgoSCgoKipILI7qaCyoCCpKSmgoKmgrim2LiChMqmgoKCgoKEyoKmooKCgoKCgoSCgpKCgoKClIKAgqSChIKCgoKAggAMCoKCpoKCpoK4goKmgriCgqaCuIKClIKmgtyi1IKCgoKCgoKCgoKCgpSWgoTKgoTugoTugoTugoTuogAJBKKEAAYQlISCooKCgIKklIKCgtqClIKUlIKWgoKWgrQADAQADzKGgu6ClKaCgpSCuIKUyoKClqaCgriClMqCgoKEsoKWgpKCgpSChNKSopSChISCgpSCgILIgqTcgoKCooKUgoKCgIK26IKCgoKCgoK4goKkABIIggAMJIKWgoKCsoKCgoKCgpSCgqaCgoKEggAHEIKClICCAAwIwoSCgJQAZtgBspKCAA4ggoKWgtgACQTChIKAlAA7gAGykoKCAAwcgoKWgtjq0oKCgoKygoKUpoKClIKUgpSCgrSu8oKCgoKCkpKC8oKSgoKSgIKkgsqCgoKClNbEkoKCgpKAgqSCyoKCgpTWpICCxOSCgpSCAAgIgpSS7oKClIKClu6CgpSCgoKWlKLChIKCyIKCgpaChIKCyIKCgoKCgpiCgoKClIKUgqiChJSCyJaChKiCpoKClIK4gpLugoKUgoKUgoKUgoKW7oKClIKCloKCyJaCgoKCooKUgoKClIKEgoLIgoKCloKEgoKCgpSClIKogoSohIKmgoKUgriCku6CgpSCgpSCgpSCgpbugoKUgoKWgoLIloKCgoKEgoLIgoKCloKEgoKCgpSClIKogoSCgoSEgg==", "47-56:1;229-244:1;292-305:1;345-395:1;357-372:1.1;401-404:1;458-466:1;482-482:1;483-483:2;515-515:1;516-516:2;643-650:1;695-699:1;915-920:1;935-940:1;955-961:1;993-998:1;1014-1019:1;1044-1047:1;1063-1065:1;1085-1125:1;1116-1123:1.1;1139-1141:1;1159-1161:1;1197-1202:1;1219-1224:2;1246-1251:1;1285-1292:1;1353-1359:1;1378-1381:1;1382-1391:2;1383-1386:2.1;1387-1390:2.2;1392-1401:3;1393-1396:3.1;1397-1400:3.2;1402-1411:4;1403-1406:4.1;1407-1410:4.2;1412-1424:5;1416-1419:5.1;1420-1423:5.2;1514-1521:1;1644-1648:1;1652-1655:2;1684-1687:1;1744-1752:1;1785-1785:1;1896-1922:2;1930-1930:1;1997-2022:2;2034-2038:1;2072-2098:1;2077-2086:1.1;2102-2111:2;2128-2130:1;2131-2133:2;2139-2174:1;2175-2242:2;2176-2241:2.1;2188-2190:2.1.1;2206-2208:2.1.2;2210-2222:2.1.3;2234-2236:2.1.4;2249-2251:1;2252-2254:2;2305-2307:1;2312-2320:2;2333-2335:3;2339-2351:4;2367-2369:1;2370-2372:2;2423-2425:1;2441-2443:2;2447-2459:3")]
[assembly: go.GoPositionMap("crypto/tls/handshake_test.go", "handshake_test.cs", "ADRcgoKCgpa0hIKClJaCggAKDqKCloKCgpaCgpaCgoKCgoKCgoQAJBrigIKkhIKEgIKCgpSkgtbigIKkhIKEgIKCgpSkgtikgoKCkoKUgoKClIKCgoKUgoKUlNaihIKCpoKCgpTMgoKUhIKClISCgoKClKiClgAKFpTigoSCgpaCgoKCgpSm5uKChIKCloKClIKCgpTm0oKEgoKU1oCigKKAooCigNaSgoKUgoKCAA8agoKCgpSCgpS4lKKChISChJKCkoKCgqaClJSCgpSCtIKCtoKAkpCSlILqggAJDIKCpoKCgpbKgoKCgoKohIKCltbGloKClIKClIKCkoSAgoKmAAkUgoKCgoKCgoKUgqYACAbCgoKCwoKCgoKClJCSgoKClICC2qKSgoKCgoCCpICCtoKUgqaCgtyUlJSUlOqUlJSUlKS0pAASIgAjQAAaLgAJCt6CkoKCgoKW2oCCpICCpoKCloCCpJaCgoCCpMiWgpKEgoKWgoKWgIKkloKCgIKkyLqEkoSCgpSEgoKWgIKkloKCgIKkzMKCgoKClJSCgoKCgoKCgoI=", "57-62:1;65-67:2;374-374:1;375-375:2;408-414:1;451-451:1;481-502:1;489-489:1.1;645-674:1;689-706:1;724-742:1")]
[assembly: go.GoPositionMap("crypto/tls/key_schedule_test.go", "key_schedule_test.cs", "ABUilIKCgoKCloKCgoKCgoKohISEgIKkgIKmhISAgqSAgqaEhICCpICCpICCpoSAgsySgoKUlIKCgoKUAA0GgpaWloKCgpSC", "86-91:1")]
[assembly: go.GoPositionMap("crypto/tls/link_test.go", "link_test.cs", "ACAikoKUgoKEADBigoKCspKAgqSCgoKAgqaCgoKClIKCpoKC", "78-105:1")]
[assembly: go.GoPositionMap("crypto/tls/prf_test.go", "prf_test.cs", "ABAgAAcQgoKCgoKCggATIoKCgoKEgoCCgqaCgoKCgoiWgoKCloKCloTs")]
[assembly: go.GoPositionMap("crypto/tls/quic_test.go", "quic_test.cs", "ABo2griSlKaCuJKUAAcQgoCCpIKUgIKkgpSYpMiCgIKkgpSYpMjUooKCgoCCyIKCgoKUlIKCgpSUpKSkgILGgoK2pIKCgILYgpSkgrakgsqCgoSChIKEgIKmgIKkgIKkgIKkgIKkgoCCpICCpIKUgvqCgoKChIKEgoKCgoCCpIKWgoKCgoCCpIK4goKCgoSChIKCgoKClIKAgoK2lJSAgsiUgoKCgoKCgIKmgoKCgoKClIC4yJSCgoKCgoKAgqaCgoKUgLjIgoKCgoKCgoCCpoKA3PiCgoKChIKEgoKCgoKClIKCvrKCgoKCgoKCooKAgoCSxoKAgoCSxpSAggAKCIKCgoKCgoKCgoKClJSCgoKCgIKkgviCgoKChIKEgoSCgoCCpIKAgqSCgIKmgJKkgJLIgoKEgoKCgoCCpoKUgpSClIK4goKCgoKClIKCuIKCgoKClIKCuIKCgoKChIKChIKCgoKCgIKkgpaCgoKCgpSklICCpIKUgoKUgoKUgriCgpSCuIKCgoKCgoSCgoKEgoKCgoKAgqSCloKCgoKClIKUlICCpIKUgoKClIKmgII=", "32-34:1;43-45:1;248-260:1;375-389:1;402-409:1;536-542:1;563-565:1;566-568:2;600-602:1")]
[assembly: go.GoPositionMap("crypto/tls/ticket_test.go", "ticket_test.cs", "AAkOkg==")]
[assembly: go.GoPositionMap("crypto/tls/tls_test.go", "tls_test.cs", "AD9qAA4aAB00ABQYAAUUgoKCgoKAgqSCgIIAEwqigoKUgIKmgoKUgIKmjIKClICCAAgIgoCCpICCyIKCgoKUgpTWooKCgpSogqLUooKCgpSogqLUgoK4goKWkoKCgqKCgoKClKiCpoCCgqSm3oKmgriCgpSCloIACQiigpaClISigoKClIKAgoKkloKCgoKUlIKCqIKAgqSAgriAgqSAgriAgqSAgriAgqSCtMiAqtKClIKCwoKClJKilIL+gsiCgrSkgoCCpKwACQ6ClIKCgIK2psKClIKCooKCgoKUgoKAgoKCpJamgoKClJSCgpaEgoKCloKCgoKClIKUAAwGooKUgoKCguKCgoKCgpSCgoKAgoKktPiUgoKCgpaCtMiClIKUhIKClJKClrTIgpSCtPSChIKClICCpICCpoKClICCAAoIgoS6goKCgqaCgoSCgoKUgoSCqNbCgpSCgoKigoKCgoKUgoKAgoKCpJaCgpSUqIKCgIKmgoKUhIKSgpaCgrKCgpaCsoKCloKCloKAgsTUooKUhMKCgpSUgoKAgqSUgoKUgpaAgu6CxsKEgoKClICCpJSAgqaAgqaCgpSClMaEoJKglIK0gsbcgoKUgoKUkoSAgtYACASigpSygoKUlIKCgIKklIKClIKAgqbGgqCUgoKCgpSSgIKmgpaAgsSkgoKEpIKmgqaCpoKmgqaCpoKmgqaCqISCgoKCgoKCgoSCuKKChIKCpoCkzgAICKSkpKSkpKSkpKSkpKSkyKTYgoSCgriCgoCCACQYgoKUpoKClKbCgpSIpJKCgoKmlIKCgoKAgqSAgsqCgoKChIKCgoKClIKCgpSCgqa0pIKCkoKCgpSCACcYgoKUgoKCgoKClIKCgoK4psKClISSgoKmlIKCgoCCpKiCgoSChIKCgqaAgqSAgqSAgqSAgqS0pIKCgoKCgpSC7oKCgoIACQiChIKClIKEgoKChLKClKSkwgAKFoKCgoSCgpaCloKWgpaCloKWgqiClIKWgpaCpJaClIKmgoKUgqiCgqaCAAYSori4goSCgoKCuIAACASihLjKyroAiwGaAoKClLS0AA8KgoKCgpSWgqaCgoKUloKogoKCpoKCppaCgoKCloCCpKaCgqaCqICCtoKCgoKWgpSCloCCuICCpoKUgqiCgoKCgpSCuoaygoKCgpaCpJaCpLqCgqSCpKaCpKaCpKaCpKaCpKaUgpTIgpS4gqSmgqSUgpSCyoKAkqSAksyiAAEipAAIDJTawoLIgoKWgIKmqoCCyIKWgJKAABMGggAkWIKClIKEgpKihJKUgoKCgoKCgoKEgoKUgoKUgoKUgoKWgoKCloCCpoKCpoKUgpSCloKCgoKUgpaClIKUgpSCABkMgoIACSTuAAgSAAgSAAcQAAcQ7gAHEMyCgrKigpSUgoKUkoKklJSCgpSCgpSCgpSCpoKUgqaCgpSCpoKUggANDoKCgpSCgpSCuIKClISigoKClIKmooKCgpSCpqKCgpSCAAgKgoSSgIK4goKCgpSCuIKCgpQACxiCgpaCgtyAggANCIKCgpTcgoKUgoKUgoKClIKCloKCgpKCgpKUgoKCpoKClJaWgoKWhIKSlISCgoKCgoKCgoKCgoKCgoK4qIKClIKUgpSClIKUgpSC", "184-190:1;198-204:1;225-234:1;286-298:1;360-367:1;371-379:2;423-438:1;484-505:1;617-633:1;658-661:2;665-669:3;672-676:4;695-727:1;729-758:2;762-762:3;763-763:4;799-823:1;826-826:2;853-856:1;857-860:2;861-864:3;865-868:4;869-872:5;873-876:6;877-880:7;881-884:8;885-888:9;1016-1036:1;1069-1076:1;1070-1072:1.1;1073-1075:1.2;1115-1131:1;1166-1173:1;1167-1169:1.1;1170-1172:1.2;1209-1298:1;1528-1540:1;1638-1701:2;1781-1781:1;1782-1782:2;1839-1919:1;1855-1858:1.1;1859-1862:1.2;1863-1866:1.3;1867-1870:1.4;1942-1944:1;1949-1951:2;1958-1960:3;1967-1969:4;1975-1977:5;1983-1985:6;1990-1992:7;1998-2000:8;2008-2064:9;2018-2025:9.1;2088-2097:1;2098-2107:2;2108-2116:3;2122-2126:1;2203-2226:1;2206-2223:1.1;2209-2211:1.1.1;2212-2217:1.1.2;2219-2221:1.1.3;2236-2238:2")]
// </GoSourcePositionMaps>

namespace go.crypto;

[GoPackage("tls")]
public static partial class tls_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸcrypto() => builtin.initPackage(typeof(crypto_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸecdh() => builtin.initPackage(typeof(go.crypto.ecdh_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸecdsa() => builtin.initPackage(typeof(go.crypto.ecdsa_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸed25519() => builtin.initPackage(typeof(go.crypto.ed25519_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸelliptic() => builtin.initPackage(typeof(go.crypto.elliptic_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸcryptotest() => builtin.initPackage(typeof(go.crypto.@internal.cryptotest_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸtls13() => builtin.initPackage(typeof(go.crypto.@internal.fips140.tls13_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸhpke() => builtin.initPackage(typeof(go.crypto.@internal.hpke_package));
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
