// Copyright 2020 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

global using Tsize_t = ulong;
global using syscallErrno = go.syscall_package.Errno;
global using @long = int;
global using @ulong = uint;
global using Timeval = go.modernc.org.libc.time_package.Timevalᴛ1;
global using Timespec = go.modernc.org.libc_package.Timespecᴛ1;
global using _ino_t = ushort;
global using _dev_t = uint;
global using _stat64 = go.modernc.org.libc_package._stat64ᴛ1;

namespace go.modernc.org;

using errors = errors_package;
using fmt = fmt_package;
using math = math_package;
using mbits = go.math.bits_package;
using os = os_package;
using exec = go.os.exec_package;
using user = go.os.user_package;
using filepath = path.filepath_package;
using debug = go.runtime.debug_package;
using strings = strings_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
using gotime = time_package;
using unicode = unicode_package;
using utf16 = go.unicode.utf16_package;
using @unsafe = unsafe_package;
using strftime = github.com.ncruces.go_strftime.strftime_package;
using windows = golang.org.x.sys.windows_package;
using errno = go.modernc.org.libc.errno_package;
using fcntl = go.modernc.org.libc.fcntl_package;
using limits = go.modernc.org.libc.limits_package;
using stdio = go.modernc.org.libc.stdio_package;
using stat = go.modernc.org.libc.sys.stat_package;
using types = go.modernc.org.libc.sys.types_package;
using time = go.modernc.org.libc.time_package;
using unistd = go.modernc.org.libc.unistd_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using FieldOffsetAttribute = global::System.Runtime.InteropServices.FieldOffsetAttribute;
using LayoutKind = global::System.Runtime.InteropServices.LayoutKind;
using StructLayoutAttribute = global::System.Runtime.InteropServices.StructLayoutAttribute;
using fs = go.io.fs_package;
using github.com.ncruces.go_strftime;
using go.arrays;
using go.golib;
using go.io;
using go.modernc.org.libc;
using go.modernc.org.libc.sys;
using go.os;
using go.runtime;
using go.sync;
using go.unicode;
using golang.org.x.sys;
using io = io_package;
using path;
using ꓸꓸꓸany = Span<any>;
using static libc_package;

partial class libc_package {

public static readonly ж<uintptr> ᏑX__imp__environ = new StaticBox<uintptr>(default(uintptr));
public static ref uintptr X__imp__environ => ref ((StaticBox<uintptr>)ᏑX__imp__environ).ValueSlot;
internal static void initᴛX__imp__environ() { X__imp__environ = EnvironP(); }
public static readonly ж<uintptr> ᏑX__imp__wenviron = new StaticBox<uintptr>(default(uintptr));
public static ref uintptr X__imp__wenviron => ref ((StaticBox<uintptr>)ᏑX__imp__wenviron).ValueSlot;
internal static void initᴛX__imp__wenviron() { X__imp__wenviron = (uintptr)@unsafe.Pointer.FromBox(Ꮡwenviron); }
public static readonly ж<uintptr> ᏑX_imp___environ = new StaticBox<uintptr>(default(uintptr));
public static ref uintptr X_imp___environ => ref ((StaticBox<uintptr>)ᏑX_imp___environ).ValueSlot;
internal static void initᴛX_imp___environ() { X_imp___environ = EnvironP(); }
public static readonly ж<uintptr> ᏑX_imp___wenviron = new StaticBox<uintptr>(default(uintptr));
public static ref uintptr X_imp___wenviron => ref ((StaticBox<uintptr>)ᏑX_imp___wenviron).ValueSlot;
internal static void initᴛX_imp___wenviron() { X_imp___wenviron = (uintptr)@unsafe.Pointer.FromBox(Ꮡwenviron); }
public static readonly ж<array<stdioꓸFILE>> ᏑX_iob = new StaticBox<array<stdioꓸFILE>>(new array<stdioꓸFILE>(20, () => new()));
public static ref array<stdioꓸFILE> X_iob => ref ((StaticBox<array<stdioꓸFILE>>)ᏑX_iob).ValueSlot;
public static readonly ж<array<byte>> ᏑXin6addr_any = new StaticBox<array<byte>>(new array<byte>(16));
public static ref array<byte> Xin6addr_any => ref ((StaticBox<array<byte>>)ᏑXin6addr_any).ValueSlot;
public static readonly ж<@long> ᏑXtimezone = new StaticBox<@long>(default(@long));
public static ref @long Xtimezone => ref ((StaticBox<@long>)ᏑXtimezone).ValueSlot;

internal static map<uintptr, int32> iobMap = new map<uintptr, int32>{};
internal static bool wenvValid;
internal static readonly ж<uintptr> Ꮡwenviron = new StaticBox<uintptr>(default(uintptr));
internal static ref uintptr wenviron => ref ((StaticBox<uintptr>)Ꮡwenviron).ValueSlot;
internal static readonly ж<slice<uintptr>> ᏑwinEnviron = new StaticBox<slice<uintptr>>([0]);
internal static ref slice<uintptr> winEnviron => ref ((StaticBox<slice<uintptr>>)ᏑwinEnviron).ValueSlot;

[GoInit] internal static void initΔ1() {
    foreach (var i in range(len(X_iob))) {
        iobMap[(uintptr)ᏑX_iob.at<stdioꓸFILE>(i)] = (int32)i;
    }
}

public static partial uintptr X__p__wenviron(ж<TLS> t) {
    if (!wenvValid) {
        bootWinEnviron(t);
    }
    return (uintptr)@unsafe.Pointer.FromBox(Ꮡwenviron);
}

internal static any winGetObject(uintptr stream) {
    var (fd, ok) = iobMap[stream, ꟷ]; if (ok) {
        var (f, _) = fdToFile(fd);
        return f.OrTypedNil();
    }

    return getObject(stream);
}

internal static ж<windows.LazyDLL> modkernel32 = windows.NewLazySystemDLL("kernel32.dll");
internal static ж<windows.LazyProc> procAreFileApisANSI = modkernel32.NewProc("AreFileApisANSI");
internal static ж<windows.LazyProc> procCopyFileW = modkernel32.NewProc("CopyFileW");
internal static ж<windows.LazyProc> procCreateEventA = modkernel32.NewProc("CreateEventA");
internal static ж<windows.LazyProc> procCreateEventW = modkernel32.NewProc("CreateEventW");
internal static ж<windows.LazyProc> procCreateFileA = modkernel32.NewProc("CreateFileA");
internal static ж<windows.LazyProc> procCreateFileMappingW = modkernel32.NewProc("CreateFileMappingW");
internal static ж<windows.LazyProc> procCreateFileW = modkernel32.NewProc("CreateFileW");
internal static ж<windows.LazyProc> procCreateHardLinkW = modkernel32.NewProc("CreateHardLinkW");
internal static ж<windows.LazyProc> procCreatePipe = modkernel32.NewProc("CreatePipe");
internal static ж<windows.LazyProc> procCreateProcessA = modkernel32.NewProc("CreateProcessA");
internal static ж<windows.LazyProc> procCreateProcessW = modkernel32.NewProc("CreateProcessW");
internal static ж<windows.LazyProc> procCreateThread = modkernel32.NewProc("CreateThread");
internal static ж<windows.LazyProc> procDeleteCriticalSection = modkernel32.NewProc("DeleteCriticalSection");
internal static ж<windows.LazyProc> procDeviceIoControl = modkernel32.NewProc("DeviceIoControl");
internal static ж<windows.LazyProc> procDuplicateHandle = modkernel32.NewProc("DuplicateHandle");
internal static ж<windows.LazyProc> procOutputDebugStringW = modkernel32.NewProc("OutputDebugStringW");
internal static ж<windows.LazyProc> procEnterCriticalSection = modkernel32.NewProc("EnterCriticalSection");
internal static ж<windows.LazyProc> procFindClose = modkernel32.NewProc("FindClose");
internal static ж<windows.LazyProc> procFindFirstFileExW = modkernel32.NewProc("FindFirstFileExW");
internal static ж<windows.LazyProc> procFindFirstFileW = modkernel32.NewProc("FindFirstFileW");
internal static ж<windows.LazyProc> procFindNextFileW = modkernel32.NewProc("FindNextFileW");
internal static ж<windows.LazyProc> procFormatMessageW = modkernel32.NewProc("FormatMessageW");
internal static ж<windows.LazyProc> procGetACP = modkernel32.NewProc("GetACP");
internal static ж<windows.LazyProc> procGetCommState = modkernel32.NewProc("GetCommState");
internal static ж<windows.LazyProc> procGetComputerNameExW = modkernel32.NewProc("GetComputerNameExW");
internal static ж<windows.LazyProc> procGetConsoleCP = modkernel32.NewProc("GetConsoleCP");
internal static ж<windows.LazyProc> procGetConsoleScreenBufferInfo = modkernel32.NewProc("GetConsoleScreenBufferInfo");
internal static ж<windows.LazyProc> procGetCurrentProcess = modkernel32.NewProc("GetCurrentProcess");
internal static ж<windows.LazyProc> procGetCurrentProcessId = modkernel32.NewProc("GetCurrentProcessId");
internal static ж<windows.LazyProc> procGetCurrentThread = modkernel32.NewProc("GetCurrentThread");
internal static ж<windows.LazyProc> procGetCurrentThreadId = modkernel32.NewProc("GetCurrentThreadId");
internal static ж<windows.LazyProc> procGetDriveTypeW = modkernel32.NewProc("GetDriveTypeW");
internal static ж<windows.LazyProc> procGetEnvironmentVariableA = modkernel32.NewProc("GetEnvironmentVariableA");
internal static ж<windows.LazyProc> procGetEnvironmentVariableW = modkernel32.NewProc("GetEnvironmentVariableW");
internal static ж<windows.LazyProc> procGetExitCodeProcess = modkernel32.NewProc("GetExitCodeProcess");
internal static ж<windows.LazyProc> procGetExitCodeThread = modkernel32.NewProc("GetExitCodeThread");
internal static ж<windows.LazyProc> procGetFileAttributesA = modkernel32.NewProc("GetFileAttributesA");
internal static ж<windows.LazyProc> procGetFileAttributesExA = modkernel32.NewProc("GetFileAttributesExA");
internal static ж<windows.LazyProc> procGetFileAttributesExW = modkernel32.NewProc("GetFileAttributesExW");
internal static ж<windows.LazyProc> procGetFileInformationByHandle = modkernel32.NewProc("GetFileInformationByHandle");
internal static ж<windows.LazyProc> procGetFileSize = modkernel32.NewProc("GetFileSize");
internal static ж<windows.LazyProc> procGetFileSizeEx = modkernel32.NewProc("GetFileSizeEx");
internal static ж<windows.LazyProc> procGetFullPathNameW = modkernel32.NewProc("GetFullPathNameW");
internal static ж<windows.LazyProc> procGetLastError = modkernel32.NewProc("GetLastError");
internal static ж<windows.LazyProc> procGetLogicalDriveStringsA = modkernel32.NewProc("GetLogicalDriveStringsA");
internal static ж<windows.LazyProc> procGetModuleFileNameW = modkernel32.NewProc("GetModuleFileNameW");
internal static ж<windows.LazyProc> procGetModuleHandleA = modkernel32.NewProc("GetModuleHandleA");
internal static ж<windows.LazyProc> procGetModuleHandleW = modkernel32.NewProc("GetModuleHandleW");
internal static ж<windows.LazyProc> procGetNativeSystemInfo = modkernel32.NewProc("GetNativeSystemInfo");
internal static ж<windows.LazyProc> procGetPrivateProfileStringA = modkernel32.NewProc("GetPrivateProfileStringA");
internal static ж<windows.LazyProc> procGetProcAddress = modkernel32.NewProc("GetProcAddress");
internal static ж<windows.LazyProc> procGetProcessHeap = modkernel32.NewProc("GetProcessHeap");
internal static ж<windows.LazyProc> procGetSystemInfo = modkernel32.NewProc("GetSystemInfo");
internal static ж<windows.LazyProc> procGetSystemTime = modkernel32.NewProc("GetSystemTime");
internal static ж<windows.LazyProc> procGetSystemTimeAsFileTime = modkernel32.NewProc("GetSystemTimeAsFileTime");
internal static ж<windows.LazyProc> procGetTempFileNameW = modkernel32.NewProc("GetTempFileNameW");
internal static ж<windows.LazyProc> procGetTickCount = modkernel32.NewProc("GetTickCount");
internal static ж<windows.LazyProc> procGetTickCount64 = modkernel32.NewProc("GetTickCount64");
internal static ж<windows.LazyProc> procGetVersionExA = modkernel32.NewProc("GetVersionExA");
internal static ж<windows.LazyProc> procGetVersionExW = modkernel32.NewProc("GetVersionExW");
internal static ж<windows.LazyProc> procGetVolumeInformationA = modkernel32.NewProc("GetVolumeInformationA");
internal static ж<windows.LazyProc> procGetVolumeInformationW = modkernel32.NewProc("GetVolumeInformationW");
internal static ж<windows.LazyProc> procHeapAlloc = modkernel32.NewProc("HeapAlloc");
internal static ж<windows.LazyProc> procHeapFree = modkernel32.NewProc("HeapFree");
internal static ж<windows.LazyProc> procInitializeCriticalSection = modkernel32.NewProc("InitializeCriticalSection");
internal static ж<windows.LazyProc> procLeaveCriticalSection = modkernel32.NewProc("LeaveCriticalSection");
internal static ж<windows.LazyProc> procLockFile = modkernel32.NewProc("LockFile");
internal static ж<windows.LazyProc> procLockFileEx = modkernel32.NewProc("LockFileEx");
internal static ж<windows.LazyProc> procLstrlenW = modkernel32.NewProc("lstrlenW");
internal static ж<windows.LazyProc> procMapViewOfFile = modkernel32.NewProc("MapViewOfFile");
internal static ж<windows.LazyProc> procMoveFileW = modkernel32.NewProc("MoveFileW");
internal static ж<windows.LazyProc> procMultiByteToWideChar = modkernel32.NewProc("MultiByteToWideChar");
internal static ж<windows.LazyProc> procOpenEventA = modkernel32.NewProc("OpenEventA");
internal static ж<windows.LazyProc> procOpenProcessToken = modkernel32.NewProc("OpenProcessToken");
internal static ж<windows.LazyProc> procPeekConsoleInputW = modkernel32.NewProc("PeekConsoleInputW");
internal static ж<windows.LazyProc> procPeekNamedPipe = modkernel32.NewProc("PeekNamedPipe");
internal static ж<windows.LazyProc> procQueryPerformanceCounter = modkernel32.NewProc("QueryPerformanceCounter");
internal static ж<windows.LazyProc> procQueryPerformanceFrequency = modkernel32.NewProc("QueryPerformanceFrequency");
internal static ж<windows.LazyProc> procReadConsoleW = modkernel32.NewProc("ReadConsoleW");
internal static ж<windows.LazyProc> procReadFile = modkernel32.NewProc("ReadFile");
internal static ж<windows.LazyProc> procResetEvent = modkernel32.NewProc("ResetEvent");
internal static ж<windows.LazyProc> procSearchPathW = modkernel32.NewProc("SearchPathW");
internal static ж<windows.LazyProc> procSetConsoleCtrlHandler = modkernel32.NewProc("SetConsoleCtrlHandler");
internal static ж<windows.LazyProc> procSetConsoleMode = modkernel32.NewProc("SetConsoleMode");
internal static ж<windows.LazyProc> procSetConsoleTextAttribute = modkernel32.NewProc("SetConsoleTextAttribute");
internal static ж<windows.LazyProc> procSetEvent = modkernel32.NewProc("SetEvent");
internal static ж<windows.LazyProc> procSetFilePointer = modkernel32.NewProc("SetFilePointer");
internal static ж<windows.LazyProc> procSetFilePointerEx = modkernel32.NewProc("SetFilePointerEx");
internal static ж<windows.LazyProc> procSetFileTime = modkernel32.NewProc("SetFileTime");
internal static ж<windows.LazyProc> procSleepEx = modkernel32.NewProc("SleepEx");
internal static ж<windows.LazyProc> procSystemTimeToFileTime = modkernel32.NewProc("SystemTimeToFileTime");
internal static ж<windows.LazyProc> procTerminateThread = modkernel32.NewProc("TerminateThread");
internal static ж<windows.LazyProc> procTryEnterCriticalSection = modkernel32.NewProc("TryEnterCriticalSection");
internal static ж<windows.LazyProc> procUnlockFile = modkernel32.NewProc("UnlockFile");
internal static ж<windows.LazyProc> procUnlockFileEx = modkernel32.NewProc("UnlockFileEx");
internal static ж<windows.LazyProc> procWaitForSingleObjectEx = modkernel32.NewProc("WaitForSingleObjectEx");
internal static ж<windows.LazyProc> procWideCharToMultiByte = modkernel32.NewProc("WideCharToMultiByte");
internal static ж<windows.LazyProc> procWriteConsoleA = modkernel32.NewProc("WriteConsoleA");
internal static ж<windows.LazyProc> procWriteConsoleW = modkernel32.NewProc("WriteConsoleW");
internal static ж<windows.LazyProc> procWriteFile = modkernel32.NewProc("WriteFile");
internal static ж<windows.LazyDLL> modadvapi = windows.NewLazySystemDLL("advapi32.dll");
internal static ж<windows.LazyProc> procAccessCheck = modadvapi.NewProc("AccessCheck");
internal static ж<windows.LazyProc> procAddAce = modadvapi.NewProc("AddAce");
internal static ж<windows.LazyProc> procEqualSid = modadvapi.NewProc("EqualSid");
internal static ж<windows.LazyProc> procGetAce = modadvapi.NewProc("GetAce");
internal static ж<windows.LazyProc> procGetAclInformation = modadvapi.NewProc("GetAclInformation");
internal static ж<windows.LazyProc> procGetFileSecurityA = modadvapi.NewProc("GetFileSecurityA");
internal static ж<windows.LazyProc> procGetFileSecurityW = modadvapi.NewProc("GetFileSecurityW");
internal static ж<windows.LazyProc> procGetLengthSid = modadvapi.NewProc("GetLengthSid");
internal static ж<windows.LazyProc> procGetNamedSecurityInfoW = modadvapi.NewProc("GetNamedSecurityInfoW");
internal static ж<windows.LazyProc> procGetSecurityDescriptorDacl = modadvapi.NewProc("GetSecurityDescriptorDacl");
internal static ж<windows.LazyProc> procGetSecurityDescriptorOwner = modadvapi.NewProc("GetSecurityDescriptorOwner");
internal static ж<windows.LazyProc> procGetSidIdentifierAuthority = modadvapi.NewProc("GetSidIdentifierAuthority");
internal static ж<windows.LazyProc> procGetSidLengthRequired = modadvapi.NewProc("GetSidLengthRequired");
internal static ж<windows.LazyProc> procGetSidSubAuthority = modadvapi.NewProc("GetSidSubAuthority");
internal static ж<windows.LazyProc> procGetTokenInformation = modadvapi.NewProc("GetTokenInformation");
internal static ж<windows.LazyProc> procImpersonateSelf = modadvapi.NewProc("ImpersonateSelf");
internal static ж<windows.LazyProc> procInitializeAcl = modadvapi.NewProc("InitializeAcl");
internal static ж<windows.LazyProc> procInitializeSid = modadvapi.NewProc("InitializeSid");
internal static ж<windows.LazyProc> procOpenThreadToken = modadvapi.NewProc("OpenThreadToken");
internal static ж<windows.LazyProc> procRevertToSelf = modadvapi.NewProc("RevertToSelf");
internal static ж<windows.LazyDLL> modws2_32 = windows.NewLazySystemDLL("ws2_32.dll");
internal static ж<windows.LazyProc> procWSAStartup = modws2_32.NewProc("WSAStartup");
internal static ж<windows.LazyDLL> moduser32 = windows.NewLazySystemDLL("user32.dll");
internal static ж<windows.LazyProc> procCharLowerW = moduser32.NewProc("CharLowerW");
internal static ж<windows.LazyProc> procCreateWindowExW = moduser32.NewProc("CreateWindowExW");
internal static ж<windows.LazyProc> procMsgWaitForMultipleObjectsEx = moduser32.NewProc("MsgWaitForMultipleObjectsEx");
internal static ж<windows.LazyProc> procPeekMessageW = moduser32.NewProc("PeekMessageW");
internal static ж<windows.LazyProc> procRegisterClassW = moduser32.NewProc("RegisterClassW");
internal static ж<windows.LazyProc> procUnregisterClassW = moduser32.NewProc("UnregisterClassW");
internal static ж<windows.LazyProc> procWaitForInputIdle = moduser32.NewProc("WaitForInputIdle");
internal static ж<windows.LazyDLL> netapi = windows.NewLazySystemDLL("netapi32.dll");
internal static ж<windows.LazyProc> procNetGetDCName = netapi.NewProc("NetGetDCName");
internal static ж<windows.LazyProc> procNetUserGetInfo = netapi.NewProc("NetUserGetInfo");
internal static ж<windows.LazyDLL> userenvapi = windows.NewLazySystemDLL("userenv.dll");
internal static ж<windows.LazyProc> procGetProfilesDirectoryW = userenvapi.NewProc("GetProfilesDirectoryW");
internal static ж<windows.LazyDLL> modcrt = windows.NewLazySystemDLL("msvcrt.dll");
internal static ж<windows.LazyProc> procAccess = modcrt.NewProc("_access");
internal static ж<windows.LazyProc> procChmod = modcrt.NewProc("_chmod");
internal static ж<windows.LazyProc> procCtime64 = modcrt.NewProc("ctime64");
internal static ж<windows.LazyProc> procGmtime = modcrt.NewProc("gmtime");
internal static ж<windows.LazyProc> procGmtime32 = modcrt.NewProc("_gmtime32");
internal static ж<windows.LazyProc> procGmtime64 = modcrt.NewProc("_gmtime64");
internal static ж<windows.LazyProc> procStati64 = modcrt.NewProc("_stati64");
internal static ж<windows.LazyProc> procStrftime = modcrt.NewProc("strftime");
internal static ж<windows.LazyProc> procStrnicmp = modcrt.NewProc("_strnicmp");
internal static ж<windows.LazyProc> procStrtod = modcrt.NewProc("strtod");
internal static ж<windows.LazyProc> procTime64 = modcrt.NewProc("time64");
internal static ж<windows.LazyProc> procWcsncpy = modcrt.NewProc("wcsncpy");
internal static ж<windows.LazyProc> procWcsrchr = modcrt.NewProc("wcsrchr");
internal static ж<windows.LazyDLL> moducrt = windows.NewLazySystemDLL("ucrtbase.dll");
internal static ж<windows.LazyProc> procFindfirst32 = moducrt.NewProc("_findfirst32");
internal static ж<windows.LazyProc> procFindnext32 = moducrt.NewProc("_findnext32");
internal static ж<windows.LazyProc> procStat32i64 = moducrt.NewProc("_stat32i64");
internal static ж<windows.LazyProc> procStat64i32 = moducrt.NewProc("_stat64i32");
internal static ж<windows.LazyProc> procWchmod = moducrt.NewProc("_wchmod");
internal static ж<windows.LazyProc> procWfindfirst32 = moducrt.NewProc("_wfindfirst32");
internal static ж<windows.LazyProc> procWfindfirst64i32 = moducrt.NewProc("_wfindfirst64i32");
internal static ж<windows.LazyProc> procWfindnext32 = moducrt.NewProc("_wfindnext32");
internal static ж<windows.LazyProc> procWfindnext64i32 = moducrt.NewProc("_wfindnext64i32");
internal static ж<windows.LazyProc> procWfullpath = moducrt.NewProc("_wfullpath");
internal static ж<windows.LazyProc> procWmkdir = moducrt.NewProc("_wmkdir");
internal static ж<windows.LazyProc> procWstat32 = moducrt.NewProc("_wstat32");
internal static ж<windows.LazyProc> procWstat64i32 = moducrt.NewProc("_wstat64i32");

internal static uintptr threadCallback;

[GoInit] internal static void initΔ2() {
    isWindows = true;
    threadCallback = windows.NewCallback(ThreadProc);
}

public static error EBADF = errors.New("EBADF");

internal static readonly ж<int32> Ꮡw_nextFd = new StaticBox<int32>(42);
internal static ref int32 w_nextFd => ref ((StaticBox<int32>)Ꮡw_nextFd).ValueSlot;
internal static readonly ж<sync.Mutex> Ꮡw_fdLock = new StaticBox<sync.Mutex>(default(sync.Mutex));
internal static ref sync.Mutex w_fdLock => ref ((StaticBox<sync.Mutex>)Ꮡw_fdLock).ValueSlot;
internal static map<int32, ж<@file>> w_fd_to_file = new map<int32, ж<@file>>{};

partial struct @file {
    internal int32 _fd;
    internal bool hadErr;
    internal bool eof;
    internal int32 unget;
    internal uintptr t;
    /*embed*/ public golang.org.x.sys.windows_package.Handle Handle;
}

internal static uintptr addFile(windows.Handle hdl, int32 fd) {
    GoFrame ᒐ = default;
    try {
        ref var f = ref heap(new @file(), out var Ꮡf);

        f = new @file(_fd: fd, Handle: hdl, unget: -1);
        Ꮡw_fdLock.Lock();
        defer(Ꮡw_fdLock.Unlock, ref ᒐ);
        w_fd_to_file[fd] = Ꮡf;
        f.t = addObject(Ꮡf);
        return f.t;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static void remFile(ref @file f) {
    GoFrame ᒐ = default;
    try {
        removeObject(f.t);
        Ꮡw_fdLock.Lock();
        defer(Ꮡw_fdLock.Unlock, ref ᒐ);
        delete(w_fd_to_file, f._fd);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static (ж<@file>, bool) fdToFile(int32 fd) {
    GoFrame ᒐ = default;
    try {
        Ꮡw_fdLock.Lock();
        defer(Ꮡw_fdLock.Unlock, ref ᒐ);
        var (f, ok) = w_fd_to_file[fd, ꟷ];
        return (f, ok);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static (uintptr, int32) wrapFdHandle(windows.Handle hdl) {
    var newFd = atomic.AddInt32(Ꮡw_nextFd, 1);
    return (addFile(hdl, newFd), newFd);
}

} partial class libcᴛfile {

internal static bool err(this ref @file f) {
    return f.hadErr;
}

internal static void setErr(this ref @file f) {
    f.hadErr = true;
}

} partial class libcᴛTLS {

public static void SetLastError(this ж<TLS> Ꮡtls, uint32 _dwErrCode) {
    ref var tls = ref Ꮡtls.DerefOrNull();

    if (Ꮡtls != nil) {
        tls.lastError = _dwErrCode;
    }
}

public static uint32 /*r*/ GetLastError(this ж<TLS> Ꮡtls) {
    ref var tls = ref Ꮡtls.DerefOrNull();

    if (Ꮡtls == nil) {
        return 0;
    }

    return tls.lastError;
}

} partial class libc_package {

internal static uintptr newFile(ref TLS t, int32 fd) {
    if (fd == unistd.STDIN_FILENO) {
        var (h, err) = windows.GetStdHandle(windows.STD_INPUT_HANDLE);
        if (err != nil) {
            throw panic("no console");
        }
        return addFile(h, fd);
    }
    if (fd == unistd.STDOUT_FILENO) {
        var (h, err) = windows.GetStdHandle(windows.STD_OUTPUT_HANDLE);
        if (err != nil) {
            throw panic("no console");
        }
        return addFile(h, fd);
    }
    if (fd == unistd.STDERR_FILENO) {
        var (h, err) = windows.GetStdHandle(windows.STD_ERROR_HANDLE);
        if (err != nil) {
            throw panic("no console");
        }
        return addFile(h, fd);
    }

    throw panic("unknown fd source");
    return 0;
}

} partial class libcᴛfile {

internal static int32 close(this ж<@file> Ꮡf, ж<TLS> t) {
    ref var f = ref Ꮡf.DerefOrNull();

    remFile(ref (Ꮡf).DerefOrNull());
    var err = windows.Close(f.Handle);
    if (err != nil) {
        return (-1);
    }
    return 0;
}

} partial class libc_package {

internal static (nint, error) fwrite(int32 fd, slice<byte> b) {
    if (fd == unistd.STDOUT_FILENO) {
        return write(b);
    }

    var (f, ok) = fdToFile(fd);
    if (!ok) {
        return (-1, EBADF);
    }

    if (dmesgs) {
        dmesg("%v: fd %v: %s", origin(1), fd, b);
    }
    return windows.Write(f.Handle, b);
}

public static partial int32 Xfprintf(ж<TLS> t, uintptr stream, uintptr format, uintptr args) {
    if (__ccgo_strace) {
        trc("t=%v args=%v, (%v:)", t.OrTypedNil(), args, origin(2));
    }
    var (f, ok) = winGetObject(stream)._<ж<@file>>(ᐧ);
    if (!ok) {
        t.setErrno((nint)(errno.EBADF));
        return -1;
    }

    var (n, _) = fwrite(f._fd, printf(format, args));
    return (int32)n;
}

public static partial int32 Xusleep(ж<TLS> t, typesꓸUseconds_t usec) {
    if (__ccgo_strace) {
        trc("t=%v usec=%v, (%v:)", t.OrTypedNil(), usec, origin(2));
    }
    gotime.Sleep(gotime.Microsecond * ((gotime.Duration)(int64)usec));
    return 0;
}

public static partial int32 Xgetrusage(ж<TLS> t, int32 who, uintptr usage) {
    if (__ccgo_strace) {
        trc("t=%v who=%v usage=%v, (%v:)", t.OrTypedNil(), who, usage, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xlstat(ж<TLS> t, uintptr pathname, uintptr statbuf) {
    if (__ccgo_strace) {
        trc("t=%v statbuf=%v, (%v:)", t.OrTypedNil(), statbuf, origin(2));
    }
    return Xlstat64(t, pathname, statbuf);
}

public static partial int32 Xstat(ж<TLS> t, uintptr pathname, uintptr statbuf) {
    if (__ccgo_strace) {
        trc("t=%v statbuf=%v, (%v:)", t.OrTypedNil(), statbuf, origin(2));
    }
    return Xstat64(t, pathname, statbuf);
}

public static partial int32 Xchdir(ж<TLS> t, uintptr path) {
    if (__ccgo_strace) {
        trc("t=%v path=%v, (%v:)", t.OrTypedNil(), path, origin(2));
    }
    var err = windows.Chdir(GoString(path));
    if (err != nil) {
        t.setErrno(err);
        return -1;
    }

    if (dmesgs) {
        dmesg("%v: %q: ok", origin(1), GoString(path));
    }
    return 0;
}

internal static readonly ж<timeꓸTm> Ꮡlocaltime = new StaticBox<timeꓸTm>(default(timeꓸTm));
internal static ref timeꓸTm localtime => ref ((StaticBox<timeꓸTm>)Ꮡlocaltime).ValueSlot;

public static uintptr Xlocaltime(ж<TLS> _, uintptr timep) {
    var loc = getLocalLocation();
    var ut = ~@unsafe.Pointer.To<timeꓸTime_t>((@unsafe.Pointer)timep);
    var t = gotime.Unix((int64)ut, 0).In(loc);
    localtime.Ftm_sec = (int32)t.Second();
    localtime.Ftm_min = (int32)t.Minute();
    localtime.Ftm_hour = (int32)t.Hour();
    localtime.Ftm_mday = (int32)t.Day();
    localtime.Ftm_mon = (int32)(t.Month() - 1);
    localtime.Ftm_year = (int32)(t.Year() - 1900);
    localtime.Ftm_wday = (int32)t.Weekday();
    localtime.Ftm_yday = (int32)(t.YearDay() - 1);
    localtime.Ftm_isdst = Bool32(isTimeDST(t));
    return (uintptr)Ꮡlocaltime;
}

public static partial uintptr X_localtime64(ж<TLS> _, uintptr timep) {
    return Xlocaltime(nil, timep);
}

public static partial uintptr Xlocaltime_r(ж<TLS> _, uintptr timep, uintptr result) {
    throw panic(todo(""));
}

public static partial int32 X_wopen(ж<TLS> t, uintptr pathname, int32 flags, uintptr args) {
    if (__ccgo_strace) {
        trc("t=%v pathname=%v flags=%v args=%v, (%v:)", t.OrTypedNil(), pathname, flags, args, origin(2));
    }
    typesꓸMode_t mode = default;
    if (args != 0) {
        mode = ~@unsafe.Pointer.To<typesꓸMode_t>((@unsafe.Pointer)args);
    }
    @string s = goWideStringNZ(pathname);
    var (h, err) = windows.Open(s, (nint)flags, (uint32)mode);
    if (err != nil) {
        if (dmesgs) {
            dmesg("%v: %q %#x: %v", origin(1), s, flags, err);
        }

        t.setErrno(err);
        return 0;
    }

    var (_, n) = wrapFdHandle(h);
    if (dmesgs) {
        dmesg("%v: %q flags %#x mode %#o: fd %v", origin(1), s, flags, mode, n);
    }
    return n;
}

public static partial int32 Xopen(ж<TLS> t, uintptr pathname, int32 flags, uintptr args) {
    if (__ccgo_strace) {
        trc("t=%v pathname=%v flags=%v args=%v, (%v:)", t.OrTypedNil(), pathname, flags, args, origin(2));
    }
    return Xopen64(t, pathname, flags, args);
}

public static partial int32 Xopen64(ж<TLS> t, uintptr pathname, int32 flags, uintptr cmodeʗp) {
    ref var cmode = ref heap(cmodeʗp, out var Ꮡcmode);

    if (__ccgo_strace) {
        trc("t=%v pathname=%v flags=%v cmode=%v, (%v:)", t.OrTypedNil(), pathname, flags, cmode, origin(2));
    }

    typesꓸMode_t mode = default;
    if (cmode != 0) {
        mode = (typesꓸMode_t)VaUint32(Ꮡcmode);
    }
    var (h, err) = windows.Open(GoString(pathname), (nint)flags, (uint32)mode);
    if (err != nil) {
        if (dmesgs) {
            dmesg("%v: %q %#x: %v", origin(1), GoString(pathname), flags, err);
        }

        t.setErrno(err);
        return -1;
    }

    var (_, n) = wrapFdHandle(h);
    if (dmesgs) {
        dmesg("%v: %q flags %#x mode %#o: fd %v", origin(1), GoString(pathname), flags, mode, n);
    }
    return n;
}

public static partial typesꓸOff_t Xlseek(ж<TLS> t, int32 fd, typesꓸOff_t offset, int32 whence) {
    if (__ccgo_strace) {
        trc("t=%v fd=%v offset=%v whence=%v, (%v:)", t.OrTypedNil(), fd, offset, whence, origin(2));
    }
    return (typesꓸOff_t)Xlseek64(t, fd, offset, whence);
}

internal static @string whenceStr(int32 whence) {
    switch (whence) {
    case var ᴛ when ᴛ == windows.FILE_CURRENT: {
        return "SEEK_CUR";
    }
    case var ᴛ when ᴛ == windows.FILE_END: {
        return "SEEK_END";
    }
    case var ᴛ when ᴛ == windows.FILE_BEGIN: {
        return "SEEK_SET";
    }
    default: {
        return fmt.Sprintf("whence(%d)"u8, whence);
    }}

}

internal static statꓸStat fsyncStatbuf = new();

public static partial int32 Xfsync(ж<TLS> t, int32 fd) {
    if (__ccgo_strace) {
        trc("t=%v fd=%v, (%v:)", t.OrTypedNil(), fd, origin(2));
    }

    var (f, ok) = fdToFile(fd);
    if (!ok) {
        t.setErrno((nint)(errno.EBADF));
        return -1;
    }
    var err = windows.FlushFileBuffers(f.Handle);
    if (err != nil) {
        t.setErrno(err);
        return -1;
    }

    if (dmesgs) {
        dmesg("%v: %d: ok", origin(1), fd);
    }
    return 0;
}

public static partial @long Xsysconf(ж<TLS> t, int32 name) {
    if (__ccgo_strace) {
        trc("t=%v name=%v, (%v:)", t.OrTypedNil(), name, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xclose(ж<TLS> t, int32 fd) {
    if (__ccgo_strace) {
        trc("t=%v fd=%v, (%v:)", t.OrTypedNil(), fd, origin(2));
    }

    var (f, ok) = fdToFile(fd);
    if (!ok) {
        t.setErrno((nint)(errno.EBADF));
        return -1;
    }

    var err = windows.Close(f.Handle);
    if (err != nil) {
        t.setErrno(err);
        return -1;
    }

    if (dmesgs) {
        dmesg("%v: %d: ok", origin(1), fd);
    }
    return 0;
}

public static partial uintptr Xgetcwd(ж<TLS> t, uintptr buf, typesꓸSize_t size) {
    if (__ccgo_strace) {
        trc("t=%v buf=%v size=%v, (%v:)", t.OrTypedNil(), buf, size, origin(2));
    }

    var b = new slice<uint16>((nint)(size));
    var (n, err) = windows.GetCurrentDirectory((uint32)len(b), Ꮡ(b, 0));
    if (err != nil) {
        t.setErrno(err);
        return 0;
    }
    slice<byte> wd = slice<byte>((@string)utf16.Decode(b.slice(0, (nint)(n))));
    if (((typesꓸSize_t)len(wd)) > size) {
        t.setErrno((nint)(errno.ERANGE));
        return 0;
    }

    @unsafe.Pointer.CopyInto<byte>((@unsafe.Pointer)buf, 0, 1125899906842623, wd);
    @unsafe.Pointer.ElementRef<byte>((@unsafe.Pointer)buf, len(wd), 1125899906842623) = 0;

    if (dmesgs) {
        dmesg("%v: %q: ok", origin(1), GoString(buf));
    }
    return buf;
}

public static partial int32 Xfstat(ж<TLS> t, int32 fd, uintptr statbuf) {
    if (__ccgo_strace) {
        trc("t=%v fd=%v statbuf=%v, (%v:)", t.OrTypedNil(), fd, statbuf, origin(2));
    }
    return Xfstat64(t, fd, statbuf);
}

public static partial int32 Xftruncate(ж<TLS> t, int32 fd, typesꓸOff_t length) {
    if (__ccgo_strace) {
        trc("t=%v fd=%v length=%v, (%v:)", t.OrTypedNil(), fd, length, origin(2));
    }
    return Xftruncate64(t, fd, length);
}

public static partial int32 Xfcntl(ж<TLS> t, int32 fd, int32 cmd, uintptr args) {
    if (__ccgo_strace) {
        trc("t=%v cmd=%v args=%v, (%v:)", t.OrTypedNil(), cmd, args, origin(2));
    }
    return Xfcntl64(t, fd, cmd, args);
}

public static partial int32 Xread(ж<TLS> t, int32 fd, uintptr buf, uint32 count) {
    if (__ccgo_strace) {
        trc("t=%v fd=%v buf=%v count=%v, (%v:)", t.OrTypedNil(), fd, buf, count, origin(2));
    }
    var (f, ok) = fdToFile(fd);
    if (!ok) {
        t.setErrno((nint)(errno.EBADF));
        return -1;
    }

    slice<byte> obuf = @unsafe.Pointer.View<byte>((@unsafe.Pointer)buf, 0, (nint)(count), 1125899906842623);
    var (n, err) = windows.Read(f.Handle, obuf);
    if (err != nil) {
        t.setErrno(err);
        return -1;
    }

    if (dmesgs) {
        dmesg("%v: %d %#x: %#x", origin(1), fd, count, n);
    }
    return (int32)n;
}

public static partial int32 X_read(ж<TLS> t, int32 fd, uintptr buf, uint32 count) {
    if (__ccgo_strace) {
        trc("t=%v fd=%v buf=%v count=%v, (%v:)", t.OrTypedNil(), fd, buf, count, origin(2));
    }
    return Xread(t, fd, buf, count);
}

public static partial int32 Xwrite(ж<TLS> t, int32 fd, uintptr buf, uint32 count) {
    if (__ccgo_strace) {
        trc("t=%v fd=%v buf=%v count=%v, (%v:)", t.OrTypedNil(), fd, buf, count, origin(2));
    }
    var (f, ok) = fdToFile(fd);
    if (!ok) {
        t.setErrno((nint)(errno.EBADF));
        return -1;
    }

    slice<byte> obuf = @unsafe.Pointer.View<byte>((@unsafe.Pointer)buf, 0, (nint)(count), 1125899906842623);
    var (n, err) = windows.Write(f.Handle, obuf);
    if (err != nil) {
        if (dmesgs) {
            dmesg("%v: fd %v, count %#x: %v", origin(1), fd, count, err);
        }
        t.setErrno(err);
        return -1;
    }

    if (dmesgs) {
        dmesg("%v: %d %#x: %#x", origin(1), fd, count, n);
    }
    return (int32)n;
}

public static partial int32 X_write(ж<TLS> t, int32 fd, uintptr buf, uint32 count) {
    if (__ccgo_strace) {
        trc("t=%v fd=%v buf=%v count=%v, (%v:)", t.OrTypedNil(), fd, buf, count, origin(2));
    }
    return Xwrite(t, fd, buf, count);
}

public static partial int32 Xfchmod(ж<TLS> t, int32 fd, typesꓸMode_t mode) {
    if (__ccgo_strace) {
        trc("t=%v fd=%v mode=%v, (%v:)", t.OrTypedNil(), fd, mode, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xmunmap(ж<TLS> t, uintptr addr, typesꓸSize_t length) {
    if (__ccgo_strace) {
        trc("t=%v addr=%v length=%v, (%v:)", t.OrTypedNil(), addr, length, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xgettimeofday(ж<TLS> t, uintptr tv, uintptr tz) {
    if (__ccgo_strace) {
        trc("t=%v tz=%v, (%v:)", t.OrTypedNil(), tz, origin(2));
    }
    if (tv == 0) {
        return 0;
    }

    windows.Gettimeofday(@unsafe.Pointer.To<windows.Timeval>((@unsafe.Pointer)tv));
    return 0;
}

partial struct Timespecᴛ1 /*dyn*/ {
    public timeꓸTime_t Ftv_sec;
    public int32 Ftv_nsec;
}

public static partial int32 Xclock_gettime(ж<TLS> t, int32 clk_id, uintptr tp) {
    if (__ccgo_strace) {
        trc("t=%v clk_id=%v tp=%v, (%v:)", t.OrTypedNil(), clk_id, tp, origin(2));
    }
    ref var u64 = ref heap(new uint64(), out var Ꮡu64);
    procGetSystemTimeAsFileTime.Call((uintptr)Ꮡu64, 0, 0);
    (@unsafe.Pointer.To<Timespec>((@unsafe.Pointer)tp)).Value.Ftv_sec = (timeꓸTime_t)((u64 / 10 - 11644473600000000UL) / 1000000);
    (@unsafe.Pointer.To<Timespec>((@unsafe.Pointer)tp)).Value.Ftv_nsec = (int32)((u64 * 100) % 1000000000);
    return 0;
}

public static partial int32 Xgetsockopt(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 Xsetsockopt(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 Xioctl(ж<TLS> t, int32 fd, @ulong request, uintptr va) {
    if (__ccgo_strace) {
        trc("t=%v fd=%v request=%v va=%v, (%v:)", t.OrTypedNil(), fd, request, va, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xselect(ж<TLS> t, int32 nfds, uintptr readfds, uintptr writefds, uintptr exceptfds, uintptr timeout) {
    if (__ccgo_strace) {
        trc("t=%v nfds=%v timeout=%v, (%v:)", t.OrTypedNil(), nfds, timeout, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xmkfifo(ж<TLS> t, uintptr pathname, typesꓸMode_t mode) {
    if (__ccgo_strace) {
        trc("t=%v pathname=%v mode=%v, (%v:)", t.OrTypedNil(), pathname, mode, origin(2));
    }
    throw panic(todo(""));
}

public static partial typesꓸMode_t Xumask(ж<TLS> t, typesꓸMode_t mask) {
    if (__ccgo_strace) {
        trc("t=%v mask=%v, (%v:)", t.OrTypedNil(), mask, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xexecvp(ж<TLS> t, uintptr @file, uintptr argv) {
    if (__ccgo_strace) {
        trc("t=%v argv=%v, (%v:)", t.OrTypedNil(), argv, origin(2));
    }
    throw panic(todo(""));
}

public static partial typesꓸPid_t Xwaitpid(ж<TLS> t, typesꓸPid_t pid, uintptr wstatus, int32 optname) {
    if (__ccgo_strace) {
        trc("t=%v pid=%v wstatus=%v optname=%v, (%v:)", t.OrTypedNil(), pid, wstatus, optname, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xuname(ж<TLS> t, uintptr buf) {
    if (__ccgo_strace) {
        trc("t=%v buf=%v, (%v:)", t.OrTypedNil(), buf, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xgetrlimit(ж<TLS> t, int32 resource, uintptr rlim) {
    if (__ccgo_strace) {
        trc("t=%v resource=%v rlim=%v, (%v:)", t.OrTypedNil(), resource, rlim, origin(2));
    }
    return Xgetrlimit64(t, resource, rlim);
}

public static partial int32 Xsetrlimit(ж<TLS> t, int32 resource, uintptr rlim) {
    if (__ccgo_strace) {
        trc("t=%v resource=%v rlim=%v, (%v:)", t.OrTypedNil(), resource, rlim, origin(2));
    }
    return Xsetrlimit64(t, resource, rlim);
}

public static partial int32 Xsetrlimit64(ж<TLS> t, int32 resource, uintptr rlim) {
    if (__ccgo_strace) {
        trc("t=%v resource=%v rlim=%v, (%v:)", t.OrTypedNil(), resource, rlim, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xgetpid(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    return (int32)os.Getpid();
}

public static partial int32 Xsystem(ж<TLS> t, uintptr command) {
    if (__ccgo_strace) {
        trc("t=%v command=%v, (%v:)", t.OrTypedNil(), command, origin(2));
    }
    @string s = GoString(command);
    if (command == 0) {
        throw panic(todo(""));
    }

    var cmd = exec.Command("sh", "-c", s);
    cmd.Stdout = new os.FileжWriter(os.Stdout);
    cmd.Stderr = new os.FileжWriter(os.Stderr);
    var err = cmd.Run();
    if (err != nil) {
        var ps = err._<ж<exec.ExitError>>();
        return (int32)ps.Value.ProcessState.ExitCode();
    }

    return 0;
}

public static partial uintptr Xgetpwuid(ж<TLS> t, uint32 uid) {
    if (__ccgo_strace) {
        trc("t=%v uid=%v, (%v:)", t.OrTypedNil(), uid, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xsetvbuf(ж<TLS> t, uintptr stream, uintptr buf, int32 mode, typesꓸSize_t size) {
    if (__ccgo_strace) {
        trc("t=%v buf=%v mode=%v size=%v, (%v:)", t.OrTypedNil(), buf, mode, size, origin(2));
    }
    return 0;
}

public static partial int32 Xraise(ж<TLS> t, int32 sig) {
    if (__ccgo_strace) {
        trc("t=%v sig=%v, (%v:)", t.OrTypedNil(), sig, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xbacktrace(ж<TLS> t, uintptr buf, int32 size) {
    if (__ccgo_strace) {
        trc("t=%v buf=%v size=%v, (%v:)", t.OrTypedNil(), buf, size, origin(2));
    }
    throw panic(todo(""));
}

public static partial void Xbacktrace_symbols_fd(ж<TLS> t, uintptr buffer, int32 size, int32 fd) {
    if (__ccgo_strace) {
        trc("t=%v buffer=%v fd=%v, (%v:)", t.OrTypedNil(), buffer, fd, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xfileno(ж<TLS> t, uintptr stream) {
    if (__ccgo_strace) {
        trc("t=%v stream=%v, (%v:)", t.OrTypedNil(), stream, origin(2));
    }
    if (stream == 0) {
        t.setErrno((nint)(errno.EBADF));
        return -1;
    }

    var (f, ok) = winGetObject(stream)._<ж<@file>>(ᐧ);
    if (!ok) {
        t.setErrno((nint)(errno.EBADF));
        return -1;
    }
    return f._fd;
}

public static partial int32 Xmkstemps(ж<TLS> t, uintptr template, int32 suffixlen) {
    if (__ccgo_strace) {
        trc("t=%v template=%v suffixlen=%v, (%v:)", t.OrTypedNil(), template, suffixlen, origin(2));
    }
    return Xmkstemps64(t, template, suffixlen);
}

public static partial int32 Xmkstemps64(ж<TLS> t, uintptr template, int32 suffixlen) {
    if (__ccgo_strace) {
        trc("t=%v template=%v suffixlen=%v, (%v:)", t.OrTypedNil(), template, suffixlen, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xmkstemp64(ж<TLS> t, uintptr template) {
    if (__ccgo_strace) {
        trc("t=%v template=%v, (%v:)", t.OrTypedNil(), template, origin(2));
    }
    return Xmkstemps64(t, template, 0);
}

partial struct ftstream {
    internal slice<uintptr> s;
    internal nint x;
}

public static partial uintptr Xfts64_open(ж<TLS> t, uintptr path_argv, int32 options, uintptr compar) {
    if (__ccgo_strace) {
        trc("t=%v path_argv=%v options=%v compar=%v, (%v:)", t.OrTypedNil(), path_argv, options, compar, origin(2));
    }
    throw panic(todo(""));
}

public static partial uintptr Xfts_read(ж<TLS> t, uintptr ftsp) {
    if (__ccgo_strace) {
        trc("t=%v ftsp=%v, (%v:)", t.OrTypedNil(), ftsp, origin(2));
    }
    return Xfts64_read(t, ftsp);
}

public static partial uintptr Xfts64_read(ж<TLS> t, uintptr ftsp) {
    if (__ccgo_strace) {
        trc("t=%v ftsp=%v, (%v:)", t.OrTypedNil(), ftsp, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xfts_close(ж<TLS> t, uintptr ftsp) {
    if (__ccgo_strace) {
        trc("t=%v ftsp=%v, (%v:)", t.OrTypedNil(), ftsp, origin(2));
    }
    return Xfts64_close(t, ftsp);
}

public static partial int32 Xfts64_close(ж<TLS> t, uintptr ftsp) {
    if (__ccgo_strace) {
        trc("t=%v ftsp=%v, (%v:)", t.OrTypedNil(), ftsp, origin(2));
    }
    throw panic(todo(""));
}

public static partial void Xtzset(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
}

internal static readonly ж<array<byte>> ᏑstrerrorBuf = new StaticBox<array<byte>>(new array<byte>(256));
internal static ref array<byte> strerrorBuf => ref ((StaticBox<array<byte>>)ᏑstrerrorBuf).ValueSlot;

public static partial uintptr Xstrerror(ж<TLS> t, int32 errnum) {
    if (__ccgo_strace) {
        trc("t=%v errnum=%v, (%v:)", t.OrTypedNil(), errnum, origin(2));
    }
    copy(@unsafe.Pointer.View<byte>(@unsafe.Pointer.FromPinnedBox(ᏑstrerrorBuf.at<byte>(0)), 0, len(strerrorBuf), len(strerrorBuf), 1125899906842623), fmt.Sprintf("errno %d\x00"u8, errnum));
    return (uintptr)ᏑstrerrorBuf.at<byte>(0);
}

public static partial uintptr Xdlopen(ж<TLS> t, uintptr filename, int32 flags) {
    if (__ccgo_strace) {
        trc("t=%v filename=%v flags=%v, (%v:)", t.OrTypedNil(), filename, flags, origin(2));
    }
    throw panic(todo(""));
}

public static partial uintptr Xdlerror(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xdlclose(ж<TLS> t, uintptr handle) {
    if (__ccgo_strace) {
        trc("t=%v handle=%v, (%v:)", t.OrTypedNil(), handle, origin(2));
    }
    throw panic(todo(""));
}

public static partial uintptr Xdlsym(ж<TLS> t, uintptr handle, uintptr symbol) {
    if (__ccgo_strace) {
        trc("t=%v symbol=%v, (%v:)", t.OrTypedNil(), symbol, origin(2));
    }
    throw panic(todo(""));
}

public static partial void Xperror(ж<TLS> tls, uintptr msg) {
    if (__ccgo_strace) {
        trc("tls=%v msg=%v, (%v:)", tls.OrTypedNil(), msg, origin(2));
    }
    if (msg != 0 && ~@unsafe.Pointer.To<int8>((@unsafe.Pointer)msg) != 0) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "%s: "u8, GoString(msg));
    }
    var errstr = Xstrerror(tls, ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)X__errno_location(tls)));
    fmt.Fprintf(new os.FileжWriter(os.Stderr), "%s\n"u8, GoString(errstr));
}

public static partial int32 Xpclose(ж<TLS> t, uintptr stream) {
    if (__ccgo_strace) {
        trc("t=%v stream=%v, (%v:)", t.OrTypedNil(), stream, origin(2));
    }
    throw panic(todo(""));
}

internal static readonly ж<array<byte>> Ꮡgai_strerrorBuf = new StaticBox<array<byte>>(new array<byte>(100));
internal static ref array<byte> gai_strerrorBuf => ref ((StaticBox<array<byte>>)Ꮡgai_strerrorBuf).ValueSlot;

public static partial uintptr Xgai_strerror(ж<TLS> t, int32 errcode) {
    if (__ccgo_strace) {
        trc("t=%v errcode=%v, (%v:)", t.OrTypedNil(), errcode, origin(2));
    }
    copy(gai_strerrorBuf[..], fmt.Sprintf("gai error %d\x00"u8, errcode));
    return (uintptr)Ꮡgai_strerrorBuf;
}

public static partial int32 Xtcgetattr(ж<TLS> t, int32 fd, uintptr termios_p) {
    if (__ccgo_strace) {
        trc("t=%v fd=%v termios_p=%v, (%v:)", t.OrTypedNil(), fd, termios_p, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xtcsetattr(ж<TLS> t, int32 fd, int32 optional_actions, uintptr termios_p) {
    if (__ccgo_strace) {
        trc("t=%v optional_actions=%v termios_p=%v, (%v:)", t.OrTypedNil(), optional_actions, termios_p, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xcfsetospeed(ж<TLS> t, uintptr termios_p, uint32 speed) {
    if (__ccgo_strace) {
        trc("t=%v termios_p=%v speed=%v, (%v:)", t.OrTypedNil(), termios_p, speed, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xcfsetispeed(ж<TLS> t, uintptr termios_p, uint32 speed) {
    if (__ccgo_strace) {
        trc("t=%v termios_p=%v speed=%v, (%v:)", t.OrTypedNil(), termios_p, speed, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xfork(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    t.setErrno((nint)(errno.ENOSYS));
    return -1;
}

public static partial uintptr Xsetlocale(ж<TLS> t, int32 category, uintptr locale) {
    if (__ccgo_strace) {
        trc("t=%v category=%v locale=%v, (%v:)", t.OrTypedNil(), category, locale, origin(2));
    }
    if (locale != 0) {
        var exprᴛ1 = GoString(locale);
        switch (ᐧ) {
        case {} when exprᴛ1 == ""u8 || exprᴛ1 == "C"u8 || exprᴛ1 == "POSIX"u8: {
            break;
        }
        default: {
            return 0;
        }}

    }
    ᏑsetlocaleOnce.Do(() => {
        setlocaleBuf = Xmalloc(t, 2);
        @unsafe.Pointer.StoreArray<byte>((@unsafe.Pointer)setlocaleBuf, 2, new byte[]{(byte)'C', 0}.array());
    });
    return setlocaleBuf;
}

internal static readonly ж<sync.Once> ᏑsetlocaleOnce = new StaticBox<sync.Once>(default(sync.Once));
internal static ref sync.Once setlocaleOnce => ref ((StaticBox<sync.Once>)ᏑsetlocaleOnce).ValueSlot;
internal static uintptr setlocaleBuf;

public static partial uintptr Xpopen(ж<TLS> t, uintptr command, uintptr type1) {
    if (__ccgo_strace) {
        trc("t=%v type1=%v, (%v:)", t.OrTypedNil(), type1, origin(2));
    }
    throw panic(todo(""));
}

public static partial uintptr Xrealpath(ж<TLS> t, uintptr path, uintptr resolved_path) {
    if (__ccgo_strace) {
        trc("t=%v resolved_path=%v, (%v:)", t.OrTypedNil(), resolved_path, origin(2));
    }
    var (s, err) = filepath.EvalSymlinks(GoString(path));
    if (err != nil) {
        if (os.IsNotExist(err)) {
            if (dmesgs) {
                dmesg("%v: %q: %v", origin(1), GoString(path), err);
            }
            t.setErrno((nint)(errno.ENOENT));
            return 0;
        }

        throw panic(todo("", err));
    }

    if (resolved_path == 0) {
        throw panic(todo(""));
    }

    if (len(s) >= limits.PATH_MAX) {
        s = s[..(int)(limits.PATH_MAX - 1)];
    }

    copy(@unsafe.Pointer.View<byte>((@unsafe.Pointer)resolved_path, 0, len(s), len(s), 1125899906842623), s);
    @unsafe.Pointer.ElementRef<byte>((@unsafe.Pointer)resolved_path, len(s), 1125899906842623) = 0;
    return resolved_path;
}

public static partial void Xabort(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xfflush(ж<TLS> t, uintptr stream) {
    if (__ccgo_strace) {
        trc("t=%v stream=%v, (%v:)", t.OrTypedNil(), stream, origin(2));
    }
    var (f, ok) = winGetObject(stream)._<ж<@file>>(ᐧ);
    if (!ok) {
        t.setErrno((nint)(errno.EBADF));
        return -1;
    }
    var err = windows.FlushFileBuffers(f.Handle);
    if (err != nil) {
        t.setErrno(err);
        return -1;
    }
    return 0;
}

public static partial typesꓸSize_t Xfread(ж<TLS> t, uintptr ptr, typesꓸSize_t size, typesꓸSize_t nmemb, uintptr stream) {
    if (__ccgo_strace) {
        trc("t=%v ptr=%v nmemb=%v stream=%v, (%v:)", t.OrTypedNil(), ptr, nmemb, stream, origin(2));
    }
    if (size == 0 || nmemb == 0) {
        return 0;
    }
    var (f, ok) = winGetObject(stream)._<ж<@file>>(ᐧ);
    if (!ok) {
        t.setErrno((nint)(errno.EBADF));
        return 0;
    }

    typesꓸSize_t sz = size * nmemb;
    slice<byte> obuf = @unsafe.Pointer.View<byte>((@unsafe.Pointer)ptr, 0, (nint)(sz), 1125899906842623);
    nint n = default;
    var c = f.unget; if (c >= 0) {
        f.unget = -1;
        obuf[0] = (byte)c;
        n = 1;
        obuf = obuf[1..];
    }
    if (len(obuf) != 0) {
        var (m, err) = windows.Read(f.Handle, obuf);
        if (err != nil) {
            f.setErr();
            return 0;
        }

        n += m;
    }
    if (((typesꓸSize_t)n) < sz) {
        f.eof = true;
    }

    if (dmesgs) {
        dmesg("%v: %d %#x x %#x: %#x", origin(1), f._fd, size, nmemb, ((typesꓸSize_t)n) / size);
    }

    return ((typesꓸSize_t)n) / size;
}

public static partial typesꓸSize_t Xfwrite(ж<TLS> t, uintptr ptr, typesꓸSize_t size, typesꓸSize_t nmemb, uintptr stream) {
    if (__ccgo_strace) {
        trc("t=%v ptr=%v nmemb=%v stream=%v, (%v:)", t.OrTypedNil(), ptr, nmemb, stream, origin(2));
    }
    if (ptr == 0 || size == 0 || nmemb == 0) {
        return 0;
    }

    var (f, ok) = winGetObject(stream)._<ж<@file>>(ᐧ);
    if (!ok) {
        t.setErrno((nint)(errno.EBADF));
        return 0;
    }

    typesꓸSize_t sz = size * nmemb;
    slice<byte> obuf = @unsafe.Pointer.View<byte>((@unsafe.Pointer)ptr, 0, (nint)(sz), 1125899906842623);
    var (n, err) = windows.Write(f.Handle, obuf);
    if (err != nil) {
        f.setErr();
        return 0;
    }

    if (dmesgs) {
        dmesg("%v: %d %#x x %#x: %#x", origin(1), f._fd, size, nmemb, ((typesꓸSize_t)n) / size);
    }
    return ((typesꓸSize_t)n) / size;
}

public static partial int32 Xfclose(ж<TLS> t, uintptr stream) {
    if (__ccgo_strace) {
        trc("t=%v stream=%v, (%v:)", t.OrTypedNil(), stream, origin(2));
    }
    var (f, ok) = winGetObject(stream)._<ж<@file>>(ᐧ);
    if (!ok) {
        t.setErrno((nint)(errno.EBADF));
        return -1;
    }
    return f.close(t);
}

public static partial int32 Xfputc(ж<TLS> t, int32 c, uintptr stream) {
    if (__ccgo_strace) {
        trc("t=%v c=%v stream=%v, (%v:)", t.OrTypedNil(), c, stream, origin(2));
    }
    var (f, ok) = winGetObject(stream)._<ж<@file>>(ᐧ);
    if (!ok) {
        t.setErrno((nint)(errno.EBADF));
        return -1;
    }
    var (_, err) = fwrite(f._fd, [(byte)c]); if (err != nil) {
        return -1;
    }
    return (int32)(byte)c;
}

public static partial int32 Xfseek(ж<TLS> t, uintptr stream, @long offset, int32 whence) {
    if (__ccgo_strace) {
        trc("t=%v stream=%v offset=%v whence=%v, (%v:)", t.OrTypedNil(), stream, offset, whence, origin(2));
    }
    var (f, ok) = winGetObject(stream)._<ж<@file>>(ᐧ);
    if (!ok) {
        t.setErrno((nint)(errno.EBADF));
        return -1;
    }
    f.eof = false;
    f.unget = -1;
    var n = Xlseek(t, f._fd, (typesꓸOff_t)offset, whence); if (n < 0) {
        if (dmesgs) {
            dmesg("%v: fd %v, off %#x, whence %v: %v", origin(1), f._fd, offset, whenceStr(whence), n);
        }
        f.setErr();
        return -1;
    }

    if (dmesgs) {
        dmesg("%v: fd %v, off %#x, whence %v: ok", origin(1), f._fd, offset, whenceStr(whence));
    }
    return 0;
}

public static partial @long Xftell(ж<TLS> t, uintptr stream) {
    if (__ccgo_strace) {
        trc("t=%v stream=%v, (%v:)", t.OrTypedNil(), stream, origin(2));
    }
    var (f, ok) = winGetObject(stream)._<ж<@file>>(ᐧ);
    if (!ok) {
        t.setErrno((nint)(errno.EBADF));
        return -1;
    }

    var n = Xlseek(t, f._fd, 0, windows.FILE_CURRENT);
    if (n < 0) {
        f.setErr();
        return -1;
    }

    if (dmesgs) {
        dmesg("%v: fd %v, n %#x: ok %#x", origin(1), f._fd, n, (@long)n);
    }
    return (@long)n;
}

public static partial int32 Xferror(ж<TLS> t, uintptr stream) {
    if (__ccgo_strace) {
        trc("t=%v stream=%v, (%v:)", t.OrTypedNil(), stream, origin(2));
    }
    var (f, ok) = winGetObject(stream)._<ж<@file>>(ᐧ);
    if (!ok) {
        t.setErrno((nint)(errno.EBADF));
        return -1;
    }

    return Bool32(f.err());
}

public static partial int32 Xfgetc(ж<TLS> t, uintptr stream) {
    if (__ccgo_strace) {
        trc("t=%v stream=%v, (%v:)", t.OrTypedNil(), stream, origin(2));
    }
    var (f, ok) = winGetObject(stream)._<ж<@file>>(ᐧ);
    if (!ok) {
        t.setErrno((nint)(errno.EBADF));
        return stdio.EOF;
    }

    var c = f.unget; if (c >= 0) {
        f.unget = -1;
        return c;
    }

    array<byte> buf = new(1);
    var (n, _) = windows.Read(f.Handle, buf[..]); if (n != 0) {
        return (int32)buf[0];
    }

    f.eof = true;
    return stdio.EOF;
}

public static partial int32 Xungetc(ж<TLS> t, int32 c, uintptr stream) {
    if (__ccgo_strace) {
        trc("t=%v c=%v stream=%v, (%v:)", t.OrTypedNil(), c, stream, origin(2));
    }
    var (f, ok) = winGetObject(stream)._<ж<@file>>(ᐧ);
    if (!ok || c == stdio.EOF || f.unget >= 0) {
        return stdio.EOF;
    }

    f.unget = (int32)(byte)c;
    f.eof = false;
    return (int32)(byte)c;
}

public static partial int32 Xfeof(ж<TLS> t, uintptr stream) {
    if (__ccgo_strace) {
        trc("t=%v stream=%v, (%v:)", t.OrTypedNil(), stream, origin(2));
    }
    var (f, ok) = winGetObject(stream)._<ж<@file>>(ᐧ);
    if (!ok) {
        t.setErrno((nint)(errno.EBADF));
        return 0;
    }

    return Bool32(f.eof);
}

public static partial void Xclearerr(ж<TLS> t, uintptr stream) {
    if (__ccgo_strace) {
        trc("t=%v stream=%v, (%v:)", t.OrTypedNil(), stream, origin(2));
    }
    var (f, ok) = winGetObject(stream)._<ж<@file>>(ᐧ); if (ok) {
        f.hadErr = false;
        f.eof = false;
    }
}

public static partial int32 Xfscanf(ж<TLS> t, uintptr stream, uintptr format, uintptr va) {
    if (__ccgo_strace) {
        trc("t=%v va=%v, (%v:)", t.OrTypedNil(), va, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xfputs(ж<TLS> t, uintptr s, uintptr stream) {
    if (__ccgo_strace) {
        trc("t=%v stream=%v, (%v:)", t.OrTypedNil(), stream, origin(2));
    }
    var (f, ok) = winGetObject(stream)._<ж<@file>>(ᐧ);
    if (!ok) {
        t.setErrno((nint)(errno.EBADF));
        return -1;
    }
    @string gS = GoString(s);
    var (_, err) = fwrite(f._fd, slice<byte>(gS)); if (err != nil) {
        return -1;
    }
    return 0;
}

public static partial uintptr X_errno(ж<TLS> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (__ccgo_strace) {
        trc("t=%v, (%v:)", Ꮡt.OrTypedNil(), origin(2));
    }
    return t.errnop;
}

public static partial int32 X__ms_vfscanf(ж<TLS> t, uintptr stream, uintptr format, uintptr ap) {
    if (__ccgo_strace) {
        trc("t=%v ap=%v, (%v:)", t.OrTypedNil(), ap, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 X__ms_vsscanf(ж<TLS> t, uintptr str, uintptr format, uintptr ap) {
    if (__ccgo_strace) {
        trc("t=%v ap=%v, (%v:)", t.OrTypedNil(), ap, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 X__ms_vscanf(ж<TLS> t, uintptr format, uintptr ap) {
    if (__ccgo_strace) {
        trc("t=%v ap=%v, (%v:)", t.OrTypedNil(), ap, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 X__ms_vsnprintf(ж<TLS> t, uintptr str, typesꓸSize_t size, uintptr format, uintptr ap) {
    if (__ccgo_strace) {
        trc("t=%v str=%v size=%v ap=%v, (%v:)", t.OrTypedNil(), str, size, ap, origin(2));
    }
    return Xvsnprintf(t, str, size, format, ap);
}

public static partial int32 X__ms_vfwscanf(ж<TLS> t, uintptr stream, uintptr format, uintptr ap) {
    if (__ccgo_strace) {
        trc("t=%v stream=%v ap=%v, (%v:)", t.OrTypedNil(), stream, ap, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 X__ms_vwscanf(ж<TLS> t, uintptr format, uintptr ap) {
    if (__ccgo_strace) {
        trc("t=%v ap=%v, (%v:)", t.OrTypedNil(), ap, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 X_vsnwprintf(ж<TLS> t, uintptr buffer, typesꓸSize_t count, uintptr format, uintptr ap) {
    if (__ccgo_strace) {
        trc("t=%v buffer=%v count=%v ap=%v, (%v:)", t.OrTypedNil(), buffer, count, ap, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 X__ms_vswscanf(ж<TLS> t, uintptr stream, uintptr format, uintptr ap) {
    if (__ccgo_strace) {
        trc("t=%v stream=%v ap=%v, (%v:)", t.OrTypedNil(), stream, ap, origin(2));
    }
    throw panic(todo(""));
}

public static partial uintptr X__acrt_iob_func(ж<TLS> Ꮡt, uint32 fd) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (__ccgo_strace) {
        trc("t=%v fd=%v, (%v:)", Ꮡt.OrTypedNil(), fd, origin(2));
    }

    var (f, ok) = fdToFile((int32)fd);
    if (!ok) {
        Ꮡt.setErrno(EBADF);
        return 0;
    }
    return f.t;
}

public static partial int32 XSetEvent(ж<TLS> t, uintptr hEvent) {
    if (__ccgo_strace) {
        trc("t=%v hEvent=%v, (%v:)", t.OrTypedNil(), hEvent, origin(2));
    }
    var (r0, _, err) = procSetEvent.Call(hEvent, 0, 0);
    if (r0 == 0) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial int32 X_stricmp(ж<TLS> t, uintptr string1, uintptr string2) {
    if (__ccgo_strace) {
        trc("t=%v string2=%v, (%v:)", t.OrTypedNil(), string2, origin(2));
    }
    @string s1 = strings.ToLower(GoString(string1));
    @string s2 = strings.ToLower(GoString(string2));
    return (int32)strings.Compare(s1, s2);
}

public static partial int32 XHeapFree(ж<TLS> t, uintptr hHeap, uint32 dwFlags, uintptr lpMem) {
    if (__ccgo_strace) {
        trc("t=%v hHeap=%v dwFlags=%v lpMem=%v, (%v:)", t.OrTypedNil(), hHeap, dwFlags, lpMem, origin(2));
    }
    var (r0, _, err) = procHeapFree.Call(hHeap, (uintptr)dwFlags, lpMem);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial uintptr XGetProcessHeap(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    var (r0, _, err) = procGetProcessHeap.Call(0, 0, 0);
    if (r0 == 0) {
        t.setErrno(err);
    }
    return r0;
}

public static partial uintptr XHeapAlloc(ж<TLS> t, uintptr hHeap, uint32 dwFlags, typesꓸSize_t dwBytes) {
    if (__ccgo_strace) {
        trc("t=%v hHeap=%v dwFlags=%v dwBytes=%v, (%v:)", t.OrTypedNil(), hHeap, dwFlags, dwBytes, origin(2));
    }
    var (r0, _, err) = procHeapAlloc.Call(hHeap, (uintptr)dwFlags, (uintptr)dwBytes);
    if (r0 == 0) {
        t.setErrno(err);
    }
    return r0;
}

public static partial uintptr Xgai_strerrorW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial uintptr Xgetservbyname(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XWspiapiGetAddrInfo(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 Xwcscmp(ж<TLS> t, uintptr string1, uintptr string2) {
    if (__ccgo_strace) {
        trc("t=%v string2=%v, (%v:)", t.OrTypedNil(), string2, origin(2));
    }
    @string s1 = goWideString(string1);
    @string s2 = goWideString(string2);
    return (int32)strings.Compare(s1, s2);
}

public static partial int32 XIsDebuggerPresent(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    return 0;
}

public static partial int32 XExitProcess(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XGetVersionExW(ж<TLS> t, uintptr lpVersionInformation) {
    if (__ccgo_strace) {
        trc("t=%v lpVersionInformation=%v, (%v:)", t.OrTypedNil(), lpVersionInformation, origin(2));
    }
    var (r0, _, err) = procGetVersionExW.Call(lpVersionInformation, 0, 0);
    if (r0 == 0) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial int32 XGetVolumeNameForVolumeMountPointW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial typesꓸSize_t Xwcslen(ж<TLS> t, uintptr str) {
    if (__ccgo_strace) {
        trc("t=%v str=%v, (%v:)", t.OrTypedNil(), str, origin(2));
    }
    var (r0, _, _) = procLstrlenW.Call(str, 0, 0);
    return (typesꓸSize_t)r0;
}

public static partial uintptr XGetStdHandle(ж<TLS> t, uint32 nStdHandle) {
    if (__ccgo_strace) {
        trc("t=%v nStdHandle=%v, (%v:)", t.OrTypedNil(), nStdHandle, origin(2));
    }
    var (h, err) = windows.GetStdHandle(nStdHandle);
    if (err != nil) {
        throw panic("no console");
    }
    return (uintptr)h;
}

public static partial int32 XCloseHandle(ж<TLS> t, uintptr hObject) {
    if (__ccgo_strace) {
        trc("t=%v hObject=%v, (%v:)", t.OrTypedNil(), hObject, origin(2));
    }
    var err = windows.CloseHandle((windows.Handle)hObject); if (err != nil) {
        t.setErrno(err);
        return 0;
    }

    return 1;
}

public static partial uint32 XGetLastError(ж<TLS> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (__ccgo_strace) {
        trc("t=%v, (%v:)", Ꮡt.OrTypedNil(), origin(2));
    }
    int32 rv = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)t.errnop);
    return (uint32)rv;
}

public static partial uint32 XSetFilePointer(ж<TLS> t, uintptr hFile, @long lDistanceToMove, uintptr lpDistanceToMoveHigh, uint32 dwMoveMethod) {
    if (__ccgo_strace) {
        trc("t=%v hFile=%v lDistanceToMove=%v lpDistanceToMoveHigh=%v dwMoveMethod=%v, (%v:)", t.OrTypedNil(), hFile, lDistanceToMove, lpDistanceToMoveHigh, dwMoveMethod, origin(2));
    }
    var (r0, _, e1) = procSetFilePointer.Call(hFile, (uintptr)lDistanceToMove, lpDistanceToMoveHigh, (uintptr)dwMoveMethod, 0, 0);
    uint32 uOff = (uint32)r0;
    if (uOff == 0xffffffffU) {
        if (!AreEqual(e1, windows.NOERROR)) {
            t.setErrno(e1);
        } else {
            t.setErrno((nint)(errno.EINVAL));
        }
    }
    return (uint32)r0;
}

public static partial int32 XSetEndOfFile(ж<TLS> t, uintptr hFile) {
    if (__ccgo_strace) {
        trc("t=%v hFile=%v, (%v:)", t.OrTypedNil(), hFile, origin(2));
    }
    var err = windows.SetEndOfFile((windows.Handle)hFile);
    if (err != nil) {
        t.setErrno(err);
        return 0;
    }
    return 1;
}

public static partial int32 XReadFile(ж<TLS> t, uintptr hFile, uintptr lpBuffer, uint32 nNumberOfBytesToRead, uintptr lpNumberOfBytesRead, uintptr lpOverlapped) {
    if (__ccgo_strace) {
        trc("t=%v lpBuffer=%v nNumberOfBytesToRead=%v lpOverlapped=%v, (%v:)", t.OrTypedNil(), lpBuffer, nNumberOfBytesToRead, lpOverlapped, origin(2));
    }
    var (r1, _, e1) = procReadFile.Call(hFile, lpBuffer, (uintptr)nNumberOfBytesToRead, (uintptr)lpNumberOfBytesRead, (uintptr)lpOverlapped, 0);
    if (r1 == 0) {
        if (!AreEqual(e1, windows.NOERROR)) {
            t.setErrno(e1);
        } else {
            t.setErrno((nint)(errno.EINVAL));
        }
        return 0;
    }
    return (int32)r1;
}

public static partial int32 XWriteFile(ж<TLS> t, uintptr hFile, uintptr lpBuffer, uint32 nNumberOfBytesToWrite, uintptr lpNumberOfBytesWritten, uintptr lpOverlapped) {
    if (__ccgo_strace) {
        trc("t=%v lpBuffer=%v nNumberOfBytesToWrite=%v lpOverlapped=%v, (%v:)", t.OrTypedNil(), lpBuffer, nNumberOfBytesToWrite, lpOverlapped, origin(2));
    }
    var (r1, _, e1) = procWriteFile.Call(hFile, lpBuffer, (uintptr)nNumberOfBytesToWrite, lpNumberOfBytesWritten, lpOverlapped, 0);
    if (r1 == 0) {
        if (!AreEqual(e1, windows.NOERROR)) {
            t.setErrno(e1);
        } else {
            t.setErrno((nint)(errno.EINVAL));
        }
        return 0;
    }
    return (int32)r1;
}

public static partial uint32 XGetFileAttributesW(ж<TLS> t, uintptr lpFileName) {
    if (__ccgo_strace) {
        trc("t=%v lpFileName=%v, (%v:)", t.OrTypedNil(), lpFileName, origin(2));
    }
    var (attrs, err) = windows.GetFileAttributes(@unsafe.Pointer.To<uint16>((@unsafe.Pointer)lpFileName));
    if (attrs == windows.INVALID_FILE_ATTRIBUTES) {
        if (err != nil) {
            t.setErrno(err);
        } else {
            t.setErrno((nint)(errno.EINVAL));
        }
    }
    return attrs;
}

public static partial uintptr XCreateFileW(ж<TLS> t, uintptr lpFileName, uint32 dwDesiredAccess, uint32 dwShareMode, uintptr lpSecurityAttributes, uint32 dwCreationDisposition, uint32 dwFlagsAndAttributes, uintptr hTemplateFile) {
    if (__ccgo_strace) {
        trc("t=%v lpFileName=%v dwShareMode=%v lpSecurityAttributes=%v dwFlagsAndAttributes=%v hTemplateFile=%v, (%v:)", t.OrTypedNil(), lpFileName, dwShareMode, lpSecurityAttributes, dwFlagsAndAttributes, hTemplateFile, origin(2));
    }

    var (r0, _, e1) = procCreateFileW.Call(lpFileName, (uintptr)dwDesiredAccess, (uintptr)dwShareMode, lpSecurityAttributes,
        (uintptr)dwCreationDisposition, (uintptr)dwFlagsAndAttributes, hTemplateFile, 0, 0);
    var h = (windows.Handle)r0;
    if (h == windows.InvalidHandle) {
        if (!AreEqual(e1, windows.NOERROR)) {
            t.setErrno(e1);
        } else {
            t.setErrno((nint)(errno.EINVAL));
        }
        return r0;
    }
    return (uintptr)h;
}

public static partial int32 XDuplicateHandle(ж<TLS> t, uintptr hSourceProcessHandle, uintptr hSourceHandle, uintptr hTargetProcessHandle, uintptr lpTargetHandle, uint32 dwDesiredAccess, int32 bInheritHandle, uint32 dwOptions) {
    if (__ccgo_strace) {
        trc("t=%v lpTargetHandle=%v dwDesiredAccess=%v bInheritHandle=%v dwOptions=%v, (%v:)", t.OrTypedNil(), lpTargetHandle, dwDesiredAccess, bInheritHandle, dwOptions, origin(2));
    }
    var (r0, _, err) = procDuplicateHandle.Call(hSourceProcessHandle, hSourceHandle, hTargetProcessHandle,
        lpTargetHandle, (uintptr)dwDesiredAccess, (uintptr)bInheritHandle, (uintptr)dwOptions, 0, 0);
    if (r0 == 0) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial uintptr XGetCurrentProcess(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    var (r0, _, e1) = procGetCurrentProcess.Call(0, 0, 0);
    if (r0 == 0) {
        if (!AreEqual(e1, windows.NOERROR)) {
            t.setErrno(e1);
        } else {
            t.setErrno((nint)(errno.EINVAL));
        }
    }
    return r0;
}

public static partial int32 XFlushFileBuffers(ж<TLS> t, uintptr hFile) {
    if (__ccgo_strace) {
        trc("t=%v hFile=%v, (%v:)", t.OrTypedNil(), hFile, origin(2));
    }
    var err = windows.FlushFileBuffers((windows.Handle)hFile);
    if (err != nil) {
        t.setErrno(err);
        return -1;
    }
    return 1;
}

public static partial uint32 XGetFileType(ж<TLS> t, uintptr hFile) {
    if (__ccgo_strace) {
        trc("t=%v hFile=%v, (%v:)", t.OrTypedNil(), hFile, origin(2));
    }
    var (n, err) = windows.GetFileType((windows.Handle)hFile);
    if (err != nil) {
        t.setErrno(err);
    }
    return n;
}

public static partial int32 XGetConsoleMode(ж<TLS> t, uintptr hConsoleHandle, uintptr lpMode) {
    if (__ccgo_strace) {
        trc("t=%v lpMode=%v, (%v:)", t.OrTypedNil(), lpMode, origin(2));
    }
    var err = windows.GetConsoleMode((windows.Handle)hConsoleHandle, @unsafe.Pointer.To<uint32>((@unsafe.Pointer)lpMode));
    if (err != nil) {
        t.setErrno(err);
        return 0;
    }
    return 1;
}

public static partial int32 XGetCommState(ж<TLS> t, uintptr hFile, uintptr lpDCB) {
    if (__ccgo_strace) {
        trc("t=%v lpDCB=%v, (%v:)", t.OrTypedNil(), lpDCB, origin(2));
    }
    var (r1, _, err) = procGetCommState.Call(hFile, lpDCB, 0);
    if (r1 == 0) {
        t.setErrno(err);
        return 0;
    }
    return (int32)r1;
}

public static partial int32 X_wcsnicmp(ж<TLS> t, uintptr string1, uintptr string2, typesꓸSize_t count) {
    if (__ccgo_strace) {
        trc("t=%v string2=%v count=%v, (%v:)", t.OrTypedNil(), string2, count, origin(2));
    }

    @string s1 = strings.ToLower(goWideString(string1));
    nint l1 = len(s1);
    @string s2 = strings.ToLower(goWideString(string2));
    nint l2 = len(s2);

    if (l1 < l2) {
        return -1;
    }
    if (l2 > l1) {
        return 1;
    }

    typesꓸSize_t cmpLen = count;
    if (((typesꓸSize_t)l1) < cmpLen) {
        cmpLen = (typesꓸSize_t)l1;
    }
    return (int32)strings.Compare(s1.slice(0, (nint)(cmpLen)), s2.slice(0, (nint)(cmpLen)));
}

public static partial int32 XReadConsoleW(ж<TLS> t, uintptr hConsoleInput, uintptr lpBuffer, uint32 nNumberOfCharsToRead, uintptr lpNumberOfCharsRead, uintptr pInputControl) {
    if (__ccgo_strace) {
        trc("t=%v lpBuffer=%v nNumberOfCharsToRead=%v pInputControl=%v, (%v:)", t.OrTypedNil(), lpBuffer, nNumberOfCharsToRead, pInputControl, origin(2));
    }

    var (rv, _, err) = procReadConsoleW.Call(hConsoleInput,
        lpBuffer, (uintptr)nNumberOfCharsToRead, lpNumberOfCharsRead, pInputControl, 0);
    if (rv == 0) {
        t.setErrno(err);
    }

    return (int32)rv;
}

public static partial int32 XWriteConsoleW(ж<TLS> t, uintptr hConsoleOutput, uintptr lpBuffer, uint32 nNumberOfCharsToWrite, uintptr lpNumberOfCharsWritten, uintptr lpReserved) {
    if (__ccgo_strace) {
        trc("t=%v lpBuffer=%v nNumberOfCharsToWrite=%v lpReserved=%v, (%v:)", t.OrTypedNil(), lpBuffer, nNumberOfCharsToWrite, lpReserved, origin(2));
    }
    var (rv, _, err) = procWriteConsoleW.Call(hConsoleOutput,
        lpBuffer, (uintptr)nNumberOfCharsToWrite, lpNumberOfCharsWritten, lpReserved, 0);
    if (rv == 0) {
        t.setErrno(err);
    }
    return (int32)rv;
}

public static partial uint32 XWaitForSingleObject(ж<TLS> t, uintptr hHandle, uint32 dwMilliseconds) {
    if (__ccgo_strace) {
        trc("t=%v hHandle=%v dwMilliseconds=%v, (%v:)", t.OrTypedNil(), hHandle, dwMilliseconds, origin(2));
    }
    var (rv, err) = windows.WaitForSingleObject((windows.Handle)hHandle, dwMilliseconds);
    if (err != nil) {
        t.setErrno(err);
    }
    return rv;
}

public static partial int32 XResetEvent(ж<TLS> t, uintptr hEvent) {
    if (__ccgo_strace) {
        trc("t=%v hEvent=%v, (%v:)", t.OrTypedNil(), hEvent, origin(2));
    }
    var (rv, _, err) = procResetEvent.Call(hEvent, 0, 0);
    if (rv == 0) {
        t.setErrno(err);
    }
    return (int32)rv;
}

public static partial int32 XPeekConsoleInputW(ж<TLS> t, uintptr hConsoleInput, uintptr lpBuffer, uint32 nLength, uintptr lpNumberOfEventsRead) {
    if (__ccgo_strace) {
        trc("t=%v lpBuffer=%v nLength=%v lpNumberOfEventsRead=%v, (%v:)", t.OrTypedNil(), lpBuffer, nLength, lpNumberOfEventsRead, origin(2));
    }
    var (r0, _, err) = procPeekConsoleInputW.Call(hConsoleInput, lpBuffer, (uintptr)nLength, lpNumberOfEventsRead, 0, 0);
    if (r0 == 0) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial int32 XwsprintfA(ж<TLS> t, uintptr buf, uintptr format, uintptr args) {
    if (__ccgo_strace) {
        trc("t=%v args=%v, (%v:)", t.OrTypedNil(), args, origin(2));
    }
    return Xsprintf(t, buf, format, args);
}

public static partial uint32 XGetConsoleCP(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    var (r0, _, err) = procGetConsoleCP.Call(0, 0, 0);
    if (r0 == 0) {
        t.setErrno(err);
    }
    return (uint32)r0;
}

public static partial uintptr XCreateEventW(ж<TLS> t, uintptr lpEventAttributes, int32 bManualReset, int32 bInitialState, uintptr lpName) {
    if (__ccgo_strace) {
        trc("t=%v lpEventAttributes=%v bInitialState=%v lpName=%v, (%v:)", t.OrTypedNil(), lpEventAttributes, bInitialState, lpName, origin(2));
    }
    var (r0, _, err) = procCreateEventW.Call(lpEventAttributes, (uintptr)bManualReset,
        (uintptr)bInitialState, lpName, 0, 0);
    if (r0 == 0) {
        t.setErrno(err);
    }
    return r0;
}

partial struct ThreadAdapter {
    internal uintptr token;
    internal ж<TLS> tls;
    internal uintptr param;
    internal Func<ж<TLS>, uintptr, uint32> threadFunc;
}

} partial class libcᴛThreadAdapter {

internal static uintptr run(this ref ThreadAdapter ta) {
    var r = ta.threadFunc(ta.tls, ta.param);
    ta.tls.Close();
    removeObject(ta.token);
    return (uintptr)r;
}

} partial class libc_package {

public static uintptr ThreadProc(uintptr p) {
    var (adp, ok) = winGetObject(p)._<ж<ThreadAdapter>>(ᐧ);
    if (!ok) {
        throw panic("invalid thread");
    }
    return adp.run();
}

public static partial uintptr XCreateThread(ж<TLS> t, uintptr lpThreadAttributes, typesꓸSize_t dwStackSize, uintptr lpStartAddress, uintptr lpParameter, uint32 dwCreationFlags, uintptr lpThreadId) {
    if (__ccgo_strace) {
        trc("t=%v lpThreadAttributes=%v dwStackSize=%v lpParameter=%v dwCreationFlags=%v lpThreadId=%v, (%v:)", t.OrTypedNil(), lpThreadAttributes, dwStackSize, lpParameter, dwCreationFlags, lpThreadId, origin(2));
    }
    var f = GoFuncCookie.Func<Func<ж<TLS>, uintptr, uint32>>(lpStartAddress);
    ref var tAdp = ref heap(new ThreadAdapter(), out var ᏑtAdp);

    tAdp = new ThreadAdapter(threadFunc: f, tls: NewTLS(), param: lpParameter);
    tAdp.token = addObject(ᏑtAdp);

    var (r0, _, err) = procCreateThread.Call(lpThreadAttributes, (uintptr)dwStackSize,
        threadCallback, tAdp.token, (uintptr)dwCreationFlags, lpThreadId);
    if (r0 == 0) {
        t.setErrno(err);
    }
    return r0;
}

public static partial int32 XSetThreadPriority(ж<TLS> t, uintptr hThread, int32 nPriority) {
    if (__ccgo_strace) {
        trc("t=%v hThread=%v nPriority=%v, (%v:)", t.OrTypedNil(), hThread, nPriority, origin(2));
    }

    return 1;
}

public static partial int32 XSetConsoleMode(ж<TLS> t, uintptr hConsoleHandle, uint32 dwMode) {
    if (__ccgo_strace) {
        trc("t=%v hConsoleHandle=%v dwMode=%v, (%v:)", t.OrTypedNil(), hConsoleHandle, dwMode, origin(2));
    }
    var (rv, _, err) = procSetConsoleMode.Call(hConsoleHandle, (uintptr)dwMode, 0);
    if (rv == 0) {
        t.setErrno(err);
    }
    return (int32)rv;
}

public static partial int32 XPurgeComm(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XClearCommError(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial void XDeleteCriticalSection(ж<TLS> t, uintptr lpCriticalSection) {
    if (__ccgo_strace) {
        trc("t=%v lpCriticalSection=%v, (%v:)", t.OrTypedNil(), lpCriticalSection, origin(2));
    }
    procDeleteCriticalSection.Call(lpCriticalSection, 0, 0);
}

public static partial void XEnterCriticalSection(ж<TLS> t, uintptr lpCriticalSection) {
    if (__ccgo_strace) {
        trc("t=%v lpCriticalSection=%v, (%v:)", t.OrTypedNil(), lpCriticalSection, origin(2));
    }
    procEnterCriticalSection.Call(lpCriticalSection, 0, 0);
}

public static partial int32 /*r*/ XTryEnterCriticalSection(ж<TLS> t, uintptr lpCriticalSection) {
    if (__ccgo_strace) {
        trc("t=%v lpCriticalSection=%v, (%v:)", t.OrTypedNil(), lpCriticalSection, origin(2));
    }
    var (r0, _, err) = procTryEnterCriticalSection.Call(lpCriticalSection);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial void XLeaveCriticalSection(ж<TLS> t, uintptr lpCriticalSection) {
    if (__ccgo_strace) {
        trc("t=%v lpCriticalSection=%v, (%v:)", t.OrTypedNil(), lpCriticalSection, origin(2));
    }
    procLeaveCriticalSection.Call(lpCriticalSection, 0, 0);
}

public static partial int32 XGetOverlappedResult(ж<TLS> t, uintptr hFile, uintptr lpOverlapped, uintptr lpNumberOfBytesTransferred, int32 bWait) {
    if (__ccgo_strace) {
        trc("t=%v hFile=%v lpOverlapped=%v lpNumberOfBytesTransferred=%v bWait=%v, (%v:)", t.OrTypedNil(), hFile, lpOverlapped, lpNumberOfBytesTransferred, bWait, origin(2));
    }
    var err = windows.GetOverlappedResult((windows.Handle)hFile, @unsafe.Pointer.To<windows.Overlapped>((@unsafe.Pointer)lpOverlapped), @unsafe.Pointer.To<uint32>((@unsafe.Pointer)lpNumberOfBytesTransferred), bWait != 0); if (err != nil) {
        t.setErrno(err);
        return 0;
    }

    return 1;
}

public static partial int32 XSetupComm(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XSetCommTimeouts(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial void XInitializeCriticalSection(ж<TLS> t, uintptr lpCriticalSection) {
    if (__ccgo_strace) {
        trc("t=%v lpCriticalSection=%v, (%v:)", t.OrTypedNil(), lpCriticalSection, origin(2));
    }
    procInitializeCriticalSection.Call(lpCriticalSection, 0, 0);
}

public static partial int32 XBuildCommDCBW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XSetCommState(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XEscapeCommFunction(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XGetCommModemStatus(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XMoveFileW(ж<TLS> t, uintptr lpExistingFileName, uintptr lpNewFileName) {
    if (__ccgo_strace) {
        trc("t=%v lpNewFileName=%v, (%v:)", t.OrTypedNil(), lpNewFileName, origin(2));
    }
    var (r0, _, err) = procMoveFileW.Call(lpExistingFileName, lpNewFileName, 0);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial uint32 XGetFullPathNameW(ж<TLS> t, uintptr lpFileName, uint32 nBufferLength, uintptr lpBuffer, uintptr lpFilePart) {
    if (__ccgo_strace) {
        trc("t=%v lpFileName=%v nBufferLength=%v lpFilePart=%v, (%v:)", t.OrTypedNil(), lpFileName, nBufferLength, lpFilePart, origin(2));
    }
    var (r0, _, e1) = procGetFullPathNameW.Call(lpFileName, (uintptr)nBufferLength, (uintptr)lpBuffer, (uintptr)lpFilePart, 0, 0);
    var n = (uint32)r0;
    if (n == 0) {
        if (!AreEqual(e1, windows.NOERROR)) {
            t.setErrno(e1);
        } else {
            t.setErrno((nint)(errno.EINVAL));
        }
    }
    return n;
}

public static uintptr /*r*/ XCharLowerW(ж<TLS> tls, uintptr _lpsz) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("lpsz=%+v", _lpsz);
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc(@"XCharLowerW->%+v", r);
            }, ref ᒐ);
        }
        var (r0, r1, err) = procCharLowerW.Call(_lpsz);
        if (!AreEqual(err, windows.NOERROR)) {
            if (__ccgo_strace) {
                trc(@"r0=%v r1=%v err=%v", r0, r1, err);
            }
            tls.SetLastError((uint32)(err._<windowsꓸErrno>()));
        }
        r = r0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static partial int32 XCreateDirectoryW(ж<TLS> t, uintptr lpPathName, uintptr lpSecurityAttributes) {
    if (__ccgo_strace) {
        trc("t=%v lpSecurityAttributes=%v, (%v:)", t.OrTypedNil(), lpSecurityAttributes, origin(2));
    }
    var err = windows.CreateDirectory(@unsafe.Pointer.To<uint16>((@unsafe.Pointer)lpPathName),
        @unsafe.Pointer.To<windows.SecurityAttributes>((@unsafe.Pointer)lpSecurityAttributes));
    if (err != nil) {
        t.setErrno(err);
        return 0;
    }
    return 1;
}

public static partial int32 XSetFileAttributesW(ж<TLS> t, uintptr lpFileName, uint32 dwFileAttributes) {
    if (__ccgo_strace) {
        trc("t=%v lpFileName=%v dwFileAttributes=%v, (%v:)", t.OrTypedNil(), lpFileName, dwFileAttributes, origin(2));
    }
    var err = windows.SetFileAttributes(@unsafe.Pointer.To<uint16>((@unsafe.Pointer)lpFileName), dwFileAttributes);
    if (err != nil) {
        t.setErrno(err);
        return 0;
    }
    return 1;
}

public static partial uint32 XGetTempFileNameW(ж<TLS> t, uintptr lpPathName, uintptr lpPrefixString, uint32 uUnique, uintptr lpTempFileName) {
    if (__ccgo_strace) {
        trc("t=%v lpPrefixString=%v uUnique=%v lpTempFileName=%v, (%v:)", t.OrTypedNil(), lpPrefixString, uUnique, lpTempFileName, origin(2));
    }
    var (r0, _, e1) = procGetTempFileNameW.Call(lpPathName, lpPrefixString, (uintptr)uUnique, lpTempFileName, 0, 0);
    if (r0 == 0) {
        t.setErrno(e1);
    }
    return (uint32)r0;
}

public static partial int32 XCopyFileW(ж<TLS> t, uintptr lpExistingFileName, uintptr lpNewFileName, int32 bFailIfExists) {
    if (__ccgo_strace) {
        trc("t=%v lpNewFileName=%v bFailIfExists=%v, (%v:)", t.OrTypedNil(), lpNewFileName, bFailIfExists, origin(2));
    }
    var (r0, _, e1) = procCopyFileW.Call(lpExistingFileName, lpNewFileName, (uintptr)bFailIfExists);
    if (r0 == 0) {
        t.setErrno(e1);
    }
    return (int32)r0;
}

public static partial int32 XDeleteFileW(ж<TLS> t, uintptr lpFileName) {
    if (__ccgo_strace) {
        trc("t=%v lpFileName=%v, (%v:)", t.OrTypedNil(), lpFileName, origin(2));
    }
    var err = windows.DeleteFile(@unsafe.Pointer.To<uint16>((@unsafe.Pointer)lpFileName));
    if (err != nil) {
        t.setErrno(err);
        return 0;
    }
    return 1;
}

public static partial int32 XRemoveDirectoryW(ж<TLS> t, uintptr lpPathName) {
    if (__ccgo_strace) {
        trc("t=%v lpPathName=%v, (%v:)", t.OrTypedNil(), lpPathName, origin(2));
    }
    var err = windows.RemoveDirectory(@unsafe.Pointer.To<uint16>((@unsafe.Pointer)lpPathName));
    if (err != nil) {
        t.setErrno(err);
        return 0;
    }
    return 1;
}

public static partial uintptr XFindFirstFileW(ж<TLS> t, uintptr lpFileName, uintptr lpFindFileData) {
    if (__ccgo_strace) {
        trc("t=%v lpFindFileData=%v, (%v:)", t.OrTypedNil(), lpFindFileData, origin(2));
    }
    var (r0, _, e1) = procFindFirstFileW.Call(lpFileName, lpFindFileData, 0);
    var handle = (windows.Handle)r0;
    if (handle == windows.InvalidHandle) {
        if (!AreEqual(e1, windows.NOERROR)) {
            t.setErrno(e1);
        } else {
            t.setErrno((nint)(errno.EINVAL));
        }
    }
    return r0;
}

public static partial uintptr XFindFirstFileExW(ж<TLS> t, uintptr lpFileName, int32 fInfoLevelId, uintptr lpFindFileData, int32 fSearchOp, uintptr lpSearchFilter, uint32 dwAdditionalFlags) {
    if (__ccgo_strace) {
        trc("t=%v lpFileName=%v fInfoLevelId=%v lpFindFileData=%v fSearchOp=%v lpSearchFilter=%v dwAdditionalFlags=%v, (%v:)", t.OrTypedNil(), lpFileName, fInfoLevelId, lpFindFileData, fSearchOp, lpSearchFilter, dwAdditionalFlags, origin(2));
    }
    var (r0, _, e1) = procFindFirstFileExW.Call(lpFileName, (uintptr)fInfoLevelId, lpFindFileData, (uintptr)fSearchOp, lpSearchFilter, (uintptr)dwAdditionalFlags);
    var handle = (windows.Handle)r0;
    if (handle == windows.InvalidHandle) {
        if (!AreEqual(e1, windows.NOERROR)) {
            t.setErrno(e1);
        } else {
            t.setErrno((nint)(errno.EINVAL));
        }
    }
    return r0;
}

public static partial int32 XFindClose(ж<TLS> t, uintptr hFindFile) {
    if (__ccgo_strace) {
        trc("t=%v hFindFile=%v, (%v:)", t.OrTypedNil(), hFindFile, origin(2));
    }
    var (r0, _, e1) = procFindClose.Call(hFindFile, 0, 0);
    if (r0 == 0) {
        if (!AreEqual(e1, windows.NOERROR)) {
            t.setErrno(e1);
        } else {
            t.setErrno((nint)(errno.EINVAL));
        }
    }
    return (int32)r0;
}

public static partial int32 XFindNextFileW(ж<TLS> t, uintptr hFindFile, uintptr lpFindFileData) {
    if (__ccgo_strace) {
        trc("t=%v lpFindFileData=%v, (%v:)", t.OrTypedNil(), lpFindFileData, origin(2));
    }
    var (r0, _, e1) = procFindNextFileW.Call(hFindFile, lpFindFileData, 0);
    if (r0 == 0) {
        if (!AreEqual(e1, windows.NOERROR)) {
            t.setErrno(e1);
        } else {
            t.setErrno((nint)(errno.EINVAL));
        }
    }
    return (int32)r0;
}

public static partial uint32 XGetLogicalDriveStringsA(ж<TLS> t, uint32 nBufferLength, uintptr lpBuffer) {
    if (__ccgo_strace) {
        trc("t=%v nBufferLength=%v lpBuffer=%v, (%v:)", t.OrTypedNil(), nBufferLength, lpBuffer, origin(2));
    }
    var (r0, _, err) = procGetLogicalDriveStringsA.Call((uintptr)nBufferLength, lpBuffer, 0);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (uint32)r0;
}

public static partial int32 XGetVolumeInformationA(ж<TLS> t, uintptr lpRootPathName, uintptr lpVolumeNameBuffer, uint32 nVolumeNameSize, uintptr lpVolumeSerialNumber, uintptr lpMaximumComponentLength, uintptr lpFileSystemFlags, uintptr lpFileSystemNameBuffer, uint32 nFileSystemNameSize) {
    if (__ccgo_strace) {
        trc("t=%v lpVolumeNameBuffer=%v nVolumeNameSize=%v lpFileSystemNameBuffer=%v nFileSystemNameSize=%v, (%v:)", t.OrTypedNil(), lpVolumeNameBuffer, nVolumeNameSize, lpFileSystemNameBuffer, nFileSystemNameSize, origin(2));
    }
    var (r0, _, err) = procGetVolumeInformationA.Call(lpRootPathName,
        lpVolumeNameBuffer,
        (uintptr)nVolumeNameSize,
        lpVolumeSerialNumber,
        lpMaximumComponentLength,
        lpFileSystemFlags,
        lpFileSystemNameBuffer,
        (uintptr)nFileSystemNameSize,
        0);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial int32 XCreateHardLinkW(ж<TLS> t, uintptr lpFileName, uintptr lpExistingFileName, uintptr lpSecurityAttributes) {
    if (__ccgo_strace) {
        trc("t=%v lpSecurityAttributes=%v, (%v:)", t.OrTypedNil(), lpSecurityAttributes, origin(2));
    }
    var (r0, _, err) = procCreateHardLinkW.Call(lpFileName, lpExistingFileName, lpSecurityAttributes);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial int32 XDeviceIoControl(ж<TLS> t, uintptr hDevice, uint32 dwIoControlCode, uintptr lpInBuffer, uint32 nInBufferSize, uintptr lpOutBuffer, uint32 nOutBufferSize, uintptr lpBytesReturned, uintptr lpOverlapped) {
    if (__ccgo_strace) {
        trc("t=%v hDevice=%v dwIoControlCode=%v lpInBuffer=%v nInBufferSize=%v lpOutBuffer=%v nOutBufferSize=%v lpOverlapped=%v, (%v:)", t.OrTypedNil(), hDevice, dwIoControlCode, lpInBuffer, nInBufferSize, lpOutBuffer, nOutBufferSize, lpOverlapped, origin(2));
    }
    var (r0, _, err) = procDeviceIoControl.Call(hDevice, (uintptr)dwIoControlCode, lpInBuffer,
        (uintptr)nInBufferSize, lpOutBuffer, (uintptr)nOutBufferSize, lpBytesReturned, lpOverlapped, 0);
    if (r0 == 0) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial int32 Xwcsncmp(ж<TLS> t, uintptr string1, uintptr string2, typesꓸSize_t count) {
    if (__ccgo_strace) {
        trc("t=%v string2=%v count=%v, (%v:)", t.OrTypedNil(), string2, count, origin(2));
    }
    @string s1 = goWideString(string1);
    nint l1 = len(s1);
    @string s2 = goWideString(string2);
    nint l2 = len(s2);

    if (l1 < l2) {
        return -1;
    }
    if (l2 > l1) {
        return 1;
    }

    typesꓸSize_t cmpLen = count;
    if (((typesꓸSize_t)l1) < cmpLen) {
        cmpLen = (typesꓸSize_t)l1;
    }
    return (int32)strings.Compare(s1.slice(0, (nint)(cmpLen)), s2.slice(0, (nint)(cmpLen)));
}

public static partial int32 XMultiByteToWideChar(ж<TLS> t, uint32 CodePage, uint32 dwFlags, uintptr lpMultiByteStr, int32 cbMultiByte, uintptr lpWideCharStr, int32 cchWideChar) {
    if (__ccgo_strace) {
        trc("t=%v CodePage=%v dwFlags=%v lpMultiByteStr=%v cbMultiByte=%v lpWideCharStr=%v cchWideChar=%v, (%v:)", t.OrTypedNil(), CodePage, dwFlags, lpMultiByteStr, cbMultiByte, lpWideCharStr, cchWideChar, origin(2));
    }
    var (r1, _, _) = procMultiByteToWideChar.Call((uintptr)CodePage, (uintptr)dwFlags, (uintptr)lpMultiByteStr,
        (uintptr)cbMultiByte, (uintptr)lpWideCharStr, (uintptr)cchWideChar);
    return ((int32)r1);
}

public static partial void XOutputDebugStringW(ж<TLS> t, uintptr lpOutputString) {
    if (__ccgo_strace) {
        trc("t=%v lpOutputString=%v, (%v:)", t.OrTypedNil(), lpOutputString, origin(2));
    }
    procOutputDebugStringW.Call(lpOutputString);
}

public static partial int32 XMessageBeep(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial @long X_InterlockedCompareExchange(ж<TLS> t, uintptr Destination, @long Exchange, @long Comparand) {
    if (__ccgo_strace) {
        trc("t=%v Destination=%v Comparand=%v, (%v:)", t.OrTypedNil(), Destination, Comparand, origin(2));
    }

    int32 v = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)Destination);
    _ = atomic.CompareAndSwapInt32(@unsafe.Pointer.To<int32>((@unsafe.Pointer)Destination), Comparand, Exchange);
    return (@long)v;
}

public static partial int32 Xrename(ж<TLS> t, uintptr oldpath, uintptr newpath) {
    if (__ccgo_strace) {
        trc("t=%v newpath=%v, (%v:)", t.OrTypedNil(), newpath, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 XAreFileApisANSI(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }

    var (r0, _, _) = procAreFileApisANSI.Call(0, 0, 0);
    return (int32)r0;
}

public static uintptr XCreateFileA(ж<TLS> t, uintptr lpFileName, uint32 dwDesiredAccess, uint32 dwShareMode, uintptr lpSecurityAttributes, uint32 dwCreationDisposition, uint32 dwFlagsAndAttributes, uintptr hTemplateFile) {
    var (r0, _, e1) = procCreateFileA.Call(lpFileName, (uintptr)dwDesiredAccess, (uintptr)dwShareMode, lpSecurityAttributes,
        (uintptr)dwCreationDisposition, (uintptr)dwFlagsAndAttributes, hTemplateFile, 0, 0);
    var h = (windows.Handle)r0;
    if (h == windows.InvalidHandle) {
        if (!AreEqual(e1, windows.NOERROR)) {
            t.setErrno(e1);
        } else {
            t.setErrno((nint)(errno.EINVAL));
        }
        return r0;
    }
    return (uintptr)h;
}

public static partial uintptr XCreateFileMappingA(ж<TLS> t, uintptr hFile, uintptr lpFileMappingAttributes, uint32 flProtect, uint32 dwMaximumSizeHigh, uint32 dwMaximumSizeLow, uintptr lpName) {
    if (__ccgo_strace) {
        trc("t=%v lpFileMappingAttributes=%v dwMaximumSizeLow=%v lpName=%v, (%v:)", t.OrTypedNil(), lpFileMappingAttributes, dwMaximumSizeLow, lpName, origin(2));
    }
    throw panic(todo(""));
}

public static partial uintptr XCreateFileMappingW(ж<TLS> t, uintptr hFile, uintptr lpFileMappingAttributes, uint32 flProtect, uint32 dwMaximumSizeHigh, uint32 dwMaximumSizeLow, uintptr lpName) {
    if (__ccgo_strace) {
        trc("t=%v lpFileMappingAttributes=%v dwMaximumSizeLow=%v lpName=%v, (%v:)", t.OrTypedNil(), lpFileMappingAttributes, dwMaximumSizeLow, lpName, origin(2));
    }
    var (h, _, e1) = procCreateFileMappingW.Call(hFile, lpFileMappingAttributes, (uintptr)flProtect,
        (uintptr)dwMaximumSizeHigh, (uintptr)dwMaximumSizeLow, lpName);
    if (h == 0) {
        if (!AreEqual(e1, windows.NOERROR)) {
            t.setErrno(e1);
        } else {
            t.setErrno((nint)(errno.EINVAL));
        }
    }
    return h;
}

public static partial uintptr XCreateMutexW(ж<TLS> t, uintptr lpMutexAttributes, int32 bInitialOwner, uintptr lpName) {
    if (__ccgo_strace) {
        trc("t=%v lpMutexAttributes=%v bInitialOwner=%v lpName=%v, (%v:)", t.OrTypedNil(), lpMutexAttributes, bInitialOwner, lpName, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 XDeleteFileA(ж<TLS> t, uintptr lpFileName) {
    if (__ccgo_strace) {
        trc("t=%v lpFileName=%v, (%v:)", t.OrTypedNil(), lpFileName, origin(2));
    }
    throw panic(todo(""));
}

public static partial uint32 XFormatMessageA(ж<TLS> t, uint32 dwFlagsAndAttributes, uintptr lpSource, uint32 dwMessageId, uint32 dwLanguageId, uintptr lpBuffer, uint32 nSize, uintptr Arguments) {
    if (__ccgo_strace) {
        trc("t=%v dwFlagsAndAttributes=%v lpSource=%v dwLanguageId=%v lpBuffer=%v nSize=%v Arguments=%v, (%v:)", t.OrTypedNil(), dwFlagsAndAttributes, lpSource, dwLanguageId, lpBuffer, nSize, Arguments, origin(2));
    }
    throw panic(todo(""));
}

public static partial uint32 XFormatMessageW(ж<TLS> t, uint32 dwFlags, uintptr lpSource, uint32 dwMessageId, uint32 dwLanguageId, uintptr lpBuffer, uint32 nSize, uintptr Arguments) {
    if (__ccgo_strace) {
        trc("t=%v dwFlags=%v lpSource=%v dwLanguageId=%v lpBuffer=%v nSize=%v Arguments=%v, (%v:)", t.OrTypedNil(), dwFlags, lpSource, dwLanguageId, lpBuffer, nSize, Arguments, origin(2));
    }
    var (r0, _, e1) = procFormatMessageW.Call((uintptr)dwFlags, lpSource, (uintptr)dwMessageId, (uintptr)dwLanguageId,
        lpBuffer, (uintptr)nSize, Arguments, 0, 0);
    var n = (uint32)r0;
    if (n == 0) {
        if (!AreEqual(e1, windows.NOERROR)) {
            t.setErrno(e1);
        } else {
            t.setErrno((nint)(errno.EINVAL));
        }
    }
    return n;
}

public static partial int32 XFreeLibrary(ж<TLS> t, uintptr hLibModule) {
    if (__ccgo_strace) {
        trc("t=%v hLibModule=%v, (%v:)", t.OrTypedNil(), hLibModule, origin(2));
    }
    throw panic(todo(""));
}

public static partial uint32 XGetCurrentProcessId(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    var (r0, _, _) = procGetCurrentProcessId.Call(0, 0, 0);
    var pid = (uint32)r0;
    return pid;
}

public static partial int32 XGetDiskFreeSpaceA(ж<TLS> t, uintptr lpRootPathName, uintptr lpSectorsPerCluster, uintptr lpBytesPerSector, uintptr lpNumberOfFreeClusters, uintptr lpTotalNumberOfClusters) {
    if (__ccgo_strace) {
        trc("t=%v lpTotalNumberOfClusters=%v, (%v:)", t.OrTypedNil(), lpTotalNumberOfClusters, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 XGetDiskFreeSpaceW(ж<TLS> t, uintptr lpRootPathName, uintptr lpSectorsPerCluster, uintptr lpBytesPerSector, uintptr lpNumberOfFreeClusters, uintptr lpTotalNumberOfClusters) {
    if (__ccgo_strace) {
        trc("t=%v lpTotalNumberOfClusters=%v, (%v:)", t.OrTypedNil(), lpTotalNumberOfClusters, origin(2));
    }
    throw panic(todo(""));
}

public static partial uint32 XGetFileAttributesA(ж<TLS> t, uintptr lpFileName) {
    if (__ccgo_strace) {
        trc("t=%v lpFileName=%v, (%v:)", t.OrTypedNil(), lpFileName, origin(2));
    }
    var (r0, _, err) = procGetFileAttributesA.Call(lpFileName, 0, 0);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (uint32)r0;
}

public static partial int32 XGetFileAttributesExW(ж<TLS> t, uintptr lpFileName, int32 fInfoLevelId, uintptr lpFileInformation) {
    if (__ccgo_strace) {
        trc("t=%v lpFileName=%v fInfoLevelId=%v lpFileInformation=%v, (%v:)", t.OrTypedNil(), lpFileName, fInfoLevelId, lpFileInformation, origin(2));
    }
    var (r1, _, e1) = procGetFileAttributesExW.Call(lpFileName, (uintptr)fInfoLevelId, lpFileInformation);
    if (r1 == 0) {
        if (!AreEqual(e1, windows.NOERROR)) {
            t.setErrno(e1);
        } else {
            t.setErrno((nint)(errno.EINVAL));
        }
        return 0;
    }
    return (int32)r1;
}

public static partial uint32 XGetFileSize(ж<TLS> t, uintptr hFile, uintptr lpFileSizeHigh) {
    if (__ccgo_strace) {
        trc("t=%v lpFileSizeHigh=%v, (%v:)", t.OrTypedNil(), lpFileSizeHigh, origin(2));
    }
    var (r1, _, e1) = procGetFileSize.Call(hFile, lpFileSizeHigh, 0);
    if (r1 == math.MaxUint32) {
        if (lpFileSizeHigh == 0) {
            t.setErrno(e1);
            return math.MaxUint32;
        } else {
            t.setErrno(e1);
            return math.MaxUint32;
        }
    }
    return (uint32)r1;
}

public static partial int32 XGetFileSizeEx(ж<TLS> t, uintptr hFile, uintptr lpFileSize) {
    if (__ccgo_strace) {
        trc("t=%v hFile=%v lpFileSize=%v, (%v:)", t.OrTypedNil(), hFile, lpFileSize, origin(2));
    }
    var (r0, _, e1) = procGetFileSizeEx.Call(hFile, lpFileSize);
    if (r0 == 0) {
        t.setErrno(e1);
    }
    return (int32)r0;
}

public static partial uint32 XGetDriveTypeW(ж<TLS> t, uintptr lpRootPathName) {
    if (__ccgo_strace) {
        trc("t=%v lpRootPathName=%v, (%v:)", t.OrTypedNil(), lpRootPathName, origin(2));
    }
    var (r0, _, _) = procGetDriveTypeW.Call(lpRootPathName);
    return (uint32)r0;
}

public static partial uint32 XGetFullPathNameA(ж<TLS> t, uintptr lpFileName, uint32 nBufferLength, uintptr lpBuffer, uintptr lpFilePart) {
    if (__ccgo_strace) {
        trc("t=%v lpFileName=%v nBufferLength=%v lpFilePart=%v, (%v:)", t.OrTypedNil(), lpFileName, nBufferLength, lpFilePart, origin(2));
    }
    throw panic(todo(""));
}

public static partial uintptr XGetProcAddress(ж<TLS> t, uintptr hModule, uintptr lpProcName) {
    if (__ccgo_strace) {
        trc("t=%v lpProcName=%v, (%v:)", t.OrTypedNil(), lpProcName, origin(2));
    }

    return 0;
}

public static partial uintptr XRtlGetVersion(ж<TLS> t, uintptr lpVersionInformation) {
    if (__ccgo_strace) {
        trc("t=%v lpVersionInformation=%v, (%v:)", t.OrTypedNil(), lpVersionInformation, origin(2));
    }
    throw panic(todo(""));
}

public static partial void XGetNativeSystemInfo(ж<TLS> t, uintptr lpSystemInfo) {
    if (__ccgo_strace) {
        trc("t=%v lpSystemInfo=%v, (%v:)", t.OrTypedNil(), lpSystemInfo, origin(2));
    }
    procGetNativeSystemInfo.Call(lpSystemInfo, 0, 0);
}

public static partial void XGetSystemInfo(ж<TLS> t, uintptr lpSystemInfo) {
    if (__ccgo_strace) {
        trc("t=%v lpSystemInfo=%v, (%v:)", t.OrTypedNil(), lpSystemInfo, origin(2));
    }
    procGetSystemInfo.Call(lpSystemInfo, 0, 0);
}

public static partial void XGetSystemTime(ж<TLS> t, uintptr lpSystemTime) {
    if (__ccgo_strace) {
        trc("t=%v lpSystemTime=%v, (%v:)", t.OrTypedNil(), lpSystemTime, origin(2));
    }
    procGetSystemTime.Call(lpSystemTime, 0, 0);
}

public static partial void XGetSystemTimeAsFileTime(ж<TLS> t, uintptr lpSystemTimeAsFileTime) {
    if (__ccgo_strace) {
        trc("t=%v lpSystemTimeAsFileTime=%v, (%v:)", t.OrTypedNil(), lpSystemTimeAsFileTime, origin(2));
    }
    procGetSystemTimeAsFileTime.Call(lpSystemTimeAsFileTime, 0, 0);
}

public static partial uint32 XGetTempPathA(ж<TLS> t, uint32 nBufferLength, uintptr lpBuffer) {
    if (__ccgo_strace) {
        trc("t=%v nBufferLength=%v lpBuffer=%v, (%v:)", t.OrTypedNil(), nBufferLength, lpBuffer, origin(2));
    }
    throw panic(todo(""));
}

public static partial uint32 XGetTempPathW(ж<TLS> t, uint32 nBufferLength, uintptr lpBuffer) {
    if (__ccgo_strace) {
        trc("t=%v nBufferLength=%v lpBuffer=%v, (%v:)", t.OrTypedNil(), nBufferLength, lpBuffer, origin(2));
    }
    var (rv, err) = windows.GetTempPath(nBufferLength, @unsafe.Pointer.To<uint16>((@unsafe.Pointer)lpBuffer));
    if (err != nil) {
        t.setErrno(err);
    }
    return rv;
}

public static partial uint32 XGetTickCount(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    var (r0, _, _) = procGetTickCount.Call(0, 0, 0);
    return (uint32)r0;
}

public static partial int32 XGetVersionExA(ж<TLS> t, uintptr lpVersionInformation) {
    if (__ccgo_strace) {
        trc("t=%v lpVersionInformation=%v, (%v:)", t.OrTypedNil(), lpVersionInformation, origin(2));
    }
    var (r0, _, err) = procGetVersionExA.Call(lpVersionInformation, 0, 0);
    if (r0 == 0) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial uintptr XHeapCreate(ж<TLS> t, uint32 flOptions, typesꓸSize_t dwInitialSize, typesꓸSize_t dwMaximumSize) {
    if (__ccgo_strace) {
        trc("t=%v flOptions=%v dwMaximumSize=%v, (%v:)", t.OrTypedNil(), flOptions, dwMaximumSize, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 XHeapDestroy(ж<TLS> t, uintptr hHeap) {
    if (__ccgo_strace) {
        trc("t=%v hHeap=%v, (%v:)", t.OrTypedNil(), hHeap, origin(2));
    }
    throw panic(todo(""));
}

public static partial uintptr XHeapReAlloc(ж<TLS> t, uintptr hHeap, uint32 dwFlags, uintptr lpMem, typesꓸSize_t dwBytes) {
    if (__ccgo_strace) {
        trc("t=%v hHeap=%v dwFlags=%v lpMem=%v dwBytes=%v, (%v:)", t.OrTypedNil(), hHeap, dwFlags, lpMem, dwBytes, origin(2));
    }
    throw panic(todo(""));
}

public static partial typesꓸSize_t XHeapSize(ж<TLS> t, uintptr hHeap, uint32 dwFlags, uintptr lpMem) {
    if (__ccgo_strace) {
        trc("t=%v hHeap=%v dwFlags=%v lpMem=%v, (%v:)", t.OrTypedNil(), hHeap, dwFlags, lpMem, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 XHeapValidate(ж<TLS> t, uintptr hHeap, uint32 dwFlags, uintptr lpMem) {
    if (__ccgo_strace) {
        trc("t=%v hHeap=%v dwFlags=%v lpMem=%v, (%v:)", t.OrTypedNil(), hHeap, dwFlags, lpMem, origin(2));
    }
    throw panic(todo(""));
}

public static partial typesꓸSize_t XHeapCompact(ж<TLS> t, uintptr hHeap, uint32 dwFlags) {
    if (__ccgo_strace) {
        trc("t=%v hHeap=%v dwFlags=%v, (%v:)", t.OrTypedNil(), hHeap, dwFlags, origin(2));
    }
    throw panic(todo(""));
}

public static partial uintptr XLoadLibraryA(ж<TLS> t, uintptr lpLibFileName) {
    if (__ccgo_strace) {
        trc("t=%v lpLibFileName=%v, (%v:)", t.OrTypedNil(), lpLibFileName, origin(2));
    }
    throw panic(todo(""));
}

public static partial uintptr XLoadLibraryW(ж<TLS> t, uintptr lpLibFileName) {
    if (__ccgo_strace) {
        trc("t=%v lpLibFileName=%v, (%v:)", t.OrTypedNil(), lpLibFileName, origin(2));
    }
    throw panic(todo(""));
}

public static partial uintptr XLocalFree(ж<TLS> t, uintptr hMem) {
    if (__ccgo_strace) {
        trc("t=%v hMem=%v, (%v:)", t.OrTypedNil(), hMem, origin(2));
    }
    var (h, err) = windows.LocalFree((windows.Handle)hMem);
    if (h != 0) {
        if (err != nil) {
            t.setErrno(err);
        } else {
            t.setErrno((nint)(errno.EINVAL));
        }
        return (uintptr)h;
    }
    return 0;
}

public static partial int32 XLockFile(ж<TLS> t, uintptr hFile, uint32 dwFileOffsetLow, uint32 dwFileOffsetHigh, uint32 nNumberOfBytesToLockLow, uint32 nNumberOfBytesToLockHigh) {
    if (__ccgo_strace) {
        trc("t=%v hFile=%v nNumberOfBytesToLockHigh=%v, (%v:)", t.OrTypedNil(), hFile, nNumberOfBytesToLockHigh, origin(2));
    }

    var (r1, _, e1) = procLockFile.Call(hFile, (uintptr)dwFileOffsetLow, (uintptr)dwFileOffsetHigh, (uintptr)nNumberOfBytesToLockLow, (uintptr)nNumberOfBytesToLockHigh, 0);
    if (r1 == 0) {
        if (!AreEqual(e1, windows.NOERROR)) {
            t.setErrno(e1);
        } else {
            t.setErrno((nint)(errno.EINVAL));
        }
        return 0;
    }
    return (int32)r1;
}

public static partial int32 XLockFileEx(ж<TLS> t, uintptr hFile, uint32 dwFlags, uint32 dwReserved, uint32 nNumberOfBytesToLockLow, uint32 nNumberOfBytesToLockHigh, uintptr lpOverlapped) {
    if (__ccgo_strace) {
        trc("t=%v hFile=%v nNumberOfBytesToLockHigh=%v lpOverlapped=%v, (%v:)", t.OrTypedNil(), hFile, nNumberOfBytesToLockHigh, lpOverlapped, origin(2));
    }
    var (r1, _, e1) = procLockFileEx.Call(hFile, (uintptr)dwFlags, (uintptr)dwReserved, (uintptr)nNumberOfBytesToLockLow, (uintptr)nNumberOfBytesToLockHigh, lpOverlapped);
    if (r1 == 0) {
        if (!AreEqual(e1, windows.NOERROR)) {
            t.setErrno(e1);
        } else {
            t.setErrno((nint)(errno.EINVAL));
        }
        return 0;
    }
    return (int32)r1;
}

public static partial uintptr XMapViewOfFile(ж<TLS> t, uintptr hFileMappingObject, uint32 dwDesiredAccess, uint32 dwFileOffsetHigh, uint32 dwFileOffsetLow, typesꓸSize_t dwNumberOfBytesToMap) {
    if (__ccgo_strace) {
        trc("t=%v hFileMappingObject=%v dwFileOffsetLow=%v dwNumberOfBytesToMap=%v, (%v:)", t.OrTypedNil(), hFileMappingObject, dwFileOffsetLow, dwNumberOfBytesToMap, origin(2));
    }
    var (h, _, e1) = procMapViewOfFile.Call(hFileMappingObject, (uintptr)dwDesiredAccess,
        (uintptr)dwFileOffsetHigh, (uintptr)dwFileOffsetLow, (uintptr)dwNumberOfBytesToMap, 0);
    if (h == 0) {
        if (!AreEqual(e1, windows.NOERROR)) {
            t.setErrno(e1);
        } else {
            t.setErrno((nint)(errno.EINVAL));
        }
    }
    return h;
}

public static partial int32 XQueryPerformanceCounter(ж<TLS> t, uintptr lpPerformanceCount) {
    if (__ccgo_strace) {
        trc("t=%v lpPerformanceCount=%v, (%v:)", t.OrTypedNil(), lpPerformanceCount, origin(2));
    }
    var (r0, _, _) = procQueryPerformanceCounter.Call(lpPerformanceCount, 0, 0);
    return (int32)r0;
}

public static partial void XSleep(ж<TLS> t, uint32 dwMilliseconds) {
    if (__ccgo_strace) {
        trc("t=%v dwMilliseconds=%v, (%v:)", t.OrTypedNil(), dwMilliseconds, origin(2));
    }
    gotime.Sleep(((gotime.Duration)(int64)dwMilliseconds) * gotime.Millisecond);
}

public static partial int32 XSystemTimeToFileTime(ж<TLS> t, uintptr lpSystemTime, uintptr lpFileTime) {
    if (__ccgo_strace) {
        trc("t=%v lpFileTime=%v, (%v:)", t.OrTypedNil(), lpFileTime, origin(2));
    }
    var (r0, _, _) = procSystemTimeToFileTime.Call(lpSystemTime, lpFileTime, 0);
    return (int32)r0;
}

public static partial int32 XUnlockFile(ж<TLS> t, uintptr hFile, uint32 dwFileOffsetLow, uint32 dwFileOffsetHigh, uint32 nNumberOfBytesToUnlockLow, uint32 nNumberOfBytesToUnlockHigh) {
    if (__ccgo_strace) {
        trc("t=%v hFile=%v nNumberOfBytesToUnlockHigh=%v, (%v:)", t.OrTypedNil(), hFile, nNumberOfBytesToUnlockHigh, origin(2));
    }
    var (r1, _, e1) = procUnlockFile.Call(hFile, (uintptr)dwFileOffsetLow, (uintptr)dwFileOffsetHigh, (uintptr)nNumberOfBytesToUnlockLow, (uintptr)nNumberOfBytesToUnlockHigh, 0);
    if (r1 == 0) {
        if (!AreEqual(e1, windows.NOERROR)) {
            t.setErrno(e1);
        } else {
            t.setErrno((nint)(errno.EINVAL));
        }
        return 0;
    }
    return (int32)r1;
}

public static partial int32 XUnlockFileEx(ж<TLS> t, uintptr hFile, uint32 dwReserved, uint32 nNumberOfBytesToUnlockLow, uint32 nNumberOfBytesToUnlockHigh, uintptr lpOverlapped) {
    if (__ccgo_strace) {
        trc("t=%v hFile=%v nNumberOfBytesToUnlockHigh=%v lpOverlapped=%v, (%v:)", t.OrTypedNil(), hFile, nNumberOfBytesToUnlockHigh, lpOverlapped, origin(2));
    }
    var (r1, _, e1) = procUnlockFileEx.Call(hFile, (uintptr)dwReserved, (uintptr)nNumberOfBytesToUnlockLow, (uintptr)nNumberOfBytesToUnlockHigh, lpOverlapped, 0);
    if (r1 == 0) {
        if (!AreEqual(e1, windows.NOERROR)) {
            t.setErrno(e1);
        } else {
            t.setErrno((nint)(errno.EINVAL));
        }
        return 0;
    }
    return (int32)r1;
}

public static partial int32 XUnmapViewOfFile(ж<TLS> t, uintptr lpBaseAddress) {
    if (__ccgo_strace) {
        trc("t=%v lpBaseAddress=%v, (%v:)", t.OrTypedNil(), lpBaseAddress, origin(2));
    }
    var err = windows.UnmapViewOfFile(lpBaseAddress);
    if (err != nil) {
        t.setErrno(err);
        return 0;
    }
    return 1;
}

public static partial int32 XWideCharToMultiByte(ж<TLS> t, uint32 CodePage, uint32 dwFlags, uintptr lpWideCharStr, int32 cchWideChar, uintptr lpMultiByteStr, int32 cbMultiByte, uintptr lpDefaultChar, uintptr lpUsedDefaultChar) {
    if (__ccgo_strace) {
        trc("t=%v CodePage=%v dwFlags=%v lpWideCharStr=%v cchWideChar=%v lpMultiByteStr=%v cbMultiByte=%v lpUsedDefaultChar=%v, (%v:)", t.OrTypedNil(), CodePage, dwFlags, lpWideCharStr, cchWideChar, lpMultiByteStr, cbMultiByte, lpUsedDefaultChar, origin(2));
    }
    var (r1, _, _) = procWideCharToMultiByte.Call((uintptr)CodePage, (uintptr)dwFlags, lpWideCharStr,
        (uintptr)cchWideChar, lpMultiByteStr, (uintptr)cbMultiByte,
        lpDefaultChar, lpUsedDefaultChar, 0);
    return ((int32)r1);
}

public static partial void XOutputDebugStringA(ж<TLS> t, uintptr lpOutputString) {
    if (__ccgo_strace) {
        trc("t=%v lpOutputString=%v, (%v:)", t.OrTypedNil(), lpOutputString, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 XFlushViewOfFile(ж<TLS> t, uintptr lpBaseAddress, typesꓸSize_t dwNumberOfBytesToFlush) {
    if (__ccgo_strace) {
        trc("t=%v lpBaseAddress=%v dwNumberOfBytesToFlush=%v, (%v:)", t.OrTypedNil(), lpBaseAddress, dwNumberOfBytesToFlush, origin(2));
    }
    var err = windows.FlushViewOfFile(lpBaseAddress, (uintptr)dwNumberOfBytesToFlush);
    if (err != nil) {
        t.setErrno(err);
        return 0;
    }
    return 1;
}

[global::go.golib.GoInlineLayout(Size = 56, Align = 8), StructLayout(LayoutKind.Explicit, Size = 56)] partial struct _stat64ᴛ1 /*dyn*/ {
    [FieldOffset(0)] public _dev_t Fst_dev;
    [FieldOffset(4)] public _ino_t Fst_ino;
    [FieldOffset(6)] public uint16 Fst_mode;
    [FieldOffset(8)] public int16 Fst_nlink;
    [FieldOffset(10)] public int16 Fst_uid;
    [FieldOffset(12)] public int16 Fst_gid;
    [FieldOffset(14)] internal array2<byte> _;
    [FieldOffset(16)] public _dev_t Fst_rdev;
    [FieldOffset(20)] internal array4<byte> __;
    [FieldOffset(24)] public int64 Fst_size;
    [FieldOffset(32)] public int64 Fst_atime;
    [FieldOffset(40)] public int64 Fst_mtime;
    [FieldOffset(48)] public int64 Fst_ctime;
}

public static readonly ж<int64> ᏑWindows_Tick = new StaticBox<int64>(10000000);
public static ref int64 Windows_Tick => ref ((StaticBox<int64>)ᏑWindows_Tick).ValueSlot;
public static readonly ж<int64> ᏑSecToUnixEpoch = new StaticBox<int64>(11644473600L);
public static ref int64 SecToUnixEpoch => ref ((StaticBox<int64>)ᏑSecToUnixEpoch).ValueSlot;

public static int64 WindowsTickToUnixSeconds(int64 windowsTicks) {
    return (quo(windowsTicks, Windows_Tick) - SecToUnixEpoch);
}

public static partial int32 X_stat64(ж<TLS> t, uintptr path, uintptr buffer) {
    if (__ccgo_strace) {
        trc("t=%v buffer=%v, (%v:)", t.OrTypedNil(), buffer, origin(2));
    }

    ref var fa = ref heap(new windows.Win32FileAttributeData(), out var Ꮡfa);
    var (r1, _, e1) = procGetFileAttributesExA.Call(path, windows.GetFileExInfoStandard, (uintptr)Ꮡfa);
    if (r1 == 0) {
        if (!AreEqual(e1, windows.NOERROR)) {
            t.setErrno(e1);
        } else {
            t.setErrno((nint)(errno.EINVAL));
        }
        return -1;
    }

    ж<_stat64> bStat64 = @unsafe.Pointer.To<_stat64>((@unsafe.Pointer)buffer);
    int64 accessTime = ((int64)fa.LastAccessTime.HighDateTime << 32) + (int64)fa.LastAccessTime.LowDateTime;
    bStat64.Fst_atime = WindowsTickToUnixSeconds(accessTime);
    int64 modTime = ((int64)fa.LastWriteTime.HighDateTime << 32) + (int64)fa.LastWriteTime.LowDateTime;
    bStat64.Fst_mtime = WindowsTickToUnixSeconds(modTime);
    int64 crTime = ((int64)fa.CreationTime.HighDateTime << 32) + (int64)fa.CreationTime.LowDateTime;
    bStat64.Fst_ctime = WindowsTickToUnixSeconds(crTime);
    int64 fSz = ((int64)fa.FileSizeHigh << 32) + (int64)fa.FileSizeLow;
    bStat64.Fst_size = fSz;
    bStat64.Fst_mode = WindowsAttrbiutesToStat(fa.FileAttributes);

    return 0;
}

public static uint16 WindowsAttrbiutesToStat(uint32 fa) {
    uint32 src_mode = (fa & 0xff);
    uint16 st_mode = default;
    if ((src_mode & (uint32)windows.FILE_ATTRIBUTE_DIRECTORY) != 0) {
        st_mode = windows.S_IFDIR;
    } else {
        st_mode = windows.S_IFREG;
    }

    if ((src_mode & (uint32)windows.FILE_ATTRIBUTE_READONLY) != 0) {
        st_mode = (uint16)(st_mode | (uint16)windows.S_IRUSR);
    } else {
        st_mode = (uint16)((uint16)(st_mode | (uint16)windows.S_IRUSR) | (uint16)windows.S_IWUSR);
    }
    st_mode = (uint16)(st_mode | ((uint16)(st_mode & 0x700)) >> 3);
    st_mode = (uint16)(st_mode | ((uint16)(st_mode & 0x700)) >> 6);
    return st_mode;
}

public static partial int32 X_chsize(ж<TLS> t, int32 fd, @long size) {
    if (__ccgo_strace) {
        trc("t=%v fd=%v size=%v, (%v:)", t.OrTypedNil(), fd, size, origin(2));
    }

    var (f, ok) = fdToFile(fd);
    if (!ok) {
        t.setErrno(EBADF);
        return -1;
    }

    var err = windows.Ftruncate(f.Handle, (int64)size);
    if (err != nil) {
        t.setErrno(err);
        return -1;
    }

    return 0;
}

public static partial int32 X_snprintf(ж<TLS> t, uintptr str, typesꓸSize_t size, uintptr format, uintptr args) {
    if (__ccgo_strace) {
        trc("t=%v str=%v size=%v args=%v, (%v:)", t.OrTypedNil(), str, size, args, origin(2));
    }
    return Xsnprintf(t, str, size, format, args);
}

internal static UntypedInt wErr_ERROR_INSUFFICIENT_BUFFER => 122;

internal static int32 win32FindDataToFileInfo(ж<TLS> t, ж<statꓸX_finddata64i32_t> Ꮡfdata, ж<windows.Win32finddata> Ꮡwfd) {
    ref var fdata = ref Ꮡfdata.DerefOrNull();
    ref var wfd = ref Ꮡwfd.DerefOrNull();

    int64 accessTime = ((int64)wfd.LastAccessTime.HighDateTime << 32) + (int64)wfd.LastAccessTime.LowDateTime;
    fdata.Ftime_access = WindowsTickToUnixSeconds(accessTime);
    int64 modTime = ((int64)wfd.LastWriteTime.HighDateTime << 32) + (int64)wfd.LastWriteTime.LowDateTime;
    fdata.Ftime_write = WindowsTickToUnixSeconds(modTime);
    int64 crTime = ((int64)wfd.CreationTime.HighDateTime << 32) + (int64)wfd.CreationTime.LowDateTime;
    fdata.Ftime_create = WindowsTickToUnixSeconds(crTime);
    fdata.Fsize = wfd.FileSizeLow;
    fdata.Fattrib = wfd.FileAttributes;

    uint32 cp = XGetConsoleCP(t);
    uintptr wcFn = (uintptr)Ꮡwfd.at(windows.Win32finddata.ᏑFileName, 0);
    uintptr mbcsFn = (uintptr)Ꮡfdata.at(statꓸX_finddata64i32_t.ᏑFname, 0);
    var rv = XWideCharToMultiByte(t, cp, 0, wcFn, -1, mbcsFn, 260, 0, 0);
    if (rv == wErr_ERROR_INSUFFICIENT_BUFFER) {
        t.setErrno((nint)(errno.ENOMEM));
        return -1;
    }
    return 0;
}

public static partial typesꓸIntptr_t X_findfirst64i32(ж<TLS> t, uintptr filespec, uintptr fileinfo) {
    if (__ccgo_strace) {
        trc("t=%v fileinfo=%v, (%v:)", t.OrTypedNil(), fileinfo, origin(2));
    }

    @string gsFileSpec = GoString(filespec);
    var (namep, err) = windows.UTF16PtrFromString(gsFileSpec);
    if (err != nil) {
        t.setErrno(err);
        return (typesꓸIntptr_t)(-1);
    }

    ж<statꓸX_finddata64i32_t> fdata = @unsafe.Pointer.To<statꓸX_finddata64i32_t>((@unsafe.Pointer)fileinfo);
    ref var wfd = ref heap(new windows.Win32finddata(), out var Ꮡwfd);
    (var h, err) = windows.FindFirstFile(namep, Ꮡwfd);
    if (err != nil) {
        t.setErrno(err);
        return (typesꓸIntptr_t)(-1);
    }
    var rv = win32FindDataToFileInfo(t, fdata, Ꮡwfd);
    if (rv != 0) {
        if (h != 0) {
            windows.FindClose(h);
        }
        return (typesꓸIntptr_t)(-1);
    }
    return (typesꓸIntptr_t)h;
}

public static partial int32 X_findnext64i32(ж<TLS> t, typesꓸIntptr_t handle, uintptr fileinfo) {
    if (__ccgo_strace) {
        trc("t=%v handle=%v fileinfo=%v, (%v:)", t.OrTypedNil(), handle, fileinfo, origin(2));
    }

    ж<statꓸX_finddata64i32_t> fdata = @unsafe.Pointer.To<statꓸX_finddata64i32_t>((@unsafe.Pointer)fileinfo);
    ref var wfd = ref heap(new windows.Win32finddata(), out var Ꮡwfd);

    var err = windows.FindNextFile((windows.Handle)(uintptr)handle, Ꮡwfd);
    if (err != nil) {
        t.setErrno(err);
        return -1;
    }

    var rv = win32FindDataToFileInfo(t, fdata, Ꮡwfd);
    if (rv != 0) {
        return -1;
    }
    return 0;
}

public static partial int32 X_findclose(ж<TLS> t, typesꓸIntptr_t handle) {
    if (__ccgo_strace) {
        trc("t=%v handle=%v, (%v:)", t.OrTypedNil(), handle, origin(2));
    }

    var err = windows.FindClose((windows.Handle)(uintptr)handle);
    if (err != nil) {
        t.setErrno(err);
        return -1;
    }
    return 0;
}

public static partial uint32 XGetEnvironmentVariableA(ж<TLS> t, uintptr lpName, uintptr lpBuffer, uint32 nSize) {
    if (__ccgo_strace) {
        trc("t=%v lpBuffer=%v nSize=%v, (%v:)", t.OrTypedNil(), lpBuffer, nSize, origin(2));
    }
    var (r0, _, e1) = procGetEnvironmentVariableA.Call(lpName, lpBuffer, (uintptr)nSize);
    var n = (uint32)r0;
    if (n == 0) {
        if (!AreEqual(e1, windows.NOERROR)) {
            t.setErrno(e1);
        } else {
            t.setErrno((nint)(errno.EINVAL));
        }
    }
    return n;
}

public static partial int32 X_fstat64(ж<TLS> t, int32 fd, uintptr buffer) {
    if (__ccgo_strace) {
        trc("t=%v fd=%v buffer=%v, (%v:)", t.OrTypedNil(), fd, buffer, origin(2));
    }

    var (f, ok) = fdToFile(fd);
    if (!ok) {
        t.setErrno(EBADF);
        return -1;
    }

    ref var d = ref heap(new windows.ByHandleFileInformation(), out var Ꮡd);
    var err = windows.GetFileInformationByHandle(f.Handle, Ꮡd);
    if (err != nil) {
        t.setErrno(EBADF);
        return -1;
    }

    ж<_stat64> bStat64 = @unsafe.Pointer.To<_stat64>((@unsafe.Pointer)buffer);
    int64 accessTime = ((int64)d.LastAccessTime.HighDateTime << 32) + (int64)d.LastAccessTime.LowDateTime;
    bStat64.Fst_atime = WindowsTickToUnixSeconds(accessTime);
    int64 modTime = ((int64)d.LastWriteTime.HighDateTime << 32) + (int64)d.LastWriteTime.LowDateTime;
    bStat64.Fst_mtime = WindowsTickToUnixSeconds(modTime);
    int64 crTime = ((int64)d.CreationTime.HighDateTime << 32) + (int64)d.CreationTime.LowDateTime;
    bStat64.Fst_ctime = WindowsTickToUnixSeconds(crTime);
    int64 fSz = ((int64)d.FileSizeHigh << 32) + (int64)d.FileSizeLow;
    bStat64.Fst_size = fSz;
    bStat64.Fst_mode = WindowsAttrbiutesToStat(d.FileAttributes);

    return 0;
}

public static partial uintptr XCreateEventA(ж<TLS> t, uintptr lpEventAttributes, int32 bManualReset, int32 bInitialState, uintptr lpName) {
    if (__ccgo_strace) {
        trc("t=%v lpEventAttributes=%v bInitialState=%v lpName=%v, (%v:)", t.OrTypedNil(), lpEventAttributes, bInitialState, lpName, origin(2));
    }
    var (r0, _, err) = procCreateEventA.Call(lpEventAttributes, (uintptr)bManualReset,
        (uintptr)bInitialState, lpName, 0, 0);
    if (r0 == 0) {
        t.setErrno(err);
    }
    return r0;
}

public static partial int32 XCancelSynchronousIo(ж<TLS> t, uintptr hThread) {
    if (__ccgo_strace) {
        trc("t=%v hThread=%v, (%v:)", t.OrTypedNil(), hThread, origin(2));
    }
    throw panic(todo(""));
}

public static void X_endthreadex(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
}

public static partial int32 X_beginthread(ж<TLS> t, uintptr procAddr, uint32 stack_sz, uintptr args) {
    if (__ccgo_strace) {
        trc("t=%v procAddr=%v stack_sz=%v args=%v, (%v:)", t.OrTypedNil(), procAddr, stack_sz, args, origin(2));
    }
    var f = GoFuncCookie.Func<Func<ж<TLS>, uintptr, uint32>>(procAddr);
    ref var tAdp = ref heap(new ThreadAdapter(), out var ᏑtAdp);

    tAdp = new ThreadAdapter(threadFunc: f, tls: NewTLS(), param: args);
    tAdp.token = addObject(ᏑtAdp);

    var (r0, _, err) = procCreateThread.Call(0, (uintptr)stack_sz,
        threadCallback, tAdp.token, 0, 0);
    if (r0 == 0) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static int32 X_beginthreadex(ж<TLS> t, uintptr _Δp1, uint32 stack_sz, uintptr procAddr, uintptr args, uint32 initf, uintptr thAddr) {
    var f = GoFuncCookie.Func<Func<ж<TLS>, uintptr, uint32>>(procAddr);
    ref var tAdp = ref heap(new ThreadAdapter(), out var ᏑtAdp);

    tAdp = new ThreadAdapter(threadFunc: f, tls: NewTLS(), param: args);
    tAdp.token = addObject(ᏑtAdp);

    var (r0, _, err) = procCreateThread.Call(0, (uintptr)stack_sz,
        threadCallback, tAdp.token, (uintptr)initf, thAddr);
    if (r0 == 0) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial uint32 XGetCurrentThreadId(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    var (r0, _, _) = procGetCurrentThreadId.Call(0, 0, 0);
    return (uint32)r0;
}

public static partial int32 XGetExitCodeThread(ж<TLS> t, uintptr hThread, uintptr lpExitCode) {
    if (__ccgo_strace) {
        trc("t=%v lpExitCode=%v, (%v:)", t.OrTypedNil(), lpExitCode, origin(2));
    }
    var (r0, _, _) = procGetExitCodeThread.Call(hThread, lpExitCode, 0);
    return (int32)r0;
}

public static partial uint32 XWaitForSingleObjectEx(ж<TLS> t, uintptr hHandle, uint32 dwMilliseconds, int32 bAlertable) {
    if (__ccgo_strace) {
        trc("t=%v hHandle=%v dwMilliseconds=%v bAlertable=%v, (%v:)", t.OrTypedNil(), hHandle, dwMilliseconds, bAlertable, origin(2));
    }
    var (rv, _, _) = procWaitForSingleObjectEx.Call(hHandle, (uintptr)dwMilliseconds, (uintptr)bAlertable);
    return (uint32)rv;
}

public static partial uint32 XMsgWaitForMultipleObjectsEx(ж<TLS> t, uint32 nCount, uintptr pHandles, uint32 dwMilliseconds, uint32 dwWakeMask, uint32 dwFlags) {
    if (__ccgo_strace) {
        trc("t=%v nCount=%v pHandles=%v dwFlags=%v, (%v:)", t.OrTypedNil(), nCount, pHandles, dwFlags, origin(2));
    }
    var (r0, _, err) = procMsgWaitForMultipleObjectsEx.Call((uintptr)nCount,
        pHandles,
        (uintptr)dwMilliseconds,
        (uintptr)dwWakeMask,
        (uintptr)dwFlags,
        0);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (uint32)r0;
}

public static partial int32 XMessageBoxW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial uint32 XGetModuleFileNameW(ж<TLS> t, uintptr hModule, uintptr lpFileName, uint32 nSize) {
    if (__ccgo_strace) {
        trc("t=%v lpFileName=%v nSize=%v, (%v:)", t.OrTypedNil(), lpFileName, nSize, origin(2));
    }
    var (r0, _, err) = procGetModuleFileNameW.Call(hModule, lpFileName, (uintptr)nSize);
    if (r0 == 0) {
        t.setErrno(err);
    }
    return (uint32)r0;
}

public static partial int32 XNetGetDCName(ж<TLS> t, uintptr ServerName, uintptr DomainName, uintptr Buffer) {
    if (__ccgo_strace) {
        trc("t=%v Buffer=%v, (%v:)", t.OrTypedNil(), Buffer, origin(2));
    }
    var (r0, _, err) = procNetGetDCName.Call(ServerName, DomainName, Buffer);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial uint32 XNetUserGetInfo(ж<TLS> t, uintptr servername, uintptr username, uint32 level, uintptr bufptr) {
    if (__ccgo_strace) {
        trc("t=%v username=%v level=%v bufptr=%v, (%v:)", t.OrTypedNil(), username, level, bufptr, origin(2));
    }
    var (r0, _, err) = procNetUserGetInfo.Call(servername,
        username,
        (uintptr)level,
        bufptr,
        0,
        0);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (uint32)r0;
}

public static partial int32 XlstrlenW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XGetProfilesDirectoryW(ж<TLS> t, uintptr lpProfileDir, uintptr lpcchSize) {
    if (__ccgo_strace) {
        trc("t=%v lpcchSize=%v, (%v:)", t.OrTypedNil(), lpcchSize, origin(2));
    }
    var (r0, _, err) = procGetProfilesDirectoryW.Call(lpProfileDir, lpcchSize, 0);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial int32 XNetApiBufferFree(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial uint32 XGetPrivateProfileStringA(ж<TLS> t, uintptr lpAppName, uintptr lpKeyName, uintptr lpDefault, uintptr lpReturnedString, uint32 nSize, uintptr lpFileName) {
    if (__ccgo_strace) {
        trc("t=%v lpReturnedString=%v nSize=%v lpFileName=%v, (%v:)", t.OrTypedNil(), lpReturnedString, nSize, lpFileName, origin(2));
    }
    var (r0, _, err) = procGetPrivateProfileStringA.Call(lpAppName,
        lpKeyName,
        lpDefault,
        lpReturnedString,
        (uintptr)nSize,
        lpFileName);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno((nint)0x02);
    }
    return (uint32)r0;
}

public static partial int32 XGetWindowsDirectoryA(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XGetFileSecurityW(ж<TLS> t, uintptr lpFileName, uint32 RequestedInformation, uintptr pSecurityDescriptor, uint32 nLength, uintptr lpnLengthNeeded) {
    if (__ccgo_strace) {
        trc("t=%v lpFileName=%v RequestedInformation=%v pSecurityDescriptor=%v nLength=%v lpnLengthNeeded=%v, (%v:)", t.OrTypedNil(), lpFileName, RequestedInformation, pSecurityDescriptor, nLength, lpnLengthNeeded, origin(2));
    }
    var (r0, _, err) = procGetFileSecurityW.Call(lpFileName, (uintptr)RequestedInformation, pSecurityDescriptor, (uintptr)nLength, lpnLengthNeeded, 0);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial int32 XGetSecurityDescriptorOwner(ж<TLS> t, uintptr pSecurityDescriptor, uintptr pOwner, uintptr lpbOwnerDefaulted) {
    if (__ccgo_strace) {
        trc("t=%v lpbOwnerDefaulted=%v, (%v:)", t.OrTypedNil(), lpbOwnerDefaulted, origin(2));
    }
    var (r0, _, err) = procGetSecurityDescriptorOwner.Call(pSecurityDescriptor, pOwner, lpbOwnerDefaulted);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial uintptr XGetSidIdentifierAuthority(ж<TLS> t, uintptr pSid) {
    if (__ccgo_strace) {
        trc("t=%v pSid=%v, (%v:)", t.OrTypedNil(), pSid, origin(2));
    }
    var (r0, _, err) = procGetSidIdentifierAuthority.Call(pSid, 0, 0);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return r0;
}

public static partial int32 XImpersonateSelf(ж<TLS> t, int32 ImpersonationLevel) {
    if (__ccgo_strace) {
        trc("t=%v ImpersonationLevel=%v, (%v:)", t.OrTypedNil(), ImpersonationLevel, origin(2));
    }
    var (r0, _, err) = procImpersonateSelf.Call((uintptr)ImpersonationLevel, 0, 0);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial int32 XOpenThreadToken(ж<TLS> t, uintptr ThreadHandle, uint32 DesiredAccess, int32 OpenAsSelf, uintptr TokenHandle) {
    if (__ccgo_strace) {
        trc("t=%v ThreadHandle=%v DesiredAccess=%v OpenAsSelf=%v TokenHandle=%v, (%v:)", t.OrTypedNil(), ThreadHandle, DesiredAccess, OpenAsSelf, TokenHandle, origin(2));
    }
    var (r0, _, err) = procOpenThreadToken.Call(ThreadHandle, (uintptr)DesiredAccess, (uintptr)OpenAsSelf, TokenHandle, 0, 0);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial uintptr XGetCurrentThread(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    var (r0, _, err) = procGetCurrentThread.Call(0, 0, 0);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return r0;
}

public static partial int32 XRevertToSelf(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    var (r0, _, err) = procRevertToSelf.Call(0, 0, 0);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial int32 XAccessCheck(ж<TLS> t, uintptr pSecurityDescriptor, uintptr ClientToken, uint32 DesiredAccess, uintptr GenericMapping, uintptr PrivilegeSet, uintptr PrivilegeSetLength, uintptr GrantedAccess, uintptr AccessStatus) {
    if (__ccgo_strace) {
        trc("t=%v ClientToken=%v DesiredAccess=%v AccessStatus=%v, (%v:)", t.OrTypedNil(), ClientToken, DesiredAccess, AccessStatus, origin(2));
    }
    var (r0, _, err) = procAccessCheck.Call(pSecurityDescriptor,
        ClientToken,
        (uintptr)DesiredAccess,
        GenericMapping,
        PrivilegeSet,
        PrivilegeSetLength,
        GrantedAccess,
        AccessStatus,
        0);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial int32 Xwcsicmp(ж<TLS> t, uintptr string1, uintptr string2) {
    if (__ccgo_strace) {
        trc("t=%v string2=%v, (%v:)", t.OrTypedNil(), string2, origin(2));
    }
    @string s1 = strings.ToLower(goWideString(string1));
    @string s2 = strings.ToLower(goWideString(string2));
    return (int32)strings.Compare(s1, s2);
}

public static partial int32 XSetCurrentDirectoryW(ж<TLS> t, uintptr lpPathName) {
    if (__ccgo_strace) {
        trc("t=%v lpPathName=%v, (%v:)", t.OrTypedNil(), lpPathName, origin(2));
    }
    var err = windows.SetCurrentDirectory(@unsafe.Pointer.To<uint16>((@unsafe.Pointer)lpPathName));
    if (err != nil) {
        t.setErrno(err);
        return 0;
    }
    return 1;
}

public static partial uint32 XGetCurrentDirectoryW(ж<TLS> t, uint32 nBufferLength, uintptr lpBuffer) {
    if (__ccgo_strace) {
        trc("t=%v nBufferLength=%v lpBuffer=%v, (%v:)", t.OrTypedNil(), nBufferLength, lpBuffer, origin(2));
    }
    var (n, err) = windows.GetCurrentDirectory(nBufferLength, @unsafe.Pointer.To<uint16>((@unsafe.Pointer)lpBuffer));
    if (err != nil) {
        t.setErrno(err);
    }
    return n;
}

public static partial int32 XGetFileInformationByHandle(ж<TLS> t, uintptr hFile, uintptr lpFileInformation) {
    if (__ccgo_strace) {
        trc("t=%v lpFileInformation=%v, (%v:)", t.OrTypedNil(), lpFileInformation, origin(2));
    }
    var (r1, _, e1) = procGetFileInformationByHandle.Call(hFile, lpFileInformation, 0);
    if (r1 == 0) {
        if (!AreEqual(e1, windows.NOERROR)) {
            t.setErrno(e1);
        } else {
            t.setErrno((nint)(errno.EINVAL));
        }
    }
    return (int32)r1;
}

public static partial int32 XGetVolumeInformationW(ж<TLS> t, uintptr lpRootPathName, uintptr lpVolumeNameBuffer, uint32 nVolumeNameSize, uintptr lpVolumeSerialNumber, uintptr lpMaximumComponentLength, uintptr lpFileSystemFlags, uintptr lpFileSystemNameBuffer, uint32 nFileSystemNameSize) {
    if (__ccgo_strace) {
        trc("t=%v lpVolumeNameBuffer=%v nVolumeNameSize=%v lpFileSystemNameBuffer=%v nFileSystemNameSize=%v, (%v:)", t.OrTypedNil(), lpVolumeNameBuffer, nVolumeNameSize, lpFileSystemNameBuffer, nFileSystemNameSize, origin(2));
    }
    var (r0, _, err) = procGetVolumeInformationW.Call(lpRootPathName,
        lpVolumeNameBuffer,
        (uintptr)nVolumeNameSize,
        lpVolumeSerialNumber,
        lpMaximumComponentLength,
        lpFileSystemFlags,
        lpFileSystemNameBuffer,
        (uintptr)nFileSystemNameSize,
        0);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial uintptr Xwcschr(ж<TLS> t, uintptr str, wchar_t c) {
    if (__ccgo_strace) {
        trc("t=%v str=%v c=%v, (%v:)", t.OrTypedNil(), str, c, origin(2));
    }
    for (;;) {
        var w = ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)str);
        if (w == c) {
            return str;
        }

        if (w == 0) {
            return 0;
        }

        str += 2;
    }
}

public static int32 /*r*/ XSetFileTime(ж<TLS> t, uintptr _hFile, uintptr _lpCreationTime, uintptr _lpLastAccessTime, uintptr _lpLastWriteTime) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("hFile=%+v lpCreationTime=%+v lpLastAccessTime=%+v lpLastWriteTime=%+v", _hFile, _lpCreationTime, _lpLastAccessTime, _lpLastWriteTime);
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc(@"XSetFileTime->%+v", r);
            }, ref ᒐ);
        }
        var (r0, r1, err) = procSetFileTime.Call(_hFile, _lpCreationTime, _lpLastAccessTime, _lpLastWriteTime);
        if (!AreEqual(err, windows.NOERROR)) {
            if (__ccgo_strace) {
                trc(@"r0=%v r1=%v err=%v", r0, r1, err);
            }
            t.SetLastError((uint32)(err._<windowsꓸErrno>()));
        }
        r = (int32)r0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static uint32 /*r*/ XGetNamedSecurityInfoW(ж<TLS> tls, uintptr _pObjectName, int32 _ObjectType, uint32 _SecurityInfo, uintptr _ppsidOwner, uintptr _ppsidGroup, uintptr _ppDacl, uintptr _ppSacl, uintptr _ppSecurityDescriptor) {
    uint32 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("pObjectName=%+v ObjectType=%+v SecurityInfo=%+v ppsidOwner=%+v ppsidGroup=%+v ppDacl=%+v ppSacl=%+v ppSecurityDescriptor=%+v", _pObjectName, _ObjectType, _SecurityInfo, _ppsidOwner, _ppsidGroup, _ppDacl, _ppSacl, _ppSecurityDescriptor);
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc(@"XGetNamedSecurityInfoW->%+v", r);
            }, ref ᒐ);
        }
        var (r0, r1, err) = procGetNamedSecurityInfoW.Call(_pObjectName, (uintptr)_ObjectType, (uintptr)_SecurityInfo, _ppsidOwner, _ppsidGroup, _ppDacl, _ppSacl, _ppSecurityDescriptor);
        if (!AreEqual(err, windows.NOERROR)) {
            if (__ccgo_strace) {
                trc(@"r0=%v r1=%v err=%v", r0, r1, err);
            }
            tls.SetLastError((uint32)(err._<windowsꓸErrno>()));
        }
        r = (uint32)r0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static int32 /*r*/ XOpenProcessToken(ж<TLS> tls, uintptr _ProcessHandle, uint32 _DesiredAccess, uintptr _TokenHandle) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("ProcessHandle=%+v DesiredAccess=%+v TokenHandle=%+v", _ProcessHandle, _DesiredAccess, _TokenHandle);
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc(@"XOpenProcessToken->%+v", r);
            }, ref ᒐ);
        }
        var (r0, r1, err) = procOpenProcessToken.Call(_ProcessHandle, (uintptr)_DesiredAccess, _TokenHandle);
        if (!AreEqual(err, windows.NOERROR)) {
            if (__ccgo_strace) {
                trc(@"r0=%v r1=%v err=%v", r0, r1, err);
            }
            tls.SetLastError((uint32)(err._<windowsꓸErrno>()));
        }
        r = (int32)r0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static int32 /*r*/ XGetTokenInformation(ж<TLS> tls, uintptr _TokenHandle, int32 _TokenInformationClass, uintptr _TokenInformation, uint32 _TokenInformationLength, uintptr _ReturnLength) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("TokenHandle=%+v TokenInformationClass=%+v TokenInformation=%+v TokenInformationLength=%+v ReturnLength=%+v", _TokenHandle, _TokenInformationClass, _TokenInformation, _TokenInformationLength, _ReturnLength);
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc(@"XGetTokenInformation->%+v", r);
            }, ref ᒐ);
        }
        var (r0, r1, err) = procGetTokenInformation.Call(_TokenHandle, (uintptr)_TokenInformationClass, _TokenInformation, (uintptr)_TokenInformationLength, _ReturnLength);
        if (!AreEqual(err, windows.NOERROR)) {
            if (__ccgo_strace) {
                trc(@"r0=%v r1=%v err=%v", r0, r1, err);
            }
            tls.SetLastError((uint32)(err._<windowsꓸErrno>()));
        }
        r = (int32)r0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static int32 /*r*/ XEqualSid(ж<TLS> tls, uintptr _pSid1, uintptr _pSid2) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("pSid1=%+v pSid2=%+v", _pSid1, _pSid2);
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc(@"XEqualSid->%+v", r);
            }, ref ᒐ);
        }
        var (r0, r1, err) = procEqualSid.Call(_pSid1, _pSid2);
        if (!AreEqual(err, windows.NOERROR)) {
            if (__ccgo_strace) {
                trc(@"r0=%v r1=%v err=%v", r0, r1, err);
            }
            tls.SetLastError((uint32)(err._<windowsꓸErrno>()));
        }
        r = (int32)r0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static partial int32 XWSAStartup(ж<TLS> t, uint16 wVersionRequired, uintptr lpWSAData) {
    if (__ccgo_strace) {
        trc("t=%v wVersionRequired=%v lpWSAData=%v, (%v:)", t.OrTypedNil(), wVersionRequired, lpWSAData, origin(2));
    }
    var (r0, _, _) = procWSAStartup.Call((uintptr)wVersionRequired, lpWSAData, 0);
    if (r0 != 0) {
        t.setErrno(r0);
    }
    return (int32)r0;
}

public static partial uintptr XGetModuleHandleA(ж<TLS> t, uintptr lpModuleName) {
    if (__ccgo_strace) {
        trc("t=%v lpModuleName=%v, (%v:)", t.OrTypedNil(), lpModuleName, origin(2));
    }
    var (r0, _, err) = procGetModuleHandleA.Call(lpModuleName, 0, 0);
    if (r0 == 0) {
        t.setErrno(err);
    }
    return r0;
}

public static partial uintptr XGetModuleHandleW(ж<TLS> t, uintptr lpModuleName) {
    if (__ccgo_strace) {
        trc("t=%v lpModuleName=%v, (%v:)", t.OrTypedNil(), lpModuleName, origin(2));
    }
    var (r0, _, err) = procGetModuleHandleW.Call(lpModuleName, 0, 0);
    if (r0 == 0) {
        t.setErrno(err);
    }
    return r0;
}

public static partial uint32 XGetEnvironmentVariableW(ж<TLS> t, uintptr lpName, uintptr lpBuffer, uint32 nSize) {
    if (__ccgo_strace) {
        trc("t=%v lpBuffer=%v nSize=%v, (%v:)", t.OrTypedNil(), lpBuffer, nSize, origin(2));
    }
    var (r0, _, e1) = procGetEnvironmentVariableW.Call(lpName, lpBuffer, (uintptr)nSize);
    var n = (uint32)r0;
    if (n == 0) {
        if (!AreEqual(e1, windows.NOERROR)) {
            t.setErrno(e1);
        } else {
            t.setErrno((nint)(errno.EINVAL));
        }
    }
    return n;
}

public static partial int32 XlstrcmpiA(ж<TLS> t, uintptr lpString1, uintptr lpString2) {
    if (__ccgo_strace) {
        trc("t=%v lpString2=%v, (%v:)", t.OrTypedNil(), lpString2, origin(2));
    }
    @string s1 = strings.ToLower(GoString(lpString1));
    @string s2 = strings.ToLower(GoString(lpString2));
    return (int32)strings.Compare(s1, s2);
}

public static partial int32 XGetModuleFileNameA(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial uint32 XGetACP(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    var (r0, _, _) = procGetACP.Call(0, 0, 0);
    return (uint32)r0;
}

public static partial int32 XGetUserNameW(ж<TLS> t, uintptr lpBuffer, uintptr pcbBuffer) {
    if (__ccgo_strace) {
        trc("t=%v pcbBuffer=%v, (%v:)", t.OrTypedNil(), pcbBuffer, origin(2));
    }
    var (u, err) = user.Current();
    if (err != nil) {
        throw panic(todo(""));
        return 0;
    }

    var wcnt = ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)pcbBuffer);
    var s = utf16.Encode(slice<rune>(u.Username));
    if (len(s) + 1 > (nint)wcnt) {
        throw panic(todo(""));
    }

    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)pcbBuffer)).Value = (uint16)(len(s) + 1);
    foreach (var (_, v) in range(s)) {
        (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)lpBuffer)).Value = v;
        lpBuffer += 2;
    }
    return 1;
}

public static partial uintptr XLoadLibraryExW(ж<TLS> t, uintptr lpLibFileName, uintptr hFile, uint32 dwFlags) {
    if (__ccgo_strace) {
        trc("t=%v hFile=%v dwFlags=%v, (%v:)", t.OrTypedNil(), hFile, dwFlags, origin(2));
    }
    return 0;
}

public static partial uintptr Xwcscpy(ж<TLS> t, uintptr strDestination, uintptr strSource) {
    if (__ccgo_strace) {
        trc("t=%v strSource=%v, (%v:)", t.OrTypedNil(), strSource, origin(2));
    }
    if (strSource == 0) {
        return 0;
    }

    var d = strDestination;
    for (;;) {
        var c = ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)strSource);
        strSource += 2;
        (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)d)).Value = c;
        d += 2;
        if (c == 0) {
            return strDestination;
        }
    }
}

public static partial int32 XwsprintfW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XRegisterClassW(ж<TLS> t, uintptr lpWndClass) {
    if (__ccgo_strace) {
        trc("t=%v lpWndClass=%v, (%v:)", t.OrTypedNil(), lpWndClass, origin(2));
    }
    var (r0, _, err) = procRegisterClassW.Call(lpWndClass, 0, 0);
    if (r0 == 0) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial int32 XKillTimer(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XDestroyWindow(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XUnregisterClassW(ж<TLS> t, uintptr lpClassName, uintptr hInstance) {
    if (__ccgo_strace) {
        trc("t=%v hInstance=%v, (%v:)", t.OrTypedNil(), hInstance, origin(2));
    }
    var (r0, _, err) = procUnregisterClassW.Call(lpClassName, hInstance, 0);
    if (r0 == 0) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial int32 XPostMessageW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XSetTimer(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial uintptr XCreateWindowExW(ж<TLS> t, uint32 dwExStyle, uintptr lpClassName, uintptr lpWindowName, uint32 dwStyle, int32 x, int32 y, int32 nWidth, int32 nHeight, uintptr hWndParent, uintptr hMenu, uintptr hInstance, uintptr lpParam) {
    if (__ccgo_strace) {
        trc("t=%v dwExStyle=%v lpWindowName=%v dwStyle=%v nHeight=%v lpParam=%v, (%v:)", t.OrTypedNil(), dwExStyle, lpWindowName, dwStyle, nHeight, lpParam, origin(2));
    }
    var (r0, _, err) = procCreateWindowExW.Call(
        (uintptr)dwExStyle,
        lpClassName,
        lpWindowName,
        (uintptr)dwStyle,
        (uintptr)x,
        (uintptr)y,
        (uintptr)nWidth,
        (uintptr)nHeight,
        hWndParent,
        hMenu,
        hInstance,
        lpParam);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return r0;
}

public static partial int32 XPeekMessageW(ж<TLS> t, uintptr lpMsg, uintptr hWnd, uint32 wMsgFilterMin, uint32 wMsgFilterMax, uint32 wRemoveMsg) {
    if (__ccgo_strace) {
        trc("t=%v hWnd=%v wRemoveMsg=%v, (%v:)", t.OrTypedNil(), hWnd, wRemoveMsg, origin(2));
    }
    var (r0, _, err) = procPeekMessageW.Call(lpMsg,
        hWnd,
        (uintptr)wMsgFilterMin,
        (uintptr)wMsgFilterMax,
        (uintptr)wRemoveMsg,
        0);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial int32 XGetMessageW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XPostQuitMessage(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XTranslateMessage(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XDispatchMessageW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial uint32 XSleepEx(ж<TLS> t, uint32 dwMilliseconds, int32 bAlertable) {
    if (__ccgo_strace) {
        trc("t=%v dwMilliseconds=%v bAlertable=%v, (%v:)", t.OrTypedNil(), dwMilliseconds, bAlertable, origin(2));
    }
    var (r0, _, _) = procSleepEx.Call((uintptr)dwMilliseconds, (uintptr)bAlertable, 0);
    return (uint32)r0;
}

public static partial int32 XCreatePipe(ж<TLS> t, uintptr hReadPipe, uintptr hWritePipe, uintptr lpPipeAttributes, uint32 nSize) {
    if (__ccgo_strace) {
        trc("t=%v lpPipeAttributes=%v nSize=%v, (%v:)", t.OrTypedNil(), lpPipeAttributes, nSize, origin(2));
    }
    var (r0, _, err) = procCreatePipe.Call(hReadPipe, hWritePipe, lpPipeAttributes, (uintptr)nSize, 0, 0);
    if (r0 == 0) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static int32 XCreateProcessW(ж<TLS> t, uintptr lpApplicationName, uintptr lpCommandLine, uintptr lpProcessAttributes, uintptr lpThreadAttributes, int32 bInheritHandles, uint32 dwCreationFlags, uintptr lpEnvironment, uintptr lpCurrentDirectory, uintptr lpStartupInfo, uintptr lpProcessInformation) {
    var (r1, _, e1) = procCreateProcessW.Call(lpApplicationName, lpCommandLine, lpProcessAttributes, lpThreadAttributes,
        (uintptr)bInheritHandles, (uintptr)dwCreationFlags, lpEnvironment, lpCurrentDirectory, lpStartupInfo, lpProcessInformation, 0, 0);
    if (r1 == 0) {
        if (!AreEqual(e1, windows.NOERROR)) {
            t.setErrno(e1);
        } else {
            t.setErrno((nint)(errno.EINVAL));
        }
    }

    return (int32)r1;
}

public static partial int32 XWaitForInputIdle(ж<TLS> t, uintptr hProcess, uint32 dwMilliseconds) {
    if (__ccgo_strace) {
        trc("t=%v hProcess=%v dwMilliseconds=%v, (%v:)", t.OrTypedNil(), hProcess, dwMilliseconds, origin(2));
    }
    var (r0, _, _) = procWaitForInputIdle.Call(hProcess, (uintptr)dwMilliseconds, 0);
    return (int32)r0;
}

public static partial int32 XSearchPathW(ж<TLS> t, uintptr lpPath, uintptr lpFileName, uintptr lpExtension, uint32 nBufferLength, uintptr lpBuffer, uintptr lpFilePart) {
    if (__ccgo_strace) {
        trc("t=%v lpExtension=%v nBufferLength=%v lpFilePart=%v, (%v:)", t.OrTypedNil(), lpExtension, nBufferLength, lpFilePart, origin(2));
    }
    var (r0, _, err) = procSearchPathW.Call(lpPath, lpFileName, lpExtension, (uintptr)nBufferLength, lpBuffer, lpFilePart);
    if (r0 == 0) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial int32 XGetShortPathNameW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XGetExitCodeProcess(ж<TLS> t, uintptr hProcess, uintptr lpExitCode) {
    if (__ccgo_strace) {
        trc("t=%v lpExitCode=%v, (%v:)", t.OrTypedNil(), lpExitCode, origin(2));
    }
    var (r0, _, err) = procGetExitCodeProcess.Call(hProcess, lpExitCode, 0);
    if (r0 == 0) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial int32 XPeekNamedPipe(ж<TLS> t, uintptr hNamedPipe, uintptr lpBuffer, uint32 nBufferSize, uintptr lpBytesRead, uintptr lpTotalBytesAvail, uintptr lpBytesLeftThisMessage) {
    if (__ccgo_strace) {
        trc("t=%v lpBuffer=%v nBufferSize=%v lpBytesLeftThisMessage=%v, (%v:)", t.OrTypedNil(), lpBuffer, nBufferSize, lpBytesLeftThisMessage, origin(2));
    }
    var (r0, _, err) = procPeekNamedPipe.Call(hNamedPipe, lpBuffer, (uintptr)nBufferSize, lpBytesRead, lpTotalBytesAvail, lpBytesLeftThisMessage);
    if (r0 == 0) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial @long X_InterlockedExchange(ж<TLS> t, uintptr Target, @long Value) {
    if (__ccgo_strace) {
        trc("t=%v Target=%v Value=%v, (%v:)", t.OrTypedNil(), Target, Value, origin(2));
    }
    var old = atomic.SwapInt32(@unsafe.Pointer.To<int32>((@unsafe.Pointer)Target), Value);
    return old;
}

public static partial int32 XTerminateThread(ж<TLS> t, uintptr hThread, uint32 dwExitCode) {
    if (__ccgo_strace) {
        trc("t=%v hThread=%v dwExitCode=%v, (%v:)", t.OrTypedNil(), hThread, dwExitCode, origin(2));
    }
    var (r0, _, err) = procTerminateThread.Call(hThread, (uintptr)dwExitCode, 0);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial int32 XGetComputerNameW(ж<TLS> t, uintptr lpBuffer, uintptr nSize) {
    if (__ccgo_strace) {
        trc("t=%v nSize=%v, (%v:)", t.OrTypedNil(), nSize, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xgethostname(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XSendMessageW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XWSAGetLastError(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 Xclosesocket(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XWspiapiFreeAddrInfo(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XWspiapiGetNameInfo(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XIN6_ADDR_EQUAL(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 X__ccgo_in6addr_anyp(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XIN6_IS_ADDR_V4MAPPED(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XSetHandleInformation(ж<TLS> t, uintptr hObject, uint32 dwMask, uint32 dwFlags) {
    if (__ccgo_strace) {
        trc("t=%v hObject=%v dwMask=%v dwFlags=%v, (%v:)", t.OrTypedNil(), hObject, dwMask, dwFlags, origin(2));
    }
    var err = windows.SetHandleInformation((windows.Handle)hObject, dwMask, dwFlags); if (err != nil) {
        t.setErrno(err);
        return 0;
    }

    return 1;
}

public static partial int32 Xioctlsocket(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XGetWindowLongPtrW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XSetWindowLongPtrW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XWSAAsyncSelect(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial uintptr Xinet_ntoa(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial uint32 X_controlfp(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XQueryPerformanceFrequency(ж<TLS> t, uintptr lpFrequency) {
    if (__ccgo_strace) {
        trc("t=%v lpFrequency=%v, (%v:)", t.OrTypedNil(), lpFrequency, origin(2));
    }

    var (r1, _, err) = procQueryPerformanceFrequency.Call(lpFrequency, 0, 0);
    if (r1 == 0) {
        t.setErrno(err);
        return 0;
    }
    return (int32)r1;
}

internal static bool inDST(gotime.Time t) {
    var jan1st = gotime.Date(t.Year(), 1, 1, 0, 0, 0, 0, t.Location());

    var (_, off1) = t.Zone();
    var (_, off2) = jan1st.Zone();

    return off1 != off2;
}

public static partial void X_ftime(ж<TLS> t, uintptr timeptr) {
    if (__ccgo_strace) {
        trc("t=%v timeptr=%v, (%v:)", t.OrTypedNil(), timeptr, origin(2));
    }
    gotime.Time tm = gotime.Now();
    ж<timeꓸX__timeb64> tPtr = @unsafe.Pointer.To<timeꓸX__timeb64>((@unsafe.Pointer)timeptr);
    tPtr.Ftime = tm.Unix();
    tPtr.Fmillitm = (uint16)(((gotime.Duration)(int64)tm.Nanosecond()) / gotime.Millisecond);
    if (inDST(tm)) {
        tPtr.Fdstflag = 1;
    }
    var (_, offset) = tm.Zone();
    tPtr.Ftimezone = (int16)offset;
}

public static partial uint32 XDdeInitializeW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial uintptr XDdeCreateStringHandleW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XDdeNameService(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 X_snwprintf(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XDdeQueryStringW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 X_wcsicmp(ж<TLS> t, uintptr string1, uintptr string2) {
    if (__ccgo_strace) {
        trc("t=%v string2=%v, (%v:)", t.OrTypedNil(), string2, origin(2));
    }
    return Xwcsicmp(t, string1, string2);
}

public static partial uintptr XDdeCreateDataHandle(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial uintptr XDdeAccessData(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XDdeUnaccessData(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XDdeUninitialize(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial uintptr XDdeConnect(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XDdeFreeStringHandle(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XRegisterClassExW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XGlobalGetAtomNameW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial uint16 XGlobalAddAtomW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XEnumWindows(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XIsWindow(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XGlobalDeleteAtom(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial uint32 XDdeGetLastError(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial uintptr XDdeClientTransaction(ж<TLS> t, uintptr pData, uint32 cbData, uintptr hConv, uintptr hszItem, uint32 wFmt, uint32 wType, uint32 dwTimeout, uintptr pdwResult) {
    if (__ccgo_strace) {
        trc("t=%v pData=%v cbData=%v hConv=%v hszItem=%v dwTimeout=%v pdwResult=%v, (%v:)", t.OrTypedNil(), pData, cbData, hConv, hszItem, dwTimeout, pdwResult, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 XDdeAbandonTransaction(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XDdeFreeDataHandle(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XDdeGetData(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XDdeDisconnect(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XRegCloseKey(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XRegDeleteValueW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XRegEnumKeyExW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XRegQueryValueExW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XRegEnumValueW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XRegConnectRegistryW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XRegCreateKeyExW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XRegOpenKeyExW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XRegDeleteKeyW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 XRegSetValueExW(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial int32 X__mingw_vsnwprintf(ж<TLS> t, uintptr buffer, typesꓸSize_t count, uintptr format, uintptr va) {
    if (__ccgo_strace) {
        trc("t=%v buffer=%v count=%v va=%v, (%v:)", t.OrTypedNil(), buffer, count, va, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 X__mingw_vprintf(ж<TLS> t, uintptr s, uintptr ap) {
    if (__ccgo_strace) {
        trc("t=%v ap=%v, (%v:)", t.OrTypedNil(), ap, origin(2));
    }
    return Xvprintf(t, s, ap);
}

public static partial int32 X__mingw_vfscanf(ж<TLS> t, uintptr stream, uintptr format, uintptr ap) {
    if (__ccgo_strace) {
        trc("t=%v ap=%v, (%v:)", t.OrTypedNil(), ap, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 X__mingw_vsscanf(ж<TLS> t, uintptr str, uintptr format, uintptr ap) {
    if (__ccgo_strace) {
        trc("t=%v ap=%v, (%v:)", t.OrTypedNil(), ap, origin(2));
    }
    return Xsscanf(t, str, format, ap);
}

public static partial int32 X__mingw_vfprintf(ж<TLS> t, uintptr f, uintptr format, uintptr va) {
    if (__ccgo_strace) {
        trc("t=%v f=%v va=%v, (%v:)", t.OrTypedNil(), f, va, origin(2));
    }
    return Xvfprintf(t, f, format, va);
}

public static partial int32 X__mingw_vsprintf(ж<TLS> t, uintptr s, uintptr format, uintptr ap) {
    if (__ccgo_strace) {
        trc("t=%v ap=%v, (%v:)", t.OrTypedNil(), ap, origin(2));
    }
    return Xvsprintf(t, s, format, ap);
}

public static partial int32 X__mingw_vsnprintf(ж<TLS> t, uintptr str, typesꓸSize_t size, uintptr format, uintptr ap) {
    if (__ccgo_strace) {
        trc("t=%v str=%v size=%v ap=%v, (%v:)", t.OrTypedNil(), str, size, ap, origin(2));
    }
    return Xvsnprintf(t, str, size, format, ap);
}

public static partial int32 X_putchar(ж<TLS> t, int32 c) {
    if (__ccgo_strace) {
        trc("t=%v c=%v, (%v:)", t.OrTypedNil(), c, origin(2));
    }
    var (_, err) = fwrite(unistd.STDOUT_FILENO, [(byte)c]); if (err != nil) {
        return -1;
    }
    return (int32)(byte)c;
}

public static partial int32 X__mingw_vfwscanf(ж<TLS> t, uintptr stream, uintptr format, uintptr ap) {
    if (__ccgo_strace) {
        trc("t=%v stream=%v ap=%v, (%v:)", t.OrTypedNil(), stream, ap, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 X__mingw_vswscanf(ж<TLS> t, uintptr stream, uintptr format, uintptr ap) {
    if (__ccgo_strace) {
        trc("t=%v stream=%v ap=%v, (%v:)", t.OrTypedNil(), stream, ap, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 X__mingw_vfwprintf(ж<TLS> t, uintptr stream, uintptr format, uintptr ap) {
    if (__ccgo_strace) {
        trc("t=%v ap=%v, (%v:)", t.OrTypedNil(), ap, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xputchar(ж<TLS> t, int32 c) {
    if (__ccgo_strace) {
        trc("t=%v c=%v, (%v:)", t.OrTypedNil(), c, origin(2));
    }
    var (_, err) = fwrite(unistd.STDOUT_FILENO, [(byte)c]); if (err != nil) {
        return -1;
    }
    return (int32)(byte)c;
}

public static partial void X_assert(ж<TLS> t, uintptr message, uintptr filename, uint32 line) {
    if (__ccgo_strace) {
        trc("t=%v filename=%v line=%v, (%v:)", t.OrTypedNil(), filename, line, origin(2));
    }
    throw panic(todo(""));
}

public static partial uintptr X_strdup(ж<TLS> t, uintptr s) {
    if (__ccgo_strace) {
        trc("t=%v s=%v, (%v:)", t.OrTypedNil(), s, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 X_access(ж<TLS> t, uintptr pathname, int32 mode) {
    if (__ccgo_strace) {
        trc("t=%v pathname=%v mode=%v, (%v:)", t.OrTypedNil(), pathname, mode, origin(2));
    }

    @string path = GoString(pathname);

    var (info, err) = os.Stat(path);
    if (err != nil) {
        return errno.ENOENT;
    }

    switch (mode) {
    case 0: {
        return 0;
    }
    case 2: {
        if ((fs.FileMode)(info.Mode().Perm() & ((fs.FileMode)((uint32)1 << (int)((nuint)7)))) == 1) {
            return 0;
        }
        break;
    }
    case 4: {
        if ((fs.FileMode)(info.Mode().Perm() & ((fs.FileMode)((uint32)1 << (int)((nuint)7)))) == 0) {
            return 0;
        }
        break;
    }
    case 6: {
        if ((fs.FileMode)(info.Mode().Perm() & ((fs.FileMode)((uint32)1 << (int)((nuint)7)))) == 1) {
            return 0;
        }
        break;
    }}

    return errno.EACCES;
}

public static partial int32 XSetConsoleCtrlHandler(ж<TLS> t, uintptr HandlerRoutine, int32 Add) {
    if (__ccgo_strace) {
        trc("t=%v HandlerRoutine=%v Add=%v, (%v:)", t.OrTypedNil(), HandlerRoutine, Add, origin(2));
    }

    return 0;
}

public static partial void XDebugBreak(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 X_isatty(ж<TLS> t, int32 fd) {
    if (__ccgo_strace) {
        trc("t=%v fd=%v, (%v:)", t.OrTypedNil(), fd, origin(2));
    }

    var (f, ok) = fdToFile(fd);
    if (!ok) {
        t.setErrno((nint)(errno.EBADF));
        return 0;
    }

    if (fd == unistd.STDOUT_FILENO || fd == unistd.STDIN_FILENO || fd == unistd.STDERR_FILENO) {
        ref var mode = ref heap(new uint32(), out var Ꮡmode);
        var err = windows.GetConsoleMode(f.Handle, Ꮡmode);
        if (err != nil) {
            t.setErrno((nint)(errno.EINVAL));
            return 0;
        }
        return 1;
    }

    return 0;
}

public static partial int32 XSetConsoleTextAttribute(ж<TLS> t, uintptr hConsoleOutput, uint16 wAttributes) {
    if (__ccgo_strace) {
        trc("t=%v hConsoleOutput=%v wAttributes=%v, (%v:)", t.OrTypedNil(), hConsoleOutput, wAttributes, origin(2));
    }
    var (r1, _, _) = procSetConsoleTextAttribute.Call(hConsoleOutput, (uintptr)wAttributes, 0);
    return (int32)r1;
}

public static partial int32 XGetConsoleScreenBufferInfo(ж<TLS> t, uintptr hConsoleOutput, uintptr lpConsoleScreenBufferInfo) {
    if (__ccgo_strace) {
        trc("t=%v lpConsoleScreenBufferInfo=%v, (%v:)", t.OrTypedNil(), lpConsoleScreenBufferInfo, origin(2));
    }
    var (r1, _, _) = procGetConsoleScreenBufferInfo.Call(hConsoleOutput, lpConsoleScreenBufferInfo, 0);
    return (int32)r1;
}

public static partial uintptr X_popen(ж<TLS> t, uintptr command, uintptr mode) {
    if (__ccgo_strace) {
        trc("t=%v mode=%v, (%v:)", t.OrTypedNil(), mode, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 X_wunlink(ж<TLS> t, uintptr filename) {
    if (__ccgo_strace) {
        trc("t=%v filename=%v, (%v:)", t.OrTypedNil(), filename, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xclosedir(ж<TLS> tls, uintptr dir) {
    if (__ccgo_strace) {
        trc("tls=%v dir=%v, (%v:)", tls.OrTypedNil(), dir, origin(2));
    }
    throw panic(todo(""));
}

public static partial uintptr Xopendir(ж<TLS> tls, uintptr name) {
    if (__ccgo_strace) {
        trc("tls=%v name=%v, (%v:)", tls.OrTypedNil(), name, origin(2));
    }
    throw panic(todo(""));
}

public static partial uintptr Xreaddir(ж<TLS> tls, uintptr dir) {
    if (__ccgo_strace) {
        trc("tls=%v dir=%v, (%v:)", tls.OrTypedNil(), dir, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 X_unlink(ж<TLS> t, uintptr filename) {
    if (__ccgo_strace) {
        trc("t=%v filename=%v, (%v:)", t.OrTypedNil(), filename, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 X_pclose(ж<TLS> t, uintptr stream) {
    if (__ccgo_strace) {
        trc("t=%v stream=%v, (%v:)", t.OrTypedNil(), stream, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xsetmode(ж<TLS> t, int32 fd, int32 mode) {
    if (__ccgo_strace) {
        trc("t=%v mode=%v, (%v:)", t.OrTypedNil(), mode, origin(2));
    }
    return X_setmode(t, fd, mode);
}

public static partial int32 X_setmode(ж<TLS> t, int32 fd, int32 mode) {
    if (__ccgo_strace) {
        trc("t=%v mode=%v, (%v:)", t.OrTypedNil(), mode, origin(2));
    }

    var (_, ok) = fdToFile(fd);
    if (!ok) {
        t.setErrno((nint)(errno.EBADF));
        return -1;
    }

    if (mode == fcntl.O_BINARY) {
        return fcntl.O_BINARY;
    } else {
        t.setErrno((nint)(errno.EINVAL));
        return -1;
    }
}

public static partial int32 X_mkdir(ж<TLS> t, uintptr dirname) {
    if (__ccgo_strace) {
        trc("t=%v dirname=%v, (%v:)", t.OrTypedNil(), dirname, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 X_chmod(ж<TLS> t, uintptr filename, int32 pmode) {
    if (__ccgo_strace) {
        trc("t=%v filename=%v pmode=%v, (%v:)", t.OrTypedNil(), filename, pmode, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 X_fileno(ж<TLS> t, uintptr stream) {
    if (__ccgo_strace) {
        trc("t=%v stream=%v, (%v:)", t.OrTypedNil(), stream, origin(2));
    }
    var (f, ok) = winGetObject(stream)._<ж<@file>>(ᐧ);
    if (!ok) {
        t.setErrno((nint)(errno.EBADF));
        return -1;
    }
    return f._fd;
}

public static partial void Xrewind(ж<TLS> t, uintptr stream) {
    if (__ccgo_strace) {
        trc("t=%v stream=%v, (%v:)", t.OrTypedNil(), stream, origin(2));
    }
    Xfseek(t, stream, 0, unistd.SEEK_SET);
}

public static partial void X__atomic_load_n(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 X__atomic_store_n(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial void X__builtin_add_overflow(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    throw panic(todo(""));
}

public static partial void X__builtin_mul_overflow(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    throw panic(todo(""));
}

public static partial void X__builtin_sub_overflow(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    throw panic(todo(""));
}

internal static slice<uint16> goWideBytes(uintptr p, nint n) {
    var wᴷ = new stackAppend<uint16>(moves: true);

    var b = GoBytes(p, 2 * n);
    slice<uint16> w = default;
    for (nint i = 0; i < len(b); i += 2) {
        w = append(wᴷ, w, ~Ꮡ(b, i).Reinterpret<byte, uint16>());
    }
    w = move2heap(wᴷ, w);
    return w;
}

internal static @string goWideString(uintptr p) {
    var wᴷ = new stackAppend<uint16>();

    if (p == 0) {
        return "";
    }
    slice<uint16> w = default;
    ж<RawMem> raw = @unsafe.Pointer.To<RawMem>((@unsafe.Pointer)p);
    nint i = 0;
    for (;;) {
        var wc = ~raw.at<byte>(i).Reinterpret<byte, uint16>();
        w = append(wᴷ, w, wc);
        if (wc == 0) {
            break;
        }
        i = i + 2;
    }
    var s = utf16.Decode(w);
    return (@string)s;
}

internal static partial @string goWideStringN(uintptr p, nint n) {
    throw panic(todo(""));
}

internal static @string goWideStringNZ(uintptr p) {
    var wᴷ = new stackAppend<uint16>();

    if (p == 0) {
        return "";
    }

    slice<uint16> w = default;
    ж<RawMem> raw = @unsafe.Pointer.To<RawMem>((@unsafe.Pointer)p);
    nint i = 0;
    for (;;) {
        var wc = ~raw.at<byte>(i).Reinterpret<byte, uint16>();
        if (wc == 0) {
            break;
        }

        w = append(wᴷ, w, wc);
        i = i + 2;
    }
    var s = utf16.Decode(w);
    return (@string)s;
}

public static partial uintptr XGetCommandLineW(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    return (uintptr)windows.GetCommandLine();
}

public static partial int32 XAddAccessDeniedAce(ж<TLS> t, uintptr pAcl, uint32 dwAceRevision, uint32 AccessMask, uintptr pSid) {
    if (__ccgo_strace) {
        trc("t=%v pAcl=%v AccessMask=%v pSid=%v, (%v:)", t.OrTypedNil(), pAcl, AccessMask, pSid, origin(2));
    }
    throw panic(todo(""));
}

public static uint32 /*r*/ XAddAce(ж<TLS> tls, uintptr _pAcl, uint32 _dwAceRevision, uint32 _dwStartingAceIndex, uintptr _pAceList, uint32 _nAceListLength) {
    uint32 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("pAcl=%+v dwAceRevision=%+v dwStartingAceIndex=%+v pAceList=%+v nAceListLength=%+v", _pAcl, _dwAceRevision, _dwStartingAceIndex, _pAceList, _nAceListLength);
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc(@"XAddAce->%+v", r);
            }, ref ᒐ);
        }
        var (r0, r1, err) = procAddAce.Call(_pAcl, (uintptr)_dwAceRevision, (uintptr)_dwStartingAceIndex, _pAceList, (uintptr)_nAceListLength);
        if (!AreEqual(err, windows.NOERROR)) {
            if (__ccgo_strace) {
                trc(@"r0=%v r1=%v err=%v", r0, r1, err);
            }
            tls.SetLastError((uint32)(err._<windowsꓸErrno>()));
        }
        r = (uint32)r0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static int32 /*r*/ XGetAce(ж<TLS> tls, uintptr _pAcl, uint32 _dwAceIndex, uintptr _pAce) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("pAcl=%+v dwAceIndex=%+v pAce=%+v", _pAcl, _dwAceIndex, _pAce);
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc(@"XGetAce->%+v", r);
            }, ref ᒐ);
        }
        var (r0, r1, err) = procGetAce.Call(_pAcl, (uintptr)_dwAceIndex, _pAce);
        if (!AreEqual(err, windows.NOERROR)) {
            if (__ccgo_strace) {
                trc(@"r0=%v r1=%v err=%v", r0, r1, err);
            }
            tls.SetLastError((uint32)(err._<windowsꓸErrno>()));
        }
        r = (int32)r0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static partial int32 XGetAclInformation(ж<TLS> t, uintptr pAcl, uintptr pAclInformation, uint32 nAclInformationLength, int32 dwAclInformationClass) {
    if (__ccgo_strace) {
        trc("t=%v pAclInformation=%v dwAclInformationClass=%v, (%v:)", t.OrTypedNil(), pAclInformation, dwAclInformationClass, origin(2));
    }
    var (r0, _, err) = procGetAclInformation.Call(pAclInformation,
        pAclInformation,
        (uintptr)nAclInformationLength,
        (uintptr)dwAclInformationClass,
        0,
        0);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial int32 XGetFileSecurityA(ж<TLS> t, uintptr lpFileName, uint32 RequestedInformation, uintptr pSecurityDescriptor, uint32 nLength, uintptr lpnLengthNeeded) {
    if (__ccgo_strace) {
        trc("t=%v lpFileName=%v RequestedInformation=%v pSecurityDescriptor=%v nLength=%v lpnLengthNeeded=%v, (%v:)", t.OrTypedNil(), lpFileName, RequestedInformation, pSecurityDescriptor, nLength, lpnLengthNeeded, origin(2));
    }
    var (r0, _, err) = procGetFileSecurityA.Call(lpFileName,
        (uintptr)RequestedInformation,
        pSecurityDescriptor,
        (uintptr)nLength,
        lpnLengthNeeded,
        0);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static uint32 /*r*/ XGetLengthSid(ж<TLS> tls, uintptr _pSid) {
    uint32 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("pSid=%+v", _pSid);
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc(@"XGetLengthSid->%+v", r);
            }, ref ᒐ);
        }
        var (r0, r1, err) = procGetLengthSid.Call(_pSid);
        if (!AreEqual(err, windows.NOERROR)) {
            if (__ccgo_strace) {
                trc(@"r0=%v r1=%v err=%v", r0, r1, err);
            }
            tls.SetLastError((uint32)(err._<windowsꓸErrno>()));
        }
        r = (uint32)r0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static partial int32 XGetSecurityDescriptorDacl(ж<TLS> t, uintptr pSecurityDescriptor, uintptr lpbDaclPresent, uintptr pDacl, uintptr lpbDaclDefaulted) {
    if (__ccgo_strace) {
        trc("t=%v lpbDaclDefaulted=%v, (%v:)", t.OrTypedNil(), lpbDaclDefaulted, origin(2));
    }
    var (r0, _, err) = procGetSecurityDescriptorDacl.Call(pSecurityDescriptor,
        lpbDaclPresent,
        pDacl,
        lpbDaclDefaulted,
        0,
        0);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial int32 XGetSidLengthRequired(ж<TLS> t, uint8 nSubAuthorityCount) {
    if (__ccgo_strace) {
        trc("t=%v nSubAuthorityCount=%v, (%v:)", t.OrTypedNil(), nSubAuthorityCount, origin(2));
    }
    var (r0, _, err) = procGetSidLengthRequired.Call((uintptr)nSubAuthorityCount, 0, 0);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial uintptr XGetSidSubAuthority(ж<TLS> t, uintptr pSid, uint32 nSubAuthority) {
    if (__ccgo_strace) {
        trc("t=%v pSid=%v nSubAuthority=%v, (%v:)", t.OrTypedNil(), pSid, nSubAuthority, origin(2));
    }
    var (r0, _, err) = procGetSidSubAuthority.Call(pSid, (uintptr)nSubAuthority, 0);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return r0;
}

public static int32 /*r*/ XInitializeAcl(ж<TLS> tls, uintptr _pAcl, uint32 _nAclLength, uint32 _dwAclRevision) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("pAcl=%+v nAclLength=%+v dwAclRevision=%+v", _pAcl, _nAclLength, _dwAclRevision);
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc(@"XInitializeAcl->%+v", r);
            }, ref ᒐ);
        }
        var (r0, r1, err) = procInitializeAcl.Call(_pAcl, (uintptr)_nAclLength, (uintptr)_dwAclRevision);
        if (!AreEqual(err, windows.NOERROR)) {
            if (__ccgo_strace) {
                trc(@"r0=%v r1=%v err=%v", r0, r1, err);
            }
            tls.SetLastError((uint32)(err._<windowsꓸErrno>()));
        }
        r = (int32)r0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static partial int32 XInitializeSid(ж<TLS> t, uintptr Sid, uintptr pIdentifierAuthority, uint8 nSubAuthorityCount) {
    if (__ccgo_strace) {
        trc("t=%v pIdentifierAuthority=%v nSubAuthorityCount=%v, (%v:)", t.OrTypedNil(), pIdentifierAuthority, nSubAuthorityCount, origin(2));
    }
    var (r0, _, err) = procInitializeSid.Call(Sid, pIdentifierAuthority, (uintptr)nSubAuthorityCount);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial void XRaiseException(ж<TLS> t, uint32 dwExceptionCode, uint32 dwExceptionFlags, uint32 nNumberOfArguments, uintptr lpArguments) {
    if (__ccgo_strace) {
        trc("t=%v nNumberOfArguments=%v lpArguments=%v, (%v:)", t.OrTypedNil(), nNumberOfArguments, lpArguments, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 XSetErrorMode(ж<TLS> t, uint32 uMode) {
    if (__ccgo_strace) {
        trc("t=%v uMode=%v, (%v:)", t.OrTypedNil(), uMode, origin(2));
    }
    throw panic(todo(""));
}

public static partial uint32 XSetNamedSecurityInfoA(ж<TLS> t, uintptr pObjectName, int32 ObjectType, uint32 SecurityInfo, uintptr psidOwner, uintptr psidGroup, uintptr pDacl, uintptr pSacl) {
    if (__ccgo_strace) {
        trc("t=%v pObjectName=%v SecurityInfo=%v pSacl=%v, (%v:)", t.OrTypedNil(), pObjectName, SecurityInfo, pSacl, origin(2));
    }
    throw panic(todo(""));
}

public static int32 XCreateProcessA(ж<TLS> t, uintptr lpApplicationName, uintptr lpCommandLine, uintptr lpProcessAttributes, uintptr lpThreadAttributes, int32 bInheritHandles, uint32 dwCreationFlags, uintptr lpEnvironment, uintptr lpCurrentDirectory, uintptr lpStartupInfo, uintptr lpProcessInformation) {
    var (r1, _, err) = procCreateProcessA.Call(lpApplicationName, lpCommandLine, lpProcessAttributes, lpThreadAttributes,
        (uintptr)bInheritHandles, (uintptr)dwCreationFlags, lpEnvironment, lpCurrentDirectory, lpStartupInfo, lpProcessInformation, 0, 0);
    if (r1 == 0) {
        if (!AreEqual(err, windows.NOERROR)) {
            t.setErrno(err);
        } else {
            t.setErrno((nint)(errno.EINVAL));
        }
    }
    return (int32)r1;
}

public static partial uint32 X_set_abort_behavior(ж<TLS> t, params ꓸꓸꓸany _ʗp) {
    throw panic(todo(""));
}

public static partial uintptr XOpenEventA(ж<TLS> t, uint32 dwDesiredAccess, uint32 bInheritHandle, uintptr lpName) {
    if (__ccgo_strace) {
        trc("t=%v dwDesiredAccess=%v bInheritHandle=%v lpName=%v, (%v:)", t.OrTypedNil(), dwDesiredAccess, bInheritHandle, lpName, origin(2));
    }
    var (r0, _, err) = procOpenEventA.Call((uintptr)dwDesiredAccess, (uintptr)bInheritHandle, lpName);
    if (r0 == 0) {
        t.setErrno(err);
    }
    return r0;
}

public static partial typesꓸSize_t X_msize(ж<TLS> t, uintptr memblock) {
    if (__ccgo_strace) {
        trc("t=%v memblock=%v, (%v:)", t.OrTypedNil(), memblock, origin(2));
    }
    return (typesꓸSize_t)UsableSize(memblock);
}

public static partial @ulong X_byteswap_ulong(ж<TLS> t, @ulong val) {
    if (__ccgo_strace) {
        trc("t=%v val=%v, (%v:)", t.OrTypedNil(), val, origin(2));
    }
    return X__builtin_bswap32(t, val);
}

public static partial uint64 X_byteswap_uint64(ж<TLS> t, uint64 val) {
    if (__ccgo_strace) {
        trc("t=%v val=%v, (%v:)", t.OrTypedNil(), val, origin(2));
    }
    return X__builtin_bswap64(t, val);
}

public static partial int32 X_commit(ж<TLS> t, int32 fd) {
    if (__ccgo_strace) {
        trc("t=%v fd=%v, (%v:)", t.OrTypedNil(), fd, origin(2));
    }
    return Xfsync(t, fd);
}

public static partial int32 X_stati64(ж<TLS> t, uintptr path, uintptr buffer) {
    if (__ccgo_strace) {
        trc("t=%v path=%v buffer=%v, (%v:)", t.OrTypedNil(), path, buffer, origin(2));
    }
    var (r0, _, err) = procStati64.Call((uintptr)path, (uintptr)buffer);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial int32 X_fstati64(ж<TLS> t, int32 fd, uintptr buffer) {
    return X_fstat64(t, fd, buffer);
}

public static partial int32 X_findnext32(ж<TLS> t, typesꓸIntptr_t handle, uintptr buffer) {
    if (__ccgo_strace) {
        trc("t=%v handle=%v buffer=%v, (%v:)", t.OrTypedNil(), handle, buffer, origin(2));
    }
    var (r0, _, err) = procFindnext32.Call((uintptr)handle, buffer);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial typesꓸIntptr_t X_findfirst32(ж<TLS> t, uintptr filespec, uintptr fileinfo) {
    if (__ccgo_strace) {
        trc("t=%v fileinfo=%v, (%v:)", t.OrTypedNil(), fileinfo, origin(2));
    }
    var (r0, _, err) = procFindfirst32.Call(filespec, fileinfo);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (typesꓸIntptr_t)r0;
}

public static partial @long Xstrtol(ж<TLS> t, uintptr nptr, uintptr endptr, int32 @base) {
    if (__ccgo_strace) {
        trc("t=%v endptr=%v base=%v, (%v:)", t.OrTypedNil(), endptr, @base, origin(2));
    }

    ref var s = ref heap(new uintptr(), out var Ꮡs);

    s = nptr;
    @ulong acc = default;
    byte c = default;
    @ulong cutoff = default;
    int32 neg = default;
    int32 any = default;
    int32 cutlim = default;

    for (;;) {
        c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s);
        PostIncUintptr(Ꮡs, 1);
        @string spΔ1 = strings.TrimSpace((@string)c);
        if (len(spΔ1) > 0) {
            break;
        }
    }

    if (c == '-') {
        neg = 1;
        c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s);
        PostIncUintptr(Ꮡs, 1);
    } else if (c == '+') {
        c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s);
        PostIncUintptr(Ꮡs, 1);
    }

    var sp = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s);

    if ((@base == 0 || @base == 16) && c == '0' && (sp == 'x' || sp == 'X')) {
        PostIncUintptr(Ꮡs, 1);
        c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s);
        PostIncUintptr(Ꮡs, 1);
        @base = 16;
    }
    if (@base == 0) {
        if (c == '0') {
            @base = 0;
        } else {
            @base = 10;
        }
    }
    @ulong ULONG_MAX = 0xFFFFFFFFU;
    @long LONG_MAX = ((@long)(ULONG_MAX >> 1));
    @long LONG_MIN = ~LONG_MAX;

    if (neg == 1) {
        cutoff = (@ulong)(-1 * LONG_MIN);
    } else {
        cutoff = (@ulong)LONG_MAX;
    }
    cutlim = (int32)(cutoff % ((@ulong)@base));
    cutoff = cutoff / ((@ulong)@base);

    acc = 0;
    any = 0;

    for (;;) {
        @string cs = ((@string)c);
        if (unicode.IsDigit((slice<rune>(cs))[0])) {
            c -= (byte)'0';
        } else if (unicode.IsLetter((slice<rune>(cs))[0])) {
            if (unicode.IsUpper((slice<rune>(cs))[0])) {
                c -= (byte)((rune)'A' - 10);
            } else {
                c -= (byte)((rune)'a' - 10);
            }
        } else {
            break;
        }

        if ((int32)c >= @base) {
            break;
        }
        if (any < 0 || acc > cutoff || (acc == cutoff && (int32)c > cutlim)) {
            any = -1;
        } else {
            any = 1;
            acc *= ((@ulong)@base);
            acc += ((@ulong)c);
        }

        c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s);
        PostIncUintptr(Ꮡs, 1);
    }

    if (any < 0) {
        if (neg == 1) {
            acc = (@ulong)LONG_MIN;
        } else {
            acc = (@ulong)LONG_MAX;
        }
        t.setErrno((nint)(errno.ERANGE));
    } else if (neg == 1) {
        acc = ((@ulong)0 - acc);
    }

    if (endptr != 0) {
        if (any == 1) {
            PostDecUintptr(Ꮡs, 1);
            AssignPtrUintptr(endptr, s);
        } else {
            AssignPtrUintptr(endptr, nptr);
        }
    }
    return (@long)acc;
}

public static partial @ulong Xstrtoul(ж<TLS> t, uintptr nptr, uintptr endptr, int32 @base) {
    if (__ccgo_strace) {
        trc("t=%v endptr=%v base=%v, (%v:)", t.OrTypedNil(), endptr, @base, origin(2));
    }
    ref var s = ref heap(new uintptr(), out var Ꮡs);

    s = nptr;
    @ulong acc = default;
    byte c = default;
    @ulong cutoff = default;
    int32 neg = default;
    int32 any = default;
    int32 cutlim = default;

    for (;;) {
        c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s);
        PostIncUintptr(Ꮡs, 1);
        @string spΔ1 = strings.TrimSpace((@string)c);
        if (len(spΔ1) > 0) {
            break;
        }
    }

    if (c == '-') {
        neg = 1;
        c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s);
        PostIncUintptr(Ꮡs, 1);
    } else if (c == '+') {
        c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s);
        PostIncUintptr(Ꮡs, 1);
    }

    var sp = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s);

    if ((@base == 0 || @base == 16) && c == '0' && (sp == 'x' || sp == 'X')) {
        PostIncUintptr(Ꮡs, 1);
        c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s);
        PostIncUintptr(Ꮡs, 1);
        @base = 16;
    }
    if (@base == 0) {
        if (c == '0') {
            @base = 0;
        } else {
            @base = 10;
        }
    }
    @ulong ULONG_MAX = 0xFFFFFFFFU;

    cutoff = ULONG_MAX / ((@ulong)@base);
    cutlim = (int32)(ULONG_MAX % ((@ulong)@base));

    acc = 0;
    any = 0;

    for (;;) {
        @string cs = ((@string)c);
        if (unicode.IsDigit((slice<rune>(cs))[0])) {
            c -= (byte)'0';
        } else if (unicode.IsLetter((slice<rune>(cs))[0])) {
            if (unicode.IsUpper((slice<rune>(cs))[0])) {
                c -= (byte)((rune)'A' - 10);
            } else {
                c -= (byte)((rune)'a' - 10);
            }
        } else {
            break;
        }

        if ((int32)c >= @base) {
            break;
        }
        if (any < 0 || acc > cutoff || (acc == cutoff && (int32)c > cutlim)) {
            any = -1;
        } else {
            any = 1;
            acc *= ((@ulong)@base);
            acc += ((@ulong)c);
        }

        c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s);
        PostIncUintptr(Ꮡs, 1);
    }

    if (any < 0) {
        acc = ULONG_MAX;
        t.setErrno((nint)(errno.ERANGE));
    } else if (neg == 1) {
        acc = ((@ulong)0 - acc);
    }

    if (endptr != 0) {
        if (any == 1) {
            PostDecUintptr(Ꮡs, 1);
            AssignPtrUintptr(endptr, s);
        } else {
            AssignPtrUintptr(endptr, nptr);
        }
    }
    return acc;
}

public static partial int32 X__isoc99_sscanf(ж<TLS> t, uintptr str, uintptr format, uintptr va) {
    if (__ccgo_strace) {
        trc("t=%v va=%v, (%v:)", t.OrTypedNil(), va, origin(2));
    }
    var r = scanf(new strings_ReaderжByteScanner(strings.NewReader(GoString(str))), format, va);
    return r;
}

public static partial int32 Xsscanf(ж<TLS> t, uintptr str, uintptr format, uintptr va) {
    if (__ccgo_strace) {
        trc("t=%v va=%v, (%v:)", t.OrTypedNil(), va, origin(2));
    }
    var r = scanf(new strings_ReaderжByteScanner(strings.NewReader(GoString(str))), format, va);
    return r;
}

internal static float64 _toint4 = Float64FromInt32(1) / Float64FromFloat64(2.220446049250313e-16D);

[global::go.golib.GoInlineLayout(Size = 8, Align = 8), StructLayout(LayoutKind.Explicit, Size = 8)] internal partial struct Xrint__ᴛ1 /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<Tuint64_t>))] public readonly long Fi;
    [FieldOffset(0)] public float64 Ff;
}

public static partial float64 Xrint(ж<TLS> tls, float64 x) {
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v x=%v, (%v:)", tls.OrTypedNil(), x, origin(2));
        }
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 e = default;
        int32 s = default;
        Tdouble_t y = default;
        float64 v1 = default;
        Xrint__ᴛ1 _ᴛ1 = new();
        _ = e;
        _ = s;
        _ = y;
        _ = v1;
        (@unsafe.Pointer.To<Xrint__ᴛ1>((@unsafe.Pointer)bp)).Value = new Xrint__ᴛ1();
        (@unsafe.Pointer.To<float64>((@unsafe.Pointer)bp)).Value = x;
        e = Int32FromUint64((Tuint64_t)(~@unsafe.Pointer.To<Tuint64_t>((@unsafe.Pointer)bp) >> (int)((int32)52) & (uint64)0x7ff));
        s = Int32FromUint64(~@unsafe.Pointer.To<Tuint64_t>((@unsafe.Pointer)bp) >> (int)((int32)63));
        if (e >= Int32FromInt32(0x3ff) + Int32FromInt32(52)) {
            return x;
        }
        if (s != 0) {
            y = x - _toint4 + _toint4;
        } else {
            y = x + _toint4 - _toint4;
        }
        if (y == Float64FromInt32(0)) {
            if (s != 0) {
                v1 = -Float64FromFloat64(0D);
            } else {
                v1 = Float64FromInt32(0);
            }
            return v1;
        }
        return y;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public static partial uintptr Xfdopen(ж<TLS> t, int32 fd, uintptr mode) {
    if (__ccgo_strace) {
        trc("t=%v fd=%v mode=%v, (%v:)", t.OrTypedNil(), fd, mode, origin(2));
    }
    throw panic(todo(""));
}

public static partial uintptr X_gmtime64(ж<TLS> t, uintptr sourceTime) {
    if (__ccgo_strace) {
        trc("t=%v sourceTime=%v, (%v:)", t.OrTypedNil(), sourceTime, origin(2));
    }
    var (r0, _, err) = procGmtime64.Call((uintptr)sourceTime);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (uintptr)r0;
}

public static partial timeꓸX__time64_t X_mktime64(ж<TLS> t, uintptr timeptr) {
    if (__ccgo_strace) {
        trc("t=%v timeptr=%v, (%v:)", t.OrTypedNil(), timeptr, origin(2));
    }
    return (timeꓸX__time64_t)Xmktime(t, timeptr);
}

public static partial uintptr Xgai_strerrorA(ж<TLS> t, int32 ecode) {
    if (__ccgo_strace) {
        trc("t=%v ecode=%v, (%v:)", t.OrTypedNil(), ecode, origin(2));
    }
    throw panic(todo(""));
}

partial struct __timeb64 {
    internal typesꓸX__time64_t time;
    internal uint32 millitm;
    internal int16 timezone;
    internal int16 dstflag;
}

public static partial void X_ftime64(ж<TLS> t, uintptr timeptr) {
    if (__ccgo_strace) {
        trc("t=%v timeptr=%v, (%v:)", t.OrTypedNil(), timeptr, origin(2));
    }
    var tm = gotime.Now();
    (@unsafe.Pointer.To<__timeb64>((@unsafe.Pointer)timeptr)).Value.time = (typesꓸX__time64_t)tm.Unix();

    (@unsafe.Pointer.To<__timeb64>((@unsafe.Pointer)timeptr)).Value.millitm = (uint32)((int64)tm.Nanosecond() / 1000000);
}

public static partial int32 X__ccgo_getMutexType(ж<TLS> tls, uintptr m) {
    if (__ccgo_strace) {
        trc("tls=%v m=%v, (%v:)", tls.OrTypedNil(), m, origin(2));
    }
    return ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)m) & 15;
}

public static partial int32 X__ccgo_pthreadAttrGetDetachState(ж<TLS> tls, uintptr a) {
    if (__ccgo_strace) {
        trc("tls=%v a=%v, (%v:)", tls.OrTypedNil(), a, origin(2));
    }
    return ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)a);
}

public static partial int32 X__ccgo_pthreadMutexattrGettype(ж<TLS> tls, uintptr a) {
    if (__ccgo_strace) {
        trc("tls=%v a=%v, (%v:)", tls.OrTypedNil(), a, origin(2));
    }
    return ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)a) & (int32)3;
}

public static int32 Xchmod(ж<TLS> t, uintptr pathname, int32 mode) {
    var (r0, _, err) = procChmod.Call(pathname, (uintptr)mode);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial int32 XGetComputerNameExW(ж<TLS> t, int32 nameType, uintptr lpBuffer, uintptr nSize) {
    if (__ccgo_strace) {
        trc("t=%v nameType=%v nSize=%v, (%v:)", t.OrTypedNil(), nameType, nSize, origin(2));
    }
    var (r0, _, err) = procGetComputerNameExW.Call((uintptr)nameType, lpBuffer, nSize);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial float64 X_copysign(ж<TLS> t, float64 x, float64 y) {
    if (__ccgo_strace) {
        trc("t=%v y=%v, (%v:)", t.OrTypedNil(), y, origin(2));
    }
    return Xcopysign(t, x, y);
}

public static partial int32 X_wtoi(ж<TLS> t, uintptr str) {
    if (__ccgo_strace) {
        trc("t=%v str=%v, (%v:)", t.OrTypedNil(), str, origin(2));
    }
    throw panic(todo(""));
}

internal static uintptr /*r*/ allocW(ж<TLS> t, @string v) {
    uintptr r = default;

    var s = utf16.Encode(slice<rune>(v));
    var p = Xcalloc(t, (typesꓸSize_t)(len(s) + 1), 2);
    if (p == 0) {
        throw panic(todo(""));
    }

    r = p;
    foreach (var (_, vΔ1) in range(s)) {
        (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = vΔ1;
        p += 2;
    }
    return r;
}

public static partial uintptr X_wgetenv(ж<TLS> t, uintptr varname) {
    if (__ccgo_strace) {
        trc("t=%v varname=%v, (%v:)", t.OrTypedNil(), varname, origin(2));
    }
    if (!wenvValid) {
        bootWinEnviron(t);
    }
    @string k0 = goWideStringNZ(varname);
    @string k = strings.ToLower(k0);
    foreach (var (_, v) in range(winEnviron.slice(0, len(winEnviron) - 1))) {
        @string s = strings.ToLower(goWideStringNZ(v));
        nint x = strings.IndexByte(s, (byte)'=');
        if (x >= 0 && s.slice(0, x) == k) {
            return v + (uintptr)(2 * (x + 1));
        }
    }

    var (val, ok) = os.LookupEnv(k0); if (ok) {
        var np = allocW(t, k0 + "="u8 + val);
        winEnviron = winEnviron.slice(0, len(winEnviron) - 1);
        winEnviron = append(winEnviron, np, (uintptr)0);
        wenviron = (uintptr)@unsafe.Pointer.FromBox(Ꮡ(winEnviron, 0));
        return np + (uintptr)(2 * (len(slice<rune>(k0)) + 1));
    }

    return 0;
}

public static partial int32 X_wputenv(ж<TLS> t, uintptr envstring) {
    if (__ccgo_strace) {
        trc("t=%v envstring=%v, (%v:)", t.OrTypedNil(), envstring, origin(2));
    }
    if (!wenvValid) {
        bootWinEnviron(t);
    }
    @string s0 = goWideStringNZ(envstring);
    nint x0 = strings.IndexByte(s0, (byte)'=');
    if (x0 <= 0) {
        t.setErrno((nint)(errno.EINVAL));
        return -1;
    }

    putenvNarrow(t, s0);
    os.Setenv(s0.slice(0, x0), s0.slice(x0 + 1));

    @string s = strings.ToLower(s0);
    @string k = s.slice(0, x0);
    foreach (var (i, v) in range(winEnviron.slice(0, len(winEnviron) - 1))) {
        @string s2 = strings.ToLower(goWideStringNZ(v));
        nint x = strings.IndexByte(s2, (byte)'=');
        if (x >= 0 && s2.slice(0, x) == k) {
            Xfree(t, v);
            winEnviron[i] = allocW(t, s0);
            return 0;
        }
    }

    var np = allocW(t, s0);
    winEnviron = winEnviron.slice(0, len(winEnviron) - 1);
    winEnviron = append(winEnviron, np, (uintptr)0);
    wenviron = (uintptr)@unsafe.Pointer.FromBox(Ꮡ(winEnviron, 0));
    return 0;
}

internal static void bootWinEnviron(ж<TLS> t) {
    winEnviron = winEnviron[..0];
    foreach (var (_, s) in range(os.Environ())) {
        winEnviron = append(winEnviron, allocW(t, s));
    }
    winEnviron = append(winEnviron, (uintptr)0);
    wenviron = (uintptr)@unsafe.Pointer.FromBox(Ꮡ(winEnviron, 0));
    wenvValid = true;
}

public static partial float64 Xfabsl(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return math.Abs(x);
}

public static partial float64 Xscalbn(ж<TLS> t, float64 x, int32 n) {
    if (__ccgo_strace) {
        trc("t=%v x=%v n=%v, (%v:)", t.OrTypedNil(), x, n, origin(2));
    }
    return math.Ldexp(x, (nint)n);
}

public static int32 X__stdio_common_vfprintf(ж<TLS> t, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    throw panic("TODO");
}

public static int32 X__stdio_common_vfprintf_p(ж<TLS> t, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    throw panic("TODO");
}

public static int32 X__stdio_common_vfprintf_s(ж<TLS> t, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    throw panic("TODO");
}

public static int32 X__stdio_common_vfscanf(ж<TLS> t, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    throw panic("TODO");
}

public static int32 X__stdio_common_vfwprintf_s(ж<TLS> t, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    throw panic("TODO");
}

public static int32 X__stdio_common_vfwscanf(ж<TLS> t, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    throw panic("TODO");
}

public static int32 X__stdio_common_vsnprintf_s(ж<TLS> t, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    throw panic("TODO");
}

public static int32 X__stdio_common_vsnwprintf_s(ж<TLS> t, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    throw panic("TODO");
}

public static int32 X__stdio_common_vsprintf(ж<TLS> t, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    throw panic("TODO");
}

public static int32 X__stdio_common_vsprintf_p(ж<TLS> t, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    throw panic("TODO");
}

public static int32 X__stdio_common_vsprintf_s(ж<TLS> t, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    throw panic("TODO");
}

public static int32 X__stdio_common_vsscanf(ж<TLS> t, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    throw panic("TODO");
}

public static int32 X__stdio_common_vswprintf(ж<TLS> t, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    throw panic("TODO");
}

public static int32 X__stdio_common_vswprintf_s(ж<TLS> t, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    throw panic("TODO");
}

public static int32 X__stdio_common_vswscanf(ж<TLS> t, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    throw panic("TODO");
}

public static partial int64 X_lseeki64(ж<TLS> t, int32 fd, int64 offset, int32 whence) {
    if (__ccgo_strace) {
        trc("t=%v fd=%v offset=%v whence=%v, (%v:)", t.OrTypedNil(), fd, offset, whence, origin(2));
    }

    var (f, ok) = fdToFile(fd);
    if (!ok) {
        t.setErrno((nint)(errno.EBADF));
        return -1;
    }

    var (n, err) = windows.Seek(f.Handle, offset, (nint)whence);
    if (err != nil) {
        if (dmesgs) {
            dmesg("%v: fd %v, off %#x, whence %v: %v", origin(1), f._fd, offset, whenceStr(whence), n);
        }
        t.setErrno(err);
        return -1;
    }

    if (dmesgs) {
        dmesg("%v: fd %v, off %#x, whence %v: ok", origin(1), f._fd, offset, whenceStr(whence));
    }
    return n;
}

public static partial int32 Xislower(ж<TLS> tls, int32 c) {
    if (__ccgo_strace) {
        trc("tls=%v c=%v, (%v:)", tls.OrTypedNil(), c, origin(2));
    }
    return Bool32((uint32)c - (uint32)'a' < (uint32)26);
}

public static partial int32 Xisupper(ж<TLS> tls, int32 c) {
    if (__ccgo_strace) {
        trc("tls=%v c=%v, (%v:)", tls.OrTypedNil(), c, origin(2));
    }
    return Bool32((uint32)c - (uint32)'A' < (uint32)26);
}

public static partial int32 Xaccess(ж<TLS> t, uintptr pathname, int32 mode) {
    if (__ccgo_strace) {
        trc("t=%v pathname=%v mode=%v, (%v:)", t.OrTypedNil(), pathname, mode, origin(2));
    }
    var (r0, _, err) = procAccess.Call((uintptr)pathname, (uintptr)mode);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial int32 X_vscprintf(ж<TLS> t, uintptr format, uintptr argptr) {
    if (__ccgo_strace) {
        trc("t=%v format=%v argptr=%v, (%v:)", t.OrTypedNil(), format, argptr, origin(2));
    }

    return (int32)len(printf(format, argptr));
}

public static partial int32 X_stat32i64(ж<TLS> t, uintptr path, uintptr buffer) {
    if (__ccgo_strace) {
        trc("t=%v path=%v buffer=%v, (%v:)", t.OrTypedNil(), path, buffer, origin(2));
    }
    var (r0, _, err) = procStat32i64.Call((uintptr)path, (uintptr)buffer);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial int32 X_stat64i32(ж<TLS> t, uintptr path, uintptr buffer) {
    if (__ccgo_strace) {
        trc("t=%v path=%v buffer=%v, (%v:)", t.OrTypedNil(), path, buffer, origin(2));
    }
    var (r0, _, err) = procStat64i32.Call((uintptr)path, (uintptr)buffer);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return (int32)r0;
}

public static partial int32 X__stat32i64(ж<TLS> t, uintptr path, uintptr buffer) {
    return X_stat32i64(t, path, buffer);
}

public static partial int32 X__stat64i32(ж<TLS> t, uintptr path, uintptr buffer) {
    return X_stat64i32(t, path, buffer);
}

public static uint8 AtomicLoadNUint8(uintptr ptr, int32 memorder) {
    return (byte)a_load_8(ptr);
}

internal static readonly ж<timeꓸTm> Ꮡ_tm = new StaticBox<timeꓸTm>(default(timeꓸTm));
internal static ref timeꓸTm _tm => ref ((StaticBox<timeꓸTm>)Ꮡ_tm).ValueSlot;

public static partial uintptr /*r*/ Xgmtime(ж<TLS> tls, uintptr t) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v t=%v, (%v:)", tls.OrTypedNil(), t, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        r = Xgmtime_r(tls, t, (uintptr)Ꮡ_tm);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

[GoLazy] partial class lazyᴛ_days_in_month { internal static array<int8> _days_in_month = new array<int8>(12){
    [0] = (int8)31,
    [1] = (int8)30,
    [2] = (int8)31,
    [3] = (int8)30,
    [4] = (int8)31,
    [5] = (int8)31,
    [6] = (int8)30,
    [7] = (int8)31,
    [8] = (int8)30,
    [9] = (int8)31,
    [10] = (int8)31,
    [11] = (int8)29
}; }

internal static array<int8> x___utc = new int8[]{(int8)'U', (int8)'T', (int8)'C'}.array(4);

public static partial uintptr /*r*/ Xgmtime_r(ж<TLS> tls, uintptr t, uintptr tm) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v t=%v tm=%v, (%v:)", tls.OrTypedNil(), t, tm, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        if (x___secs_to_tm(ref (tls).DerefOrNull(), (int64)(~@unsafe.Pointer.To<timeꓸTime_t>((@unsafe.Pointer)t)), tm) < 0) {
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)X__errno_location(tls))).Value = (int32)errno.EOVERFLOW;
            r = (uintptr)0; goto ᒐdone;
        }
        (@unsafe.Pointer.To<timeꓸTm>((@unsafe.Pointer)tm)).Value.Ftm_isdst = 0;
        r = tm;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ x___secs_to_tm(ref TLS tls, int64 t, uintptr tm) {
    int32 c_cycles = default;
    int32 leap = default;
    int32 months = default;
    int32 q_cycles = default;
    int32 qc_cycles = default;
    int32 remdays = default;
    int32 remsecs = default;
    int32 remyears = default;
    int32 wday = default;
    int32 yday = default;
    int64 days = default;
    int64 secs = default;
    int64 years = default;
    _ = c_cycles;
    _ = days;
    _ = leap;
    _ = months;
    _ = q_cycles;
    _ = qc_cycles;
    _ = remdays;
    _ = remsecs;
    _ = remyears;
    _ = secs;
    _ = wday;
    _ = yday;
    _ = years;
    if (t < (int64)(-Int32FromInt32(1) - Int32FromInt32(0x7fffffff)) * Int64FromInt64(31622400) || t > Int64FromInt32(limits.INT_MAX) * Int64FromInt64(31622400)) {
        return -(int32)1;
    }
    secs = t - (Int64FromInt64(946684800) + (int64)(Int32FromInt32(86400) * (Int32FromInt32(31) + Int32FromInt32(29))));
    days = secs / (int64)86400;
    remsecs = (int32)(secs % (int64)86400);
    if (remsecs < 0) {
        remsecs += (int32)86400;
        days--;
    }
    wday = (int32)(((int64)3 + days) % (int64)7);
    if (wday < 0) {
        wday += (int32)7;
    }
    qc_cycles = (int32)(quo(days, (int64)(Int32FromInt32(365) * Int32FromInt32(400) + Int32FromInt32(97))));
    remdays = (int32)(rem(days, (int64)(Int32FromInt32(365) * Int32FromInt32(400) + Int32FromInt32(97))));
    if (remdays < 0) {
        remdays += Int32FromInt32(365) * Int32FromInt32(400) + Int32FromInt32(97);
        qc_cycles--;
    }
    c_cycles = quo(remdays, (Int32FromInt32(365) * Int32FromInt32(100) + Int32FromInt32(24)));
    if (c_cycles == (int32)4) {
        c_cycles--;
    }
    remdays -= c_cycles * (Int32FromInt32(365) * Int32FromInt32(100) + Int32FromInt32(24));
    q_cycles = quo(remdays, (Int32FromInt32(365) * Int32FromInt32(4) + Int32FromInt32(1)));
    if (q_cycles == (int32)25) {
        q_cycles--;
    }
    remdays -= q_cycles * (Int32FromInt32(365) * Int32FromInt32(4) + Int32FromInt32(1));
    remyears = remdays / (int32)365;
    if (remyears == (int32)4) {
        remyears--;
    }
    remdays -= remyears * (int32)365;
    leap = BoolInt32(!(remyears != 0) && (q_cycles != 0 || !(c_cycles != 0)));
    yday = remdays + (int32)31 + (int32)28 + leap;
    if (yday >= (int32)365 + leap) {
        yday -= (int32)365 + leap;
    }
    years = (int64)(remyears + (int32)4 * q_cycles + (int32)100 * c_cycles) + (int64)400 * (int64)(int64)qc_cycles;
    months = 0;
    for (;;) {
        if (!((int32)_days_in_month[months] <= remdays)) {
            break;
        }
        remdays -= (int32)_days_in_month[months];
        goto _1;
_1:
        months++;
    }
    if (months >= (int32)10) {
        months -= (int32)12;
        years++;
    }
    if (years + (int64)100 > (int64)limits.INT_MAX || years + (int64)100 < (int64)(-Int32FromInt32(1) - Int32FromInt32(0x7fffffff))) {
        return -(int32)1;
    }
    (@unsafe.Pointer.To<timeꓸTm>((@unsafe.Pointer)tm)).Value.Ftm_year = (int32)(years + (int64)100);
    (@unsafe.Pointer.To<timeꓸTm>((@unsafe.Pointer)tm)).Value.Ftm_mon = months + (int32)2;
    (@unsafe.Pointer.To<timeꓸTm>((@unsafe.Pointer)tm)).Value.Ftm_mday = remdays + (int32)1;
    (@unsafe.Pointer.To<timeꓸTm>((@unsafe.Pointer)tm)).Value.Ftm_wday = wday;
    (@unsafe.Pointer.To<timeꓸTm>((@unsafe.Pointer)tm)).Value.Ftm_yday = yday;
    (@unsafe.Pointer.To<timeꓸTm>((@unsafe.Pointer)tm)).Value.Ftm_hour = remsecs / (int32)3600;
    (@unsafe.Pointer.To<timeꓸTm>((@unsafe.Pointer)tm)).Value.Ftm_min = remsecs / (int32)60 % (int32)60;
    (@unsafe.Pointer.To<timeꓸTm>((@unsafe.Pointer)tm)).Value.Ftm_sec = remsecs % (int32)60;
    return 0;
}

public static partial size_t /*r*/ Xstrftime(ж<TLS> tls, uintptr s, size_t n, uintptr f, uintptr tm) {
    size_t r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v s=%v n=%v f=%v tm=%v, (%v:)", tls.OrTypedNil(), s, n, f, tm, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        var tt = gotime.Date(
            (nint)((@unsafe.Pointer.To<timeꓸTm>((@unsafe.Pointer)tm)).Value.Ftm_year + 1900),
            (gotime.Month)(nint)((@unsafe.Pointer.To<timeꓸTm>((@unsafe.Pointer)tm)).Value.Ftm_mon + 1),
            (nint)(@unsafe.Pointer.To<timeꓸTm>((@unsafe.Pointer)tm)).Value.Ftm_mday,
            (nint)(@unsafe.Pointer.To<timeꓸTm>((@unsafe.Pointer)tm)).Value.Ftm_hour,
            (nint)(@unsafe.Pointer.To<timeꓸTm>((@unsafe.Pointer)tm)).Value.Ftm_min,
            (nint)(@unsafe.Pointer.To<timeꓸTm>((@unsafe.Pointer)tm)).Value.Ftm_sec,
            0,
            gotime.UTC);
        @string fmt = GoString(f);
        @string result = default;
        if (fmt != ""u8) {
            result = strftime.Format(fmt, tt);
        }
        r = (size_t)len(result);
        switch (ᐧ) {
        case {} when r > n: {
            r = 0;
            break;
        }
        default: {
            copy(@unsafe.Pointer.View<byte>((@unsafe.Pointer)s, 0, (nint)(r), (nint)(r), 1125899906842623), result);
            (@unsafe.Pointer.To<byte>((@unsafe.Pointer)(s + (uintptr)r))).Value = 0;
            break;
        }}

    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static partial float64 X__mingw_strtod(ж<TLS> t, uintptr s, uintptr p) {
    return Xstrtod(t, s, p);
}

public static partial int32 X_vsnprintf(ж<TLS> t, uintptr str, typesꓸSize_t size, uintptr format, uintptr ap) {
    if (__ccgo_strace) {
        trc("t=%v str=%v size=%v ap=%v, (%v:)", t.OrTypedNil(), str, size, ap, origin(2));
    }
    return Xvsnprintf(t, str, size, format, ap);
}

public static partial void X__ccgo_SyscallFP() {
    @string s = fmt.Sprintf("%s\nTODO syscall: function pointer"u8, debug.Stack());
    throw panic(s);
}

public static partial uintptr CreateThread(ж<TLS> t, uintptr lpThreadAttributes, typesꓸSize_t dwStackSize, uintptr lpStartAddress, uintptr lpParameter, uint32 dwCreationFlags, uintptr lpThreadId) {
    return XCreateThread(t, lpThreadAttributes, dwStackSize, lpStartAddress, lpParameter, dwCreationFlags, lpThreadId);
}

public static uintptr Xwcsncpy(ж<TLS> t, uintptr strDest, uintptr strSource, typesꓸSize_t count) {
    var (r0, _, err) = procWcsncpy.Call(strDest, strSource, (uintptr)count);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return r0;
}

public static uintptr Xwcsrchr(ж<TLS> t, uintptr str, typesꓸWchar_t c) {
    var (r0, _, err) = procWcsrchr.Call(str, (uintptr)c);
    if (!AreEqual(err, windows.NOERROR)) {
        t.setErrno(err);
    }
    return r0;
}

public static uintptr /*r*/ X_ctime64(ж<TLS> tls, uintptr __Time) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("_Time=%+v", __Time);
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc(@"X_ctime64->%+v", r);
            }, ref ᒐ);
        }
        var (r0, _, err) = procCtime64.Call(__Time);
        if (!AreEqual(err, windows.NOERROR)) {
            tls.setErrno((int32)(err._<windowsꓸErrno>()));
        }
        r = (uintptr)r0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static int64 /*r*/ X_time64(ж<TLS> tls, uintptr __Time) {
    int64 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("_Time=%+v", __Time);
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc(@"X_time64->%+v", r);
            }, ref ᒐ);
        }
        var (r0, _, err) = procTime64.Call(__Time);
        if (!AreEqual(err, windows.NOERROR)) {
            tls.setErrno((int32)(err._<windowsꓸErrno>()));
        }
        r = (int64)r0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static int32 /*r*/ X_strnicmp(ж<TLS> tls, uintptr __Str1, uintptr __Str2, typesꓸSize_t __MaxCount) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("_Str1=%+v _Str2=%+v _MaxCount=%+v", __Str1, __Str2, __MaxCount);
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc(@"X_strnicmp->%+v", r);
            }, ref ᒐ);
        }
        var (r0, _, err) = procStrnicmp.Call(__Str1, __Str2, (uintptr)__MaxCount);
        if (!AreEqual(err, windows.NOERROR)) {
            tls.setErrno((int32)(err._<windowsꓸErrno>()));
        }
        r = (int32)r0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static int32 X__builtin_ctz(ж<TLS> t, uint32 n) {
    return (int32)mbits.TrailingZeros32(n);
}

public static typesꓸIntptr_t /*r*/ X_wfindfirst64i32(ж<TLS> tls, uintptr filespec, uintptr fileinfo) {
    var (r0, _, err) = procWfindfirst64i32.Call(filespec, fileinfo);
    if (!AreEqual(err, windows.NOERROR)) {
        tls.setErrno((int32)(err._<windowsꓸErrno>()));
    }
    return (typesꓸIntptr_t)r0;
}

public static int32 /*r*/ X_wfindnext64i32(ж<TLS> tls, typesꓸIntptr_t handle, uintptr fileinfo) {
    var (r0, _, err) = procWfindnext64i32.Call((uintptr)handle, fileinfo);
    if (!AreEqual(err, windows.NOERROR)) {
        tls.setErrno((int32)(err._<windowsꓸErrno>()));
    }
    return (int32)r0;
}

public static uintptr /*r*/ X_wfullpath(ж<TLS> tls, uintptr absPath, uintptr relPath, Tsize_t maxLength) {
    var (r0, _, err) = procWfullpath.Call(absPath, relPath, (uintptr)maxLength);
    if (!AreEqual(err, windows.NOERROR)) {
        tls.setErrno((int32)(err._<windowsꓸErrno>()));
    }
    return r0;
}

public static int32 /*r*/ X_wchmod(ж<TLS> tls, uintptr filename, int32 pmode) {
    var (r0, _, err) = procWchmod.Call(filename, (uintptr)pmode);
    if (!AreEqual(err, windows.NOERROR)) {
        tls.setErrno((int32)(err._<windowsꓸErrno>()));
    }
    return (int32)r0;
}

public static int32 /*r*/ X_wmkdir(ж<TLS> tls, uintptr dirname) {
    var (r0, _, err) = procWmkdir.Call(dirname);
    if (!AreEqual(err, windows.NOERROR)) {
        tls.setErrno((int32)(err._<windowsꓸErrno>()));
    }
    return (int32)r0;
}

public static int32 /*r*/ X_wstat64i32(ж<TLS> tls, uintptr path, uintptr buffer) {
    var (r0, _, err) = procWstat64i32.Call(path, buffer);
    if (!AreEqual(err, windows.NOERROR)) {
        tls.setErrno((int32)(err._<windowsꓸErrno>()));
    }
    return (int32)r0;
}

public static typesꓸIntptr_t /*r*/ X_wfindfirst32(ж<TLS> tls, uintptr filespec, uintptr fileinfo) {
    var (r0, _, err) = procWfindfirst32.Call(filespec, fileinfo);
    if (!AreEqual(err, windows.NOERROR)) {
        tls.setErrno((int32)(err._<windowsꓸErrno>()));
    }
    return (typesꓸIntptr_t)r0;
}

public static int32 /*r*/ X_wfindnext32(ж<TLS> tls, typesꓸIntptr_t handle, uintptr fileinfo) {
    var (r0, _, err) = procWfindnext32.Call((uintptr)handle, fileinfo);
    if (!AreEqual(err, windows.NOERROR)) {
        tls.setErrno((int32)(err._<windowsꓸErrno>()));
    }
    return (int32)r0;
}

public static int32 /*r*/ X_wstat32(ж<TLS> tls, uintptr path, uintptr buffer) {
    var (r0, _, err) = procWstat32.Call(path, buffer);
    if (!AreEqual(err, windows.NOERROR)) {
        tls.setErrno((int32)(err._<windowsꓸErrno>()));
    }
    return (int32)r0;
}

public static partial uintptr Xbsearch(ж<TLS> tls, uintptr key, uintptr @base, Tsize_t nel, Tsize_t width, uintptr cmp) {
    if (__ccgo_strace) {
        trc("tls=%v key=%v base=%v nel=%v width=%v cmp=%v, (%v:)", tls.OrTypedNil(), key, @base, nel, width, cmp, origin(2));
    }
    uintptr @try = default;
    int32 sign = default;
    while (nel > 0) {
        @try = @base + (uintptr)(width * (nel / 2));
        sign = GoFuncCookie.Func<Func<ж<TLS>, uintptr, uintptr, int32>>(cmp)(tls, key, @try);
        if (sign < 0) {
            nel /= 2;
        } else if (sign > 0) {
            @base = @try + (uintptr)width;
            nel -= nel / 2 + 1;
        } else {
            return @try;
        }
    }
    return 0;
}

public static partial int32 Xkill(ж<TLS> t, typesꓸPid_t pid, int32 sig) {
    if (__ccgo_strace) {
        trc("t=%v pid=%v sig=%v, (%v:)", t.OrTypedNil(), pid, sig, origin(2));
    }
    throw panic(todo(""));
}

public static partial typesꓸSsize_t Xreadv(ж<TLS> t, int32 fd, uintptr iov, int32 iovcnt) {
    if (__ccgo_strace) {
        trc("t=%v fd=%v iov=%v iovcnt=%v, (%v:)", t.OrTypedNil(), fd, iov, iovcnt, origin(2));
    }
    throw panic(todo(""));
}

public static partial typesꓸPid_t Xsetsid(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    throw panic(todo(""));
}

public static partial typesꓸSize_t Xconfstr(ж<TLS> t, int32 name, uintptr buf, typesꓸSize_t len) {
    if (__ccgo_strace) {
        trc("t=%v name=%v buf=%v len=%v, (%v:)", t.OrTypedNil(), name, buf, len, origin(2));
    }
    throw panic(todo(""));
}

public static partial uintptr /*r*/ Xstrncat(ж<TLS> t, uintptr dest, uintptr src, typesꓸSize_t n) {
    if (__ccgo_strace) {
        trc("t=%v dest=%v src=%v n=%v, (%v:)", t.OrTypedNil(), dest, src, n, origin(2));
    }
    return strncat(dest, src, n);
}

internal static uint64 strtoi64(ж<TLS> t, uintptr nptr, uintptr endptr, int32 @base, bool unsigned) {
    var p = nptr;
    for (;;) {
        switch (~@unsafe.Pointer.To<byte>((@unsafe.Pointer)p)) {
        case (byte)' ' or (byte)'\t' or (byte)'\n' or (byte)'\v' or (byte)'\f' or (byte)'\r': {
            p++;
            continue;
            break;
        }}

        break;
    }
    var neg = false;
    switch (~@unsafe.Pointer.To<byte>((@unsafe.Pointer)p)) {
    case (byte)'-': {
        neg = true;
        p++;
        break;
    }
    case (byte)'+': {
        p++;
        break;
    }}

    if ((@base == 0 || @base == 16) && ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)p) == '0' && (~@unsafe.Pointer.To<byte>((@unsafe.Pointer)(p + 1)) == 'x' || ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)(p + 1)) == 'X')) {
        @base = 16;
        p += 2;
    }
    if (@base == 0) {
        @base = 10;
        if (~@unsafe.Pointer.To<byte>((@unsafe.Pointer)p) == '0') {
            @base = 8;
        }
    }
    if (@base < 2 || @base > 36) {
        t.setErrno((nint)(errno.EINVAL));
        return 0;
    }

    uint64 v = default;
    bool overflow = default;
    bool any = default;
    for (; ; p++) {
        var c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)p);
        int32 d = default;
        switch (ᐧ) {
        case {} when c >= (byte)'0' && c <= (byte)'9': {
            d = (int32)((byte)(c - '0'));
            break;
        }
        case {} when c >= (byte)'a' && c <= (byte)'z': {
            d = (int32)((byte)(c - 'a')) + 10;
            break;
        }
        case {} when c >= (byte)'A' && c <= (byte)'Z': {
            d = (int32)((byte)(c - 'A')) + 10;
            break;
        }
        default: {
            d = 99;
            break;
        }}

        if (d >= @base) {
            break;
        }

        any = true;
        if (!overflow) {
            var nv = v * (uint64)@base + (uint64)d;
            if (nv / (uint64)@base != v) {
                overflow = true;
            }
            v = nv;
        }
    }
    if (!any) {
        if (endptr != 0) {
            (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)endptr)).Value = nptr;
        }
        return 0;
    }

    if (endptr != 0) {
        (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)endptr)).Value = p;
    }
    switch (ᐧ) {
    case {} when unsigned: {
        if (overflow) {
            t.setErrno((nint)(errno.ERANGE));
            return math.MaxUint64;
        }
        if (neg) {
            return ((uint64)0 - v);
        }
        return v;
    }
    case {} when neg: {
        if (overflow || v > (uint64)1 << 63) {
            t.setErrno((nint)(errno.ERANGE));
            return (uint64)1 << 63;
        }
        return ((uint64)0 - v);
    }
    default: {
        if (overflow || v > math.MaxInt64) {
            t.setErrno((nint)(errno.ERANGE));
            return math.MaxInt64;
        }
        return v;
    }}

}

public static partial int64 Xstrtoll(ж<TLS> t, uintptr nptr, uintptr endptr, int32 @base) {
    if (__ccgo_strace) {
        trc("t=%v nptr=%v endptr=%v base=%v, (%v:)", t.OrTypedNil(), nptr, endptr, @base, origin(2));
    }
    return (int64)strtoi64(t, nptr, endptr, @base, false);
}

public static partial int64 X_strtoi64(ж<TLS> t, uintptr nptr, uintptr endptr, int32 @base) {
    return Xstrtoll(t, nptr, endptr, @base);
}

public static partial uint64 Xstrtoull(ж<TLS> t, uintptr nptr, uintptr endptr, int32 @base) {
    if (__ccgo_strace) {
        trc("t=%v nptr=%v endptr=%v base=%v, (%v:)", t.OrTypedNil(), nptr, endptr, @base, origin(2));
    }
    return strtoi64(t, nptr, endptr, @base, true);
}

public static partial uint64 X_strtoui64(ж<TLS> t, uintptr nptr, uintptr endptr, int32 @base) {
    return Xstrtoull(t, nptr, endptr, @base);
}

internal static int32 dup2(ж<TLS> t, int32 oldfd, int32 newfd) {
    var (f, ok) = fdToFile(oldfd);
    if (!ok) {
        t.setErrno((nint)(errno.EBADF));
        return -1;
    }

    if (oldfd == newfd) {
        return newfd;
    }

    ref var h = ref heap(new windows.Handle(), out var Ꮡh);
    var p = windows.CurrentProcess();
    var err = windows.DuplicateHandle(p, f.Handle, p, Ꮡh, 0, true, windows.DUPLICATE_SAME_ACCESS); if (err != nil) {
        t.setErrno(err);
        return -1;
    }

    var (old, okΔ1) = fdToFile(newfd); if (okΔ1) {
        remFile(ref (old).DerefOrNull());
        windows.Close(old.Handle);
    }
    addFile(h, newfd);
    for (;;) {
        var n = atomic.LoadInt32(Ꮡw_nextFd);
        if (n >= newfd || atomic.CompareAndSwapInt32(Ꮡw_nextFd, n, newfd)) {
            break;
        }
    }
    return newfd;
}

public static (windows.Handle, bool) WindowsHandle(int32 fd) {
    var (f, ok) = fdToFile(fd);
    if (!ok) {
        return (windows.InvalidHandle, false);
    }

    return (f.Handle, true);
}

public static int32 WrapWindowsHandle(windows.Handle h) {
    var (_, fd) = wrapFdHandle(h);
    return fd;
}

public static void WrapWindowsHandleAt(windows.Handle h, int32 fd) {
    var (old, ok) = fdToFile(fd); if (ok) {
        remFile(ref (old).DerefOrNull());
    }
    addFile(h, fd);
    for (;;) {
        var n = atomic.LoadInt32(Ꮡw_nextFd);
        if (n >= fd || atomic.CompareAndSwapInt32(Ꮡw_nextFd, n, fd)) {
            break;
        }
    }
}

public static (windows.Handle, bool) ReleaseWindowsHandle(int32 fd) {
    var (f, ok) = fdToFile(fd);
    if (!ok) {
        return (windows.InvalidHandle, false);
    }

    remFile(ref (f).DerefOrNull());
    return (f.Handle, true);
}

} // end libc_package
