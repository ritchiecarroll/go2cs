// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class syscall_package {

partial struct WSAData {
    public uint16 Version;
    public uint16 HighVersion;
    public uint16 MaxSockets;
    public uint16 MaxUdpDg;
    public ж<byte> VendorInfo;
    public array<byte> Description = new(WSADESCRIPTION_LEN + 1);
    public array<byte> SystemStatus = new(WSASYS_STATUS_LEN + 1);
}

partial struct Servent {
    public ж<byte> Name;
    public ж<ж<byte>> Aliases;
    public ж<byte> Proto;
    public uint16 Port;
}

} // end syscall_package
