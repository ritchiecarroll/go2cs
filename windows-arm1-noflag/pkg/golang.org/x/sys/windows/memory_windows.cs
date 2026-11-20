// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

partial class windows_package {

public static UntypedInt MEM_COMMIT => 0x00001000;
public static UntypedInt MEM_RESERVE => 0x00002000;
public static UntypedInt MEM_DECOMMIT => 0x00004000;
public static UntypedInt MEM_RELEASE => 0x00008000;
public static UntypedInt MEM_RESET => 0x00080000;
public static UntypedInt MEM_TOP_DOWN => 0x00100000;
public static UntypedInt MEM_WRITE_WATCH => 0x00200000;
public static UntypedInt MEM_PHYSICAL => 0x00400000;
public static UntypedInt MEM_RESET_UNDO => 0x01000000;
public static UntypedInt MEM_LARGE_PAGES => 0x20000000;
public static UntypedInt PAGE_NOACCESS => 0x00000001;
public static UntypedInt PAGE_READONLY => 0x00000002;
public static UntypedInt PAGE_READWRITE => 0x00000004;
public static UntypedInt PAGE_WRITECOPY => 0x00000008;
public static UntypedInt PAGE_EXECUTE => 0x00000010;
public static UntypedInt PAGE_EXECUTE_READ => 0x00000020;
public static UntypedInt PAGE_EXECUTE_READWRITE => 0x00000040;
public static UntypedInt PAGE_EXECUTE_WRITECOPY => 0x00000080;
public static UntypedInt PAGE_GUARD => 0x00000100;
public static UntypedInt PAGE_NOCACHE => 0x00000200;
public static UntypedInt PAGE_WRITECOMBINE => 0x00000400;
public static UntypedInt PAGE_TARGETS_INVALID => 0x40000000;
public static UntypedInt PAGE_TARGETS_NO_UPDATE => 0x40000000;
public static UntypedInt QUOTA_LIMITS_HARDWS_MIN_DISABLE => 0x00000002;
public static UntypedInt QUOTA_LIMITS_HARDWS_MIN_ENABLE => 0x00000001;
public static UntypedInt QUOTA_LIMITS_HARDWS_MAX_DISABLE => 0x00000008;
public static UntypedInt QUOTA_LIMITS_HARDWS_MAX_ENABLE => 0x00000004;

partial struct MemoryBasicInformation {
    public uintptr BaseAddress;
    public uintptr AllocationBase;
    public uint32 AllocationProtect;
    public uint16 PartitionId;
    public uintptr RegionSize;
    public uint32 State;
    public uint32 Protect;
    public uint32 Type;
}

} // end windows_package
