// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

using syscall = syscall_package;

partial class windows_package {

public static syscall.Errno ERROR_INVALID_PARAMETER => 87;
public static UntypedInt FILE_SUPPORTS_OBJECT_IDS => 0x00010000;
public static UntypedInt FILE_SUPPORTS_OPEN_BY_FILE_ID => 0x01000000;
public static UntypedInt SYMBOLIC_LINK_FLAG_ALLOW_UNPRIVILEGED_CREATE => 0x2;
public static UntypedInt FileBasicInfo => 0; // FILE_BASIC_INFO
public static UntypedInt FileStandardInfo => 1; // FILE_STANDARD_INFO
public static UntypedInt FileNameInfo => 2; // FILE_NAME_INFO
public static UntypedInt FileStreamInfo => 7; // FILE_STREAM_INFO
public static UntypedInt FileCompressionInfo => 8; // FILE_COMPRESSION_INFO
public static UntypedInt FileAttributeTagInfo => 9; // FILE_ATTRIBUTE_TAG_INFO
public static UntypedInt FileIdBothDirectoryInfo => 0xa; // FILE_ID_BOTH_DIR_INFO
public static UntypedInt FileIdBothDirectoryRestartInfo => 0xb; // FILE_ID_BOTH_DIR_INFO
public static UntypedInt FileRemoteProtocolInfo => 0xd; // FILE_REMOTE_PROTOCOL_INFO
public static UntypedInt FileFullDirectoryInfo => 0xe; // FILE_FULL_DIR_INFO
public static UntypedInt FileFullDirectoryRestartInfo => 0xf; // FILE_FULL_DIR_INFO
public static UntypedInt FileStorageInfo => 0x10; // FILE_STORAGE_INFO
public static UntypedInt FileAlignmentInfo => 0x11; // FILE_ALIGNMENT_INFO
public static UntypedInt FileIdInfo => 0x12; // FILE_ID_INFO
public static UntypedInt FileIdExtdDirectoryInfo => 0x13; // FILE_ID_EXTD_DIR_INFO
public static UntypedInt FileIdExtdDirectoryRestartInfo => 0x14; // FILE_ID_EXTD_DIR_INFO

partial struct FILE_ATTRIBUTE_TAG_INFO {
    public uint32 FileAttributes;
    public uint32 ReparseTag;
}

//sys	GetFileInformationByHandleEx(handle syscall.Handle, class uint32, info *byte, bufsize uint32) (err error)

} // end windows_package
