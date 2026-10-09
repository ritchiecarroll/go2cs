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
[assembly: go.GoPositionMap("log/slog/attr_test.go", "attr_test.cs", "ABMcooKUhAAPFJKCgoKCgoKUgpSCgoKCpqIACBaCgoKCgoKCgpSCgoKCgoI=", "30-38:1")]
[assembly: go.GoPositionMap("log/slog/handler_test.go", "handler_test.cs", "ACcwgoKCggA2eqKCkoKUgpSCgoCCpIIACwyCgoKCkoKClKSkpIKCgoKSgpKCgtKCgIKkgILEpIKCgoKCAB8QxISChACDAvQDgoKClAAIEoKUgpQARrIBgpQACQ6SgoKCkpIABhCigoKUgoCCpIKCggAHFMKSgoKmuIKCAAoOggAJCqKCgoKWAAgWgoKCAA8KhqKCuoKCgoKCggANCIQAABCCgoKUgqQABhwADBqC3ILKgoKCyqKCgoKCAAoIgoKCgqKogoKCgoKCgoI=", "45-45:1;63-63:2;69-74:3;80-84:4;89-106:5;91-94:5.1;114-152:1;134-142:1.1;159-159:1;202-202:2;211-211:3;220-220:4;236-236:5;320-320:6;328-334:7;342-346:8;354-356:9;363-365:10;372-374:11;413-420:12;427-435:13;441-441:14;448-450:15;451-451:16;458-460:17;461-461:18;468-468:19;469-469:20;476-478:21;479-479:22;486-488:23;489-491:24;498-500:25;501-503:26;510-512:27;513-515:28;522-527:29;538-563:30;547-561:30.1;570-577:1;596-600:1;651-658:1;720-722:1", "", "662=String/1/2/1,String/2/2/2;713=String/1/2/1,String/2/2/2,Int/1/1/3;748=Int/1/8/1,With/1/3/1,WithGroup/1/2/2,Int/2/8/3,With/2/3/3,WithGroup/2/2/4,Int/3/8/6,Int/4/8/7,Group/1/2/7,Int/5/8/8,With/3/3/5,Int/6/8/10,Int/7/8/11,Group/2/2/11,Int/8/8/12,Info/1/1/9")]
[assembly: go.GoPositionMap("log/slog/json_handler_test.go", "json_handler_test.cs", "AB8uggAOIIKCgoKCgIKkgoIAChaA1IKClAAJDoCktAASJoKCgpSCyoKCgoKAgqT2lAAFEoKCyoKCgoCCpAAXBoK+goKUgpTKgoKUgpTogoLKgoKCAB0WogAJJIKClJKAgrgADiCCooKCgoIACRLkooKCkpKCgoKClIKmkoKCgIKkpqI=", "40-52:1;152-161:1;164-173:2;176-193:3;220-224:1;243-255:2;263-273:1;274-282:2", "", "227=String/1/4/1,String/2/4/2,String/3/4/3,String/4/4/4;235=String/1/2/1,String/2/2/2,Int/1/2/3,Int/2/2/4;313=String/1/2/1,String/2/2/2,Int/1/2/3,Int/2/2/4")]
[assembly: go.GoPositionMap("log/slog/level_test.go", "level_test.cs", "ABMcggAMIIKCyoKCgJKkgoCSpIKAksqCgoKCgpSClIKAgqSCuIKCgoKClIKUgoCCpIK4goKCgoKClIIACAiCAAocgoCCpIIACQqCAAYUgoKCAAkKgoKSgoKClICSyIKCgoKClIKAgqSAksiCgoKCgoKUgoCCpICSyIKCgoKCgoKUgJL4goKCgoKC")]
[assembly: go.GoPositionMap("log/slog/logger_test.go", "logger_test.cs", "AC5GgoKEhIKCgpSCloKWgoSChIKEgoSChIKEggAQBoLKgoKCgqKCgqiCgoKCgoKCgoKCgoKCgoKCloKChLiCgrqCgpaCgoKAgriCgoKCgoKAggAIEIKkgKKAoqCkgpKCgqiCgoKCggAIBqKCgoSSgoKCgoKCgrqCgqiCloKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCogAVBKKCgoKEgoCkgoCkkpCkoqCkkoKCksySgoKCosySgoKCooLekoKCgpLckpCkoqCSoKSioqaSgqKmoqLKoqL89IIABxaCgoKCyrSCkoCSgpKCgoKUgoCCxAAIBqSCgoKSgoKWgoKEAAUUgoKC6pKCgpTMgoKCgqKCgoKWgoKCgoKC5oKEgoKUlIKCgpSCgqaCgoKCgtaCgoKUgpSCAAgIxoKCgoQACRqChIKAgtakgoKCgoKClIK6koKUAAoW4oKCgoLWgKTSgoKCgoKC1tKCgoKCgoLW0oKCgqLsgKKAooKCpIKqooKCggAKCLKCgqKCgqaigoKmooKigriigoKmooKCpqKCooLekoKC1oKClIKCgoIACBKCpoL2uIKCgqKCgpaCgoSCAAUSgoKWggAFEoKC", "41-48:1;86-90:1;116-123:2;163-168:1;183-194:1;238-240:1;239-239:1.1;241-243:2;242-242:2.1;244-246:3;245-245:3.1;247-249:4;248-248:4.1;250-259:5;253-258:5.1;260-270:6;264-269:6.1;271-283:7;275-282:7.1;284-294:8;288-293:8.1;295-297:9;296-296:9.1;298-301:10;299-299:10.1;300-300:10.2;302-306:11;303-305:11.1;307-312:12;309-311:12.1;313-319:13;314-318:13.1;320-327:14;321-326:14.1;355-355:1;357-362:2;375-379:1;404-409:1;418-423:2;437-442:1;598-603:1;604-609:2;610-617:3;612-616:3.1;618-623:4;624-629:5;630-637:6;632-636:6.1;679-683:1", "", "416=Int/1/2/1,String/1/2/1,Duration/1/2/1,Int/2/2/2,String/2/2/2,Duration/2/2/2;427=Int/1/3/1,String/1/3/1,Duration/1/3/1,Int/2/3/2,String/2/3/2,Duration/2/3/2,Int/3/3/3,String/3/3/3,Duration/3/3/3")]
[assembly: go.GoPositionMap("log/slog/record_test.go", "record_test.cs", "AA4egpSCgJKkgILKgoKCgpSCggALCrQABhaCgpSCgoCCpIIABxCCgoKClJaSgoKCzIKUgoKUlIKCgoKCqIKCgoKmgoKCpqKSgICSpoKqopKCgoKClMqigoSCgoKUgICk", "30-33:1;73-79:1;81-87:2;124-124:1;136-143:1;156-156:1", "", "16=Int64/1/1/1,Float64/1/1/1,Uint64/1/1/1;115=String/1/1/1,Int/1/1/1")]
[assembly: go.GoPositionMap("log/slog/text_handler_test.go", "text_handler_test.cs", "ABMmABcEogAnUpIAECaSgoKCgoCCpJSCgoIADBqA/oDUgoKU1oKCgpSSgoCCpIKCgriCgpKClKKAlIKCgAAIBqIAChyCgg==", "63-100:1;74-74:1.1;83-98:1.2;149-149:1;153-153:2")]
[assembly: go.GoPositionMap("log/slog/value_test.go", "value_test.cs", "ABQigoCS+KKSABgugoKCgoLcsoKAgraCAAsGggAJGoCCAAgKooKYABEWkoKCgoKCgoKUgpSCgoKCgqaGooKAkvaCABQwgoLcggAPHIKCggAICoKCgoKAkqSCgqiCgqiCgoCCuIKCgoKCqIKCgoK4gJLIlOqCgpSAgtqCuIKCggAHEIDYgK7ygoKCgoKUgoKCgoKm", "60-64:1;106-115:1;132-132:1", "", "38=Int64Value/1/2/2,Int64Value/2/2/3,Float64Value/1/2/4,Float64Value/2/2/5,BoolValue/1/2/6,BoolValue/2/2/7,TimeValue/1/5/8,TimeValue/2/5/9,Date/1/3/10,TimeValue/3/5/10,Date/2/3/11,TimeValue/4/5/11,Date/3/3/12,TimeValue/5/5/12,AnyValue/1/2/13,AnyValue/2/2/14,Bool/1/3/15,Int/1/3/15,GroupValue/1/3/15,Bool/2/3/16,Int/2/3/16,GroupValue/2/3/16,Bool/3/3/17,Int/3/3/17,GroupValue/3/3/17,DurationValue/1/2/18,DurationValue/2/2/19,StringValue/1/2/20,StringValue/2/2/21;206=Kind/1/2/1,Kind/2/2/1;315=Int/1/2/1,Group/1/4/2,Group/2/4/2,Int/2/2/3,Group/3/4/3,Group/4/4/3")]
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
    [GoInit] internal static void initᴛᴛimportꓸlogꓸslogꓸinternalꓸslogtest() => builtin.initPackage(typeof(go.log.slog.@internal.slogtest_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
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
