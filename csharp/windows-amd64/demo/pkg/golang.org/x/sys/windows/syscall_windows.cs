// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using errorspkg = errors_package;
using fmt = fmt_package;
using runtime = runtime_package;
using sync = sync_package;
using syscall = syscall_package;
using time = time_package;
using utf16 = unicode.utf16_package;
using @unsafe = unsafe_package;
using FieldOffsetAttribute = global::System.Runtime.InteropServices.FieldOffsetAttribute;
using LayoutKind = global::System.Runtime.InteropServices.LayoutKind;
using StructLayoutAttribute = global::System.Runtime.InteropServices.StructLayoutAttribute;
using go.arrays;
using go.golib;
using unicode;
using static windows_package;

partial class windows_package {

partial struct Handle /*num:uintptr*/;

partial struct HWND /*num:uintptr*/;

public static Handle InvalidHandle => /* ^Handle(0) */ unchecked((Handle)18446744073709551615);
public static HWND InvalidHWND => /* ^HWND(0) */ unchecked((HWND)18446744073709551615);
public static UntypedInt DDD_EXACT_MATCH_ON_REMOVE => 0x00000004;
public static UntypedInt DDD_NO_BROADCAST_SYSTEM => 0x00000008;
public static UntypedInt DDD_RAW_TARGET_PATH => 0x00000001;
public static UntypedInt DDD_REMOVE_DEFINITION => 0x00000002;
public static UntypedInt DRIVE_UNKNOWN => 0;
public static UntypedInt DRIVE_NO_ROOT_DIR => 1;
public static UntypedInt DRIVE_REMOVABLE => 2;
public static UntypedInt DRIVE_FIXED => 3;
public static UntypedInt DRIVE_REMOTE => 4;
public static UntypedInt DRIVE_CDROM => 5;
public static UntypedInt DRIVE_RAMDISK => 6;
public static UntypedInt FILE_CASE_SENSITIVE_SEARCH => 0x00000001;
public static UntypedInt FILE_CASE_PRESERVED_NAMES => 0x00000002;
public static UntypedInt FILE_FILE_COMPRESSION => 0x00000010;
public static UntypedInt FILE_DAX_VOLUME => 0x20000000;
public static UntypedInt FILE_NAMED_STREAMS => 0x00040000;
public static UntypedInt FILE_PERSISTENT_ACLS => 0x00000008;
public static UntypedInt FILE_READ_ONLY_VOLUME => 0x00080000;
public static UntypedInt FILE_SEQUENTIAL_WRITE_ONCE => 0x00100000;
public static UntypedInt FILE_SUPPORTS_ENCRYPTION => 0x00020000;
public static UntypedInt FILE_SUPPORTS_EXTENDED_ATTRIBUTES => 0x00800000;
public static UntypedInt FILE_SUPPORTS_HARD_LINKS => 0x00400000;
public static UntypedInt FILE_SUPPORTS_OBJECT_IDS => 0x00010000;
public static UntypedInt FILE_SUPPORTS_OPEN_BY_FILE_ID => 0x01000000;
public static UntypedInt FILE_SUPPORTS_REPARSE_POINTS => 0x00000080;
public static UntypedInt FILE_SUPPORTS_SPARSE_FILES => 0x00000040;
public static UntypedInt FILE_SUPPORTS_TRANSACTIONS => 0x00200000;
public static UntypedInt FILE_SUPPORTS_USN_JOURNAL => 0x02000000;
public static UntypedInt FILE_UNICODE_ON_DISK => 0x00000004;
public static UntypedInt FILE_VOLUME_IS_COMPRESSED => 0x00008000;
public static UntypedInt FILE_VOLUME_QUOTAS => 0x00000020;
public static UntypedInt LOCKFILE_FAIL_IMMEDIATELY => 0x00000001;
public static UntypedInt LOCKFILE_EXCLUSIVE_LOCK => 0x00000002;
public static UntypedInt WAIT_IO_COMPLETION => 0x000000C0;

public static slice<uint16> StringToUTF16(@string s) {
    var (a, err) = UTF16FromString(s);
    if (err != nil) {
        throw panic("windows: string with NUL passed to StringToUTF16");
    }
    return a;
}

public static (slice<uint16>, error) UTF16FromString(@string s) {
    return syscall.UTF16FromString(s);
}

public static @string UTF16ToString(slice<uint16> s) {
    return syscall.UTF16ToString(s);
}

public static ж<uint16> StringToUTF16Ptr(@string s) { return Ꮡ(StringToUTF16(s), 0); }

public static (ж<uint16>, error) UTF16PtrFromString(@string s) {
    var (a, err) = UTF16FromString(s);
    if (err != nil) {
        return (default, err);
    }
    return (Ꮡ(a, 0), default);
}

public static @string UTF16PtrToString(ж<uint16> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (Ꮡp == nil) {
        return "";
    }
    if (p == 0) {
        return "";
    }

    nint n = 0;
    for (@unsafe.Pointer ptr = @unsafe.Pointer.FromPinnedBox(Ꮡp); ~@unsafe.Pointer.To<uint16>(ptr) != 0; n++) {
        ptr = @unsafe.Add(ptr, /* unsafe.Sizeof(*p) */ (uintptr)2);
    }
    return UTF16ToString(@unsafe.Slice(Ꮡp, n));
}

public static nint Getpagesize() { return 4096; }

public static uintptr NewCallback(any fn) {
    return syscall.NewCallback(fn);
}

public static uintptr NewCallbackCDecl(any fn) {
    return syscall.NewCallbackCDecl(fn);
}

public static (Handle, error) GetCurrentProcess() {
    return (CurrentProcess(), default);
}

public static Handle CurrentProcess() { return (Handle)(~(uintptr)(1 - 1)); }

public static (Handle, error) GetCurrentThread() {
    return (CurrentThread(), default);
}

public static Handle CurrentThread() { return (Handle)(~(uintptr)(2 - 1)); }

public static (uintptr proc, error err) GetProcAddressByOrdinal(Handle module, uintptr ordinal) {
    uintptr proc = default;
    error err = default;

    var (r0, _, e1) = syscall.Syscall(procGetProcAddress.Addr(), 2, (uintptr)module, ordinal, 0);
    proc = (uintptr)r0;
    if (proc == 0) {
        err = errnoErr(e1);
    }
    return (proc, err);
}

public static void Exit(nint code) { ExitProcess((uint32)code); }

internal static ж<SecurityAttributes> makeInheritSa() {
    ref var sa = ref heap(new SecurityAttributes(), out var Ꮡsa);
    sa.Length = (uint32)/* unsafe.Sizeof(sa) */ (uintptr)24;
    sa.InheritHandle = 1;
    return Ꮡsa;
}

public static (Handle fd, error err) Open(@string path, nint mode, uint32 perm) {
    error err = default;

    if (len(path) == 0) {
        return (InvalidHandle, ERROR_FILE_NOT_FOUND);
    }
    (var pathp, err) = UTF16PtrFromString(path);
    if (err != nil) {
        return (InvalidHandle, err);
    }
    uint32 access = default;
    switch ((mode & (nint)((nint)((nint)(UntypedInt)(O_RDONLY | O_WRONLY) | (nint)O_RDWR)))) {
    case var ᴛ when ᴛ == O_RDONLY: {
        access = GENERIC_READ;
        break;
    }
    case var ᴛ when ᴛ == O_WRONLY: {
        access = GENERIC_WRITE;
        break;
    }
    case var ᴛ when ᴛ == O_RDWR: {
        access = (uint32)((uint32)GENERIC_READ | (uint32)GENERIC_WRITE);
        break;
    }}

    if ((mode & (nint)O_CREAT) != 0) {
        access |= (uint32)(GENERIC_WRITE);
    }
    if ((mode & (nint)O_APPEND) != 0) {
        access &= unchecked((uint32)~(uint32)(GENERIC_WRITE));
        access |= (uint32)(FILE_APPEND_DATA);
    }
    var sharemode = (uint32)((uint32)((uint32)FILE_SHARE_READ | (uint32)FILE_SHARE_WRITE));
    ж<SecurityAttributes> sa = default;
    if ((mode & (nint)O_CLOEXEC) == 0) {
        sa = makeInheritSa();
    }
    uint32 createmode = default;
    switch (ᐧ) {
    case {} when (mode & (nint)((nint)((nint)O_CREAT | (nint)O_EXCL))) == (nint)((nint)((nint)O_CREAT | (nint)O_EXCL)): {
        createmode = CREATE_NEW;
        break;
    }
    case {} when (mode & (nint)((nint)((nint)O_CREAT | (nint)O_TRUNC))) == (nint)((nint)((nint)O_CREAT | (nint)O_TRUNC)): {
        createmode = CREATE_ALWAYS;
        break;
    }
    case {} when (mode & (nint)O_CREAT) == O_CREAT: {
        createmode = OPEN_ALWAYS;
        break;
    }
    case {} when (mode & (nint)O_TRUNC) == O_TRUNC: {
        createmode = TRUNCATE_EXISTING;
        break;
    }
    default: {
        createmode = OPEN_EXISTING;
        break;
    }}

    uint32 attrs = FILE_ATTRIBUTE_NORMAL;
    if ((perm & (uint32)S_IWRITE) == 0) {
        attrs = FILE_ATTRIBUTE_READONLY;
    }
    var (h, e) = CreateFile(pathp, access, sharemode, sa, createmode, attrs, 0);
    return (h, e);
}

public static (nint n, error err) Read(Handle fd, slice<byte> p) {
    ref var done = ref heap(new uint32(), out var Ꮡdone);
    var e = ReadFile(fd, p, Ꮡdone, nil);
    if (e != nil) {
        if (AreEqual(e, ERROR_BROKEN_PIPE)) {
            return (0, default);
        }
        return (0, e);
    }
    return ((nint)done, default);
}

public static (nint n, error err) Write(Handle fd, slice<byte> p) {
    if (raceenabled) {
        raceReleaseMerge(@unsafe.Pointer.FromPinnedBox(ᏑioSync));
    }
    ref var done = ref heap(new uint32(), out var Ꮡdone);
    var e = WriteFile(fd, p, Ꮡdone, nil);
    if (e != nil) {
        return (0, e);
    }
    return ((nint)done, default);
}

public static error ReadFile(Handle fd, slice<byte> p, ж<uint32> Ꮡdone, ж<Overlapped> overlapped) {
    ref var done = ref Ꮡdone.DerefOrNull();

    var err = readFile(fd, p, Ꮡdone, overlapped);
    if (raceenabled) {
        if (done > 0) {
            raceWriteRange(@unsafe.Pointer.FromPinnedBox(Ꮡ(p, 0)), (nint)(done));
        }
        raceAcquire(@unsafe.Pointer.FromPinnedBox(ᏑioSync));
    }
    return err;
}

public static error WriteFile(Handle fd, slice<byte> p, ж<uint32> Ꮡdone, ж<Overlapped> overlapped) {
    ref var done = ref Ꮡdone.DerefOrNull();

    if (raceenabled) {
        raceReleaseMerge(@unsafe.Pointer.FromPinnedBox(ᏑioSync));
    }
    var err = writeFile(fd, p, Ꮡdone, overlapped);
    if (raceenabled && done > 0) {
        raceReadRange(@unsafe.Pointer.FromPinnedBox(Ꮡ(p, 0)), (nint)(done));
    }
    return err;
}

internal static readonly ж<int64> ᏑioSync = new StaticBox<int64>(default(int64));
internal static ref int64 ioSync => ref ((StaticBox<int64>)ᏑioSync).ValueSlot;

public static (int64 newoffset, error err) Seek(Handle fd, int64 offset, nint whence) {
    uint32 w = default;
    switch (whence) {
    case 0: {
        w = FILE_BEGIN;
        break;
    }
    case 1: {
        w = FILE_CURRENT;
        break;
    }
    case 2: {
        w = FILE_END;
        break;
    }}

    ref var hi = ref heap<int32>((int32)(offset >> 32), out var Ꮡhi);
    var lo = (int32)offset;
    var (ft, _) = GetFileType(fd);
    if (ft == FILE_TYPE_PIPE) {
        return (0, syscall.EPIPE);
    }
    var (rlo, e) = SetFilePointer(fd, lo, Ꮡhi, w);
    if (e != nil) {
        return (0, e);
    }
    return (((int64)hi << 32) + (int64)rlo, default);
}

public static error /*err*/ Close(Handle fd) {
    return CloseHandle(fd);
}

public static readonly ж<Handle> ᏑStdin = new StaticBox<Handle>(default(Handle));
public static ref Handle Stdin => ref ((StaticBox<Handle>)ᏑStdin).ValueSlot;
internal static void initᴛStdin() { Stdin = getStdHandle(STD_INPUT_HANDLE); }
public static readonly ж<Handle> ᏑStdout = new StaticBox<Handle>(default(Handle));
public static ref Handle Stdout => ref ((StaticBox<Handle>)ᏑStdout).ValueSlot;
internal static void initᴛStdout() { Stdout = getStdHandle(STD_OUTPUT_HANDLE); }
public static readonly ж<Handle> ᏑStderr = new StaticBox<Handle>(default(Handle));
public static ref Handle Stderr => ref ((StaticBox<Handle>)ᏑStderr).ValueSlot;
internal static void initᴛStderr() { Stderr = getStdHandle(STD_ERROR_HANDLE); }

internal static Handle /*fd*/ getStdHandle(uint32 stdhandle) {
    var (r, _) = GetStdHandle(stdhandle);
    return r;
}

public const bool ImplementsGetwd = true;

public static (@string wd, error err) Getwd() {
    var b = new slice<uint16>(300);
    var (n, e) = GetCurrentDirectory((uint32)len(b), Ꮡ(b, 0));
    if (e != nil) {
        return ("", e);
    }
    return ((@string)utf16.Decode(b.slice(0, (nint)(n))), default);
}

public static error /*err*/ Chdir(@string path) {
    error err = default;

    (var pathp, err) = UTF16PtrFromString(path);
    if (err != nil) {
        return err;
    }
    return SetCurrentDirectory(pathp);
}

public static error /*err*/ Mkdir(@string path, uint32 mode) {
    error err = default;

    (var pathp, err) = UTF16PtrFromString(path);
    if (err != nil) {
        return err;
    }
    return CreateDirectory(pathp, nil);
}

public static error /*err*/ Rmdir(@string path) {
    error err = default;

    (var pathp, err) = UTF16PtrFromString(path);
    if (err != nil) {
        return err;
    }
    return RemoveDirectory(pathp);
}

public static error /*err*/ Unlink(@string path) {
    error err = default;

    (var pathp, err) = UTF16PtrFromString(path);
    if (err != nil) {
        return err;
    }
    return DeleteFile(pathp);
}

public static error /*err*/ Rename(@string oldpath, @string newpath) {
    error err = default;

    (var from, err) = UTF16PtrFromString(oldpath);
    if (err != nil) {
        return err;
    }
    (var to, err) = UTF16PtrFromString(newpath);
    if (err != nil) {
        return err;
    }
    return MoveFileEx(from, to, MOVEFILE_REPLACE_EXISTING);
}

public static (@string name, error err) ComputerName() {
    ref var n = ref heap(new uint32(), out var Ꮡn);
    n = MAX_COMPUTERNAME_LENGTH + 1;
    var b = new slice<uint16>((nint)(n));
    var e = GetComputerName(Ꮡ(b, 0), Ꮡn);
    if (e != nil) {
        return ("", e);
    }
    return ((@string)utf16.Decode(b.slice(0, (nint)(n))), default);
}

public static time.Duration DurationSinceBoot() {
    return ((time.Duration)(int64)getTickCount64()) * time.Millisecond;
}

internal partial struct Ftruncate__FILE_END_OF_FILE_INFO /*dyn*/ {
    public int64 EndOfFile;
}

public static error /*err*/ Ftruncate(Handle fd, int64 length) {
    ref var info = ref heap(new Ftruncate__FILE_END_OF_FILE_INFO(), out var Ꮡinfo);
    info.EndOfFile = length;
    return SetFileInformationByHandle(fd, FileEndOfFileInfo, Ꮡinfo.Reinterpret<Ftruncate__FILE_END_OF_FILE_INFO, byte>(), (uint32)/* unsafe.Sizeof(info) */ (uintptr)8);
}

public static error /*err*/ Gettimeofday(ж<Timeval> Ꮡtv) {
    ref var tv = ref Ꮡtv.DerefOrNull();

    ref var ft = ref heap(new Filetime(), out var Ꮡft);
    GetSystemTimeAsFileTime(Ꮡft);
    tv = NsecToTimeval(ft.Nanoseconds());
    return default;
}

public static error /*err*/ Pipe(slice<Handle> p) {
    if (len(p) != 2) {
        return syscall.EINVAL;
    }
    ref var r = ref heap(new Handle(), out var Ꮡr);
    ref var w = ref heap(new Handle(), out var Ꮡw);
    var e = CreatePipe(Ꮡr, Ꮡw, makeInheritSa(), 0);
    if (e != nil) {
        return e;
    }
    p[0] = r;
    p[1] = w;
    return default;
}

public static error /*err*/ Utimes(@string path, slice<Timeval> tv) {
    error err = default;
    GoFrame ᒐ = default;
    try {
        if (len(tv) != 2) {
            err = syscall.EINVAL; goto ᒐdone;
        }
        var (pathp, e) = UTF16PtrFromString(path);
        if (e != nil) {
            err = e; goto ᒐdone;
        }
        (var h, e) = CreateFile(pathp,
            FILE_WRITE_ATTRIBUTES, FILE_SHARE_WRITE, nil,
            OPEN_EXISTING, FILE_FLAG_BACKUP_SEMANTICS, 0);
        if (e != nil) {
            err = e; goto ᒐdone;
        }
        defer(CloseHandle, h, ref ᒐ);
        ref var a = ref heap<Filetime>(NsecToFiletime(tv[0].Nanoseconds()), out var Ꮡa);
        ref var w = ref heap<Filetime>(NsecToFiletime(tv[1].Nanoseconds()), out var Ꮡw);
        err = SetFileTime(h, nil, Ꮡa, Ꮡw);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return err;
}

public static error /*err*/ UtimesNano(@string path, slice<Timespec> ts) {
    error err = default;
    GoFrame ᒐ = default;
    try {
        if (len(ts) != 2) {
            err = syscall.EINVAL; goto ᒐdone;
        }
        var (pathp, e) = UTF16PtrFromString(path);
        if (e != nil) {
            err = e; goto ᒐdone;
        }
        (var h, e) = CreateFile(pathp,
            FILE_WRITE_ATTRIBUTES, FILE_SHARE_WRITE, nil,
            OPEN_EXISTING, FILE_FLAG_BACKUP_SEMANTICS, 0);
        if (e != nil) {
            err = e; goto ᒐdone;
        }
        defer(CloseHandle, h, ref ᒐ);
        ref var a = ref heap<Filetime>(NsecToFiletime(TimespecToNsec(ts[0])), out var Ꮡa);
        ref var w = ref heap<Filetime>(NsecToFiletime(TimespecToNsec(ts[1])), out var Ꮡw);
        err = SetFileTime(h, nil, Ꮡa, Ꮡw);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return err;
}

public static error /*err*/ Fsync(Handle fd) {
    return FlushFileBuffers(fd);
}

public static error /*err*/ Chmod(@string path, uint32 mode) {
    var (p, e) = UTF16PtrFromString(path);
    if (e != nil) {
        return e;
    }
    (var attrs, e) = GetFileAttributes(p);
    if (e != nil) {
        return e;
    }
    if ((mode & (uint32)S_IWRITE) != 0) {
        attrs &= unchecked((uint32)~(uint32)(FILE_ATTRIBUTE_READONLY));
    } else {
        attrs |= (uint32)(FILE_ATTRIBUTE_READONLY);
    }
    return SetFileAttributes(p, attrs);
}

public static error LoadGetSystemTimePreciseAsFileTime() {
    return procGetSystemTimePreciseAsFileTime.Find();
}

public static error LoadCancelIoEx() {
    return procCancelIoEx.Find();
}

public static error LoadSetFileCompletionNotificationModes() {
    return procSetFileCompletionNotificationModes.Find();
}

public static (uint32 @event, error err) WaitForMultipleObjects(slice<Handle> handles, bool waitAll, uint32 waitMilliseconds) {
    ж<Handle> handlePtr = default;
    if (len(handles) > 0) {
        handlePtr = Ꮡ(handles, 0);
    }
    return waitForMultipleObjects((uint32)len(handles), (uintptr)@unsafe.Pointer.FromBox(handlePtr), waitAll, waitMilliseconds);
}

internal static uintptr socket_error => /* uintptr(^uint32(0)) */ unchecked((uintptr)4294967295);

public static readonly ж<bool> ᏑSocketDisableIPv6 = new StaticBox<bool>(default(bool));
public static ref bool SocketDisableIPv6 => ref ((StaticBox<bool>)ᏑSocketDisableIPv6).ValueSlot;

partial struct RawSockaddrInet4 {
    public uint16 Family;
    public uint16 Port;
    public array<byte> Addr = new(4);
    public array<uint8> Zero = new(8);
}

partial struct RawSockaddrInet6 {
    public uint16 Family;
    public uint16 Port;
    public uint32 Flowinfo;
    public array<byte> Addr = new(16);
    public uint32 Scope_id;
}

[global::go.golib.GoInlineLayout(Size = 28, Align = 4), StructLayout(LayoutKind.Explicit, Size = 28)] partial struct RawSockaddrInet {
    [FieldOffset(0)] public uint16 Family;
    [FieldOffset(2)] public uint16 Port;
    [FieldOffset(4)] public array6<uint32> Data;
}

[global::go.golib.GoInlineLayout(Size = 16, Align = 2), StructLayout(LayoutKind.Explicit, Size = 16)] partial struct RawSockaddr {
    [FieldOffset(0)] public uint16 Family;
    [FieldOffset(2)] public array14<int8> Data;
}

partial struct RawSockaddrAny {
    public RawSockaddr Addr;
    public array<int8> Pad = new(100);
}

partial interface Sockaddr {
    (@unsafe.Pointer ptr, int32 len, error err) sockaddr();
}

partial struct SockaddrInet4 {
    public nint Port;
    public array<byte> Addr = new(4);
    internal RawSockaddrInet4 raw;
}

} partial class windowsᴛSockaddrInet4 {

internal static (@unsafe.Pointer, int32, error) sockaddr(this ж<SockaddrInet4> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    if (sa.Port < 0 || sa.Port > 0xFFFF) {
        return (default, 0, syscall.EINVAL);
    }
    sa.raw.Family = AF_INET;
    var p = @unsafe.ArrayPointer<byte>.Of(Ꮡsa.Ꮡraw.ᏑPort, 2);
    p.ElementRef(0) = (byte)(sa.Port >> 8);
    p.ElementRef(1) = (byte)sa.Port;
    sa.raw.Addr.Assign(sa.Addr);
    return (@unsafe.Pointer.FromPinnedBox(Ꮡsa.Ꮡraw), (int32)/* unsafe.Sizeof(sa.raw) */ (uintptr)16, default);
}

} partial class windows_package {

partial struct SockaddrInet6 {
    public nint Port;
    public uint32 ZoneId;
    public array<byte> Addr = new(16);
    internal RawSockaddrInet6 raw;
}

} partial class windowsᴛSockaddrInet6 {

internal static (@unsafe.Pointer, int32, error) sockaddr(this ж<SockaddrInet6> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    if (sa.Port < 0 || sa.Port > 0xFFFF) {
        return (default, 0, syscall.EINVAL);
    }
    sa.raw.Family = AF_INET6;
    var p = @unsafe.ArrayPointer<byte>.Of(Ꮡsa.Ꮡraw.ᏑPort, 2);
    p.ElementRef(0) = (byte)(sa.Port >> 8);
    p.ElementRef(1) = (byte)sa.Port;
    sa.raw.Scope_id = sa.ZoneId;
    sa.raw.Addr.Assign(sa.Addr);
    return (@unsafe.Pointer.FromPinnedBox(Ꮡsa.Ꮡraw), (int32)/* unsafe.Sizeof(sa.raw) */ (uintptr)28, default);
}

} partial class windows_package {

partial struct RawSockaddrUnix {
    public uint16 Family;
    public array<int8> Path = new(UNIX_PATH_MAX);
}

partial struct SockaddrUnix {
    public @string Name;
    internal RawSockaddrUnix raw;
}

} partial class windowsᴛSockaddrUnix {

internal static (@unsafe.Pointer, int32, error) sockaddr(this ж<SockaddrUnix> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    @string name = sa.Name;
    nint n = len(name);
    if (n > len(sa.raw.Path)) {
        return (default, 0, syscall.EINVAL);
    }
    if (n == len(sa.raw.Path) && name[0] != '@') {
        return (default, 0, syscall.EINVAL);
    }
    sa.raw.Family = AF_UNIX;
    for (nint i = 0; i < n; i++) {
        sa.raw.Path[i] = (int8)name[i];
    }
    var sl = (int32)2;
    if (n > 0) {
        sl += (int32)n + 1;
    }
    if (sa.raw.Path[0] == '@' || (sa.raw.Path[0] == 0 && sl > 3)) {
        sa.raw.Path[0] = 0;
        sl--;
    }

    return (@unsafe.Pointer.FromPinnedBox(Ꮡsa.Ꮡraw), sl, default);
}

} partial class windows_package {

partial struct RawSockaddrBth {
    public array<byte> AddressFamily = new(2);
    public array<byte> BtAddr = new(8);
    public array<byte> ServiceClassId = new(16);
    public array<byte> Port = new(4);
}

partial struct SockaddrBth {
    public uint64 BtAddr;
    public GUID ServiceClassId;
    public uint32 Port;
    internal RawSockaddrBth raw;
}

} partial class windowsᴛSockaddrBth {

internal static (@unsafe.Pointer, int32, error) sockaddr(this ж<SockaddrBth> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    ref var family = ref heap<nint>(AF_BTH, out var Ꮡfamily);
    sa.raw.AssignFrom(new RawSockaddrBth(
        AddressFamily: (~@unsafe.Pointer.To<array<byte>>(@unsafe.Pointer.FromPinnedBox(Ꮡfamily))).Clone(),
        BtAddr: @unsafe.ArrayPointer<byte>.Load(@unsafe.ArrayPointer<byte>.Of(Ꮡsa.ᏑBtAddr, 8)),
        Port: @unsafe.ArrayPointer<byte>.Load(@unsafe.ArrayPointer<byte>.Of(Ꮡsa.ᏑPort, 4)),
        ServiceClassId: @unsafe.ArrayPointer<byte>.Load(@unsafe.ArrayPointer<byte>.Of(Ꮡsa.ᏑServiceClassId, 16))
    ));
    return (@unsafe.Pointer.FromPinnedBox(Ꮡsa.Ꮡraw), (int32)/* unsafe.Sizeof(sa.raw) */ (uintptr)30, default);
}

} partial class windowsᴛRawSockaddrAny {

public static (Sockaddr, error) Sockaddr(this ж<RawSockaddrAny> Ꮡrsa) {
    ref var rsa = ref Ꮡrsa.DerefOrNull();

    switch (rsa.Addr.Family) {
    case var ᴛ when ᴛ == AF_UNIX: {
        var pp = Ꮡrsa.Reinterpret<RawSockaddrAny, RawSockaddrUnix>();
        var sa = @new<SockaddrUnix>();
        if (pp.of(RawSockaddrUnix.ᏑPath).ElementRef(0) == 0) {
            pp.of(RawSockaddrUnix.ᏑPath).ElementRef(0) = (int8)'@';
        }

        nint n = 0;
        while (n < len(pp.Path) && pp.of(RawSockaddrUnix.ᏑPath).ElementRef(n) != 0) {
            n++;
        }
        sa.Name = (@string)@unsafe.Slice(pp.at(RawSockaddrUnix.ᏑPath, 0).Reinterpret<int8, byte>(), n);
        return (new SockaddrUnixжSockaddr(sa), default);
    }
    case var ᴛ when ᴛ == AF_INET: {
        var pp = Ꮡrsa.Reinterpret<RawSockaddrAny, RawSockaddrInet4>();
        var sa = @new<SockaddrInet4>();
        var p = @unsafe.ArrayPointer<byte>.Of(pp.ᏑPort, 2);
        sa.Port = ((nint)p.ElementRef(0) << 8) + (nint)p.ElementRef(1);
        sa.Addr.Assign(pp.Addr);
        return (new SockaddrInet4жSockaddr(sa), default);
    }
    case var ᴛ when ᴛ == AF_INET6: {
        var pp = Ꮡrsa.Reinterpret<RawSockaddrAny, RawSockaddrInet6>();
        var sa = @new<SockaddrInet6>();
        var p = @unsafe.ArrayPointer<byte>.Of(pp.ᏑPort, 2);
        sa.Port = ((nint)p.ElementRef(0) << 8) + (nint)p.ElementRef(1);
        sa.ZoneId = pp.Scope_id;
        sa.Addr.Assign(pp.Addr);
        return (new SockaddrInet6жSockaddr(sa), default);
    }}

    return (default, syscall.EAFNOSUPPORT);
}

} partial class windows_package {

public static (Handle fd, error err) Socket(nint domain, nint typ, nint proto) {
    if (domain == AF_INET6 && SocketDisableIPv6) {
        return (InvalidHandle, syscall.EAFNOSUPPORT);
    }
    return socket((int32)domain, (int32)typ, (int32)proto);
}

public static error /*err*/ SetsockoptInt(Handle fd, nint level, nint opt, nint value) {
    ref var v = ref heap<int32>((int32)value, out var Ꮡv);
    return Setsockopt(fd, (int32)level, (int32)opt, Ꮡv.Reinterpret<int32, byte>(), (int32)/* unsafe.Sizeof(v) */ (uintptr)4);
}

public static error /*err*/ Bind(Handle fd, Sockaddr sa) {
    error err = default;

    (var ptr, var n, err) = sa.sockaddr();
    if (err != nil) {
        return err;
    }
    return bind(fd, ptr, n);
}

public static error /*err*/ Connect(Handle fd, Sockaddr sa) {
    error err = default;

    (var ptr, var n, err) = sa.sockaddr();
    if (err != nil) {
        return err;
    }
    return connect(fd, ptr, n);
}

public static error /*err*/ GetBestInterfaceEx(Sockaddr sa, ж<uint32> pdwBestIfIndex) {
    error err = default;

    (var ptr, _, err) = sa.sockaddr();
    if (err != nil) {
        return err;
    }
    return getBestInterfaceEx(ptr, pdwBestIfIndex);
}

public static (Sockaddr sa, error err) Getsockname(Handle fd) {
    Sockaddr sa = default;
    error err = default;

    ref var rsa = ref heap(new RawSockaddrAny(), out var Ꮡrsa);
    ref var l = ref heap<int32>((int32)/* unsafe.Sizeof(rsa) */ (uintptr)116, out var Ꮡl);
    err = getsockname(fd, Ꮡrsa, Ꮡl); if (err != nil) {
        return (sa, err);
    }
    return Ꮡrsa.Sockaddr();
}

public static (Sockaddr sa, error err) Getpeername(Handle fd) {
    Sockaddr sa = default;
    error err = default;

    ref var rsa = ref heap(new RawSockaddrAny(), out var Ꮡrsa);
    ref var l = ref heap<int32>((int32)/* unsafe.Sizeof(rsa) */ (uintptr)116, out var Ꮡl);
    err = getpeername(fd, Ꮡrsa, Ꮡl); if (err != nil) {
        return (sa, err);
    }
    return Ꮡrsa.Sockaddr();
}

public static error /*err*/ Listen(Handle s, nint n) {
    return listen(s, (int32)n);
}

public static error /*err*/ Shutdown(Handle fd, nint how) {
    return shutdown(fd, (int32)how);
}

public static error /*err*/ WSASendto(Handle s, ж<WSABuf> bufs, uint32 bufcnt, ж<uint32> sent, uint32 flags, Sockaddr to, ж<Overlapped> overlapped, ж<byte> croutine) {
    error err = default;

    @unsafe.Pointer rsa = default;
    int32 l = default;
    if (to != nil) {
        (rsa, l, err) = to.sockaddr();
        if (err != nil) {
            return err;
        }
    }
    return WSASendTo(s, bufs, bufcnt, sent, flags, @unsafe.Pointer.To<RawSockaddrAny>((@unsafe.Pointer)rsa), l, overlapped, croutine);
}

public static error LoadGetAddrInfo() {
    return procGetAddrInfoW.Find();
}


partial struct connectExFuncᴛ1 /*dyn*/ {
    internal sync.Once once;
    internal uintptr addr;
    internal error err;
}
internal static readonly ж<connectExFuncᴛ1> ᏑconnectExFunc = new StaticBox<connectExFuncᴛ1>(default(connectExFuncᴛ1));
internal static ref connectExFuncᴛ1 connectExFunc => ref ((StaticBox<connectExFuncᴛ1>)ᏑconnectExFunc).ValueSlot;

public static error LoadConnectEx() {
    ᏑconnectExFunc.of(connectExFuncᴛ1.Ꮡonce).Do(() => {
        GoFrame ᒐ = default;
        try {
            Handle s = default;
            (s, connectExFunc.err) = Socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
            if (connectExFunc.err != nil) {
                return;
            }
            defer(CloseHandle, s, ref ᒐ);
            ref var n = ref heap(new uint32(), out var Ꮡn);
            connectExFunc.err = WSAIoctl(s,
                SIO_GET_EXTENSION_FUNCTION_POINTER,
                ᏑWSAID_CONNECTEX.Reinterpret<GUID, byte>(),
                (uint32)/* unsafe.Sizeof(WSAID_CONNECTEX) */ (uintptr)16,
                ᏑconnectExFunc.of(connectExFuncᴛ1.Ꮡaddr).Reinterpret<uintptr, byte>(),
                (uint32)/* unsafe.Sizeof(connectExFunc.addr) */ (uintptr)8,
                Ꮡn, nil, 0);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    return connectExFunc.err;
}

internal static error /*err*/ connectEx(Handle s, @unsafe.Pointer name, int32 namelen, ж<byte> sendBuf, uint32 sendDataLen, ж<uint32> bytesSent, ж<Overlapped> overlapped) {
    error err = default;

    var ᴋ0 = name;
    var ᴋ1 = sendBuf;
    var ᴋ2 = bytesSent;
    var ᴋ3 = overlapped;
        var (r1, _, e1) = syscall.Syscall9(connectExFunc.addr, 7, (uintptr)s, (uintptr)ᴋ0, (uintptr)namelen, (uintptr)ᴋ1, (uintptr)sendDataLen, (uintptr)ᴋ2, (uintptr)ᴋ3, 0, 0);
    System.GC.KeepAlive(ᴋ0);
    System.GC.KeepAlive(ᴋ1);
    System.GC.KeepAlive(ᴋ2);
    System.GC.KeepAlive(ᴋ3);
    if (r1 == 0) {
        if (e1 != 0) {
            err = ((error)e1);
        } else {
            err = syscall.EINVAL;
        }
    }
    return err;
}

public static error ConnectEx(Handle fd, Sockaddr sa, ж<byte> sendBuf, uint32 sendDataLen, ж<uint32> bytesSent, ж<Overlapped> overlapped) {
    var err = LoadConnectEx();
    if (err != nil) {
        return errorspkg.New("failed to find ConnectEx: "u8 + err.Error());
    }
    (var ptr, var n, err) = sa.sockaddr();
    if (err != nil) {
        return err;
    }
    return connectEx(fd, ptr, n, sendBuf, sendDataLen, bytesSent, overlapped);
}


partial struct sendRecvMsgFuncᴛ1 /*dyn*/ {
    internal sync.Once once;
    internal uintptr sendAddr;
    internal uintptr recvAddr;
    internal error err;
}
internal static readonly ж<sendRecvMsgFuncᴛ1> ᏑsendRecvMsgFunc = new StaticBox<sendRecvMsgFuncᴛ1>(default(sendRecvMsgFuncᴛ1));
internal static ref sendRecvMsgFuncᴛ1 sendRecvMsgFunc => ref ((StaticBox<sendRecvMsgFuncᴛ1>)ᏑsendRecvMsgFunc).ValueSlot;

internal static error loadWSASendRecvMsg() {
    ᏑsendRecvMsgFunc.of(sendRecvMsgFuncᴛ1.Ꮡonce).Do(() => {
        GoFrame ᒐ = default;
        try {
            Handle s = default;
            (s, sendRecvMsgFunc.err) = Socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
            if (sendRecvMsgFunc.err != nil) {
                return;
            }
            defer(CloseHandle, s, ref ᒐ);
            ref var n = ref heap(new uint32(), out var Ꮡn);
            sendRecvMsgFunc.err = WSAIoctl(s,
                SIO_GET_EXTENSION_FUNCTION_POINTER,
                ᏑWSAID_WSARECVMSG.Reinterpret<GUID, byte>(),
                (uint32)/* unsafe.Sizeof(WSAID_WSARECVMSG) */ (uintptr)16,
                ᏑsendRecvMsgFunc.of(sendRecvMsgFuncᴛ1.ᏑrecvAddr).Reinterpret<uintptr, byte>(),
                (uint32)/* unsafe.Sizeof(sendRecvMsgFunc.recvAddr) */ (uintptr)8,
                Ꮡn, nil, 0);
            if (sendRecvMsgFunc.err != nil) {
                return;
            }
            sendRecvMsgFunc.err = WSAIoctl(s,
                SIO_GET_EXTENSION_FUNCTION_POINTER,
                ᏑWSAID_WSASENDMSG.Reinterpret<GUID, byte>(),
                (uint32)/* unsafe.Sizeof(WSAID_WSASENDMSG) */ (uintptr)16,
                ᏑsendRecvMsgFunc.of(sendRecvMsgFuncᴛ1.ᏑsendAddr).Reinterpret<uintptr, byte>(),
                (uint32)/* unsafe.Sizeof(sendRecvMsgFunc.sendAddr) */ (uintptr)8,
                Ꮡn, nil, 0);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    return sendRecvMsgFunc.err;
}

public static error WSASendMsg(Handle fd, ж<WSAMsg> msg, uint32 flags, ж<uint32> bytesSent, ж<Overlapped> overlapped, ж<byte> croutine) {
    var err = loadWSASendRecvMsg();
    if (err != nil) {
        return err;
    }
    var ᴋ4 = msg;
    var ᴋ5 = bytesSent;
    var ᴋ6 = overlapped;
    var ᴋ7 = croutine;
        var (r1, _, e1) = syscall.Syscall6(sendRecvMsgFunc.sendAddr, 6, (uintptr)fd, (uintptr)ᴋ4, (uintptr)flags, (uintptr)ᴋ5, (uintptr)ᴋ6, (uintptr)ᴋ7);
    System.GC.KeepAlive(ᴋ4);
    System.GC.KeepAlive(ᴋ5);
    System.GC.KeepAlive(ᴋ6);
    System.GC.KeepAlive(ᴋ7);
    if (r1 == socket_error) {
        err = errnoErr(e1);
    }
    return err;
}

public static error WSARecvMsg(Handle fd, ж<WSAMsg> msg, ж<uint32> bytesReceived, ж<Overlapped> overlapped, ж<byte> croutine) {
    var err = loadWSASendRecvMsg();
    if (err != nil) {
        return err;
    }
    var ᴋ8 = msg;
    var ᴋ9 = bytesReceived;
    var ᴋ10 = overlapped;
    var ᴋ11 = croutine;
        var (r1, _, e1) = syscall.Syscall6(sendRecvMsgFunc.recvAddr, 5, (uintptr)fd, (uintptr)ᴋ8, (uintptr)ᴋ9, (uintptr)ᴋ10, (uintptr)ᴋ11, 0);
    System.GC.KeepAlive(ᴋ8);
    System.GC.KeepAlive(ᴋ9);
    System.GC.KeepAlive(ᴋ10);
    System.GC.KeepAlive(ᴋ11);
    if (r1 == socket_error) {
        err = errnoErr(e1);
    }
    return err;
}

partial struct Rusage {
    public Filetime CreationTime;
    public Filetime ExitTime;
    public Filetime KernelTime;
    public Filetime UserTime;
}

partial struct WaitStatus {
    public uint32 ExitCode;
}

} partial class windowsᴛWaitStatus {

public static bool Exited(this WaitStatus w) { return true; }

public static nint ExitStatus(this WaitStatus w) { return (nint)w.ExitCode; }

public static Signal Signal(this WaitStatus w) { return -1; }

public static bool CoreDump(this WaitStatus w) { return false; }

public static bool Stopped(this WaitStatus w) { return false; }

public static bool Continued(this WaitStatus w) { return false; }

public static Signal StopSignal(this WaitStatus w) { return -1; }

public static bool Signaled(this WaitStatus w) { return false; }

public static nint TrapCause(this WaitStatus w) { return -1; }

} partial class windows_package {

partial struct Timespec {
    public int64 Sec;
    public int64 Nsec;
}

public static int64 TimespecToNsec(Timespec ts) { return (int64)ts.Sec * 1000000000 + (int64)ts.Nsec; }

public static Timespec /*ts*/ NsecToTimespec(int64 nsec) {
    Timespec ts = default;

    ts.Sec = nsec / 1000000000;
    ts.Nsec = nsec % 1000000000;
    return ts;
}

public static (Handle nfd, Sockaddr sa, error err) Accept(Handle fd) { return (0, default, syscall.EWINDOWS); }

public static (nint n, Sockaddr from, error err) Recvfrom(Handle fd, slice<byte> p, nint flags) {
    nint n = default;
    Sockaddr from = default;
    error err = default;

    ref var rsa = ref heap(new RawSockaddrAny(), out var Ꮡrsa);
    ref var l = ref heap<int32>((int32)/* unsafe.Sizeof(rsa) */ (uintptr)116, out var Ꮡl);
    (var n32, err) = recvfrom(fd, p, (int32)flags, Ꮡrsa, Ꮡl);
    n = (nint)n32;
    if (err != nil) {
        return (n, from, err);
    }
    (from, err) = Ꮡrsa.Sockaddr();
    return (n, from, err);
}

public static error /*err*/ Sendto(Handle fd, slice<byte> p, nint flags, Sockaddr to) {
    error err = default;

    (var ptr, var l, err) = to.sockaddr();
    if (err != nil) {
        return err;
    }
    return sendto(fd, p, (int32)flags, ptr, l);
}

public static error /*err*/ SetsockoptTimeval(Handle fd, nint level, nint opt, ж<Timeval> tv) { return syscall.EWINDOWS; }

partial struct Linger {
    public int32 Onoff;
    public int32 ΔLinger;
}

partial struct sysLinger {
    public uint16 Onoff;
    public uint16 Linger;
}

[global::go.golib.GoInlineLayout(Size = 8, Align = 1), StructLayout(LayoutKind.Explicit, Size = 8)] partial struct IPMreq {
    [FieldOffset(0)] public array4<byte> Multiaddr;
    [FieldOffset(4)] public array4<byte> Interface;
}

partial struct IPv6Mreq {
    public array<byte> Multiaddr = new(16);
    public uint32 Interface;
}

public static (nint, error) GetsockoptInt(Handle fd, nint level, nint opt) {
    ref var v = ref heap<int32>((int32)0, out var Ꮡv);
    ref var l = ref heap<int32>((int32)/* unsafe.Sizeof(v) */ (uintptr)4, out var Ꮡl);
    var err = Getsockopt(fd, (int32)level, (int32)opt, Ꮡv.Reinterpret<int32, byte>(), Ꮡl);
    return ((nint)v, err);
}

public static error /*err*/ SetsockoptLinger(Handle fd, nint level, nint opt, ж<Linger> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

    ref var sys = ref heap<sysLinger>(new sysLinger(Onoff: (uint16)l.Onoff, Linger: (uint16)l.ΔLinger), out var Ꮡsys);
    return Setsockopt(fd, (int32)level, (int32)opt, Ꮡsys.Reinterpret<sysLinger, byte>(), (int32)/* unsafe.Sizeof(sys) */ (uintptr)4);
}

public static error /*err*/ SetsockoptInet4Addr(Handle fd, nint level, nint opt, /*[4]*/ array<byte> valueʗp) {
    ref var value = ref heap(valueʗp.Clone(), out var Ꮡvalue);

    return Setsockopt(fd, (int32)level, (int32)opt, Ꮡvalue.at<byte>(0), 4);
}

public static error /*err*/ SetsockoptIPMreq(Handle fd, nint level, nint opt, ж<IPMreq> mreq) {
    return Setsockopt(fd, (int32)level, (int32)opt, mreq.Reinterpret<IPMreq, byte>(), (int32)/* unsafe.Sizeof(*mreq) */ (uintptr)8);
}

public static error /*err*/ SetsockoptIPv6Mreq(Handle fd, nint level, nint opt, ж<IPv6Mreq> mreq) {
    return syscall.EWINDOWS;
}

public static error EnumProcesses(slice<uint32> processIds, ж<uint32> bytesReturned) {
    ж<uint32> p = default;
    if (len(processIds) > 0) {
        p = Ꮡ(processIds, 0);
    }
    var size = (uint32)(len(processIds) * 4);
    return enumProcesses(p, size, bytesReturned);
}

public static nint /*pid*/ Getpid() { return (nint)GetCurrentProcessId(); }

public static (Handle handle, error err) FindFirstFile(ж<uint16> name, ж<Win32finddata> data) {
    Handle handle = default;
    error err = default;

    ref var data1 = ref heap(new win32finddata1(), out var Ꮡdata1);
    (handle, err) = findFirstFile1(name, Ꮡdata1);
    if (err == nil) {
        copyFindData(ref (data).DerefOrNull(), ref data1);
    }
    return (handle, err);
}

public static error /*err*/ FindNextFile(Handle handle, ж<Win32finddata> data) {
    error err = default;

    ref var data1 = ref heap(new win32finddata1(), out var Ꮡdata1);
    err = findNextFile1(handle, Ꮡdata1);
    if (err == nil) {
        copyFindData(ref (data).DerefOrNull(), ref data1);
    }
    return err;
}

internal static (ж<ProcessEntry32>, error) getProcessEntry(nint pid) {
    GoFrame ᒐ = default;
    try {
        var (snapshot, err) = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
        if (err != nil) {
            return (default, err);
        }
        defer(CloseHandle, snapshot, ref ᒐ);
        ref var procEntry = ref heap(new ProcessEntry32(), out var ᏑprocEntry);
        procEntry.Size = (uint32)/* unsafe.Sizeof(procEntry) */ (uintptr)568;
        err = Process32First(snapshot, ᏑprocEntry); if (err != nil) {
            return (default, err);
        }
        for (;;) {
            if (procEntry.ProcessID == (uint32)pid) {
                return (ᏑprocEntry, default);
            }
            err = Process32Next(snapshot, ᏑprocEntry);
            if (err != nil) {
                return (default, err);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public static nint /*ppid*/ Getppid() {
    var (pe, err) = getProcessEntry(Getpid());
    if (err != nil) {
        return -1;
    }
    return (nint)pe.ParentProcessID;
}

public static error /*err*/ Fchdir(Handle fd) { return syscall.EWINDOWS; }
public static error /*err*/ Link(@string oldpath, @string newpath) { return syscall.EWINDOWS; }
public static error /*err*/ Symlink(@string path, @string link) { return syscall.EWINDOWS; }

public static error /*err*/ Fchmod(Handle fd, uint32 mode) { return syscall.EWINDOWS; }
public static error /*err*/ Chown(@string path, nint uid, nint gid) { return syscall.EWINDOWS; }
public static error /*err*/ Lchown(@string path, nint uid, nint gid) { return syscall.EWINDOWS; }
public static error /*err*/ Fchown(Handle fd, nint uid, nint gid) { return syscall.EWINDOWS; }

public static nint /*uid*/ Getuid() { return -1; }
public static nint /*euid*/ Geteuid() { return -1; }
public static nint /*gid*/ Getgid() { return -1; }
public static nint /*egid*/ Getegid() { return -1; }
public static (slice<nint> gids, error err) Getgroups() { return (default, syscall.EWINDOWS); }

public static error LoadCreateSymbolicLink() {
    return procCreateSymbolicLinkW.Find();
}

public static (nint n, error err) Readlink(@string path, slice<byte> buf) {
    nint n = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        (var fd, err) = CreateFile(StringToUTF16Ptr(path), GENERIC_READ, 0, nil, OPEN_EXISTING,
            (uint32)((uint32)FILE_FLAG_OPEN_REPARSE_POINT | (uint32)FILE_FLAG_BACKUP_SEMANTICS), 0);
        if (err != nil) {
            (n, err) = (-1, err); goto ᒐdone;
        }
        defer(CloseHandle, fd, ref ᒐ);

        var rdbbuf = new slice<byte>(MAXIMUM_REPARSE_DATA_BUFFER_SIZE);
        ref var bytesReturned = ref heap(new uint32(), out var ᏑbytesReturned);
        err = DeviceIoControl(fd, FSCTL_GET_REPARSE_POINT, nil, 0, Ꮡ(rdbbuf, 0), (uint32)len(rdbbuf), ᏑbytesReturned, nil);
        if (err != nil) {
            (n, err) = (-1, err); goto ᒐdone;
        }

        var rdb = Ꮡ(rdbbuf, 0).Reinterpret<byte, reparseDataBuffer>();
        @string s = default;
        switch (rdb.ReparseTag) {
        case var ᴛ when ᴛ == IO_REPARSE_TAG_SYMLINK: {
            var data = rdb.ᏑreparseBuffer.Reinterpret<byte, symbolicLinkReparseBuffer>();
            var p = array<uint16>.AliasPointer(data.at(symbolicLinkReparseBuffer.ᏑPathBuffer, 0), 65535);
            s = UTF16ToString(p.slice(data.PrintNameOffset / 2, (uint16)(data.PrintNameLength - data.PrintNameOffset) / 2));
            break;
        }
        case var ᴛ when ᴛ == IO_REPARSE_TAG_MOUNT_POINT: {
            var data = rdb.ᏑreparseBuffer.Reinterpret<byte, mountPointReparseBuffer>();
            var p = array<uint16>.AliasPointer(data.at(mountPointReparseBuffer.ᏑPathBuffer, 0), 65535);
            s = UTF16ToString(p.slice(data.PrintNameOffset / 2, (uint16)(data.PrintNameLength - data.PrintNameOffset) / 2));
            break;
        }
        default: {
            (n, err) = (-1, syscall.ENOENT); goto ᒐdone;
        }}

        n = copy(buf, slice<byte>(s));

        (n, err) = (n, default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (n, err);
}

public static (GUID, error) GUIDFromString(@string str) {
    ref var guid = ref heap<GUID>(new GUID(), out var Ꮡguid);
    var (str16, err) = syscall.UTF16PtrFromString(str);
    if (err != nil) {
        return (guid, err);
    }
    err = clsidFromString(str16, Ꮡguid);
    if (err != nil) {
        return (guid, err);
    }
    return (guid, default);
}

public static (GUID, error) GenerateGUID() {
    ref var guid = ref heap<GUID>(new GUID(), out var Ꮡguid);
    var err = coCreateGuid(Ꮡguid);
    if (err != nil) {
        return (guid, err);
    }
    return (guid, default);
}

} partial class windowsᴛGUID {

public static @string String(this GUID guidʗp) {
    ref var guid = ref heap(guidʗp, out var Ꮡguid);

    ref var str = ref heap(new array<uint16>(100), out var Ꮡstr);
    var chars = stringFromGUID2(Ꮡguid, Ꮡstr.at<uint16>(0), (int32)len(str));
    if (chars <= 1) {
        return "";
    }
    return (@string)utf16.Decode(str.slice(0, chars - 1));
}

} partial class windows_package {

public static (@string, error) KnownFolderPath(ж<KNOWNFOLDERID> folderID, uint32 flags) {
    return ((Token)0).KnownFolderPath(folderID, flags);
}

} partial class windowsᴛToken {

public static (@string, error) KnownFolderPath(this Token t, ж<KNOWNFOLDERID> folderID, uint32 flags) {
    GoFrame ᒐ = default;
    try {
        ref var p = ref heap<ж<uint16>>(out var Ꮡp);
        var err = shGetKnownFolderPath(folderID, flags, t, Ꮡp);
        if (err != nil) {
            return ("", err);
        }
        defer(CoTaskMemFree, @unsafe.Pointer.FromPinnedBox(p), ref ᒐ);
        return (UTF16PtrToString(p), default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

} partial class windows_package {

public static ж<OsVersionInfoEx> RtlGetVersion() {
    var info = Ꮡ(new OsVersionInfoEx());
    info.osVersionInfoSize = (uint32)/* unsafe.Sizeof(*info) */ (uintptr)284;
    _ = rtlGetVersion(info);
    return info;
}

public static (uint32 majorVersion, uint32 minorVersion, uint32 buildNumber) RtlGetNtVersionNumbers() {
    ref var majorVersion = ref heap(new uint32(), out var ᏑmajorVersion);
    ref var minorVersion = ref heap(new uint32(), out var ᏑminorVersion);
    ref var buildNumber = ref heap(new uint32(), out var ᏑbuildNumber);

    rtlGetNtVersionNumbers(ᏑmajorVersion, ᏑminorVersion, ᏑbuildNumber);
    buildNumber &= (uint32)(0xffff);
    return (majorVersion, minorVersion, buildNumber);
}

public static (slice<@string>, error) GetProcessPreferredUILanguages(uint32 flags) {
    return getUILanguages(flags, getProcessPreferredUILanguages);
}

public static (slice<@string>, error) GetThreadPreferredUILanguages(uint32 flags) {
    return getUILanguages(flags, getThreadPreferredUILanguages);
}

public static (slice<@string>, error) GetUserPreferredUILanguages(uint32 flags) {
    return getUILanguages(flags, getUserPreferredUILanguages);
}

public static (slice<@string>, error) GetSystemPreferredUILanguages(uint32 flags) {
    return getUILanguages(flags, getSystemPreferredUILanguages);
}

internal static (slice<@string>, error) getUILanguages(uint32 flags, Func<uint32, ж<uint32>, ж<uint16>, ж<uint32>, error> f) {
    ref var size = ref heap<uint32>((uint32)128, out var Ꮡsize);
    for (;;) {
        ref var numLanguages = ref heap(new uint32(), out var ᏑnumLanguages);
        var buf = new slice<uint16>((nint)(size));
        var err = f(flags, ᏑnumLanguages, Ꮡ(buf, 0), Ꮡsize);
        if (AreEqual(err, ERROR_INSUFFICIENT_BUFFER)) {
            continue;
        }
        if (err != nil) {
            return (default, err);
        }
        buf = buf.slice(0, (nint)(size));
        if (numLanguages == 0 || len(buf) == 0) {
            return (slice<@string>([]), default);
        }
        if (buf[len(buf) - 1] == 0) {
            buf = buf.slice(0, len(buf) - 1);
        }
        var languages = new slice<@string>(0, (nint)(numLanguages));
        nint from = 0;
        foreach (var (i, c) in range(buf)) {
            if (c == 0) {
                languages = append(languages, (@string)utf16.Decode(buf.slice(from, i)));
                from = i + 1;
            }
        }
        return (languages, default);
    }
}

public static error SetConsoleCursorPosition(Handle console, Coord positionʗp) {
    ref var position = ref heap(positionʗp, out var Ꮡposition);

    return setConsoleCursorPosition(console, (Ꮡposition.Reinterpret<Coord, uint32>()).Value);
}

public static error GetStartupInfo(ж<StartupInfo> startupInfo) {
    getStartupInfo(startupInfo);
    return default;
}

} partial class windowsᴛNTStatus {

public static syscall.Errno Errno(this NTStatus s) {
    return rtlNtStatusToDosErrorNoTeb(s);
}

} partial class windows_package {

internal static uint32 langID(uint16 pri, uint16 sub) { return (uint32)sub << 10 | (uint32)pri; }

} partial class windowsᴛNTStatus {

public static @string Error(this NTStatus s) {
    var b = new slice<uint16>(300);
    var (n, err) = FormatMessage((uint32)((UntypedInt)(FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_FROM_HMODULE) | (uint32)FORMAT_MESSAGE_ARGUMENT_ARRAY), modntdll.Handle(), (uint32)s, langID(LANG_ENGLISH, SUBLANG_ENGLISH_US), b, nil);
    if (err != nil) {
        return fmt.Sprintf("NTSTATUS 0x%08x"u8, (uint32)s);
    }
    for (; n > 0 && (b[n - 1] == '\n' || b[n - 1] == '\r'); n--) {
    }
    return (@string)utf16.Decode(b.slice(0, (nint)(n)));
}

} partial class windows_package {

public static (ж<NTUnicodeString>, error) NewNTUnicodeString(@string s) {
    var (s16, err) = UTF16FromString(s);
    if (err != nil) {
        return (default, err);
    }
    nint n = len(s16) * 2;
    if (n > (1 << 16) - 1) {
        return (default, syscall.EINVAL);
    }
    return (Ꮡ(new NTUnicodeString(
        Length: (uint16)((uint16)n - 2),
        MaximumLength: (uint16)n,
        Buffer: Ꮡ(s16, 0)
    )), default);
}

} partial class windowsᴛNTUnicodeString {

public static slice<uint16> Slice(this ref NTUnicodeString s) {
    return @unsafe.Slice(s.Buffer, (uint16)(s.MaximumLength / 2)).slice(0, s.Length / 2);
}

public static @string String(this ref NTUnicodeString s) {
    return UTF16ToString(s.Slice());
}

} partial class windows_package {

public static (ж<NTString>, error) NewNTString(@string s) {
    ref var nts = ref heap(new NTString(), out var Ꮡnts);
    var (s8, err) = ByteSliceFromString(s);
    if (err != nil) {
        return (default, err);
    }
    if (len(s8) > MAX_USHORT) {
        return (default, syscall.EINVAL);
    }
    RtlInitString(Ꮡnts, Ꮡ(s8, 0));
    return (Ꮡnts, default);
}

} partial class windowsᴛNTString {

public static slice<byte> Slice(this ref NTString s) {
    var Δslice = @unsafe.Slice(s.Buffer, s.MaximumLength);
    return Δslice.slice(0, s.Length);
}

public static @string String(this ref NTString s) {
    return ByteSliceToString(s.Slice());
}

} partial class windows_package {

public static (Handle, error) FindResource(Handle module, ResourceIDOrString name, ResourceIDOrString resType) {
    uintptr namePtr = default;
    uintptr resTypePtr = default;
    ref var name16 = ref heap<ж<uint16>>(out var Ꮡname16);
    ref var resType16 = ref heap<ж<uint16>>(out var ᏑresType16);
    ref var err = ref heap<error>(out var Ꮡerr);
    (uintptr, error) resolvePtr(any i, ж<ж<uint16>> keep) {
        switch (i.type()) {
        case @string v: {
            (keep.ValueSlot, Ꮡerr.ValueSlot) = UTF16PtrFromString(v);
            if (Ꮡerr.ValueSlot != nil) {
                return ((uintptr)(0), Ꮡerr.ValueSlot);
            }
            return ((uintptr)(keep.ValueSlot), default);
        }
        case ResourceID v: {
            return ((uintptr)v, default);
        }}
        return ((uintptr)(0), errorspkg.New("parameter must be a ResourceID or a string"));
    }
    (namePtr, err) = resolvePtr(name, Ꮡname16);
    if (err != nil) {
        return (0, err);
    }
    (resTypePtr, err) = resolvePtr(resType, ᏑresType16);
    if (err != nil) {
        return (0, err);
    }
    (var resInfo, err) = findResource(module, namePtr, resTypePtr);
    runtime.KeepAlive(name16.OrTypedNil());
    runtime.KeepAlive(resType16.OrTypedNil());
    return (resInfo, err);
}

public static (slice<byte> data, error err) LoadResourceData(Handle module, Handle resInfo) {
    slice<byte> data = default;
    error err = default;

    (var size, err) = SizeofResource(module, resInfo);
    if (err != nil) {
        return (data, err);
    }
    (var resData, err) = LoadResource(module, resInfo);
    if (err != nil) {
        return (data, err);
    }
    (var ptr, err) = LockResource(resData);
    if (err != nil) {
        return (data, err);
    }
    data = @unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)ptr), size);
    return (data, err);
}

partial struct PSAPI_WORKING_SET_EX_BLOCK /*num:uint64*/;

} partial class windowsᴛPSAPI_WORKING_SET_EX_BLOCK {

public static bool Valid(this PSAPI_WORKING_SET_EX_BLOCK b) {
    return ((PSAPI_WORKING_SET_EX_BLOCK)(b & 1)) == 1;
}

public static uint64 ShareCount(this PSAPI_WORKING_SET_EX_BLOCK b) {
    return b.intField(1, 3);
}

public static uint64 Win32Protection(this PSAPI_WORKING_SET_EX_BLOCK b) {
    return b.intField(4, 11);
}

public static bool Shared(this PSAPI_WORKING_SET_EX_BLOCK b) {
    return ((PSAPI_WORKING_SET_EX_BLOCK)(b & ((PSAPI_WORKING_SET_EX_BLOCK)((uint64)1 << 15)))) == 1;
}

public static uint64 Node(this PSAPI_WORKING_SET_EX_BLOCK b) {
    return b.intField(16, 6);
}

public static bool Locked(this PSAPI_WORKING_SET_EX_BLOCK b) {
    return ((PSAPI_WORKING_SET_EX_BLOCK)(b & ((PSAPI_WORKING_SET_EX_BLOCK)((uint64)1 << 22)))) == 1;
}

public static bool LargePage(this PSAPI_WORKING_SET_EX_BLOCK b) {
    return ((PSAPI_WORKING_SET_EX_BLOCK)(b & ((PSAPI_WORKING_SET_EX_BLOCK)((uint64)1 << 23)))) == 1;
}

public static bool Bad(this PSAPI_WORKING_SET_EX_BLOCK b) {
    return ((PSAPI_WORKING_SET_EX_BLOCK)(b & ((PSAPI_WORKING_SET_EX_BLOCK)((uint64)1 << 31)))) == 1;
}

internal static uint64 intField(this PSAPI_WORKING_SET_EX_BLOCK b, nint start, nint length) {
    PSAPI_WORKING_SET_EX_BLOCK mask = default;
    for (nint pos = start; pos < start + length; pos++) {
        mask |= (PSAPI_WORKING_SET_EX_BLOCK)(((PSAPI_WORKING_SET_EX_BLOCK)((uint64)1 << (int)(pos))));
    }

    var masked = (PSAPI_WORKING_SET_EX_BLOCK)(b & mask);
    return (uint64)(masked >> (int64)(start));
}

} partial class windows_package {

partial struct PSAPI_WORKING_SET_EX_INFORMATION {
    public Pointer VirtualAddress;
    public PSAPI_WORKING_SET_EX_BLOCK VirtualAttributes;
}

public static error CreatePseudoConsole(Coord sizeʗp, Handle @in, Handle @out, uint32 flags, ж<Handle> pconsole) {
    ref var size = ref heap(sizeʗp, out var Ꮡsize);

    return createPseudoConsole((Ꮡsize.Reinterpret<Coord, uint32>()).Value, @in, @out, flags, pconsole);
}

public static error ResizePseudoConsole(Handle pconsole, Coord sizeʗp) {
    ref var size = ref heap(sizeʗp, out var Ꮡsize);

    return resizePseudoConsole(pconsole, (Ꮡsize.Reinterpret<Coord, uint32>()).Value);
}

public static UntypedInt CBR_110 => 110;
public static UntypedInt CBR_300 => 300;
public static UntypedInt CBR_600 => 600;
public static UntypedInt CBR_1200 => 1200;
public static UntypedInt CBR_2400 => 2400;
public static UntypedInt CBR_4800 => 4800;
public static UntypedInt CBR_9600 => 9600;
public static UntypedInt CBR_14400 => 14400;
public static UntypedInt CBR_19200 => 19200;
public static UntypedInt CBR_38400 => 38400;
public static UntypedInt CBR_57600 => 57600;
public static UntypedInt CBR_115200 => 115200;
public static UntypedInt CBR_128000 => 128000;
public static UntypedInt CBR_256000 => 256000;
public static UntypedInt DTR_CONTROL_DISABLE => 0x00000000;
public static UntypedInt DTR_CONTROL_ENABLE => 0x00000010;
public static UntypedInt DTR_CONTROL_HANDSHAKE => 0x00000020;
public static UntypedInt RTS_CONTROL_DISABLE => 0x00000000;
public static UntypedInt RTS_CONTROL_ENABLE => 0x00001000;
public static UntypedInt RTS_CONTROL_HANDSHAKE => 0x00002000;
public static UntypedInt RTS_CONTROL_TOGGLE => 0x00003000;
public static UntypedInt NOPARITY => 0;
public static UntypedInt ODDPARITY => 1;
public static UntypedInt EVENPARITY => 2;
public static UntypedInt MARKPARITY => 3;
public static UntypedInt SPACEPARITY => 4;
public static UntypedInt ONESTOPBIT => 0;
public static UntypedInt ONE5STOPBITS => 1;
public static UntypedInt TWOSTOPBITS => 2;

public static UntypedInt SETXOFF => 1;
public static UntypedInt SETXON => 2;
public static UntypedInt SETRTS => 3;
public static UntypedInt CLRRTS => 4;
public static UntypedInt SETDTR => 5;
public static UntypedInt CLRDTR => 6;
public static UntypedInt SETBREAK => 8;
public static UntypedInt CLRBREAK => 9;

public static UntypedInt PURGE_TXABORT => 0x0001;
public static UntypedInt PURGE_RXABORT => 0x0002;
public static UntypedInt PURGE_TXCLEAR => 0x0004;
public static UntypedInt PURGE_RXCLEAR => 0x0008;

public static UntypedInt EV_RXCHAR => 0x0001;
public static UntypedInt EV_RXFLAG => 0x0002;
public static UntypedInt EV_TXEMPTY => 0x0004;
public static UntypedInt EV_CTS => 0x0008;
public static UntypedInt EV_DSR => 0x0010;
public static UntypedInt EV_RLSD => 0x0020;
public static UntypedInt EV_BREAK => 0x0040;
public static UntypedInt EV_ERR => 0x0080;
public static UntypedInt EV_RING => 0x0100;

} // end windows_package
