# DATA: Go attribution scout over the hand-written C# in src/core (2026-11-26)

Record, point-in-time. Measured at base `d6805be2e8` (branch `claude/p5-go-attribution`) against GOROOT 1.26.11.
Step 1 of the attribution sweep (COORD 20261126T053237Z). No source file is changed by this record.

## Method

- Population: every tracked `src/core/**/*.cs` outside golib, go2cs, go.arrays and go2cs.CpuProfiler that is an `*_impl.cs` or carries an uncommented `[module: go.GoManualConversion]` / `[module: GoManualConversion]` marker.
- Go side: the package's GOROOT directory, parsed with go/ast (all files, all tags), restricted to the Go files the file's own targets select (folder and file-name tokens, `build.Context.MatchFile`, purego, CgoEnabled false).
- Per C# method: a name match with a **bodied** Go func and identifier coverage >= 0.5 reads `derived`; a name match below that reads `inspired`; a name match with a **bodyless** Go declaration (a linkname push, an assembly func) reads `glue`, counted as own; no match reads own. Copied comment lines (normalized, >= 30 chars, present in the Go package's comments) count separately; >= 3 makes a file derived.
- File class: `derived` = any derived method or >= 3 copied lines; `inspired` = inspired methods only; `mixed` = either of those plus >= 2 own methods carrying >= 25% of the lines; `own` = none of them; `own (glue)` = own whose only Go counterparts are bodyless declarations.
- This is a **pre-sort**, not the verdict. Hand-read: 11 rows, 9 agree on class. Two disagree: `runtime/sema_impl.cs` reads inspired but its probe methods make it mixed; `internal/reflectlite/type_impl.cs` reads mixed (right class) but names Elem/implements/haveIdenticalUnderlyingType derived where they bridge over GoReflect (inspired), while Implements carries Go's two panic strings verbatim. Identifier coverage overcalls `derived` for bodies that keep Go's names and guards and bridge the rest. Step 2 reads every file before writing its header, because the header names functions part by part.

## Counts

Hand-owned files: 242. derived 55, inspired 26, mixed 72, own 35, own (glue) 54.

Header today: go2cs-notice 192, go-notice 49, go-notice+provenance 1.

Action: none (go2cs header stays) 88, add Go notice + provenance block 84, add provenance block 49, Q1: notice of the //sys-declaring Go file, then provenance 16, Q2: fiat-crypto notice + BSD-1 disclaimer, then provenance 4, none 1.

golib / go2cs / go.arrays files whose comments cite a Go source: 48. cites-only 26, inspired 22. None carries >= 3 copied Go comment lines (most: GoReflect.ValueMarshalling.cs, 2).

## Hand-owned files

| file | Go source followed (path:func) | class | header today | action |
|---|---|---|---|---|
| `crypto/internal/boring/bcache/cache.cs` | crypto/internal/boring/bcache/cache.go:Cache.Clear;crypto/internal/boring/bcache/cache.go:Cache.Get;crypto/internal/boring/bcache/cache.go:Cache.Put;crypto/internal/boring/bcache/cache.go:Cache.table;crypto/internal/boring/bcache/cache.go:Cache.Register | derived | go-notice | add provenance block |
| `crypto/internal/boring/sig/sig_impl.cs` | implements bodyless crypto/internal/boring/sig/sig.go:BoringCrypto;crypto/internal/boring/sig/sig.go:FIPSOnly;crypto/internal/boring/sig/sig.go:StandardCrypto | mixed | go2cs-notice | add Go notice + provenance block |
| `crypto/internal/constanttime/constant_time_impl.cs` | crypto/internal/constanttime/constant_time.go:boolToUint8 | inspired | go2cs-notice | add Go notice + provenance block |
| `crypto/internal/fips140/alias/alias_impl.cs` | crypto/internal/fips140/alias/alias.go:AnyOverlap | inspired | go2cs-notice | add Go notice + provenance block |
| `crypto/internal/fips140/check/check_impl.cs` | crypto/internal/fips140/check/check.go:init | inspired | go2cs-notice | add Go notice + provenance block |
| `crypto/internal/fips140/nistec/fiat/p256_field_impl.cs` | crypto/internal/fips140/nistec/fiat/p256_fiat64.go:p256Mul;crypto/internal/fips140/nistec/fiat/p256_fiat64.go:p256Square | derived | go2cs-notice | Q2: fiat-crypto notice + BSD-1 disclaimer, then provenance |
| `crypto/internal/fips140/nistec/fiat/p256_impl.cs` | - | own | go2cs-notice | Q2: fiat-crypto notice + BSD-1 disclaimer, then provenance |
| `crypto/internal/fips140/nistec/fiat/p384_impl.cs` | crypto/internal/fips140/nistec/fiat/p384_fiat64.go:p384Mul;crypto/internal/fips140/nistec/fiat/p384_fiat64.go:p384Square | derived | go2cs-notice | Q2: fiat-crypto notice + BSD-1 disclaimer, then provenance |
| `crypto/internal/fips140/nistec/fiat/p521_impl.cs` | crypto/internal/fips140/nistec/fiat/p521_fiat64.go:p521Mul;crypto/internal/fips140/nistec/fiat/p521_fiat64.go:p521Square | derived | go2cs-notice | Q2: fiat-crypto notice + BSD-1 disclaimer, then provenance |
| `crypto/internal/fips140/nistec/p256_impl.cs` | crypto/internal/fips140/nistec/p256.go:init | inspired | go2cs-notice | add Go notice + provenance block |
| `crypto/internal/fips140/sha3/keccakf_impl.cs` | crypto/internal/fips140/sha3/keccakf.go:keccakF1600Generic | derived | go2cs-notice | add Go notice + provenance block |
| `crypto/internal/fips140/subtle/xor_generic.cs` | crypto/internal/fips140/subtle/xor_generic.go:aligned;crypto/internal/fips140/subtle/xor_generic.go:xorBytes;crypto/internal/fips140/subtle/xor_generic.go:xorLoop | derived | go-notice | add provenance block |
| `crypto/rand/rand_impl.cs` | implements bodyless crypto/rand/rand.go:fatal | own (glue) | go2cs-notice | none (go2cs header stays) |
| `crypto/x509/internal/macos/darwin/corefoundation_impl.cs` | implements bodyless crypto/x509/internal/macos/corefoundation.go:syscall | own (glue) | go2cs-notice | none (go2cs header stays) |
| `debug/pe/symbol_impl.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `embed/embed_impl.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `hash/crc32/amd64/crc32_amd64.cs` | hash/crc32/crc32_amd64.go:archInitCastagnoli;hash/crc32/crc32_amd64.go:archInitIEEE;hash/crc32/crc32_amd64.go:archUpdateCastagnoli;hash/crc32/crc32_amd64.go:archUpdateIEEE;hash/crc32/crc32_amd64.go:archAvailableCastagnoli;hash/crc32/crc32_amd64.go:archAvailableIEEE | mixed | go-notice | add provenance block |
| `hash/crc32/linux-arm64/crc32_arm64_impl.cs` | implements bodyless hash/crc32/crc32_arm64.go:castagnoliUpdate;hash/crc32/crc32_arm64.go:ieeeUpdate | own (glue) | go2cs-notice | none (go2cs header stays) |
| `internal/abi/funcpc_impl.cs` | implements bodyless internal/abi/funcpc.go:FuncPCABI0;internal/abi/funcpc.go:FuncPCABIInternal | own (glue) | go2cs-notice | none (go2cs header stays) |
| `internal/abi/type_impl.cs` | internal/abi/type.go:Type.ArrayType;internal/abi/type.go:Type.Elem;internal/abi/type.go:Type.FuncType;internal/abi/type.go:Type.InterfaceType;internal/abi/type.go:FuncType.InSlice;internal/abi/type.go:FuncType.OutSlice | mixed | go-notice | add provenance block |
| `internal/buildcfg/darwin/zbootstrap_darwin_impl.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `internal/buildcfg/windows/zbootstrap_windows_impl.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `internal/bytealg/bytealg_impl.cs` | implements bodyless internal/bytealg/bytealg.go:MakeNoZero;internal/bytealg/compare_native.go:Compare;internal/bytealg/compare_native.go:abigen_runtime_cmpstring | own (glue) | go2cs-notice | none (go2cs header stays) |
| `internal/chacha8rand/chacha8_impl.cs` | internal/chacha8rand/chacha8_generic.go:block_generic;internal/chacha8rand/chacha8_generic.go:setup | mixed | go2cs-notice | add Go notice + provenance block |
| `internal/coverage/cfile/emit_impl.cs` | implements bodyless internal/coverage/cfile/emit.go:getCovCounterList | own (glue) | go2cs-notice | none (go2cs header stays) |
| `internal/cpu/amd64/cpu_arm64_process_impl.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `internal/cpu/amd64/cpu_x86_impl.cs` | implements bodyless internal/cpu/cpu_x86.go:getGOAMD64level | own (glue) | go2cs-notice | none (go2cs header stays) |
| `internal/cpu/darwin/cpu_arm64_darwin_impl.cs` | - | derived | go2cs-notice | add Go notice + provenance block |
| `internal/cpu/darwin/cpu_darwin_impl.cs` | implements bodyless internal/cpu/cpu_darwin.go:sysctlbynameBytes | own (glue) | go2cs-notice | none (go2cs header stays) |
| `internal/cpu/linux-amd64/cpu_arm64_process_linux_impl.cs` | - | mixed | go2cs-notice | add Go notice + provenance block |
| `internal/cpu/linux-arm64/cpu_arm64_hwcap_impl.cs` | implements bodyless internal/cpu/cpu_arm64.go:getMIDR | own (glue) | go2cs-notice | none (go2cs header stays) |
| `internal/godebug/godebug.cs` | internal/godebug/godebug.go:Setting.IncNonDefault;internal/godebug/godebug.go:Setting.Value;internal/godebug/godebug.go:Setting.register;internal/godebug/godebug.go:init;internal/godebug/godebug.go:New;internal/godebug/godebug.go:Setting.Name | derived | go-notice | add provenance block |
| `internal/poll/darwin/runtime_netpoll_impl.cs` | implements bodyless internal/poll/fd_poll_runtime.go:runtimeNano;internal/poll/fd_poll_runtime.go:runtime_isPollServerDescriptor;internal/poll/fd_poll_runtime.go:runtime_pollClose | own (glue) | go2cs-notice | none (go2cs header stays) |
| `internal/poll/deadline_prepay_impl.cs` | internal/poll/fd_poll_runtime.go:pollDesc.prepare | mixed | go2cs-notice | add Go notice + provenance block |
| `internal/poll/fd_mutex_impl.cs` | internal/poll/fd_mutex.go:fdMutex.increfAndClose;internal/poll/fd_mutex.go:fdMutex.rwlock;internal/poll/fd_mutex.go:fdMutex.rwunlock | derived | go2cs-notice | add Go notice + provenance block |
| `internal/poll/linux/fd_writev_unix.cs` | - | derived | go-notice | add provenance block |
| `internal/poll/linux/runtime_netpoll_impl.cs` | implements bodyless internal/poll/fd_poll_runtime.go:runtimeNano;internal/poll/fd_poll_runtime.go:runtime_isPollServerDescriptor;internal/poll/fd_poll_runtime.go:runtime_pollClose | own (glue) | go2cs-notice | none (go2cs header stays) |
| `internal/poll/runtime_sema_impl.cs` | implements bodyless internal/poll/fd_mutex.go:runtime_Semacquire;internal/poll/fd_mutex.go:runtime_Semrelease | own (glue) | go2cs-notice | none (go2cs header stays) |
| `internal/poll/windows/deadline_prepay_windows_impl.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `internal/poll/windows/fd_windows_impl.cs` | internal/poll/fd_windows.go:FD.DisassociateIOCP;internal/poll/fd_windows.go:rawToSockaddrInet4;internal/poll/fd_windows.go:rawToSockaddrInet6 | derived | go2cs-notice | add Go notice + provenance block |
| `internal/poll/windows/runtime_netpoll_impl.cs` | implements bodyless internal/poll/fd_poll_runtime.go:runtimeNano;internal/poll/fd_poll_runtime.go:runtime_isPollServerDescriptor;internal/poll/fd_poll_runtime.go:runtime_pollClose | own (glue) | go2cs-notice | none (go2cs header stays) |
| `internal/reflectlite/swapper_impl.cs` | internal/reflectlite/swapper.go:Swapper | derived | go-notice | add provenance block |
| `internal/reflectlite/type_impl.cs` | internal/reflectlite/type.go:haveIdenticalUnderlyingType;internal/reflectlite/type.go:rtype.Implements;internal/reflectlite/type.go:implements;internal/reflectlite/type.go:rtype.Elem | mixed | go2cs-notice | add Go notice + provenance block |
| `internal/reflectlite/value_impl.cs` | internal/reflectlite/value.go:Value.Elem;internal/reflectlite/value.go:ValueOf;internal/reflectlite/value.go:valueInterface;internal/reflectlite/type.go:rtype.Len;internal/reflectlite/value.go:Value.IsNil | mixed | go2cs-notice | add Go notice + provenance block |
| `internal/routebsd/darwin/sys_impl.cs` | internal/routebsd/sys.go:init | derived | go2cs-notice | add Go notice + provenance block |
| `internal/runtime/atomic/atomic_impl.cs` | internal/runtime/atomic/atomic_amd64.go:Load;internal/runtime/atomic/atomic_amd64.go:Load64 | mixed | go2cs-notice | add Go notice + provenance block |
| `internal/runtime/cgroup/runtime_impl.cs` | implements bodyless internal/runtime/cgroup/runtime.go:throw | own (glue) | go2cs-notice | none (go2cs header stays) |
| `internal/runtime/maps/runtime_impl.cs` | implements bodyless internal/runtime/maps/map.go:typeString;internal/runtime/maps/runtime.go:fatal;internal/runtime/maps/runtime.go:newarray | own (glue) | go2cs-notice | none (go2cs header stays) |
| `internal/runtime/startlinetest/asmfunc_impl.cs` | implements bodyless internal/runtime/startlinetest/func_amd64.go:AsmFunc | own (glue) | go2cs-notice | none (go2cs header stays) |
| `internal/runtime/sys/intrinsics_impl.cs` | implements bodyless internal/runtime/sys/intrinsics.go:GetCallerPC;internal/runtime/sys/intrinsics.go:GetCallerSP | own (glue) | go2cs-notice | none (go2cs header stays) |
| `internal/runtime/sys/linux-arm64/dit_arm64_impl.cs` | implements bodyless internal/runtime/sys/dit_arm64.go:DITEnabled;internal/runtime/sys/dit_arm64.go:DisableDIT;internal/runtime/sys/dit_arm64.go:EnableDIT | own (glue) | go2cs-notice | none (go2cs header stays) |
| `internal/runtime/syscall/linux/linux/syscall_linux_impl.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `internal/sync/hashtriemap.cs` | internal/sync/hashtriemap.go:HashTrieMap.Store;internal/sync/hashtriemap.go:HashTrieMap.All;internal/sync/hashtriemap.go:HashTrieMap.Clear | mixed | go-notice | add provenance block |
| `internal/sync/runtime_impl.cs` | implements bodyless internal/sync/runtime.go:fatal;internal/sync/runtime.go:runtime_SemacquireMutex;internal/sync/runtime.go:runtime_Semrelease | own (glue) | go2cs-notice | none (go2cs header stays) |
| `internal/synctest/synctest_impl.cs` | internal/synctest/synctest.go:Bubble.Run | mixed | go2cs-notice | add Go notice + provenance block |
| `internal/syscall/unix/darwin/net_darwin_impl.cs` | internal/syscall/unix/net_darwin.go:Freeaddrinfo | mixed | go2cs-notice | add Go notice + provenance block |
| `internal/syscall/unix/darwin/user_darwin_impl.cs` | internal/syscall/unix/user_darwin.go:Getgrgid;internal/syscall/unix/user_darwin.go:Getgrnam;internal/syscall/unix/user_darwin.go:Getpwnam;internal/syscall/unix/user_darwin.go:Getpwuid | mixed | go2cs-notice | add Go notice + provenance block |
| `internal/syscall/unix/linux/net_linux_impl.cs` | implements bodyless internal/syscall/unix/net.go:SendtoInet4;internal/syscall/unix/net.go:SendtoInet6 | own (glue) | go2cs-notice | none (go2cs header stays) |
| `internal/syscall/unix/linux/siginfo_linux.cs` | internal/syscall/unix/siginfo_linux.go:SiginfoChild.WaitStatus | derived | go2cs-notice | add Go notice + provenance block |
| `internal/syscall/windows/exec_windows_test.cs` | internal/syscall/windows/exec_windows_test.go:TestRunAtLowIntegrity | mixed | go-notice | add provenance block |
| `internal/syscall/windows/registry/registry_test.cs` | internal/syscall/windows/registry/registry_test.go:TestCreateOpenDeleteKey;internal/syscall/windows/registry/registry_test.go:TestExpandString;internal/syscall/windows/registry/registry_test.go:TestGetMUIStringValue;internal/syscall/windows/registry/registry_test.go:TestInvalidValues | derived | go-notice | add provenance block |
| `internal/syscall/windows/registry/windows/value.cs` | internal/syscall/windows/registry/value.go:Key.DeleteValue;internal/syscall/windows/registry/value.go:Key.SetBinaryValue;internal/syscall/windows/registry/value.go:Key.SetExpandStringValue;internal/syscall/windows/registry/value.go:Key.SetStringValue;internal/syscall/windows/registry/value.go:Key.SetDWordValue;internal/syscall/windows/registry/value.go:Key.SetQWordValue | derived | go-notice | add provenance block |
| `internal/syscall/windows/windows/deadline_prepay_windows_impl.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `internal/syscall/windows/windows/net_windows_impl.cs` | implements bodyless internal/syscall/windows/net_windows.go:WSASendtoInet4;internal/syscall/windows/net_windows.go:WSASendtoInet6 | own (glue) | go2cs-notice | none (go2cs header stays) |
| `internal/syscall/windows/windows/security_windows_impl.cs` | internal/syscall/windows/security_windows.go:GetSidIdentifierAuthority;internal/syscall/windows/security_windows.go:TOKEN_GROUPS.AllGroups | mixed | go2cs-notice | add Go notice + provenance block |
| `internal/syscall/windows/windows/syscall_windows_impl.cs` | internal/syscall/windows/syscall_windows.go:WSARecvMsg;internal/syscall/windows/syscall_windows.go:WSASendMsg;internal/syscall/windows/syscall_windows.go:loadWSASendRecvMsg | derived | go2cs-notice | add Go notice + provenance block |
| `internal/syscall/windows/windows/zsyscall_windows_impl.cs` | internal/syscall/windows/zsyscall_windows.go:NetShareAdd | derived | go2cs-notice | Q1: notice of the //sys-declaring Go file, then provenance |
| `internal/syscall/windows/windows/zsyscall_windows_module_impl.cs` | internal/syscall/windows/zsyscall_windows.go:Module32First;internal/syscall/windows/zsyscall_windows.go:Module32Next | mixed | go2cs-notice | Q1: notice of the //sys-declaring Go file, then provenance |
| `internal/syscall/windows/windows/zsyscall_windows_ntfile_impl.cs` | internal/syscall/windows/zsyscall_windows.go:NtCreateFile;internal/syscall/windows/zsyscall_windows.go:NtOpenFile | derived | go2cs-notice | Q1: notice of the //sys-declaring Go file, then provenance |
| `internal/syscall/windows/windows/zsyscall_windows_privilege_impl.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `internal/syscall/windows/windows/zsyscall_windows_ptrout_impl.cs` | internal/syscall/windows/zsyscall_windows.go:NetUserGetLocalGroups | derived | go2cs-notice | Q1: notice of the //sys-declaring Go file, then provenance |
| `internal/syscall/windows/windows/zsyscall_windows_version_impl.cs` | internal/syscall/windows/zsyscall_windows.go:rtlGetVersion | derived | go2cs-notice | Q1: notice of the //sys-declaring Go file, then provenance |
| `internal/syscall/windows/windows/zsyscall_windows_wsa_impl.cs` | internal/syscall/windows/zsyscall_windows.go:GetOverlappedResult;internal/syscall/windows/zsyscall_windows.go:WSAGetOverlappedResult | derived | go2cs-notice | Q1: notice of the //sys-declaring Go file, then provenance |
| `internal/testenv/testenv_impl.cs` | internal/testenv/testenv.go:CPUProfilingBroken | inspired | go2cs-notice | add Go notice + provenance block |
| `iter/iter_impl.cs` | implements bodyless iter/iter.go:coroswitch;iter/iter.go:newcoro | own (glue) | go2cs-notice | none (go2cs header stays) |
| `math/amd64/hypot_asm_impl.cs` | implements bodyless math/hypot_asm.go:archHypot | own (glue) | go2cs-notice | none (go2cs header stays) |
| `math/amd64/log_asm_impl.cs` | implements bodyless math/log_asm.go:archLog | own (glue) | go2cs-notice | none (go2cs header stays) |
| `math/bits/bits_impl.cs` | - | derived | go2cs-notice | add Go notice + provenance block |
| `math/linux-arm64/exp2_asm_impl.cs` | implements bodyless math/exp2_asm.go:archExp2 | own (glue) | go2cs-notice | none (go2cs header stays) |
| `math/math_impl.cs` | implements bodyless math/dim_asm.go:archMax;math/dim_asm.go:archMin;math/exp_asm.go:archExp | own (glue) | go2cs-notice | none (go2cs header stays) |
| `math/rand/rand_impl.cs` | implements bodyless math/rand/rand.go:runtime_rand | own (glue) | go2cs-notice | none (go2cs header stays) |
| `math/rand/v2/rand_impl.cs` | implements bodyless math/rand/v2/rand.go:runtime_rand | own (glue) | go2cs-notice | none (go2cs header stays) |
| `net/dnsclient_impl.cs` | implements bodyless net/dnsclient.go:runtime_rand | own (glue) | go2cs-notice | none (go2cs header stays) |
| `net/windows/interface_windows_impl.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `net/windows/lookup_windows.cs` | net/lookup_windows.go:resolveCNAME;net/lookup_windows.go:validRecs;net/lookup_windows.go:winError;net/lookup_windows.go:concurrentThreadsLimit | derived | go-notice | add provenance block |
| `os/darwin/dir_darwin_impl.cs` | - | mixed | go-notice | add provenance block |
| `os/deadline_prepay_impl.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `os/linux/pidfd_linux_impl.cs` | implements bodyless os/pidfd_linux.go:checkClonePidfd | own (glue) | go2cs-notice | none (go2cs header stays) |
| `os/linux/wait_waitid.cs` | - | derived | go-notice | add provenance block |
| `os/proc_impl.cs` | implements bodyless os/proc.go:runtime_beforeExit | own (glue) | go2cs-notice | none (go2cs header stays) |
| `os/tempfile_impl.cs` | implements bodyless os/tempfile.go:runtime_rand | own (glue) | go2cs-notice | none (go2cs header stays) |
| `os/user/windows/lookup_windows_impl.cs` | - | mixed | go2cs-notice | add Go notice + provenance block |
| `os/windows/dir_windows_impl.cs` | - | mixed | go-notice | add provenance block |
| `os/windows/exec_windows_impl.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `os/windows/file_windows_impl.cs` | - | mixed | go-notice | add provenance block |
| `reflect/abi_impl.cs` | reflect/abi.go:abiSeq.regAssign;reflect/type.go:addTypeBits | derived | go-notice | add provenance block |
| `reflect/deepequal_impl.cs` | reflect/deepequal.go:DeepEqual;reflect/deepequal.go:deepValueEqual | mixed | go-notice | add provenance block |
| `reflect/makefunc_impl.cs` | reflect/makefunc.go:MakeFunc | mixed | go-notice | add provenance block |
| `reflect/value_impl.cs` | reflect/map.go:MapIter.Reset;reflect/map.go:Value.MapRange;reflect/type.go:TypeFor;reflect/type.go:haveIdenticalUnderlyingType;reflect/map.go:MapIter.Key;reflect/map.go:MapIter.Next | mixed | go-notice+provenance | none |
| `runtime/amd64/cputicks_impl.cs` | implements bodyless runtime/cputicks.go:cputicks | own (glue) | go2cs-notice | none (go2cs header stays) |
| `runtime/amd64/test_amd64_impl.cs` | implements bodyless runtime/test_amd64.go:testSPWrite | own (glue) | go2cs-notice | none (go2cs header stays) |
| `runtime/arena_impl.cs` | runtime/arena.go:arena_heapify;runtime/arena.go:newUserArena | mixed | go2cs-notice | add Go notice + provenance block |
| `runtime/cpuflags_impl.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `runtime/cpusampler_impl.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `runtime/darwin/cpuprof_darwin_impl.cs` | runtime/os_darwin.go:setProcessCPUProfiler;runtime/os_darwin.go:setThreadCPUProfiler | inspired | go2cs-notice | add Go notice + provenance block |
| `runtime/darwin/goarch_darwin_impl.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `runtime/darwin/libccall_impl.cs` | runtime/sys_darwin.go:libc_error_addr;runtime/sys_libc.go:libcCall | mixed | go2cs-notice | add Go notice + provenance block |
| `runtime/darwin/lock_sema_impl.cs` | runtime/lock_sema.go:notetsleep_internal | derived | go-notice | add provenance block |
| `runtime/darwin/nanotime_impl.cs` | runtime/sys_darwin.go:nanotime1 | inspired | go2cs-notice | add Go notice + provenance block |
| `runtime/darwin/netpoll_kqueue_impl.cs` | runtime/netpoll_kqueue.go:netpollBreak | inspired | go2cs-notice | add Go notice + provenance block |
| `runtime/darwin/os_darwin_impl.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `runtime/darwin/sigaction_impl.cs` | runtime/sys_darwin.go:sigaction | mixed | go2cs-notice | add Go notice + provenance block |
| `runtime/darwin/signal_posix_darwin_impl.cs` | runtime/signal_unix.go:sigdisable;runtime/signal_unix.go:sigenable;runtime/signal_unix.go:sigignore | mixed | go-notice | add provenance block |
| `runtime/darwin/sigprocmask_impl.cs` | runtime/sys_darwin.go:sigprocmask | inspired | go2cs-notice | add Go notice + provenance block |
| `runtime/darwin/sys_darwin_mmap_impl.cs` | runtime/sys_darwin.go:munmap | mixed | go2cs-notice | add Go notice + provenance block |
| `runtime/darwin/sys_darwin_signote_impl.cs` | runtime/sys_darwin.go:read;runtime/sys_darwin.go:write1 | inspired | go2cs-notice | add Go notice + provenance block |
| `runtime/debug/stubs_impl.cs` | implements bodyless runtime/debug/garbage.go:SetTraceback;runtime/debug/garbage.go:WriteHeapDump;runtime/debug/mod.go:modinfo | own (glue) | go2cs-notice | none (go2cs header stays) |
| `runtime/debuglog_impl.cs` | runtime/debuglog.go:debugLogReader.printVal;runtime/debuglog.go:dlogImpl;runtime/debuglog.go:printDebugLogImpl;runtime/debuglog.go:dloggerImpl.s;runtime/debuglog.go:printDebugLogPC | derived | go2cs-notice | add Go notice + provenance block |
| `runtime/goargs_impl.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `runtime/goenvs_impl.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `runtime/hash_impl.cs` | implements bodyless runtime/alg.go:memhash;runtime/alg.go:memhash32;runtime/alg.go:memhash64 | own (glue) | go2cs-notice | none (go2cs header stays) |
| `runtime/hexdump_impl.cs` | runtime/hexdump.go:hexdumpWords | derived | go2cs-notice | add Go notice + provenance block |
| `runtime/hostofrecord_impl.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `runtime/linux-arm64/timestub2_impl.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `runtime/linux/cpuprof_linux_impl.cs` | runtime/os_linux.go:setProcessCPUProfiler;runtime/os_linux.go:setThreadCPUProfiler | inspired | go2cs-notice | add Go notice + provenance block |
| `runtime/linux/lock_futex_impl.cs` | runtime/lock_futex.go:notetsleep_internal | derived | go-notice | add provenance block |
| `runtime/linux/mem_linux_impl.cs` | implements bodyless runtime/cgo_mmap.go:sysMunmap;runtime/stubs2.go:madvise;runtime/stubs2.go:usleep | own (glue) | go2cs-notice | none (go2cs header stays) |
| `runtime/linux/nanotime_impl.cs` | implements bodyless runtime/stubs3.go:nanotime1 | own (glue) | go2cs-notice | none (go2cs header stays) |
| `runtime/linux/netpoll_epoll_impl.cs` | runtime/netpoll_epoll.go:netpollBreak | inspired | go2cs-notice | add Go notice + provenance block |
| `runtime/linux/os_linux_impl.cs` | implements bodyless runtime/os_linux.go:clone;runtime/os_linux.go:getpid;runtime/os_linux.go:tgkill | own (glue) | go2cs-notice | none (go2cs header stays) |
| `runtime/linux/set_vma_name_linux.cs` | runtime/set_vma_name_linux.go:setVMAName;runtime/set_vma_name_linux.go:setVMANameSupported | derived | go-notice | add provenance block |
| `runtime/linux/signal_posix_impl.cs` | runtime/signal_unix.go:dieFromSignal;runtime/signal_unix.go:sigdisable;runtime/signal_unix.go:sigenable;runtime/signal_unix.go:sigignore | mixed | go-notice | add provenance block |
| `runtime/linux/sigprocmask_impl.cs` | implements bodyless runtime/os_linux.go:rtsigprocmask | own (glue) | go2cs-notice | none (go2cs header stays) |
| `runtime/linux/stubs2_impl.cs` | runtime/profbuf.go:profBuf.read | mixed | go2cs-notice | add Go notice + provenance block |
| `runtime/lock_managed_impl.cs` | runtime/lock_futex.go:notetsleepg;runtime/lock_sema.go:notesleep | mixed | go-notice | add provenance block |
| `runtime/malloc_impl.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `runtime/managed_impl.cs` | runtime/extern.go:Callers;runtime/heapdump.go:runtime_debug_WriteHeapDump;runtime/histogram.go:timeHistogram.record;runtime/mstats.go:ReadMemStats;runtime/alg.go:efaceHash;runtime/alg.go:ifaceHash | mixed | go2cs-notice | add Go notice + provenance block |
| `runtime/mbitmap_impl.cs` | runtime/mbitmap.go:pointerMask | inspired | go2cs-notice | add Go notice + provenance block |
| `runtime/mcleanup.cs` | runtime/arena.go:writeUserArenaHeapBits.flush;runtime/export_test.go:AddrRanges.Add | mixed | go-notice | add provenance block |
| `runtime/mem_persistent_impl.cs` | runtime/malloc.go:inPersistentAlloc;runtime/malloc.go:persistentalloc1 | mixed | go2cs-notice | add Go notice + provenance block |
| `runtime/memmove_impl.cs` | implements bodyless runtime/stubs.go:memclrNoHeapPointers;runtime/stubs.go:memequal;runtime/stubs.go:memmove | own (glue) | go2cs-notice | none (go2cs header stays) |
| `runtime/metrics/sample.cs` | runtime/metrics/sample.go:Read | mixed | go-notice | add provenance block |
| `runtime/metrics_impl.cs` | runtime/metrics.go:schedStatsAggregate.compute | derived | go2cs-notice | add Go notice + provenance block |
| `runtime/mfinal.cs` | runtime/mfinal.go:finalizercommit;runtime/mfinal.go:isGoPointerWithoutSpan;runtime/mfinal.go:iterate_finq;runtime/mfinal.go:lockRankMayQueueFinalizer;runtime/export_test.go:SemTable.Enqueue;runtime/mfinal.go:KeepAlive | mixed | go-notice | add provenance block |
| `runtime/mgc_impl.cs` | runtime/mgc.go:gcTestIsReachable | inspired | go2cs-notice | add Go notice + provenance block |
| `runtime/mprof_impl.cs` | runtime/mprof.go:newBucket;runtime/mprof.go:stkbucket;runtime/mprof.go:bucket.bp;runtime/mprof.go:bucket.mp | mixed | go2cs-notice | add Go notice + provenance block |
| `runtime/mranges_impl.cs` | runtime/mranges.go:addrRanges.add;runtime/mranges.go:addrRanges.cloneInto;runtime/mranges.go:addrRanges.init | derived | go2cs-notice | add Go notice + provenance block |
| `runtime/netpoll_impl.cs` | runtime/netpoll.go:netpollGenericInit | inspired | go2cs-notice | add Go notice + provenance block |
| `runtime/panic_impl.cs` | runtime/panic.go:fatal;runtime/panic.go:throw | inspired | go2cs-notice | add Go notice + provenance block |
| `runtime/panicvalues_impl.cs` | - | derived | go-notice | add provenance block |
| `runtime/park_impl.cs` | runtime/proc.go:gopark;runtime/proc.go:injectglist | mixed | go2cs-notice | add Go notice + provenance block |
| `runtime/pinner_impl.cs` | runtime/cgocall.go:cgoCheckPointer;runtime/export_test.go:TimeHistogram.Count | mixed | go2cs-notice | add Go notice + provenance block |
| `runtime/pprof/darwin/proto_darwin_impl.cs` | runtime/pprof/proto_darwin.go:profileBuilder.readMapping | derived | go2cs-notice | add Go notice + provenance block |
| `runtime/pprof/linux/proto_other_impl.cs` | runtime/pprof/proto_other.go:profileBuilder.readMapping | derived | go2cs-notice | add Go notice + provenance block |
| `runtime/pprof/proflabel_impl.cs` | implements bodyless runtime/pprof/runtime.go:runtime_getProfLabel;runtime/pprof/runtime.go:runtime_setProfLabel | own (glue) | go2cs-notice | none (go2cs header stays) |
| `runtime/pprof/symtab_impl.cs` | implements bodyless runtime/pprof/runtime.go:runtime_FrameStartLine;runtime/pprof/runtime.go:runtime_FrameSymbolName;runtime/pprof/runtime.go:runtime_expandFinalInlineFrame | own (glue) | go2cs-notice | none (go2cs header stays) |
| `runtime/pprof/windows/proto_windows_impl.cs` | runtime/pprof/proto_windows.go:profileBuilder.readMapping | derived | go2cs-notice | add Go notice + provenance block |
| `runtime/print_impl.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `runtime/profbuf_impl.cs` | runtime/profbuf.go:profBuf.write | derived | go2cs-notice | add Go notice + provenance block |
| `runtime/runtime2.cs` | runtime/runtime2.go:getcallerfp;runtime/runtime2.go:maybeTraceableChan.get;runtime/runtime2.go:maybeTraceableChan.set;runtime/runtime2.go:newMWeakPointer;runtime/mgcsweep.go:sweepClass.clear;runtime/mheap.go:mSpanStateBox.get | derived | go-notice | add provenance block |
| `runtime/runtime2_impl.cs` | runtime/proc.go:runqempty;runtime/debuglog.go:dloggerImpl.hex;runtime/export_test.go:AddrRange.Equals | mixed | go-notice | add provenance block |
| `runtime/sema_impl.cs` | runtime/sema.go:semacquire1;runtime/sema.go:semrelease1 | inspired | go2cs-notice | add Go notice + provenance block |
| `runtime/stubs_impl.cs` | implements bodyless runtime/stubs.go:breakpoint;runtime/stubs.go:getg;runtime/stubs.go:procyieldAsm | own (glue) | go2cs-notice | none (go2cs header stays) |
| `runtime/syscall_managed_impl.cs` | runtime/proc.go:entersyscall;runtime/proc.go:entersyscallblock | mixed | go2cs-notice | add Go notice + provenance block |
| `runtime/trace/annotation_impl.cs` | implements bodyless runtime/trace/annotation.go:userLog;runtime/trace/annotation.go:userRegion;runtime/trace/annotation.go:userTaskCreate | own (glue) | go2cs-notice | none (go2cs header stays) |
| `runtime/trace/flightrecorder_impl.cs` | implements bodyless runtime/trace/flightrecorder.go:runtime_traceAdvance;runtime/trace/recorder.go:runtime_traceClockNow;runtime/trace/subscribe.go:runtime_readTrace | own (glue) | go2cs-notice | none (go2cs header stays) |
| `runtime/trace_impl.cs` | runtime/trace.go:ReadTrace;runtime/trace.go:StartTrace | derived | go-notice | add provenance block |
| `runtime/tracemap_impl.cs` | runtime/tracemap.go:traceMap.newTraceMapNode | inspired | go2cs-notice | add Go notice + provenance block |
| `runtime/typestring_impl.cs` | runtime/type.go:rtype.string | derived | go2cs-notice | add Go notice + provenance block |
| `runtime/windows/cpuprof_windows_impl.cs` | runtime/os_windows.go:setProcessCPUProfiler;runtime/os_windows.go:setThreadCPUProfiler | inspired | go2cs-notice | add Go notice + provenance block |
| `runtime/windows/goarch_windows_impl.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `runtime/windows/lock_sema_impl.cs` | runtime/lock_sema.go:notetsleep_internal | derived | go-notice | add provenance block |
| `runtime/windows/mem_windows_impl.cs` | runtime/mem_windows.go:sysAllocOS;runtime/mem_windows.go:sysReserveOS;runtime/mem_windows.go:sysUsedOS;runtime/mem_windows.go:sysFreeOS;runtime/mem_windows.go:sysUnusedOS | derived | go2cs-notice | add Go notice + provenance block |
| `runtime/windows/nanotime_impl.cs` | implements bodyless runtime/stubs3.go:nanotime1 | own (glue) | go2cs-notice | none (go2cs header stays) |
| `runtime/windows/netpoll_windows_impl.cs` | runtime/netpoll_windows.go:netpollBreak | inspired | go2cs-notice | add Go notice + provenance block |
| `runtime/windows/os_windows_impl.cs` | runtime/os_windows.go:getCPUCount;runtime/os_windows.go:windows_QueryPerformanceCounter | mixed | go2cs-notice | add Go notice + provenance block |
| `runtime/windows/signal_windows_impl.cs` | runtime/signal_windows.go:enableWER | mixed | go2cs-notice | add Go notice + provenance block |
| `runtime/windows/usleep_windows_impl.cs` | runtime/os_windows.go:usleep | inspired | go2cs-notice | add Go notice + provenance block |
| `slices/slices_impl.cs` | slices/slices.go:overlaps | inspired | go2cs-notice | add Go notice + provenance block |
| `sync/atomic/doc_impl.cs` | implements bodyless sync/atomic/doc.go:AddInt32;sync/atomic/doc.go:AddUint32;sync/atomic/doc.go:AddUintptr | own (glue) | go2cs-notice | none (go2cs header stays) |
| `sync/atomic/type.cs` | sync/atomic/type.go:Bool.CompareAndSwap;sync/atomic/type.go:Int32.CompareAndSwap;sync/atomic/type.go:b32 | mixed | go-notice | add provenance block |
| `sync/atomic/value.cs` | sync/atomic/type.go:Int32.CompareAndSwap;sync/atomic/value.go:Value.Load | derived | go-notice | add provenance block |
| `sync/cond_impl.cs` | sync/cond.go:copyChecker.check | inspired | go2cs-notice | add Go notice + provenance block |
| `sync/mutex.cs` | sync/mutex.go:Mutex.Lock;sync/mutex.go:Mutex.TryLock | mixed | go-notice | add provenance block |
| `sync/once.cs` | sync/once.go:Once.Do;sync/once.go:Once.doSlow | derived | go-notice | add provenance block |
| `sync/oncefunc.cs` | sync/oncefunc.go:OnceValue;sync/oncefunc.go:OnceFunc | derived | go-notice | add provenance block |
| `sync/pool.cs` | sync/pool.go:poolCleanup;sync/mutex_test.go:init;sync/pool.go:Pool.Get | derived | go-notice | add provenance block |
| `sync/poolqueue.cs` | sync/poolqueue.go:poolChain.pushHead;sync/poolqueue.go:poolDequeue.pack;sync/poolqueue.go:poolDequeue.pushHead | derived | go-notice | add provenance block |
| `sync/runtime_impl.cs` | implements bodyless sync/pool.go:runtime_LoadAcquintptr;sync/pool.go:runtime_StoreReluintptr;sync/pool.go:runtime_procPin | own (glue) | go2cs-notice | none (go2cs header stays) |
| `sync/waitgroup.cs` | sync/waitgroup.go:WaitGroup.Add;sync/waitgroup.go:WaitGroup.Done | mixed | go-notice | add provenance block |
| `syscall/darwin/exec_libc2_impl.cs` | syscall/exec_unix.go:Exec;syscall/zsyscall_darwin_amd64.go:pipe | mixed | go2cs-notice | add Go notice + provenance block |
| `syscall/darwin/getdirentries_darwin_impl.cs` | - | derived | go2cs-notice | add Go notice + provenance block |
| `syscall/darwin/sockaddr_darwin_impl.cs` | syscall/syscall_unix.go:Bind;syscall/syscall_unix.go:Connect | mixed | go2cs-notice | add Go notice + provenance block |
| `syscall/darwin/syscall_darwin_impl.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `syscall/darwin/writev_darwin_impl.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `syscall/linux-amd64/syscall_linux_amd64_impl.cs` | implements bodyless syscall/syscall_linux_amd64.go:gettimeofday | own (glue) | go2cs-notice | none (go2cs header stays) |
| `syscall/linux-amd64/zsyscall_linux_amd64_impl.cs` | syscall/zsyscall_linux_amd64.go:Fstat;syscall/zsyscall_linux_amd64.go:fstatat | mixed | go2cs-notice | Q1: notice of the //sys-declaring Go file, then provenance |
| `syscall/linux-arm64/zsyscall_linux_arm64_impl.cs` | syscall/zsyscall_linux_arm64.go:Fstat;syscall/zsyscall_linux_arm64.go:fstatat | mixed | go2cs-notice | Q1: notice of the //sys-declaring Go file, then provenance |
| `syscall/linux/cgocaller_linux_impl.cs` | syscall/syscall_linux.go:Setgroups | mixed | go2cs-notice | add Go notice + provenance block |
| `syscall/linux/exec_unix.cs` | syscall/exec_unix.go:Exec;syscall/exec_unix.go:SetNonblock;syscall/exec_unix.go:StringSlicePtr | mixed | go-notice | add provenance block |
| `syscall/linux/sockaddr_linux_impl.cs` | syscall/syscall_unix.go:Bind;syscall/syscall_unix.go:Connect | mixed | go2cs-notice | add Go notice + provenance block |
| `syscall/linux/structclass_linux_impl.cs` | syscall/flock_linux.go:FcntlFlock;syscall/zsyscall_linux_amd64.go:Fstatfs;syscall/zsyscall_linux_amd64.go:Statfs;syscall/zsyscall_linux_amd64.go:Sysinfo;syscall/syscall_linux.go:SetsockoptIPMreqn;syscall/syscall_unix.go:SetsockoptICMPv6Filter | mixed | go2cs-notice | add Go notice + provenance block |
| `syscall/linux/syscall_linux_impl.cs` | implements bodyless syscall/syscall_linux.go:runtime_entersyscall;syscall/syscall_linux.go:runtime_exitsyscall | own (glue) | go2cs-notice | none (go2cs header stays) |
| `syscall/syscall_impl.cs` | implements bodyless syscall/syscall.go:Exit;syscall/syscall.go:Getpagesize;syscall/syscall.go:runtimeClearenv | own (glue) | go2cs-notice | none (go2cs header stays) |
| `syscall/windows/deadline_prepay_windows_impl.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `syscall/windows/dll_windows.cs` | syscall/dll_windows.go:DLL.MustFindProc;syscall/dll_windows.go:LazyDLL.Handle;syscall/dll_windows.go:LazyDLL.Load;syscall/dll_windows.go:LazyDLL.NewProc;syscall/dll_windows.go:DLL.Release;syscall/dll_windows.go:Proc.Addr | mixed | go-notice | add provenance block |
| `syscall/windows/exec_windows.cs` | syscall/exec_windows.go:CloseOnExec;syscall/exec_windows.go:appendEscapeArg;syscall/exec_windows.go:envSorted;syscall/exec_windows.go:makeCmdLine;syscall/exec_windows.go:EscapeArg;syscall/exec_windows.go:Exec | derived | go-notice | add provenance block |
| `syscall/windows/security_windows.cs` | syscall/security_windows.go:SID.Len;syscall/security_windows.go:Token.Close | mixed | go-notice | add provenance block |
| `syscall/windows/syscall_windows_callback_impl.cs` | syscall/syscall_windows.go:Bind | mixed | go2cs-notice | add Go notice + provenance block |
| `syscall/windows/syscall_windows_impl.cs` | syscall/syscall_windows.go:ConnectEx;syscall/syscall_windows.go:SetsockoptIPMreq;syscall/syscall_windows.go:SetsockoptIPv6Mreq;syscall/syscall_windows.go:Bind;syscall/syscall_windows.go:Connect | mixed | go2cs-notice | add Go notice + provenance block |
| `syscall/windows/zsyscall_windows_addrinfo_impl.cs` | syscall/zsyscall_windows.go:GetAddrInfoW;syscall/zsyscall_windows.go:FreeAddrInfoW | mixed | go2cs-notice | Q1: notice of the //sys-declaring Go file, then provenance |
| `syscall/windows/zsyscall_windows_certchain_impl.cs` | syscall/zsyscall_windows.go:CertAddCertificateContextToStore;syscall/zsyscall_windows.go:CertGetCertificateChain;syscall/zsyscall_windows.go:CertVerifyCertificateChainPolicy;syscall/zsyscall_windows.go:CertFreeCertificateChain;syscall/zsyscall_windows.go:CertFreeCertificateContext | mixed | go2cs-notice | Q1: notice of the //sys-declaring Go file, then provenance |
| `syscall/windows/zsyscall_windows_cmdline_impl.cs` | syscall/zsyscall_windows.go:GetCommandLine | mixed | go2cs-notice | Q1: notice of the //sys-declaring Go file, then provenance |
| `syscall/windows/zsyscall_windows_dnsrecord_impl.cs` | syscall/zsyscall_windows.go:_DnsQuery;syscall/zsyscall_windows.go:DnsRecordListFree | mixed | go2cs-notice | Q1: notice of the //sys-declaring Go file, then provenance |
| `syscall/windows/zsyscall_windows_file_impl.cs` | syscall/zsyscall_windows.go:readFile;syscall/zsyscall_windows.go:writeFile | inspired | go2cs-notice | Q1: notice of the //sys-declaring Go file, then provenance |
| `syscall/windows/zsyscall_windows_impl.cs` | syscall/zsyscall_windows.go:Process32First;syscall/zsyscall_windows.go:Process32Next | mixed | go2cs-notice | Q1: notice of the //sys-declaring Go file, then provenance |
| `syscall/windows/zsyscall_windows_netdb_impl.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `syscall/windows/zsyscall_windows_ptrout_impl.cs` | syscall/zsyscall_windows.go:ConvertSidToStringSid;syscall/zsyscall_windows.go:ConvertStringSidToSid;syscall/zsyscall_windows.go:NetGetJoinInformation;syscall/zsyscall_windows.go:NetUserGetInfo | derived | go2cs-notice | Q1: notice of the //sys-declaring Go file, then provenance |
| `syscall/windows/zsyscall_windows_startupinfo_impl.cs` | syscall/zsyscall_windows.go:getStartupInfo | derived | go2cs-notice | Q1: notice of the //sys-declaring Go file, then provenance |
| `syscall/windows/zsyscall_windows_wsa_impl.cs` | syscall/syscall_windows.go:LoadConnectEx;syscall/syscall_windows.go:WSASendto;syscall/zsyscall_windows.go:AcceptEx;syscall/zsyscall_windows.go:CancelIoEx | mixed | go2cs-notice | add Go notice + provenance block |
| `testing/ExportTest.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `testing/HostDescriptorLimit.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `testing/LongPathTree.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `testing/PackageAncestry.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `testing/TestExecution.cs` | testing/testing.go:T.Chdir;testing/fuzz.go:F.Fail;testing/fuzz.go:F.Helper | mixed | go2cs-notice | add Go notice + provenance block |
| `testing/TestFlagBridge.cs` | testing/benchmark.go:durationOrCountFlag.String;testing/testing.go:chattyFlag.Get | mixed | go2cs-notice | add Go notice + provenance block |
| `testing/TestFormat.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `testing/TestHost.cs` | testing/testing.go:M.Run;testing/testing.go:RunTests | mixed | go2cs-notice | add Go notice + provenance block |
| `testing/TestLogBridge.cs` | testing/fuzz.go:F.Add;testing/testing.go:T.Chdir | mixed | go2cs-notice | add Go notice + provenance block |
| `testing/TestOptions.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `testing/TestRegistry.cs` | testing/fuzz.go:F.Add | mixed | go2cs-notice | add Go notice + provenance block |
| `testing/TestReporter.cs` | - | own | go2cs-notice | none (go2cs header stays) |
| `testing/TestRunner.cs` | testing/sub_test.go:funcWriter.Write;testing/testing.go:RunTests | mixed | go2cs-notice | add Go notice + provenance block |
| `testing/testing.cs` | testing/testing.go:T.Chdir;testing/testing.go:common.Error;testing/testing.go:common.Errorf;testing/testing.go:common.Fatal;testing/allocs.go:AllocsPerRun;testing/benchmark.go:B.Elapsed | derived | go2cs-notice | add Go notice + provenance block |
| `time/sleep_impl.cs` | time/sleep.go:syncTimer | inspired | go2cs-notice | add Go notice + provenance block |
| `time/tick.cs` | time/tick.go:NewTicker;time/tick.go:Tick | derived | go-notice | add provenance block |
| `time/time_impl.cs` | implements bodyless time/sleep.go:Sleep;time/sleep.go:newTimer;time/sleep.go:resetTimer | own (glue) | go2cs-notice | none (go2cs header stays) |
| `unique/canonmap_impl.cs` | implements bodyless unique/canonmap.go:runtime_rand | own (glue) | go2cs-notice | none (go2cs header stays) |
| `unique/clone.cs` | unique/clone.go:buildArrayCloneSeq;unique/clone.go:buildStructCloneSeq;unique/clone.go:makeCloneSeq;unique/clone.go:clone | derived | go-notice | add provenance block |
| `unsafe/unsafe.cs` | implements bodyless unsafe/unsafe.go:Add;unsafe/unsafe.go:Alignof;unsafe/unsafe.go:Offsetof | mixed | go-notice | add provenance block |
| `vendor/golang.org/x/crypto/internal/alias/alias_purego_impl.cs` | vendor/golang.org/x/crypto/internal/alias/alias_purego.go:AnyOverlap | inspired | go2cs-notice | add Go notice + provenance block |
| `weak/pointer.cs` | weak/pointer.go:Make;weak/pointer.go:Pointer.Value | mixed | go-notice | add provenance block |

## golib / go2cs files citing Go

| file | Go files cited | Go funcs matched by name | class | header today | action |
|---|---|---|---|---|---|
| `go2cs/Symbols.cs` | - | - | cites-only | go2cs-mit | Q3 |
| `golib/DynamicSecurityMethodAttribute.cs` | log/slog/logger_test.go | - | cites-only | go2cs-mit | Q3 |
| `golib/GoArchExclusiveAttribute.cs` | internal/abi/abi_amd64.go; internal/abi/abi_arm64.go; internal/goarch/goarch_amd64.go; internal/goarch/goarch_arm64.go | - | cites-only | go2cs-mit | Q3 |
| `golib/GoArrayDimsAttribute.cs` | runtime/vdso_linux.go | - | cites-only | go2cs-mit | Q3 |
| `golib/GoAsyncIO.cs` | net/fd_windows.go | - | cites-only | go2cs-mit | Q3 |
| `golib/GoCgoDynamicImports.cs` | cmd/link/internal/ld/macho.go; debug/macho/macho.go | - | cites-only | go2cs-mit | Q3 |
| `golib/GoDelegateSynthesis.cs` | - | - | cites-only | go2cs-mit | Q3 |
| `golib/GoLibcCall.cs` | runtime/sys_darwin.go | runtime/sys_darwin.go:syscallN_trampoline | inspired | go2cs-mit | Q3 |
| `golib/GoMemProfile.cs` | runtime/malloc.go; runtime/mprof.go | runtime/malloc.go:mallocgc; runtime/malloc.go:nextSample; runtime/malloc.go:profilealloc | inspired | go2cs-mit | Q3 |
| `golib/GoModInfoAttribute.cs` | cmd/go/internal/load/pkg.go; cmd/go/internal/load/test.go; cmd/go/internal/modload/build.go | - | cites-only | go2cs-mit | Q3 |
| `golib/GoPositionMapAttribute.cs` | runtime/debug/stack.go | - | cites-only | go2cs-mit | Q3 |
| `golib/GoPrintText.cs` | runtime/print.go | - | cites-only | go2cs-mit | Q3 |
| `golib/GoReflect.FinalizerBinding.cs` | runtime/mfinal.go | runtime/mfinal.go:SetFinalizer | inspired | none | Q3 |
| `golib/GoReflect.MethodSets.cs` | reflect/value.go | reflect/value.go:Append; reflect/value.go:Indirect; reflect/value.go:Select; reflect/value.go:Value.Close | inspired | go2cs-mit | Q3 |
| `golib/GoReflect.TypeNaming.cs` | go/types/typestring.go | go/types/typestring.go:typeWriter.string | inspired | go2cs-mit | Q3 |
| `golib/GoReflect.ValueMarshalling.cs` | runtime/metrics/value.go; runtime/type.go | runtime/metrics/value.go:Value.Kind; runtime/type.go:rtype.string | inspired | go-notice+go2cs-mit+provenance | Q3 |
| `golib/GoRefusalDowngradedAttribute.cs` | - | - | cites-only | go2cs-mit | Q3 |
| `golib/GoStructSynthesis.cs` | runtime/type.go | runtime/type.go:rtype.name; runtime/type.go:rtype.string | inspired | go2cs-mit | Q3 |
| `golib/NativeStructMarshal.Deep.cs` | os/os_windows_test.go | - | cites-only | go2cs-mit | Q3 |
| `golib/PanicException.cs` | runtime/error.go; runtime/panic.go; runtime/print.go | runtime/panic.go:PanicNilError.Error; runtime/panic.go:preprintpanics; runtime/panic.go:throw | inspired | go2cs-mit | Q3 |
| `golib/builtin.DarwinStdDescriptors.cs` | syscall/zsyscall_darwin_amd64.go; syscall/zsyscall_darwin_arm64.go | - | cites-only | go2cs-mit | Q3 |
| `golib/builtin.WindowsLongPaths.cs` | runtime/os_windows.go | runtime/os_windows.go:initLongPathSupport | inspired | go2cs-mit | Q3 |
| `golib/builtin.cs` | runtime/panic.go; runtime/print.go | runtime/panic.go:fatal; runtime/print.go:bytes; runtime/print.go:printpointer | inspired | go2cs-mit | Q3 |
| `golib/channel.cs` | runtime/chan.go; runtime/select.go | runtime/chan.go:chanrecv; runtime/chan.go:chansend; runtime/chan.go:empty; runtime/chan.go:recv | inspired | go2cs-mit | Q3 |
| `golib/funcArity.cs` | runtime/abi_test.go | - | cites-only | go2cs-mit | Q3 |
| `golib/map.cs` | - | - | cites-only | go2cs-mit | Q3 |
| `golib/runtime/CrashReport.cs` | database/sql/convert.go; runtime/debug/stack_test.go; runtime/iface.go; runtime/panic.go | runtime/panic.go:PanicNilError.Error; runtime/panic.go:printpanics | inspired | go2cs-mit | Q3 |
| `golib/runtime/ExecutionTracer.cs` | internal/trace/tracev2/events.go; runtime/trace.go; runtime/traceruntime.go; runtime/tracetime.go | runtime/trace.go:ReadTrace; runtime/trace.go:StopTrace; runtime/trace.go:traceAdvance; runtime/trace.go:traceAdvancerState.start | inspired | go2cs-mit | Q3 |
| `golib/runtime/GoHighResolutionTimer.cs` | runtime/os_windows.go | - | cites-only | go2cs-mit | Q3 |
| `golib/runtime/Goroutine.cs` | runtime/mgc.go; runtime/proc.go; runtime/traceback.go | runtime/proc.go:gcount; runtime/proc.go:gopark; runtime/proc.go:main; runtime/proc.go:pMask.read | inspired | go2cs-mit | Q3 |
| `golib/runtime/MainExit.cs` | runtime/proc.go | runtime/proc.go:pMask.read; runtime/proc.go:randomEnum.done; runtime/proc.go:randomOrder.reset | inspired | go2cs-mit | Q3 |
| `golib/runtime/MainGoexit.cs` | runtime/proc.go | - | cites-only | go2cs-mit | Q3 |
| `golib/runtime/RuntimeErrorPanic.cs` | runtime/alg.go; runtime/chan.go; runtime/error.go; runtime/iface.go | runtime/error.go:panicwrap; runtime/iface.go:panicnildottype; runtime/panic.go:PanicNilError.Error; runtime/panic.go:PanicNilError.RuntimeError | inspired | go2cs-mit | Q3 |
| `golib/runtime/RuntimePanicCheck.cs` | - | - | cites-only | go2cs-mit | Q3 |
| `golib/runtime/RuntimeSemaphore.cs` | runtime/proc.go; runtime/sema.go | runtime/proc.go:goparkunlock; runtime/proc.go:goyield; runtime/proc.go:pMask.read; runtime/proc.go:ready | inspired | go2cs-mit | Q3 |
| `golib/runtime/SyncTestBubble.cs` | runtime/runtime2.go; runtime/synctest.go | runtime/runtime2.go:waitReason.isIdleInSynctest; runtime/synctest.go:synctestBubble.decActive; runtime/synctest.go:synctestBubble.incActive; runtime/synctest.go:synctestBubble.maybeWakeLocked | inspired | go2cs-mit | Q3 |
| `golib/runtime/WaitReason.cs` | runtime/runtime2.go | - | cites-only | go2cs-mit | Q3 |
| `golib/slice.cs` | runtime/slice.go | runtime/slice.go:nextslicecap | inspired | go2cs-mit | Q3 |
| `golib/stackAppend.cs` | cmd/compile/internal/base/flag.go; cmd/compile/internal/ssagen/ssa.go | cmd/compile/internal/ssagen/ssa.go:state.append; cmd/compile/internal/ssagen/ssa.go:state.move2heap; cmd/compile/internal/ssagen/ssa.go:state.slice; cmd/compile/internal/ssagen/ssa.go:state.store | inspired | go2cs-mit | Q3 |
| `golib/string.cs` | runtime/string.go | runtime/string.go:concatstrings | inspired | go2cs-mit | Q3 |
| `golib/ж.FieldRefBox.cs` | os/os_windows_test.go | - | cites-only | go2cs-mit | Q3 |
| `golib/ж.HeaderSliceBox.cs` | runtime/mbitmap.go; runtime/mpagealloc_64bit.go | - | cites-only | none | Q3 |
| `golib/ж.NativeBox.cs` | runtime/atomic_pointer.go; runtime/map.go | - | cites-only | go2cs-mit | Q3 |
| `golib/ж.NilArrayBox.cs` | runtime/vdso_linux.go | - | cites-only | go2cs-mit | Q3 |
| `golib/ж.PointerTokens.cs` | go/types/check_test.go; runtime/abi_test.go | - | cites-only | go2cs-mit | Q3 |
| `golib/ж.SliceStringViewBox.cs` | runtime/hash64.go | - | cites-only | none | Q3 |
| `golib/ж.StandardBox.cs` | runtime/synctest.go | - | cites-only | go2cs-mit | Q3 |
| `golib/ж.cs` | reflect/all_test.go | reflect/all_test.go:WC.Write; reflect/all_test.go:testTypeWithMethod.String | inspired | go2cs-mit | Q3 |
## Sample headers (for the shape read before step 2)

Each block goes directly under the file's existing go2cs notice, comments only. The shape is the one
`internal/cpu/darwin-arm64/cpu_arm64_darwin_impl.cs` carries on the darwin/arm64 branch. A file that
already opens with Go's notice keeps it where it is and takes only the PROVENANCE block.

**Derived**, `runtime/profbuf_impl.cs`:

```
//
// profBuf.write below is derived from Go, whose notice it carries (license of record: LICENSES/BSD-3-Clause.txt):
//
// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//
// PROVENANCE:
// - profBuf.write: DERIVED FROM src/runtime/profbuf.go, Go 1.26.11, method (*profBuf).write. Go's statements in
//   Go's order and Go's comments verbatim, with the one departure the note below explains (the tag slot holds
//   the reference itself).
```

**Inspired**, `runtime/windows/usleep_windows_impl.cs`:

```
//
// usleep below is inspired by Go, whose notice it carries (license of record: LICENSES/BSD-3-Clause.txt):
//
// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//
// PROVENANCE:
// - usleep: INSPIRED BY src/runtime/os_windows.go, Go 1.26.11, funcs usleep and usleep_no_g. The same wait (a
//   high-resolution waitable timer, else whole milliseconds), in go2cs's own shape over golib's
//   GoHighResolutionTimer, Thread.Sleep and a Stopwatch tail. No Go statement or comment is carried.
```

**Mixed**, `internal/reflectlite/type_impl.cs`:

```
//
// Parts of this file are derived from or inspired by Go, whose notice they carry (license of record:
// LICENSES/BSD-3-Clause.txt):
//
// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//
// PROVENANCE, part by part:
// - rtype.Implements: DERIVED FROM src/internal/reflectlite/type.go, Go 1.26.11, method rtype.Implements. Go's two
//   panic guards verbatim; the method-set test is golib's GoReflect.GoImplements in place of Go's implements call.
// - rtype.Elem, rtype.PkgPath, rtype.String, implements, haveIdenticalUnderlyingType: INSPIRED BY the same file,
//   methods rtype.Elem, rtype.PkgPath, rtype.String and funcs implements, haveIdenticalUnderlyingType. Same
//   contract, go2cs's own bodies over golib's GoReflect.
// - resolveTypeOff, resolveNameOff: bodies for Go's bodyless declarations of the same names (the Go runtime
//   supplies them). Original go2cs code.
// - haveIdenticalFuncShape, funcParamDimsAt, haveIdenticalStructShape, structTypePkgPath, structFieldDescriptor,
//   sysTypeOfLiteType: original go2cs code, helpers over GoReflect. They port no Go function.
```

## Open questions (to COORD)

- **Q1, generated Go sources.** 16 files follow `zsyscall_*.go`, which open with "Code generated ... DO NOT
  EDIT." and carry no notice. Recommended: carry the notice of the Go file that holds the `//sys` declaration
  (for example `src/syscall/syscall_windows.go`) and name both files in the provenance line.
- **Q2, fiat-crypto.** 4 files under `crypto/internal/fips140/nistec/fiat/` follow code generated by fiat-crypto,
  whose BSD-1-Clause notice (in that directory's README) requires keeping its copyright line and disclaimer in
  source. Recommended: carry that notice and disclaimer verbatim, then Go's notice for the Go wrapper files. No
  `LICENSES/` file covers BSD-1-Clause today.
- **Q3, golib.** golib is MIT, outside TestLicensing, and packs only the MIT LICENSE. 22 golib files implement
  a Go function of the same name (inspired by the pre-sort); 26 only cite a Go file for reference. Recommended:
  the step 2 seat covers src/core outside golib; the 22 wait for a separate ruling, because shipping Go's notice
  in the go.lib package is a packaging change, not a comment. The 26 citing files need nothing.
- **Q4, glue.** 54 files only give bodies to bodyless Go declarations (linkname pushes, assembly funcs).
  Recommended: own, header unchanged, unless the step 2 read finds the body follows the pushing Go function, in
  which case that function is the one cited.

## Step 2 results (amended 2026-11-26)

<!-- P5, step 2 of the sweep, under COORD 20261126T055140Z (GO) and 20261126T061317Z (Q2 owner ruling).
     P3 findings F1-F5 on the scout applied. Base d6805be2e8. -->

**What was done.** Every one of the 242 hand-owned files in the pre-sort was read against its Go source (F1).
Each verdict was written as one block of a spec, and every Go name the spec cites is checked against the Go
file it names before a header is written, so a bullet cannot cite a function that file does not declare (F5).

| Outcome | Files |
|---|---|
| Header added, whole file derived from Go | 27 |
| Header added, whole file inspired by Go | 18 |
| Header added, whole file derived and inspired in parts | 9 |
| Header added, mixed: Go parts plus original go2cs code (part by part) | 94 |
| **Headed** | **148** |
| Own or glue, header unchanged (Q4) | 93 |
| Only bodies for bodyless Go declarations plus original code, header unchanged | 1 |
| Skipped: `internal/cpu/darwin/cpu_arm64_darwin_impl.cs`, whose header the darwin/arm64 branch carries | 1 |
| **Total** | **242** |

The pre-sort's buckets (derived 55, mixed 72, inspired 26, own 35, own-glue 54) moved after reading: the
classifier counted as derived any body that keeps Go's names and guards, and many of those bridge the rest
with go2cs's own code, which makes them mixed.

**Header shape.** Inserted under the go2cs notice, comments only:

- one lead sentence naming what the file is derived from or inspired by and the license of record;
- each distinct notice once, verbatim from the Go file followed, unless it is already in the file's head (F2);
- `// PROVENANCE:` or `// PROVENANCE, part by part:`, then one bullet per part. Each Go bullet names the C#
  members, `DERIVED FROM` or `INSPIRED BY`, the GOROOT-relative `src/...` path, the Go version and the Go
  functions, methods, assembly routines or C functions followed. Original parts say "original go2cs code".

`reflect/value_impl.cs` had a one-paragraph `// Provenance:` note; the new block replaces it (the 13 deleted
lines of the diff). Q1 files carry the notice of the Go file holding the `//sys` declarations and name the
generated `zsyscall_*.go` file in the bullet. The 4 fiat files carry fiat-crypto's notice and disclaimer
verbatim, and the new `LICENSES/BSD-1-Clause.txt` holds that text. Per the owner's Q2 ruling ("Notice in files
only"), no `.csproj`, nuspec or `PackageLicenseExpression` changed.

**Version check (F4).** Each Go part was compared against Go 1.23.12, 1.24.13, 1.25.14 and 1.26.11. A part
whose C# follows an older release's text carries that release:

| Part | Version | Reason |
|---|---|---|
| `os/wait_waitid.go` `Process.blockUntilWaitable` | 1.23.12 | the raw waitid loop; 1.26.11 calls `unix.Waitid` |
| `sync/oncefunc.go` `OnceFunc`, `OnceValue`, `OnceValues` | 1.24.13 | the 1.24 body shape |
| `internal/syscall/windows` zsyscall: `NetShareAdd`, `Module32First/Next`, `NtCreateFile/NtOpenFile`, `adjustTokenPrivileges`, `NetUserGetLocalGroups`, `WSAGetOverlappedResult` (6 parts) | 1.24.13 | the generated `Syscall(proc.Addr(), n, ...)` form, changed at 1.25 |
| `syscall` zsyscall: `ConvertSidToStringSid`, `ConvertStringSidToSid`, `NetGetJoinInformation`, `NetUserGetInfo`; `getStartupInfo` (2 parts) | 1.24.13 | same |
| `registry_test.go` `testGetStringsValue` | 1.24.13 | compares with the file's own `equalStringSlice` |

That is 11 pinned parts. In `syscall/windows/dll_windows.cs`, `Syscall` through `Syscall18` and `SyscallN`
are original go2cs code: they were written as forwarders while Go declared them bodyless (through 1.25.14).
Every other part follows 1.26.11. `docs/GoCorpusMigration.md` gains a paragraph on when a hand-own's
version moves: only when the hand-own is re-read at a hop.

**Guard (F3).** `TestLicensingGoProvenanceHeaders` (`src/go2cs/licensing_test.go`) walks `src/core` outside
golib and the generated trees. Every file with a PROVENANCE block must carry a Go notice (or fiat-crypto's),
name a license of record that exists under `LICENSES/`, and every DERIVED FROM / INSPIRED BY bullet must name a
`src/...` `.go`, `.s` or `.c` path and a Go version. Seven fixtures cover the accept and refuse cases. Regress
control on the real tree: deleting the Go copyright line in `time/sleep_impl.cs` and the version from the first
bullet of `runtime/profbuf_impl.cs` made the guard fail naming both files (no Go notice; a bullet with no Go
version). Both were restored from copies and checked by sha256 (`3d604e3e...` and `a3dfe325...`), and the
guard passed again. The guard cannot tell whether a version is current; that stays with the hop runbook.

**Line-number drift.** A header moves every line below it, and three things read line numbers in these files.
The first gate run (repoguard) named two of them. (1) Two hand-owns, `net/windows/lookup_windows.cs` and
`sync/waitgroup.cs`, carry a hand-kept `GoPositionMap` record that maps each `goǃ(` line to its Go `go`
statement; both tables were re-encoded with every C# line moved down by the 9 header lines (`AHNwAABvAJAByAIAAMcC`
to `AHxwAABvAJAByAIAAMcC`, `AP0B3AMAANsD` to `AIYC3AMAANsD`; decoded before and after, Go lines unchanged). These
two attribute lines are the only non-comment lines in the src/core diff. (2) 34 provenance notes in the
published docs cite moved lines with a quote or fingerprint. (3) 8 code-sample source comments cite moved
lines (`TestDocsSamplesHoldTheirLines` reads HEAD, so it shows only once the headers are committed;
`docsamples -base WORKTREE -recite -check` showed them before). Every citation of a headed file outside the
records (`docs/phase3`, `docs/phase4`, `docs/doctrine`, `docs/validation`, and the disclosure JSON prose) was
moved by its file's diff: 114 citations in docs, `.claude` rules and skills, and code comments, each checked to
name the same text at HEAD and in the worktree (the 3 that quote a re-encoded record line were moved by hand).
20 citations name a file by a bare name that matches more than one file and were left as they were; the ones
checked (`runtime2.cs`, `godebug.cs`, `sync/mutex.cs`, `unsafe.cs`) already pointed at other text at base. The disclosure records'
prose cites `testing.cs` and `unsafe.cs` lines too; no disclosure SIGNATURE carries a C# line, so matching is
unaffected, and the prose and the proof pages built from it move at the next bank.

### Q3: golib's 22 name-match files, read one by one

For COORD's ruling. No golib file was changed. "Derived" means Go's statements in Go's order; "inspired" means
the same contract in go2cs's own shape; "cites only" means the name match is a coincidence or a reference.

| golib file | Verdict | Go source followed |
|---|---|---|
| `GoReflect.FinalizerBinding.cs` | **derived (part)** | `runtime/mfinal.go` `SetFinalizer`: the binding switch and its messages, transcribed |
| `slice.cs` | **derived (part)** | `runtime/slice.go` `nextslicecap`, statement by statement, in `NextSliceCap` |
| `runtime/SyncTestBubble.cs` | **derived (part)** + inspired | `runtime/runtime2.go` `isIdleInSynctest` whole in `IsIdle`; `incActive`, `decActive`, `maybeWakeLocked` inspired |
| `GoLibcCall.cs` | inspired | `runtime/sys_darwin.go` syscalln errno rule |
| `GoMemProfile.cs` | inspired | `runtime/malloc.go` `mallocgc` profiling branch, `nextSample`, `profilealloc` |
| `GoReflect.TypeNaming.cs` | inspired | `go/types/typestring.go` |
| `GoReflect.ValueMarshalling.cs` | inspired | `reflect/type.go`, `reflect/value.go`; already carries Go's notice and a provenance line (2 copied lines) |
| `PanicException.cs` | inspired | `runtime/panic.go` `preprintpanics` |
| `builtin.WindowsLongPaths.cs` | inspired | `runtime/os_windows.go` `initLongPathSupport` |
| `channel.cs` | inspired | `runtime/chan.go` `chansend`, `chanrecv`, `send`, `recv` |
| `runtime/CrashReport.cs` | inspired | `runtime/panic.go` `printpanics` |
| `runtime/ExecutionTracer.cs` | inspired | `runtime/trace.go` `StopTrace`, `ReadTrace`; `runtime/tracetime.go` `traceSyncBatch` |
| `runtime/Goroutine.cs` | inspired | `runtime/proc.go` `gopark` accounting, `gcount` |
| `runtime/RuntimeErrorPanic.cs` | inspired | `runtime/error.go` `panicwrap`; `runtime/panic.go` `panicdivide`, `panicmem`; `runtime/iface.go` `panicnildottype` |
| `runtime/RuntimeSemaphore.cs` | inspired | `runtime/sema.go` `cansemacquire`, `semaRoot` queue and dequeue, `semacquire1` order |
| `stackAppend.cs` | inspired | `cmd/compile` ssagen stack-append backing store |
| `string.cs` | inspired | `runtime/string.go` `concatstrings` |
| `GoReflect.MethodSets.cs` | cites only | the matches are LINQ `Select` and `Expression.Convert` |
| `GoStructSynthesis.cs` | cites only | the matches are the words "name" and "string" in comments |
| `runtime/MainExit.cs` | cites only | the match is the word "read" in comments |
| `builtin.cs` | cites only | `fatal` is go2cs's own method; `printpointer` is named in one comment |
| `ж.cs` | cites only | the match is `String()` in a doc comment |

Tally: 3 files hold derived parts, 14 are inspired, 5 cite only. The three derived parts are the ones where
Go's notice would apply most clearly; whether that means a header in golib, which packs only its MIT LICENSE,
is the ruling Q3 left open.

## Q3 ruled: golib headers (amended 2026-11-26)

<!-- P5, on COORD 20261126T080412Z (Q3 ruling, by analogy with the owner's fiat ruling "Notice in files only").
     Commits on top of dc58daa7fb and 3b6ad8d976. -->

**What was done.** The 17 golib files of the Q3 table above that hold derived or inspired parts carry the
step 2 header shape, inserted under golib's own MIT notice: a lead sentence, Go's notice once, then
`// PROVENANCE, part by part:` with one bullet per Go part and one for the original rest. The 5 cite-only
files are unchanged. go.lib's package metadata, its packed LICENSE and NOTICE are unchanged.

| golib file | Go parts |
|---|---|
| `GoReflect.FinalizerBinding.cs` | derived: `runtime/mfinal.go` `SetFinalizer` (the binding switch and its panic texts) |
| `slice.cs` | derived: `runtime/slice.go` `nextslicecap` |
| `runtime/SyncTestBubble.cs` | derived: `runtime/runtime2.go` `waitReason.isIdleInSynctest`; inspired: `runtime/synctest.go` `incActive`, `decActive`, `maybeWakeLocked` |
| `GoLibcCall.cs` | inspired: `runtime/sys_darwin.go` `syscall_rawsyscalln` |
| `GoMemProfile.cs` | inspired: `runtime/malloc.go` `mallocgc`, `profilealloc`, `nextSample` |
| `GoReflect.TypeNaming.cs` | inspired: `go/types/typestring.go` `typeWriter.typ` |
| `GoReflect.ValueMarshalling.cs` | inspired: `reflect/type.go` `directlyAssignable`, `haveIdenticalType`, `haveIdenticalUnderlyingType`; `reflect/value.go` `Value.Convert`, `convertOp` |
| `PanicException.cs` | inspired: `runtime/panic.go` `preprintpanics` |
| `builtin.WindowsLongPaths.cs` | inspired: `runtime/os_windows.go` `initLongPathSupport` |
| `channel.cs` | inspired: `runtime/chan.go` `chansend`, `chanrecv`, `send`, `recv` |
| `runtime/CrashReport.cs` | inspired: `runtime/panic.go` `printpanics` |
| `runtime/ExecutionTracer.cs` | inspired: `runtime/trace.go` `StopTrace`, `ReadTrace`; `runtime/tracetime.go` `traceSyncBatch` |
| `runtime/Goroutine.cs` | inspired: `runtime/proc.go` `gopark`, `goready`, `gcount` |
| `runtime/RuntimeErrorPanic.cs` | inspired: `runtime/error.go` `panicwrap`; `runtime/panic.go` `panicdivide`, `panicmem`; `runtime/iface.go` `panicnildottype` |
| `runtime/RuntimeSemaphore.cs` | inspired: `runtime/sema.go` `cansemacquire`, `semaRoot.queue`, `semaRoot.dequeue`, `semacquire1` |
| `stackAppend.cs` | inspired: `cmd/compile/internal/ssagen/ssa.go` `state.getBackingStoreInfoForAppend`, `state.append` |
| `string.cs` | inspired: `runtime/string.go` `concatstrings` |

Every part cites Go 1.26.11, and every Go name was checked against the file it names before the header was
written. `GoReflect.ValueMarshalling.cs` already carried Go's notice and a `// Provenance:` paragraph; the new
block replaces them (the 10 deleted lines of the golib diff). All golib changes are comments.

**Guard.** It fits: `TestLicensingGoProvenanceHeaders` no longer skips golib, and it fails if the walk reaches
no golib file with a PROVENANCE block, so a later skip cannot turn the golib half off without a red. Red
controls on the real tree: deleting Go's copyright line from `string.cs` made the guard fail naming that file;
putting golib back on the skip list made it fail with the golib liveness message. Both restored and checked by
sha256. The gap is the same as in the rest of src/core: a file that follows Go and carries no PROVENANCE block
at all is not seen by the guard.

**Line citations, and a correction to step 2.** The golib headers move every line below them, so citations of
the 17 files were moved the same way as in step 2: 41 citations in the living docs, each checked to name the
same text at HEAD and in the worktree, including 5 on continuation lines and 2 given as a bare `slice.cs`. No
code comment cites a moved golib line.
Correction: the four `docs/PLAN-*.md` files and `docs/DESIGN-aeshash-arm64.md` cite lines at a stated commit
("At 30ad20e17e", "at 59ee0d21bf", "Taken at 4aebdadc1a"), so their line numbers are right as written and must
not move. Step 2 had moved 26 of its 114 citations in those five files (one of them, a `:31` continuation in
PLAN-cgo-interop, onto the wrong file); those five files are restored to their base text, which leaves 88 step 2
citations, all in living docs, `.claude` rules and skills, and code comments.

## Master merge after train 15 (amended 2026-11-26)

<!-- P5. Master 51f4d4b37a merged into the seat at 773508cb86. -->

Train 15 touched three things the sweep owns. (1) `docs/ConversionStrategies.md`: one conflict, where master
re-cited `visitFuncDecl.go` and the seat re-cited `testing.cs`; both moves kept. (2) R's
`internal/cpu/amd64/cpu_x86_impl.cs` gained a second attribution block above the go2cs notice
(`// PROVENANCE — Go 1.26.11 ...`), in another shape from the seat's block below it. The merge was clean and left
the file with two Go notices and two blocks that disagreed (the seat had missed `cpuid`; R named it, and
`getGOAMD64level` as following cpu_x86.s). Resolved to one block in the sweep's shape carrying R's parts:
`cpuid` and `getGOAMD64level` INSPIRED BY `src/internal/cpu/cpu_x86.s`, the HasAVX512BITALG read DERIVED FROM
doinit. (3) The new `internal/runtime/gc/scan/amd64/scan_amd64_impl.cs` carried the same other shape; it now
has the sweep's shape with R's parts and notes unchanged, in the same number of lines.
The guard read neither file, because it checked only a block under the two exact headings. It now refuses a
`// PROVENANCE` heading in any other shape; a fixture covers it, and master's scan_amd64_impl.cs put back made it
fail naming the file (restored, sha256 checked).

## P3's read folded (amended 2026-11-26)

<!-- P5, on P3 20261126T170601Z (CLEAR WITH NITS) and COORD 20261126T170816Z (fold SHOULD 2 and 3; SHOULD 1, a
     golib NOTICE, waits for the owner and is not in the seat). -->

**SHOULD 2, the release a part cites.** `internal/syscall/windows/windows/zsyscall_windows_version_impl.cs`'s
`rtlGetVersion` bullet now says what follows which release: the record is 1.26.11's `_OSVERSIONINFOEXW`, and the
call keeps 1.24.13's `syscall.Syscall(addr, 1, ...)` form where 1.26.11 calls `SyscallN`. A census of every DERIVED
bullet citing a `zsyscall_windows.go` found it the only part at 1.26.11 over a fixed-arity call; the others (seven files)
were already pinned to 1.24.13. The INSPIRED wrappers P3 named (ws2_32, addrinfo, certchain, dnsrecord) keep
1.26.11: the runbook paragraph now states the rule both kinds follow. A DERIVED part cites the release its
statements follow and names any detail that follows another; an INSPIRED part cites the release its contract was
read against, its shape is go2cs's own, and a change to Go's form alone does not make it stale.

**SHOULD 3, the guard.** `goProvenanceProblems` now refuses `// PROVENANCE` text with no block line in the first
300 lines, besides a heading in another shape (fixture "no block line"). Red control: a `// PROVENANCE:` line
indented inside `runtime/trace/annotation_impl.cs` (an own file) made the guard fail naming it; restored, sha256
checked. P3's floor (every marked or `*_impl.cs` file has a block or is on a checked-in own list) is not in this
seat; it can be its own. Train 16's new hand-owns get headed as they land.

**NIT 5, the step 2 table.** The "own or glue" row is 92, not 93: the 93 spec entries with no Go part include the
skipped `cpu_arm64_darwin_impl.cs`, which has its own row. 148 headed + 92 own or glue + 1 G+O only + 1 skipped
= 242, so 94 files are unheaded, as P3 counted.
