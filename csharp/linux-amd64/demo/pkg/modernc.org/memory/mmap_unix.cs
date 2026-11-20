// Copyright 2011 Evan Shaw. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE-MMAP-GO file.

namespace go.modernc.org;

using unix = golang.org.x.sys.unix_package;
using os = os_package;
using @unsafe = unsafe_package;
using golang.org.x.sys;

partial class memory_package {

internal static UntypedInt pageSizeLog => 16;

internal const bool canCarve = true;

internal static nint osPageMask;
internal static void initᴛosPageMask() { osPageMask = osPageSize - 1; }
internal static nint osPageSize = os.Getpagesize();
internal static nint mmapGranularity = osPageSize;

internal static error unmap(uintptr addr, nint size) {
    return unix.MunmapPtr((@unsafe.Pointer)addr, (uintptr)size);
}

internal static (uintptr, nint, error) mmap(nint size) {
    size = roundup(size, osPageSize);
    nint n = size + (nint)pageSize;
    var (up, err) = unix.MmapPtr(-1, 0, nil, (uintptr)n, (nint)((nint)unix.PROT_READ | (nint)unix.PROT_WRITE), (nint)((nint)unix.MAP_PRIVATE | (nint)unix.MAP_ANON));
    if (err != nil) {
        return (0, 0, err);
    }

    var p = (uintptr)up;
    if ((uintptr)(p & (uintptr)osPageMask) != 0) {
        throw panic("internal error");
    }

    nint mod = (nint)p & (nint)pageMask;
    if (mod != 0) {
        nint m = (nint)pageSize - mod;
        var errΔ1 = unmap(p, m); if (errΔ1 != nil) {
            unmap(p, n);
            return (0, 0, errΔ1);
        }

        n -= m;
        p += (uintptr)m;
    }

    if ((uintptr)(p & (uintptr)(uintptr)pageMask) != 0) {
        throw panic("internal error");
    }

    if (n > size) {
        var errΔ2 = unmap(p + (uintptr)size, n - size); if (errΔ2 != nil) {
            size = n;
        }
    }

    return (p, size, default);
}

} // end memory_package
