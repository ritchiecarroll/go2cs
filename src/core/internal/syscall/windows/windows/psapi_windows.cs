// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

partial class windows_package {

partial struct PROCESS_MEMORY_COUNTERS {
    public uint32 CB;
    public uint32 PageFaultCount;
    public uintptr PeakWorkingSetSize;
    public uintptr WorkingSetSize;
    public uintptr QuotaPeakPagedPoolUsage;
    public uintptr QuotaPagedPoolUsage;
    public uintptr QuotaPeakNonPagedPoolUsage;
    public uintptr QuotaNonPagedPoolUsage;
    public uintptr PagefileUsage;
    public uintptr PeakPagefileUsage;
}

//sys	GetProcessMemoryInfo(handle syscall.Handle, memCounters *PROCESS_MEMORY_COUNTERS, cb uint32) (err error) = psapi.GetProcessMemoryInfo

} // end windows_package
