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
global using execꓸError = go.os.exec_package.ΔError;
global using flagꓸErrorHandling = go.flag_package.ΔErrorHandling;
global using jsonꓸToken = object;
global using jsonꓸΔToken = object;
global using netipꓸAddr = go.net.netip_package.ΔAddr;
global using netipꓸPrefix = go.net.netip_package.ΔPrefix;
global using netꓸAddr = go.net_package.ΔAddr;
global using netꓸError = go.net_package.ΔError;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using runtimeꓸError = go.runtime_package.ΔError;
// </ImportedTypeAliases>

using go;
using static go.net.netip_package;
using static go.net.netip_test_package;

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
[assembly: GoDynamicTypeLift("7374727563747b6970206e65742f6e657469702e416464723b206e657874206e65742f6e657469702e416464723b2070726576206e65742f6e657469702e416464727d", "nextPrevTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20697020737472696e677d", "parseBenchInputsᴛ1")]
[assembly: GoTypeAlias("Addr", "ΔAddr")]
[assembly: GoTypeAlias("AddrDetail", "go.net.netip_package.addrDetail")]
[assembly: GoTypeAlias("Prefix", "ΔPrefix")]
[assembly: GoTypeAlias("Uint128", "go.net.netip_package.uint128")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<AddrPort, go.net.netip_package.appendMarshaler>]
[assembly: GoImplement<AddrPort, netipTypeCmp>]
[assembly: GoImplement<parseAddrError, error>]
[assembly: GoImplement<parsePrefixError, error>]
[assembly: GoImplement<testing_package.B, testing_package.TB>(Pointer = true)]
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
[assembly: GoImplement<ΔAddr, go.net.netip_package.appendMarshaler>]
[assembly: GoImplement<ΔAddr, netipTypeCmp>]
[assembly: GoImplement<ΔPrefix, go.net.netip_package.appendMarshaler>]
[assembly: GoImplement<ΔPrefix, netipTypeCmp>]
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
[assembly: go.GoPositionMap("net/netip/export_test.go", "export_test.cs", "AA4UkpKegqaCpoKmgKSUgKKApIA=")]
[assembly: go.GoPositionMap("net/netip/fuzz_test.go", "fuzz_test.cs", "AEIkADzaAaKCloKCgpaCgoKWgoKClIKClIKUgpSClIKUgpSClIKUgpSClILMgpSCloKCgpSCgoSCgoKUgoK6koKClIKCgoKUgoKCgpSCgoKClIKCgoK6koKClIKCgoKUgoKCgpSCgoKClIKCgoK4goKClIKCAAkUkoKCloKCggAQIrKUloKCgpSClIKCuIKCgoKWgII=", "132-211:1")]
[assembly: go.GoPositionMap("net/netip/inlining_test.go", "inlining_test.cs", "ABggooKCyoKUgpKClAAsWpbugoKClJSCgpQ=", "28-31:1", "", "30=GoToolPath/1/1/1,CombinedOutput/1/1/4")]
[assembly: go.GoPositionMap("net/netip/netip.go", "netip.cs", "AEWMAZKaoKigppCmkKaQprIABRLSAAgYsoKUpKjWqqKCgpQACBKCgoKUpoKSgoKCgoKUgoKCpLiCpoKUgoKClKaClIKosoKCgpSoksyCgoKClKiCloKClIK6gqaCgoKCgpKCkoKUlJSUlKaUqIKUlJSWgoKUgoKUgoKogoKWgoKogpKClJaCkpSCgpLMgqiCgpSCgpSSlJSsspSkpKqiqqKqogACEPassK7SlKSkqJKClKyykoKUgpSCgpSClIKClIKUgpKClIKmrLCqsqyyqqKuwoKUrLKClIKClIKqooKUgqiSqJKCuoK4gpSokoK6griClKiSgrqCuIKUqsaClKiSgrqCuIKUAAIYAAkClJaCuoKWAAIUwoKoppqmlq7CAAUQ0oKUgpSkgpSkgraCAAIQ8oKCrNKCgpSClKiSlKSCgqSCgoLMooKClKaUpqqigoKkgpSCAAUeAAwClKSkgpTOspSkpIKUAAgSuIKUgpSouIKUgpSClKiSpoKCgoKmgoKCgoKCgoKmgoKCgqaCgoKCgpQAAhAABRDygoKmgoKCgoKUgIK4goKCgoKkgpaWgoKUrLKUpoKCgoKWlqaClKqirLKCtoK0goKClIK0AAIQ0oKClIKCqJK2pIKCpKaClKSkAAQQwtqigpSCpIKkgqSCpAAIFqCmkKaQAAgOAAkCgoKWgoKUgpSCgpSClq7CgoKClIKClIKCgpSCkoKUqqKCgpSqoKqygIKk1oKClKSCgqSCgoKClIKCgpSkgoKsspSkpIKClJSkgoKqoqyygraCpIKkrLKCgpSCgqyygoKUrLKqooKUgoKClIIADS4ACAKCgpTekKqwqrCkgKaQAAIUAAgCgIKkgIKkAAcQggACFPKCgpSCgqaClpaCloKClIKClIKUqqKCgpSuwoIAAhTygpSAkqQACBLKAAMS0oKUgpSClIKAkpSkgpzCgIKkgIKkrLKClIKogpSCgpSogoKqoqyygraCpIKkrLKCgpSCgqyygoKUrMSqooKUgoKClILYkoKU", "", "", "117=BEUint64/1/2/2,BEUint64/2/2/3;649=IsLoopback/1/1/1,IsMulticast/1/1/2,IsLinkLocalUnicast/1/1/3;664=v4/2/5/1,v4/3/5/1,v4/4/5/2,v4/5/5/2;1386=withoutZone/1/1/1")]
[assembly: go.GoPositionMap("net/netip/netip_pkg_test.go", "netip_pkg_test.cs", "AA8gkuaCgoIAES6CgoKogoIACwoADCKChLiCgqiCgpa4goLKooK4goKCgpSClIKUggANCoIABxaCgoIACgqCAB1EgoKCAA0KggAUMLKSgoKUgIKkgpSAggAMDIIACRqykoKCgpSUgpSCqJKCgt6SgoKCgoKUlIKUgoKUggAKDIKuspKEgoCCpoKCloKClgAKGKKCgoKUgoKCuIKCgpSCgoCC", "219-233:1;252-266:1;268-273:2;278-298:3;311-330:1", "", "28=PrefixFrom/1/16/4,PrefixFrom/2/16/5,PrefixFrom/3/16/6,PrefixFrom/4/16/7,PrefixFrom/5/16/8,PrefixFrom/6/16/10,PrefixFrom/7/16/11,PrefixFrom/8/16/12,PrefixFrom/9/16/13,PrefixFrom/10/16/14,PrefixFrom/11/16/15,PrefixFrom/12/16/17,PrefixFrom/13/16/18,PrefixFrom/14/16/19,PrefixFrom/15/16/20,PrefixFrom/16/16/21;66=Invoke/1/25/5,Invoke/2/25/5,Invoke/3/25/5,Invoke/4/25/6,Invoke/5/25/6,Invoke/6/25/6,Invoke/7/25/7,Invoke/8/25/7,Invoke/9/25/7,Invoke/10/25/8,Invoke/11/25/8,Invoke/12/25/8,Invoke/13/25/9,Invoke/14/25/9,Invoke/15/25/10,Invoke/16/25/10,Invoke/17/25/11,Invoke/18/25/11,Invoke/19/25/12,Invoke/20/25/12,Invoke/21/25/13,Invoke/22/25/13,Invoke/23/25/13,Invoke/24/25/14,Invoke/25/25/14;141=Invoke/1/5/5,Invoke/2/5/6,Invoke/3/5/7,Invoke/4/5/8,Invoke/5/5/9;164=Invoke/1/43/5,Invoke/2/43/5,Invoke/3/43/6,Invoke/4/43/6,Invoke/5/43/7,Invoke/6/43/7,Invoke/7/43/8,Invoke/8/43/8,Invoke/9/43/9,Invoke/10/43/9,Invoke/11/43/10,Invoke/12/43/10,Invoke/13/43/11,Invoke/14/43/11,Invoke/15/43/12,Invoke/16/43/12,Invoke/17/43/13,Invoke/18/43/13,Invoke/19/43/14,Invoke/20/43/14,Invoke/21/43/15,Invoke/22/43/15,Invoke/23/43/16,Invoke/24/43/16,Invoke/25/43/17,Invoke/26/43/17,Invoke/27/43/19,WithZone/1/2/19,Invoke/28/43/19,Invoke/29/43/20,WithZone/2/2/20,Invoke/30/43/20,Invoke/31/43/22,Invoke/32/43/23,Invoke/33/43/25,PrefixFrom/1/5/25,Invoke/34/43/25,Invoke/35/43/26,PrefixFrom/2/5/26,Invoke/36/43/26,PrefixFrom/3/5/27,Invoke/37/43/27,PrefixFrom/4/5/28,Invoke/38/43/28,PrefixFrom/5/5/29,Invoke/39/43/29,Invoke/40/43/31,Invoke/41/43/31,Invoke/42/43/32,Invoke/43/43/32;267=Invoke/1/2/5,Invoke/2/2/8")]
[assembly: go.GoPositionMap("net/netip/netip_test.go", "netip_test.cs", "ABwumoKCAAwGogBh0AGykoKCgpSUgpSCqIKClIKogoKClIKogoKUgqiCgoKUgqjMgoKAgqSClIKClIKCgroAPoABgoKCgpaCgpaCgpamlIKCgIIADwyCABU0goKCyqKCAA0IggAHFIKEgoKCgpSCAAkKggAHFIKCgpSCgpSClIKAgqSCloKCgoKUgpSCgIKkgrqCgoCCABAKggAHFoKAgqSCgoKUgpaCgoKCgpSCAAkKggAFEoKCgpSCgpSClIKAgqSCloKCgoKUgpSCgIKkgrqCgoCCABAKggAHFIKAgqSCgoKUgpaCgoKCgpSCAAkKggAGFKaCgoKClIKUgoCCpIKWgoKCgpSClIKAgqSCuoKCgILauIKCgIKkgpaCgpSCggAJCIIACyCykoKCACEMggAVPgCeAdYCspKCgpaCgpaCgpaCgpaCgpaCgpaCgpaCggALDIIAGkCykoKEggAPDKIAGEKCgoKUgoKUgpSClIKogoKCuoKCugAHEIKCgoIADAiCAAokgoKCqIKCgILcAAcQgoKCggAPCIIACiSCgoKogoKAgtwACBKCgoKCAAsIggAVNLKSgoSCABcMggAAEIKClgAeQAAzcLKSsqSEgoKUgpSCgpaCloCCAAgQggAHEoKmhIKAgqaCgpaCggAIDIKCgIIADwiCAAgagoKCloKCABEKggAMIoKCgpSCggAJCoIAESqykoKCABQMggAzdLKSgoKUgpSClIKCpoKCpoKClICCpgAJCoKCggAOJIKCgIIACgqCvrKCkpKUgpSAggAODIIAMmyykoKClICCAAsMggAIGIKCgsqigoKU9qKCAAQQspKCgoKClIKAgv6igoKCgoKCyqKCgoKCgoIAECKCqLKCgoKCgoLKooKCgoKCgsqigoKCgoKCyqKCgoKCuKKCgoKCAAgIAAcWgoKykoKC3IKykoKC3IKCkpKCgtyCgpKSgoLcgoKSkoKC3IKCgpKSgoLcgoKCkpKCggALDIIALmiykoSC3KKCgoK4goKCgpSUgoSCAAsMggAWNMKCgoKmgtSigoKClIIACQqCggAdSoKAgraAggAhLgA3BoKCuoKClJSCgpSUgoKUlIKCgoLMgJKAkoCSkKKQooCSgJKAkoCSgKaAkoCSgJKAkoCSgoKClIKCgpSAkoCSgJKAkoCSgJKAkoCSgJKAkoCSgJKAkpCikKKAkoCSgJKApoCSkKKApoCSkKKQooCmgJKCgpSAkoCSgAANBoIABxiCspKUlJKUggAKDIIABBKCgIIADQqCAAYWgoCCAA0KggAEEoKCgu6igoI=", "139-216:1;284-310:2;670-675:1;883-923:1;961-968:1;1188-1195:1;1208-1243:1;1302-1329:2;1304-1327:2.1;1344-1363:1;1454-1459:1;1523-1553:1;1596-1606:1;1597-1599:1.1;1600-1602:1.2;1666-1674:1;1718-1730:1;1843-1848:1;1854-1859:1;1866-1871:1;1878-1883:1;1890-1895:1;1903-1908:1;1916-1921:1;1979-1985:1;2005-2011:1;2042-2051:1;2043-2048:1.1;2142-2147:1;2148-2153:2;2154-2159:3;2160-2167:4;2161-2166:4.1;2170-2170:5;2171-2171:6;2172-2172:7;2173-2173:8;2174-2174:9;2175-2175:10;2176-2176:11;2177-2177:12;2178-2178:13;2179-2179:14;2182-2182:15;2183-2183:16;2184-2184:17;2185-2185:18;2186-2186:19;2187-2191:20;2192-2196:21;2197-2197:22;2198-2198:23;2199-2199:24;2200-2200:25;2201-2201:26;2202-2202:27;2203-2203:28;2204-2204:29;2205-2205:30;2206-2206:31;2207-2207:32;2208-2208:33;2209-2209:34;2210-2210:35;2211-2211:36;2212-2212:37;2213-2213:38;2214-2214:39;2215-2215:40;2218-2218:41;2219-2219:42;2220-2220:43;2223-2223:44;2224-2224:45;2225-2225:46;2226-2226:47;2229-2229:48;2230-2233:49;2234-2234:50;2235-2235:51;2236-2236:52;2254-2265:1;2259-2261:1.1", "", "49=Mk128/1/14/9,MkAddr/1/14/9,Mk128/2/14/14,MkAddr/2/14/14,Mk128/3/14/39,MkAddr/3/14/39,Mk128/4/14/44,MkAddr/4/14/44,Mk128/5/14/49,MkAddr/5/14/49,Mk128/6/14/54,MkAddr/6/14/54,Mk128/7/14/59,MkAddr/7/14/59,Mk128/8/14/64,MkAddr/8/14/64,Mk128/9/14/70,MkAddr/9/14/70,Mk128/10/14/76,MkAddr/10/14/76,Mk128/11/14/82,MakeAddrDetail/1/3/82,Make/1/3/82,MkAddr/11/14/82,Mk128/12/14/87,MakeAddrDetail/2/3/87,Make/2/3/87,MkAddr/12/14/87,Mk128/13/14/93,MakeAddrDetail/3/3/93,Make/3/3/93,MkAddr/13/14/93,Mk128/14/14/99,MkAddr/14/14/99;330=Invoke/1/2/7,Invoke/2/2/12;379=Invoke/1/4/5,Invoke/2/4/6,Invoke/3/4/7,Invoke/4/4/8;478=Invoke/1/6/4,Invoke/2/6/5,Invoke/3/6/6,Invoke/4/6/7,Invoke/5/6/8,Invoke/6/6/9;588=Invoke/1/5/4,Invoke/2/5/5,Invoke/3/5/6,Invoke/4/5/7,Invoke/5/5/8,WithZone/1/1/8,PrefixFrom/1/1/8;636=Addr/1/2/1,PrefixFrom/1/2/1,Addr/2/2/2,PrefixFrom/2/2/2;718=Mk128/1/2/8,MkAddr/1/2/8,Mk128/2/2/13,MkAddr/2/2/13;795=As16/1/4/23,AddrFrom16/1/4/23,As16/2/4/49,AddrFrom16/2/4/49,As16/3/4/72,AddrFrom16/3/4/72,IPv6Loopback/1/2/97,IPv6Loopback/2/2/102,As16/4/4/102,AddrFrom16/4/4/102,IPv4Unspecified/1/1/161,IPv6Unspecified/1/1/166;1003=IPv4Unspecified/1/1/7,IPv6LinkLocalAllNodes/1/1/12,IPv6LinkLocalAllRouters/1/1/17,IPv6Loopback/1/1/22,IPv6Unspecified/1/1/27;1058=Invoke/1/28/5,Invoke/2/28/6,Invoke/3/28/8,Invoke/4/28/8,Invoke/5/28/9,Invoke/6/28/9,Invoke/7/28/10,Invoke/8/28/10,Invoke/9/28/12,Invoke/10/28/12,Invoke/11/28/13,Invoke/12/28/13,Invoke/13/28/14,Invoke/14/28/14,Invoke/15/28/15,Invoke/16/28/15,Invoke/17/28/17,Invoke/18/28/17,Invoke/19/28/18,Invoke/20/28/18,Invoke/21/28/20,Invoke/22/28/20,Invoke/23/28/21,Invoke/24/28/21,Invoke/25/28/22,Invoke/26/28/22,Invoke/27/28/31,Invoke/28/28/31;1115=Invoke/1/5/1,Invoke/2/5/2,Invoke/3/5/4,Invoke/4/5/5,Invoke/5/5/6;1141=Invoke/1/15/5,Invoke/2/15/7,Invoke/3/15/7,Invoke/4/15/8,Invoke/5/15/8,Invoke/6/15/10,Invoke/7/15/10,Invoke/8/15/11,Invoke/9/15/11,Invoke/10/15/13,Invoke/11/15/13,Invoke/12/15/14,Invoke/13/15/14,Invoke/14/15/16,Invoke/15/15/16;1168=Invoke/1/5/1,Invoke/2/5/2,Invoke/3/5/4,Invoke/4/5/5,Invoke/5/5/6;1197=Invoke/1/15/5,Invoke/2/15/7,Invoke/3/15/7,Invoke/4/15/8,Invoke/5/15/8,Invoke/6/15/10,Invoke/7/15/10,Invoke/8/15/11,Invoke/9/15/11,Invoke/10/15/13,Invoke/11/15/13,Invoke/12/15/14,Invoke/13/15/14,Invoke/14/15/16,Invoke/15/15/16;1224=Invoke/1/6/1,Invoke/2/6/2,Invoke/3/6/3,Invoke/4/6/4,Invoke/5/6/6,Invoke/6/6/7;1250=Invoke/1/4/9,Invoke/2/4/13,Invoke/3/4/17,Invoke/4/4/21;1311=Sprintf/1/5/2,Invoke/1/9/2,Sprintf/2/5/6,Invoke/2/9/6,Invoke/3/9/8,Sprintf/3/5/12,Invoke/4/9/12,Invoke/5/9/14,Sprintf/4/5/18,Invoke/6/9/18,Invoke/7/9/20,Sprintf/5/5/24,Invoke/8/9/24,Invoke/9/9/26;1342=Invoke/1/9/21,Invoke/2/9/25,Invoke/3/9/27,Invoke/4/9/31,Invoke/5/9/33,Invoke/6/9/39,Invoke/7/9/41,Invoke/8/9/48,Invoke/9/9/52;1489=Invoke/1/6/6,Invoke/2/6/7,Invoke/3/6/8,Invoke/4/6/9,Invoke/5/6/10,Invoke/6/6/11;1524=Invoke/1/20/6,Invoke/2/20/6,Invoke/3/20/7,Invoke/4/20/7,Invoke/5/20/8,Invoke/6/20/8,Invoke/7/20/9,Invoke/8/20/9,Invoke/9/20/10,Invoke/10/20/10,Invoke/11/20/11,Invoke/12/20/11,Invoke/13/20/12,Invoke/14/20/12,Invoke/15/20/13,Invoke/16/20/13,Invoke/17/20/14,Invoke/18/20/14,Invoke/19/20/15,Invoke/20/20/15;1555=Invoke/1/6/5,Invoke/2/6/6,Invoke/3/6/9,Invoke/4/6/10,Invoke/5/6/13,PrefixFrom/1/2/13,Invoke/6/6/17,PrefixFrom/2/2/17;1603=Invoke/1/7/10,mustIPs/1/14/12,mustIPs/2/14/13,Invoke/2/7/17,mustIPs/3/14/19,mustIPs/4/14/20,Invoke/3/7/24,mustIPs/5/14/26,mustIPs/6/14/27,Invoke/4/7/31,mustIPs/7/14/33,mustIPs/8/14/34,Invoke/5/7/38,mustIPs/9/14/40,mustIPs/10/14/41,Invoke/6/7/45,mustIPs/11/14/47,mustIPs/12/14/48,Invoke/7/7/52,mustIPs/13/14/54,mustIPs/14/14/55;1846=Invoke/1/6/4,Invoke/2/6/5,Invoke/3/6/6,Invoke/4/6/7,Invoke/5/6/8,Invoke/6/6/9;2142=IPv4/1/3/7,IPv4/2/3/12,IPv4/3/3/17,Invoke/1/6/22,Invoke/2/6/27,Invoke/3/6/32,Invoke/4/6/37,Invoke/5/6/42,Invoke/6/6/47;2237=Invoke/1/4/6,Invoke/2/4/10,As16/1/1/10,AddrFrom16/1/1/10,Invoke/3/4/14,Invoke/4/4/22;2299=Invoke/1/34/4,Invoke/2/34/5,Invoke/3/34/6,Invoke/4/34/6,Invoke/5/34/8,Invoke/6/34/8,Invoke/7/34/10,Invoke/8/34/10,Invoke/9/34/11,Invoke/10/34/11,Invoke/11/34/13,Invoke/12/34/13,Invoke/13/34/14,Invoke/14/34/14,Invoke/15/34/17,Invoke/16/34/17,Invoke/17/34/18,Invoke/18/34/18,Invoke/19/34/20,Invoke/20/34/20,Invoke/21/34/23,Invoke/22/34/23,Invoke/23/34/24,Invoke/24/34/24,Invoke/25/34/27,Invoke/26/34/27,Invoke/27/34/28,Invoke/28/34/28,Invoke/29/34/31,As16/1/1/31,AddrFrom16/1/1/31,PrefixFrom/1/3/31,Invoke/30/34/31,Invoke/31/34/34,PrefixFrom/2/3/34,Invoke/32/34/34,Invoke/33/34/35,PrefixFrom/3/3/35,Invoke/34/34/35;2641=MustParseAddr/1/5/6,MustParseAddr/2/5/7,MustParseAddr/3/5/8,MustParseAddr/4/5/9,MustParseAddr/5/5/10;2677=PrefixFrom/1/2/5,MustParseAddr/1/1/6,PrefixFrom/2/2/6;2700=MustParseAddrPort/1/3/4,MustParseAddrPort/2/3/5,MustParseAddrPort/3/3/6,AddrPortFrom/1/1/8;2725=Invoke/1/2/5,Invoke/2/2/6")]
[assembly: go.GoPositionMap("net/netip/slow_test.go", "slow_test.cs", "ABEiAAEuABUGooLIgpLGuIKClJaCgrqCgpSCgoKClIKWAAIqABICloKClIKCgpSCqILIlrKClIKUgpSGopSCgpS2tq7CgoKUgoKCgpSUqqKClIKClA==")]
[assembly: go.GoPositionMap("net/netip/uint128.go", "uint128.cs", "ABMoogACENCmkqiSqJKokqiSgqiSgq7CqqKqog==")]
[assembly: go.GoPositionMap("net/netip/uint128_test.go", "uint128_test.cs", "AA8WgoKCAAsigoKUpKSkggAJCoIACBiCgoKCAAkKggAIGIKCgoI=")]
// </GoSourcePositionMaps>

namespace go.net;

[GoPackage("netip")]
public static partial class netip_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct TestAddrPortMarshalUnmarshal_tests {}
    internal partial struct TestBitsClearedFrom_tests {}
    internal partial struct TestBitsSetFrom_tests {}
    internal partial struct TestIPBitLen_tests {}
    internal partial struct TestParseAddrPort_tests {}
    internal partial struct TestParseIPError_tests {}
    internal partial struct TestPrefixContains_tests {}
    internal partial struct TestPrefixValid_tests {}
    internal partial struct TestUint128AddSub_tests {}
    internal partial struct addrDetail {}
    internal partial struct nextPrevTestsᴛ1 {}
    internal partial struct parseAddrError {}
    internal partial struct parsePrefixError {}
    internal partial struct uint128 {}
    public partial interface appendMarshaler {}
    public partial struct AddrPort {}
    public partial struct ΔAddr {}
    public partial struct ΔPrefix {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(go.encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸbytealg() => builtin.initPackage(typeof(@internal.bytealg_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(go.math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸunique() => builtin.initPackage(typeof(unique_package));
    // </ImportInitializers>
}

[GoPackage("netip_test")]
public static partial class netip_test_package
{
}
