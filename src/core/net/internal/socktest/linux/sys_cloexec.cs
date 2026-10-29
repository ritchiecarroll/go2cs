// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build dragonfly || freebsd || linux || netbsd || openbsd || solaris
namespace go.net.@internal;

using syscall = syscall_package;

partial class socktest_package {

// Accept4 wraps syscall.Accept4.
public static (nint ns, syscall.Sockaddr sa, error err) Accept4(this ж<Switch> Ꮡsw, nint s, nint flags) {
    nint ns = default;
    syscall.Sockaddr sa = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var sw = ref Ꮡsw.DerefOrNull();

        var so = Ꮡsw.sockso(s);
        if (so == nil) {
            (ns, sa, err) = syscall.Accept4(s, flags); goto ᒐdone;
        }
        Ꮡsw.of(Switch.Ꮡfmu).RLock();
        var f = sw.fltab[FilterAccept];
        Ꮡsw.of(Switch.Ꮡfmu).RUnlock();

        (var af, err) = f.apply(so);
        if (err != nil) {
            (ns, sa, err) = (-1, default, err); goto ᒐdone;
        }
        (ns, sa, so.Err) = syscall.Accept4(s, flags);
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

} // end socktest_package
