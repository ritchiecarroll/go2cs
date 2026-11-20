namespace go.golang.org.x.sys;

using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class unix_package {

public static (nint fd, error err) FanotifyInit(nuint flags, nuint event_f_flags) {
    nint fd = default;
    error err = default;

    var (r0, _, e1) = Syscall(SYS_FANOTIFY_INIT, (uintptr)flags, (uintptr)event_f_flags, 0);
    fd = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (fd, err);
}

internal static error /*err*/ fchmodat(nint dirfd, @string path, uint32 mode) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall(SYS_FCHMODAT, (uintptr)dirfd, (uintptr)_p0, (uintptr)mode);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ fchmodat2(nint dirfd, @string path, uint32 mode, nint flags) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall6(SYS_FCHMODAT2, (uintptr)dirfd, (uintptr)_p0, (uintptr)mode, (uintptr)flags, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ ioctl(nint fd, nuint req, uintptr arg) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_IOCTL, (uintptr)fd, (uintptr)req, (uintptr)arg);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ ioctlPtr(nint fd, nuint req, @unsafe.Pointer arg) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_IOCTL, (uintptr)fd, (uintptr)req, (uintptr)arg);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Linkat(nint olddirfd, @string oldpath, nint newdirfd, @string newpath, nint flags) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(oldpath);
    if (err != nil) {
        return err;
    }
    ж<byte> _p1 = default;
    (_p1, err) = BytePtrFromString(newpath);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall6(SYS_LINKAT, (uintptr)olddirfd, (uintptr)_p0, (uintptr)newdirfd, (uintptr)_p1, (uintptr)flags, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static (nint fd, error err) openat(nint dirfd, @string path, nint flags, uint32 mode) {
    nint fd = default;
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return (fd, err);
    }
    var (r0, _, e1) = Syscall6(SYS_OPENAT, (uintptr)dirfd, (uintptr)_p0, (uintptr)flags, (uintptr)mode, 0, 0);
    fd = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (fd, err);
}

internal static (nint fd, error err) openat2(nint dirfd, @string path, ж<OpenHow> open_how, nint size) {
    nint fd = default;
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return (fd, err);
    }
    var (r0, _, e1) = Syscall6(SYS_OPENAT2, (uintptr)dirfd, (uintptr)_p0, (uintptr)open_how, (uintptr)size, 0, 0);
    fd = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (fd, err);
}

internal static error /*err*/ pipe2(/*[2]*/ ж<array<_C_int>> p, nint flags) {
    error err = default;

    var (_, _, e1) = RawSyscall(SYS_PIPE2, (uintptr)p, (uintptr)flags, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static (nint n, error err) ppoll(ж<PollFd> fds, nint nfds, ж<Timespec> timeout, ж<Sigset_t> sigmask) {
    nint n = default;
    error err = default;

    var (r0, _, e1) = Syscall6(SYS_PPOLL, (uintptr)fds, (uintptr)nfds, (uintptr)timeout, (uintptr)sigmask, 0, 0);
    n = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

public static (nint n, error err) Readlinkat(nint dirfd, @string path, slice<byte> buf) {
    nint n = default;
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return (n, err);
    }
    @unsafe.Pointer _p1 = default;
    if (len(buf) > 0) {
        _p1 = @unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 0));
    } else {
        _p1 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (r0, _, e1) = Syscall6(SYS_READLINKAT, (uintptr)dirfd, (uintptr)_p0, (uintptr)_p1, (uintptr)len(buf), 0, 0);
    n = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

public static error /*err*/ Symlinkat(@string oldpath, nint newdirfd, @string newpath) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(oldpath);
    if (err != nil) {
        return err;
    }
    ж<byte> _p1 = default;
    (_p1, err) = BytePtrFromString(newpath);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall(SYS_SYMLINKAT, (uintptr)_p0, (uintptr)newdirfd, (uintptr)_p1);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Unlinkat(nint dirfd, @string path, nint flags) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall(SYS_UNLINKAT, (uintptr)dirfd, (uintptr)_p0, (uintptr)flags);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ utimensat(nint dirfd, @string path, /*[2]*/ ж<array<Timespec>> times, nint flags) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall6(SYS_UTIMENSAT, (uintptr)dirfd, (uintptr)_p0, (uintptr)times, (uintptr)flags, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (nint n, error err) Getcwd(slice<byte> buf) {
    nint n = default;
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(buf) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (r0, _, e1) = Syscall(SYS_GETCWD, (uintptr)_p0, (uintptr)len(buf), 0);
    n = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

internal static (nint wpid, error err) wait4(nint pid, ж<_C_int> wstatus, nint options, ж<Rusage> rusage) {
    nint wpid = default;
    error err = default;

    var (r0, _, e1) = Syscall6(SYS_WAIT4, (uintptr)pid, (uintptr)wstatus, (uintptr)options, (uintptr)rusage, 0, 0);
    wpid = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (wpid, err);
}

public static error /*err*/ Waitid(nint idType, nint id, ж<Siginfo> info, nint options, ж<Rusage> rusage) {
    error err = default;

    var (_, _, e1) = Syscall6(SYS_WAITID, (uintptr)idType, (uintptr)id, (uintptr)info, (uintptr)options, (uintptr)rusage, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (nint ret, error err) KeyctlInt(nint cmd, nint arg2, nint arg3, nint arg4, nint arg5) {
    nint ret = default;
    error err = default;

    var (r0, _, e1) = Syscall6(SYS_KEYCTL, (uintptr)cmd, (uintptr)arg2, (uintptr)arg3, (uintptr)arg4, (uintptr)arg5, 0);
    ret = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (ret, err);
}

public static (nint ret, error err) KeyctlBuffer(nint cmd, nint arg2, slice<byte> buf, nint arg5) {
    nint ret = default;
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(buf) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (r0, _, e1) = Syscall6(SYS_KEYCTL, (uintptr)cmd, (uintptr)arg2, (uintptr)_p0, (uintptr)len(buf), (uintptr)arg5, 0);
    ret = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (ret, err);
}

internal static (nint ret, error err) keyctlJoin(nint cmd, @string arg2) {
    nint ret = default;
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(arg2);
    if (err != nil) {
        return (ret, err);
    }
    var (r0, _, e1) = Syscall(SYS_KEYCTL, (uintptr)cmd, (uintptr)_p0, 0);
    ret = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (ret, err);
}

internal static (nint ret, error err) keyctlSearch(nint cmd, nint arg2, @string arg3, @string arg4, nint arg5) {
    nint ret = default;
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(arg3);
    if (err != nil) {
        return (ret, err);
    }
    ж<byte> _p1 = default;
    (_p1, err) = BytePtrFromString(arg4);
    if (err != nil) {
        return (ret, err);
    }
    var (r0, _, e1) = Syscall6(SYS_KEYCTL, (uintptr)cmd, (uintptr)arg2, (uintptr)_p0, (uintptr)_p1, (uintptr)arg5, 0);
    ret = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (ret, err);
}

internal static error /*err*/ keyctlIOV(nint cmd, nint arg2, slice<Iovec> payload, nint arg5) {
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(payload) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(payload, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (_, _, e1) = Syscall6(SYS_KEYCTL, (uintptr)cmd, (uintptr)arg2, (uintptr)_p0, (uintptr)len(payload), (uintptr)arg5, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static (nint ret, error err) keyctlDH(nint cmd, ж<KeyctlDHParams> arg2, slice<byte> buf) {
    nint ret = default;
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(buf) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (r0, _, e1) = Syscall6(SYS_KEYCTL, (uintptr)cmd, (uintptr)arg2, (uintptr)_p0, (uintptr)len(buf), 0, 0);
    ret = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (ret, err);
}

internal static error /*err*/ keyctlRestrictKeyringByType(nint cmd, nint arg2, @string keyType, @string restriction) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(keyType);
    if (err != nil) {
        return err;
    }
    ж<byte> _p1 = default;
    (_p1, err) = BytePtrFromString(restriction);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall6(SYS_KEYCTL, (uintptr)cmd, (uintptr)arg2, (uintptr)_p0, (uintptr)_p1, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ keyctlRestrictKeyring(nint cmd, nint arg2) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_KEYCTL, (uintptr)cmd, (uintptr)arg2, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ ptrace(nint request, nint pid, uintptr addr, uintptr data) {
    error err = default;

    var (_, _, e1) = Syscall6(SYS_PTRACE, (uintptr)request, (uintptr)pid, (uintptr)addr, (uintptr)data, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ ptracePtr(nint request, nint pid, uintptr addr, @unsafe.Pointer data) {
    error err = default;

    var (_, _, e1) = Syscall6(SYS_PTRACE, (uintptr)request, (uintptr)pid, (uintptr)addr, (uintptr)data, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ reboot(nuint magic1, nuint magic2, nint cmd, @string arg) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(arg);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall6(SYS_REBOOT, (uintptr)magic1, (uintptr)magic2, (uintptr)cmd, (uintptr)_p0, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ mount(@string source, @string target, @string fstype, uintptr flags, ж<byte> data) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(source);
    if (err != nil) {
        return err;
    }
    ж<byte> _p1 = default;
    (_p1, err) = BytePtrFromString(target);
    if (err != nil) {
        return err;
    }
    ж<byte> _p2 = default;
    (_p2, err) = BytePtrFromString(fstype);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall6(SYS_MOUNT, (uintptr)_p0, (uintptr)_p1, (uintptr)_p2, (uintptr)flags, (uintptr)data, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ mountSetattr(nint dirfd, @string pathname, nuint flags, ж<MountAttr> attr, uintptr size) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(pathname);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall6(SYS_MOUNT_SETATTR, (uintptr)dirfd, (uintptr)_p0, (uintptr)flags, (uintptr)attr, (uintptr)size, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Acct(@string path) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall(SYS_ACCT, (uintptr)_p0, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (nint id, error err) AddKey(@string keyType, @string description, slice<byte> payload, nint ringid) {
    nint id = default;
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(keyType);
    if (err != nil) {
        return (id, err);
    }
    ж<byte> _p1 = default;
    (_p1, err) = BytePtrFromString(description);
    if (err != nil) {
        return (id, err);
    }
    @unsafe.Pointer _p2 = default;
    if (len(payload) > 0) {
        _p2 = @unsafe.Pointer.FromPinnedBox(Ꮡ(payload, 0));
    } else {
        _p2 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (r0, _, e1) = Syscall6(SYS_ADD_KEY, (uintptr)_p0, (uintptr)_p1, (uintptr)_p2, (uintptr)len(payload), (uintptr)ringid, 0);
    id = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (id, err);
}

public static (nint state, error err) Adjtimex(ж<Timex> buf) {
    nint state = default;
    error err = default;

    var (r0, _, e1) = Syscall(SYS_ADJTIMEX, (uintptr)buf, 0, 0);
    state = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (state, err);
}

public static error /*err*/ Capget(ж<CapUserHeader> hdr, ж<CapUserData> data) {
    error err = default;

    var (_, _, e1) = RawSyscall(SYS_CAPGET, (uintptr)hdr, (uintptr)data, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Capset(ж<CapUserHeader> hdr, ж<CapUserData> data) {
    error err = default;

    var (_, _, e1) = RawSyscall(SYS_CAPSET, (uintptr)hdr, (uintptr)data, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Chdir(@string path) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall(SYS_CHDIR, (uintptr)_p0, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Chroot(@string path) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall(SYS_CHROOT, (uintptr)_p0, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (nint state, error err) ClockAdjtime(int32 clockid, ж<Timex> buf) {
    nint state = default;
    error err = default;

    var (r0, _, e1) = Syscall(SYS_CLOCK_ADJTIME, (uintptr)clockid, (uintptr)buf, 0);
    state = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (state, err);
}

public static error /*err*/ ClockGetres(int32 clockid, ж<Timespec> res) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_CLOCK_GETRES, (uintptr)clockid, (uintptr)res, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ ClockGettime(int32 clockid, ж<Timespec> time) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_CLOCK_GETTIME, (uintptr)clockid, (uintptr)time, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ ClockSettime(int32 clockid, ж<Timespec> time) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_CLOCK_SETTIME, (uintptr)clockid, (uintptr)time, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ ClockNanosleep(int32 clockid, nint flags, ж<Timespec> request, ж<Timespec> remain) {
    error err = default;

    var (_, _, e1) = Syscall6(SYS_CLOCK_NANOSLEEP, (uintptr)clockid, (uintptr)flags, (uintptr)request, (uintptr)remain, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Close(nint fd) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_CLOSE, (uintptr)fd, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ CloseRange(nuint first, nuint last, nuint flags) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_CLOSE_RANGE, (uintptr)first, (uintptr)last, (uintptr)flags);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (nint n, error err) CopyFileRange(nint rfd, ж<int64> roff, nint wfd, ж<int64> woff, nint len, nint flags) {
    nint n = default;
    error err = default;

    var (r0, _, e1) = Syscall6(SYS_COPY_FILE_RANGE, (uintptr)rfd, (uintptr)roff, (uintptr)wfd, (uintptr)woff, (uintptr)len, (uintptr)flags);
    n = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

public static error /*err*/ DeleteModule(@string name, nint flags) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(name);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall(SYS_DELETE_MODULE, (uintptr)_p0, (uintptr)flags, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (nint fd, error err) Dup(nint oldfd) {
    nint fd = default;
    error err = default;

    var (r0, _, e1) = Syscall(SYS_DUP, (uintptr)oldfd, 0, 0);
    fd = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (fd, err);
}

public static error /*err*/ Dup3(nint oldfd, nint newfd, nint flags) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_DUP3, (uintptr)oldfd, (uintptr)newfd, (uintptr)flags);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (nint fd, error err) EpollCreate1(nint flag) {
    nint fd = default;
    error err = default;

    var (r0, _, e1) = RawSyscall(SYS_EPOLL_CREATE1, (uintptr)flag, 0, 0);
    fd = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (fd, err);
}

public static error /*err*/ EpollCtl(nint epfd, nint op, nint fd, ж<EpollEvent> @event) {
    error err = default;

    var (_, _, e1) = RawSyscall6(SYS_EPOLL_CTL, (uintptr)epfd, (uintptr)op, (uintptr)fd, (uintptr)@event, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (nint n, error err) EpollWait(nint epfd, slice<EpollEvent> events, nint msec) {
    nint n = default;
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(events) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(events, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (r0, _, e1) = Syscall6(SYS_EPOLL_PWAIT, (uintptr)epfd, (uintptr)_p0, (uintptr)len(events), (uintptr)msec, 0, 0);
    n = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

public static (nint fd, error err) Eventfd(nuint initval, nint flags) {
    nint fd = default;
    error err = default;

    var (r0, _, e1) = Syscall(SYS_EVENTFD2, (uintptr)initval, (uintptr)flags, 0);
    fd = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (fd, err);
}

public static void Exit(nint code) {
    SyscallNoError(SYS_EXIT_GROUP, (uintptr)code, 0, 0);
    return;
}

public static error /*err*/ Fchdir(nint fd) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_FCHDIR, (uintptr)fd, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Fchmod(nint fd, uint32 mode) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_FCHMOD, (uintptr)fd, (uintptr)mode, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Fchownat(nint dirfd, @string path, nint uid, nint gid, nint flags) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall6(SYS_FCHOWNAT, (uintptr)dirfd, (uintptr)_p0, (uintptr)uid, (uintptr)gid, (uintptr)flags, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Fdatasync(nint fd) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_FDATASYNC, (uintptr)fd, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (nint sz, error err) Fgetxattr(nint fd, @string attr, slice<byte> dest) {
    nint sz = default;
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(attr);
    if (err != nil) {
        return (sz, err);
    }
    @unsafe.Pointer _p1 = default;
    if (len(dest) > 0) {
        _p1 = @unsafe.Pointer.FromPinnedBox(Ꮡ(dest, 0));
    } else {
        _p1 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (r0, _, e1) = Syscall6(SYS_FGETXATTR, (uintptr)fd, (uintptr)_p0, (uintptr)_p1, (uintptr)len(dest), 0, 0);
    sz = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (sz, err);
}

public static error /*err*/ FinitModule(nint fd, @string @params, nint flags) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(@params);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall(SYS_FINIT_MODULE, (uintptr)fd, (uintptr)_p0, (uintptr)flags);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (nint sz, error err) Flistxattr(nint fd, slice<byte> dest) {
    nint sz = default;
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(dest) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(dest, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (r0, _, e1) = Syscall(SYS_FLISTXATTR, (uintptr)fd, (uintptr)_p0, (uintptr)len(dest));
    sz = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (sz, err);
}

public static error /*err*/ Flock(nint fd, nint how) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_FLOCK, (uintptr)fd, (uintptr)how, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Fremovexattr(nint fd, @string attr) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(attr);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall(SYS_FREMOVEXATTR, (uintptr)fd, (uintptr)_p0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Fsetxattr(nint fd, @string attr, slice<byte> dest, nint flags) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(attr);
    if (err != nil) {
        return err;
    }
    @unsafe.Pointer _p1 = default;
    if (len(dest) > 0) {
        _p1 = @unsafe.Pointer.FromPinnedBox(Ꮡ(dest, 0));
    } else {
        _p1 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (_, _, e1) = Syscall6(SYS_FSETXATTR, (uintptr)fd, (uintptr)_p0, (uintptr)_p1, (uintptr)len(dest), (uintptr)flags, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Fsync(nint fd) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_FSYNC, (uintptr)fd, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (nint fsfd, error err) Fsmount(nint fd, nint flags, nint mountAttrs) {
    nint fsfd = default;
    error err = default;

    var (r0, _, e1) = Syscall(SYS_FSMOUNT, (uintptr)fd, (uintptr)flags, (uintptr)mountAttrs);
    fsfd = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (fsfd, err);
}

public static (nint fd, error err) Fsopen(@string fsName, nint flags) {
    nint fd = default;
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(fsName);
    if (err != nil) {
        return (fd, err);
    }
    var (r0, _, e1) = Syscall(SYS_FSOPEN, (uintptr)_p0, (uintptr)flags, 0);
    fd = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (fd, err);
}

public static (nint fd, error err) Fspick(nint dirfd, @string pathName, nint flags) {
    nint fd = default;
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(pathName);
    if (err != nil) {
        return (fd, err);
    }
    var (r0, _, e1) = Syscall(SYS_FSPICK, (uintptr)dirfd, (uintptr)_p0, (uintptr)flags);
    fd = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (fd, err);
}

internal static error /*err*/ fsconfig(nint fd, nuint cmd, ж<byte> key, ж<byte> value, nint aux) {
    error err = default;

    var (_, _, e1) = Syscall6(SYS_FSCONFIG, (uintptr)fd, (uintptr)cmd, (uintptr)key, (uintptr)value, (uintptr)aux, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static (nint n, error err) getdents(nint fd, slice<byte> buf) {
    nint n = default;
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(buf) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (r0, _, e1) = Syscall(SYS_GETDENTS64, (uintptr)fd, (uintptr)_p0, (uintptr)len(buf));
    n = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

public static (nint pgid, error err) Getpgid(nint pid) {
    nint pgid = default;
    error err = default;

    var (r0, _, e1) = RawSyscall(SYS_GETPGID, (uintptr)pid, 0, 0);
    pgid = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (pgid, err);
}

public static nint /*pid*/ Getpid() {
    nint pid = default;

    var (r0, _) = RawSyscallNoError(SYS_GETPID, 0, 0, 0);
    pid = (nint)r0;
    return pid;
}

public static nint /*ppid*/ Getppid() {
    nint ppid = default;

    var (r0, _) = RawSyscallNoError(SYS_GETPPID, 0, 0, 0);
    ppid = (nint)r0;
    return ppid;
}

public static (nint prio, error err) Getpriority(nint which, nint who) {
    nint prio = default;
    error err = default;

    var (r0, _, e1) = Syscall(SYS_GETPRIORITY, (uintptr)which, (uintptr)who, 0);
    prio = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (prio, err);
}

public static error /*err*/ Getrusage(nint who, ж<Rusage> rusage) {
    error err = default;

    var (_, _, e1) = RawSyscall(SYS_GETRUSAGE, (uintptr)who, (uintptr)rusage, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (nint sid, error err) Getsid(nint pid) {
    nint sid = default;
    error err = default;

    var (r0, _, e1) = RawSyscall(SYS_GETSID, (uintptr)pid, 0, 0);
    sid = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (sid, err);
}

public static nint /*tid*/ Gettid() {
    nint tid = default;

    var (r0, _) = RawSyscallNoError(SYS_GETTID, 0, 0, 0);
    tid = (nint)r0;
    return tid;
}

public static (nint sz, error err) Getxattr(@string path, @string attr, slice<byte> dest) {
    nint sz = default;
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return (sz, err);
    }
    ж<byte> _p1 = default;
    (_p1, err) = BytePtrFromString(attr);
    if (err != nil) {
        return (sz, err);
    }
    @unsafe.Pointer _p2 = default;
    if (len(dest) > 0) {
        _p2 = @unsafe.Pointer.FromPinnedBox(Ꮡ(dest, 0));
    } else {
        _p2 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (r0, _, e1) = Syscall6(SYS_GETXATTR, (uintptr)_p0, (uintptr)_p1, (uintptr)_p2, (uintptr)len(dest), 0, 0);
    sz = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (sz, err);
}

public static error /*err*/ InitModule(slice<byte> moduleImage, @string @params) {
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(moduleImage) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(moduleImage, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    ж<byte> _p1 = default;
    (_p1, err) = BytePtrFromString(@params);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall(SYS_INIT_MODULE, (uintptr)_p0, (uintptr)len(moduleImage), (uintptr)_p1);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (nint watchdesc, error err) InotifyAddWatch(nint fd, @string pathname, uint32 mask) {
    nint watchdesc = default;
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(pathname);
    if (err != nil) {
        return (watchdesc, err);
    }
    var (r0, _, e1) = Syscall(SYS_INOTIFY_ADD_WATCH, (uintptr)fd, (uintptr)_p0, (uintptr)mask);
    watchdesc = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (watchdesc, err);
}

public static (nint fd, error err) InotifyInit1(nint flags) {
    nint fd = default;
    error err = default;

    var (r0, _, e1) = RawSyscall(SYS_INOTIFY_INIT1, (uintptr)flags, 0, 0);
    fd = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (fd, err);
}

public static (nint success, error err) InotifyRmWatch(nint fd, uint32 watchdesc) {
    nint success = default;
    error err = default;

    var (r0, _, e1) = RawSyscall(SYS_INOTIFY_RM_WATCH, (uintptr)fd, (uintptr)watchdesc, 0);
    success = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (success, err);
}

public static error /*err*/ Kill(nint pid, syscall.Signal sig) {
    error err = default;

    var (_, _, e1) = RawSyscall(SYS_KILL, (uintptr)pid, (uintptr)sig, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (nint n, error err) Klogctl(nint typ, slice<byte> buf) {
    nint n = default;
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(buf) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (r0, _, e1) = Syscall(SYS_SYSLOG, (uintptr)typ, (uintptr)_p0, (uintptr)len(buf));
    n = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

public static (nint sz, error err) Lgetxattr(@string path, @string attr, slice<byte> dest) {
    nint sz = default;
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return (sz, err);
    }
    ж<byte> _p1 = default;
    (_p1, err) = BytePtrFromString(attr);
    if (err != nil) {
        return (sz, err);
    }
    @unsafe.Pointer _p2 = default;
    if (len(dest) > 0) {
        _p2 = @unsafe.Pointer.FromPinnedBox(Ꮡ(dest, 0));
    } else {
        _p2 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (r0, _, e1) = Syscall6(SYS_LGETXATTR, (uintptr)_p0, (uintptr)_p1, (uintptr)_p2, (uintptr)len(dest), 0, 0);
    sz = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (sz, err);
}

public static (nint n, error err) Listns(ж<NsIdReq> req, slice<uint64> nsIds, nuint flags) {
    nint n = default;
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(nsIds) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(nsIds, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (r0, _, e1) = Syscall6(SYS_LISTNS, (uintptr)req, (uintptr)_p0, (uintptr)len(nsIds), (uintptr)flags, 0, 0);
    n = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

public static (nint sz, error err) Listxattr(@string path, slice<byte> dest) {
    nint sz = default;
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return (sz, err);
    }
    @unsafe.Pointer _p1 = default;
    if (len(dest) > 0) {
        _p1 = @unsafe.Pointer.FromPinnedBox(Ꮡ(dest, 0));
    } else {
        _p1 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (r0, _, e1) = Syscall(SYS_LISTXATTR, (uintptr)_p0, (uintptr)_p1, (uintptr)len(dest));
    sz = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (sz, err);
}

public static (nint sz, error err) Llistxattr(@string path, slice<byte> dest) {
    nint sz = default;
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return (sz, err);
    }
    @unsafe.Pointer _p1 = default;
    if (len(dest) > 0) {
        _p1 = @unsafe.Pointer.FromPinnedBox(Ꮡ(dest, 0));
    } else {
        _p1 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (r0, _, e1) = Syscall(SYS_LLISTXATTR, (uintptr)_p0, (uintptr)_p1, (uintptr)len(dest));
    sz = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (sz, err);
}

public static error /*err*/ Lremovexattr(@string path, @string attr) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return err;
    }
    ж<byte> _p1 = default;
    (_p1, err) = BytePtrFromString(attr);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall(SYS_LREMOVEXATTR, (uintptr)_p0, (uintptr)_p1, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Lsetxattr(@string path, @string attr, slice<byte> data, nint flags) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return err;
    }
    ж<byte> _p1 = default;
    (_p1, err) = BytePtrFromString(attr);
    if (err != nil) {
        return err;
    }
    @unsafe.Pointer _p2 = default;
    if (len(data) > 0) {
        _p2 = @unsafe.Pointer.FromPinnedBox(Ꮡ(data, 0));
    } else {
        _p2 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (_, _, e1) = Syscall6(SYS_LSETXATTR, (uintptr)_p0, (uintptr)_p1, (uintptr)_p2, (uintptr)len(data), (uintptr)flags, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (nint fd, error err) MemfdCreate(@string name, nint flags) {
    nint fd = default;
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(name);
    if (err != nil) {
        return (fd, err);
    }
    var (r0, _, e1) = Syscall(SYS_MEMFD_CREATE, (uintptr)_p0, (uintptr)flags, 0);
    fd = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (fd, err);
}

public static error /*err*/ Mkdirat(nint dirfd, @string path, uint32 mode) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall(SYS_MKDIRAT, (uintptr)dirfd, (uintptr)_p0, (uintptr)mode);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Mknodat(nint dirfd, @string path, uint32 mode, nint dev) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall6(SYS_MKNODAT, (uintptr)dirfd, (uintptr)_p0, (uintptr)mode, (uintptr)dev, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ MoveMount(nint fromDirfd, @string fromPathName, nint toDirfd, @string toPathName, nint flags) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(fromPathName);
    if (err != nil) {
        return err;
    }
    ж<byte> _p1 = default;
    (_p1, err) = BytePtrFromString(toPathName);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall6(SYS_MOVE_MOUNT, (uintptr)fromDirfd, (uintptr)_p0, (uintptr)toDirfd, (uintptr)_p1, (uintptr)flags, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Nanosleep(ж<Timespec> time, ж<Timespec> leftover) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_NANOSLEEP, (uintptr)time, (uintptr)leftover, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (nint r, error err) OpenTree(nint dfd, @string fileName, nuint flags) {
    nint r = default;
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(fileName);
    if (err != nil) {
        return (r, err);
    }
    var (r0, _, e1) = Syscall(SYS_OPEN_TREE, (uintptr)dfd, (uintptr)_p0, (uintptr)flags);
    r = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (r, err);
}

public static (nint fd, error err) PerfEventOpen(ж<PerfEventAttr> attr, nint pid, nint cpu, nint groupFd, nint flags) {
    nint fd = default;
    error err = default;

    var (r0, _, e1) = Syscall6(SYS_PERF_EVENT_OPEN, (uintptr)attr, (uintptr)pid, (uintptr)cpu, (uintptr)groupFd, (uintptr)flags, 0);
    fd = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (fd, err);
}

public static error /*err*/ PivotRoot(@string newroot, @string putold) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(newroot);
    if (err != nil) {
        return err;
    }
    ж<byte> _p1 = default;
    (_p1, err) = BytePtrFromString(putold);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall(SYS_PIVOT_ROOT, (uintptr)_p0, (uintptr)_p1, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Prctl(nint option, uintptr arg2, uintptr arg3, uintptr arg4, uintptr arg5) {
    error err = default;

    var (_, _, e1) = Syscall6(SYS_PRCTL, (uintptr)option, (uintptr)arg2, (uintptr)arg3, (uintptr)arg4, (uintptr)arg5, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ PrctlArg5Ptr(nint option, uintptr arg2, uintptr arg3, uintptr arg4, @unsafe.Pointer arg5) {
    error err = default;

    var (_, _, e1) = Syscall6(SYS_PRCTL, (uintptr)option, (uintptr)arg2, (uintptr)arg3, (uintptr)arg4, (uintptr)arg5, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static (nint n, error err) pselect6(nint nfd, ж<FdSet> r, ж<FdSet> w, ж<FdSet> e, ж<Timespec> timeout, ж<sigset_argpack> sigmask) {
    nint n = default;
    error err = default;

    var (r0, _, e1) = Syscall6(SYS_PSELECT6, (uintptr)nfd, (uintptr)r, (uintptr)w, (uintptr)e, (uintptr)timeout, (uintptr)sigmask);
    n = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

internal static (nint n, error err) read(nint fd, slice<byte> p) {
    nint n = default;
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(p) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(p, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (r0, _, e1) = Syscall(SYS_READ, (uintptr)fd, (uintptr)_p0, (uintptr)len(p));
    n = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

public static error /*err*/ Removexattr(@string path, @string attr) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return err;
    }
    ж<byte> _p1 = default;
    (_p1, err) = BytePtrFromString(attr);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall(SYS_REMOVEXATTR, (uintptr)_p0, (uintptr)_p1, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Renameat2(nint olddirfd, @string oldpath, nint newdirfd, @string newpath, nuint flags) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(oldpath);
    if (err != nil) {
        return err;
    }
    ж<byte> _p1 = default;
    (_p1, err) = BytePtrFromString(newpath);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall6(SYS_RENAMEAT2, (uintptr)olddirfd, (uintptr)_p0, (uintptr)newdirfd, (uintptr)_p1, (uintptr)flags, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (nint id, error err) RequestKey(@string keyType, @string description, @string callback, nint destRingid) {
    nint id = default;
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(keyType);
    if (err != nil) {
        return (id, err);
    }
    ж<byte> _p1 = default;
    (_p1, err) = BytePtrFromString(description);
    if (err != nil) {
        return (id, err);
    }
    ж<byte> _p2 = default;
    (_p2, err) = BytePtrFromString(callback);
    if (err != nil) {
        return (id, err);
    }
    var (r0, _, e1) = Syscall6(SYS_REQUEST_KEY, (uintptr)_p0, (uintptr)_p1, (uintptr)_p2, (uintptr)destRingid, 0, 0);
    id = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (id, err);
}

public static error /*err*/ Setdomainname(slice<byte> p) {
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(p) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(p, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (_, _, e1) = Syscall(SYS_SETDOMAINNAME, (uintptr)_p0, (uintptr)len(p), 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Sethostname(slice<byte> p) {
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(p) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(p, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (_, _, e1) = Syscall(SYS_SETHOSTNAME, (uintptr)_p0, (uintptr)len(p), 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Setpgid(nint pid, nint pgid) {
    error err = default;

    var (_, _, e1) = RawSyscall(SYS_SETPGID, (uintptr)pid, (uintptr)pgid, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (nint pid, error err) Setsid() {
    nint pid = default;
    error err = default;

    var (r0, _, e1) = RawSyscall(SYS_SETSID, 0, 0, 0);
    pid = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (pid, err);
}

public static error /*err*/ Settimeofday(ж<Timeval> tv) {
    error err = default;

    var (_, _, e1) = RawSyscall(SYS_SETTIMEOFDAY, (uintptr)tv, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Setns(nint fd, nint nstype) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_SETNS, (uintptr)fd, (uintptr)nstype, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Setpriority(nint which, nint who, nint prio) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_SETPRIORITY, (uintptr)which, (uintptr)who, (uintptr)prio);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Setxattr(@string path, @string attr, slice<byte> data, nint flags) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return err;
    }
    ж<byte> _p1 = default;
    (_p1, err) = BytePtrFromString(attr);
    if (err != nil) {
        return err;
    }
    @unsafe.Pointer _p2 = default;
    if (len(data) > 0) {
        _p2 = @unsafe.Pointer.FromPinnedBox(Ꮡ(data, 0));
    } else {
        _p2 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (_, _, e1) = Syscall6(SYS_SETXATTR, (uintptr)_p0, (uintptr)_p1, (uintptr)_p2, (uintptr)len(data), (uintptr)flags, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static (nint newfd, error err) signalfd(nint fd, ж<Sigset_t> sigmask, uintptr maskSize, nint flags) {
    nint newfd = default;
    error err = default;

    var (r0, _, e1) = Syscall6(SYS_SIGNALFD4, (uintptr)fd, (uintptr)sigmask, (uintptr)maskSize, (uintptr)flags, 0, 0);
    newfd = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (newfd, err);
}

public static error /*err*/ Statx(nint dirfd, @string path, nint flags, nint mask, ж<Statx_t> stat) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall6(SYS_STATX, (uintptr)dirfd, (uintptr)_p0, (uintptr)flags, (uintptr)mask, (uintptr)stat, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static void Sync() {
    SyscallNoError(SYS_SYNC, 0, 0, 0);
    return;
}

public static error /*err*/ Syncfs(nint fd) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_SYNCFS, (uintptr)fd, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Sysinfo(ж<Sysinfo_t> info) {
    error err = default;

    var (_, _, e1) = RawSyscall(SYS_SYSINFO, (uintptr)info, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (nint fd, error err) TimerfdCreate(nint clockid, nint flags) {
    nint fd = default;
    error err = default;

    var (r0, _, e1) = RawSyscall(SYS_TIMERFD_CREATE, (uintptr)clockid, (uintptr)flags, 0);
    fd = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (fd, err);
}

public static error /*err*/ TimerfdGettime(nint fd, ж<ItimerSpec> currValue) {
    error err = default;

    var (_, _, e1) = RawSyscall(SYS_TIMERFD_GETTIME, (uintptr)fd, (uintptr)currValue, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ TimerfdSettime(nint fd, nint flags, ж<ItimerSpec> newValue, ж<ItimerSpec> oldValue) {
    error err = default;

    var (_, _, e1) = RawSyscall6(SYS_TIMERFD_SETTIME, (uintptr)fd, (uintptr)flags, (uintptr)newValue, (uintptr)oldValue, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Tgkill(nint tgid, nint tid, syscall.Signal sig) {
    error err = default;

    var (_, _, e1) = RawSyscall(SYS_TGKILL, (uintptr)tgid, (uintptr)tid, (uintptr)sig);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (uintptr ticks, error err) Times(ж<Tms> tms) {
    uintptr ticks = default;
    error err = default;

    var (r0, _, e1) = RawSyscall(SYS_TIMES, (uintptr)tms, 0, 0);
    ticks = (uintptr)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (ticks, err);
}

public static nint /*oldmask*/ Umask(nint mask) {
    nint oldmask = default;

    var (r0, _) = RawSyscallNoError(SYS_UMASK, (uintptr)mask, 0, 0);
    oldmask = (nint)r0;
    return oldmask;
}

public static error /*err*/ Uname(ж<Utsname> buf) {
    error err = default;

    var (_, _, e1) = RawSyscall(SYS_UNAME, (uintptr)buf, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Unmount(@string target, nint flags) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(target);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall(SYS_UMOUNT2, (uintptr)_p0, (uintptr)flags, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Unshare(nint flags) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_UNSHARE, (uintptr)flags, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static (nint n, error err) write(nint fd, slice<byte> p) {
    nint n = default;
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(p) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(p, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (r0, _, e1) = Syscall(SYS_WRITE, (uintptr)fd, (uintptr)_p0, (uintptr)len(p));
    n = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

internal static error /*err*/ exitThread(nint code) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_EXIT, (uintptr)code, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static (nint n, error err) readv(nint fd, slice<Iovec> iovs) {
    nint n = default;
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(iovs) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(iovs, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (r0, _, e1) = Syscall(SYS_READV, (uintptr)fd, (uintptr)_p0, (uintptr)len(iovs));
    n = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

internal static (nint n, error err) writev(nint fd, slice<Iovec> iovs) {
    nint n = default;
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(iovs) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(iovs, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (r0, _, e1) = Syscall(SYS_WRITEV, (uintptr)fd, (uintptr)_p0, (uintptr)len(iovs));
    n = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

internal static (nint n, error err) preadvSyscall(nint fd, slice<Iovec> iovs, uintptr offs_l, uintptr offs_h) {
    nint n = default;
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(iovs) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(iovs, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (r0, _, e1) = Syscall6(SYS_PREADV, (uintptr)fd, (uintptr)_p0, (uintptr)len(iovs), (uintptr)offs_l, (uintptr)offs_h, 0);
    n = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

internal static (nint n, error err) pwritevSyscall(nint fd, slice<Iovec> iovs, uintptr offs_l, uintptr offs_h) {
    nint n = default;
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(iovs) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(iovs, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (r0, _, e1) = Syscall6(SYS_PWRITEV, (uintptr)fd, (uintptr)_p0, (uintptr)len(iovs), (uintptr)offs_l, (uintptr)offs_h, 0);
    n = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

internal static (nint n, error err) preadv2Syscall(nint fd, slice<Iovec> iovs, uintptr offs_l, uintptr offs_h, nint flags) {
    nint n = default;
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(iovs) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(iovs, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (r0, _, e1) = Syscall6(SYS_PREADV2, (uintptr)fd, (uintptr)_p0, (uintptr)len(iovs), (uintptr)offs_l, (uintptr)offs_h, (uintptr)flags);
    n = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

internal static (nint n, error err) pwritev2Syscall(nint fd, slice<Iovec> iovs, uintptr offs_l, uintptr offs_h, nint flags) {
    nint n = default;
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(iovs) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(iovs, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (r0, _, e1) = Syscall6(SYS_PWRITEV2, (uintptr)fd, (uintptr)_p0, (uintptr)len(iovs), (uintptr)offs_l, (uintptr)offs_h, (uintptr)flags);
    n = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

internal static error /*err*/ munmap(uintptr addr, uintptr length) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_MUNMAP, (uintptr)addr, (uintptr)length, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static (uintptr xaddr, error err) mremap(uintptr oldaddr, uintptr oldlength, uintptr newlength, nint flags, uintptr newaddr) {
    uintptr xaddr = default;
    error err = default;

    var (r0, _, e1) = Syscall6(SYS_MREMAP, (uintptr)oldaddr, (uintptr)oldlength, (uintptr)newlength, (uintptr)flags, (uintptr)newaddr, 0);
    xaddr = (uintptr)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (xaddr, err);
}

public static error /*err*/ Madvise(slice<byte> b, nint advice) {
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(b) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(b, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (_, _, e1) = Syscall(SYS_MADVISE, (uintptr)_p0, (uintptr)len(b), (uintptr)advice);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Mprotect(slice<byte> b, nint prot) {
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(b) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(b, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (_, _, e1) = Syscall(SYS_MPROTECT, (uintptr)_p0, (uintptr)len(b), (uintptr)prot);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Mlock(slice<byte> b) {
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(b) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(b, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (_, _, e1) = Syscall(SYS_MLOCK, (uintptr)_p0, (uintptr)len(b), 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Mlockall(nint flags) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_MLOCKALL, (uintptr)flags, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Msync(slice<byte> b, nint flags) {
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(b) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(b, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (_, _, e1) = Syscall(SYS_MSYNC, (uintptr)_p0, (uintptr)len(b), (uintptr)flags);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Munlock(slice<byte> b) {
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(b) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(b, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (_, _, e1) = Syscall(SYS_MUNLOCK, (uintptr)_p0, (uintptr)len(b), 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Munlockall() {
    error err = default;

    var (_, _, e1) = Syscall(SYS_MUNLOCKALL, 0, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ faccessat(nint dirfd, @string path, uint32 mode) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall(SYS_FACCESSAT, (uintptr)dirfd, (uintptr)_p0, (uintptr)mode);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Faccessat2(nint dirfd, @string path, uint32 mode, nint flags) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(path);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall6(SYS_FACCESSAT2, (uintptr)dirfd, (uintptr)_p0, (uintptr)mode, (uintptr)flags, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ nameToHandleAt(nint dirFD, @string pathname, ж<fileHandle> fh, ж<_C_int> mountID, nint flags) {
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = BytePtrFromString(pathname);
    if (err != nil) {
        return err;
    }
    var (_, _, e1) = Syscall6(SYS_NAME_TO_HANDLE_AT, (uintptr)dirFD, (uintptr)_p0, (uintptr)fh, (uintptr)mountID, (uintptr)flags, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static (nint fd, error err) openByHandleAt(nint mountFD, ж<fileHandle> fh, nint flags) {
    nint fd = default;
    error err = default;

    var (r0, _, e1) = Syscall(SYS_OPEN_BY_HANDLE_AT, (uintptr)mountFD, (uintptr)fh, (uintptr)flags);
    fd = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (fd, err);
}

public static (nint n, error err) ProcessVMReadv(nint pid, slice<Iovec> localIov, slice<RemoteIovec> remoteIov, nuint flags) {
    nint n = default;
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(localIov) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(localIov, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    @unsafe.Pointer _p1 = default;
    if (len(remoteIov) > 0) {
        _p1 = @unsafe.Pointer.FromPinnedBox(Ꮡ(remoteIov, 0));
    } else {
        _p1 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (r0, _, e1) = Syscall6(SYS_PROCESS_VM_READV, (uintptr)pid, (uintptr)_p0, (uintptr)len(localIov), (uintptr)_p1, (uintptr)len(remoteIov), (uintptr)flags);
    n = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

public static (nint n, error err) ProcessVMWritev(nint pid, slice<Iovec> localIov, slice<RemoteIovec> remoteIov, nuint flags) {
    nint n = default;
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(localIov) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(localIov, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    @unsafe.Pointer _p1 = default;
    if (len(remoteIov) > 0) {
        _p1 = @unsafe.Pointer.FromPinnedBox(Ꮡ(remoteIov, 0));
    } else {
        _p1 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (r0, _, e1) = Syscall6(SYS_PROCESS_VM_WRITEV, (uintptr)pid, (uintptr)_p0, (uintptr)len(localIov), (uintptr)_p1, (uintptr)len(remoteIov), (uintptr)flags);
    n = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

public static (nint fd, error err) PidfdOpen(nint pid, nint flags) {
    nint fd = default;
    error err = default;

    var (r0, _, e1) = Syscall(SYS_PIDFD_OPEN, (uintptr)pid, (uintptr)flags, 0);
    fd = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (fd, err);
}

public static (nint fd, error err) PidfdGetfd(nint pidfd, nint targetfd, nint flags) {
    nint fd = default;
    error err = default;

    var (r0, _, e1) = Syscall(SYS_PIDFD_GETFD, (uintptr)pidfd, (uintptr)targetfd, (uintptr)flags);
    fd = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (fd, err);
}

public static error /*err*/ PidfdSendSignal(nint pidfd, Signal sig, ж<Siginfo> info, nint flags) {
    error err = default;

    var (_, _, e1) = Syscall6(SYS_PIDFD_SEND_SIGNAL, (uintptr)pidfd, (uintptr)sig, (uintptr)info, (uintptr)flags, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static (uintptr ret, error err) shmat(nint id, uintptr addr, nint flag) {
    uintptr ret = default;
    error err = default;

    var (r0, _, e1) = Syscall(SYS_SHMAT, (uintptr)id, (uintptr)addr, (uintptr)flag);
    ret = (uintptr)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (ret, err);
}

internal static (nint result, error err) shmctl(nint id, nint cmd, ж<SysvShmDesc> buf) {
    nint result = default;
    error err = default;

    var (r0, _, e1) = Syscall(SYS_SHMCTL, (uintptr)id, (uintptr)cmd, (uintptr)buf);
    result = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (result, err);
}

internal static error /*err*/ shmdt(uintptr addr) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_SHMDT, (uintptr)addr, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static (nint id, error err) shmget(nint key, nint size, nint flag) {
    nint id = default;
    error err = default;

    var (r0, _, e1) = Syscall(SYS_SHMGET, (uintptr)key, (uintptr)size, (uintptr)flag);
    id = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (id, err);
}

internal static error /*err*/ getitimer(nint which, ж<Itimerval> currValue) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_GETITIMER, (uintptr)which, (uintptr)currValue, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ setitimer(nint which, ж<Itimerval> newValue, ж<Itimerval> oldValue) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_SETITIMER, (uintptr)which, (uintptr)newValue, (uintptr)oldValue);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ rtSigprocmask(nint how, ж<Sigset_t> set, ж<Sigset_t> oldset, uintptr sigsetsize) {
    error err = default;

    var (_, _, e1) = RawSyscall6(SYS_RT_SIGPROCMASK, (uintptr)how, (uintptr)set, (uintptr)oldset, (uintptr)sigsetsize, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static void getresuid(ж<_C_int> ruid, ж<_C_int> euid, ж<_C_int> suid) {
    RawSyscallNoError(SYS_GETRESUID, (uintptr)ruid, (uintptr)euid, (uintptr)suid);
    return;
}

internal static void getresgid(ж<_C_int> rgid, ж<_C_int> egid, ж<_C_int> sgid) {
    RawSyscallNoError(SYS_GETRESGID, (uintptr)rgid, (uintptr)egid, (uintptr)sgid);
    return;
}

internal static error /*err*/ schedSetattr(nint pid, ж<SchedAttr> attr, nuint flags) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_SCHED_SETATTR, (uintptr)pid, (uintptr)attr, (uintptr)flags);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ schedGetattr(nint pid, ж<SchedAttr> attr, nuint size, nuint flags) {
    error err = default;

    var (_, _, e1) = Syscall6(SYS_SCHED_GETATTR, (uintptr)pid, (uintptr)attr, (uintptr)size, (uintptr)flags, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Cachestat(nuint fd, ж<CachestatRange> crange, ж<Cachestat_t> cstat, nuint flags) {
    error err = default;

    var (_, _, e1) = Syscall6(SYS_CACHESTAT, (uintptr)fd, (uintptr)crange, (uintptr)cstat, (uintptr)flags, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Mseal(slice<byte> b, nuint flags) {
    error err = default;

    @unsafe.Pointer _p0 = default;
    if (len(b) > 0) {
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(b, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var (_, _, e1) = Syscall(SYS_MSEAL, (uintptr)_p0, (uintptr)len(b), (uintptr)flags);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ setMemPolicy(nint mode, @unsafe.Pointer mask, uintptr size) {
    error err = default;

    var (_, _, e1) = Syscall(SYS_SET_MEMPOLICY, (uintptr)mode, (uintptr)mask, (uintptr)size);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

} // end unix_package
