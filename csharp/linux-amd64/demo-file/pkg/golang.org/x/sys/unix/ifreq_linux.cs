// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using @unsafe = unsafe_package;
using static unix_package;

partial class unix_package {

partial struct Ifreq {
    internal array<int64> _ = new(0);
    internal ifreq raw;
}

public static (ж<Ifreq>, error) NewIfreq(@string name) {
    if (len(name) >= IFNAMSIZ) {
        return (default, EINVAL);
    }

    ref var ifr = ref heap(new ifreq(), out var Ꮡifr);
    copy(ifr.Ifrn[..], name);

    return (Ꮡ(new Ifreq(raw: ifr.ΔClone())), default);
}

} partial class unixᴛIfreq {

public static @string Name(this ref Ifreq ifr) {
    return ByteSliceToString(ifr.raw.Ifrn[..]);
}

public static (slice<byte>, error) Inet4Addr(this ref Ifreq ifr) {
    var raw = (Ꮡ(ifr.raw.Ifru[..(int)(SizeofSockaddrInet4)], 0).Reinterpret<byte, RawSockaddrInet4>()).GoValue().ΔClone();
    if (raw.Family != AF_INET) {
        return (default, EINVAL);
    }

    return (raw.Addr[..], default);
}

public static error SetInet4Addr(this ref Ifreq ifr, slice<byte> v) {
    if (len(v) != 4) {
        return EINVAL;
    }

    array<byte> addr = new(4);
    copy(addr[..], v);

    ifr.clear();
    (Ꮡ(ifr.raw.Ifru[..(int)(SizeofSockaddrInet4)], 0).Reinterpret<byte, RawSockaddrInet4>()).Value.AssignFrom(new RawSockaddrInet4(
        Family: AF_INET,
        Addr: addr.Clone()
    ));

    return default;
}

public static uint16 Uint16(this ref Ifreq ifr) {
    return ~Ꮡ(ifr.raw.Ifru[..2], 0).Reinterpret<byte, uint16>();
}

public static void SetUint16(this ref Ifreq ifr, uint16 v) {
    ifr.clear();
    (Ꮡ(ifr.raw.Ifru[..2], 0).Reinterpret<byte, uint16>()).Value = v;
}

public static uint32 Uint32(this ref Ifreq ifr) {
    return ~Ꮡ(ifr.raw.Ifru[..4], 0).Reinterpret<byte, uint32>();
}

public static void SetUint32(this ref Ifreq ifr, uint32 v) {
    ifr.clear();
    (Ꮡ(ifr.raw.Ifru[..4], 0).Reinterpret<byte, uint32>()).Value = v;
}

internal static void clear(this ref Ifreq ifr) {
    builtin.clear(ifr.raw.Ifru[..]);
}

} partial class unix_package {

partial struct ifreqData {
    internal array<byte> name = new(IFNAMSIZ);
    internal @unsafe.Pointer data;
    internal array<byte> _ = new(len(new ifreq().Ifru) - (nint)SizeofPtr);
}

} partial class unixᴛIfreq {

internal static ifreqData withData(this Ifreq ifr, @unsafe.Pointer p) {
    ifr = ifr.ΔClone();

    return new ifreqData(
        name: ifr.raw.Ifrn.Clone(),
        data: p
    );
}

} // end unixᴛIfreq
