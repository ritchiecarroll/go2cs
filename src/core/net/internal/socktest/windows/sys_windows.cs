// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net.@internal;

using windows = go.@internal.syscall.windows_package;
using syscall = syscall_package;
using go.@internal.syscall;

partial class socktest_package {

// WSASocket wraps [syscall.WSASocket].
public static (syscallꓸHandle s, error err) WSASocket(this ж<Switch> Ꮡsw, int32 family, int32 sotype, int32 proto, ж<syscall.WSAProtocolInfo> protinfo, uint32 group, uint32 flags) {
    syscallꓸHandle s = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var sw = ref Ꮡsw.DerefOrNull();

        Ꮡsw.of(Switch.Ꮡonce).Do(Ꮡsw.init);

        var so = Ꮡ(new Status(Cookie: cookie((nint)family, (nint)sotype, (nint)proto)));
        Ꮡsw.of(Switch.Ꮡfmu).RLock();
        var (f, _) = sw.fltab[FilterSocket, ꟷ];
        Ꮡsw.of(Switch.Ꮡfmu).RUnlock();

        (var af, err) = f.apply(so);
        if (err != nil) {
            (s, err) = (syscall.InvalidHandle, err); goto ᒐdone;
        }
        (s, so.Err) = windows.WSASocket(family, sotype, proto, protinfo, group, flags);
        {
            err = af.apply(so); if (err != nil) {
                if (so.Err == nil) {
                    syscall.Closesocket(s);
                }
                (s, err) = (syscall.InvalidHandle, err); goto ᒐdone;
            }
        }

        Ꮡsw.of(Switch.Ꮡsmu).Lock();
        defer(Ꮡsw.of(Switch.Ꮡsmu).Unlock, ref ᒐ);
        if (so.Err != nil) {
            sw.stats.getLocked(so.Cookie).OpenFailed++;
            (s, err) = (syscall.InvalidHandle, so.Err); goto ᒐdone;
        }
        var nso = Ꮡsw.addLocked(s, (nint)family, (nint)sotype, (nint)proto);
        sw.stats.getLocked(nso.Cookie).Opened++;
        (s, err) = (s, default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (s, err);
}

// Closesocket wraps [syscall.Closesocket].
public static error /*err*/ Closesocket(this ж<Switch> Ꮡsw, syscallꓸHandle s) {
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var sw = ref Ꮡsw.DerefOrNull();

        var so = Ꮡsw.sockso(s);
        if (so == nil) {
            err = syscall.Closesocket(s); goto ᒐdone;
        }
        Ꮡsw.of(Switch.Ꮡfmu).RLock();
        var (f, _) = sw.fltab[FilterClose, ꟷ];
        Ꮡsw.of(Switch.Ꮡfmu).RUnlock();

        (var af, err) = f.apply(so);
        if (err != nil) {
            goto ᒐdone;
        }
        so.Err = syscall.Closesocket(s);
        {
            err = af.apply(so); if (err != nil) {
                goto ᒐdone;
            }
        }

        Ꮡsw.of(Switch.Ꮡsmu).Lock();
        defer(Ꮡsw.of(Switch.Ꮡsmu).Unlock, ref ᒐ);
        if (so.Err != nil) {
            sw.stats.getLocked(so.Cookie).CloseFailed++;
            err = so.Err; goto ᒐdone;
        }
        delete(sw.sotab, s);
        sw.stats.getLocked(so.Cookie).Closed++;
        err = default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return err;
}

// Connect wraps [syscall.Connect].
public static error /*err*/ Connect(this ж<Switch> Ꮡsw, syscallꓸHandle s, syscallꓸSockaddr sa) {
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var sw = ref Ꮡsw.DerefOrNull();

        var so = Ꮡsw.sockso(s);
        if (so == nil) {
            err = syscall.Connect(s, sa); goto ᒐdone;
        }
        Ꮡsw.of(Switch.Ꮡfmu).RLock();
        var (f, _) = sw.fltab[FilterConnect, ꟷ];
        Ꮡsw.of(Switch.Ꮡfmu).RUnlock();

        (var af, err) = f.apply(so);
        if (err != nil) {
            goto ᒐdone;
        }
        so.Err = syscall.Connect(s, sa);
        {
            err = af.apply(so); if (err != nil) {
                goto ᒐdone;
            }
        }

        Ꮡsw.of(Switch.Ꮡsmu).Lock();
        defer(Ꮡsw.of(Switch.Ꮡsmu).Unlock, ref ᒐ);
        if (so.Err != nil) {
            sw.stats.getLocked(so.Cookie).ConnectFailed++;
            err = so.Err; goto ᒐdone;
        }
        sw.stats.getLocked(so.Cookie).Connected++;
        err = default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return err;
}

// ConnectEx wraps [syscall.ConnectEx].
public static error /*err*/ ConnectEx(this ж<Switch> Ꮡsw, syscallꓸHandle s, syscallꓸSockaddr sa, ж<byte> b, uint32 n, ж<uint32> nwr, ж<syscall.Overlapped> o) {
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var sw = ref Ꮡsw.DerefOrNull();

        var so = Ꮡsw.sockso(s);
        if (so == nil) {
            err = syscall.ConnectEx(s, sa, b, n, nwr, o); goto ᒐdone;
        }
        Ꮡsw.of(Switch.Ꮡfmu).RLock();
        var (f, _) = sw.fltab[FilterConnect, ꟷ];
        Ꮡsw.of(Switch.Ꮡfmu).RUnlock();

        (var af, err) = f.apply(so);
        if (err != nil) {
            goto ᒐdone;
        }
        so.Err = syscall.ConnectEx(s, sa, b, n, nwr, o);
        {
            err = af.apply(so); if (err != nil) {
                goto ᒐdone;
            }
        }

        Ꮡsw.of(Switch.Ꮡsmu).Lock();
        defer(Ꮡsw.of(Switch.Ꮡsmu).Unlock, ref ᒐ);
        if (so.Err != nil) {
            sw.stats.getLocked(so.Cookie).ConnectFailed++;
            err = so.Err; goto ᒐdone;
        }
        sw.stats.getLocked(so.Cookie).Connected++;
        err = default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return err;
}

// Listen wraps [syscall.Listen].
public static error /*err*/ Listen(this ж<Switch> Ꮡsw, syscallꓸHandle s, nint backlog) {
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var sw = ref Ꮡsw.DerefOrNull();

        var so = Ꮡsw.sockso(s);
        if (so == nil) {
            err = syscall.Listen(s, backlog); goto ᒐdone;
        }
        Ꮡsw.of(Switch.Ꮡfmu).RLock();
        var (f, _) = sw.fltab[FilterListen, ꟷ];
        Ꮡsw.of(Switch.Ꮡfmu).RUnlock();

        (var af, err) = f.apply(so);
        if (err != nil) {
            goto ᒐdone;
        }
        so.Err = syscall.Listen(s, backlog);
        {
            err = af.apply(so); if (err != nil) {
                goto ᒐdone;
            }
        }

        Ꮡsw.of(Switch.Ꮡsmu).Lock();
        defer(Ꮡsw.of(Switch.Ꮡsmu).Unlock, ref ᒐ);
        if (so.Err != nil) {
            sw.stats.getLocked(so.Cookie).ListenFailed++;
            err = so.Err; goto ᒐdone;
        }
        sw.stats.getLocked(so.Cookie).Listened++;
        err = default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return err;
}

// AcceptEx wraps [syscall.AcceptEx].
public static error AcceptEx(this ж<Switch> Ꮡsw, syscallꓸHandle ls, syscallꓸHandle @as, ж<byte> b, uint32 rxdatalen, uint32 laddrlen, uint32 raddrlen, ж<uint32> rcvd, ж<syscall.Overlapped> overlapped) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var sw = ref Ꮡsw.DerefOrNull();

        var so = Ꮡsw.sockso(ls);
        if (so == nil) {
            return syscall.AcceptEx(ls, @as, b, rxdatalen, laddrlen, raddrlen, rcvd, overlapped);
        }
        Ꮡsw.of(Switch.Ꮡfmu).RLock();
        var (f, _) = sw.fltab[FilterAccept, ꟷ];
        Ꮡsw.of(Switch.Ꮡfmu).RUnlock();

        var (af, err) = f.apply(so);
        if (err != nil) {
            return err;
        }
        so.Err = syscall.AcceptEx(ls, @as, b, rxdatalen, laddrlen, raddrlen, rcvd, overlapped);
        {
            err = af.apply(so); if (err != nil) {
                return err;
            }
        }

        Ꮡsw.of(Switch.Ꮡsmu).Lock();
        ᒐd1 = true;
        if (so.Err != nil) {
            sw.stats.getLocked(so.Cookie).AcceptFailed++;
            return so.Err;
        }
        var nso = Ꮡsw.addLocked(@as, so.Cookie.Family(), so.Cookie.Type(), so.Cookie.Protocol());
        sw.stats.getLocked(nso.Cookie).Accepted++;
        return default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡsw.of(Switch.Ꮡsmu).Unlock(); ᒐ.Run(); }
}

} // end socktest_package
