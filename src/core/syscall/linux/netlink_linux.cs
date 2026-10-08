// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Netlink sockets and messages
namespace go;

using Δsync = sync_package;
using @unsafe = unsafe_package;

partial class syscall_package {

// Round the length of a netlink message up to align it properly.
internal static nint nlmAlignOf(nint msglen) {
    return (nint)((msglen + (nint)NLMSG_ALIGNTO - 1) & (nint)~(NLMSG_ALIGNTO - 1));
}

// Round the length of a netlink route attribute up to align it
// properly.
internal static nint rtaAlignOf(nint attrlen) {
    return (nint)((attrlen + (nint)RTA_ALIGNTO - 1) & (nint)~(RTA_ALIGNTO - 1));
}

// NetlinkRouteRequest represents a request message to receive routing
// and link states from the kernel.
partial struct NetlinkRouteRequest {
    public NlMsghdr Header;
    public RtGenmsg Data;
}

internal static slice<byte> toWireFormat(this ref NetlinkRouteRequest rr) {
    var b = new slice<byte>((nint)(rr.Header.Len));
    (Ꮡ(b[0..4], 0).Reinterpret<byte, uint32>()).Value = rr.Header.Len;
    (Ꮡ(b[4..6], 0).Reinterpret<byte, uint16>()).Value = rr.Header.Type;
    (Ꮡ(b[6..8], 0).Reinterpret<byte, uint16>()).Value = rr.Header.Flags;
    (Ꮡ(b[8..12], 0).Reinterpret<byte, uint32>()).Value = rr.Header.Seq;
    (Ꮡ(b[12..16], 0).Reinterpret<byte, uint32>()).Value = rr.Header.Pid;
    b[16] = rr.Data.Family;
    return b;
}

internal static slice<byte> newNetlinkRouteRequest(nint proto, nint seq, nint family) {
    var rr = Ꮡ(new NetlinkRouteRequest(nil));
    rr.Value.Header.Len = (uint32)(NLMSG_HDRLEN + SizeofRtGenmsg);
    rr.Value.Header.Type = (uint16)proto;
    rr.Value.Header.Flags = (uint16)((uint16)NLM_F_DUMP | (uint16)NLM_F_REQUEST);
    rr.Value.Header.Seq = (uint32)seq;
    rr.Value.Data.Family = (uint8)family;
    return rr.toWireFormat();
}

internal static ж<Δsync.Pool> pageBufPool = Ꮡ(new Δsync.Pool(New: () => {
    var b = new slice<byte>(Getpagesize());
    return Ꮡ(b);
}
));

// NetlinkRIB returns routing information base, as known as RIB, which
// consists of network facility information, states and parameters.
public static (slice<byte>, error) NetlinkRIB(nint proto, nint family) {
    GoFrame ᒐ = default;
    try {
        var (s, err) = Socket(AF_NETLINK, (nint)((nint)SOCK_RAW | (nint)SOCK_CLOEXEC), NETLINK_ROUTE);
        if (err != default!) {
            return (default!, err);
        }
        defer(Close, s, ref ᒐ);
        var sa = Ꮡ(new SockaddrNetlink(Family: AF_NETLINK));
        {
            var errΔ1 = Bind(s, new SockaddrNetlinkжSockaddr(sa)); if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
        }
        var wb = newNetlinkRouteRequest(proto, 1, family);
        {
            var errΔ2 = Sendto(s, wb, 0, new SockaddrNetlinkжSockaddr(sa)); if (errΔ2 != default!) {
                return (default!, errΔ2);
            }
        }
        (var lsa, err) = Getsockname(s);
        if (err != default!) {
            return (default!, err);
        }
        var (lsanl, ok) = lsa._<ж<SockaddrNetlink>>(ᐧ);
        if (!ok) {
            return (default!, EINVAL);
        }
        slice<byte> tab = default!;
        var rbNew = pageBufPool.Get()._<ж<slice<byte>>>();
        defer(pageBufPool.Put, rbNew.OrTypedNil(), ref ᒐ);
done:
        while (ᐧ) {
            var rb = rbNew.ValueSlot;
            var (nr, _, errΔ3) = Recvfrom(s, rb, 0);
            if (errΔ3 != default!) {
                return (default!, errΔ3);
            }
            if (nr < NLMSG_HDRLEN) {
                return (default!, EINVAL);
            }
            rb = rb.slice(0, nr);
            tab = appendꓸꓸꓸ(tab, rb);
            (var msgs, errΔ3) = ParseNetlinkMessage(rb);
            if (errΔ3 != default!) {
                return (default!, errΔ3);
            }
            foreach (var (_, m) in msgs) {
                if (m.Header.Seq != 1 || m.Header.Pid != (~lsanl).Pid) {
                    return (default!, EINVAL);
                }
                if (m.Header.Type == NLMSG_DONE) {
                    goto break_done;
                }
                if (m.Header.Type == NLMSG_ERROR) {
                    return (default!, EINVAL);
                }
            }
continue_done:;
        }
break_done:;
        return (tab, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// NetlinkMessage represents a netlink message.
partial struct NetlinkMessage {
    public NlMsghdr Header;
    public slice<byte> Data;
}

// ParseNetlinkMessage parses b as an array of netlink messages and
// returns the slice containing the NetlinkMessage structures.
public static (slice<NetlinkMessage>, error) ParseNetlinkMessage(slice<byte> b) {
    slice<NetlinkMessage> msgs = default!;
    while (len(b) >= NLMSG_HDRLEN) {
        var (h, dbuf, dlen, err) = netlinkMessageHeaderAndData(b);
        if (err != default!) {
            return (default!, err);
        }
        var m = new NetlinkMessage(Header: h.Value, Data: dbuf.slice(0, (nint)(~h).Len - (nint)NLMSG_HDRLEN));
        msgs = append(msgs, m);
        b = b.slice(dlen);
    }
    return (msgs, default!);
}

internal static (ж<NlMsghdr>, slice<byte>, nint, error) netlinkMessageHeaderAndData(slice<byte> b) {
    var h = Ꮡ(b, 0).Reinterpret<byte, NlMsghdr>();
    nint l = nlmAlignOf((nint)(~h).Len);
    if ((nint)(~h).Len < NLMSG_HDRLEN || l > len(b)) {
        return (default!, default!, 0, EINVAL);
    }
    return (h, b[(int)(NLMSG_HDRLEN)..], l, default!);
}

// NetlinkRouteAttr represents a netlink route attribute.
partial struct NetlinkRouteAttr {
    public RtAttr Attr;
    public slice<byte> Value;
}

// ParseNetlinkRouteAttr parses m's payload as an array of netlink
// route attributes and returns the slice containing the
// NetlinkRouteAttr structures.
public static (slice<NetlinkRouteAttr>, error) ParseNetlinkRouteAttr(ж<NetlinkMessage> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

    slice<byte> b = default!;
    var exprᴛ1 = m.Header.Type;
    if (exprᴛ1 == RTM_NEWLINK || exprᴛ1 == RTM_DELLINK) {
        b = m.Data[(int)(SizeofIfInfomsg)..];
    }
    else if (exprᴛ1 == RTM_NEWADDR || exprᴛ1 == RTM_DELADDR) {
        b = m.Data[(int)(SizeofIfAddrmsg)..];
    }
    else if (exprᴛ1 == RTM_NEWROUTE || exprᴛ1 == RTM_DELROUTE) {
        b = m.Data[(int)(SizeofRtMsg)..];
    }
    else { /* default: */
        return (default!, EINVAL);
    }

    slice<NetlinkRouteAttr> attrs = default!;
    while (len(b) >= SizeofRtAttr) {
        var (a, vbuf, alen, err) = netlinkRouteAttrAndValue(b);
        if (err != default!) {
            return (default!, err);
        }
        var ra = new NetlinkRouteAttr(Attr: a.Value, Value: vbuf.slice(0, (nint)(~a).Len - (nint)SizeofRtAttr));
        attrs = append(attrs, ra);
        b = b.slice(alen);
    }
    return (attrs, default!);
}

internal static (ж<RtAttr>, slice<byte>, nint, error) netlinkRouteAttrAndValue(slice<byte> b) {
    var a = Ꮡ(b, 0).Reinterpret<byte, RtAttr>();
    if ((nint)(~a).Len < SizeofRtAttr || (nint)(~a).Len > len(b)) {
        return (default!, default!, 0, EINVAL);
    }
    return (a, b[(int)(SizeofRtAttr)..], rtaAlignOf((nint)(~a).Len), default!);
}

} // end syscall_package
