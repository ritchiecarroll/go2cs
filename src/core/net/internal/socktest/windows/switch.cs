// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package socktest provides utilities for socket testing.
namespace go.net.@internal;

using fmt = fmt_package;
using sync = sync_package;
using syscall = syscall_package;

partial class socktest_package {

// A Switch represents a callpath point switch for socket system
// calls.
partial struct Switch {
    internal sync.Once once;
    internal sync.RWMutex fmu;
    internal map<FilterType, Filter> fltab;
    internal sync.RWMutex smu;
    internal ΔSockets sotab;
    internal stats stats;
}

internal static void init(this ref Switch sw) {
    sw.fltab = new map<FilterType, Filter>();
    sw.sotab = new ΔSockets(0);
    sw.stats = new stats(0);
}

// Stats returns a list of per-cookie socket statistics.
public static slice<Stat> Stats(this ж<Switch> Ꮡsw) {
    ref var sw = ref Ꮡsw.DerefOrNull();

    slice<Stat> st = default;
    Ꮡsw.of(Switch.Ꮡsmu).RLock();
    foreach (var (_, s) in sw.stats) {
        var ns = s.Value;
        st = append(st, ns);
    }
    Ꮡsw.of(Switch.Ꮡsmu).RUnlock();
    return st;
}

// Sockets returns mappings of socket descriptor to socket status.
public static ΔSockets Sockets(this ж<Switch> Ꮡsw) {
    ref var sw = ref Ꮡsw.DerefOrNull();

    Ꮡsw.of(Switch.Ꮡsmu).RLock();
    var tab = new ΔSockets(len(sw.sotab));
    foreach (var (i, s) in sw.sotab) {
        tab[i] = s;
    }
    Ꮡsw.of(Switch.Ꮡsmu).RUnlock();
    return tab;
}

// A Cookie represents a 3-tuple of a socket; address family, socket
// type and protocol number.
partial struct Cookie /*num:uint64*/;

// Family returns an address family.
public static nint Family(this Cookie c) {
    return (nint)(c >> 48);
}

// Type returns a socket type.
public static nint Type(this Cookie c) {
    return (nint)((c << 16) >> 32);
}

// Protocol returns a protocol number.
public static nint Protocol(this Cookie c) {
    return (nint)((Cookie)(c & 0xff));
}

internal static Cookie cookie(nint family, nint sotype, nint proto) {
    return (Cookie)((Cookie)(((Cookie)(uint64)family) << 48 | (Cookie)(((Cookie)(uint64)sotype) & 0xffffffffU) << 16) | (Cookie)(((Cookie)(uint64)proto) & 0xff));
}

// A Status represents the status of a socket.
partial struct Status {
    public Cookie Cookie;
    public error Err;       // error status of socket system call
    public error SocketErr; // error status of socket by SO_ERROR
}

public static @string String(this Status so) {
    return fmt.Sprintf("(%s, %s, %s): syscallerr=%v socketerr=%v"u8, familyString(so.Cookie.Family()), typeString(so.Cookie.Type()), protocolString(so.Cookie.Protocol()), so.Err, so.SocketErr);
}

// A Stat represents a per-cookie socket statistics.
partial struct Stat {
    public nint Family;   // address family
    public nint Type;     // socket type
    public nint Protocol; // protocol number
    public uint64 Opened;    // number of sockets opened
    public uint64 Connected; // number of sockets connected
    public uint64 Listened;  // number of sockets listened
    public uint64 Accepted;  // number of sockets accepted
    public uint64 Closed;    // number of sockets closed
    public uint64 OpenFailed;    // number of sockets open failed
    public uint64 ConnectFailed; // number of sockets connect failed
    public uint64 ListenFailed;  // number of sockets listen failed
    public uint64 AcceptFailed;  // number of sockets accept failed
    public uint64 CloseFailed;   // number of sockets close failed
}

public static @string String(this Stat st) {
    return fmt.Sprintf("(%s, %s, %s): opened=%d connected=%d listened=%d accepted=%d closed=%d openfailed=%d connectfailed=%d listenfailed=%d acceptfailed=%d closefailed=%d"u8, familyString(st.Family), typeString(st.Type), protocolString(st.Protocol), st.Opened, st.Connected, st.Listened, st.Accepted, st.Closed, st.OpenFailed, st.ConnectFailed, st.ListenFailed, st.AcceptFailed, st.CloseFailed);
}

partial struct stats /*map[Cookie, ж<Stat>]*/;

internal static ж<Stat> getLocked(this stats st, Cookie c) {
    var (s, ok) = st[c, ꟷ];
    if (!ok) {
        s = Ꮡ(new Stat(Family: c.Family(), Type: c.Type(), Protocol: c.Protocol()));
        st[c] = s;
    }
    return s;
}

// A FilterType represents a filter type.
partial struct FilterType /*num:nint*/;

public static FilterType FilterSocket => /* iota */ 0; // for Socket
public static FilterType FilterConnect => 1;           // for Connect or ConnectEx
public static FilterType FilterListen => 2;            // for Listen
public static FilterType FilterAccept => 3;            // for Accept, Accept4 or AcceptEx
public static FilterType FilterGetsockoptInt => 4;     // for GetsockoptInt
public static FilterType FilterClose => 5;             // for Close or Closesocket

// A Filter represents a socket system call filter.
//
// It will only be executed before a system call for a socket that has
// an entry in internal table.
// If the filter returns a non-nil error, the execution of system call
// will be canceled and the system call function returns the non-nil
// error.
// It can return a non-nil [AfterFilter] for filtering after the
// execution of the system call.
public delegate (AfterFilter, error) Filter(ж<Status> _);

internal static (AfterFilter, error) apply(this Filter f, ж<Status> st) {
    if (f == nil) {
        return (default, default);
    }
    return f(st);
}

// An AfterFilter represents a socket system call filter after an
// execution of a system call.
//
// It will only be executed after a system call for a socket that has
// an entry in internal table.
// If the filter returns a non-nil error, the system call function
// returns the non-nil error.
public delegate error AfterFilter(ж<Status> _);

internal static error apply(this AfterFilter f, ж<Status> st) {
    if (f == nil) {
        return default;
    }
    return f(st);
}

// Set deploys the socket system call filter f for the filter type t.
public static void Set(this ж<Switch> Ꮡsw, FilterType t, Filter f) {
    ref var sw = ref Ꮡsw.DerefOrNull();

    Ꮡsw.of(Switch.Ꮡonce).Do(Ꮡsw.init);
    Ꮡsw.of(Switch.Ꮡfmu).Lock();
    sw.fltab[t] = f;
    Ꮡsw.of(Switch.Ꮡfmu).Unlock();
}

} // end socktest_package
