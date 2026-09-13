// fd_windows_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementations of fd_windows.go's FOUR raw-sockaddr converters -- the two DECODERS
// rawToSockaddrInet4/rawToSockaddrInet6 and the two ENCODERS sockaddrInet4ToRaw/sockaddrInet6ToRaw --
// which between them are the whole managed-representation obstacle on the Windows datagram path
// (docs/phase4/DESIGN-netpoll-managed-poller.md §4.8, RATIFIED).
//
// The decoders came first, with this file; the encoders followed on 2026-08-26 once a measurement
// reached them. The two halves fail through the SAME two mechanisms, described below in the reading
// direction, and the encoders' section at the end of this header says what changes when those
// mechanisms run backwards -- which is the difference between a wrong answer and a dead process.
//
// WHY THEY CANNOT BE CONVERTED. Go reads the address by pointer arithmetic over flat bytes:
//
//     pp := (*syscall.RawSockaddrInet4)(unsafe.Pointer(rsa))
//     p  := (*[2]byte)(unsafe.Pointer(&pp.Port))
//     sa.Port = int(p[0])<<8 + int(p[1])
//     sa.Addr = pp.Addr
//
// Neither line survives the managed representation, and they fail in DIFFERENT ways -- which is why
// this is a hand-own rather than a patch to either mechanism:
//
//   1. The REINTERPRET. `Ꮡrsa.Reinterpret<RawSockaddrAny, RawSockaddrInet4>()` asks golib to alias
//      one reference-bearing struct as another. The two managed layouts share no field offsets at
//      all: RawSockaddrAny holds `int8[14]` and `int8[100]` OBJECT REFERENCES where sockaddr_in has
//      four inline octets. Measured -- the reinterpreted `Addr` reports Length=14 (that is
//      RawSockaddr.Data) and `Zero` reports Length=100 (that is Pad). Line 4 above therefore clones
//      the WRONG FIELD, not a mis-sized one.
//   2. The BYTE VIEW. `(ж<array<byte>>)(uintptr)(…ᏑPort)` reinterprets the pointed-at bytes as a
//      managed `array<byte>` STRUCT -- whose first field is a `T[]` reference. Over zeroed memory it
//      reads null and yields Length=0 (a silent wrong answer); over real data it fabricates a
//      managed reference out of the sockaddr's own bytes and dereferences it. That is the
//      corpus-wide class DESIGN-native-array-view.md exists to close; this file does not wait for it.
//
// WHAT THEY DO INSTEAD is not a third mechanism but the one the package next door already ships.
// `Δsyscall.RawSockaddrAny.Sockaddr()` is hand-owned and does exactly this decode correctly: it
// FLATTENS the managed struct back to the 116-byte native image its fields are a transcription of
// (Family at 0, Addr.Data covering 2..15, Pad covering 16..115) and hands that to the single
// definition of the decode. Routing through it means the sockaddr layout is spelled in exactly one
// place in the corpus, and these two functions carry none of it.
//
// The managed image is faithful because the SUBMIT seam makes it so -- syscall's hand-owned
// WSARecvFrom stages a native buffer, lets the kernel write THERE, and transcribes the result into
// this box at harvest. Without that half the box holds whatever the kernel scribbled over a
// forty-byte managed object, and no decode could help. The two are a pair, exactly as
// AcceptEx/GetAcceptExSockaddrs are.
//
// ONE DELIBERATE DIVERGENCE, and it is in the safe direction. Go blindly reinterprets whatever
// family the raw address carries; these check it, and leave `sa` at its zero value on a mismatch
// rather than filling it with nonsense. Every caller (ReadFromInet4/6, ReadMsgInet4/6) has already
// bound the socket to that family, so a mismatch is unreachable in practice -- and where Go would
// hand back a garbage address, this hands back an empty one.
//
// THE ENCODERS: THE SAME TWO MECHANISMS, WRITING. sockaddrInet4ToRaw/sockaddrInet6ToRaw run the
// identical reinterpret-then-byte-view sequence in reverse:
//
//     *rsa = syscall.RawSockaddrAny{}
//     raw := (*syscall.RawSockaddrInet6)(unsafe.Pointer(rsa))
//     raw.Family = syscall.AF_INET6
//     p := (*[2]byte)(unsafe.Pointer(&raw.Port))
//     p[0], p[1] = byte(sa.Port>>8), byte(sa.Port)
//
// Reading the wrong offsets returns a wrong answer; WRITING them corrupts the heap. Measured on the
// CLR: RawSockaddrAny holds object REFERENCES at offsets 8 and 24, so `raw.Value.Family = AF_INET6`
// deposits a uint16 over the low half of a live reference -- after which the death site MOVES from
// run to run, which is the board's signature for this family. The v4 twin escapes that by layout
// ACCIDENT (its Family lands where nothing live sits) and still dies on the byte view, with
// `index out of range [0] with length 0`; the v6 twin reports the same panic with a garbage NEGATIVE
// length, because its view is fabricated out of reference bytes rather than zeros. Both are fixed
// here: an accident is not a contract, and the accident is a field OFFSET, which any future
// declaration change would move.
//
// WHAT THEY DO INSTEAD is again the package next door's one definition, and again nothing more.
// `Δsyscall.GoRawSockaddrFromInet4/6` builds the native image through the same writeNativeSockaddr
// every other Windows sockaddr consumer uses, then transcribes it into the managed RawSockaddrAny's
// field encoding -- the exact inverse of the flatten `Sockaddr()` performs for the decoders above,
// naming the same helper pair. So all four functions in this file carry Go field names and no
// layout, and the encode cannot drift from the decode.
//
// ⚠ THE SUBMIT SIDE HELD TWO MORE DEFECTS, and the pairing note above applies in reverse: a faithful
// managed `rsa` is NECESSARY for a correct WriteMsg and is not SUFFICIENT. Fixing only these two
// functions moved the failure from a dead host to a clean
// `wsasendmsg: An invalid argument was supplied` -- which turned out to name a lookup rather than the
// send. Both are hand-owned in internal/syscall/windows/windows/syscall_windows_impl.cs, whose header
// carries the full chain; together with these two, WriteMsgUDP and WriteMsgUDPAddrPort work on both
// families (the UdpWriteMsgAddrPort guard).
//
// The HARVEST twin, WSARecvMsg, held the identical defect in the opposite direction and is hand-owned
// in that same file now, which is what makes the DECODERS at the top of this file reachable from
// ReadMsg/ReadMsgInet4/ReadMsgInet6 at all rather than only from ReadFrom's recvfrom path. All four
// functions here are therefore live on both directions of the Windows datagram surface, and the
// UdpWriteMsgAddrPort guard round-trips them: WriteMsg out through the encoders, ReadMsg back through
// the decoders.

using System;
using golib = go.golib;

// Hand-owned (no fd_windows_impl.go exists, so a reconvert never regenerates this file); the two
// declarations it replaces are registered in the converter's manualConversionFuncs, which is what
// turns their generated bodies into placeholders.
[module: go.GoManualConversion]

namespace go.@internal;

// Δsyscall, not syscall: the enclosing namespace go.@internal already contains a `syscall`
// (internal/syscall), and a plain alias collides with it (CS0576). The generated fd_windows.cs
// spells it the same way for the same reason.
using Δsyscall = go.syscall_package;

partial class poll_package
{
    internal static void rawToSockaddrInet4(ж<Δsyscall.RawSockaddrAny> Ꮡrsa, ref Δsyscall.SockaddrInet4 sa) {
        var (decoded, err) = Ꮡrsa.Sockaddr();

        if (err != default!) {
            return;
        }
        if (decoded.type() is ж<Δsyscall.SockaddrInet4> Ꮡv4) {
            sa.Port = (~Ꮡv4).Port;
            sa.Addr = (~Ꮡv4).Addr.Clone();
        }
    }

    internal static void rawToSockaddrInet6(ж<Δsyscall.RawSockaddrAny> Ꮡrsa, ref Δsyscall.SockaddrInet6 sa) {
        var (decoded, err) = Ꮡrsa.Sockaddr();

        if (err != default!) {
            return;
        }
        if (decoded.type() is ж<Δsyscall.SockaddrInet6> Ꮡv6) {
            sa.Port = (~Ꮡv6).Port;
            sa.ZoneId = (~Ꮡv6).ZoneId;
            sa.Addr = (~Ꮡv6).Addr.Clone();
        }
    }

    // The encoders. Each reads exactly the fields its decode sibling above writes, and hands them to
    // the one definition of what a Go Sockaddr looks like in a raw address; the zeroing of `*rsa` and
    // the returned sizeof both live there, because both are properties of that image rather than of
    // this package. See the file header for why writing these offsets is a heap-corruption class
    // where reading them is only a wrong-answer one.
    internal static int32 sockaddrInet4ToRaw(ж<Δsyscall.RawSockaddrAny> Ꮡrsa, ref Δsyscall.SockaddrInet4 sa) {
        return Δsyscall.GoRawSockaddrFromInet4(Ꮡrsa, sa.Port, sa.Addr);
    }

    internal static int32 sockaddrInet6ToRaw(ж<Δsyscall.RawSockaddrAny> Ꮡrsa, ref Δsyscall.SockaddrInet6 sa) {
        return Δsyscall.GoRawSockaddrFromInet6(Ꮡrsa, sa.Port, sa.ZoneId, sa.Addr);
    }
}
