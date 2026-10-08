// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using os = os_package;

partial class net_package {

partial struct fileAddr /*@string*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fileNetˢ = "file+net"u8;

// BUG(mikio): On JS and Windows, the FileConn, FileListener and
// FilePacketConn functions are not implemented.
internal static @string Network(this fileAddr _) {
    return fileNetˢ;
}

internal static @string String(this fileAddr f) {
    return ((@string)f);
}

// FileConn returns a copy of the network connection corresponding to
// the open file f.
// It is the caller's responsibility to close f when finished.
// Closing c does not affect f, and closing f does not affect c.
public static (Conn c, error err) FileConn(ж<os.File> Ꮡf) {
    Conn c = default!;
    error err = default!;

    ref var f = ref Ꮡf.DerefOrNull();
    (c, err) = fileConn(Ꮡf);
    if (err != default!) {
        err = new OpErrorжerror(Ꮡ(new OpError(Op: "file"u8, Net: "file+net"u8, Source: default!, Addr: ((fileAddr)f.Name()), Err: err)));
    }
    return (c, err);
}

// FileListener returns a copy of the network listener corresponding
// to the open file f.
// It is the caller's responsibility to close ln when finished.
// Closing ln does not affect f, and closing f does not affect ln.
public static (Listener ln, error err) FileListener(ж<os.File> Ꮡf) {
    Listener ln = default!;
    error err = default!;

    ref var f = ref Ꮡf.DerefOrNull();
    (ln, err) = fileListener(Ꮡf);
    if (err != default!) {
        err = new OpErrorжerror(Ꮡ(new OpError(Op: "file"u8, Net: "file+net"u8, Source: default!, Addr: ((fileAddr)f.Name()), Err: err)));
    }
    return (ln, err);
}

// FilePacketConn returns a copy of the packet network connection
// corresponding to the open file f.
// It is the caller's responsibility to close f when finished.
// Closing c does not affect f, and closing f does not affect c.
public static (PacketConn c, error err) FilePacketConn(ж<os.File> Ꮡf) {
    PacketConn c = default!;
    error err = default!;

    ref var f = ref Ꮡf.DerefOrNull();
    (c, err) = filePacketConn(Ꮡf);
    if (err != default!) {
        err = new OpErrorжerror(Ꮡ(new OpError(Op: "file"u8, Net: "file+net"u8, Source: default!, Addr: ((fileAddr)f.Name()), Err: err)));
    }
    return (c, err);
}

} // end net_package
