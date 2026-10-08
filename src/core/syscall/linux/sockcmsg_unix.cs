// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix
// Socket control messages
namespace go;

using @unsafe = unsafe_package;
using ꓸꓸꓸnint = Span<nint>;

partial class syscall_package {

// CmsgLen returns the value to store in the Len field of the [Cmsghdr]
// structure, taking into account any necessary alignment.
public static nint CmsgLen(nint datalen) {
    return cmsgAlignOf(SizeofCmsghdr) + datalen;
}

// CmsgSpace returns the number of bytes an ancillary element with
// payload of the passed data length occupies.
public static nint CmsgSpace(nint datalen) {
    return cmsgAlignOf(SizeofCmsghdr) + cmsgAlignOf(datalen);
}

internal static @unsafe.Pointer data(this ж<Cmsghdr> Ꮡh, uintptr offset) {
    ref var h = ref Ꮡh.DerefOrNull();

    return (@unsafe.Pointer)((uintptr)(uintptr)@unsafe.Pointer.FromRef(ref h) + (uintptr)cmsgAlignOf(SizeofCmsghdr) + offset);
}

// SocketControlMessage represents a socket control message.
partial struct SocketControlMessage {
    public Cmsghdr Header;
    public slice<byte> Data;
}

// ParseSocketControlMessage parses b as an array of socket control
// messages.
public static (slice<SocketControlMessage>, error) ParseSocketControlMessage(slice<byte> b) {
    slice<SocketControlMessage> msgs = default!;
    nint i = 0;
    while (i + CmsgLen(0) <= len(b)) {
        var (h, dbuf, err) = socketControlMessageHeaderAndData(b.slice(i));
        if (err != default!) {
            return (default!, err);
        }
        var m = new SocketControlMessage(Header: h.Value, Data: dbuf);
        msgs = append(msgs, m);
        i += cmsgAlignOf((nint)(~h).Len);
    }
    return (msgs, default!);
}

internal static (ж<Cmsghdr>, slice<byte>, error) socketControlMessageHeaderAndData(slice<byte> b) {
    var h = Ꮡ(b, 0).Reinterpret<byte, Cmsghdr>();
    if ((~h).Len < SizeofCmsghdr || (uint64)(~h).Len > (uint64)len(b)) {
        return (default!, default!, EINVAL);
    }
    return (h, b.slice(cmsgAlignOf(SizeofCmsghdr), (nint)((~h).Len)), default!);
}

// UnixRights encodes a set of open file descriptors into a socket
// control message for sending to another process.
public static slice<byte> UnixRights(params ꓸꓸꓸnint fdsʗp) {
    var fds = fdsʗp.sslice();

    nint datalen = len(fds) * 4;
    var b = new slice<byte>(CmsgSpace(datalen));
    var h = Ꮡ(b, 0).Reinterpret<byte, Cmsghdr>();
    h.Value.Level = SOL_SOCKET;
    h.Value.Type = SCM_RIGHTS;
    h.SetLen(CmsgLen(datalen));
    foreach (var (i, fd) in fds) {
        ((ж<int32>)(uintptr)(h.data(4 * (uintptr)i))).Value = (int32)fd;
    }
    return b;
}

// ParseUnixRights decodes a socket control message that contains an
// integer array of open file descriptors from another process.
public static (slice<nint>, error) ParseUnixRights(ж<SocketControlMessage> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (m.Header.Level != SOL_SOCKET) {
        return (default!, EINVAL);
    }
    if (m.Header.Type != SCM_RIGHTS) {
        return (default!, EINVAL);
    }
    var fds = new slice<nint>((len(m.Data) >> (int)(2)));
    for ((nint i, nint j) = (0, 0); i < len(m.Data); i += 4) {
        fds[j] = (nint)(~Ꮡ(m.Data, i).Reinterpret<byte, int32>());
        j++;
    }
    return (fds, default!);
}

} // end syscall_package
