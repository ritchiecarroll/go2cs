// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

partial class windows_package {

partial struct MemoryBasicInformation {
    // A pointer to the base address of the region of pages.
    public uintptr BaseAddress;
    // A pointer to the base address of a range of pages allocated by the VirtualAlloc function.
    // The page pointed to by the BaseAddress member is contained within this allocation range.
    public uintptr AllocationBase;
    // The memory protection option when the region was initially allocated
    public uint32 AllocationProtect;
    public uint16 PartitionId;
    // The size of the region beginning at the base address in which all pages have identical attributes, in bytes.
    public uintptr RegionSize;
    // The state of the pages in the region.
    public uint32 State;
    // The access protection of the pages in the region.
    public uint32 Protect;
    // The type of pages in the region.
    public uint32 Type;
}

} // end windows_package
