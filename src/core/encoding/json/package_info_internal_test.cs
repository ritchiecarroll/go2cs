// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.encoding.json_package;
using static go.encoding.json_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b4120656e636f64696e672f6a736f6e2e4e756d62657220226a736f6e3a5c222c737472696e675c22227d", "Δtypeᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b4120656e636f64696e672f6a736f6e2e4e756d6265727d", "Δtype")]
[assembly: GoDynamicTypeLift("7374727563747b4e20656e636f64696e672f6a736f6e2e4e756d62657220226a736f6e3a5c222c737472696e675c22227d", "Δtypeᴛ3")]
[assembly: GoDynamicTypeLift("7374727563747b4e20656e636f64696e672f6a736f6e2e4e756d6265727d", "Δtypeᴛ2")]
[assembly: GoDynamicTypeLift("7374727563747b656e636f64696e672f6a736f6e2e436173654e616d653b20696e20737472696e673b2070747220616e793b206f757420616e793b20657272206572726f723b207573654e756d62657220626f6f6c3b20676f6c64656e20626f6f6c3b20646973616c6c6f77556e6b6e6f776e4669656c647320626f6f6c7d", "unmarshalTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f757420737472696e677d", "encodeStringTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<NoPanicStruct, global::go.encoding.json_package.isZeroer>(Pointer = true)]
[assembly: GoImplement<nilJSONMarshaler, global::go.encoding.json_package.Marshaler>(Pointer = true)]
[assembly: GoImplement<nilTextMarshaler, encoding_package.TextMarshaler>(Pointer = true)]
[assembly: GoImplement<u8marshal, encoding_package.TextUnmarshaler>(Pointer = true)]
[assembly: GoImplement<unmarshalerText, encoding_package.TextUnmarshaler>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<byteWithPtrMarshalJSON, byteWithMarshalJSON>(Inverted = true, ValueType = "byte")]
[assembly: GoImplicitConv<byteWithPtrMarshalText, byteWithMarshalText>(Inverted = true, ValueType = "byte")]
[assembly: GoImplicitConv<intWithPtrMarshalJSON, intWithMarshalJSON>(Inverted = true, ValueType = "nint")]
[assembly: GoImplicitConv<intWithPtrMarshalText, intWithMarshalText>(Inverted = true, ValueType = "nint")]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("encoding/json/bench_test.go", "bench_test.cs", "AD5cooKClJKCgpSCgpaEgIKmgIKmgoKCgoKCgqa0pKKCgoKClIKCgoCCyAALBqKCgoKCmJqEgoKCgIKkgILIpqKCgoKClIKCgILIAAgGooKCgoKYmoSCgoCCpICCyOaCpqqSgoCCABAMgqaumoSSgoCCpICC7KKUppSmopSmlNaigsqSgoCC7KKCgoKClIKCgoKClIKCgoCCyKaigoKCgoKCgoKAgqS4ooKCgoKCgoCCpIKCgoKUgoCktPqigoKCgpSCgoKAgsimooKCgoKUgoKCgILIpqKCgpKCgoCC7KKCgpKCgoCC7KKCgpKCgoCC7KKCgpKSgoCC7KKCgpKCgoCCAAsMooK6goKAguyigoKSgoKAguyigoKCqILIgoKogrqCgqKCgoKCgoKSgpSm3pSCgpSSkoIACw6ihJqChIKAggAKDKKCiJKCgILsooKC6KKCgoLoooKCgoKAgg==", "92-99:1;119-129:1;140-146:1;166-175:1;188-194:1;215-224:1;256-262:1;272-286:1;339-346:1;357-364:1;371-378:1;384-391:1;397-404:1;410-417:1;423-430:1;440-446:1;452-459:1;481-483:1;489-505:2;496-501:2.1;516-522:3;517-521:3.1;534-542:1;551-557:1")]
[assembly: go.GoPositionMap("encoding/json/decode_test.go", "decode_test.cs", "AEB6gsqCkoCCpIIACBLuABEmgoIAChiS1oKCgpSCpgAHEoKmlIKClIKClIKmlpKShJKShAB96AHIgqaCgpSCgpSCyoKmgsqCpoKClIKClILKgqaCyoKmgoKUgoKUgsqCpoLKgqaCgpSCgpSCyoKmggBWIAC5BZAMgoKClIKCgpaCgpSCgoIACQiCAAcYspKCgtyCgoKClIKC6IIAHj6CgpSCgriCggAIFKSWgpSClKaispKCgoCCgraCloKClJaE7paCgpSClICCkqakgIKCgriCgoKUgpSCgoKUgIKkggAIEoKCgoCCpIKClIKCggAMCpIABBiykoKAgqSAgpKCpICCkoLsgoKClIKClIKAgqSCggAHEIKCooCCpIK4goKSgIKkggAJCIKCgoKClIIADgykAAccspKCgoKCggBGngEAJ04AdeYBAFqyAQAJBIIABRqChIKAgqSCAA0MooqCgoCktAAPDKKMgoKCgoCktLToogANBoKCACpmspKSgoCCgIKkpIIAJlb+ACFAAB08hIKClIaWgpSClIKUgpSClIKUgpSClIKUgpaClIKUggAJDIIACAqCxoKEgoKUgoKClILesoSCgpSCgoKUgtyihIKClIKCgpSCAAkIggAJGLKSgoCCAAwOgoIACRqykoKAggAWIoKChIKCgpSCAAUQgoKUgoKUgtaCgoCktO6ygoSCggAKDsQAH0iykoKCgpSCAAwMggAIHLKSgKS0ABcQopIAARKCgpaCggBJHAAKAgBF7AGykoKClIIACwyCABUyspKCgoKClIIACBCApKKCgIK2gqKoooKChICC7IKC2qKCgIKmgIIACgiEkoCCpICCqIqCgpSCgIKkgqiEgoKUgoCCpIKCACEIggAZPKyCypbKlsqSuLKyooKCgqaC", "1228-1233:1;1292-1306:1;1319-1393:1;1429-1444:1;1524-1532:1;1979-1991:1;2237-2243:1;2263-2269:1;2378-2387:1;2407-2414:1;2572-2580:1;2611-2621:1;2630-2634:1;2752-2755:1;2758-2763:2;2766-2771:3;2774-2777:4;2782-2793:5")]
[assembly: go.GoPositionMap("encoding/json/encode_test.go", "encode_test.cs", "AE5gggAAFoKCgoSCgpSAgvyC/oIAV1yCAAAggoKCgoSChISCgoKEgoKUgIIAFgiCAAAcgoKClICCggA7RIIAABSCgoKChIKClICCABsYogAkRrKCgoKUgIKokoCCpIIADhaCgoKClIKClIKCgpSCAAcQ3AAHEoKCgoKWgoKChIKEgoKCgpSmgoCCyIKAggAJCIIACRqykoCCgIK2AAkOkriCgpSCgt6CpoKCzILMgqaCgsyCABAGggALJoKCgpSAgu6CzILmgoKCgoKUgIKmgoKCgpSAggCHAQiCAAUUAAcgAAYgAAYWAAcYioIABxCKggAIEgAIIgAHIAAINgAEErKSgoKUggAbPqKClAAEEKKClAAYCJIADiaykoCktAAMDpK4goKUgoKCpqaCgpSCgoIAEh6SuIKClIKCggAKFoIABxCCgpSCgoIACgiCmoCCAA8ItIKGmoKAgqaKgoKWgoCCpIK0pIKSgoKCggALCpKYgoKUgJKkgoCktLQACwgAIkyCgoKCgpSCggAJDoD4gPiA+ICkooKC+oAACAaSAA4mspKCgpSSggAIDILcgpSCguqSuIKUgoK4ABQGABMqgoKCgoKCgpSCgoKClJaCgoKClJKCgpaCgpSCgoKCupqCgoKUgoKCgoKClIKCmIKCgoLcgoKCACMGggAIHgBFjgGykoKAgoKUtoCCAAkQgNSigoCCtoKiAAkEgoaCgpSCgoIADwiCgoKEAAkcspKCggAHEIKmggALGIKC", "300-317:1;416-424:1;558-568:1;574-585:2;590-596:3;601-607:4;613-621:5;627-635:6;642-652:7;659-669:8;675-695:9;702-708:10;713-721:11;791-798:1;1066-1075:1;1137-1175:1;1197-1199:2;1305-1317:1;1326-1330:1;1370-1375:1")]
[assembly: go.GoPositionMap("encoding/json/fold_test.go", "fold_test.cs", "AAsYogAcNpSCkoKCgg==", "42-42:1;43-49:2")]
[assembly: go.GoPositionMap("encoding/json/fuzz_test.go", "fuzz_test.cs", "AAwaogAQIILKgoCCpoKCloCC7KIAECCCgoKCgoKClA==", "30-50:1;32-32:1.1;33-33:1.2;34-34:1.3;70-82:1")]
[assembly: go.GoPositionMap("encoding/json/number_test.go", "number_test.cs", "AAsYhJQAL2KCgpaCgIKmgqgAFjCCgpaCgIKmgg==")]
[assembly: go.GoPositionMap("encoding/json/scanner_test.go", "scanner_test.cs", "ABAggqaCgoKUAAkIggAHGLKSgIIADAyiABYwgrKSgoCCkoCCtoKAgpKAgraCgIKSgIK2goCCkoCCAAwMpr6ykoKAgpKAggAKEJKCgoCCpIKCgoL4ooKCgoCCpIKmmJKAgqSCgoKCqIKAgqSCgoKCAAkIggADELKSgoKAgoL+goKCgoKClILKgt6CgoKUgoKUpoKClKS2lKSkpKaCgoKCgoKUlKaCgoKUgpSCgpSmgoKClIKUgoKU", "21-26:1;43-47:1;78-106:1;121-128:1;197-205:1")]
[assembly: go.GoPositionMap("encoding/json/stream_test.go", "stream_test.cs", "ACNAsoKC/KKCgtoAFiqCgoKUgoKCgIK2gJKCggAQCrSChpqEgoKAgqaKgIKmgoCCpIK0ABUmgoKCgoKUgJKC7ILKggAfBoKCggADEJyIABMyspKCgoCCpICCpIKCgIKkgIL+gozSgoKmgoKCgIK2goKCgqYADAqCgoaCgoKUgpSCgpSAksiCgpSCgoCCyAAOBoKKgoKCgpSClIKClIIADQiCjIKCgpSAkqSClIKClIIACAiCvrKSgpKogIKkggAOEoIARJQBspKCooKEgIKClKSAgoKUkoKkggANELKEgpSSgoKUlJiCgoKUgqiCgrQ=", "216-234:1;366-378:1;462-486:1;494-496:1")]
[assembly: go.GoPositionMap("encoding/json/tagkey_test.go", "tagkey_test.cs", "AGCWAYIAES6ykoKClIKCgpSCgoCCtg==", "100-119:1")]
[assembly: go.GoPositionMap("encoding/json/tags_test.go", "tags_test.cs", "ABISgoKClAAEEII=")]
// </GoSourcePositionMaps>

namespace go.encoding;

[GoPackage("json")]
public static partial class json_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸcompressꓸgzip() => builtin.initPackage(typeof(compress.gzip_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(go.encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸimage() => builtin.initPackage(typeof(image_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸmaps() => builtin.initPackage(typeof(maps_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(go.math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(go.math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttp() => builtin.initPackage(typeof(go.net.http_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸhttptest() => builtin.initPackage(typeof(go.net.http.httptest_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpath() => builtin.initPackage(typeof(path_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸruntimeꓸdebug() => builtin.initPackage(typeof(go.runtime.debug_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}
