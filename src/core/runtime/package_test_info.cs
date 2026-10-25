// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.runtime_package;
global using static global::go.runtime_internal_test_package;

// <ImportedTypeAliases>
global using abiꓸArrayType = go.@internal.abi_package.ΔArrayType;
global using abiꓸChanDir = go.@internal.abi_package.ΔChanDir;
global using abiꓸFuncType = go.@internal.abi_package.ΔFuncType;
global using abiꓸInterfaceType = go.@internal.abi_package.ΔInterfaceType;
global using abiꓸKind = go.@internal.abi_package.ΔKind;
global using abiꓸMapType = go.@internal.abi_package.ΔMapType;
global using abiꓸName = go.@internal.abi_package.ΔName;
global using abiꓸStructType = go.@internal.abi_package.ΔStructType;
global using execꓸError = go.os.exec_package.ΔError;
global using flagꓸErrorHandling = go.flag_package.ΔErrorHandling;
global using mapsꓸMap = go.@internal.runtime.maps_package.ΔMap;
global using metricsꓸFloat64Histogram = go.runtime.metrics_package.ΔFloat64Histogram;
global using netꓸAddr = go.net_package.ΔAddr;
global using netꓸError = go.net_package.ΔError;
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
global using strconvꓸError = go.@internal.strconv_package.ΔError;
global using syscallꓸHandle = go.syscall_package.ΔHandle;
global using syscallꓸSignal = go.syscall_package.ΔSignal;
global using syscallꓸSockaddr = go.syscall_package.ΔSockaddr;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
global using tokenꓸFile = go.go.token_package.ΔFile;
global using tokenꓸPos = go.go.token_package.ΔPos;
global using tokenꓸPosition = go.go.token_package.ΔPosition;
global using traceꓸEvent = go.@internal.trace_package.ΔEvent;
global using traceꓸLabel = go.@internal.trace_package.ΔLabel;
global using traceꓸLog = go.@internal.trace_package.ΔLog;
global using traceꓸMetric = go.@internal.trace_package.ΔMetric;
global using traceꓸRange = go.@internal.trace_package.ΔRange;
global using traceꓸRegion = go.@internal.trace_package.ΔRegion;
global using traceꓸStack = go.@internal.trace_package.ΔStack;
global using traceꓸStateTransition = go.@internal.trace_package.ΔStateTransition;
global using traceꓸSync = go.@internal.trace_package.ΔSync;
global using traceꓸTask = go.@internal.trace_package.ΔTask;
global using traceꓸTime = go.@internal.trace_package.ΔTime;
global using typesꓸError = go.go.types_package.ΔError;
global using typesꓸInfo = go.go.types_package.ΔInfo;
global using typesꓸScope = go.go.types_package.ΔScope;
global using typesꓸSignature = go.go.types_package.ΔSignature;
global using typesꓸTerm = go.go.types_package.ΔTerm;
global using typesꓸType = go.go.types_package.ΔType;
global using windowsꓸContext = go.@internal.runtime.syscall.windows_package.Context;
global using windowsꓸMemoryBasicInformation = go.@internal.runtime.syscall.windows_package.MemoryBasicInformation;
// </ImportedTypeAliases>

using go;
using static global::go.runtime_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("696e746572666163657b537472696e67282920737472696e677d", "TestMapInterfaceKey_GrabBag_i1")]
[assembly: GoDynamicTypeLift("7374727563747b6120696e743b206220696e743b206320696e743b206420696e743b206520696e743b206620696e743b206720696e743b206820696e743b206920696e747d", "globstructᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6120696e743b206220696e743b206320696e743b2078205b325d696e747d", "TestTracebackArgs_b")]
[assembly: GoDynamicTypeLift("7374727563747b6c696e654120696e743b206c696e654220696e747d", "compLitᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6d696e20666c6f617436343b206d617820666c6f617436347d", "testsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b206461746120737472696e677d", "stringdataᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b70205b315d2a696e747d", "BenchmarkMallocTypeInfo8_type")]
[assembly: GoDynamicTypeLift("7374727563747b70205b325d2a696e747d", "BenchmarkMallocTypeInfo16_type")]
[assembly: GoDynamicTypeLift("7374727563747b70205b345d2a696e747d", "BenchmarkMallocTypeInfo32_type")]
[assembly: GoDynamicTypeLift("7374727563747b73796e632e4d757465783b2064697220737472696e673b20746172676574206d61705b737472696e675d2a72756e74696d655f746573742e6275696c646578657d", "testprogᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820666c6f617436343b2079205b343339383436353934313530345d2a627974653b207a205b5d737472696e677d", "TestHugeGCInfo_type")]
[assembly: GoDynamicTypeLift("7374727563747b7820666c6f617436343b2079205b343339383436353934313530345d75696e747074723b207a205b5d737472696e677d", "TestHugeGCInfo_typeᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820696e743b2079205b305d696e743b207a205b325d5b305d696e747d", "TestTracebackArgs_x")]
[assembly: GoTypeAlias("CPUStats", "global::go.runtime_package.cpuStats")]
[assembly: GoTypeAlias("Close", "const:ΔClose")]
[assembly: GoTypeAlias("Dlogger", "global::go.runtime_package.dloggerImpl")]
[assembly: GoTypeAlias("Error", "ΔError")]
[assembly: GoTypeAlias("G", "global::go.runtime_package.g")]
[assembly: GoTypeAlias("Lock", "const:ΔLock")]
[assembly: GoTypeAlias("Mutex", "global::go.runtime_package.mutex")]
[assembly: GoTypeAlias("PallocBits", "ΔPallocBits")]
[assembly: GoTypeAlias("PallocData", "ΔPallocData")]
[assembly: GoTypeAlias("Read", "const:ΔRead")]
[assembly: GoTypeAlias("Sudog", "global::go.runtime_package.sudog")]
[assembly: GoTypeAlias("TimeTimer", "global::go.runtime_package.timeTimer")]
[assembly: GoTypeAlias("Unlock", "const:ΔUnlock")]
[assembly: GoTypeAlias("Write", "const:ΔWrite")]
[assembly: GoTypeAlias("XRegPerG", "global::go.runtime_package.xRegPerG")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<BytesKey, Key>(Pointer = true)]
[assembly: GoImplement<EfaceKey, Key>(Pointer = true)]
[assembly: GoImplement<I2, I1>]
[assembly: GoImplement<IfaceImpl, Iface>]
[assembly: GoImplement<IfaceKey, Key>(Pointer = true)]
[assembly: GoImplement<Int32Key, Key>(Pointer = true)]
[assembly: GoImplement<Int64Key, Key>(Pointer = true)]
[assembly: GoImplement<T16, I1>]
[assembly: GoImplement<T32, I1>]
[assembly: GoImplement<T64, I1>]
[assembly: GoImplement<T8, I1>]
[assembly: GoImplement<TL, I1>]
[assembly: GoImplement<TM, I1>]
[assembly: GoImplement<TM, I2>]
[assembly: GoImplement<TS, I1>]
[assembly: GoImplement<Tslice, I1>]
[assembly: GoImplement<Tstr, I1>]
[assembly: GoImplement<Visitor, go.go.ast_package.Visitor>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<canString, TestMapInterfaceKey_GrabBag_i1>]
[assembly: GoImplement<fInter, ifaceHash_i>]
[assembly: GoImplement<go.runtime_test_package.mutex, locker2>(Pointer = true)]
[assembly: GoImplement<myError, error>]
[assembly: GoImplement<panicStructKey, TestMapInterfaceKey_GrabBag_i1>]
[assembly: GoImplement<rwmutexReadWrite, locker2>(Pointer = true)]
[assembly: GoImplement<rwmutexWrite, locker2>(Pointer = true)]
[assembly: GoImplement<rwmutexWriteRead, locker2>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<structKey, TestMapInterfaceKey_GrabBag_i1>]
[assembly: GoImplement<testing_package.B, testing_package.TB>(Pointer = true)]
[assembly: GoImplement<testing_package.T, global::go.runtime_internal_test_package.TestingT>(Pointer = true)]
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: global::go.GoHoistedLiterals]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: global::go.GoPositionMap("runtime/abi_test.go", "abi_test.cs", "ABs2soKokoIACBIAEATGgoKCgoKUAAkUgqbIgoKCgpSUgqaClAADEIKCkoKClKaClIKUgILo", "70-70:1;99-116:2")]
[assembly: global::go.GoPositionMap("runtime/align_test.go", "align_test.cs", "ACEywriCgoKUgoKCgoKUgoKCgoK4goKmgoIACBKCgoKUpoKClIKCgrqSgpKCgriSAAkUooKClIKClIKClIKUgqbcgIKCyJSokqaCpIKClKSCppSAppSkgpSCgtbKgoKCqsKSgoKClA==")]
[assembly: global::go.GoPositionMap("runtime/arena_test.go", "arena_test.cs", "ADlMopTKpoKClIKUgpSCgpSUgoKUlIKClJSCgpSUgoKmlIKCppSCgpSUgoKUlIKCpqaCgoKCgoCCpJSCgoKCgIKklIKCgoKClMqUgILEpIKSgu6CgriUgoKCgoKCgqaCgsq4gqKC7oKCuJSCgoKCgqaCgoKCgtz4gpSmgpSCAAgIxqaClLiCgoKmgoKClIKCgoKClLiCyoKCgriClJSCgqaCuIK4uIKUgpSCtNSClAAHEIKCgoLcgoKCgpSmgpSCgoKCgpSUgqaClIKUgriCgvaClqaSgsqmgoK4goKClIKUgoLcgoKCuIKCpqaCpoKCgqiSgoKCgoKUgqbWgpamkoKmgoKUgpSCgtyCgoKUgpSCgtyCgoKCpqaCgqKCgoCCtqKSgg==", "49-145:1;117-126:1.1;127-135:1.2;136-144:1.3;156-191:1;195-232:1;241-243:1;244-246:2;276-281:1;346-348:1;529-537:1;531-535:1.1")]
[assembly: global::go.GoPositionMap("runtime/bitcursor_test.go", "bitcursor_test.cs", "AAwYgpKkgoKSlIKCgpSC3IKCkpSCgoKUgg==")]
[assembly: global::go.GoPositionMap("runtime/callers_test.go", "callers_test.cs", "ABAcgqaCpoKClIL2ooKCgoKClIK4goKUlIKClJQABBCCgILagpSUgoKCgpSUgriC1sa4koCCpIKCgpSipMamspKCgoKUlIKUxAAICsKUkoKClIKCpgAIBqKUgoKCuLaCgpSCgoKmuJTWooKCgoKmkoKClKKCgoKmopSU5LLUxriSgIKkgoKUgoK01Ma4koCCpIKClIKCtNTqgqaSgIKkgoKCgqiCtqKowoKmkoCCpIKCgoK6goLIoqqygqaioqSioqSCgILIgoKClPaClJSUlJS4ooKCgoKUgpSmooKCgoKUgpSkgKKAooCigKSigoKCgpSClJSkpKSkpKSkpKSkpKSkpKS4gpS4ooKCgoKUgpTWooIAChaCggAIEpSClIKCptiSgoKmooKUgoKCgpSCgpSi", "99-107:1;117-130:1;118-125:1.1;139-143:1;144-148:2;155-160:1;162-168:2;169-174:3;175-180:4;186-191:1;192-196:2;197-211:3;198-203:3.1;204-210:3.2;205-208:3.2.1;221-228:1;242-249:1;265-276:1;291-302:1;344-347:1;348-351:2;352-355:3;435-438:1;467-482:1;483-487:2;504-511:1")]
[assembly: global::go.GoPositionMap("runtime/chan_test.go", "chan_test.cs", "ABQiooKCgpSClIKCkoKUgpKClIKCpqTWpNaCppSCgpSSkoKUgoKmpNamlIKClIKCgoKmgIKkgILIlIKCooKUgoKCuKaCkoKmgoKCypKCpoKCgpSCvKKCgpKCuIKCooKCgpSmgoKCpoKUgoLKlIKClIKUgtrUgoKClIKCgpLWtoKCggAHJgAOAoKClIKCgoKCsgAJCIKUlIL4gsySgoKUgoKCgoKyAAkIgpSUgviCysaCgoKCgpKygoKC5oKCyLSCggAKDKS0pKKCgoKCgoKCggABEOKCkpKClJSSgpSmkoKSgtSCgsaCgsaCgsaCgsiUkoKSgtSCgsaCgsaCgsaCgsiUoqSCgoKUgoKCgpSCgoKCgoIACAKCgoIAEAzm6JKCgKS0tAAEENKCgoKUgtaChIKC+AANDoKCgoKCgoKigoKUlIIACQqCgoKCgpSCyoKClJSClpKCgrKUgpSUyoKygoKClIKCuIKCgoKUgsqKwoKUooKUlKaCgoK4lJSmgoKUggACKAARAoKC7pSUgqaClJSCgoKSyIKCgrimgoKmsgAMCpSmkpSUgoL2poKCgpS0ppSSgpSCgoKCuAASEIKSgpSCgqamkraCgqamgoIACwqCgoKClJSCgoKUlIKCgpSUgoKCgpSUgoKClJSCgoKUyoKCkoIACxCCgoKCgoK0tOyCgoKCgsLCggAOCriCAA4OpoKCgoKigoK0tOyCgoKCgsKC+Pj4AAsQgoKCgoKClILcgoKCkoKClILcooKCkoKCgqKCgoKUgoKCgoKUgoKCuKaCuIKmgqaigoKSgoKCooKCgoKClKaClKKCgoKClIKCpqaCgriCpoKmgqaCpoKmgqaigoKSgoKCgsSUgoKUgoKUAA4OgpTElIKCgsSCAAoMgoLGpoKCuKKCgoKUlIK4goKCgoL6goSSgoLKsoKCgoKCgoKCooIACQqmgoKm1oKCgpKC1gAJFoKCgpSC", "28-31:1;33-36:2;63-66:3;105-108:4;120-124:5;133-137:6;153-157:7;161-168:8;211-217:1;250-259:1;282-291:1;312-335:1;361-366:1;367-372:2;374-402:3;403-431:4;452-468:1;518-524:1;557-566:1;571-580:2;604-611:1;653-665:1;666-674:2;698-705:1;709-711:2;727-765:1;787-793:1;794-800:2;801-807:3;808-830:4;809-815:4.1;816-822:4.2;823-829:4.3;835-842:1;846-858:1;866-885:1;867-877:1.1;893-903:1;911-930:1;935-945:1;951-960:1;970-991:1;1013-1026:1;1027-1040:2;1080-1101:1;1102-1124:2;1146-1152:1;1158-1163:1;1175-1183:1;1196-1204:1")]
[assembly: global::go.GoPositionMap("runtime/chanbarrier_test.go", "chanbarrier_test.cs", "ABQmgPSCipSCogAKDJK4goKmgoKmgoKmgoKCgoKClIKCooKCgoKCgpSUoqSokg==", "30-35:1;37-39:2;67-79:1")]
[assembly: global::go.GoPositionMap("runtime/checkptr_test.go", "checkptr_test.cs", "ABkcpoKmgpSCgqYADiSykoKCgpSCgpSUggANDKaCpoKUgoKmvrKSgoKClIKClJSC", "48-63:1;90-105:1")]
[assembly: global::go.GoPositionMap("runtime/closure_test.go", "closure_test.cs", "AAwWooKAyKKCgoDsooKSgoLKooC2ooK4ooC2ooI=", "13-13:1;20-20:1;29-32:1;37-37:1;47-47:1")]
[assembly: global::go.GoPositionMap("runtime/complex_test.go", "complex_test.cs", "AA4cooKCgoKClKaigoKCgoKUpqKCgoKCgpSmooKCgoKClKaigoKCgoKU")]
[assembly: global::go.GoPositionMap("runtime/coro_test.go", "coro_test.cs", "AA8eggALGIIACAqCgoKUgpQAChaC+oKUgoKUgoKUgoKUgg==", "28-30:1;53-55:1")]
[assembly: global::go.GoPositionMap("runtime/cpuflags_amd64_test.go", "cpuflags_amd64_test.cs", "AA4YgoKCgoI=")]
[assembly: global::go.GoPositionMap("runtime/crash_test.go", "crash_test.cs", "ADBKgoCkgqSC1LqUgoKmgoKCgrgAEx6igqaClIKCpqailIKmopSCppSCgoKUgoKUgIKSgpS2puSigpSUgoKCgpSCpoKUgoKUgoKCpriU1oKAtpSClIKUgqaUgoKCgsqCgoKAgoLGgqaCgpSCgrSk5oKCgoKCAAsIgoiCgpSUlIKCuIKmAAoQlJSCgoLogtaC1oLWguaUlIKCguiCgu6ClIKCpoL4goKCggAMCIKCiIIADQqCgoqCAAsKgoKGggALCoKChoIADAqCgoiCAAoKgoKEggAMCIKCiIIADAiCgoiCAAoIgoKEguiUlIKCgriCgrKSgoKUlKKk5oKCgoL4goKCgviCgoKC+IKmgoL4lKaCgoK6kpSCpoKCpoKCpoKmgqa4goKAgtqygpSCAAkGlIKCgriUlIKCgriUlIKCgoK4lJSCgoKCAAgIgoKCgoL4goKCgoK4goKCgoKUuJSCgviC5oLmgoKAggANCKKUgoKmgoKUgpSClIKUlIKCgoKCpoKmgoKCkoLGpAAIBIKClIKUgoKmgoL2goKUgpSCgqaCguaCgpSClIKCpoKCuIKCgpSCgpSCgu6UpoKCAAgSgoKosoKCgpSCgoKCuIKi2KKUgoKuwoKCgoKCpoKUlMqCgoLKtOaCgpSC3IIACwrGlJSCyoKClIKCgoKCkoK0AAsGpIKAgqSCppSUlKSkpoK8woKCgKa2lJSCgpSCgoK29IKCgoKCgpKAggAICIKCgoKCgpKAggAICIKCgoKCgpKAggAKCpKCpoKCgoKUgIKklIKAgqSmgoCCtqaqooKCgviClIKCgoKUgIKkgoKUgoKCgoKCpoKUlJSkgrjYgIKkgpSClIKCgIIACAyigoKCggAJDqIAHUKCgoL6gqaCguiCgoKCggATCIKUgoKCgoKUpoKCgoKC3IKCgoKC3IKClIKmgpSCgoKUysqCgoKCgtyCgoKCggAJDoLcgoKCgg==", "170-220:1;174-174:1.1;445-454:1;446-452:1.1;508-511:1;512-516:2;517-521:3;522-525:4;526-529:5;530-532:6;544-546:1;787-798:1;928-934:1;1176-1246:1;1179-1186:1.1;1190-1197:1.2;1201-1223:1.3;1227-1234:1.4;1238-1245:1.5;1257-1263:1")]
[assembly: global::go.GoPositionMap("runtime/debuglog_test.go", "debuglog_test.cs", "ACZEgoLogoKCgoLmgoKCgoKAggAICIKCgoKCgoCC+IKCgoKCgoKCuIKClIKUgpaSgoKCgoKygoKCgoKUgoKCyoKUtLamgoKUgoKCqJKCprjooqaClIKCgoKClKaCgpSUpoK0pIKUgoKCgoKC", "98-119:1")]
[assembly: global::go.GoPositionMap("runtime/decoratemappings_test.go", "decoratemappings_test.cs", "AA0c3AALGIKAgqSUgoIACgqCgriCgIKUgoKUtoKClIKU")]
[assembly: global::go.GoPositionMap("runtime/defer_test.go", "defer_test.cs", "ABEewoKCpgAKDsKUgoKmgqaCAAgIsoKCpoK0qMKClIKCpoKmAAoOwpSCgpSCgqiipKKCppKCkoKktgAICsKUgsYAAhYACwKCgoKmgoKCpoKUAAgO4oKCgqaiuIKCxpIADxqCguySgqaCgqiygoKCgoKClIKClIKUgpSClIKUgpSCypKmgriSgoKUgrSmktaigoKClIKmtoKCpgAGFOKCgoKCgqaigoKClJS4gqa0pKKClKLIgoKUgpQAAxDCgoIABhC4tqKCpoKkkpKmoqSU/qLKgoCkgoKCgoKCgpTWgoK4ooKUtIKSpqKClMTmpIKCuKKUgpSiooKUgoK0koKUopKiguSkooKCuKKSgoKUoqKigpSCxIKUopKSkoKCopKigoKigvqSgoKCsrI=", "16-20:1;31-35:1;46-50:1;62-66:1;81-90:1;95-97:1;116-118:1;132-137:1;138-143:2;144-146:3;155-160:1;161-171:2;162-170:2.1;213-235:1;238-240:2;242-244:3;249-251:4;252-254:5;264-271:1;300-310:1;320-322:1;392-403:1;414-416:1;417-432:2;420-420:2.1;424-429:2.2;425-427:2.2.1;444-446:1;447-461:2;448-456:2.1;449-451:2.1.1;453-455:2.1.2;457-459:2.2;462-465:3;477-480:1;481-500:2;482-494:2.1;483-489:2.1.1;484-486:2.1.1.1;490-492:2.1.2;495-495:2.2;496-496:2.3;501-516:3;504-507:3.1;511-511:3.2;515-515:3.3")]
[assembly: global::go.GoPositionMap("runtime/ehooks_test.go", "ehooks_test.cs", "ABcagoKmAB9IgoKmgoKCgoKCpoKCuII=")]
[assembly: global::go.GoPositionMap("runtime/env_test.go", "env_test.cs", "AA8atICClKSCgqaClICCpICCpICCpICCpICCxA==")]
[assembly: global::go.GoPositionMap("runtime/fastlog2_test.go", "fastlog2_test.cs", "AAwahqKUgpSUgoKClJSC")]
[assembly: global::go.GoPositionMap("runtime/float_test.go", "float_test.cs", "AAoWoriCgrg=")]
[assembly: global::go.GoPositionMap("runtime/gc_test.go", "gc_test.cs", "ACg6goKClIKCgtiCupSCgoKC+KKCgoiCgoK0AAsEgoyCgoKCgpSClIKCAA8KggAAFIKCgoKUgoKCgpSCgsqCgoKCgoKCgpSmgpKCABEMpoSSgoKKAAYQgoK4lJKmgpzCgoKmgpSUggAJCIKC3IKCgviCgpSCuIKClKauAAkMlIKUgIK2lKamgoK6pIK4spKCpoKCgoK4gqaUypQAEAyCgpSCgoKCpqaCgoKCgoLmooiCgoKUgpSCooKCgqamguiigpSCgpKCpKTIgqKipKKkgtiUpoKAgqSmgpKClMySgriIsoKCuIKCuJTmgoKCguiCgpSCgpSCtoKCtqSCyqKCgoKCpoKCpgAKBsqCgpiIgoKCgqa4goKCgrKCgoKCgqQACAiipJKCAAsIsqa4goKmgoKCyoLKgoKmgoKCprSUkoKCgoKUgrSkwoKCqqKCkoKUlJKCpoKCogALBIKUmoiCgoKUgriUlIKUkoKmgoKCuIKClIKCgqaUysaClKiShIKmkoKClIKCgpSCgqa4pqKCgoKCkpKmgoKCgoKClKa0gsqCpIKUgoLYpKKCgoKUgvaCgpSClIKCguiCgpSClIKCgriC9qKEioKmpoKCgrim3IKSgpTKppSWkrKCsoKiyKYACxiCtAAJBIKCgoKUgqaCgoKUlII=", "316-324:1;348-356:1;369-378:1;380-382:2;507-515:1;522-535:2;537-540:3;607-612:1;613-616:2;636-648:1;652-679:2;698-715:1;725-728:1;750-759:1;837-840:1;841-850:2;857-860:3")]
[assembly: global::go.GoPositionMap("runtime/gcinfo_test.go", "gcinfo_test.cs", "ADImkoKCgoKCgoKUgoKCgoKCgpSCgoKUgoKClIKCgpSCgoKUgoKClIKCgpSCgoKUgoKCpoKCgoKCgoKCgriCggAHEJSmgoKUptwAChaUAAoWAAwagpQACBAACBAACRQAFR6SkpK06IKCgg==")]
[assembly: global::go.GoPositionMap("runtime/gomaxprocs_windows_test.go", "gomaxprocs_windows_test.cs", "ABAYgoKmgoKUgriCgsqCgoKClII=", "34-42:1")]
[assembly: global::go.GoPositionMap("runtime/goroutineleakprofile_test.go", "goroutineleakprofile_test.cs", "AIUCILiCggACFgADJpIABxCCgpS4uKKCgriigpSmABYuooKCgoIACBKmlLiCgpaSgoKCpoKmlMgAIV4ACBQAGkIAuAHoA4KUpoKCprLSlMqmpoKUgoK4gqa4uIKmpoKGopSCgoKmgriUpqaClLjYgoKCuIKClIKUgtyC", "52-70:1;73-77:2;80-85:3;110-152:4;492-599:5;501-597:5.1", "", "386=Invoke/1/14/1,Invoke/2/14/4,Invoke/3/14/7,Invoke/4/14/10,Invoke/5/14/13,Invoke/6/14/16,Invoke/7/14/19,Invoke/8/14/22,Invoke/9/14/25,Invoke/10/14/28,Invoke/11/14/31,Invoke/12/14/34,Invoke/13/14/37,Invoke/14/14/41;420=Invoke/1/2/1,Invoke/2/2/4;429=Invoke/1/6/1,Invoke/2/6/5,Invoke/3/6/8,Invoke/4/6/11,Invoke/5/6/14,Invoke/6/6/18;456=Invoke/1/62/1,Invoke/2/62/4,Invoke/3/62/10,Invoke/4/62/14,Invoke/5/62/18,Invoke/6/62/22,Invoke/7/62/27,Invoke/8/62/31,Invoke/9/62/34,Invoke/10/62/38,Invoke/11/62/41,Invoke/12/62/44,Invoke/13/62/47,Invoke/14/62/51,Invoke/15/62/54,Invoke/16/62/57,Invoke/17/62/60,Invoke/18/62/64,Invoke/19/62/67,Invoke/20/62/70,Invoke/21/62/73,Invoke/22/62/76,Invoke/23/62/80,Invoke/24/62/85,Invoke/25/62/90,Invoke/26/62/93,Invoke/27/62/96,Invoke/28/62/100,Invoke/29/62/102,Invoke/30/62/104,Invoke/31/62/106,Invoke/32/62/110,Invoke/33/62/116,Invoke/34/62/121,Invoke/35/62/126,Invoke/36/62/131,Invoke/37/62/134,Invoke/38/62/146,Invoke/39/62/149,Invoke/40/62/153,Invoke/41/62/157,Invoke/42/62/162,Invoke/43/62/166,Invoke/44/62/169,Invoke/45/62/174,Invoke/46/62/179,Invoke/47/62/182,Invoke/48/62/187,Invoke/49/62/191,Invoke/50/62/194,Invoke/51/62/197,Invoke/52/62/200,Invoke/53/62/203,Invoke/54/62/208,Invoke/55/62/211,Invoke/56/62/215,Invoke/57/62/219,Invoke/58/62/223,Invoke/59/62/226,Invoke/60/62/229,Invoke/61/62/234,Invoke/62/62/237")]
[assembly: global::go.GoPositionMap("runtime/hash_test.go", "hash_test.cs", "ACAugoKUgoKCgoKCgoLKgoKUgoKCgoKCgoIABCAACwKCgoKCgoKCgoKCgoKCggAKFoKkgqSCpIKkgqSCgpSCgoKmlIKCgoKCpqiSgoKClNiSgpSCgoKCgoKCgoKCgoLKqJKCgoKUgoKClNiSgpSClIKUgoKClKSipqaCgoKCyoKCgoKCgoKmzJKClIKUgoKCgoKCgoKCgoKCgoKUlLqSgpSClIKCgoKCgoKCgqSigoKosoKClIKCgryigpSClIKUgoKCgoKCpKKCgqSigoKUgoIAECKCpIKkgqSCpIKkgu6CpKKkgqSCpILUgu6CpKKkgqSCpILUgu6CpKKkuKSCpILUggAJFKaCpKKkuKSCpILUgqiSgpSClIKUgoKCgoKCgoKCgqSCgoK4lJSCpoKCgqaCgoIACRSClJSCgoKCgoKCgoLekoKUgoKCgoKCgtSigpTKlIKUlIKCgoKCppQACAqSgpSCgoKC1KKCgoKCgoKCgoKCgoKCgoLK2JKCgoKClOaCpoKCgoK4goKCgoKU3KKCuKKUgpSmgKKAooCigKKAAAkEAAsYhqiCgoKCgoKmgqaCpoCCAAgGlIKsgoKCgoKUgoKUgoKUgoKUgoKUgoKUgoKUgoKUgqaCpoCC7KKCgoKClKaigoKCgpSmgoKUgoKCgpSCgoKCgqaCgoI=", "696-719:1;726-775:1")]
[assembly: global::go.GoPositionMap("runtime/heap_test.go", "heap_test.cs", "AA0axIKm")]
[assembly: global::go.GoPositionMap("runtime/hexdump_test.go", "hexdump_test.cs", "AE8kooKCgoKCuIKCppqUiL6KlAADEIKClK6CgoIAAhaCgoKCAAgWopTKgpaSgpSCpqaCgoKCgoKUpvyUgrQ=", "19-26:1;77-82:2;92-97:3")]
[assembly: global::go.GoPositionMap("runtime/histogram_test.go", "histogram_test.cs", "ABAeyqaCgpSmgoKCgpTKgoK4goKCgpKAgsiCgpSCpoKClIKmpoKUgoKCpgAVLIKAgg==", "", "", "90=Inf/1/2/1,Inf/2/2/20")]
[assembly: global::go.GoPositionMap("runtime/iface_test.go", "iface_test.cs", "ABoyoqKioqIADhKioqKiogAOHJKCpoKCgqaCuKKCuKKCuKKCuKKC+IKCgqaCgsqigriigriigriigriigriigriigoK4ooKCuKKCgriigoK4ooKCuKKCgriigoK4ooKCuKKCgriigoK4ooKCuKKCgriigoK4ooKCuKKCgriCgoKClIKmkoKmgriCgoKClIKmkoKmggALCIKYoqKioqKioqKioqKioraykoKCABw8goKmAAoGgoKCgqaCgqaCgqaCgqaCgqaCgriCgoKmgoKmgoK4goKCpoKCpoKCuIKCgqaCgqaCgg==", "63-67:1;99-103:1;104-108:2;262-266:1;282-286:1;297-297:1;298-298:2;299-299:3;300-300:4;301-301:5;302-302:6;303-303:7;304-304:8;305-305:9;306-306:10;307-307:11;308-308:12;309-309:13;310-310:14;314-319:15;356-387:1;357-361:1.1;362-366:1.2;367-371:1.3;372-376:1.4;377-381:1.5;382-386:1.6;388-404:2;389-393:2.1;394-398:2.2;399-403:2.3;405-421:3;406-410:3.1;411-415:3.2;416-420:3.3;422-438:4;423-427:4.1;428-432:4.2;433-437:4.3")]
[assembly: global::go.GoPositionMap("runtime/import_test.go", "import_test.cs", "ACBEgoKmgqaC")]
[assembly: global::go.GoPositionMap("runtime/lfstack_test.go", "lfstack_test.cs", "ABYuwoKCgqaCpoLKgoKmgriCpoKmgoKUgriCgpSCuIKUgriCgoKCgqakgoKCgpSCgqKUgoKCpqaCpoKCgoKCgpSCgqaClII=", "102-112:1")]
[assembly: global::go.GoPositionMap("runtime/list_manual_test.go", "list_manual_test.cs", "ACA+gqaCpoKClIKUgpSClIKCgpSCpoKCgpSCpoKCgpSCpoKCgqaCgqaigoKUgriCgpSClIKUgpSUgoKUgoKUgoKCpoKCpoKClIKUgpSClJSCgpSCgpSCgoKmgoKmgoKUgpSClIKUlIKClIKClIKCgqaCgqaCgpSClIKUgpSCgpSCgoKmgoIACBSCgoKmgqaCgpSClIKUgpSCgoKUgqaCgoKUgqaCgoKUgqaCgoK4ooKClIK4goKUgpSClIKUlIKClIKClIKCgriCgpSClIKUgpSUgoKUgoKUgoKCuIKClIKUgpSClJSCgpSCgpSCgoK4goKUgpSClIKUgoKUgoKC")]
[assembly: global::go.GoPositionMap("runtime/list_test.go", "list_test.cs", "ABUogsqCgpSClIKUgpSCgoKUgqaCgoKUgqaCgoKUgqaCgoK4ooKClIK4goKUgpSClIKUlIKClIKClIKCgriCgpSClIKUgpSUgoKUgoKUgoKCuIKClIKUgpSClJSCgpSCgpSCgoK4goKUgpSClIKUgoKUgoKCuIKClIKCgoKClKaU")]
[assembly: global::go.GoPositionMap("runtime/lockrank_test.go", "lockrank_test.cs", "ABUgkoKCgoKAgqSUgoKUgg==")]
[assembly: global::go.GoPositionMap("runtime/malloc_test.go", "malloc_test.cs", "ACA4oqamgpSCgpSUAAweAAcUAA8ggoKSgoKClIKAgtqEpoKmgIK2goKUlIKUgoKUgriCuILogoKCgpSCgILIggAICIKClIKUgoKCpoKCpoIACxKCgpSCAAgSgrimgoKCgoKmgoLK7qaUAAgIkgAOJoKClJSCyoKUABMG/IKCpoKUgoKCgpSmgoKCgpSmtIKCuJSCgoKCAAoWpJKCgqamuMamlIKUooKCpoK42KaUgpSCkqKCgoKUgqaCuNym1oKCgoKCgpSCyvqCgpSCgoKClIKmkoKCgoLK2IKCgoKCgoLCgoK0pIKClILKxoKCgoKCwoKCgoKCgpSCgrSkuAAOFqaUgriCgpKCpKT+lIKCpoKmyoK4goKCgoKCgpSClIKCgoKClJTYAA0IooKCgrTUgoKCgriigoCCgoKCgpQAEBTGgoKCgtyAgrSClJSmpqaCgpSClILKyoKAgpKCpIKmgqKC7qKCgriigoK4ooKC+KKChviigob4ooKGAAcQooKCuJSCgpKCtILGuKaCgoKAgsimgoKmpqKCgoKUgoKUgoKCpoKCgqaClKaigpKUgraClIKmgoLmgoKCgoKC", "38-43:1;44-58:2;45-57:2.1;59-66:3;60-65:3.1;142-151:1;288-593:1;289-298:1.1;300-307:1.2;309-315:1.3;317-325:1.4;334-347:1.5;336-341:1.5.1;349-369:1.6;360-365:1.6.1;371-398:1.7;385-394:1.7.1;409-424:1.8;426-453:1.9;455-481:1.10;466-471:1.10.1;483-508:1.11;491-504:1.11.1;510-592:1.12;533-542:1.12.1;762-773:1;778-784:1;789-792:1;826-828:1", "", "72=Invoke/1/28/1,Invoke/2/28/1,Invoke/3/28/1,Invoke/4/28/2,Invoke/5/28/2,Invoke/6/28/2,Invoke/7/28/3,Invoke/8/28/3,Invoke/9/28/3,Invoke/10/28/4,Invoke/11/28/4,Invoke/12/28/4,Invoke/13/28/5,Invoke/14/28/5,Invoke/15/28/6,Invoke/16/28/6,Invoke/17/28/7,Invoke/18/28/7,Invoke/19/28/8,Invoke/20/28/8,Invoke/21/28/8,Invoke/22/28/9,Invoke/23/28/10,Invoke/24/28/11,Invoke/25/28/11,Invoke/26/28/12,Invoke/27/28/12,Invoke/28/28/12")]
[assembly: global::go.GoPositionMap("runtime/map_benchmark_test.go", "map_benchmark_test.cs", "ABcouKKCgpSCgoKUgoKCgoKCAAkOpKSCpoKilIKCgoKUgoLKooKClIKCgpSCgoKCgoLKooKClIKCgpSCgoKCgoLIooKCgqaCgoKUgoKCgoKCyqKCgpSCgoK4ooKCgoKCuKKCgoKCgoK4ooKCgoK4ooKCgoK4ooKClIKCgriAooCigKKAooCigKSigoKUgoKCuIKSgoKClIKC2oKSgoKClIKC2oKSgoKClIKC3JSCgoKCgpSCgriCgoKCpoKClKaosoKCgpSCgoKUqLKUgpSCgoKCgoKCgriAooDkgoKCgpSUgoKClLiigoK4ooKCgoK4xoKCgoKCgoK4ooKCgriCkoKCgoKCABAiooKCgoL4goKSgoKCgsqCkoKCgoIAEQ6CgoKCkoKCgoKmkoKEgoKmkoKEgoIACBKilIKmgoKCtqKUoraCgoIABhKigoK4ooKC6IIAGTTcooKSgqbagoKSggAaLqKCgpSmgoKClKaCgoKCgpSmgoKCgoKUpoKCgoKClKamgoKClKaigoKUpoKClKSkpKSkpKSkzrKokoKClKb8gpSmgoKCgpSCuKKCgoKCgoKClIKCggAQCoKCgoKCgoKCgoKmtIKUgoKmgoKClIKCgsqCgoKCgoKCgoKC1qKClIKCgoKClIK4goKCgoKCgoKCgsqigoKCgoKUgpSCgqamgoKCgoKCgoKCgtiygpSCgoKClIK4goKCgoKCgoKCggAFEgAIAoKUgoKClIKCgpS4goKCgoKCgoKCggAJCsKClIKCypSUgoKClIKCgqaUuIKCgoKmgoKCgoKCgoKCgq7igpSCgoKUgoKClLiCgoKCgoKCgoKCruKClIKCgoKUgoKUuIKCgoKCgoKCgoLY0oKUgoKCgpSCuIKCgoKCgtiygpSCgoKClIIACAiCgoLWooKUgoKCgpSCuIKmuIKCgoKCgoKCgoKqwoKUgoKCgpSCuIKmgoLKgoKCgoKCgoKCgqaigoKUgoK4goKCgqaCgoKCpqKCgoIACgiCgoKCgqaigoKUgoKCuIKCgoKC", "220-229:1;234-243:1;248-257:1;331-337:1;338-344:2;387-395:1;419-429:1;421-427:1.1;430-440:2;432-438:2.1;445-473:1;447-454:1.1;455-463:1.2;464-472:1.3;558-568:1;560-566:1.1;571-577:1;573-575:1.1")]
[assembly: global::go.GoPositionMap("runtime/map_test.go", "map_test.cs", "ACM+4pSClIKmgoK4goKUgqaCggAJCoKClIKCgpSClJSCvKKCpoKCgqqigqaCgoKmgsqCgoKUgoCC7IKUgoKCgoKUgoCC+pKCgoKCgviCgriCgpSCgoKClIKUgpSUgoKmgpSCAA8SgoKCgoKCgoKCgtyCgpSCgoKUgpSClJSUgriCgoKCpoKUgpSC6IKCgpSCgpSCpoKUlIIACBCigoKUgpSCgoKClJSCpoKmgriigoKUgoKCgpSCgpKCgoKCsoK0krKCgrSSgoKygpKCsrS2xqSCpoKmgoKClIKCgpSCgoKygoKUgoKCgpSC6oKEgoKUgoKUgu6CgoKCgoKClIKUgpSClIK8ogAKGJIAChaCgoLOooKClKaCpoKmgoKCgIKklJSCuIK6toKClIKUgqaCgsqCgoKUlKbatoKUgpSCpoKCpoKCppSCyqa4goCCpIKAgraCopSCpoKigoKUlILogpiEgoKClJSCgoKClIL6gpiEgoKClJSCgoKClIKCggAFEKKC3IKCgpSClIKClIKUgoKUgpSCgpSCuoKUgoKUgqbCgrSSgr6ygpSCgoKCgILIgoKUgoKCgoCCyIKClIKCgoKAgviCgpSCgoKCgIL4goKUgoKCgoCC/rKClIKCgpSCgILsggASBoQAAiCClIKClIKClIKClIKClIKClIKClIKClIKCAAoQggAIDIKmogAfTIKUooKCgqaikpKUkpSSlJKUkpSSlJKUkpSSlJKUkpSSlJKUkpSSlJKUopSSlKKUkpSilKKUopSilKKUopSilKKUkqailKK4goLWgpSCgoKmgoKmgoKClAAJDsKCgoKmuIKCpoKCgoKClIK4goKCpoKCgoKClILKgoKClJKCgoK4goKmgoLuuIKCgrqSgoLcgriCgoKUgoKmkoKClJSCgoKCgqaCgg==", "327-331:1;332-337:2;340-347:3;593-595:1;601-607:2;679-682:1;686-689:2;693-696:3;700-703:4;721-725:1;932-934:1;935-943:2;936-941:2.1;944-946:3;947-949:4;950-952:5;953-955:6;956-958:7;959-961:8;962-964:9;965-967:10;968-970:11;971-973:12;974-976:13;977-979:14;980-982:15;983-985:16;986-988:17;989-991:18;992-994:19;995-997:20;998-1000:21;1001-1003:22;1004-1006:23;1007-1009:24;1010-1012:25;1013-1015:26;1016-1018:27;1019-1021:28;1022-1024:29;1025-1027:30;1028-1030:31;1032-1034:32;1035-1037:33;1082-1097:1;1099-1115:2;1124-1141:1;1181-1187:1", "", "510=Repeat/1/2/6,Repeat/2/2/7;523=Repeat/1/1/7")]
[assembly: global::go.GoPositionMap("runtime/mcleanup_test.go", "mcleanup_test.cs", "ABoggoKCgqiKgpKClJSCgpSCggAKBoKCgoKoioKSgpSUgoKCgpSCgoKC1oKEAAsGgoKCgqiKgpKUkoKUlIKCgpSCgoKCgpSCgoIADgiCgoKCqAAAEIKCgpKClJSCgoKClIKCgoIADQaCgpiKgoKUkoKClIIACgaCgpiKgoKUkoKCgoKUggAKBoKCgqiKgpKClJSCgoKCgoKUgoKCggAKBoKCgoKoioKSlJKCkpSCgoIADga0goKAgpKmvoqCkoKi9pKMgpSCgoKCgoKilIKCgoLGpIKCgoKCpoCCAA0IwoKCqIqCtAAKBMKCgqiKkoK0AAciABEChIqC", "20-39:1;30-35:1.1;49-70:1;59-64:1.1;81-81:1;88-111:1;98-100:1.1;101-106:1.2;130-156:1;145-150:1.1;166-183:1;176-178:1.1;190-209:1;200-202:1.1;217-240:1;227-232:1.1;252-269:1;262-264:1.1;278-287:1;298-298:2;320-330:1;326-328:1.1;346-350:1;357-359:2;363-367:1;374-377:2;404-404:1", "267=runtime.Cleanup.Stop")]
[assembly: global::go.GoPositionMap("runtime/memmove_test.go", "memmove_test.cs", "ABoqgoKUgoKClIKCgpSClIKCgoKCgqaCgpSUgoIABxCCgpSCgoKUgoKUgoKCgoKCpoKClJSCggALEIKCpoKClKaCgqaCgpSmgoKCgpSClIKCgoKCgoLugoKCgpSCgpSCgoKClJSCggAGEpKCupKClIK6koKCgoKmpoKCgpTaooKmlIKUgoKClJSEkpKSgpSClJSCgoKCgoKClKaCpoKCgoIACBKCgoKCysi4goKCgoKCyoKCgoKCyoKCgoKCgsqCgoKCgsqCgoKCgoLKgoKCgoKCooKCuKKCgu6CgoKCgsqCgoKUgoKUgoKCgoKmgoKUlIKC7oKCgpKCgriCgpKCgtyCgoKCkoKC3IKCgpKCgu6CgoKCggAICoIAECqygoKUgoKClJSUgoKSgpSUooKCgu6igoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgoK4ooKCgoKCuKKCgoKCgriigoKCgoK4ooKCgoKCuKKCgoKCgriigoKCgoK4ooKCgoKCuKKCgoKCgriigoKCgoK4ooKCgoKCuKKCgoKCgriigoKCgoK4ooKCgoKCuKKCgoKCgriigoKCgoK4ooKCgoKCuKKCgoKCgriigoKCgoK4ooKCgoKCuKKCgoKCgriigoKCgoK4ooKCgoKCuKKCgoKCgriigoKCgoK4ooKCgoKCuKKCgoKCggAJDrIABRSCsoKygoKCAAgSopSCgoK4pKKUgoKCuKSilIKCgrikopSCgoK4pKKUgoKCuKSilIKCgrikopSCgoK4pKKUgoKCuKailIKCgrimopSCgoK4pqKUgoKCuKailIKCgrikopSCgoK4pKKUgoKCuKSilIKCgrikopSCgoK45qKIlIKCpuSiiJSCgqbkooiUgoKm5KKIlIKCpuSiiJSCgqbkooiUgoKm5KKIlIKCpg==", "232-269:1;246-255:1.1;276-279:1;292-299:1;303-309:1;313-320:1;324-330:1;334-341:1;350-355:1;357-362:2;368-374:1;412-417:1;421-426:2;434-439:1;446-451:2;457-463:1;510-517:1;1061-1061:1;1062-1062:2;1063-1063:3;1069-1076:4")]
[assembly: global::go.GoPositionMap("runtime/metrics_test.go", "metrics_test.cs", "ACM8goKCgoKClAAIBrSmgoKmgoKYooKUgoKCurKCkoKApKSkpKSkpKSkpKSkpKSkpKaCgIKUpICCtqSkpoKAgpSkgILYAAsW2qQAChSAgpKUxqSkpKSkpKSk2oKClKaCpoKUgrQAJgQACA6CgriCgqaClpaMiIYAACCCgoCCgqSCgsqCppSkpKSkpKSkpKSkpKSkpKSkpKSkpKSCgoK2pKSkpICCxoLKgoKUgpSClICCpICCpIKUgpSAgraClICCpICCpICSpICSpIKSgpSCgoKCpoKSgoKCgpSClICCpIKUgtyClIKUgriipoKmgoKCgsqCyoKCppaCgsqUgoKCgoKCgpSClIKClpKCgrKClIKSpIKCpNjogoKClLimgoKCgpSUgoKCtoKCgraCgoLKlKaC1oKClJyCooKUlIKUgIIAFSqgoqCioKKg7ICigKKAooDsgKKAooCigOyAooCigKKArNSUpoKCopSCgoKCuJjGgoKUuKaCpoLYwrimAA0ckoKClJSUAAUQABAi3JSUgqamgpSClJTKgpSCgoKUlKqi7oKCgpSClIKClLiUgoKClJSUgoCCpICCtoCCpICCtoCCpICCtoCCpICCABEKogAEEvyCpoKUotqC3ILcgvyCpoKClIKSotqCpoKAgqQAChaClLKSxgAUBKKCgoKmgoKAgraCgoKCgpSAgqSmwoKClJSCgpSCgoKUpoK4goKCgrimkuKCkoKCgqSCprikpoKUAAkU7oKUgqKCzqamgoKCgoKCpoKCgoKCgoKmgoKmgoKCgoKClIKCkpSCuJLKlAAKEJSClIKClKqCpoKCgoKUgrKCgpTKgpSSggARJLKCgoKCgoKUgpSCpoKmpoKCgoKmAAgSAAgCgpSCgpKCuLaUgMq2gIK2koKCgpS4koKCgpTKggANJoKClIKUgriCgqaCgoKCxqTilIKUgoKSggADENSAyqSAgraCgqaCgrSypLKUgpSCgqaSgoKCgoKClIKmgoKClKYAEiqUgILEsqSAooAACRLCgpS09JSCgriCgpSCgoKUgoKUgoKUgpSClIK4uIKCgrKCgoKUgqQACQqmgoKCgqaC5oKClIKCgpSUgoK2gpSUgIKkgIIACAiCgpSCgoKUlIKCtoKClJSAgqSAgviUgoKC", "61-66:1;466-468:1;498-516:1;503-503:1.1;504-504:1.2;518-524:2;578-590:1;654-664:1;714-720:1;806-814:1;868-870:1;874-883:2;887-890:3;894-897:4;901-904:5;908-920:6;924-934:7;948-950:8;967-978:1;980-1013:2;991-997:2.1;1015-1122:3;1016-1121:3.1;1017-1035:3.1.1;1023-1026:3.1.1.1;1027-1029:3.1.1.2;1063-1065:3.1.2;1118-1120:3.1.3;1126-1368:4;1159-1213:4.1;1192-1210:4.1.1;1231-1325:4.2;1237-1241:4.2.1;1243-1255:4.2.2;1319-1324:4.2.3;1327-1367:4.3;1335-1339:4.3.1;1370-1428:5;1386-1400:5.1;1492-1505:1;1533-1533:1;1562-1562:1")]
[assembly: global::go.GoPositionMap("runtime/mfinal_test.go", "mfinal_test.cs", "ABUkABMMgoKSgpSmABMIoqKiouyCgriygqiKgoKClIKCAAkUgoKCooKSooKClIKUlIKUgoKm2JKEtoKCgoKClIKClILcgoKCggANGqKUgoKCgpSCpuqSgpSmlIKQkoKCyoKCgpKCgoKCzJKClJSClJCSgoLMkpKSggAKGIK2ooK4goSCgpSCgoKCtA==", "26-31:1;37-37:2;37-37:3;38-38:4;38-38:5;39-39:6;39-39:7;40-40:8;40-40:9;41-41:10;41-41:11;46-50:13;46-46:12;55-68:14;84-99:1;87-96:1.1;112-112:1;117-130:1;134-139:1;178-178:1;209-209:1;219-219:1;220-220:2;248-250:1")]
[assembly: global::go.GoPositionMap("runtime/mgclimit_test.go", "mgclimit_test.cs", "ABIaoqaCgoKCyoKCppaSgpSClILKgoKCgsqClIKUgoLKgpSAggAPHoKCgoK4goK4goKCuIKCgIKkgpSCuIKCgpSClIKUgsqCgoKUgpSClIK4goKAgqSClIK4goKClIKUgIKkgriCgoKUgpSAgqSCuIKClIKUgIKkgriCgoKUgpSAgqSCAAcQgoKUgpSAgqSCuKY=", "18-22:1;26-29:2")]
[assembly: global::go.GoPositionMap("runtime/mgcpacer_test.go", "mgcpacer_test.cs", "ABoggpSCAA8ggpSmggASKIKmgqYAEiiCpoKmABAkgqaCABAkgqaCABAkgqaCABIogoK4AAcQ7oIAESaCuIIAECSCuIIAESaCyqaCABEmgri4lLiUggARJoK4uJS4lIIAESaCpqaUuJSCABQsgoCCpJSmggARJoKAgqSUpoIAESaCpoCCtri4ABAkgoKAgramgIK2uLgAECSClICCtqa4ggAQJIKAgqSmuIIACRaSlIKCgoKClqiCgpSUkpKSgqaCgpSCgoKClIKCppSqACZOgoKCuIKmyoKCgKaCyAAIEoKCuAAIEoKmlJQAGDiCABASgqaClIKkgsaCtoCCpICCpIK4ABg2gqaCpoKmgqaCgqaCgoIABxKSAAISwoKCgpSCvKKCgoKCgpS8ooKCgoKUvKKCvqKCgoKClLyirqKCgrzCkoKClLyigoKClLyirqKuooKCgpKClLiCgoKUgpSUpoKCgoKCgpSCpoKUgpSCgoKmgoKmgoKUgpSmgoKUgpSClIKUgpSCgpSC", "35-45:1;60-73:2;88-101:3;116-124:4;139-147:5;162-170:6;187-212:7;228-237:8;252-261:9;277-290:10;306-327:11;343-364:12;380-399:13;418-431:14;447-460:15;476-495:16;510-534:17;549-567:18;582-597:19;606-755:20;630-650:20.1;881-883:1;892-898:1;905-912:1;919-925:1;932-934:1;941-947:1;953-955:1;961-964:1;970-976:1;982-988:1;994-996:1;1002-1004:1;1010-1018:1", "", "744=HeapMarked/1/2/2,HeapMarked/2/2/3,Triggered/1/1/4,HeapLive/1/1/5,HeapGoal/1/1/6;787=min/1/4/1,Invoke/1/5/1,min/2/4/2,Invoke/2/5/2,min/3/4/3,Invoke/3/5/3,limit/1/1/4,Invoke/4/5/4,quantize/1/1/5,min/4/4/5,Invoke/5/5/5")]
[assembly: global::go.GoPositionMap("runtime/mgcscavenge_test.go", "mgcscavenge_test.cs", "ABUoooKCppSUgpSUlKaCgoKUgoKCgoK4lJKCgIKCgraCABImgpSUlLiCuIKmABUMggAIHISSAAgSgoLK3O7c3ILc7gAHENyC3AAHEIKC7oSS3O4ABxCUgoLclO7sggAIEu6ykoKCgoIAFA6SgpSIgpQAbuoBggASJrKygpSCgIK2gpSi+Ka8gpKClpKCgpSSgoKUgoKClJSCgpSUqoKmgIK2goKCyqaCgoKmuIKCpriCgoKmAAgSgoKCgoKmpgANBsq6ABEesqbKgoKCgpSmkpSClJKUgoKUgpSClJSSlJSYACMcptym3Kbcptym3KaCgu6mgu6mgu6CuILugqbcgqaC7oKmgsqC6KaC3KaCurKigoKCgoKUooKCgqaSgoKCgpSUkoKCgpT4goKCpoK4oobMgoIACA6ClIKmgpaCgoKGooKC", "41-55:1;56-63:2;288-295:1;449-462:1;469-471:1;482-485:2;486-497:3;498-503:4;504-506:5;509-522:6;601-646:1;619-624:1.1;625-639:1.2;640-644:1.3;657-657:2;658-658:3;662-664:4;665-667:5;671-673:6;674-676:7;680-682:8;683-685:9;689-691:10;692-694:11;698-700:12;701-703:13;707-709:14;710-715:15;719-721:16;722-726:17;730-732:18;733-737:19;741-745:20;746-750:21;754-757:22;758-760:23;764-767:24;768-772:25;776-779:26;780-784:27;791-793:28;794-797:29;801-803:30;804-807:31;812-819:32;820-825:33;827-835:34;836-843:35;858-860:1;861-863:2;869-899:3;884-886:3.1")]
[assembly: global::go.GoPositionMap("runtime/minmax_test.go", "minmax_test.cs", "AA0egoKCgsYAChaUgqaCgoCCpICCtoKAgqSAgtqCgoCCpICCtoKAgqSAggAFEgAIAoKCgqaAgqSAgraAgqSAguyAooCigNaSgpSSgIKkgILIgoKCgoKmooKCgsqigoKC", "114-121:1")]
[assembly: global::go.GoPositionMap("runtime/mpagealloc_test.go", "mpagealloc_test.cs", "AA8cxIKCgpSCppSCgpSClIKUggANDKKClAB29gGCAAoWstaCgpSClIKmgpSCgoK4goKCgpSCgsYAFgiCgpQA5QHeA4IADyAADyAAEiYAJEoAGDKCABAigoIAFTryAC9isrKClIKCgpSCpoKUosiCgpSCtIKClIKmgoKCgILagILIgoKCgoKSgoKUpoKmogAPCIKClACxAfAChJIADRwADh6ysoKUgpSClKIADwiCgpQAGUSysoKUgoKAgrbG", "184-219:1;680-697:1;706-748:1;973-984:1;1027-1040:1", "", "183=PageBase/1/4/6,PageBase/2/4/6,MakeAddrRange/1/2/6,PageBase/3/4/7,PageBase/4/4/7,MakeAddrRange/2/2/7;496=PageBase/1/3/8,PageBase/2/3/9,PageBase/3/3/10;512=PageBase/1/3/8,PageBase/2/3/9,PageBase/3/3/10;528=PageBase/1/1/10;547=PageBase/1/2/22,PageBase/2/2/24;584=PageBase/1/1/12;629=PageBase/1/4/10,PageBase/2/4/11,PageBase/3/4/12,PageBase/4/4/13;659=PageBase/1/3/26,PageBase/2/3/34,PageBase/3/3/40;995=PageBase/1/3/6,PageBase/2/3/7,PageBase/3/3/8;1009=PageBase/1/3/6,PageBase/2/3/7,PageBase/3/3/8")]
[assembly: global::go.GoPositionMap("runtime/mpagecache_test.go", "mpagecache_test.cs", "ABAcsoKUgpSCAA4IgoIAgQGUArKSkoKCgpSC7oKClIKCkoKCgpSUgoK4gpSUxKamgqaSgoK4pqaCpqK2pqaUpoKmABAIgoKUADR4ggAQIgAUKgARJILKgoIAEyiysoKUgoKCpoKUog==", "167-178:1;186-206:1;207-237:2;409-423:1", "", "245=Invoke/1/1/1;248=Invoke/1/1/1;352=PageBase/1/4/8,NewPageCache/1/4/8,PageBase/2/4/9,NewPageCache/2/4/9,PageBase/3/4/10,NewPageCache/3/4/10,PageBase/4/4/11,NewPageCache/4/4/11;369=PageBase/1/1/10,NewPageCache/1/1/10;390=PageBase/1/2/8,NewPageCache/1/2/8,PageBase/2/2/9,NewPageCache/2/2/9;416=PageBase/1/2/10,NewPageCache/1/3/10,PageBase/2/2/11,NewPageCache/2/3/11,NewPageCache/3/3/12")]
[assembly: global::go.GoPositionMap("runtime/mpallocbits_test.go", "mpallocbits_test.cs", "AA8g4oKCgoKCgpSUqqKCgpQACwyygpSSgoKUkoKClIKSgoKUkoKCgpSSgoKCgoKCgpSUkoKCgoKCppKCgpS6soK8ooKUgpSCABEIggAkWoIADyAAEiaykoKCgIIABxKigpSCpgASCpKCitzc3NyC3Nzu3NyykqaCgt6SAA8ggoKClJKCggARDpIAOoIBhJLKytzc7rKSgoKCgpSCpoIAEAySABAugtzK3LKSgoKClIKCyoKCgoKSgqaClIKUgoKCgoKCgqaCAA4epoKCgoI=", "44-46:1;47-51:2;52-56:3;58-62:4;63-68:5;69-78:6;79-88:7;90-96:8;203-210:1;293-301:1;328-333:1;437-450:1;498-507:1;512-519:1;557-561:1", "", "283=PackPallocSum/1/1/3;289=PackPallocSum/1/1/3;295=PackPallocSum/1/1/3;301=PackPallocSum/1/1/3;308=PackPallocSum/1/1/3;314=PackPallocSum/1/1/3;320=PackPallocSum/1/1/3;327=PackPallocSum/1/1/3")]
[assembly: global::go.GoPositionMap("runtime/mranges_test.go", "mranges_test.cs", "ABEYsoKCgpSCgoKCgoKmgrqUgqaCuIK4gpSAgqSCgriCtoK6grqCzILMgrqCgoKCgqiSgoKCggAKCIKCgpYAigGiArKSgoKC", "267-273:1", "", "37=Base/1/2/1,Limit/1/2/1,Base/2/2/2,Limit/2/2/2;77=MakeAddrRange/1/1/1;82=MakeAddrRange/1/1/1;87=MakeAddrRange/1/2/1,MakeAddrRange/2/2/2;93=MakeAddrRange/1/2/1,MakeAddrRange/2/2/2;99=MakeAddrRange/1/1/1")]
[assembly: global::go.GoPositionMap("runtime/netpoll_os_test.go", "netpoll_os_test.cs", "ABEegqaigoKCgoKCuII=", "24-27:1")]
[assembly: global::go.GoPositionMap("runtime/norace_test.go", "norace_test.cs", "AA4ikqaCpoKmgqaigoKCgoKCgpSU", "35-46:1")]
[assembly: global::go.GoPositionMap("runtime/panic_test.go", "panic_test.cs", "ABAe0gAVMrKCkoKC", "43-48:1")]
[assembly: global::go.GoPositionMap("runtime/panicnil_test.go", "panicnil_test.cs", "ABYcgoKUgoKUgoLoooKCgpSSgoKCgpSCgoKCpoK4", "15-17:1;18-21:2;22-25:3;34-52:1")]
[assembly: global::go.GoPositionMap("runtime/pinner_test.go", "pinner_test.cs", "AB06goKAkoCmwoK01KKCgqaiAAgEgoKCgoKUgoKUgpSCgviCgoKCkpSCgoKCtLSkgoKCtLT4goKCgoKClIKUgpSAgqSCgpSCuIKSgoKClIKClIKUgoKUgIKkgoKUgpSCgpSCuKKCgoKCgrSkooKCgoKCoqSCgoKCgoKCgoKClICCtoKCgpSC+oKCgpKCgpSClIKCgpSClKaigoKCgqKkgoKSgoKCgoKCgoKmgoKC1oKCsoKikoKSlIKCgpSCgrS0pKaigoKCgoKCoqSigoKSgoKCoqSigoKSgrKCopKCoqSigoKCgoKCgoKipKKCgpKCgqKkooKCgpKCgqKkooKCgoKCgqKkooKCgoKCgoKCgoKCgqKkooKCgpSCgoKClLiigoKClIKCgoKClLiigoKClIKCgoKUuKKCgoKCuKKCgoKCuKKCgoKCgriCgoKCgoLKgoKCgoKCyoKCgoKCgoLKooKCgoKClKaigoKCuKKCgoKCkoKmpqKCgpKC/JKCgoKCgpSCgoI=", "42-46:1;74-76:1;259-262:1;264-266:2;267-271:3;308-311:1;451-458:1;462-469:1;473-481:1;508-512:1;519-523:1")]
[assembly: global::go.GoPositionMap("runtime/print_quoted_test.go", "print_quoted_test.cs", "AA8YggASJJKCgg==", "31-36:1")]
[assembly: global::go.GoPositionMap("runtime/print_test.go", "print_test.cs", "AAwaooKClIKCgsqigoKUgoKCyqKCgpSCgoKCyqKCgpSCgoKC", "18-23:1;31-36:1;44-50:1;58-64:1")]
[assembly: global::go.GoPositionMap("runtime/proc_test.go", "proc_test.cs", "ACI4lILWAAgIgoKUgpSCgpKClJSSgpSUgoKCgqaCpoKmgoKCooKUgqSCpMiCgqaCgoKSgoKClKbWopSUgoKCgpS4uIKCgpKUkoKClJSmgsampIKC5qKUlIKCgpS4uIKCuIKSgpKCgpS4lJSCuJSCgraCpIKSgpKCgpSUyoLGpIKCgpKCgpSUgriCgqaCgoKigubcgoLmgsqCgqaCgoKigoKCggAKCILKgsqCgoKClKaCgriCgpSCgoKSgoKUlKaCpqKCuIKCgoKUgoKCgoK4goKUouSCgpSCgoLogoKCguiCgoKCAAgIgoKCgqbKypSCuIKCgpSCAAgKooKUgpSmpoKCgsSSgrSmgpTsgsiCyIKCgoKCAAEQ4oK0pMKClKaCooKkggAIDoLCgpSClLKClJSCgoKCgoKCosiCgoKCgqaCuKKClIKCgoKClKKkgoKUgrimpNqIsqaigpSCgsqUppSkpLiCpoKmopSUuLiUgoKUoqSCgoLKgqaCpoKmgqaigoKSgoKUlIKUgriigoKCgoKSgoKUpryigoKCgoKCyoKmpoKCAAMUAAoGgpSCgoKCpqaigoKSgoKm1oKCpoIACRSCggAhRIKCgsKClIKUgpSUspSClIKUlIKCgsqCgoIACxiCgpSC3KKmgoKCgoKmgoKCgoKmpoKCgoKUgoKCgpKUgoKCgpKmgoKmgoKCyoK4gtaCgqaCgoKClIKCgoKCgpSCgoKCyoL2goKCgqaCgviCgoKCgpKCuIKUgoKmgqaUgoKCgvqCgpSCgoK+soKUqLKCppSCgpSqgoKkgoKSgoKUuIKUlIKUlILGpIKCpoKCgsqCrsKCuJSCgpKCgpS4goKCgoKkAAgIgIIACQyigpS4gqaCAAkUggALEgAUJoKmgqaUgoKCgoK4goKCAAgMAAkQgqaCppSCgoKCgriCgoIACAjy7gAHEJKCuIKCpoKCgoKmlIKCgqYAAhoACgKCgoKClJSClJSUgoKmgqaCgoKCysqCgoKUgoLIgoKUgriCpJSmgoKCguqCpoKCpoKmgoKUggADTAAiAoKCgoKClIKUlJSClIKmgoKCpsqUlIKCgsiCpoKClILKgoKmyqaCpoKmggALFoKClIK4gtgACQaCgoKC", "47-52:1;53-58:2;77-90:1;99-107:1;136-144:1;187-194:1;216-228:2;218-226:2.1;239-245:1;259-267:1;289-302:1;332-340:1;361-365:1;454-467:1;507-516:1;520-526:2;527-532:3;656-660:1;682-688:1;704-709:1;722-731:1;757-759:1;769-771:1;792-858:1;831-842:1.1;843-853:1.2;863-868:1;881-883:1;959-979:1;1077-1083:1;1109-1111:1;1133-1137:1;1290-1294:1;1302-1309:2;1310-1317:3")]
[assembly: global::go.GoPositionMap("runtime/profbuf_test.go", "profbuf_test.cs", "ACEmgpSSlIKCgpSCpoKCsoKClJSUgpCkgoKClIKCuIKUsoKCgoKCprKCgoKCprKCgoKmsoKCgqbEgoKCgpSCgpSCpsSCgoKClIKCgoKmsoKCgoKUgoKCpsKCgoKCgoKCgoKCgoKCprKCgoKCuLKCgoKCuLKCgoKCyoKCkoKUlIKCggAKCoKCgpKCgoKClIKCAAQSwpSkpLimAAkUgoKmqKSClIKCgoKmgpSCgg==", "22-24:1;25-33:2;34-47:3;36-44:3.1;46-46:3.2;48-57:4;62-69:5;71-77:6;79-84:7;86-91:8;93-107:9;109-122:10;124-135:11;137-152:12;154-161:13;163-170:14;172-179:15;184-189:1;201-213:1")]
[assembly: global::go.GoPositionMap("runtime/rand_test.go", "rand_test.cs", "AA0cgoLEzoKCgsqCgoLKgoKClJKCggAHEIKSgoLexsbEpoKCgqaCgoKmgoKC", "26-30:1;34-38:1;46-52:1;59-63:1")]
[assembly: global::go.GoPositionMap("runtime/runtime-gdb_test.go", "runtime-gdb_test.cs", "ACU60oKUpKSClIKmgrakgrb4tIKClIKCgpSCgoK4gpSClNaCgpSCgoKUgpSCvKKCgoKUgoIAChwACAKCyoKAgraCgpSCgpSUtIIALFSCgoKCpqaCgpiigoKSgoKUyoLWgoKUACMGooKmgoKCgpSUgoKClJSWkoKCgoK4goKUlIKCgoKmytyoqAAoVIKCgoKmlIKCgqaCgIK2goCCtoKCgITIgoKAgraCgIK2goCCtoKAggAPHoCGyIKUgoCCtoCCtoKAggAjPqKClO6mgoKClKaCgoKUgoKCgriSAAgSggATKKKCpoKCgoKmysbYpLgABxCCgoKAggAZKqKCgoKUgqamgoKClIKCgoK4AAwagoKCgqamAAcQgoIAEiKCgoKClKaCgoKUgoKCgrgACxiCgoKCppSCABcqooKCgpSCpqaCgoKUgoKCgrjugoKCgri4goKCgIIAGzKylIKCpoKClKaCgoKUgoKCgsoACRSCgoKCysqCgoKAgg==", "523-526:1", "", "357=GOROOT/1/1/1,Join/1/1/1;368=GOROOT/1/1/1,Join/1/1/1;374=Sprintf/1/2/2,Sprintf/2/2/35,Join/1/1/40;595=GOROOT/1/1/1,Join/1/2/1,Join/2/2/7;729=GOROOT/1/1/1,Join/1/2/1,Join/2/2/11;803=GOROOT/1/1/1,Join/1/2/1,Join/2/2/10;877=GOROOT/1/1/1,Join/1/2/1,Join/2/2/5;961=GOROOT/1/1/1,Join/1/2/1,Join/2/2/8")]
[assembly: global::go.GoPositionMap("runtime/runtime-lldb_test.go", "runtime-lldb_test.cs", "ACEmgoKCgpSUgpSClIKmlIKCgpKClIKCgpKCAGCqAYKClJSUgoKCpoKCgsqCgoKCgqaCgoKmgoKUgoKU")]
[assembly: global::go.GoPositionMap("runtime/runtime-seh_windows_test.go", "runtime-seh_windows_test.cs", "ABYigqYACwSCggAIEoKCgpSSAAgagoKCgoKUlILMkoKCgoKmuJSCkoKCgoKUgoKUrLKokoKClIKmgoKCgoKClIK4tpSUgriCgpSCpqKClJSSgIKkgpSipKKClJSykoKUgpSClMTWooKUlJKAgqSClIKCtA==", "36-38:1;39-39:2;154-160:1;170-182:1;171-177:1.1;192-198:1", "", "57=FuncPCABIInternal/1/3/5,FuncPCABIInternal/2/3/9,FuncPCABIInternal/3/3/10,GetCallerPC/1/1/11")]
[assembly: global::go.GoPositionMap("runtime/runtime_test.go", "runtime_test.cs", "AClAlLjKgqaC1qKCgoLcooKCggAJEqKSgpKCgoKC3KKCgoKCgtyigriigoLGpKKCuKKCgoLYpMKCgoLYpKKCuKKCgqbYkgAOIAAXMKKClJKClIK0rvKCpoKAggAIEIKipIKCuIKCpqbYAAgSgoKCgoIAIQyEipSKlIqWjpaGuIKCgoK4goKCgoKCptaigpSCgoK4goKClIKCgoLKlIKCgoK4goKCgoKCysqCgoKUgoKUgoIADwqCgpKUgoKCgoKUlKaCgoKUkpSSgriC3IKCuIKSgoK4gpKCgoKQgICkpoKCpoKCgoKmgqaCgriCgoKCuJSCgriCAAUQgoKCgqaiggALCAAOHgABEIKCgoKigoKCgoKC+gAQBgAQIAABEIKCgoKCkqqCgoKCgoKClIKClIKCgoKUgtyClIKCgoKCypKCgoLugoKClIK4poKCgoIADwaCAF2sAYKCgoK4kpSYog==", "107-111:1;122-126:1;132-136:1;147-151:1;220-224:1;315-319:1;338-342:1;362-366:1;401-435:1;402-421:1.1;422-434:1.2;425-433:1.2.1;438-441:2;446-449:3;457-457:4;462-465:5;469-472:6;478-481:7;486-489:8;501-507:1;502-506:1.1;549-559:1;593-622:1;638-660:2;670-754:1;671-753:1.1;712-726:1.1.1;733-749:1.1.2;756-762:2;764-764:3;765-774:4")]
[assembly: global::go.GoPositionMap("runtime/rwmutex_test.go", "rwmutex_test.cs", "ABYogoKClIKmgoKCgoKCgoKmgpSUguiigpS4uJSCgqKkgoKCgoKUlIKUpoKCgoKClJSClKaChJKCgoKCgoKUgoKmgriigoKClIKCgoKCgoKCgqL0goqCgoKCgoKCgsqCgoKCgoKCgoKUgoKClKa4gqaCpoKmgg==", "144-155:1;161-178:1")]
[assembly: global::go.GoPositionMap("runtime/sema_test.go", "sema_test.cs", "ABUmwoKCgoIAChaC6KKClIKCotSigpSCgqKkgpKcwoKCsoKCpMb6grKCgpSCopKC3IKUlNaCgoKClJQADBqCgpSmgoKClJSCyoKClJTcgpSCgg==", "69-79:1;83-90:2;108-146:1;147-168:2")]
[assembly: global::go.GoPositionMap("runtime/signal_windows_test.go", "signal_windows_test.cs", "ACQoooKUgpSCgoKCgqaCgoKCuIKCgoK4goKClICCgqa2goKClICCgqbEAAwEooKmgoKCgoKmgoKCgriCgoKCuIKCgpSClJSmgoK05IKCgpSCgpSCgpTawoKmgoKCgriCgoKCgoKCpqaSuICCpJKCuICCyICSxAAICMKClIKUgoKCgoKmgoKCgriCgoKCuIKCgoKClJSmgIK2grKAgpKClMiAgqSAgsQADASCgpSClIKUgoKUgoKUgoKmgpSUgoKmgoKCuIKCgoKCuIKCgg==", "178-181:1;244-252:1;272-283:1")]
[assembly: global::go.GoPositionMap("runtime/sizeof_test.go", "sizeof_test.cs", "ABMgkoKCAAQSgqaCgoKUgoI=")]
[assembly: global::go.GoPositionMap("runtime/slice_test.go", "slice_test.cs", "ABsogoKCgoKykoKCgqaSgoKCppKCgoK6spKCgoKmkoKCgqaSgoKCurKSgoKCppKCgoKmkoKCggAUGIKCgoKCgqaCgoKCpoKCgoKmgoKCgqaCgoKCgqaCgoKCpoKCgoLegoKCgqaCgoKmgoKCpoKCgqaCgoKCpoKCgqaCgoIADRiCgoKCgpSUgoKClJSCgoKUuLKCgoKCgoLKsoKCgsqygoKCgvqCgoKCgoKCAAcUgoKCkoKC3ILugoKCgtyygoKCgoKCgoKUAAcQgoLWgoKCgriCgoKCgoLogoKCkoKCgpSUkoKCgpQADTYADgKClIKCgoLKgoKCgsqCgoKCyoKCgoLKgoKCgt6ClIKCgoKCyoKCgoKCyoKCgoKCyoKCgoKCyoKCgoKCAAYSsoKClOSCgoKUgpQAEiKCgpQABxCmgoKUgpSCzMKCgpS0goKClIKUgoIADBoAFCqigoKCgoKUgpSCzLKCgpS0goKClIKUABIigoKUlKaCgpSClILqgoSCupKCgoKUpqKClLaCgoKUpoKClJSCgsyCgpSCgtykkpKClKaCgoKCgpSClIKUqJKmgoKCgoKUgpSUgpSkgoKCgoKUgpSUgpSokoKClJKCgrimgoKCqJKCgpQ=", "25-48:1;26-32:1.1;33-39:1.2;40-47:1.3;49-72:2;50-56:2.1;57-63:2.2;64-71:2.3;73-95:3;74-80:3.1;81-87:3.2;88-94:3.3;106-112:1;113-119:2;120-126:3;127-133:4;134-157:5;135-141:5.1;142-148:5.2;149-155:5.3;161-166:1;167-172:2;173-178:3;179-184:4;185-205:5;186-191:5.1;192-197:5.2;198-203:5.3;215-221:1;222-228:2;229-235:3;271-278:1;289-294:1;306-312:1;361-368:1;369-376:2;403-451:1;406-413:1.1;415-422:1.2;424-431:1.3;433-440:1.4;442-449:1.5;453-506:2;456-464:2.1;466-474:2.2;476-484:2.3;486-494:2.4;496-504:2.5;558-560:1;627-629:1;682-684:1;744-749:1;765-772:1;786-795:1;791-793:1.1;803-812:1;808-810:1.1;825-831:1")]
[assembly: global::go.GoPositionMap("runtime/softfloat64_test.go", "softfloat64_test.cs", "AA8ekoKCgriAooCigKKApIIAK1iCgoKmgoKCgrqSqJKokqiSqJKokoKmlKiS+qLcgIIACwiCgoKCgoKUgoKCgoKCgvqCgoKCuIKUpKSU1oKCgoK4goKUgpQ=", "16-20:1", "", "42=Copysign/1/1/2,NaN/1/1/5,Inf/1/2/6,Inf/2/2/7")]
[assembly: global::go.GoPositionMap("runtime/stack_test.go", "stack_test.cs", "ABswwryClIKCgoKCgpKCkoKClJSCpoLclIKCgoKCgriCgoK01tKCppaSgqKCkoKikoKmgqKCgoKikpaSgoKCgqKCgoKikpKUgJSCyIKUpsaCgqKuwoKClIKSgoKUgqaqwoKCgpSCkoKCuIKCpoKigoKSgrS2gqKCgpKCtLaCooKCgpKUtJKmgoKCgoKUlIK8wqSilIKCpoKiAAUU4oKCgoCC2qKCkpSCoqSyrLKCgoKAgtqigpKCgpSUgoCypv4ABRAACASCgIKkAAcQoqrSgpKCgrKSgoKykoKU1tbWooKCyJSClKaUgpSi7IKClKiSgoKUgqaCtoKCgpSmhsqCgoKCgpSCgpSCpoKUgsqCgoKCgriCgoKCgrj8goCCtoLWooKCkpKClLiigpSCpqKCgpKClLiCgpSmooKCkoKUuIKClKaAooCigKKAooCigKKAooCigKKAooCigKKAooCigKKAooCigKKAooCigKKAAAgSsoKUkoKmooKCkoKClLikkoKCpoKCkoKCyqKIsoKUgpTagoKClKaCgqaCgqakgKSClJSAksiCgpSCgoKCgpSC+oKClIKC6KrCgoKCgpSCgoKmgoK4ogAMDIKUgKSUgpD4pqKCgoKUgoKCgpKCgpSCppSikqKCgoKUgoKCpqK23IKCgoKUguiCgsqCppKCgoKCgpSklIKmggAJCIKCgpSCgoKCpoKCuIKCgriAkraAkraAkgAIDJKCgriCgoKUgoKClIKCkoKCpqaCusKClJSCgsqClIKmgoLo5IK2lIKUgoK+sq7igpSC", "40-51:1;42-48:1.1;90-95:1;101-106:2;115-120:3;127-136:4;190-197:1;193-196:1.1;201-208:2;204-207:2.1;212-221:3;214-220:3.1;216-218:3.1.1;227-233:1;247-251:1;276-278:1;302-308:1;310-310:2;331-336:1;351-368:1;354-367:1.1;355-365:1.1.1;358-364:1.1.1.1;359-362:1.1.1.1.1;375-380:1;381-383:2;384-391:3;417-417:1;484-488:1;496-500:1;516-519:1;534-537:1;591-595:1;610-614:1;701-717:1;726-729:1;728-728:1.1;730-734:2;733-733:2.1;740-762:1;741-760:1.1;763-776:2;764-774:2.1;887-894:1;909-911:1;915-917:2;918-920:3")]
[assembly: global::go.GoPositionMap("runtime/start_line_amd64_test.go", "start_line_amd64_test.cs", "AAwespSCgoI=")]
[assembly: global::go.GoPositionMap("runtime/start_line_test.go", "start_line_test.cs", "ABAq4qaCqJSm3qaCpoLKlIamgoIACQimlAAgSrKSgoLekoKCgqaCpIKCpg==", "57-59:1;64-66:1;112-117:1")]
[assembly: global::go.GoPositionMap("runtime/string_test.go", "string_test.cs", "ABYmooKSgoL6ooKCgoIACAqigoKCgvqigoKCgsqigoKUkoKCpqaigoKUkoKCpqaikoLcgpKCgoKm6AAMEJSCspKCyoKykoKCgpTKgrKSggALDoKCspKC3IKykoLcgrKSggAKEKKCgoKCgsqiyoKCgoKCpoKCgvqCgoSC+IKCgpKCpoK4goKCkoKUgpSClJSUlJSUpoK4lIKCkoKmgriUgoKSgqaC6IKCgqaAgsiUgoKUgIK4koKUgoCCyIKCgoKCgqaCuIKCgoKCuIK4goKCpqaCgoKm1riCgoKUgoIACQiCAG3kAYKC", "99-103:1;118-126:1;120-124:1.1;127-139:2;129-137:2.1;140-148:3;142-146:3.1;152-161:1;154-159:1.1;162-171:2;164-169:2.1;172-181:3;174-179:3.1;229-233:1;242-264:1;274-278:1;288-292:1;332-338:1;346-352:1", "", "299=Repeat/3/4/1,Repeat/4/4/1")]
[assembly: global::go.GoPositionMap("runtime/symtab_test.go", "symtab_test.cs", "AA8cooKCgpKClJSSxqzC+LKCgoKM3IKCuKSCgPQAEB7IyOy2kgAJEoKCggAIBoKCACBKgILsooKAgraAgsTIlAACEPaCpoKCpgAFMgAWAqaCgsqClMqCgoLcgoKCAAoIgoKClIKSgoKCuJKCgoK4koKCgg==", "18-23:1;24-26:2;161-165:1;261-268:1;269-276:2;277-284:3", "", "59=HasSuffix/1/3/1,Name/1/2/2,HasSuffix/2/3/2,Name/2/2/3,HasSuffix/3/3/3,Entry/1/1/5;60=Entry/1/1/1,Name/1/1/1;85=lineNumber/1/6/4,lineNumber/2/6/4,lineNumber/3/6/7,lineNumber/4/6/8,lineNumber/5/6/11,lineNumber/6/6/12;102=lineNumber/2/4/1,lineNumber/3/4/1,lineNumber/4/4/2;107=lineNumber/2/4/1,lineNumber/3/4/1,lineNumber/4/4/2;112=lineNumber/1/4/1,lineNumber/2/4/2,lineNumber/3/4/3,lineNumber/4/4/4;119=lineNumber/2/3/1,lineNumber/3/3/2;125=lineNumber/2/3/1,lineNumber/3/3/2")]
[assembly: global::go.GoPositionMap("runtime/synctest_test.go", "synctest_test.cs", "ABIagoKCguyihg==")]
[assembly: global::go.GoPositionMap("runtime/syscall_test.go", "syscall_test.cs", "ABIagoKUlIKCpoKAgg==")]
[assembly: global::go.GoPositionMap("runtime/syscall_windows_test.go", "syscall_windows_test.cs", "AChCgoKClKaigoKUAAwGgpiCuIgAFQiEAA8mAAAclJKCgoKClOyCuIIACQiCgoK4ggALCIKCgoKSgpSCgpSClIKClIK4ooLY0oKCkoKi1IKCgJKCuIL2ooKUgpSCgoKUgpSCpoCSAAkGtIKUgoKClIKUgqaAktaUgriCgpKCppKCgoCCAAoMopSGgoKUgpSAtLSktoKCgpSCtAAKDIKCgpSCgpTmpoKCgpSCgoKCpoKClKYACRCClIKClIL8ABBWgrqSqJKokqiSqJKokqiSqJKokqiSqJKokqiSqJIAAhLigtwAESSCgIKkAA8QooKClKSIgoK0pIKCgoKCgoKCgpSmAA0eooCCpJSClLKigoKSsqKClKKCxsQAGASCgoKCgoKUAAAe2oKCgpSCgviCgoL2goKUgoKUgviCgoIACAiCgoKCgoKUgoKUgqaCgviCgpSCgoKUgJLsgoK4ooKCgoKClJSCAB8GooCCtgAAGpSCgoKUgoKCgoKUlIKklIKCmAAFEoKygpSAgsSkgoCCpIKmgoKigoKCgoKCgpSU6pSCgoKUgoKCgoKUlIKkpoKCtAATCKKAgqSCpgAAFpSCgoKUgoKCgoKUlIKklMyCtAAcBKKAgqSCpgAAJJSCgoKUgoKCgoKUlIKklMyCgqaUzIKCtKSCgoK8ooKUgoKCgqgAEgaigoKClIKClJSEAAAUgoKCgpSCgoKmgoKUlIKClAALBsKUgqaApLSClLaqgoKUgoKCgoKCkoKUkoKCgqaCgriigoK4goKUlJKCgqaCgqaCgpSCgpSCtAATBpKAgraUAAASgoKClIKCgoKClJSCgty4goKClMqCgoKCAAgQ4oCCtoKCppSCgoKCgpSUgqSCgpSCgpSCgrSkpoKogpSCloKCgpSmgoKClKaigoKCgpSSgoKmgoKClLiigoKClIKClIKCgoCCgsiCgIKkgpS4ooKCgqKCgqaCgriiguiilIKCgqaCgoKCgqaCgoKCgg==", "143-153:1;179-179:1;196-207:1;208-208:2;217-228:1;229-229:2;242-246:1;247-254:2;260-262:1;567-581:1;572-575:1.1;576-579:1.2;589-592:1;696-703:1;747-750:1;763-766:2;782-825:1;1061-1072:1;1074-1079:2;1202-1207:1;1252-1257:1;1277-1285:1;1301-1306:1", "", "427=Join/1/2/5,Join/2/2/5;972=Join/1/2/3,Join/2/2/3;1063=Float64bits/1/2/2,Float32bits/1/1/3,Float64bits/2/2/4;1137=Float64bits/1/2/2,Float32bits/1/1/3,Float64bits/2/2/4;1149=Float64bits/1/2/2,Float32bits/1/1/3,Float64bits/2/2/4")]
[assembly: global::go.GoPositionMap("runtime/time_test.go", "time_test.cs", "ABomgoLKlJSCgqaSgoKUgoKmgpSCgpSCgqaCggAHEoIADxKigoKClIKClIKCgoKCgoKU9riUkoKCgoKUgoKCgoKClILKgg==", "106-125:1")]
[assembly: global::go.GoPositionMap("runtime/trace2map_test.go", "trace2map_test.cs", "ABAcgriCAAcQgoKClIKmgoKClIKmuIKUgoKCspSCAAkQgoKCgpSUgoKClILGpII=", "56-85:1")]
[assembly: global::go.GoPositionMap("runtime/traceback_system_test.go", "traceback_system_test.cs", "ACE2tIKClJSUqrSCgpSUlKaCpoKmgqaCqKKCqLiigqi4lJKCgqamgsqCpqIAChgACQKCgqYAJEiypIKClIKCkoKCgoKmgriCgriCggAKIAAZHIKCgoKClIKClJTKgoKClKYABxSCpoKCgpS4goSUgqa4griCAAgSgoKCgqa4gu6CgqYAIkaCpqaCgqYAAxLipoKqpKSCgoKCgoKClICCgoKUgqa2gqY=", "33-36:1;48-51:1;169-200:1;228-242:1;246-252:2", "", "310=Contains/1/1/1")]
[assembly: global::go.GoPositionMap("runtime/traceback_test.go", "traceback_test.cs", "ADA4koKSpoKUgpSCgpSUgoKCgoKmgoLKpIKmpIKmpIKmtoIABxKkkqiSpIKkgqjCgoKUuIKU5IKkgoLMkoKC2oKokgACFPKkggAICsqCgqaCgoKmgoKCgpSClIKClJSCgpKClIKClJSUgpSCgoKClIIABRKyppS0tLS0tMaCpIKkpoKUgpSkgqSCABwGgoKUgqaClKYAFj6CAD2EAYIACRSCAAkUggAJFIIAChaC3IKCgoLMkoKUlKjIgpSUqtKClJSosoKClKi6goKUqJKClJSokoKUlKiygpSUqLKClJSosoKUlKiygpSUABgusoKUlKiygpSUqLKClJSosoKUlK7ygpSClJSu1AACEuKClIKUAAIS4oKCgpSClIKUAAIS4oIABRCypoKCgoKigsqCgoK0kgAXLPaSgpKClKSClIKCkpSCgoKCgoKCgoKUgsa0tIK0goKClILGgpKCtrqigoKUqJKmguySpoL2goKUggAYMoKCgoKClIIACAqCgqaCgoIAFgiCggARJJKSgqaCgriCgpSUggAICoKCkpKCpoKCuIKClJSC", "30-59:1;61-65:2;67-71:3;73-77:4;79-84:5;111-114:1;174-225:1;280-287:1;295-295:2;300-305:3;309-309:4;314-314:5;319-326:6;333-333:7;338-338:8;343-343:9;348-348:10;353-353:11;358-358:12;363-363:13;368-368:14;373-373:15;378-378:16;386-389:17;397-400:18;408-411:19;419-422:20;431-434:21;709-721:1;749-752:1;753-762:2;849-849:1;859-859:2;908-928:1;909-918:1.1;935-944:1", "854=runtime_test.testTracebackGenericTyp[...].M", "377=Invoke/1/4/101,Invoke/2/4/112,Invoke/3/4/123,Invoke/4/4/134,Sprintf/1/1/147")]
[assembly: global::go.GoPositionMap("runtime/tracestack_test.go", "tracestack_test.cs", "AA4agoKCyqKClIKCgoKygqKClJSUgriCog==", "15-17:1;29-36:1;31-34:1.1")]
[assembly: global::go.GoPositionMap("runtime/unsafepoint_test.go", "unsafepoint_test.cs", "ABMoogAJCqKCtsrugoKClJaSgoKCgqKCgoKClIKUgoKUgsqCgoKCuIK4lKaUpraClILKgpSC")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("runtime_test")]
public static partial class runtime_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("I")] internal partial class TestAppendGeneric_I {}
    [GoLocalName("T")] internal partial class TestDeferKeepAlive_T {}
    internal partial interface TestMapInterfaceKey_GrabBag_i1 {}
    internal partial interface locker2 {}
    internal partial interface mapBenchmarkElemType<ΔT> {}
    internal partial interface mapBenchmarkKeyType<ΔT> {}
    [GoLocalName("T")] internal partial struct BenchmarkAddCleanupAndStop_T {}
    [GoLocalName("T")] internal partial struct BenchmarkAllocation_T {}
    [GoLocalName("obj")] internal partial struct BenchmarkBulkWriteBarrier_obj {}
    [GoLocalName("Empty")] internal partial struct BenchmarkChanSem_Empty {}
    internal partial struct BenchmarkIssue18740_benchmarks {}
    [GoValueClone("p")] internal partial struct BenchmarkMallocTypeInfo16_type {}
    [GoValueClone("p")] internal partial struct BenchmarkMallocTypeInfo32_type {}
    [GoValueClone("p")] internal partial struct BenchmarkMallocTypeInfo8_type {}
    [GoLocalName("stringarray")] internal partial struct BenchmarkMapStringConversion_stringarray {}
    [GoLocalName("stringstruct")] internal partial struct BenchmarkMapStringConversion_stringstruct {}
    [GoLocalName("RunData")] internal partial struct BenchmarkMemclrRange_RunData {}
    [GoLocalName("T")] [GoValueClone("x")] internal partial struct BenchmarkMemmoveKnownSize1024_T {}
    [GoLocalName("T")] [GoValueClone("x")] internal partial struct BenchmarkMemmoveKnownSize112_T {}
    [GoLocalName("T")] [GoValueClone("x")] internal partial struct BenchmarkMemmoveKnownSize128_T {}
    [GoLocalName("T")] [GoValueClone("x")] internal partial struct BenchmarkMemmoveKnownSize192_T {}
    [GoLocalName("T")] [GoValueClone("x")] internal partial struct BenchmarkMemmoveKnownSize248_T {}
    [GoLocalName("T")] [GoValueClone("x")] internal partial struct BenchmarkMemmoveKnownSize256_T {}
    [GoLocalName("T")] [GoValueClone("x")] internal partial struct BenchmarkMemmoveKnownSize512_T {}
    internal partial struct BenchmarkMutexCapture_state {}
    internal partial struct BenchmarkMutexContention_state {}
    internal partial struct BenchmarkMutexHandoff_state {}
    [GoLocalName("PaddedRWMutex")] [GoValueClone("pad")] internal partial struct BenchmarkRWMutexUncontended_PaddedRWMutex {}
    [GoLocalName("node")] internal partial struct BenchmarkWriteBarrier_node {}
    [GoLocalName("OSVersionInfoEx")] [GoValueClone("CSDVersion")] internal partial struct Test64BitReturnStdCall_OSVersionInfoEx {}
    [GoLocalName("testt")] internal partial struct TestAddrRangesFindSucc_testt {}
    [GoLocalName("key")] internal partial struct TestArrayHash_key {}
    [GoLocalName("mt")] internal partial struct TestChanSendInterface_mt {}
    internal partial struct TestCheckPtr2_testCases {}
    internal partial struct TestCheckPtr_testCases {}
    [GoLocalName("T")] internal partial struct TestCleanupAfterFinalizer_T {}
    [GoLocalName("T")] internal partial struct TestCleanupInteriorPointer_T {}
    [GoLocalName("T")] internal partial struct TestCleanupLost_T {}
    [GoLocalName("T")] internal partial struct TestCleanupMultiple_T {}
    [GoLocalName("T")] internal partial struct TestCleanupPointerEqualsArg_T {}
    [GoLocalName("T")] internal partial struct TestCleanupStopAfterCleanupRuns_T {}
    [GoLocalName("T")] internal partial struct TestCleanupStopMultiple_T {}
    [GoLocalName("T")] internal partial struct TestCleanupStop_T {}
    [GoLocalName("T")] internal partial struct TestCleanupStopinterleavedMultiple_T {}
    [GoLocalName("T")] internal partial struct TestCleanupUnreachableClosure_T {}
    [GoLocalName("T")] internal partial struct TestCleanupUnreachablePointer_T {}
    [GoLocalName("Z")] internal partial struct TestCleanupZeroSizedStruct_Z {}
    [GoLocalName("T")] internal partial struct TestCleanup_T {}
    internal partial struct TestExitHooks_scenarios {}
    internal partial struct TestFinalizerRegisterABI_tests {}
    [GoLocalName("T")] internal partial struct TestFinalizerType_T {}
    internal partial struct TestFinalizerType_type {}
    [GoLocalName("Z")] internal partial struct TestFinalizerZeroSizedStruct_Z {}
    internal partial struct TestFreegc_tests {}
    [GoLocalName("X")] [GoValueClone("buf")] internal partial struct TestGcArraySlice_X {}
    [GoLocalName("T")] internal partial struct TestGcDeepNesting_T {}
    [GoLocalName("T")] [GoValueClone("a")] internal partial struct TestGcMapIndirection_T {}
    [GoLocalName("X")] internal partial struct TestGcRescan_X {}
    [GoLocalName("Y")] internal partial struct TestGcRescan_Y {}
    [GoLocalName("testCase")] internal partial struct TestGoroutineLeakProfile_testCase {}
    [GoValueClone("y")] internal partial struct TestHugeGCInfo_type {}
    [GoValueClone("y")] internal partial struct TestHugeGCInfo_typeᴛ1 {}
    internal partial struct TestLineNumber_type {}
    [GoLocalName("test")] internal partial struct TestMallocBitsPopcntRange_test {}
    [GoLocalName("testCase")] internal partial struct TestMallocBitsPopcntRange_testCase {}
    [GoLocalName("T")] internal partial struct TestMapHugeZero_T {}
    [GoLocalName("GrabBag")] [GoValueClone("a")] internal partial struct TestMapInterfaceKey_GrabBag {}
    [GoLocalName("T")] internal partial struct TestMapLargeKeyNoPointer_T {}
    [GoLocalName("T")] internal partial struct TestMapLargeValNoPointer_T {}
    [GoLocalName("hit")] internal partial struct TestPageAllocAllocAndFree_hit {}
    internal partial struct TestPageAllocAllocAndFree_tests {}
    [GoLocalName("test")] internal partial struct TestPageAllocAllocToCache_test {}
    [GoLocalName("hit")] internal partial struct TestPageAllocAlloc_hit {}
    [GoLocalName("test")] internal partial struct TestPageAllocAlloc_test {}
    [GoLocalName("test")] internal partial struct TestPageAllocFree_test {}
    [GoLocalName("test")] internal partial struct TestPageAllocGrow_test {}
    [GoLocalName("setup")] internal partial struct TestPageAllocScavenge_setup {}
    [GoLocalName("test")] internal partial struct TestPageAllocScavenge_test {}
    [GoLocalName("hit")] internal partial struct TestPageCacheAlloc_hit {}
    internal partial struct TestPageCacheAlloc_tests {}
    [GoLocalName("test")] internal partial struct TestPallocBitsAlloc_test {}
    [GoLocalName("test")] internal partial struct TestPallocBitsFree_test {}
    [GoLocalName("test")] internal partial struct TestPallocBitsSummarize_test {}
    [GoLocalName("test")] internal partial struct TestPallocDataFindScavengeCandidate_test {}
    internal partial struct TestPanicWhilePanicking_tests {}
    internal partial struct TestPanicWithDirectlyPrintableCustomTypes_tests {}
    internal partial struct TestParseByteCount_type {}
    internal partial struct TestPrintQuoted_type {}
    internal partial struct TestReadMetricsConsistency_cpu {}
    internal partial struct TestReadMetricsConsistency_gc {}
    internal partial struct TestReadMetricsConsistency_objects {}
    internal partial struct TestReadMetricsConsistency_totalVirtual {}
    [GoLocalName("Wndclassex")] internal partial struct TestRegisterClass_Wndclassex {}
    [GoLocalName("result")] internal partial struct TestReturnAfterStackGrowInCallback_result {}
    internal partial struct TestRuntimeLockMetricsAndProfile_type {}
    [GoLocalName("testCase")] internal partial struct TestScavengeIndex_testCase {}
    internal partial struct TestSchedPauseMetrics_tests {}
    internal partial struct TestSehLookupFunctionEntry_tests {}
    internal partial struct TestSizeof_type {}
    internal partial struct TestStartLine_testCases {}
    [GoLocalName("Rect")] internal partial struct TestStdCall_Rect {}
    [GoLocalName("key")] internal partial struct TestStructHash_key {}
    [GoValueClone("x")] internal partial struct TestTracebackArgs_b {}
    internal partial struct TestTracebackArgs_tests {}
    [GoValueClone("y", "z")] internal partial struct TestTracebackArgs_x {}
    internal partial struct TestTracebackGeneric_tests {}
    internal partial struct TestTracebackGoroutineLabels_type {}
    internal partial struct TestTracebackSystem_tests {}
    [GoLocalName("T1")] [GoValueClone("z")] internal partial struct TestTrailingZero_T1 {}
    [GoLocalName("T2")] internal partial struct TestTrailingZero_T2 {}
    [GoLocalName("T3")] [GoValueClone("z")] internal partial struct TestTrailingZero_T3 {}
    [GoLocalName("T4")] internal partial struct TestTrailingZero_T4 {}
    [GoLocalName("T5")] internal partial struct TestTrailingZero_T5 {}
    [GoLocalName("T")] internal partial struct TestWeakToStrongMarkTermination_T {}
    internal partial struct TestZeroConvT2x_tests {}
    [GoValueClone("x")] internal partial struct acLink {}
    [GoLocalName("node")] [GoValueClone("children")] internal partial struct applyGCLoad_node {}
    internal partial struct bigBuf {}
    internal partial struct bigStruct {}
    internal partial struct bigType {}
    internal partial struct bigValue {}
    internal partial struct buildexe {}
    internal partial struct canString {}
    internal partial struct cbDLL {}
    internal partial struct cbFunc {}
    internal partial struct chunk {}
    internal partial struct compLitᴛ1 {}
    internal partial struct containsBigStruct {}
    internal partial struct contentionWorker {}
    [GoLocalName("async")] internal partial struct doRequest_async {}
    internal partial struct empty {}
    internal partial struct fInter {}
    internal partial struct fakeTimeFrame {}
    internal partial struct foo {}
    internal partial struct gcCycle {}
    internal partial struct gcCycleResult {}
    internal partial struct gcExecTest {}
    internal partial struct globstructᴛ1 {}
    internal partial struct goroutineState {}
    internal partial struct largePointer {}
    internal partial struct largeScalar {}
    internal partial struct listedVal {}
    internal partial struct listedValManual {}
    internal partial struct listedValNIH {}
    internal partial struct mediumPointerEven {}
    internal partial struct mediumPointerOdd {}
    internal partial struct mediumScalarEven {}
    internal partial struct mediumScalarOdd {}
    internal partial struct mediumType {}
    internal partial struct mutex {}
    internal partial struct myError {}
    internal partial struct nonSSAable {}
    internal partial struct obj {}
    internal partial struct obj12 {}
    internal partial struct objWith<T> {}
    internal partial struct objtype {}
    internal partial struct panicStructKey {}
    internal partial struct point {}
    internal partial struct response {}
    [GoLocalName("ThreadEntry32")] internal partial struct resumeChildThread_ThreadEntry32 {}
    internal partial struct rwmutexReadWrite {}
    internal partial struct rwmutexWrite {}
    internal partial struct rwmutexWriteRead {}
    [GoValueClone("D")] internal partial struct smallPointerMix {}
    internal partial struct smallScalar {}
    internal partial struct smallType {}
    internal partial struct stringdataᴛ1 {}
    internal partial struct struct0 {}
    internal partial struct struct24 {}
    internal partial struct struct32 {}
    internal partial struct struct40 {}
    internal partial struct structKey {}
    internal partial struct structWithMethod {}
    internal partial struct tbFrame {}
    [GoValueClone("i")] internal partial struct testArgsType8a {}
    [GoValueClone("i")] internal partial struct testArgsType8b {}
    [GoValueClone("i")] internal partial struct testArgsType8c {}
    [GoValueClone("i")] internal partial struct testArgsType8d {}
    internal partial struct testCallers_want {}
    [GoLocalName("crashTest")] internal partial struct testCrashHandler_crashTest {}
    internal partial struct testTracebackGenericTyp<P> {}
    internal partial struct testprogᴛ1 {}
    internal partial struct testsᴛ1 {}
    internal partial struct traceback {}
    internal partial struct ttiResult {}
    internal partial struct ttiWrapper {}
    internal partial struct uint8Pair {}
    internal partial struct xtreeNode {}
    public partial class Tintptr {}
    public partial interface I {}
    public partial interface I1 {}
    public partial interface I2 {}
    public partial interface Iface {}
    public partial interface Key {}
    public partial interface Tinter {}
    [GoValueClone("e")] public partial struct BigStruct {}
    public partial struct BytesKey {}
    public partial struct ComplexAlgKey {}
    public partial struct DLL {}
    public partial struct EfaceKey {}
    public partial struct FloatInt {}
    public partial struct HashSet {}
    public partial struct IfaceImpl {}
    public partial struct IfaceKey {}
    public partial struct Int32Key {}
    public partial struct Int64Key {}
    [GoValueClone("x")] public partial struct LargeStruct {}
    public partial struct Matrix {}
    public partial struct MyNode {}
    public partial struct Object1 {}
    public partial struct Object2 {}
    public partial struct Ptr {}
    public partial struct PtrScalar {}
    public partial struct ScalarPtr {}
    public partial struct T16 {}
    public partial struct T32 {}
    public partial struct T64 {}
    public partial struct T8 {}
    public partial struct TL {}
    public partial struct TM {}
    public partial struct TS {}
    public partial struct Tint {}
    public partial struct TintPointer {}
    public partial struct Tslice {}
    public partial struct Tstr {}
    public partial struct Visitor {}
    public partial struct smallPointer {}
    [GoValueClone("y")] public partial struct stkobjT {}
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
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸast() => builtin.initPackage(typeof(global::go.go.ast_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸbuild() => builtin.initPackage(typeof(global::go.go.build_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸimporter() => builtin.initPackage(typeof(global::go.go.importer_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸparser() => builtin.initPackage(typeof(global::go.go.parser_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸprinter() => builtin.initPackage(typeof(global::go.go.printer_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtypes() => builtin.initPackage(typeof(global::go.go.types_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸabi() => builtin.initPackage(typeof(@internal.abi_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸprofile() => builtin.initPackage(typeof(@internal.profile_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸrace() => builtin.initPackage(typeof(@internal.race_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸruntimeꓸgc() => builtin.initPackage(typeof(@internal.runtime.gc_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸruntimeꓸmaps() => builtin.initPackage(typeof(@internal.runtime.maps_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸruntimeꓸsys() => builtin.initPackage(typeof(@internal.runtime.sys_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸruntimeꓸsyscallꓸwindows() => builtin.initPackage(typeof(@internal.runtime.syscall.windows_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸstringslite() => builtin.initPackage(typeof(@internal.stringslite_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsynctest() => builtin.initPackage(typeof(@internal.synctest_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyscallꓸwindows() => builtin.initPackage(typeof(@internal.syscall.windows_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyscallꓸwindowsꓸsysdll() => builtin.initPackage(typeof(@internal.syscall.windows.sysdll_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtrace() => builtin.initPackage(typeof(@internal.trace_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtraceꓸtesttrace() => builtin.initPackage(typeof(@internal.trace.testtrace_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(global::go.math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸcmplx() => builtin.initPackage(typeof(global::go.math.cmplx_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(global::go.math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(global::go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸruntimeꓸdebug() => builtin.initPackage(typeof(global::go.runtime.debug_package));
    [GoInit] internal static void initᴛᴛimportꓸruntimeꓸmetrics() => builtin.initPackage(typeof(global::go.runtime.metrics_package));
    [GoInit] internal static void initᴛᴛimportꓸruntimeꓸpprof() => builtin.initPackage(typeof(global::go.runtime.pprof_package));
    [GoInit] internal static void initᴛᴛimportꓸruntimeꓸtrace() => builtin.initPackage(typeof(global::go.runtime.trace_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(global::go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸsyscall() => builtin.initPackage(typeof(syscall_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(unicode.utf8_package));
    [GoInit] internal static void initᴛᴛimportꓸweak() => builtin.initPackage(typeof(weak_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.runtime_package));
    }
}
