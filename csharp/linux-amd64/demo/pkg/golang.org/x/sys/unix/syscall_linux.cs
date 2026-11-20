// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using binary = encoding.binary_package;
using slices = slices_package;
using strconv = strconv_package;
using syscall = syscall_package;
using time = time_package;
using @unsafe = unsafe_package;
using encoding;
using static unix_package;

partial class unix_package {

public static error /*err*/ Access(@string path, uint32 mode) {
    return Faccessat(AT_FDCWD, path, mode, 0);
}

public static error /*err*/ Chmod(@string path, uint32 mode) {
    return Fchmodat(AT_FDCWD, path, mode, 0);
}

public static error /*err*/ Chown(@string path, nint uid, nint gid) {
    return Fchownat(AT_FDCWD, path, uid, gid, 0);
}

public static (nint fd, error err) Creat(@string path, uint32 mode) {
    return Open(path, (nint)((nint)(UntypedInt)(O_CREAT | O_WRONLY) | (nint)O_TRUNC), mode);
}

public static (nint fd, error err) EpollCreate(nint size) {
    if (size <= 0) {
        return (-1, EINVAL);
    }
    return EpollCreate1(0);
}

public static error /*err*/ FanotifyMark(nint fd, nuint flags, uint64 mask, nint dirFd, @string pathname) {
    error err = default;

    if (pathname == ""u8) {
        return fanotifyMark(fd, flags, mask, dirFd, nil);
    }
    (var p, err) = BytePtrFromString(pathname);
    if (err != nil) {
        return err;
    }
    return fanotifyMark(fd, flags, mask, dirFd, p);
}

public static error Fchmodat(nint dirfd, @string path, uint32 mode, nint flags) {
    if (flags != 0) {
        var err = fchmodat2(dirfd, path, mode, flags);
        if (AreEqual(err, ENOSYS)) {
            if ((flags & ~(nint)((nint)((nint)AT_SYMLINK_NOFOLLOW | (nint)AT_EMPTY_PATH))) != 0) {
                return EINVAL;
            } else if ((flags & (nint)((nint)((nint)AT_SYMLINK_NOFOLLOW | (nint)AT_EMPTY_PATH))) != 0) {
                return EOPNOTSUPP;
            }
        }
        return err;
    }
    return fchmodat(dirfd, path, mode);
}

public static (nint fd, error err) InotifyInit() {
    return InotifyInit1(0);
}

public static error /*err*/ Link(@string oldpath, @string newpath) {
    return Linkat(AT_FDCWD, oldpath, AT_FDCWD, newpath, 0);
}

public static error /*err*/ Mkdir(@string path, uint32 mode) {
    return Mkdirat(AT_FDCWD, path, mode);
}

public static error /*err*/ Mknod(@string path, uint32 mode, nint dev) {
    return Mknodat(AT_FDCWD, path, mode, dev);
}

public static (nint fd, error err) Open(@string path, nint mode, uint32 perm) {
    return openat(AT_FDCWD, path, mode | (nint)O_LARGEFILE, perm);
}

public static (nint fd, error err) Openat(nint dirfd, @string path, nint flags, uint32 mode) {
    return openat(dirfd, path, flags | (nint)O_LARGEFILE, mode);
}

public static (nint fd, error err) Openat2(nint dirfd, @string path, ж<OpenHow> how) {
    return openat2(dirfd, path, how, SizeofOpenHow);
}

public static error Pipe(slice<nint> p) {
    return Pipe2(p, 0);
}

public static error Pipe2(slice<nint> p, nint flags) {
    if (len(p) != 2) {
        return EINVAL;
    }
    ref var pp = ref heap(new array<_C_int>(2), out var Ꮡpp);
    var err = pipe2(Ꮡpp, flags);
    if (err == nil) {
        p[0] = (nint)pp[0];
        p[1] = (nint)pp[1];
    }
    return err;
}

public static (nint n, error err) Ppoll(slice<PollFd> fds, ж<Timespec> timeout, ж<Sigset_t> sigmask) {
    if (len(fds) == 0) {
        return ppoll(nil, 0, timeout, sigmask);
    }
    return ppoll(Ꮡ(fds, 0), len(fds), timeout, sigmask);
}

public static (nint n, error err) Poll(slice<PollFd> fds, nint timeout) {
    ж<Timespec> ts = default;
    if (timeout >= 0) {
        ts = @new<Timespec>();
        ts.Value = NsecToTimespec((int64)timeout * 1000000);
    }
    return Ppoll(fds, ts, nil);
}

public static (nint n, error err) Readlink(@string path, slice<byte> buf) {
    return Readlinkat(AT_FDCWD, path, buf);
}

public static error /*err*/ Rename(@string oldpath, @string newpath) {
    return Renameat(AT_FDCWD, oldpath, AT_FDCWD, newpath);
}

public static error Rmdir(@string path) {
    return Unlinkat(AT_FDCWD, path, AT_REMOVEDIR);
}

public static error /*err*/ Symlink(@string oldpath, @string newpath) {
    return Symlinkat(oldpath, AT_FDCWD, newpath);
}

public static error Unlink(@string path) {
    return Unlinkat(AT_FDCWD, path, 0);
}

public static error Utimes(@string path, slice<Timeval> tv) {
    if (tv == nil) {
        var errΔ1 = utimensat(AT_FDCWD, path, ж<array<Timespec>>.NilBoxOfDims(2L), 0);
        if (!AreEqual(errΔ1, ENOSYS)) {
            return errΔ1;
        }
        return utimes(path, ж<array<Timeval>>.NilBoxOfDims(2L));
    }
    if (len(tv) != 2) {
        return EINVAL;
    }
    ref var ts = ref heap(new array<Timespec>(2), out var Ꮡts);
    ts[0] = NsecToTimespec(TimevalToNsec(tv[0]));
    ts[1] = NsecToTimespec(TimevalToNsec(tv[1]));
    var err = utimensat(AT_FDCWD, path, array<Timespec>.AliasPointer(Ꮡts.at<Timespec>(0), 2), 0);
    if (!AreEqual(err, ENOSYS)) {
        return err;
    }
    return utimes(path, array<Timeval>.AliasPointer(Ꮡ(tv, 0), 2));
}

public static error UtimesNano(@string path, slice<Timespec> ts) {
    return UtimesNanoAt(AT_FDCWD, path, ts, 0);
}

public static error UtimesNanoAt(nint dirfd, @string path, slice<Timespec> ts, nint flags) {
    if (ts == nil) {
        return utimensat(dirfd, path, ж<array<Timespec>>.NilBoxOfDims(2L), flags);
    }
    if (len(ts) != 2) {
        return EINVAL;
    }
    return utimensat(dirfd, path, array<Timespec>.AliasPointer(Ꮡ(ts, 0), 2), flags);
}

public static error Futimesat(nint dirfd, @string path, slice<Timeval> tv) {
    if (tv == nil) {
        return futimesat(dirfd, path, ж<array<Timeval>>.NilBoxOfDims(2L));
    }
    if (len(tv) != 2) {
        return EINVAL;
    }
    return futimesat(dirfd, path, array<Timeval>.AliasPointer(Ꮡ(tv, 0), 2));
}

public static error /*err*/ Futimes(nint fd, slice<Timeval> tv) {
    return Utimes("/proc/self/fd/"u8 + strconv.Itoa(fd), tv);
}

public const bool ImplementsGetwd = true;

public static (@string wd, error err) Getwd() {
    error err = default;

    array<byte> buf = new(4096); /* PathMax */
    (var n, err) = Getcwd(buf[0..]);
    if (err != nil) {
        return ("", err);
    }
    if (n < 1 || n > len(buf) || buf[n - 1] != 0) {
        return ("", EINVAL);
    }
    if (buf[0] != '/') {
        return ("", ENOENT);
    }

    return ((@string)(buf.slice(0, n - 1)), default);
}

public static (slice<nint> gids, error err) Getgroups() {
    slice<nint> gids = default;
    error err = default;

    (var n, err) = getgroups(0, nil);
    if (err != nil) {
        return (default, err);
    }
    if (n == 0) {
        return (default, default);
    }

    if (n < 0 || n > 1 << 20) {
        return (default, EINVAL);
    }

    var a = new slice<_Gid_t>(n);
    (n, err) = getgroups(n, Ꮡ(a, 0));
    if (err != nil) {
        return (default, err);
    }
    gids = new slice<nint>(n);
    foreach (var (i, v) in range(a.slice(0, n))) {
        gids[i] = (nint)v;
    }
    return (gids, err);
}

public static error /*err*/ Setgroups(slice<nint> gids) {
    if (len(gids) == 0) {
        return setgroups(0, nil);
    }

    var a = new slice<_Gid_t>(len(gids));
    foreach (var (i, v) in range(gids)) {
        a[i] = (_Gid_t)(uint32)v;
    }
    return setgroups(len(a), Ꮡ(a, 0));
}

partial struct WaitStatus /*num:uint32*/;

internal static UntypedInt mask => 0x7F;
internal static UntypedInt core => 0x80;
internal static UntypedInt exited => 0x00;
internal static UntypedInt stopped => 0x7F;
internal static UntypedInt shift => 8;

} partial class unixᴛWaitStatus {

public static bool Exited(this WaitStatus w) { return (WaitStatus)(w & (uint32)mask) == exited; }

public static bool Signaled(this WaitStatus w) { return (WaitStatus)(w & (uint32)mask) != stopped && (WaitStatus)(w & (uint32)mask) != exited; }

public static bool Stopped(this WaitStatus w) { return (WaitStatus)(w & 0xFF) == stopped; }

public static bool Continued(this WaitStatus w) { return w == 0xFFFF; }

public static bool CoreDump(this WaitStatus w) { return w.Signaled() && (WaitStatus)(w & (uint32)core) != 0; }

public static nint ExitStatus(this WaitStatus w) {
    if (!w.Exited()) {
        return -1;
    }
    return (nint)(w >> (int)(shift)) & 0xFF;
}

public static syscall.Signal Signal(this WaitStatus w) {
    if (!w.Signaled()) {
        return -1;
    }
    return (syscall.Signal)(nint)((WaitStatus)(w & (uint32)mask));
}

public static syscall.Signal StopSignal(this WaitStatus w) {
    if (!w.Stopped()) {
        return -1;
    }
    return (syscall.Signal)(((syscall.Signal)(nint)((w >> (int)(shift)))) & 0xFF);
}

public static nint TrapCause(this WaitStatus w) {
    if (w.StopSignal() != SIGTRAP) {
        return -1;
    }
    return (nint)(w >> (int)(shift)) >> 8;
}

} partial class unix_package {

public static (nint wpid, error err) Wait4(nint pid, ж<WaitStatus> Ꮡwstatus, nint options, ж<Rusage> rusage) {
    nint wpid = default;
    error err = default;
    ref var wstatus = ref Ꮡwstatus.DerefOrNull();

    ref var status = ref heap(new _C_int(), out var Ꮡstatus);
    (wpid, err) = wait4(pid, Ꮡstatus, options, rusage);
    if (Ꮡwstatus != nil) {
        wstatus = (WaitStatus)(uint32)status;
    }
    return (wpid, err);
}

public static error Mkfifo(@string path, uint32 mode) {
    return Mknod(path, mode | (uint32)S_IFIFO, 0);
}

public static error Mkfifoat(nint dirfd, @string path, uint32 mode) {
    return Mknodat(dirfd, path, mode | (uint32)S_IFIFO, 0);
}

} partial class unixᴛSockaddrInet4 {

internal static (@unsafe.Pointer, _Socklen, error) sockaddr(this ж<SockaddrInet4> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    if (sa.Port < 0 || sa.Port > 0xFFFF) {
        return (default, 0, EINVAL);
    }
    sa.raw.Family = AF_INET;
    var p = @unsafe.ArrayPointer<byte>.Of(Ꮡsa.Ꮡraw.ᏑPort, 2);
    p.ElementRef(0) = (byte)(sa.Port >> 8);
    p.ElementRef(1) = (byte)sa.Port;
    sa.raw.Addr.Assign(sa.Addr);
    return (@unsafe.Pointer.FromPinnedBox(Ꮡsa.Ꮡraw), SizeofSockaddrInet4, default);
}

} partial class unixᴛSockaddrInet6 {

internal static (@unsafe.Pointer, _Socklen, error) sockaddr(this ж<SockaddrInet6> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    if (sa.Port < 0 || sa.Port > 0xFFFF) {
        return (default, 0, EINVAL);
    }
    sa.raw.Family = AF_INET6;
    var p = @unsafe.ArrayPointer<byte>.Of(Ꮡsa.Ꮡraw.ᏑPort, 2);
    p.ElementRef(0) = (byte)(sa.Port >> 8);
    p.ElementRef(1) = (byte)sa.Port;
    sa.raw.Scope_id = sa.ZoneId;
    sa.raw.Addr.Assign(sa.Addr);
    return (@unsafe.Pointer.FromPinnedBox(Ꮡsa.Ꮡraw), SizeofSockaddrInet6, default);
}

} partial class unixᴛSockaddrUnix {

internal static (@unsafe.Pointer, _Socklen, error) sockaddr(this ж<SockaddrUnix> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    @string name = sa.Name;
    nint n = len(name);
    if (n >= len(sa.raw.Path)) {
        return (default, 0, EINVAL);
    }
    sa.raw.Family = AF_UNIX;
    foreach (var i in range(n)) {
        sa.raw.Path[i] = (int8)name[i];
    }
    var sl = (_Socklen)2;
    if (n > 0) {
        sl += ((_Socklen)(uint32)n) + 1;
    }
    if (sa.raw.Path[0] == '@' || (sa.raw.Path[0] == 0 && sl > 3)) {
        sa.raw.Path[0] = 0;
        sl--;
    }

    return (@unsafe.Pointer.FromPinnedBox(Ꮡsa.Ꮡraw), sl, default);
}

} partial class unix_package {

partial struct SockaddrLinklayer {
    public uint16 Protocol;
    public nint Ifindex;
    public uint16 Hatype;
    public uint8 Pkttype;
    public uint8 Halen;
    public array<byte> Addr = new(8);
    internal RawSockaddrLinklayer raw;
}

} partial class unixᴛSockaddrLinklayer {

internal static (@unsafe.Pointer, _Socklen, error) sockaddr(this ж<SockaddrLinklayer> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    if (sa.Ifindex < 0 || sa.Ifindex > 0x7fffffff) {
        return (default, 0, EINVAL);
    }
    sa.raw.Family = AF_PACKET;
    sa.raw.Protocol = sa.Protocol;
    sa.raw.Ifindex = (int32)sa.Ifindex;
    sa.raw.Hatype = sa.Hatype;
    sa.raw.Pkttype = sa.Pkttype;
    sa.raw.Halen = sa.Halen;
    sa.raw.Addr.Assign(sa.Addr);
    return (@unsafe.Pointer.FromPinnedBox(Ꮡsa.Ꮡraw), SizeofSockaddrLinklayer, default);
}

} partial class unix_package {

partial struct SockaddrNetlink {
    public uint16 Family;
    public uint16 Pad;
    public uint32 Pid;
    public uint32 Groups;
    internal RawSockaddrNetlink raw;
}

} partial class unixᴛSockaddrNetlink {

internal static (@unsafe.Pointer, _Socklen, error) sockaddr(this ж<SockaddrNetlink> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    sa.raw.Family = AF_NETLINK;
    sa.raw.Pad = sa.Pad;
    sa.raw.Pid = sa.Pid;
    sa.raw.Groups = sa.Groups;
    return (@unsafe.Pointer.FromPinnedBox(Ꮡsa.Ꮡraw), SizeofSockaddrNetlink, default);
}

} partial class unix_package {

partial struct SockaddrHCI {
    public uint16 Dev;
    public uint16 Channel;
    internal RawSockaddrHCI raw;
}

} partial class unixᴛSockaddrHCI {

internal static (@unsafe.Pointer, _Socklen, error) sockaddr(this ж<SockaddrHCI> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    sa.raw.Family = AF_BLUETOOTH;
    sa.raw.Dev = sa.Dev;
    sa.raw.Channel = sa.Channel;
    return (@unsafe.Pointer.FromPinnedBox(Ꮡsa.Ꮡraw), SizeofSockaddrHCI, default);
}

} partial class unix_package {

partial struct SockaddrL2 {
    public uint16 PSM;
    public uint16 CID;
    public array<uint8> Addr = new(6);
    public uint8 AddrType;
    internal RawSockaddrL2 raw;
}

} partial class unixᴛSockaddrL2 {

internal static (@unsafe.Pointer, _Socklen, error) sockaddr(this ж<SockaddrL2> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    sa.raw.Family = AF_BLUETOOTH;
    var psm = @unsafe.ArrayPointer<byte>.Of(Ꮡsa.Ꮡraw.ᏑPsm, 2);
    psm.ElementRef(0) = (byte)sa.PSM;
    psm.ElementRef(1) = (byte)(sa.PSM >> 8);
    foreach (var i in range(len(sa.Addr))) {
        sa.raw.Bdaddr[i] = sa.Addr[len(sa.Addr) - 1 - i];
    }
    var cid = @unsafe.ArrayPointer<byte>.Of(Ꮡsa.Ꮡraw.ᏑCid, 2);
    cid.ElementRef(0) = (byte)sa.CID;
    cid.ElementRef(1) = (byte)(sa.CID >> 8);
    sa.raw.Bdaddr_type = sa.AddrType;
    return (@unsafe.Pointer.FromPinnedBox(Ꮡsa.Ꮡraw), SizeofSockaddrL2, default);
}

} partial class unix_package {

partial struct SockaddrRFCOMM {
    public array<uint8> Addr = new(6);
    public uint8 Channel;
    internal RawSockaddrRFCOMM raw;
}

} partial class unixᴛSockaddrRFCOMM {

internal static (@unsafe.Pointer, _Socklen, error) sockaddr(this ж<SockaddrRFCOMM> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    sa.raw.Family = AF_BLUETOOTH;
    sa.raw.Channel = sa.Channel;
    sa.raw.Bdaddr.Assign(sa.Addr);
    return (@unsafe.Pointer.FromPinnedBox(Ꮡsa.Ꮡraw), SizeofSockaddrRFCOMM, default);
}

} partial class unix_package {

partial struct SockaddrCAN {
    public nint Ifindex;
    public uint32 RxID;
    public uint32 TxID;
    internal RawSockaddrCAN raw;
}

} partial class unixᴛSockaddrCAN {

internal static (@unsafe.Pointer, _Socklen, error) sockaddr(this ж<SockaddrCAN> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    if (sa.Ifindex < 0 || sa.Ifindex > 0x7fffffff) {
        return (default, 0, EINVAL);
    }
    sa.raw.Family = AF_CAN;
    sa.raw.Ifindex = (int32)sa.Ifindex;
    var rx = @unsafe.ArrayPointer<byte>.Of(Ꮡsa.ᏑRxID, 4);
    foreach (var i in range(4)) {
        sa.raw.Addr[i] = rx.ElementRef(i);
    }
    var tx = @unsafe.ArrayPointer<byte>.Of(Ꮡsa.ᏑTxID, 4);
    foreach (var i in range(4)) {
        sa.raw.Addr[i + 4] = tx.ElementRef(i);
    }
    return (@unsafe.Pointer.FromPinnedBox(Ꮡsa.Ꮡraw), SizeofSockaddrCAN, default);
}

} partial class unix_package {

partial struct SockaddrCANJ1939 {
    public nint Ifindex;
    public uint64 Name;
    public uint32 PGN;
    public uint8 Addr;
    internal RawSockaddrCAN raw;
}

} partial class unixᴛSockaddrCANJ1939 {

internal static (@unsafe.Pointer, _Socklen, error) sockaddr(this ж<SockaddrCANJ1939> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    if (sa.Ifindex < 0 || sa.Ifindex > 0x7fffffff) {
        return (default, 0, EINVAL);
    }
    sa.raw.Family = AF_CAN;
    sa.raw.Ifindex = (int32)sa.Ifindex;
    var n = @unsafe.ArrayPointer<byte>.Of(Ꮡsa.ᏑName, 8);
    foreach (var i in range(8)) {
        sa.raw.Addr[i] = n.ElementRef(i);
    }
    var p = @unsafe.ArrayPointer<byte>.Of(Ꮡsa.ᏑPGN, 4);
    foreach (var i in range(4)) {
        sa.raw.Addr[i + 8] = p.ElementRef(i);
    }
    sa.raw.Addr[12] = sa.Addr;
    return (@unsafe.Pointer.FromPinnedBox(Ꮡsa.Ꮡraw), SizeofSockaddrCAN, default);
}

} partial class unix_package {

partial struct SockaddrALG {
    public @string Type;
    public @string Name;
    public uint32 Feature;
    public uint32 Mask;
    internal RawSockaddrALG raw;
}

} partial class unixᴛSockaddrALG {

internal static (@unsafe.Pointer, _Socklen, error) sockaddr(this ж<SockaddrALG> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    if (len(sa.Type) > len(sa.raw.Type) - 1) {
        return (default, 0, EINVAL);
    }
    if (len(sa.Name) > len(sa.raw.Name) - 1) {
        return (default, 0, EINVAL);
    }

    sa.raw.Family = AF_ALG;
    sa.raw.Feat = sa.Feature;
    sa.raw.Mask = sa.Mask;

    copy(sa.raw.Type[..], sa.Type);
    copy(sa.raw.Name[..], sa.Name);

    return (@unsafe.Pointer.FromPinnedBox(Ꮡsa.Ꮡraw), SizeofSockaddrALG, default);
}

} partial class unix_package {

partial struct SockaddrVM {
    public uint32 CID;
    public uint32 Port;
    public uint8 Flags;
    internal RawSockaddrVM raw;
}

} partial class unixᴛSockaddrVM {

internal static (@unsafe.Pointer, _Socklen, error) sockaddr(this ж<SockaddrVM> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    sa.raw.Family = AF_VSOCK;
    sa.raw.Port = sa.Port;
    sa.raw.Cid = sa.CID;
    sa.raw.Flags = sa.Flags;

    return (@unsafe.Pointer.FromPinnedBox(Ꮡsa.Ꮡraw), SizeofSockaddrVM, default);
}

} partial class unix_package {

partial struct SockaddrXDP {
    public uint16 Flags;
    public uint32 Ifindex;
    public uint32 QueueID;
    public uint32 SharedUmemFD;
    internal RawSockaddrXDP raw;
}

} partial class unixᴛSockaddrXDP {

internal static (@unsafe.Pointer, _Socklen, error) sockaddr(this ж<SockaddrXDP> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    sa.raw.Family = AF_XDP;
    sa.raw.Flags = sa.Flags;
    sa.raw.Ifindex = sa.Ifindex;
    sa.raw.Queue_id = sa.QueueID;
    sa.raw.Shared_umem_fd = sa.SharedUmemFD;

    return (@unsafe.Pointer.FromPinnedBox(Ꮡsa.Ꮡraw), SizeofSockaddrXDP, default);
}

} partial class unix_package {

internal static UntypedInt px_proto_oe => 0;

partial struct SockaddrPPPoE {
    public uint16 SID;
    public slice<byte> Remote;
    public @string Dev;
    internal RawSockaddrPPPoX raw;
}

} partial class unixᴛSockaddrPPPoE {

internal static (@unsafe.Pointer, _Socklen, error) sockaddr(this ж<SockaddrPPPoE> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    if (len(sa.Remote) != 6) {
        return (default, 0, EINVAL);
    }
    if (len(sa.Dev) > (nint)(IFNAMSIZ - 1)) {
        return (default, 0, EINVAL);
    }

    (Ꮡ(sa.raw.Value, 0).Reinterpret<byte, uint16>()).Value = AF_PPPOX;
    binary.BigEndian.PutUint32(sa.raw[2..6], px_proto_oe);
    binary.BigEndian.PutUint16(sa.raw[6..8], sa.SID);
    copy(sa.raw[8..14], sa.Remote);
    clear(sa.raw[14..(int)(14 + IFNAMSIZ)]);
    copy(sa.raw[14..], sa.Dev);
    return (@unsafe.Pointer.FromPinnedBox(Ꮡsa.Ꮡraw), SizeofSockaddrPPPoX, default);
}

} partial class unix_package {

partial struct SockaddrTIPC {
    public nint Scope;
    public TIPCAddr Addr;
    internal RawSockaddrTIPC raw;
}

partial interface TIPCAddr {
    uint8 tipcAddrtype();
    array<byte> tipcAddr();
}

} partial class unixᴛTIPCSocketAddr {

internal static array<byte> tipcAddr(this ж<TIPCSocketAddr> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    array<byte> @out = new(12);
    copy(@out[..], (~@unsafe.Pointer.To<array<byte>>(@unsafe.Pointer.FromRef(ref sa)))[..]);
    return @out.Clone();
}

internal static uint8 tipcAddrtype(this ref TIPCSocketAddr sa) { return TIPC_SOCKET_ADDR; }

} partial class unixᴛTIPCServiceRange {

internal static array<byte> tipcAddr(this ж<TIPCServiceRange> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    array<byte> @out = new(12);
    copy(@out[..], (~@unsafe.Pointer.To<array<byte>>(@unsafe.Pointer.FromRef(ref sa)))[..]);
    return @out.Clone();
}

internal static uint8 tipcAddrtype(this ref TIPCServiceRange sa) { return TIPC_SERVICE_RANGE; }

} partial class unixᴛTIPCServiceName {

internal static array<byte> tipcAddr(this ж<TIPCServiceName> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    array<byte> @out = new(12);
    copy(@out[..], (~@unsafe.Pointer.To<array<byte>>(@unsafe.Pointer.FromRef(ref sa)))[..]);
    return @out.Clone();
}

internal static uint8 tipcAddrtype(this ref TIPCServiceName sa) { return TIPC_SERVICE_ADDR; }

} partial class unixᴛSockaddrTIPC {

internal static (@unsafe.Pointer, _Socklen, error) sockaddr(this ж<SockaddrTIPC> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    if (sa.Addr == nil) {
        return (default, 0, EINVAL);
    }
    sa.raw.Family = AF_TIPC;
    sa.raw.Scope = (int8)sa.Scope;
    sa.raw.Addrtype = sa.Addr.tipcAddrtype();
    sa.raw.Addr.Assign(sa.Addr.tipcAddr());
    return (@unsafe.Pointer.FromPinnedBox(Ꮡsa.Ꮡraw), SizeofSockaddrTIPC, default);
}

} partial class unix_package {

partial struct SockaddrL2TPIP {
    public array<byte> Addr = new(4);
    public uint32 ConnId;
    internal RawSockaddrL2TPIP raw;
}

} partial class unixᴛSockaddrL2TPIP {

internal static (@unsafe.Pointer, _Socklen, error) sockaddr(this ж<SockaddrL2TPIP> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    sa.raw.Family = AF_INET;
    sa.raw.Conn_id = sa.ConnId;
    sa.raw.Addr.Assign(sa.Addr);
    return (@unsafe.Pointer.FromPinnedBox(Ꮡsa.Ꮡraw), SizeofSockaddrL2TPIP, default);
}

} partial class unix_package {

partial struct SockaddrL2TPIP6 {
    public array<byte> Addr = new(16);
    public uint32 ZoneId;
    public uint32 ConnId;
    internal RawSockaddrL2TPIP6 raw;
}

} partial class unixᴛSockaddrL2TPIP6 {

internal static (@unsafe.Pointer, _Socklen, error) sockaddr(this ж<SockaddrL2TPIP6> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    sa.raw.Family = AF_INET6;
    sa.raw.Conn_id = sa.ConnId;
    sa.raw.Scope_id = sa.ZoneId;
    sa.raw.Addr.Assign(sa.Addr);
    return (@unsafe.Pointer.FromPinnedBox(Ꮡsa.Ꮡraw), SizeofSockaddrL2TPIP6, default);
}

} partial class unix_package {

partial struct SockaddrIUCV {
    public @string UserID;
    public @string Name;
    internal RawSockaddrIUCV raw;
}

} partial class unixᴛSockaddrIUCV {

internal static (@unsafe.Pointer, _Socklen, error) sockaddr(this ж<SockaddrIUCV> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    sa.raw.Family = AF_IUCV;
    foreach (var i in range(8)) {
        sa.raw.Nodeid[i] = (int8)' ';
        sa.raw.User_id[i] = (int8)' ';
        sa.raw.Name[i] = (int8)' ';
    }
    if (len(sa.UserID) > 8 || len(sa.Name) > 8) {
        return (default, 0, EINVAL);
    }
    foreach (var (i, b) in range(slice<byte>(sa.UserID[..]))) {
        sa.raw.User_id[i] = (int8)b;
    }
    foreach (var (i, b) in range(slice<byte>(sa.Name[..]))) {
        sa.raw.Name[i] = (int8)b;
    }
    return (@unsafe.Pointer.FromPinnedBox(Ꮡsa.Ꮡraw), SizeofSockaddrIUCV, default);
}

} partial class unix_package {

partial struct SockaddrNFC {
    public uint32 DeviceIdx;
    public uint32 TargetIdx;
    public uint32 NFCProtocol;
    internal RawSockaddrNFC raw;
}

} partial class unixᴛSockaddrNFC {

internal static (@unsafe.Pointer, _Socklen, error) sockaddr(this ж<SockaddrNFC> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    sa.raw.Sa_family = AF_NFC;
    sa.raw.Dev_idx = sa.DeviceIdx;
    sa.raw.Target_idx = sa.TargetIdx;
    sa.raw.Nfc_protocol = sa.NFCProtocol;
    return (@unsafe.Pointer.FromPinnedBox(Ꮡsa.Ꮡraw), SizeofSockaddrNFC, default);
}

} partial class unix_package {

partial struct SockaddrNFCLLCP {
    public uint32 DeviceIdx;
    public uint32 TargetIdx;
    public uint32 NFCProtocol;
    public uint8 DestinationSAP;
    public uint8 SourceSAP;
    public @string ServiceName;
    internal RawSockaddrNFCLLCP raw;
}

} partial class unixᴛSockaddrNFCLLCP {

internal static (@unsafe.Pointer, _Socklen, error) sockaddr(this ж<SockaddrNFCLLCP> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    sa.raw.Sa_family = AF_NFC;
    sa.raw.Dev_idx = sa.DeviceIdx;
    sa.raw.Target_idx = sa.TargetIdx;
    sa.raw.Nfc_protocol = sa.NFCProtocol;
    sa.raw.Dsap = sa.DestinationSAP;
    sa.raw.Ssap = sa.SourceSAP;
    if (len(sa.ServiceName) > len(sa.raw.Service_name)) {
        return (default, 0, EINVAL);
    }
    copy(sa.raw.Service_name[..], sa.ServiceName);
    sa.raw.SetServiceNameLen(len(sa.ServiceName));
    return (@unsafe.Pointer.FromPinnedBox(Ꮡsa.Ꮡraw), SizeofSockaddrNFCLLCP, default);
}

} partial class unix_package {

internal static Func<nint, (nint, error)> socketProtocol;
internal static void initᴛsocketProtocol() { socketProtocol = (nint fd) => GetsockoptInt(fd, SOL_SOCKET, SO_PROTOCOL); }

internal static (Sockaddr, error) anyToSockaddr(nint fd, ж<RawSockaddrAny> Ꮡrsa) {
    ref var rsa = ref Ꮡrsa.DerefOrNull();

    switch (rsa.Addr.Family) {
    case var ᴛ when ᴛ == AF_NETLINK: {
        var pp = Ꮡrsa.Reinterpret<RawSockaddrAny, RawSockaddrNetlink>();
        var sa = @new<SockaddrNetlink>();
        sa.Family = pp.Family;
        sa.Pad = pp.Pad;
        sa.Pid = pp.Pid;
        sa.Groups = pp.Groups;
        return (new SockaddrNetlinkжSockaddr(sa), default);
    }
    case var ᴛ when ᴛ == AF_PACKET: {
        var pp = Ꮡrsa.Reinterpret<RawSockaddrAny, RawSockaddrLinklayer>();
        var sa = @new<SockaddrLinklayer>();
        sa.Protocol = pp.Protocol;
        sa.Ifindex = (nint)pp.Ifindex;
        sa.Hatype = pp.Hatype;
        sa.Pkttype = pp.Pkttype;
        sa.Halen = pp.Halen;
        sa.Addr.Assign(pp.Addr);
        return (new SockaddrLinklayerжSockaddr(sa), default);
    }
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
        var (proto, err) = socketProtocol(fd);
        if (err != nil) {
            return (default, err);
        }

        switch (proto) {
        case var ᴛ2 when ᴛ2 == IPPROTO_L2TP: {
            var pp = Ꮡrsa.Reinterpret<RawSockaddrAny, RawSockaddrL2TPIP>();
            var sa = @new<SockaddrL2TPIP>();
            sa.ConnId = pp.Conn_id;
            sa.Addr.Assign(pp.Addr);
            return (new SockaddrL2TPIPжSockaddr(sa), default);
        }
        default: {
            var pp = Ꮡrsa.Reinterpret<RawSockaddrAny, RawSockaddrInet4>();
            var sa = @new<SockaddrInet4>();
            var p = @unsafe.ArrayPointer<byte>.Of(pp.ᏑPort, 2);
            sa.Port = ((nint)p.ElementRef(0) << 8) + (nint)p.ElementRef(1);
            sa.Addr.Assign(pp.Addr);
            return (new SockaddrInet4жSockaddr(sa), default);
        }}

        break;
    }
    case var ᴛ when ᴛ == AF_INET6: {
        var (proto, err) = socketProtocol(fd);
        if (err != nil) {
            return (default, err);
        }

        switch (proto) {
        case var ᴛ2 when ᴛ2 == IPPROTO_L2TP: {
            var pp = Ꮡrsa.Reinterpret<RawSockaddrAny, RawSockaddrL2TPIP6>();
            var sa = @new<SockaddrL2TPIP6>();
            sa.ConnId = pp.Conn_id;
            sa.ZoneId = pp.Scope_id;
            sa.Addr.Assign(pp.Addr);
            return (new SockaddrL2TPIP6жSockaddr(sa), default);
        }
        default: {
            var pp = Ꮡrsa.Reinterpret<RawSockaddrAny, RawSockaddrInet6>();
            var sa = @new<SockaddrInet6>();
            var p = @unsafe.ArrayPointer<byte>.Of(pp.ᏑPort, 2);
            sa.Port = ((nint)p.ElementRef(0) << 8) + (nint)p.ElementRef(1);
            sa.ZoneId = pp.Scope_id;
            sa.Addr.Assign(pp.Addr);
            return (new SockaddrInet6жSockaddr(sa), default);
        }}

        break;
    }
    case var ᴛ when ᴛ == AF_VSOCK: {
        var pp = Ꮡrsa.Reinterpret<RawSockaddrAny, RawSockaddrVM>();
        var sa = Ꮡ(new SockaddrVM(
            CID: pp.Cid,
            Port: pp.Port,
            Flags: pp.Flags
        ));
        return (new SockaddrVMжSockaddr(sa), default);
    }
    case var ᴛ when ᴛ == AF_BLUETOOTH: {
        var (proto, err) = socketProtocol(fd);
        if (err != nil) {
            return (default, err);
        }
        switch (proto) {
        case var ᴛ2 when ᴛ2 == BTPROTO_L2CAP: {
            var pp = Ꮡrsa.Reinterpret<RawSockaddrAny, RawSockaddrL2>();
            var sa = Ꮡ(new SockaddrL2(
                PSM: pp.Psm,
                CID: pp.Cid,
                Addr: pp.Bdaddr.Clone(),
                AddrType: pp.Bdaddr_type
            ));
            return (new SockaddrL2жSockaddr(sa), default);
        }
        case var ᴛ2 when ᴛ2 == BTPROTO_RFCOMM: {
            var pp = Ꮡrsa.Reinterpret<RawSockaddrAny, RawSockaddrRFCOMM>();
            var sa = Ꮡ(new SockaddrRFCOMM(
                Channel: pp.Channel,
                Addr: pp.Bdaddr.Clone()
            ));
            return (new SockaddrRFCOMMжSockaddr(sa), default);
        }}

        break;
    }
    case var ᴛ when ᴛ == AF_XDP: {
        var pp = Ꮡrsa.Reinterpret<RawSockaddrAny, RawSockaddrXDP>();
        var sa = Ꮡ(new SockaddrXDP(
            Flags: pp.Flags,
            Ifindex: pp.Ifindex,
            QueueID: pp.Queue_id,
            SharedUmemFD: pp.Shared_umem_fd
        ));
        return (new SockaddrXDPжSockaddr(sa), default);
    }
    case var ᴛ when ᴛ == AF_PPPOX: {
        var pp = @unsafe.Pointer.To<RawSockaddrPPPoX>(@unsafe.Pointer.FromPinnedBox(Ꮡrsa));
        if (binary.BigEndian.Uint32(pp.Value[2..6]) != px_proto_oe) {
            return (default, EINVAL);
        }
        var sa = Ꮡ(new SockaddrPPPoE(
            SID: binary.BigEndian.Uint16(pp.Value[6..8]),
            Remote: pp.Value[8..14]
        ));
        for (nint i = 14; i < (nint)(14 + IFNAMSIZ); i++) {
            if (pp.Value[i] == 0) {
                sa.Dev = (@string)(pp.Value.slice(14, i));
                break;
            }
        }
        return (new SockaddrPPPoEжSockaddr(sa), default);
    }
    case var ᴛ when ᴛ == AF_TIPC: {
        var pp = Ꮡrsa.Reinterpret<RawSockaddrAny, RawSockaddrTIPC>();

        var sa = Ꮡ(new SockaddrTIPC(
            Scope: (nint)pp.Scope
        ));

        switch (pp.Addrtype) {
        case var ᴛ2 when ᴛ2 == TIPC_SERVICE_RANGE: {
            sa.Addr = new TIPCServiceRangeжTIPCAddr(pp.ᏑAddr.Reinterpret<array<byte>, TIPCServiceRange>());
            break;
        }
        case var ᴛ2 when ᴛ2 == TIPC_SERVICE_ADDR: {
            sa.Addr = new TIPCServiceNameжTIPCAddr(pp.ᏑAddr.Reinterpret<array<byte>, TIPCServiceName>());
            break;
        }
        case var ᴛ2 when ᴛ2 == TIPC_SOCKET_ADDR: {
            sa.Addr = new TIPCSocketAddrжTIPCAddr(pp.ᏑAddr.Reinterpret<array<byte>, TIPCSocketAddr>());
            break;
        }
        default: {
            return (default, EINVAL);
        }}

        return (new SockaddrTIPCжSockaddr(sa), default);
    }
    case var ᴛ when ᴛ == AF_IUCV: {
        var pp = Ꮡrsa.Reinterpret<RawSockaddrAny, RawSockaddrIUCV>();

        array<byte> user = new(8);
        array<byte> name = new(8);

        foreach (var i in range(8)) {
            user[i] = (byte)pp.of(RawSockaddrIUCV.ᏑUser_id).ElementRef(i);
            name[i] = (byte)pp.of(RawSockaddrIUCV.ᏑName).ElementRef(i);
        }

        var sa = Ꮡ(new SockaddrIUCV(
            UserID: ((@string)(user[..])),
            Name: ((@string)(name[..]))
        ));
        return (new SockaddrIUCVжSockaddr(sa), default);
    }
    case var ᴛ when ᴛ == AF_CAN: {
        var (proto, err) = socketProtocol(fd);
        if (err != nil) {
            return (default, err);
        }

        var pp = Ꮡrsa.Reinterpret<RawSockaddrAny, RawSockaddrCAN>();

        switch (proto) {
        case var ᴛ2 when ᴛ2 == CAN_J1939: {
            var sa = Ꮡ(new SockaddrCANJ1939(
                Ifindex: (nint)pp.Ifindex
            ));
            var name = @unsafe.ArrayPointer<byte>.Of(sa.ᏑName, 8);
            foreach (var i in range(8)) {
                name.ElementRef(i) = pp.of(RawSockaddrCAN.ᏑAddr).ElementRef(i);
            }
            var pgn = @unsafe.ArrayPointer<byte>.Of(sa.ᏑPGN, 4);
            foreach (var i in range(4)) {
                pgn.ElementRef(i) = pp.of(RawSockaddrCAN.ᏑAddr).ElementRef(i + 8);
            }
            var addr = array<byte>.AliasPointer(sa.ᏑAddr, 1);
            addr.Value[0] = pp.of(RawSockaddrCAN.ᏑAddr).ElementRef(12);
            return (new SockaddrCANJ1939жSockaddr(sa), default);
        }
        default: {
            var sa = Ꮡ(new SockaddrCAN(
                Ifindex: (nint)pp.Ifindex
            ));
            var rx = @unsafe.ArrayPointer<byte>.Of(sa.ᏑRxID, 4);
            foreach (var i in range(4)) {
                rx.ElementRef(i) = pp.of(RawSockaddrCAN.ᏑAddr).ElementRef(i);
            }
            var tx = @unsafe.ArrayPointer<byte>.Of(sa.ᏑTxID, 4);
            foreach (var i in range(4)) {
                tx.ElementRef(i) = pp.of(RawSockaddrCAN.ᏑAddr).ElementRef(i + 4);
            }
            return (new SockaddrCANжSockaddr(sa), default);
        }}

        break;
    }
    case var ᴛ when ᴛ == AF_NFC: {
        var (proto, err) = socketProtocol(fd);
        if (err != nil) {
            return (default, err);
        }
        switch (proto) {
        case var ᴛ2 when ᴛ2 == NFC_SOCKPROTO_RAW: {
            var pp = Ꮡrsa.Reinterpret<RawSockaddrAny, RawSockaddrNFC>();
            var sa = Ꮡ(new SockaddrNFC(
                DeviceIdx: pp.Dev_idx,
                TargetIdx: pp.Target_idx,
                NFCProtocol: pp.Nfc_protocol
            ));
            return (new SockaddrNFCжSockaddr(sa), default);
        }
        case var ᴛ2 when ᴛ2 == NFC_SOCKPROTO_LLCP: {
            var pp = Ꮡrsa.Reinterpret<RawSockaddrAny, RawSockaddrNFCLLCP>();
            if ((uint64)pp.Service_name_len > (uint64)len(pp.Service_name)) {
                return (default, EINVAL);
            }
            var sa = Ꮡ(new SockaddrNFCLLCP(
                DeviceIdx: pp.Dev_idx,
                TargetIdx: pp.Target_idx,
                NFCProtocol: pp.Nfc_protocol,
                DestinationSAP: pp.Dsap,
                SourceSAP: pp.Ssap,
                ServiceName: ((@string)(pp.of(RawSockaddrNFCLLCP.ᏑService_name).slice(0, (nint)(pp.Service_name_len))))
            ));
            return (new SockaddrNFCLLCPжSockaddr(sa), default);
        }
        default: {
            return (default, EINVAL);
        }}

        break;
    }}

    return (default, EAFNOSUPPORT);
}

public static (nint nfd, Sockaddr sa, error err) Accept(nint fd) {
    nint nfd = default;
    Sockaddr sa = default;
    error err = default;

    ref var rsa = ref heap(new RawSockaddrAny(), out var Ꮡrsa);
    ref var len = ref heap(new _Socklen(), out var Ꮡlen);
    len = SizeofSockaddrAny;
    (nfd, err) = accept4(fd, Ꮡrsa, Ꮡlen, 0);
    if (err != nil) {
        return (nfd, sa, err);
    }
    (sa, err) = anyToSockaddr(fd, Ꮡrsa);
    if (err != nil) {
        Close(nfd);
        nfd = 0;
    }
    return (nfd, sa, err);
}

public static (nint nfd, Sockaddr sa, error err) Accept4(nint fd, nint flags) {
    nint nfd = default;
    Sockaddr sa = default;
    error err = default;

    ref var rsa = ref heap(new RawSockaddrAny(), out var Ꮡrsa);
    ref var len = ref heap(new _Socklen(), out var Ꮡlen);
    len = SizeofSockaddrAny;
    (nfd, err) = accept4(fd, Ꮡrsa, Ꮡlen, flags);
    if (err != nil) {
        return (nfd, sa, err);
    }
    if (len > SizeofSockaddrAny) {
        throw panic("RawSockaddrAny too small");
    }
    (sa, err) = anyToSockaddr(fd, Ꮡrsa);
    if (err != nil) {
        Close(nfd);
        nfd = 0;
    }
    return (nfd, sa, err);
}

public static (Sockaddr sa, error err) Getsockname(nint fd) {
    Sockaddr sa = default;
    error err = default;

    ref var rsa = ref heap(new RawSockaddrAny(), out var Ꮡrsa);
    ref var len = ref heap(new _Socklen(), out var Ꮡlen);
    len = SizeofSockaddrAny;
    err = getsockname(fd, Ꮡrsa, Ꮡlen); if (err != nil) {
        return (sa, err);
    }
    return anyToSockaddr(fd, Ꮡrsa);
}

public static (ж<IPMreqn>, error) GetsockoptIPMreqn(nint fd, nint level, nint opt) {
    ref var value = ref heap(new IPMreqn(), out var Ꮡvalue);
    ref var vallen = ref heap<_Socklen>((_Socklen)SizeofIPMreqn, out var Ꮡvallen);
    var err = getsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡvalue), Ꮡvallen);
    return (Ꮡvalue, err);
}

public static (ж<Ucred>, error) GetsockoptUcred(nint fd, nint level, nint opt) {
    ref var value = ref heap(new Ucred(), out var Ꮡvalue);
    ref var vallen = ref heap<_Socklen>((_Socklen)SizeofUcred, out var Ꮡvallen);
    var err = getsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡvalue), Ꮡvallen);
    return (Ꮡvalue, err);
}

public static (ж<TCPInfo>, error) GetsockoptTCPInfo(nint fd, nint level, nint opt) {
    ref var value = ref heap(new TCPInfo(), out var Ꮡvalue);
    ref var vallen = ref heap<_Socklen>((_Socklen)SizeofTCPInfo, out var Ꮡvallen);
    var err = getsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡvalue), Ꮡvallen);
    return (Ꮡvalue, err);
}

public static (ж<TCPVegasInfo>, error) GetsockoptTCPCCVegasInfo(nint fd, nint level, nint opt) {
    ref var value = ref heap(new array<uint32>(5), out var Ꮡvalue);
    ref var vallen = ref heap<_Socklen>((_Socklen)SizeofTCPCCInfo, out var Ꮡvallen);
    var err = getsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡvalue.at<uint32>(0)), Ꮡvallen);
    var @out = Ꮡvalue.at<uint32>(0).Reinterpret<uint32, TCPVegasInfo>();
    return (@out, err);
}

public static (ж<TCPDCTCPInfo>, error) GetsockoptTCPCCDCTCPInfo(nint fd, nint level, nint opt) {
    ref var value = ref heap(new array<uint32>(5), out var Ꮡvalue);
    ref var vallen = ref heap<_Socklen>((_Socklen)SizeofTCPCCInfo, out var Ꮡvallen);
    var err = getsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡvalue.at<uint32>(0)), Ꮡvallen);
    var @out = Ꮡvalue.at<uint32>(0).Reinterpret<uint32, TCPDCTCPInfo>();
    return (@out, err);
}

public static (ж<TCPBBRInfo>, error) GetsockoptTCPCCBBRInfo(nint fd, nint level, nint opt) {
    ref var value = ref heap(new array<uint32>(5), out var Ꮡvalue);
    ref var vallen = ref heap<_Socklen>((_Socklen)SizeofTCPCCInfo, out var Ꮡvallen);
    var err = getsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡvalue.at<uint32>(0)), Ꮡvallen);
    var @out = Ꮡvalue.at<uint32>(0).Reinterpret<uint32, TCPBBRInfo>();
    return (@out, err);
}

public static (@string, error) GetsockoptString(nint fd, nint level, nint opt) {
    var buf = new slice<byte>(256);
    ref var vallen = ref heap<_Socklen>((_Socklen)(uint32)len(buf), out var Ꮡvallen);
    var err = getsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 0)), Ꮡvallen);
    if (err != nil) {
        if (AreEqual(err, ERANGE)) {
            buf = new slice<byte>((nint)(vallen));
            err = getsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 0)), Ꮡvallen);
        }
        if (err != nil) {
            return ("", err);
        }
    }
    return (ByteSliceToString(buf.slice(0, (nint)(vallen))), default);
}

public static (ж<TpacketStats>, error) GetsockoptTpacketStats(nint fd, nint level, nint opt) {
    ref var value = ref heap(new TpacketStats(), out var Ꮡvalue);
    ref var vallen = ref heap<_Socklen>((_Socklen)SizeofTpacketStats, out var Ꮡvallen);
    var err = getsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡvalue), Ꮡvallen);
    return (Ꮡvalue, err);
}

public static (ж<TpacketStatsV3>, error) GetsockoptTpacketStatsV3(nint fd, nint level, nint opt) {
    ref var value = ref heap(new TpacketStatsV3(), out var Ꮡvalue);
    ref var vallen = ref heap<_Socklen>((_Socklen)SizeofTpacketStatsV3, out var Ꮡvallen);
    var err = getsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡvalue), Ꮡvallen);
    return (Ꮡvalue, err);
}

public static error /*err*/ SetsockoptIPMreqn(nint fd, nint level, nint opt, ж<IPMreqn> mreq) {
    return setsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(mreq), /* unsafe.Sizeof(*mreq) */ (uintptr)12);
}

public static error SetsockoptPacketMreq(nint fd, nint level, nint opt, ж<PacketMreq> mreq) {
    return setsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(mreq), /* unsafe.Sizeof(*mreq) */ (uintptr)16);
}

public static error SetsockoptSockFprog(nint fd, nint level, nint opt, ж<SockFprog> fprog) {
    return setsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(fprog), /* unsafe.Sizeof(*fprog) */ (uintptr)16);
}

public static error SetsockoptCanRawFilter(nint fd, nint level, nint opt, slice<CanFilter> filter) {
    @unsafe.Pointer p = default;
    if (len(filter) > 0) {
        p = @unsafe.Pointer.FromPinnedBox(Ꮡ(filter, 0));
    }
    return setsockopt(fd, level, opt, p, (uintptr)(len(filter) * (nint)SizeofCanFilter));
}

public static error SetsockoptTpacketReq(nint fd, nint level, nint opt, ж<TpacketReq> tp) {
    return setsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(tp), /* unsafe.Sizeof(*tp) */ (uintptr)16);
}

public static error SetsockoptTpacketReq3(nint fd, nint level, nint opt, ж<TpacketReq3> tp) {
    return setsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(tp), /* unsafe.Sizeof(*tp) */ (uintptr)28);
}

public static error /*err*/ SetsockoptTCPRepairOpt(nint fd, nint level, nint opt, slice<TCPRepairOpt> o) {
    if (len(o) == 0) {
        return EINVAL;
    }
    return setsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(Ꮡ(o, 0)), (uintptr)((nint)SizeofTCPRepairOpt * len(o)));
}

public static error SetsockoptTCPMD5Sig(nint fd, nint level, nint opt, ж<TCPMD5Sig> s) {
    return setsockopt(fd, level, opt, @unsafe.Pointer.FromPinnedBox(s), /* unsafe.Sizeof(*s) */ (uintptr)216);
}

public static (@string, error) KeyctlString(nint cmd, nint id) {
    slice<byte> buffer = default;
    for (;;) {
        var (length, err) = KeyctlBuffer(cmd, id, buffer, 0);
        if (err != nil) {
            return ("", err);
        }

        if (length <= len(buffer)) {
            return ((@string)(buffer.slice(0, length - 1)), default);
        }

        buffer = new slice<byte>(length);
    }
}

public static (nint ringid, error err) KeyctlGetKeyringID(nint id, bool create) {
    nint createInt = 0;
    if (create) {
        createInt = 1;
    }
    return KeyctlInt(KEYCTL_GET_KEYRING_ID, id, createInt, 0, 0);
}

public static error KeyctlSetperm(nint id, uint32 perm) {
    var (_, err) = KeyctlInt(KEYCTL_SETPERM, id, (nint)perm, 0, 0);
    return err;
}

public static (nint ringid, error err) KeyctlJoinSessionKeyring(@string name) {
    return keyctlJoin(KEYCTL_JOIN_SESSION_KEYRING, name);
}

public static (nint id, error err) KeyctlSearch(nint ringid, @string keyType, @string description, nint destRingid) {
    return keyctlSearch(KEYCTL_SEARCH, ringid, keyType, description, destRingid);
}

public static error KeyctlInstantiateIOV(nint id, slice<Iovec> payload, nint ringid) {
    return keyctlIOV(KEYCTL_INSTANTIATE_IOV, id, payload, ringid);
}

public static (nint size, error err) KeyctlDHCompute(ж<KeyctlDHParams> @params, slice<byte> buffer) {
    return keyctlDH(KEYCTL_DH_COMPUTE, @params, buffer);
}

public static error KeyctlRestrictKeyring(nint ringid, @string keyType, @string restriction) {
    if (keyType == ""u8) {
        return keyctlRestrictKeyring(KEYCTL_RESTRICT_KEYRING, ringid);
    }
    return keyctlRestrictKeyringByType(KEYCTL_RESTRICT_KEYRING, ringid, keyType, restriction);
}

internal static (nint n, nint oobn, nint recvflags, error err) recvmsgRaw(nint fd, slice<Iovec> iov, slice<byte> oob, nint flags, ж<RawSockaddrAny> rsa) {
    nint n = default;
    nint oobn = default;
    nint recvflags = default;
    error err = default;

    ref var msg = ref heap(new Msghdr(), out var Ꮡmsg);
    msg.Name = rsa.Reinterpret<RawSockaddrAny, byte>();
    msg.Namelen = (uint32)SizeofSockaddrAny;
    ref var dummy = ref heap(new byte(), out var Ꮡdummy);
    if (len(oob) > 0) {
        if (emptyIovecs(iov)) {
            nint sockType = default;
            (sockType, err) = GetsockoptInt(fd, SOL_SOCKET, SO_TYPE);
            if (err != nil) {
                return (n, oobn, recvflags, err);
            }
            if (sockType != SOCK_DGRAM) {
                array<Iovec> iova = new(1);
                iova[0].Base = Ꮡdummy;
                iova[0].SetLen(1);
                iov = iova[..];
            }
        }
        msg.Control = Ꮡ(oob, 0);
        msg.SetControllen(len(oob));
    }
    if (len(iov) > 0) {
        msg.Iov = Ꮡ(iov, 0);
        msg.SetIovlen(len(iov));
    }
    (n, err) = recvmsg(fd, Ꮡmsg, flags); if (err != nil) {
        return (n, oobn, recvflags, err);
    }
    oobn = (nint)msg.Controllen;
    recvflags = (nint)msg.Flags;
    return (n, oobn, recvflags, err);
}

internal static (nint n, error err) sendmsgN(nint fd, slice<Iovec> iov, slice<byte> oob, @unsafe.Pointer ptr, _Socklen salen, nint flags) {
    nint n = default;
    error err = default;

    ref var msg = ref heap(new Msghdr(), out var Ꮡmsg);
    msg.Name = @unsafe.Pointer.To<byte>(ptr);
    msg.Namelen = (uint32)salen;
    ref var dummy = ref heap(new byte(), out var Ꮡdummy);
    bool empty = default;
    if (len(oob) > 0) {
        empty = emptyIovecs(iov);
        if (empty) {
            nint sockType = default;
            (sockType, err) = GetsockoptInt(fd, SOL_SOCKET, SO_TYPE);
            if (err != nil) {
                return (0, err);
            }
            if (sockType != SOCK_DGRAM) {
                array<Iovec> iova = new(1);
                iova[0].Base = Ꮡdummy;
                iova[0].SetLen(1);
                iov = iova[..];
            }
        }
        msg.Control = Ꮡ(oob, 0);
        msg.SetControllen(len(oob));
    }
    if (len(iov) > 0) {
        msg.Iov = Ꮡ(iov, 0);
        msg.SetIovlen(len(iov));
    }
    (n, err) = sendmsg(fd, Ꮡmsg, flags); if (err != nil) {
        return (0, err);
    }
    if (len(oob) > 0 && empty) {
        n = 0;
    }
    return (n, default);
}

public static error /*err*/ BindToDevice(nint fd, @string device) {
    return SetsockoptString(fd, SOL_SOCKET, SO_BINDTODEVICE, device);
}

internal static (nint count, error err) ptracePeek(nint req, nint pid, uintptr addr, slice<byte> @out) {
    error err = default;

    ref var buf = ref heap(new array<byte>(8), out var Ꮡbuf);

    nint n = 0;
    if (addr % (uintptr)SizeofPtr != 0) {
        err = ptracePtr(req, pid, addr - addr % (uintptr)SizeofPtr, @unsafe.Pointer.FromPinnedBox(Ꮡbuf.at<byte>(0)));
        if (err != nil) {
            return (0, err);
        }
        n += copy(@out, buf.slice((nint)(addr % (uintptr)SizeofPtr)));
        @out = @out.slice(n);
    }

    while (len(@out) > 0) {
        err = ptracePtr(req, pid, addr + (uintptr)n, @unsafe.Pointer.FromPinnedBox(Ꮡbuf.at<byte>(0)));
        if (err != nil) {
            return (n, err);
        }
        nint copied = copy(@out, buf[0..]);
        n += copied;
        @out = @out.slice(copied);
    }

    return (n, default);
}

public static (nint count, error err) PtracePeekText(nint pid, uintptr addr, slice<byte> @out) {
    return ptracePeek(PTRACE_PEEKTEXT, pid, addr, @out);
}

public static (nint count, error err) PtracePeekData(nint pid, uintptr addr, slice<byte> @out) {
    return ptracePeek(PTRACE_PEEKDATA, pid, addr, @out);
}

public static (nint count, error err) PtracePeekUser(nint pid, uintptr addr, slice<byte> @out) {
    return ptracePeek(PTRACE_PEEKUSR, pid, addr, @out);
}

internal static (nint count, error err) ptracePoke(nint pokeReq, nint peekReq, nint pid, uintptr addr, slice<byte> data) {
    error err = default;

    nint n = 0;
    if (addr % (uintptr)SizeofPtr != 0) {
        ref var buf = ref heap(new array<byte>(8), out var Ꮡbuf);
        err = ptracePtr(peekReq, pid, addr - addr % (uintptr)SizeofPtr, @unsafe.Pointer.FromPinnedBox(Ꮡbuf.at<byte>(0)));
        if (err != nil) {
            return (0, err);
        }
        n += copy(buf.slice((nint)(addr % (uintptr)SizeofPtr)), data);
        var word = (Ꮡbuf.at<byte>(0).Reinterpret<byte, uintptr>()).Value;
        err = ptrace(pokeReq, pid, addr - addr % (uintptr)SizeofPtr, word);
        if (err != nil) {
            return (0, err);
        }
        data = data.slice(n);
    }

    while (len(data) > SizeofPtr) {
        var word = (Ꮡ(data, 0).Reinterpret<byte, uintptr>()).Value;
        err = ptrace(pokeReq, pid, addr + (uintptr)n, word);
        if (err != nil) {
            return (n, err);
        }
        n += SizeofPtr;
        data = data[(int)(SizeofPtr)..];
    }

    if (len(data) > 0) {
        ref var buf = ref heap(new array<byte>(8), out var Ꮡbuf);
        err = ptracePtr(peekReq, pid, addr + (uintptr)n, @unsafe.Pointer.FromPinnedBox(Ꮡbuf.at<byte>(0)));
        if (err != nil) {
            return (n, err);
        }
        copy(buf[0..], data);
        var word = (Ꮡbuf.at<byte>(0).Reinterpret<byte, uintptr>()).Value;
        err = ptrace(pokeReq, pid, addr + (uintptr)n, word);
        if (err != nil) {
            return (n, err);
        }
        n += len(data);
    }

    return (n, default);
}

public static (nint count, error err) PtracePokeText(nint pid, uintptr addr, slice<byte> data) {
    return ptracePoke(PTRACE_POKETEXT, PTRACE_PEEKTEXT, pid, addr, data);
}

public static (nint count, error err) PtracePokeData(nint pid, uintptr addr, slice<byte> data) {
    return ptracePoke(PTRACE_POKEDATA, PTRACE_PEEKDATA, pid, addr, data);
}

public static (nint count, error err) PtracePokeUser(nint pid, uintptr addr, slice<byte> data) {
    return ptracePoke(PTRACE_POKEUSR, PTRACE_PEEKUSR, pid, addr, data);
}

internal static UntypedInt elfNT_PRSTATUS => 1;

public static error /*err*/ PtraceGetRegs(nint pid, ж<PtraceRegs> regsout) {
    ref var iov = ref heap(new Iovec(), out var Ꮡiov);
    iov.Base = regsout.Reinterpret<PtraceRegs, byte>();
    iov.SetLen((nint)/* unsafe.Sizeof(*regsout) */ (uintptr)216);
    return ptracePtr(PTRACE_GETREGSET, pid, (uintptr)elfNT_PRSTATUS, @unsafe.Pointer.FromPinnedBox(Ꮡiov));
}

public static error /*err*/ PtraceSetRegs(nint pid, ж<PtraceRegs> regs) {
    ref var iov = ref heap(new Iovec(), out var Ꮡiov);
    iov.Base = regs.Reinterpret<PtraceRegs, byte>();
    iov.SetLen((nint)/* unsafe.Sizeof(*regs) */ (uintptr)216);
    return ptracePtr(PTRACE_SETREGSET, pid, (uintptr)elfNT_PRSTATUS, @unsafe.Pointer.FromPinnedBox(Ꮡiov));
}

public static error /*err*/ PtraceSetOptions(nint pid, nint options) {
    return ptrace(PTRACE_SETOPTIONS, pid, 0, (uintptr)options);
}

public static (nuint msg, error err) PtraceGetEventMsg(nint pid) {
    nuint msg = default;
    error err = default;

    ref var data = ref heap(new _C_long(), out var Ꮡdata);
    err = ptracePtr(PTRACE_GETEVENTMSG, pid, 0, @unsafe.Pointer.FromPinnedBox(Ꮡdata));
    msg = (nuint)data;
    return (msg, err);
}

public static error /*err*/ PtraceCont(nint pid, nint signal) {
    return ptrace(PTRACE_CONT, pid, 0, (uintptr)signal);
}

public static error /*err*/ PtraceSyscall(nint pid, nint signal) {
    return ptrace(PTRACE_SYSCALL, pid, 0, (uintptr)signal);
}

public static error /*err*/ PtraceSingleStep(nint pid) { return ptrace(PTRACE_SINGLESTEP, pid, 0, 0); }

public static error /*err*/ PtraceInterrupt(nint pid) { return ptrace(PTRACE_INTERRUPT, pid, 0, 0); }

public static error /*err*/ PtraceAttach(nint pid) { return ptrace(PTRACE_ATTACH, pid, 0, 0); }

public static error /*err*/ PtraceSeize(nint pid) { return ptrace(PTRACE_SEIZE, pid, 0, 0); }

public static error /*err*/ PtraceDetach(nint pid) { return ptrace(PTRACE_DETACH, pid, 0, 0); }

public static error /*err*/ Reboot(nint cmd) {
    return reboot(LINUX_REBOOT_MAGIC1, LINUX_REBOOT_MAGIC2, cmd, "");
}

internal static (uint64, bool) direntIno(slice<byte> buf) {
    return readInt(buf, /* unsafe.Offsetof(Dirent{}.Ino) */ (uintptr)0, /* unsafe.Sizeof(Dirent{}.Ino) */ (uintptr)8);
}

internal static (uint64, bool) direntReclen(slice<byte> buf) {
    return readInt(buf, /* unsafe.Offsetof(Dirent{}.Reclen) */ (uintptr)16, /* unsafe.Sizeof(Dirent{}.Reclen) */ (uintptr)2);
}

internal static (uint64, bool) direntNamlen(slice<byte> buf) {
    var (reclen, ok) = direntReclen(buf);
    if (!ok) {
        return (0, false);
    }
    return (reclen - (uint64)/* unsafe.Offsetof(Dirent{}.Name) */ (uintptr)19, true);
}

public static error /*err*/ Mount(@string source, @string target, @string fstype, uintptr flags, @string data) {
    error err = default;

    if (data == ""u8) {
        return mount(source, target, fstype, flags, nil);
    }
    (var datap, err) = BytePtrFromString(data);
    if (err != nil) {
        return err;
    }
    return mount(source, target, fstype, flags, datap);
}

public static error MountSetattr(nint dirfd, @string pathname, nuint flags, ж<MountAttr> attr) {
    return mountSetattr(dirfd, pathname, flags, attr, /* unsafe.Sizeof(*attr) */ (uintptr)32);
}

public static (nint written, error err) Sendfile(nint outfd, nint infd, ж<int64> offset, nint count) {
    if (raceenabled) {
        raceReleaseMerge(@unsafe.Pointer.FromPinnedBox(ᏑioSync));
    }
    return sendfile(outfd, infd, offset, count);
}

public static error Dup2(nint oldfd, nint newfd) {
    return Dup3(oldfd, newfd, 0);
}

internal static error /*err*/ fsconfigCommon(nint fd, nuint cmd, @string key, ж<byte> value, nint aux) {
    error err = default;

    ж<byte> keyp = default;
    (keyp, err) = BytePtrFromString(key); if (err != nil) {
        return err;
    }
    return fsconfig(fd, cmd, keyp, value, aux);
}

public static error /*err*/ FsconfigSetFlag(nint fd, @string key) {
    return fsconfigCommon(fd, FSCONFIG_SET_FLAG, key, nil, 0);
}

public static error /*err*/ FsconfigSetString(nint fd, @string key, @string value) {
    error err = default;

    ж<byte> valuep = default;
    (valuep, err) = BytePtrFromString(value); if (err != nil) {
        return err;
    }
    return fsconfigCommon(fd, FSCONFIG_SET_STRING, key, valuep, 0);
}

public static error /*err*/ FsconfigSetBinary(nint fd, @string key, slice<byte> value) {
    if (len(value) == 0) {
        return EINVAL;
    }
    return fsconfigCommon(fd, FSCONFIG_SET_BINARY, key, Ꮡ(value, 0), len(value));
}

public static error /*err*/ FsconfigSetPath(nint fd, @string key, @string path, nint atfd) {
    error err = default;

    ж<byte> valuep = default;
    (valuep, err) = BytePtrFromString(path); if (err != nil) {
        return err;
    }
    return fsconfigCommon(fd, FSCONFIG_SET_PATH, key, valuep, atfd);
}

public static error /*err*/ FsconfigSetPathEmpty(nint fd, @string key, @string path, nint atfd) {
    error err = default;

    ж<byte> valuep = default;
    (valuep, err) = BytePtrFromString(path); if (err != nil) {
        return err;
    }
    return fsconfigCommon(fd, FSCONFIG_SET_PATH_EMPTY, key, valuep, atfd);
}

public static error /*err*/ FsconfigSetFd(nint fd, @string key, nint value) {
    return fsconfigCommon(fd, FSCONFIG_SET_FD, key, nil, value);
}

public static error /*err*/ FsconfigCreate(nint fd) {
    return fsconfig(fd, FSCONFIG_CMD_CREATE, nil, nil, 0);
}

public static error /*err*/ FsconfigReconfigure(nint fd) {
    return fsconfig(fd, FSCONFIG_CMD_RECONFIGURE, nil, nil, 0);
}

public static (nint n, error err) Getdents(nint fd, slice<byte> buf) {
    nint n = default;
    error err = default;

    if (len(buf) > 0) {
        nint off = (nint)((uintptr)(((uintptr)0 - (uintptr)Ꮡ(buf, 0)) & 7)); if (off > 0 && off < len(buf)) {
            (n, err) = getdents(fd, buf.slice(off));
            if (n > 0) {
                copy(buf, buf.slice(off, off + n));
            }
            return (n, err);
        }
    }
    return getdents(fd, buf);
}

public static nint /*pid*/ Getpgrp() {
    nint pid = default;

    (pid, _) = Getpgid(0);
    return pid;
}

public static (nint n, error err) Getrandom(slice<byte> buf, nint flags) {
    var (vdsoRet, supported) = vgetrandom(buf, (uint32)flags);
    if (supported) {
        if (vdsoRet < 0) {
            return (0, errnoErr((syscall.Errno)(uintptr)(-vdsoRet)));
        }
        return (vdsoRet, default);
    }
    ж<byte> p = default;
    if (len(buf) > 0) {
        p = Ꮡ(buf, 0);
    }
    var (r, _, e) = Syscall(SYS_GETRANDOM, (uintptr)p, (uintptr)len(buf), (uintptr)flags);
    if (e != 0) {
        return (0, errnoErr(e));
    }
    return ((nint)r, default);
}

/*linkname*/ internal static partial error syscall_prlimit(nint pid, nint resource, ж<syscall.Rlimit> newlimit, ж<syscall.Rlimit> old) {
    return syscall.prlimit(pid, resource, newlimit, old);
}

public static error Prlimit(nint pid, nint resource, ж<Rlimit> newlimit, ж<Rlimit> old) {
    return syscall_prlimit(pid, resource, newlimit.Reinterpret<Rlimit, syscall.Rlimit>(), old.Reinterpret<Rlimit, syscall.Rlimit>());
}

public static (nint, error) PrctlRetInt(nint option, uintptr arg2, uintptr arg3, uintptr arg4, uintptr arg5) {
    var (ret, _, err) = Syscall6(SYS_PRCTL, (uintptr)option, (uintptr)arg2, (uintptr)arg3, (uintptr)arg4, (uintptr)arg5, 0);
    if (err != 0) {
        return (0, err);
    }
    return ((nint)ret, default);
}

public static error /*err*/ Setuid(nint uid) {
    return syscall.Setuid(uid);
}

public static error /*err*/ Setgid(nint gid) {
    return syscall.Setgid(gid);
}

public static error /*err*/ Setreuid(nint ruid, nint euid) {
    return syscall.Setreuid(ruid, euid);
}

public static error /*err*/ Setregid(nint rgid, nint egid) {
    return syscall.Setregid(rgid, egid);
}

public static error /*err*/ Setresuid(nint ruid, nint euid, nint suid) {
    return syscall.Setresuid(ruid, euid, suid);
}

public static error /*err*/ Setresgid(nint rgid, nint egid, nint sgid) {
    return syscall.Setresgid(rgid, egid, sgid);
}

public static (nint, error) SetfsgidRetGid(nint gid) {
    return setfsgid(gid);
}

public static (nint, error) SetfsuidRetUid(nint uid) {
    return setfsuid(uid);
}

public static error Setfsgid(nint gid) {
    var (_, err) = setfsgid(gid);
    return err;
}

public static error Setfsuid(nint uid) {
    var (_, err) = setfsuid(uid);
    return err;
}

public static (nint newfd, error err) Signalfd(nint fd, ж<Sigset_t> sigmask, nint flags) {
    return signalfd(fd, sigmask, _C__NSIG / 8, flags);
}

internal static (uintptr lo, uintptr hi) offs2lohi(int64 offs) {
    UntypedInt longBits = /* SizeofLong * 8 */ 64;
    return ((uintptr)offs, (uintptr)(((uint64)offs >> (int)(longBits - 1)) >> 1));
}

internal static (nint n, error err) preadv(nint fd, slice<Iovec> iovecs, int64 offset) {
    var (lo, hi) = offs2lohi(offset);
    return preadvSyscall(fd, iovecs, lo, hi);
}

public static (nint n, error err) Preadv2(nint fd, slice<slice<byte>> iovs, int64 offset, nint flags) {
    nint n = default;
    error err = default;

    var iovecs = new slice<Iovec>(0, minIovec);
    iovecs = appendBytes(iovecs, iovs);
    var (lo, hi) = offs2lohi(offset);
    (n, err) = preadv2Syscall(fd, iovecs, lo, hi, flags);
    readvRaceDetect(iovecs, n, err);
    return (n, err);
}

internal static (nint n, error err) pwritev(nint fd, slice<Iovec> iovecs, int64 offset) {
    var (lo, hi) = offs2lohi(offset);
    return pwritevSyscall(fd, iovecs, lo, hi);
}

public static (nint n, error err) Pwritev2(nint fd, slice<slice<byte>> iovs, int64 offset, nint flags) {
    nint n = default;
    error err = default;

    var iovecs = new slice<Iovec>(0, minIovec);
    iovecs = appendBytes(iovecs, iovs);
    if (raceenabled) {
        raceReleaseMerge(@unsafe.Pointer.FromPinnedBox(ᏑioSync));
    }
    var (lo, hi) = offs2lohi(offset);
    (n, err) = pwritev2Syscall(fd, iovecs, lo, hi, flags);
    writevRaceDetect(iovecs, n);
    return (n, err);
}

internal static UntypedInt mremapFixed => /* MREMAP_FIXED */ 2;
internal static UntypedInt mremapDontunmap => /* MREMAP_DONTUNMAP */ 4;
internal static UntypedInt mremapMaymove => /* MREMAP_MAYMOVE */ 1;

public static (nint, error) Vmsplice(nint fd, slice<Iovec> iovs, nint flags) {
    @unsafe.Pointer p = default;
    if (len(iovs) > 0) {
        p = @unsafe.Pointer.FromPinnedBox(Ꮡ(iovs, 0));
    }

    var (n, _, errno) = Syscall6(SYS_VMSPLICE, (uintptr)fd, (uintptr)p, (uintptr)len(iovs), (uintptr)flags, 0, 0);
    if (errno != 0) {
        return (0, errno);
    }

    return ((nint)n, default);
}

internal static bool isGroupMember(nint gid) {
    var (groups, err) = Getgroups();
    if (err != nil) {
        return false;
    }

    return slices.Contains(groups, gid);
}

internal static bool isCapDacOverrideSet() {
    ref var hdr = ref heap<CapUserHeader>(new CapUserHeader(Version: LINUX_CAPABILITY_VERSION_3), out var Ꮡhdr);
    ref var data = ref heap<array<CapUserData>>(new CapUserData[]{}.array(2), out var Ꮡdata);
    var err = Capget(Ꮡhdr, Ꮡdata.at<CapUserData>(0));

    return err == nil && (uint32)(data[0].Effective & ((uint32)1 << (int)(CAP_DAC_OVERRIDE))) != 0;
}

public static error /*err*/ Faccessat(nint dirfd, @string path, uint32 mode, nint flags) {
    if (flags == 0) {
        return faccessat(dirfd, path, mode);
    }

    var errΔ1 = Faccessat2(dirfd, path, mode, flags); if (!AreEqual(errΔ1, ENOSYS) && !AreEqual(errΔ1, EPERM)) {
        return errΔ1;
    }

    if ((nint)(flags & (nint)~((UntypedInt)(AT_SYMLINK_NOFOLLOW | AT_EACCESS))) != 0) {
        return EINVAL;
    }

    ref var st = ref heap(new Stat_t(), out var Ꮡst);
    var errΔ2 = Fstatat(dirfd, path, Ꮡst, flags & (nint)AT_SYMLINK_NOFOLLOW); if (errΔ2 != nil) {
        return errΔ2;
    }

    mode &= (uint32)(7);
    if (mode == 0) {
        return default;
    }

    nint uid = default;
    if ((flags & (nint)AT_EACCESS) != 0) {
        uid = Geteuid();
        if (uid != 0 && isCapDacOverrideSet()) {
            uid = 0;
        }
    } else {
        uid = Getuid();
    }

    if (uid == 0) {
        if ((mode & 1) == 0) {
            return default;
        }
        if ((st.Mode & 73) != 0) {
            return default;
        }
        return EACCES;
    }

    uint32 fmode = default;
    if ((uint32)uid == st.Uid) {
        fmode = (st.Mode >> 6) & 7;
    } else {
        nint gid = default;
        if ((flags & (nint)AT_EACCESS) != 0) {
            gid = Getegid();
        } else {
            gid = Getgid();
        }

        if ((uint32)gid == st.Gid || isGroupMember((nint)st.Gid)) {
            fmode = (st.Mode >> 3) & 7;
        } else {
            fmode = st.Mode & 7;
        }
    }

    if ((fmode & mode) == mode) {
        return default;
    }

    return EACCES;
}

partial struct fileHandle {
    public uint32 Bytes;
    public int32 Type;
}

partial struct FileHandle {
    /*embed*/ internal ж<fileHandle> fileHandle;
}

public static FileHandle NewFileHandle(int32 handleType, slice<byte> handle) {
    uintptr hdrSize = /* unsafe.Sizeof(fileHandle{}) */ 8;
    var buf = new slice<byte>((nint)(hdrSize + (uintptr)len(handle)));
    copy(buf[(int)(hdrSize)..], handle);
    var fh = Ꮡ(buf, 0).Reinterpret<byte, fileHandle>();
    fh.Type = handleType;
    fh.Bytes = (uint32)len(handle);
    return new FileHandle(fh);
}

} partial class unixᴛFileHandle {

public static nint Size(this ref FileHandle fh) { return (nint)fh.fileHandle.Bytes; }
public static int32 Type(this ref FileHandle fh) { return fh.fileHandle.Type; }
public static slice<byte> Bytes(this ref FileHandle fh) {
    nint n = fh.Size();
    if (n == 0) {
        return default;
    }
    return @unsafe.Slice(@unsafe.Pointer.To<byte>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(fh.fileHandle.ᏑType), 4)), n);
}

} partial class unix_package {

public static (FileHandle handle, nint mountID, error err) NameToHandleAt(nint dirfd, @string path, nint flags) {
    FileHandle handle = new(nil);
    nint mountID = default;
    error err = default;

    ref var mid = ref heap(new _C_int(), out var Ꮡmid);
    var size = (uint32)(32 + /* unsafe.Sizeof(fileHandle{}) */ (uintptr)8);
    var didResize = false;
    for (;;) {
        var buf = new slice<byte>((nint)(size));
        var fh = Ꮡ(buf, 0).Reinterpret<byte, fileHandle>();
        fh.Bytes = size - (uint32)/* unsafe.Sizeof(fileHandle{}) */ (uintptr)8;
        err = nameToHandleAt(dirfd, path, fh, Ꮡmid, flags);
        if (AreEqual(err, EOVERFLOW)) {
            if (didResize) {
                return (handle, mountID, err);
            }
            didResize = true;
            size = fh.Bytes + (uint32)/* unsafe.Sizeof(fileHandle{}) */ (uintptr)8;
            continue;
        }
        if (err != nil) {
            return (handle, mountID, err);
        }
        return (new FileHandle(fh), (nint)mid, default);
    }
}

public static (nint fd, error err) OpenByHandleAt(nint mountFD, FileHandle handle, nint flags) {
    return openByHandleAt(mountFD, handle.fileHandle, flags);
}

public static error /*err*/ Klogset(nint typ, nint arg) {
    @unsafe.Pointer p = default;
    var (_, _, errno) = Syscall(SYS_SYSLOG, (uintptr)typ, (uintptr)p, (uintptr)arg);
    if (errno != 0) {
        return errnoErr(errno);
    }
    return default;
}

partial struct RemoteIovec {
    public uintptr Base;
    public nint Len;
}

public static Itimerval MakeItimerval(time.Duration interval, time.Duration value) {
    return new Itimerval(
        Interval: NsecToTimeval(interval.Nanoseconds()),
        Value: NsecToTimeval(value.Nanoseconds())
    );
}

partial struct ItimerWhich /*num:nint*/;

public static ItimerWhich ItimerReal => /* ITIMER_REAL */ 0;
public static ItimerWhich ItimerVirtual => /* ITIMER_VIRTUAL */ 1;
public static ItimerWhich ItimerProf => /* ITIMER_PROF */ 2;

public static (Itimerval, error) Getitimer(ItimerWhich which) {
    ref var it = ref heap(new Itimerval(), out var Ꮡit);
    var err = getitimer((nint)which, Ꮡit); if (err != nil) {
        return (new Itimerval(), err);
    }

    return (it, default);
}

public static (Itimerval, error) Setitimer(ItimerWhich which, Itimerval itʗp) {
    ref var it = ref heap(itʗp, out var Ꮡit);

    ref var prev = ref heap(new Itimerval(), out var Ꮡprev);
    var err = setitimer((nint)which, Ꮡit, Ꮡprev); if (err != nil) {
        return (new Itimerval(), err);
    }

    return (prev, default);
}

public static error PthreadSigmask(nint how, ж<Sigset_t> set, ж<Sigset_t> Ꮡoldset) {
    ref var oldset = ref Ꮡoldset.DerefOrNull();

    if (Ꮡoldset != nil) {
        oldset = new Sigset_t();
    }
    return rtSigprocmask(how, set, Ꮡoldset, _C__NSIG / 8);
}

public static (nint ruid, nint euid, nint suid) Getresuid() {
    ref var r = ref heap(new _C_int(), out var Ꮡr);
    ref var e = ref heap(new _C_int(), out var Ꮡe);
    ref var s = ref heap(new _C_int(), out var Ꮡs);
    getresuid(Ꮡr, Ꮡe, Ꮡs);
    return ((nint)r, (nint)e, (nint)s);
}

public static (nint rgid, nint egid, nint sgid) Getresgid() {
    ref var r = ref heap(new _C_int(), out var Ꮡr);
    ref var e = ref heap(new _C_int(), out var Ꮡe);
    ref var s = ref heap(new _C_int(), out var Ꮡs);
    getresgid(Ꮡr, Ꮡe, Ꮡs);
    return ((nint)r, (nint)e, (nint)s);
}

public static (nint n, error err) Pselect(nint nfd, ж<FdSet> r, ж<FdSet> w, ж<FdSet> e, ж<Timespec> Ꮡtimeout, ж<Sigset_t> sigmask) {
    ref var timeout = ref Ꮡtimeout.DerefOrNull();

    ж<Timespec> mutableTimeout = default;
    if (Ꮡtimeout != nil) {
        mutableTimeout = @new<Timespec>();
        mutableTimeout.Value = timeout;
    }

    ж<sigset_argpack> kernelMask = default;
    if (sigmask != nil) {
        nint wordBits = 32 << (int)(~(uintptr)0 >> 63);

        nint sigsetWords = quo(((nint)(_C__NSIG - 1) + wordBits - 1), (wordBits));

        ref var sigsetBytes = ref heap<uintptr>((uintptr)(sigsetWords * (wordBits / 8)), out var ᏑsigsetBytes);
        kernelMask = Ꮡ(new sigset_argpack(
            ss: sigmask,
            ssLen: sigsetBytes
        ));
    }

    return pselect6(nfd, r, w, e, mutableTimeout, kernelMask);
}

public static error SchedSetAttr(nint pid, ж<SchedAttr> Ꮡattr, nuint flags) {
    ref var attr = ref Ꮡattr.DerefOrNull();

    if (Ꮡattr == nil) {
        return EINVAL;
    }
    attr.Size = SizeofSchedAttr;
    return schedSetattr(pid, Ꮡattr, flags);
}

public static (ж<SchedAttr>, error) SchedGetAttr(nint pid, nuint flags) {
    var attr = Ꮡ(new SchedAttr());
    var err = schedGetattr(pid, attr, SizeofSchedAttr, flags); if (err != nil) {
        return (default, err);
    }
    return (attr, default);
}

public static error SetMemPolicy(nint mode, ж<CPUSet> mask) {
    return setMemPolicy(mode, @unsafe.Pointer.FromPinnedBox(mask), _CPU_SETSIZE);
}

public static error SetMemPolicyDynamic(nint mode, CPUSetDynamic mask) {
    return setMemPolicy(mode, mask.pointer(), mask.size());
}

} // end unix_package
