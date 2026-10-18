// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
global using MemoryBasicInformation = go.@internal.runtime.syscall.windows_package.MemoryBasicInformation;
global using Context = go.@internal.runtime.syscall.windows_package.Context;

namespace go.@internal.syscall;

using Δwindows = go.@internal.runtime.syscall.windows_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using go.@internal.runtime.syscall;

partial class windows_package {

// Socket related.
public static UntypedInt TCP_KEEPIDLE => 0x03;
public static UntypedInt TCP_KEEPCNT => 0x10;
public static UntypedInt TCP_KEEPINTVL => 0x11;

public static UntypedInt SIO_UDP_NETRESET => /* syscall.IOC_IN | syscall.IOC_VENDOR | 15 */ 2550136847;

public static UntypedInt FILE_READ_DATA => 0x00000001;
public static UntypedInt FILE_READ_ATTRIBUTES => 0x00000080;
public static UntypedInt FILE_READ_EA => 0x00000008;
public static UntypedInt FILE_WRITE_DATA => 0x00000002;
public static UntypedInt FILE_WRITE_ATTRIBUTES => 0x00000100;
public static UntypedInt FILE_WRITE_EA => 0x00000010;
public static UntypedInt FILE_APPEND_DATA => 0x00000004;
public static UntypedInt FILE_EXECUTE => 0x00000020;

public static UntypedInt FILE_GENERIC_READ => /* STANDARD_RIGHTS_READ | FILE_READ_DATA | FILE_READ_ATTRIBUTES | FILE_READ_EA | SYNCHRONIZE */ 1179785;
public static UntypedInt FILE_GENERIC_WRITE => /* STANDARD_RIGHTS_WRITE | FILE_WRITE_DATA | FILE_WRITE_ATTRIBUTES | FILE_WRITE_EA | FILE_APPEND_DATA | SYNCHRONIZE */ 1179926;
public static UntypedInt FILE_GENERIC_EXECUTE => /* STANDARD_RIGHTS_EXECUTE | FILE_READ_ATTRIBUTES | FILE_EXECUTE | SYNCHRONIZE */ 1179808;

public static UntypedInt FILE_LIST_DIRECTORY => 0x00000001;
public static UntypedInt FILE_TRAVERSE => 0x00000020;

public static UntypedInt FILE_SHARE_READ => 0x00000001;
public static UntypedInt FILE_SHARE_WRITE => 0x00000002;
public static UntypedInt FILE_SHARE_DELETE => 0x00000004;
public static UntypedInt FILE_ATTRIBUTE_READONLY => 0x00000001;
public static UntypedInt FILE_ATTRIBUTE_HIDDEN => 0x00000002;
public static UntypedInt FILE_ATTRIBUTE_SYSTEM => 0x00000004;
public static UntypedInt FILE_ATTRIBUTE_DIRECTORY => 0x00000010;
public static UntypedInt FILE_ATTRIBUTE_ARCHIVE => 0x00000020;
public static UntypedInt FILE_ATTRIBUTE_DEVICE => 0x00000040;
public static UntypedInt FILE_ATTRIBUTE_NORMAL => 0x00000080;
public static UntypedInt FILE_ATTRIBUTE_TEMPORARY => 0x00000100;
public static UntypedInt FILE_ATTRIBUTE_SPARSE_FILE => 0x00000200;
public static UntypedInt FILE_ATTRIBUTE_REPARSE_POINT => 0x00000400;
public static UntypedInt FILE_ATTRIBUTE_COMPRESSED => 0x00000800;
public static UntypedInt FILE_ATTRIBUTE_OFFLINE => 0x00001000;
public static UntypedInt FILE_ATTRIBUTE_NOT_CONTENT_INDEXED => 0x00002000;
public static UntypedInt FILE_ATTRIBUTE_ENCRYPTED => 0x00004000;
public static UntypedInt FILE_ATTRIBUTE_INTEGRITY_STREAM => 0x00008000;
public static UntypedInt FILE_ATTRIBUTE_VIRTUAL => 0x00010000;
public static UntypedInt FILE_ATTRIBUTE_NO_SCRUB_DATA => 0x00020000;
public static UntypedInt FILE_ATTRIBUTE_RECALL_ON_OPEN => 0x00040000;
public static UntypedInt FILE_ATTRIBUTE_RECALL_ON_DATA_ACCESS => 0x00400000;

public static UntypedInt INVALID_FILE_ATTRIBUTES => 0xffffffff;

// https://learn.microsoft.com/en-us/windows/win32/secauthz/access-mask
partial struct ACCESS_MASK /*num:uint32*/;

// Constants for type ACCESS_MASK
public static UntypedInt DELETE => 0x00010000;
public static UntypedInt READ_CONTROL => 0x00020000;
public static UntypedInt WRITE_DAC => 0x00040000;
public static UntypedInt WRITE_OWNER => 0x00080000;
public static UntypedInt SYNCHRONIZE => 0x00100000;
public static UntypedInt STANDARD_RIGHTS_REQUIRED => 0x000F0000;
public static UntypedInt STANDARD_RIGHTS_READ => /* READ_CONTROL */ 131072;
public static UntypedInt STANDARD_RIGHTS_WRITE => /* READ_CONTROL */ 131072;
public static UntypedInt STANDARD_RIGHTS_EXECUTE => /* READ_CONTROL */ 131072;
public static UntypedInt STANDARD_RIGHTS_ALL => 0x001F0000;
public static UntypedInt SPECIFIC_RIGHTS_ALL => 0x0000FFFF;
public static UntypedInt ACCESS_SYSTEM_SECURITY => 0x01000000;
public static UntypedInt MAXIMUM_ALLOWED => 0x02000000;
public static UntypedInt GENERIC_READ => 0x80000000;
public static UntypedInt GENERIC_WRITE => 0x40000000;
public static UntypedInt GENERIC_EXECUTE => 0x20000000;
public static UntypedInt GENERIC_ALL => 0x10000000;

// https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/wdm/ns-wdm-_acl
partial struct ACL {
    public byte AclRevision;
    public byte Sbz1;
    public uint16 AclSize;
    public uint16 AceCount;
    public uint16 Sbz2;
}

// https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/wdm/ns-wdm-_io_status_block
partial struct IO_STATUS_BLOCK {
    public NTStatus Status;
    public uintptr Information;
}

// https://learn.microsoft.com/en-us/windows/win32/api/ntdef/ns-ntdef-_object_attributes
partial struct OBJECT_ATTRIBUTES {
    public uint32 Length;
    public syscallꓸHandle RootDirectory;
    public ж<NTUnicodeString> ObjectName;
    public uint32 Attributes;
    public ж<SECURITY_DESCRIPTOR> SecurityDescriptor;
    public ж<SECURITY_QUALITY_OF_SERVICE> SecurityQoS;
}

// init sets o's RootDirectory, ObjectName, and Length.
internal static error init(this ref OBJECT_ATTRIBUTES o, syscallꓸHandle root, @string name) {
    if (name == "."u8) {
        name = ""u8;
    }
    var (objectName, err) = NewNTUnicodeString(name);
    if (err != default!) {
        return err;
    }
    o.ObjectName = objectName;
    if (root != syscall.InvalidHandle) {
        o.RootDirectory = root;
    }
    o.Length = (uint32)/* unsafe.Sizeof(*o) */ (uintptr)48;
    return default!;
}

// Values for the Attributes member of OBJECT_ATTRIBUTES.
public static UntypedInt OBJ_INHERIT => 0x00000002;
public static UntypedInt OBJ_PERMANENT => 0x00000010;
public static UntypedInt OBJ_EXCLUSIVE => 0x00000020;
public static UntypedInt OBJ_CASE_INSENSITIVE => 0x00000040;
public static UntypedInt OBJ_OPENIF => 0x00000080;
public static UntypedInt OBJ_OPENLINK => 0x00000100;
public static UntypedInt OBJ_KERNEL_HANDLE => 0x00000200;
public static UntypedInt OBJ_FORCE_ACCESS_CHECK => 0x00000400;
public static UntypedInt OBJ_IGNORE_IMPERSONATED_DEVICEMAP => 0x00000800;
public static UntypedInt OBJ_DONT_REPARSE => 0x00001000;
public static UntypedInt OBJ_VALID_ATTRIBUTES => 0x00001FF2;

// https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/ntifs/ns-ntifs-_security_descriptor
partial struct SECURITY_DESCRIPTOR {
    internal byte revision;
    internal byte sbz1;
    internal SECURITY_DESCRIPTOR_CONTROL control;
    internal ж<syscall.SID> owner;
    internal ж<syscall.SID> group;
    internal ж<ACL> sacl;
    internal ж<ACL> dacl;
}

// https://learn.microsoft.com/en-us/windows-hardware/drivers/ifs/security-descriptor-control
partial struct SECURITY_DESCRIPTOR_CONTROL /*num:uint16*/;

// https://learn.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-security_quality_of_service
partial struct SECURITY_QUALITY_OF_SERVICE {
    public uint32 Length;
    public uint32 ImpersonationLevel; // type SECURITY_IMPERSONATION_LEVEL
    public byte ContextTrackingMode;   // type SECURITY_CONTEXT_TRACKING_MODE
    public byte EffectiveOnly;
}

// File flags for [os.OpenFile]. The O_ prefix is used to indicate
// that these flags are specific to the OpenFile function.
public static UntypedInt O_FILE_FLAG_OPEN_NO_RECALL => 0x00100000;
public static UntypedInt O_FILE_FLAG_OPEN_REPARSE_POINT => 0x00200000;
public static UntypedInt O_FILE_FLAG_SESSION_AWARE => 0x00800000;
public static UntypedInt O_FILE_FLAG_POSIX_SEMANTICS => 0x01000000;
public static UntypedInt O_FILE_FLAG_BACKUP_SEMANTICS => 0x02000000;
public static UntypedInt O_FILE_FLAG_DELETE_ON_CLOSE => 0x04000000;
public static UntypedInt O_FILE_FLAG_SEQUENTIAL_SCAN => 0x08000000;
public static UntypedInt O_FILE_FLAG_RANDOM_ACCESS => 0x10000000;
public static UntypedInt O_FILE_FLAG_NO_BUFFERING => 0x20000000;
public static UntypedInt O_FILE_FLAG_OVERLAPPED => 0x40000000;
public static UntypedInt O_FILE_FLAG_WRITE_THROUGH => 0x80000000;

// CreateDisposition flags for NtCreateFile and NtCreateNamedPipeFile.
public static UntypedInt FILE_SUPERSEDE => 0x00000000;
public static UntypedInt FILE_OPEN => 0x00000001;
public static UntypedInt FILE_CREATE => 0x00000002;
public static UntypedInt FILE_OPEN_IF => 0x00000003;
public static UntypedInt FILE_OVERWRITE => 0x00000004;
public static UntypedInt FILE_OVERWRITE_IF => 0x00000005;
public static UntypedInt FILE_MAXIMUM_DISPOSITION => 0x00000005;

// CreateOptions flags for NtCreateFile and NtCreateNamedPipeFile.
public static UntypedInt FILE_DIRECTORY_FILE => 0x00000001;
public static UntypedInt FILE_WRITE_THROUGH => 0x00000002;
public static UntypedInt FILE_SEQUENTIAL_ONLY => 0x00000004;
public static UntypedInt FILE_NO_INTERMEDIATE_BUFFERING => 0x00000008;
public static UntypedInt FILE_SYNCHRONOUS_IO_ALERT => 0x00000010;
public static UntypedInt FILE_SYNCHRONOUS_IO_NONALERT => 0x00000020;
public static UntypedInt FILE_NON_DIRECTORY_FILE => 0x00000040;
public static UntypedInt FILE_CREATE_TREE_CONNECTION => 0x00000080;
public static UntypedInt FILE_COMPLETE_IF_OPLOCKED => 0x00000100;
public static UntypedInt FILE_NO_EA_KNOWLEDGE => 0x00000200;
public static UntypedInt FILE_OPEN_REMOTE_INSTANCE => 0x00000400;
public static UntypedInt FILE_RANDOM_ACCESS => 0x00000800;
public static UntypedInt FILE_DELETE_ON_CLOSE => 0x00001000;
public static UntypedInt FILE_OPEN_BY_FILE_ID => 0x00002000;
public static UntypedInt FILE_OPEN_FOR_BACKUP_INTENT => 0x00004000;
public static UntypedInt FILE_NO_COMPRESSION => 0x00008000;
public static UntypedInt FILE_OPEN_REQUIRING_OPLOCK => 0x00010000;
public static UntypedInt FILE_DISALLOW_EXCLUSIVE => 0x00020000;
public static UntypedInt FILE_SESSION_AWARE => 0x00040000;
public static UntypedInt FILE_RESERVE_OPFILTER => 0x00100000;
public static UntypedInt FILE_OPEN_REPARSE_POINT => 0x00200000;
public static UntypedInt FILE_OPEN_NO_RECALL => 0x00400000;
public static UntypedInt FILE_OPEN_FOR_FREE_SPACE_QUERY => 0x00800000;

// https://learn.microsoft.com/en-us/windows/win32/api/winbase/ns-winbase-file_disposition_info
partial struct FILE_DISPOSITION_INFO {
    public bool DeleteFile;
}

// https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/ntddk/ns-ntddk-_file_disposition_information
partial struct FILE_DISPOSITION_INFORMATION {
    public bool DeleteFile;
}

// https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/ntddk/ns-ntddk-_file_disposition_information_ex
partial struct FILE_DISPOSITION_INFORMATION_EX {
    public uint32 Flags;
}

// https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/ntddk/ns-ntddk-_file_disposition_information_ex
public static UntypedInt FILE_DISPOSITION_DO_NOT_DELETE => 0x00000000;
public static UntypedInt FILE_DISPOSITION_DELETE => 0x00000001;
public static UntypedInt FILE_DISPOSITION_POSIX_SEMANTICS => 0x00000002;
public static UntypedInt FILE_DISPOSITION_FORCE_IMAGE_SECTION_CHECK => 0x00000004;
public static UntypedInt FILE_DISPOSITION_ON_CLOSE => 0x00000008;
public static UntypedInt FILE_DISPOSITION_IGNORE_READONLY_ATTRIBUTE => 0x00000010;

// Flags for FILE_RENAME_INFORMATION_EX.
public static UntypedInt FILE_RENAME_REPLACE_IF_EXISTS => 0x00000001;
public static UntypedInt FILE_RENAME_POSIX_SEMANTICS => 0x00000002;

// https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/ntifs/ns-ntifs-_file_rename_information
partial struct FILE_RENAME_INFORMATION {
    public bool ReplaceIfExists;
    public syscallꓸHandle RootDirectory;
    public uint32 FileNameLength;
    public array<uint16> FileName = new(syscall.MAX_PATH);
}

// https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/ntifs/ns-ntifs-_file_rename_information
partial struct FILE_RENAME_INFORMATION_EX {
    public uint32 Flags;
    public syscallꓸHandle RootDirectory;
    public uint32 FileNameLength;
    public array<uint16> FileName = new(syscall.MAX_PATH);
}

// https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/ntifs/ns-ntifs-_file_link_information
partial struct FILE_LINK_INFORMATION {
    public bool ReplaceIfExists;
    public syscallꓸHandle RootDirectory;
    public uint32 FileNameLength;
    public array<uint16> FileName = new(syscall.MAX_PATH);
}

public static UntypedInt FileIoCompletionNotificationInformation => 41;
public static UntypedInt FileReplaceCompletionInformation => 61;

partial struct FILE_IO_COMPLETION_NOTIFICATION_INFORMATION {
    public uint32 Flags;
}

// https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/ntifs/ns-ntifs-_file_completion_information
partial struct FILE_COMPLETION_INFORMATION {
    public syscallꓸHandle Port;
    public uintptr Key;
}

// https://learn.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-osversioninfoexa
// https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/wdm/ns-wdm-_osversioninfoexw
public static UntypedInt VER_NT_WORKSTATION => 0x0000001;

public static UntypedInt FileFlagsMask => 0xFFF00000;

public static UntypedInt ValidFileFlagsMask => /* O_FILE_FLAG_OPEN_REPARSE_POINT |
	O_FILE_FLAG_BACKUP_SEMANTICS |
	O_FILE_FLAG_OVERLAPPED |
	O_FILE_FLAG_OPEN_NO_RECALL |
	O_FILE_FLAG_SESSION_AWARE |
	O_FILE_FLAG_POSIX_SEMANTICS |
	O_FILE_FLAG_DELETE_ON_CLOSE |
	O_FILE_FLAG_SEQUENTIAL_SCAN |
	O_FILE_FLAG_NO_BUFFERING |
	O_FILE_FLAG_RANDOM_ACCESS |
	O_FILE_FLAG_WRITE_THROUGH */ 4289724416;

// https://msdn.microsoft.com/en-us/library/windows/desktop/aa379636.aspx
partial struct TRUSTEE {
    public ж<TRUSTEE> MultipleTrustee;
    public uint32 MultipleTrusteeOperation;
    public uint32 TrusteeForm;
    public uint32 TrusteeType;
    public uintptr Name;
}

// https://msdn.microsoft.com/en-us/library/windows/desktop/aa379638.aspx
public static UntypedInt TRUSTEE_IS_SID => 0x0;
public static UntypedInt TRUSTEE_IS_NAME => 0x1;
public static UntypedInt TRUSTEE_BAD_FORM => 0x2;
public static UntypedInt TRUSTEE_IS_OBJECTS_AND_SID => 0x3;
public static UntypedInt TRUSTEE_IS_OBJECTS_AND_NAME => 0x4;

// https://msdn.microsoft.com/en-us/library/windows/desktop/aa446627.aspx
partial struct EXPLICIT_ACCESS {
    public uint32 AccessPermissions;
    public uint32 AccessMode;
    public uint32 Inheritance;
    public TRUSTEE Trustee;
}

// https://msdn.microsoft.com/en-us/library/windows/desktop/aa374899.aspx
public static UntypedInt NOT_USED_ACCESS => 0x0;
public static UntypedInt GRANT_ACCESS => 0x1;
public static UntypedInt SET_ACCESS => 0x2;
public static UntypedInt DENY_ACCESS => 0x3;
public static UntypedInt REVOKE_ACCESS => 0x4;
public static UntypedInt SET_AUDIT_SUCCESS => 0x5;
public static UntypedInt SET_AUDIT_FAILURE => 0x6;

// https://msdn.microsoft.com/en-us/library/windows/desktop/aa446627.aspx
public static UntypedInt NO_INHERITANCE => 0x0;
public static UntypedInt SUB_OBJECTS_ONLY_INHERIT => 0x1;
public static UntypedInt SUB_CONTAINERS_ONLY_INHERIT => 0x2;
public static UntypedInt SUB_CONTAINERS_AND_OBJECTS_INHERIT => 0x3;
public static UntypedInt INHERIT_NO_PROPAGATE => 0x4;
public static UntypedInt INHERIT_ONLY => 0x8;

// https://msdn.microsoft.com/en-us/library/windows/desktop/aa379593.aspx
public static UntypedInt SE_UNKNOWN_OBJECT_TYPE => 0x0;
public static UntypedInt SE_FILE_OBJECT => 0x1;
public static UntypedInt SE_SERVICE => 0x2;
public static UntypedInt SE_PRINTER => 0x3;
public static UntypedInt SE_REGISTRY_KEY => 0x4;
public static UntypedInt SE_LMSHARE => 0x5;
public static UntypedInt SE_KERNEL_OBJECT => 0x6;
public static UntypedInt SE_WINDOW_OBJECT => 0x7;
public static UntypedInt SE_DS_OBJECT => 0x8;
public static UntypedInt SE_DS_OBJECT_ALL => 0x9;
public static UntypedInt SE_PROVIDER_DEFINED_OBJECT => 0xa;
public static UntypedInt SE_WMIGUID_OBJECT => 0xb;
public static UntypedInt SE_REGISTRY_WOW64_32KEY => 0xc;
public static UntypedInt SE_REGISTRY_WOW64_64KEY => 0xd;

// https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-dtyp/23e75ca3-98fd-4396-84e5-86cd9d40d343
public static UntypedInt OWNER_SECURITY_INFORMATION => 0x00000001;
public static UntypedInt GROUP_SECURITY_INFORMATION => 0x00000002;
public static UntypedInt DACL_SECURITY_INFORMATION => 0x00000004;
public static UntypedInt SACL_SECURITY_INFORMATION => 0x00000008;
public static UntypedInt LABEL_SECURITY_INFORMATION => 0x00000010;
public static UntypedInt UNPROTECTED_SACL_SECURITY_INFORMATION => 0x10000000;
public static UntypedInt UNPROTECTED_DACL_SECURITY_INFORMATION => 0x20000000;
public static UntypedInt PROTECTED_SACL_SECURITY_INFORMATION => 0x40000000;
public static UntypedInt PROTECTED_DACL_SECURITY_INFORMATION => 0x80000000;
public static UntypedInt ATTRIBUTE_SECURITY_INFORMATION => 0x00000020;
public static UntypedInt SCOPE_SECURITY_INFORMATION => 0x00000040;
public static UntypedInt PROCESS_TRUST_LABEL_SECURITY_INFORMATION => 0x00000080;
public static UntypedInt BACKUP_SECURITY_INFORMATION => 0x00010000;

} // end windows_package
