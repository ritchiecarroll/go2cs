// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.os_package;
global using static global::go.os_internal_test_package;

// <ImportedTypeAliases>
global using DirEntry = go.io.fs_package.DirEntry;
global using FileInfo = go.io.fs_package.FileInfo;
global using FileMode = go.io.fs_package.FileMode;
global using PathError = go.io.fs_package.PathError;
global using execꓸError = go.os.exec_package.ΔError;
global using flagꓸErrorHandling = go.flag_package.ΔErrorHandling;
global using netꓸAddr = go.net_package.ΔAddr;
global using netꓸError = go.net_package.ΔError;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using runtimeꓸError = go.runtime_package.ΔError;
global using strconvꓸError = go.@internal.strconv_package.ΔError;
global using syscallꓸHandle = go.syscall_package.ΔHandle;
global using syscallꓸSignal = go.syscall_package.ΔSignal;
global using syscallꓸSockaddr = go.syscall_package.ΔSockaddr;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
global using windowsꓸContext = go.@internal.runtime.syscall.windows_package.Context;
global using windowsꓸMemoryBasicInformation = go.@internal.runtime.syscall.windows_package.MemoryBasicInformation;
// </ImportedTypeAliases>

using go;
using static global::go.os_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f757420737472696e677d", "expandTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20662066756e63282a6f732e46696c6529206572726f727d", "nilFileMethodTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b74696d656f75742074696d652e4475726174696f6e3b207865727273205b325d6572726f727d", "readTimeoutTestsᴛ1")]
[assembly: GoTypeAlias("Kill", "const:ΔKill")]
[assembly: GoTypeAlias("Signal", "ΔSignal")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<go.io.fs_package.File, io_package.Reader>]
[assembly: GoImplement<go.math.rand_package.Rand, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<go.testing.fstest_package.MapFS, go.io.fs_package.FS>]
[assembly: GoImplement<myErrorIs, error>(Promoted = true)]
[assembly: GoImplement<myErrorIs, error>]
[assembly: GoImplement<net_package.Conn, io_package.Reader>]
[assembly: GoImplement<net_package.Conn, io_package.Writer>]
[assembly: GoImplement<neverEnding, io_package.Reader>]
[assembly: GoImplement<os_package.File, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.WriteCloser>(Pointer = true)]
[assembly: GoImplement<randReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<syscall_package.ΔSignal, os_package.ΔSignal>]
[assembly: GoImplement<testing_package.T, global::go.os_internal_test_package.cleanuper>(Pointer = true)]
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
[assembly: GoImplement<zeroReader, io_package.Reader>]
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
[assembly: go.GoPositionMap("os/copy_test.go", "copy_test.cs", "ABw00oKUgoKUkoCCpICCtoKClKSCgoLCgoCCxJLCgpKAgsSSlICCpICCxNSigpSCgpSSgIKkgIK2goKClKaCgoKCgpSCwoKClJKUgoCCtoKClJSCgqaCuIKClIKmgoKCpoKCgqaCgoKogILE6tSCgoKCgoKClIKClIKUgpSU2oKC7oKmgoKUprKCgqaCgpSSgpSClIKmgqKCgoKUlIKCgpQ=", "51-56:1;57-63:2;90-96:1;106-165:2;228-238:1;240-247:2", "", "215=LimitReader/1/2/1,LimitReader/2/2/2")]
[assembly: go.GoPositionMap("os/env_test.go", "env_test.cs", "ABcekpSkpKSkpKSklNYAFC6CgoKCAA0OgoKCgoKUlIKCgoKUuIKCgoKCAAsKgoKCgoKCppSAgqSClICCpIIACQjCgqaCpoKAgtqAgqSAgqSCgILEAAgEooKCgpSCgoKUgoK0qtKUgoLKlIKmgoKCgsqAgg==", "72-79:1;76-76:1.1;80-87:2;84-84:2.1;102-110:1;130-139:1")]
[assembly: go.GoPositionMap("os/error_test.go", "error_test.cs", "ABggopSCgoKUgpKCgoKUgILE5IKCgqaCgoKUgIK2goKAgqSUgIKk5oKCgoCCtoKAgsiCgpSClAAIEgAPIIKCgIKkgIKkgIKkgIIAChTcgoKAgqSAggAKCqKUgoKClIKClIKSgoKUgoKCgrTUgoKC/IDUgoI=")]
[assembly: go.GoPositionMap("os/error_windows_test.go", "error_windows_test.cs", "AA4egpQADyI=")]
[assembly: go.GoPositionMap("os/exec_test.go", "exec_test.cs", "ABciooKUgqaCgpSCgILItAAICKSigpSCgpSSpMqClICCtpaSgqaCtA==")]
[assembly: go.GoPositionMap("os/exec_windows_test.go", "exec_windows_test.cs", "AB8otIKUlJKClIKClJKClJSAgqSCgoKClICCgqSAgoKkgIK8soKCsoKClIKAgraCgrSkogAJBKKCgoKmgoKCgIKkkoKmgoKEkoKCpoKUgrQ=", "68-82:1;100-103:1;106-114:2")]
[assembly: go.GoPositionMap("os/executable_test.go", "executable_test.cs", "ABsggpSUgoKCgpSUgoCClKSmgpSCgoKmlIKCypSCgoKUgoKUgriCgoKUgoKU9oKClKSUlJSClIKCpoKCgqaCgoI=")]
[assembly: go.GoPositionMap("os/file_test.go", "file_test.cs", "ABoegpSCgoCCpAAHEIKAgsiCAAcQgoKC+oKUgoKAgqSAgraCuIKCgoKUggAICoKUgoKAgqSAgqSAgqSUkriCooKAgpKAgrSClJSClIKAgtqS3IKigoCCkoCCtIKUlIKUgoCC", "99-125:1;105-116:1.1;127-155:2;135-146:2.1", "", "37=Join/1/11/1,Join/2/11/1,Join/3/11/2,Join/4/11/2,Join/5/11/3,Join/6/11/3,Join/7/11/4,Join/8/11/5,Join/9/11/5,Join/10/11/6,Join/11/11/6;54=Join/1/5/2,Join/2/5/3,Join/3/5/4,Join/4/5/5,Join/5/5/6")]
[assembly: go.GoPositionMap("os/os_test.go", "os_test.cs", "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", "139-143:1;167-174:1;341-374:1;378-411:1;415-466:1;684-690:1;692-701:2;703-712:3;714-723:4;770-775:1;781-788:2;789-795:3;796-803:4;1148-1150:1;1151-1157:2;1161-1200:3;1205-1231:1;1356-1361:1;1742-1783:1;1869-1869:1;1870-1870:2;2161-2171:1;2239-2269:1;2285-2342:1;2294-2341:1.1;2349-2370:1;2383-2438:1;2384-2437:1.1;2720-2774:1;2797-2802:1;2812-2817:1;2850-2859:1;2932-2939:1;2984-2994:1;3016-3035:1;3259-3261:1;3262-3272:2;3263-3271:2.1;3314-3333:1;3448-3455:1;3509-3511:1;3525-3530:1;3531-3536:2;3537-3541:3;3561-3585:1;3587-3602:2;3604-3618:3;3637-3641:1;3766-3831:1;3913-3965:1;3971-4001:1;4005-4021:1;4097-4127:1;4100-4117:1.1", "", "87=Join/1/1/1;111=Getenv/1/1/1;133=GOROOT/1/1/1;1499=Join/1/1/1;2332=Error/1/2/1,HasSuffix/1/2/1,Error/2/2/2,HasSuffix/2/2/2;4651=Name/1/1/1,Mode/1/1/1;4695=Mode/1/2/1,Mode/2/2/1;4701=Mode/1/1/1")]
[assembly: go.GoPositionMap("os/os_windows_test.go", "os_windows_test.cs", "ADFEkpqClIKClJSCgqaCgpSCgpSCpoKCgpSCAAwUgoKUgoKClIKClIKClIKCgoKCpoKCgpSCgqaCgoKUgIKCpIKClIKCpoKCgpSClKaUgIIAESKCgoKmgoKmgqaCpoKCgqaCpoKokgALGMKCgqaClIKUlIKC6KKCgoKCgoKCgoKUgoKClPai3IKCggAHEIKCgriCgoLKgoKUupSmooKClIKCgpSUlIKClIKClIKC1qKCgoKCgoKCgoKUgpSCgoKU1sKCgoKCuIKClLq4gpSCgpSUgoKUAAgWgoKC3IKCgqii5KKCgpSUgoKUgoKCgpSCtNSilJSClIKCgpSCgqaCgpSCggACGAAKBAALFoKCgpSUkoKCuJSCgpSCgoKUgqaClIKClJSCgpSCpoKClIK0AA8EgoCCtpSClIKUpoKCpoKClICUyIKCgriilIKClJKi9IKClJSCgpSC6IKCguiilIKClKSCgqaCgqaCgqaCtKSigpSCgoKCgriCgpSkgoKUlIK0pIKUgpSCgpSUgIKkgILIooKSpoKClIKCgpSUAAgSgoKCkpKCgpSCgpSCgoKCpoKCgoKCgoKCgpSSgpSCuIKClIL8AAgEgpSCgoKClIKUgpSq4oKCgpSUgoKU7LKCgpSCgJKCABgIooKUlJQAABiCgIK2goKCgoKmADJmgoKCpoKCgpSAkoIACwqkkoKClJSCgsqCgoKUptiSlIKClOailIKClKSCgqaCgpSkgoKmgrSkopSCgpSkgoKUgILEpIKUgoKUgIL+soKUlIKCgIK2goCCAAQQ4oKUgoKAgraUgoKClJSCgpSSgIKSgsTcAAgCpoKAgraCgILagoKUgpSCgoKClMqAgqSUgoKSgqaCgpSkgoKUgqaCgqaCtNaSlIKC7taCgoKCptaCgoKmgpSUgoKmgoKCgoKCgpSAkvyigoKCgpSCAAwGggAKIIKSgoKUlIKUlIKUlIKUppKClIKmgoKUgoKCgIK2gIK2goKCgpSUgoKUgpSCgpSUpoKCgIK2gIK2goKUggAIDIK4goKAgqSCgpSCgoKClIKCggAJCMqCgqaClIKCyoKCpoKCpoKCgqaClICmtoKUgIKkgIKkgKa2goKUggAJCIKCgoK4yoKCgoKUlIKUgoKUgIK2goKUgoKClIK4goK4goKCgoKmgpSC6IKCgIL4goKCgpSCgriigoCkgoKCgIKkgIK2goKUgriCgpSCgpSClIKUgriCgpSCgpSClIK0pLSUgoKClKSCgoKUgqaAgraCgpSCtNTGgoKCgoKUgqaCpoK2goKUtoKAgtqCgoKCgoKmgrSkgoKClIKClJKAgramlNqCpoKmgqaCgoKmgoKUgoKUgoKUgpKAgramgoKCooKCgoKUgriCgoKCgoKClIKmgvaCioKisoKCgpSCuIKCgoKCgoKClIKUgqaCpoKCgpSCgoKUgqaCgpSCAAkIgoIABRSykoKCgoKCgsqCgoKCgoCCtoKClIKmgIKmkoCCpIKmgoKUgpSAgpKCpICCkoKkgIKkgIL4xoKCgoKClJSkgoCCpICCxKSigoKClKKAgqSAgraiAAgEgoIABRSykoKCgoLKgoKCgpSCgqaCgoKUgriCgoKClJSCgoKUgriCgoKClIKCpoKCguiCgoKCgoKigqSkgoKClICClIKkyJSCgoKUgriCgtbqgoKCppSCgpSClJSAgsiCkpKCgqaAgqSAgraAgqSAgqSAgsimgILIgoKCyKS0pKKUlIKUsoKCpuqklIKCtJKygqKSlIKClIKAgraAgsTkgoIAGziAggAKCoIABxAABxaysoKCgpSSgoCUpIK0yIKCgoKClICCtoCCyIKGooKClKaigoKCpIKClKIADQSmggAIErKCgoKClJSClICCgqSCgoKUppKAlKSCpoCCpIKAgqSCgIKkgrTIlMqmlIKClIKkgILIgoKCgoKAgraAgqQ=", "259-264:1;270-275:2;285-291:3;358-364:1;388-390:2;395-400:3;404-409:4;493-498:1;693-695:1;720-764:2;722-735:2.1;1260-1324:1;1477-1483:1;1484-1486:2;1487-1489:3;1513-1513:1;1661-1665:1;1707-1711:1;1718-1729:1;1754-1765:1;1823-1831:1;1918-1925:1;1942-1948:1;2015-2035:1;2026-2028:1.1;2070-2078:1;2081-2086:2;2135-2154:1;2155-2158:2;2232-2247:1;2305-2352:1;2373-2378:1")]
[assembly: go.GoPositionMap("os/path_test.go", "path_test.cs", "ABQg1IKigoKCgqaCgoK4goK4goKClLaCgpSCgpSCuIKCgpSCgpSCpoKCgoLG5oKCgoKAgqSCgpSCuIKClIKCgIK2goCCtoKAggAKCIKUlIKmgoKCgpSClJQ=", "19-81:1")]
[assembly: go.GoPositionMap("os/path_windows_test.go", "path_windows_test.cs", "AB4kuIKClIK4ggBFjAGCgpSCgoKUgoKCgoKCyoKUgoKClICCpICC+IKClIKCgoKUlIKAgraCgILIgpTKgoKClLiUgoKAgraAgviCgqaCgoKUpoKCpoKClIKCgoKUgtamgoKCgoKUgoL4goKApIKAgqSAgraAgsiCgoKClJSAgsimgILIgpSCpoKUgqaigoKC", "241-241:1;258-263:1")]
[assembly: go.GoPositionMap("os/pipe_test.go", "pipe_test.cs", "ACQ8AAoWgoKUgIK2gpSmgoKClICCpICCpIIACwqilKaCgpSUpKSAgsbcpsqCgpSAggAIEIKCgoKCgoKClICCgqSAgpKAgpKCgqYACAyCgoKCgoCCkoCCkoCCkoLEgIL46oKmgoCCgILGpoKClJLI2JSCgpSUgriCgpSUgpKAgpKClMSkgqaC7NKUlIKCgoKCgoCCtqaClIKClJKSgoKCgoKCtOSClIKCgqamgpSCgqaCqJKYooKCgoKmgIKkgpTcgpSAgsiCpoKUgoKmAAIQAAsKgoKUgpKAgqSmgsKUgoKCgoK4goCCxKSCgoKCgpSmgoKCtKYACAyCgpSSpIKygqKkgoKCpKLUopSCgpSSpJSCgoLCgoKCgoCCxKSC0oK4gsqCoqSi", "194-209:1;306-321:1;364-369:1;372-388:2;422-425:1;450-458:1;461-475:2")]
[assembly: go.GoPositionMap("os/rawconn_test.go", "rawconn_test.cs", "AA8iopSCgpSSpIKClIKCpoKCgpSClIKmgoKSgpSClIKUgpSCtA==", "37-40:1;50-53:2")]
[assembly: go.GoPositionMap("os/read_test.go", "read_test.cs", "ABQigoKClIL4gpSCgoKmgoKCptailIKClJKUmICCtoKCpoK01IKClIKUppSCgoKClIKClIKClIK4gpSCgIK2goKClIKAgraCgoKmgoKClLS2gpSC")]
[assembly: go.GoPositionMap("os/removeall_test.go", "removeall_test.cs", "ABwogpSCgIK2goKCpoKClIKAgqSAgsiAgqSCgpSCgIKkgILIgIKkgoKUgoKClIKAgqSAgsiUgIK2goKClJSAggAKFIKUgoCCyICCpICCAAgKkoKUlIKmgIKkgoKCgpSUgIKkgIL4gqS0poKmgoKAgqSAguyCgqaCgriClICCyIKUgoKCgIKkgIKkgoCCAAgMspSCgoCC2pSAgraAgsTWspSUggAKFqaSgoKUgoKmpoKAgraCgoCC2qaCgsqAgoKCpraCgoKCpoLYpIKUpoKmlIKCgIKkgIKkgIL6soKUlIKmgIKkgoKCgpTKgIKkuJSUlILclKaCgpSkgoK0AAsEgoKmgoCCgIKk7IKUgIKkgoLcgoKAgqSCgoCCpICCyIKCgoKClIKmgILIooKClILKgoCCpICC2qKCuICCpIKmgoKmotSCgpSUgriigoKCgoKCgpSCgII=", "261-272:1;498-500:1")]
[assembly: go.GoPositionMap("os/root_test.go", "root_test.cs", "ACA6ooKClKKCgoKUkqIAByIADgKCgIKkooKCgoKUgpSUgIKkgoCCtIKAgraAgsgAHkyyooKCgpTIgoKUogADEPKCgoKClJSCgpS4AHHkAZSUAFiqAYKCssKCgIK2goKUkoKCtMiCsrKCgIKkgIK2goKUkoKClICCxPiCgrKigoKUgIKkgoKClIIACQyCgpSykqaAgramgoKmgoKClIKClICCAA0MpoKykoKAgrYADx6UgqSCpoKClIKClICCpIKAggAIEIKykoKmpoKClIKClIKUgLgACQyCsqaUkoKmpoKClIKClIKUgLgACQyCsrKCgIKkgIK2goKUkoKClKL4grKSgqaCkoKAgsiCgpSCgtyCspKCpoKSgoCCyIKClIKCAAgMgrKSuIKAgraCpoKSgoCCpICCtoKApoK2goKUgpSCggAJDKKCgoCCpIKCgpSCgpSSgoKCtAAIBIKykoKCgILIgoKUgJKkgIIACQyCspKCgpSSgoCCyIKClICSpIKAgqSAgraAggANDoKykoKClKamgoKmgoKUgt6SqJKCAAoGgoKyooKAgraCgpSCgoKCuIIAChaCgoLKpoKmgoKCgpSClIKmgoKUgoKCpoK4goKCgqaCgpSCggAGEpKokoLWgoKyooKAgraUgoKCuIKClIK4gqaCgoKClIKUgqaCgoKmgriCggALDIKCspKClKaCgoKUgoIAGTwAGTQAFSwAFSyUlJQANWyCgpQADSSmgu62AAwYgoKClIKAgraCgpSCgpSSlPaCuKaygqaCgoKCpoKClKSClIKCgoKCpoKSlKS0pJSCgoKAgqSUlIKCgsa2goKigoKClJSClJKCgoKUgoIACAyCgoKCgoKUlIKClAAICoKClIKCgoKCgqaSgIKkgoKUgIKkgoKUyoKCgoKClJTKgoKCgoKUlMqCgoKUgoKClJTKgoKClIKCgpSUyoKCgoKCgpSUgpTKgoKCgoKClJSClMqCgoKClNyCpoKC9oKUyoKCgsqCuIKClLS0tKSCgqaCgqaSgoKUgqYABxCCgriAgqSCgoKUAAkOgoKCgoKCgoKClIKUAAgKopSkpIK4poCCpIKClLaAgqSAgsiCgpSSgoKUgJLIgJLEpKKCgpSSgIKkgILE9IKCgpSCrILKgsqCyoIABhCCgoLKgoKClILCgoKCgoKClIKClIKU6ICCpIKAggAHIAAOAoKCgpSklJSCgpaSgoKCgoKUgoKUgpSClKaUgsKCgpSSgqLIgoCmtsiCgoLG1KKmgoKUkoCCpIKClJKCgpSCgILEpIK4goKUgsqCgoL6ooKCgpSSgIKkgIKkgILE1KKCgoKUpIKCgIK2goK0pKKCgoKUkoCStoKClJKAkraAgqSCgpSSgJLEAAgIwoKmpoKCgIKkgoKUpJKygoKUlIKClJKitoKCgJKkgIK2koKAkqSAgqSCgpSmgoKClIK4koKClIKUppKCgpSClKaCgoKUgILWACNQuJiispKCAAQQ6IKClIKUlLKygpSCogAIFOKClJSmlKKCqqKCgpTWgoKUgpKCgoLKlAAQIoK4poK4lNKClIKCgoKUgoKCgpSCgoKCuKamgsqCgoKCgpSCgoKCgriCgoKCgoKUgoL6graCgoKCgpSSptyygpSCgoKUlIKCABYogpSkpKSkpKSkuIIAGDYADBrcwoKCgIKkqJIADh7MABw4lIKCgpS4gtyCgpSCgpSCsoKmgoKUsgAHEIIACCgACwKCgpSqooKUgpSClKaCgpS4lIKUqJKClKaClIK2gqamgoKUgoKClIKUlKSClKSClKSmpoKClIKUppSClKyygoKUgpSokoKClAACGgANAoLagIL8gILYgpSCgpSCgILYgpSCgqSUgoKCgqYABRDSgpSCgpSUpKQAAxQACQKCgpSSkoKUgoKUgpSkgoKUgoKCpqSCgpSCgpSmgpQACA7CAAIaAAwCgraCgoIACBCUlMbGAAwSgoCCAAgOAAkIgoKUgsqCkpIACRKCuJSCpoKUgoKmgoKmgpS0+JSCgpSUpoKCyMqm6IKigoK2goKCtpTYxugACgiCgrimgoKGgraClJS06qSk6IKChoK2gpSCgqaCgqaCppTGtIKAgtaCgIIACAiCyOiCgpiCkraClKb8gpSm2LSCgoKUgra4gqqCgoKClJToooKCtoKUpsaC7MbWgoK4poKGgtqClAAJFqaUtLS0AAoOtLSmxsiCpuiCgoKCtoKCgoKmlLS0AAoOpAAKDoKAgtjogoKCgraClJS0tOaCgrbogoKCgraClJS0tMrGpOiCgoKCtoKUyuqkpOiCgoKCtoKClJS0tOak6IKCgoK2goKAlLaUtOak6IKCgoK2gpSUtLS05qTogqKCgraCgoK2lJS0tOaCgILY", "30-33:1;34-42:2;132-147:1;378-393:1;399-420:1;427-443:1;452-480:1;489-536:1;542-567:1;578-603:1;609-628:1;634-655:1;661-682:1;688-731:1;760-778:1;784-815:1;821-844:1;862-952:1;970-1026:1;1033-1049:1;1260-1264:1;1273-1275:2;1287-1344:1;1317-1327:1.1;1349-1370:1;1376-1389:1;1398-1422:1;1428-1436:1;1442-1450:1;1459-1467:1;1476-1484:1;1490-1502:1;1508-1520:1;1526-1532:1;1554-1622:1;1566-1620:1.1;1629-1641:1;1722-1725:1;1728-1731:2;1734-1737:3;1740-1743:4;1746-1748:5;1764-1783:1;1846-1854:1;2019-2034:1;2020-2033:1.1;2021-2026:1.1.1;2036-2044:2;2045-2058:3;2060-2068:4;2070-2079:5;2081-2090:6;2092-2100:7;2151-2154:1;2165-2173:1;2180-2182:2;2222-2229:1;2265-2287:2;2343-2345:1;2353-2363:2;2514-2566:1;2646-2669:1;2810-2848:1;2857-2915:1;2922-2999:1;2947-2949:1.1;3003-3029:1;3042-3067:1;3071-3120:1;3124-3168:1;3172-3174:1;3178-3186:1;3226-3287:1;3291-3333:1;3337-3361:1;3365-3391:1;3395-3419:1;3423-3445:1;3449-3472:1;3476-3499:1;3503-3533:1", "", "1337=Repeat/1/1/123;3897=finalKind/1/2/1,finalKind/2/2/1,isLink/1/2/1,isLink/2/2/1")]
[assembly: go.GoPositionMap("os/root_windows_test.go", "root_windows_test.cs", "ABgqgoKClIKClIKCgoCCAA4KgoKAyIKCgoKCgoKCgoKAkoK6soKClJKAgoLE2tKCgIKkgoKUkoKClIKAgqSAgsT4woKCgpSkABcsooKAgqSAgraCgpSCgpSC3IKClIKClILYpKKCgpSUgpSUgoKCgqaCgqaClAALCsKCgoKUpICCpICCtpQAFCrCgoCC7IKClJSClIKCgIKklICCxMSkooKCgqSCgpSkgoKUovSilIKCgpQACBAABxaywoKClJKCgJSkgrTEpIKC1qKCgoKUkoKCgpSAgraAgsSkooKCgoKUlqKCgpSi", "22-36:1;41-41:1;53-58:2;55-55:2.1;132-167:1;241-269:1;320-334:1")]
[assembly: go.GoPositionMap("os/stat_test.go", "stat_test.cs", "ABkwxIKCgpSmgoKClJSCgqaCyoKCgpSkgoKClJSCpoKCpoK4gqamgoKClKSCgoKUgoKCgqaCgpSUgrSmkoKClIK6koKClIK6koKClIK4gsqmgsqmgriClJSmgoKCgqaCgoKUgqaCgoKUgriigoKClKSCgoKmgoKCpoK01IKClIKCgIKklIKAgqSCgpSCgIKkgoKmgoKUgoKAgqSUgoCCpIKClIKAgqSCgqiSgpSCgoCCpIKAgqSUlIKCgpSCgoKUguiCgpSCyoLKgviUlKaCgoKUgIKkgoKSgviCgoKCgqaCgoI=")]
[assembly: go.GoPositionMap("os/tempfile_test.go", "tempfile_test.cs", "ABYigpSCgoL4opTKgoKCgpSCgoKC2AAKBIKUlIIACBiysoKCpIKClIKkgrQADQiilIKCpgAFEpSigoKUlIKCtKSyooIACRSSguyilIKCgIIADAiClJSCAAgYspKCgoKUgqSC", "70-85:1;108-119:1;122-125:2;134-137:3;171-183:1")]
[assembly: go.GoPositionMap("os/timeout_test.go", "timeout_test.cs", "ABgoxoKUlIKClIKSgoCCpICCpICCxAAQGAAHGJKUsrKUgpKkgIK2ooCCpIKCgoKCgqaCgpSClNjKkpSyspSCkqSCkoKigIKkgIKkgIKkgoKmtLSCgoLWyAAHGJKUspKUssKCkqSAgqSCgoKCgqaCgpSClMbukpSyspSCkqSCkoKigIKkgIKkgIKkgoKAgoLatLSCgoLWABQwogAIEriqwoLKgqiSlLKylIKSpIKCgoKCgoCCpIKClIKmgoKUgpSAgoKCuIKCtrTKkpSyspSCkqSCgoKCgoCCpIKCgoKCgoKmlIKmgoKUgpSAgsqCuKSAgriCpLa0ypKCspKCzMSClIKykoLGpsSClIKykoLGyIKClAAIBoIAAhKCgoKmABQqgoKCgqaCsoKSpIKkgoKSgqKSgIKkgoKCprSClMa2tLTEAAkMkpSCgqayspSCkqSCgsKCgpKCgIKkgIKktJKygoKCgoLGkrKCgoKCgsaSosyilLKylIKSpIKUlIKCgrKUgoKCgoKCgpTGxMyilLKylIKSpIKUlIKCgrKUgoKCgoKCgpTGxA==", "72-107:1;116-151:1;126-139:1.1;171-203:1;175-201:1.1;212-251:1;222-239:1.1;310-355:1;364-428:1;436-439:1;451-454:1;466-469:1;489-496:1;527-567:2;539-548:2.1;582-626:1;591-604:1.1;605-614:1.2;615-624:1.3;636-668:1;651-666:1.1;678-710:1;693-708:1.1")]
[assembly: go.GoPositionMap("os/timeout_windows_test.go", "timeout_windows_test.cs", "AAsYgtiCgoI=", "16-21:1")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("os_test")]
public static partial class os_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct TestAddExtendedPrefix_type {}
    internal partial struct TestCreateTempBadPattern_tests {}
    internal partial struct TestCreateTempPattern_tests {}
    internal partial struct TestFileKeepsCompletionNotificationModes_type {}
    internal partial struct TestFilePermissions_type {}
    internal partial struct TestFileRDWRFlags_type {}
    internal partial struct TestFile_tests {}
    internal partial struct TestMkdirTempBadPattern_tests {}
    internal partial struct TestMkdirTemp_tests {}
    internal partial struct TestNamedPipe_tests {}
    internal partial struct TestOpenFileFlags_tests {}
    [GoLocalName("readStep")] internal partial struct TestReadFileContents_readStep {}
    internal partial struct TestReadFileContents_tests {}
    internal partial struct TestReadlink_tests {}
    internal partial struct TestRenameCaseDifference_tests {}
    internal partial struct TestRootChtimes_type {}
    internal partial struct TestRootOpenFileFlags_tests {}
    internal partial struct TestRootSymlinkRelativity_type {}
    internal partial struct TestRootSymlinkToDirectory_type {}
    internal partial struct TestRootUseAfterClose_type {}
    [GoLocalName("test")] internal partial struct TestSeek_test {}
    internal partial struct TestSplitPath_type {}
    [GoValueClone("detail")] internal partial struct _REPARSE_DATA_BUFFER {}
    internal partial struct dirLinkTest {}
    internal partial struct expandTestsᴛ1 {}
    internal partial struct isExistTest {}
    internal partial struct isPermissionTest {}
    internal partial struct myErrorIs {}
    internal partial struct namePosition {}
    internal partial struct neverEnding {}
    internal partial struct nilFileMethodTestsᴛ1 {}
    internal partial struct pipeDeadlineTest {}
    internal partial struct randReader {}
    [GoValueClone("xerrs")] internal partial struct readTimeoutTestsᴛ1 {}
    internal partial struct reparseData {}
    internal partial struct rootConsistencyTest {}
    internal partial struct rootMultiTest {}
    internal partial struct rootTest {}
    internal partial struct sysDir {}
    internal partial struct testFileDesc {}
    internal partial struct testFileKind {}
    internal partial struct testFileRef {}
    internal partial struct testOpenError_type {}
    [GoLocalName("op")] internal partial struct testPreadPwrite_op {}
    internal partial struct testStatAndLstatParams {}
    [GoLocalName("result")] internal partial struct testVariousDeadlines_result {}
    internal partial struct zeroReader {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸpoll() => builtin.initPackage(typeof(@internal.poll_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸstrconv() => builtin.initPackage(typeof(@internal.strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyscallꓸwindows() => builtin.initPackage(typeof(@internal.syscall.windows_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyscallꓸwindowsꓸregistry() => builtin.initPackage(typeof(@internal.syscall.windows.registry_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸiter() => builtin.initPackage(typeof(iter_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrandꓸv2() => builtin.initPackage(typeof(math.rand.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸsignal() => builtin.initPackage(typeof(go.os.signal_package));
    [GoInit] internal static void initᴛᴛimportꓸpath() => builtin.initPackage(typeof(path_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(go.path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸruntimeꓸdebug() => builtin.initPackage(typeof(go.runtime.debug_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸsyscall() => builtin.initPackage(typeof(syscall_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸfstest() => builtin.initPackage(typeof(go.testing.fstest_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸnetꓸnettest() => builtin.initPackage(typeof(vendor.golang.org.x.net.nettest_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.os_package));
    }
}
