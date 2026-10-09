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
global using traceꓸTask = go.@internal.trace_package.ΔTask;
global using traceꓸTime = go.@internal.trace_package.ΔTime;
global using typesꓸError = go.go.types_package.ΔError;
global using typesꓸInfo = go.go.types_package.ΔInfo;
global using typesꓸScope = go.go.types_package.ΔScope;
global using typesꓸSignature = go.go.types_package.ΔSignature;
global using typesꓸTerm = go.go.types_package.ΔTerm;
global using typesꓸType = go.go.types_package.ΔType;
using testing = go.testing_package;
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
[assembly: GoImplement<canString, TestMapInterfaceKey_GrabBag_i1>]
[assembly: GoImplement<fInter, ifaceHash_i>]
[assembly: GoImplement<go.runtime_test_package.mutex, locker2>(Pointer = true)]
[assembly: GoImplement<myError, error>]
[assembly: GoImplement<panicStructKey, TestMapInterfaceKey_GrabBag_i1>]
[assembly: GoImplement<rwmutexReadWrite, locker2>(Pointer = true)]
[assembly: GoImplement<rwmutexWrite, locker2>(Pointer = true)]
[assembly: GoImplement<rwmutexWriteRead, locker2>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<structKey, TestMapInterfaceKey_GrabBag_i1>]
[assembly: GoImplement<testing_package.B, testing_package.TB>(Pointer = true)]
[assembly: GoImplement<testing_package.T, global::go.runtime_internal_test_package.TestingT>(Pointer = true)]
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<BigStruct, ж<BigStruct>>(Indirect = true)]
[assembly: GoImplicitConv<BytesKey, ж<BytesKey>>(Indirect = true)]
[assembly: GoImplicitConv<EfaceKey, ж<EfaceKey>>(Indirect = true)]
[assembly: GoImplicitConv<HashSet, ж<HashSet>>(Indirect = true)]
[assembly: GoImplicitConv<IfaceKey, ж<IfaceKey>>(Indirect = true)]
[assembly: GoImplicitConv<Int32Key, ж<Int32Key>>(Indirect = true)]
[assembly: GoImplicitConv<Int64Key, ж<Int64Key>>(Indirect = true)]
[assembly: GoImplicitConv<Ptr, ж<Ptr>>(Indirect = true)]
[assembly: GoImplicitConv<PtrScalar, ж<PtrScalar>>(Indirect = true)]
[assembly: GoImplicitConv<ScalarPtr, ж<ScalarPtr>>(Indirect = true)]
[assembly: GoImplicitConv<global::go.runtime_package.g, ж<global::go.runtime_package.g>>(Indirect = true)]
[assembly: GoImplicitConv<objWith<go.unsafe_package.Pointer>, ж<objWith<go.unsafe_package.Pointer>>>(Indirect = true)]
[assembly: GoImplicitConv<objWith<ж<obj>>, ж<objWith<ж<obj>>>>(Indirect = true)]
[assembly: GoImplicitConv<objWith<ж<objWith<ж<obj>>>>, ж<objWith<ж<objWith<ж<obj>>>>>>(Indirect = true)]
[assembly: GoImplicitConv<smallPointer, ж<smallPointer>>(Indirect = true)]
[assembly: GoImplicitConv<smallPointerMix, ж<smallPointerMix>>(Indirect = true)]
[assembly: GoImplicitConv<smallScalar, ж<smallScalar>>(Indirect = true)]
[assembly: GoImplicitConv<ttiResult, ж<ttiResult>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: global::go.GoPositionMap("runtime/abi_test.go", "abi_test.cs", "ABs2soKokoIACBIAEATCqIKCgoKClAAIFIKogoKCgpSUgpaChAADEIKCkoKChJaChIKUgILo", "98-115:1")]
[assembly: global::go.GoPositionMap("runtime/align_test.go", "align_test.cs", "ACEyoqiCgoKUgoKCgoKUgoKCgoKogoKmgoIABhKCgoKUloKClIKCgqqSgpKCgqiSAAkUooKClIKClIKClIKUgpa8gIKCuISokpaCpIKSlKSCppSAppSkgpSCgtbKgoKCqsKSgoKClA==")]
[assembly: global::go.GoPositionMap("runtime/arena_test.go", "arena_test.cs", "ADhM5paCgoSChIKEgoKUhIKClISCgpSEgoKUhIKCpoSCgqaEgoKUhIKClISCgqaWgoKCgoKAgqSUgoKCgoCCpJSCgoKCgpS6hICCxKSCgoLegoKohIKCgoKCgoKmgoK6uIKSgt6CgqiEgoKCgoKmgoKCgoLM+IKClIIACAjGloKEqIKCgpaCgoKEgoKCgoKUqIKqgoKCqIKEhIKCloKogriogoSClIK01OyCgoKCzIKCgoKUloKEgoKCgoKUlIKWgoSClIKogoL2ooaWkoK6loKCqIKCgpSClIKCzIKCgqiCgpamgpaCgoKYkoKCgoKClIKW1oKGlpKCloKClIKUgoLMgoKClIKUgoLMgoKCgpamgoKigoKAgraikoI=", "44-140:1;112-121:1.1;122-130:1.2;131-139:1.3;151-186:1;190-227:1;231-233:1;234-236:2;266-271:1;331-333:1;514-522:1;516-520:1.1")]
[assembly: global::go.GoPositionMap("runtime/bitcursor_test.go", "bitcursor_test.cs", "AAwYgpKUgoKShIKCgpSCzIKCkoSCgoKUgg==")]
[assembly: global::go.GoPositionMap("runtime/callers_test.go", "callers_test.cs", "AA8cgqaCpoKClIL2goKCgoKClIKogoKUhIKClJQABBCCgILagoSEgoKCgpSUgriC1uaokoCCpIKCgpSipOaWspKCgoKUlIKUxAAICsKEkoKClIKCpgAIBqKEgoKCqLaCgpSCgoKmuJTWooKCgoKmkoKClKKCgoKmopSU5LLU5qiSgIKkgoKUgoK01OaokoCCpIKClIKCtNQACAqClpKAgqSCgoKCqIK2oqjCgpaSgIKkgoKCgqqCgsiiqrKCpqKipKKipIKAgsiCgoKU9oKUlJSUlLiigoKCgpSClKaigoKCgpSClKSAooCigKKApKKCgoKClIKUlKSkpKSkpKSkpKSkpKSkpMiClLiigoKCgpSClNbCggAKFoKCAAgSlIKUgoKm", "99-107:1;117-130:1;118-125:1.1;139-143:1;144-148:2;155-160:1;162-168:2;169-174:3;175-180:4;186-191:1;192-196:2;197-211:3;198-203:3.1;204-210:3.2;205-208:3.2.1;221-228:1;242-249:1;265-276:1;291-302:1;344-347:1;348-351:2;352-355:3;435-438:1;467-482:1;483-487:2")]
[assembly: global::go.GoPositionMap("runtime/chan_test.go", "chan_test.cs", "ABQiooKCgpSClIKCkoKUgpKClIKCpqTWpNaClpSCgpSSkoKUgoKmpNaWlIKClIKCgoKmgIKkgIK4lIKCooKUgoKCqKaCkoKmgoKCupKCpoKCgpSCrKKCgpKCuIKCooKCgpSmgoKCpoKUgoK6lIKClIKUgtrUgoKClIKCgpLWtoKCggAHJgAOAoKClIKCgoKCsgAJCIKUlIL4gsySgoKUgoKCgoKyAAkIgpSUgviCysaCgoKCgoKCsoKCguaCgsi0goIACgyktKSigoKCgoKCgoIAARDigoKCkoKUlJKClKaSgpKC1IKCxoKCxoKCxoKCyJSSgpKC1IKCxoKCxoKCxoKCyJSipKKCgpSCgoKClIKCgoKCggAIAoKCggAQDObokoKApLS0AAUQ0oKCgpSC1oKEgoL4AA0OgoKCgoKCgqKCgpSUggAJCoKCgoKClILKgoKEhIKGkoKCspSClJS6grKCgoKUgoKogoKCgpSCyqrCgoSigpSUloKCgriUlKaCgpSCAAEoABECgoLulJSCpoKUlIKCgpLIgoKCuKaCgpayAAwKlJaSlISCgvbGgoKChLSWhJKChIKCgoKoABEQgpKClIKCppaSpoKClpaCggALCoKCgoKUlIKCgpSUgoKClJSCgoKClJSCgoKUlIKCgpTKgoKSggALEIKCgoKCgrS07IKCgoKCwsKCAA4KuIIADg6mgoKCgqKCgrS07IKCgoKCwoL4+PgACxCCgoKCgoKUgtyCgoKSgoKUgtyigoKSgoKCooKCgpSCgoKCgpSCgoK4poK4gqaCpqKCgpKCgoKigoKCgoKUpoKUooKCgoKUgoKmpoKCuIKmgqaCpoKmgqaCpqKCgpKCgoKCxJSCgpSCgpQADg6ClMSUgoKCxIIACgyCgsamgoK4ooKCgpSEgriCgoKCgvqChJKCgsqigoKCgoKCgoKiggAJCqaCgqbWgoKCkoLWAAkWgoKClII=", "28-31:1;33-36:2;63-66:3;105-108:4;120-124:5;133-137:6;153-157:7;161-168:8;211-217:1;250-259:1;282-291:1;313-336:1;363-368:1;369-374:2;376-404:3;405-433:4;454-470:1;520-526:1;559-568:1;573-582:2;606-613:1;655-667:1;668-676:2;700-707:1;711-713:2;729-767:1;789-795:1;796-802:2;803-809:3;810-832:4;811-817:4.1;818-824:4.2;825-831:4.3;837-844:1;848-860:1;868-887:1;869-879:1.1;895-905:1;913-932:1;937-947:1;953-962:1;972-993:1;1015-1028:1;1029-1042:2;1082-1103:1;1104-1126:2;1148-1154:1;1160-1165:1;1177-1185:1;1198-1206:1")]
[assembly: global::go.GoPositionMap("runtime/chanbarrier_test.go", "chanbarrier_test.cs", "ABQmgPSCioSCogAKDJKogoKmgoKmgoKmgoKCgoKClIKCooKCgoKCgpSUoqSokg==", "30-35:1;37-39:2;67-79:1")]
[assembly: global::go.GoPositionMap("runtime/checkptr_test.go", "checkptr_test.cs", "ABkcpoKWgoSCgpYADCKCkpKCgoKUgoKUlIIADQymgpaChIKClq6CkpKCgoKUgoKUlII=", "48-63:1;91-106:1")]
[assembly: global::go.GoPositionMap("runtime/closure_test.go", "closure_test.cs", "AAwWooKAyKKCgoDsooKSgoLKooC2ooK4ooC2ooI=", "13-13:1;20-20:1;29-32:1;37-37:1;47-47:1")]
[assembly: global::go.GoPositionMap("runtime/complex_test.go", "complex_test.cs", "AA4cooKCgoKClKaigoKCgoKUpqKCgoKCgpSmooKCgoKClKaigoKCgoKU")]
[assembly: global::go.GoPositionMap("runtime/coro_test.go", "coro_test.cs", "AA4cggALGIIACAqCgoKUAAoWgvqChIKClIKClIKClII=", "27-29:1;49-51:1")]
[assembly: global::go.GoPositionMap("runtime/crash_test.go", "crash_test.cs", "ACxCgoCkgqSCxMqEgoKWgoKCgqgAFB6igpaChIKClqaihIKWhIKCgpSCgpSAgpKClLam5LKClISCgoKClIKWgpSCgpSCgoKWqITmgoCmhISCgoKCqIKCgoCCgraCloKClIKCtJTmgoKCgoIACwiCiIKUlIKCuILWlISCgoLogtaC1oLWguaUhIKCguiCgu6ClIKCpoL4goKCggAMCIKCiIIADQqCgoqCAAsKgoKGggALCoKChoIADAqCgoiC6pSEgoKCuIKCspKCgpSUoqTmgoKCgviCgoKC+IKCgoL4gqaCgviUloKCgrqSlIKmgoKmgoKmgqaCpqiCgoCC2rKClIIACQaUgoKCuJSEgoKCuJSEgoKCgriUhIKCgoIACAiCgoKCgviCgoKCgqiCgoKCgpS4lIKC+ILmguaCgoCCAA0IooSCgpaCgpSChIKUgpSUgoKCgoKmgpaCgoKSgsak9IKClIKCpoKC9oKClIKCpoKC5oKClIKCpoKCuKKCgoKUgoLulKaCggAIEoKCqLKCgoKUgoKCgqiCotiihIKCnsKCgoKCgpaClITKgoKCurTWgoL8ggAKCsaUyoKChIKCgoKCkoK0AAsGpIKAgqSCppSUlKSkxoK8woKCgKa2lJSCgpSCgoK29IKCgoKCgoKSgIIACAiCgoKCgoKCkoCCAAgIgoKCgoKCgpKAggAKCrKEgpaCgoKClICCpJSCgIKkpoKAgraWqqKCgoL4goSCgoKChICCpIKClJKCgoKCpoKUlJSkgrjogIKkgpSClIKCgIIACAyigoKCggAJDqIAHUKCgoL6goKCguiCgoKCgg==", "159-198:1;163-163:1.1;372-381:1;373-379:1.1;435-438:1;439-443:2;444-448:3;449-452:4;453-456:5;457-459:6;471-473:1;702-713:1;837-843:1")]
[assembly: global::go.GoPositionMap("runtime/debuglog_test.go", "debuglog_test.cs", "ACZEgoLogoKCgoLmgoKCgoKAggAICIKCgoKCgoCC+IKCgoKCgoKCuKKChIKEgoaSgoKCgoKygoKCgoKUgoKCyoKUtLaWgoKEgoKCmJKClrjowpaChIKCgoKClJaCgpSUloK0pIKEgoKCgoKC", "98-119:1")]
[assembly: global::go.GoPositionMap("runtime/defer_test.go", "defer_test.cs", "ABEewoKCpgAKDsKUgoKmgqaCAAgIsoKCpoK0qMKClIKCpoKmAAoOwoSCgpSCgqiipKKClpKCkoKktgAICsKUgtYAAhYACwKCgoKmgoKCpoKUAAgOAAgCgoKCpqK4goLGkgAPGoKC7JKCpoKCqLKCgoKCgoKEgoKUgpSClIKUgpSClIK6kqaCqJKCgpSCtKaS1qKCgoKUgqa2goKWAAYU4oKCgoKCpqKCgoKUlLiCprSkooKUosiCgpSClAADEMKCggAGELi2ooKmgqSSkqaipJQACA6iuoKApIKCgoKCgoKU1oKCuKKClLSCkpaigpTE5qSCgriihIKUoqKClIKCtJKClKKSooLkpKKCgrjCkoKClKKiooKUgsSClKKSkpKCgqKSooKCooLqkoKCgrKy", "16-20:1;31-35:1;46-50:1;62-66:1;81-90:1;95-97:1;116-118:1;132-137:1;138-143:2;144-146:3;155-160:1;161-171:2;162-170:2.1;213-235:1;238-240:2;242-244:3;249-251:4;252-254:5;264-271:1;300-310:1;320-322:1;392-403:1;414-416:1;417-432:2;420-420:2.1;424-429:2.2;425-427:2.2.1;444-446:1;447-461:2;448-456:2.1;449-451:2.1.1;453-455:2.1.2;457-459:2.2;462-465:3;477-480:1;481-500:2;482-494:2.1;483-489:2.1.1;484-486:2.1.1.1;490-492:2.1.2;495-495:2.2;496-496:2.3;501-516:3;504-507:3.1;511-511:3.2;515-515:3.3")]
[assembly: global::go.GoPositionMap("runtime/ehooks_test.go", "ehooks_test.cs", "ABsggoKCuIKClIIAHkiCgpaCgpSCgoKCgoKmgoK4gg==")]
[assembly: global::go.GoPositionMap("runtime/env_test.go", "env_test.cs", "AA8atICClKSCgpaChICCpICCpICCpICCpICCxA==")]
[assembly: global::go.GoPositionMap("runtime/fastlog2_test.go", "fastlog2_test.cs", "AAwahqKEgpSUgoKClISC")]
[assembly: global::go.GoPositionMap("runtime/float_test.go", "float_test.cs", "AAoWosiCgrg=")]
[assembly: global::go.GoPositionMap("runtime/gc_test.go", "gc_test.cs", "ACY2goKClIKCgtiCqoSCgoKC+KKCgoiCgoK0AAsEgoyCgoKCgpSClIKCAA8KggAAFIKCgoKUgoKCgpSCgsqCgoKCgoKCgpSmgpKCABEMpoSSgoKKAAYQgoKohJKWgozCgoKWgpSEggAICMqCgoL4ooKUgriCgpSmrgAKDISClICCtpSmpoKCuqSCuKKSgqaCgoKCqIKmlMqUABAMgoKUgoKCgqamgoKCgoKC5qKIgoKClIKUgqKCgoKmpoLoooKUgoKSgqSkyIKioqSipILIpKaCgIKkpoKSgpTMkoK4iLKCgqiCgriU5oKCgoLogoKUgoKUgraCgrakgtqigoKCgpaCgpYACgbKgoKYiIKCgoKmqIKCgoKygoKCgoKkAAgIoqSSggALCKKWqIKCpoKCgsqCuoKCloKCgqa0hJKCgoKClIK0pMKCgpqigpKClJSSgpaCgqIACwSClJqIgoKClIK4lJSChJKCloKCgqiCgpSCgoKWlLrGgpSYkoSClpKCgpSCgoKEgoKWqKaigoKCgpKSpoKCgoKCgpSmtIK6gqSChIKC2KSigoKClIL2goKUgpSCgoLogoKUgpSCgoK4gvbChIqClpaCgoKolsyCkoKUypaUhpKygrKCoriWAAoYgrQ=", "311-319:1;343-351:1;364-373:1;375-377:2;502-510:1;517-530:2;532-535:3;602-607:1;608-611:2;631-643:1;647-674:2;693-710:1;720-723:1;745-754:1;832-835:1;836-845:2;852-855:3")]
[assembly: global::go.GoPositionMap("runtime/gcinfo_test.go", "gcinfo_test.cs", "ADImkoKCgoKCgoKEgoKCgoKCgoSCgoKUgoKClIKCgpSCgoKUgoKClIKCgpSCgoKUgoKCloKCgoKCgoKCgriCggAHEJSmgoKUptwAChaUAAoWAAwagpQADBAADRAACxQAEh6SkpK02IKCgg==")]
[assembly: global::go.GoPositionMap("runtime/hash_test.go", "hash_test.cs", "AB8sgoKUgoKCgoKCgoLKgoKUgoKCgoKCgoIABCAACwKCgoKCgoKCgoKCgoKCggAKFoKkgqSCpIKkgqSCgoSCgoKmhIKCgoKCpqiSgoKClNiSgpSCgoKCgoKCgoKCgoLKqJKCgoKUgoKClNiSgpSClIKUgoKClKSilpaCgoKCuoKCgoKCgoKmzJKClIKUgoKCgoKCgoKCgoKCgoKUlLqSgpSClIKCgoKCgoKCgqSigoKosoKClIKCgryigpSClIKUgoKCgoKCpKKCgqSigoKUgoKCgoIAECKCpIKkgqSCpIKkgu6CpKKkgqSCpILUgu6CpKKkgqSCpILUgu6CpKKkuKSCpILUggAJFKaCpKKkuKSCpILUgqiSgpSClIKUgoKCgoKCgoKCgqSigoKohJSCloKCgpaCgoIACBSClJSCgoKCgoKCgoLekoKUgoKCgoKCgtSigpTKlIKUhIKCgoKCppQACAqSgpSCgoKC1KKCgoKCgoKCgoKCgoKCgoLK2JKCgoKClOaCgqaCgoKCuIKCgoKClNyigriihIKUpoCigKKAooCigPQADBiGmIKCgoKCgqaCpoKmgIIACAaUgpyCgoKCgpSCgpSCgpSCgpSCgpSCgpSCgpSCgpSCpoKmgILsooKCgoKUpqKCgoKClKaCgpSCgoKClIKCgoKCpoKCgg==", "699-722:1;729-778:1")]
[assembly: global::go.GoPositionMap("runtime/heap_test.go", "heap_test.cs", "AAsaxIKm")]
[assembly: global::go.GoPositionMap("runtime/histogram_test.go", "histogram_test.cs", "ABAeypaCgpSmgoKCgpTKgoKogoKCgpKAgsiCgpSCloKClIKWpoKEgoKCpgAVLIKAgg==", "", "", "85=Inf/1/2/1,Inf/2/2/20")]
[assembly: global::go.GoPositionMap("runtime/iface_test.go", "iface_test.cs", "ABoyoqKioqIADhKioqKiogAOHJKCloKCgpaCuKKCuKKCuKKCuKKC+IKCgqaCgsqigriigriigriigriigriigriigoK4ooKCuKKCgriigoK4ooKCuKKCgriigoK4ooKCuKKCgriigoK4ooKCuKKCgriigoK4ooKCuKKCgriCgoKClIKWkoKmgriCgoKClIKWkoKmggALCIKYoqKioqKioqKioqKioqaykoKCABc8goKWAAoGgoKCgqaCgqaCgqaCgqaCgqaCgriCgoKmgoKmgoK4goKCpoKCpoKCuIKCgqaCgqaCgg==", "63-67:1;99-103:1;104-108:2;262-266:1;282-286:1;297-297:1;298-298:2;299-299:3;300-300:4;301-301:5;302-302:6;303-303:7;304-304:8;305-305:9;306-306:10;307-307:11;308-308:12;309-309:13;310-310:14;314-319:15;356-387:1;357-361:1.1;362-366:1.2;367-371:1.3;372-376:1.4;377-381:1.5;382-386:1.6;388-404:2;389-393:2.1;394-398:2.2;399-403:2.3;405-421:3;406-410:3.1;411-415:3.2;416-420:3.3;422-438:4;423-427:4.1;428-432:4.2;433-437:4.3")]
[assembly: global::go.GoPositionMap("runtime/import_test.go", "import_test.cs", "ACBEgoKmgqaC")]
[assembly: global::go.GoPositionMap("runtime/lfstack_test.go", "lfstack_test.cs", "ABYuwoKCgqaCpoLKgoKWgqiCloKWgoKUgqiCgpSCqIKUgriCgoKCgqakgoKCgpSCgqKUgoKCpqaCpoKCgoKCgpSCgqaClII=", "102-112:1")]
[assembly: global::go.GoPositionMap("runtime/lockrank_test.go", "lockrank_test.cs", "ABUgkoKCgoKAgqSUgoKUgg==")]
[assembly: global::go.GoPositionMap("runtime/malloc_test.go", "malloc_test.cs", "AB82opaWgoSCgpSUAAseAAcUAA4ggoKSgoKClIKAgsqEloKWgIKmgoKUlIKUgoKUgriCqILogoKCgpSCgILIggAICIKClIKUgoKCloKCloIACxKigpSCAAcSgqiWgoKCgoKmgoK63paEpqKCgoK01IKCgoK4ooKAgoKCgoKUAA8UoqiCgoKC3ICCtIKUlKampoKClIKmgoKCpoKigu6igoK4ooKC+KKChviigoYABxCigoK4lIKCkoK0gsa4poKCgoCCyKaCgqamooKCgpSCgoSCgoKmgoKCloKUpqKCkpSCpoKEgpaCgg==", "37-42:1;43-57:2;44-56:2.1;58-65:3;59-64:3.1;141-150:1;375-386:1;391-397:1;402-405:1;439-441:1", "", "67=Invoke/1/28/1,Invoke/2/28/1,Invoke/3/28/1,Invoke/4/28/2,Invoke/5/28/2,Invoke/6/28/2,Invoke/7/28/3,Invoke/8/28/3,Invoke/9/28/3,Invoke/10/28/4,Invoke/11/28/4,Invoke/12/28/4,Invoke/13/28/5,Invoke/14/28/5,Invoke/15/28/6,Invoke/16/28/6,Invoke/17/28/7,Invoke/18/28/7,Invoke/19/28/8,Invoke/20/28/8,Invoke/21/28/8,Invoke/22/28/9,Invoke/23/28/10,Invoke/24/28/11,Invoke/25/28/11,Invoke/26/28/12,Invoke/27/28/12,Invoke/28/28/12")]
[assembly: global::go.GoPositionMap("runtime/map_benchmark_test.go", "map_benchmark_test.cs", "ABcouKKCgpSCgoKUgoKCgoKCAAkOpKSCpoKilIKCgoKUgoLKooKClIKCgpSCgoKCgoLKooKClIKCgpSCgoKCgoLIooKCgqaCgoKUgoKCgoKCyqKCgpSCgoK4ooKCgoKCuKKCgoKCgoK4ooKCgoK4ooKCgoK4ooKClIKCgriAooCigKKAooCigKSigoKUgoKCuIKSgoKClIKC2oKSgoKClIKC2oKSgoKClIKC3JSCgoKCgpSCgqiCgoKCpoKClKaosoKCgpSCgoKUqLKUgpSCgoKCgoKCgriAooDkgoKCgpSUgoKClLiigoK4ooKCgoK4xoKCgoKCgoK4ooKCgriCkoKCgoKCABAiooKCgoL4goKSgoKCgsqCkoKCgoIAEQ6CgoKCkoKCgoKmkoKEgoKmkoKEgoIACBKihIKWgoKCtqKEgpKWgoKCAAYSooKCuKKCguiCABg0zKKCkoKW2oKCkoIAFy6igoKUpoKCgpSmgoKCgoKUpoKCgoKClKaCgoKCgpSmpoKCgpSmooKClKaCgpSkpKSkpKSkpN6yqJKCgpSm/IKUpoKCgoKEgriigoKCgoKCgoSCgoIAEAqCgoKCgoKCgoKCprSChIKCloKCgoSCgoLKgoKCgoKCgoKCgtaigpSCgoKCgoSCuIKCgoKCgoKCgoLKooKCgoKSlIKEgoKWpoKCgoKCgoKCgoLYsoKUgoKCgoSCuIKCgoKCgoKCgoIABRIACAKClIKCgoSCgoKUuIKCgoKCgoKCgoIACQrCgpSCgrqEhIKCgpSCgoKWhLiCgoKCpoKCgoKCgoKCgoKu4oKUgoKChIKCgpS4goKCgoKCgoKCgq7igpSCgoKChIKClLiCgoKCgoKCgoKC2NKClIKCgoKEgriCgoKCgoLYsoKUgoKCgoSCAAgIgoKC1qKClIKCgoKEgriCpriCgoKCgoKCgoKCqsKClIKCgoKEgriCpoKCyoKCgoKCgoKCgoKmooKClIKCuIKCgqSCgoI=", "220-229:1;234-243:1;248-257:1;331-337:1;338-344:2;387-395:1;419-429:1;421-427:1.1;430-440:2;432-438:2.1;445-473:1;447-454:1.1;455-463:1.2;464-472:1.3;559-569:1;561-567:1.1;572-578:1;574-576:1.1")]
[assembly: global::go.GoPositionMap("runtime/map_swiss_test.go", "map_swiss_test.cs", "ABIkuIKCgrqSgoLMgriCgoKUgoKmkoKClJSCgoKCgqaCgg==", "54-60:1")]
[assembly: global::go.GoPositionMap("runtime/map_test.go", "map_test.cs", "AB864oSChIKWgoKogoKEgpaCggAJCoKClIKCgpSClJSCvKKCloKCgqqigpaCgoKmorqCgoKEgoCC7IKEgoKCgoKEgoCC+pKCgoKCgviCgqiCgoSCgoKClIKUgpSUgoKmgpSCAA8SgoKCgoKCgoKCgtyCgpSCgoKUgpSClJSUgriCgoKCpoKUgpSC6IKCgpSCgpSCpoKUlIIACBCigoKUgpSCgoKClJSCpoKmgriigoKUgoKCgpSCgpKCgoKCsoK0krKCgrSSgoKygpKCsrS2xqSCpoKmgoKClIKCgpSCgoKygoKUgoKCgpSC6oKEgoKUgoKUgu6CgoKCgoKChIKUgpSClIK8ogALGJIAChaCgoLOooKClJaCpoKWgoKCgIKklJSCuIK6toKClIKUgpaCgrqCgoKUlKbatoKUgpSCloKCloKCloSCyqa4goCCpIKAgqaCopSCloKigoKUlILogpiEgoKClJSCgoKClIL6gpiEgoKClJSCgoKClIKCggAFEKKC3IKCgpSClIKClIKUgoKUgpSCgpSCuoKEgoKUgpbCgrSSgr6ygoSCgoKCgILIgoKEgoKCgoCCyIKChIKCgoKAgviCgoSCgoKCgIL4goKEgoKCgoCC/rKChIKCgpSCgILIgoKUgpSUgpSUgpSUgpTKggASBoQAAiCClIKClIKClIKClIKClIKClIKClIKClIKCAAoQggAIDIKmogAfTIKUooKCgqaikpKUkpSSlJKUkpSSlJKUkpSSlJKUkpSSlJKUkpSSlJKUopSSlKKUkpSilKKUopSilKKUopSilKKUkpailKIACwiCgpaKkoKiggAMCoKCloqSgqKCyoKC1oKEgoKCloKCloKCgpQACQ7CgoKClqiCgpaCgoKCgoSCqIKCgpaCgoKCgoSCyoKCgpSSgoKCqIKCloKC", "325-329:1;330-335:2;338-345:3;591-593:1;599-605:2;677-680:1;684-687:2;691-694:3;698-701:4;719-723:1;955-957:1;958-966:2;959-964:2.1;967-969:3;970-972:4;973-975:5;976-978:6;979-981:7;982-984:8;985-987:9;988-990:10;991-993:11;994-996:12;997-999:13;1000-1002:14;1003-1005:15;1006-1008:16;1009-1011:17;1012-1014:18;1015-1017:19;1018-1020:20;1021-1023:21;1024-1026:22;1027-1029:23;1030-1032:24;1033-1035:25;1036-1038:26;1039-1041:27;1042-1044:28;1045-1047:29;1048-1050:30;1051-1053:31;1055-1057:32;1058-1060:33;1143-1158:1;1160-1176:2;1185-1202:1", "", "493=Repeat/1/2/6,Repeat/2/2/7;507=Repeat/1/1/7")]
[assembly: global::go.GoPositionMap("runtime/mcleanup_test.go", "mcleanup_test.cs", "ABYagoKCgqiKgpKClJSCgpSCggAKBoKCgoKoioKSgpSUgoKCgpSCgoKC1oKEAAsGgoKCgqiKgpKUkoKUlIKCgpSCgoKCgpSCgoIADgiCgoKCqAAAEIKCgpKClJSCgoKClIKCgoIADQaCgpiKgoKUkoKClIIACgaCgpiKgoKUkoKCgoKUggAKBoKCgqiKgpKClJSCgoKCgoKUgoKCggAKBoKCgoKoioKSlJKCkpSCgoIADga0goKAgpKmvoqCkoKi", "17-36:1;27-32:1.1;46-67:1;56-61:1.1;78-78:1;85-108:1;95-97:1.1;98-103:1.2;127-153:1;142-147:1.1;163-180:1;173-175:1.1;187-206:1;197-199:1.1;214-237:1;224-229:1.1;249-266:1;259-261:1.1;275-284:1;295-295:2", "264=runtime.Cleanup.Stop")]
[assembly: global::go.GoPositionMap("runtime/memmove_test.go", "memmove_test.cs", "ABgmgoKUgoKClIKCgpSClIKSkoKCgqaCgpSUgoIABxCCgpSCgoKUgoKUgpKSgoKCpoKClJSCggALEIKCloKClKaCgpaCgpSmgoKCgoSChIKSkoKCgoLugoKCgoSCkqSCgoKClJSCggAGEpKCupKClIK6koKCgoKmpoKCgpTaooKWhIKCloKCgpSUhJKSkoKUgpSEgoKCgoKCgpSWgpaCgoKCAAgSgoKCgsrIuIKCgoKCyoKCgoLKgoKCgoLKgoKCgsqCgoKCgsqCgoKCgoKigoKoooKC7oKCgoLKgoKClIKClIKSgoKCpoKClJSCgu6CgoKSgoK4goKSgoLcgoKCgpKCgsyCgoKSgoLugoKCggAICoIAEyqygoKEgoKClJSEgoKSgpSUooKCgu6igoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgriigoKCgoK4ooKCgoKCuKKCgoKCgriigoKCgoK4ooKCgoKCuKKCgoKCgriigoKCgoK4ooKCgoKCuKKCgoKCgriigoKCgoK4ooKCgoKCuKKCgoKCgriigoKCgoK4ooKCgoKCuKKCgoKCgriigoKCgoK4ooKCgoKCuKKCgoKCgriigoKCgoIACQ6yAAQUgrKCsoKCggAIEqKEgoKCqKSihIKCgqikooSCgoKopKKEgoKCqKSihIKCgqikooSCgoKopKKEgoKCqKSihIKCgqimooSCgoKopqKEgoKCqKaihIKCgqimooSCgoKopKKEgoKCqKSihIKCgqikooSCgoKopKKEgoKCqA==", "232-269:1;246-255:1.1;276-279:1;292-298:1;302-307:1;311-317:1;321-326:1;330-336:1;345-350:1;352-357:2;363-368:1;406-411:1;415-420:2;428-433:1;440-445:2;451-456:1;503-510:1;902-902:1;903-903:2;904-904:3;910-917:4")]
[assembly: global::go.GoPositionMap("runtime/metrics_test.go", "metrics_test.cs", "ACQ+goKCgoKClAAIBtSWgoKWgoKIooKEgoKCqrKCkoKApKSkpKSkpKSkpKSkpKSkpIaigIKUpICCtqSkhqKAgpSkgILYAAIWqqQAAhSAABQCkpTGpKSkpKSkpKTagoKUloKWgpSCtAAmBP6CgqiCgpaChpaMiIYAACCCgoCCgqSCgrqCppSkpKSkpKSkpKSkpKSkpKSkpKSkpKSCgoK2pKSkpICCxoLagoKUgpSClICCpICCpIKUgpSAgraClICCpICCpICSpICSpIKSgpSCgoKCpoKSgoKCgpSClICCpIKUgtyClIKUgriiloKWgoKCgsqCuoKClpaCgsqUgoKCgoKCgpSClIKChpKCgrKClIKCpNjYgoKClKiWgoKCgoSUgoKCtoKCgraCgoLahKaC1oKChJyCooKEhIKEgIIAFSqgoqCioKKg7ICigKKAooDsgKKAooCigOyAooCigKKArNSEloKCooSCgoKCuIi2goKUqJaCloLYsriWAAwckoKClJSEAAEQAA8izISUgpaWgoSClJS6gpSCgoKUlKqi3oKCgoSChIKClKiEgoKChISEgoCCpICCpoCCpICCtoCCpICCpoCCpICCABEKwgAEEvyCloKEotqC3ILcgvyCloKClIKSotqCloKAgqQACRaChLKSxgANBMKCgoKWgoKCgqaAkpaCgoCCpoKCgoKClICCpJbCgoKEhIKChIKCgpSWgqiCgoKCqJaS0oKSgoKCpIKmuKSWgpQACRTugoSCooLOppaCgoKCgoKmgoKCgriCgoKUgoKClIKoqISygoKCAAcShIKClpKCgpSEgoKUgpSmgoSWzOKChIKEtoSAyraAgqaCgoKCxpTihIKEgozEgMqkgIKmgoK0spSyhIKEgoKWkoKCgoKCgpSCloKCgpSWABAqhICCxLIACRLCgoS09JSCgqiCgoSCgoKUgoKUgoKUgpSClIK42IKCgrKCgoKUgqQACQqmgoKCgqaC", "62-67:1;467-469:1;499-513:1;515-521:2;575-587:1;651-661:1;711-717:1;803-811:1;865-867:1;871-880:2;884-887:3;891-894:4;898-901:5;905-917:6;921-931:7;945-947:8;965-965:1;975-986:2;988-1021:3;999-1005:3.1;1023-1118:4;1024-1117:4.1;1025-1043:4.1.1;1031-1034:4.1.1.1;1035-1037:4.1.1.2;1071-1073:4.1.2;1122-1235:5;1142-1163:5.1;1171-1202:5.2;1178-1190:5.2.1;1196-1201:5.2.2;1204-1234:5.3;1237-1295:6;1253-1267:6.1;1356-1369:1")]
[assembly: global::go.GoPositionMap("runtime/mfinal_test.go", "mfinal_test.cs", "ABUkABMMgoKSgpSWABgIoqKioqyCgqiygqiKgoKClIKCAAkUgoKCooKSooKClIKUlIKUgoKm2JKEtoKCgoKClIKClILcgoKCggANGoKEgoKCgpSCpuqSgpSWhIKQkoKCyoKCgpKCgoKCzJKClISClJCSgoLMkpKSggAKGIK2ooKogoSCgpSCgoKCtA==", "26-31:1;37-37:2;37-37:3;38-38:4;38-38:5;39-39:6;39-39:7;40-40:8;40-40:9;41-41:10;41-41:11;46-50:13;46-46:12;55-68:14;84-99:1;87-96:1.1;112-112:1;117-130:1;134-139:1;178-178:1;209-209:1;219-219:1;220-220:2;248-250:1")]
[assembly: global::go.GoPositionMap("runtime/mgclimit_test.go", "mgclimit_test.cs", "ABIaopaCgoKCuoKCloaSgoSClIKqgoKCgqqClIKUgoKqgoSAggAOHoKCgoKogoKogoKCqIKCgIKkgpSCqIKCgpSClIKUgrqCgoKUgpSClIKogoKAgqSClIKogoKClIKUgIKkgqiCgoKUgpSAgqSCqIKClIKUgIKkgqiCgoKUgpSAgqSCAAYQgoKUgpSAgqSCqJY=", "18-22:1;26-29:2")]
[assembly: global::go.GoPositionMap("runtime/mgcpacer_test.go", "mgcpacer_test.cs", "ABggooSCAA8ggpSWggASKIKmgpYAEiiCpoKWABAkgqaCABAkgqaCABAkgqaCABIogoK4AAcQ3oIAESaCuIIAECSCuIIAESaCyqaCABEmgri4lKiUggARJoK4uJSolIIAESaCpqaUqJSCABQsgoCCpJSWggARJoKAgqSUloIAESaCpoCCtrioABAkgoKAgramgIK2uKgAECSClICCtqaoggAQJIKAgqSmqIIACRaCkoSCgoKCgoa4goKUlJKSkoKWgoKUgoKCgpSCgqaUqgAlToKCgqiClrqCgoCmgrgABxKCgqgACBKCloSEABg4ggAQEoKWgpSCpILGgsaAgqSAgqSCqAAYNoKmgqaCpoKmgoKmgoKCAAUSkgACEsKCgoKUgryigoKCgoKUvKKCgoKClLyigr6igoKCgpS8oq6igoK8wpKCgpS8ooKCgpS8oq6irqKCgoKSgpS4goKClIKUlKaCgoKCgoKUgqaClIKUgoKCpoKCpoKClIKUpoKClIKUgpSClIKUgoKUgg==", "35-45:1;60-73:2;88-101:3;116-124:4;139-147:5;162-170:6;187-212:7;228-237:8;252-261:9;277-290:10;306-327:11;343-364:12;380-399:13;418-431:14;447-460:15;476-495:16;510-534:17;549-567:18;582-597:19;607-756:20;631-651:20.1;882-884:1;893-899:1;906-913:1;920-926:1;933-935:1;942-948:1;954-956:1;962-965:1;971-977:1;983-989:1;995-997:1;1003-1005:1;1011-1019:1", "", "722=HeapMarked/1/2/2,HeapMarked/2/2/3,Triggered/1/1/4,HeapLive/1/1/5,HeapGoal/1/1/6;762=min/1/4/1,Invoke/1/5/1,min/2/4/2,Invoke/2/5/2,min/3/4/3,Invoke/3/5/3,limit/1/1/4,Invoke/4/5/4,quantize/1/1/5,min/4/4/5,Invoke/5/5/5")]
[assembly: global::go.GoPositionMap("runtime/mgcscavenge_test.go", "mgcscavenge_test.cs", "ABUoooKCppSUgpSUlKaCgoKUgoKCgoK4lJKCgIKCgraCABImgpSUhLiCuIKmABQMggAQLIKCytzu3Nzc7u7cgtwABxCC3NzcAAcQlIKC7JT+/IIACRLugpKSgoKCggAUDpKClIiClABu6gGCABImgpKygoSCgIK2goSi+KasgpKChpKCgpSSgoKUgoKClJSCgpSUmoKWgIK2goKCupaCgpKmqIKSpqiCgpKmAAcSgoKCgpKmlgANBqqKAA4espa6goKCgpSWkoSClJKEgoKUgpSClJSShJSYACccptym3Kbcptym3KaC3KaC3KaCgu6mgu6mgu6CuILugqbcgqaC7oKmgsqSooKCgoKClKKCgoKmkoKCgoKUlJKCgoKU+IKClIK4oobMgoIACA6ClIKmgpaCgoKGooKC", "41-55:1;56-63:2;277-284:1;439-452:1;459-461:1;472-475:2;476-487:3;488-493:4;494-496:5;499-512:6;591-636:1;609-614:1.1;615-629:1.2;630-634:1.3;647-647:2;648-648:3;652-654:4;655-657:5;661-663:6;664-666:7;670-672:8;673-675:9;679-681:10;682-684:11;688-690:12;691-693:13;697-699:14;700-703:15;707-709:16;710-713:17;717-719:18;720-725:19;729-731:20;732-736:21;740-742:22;743-747:23;751-755:24;756-760:25;764-767:26;768-770:27;774-777:28;778-782:29;786-789:30;790-794:31;798-805:32;806-811:33;813-821:34;822-829:35;842-844:1;845-847:2;853-883:3;868-870:3.1")]
[assembly: global::go.GoPositionMap("runtime/minmax_test.go", "minmax_test.cs", "AA0egoKCgtYAChaUgqaCgoCCpICCtoKAgqSAgtqCgoCCpICCtoKAgqSAggAFEgAIAoKCgpaAgqSAgqaAgqSAguyAooCigNaSgoSSgIKkgIK4goKCgoKmooKCgsqigoKC", "114-121:1")]
[assembly: global::go.GoPositionMap("runtime/mpagealloc_test.go", "mpagealloc_test.cs", "AA8cxIKCgpSClpSCgpSClIKUggAMDLKClAB29gGCAAsWgpLWgoKUgoSCloKUgoKCuIKCgoKUgoLGAA8IooKUANUCvgWCAA4igoIAFDryAC9igpKygoSCgoKUgqaChKLIgoKUgoK0goKUgpaCgoKAgsqAgriCgoKCgpKCgpSmgpaiAAsIgoKUAM0BpgOCkrKChIKUgoSiAA8IgoKUABlEgpKygoSCgoCCtsY=", "185-220:1;680-697:1;707-749:1;970-981:1;1025-1038:1", "", "182=PageBase/1/4/6,PageBase/2/4/6,MakeAddrRange/1/2/6,PageBase/3/4/7,PageBase/4/4/7,MakeAddrRange/2/2/7;617=PageBase/1/4/10,PageBase/2/4/11,PageBase/3/4/12,PageBase/4/4/13;646=PageBase/1/3/26,PageBase/2/3/34,PageBase/3/3/40;791=PageBase/1/31/12,PageBase/2/31/13,PageBase/3/31/14,PageBase/4/31/15,PageBase/5/31/16,PageBase/6/31/30,PageBase/7/31/31,PageBase/8/31/32,PageBase/9/31/46,PageBase/10/31/47,PageBase/11/31/48,PageBase/12/31/49,PageBase/13/31/50,PageBase/14/31/63,PageBase/15/31/76,PageBase/16/31/77,PageBase/17/31/78,PageBase/18/31/79,PageBase/19/31/80,PageBase/20/31/92,PageBase/21/31/93,PageBase/22/31/94,PageBase/23/31/106,PageBase/24/31/107,PageBase/25/31/108,PageBase/26/31/120,PageBase/27/31/133,PageBase/28/31/147,PageBase/29/31/161,PageBase/30/31/176,PageBase/31/31/197")]
[assembly: global::go.GoPositionMap("runtime/mpagecache_test.go", "mpagecache_test.cs", "ABAcsoKUgpSCAA4IgoIAgQGUAoKSkpKCgoKUgu6CgpSCgpKCgoKUlIKCuIKUlMSmpoKWkoKCqKamgpaippaWlJaClgALCIKClABq5AGCuoKCABMogpKygoSCgoKmgoSi", "168-179:1;187-207:1;208-238:2;408-422:1", "", "242=Invoke/1/1/1;245=Invoke/1/1/1;398=PageBase/1/2/10,NewPageCache/1/3/10,PageBase/2/2/11,NewPageCache/2/3/11,NewPageCache/3/3/12")]
[assembly: global::go.GoPositionMap("runtime/mpallocbits_test.go", "mpallocbits_test.cs", "AA8g4oKCgoKCgpSUqqKCgpQACwyygpSSgoKUkoKClJKCgpSSgoKClJKCgoKCgoKClJKCgoKCgpSSgoKUurKCvKKClIKUggANCIIARpwBgpKSgoKAggAHEqKClIKmABMKkoKK3Nzc3Nzc3NzcgpKSpoKC3pIADyCCgoKUkoKCAAwOkgBWuAGCkpKCgoKClIKmggALDJIAIEyCkpKCgoKUgoLKgoKCgpKCpoKUgpSCgoKCgoKCpoIADh6mgoKCgg==", "44-46:1;47-51:2;52-56:3;57-61:4;62-67:5;68-77:6;78-85:7;86-92:8;197-204:1;286-294:1;321-326:1;426-439:1;485-494:1;499-506:1;544-548:1", "", "273=PackPallocSum/1/1/3;279=PackPallocSum/1/1/3;285=PackPallocSum/1/1/3;291=PackPallocSum/1/1/3;297=PackPallocSum/1/1/3;303=PackPallocSum/1/1/3;309=PackPallocSum/1/1/3;315=PackPallocSum/1/1/3")]
[assembly: global::go.GoPositionMap("runtime/mranges_test.go", "mranges_test.cs", "ABEYsoKCgpSCgoKCgoKmgrqUgqaCuIK4gpSAgqSCgriCpoKqgqqCvIK8gqqCgoKCgpiSgoKCggAKCIKCgpYAiQGiArKSgoKC", "267-273:1", "", "37=Base/1/2/1,Limit/1/2/1,Base/2/2/2,Limit/2/2/2;76=MakeAddrRange/1/1/1;80=MakeAddrRange/1/1/1;84=MakeAddrRange/1/2/1,MakeAddrRange/2/2/2;89=MakeAddrRange/1/2/1,MakeAddrRange/2/2/2;94=MakeAddrRange/1/1/1")]
[assembly: global::go.GoPositionMap("runtime/netpoll_os_test.go", "netpoll_os_test.cs", "ABEegqaigoKCgoKCuII=", "24-27:1")]
[assembly: global::go.GoPositionMap("runtime/norace_test.go", "norace_test.cs", "AA4ikqaCpoKmgqaigoKCgoKCgpSU", "35-46:1")]
[assembly: global::go.GoPositionMap("runtime/panic_test.go", "panic_test.cs", "ABAe0gASLrKCkoKC", "41-46:1")]
[assembly: global::go.GoPositionMap("runtime/panicnil_test.go", "panicnil_test.cs", "ABUcgoKUgoKUgoLoooKCgoSSgoKCgpSCgoKCpoK4", "15-17:1;18-21:2;22-25:3;34-52:1")]
[assembly: global::go.GoPositionMap("runtime/pinner_test.go", "pinner_test.cs", "ABs6goKAkoCmooK01KKCgqaiAAgEgoKCgoKUgoKUgpSCgviCgoKCkpSCgoKCtLSkgoKCtLT4goKCgoKClIKUgpSAgqSCgpSCuIKSgoKClIKClIKUgoKUgIKkgoKUgpSCgpSCuKKCgoKCgrSkooKCgoKCoqSCgoKCgoKCgoKClICCtoKCgpSC+oKCgpKCgpSClIKCgpSClKaigoKCgqKkgoKSgoKCgoKCgoKmgoKC1oKCsoKikoKSlIKCgpSCgrS0pKaigoKCgoKCoqSigoKSgoKCoqSigoKSgrKCopKCoqSigoKCgoKCgoKipKKCgpKCgqKkooKCgpKCgqKkooKCgoKCgqKkooKCgoKCgoKCgoKCgqKkooKCgpSCgoKClLiigoKClIKCgoKClLiigoKClIKCgoKUuKKCgoKCuKKCgoKCuKKCgoKCgriCgoKCgoLKgoKCgoKCyoKCgoKCgoLKooKCgoKClKaigoKCuKKCgoKCkoKmpqKCgpKC/JKCgoKCgpSCgoI=", "42-46:1;74-76:1;259-262:1;264-266:2;267-271:3;308-311:1;451-458:1;462-469:1;473-481:1;508-512:1;519-523:1")]
[assembly: global::go.GoPositionMap("runtime/proc_test.go", "proc_test.cs", "ABsulILWAAgIgoKUgpSCgpKClJSSgpSUgoKCgqaCpoKmgoKCooKUgqSCpMiCgqaCgoKSgoKClKbWwpSUgoKCgpS4uIKCgpKUkoKClJSmgsamxIKC5sKUlIKCgpS4uIKCuIKSgpKCgpS4lJSCuJSCgraCpIKSgpKCgpSUyoLGpIKCgpKCgpSUgriCgpaCgoKigubMgoLmgsqCgpaCgoKigoKCggAKCILKgsqCgoKClKaCgqiCgpSCgoKSgoKUlKaCpqKCqIKCgoKUgoKCgoK4goKUouSCgpSCgoLogoKCguiCgoKCAAgIgoKCgpa6yoSCqIKCgpSCAAgKwoKUgpSmloKCgrSSgrSmgpTcgriCuIKCgoKCAAAQ4oK0pMKClJaCooKkgv6CwoKUgpSygpSUgoKCgoKCgqLIgoKCgoKmgriigpSCgoKCgpSipIKClIKolqTaiLKmooKUgoK6hKaUpKS4gqaCpsKUlKi4hIKClKKkgoKCyoKmgqaCpoKmooKCkoKClJSClIK4ooKCgoKCgoKCgpSmvKKCgoKCgoK6gqamgoIAAxQACgaChIKCgoKmpqKCgpKCgqbWgoKWggAJFIKCACFEgoKCwoKUgpSClJSylIKUgpSUgoKCyoKCggAMGIKClILcoqaCgoKCgqaCgoKCgqamgoKCgpSCgoKCkpSCgoKCkqaCgqaCgoLKgriC1oKCloKCgoKUgoKCgoKClIKCgoLKgvaCgoKCloKC+IKCgoKCkoK4goSCgpaCppSCgoKCAAQQsoKUqLKCloSCgpSagoKUgoKSgoKUuIKUlIKUhILGpIKCloKCgsqCrsKCqISCgpKCgpS4goKCgoKkAAgIgIIACQyigpS4gqaC", "42-47:1;48-53:2;72-85:1;94-102:1;131-139:1;182-189:1;211-223:2;213-221:2.1;234-240:1;254-262:1;284-297:1;327-335:1;356-360:1;449-462:1;502-511:1;515-521:2;522-527:3;651-655:1;677-683:1;700-705:1;718-727:1;753-755:1;765-767:1;788-854:1;827-838:1.1;839-849:1.2;859-864:1;877-879:1;955-975:1;1062-1068:1;1094-1096:1;1118-1122:1")]
[assembly: global::go.GoPositionMap("runtime/profbuf_test.go", "profbuf_test.cs", "AB4egoSSlIKCgpSCpoKCsoKClJSUgpCkgoKClIKCqIKEsoKCgoKClrKCgoKClrKCgoKWsoKCgpbEgoKCgpSCgpSClsSCgoKClIKCgoKWsoKCgoKUgoKClsKCgoKCgoKCgoKCgoKClrKCgoKCqLKCgoKCqLKCgoKC", "18-20:1;21-29:2;30-43:3;32-40:3.1;42-42:3.2;44-53:4;58-65:5;67-73:6;75-80:7;82-87:8;89-103:9;105-118:10;120-131:11;133-148:12;150-157:13;159-166:14;168-175:15")]
[assembly: global::go.GoPositionMap("runtime/rand_test.go", "rand_test.cs", "AA0cgoK03pKCgsqCgoLKgoKClJKCggAHEIKSgoLexsbExoKCgqaCgoKmgoKC", "26-30:1;34-38:1;46-52:1;59-63:1")]
[assembly: global::go.GoPositionMap("runtime/runtime-gdb_test.go", "runtime-gdb_test.cs", "ACc+0oKUpKSClIKWkrakgrYACAi0goKUgoKClIKCgpSClNaCgpSCgoKEgpSCvKKCgoKUgoIACRwACgKCuoKAgqaCgpSCgpSUtIIACBAAKEaCgoKCpqaCgpiigoKSgoKUyoLWgoKUACMGgoKWgoKCgoSEgoKClISGkoKCgoKogoKUhIKCgoKWytyoqAAqVIKCgoKWlIKCgpaCgIKmgoCCpoKCgIS4goKAgqaCgIKmgoCCpoKAggANHoCGuIKEgoCCpoCCpoKAggAiPsKClO6WgoKChJaCgoKUgoKCgqiSAAgSggASKKKCloKCgpbKxsjUqAAHEIKCgoCCABkqooKCgoSClpaCgoKUgoKCgqgADBqCgoKClpa4gu7cgoIAEiKCgoKChJaCgoKUgoKCgqgACxiCgoKCloSCABcqooKCgoSClpaCgoKUgoKCgqjugoKCgqi4goKCgIIAGzKyhIKCloKChJaCgoKUgoKCgroACRSCgoKCusqCgoKAgg==", "521-524:1", "", "193=FormatInt/1/2/8,FormatInt/2/2/9;348=GOROOT/1/1/1,Join/1/1/1;359=GOROOT/1/1/1,Join/1/1/1;365=Sprintf/1/2/2,Sprintf/2/2/35,Join/1/1/40;571=GOROOT/1/1/1,Join/1/2/1,Join/2/2/7;698=GOROOT/1/1/1,Join/1/2/1,Join/2/2/11;777=GOROOT/1/1/1,Join/1/2/1,Join/2/2/10;845=GOROOT/1/1/1,Join/1/2/1,Join/2/2/5;923=GOROOT/1/1/1,Join/1/2/1,Join/2/2/8")]
[assembly: global::go.GoPositionMap("runtime/runtime-lldb_test.go", "runtime-lldb_test.cs", "ACEmgoKCgpSEgoSClIKWlIKCgpKClIKCgpKCAGCqAaKChISEgoKCloKCgrqCgoKCgpaCgoKWgoKEgoKU")]
[assembly: global::go.GoPositionMap("runtime/runtime-seh_windows_test.go", "runtime-seh_windows_test.cs", "ABMggqYACwSiggAIEoKCgpSSAAgagoKCgoKUlILKuISCkoKCgoKUgoKUrLKokoKClIKmgoKCgoKClIKYxJSCuIKClIK4ooKUlJKAgqSClKKkooKUlLKSgpSClIKUxNaigpSUkoCCpIKUgoK0", "35-37:1;38-38:2;142-148:1;158-170:1;159-165:1.1;180-186:1", "", "56=FuncPCABIInternal/1/3/5,FuncPCABIInternal/2/3/9,FuncPCABIInternal/3/3/10,NewContextStub/1/1/11,GetPC/1/1/11")]
[assembly: global::go.GoPositionMap("runtime/runtime_test.go", "runtime_test.cs", "ACY6lLjKgqaC1qKCgoLcooKCggAJEqKSgpKCgoKC3KKCgoKCgtyigriigoLGpKKCuKKCgoLYpMKCgoLYpKKCuKKCgqbYkgARIAAUMKKChJKClIK0rvKCloKAggAHEIKipIKCuIKCpqbYAAkSgoKCgoIAIQyEipSKlIqWjpaGuIKCgoKogoKCgoKCypSCgoKCqIKCgoKCgsrKgoKClIKClIKCAA8KooKChIKCgoKClJSWgoKClJKEkoK4gsyCgqiCkoKCqIKSgoKCkICApJaCgpaCgoKCloKWgoKogoKCgriUgoIACgiCADJkgraCgpSClIKWgoKC3IIABBCCgoKCpqKCAAsIAA0eAAAQgoKCgqKCgoKCgoL6ABAGAA8gAAAQgoKCgoKSqoKCgoKCgoKUgoKUgoKCgpSCzIKEgoKCgoK6koKCgt6CgoKUgqiWgoKCggAPBoIAV6wBgoKCgqiSlJii", "104-108:1;119-123:1;129-133:1;144-148:1;217-221:1;312-316:1;333-337:1;372-406:1;373-392:1.1;393-405:1.2;396-404:1.2.1;409-412:2;417-420:3;428-428:4;433-436:5;440-443:6;449-452:7;457-460:8;523-542:1;547-553:1;548-552:1.1;595-605:1;639-668:1;684-706:2;716-800:1;717-799:1.1;758-772:1.1.1;779-795:1.1.2;802-808:2;810-810:3;811-820:4")]
[assembly: global::go.GoPositionMap("runtime/rwmutex_test.go", "rwmutex_test.cs", "ABYogoKClIKmgoKCgoKCgoKmgpSUgujCgpS4uISCgqKkgoKCgoKUlIKUpoKCgoKClJSClKaChJKCgoKCgoKUgoKmgriigoKClIKCgoKCgoKCgqL0goqCgoKCgoKCgsqCgoKCgoKCgoKUgoKClKa4gqaCpoKmgg==", "144-155:1;161-178:1")]
[assembly: global::go.GoPositionMap("runtime/sema_test.go", "sema_test.cs", "ABUmwoKCgoIAChaC6KKClIKCotSigpSCgqKkgpKMwoKCsoKCpMbqgrKCgoSCopKCzIKEhNaCgoKChIQADBqCgpSmgoKClJSCyoKChITcgpSCgg==", "69-79:1;83-90:2;108-146:1;147-168:2")]
[assembly: global::go.GoPositionMap("runtime/signal_windows_test.go", "signal_windows_test.cs", "ACQoooKUgpSCgoKCgpaCgoKCqIKCgoKogoKClICCgqa2goKClICCgqbEAA0EooKUuJSCgoKCgpaCgoKCqIKCgoKogoKClIKUlKaCgrTkgoKClIKClIKClNrCgpaCgoKCqIKCgoKCgoKmlpK4gIKkkoKogIK4gJLEAAgIwoKUgpSCgoKCgpaCgoKCqIKCgoKogoKCgoKUhKaAgqaCsoCCkoKUuICCpICCxAAMBIKClIKUgoSCgoSCgpSCgpaChISCgpaCgoKogoKCgoKogoKC", "183-186:1;249-257:1;277-288:1")]
[assembly: global::go.GoPositionMap("runtime/sizeof_test.go", "sizeof_test.cs", "ABMgkoIABRKCgoKUgoI=")]
[assembly: global::go.GoPositionMap("runtime/slice_test.go", "slice_test.cs", "ABIcgoKCgoKykoKCgqaSgoKCppKCgoK6spKCgoKmkoKCgqaSgoKCurKSgoKCppKCgoKmkoKCggAUGIKCgoKCgqaCgoKCpoKCgoKmgoKCgqaCgoKCgqaCgoKCpoKCgoLegoKCgqaCgoKmgoKCpoKCgqaCgoKCpoKCgqaCgoIADRiCgoKCgpSUgoKClJSCgoKUuKKCgoKCgoLKooKCgsqigoKCgvqCgoKCgoKCAAcUgoKCkoKC3ILugoKCgtyigoKCgoKCgoKUAAcQgoLWgoKCgriCgoKCgoLogoKCkoKCgpSUkoKCgpQADTYADgKChIKCgoK6goKCgrqCgoKCuoKCgoK6goKCgs6ChIKCgoKCuoKCgoKCuoKCgoKCuoKCgoKCuoKCgoKC", "19-42:1;20-26:1.1;27-33:1.2;34-41:1.3;43-66:2;44-50:2.1;51-57:2.2;58-65:2.3;67-89:3;68-74:3.1;75-81:3.2;82-88:3.3;100-106:1;107-113:2;114-120:3;121-127:4;128-151:5;129-135:5.1;136-142:5.2;143-149:5.3;155-160:1;161-166:2;167-172:3;173-178:4;179-199:5;180-185:5.1;186-191:5.2;192-197:5.3;209-215:1;216-222:2;223-229:3;265-272:1;283-288:1;300-306:1;355-362:1;363-370:2;397-445:1;400-407:1.1;409-416:1.2;418-425:1.3;427-434:1.4;436-443:1.5;447-500:2;450-458:2.1;460-468:2.2;470-478:2.3;480-488:2.4;490-498:2.5")]
[assembly: global::go.GoPositionMap("runtime/softfloat64_test.go", "softfloat64_test.cs", "AA8ekoKCgriAooCigKKApIIAL1iCgoKWgoKSgrqSqJKokqiSqJKokoKmlKiS+qLMgIIACwiCgoKCgoKUgoKCgoKCgvqCgoKCuIKUpKSk1oKCgoK4goKUgpQ=", "16-20:1", "", "42=Copysign/1/1/2,NaN/1/1/5,Inf/1/2/6,Inf/2/2/7")]
[assembly: global::go.GoPositionMap("runtime/stack_test.go", "stack_test.cs", "ABou4ryClIKCgoKCgpKCkoKClJSCpoLMlIKCgoKCgriCgoK01tKCloaSgqKCkoKikoKWgqKCgoKikoaSgoKCgqKCgoKikpKEgJSCyIKUpraCgqKuwoKClIKSgoKUgqaqwoKCgpSCkoKCuIKCloKigoKSgrSmgqKCgpKCtKaCooKCgpKUtJKmgoKCgoKUlIK8wqSihIKCpoKiAAUU4oKCgoCC2qKCkpSCoqSyrLKCgoKAgtqigpKCgpSUgoCypv4ABRAACASCgIKkAAcQoqrSgpKCgrKSgoKykoKU1tbWooKCyJSClKaUgpSi7IKClKiSgoKUgqaCtoKCgpSmhuqCgoKCgpSCgpSCpoKUgsqCgoKCgriCgoKCgrj8goCCtoLWooKCkpKClLiigpSCpqKCgpKClLiCgpSmooKCkoKUuIKClKaAooCigKKAooCigKKAooCigKKAooCigKKAooCigKKAooCigKKAooCigKKAAAgSsoKUkoKmooKCkoKClLikkoKCloKCkoKCyqKIsoKUgpTagoKClKaCgqaCgqakgKSilJSAksiigpSCgoKCgpSC+qKClIKC6KrCgoKCgpSCgoKmgoK4ogAMDIKUgKSUgpAACAjGooKCgpSCgoKCkoKClIKmlKKSooKCgpSCgoKmorbcgoKCgpSC6IKCuoKmkoKCgoKClKSkgqaCAAkIgoKChIKCgoKWgoKogoKCqICSpoCSpoCSAAgMkoKCuIKCgoSCgoKUgoKSgoKmpoK6woKUlIKCyoKUgqaCgujUgrakgoK+sq7igpSC", "39-50:1;41-47:1.1;89-94:1;100-105:2;114-119:3;126-135:4;189-196:1;192-195:1.1;200-207:2;203-206:2.1;211-220:3;213-219:3.1;215-217:3.1.1;226-232:1;246-250:1;275-277:1;301-307:1;309-309:2;330-335:1;350-367:1;353-366:1.1;354-364:1.1.1;357-363:1.1.1.1;358-361:1.1.1.1.1;374-379:1;380-382:2;383-390:3;416-416:1;483-487:1;495-499:1;515-518:1;533-536:1;590-594:1;609-613:1;700-716:1;725-728:1;727-727:1.1;729-733:2;732-732:2.1;739-761:1;740-759:1.1;762-775:2;763-773:2.1;886-893:1;908-910:1;914-916:2;917-919:3")]
[assembly: global::go.GoPositionMap("runtime/start_line_amd64_test.go", "start_line_amd64_test.cs", "AAwesoSCgoI=")]
[assembly: global::go.GoPositionMap("runtime/start_line_test.go", "start_line_test.cs", "ABAq4qaCqJSmzqaCpoLKlIamgoIACQjGhAAfSrKSgoLekoKCgpaClIKClg==", "57-59:1;64-66:1;112-117:1")]
[assembly: global::go.GoPositionMap("runtime/string_test.go", "string_test.cs", "ABYmooKSgoL6ooKCgoIACAqigoKCgvqigoKCgsqigoKUkoKCpqaigoKUkoKCpqaikoLcgpKCgoKm+AAMEJSCspKCyoKykoKCgpTKgrKSggALDoKCspKC3IKykoLcgrKSggAKEKKCgoKCgsqiuoKCgoKCpoKCgvqCgoSC+IKCgpKCpoK4goKCkoKUgpSClJSUlJSUpoK4lIKCkoKmgriUgoKSgqaC6IKCgpaAgsiUgoKUgIKokoKUgoCCyIKCgoKCgqaCuIKCgoKCuIK4goKCpqaCgoKm1tiCgoKUgoIACxgAGDCClIKCgoLagoKCggANGgAWLIKCgoKCAAsMggBl5AGCgg==", "99-103:1;118-126:1;120-124:1.1;127-139:2;129-137:2.1;140-148:3;142-146:3.1;152-161:1;154-159:1.1;162-171:2;164-169:2.1;172-181:3;174-179:3.1;229-233:1;242-264:1;274-278:1;288-292:1;332-338:1;346-352:1", "", "299=Repeat/3/4/1,Repeat/4/4/1")]
[assembly: global::go.GoPositionMap("runtime/symtab_test.go", "symtab_test.cs", "AA8cooKCgpKClJSSxqzC+LKCgoKM3IKCuKSCgAAUBAAQHvgACQgACAyWkgALEoKCggAIBoKCAB1KgILsooKAgraAgsTIlAACEPaCloKClgAFMgAYApaCgrqClLqCgpLMgoKCAAoIgoKClIKSgoKCuJKCgoK4koKCgg==", "18-23:1;24-26:2;161-165:1;261-268:1;269-276:2;277-284:3", "", "59=HasSuffix/1/3/1,Name/1/2/2,HasSuffix/2/3/2,Name/2/2/3,HasSuffix/3/3/3,Entry/1/1/5;60=Entry/1/1/1,Name/1/1/1;98=lineNumber/1/6/4,lineNumber/2/6/4,lineNumber/3/6/7,lineNumber/4/6/8,lineNumber/5/6/11,lineNumber/6/6/12;115=lineNumber/2/4/1,lineNumber/3/4/1,lineNumber/4/4/2;123=lineNumber/2/4/1,lineNumber/3/4/1,lineNumber/4/4/2;133=lineNumber/1/4/1,lineNumber/2/4/2,lineNumber/3/4/3,lineNumber/4/4/4;142=lineNumber/2/3/1,lineNumber/3/3/2;146=lineNumber/2/3/1,lineNumber/3/3/2")]
[assembly: global::go.GoPositionMap("runtime/synctest_test.go", "synctest_test.cs", "AA0WgoKCgg==")]
[assembly: global::go.GoPositionMap("runtime/syscall_windows_test.go", "syscall_windows_test.cs", "ACU+goKClKaigoKUAAwGgpiCuIgAFQiEAA0mAAAgkoKCgoKE7IK4ggAJCIKCgriCAAsIgoKCgpKClIKClIKUgoKUgriigtjSgoKSgqLUgoKAkoK4gvaigoSClIKCgpSClIKmgJIACQa0gpSCgoKUgpSCpoCS1rSCuIKCkoKmkoKCgIIACgyihIaCgpSChIC0tKS2goKClIK0AAoMgoKClIKClOamgoKClIKCgoKmgoKUpgAJEIKEgoKUggAIDAAOVoK6kqiSqJKokqiSqJKokqiSqJKokqiSqJKokqiSAAIS4oLcABEkgoCCpAAPEKKCgpSUiIKCtKSCgoKCgoKCgoKUpgANHqKAgqSEgoSyooKCkrKigpSigsbEABgEgoKCgoKClAAAHtqCgoKUgoL4goKC9oKClIKClIL4goKCAAgIooKCgoKClIKClIKmgoLogoKCgpSAkuyCgriigoKCgoKUlIIAHwaigIKmAAAahIKCgpSCgoKCgpSEgpSEgoKYAAUSgrKClICCxKSCgIKkgpaCgqKCgoKCgoKClITahIKCgpSCgoKCgpSEgpSWgoK0ABMIooCCpIKWAAAWhIKCgpSCgoKCgpSEgpSEzIK0ABwEooCCpIKWAAAkhIKCgpSCgoKCgpSEgpSEzIKCloTMgoK0pIKCgryigpSCgoKCqAASBqKCgoKEgoKUhIQAABaCgoKUgoKCpoKClISCgpQACwbClIKWgKS0gpS2moKChIKCgoKCgpKClJKCgoKmgoKoooKCqIKClISSgoKWgoKWgoKUgoKUgrQAEwaSgIKmhAAAEoKCgpSCgoKCgpSEgoLMqIKCgpS6goKCggAIEOKAgqaCgpaEgoKCgoKUhIKUgoKUgoKUgoK0pMaCqIKEgpaCgoKUpoKCgpSmooKCgoKUkoKCpoKCgpS4ooKCgpSCgpSCgoKAgoLIgoCCpIKUuKKCgoKigoKmgoK4ooLoooSCgoKWgoKCgoKWgoKCgoI=", "141-151:1;177-177:1;194-205:1;206-206:2;215-226:1;227-227:2;240-244:1;245-252:2;258-260:1;565-579:1;570-573:1.1;574-577:1.2;587-590:1;691-698:1;742-745:1;758-761:2;777-820:1;1056-1067:1;1069-1074:2;1197-1202:1;1247-1252:1;1272-1280:1;1296-1301:1", "", "418=Join/1/2/5,Join/2/2/5;949=Join/1/2/3,Join/2/2/3;1030=Float64bits/1/2/2,Float32bits/1/1/3,Float64bits/2/2/4;1099=Float64bits/1/2/2,Float32bits/1/1/3,Float64bits/2/2/4;1109=Float64bits/1/2/2,Float32bits/1/1/3,Float64bits/2/2/4")]
[assembly: global::go.GoPositionMap("runtime/time_test.go", "time_test.cs", "ABomgoK6hISCgpaSgoKEgoKWgoSCgpSCgpaCggAHEoIADxKCgoKClIKClIKCgoKCgoKU5riEkoKCgoKUgoKCgoKClIK6gg==", "106-125:1")]
[assembly: global::go.GoPositionMap("runtime/trace2map_test.go", "trace2map_test.cs", "ABAcgqiCAAcQgoKClIKmgoKClIKmuIKEgoKCooSCAAkQgoKCgpSUgoKClILGpII=", "56-85:1")]
[assembly: global::go.GoPositionMap("runtime/traceback_system_test.go", "traceback_system_test.cs", "ACE2tIKChJSUqrSCgoSUlKaCpoKmgqaCqKKCqLiigqi4lJKCgpamgsqCpqIAChgACwKCgpYAI0iypIKClIKCkoKCgoKWgqiCgqiCggAKIAAXHIKCgoKClIKClJS6goKClJYABxSCloKCgpSogoSEgqaogqiCAAYSgoKCgpa4gt6CgpYAIUaClpaCgqYAAhLipoKqpJSCgoKCgoKClICCgoKUgqamgqY=", "33-36:1;48-51:1;169-200:1;228-242:1;246-252:2", "", "298=Contains/1/1/1")]
[assembly: global::go.GoPositionMap("runtime/traceback_test.go", "traceback_test.cs", "ACsukoKSloKUgpSCgpSUgoKCgoKmgoK6pIKWpIKWpIKWtoIABxKkkqiSpIKkgqjCgoKUuIKU5IKkgoLMkoKC2oKokgACFPKkggAICuqCgpaCgoKWgoKCgpSClIKClJSCgpKClIKClJSUgpSCgoKClIIABRKyppS0tLS0tNaCpIKkpoKUgpSkgqSCABwGgoKUgqaClJYAFj6CADuEAYIACRSCAAkUggAJFIIAChaC3IKCgoLMkoKUlKjIgpSUqtKClJSosoKClKi6goKUqJKClJSokoKUlKiygpSUqLKClJSosoKUlKiygpSUABgusoKUlKiygpSUqLKClJSosoKUlK7ygpSClJSu1AACEuKClIKUAAIS4oKCgpSClJKUAAIS4oIABRCypoKCgoKigsqCgoK0kgAXLOaSgpKClKSClIKCkpSCgoKCgoKCgoKUgra0xIK0goKClIK2gpKSxqqigoKUqJKmguySpoL2goKUggAYMoKCgoKClJI=", "25-54:1;56-60:2;62-66:3;68-72:4;74-79:5;106-109:1;169-220:1;275-282:1;290-290:2;295-300:3;304-304:4;309-309:5;314-321:6;328-328:7;333-333:8;338-338:9;343-343:10;348-348:11;353-353:12;358-358:13;363-363:14;368-368:15;373-373:16;381-384:17;392-395:18;403-406:19;414-417:20;426-429:21;704-716:1;744-747:1;748-757:2;844-844:1;854-854:2", "849=runtime_test.testTracebackGenericTyp[...].M", "367=Invoke/1/4/101,Invoke/2/4/112,Invoke/3/4/123,Invoke/4/4/134,Sprintf/1/1/147")]
[assembly: global::go.GoPositionMap("runtime/unsafepoint_test.go", "unsafepoint_test.cs", "ABMoogAJCqKCtsregoKClIaSgoKCgqKCgoKClIKUgoKUgsqCgoKCqIKolKaUpraClILKgpSC")]
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
    [GoLocalName("T")] internal partial class TestDeferKeepAlive_T {}
    internal partial interface TestMapInterfaceKey_GrabBag_i1 {}
    internal partial interface locker2 {}
    internal partial interface mapBenchmarkElemType<ΔT> {}
    internal partial interface mapBenchmarkKeyType<ΔT> {}
    [GoLocalName("T")] internal partial struct BenchmarkAllocation_T {}
    [GoLocalName("obj")] internal partial struct BenchmarkBulkWriteBarrier_obj {}
    [GoLocalName("Empty")] internal partial struct BenchmarkChanSem_Empty {}
    internal partial struct BenchmarkIssue18740_benchmarks {}
    [GoValueClone("p")] internal partial struct BenchmarkMallocTypeInfo16_type {}
    [GoValueClone("p")] internal partial struct BenchmarkMallocTypeInfo8_type {}
    [GoLocalName("stringarray")] internal partial struct BenchmarkMapStringConversion_stringarray {}
    [GoLocalName("stringstruct")] internal partial struct BenchmarkMapStringConversion_stringstruct {}
    [GoLocalName("RunData")] internal partial struct BenchmarkMemclrRange_RunData {}
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
    [GoLocalName("T")] internal partial struct TestCleanupMultiple_T {}
    [GoLocalName("T")] internal partial struct TestCleanupPointerEqualsArg_T {}
    [GoLocalName("T")] internal partial struct TestCleanupStopAfterCleanupRuns_T {}
    [GoLocalName("T")] internal partial struct TestCleanupStopMultiple_T {}
    [GoLocalName("T")] internal partial struct TestCleanupStop_T {}
    [GoLocalName("T")] internal partial struct TestCleanupStopinterleavedMultiple_T {}
    [GoLocalName("Z")] internal partial struct TestCleanupZeroSizedStruct_Z {}
    [GoLocalName("T")] internal partial struct TestCleanup_T {}
    internal partial struct TestExitHooks_scenarios {}
    internal partial struct TestFinalizerRegisterABI_tests {}
    [GoLocalName("T")] internal partial struct TestFinalizerType_T {}
    internal partial struct TestFinalizerType_type {}
    [GoLocalName("Z")] internal partial struct TestFinalizerZeroSizedStruct_Z {}
    [GoLocalName("X")] [GoValueClone("buf")] internal partial struct TestGcArraySlice_X {}
    [GoLocalName("T")] internal partial struct TestGcDeepNesting_T {}
    [GoLocalName("T")] [GoValueClone("a")] internal partial struct TestGcMapIndirection_T {}
    [GoLocalName("X")] internal partial struct TestGcRescan_X {}
    [GoLocalName("Y")] internal partial struct TestGcRescan_Y {}
    [GoValueClone("y")] internal partial struct TestHugeGCInfo_type {}
    [GoValueClone("y")] internal partial struct TestHugeGCInfo_typeᴛ1 {}
    internal partial struct TestLineNumber_type {}
    [GoLocalName("test")] internal partial struct TestMallocBitsPopcntRange_test {}
    internal partial struct TestMallocBitsPopcntRange_tests {}
    [GoLocalName("T")] internal partial struct TestMapHugeZero_T {}
    [GoLocalName("GrabBag")] [GoValueClone("a")] internal partial struct TestMapInterfaceKey_GrabBag {}
    [GoLocalName("key")] [GoValueClone("pad")] internal partial struct TestMapKeys_key {}
    [GoLocalName("T")] internal partial struct TestMapLargeKeyNoPointer_T {}
    [GoLocalName("T")] internal partial struct TestMapLargeValNoPointer_T {}
    [GoLocalName("val")] [GoValueClone("pad")] internal partial struct TestMapValues_val {}
    [GoLocalName("hit")] internal partial struct TestPageAllocAllocAndFree_hit {}
    internal partial struct TestPageAllocAllocAndFree_tests {}
    [GoLocalName("test")] internal partial struct TestPageAllocAllocToCache_test {}
    [GoLocalName("hit")] internal partial struct TestPageAllocAlloc_hit {}
    [GoLocalName("test")] internal partial struct TestPageAllocAlloc_test {}
    internal partial struct TestPageAllocFree_tests {}
    [GoLocalName("test")] internal partial struct TestPageAllocGrow_test {}
    [GoLocalName("setup")] internal partial struct TestPageAllocScavenge_setup {}
    [GoLocalName("test")] internal partial struct TestPageAllocScavenge_test {}
    [GoLocalName("hit")] internal partial struct TestPageCacheAlloc_hit {}
    internal partial struct TestPageCacheAlloc_tests {}
    internal partial struct TestPallocBitsAlloc_tests {}
    internal partial struct TestPallocBitsFree_tests {}
    [GoLocalName("test")] internal partial struct TestPallocBitsSummarize_test {}
    [GoLocalName("test")] internal partial struct TestPallocDataFindScavengeCandidate_test {}
    internal partial struct TestPanicWhilePanicking_tests {}
    internal partial struct TestPanicWithDirectlyPrintableCustomTypes_tests {}
    internal partial struct TestParseByteCount_type {}
    internal partial struct TestReadMetricsConsistency_cpu {}
    internal partial struct TestReadMetricsConsistency_gc {}
    internal partial struct TestReadMetricsConsistency_objects {}
    internal partial struct TestReadMetricsConsistency_totalVirtual {}
    [GoLocalName("Wndclassex")] internal partial struct TestRegisterClass_Wndclassex {}
    [GoLocalName("result")] internal partial struct TestReturnAfterStackGrowInCallback_result {}
    [GoLocalName("testCase")] internal partial struct TestScavengeIndex_testCase {}
    internal partial struct TestSchedPauseMetrics_tests {}
    internal partial struct TestSehLookupFunctionEntry_tests {}
    internal partial struct TestSizeof_type {}
    internal partial struct TestStartLine_testCases {}
    [GoLocalName("Rect")] internal partial struct TestStdCall_Rect {}
    [GoLocalName("key")] internal partial struct TestStructHash_key {}
    internal partial struct TestTimediv_type {}
    [GoValueClone("x")] internal partial struct TestTracebackArgs_b {}
    internal partial struct TestTracebackArgs_tests {}
    [GoValueClone("y", "z")] internal partial struct TestTracebackArgs_x {}
    internal partial struct TestTracebackGeneric_tests {}
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
    internal partial struct atoi32Test {}
    internal partial struct atoi64Test {}
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
    internal partial struct largePointer {}
    internal partial struct largeScalar {}
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
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸplatform() => builtin.initPackage(typeof(@internal.platform_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸprofile() => builtin.initPackage(typeof(@internal.profile_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸrace() => builtin.initPackage(typeof(@internal.race_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸruntimeꓸmaps() => builtin.initPackage(typeof(@internal.runtime.maps_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸruntimeꓸsys() => builtin.initPackage(typeof(@internal.runtime.sys_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸstringslite() => builtin.initPackage(typeof(@internal.stringslite_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyscallꓸwindows() => builtin.initPackage(typeof(@internal.syscall.windows_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyscallꓸwindowsꓸsysdll() => builtin.initPackage(typeof(@internal.syscall.windows.sysdll_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtrace() => builtin.initPackage(typeof(@internal.trace_package));
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
