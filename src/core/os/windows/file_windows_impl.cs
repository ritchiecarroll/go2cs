// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
using go;

// Hand-finished conversion (Phase 4 — os operational on Windows).
//
// Go's readReparseLink asks DeviceIoControl to fill a byte slice with a REPARSE_DATA_BUFFER and then
// REINTERPRETS those bytes as Go structs:
//
//	rdb := (*windows.REPARSE_DATA_BUFFER)(unsafe.Pointer(&rdbbuf[0]))
//	rb  := (*windows.MountPointReparseBuffer)(unsafe.Pointer(&rdb.DUMMYUNIONNAME))
//	return rb.Path()   // (*[0xffff]uint16)(unsafe.Pointer(&rb.PathBuffer[0]))[n1:n2]
//
// Both reparse-buffer structs end in `PathBuffer [1]uint16` — a Go inline array standing in for the
// variable-length name the OS actually wrote after it. In the conversion that field is a golib
// `array<uint16>`, an 8-byte MANAGED REFERENCE where the OS put 2+ bytes of inline UTF-16, so the
// managed layout does not describe the bytes at all. The reinterpret cannot alias managed storage
// (a struct carrying a reference is exactly the fabrication case golib's
// PointerExtensions.ReinterpretAliasesStorage refuses), so it falls back to the raw-address route
// and every read of PathBuffer resolves an object reference synthesized out of path bytes:
// `&rb.PathBuffer[0]` faulted with an ACCESS_VIOLATION inside `array<uint16>.get_Item`, which KILLED
// the whole C# test host mid-run at os's TestReadlink and truncated every verdict after it.
//
// No converter or golib change can rescue this: a managed array reference can never be laid out like
// an inline OS array. It is the same "raw metal on non-native types" arm of the conversion fork that
// dir_windows_impl.cs already owns for FILE_ID_BOTH_DIR_INFO (see
// src/archived/Baseline-vs-FullConversion.md), and it takes the same remedy — decode the OS record straight
// out of the byte slice at its documented offsets and never materialize a managed struct over it.
// Every read below is an ordinary bounds-checked managed read of `rdbbuf`; there is no pointer, no
// pinning and no unsafe block in this file.
//
// Field offsets are taken from the GO declarations in internal/syscall/windows/reparse_windows.go,
// which match the native MS-FSC layout the kernel writes:
//
//	REPARSE_DATA_BUFFER            SymbolicLinkReparseBuffer     MountPointReparseBuffer
//	  0  ReparseTag        uint32    +0  SubstituteNameOffset      +0  SubstituteNameOffset
//	  4  ReparseDataLength uint16    +2  SubstituteNameLength      +2  SubstituteNameLength
//	  6  Reserved          uint16    +4  PrintNameOffset           +4  PrintNameOffset
//	  8  DUMMYUNIONNAME    …         +6  PrintNameLength           +6  PrintNameLength
//	                                 +8  Flags       uint32        +8  PathBuffer [...]uint16
//	                                +12  PathBuffer [...]uint16
//
// SubstituteNameOffset/Length are BYTE counts measured from the start of PathBuffer.
//
// The converter skips the auto forms of readReparseLink AND readReparseLinkHandle via the
// manualConversionFuncs registry (go2cs/manualTypeOperations.go); the module marker below makes
// go2cs skip re-converting this file wholesale. Note what does NOT need this: openSymlink and
// normaliseLinkPath pass scalars, handles and strings, so their conversions are faithful and this
// file calls them.
//
// TWO ENTRIES BECAUSE 1.24 SPLIT THE FUNCTION. Until 1.24 the decode lived inside readReparseLink
// and one entry covered it. 1.24 moved the body into readReparseLinkHandle and left readReparseLink
// as an open-and-delegate wrapper, so the single entry stopped covering the defect -- and os.Root,
// added in the same release, calls readReparseLinkHandle directly. Both are registered here.
//
// ⚠ syscall.Readlink (syscall/syscall_windows.go) has the SAME defect over its own private
// reparseDataBuffer/symbolicLinkReparseBuffer/mountPointReparseBuffer copies. It is LATENT — os does
// not use it, and nothing in the validated corpus reaches it — and is recorded as such rather than
// fixed speculatively (docs/phase4/BOARD-next-validation-candidates.md).

[module: GoManualConversion]

namespace go;

using syscall = syscall_package;
using windows = @internal.syscall.windows_package;

partial class os_package
{
    // Byte offsets of the reparse-record fields this file reads. See the layout table above.
    private const int reparseTagOff = 0;
    private const int reparseDataOff = 8;
    private const int reparseSubstituteNameOffsetOff = 0;
    private const int reparseSubstituteNameLengthOff = 2;
    private const int symbolicLinkFlagsOff = 8;
    private const int symbolicLinkPathBufferOff = 12;
    private const int mountPointPathBufferOff = 8;

    // Reads one little-endian uint16/uint32 field out of the OS-filled buffer. Both go through the
    // bounds-checked slice indexer, so a short or truncated record surfaces as an ordinary index
    // panic rather than reading past the data DeviceIoControl wrote.
    private static uint16 readReparseUint16(slice<byte> buf, nint at) {
        return (uint16)((uint32)buf[at] | ((uint32)buf[at + 1] << 8));
    }

    private static uint32 readReparseUint32(slice<byte> buf, nint at) {
        return (uint32)((uint32)buf[at] | ((uint32)buf[at + 1] << 8) | ((uint32)buf[at + 2] << 16) | ((uint32)buf[at + 3] << 24));
    }

    // reparseSubstituteName is Go's `rb.Path()` for either buffer shape: the substitute name is
    // SubstituteNameLength bytes of UTF-16 starting SubstituteNameOffset bytes into PathBuffer.
    // Decoding through syscall.UTF16ToString keeps Go's exact string semantics, NUL truncation
    // included.
    private static @string reparseSubstituteName(slice<byte> buf, nint bufferOff, int pathBufferOff) {
        nint nameOff = bufferOff + pathBufferOff + readReparseUint16(buf, bufferOff + reparseSubstituteNameOffsetOff);
        nint runes = (nint)(readReparseUint16(buf, bufferOff + reparseSubstituteNameLengthOff) / 2);

        slice<uint16> name = new slice<uint16>(runes);

        for (nint i = 0; i < runes; i++) {
            name[i] = readReparseUint16(buf, nameOff + i * 2);
        }

        return syscall.UTF16ToString(name);
    }

    internal static (@string, error) readReparseLink(@string path) {
        var (h, err) = openSymlink(path);
        if (err != default!) {
            return ("", err);
        }
        try {
            return readReparseLinkHandle(h);
        } finally {
            syscall.CloseHandle(h);
        }
    }

    // readReparseLinkHandle is the decode, on a handle the CALLER owns. Go split it out of
    // readReparseLink at 1.24 and it is hand-owned for exactly the reason readReparseLink was: the
    // body IS the reinterpret. Nothing about the defect moved -- only its address did, and it
    // acquired two new callers with it.
    //
    // ⚠ IT IS NOT REACHED ONLY THROUGH readReparseLink. The same release added os.Root, and
    // root_windows.go calls this function DIRECTLY -- readReparseLinkAt, and the lstat branch of
    // rootStat -- so an os.Root symlink read takes the fault with no readReparseLink anywhere
    // on the stack. That is why the row is live at 1.24 rather than latent: leaving the auto form
    // in place would keep a working readlink beside an os.Root that kills the host.
    //
    // The handle is NOT closed here. readReparseLink closes what readReparseLink opened; the
    // root_windows callers close what they opened. Go's ownership, kept exactly -- and the reason
    // the eager finally stays up there rather than moving down with the body.
    internal static (@string, error) readReparseLinkHandle(syscall.Handle h) {
        var rdbbuf = new slice<byte>(syscall.MAXIMUM_REPARSE_DATA_BUFFER_SIZE);
        ref var bytesReturned = ref heap(new uint32(), out var ᏑbytesReturned);
        var err = syscall.DeviceIoControl(h, syscall.FSCTL_GET_REPARSE_POINT, nil, 0, Ꮡ(rdbbuf, 0), (uint32)len(rdbbuf), ᏑbytesReturned, nil);
        if (err != default!) {
            return ("", err);
        }

        uint32 reparseTag = readReparseUint32(rdbbuf, reparseTagOff);

        if (reparseTag == syscall.IO_REPARSE_TAG_SYMLINK) {
            @string s = reparseSubstituteName(rdbbuf, reparseDataOff, symbolicLinkPathBufferOff);
            uint32 flags = readReparseUint32(rdbbuf, reparseDataOff + symbolicLinkFlagsOff);
            if ((uint32)(flags & (uint32)windows.SYMLINK_FLAG_RELATIVE) != 0) {
                return (s, default!);
            }
            return normaliseLinkPath(s);
        }

        if (reparseTag == windows.IO_REPARSE_TAG_MOUNT_POINT) {
            return normaliseLinkPath(reparseSubstituteName(rdbbuf, reparseDataOff, mountPointPathBufferOff));
        }

        // The path is not a symlink or junction but another type of reparse point.
        return ("", syscall.ENOENT);
    }
}
