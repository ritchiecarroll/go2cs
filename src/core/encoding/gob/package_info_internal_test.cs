// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static go.encoding.gob_package;
using static go.encoding.gob_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b4120696e747d", "Δtype")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<Float, Squarer>]
[assembly: GoImplement<Int, Squarer>]
[assembly: GoImplement<benchmarkBuf, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<go.encoding.gob_internal_test_package.Point, Squarer>]
[assembly: GoImplement<go.encoding.gob_internal_test_package.Vector, Squarer>]
[assembly: GoImplement<interfaceIndirectTestT, interfaceIndirectTestI>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<LargeSliceByte, ж<LargeSliceByte>>(Indirect = true)]
[assembly: GoImplicitConv<LargeSliceInt8, ж<LargeSliceInt8>>(Indirect = true)]
[assembly: GoImplicitConv<LargeSliceString, ж<LargeSliceString>>(Indirect = true)]
[assembly: GoImplicitConv<LargeSliceStruct, ж<LargeSliceStruct>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("encoding/gob/codec_test.go", "codec_test.cs", "ABYoAAcQABAmwoCCxKSCzLKCgoKCgoKCpoKCgoKCgpSCxqSigoKCgoKCgrSmkpSCgoKCgqa4ppKSpqaUgoKCpqKCgoLaopaCgoKCgoK6goKCgoKCgrqCgoKCgoKCuoKCgoKCgoK6goKCgoKCgrqCgoKCgoKCuoKCgoKCgoK6goKCgoKCgrqCgoKCgoKCuoKCgoKCgoK6goKCgoKCgrqCgoKCgoKCuoKCgoKCgoK6goKCgoKCgrqCgoKCgoKCytKCgoKUgqKkgoKCgtqiloKCgoKCgriCgoKCgoK6goKCgoKCuoKCgoKCgrqCgoKCgoK6goKCgoKCuoKCgoKCgrqCgoKCgoK6goKCgoKCuoKCgoKCgrqCgoKCgoK6goKCgoKCuoKCgoKCgrqCgoKCgoK6goKCgoKCuoKCgoKCgrqCgoKCgoK6goKCgoKCACgKggABMpKSkpKSkoKCgoKCgoKCgoKCgoKCABImgoKClIKCgpSCpoKUggA/CIIAABSCgoKWgqaKgoKClKaCgoKCqIKmioKCgpSmgoKCgqiCpoqCgoKUpoKCgoKogqaIgoKCqIKmiIKCgqiCpoiCgoKogqaKgoKCqIKmioKCggANCIKKgoKCgoKCgoKClIKUgpSCABkwhJKCgoKCgoKCgoKCgoKCgoKCgpiSgoKCgoKCgoKCgoKCgoKCqIKCgoKCgqiCgoKCgqiCgoKCgoKCggATIIKCgoKCgoKClIKClIIAFSSCgoKCgoKCgoKCgoKChIKCgpSCgpSCAAoIgoSCgoKSggAZIpSSgoKCgoKCgoKCgoKCgoKCgoKCgpSClIKUgqaCgoKCgoKCgoKUgpSClIIAChaCyoLKgoKClO6CABgigpK2goKUgoKCgoKWkoKClIKUgpSClIKUgqaClIKCgoKkggASIIKCAAgSgoKWkoKClIKmgIKkgIIADxqigpKSyoKCgpaSgoKmgIKkgILIgpK2koKUgoKCgoKWkoKClIKUggANFoKCgoKCgoKCgoKCgoKUgpSCuAAKFoKClJSCgoKmABQqgoKCgoKCgoKCgoKmgoKUgoKCgoKUgpKCgpSmsoKCgIKmgoKCqIKCgIKkqJKCqAAJFKaCgqimpqKCgoKCzqKCloKCgoCCpISCgpS45JSClIKSgoKCsoKAgoK2gqLewoLcgoKCggAYMoKCgoKClIKClIIACQiCgoKUgoKUgoKUgoKUgoKUgoKUgoKUgoKClIKClIKClIKClIKC6JSEgoKCgg==", "1499-1508:1;1500-1505:1.1;1569-1580:1;1581-1592:2;1593-1605:3;1606-1617:4")]
[assembly: go.GoPositionMap("encoding/gob/encoder_test.go", "encoder_test.cs", "ABgskgATKIKCgoKClIKCgoKUggAJCoSCgoKEkoKChIKShIKokoKChIKShIKokoKChIKShIKokoKChIKShIIALEaigoKCgoKCgriCgoKCloKUgqiCgoKCgoKClIKCgoKWgpSCloKCgoKUgpSCqIKClIKCgvyigoKCgoKCgpSCgoKUggAICpKCgrjKgoKCgoKCyoKCgoKClIKCgpT2hIiCgIIACQiEiIKAggAICIKIgoKCgoKCgIKkgviCiIKAggANCISIgoCCABAIggAAEIKAgqSCgILogoSSgILogoSSgIIACwqSAAUYgoCCAB4iAAoagoKCgoKCgoKClIKUgrSCtIKUxoKCABEKgpqCgIKkgoCCkoKmkoCCpICCkoIACxSC3LKCgoKCgpaCgoIAIDIACB6CgoKCgoKCgpSCgoKCppKCgpSCgoKUgsoAChLulIKCABAagoKCgoKCgqKCgpSCgqaCgpSCgpSCgpSCAA8agoKChIKCgoKUgpKCgpSCuIIAEyiCgoL4gpSCgoKCgqaSgoKCgpSClIK4goKCgoKmkoKCgoKUgpSCAAgMkoKAgpKC/rKCgoKCgoCCtoKCgoCCpIIAERyCkqKSgoKCgIKkgoKClIKUguiCgoKAggAKFILIgoKCloKCgpSCAAgMooKCgoKEgoKClIKCgpSC7KKCgpSCkoIACQqSgoKClIKUgoK4woKCgpTEyoIACgaCAAsaAAsggoKCgpSUggANCoKCqoKCgpSCggARHqK4goKCgpSCgoKClIIAChiCgoKClIKCgoKWgoKCgvy0gpKCgqaUgoKUggANFAALIKKCgoKUgoKCgpSCAAwKgoqEkoKCgpaCgoKCkoIACAqSgoTMgoKAgqaSgoCCppqClIKCgpSUloKCggAICIKMkpKEgoCCpoKAgqaCgoaigoKCgpaWkriSlIKUgr7C7JS4goKC", "69-81:1;83-95:2;97-109:3;111-123:4;822-822:1;933-943:1;1187-1199:1;1191-1197:1.1")]
[assembly: go.GoPositionMap("encoding/gob/gobencdec_test.go", "gobencdec_test.cs", "ACxikoKCgoLmooKmgpSCgoKmpoKmlIKUgoKCpoKmgtaCgpSCpoKmgoKmgqaCgqaCpoKCpoKmgoKmgqaCgqaCpoKCAFOqAYKUgoKClIKCgoKUgqaCkpKSgoKUgoKClIK8opSCgoKUgoKCgpSCvKKUgpKSgoKUgoKCgpSCupKCgoKCgpSCgpSCgoKClIKCgsySgoKCgoKSkoKClIKClIKCgoKUgoKCAAQU5IKCgoKUgoKCgpSCpoKCgpSCgoKUgrqkgoKCgpSCgoKClIIACQyigpKCkoK6goKAgqSCgoCCpoCSpICSkoKmgJKkgJKSgqaAkqSAkpKCzKKSgpKCkoSCgoCCpIKCgIKmgJKSgqSAkqaAkpKCpICSpoCSkoKkgJIACQiUgoKCgpSCgoKClIKmgoKClIKCgpSCupKCgoKClIKCgoKUgriCgoKSgoKUgoKCgpSCuIKUgoKClIKCgoKUgriClIKSkpKCgpSCgoKClIK4gpSCgoKUgoKCgpSClIIABxCCpoKmgoKCqqKCgoKAgqSCgoCCpIIAECTCgqaigpSCgu6CpoKmgoKCgoKClIKCgoKUgriCgoKCgoKUgpKCgpSCuJSEgoKClIL45oKCgJKCloKCgpSCgoKCgoCCuIKClIKCgoCCxA==", "806-806:1")]
[assembly: go.GoPositionMap("encoding/gob/timing_test.go", "timing_test.cs", "ABkuooKCgoKUgoKCgoCCpICC7IK2kriChoK4goKCgpKClJSC+IKClIKWhIKChKKCgqaCuIKClIKWhIKCppKCgqiCkoKCgqaCuKKCgoKChIKCgoLcgoKClKaCgoKUpoKCgpTWgoKClKaCgoKUAAgSsoKClIKmsoKUgoKmgqaigoKCgpaSgoSCtIKChJSCgoKCgtyCgoKUpoKCgpSmgoKClKaCgoKUpIKCgpSkgoKClKaCgoKUpqKCgoKUgoKCgpSSgoKCgoKCgoI=", "25-41:1;45-47:1;47-50:2;54-56:1;56-59:2;63-71:1;71-74:2;91-96:1;117-122:1;125-131:2;140-151:1;235-251:1")]
[assembly: go.GoPositionMap("encoding/gob/type_test.go", "type_test.cs", "ABYmAAgSooKCgoKU2JKCgpSCAAgMkoKClIKClIKC+IKCgoKClIKCgpSCgoKUgoKCAAgIgoKCgoKClIKCgpSCgoIACAiCgoKCgoKUgoKClIKCggAYKoKClIKC/KKEAA0OsgADEIKEgoKCpoKUgIIACQqChIKCkoKCgoKCgpSCgoKUpoLqkoKCgoKygoKCgoKUtLSCpJKCgIKCpICCgqSAgoLEpII=", "201-216:1;229-258:1")]
// </GoSourcePositionMaps>

namespace go.encoding;

[GoPackage("gob")]
public static partial class gob_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸgob() => builtin.initPackage(typeof(go.encoding.gob_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(go.encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸmaps() => builtin.initPackage(typeof(maps_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(go.math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}
