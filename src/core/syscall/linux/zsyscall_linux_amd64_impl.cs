// zsyscall_linux_amd64_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementations of the two generated Linux syscall wrappers whose STRUCT cannot
// cross the managed/native boundary by address: Fstat and fstatat, the bottom of every os.Stat,
// os.Lstat, File.Stat and DirEntry.Info on the Linux flavor.
//
// Go's wrappers are plain mksyscall output (zsyscall_linux_amd64.go):
//
//     _, _, e1 := Syscall(SYS_FSTAT, uintptr(fd), uintptr(unsafe.Pointer(stat)), 0)
//     _, _, e1 := Syscall6(SYS_NEWFSTATAT, uintptr(fd), uintptr(unsafe.Pointer(_p0)), uintptr(unsafe.Pointer(stat)), uintptr(flags), 0, 0)
//
// which work in Go because Stat_t on linux/amd64 IS the kernel's 144-byte `struct stat`, field for
// field, with `X__unused [3]int64` inline at the end. The converted Stat_t cannot be: that trailing
// array is a golib `array<int64>` MANAGED REFERENCE, so the struct is not blittable, the CLR lays it
// out as it pleases (~128 bytes, fields reordered), and `(uintptr)Ꮡstat` — golib pinning the box's
// VALUE slot — hands the kernel that managed image. fstat(2)/fstatat(2) then write 144 bytes of
// native `struct stat` over a field order that is not the kernel's AND 16 bytes past the object.
// Nothing faults. MEASURED on the 2026-08-22 Linux roster re-run, in an isolated probe: `os.Stat(dir)`
// returned err == nil with `IsDir() == false` and `Mode() == p---------` for a real directory, and
// `Stat().Size()` read 0 for a 3,302-byte file — a quiet wrong ANSWER, the worst shape this class
// takes — while Readdirnames/ReadDir/Read (dirent-typed, no Stat) were correct. Downstream: every
// filepath.Glob answered nothing (glob swallows the I/O error and tests IsDir), every Walk/WalkDir
// visited only its root, archive/zip read "not a valid zip file" from a mis-sized archive, MkdirAll
// said "not a directory" — 8 roster rows wall-to-wall plus partials, all attributed to this one seam
// (board entry 2026-08-22, R1).
//
// This is the Linux instance of the syscall STRUCT-PASSING class the Windows lane has been retiring
// member by member (board: "the syscall STRUCT-PASSING seam"; zsyscall_windows_impl.cs is the
// worked example), and it takes the same remedy: a blittable [StructLayout(LayoutKind.Sequential)]
// mirror of the NATIVE layout, the keystone libc syscall(2) binding handed the mirror's address, and
// an explicit field-for-field copy back into the converted struct at the boundary. The two
// wrappers are displaced from the generated file by `manualConversionFuncs` ("syscall": Fstat,
// fstatat — scoped goosLinux: darwin declares both names with libc-backed bodies that are not
// defective). Stat and Lstat are NOT hand-owned: on linux/amd64 they are pure Go over fstatat
// (syscall_linux_amd64.go) and convert faithfully, so they inherit the fix through this file.
//
// WHAT IS NOT DONE, AND WHY. The mirror is linux/AMD64's `struct stat` (ztypes_linux_amd64.go).
// The corpus converts exactly that GOARCH; another Linux arch's Stat_t (386, arm64, …) has a
// different layout and would need its own mirror in its own zsyscall_<arch>_impl.cs — stated
// rather than generalized, because no such flavor exists in the corpus to measure against.
// THE DEFERRAL RULE THIS HEADER USED TO STATE IS RETIRED (2026-08-28). It read: Statfs_t,
// Sysinfo_t, Utsname and the rest "are NOT taken here: no roster row reached them, and the class
// doctrine is per-member, when reached." That rule was written for the symptom Stat_t
// demonstrated — a quiet wrong ANSWER — and it is unsafe for what the write actually does. These
// structs hold their `array<>` fields as MANAGED REFERENCES in a GC-tracked slot, so the kernel
// writes its bytes over reference slots and the collector then follows the wreckage: not a bad
// value in the caller's struct, but heap corruption surfacing arbitrarily far away. Uname proved
// it — it sat on that deferred list, with that reason, WHILE os/exec reached it, and `uname(2)`
// writing 390 bytes over six reference slots was the corruptor behind a campaign of
// unattributable crashes (verifyheap: 6 errors in one contiguous run; ground truth: the host
// killed by SIGSEGV before the mirror, an ordinary exit after). "No roster row reached it" is a
// statement about COVERAGE, not about execution.
//
// So the rule is now: a member of this class whose converted struct carries `array<>` fields is
// taken ON THE MECHANISM, before it is reached. Uname is below; Select, FcntlFlock, Statfs,
// Fstatfs, Sysinfo and Adjtimex — the rest of the linux/amd64 surface — are in
// structclass_linux_impl.cs. What remains genuinely not done is the ARCH dimension above: another
// Linux arch's layouts would need their own mirrors in their own zsyscall_<arch>_impl.cs.
//
// And the landing rule both halves of the fleet learned the same day, by each hitting it: a
// hand-own here is not landable without its GENERATED body going in the same commit.
// `manualConversionFuncs` displaces a wrapper at CONVERSION time, so a committed corpus file that
// still carries the generated body gives `CS0111: already defines a member` — visible only at a
// build of this flavor, because the `-tests` pipeline converts only the package under test and
// never regenerates `syscall`.
//
// THE SEAM RULE, which generalizes past this class: A GENERATED FILE THAT A HAND-OWN PARTLY
// DISPLACES IS REGENERATED AT A MERGE, NEVER MERGED. When two lanes displace different wrappers in
// one generated file their edits touch disjoint regions, so git auto-merges them WITHOUT A
// CONFLICT — producing a file that compiles and that no converter run would ever emit. Measured
// 2026-08-28 at exactly that seam: one lane had hand-deleted `Uname`'s body while the other's
// reconvert had emitted placeholder comments for six more members, and the clean auto-merge kept
// the deletion without the placeholder, because a hand-deletion and a reconvert express the same
// fact in different text. A clean auto-merge here is evidence of NOTHING — the generator is the
// only authority on this file's content, so re-run it and let the registrations decide. The
// property that makes it checkable: every name registered for this flavor has ZERO bodies and
// EXACTLY ONE placeholder in the generated file. Assert that after the reconvert, before the
// build.
//
// That is the same shape as the two false verdicts the os/exec arc paid for on the same day — an
// instrument reading "no crash strings" as a clean run when nothing had run, then the same
// instrument reading a test's own `core dumped` payload as a host crash. THE ABSENCE OF A FAILURE
// SIGNAL IS NOT THE PRESENCE OF CORRECTNESS. Assert the property; never the silence.

using System;
using System.Runtime.InteropServices;

// Hand-owned (no zsyscall_linux_amd64_impl.go exists, so a reconvert never regenerates this file);
// marked per the hand-own rules, and it declares its own /unsafe need (the mirror's `fixed` buffer
// and the address-of below) rather than inheriting it from the Windows flavor's declaration.
[module: go.GoManualConversion]
[module: go.GoRequiresUnsafe]

namespace go;

partial class syscall_package
{
    // `struct stat` exactly as linux/amd64 lays it out — 144 bytes, the trailing reserved words
    // INLINE. `fixed` keeps them inline (an array field would be another reference, which is the
    // whole bug); the struct is therefore blittable and the address of a stack instance is a real
    // kernel-writable buffer. Field names follow Go's Stat_t so the copy below reads as identity.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeStatLinuxAmd64
    {
        public uint64 Dev;
        public uint64 Ino;
        public uint64 Nlink;
        public uint32 Mode;
        public uint32 Uid;
        public uint32 Gid;
        public int32 X__pad0;
        public uint64 Rdev;
        public int64 Size;
        public int64 Blksize;
        public int64 Blocks;
        public NativeTimespec Atim;
        public NativeTimespec Mtim;
        public NativeTimespec Ctim;
        public fixed int64 X__unused[3];
    }

    // `struct timespec`: two int64 words. The converted Timespec already has this exact shape, but
    // mirrored here so the enclosing layout is stated in one place.
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeTimespec
    {
        public int64 Sec;
        public int64 Nsec;
    }

    // The kernel's own size for this arch. Checked at the boundary rather than asserted at type
    // initialization, so a wrong mirror fails the CALL loudly instead of taking the whole package's
    // static constructor down.
    private const int nativeStatSizeLinuxAmd64 = 144;

    // Fstat is the native transcription of the generated wrapper — see the file header for why it
    // cannot be a literal conversion. The kernel writes the blittable mirror; on success its fields
    // are copied into the converted Stat_t exactly as Go's stat landed in place. Errors follow the
    // Go original: e1 != 0 → errnoErr(e1), and the caller's struct is left untouched.
    public static unsafe error /*err*/ Fstat(nint fd, ж<Stat_t> Ꮡstat) {
        if (sizeof(NativeStatLinuxAmd64) != nativeStatSizeLinuxAmd64)
            throw new InvalidOperationException($"syscall: NativeStatLinuxAmd64 is {sizeof(NativeStatLinuxAmd64)} bytes, the kernel's struct stat is {nativeStatSizeLinuxAmd64}");

        NativeStatLinuxAmd64 native = default;
        // A nil *Stat_t reaches the kernel as address 0 in Go and answers EFAULT; keep that.
        uintptr statAddr = Ꮡstat is null || Ꮡstat.IsNilPointer ? (uintptr)0 : (uintptr)(nint)(&native);

        var (_, _, e1) = Syscall(SYS_FSTAT, (uintptr)fd, statAddr, 0);
        if (e1 != 0) {
            return errnoErr(e1);
        }

        copyNativeStat(ref native, Ꮡstat!);
        return default!;
    }

    // fstatat: the same transcription over the four-argument form. The path handling is the
    // generated wrapper's own, verbatim — BytePtrFromString and golib's byte-box pinning are what
    // every other path-taking wrapper in this package already relies on; only the struct argument
    // is replaced by the mirror.
    internal static unsafe error /*err*/ fstatat(nint fd, @string path, ж<Stat_t> Ꮡstat, nint flags) {
        error err = default!;

        ж<byte> _p0 = default!;
        (_p0, err) = BytePtrFromString(path);
        if (err != default!) {
            return err;
        }

        if (sizeof(NativeStatLinuxAmd64) != nativeStatSizeLinuxAmd64)
            throw new InvalidOperationException($"syscall: NativeStatLinuxAmd64 is {sizeof(NativeStatLinuxAmd64)} bytes, the kernel's struct stat is {nativeStatSizeLinuxAmd64}");

        NativeStatLinuxAmd64 native = default;
        uintptr statAddr = Ꮡstat is null || Ꮡstat.IsNilPointer ? (uintptr)0 : (uintptr)(nint)(&native);

        var (_, _, e1) = Syscall6(SYS_NEWFSTATAT, (uintptr)fd, (uintptr)_p0, statAddr, (uintptr)flags, 0, 0);
        if (e1 != 0) {
            return errnoErr(e1);
        }

        copyNativeStat(ref native, Ꮡstat!);
        return default!;
    }

    // The field-for-field copy at the boundary. Every scalar is assigned, the three timespecs are
    // rebuilt, and the reserved words are copied too (Go's caller sees whatever the kernel wrote
    // there, which is zero) — reusing the struct's own `array<int64>` when it is already the
    // right length, so a Stat_t that is reused across calls does not allocate per call.
    private static unsafe void copyNativeStat(ref NativeStatLinuxAmd64 native, ж<Stat_t> Ꮡstat) {
        ref var st = ref Ꮡstat.Value;

        st.Dev = native.Dev;
        st.Ino = native.Ino;
        st.Nlink = native.Nlink;
        st.Mode = native.Mode;
        st.Uid = native.Uid;
        st.Gid = native.Gid;
        st.X__pad0 = native.X__pad0;
        st.Rdev = native.Rdev;
        st.Size = native.Size;
        st.Blksize = native.Blksize;
        st.Blocks = native.Blocks;
        st.Atim = new Timespec{ Sec = native.Atim.Sec, Nsec = native.Atim.Nsec };
        st.Mtim = new Timespec{ Sec = native.Mtim.Sec, Nsec = native.Mtim.Nsec };
        st.Ctim = new Timespec{ Sec = native.Ctim.Sec, Nsec = native.Ctim.Nsec };

        // array<T> is a golib VALUE type: a default Stat_t carries a zero-length one (Length 0, no
        // backing store), the converted initializer a 3-long one. Reuse the latter, mint the former.
        if (st.X__unused.Length != 3)
            st.X__unused = new array<int64>(3);

        for (int i = 0; i < 3; i++)
            st.X__unused[i] = native.X__unused[i];
    }

    // ── wait4: the class's first BLOCKING member (JOB-024's os/exec SIGSEGV arc, 2026-08-26) ──
    //
    // The generated wrapper handed the kernel `(uintptr)Ꮡwstatus` and `(uintptr)Ꮡrusage` — golib
    // box addresses — across SYS_WAIT4. Unlike Fstat's instantaneous write, wait4 BLOCKS until a
    // child changes state, and a transient box address does not survive a blocking call: GC
    // compaction relocates the boxes mid-wait and the kernel's eventual status/rusage write lands
    // on whatever object moved in — heap corruption that surfaces as a SIGSEGV at an unrelated
    // later point. Measured 4-for-4 on os/exec's suite the day the exec wall opened (the death
    // point moved between runs, which IS this mechanism's signature; rooted from a crash dump
    // whose wait threads sat parked in exactly this call). Same remedy as above, plus the rule
    // the blocking shape adds: the native buffer must live for the WHOLE call, so it is a stack
    // local of this frame, which cannot move and cannot outlive the syscall.
    //
    // Rusage on linux/amd64 is all-scalar (two Timevals + fourteen int64s, 144 bytes) so its
    // mirror is layout-identity; the copy back is field-for-field like copyNativeStat's. A nil
    // Ꮡwstatus/Ꮡrusage reaches the kernel as address 0, exactly as in Go.
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeTimevalLinuxAmd64
    {
        public int64 Sec;
        public int64 Usec;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRusageLinuxAmd64
    {
        public NativeTimevalLinuxAmd64 Utime;
        public NativeTimevalLinuxAmd64 Stime;
        public int64 Maxrss;
        public int64 Ixrss;
        public int64 Idrss;
        public int64 Isrss;
        public int64 Minflt;
        public int64 Majflt;
        public int64 Nswap;
        public int64 Inblock;
        public int64 Oublock;
        public int64 Msgsnd;
        public int64 Msgrcv;
        public int64 Nsignals;
        public int64 Nvcsw;
        public int64 Nivcsw;
    }

    private const int nativeRusageSizeLinuxAmd64 = 144;

    internal static unsafe (nint wpid, error err) wait4(nint pid, ж<_C_int> Ꮡwstatus, nint options, ж<Rusage> Ꮡrusage) {
        if (sizeof(NativeRusageLinuxAmd64) != nativeRusageSizeLinuxAmd64)
            throw new InvalidOperationException($"syscall: NativeRusageLinuxAmd64 is {sizeof(NativeRusageLinuxAmd64)} bytes, the kernel's struct rusage is {nativeRusageSizeLinuxAmd64}");

        int32 nativeStatus = 0;
        NativeRusageLinuxAmd64 nativeRusage = default;
        uintptr statusAddr = Ꮡwstatus is null || Ꮡwstatus.IsNilPointer ? (uintptr)0 : (uintptr)(nint)(&nativeStatus);
        uintptr rusageAddr = Ꮡrusage is null || Ꮡrusage.IsNilPointer ? (uintptr)0 : (uintptr)(nint)(&nativeRusage);

        var (r0, _, e1) = Syscall6(SYS_WAIT4, (uintptr)pid, statusAddr, (uintptr)options, rusageAddr, 0, 0);
        nint wpid = (nint)r0;
        if (e1 != 0) {
            return (wpid, errnoErr(e1));
        }

        if (Ꮡwstatus is not null && !Ꮡwstatus.IsNilPointer) {
            Ꮡwstatus.Value = nativeStatus;
        }

        if (Ꮡrusage is not null && !Ꮡrusage.IsNilPointer) {
            ref var ru = ref Ꮡrusage.Value;
            ru.Utime = new Timeval{ Sec = nativeRusage.Utime.Sec, Usec = nativeRusage.Utime.Usec };
            ru.Stime = new Timeval{ Sec = nativeRusage.Stime.Sec, Usec = nativeRusage.Stime.Usec };
            ru.Maxrss = nativeRusage.Maxrss;
            ru.Ixrss = nativeRusage.Ixrss;
            ru.Idrss = nativeRusage.Idrss;
            ru.Isrss = nativeRusage.Isrss;
            ru.Minflt = nativeRusage.Minflt;
            ru.Majflt = nativeRusage.Majflt;
            ru.Nswap = nativeRusage.Nswap;
            ru.Inblock = nativeRusage.Inblock;
            ru.Oublock = nativeRusage.Oublock;
            ru.Msgsnd = nativeRusage.Msgsnd;
            ru.Msgrcv = nativeRusage.Msgrcv;
            ru.Nsignals = nativeRusage.Nsignals;
            ru.Nvcsw = nativeRusage.Nvcsw;
            ru.Nivcsw = nativeRusage.Nivcsw;
        }

        return (wpid, default!);
    }

    // ── Uname: the class's THIRD member, and the one that is ALL inline storage ──────────────
    //
    // Reached 2026-08-28 by os/exec's TestFindExecutableVsNoexec, which gates itself on
    // `unix.KernelVersion()` — and that function's whole body is `syscall.Uname` plus a parse of
    // the Release field (internal/syscall/unix/kernel_version_linux.go). The generated wrapper
    // hands the kernel `(uintptr)Ꮡbuf`, and the converted Utsname is SIX `array<int8>` MANAGED
    // REFERENCES where the kernel's `struct utsname` is six 65-byte character arrays INLINE. So
    // this member is the class taken to its limit: not a struct with one array field at the end,
    // but a struct that is nothing BUT inline arrays — 390 native bytes against a managed image
    // that is six references and no characters at all.
    //
    // The observable was a quiet wrong ANSWER of the Stat_t kind, one level further removed:
    // KernelVersion returned (0, 0) — the version parse reading a Release field the kernel never
    // filled — so the test took Go's own `t.Skip("requires Linux kernel v5.8 with faccessat2(2)
    // syscall")` on a WSL2 kernel that is 5.15 and has the syscall. Go passes the test; the
    // converted side skipped it. That divergence is a DEFECT, never a platform-skip disclosure:
    // the class's admission test requires the skipped-on property to be one the deployment
    // genuinely holds, and this kernel genuinely has faccessat2 — the conversion simply could not
    // read its own uname. A disclosure here would have laundered a fixable seam into a permanent
    // one, which is exactly what that class's anti-laundering clause exists to prevent.
    //
    // Remedy identical to Fstat's, and simpler for having no scalars: a blittable mirror the
    // kernel writes, then a field-for-field copy back. //sysnb in Go, so RawSyscall with no
    // enter/exitsyscall bracket, exactly as the generated wrapper had it.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeUtsnameLinux
    {
        public fixed int8 Sysname[utsnameFieldLength];
        public fixed int8 Nodename[utsnameFieldLength];
        public fixed int8 Release[utsnameFieldLength];
        public fixed int8 Version[utsnameFieldLength];
        public fixed int8 Machine[utsnameFieldLength];
        public fixed int8 Domainname[utsnameFieldLength];
    }

    // _UTSNAME_LENGTH on Linux: 65 per field (64 characters plus the NUL), six fields, no padding.
    private const int utsnameFieldLength = 65;
    private const int nativeUtsnameSizeLinux = 390;

    public static unsafe error /*err*/ Uname(ж<Utsname> Ꮡbuf) {
        if (sizeof(NativeUtsnameLinux) != nativeUtsnameSizeLinux)
            throw new InvalidOperationException($"syscall: NativeUtsnameLinux is {sizeof(NativeUtsnameLinux)} bytes, the kernel's struct utsname is {nativeUtsnameSizeLinux}");

        NativeUtsnameLinux native = default;
        // A nil *Utsname reaches the kernel as address 0 in Go and answers EFAULT; keep that.
        uintptr bufAddr = Ꮡbuf is null || Ꮡbuf.IsNilPointer ? (uintptr)0 : (uintptr)(nint)(&native);

        var (_, _, e1) = RawSyscall(SYS_UNAME, bufAddr, 0, 0);
        if (e1 != 0) {
            return errnoErr(e1);
        }

        ref var uts = ref Ꮡbuf!.Value;
        copyNativeUtsnameField(native.Sysname, ref uts.Sysname);
        copyNativeUtsnameField(native.Nodename, ref uts.Nodename);
        copyNativeUtsnameField(native.Release, ref uts.Release);
        copyNativeUtsnameField(native.Version, ref uts.Version);
        copyNativeUtsnameField(native.Machine, ref uts.Machine);
        copyNativeUtsnameField(native.Domainname, ref uts.Domainname);
        return default!;
    }

    // One field's 65 inline characters into the converted `array<int8>`. Reuses a backing store
    // that is already the right length — a default Utsname carries zero-length arrays, the
    // converted initializer 65-long ones — so the copy allocates only when it must, the same rule
    // copyNativeStat applies to X__unused. The NUL and everything after it are copied verbatim
    // rather than trimmed: Go's caller receives the kernel's bytes exactly, and the callers that
    // want a string (KernelVersion's parse, os.Hostname's) do their own scan for the terminator.
    private static unsafe void copyNativeUtsnameField(int8* source, ref array<int8> destination) {
        if (destination.Length != utsnameFieldLength)
            destination = new array<int8>(utsnameFieldLength);

        for (int i = 0; i < utsnameFieldLength; i++)
            destination[i] = source[i];
    }
}
