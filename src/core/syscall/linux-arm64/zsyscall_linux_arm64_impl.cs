// zsyscall_linux_arm64_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// linux/arm64's Fstat and fstatat: the struct-passing class's first members (its write-up is the header of
// linux-amd64/zsyscall_linux_amd64_impl.cs), with arm64's own `struct stat` (S3, the zsyscall twin:
// docs/phase4/DATA-la64-s3-census.md §3 and §6).
//
// WHY A TWIN AND NOT THE SHARED FILE. The class's other members sit in linux/structclass_linux_impl.cs,
// which both linux builds compile, because Go lays their structs out identically at amd64 and arm64.
// Stat_t is the exception: 144 bytes at amd64, 128 at arm64, and in a different order (Mode@16 before
// Nlink@20, Blksize an int32 plus a pad, two int32 reserved words at the end where amd64 has three
// int64). So the remedy is the same, a blittable mirror the kernel writes and a field-for-field copy into
// the converted Stat_t, but the mirror is arm64's, read from ztypes_linux_arm64.go:
//
//     Dev@0 Ino@8 Mode@16 Nlink@20 Uid@24 Gid@28 Rdev@32 X__pad1@40 Size@48 Blksize@56 X__pad2@60
//     Blocks@64 Atim@72 Mtim@88 Ctim@104 X__glibc_reserved@120, size 128
//
// (Go's own layouts at both arches, through the census's layoutprobe:
// docs/phase4/data/la64-s3/layouts-linux-amd64-arm64.tsv). GoNativeStatLayoutProbe below hands the
// mirror's MEASURED size and offsets to GolibTests' LinuxArm64StatLayoutTests, which asserts them against
// those values; the size check at each call is the run-time half of the same guard.
//
// THE NUMBERS ARE GO'S. zsyscall_linux_arm64.go's Fstat is Syscall(SYS_FSTAT, …), 80, and its fstatat is
// Syscall6(SYS_FSTATAT, …), 79: the kernel's newfstatat, which amd64's table names SYS_NEWFSTATAT. Both
// come from this flavour's converted zsysnum_linux_arm64.cs, never a literal.
//
// SELECT'S TRAP (S3 site 6) is here too: selectTrap, at the end of this file. arm64 has no select(2), and
// Select itself is linux/structclass_linux_impl.cs's, shared with amd64 down to the fd_set mirrors.
//
// Displaced by `manualConversionFuncs` ("syscall": Fstat, fstatat, goosLinux), like the amd64 pair. The principal
// is the converted zsyscall_linux_arm64.cs, which only linux/arm64 emits, so the router keeps this file in
// linux-arm64/.

using System;
using System.Runtime.InteropServices;

// Hand-owned (no zsyscall_linux_arm64_impl.go exists, so a reconvert never regenerates this file).
[module: go.GoManualConversion]
[module: go.GoRequiresUnsafe]

namespace go;

partial class syscall_package
{
    // `struct stat` exactly as linux/arm64 lays it out: 128 bytes. Field names follow Go's Stat_t, so the
    // copy below reads as identity. `fixed` keeps the reserved words inline.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeStatLinuxArm64
    {
        public uint64 Dev;
        public uint64 Ino;
        public uint32 Mode;
        public uint32 Nlink;
        public uint32 Uid;
        public uint32 Gid;
        public uint64 Rdev;
        public uint64 X__pad1;
        public int64 Size;
        public int32 Blksize;
        public int32 X__pad2;
        public int64 Blocks;
        public NativeTimespecLinuxArm64 Atim;
        public NativeTimespecLinuxArm64 Mtim;
        public NativeTimespecLinuxArm64 Ctim;
        public fixed int32 X__glibc_reserved[2];
    }

    // `struct timespec`: two int64 words, as at amd64.
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeTimespecLinuxArm64
    {
        public int64 Sec;
        public int64 Nsec;
    }

    // The kernel's own size for this arch, checked at the boundary (amd64's file explains why not at type
    // initialization).
    private const int nativeStatSizeLinuxArm64 = 128;

    // Fstat is the native transcription of the generated wrapper. The kernel writes the blittable mirror;
    // on success its fields are copied into the converted Stat_t exactly as Go's stat landed in place.
    // Errors follow the Go original: e1 != 0 → errnoErr(e1), and the caller's struct is left untouched.
    public static unsafe error /*err*/ Fstat(nint fd, ж<Stat_t> Ꮡstat) {
        if (sizeof(NativeStatLinuxArm64) != nativeStatSizeLinuxArm64)
            throw new InvalidOperationException($"syscall: NativeStatLinuxArm64 is {sizeof(NativeStatLinuxArm64)} bytes, the kernel's struct stat is {nativeStatSizeLinuxArm64}");

        NativeStatLinuxArm64 native = default;
        // A nil *Stat_t reaches the kernel as address 0 in Go and answers EFAULT; keep that.
        uintptr statAddr = Ꮡstat is null || Ꮡstat.IsNilPointer ? (uintptr)0 : (uintptr)(nint)(&native);

        var (_, _, e1) = Syscall(SYS_FSTAT, (uintptr)fd, statAddr, 0);
        if (e1 != 0) {
            return errnoErr(e1);
        }

        copyNativeStat(ref native, Ꮡstat!);
        return default!;
    }

    // fstatat: the same transcription over the four-argument form, under Go's own number for it at arm64,
    // SYS_FSTATAT (79: the kernel's newfstatat, which amd64's table names SYS_NEWFSTATAT). The path
    // handling is the generated wrapper's own, verbatim, as in the amd64 body; only the struct argument is
    // replaced by the mirror. Stat and Lstat are pure Go over this (syscall_linux_arm64.go) and inherit it.
    internal static unsafe error /*err*/ fstatat(nint fd, @string path, ж<Stat_t> Ꮡstat, nint flags) {
        error err = default!;

        ж<byte> _p0 = default!;
        (_p0, err) = BytePtrFromString(path);
        if (err != default!) {
            return err;
        }

        if (sizeof(NativeStatLinuxArm64) != nativeStatSizeLinuxArm64)
            throw new InvalidOperationException($"syscall: NativeStatLinuxArm64 is {sizeof(NativeStatLinuxArm64)} bytes, the kernel's struct stat is {nativeStatSizeLinuxArm64}");

        NativeStatLinuxArm64 native = default;
        uintptr statAddr = Ꮡstat is null || Ꮡstat.IsNilPointer ? (uintptr)0 : (uintptr)(nint)(&native);

        var (_, _, e1) = Syscall6(SYS_FSTATAT, (uintptr)fd, (uintptr)_p0, statAddr, (uintptr)flags, 0, 0);
        if (e1 != 0) {
            return errnoErr(e1);
        }

        copyNativeStat(ref native, Ꮡstat!);
        return default!;
    }

    // The field-for-field copy at the boundary; the reserved words are copied too, reusing the struct's own
    // `array<int32>` when it is already the right length (a default Stat_t carries a zero-length one, the
    // converted initializer a 2-long one).
    private static unsafe void copyNativeStat(ref NativeStatLinuxArm64 native, ж<Stat_t> Ꮡstat) {
        ref var st = ref Ꮡstat.Value;

        st.Dev = native.Dev;
        st.Ino = native.Ino;
        st.Mode = native.Mode;
        st.Nlink = native.Nlink;
        st.Uid = native.Uid;
        st.Gid = native.Gid;
        st.Rdev = native.Rdev;
        st.X__pad1 = native.X__pad1;
        st.Size = native.Size;
        st.Blksize = native.Blksize;
        st.X__pad2 = native.X__pad2;
        st.Blocks = native.Blocks;
        st.Atim = new Timespec{ Sec = native.Atim.Sec, Nsec = native.Atim.Nsec };
        st.Mtim = new Timespec{ Sec = native.Mtim.Sec, Nsec = native.Mtim.Nsec };
        st.Ctim = new Timespec{ Sec = native.Ctim.Sec, Nsec = native.Ctim.Nsec };

        if (st.X__glibc_reserved.Length != 2)
            st.X__glibc_reserved = new array<int32>(2);

        for (int i = 0; i < 2; i++)
            st.X__glibc_reserved[i] = native.X__glibc_reserved[i];
    }

    /// <summary>
    /// GolibTests' probe (LinuxArm64StatLayoutTests): the mirror's size and each field's offset, measured
    /// from a live instance, in Go's field order, with the three timespecs read to their Sec and Nsec words.
    /// </summary>
    public static unsafe (int size, (string field, int offset)[] offsets) GoNativeStatLayoutProbe() {
        NativeStatLinuxArm64 s = default;
        byte* b = (byte*)&s;

        return (sizeof(NativeStatLinuxArm64), new (string, int)[] {
            ("Dev", offsetIn(b, &s.Dev)), ("Ino", offsetIn(b, &s.Ino)),
            ("Mode", offsetIn(b, &s.Mode)), ("Nlink", offsetIn(b, &s.Nlink)),
            ("Uid", offsetIn(b, &s.Uid)), ("Gid", offsetIn(b, &s.Gid)),
            ("Rdev", offsetIn(b, &s.Rdev)), ("X__pad1", offsetIn(b, &s.X__pad1)),
            ("Size", offsetIn(b, &s.Size)), ("Blksize", offsetIn(b, &s.Blksize)),
            ("X__pad2", offsetIn(b, &s.X__pad2)), ("Blocks", offsetIn(b, &s.Blocks)),
            ("Atim.Sec", offsetIn(b, &s.Atim.Sec)), ("Atim.Nsec", offsetIn(b, &s.Atim.Nsec)),
            ("Mtim.Sec", offsetIn(b, &s.Mtim.Sec)), ("Mtim.Nsec", offsetIn(b, &s.Mtim.Nsec)),
            ("Ctim.Sec", offsetIn(b, &s.Ctim.Sec)), ("Ctim.Nsec", offsetIn(b, &s.Ctim.Nsec)),
            ("X__glibc_reserved", offsetIn(b, s.X__glibc_reserved)),
        });

        static int offsetIn(byte* start, void* field) => (int)((byte*)field - start);
    }

    // Select's trap at linux/arm64 (S3 site 6; the fd_set mirrors and the rest of Select are in
    // linux/structclass_linux_impl.cs). Go's arm64 Select (syscall_linux_arm64.go) is
    //
    //     var ts *Timespec
    //     if timeout != nil { ts = &Timespec{Sec: timeout.Sec, Nsec: timeout.Usec * 1000} }
    //     return pselect(nfd, r, w, e, ts, nil)
    //
    // and pselect is Syscall6(SYS_PSELECT6, …), 72, from this flavour's converted zsysnum_linux_arm64.cs.
    // The Timespec is Go's TEMPORARY. The kernel writes the time not slept into it, and the caller's Timeval
    // never sees that, unlike amd64's select(2). So nothing is copied back. The converted pselect is not
    // called: it hands the kernel its ж<FdSet> boxes, not inline fd_sets. A nil timeout stays address 0
    // ("block forever"), and the sigmask is nil, as in Go.
    private static unsafe (uintptr r0, Errno e1) selectTrap(uintptr nfd, uintptr r, uintptr w, uintptr e, ж<Timeval> Ꮡtimeout) {
        NativeTimespecLinuxArm64 ts = default;
        uintptr tsAddr = 0;

        if (Ꮡtimeout is not null && !Ꮡtimeout.IsNilPointer) {
            ref var tv = ref Ꮡtimeout.Value;
            ts.Sec = tv.Sec;
            ts.Nsec = tv.Usec * 1000;
            tsAddr = (uintptr)(nint)(&ts);
        }

        var (r0, _, e1) = Syscall6(SYS_PSELECT6, nfd, r, w, e, tsAddr, 0);
        return (r0, e1);
    }
}
