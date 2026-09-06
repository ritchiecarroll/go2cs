// user_darwin_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// Hand-written implementations of the four user/group lookups internal/syscall/unix declares on
// DARWIN -- getpwnam_r, getpwuid_r, getgrnam_r, getgrgid_r (user_darwin.go). They are this
// package's members of the PTROUT class -- a Go `**T` OUT-PARAMETER the C library writes a raw
// address into -- and each is taken WITH the record transcription it cannot land without, for the
// reason NetUserGetInfo records one package over: a wrapper that publishes the pointer and stops
// leaves the caller reading a record nothing filled in.
//
// THE CLASS'S WRITE-UP is syscall/windows/zsyscall_windows_ptrout_impl.cs and it holds here word
// for word. `**Passwd` renders as `ж<ж<Passwd>>`, a managed box whose storage is an OBJECT
// REFERENCE, so there is no eight-byte slot to hand libc; while the held pointer is still null --
// which is every out-parameter BEFORE the call -- golib's `ж<T>` -> `uintptr` operator value-peeks
// and answers 0, and once it is non-null the same operator answers a live MANAGED address the
// library would write a raw word over. Neither answer is fixable in the operator, and the sync
// point that reconciles the two representations ("after the call returns") is known only to the
// wrapper. That is why the remedy is here and why ж.cs is deliberately untouched.
//
// WHAT WAS MEASURED, BEFORE ANY OF THIS WAS WRITTEN -- two probe arms, both mac legs (arm64 and
// x64), every buffer size Go's retryWithBuffer tries, with no reading varying between legs:
//
//   ARM ONE (run 34026852472 @ 69e8077343) -- the out-parameter IS the mechanism, not a null beside
//       one. `result` arrives as 0x0 at every size; call A (the emission verbatim) answers errno 34,
//       and the SAME call with one axis changed -- an honest native out-cell -- answers errno 0 with
//       the cell holding exactly the `pwd` pointer, which is getpwuid_r saying FOUND in the only way
//       it can. darwin spells "the caller passed a NULL result" as ERANGE, which is why doubling the
//       buffer from 1 KB to 1 MB never helped and why os/user gave up with "internal buffer exceeds
//       1048576 bytes" -- a message naming the one argument that was never the problem.
//
//   ARM TWO (run 34034875069 @ 2b992eb7d0) -- the record does NOT read back. The same storage
//       decoded two ways: NATIVE at C's offsets reads uid@16=501 gid@20=20, name@0 exactly the buf
//       pointer and dir@48 twenty-two bytes past it, with the name string readable; the MANAGED view
//       the converted code actually reads answers Uid=0 Gid=0 Name=nil Dir=nil -- not garbage,
//       UNWRITTEN. `Passwd` carries six `ж<byte>` fields, i.e. six object references, so the CLR
//       lays it out AUTO and reorders it, and libc's 72 bytes landed beside the managed fields
//       rather than on them. buildUser would have built a user with no name, no home and uid 0.
//
// Both halves are therefore owed by measurement rather than by argument, and they are ONE change:
// the out-cell alone makes the call succeed and still hands buildUser an empty record.
//
// THREE DECISIONS, EACH WITH ITS REASON.
//
// (1) THE RECORD IS A NATIVE MIRROR, NOT THE CALLER'S MANAGED STRUCT. Beyond the layout, the
//     emission's own shape is a memory-safety hazard whatever the layout happens to be: it hands
//     libc the address of a GC-tracked object and lets it write eight-byte raw pointers across it.
//     That those bytes missed the reference slots on two runs is not a guarantee that they must. The
//     mirror is a blittable stack local, live for exactly the call -- the mirror-is-a-local doctrine
//     this repository's other struct-passing seams established.
//
// (2) THE STRINGS ARE COPIED, NOT ALIASED. Each `char *` libc returns points INTO the caller's
//     `buf`, and an element reference into that slice would be sound -- an ElemRefBox holds its
//     backing array, so the storage stays alive and re-pins on every read. It is still not what this
//     file does, for two reasons that cut the same way: libc is not required to point every field
//     into `buf` (a static "" for pw_class is legal), so an aliasing form needs an
//     inside-the-buffer / outside-the-buffer fork whose second arm no measured run has ever taken;
//     and the class already has a copying precedent that needs no fork at all (copyNativeUtf16 in
//     os/user/windows/lookup_windows_impl.cs, copyNativeCanonname in
//     syscall/windows/zsyscall_windows_addrinfo_impl.cs). The copy is one uniform path with no dead
//     arm, and nothing observable is lost: no consumer writes through these pointers, and Go's own
//     buildUser reads them into strings and drops the buffer.
//
// (3) `buf` STAYS THE CALLER'S MANAGED SLICE. It is a `[]byte` -- reference-free storage whose
//     pinned address is exactly what a pinned buffer is for -- and handing libc the caller's own
//     buffer is what keeps `size` and Go's ERANGE retry loop faithful. `(uintptr)Ꮡbuf` pins it
//     through THAT box and nothing longer, so each body carries the `System.GC.KeepAlive` closure
//     the converter emits around its own call sites and a `[module: go.GoManualConversion]` file
//     never receives (dll_windows.cs's soundness note; the ptrout file's transcription of it).
//
// WHAT IS DELIBERATELY NOT DONE, each for a stated reason rather than for lack of effort.
//
//   Group.Mem (`**byte`, the NUL-terminated member vector) is LEFT NIL. Nothing in the corpus reads
//       it: os/user's lookupGroup takes only _C_gr_name and the gid, and cgo_listgroups_unix goes
//       through getgrouplist, which is its own wrapper and not a member of this class. Transcribing
//       the vector would be an unexercised branch inside a hand-own -- a false-green seed -- so it
//       is left as the contained nil Go's own `*Group` holds for an empty list, named here rather
//       than left to be rediscovered. A consumer that ever reads it owes the walk, and this is
//       where the walk goes.
//
//   Getaddrinfo (net_darwin.cs) is the package's FIFTH member of this class and is NOT taken here.
//       Its out-parameter is a `**Addrinfo` over a LINKED NATIVE CHAIN that libc allocates and
//       freeaddrinfo releases -- not a record in the caller's buffer -- and the converted Addrinfo
//       holds Canonname, Addr and Next as managed references, so publishing the address alone would
//       replace a contained nil with a fabricated-reference landmine. That is the darwin twin of the
//       windows DnsQuery exclusion; it wants the whole chain transcribed the way
//       zsyscall_windows_addrinfo_impl.cs transcribes ADDRINFOW, its consumer is `net` rather than
//       os/user, and it therefore lands as its own increment with its own probe.
//
//   readdir_r, the darwin corpus's other `**T` out-parameter, is already answered elsewhere
//       (os/darwin/dir_darwin_impl.cs). With it and Getaddrinfo named, the census of this shape on
//       darwin closes at six.
//
// A wrapper's absence from this file is NOT evidence that it is sound.
//
// THE DARWIN LAYOUTS, transcribed from <pwd.h> / <grp.h> and matching Go's own structs field for
// field (internal/syscall/unix/user_darwin.go), on the two LP64 targets darwin has:
//
//   struct passwd  72:  pw_name 0, pw_passwd 8, pw_uid 16, pw_gid 20, pw_change 24,
//                       pw_class 32, pw_gecos 40, pw_dir 48, pw_shell 56, pw_expire 64
//   struct group   32:  gr_name 0, gr_passwd 8, gr_gid 16, (four bytes of padding), gr_mem 24

using System;

// Hand-owned (no user_darwin_impl.go exists, so a reconvert never regenerates this file); the four
// declarations it replaces are registered in the converter's manualConversionFuncs under
// "internal/syscall/unix" scoped goosDarwin, which is what turns their generated bodies into
// placeholders.
[module: go.GoManualConversion]
// Native record mirrors and out-cells are built on the stack here and handed to libc by address, so
// the package's emitted .csproj must allow unsafe -- the marker is how a hand-own declares that.
[module: go.GoRequiresUnsafe]

namespace go.@internal.syscall;

using abi = go.@internal.abi_package;
using syscall = syscall_package;
using System.Runtime.InteropServices;

partial class unix_package
{
    // struct passwd exactly as darwin lays it out: 72 bytes, every pointer RAW. The converted
    // Passwd holds six `ж<byte>` OBJECT REFERENCES and gets AUTO layout from the CLR besides, so
    // its address means nothing to libc -- that is arm two's measurement, and this mirror is the
    // remedy for it.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativePasswd
    {
        public byte* Name;                  //  0  char *pw_name
        public byte* Passwd;                //  8  char *pw_passwd
        public uint32 Uid;                  // 16  uid_t
        public uint32 Gid;                  // 20  gid_t
        public int64 Change;                // 24  time_t
        public byte* Class;                 // 32  char *pw_class
        public byte* Gecos;                 // 40  char *pw_gecos
        public byte* Dir;                   // 48  char *pw_dir
        public byte* Shell;                 // 56  char *pw_shell
        public int64 Expire;                // 64  time_t
    }

    // struct group: 32 bytes. The four bytes after gr_gid are C's own alignment padding before
    // gr_mem and they are REAL -- libc reads gr_mem at 24 -- so the field is spelled out rather than
    // left to Sequential, exactly as the darwin sockaddr twin spells Go's Pad_cgo_N.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeGroup
    {
        public byte* Name;                  //  0  char *gr_name
        public byte* Passwd;                //  8  char *gr_passwd
        public uint32 Gid;                  // 16  gid_t
        public uint32 Pad0;                 // 20  alignment before gr_mem
        public byte** Mem;                  // 24  char **gr_mem
    }

    // Lifts a NUL-terminated native C string into a managed, NUL-terminated array<byte>, so the
    // corpus's own decoder (unix.GoString -> runtime.gostring) reads it exactly as it reads any
    // converted `*byte`. Copying rather than publishing the address is decision (2) in this file's
    // header: it needs no inside-the-buffer fork, and what comes back does not depend on a pin
    // nobody holds once this call has returned.
    private static unsafe ж<byte> copyNativeString(byte* source) {
        if (source == null) {
            return default!;
        }

        nint length = 0;

        while (source[length] != 0) {
            length++;
        }

        var text = new array<byte>(length + 1);

        for (nint i = 0; i < length; i++) {
            text[i] = source[i];
        }

        return Ꮡ(text, 0);
    }

    // The record half of getpwnam_r / getpwuid_r. All ten fields are transcribed, not the five
    // os/user reads today: a partial fill is a silent wrong value, and the other five cost nothing.
    private static unsafe void publishPasswd(NativePasswd* record, ж<Passwd> Ꮡpwd, ж<ж<Passwd>> Ꮡresult) {
        if (Ꮡpwd == nil) {
            return;
        }

        ref var pwd = ref Ꮡpwd.Value;

        pwd.Name = copyNativeString(record->Name);
        pwd.ΔPasswd = copyNativeString(record->Passwd);
        pwd.Uid = record->Uid;
        pwd.Gid = record->Gid;
        pwd.Change = record->Change;
        pwd.Class = copyNativeString(record->Class);
        pwd.Gecos = copyNativeString(record->Gecos);
        pwd.Dir = copyNativeString(record->Dir);
        pwd.Shell = copyNativeString(record->Shell);
        pwd.Expire = record->Expire;

        // libc sets *result = pwd on success -- the caller's OWN pointer, which is what the "found"
        // test reads (`result != nil` in os/user's _C_getpwuid_r). ValueSlot, NOT Value: the
        // caller's box legitimately holds null on entry -- that is what an out-parameter IS -- and
        // Value's nil guard VALUE-PEEKS, so it would panic on the very write that fills the slot in.
        if (Ꮡresult != nil) {
            Ꮡresult.ValueSlot = Ꮡpwd;
        }
    }

    // The record half of getgrnam_r / getgrgid_r. Mem is left nil -- see the header.
    private static unsafe void publishGroup(NativeGroup* record, ж<Group> Ꮡgrp, ж<ж<Group>> Ꮡresult) {
        if (Ꮡgrp == nil) {
            return;
        }

        ref var grp = ref Ꮡgrp.Value;

        grp.Name = copyNativeString(record->Name);
        grp.Passwd = copyNativeString(record->Passwd);
        grp.Gid = record->Gid;

        if (Ꮡresult != nil) {
            Ꮡresult.ValueSlot = Ꮡgrp;
        }
    }

    // getpwnam_r(name, &record, buf, size, &cell). ONE axis differs from the emission: libc is
    // handed a native record and a native out-cell instead of two managed boxes. `name` and `buf`
    // stay the caller's own managed storage, pinned through their own boxes for the call.
    //
    // errno 0 with a NULL cell is POSIX for "no such user": the record is left untouched and the
    // caller's result stays nil, which is what Go's wrapper leaves behind for the same input.
    public static unsafe syscall.Errno Getpwnam(ж<byte> Ꮡname, ж<Passwd> Ꮡpwd, ж<byte> Ꮡbuf, uintptr size, ж<ж<Passwd>> Ꮡresult) {
        NativePasswd record = default;
        NativePasswd* cell = null;

        // Note: Returns an errno as its actual result, not in global errno.
        var (errno, _, _) = syscall_syscall6(abi.FuncPCABI0(libc_getpwnam_r_trampoline),
            (uintptr)Ꮡname,
            (uintptr)(void*)(&record),
            (uintptr)Ꮡbuf,
            size,
            (uintptr)(void*)(&cell),
            0);

        System.GC.KeepAlive(Ꮡname);
        System.GC.KeepAlive(Ꮡbuf);

        if (errno == 0 && cell != null) {
            publishPasswd(&record, Ꮡpwd, Ꮡresult);
        }

        return ((syscall.Errno)errno);
    }

    // getpwuid_r(uid, &record, buf, size, &cell) -- the member both probe arms measured.
    public static unsafe syscall.Errno Getpwuid(uint32 uid, ж<Passwd> Ꮡpwd, ж<byte> Ꮡbuf, uintptr size, ж<ж<Passwd>> Ꮡresult) {
        NativePasswd record = default;
        NativePasswd* cell = null;

        // Note: Returns an errno as its actual result, not in global errno.
        var (errno, _, _) = syscall_syscall6(abi.FuncPCABI0(libc_getpwuid_r_trampoline),
            (uintptr)uid,
            (uintptr)(void*)(&record),
            (uintptr)Ꮡbuf,
            size,
            (uintptr)(void*)(&cell),
            0);

        System.GC.KeepAlive(Ꮡbuf);

        if (errno == 0 && cell != null) {
            publishPasswd(&record, Ꮡpwd, Ꮡresult);
        }

        return ((syscall.Errno)errno);
    }

    // getgrnam_r(name, &record, buf, size, &cell).
    public static unsafe syscall.Errno Getgrnam(ж<byte> Ꮡname, ж<Group> Ꮡgrp, ж<byte> Ꮡbuf, uintptr size, ж<ж<Group>> Ꮡresult) {
        NativeGroup record = default;
        NativeGroup* cell = null;

        // Note: Returns an errno as its actual result, not in global errno.
        var (errno, _, _) = syscall_syscall6(abi.FuncPCABI0(libc_getgrnam_r_trampoline),
            (uintptr)Ꮡname,
            (uintptr)(void*)(&record),
            (uintptr)Ꮡbuf,
            size,
            (uintptr)(void*)(&cell),
            0);

        System.GC.KeepAlive(Ꮡname);
        System.GC.KeepAlive(Ꮡbuf);

        if (errno == 0 && cell != null) {
            publishGroup(&record, Ꮡgrp, Ꮡresult);
        }

        return ((syscall.Errno)errno);
    }

    // getgrgid_r(gid, &record, buf, size, &cell).
    public static unsafe syscall.Errno Getgrgid(uint32 gid, ж<Group> Ꮡgrp, ж<byte> Ꮡbuf, uintptr size, ж<ж<Group>> Ꮡresult) {
        NativeGroup record = default;
        NativeGroup* cell = null;

        // Note: Returns an errno as its actual result, not in global errno.
        var (errno, _, _) = syscall_syscall6(abi.FuncPCABI0(libc_getgrgid_r_trampoline),
            (uintptr)gid,
            (uintptr)(void*)(&record),
            (uintptr)Ꮡbuf,
            size,
            (uintptr)(void*)(&cell),
            0);

        System.GC.KeepAlive(Ꮡbuf);

        if (errno == 0 && cell != null) {
            publishGroup(&record, Ꮡgrp, Ꮡresult);
        }

        return ((syscall.Errno)errno);
    }
}
