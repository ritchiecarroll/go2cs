// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Socket control messages
namespace go;

using @unsafe = unsafe_package;

partial class syscall_package {

// UnixCredentials encodes credentials into a socket control message
// for sending to another process. This can be used for
// authentication.
public static slice<byte> UnixCredentials(ж<Ucred> Ꮡucred) {
    ref var ucred = ref Ꮡucred.DerefOrNull();

    var b = new slice<byte>(CmsgSpace(SizeofUcred));
    var h = Ꮡ(b, 0).Reinterpret<byte, Cmsghdr>();
    h.Level = SOL_SOCKET;
    h.Type = SCM_CREDENTIALS;
    h.SetLen(CmsgLen(SizeofUcred));
    ((ж<Ucred>)(uintptr)(h.data(0))).Value = ucred;
    return b;
}

// ParseUnixCredentials decodes a socket control message that contains
// credentials in a Ucred structure. To receive such a message, the
// SO_PASSCRED option must be enabled on the socket.
public static (ж<Ucred>, error) ParseUnixCredentials(ж<SocketControlMessage> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (m.Header.Level != SOL_SOCKET) {
        return (default, EINVAL);
    }
    if (m.Header.Type != SCM_CREDENTIALS) {
        return (default, EINVAL);
    }
    if ((uintptr)len(m.Data) < /* unsafe.Sizeof(Ucred{}) */ (uintptr)12) {
        return (default, EINVAL);
    }
    ref var ucred = ref heap<Ucred>(~Ꮡ(m.Data, 0).Reinterpret<byte, Ucred>(), out var Ꮡucred);
    return (Ꮡucred, default);
}

} // end syscall_package
