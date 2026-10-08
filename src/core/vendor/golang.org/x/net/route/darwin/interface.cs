// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || dragonfly || freebsd || netbsd || openbsd
namespace go.vendor.golang.org.x.net;

partial class route_package {

// An InterfaceMessage represents an interface message.
partial struct InterfaceMessage {
    public nint Version;   // message version
    public nint Type;   // message type
    public nint Flags;   // interface flags
    public nint Index;   // interface index
    public @string Name; // interface name
    public slice<Addr> Addrs; // addresses
    internal nint extOff;   // offset of header extension
    internal slice<byte> raw; // raw message
}

// An InterfaceAddrMessage represents an interface address message.
partial struct InterfaceAddrMessage {
    public nint Version;   // message version
    public nint Type;   // message type
    public nint Flags;   // interface flags
    public nint Index;   // interface index
    public slice<Addr> Addrs; // addresses
    internal slice<byte> raw; // raw message
}

// Sys implements the Sys method of Message interface.
public static slice<ΔSys> Sys(this ref InterfaceAddrMessage m) {
    return default!;
}

// An InterfaceMulticastAddrMessage represents an interface multicast
// address message.
partial struct InterfaceMulticastAddrMessage {
    public nint Version;   // message version
    public nint Type;   // message type
    public nint Flags;   // interface flags
    public nint Index;   // interface index
    public slice<Addr> Addrs; // addresses
    internal slice<byte> raw; // raw message
}

// Sys implements the Sys method of Message interface.
public static slice<ΔSys> Sys(this ref InterfaceMulticastAddrMessage m) {
    return default!;
}

// An InterfaceAnnounceMessage represents an interface announcement
// message.
partial struct InterfaceAnnounceMessage {
    public nint Version;   // message version
    public nint Type;   // message type
    public nint Index;   // interface index
    public @string Name; // interface name
    public nint What;   // what type of announcement
    internal slice<byte> raw; // raw message
}

// Sys implements the Sys method of Message interface.
public static slice<ΔSys> Sys(this ref InterfaceAnnounceMessage m) {
    return default!;
}

} // end route_package
