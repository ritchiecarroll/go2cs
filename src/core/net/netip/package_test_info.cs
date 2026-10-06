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
[assembly: go.GoPositionMap("net/netip/inlining_test.go", "inlining_test.cs", "ABggooKCyoKUgpKClAAsWpbugoKClJSCgpQ=", "28-31:1")]
[assembly: go.GoPositionMap("net/netip/netip.go", "netip.cs", "AEWMAZKaoKigppCmkKaQprIABRLSAAgYsoKUpKjWqqKCgpQACBKCgoKUpoKSgoKCgoKUgoKCpLiCpoKUgoKClKaClIKosoKCgpSoksyCgoKClKiCloKClIK6gqaCgoKCgpKCkoKUlJSUlKaUqIKUlJSWgoKUgoKUgoKogoKWgoKogpKClJaCkpSCgpLMgqiCgpSCgpSSlJSsspSkpKqiqqKqogACEPassK7SlKSkqJKClKyykoKUgpSCgpSClIKClIKUgpKClIKmrLCqsqyyqqKuwoKUrLKClIKClIKqooKUgqiSqJKCuoK4gpSokoK6griClKiSgrqCuIKUqsaClKiSgrqCuIKUAAIYAAkClJaCuoKWAAIUwoKoppqmlq7CAAUQ0oKUgpSkgpSkgraCAAIQ8oKCrNKCgpSClKiSlKSCgqSCgoLMooKClKaUpqqigoKkgpSCAAUeAAwClKSkgpTOspSkpIKUAAgSuIKUgpSouIKUgpSClKiSpoKCgoKmgoKCgoKCgoKmgoKCgqaCgoKCgpQAAhAABRDygoKmgoKCgoKUgIK4goKCgoKkgpaWgoKUrLKUpoKCgoKWlqaClKqirLKCtoK0goKClIK0AAIQ0oKClIKCqJK2pIKCpKaClKSkAAQQwtqigpSCpIKkgqSCpAAIFqCmkKaQAAgOAAkCgoKWgoKUgpSCgpSClq7CgoKClIKClIKCgpSCkoKUqqKCgpSqoKqygIKk1oKClKSCgqSCgoKClIKCgpSkgoKsspSkpIKClJSkgoKqoqyygraCpIKkrLKCgpSCgqyygoKUrLKqooKUgoKClIIADS4ACAKCgpTekKqwqrCkgKaQAAIUAAgCgIKkgIKkAAcQggACFPKCgpSCgqaClpaCloKClIKClIKUqqKCgpSuwoIAAhTygpSAkqQACBLKAAMS0oKUgpSClIKAkpSkgpzCgIKkgIKkrLKClIKogpSCgpSogoKqoqyygraCpIKkrLKCgpSCgqyygoKUrMSqooKUgoKClILYkoKU")]
[assembly: go.GoPositionMap("net/netip/netip_pkg_test.go", "netip_pkg_test.cs", "AA8gkuaCgoIAES6CgoKogoIACwoADCKChLiCgqiCgpa4goLKooK4goKCgpSClIKUggANCoIABxaCgoIACgqCAB1EgoKCAA0KggAUMLKSgoKUgIKkgpSAggAMDIIACRqykoKCgpSUgpSCqJKCgt6SgoKCgoKUlIKUgoKUggAKDIKuspKEgoCCpoKCloKClgAKGKKCgoKUgoKCuIKCgpSCgoCC", "219-233:1;252-266:1;268-273:2;278-298:3;311-330:1")]
[assembly: go.GoPositionMap("net/netip/netip_test.go", "netip_test.cs", "ABwumoKCAAwGogBh0AGykoKCgpSUgpSCqIKClIKogoKClIKogoKUgqiCgoKUgqjMgoKAgqSClIKClIKCgroAPoABgoKCgpaCgpaCgpamlIKCgIIADwyCABU0goKCyqKCAA0IggAHFIKEgoKCgpSCAAkKggAHFIKCgpSCgpSClIKAgqSCloKCgoKUgpSCgIKkgrqCgoCCABAKggAHFoKAgqSCgoKUgpaCgoKCgpSCAAkKggAFEoKCgpSCgpSClIKAgqSCloKCgoKUgpSCgIKkgrqCgoCCABAKggAHFIKAgqSCgoKUgpaCgoKCgpSCAAkKggAGFKaCgoKClIKUgoCCpIKWgoKCgpSClIKAgqSCuoKCgILauIKCgIKkgpaCgpSCggAJCIIACyCykoKCACEMggAVPgCeAdYCspKCgpaCgpaCgpaCgpaCgpaCgpaCgpaCggALDIIAGkCykoKEggAPDKIAGEKCgoKUgoKUgpSClIKogoKCuoKCugAHEIKCgoIADAiCAAokgoKCqIKCgILcAAcQgoKCggAPCIIACiSCgoKogoKAgtwACBKCgoKCAAsIggAVNLKSgoSCABcMggAAEIKClgAeQAAzcLKSsqSEgoKUgpSCgpaCloCCAAgQggAHEoKmhIKAgqaCgpaCggAIDIKCgIIADwiCAAgagoKCloKCABEKggAMIoKCgpSCggAJCoIAESqykoKCABQMggAzdLKSgoKUgpSClIKCpoKCpoKClICCpgAJCoKCggAOJIKCgIIACgqCvrKCkpKUgpSAggAODIIAMmyykoKClICCAAsMggAIGIKCgsqigoKU9qKCAAQQspKCgoKClIKAgv6igoKCgoKCyqKCgoKCgoIAECKCqLKCgoKCgoLKooKCgoKCgsqigoKCgoKCyqKCgoKCuKKCgoKCAAgIAAcWgoKykoKC3IKykoKC3IKCkpKCgtyCgpKSgoLcgoKSkoKC3IKCgpKSgoLcgoKCkpKCggALDIIALmiykoSC3KKCgoK4goKCgpSUgoSCAAsMggAWNMKCgoKmgtSigoKClIIACQqCggAdSoKAgraAggAhLgA3BoKCuoKClJSCgpSUgoKUlIKCgoLMgJKAkoCSkKKQooCSgJKAkoCSgKaAkoCSgJKAkoCSgoKClIKCgpSAkoCSgJKAkoCSgJKAkoCSgJKAkoCSgJKAkpCikKKAkoCSgJKApoCSkKKApoCSkKKQooCmgJKCgpSAkoCSgAANBoIABxiCspKUlJKUggAKDIIABBKCgIIADQqCAAYWgoCCAA0KggAEEoKCgu6igoI=", "139-216:1;284-310:2;670-675:1;883-923:1;961-968:1;1188-1195:1;1208-1243:1;1302-1329:2;1304-1327:2.1;1344-1363:1;1454-1459:1;1523-1553:1;1596-1606:1;1597-1599:1.1;1600-1602:1.2;1666-1674:1;1718-1730:1;1843-1848:1;1854-1859:1;1866-1871:1;1878-1883:1;1890-1895:1;1903-1908:1;1916-1921:1;1979-1985:1;2005-2011:1;2042-2051:1;2043-2048:1.1;2142-2147:1;2148-2153:2;2154-2159:3;2160-2167:4;2161-2166:4.1;2170-2170:5;2171-2171:6;2172-2172:7;2173-2173:8;2174-2174:9;2175-2175:10;2176-2176:11;2177-2177:12;2178-2178:13;2179-2179:14;2182-2182:15;2183-2183:16;2184-2184:17;2185-2185:18;2186-2186:19;2187-2191:20;2192-2196:21;2197-2197:22;2198-2198:23;2199-2199:24;2200-2200:25;2201-2201:26;2202-2202:27;2203-2203:28;2204-2204:29;2205-2205:30;2206-2206:31;2207-2207:32;2208-2208:33;2209-2209:34;2210-2210:35;2211-2211:36;2212-2212:37;2213-2213:38;2214-2214:39;2215-2215:40;2218-2218:41;2219-2219:42;2220-2220:43;2223-2223:44;2224-2224:45;2225-2225:46;2226-2226:47;2229-2229:48;2230-2233:49;2234-2234:50;2235-2235:51;2236-2236:52;2254-2265:1;2259-2261:1.1")]
[assembly: go.GoPositionMap("net/netip/slow_test.go", "slow_test.cs", "ABEiAAEuABUGooLIgpLGuIKClJaCgrqCgpSCgoKClIKWAAIqABICloKClIKCgpSCqILIlrKClIKUgpSGopSCgpS2tq7CgoKUgoKCgpSUqqKClIKClA==")]
[assembly: go.GoPositionMap("net/netip/uint128.go", "uint128.cs", "ABMoogACENCmkqiSqJKokqiSgqiSgq7CqqKqog==")]
[assembly: go.GoPositionMap("net/netip/uint128_test.go", "uint128_test.cs", "AA8WgoKCAAsigoKUpKSkggAJCoIACBiCgoKCAAkKggAIGIKCgoI=")]
// </GoSourcePositionMaps>

namespace go.net;

[GoPackage("netip")]
public static partial class netip_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
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
