// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using bits = math.bits_package;
using @unsafe = unsafe_package;
using math;
using syscall = syscall_package;
using static unix_package;

partial class unix_package {

internal static UntypedInt cpuSetSize => /* _CPU_SETSIZE / _NCPUBITS */ 16;

/* [cpuSetSize]cpuMask */
partial struct CPUSet /*[16]cpuMask*/;

partial struct CPUSetDynamic /*[]cpuMask*/;

internal static error schedAffinity(uintptr trap, nint pid, uintptr size, @unsafe.Pointer ptr) {
    var (_, _, e) = RawSyscall(trap, (uintptr)pid, (uintptr)size, (uintptr)ptr);
    if (e != 0) {
        return errnoErr(e);
    }
    return default;
}

public static error SchedGetaffinity(nint pid, ж<CPUSet> set) {
    return schedAffinity(SYS_SCHED_GETAFFINITY, pid, /* unsafe.Sizeof(*set) */ (uintptr)128, @unsafe.Pointer.FromPinnedBox(set));
}

public static error SchedSetaffinity(nint pid, ж<CPUSet> set) {
    return schedAffinity(SYS_SCHED_SETAFFINITY, pid, /* unsafe.Sizeof(*set) */ (uintptr)128, @unsafe.Pointer.FromPinnedBox(set));
}

} partial class unixᴛCPUSet {

public static void Zero(this ref CPUSet s) {
    clear(s.Value[..]);
}

public static void Fill(this ref CPUSet s) {
    cpuMaskFill(s.Value[..]);
}

} partial class unix_package {

internal static nint cpuBitsIndex(nint cpu) {
    return cpu / (nint)_NCPUBITS;
}

internal static cpuMask cpuBitsMask(nint cpu) {
    return (cpuMask)((uint64)1 << (int)((nuint)cpu % (nuint)_NCPUBITS));
}

internal static void cpuMaskFill(slice<cpuMask> s) {
    foreach (var i in range(len(s))) {
        s[i] = ~((cpuMask)((cpuMask)0));
    }
}

internal static void cpuMaskSet(slice<cpuMask> s, nint cpu) {
    nint i = cpuBitsIndex(cpu);
    if (i < len(s)) {
        s[i] |= (cpuMask)(cpuBitsMask(cpu));
    }
}

internal static void cpuMaskClear(slice<cpuMask> s, nint cpu) {
    nint i = cpuBitsIndex(cpu);
    if (i < len(s)) {
        s[i] &= unchecked((cpuMask)~(cpuMask)(cpuBitsMask(cpu)));
    }
}

internal static bool cpuMaskIsSet(slice<cpuMask> s, nint cpu) {
    nint i = cpuBitsIndex(cpu);
    if (i < len(s)) {
        return (cpuMask)(s[i] & cpuBitsMask(cpu)) != 0;
    }
    return false;
}

internal static nint cpuMaskCount(slice<cpuMask> s) {
    nint c = 0;
    foreach (var (_, b) in range(s)) {
        c += bits.OnesCount64((uint64)b);
    }
    return c;
}

} partial class unixᴛCPUSet {

public static void Set(this ref CPUSet s, nint cpu) {
    cpuMaskSet(s.Value[..], cpu);
}

public static void Clear(this ref CPUSet s, nint cpu) {
    cpuMaskClear(s.Value[..], cpu);
}

public static bool IsSet(this ref CPUSet s, nint cpu) {
    return cpuMaskIsSet(s.Value[..], cpu);
}

public static nint Count(this ref CPUSet s) {
    return cpuMaskCount(s.Value[..]);
}

} partial class unix_package {

public static CPUSetDynamic NewCPUSet(nint maxCPU) {
    nint numMasks = (maxCPU + (nint)_NCPUBITS - 1) / (nint)_NCPUBITS;
    if (numMasks == 0) {
        numMasks = 1;
    }
    return new CPUSetDynamic(numMasks);
}

} partial class unixᴛCPUSetDynamic {

public static void Zero(this CPUSetDynamic s) {
    clear(s);
}

public static void Fill(this CPUSetDynamic s) {
    cpuMaskFill(s);
}

public static void Set(this CPUSetDynamic s, nint cpu) {
    cpuMaskSet(s, cpu);
}

public static void Clear(this CPUSetDynamic s, nint cpu) {
    cpuMaskClear(s, cpu);
}

public static bool IsSet(this CPUSetDynamic s, nint cpu) {
    return cpuMaskIsSet(s, cpu);
}

public static nint Count(this CPUSetDynamic s) {
    return cpuMaskCount(s);
}

internal static uintptr size(this CPUSetDynamic s) {
    return (uintptr)len(s) * /* unsafe.Sizeof(cpuMask(0)) */ (uintptr)8;
}

internal static @unsafe.Pointer pointer(this CPUSetDynamic s) {
    if (len(s) == 0) {
        return default;
    }
    return @unsafe.Pointer.FromPinnedBox(Ꮡ(s, 0));
}

} partial class unix_package {

public static error SchedGetaffinityDynamic(nint pid, CPUSetDynamic set) {
    return schedAffinity(SYS_SCHED_GETAFFINITY, pid, set.size(), set.pointer());
}

public static error SchedSetaffinityDynamic(nint pid, CPUSetDynamic set) {
    return schedAffinity(SYS_SCHED_SETAFFINITY, pid, set.size(), set.pointer());
}

} // end unix_package
