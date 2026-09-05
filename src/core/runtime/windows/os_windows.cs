// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

// TODO(brainman): should not need those
internal static UntypedInt _NSIG => 65;

[GoType("unsafe_package.Pointer")] partial struct stdFunction;

//go:cgo_import_dynamic runtime._AddVectoredContinueHandler AddVectoredContinueHandler%2 "kernel32.dll"
//go:cgo_import_dynamic runtime._AddVectoredExceptionHandler AddVectoredExceptionHandler%2 "kernel32.dll"
//go:cgo_import_dynamic runtime._CloseHandle CloseHandle%1 "kernel32.dll"
//go:cgo_import_dynamic runtime._CreateEventA CreateEventA%4 "kernel32.dll"
//go:cgo_import_dynamic runtime._CreateIoCompletionPort CreateIoCompletionPort%4 "kernel32.dll"
//go:cgo_import_dynamic runtime._CreateThread CreateThread%6 "kernel32.dll"
//go:cgo_import_dynamic runtime._CreateWaitableTimerA CreateWaitableTimerA%3 "kernel32.dll"
//go:cgo_import_dynamic runtime._CreateWaitableTimerExW CreateWaitableTimerExW%4 "kernel32.dll"
//go:cgo_import_dynamic runtime._DuplicateHandle DuplicateHandle%7 "kernel32.dll"
//go:cgo_import_dynamic runtime._ExitProcess ExitProcess%1 "kernel32.dll"
//go:cgo_import_dynamic runtime._FreeEnvironmentStringsW FreeEnvironmentStringsW%1 "kernel32.dll"
//go:cgo_import_dynamic runtime._GetConsoleMode GetConsoleMode%2 "kernel32.dll"
//go:cgo_import_dynamic runtime._GetCurrentThreadId GetCurrentThreadId%0 "kernel32.dll"
//go:cgo_import_dynamic runtime._GetEnvironmentStringsW GetEnvironmentStringsW%0 "kernel32.dll"
//go:cgo_import_dynamic runtime._GetErrorMode GetErrorMode%0 "kernel32.dll"
//go:cgo_import_dynamic runtime._GetProcAddress GetProcAddress%2 "kernel32.dll"
//go:cgo_import_dynamic runtime._GetProcessAffinityMask GetProcessAffinityMask%3 "kernel32.dll"
//go:cgo_import_dynamic runtime._GetQueuedCompletionStatusEx GetQueuedCompletionStatusEx%6 "kernel32.dll"
//go:cgo_import_dynamic runtime._GetStdHandle GetStdHandle%1 "kernel32.dll"
//go:cgo_import_dynamic runtime._GetSystemDirectoryA GetSystemDirectoryA%2 "kernel32.dll"
//go:cgo_import_dynamic runtime._GetSystemInfo GetSystemInfo%1 "kernel32.dll"
//go:cgo_import_dynamic runtime._GetThreadContext GetThreadContext%2 "kernel32.dll"
//go:cgo_import_dynamic runtime._SetThreadContext SetThreadContext%2 "kernel32.dll"
//go:cgo_import_dynamic runtime._LoadLibraryExW LoadLibraryExW%3 "kernel32.dll"
//go:cgo_import_dynamic runtime._LoadLibraryW LoadLibraryW%1 "kernel32.dll"
//go:cgo_import_dynamic runtime._PostQueuedCompletionStatus PostQueuedCompletionStatus%4 "kernel32.dll"
//go:cgo_import_dynamic runtime._QueryPerformanceCounter QueryPerformanceCounter%1 "kernel32.dll"
//go:cgo_import_dynamic runtime._QueryPerformanceFrequency QueryPerformanceFrequency%1 "kernel32.dll"
//go:cgo_import_dynamic runtime._RaiseFailFastException RaiseFailFastException%3 "kernel32.dll"
//go:cgo_import_dynamic runtime._ResumeThread ResumeThread%1 "kernel32.dll"
//go:cgo_import_dynamic runtime._RtlLookupFunctionEntry RtlLookupFunctionEntry%3 "kernel32.dll"
//go:cgo_import_dynamic runtime._RtlVirtualUnwind  RtlVirtualUnwind%8 "kernel32.dll"
//go:cgo_import_dynamic runtime._SetConsoleCtrlHandler SetConsoleCtrlHandler%2 "kernel32.dll"
//go:cgo_import_dynamic runtime._SetErrorMode SetErrorMode%1 "kernel32.dll"
//go:cgo_import_dynamic runtime._SetEvent SetEvent%1 "kernel32.dll"
//go:cgo_import_dynamic runtime._SetProcessPriorityBoost SetProcessPriorityBoost%2 "kernel32.dll"
//go:cgo_import_dynamic runtime._SetThreadPriority SetThreadPriority%2 "kernel32.dll"
//go:cgo_import_dynamic runtime._SetUnhandledExceptionFilter SetUnhandledExceptionFilter%1 "kernel32.dll"
//go:cgo_import_dynamic runtime._SetWaitableTimer SetWaitableTimer%6 "kernel32.dll"
//go:cgo_import_dynamic runtime._SuspendThread SuspendThread%1 "kernel32.dll"
//go:cgo_import_dynamic runtime._SwitchToThread SwitchToThread%0 "kernel32.dll"
//go:cgo_import_dynamic runtime._TlsAlloc TlsAlloc%0 "kernel32.dll"
//go:cgo_import_dynamic runtime._VirtualAlloc VirtualAlloc%4 "kernel32.dll"
//go:cgo_import_dynamic runtime._VirtualFree VirtualFree%3 "kernel32.dll"
//go:cgo_import_dynamic runtime._VirtualQuery VirtualQuery%3 "kernel32.dll"
//go:cgo_import_dynamic runtime._WaitForSingleObject WaitForSingleObject%2 "kernel32.dll"
//go:cgo_import_dynamic runtime._WaitForMultipleObjects WaitForMultipleObjects%4 "kernel32.dll"
//go:cgo_import_dynamic runtime._WerGetFlags WerGetFlags%2 "kernel32.dll"
//go:cgo_import_dynamic runtime._WerSetFlags WerSetFlags%1 "kernel32.dll"
//go:cgo_import_dynamic runtime._WriteConsoleW WriteConsoleW%5 "kernel32.dll"
//go:cgo_import_dynamic runtime._WriteFile WriteFile%5 "kernel32.dll"
internal static stdFunction _AddVectoredContinueHandler;
internal static stdFunction _AddVectoredExceptionHandler;
internal static stdFunction _CloseHandle;
internal static stdFunction _CreateEventA;
internal static stdFunction _CreateIoCompletionPort;
internal static stdFunction _CreateThread;
internal static stdFunction _CreateWaitableTimerA;
internal static stdFunction _CreateWaitableTimerExW;
internal static stdFunction _DuplicateHandle;
internal static stdFunction _ExitProcess;
internal static stdFunction _FreeEnvironmentStringsW;
internal static stdFunction _GetConsoleMode;
internal static stdFunction _GetCurrentThreadId;
internal static stdFunction _GetEnvironmentStringsW;
internal static stdFunction _GetErrorMode;
internal static stdFunction _GetProcAddress;
internal static stdFunction _GetProcessAffinityMask;
internal static stdFunction _GetQueuedCompletionStatusEx;
internal static stdFunction _GetStdHandle;
internal static stdFunction _GetSystemDirectoryA;
internal static stdFunction _GetSystemInfo;
internal static stdFunction _GetThreadContext;
internal static stdFunction _SetThreadContext;
internal static stdFunction _LoadLibraryExW;
internal static stdFunction _LoadLibraryW;
internal static stdFunction _PostQueuedCompletionStatus;
internal static stdFunction _QueryPerformanceCounter;
internal static stdFunction _QueryPerformanceFrequency;
internal static stdFunction _RaiseFailFastException;
internal static stdFunction _ResumeThread;
internal static stdFunction _RtlLookupFunctionEntry;
internal static stdFunction _RtlVirtualUnwind;
internal static stdFunction _SetConsoleCtrlHandler;
internal static stdFunction _SetErrorMode;
internal static stdFunction _SetEvent;
internal static stdFunction _SetProcessPriorityBoost;
internal static stdFunction _SetThreadPriority;
internal static stdFunction _SetUnhandledExceptionFilter;
internal static stdFunction _SetWaitableTimer;
internal static stdFunction _SuspendThread;
internal static stdFunction _SwitchToThread;
internal static stdFunction _TlsAlloc;
internal static stdFunction _VirtualAlloc;
internal static stdFunction _VirtualFree;
internal static stdFunction _VirtualQuery;
internal static stdFunction _WaitForSingleObject;
internal static stdFunction _WaitForMultipleObjects;
internal static stdFunction _WerGetFlags;
internal static stdFunction _WerSetFlags;
internal static stdFunction _WriteConsoleW;
internal static stdFunction _WriteFile;
internal static stdFunction _ᴛ1ʗ;
internal static stdFunction _ProcessPrng;
internal static stdFunction _NtCreateWaitCompletionPacket;
internal static stdFunction _NtAssociateWaitCompletionPacket;
internal static stdFunction _NtCancelWaitCompletionPacket;
internal static stdFunction _RtlGetCurrentPeb;
internal static stdFunction _RtlGetVersion;
internal static stdFunction _timeBeginPeriod;
internal static stdFunction _timeEndPeriod;
internal static stdFunction _ᴛ2ʗ;

internal static array<uint16> bcryptprimitivesdll = new uint16[]{(rune)'b', (rune)'c', (rune)'r', (rune)'y', (rune)'p', (rune)'t', (rune)'p', (rune)'r', (rune)'i', (rune)'m', (rune)'i', (rune)'t', (rune)'i', (rune)'v', (rune)'e', (rune)'s', (rune)'.', (rune)'d', (rune)'l', (rune)'l', 0}.array();
internal static array<uint16> ntdlldll = new uint16[]{(rune)'n', (rune)'t', (rune)'d', (rune)'l', (rune)'l', (rune)'.', (rune)'d', (rune)'l', (rune)'l', 0}.array();
internal static array<uint16> powrprofdll = new uint16[]{(rune)'p', (rune)'o', (rune)'w', (rune)'r', (rune)'p', (rune)'r', (rune)'o', (rune)'f', (rune)'.', (rune)'d', (rune)'l', (rune)'l', 0}.array();
internal static array<uint16> winmmdll = new uint16[]{(rune)'w', (rune)'i', (rune)'n', (rune)'m', (rune)'m', (rune)'.', (rune)'d', (rune)'l', (rune)'l', 0}.array();

// Function to be called by windows CreateThread
// to start new os thread.
internal static partial void tstart_stdcall(ж<m> newm);

// Init-time helper
internal static partial void wintls();

[GoType] partial struct mOS {
    internal mutex threadLock;   // protects "thread" and prevents closing
    internal uintptr thread; // thread handle
    internal uintptr waitsema; // semaphore for parking on locks
    internal uintptr resumesema; // semaphore to indicate suspend/resume
    internal uintptr highResTimer; // high resolution timer handle used in usleep
    internal uintptr waitIocpTimer; // high resolution timer handle used in netpoll
    internal uintptr waitIocpHandle; // wait completion handle used in netpoll
    // preemptExtLock synchronizes preemptM with entry/exit from
    // external C code.
    //
    // This protects against races between preemptM calling
    // SuspendThread and external code on this thread calling
    // ExitProcess. If these happen concurrently, it's possible to
    // exit the suspending thread and suspend the exiting thread,
    // leading to deadlock.
    //
    // 0 indicates this M is not being preempted or in external
    // code. Entering external code CASes this from 0 to 1. If
    // this fails, a preemption is in progress, so the thread must
    // wait for the preemption. preemptM also CASes this from 0 to
    // 1. If this fails, the preemption fails (as it would if the
    // PC weren't in Go code). The value is reset to 0 when
    // returning from external code or after a preemption is
    // complete.
    //
    // TODO(austin): We may not need this if preemption were more
    // tightly synchronized on the G/P status and preemption
    // blocked transition into _Gsyscall/_Psyscall.
    internal uint32 preemptExtLock;
}

// Stubs so tests can link correctly. These should never be called.
internal static int32 open(ref byte name, int32 mode, int32 perm) {
    @throw(unimplementedˢ);
    return -1;
}

internal static int32 closefd(int32 fd) {
    @throw(unimplementedˢ);
    return -1;
}

internal static int32 read(int32 fd, @unsafe.Pointer Δp, int32 n) {
    @throw(unimplementedˢ);
    return -1;
}

[GoType] partial struct sigset {
}

// Call a Windows function with stdcall conventions,
// and switch to os stack during the call.
internal static partial void asmstdcall(@unsafe.Pointer fn);

internal static @unsafe.Pointer asmstdcallAddr;

[GoType("libcall")] partial struct winlibcall;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string usageˢ = "usage"u8;

internal static stdFunction windowsFindfunc(uintptr lib, slice<byte> name) {
    if (name[len(name) - 1] != 0) {
        @throw(usageˢ);
    }
    var f = stdcall2(_GetProcAddress, lib, (uintptr)Ꮡ(name, 0));
    return ((stdFunction)(@unsafe.Pointer)f);
}

internal static UntypedInt _MAX_PATH => 260; // https://docs.microsoft.com/en-us/windows/win32/fileio/maximum-file-path-limitation

internal static ж<array<byte>> ᏑsysDirectory = new StandardBox<array<byte>>(new array<byte>(261));
internal static ref array<byte> sysDirectory => ref ᏑsysDirectory.Value;

internal static uintptr sysDirectoryLen;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unableToDetermineSystemˢ = "Unable to determine system directory"u8;

internal static void initSysDirectory() {
    var l = stdcall2(_GetSystemDirectoryA, (uintptr)ᏑsysDirectory.at<byte>(0), (uintptr)(len(sysDirectory) - 1));
    if (l == 0 || l > (uintptr)(len(sysDirectory) - 1)) {
        @throw(unableToDetermineSystemˢ);
    }
    sysDirectory[(nint)(l)] = (rune)'\\';
    sysDirectoryLen = l + 1;
}

//go:linkname windows_GetSystemDirectory internal/syscall/windows.GetSystemDirectory
public static @string windows_GetSystemDirectory() {
    return @unsafe.String(ᏑsysDirectory.at<byte>(0), sysDirectoryLen);
}

internal static uintptr windowsLoadSystemLib(slice<uint16> name) {
    return stdcall3(_LoadLibraryExW, (uintptr)Ꮡ(name, 0), 0, _LOAD_LIBRARY_SEARCH_SYSTEM32);
}

//go:linkname windows_QueryPerformanceCounter internal/syscall/windows.QueryPerformanceCounter
internal static int64 windows_QueryPerformanceCounter() {
    ref var counter = ref heap(new int64(), out var Ꮡcounter);
    stdcall1(_QueryPerformanceCounter, (uintptr)Ꮡcounter);
    return counter;
}

//go:linkname windows_QueryPerformanceFrequency internal/syscall/windows.QueryPerformanceFrequency
internal static int64 windows_QueryPerformanceFrequency() {
    ref var frequency = ref heap(new int64(), out var Ꮡfrequency);
    stdcall1(_QueryPerformanceFrequency, (uintptr)Ꮡfrequency);
    return frequency;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string bcryptprimitivesDllNotˢ = "bcryptprimitives.dll not found"u8;
internal static readonly @string ntdllDllNotFoundˢ = "ntdll.dll not found"u8;

internal static void loadOptionalSyscalls() {
    var bcryptPrimitives = windowsLoadSystemLib(bcryptprimitivesdll[..]);
    if (bcryptPrimitives == 0) {
        @throw(bcryptprimitivesDllNotˢ);
    }
    _ProcessPrng = windowsFindfunc(bcryptPrimitives, slice<byte>("ProcessPrng\u0000"u8));
    var n32 = windowsLoadSystemLib(ntdlldll[..]);
    if (n32 == 0) {
        @throw(ntdllDllNotFoundˢ);
    }
    _NtCreateWaitCompletionPacket = windowsFindfunc(n32, slice<byte>("NtCreateWaitCompletionPacket\u0000"u8));
    if (_NtCreateWaitCompletionPacket != default!) {
        // These functions should exists if NtCreateWaitCompletionPacket exists.
        _NtAssociateWaitCompletionPacket = windowsFindfunc(n32, slice<byte>("NtAssociateWaitCompletionPacket\u0000"u8));
        if (_NtAssociateWaitCompletionPacket == default!) {
            @throw("NtCreateWaitCompletionPacket exists but NtAssociateWaitCompletionPacket does not"u8);
        }
        _NtCancelWaitCompletionPacket = windowsFindfunc(n32, slice<byte>("NtCancelWaitCompletionPacket\u0000"u8));
        if (_NtCancelWaitCompletionPacket == default!) {
            @throw("NtCreateWaitCompletionPacket exists but NtCancelWaitCompletionPacket does not"u8);
        }
    }
    _RtlGetCurrentPeb = windowsFindfunc(n32, slice<byte>("RtlGetCurrentPeb\u0000"u8));
    _RtlGetVersion = windowsFindfunc(n32, slice<byte>("RtlGetVersion\u0000"u8));
}

[GoType("dyn")] internal partial struct monitorSuspendResume__DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS {
    internal uintptr callback;
    internal uintptr context;
}

internal static void monitorSuspendResume() {
    uintptr _DEVICE_NOTIFY_CALLBACK = 2;
    var powrprof = windowsLoadSystemLib(powrprofdll[..]);
    if (powrprof == 0) {
        return; // Running on Windows 7, where we don't need it anyway.
    }
    stdFunction powerRegisterSuspendResumeNotification = windowsFindfunc(powrprof, slice<byte>("PowerRegisterSuspendResumeNotification\u0000"u8));
    if (powerRegisterSuspendResumeNotification == default!) {
        return; // Running on Windows 7, where we don't need it anyway.
    }
    ref var fn = ref heap<any>(out var Ꮡfn);

    fn = (uintptr context, uint32 changeType, uintptr setting) => {
        for (var mp = (ж<m>)(uintptr)(atomic.Loadp(@unsafe.Pointer.FromBox(Ꮡallm))); mp != nil; mp = mp.Value.alllink) {
            if ((~mp).resumesema != 0) {
                stdcall1(_SetEvent, (~mp).resumesema);
            }
        }
        return (uintptr)(0);
    };
    ref var @params = ref heap<monitorSuspendResume__DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS>(out var Ꮡparams);
    @params = new monitorSuspendResume__DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS(
        callback: compileCallback(efaceOf(Ꮡfn).Value, true)
    );
    ref var handle = ref heap<uintptr>(out var Ꮡhandle);
    handle = (uintptr)0;
    stdcall3(powerRegisterSuspendResumeNotification, _DEVICE_NOTIFY_CALLBACK,
        (uintptr)Ꮡparams, (uintptr)@unsafe.Pointer.FromBox(Ꮡhandle));
}

internal static int32 getproccount() {
    ref var mask = ref heap(new uintptr(), out var Ꮡmask);
    ref var sysmask = ref heap(new uintptr(), out var Ꮡsysmask);
    var ret = stdcall3(_GetProcessAffinityMask, currentProcess, (uintptr)@unsafe.Pointer.FromBox(Ꮡmask), (uintptr)@unsafe.Pointer.FromBox(Ꮡsysmask));
    if (ret != 0) {
        nint n = 0;
        nint maskbits = (nint)(/* unsafe.Sizeof(mask) */ (uintptr)8 * 8);
        for (nint i = 0; i < maskbits; i++) {
            if ((uintptr)(mask & (((uintptr)1).Lsh((nuint)i))) != 0) {
                n++;
            }
        }
        if (n != 0) {
            return (int32)n;
        }
    }
    // use GetSystemInfo if GetProcessAffinityMask fails
    ref var info = ref heap(new systeminfo(), out var Ꮡinfo);
    stdcall1(_GetSystemInfo, (uintptr)Ꮡinfo);
    return (int32)info.dwnumberofprocessors;
}

internal static uintptr getPageSize() {
    ref var info = ref heap(new systeminfo(), out var Ꮡinfo);
    stdcall1(_GetSystemInfo, (uintptr)Ꮡinfo);
    return (uintptr)info.dwpagesize;
}

internal static uintptr currentProcess => /* ^uintptr(0) */ unchecked((uintptr)18446744073709551615); // -1 = current process
internal static uintptr currentThread => /* ^uintptr(1) */ unchecked((uintptr)18446744073709551614); // -2 = current thread

// in sys_windows_386.s and sys_windows_amd64.s:
internal static partial uint32 getlasterror();

internal static ж<uint32> ᏑtimeBeginPeriodRetValue = new StandardBox<uint32>(default(uint32));
internal static ref uint32 timeBeginPeriodRetValue => ref ᏑtimeBeginPeriodRetValue.Value;

// osRelaxMinNS indicates that sysmon shouldn't osRelax if the next
// timer is less than 60 ms from now. Since osRelaxing may reduce
// timer resolution to 15.6 ms, this keeps timer error under roughly 1
// part in 4.
internal static UntypedFloat osRelaxMinNS => /* 60 * 1e6 */ 6e+07;

// osRelax is called by the scheduler when transitioning to and from
// all Ps being idle.
//
// Some versions of Windows have high resolution timer. For those
// versions osRelax is noop.
// For Windows versions without high resolution timer, osRelax
// adjusts the system-wide timer resolution. Go needs a
// high resolution timer while running and there's little extra cost
// if we're already using the CPU, but if all Ps are idle there's no
// need to consume extra power to drive the high-res timer.
internal static uint32 osRelax(bool relax) {
    if (haveHighResTimer) {
        // If the high resolution timer is available, the runtime uses the timer
        // to sleep for short durations. This means there's no need to adjust
        // the global clock frequency.
        return 0;
    }
    if (relax){
        return (uint32)stdcall1(_timeEndPeriod, 1);
    } else {
        return (uint32)stdcall1(_timeBeginPeriod, 1);
    }
}

// haveHighResTimer indicates that the CreateWaitableTimerEx
// CREATE_WAITABLE_TIMER_HIGH_RESOLUTION flag is available.
internal static bool haveHighResTimer = false;

// haveHighResSleep indicates that NtCreateWaitCompletionPacket
// exists and haveHighResTimer is true.
// NtCreateWaitCompletionPacket has been available since Windows 10,
// but has just been publicly documented, so some platforms, like Wine,
// doesn't support it yet.
public static bool haveHighResSleep = false;

// createHighResTimer calls CreateWaitableTimerEx with
// CREATE_WAITABLE_TIMER_HIGH_RESOLUTION flag to create high
// resolution timer. createHighResTimer returns new timer
// handle or 0, if CreateWaitableTimerEx failed.
internal static uintptr createHighResTimer() {
    uintptr _CREATE_WAITABLE_TIMER_HIGH_RESOLUTION = 0x00000002;
    UntypedInt _SYNCHRONIZE = 0x00100000;
    UntypedInt _TIMER_QUERY_STATE = 0x0001;
    UntypedInt _TIMER_MODIFY_STATE = 0x0002;
    return stdcall4(_CreateWaitableTimerExW, 0, 0,
        _CREATE_WAITABLE_TIMER_HIGH_RESOLUTION,
        (uintptr)((uintptr)(UntypedInt)(_SYNCHRONIZE | _TIMER_QUERY_STATE) | (uintptr)_TIMER_MODIFY_STATE));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string winmmDllNotFoundˢ = "winmm.dll not found"u8;
internal static readonly @string timeBeginEndPeriodNotˢ = "timeBegin/EndPeriod not found"u8;

internal static void initHighResTimer() {
    var h = createHighResTimer();
    if (h != 0){
        haveHighResTimer = true;
        haveHighResSleep = _NtCreateWaitCompletionPacket != default!;
        stdcall1(_CloseHandle, h);
    } else {
        // Only load winmm.dll if we need it.
        // This avoids a dependency on winmm.dll for Go programs
        // that run on new Windows versions.
        var m32 = windowsLoadSystemLib(winmmdll[..]);
        if (m32 == 0) {
            print((@string)"runtime: LoadLibraryExW failed; errno="u8, getlasterror(), (@string)"\n"u8);
            @throw(winmmDllNotFoundˢ);
        }
        _timeBeginPeriod = windowsFindfunc(m32, slice<byte>("timeBeginPeriod\u0000"u8));
        _timeEndPeriod = windowsFindfunc(m32, slice<byte>("timeEndPeriod\u0000"u8));
        if (_timeBeginPeriod == default! || _timeEndPeriod == default!) {
            print((@string)"runtime: GetProcAddress failed; errno="u8, getlasterror(), (@string)"\n"u8);
            @throw(timeBeginEndPeriodNotˢ);
        }
    }
}

//go:linkname canUseLongPaths internal/syscall/windows.CanUseLongPaths
public static bool canUseLongPaths;

// initLongPathSupport enables long path support.
internal static void initLongPathSupport() {
    const byte IsLongPathAwareProcess = 0x80;
    uintptr PebBitFieldOffset = 3;
    // Check that we're ≥ 10.0.15063.
    ref var info = ref heap<_OSVERSIONINFOW>(out var Ꮡinfo);
    info = new _OSVERSIONINFOW(nil);
    info.osVersionInfoSize = (uint32)/* unsafe.Sizeof(info) */ (uintptr)276;
    stdcall1(_RtlGetVersion, (uintptr)Ꮡinfo);
    if (info.majorVersion < 10 || (info.majorVersion == 10 && info.minorVersion == 0 && info.buildNumber < 15063)) {
        return;
    }
    // Set the IsLongPathAwareProcess flag of the PEB's bit field.
    // This flag is not documented, but it's known to be used
    // by Windows to enable long path support.
    var bitField = (ж<byte>)(uintptr)((@unsafe.Pointer)(stdcall0(_RtlGetCurrentPeb) + PebBitFieldOffset));
    bitField.Value |= (byte)(IsLongPathAwareProcess);
    canUseLongPaths = true;
}

internal static void osinit() {
    asmstdcallAddr = (@unsafe.Pointer)abi.FuncPCABI0(asmstdcall);
    loadOptionalSyscalls();
    preventErrorDialogs();
    initExceptionHandler();
    initHighResTimer();
    timeBeginPeriodRetValue = osRelax(false);
    initSysDirectory();
    initLongPathSupport();
    ncpu = getproccount();
    physPageSize = getPageSize();
    // Windows dynamic priority boosting assumes that a process has different types
    // of dedicated threads -- GUI, IO, computational, etc. Go processes use
    // equivalent threads that all do a mix of GUI, IO, computations, etc.
    // In such context dynamic priority boosting does nothing but harm, so we turn it off.
    stdcall2(_SetProcessPriorityBoost, currentProcess, 1);
}

//go:nosplit
internal static nint readRandom(slice<byte> r) {
    nint n = 0;
    if ((uintptr)(stdcall2(_ProcessPrng, (uintptr)Ꮡ(r, 0), (uintptr)len(r)) & 0xff) != 0) {
        n = len(r);
    }
    return n;
}

internal static void goenvs() {
    // strings is a pointer to environment variable pairs in the form:
    //     "envA=valA\x00envB=valB\x00\x00" (in UTF-16)
    // Two consecutive zero bytes end the list.
    @unsafe.Pointer strings = (@unsafe.Pointer)stdcall0(_GetEnvironmentStringsW);
    var Δp = (~(ж<array<uint16>>)(uintptr)(strings))[..];
    nint n = 0;
    for ((nint from, nint i) = (0, 0); true; i++) {
        if (Δp[i] == 0) {
            // empty string marks the end
            if (i == from) {
                break;
            }
            from = i + 1;
            n++;
        }
    }
    envs = new slice<@string>(n);
    foreach (var (i, _) in envs) {
        envs[i] = gostringw(Ꮡ(Δp, 0));
        while (Δp[0] != 0) {
            Δp = Δp[1..];
        }
        Δp = Δp[1..]; // skip nil byte
    }
    stdcall1(_FreeEnvironmentStringsW, (uintptr)strings);
    // We call these all the way here, late in init, so that malloc works
    // for the callback functions these generate.
    ref var fn = ref heap<any>(out var Ꮡfn);

    fn = ctrlHandler;
    var ctrlHandlerPC = compileCallback(efaceOf(Ꮡfn).Value, true);
    stdcall2(_SetConsoleCtrlHandler, ctrlHandlerPC, 1);
    monitorSuspendResume();
}

// exiting is set to non-zero when the process is exiting.
internal static ж<uint32> Ꮡexiting = new StandardBox<uint32>(default(uint32));
internal static ref uint32 exiting => ref Ꮡexiting.Value;

//go:nosplit
internal static void exit(int32 code) {
    // Disallow thread suspension for preemption. Otherwise,
    // ExitProcess and SuspendThread can race: SuspendThread
    // queues a suspension request for this thread, ExitProcess
    // kills the suspending thread, and then this thread suspends.
    @lock(ᏑsuspendLock);
    atomic.Store(Ꮡexiting, 1);
    stdcall1(_ExitProcess, (uintptr)code);
}

// write1 must be nosplit because it's used as a last resort in
// functions like badmorestackg0. In such cases, we'll always take the
// ASCII path.
//
//go:nosplit
internal static unsafe int32 write1(uintptr fd, @unsafe.Pointer buf, int32 n) {
    uintptr _STD_OUTPUT_HANDLE = /* ^uintptr(10) */ unchecked((uintptr)18446744073709551605); // -11
    uintptr _STD_ERROR_HANDLE = /* ^uintptr(11) */ unchecked((uintptr)18446744073709551604); // -12
    uintptr handle = default!;
    var exprᴛ1 = fd;
    if (exprᴛ1 == 1) {
        handle = stdcall1(_GetStdHandle, _STD_OUTPUT_HANDLE);
    }
    else if (exprᴛ1 == 2) {
        handle = stdcall1(_GetStdHandle, _STD_ERROR_HANDLE);
    }
    else { /* default: */
        handle = fd;
    }

    // assume fd is real windows handle.
    var isASCII = true;
    var b = new slice<byte>(new ReadOnlySpan<byte>((byte*)(uintptr)(buf), (int)(n)));
    foreach (var (_, x) in b) {
        if (x >= 0x80) {
            isASCII = false;
            break;
        }
    }
    if (!isASCII) {
        ref var m = ref heap(new uint32(), out var Ꮡm);
        var isConsole = stdcall2(_GetConsoleMode, handle, (uintptr)Ꮡm) != 0;
        // If this is a console output, various non-unicode code pages can be in use.
        // Use the dedicated WriteConsole call to ensure unicode is printed correctly.
        if (isConsole) {
            return (int32)writeConsole(handle, buf, n);
        }
    }
    ref var written = ref heap(new uint32(), out var Ꮡwritten);
    stdcall5(_WriteFile, handle, (uintptr)buf, (uintptr)n, (uintptr)Ꮡwritten, 0);
    return (int32)written;
}

internal static array<uint16> utf16ConsoleBack = new(1000);
internal static ж<mutex> Ꮡutf16ConsoleBackLock = new StandardBox<mutex>(new mutex(nil));
internal static ref mutex utf16ConsoleBackLock => ref Ꮡutf16ConsoleBackLock.Value;

// writeConsole writes bufLen bytes from buf to the console File.
// It returns the number of bytes written.
internal static unsafe nint writeConsole(uintptr handle, @unsafe.Pointer buf, int32 bufLen) {
    const uint16 surr2 = /* (surrogateMin + surrogateMax + 1) / 2 */ 56320;
    // Do not use defer for unlock. May cause issues when printing a panic.
    @lock(Ꮡutf16ConsoleBackLock);
    ref var b = ref heap<slice<byte>>(out var Ꮡb);
    b = new slice<byte>(new ReadOnlySpan<byte>((byte*)(uintptr)(buf), (int)(bufLen)));
    @string s = ~Ꮡb.Reinterpret<slice<byte>, @string>();
    var utf16tmp = utf16ConsoleBack[..];
    nint total = len(s);
    nint w = 0;
    foreach (var (_, rᴛ1) in s) {
        var r = rᴛ1;

        if (w >= len(utf16tmp) - 2) {
            writeConsoleUTF16(handle, utf16tmp[..(int)(w)]);
            w = 0;
        }
        if (r < 0x10000){
            utf16tmp[w] = (uint16)r;
            w++;
        } else {
            r -= 0x10000;
            utf16tmp[w] = (uint16)((uint16)surrogateMin + (uint16)((uint16)((r >> (int)(10))) & 0x3ff));
            utf16tmp[w + 1] = (uint16)(surr2 + (uint16)((uint16)r & 0x3ff));
            w += 2;
        }
    }
    writeConsoleUTF16(handle, utf16tmp[..(int)(w)]);
    unlock(Ꮡutf16ConsoleBackLock);
    return total;
}

// writeConsoleUTF16 is the dedicated windows calls that correctly prints
// to the console regardless of the current code page. Input is utf-16 code points.
// The handle must be a console handle.
internal static void writeConsoleUTF16(uintptr handle, slice<uint16> b) {
    var l = (uint32)len(b);
    if (l == 0) {
        return;
    }
    ref var written = ref heap(new uint32(), out var Ꮡwritten);
    stdcall5(_WriteConsoleW,
        handle,
        (uintptr)Ꮡ(b, 0),
        (uintptr)l,
        (uintptr)Ꮡwritten,
        0);
    return;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeSemasleepWaitˢ = "runtime.semasleep wait_abandoned"u8;
internal static readonly @string runtimeSemasleepWaitˢ2 = "runtime.semasleep wait_failed"u8;
internal static readonly @string runtimeSemasleepˢ = "runtime.semasleep unexpected"u8;

//go:nosplit
internal static int32 semasleep(int64 ns) {
    uintptr _WAIT_ABANDONED = 0x00000080;
    UntypedInt _WAIT_OBJECT_0 = 0x00000000;
    uintptr _WAIT_TIMEOUT = 0x00000102;
    uintptr _WAIT_FAILED = unchecked((uintptr)0xFFFFFFFF);
    uintptr result = default!;
    if (ns < 0){
        result = stdcall2(_WaitForSingleObject, (~(~getg()).m).waitsema, (uintptr)_INFINITE);
    } else {
        var start = nanotime();
        var elapsed = (int64)0;
        while (ᐧ) {
            var ms = (int64)timediv(ns - elapsed, 1000000, nil);
            if (ms == 0) {
                ms = 1;
            }
            result = stdcall4(_WaitForMultipleObjects, 2,
                (uintptr)Ꮡ(new uintptr[]{(~(~getg()).m).waitsema, (~(~getg()).m).resumesema}.array()),
                0, (uintptr)ms);
            if (result != (uintptr)(_WAIT_OBJECT_0 + 1)) {
                // Not a suspend/resume event
                break;
            }
            elapsed = nanotime() - start;
            if (elapsed >= ns) {
                return -1;
            }
        }
    }
    var exprᴛ1 = result;
    if (exprᴛ1 == _WAIT_OBJECT_0) {
        return 0;
    }
    if (exprᴛ1 == _WAIT_TIMEOUT) {
        return -1;
    }
    if (exprᴛ1 == _WAIT_ABANDONED) {
        systemstack(() => {
            // Signaled
            @throw(runtimeSemasleepWaitˢ);
        });
    }
    else if (exprᴛ1 == _WAIT_FAILED) {
        systemstack(() => {
            print((@string)"runtime: waitforsingleobject wait_failed; errno="u8, getlasterror(), (@string)"\n"u8);
            @throw(runtimeSemasleepWaitˢ2);
        });
    }
    else { /* default: */
        systemstack(() => {
            print((@string)"runtime: waitforsingleobject unexpected; result="u8, result, (@string)"\n"u8);
            @throw(runtimeSemasleepˢ);
        });
    }

    return -1; // unreachable
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeSemawakeupˢ = "runtime.semawakeup"u8;

//go:nosplit
internal static void semawakeup(ж<m> Ꮡmp) {
    ref var mp = ref Ꮡmp.DerefOrNull();

    if (stdcall1(_SetEvent, mp.waitsema) == 0) {
        systemstack(() => {
            print((@string)"runtime: setevent failed; errno="u8, getlasterror(), (@string)"\n"u8);
            @throw(runtimeSemawakeupˢ);
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeSemacreateˢ = "runtime.semacreate"u8;

//go:nosplit
internal static void semacreate(ref m mp) {
    if (mp.waitsema != 0) {
        return;
    }
    mp.waitsema = stdcall4(_CreateEventA, 0, 0, 0, 0);
    if (mp.waitsema == 0) {
        systemstack(() => {
            print((@string)"runtime: createevent failed; errno="u8, getlasterror(), (@string)"\n"u8);
            @throw(runtimeSemacreateˢ);
        });
    }
    mp.resumesema = stdcall4(_CreateEventA, 0, 0, 0, 0);
    if (mp.resumesema == 0) {
        systemstack(() => {
            print((@string)"runtime: createevent failed; errno="u8, getlasterror(), (@string)"\n"u8);
            @throw(runtimeSemacreateˢ);
        });
        stdcall1(_CloseHandle, mp.waitsema);
        mp.waitsema = 0;
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeNewosprocˢ = "runtime.newosproc"u8;

// May run with m.p==nil, so write barriers are not allowed. This
// function is called by newosproc0, so it is also required to
// operate without stack guards.
//
//go:nowritebarrierrec
//go:nosplit
internal static void newosproc(ж<m> Ꮡmp) {
    // We pass 0 for the stack size to use the default for this binary.
    var thandle = stdcall6(_CreateThread, 0, 0,
        abi.FuncPCABI0(tstart_stdcall), (uintptr)Ꮡmp,
        0, 0);
    if (thandle == 0) {
        if (atomic.Load(Ꮡexiting) != 0) {
            // CreateThread may fail if called
            // concurrently with ExitProcess. If this
            // happens, just freeze this thread and let
            // the process exit. See issue #18253.
            @lock(Ꮡdeadlock);
            @lock(Ꮡdeadlock);
        }
        print((@string)"runtime: failed to create new OS thread (have "u8, mcount(), (@string)" already; errno="u8, getlasterror(), (@string)")\n"u8);
        @throw(runtimeNewosprocˢ);
    }
    // Close thandle to avoid leaking the thread object if it exits.
    stdcall1(_CloseHandle, thandle);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badNewosproc0ˢ = "bad newosproc0"u8;

// Used by the C library build mode. On Linux this function would allocate a
// stack, but that's not necessary for Windows. No stack guards are present
// and the GC has not been initialized, so write barriers will fail.
//
//go:nowritebarrierrec
//go:nosplit
internal static void newosproc0(ref m mp, @unsafe.Pointer stk) {
    // TODO: this is completely broken. The args passed to newosproc0 (in asm_amd64.s)
    // are stacksize and function, not *m and stack.
    // Check os_linux.go for an implementation that might actually work.
    @throw(badNewosproc0ˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string exitThreadˢ = "exitThread"u8;

internal static void exitThread(ref atomic.Uint32 wait) {
    // We should never reach exitThread on Windows because we let
    // the OS clean up threads.
    @throw(exitThreadˢ);
}

// Called to initialize a new m (including the bootstrap m).
// Called on the parent thread (main thread in case of bootstrap), can allocate memory.
internal static void mpreinit(ref m mp) {
}

//go:nosplit
internal static void sigsave(ж<sigset> Ꮡp) {
}

//go:nosplit
internal static void msigrestore(sigset sigmask) {
}

//go:nosplit
//go:nowritebarrierrec
internal static void clearSignalHandlers() {
}

//go:nosplit
internal static void sigblock(bool exiting) {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeMinitˢ = "runtime.minit: duplicatehandle failed"u8;
internal static readonly @string createWaitableTimerExˢ = "CreateWaitableTimerEx when creating timer failed"u8;
internal static readonly @string virtualQueryForStackBaseˢ = "VirtualQuery for stack base failed"u8;
internal static readonly @string badG0Stackˢ = "bad g0 stack"u8;

// Called to initialize a new m (including the bootstrap m).
// Called on the new thread, cannot allocate Go memory.
internal static void minit() {
    ref var thandle = ref heap(new uintptr(), out var Ꮡthandle);
    if (stdcall7(_DuplicateHandle, currentProcess, currentThread, currentProcess, (uintptr)@unsafe.Pointer.FromBox(Ꮡthandle), 0, 0, _DUPLICATE_SAME_ACCESS) == 0) {
        print((@string)"runtime.minit: duplicatehandle failed; errno="u8, getlasterror(), (@string)"\n"u8);
        @throw(runtimeMinitˢ);
    }
    var mp = getg().Value.m;
    @lock(mp.of(m.ᏑthreadLock));
    mp.Value.thread = thandle;
    mp.Value.procid = (uint64)stdcall0(_GetCurrentThreadId);
    // Configure usleep timer, if possible.
    if ((~mp).highResTimer == 0 && haveHighResTimer) {
        mp.Value.highResTimer = createHighResTimer();
        if ((~mp).highResTimer == 0) {
            print((@string)"runtime: CreateWaitableTimerEx failed; errno="u8, getlasterror(), (@string)"\n"u8);
            @throw(createWaitableTimerExˢ);
        }
    }
    if ((~mp).waitIocpHandle == 0 && haveHighResSleep) {
        mp.Value.waitIocpTimer = createHighResTimer();
        if ((~mp).waitIocpTimer == 0) {
            print((@string)"runtime: CreateWaitableTimerEx failed; errno="u8, getlasterror(), (@string)"\n"u8);
            @throw(createWaitableTimerExˢ);
        }
        uintptr GENERIC_ALL = 0x10000000;
        var errno = stdcall3(_NtCreateWaitCompletionPacket, (uintptr)@unsafe.Pointer.FromBox(mp.of(m.ᏑwaitIocpHandle)), GENERIC_ALL, 0);
        if ((~mp).waitIocpHandle == 0) {
            print((@string)"runtime: NtCreateWaitCompletionPacket failed; errno="u8, errno, (@string)"\n"u8);
            @throw("NtCreateWaitCompletionPacket failed"u8);
        }
    }
    unlock(mp.of(m.ᏑthreadLock));
    // Query the true stack base from the OS. Currently we're
    // running on a small assumed stack.
    ref var mbi = ref heap(new memoryBasicInformation(), out var Ꮡmbi);
    var res = stdcall3(_VirtualQuery, (uintptr)Ꮡmbi, (uintptr)Ꮡmbi, /* unsafe.Sizeof(mbi) */ (uintptr)48);
    if (res == 0) {
        print((@string)"runtime: VirtualQuery failed; errno="u8, getlasterror(), (@string)"\n"u8);
        @throw(virtualQueryForStackBaseˢ);
    }
    // The system leaves an 8K PAGE_GUARD region at the bottom of
    // the stack (in theory VirtualQuery isn't supposed to include
    // that, but it does). Add an additional 8K of slop for
    // calling C functions that don't have stack checks and for
    // lastcontinuehandler. We shouldn't be anywhere near this
    // bound anyway.
    var @base = mbi.allocationBase + ((uintptr)16 << (int)(10));
    // Sanity check the stack bounds.
    var g0 = getg();
    if (@base > (~g0).stack.hi || (~g0).stack.hi - @base > ((uintptr)64 << (int)(20))) {
        print((@string)"runtime: g0 stack ["u8, ((Δhex)(uint64)@base), (@string)","u8, ((Δhex)(uint64)(~g0).stack.hi), (@string)")\n"u8);
        @throw(badG0Stackˢ);
    }
    g0.Value.stack.lo = @base;
    g0.Value.stackguard0 = (~g0).stack.lo + (uintptr)stackGuard;
    g0.Value.stackguard1 = g0.Value.stackguard0;
    // Sanity check the SP.
    stackcheck();
}

// Called from dropm to undo the effect of an minit.
//
//go:nosplit
internal static void unminit() {
    var mp = getg().Value.m;
    @lock(mp.of(m.ᏑthreadLock));
    if ((~mp).thread != 0) {
        stdcall1(_CloseHandle, (~mp).thread);
        mp.Value.thread = 0;
    }
    unlock(mp.of(m.ᏑthreadLock));
    mp.Value.procid = 0;
}

// Called from exitm, but not from drop, to undo the effect of thread-owned
// resources in minit, semacreate, or elsewhere. Do not take locks after calling this.
//
//go:nosplit
internal static void mdestroy(ref m mp) {
    if (mp.highResTimer != 0) {
        stdcall1(_CloseHandle, mp.highResTimer);
        mp.highResTimer = 0;
    }
    if (mp.waitIocpTimer != 0) {
        stdcall1(_CloseHandle, mp.waitIocpTimer);
        mp.waitIocpTimer = 0;
    }
    if (mp.waitIocpHandle != 0) {
        stdcall1(_CloseHandle, mp.waitIocpHandle);
        mp.waitIocpHandle = 0;
    }
    if (mp.waitsema != 0) {
        stdcall1(_CloseHandle, mp.waitsema);
        mp.waitsema = 0;
    }
    if (mp.resumesema != 0) {
        stdcall1(_CloseHandle, mp.resumesema);
        mp.resumesema = 0;
    }
}

// asmstdcall_trampoline calls asmstdcall converting from Go to C calling convention.
internal static partial void asmstdcall_trampoline(@unsafe.Pointer args);

// stdcall_no_g calls asmstdcall on os stack without using g.
//
//go:nosplit
internal static uintptr stdcall_no_g(stdFunction fn, nint n, uintptr args) {
    ref var libcall = ref heap<libcall>(out var Ꮡlibcall);
    libcall = new libcall(
        fn: (uintptr)(@unsafe.Pointer)fn,
        n: (uintptr)n,
        args: args
    );
    asmstdcall_trampoline((uintptr)noescape(@unsafe.Pointer.FromPinnedBox(Ꮡlibcall)));
    System.GC.KeepAlive(Ꮡlibcall);
    return libcall.r1;
}

// Calling stdcall on os stack.
// May run during STW, so write barriers are not allowed.
//
//go:nowritebarrier
//go:nosplit
internal static uintptr stdcall(stdFunction fn) {
    var gp = getg();
    var mp = gp.Value.m;
    mp.Value.libcall.fn = (uintptr)(@unsafe.Pointer)fn;
    var resetLibcall = false;
    if ((~mp).profilehz != 0 && (~mp).libcallsp == 0) {
        // leave pc/sp for cpu profiler
        mp.of(m.Ꮡlibcallg).set(gp);
        mp.Value.libcallpc = getcallerpc();
        // sp must be the last, because once async cpu profiler finds
        // all three values to be non-zero, it will use them
        mp.Value.libcallsp = getcallersp();
        resetLibcall = true; // See comment in sys_darwin.go:libcCall
    }
    asmcgocall(asmstdcallAddr, @unsafe.Pointer.FromPinnedBox(mp.of(m.Ꮡlibcall)));
    if (resetLibcall) {
        mp.Value.libcallsp = 0;
    }
    return (~mp).libcall.r1;
}

//go:nosplit
internal static uintptr stdcall0(stdFunction fn) {
    var mp = getg().Value.m;
    mp.Value.libcall.n = 0;
    mp.Value.libcall.args = 0;
    return stdcall(fn);
}

//go:nosplit
//go:cgo_unsafe_args
internal static uintptr stdcall1(stdFunction fn, uintptr a0ʗp) {
    ref var a0 = ref heap(a0ʗp, out var Ꮡa0);

    var mp = getg().Value.m;
    mp.Value.libcall.n = 1;
    mp.Value.libcall.args = (uintptr)(uintptr)noescape(@unsafe.Pointer.FromBox(Ꮡa0));
    return stdcall(fn);
}

//go:nosplit
//go:cgo_unsafe_args
internal static uintptr stdcall2(stdFunction fn, uintptr a0ʗp, uintptr a1) {
    ref var a0 = ref heap(a0ʗp, out var Ꮡa0);

    var mp = getg().Value.m;
    mp.Value.libcall.n = 2;
    mp.Value.libcall.args = (uintptr)(uintptr)noescape(@unsafe.Pointer.FromBox(Ꮡa0));
    return stdcall(fn);
}

//go:nosplit
//go:cgo_unsafe_args
internal static uintptr stdcall3(stdFunction fn, uintptr a0ʗp, uintptr a1, uintptr a2) {
    ref var a0 = ref heap(a0ʗp, out var Ꮡa0);

    var mp = getg().Value.m;
    mp.Value.libcall.n = 3;
    mp.Value.libcall.args = (uintptr)(uintptr)noescape(@unsafe.Pointer.FromBox(Ꮡa0));
    return stdcall(fn);
}

//go:nosplit
//go:cgo_unsafe_args
internal static uintptr stdcall4(stdFunction fn, uintptr a0ʗp, uintptr a1, uintptr a2, uintptr a3) {
    ref var a0 = ref heap(a0ʗp, out var Ꮡa0);

    var mp = getg().Value.m;
    mp.Value.libcall.n = 4;
    mp.Value.libcall.args = (uintptr)(uintptr)noescape(@unsafe.Pointer.FromBox(Ꮡa0));
    return stdcall(fn);
}

//go:nosplit
//go:cgo_unsafe_args
internal static uintptr stdcall5(stdFunction fn, uintptr a0ʗp, uintptr a1, uintptr a2, uintptr a3, uintptr a4) {
    ref var a0 = ref heap(a0ʗp, out var Ꮡa0);

    var mp = getg().Value.m;
    mp.Value.libcall.n = 5;
    mp.Value.libcall.args = (uintptr)(uintptr)noescape(@unsafe.Pointer.FromBox(Ꮡa0));
    return stdcall(fn);
}

//go:nosplit
//go:cgo_unsafe_args
internal static uintptr stdcall6(stdFunction fn, uintptr a0ʗp, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5) {
    ref var a0 = ref heap(a0ʗp, out var Ꮡa0);

    var mp = getg().Value.m;
    mp.Value.libcall.n = 6;
    mp.Value.libcall.args = (uintptr)(uintptr)noescape(@unsafe.Pointer.FromBox(Ꮡa0));
    return stdcall(fn);
}

//go:nosplit
//go:cgo_unsafe_args
internal static uintptr stdcall7(stdFunction fn, uintptr a0ʗp, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6) {
    ref var a0 = ref heap(a0ʗp, out var Ꮡa0);

    var mp = getg().Value.m;
    mp.Value.libcall.n = 7;
    mp.Value.libcall.args = (uintptr)(uintptr)noescape(@unsafe.Pointer.FromBox(Ꮡa0));
    return stdcall(fn);
}

//go:nosplit
//go:cgo_unsafe_args
internal static uintptr stdcall8(stdFunction fn, uintptr a0ʗp, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6, uintptr a7) {
    ref var a0 = ref heap(a0ʗp, out var Ꮡa0);

    var mp = getg().Value.m;
    mp.Value.libcall.n = 8;
    mp.Value.libcall.args = (uintptr)(uintptr)noescape(@unsafe.Pointer.FromBox(Ꮡa0));
    return stdcall(fn);
}

// These must run on the system stack only.

//go:nosplit
internal static void osyield_no_g() {
    stdcall_no_g(_SwitchToThread, 0, 0);
}

//go:nosplit
internal static void osyield() {
    systemstack(() => {
        stdcall0(_SwitchToThread);
    });
}

//go:nosplit
internal static void usleep_no_g(uint32 us) {
    var timeout = (uintptr)us / 1000; // ms units
    ref var args = ref heap<array<uintptr>>(out var Ꮡargs);
    args = new uintptr[]{_INVALID_HANDLE_VALUE, timeout}.array();
    stdcall_no_g(_WaitForSingleObject, len(args), (uintptr)(uintptr)noescape(@unsafe.Pointer.FromBox(Ꮡargs.at<uintptr>(0))));
}

//go:nosplit
internal static void usleep(uint32 us) {
    systemstack(() => {
        uintptr h = default!;
        uintptr timeout = default!;
        // If the high-res timer is available and its handle has been allocated for this m, use it.
        // Otherwise fall back to the low-res one, which doesn't need a handle.
        if (haveHighResTimer && (~(~getg()).m).highResTimer != 0){
            h = getg().Value.m.Value.highResTimer;
            ref var dt = ref heap<int64>(out var Ꮡdt);
            dt = -10 * (int64)us; // relative sleep (negative), 100ns units
            stdcall6(_SetWaitableTimer, h, (uintptr)Ꮡdt, 0, 0, 0, 0);
            timeout = _INFINITE;
        } else {
            h = _INVALID_HANDLE_VALUE;
            timeout = (uintptr)us / 1000; // ms units
        }
        stdcall2(_WaitForSingleObject, h, timeout);
    });
}

internal static uintptr ctrlHandler(uint32 _type) {
    uint32 s = default!;
    var exprᴛ1 = _type;
    if (exprᴛ1 == _CTRL_C_EVENT || exprᴛ1 == _CTRL_BREAK_EVENT) {
        s = _SIGINT;
    }
    else if (exprᴛ1 == _CTRL_CLOSE_EVENT || exprᴛ1 == _CTRL_LOGOFF_EVENT || exprᴛ1 == _CTRL_SHUTDOWN_EVENT) {
        s = _SIGTERM;
    }
    else { /* default: */
        return 0;
    }

    if (sigsend(s)) {
        if (s == _SIGTERM) {
            // Windows terminates the process after this handler returns.
            // Block indefinitely to give signal handlers a chance to clean up,
            // but make sure to be properly parked first, so the rest of the
            // program can continue executing.
            block();
        }
        return 1;
    }
    return 0;
}

// called from zcallback_windows_*.s to sys_windows_*.s
internal static partial void callbackasm1();

internal static ж<uintptr> Ꮡprofiletimer = new StandardBox<uintptr>(default(uintptr));
internal static ref uintptr profiletimer => ref Ꮡprofiletimer.Value;

internal static void profilem(ж<m> Ꮡmp, uintptr thread) {
    // Align Context to 16 bytes.
    ж<context> c = default!;
    ref var cbuf = ref heap(new array<byte>(1247), out var Ꮡcbuf);
    c = (ж<context>)(uintptr)((@unsafe.Pointer)((uintptr)(((uintptr)Ꮡcbuf.at<byte>(15)) & ~(uintptr)15)));
    c.Value.contextflags = _CONTEXT_CONTROL;
    stdcall2(_GetThreadContext, thread, (uintptr)c);
    var gp = gFromSP(ref (Ꮡmp).DerefOrNull(), c.sp());
    sigprof(c.ip(), c.sp(), c.lr(), gp, Ꮡmp);
}

internal static ж<g> gFromSP(ref m mp, uintptr sp) {
    {
        var gp = mp.g0; if (gp != nil && (~gp).stack.lo < sp && sp < (~gp).stack.hi) {
            return gp;
        }
    }
    {
        var gp = mp.gsignal; if (gp != nil && (~gp).stack.lo < sp && sp < (~gp).stack.hi) {
            return gp;
        }
    }
    {
        var gp = mp.curg; if (gp != nil && (~gp).stack.lo < sp && sp < (~gp).stack.hi) {
            return gp;
        }
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string duplicatehandleFailedˢ = "duplicatehandle failed"u8;

internal static void profileLoop() {
    stdcall2(_SetThreadPriority, currentThread, _THREAD_PRIORITY_HIGHEST);
    while (ᐧ) {
        stdcall2(_WaitForSingleObject, profiletimer, _INFINITE);
        var first = (ж<m>)(uintptr)(atomic.Loadp(@unsafe.Pointer.FromBox(Ꮡallm)));
        for (var mp = first; mp != nil; mp = mp.Value.alllink) {
            if (mp == (~getg()).m) {
                // Don't profile ourselves.
                continue;
            }
            @lock(mp.of(m.ᏑthreadLock));
            // Do not profile threads blocked on Notes,
            // this includes idle worker threads,
            // idle timer thread, idle heap scavenger, etc.
            if ((~mp).thread == 0 || (~mp).profilehz == 0 || (~mp).blocked) {
                unlock(mp.of(m.ᏑthreadLock));
                continue;
            }
            // Acquire our own handle to the thread.
            ref var thread = ref heap(new uintptr(), out var Ꮡthread);
            if (stdcall7(_DuplicateHandle, currentProcess, (~mp).thread, currentProcess, (uintptr)@unsafe.Pointer.FromBox(Ꮡthread), 0, 0, _DUPLICATE_SAME_ACCESS) == 0) {
                print((@string)"runtime: duplicatehandle failed; errno="u8, getlasterror(), (@string)"\n"u8);
                @throw(duplicatehandleFailedˢ);
            }
            unlock(mp.of(m.ᏑthreadLock));
            // mp may exit between the DuplicateHandle
            // above and the SuspendThread. The handle
            // will remain valid, but SuspendThread may
            // fail.
            if ((int32)stdcall1(_SuspendThread, thread) == -1) {
                // The thread no longer exists.
                stdcall1(_CloseHandle, thread);
                continue;
            }
            if ((~mp).profilehz != 0 && !(~mp).blocked) {
                // Pass the thread handle in case mp
                // was in the process of shutting down.
                profilem(mp, thread);
            }
            stdcall1(_ResumeThread, thread);
            stdcall1(_CloseHandle, thread);
        }
    }
}

internal static void setProcessCPUProfiler(int32 hz) {
    if (profiletimer == 0) {
        uintptr timer = default!;
        if (haveHighResTimer){
            timer = createHighResTimer();
        } else {
            timer = stdcall3(_CreateWaitableTimerA, 0, 0, 0);
        }
        atomic.Storeuintptr(Ꮡprofiletimer, timer);
        newm(profileLoop, nil, -1);
    }
}

internal static void setThreadCPUProfiler(int32 hz) {
    var ms = (int32)0;
    ref var due = ref heap<int64>(out var Ꮡdue);
    due = ~(int64)(~(uint64)(((uint64)1 << (int)(63))));
    if (hz > 0) {
        ms = 1000 / hz;
        if (ms == 0) {
            ms = 1;
        }
        due = (int64)ms * -10000;
    }
    stdcall6(_SetWaitableTimer, profiletimer, (uintptr)Ꮡdue, (uintptr)ms, 0, 0, 0);
    atomic.Store((~getg()).m.of(m.Ꮡprofilehz).Reinterpret<int32, uint32>(), (uint32)hz);
}

internal const bool preemptMSupported = true;

// suspendLock protects simultaneous SuspendThread operations from
// suspending each other.
internal static ж<mutex> ᏑsuspendLock = new StandardBox<mutex>(new mutex(nil));
internal static ref mutex suspendLock => ref ᏑsuspendLock.Value;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string selfPreemptˢ = "self-preempt"u8;
internal static readonly @string runtimePreemptMˢ = "runtime.preemptM: duplicatehandle failed"u8;
internal static readonly @string unsupportedArchitectureˢ = "unsupported architecture"u8;

internal static void preemptM(ж<m> Ꮡmp) {
    ref var mp = ref Ꮡmp.DerefOrNull();

    if (Ꮡmp == (~getg()).m) {
        @throw(selfPreemptˢ);
    }
    // Synchronize with external code that may try to ExitProcess.
    if (!atomic.Cas(Ꮡmp.of(m.ᏑpreemptExtLock), 0, 1)) {
        // External code is running. Fail the preemption
        // attempt.
        Ꮡmp.of(m.ᏑpreemptGen).Add(1);
        return;
    }
    // Acquire our own handle to mp's thread.
    @lock(Ꮡmp.of(m.ᏑthreadLock));
    if (mp.thread == 0) {
        // The M hasn't been minit'd yet (or was just unminit'd).
        unlock(Ꮡmp.of(m.ᏑthreadLock));
        atomic.Store(Ꮡmp.of(m.ᏑpreemptExtLock), 0);
        Ꮡmp.of(m.ᏑpreemptGen).Add(1);
        return;
    }
    ref var thread = ref heap(new uintptr(), out var Ꮡthread);
    if (stdcall7(_DuplicateHandle, currentProcess, mp.thread, currentProcess, (uintptr)@unsafe.Pointer.FromBox(Ꮡthread), 0, 0, _DUPLICATE_SAME_ACCESS) == 0) {
        print((@string)"runtime.preemptM: duplicatehandle failed; errno="u8, getlasterror(), (@string)"\n"u8);
        @throw(runtimePreemptMˢ);
    }
    unlock(Ꮡmp.of(m.ᏑthreadLock));
    // Prepare thread context buffer. This must be aligned to 16 bytes.
    ж<context> c = default!;
    ref var cbuf = ref heap(new array<byte>(1247), out var Ꮡcbuf);
    c = (ж<context>)(uintptr)((@unsafe.Pointer)((uintptr)(((uintptr)Ꮡcbuf.at<byte>(15)) & ~(uintptr)15)));
    c.Value.contextflags = _CONTEXT_CONTROL;
    // Serialize thread suspension. SuspendThread is asynchronous,
    // so it's otherwise possible for two threads to suspend each
    // other and deadlock. We must hold this lock until after
    // GetThreadContext, since that blocks until the thread is
    // actually suspended.
    @lock(ᏑsuspendLock);
    // Suspend the thread.
    if ((int32)stdcall1(_SuspendThread, thread) == -1) {
        unlock(ᏑsuspendLock);
        stdcall1(_CloseHandle, thread);
        atomic.Store(Ꮡmp.of(m.ᏑpreemptExtLock), 0);
        // The thread no longer exists. This shouldn't be
        // possible, but just acknowledge the request.
        Ꮡmp.of(m.ᏑpreemptGen).Add(1);
        return;
    }
    // We have to be very careful between this point and once
    // we've shown mp is at an async safe-point. This is like a
    // signal handler in the sense that mp could have been doing
    // anything when we stopped it, including holding arbitrary
    // locks.
    // We have to get the thread context before inspecting the M
    // because SuspendThread only requests a suspend.
    // GetThreadContext actually blocks until it's suspended.
    stdcall2(_GetThreadContext, thread, (uintptr)c);
    unlock(ᏑsuspendLock);
    // Does it want a preemption and is it safe to preempt?
    var gp = gFromSP(ref (Ꮡmp).DerefOrNull(), c.sp());
    if (gp != nil && wantAsyncPreempt(gp)) {
        {
            var (ok, newpc) = isAsyncSafePoint(gp, c.ip(), c.sp(), c.lr()); if (ok) {
                // Inject call to asyncPreempt
                var targetPC = abi.FuncPCABI0(asyncPreempt);
                var exprᴛ1 = GOARCH;
                if (exprᴛ1 == "386"u8 || exprᴛ1 == "amd64"u8) {
                    var sp = c.sp();
                    sp -= goarch.PtrSize;
                    ((ж<uintptr>)(uintptr)((@unsafe.Pointer)sp)).Value = newpc;
                    c.set_sp(sp);
                    c.set_ip(targetPC);
                }
                else if (exprᴛ1 == "arm"u8) {
                    var sp = c.sp();
                    sp -= goarch.PtrSize;
                    c.set_sp(sp);
                    ((ж<uint32>)(uintptr)((@unsafe.Pointer)sp)).Value = (uint32)c.lr();
                    c.set_lr(newpc - 1);
                    c.set_ip(targetPC);
                }
                else if (exprᴛ1 == "arm64"u8) {
                    var sp = c.sp() - 16; // SP needs 16-byte alignment
                    c.set_sp(sp);
                    ((ж<uint64>)(uintptr)((@unsafe.Pointer)sp)).Value = (uint64)c.lr();
                    c.set_lr(newpc);
                    c.set_ip(targetPC);
                }
                else { /* default: */
                    @throw(unsupportedArchitectureˢ);
                }

                // Make it look like the thread called targetPC.
                // Push LR. The injected call is responsible
                // for restoring LR. gentraceback is aware of
                // this extra slot. See sigctxt.pushCall in
                // signal_arm.go, which is similar except we
                // subtract 1 from IP here.
                // Push LR. The injected call is responsible
                // for restoring LR. gentraceback is aware of
                // this extra slot. See sigctxt.pushCall in
                // signal_arm64.go.
                stdcall2(_SetThreadContext, thread, (uintptr)c);
            }
        }
    }
    atomic.Store(Ꮡmp.of(m.ᏑpreemptExtLock), 0);
    // Acknowledge the preemption.
    Ꮡmp.of(m.ᏑpreemptGen).Add(1);
    stdcall1(_ResumeThread, thread);
    stdcall1(_CloseHandle, thread);
}

// osPreemptExtEnter is called before entering external code that may
// call ExitProcess.
//
// This must be nosplit because it may be called from a syscall with
// untyped stack slots, so the stack must not be grown or scanned.
//
//go:nosplit
internal static void osPreemptExtEnter(ж<m> Ꮡmp) {
    while (!atomic.Cas(Ꮡmp.of(m.ᏑpreemptExtLock), 0, 1)) {
        // An asynchronous preemption is in progress. It's not
        // safe to enter external code because it may call
        // ExitProcess and deadlock with SuspendThread.
        // Ideally we would do the preemption ourselves, but
        // can't since there may be untyped syscall arguments
        // on the stack. Instead, just wait and encourage the
        // SuspendThread APC to run. The preemption should be
        // done shortly.
        osyield();
    }
}

// Asynchronous preemption is now blocked.

// osPreemptExtExit is called after returning from external code that
// may call ExitProcess.
//
// See osPreemptExtEnter for why this is nosplit.
//
//go:nosplit
internal static void osPreemptExtExit(ж<m> Ꮡmp) {
    atomic.Store(Ꮡmp.of(m.ᏑpreemptExtLock), 0);
}

} // end runtime_package
