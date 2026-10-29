// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || (js && wasm) || wasip1
namespace go.net.@internal;

using syscall = syscall_package;

partial class socktest_package {

// Socket wraps [syscall.Socket].
public static (nint s, error err) Socket(this ж<Switch> Ꮡsw, nint family, nint sotype, nint proto) {
    nint s = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var sw = ref Ꮡsw.DerefOrNull();

        Ꮡsw.of(Switch.Ꮡonce).Do(Ꮡsw.init);

        var so = Ꮡ(new Status(Cookie: cookie(family, sotype, proto)));
        Ꮡsw.of(Switch.Ꮡfmu).RLock();
        var f = sw.fltab[FilterSocket];
        Ꮡsw.of(Switch.Ꮡfmu).RUnlock();

        (var af, err) = f.apply(so);
        if (err != nil) {
            (s, err) = (-1, err); goto ᒐdone;
        }
        (s, so.Err) = syscall.Socket(family, sotype, proto);
        {
            err = af.apply(so); if (err != nil) {
                if (so.Err == nil) {
                    syscall.Close(s);
                }
                (s, err) = (-1, err); goto ᒐdone;
            }
        }

        Ꮡsw.of(Switch.Ꮡsmu).Lock();
        defer(Ꮡsw.of(Switch.Ꮡsmu).Unlock, ref ᒐ);
        if (so.Err != nil) {
            sw.stats.getLocked(so.Cookie).OpenFailed++;
            (s, err) = (-1, so.Err); goto ᒐdone;
        }
        var nso = Ꮡsw.addLocked(s, family, sotype, proto);
        sw.stats.getLocked(nso.Cookie).Opened++;
        (s, err) = (s, default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (s, err);
}

// Close wraps syscall.Close.
public static error /*err*/ Close(this ж<Switch> Ꮡsw, nint s) {
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var sw = ref Ꮡsw.DerefOrNull();

        var so = Ꮡsw.sockso(s);
        if (so == nil) {
            err = syscall.Close(s); goto ᒐdone;
        }
        Ꮡsw.of(Switch.Ꮡfmu).RLock();
        var f = sw.fltab[FilterClose];
        Ꮡsw.of(Switch.Ꮡfmu).RUnlock();

        (var af, err) = f.apply(so);
        if (err != nil) {
            goto ᒐdone;
        }
        so.Err = syscall.Close(s);
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

// Connect wraps syscall.Connect.
public static error /*err*/ Connect(this ж<Switch> Ꮡsw, nint s, syscall.Sockaddr sa) {
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var sw = ref Ꮡsw.DerefOrNull();

        var so = Ꮡsw.sockso(s);
        if (so == nil) {
            err = syscall.Connect(s, sa); goto ᒐdone;
        }
        Ꮡsw.of(Switch.Ꮡfmu).RLock();
        var f = sw.fltab[FilterConnect];
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

// Listen wraps syscall.Listen.
public static error /*err*/ Listen(this ж<Switch> Ꮡsw, nint s, nint backlog) {
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var sw = ref Ꮡsw.DerefOrNull();

        var so = Ꮡsw.sockso(s);
        if (so == nil) {
            err = syscall.Listen(s, backlog); goto ᒐdone;
        }
        Ꮡsw.of(Switch.Ꮡfmu).RLock();
        var f = sw.fltab[FilterListen];
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

// Accept wraps syscall.Accept.
public static (nint ns, syscall.Sockaddr sa, error err) Accept(this ж<Switch> Ꮡsw, nint s) {
    nint ns = default;
    syscall.Sockaddr sa = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var sw = ref Ꮡsw.DerefOrNull();

        var so = Ꮡsw.sockso(s);
        if (so == nil) {
            (ns, sa, err) = syscall.Accept(s); goto ᒐdone;
        }
        Ꮡsw.of(Switch.Ꮡfmu).RLock();
        var f = sw.fltab[FilterAccept];
        Ꮡsw.of(Switch.Ꮡfmu).RUnlock();

        (var af, err) = f.apply(so);
        if (err != nil) {
            (ns, sa, err) = (-1, default, err); goto ᒐdone;
        }
        (ns, sa, so.Err) = syscall.Accept(s);
        {
            err = af.apply(so); if (err != nil) {
                if (so.Err == nil) {
                    syscall.Close(ns);
                }
                (ns, sa, err) = (-1, default, err); goto ᒐdone;
            }
        }

        Ꮡsw.of(Switch.Ꮡsmu).Lock();
        defer(Ꮡsw.of(Switch.Ꮡsmu).Unlock, ref ᒐ);
        if (so.Err != nil) {
            sw.stats.getLocked(so.Cookie).AcceptFailed++;
            (ns, sa, err) = (-1, default, so.Err); goto ᒐdone;
        }
        var nso = Ꮡsw.addLocked(ns, so.Cookie.Family(), so.Cookie.Type(), so.Cookie.Protocol());
        sw.stats.getLocked(nso.Cookie).Accepted++;
        (ns, sa, err) = (ns, sa, default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (ns, sa, err);
}

// GetsockoptInt wraps syscall.GetsockoptInt.
public static (nint soerr, error err) GetsockoptInt(this ж<Switch> Ꮡsw, nint s, nint level, nint opt) {
    nint soerr = default;
    error err = default;
    ref var sw = ref Ꮡsw.DerefOrNull();

    var so = Ꮡsw.sockso(s);
    if (so == nil) {
        return syscall.GetsockoptInt(s, level, opt);
    }
    Ꮡsw.of(Switch.Ꮡfmu).RLock();
    var f = sw.fltab[FilterGetsockoptInt];
    Ꮡsw.of(Switch.Ꮡfmu).RUnlock();

    (var af, err) = f.apply(so);
    if (err != nil) {
        return (-1, err);
    }
    (soerr, so.Err) = syscall.GetsockoptInt(s, level, opt);
    so.SocketErr = ((syscall.Errno)(uintptr)soerr);
    {
        err = af.apply(so); if (err != nil) {
            return (-1, err);
        }
    }

    if (so.Err != nil) {
        return (-1, so.Err);
    }
    if (opt == syscall.SO_ERROR && (AreEqual(so.SocketErr, ((syscall.Errno)0)) || AreEqual(so.SocketErr, syscall.EISCONN))) {
        Ꮡsw.of(Switch.Ꮡsmu).Lock();
        sw.stats.getLocked(so.Cookie).Connected++;
        Ꮡsw.of(Switch.Ꮡsmu).Unlock();
    }
    return (soerr, default);
}

} // end socktest_package
