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
global using syscallꓸHandle = go.syscall_package.ΔHandle;
global using syscallꓸSignal = go.syscall_package.ΔSignal;
global using syscallꓸSockaddr = go.syscall_package.ΔSockaddr;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static global::go.os_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f757420737472696e677d", "expandTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20662066756e63282a6f732e46696c6529206572726f727d", "nilFileMethodTestsᴛ1")]
[assembly: GoTypeAlias("Kill", "const:ΔKill")]
[assembly: GoTypeAlias("Signal", "ΔSignal")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<go.io.fs_package.File, io_package.Reader>]
[assembly: GoImplement<myErrorIs, error>(Promoted = true)]
[assembly: GoImplement<myErrorIs, error>]
[assembly: GoImplement<net_package.Conn, io_package.Reader>]
[assembly: GoImplement<net_package.Conn, io_package.Writer>]
[assembly: GoImplement<os_package.File, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.WriteCloser>(Pointer = true)]
[assembly: GoImplement<randReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<syscall_package.ΔSignal, os_package.ΔSignal>]
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
[assembly: GoImplement<zeroReader, io_package.Reader>]
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
[assembly: go.GoPositionMap("os/copy_test.go", "copy_test.cs", "ABw00oKEgoKUkoCCpICCpoKClJSCgoLCgoCCxJLCgpKAgsSShICCpICCxNSigoSCgpSSgIKkgIKmgoKClJaCgoKCgpSCwoKClJKEgoCCpoKClJSCgqaCqIKClIKWgoKCloKCgpaCgoKogILE6tSCgoKCgoKClIKClIKUgpSU2oKC7oKmgoKUprKCgpaCgpSSgpSClIKmgqKCgoKUlIKCgpQ=", "51-56:1;57-63:2;90-96:1;106-165:2;228-238:1;240-247:2")]
[assembly: go.GoPositionMap("os/env_test.go", "env_test.cs", "ABcekpSkpKSkpKSkpAAIBgAULoKCgoIADQ6CgoKCgpSUgoKCgpS4goKCgoIACwqCgoKCgoKmlICCpIKUgIKkggAJCMKCloKmgoCCyoCCpICCpIKAgsQACASigoKClIKCgpSCgrSq0oSCgsqUgpaCgoKClLqAgpQ=", "72-79:1;76-76:1.1;80-87:2;84-84:2.1;102-110:1;130-139:1")]
[assembly: go.GoPositionMap("os/error_test.go", "error_test.cs", "ABYgooSCgoKUgpKCgoKUgILE5IKCgpaCgoKUgIKmgoKAgqSUgIKk5oKCgoCCpoKAgsiCgpSClAAIEgAPIIKCgIKkgIKkgIKkgIIAChTcgoKAgqSAggAKCqKEgoKClIKClIKSgoKUgoKCgrTUgoKC/IDUgoI=")]
[assembly: go.GoPositionMap("os/error_windows_test.go", "error_windows_test.cs", "AA4egoQADCI=")]
[assembly: go.GoPositionMap("os/exec_test.go", "exec_test.cs", "ABciooKUgpaCgoSCgIK4tPiksoKEgoKUkpTKgpSAgqaGkoKWgrQ=")]
[assembly: go.GoPositionMap("os/exec_windows_test.go", "exec_windows_test.cs", "ABwk1IKUhJKClIKClJKClJSAgqSCgoKClICCgqSAgoKkgIK8soKCsoKClIKAgraCgrSkog==", "66-80:1")]
[assembly: go.GoPositionMap("os/executable_test.go", "executable_test.cs", "ABsggoSUgoKCgpSUgoCClKSWgpSCgoKWlIKCypSCgoKUgoKUgriCgoKUgoKU9oKClKSkhISChIKCloKCgpaCgoI=")]
[assembly: go.GoPositionMap("os/os_test.go", "os_test.cs", "ACk8goKCgpaEpgAUIIKUAAgQgoKU7oKCgoCCgraCugAJEgAIFgAJEgAMFKKCgpSSgIK2goKU1qKUpKSmooKCgpSSgIKkgIK2ppLkgoSCgoKUgpSCggAICIKEhIKClIKUgIKmhIKCgpaCgpSClICCyKKChIKClISCgpSEgoCCxNSihIKCgpSSgoKUgpSCgrTUgoSCgoKUgpSCgoLMsoSCgoKUlIKCgpSCgoK0ppKEgoKClISChIKCAAgIgrKEgoKUkoKClIKCgoKUgpSClJSCpoK05oKyhIKClJKCgpSCgoKClIKUgpSUgqaCtOaCsoSCgpSSgoKUgoKCgpSClIKUgoKClIKUgpSCgoKUgqaCpoK09oKEgoKmgoSCgqaChIKCpqKCgoKClIKCgpSUpqKCgoKClIKCgpSUpqKCgoKClIKCgpSUpoKmgqaCpqKCgoKCyqKCgoKCyoLWgtaCpoKmgqaCqLKCgoKCgpSClIKUgpQACAriloKUpIKClKSkpIKClJKCgpSCgpSSgoKUgoLG1IKClISCgoKClIKWgoKCgoKogoKCgpSAkriCgoKClICSuIKCgoKUgJK4lIKCgpaCgoKCloKCgoKCuIKCgpSAggALCMKc5oKCgpSUgJSCgoKCooKClJLEkoKClJSCgoKUgpaAhKaCgISmgoCCxOayhIKClIKCgoKUlIKClIKClIK0AAkEgoKEkoKClICCpIKCloKUgIKmgoKUgoKUgqaClIKUgpSClILGxAAICIKChJKCgpSAgqSCgpSCgpSClIKClIKUgoKUgpSCgpSClIKUgoKUgpSCgpTWgoKElIKCgoKUgoKUgviCgpSCgpSAgqSCgpSCgriCgpSChIKCloKClIKCloKClIKUgoKUguiCgpSClIKUgpSCxsQACAiCgpSEgILIgoKUgoSClIKUgpSCxsQADQiCkgACEIKClKiykoSAgqaAlIKUpoCCpoKCuoKCgpaAgqaC3IKyhIKClJKCgoKUhIKChIKCgqaiAAoGooKEkoKUpIKCpIKCgpSCgqSCgoKmgoKClIK4lIKUhJaCgpSAgqSEgoKUgIKkpqKCgoKUgriChISCgoKCgoKCgoKCgriChISCgoKCgoKCgoKCguiihIKCgIK4hIKWggAIBo6CpoKElISmgoSCgoL2goKUgoKClIKCloKClIKmkoKClIKCqICCuIKClIK6goK8soIACBKCtoKClLa2goKUgoKUttqChKaigoKUloKCgoKWgoKUhIKCgoK+woKUtriCuIKCgoCCpICCpoKAgqaCgpaAgsiigoKUhIKClJSAgqaAgqaCgpaCgpSCgpSCtOSCqJSUpKSClIKClLaCgoKClIKCgpSClIKUgoKUgpSC3tKCgoKEgt6ChIKCwqbclLTYgoKClIKCgpSCgpSCgoKUgoK0pIKAgsiCgpSCogALBIKEhIKEAA4qgoKC+oKUpISCgpSCgpSAgqSCgpSAggAICIKCuICSgAAMBKKCgoKCgoKUpAANIoKCgoKClIKClIKCgpSCgpSCgoKCuIaUlJSmgqbG1IKCgoL4tIKClJSCgoKUloKClIKClISCgoKClIKClIKAgqSCltaCgoKClIKC6IKEgoKUgpSCupakkqa6goKCggAKCoKEhIKEgoKClIIACRDChISChIKEgoKUgpaCgpSCAAkKkoSEgoSChISCggAJCIKEhIKEgoKWgoKUguqShISEgoLqsoKCgpSUgoK0pIKCgoKClJSClIKClIKCgpQADgaCgoKCgpSCgpSCgpSCgpSCgpSCgpSCggAPDJKClAAHEJKUgrakkoKCgoKUlIKUgoKCgqaCpoKClIKCgoKCgqaCgoKClIKCAA0SgoKCgoCCpIKCgpSUgpSClICCAA0MkgAHEJKygoKAgqSCgoKUlIKUkoKCgqaCpoCCpIKCgoKmgqaCgoKUgoKUgrTagpaWgIK4goCC+IKCgoK4goKCgpSCgoKUhIKClIKClIKWgoKUgriCgoKUgpSClIL4ooKClJSCgpSEgoKUouSChIKCgrjEgoKUgoKUgoKClIKUgoKUgriCgpSEhIKAgqaCgpSCAAgIopSmgoKClIKWgoSCgpSAAAgIxoKClISCgqiC+KKChIKCgoKUlIKCloKCgpaCgpaCloKCgoKWgoKWgsakgoSEgrik2IKEloKClIKCqICCpIKAgqSAgqSCgIKkgoKAgqSUgoCCpJSCgoKCgpSCgpSSgoK4gIL+opaCgoKClIKClIKClqKAgqS6hKKkgoKCgvqClJaCgpaEgpaCgpaCgoK4goKCgpSCggAJCgAFEICigKKAooCigKKAxoCigKKAuJKEgoKCgsqCgpSCgoKAgqTs4sqUgpaCgoKCgoKCgrKCgoKCtKSCogAKBrKUpKSkpKSmhIKCgoKCgoKUlIKWhIKCgrKCgoCCpICCpKiCvIKAgqSAgqS01IKChIKClICCpICCkoCCkoKUAAoKgoKCpoKEgoKUgpaCgoKClJSCAAkIgpSmgoKUhIKClIKWgoKCuIKEgoKUgpaCgoKClJSCAAgIgpSmgoKUhIKClIKWgoKC6IKEgoKUppaCgriClJSCuIKEgoKUgoKUgoKWgoKUgpaCgpaClIKCggAICriEgoKUgoKUgoKUgIKkgoKUgryigpSClOiSgpSCooKCgpSS+IKEgoKCgpSAgqSCgpSAgpKCpICCpICCkoLIgoSCuoCCgpSCgpSCgpSClICCpJT4goKmgoKCgpQADQaChICCpoKClICCqqKCgpSClIK6goKCqIKCuIKEgoKUgoKWgoKClKaChIKC2IKCAAgKgoKUhIKAgqSAgqaCgoKUlJSC6ILegoCCpIKClILogoSCgoCCpIKClIIACAiigpSCgoKUkpKUgrT2xIKUhIKCloKEsoKCgqYABBDUlIKC6rKCgoKCgoKUlILqwrqEgIKkgILElKjEgpSEgoKUgoLSgoKikoKSkpKCgoKUpoK4goKCgpSCgqaCgoKUgoKCAA4KgpaCgoKAgqSCgoCCpICCyoCCvO6CgIKkgoKAgqSAgsqAggAIEOKmgIKkgoKUhIKClJKCgpSEspSCloKCloKmloKClJKCgpSmgoKUgoKUgpaCgpSCgqqCqIKCpgAZCJSWgoKAgqSEgIK4goCCpIKAgriCgIKkgoCCuIKAgriCgpSCgIK4goKUgoCCuIKCgoCC3ICCpISCgoCCpICigpaCgpSCgpSCgpSCqIKClIKClIKWgoCkpLSCgoKUgqiU+IKCgoCCpIKCgpSUgpSAgoKkgIKkgoKUgoLKgoKAgqSCgpSUgpSAgtqUhIKClA==", "138-142:1;166-173:1;340-373:1;377-410:1;414-465:1;683-689:1;691-700:2;702-711:3;713-722:4;769-774:1;775-775:2;781-788:3;789-795:4;796-803:5;1115-1117:1;1118-1124:2;1128-1167:3;1172-1198:1;1323-1328:1;1659-1700:1;1786-1786:1;1787-1787:2;2118-2148:1;2164-2221:1;2173-2220:1.1;2228-2249:1;2262-2317:1;2263-2316:1.1;2599-2653:1;2676-2681:1;2691-2696:1;2729-2738:1;2811-2818:1;2863-2873:1;2895-2914:1;3138-3140:1;3141-3151:2;3142-3150:2.1;3193-3212:1;3326-3333:1;3387-3389:1;3411-3435:1;3437-3452:2;3454-3468:3;3487-3491:1;3616-3681:1;3767-3819:1;3825-3855:1;3859-3875:1")]
[assembly: go.GoPositionMap("os/os_windows_test.go", "os_windows_test.cs", "ACU6kpqChIKClISCgpaCgpSCgpSCloKClIKClIIADBSCgoSCgoKUgoKUgoKUgoKCgoKWgoKClIKCloKCgpSAgoKkgoKUgoKWgoKClIKUppSAggARIoKCgqaCgqaCpoKmgoKCpoKmgqiSAAsYwoKCloKUgpSEgoLoooKCgoKCgoKCgoSCgoKE9oLsgoKCAAYQgoKCuIKCgsqCgpS6lKaigoKUgoKClISEgoKUgoKUgoLWooKCgoKCgoKCgpSChIKCgoTWooKCgoK4goKUuqiChIKClISCgpQACBaCgoLcgoKCqKLkooKClJSCgpSCgoKClIK01MKEhIKEgoKChIKCloKClIKCAAEYAAkEAAoWgoKClJSSgoKohIKClIKCgpSCloKEgoKUhIKClIKWgoKUgrQADwSigIKmhIKEgpSWgoKWgoKUgJTIgoKCuKKEgoKUkqL0goKUhIKClILogoKC6KKEgoKUlIKCloKCloKCloK0pKKChIKCgoKCqIKClJSCgpSEgrSkgoSClIKClISAgqSAgsiigpKWgoKUgoKClIQABxKCgoKSkoKClIKClIKCgoKWgoKCgoKCgoKClJKClIKogoKUgvwACASChIKCgoKUgpSClKrCgoKClISCgpTcsoKClIKAkoIAGAiigpSEhAAAGIKAgqaCgoKCgpYAMmaCgoKWgoKClICSggALCqSSgoKUhIKCuoKCgpSW2JKEgoKU5qKEgoKUlIKCloKClJSCgpaCtKSihIKClJSCgpSAgsSkgoSCgpSAgv6ygpSEgoKAgqaCgIIABBDigoSCgoCCpoSCgoKUhIKClJKAgpKCxNwACAKWgoCCpoKAgsqCgpSChIKCgoKUuoCCpISCgpKCloKClJSCgpSCloKCloK01pKEgoLu1oKCgoKm1oKCgpaChISCgpaCgoKCgoKClICS/KKCgoKClIIADAaCAAoggpKCgpSUgpSUgpSUgpSWkoKUgqaCgpSCgoKAgraAgraCgoKClJSCgpSClIKClJSmgoKAgraAgraCgpSCAAgMoqiCgoCCpIKClIKCgoKUgoKCAAkI6oKCloKEgoK6goKWgoKWgoKCloKUgKamgpSAgqSAgqSApqaCgpSCAAkIooKCgqi6goKCgpSEgpSCgpSAgraCgpSCgoKUgriCgriCgoKCgqaClILogoKAgviCgoKClIKCuKKCgJSCgoKAgqSAgqaCgpSCqIKClIKClIKUgpSCqIKClIKClIKUgrQ=", "257-262:1;268-273:2;283-289:3;356-362:1;386-388:2;393-398:3;402-407:4;491-496:1;691-693:1;718-762:2;720-733:2.1;1258-1322:1;1475-1481:1;1482-1484:2;1485-1487:3;1511-1511:1")]
[assembly: go.GoPositionMap("os/path_test.go", "path_test.cs", "ABMglMKEgoKCgpSWgoKogoKClKaCgpSCgpSCqIKCgpSCgpSCloKCgoLGpIKChIKCgIKmgoCCpoKAggAKCIKUpIKWgoKCgpSClJQ=")]
[assembly: go.GoPositionMap("os/path_windows_test.go", "path_windows_test.cs", "AB4kuIKClIK4ggA/jAGCgoSCgoKEgoKCgoKCyoKEgoKClICCpICC+IKChIKCgoKUlIKAgqaCgILIgoS6goKClLiUgoKAgraAgviCgpaCgoKUpoKCloKClIKCgoKUgubGgoKCgoKUgoK4ooKCgpSUgILIpoCCyIKEgqaChIKmooKCgg==", "241-246:1")]
[assembly: go.GoPositionMap("os/pipe_test.go", "pipe_test.cs", "ACQ8AAkWgoKUgIKmgpSmgoKClICCpICCpIIACwqilKaCgpSUpKSAgsbslrqCgpSAggAIEIKCgoKCgoKClICCgqSAgpKAgpKCgqb8goKCgoKAgpKAgpKAgpKCxICC+NqCpoKAgoCCxpaCgpSSuNiEgoKUlIKogoKUlIKSgIKSgpTEpIKmguzSlKSCgoKCgoKAgraWgoSCgpSSkoKCgoKCgrTkgoSCgoKmpoKEgoKmgqjSmKKCgoKCloCCpIKUzIKEgILIgqaChIKClgACEAALCoKClIKSgIKkloLChIKCgoKCqIKAgsSUgoKCgoKUloKCgrSm/IKClJKUgrKCopSCgoKkotTChIKClJKUhIKCgsKCgoKCgILElILSgriCuoKilKI=", "194-209:1;306-321:1;364-369:1;372-388:2;422-425:1;450-458:1;461-475:2")]
[assembly: go.GoPositionMap("os/rawconn_test.go", "rawconn_test.cs", "AA8iooSCgpSSlIKClIKCloKCgpSClIKWgoKSgpSClIKUgpSCtA==", "37-40:1;50-53:2")]
[assembly: go.GoPositionMap("os/read_test.go", "read_test.cs", "ABIegoKClIL4goSCgoKWgoKCltaihIKClJKEiICCpoKCloK01KKClIKUloSCgoKClIKClIKClIK4goSCgoKWgoKCloKCgpS0xoKUgg==")]
[assembly: go.GoPositionMap("os/removeall_test.go", "removeall_test.cs", "ABsmooSCgIKmgoKCloKClIKAgqSAgriAgqSCgpSCgIKkgIK4gIKkgoKUgoKClIKAgqSAgriUgIKmgoKClJSAggAJFIKEgoCCyICCpICCAAgKkoKUhIKWgIKkgoKCgpSUgIKkgIL4gqS0poKWgoKAgqSAgtyCgpaCgriChICCyIKEgoKCgIKkgIKkgoCCAAgMsoSCgoCCyoSAgqaAgsTWspSmgpaEggAKFqaSgoKUgoKmloKAgraCgoCCypaCgrqAgoKCpqaCgoKCpoLYpIKUpoKWhIKCgIKkgIKkgIL6soKUhIKWgIKkgoKCgpS6gIKkqISUlILclJaCgpSUgoK0AAsEgoKWgoCCgIKk3IKUgIKkgoLMgoKAgqSCgoCCpICCuIKCgoKClIKWgILIooKCgoKCgoKUgoCC", "267-278:1")]
[assembly: go.GoPositionMap("os/root_test.go", "root_test.cs", "ABw0ooKClKKCgoKUkqIAByIADgKCgIKkooKCgoKUgpSUgIKkgoCCtIKAgraAgsgAHkyyooKCgpTIgoKUogADEPKCgoKClJSCgpTIAF/AAYSUAFiqAYKCssKCgIK2goKUkoKCtMiCsrKCgIKkgIK2goKUkoKClICCxPiCgrKigoKUgIKkgoKClILcgrKSgoKCpqiCgpSCgpSClIC4AAkMgrKygoCCpICCtoKClJKCgpSi+KKykoKmgpKCgIK4goKUgoLcorKSgqaCkoKAgriCgpSCggAJDKKCgoCCpIKClIKClJKCgrQACASCspKCgoCCuIKClICSpICCAAkMgrKSgoKUkoKAgriCgpSAkqSCgIKkgIK2gIIAHjgAa+oBgoKUABQ4goKClIKAgraCgpSCgpSSlNaiuJaygoKCgpaCgpSUgoSCgoKCgpaCgoKUgoKUgoCCpJSUgoKCxraCgqKCgoKUlIKUkoKCgpSCggAIDIKCgoKCgpSUgoKUyoKCgoKClJTKooKClIKCgpSUyoKCgoKCgpSUgpTKgoKCgoKClJSClAAICqKUpKSCuJaAgqSCgpSmgIKkgIK4goKUkoKClICSuICSxKSigoKUkoCCpICCxPSCgoKUgqyCyoLKgsqCAAYQgoKCyoKCgpSCwoKCgoKCgpSCgpSClOiAgqSCgIIAByAAEAKCgoKUlISEgoKGkoKCgoKClIKClIKUgpSWlILCgoKUkoKiuIKAprbIgoKCxtSipoKClJKAgqSCgpSSgoKUgoCCxKSCuIKClILKgoKCyqKCgoKUkoCSpoKClJKAkqaAgqSCgpSSgJLE", "27-30:1;31-39:2;129-144:1;357-372:1;378-399:1;406-422:1;428-456:1;462-481:1;487-508:1;514-535:1;562-580:1;586-617:1;796-800:1;809-811:2;823-871:1;876-897:1;903-916:1;922-930:1;939-947:1;953-965:1;971-983:1;1064-1067:1;1070-1073:2;1076-1079:3;1082-1085:4;1088-1090:5;1106-1125:1;1188-1196:1")]
[assembly: go.GoPositionMap("os/root_windows_test.go", "root_windows_test.cs", "ABAisoKClJKAgoLE2tKCgIKkgoKUkoKClIKAgqSAgsQ=")]
[assembly: go.GoPositionMap("os/stat_test.go", "stat_test.cs", "ABkwxIKCgpSWgoKClISCgqaCuoKCgpSUgoKClISCloKCpoKogqaWgoKClJSCgoKUgoKCgqaCgpSEgrSmkoKClIK6koKClIK6koKClIK4gsqmgsqmgriClJSmgoKCgpaCgoKUgpaCgoKUgriigoKClJSCgoKWgoKCloK01IKChIKCgIKkhIKAgqSCgoSCgIKkgoKmgoKEgoKAgqSEgoCCpIKChIKAgqSCgqiSgoSCgoCCpIKAgqSEhIKCgpSCgoKUguiCgpSCyoLKgviUlKaCgoKUgIKkgoKSgg==")]
[assembly: go.GoPositionMap("os/tempfile_test.go", "tempfile_test.cs", "ABYigoSCgoL4ooTKgoKCgpSCgoKC2AAKBIKEhIIACBiysoKCpIKClIKkgrQADQiihIKClgAEEoSigoKUhIKCtJSyooIACBSSguyihIKCgIIADAiChISCAAgYspKCgoKUgqSC", "70-85:1;108-119:1;122-125:2;134-137:3;171-183:1")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("os_test")]
public static partial class os_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct TestAddExtendedPrefix_type {}
    internal partial struct TestCreateTempBadPattern_tests {}
    internal partial struct TestCreateTempPattern_tests {}
    internal partial struct TestFilePermissions_type {}
    internal partial struct TestFileRDWRFlags_type {}
    internal partial struct TestMkdirTempBadPattern_tests {}
    internal partial struct TestMkdirTemp_tests {}
    internal partial struct TestReadlink_tests {}
    internal partial struct TestRenameCaseDifference_tests {}
    internal partial struct TestRootUseAfterClose_type {}
    [GoLocalName("test")] internal partial struct TestSeek_test {}
    [GoValueClone("detail")] internal partial struct _REPARSE_DATA_BUFFER {}
    internal partial struct dirLinkTest {}
    internal partial struct expandTestsᴛ1 {}
    internal partial struct isExistTest {}
    internal partial struct isPermissionTest {}
    internal partial struct myErrorIs {}
    internal partial struct namePosition {}
    internal partial struct nilFileMethodTestsᴛ1 {}
    internal partial struct randReader {}
    internal partial struct reparseData {}
    internal partial struct rootConsistencyTest {}
    internal partial struct rootTest {}
    internal partial struct sysDir {}
    internal partial struct testOpenError_type {}
    internal partial struct testStatAndLstatParams {}
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
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyscallꓸwindows() => builtin.initPackage(typeof(@internal.syscall.windows_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyscallꓸwindowsꓸregistry() => builtin.initPackage(typeof(@internal.syscall.windows.registry_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
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
