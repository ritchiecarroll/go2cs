// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using @unsafe = unsafe_package;
using ꓸꓸꓸnint = Span<nint>;
using static unix_package;

partial class unix_package {

public static nint CmsgLen(nint datalen) {
    return cmsgAlignOf(SizeofCmsghdr) + datalen;
}

public static nint CmsgSpace(nint datalen) {
    return cmsgAlignOf(SizeofCmsghdr) + cmsgAlignOf(datalen);
}

} partial class unixᴛCmsghdr {

internal static @unsafe.Pointer data(this ж<Cmsghdr> Ꮡh, uintptr offset) {
    return @unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡh), (uintptr)cmsgAlignOf(SizeofCmsghdr) + offset);
}

} partial class unix_package {

partial struct SocketControlMessage {
    public Cmsghdr Header;
    public slice<byte> Data;
}

public static (slice<SocketControlMessage>, error) ParseSocketControlMessage(slice<byte> b) {
    slice<SocketControlMessage> msgs = default;
    nint i = 0;
    while (i + CmsgLen(0) <= len(b)) {
        var (h, dbuf, err) = socketControlMessageHeaderAndData(b.slice(i));
        if (err != nil) {
            return (default, err);
        }
        var m = new SocketControlMessage(Header: h.Value, Data: dbuf);
        msgs = append(msgs, m);
        i += cmsgAlignOf((nint)h.Len);
    }
    return (msgs, default);
}

public static (Cmsghdr hdr, slice<byte> data, slice<byte> remainder, error err) ParseOneSocketControlMessage(slice<byte> b) {
    slice<byte> remainder = default;
    error err = default;

    (var h, var dbuf, err) = socketControlMessageHeaderAndData(b);
    if (err != nil) {
        return (new Cmsghdr(), default, default, err);
    }
    nint i = cmsgAlignOf((nint)h.Len); if (i < len(b)) {
        remainder = b.slice(i);
    }
    return (h.Value, dbuf, remainder, default);
}

internal static (ж<Cmsghdr>, slice<byte>, error) socketControlMessageHeaderAndData(slice<byte> b) {
    var h = Ꮡ(b, 0).Reinterpret<byte, Cmsghdr>();
    if (h.Len < SizeofCmsghdr || (uint64)h.Len > (uint64)len(b)) {
        return (default, default, EINVAL);
    }
    return (h, b.slice(cmsgAlignOf(SizeofCmsghdr), (nint)(h.Len)), default);
}

public static slice<byte> UnixRights(params ꓸꓸꓸnint fdsʗp) {
    var fds = fdsʗp.sslice();

    nint datalen = len(fds) * 4;
    var b = new slice<byte>(CmsgSpace(datalen));
    var h = Ꮡ(b, 0).Reinterpret<byte, Cmsghdr>();
    h.Level = SOL_SOCKET;
    h.Type = SCM_RIGHTS;
    h.SetLen(CmsgLen(datalen));
    foreach (var (i, fd) in range(fds)) {
        (@unsafe.Pointer.To<int32>(h.data(4 * (uintptr)i))).Value = (int32)fd;
    }
    return b;
}

public static (slice<nint>, error) ParseUnixRights(ж<SocketControlMessage> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (m.Header.Level != SOL_SOCKET) {
        return (default, EINVAL);
    }
    if (m.Header.Type != SCM_RIGHTS) {
        return (default, EINVAL);
    }
    var fds = new slice<nint>((len(m.Data) >> 2));
    for ((nint i, nint j) = (0, 0); i < len(m.Data); i += 4) {
        fds[j] = (nint)(~Ꮡ(m.Data, i).Reinterpret<byte, int32>());
        j++;
    }
    return (fds, default);
}

} // end unix_package
