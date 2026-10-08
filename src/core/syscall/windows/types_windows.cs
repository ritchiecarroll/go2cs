// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class syscall_package {

public static Errno ERROR_FILE_NOT_FOUND => 2;
public static Errno ERROR_PATH_NOT_FOUND => 3;
public static Errno ERROR_ACCESS_DENIED => 5;
public static Errno ERROR_NO_MORE_FILES => 18;
public static Errno ERROR_HANDLE_EOF => 38;
public static Errno ERROR_NETNAME_DELETED => 64;
public static Errno ERROR_FILE_EXISTS => 80;
public static Errno ERROR_BROKEN_PIPE => 109;
public static Errno ERROR_BUFFER_OVERFLOW => 111;
public static Errno ERROR_INSUFFICIENT_BUFFER => 122;
public static Errno ERROR_MOD_NOT_FOUND => 126;
public static Errno ERROR_PROC_NOT_FOUND => 127;
public static Errno ERROR_DIR_NOT_EMPTY => 145;
public static Errno ERROR_ALREADY_EXISTS => 183;
public static Errno ERROR_ENVVAR_NOT_FOUND => 203;
public static Errno ERROR_MORE_DATA => 234;
public static Errno ERROR_OPERATION_ABORTED => 995;
public static Errno ERROR_IO_PENDING => 997;
public static Errno ERROR_NOT_FOUND => 1168;
public static Errno ERROR_PRIVILEGE_NOT_HELD => 1314;
public static Errno WSAEACCES => 10013;
public static Errno WSAENOPROTOOPT => 10042;
public static Errno WSAECONNABORTED => 10053;
public static Errno WSAECONNRESET => 10054;

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
internal static UntypedInt o_DIRECTORY => 0x100000; // used by internal/syscall/windows
internal static UntypedInt o_NOFOLLOW_ANY => 0x20000000; // used by internal/syscall/windows
internal static UntypedInt o_OPEN_REPARSE => 0x40000000; // used by internal/syscall/windows

public static ΔSignal SIGHUP => /* Signal(0x1) */ 1;
public static ΔSignal SIGINT => /* Signal(0x2) */ 2;
public static ΔSignal SIGQUIT => /* Signal(0x3) */ 3;
public static ΔSignal SIGILL => /* Signal(0x4) */ 4;
public static ΔSignal SIGTRAP => /* Signal(0x5) */ 5;
public static ΔSignal SIGABRT => /* Signal(0x6) */ 6;
public static ΔSignal SIGBUS => /* Signal(0x7) */ 7;
public static ΔSignal SIGFPE => /* Signal(0x8) */ 8;
public static ΔSignal SIGKILL => /* Signal(0x9) */ 9;
public static ΔSignal SIGSEGV => /* Signal(0xb) */ 11;
public static ΔSignal SIGPIPE => /* Signal(0xd) */ 13;
public static ΔSignal SIGALRM => /* Signal(0xe) */ 14;
public static ΔSignal SIGTERM => /* Signal(0xf) */ 15;

internal static array<@string> signals = new array<@string>(16){
    [1] = "hangup"u8,
    [2] = "interrupt"u8,
    [3] = "quit"u8,
    [4] = "illegal instruction"u8,
    [5] = "trace/breakpoint trap"u8,
    [6] = "aborted"u8,
    [7] = "bus error"u8,
    [8] = "floating point exception"u8,
    [9] = "killed"u8,
    [10] = "user defined signal 1"u8,
    [11] = "segmentation fault"u8,
    [12] = "user defined signal 2"u8,
    [13] = "broken pipe"u8,
    [14] = "alarm clock"u8,
    [15] = "terminated"u8
};

public static UntypedInt GENERIC_READ => 0x80000000;
public static UntypedInt GENERIC_WRITE => 0x40000000;
public static UntypedInt GENERIC_EXECUTE => 0x20000000;
public static UntypedInt GENERIC_ALL => 0x10000000;
public static UntypedInt FILE_LIST_DIRECTORY => 0x00000001;
public static UntypedInt FILE_APPEND_DATA => 0x00000004;
internal static UntypedInt _FILE_WRITE_EA => 0x00000010;
public static UntypedInt FILE_WRITE_ATTRIBUTES => 0x00000100;
public static UntypedInt FILE_SHARE_READ => 0x00000001;
public static UntypedInt FILE_SHARE_WRITE => 0x00000002;
public static UntypedInt FILE_SHARE_DELETE => 0x00000004;
public static UntypedInt FILE_ATTRIBUTE_READONLY => 0x00000001;
public static UntypedInt FILE_ATTRIBUTE_HIDDEN => 0x00000002;
public static UntypedInt FILE_ATTRIBUTE_SYSTEM => 0x00000004;
public static UntypedInt FILE_ATTRIBUTE_DIRECTORY => 0x00000010;
public static UntypedInt FILE_ATTRIBUTE_ARCHIVE => 0x00000020;
public static UntypedInt FILE_ATTRIBUTE_NORMAL => 0x00000080;
public static UntypedInt FILE_ATTRIBUTE_REPARSE_POINT => 0x00000400;
public static UntypedInt INVALID_FILE_ATTRIBUTES => 0xffffffff;
public static UntypedInt CREATE_NEW => 1;
public static UntypedInt CREATE_ALWAYS => 2;
public static UntypedInt OPEN_EXISTING => 3;
public static UntypedInt OPEN_ALWAYS => 4;
public static UntypedInt TRUNCATE_EXISTING => 5;
public static UntypedInt FILE_FLAG_OPEN_REPARSE_POINT => 0x00200000;
public static UntypedInt FILE_FLAG_BACKUP_SEMANTICS => 0x02000000;
public static UntypedInt FILE_FLAG_OVERLAPPED => 0x40000000;
public static UntypedInt HANDLE_FLAG_INHERIT => 0x00000001;
public static UntypedInt STARTF_USESTDHANDLES => 0x00000100;
public static UntypedInt STARTF_USESHOWWINDOW => 0x00000001;
public static UntypedInt DUPLICATE_CLOSE_SOURCE => 0x00000001;
public static UntypedInt DUPLICATE_SAME_ACCESS => 0x00000002;
public static UntypedInt STD_INPUT_HANDLE => -10;
public static UntypedInt STD_OUTPUT_HANDLE => -11;
public static UntypedInt STD_ERROR_HANDLE => -12;
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
public static UntypedInt MAX_PATH => 260;
public static UntypedInt MAX_LONG_PATH => 32768;
public static UntypedInt MAX_COMPUTERNAME_LENGTH => 15;
public static UntypedInt TIME_ZONE_ID_UNKNOWN => 0;
public static UntypedInt TIME_ZONE_ID_STANDARD => 1;
public static UntypedInt TIME_ZONE_ID_DAYLIGHT => 2;
public static UntypedInt IGNORE => 0;
public static UntypedInt INFINITE => 0xffffffff;
public static UntypedInt WAIT_TIMEOUT => 258;
public static UntypedInt WAIT_ABANDONED => 0x00000080;
public static UntypedInt WAIT_OBJECT_0 => 0x00000000;
public static UntypedInt WAIT_FAILED => 0xFFFFFFFF;
public static UntypedInt CREATE_NEW_PROCESS_GROUP => 0x00000200;
public static UntypedInt CREATE_UNICODE_ENVIRONMENT => 0x00000400;
public static UntypedInt PROCESS_TERMINATE => 1;
public static UntypedInt PROCESS_QUERY_INFORMATION => 0x00000400;
public static UntypedInt SYNCHRONIZE => 0x00100000;
public static UntypedInt PAGE_READONLY => 0x02;
public static UntypedInt PAGE_READWRITE => 0x04;
public static UntypedInt PAGE_WRITECOPY => 0x08;
public static UntypedInt PAGE_EXECUTE_READ => 0x20;
public static UntypedInt PAGE_EXECUTE_READWRITE => 0x40;
public static UntypedInt PAGE_EXECUTE_WRITECOPY => 0x80;
public static UntypedInt FILE_MAP_COPY => 0x01;
public static UntypedInt FILE_MAP_WRITE => 0x02;
public static UntypedInt FILE_MAP_READ => 0x04;
public static UntypedInt FILE_MAP_EXECUTE => 0x20;
public static UntypedInt CTRL_C_EVENT => 0;
public static UntypedInt CTRL_BREAK_EVENT => 1;
public static UntypedInt CTRL_CLOSE_EVENT => 2;
public static UntypedInt CTRL_LOGOFF_EVENT => 5;
public static UntypedInt CTRL_SHUTDOWN_EVENT => 6;

public static UntypedInt TH32CS_SNAPHEAPLIST => 0x01;
public static UntypedInt TH32CS_SNAPPROCESS => 0x02;
public static UntypedInt TH32CS_SNAPTHREAD => 0x04;
public static UntypedInt TH32CS_SNAPMODULE => 0x08;
public static UntypedInt TH32CS_SNAPMODULE32 => 0x10;
public static UntypedInt TH32CS_SNAPALL => /* TH32CS_SNAPHEAPLIST | TH32CS_SNAPMODULE | TH32CS_SNAPPROCESS | TH32CS_SNAPTHREAD */ 15;
public static UntypedInt TH32CS_INHERIT => 0x80000000;

public static UntypedInt FILE_NOTIFY_CHANGE_FILE_NAME => /* 1 << iota */ 1;
public static UntypedInt FILE_NOTIFY_CHANGE_DIR_NAME => 2;
public static UntypedInt FILE_NOTIFY_CHANGE_ATTRIBUTES => 4;
public static UntypedInt FILE_NOTIFY_CHANGE_SIZE => 8;
public static UntypedInt FILE_NOTIFY_CHANGE_LAST_WRITE => 16;
public static UntypedInt FILE_NOTIFY_CHANGE_LAST_ACCESS => 32;
public static UntypedInt FILE_NOTIFY_CHANGE_CREATION => 64;

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
public static UntypedInt USAGE_MATCH_TYPE_AND => 0;
public static UntypedInt USAGE_MATCH_TYPE_OR => 1;
public static UntypedInt X509_ASN_ENCODING => 0x00000001;
public static UntypedInt PKCS_7_ASN_ENCODING => 0x00010000;
public static UntypedInt CERT_STORE_PROV_MEMORY => 2;
public static UntypedInt CERT_STORE_ADD_ALWAYS => 4;
public static UntypedInt CERT_STORE_DEFER_CLOSE_UNTIL_LAST_FREE_FLAG => 0x00000004;
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
public static UntypedInt CERT_TRUST_IS_OFFLINE_REVOCATION => 0x01000000;
public static UntypedInt CERT_TRUST_NO_ISSUANCE_CHAIN_POLICY => 0x02000000;
public static UntypedInt CERT_TRUST_IS_EXPLICIT_DISTRUST => 0x04000000;
public static UntypedInt CERT_TRUST_HAS_NOT_SUPPORTED_CRITICAL_EXT => 0x08000000;
public static UntypedInt CERT_CHAIN_POLICY_BASE => 1;
public static UntypedInt CERT_CHAIN_POLICY_AUTHENTICODE => 2;
public static UntypedInt CERT_CHAIN_POLICY_AUTHENTICODE_TS => 3;
public static UntypedInt CERT_CHAIN_POLICY_SSL => 4;
public static UntypedInt CERT_CHAIN_POLICY_BASIC_CONSTRAINTS => 5;
public static UntypedInt CERT_CHAIN_POLICY_NT_AUTH => 6;
public static UntypedInt CERT_CHAIN_POLICY_MICROSOFT_ROOT => 7;
public static UntypedInt CERT_CHAIN_POLICY_EV => 8;
public static UntypedInt CERT_E_EXPIRED => 0x800B0101;
public static UntypedInt CERT_E_ROLE => 0x800B0103;
public static UntypedInt CERT_E_PURPOSE => 0x800B0106;
public static UntypedInt CERT_E_UNTRUSTEDROOT => 0x800B0109;
public static UntypedInt CERT_E_CN_NO_MATCH => 0x800B010F;
public static UntypedInt AUTHTYPE_CLIENT => 1;
public static UntypedInt AUTHTYPE_SERVER => 2;

public static slice<byte> OID_PKIX_KP_SERVER_AUTH = slice<byte>("1.3.6.1.5.5.7.3.1\x00"u8);
public static slice<byte> OID_SERVER_GATED_CRYPTO = slice<byte>("1.3.6.1.4.1.311.10.3.3\x00"u8);
public static slice<byte> OID_SGC_NETSCAPE = slice<byte>("2.16.840.1.113730.4.1\x00"u8);

partial class Pointer /*ж<EmptyStruct>*/;

// Invented values to support what package os expects.
partial struct Timeval {
    public int32 Sec;
    public int32 Usec;
}

public static int64 Nanoseconds(this ref Timeval tv) {
    return ((int64)tv.Sec * 1000000 + (int64)tv.Usec) * 1000;
}

public static Timeval /*tv*/ NsecToTimeval(int64 nsec) {
    Timeval tv = default!;

    tv.Sec = (int32)(nsec / 1000000000);
    tv.Usec = (int32)(nsec % 1000000000 / 1000);
    return tv;
}

partial struct SecurityAttributes {
    public uint32 Length;
    public uintptr SecurityDescriptor;
    public uint32 InheritHandle;
}

partial struct Overlapped {
    public uintptr Internal;
    public uintptr InternalHigh;
    public uint32 Offset;
    public uint32 OffsetHigh;
    public ΔHandle HEvent;
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

// Nanoseconds returns Filetime ft in nanoseconds
// since Epoch (00:00:00 UTC, January 1, 1970).
public static int64 Nanoseconds(this ref Filetime ft) {
    // 100-nanosecond intervals since January 1, 1601
    var nsec = ((int64)ft.HighDateTime << (int)(32)) + (int64)ft.LowDateTime;
    // change starting time to the Epoch (00:00:00 UTC, January 1, 1970)
    nsec -= 116444736000000000L;
    // convert into nanoseconds
    nsec *= 100;
    return nsec;
}

public static Filetime /*ft*/ NsecToFiletime(int64 nsec) {
    Filetime ft = default!;

    // convert into 100-nanosecond
    nsec /= 100;
    // change starting time to January 1, 1601
    nsec += 116444736000000000L;
    // split into high / low
    ft.LowDateTime = (uint32)((int64)(nsec & 0xffffffffL));
    ft.HighDateTime = (uint32)((int64)((nsec >> (int)(32)) & 0xffffffffL));
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

// This is the actual system call structure.
// Win32finddata is what we committed to in Go 1.
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

// The Microsoft documentation for this struct¹ describes three additional
// fields: dwFileType, dwCreatorType, and wFinderFlags. However, those fields
// are empirically only present in the macOS port of the Win32 API,² and thus
// not needed for binaries built for Windows.
//
// ¹ https://docs.microsoft.com/en-us/windows/win32/api/minwinbase/ns-minwinbase-win32_find_dataw
// ² https://golang.org/issue/42637#issuecomment-760715755
internal static void copyFindData(ref Win32finddata dst, ref win32finddata1 src) {
    dst.FileAttributes = src.FileAttributes;
    dst.CreationTime = src.CreationTime;
    dst.LastAccessTime = src.LastAccessTime;
    dst.LastWriteTime = src.LastWriteTime;
    dst.FileSizeHigh = src.FileSizeHigh;
    dst.FileSizeLow = src.FileSizeLow;
    dst.Reserved0 = src.Reserved0;
    dst.Reserved1 = src.Reserved1;
    // The src is 1 element bigger than dst, but it must be NUL.
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

// ShowWindow constants
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
    public ΔHandle StdInput;
    public ΔHandle StdOutput;
    public ΔHandle StdErr;
}

public partial struct _PROC_THREAD_ATTRIBUTE_LIST {
    internal array<byte> _ = new(1);
}

internal static UntypedInt _PROC_THREAD_ATTRIBUTE_PARENT_PROCESS => 0x00020000;
internal static UntypedInt _PROC_THREAD_ATTRIBUTE_HANDLE_LIST => 0x00020002;

partial struct _STARTUPINFOEXW {
    public partial ref StartupInfo StartupInfo { get; }
    public ж<_PROC_THREAD_ATTRIBUTE_LIST> ProcThreadAttributeList;
}

internal static UntypedInt _EXTENDED_STARTUPINFO_PRESENT => 0x00080000;

partial struct ProcessInformation {
    public ΔHandle Process;
    public ΔHandle Thread;
    public uint32 ProcessId;
    public uint32 ThreadId;
}

partial struct ProcessEntry32 {
    public uint32 Size;
    public uint32 Usage;
    public uint32 ProcessID;
    public uintptr DefaultHeapID;
    public uint32 ModuleID;
    public uint32 Threads;
    public uint32 ParentProcessID;
    public int32 PriClassBase;
    public uint32 Flags;
    public array<uint16> ExeFile = new(MAX_PATH);
}

partial struct Systemtime {
    public uint16 Year;
    public uint16 Month;
    public uint16 DayOfWeek;
    public uint16 Day;
    public uint16 Hour;
    public uint16 Minute;
    public uint16 Second;
    public uint16 Milliseconds;
}

partial struct Timezoneinformation {
    public int32 Bias;
    public array<uint16> StandardName = new(32);
    public Systemtime StandardDate;
    public int32 StandardBias;
    public array<uint16> DaylightName = new(32);
    public Systemtime DaylightDate;
    public int32 DaylightBias;
}

// Socket related.
public static UntypedInt AF_UNSPEC => 0;
public static UntypedInt AF_UNIX => 1;
public static UntypedInt AF_INET => 2;
public static UntypedInt AF_INET6 => 23;
public static UntypedInt AF_NETBIOS => 17;
public static UntypedInt SOCK_STREAM => 1;
public static UntypedInt SOCK_DGRAM => 2;
public static UntypedInt SOCK_RAW => 3;
public static UntypedInt SOCK_SEQPACKET => 5;
public static UntypedInt IPPROTO_IP => 0;
public static UntypedInt IPPROTO_IPV6 => 0x29;
public static UntypedInt IPPROTO_TCP => 6;
public static UntypedInt IPPROTO_UDP => 17;
public static UntypedInt SOL_SOCKET => 0xffff;
public static UntypedInt SO_REUSEADDR => 4;
public static UntypedInt SO_KEEPALIVE => 8;
public static UntypedInt SO_DONTROUTE => 16;
public static UntypedInt SO_BROADCAST => 32;
public static UntypedInt SO_LINGER => 128;
public static UntypedInt SO_RCVBUF => 0x1002;
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
// cf. https://learn.microsoft.com/en-US/troubleshoot/windows/win32/header-library-requirement-socket-ipproto-ip
public static UntypedInt IP_TOS => 0x3;
public static UntypedInt IP_TTL => 0x4;
public static UntypedInt IP_MULTICAST_IF => 0x9;
public static UntypedInt IP_MULTICAST_TTL => 0xa;
public static UntypedInt IP_MULTICAST_LOOP => 0xb;
public static UntypedInt IP_ADD_MEMBERSHIP => 0xc;
public static UntypedInt IP_DROP_MEMBERSHIP => 0xd;
public static UntypedInt IPV6_V6ONLY => 0x1b;
public static UntypedInt IPV6_UNICAST_HOPS => 0x4;
public static UntypedInt IPV6_MULTICAST_IF => 0x9;
public static UntypedInt IPV6_MULTICAST_HOPS => 0xa;
public static UntypedInt IPV6_MULTICAST_LOOP => 0xb;
public static UntypedInt IPV6_JOIN_GROUP => 0xc;
public static UntypedInt IPV6_LEAVE_GROUP => 0xd;
public static UntypedInt SOMAXCONN => 0x7fffffff;
public static UntypedInt TCP_NODELAY => 1;
public static UntypedInt SHUT_RD => 0;
public static UntypedInt SHUT_WR => 1;
public static UntypedInt SHUT_RDWR => 2;
public static UntypedInt WSADESCRIPTION_LEN => 256;
public static UntypedInt WSASYS_STATUS_LEN => 128;

partial struct WSABuf {
    public uint32 Len;
    public ж<byte> Buf;
}

// Invented values to support what package os expects.
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

public static UntypedInt DNS_INFO_NO_RECORDS => 0x251D;

public static UntypedInt DnsSectionQuestion => 0x0000;
public static UntypedInt DnsSectionAnswer => 0x0001;
public static UntypedInt DnsSectionAuthority => 0x0002;
public static UntypedInt DnsSectionAdditional => 0x0003;

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

// TODO(mattn): SockaddrGen is union of sockaddr/sockaddr_in/sockaddr_in6_old.
// will be fixed to change variable type as suitable.
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

partial struct MibIfRow {
    public array<uint16> Name = new(MAX_INTERFACE_NAME_LEN);
    public uint32 Index;
    public uint32 Type;
    public uint32 Mtu;
    public uint32 Speed;
    public uint32 PhysAddrLen;
    public array<byte> PhysAddr = new(MAXLEN_PHYSADDR);
    public uint32 AdminStatus;
    public uint32 OperStatus;
    public uint32 LastChange;
    public uint32 InOctets;
    public uint32 InUcastPkts;
    public uint32 InNUcastPkts;
    public uint32 InDiscards;
    public uint32 InErrors;
    public uint32 InUnknownProtos;
    public uint32 OutOctets;
    public uint32 OutUcastPkts;
    public uint32 OutNUcastPkts;
    public uint32 OutDiscards;
    public uint32 OutErrors;
    public uint32 OutQLen;
    public uint32 DescrLen;
    public array<byte> Descr = new(MAXLEN_IFDESCR);
}

partial struct CertInfo {
}

// Not implemented
partial struct CertContext {
    public uint32 EncodingType;
    public ж<byte> EncodedCert;
    public uint32 Length;
    public ж<CertInfo> CertInfo;
    public ΔHandle Store;
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

// Not implemented
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

// Not implemented
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

partial struct AddrinfoW {
    public int32 Flags;
    public int32 Family;
    public int32 Socktype;
    public int32 Protocol;
    public uintptr Addrlen;
    public ж<uint16> Canonname;
    public Pointer Addr;
    public ж<AddrinfoW> Next;
}

public static UntypedInt AI_PASSIVE => 1;
public static UntypedInt AI_CANONNAME => 2;
public static UntypedInt AI_NUMERICHOST => 4;

partial struct GUID {
    public uint32 Data1;
    public uint16 Data2;
    public uint16 Data3;
    public array<byte> Data4 = new(8);
}

public static ж<GUID> ᏑWSAID_CONNECTEX = new StandardBox<GUID>(new GUID(
    0x25a207b9,
    0xddf3,
    0x4660,
    new byte[]{0x8e, 0xe9, 0x76, 0xe5, 0x8c, 0x74, 0x06, 0x3e}.array()
));
public static ref GUID WSAID_CONNECTEX => ref ᏑWSAID_CONNECTEX.Value;

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

partial struct WSAProtocolInfo {
    public uint32 ServiceFlags1;
    public uint32 ServiceFlags2;
    public uint32 ServiceFlags3;
    public uint32 ServiceFlags4;
    public uint32 ProviderFlags;
    public GUID ProviderId;
    public uint32 CatalogEntryId;
    public WSAProtocolChain ProtocolChain;
    public int32 Version;
    public int32 AddressFamily;
    public int32 MaxSockAddr;
    public int32 MinSockAddr;
    public int32 SocketType;
    public int32 Protocol;
    public int32 ProtocolMaxOffset;
    public int32 NetworkByteOrder;
    public int32 SecurityScheme;
    public uint32 MessageSize;
    public uint32 ProviderReserved;
    public array<uint16> ProtocolName = new(WSAPROTOCOL_LEN + 1);
}

partial struct WSAProtocolChain {
    public int32 ChainLen;
    public array<uint32> ChainEntries = new(MAX_PROTOCOL_CHAIN);
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
    // GenericReparseBuffer
    internal byte reparseBuffer;
}

public static UntypedInt FSCTL_GET_REPARSE_POINT => 0x900A8;
public static UntypedInt MAXIMUM_REPARSE_DATA_BUFFER_SIZE => /* 16 * 1024 */ 16384;
internal static UntypedInt _IO_REPARSE_TAG_MOUNT_POINT => 0xA0000003;
public static UntypedInt IO_REPARSE_TAG_SYMLINK => 0xA000000C;
public static UntypedInt SYMBOLIC_LINK_FLAG_DIRECTORY => 0x1;
internal static UntypedInt _SYMLINK_FLAG_RELATIVE => 1;

public static UntypedInt UNIX_PATH_MAX => 108; // defined in afunix.h

} // end syscall_package
