// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// go2cs HAND-OWNED (Go's probe added; the auto conversion is kept beside it as set_vma_name_linux.cs.auto).
// RULED by COORD 2026-11-09 (runtime TestDecorateMappings, linux).
//
// WHY. Go learns whether the kernel can name anonymous mappings by trying: the first setVMAName, made
// during mallocinit, calls prctl(PR_SET_VMA) and stores prSetVMAUnsupported on EINVAL. The managed
// host never runs mallocinit, so a run that reaches none of the seven setVMAName sites (mem_linux.cs,
// vgetrandom_linux.cs) never asks, and setVMANameSupported read true on every kernel. On a kernel
// without PR_SET_VMA (the WSL class), runtime's TestDecorateMappings then took the supported branch,
// where Go skips "mapping labels absent as expected".
//
// WHAT CHANGED. setVMANameSupported makes Go's probe itself, once, when no setVMAName call has stored the
// answer yet: one prctl(PR_SET_VMA, PR_SET_VMA_ANON_NAME) over a page it maps and unmaps, with a NULL
// name, so the probe leaves no label behind. EINVAL stores unsupported, exactly as setVMAName's own
// store does. Like Go, it asks only while decoratemappings is enabled. setVMAName is Go's body, unchanged:
// on a supporting kernel the converted runtime still names the native mappings sysMmap makes.
//
// The probe runs under a three-state flag (0 not run, 1 running, 2 done) set by Interlocked, not by a
// static initializer, so it does not depend on the order in which the package's partial files
// initialize their statics.
[module: go.GoManualConversion]

namespace go;

using System.Threading;
using atomic = @internal.runtime.atomic_package;
using linux = @internal.runtime.syscall.linux_package;
using @unsafe = unsafe_package;
using @internal.runtime;
using @internal.runtime.syscall;

partial class runtime_package {

internal static readonly ж<atomic.Bool> ᏑprSetVMAUnsupported = new StaticBox<atomic.Bool>(default(atomic.Bool));
internal static ref atomic.Bool prSetVMAUnsupported => ref ((StaticBox<atomic.Bool>)ᏑprSetVMAUnsupported).ValueSlot;

// vmaNameProbe is 0 before the probe, 1 while one thread runs it, and 2 once it has run.
private static int vmaNameProbe;

internal static bool setVMANameSupported() {
    if (Volatile.Read(ref vmaNameProbe) != 2) {
        probeVMAName();
    }
    return !ᏑprSetVMAUnsupported.Load();
}

// probeVMAName is the prctl Go's first setVMAName makes, run once. A thread that finds another one
// probing waits for its answer rather than reading the flag before it is stored.
private static void probeVMAName() {
    if (Interlocked.CompareExchange(ref vmaNameProbe, 1, 0) != 0) {
        var spin = new SpinWait();
        while (Volatile.Read(ref vmaNameProbe) != 2) {
            spin.SpinOnce();
        }
        return;
    }

    try {
        if (debug.decoratemappings == 0 || ᏑprSetVMAUnsupported.Load()) {
            return;
        }

        uintptr n = (uintptr)4096;
        var (p, err) = mmap(nil, n, (int32)((int32)_PROT_READ | (int32)_PROT_WRITE), (int32)((int32)_MAP_ANON | (int32)_MAP_PRIVATE), -1, 0);
        if (err != 0) {
            return;
        }

        var (_, _, errno) = linux.Syscall6(linux.SYS_PRCTL, linux.PR_SET_VMA, linux.PR_SET_VMA_ANON_NAME, (uintptr)p, n, 0, 0);
        munmap(p, n);
        if (errno == _EINVAL) {
            ᏑprSetVMAUnsupported.Store(true);
        }
    }
    finally {
        Volatile.Write(ref vmaNameProbe, 2);
    }
}

// setVMAName calls prctl(PR_SET_VMA, PR_SET_VMA_ANON_NAME, start, len, name)
internal static void setVMAName(@unsafe.Pointer start, uintptr length, @string name) {
    if (debug.decoratemappings == 0 || !setVMANameSupported()) {
        return;
    }

    ref var sysName = ref heap(new array<byte>(80), out var ᏑsysName);
    nint n = copy(sysName[..], " Go: "u8);
    copy(sysName.slice(n, 79), name); // leave final byte zero

    var ᴋ0 = start;
    var ᴋ1 = ᏑsysName.at<byte>(0);
        var (_, _, err) = linux.Syscall6(linux.SYS_PRCTL, linux.PR_SET_VMA, linux.PR_SET_VMA_ANON_NAME, (uintptr)ᴋ0, length, (uintptr)ᴋ1, 0);
    System.GC.KeepAlive(ᴋ0);
    System.GC.KeepAlive(ᴋ1);
    if (err == _EINVAL) {
        ᏑprSetVMAUnsupported.Store(true);
    }
    // ignore other errors
}

} // end runtime_package
