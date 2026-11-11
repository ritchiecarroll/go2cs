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
// taken ON THE MECHANISM, before it is reached. wait4 and Uname (whose Rusage and Utsname are the
// same 144 and 390 bytes at linux/arm64, so both linux builds share one body each since S3), Select,
// FcntlFlock, Statfs, Fstatfs, Sysinfo and Adjtimex — the rest of the linux/amd64 surface — are in
// structclass_linux_impl.cs. Select's trap and timeout are this flavour's own: selectTrap, at the end of
// this file (S3 site 6). What remains genuinely not done is the ARCH dimension above: another
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

    // Select's trap at linux/amd64 (S3 site 6; the fd_set mirrors and the rest of Select are in
    // linux/structclass_linux_impl.cs). Go's amd64 Select is `//sys Select(...) = SYS_SELECT`
    // (syscall_linux_amd64.go): the caller's Timeval is the kernel's timeout, and select(2) writes the
    // time not slept back into it, on EINTR too. So the Timeval crosses in a NativeTimeval mirror and is
    // copied back UNCONDITIONALLY, an identity copy wherever the kernel wrote nothing. A nil timeout stays
    // address 0, which is "block forever".
    private static unsafe (uintptr r0, Errno e1) selectTrap(uintptr nfd, uintptr r, uintptr w, uintptr e, ж<Timeval> Ꮡtimeout) {
        if (Ꮡtimeout is null || Ꮡtimeout.IsNilPointer) {
            var (r0, _, e1) = Syscall6(SYS_SELECT, nfd, r, w, e, 0, 0);
            return (r0, e1);
        }

        ref var tv = ref Ꮡtimeout.Value;
        NativeTimeval native = default;
        native.Sec = tv.Sec;
        native.Usec = tv.Usec;

        var (tr0, _, te1) = Syscall6(SYS_SELECT, nfd, r, w, e, (uintptr)(nint)(&native), 0);

        tv.Sec = native.Sec;
        tv.Usec = native.Usec;
        return (tr0, te1);
    }
}
