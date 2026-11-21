// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || dragonfly || freebsd || netbsd || openbsd
namespace go.@internal;

using static routebsd_package;

partial class routebsd_package {

// An InterfaceMessage represents an interface message.
partial struct InterfaceMessage {
    public nint Version;      // message version
    public nint Type;         // message type
    public nint Flags;        // interface flags
    public nint Index;        // interface index
    public @string Name;      // interface name
    public slice<Addr> Addrs; // addresses
    internal nint extOff;     // offset of header extension
    internal slice<byte> raw; // raw message
}

// An InterfaceAddrMessage represents an interface address message.
partial struct InterfaceAddrMessage {
    public nint Version;      // message version
    public nint Type;         // message type
    public nint Flags;        // interface flags
    public nint Index;        // interface index
    public slice<Addr> Addrs; // addresses
    internal slice<byte> raw; // raw message
}

// An InterfaceMulticastAddrMessage represents an interface multicast
// address message.
partial struct InterfaceMulticastAddrMessage {
    public nint Version;      // message version
    public nint Type;         // message type
    public nint Flags;        // interface flags
    public nint Index;        // interface index
    public slice<Addr> Addrs; // addresses
    internal slice<byte> raw; // raw message
}

} partial class routebsd_package {

// Implement the Message interface.
[GoMethodBody("message")] internal static void messageᴍ(InterfaceMessage _) { }

} partial class routebsd_package {

[GoMethodBody("message")] internal static void messageᴍ(InterfaceAddrMessage _) { }

} partial class routebsd_package {

[GoMethodBody("message")] internal static void messageᴍ(InterfaceMulticastAddrMessage _) { }

}
