// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
using go;

// Hand-finished conversion (Phase 4 — os operational on darwin).
//
// Go's (*File).readdir on darwin walks a `DIR*` with readdir_r(3), and BOTH of its native
// arguments are unrepresentable in the conversion for reasons this corpus has already censused
// twice:
//
//	var dirent syscall.Dirent            // entry buffer the LIBC writes
//	var entptr *syscall.Dirent           // OUT-parameter libc sets to &dirent or NULL
//	readdir_r(d.dir, &dirent, &entptr)
//
//   1. `syscall.Dirent` is NON-BLITTABLE in the conversion: its inline `Name [1024]int8` and
//      `Pad_cgo_0 [3]byte` become golib `array<T>` OBJECT REFERENCES — 8 bytes each where the
//      kernel writes 1024 and 3 INLINE. The generated wrapper hands libc `(uintptr)Ꮡentry`
//      (zsyscall_darwin_amd64.cs), so libc writes a ~1,048-byte record over a ~48-byte managed
//      object: heap corruption past its end, raw bytes over a live GC reference in the Name
//      field, and every field after Namlen read from the wrong offset. This is the
//      struct-passing seam the board tracks (Timezoneinformation, win32finddata1,
//      ProcessEntry32, SiginfoChild) — the SAME class, arriving on darwin through its first
//      directory read.
//   2. `**Dirent` is the OUT-PARAMETER class beside it: `ж<T> → uintptr` answers 0 for a
//      heap-boxed pointer that is still nil, so libc receives a NULL slot to publish through,
//      and the EOF test (`entptr == nil`) can never observe anything else. Go's loop would
//      terminate immediately on every directory.
//
// Neither is fixable by the converter or golib — a managed array reference can never be laid out
// like an inline C array — so readdir is hand-owned here, exactly as it is on windows
// (windows/dir_windows_impl.cs, whose header states the same fork).
//
// WHAT THIS FILE DOES INSTEAD. It keeps Go's protocol and replaces only the unrepresentable
// buffer: one UNMANAGED block per call holds the `struct dirent` libc writes and the `dirent *`
// out-slot, and the entry's fields are decoded from that block at their documented darwin
// offsets. Nothing managed is ever handed to libc, and nothing native is ever reinterpreted as a
// managed struct.
//
//	struct dirent (darwin, _DARWIN_FEATURE_64_BIT_INODE — what libSystem exports and what
//	syscall.Dirent mirrors field-for-field):
//	   0  d_ino     uint64
//	   8  d_seekoff uint64
//	  16  d_reclen  uint16
//	  18  d_namlen  uint16
//	  20  d_type    uint8
//	  21  d_name    char[1024]      // NUL-terminated, d_namlen bytes of content
//
// The libc entry points are taken directly rather than through the generated trampolines, for
// the reason above: the trampolines' arguments are precisely the two unrepresentable shapes.
// readdir_r is resolved in libSystem.B.dylib by golib's import resolver, the library the
// `//go:cgo_import_dynamic` the generated file carries names, and under the same $INODE64 rule
// Go's linker applies (see readdir_r_native below). Only `readdir_r` and the size constant are
// needed: the DIR* itself still comes from
// internal/poll's OpenDir (Go's own source for it) and is still closed by the package's
// converted `dirInfo.close` → `closedir`, so this file owns exactly one step of the protocol.
//
// The converter drops the auto form of `readdir` for darwin via the manualConversionFuncs
// registry, leaving the placeholder comment in dir_darwin.cs; the module marker below makes a
// -stdlib reconvert skip this file wholesale (L3 routing: darwin-exclusive, so it lives in
// darwin/ only and no other GOOS folder receives it).

[module: GoManualConversion]

namespace go;

using System.Runtime.InteropServices;
using Δio = io_package;
using Δruntime = runtime_package;
using atomic = go.sync.atomic_package;
using poll = go.@internal.poll_package;
using fs = go.io.fs_package;
using syscall = syscall_package;
using go.@internal;
using go.sync;

partial class os_package {

// Byte offsets and sizes of darwin's `struct dirent`. See the layout table above.
private const int direntInoOff = 0;
private const int direntReclenOff = 16;
private const int direntNamlenOff = 18;
private const int direntTypeOff = 20;
private const int direntNameOff = 21;
private const int direntNameMax = 1024;
private const int direntSize = direntNameOff + direntNameMax;

// libc's readdir_r(3). The `entry` and `result` arguments are UNMANAGED addresses — that is the
// whole point of this file (see the header): libc writes the entry record and publishes the
// result pointer through them, and neither can be a managed object.
//
// It is bound through golib's import resolver, not a DllImport of "readdir_r": on macOS x86_64 the
// bare readdir_r is the LEGACY 32-bit-inode ABI, whose struct dirent is not the layout decoded
// above, and Go's linker binds readdir_r$INODE64 there instead. That rule lives ONCE, in
// GoCgoDynamicImports.LinkerSymbolName (S10, 2026-10-02); a bare DllImport read four empty,
// directory-typed names per directory on osx-x64 (run 36976772120). The resolved function is the
// same one the converted DIR* came from (the forwarded fdopendir resolves through the same rule).
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
private delegate int readdir_r_fn(nint dir, nint entry, nint result);

private static readdir_r_fn? s_readdir_r;

private static int readdir_r_native(nint dir, nint entry, nint result) {
    s_readdir_r ??= Marshal.GetDelegateForFunctionPointer<readdir_r_fn>(
        GoCgoDynamicImports.Resolve("readdir_r", "/usr/lib/libSystem.B.dylib"));

    return s_readdir_r(dir, entry, result);
}

// readdir is Go's dir_darwin.go (*File).readdir, protocol-for-protocol, over an unmanaged entry
// buffer. The dirInfo/OpenDir handshake, the size/n convention, the EINTR retry, the zero-inode
// skip, the "." / ".." filter, the three modes and the io.EOF rule are all Go's own.
[GoRecv] internal static (slice<@string> names, slice<DirEntry> dirents, slice<FileInfo> infos, error err) readdir(this ж<File> Ꮡfile, nint n, readdirMode mode) {
    slice<@string> names = default!;
    slice<DirEntry> dirents = default!;
    slice<FileInfo> infos = default!;

    ref var file = ref Ꮡfile.DerefOrNull();

    // If this file has no dirinfo, create one. The atomic field is reached through the corpus's
    // own field-pointer idiom (`Ꮡfile.of(File.Ꮡdirinfo)`), the same one file.cs uses for it.
    // Declared as the BASE `ж<dirInfo>`, not `var`. `Load()` below returns `ж<dirInfo>`, and
    // `StandardBox<T>` derives FROM `ж<T>` -- so inferring the derived type here makes that
    // assignment a base-to-derived conversion the compiler rejects (CS0266). This was `var d =
    // new ж<dirInfo>()` until the box-kind split (36b7e9d96) mechanically flipped every `new
    // ж<T>` to `new StandardBox<T>`, which is correct at the allocation and wrong at the
    // inference. Naming the type is also what docs/coding-style.md asks for.
    ж<dirInfo> d = new StandardBox<dirInfo>(nil);
    while (ᐧ) {
        d = Ꮡfile.of(File.Ꮡdirinfo).Load();
        if (d != nil) {
            break;
        }
        (var dir, var call, var errno) = Ꮡfile.of(File.Ꮡpfd).OpenDir();
        if (errno != default!) {
            return (default!, default!, default!, new fs.PathErrorжerror(Ꮡ(new PathError(Op: call, Path: file.name, Err: errno))));
        }
        d = new StandardBox<dirInfo>(new dirInfo(dir: dir));
        if (Ꮡfile.of(File.Ꮡdirinfo).CompareAndSwap(default!, d)) {
            break;
        }
        // We lost the race: try again.
        d.close();
    }

    nint size = n;
    if (size <= 0) {
        size = 100;
        n = -1;
    }

    // ONE unmanaged block per call, freed in the finally: the entry record libc fills, followed
    // by the `dirent *` out-slot it publishes through. A single allocation keeps the lifetime
    // rule trivial — nothing here outlives the call, and nothing managed is exposed to libc.
    nint block = Marshal.AllocHGlobal(direntSize + nint.Size);
    try {
        nint entry = block;
        nint resultSlot = block + direntSize;

        while (len(names) + len(dirents) + len(infos) < size || n == -1) {
            Marshal.WriteIntPtr(resultSlot, 0);

            int rc = readdir_r_native((nint)(nuint)(~d).dir, entry, resultSlot);
            if (rc != 0) {
                var errno = ((syscall.Errno)(uintptr)rc);
                if (errno == syscall.EINTR) {
                    continue;
                }
                return (names, dirents, infos, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "readdir"u8, Path: file.name, Err: errno))));
            }

            // EOF: libc publishes NULL rather than the entry's address.
            if (Marshal.ReadIntPtr(resultSlot) == 0) {
                break;
            }

            // Darwin may return a zero inode when a directory entry has been deleted but not yet
            // removed from the directory; getdirentries(2) makes skipping those the caller's job.
            if (ReadUInt64(entry + direntInoOff) == 0) {
                continue;
            }

            @string name = ReadEntryName(entry);

            // Check for useless names before allocating anything further.
            if (name == "."u8 || name == ".."u8) {
                continue;
            }

            if (mode == readdirName) {
                names = append(names, name);
            }
            else if (mode == readdirDirEntry) {
                (var de, var err) = newUnixDirent(file.name, name, dtToType(Marshal.ReadByte(entry + direntTypeOff)));
                if (IsNotExist(err)) {
                    // File disappeared between readdir and stat. Treat as if it didn't exist.
                    continue;
                }
                if (err != default!) {
                    return (default!, dirents, default!, err);
                }
                dirents = append(dirents, de);
            }
            else {
                (var info, var err) = lstat(file.name + "/"u8 + name);
                if (IsNotExist(err)) {
                    // File disappeared between readdir + stat. Treat as if it didn't exist.
                    continue;
                }
                if (err != default!) {
                    return (default!, default!, infos, err);
                }
                infos = append(infos, info);
            }

            Δruntime.KeepAlive(Ꮡfile);
        }
    }
    finally {
        Marshal.FreeHGlobal(block);
    }

    if (n > 0 && len(names) + len(dirents) + len(infos) == 0) {
        return (default!, default!, default!, Δio.EOF);
    }

    return (names, dirents, infos, default!);
}

// ReadUInt64 reads one little-endian 64-bit field out of the unmanaged entry record. Darwin on
// both amd64 and arm64 is little-endian, matching the field order the layout table documents.
private static uint64 ReadUInt64(nint at) {
    return (uint64)(uint32)Marshal.ReadInt32(at) | ((uint64)(uint32)Marshal.ReadInt32(at + 4) << 32);
}

// ReadEntryName decodes the entry's `d_name` — Go reads the inline array and truncates at the
// first NUL, which is what d_namlen already reports; the NUL scan is kept as the authority
// because Go's own loop trusts it over the length field, and both are bounded by d_name's size.
private static @string ReadEntryName(nint entry) {
    nint length = (nint)(uint16)Marshal.ReadInt16(entry + direntNamlenOff);
    if (length > direntNameMax) {
        length = direntNameMax;
    }

    nint at = entry + direntNameOff;
    slice<byte> name = new slice<byte>(length);
    for (nint i = 0; i < length; i++) {
        byte c = Marshal.ReadByte(at + i);
        if (c == 0) {
            return ((@string)name[..(int)i]);
        }
        name[i] = c;
    }

    return ((@string)name);
}

} // end os_package
