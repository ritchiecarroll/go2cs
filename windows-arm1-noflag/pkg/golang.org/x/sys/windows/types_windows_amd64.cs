// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

partial class windows_package {

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

partial struct JOBOBJECT_BASIC_LIMIT_INFORMATION {
    public int64 PerProcessUserTimeLimit;
    public int64 PerJobUserTimeLimit;
    public uint32 LimitFlags;
    public uintptr MinimumWorkingSetSize;
    public uintptr MaximumWorkingSetSize;
    public uint32 ActiveProcessLimit;
    public uintptr Affinity;
    public uint32 PriorityClass;
    public uint32 SchedulingClass;
}

} // end windows_package
