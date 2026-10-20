// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.log.slog_package;
using static go.log.slog_internal_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<captureHandler, global::go.log.slog_package.ΔHandler>(Pointer = true)]
[assembly: GoImplement<discardTestHandler, global::go.log.slog_package.ΔHandler>]
[assembly: GoImplement<mockFailingHandler, global::go.log.slog_package.ΔHandler>(Pointer = true)]
[assembly: GoImplement<wrappingHandler, global::go.log.slog_package.ΔHandler>]
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
[assembly: go.GoPositionMap("log/slog/attr_test.go", "attr_test.cs", "ABMcgoKUhAAIFJKCgoKCgoKUgpSCgoKCpqIACBaCgoKCgoKCgpSCgoKCgoI=", "30-38:1")]
[assembly: go.GoPositionMap("log/slog/handler_test.go", "handler_test.cs", "ACo0goKCggA2eqKCkoKUgpSCgoCCpIIACwyCgoKCkoKClKSklIKCgoKSgpKCgtKCgIKkgILEpIKCgoKCACQQxJSClACAAvQDgoKClAAIEoKUgpQARrIBgpQAFSqSgoKUgoKCkpIABhCigoKUgoCCpIKCggAHFMKSgoKmuIKCAAoOggAJCqKCgoKmAAcWgoKCAAoKhqLKAAYQkoKCgoKmgoKCABAMhqKCuoKCgoKCggANCIQAARCCgoKUgqQABxwADBqC3ILKgoKCyqKCgoKCAAoIgoKCgqK4goKCgoKCgoKmgoKCgtyClIKmlIKC", "47-47:1;65-65:2;71-76:3;82-86:4;91-108:5;93-96:5.1;116-154:1;136-144:1.1;161-161:1;204-204:2;213-213:3;222-222:4;238-238:5;322-322:6;330-336:7;344-348:8;356-358:9;365-367:10;374-376:11;415-422:12;429-437:13;443-443:14;450-452:15;453-453:16;460-462:17;463-463:18;470-470:19;471-471:20;478-480:21;481-481:22;488-490:23;491-493:24;500-502:25;503-505:26;512-514:27;515-517:28;524-529:29;558-583:30;567-581:30.1;590-597:1;616-620:1;658-671:1;705-712:1;774-776:1;793-813:1", "", "684=String/1/2/1,String/2/2/2;729=removeKeys/1/1/1;775=String/1/2/1,String/2/2/2,Int/1/1/3;811=Int/1/8/1,With/1/3/1,WithGroup/1/2/2,Int/2/8/3,With/2/3/3,WithGroup/2/2/4,Int/3/8/6,Int/4/8/7,Group/1/2/7,Int/5/8/8,With/3/3/5,Int/6/8/10,Int/7/8/11,Group/2/2/11,Int/8/8/12,Info/1/1/9")]
[assembly: go.GoPositionMap("log/slog/json_handler_test.go", "json_handler_test.cs", "ACEuggAOIIKCgoKCgIKkgoIAChaA1IKClAAJDoCktAASJoKCgpSCyoKCgoKAgqT2lAAFEoKCyoKCgoCCpAASBoKCgoIABBCiorrCwgAfGIK+goKUgpTKgoKUgpTogoLKgoKCAB0WogAKJIKClJKAgsgADiCCooKCgoIACRLkooKCkpKCgoKClIKmkoKCgIKkpqI=", "40-52:1;157-163:1;158-162:1.1;164-175:2;165-174:2.1;185-194:1;197-206:2;209-226:3;253-257:1;276-288:2;296-306:1;307-315:2", "", "220=String/1/1/2,Int/1/1/3,Bool/1/1/4,Any/1/2/5,Any/2/2/6;285=String/1/4/1,String/2/4/2,String/3/4/3,String/4/4/4;293=String/1/2/1,String/2/2/2,Int/1/2/3,Int/2/2/4;373=String/1/2/1,String/2/2/2,Int/1/2/3,Int/2/2/4")]
[assembly: go.GoPositionMap("log/slog/level_test.go", "level_test.cs", "ABMcggAMIIKCyoKCgJKkgoCSpIKAksqCgoKCgpSClIKAgqSCuIKCgoKClIKUgoCCpIK4goKCgoKClIIACAiCAAocgoCCpIIACQqCAAYUgoKCAAkKgoKSgoKClICSyIKCgoKClIKAgqSAksiCgoKCgoKUgoCCpICSyIKCgoKCgoKUgJL4goKCgoKCuKIADBqCgoI=")]
[assembly: go.GoPositionMap("log/slog/logger_test.go", "logger_test.cs", "ADRQgoKUlIKCgpSCpoKmgpSClIKUgpSClIKUggAQBoLagoKCgqKCgriCgoKCgoKCgoKCgoKCgoKCpoKClLiCgsqCgqaCgoKAgsiCgoKCgoKAggAIEIKkgKKAoqCkgpKCgriCgoKCggALBqKCgpSSgoKCgoKClIKCyoKCuIKmgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKiABYEooKmgoKCppiioqKioqKioqKioqKiyIKClJSylJKCpoKUpoKCgriolIKAgraCgpSUqIIAGRKigoKClIKApIKApJKQpKKgpJKCgpLMkoKCgqLMkoKCgqKC3pKCgoKS3JKQpKKgkqCkoqKmkoKipqKiyqKi/PSCAAcWgoKCgsq0gpKAkoKSgoKClIKAgsQACAakgoKCkoKCpoKClAAFFIKCguqSgoKU3IKCgoKigoKCpoKCgoKCguaClIKClJSCgoKUgoKmgoKCgoLWgoKClIKUggAICMaCgoKUAAkagpSCgILWpIKCgoKCgpSCupKClAAKFuKCgoKC1oCk0oKCgoKCgtbSgoKCgoKC1tKCgoKi7ICigKKCgqSCqqKCgoIACQiygoKigoKmooKCpqKCooK4ooKCpqKCgqaigqKC3pKCgtaCgpSCgoKCAAgSgqaC9riCgoKigoKmgoKUggAFEoKCpoIABRKCgg==", "46-53:1;91-95:1;121-128:2;168-173:1;188-202:1;255-255:1;256-256:2;257-257:3;258-258:4;259-259:5;260-260:6;261-261:7;262-262:8;263-263:9;264-264:10;265-265:11;266-266:12;267-267:13;268-268:14;279-326:15;285-290:15.1;336-338:1;337-337:1.1;339-341:2;340-340:2.1;342-344:3;343-343:3.1;345-347:4;346-346:4.1;348-357:5;351-356:5.1;358-368:6;362-367:6.1;369-381:7;373-380:7.1;382-392:8;386-391:8.1;393-395:9;394-394:9.1;396-399:10;397-397:10.1;398-398:10.2;400-404:11;401-403:11.1;405-410:12;407-409:12.1;411-417:13;412-416:13.1;418-425:14;419-424:14.1;453-453:1;455-460:2;473-477:1;502-507:1;516-521:2;535-540:1;696-701:1;702-707:2;708-715:3;710-714:3.1;716-721:4;722-727:5;728-735:6;730-734:6.1;777-781:1", "", "411=Name/1/1/1,QuoteMeta/1/1/1;590=Int/1/2/1,String/1/2/1,Duration/1/2/1,Int/2/2/2,String/2/2/2,Duration/2/2/2;601=Int/1/3/1,String/1/3/1,Duration/1/3/1,Int/2/3/2,String/2/3/2,Duration/2/3/2,Int/3/3/3,String/3/3/3,Duration/3/3/3")]
[assembly: go.GoPositionMap("log/slog/multi_handler_test.go", "multi_handler_test.cs", "ACIsooIAFwaCgpKClIKUlIKmgoKUlIKUgriCgpSUgriCkoKUgpSUgqaCkoKUgpSUgqaClJKClJSCgqaCpoKClJSCgsySgoKCgpSCgpQ=", "28-40:1;42-54:2;56-65:3;67-79:4;81-93:5;95-111:6;113-123:7")]
[assembly: go.GoPositionMap("log/slog/record_test.go", "record_test.cs", "AA4egpSCgJKkgILagoKCgpSCggAMCrQABRiCgpSCgoKClJSCgpSAgqSCAAcQgoKCgpSmkoKCgtyClIKClJSCgoKCgriCgoKCpoKCgqaikoCAkqaCqqKSgoKCgpTKooKUgoKClICApA==", "30-33:1;84-90:1;92-98:2;135-135:1;147-154:1;167-167:1", "", "16=Int64/1/1/1,Float64/1/1/1,Uint64/1/1/1;128=String/1/1/1,Int/1/1/1")]
[assembly: go.GoPositionMap("log/slog/text_handler_test.go", "text_handler_test.cs", "ABMmABcEogAnUpIAECaSgoKCgoCCpJSCgoIADBqA/oDUgoKU1oKCgpSCgoCCpIKCgriCgpKClKKApIKCgAAIBqIAChyCgg==", "63-100:1;74-74:1.1;83-98:1.2;149-149:1;153-153:2")]
[assembly: go.GoPositionMap("log/slog/value_test.go", "value_test.cs", "ABQigoCS+KKSABYugoKCgoLcsoKAgraCAAsGggAJGoCCAAgKgoKoAAkWkoKCgoKCgoKUgpSCgoKCgqaGooKAkvaCABQwgoLcggANHIKCggAICoKCgoKAkqSCgriCgriCgoCCyIKCgoKCuIKCgoK4gJLIlMqCgpSAgtqCuIKCggAHEIDYgK7ygoKCgoKUgoKCgoKm", "60-64:1;106-115:1;132-132:1", "", "38=Int64Value/1/2/2,Int64Value/2/2/3,Float64Value/1/2/4,Float64Value/2/2/5,BoolValue/1/2/6,BoolValue/2/2/7,TimeValue/1/5/8,TimeValue/2/5/9,Date/1/3/10,TimeValue/3/5/10,Date/2/3/11,TimeValue/4/5/11,Date/3/3/12,TimeValue/5/5/12,AnyValue/1/2/13,AnyValue/2/2/14,Bool/1/3/15,Int/1/3/15,GroupValue/1/3/15,Bool/2/3/16,Int/2/3/16,GroupValue/2/3/16,Bool/3/3/17,Int/3/3/17,GroupValue/3/3/17,DurationValue/1/2/18,DurationValue/2/2/19,StringValue/1/2/20,StringValue/2/2/21;195=Kind/1/2/1,Kind/2/2/1;304=Int/1/2/1,Group/1/4/2,Group/2/4/2,Int/2/2/3,Group/3/4/3,Group/4/4/3")]
// </GoSourcePositionMaps>

namespace go.log;

[GoPackage("slog")]
public static partial class slog_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("req")] partial struct BenchmarkPreformatting_req {}
    [GoLocalName("ga")] partial struct TestReplaceAttrGroups_ga {}
    [GoValueClone("r")] partial struct captureHandler {}
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
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸrace() => builtin.initPackage(typeof(go.@internal.race_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(go.@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸlogꓸslog() => builtin.initPackage(typeof(go.log.slog_package));
    [GoInit] internal static void initᴛᴛimportꓸlogꓸslogꓸinternalꓸbuffer() => builtin.initPackage(typeof(go.log.slog.@internal.buffer_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
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
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸslogtest() => builtin.initPackage(typeof(go.testing.slogtest_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}
