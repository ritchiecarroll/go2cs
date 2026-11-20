// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

global using ResourceIDOrString = object;

namespace go.golang.org.x.sys;

using net = net_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using FieldOffsetAttribute = global::System.Runtime.InteropServices.FieldOffsetAttribute;
using LayoutKind = global::System.Runtime.InteropServices.LayoutKind;
using StructLayoutAttribute = global::System.Runtime.InteropServices.StructLayoutAttribute;
using go.arrays;
using go.golib;
using static windows_package;

partial class windows_package {

partial struct NTStatus /*num:uint32*/;

public static UntypedInt O_RDONLY => 0x00000;
public static UntypedInt O_WRONLY => 0x00001;
public static UntypedInt O_RDWR => 0x00002;
public static UntypedInt O_CREAT => 0x00040;
public static UntypedInt O_EXCL => 0x00080;
public static UntypedInt O_NOCTTY => 0x00100;
public static UntypedInt O_TRUNC => 0x00200;
public static UntypedInt O_NONBLOCK => 0x00800;
public static UntypedInt O_APPEND => 0x00400;
public static UntypedInt O_SYNC => 0x01000;
public static UntypedInt O_ASYNC => 0x02000;
public static UntypedInt O_CLOEXEC => 0x80000;

public static Signal SIGHUP => /* Signal(0x1) */ 1;
public static Signal SIGINT => /* Signal(0x2) */ 2;
public static Signal SIGQUIT => /* Signal(0x3) */ 3;
public static Signal SIGILL => /* Signal(0x4) */ 4;
public static Signal SIGTRAP => /* Signal(0x5) */ 5;
public static Signal SIGABRT => /* Signal(0x6) */ 6;
public static Signal SIGBUS => /* Signal(0x7) */ 7;
public static Signal SIGFPE => /* Signal(0x8) */ 8;
public static Signal SIGKILL => /* Signal(0x9) */ 9;
public static Signal SIGSEGV => /* Signal(0xb) */ 11;
public static Signal SIGPIPE => /* Signal(0xd) */ 13;
public static Signal SIGALRM => /* Signal(0xe) */ 14;
public static Signal SIGTERM => /* Signal(0xf) */ 15;

[GoLazy] partial class lazyᴛsignals { internal static array<@string> signals = new array<@string>(16){
    [1] = "hangup",
    [2] = "interrupt",
    [3] = "quit",
    [4] = "illegal instruction",
    [5] = "trace/breakpoint trap",
    [6] = "aborted",
    [7] = "bus error",
    [8] = "floating point exception",
    [9] = "killed",
    [10] = "user defined signal 1",
    [11] = "segmentation fault",
    [12] = "user defined signal 2",
    [13] = "broken pipe",
    [14] = "alarm clock",
    [15] = "terminated"
}; }

public static UntypedInt O_FILE_FLAG_OPEN_NO_RECALL => /* FILE_FLAG_OPEN_NO_RECALL */ 1048576;
public static UntypedInt O_FILE_FLAG_OPEN_REPARSE_POINT => /* FILE_FLAG_OPEN_REPARSE_POINT */ 2097152;
public static UntypedInt O_FILE_FLAG_SESSION_AWARE => /* FILE_FLAG_SESSION_AWARE */ 8388608;
public static UntypedInt O_FILE_FLAG_POSIX_SEMANTICS => /* FILE_FLAG_POSIX_SEMANTICS */ 16777216;
public static UntypedInt O_FILE_FLAG_BACKUP_SEMANTICS => /* FILE_FLAG_BACKUP_SEMANTICS */ 33554432;
public static UntypedInt O_FILE_FLAG_DELETE_ON_CLOSE => /* FILE_FLAG_DELETE_ON_CLOSE */ 67108864;
public static UntypedInt O_FILE_FLAG_SEQUENTIAL_SCAN => /* FILE_FLAG_SEQUENTIAL_SCAN */ 134217728;
public static UntypedInt O_FILE_FLAG_RANDOM_ACCESS => /* FILE_FLAG_RANDOM_ACCESS */ 268435456;
public static UntypedInt O_FILE_FLAG_NO_BUFFERING => /* FILE_FLAG_NO_BUFFERING */ 536870912;
public static UntypedInt O_FILE_FLAG_OVERLAPPED => /* FILE_FLAG_OVERLAPPED */ 1073741824;
public static UntypedInt O_FILE_FLAG_WRITE_THROUGH => /* FILE_FLAG_WRITE_THROUGH */ 2147483648;

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
public static UntypedInt CREATE_NEW => 1;
public static UntypedInt CREATE_ALWAYS => 2;
public static UntypedInt OPEN_EXISTING => 3;
public static UntypedInt OPEN_ALWAYS => 4;
public static UntypedInt TRUNCATE_EXISTING => 5;
public static UntypedInt FILE_FLAG_OPEN_REQUIRING_OPLOCK => 0x00040000;
public static UntypedInt FILE_FLAG_FIRST_PIPE_INSTANCE => 0x00080000;
public static UntypedInt FILE_FLAG_OPEN_NO_RECALL => 0x00100000;
public static UntypedInt FILE_FLAG_OPEN_REPARSE_POINT => 0x00200000;
public static UntypedInt FILE_FLAG_SESSION_AWARE => 0x00800000;
public static UntypedInt FILE_FLAG_POSIX_SEMANTICS => 0x01000000;
public static UntypedInt FILE_FLAG_BACKUP_SEMANTICS => 0x02000000;
public static UntypedInt FILE_FLAG_DELETE_ON_CLOSE => 0x04000000;
public static UntypedInt FILE_FLAG_SEQUENTIAL_SCAN => 0x08000000;
public static UntypedInt FILE_FLAG_RANDOM_ACCESS => 0x10000000;
public static UntypedInt FILE_FLAG_NO_BUFFERING => 0x20000000;
public static UntypedInt FILE_FLAG_OVERLAPPED => 0x40000000;
public static UntypedInt FILE_FLAG_WRITE_THROUGH => 0x80000000;
public static UntypedInt HANDLE_FLAG_INHERIT => 0x00000001;
public static UntypedInt STARTF_USESTDHANDLES => 0x00000100;
public static UntypedInt STARTF_USESHOWWINDOW => 0x00000001;
public static UntypedInt DUPLICATE_CLOSE_SOURCE => 0x00000001;
public static UntypedInt DUPLICATE_SAME_ACCESS => 0x00000002;
public static UntypedInt STD_INPUT_HANDLE => /* -10 & (1<<32 - 1) */ 4294967286;
public static UntypedInt STD_OUTPUT_HANDLE => /* -11 & (1<<32 - 1) */ 4294967285;
public static UntypedInt STD_ERROR_HANDLE => /* -12 & (1<<32 - 1) */ 4294967284;
public static UntypedInt FILE_BEGIN => 0;
public static UntypedInt FILE_CURRENT => 1;
public static UntypedInt FILE_END => 2;
public static UntypedInt LANG_ENGLISH => 0x09;
public static UntypedInt SUBLANG_ENGLISH_US => 0x01;
public static UntypedInt FORMAT_MESSAGE_ALLOCATE_BUFFER => 256;
public static UntypedInt FORMAT_MESSAGE_IGNORE_INSERTS => 512;
public static UntypedInt FORMAT_MESSAGE_FROM_STRING => 1024;
public static UntypedInt FORMAT_MESSAGE_FROM_HMODULE => 2048;
public static UntypedInt FORMAT_MESSAGE_FROM_SYSTEM => 4096;
public static UntypedInt FORMAT_MESSAGE_ARGUMENT_ARRAY => 8192;
public static UntypedInt FORMAT_MESSAGE_MAX_WIDTH_MASK => 255;
public static UntypedInt MAX_USHORT => 0xffff;
public static UntypedInt MAX_PATH => 260;
public static UntypedInt MAX_LONG_PATH => 32768;
public static UntypedInt MAX_MODULE_NAME32 => 255;
public static UntypedInt MAX_COMPUTERNAME_LENGTH => 15;
public static UntypedInt MAX_DHCPV6_DUID_LENGTH => 130;
public static UntypedInt MAX_DNS_SUFFIX_STRING_LENGTH => 256;
public static UntypedInt TIME_ZONE_ID_UNKNOWN => 0;
public static UntypedInt TIME_ZONE_ID_STANDARD => 1;
public static UntypedInt TIME_ZONE_ID_DAYLIGHT => 2;
public static UntypedInt IGNORE => 0;
public static UntypedInt INFINITE => 0xffffffff;
public static UntypedInt WAIT_ABANDONED => 0x00000080;
public static UntypedInt WAIT_OBJECT_0 => 0x00000000;
public static UntypedInt WAIT_FAILED => 0xFFFFFFFF;
public static UntypedInt PROCESS_ALL_ACCESS => 0xFFFF;
public static UntypedInt PROCESS_CREATE_PROCESS => 0x0080;
public static UntypedInt PROCESS_CREATE_THREAD => 0x0002;
public static UntypedInt PROCESS_DUP_HANDLE => 0x0040;
public static UntypedInt PROCESS_QUERY_INFORMATION => 0x0400;
public static UntypedInt PROCESS_QUERY_LIMITED_INFORMATION => 0x1000;
public static UntypedInt PROCESS_SET_INFORMATION => 0x0200;
public static UntypedInt PROCESS_SET_QUOTA => 0x0100;
public static UntypedInt PROCESS_SUSPEND_RESUME => 0x0800;
public static UntypedInt PROCESS_TERMINATE => 0x0001;
public static UntypedInt PROCESS_VM_OPERATION => 0x0008;
public static UntypedInt PROCESS_VM_READ => 0x0010;
public static UntypedInt PROCESS_VM_WRITE => 0x0020;
public static UntypedInt THREAD_DIRECT_IMPERSONATION => 0x0200;
public static UntypedInt THREAD_GET_CONTEXT => 0x0008;
public static UntypedInt THREAD_IMPERSONATE => 0x0100;
public static UntypedInt THREAD_QUERY_INFORMATION => 0x0040;
public static UntypedInt THREAD_QUERY_LIMITED_INFORMATION => 0x0800;
public static UntypedInt THREAD_SET_CONTEXT => 0x0010;
public static UntypedInt THREAD_SET_INFORMATION => 0x0020;
public static UntypedInt THREAD_SET_LIMITED_INFORMATION => 0x0400;
public static UntypedInt THREAD_SET_THREAD_TOKEN => 0x0080;
public static UntypedInt THREAD_SUSPEND_RESUME => 0x0002;
public static UntypedInt THREAD_TERMINATE => 0x0001;
public static UntypedInt FILE_MAP_COPY => 0x01;
public static UntypedInt FILE_MAP_WRITE => 0x02;
public static UntypedInt FILE_MAP_READ => 0x04;
public static UntypedInt FILE_MAP_EXECUTE => 0x20;
public static UntypedInt CTRL_C_EVENT => 0;
public static UntypedInt CTRL_BREAK_EVENT => 1;
public static UntypedInt CTRL_CLOSE_EVENT => 2;
public static UntypedInt CTRL_LOGOFF_EVENT => 5;
public static UntypedInt CTRL_SHUTDOWN_EVENT => 6;
public static UntypedInt APPLICATION_ERROR => /* 1 << 29 */ 536870912;

public static UntypedInt CREATE_BREAKAWAY_FROM_JOB => 0x01000000;
public static UntypedInt CREATE_DEFAULT_ERROR_MODE => 0x04000000;
public static UntypedInt CREATE_NEW_CONSOLE => 0x00000010;
public static UntypedInt CREATE_NEW_PROCESS_GROUP => 0x00000200;
public static UntypedInt CREATE_NO_WINDOW => 0x08000000;
public static UntypedInt CREATE_PROTECTED_PROCESS => 0x00040000;
public static UntypedInt CREATE_PRESERVE_CODE_AUTHZ_LEVEL => 0x02000000;
public static UntypedInt CREATE_SEPARATE_WOW_VDM => 0x00000800;
public static UntypedInt CREATE_SHARED_WOW_VDM => 0x00001000;
public static UntypedInt CREATE_SUSPENDED => 0x00000004;
public static UntypedInt CREATE_UNICODE_ENVIRONMENT => 0x00000400;
public static UntypedInt DEBUG_ONLY_THIS_PROCESS => 0x00000002;
public static UntypedInt DEBUG_PROCESS => 0x00000001;
public static UntypedInt DETACHED_PROCESS => 0x00000008;
public static UntypedInt EXTENDED_STARTUPINFO_PRESENT => 0x00080000;
public static UntypedInt INHERIT_PARENT_AFFINITY => 0x00010000;

public static UntypedInt PROC_THREAD_ATTRIBUTE_PARENT_PROCESS => 0x00020000;
public static UntypedInt PROC_THREAD_ATTRIBUTE_HANDLE_LIST => 0x00020002;
public static UntypedInt PROC_THREAD_ATTRIBUTE_GROUP_AFFINITY => 0x00030003;
public static UntypedInt PROC_THREAD_ATTRIBUTE_PREFERRED_NODE => 0x00020004;
public static UntypedInt PROC_THREAD_ATTRIBUTE_IDEAL_PROCESSOR => 0x00030005;
public static UntypedInt PROC_THREAD_ATTRIBUTE_MITIGATION_POLICY => 0x00020007;
public static UntypedInt PROC_THREAD_ATTRIBUTE_UMS_THREAD => 0x00030006;
public static UntypedInt PROC_THREAD_ATTRIBUTE_PROTECTION_LEVEL => 0x0002000b;
public static UntypedInt PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE => 0x00020016;

public static UntypedInt TH32CS_SNAPHEAPLIST => 0x01;
public static UntypedInt TH32CS_SNAPPROCESS => 0x02;
public static UntypedInt TH32CS_SNAPTHREAD => 0x04;
public static UntypedInt TH32CS_SNAPMODULE => 0x08;
public static UntypedInt TH32CS_SNAPMODULE32 => 0x10;
public static UntypedInt TH32CS_SNAPALL => /* TH32CS_SNAPHEAPLIST | TH32CS_SNAPMODULE | TH32CS_SNAPPROCESS | TH32CS_SNAPTHREAD */ 15;
public static UntypedInt TH32CS_INHERIT => 0x80000000;

public static UntypedInt LIST_MODULES_32BIT => 0x01;
public static UntypedInt LIST_MODULES_64BIT => 0x02;
public static UntypedInt LIST_MODULES_ALL => 0x03;
public static UntypedInt LIST_MODULES_DEFAULT => 0x00;

public static UntypedInt FILE_NOTIFY_CHANGE_FILE_NAME => 0x001;
public static UntypedInt FILE_NOTIFY_CHANGE_DIR_NAME => 0x002;
public static UntypedInt FILE_NOTIFY_CHANGE_ATTRIBUTES => 0x004;
public static UntypedInt FILE_NOTIFY_CHANGE_SIZE => 0x008;
public static UntypedInt FILE_NOTIFY_CHANGE_LAST_WRITE => 0x010;
public static UntypedInt FILE_NOTIFY_CHANGE_LAST_ACCESS => 0x020;
public static UntypedInt FILE_NOTIFY_CHANGE_CREATION => 0x040;
public static UntypedInt FILE_NOTIFY_CHANGE_SECURITY => 0x100;

public static UntypedInt FILE_ACTION_ADDED => /* iota + 1 */ 1;
public static UntypedInt FILE_ACTION_REMOVED => 2;
public static UntypedInt FILE_ACTION_MODIFIED => 3;
public static UntypedInt FILE_ACTION_RENAMED_OLD_NAME => 4;
public static UntypedInt FILE_ACTION_RENAMED_NEW_NAME => 5;

public static UntypedInt PROV_RSA_FULL => 1;
public static UntypedInt PROV_RSA_SIG => 2;
public static UntypedInt PROV_DSS => 3;
public static UntypedInt PROV_FORTEZZA => 4;
public static UntypedInt PROV_MS_EXCHANGE => 5;
public static UntypedInt PROV_SSL => 6;
public static UntypedInt PROV_RSA_SCHANNEL => 12;
public static UntypedInt PROV_DSS_DH => 13;
public static UntypedInt PROV_EC_ECDSA_SIG => 14;
public static UntypedInt PROV_EC_ECNRA_SIG => 15;
public static UntypedInt PROV_EC_ECDSA_FULL => 16;
public static UntypedInt PROV_EC_ECNRA_FULL => 17;
public static UntypedInt PROV_DH_SCHANNEL => 18;
public static UntypedInt PROV_SPYRUS_LYNKS => 20;
public static UntypedInt PROV_RNG => 21;
public static UntypedInt PROV_INTEL_SEC => 22;
public static UntypedInt PROV_REPLACE_OWF => 23;
public static UntypedInt PROV_RSA_AES => 24;
public static UntypedInt CRYPT_VERIFYCONTEXT => 0xF0000000;
public static UntypedInt CRYPT_NEWKEYSET => 0x00000008;
public static UntypedInt CRYPT_DELETEKEYSET => 0x00000010;
public static UntypedInt CRYPT_MACHINE_KEYSET => 0x00000020;
public static UntypedInt CRYPT_SILENT => 0x00000040;
public static UntypedInt CRYPT_DEFAULT_CONTAINER_OPTIONAL => 0x00000080;
public static UntypedInt CRYPT_EXPORTABLE => 0x00000001;
public static UntypedInt CRYPT_USER_PROTECTED => 0x00000002;
public static UntypedInt CRYPT_USER_KEYSET => 0x00001000;
public static UntypedInt PKCS12_PREFER_CNG_KSP => 0x00000100;
public static UntypedInt PKCS12_ALWAYS_CNG_KSP => 0x00000200;
public static UntypedInt PKCS12_ALLOW_OVERWRITE_KEY => 0x00004000;
public static UntypedInt PKCS12_NO_PERSIST_KEY => 0x00008000;
public static UntypedInt PKCS12_INCLUDE_EXTENDED_PROPERTIES => 0x00000010;
public static UntypedInt CRYPT_ACQUIRE_CACHE_FLAG => 0x00000001;
public static UntypedInt CRYPT_ACQUIRE_USE_PROV_INFO_FLAG => 0x00000002;
public static UntypedInt CRYPT_ACQUIRE_COMPARE_KEY_FLAG => 0x00000004;
public static UntypedInt CRYPT_ACQUIRE_NO_HEALING => 0x00000008;
public static UntypedInt CRYPT_ACQUIRE_SILENT_FLAG => 0x00000040;
public static UntypedInt CRYPT_ACQUIRE_WINDOW_HANDLE_FLAG => 0x00000080;
public static UntypedInt CRYPT_ACQUIRE_NCRYPT_KEY_FLAGS_MASK => 0x00070000;
public static UntypedInt CRYPT_ACQUIRE_ALLOW_NCRYPT_KEY_FLAG => 0x00010000;
public static UntypedInt CRYPT_ACQUIRE_PREFER_NCRYPT_KEY_FLAG => 0x00020000;
public static UntypedInt CRYPT_ACQUIRE_ONLY_NCRYPT_KEY_FLAG => 0x00040000;
public static UntypedInt AT_KEYEXCHANGE => 1;
public static UntypedInt AT_SIGNATURE => 2;
public static UntypedInt CERT_NCRYPT_KEY_SPEC => 0xFFFFFFFF;
public static UntypedInt USAGE_MATCH_TYPE_AND => 0;
public static UntypedInt USAGE_MATCH_TYPE_OR => 1;
public static UntypedInt X509_ASN_ENCODING => 0x00000001;
public static UntypedInt PKCS_7_ASN_ENCODING => 0x00010000;
public static UntypedInt CERT_STORE_PROV_MSG => 1;
public static UntypedInt CERT_STORE_PROV_MEMORY => 2;
public static UntypedInt CERT_STORE_PROV_FILE => 3;
public static UntypedInt CERT_STORE_PROV_REG => 4;
public static UntypedInt CERT_STORE_PROV_PKCS7 => 5;
public static UntypedInt CERT_STORE_PROV_SERIALIZED => 6;
public static UntypedInt CERT_STORE_PROV_FILENAME_A => 7;
public static UntypedInt CERT_STORE_PROV_FILENAME_W => 8;
public static UntypedInt CERT_STORE_PROV_FILENAME => /* CERT_STORE_PROV_FILENAME_W */ 8;
public static UntypedInt CERT_STORE_PROV_SYSTEM_A => 9;
public static UntypedInt CERT_STORE_PROV_SYSTEM_W => 10;
public static UntypedInt CERT_STORE_PROV_SYSTEM => /* CERT_STORE_PROV_SYSTEM_W */ 10;
public static UntypedInt CERT_STORE_PROV_COLLECTION => 11;
public static UntypedInt CERT_STORE_PROV_SYSTEM_REGISTRY_A => 12;
public static UntypedInt CERT_STORE_PROV_SYSTEM_REGISTRY_W => 13;
public static UntypedInt CERT_STORE_PROV_SYSTEM_REGISTRY => /* CERT_STORE_PROV_SYSTEM_REGISTRY_W */ 13;
public static UntypedInt CERT_STORE_PROV_PHYSICAL_W => 14;
public static UntypedInt CERT_STORE_PROV_PHYSICAL => /* CERT_STORE_PROV_PHYSICAL_W */ 14;
public static UntypedInt CERT_STORE_PROV_SMART_CARD_W => 15;
public static UntypedInt CERT_STORE_PROV_SMART_CARD => /* CERT_STORE_PROV_SMART_CARD_W */ 15;
public static UntypedInt CERT_STORE_PROV_LDAP_W => 16;
public static UntypedInt CERT_STORE_PROV_LDAP => /* CERT_STORE_PROV_LDAP_W */ 16;
public static UntypedInt CERT_STORE_PROV_PKCS12 => 17;
public static UntypedInt CERT_STORE_NO_CRYPT_RELEASE_FLAG => 0x00000001;
public static UntypedInt CERT_STORE_SET_LOCALIZED_NAME_FLAG => 0x00000002;
public static UntypedInt CERT_STORE_DEFER_CLOSE_UNTIL_LAST_FREE_FLAG => 0x00000004;
public static UntypedInt CERT_STORE_DELETE_FLAG => 0x00000010;
public static UntypedInt CERT_STORE_UNSAFE_PHYSICAL_FLAG => 0x00000020;
public static UntypedInt CERT_STORE_SHARE_STORE_FLAG => 0x00000040;
public static UntypedInt CERT_STORE_SHARE_CONTEXT_FLAG => 0x00000080;
public static UntypedInt CERT_STORE_MANIFOLD_FLAG => 0x00000100;
public static UntypedInt CERT_STORE_ENUM_ARCHIVED_FLAG => 0x00000200;
public static UntypedInt CERT_STORE_UPDATE_KEYID_FLAG => 0x00000400;
public static UntypedInt CERT_STORE_BACKUP_RESTORE_FLAG => 0x00000800;
public static UntypedInt CERT_STORE_MAXIMUM_ALLOWED_FLAG => 0x00001000;
public static UntypedInt CERT_STORE_CREATE_NEW_FLAG => 0x00002000;
public static UntypedInt CERT_STORE_OPEN_EXISTING_FLAG => 0x00004000;
public static UntypedInt CERT_STORE_READONLY_FLAG => 0x00008000;
public static UntypedInt CERT_SYSTEM_STORE_CURRENT_USER => 0x00010000;
public static UntypedInt CERT_SYSTEM_STORE_LOCAL_MACHINE => 0x00020000;
public static UntypedInt CERT_SYSTEM_STORE_CURRENT_SERVICE => 0x00040000;
public static UntypedInt CERT_SYSTEM_STORE_SERVICES => 0x00050000;
public static UntypedInt CERT_SYSTEM_STORE_USERS => 0x00060000;
public static UntypedInt CERT_SYSTEM_STORE_CURRENT_USER_GROUP_POLICY => 0x00070000;
public static UntypedInt CERT_SYSTEM_STORE_LOCAL_MACHINE_GROUP_POLICY => 0x00080000;
public static UntypedInt CERT_SYSTEM_STORE_LOCAL_MACHINE_ENTERPRISE => 0x00090000;
public static UntypedInt CERT_SYSTEM_STORE_UNPROTECTED_FLAG => 0x40000000;
public static UntypedInt CERT_SYSTEM_STORE_RELOCATE_FLAG => 0x80000000;
public static UntypedInt CERT_REGISTRY_STORE_REMOTE_FLAG => 0x00010000;
public static UntypedInt CERT_REGISTRY_STORE_SERIALIZED_FLAG => 0x00020000;
public static UntypedInt CERT_REGISTRY_STORE_ROAMING_FLAG => 0x00040000;
public static UntypedInt CERT_REGISTRY_STORE_MY_IE_DIRTY_FLAG => 0x00080000;
public static UntypedInt CERT_REGISTRY_STORE_LM_GPT_FLAG => 0x01000000;
public static UntypedInt CERT_REGISTRY_STORE_CLIENT_GPT_FLAG => 0x80000000;
public static UntypedInt CERT_FILE_STORE_COMMIT_ENABLE_FLAG => 0x00010000;
public static UntypedInt CERT_LDAP_STORE_SIGN_FLAG => 0x00010000;
public static UntypedInt CERT_LDAP_STORE_AREC_EXCLUSIVE_FLAG => 0x00020000;
public static UntypedInt CERT_LDAP_STORE_OPENED_FLAG => 0x00040000;
public static UntypedInt CERT_LDAP_STORE_UNBIND_FLAG => 0x00080000;
public static UntypedInt CERT_STORE_ADD_NEW => 1;
public static UntypedInt CERT_STORE_ADD_USE_EXISTING => 2;
public static UntypedInt CERT_STORE_ADD_REPLACE_EXISTING => 3;
public static UntypedInt CERT_STORE_ADD_ALWAYS => 4;
public static UntypedInt CERT_STORE_ADD_REPLACE_EXISTING_INHERIT_PROPERTIES => 5;
public static UntypedInt CERT_STORE_ADD_NEWER => 6;
public static UntypedInt CERT_STORE_ADD_NEWER_INHERIT_PROPERTIES => 7;
public static UntypedInt CERT_TRUST_NO_ERROR => 0x00000000;
public static UntypedInt CERT_TRUST_IS_NOT_TIME_VALID => 0x00000001;
public static UntypedInt CERT_TRUST_IS_REVOKED => 0x00000004;
public static UntypedInt CERT_TRUST_IS_NOT_SIGNATURE_VALID => 0x00000008;
public static UntypedInt CERT_TRUST_IS_NOT_VALID_FOR_USAGE => 0x00000010;
public static UntypedInt CERT_TRUST_IS_UNTRUSTED_ROOT => 0x00000020;
public static UntypedInt CERT_TRUST_REVOCATION_STATUS_UNKNOWN => 0x00000040;
public static UntypedInt CERT_TRUST_IS_CYCLIC => 0x00000080;
public static UntypedInt CERT_TRUST_INVALID_EXTENSION => 0x00000100;
public static UntypedInt CERT_TRUST_INVALID_POLICY_CONSTRAINTS => 0x00000200;
public static UntypedInt CERT_TRUST_INVALID_BASIC_CONSTRAINTS => 0x00000400;
public static UntypedInt CERT_TRUST_INVALID_NAME_CONSTRAINTS => 0x00000800;
public static UntypedInt CERT_TRUST_HAS_NOT_SUPPORTED_NAME_CONSTRAINT => 0x00001000;
public static UntypedInt CERT_TRUST_HAS_NOT_DEFINED_NAME_CONSTRAINT => 0x00002000;
public static UntypedInt CERT_TRUST_HAS_NOT_PERMITTED_NAME_CONSTRAINT => 0x00004000;
public static UntypedInt CERT_TRUST_HAS_EXCLUDED_NAME_CONSTRAINT => 0x00008000;
public static UntypedInt CERT_TRUST_IS_PARTIAL_CHAIN => 0x00010000;
public static UntypedInt CERT_TRUST_CTL_IS_NOT_TIME_VALID => 0x00020000;
public static UntypedInt CERT_TRUST_CTL_IS_NOT_SIGNATURE_VALID => 0x00040000;
public static UntypedInt CERT_TRUST_CTL_IS_NOT_VALID_FOR_USAGE => 0x00080000;
public static UntypedInt CERT_TRUST_HAS_WEAK_SIGNATURE => 0x00100000;
public static UntypedInt CERT_TRUST_IS_OFFLINE_REVOCATION => 0x01000000;
public static UntypedInt CERT_TRUST_NO_ISSUANCE_CHAIN_POLICY => 0x02000000;
public static UntypedInt CERT_TRUST_IS_EXPLICIT_DISTRUST => 0x04000000;
public static UntypedInt CERT_TRUST_HAS_NOT_SUPPORTED_CRITICAL_EXT => 0x08000000;
public static UntypedInt CERT_TRUST_HAS_EXACT_MATCH_ISSUER => 0x00000001;
public static UntypedInt CERT_TRUST_HAS_KEY_MATCH_ISSUER => 0x00000002;
public static UntypedInt CERT_TRUST_HAS_NAME_MATCH_ISSUER => 0x00000004;
public static UntypedInt CERT_TRUST_IS_SELF_SIGNED => 0x00000008;
public static UntypedInt CERT_TRUST_HAS_PREFERRED_ISSUER => 0x00000100;
public static UntypedInt CERT_TRUST_HAS_ISSUANCE_CHAIN_POLICY => 0x00000400;
public static UntypedInt CERT_TRUST_HAS_VALID_NAME_CONSTRAINTS => 0x00000400;
public static UntypedInt CERT_TRUST_IS_PEER_TRUSTED => 0x00000800;
public static UntypedInt CERT_TRUST_HAS_CRL_VALIDITY_EXTENDED => 0x00001000;
public static UntypedInt CERT_TRUST_IS_FROM_EXCLUSIVE_TRUST_STORE => 0x00002000;
public static UntypedInt CERT_TRUST_IS_CA_TRUSTED => 0x00004000;
public static UntypedInt CERT_TRUST_IS_COMPLEX_CHAIN => 0x00010000;
public static UntypedInt CERT_INFO_VERSION_FLAG => 1;
public static UntypedInt CERT_INFO_SERIAL_NUMBER_FLAG => 2;
public static UntypedInt CERT_INFO_SIGNATURE_ALGORITHM_FLAG => 3;
public static UntypedInt CERT_INFO_ISSUER_FLAG => 4;
public static UntypedInt CERT_INFO_NOT_BEFORE_FLAG => 5;
public static UntypedInt CERT_INFO_NOT_AFTER_FLAG => 6;
public static UntypedInt CERT_INFO_SUBJECT_FLAG => 7;
public static UntypedInt CERT_INFO_SUBJECT_PUBLIC_KEY_INFO_FLAG => 8;
public static UntypedInt CERT_INFO_ISSUER_UNIQUE_ID_FLAG => 9;
public static UntypedInt CERT_INFO_SUBJECT_UNIQUE_ID_FLAG => 10;
public static UntypedInt CERT_INFO_EXTENSION_FLAG => 11;
public static UntypedInt CERT_COMPARE_MASK => 0xFFFF;
public static UntypedInt CERT_COMPARE_SHIFT => 16;
public static UntypedInt CERT_COMPARE_ANY => 0;
public static UntypedInt CERT_COMPARE_SHA1_HASH => 1;
public static UntypedInt CERT_COMPARE_NAME => 2;
public static UntypedInt CERT_COMPARE_ATTR => 3;
public static UntypedInt CERT_COMPARE_MD5_HASH => 4;
public static UntypedInt CERT_COMPARE_PROPERTY => 5;
public static UntypedInt CERT_COMPARE_PUBLIC_KEY => 6;
public static UntypedInt CERT_COMPARE_HASH => /* CERT_COMPARE_SHA1_HASH */ 1;
public static UntypedInt CERT_COMPARE_NAME_STR_A => 7;
public static UntypedInt CERT_COMPARE_NAME_STR_W => 8;
public static UntypedInt CERT_COMPARE_KEY_SPEC => 9;
public static UntypedInt CERT_COMPARE_ENHKEY_USAGE => 10;
public static UntypedInt CERT_COMPARE_CTL_USAGE => /* CERT_COMPARE_ENHKEY_USAGE */ 10;
public static UntypedInt CERT_COMPARE_SUBJECT_CERT => 11;
public static UntypedInt CERT_COMPARE_ISSUER_OF => 12;
public static UntypedInt CERT_COMPARE_EXISTING => 13;
public static UntypedInt CERT_COMPARE_SIGNATURE_HASH => 14;
public static UntypedInt CERT_COMPARE_KEY_IDENTIFIER => 15;
public static UntypedInt CERT_COMPARE_CERT_ID => 16;
public static UntypedInt CERT_COMPARE_CROSS_CERT_DIST_POINTS => 17;
public static UntypedInt CERT_COMPARE_PUBKEY_MD5_HASH => 18;
public static UntypedInt CERT_COMPARE_SUBJECT_INFO_ACCESS => 19;
public static UntypedInt CERT_COMPARE_HASH_STR => 20;
public static UntypedInt CERT_COMPARE_HAS_PRIVATE_KEY => 21;
public static UntypedInt CERT_FIND_ANY => /* (CERT_COMPARE_ANY << CERT_COMPARE_SHIFT) */ 0;
public static UntypedInt CERT_FIND_SHA1_HASH => /* (CERT_COMPARE_SHA1_HASH << CERT_COMPARE_SHIFT) */ 65536;
public static UntypedInt CERT_FIND_MD5_HASH => /* (CERT_COMPARE_MD5_HASH << CERT_COMPARE_SHIFT) */ 262144;
public static UntypedInt CERT_FIND_SIGNATURE_HASH => /* (CERT_COMPARE_SIGNATURE_HASH << CERT_COMPARE_SHIFT) */ 917504;
public static UntypedInt CERT_FIND_KEY_IDENTIFIER => /* (CERT_COMPARE_KEY_IDENTIFIER << CERT_COMPARE_SHIFT) */ 983040;
public static UntypedInt CERT_FIND_HASH => /* CERT_FIND_SHA1_HASH */ 65536;
public static UntypedInt CERT_FIND_PROPERTY => /* (CERT_COMPARE_PROPERTY << CERT_COMPARE_SHIFT) */ 327680;
public static UntypedInt CERT_FIND_PUBLIC_KEY => /* (CERT_COMPARE_PUBLIC_KEY << CERT_COMPARE_SHIFT) */ 393216;
public static UntypedInt CERT_FIND_SUBJECT_NAME => /* (CERT_COMPARE_NAME<<CERT_COMPARE_SHIFT | CERT_INFO_SUBJECT_FLAG) */ 131079;
public static UntypedInt CERT_FIND_SUBJECT_ATTR => /* (CERT_COMPARE_ATTR<<CERT_COMPARE_SHIFT | CERT_INFO_SUBJECT_FLAG) */ 196615;
public static UntypedInt CERT_FIND_ISSUER_NAME => /* (CERT_COMPARE_NAME<<CERT_COMPARE_SHIFT | CERT_INFO_ISSUER_FLAG) */ 131076;
public static UntypedInt CERT_FIND_ISSUER_ATTR => /* (CERT_COMPARE_ATTR<<CERT_COMPARE_SHIFT | CERT_INFO_ISSUER_FLAG) */ 196612;
public static UntypedInt CERT_FIND_SUBJECT_STR_A => /* (CERT_COMPARE_NAME_STR_A<<CERT_COMPARE_SHIFT | CERT_INFO_SUBJECT_FLAG) */ 458759;
public static UntypedInt CERT_FIND_SUBJECT_STR_W => /* (CERT_COMPARE_NAME_STR_W<<CERT_COMPARE_SHIFT | CERT_INFO_SUBJECT_FLAG) */ 524295;
public static UntypedInt CERT_FIND_SUBJECT_STR => /* CERT_FIND_SUBJECT_STR_W */ 524295;
public static UntypedInt CERT_FIND_ISSUER_STR_A => /* (CERT_COMPARE_NAME_STR_A<<CERT_COMPARE_SHIFT | CERT_INFO_ISSUER_FLAG) */ 458756;
public static UntypedInt CERT_FIND_ISSUER_STR_W => /* (CERT_COMPARE_NAME_STR_W<<CERT_COMPARE_SHIFT | CERT_INFO_ISSUER_FLAG) */ 524292;
public static UntypedInt CERT_FIND_ISSUER_STR => /* CERT_FIND_ISSUER_STR_W */ 524292;
public static UntypedInt CERT_FIND_KEY_SPEC => /* (CERT_COMPARE_KEY_SPEC << CERT_COMPARE_SHIFT) */ 589824;
public static UntypedInt CERT_FIND_ENHKEY_USAGE => /* (CERT_COMPARE_ENHKEY_USAGE << CERT_COMPARE_SHIFT) */ 655360;
public static UntypedInt CERT_FIND_CTL_USAGE => /* CERT_FIND_ENHKEY_USAGE */ 655360;
public static UntypedInt CERT_FIND_SUBJECT_CERT => /* (CERT_COMPARE_SUBJECT_CERT << CERT_COMPARE_SHIFT) */ 720896;
public static UntypedInt CERT_FIND_ISSUER_OF => /* (CERT_COMPARE_ISSUER_OF << CERT_COMPARE_SHIFT) */ 786432;
public static UntypedInt CERT_FIND_EXISTING => /* (CERT_COMPARE_EXISTING << CERT_COMPARE_SHIFT) */ 851968;
public static UntypedInt CERT_FIND_CERT_ID => /* (CERT_COMPARE_CERT_ID << CERT_COMPARE_SHIFT) */ 1048576;
public static UntypedInt CERT_FIND_CROSS_CERT_DIST_POINTS => /* (CERT_COMPARE_CROSS_CERT_DIST_POINTS << CERT_COMPARE_SHIFT) */ 1114112;
public static UntypedInt CERT_FIND_PUBKEY_MD5_HASH => /* (CERT_COMPARE_PUBKEY_MD5_HASH << CERT_COMPARE_SHIFT) */ 1179648;
public static UntypedInt CERT_FIND_SUBJECT_INFO_ACCESS => /* (CERT_COMPARE_SUBJECT_INFO_ACCESS << CERT_COMPARE_SHIFT) */ 1245184;
public static UntypedInt CERT_FIND_HASH_STR => /* (CERT_COMPARE_HASH_STR << CERT_COMPARE_SHIFT) */ 1310720;
public static UntypedInt CERT_FIND_HAS_PRIVATE_KEY => /* (CERT_COMPARE_HAS_PRIVATE_KEY << CERT_COMPARE_SHIFT) */ 1376256;
public static UntypedInt CERT_FIND_OPTIONAL_ENHKEY_USAGE_FLAG => 0x1;
public static UntypedInt CERT_FIND_EXT_ONLY_ENHKEY_USAGE_FLAG => 0x2;
public static UntypedInt CERT_FIND_PROP_ONLY_ENHKEY_USAGE_FLAG => 0x4;
public static UntypedInt CERT_FIND_NO_ENHKEY_USAGE_FLAG => 0x8;
public static UntypedInt CERT_FIND_OR_ENHKEY_USAGE_FLAG => 0x10;
public static UntypedInt CERT_FIND_VALID_ENHKEY_USAGE_FLAG => 0x20;
public static UntypedInt CERT_FIND_OPTIONAL_CTL_USAGE_FLAG => /* CERT_FIND_OPTIONAL_ENHKEY_USAGE_FLAG */ 1;
public static UntypedInt CERT_FIND_EXT_ONLY_CTL_USAGE_FLAG => /* CERT_FIND_EXT_ONLY_ENHKEY_USAGE_FLAG */ 2;
public static UntypedInt CERT_FIND_PROP_ONLY_CTL_USAGE_FLAG => /* CERT_FIND_PROP_ONLY_ENHKEY_USAGE_FLAG */ 4;
public static UntypedInt CERT_FIND_NO_CTL_USAGE_FLAG => /* CERT_FIND_NO_ENHKEY_USAGE_FLAG */ 8;
public static UntypedInt CERT_FIND_OR_CTL_USAGE_FLAG => /* CERT_FIND_OR_ENHKEY_USAGE_FLAG */ 16;
public static UntypedInt CERT_FIND_VALID_CTL_USAGE_FLAG => /* CERT_FIND_VALID_ENHKEY_USAGE_FLAG */ 32;
public static UntypedInt CERT_CHAIN_POLICY_BASE => 1;
public static UntypedInt CERT_CHAIN_POLICY_AUTHENTICODE => 2;
public static UntypedInt CERT_CHAIN_POLICY_AUTHENTICODE_TS => 3;
public static UntypedInt CERT_CHAIN_POLICY_SSL => 4;
public static UntypedInt CERT_CHAIN_POLICY_BASIC_CONSTRAINTS => 5;
public static UntypedInt CERT_CHAIN_POLICY_NT_AUTH => 6;
public static UntypedInt CERT_CHAIN_POLICY_MICROSOFT_ROOT => 7;
public static UntypedInt CERT_CHAIN_POLICY_EV => 8;
public static UntypedInt CERT_CHAIN_POLICY_SSL_F12 => 9;
public static UntypedInt CERT_CHAIN_FIND_BY_ISSUER => 1;
public static UntypedInt CERT_CHAIN_FIND_BY_ISSUER_COMPARE_KEY_FLAG => 0x0001;
public static UntypedInt CERT_CHAIN_FIND_BY_ISSUER_COMPLEX_CHAIN_FLAG => 0x0002;
public static UntypedInt CERT_CHAIN_FIND_BY_ISSUER_CACHE_ONLY_URL_FLAG => 0x0004;
public static UntypedInt CERT_CHAIN_FIND_BY_ISSUER_LOCAL_MACHINE_FLAG => 0x0008;
public static UntypedInt CERT_CHAIN_FIND_BY_ISSUER_NO_KEY_FLAG => 0x4000;
public static UntypedInt CERT_CHAIN_FIND_BY_ISSUER_CACHE_ONLY_FLAG => 0x8000;
public static UntypedInt CERT_CLOSE_STORE_FORCE_FLAG => 0x00000001;
public static UntypedInt CERT_CLOSE_STORE_CHECK_FLAG => 0x00000002;
public static UntypedInt CERT_QUERY_OBJECT_FILE => 1;
public static UntypedInt CERT_QUERY_OBJECT_BLOB => 2;
public static UntypedInt CERT_QUERY_CONTENT_CERT => 1;
public static UntypedInt CERT_QUERY_CONTENT_CTL => 2;
public static UntypedInt CERT_QUERY_CONTENT_CRL => 3;
public static UntypedInt CERT_QUERY_CONTENT_SERIALIZED_STORE => 4;
public static UntypedInt CERT_QUERY_CONTENT_SERIALIZED_CERT => 5;
public static UntypedInt CERT_QUERY_CONTENT_SERIALIZED_CTL => 6;
public static UntypedInt CERT_QUERY_CONTENT_SERIALIZED_CRL => 7;
public static UntypedInt CERT_QUERY_CONTENT_PKCS7_SIGNED => 8;
public static UntypedInt CERT_QUERY_CONTENT_PKCS7_UNSIGNED => 9;
public static UntypedInt CERT_QUERY_CONTENT_PKCS7_SIGNED_EMBED => 10;
public static UntypedInt CERT_QUERY_CONTENT_PKCS10 => 11;
public static UntypedInt CERT_QUERY_CONTENT_PFX => 12;
public static UntypedInt CERT_QUERY_CONTENT_CERT_PAIR => 13;
public static UntypedInt CERT_QUERY_CONTENT_PFX_AND_LOAD => 14;
public static UntypedInt CERT_QUERY_CONTENT_FLAG_CERT => /* (1 << CERT_QUERY_CONTENT_CERT) */ 2;
public static UntypedInt CERT_QUERY_CONTENT_FLAG_CTL => /* (1 << CERT_QUERY_CONTENT_CTL) */ 4;
public static UntypedInt CERT_QUERY_CONTENT_FLAG_CRL => /* (1 << CERT_QUERY_CONTENT_CRL) */ 8;
public static UntypedInt CERT_QUERY_CONTENT_FLAG_SERIALIZED_STORE => /* (1 << CERT_QUERY_CONTENT_SERIALIZED_STORE) */ 16;
public static UntypedInt CERT_QUERY_CONTENT_FLAG_SERIALIZED_CERT => /* (1 << CERT_QUERY_CONTENT_SERIALIZED_CERT) */ 32;
public static UntypedInt CERT_QUERY_CONTENT_FLAG_SERIALIZED_CTL => /* (1 << CERT_QUERY_CONTENT_SERIALIZED_CTL) */ 64;
public static UntypedInt CERT_QUERY_CONTENT_FLAG_SERIALIZED_CRL => /* (1 << CERT_QUERY_CONTENT_SERIALIZED_CRL) */ 128;
public static UntypedInt CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED => /* (1 << CERT_QUERY_CONTENT_PKCS7_SIGNED) */ 256;
public static UntypedInt CERT_QUERY_CONTENT_FLAG_PKCS7_UNSIGNED => /* (1 << CERT_QUERY_CONTENT_PKCS7_UNSIGNED) */ 512;
public static UntypedInt CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED_EMBED => /* (1 << CERT_QUERY_CONTENT_PKCS7_SIGNED_EMBED) */ 1024;
public static UntypedInt CERT_QUERY_CONTENT_FLAG_PKCS10 => /* (1 << CERT_QUERY_CONTENT_PKCS10) */ 2048;
public static UntypedInt CERT_QUERY_CONTENT_FLAG_PFX => /* (1 << CERT_QUERY_CONTENT_PFX) */ 4096;
public static UntypedInt CERT_QUERY_CONTENT_FLAG_CERT_PAIR => /* (1 << CERT_QUERY_CONTENT_CERT_PAIR) */ 8192;
public static UntypedInt CERT_QUERY_CONTENT_FLAG_PFX_AND_LOAD => /* (1 << CERT_QUERY_CONTENT_PFX_AND_LOAD) */ 16384;
public static UntypedInt CERT_QUERY_CONTENT_FLAG_ALL => /* (CERT_QUERY_CONTENT_FLAG_CERT | CERT_QUERY_CONTENT_FLAG_CTL | CERT_QUERY_CONTENT_FLAG_CRL | CERT_QUERY_CONTENT_FLAG_SERIALIZED_STORE | CERT_QUERY_CONTENT_FLAG_SERIALIZED_CERT | CERT_QUERY_CONTENT_FLAG_SERIALIZED_CTL | CERT_QUERY_CONTENT_FLAG_SERIALIZED_CRL | CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED | CERT_QUERY_CONTENT_FLAG_PKCS7_UNSIGNED | CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED_EMBED | CERT_QUERY_CONTENT_FLAG_PKCS10 | CERT_QUERY_CONTENT_FLAG_PFX | CERT_QUERY_CONTENT_FLAG_CERT_PAIR) */ 16382;
public static UntypedInt CERT_QUERY_CONTENT_FLAG_ALL_ISSUER_CERT => /* (CERT_QUERY_CONTENT_FLAG_CERT | CERT_QUERY_CONTENT_FLAG_SERIALIZED_STORE | CERT_QUERY_CONTENT_FLAG_SERIALIZED_CERT | CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED | CERT_QUERY_CONTENT_FLAG_PKCS7_UNSIGNED) */ 818;
public static UntypedInt CERT_QUERY_FORMAT_BINARY => 1;
public static UntypedInt CERT_QUERY_FORMAT_BASE64_ENCODED => 2;
public static UntypedInt CERT_QUERY_FORMAT_ASN_ASCII_HEX_ENCODED => 3;
public static UntypedInt CERT_QUERY_FORMAT_FLAG_BINARY => /* (1 << CERT_QUERY_FORMAT_BINARY) */ 2;
public static UntypedInt CERT_QUERY_FORMAT_FLAG_BASE64_ENCODED => /* (1 << CERT_QUERY_FORMAT_BASE64_ENCODED) */ 4;
public static UntypedInt CERT_QUERY_FORMAT_FLAG_ASN_ASCII_HEX_ENCODED => /* (1 << CERT_QUERY_FORMAT_ASN_ASCII_HEX_ENCODED) */ 8;
public static UntypedInt CERT_QUERY_FORMAT_FLAG_ALL => /* (CERT_QUERY_FORMAT_FLAG_BINARY | CERT_QUERY_FORMAT_FLAG_BASE64_ENCODED | CERT_QUERY_FORMAT_FLAG_ASN_ASCII_HEX_ENCODED) */ 14;
public static UntypedInt CERT_NAME_EMAIL_TYPE => 1;
public static UntypedInt CERT_NAME_RDN_TYPE => 2;
public static UntypedInt CERT_NAME_ATTR_TYPE => 3;
public static UntypedInt CERT_NAME_SIMPLE_DISPLAY_TYPE => 4;
public static UntypedInt CERT_NAME_FRIENDLY_DISPLAY_TYPE => 5;
public static UntypedInt CERT_NAME_DNS_TYPE => 6;
public static UntypedInt CERT_NAME_URL_TYPE => 7;
public static UntypedInt CERT_NAME_UPN_TYPE => 8;
public static UntypedInt CERT_NAME_ISSUER_FLAG => 0x1;
public static UntypedInt CERT_NAME_DISABLE_IE4_UTF8_FLAG => 0x10000;
public static UntypedInt CERT_NAME_SEARCH_ALL_NAMES_FLAG => 0x2;
public static UntypedInt CERT_NAME_STR_ENABLE_PUNYCODE_FLAG => 0x00200000;
public static UntypedInt AUTHTYPE_CLIENT => 1;
public static UntypedInt AUTHTYPE_SERVER => 2;
public static UntypedInt SECURITY_FLAG_IGNORE_REVOCATION => 0x00000080;
public static UntypedInt SECURITY_FLAG_IGNORE_UNKNOWN_CA => 0x00000100;
public static UntypedInt SECURITY_FLAG_IGNORE_WRONG_USAGE => 0x00000200;
public static UntypedInt SECURITY_FLAG_IGNORE_CERT_CN_INVALID => 0x00001000;
public static UntypedInt SECURITY_FLAG_IGNORE_CERT_DATE_INVALID => 0x00002000;
public static UntypedInt CRYPTPROTECT_UI_FORBIDDEN => 0x1;
public static UntypedInt CRYPTPROTECT_LOCAL_MACHINE => 0x4;
public static UntypedInt CRYPTPROTECT_CRED_SYNC => 0x8;
public static UntypedInt CRYPTPROTECT_AUDIT => 0x10;
public static UntypedInt CRYPTPROTECT_NO_RECOVERY => 0x20;
public static UntypedInt CRYPTPROTECT_VERIFY_PROTECTION => 0x40;
public static UntypedInt CRYPTPROTECT_CRED_REGENERATE => 0x80;
public static UntypedInt CRYPTPROTECT_PROMPT_ON_UNPROTECT => 1;
public static UntypedInt CRYPTPROTECT_PROMPT_ON_PROTECT => 2;
public static UntypedInt CRYPTPROTECT_PROMPT_RESERVED => 4;
public static UntypedInt CRYPTPROTECT_PROMPT_STRONG => 8;
public static UntypedInt CRYPTPROTECT_PROMPT_REQUIRE_STRONG => 16;

public static UntypedInt SEM_FAILCRITICALERRORS => 0x0001;
public static UntypedInt SEM_NOALIGNMENTFAULTEXCEPT => 0x0004;
public static UntypedInt SEM_NOGPFAULTERRORBOX => 0x0002;
public static UntypedInt SEM_NOOPENFILEERRORBOX => 0x8000;

public static UntypedInt ABOVE_NORMAL_PRIORITY_CLASS => 0x00008000;
public static UntypedInt BELOW_NORMAL_PRIORITY_CLASS => 0x00004000;
public static UntypedInt HIGH_PRIORITY_CLASS => 0x00000080;
public static UntypedInt IDLE_PRIORITY_CLASS => 0x00000040;
public static UntypedInt NORMAL_PRIORITY_CLASS => 0x00000020;
public static UntypedInt PROCESS_MODE_BACKGROUND_BEGIN => 0x00100000;
public static UntypedInt PROCESS_MODE_BACKGROUND_END => 0x00200000;
public static UntypedInt REALTIME_PRIORITY_CLASS => 0x00000100;

public static UntypedInt WTD_UI_ALL => 1;
public static UntypedInt WTD_UI_NONE => 2;
public static UntypedInt WTD_UI_NOBAD => 3;
public static UntypedInt WTD_UI_NOGOOD => 4;
public static UntypedInt WTD_REVOKE_NONE => 0;
public static UntypedInt WTD_REVOKE_WHOLECHAIN => 1;
public static UntypedInt WTD_CHOICE_FILE => 1;
public static UntypedInt WTD_CHOICE_CATALOG => 2;
public static UntypedInt WTD_CHOICE_BLOB => 3;
public static UntypedInt WTD_CHOICE_SIGNER => 4;
public static UntypedInt WTD_CHOICE_CERT => 5;
public static UntypedInt WTD_STATEACTION_IGNORE => 0x00000000;
public static UntypedInt WTD_STATEACTION_VERIFY => 0x00000001;
public static UntypedInt WTD_STATEACTION_CLOSE => 0x00000002;
public static UntypedInt WTD_STATEACTION_AUTO_CACHE => 0x00000003;
public static UntypedInt WTD_STATEACTION_AUTO_CACHE_FLUSH => 0x00000004;
public static UntypedInt WTD_USE_IE4_TRUST_FLAG => 0x1;
public static UntypedInt WTD_NO_IE4_CHAIN_FLAG => 0x2;
public static UntypedInt WTD_NO_POLICY_USAGE_FLAG => 0x4;
public static UntypedInt WTD_REVOCATION_CHECK_NONE => 0x10;
public static UntypedInt WTD_REVOCATION_CHECK_END_CERT => 0x20;
public static UntypedInt WTD_REVOCATION_CHECK_CHAIN => 0x40;
public static UntypedInt WTD_REVOCATION_CHECK_CHAIN_EXCLUDE_ROOT => 0x80;
public static UntypedInt WTD_SAFER_FLAG => 0x100;
public static UntypedInt WTD_HASH_ONLY_FLAG => 0x200;
public static UntypedInt WTD_USE_DEFAULT_OSVER_CHECK => 0x400;
public static UntypedInt WTD_LIFETIME_SIGNING_FLAG => 0x800;
public static UntypedInt WTD_CACHE_ONLY_URL_RETRIEVAL => 0x1000;
public static UntypedInt WTD_DISABLE_MD2_MD4 => 0x2000;
public static UntypedInt WTD_MOTW => 0x4000;
public static UntypedInt WTD_UICONTEXT_EXECUTE => 0;
public static UntypedInt WTD_UICONTEXT_INSTALL => 1;

public static readonly ж<slice<byte>> ᏑOID_PKIX_KP_SERVER_AUTH = new StaticBox<slice<byte>>(slice<byte>("1.3.6.1.5.5.7.3.1\x00"u8));
public static ref slice<byte> OID_PKIX_KP_SERVER_AUTH => ref ((StaticBox<slice<byte>>)ᏑOID_PKIX_KP_SERVER_AUTH).ValueSlot;
public static readonly ж<slice<byte>> ᏑOID_SERVER_GATED_CRYPTO = new StaticBox<slice<byte>>(slice<byte>("1.3.6.1.4.1.311.10.3.3\x00"u8));
public static ref slice<byte> OID_SERVER_GATED_CRYPTO => ref ((StaticBox<slice<byte>>)ᏑOID_SERVER_GATED_CRYPTO).ValueSlot;
public static readonly ж<slice<byte>> ᏑOID_SGC_NETSCAPE = new StaticBox<slice<byte>>(slice<byte>("2.16.840.1.113730.4.1\x00"u8));
public static ref slice<byte> OID_SGC_NETSCAPE => ref ((StaticBox<slice<byte>>)ᏑOID_SGC_NETSCAPE).ValueSlot;
[GoLazy] partial class lazyᴛWINTRUST_ACTION_GENERIC_VERIFY_V2 { public static readonly ж<GUID> ᏑWINTRUST_ACTION_GENERIC_VERIFY_V2 = new StaticBox<GUID>(new GUID(
    Data1: 0xaac56b,
    Data2: 0xcd44,
    Data3: 0x11d0,
    Data4: new array8<byte>(0x8c, 0xc2, 0x0, 0xc0, 0x4f, 0xc2, 0x95, 0xee)
)); }
public static ref GUID WINTRUST_ACTION_GENERIC_VERIFY_V2 => ref ((StaticBox<GUID>)ᏑWINTRUST_ACTION_GENERIC_VERIFY_V2).ValueSlot;

partial class Pointer /*ж<EmptyStruct>*/;

partial struct Timeval {
    public int32 Sec;
    public int32 Usec;
}

} partial class windowsᴛTimeval {

public static int64 Nanoseconds(this ref Timeval tv) {
    return ((int64)tv.Sec * 1000000 + (int64)tv.Usec) * 1000;
}

} partial class windows_package {

public static Timeval /*tv*/ NsecToTimeval(int64 nsec) {
    Timeval tv = default;

    tv.Sec = (int32)(nsec / 1000000000);
    tv.Usec = (int32)(nsec % 1000000000 / 1000);
    return tv;
}

partial struct Overlapped {
    public uintptr Internal;
    public uintptr InternalHigh;
    public uint32 Offset;
    public uint32 OffsetHigh;
    public Handle HEvent;
}

partial struct FileNotifyInformation {
    public uint32 NextEntryOffset;
    public uint32 Action;
    public uint32 FileNameLength;
    public uint16 FileName;
}

partial struct Filetime {
    public uint32 LowDateTime;
    public uint32 HighDateTime;
}

} partial class windowsᴛFiletime {

public static int64 Nanoseconds(this ref Filetime ft) {
    var nsec = ((int64)ft.HighDateTime << 32) + (int64)ft.LowDateTime;
    nsec -= 116444736000000000L;
    nsec *= 100;
    return nsec;
}

} partial class windows_package {

public static Filetime /*ft*/ NsecToFiletime(int64 nsec) {
    Filetime ft = default;

    nsec /= 100;
    nsec += 116444736000000000L;
    ft.LowDateTime = (uint32)(nsec & 0xffffffffL);
    ft.HighDateTime = (uint32)(nsec >> 32 & 0xffffffffL);
    return ft;
}

partial struct Win32finddata {
    public uint32 FileAttributes;
    public Filetime CreationTime;
    public Filetime LastAccessTime;
    public Filetime LastWriteTime;
    public uint32 FileSizeHigh;
    public uint32 FileSizeLow;
    public uint32 Reserved0;
    public uint32 Reserved1;
    public array<uint16> FileName = new(MAX_PATH - 1);
    public array<uint16> AlternateFileName = new(13);
}

partial struct win32finddata1 {
    public uint32 FileAttributes;
    public Filetime CreationTime;
    public Filetime LastAccessTime;
    public Filetime LastWriteTime;
    public uint32 FileSizeHigh;
    public uint32 FileSizeLow;
    public uint32 Reserved0;
    public uint32 Reserved1;
    public array<uint16> FileName = new(MAX_PATH);
    public array<uint16> AlternateFileName = new(14);
}

internal static void copyFindData(ref Win32finddata dst, ref win32finddata1 src) {
    dst.FileAttributes = src.FileAttributes;
    dst.CreationTime = src.CreationTime;
    dst.LastAccessTime = src.LastAccessTime;
    dst.LastWriteTime = src.LastWriteTime;
    dst.FileSizeHigh = src.FileSizeHigh;
    dst.FileSizeLow = src.FileSizeLow;
    dst.Reserved0 = src.Reserved0;
    dst.Reserved1 = src.Reserved1;

    copy(dst.FileName[..], src.FileName[..]);
    copy(dst.AlternateFileName[..], src.AlternateFileName[..]);
}

partial struct ByHandleFileInformation {
    public uint32 FileAttributes;
    public Filetime CreationTime;
    public Filetime LastAccessTime;
    public Filetime LastWriteTime;
    public uint32 VolumeSerialNumber;
    public uint32 FileSizeHigh;
    public uint32 FileSizeLow;
    public uint32 NumberOfLinks;
    public uint32 FileIndexHigh;
    public uint32 FileIndexLow;
}

public static UntypedInt GetFileExInfoStandard => 0;
public static UntypedInt GetFileExMaxInfoLevel => 1;

partial struct Win32FileAttributeData {
    public uint32 FileAttributes;
    public Filetime CreationTime;
    public Filetime LastAccessTime;
    public Filetime LastWriteTime;
    public uint32 FileSizeHigh;
    public uint32 FileSizeLow;
}

public static UntypedInt SW_HIDE => 0;
public static UntypedInt SW_NORMAL => 1;
public static UntypedInt SW_SHOWNORMAL => 1;
public static UntypedInt SW_SHOWMINIMIZED => 2;
public static UntypedInt SW_SHOWMAXIMIZED => 3;
public static UntypedInt SW_MAXIMIZE => 3;
public static UntypedInt SW_SHOWNOACTIVATE => 4;
public static UntypedInt SW_SHOW => 5;
public static UntypedInt SW_MINIMIZE => 6;
public static UntypedInt SW_SHOWMINNOACTIVE => 7;
public static UntypedInt SW_SHOWNA => 8;
public static UntypedInt SW_RESTORE => 9;
public static UntypedInt SW_SHOWDEFAULT => 10;
public static UntypedInt SW_FORCEMINIMIZE => 11;

partial struct StartupInfo {
    public uint32 Cb;
    internal ж<uint16> _;
    public ж<uint16> Desktop;
    public ж<uint16> Title;
    public uint32 X;
    public uint32 Y;
    public uint32 XSize;
    public uint32 YSize;
    public uint32 XCountChars;
    public uint32 YCountChars;
    public uint32 FillAttribute;
    public uint32 Flags;
    public uint16 ShowWindow;
    internal uint16 __;
    internal ж<byte> ___;
    public Handle StdInput;
    public Handle StdOutput;
    public Handle StdErr;
}

partial struct StartupInfoEx {
    /*embed*/ public StartupInfo StartupInfo;
    public ж<ProcThreadAttributeList> ProcThreadAttributeList;
}

partial struct ProcThreadAttributeList {
}

partial struct ProcThreadAttributeListContainer {
    internal ж<ProcThreadAttributeList> data;
    internal slice<@unsafe.Pointer> pointers;
}

partial struct ProcessInformation {
    public Handle Process;
    public Handle Thread;
    public uint32 ProcessId;
    public uint32 ThreadId;
}

[global::go.golib.GoInlineLayout(Size = 568, Align = 8), StructLayout(LayoutKind.Explicit, Size = 568)] partial struct ProcessEntry32 {
    [FieldOffset(0)] public uint32 Size;
    [FieldOffset(4)] public uint32 Usage;
    [FieldOffset(8)] public uint32 ProcessID;
    [FieldOffset(16)] public uintptr DefaultHeapID;
    [FieldOffset(24)] public uint32 ModuleID;
    [FieldOffset(28)] public uint32 Threads;
    [FieldOffset(32)] public uint32 ParentProcessID;
    [FieldOffset(36)] public int32 PriClassBase;
    [FieldOffset(40)] public uint32 Flags;
    [FieldOffset(44)] public array260<uint16> ExeFile;
}

partial struct ThreadEntry32 {
    public uint32 Size;
    public uint32 Usage;
    public uint32 ThreadID;
    public uint32 OwnerProcessID;
    public int32 BasePri;
    public int32 DeltaPri;
    public uint32 Flags;
}

[global::go.golib.GoInlineLayout(Size = 1080, Align = 8), StructLayout(LayoutKind.Explicit, Size = 1080)] partial struct ModuleEntry32 {
    [FieldOffset(0)] public uint32 Size;
    [FieldOffset(4)] public uint32 ModuleID;
    [FieldOffset(8)] public uint32 ProcessID;
    [FieldOffset(12)] public uint32 GlblcntUsage;
    [FieldOffset(16)] public uint32 ProccntUsage;
    [FieldOffset(24)] public uintptr ModBaseAddr;
    [FieldOffset(32)] public uint32 ModBaseSize;
    [FieldOffset(40)] public Handle ModuleHandle;
    [FieldOffset(48)] public array256<uint16> Module;
    [FieldOffset(560)] public array260<uint16> ExePath;
}

public static uintptr SizeofModuleEntry32 => /* unsafe.Sizeof(ModuleEntry32{}) */ 1080;

[global::go.golib.GoInlineLayout(Size = 16, Align = 2), StructLayout(LayoutKind.Explicit, Size = 16)] partial struct Systemtime {
    [FieldOffset(0)] public uint16 Year;
    [FieldOffset(2)] public uint16 Month;
    [FieldOffset(4)] public uint16 DayOfWeek;
    [FieldOffset(6)] public uint16 Day;
    [FieldOffset(8)] public uint16 Hour;
    [FieldOffset(10)] public uint16 Minute;
    [FieldOffset(12)] public uint16 Second;
    [FieldOffset(14)] public uint16 Milliseconds;
}

[global::go.golib.GoInlineLayout(Size = 172, Align = 4), StructLayout(LayoutKind.Explicit, Size = 172)] partial struct Timezoneinformation {
    [FieldOffset(0)] public int32 Bias;
    [FieldOffset(4)] public array32<uint16> StandardName;
    [FieldOffset(68)] public Systemtime StandardDate;
    [FieldOffset(84)] public int32 StandardBias;
    [FieldOffset(88)] public array32<uint16> DaylightName;
    [FieldOffset(152)] public Systemtime DaylightDate;
    [FieldOffset(168)] public int32 DaylightBias;
}

public static UntypedInt AF_UNSPEC => 0;
public static UntypedInt AF_UNIX => 1;
public static UntypedInt AF_INET => 2;
public static UntypedInt AF_NETBIOS => 17;
public static UntypedInt AF_INET6 => 23;
public static UntypedInt AF_IRDA => 26;
public static UntypedInt AF_BTH => 32;
public static UntypedInt SOCK_STREAM => 1;
public static UntypedInt SOCK_DGRAM => 2;
public static UntypedInt SOCK_RAW => 3;
public static UntypedInt SOCK_RDM => 4;
public static UntypedInt SOCK_SEQPACKET => 5;
public static UntypedInt IPPROTO_IP => 0;
public static UntypedInt IPPROTO_ICMP => 1;
public static UntypedInt IPPROTO_IGMP => 2;
public static UntypedInt BTHPROTO_RFCOMM => 3;
public static UntypedInt IPPROTO_TCP => 6;
public static UntypedInt IPPROTO_UDP => 17;
public static UntypedInt IPPROTO_IPV6 => 41;
public static UntypedInt IPPROTO_ICMPV6 => 58;
public static UntypedInt IPPROTO_RM => 113;
public static UntypedInt SOL_SOCKET => 0xffff;
public static UntypedInt SO_REUSEADDR => 4;
public static UntypedInt SO_KEEPALIVE => 8;
public static UntypedInt SO_DONTROUTE => 16;
public static UntypedInt SO_BROADCAST => 32;
public static UntypedInt SO_LINGER => 128;
public static UntypedInt SO_RCVBUF => 0x1002;
public static UntypedInt SO_SNDTIMEO => 0x1005;
public static UntypedInt SO_RCVTIMEO => 0x1006;
public static UntypedInt SO_SNDBUF => 0x1001;
public static UntypedInt SO_UPDATE_ACCEPT_CONTEXT => 0x700b;
public static UntypedInt SO_UPDATE_CONNECT_CONTEXT => 0x7010;
public static UntypedInt IOC_OUT => 0x40000000;
public static UntypedInt IOC_IN => 0x80000000;
public static UntypedInt IOC_VENDOR => 0x18000000;
public static UntypedInt IOC_INOUT => /* IOC_IN | IOC_OUT */ 3221225472;
public static UntypedInt IOC_WS2 => 0x08000000;
public static UntypedInt SIO_GET_EXTENSION_FUNCTION_POINTER => /* IOC_INOUT | IOC_WS2 | 6 */ 3355443206;
public static UntypedInt SIO_KEEPALIVE_VALS => /* IOC_IN | IOC_VENDOR | 4 */ 2550136836;
public static UntypedInt SIO_UDP_CONNRESET => /* IOC_IN | IOC_VENDOR | 12 */ 2550136844;
public static UntypedInt SIO_UDP_NETRESET => /* IOC_IN | IOC_VENDOR | 15 */ 2550136847;
public static UntypedInt IP_HDRINCL => 0x2;
public static UntypedInt IP_TOS => 0x3;
public static UntypedInt IP_TTL => 0x4;
public static UntypedInt IP_MULTICAST_IF => 0x9;
public static UntypedInt IP_MULTICAST_TTL => 0xa;
public static UntypedInt IP_MULTICAST_LOOP => 0xb;
public static UntypedInt IP_ADD_MEMBERSHIP => 0xc;
public static UntypedInt IP_DROP_MEMBERSHIP => 0xd;
public static UntypedInt IP_PKTINFO => 0x13;
public static UntypedInt IP_MTU_DISCOVER => 0x47;
public static UntypedInt IPV6_V6ONLY => 0x1b;
public static UntypedInt IPV6_UNICAST_HOPS => 0x4;
public static UntypedInt IPV6_MULTICAST_IF => 0x9;
public static UntypedInt IPV6_MULTICAST_HOPS => 0xa;
public static UntypedInt IPV6_MULTICAST_LOOP => 0xb;
public static UntypedInt IPV6_JOIN_GROUP => 0xc;
public static UntypedInt IPV6_LEAVE_GROUP => 0xd;
public static UntypedInt IPV6_PKTINFO => 0x13;
public static UntypedInt IPV6_MTU_DISCOVER => 0x47;
public static UntypedInt MSG_OOB => 0x1;
public static UntypedInt MSG_PEEK => 0x2;
public static UntypedInt MSG_DONTROUTE => 0x4;
public static UntypedInt MSG_WAITALL => 0x8;
public static UntypedInt MSG_TRUNC => 0x0100;
public static UntypedInt MSG_CTRUNC => 0x0200;
public static UntypedInt MSG_BCAST => 0x0400;
public static UntypedInt MSG_MCAST => 0x0800;
public static UntypedInt SOMAXCONN => 0x7fffffff;
public static UntypedInt TCP_NODELAY => 1;
public static UntypedInt TCP_EXPEDITED_1122 => 2;
public static UntypedInt TCP_KEEPALIVE => 3;
public static UntypedInt TCP_MAXSEG => 4;
public static UntypedInt TCP_MAXRT => 5;
public static UntypedInt TCP_STDURG => 6;
public static UntypedInt TCP_NOURG => 7;
public static UntypedInt TCP_ATMARK => 8;
public static UntypedInt TCP_NOSYNRETRIES => 9;
public static UntypedInt TCP_TIMESTAMPS => 10;
public static UntypedInt TCP_OFFLOAD_PREFERENCE => 11;
public static UntypedInt TCP_CONGESTION_ALGORITHM => 12;
public static UntypedInt TCP_DELAY_FIN_ACK => 13;
public static UntypedInt TCP_MAXRTMS => 14;
public static UntypedInt TCP_FASTOPEN => 15;
public static UntypedInt TCP_KEEPCNT => 16;
public static UntypedInt TCP_KEEPIDLE => /* TCP_KEEPALIVE */ 3;
public static UntypedInt TCP_KEEPINTVL => 17;
public static UntypedInt TCP_FAIL_CONNECT_ON_ICMP_ERROR => 18;
public static UntypedInt TCP_ICMP_ERROR_INFO => 19;
public static UntypedInt UDP_NOCHECKSUM => 1;
public static UntypedInt UDP_SEND_MSG_SIZE => 2;
public static UntypedInt UDP_RECV_MAX_COALESCED_SIZE => 3;
public static UntypedInt UDP_CHECKSUM_COVERAGE => 20;
public static UntypedInt UDP_COALESCED_INFO => 3;
public static UntypedInt SHUT_RD => 0;
public static UntypedInt SHUT_WR => 1;
public static UntypedInt SHUT_RDWR => 2;
public static UntypedInt WSADESCRIPTION_LEN => 256;
public static UntypedInt WSASYS_STATUS_LEN => 128;

public static UntypedInt IP_PMTUDISC_NOT_SET => 0;
public static UntypedInt IP_PMTUDISC_DO => 1;
public static UntypedInt IP_PMTUDISC_DONT => 2;
public static UntypedInt IP_PMTUDISC_PROBE => 3;
public static UntypedInt IP_PMTUDISC_MAX => 4;

partial struct WSABuf {
    public uint32 Len;
    public ж<byte> Buf;
}

partial struct WSAMsg {
    public ж<syscall.RawSockaddrAny> Name;
    public int32 Namelen;
    public ж<WSABuf> Buffers;
    public uint32 BufferCount;
    public WSABuf Control;
    public uint32 Flags;
}

partial struct WSACMSGHDR {
    public uintptr Len;
    public int32 Level;
    public int32 Type;
}

partial struct IN_PKTINFO {
    public array<byte> Addr = new(4);
    public uint32 Ifindex;
}

partial struct IN6_PKTINFO {
    public array<byte> Addr = new(16);
    public uint32 Ifindex;
}

public static UntypedInt WSA_FLAG_OVERLAPPED => 0x01;
public static UntypedInt WSA_FLAG_MULTIPOINT_C_ROOT => 0x02;
public static UntypedInt WSA_FLAG_MULTIPOINT_C_LEAF => 0x04;
public static UntypedInt WSA_FLAG_MULTIPOINT_D_ROOT => 0x08;
public static UntypedInt WSA_FLAG_MULTIPOINT_D_LEAF => 0x10;
public static UntypedInt WSA_FLAG_ACCESS_SYSTEM_SECURITY => 0x40;
public static UntypedInt WSA_FLAG_NO_HANDLE_INHERIT => 0x80;
public static UntypedInt WSA_FLAG_REGISTERED_IO => 0x100;

public static UntypedInt S_IFMT => 0x1f000;
public static UntypedInt S_IFIFO => 0x1000;
public static UntypedInt S_IFCHR => 0x2000;
public static UntypedInt S_IFDIR => 0x4000;
public static UntypedInt S_IFBLK => 0x6000;
public static UntypedInt S_IFREG => 0x8000;
public static UntypedInt S_IFLNK => 0xa000;
public static UntypedInt S_IFSOCK => 0xc000;
public static UntypedInt S_ISUID => 0x800;
public static UntypedInt S_ISGID => 0x400;
public static UntypedInt S_ISVTX => 0x200;
public static UntypedInt S_IRUSR => 0x100;
public static UntypedInt S_IWRITE => 0x80;
public static UntypedInt S_IWUSR => 0x80;
public static UntypedInt S_IXUSR => 0x40;

public static UntypedInt FILE_TYPE_CHAR => 0x0002;
public static UntypedInt FILE_TYPE_DISK => 0x0001;
public static UntypedInt FILE_TYPE_PIPE => 0x0003;
public static UntypedInt FILE_TYPE_REMOTE => 0x8000;
public static UntypedInt FILE_TYPE_UNKNOWN => 0x0000;

partial struct Hostent {
    public ж<byte> Name;
    public ж<ж<byte>> Aliases;
    public uint16 AddrType;
    public uint16 Length;
    public ж<ж<byte>> AddrList;
}

partial struct Protoent {
    public ж<byte> Name;
    public ж<ж<byte>> Aliases;
    public uint16 Proto;
}

public static UntypedInt DNS_TYPE_A => 0x0001;
public static UntypedInt DNS_TYPE_NS => 0x0002;
public static UntypedInt DNS_TYPE_MD => 0x0003;
public static UntypedInt DNS_TYPE_MF => 0x0004;
public static UntypedInt DNS_TYPE_CNAME => 0x0005;
public static UntypedInt DNS_TYPE_SOA => 0x0006;
public static UntypedInt DNS_TYPE_MB => 0x0007;
public static UntypedInt DNS_TYPE_MG => 0x0008;
public static UntypedInt DNS_TYPE_MR => 0x0009;
public static UntypedInt DNS_TYPE_NULL => 0x000a;
public static UntypedInt DNS_TYPE_WKS => 0x000b;
public static UntypedInt DNS_TYPE_PTR => 0x000c;
public static UntypedInt DNS_TYPE_HINFO => 0x000d;
public static UntypedInt DNS_TYPE_MINFO => 0x000e;
public static UntypedInt DNS_TYPE_MX => 0x000f;
public static UntypedInt DNS_TYPE_TEXT => 0x0010;
public static UntypedInt DNS_TYPE_RP => 0x0011;
public static UntypedInt DNS_TYPE_AFSDB => 0x0012;
public static UntypedInt DNS_TYPE_X25 => 0x0013;
public static UntypedInt DNS_TYPE_ISDN => 0x0014;
public static UntypedInt DNS_TYPE_RT => 0x0015;
public static UntypedInt DNS_TYPE_NSAP => 0x0016;
public static UntypedInt DNS_TYPE_NSAPPTR => 0x0017;
public static UntypedInt DNS_TYPE_SIG => 0x0018;
public static UntypedInt DNS_TYPE_KEY => 0x0019;
public static UntypedInt DNS_TYPE_PX => 0x001a;
public static UntypedInt DNS_TYPE_GPOS => 0x001b;
public static UntypedInt DNS_TYPE_AAAA => 0x001c;
public static UntypedInt DNS_TYPE_LOC => 0x001d;
public static UntypedInt DNS_TYPE_NXT => 0x001e;
public static UntypedInt DNS_TYPE_EID => 0x001f;
public static UntypedInt DNS_TYPE_NIMLOC => 0x0020;
public static UntypedInt DNS_TYPE_SRV => 0x0021;
public static UntypedInt DNS_TYPE_ATMA => 0x0022;
public static UntypedInt DNS_TYPE_NAPTR => 0x0023;
public static UntypedInt DNS_TYPE_KX => 0x0024;
public static UntypedInt DNS_TYPE_CERT => 0x0025;
public static UntypedInt DNS_TYPE_A6 => 0x0026;
public static UntypedInt DNS_TYPE_DNAME => 0x0027;
public static UntypedInt DNS_TYPE_SINK => 0x0028;
public static UntypedInt DNS_TYPE_OPT => 0x0029;
public static UntypedInt DNS_TYPE_DS => 0x002B;
public static UntypedInt DNS_TYPE_RRSIG => 0x002E;
public static UntypedInt DNS_TYPE_NSEC => 0x002F;
public static UntypedInt DNS_TYPE_DNSKEY => 0x0030;
public static UntypedInt DNS_TYPE_DHCID => 0x0031;
public static UntypedInt DNS_TYPE_UINFO => 0x0064;
public static UntypedInt DNS_TYPE_UID => 0x0065;
public static UntypedInt DNS_TYPE_GID => 0x0066;
public static UntypedInt DNS_TYPE_UNSPEC => 0x0067;
public static UntypedInt DNS_TYPE_ADDRS => 0x00f8;
public static UntypedInt DNS_TYPE_TKEY => 0x00f9;
public static UntypedInt DNS_TYPE_TSIG => 0x00fa;
public static UntypedInt DNS_TYPE_IXFR => 0x00fb;
public static UntypedInt DNS_TYPE_AXFR => 0x00fc;
public static UntypedInt DNS_TYPE_MAILB => 0x00fd;
public static UntypedInt DNS_TYPE_MAILA => 0x00fe;
public static UntypedInt DNS_TYPE_ALL => 0x00ff;
public static UntypedInt DNS_TYPE_ANY => 0x00ff;
public static UntypedInt DNS_TYPE_WINS => 0xff01;
public static UntypedInt DNS_TYPE_WINSR => 0xff02;
public static UntypedInt DNS_TYPE_NBSTAT => 0xff01;

public static UntypedInt DnsSectionQuestion => 0x0000;
public static UntypedInt DnsSectionAnswer => 0x0001;
public static UntypedInt DnsSectionAuthority => 0x0002;
public static UntypedInt DnsSectionAdditional => 0x0003;

public static UntypedInt LUP_DEEP => 0x0001;
public static UntypedInt LUP_CONTAINERS => 0x0002;
public static UntypedInt LUP_NOCONTAINERS => 0x0004;
public static UntypedInt LUP_NEAREST => 0x0008;
public static UntypedInt LUP_RETURN_NAME => 0x0010;
public static UntypedInt LUP_RETURN_TYPE => 0x0020;
public static UntypedInt LUP_RETURN_VERSION => 0x0040;
public static UntypedInt LUP_RETURN_COMMENT => 0x0080;
public static UntypedInt LUP_RETURN_ADDR => 0x0100;
public static UntypedInt LUP_RETURN_BLOB => 0x0200;
public static UntypedInt LUP_RETURN_ALIASES => 0x0400;
public static UntypedInt LUP_RETURN_QUERY_STRING => 0x0800;
public static UntypedInt LUP_RETURN_ALL => 0x0FF0;
public static UntypedInt LUP_RES_SERVICE => 0x8000;
public static UntypedInt LUP_FLUSHCACHE => 0x1000;
public static UntypedInt LUP_FLUSHPREVIOUS => 0x2000;
public static UntypedInt LUP_NON_AUTHORITATIVE => 0x4000;
public static UntypedInt LUP_SECURE => 0x8000;
public static UntypedInt LUP_RETURN_PREFERRED_NAMES => 0x10000;
public static UntypedInt LUP_DNS_ONLY => 0x20000;
public static UntypedInt LUP_ADDRCONFIG => 0x100000;
public static UntypedInt LUP_DUAL_ADDR => 0x200000;
public static UntypedInt LUP_FILESERVER => 0x400000;
public static UntypedInt LUP_DISABLE_IDN_ENCODING => 0x00800000;
public static UntypedInt LUP_API_ANSI => 0x01000000;
public static UntypedInt LUP_RESOLUTION_HANDLE => 0x80000000;

public static UntypedInt NS_ALL => 0;
public static UntypedInt NS_DNS => 12;
public static UntypedInt NS_NLA => 15;
public static UntypedInt NS_BTH => 16;
public static UntypedInt NS_EMAIL => 37;
public static UntypedInt NS_PNRPNAME => 38;
public static UntypedInt NS_PNRPCLOUD => 39;

partial struct DNSSRVData {
    public ж<uint16> Target;
    public uint16 Priority;
    public uint16 Weight;
    public uint16 Port;
    public uint16 Pad;
}

partial struct DNSPTRData {
    public ж<uint16> Host;
}

partial struct DNSMXData {
    public ж<uint16> NameExchange;
    public uint16 Preference;
    public uint16 Pad;
}

partial struct DNSTXTData {
    public uint16 StringCount;
    public array<ж<uint16>> StringArray = new(1);
}

partial struct DNSRecord {
    public ж<DNSRecord> Next;
    public ж<uint16> Name;
    public uint16 Type;
    public uint16 Length;
    public uint32 Dw;
    public uint32 Ttl;
    public uint32 Reserved;
    public array<byte> Data = new(40);
}

public static UntypedInt TF_DISCONNECT => 1;
public static UntypedInt TF_REUSE_SOCKET => 2;
public static UntypedInt TF_WRITE_BEHIND => 4;
public static UntypedInt TF_USE_DEFAULT_WORKER => 0;
public static UntypedInt TF_USE_SYSTEM_THREAD => 16;
public static UntypedInt TF_USE_KERNEL_APC => 32;

partial struct TransmitFileBuffers {
    public uintptr Head;
    public uint32 HeadLength;
    public uintptr Tail;
    public uint32 TailLength;
}

public static UntypedInt IFF_UP => 1;
public static UntypedInt IFF_BROADCAST => 2;
public static UntypedInt IFF_LOOPBACK => 4;
public static UntypedInt IFF_POINTTOPOINT => 8;
public static UntypedInt IFF_MULTICAST => 16;

public static UntypedInt SIO_GET_INTERFACE_LIST => 0x4004747F;

partial struct SockaddrGen /*[24]byte*/;

partial struct InterfaceInfo {
    public uint32 Flags;
    public SockaddrGen Address;
    public SockaddrGen BroadcastAddress;
    public SockaddrGen Netmask;
}

partial struct IpAddressString {
    public array<byte> String = new(16);
}

partial struct IpMaskString /*IpAddressString*/;

partial struct IpAddrString {
    public ж<IpAddrString> Next;
    public IpAddressString IpAddress;
    public IpMaskString IpMask;
    public uint32 Context;
}

public static UntypedInt MAX_ADAPTER_NAME_LENGTH => 256;
public static UntypedInt MAX_ADAPTER_DESCRIPTION_LENGTH => 128;
public static UntypedInt MAX_ADAPTER_ADDRESS_LENGTH => 8;

partial struct IpAdapterInfo {
    public ж<IpAdapterInfo> Next;
    public uint32 ComboIndex;
    public array<byte> AdapterName = new(MAX_ADAPTER_NAME_LENGTH + 4);
    public array<byte> Description = new(MAX_ADAPTER_DESCRIPTION_LENGTH + 4);
    public uint32 AddressLength;
    public array<byte> Address = new(MAX_ADAPTER_ADDRESS_LENGTH);
    public uint32 Index;
    public uint32 Type;
    public uint32 DhcpEnabled;
    public ж<IpAddrString> CurrentIpAddress;
    public IpAddrString IpAddressList;
    public IpAddrString GatewayList;
    public IpAddrString DhcpServer;
    public bool HaveWins;
    public IpAddrString PrimaryWinsServer;
    public IpAddrString SecondaryWinsServer;
    public int64 LeaseObtained;
    public int64 LeaseExpires;
}

public static UntypedInt MAXLEN_PHYSADDR => 8;
public static UntypedInt MAX_INTERFACE_NAME_LEN => 256;
public static UntypedInt MAXLEN_IFDESCR => 256;

[global::go.golib.GoInlineLayout(Size = 860, Align = 4), StructLayout(LayoutKind.Explicit, Size = 860)] partial struct MibIfRow {
    [FieldOffset(0)] public array256<uint16> Name;
    [FieldOffset(512)] public uint32 Index;
    [FieldOffset(516)] public uint32 Type;
    [FieldOffset(520)] public uint32 Mtu;
    [FieldOffset(524)] public uint32 Speed;
    [FieldOffset(528)] public uint32 PhysAddrLen;
    [FieldOffset(532)] public array8<byte> PhysAddr;
    [FieldOffset(540)] public uint32 AdminStatus;
    [FieldOffset(544)] public uint32 OperStatus;
    [FieldOffset(548)] public uint32 LastChange;
    [FieldOffset(552)] public uint32 InOctets;
    [FieldOffset(556)] public uint32 InUcastPkts;
    [FieldOffset(560)] public uint32 InNUcastPkts;
    [FieldOffset(564)] public uint32 InDiscards;
    [FieldOffset(568)] public uint32 InErrors;
    [FieldOffset(572)] public uint32 InUnknownProtos;
    [FieldOffset(576)] public uint32 OutOctets;
    [FieldOffset(580)] public uint32 OutUcastPkts;
    [FieldOffset(584)] public uint32 OutNUcastPkts;
    [FieldOffset(588)] public uint32 OutDiscards;
    [FieldOffset(592)] public uint32 OutErrors;
    [FieldOffset(596)] public uint32 OutQLen;
    [FieldOffset(600)] public uint32 DescrLen;
    [FieldOffset(604)] public array256<byte> Descr;
}

partial struct CertInfo {
    public uint32 Version;
    public CryptIntegerBlob SerialNumber;
    public CryptAlgorithmIdentifier SignatureAlgorithm;
    public CertNameBlob Issuer;
    public Filetime NotBefore;
    public Filetime NotAfter;
    public CertNameBlob Subject;
    public CertPublicKeyInfo SubjectPublicKeyInfo;
    public CryptBitBlob IssuerUniqueId;
    public CryptBitBlob SubjectUniqueId;
    public uint32 CountExtensions;
    public ж<CertExtension> Extensions;
}

partial struct CertExtension {
    public ж<byte> ObjId;
    public int32 Critical;
    public CryptObjidBlob Value;
}

partial struct CryptAlgorithmIdentifier {
    public ж<byte> ObjId;
    public CryptObjidBlob Parameters;
}

partial struct CertPublicKeyInfo {
    public CryptAlgorithmIdentifier Algorithm;
    public CryptBitBlob PublicKey;
}

partial struct DataBlob {
    public uint32 Size;
    public ж<byte> Data;
}
partial struct CryptIntegerBlob /*DataBlob*/;
partial struct CryptUintBlob /*DataBlob*/;
partial struct CryptObjidBlob /*DataBlob*/;
partial struct CertNameBlob /*DataBlob*/;
partial struct CertRdnValueBlob /*DataBlob*/;
partial struct CertBlob /*DataBlob*/;
partial struct CrlBlob /*DataBlob*/;
partial struct CryptDataBlob /*DataBlob*/;
partial struct CryptHashBlob /*DataBlob*/;
partial struct CryptDigestBlob /*DataBlob*/;
partial struct CryptDerBlob /*DataBlob*/;
partial struct CryptAttrBlob /*DataBlob*/;

partial struct CryptBitBlob {
    public uint32 Size;
    public ж<byte> Data;
    public uint32 UnusedBits;
}

partial struct CertContext {
    public uint32 EncodingType;
    public ж<byte> EncodedCert;
    public uint32 Length;
    public ж<CertInfo> CertInfo;
    public Handle Store;
}

partial struct CertChainContext {
    public uint32 Size;
    public CertTrustStatus TrustStatus;
    public uint32 ChainCount;
    public ж<ж<CertSimpleChain>> Chains;
    public uint32 LowerQualityChainCount;
    public ж<ж<CertChainContext>> LowerQualityChains;
    public uint32 HasRevocationFreshnessTime;
    public uint32 RevocationFreshnessTime;
}

partial struct CertTrustListInfo {
}

partial struct CertSimpleChain {
    public uint32 Size;
    public CertTrustStatus TrustStatus;
    public uint32 NumElements;
    public ж<ж<CertChainElement>> Elements;
    public ж<CertTrustListInfo> TrustListInfo;
    public uint32 HasRevocationFreshnessTime;
    public uint32 RevocationFreshnessTime;
}

partial struct CertChainElement {
    public uint32 Size;
    public ж<CertContext> CertContext;
    public CertTrustStatus TrustStatus;
    public ж<CertRevocationInfo> RevocationInfo;
    public ж<CertEnhKeyUsage> IssuanceUsage;
    public ж<CertEnhKeyUsage> ApplicationUsage;
    public ж<uint16> ExtendedErrorInfo;
}

partial struct CertRevocationCrlInfo {
}

partial struct CertRevocationInfo {
    public uint32 Size;
    public uint32 RevocationResult;
    public ж<byte> RevocationOid;
    public Pointer OidSpecificInfo;
    public uint32 HasFreshnessTime;
    public uint32 FreshnessTime;
    public ж<CertRevocationCrlInfo> CrlInfo;
}

partial struct CertTrustStatus {
    public uint32 ErrorStatus;
    public uint32 InfoStatus;
}

partial struct CertUsageMatch {
    public uint32 Type;
    public CertEnhKeyUsage Usage;
}

partial struct CertEnhKeyUsage {
    public uint32 Length;
    public ж<ж<byte>> UsageIdentifiers;
}

partial struct CertChainPara {
    public uint32 Size;
    public CertUsageMatch RequestedUsage;
    public CertUsageMatch RequstedIssuancePolicy;
    public uint32 URLRetrievalTimeout;
    public uint32 CheckRevocationFreshnessTime;
    public uint32 RevocationFreshnessTime;
    public ж<Filetime> CacheResync;
}

partial struct CertChainPolicyPara {
    public uint32 Size;
    public uint32 Flags;
    public Pointer ExtraPolicyPara;
}

partial struct SSLExtraCertChainPolicyPara {
    public uint32 Size;
    public uint32 AuthType;
    public uint32 Checks;
    public ж<uint16> ServerName;
}

partial struct CertChainPolicyStatus {
    public uint32 Size;
    public uint32 Error;
    public uint32 ChainIndex;
    public uint32 ElementIndex;
    public Pointer ExtraPolicyStatus;
}

partial struct CertPolicyInfo {
    public ж<byte> Identifier;
    public uint32 CountQualifiers;
    public ж<CertPolicyQualifierInfo> Qualifiers;
}

partial struct CertPoliciesInfo {
    public uint32 Count;
    public ж<CertPolicyInfo> PolicyInfos;
}

partial struct CertPolicyQualifierInfo {
}

partial struct CertStrongSignPara {
    public uint32 Size;
    public uint32 InfoChoice;
    public @unsafe.Pointer InfoOrSerializedInfoOrOID;
}

partial struct CryptProtectPromptStruct {
    public uint32 Size;
    public uint32 PromptFlags;
    public HWND App;
    public ж<uint16> Prompt;
}

partial struct CertChainFindByIssuerPara {
    public uint32 Size;
    public ж<byte> UsageIdentifier;
    public uint32 KeySpec;
    public uint32 AcquirePrivateKeyFlags;
    public uint32 IssuerCount;
    public Pointer Issuer;
    public Pointer FindCallback;
    public Pointer FindArg;
    public ж<uint32> IssuerChainIndex;
    public ж<uint32> IssuerElementIndex;
}

partial struct WinTrustData {
    public uint32 Size;
    public uintptr PolicyCallbackData;
    public uintptr SIPClientData;
    public uint32 UIChoice;
    public uint32 RevocationChecks;
    public uint32 UnionChoice;
    public @unsafe.Pointer FileOrCatalogOrBlobOrSgnrOrCert;
    public uint32 StateAction;
    public Handle StateData;
    public ж<uint16> URLReference;
    public uint32 ProvFlags;
    public uint32 UIContext;
    public ж<WinTrustSignatureSettings> SignatureSettings;
}

partial struct WinTrustFileInfo {
    public uint32 Size;
    public ж<uint16> FilePath;
    public Handle File;
    public ж<GUID> KnownSubject;
}

partial struct WinTrustSignatureSettings {
    public uint32 Size;
    public uint32 Index;
    public uint32 Flags;
    public uint32 SecondarySigs;
    public uint32 VerifiedSigIndex;
    public ж<CertStrongSignPara> CryptoPolicy;
}

public static UntypedInt HKEY_CLASSES_ROOT => /* 0x80000000 + iota */ 2147483648;
public static UntypedInt HKEY_CURRENT_USER => 2147483649;
public static UntypedInt HKEY_LOCAL_MACHINE => 2147483650;
public static UntypedInt HKEY_USERS => 2147483651;
public static UntypedInt HKEY_PERFORMANCE_DATA => 2147483652;
public static UntypedInt HKEY_CURRENT_CONFIG => 2147483653;
public static UntypedInt HKEY_DYN_DATA => 2147483654;
public static UntypedInt KEY_QUERY_VALUE => 1;
public static UntypedInt KEY_SET_VALUE => 2;
public static UntypedInt KEY_CREATE_SUB_KEY => 4;
public static UntypedInt KEY_ENUMERATE_SUB_KEYS => 8;
public static UntypedInt KEY_NOTIFY => 16;
public static UntypedInt KEY_CREATE_LINK => 32;
public static UntypedInt KEY_WRITE => 0x20006;
public static UntypedInt KEY_EXECUTE => 0x20019;
public static UntypedInt KEY_READ => 0x20019;
public static UntypedInt KEY_WOW64_64KEY => 0x0100;
public static UntypedInt KEY_WOW64_32KEY => 0x0200;
public static UntypedInt KEY_ALL_ACCESS => 0xf003f;

public static UntypedInt REG_NONE => iota;
public static UntypedInt REG_SZ => 1;
public static UntypedInt REG_EXPAND_SZ => 2;
public static UntypedInt REG_BINARY => 3;
public static UntypedInt REG_DWORD_LITTLE_ENDIAN => 4;
public static UntypedInt REG_DWORD_BIG_ENDIAN => 5;
public static UntypedInt REG_LINK => 6;
public static UntypedInt REG_MULTI_SZ => 7;
public static UntypedInt REG_RESOURCE_LIST => 8;
public static UntypedInt REG_FULL_RESOURCE_DESCRIPTOR => 9;
public static UntypedInt REG_RESOURCE_REQUIREMENTS_LIST => 10;
public static UntypedInt REG_QWORD_LITTLE_ENDIAN => 11;
public static UntypedInt REG_DWORD => /* REG_DWORD_LITTLE_ENDIAN */ 4;
public static UntypedInt REG_QWORD => /* REG_QWORD_LITTLE_ENDIAN */ 11;

public static UntypedInt EVENT_MODIFY_STATE => 0x0002;
public static UntypedInt EVENT_ALL_ACCESS => /* STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0x3 */ 2031619;
public static UntypedInt MUTANT_QUERY_STATE => 0x0001;
public static UntypedInt MUTANT_ALL_ACCESS => /* STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | MUTANT_QUERY_STATE */ 2031617;
public static UntypedInt SEMAPHORE_MODIFY_STATE => 0x0002;
public static UntypedInt SEMAPHORE_ALL_ACCESS => /* STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0x3 */ 2031619;
public static UntypedInt TIMER_QUERY_STATE => 0x0001;
public static UntypedInt TIMER_MODIFY_STATE => 0x0002;
public static UntypedInt TIMER_ALL_ACCESS => /* STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | TIMER_QUERY_STATE | TIMER_MODIFY_STATE */ 2031619;
public static UntypedInt MUTEX_MODIFY_STATE => /* MUTANT_QUERY_STATE */ 1;
public static UntypedInt MUTEX_ALL_ACCESS => /* MUTANT_ALL_ACCESS */ 2031617;
public static UntypedInt CREATE_EVENT_MANUAL_RESET => 0x1;
public static UntypedInt CREATE_EVENT_INITIAL_SET => 0x2;
public static UntypedInt CREATE_MUTEX_INITIAL_OWNER => 0x1;

partial struct AddrinfoW {
    public int32 Flags;
    public int32 Family;
    public int32 Socktype;
    public int32 Protocol;
    public uintptr Addrlen;
    public ж<uint16> Canonname;
    public uintptr Addr;
    public ж<AddrinfoW> Next;
}

public static UntypedInt AI_PASSIVE => 1;
public static UntypedInt AI_CANONNAME => 2;
public static UntypedInt AI_NUMERICHOST => 4;

[global::go.golib.GoInlineLayout(Size = 16, Align = 4), StructLayout(LayoutKind.Explicit, Size = 16)] partial struct GUID {
    [FieldOffset(0)] public uint32 Data1;
    [FieldOffset(4)] public uint16 Data2;
    [FieldOffset(6)] public uint16 Data3;
    [FieldOffset(8)] public array8<byte> Data4;
}

public static readonly ж<GUID> ᏑWSAID_CONNECTEX = new StaticBox<GUID>(new GUID(
    0x25a207b9,
    0xddf3,
    0x4660,
    new array8<byte>(0x8e, 0xe9, 0x76, 0xe5, 0x8c, 0x74, 0x06, 0x3e)
));
public static ref GUID WSAID_CONNECTEX => ref ((StaticBox<GUID>)ᏑWSAID_CONNECTEX).ValueSlot;

public static readonly ж<GUID> ᏑWSAID_WSASENDMSG = new StaticBox<GUID>(new GUID(
    0xa441e712U,
    0x754f,
    0x43ca,
    new array8<byte>(0x84, 0xa7, 0x0d, 0xee, 0x44, 0xcf, 0x60, 0x6d)
));
public static ref GUID WSAID_WSASENDMSG => ref ((StaticBox<GUID>)ᏑWSAID_WSASENDMSG).ValueSlot;

public static readonly ж<GUID> ᏑWSAID_WSARECVMSG = new StaticBox<GUID>(new GUID(
    0xf689d7c8U,
    0x6f1f,
    0x436b,
    new array8<byte>(0x8a, 0x53, 0xe5, 0x4f, 0xe3, 0x51, 0xc3, 0x22)
));
public static ref GUID WSAID_WSARECVMSG => ref ((StaticBox<GUID>)ᏑWSAID_WSARECVMSG).ValueSlot;

public static UntypedInt FILE_SKIP_COMPLETION_PORT_ON_SUCCESS => 1;
public static UntypedInt FILE_SKIP_SET_EVENT_ON_HANDLE => 2;

public static UntypedInt WSAPROTOCOL_LEN => 255;
public static UntypedInt MAX_PROTOCOL_CHAIN => 7;
public static UntypedInt BASE_PROTOCOL => 1;
public static UntypedInt LAYERED_PROTOCOL => 0;
public static UntypedInt XP1_CONNECTIONLESS => 0x00000001;
public static UntypedInt XP1_GUARANTEED_DELIVERY => 0x00000002;
public static UntypedInt XP1_GUARANTEED_ORDER => 0x00000004;
public static UntypedInt XP1_MESSAGE_ORIENTED => 0x00000008;
public static UntypedInt XP1_PSEUDO_STREAM => 0x00000010;
public static UntypedInt XP1_GRACEFUL_CLOSE => 0x00000020;
public static UntypedInt XP1_EXPEDITED_DATA => 0x00000040;
public static UntypedInt XP1_CONNECT_DATA => 0x00000080;
public static UntypedInt XP1_DISCONNECT_DATA => 0x00000100;
public static UntypedInt XP1_SUPPORT_BROADCAST => 0x00000200;
public static UntypedInt XP1_SUPPORT_MULTIPOINT => 0x00000400;
public static UntypedInt XP1_MULTIPOINT_CONTROL_PLANE => 0x00000800;
public static UntypedInt XP1_MULTIPOINT_DATA_PLANE => 0x00001000;
public static UntypedInt XP1_QOS_SUPPORTED => 0x00002000;
public static UntypedInt XP1_UNI_SEND => 0x00008000;
public static UntypedInt XP1_UNI_RECV => 0x00010000;
public static UntypedInt XP1_IFS_HANDLES => 0x00020000;
public static UntypedInt XP1_PARTIAL_MESSAGE => 0x00040000;
public static UntypedInt XP1_SAN_SUPPORT_SDP => 0x00080000;
public static UntypedInt PFL_MULTIPLE_PROTO_ENTRIES => 0x00000001;
public static UntypedInt PFL_RECOMMENDED_PROTO_ENTRY => 0x00000002;
public static UntypedInt PFL_HIDDEN => 0x00000004;
public static UntypedInt PFL_MATCHES_PROTOCOL_ZERO => 0x00000008;
public static UntypedInt PFL_NETWORKDIRECT_PROVIDER => 0x00000010;

[global::go.golib.GoInlineLayout(Size = 628, Align = 4), StructLayout(LayoutKind.Explicit, Size = 628)] partial struct WSAProtocolInfo {
    [FieldOffset(0)] public uint32 ServiceFlags1;
    [FieldOffset(4)] public uint32 ServiceFlags2;
    [FieldOffset(8)] public uint32 ServiceFlags3;
    [FieldOffset(12)] public uint32 ServiceFlags4;
    [FieldOffset(16)] public uint32 ProviderFlags;
    [FieldOffset(20)] public GUID ProviderId;
    [FieldOffset(36)] public uint32 CatalogEntryId;
    [FieldOffset(40)] public WSAProtocolChain ProtocolChain;
    [FieldOffset(72)] public int32 Version;
    [FieldOffset(76)] public int32 AddressFamily;
    [FieldOffset(80)] public int32 MaxSockAddr;
    [FieldOffset(84)] public int32 MinSockAddr;
    [FieldOffset(88)] public int32 SocketType;
    [FieldOffset(92)] public int32 Protocol;
    [FieldOffset(96)] public int32 ProtocolMaxOffset;
    [FieldOffset(100)] public int32 NetworkByteOrder;
    [FieldOffset(104)] public int32 SecurityScheme;
    [FieldOffset(108)] public uint32 MessageSize;
    [FieldOffset(112)] public uint32 ProviderReserved;
    [FieldOffset(116)] public array256<uint16> ProtocolName;
}

[global::go.golib.GoInlineLayout(Size = 32, Align = 4), StructLayout(LayoutKind.Explicit, Size = 32)] partial struct WSAProtocolChain {
    [FieldOffset(0)] public int32 ChainLen;
    [FieldOffset(4)] public array7<uint32> ChainEntries;
}

partial struct TCPKeepalive {
    public uint32 OnOff;
    public uint32 Time;
    public uint32 Interval;
}

partial struct symbolicLinkReparseBuffer {
    public uint16 SubstituteNameOffset;
    public uint16 SubstituteNameLength;
    public uint16 PrintNameOffset;
    public uint16 PrintNameLength;
    public uint32 Flags;
    public array<uint16> PathBuffer = new(1);
}

partial struct mountPointReparseBuffer {
    public uint16 SubstituteNameOffset;
    public uint16 SubstituteNameLength;
    public uint16 PrintNameOffset;
    public uint16 PrintNameLength;
    public array<uint16> PathBuffer = new(1);
}

partial struct reparseDataBuffer {
    public uint32 ReparseTag;
    public uint16 ReparseDataLength;
    public uint16 Reserved;
    internal byte reparseBuffer;
}

public static UntypedInt FSCTL_CREATE_OR_GET_OBJECT_ID => 0x0900C0;
public static UntypedInt FSCTL_DELETE_OBJECT_ID => 0x0900A0;
public static UntypedInt FSCTL_DELETE_REPARSE_POINT => 0x0900AC;
public static UntypedInt FSCTL_DUPLICATE_EXTENTS_TO_FILE => 0x098344;
public static UntypedInt FSCTL_DUPLICATE_EXTENTS_TO_FILE_EX => 0x0983E8;
public static UntypedInt FSCTL_FILESYSTEM_GET_STATISTICS => 0x090060;
public static UntypedInt FSCTL_FILE_LEVEL_TRIM => 0x098208;
public static UntypedInt FSCTL_FIND_FILES_BY_SID => 0x09008F;
public static UntypedInt FSCTL_GET_COMPRESSION => 0x09003C;
public static UntypedInt FSCTL_GET_INTEGRITY_INFORMATION => 0x09027C;
public static UntypedInt FSCTL_GET_NTFS_VOLUME_DATA => 0x090064;
public static UntypedInt FSCTL_GET_REFS_VOLUME_DATA => 0x0902D8;
public static UntypedInt FSCTL_GET_OBJECT_ID => 0x09009C;
public static UntypedInt FSCTL_GET_REPARSE_POINT => 0x0900A8;
public static UntypedInt FSCTL_GET_RETRIEVAL_POINTER_COUNT => 0x09042B;
public static UntypedInt FSCTL_GET_RETRIEVAL_POINTERS => 0x090073;
public static UntypedInt FSCTL_GET_RETRIEVAL_POINTERS_AND_REFCOUNT => 0x0903D3;
public static UntypedInt FSCTL_IS_PATHNAME_VALID => 0x09002C;
public static UntypedInt FSCTL_LMR_SET_LINK_TRACKING_INFORMATION => 0x1400EC;
public static UntypedInt FSCTL_MARK_HANDLE => 0x0900FC;
public static UntypedInt FSCTL_OFFLOAD_READ => 0x094264;
public static UntypedInt FSCTL_OFFLOAD_WRITE => 0x098268;
public static UntypedInt FSCTL_PIPE_PEEK => 0x11400C;
public static UntypedInt FSCTL_PIPE_TRANSCEIVE => 0x11C017;
public static UntypedInt FSCTL_PIPE_WAIT => 0x110018;
public static UntypedInt FSCTL_QUERY_ALLOCATED_RANGES => 0x0940CF;
public static UntypedInt FSCTL_QUERY_FAT_BPB => 0x090058;
public static UntypedInt FSCTL_QUERY_FILE_REGIONS => 0x090284;
public static UntypedInt FSCTL_QUERY_ON_DISK_VOLUME_INFO => 0x09013C;
public static UntypedInt FSCTL_QUERY_SPARING_INFO => 0x090138;
public static UntypedInt FSCTL_READ_FILE_USN_DATA => 0x0900EB;
public static UntypedInt FSCTL_RECALL_FILE => 0x090117;
public static UntypedInt FSCTL_REFS_STREAM_SNAPSHOT_MANAGEMENT => 0x090440;
public static UntypedInt FSCTL_SET_COMPRESSION => 0x09C040;
public static UntypedInt FSCTL_SET_DEFECT_MANAGEMENT => 0x098134;
public static UntypedInt FSCTL_SET_ENCRYPTION => 0x0900D7;
public static UntypedInt FSCTL_SET_INTEGRITY_INFORMATION => 0x09C280;
public static UntypedInt FSCTL_SET_INTEGRITY_INFORMATION_EX => 0x090380;
public static UntypedInt FSCTL_SET_OBJECT_ID => 0x090098;
public static UntypedInt FSCTL_SET_OBJECT_ID_EXTENDED => 0x0900BC;
public static UntypedInt FSCTL_SET_REPARSE_POINT => 0x0900A4;
public static UntypedInt FSCTL_SET_SPARSE => 0x0900C4;
public static UntypedInt FSCTL_SET_ZERO_DATA => 0x0980C8;
public static UntypedInt FSCTL_SET_ZERO_ON_DEALLOCATION => 0x090194;
public static UntypedInt FSCTL_SIS_COPYFILE => 0x090100;
public static UntypedInt FSCTL_WRITE_USN_CLOSE_RECORD => 0x0900EF;
public static UntypedInt MAXIMUM_REPARSE_DATA_BUFFER_SIZE => /* 16 * 1024 */ 16384;
public static UntypedInt IO_REPARSE_TAG_MOUNT_POINT => 0xA0000003;
public static UntypedInt IO_REPARSE_TAG_SYMLINK => 0xA000000C;
public static UntypedInt SYMBOLIC_LINK_FLAG_DIRECTORY => 0x1;

partial struct FileZeroDataInformation {
    public int64 FileOffset;
    public int64 BeyondFinalZero;
}

public static UntypedInt ComputerNameNetBIOS => 0;
public static UntypedInt ComputerNameDnsHostname => 1;
public static UntypedInt ComputerNameDnsDomain => 2;
public static UntypedInt ComputerNameDnsFullyQualified => 3;
public static UntypedInt ComputerNamePhysicalNetBIOS => 4;
public static UntypedInt ComputerNamePhysicalDnsHostname => 5;
public static UntypedInt ComputerNamePhysicalDnsDomain => 6;
public static UntypedInt ComputerNamePhysicalDnsFullyQualified => 7;
public static UntypedInt ComputerNameMax => 8;

public static UntypedInt MB_OK => 0x00000000;
public static UntypedInt MB_OKCANCEL => 0x00000001;
public static UntypedInt MB_ABORTRETRYIGNORE => 0x00000002;
public static UntypedInt MB_YESNOCANCEL => 0x00000003;
public static UntypedInt MB_YESNO => 0x00000004;
public static UntypedInt MB_RETRYCANCEL => 0x00000005;
public static UntypedInt MB_CANCELTRYCONTINUE => 0x00000006;
public static UntypedInt MB_ICONHAND => 0x00000010;
public static UntypedInt MB_ICONQUESTION => 0x00000020;
public static UntypedInt MB_ICONEXCLAMATION => 0x00000030;
public static UntypedInt MB_ICONASTERISK => 0x00000040;
public static UntypedInt MB_USERICON => 0x00000080;
public static UntypedInt MB_ICONWARNING => /* MB_ICONEXCLAMATION */ 48;
public static UntypedInt MB_ICONERROR => /* MB_ICONHAND */ 16;
public static UntypedInt MB_ICONINFORMATION => /* MB_ICONASTERISK */ 64;
public static UntypedInt MB_ICONSTOP => /* MB_ICONHAND */ 16;
public static UntypedInt MB_DEFBUTTON1 => 0x00000000;
public static UntypedInt MB_DEFBUTTON2 => 0x00000100;
public static UntypedInt MB_DEFBUTTON3 => 0x00000200;
public static UntypedInt MB_DEFBUTTON4 => 0x00000300;
public static UntypedInt MB_APPLMODAL => 0x00000000;
public static UntypedInt MB_SYSTEMMODAL => 0x00001000;
public static UntypedInt MB_TASKMODAL => 0x00002000;
public static UntypedInt MB_HELP => 0x00004000;
public static UntypedInt MB_NOFOCUS => 0x00008000;
public static UntypedInt MB_SETFOREGROUND => 0x00010000;
public static UntypedInt MB_DEFAULT_DESKTOP_ONLY => 0x00020000;
public static UntypedInt MB_TOPMOST => 0x00040000;
public static UntypedInt MB_RIGHT => 0x00080000;
public static UntypedInt MB_RTLREADING => 0x00100000;
public static UntypedInt MB_SERVICE_NOTIFICATION => 0x00200000;

public static UntypedInt MOVEFILE_REPLACE_EXISTING => 0x1;
public static UntypedInt MOVEFILE_COPY_ALLOWED => 0x2;
public static UntypedInt MOVEFILE_DELAY_UNTIL_REBOOT => 0x4;
public static UntypedInt MOVEFILE_WRITE_THROUGH => 0x8;
public static UntypedInt MOVEFILE_CREATE_HARDLINK => 0x10;
public static UntypedInt MOVEFILE_FAIL_IF_NOT_TRACKABLE => 0x20;

public static UntypedInt GAA_FLAG_SKIP_UNICAST => 0x1;
public static UntypedInt GAA_FLAG_SKIP_ANYCAST => 0x2;
public static UntypedInt GAA_FLAG_SKIP_MULTICAST => 0x4;
public static UntypedInt GAA_FLAG_SKIP_DNS_SERVER => 0x8;
public static UntypedInt GAA_FLAG_INCLUDE_PREFIX => 0x10;
public static UntypedInt GAA_FLAG_SKIP_FRIENDLY_NAME => 0x20;
public static UntypedInt GAA_FLAG_INCLUDE_WINS_INFO => 0x40;
public static UntypedInt GAA_FLAG_INCLUDE_GATEWAYS => 0x80;
public static UntypedInt GAA_FLAG_INCLUDE_ALL_INTERFACES => 0x100;
public static UntypedInt GAA_FLAG_INCLUDE_ALL_COMPARTMENTS => 0x200;
public static UntypedInt GAA_FLAG_INCLUDE_TUNNEL_BINDINGORDER => 0x400;

public static UntypedInt IF_TYPE_OTHER => 1;
public static UntypedInt IF_TYPE_ETHERNET_CSMACD => 6;
public static UntypedInt IF_TYPE_ISO88025_TOKENRING => 9;
public static UntypedInt IF_TYPE_PPP => 23;
public static UntypedInt IF_TYPE_SOFTWARE_LOOPBACK => 24;
public static UntypedInt IF_TYPE_ATM => 37;
public static UntypedInt IF_TYPE_IEEE80211 => 71;
public static UntypedInt IF_TYPE_TUNNEL => 131;
public static UntypedInt IF_TYPE_IEEE1394 => 144;

public static UntypedInt IpPrefixOriginOther => 0;
public static UntypedInt IpPrefixOriginManual => 1;
public static UntypedInt IpPrefixOriginWellKnown => 2;
public static UntypedInt IpPrefixOriginDhcp => 3;
public static UntypedInt IpPrefixOriginRouterAdvertisement => 4;
public static UntypedInt IpPrefixOriginUnchanged => /* 1 << 4 */ 16;

public static UntypedInt NlsoOther => 0;
public static UntypedInt NlsoManual => 1;
public static UntypedInt NlsoWellKnown => 2;
public static UntypedInt NlsoDhcp => 3;
public static UntypedInt NlsoLinkLayerAddress => 4;
public static UntypedInt NlsoRandom => 5;
public static UntypedInt IpSuffixOriginOther => 0;
public static UntypedInt IpSuffixOriginManual => 1;
public static UntypedInt IpSuffixOriginWellKnown => 2;
public static UntypedInt IpSuffixOriginDhcp => 3;
public static UntypedInt IpSuffixOriginLinkLayerAddress => 4;
public static UntypedInt IpSuffixOriginRandom => 5;
public static UntypedInt IpSuffixOriginUnchanged => /* 1 << 4 */ 16;

public static UntypedInt NldsInvalid => 0;
public static UntypedInt NldsTentative => 1;
public static UntypedInt NldsDuplicate => 2;
public static UntypedInt NldsDeprecated => 3;
public static UntypedInt NldsPreferred => 4;
public static UntypedInt IpDadStateInvalid => 0;
public static UntypedInt IpDadStateTentative => 1;
public static UntypedInt IpDadStateDuplicate => 2;
public static UntypedInt IpDadStateDeprecated => 3;
public static UntypedInt IpDadStatePreferred => 4;

partial struct SocketAddress {
    public ж<syscall.RawSockaddrAny> Sockaddr;
    public int32 SockaddrLength;
}

} partial class windowsᴛSocketAddress {

public static net.IP IP(this ref SocketAddress addr) {
    if ((uintptr)addr.SockaddrLength >= /* unsafe.Sizeof(RawSockaddrInet4{}) */ (uintptr)16 && addr.Sockaddr.Addr.Family == AF_INET) {
        return (addr.Sockaddr.Reinterpret<syscall.RawSockaddrAny, RawSockaddrInet4>()).Value.Addr[..];
    } else if ((uintptr)addr.SockaddrLength >= /* unsafe.Sizeof(RawSockaddrInet6{}) */ (uintptr)28 && addr.Sockaddr.Addr.Family == AF_INET6) {
        return (addr.Sockaddr.Reinterpret<syscall.RawSockaddrAny, RawSockaddrInet6>()).Value.Addr[..];
    }
    return default;
}

} partial class windows_package {

partial struct IpAdapterUnicastAddress {
    public uint32 Length;
    public uint32 Flags;
    public ж<IpAdapterUnicastAddress> Next;
    public SocketAddress Address;
    public int32 PrefixOrigin;
    public int32 SuffixOrigin;
    public int32 DadState;
    public uint32 ValidLifetime;
    public uint32 PreferredLifetime;
    public uint32 LeaseLifetime;
    public uint8 OnLinkPrefixLength;
}

partial struct IpAdapterAnycastAddress {
    public uint32 Length;
    public uint32 Flags;
    public ж<IpAdapterAnycastAddress> Next;
    public SocketAddress Address;
}

partial struct IpAdapterMulticastAddress {
    public uint32 Length;
    public uint32 Flags;
    public ж<IpAdapterMulticastAddress> Next;
    public SocketAddress Address;
}

partial struct IpAdapterDnsServerAdapter {
    public uint32 Length;
    public uint32 Reserved;
    public ж<IpAdapterDnsServerAdapter> Next;
    public SocketAddress Address;
}

partial struct IpAdapterPrefix {
    public uint32 Length;
    public uint32 Flags;
    public ж<IpAdapterPrefix> Next;
    public SocketAddress Address;
    public uint32 PrefixLength;
}

partial struct IpAdapterAddresses {
    public uint32 Length;
    public uint32 IfIndex;
    public ж<IpAdapterAddresses> Next;
    public ж<byte> AdapterName;
    public ж<IpAdapterUnicastAddress> FirstUnicastAddress;
    public ж<IpAdapterAnycastAddress> FirstAnycastAddress;
    public ж<IpAdapterMulticastAddress> FirstMulticastAddress;
    public ж<IpAdapterDnsServerAdapter> FirstDnsServerAddress;
    public ж<uint16> DnsSuffix;
    public ж<uint16> Description;
    public ж<uint16> FriendlyName;
    public array<byte> PhysicalAddress = new(syscall.MAX_ADAPTER_ADDRESS_LENGTH);
    public uint32 PhysicalAddressLength;
    public uint32 Flags;
    public uint32 Mtu;
    public uint32 IfType;
    public uint32 OperStatus;
    public uint32 Ipv6IfIndex;
    public array<uint32> ZoneIndices = new(16);
    public ж<IpAdapterPrefix> FirstPrefix;
    public uint64 TransmitLinkSpeed;
    public uint64 ReceiveLinkSpeed;
    public ж<IpAdapterWinsServerAddress> FirstWinsServerAddress;
    public ж<IpAdapterGatewayAddress> FirstGatewayAddress;
    public uint32 Ipv4Metric;
    public uint32 Ipv6Metric;
    public uint64 Luid;
    public SocketAddress Dhcpv4Server;
    public uint32 CompartmentId;
    public GUID NetworkGuid;
    public uint32 ConnectionType;
    public uint32 TunnelType;
    public SocketAddress Dhcpv6Server;
    public array<byte> Dhcpv6ClientDuid = new(MAX_DHCPV6_DUID_LENGTH);
    public uint32 Dhcpv6ClientDuidLength;
    public uint32 Dhcpv6Iaid;
    public ж<IpAdapterDNSSuffix> FirstDnsSuffix;
}

partial struct IpAdapterWinsServerAddress {
    public uint32 Length;
    public uint32 Reserved;
    public ж<IpAdapterWinsServerAddress> Next;
    public SocketAddress Address;
}

partial struct IpAdapterGatewayAddress {
    public uint32 Length;
    public uint32 Reserved;
    public ж<IpAdapterGatewayAddress> Next;
    public SocketAddress Address;
}

partial struct IpAdapterDNSSuffix {
    public ж<IpAdapterDNSSuffix> Next;
    public array<uint16> String = new(MAX_DNS_SUFFIX_STRING_LENGTH);
}

public static UntypedInt IfOperStatusUp => 1;
public static UntypedInt IfOperStatusDown => 2;
public static UntypedInt IfOperStatusTesting => 3;
public static UntypedInt IfOperStatusUnknown => 4;
public static UntypedInt IfOperStatusDormant => 5;
public static UntypedInt IfOperStatusNotPresent => 6;
public static UntypedInt IfOperStatusLowerLayerDown => 7;

public static UntypedInt IF_MAX_PHYS_ADDRESS_LENGTH => 32;
public static UntypedInt IF_MAX_STRING_SIZE => 256;

public static UntypedInt MibIfEntryNormal => 0;
public static UntypedInt MibIfEntryNormalWithoutStatistics => 2;

public static UntypedInt MibParameterNotification => 0;
public static UntypedInt MibAddInstance => 1;
public static UntypedInt MibDeleteInstance => 2;
public static UntypedInt MibInitialNotification => 3;

[global::go.golib.GoInlineLayout(Size = 1352, Align = 8), StructLayout(LayoutKind.Explicit, Size = 1352)] partial struct MibIfRow2 {
    [FieldOffset(0)] public uint64 InterfaceLuid;
    [FieldOffset(8)] public uint32 InterfaceIndex;
    [FieldOffset(12)] public GUID InterfaceGuid;
    [FieldOffset(28)] public array257<uint16> Alias;
    [FieldOffset(542)] public array257<uint16> Description;
    [FieldOffset(1056)] public uint32 PhysicalAddressLength;
    [FieldOffset(1060)] public array32<uint8> PhysicalAddress;
    [FieldOffset(1092)] public array32<uint8> PermanentPhysicalAddress;
    [FieldOffset(1124)] public uint32 Mtu;
    [FieldOffset(1128)] public uint32 Type;
    [FieldOffset(1132)] public uint32 TunnelType;
    [FieldOffset(1136)] public uint32 MediaType;
    [FieldOffset(1140)] public uint32 PhysicalMediumType;
    [FieldOffset(1144)] public uint32 AccessType;
    [FieldOffset(1148)] public uint32 DirectionType;
    [FieldOffset(1152)] public uint8 InterfaceAndOperStatusFlags;
    [FieldOffset(1156)] public uint32 OperStatus;
    [FieldOffset(1160)] public uint32 AdminStatus;
    [FieldOffset(1164)] public uint32 MediaConnectState;
    [FieldOffset(1168)] public GUID NetworkGuid;
    [FieldOffset(1184)] public uint32 ConnectionType;
    [FieldOffset(1192)] public uint64 TransmitLinkSpeed;
    [FieldOffset(1200)] public uint64 ReceiveLinkSpeed;
    [FieldOffset(1208)] public uint64 InOctets;
    [FieldOffset(1216)] public uint64 InUcastPkts;
    [FieldOffset(1224)] public uint64 InNUcastPkts;
    [FieldOffset(1232)] public uint64 InDiscards;
    [FieldOffset(1240)] public uint64 InErrors;
    [FieldOffset(1248)] public uint64 InUnknownProtos;
    [FieldOffset(1256)] public uint64 InUcastOctets;
    [FieldOffset(1264)] public uint64 InMulticastOctets;
    [FieldOffset(1272)] public uint64 InBroadcastOctets;
    [FieldOffset(1280)] public uint64 OutOctets;
    [FieldOffset(1288)] public uint64 OutUcastPkts;
    [FieldOffset(1296)] public uint64 OutNUcastPkts;
    [FieldOffset(1304)] public uint64 OutDiscards;
    [FieldOffset(1312)] public uint64 OutErrors;
    [FieldOffset(1320)] public uint64 OutUcastOctets;
    [FieldOffset(1328)] public uint64 OutMulticastOctets;
    [FieldOffset(1336)] public uint64 OutBroadcastOctets;
    [FieldOffset(1344)] public uint64 OutQLen;
}

public static UntypedInt MibIfTableNormal => 0;
public static UntypedInt MibIfTableRaw => 1;
public static UntypedInt MibIfTableNormalWithoutStatistics => 2;

partial struct MibIfTable2 {
    public uint32 NumEntries;
    public array<MibIfRow2> Table = new(1, () => new());
}

[global::go.golib.GoInlineLayout(Size = 32, Align = 4), StructLayout(LayoutKind.Explicit, Size = 32)] partial struct IpAddressPrefix {
    [FieldOffset(0)] public RawSockaddrInet Prefix;
    [FieldOffset(28)] public uint8 PrefixLength;
}

public static UntypedInt NlroManual => 0;
public static UntypedInt NlroWellKnown => 1;
public static UntypedInt NlroDHCP => 2;
public static UntypedInt NlroRouterAdvertisement => 3;
public static UntypedInt Nlro6to4 => 4;

public static UntypedInt MIB_IPPROTO_OTHER => 1;
public static UntypedInt MIB_IPPROTO_LOCAL => 2;
public static UntypedInt MIB_IPPROTO_NETMGMT => 3;
public static UntypedInt MIB_IPPROTO_ICMP => 4;
public static UntypedInt MIB_IPPROTO_EGP => 5;
public static UntypedInt MIB_IPPROTO_GGP => 6;
public static UntypedInt MIB_IPPROTO_HELLO => 7;
public static UntypedInt MIB_IPPROTO_RIP => 8;
public static UntypedInt MIB_IPPROTO_IS_IS => 9;
public static UntypedInt MIB_IPPROTO_ES_IS => 10;
public static UntypedInt MIB_IPPROTO_CISCO => 11;
public static UntypedInt MIB_IPPROTO_BBN => 12;
public static UntypedInt MIB_IPPROTO_OSPF => 13;
public static UntypedInt MIB_IPPROTO_BGP => 14;
public static UntypedInt MIB_IPPROTO_IDPR => 15;
public static UntypedInt MIB_IPPROTO_EIGRP => 16;
public static UntypedInt MIB_IPPROTO_DVMRP => 17;
public static UntypedInt MIB_IPPROTO_RPL => 18;
public static UntypedInt MIB_IPPROTO_DHCP => 19;
public static UntypedInt MIB_IPPROTO_NT_AUTOSTATIC => 10002;
public static UntypedInt MIB_IPPROTO_NT_STATIC => 10006;
public static UntypedInt MIB_IPPROTO_NT_STATIC_NON_DOD => 10007;

[global::go.golib.GoInlineLayout(Size = 104, Align = 8), StructLayout(LayoutKind.Explicit, Size = 104)] partial struct MibIpForwardRow2 {
    [FieldOffset(0)] public uint64 InterfaceLuid;
    [FieldOffset(8)] public uint32 InterfaceIndex;
    [FieldOffset(12)] public IpAddressPrefix DestinationPrefix;
    [FieldOffset(44)] public RawSockaddrInet NextHop;
    [FieldOffset(72)] public uint8 SitePrefixLength;
    [FieldOffset(76)] public uint32 ValidLifetime;
    [FieldOffset(80)] public uint32 PreferredLifetime;
    [FieldOffset(84)] public uint32 Metric;
    [FieldOffset(88)] public uint32 Protocol;
    [FieldOffset(92)] public uint8 Loopback;
    [FieldOffset(93)] public uint8 AutoconfigureAddress;
    [FieldOffset(94)] public uint8 Publish;
    [FieldOffset(95)] public uint8 Immortal;
    [FieldOffset(96)] public uint32 Age;
    [FieldOffset(100)] public uint32 Origin;
}

partial struct MibIpForwardTable2 {
    public uint32 NumEntries;
    public array<MibIpForwardRow2> Table = new(1, () => new());
}

} partial class windowsᴛMibIpForwardTable2 {

public static slice<MibIpForwardRow2> Rows(this ref MibIpForwardTable2 t) {
    return @unsafe.Slice(Ꮡ(t.Table, 0), t.NumEntries);
}

} partial class windows_package {

partial struct MibUnicastIpAddressRow {
    public RawSockaddrInet6 Address;
    public uint64 InterfaceLuid;
    public uint32 InterfaceIndex;
    public uint32 PrefixOrigin;
    public uint32 SuffixOrigin;
    public uint32 ValidLifetime;
    public uint32 PreferredLifetime;
    public uint8 OnLinkPrefixLength;
    public uint8 SkipAsSource;
    public uint32 DadState;
    public uint32 ScopeId;
    public Filetime CreationTimeStamp;
}

partial struct MibUnicastIpAddressTable {
    public uint32 NumEntries;
    public array<MibUnicastIpAddressRow> Table = new(1, () => new());
}

public static UntypedInt ScopeLevelCount => 16;

[global::go.golib.GoInlineLayout(Size = 176, Align = 8), StructLayout(LayoutKind.Explicit, Size = 176)] partial struct MibIpInterfaceRow {
    [FieldOffset(0)] public uint16 Family;
    [FieldOffset(8)] public uint64 InterfaceLuid;
    [FieldOffset(16)] public uint32 InterfaceIndex;
    [FieldOffset(20)] public uint32 MaxReassemblySize;
    [FieldOffset(24)] public uint64 InterfaceIdentifier;
    [FieldOffset(32)] public uint32 MinRouterAdvertisementInterval;
    [FieldOffset(36)] public uint32 MaxRouterAdvertisementInterval;
    [FieldOffset(40)] public uint8 AdvertisingEnabled;
    [FieldOffset(41)] public uint8 ForwardingEnabled;
    [FieldOffset(42)] public uint8 WeakHostSend;
    [FieldOffset(43)] public uint8 WeakHostReceive;
    [FieldOffset(44)] public uint8 UseAutomaticMetric;
    [FieldOffset(45)] public uint8 UseNeighborUnreachabilityDetection;
    [FieldOffset(46)] public uint8 ManagedAddressConfigurationSupported;
    [FieldOffset(47)] public uint8 OtherStatefulConfigurationSupported;
    [FieldOffset(48)] public uint8 AdvertiseDefaultRoute;
    [FieldOffset(52)] public uint32 RouterDiscoveryBehavior;
    [FieldOffset(56)] public uint32 DadTransmits;
    [FieldOffset(60)] public uint32 BaseReachableTime;
    [FieldOffset(64)] public uint32 RetransmitTime;
    [FieldOffset(68)] public uint32 PathMtuDiscoveryTimeout;
    [FieldOffset(72)] public uint32 LinkLocalAddressBehavior;
    [FieldOffset(76)] public uint32 LinkLocalAddressTimeout;
    [FieldOffset(80)] public array16<uint32> ZoneIndices;
    [FieldOffset(144)] public uint32 SitePrefixLength;
    [FieldOffset(148)] public uint32 Metric;
    [FieldOffset(152)] public uint32 NlMtu;
    [FieldOffset(156)] public uint8 Connected;
    [FieldOffset(157)] public uint8 SupportsWakeUpPatterns;
    [FieldOffset(158)] public uint8 SupportsNeighborDiscovery;
    [FieldOffset(159)] public uint8 SupportsRouterDiscovery;
    [FieldOffset(160)] public uint32 ReachableTime;
    [FieldOffset(164)] public uint32 TransmitOffload;
    [FieldOffset(168)] public uint32 ReceiveOffload;
    [FieldOffset(172)] public uint8 DisableDefaultRoutes;
}

partial struct MibIpInterfaceTable {
    public uint32 NumEntries;
    public array<MibIpInterfaceRow> Table = new(1, () => new());
}

public static UntypedInt ENABLE_PROCESSED_INPUT => 0x1;
public static UntypedInt ENABLE_LINE_INPUT => 0x2;
public static UntypedInt ENABLE_ECHO_INPUT => 0x4;
public static UntypedInt ENABLE_WINDOW_INPUT => 0x8;
public static UntypedInt ENABLE_MOUSE_INPUT => 0x10;
public static UntypedInt ENABLE_INSERT_MODE => 0x20;
public static UntypedInt ENABLE_QUICK_EDIT_MODE => 0x40;
public static UntypedInt ENABLE_EXTENDED_FLAGS => 0x80;
public static UntypedInt ENABLE_AUTO_POSITION => 0x100;
public static UntypedInt ENABLE_VIRTUAL_TERMINAL_INPUT => 0x200;
public static UntypedInt ENABLE_PROCESSED_OUTPUT => 0x1;
public static UntypedInt ENABLE_WRAP_AT_EOL_OUTPUT => 0x2;
public static UntypedInt ENABLE_VIRTUAL_TERMINAL_PROCESSING => 0x4;
public static UntypedInt DISABLE_NEWLINE_AUTO_RETURN => 0x8;
public static UntypedInt ENABLE_LVB_GRID_WORLDWIDE => 0x10;

public static UntypedInt PSEUDOCONSOLE_INHERIT_CURSOR => 0x1;

partial struct Coord {
    public int16 X;
    public int16 Y;
}

partial struct SmallRect {
    public int16 Left;
    public int16 Top;
    public int16 Right;
    public int16 Bottom;
}

partial struct ConsoleScreenBufferInfo {
    public Coord Size;
    public Coord CursorPosition;
    public uint16 Attributes;
    public SmallRect Window;
    public Coord MaximumWindowSize;
}

public static UntypedInt UNIX_PATH_MAX => 108;

public static UntypedInt JOB_OBJECT_LIMIT_ACTIVE_PROCESS => 0x00000008;
public static UntypedInt JOB_OBJECT_LIMIT_AFFINITY => 0x00000010;
public static UntypedInt JOB_OBJECT_LIMIT_BREAKAWAY_OK => 0x00000800;
public static UntypedInt JOB_OBJECT_LIMIT_DIE_ON_UNHANDLED_EXCEPTION => 0x00000400;
public static UntypedInt JOB_OBJECT_LIMIT_JOB_MEMORY => 0x00000200;
public static UntypedInt JOB_OBJECT_LIMIT_JOB_TIME => 0x00000004;
public static UntypedInt JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE => 0x00002000;
public static UntypedInt JOB_OBJECT_LIMIT_PRESERVE_JOB_TIME => 0x00000040;
public static UntypedInt JOB_OBJECT_LIMIT_PRIORITY_CLASS => 0x00000020;
public static UntypedInt JOB_OBJECT_LIMIT_PROCESS_MEMORY => 0x00000100;
public static UntypedInt JOB_OBJECT_LIMIT_PROCESS_TIME => 0x00000002;
public static UntypedInt JOB_OBJECT_LIMIT_SCHEDULING_CLASS => 0x00000080;
public static UntypedInt JOB_OBJECT_LIMIT_SILENT_BREAKAWAY_OK => 0x00001000;
public static UntypedInt JOB_OBJECT_LIMIT_SUBSET_AFFINITY => 0x00004000;
public static UntypedInt JOB_OBJECT_LIMIT_WORKINGSET => 0x00000001;

partial struct IO_COUNTERS {
    public uint64 ReadOperationCount;
    public uint64 WriteOperationCount;
    public uint64 OtherOperationCount;
    public uint64 ReadTransferCount;
    public uint64 WriteTransferCount;
    public uint64 OtherTransferCount;
}

partial struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION {
    public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
    public IO_COUNTERS IoInfo;
    public uintptr ProcessMemoryLimit;
    public uintptr JobMemoryLimit;
    public uintptr PeakProcessMemoryUsed;
    public uintptr PeakJobMemoryUsed;
}

public static UntypedInt JOB_OBJECT_UILIMIT_DESKTOP => 0x00000040;
public static UntypedInt JOB_OBJECT_UILIMIT_DISPLAYSETTINGS => 0x00000010;
public static UntypedInt JOB_OBJECT_UILIMIT_EXITWINDOWS => 0x00000080;
public static UntypedInt JOB_OBJECT_UILIMIT_GLOBALATOMS => 0x00000020;
public static UntypedInt JOB_OBJECT_UILIMIT_HANDLES => 0x00000001;
public static UntypedInt JOB_OBJECT_UILIMIT_READCLIPBOARD => 0x00000002;
public static UntypedInt JOB_OBJECT_UILIMIT_SYSTEMPARAMETERS => 0x00000008;
public static UntypedInt JOB_OBJECT_UILIMIT_WRITECLIPBOARD => 0x00000004;

partial struct JOBOBJECT_BASIC_UI_RESTRICTIONS {
    public uint32 UIRestrictionsClass;
}

public static UntypedInt JobObjectAssociateCompletionPortInformation => 7;
public static UntypedInt JobObjectBasicAccountingInformation => 1;
public static UntypedInt JobObjectBasicAndIoAccountingInformation => 8;
public static UntypedInt JobObjectBasicLimitInformation => 2;
public static UntypedInt JobObjectBasicProcessIdList => 3;
public static UntypedInt JobObjectBasicUIRestrictions => 4;
public static UntypedInt JobObjectCpuRateControlInformation => 15;
public static UntypedInt JobObjectEndOfJobTimeInformation => 6;
public static UntypedInt JobObjectExtendedLimitInformation => 9;
public static UntypedInt JobObjectGroupInformation => 11;
public static UntypedInt JobObjectGroupInformationEx => 14;
public static UntypedInt JobObjectLimitViolationInformation => 13;
public static UntypedInt JobObjectLimitViolationInformation2 => 34;
public static UntypedInt JobObjectNetRateControlInformation => 32;
public static UntypedInt JobObjectNotificationLimitInformation => 12;
public static UntypedInt JobObjectNotificationLimitInformation2 => 33;
public static UntypedInt JobObjectSecurityLimitInformation => 5;

public static UntypedInt KF_FLAG_DEFAULT => 0x00000000;
public static UntypedInt KF_FLAG_FORCE_APP_DATA_REDIRECTION => 0x00080000;
public static UntypedInt KF_FLAG_RETURN_FILTER_REDIRECTION_TARGET => 0x00040000;
public static UntypedInt KF_FLAG_FORCE_PACKAGE_REDIRECTION => 0x00020000;
public static UntypedInt KF_FLAG_NO_PACKAGE_REDIRECTION => 0x00010000;
public static UntypedInt KF_FLAG_FORCE_APPCONTAINER_REDIRECTION => 0x00020000;
public static UntypedInt KF_FLAG_NO_APPCONTAINER_REDIRECTION => 0x00010000;
public static UntypedInt KF_FLAG_CREATE => 0x00008000;
public static UntypedInt KF_FLAG_DONT_VERIFY => 0x00004000;
public static UntypedInt KF_FLAG_DONT_UNEXPAND => 0x00002000;
public static UntypedInt KF_FLAG_NO_ALIAS => 0x00001000;
public static UntypedInt KF_FLAG_INIT => 0x00000800;
public static UntypedInt KF_FLAG_DEFAULT_PATH => 0x00000400;
public static UntypedInt KF_FLAG_NOT_PARENT_RELATIVE => 0x00000200;
public static UntypedInt KF_FLAG_SIMPLE_IDLIST => 0x00000100;
public static UntypedInt KF_FLAG_ALIAS_ONLY => 0x80000000;

[global::go.golib.GoInlineLayout(Size = 284, Align = 4), StructLayout(LayoutKind.Explicit, Size = 284)] partial struct OsVersionInfoEx {
    [FieldOffset(0)] internal uint32 osVersionInfoSize;
    [FieldOffset(4)] public uint32 MajorVersion;
    [FieldOffset(8)] public uint32 MinorVersion;
    [FieldOffset(12)] public uint32 BuildNumber;
    [FieldOffset(16)] public uint32 PlatformId;
    [FieldOffset(20)] public array128<uint16> CsdVersion;
    [FieldOffset(276)] public uint16 ServicePackMajor;
    [FieldOffset(278)] public uint16 ServicePackMinor;
    [FieldOffset(280)] public uint16 SuiteMask;
    [FieldOffset(282)] public byte ProductType;
    [FieldOffset(283)] internal byte _;
}

public static UntypedInt EWX_LOGOFF => 0x00000000;
public static UntypedInt EWX_SHUTDOWN => 0x00000001;
public static UntypedInt EWX_REBOOT => 0x00000002;
public static UntypedInt EWX_FORCE => 0x00000004;
public static UntypedInt EWX_POWEROFF => 0x00000008;
public static UntypedInt EWX_FORCEIFHUNG => 0x00000010;
public static UntypedInt EWX_QUICKRESOLVE => 0x00000020;
public static UntypedInt EWX_RESTARTAPPS => 0x00000040;
public static UntypedInt EWX_HYBRID_SHUTDOWN => 0x00400000;
public static UntypedInt EWX_BOOTOPTIONS => 0x01000000;
public static UntypedInt SHTDN_REASON_FLAG_COMMENT_REQUIRED => 0x01000000;
public static UntypedInt SHTDN_REASON_FLAG_DIRTY_PROBLEM_ID_REQUIRED => 0x02000000;
public static UntypedInt SHTDN_REASON_FLAG_CLEAN_UI => 0x04000000;
public static UntypedInt SHTDN_REASON_FLAG_DIRTY_UI => 0x08000000;
public static UntypedInt SHTDN_REASON_FLAG_USER_DEFINED => 0x40000000;
public static UntypedInt SHTDN_REASON_FLAG_PLANNED => 0x80000000;
public static UntypedInt SHTDN_REASON_MAJOR_OTHER => 0x00000000;
public static UntypedInt SHTDN_REASON_MAJOR_NONE => 0x00000000;
public static UntypedInt SHTDN_REASON_MAJOR_HARDWARE => 0x00010000;
public static UntypedInt SHTDN_REASON_MAJOR_OPERATINGSYSTEM => 0x00020000;
public static UntypedInt SHTDN_REASON_MAJOR_SOFTWARE => 0x00030000;
public static UntypedInt SHTDN_REASON_MAJOR_APPLICATION => 0x00040000;
public static UntypedInt SHTDN_REASON_MAJOR_SYSTEM => 0x00050000;
public static UntypedInt SHTDN_REASON_MAJOR_POWER => 0x00060000;
public static UntypedInt SHTDN_REASON_MAJOR_LEGACY_API => 0x00070000;
public static UntypedInt SHTDN_REASON_MINOR_OTHER => 0x00000000;
public static UntypedInt SHTDN_REASON_MINOR_NONE => 0x000000ff;
public static UntypedInt SHTDN_REASON_MINOR_MAINTENANCE => 0x00000001;
public static UntypedInt SHTDN_REASON_MINOR_INSTALLATION => 0x00000002;
public static UntypedInt SHTDN_REASON_MINOR_UPGRADE => 0x00000003;
public static UntypedInt SHTDN_REASON_MINOR_RECONFIG => 0x00000004;
public static UntypedInt SHTDN_REASON_MINOR_HUNG => 0x00000005;
public static UntypedInt SHTDN_REASON_MINOR_UNSTABLE => 0x00000006;
public static UntypedInt SHTDN_REASON_MINOR_DISK => 0x00000007;
public static UntypedInt SHTDN_REASON_MINOR_PROCESSOR => 0x00000008;
public static UntypedInt SHTDN_REASON_MINOR_NETWORKCARD => 0x00000009;
public static UntypedInt SHTDN_REASON_MINOR_POWER_SUPPLY => 0x0000000a;
public static UntypedInt SHTDN_REASON_MINOR_CORDUNPLUGGED => 0x0000000b;
public static UntypedInt SHTDN_REASON_MINOR_ENVIRONMENT => 0x0000000c;
public static UntypedInt SHTDN_REASON_MINOR_HARDWARE_DRIVER => 0x0000000d;
public static UntypedInt SHTDN_REASON_MINOR_OTHERDRIVER => 0x0000000e;
public static UntypedInt SHTDN_REASON_MINOR_BLUESCREEN => 0x0000000F;
public static UntypedInt SHTDN_REASON_MINOR_SERVICEPACK => 0x00000010;
public static UntypedInt SHTDN_REASON_MINOR_HOTFIX => 0x00000011;
public static UntypedInt SHTDN_REASON_MINOR_SECURITYFIX => 0x00000012;
public static UntypedInt SHTDN_REASON_MINOR_SECURITY => 0x00000013;
public static UntypedInt SHTDN_REASON_MINOR_NETWORK_CONNECTIVITY => 0x00000014;
public static UntypedInt SHTDN_REASON_MINOR_WMI => 0x00000015;
public static UntypedInt SHTDN_REASON_MINOR_SERVICEPACK_UNINSTALL => 0x00000016;
public static UntypedInt SHTDN_REASON_MINOR_HOTFIX_UNINSTALL => 0x00000017;
public static UntypedInt SHTDN_REASON_MINOR_SECURITYFIX_UNINSTALL => 0x00000018;
public static UntypedInt SHTDN_REASON_MINOR_MMC => 0x00000019;
public static UntypedInt SHTDN_REASON_MINOR_SYSTEMRESTORE => 0x0000001a;
public static UntypedInt SHTDN_REASON_MINOR_TERMSRV => 0x00000020;
public static UntypedInt SHTDN_REASON_MINOR_DC_PROMOTION => 0x00000021;
public static UntypedInt SHTDN_REASON_MINOR_DC_DEMOTION => 0x00000022;
public static UntypedInt SHTDN_REASON_UNKNOWN => /* SHTDN_REASON_MINOR_NONE */ 255;
public static UntypedInt SHTDN_REASON_LEGACY_API => /* SHTDN_REASON_MAJOR_LEGACY_API | SHTDN_REASON_FLAG_PLANNED */ 2147942400;
public static UntypedInt SHTDN_REASON_VALID_BIT_MASK => 0xc0ffffff;
public static UntypedInt SHUTDOWN_NORETRY => 0x1;

public static UntypedInt GET_MODULE_HANDLE_EX_FLAG_PIN => 1;
public static UntypedInt GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT => 2;
public static UntypedInt GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS => 4;

public static UntypedInt MUI_LANGUAGE_ID => 0x4;
public static UntypedInt MUI_LANGUAGE_NAME => 0x8;
public static UntypedInt MUI_MERGE_SYSTEM_FALLBACK => 0x10;
public static UntypedInt MUI_MERGE_USER_FALLBACK => 0x20;
public static UntypedInt MUI_UI_FALLBACK => /* MUI_MERGE_SYSTEM_FALLBACK | MUI_MERGE_USER_FALLBACK */ 48;
public static UntypedInt MUI_THREAD_LANGUAGES => 0x40;
public static UntypedInt MUI_CONSOLE_FILTER => 0x100;
public static UntypedInt MUI_COMPLEX_SCRIPT_FILTER => 0x200;
public static UntypedInt MUI_RESET_FILTERS => 0x001;
public static UntypedInt MUI_USER_PREFERRED_UI_LANGUAGES => 0x10;
public static UntypedInt MUI_USE_INSTALLED_LANGUAGES => 0x20;
public static UntypedInt MUI_USE_SEARCH_ALL_LANGUAGES => 0x40;
public static UntypedInt MUI_LANG_NEUTRAL_PE_FILE => 0x100;
public static UntypedInt MUI_NON_LANG_NEUTRAL_FILE => 0x200;
public static UntypedInt MUI_MACHINE_LANGUAGE_SETTINGS => 0x400;
public static UntypedInt MUI_FILETYPE_NOT_LANGUAGE_NEUTRAL => 0x001;
public static UntypedInt MUI_FILETYPE_LANGUAGE_NEUTRAL_MAIN => 0x002;
public static UntypedInt MUI_FILETYPE_LANGUAGE_NEUTRAL_MUI => 0x004;
public static UntypedInt MUI_QUERY_TYPE => 0x001;
public static UntypedInt MUI_QUERY_CHECKSUM => 0x002;
public static UntypedInt MUI_QUERY_LANGUAGE_NAME => 0x004;
public static UntypedInt MUI_QUERY_RESOURCE_TYPES => 0x008;
public static UntypedInt MUI_FILEINFO_VERSION => 0x001;
public static UntypedInt MUI_FULL_LANGUAGE => 0x01;
public static UntypedInt MUI_PARTIAL_LANGUAGE => 0x02;
public static UntypedInt MUI_LIP_LANGUAGE => 0x04;
public static UntypedInt MUI_LANGUAGE_INSTALLED => 0x20;
public static UntypedInt MUI_LANGUAGE_LICENSED => 0x40;

public static UntypedInt FileBasicInfo => 0;
public static UntypedInt FileStandardInfo => 1;
public static UntypedInt FileNameInfo => 2;
public static UntypedInt FileRenameInfo => 3;
public static UntypedInt FileDispositionInfo => 4;
public static UntypedInt FileAllocationInfo => 5;
public static UntypedInt FileEndOfFileInfo => 6;
public static UntypedInt FileStreamInfo => 7;
public static UntypedInt FileCompressionInfo => 8;
public static UntypedInt FileAttributeTagInfo => 9;
public static UntypedInt FileIdBothDirectoryInfo => 10;
public static UntypedInt FileIdBothDirectoryRestartInfo => 11;
public static UntypedInt FileIoPriorityHintInfo => 12;
public static UntypedInt FileRemoteProtocolInfo => 13;
public static UntypedInt FileFullDirectoryInfo => 14;
public static UntypedInt FileFullDirectoryRestartInfo => 15;
public static UntypedInt FileStorageInfo => 16;
public static UntypedInt FileAlignmentInfo => 17;
public static UntypedInt FileIdInfo => 18;
public static UntypedInt FileIdExtdDirectoryInfo => 19;
public static UntypedInt FileIdExtdDirectoryRestartInfo => 20;
public static UntypedInt FileDispositionInfoEx => 21;
public static UntypedInt FileRenameInfoEx => 22;
public static UntypedInt FileCaseSensitiveInfo => 23;
public static UntypedInt FileNormalizedNameInfo => 24;

public static UntypedInt DONT_RESOLVE_DLL_REFERENCES => 0x1;
public static UntypedInt LOAD_LIBRARY_AS_DATAFILE => 0x2;
public static UntypedInt LOAD_WITH_ALTERED_SEARCH_PATH => 0x8;
public static UntypedInt LOAD_IGNORE_CODE_AUTHZ_LEVEL => 0x10;
public static UntypedInt LOAD_LIBRARY_AS_IMAGE_RESOURCE => 0x20;
public static UntypedInt LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE => 0x40;
public static UntypedInt LOAD_LIBRARY_REQUIRE_SIGNED_TARGET => 0x80;
public static UntypedInt LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR => 0x100;
public static UntypedInt LOAD_LIBRARY_SEARCH_APPLICATION_DIR => 0x200;
public static UntypedInt LOAD_LIBRARY_SEARCH_USER_DIRS => 0x400;
public static UntypedInt LOAD_LIBRARY_SEARCH_SYSTEM32 => 0x800;
public static UntypedInt LOAD_LIBRARY_SEARCH_DEFAULT_DIRS => 0x1000;
public static UntypedInt LOAD_LIBRARY_SAFE_CURRENT_DIRS => 0x00002000;
public static UntypedInt LOAD_LIBRARY_SEARCH_SYSTEM32_NO_FORWARDER => 0x00004000;
public static UntypedInt LOAD_LIBRARY_OS_INTEGRITY_CONTINUITY => 0x00008000;

public static UntypedInt REG_NOTIFY_CHANGE_NAME => 0x00000001;
public static UntypedInt REG_NOTIFY_CHANGE_ATTRIBUTES => 0x00000002;
public static UntypedInt REG_NOTIFY_CHANGE_LAST_SET => 0x00000004;
public static UntypedInt REG_NOTIFY_CHANGE_SECURITY => 0x00000008;
public static UntypedInt REG_NOTIFY_THREAD_AGNOSTIC => 0x10000000;

partial struct CommTimeouts {
    public uint32 ReadIntervalTimeout;
    public uint32 ReadTotalTimeoutMultiplier;
    public uint32 ReadTotalTimeoutConstant;
    public uint32 WriteTotalTimeoutMultiplier;
    public uint32 WriteTotalTimeoutConstant;
}

partial struct NTUnicodeString {
    public uint16 Length;
    public uint16 MaximumLength;
    public ж<uint16> Buffer;
}

partial struct NTString {
    public uint16 Length;
    public uint16 MaximumLength;
    public ж<byte> Buffer;
}

partial struct LIST_ENTRY {
    public ж<LIST_ENTRY> Flink;
    public ж<LIST_ENTRY> Blink;
}

partial struct RUNTIME_FUNCTION {
    public uint32 BeginAddress;
    public uint32 EndAddress;
    public uint32 UnwindData;
}

partial struct LDR_DATA_TABLE_ENTRY {
    internal array<uintptr> reserved1 = new(2);
    public LIST_ENTRY InMemoryOrderLinks;
    internal array<uintptr> reserved2 = new(2);
    public uintptr DllBase;
    internal array<uintptr> reserved3 = new(2);
    public NTUnicodeString FullDllName;
    internal array<byte> reserved4 = new(8);
    internal array<uintptr> reserved5 = new(3);
    internal uintptr reserved6;
    public uint32 TimeDateStamp;
}

partial struct PEB_LDR_DATA {
    internal array<byte> reserved1 = new(8);
    internal array<uintptr> reserved2 = new(3);
    public LIST_ENTRY InMemoryOrderModuleList;
}

partial struct CURDIR {
    public NTUnicodeString DosPath;
    public Handle Handle;
}

partial struct RTL_DRIVE_LETTER_CURDIR {
    public uint16 Flags;
    public uint16 Length;
    public uint32 TimeStamp;
    public NTString DosPath;
}

partial struct RTL_USER_PROCESS_PARAMETERS {
    public uint32 MaximumLength, Length;
    public uint32 Flags, DebugFlags;
    public Handle ConsoleHandle;
    public uint32 ConsoleFlags;
    public Handle StandardInput, StandardOutput, StandardError;
    public CURDIR CurrentDirectory;
    public NTUnicodeString DllPath;
    public NTUnicodeString ImagePathName;
    public NTUnicodeString CommandLine;
    public @unsafe.Pointer Environment;
    public uint32 StartingX, StartingY, CountX, CountY, CountCharsX, CountCharsY, FillAttribute;
    public uint32 WindowFlags, ShowWindowFlags;
    public NTUnicodeString WindowTitle, DesktopInfo, ShellInfo, RuntimeData;
    public array<RTL_DRIVE_LETTER_CURDIR> CurrentDirectories = new(32);
    public uintptr EnvironmentSize, EnvironmentVersion;
    public @unsafe.Pointer PackageDependencyData;
    public uint32 ProcessGroupId;
    public uint32 LoaderThreads;
    public NTUnicodeString RedirectionDllName;
    public NTUnicodeString HeapPartitionName;
    public uintptr DefaultThreadpoolCpuSetMasks;
    public uint32 DefaultThreadpoolCpuSetMaskCount;
}

partial struct PEB {
    internal array<byte> reserved1 = new(2);
    public byte BeingDebugged;
    public byte BitField;
    internal uintptr reserved3;
    public uintptr ImageBaseAddress;
    public ж<PEB_LDR_DATA> Ldr;
    public ж<RTL_USER_PROCESS_PARAMETERS> ProcessParameters;
    internal array<uintptr> reserved4 = new(3);
    public uintptr AtlThunkSListPtr;
    internal uintptr reserved5;
    internal uint32 reserved6;
    internal uintptr reserved7;
    internal uint32 reserved8;
    public uint32 AtlThunkSListPtr32;
    internal array<uintptr> reserved9 = new(45);
    internal array<byte> reserved10 = new(96);
    public uintptr PostProcessInitRoutine;
    internal array<byte> reserved11 = new(128);
    internal array<uintptr> reserved12 = new(1);
    public uint32 SessionId;
}

partial struct OBJECT_ATTRIBUTES {
    public uint32 Length;
    public Handle RootDirectory;
    public ж<NTUnicodeString> ObjectName;
    public uint32 Attributes;
    public ж<SECURITY_DESCRIPTOR> SecurityDescriptor;
    public ж<SECURITY_QUALITY_OF_SERVICE> SecurityQoS;
}

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

partial struct IO_STATUS_BLOCK {
    public NTStatus Status;
    public uintptr Information;
}

partial struct RTLP_CURDIR_REF {
    public int32 RefCount;
    public Handle Handle;
}

partial struct RTL_RELATIVE_NAME {
    public NTUnicodeString RelativeName;
    public Handle ContainingDirectory;
    public ж<RTLP_CURDIR_REF> CurDirRef;
}

public static UntypedInt FILE_SUPERSEDE => 0x00000000;
public static UntypedInt FILE_OPEN => 0x00000001;
public static UntypedInt FILE_CREATE => 0x00000002;
public static UntypedInt FILE_OPEN_IF => 0x00000003;
public static UntypedInt FILE_OVERWRITE => 0x00000004;
public static UntypedInt FILE_OVERWRITE_IF => 0x00000005;
public static UntypedInt FILE_MAXIMUM_DISPOSITION => 0x00000005;
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
public static UntypedInt FILE_RESERVE_OPFILTER => 0x00100000;
public static UntypedInt FILE_OPEN_REPARSE_POINT => 0x00200000;
public static UntypedInt FILE_OPEN_NO_RECALL => 0x00400000;
public static UntypedInt FILE_OPEN_FOR_FREE_SPACE_QUERY => 0x00800000;
public static UntypedInt FILE_PIPE_BYTE_STREAM_TYPE => 0x00000000;
public static UntypedInt FILE_PIPE_MESSAGE_TYPE => 0x00000001;
public static UntypedInt FILE_PIPE_ACCEPT_REMOTE_CLIENTS => 0x00000000;
public static UntypedInt FILE_PIPE_REJECT_REMOTE_CLIENTS => 0x00000002;
public static UntypedInt FILE_PIPE_TYPE_VALID_MASK => 0x00000003;
public static UntypedInt FILE_PIPE_BYTE_STREAM_MODE => 0x00000000;
public static UntypedInt FILE_PIPE_MESSAGE_MODE => 0x00000001;
public static UntypedInt FILE_PIPE_QUEUE_OPERATION => 0x00000000;
public static UntypedInt FILE_PIPE_COMPLETE_OPERATION => 0x00000001;
public static UntypedInt FILE_PIPE_INBOUND => 0x00000000;
public static UntypedInt FILE_PIPE_OUTBOUND => 0x00000001;
public static UntypedInt FILE_PIPE_FULL_DUPLEX => 0x00000002;
public static UntypedInt FILE_PIPE_DISCONNECTED_STATE => 0x00000001;
public static UntypedInt FILE_PIPE_LISTENING_STATE => 0x00000002;
public static UntypedInt FILE_PIPE_CONNECTED_STATE => 0x00000003;
public static UntypedInt FILE_PIPE_CLOSING_STATE => 0x00000004;
public static UntypedInt FILE_PIPE_CLIENT_END => 0x00000000;
public static UntypedInt FILE_PIPE_SERVER_END => 0x00000001;

public static UntypedInt FileBasicInformation => 4;
public static UntypedInt FileEaInformation => 7;
public static UntypedInt FileRenameInformation => 10;
public static UntypedInt FileDispositionInformation => 13;
public static UntypedInt FilePositionInformation => 14;
public static UntypedInt FileEndOfFileInformation => 20;
public static UntypedInt FileValidDataLengthInformation => 39;
public static UntypedInt FileShortNameInformation => 40;
public static UntypedInt FileIoPriorityHintInformation => 43;
public static UntypedInt FileReplaceCompletionInformation => 61;
public static UntypedInt FileDispositionInformationEx => 64;
public static UntypedInt FileCaseSensitiveInformation => 71;
public static UntypedInt FileLinkInformation => 72;
public static UntypedInt FileCaseSensitiveInformationForceAccessCheck => 75;
public static UntypedInt FileKnownFolderInformation => 76;
public static UntypedInt FILE_RENAME_REPLACE_IF_EXISTS => 0x00000001;
public static UntypedInt FILE_RENAME_POSIX_SEMANTICS => 0x00000002;
public static UntypedInt FILE_RENAME_SUPPRESS_PIN_STATE_INHERITANCE => 0x00000004;
public static UntypedInt FILE_RENAME_SUPPRESS_STORAGE_RESERVE_INHERITANCE => 0x00000008;
public static UntypedInt FILE_RENAME_NO_INCREASE_AVAILABLE_SPACE => 0x00000010;
public static UntypedInt FILE_RENAME_NO_DECREASE_AVAILABLE_SPACE => 0x00000020;
public static UntypedInt FILE_RENAME_PRESERVE_AVAILABLE_SPACE => 0x00000030;
public static UntypedInt FILE_RENAME_IGNORE_READONLY_ATTRIBUTE => 0x00000040;
public static UntypedInt FILE_RENAME_FORCE_RESIZE_TARGET_SR => 0x00000080;
public static UntypedInt FILE_RENAME_FORCE_RESIZE_SOURCE_SR => 0x00000100;
public static UntypedInt FILE_RENAME_FORCE_RESIZE_SR => 0x00000180;
public static UntypedInt FILE_DISPOSITION_DO_NOT_DELETE => 0x00000000;
public static UntypedInt FILE_DISPOSITION_DELETE => 0x00000001;
public static UntypedInt FILE_DISPOSITION_POSIX_SEMANTICS => 0x00000002;
public static UntypedInt FILE_DISPOSITION_FORCE_IMAGE_SECTION_CHECK => 0x00000004;
public static UntypedInt FILE_DISPOSITION_ON_CLOSE => 0x00000008;
public static UntypedInt FILE_DISPOSITION_IGNORE_READONLY_ATTRIBUTE => 0x00000010;
public static UntypedInt FILE_CS_FLAG_CASE_SENSITIVE_DIR => 0x00000001;
public static UntypedInt FILE_LINK_REPLACE_IF_EXISTS => 0x00000001;
public static UntypedInt FILE_LINK_POSIX_SEMANTICS => 0x00000002;
public static UntypedInt FILE_LINK_SUPPRESS_STORAGE_RESERVE_INHERITANCE => 0x00000008;
public static UntypedInt FILE_LINK_NO_INCREASE_AVAILABLE_SPACE => 0x00000010;
public static UntypedInt FILE_LINK_NO_DECREASE_AVAILABLE_SPACE => 0x00000020;
public static UntypedInt FILE_LINK_PRESERVE_AVAILABLE_SPACE => 0x00000030;
public static UntypedInt FILE_LINK_IGNORE_READONLY_ATTRIBUTE => 0x00000040;
public static UntypedInt FILE_LINK_FORCE_RESIZE_TARGET_SR => 0x00000080;
public static UntypedInt FILE_LINK_FORCE_RESIZE_SOURCE_SR => 0x00000100;
public static UntypedInt FILE_LINK_FORCE_RESIZE_SR => 0x00000180;

public static UntypedInt ProcessBasicInformation => iota;
public static UntypedInt ProcessQuotaLimits => 1;
public static UntypedInt ProcessIoCounters => 2;
public static UntypedInt ProcessVmCounters => 3;
public static UntypedInt ProcessTimes => 4;
public static UntypedInt ProcessBasePriority => 5;
public static UntypedInt ProcessRaisePriority => 6;
public static UntypedInt ProcessDebugPort => 7;
public static UntypedInt ProcessExceptionPort => 8;
public static UntypedInt ProcessAccessToken => 9;
public static UntypedInt ProcessLdtInformation => 10;
public static UntypedInt ProcessLdtSize => 11;
public static UntypedInt ProcessDefaultHardErrorMode => 12;
public static UntypedInt ProcessIoPortHandlers => 13;
public static UntypedInt ProcessPooledUsageAndLimits => 14;
public static UntypedInt ProcessWorkingSetWatch => 15;
public static UntypedInt ProcessUserModeIOPL => 16;
public static UntypedInt ProcessEnableAlignmentFaultFixup => 17;
public static UntypedInt ProcessPriorityClass => 18;
public static UntypedInt ProcessWx86Information => 19;
public static UntypedInt ProcessHandleCount => 20;
public static UntypedInt ProcessAffinityMask => 21;
public static UntypedInt ProcessPriorityBoost => 22;
public static UntypedInt ProcessDeviceMap => 23;
public static UntypedInt ProcessSessionInformation => 24;
public static UntypedInt ProcessForegroundInformation => 25;
public static UntypedInt ProcessWow64Information => 26;
public static UntypedInt ProcessImageFileName => 27;
public static UntypedInt ProcessLUIDDeviceMapsEnabled => 28;
public static UntypedInt ProcessBreakOnTermination => 29;
public static UntypedInt ProcessDebugObjectHandle => 30;
public static UntypedInt ProcessDebugFlags => 31;
public static UntypedInt ProcessHandleTracing => 32;
public static UntypedInt ProcessIoPriority => 33;
public static UntypedInt ProcessExecuteFlags => 34;
public static UntypedInt ProcessTlsInformation => 35;
public static UntypedInt ProcessCookie => 36;
public static UntypedInt ProcessImageInformation => 37;
public static UntypedInt ProcessCycleTime => 38;
public static UntypedInt ProcessPagePriority => 39;
public static UntypedInt ProcessInstrumentationCallback => 40;
public static UntypedInt ProcessThreadStackAllocation => 41;
public static UntypedInt ProcessWorkingSetWatchEx => 42;
public static UntypedInt ProcessImageFileNameWin32 => 43;
public static UntypedInt ProcessImageFileMapping => 44;
public static UntypedInt ProcessAffinityUpdateMode => 45;
public static UntypedInt ProcessMemoryAllocationMode => 46;
public static UntypedInt ProcessGroupInformation => 47;
public static UntypedInt ProcessTokenVirtualizationEnabled => 48;
public static UntypedInt ProcessConsoleHostProcess => 49;
public static UntypedInt ProcessWindowInformation => 50;
public static UntypedInt ProcessHandleInformation => 51;
public static UntypedInt ProcessMitigationPolicy => 52;
public static UntypedInt ProcessDynamicFunctionTableInformation => 53;
public static UntypedInt ProcessHandleCheckingMode => 54;
public static UntypedInt ProcessKeepAliveCount => 55;
public static UntypedInt ProcessRevokeFileHandles => 56;
public static UntypedInt ProcessWorkingSetControl => 57;
public static UntypedInt ProcessHandleTable => 58;
public static UntypedInt ProcessCheckStackExtentsMode => 59;
public static UntypedInt ProcessCommandLineInformation => 60;
public static UntypedInt ProcessProtectionInformation => 61;
public static UntypedInt ProcessMemoryExhaustion => 62;
public static UntypedInt ProcessFaultInformation => 63;
public static UntypedInt ProcessTelemetryIdInformation => 64;
public static UntypedInt ProcessCommitReleaseInformation => 65;
public static UntypedInt ProcessDefaultCpuSetsInformation => 66;
public static UntypedInt ProcessAllowedCpuSetsInformation => 67;
public static UntypedInt ProcessSubsystemProcess => 68;
public static UntypedInt ProcessJobMemoryInformation => 69;
public static UntypedInt ProcessInPrivate => 70;
public static UntypedInt ProcessRaiseUMExceptionOnInvalidHandleClose => 71;
public static UntypedInt ProcessIumChallengeResponse => 72;
public static UntypedInt ProcessChildProcessInformation => 73;
public static UntypedInt ProcessHighGraphicsPriorityInformation => 74;
public static UntypedInt ProcessSubsystemInformation => 75;
public static UntypedInt ProcessEnergyValues => 76;
public static UntypedInt ProcessActivityThrottleState => 77;
public static UntypedInt ProcessActivityThrottlePolicy => 78;
public static UntypedInt ProcessWin32kSyscallFilterInformation => 79;
public static UntypedInt ProcessDisableSystemAllowedCpuSets => 80;
public static UntypedInt ProcessWakeInformation => 81;
public static UntypedInt ProcessEnergyTrackingState => 82;
public static UntypedInt ProcessManageWritesToExecutableMemory => 83;
public static UntypedInt ProcessCaptureTrustletLiveDump => 84;
public static UntypedInt ProcessTelemetryCoverage => 85;
public static UntypedInt ProcessEnclaveInformation => 86;
public static UntypedInt ProcessEnableReadWriteVmLogging => 87;
public static UntypedInt ProcessUptimeInformation => 88;
public static UntypedInt ProcessImageSection => 89;
public static UntypedInt ProcessDebugAuthInformation => 90;
public static UntypedInt ProcessSystemResourceManagement => 91;
public static UntypedInt ProcessSequenceNumber => 92;
public static UntypedInt ProcessLoaderDetour => 93;
public static UntypedInt ProcessSecurityDomainInformation => 94;
public static UntypedInt ProcessCombineSecurityDomainsInformation => 95;
public static UntypedInt ProcessEnableLogging => 96;
public static UntypedInt ProcessLeapSecondInformation => 97;
public static UntypedInt ProcessFiberShadowStackAllocation => 98;
public static UntypedInt ProcessFreeFiberShadowStackAllocation => 99;
public static UntypedInt ProcessAltSystemCallInformation => 100;
public static UntypedInt ProcessDynamicEHContinuationTargets => 101;
public static UntypedInt ProcessDynamicEnforcedCetCompatibleRanges => 102;

partial struct PROCESS_BASIC_INFORMATION {
    public NTStatus ExitStatus;
    public ж<PEB> PebBaseAddress;
    public uintptr AffinityMask;
    public int32 BasePriority;
    public uintptr UniqueProcessId;
    public uintptr InheritedFromUniqueProcessId;
}

partial struct SYSTEM_PROCESS_INFORMATION {
    public uint32 NextEntryOffset;
    public uint32 NumberOfThreads;
    public int64 WorkingSetPrivateSize;
    public uint32 HardFaultCount;
    public uint32 NumberOfThreadsHighWatermark;
    public uint64 CycleTime;
    public int64 CreateTime;
    public int64 UserTime;
    public int64 KernelTime;
    public NTUnicodeString ImageName;
    public int32 BasePriority;
    public uintptr UniqueProcessID;
    public uintptr InheritedFromUniqueProcessID;
    public uint32 HandleCount;
    public uint32 SessionID;
    public ж<uint32> UniqueProcessKey;
    public uintptr PeakVirtualSize;
    public uintptr VirtualSize;
    public uint32 PageFaultCount;
    public uintptr PeakWorkingSetSize;
    public uintptr WorkingSetSize;
    public uintptr QuotaPeakPagedPoolUsage;
    public uintptr QuotaPagedPoolUsage;
    public uintptr QuotaPeakNonPagedPoolUsage;
    public uintptr QuotaNonPagedPoolUsage;
    public uintptr PagefileUsage;
    public uintptr PeakPagefileUsage;
    public uintptr PrivatePageCount;
    public int64 ReadOperationCount;
    public int64 WriteOperationCount;
    public int64 OtherOperationCount;
    public int64 ReadTransferCount;
    public int64 WriteTransferCount;
    public int64 OtherTransferCount;
}

public static UntypedInt SystemBasicInformation => iota;
public static UntypedInt SystemProcessorInformation => 1;
public static UntypedInt SystemPerformanceInformation => 2;
public static UntypedInt SystemTimeOfDayInformation => 3;
public static UntypedInt SystemPathInformation => 4;
public static UntypedInt SystemProcessInformation => 5;
public static UntypedInt SystemCallCountInformation => 6;
public static UntypedInt SystemDeviceInformation => 7;
public static UntypedInt SystemProcessorPerformanceInformation => 8;
public static UntypedInt SystemFlagsInformation => 9;
public static UntypedInt SystemCallTimeInformation => 10;
public static UntypedInt SystemModuleInformation => 11;
public static UntypedInt SystemLocksInformation => 12;
public static UntypedInt SystemStackTraceInformation => 13;
public static UntypedInt SystemPagedPoolInformation => 14;
public static UntypedInt SystemNonPagedPoolInformation => 15;
public static UntypedInt SystemHandleInformation => 16;
public static UntypedInt SystemObjectInformation => 17;
public static UntypedInt SystemPageFileInformation => 18;
public static UntypedInt SystemVdmInstemulInformation => 19;
public static UntypedInt SystemVdmBopInformation => 20;
public static UntypedInt SystemFileCacheInformation => 21;
public static UntypedInt SystemPoolTagInformation => 22;
public static UntypedInt SystemInterruptInformation => 23;
public static UntypedInt SystemDpcBehaviorInformation => 24;
public static UntypedInt SystemFullMemoryInformation => 25;
public static UntypedInt SystemLoadGdiDriverInformation => 26;
public static UntypedInt SystemUnloadGdiDriverInformation => 27;
public static UntypedInt SystemTimeAdjustmentInformation => 28;
public static UntypedInt SystemSummaryMemoryInformation => 29;
public static UntypedInt SystemMirrorMemoryInformation => 30;
public static UntypedInt SystemPerformanceTraceInformation => 31;
internal static UntypedInt systemObsolete0 => 32;
public static UntypedInt SystemExceptionInformation => 33;
public static UntypedInt SystemCrashDumpStateInformation => 34;
public static UntypedInt SystemKernelDebuggerInformation => 35;
public static UntypedInt SystemContextSwitchInformation => 36;
public static UntypedInt SystemRegistryQuotaInformation => 37;
public static UntypedInt SystemExtendServiceTableInformation => 38;
public static UntypedInt SystemPrioritySeperation => 39;
public static UntypedInt SystemVerifierAddDriverInformation => 40;
public static UntypedInt SystemVerifierRemoveDriverInformation => 41;
public static UntypedInt SystemProcessorIdleInformation => 42;
public static UntypedInt SystemLegacyDriverInformation => 43;
public static UntypedInt SystemCurrentTimeZoneInformation => 44;
public static UntypedInt SystemLookasideInformation => 45;
public static UntypedInt SystemTimeSlipNotification => 46;
public static UntypedInt SystemSessionCreate => 47;
public static UntypedInt SystemSessionDetach => 48;
public static UntypedInt SystemSessionInformation => 49;
public static UntypedInt SystemRangeStartInformation => 50;
public static UntypedInt SystemVerifierInformation => 51;
public static UntypedInt SystemVerifierThunkExtend => 52;
public static UntypedInt SystemSessionProcessInformation => 53;
public static UntypedInt SystemLoadGdiDriverInSystemSpace => 54;
public static UntypedInt SystemNumaProcessorMap => 55;
public static UntypedInt SystemPrefetcherInformation => 56;
public static UntypedInt SystemExtendedProcessInformation => 57;
public static UntypedInt SystemRecommendedSharedDataAlignment => 58;
public static UntypedInt SystemComPlusPackage => 59;
public static UntypedInt SystemNumaAvailableMemory => 60;
public static UntypedInt SystemProcessorPowerInformation => 61;
public static UntypedInt SystemEmulationBasicInformation => 62;
public static UntypedInt SystemEmulationProcessorInformation => 63;
public static UntypedInt SystemExtendedHandleInformation => 64;
public static UntypedInt SystemLostDelayedWriteInformation => 65;
public static UntypedInt SystemBigPoolInformation => 66;
public static UntypedInt SystemSessionPoolTagInformation => 67;
public static UntypedInt SystemSessionMappedViewInformation => 68;
public static UntypedInt SystemHotpatchInformation => 69;
public static UntypedInt SystemObjectSecurityMode => 70;
public static UntypedInt SystemWatchdogTimerHandler => 71;
public static UntypedInt SystemWatchdogTimerInformation => 72;
public static UntypedInt SystemLogicalProcessorInformation => 73;
public static UntypedInt SystemWow64SharedInformationObsolete => 74;
public static UntypedInt SystemRegisterFirmwareTableInformationHandler => 75;
public static UntypedInt SystemFirmwareTableInformation => 76;
public static UntypedInt SystemModuleInformationEx => 77;
public static UntypedInt SystemVerifierTriageInformation => 78;
public static UntypedInt SystemSuperfetchInformation => 79;
public static UntypedInt SystemMemoryListInformation => 80;
public static UntypedInt SystemFileCacheInformationEx => 81;
public static UntypedInt SystemThreadPriorityClientIdInformation => 82;
public static UntypedInt SystemProcessorIdleCycleTimeInformation => 83;
public static UntypedInt SystemVerifierCancellationInformation => 84;
public static UntypedInt SystemProcessorPowerInformationEx => 85;
public static UntypedInt SystemRefTraceInformation => 86;
public static UntypedInt SystemSpecialPoolInformation => 87;
public static UntypedInt SystemProcessIdInformation => 88;
public static UntypedInt SystemErrorPortInformation => 89;
public static UntypedInt SystemBootEnvironmentInformation => 90;
public static UntypedInt SystemHypervisorInformation => 91;
public static UntypedInt SystemVerifierInformationEx => 92;
public static UntypedInt SystemTimeZoneInformation => 93;
public static UntypedInt SystemImageFileExecutionOptionsInformation => 94;
public static UntypedInt SystemCoverageInformation => 95;
public static UntypedInt SystemPrefetchPatchInformation => 96;
public static UntypedInt SystemVerifierFaultsInformation => 97;
public static UntypedInt SystemSystemPartitionInformation => 98;
public static UntypedInt SystemSystemDiskInformation => 99;
public static UntypedInt SystemProcessorPerformanceDistribution => 100;
public static UntypedInt SystemNumaProximityNodeInformation => 101;
public static UntypedInt SystemDynamicTimeZoneInformation => 102;
public static UntypedInt SystemCodeIntegrityInformation => 103;
public static UntypedInt SystemProcessorMicrocodeUpdateInformation => 104;
public static UntypedInt SystemProcessorBrandString => 105;
public static UntypedInt SystemVirtualAddressInformation => 106;
public static UntypedInt SystemLogicalProcessorAndGroupInformation => 107;
public static UntypedInt SystemProcessorCycleTimeInformation => 108;
public static UntypedInt SystemStoreInformation => 109;
public static UntypedInt SystemRegistryAppendString => 110;
public static UntypedInt SystemAitSamplingValue => 111;
public static UntypedInt SystemVhdBootInformation => 112;
public static UntypedInt SystemCpuQuotaInformation => 113;
public static UntypedInt SystemNativeBasicInformation => 114;
internal static UntypedInt systemSpare1 => 115;
public static UntypedInt SystemLowPriorityIoInformation => 116;
public static UntypedInt SystemTpmBootEntropyInformation => 117;
public static UntypedInt SystemVerifierCountersInformation => 118;
public static UntypedInt SystemPagedPoolInformationEx => 119;
public static UntypedInt SystemSystemPtesInformationEx => 120;
public static UntypedInt SystemNodeDistanceInformation => 121;
public static UntypedInt SystemAcpiAuditInformation => 122;
public static UntypedInt SystemBasicPerformanceInformation => 123;
public static UntypedInt SystemQueryPerformanceCounterInformation => 124;
public static UntypedInt SystemSessionBigPoolInformation => 125;
public static UntypedInt SystemBootGraphicsInformation => 126;
public static UntypedInt SystemScrubPhysicalMemoryInformation => 127;
public static UntypedInt SystemBadPageInformation => 128;
public static UntypedInt SystemProcessorProfileControlArea => 129;
public static UntypedInt SystemCombinePhysicalMemoryInformation => 130;
public static UntypedInt SystemEntropyInterruptTimingCallback => 131;
public static UntypedInt SystemConsoleInformation => 132;
public static UntypedInt SystemPlatformBinaryInformation => 133;
public static UntypedInt SystemThrottleNotificationInformation => 134;
public static UntypedInt SystemHypervisorProcessorCountInformation => 135;
public static UntypedInt SystemDeviceDataInformation => 136;
public static UntypedInt SystemDeviceDataEnumerationInformation => 137;
public static UntypedInt SystemMemoryTopologyInformation => 138;
public static UntypedInt SystemMemoryChannelInformation => 139;
public static UntypedInt SystemBootLogoInformation => 140;
public static UntypedInt SystemProcessorPerformanceInformationEx => 141;
internal static UntypedInt systemSpare0 => 142;
public static UntypedInt SystemSecureBootPolicyInformation => 143;
public static UntypedInt SystemPageFileInformationEx => 144;
public static UntypedInt SystemSecureBootInformation => 145;
public static UntypedInt SystemEntropyInterruptTimingRawInformation => 146;
public static UntypedInt SystemPortableWorkspaceEfiLauncherInformation => 147;
public static UntypedInt SystemFullProcessInformation => 148;
public static UntypedInt SystemKernelDebuggerInformationEx => 149;
public static UntypedInt SystemBootMetadataInformation => 150;
public static UntypedInt SystemSoftRebootInformation => 151;
public static UntypedInt SystemElamCertificateInformation => 152;
public static UntypedInt SystemOfflineDumpConfigInformation => 153;
public static UntypedInt SystemProcessorFeaturesInformation => 154;
public static UntypedInt SystemRegistryReconciliationInformation => 155;
public static UntypedInt SystemEdidInformation => 156;
public static UntypedInt SystemManufacturingInformation => 157;
public static UntypedInt SystemEnergyEstimationConfigInformation => 158;
public static UntypedInt SystemHypervisorDetailInformation => 159;
public static UntypedInt SystemProcessorCycleStatsInformation => 160;
public static UntypedInt SystemVmGenerationCountInformation => 161;
public static UntypedInt SystemTrustedPlatformModuleInformation => 162;
public static UntypedInt SystemKernelDebuggerFlags => 163;
public static UntypedInt SystemCodeIntegrityPolicyInformation => 164;
public static UntypedInt SystemIsolatedUserModeInformation => 165;
public static UntypedInt SystemHardwareSecurityTestInterfaceResultsInformation => 166;
public static UntypedInt SystemSingleModuleInformation => 167;
public static UntypedInt SystemAllowedCpuSetsInformation => 168;
public static UntypedInt SystemDmaProtectionInformation => 169;
public static UntypedInt SystemInterruptCpuSetsInformation => 170;
public static UntypedInt SystemSecureBootPolicyFullInformation => 171;
public static UntypedInt SystemCodeIntegrityPolicyFullInformation => 172;
public static UntypedInt SystemAffinitizedInterruptProcessorInformation => 173;
public static UntypedInt SystemRootSiloInformation => 174;

partial struct RTL_PROCESS_MODULE_INFORMATION {
    public Handle Section;
    public uintptr MappedBase;
    public uintptr ImageBase;
    public uint32 ImageSize;
    public uint32 Flags;
    public uint16 LoadOrderIndex;
    public uint16 InitOrderIndex;
    public uint16 LoadCount;
    public uint16 OffsetToFileName;
    public array<byte> FullPathName = new(256);
}

partial struct RTL_PROCESS_MODULES {
    public uint32 NumberOfModules;
    public array<RTL_PROCESS_MODULE_INFORMATION> Modules = new(1, () => new());
}

public static UntypedInt LMEM_FIXED => 0x0;
public static UntypedInt LMEM_MOVEABLE => 0x2;
public static UntypedInt LMEM_NOCOMPACT => 0x10;
public static UntypedInt LMEM_NODISCARD => 0x20;
public static UntypedInt LMEM_ZEROINIT => 0x40;
public static UntypedInt LMEM_MODIFY => 0x80;
public static UntypedInt LMEM_DISCARDABLE => 0xf00;
public static UntypedInt LMEM_VALID_FLAGS => 0xf72;
public static UntypedInt LMEM_INVALID_HANDLE => 0x8000;
public static UntypedInt LHND => /* LMEM_MOVEABLE | LMEM_ZEROINIT */ 66;
public static UntypedInt LPTR => /* LMEM_FIXED | LMEM_ZEROINIT */ 64;
public static UntypedInt NONZEROLHND => /* LMEM_MOVEABLE */ 2;
public static UntypedInt NONZEROLPTR => /* LMEM_FIXED */ 0;

public static UntypedInt PIPE_ACCESS_INBOUND => 0x1;
public static UntypedInt PIPE_ACCESS_OUTBOUND => 0x2;
public static UntypedInt PIPE_ACCESS_DUPLEX => 0x3;
public static UntypedInt PIPE_CLIENT_END => 0x0;
public static UntypedInt PIPE_SERVER_END => 0x1;
public static UntypedInt PIPE_WAIT => 0x0;
public static UntypedInt PIPE_NOWAIT => 0x1;
public static UntypedInt PIPE_READMODE_BYTE => 0x0;
public static UntypedInt PIPE_READMODE_MESSAGE => 0x2;
public static UntypedInt PIPE_TYPE_BYTE => 0x0;
public static UntypedInt PIPE_TYPE_MESSAGE => 0x4;
public static UntypedInt PIPE_ACCEPT_REMOTE_CLIENTS => 0x0;
public static UntypedInt PIPE_REJECT_REMOTE_CLIENTS => 0x8;
public static UntypedInt PIPE_UNLIMITED_INSTANCES => 255;

public static UntypedInt SECURITY_ANONYMOUS => /* SecurityAnonymous << 16 */ 0;
public static UntypedInt SECURITY_IDENTIFICATION => /* SecurityIdentification << 16 */ 65536;
public static UntypedInt SECURITY_IMPERSONATION => /* SecurityImpersonation << 16 */ 131072;
public static UntypedInt SECURITY_DELEGATION => /* SecurityDelegation << 16 */ 196608;
public static UntypedInt SECURITY_CONTEXT_TRACKING => 0x40000;
public static UntypedInt SECURITY_EFFECTIVE_ONLY => 0x80000;
public static UntypedInt SECURITY_SQOS_PRESENT => 0x100000;
public static UntypedInt SECURITY_VALID_SQOS_FLAGS => 0x1f0000;

partial struct ResourceID /*num:uint16*/;

public static readonly ж<ResourceID> ᏑCREATEPROCESS_MANIFEST_RESOURCE_ID = new StaticBox<ResourceID>(1);
public static ref ResourceID CREATEPROCESS_MANIFEST_RESOURCE_ID => ref ((StaticBox<ResourceID>)ᏑCREATEPROCESS_MANIFEST_RESOURCE_ID).ValueSlot;
public static readonly ж<ResourceID> ᏑISOLATIONAWARE_MANIFEST_RESOURCE_ID = new StaticBox<ResourceID>(2);
public static ref ResourceID ISOLATIONAWARE_MANIFEST_RESOURCE_ID => ref ((StaticBox<ResourceID>)ᏑISOLATIONAWARE_MANIFEST_RESOURCE_ID).ValueSlot;
public static readonly ж<ResourceID> ᏑISOLATIONAWARE_NOSTATICIMPORT_MANIFEST_RESOURCE_ID = new StaticBox<ResourceID>(3);
public static ref ResourceID ISOLATIONAWARE_NOSTATICIMPORT_MANIFEST_RESOURCE_ID => ref ((StaticBox<ResourceID>)ᏑISOLATIONAWARE_NOSTATICIMPORT_MANIFEST_RESOURCE_ID).ValueSlot;
public static readonly ж<ResourceID> ᏑISOLATIONPOLICY_MANIFEST_RESOURCE_ID = new StaticBox<ResourceID>(4);
public static ref ResourceID ISOLATIONPOLICY_MANIFEST_RESOURCE_ID => ref ((StaticBox<ResourceID>)ᏑISOLATIONPOLICY_MANIFEST_RESOURCE_ID).ValueSlot;
public static readonly ж<ResourceID> ᏑISOLATIONPOLICY_BROWSER_MANIFEST_RESOURCE_ID = new StaticBox<ResourceID>(5);
public static ref ResourceID ISOLATIONPOLICY_BROWSER_MANIFEST_RESOURCE_ID => ref ((StaticBox<ResourceID>)ᏑISOLATIONPOLICY_BROWSER_MANIFEST_RESOURCE_ID).ValueSlot;
public static readonly ж<ResourceID> ᏑMINIMUM_RESERVED_MANIFEST_RESOURCE_ID = new StaticBox<ResourceID>(1);
public static ref ResourceID MINIMUM_RESERVED_MANIFEST_RESOURCE_ID => ref ((StaticBox<ResourceID>)ᏑMINIMUM_RESERVED_MANIFEST_RESOURCE_ID).ValueSlot;
public static readonly ж<ResourceID> ᏑMAXIMUM_RESERVED_MANIFEST_RESOURCE_ID = new StaticBox<ResourceID>(16);
public static ref ResourceID MAXIMUM_RESERVED_MANIFEST_RESOURCE_ID => ref ((StaticBox<ResourceID>)ᏑMAXIMUM_RESERVED_MANIFEST_RESOURCE_ID).ValueSlot;
public static readonly ж<ResourceID> ᏑRT_CURSOR = new StaticBox<ResourceID>(1);
public static ref ResourceID RT_CURSOR => ref ((StaticBox<ResourceID>)ᏑRT_CURSOR).ValueSlot;
public static readonly ж<ResourceID> ᏑRT_BITMAP = new StaticBox<ResourceID>(2);
public static ref ResourceID RT_BITMAP => ref ((StaticBox<ResourceID>)ᏑRT_BITMAP).ValueSlot;
public static readonly ж<ResourceID> ᏑRT_ICON = new StaticBox<ResourceID>(3);
public static ref ResourceID RT_ICON => ref ((StaticBox<ResourceID>)ᏑRT_ICON).ValueSlot;
public static readonly ж<ResourceID> ᏑRT_MENU = new StaticBox<ResourceID>(4);
public static ref ResourceID RT_MENU => ref ((StaticBox<ResourceID>)ᏑRT_MENU).ValueSlot;
public static readonly ж<ResourceID> ᏑRT_DIALOG = new StaticBox<ResourceID>(5);
public static ref ResourceID RT_DIALOG => ref ((StaticBox<ResourceID>)ᏑRT_DIALOG).ValueSlot;
public static readonly ж<ResourceID> ᏑRT_STRING = new StaticBox<ResourceID>(6);
public static ref ResourceID RT_STRING => ref ((StaticBox<ResourceID>)ᏑRT_STRING).ValueSlot;
public static readonly ж<ResourceID> ᏑRT_FONTDIR = new StaticBox<ResourceID>(7);
public static ref ResourceID RT_FONTDIR => ref ((StaticBox<ResourceID>)ᏑRT_FONTDIR).ValueSlot;
public static readonly ж<ResourceID> ᏑRT_FONT = new StaticBox<ResourceID>(8);
public static ref ResourceID RT_FONT => ref ((StaticBox<ResourceID>)ᏑRT_FONT).ValueSlot;
public static readonly ж<ResourceID> ᏑRT_ACCELERATOR = new StaticBox<ResourceID>(9);
public static ref ResourceID RT_ACCELERATOR => ref ((StaticBox<ResourceID>)ᏑRT_ACCELERATOR).ValueSlot;
public static readonly ж<ResourceID> ᏑRT_RCDATA = new StaticBox<ResourceID>(10);
public static ref ResourceID RT_RCDATA => ref ((StaticBox<ResourceID>)ᏑRT_RCDATA).ValueSlot;
public static readonly ж<ResourceID> ᏑRT_MESSAGETABLE = new StaticBox<ResourceID>(11);
public static ref ResourceID RT_MESSAGETABLE => ref ((StaticBox<ResourceID>)ᏑRT_MESSAGETABLE).ValueSlot;
public static readonly ж<ResourceID> ᏑRT_GROUP_CURSOR = new StaticBox<ResourceID>(12);
public static ref ResourceID RT_GROUP_CURSOR => ref ((StaticBox<ResourceID>)ᏑRT_GROUP_CURSOR).ValueSlot;
public static readonly ж<ResourceID> ᏑRT_GROUP_ICON = new StaticBox<ResourceID>(14);
public static ref ResourceID RT_GROUP_ICON => ref ((StaticBox<ResourceID>)ᏑRT_GROUP_ICON).ValueSlot;
public static readonly ж<ResourceID> ᏑRT_VERSION = new StaticBox<ResourceID>(16);
public static ref ResourceID RT_VERSION => ref ((StaticBox<ResourceID>)ᏑRT_VERSION).ValueSlot;
public static readonly ж<ResourceID> ᏑRT_DLGINCLUDE = new StaticBox<ResourceID>(17);
public static ref ResourceID RT_DLGINCLUDE => ref ((StaticBox<ResourceID>)ᏑRT_DLGINCLUDE).ValueSlot;
public static readonly ж<ResourceID> ᏑRT_PLUGPLAY = new StaticBox<ResourceID>(19);
public static ref ResourceID RT_PLUGPLAY => ref ((StaticBox<ResourceID>)ᏑRT_PLUGPLAY).ValueSlot;
public static readonly ж<ResourceID> ᏑRT_VXD = new StaticBox<ResourceID>(20);
public static ref ResourceID RT_VXD => ref ((StaticBox<ResourceID>)ᏑRT_VXD).ValueSlot;
public static readonly ж<ResourceID> ᏑRT_ANICURSOR = new StaticBox<ResourceID>(21);
public static ref ResourceID RT_ANICURSOR => ref ((StaticBox<ResourceID>)ᏑRT_ANICURSOR).ValueSlot;
public static readonly ж<ResourceID> ᏑRT_ANIICON = new StaticBox<ResourceID>(22);
public static ref ResourceID RT_ANIICON => ref ((StaticBox<ResourceID>)ᏑRT_ANIICON).ValueSlot;
public static readonly ж<ResourceID> ᏑRT_HTML = new StaticBox<ResourceID>(23);
public static ref ResourceID RT_HTML => ref ((StaticBox<ResourceID>)ᏑRT_HTML).ValueSlot;
public static readonly ж<ResourceID> ᏑRT_MANIFEST = new StaticBox<ResourceID>(24);
public static ref ResourceID RT_MANIFEST => ref ((StaticBox<ResourceID>)ᏑRT_MANIFEST).ValueSlot;

partial struct VS_FIXEDFILEINFO {
    public uint32 Signature;
    public uint32 StrucVersion;
    public uint32 FileVersionMS;
    public uint32 FileVersionLS;
    public uint32 ProductVersionMS;
    public uint32 ProductVersionLS;
    public uint32 FileFlagsMask;
    public uint32 FileFlags;
    public uint32 FileOS;
    public uint32 FileType;
    public uint32 FileSubtype;
    public uint32 FileDateMS;
    public uint32 FileDateLS;
}

partial struct COAUTHIDENTITY {
    public ж<uint16> User;
    public uint32 UserLength;
    public ж<uint16> Domain;
    public uint32 DomainLength;
    public ж<uint16> Password;
    public uint32 PasswordLength;
    public uint32 Flags;
}

partial struct COAUTHINFO {
    public uint32 AuthnSvc;
    public uint32 AuthzSvc;
    public ж<uint16> ServerPrincName;
    public uint32 AuthnLevel;
    public uint32 ImpersonationLevel;
    public ж<COAUTHIDENTITY> AuthIdentityData;
    public uint32 Capabilities;
}

partial struct COSERVERINFO {
    public uint32 Reserved1;
    public ж<uint16> Aame;
    public ж<COAUTHINFO> AuthInfo;
    public uint32 Reserved2;
}

partial struct BIND_OPTS3 {
    public uint32 CbStruct;
    public uint32 Flags;
    public uint32 Mode;
    public uint32 TickCountDeadline;
    public uint32 TrackFlags;
    public uint32 ClassContext;
    public uint32 Locale;
    public ж<COSERVERINFO> ServerInfo;
    public HWND Hwnd;
}

public static UntypedInt CLSCTX_INPROC_SERVER => 0x1;
public static UntypedInt CLSCTX_INPROC_HANDLER => 0x2;
public static UntypedInt CLSCTX_LOCAL_SERVER => 0x4;
public static UntypedInt CLSCTX_INPROC_SERVER16 => 0x8;
public static UntypedInt CLSCTX_REMOTE_SERVER => 0x10;
public static UntypedInt CLSCTX_INPROC_HANDLER16 => 0x20;
public static UntypedInt CLSCTX_RESERVED1 => 0x40;
public static UntypedInt CLSCTX_RESERVED2 => 0x80;
public static UntypedInt CLSCTX_RESERVED3 => 0x100;
public static UntypedInt CLSCTX_RESERVED4 => 0x200;
public static UntypedInt CLSCTX_NO_CODE_DOWNLOAD => 0x400;
public static UntypedInt CLSCTX_RESERVED5 => 0x800;
public static UntypedInt CLSCTX_NO_CUSTOM_MARSHAL => 0x1000;
public static UntypedInt CLSCTX_ENABLE_CODE_DOWNLOAD => 0x2000;
public static UntypedInt CLSCTX_NO_FAILURE_LOG => 0x4000;
public static UntypedInt CLSCTX_DISABLE_AAA => 0x8000;
public static UntypedInt CLSCTX_ENABLE_AAA => 0x10000;
public static UntypedInt CLSCTX_FROM_DEFAULT_CONTEXT => 0x20000;
public static UntypedInt CLSCTX_ACTIVATE_32_BIT_SERVER => 0x40000;
public static UntypedInt CLSCTX_ACTIVATE_64_BIT_SERVER => 0x80000;
public static UntypedInt CLSCTX_ENABLE_CLOAKING => 0x100000;
public static UntypedInt CLSCTX_APPCONTAINER => 0x400000;
public static UntypedInt CLSCTX_ACTIVATE_AAA_AS_IU => 0x800000;
public static UntypedInt CLSCTX_PS_DLL => 0x80000000;
public static UntypedInt COINIT_MULTITHREADED => 0x0;
public static UntypedInt COINIT_APARTMENTTHREADED => 0x2;
public static UntypedInt COINIT_DISABLE_OLE1DDE => 0x4;
public static UntypedInt COINIT_SPEED_OVER_MEMORY => 0x8;

public static UntypedInt PROCESS_NAME_NATIVE => 1;

partial struct ModuleInfo {
    public uintptr BaseOfDll;
    public uint32 SizeOfImage;
    public uintptr EntryPoint;
}

public static UntypedInt ALL_PROCESSOR_GROUPS => 0xFFFF;

partial struct Rect {
    public int32 Left;
    public int32 Top;
    public int32 Right;
    public int32 Bottom;
}

partial struct GUIThreadInfo {
    public uint32 Size;
    public uint32 Flags;
    public HWND Active;
    public HWND Focus;
    public HWND Capture;
    public HWND MenuOwner;
    public HWND MoveSize;
    public HWND CaretHandle;
    public Rect CaretRect;
}

public static UntypedInt DWMWA_NCRENDERING_ENABLED => 1;
public static UntypedInt DWMWA_NCRENDERING_POLICY => 2;
public static UntypedInt DWMWA_TRANSITIONS_FORCEDISABLED => 3;
public static UntypedInt DWMWA_ALLOW_NCPAINT => 4;
public static UntypedInt DWMWA_CAPTION_BUTTON_BOUNDS => 5;
public static UntypedInt DWMWA_NONCLIENT_RTL_LAYOUT => 6;
public static UntypedInt DWMWA_FORCE_ICONIC_REPRESENTATION => 7;
public static UntypedInt DWMWA_FLIP3D_POLICY => 8;
public static UntypedInt DWMWA_EXTENDED_FRAME_BOUNDS => 9;
public static UntypedInt DWMWA_HAS_ICONIC_BITMAP => 10;
public static UntypedInt DWMWA_DISALLOW_PEEK => 11;
public static UntypedInt DWMWA_EXCLUDED_FROM_PEEK => 12;
public static UntypedInt DWMWA_CLOAK => 13;
public static UntypedInt DWMWA_CLOAKED => 14;
public static UntypedInt DWMWA_FREEZE_REPRESENTATION => 15;
public static UntypedInt DWMWA_PASSIVE_UPDATE_MODE => 16;
public static UntypedInt DWMWA_USE_HOSTBACKDROPBRUSH => 17;
public static UntypedInt DWMWA_USE_IMMERSIVE_DARK_MODE => 20;
public static UntypedInt DWMWA_WINDOW_CORNER_PREFERENCE => 33;
public static UntypedInt DWMWA_BORDER_COLOR => 34;
public static UntypedInt DWMWA_CAPTION_COLOR => 35;
public static UntypedInt DWMWA_TEXT_COLOR => 36;
public static UntypedInt DWMWA_VISIBLE_FRAME_BORDER_THICKNESS => 37;

partial struct WSAQUERYSET {
    public uint32 Size;
    public ж<uint16> ServiceInstanceName;
    public ж<GUID> ServiceClassId;
    public ж<WSAVersion> Version;
    public ж<uint16> Comment;
    public uint32 NameSpace;
    public ж<GUID> NSProviderId;
    public ж<uint16> Context;
    public uint32 NumberOfProtocols;
    public ж<AFProtocols> AfpProtocols;
    public ж<uint16> QueryString;
    public uint32 NumberOfCsAddrs;
    public ж<CSAddrInfo> SaBuffer;
    public uint32 OutputFlags;
    public ж<BLOB> Blob;
}

partial struct WSAVersion {
    public uint32 Version;
    public int32 EnumerationOfComparison;
}

partial struct AFProtocols {
    public int32 AddressFamily;
    public int32 Protocol;
}

partial struct CSAddrInfo {
    public SocketAddress LocalAddr;
    public SocketAddress RemoteAddr;
    public int32 SocketType;
    public int32 Protocol;
}

partial struct BLOB {
    public uint32 Size;
    public ж<byte> BlobData;
}

partial struct ComStat {
    public uint32 Flags;
    public uint32 CBInQue;
    public uint32 CBOutQue;
}

partial struct DCB {
    public uint32 DCBlength;
    public uint32 BaudRate;
    public uint32 Flags;
    internal uint16 wReserved;
    public uint16 XonLim;
    public uint16 XoffLim;
    public uint8 ByteSize;
    public uint8 Parity;
    public uint8 StopBits;
    public byte XonChar;
    public byte XoffChar;
    public byte ErrorChar;
    public byte EofChar;
    public byte EvtChar;
    internal uint16 wReserved1;
}

public static UntypedInt KLF_ACTIVATE => 0x00000001;
public static UntypedInt KLF_SUBSTITUTE_OK => 0x00000002;
public static UntypedInt KLF_REORDER => 0x00000008;
public static UntypedInt KLF_REPLACELANG => 0x00000010;
public static UntypedInt KLF_NOTELLSHELL => 0x00000080;
public static UntypedInt KLF_SETFORPROCESS => 0x00000100;

public static UntypedInt VK_LBUTTON => 0x01;
public static UntypedInt VK_RBUTTON => 0x02;
public static UntypedInt VK_CANCEL => 0x03;
public static UntypedInt VK_MBUTTON => 0x04;
public static UntypedInt VK_XBUTTON1 => 0x05;
public static UntypedInt VK_XBUTTON2 => 0x06;
public static UntypedInt VK_BACK => 0x08;
public static UntypedInt VK_TAB => 0x09;
public static UntypedInt VK_CLEAR => 0x0C;
public static UntypedInt VK_RETURN => 0x0D;
public static UntypedInt VK_SHIFT => 0x10;
public static UntypedInt VK_CONTROL => 0x11;
public static UntypedInt VK_MENU => 0x12;
public static UntypedInt VK_PAUSE => 0x13;
public static UntypedInt VK_CAPITAL => 0x14;
public static UntypedInt VK_KANA => 0x15;
public static UntypedInt VK_HANGEUL => 0x15;
public static UntypedInt VK_HANGUL => 0x15;
public static UntypedInt VK_IME_ON => 0x16;
public static UntypedInt VK_JUNJA => 0x17;
public static UntypedInt VK_FINAL => 0x18;
public static UntypedInt VK_HANJA => 0x19;
public static UntypedInt VK_KANJI => 0x19;
public static UntypedInt VK_IME_OFF => 0x1A;
public static UntypedInt VK_ESCAPE => 0x1B;
public static UntypedInt VK_CONVERT => 0x1C;
public static UntypedInt VK_NONCONVERT => 0x1D;
public static UntypedInt VK_ACCEPT => 0x1E;
public static UntypedInt VK_MODECHANGE => 0x1F;
public static UntypedInt VK_SPACE => 0x20;
public static UntypedInt VK_PRIOR => 0x21;
public static UntypedInt VK_NEXT => 0x22;
public static UntypedInt VK_END => 0x23;
public static UntypedInt VK_HOME => 0x24;
public static UntypedInt VK_LEFT => 0x25;
public static UntypedInt VK_UP => 0x26;
public static UntypedInt VK_RIGHT => 0x27;
public static UntypedInt VK_DOWN => 0x28;
public static UntypedInt VK_SELECT => 0x29;
public static UntypedInt VK_PRINT => 0x2A;
public static UntypedInt VK_EXECUTE => 0x2B;
public static UntypedInt VK_SNAPSHOT => 0x2C;
public static UntypedInt VK_INSERT => 0x2D;
public static UntypedInt VK_DELETE => 0x2E;
public static UntypedInt VK_HELP => 0x2F;
public static UntypedInt VK_LWIN => 0x5B;
public static UntypedInt VK_RWIN => 0x5C;
public static UntypedInt VK_APPS => 0x5D;
public static UntypedInt VK_SLEEP => 0x5F;
public static UntypedInt VK_NUMPAD0 => 0x60;
public static UntypedInt VK_NUMPAD1 => 0x61;
public static UntypedInt VK_NUMPAD2 => 0x62;
public static UntypedInt VK_NUMPAD3 => 0x63;
public static UntypedInt VK_NUMPAD4 => 0x64;
public static UntypedInt VK_NUMPAD5 => 0x65;
public static UntypedInt VK_NUMPAD6 => 0x66;
public static UntypedInt VK_NUMPAD7 => 0x67;
public static UntypedInt VK_NUMPAD8 => 0x68;
public static UntypedInt VK_NUMPAD9 => 0x69;
public static UntypedInt VK_MULTIPLY => 0x6A;
public static UntypedInt VK_ADD => 0x6B;
public static UntypedInt VK_SEPARATOR => 0x6C;
public static UntypedInt VK_SUBTRACT => 0x6D;
public static UntypedInt VK_DECIMAL => 0x6E;
public static UntypedInt VK_DIVIDE => 0x6F;
public static UntypedInt VK_F1 => 0x70;
public static UntypedInt VK_F2 => 0x71;
public static UntypedInt VK_F3 => 0x72;
public static UntypedInt VK_F4 => 0x73;
public static UntypedInt VK_F5 => 0x74;
public static UntypedInt VK_F6 => 0x75;
public static UntypedInt VK_F7 => 0x76;
public static UntypedInt VK_F8 => 0x77;
public static UntypedInt VK_F9 => 0x78;
public static UntypedInt VK_F10 => 0x79;
public static UntypedInt VK_F11 => 0x7A;
public static UntypedInt VK_F12 => 0x7B;
public static UntypedInt VK_F13 => 0x7C;
public static UntypedInt VK_F14 => 0x7D;
public static UntypedInt VK_F15 => 0x7E;
public static UntypedInt VK_F16 => 0x7F;
public static UntypedInt VK_F17 => 0x80;
public static UntypedInt VK_F18 => 0x81;
public static UntypedInt VK_F19 => 0x82;
public static UntypedInt VK_F20 => 0x83;
public static UntypedInt VK_F21 => 0x84;
public static UntypedInt VK_F22 => 0x85;
public static UntypedInt VK_F23 => 0x86;
public static UntypedInt VK_F24 => 0x87;
public static UntypedInt VK_NUMLOCK => 0x90;
public static UntypedInt VK_SCROLL => 0x91;
public static UntypedInt VK_OEM_NEC_EQUAL => 0x92;
public static UntypedInt VK_OEM_FJ_JISHO => 0x92;
public static UntypedInt VK_OEM_FJ_MASSHOU => 0x93;
public static UntypedInt VK_OEM_FJ_TOUROKU => 0x94;
public static UntypedInt VK_OEM_FJ_LOYA => 0x95;
public static UntypedInt VK_OEM_FJ_ROYA => 0x96;
public static UntypedInt VK_LSHIFT => 0xA0;
public static UntypedInt VK_RSHIFT => 0xA1;
public static UntypedInt VK_LCONTROL => 0xA2;
public static UntypedInt VK_RCONTROL => 0xA3;
public static UntypedInt VK_LMENU => 0xA4;
public static UntypedInt VK_RMENU => 0xA5;
public static UntypedInt VK_BROWSER_BACK => 0xA6;
public static UntypedInt VK_BROWSER_FORWARD => 0xA7;
public static UntypedInt VK_BROWSER_REFRESH => 0xA8;
public static UntypedInt VK_BROWSER_STOP => 0xA9;
public static UntypedInt VK_BROWSER_SEARCH => 0xAA;
public static UntypedInt VK_BROWSER_FAVORITES => 0xAB;
public static UntypedInt VK_BROWSER_HOME => 0xAC;
public static UntypedInt VK_VOLUME_MUTE => 0xAD;
public static UntypedInt VK_VOLUME_DOWN => 0xAE;
public static UntypedInt VK_VOLUME_UP => 0xAF;
public static UntypedInt VK_MEDIA_NEXT_TRACK => 0xB0;
public static UntypedInt VK_MEDIA_PREV_TRACK => 0xB1;
public static UntypedInt VK_MEDIA_STOP => 0xB2;
public static UntypedInt VK_MEDIA_PLAY_PAUSE => 0xB3;
public static UntypedInt VK_LAUNCH_MAIL => 0xB4;
public static UntypedInt VK_LAUNCH_MEDIA_SELECT => 0xB5;
public static UntypedInt VK_LAUNCH_APP1 => 0xB6;
public static UntypedInt VK_LAUNCH_APP2 => 0xB7;
public static UntypedInt VK_OEM_1 => 0xBA;
public static UntypedInt VK_OEM_PLUS => 0xBB;
public static UntypedInt VK_OEM_COMMA => 0xBC;
public static UntypedInt VK_OEM_MINUS => 0xBD;
public static UntypedInt VK_OEM_PERIOD => 0xBE;
public static UntypedInt VK_OEM_2 => 0xBF;
public static UntypedInt VK_OEM_3 => 0xC0;
public static UntypedInt VK_OEM_4 => 0xDB;
public static UntypedInt VK_OEM_5 => 0xDC;
public static UntypedInt VK_OEM_6 => 0xDD;
public static UntypedInt VK_OEM_7 => 0xDE;
public static UntypedInt VK_OEM_8 => 0xDF;
public static UntypedInt VK_OEM_AX => 0xE1;
public static UntypedInt VK_OEM_102 => 0xE2;
public static UntypedInt VK_ICO_HELP => 0xE3;
public static UntypedInt VK_ICO_00 => 0xE4;
public static UntypedInt VK_PROCESSKEY => 0xE5;
public static UntypedInt VK_ICO_CLEAR => 0xE6;
public static UntypedInt VK_OEM_RESET => 0xE9;
public static UntypedInt VK_OEM_JUMP => 0xEA;
public static UntypedInt VK_OEM_PA1 => 0xEB;
public static UntypedInt VK_OEM_PA2 => 0xEC;
public static UntypedInt VK_OEM_PA3 => 0xED;
public static UntypedInt VK_OEM_WSCTRL => 0xEE;
public static UntypedInt VK_OEM_CUSEL => 0xEF;
public static UntypedInt VK_OEM_ATTN => 0xF0;
public static UntypedInt VK_OEM_FINISH => 0xF1;
public static UntypedInt VK_OEM_COPY => 0xF2;
public static UntypedInt VK_OEM_AUTO => 0xF3;
public static UntypedInt VK_OEM_ENLW => 0xF4;
public static UntypedInt VK_OEM_BACKTAB => 0xF5;
public static UntypedInt VK_ATTN => 0xF6;
public static UntypedInt VK_CRSEL => 0xF7;
public static UntypedInt VK_EXSEL => 0xF8;
public static UntypedInt VK_EREOF => 0xF9;
public static UntypedInt VK_PLAY => 0xFA;
public static UntypedInt VK_ZOOM => 0xFB;
public static UntypedInt VK_NONAME => 0xFC;
public static UntypedInt VK_PA1 => 0xFD;
public static UntypedInt VK_OEM_CLEAR => 0xFE;

public static UntypedInt FROM_LEFT_1ST_BUTTON_PRESSED => 0x0001;
public static UntypedInt RIGHTMOST_BUTTON_PRESSED => 0x0002;
public static UntypedInt FROM_LEFT_2ND_BUTTON_PRESSED => 0x0004;
public static UntypedInt FROM_LEFT_3RD_BUTTON_PRESSED => 0x0008;
public static UntypedInt FROM_LEFT_4TH_BUTTON_PRESSED => 0x0010;

public static UntypedInt CAPSLOCK_ON => 0x0080;
public static UntypedInt ENHANCED_KEY => 0x0100;
public static UntypedInt LEFT_ALT_PRESSED => 0x0002;
public static UntypedInt LEFT_CTRL_PRESSED => 0x0008;
public static UntypedInt NUMLOCK_ON => 0x0020;
public static UntypedInt RIGHT_ALT_PRESSED => 0x0001;
public static UntypedInt RIGHT_CTRL_PRESSED => 0x0004;
public static UntypedInt SCROLLLOCK_ON => 0x0040;
public static UntypedInt SHIFT_PRESSED => 0x0010;

public static UntypedInt MOUSE_MOVED => 0x0001;
public static UntypedInt DOUBLE_CLICK => 0x0002;
public static UntypedInt MOUSE_WHEELED => 0x0004;
public static UntypedInt MOUSE_HWHEELED => 0x0008;

public static UntypedInt FOCUS_EVENT => 0x0010;
public static UntypedInt KEY_EVENT => 0x0001;
public static UntypedInt MENU_EVENT => 0x0008;
public static UntypedInt MOUSE_EVENT => 0x0002;
public static UntypedInt WINDOW_BUFFER_SIZE_EVENT => 0x0004;

public static UntypedInt PF_ARM_64BIT_LOADSTORE_ATOMIC => 25;
public static UntypedInt PF_ARM_DIVIDE_INSTRUCTION_AVAILABLE => 24;
public static UntypedInt PF_ARM_EXTERNAL_CACHE_AVAILABLE => 26;
public static UntypedInt PF_ARM_FMAC_INSTRUCTIONS_AVAILABLE => 27;
public static UntypedInt PF_ARM_VFP_32_REGISTERS_AVAILABLE => 18;
public static UntypedInt PF_3DNOW_INSTRUCTIONS_AVAILABLE => 7;
public static UntypedInt PF_CHANNELS_ENABLED => 16;
public static UntypedInt PF_COMPARE_EXCHANGE_DOUBLE => 2;
public static UntypedInt PF_COMPARE_EXCHANGE128 => 14;
public static UntypedInt PF_COMPARE64_EXCHANGE128 => 15;
public static UntypedInt PF_FASTFAIL_AVAILABLE => 23;
public static UntypedInt PF_FLOATING_POINT_EMULATED => 1;
public static UntypedInt PF_FLOATING_POINT_PRECISION_ERRATA => 0;
public static UntypedInt PF_MMX_INSTRUCTIONS_AVAILABLE => 3;
public static UntypedInt PF_NX_ENABLED => 12;
public static UntypedInt PF_PAE_ENABLED => 9;
public static UntypedInt PF_RDTSC_INSTRUCTION_AVAILABLE => 8;
public static UntypedInt PF_RDWRFSGSBASE_AVAILABLE => 22;
public static UntypedInt PF_SECOND_LEVEL_ADDRESS_TRANSLATION => 20;
public static UntypedInt PF_SSE3_INSTRUCTIONS_AVAILABLE => 13;
public static UntypedInt PF_SSSE3_INSTRUCTIONS_AVAILABLE => 36;
public static UntypedInt PF_SSE4_1_INSTRUCTIONS_AVAILABLE => 37;
public static UntypedInt PF_SSE4_2_INSTRUCTIONS_AVAILABLE => 38;
public static UntypedInt PF_AVX_INSTRUCTIONS_AVAILABLE => 39;
public static UntypedInt PF_AVX2_INSTRUCTIONS_AVAILABLE => 40;
public static UntypedInt PF_AVX512F_INSTRUCTIONS_AVAILABLE => 41;
public static UntypedInt PF_VIRT_FIRMWARE_ENABLED => 21;
public static UntypedInt PF_XMMI_INSTRUCTIONS_AVAILABLE => 6;
public static UntypedInt PF_XMMI64_INSTRUCTIONS_AVAILABLE => 10;
public static UntypedInt PF_XSAVE_ENABLED => 17;
public static UntypedInt PF_ARM_V8_INSTRUCTIONS_AVAILABLE => 29;
public static UntypedInt PF_ARM_V8_CRYPTO_INSTRUCTIONS_AVAILABLE => 30;
public static UntypedInt PF_ARM_V8_CRC32_INSTRUCTIONS_AVAILABLE => 31;
public static UntypedInt PF_ARM_V81_ATOMIC_INSTRUCTIONS_AVAILABLE => 34;
public static UntypedInt PF_ARM_V82_DP_INSTRUCTIONS_AVAILABLE => 43;
public static UntypedInt PF_ARM_V83_JSCVT_INSTRUCTIONS_AVAILABLE => 44;
public static UntypedInt PF_ARM_V83_LRCPC_INSTRUCTIONS_AVAILABLE => 45;
public static UntypedInt PF_ARM_SVE_INSTRUCTIONS_AVAILABLE => 46;
public static UntypedInt PF_ARM_SVE2_INSTRUCTIONS_AVAILABLE => 47;
public static UntypedInt PF_ARM_SVE2_1_INSTRUCTIONS_AVAILABLE => 48;
public static UntypedInt PF_ARM_SVE_AES_INSTRUCTIONS_AVAILABLE => 49;
public static UntypedInt PF_ARM_SVE_PMULL128_INSTRUCTIONS_AVAILABLE => 50;
public static UntypedInt PF_ARM_SVE_BITPERM_INSTRUCTIONS_AVAILABLE => 51;
public static UntypedInt PF_ARM_SVE_BF16_INSTRUCTIONS_AVAILABLE => 52;
public static UntypedInt PF_ARM_SVE_EBF16_INSTRUCTIONS_AVAILABLE => 53;
public static UntypedInt PF_ARM_SVE_B16B16_INSTRUCTIONS_AVAILABLE => 54;
public static UntypedInt PF_ARM_SVE_SHA3_INSTRUCTIONS_AVAILABLE => 55;
public static UntypedInt PF_ARM_SVE_SM4_INSTRUCTIONS_AVAILABLE => 56;
public static UntypedInt PF_ARM_SVE_I8MM_INSTRUCTIONS_AVAILABLE => 57;
public static UntypedInt PF_ARM_SVE_F32MM_INSTRUCTIONS_AVAILABLE => 58;
public static UntypedInt PF_ARM_SVE_F64MM_INSTRUCTIONS_AVAILABLE => 59;
public static UntypedInt PF_BMI2_INSTRUCTIONS_AVAILABLE => 60;
public static UntypedInt PF_MOVDIR64B_INSTRUCTION_AVAILABLE => 61;
public static UntypedInt PF_ARM_LSE2_AVAILABLE => 62;
public static UntypedInt PF_ARM_SHA3_INSTRUCTIONS_AVAILABLE => 64;
public static UntypedInt PF_ARM_SHA512_INSTRUCTIONS_AVAILABLE => 65;
public static UntypedInt PF_ARM_V82_I8MM_INSTRUCTIONS_AVAILABLE => 66;
public static UntypedInt PF_ARM_V82_FP16_INSTRUCTIONS_AVAILABLE => 67;
public static UntypedInt PF_ARM_V86_BF16_INSTRUCTIONS_AVAILABLE => 68;
public static UntypedInt PF_ARM_V86_EBF16_INSTRUCTIONS_AVAILABLE => 69;
public static UntypedInt PF_ARM_SME_INSTRUCTIONS_AVAILABLE => 70;
public static UntypedInt PF_ARM_SME2_INSTRUCTIONS_AVAILABLE => 71;
public static UntypedInt PF_ARM_SME2_1_INSTRUCTIONS_AVAILABLE => 72;
public static UntypedInt PF_ARM_SME2_2_INSTRUCTIONS_AVAILABLE => 73;
public static UntypedInt PF_ARM_SME_AES_INSTRUCTIONS_AVAILABLE => 74;
public static UntypedInt PF_ARM_SME_SBITPERM_INSTRUCTIONS_AVAILABLE => 75;
public static UntypedInt PF_ARM_SME_SF8MM4_INSTRUCTIONS_AVAILABLE => 76;
public static UntypedInt PF_ARM_SME_SF8MM8_INSTRUCTIONS_AVAILABLE => 77;
public static UntypedInt PF_ARM_SME_SF8DP2_INSTRUCTIONS_AVAILABLE => 78;
public static UntypedInt PF_ARM_SME_SF8DP4_INSTRUCTIONS_AVAILABLE => 79;
public static UntypedInt PF_ARM_SME_SF8FMA_INSTRUCTIONS_AVAILABLE => 80;
public static UntypedInt PF_ARM_SME_F8F32_INSTRUCTIONS_AVAILABLE => 81;
public static UntypedInt PF_ARM_SME_F8F16_INSTRUCTIONS_AVAILABLE => 82;
public static UntypedInt PF_ARM_SME_F16F16_INSTRUCTIONS_AVAILABLE => 83;
public static UntypedInt PF_ARM_SME_B16B16_INSTRUCTIONS_AVAILABLE => 84;
public static UntypedInt PF_ARM_SME_F64F64_INSTRUCTIONS_AVAILABLE => 85;
public static UntypedInt PF_ARM_SME_I16I64_INSTRUCTIONS_AVAILABLE => 86;
public static UntypedInt PF_ARM_SME_LUTv2_INSTRUCTIONS_AVAILABLE => 87;
public static UntypedInt PF_ARM_SME_FA64_INSTRUCTIONS_AVAILABLE => 88;
public static UntypedInt PF_UMONITOR_INSTRUCTION_AVAILABLE => 89;

} // end windows_package
