// structclass_linux_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The syscall STRUCT-PASSING class, closed PROACTIVELY for the five remaining Linux members whose
// converted struct carries `array<>` fields. zsyscall_linux_amd64_impl.cs carries the class's full
// write-up and its first members (Fstat/fstatat, wait4, Uname); this file is that same remedy
// applied to the rest of the surface, and it exists because the class's DEFERRAL RULE was found to
// be unsafe for these members rather than because a suite reached them.
//
// WHY THE RULE CHANGED (the 2026-08-28 dump). The class had been taken "per-member, WHEN REACHED",
// on the strength of the symptom Stat_t demonstrated: a quiet wrong ANSWER. That understates what
// the write does. Every one of these structs holds its `array<>` fields as MANAGED REFERENCES in a
// GC-tracked slot, and the kernel writes its data straight over them — so the collector's next scan
// reads those bytes as object pointers and follows them. The failure is not a bad value in the
// caller's struct; it is heap corruption whose crash surfaces arbitrarily far away, inside the GC.
// Measured, not theorized: verifyheap on a crashed os/exec host reported 6 errors in one contiguous
// ~0x180-byte run — a zeroed method table, three members that are text where pointers belong, a
// syncblock index of 21,840,206 — and the smashed run held an `array<System.SByte>` enumerator
// (the converted Utsname is six `array<int8>`) while the object referencing INTO it was
// ManagedPointerTokens.s_table's own node array. Uname was on the deferred list, with the reason
// "no roster row reached them", while a roster row was reaching it.
//
// So "no roster row reached it" is a statement about our COVERAGE, not about execution, and for a
// corruption sub-class the cost of being wrong is not a wrong answer in one test but an
// unattributable crash in another. These five are therefore closed on the mechanism, before any
// suite is observed to reach them. Ordered here by how loudly they would bite: the two WRITE-BACK
// members any converted program can reach (Select, FcntlFlock), then the three the class had
// already catalogued or overlooked (Statfs_t, Sysinfo_t, Timex).
//
// THE REMEDY, identical to the file next door in every case: a blittable
// [StructLayout(LayoutKind.Sequential)] mirror of the NATIVE layout with `fixed` buffers where Go
// has inline arrays, a size check at the boundary (so a wrong mirror fails the CALL loudly instead
// of taking the package's static constructor down), the syscall handed the MIRROR's address, and an
// explicit field-for-field copy at the boundary.
//
// TWO-WAY MEMBERS. Unlike Fstat, four of these six wrappers are read-modify-write: the caller fills
// the struct in, the kernel reads it AND writes it back (Select's three fd_sets and its timeout,
// FcntlFlock's F_GETLK, Adjtimex's modes-driven set-and-report). Those seed the mirror from the
// caller's struct BEFORE the call and copy back UNCONDITIONALLY after it — unconditional because a
// mirror seeded from the caller is an identity copy when the kernel wrote nothing, which keeps the
// EINTR case honest (select(2) updates the timeout but not the sets) without needing to model which
// errnos write what. The one-way members (Statfs, Fstatfs, Sysinfo) copy back only on success, so a
// failed call leaves the caller's struct untouched exactly as Go's does.
//
// LIFETIME, the second reason two of these need the mirror even where layout might have survived:
// Select and FcntlFlock's F_SETLKW BLOCK. The mirror is a stack local for the whole call, which is
// the same property wait4 needed when GC compaction relocated its boxes mid-wait.
//
// SCOPE. linux/amd64's layouts (ztypes_linux_amd64.go), registered goosLinux in
// manualConversionFuncs. Another Linux arch would need its own mirrors; darwin declares Select,
// Statfs and Fstatfs too, with layouts of its own and bodies that are not defective here, which is
// exactly what the goosLinux scoping protects.

using System;
using System.Runtime.InteropServices;

// Hand-owned (no structclass_linux_impl.go exists, so a reconvert never regenerates this file).
[module: go.GoManualConversion]

// The mirrors need `fixed` buffers and the wrappers take addresses of stack locals.
[module: go.GoRequiresUnsafe]

namespace go;

partial class syscall_package
{
    // ── Select: fd_set, and the class's clearest WRITE-BACK ──────────────────────────────────
    //
    // `struct fd_set` is 1024 bits — sixteen 64-bit words INLINE, 128 bytes. The converted FdSet
    // holds all of it as ONE `array<int64>` reference, so the kernel's 128-byte readiness write
    // lands over that reference and 120 bytes beyond it. select(2) is also the class's second
    // BLOCKING member, so the mirror's stack lifetime matters as much as its layout.

    private const int fdSetBitsLength = 16;
    private const int nativeFdSetSizeLinuxAmd64 = 128;

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeFdSetLinuxAmd64
    {
        public fixed int64 Bits[fdSetBitsLength];
    }

    // `struct timeval` (two int64 words) is NativeTimevalLinuxAmd64, already declared beside the
    // wait4 mirror in zsyscall_linux_amd64_impl.cs — one partial class, so it is reused here
    // rather than restated. Select's timeout therefore crosses in a stack buffer too, which is
    // what the blocking shape needs.

    public static unsafe (nint n, error err) Select(nint nfd, ж<FdSet> Ꮡr, ж<FdSet> Ꮡw, ж<FdSet> Ꮡe, ж<Timeval> Ꮡtimeout) {
        if (sizeof(NativeFdSetLinuxAmd64) != nativeFdSetSizeLinuxAmd64)
            throw new InvalidOperationException($"syscall: NativeFdSetLinuxAmd64 is {sizeof(NativeFdSetLinuxAmd64)} bytes, the kernel's fd_set is {nativeFdSetSizeLinuxAmd64}");

        NativeFdSetLinuxAmd64 nativeR = default, nativeW = default, nativeE = default;
        NativeTimevalLinuxAmd64 nativeTimeout = default;

        seedNativeFdSet(Ꮡr, ref nativeR);
        seedNativeFdSet(Ꮡw, ref nativeW);
        seedNativeFdSet(Ꮡe, ref nativeE);

        if (Ꮡtimeout is not null && !Ꮡtimeout.IsNilPointer) {
            ref var tv = ref Ꮡtimeout.Value;
            nativeTimeout.Sec = tv.Sec;
            nativeTimeout.Usec = tv.Usec;
        }

        // A nil set reaches the kernel as address 0 in Go — "do not watch this class" — and a nil
        // timeout means block forever. Both must stay nil rather than becoming a zeroed buffer.
        uintptr rAddr = Ꮡr is null || Ꮡr.IsNilPointer ? (uintptr)0 : (uintptr)(nint)(&nativeR);
        uintptr wAddr = Ꮡw is null || Ꮡw.IsNilPointer ? (uintptr)0 : (uintptr)(nint)(&nativeW);
        uintptr eAddr = Ꮡe is null || Ꮡe.IsNilPointer ? (uintptr)0 : (uintptr)(nint)(&nativeE);
        uintptr timeoutAddr = Ꮡtimeout is null || Ꮡtimeout.IsNilPointer ? (uintptr)0 : (uintptr)(nint)(&nativeTimeout);

        var (r0, _, e1) = Syscall6(SYS_SELECT, (uintptr)nfd, rAddr, wAddr, eAddr, timeoutAddr, 0);
        nint n = (nint)r0;

        // Unconditional, per the file header: the mirrors were seeded from the caller, so this is
        // an identity copy wherever the kernel wrote nothing, and it keeps EINTR's timeout update
        // (which the kernel makes without touching the sets) faithful.
        copyNativeFdSet(ref nativeR, Ꮡr);
        copyNativeFdSet(ref nativeW, Ꮡw);
        copyNativeFdSet(ref nativeE, Ꮡe);

        if (Ꮡtimeout is not null && !Ꮡtimeout.IsNilPointer) {
            ref var tv = ref Ꮡtimeout.Value;
            tv.Sec = nativeTimeout.Sec;
            tv.Usec = nativeTimeout.Usec;
        }

        error err = default!;
        if (e1 != 0) {
            err = errnoErr(e1);
        }
        return (n, err);
    }

    private static unsafe void seedNativeFdSet(ж<FdSet> Ꮡset, ref NativeFdSetLinuxAmd64 native) {
        if (Ꮡset is null || Ꮡset.IsNilPointer)
            return;

        ref var fds = ref Ꮡset.Value;
        // A default FdSet carries a zero-length array; treat it as all-zero rather than faulting.
        nint length = fds.Bits.Length;

        for (int i = 0; i < fdSetBitsLength; i++)
            native.Bits[i] = i < length ? fds.Bits[i] : 0;
    }

    private static unsafe void copyNativeFdSet(ref NativeFdSetLinuxAmd64 native, ж<FdSet> Ꮡset) {
        if (Ꮡset is null || Ꮡset.IsNilPointer)
            return;

        ref var fds = ref Ꮡset.Value;

        if (fds.Bits.Length != fdSetBitsLength)
            fds.Bits = new array<int64>(fdSetBitsLength);

        for (int i = 0; i < fdSetBitsLength; i++)
            fds.Bits[i] = native.Bits[i];
    }

    // ── FcntlFlock: the other WRITE-BACK, and it is a lock protocol ───────────────────────────
    //
    // `struct flock` is 32 bytes with two 4-byte holes; the converted Flock_t holds both holes as
    // `array<byte>` references. F_SETLK/F_SETLKW READ the struct (a wrong image asks for the wrong
    // lock), F_GETLK WRITES it back (the answer lands over the references), and F_SETLKW BLOCKS
    // until the lock is available — so this member is all three hazards at once.

    private const int nativeFlockSizeLinuxAmd64 = 32;

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeFlockLinuxAmd64
    {
        public int16 Type;
        public int16 Whence;
        public fixed byte Pad_cgo_0[4];
        public int64 Start;
        public int64 Len;
        public int32 Pid;
        public fixed byte Pad_cgo_1[4];
    }

    public static unsafe error FcntlFlock(uintptr fd, nint cmd, ж<Flock_t> Ꮡlk) {
        if (sizeof(NativeFlockLinuxAmd64) != nativeFlockSizeLinuxAmd64)
            throw new InvalidOperationException($"syscall: NativeFlockLinuxAmd64 is {sizeof(NativeFlockLinuxAmd64)} bytes, the kernel's struct flock is {nativeFlockSizeLinuxAmd64}");

        NativeFlockLinuxAmd64 native = default;

        if (Ꮡlk is not null && !Ꮡlk.IsNilPointer) {
            ref var lk = ref Ꮡlk.Value;
            native.Type = lk.Type;
            native.Whence = lk.Whence;
            native.Start = lk.Start;
            native.Len = lk.Len;
            native.Pid = lk.Pid;
            copyPadIn(lk.Pad_cgo_0, native.Pad_cgo_0, 4);
            copyPadIn(lk.Pad_cgo_1, native.Pad_cgo_1, 4);
        }

        uintptr lkAddr = Ꮡlk is null || Ꮡlk.IsNilPointer ? (uintptr)0 : (uintptr)(nint)(&native);

        var (_, _, errno) = Syscall(fcntl64Syscall, fd, (uintptr)cmd, lkAddr);

        if (Ꮡlk is not null && !Ꮡlk.IsNilPointer) {
            ref var lk = ref Ꮡlk.Value;
            lk.Type = native.Type;
            lk.Whence = native.Whence;
            lk.Start = native.Start;
            lk.Len = native.Len;
            lk.Pid = native.Pid;
            copyPadOut(native.Pad_cgo_0, ref lk.Pad_cgo_0, 4);
            copyPadOut(native.Pad_cgo_1, ref lk.Pad_cgo_1, 4);
        }

        if (errno == 0) {
            return default!;
        }
        return errno;
    }

    // ── Statfs / Fstatfs ─────────────────────────────────────────────────────────────────────
    //
    // `struct statfs` is 120 bytes; the converted Statfs_t holds `Spare [4]int64` as a reference
    // AND nests a Fsid whose `X__val [2]int32` is another one, so this member has a reference in
    // the middle of the record as well as at the end.

    private const int nativeStatfsSizeLinuxAmd64 = 120;
    private const int fsidValLength = 2;
    private const int statfsSpareLength = 4;

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeFsidLinuxAmd64
    {
        public fixed int32 X__val[fsidValLength];
    }

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeStatfsLinuxAmd64
    {
        public int64 Type;
        public int64 Bsize;
        public uint64 Blocks;
        public uint64 Bfree;
        public uint64 Bavail;
        public uint64 Files;
        public uint64 Ffree;
        public NativeFsidLinuxAmd64 Fsid;
        public int64 Namelen;
        public int64 Frsize;
        public int64 Flags;
        public fixed int64 Spare[statfsSpareLength];
    }

    public static unsafe error /*err*/ Statfs(@string path, ж<Statfs_t> Ꮡbuf) {
        error err = default!;

        // The generated wrapper's own path handling, verbatim — only the struct argument changes.
        ж<byte> _p0 = default!;
        (_p0, err) = BytePtrFromString(path);
        if (err != default!) {
            return err;
        }

        if (sizeof(NativeStatfsLinuxAmd64) != nativeStatfsSizeLinuxAmd64)
            throw new InvalidOperationException($"syscall: NativeStatfsLinuxAmd64 is {sizeof(NativeStatfsLinuxAmd64)} bytes, the kernel's struct statfs is {nativeStatfsSizeLinuxAmd64}");

        NativeStatfsLinuxAmd64 native = default;
        uintptr bufAddr = Ꮡbuf is null || Ꮡbuf.IsNilPointer ? (uintptr)0 : (uintptr)(nint)(&native);

        var (_, _, e1) = Syscall(SYS_STATFS, (uintptr)_p0, bufAddr, 0);
        if (e1 != 0) {
            return errnoErr(e1);
        }

        copyNativeStatfs(ref native, Ꮡbuf!);
        return default!;
    }

    public static unsafe error /*err*/ Fstatfs(nint fd, ж<Statfs_t> Ꮡbuf) {
        if (sizeof(NativeStatfsLinuxAmd64) != nativeStatfsSizeLinuxAmd64)
            throw new InvalidOperationException($"syscall: NativeStatfsLinuxAmd64 is {sizeof(NativeStatfsLinuxAmd64)} bytes, the kernel's struct statfs is {nativeStatfsSizeLinuxAmd64}");

        NativeStatfsLinuxAmd64 native = default;
        uintptr bufAddr = Ꮡbuf is null || Ꮡbuf.IsNilPointer ? (uintptr)0 : (uintptr)(nint)(&native);

        var (_, _, e1) = Syscall(SYS_FSTATFS, (uintptr)fd, bufAddr, 0);
        if (e1 != 0) {
            return errnoErr(e1);
        }

        copyNativeStatfs(ref native, Ꮡbuf!);
        return default!;
    }

    private static unsafe void copyNativeStatfs(ref NativeStatfsLinuxAmd64 native, ж<Statfs_t> Ꮡbuf) {
        ref var sf = ref Ꮡbuf.Value;

        sf.Type = native.Type;
        sf.Bsize = native.Bsize;
        sf.Blocks = native.Blocks;
        sf.Bfree = native.Bfree;
        sf.Bavail = native.Bavail;
        sf.Files = native.Files;
        sf.Ffree = native.Ffree;
        sf.Namelen = native.Namelen;
        sf.Frsize = native.Frsize;
        sf.Flags = native.Flags;

        // The nested Fsid is a struct with its own array field — rebuilt, not aliased.
        ref var fsid = ref sf.Fsid;
        if (fsid.X__val.Length != fsidValLength)
            fsid.X__val = new array<int32>(fsidValLength);

        for (int i = 0; i < fsidValLength; i++)
            fsid.X__val[i] = native.Fsid.X__val[i];

        if (sf.Spare.Length != statfsSpareLength)
            sf.Spare = new array<int64>(statfsSpareLength);

        for (int i = 0; i < statfsSpareLength; i++)
            sf.Spare[i] = native.Spare[i];
    }

    // ── Sysinfo ──────────────────────────────────────────────────────────────────────────────
    //
    // `struct sysinfo` is 112 bytes with `loads[3]` INLINE near the front; the converted Sysinfo_t
    // holds that plus three padding arrays as references — four of them, the first only 8 bytes
    // into the record, so almost the whole kernel write lands on managed pointers. `X_f` is Go's
    // zero-length tail padding and has no native storage, so the mirror does not carry it.

    private const int nativeSysinfoSizeLinuxAmd64 = 112;
    private const int sysinfoLoadsLength = 3;

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeSysinfoLinuxAmd64
    {
        public int64 Uptime;
        public fixed uint64 Loads[sysinfoLoadsLength];
        public uint64 Totalram;
        public uint64 Freeram;
        public uint64 Sharedram;
        public uint64 Bufferram;
        public uint64 Totalswap;
        public uint64 Freeswap;
        public uint16 Procs;
        public uint16 Pad;
        public fixed byte Pad_cgo_0[4];
        public uint64 Totalhigh;
        public uint64 Freehigh;
        public uint32 Unit;
        public fixed byte Pad_cgo_1[4];
    }

    public static unsafe error /*err*/ Sysinfo(ж<Sysinfo_t> Ꮡinfo) {
        if (sizeof(NativeSysinfoLinuxAmd64) != nativeSysinfoSizeLinuxAmd64)
            throw new InvalidOperationException($"syscall: NativeSysinfoLinuxAmd64 is {sizeof(NativeSysinfoLinuxAmd64)} bytes, the kernel's struct sysinfo is {nativeSysinfoSizeLinuxAmd64}");

        NativeSysinfoLinuxAmd64 native = default;
        uintptr infoAddr = Ꮡinfo is null || Ꮡinfo.IsNilPointer ? (uintptr)0 : (uintptr)(nint)(&native);

        var (_, _, e1) = RawSyscall(SYS_SYSINFO, infoAddr, 0, 0);
        if (e1 != 0) {
            return errnoErr(e1);
        }

        ref var si = ref Ꮡinfo!.Value;
        si.Uptime = native.Uptime;
        si.Totalram = native.Totalram;
        si.Freeram = native.Freeram;
        si.Sharedram = native.Sharedram;
        si.Bufferram = native.Bufferram;
        si.Totalswap = native.Totalswap;
        si.Freeswap = native.Freeswap;
        si.Procs = native.Procs;
        si.Pad = native.Pad;
        si.Totalhigh = native.Totalhigh;
        si.Freehigh = native.Freehigh;
        si.Unit = native.Unit;

        if (si.Loads.Length != sysinfoLoadsLength)
            si.Loads = new array<uint64>(sysinfoLoadsLength);

        for (int i = 0; i < sysinfoLoadsLength; i++)
            si.Loads[i] = native.Loads[i];

        copyPadOut(native.Pad_cgo_0, ref si.Pad_cgo_0, 4);
        copyPadOut(native.Pad_cgo_1, ref si.Pad_cgo_1, 4);
        return default!;
    }

    // ── Adjtimex ─────────────────────────────────────────────────────────────────────────────
    //
    // `struct timex` is 208 bytes, the largest member of this batch, and its four padding holes
    // (4, 4, 4 and a 44-byte tail) are all `array<byte>` references in the converted Timex. It is
    // read-modify-write by design: `Modes` tells the kernel what to set, and the kernel fills in
    // every other field on return.

    private const int nativeTimexSizeLinuxAmd64 = 208;

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeTimexLinuxAmd64
    {
        public uint32 Modes;
        public fixed byte Pad_cgo_0[4];
        public int64 Offset;
        public int64 Freq;
        public int64 Maxerror;
        public int64 Esterror;
        public int32 Status;
        public fixed byte Pad_cgo_1[4];
        public int64 Constant;
        public int64 Precision;
        public int64 Tolerance;
        public NativeTimevalLinuxAmd64 Time;
        public int64 Tick;
        public int64 Ppsfreq;
        public int64 Jitter;
        public int32 Shift;
        public fixed byte Pad_cgo_2[4];
        public int64 Stabil;
        public int64 Jitcnt;
        public int64 Calcnt;
        public int64 Errcnt;
        public int64 Stbcnt;
        public int32 Tai;
        public fixed byte Pad_cgo_3[44];
    }

    public static unsafe (nint state, error err) Adjtimex(ж<Timex> Ꮡbuf) {
        if (sizeof(NativeTimexLinuxAmd64) != nativeTimexSizeLinuxAmd64)
            throw new InvalidOperationException($"syscall: NativeTimexLinuxAmd64 is {sizeof(NativeTimexLinuxAmd64)} bytes, the kernel's struct timex is {nativeTimexSizeLinuxAmd64}");

        NativeTimexLinuxAmd64 native = default;

        if (Ꮡbuf is not null && !Ꮡbuf.IsNilPointer) {
            ref var tx = ref Ꮡbuf.Value;
            native.Modes = tx.Modes;
            native.Offset = tx.Offset;
            native.Freq = tx.Freq;
            native.Maxerror = tx.Maxerror;
            native.Esterror = tx.Esterror;
            native.Status = tx.Status;
            native.Constant = tx.Constant;
            native.Precision = tx.Precision;
            native.Tolerance = tx.Tolerance;
            native.Time.Sec = tx.Time.Sec;
            native.Time.Usec = tx.Time.Usec;
            native.Tick = tx.Tick;
            native.Ppsfreq = tx.Ppsfreq;
            native.Jitter = tx.Jitter;
            native.Shift = tx.Shift;
            native.Stabil = tx.Stabil;
            native.Jitcnt = tx.Jitcnt;
            native.Calcnt = tx.Calcnt;
            native.Errcnt = tx.Errcnt;
            native.Stbcnt = tx.Stbcnt;
            native.Tai = tx.Tai;
            copyPadIn(tx.Pad_cgo_0, native.Pad_cgo_0, 4);
            copyPadIn(tx.Pad_cgo_1, native.Pad_cgo_1, 4);
            copyPadIn(tx.Pad_cgo_2, native.Pad_cgo_2, 4);
            copyPadIn(tx.Pad_cgo_3, native.Pad_cgo_3, 44);
        }

        uintptr bufAddr = Ꮡbuf is null || Ꮡbuf.IsNilPointer ? (uintptr)0 : (uintptr)(nint)(&native);

        var (r0, _, e1) = Syscall(SYS_ADJTIMEX, bufAddr, 0, 0);
        nint state = (nint)r0;

        if (Ꮡbuf is not null && !Ꮡbuf.IsNilPointer) {
            ref var tx = ref Ꮡbuf.Value;
            tx.Modes = native.Modes;
            tx.Offset = native.Offset;
            tx.Freq = native.Freq;
            tx.Maxerror = native.Maxerror;
            tx.Esterror = native.Esterror;
            tx.Status = native.Status;
            tx.Constant = native.Constant;
            tx.Precision = native.Precision;
            tx.Tolerance = native.Tolerance;
            tx.Time = new Timeval{ Sec = native.Time.Sec, Usec = native.Time.Usec };
            tx.Tick = native.Tick;
            tx.Ppsfreq = native.Ppsfreq;
            tx.Jitter = native.Jitter;
            tx.Shift = native.Shift;
            tx.Stabil = native.Stabil;
            tx.Jitcnt = native.Jitcnt;
            tx.Calcnt = native.Calcnt;
            tx.Errcnt = native.Errcnt;
            tx.Stbcnt = native.Stbcnt;
            tx.Tai = native.Tai;
            copyPadOut(native.Pad_cgo_0, ref tx.Pad_cgo_0, 4);
            copyPadOut(native.Pad_cgo_1, ref tx.Pad_cgo_1, 4);
            copyPadOut(native.Pad_cgo_2, ref tx.Pad_cgo_2, 4);
            copyPadOut(native.Pad_cgo_3, ref tx.Pad_cgo_3, 44);
        }

        error err = default!;
        if (e1 != 0) {
            err = errnoErr(e1);
        }
        return (state, err);
    }

    // ── the two padding-array helpers ────────────────────────────────────────────────────────
    //
    // Go's cgo padding fields are real storage the kernel may write and a caller may read back, so
    // they are carried in both directions rather than dropped. Both tolerate a default-constructed
    // (zero-length) source array, which is what a struct that has never been initialized holds.

    private static unsafe void copyPadIn(array<byte> source, byte* destination, int length) {
        nint sourceLength = source.Length;

        for (int i = 0; i < length; i++)
            destination[i] = i < sourceLength ? source[i] : (byte)0;
    }

    private static unsafe void copyPadOut(byte* source, ref array<byte> destination, int length) {
        if (destination.Length != length)
            destination = new array<byte>(length);

        for (int i = 0; i < length; i++)
            destination[i] = source[i];
    }

    // ── the MULTICAST / ICMPv6 socket-option family ────────────────────────────────────────────
    //
    // Eight members over four structs, and the class's SAME defect on a boundary that was already
    // closed on the other platform. `SetsockoptIPMreq` has been hand-owned for WINDOWS since the
    // sockaddr arc, whose registration says it exactly: ip_mreq is two INLINE in_addr, converted
    // IPMreq holds both as golib `array<byte>` MANAGED REFERENCES, and the wrapper hands the kernel
    // eight bytes that are two OBJECT REFERENCES. Only the goos SCOPE was left at windows, so the
    // Linux flavour kept the generated body and the same defect.
    //
    // MEASURED on Linux before this change: net's TestIPv4MulticastListener fails with
    // `listen udp 224.0.0.254:12345: setsockopt: cannot assign requested address` -- EADDRNOTAVAIL
    // on IP_ADD_MEMBERSHIP, which is the Linux errno spelling of the WSAEINVAL the Windows entry
    // records for the identical call. The managed struct measures 32 bytes (array<byte> is 16 --
    // a T[] reference plus two ints -- and IPMreq holds two) against the 8 that SizeofIPMreq
    // promises the kernel, and the first eight bytes handed over are m_array, a heap pointer.
    //
    // THE GET SIDE IS THE DANGEROUS HALF and none of it was registered anywhere. Getsockopt*
    // allocate a HEAP box of the managed struct and hand the kernel `Ꮡvalue`, so the kernel writes
    // its bytes over `array<>` references inside a GC-tracked object -- this file's own corruption
    // sub-class, for which the header already ruled that "no roster row reached it" is a statement
    // about coverage and not about execution. They are closed here on the mechanism.
    //
    // ICMPv6Filter is the fourth struct and was surfaced by the census rather than by the ruling
    // that commissioned this work; it is the same shape (one `array<uint32>` of 8) at the same
    // boundary, in both directions.
    //
    // The remedy is the Windows body's, which for this family is simpler than a mirror struct: the
    // layouts are inline arrays plus at most one scalar, so a stack buffer with an explicit
    // element copy IS the native image. A nil pointer reaches the kernel as a zeroed option in Go's
    // own wrappers too, so the zeroed buffer is the faithful image rather than a special case.


    // The two boundary calls, spelled as this file spells its others: straight to Syscall6 with
    // uintptr arguments, so no managed image and no golib pointer wrapper stands between the stack
    // buffer and the kernel.
    private static unsafe error nativeSetsockopt(nint fd, nint level, nint opt, byte* val, int vallen) {
        var (_, _, e1) = Syscall6(SYS_SETSOCKOPT, (uintptr)fd, (uintptr)level, (uintptr)opt, (uintptr)(nint)val, (uintptr)vallen, 0);
        return e1 != 0 ? errnoErr(e1) : default!;
    }

    private static unsafe error nativeGetsockopt(nint fd, nint level, nint opt, byte* val, _Socklen* vallen) {
        var (_, _, e1) = Syscall6(SYS_GETSOCKOPT, (uintptr)fd, (uintptr)level, (uintptr)opt, (uintptr)(nint)val, (uintptr)(nint)vallen, 0);
        return e1 != 0 ? errnoErr(e1) : default!;
    }

    private const int nativeIPMreqLen = 8;          // struct ip_mreq:      2 x in_addr
    private const int nativeIPMreqnLen = 12;        // struct ip_mreqn:     2 x in_addr + int
    private const int nativeIPv6MreqLen = 20;       // struct ipv6_mreq:    in6_addr + unsigned int
    private const int nativeICMPv6FilterLen = 32;   // struct icmp6_filter: 8 x uint32

    public static unsafe error /*err*/ SetsockoptIPMreq(nint fd, nint level, nint opt, ж<IPMreq> Ꮡmreq) {
        byte* buffer = stackalloc byte[nativeIPMreqLen];

        if (Ꮡmreq is not null && !Ꮡmreq.IsNilPointer) {
            ref var mreq = ref Ꮡmreq.Value;

            for (nint i = 0; i < 4; i++) {
                buffer[i] = mreq.Multiaddr[i];
                buffer[4 + i] = mreq.Interface[i];
            }
        }
        return nativeSetsockopt(fd, level, opt, buffer, nativeIPMreqLen);
    }

    public static unsafe error /*err*/ SetsockoptIPMreqn(nint fd, nint level, nint opt, ж<IPMreqn> Ꮡmreq) {
        byte* buffer = stackalloc byte[nativeIPMreqnLen];

        if (Ꮡmreq is not null && !Ꮡmreq.IsNilPointer) {
            ref var mreq = ref Ꮡmreq.Value;

            for (nint i = 0; i < 4; i++) {
                buffer[i] = mreq.Multiaddr[i];
                buffer[4 + i] = mreq.Address[i];
            }
            *(int32*)(buffer + 8) = mreq.Ifindex;
        }
        return nativeSetsockopt(fd, level, opt, buffer, nativeIPMreqnLen);
    }

    public static unsafe error /*err*/ SetsockoptIPv6Mreq(nint fd, nint level, nint opt, ж<IPv6Mreq> Ꮡmreq) {
        byte* buffer = stackalloc byte[nativeIPv6MreqLen];

        if (Ꮡmreq is not null && !Ꮡmreq.IsNilPointer) {
            ref var mreq = ref Ꮡmreq.Value;

            for (nint i = 0; i < 16; i++)
                buffer[i] = mreq.Multiaddr[i];

            *(uint32*)(buffer + 16) = mreq.Interface;
        }
        return nativeSetsockopt(fd, level, opt, buffer, nativeIPv6MreqLen);
    }

    public static unsafe error SetsockoptICMPv6Filter(nint fd, nint level, nint opt, ж<ICMPv6Filter> Ꮡfilter) {
        byte* buffer = stackalloc byte[nativeICMPv6FilterLen];

        if (Ꮡfilter is not null && !Ꮡfilter.IsNilPointer) {
            ref var filter = ref Ꮡfilter.Value;

            for (nint i = 0; i < 8; i++)
                ((uint32*)buffer)[i] = filter.Data[i];
        }
        return nativeSetsockopt(fd, level, opt, buffer, nativeICMPv6FilterLen);
    }

    public static unsafe (ж<IPMreq>, error) GetsockoptIPMreq(nint fd, nint level, nint opt) {
        byte* buffer = stackalloc byte[nativeIPMreqLen];
        _Socklen vallen = (_Socklen)nativeIPMreqLen;

        var err = nativeGetsockopt(fd, level, opt, buffer, &vallen);
        ref var value = ref heap(new IPMreq(), out var Ꮡvalue);

        for (nint i = 0; i < 4; i++) {
            value.Multiaddr[i] = buffer[i];
            value.Interface[i] = buffer[4 + i];
        }
        return (Ꮡvalue, err);
    }

    public static unsafe (ж<IPMreqn>, error) GetsockoptIPMreqn(nint fd, nint level, nint opt) {
        byte* buffer = stackalloc byte[nativeIPMreqnLen];
        _Socklen vallen = (_Socklen)nativeIPMreqnLen;

        var err = nativeGetsockopt(fd, level, opt, buffer, &vallen);
        ref var value = ref heap(new IPMreqn(), out var Ꮡvalue);

        for (nint i = 0; i < 4; i++) {
            value.Multiaddr[i] = buffer[i];
            value.Address[i] = buffer[4 + i];
        }
        value.Ifindex = *(int32*)(buffer + 8);
        return (Ꮡvalue, err);
    }

    public static unsafe (ж<IPv6Mreq>, error) GetsockoptIPv6Mreq(nint fd, nint level, nint opt) {
        byte* buffer = stackalloc byte[nativeIPv6MreqLen];
        _Socklen vallen = (_Socklen)nativeIPv6MreqLen;

        var err = nativeGetsockopt(fd, level, opt, buffer, &vallen);
        ref var value = ref heap(new IPv6Mreq(), out var Ꮡvalue);

        for (nint i = 0; i < 16; i++)
            value.Multiaddr[i] = buffer[i];

        value.Interface = *(uint32*)(buffer + 16);
        return (Ꮡvalue, err);
    }

    public static unsafe (ж<ICMPv6Filter>, error) GetsockoptICMPv6Filter(nint fd, nint level, nint opt) {
        byte* buffer = stackalloc byte[nativeICMPv6FilterLen];
        _Socklen vallen = (_Socklen)nativeICMPv6FilterLen;

        var err = nativeGetsockopt(fd, level, opt, buffer, &vallen);
        ref var value = ref heap(new ICMPv6Filter(), out var Ꮡvalue);

        for (nint i = 0; i < 8; i++)
            value.Data[i] = ((uint32*)buffer)[i];

        return (Ꮡvalue, err);
    }
}
