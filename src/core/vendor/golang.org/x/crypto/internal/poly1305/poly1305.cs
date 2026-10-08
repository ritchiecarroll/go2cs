// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package poly1305 implements Poly1305 one-time message authentication code as
// specified in https://cr.yp.to/mac/poly1305-20050329.pdf.
//
// Poly1305 is a fast, one-time authentication function. It is infeasible for an
// attacker to generate an authenticator for a message without the key. However, a
// key must only be used for a single message. Authenticating two different
// messages with the same key allows an attacker to forge authenticators for other
// messages with the same key.
//
// Poly1305 was originally coupled with AES in order to make Poly1305-AES. AES was
// used with a fixed key in order to generate one-time keys from an nonce.
// However, in this package AES isn't used and the one-time key is specified
// directly.
namespace go.vendor.golang.org.x.crypto.@internal;

using subtle = go.crypto.subtle_package;
using go.crypto;

partial class poly1305_package {

// TagSize is the size, in bytes, of a poly1305 authenticator.
public static UntypedInt TagSize => 16;

// Sum generates an authenticator for msg using a one-time key and puts the
// 16-byte result into out. Authenticating two different messages with the same
// key allows an attacker to forge messages at will.
public static void Sum(/*[16]*/ ж<array<byte>> Ꮡout, slice<byte> m, /*[32]*/ ж<array<byte>> Ꮡkey) {
    ref var @out = ref Ꮡout.DerefOrNull();

    var h = New(Ꮡkey);
    h.Write(m);
    h.Sum(@out[..0]);
}

// Verify returns true if mac is a valid authenticator for m with the given key.
public static bool Verify(/*[16]*/ ж<array<byte>> Ꮡmac, slice<byte> m, /*[32]*/ ж<array<byte>> Ꮡkey) {
    ref var mac = ref Ꮡmac.DerefOrNull();

    ref var tmp = ref heap(new array<byte>(16), out var Ꮡtmp);
    Sum(Ꮡtmp, m, Ꮡkey);
    return subtle.ConstantTimeCompare(tmp[..], mac[..]) == 1;
}

// New returns a new MAC computing an authentication
// tag of all data written to it with the given key.
// This allows writing the message progressively instead
// of passing it as a single slice. Common users should use
// the Sum function instead.
//
// The key must be unique for each message, as authenticating
// two different messages with the same key allows an attacker
// to forge messages at will.
public static ж<MAC> New(/*[32]*/ ж<array<byte>> Ꮡkey) {
    var m = Ꮡ(new MAC(nil));
    initialize(Ꮡkey, ref (m.of(MAC.ᏑmacState)).DerefOrNull());
    return m;
}

// MAC is an io.Writer computing an authentication tag
// of the data written to it.
//
// MAC cannot be used like common hash.Hash implementations,
// because using a poly1305 key twice breaks its security.
// Therefore writing data to a running MAC after calling
// Sum or Verify causes it to panic.
partial struct MAC {
    internal partial ref mac mac { get; } // platform-dependent implementation
    internal bool finalized;
}

// Size returns the number of bytes Sum will return.
public static nint Size(this ref MAC h) {
    return TagSize;
}

// Write adds more data to the running message authentication code.
// It never returns an error.
//
// It must not be called after the first call of Sum or Verify.
public static (nint n, error err) Write(this ж<MAC> Ꮡh, slice<byte> p) {
    ref var h = ref Ꮡh.DerefOrNull();

    if (h.finalized) {
        throw panic("poly1305: write to MAC after Sum or Verify");
    }
    return Ꮡh.of(MAC.Ꮡmac).of(mac.ᏑmacGeneric).Write(p);
}

// Sum computes the authenticator of all data written to the
// message authentication code.
public static slice<byte> Sum(this ref MAC h, slice<byte> b) {
    ref var mac = ref heap(new array<byte>(16), out var Ꮡmac);
    h.mac.macGeneric.Sum(Ꮡmac);
    h.finalized = true;
    return appendꓸꓸꓸ(b, mac[..]);
}

// Verify returns whether the authenticator of all data written to
// the message authentication code matches the expected value.
public static bool Verify(this ref MAC h, slice<byte> expected) {
    ref var mac = ref heap(new array<byte>(16), out var Ꮡmac);
    h.mac.macGeneric.Sum(Ꮡmac);
    h.finalized = true;
    return subtle.ConstantTimeCompare(expected, mac[..]) == 1;
}

} // end poly1305_package
