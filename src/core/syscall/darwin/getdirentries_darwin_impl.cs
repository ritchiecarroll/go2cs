// getdirentries_darwin_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Getdirentries on darwin -- Go's fdopendir/readdir_r simulation (syscall_darwin.go), the syscall
// twin of os/darwin/dir_darwin_impl.cs, 2026-10-25.
//
// Go's body walks a DIR* with readdir_r(d, &entry, &entryp), and BOTH native arguments are
// unrepresentable in the conversion, for the reasons os's hand-own states:
//
//   1. `entry` is a syscall.Dirent, whose `Name [1024]int8` and `Pad_cgo_0 [3]byte` are golib
//      `array<T>` references, so the box is reference-bearing and its number is an ORDER TOKEN.
//      darwin's libc funnel does marshal it (a 1048-byte Go-layout copy), and that half worked.
//   2. `&entryp` is a `ж<ж<Dirent>>`: its pointee is itself a pointer, which the funnel's marshaller
//      cannot classify, so libc received the raw token. libSystem's readdir_r stores `*result =
//      entry` IN USERSPACE, a store to a non-canonical address, and the host died with SIGBUS at
//      syscall's TestDirent on both mac legs. Behind it, the funnel never decodes a pointer word
//      back, so entryp would have stayed nil, and the record copy `copy(buf, unsafe.Slice((*byte)
//      (unsafe.Pointer(&entry)), reclen))` panics by name on a reference-bearing struct.
//
// WHAT THIS FILE DOES INSTEAD. It keeps Go's protocol line for line -- the skip count stored in the
// fd's seek offset (issue #31368), openat(fd, ".") for a really new descriptor, fdopendir, the
// readdir_r loop with its EOF and skip rules, the "not enough room" stop, closedir on every exit and
// the final Seek -- and replaces only the buffer: ONE unmanaged block per call holds the `struct
// dirent` libc fills and the `dirent *` slot it publishes through. Each record's first reclen bytes
// are copied verbatim into buf, which is exactly what Go copies, so ParseDirent and the dirent
// accessors downstream stay auto and read the bytes at darwin's offsets as they do in Go.
//
// readdir_r is called through the package's own trampoline with raw addresses, the way
// sockaddr_darwin_impl.cs calls getsockname/accept/recvmsg; plain numbers carry no token tag, so the
// funnel passes them through untouched. Registered in the converter's manualConversionFuncs
// ("syscall": Getdirentries -> goosDarwin).

using System;
using System.Runtime.InteropServices;

// The aliases the converted neighbours declare for themselves -- a converted file's aliases are
// file-scoped, so a hand-owned companion restates the ones it uses.
using abi = go.@internal.abi_package;

// Hand-owned (no getdirentries_darwin_impl.go exists, so a reconvert never regenerates this file).
[module: go.GoManualConversion]
// The entry record and its out-slot live in an unmanaged block addressed by raw pointers.
[module: go.GoRequiresUnsafe]

namespace go;

partial class syscall_package
{
    // struct dirent under _DARWIN_FEATURE_64_BIT_INODE, which is what libSystem exports and what
    // Go's Dirent mirrors: d_ino 0, d_seekoff 8, d_reclen 16, d_namlen 18, d_type 20, d_name[1024]
    // 21, padded to 1048 (Go's Pad_cgo_0 [3]byte).
    private const int nativeDirentSize = 1048;
    private const int nativeDirentReclenOff = 16;

    public static unsafe (nint n, error err) Getdirentries(nint fd, slice<byte> buf, ж<uintptr> Ꮡbasep) {
        nint n = 0;

        // Simulate Getdirentries using fdopendir/readdir_r/closedir.
        // We store the number of entries to skip in the seek
        // offset of fd. See issue #31368.
        // It's not the full required semantics, but should handle the case
        // of calling Getdirentries or ReadDirent repeatedly.
        // It won't handle assigning the results of lseek to *basep, or handle
        // the directory being edited underfoot.
        var (skip, err) = Seek(fd, 0, 1 /* SEEK_CUR */);
        if (err != default!) {
            return (0, err);
        }

        // We need to duplicate the incoming file descriptor
        // because the caller expects to retain control of it, but
        // fdopendir expects to take control of its argument.
        // Just Dup'ing the file descriptor is not enough, as the
        // result shares underlying state. Use openat to make a really
        // new file descriptor referring to the same directory.
        (var fd2, err) = openat(fd, "."u8, O_RDONLY, 0);
        if (err != default!) {
            return (0, err);
        }
        (var d, err) = fdopendir(fd2);
        if (err != default!) {
            Close(fd2);
            return (0, err);
        }

        // ONE unmanaged block for the whole walk: the entry record libc fills, then the `dirent *`
        // slot it publishes through. Freed with the DIR* on every exit.
        byte* block = (byte*)NativeMemory.Alloc((nuint)(nativeDirentSize + sizeof(nint)));
        try {
            byte* entry = block;
            nint* entryp = (nint*)(block + nativeDirentSize);

            int64 cnt = 0;
            while (true) {
                *entryp = 0;
                var (r0, _, _) = syscall(abi.FuncPCABI0(libc_readdir_r_trampoline), d, (uintptr)(void*)entry, (uintptr)(void*)entryp);
                var e = (Errno)r0;
                if (e != 0) {
                    return (n, errnoErr(e));
                }
                if (*entryp == 0) {
                    break;
                }
                if (skip > 0) {
                    skip--;
                    cnt++;
                    continue;
                }
                nint reclen = (nint)(*(uint16*)(entry + nativeDirentReclenOff));
                if (reclen > len(buf)) {
                    // Not enough room. Return for now.
                    // The counter will let us know where we should start up again.
                    // Note: this strategy for suspending in the middle and
                    // restarting is O(n^2) in the length of the directory. Oh well.
                    break;
                }
                // Copy entry into return buffer.
                new ReadOnlySpan<byte>(entry, (int)reclen).CopyTo(buf.ToSpan());
                buf = buf.slice(reclen);
                n += reclen;
                cnt++;
            }
            // Set the seek offset of the input fd to record
            // how many files we've already returned.
            (_, err) = Seek(fd, cnt, 0 /* SEEK_SET */);
            if (err != default!) {
                return (n, err);
            }

            return (n, default!);
        } finally {
            closedir(d);
            NativeMemory.Free(block);
        }
    }
}
