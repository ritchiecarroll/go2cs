// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using os = os_package;
using @unsafe = unsafe_package;

partial class pprof_package {

internal static bool isExecutable(int32 protection) {
    return ((int32)(protection & (int32)_VM_PROT_EXECUTE)) != 0 && ((int32)(protection & (int32)_VM_PROT_READ)) != 0;
}

// machVMInfo uses the mach_vm_region region system call to add mapping entries
// for the text region of the running process.
internal static bool machVMInfo(Action<uint64, uint64, uint64, @string, @string> addMapping) {
    var added = false;
    ref var addr = ref heap(new uint64(), out var Ꮡaddr);
    addr = 0x1;
    while (ᐧ) {
        ref var memRegionSize = ref heap(new uint64(), out var ᏑmemRegionSize);
        ref var info = ref heap(new machVMRegionBasicInfoData(), out var Ꮡinfo);
        // Get the first address and page size.
        var kr = mach_vm_region(
            Ꮡaddr,
            ᏑmemRegionSize,
            @unsafe.Pointer.FromPinnedBox(Ꮡinfo));
        if (kr != 0) {
            if (kr == _MACH_SEND_INVALID_DEST) {
                // No more memory regions.
                return true;
            }
            return added; // return true if at least one mapping was added
        }
        if (isExecutable(info.Protection)) {
            // NOTE: the meaning/value of Offset is unclear. However,
            // this likely doesn't matter as the text segment's file
            // offset is usually 0.
            addMapping(addr,
                addr + memRegionSize,
                read64(ref info.Offset),
                regionFilename(addr),
                ""u8);
            added = true;
        }
        addr += memRegionSize;
    }
}

internal static uint64 read64(ref array<byte> p) {
    // all supported darwin platforms are little endian
    return (uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)p[0] | ((uint64)p[1] << (int)(8))) | ((uint64)p[2] << (int)(16))) | ((uint64)p[3] << (int)(24))) | ((uint64)p[4] << (int)(32))) | ((uint64)p[5] << (int)(40))) | ((uint64)p[6] << (int)(48))) | ((uint64)p[7] << (int)(56)));
}

internal static @string regionFilename(uint64 address) {
    var buf = new slice<byte>(_MAXPATHLEN);
    var r = proc_regionfilename(
        os.Getpid(),
        address,
        @unsafe.SliceData(buf),
        (int64)cap(buf));
    if (r == 0) {
        return ""u8;
    }
    return ((@string)(buf.slice(0, r)));
}

// mach_vm_region and proc_regionfilename are implemented by
// the runtime package (runtime/sys_darwin.go).
//
//go:noescape
internal static partial int32 mach_vm_region(ж<uint64> address, ж<uint64> region_size, @unsafe.Pointer info);

//go:noescape
internal static partial int32 proc_regionfilename(nint pid, uint64 address, ж<byte> buf, int64 buflen);

} // end pprof_package
