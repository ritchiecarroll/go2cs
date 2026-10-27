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
[assembly: go.GoPositionMap("net/netip/export_test.go", "export_test.cs", "AA8UoqKegqaCpoKmgKSkgKKA")]
[assembly: go.GoPositionMap("net/netip/fuzz_test.go", "fuzz_test.cs", "ABIkAGzaAaKCpoKCgqaCgoKmgoKClIKClIKUgpSClIKUgpSClIKUgpSClILcgpSCpoKCgpSCgpSCgoKUgoK6koKClIKCgoKUgoKCgpSCgoKClIKCgoK6koKClIKCgoKUgoKCgpSCgoKClIKCgoK4goKClIKCAAkUkoKCpoKCggAQIrKUpoKCgpSClIKCuIKCgoKmgII=", "132-211:1")]
[assembly: go.GoPositionMap("net/netip/inlining_test.go", "inlining_test.cs", "ABggooKCyoKUgpKClAAsWqbegoKClJSCgpQ=", "28-31:1", "", "30=GoToolPath/1/1/1,CombinedOutput/1/1/4")]
[assembly: go.GoPositionMap("net/netip/netip.go", "netip.cs", "AEOKAYKaoKigppCmkKaQprIABRLSAAgYsoKUpMimqqKCgpQACBKCgoKUpoKSgoKCgoKUgoKCpLiCpoKUgoKClKaClIKosoKCgpSoktyCgoKClLiCpoKClILKgqaCgoKCgpKCkoKUlJSUlKaUuIKUlJSmgoKUgoKUgoK4goKmgoK4gpKClKaCgpSCgoLcgriCgpSCgpSSlJSsspSklKqiqqKqogACEPassK7SlKSUqJKClKyykoKUgpSCgpSClIKClIKUgpKClIKmrLCqsqyyqqKuwoKUrLKClIKClIKqooKUgqiSqJKCyoK4gpSokoLKgriClKiSgsqCuIKUqsaClKiSgsqCuIKUAAIYAAkClKaCyoKmAAIUwoK4pqqmpq7CAAUQ0oKUgpSkgpSkgqaCAAIQ8oKCrNKCgpSClKiSlKSCgqSCgoK8ooKClKaUpqqigoKkgpSCAAUeAAwClKSkgpS+spSkpIKUAAcSyIKUgpSoyIKUgpSClKiSpoKCgoKmgoKCgoKCgoKmgoKCgqaCgoKCgpQAAhAABRDygoKmgoKCgoKUgILIgoKCgoKkgqamgoKUrNKUpoKCgoKmpqaClKqirLKCtoK0goKClIKkAAIQ0oKClIKCqJK2pIKClKaClKSkAAMQwtqigpSCpIKkgqSClAAIFqCmkKaQAAgOAAkCgoKmgoKUgpSCgpSCpq7CgoKClIKClIKCgpSCkoKUqqKCgpSqoKqygIKk1qKClKSCgqSCgoKClIKCgpSUgoKsspSkpIKClJSUgoKqoqyygraCpIKUrLKCgpSCgqyygoKUrLKqooKUgoKClIIADS4ACAKCgpTekKqwqrCkgKaQrgAICICCtoCCtgAHEIIAAhTygoKUgoKmgqamgqaCgpSCgpSClKqigoKUrsKCAAIU8oKUgJKkAAgSygADEtKClIKUgpSCgJKUpIKcwoCCpICCpKyygpSCuIKUgoKUuIKCqqKssoK2gqSClKyygoKUgoKssoKClKzEqqKClIKCgpSC2JKClA==", "", "", "114=BEUint64/1/2/2,BEUint64/2/2/3;660=IsLoopback/1/1/1,IsMulticast/1/1/2,IsLinkLocalUnicast/1/1/3;676=v4/2/5/1,v4/3/5/1,v4/4/5/2,v4/5/5/2;1402=withoutZone/1/1/1")]
[assembly: go.GoPositionMap("net/netip/netip_pkg_test.go", "netip_pkg_test.cs", "AA8gkuaCgoIAEy6CgoK4goIACgoADCKClLiCgriCgqa4goLKooK4goKCgpSClIKUggANCoIABxaCgoIACgqCAB1EgoKCAA0KggAUMLKSgoKUgIKkgpSAggAMDIIACBqykoKCgpSUgpSCuJKCgu6SgoKCgoKUlIKUgoKUggAKDIK+spKUgoCCtoKCpoKCpgAKGKKCgoKUgoKCuIKCgpSCgoCC", "219-233:1;252-266:1;268-273:2;278-298:3;311-330:1", "", "28=PrefixFrom/1/16/4,PrefixFrom/2/16/5,PrefixFrom/3/16/6,PrefixFrom/4/16/7,PrefixFrom/5/16/8,PrefixFrom/6/16/10,PrefixFrom/7/16/11,PrefixFrom/8/16/12,PrefixFrom/9/16/13,PrefixFrom/10/16/14,PrefixFrom/11/16/15,PrefixFrom/12/16/17,PrefixFrom/13/16/18,PrefixFrom/14/16/19,PrefixFrom/15/16/20,PrefixFrom/16/16/21;68=Invoke/1/25/5,Invoke/2/25/5,Invoke/3/25/5,Invoke/4/25/6,Invoke/5/25/6,Invoke/6/25/6,Invoke/7/25/7,Invoke/8/25/7,Invoke/9/25/7,Invoke/10/25/8,Invoke/11/25/8,Invoke/12/25/8,Invoke/13/25/9,Invoke/14/25/9,Invoke/15/25/10,Invoke/16/25/10,Invoke/17/25/11,Invoke/18/25/11,Invoke/19/25/12,Invoke/20/25/12,Invoke/21/25/13,Invoke/22/25/13,Invoke/23/25/13,Invoke/24/25/14,Invoke/25/25/14;146=Invoke/1/5/5,Invoke/2/5/6,Invoke/3/5/7,Invoke/4/5/8,Invoke/5/5/9;169=Invoke/1/43/5,Invoke/2/43/5,Invoke/3/43/6,Invoke/4/43/6,Invoke/5/43/7,Invoke/6/43/7,Invoke/7/43/8,Invoke/8/43/8,Invoke/9/43/9,Invoke/10/43/9,Invoke/11/43/10,Invoke/12/43/10,Invoke/13/43/11,Invoke/14/43/11,Invoke/15/43/12,Invoke/16/43/12,Invoke/17/43/13,Invoke/18/43/13,Invoke/19/43/14,Invoke/20/43/14,Invoke/21/43/15,Invoke/22/43/15,Invoke/23/43/16,Invoke/24/43/16,Invoke/25/43/17,Invoke/26/43/17,Invoke/27/43/19,WithZone/1/2/19,Invoke/28/43/19,Invoke/29/43/20,WithZone/2/2/20,Invoke/30/43/20,Invoke/31/43/22,Invoke/32/43/23,Invoke/33/43/25,PrefixFrom/1/5/25,Invoke/34/43/25,Invoke/35/43/26,PrefixFrom/2/5/26,Invoke/36/43/26,PrefixFrom/3/5/27,Invoke/37/43/27,PrefixFrom/4/5/28,Invoke/38/43/28,PrefixFrom/5/5/29,Invoke/39/43/29,Invoke/40/43/31,Invoke/41/43/31,Invoke/42/43/32,Invoke/43/43/32;272=Invoke/1/2/5,Invoke/2/2/8")]
[assembly: go.GoPositionMap("net/netip/netip_test.go", "netip_test.cs", "AB4umoKCAAwGggBi0AGykoKCgpSUgpSCuIKClIK4goKClIK4goKUgriCgoKUgrjcgoKAgqSClIKClIKCgsoAP4ABgoKCgqaCgqaCgqamlIKCgIIADwyCABU0goKCyqKCAA0IggAGFIKUgoKCgpSCAAkKggAGFIKCgpSCgpSClIKAgqSCpoKCgoKUgpSCgIKkgsqCgoCCABAKggAHFoKAgqSCgoKUgqaCgoKCgpSCAAkKggAFEoKCgpSCgpSClIKAgqSCpoKCgoKUgpSCgIKkgsqCgoCCABAKggAGFIKAgqSCgoKUgqaCgoKCgpSCAAkKggAFFKaCgoKClIKUgoCCpIKmgoKCgpSClIKAgqSCyoKCgILauIKCgIKkgqaCgpSCggAJCIIACyCykoKCACEMggAcPgCfAdYCspKCgqaCgqaCgqaCgqaCgqaCgqaCgqaCggALDIIAG0CykoKUggAPDKIAHUKCgoKUgoKUgpSClIK4goKCyoKCygAHEIKCgoIADAiCAA4kgoKCuIKCgILsAAcQgoKCggAaCIIAESqCgoK4goKAguwAChaCgoKCAAcQAC1cgoKCggALCIIAFjSykoKUggAXDIIAABCCgqYAH0AANHCykrKklIKClIKUgoKmgqaAggAIEIIACBKCppSCgIK2goKmgoIACAyCgoCCAA8IggAIGoKCgqaCggARCoIADCKCgoKUgoIACQqCABEqspKCggAUDIIAMnSykoKClIKUgpSCgqaCgqaCgpSAgrYACQqCgoIADiSCgoCCAAoKgr6ygpKSlIKUgIIADgyCADJsspKCgpSAggALDIIACBiCgoLKooKClPaiggAEELKSgoKCgpSCgIL+soKCgoKCgsqygoKCgoKCABAigqjCgoKCgoKCyrKCgoKCgoLKsoKCgoKCgsqigoKCgriigoKCgvgABxaCgrKSgoLcgrKSgoLcgoKSkoKC3IKCkpKCgtyCgpKSgoLcgoKCkpKCgtyCgoKSkoKCAAsMggAvaLKSlILcooKCgriCgoKClJSClIIACwyCABU0woKCgqaC1KKCgoKUggAJCoKCACFKgoCCtoCCABYuADcGgoLKgoKUlIKClJSCgpSUgoKCgtyAkoCSgJKQopCigJKAkoCSgJKAtoCSgJKAkoCSgJKCgoKUgoKClICSgJKAkoCSgJKAkoCSgJKAkoCSgJKAkoCSkKKQooCSgJKAkoC2gJKQooC2gJKQopCigLaAkoKClICSgJKAAA0GggAHGIKykpSUkpSCAAoMggAFEoKAggANCoIABxaCgIIADQqCAAUSgoKC7qKCgg==", "139-216:1;284-310:2;670-675:1;883-923:1;961-968:1;1215-1215:1;1251-1258:1;1271-1306:1;1365-1392:2;1367-1390:2.1;1407-1426:1;1517-1522:1;1586-1616:1;1659-1669:1;1660-1662:1.1;1663-1665:1.2;1729-1737:1;1781-1793:1;1906-1911:1;1917-1922:1;1929-1934:1;1941-1946:1;1953-1958:1;1966-1971:1;1979-1984:1;2042-2048:1;2068-2074:1;2105-2114:1;2106-2111:1.1;2205-2210:1;2211-2216:2;2217-2222:3;2223-2230:4;2224-2229:4.1;2233-2233:5;2234-2234:6;2235-2235:7;2236-2236:8;2237-2237:9;2238-2238:10;2239-2239:11;2240-2240:12;2241-2241:13;2242-2242:14;2245-2245:15;2246-2246:16;2247-2247:17;2248-2248:18;2249-2249:19;2250-2254:20;2255-2259:21;2260-2260:22;2261-2261:23;2262-2262:24;2263-2263:25;2264-2264:26;2265-2265:27;2266-2266:28;2267-2267:29;2268-2268:30;2269-2269:31;2270-2270:32;2271-2271:33;2272-2272:34;2273-2273:35;2274-2274:36;2275-2275:37;2276-2276:38;2277-2277:39;2278-2278:40;2281-2281:41;2282-2282:42;2283-2283:43;2286-2286:44;2287-2287:45;2288-2288:46;2289-2289:47;2292-2292:48;2293-2296:49;2297-2297:50;2298-2298:51;2299-2299:52;2317-2328:1;2322-2324:1.1", "", "49=Mk128/1/14/9,MkAddr/1/14/9,Mk128/2/14/14,MkAddr/2/14/14,Mk128/3/14/39,MkAddr/3/14/39,Mk128/4/14/44,MkAddr/4/14/44,Mk128/5/14/49,MkAddr/5/14/49,Mk128/6/14/54,MkAddr/6/14/54,Mk128/7/14/59,MkAddr/7/14/59,Mk128/8/14/64,MkAddr/8/14/64,Mk128/9/14/70,MkAddr/9/14/70,Mk128/10/14/76,MkAddr/10/14/76,Mk128/11/14/82,MakeAddrDetail/1/3/82,Make/1/3/82,MkAddr/11/14/82,Mk128/12/14/87,MakeAddrDetail/2/3/87,Make/2/3/87,MkAddr/12/14/87,Mk128/13/14/93,MakeAddrDetail/3/3/93,Make/3/3/93,MkAddr/13/14/93,Mk128/14/14/99,MkAddr/14/14/99;342=Invoke/1/2/7,Invoke/2/2/12;391=Invoke/1/4/5,Invoke/2/4/6,Invoke/3/4/7,Invoke/4/4/8;491=Invoke/1/6/4,Invoke/2/6/5,Invoke/3/6/6,Invoke/4/6/7,Invoke/5/6/8,Invoke/6/6/9;604=Invoke/1/5/4,Invoke/2/5/5,Invoke/3/5/6,Invoke/4/5/7,Invoke/5/5/8,WithZone/1/1/8,PrefixFrom/1/1/8;651=Addr/1/2/1,PrefixFrom/1/2/1,Addr/2/2/2,PrefixFrom/2/2/2;736=Mk128/1/2/8,MkAddr/1/2/8,Mk128/2/2/13,MkAddr/2/2/13;820=As16/1/4/23,AddrFrom16/1/4/23,As16/2/4/49,AddrFrom16/2/4/49,As16/3/4/72,AddrFrom16/3/4/72,IPv6Loopback/1/2/97,IPv6Loopback/2/2/102,As16/4/4/102,AddrFrom16/4/4/102,IPv4Unspecified/1/1/161,IPv6Unspecified/1/1/166;1036=IPv4Unspecified/1/1/7,IPv6LinkLocalAllNodes/1/1/12,IPv6LinkLocalAllRouters/1/1/17,IPv6Loopback/1/1/22,IPv6Unspecified/1/1/27;1093=Invoke/1/28/5,Invoke/2/28/6,Invoke/3/28/8,Invoke/4/28/8,Invoke/5/28/9,Invoke/6/28/9,Invoke/7/28/10,Invoke/8/28/10,Invoke/9/28/12,Invoke/10/28/12,Invoke/11/28/13,Invoke/12/28/13,Invoke/13/28/14,Invoke/14/28/14,Invoke/15/28/15,Invoke/16/28/15,Invoke/17/28/17,Invoke/18/28/17,Invoke/19/28/18,Invoke/20/28/18,Invoke/21/28/20,Invoke/22/28/20,Invoke/23/28/21,Invoke/24/28/21,Invoke/25/28/22,Invoke/26/28/22,Invoke/27/28/31,Invoke/28/28/31;1158=Invoke/1/5/1,Invoke/2/5/2,Invoke/3/5/4,Invoke/4/5/5,Invoke/5/5/6;1184=Invoke/1/15/5,Invoke/2/15/7,Invoke/3/15/7,Invoke/4/15/8,Invoke/5/15/8,Invoke/6/15/10,Invoke/7/15/10,Invoke/8/15/11,Invoke/9/15/11,Invoke/10/15/13,Invoke/11/15/13,Invoke/12/15/14,Invoke/13/15/14,Invoke/14/15/16,Invoke/15/15/16;1217=Invoke/1/5/1,Invoke/2/5/2,Invoke/3/5/4,Invoke/4/5/5,Invoke/5/5/6;1257=Invoke/1/19/5,Invoke/2/19/7,Invoke/3/19/7,Invoke/4/19/8,Invoke/5/19/8,Invoke/6/19/10,Invoke/7/19/10,Invoke/8/19/11,Invoke/9/19/11,Invoke/10/19/13,Invoke/11/19/13,Invoke/12/19/14,Invoke/13/19/14,Invoke/14/19/16,Invoke/15/19/16,Invoke/16/19/18,Invoke/17/19/18,Invoke/18/19/19,Invoke/19/19/19;1293=Invoke/1/8/1,Invoke/2/8/2,Invoke/3/8/3,Invoke/4/8/4,Invoke/5/8/6,Invoke/6/8/7,Invoke/7/8/8,Invoke/8/8/9;1316=Invoke/1/44/1,Invoke/2/44/2,Invoke/3/44/3,Invoke/4/44/4,Invoke/5/44/5,Invoke/6/44/6,Invoke/7/44/7,Invoke/8/44/8,Invoke/9/44/9,Invoke/10/44/10,Invoke/11/44/11,Invoke/12/44/12,Invoke/13/44/13,Invoke/14/44/14,Invoke/15/44/15,Invoke/16/44/16,Invoke/17/44/17,Invoke/18/44/18,Invoke/19/44/19,Invoke/20/44/20,Invoke/21/44/21,Invoke/22/44/22,Invoke/23/44/23,Invoke/24/44/24,Invoke/25/44/25,Invoke/26/44/26,Invoke/27/44/27,Invoke/28/44/28,Invoke/29/44/29,Invoke/30/44/30,Invoke/31/44/31,Invoke/32/44/32,Invoke/33/44/33,Invoke/34/44/34,Invoke/35/44/35,Invoke/36/44/36,Invoke/37/44/37,Invoke/38/44/38,Invoke/39/44/39,Invoke/40/44/40,Invoke/41/44/41,Invoke/42/44/42,Invoke/43/44/43,Invoke/44/44/44;1379=Invoke/1/4/9,Invoke/2/4/13,Invoke/3/4/17,Invoke/4/4/21;1443=Sprintf/1/5/2,Invoke/1/9/2,Sprintf/2/5/6,Invoke/2/9/6,Invoke/3/9/8,Sprintf/3/5/12,Invoke/4/9/12,Invoke/5/9/14,Sprintf/4/5/18,Invoke/6/9/18,Invoke/7/9/20,Sprintf/5/5/24,Invoke/8/9/24,Invoke/9/9/26;1475=Invoke/1/9/21,Invoke/2/9/25,Invoke/3/9/27,Invoke/4/9/31,Invoke/5/9/33,Invoke/6/9/39,Invoke/7/9/41,Invoke/8/9/48,Invoke/9/9/52;1630=Invoke/1/6/6,Invoke/2/6/7,Invoke/3/6/8,Invoke/4/6/9,Invoke/5/6/10,Invoke/6/6/11;1666=Invoke/1/20/6,Invoke/2/20/6,Invoke/3/20/7,Invoke/4/20/7,Invoke/5/20/8,Invoke/6/20/8,Invoke/7/20/9,Invoke/8/20/9,Invoke/9/20/10,Invoke/10/20/10,Invoke/11/20/11,Invoke/12/20/11,Invoke/13/20/12,Invoke/14/20/12,Invoke/15/20/13,Invoke/16/20/13,Invoke/17/20/14,Invoke/18/20/14,Invoke/19/20/15,Invoke/20/20/15;1697=Invoke/1/6/5,Invoke/2/6/6,Invoke/3/6/9,Invoke/4/6/10,Invoke/5/6/13,PrefixFrom/1/2/13,Invoke/6/6/17,PrefixFrom/2/2/17;1745=Invoke/1/7/10,mustIPs/1/14/12,mustIPs/2/14/13,Invoke/2/7/17,mustIPs/3/14/19,mustIPs/4/14/20,Invoke/3/7/24,mustIPs/5/14/26,mustIPs/6/14/27,Invoke/4/7/31,mustIPs/7/14/33,mustIPs/8/14/34,Invoke/5/7/38,mustIPs/9/14/40,mustIPs/10/14/41,Invoke/6/7/45,mustIPs/11/14/47,mustIPs/12/14/48,Invoke/7/7/52,mustIPs/13/14/54,mustIPs/14/14/55;1988=Invoke/1/6/4,Invoke/2/6/5,Invoke/3/6/6,Invoke/4/6/7,Invoke/5/6/8,Invoke/6/6/9;2288=IPv4/1/3/7,IPv4/2/3/12,IPv4/3/3/17,Invoke/1/6/22,Invoke/2/6/27,Invoke/3/6/32,Invoke/4/6/37,Invoke/5/6/42,Invoke/6/6/47;2386=Invoke/1/4/6,Invoke/2/4/10,As16/1/1/10,AddrFrom16/1/1/10,Invoke/3/4/14,Invoke/4/4/22;2447=Invoke/1/34/4,Invoke/2/34/5,Invoke/3/34/6,Invoke/4/34/6,Invoke/5/34/8,Invoke/6/34/8,Invoke/7/34/10,Invoke/8/34/10,Invoke/9/34/11,Invoke/10/34/11,Invoke/11/34/13,Invoke/12/34/13,Invoke/13/34/14,Invoke/14/34/14,Invoke/15/34/17,Invoke/16/34/17,Invoke/17/34/18,Invoke/18/34/18,Invoke/19/34/20,Invoke/20/34/20,Invoke/21/34/23,Invoke/22/34/23,Invoke/23/34/24,Invoke/24/34/24,Invoke/25/34/27,Invoke/26/34/27,Invoke/27/34/28,Invoke/28/34/28,Invoke/29/34/31,As16/1/1/31,AddrFrom16/1/1/31,PrefixFrom/1/3/31,Invoke/30/34/31,Invoke/31/34/34,PrefixFrom/2/3/34,Invoke/32/34/34,Invoke/33/34/35,PrefixFrom/3/3/35,Invoke/34/34/35;2788=MustParseAddr/1/5/6,MustParseAddr/2/5/7,MustParseAddr/3/5/8,MustParseAddr/4/5/9,MustParseAddr/5/5/10;2824=PrefixFrom/1/2/5,MustParseAddr/1/1/6,PrefixFrom/2/2/6;2848=MustParseAddrPort/1/3/4,MustParseAddrPort/2/3/5,MustParseAddrPort/3/3/6,AddrPortFrom/1/1/8;2874=Invoke/1/2/5,Invoke/2/2/6")]
[assembly: go.GoPositionMap("net/netip/slow_test.go", "slow_test.cs", "ABEiAAEuABUGooLYgoLGuIKClKaCgsqCgpSCgoKClIKmAAIqABQCpoKClIKCgpSCuILYtoKClIKUgpSmgpSCgpTGpq7CgoKUgoKCgpSUqqKClIKClA==")]
[assembly: go.GoPositionMap("net/netip/uint128.go", "uint128.cs", "ABMoogACENCmkqiSqJKokqiSgqiSgq7CqqKqog==")]
[assembly: go.GoPositionMap("net/netip/uint128_test.go", "uint128_test.cs", "AA8WgoKCAAwigoKUpKSUggAJCoIACBiCgoKCAAkKggAIGIKCgoI=")]
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
