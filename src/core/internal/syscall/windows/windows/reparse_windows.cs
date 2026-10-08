// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class windows_package {

// Reparse tag values are taken from
// https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-fscc/c8e77b37-3909-4fe6-a4ea-2b9d423b1ee4
public static UntypedInt FSCTL_SET_REPARSE_POINT => 0x000900A4;

public static UntypedInt IO_REPARSE_TAG_MOUNT_POINT => 0xA0000003;

public static UntypedInt IO_REPARSE_TAG_DEDUP => 0x80000013;

public static UntypedInt IO_REPARSE_TAG_AF_UNIX => 0x80000023;

public static UntypedInt SYMLINK_FLAG_RELATIVE => 1;

// These structures are described
// in https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-fscc/ca069dad-ed16-42aa-b057-b6b207f447cc
// and https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-fscc/b41f1cbf-10df-4a47-98d4-1c52a833d913.
partial struct REPARSE_DATA_BUFFER {
    public uint32 ReparseTag;
    public uint16 ReparseDataLength;
    public uint16 Reserved;
    public byte DUMMYUNIONNAME;
}

// REPARSE_DATA_BUFFER_HEADER is a common part of REPARSE_DATA_BUFFER structure.
partial struct REPARSE_DATA_BUFFER_HEADER {
    public uint32 ReparseTag;
    // The size, in bytes, of the reparse data that follows
    // the common portion of the REPARSE_DATA_BUFFER element.
    // This value is the length of the data starting at the
    // SubstituteNameOffset field.
    public uint16 ReparseDataLength;
    public uint16 Reserved;
}

partial struct SymbolicLinkReparseBuffer {
    // The integer that contains the offset, in bytes,
    // of the substitute name string in the PathBuffer array,
    // computed as an offset from byte 0 of PathBuffer. Note that
    // this offset must be divided by 2 to get the array index.
    public uint16 SubstituteNameOffset;
    // The integer that contains the length, in bytes, of the
    // substitute name string. If this string is null-terminated,
    // SubstituteNameLength does not include the Unicode null character.
    public uint16 SubstituteNameLength;
    // PrintNameOffset is similar to SubstituteNameOffset.
    public uint16 PrintNameOffset;
    // PrintNameLength is similar to SubstituteNameLength.
    public uint16 PrintNameLength;
    // Flags specifies whether the substitute name is a full path name or
    // a path name relative to the directory containing the symbolic link.
    public uint32 Flags;
    public array<uint16> PathBuffer = new(1);
}

// Path returns path stored in rb.
public static @string Path(this ref SymbolicLinkReparseBuffer rb) {
    var n1 = (uint16)(rb.SubstituteNameOffset / 2);
    var n2 = (uint16)((uint16)(rb.SubstituteNameOffset + rb.SubstituteNameLength) / 2);
    return syscall.UTF16ToString((~array<uint16>.AliasPointer(Ꮡ(rb.PathBuffer, 0), 65535)).slice(n1, n2, n2));
}

partial struct MountPointReparseBuffer {
    // The integer that contains the offset, in bytes,
    // of the substitute name string in the PathBuffer array,
    // computed as an offset from byte 0 of PathBuffer. Note that
    // this offset must be divided by 2 to get the array index.
    public uint16 SubstituteNameOffset;
    // The integer that contains the length, in bytes, of the
    // substitute name string. If this string is null-terminated,
    // SubstituteNameLength does not include the Unicode null character.
    public uint16 SubstituteNameLength;
    // PrintNameOffset is similar to SubstituteNameOffset.
    public uint16 PrintNameOffset;
    // PrintNameLength is similar to SubstituteNameLength.
    public uint16 PrintNameLength;
    public array<uint16> PathBuffer = new(1);
}

// Path returns path stored in rb.
public static @string Path(this ref MountPointReparseBuffer rb) {
    var n1 = (uint16)(rb.SubstituteNameOffset / 2);
    var n2 = (uint16)((uint16)(rb.SubstituteNameOffset + rb.SubstituteNameLength) / 2);
    return syscall.UTF16ToString((~array<uint16>.AliasPointer(Ꮡ(rb.PathBuffer, 0), 65535)).slice(n1, n2, n2));
}

} // end windows_package
