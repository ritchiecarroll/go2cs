// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
using ecdsa = go.crypto.ecdsa_package;
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static go.crypto.x509_package;
using static go.crypto.x509_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b56657273696f6e20696e743b204e202a6d6174682f6269672e496e743b204520696e743b2044202a6d6174682f6269672e496e743b2050202a6d6174682f6269672e496e743b2051202a6d6174682f6269672e496e747d", "TestParsePKCS1PrivateKey_val")]
[assembly: GoDynamicTypeLift("7374727563747b636f6e73747261696e7420737472696e673b20646f6d61696e20737472696e673b206578706563744572726f7220626f6f6c3b2073686f756c644d6174636820626f6f6c7d", "nameConstraintTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b64657248657820737472696e673b2073686f756c64526573657269616c697a6520626f6f6c7d", "ecKeyTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6865784b657920737472696e673b206572726f72436f6e7461696e7320737472696e677d", "pkcs8MismatchKeyTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206c6f63616c5061727420737472696e673b20646f6d61696e20737472696e677d", "rfc2821Testsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6b696e642063727970746f2f783530392e50454d4369706865723b2070617373776f7264205b5d627974653b2070656d44617461205b5d627974653b20706c61696e44455220737472696e677d", "testDataᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b206365727420737472696e673b20657870656374656420737472696e677d", "unknownAuthorityErrorTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b726177205b5d627974653b2076616c696420626f6f6c3b2073747220737472696e673b20696e7473205b5d75696e7436347d", "oidTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b736967416c676f2063727970746f2f783530392e5369676e6174757265416c676f726974686d3b2070656d4365727420737472696e677d", "ecdsaTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
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
[assembly: go.GoPositionMap("crypto/x509/cert_pool_test.go", "cert_pool_test.cs", "ABESooKEgoKCgoKCgpSCgpSCgoKUggA9iAGykoKC", "101-106:1")]
[assembly: go.GoPositionMap("crypto/x509/name_constraints_test.go", "name_constraints_test.cs", "AMYBfgCaCqAZooKEAAoYgIKmgpSCgpaCgpYACQaigoQACxqClLaCgpS2goKUtraCgpS8spYADxi4goCCpoKWgoKWpoKSopKSkpKCloKCpKaCkoKE3KLSgpS2goKUtra2uJaClMamloKCqIKCqIKCgpaCgpaAgqamwoKUpKSkpKSkpLjmgqSCgpSospKCgqgAABDihIKCgpaCloSCgoKChIKCgpaCloKWgoSCgrqCgoKCgqiCgIKSgoKC3NyEgoKClKaCkoKUqIKCgoKUgoCCgraWggAIDIKCgpaCgoKWpuKEgoKUlISCgoKUlJaCgpSUhIKCgoSCAAsGABY4goKChIKCloKCloKWggAOCoKCgpaaAAoegoKWgqiCgpSCAAgMuMyCgpaChIKCloCC", "1803-1811:1;1832-1857:1;1859-1869:2;1928-1934:1;1938-2062:2;2045-2049:2.1;2173-2176:1;2178-2180:2", "", "1511=SetBytes/1/1/1,Unix/1/2/5,Unix/2/2/6;1553=SetBytes/1/1/1,Unix/1/2/6,Unix/2/2/7;1882=Unix/1/1/3")]
[assembly: go.GoPositionMap("crypto/x509/oid_test.go", "oid_test.cs", "ABUcABtKgoKCgoKWgpaAgqaCgoKClJaCgIKCpoKWgoKClIIACgyCAAcYgoKCloKCloKCggAKCoIABxqCgILcgoKC9oIAECyCgoKUAAUWgoKCgpaCgoKCloKWgoKWgoKWgoKCloKCgoKCloKCloKAgqaCgpaCgoKCloKAgqaCggAKCoKCABxGgoCC2oKCgoSCgpaCyqKCgoKClIKAgg==", "", "", "133=mustNewOIDFromInts/1/9/5,mustNewOIDFromInts/2/9/5,mustNewOIDFromInts/3/9/6,mustNewOIDFromInts/4/9/6,mustNewOIDFromInts/5/9/7,mustNewOIDFromInts/6/9/7,mustNewOIDFromInts/7/9/8,mustNewOIDFromInts/8/9/8,mustNewOIDFromInts/9/9/10;266=mustNewOIDFromInts/1/20/5,mustNewOIDFromInts/2/20/6,mustNewOIDFromInts/3/20/7,mustNewOIDFromInts/4/20/8,mustNewOIDFromInts/5/20/9,mustNewOIDFromInts/6/20/10,mustNewOIDFromInts/7/20/11,mustNewOIDFromInts/8/20/12,mustNewOIDFromInts/9/20/13,mustNewOIDFromInts/10/20/14,mustNewOIDFromInts/11/20/15,mustNewOIDFromInts/12/20/16,mustNewOIDFromInts/13/20/17,mustNewOIDFromInts/14/20/18,mustNewOIDFromInts/15/20/19,mustNewOIDFromInts/16/20/21,mustNewOIDFromInts/17/20/22,mustNewOIDFromInts/18/20/23,mustNewOIDFromInts/19/20/25,mustNewOIDFromInts/20/20/30")]
[assembly: go.GoPositionMap("crypto/x509/parser_test.go", "parser_test.cs", "AB0oogBDlgGykoKCkoKUggAzWoKCgoKUgpSClIKUgpSC6IK4goKClIKCggAMDKIALWKSgoKmgoKUgoLc2IKClNy4goKUgoK4goKC", "97-107:1;178-188:1;242-256:1;289-292:1", "", "297=Repeat/1/2/3,Repeat/2/2/4")]
[assembly: go.GoPositionMap("crypto/x509/pem_decrypt_test.go", "pem_decrypt_test.cs", "ABoggoKCgoKUgoKClICCpIKClIIACwqCgoKCgpSCgoKClIKUgpSClIKCgpSCAA0KAKgBsAIAEBKmgoKClIKAgsiA", "", "", "103=testingKey/1/6/9,testingKey/2/6/34,testingKey/3/6/59,testingKey/4/6/84,testingKey/5/6/109,testingKey/6/6/136")]
[assembly: go.GoPositionMap("crypto/x509/pkcs8_test.go", "pkcs8_test.cs", "AEJ0ogAoXoKCgoKUgoKClIKClICCgqSCgoKUgoKWgIKCgoKUlIKCgpSCggAPEgAEEIKCgoKC", "", "", "70=TypeOf/1/7/9,TypeOf/2/7/14,P224/1/1/15,TypeOf/3/7/20,P256/1/1/21,TypeOf/4/7/26,P384/1/1/27,TypeOf/5/7/32,P521/1/1/33,TypeOf/6/7/38,TypeOf/7/7/43")]
[assembly: go.GoPositionMap("crypto/x509/pkits_test.go", "pkits_test.cs", "ABEeABcQ3oQAABSCgpSAgqYAWbYBsoKUooKCgoKUgoKUlISCgoKClJbcgoKUlII=", "145-184:1", "", "18=mustNewOIDFromInts/1/4/2,mustNewOIDFromInts/2/4/3,mustNewOIDFromInts/3/4/4,mustNewOIDFromInts/4/4/5")]
[assembly: go.GoPositionMap("crypto/x509/platform_test.go", "platform_test.cs", "ADZMgoKWgoKUgoKCloKClIKCgpaAgqaEAHWEAoKCloKSwoKCgpSCgpSCgpaCgpSClIKWgoKSgpaCgpKCkoI=", "206-247:1", "", "83=NewInt/1/10/15,Add/1/23/17,Add/2/23/18,NewInt/2/10/25,Add/3/23/27,Add/4/23/28,NewInt/3/10/36,Add/5/23/38,Add/6/23/39,Add/7/23/42,NewInt/4/10/47,Add/8/23/49,Add/9/23/50,NewInt/5/10/58,Add/10/23/60,Add/11/23/61,NewInt/6/10/70,Add/12/23/72,Add/13/23/73,Add/14/23/76,NewInt/7/10/82,Add/15/23/84,Add/16/23/85,Add/17/23/88,NewInt/8/10/94,Add/18/23/96,Add/19/23/97,NewInt/9/10/107,Add/20/23/109,Add/21/23/110,NewInt/10/10/119,Add/22/23/121,Add/23/23/122")]
[assembly: go.GoPositionMap("crypto/x509/root_test.go", "root_test.cs", "AA0WooKCpoKiABIExoKCkoCmAClkspKCgoKUgpSClJaChISCkoLY", "12-16:1;27-27:1;81-106:2", "", "58=NewCertPool/1/6/14,NewCertPool/2/6/19,NewCertPool/3/6/24,NewCertPool/4/6/30,NewCertPool/5/6/35,NewCertPool/6/6/42")]
[assembly: go.GoPositionMap("crypto/x509/sec1_test.go", "sec1_test.cs", "ABocAAUeooKCgoKUgoKUgoIACBAABBCCgoKCgg==")]
[assembly: go.GoPositionMap("crypto/x509/verify_test.go", "verify_test.cs", "AF9gAIkCigWCgoCCpILKgoCCyIKAgsiCgoKUgriigpSAgsiCgILIgoCCyIKAgviCgoKUpqLegoKCgoK6goKCqIKCloSCgpSUgoKCpqiCgpaCgqbegoKCgoKogrqCgoKCuIKCgoLugrKSyoKClrKSgpTKgoKCgpSUpqKmgsiAgpSkgoKUgoKUALUG3gsABRSCspKCgpSCgpTKgoIACwwADiaigoSCgpaCgpaCAJMB+gGCABAogoCCpICC2qKCgpaChAAJFoKCloKClIKClgAJBoKCuoSCgpSEgoKClJaCgpaCuISCuIKCloSCgpSEgoKCgpSWgoKWgoC4pNaigpaAlMyAgqaCgpaEgoCCxOSCgoKC+IKCgoKAgqSUgoKClgANBoKCloKAgoCClLSCqqKSgpaSgoKCgqiCgpaipoKCgoIAGDKihIKClNyCgpKClIKWgoKWgoKUgoKUpoKEgoKCgoKClIKClJSCloKCgoKClIKCloKCgpKClJSCgoKUgpS6poKCgoKClJSCAAwGogA4fABLmAGCALEB5AIAH0AABxAABxAAESQAChYABxAAEyiCAAcQAAwagoKmACZMgoKmABMospKCuIKUgpSCggAWDIIAQp4BgoKWsqKCkoKUhIKCspKClIKWkoKWgoKSggAODIKCgpYAIESiAAcQgoKUgoKUgoSCgpKC3oK4goK4hIKAgviCgoKU7oKClIKCloCCACUIAAgQgoKCgoSCgoKUgoKClJaCgoKCgoKCgoKCgoKCgoKCgoKCgoKEAM0BsAOCgoKUlABQogGWspLcggAJDIKCgoKUgoKClJaChISCgoTcgpKC6IKCgpQACBKCgpSCgpaCAAgSgoKUgoKWgoKWgoKEgoKCAAcW8oKCgIKmgoKAgqSEhIKUgoKWAAYQgoKClISCAAgSyoKClA==", "374-381:1;489-500:1;542-544:1;554-559:1;1359-1377:1;1690-1690:1;1765-1772:1;1779-1789:2;1984-1986:1;2060-2063:2;2239-2241:3;2271-2273:4;2279-2281:5;2287-2289:6;2305-2307:7;2316-2318:8;2324-2326:9;2344-2347:10;2353-2355:11;2366-2373:12;2409-2416:13;2436-2452:14;2542-2572:1;2544-2547:1.1;2553-2556:1.2;2561-2564:1.3;2616-2642:1;2706-2717:1;3050-3060:2;3065-3076:1", "", "96=expectHostnameError/1/7/45,generatePEMCertWithRepeatSAN/1/4/49,generatePEMCertWithRepeatSAN/2/4/50,expectHostnameError/2/7/55,generatePEMCertWithRepeatSAN/3/4/59,generatePEMCertWithRepeatSAN/4/4/60,expectHostnameError/3/7/65,expectHostnameError/4/7/75,expectHostnameError/5/7/225,expectHostnameError/6/7/247,expectHostnameError/7/7/293;452=NewCertPool/1/1/1,Unix/1/1/3;588=Unix/1/2/1,Unix/2/2/2;1451=Errorf/1/1/2;1688=Now/1/2/3,Add/1/2/3,Now/2/2/4,Add/2/2/4;1800=NewCertPool/1/1/1,Unix/1/1/3;1915=NewCertPool/1/1/1;1955=Now/1/2/3,Add/1/2/3,Now/2/2/4,Add/2/2/4;2783=NewInt/1/1/1,Now/1/2/3,Add/1/2/3,Now/2/2/4,Add/2/2/4;2840=NewInt/1/1/1,Now/1/2/3,Add/1/2/3,Now/2/2/4,Add/2/2/4;3303=NewInt/1/1/1,Now/1/2/3,Add/1/2/3,Now/2/2/4,Add/2/2/4;3321=NewInt/1/1/1,Now/1/2/3,Add/1/2/3,Now/2/2/4,Add/2/2/4;3401=NewInt/1/1/2,Now/1/2/5,Add/1/2/5,Now/2/2/5,Add/2/2/5")]
[assembly: go.GoPositionMap("crypto/x509/x509_test.go", "x509_test.cs", "ADhSgoKCgoKUirqCgIK4AAYcgoKUgpSCAAkIhIKCgoKCuIKCgpS4griygoKCloKCgpSClPaCgoKCgqaCgoKCpoKCgoIAESAAK0CEgoKCgoLsgoSCgILIgoKCpoKCgqaCgoKmAAsYggALGoSCgoKUjpaCACUIggABOISShIKCloSCgqaCAAgIgriCgoKUgsyCgpKClIKCgpSClgBRggGCgoKCgpKCpoKClIKCAAwWAChSooKCgoL6lNyCgrrcgoKWpoKClIKClIKC6IKCgoKUgoKWgoKWgIKmgoCCAAoIgoKSgpSClIKUggAKCIKCgpaCgpaAgqaCggAFYoKCgpSmgoKClAAPBqKEgoKWgoKWAAkggoKEgoIAOIQBgoKCloKCgpaCloKWgpaCloKWgpaCloKWgpaCloKWgoKCuoKCgoKmgpaCloKWgpaCloKWgpaCloKWgpaCloKWgpaCloKCggBdogEABRKCgoKCgoKUgIKkgIKkgIKkgIIAJUCCAAcQgoKClIKUgoKUiLiCgoKClIK4goKCgqaAggBIiAGCgoKCgpaCgoKWgIKCADJggoKCloKCloKUgoKUgpaAggBMkAEADgiCgoKChIKCgoQAAxKEgoKEAAoWAAkWgoKCgpaCgoKUgtyCgoKClKaigoKCgpSCgoKWgrySgoKClIK4ooKCgoKUgoKCloIACAySgpSEgIIADhCihIKCloKCloKCloKClgAHGoIAChaCgoKWgoKCloKCgpaCkoKSgpKCkoLKgoKCgpaCgpamgoKClgAQJISCloLOAAscgoCCpoaWgoKouoSCuIKCgoKCloKWgpaCloKCgoKmggAQCoakgoKClgADEICCpoKCloLOooKCgoKWgoKCluaCAAkWgIKmgoCCpoKAgqaCgoCCpoKAgsiiAAkYgoCCpIKWgoKAgqSCloKCgoCCpIK4ggAKGoCCpoKAgsiiAAkWgIKmgoCCpoKAggAJCIIACRyCgILagoCCAAkIggAIGIKAggAUJAAWLoKCgoKUgIKkgIKkgILIgoKCgpSAgqSAgqSAggAbKoKCgIKSggAUJIKCgIKSggAtUIKCgoKWgIKmgIIACgiCgpSCgpSCgpSClIAAFiqkggAcMIKCgoKWgoCCAAkIgoKClIKCqNqClpTeAC1kgoCCuIIACAiovgAQKoKAggAcNIKCgoKWgIIAHTCCgoKCloKCAC5auIKAgsiCuIKCgpaCgpaAgqaCgqaAggAJCoLKgoKWgoKWgoKClKgAK1SCgoKClriAggAIDgAEEIKCgoKCAAwKooKClIKCuoKChIKCgoQAwwKWBbKSgoKSgpKClIKWgoKWhKiCgriCuIKCgqaCpoLMgpSCgpS4gqaCgpS4ggAHEoKCgpSC7pKCgoKoppSCqIKmgqaC7qKChJSkpKYAChi6goKCloKClpaCgpSCgvqChMyCggATCIKCgpaCgpSEABk+sqKCgpKCugAQLIKCgpSCgpKCAAkKori+goKU3IKClLqCgoKigoKCAAoWgqaC5oK4goKCkoLogpLKgoK4goKClKaigoKUqtKClIKUhJSCgoKUgoKUgoKUgqjmgoKClIKClNyEgpSClIaUggAICIIAbtwBspKCgoKCgoLugoKCgpSCAB84ooKCgpSCgpSCggAlSKKCgpSCggBYrgG0goKUgoIAJ0yigoKUgoKUguiihAAIEoKClIKCloKCkoCCptyClIKCloCC3IKCgpaCgriCgoKCgpSCgoKCAAkIgoKClIKClABBjgEABxCysoKClIKClIKCkoIACAyCgoKUAAcQgoKWgoIAERqCgoKCuIKCgpQABxCCgoIAFSKigoKUgoIAEx6igoKUgoIAEx6igoKUgoLogoQABxCoqIKCloKCloKWgviCgpYACBKCgoKWgoKWgpaChIKCloKCloK4poKCgriCAA0agoKUgoKCuIIADRqCgpSCgoK4ggANGoKClIKCgg==", "134-140:1;141-147:2;148-154:3;2895-3023:1;3028-3069:1;3141-3148:1;3198-3204:1;3208-3214:2;3221-3228:3;3466-3476:1;3882-3897:1", "", "64=Cmp/2/4/2,Cmp/3/4/3,Cmp/4/4/4;116=P256/1/1/1,NewInt/1/2/2,NewInt/2/2/2;272=bigFromString/1/4/2,bigFromString/2/4/5,bigFromString/3/4/7,bigFromString/4/4/8;285=fromBase10/1/5/2,fromBase10/2/5/5,fromBase10/3/5/7,fromBase10/4/5/8,fromBase10/5/5/9;303=Cmp/2/5/2,Cmp/3/5/4,Cmp/4/5/5,Cmp/5/5/6;373=fromBase10/1/1/1;595=ParseIP/1/2/1,ParseIP/2/2/1;751=NewInt/1/1/1,Unix/1/2/18,Unix/2/2/19,IPv4/1/1/37,To4/1/1/37,ParseIP/1/1/37,parseURI/1/1/38,mustNewOIDFromInts/1/1/40,parseCIDR/1/3/43,parseCIDR/2/3/43,parseCIDR/3/3/44;1074=bigFromHexString/1/4/2,bigFromHexString/2/4/3,bigFromHexString/3/4/4,bigFromHexString/4/4/6;1094=Cmp/2/4/1,Cmp/3/4/2,Cmp/4/4/3;1388=NewInt/1/2/2,NewInt/2/2/6;1399=NewInt/1/2/2,NewInt/2/2/6;1543=IPv4/1/1/8,To4/1/1/8,ParseIP/1/1/8;1718=fromBase64/1/2/4,fromBase64/2/2/5;1759=NewInt/1/1/1,Unix/1/2/5,Unix/2/2/6;1804=NewInt/1/1/1,Unix/1/2/5,Unix/2/2/6;1847=NewInt/1/1/1,Unix/1/2/5,Unix/2/2/6;1874=NewInt/1/1/1,Unix/1/2/5,Unix/2/2/6;2273=ToRDNSequence/1/1/4;2551=NewInt/1/1/1;2680=Add/1/35/50,Add/2/35/66,Add/3/35/67,Add/4/35/82,Add/5/35/83,NewInt/1/20/84,SetBytes/1/2/84,Add/6/35/99,Add/7/35/100,NewInt/2/20/101,SetBytes/2/2/101,NewInt/3/20/119,Add/8/35/120,NewInt/4/20/123,Add/9/35/124,Add/10/35/125,NewInt/5/20/142,Add/11/35/143,NewInt/6/20/146,Add/12/35/147,Add/13/35/148,NewInt/7/20/164,Add/14/35/165,NewInt/8/20/169,Add/15/35/170,Add/16/35/171,NewInt/9/20/187,Add/17/35/188,NewInt/10/20/197,Add/18/35/198,Add/19/35/199,NewInt/11/20/215,Add/20/35/216,NewInt/12/20/219,Add/21/35/220,Add/22/35/221,NewInt/13/20/238,Add/23/35/239,NewInt/14/20/242,Add/24/35/243,Add/25/35/244,NewInt/15/20/260,Add/26/35/261,NewInt/16/20/264,Add/27/35/265,Add/28/35/266,NewInt/17/20/288,Add/29/35/289,NewInt/18/20/298,Add/30/35/299,Add/31/35/300,NewInt/19/20/314,Add/32/35/315,Add/33/35/316,NewInt/20/20/324,Add/34/35/325,Add/35/35/326;3121=String/1/1/1;3125=String/1/2/1,String/2/2/1;3152=Size/1/1/8;3194=NewInt/1/1/1;3231=NewInt/1/3/8,NewInt/2/3/16,NewInt/3/3/24;3313=NewInt/1/1/1;3372=NewInt/1/1/1;3392=NewInt/1/1/1;3469=IPv4/1/1/3;3482=Equal/1/2/1,Equal/2/2/2;3888=NewInt/1/1/1,Now/1/2/2,Add/1/2/2,Now/2/2/3,Add/2/2/3;3914=NewInt/1/1/2,Now/1/2/3,Add/1/2/3,Now/2/2/4,Add/2/2/4;3976=Public/1/6/12,Public/2/6/22,Public/3/6/33,Public/4/6/43,Public/5/6/54,Public/6/6/65;4042=Public/1/1/4;4085=NewInt/1/1/1,Now/1/2/5,Add/1/2/5,Now/2/2/6,Add/2/2/6;4130=NewInt/1/1/1,Now/1/2/5,Add/1/2/5,Now/2/2/6,Add/2/2/6;4239=NewInt/1/1/1,Unix/1/2/3,Unix/2/2/4;4250=mustNewOIDFromInts/1/1/1;4277=NewInt/1/1/1,Unix/1/2/3,Unix/2/2/4,mustNewOIDFromInts/1/1/6;4324=NewInt/1/1/1,Unix/1/2/3,Unix/2/2/4;4350=NewInt/1/1/1,Unix/1/2/3,Unix/2/2/4;4376=NewInt/1/1/1,Unix/1/2/3,Unix/2/2/4")]
// </GoSourcePositionMaps>

namespace go.crypto;

[GoPackage("x509")]
public static partial class x509_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("ekuDescs")] partial struct TestEKUEnforcement_ekuDescs {}
    [GoLocalName("testCase")] partial struct TestPoliciesValid_testCase {}
    [GoMemberRecord("pub", GoMemberFact.Descriptor, typeof(go.crypto_package.PublicKeyᴅ))] partial struct brokenSigner {}
    [GoLocalName("dsaParams")] partial struct dsaSelfSignedCNX_dsaParams {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcrypto() => builtin.initPackage(typeof(crypto_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸdsa() => builtin.initPackage(typeof(go.crypto.dsa_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸecdh() => builtin.initPackage(typeof(go.crypto.ecdh_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸecdsa() => builtin.initPackage(typeof(go.crypto.ecdsa_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸed25519() => builtin.initPackage(typeof(go.crypto.ed25519_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸelliptic() => builtin.initPackage(typeof(go.crypto.elliptic_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(go.crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrsa() => builtin.initPackage(typeof(go.crypto.rsa_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha256() => builtin.initPackage(typeof(go.crypto.sha256_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha512() => builtin.initPackage(typeof(go.crypto.sha512_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸtls() => builtin.initPackage(typeof(go.crypto.tls_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸx509() => builtin.initPackage(typeof(go.crypto.x509_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸx509ꓸpkix() => builtin.initPackage(typeof(go.crypto.x509.pkix_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸasn1() => builtin.initPackage(typeof(go.encoding.asn1_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbase64() => builtin.initPackage(typeof(go.encoding.base64_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸgob() => builtin.initPackage(typeof(go.encoding.gob_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(go.encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(go.encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸpem() => builtin.initPackage(typeof(go.encoding.pem_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(go.@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸurl() => builtin.initPackage(typeof(go.net.url_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}
