// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.time_package;
global using static global::go.time_internal_test_package;

// <ImportedTypeAliases>
global using bigꓸInt = go.math.big_package.ΔInt;
global using bigꓸRat = go.math.big_package.ΔRat;
global using jsonꓸToken = object;
global using jsonꓸΔToken = object;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using runtimeꓸError = go.runtime_package.ΔError;
global using syscallꓸHandle = go.syscall_package.ΔHandle;
global using syscallꓸSignal = go.syscall_package.ΔSignal;
global using syscallꓸSockaddr = go.syscall_package.ΔSockaddr;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
using Δtesting = go.testing_package;
using Δtime = go.time_package;
// </ImportedTypeAliases>

using go;
using static global::go.time_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b4e616d6520737472696e673b20546573742066756e632874696d652e74657374696e6754297d", "InternalTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6279746573205b5d627974653b2077616e7420737472696e677d", "invalidEncodingTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b636f756e7420696e743b206465736320737472696e673b20666e2066756e6328297d", "mallocTestᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b642074696d652e4475726174696f6e3b206d2074696d652e4475726174696f6e3b2077616e742074696d652e4475726174696f6e7d", "durationTruncateTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b642074696d652e4475726174696f6e3b2077616e7420666c6f617436347d", "secDurationTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b642074696d652e4475726174696f6e3b2077616e7420696e7436347d", "nsDurationTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b642074696d652e4475726174696f6e3b2077616e742074696d652e4475726174696f6e7d", "durationAbsTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6461746520737472696e673b206f6b20626f6f6c7d", "dayOutOfRangeTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b2065787065637420737472696e677d", "parseDurationErrorTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b2077616e742074696d652e4475726174696f6e7d", "parseDurationTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e2074696d652e54696d653b2077616e7420737472696e677d", "goStringTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6d6f6e6f20696e7436343b2077616e7420737472696e677d", "monotonicStringTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20662066756e632874312074696d652e54696d652c2074322074696d652e54696d652920626f6f6c7d", "defaultLocTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73747220737472696e673b20642074696d652e4475726174696f6e7d", "durationTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73756d20666c6f617436343b206d61782074696d652e4475726174696f6e3b20636f756e7420696e7436343b205f205b355d696e7436347d", "BenchmarkParallelTimerLatency_type")]
[assembly: GoDynamicTypeLift("7374727563747b742074696d652e54696d653b20642074696d652e4475726174696f6e7d", "truncateRoundTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b742074696d652e54696d653b20752074696d652e54696d653b20642074696d652e4475726174696f6e7d", "subTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b74696d652074696d652e54696d653b206a736f6e20737472696e677d", "jsonTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b74696d652074696d652e54696d653b2077616e7420737472696e677d", "notEncodableTimesᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b76616c756520737472696e673b206f6b20626f6f6c7d", "monthOutOfRangeTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b76616c756520737472696e673b2077616e7420696e747d", "longFractionalDigitsTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7965617220696e743b206d6f6e746820696e743b2064617920696e743b20686f757220696e743b206d696e20696e743b2073656320696e743b206e73656320696e743b207a202a74696d652e4c6f636174696f6e3b20756e697820696e7436347d", "dateTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7965617220696e743b206d6f6e746820696e743b20646920696e747d", "daysInTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b796561727320696e743b206d6f6e74687320696e743b206461797320696e747d", "addDateTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7a6f6e654e616d6520737472696e673b2066696c654e616d6520737472696e673b20646174652066756e63282a74696d652e4c6f636174696f6e292074696d652e54696d653b2077616e744e616d6520737472696e673b2077616e744f666673657420696e747d", "slimTestsᴛ1")]
[assembly: GoTypeAlias("Hour", "const:ΔHour")]
[assembly: GoTypeAlias("Local", "const:ΔLocal")]
[assembly: GoTypeAlias("Location", "ΔLocation")]
[assembly: GoTypeAlias("Minute", "const:ΔMinute")]
[assembly: GoTypeAlias("Month", "ΔMonth")]
[assembly: GoTypeAlias("Nanosecond", "const:ΔNanosecond")]
[assembly: GoTypeAlias("Second", "const:ΔSecond")]
[assembly: GoTypeAlias("UTC", "const:ΔUTC")]
[assembly: GoTypeAlias("Weekday", "ΔWeekday")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<testing_package.T, global::go.time_internal_test_package.testingT>(Pointer = true)]
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
[assembly: GoImplement<tickerTimer, timer>(Pointer = true)]
[assembly: GoImplement<time_package.Timer, timer>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<ParseTest, ж<ParseTest>>]
[assembly: GoImplicitConv<parsedTime, ж<parsedTime>>]
[assembly: GoImplicitConv<tickerTimer, ж<tickerTimer>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("time/example_test.go", "example_test.cs", "AA4apIKCgoKmgoKClgAJFoIAAxoACQKCrLKCgpYACRaCAAMaAAkCgoKUhIKCgoIAAhLigoKCpoKCgqiSgoKEgoKEgoIAAhDCgqiSgqqigqqigqiSgqqigtzUgrS0yIKmgKSCgoLogoKCuIKC6LKCkoKSgpSCtIKkABkKlJKSloKSqJaWhIQAECqCgoKClKiWgrrMgoK4zJYACygAEASSkqiSgoKClKjMlpa6AAYW4oKCgoKCgoIADRIACA6igoqyggAGEIKCgoKCAA0U4oaSgoaSgq6ygoKCrrKCgoKusoKCgq7EgoKEgoIAAhTiggAJFoIABxoACQKCAAgUgqaCAAUYAAgCgoKWgtikgs6CqIKsooKCgoKEgoKCggAFFOKCgoKChIKCpoKChIKCgoIAAhTigoSChIKusoKEgoSCrrKChIKCAAIQwoKErKKCqIKEgoSCrrKChIKEgq6ygoSCrKKChILsooKC", "227-230:1;286-293:1;354-361:1")]
[assembly: go.GoPositionMap("time/format_test.go", "format_test.cs", "ABMkAAgSAAgUuIKCgoKClJSWgoKUlriCgoIACRQACAyCgoKCggAKCoIAHkaCgoKCAAwWAB9ElIKCgoIACQoADCCUgoLMkoKCgoK4ggAMHIKCgoKmlJSCAB0eAEa4AYKygoKUAAoMACBCgoKCyJLGtAAMHgAKBIKClpSCgpbegoKClIKogoLMgoKUgoKUgoLoooKUgoK0tAAGEpKygoKUyrSClIKUgpSClIKUgqaCgpSClIKClILogoKCgpSUgoKClIKClJSSloCCpICCABAUAB9EooKCgqQAFBYAJmyCgoKCpPqCgoKCgpSCgriCgoKCgpSCguiCgoKUgpSCgpSCuIKCgpSClIKClIL8ooKClIKCguiCgoKUgoKCAAkUAAsYlIKCgpSCgvqCgoKCgsySgoKCgpSAgqSAgqSAggALCpLOgoKCooKkAAoKAAgSgoKClILGtOySgoKCgoKkAAgMkgAIFoKAggAJDpIABhSCgoCCAA8KABI4koKCgoKClICC7KIADQ6CgoKCqIKClLS0tISCgoKWgoKCyqKClIKUgpSClpSCgoKWlIKClLTagoKUtLTagKS0", "37-51:1;53-58:2;553-569:1;570-570:2;1011-1034:1;1051-1092:1;1053-1057:1.1;1073-1073:1.2")]
[assembly: go.GoPositionMap("time/linkname_test.go", "linkname_test.cs", "AAwcxtbUgoKGooKWgoKWgoI=")]
[assembly: go.GoPositionMap("time/mono_test.go", "mono_test.cs", "ACUagpKCppKCqIKCgoKCgoKEhIKSgoKEgoKCgoKCgoSCgoKCgoKCpoKShJKClIKWkoKUgpaSgpSCloKCgoKClIKClIKUgpSClICSpICSyKKShJKEkoSSgpSSgpSSgpaCgoKmgoKClIKChIKChIKChIKCgqaCgoKEgoKChIKCgoSCgoKWgoKEgoKEgoKmgpKCgpaCgoKWgoKClIKCgpaCgoKCgpSClIKUgpSAkqSAkgAJCAAKHIKChIKSgoKCgg==", "14-18:1;19-23:2;140-150:1;141-145:1.1;163-188:2;164-168:2.1")]
[assembly: go.GoPositionMap("time/sleep_test.go", "sleep_test.cs", "ABoywoKQkoIAChzigpSClKSCyIKCgoKUgoKCgoK+soKCgpKCgoKUqIKmgoKCgrimgoKUgqaCgoKCuKaCgoKCsoKCtKSCgqYADhqErIKSgpSClJKUloCSoqSmooKCgrKCgoKUoqSEgoKEgoLKgoKCgoKCkoKClKaCyoKCggAICoKCgoK4goKC3IKCgoKCgoKUyoKCgoKCgpaC3IKCgoKClKaCgoKClMqCgoKCgoKCgoKmyoKCgoKCgIKkgILIgoKCgoKUgoKUgoKCgpSCuIKClIK4/oKCgqiCkoKCkJKCgoKUgoKUgrSCtILWgoKogoKWloKmgoKUguiGooKCgoCCtoK47oKmuISCgpSCgoKCgpSCgqaCgoKCpqaCgpSCgoKClIKmpuqCgpKClJSCgoKClKIACASCgoKClIKkxoLWloKUpgAIDoKCgoKClIKClAADEOKEgqaWtOqCgugACAzigraEgIK6koKipKbWooKCgILEpKKCgqKkooKCotaSgpSClIKCkoL6goKWgoKCgoCCAAgOwoKWgoKCgoKCgpSWgpKCtICCgta4grTa0oSCgoSCgoSCgoSUtriSxoLGgsqSgsqSxoKCgoLKguqSpoKssoKC+oKEgoKUqIKCgoKCwoKCgoKCtKbGpuikgqqigpaCgoKCgrKCggALCqSqooKWgoKCgoKygoL4tKQADwzSgoKoggACEgAFEISEgoKCgoKCgoKSooKClIKCgpSCpqimqJSCgoKCgpSClIKCqqKCgpaEgoKCgpQAAhKEhIKCgoKCkoKyhIKCgoKUgoKClIKUoqSEgoKCgoKUgpSCggAGEqKCgoKCgoK6gqamgoK4goKCgoKCkpSCgoI=", "27-27:1;59-62:1;79-87:1;95-103:1;114-122:1;127-132:2;161-167:1;168-172:2;174-174:3;185-192:1;208-224:1;213-220:1.1;228-232:1;236-242:1;237-241:1.1;243-249:2;244-248:2.1;253-263:1;267-279:1;283-291:1;284-290:1.1;292-300:2;293-299:2.1;304-317:1;310-313:1.1;356-358:1;359-361:2;372-376:1;379-379:2;382-382:3;421-423:1;424-426:2;425-425:2.1;494-500:1;495-497:1.1;512-517:1;581-586:1;609-617:1;653-655:1;656-658:2;659-665:3;661-664:3.1;801-803:1;810-816:1;823-844:2;862-870:1;887-896:1;942-957:1;995-1057:1;998-1055:1.1;1020-1038:1.1.1;1068-1077:1;1089-1103:1;1094-1094:1.1")]
[assembly: go.GoPositionMap("time/tick_test.go", "tick_test.cs", "ABYeogAFEIKW3IKWgoKCqAAHFJKCgoKUgoKUgoKCgoKCgqaUpoKkgvyCgpaWgqiSgoKokoSCgpSCgoK6pICCyrKCgIK2oqaygoCCtoKi5KKClIKChpKCgqKCoqSCAAYQgpCmgoSEgtKCgoKCgoKCgpTEtIKUtOqCgoKkgsSCxrToooKCgoKUuIKCgoKUuIKCgoKClAALCIKCgoKCgoKCgoKChIKWgoKCgpaCgoSCgoKCuoCSgJKAkoCSgJKA9oKCgoKCgpSCggARJIKCgoKmgoKCgqaigooACxSSgqTG3pKmqIIACximkoKm5pKCpsSCgqbWlJKCgoCCpJSCgIKkgoKAgraogpaChJSCgoDGpoKCgoKCqIKCqIKCgoK6goKChIKCpuiCgoKCpNiogpaSgpKClIKWgoKWgqiClJaCgpaCgoKCgpKitNaUorTWlIKCgoKCgoKCgqYACAoACAqWgoKCkrTWpoKCgqiCgoKAgqSEgoKAgqSCgoKClJSAgqS42IKCpoLcgoKCgoKClJSmgpSCgoKCgoKUlA==", "39-43:1;134-138:1;144-148:1;165-168:1;180-180:2;189-212:3;217-217:4;234-240:1;244-250:1;254-261:1;265-299:1;267-298:1.1;301-301:2;302-302:3;303-303:4;304-304:5;305-305:6;306-306:7;311-321:1;313-316:1.1;317-320:1.2;368-377:1;382-405:2;406-413:3;414-430:4;431-450:5;499-506:6;508-519:7;528-531:8;563-570:9;571-578:10;607-614:11")]
[assembly: go.GoPositionMap("time/time_test.go", "time_test.cs", "ACAygrKQyIKCgoKCgoKCAAcSspSAgoIAFSoACBLKAAsYyrSCgoKGpgACFIKygoKCgoKUgoKCyoKygoKCgoKUgoKCyoKygoKCgoKUgoKCyoKygoKCgoKUgoKCyoKCkpaAgqSAgsiCgoKClJKogIKkgILIgoKClIKAgsiCgoKUgoCCAA0ckoKCqJKCgoKCgoKUlAAIBgAHFqIABhSE8oSCgoKUgqiCgoKCloKogpaAgqjckqiAgqikqIKogoKCgoK6gqiCgqiSgpKCgt6ElJaSgoKUgpSWkoKClIKClJSUlpKClAAJFAAjSJSCgoKCzIKAggAUGAA1igEABxCCgoKCgoKUloKUlIKCgpSCAAoMAA0igoKAgqSCgIIAIgwAF0CCgoKCggAOFgAGEoKCgoKCgsyCgoKCAAwIAAcUuIKCgtymgoKCgriCgoLGAAgSgoKCgrKCgIKigqS0AAgIAAUSgoKCgoKUgoIACQoABRKCgoKClIKCAAkKAAYUgoKEgIKkooKkAAsKggAIGoKEgoKCloKCggALDIIABhaCgoKUtLaCgpS0toKCgpS0ABoKAC18goKCggANCgAXOoKCgoKkypSCgoKWgoKClqaCgoKCzJKEgpKUgoKWAAsQmqKiovaCgpSClIKCgvqUgoLMppSClIIADRQAEi6igoKCAAkKAAYUgoKAgtoABBCCgoCC2gAEEIKCgIIACgq+goKAgtoABxaCgoCC2gAHFoKCgIIACwoADiaCgoCC2gAUMoKCgIIACgoADCCCgoCCAAwKAAYWgoLugoKmgoIADiqCggAGEoKCgoKmgoKmgoKmgoKmgoKCgoKmgoIABhKmgoKCgsqigriigriigriiguiigoK4ooKCuKKCgrjGgoK4ooKCuKKCgriigoK4ooLcooK4lKKC3KKCuJSigriigoK4ooKCuKKCgriigoK4ooKCuKKCgriigoK4ooKCuKKCgriigoK4ooKClKaigoKCuIKCgoKUgoCCpILogoKClIKClIKCgpaCgoKWgoKUgsqCgoCSgpT6koCSAAgKkoCSpICSAAsIgoKAgqSCggADEtKCgoKCgoKygoK0pKiSgoKCgoKCsoKCtJKygqKkAAsGwoKUgoKUgoKUhAAJHIKCgsakhJKCgoKCgoCCyoKCgoKCgIIACgyygpSCgpYAEzKCgsYACASigpKCgqiygoKCgoLMgoKCuoKCgqiCgoKCABxGgoKC2A==", "27-27:1;195-195:1;196-196:2;209-213:1;214-214:2;228-231:1;239-242:1;304-357:1;386-401:2;405-412:3;416-428:4;432-435:5;1074-1076:1;1723-1726:1;1771-1776:1;1789-1794:1;1795-1798:2;1945-1945:1")]
[assembly: go.GoPositionMap("time/tzdata_test.go", "tzdata_test.cs", "AA0cyqKClIKCgoKWgoKClIKCgsyCgoKCgoKCgpSCgqaCxqiilIKUgoKmpIKCgqakpKSkpII=")]
[assembly: go.GoPositionMap("time/zoneinfo_test.go", "zoneinfo_test.cs", "ABEigoIADAjChIKEloKEgILE1IKCgoKCguiCgoKE3IKCgsqigpKCgrQADArSgpSCAA0ogoKClIKClIKCxqSCgpSCAAkIwoKUgoKCloKClIKClIKCloK0AAgGsoKUgoKCloKCgIKkgILE5JSCgoIADwgAHlKCgoKCgpSCgoKWgoKClIIAEAqiAAoqgoIACwqiAAcagoIACwqiAAoggoIACwqiAAoggoI=")]
[assembly: go.GoPositionMap("time/zoneinfo_windows_test.go", "zoneinfo_windows_test.cs", "AA4agpSEgoKUgriigoKipKKCgqIACwSigoKClISkgoKUkoCCpICCuIKClIK0")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("time_test")]
public static partial class time_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface timer {}
    internal partial struct BenchmarkParallelTimerLatency_type {}
    internal partial struct TestAppendInt_tests {}
    internal partial struct TestFirstZone_type {}
    internal partial struct TestFormatFractionalSecondSeparators_tests {}
    internal partial struct TestQuote_tests {}
    internal partial struct TestStd0xParseError_tests {}
    internal partial struct TestTicker_type {}
    internal partial struct TestTimeIsDST_tests {}
    internal partial struct TestTimeWithZoneTransition_tests {}
    internal partial struct TestTzsetName_type {}
    internal partial struct TestTzsetOffset_type {}
    internal partial struct TestTzsetRule_type {}
    internal partial struct TestTzset_type {}
    internal partial struct TestUnmarshalInvalidTimes_tests {}
    internal partial struct TestZoneBounds_realTests {}
    internal partial struct addDateTestsᴛ1 {}
    internal partial struct afterResult {}
    internal partial struct dateTestsᴛ1 {}
    internal partial struct dayOutOfRangeTestsᴛ1 {}
    internal partial struct daysInTestsᴛ1 {}
    internal partial struct defaultLocTestsᴛ1 {}
    internal partial struct durationAbsTestsᴛ1 {}
    internal partial struct durationTestsᴛ1 {}
    internal partial struct durationTruncateTestsᴛ1 {}
    internal partial struct goStringTestsᴛ1 {}
    internal partial struct invalidEncodingTestsᴛ1 {}
    internal partial struct jsonTestsᴛ1 {}
    internal partial struct longFractionalDigitsTestsᴛ1 {}
    internal partial struct mallocTestᴛ1 {}
    internal partial struct monotonicStringTestsᴛ1 {}
    internal partial struct monthOutOfRangeTestsᴛ1 {}
    internal partial struct notEncodableTimesᴛ1 {}
    internal partial struct nsDurationTestsᴛ1 {}
    internal partial struct parseDurationErrorTestsᴛ1 {}
    internal partial struct parseDurationTestsᴛ1 {}
    internal partial struct parsedTime {}
    internal partial struct secDurationTestsᴛ1 {}
    internal partial struct slimTestsᴛ1 {}
    internal partial struct subTestsᴛ1 {}
    internal partial struct tickerTimer {}
    internal partial struct truncateRoundTestsᴛ1 {}
    public partial struct FormatTest {}
    public partial struct ISOWeekTest {}
    public partial struct ParseErrorTest {}
    public partial struct ParseTest {}
    public partial struct ParseTimeZoneTest {}
    public partial struct SecondsTimeZoneOffsetTest {}
    public partial struct TimeFormatTest {}
    public partial struct TimeTest {}
    public partial struct YearDayTest {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸgob() => builtin.initPackage(typeof(encoding.gob_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyscallꓸwindowsꓸregistry() => builtin.initPackage(typeof(@internal.syscall.windows.registry_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(go.math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(go.math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
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
    [GoInit] internal static void initᴛᴛimportꓸtimeꓸtzdata() => builtin.initPackage(typeof(go.time.tzdata_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.time_package));
    }
}
