// zsyscall_windows_netdb_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.
//
// Parts of this file are inspired by Go, whose notice they carry (license of record: LICENSES/BSD-3-Clause.txt):
//
// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//
// PROVENANCE, part by part:
// - _GetHostByName, _GetProtoByName, _GetServByName: INSPIRED BY src/syscall/zsyscall_windows.go (generated from the
//   declarations in src/syscall/syscall_windows.go, whose notice this file carries), Go 1.26.11, funcs
//   _GetHostByName, _GetProtoByName, _GetServByName. The native database entry is copied into a managed record.
// - assertNetdbMirrors and the copy and transcribe helpers: original go2cs code.

// THE NET-DATABASE FAMILY -- gethostbyname, getprotobyname and getservbyname -- the three unclosed
// siblings of the name-resolution arc this directory's zsyscall_windows_addrinfo_impl.cs closed.
//
// THE CLASS, stated once more only where it differs. The addrinfo twin's defect is in BOTH
// directions: hints the kernel READS and a result it WRITES. These three are read-only in one
// direction and worse for it, because the defect is in the RETURN VALUE and the generated wrapper
// spells it in a single line:
//
//     h = (ж<Hostent>)(uintptr)((@unsafe.Pointer)r0);        // zsyscall_windows.cs, _GetHostByName
//
// `r0` is ws2_32's `hostent*`. That cast reinterprets a raw native address as a MANAGED BOX over a
// record whose Name is `ж<byte>`, whose Aliases and AddrList are `ж<ж<byte>>` -- managed references
// where native `struct hostent` has raw `char*` and `char**`. Nothing faults at the cast; the
// fabrication happens at the first READ, which materializes the whole record.
//
// AND THE ONE CONSUMER READS EXACTLY ONE INTEGER, which is what makes this the row-46 shape rather
// than the addrinfo one. net's getprotobyname (net/windows/lookup_windows.cs:84) does
// `(~p).Proto` -- a uint16 -- and `~p` materializes the WHOLE Protoent on the way to it, fabricating
// Name and Aliases as managed references from ws2_32's bytes. A field that looks exempt is not; that
// is the note os/user's lookup_windows_impl.cs carries about PrimaryGroupID, in a second costume.
//
// THE LIFETIME, which is why transcription is not merely tidier here. ws2_32 returns these records
// from THREAD-LOCAL STORAGE, and net's own source says so at lookup_windows.go -- the comment
// survives conversion at lookup_windows.cs:82 ("GetProtoByName return value is stored in thread
// local storage. Start new os thread before the call to prevent races."). A managed box aliasing
// that storage is stale the moment anything else on the thread calls into the resolver. Copying on
// arrival is what makes the returned record a value the caller owns, and it removes the reason
// net's lookupProtocol has to spawn an OS thread around the call -- though it does NOT remove that
// spawn, which is upstream's code and stays as Go wrote it.
//
// THE REMEDY, the same as the twin's: blittable [StructLayout(Sequential)] mirrors, the call through
// the package's own LazyProc unchanged, and an explicit copy at the boundary into records the
// caller owns. Each transcription is exposed as its own seam so a guard can drive it from a
// hand-built image with no network, no resolver and no host database.
//
// THE FIELD ORDER IS GO'S, READ AT THE PIN, NOT RECALLED. syscall/types_windows.go declares
// Hostent {Name, Aliases, AddrType, Length, AddrList} and Protoent {Name, Aliases, Proto};
// syscall/types_windows_amd64.go declares Servent {Name, Aliases, Proto, Port} -- Proto BEFORE
// Port, which is why Servent has an arch-specific file at all. The mirrors reproduce that order,
// and their sizes are ASSERTED before use.
//
// ⚠ ALIASES AND ADDRLIST ARE THE FAITHFUL-COPY ARM, NOT A MEASURED PATH -- the same status
// copyNativeCanonname carries in the twin. No consumer in the corpus reads them today: the only
// call site is net's getprotobyname, which reads Proto alone. They are transcribed anyway, because
// the whole record is materialized by any read and a half-transcribed record is the fabrication
// this file exists to remove.

using System;
using System.Runtime.InteropServices;

// Hand-owned (no zsyscall_windows_netdb_impl.go exists, so a reconvert never regenerates this
// file). The three declarations it replaces are registered in the converter's manualConversionFuncs.
[module: go.GoManualConversion]

// The mirrors and the native runs read through them are pointer work.
[module: go.GoRequiresUnsafe]

namespace go;

partial class syscall_package
{
    // The native sizes on x64, from the field order Go declares and the platform's own alignment:
    // hostent 8 + 8 + 2 + 2 + (4 pad) + 8 = 32 · protoent 8 + 8 + 2 + (6 pad) = 24 ·
    // servent 8 + 8 + 8 + 2 + (6 pad) = 32. Asserted rather than trusted -- the failure this file
    // exists to prevent is a silent offset, and an assertion is the only thing a lane without a
    // host can put between arithmetic and a kernel read.
    private const int NativeHostentSize = 32;
    private const int NativeProtoentSize = 24;
    private const int NativeServentSize = 32;

    /// <summary><c>struct hostent</c> as ws2_32 lays it out: two raw pointers, two shorts, a raw pointer.</summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeHostent
    {
        public nuint Name;
        public nuint Aliases;
        public uint16 AddrType;
        public uint16 Length;
        public nuint AddrList;
    }

    /// <summary><c>struct protoent</c> as ws2_32 lays it out.</summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeProtoent
    {
        public nuint Name;
        public nuint Aliases;
        public uint16 Proto;
    }

    /// <summary><c>struct servent</c> as ws2_32 lays it out on x64 -- Proto BEFORE Port.</summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeServent
    {
        public nuint Name;
        public nuint Aliases;
        public nuint Proto;
        public uint16 Port;
    }

    // Refuses before any read if a mirror does not have the size the platform gives its native
    // counterpart. One helper for all three so the three wrappers cannot drift apart on it.
    private static unsafe void assertNetdbMirrors()
    {
        if (sizeof(NativeHostent) != NativeHostentSize ||
            sizeof(NativeProtoent) != NativeProtoentSize ||
            sizeof(NativeServent) != NativeServentSize)
        {
            throw new InvalidOperationException(
                $"syscall: the ws2_32 netdb mirrors are {sizeof(NativeHostent)} / " +
                $"{sizeof(NativeProtoent)} / {sizeof(NativeServent)} bytes where ws2_32 writes " +
                $"{NativeHostentSize} / {NativeProtoentSize} / {NativeServentSize} -- every field " +
                "past the first would come from the wrong offset.");
        }
    }

    // Copies a NUL-terminated native C string into managed storage and answers a pointer to it, so
    // a caller reading it reads memory it owns rather than ws2_32's thread-local buffer. The
    // terminator is kept, because every reader of a *byte name in Go stops at one. The byte twin of
    // the addrinfo companion's copyNativeCanonname.
    //
    // Split in two so the COPY is drivable on its own: copyNativeCStringBytes answers the managed
    // array, which a guard can index directly, and this wrapper builds the pointer production hands
    // back. A guard that had to walk a `ж<byte>` to check the copy would be asserting golib's
    // pointer-view machinery as much as this file's, which is not what it is for.
    public static unsafe array<byte> copyNativeCStringBytes(byte* source)
    {
        if (source == null)
        {
            return default!;
        }

        nint length = 0;

        while (source[length] != 0)
        {
            length++;
        }

        var text = new array<byte>(length + 1);

        for (nint i = 0; i < length; i++)
        {
            text[i] = source[i];
        }

        return text;
    }

    private static unsafe ж<byte> copyNativeCString(byte* source)
    {
        // The NULL test is on the POINTER, not on the returned array's Length: a `default`
        // array<byte> has no backing, and asking it for a Length is the kind of question this file
        // has no business relying on an answer to.
        if (source == null)
        {
            return default!;
        }

        return Ꮡ(copyNativeCStringBytes(source), 0);
    }

    // Copies a NULL-terminated native `char**` -- an alias list -- into a managed array of copied
    // strings, and answers a pointer to it. The managed array keeps the NIL TERMINATOR the native
    // one has, because a caller walks it until the entry is nil exactly as it would walk the native
    // run.
    private static unsafe ж<ж<byte>> copyNativeCStringList(byte** source)
    {
        if (source == null)
        {
            return default!;
        }

        nint count = 0;

        while (source[count] != null)
        {
            count++;
        }

        var list = new array<ж<byte>>(count + 1);

        for (nint i = 0; i < count; i++)
        {
            list[i] = copyNativeCString(source[i]);
        }

        return Ꮡ(list, 0);
    }

    // Copies hostent's h_addr_list: a NULL-terminated `char**` whose entries are NOT strings but
    // BINARY addresses of h_length bytes each.
    //
    // ⚠ THIS IS WHY IT IS NOT copyNativeCStringList. An IPv4 address contains zero bytes as data --
    // 10.0.0.1 has three of them -- so copying an entry as a C string would truncate at the first
    // one and hand back a two-byte "address". The length comes from the record, which is what
    // ws2_32 fills it for.
    public static unsafe array<array<byte>> copyNativeAddrListBytes(byte** source, nint length)
    {
        if (source == null || length <= 0)
        {
            return default!;
        }

        nint count = 0;

        while (source[count] != null)
        {
            count++;
        }

        var list = new array<array<byte>>(count);

        for (nint i = 0; i < count; i++)
        {
            var addr = new array<byte>(length);

            for (nint j = 0; j < length; j++)
            {
                addr[j] = source[i][j];
            }

            list[i] = addr;
        }

        return list;
    }

    private static unsafe ж<ж<byte>> copyNativeAddrList(byte** source, nint length)
    {
        // Same reason as copyNativeCString's: the guard is the POINTER and the length, both of
        // which are the caller's, rather than a property of an array that may not exist.
        if (source == null || length <= 0)
        {
            return default!;
        }

        var addresses = copyNativeAddrListBytes(source, length);
        nint count = addresses.Length;

        var list = new array<ж<byte>>(count + 1);

        for (nint i = 0; i < count; i++)
        {
            list[i] = Ꮡ(addresses[i], 0);
        }

        return Ꮡ(list, 0);
    }

    /// <summary>Transcribes ws2_32's <c>hostent</c> at <paramref name="address"/> into a record the caller owns.</summary>
    /// <remarks>Public for the guard in GolibTests; it widens no Go surface, since Go has no such function.</remarks>
    public static unsafe ж<Hostent> transcribeHostent(nuint address)
    {
        assertNetdbMirrors();

        NativeHostent* native = (NativeHostent*)address;

        if (native == null)
        {
            return default!;
        }

        return Ꮡ(new Hostent(
            Name: copyNativeCString((byte*)native->Name),
            Aliases: copyNativeCStringList((byte**)native->Aliases),
            AddrType: native->AddrType,
            Length: native->Length,
            AddrList: copyNativeAddrList((byte**)native->AddrList, (nint)native->Length)
        ));
    }

    /// <summary>Transcribes ws2_32's <c>protoent</c> at <paramref name="address"/> into a record the caller owns.</summary>
    /// <remarks>
    /// The one net consumer reads Proto alone, and `~p` materializes the whole record to reach it --
    /// which is why Name and Aliases are transcribed here rather than left to fabricate.
    /// </remarks>
    public static unsafe ж<Protoent> transcribeProtoent(nuint address)
    {
        assertNetdbMirrors();

        NativeProtoent* native = (NativeProtoent*)address;

        if (native == null)
        {
            return default!;
        }

        return Ꮡ(new Protoent(
            Name: copyNativeCString((byte*)native->Name),
            Aliases: copyNativeCStringList((byte**)native->Aliases),
            Proto: native->Proto
        ));
    }

    /// <summary>Transcribes ws2_32's <c>servent</c> at <paramref name="address"/> into a record the caller owns.</summary>
    public static unsafe ж<Servent> transcribeServent(nuint address)
    {
        assertNetdbMirrors();

        NativeServent* native = (NativeServent*)address;

        if (native == null)
        {
            return default!;
        }

        return Ꮡ(new Servent(
            Name: copyNativeCString((byte*)native->Name),
            Aliases: copyNativeCStringList((byte**)native->Aliases),
            Proto: copyNativeCString((byte*)native->Proto),
            Port: native->Port
        ));
    }

    // The three wrappers. Each keeps the generated body's call -- same LazyProc, same Syscall arity
    // and argument order, same KeepAlive, same errnoErr on failure -- and replaces ONLY the line
    // that reinterpreted the returned address as a managed record.
    //
    // The failure test moves from `h == nil` after the cast to `r0 == 0` before the transcription.
    // It is the same test: these three carry mkwinsyscall's `[failretval==nil]`, so a zero return
    // IS the failure, and the generated body reached the same branch by casting zero and comparing
    // the box against nil.

    internal static unsafe (ж<Hostent> h, error err) _GetHostByName(ж<byte> Ꮡname) {
        var ᴋname = Ꮡname;
        var (r0, _, e1) = Syscall(procgethostbyname.Addr(), 1, (uintptr)ᴋname, 0, 0);
        System.GC.KeepAlive(ᴋname);

        if (r0 == 0) {
            return (default!, errnoErr(e1));
        }

        return (transcribeHostent((nuint)r0), default!);
    }

    internal static unsafe (ж<Protoent> p, error err) _GetProtoByName(ж<byte> Ꮡname) {
        var ᴋname = Ꮡname;
        var (r0, _, e1) = Syscall(procgetprotobyname.Addr(), 1, (uintptr)ᴋname, 0, 0);
        System.GC.KeepAlive(ᴋname);

        if (r0 == 0) {
            return (default!, errnoErr(e1));
        }

        return (transcribeProtoent((nuint)r0), default!);
    }

    internal static unsafe (ж<Servent> s, error err) _GetServByName(ж<byte> Ꮡname, ж<byte> Ꮡproto) {
        var ᴋname = Ꮡname;
        var ᴋproto = Ꮡproto;
        var (r0, _, e1) = Syscall(procgetservbyname.Addr(), 2, (uintptr)ᴋname, (uintptr)ᴋproto, 0);
        System.GC.KeepAlive(ᴋname);
        System.GC.KeepAlive(ᴋproto);

        if (r0 == 0) {
            return (default!, errnoErr(e1));
        }

        return (transcribeServent((nuint)r0), default!);
    }
}
