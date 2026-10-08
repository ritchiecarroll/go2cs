// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using itoa = @internal.itoa_package;
using Δsync = sync_package;
using time = time_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using @internal;

partial class net_package {

// BUG(mikio): On JS, methods and functions related to
// Interface are not implemented.
// BUG(mikio): On AIX, DragonFly BSD, NetBSD, OpenBSD, Plan 9 and
// Solaris, the MulticastAddrs method of Interface is not implemented.
// errNoSuchInterface should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/sagernet/sing
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname errNoSuchInterface
internal static error errInvalidInterface = errors.New("invalid network interface"u8);
internal static error errInvalidInterfaceIndex = errors.New("invalid network interface index"u8);
internal static error errInvalidInterfaceName = errors.New("invalid network interface name"u8);
public static error errNoSuchInterface = errors.New("no such network interface"u8);
internal static error errNoSuchMulticastInterface = errors.New("no such multicast network interface"u8);

// Interface represents a mapping between network interface name
// and index. It also represents network interface facility
// information.
partial struct Interface {
    public nint Index;         // positive integer that starts at one, zero is never used
    public nint MTU;         // maximum transmission unit
    public @string Name;      // e.g., "en0", "lo0", "eth0.100"
    public HardwareAddr HardwareAddr; // IEEE MAC-48, EUI-48 and EUI-64 form
    public Flags Flags;        // e.g., FlagUp, FlagLoopback, FlagMulticast
}

partial struct Flags /*num:nuint*/;

public static Flags FlagUp => /* 1 << iota */ 1;                 // interface is administratively up
public static Flags FlagBroadcast => 2;          // interface supports broadcast access capability
public static Flags FlagLoopback => 4;           // interface is a loopback interface
public static Flags FlagPointToPoint => 8;       // interface belongs to a point-to-point link
public static Flags FlagMulticast => 16;          // interface supports multicast access capability
public static Flags FlagRunning => 32;            // interface is in running state

internal static slice<@string> flagNames = new @string[]{
    "up"u8,
    "broadcast"u8,
    "loopback"u8,
    "pointtopoint"u8,
    "multicast"u8,
    "running"u8
}.slice();

public static @string String(this Flags f) {
    @string s = ""u8;
    foreach (var (i, name) in flagNames) {
        if ((Flags)(f & ((Flags)((nuint)1 << (int)((nuint)i)))) != 0) {
            if (s != ""u8) {
                s += "|"u8;
            }
            s += name;
        }
    }
    if (s == ""u8) {
        s = "0"u8;
    }
    return s;
}

// Addrs returns a list of unicast interface addresses for a specific
// interface.
public static (slice<ΔAddr>, error) Addrs(this ж<Interface> Ꮡifi) {
    if (Ꮡifi == nil) {
        return (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "route"u8, Net: "ip+net"u8, Source: default!, Addr: default!, Err: errInvalidInterface))));
    }
    var (ifat, err) = interfaceAddrTable(Ꮡifi);
    if (err != default!) {
        err = new OpErrorжerror(Ꮡ(new OpError(Op: "route"u8, Net: "ip+net"u8, Source: default!, Addr: default!, Err: err)));
    }
    return (ifat, err);
}

// MulticastAddrs returns a list of multicast, joined group addresses
// for a specific interface.
public static (slice<ΔAddr>, error) MulticastAddrs(this ж<Interface> Ꮡifi) {
    if (Ꮡifi == nil) {
        return (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "route"u8, Net: "ip+net"u8, Source: default!, Addr: default!, Err: errInvalidInterface))));
    }
    var (ifat, err) = interfaceMulticastAddrTable(Ꮡifi);
    if (err != default!) {
        err = new OpErrorжerror(Ꮡ(new OpError(Op: "route"u8, Net: "ip+net"u8, Source: default!, Addr: default!, Err: err)));
    }
    return (ifat, err);
}

// Interfaces returns a list of the system's network interfaces.
public static (slice<Interface>, error) Interfaces() {
    var (ift, err) = interfaceTable(0);
    if (err != default!) {
        return (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "route"u8, Net: "ip+net"u8, Source: default!, Addr: default!, Err: err))));
    }
    if (len(ift) != 0) {
        ᏑzoneCache.update(ift, false);
    }
    return (ift, default!);
}

// InterfaceAddrs returns a list of the system's unicast interface
// addresses.
//
// The returned list does not identify the associated interface; use
// Interfaces and [Interface.Addrs] for more detail.
public static (slice<ΔAddr>, error) InterfaceAddrs() {
    var (ifat, err) = interfaceAddrTable(nil);
    if (err != default!) {
        err = new OpErrorжerror(Ꮡ(new OpError(Op: "route"u8, Net: "ip+net"u8, Source: default!, Addr: default!, Err: err)));
    }
    return (ifat, err);
}

// InterfaceByIndex returns the interface specified by index.
//
// On Solaris, it returns one of the logical network interfaces
// sharing the logical data link; for more precision use
// [InterfaceByName].
public static (ж<Interface>, error) InterfaceByIndex(nint index) {
    if (index <= 0) {
        return (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "route"u8, Net: "ip+net"u8, Source: default!, Addr: default!, Err: errInvalidInterfaceIndex))));
    }
    var (ift, err) = interfaceTable(index);
    if (err != default!) {
        return (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "route"u8, Net: "ip+net"u8, Source: default!, Addr: default!, Err: err))));
    }
    (var ifi, err) = interfaceByIndex(ift, index);
    if (err != default!) {
        err = new OpErrorжerror(Ꮡ(new OpError(Op: "route"u8, Net: "ip+net"u8, Source: default!, Addr: default!, Err: err)));
    }
    return (ifi, err);
}

internal static (ж<Interface>, error) interfaceByIndex(slice<Interface> ift, nint index) {
    foreach (var (_, vᴛ1) in ift) {
        ref var ifi = ref heap(new Interface(), out var Ꮡifi);
        ifi = vᴛ1;

        if (index == ifi.Index) {
            return (Ꮡifi, default!);
        }
    }
    return (default!, errNoSuchInterface);
}

// InterfaceByName returns the interface specified by name.
public static (ж<Interface>, error) InterfaceByName(@string name) {
    if (name == ""u8) {
        return (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "route"u8, Net: "ip+net"u8, Source: default!, Addr: default!, Err: errInvalidInterfaceName))));
    }
    var (ift, err) = interfaceTable(0);
    if (err != default!) {
        return (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "route"u8, Net: "ip+net"u8, Source: default!, Addr: default!, Err: err))));
    }
    if (len(ift) != 0) {
        ᏑzoneCache.update(ift, false);
    }
    foreach (var (_, vᴛ1) in ift) {
        ref var ifi = ref heap(new Interface(), out var Ꮡifi);
        ifi = vᴛ1;

        if (name == ifi.Name) {
            return (Ꮡifi, default!);
        }
    }
    return (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "route"u8, Net: "ip+net"u8, Source: default!, Addr: default!, Err: errNoSuchInterface))));
}

// An ipv6ZoneCache represents a cache holding partial network
// interface information. It is used for reducing the cost of IPv6
// addressing scope zone resolution.
//
// Multiple names sharing the index are managed by first-come
// first-served basis for consistency.
partial struct ipv6ZoneCache {
    public partial ref sync_package.RWMutex RWMutex { get; }                // guard the following
    internal time.Time lastFetched;      // last time routing information was fetched
    internal map<@string, nint> toIndex; // interface name to its index
    internal map<nint, @string> toName; // interface index to its name
}

internal static ж<ipv6ZoneCache> ᏑzoneCache = new StandardBox<ipv6ZoneCache>(new ipv6ZoneCache(
    toIndex: new map<@string, nint>(),
    toName: new map<nint, @string>()
));
internal static ref ipv6ZoneCache zoneCache => ref ᏑzoneCache.Value;

// update refreshes the network interface information if the cache was last
// updated more than 1 minute ago, or if force is set. It reports whether the
// cache was updated.
internal static bool /*updated*/ update(this ж<ipv6ZoneCache> Ꮡzc, slice<Interface> ift, bool force) {
    bool updated = default!;
    GoFrame ᒐ = default;
    try {
        ref var zc = ref Ꮡzc.DerefOrNull();

        Ꮡzc.of(ipv6ZoneCache.ᏑRWMutex).Lock();
        defer(Ꮡzc.of(ipv6ZoneCache.ᏑRWMutex).Unlock, ref ᒐ);
        var now = time.Now();
        if (!force && zc.lastFetched.After(now.Add((time.Duration)(-60000000000L)))) {
            updated = false; goto ᒐdone;
        }
        zc.lastFetched = now;
        if (len(ift) == 0) {
            error err = default!;
            {
                (ift, err) = interfaceTable(0); if (err != default!) {
                    updated = false; goto ᒐdone;
                }
            }
        }
        zc.toIndex = new map<@string, nint>(len(ift));
        zc.toName = new map<nint, @string>(len(ift));
        foreach (var (_, ifi) in ift) {
            zc.toIndex[ifi.Name] = ifi.Index;
            {
                var (_, ok) = zc.toName[ifi.Index, ꟷ]; if (!ok) {
                    zc.toName[ifi.Index] = ifi.Name;
                }
            }
        }
        updated = true;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return updated;
}

internal static @string name(this ref ipv6ZoneCache zc, nint index) {
    if (index == 0) {
        return ""u8;
    }
    var updated = ᏑzoneCache.update(default!, false);
    ᏑzoneCache.of(ipv6ZoneCache.ᏑRWMutex).RLock();
    var (name, ok) = zoneCache.toName[index, ꟷ];
    ᏑzoneCache.of(ipv6ZoneCache.ᏑRWMutex).RUnlock();
    if (!ok && !updated) {
        ᏑzoneCache.update(default!, true);
        ᏑzoneCache.of(ipv6ZoneCache.ᏑRWMutex).RLock();
        (name, ok) = zoneCache.toName[index, ꟷ];
        ᏑzoneCache.of(ipv6ZoneCache.ᏑRWMutex).RUnlock();
    }
    if (!ok) {
        // last resort
        name = itoa.Uitoa((nuint)index);
    }
    return name;
}

internal static nint index(this ref ipv6ZoneCache zc, @string name) {
    if (name == ""u8) {
        return 0;
    }
    var updated = ᏑzoneCache.update(default!, false);
    ᏑzoneCache.of(ipv6ZoneCache.ᏑRWMutex).RLock();
    var (index, ok) = zoneCache.toIndex[name, ꟷ];
    ᏑzoneCache.of(ipv6ZoneCache.ᏑRWMutex).RUnlock();
    if (!ok && !updated) {
        ᏑzoneCache.update(default!, true);
        ᏑzoneCache.of(ipv6ZoneCache.ᏑRWMutex).RLock();
        (index, ok) = zoneCache.toIndex[name, ꟷ];
        ᏑzoneCache.of(ipv6ZoneCache.ᏑRWMutex).RUnlock();
    }
    if (!ok) {
        // last resort
        (index, _, _) = dtoi(name);
    }
    return index;
}

} // end net_package
