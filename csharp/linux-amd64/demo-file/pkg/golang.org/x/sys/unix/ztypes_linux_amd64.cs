namespace go.golang.org.x.sys;

using System.Runtime.InteropServices;
using go.arrays;
using go.golib;

partial class unix_package {

public static UntypedInt SizeofPtr => 0x8;
public static UntypedInt SizeofLong => 0x8;

partial struct _C_long /*num:int64*/;

[global::go.golib.GoInlineLayout(Size = 16, Align = 8), StructLayout(LayoutKind.Explicit, Size = 16)] partial struct Timespec {
    [FieldOffset(0)] public int64 Sec;
    [FieldOffset(8)] public int64 Nsec;
}

[global::go.golib.GoInlineLayout(Size = 16, Align = 8), StructLayout(LayoutKind.Explicit, Size = 16)] partial struct Timeval {
    [FieldOffset(0)] public int64 Sec;
    [FieldOffset(8)] public int64 Usec;
}

[global::go.golib.GoInlineLayout(Size = 208, Align = 8), StructLayout(LayoutKind.Explicit, Size = 208)] partial struct Timex {
    [FieldOffset(0)] public uint32 Modes;
    [FieldOffset(8)] public int64 Offset;
    [FieldOffset(16)] public int64 Freq;
    [FieldOffset(24)] public int64 Maxerror;
    [FieldOffset(32)] public int64 Esterror;
    [FieldOffset(40)] public int32 Status;
    [FieldOffset(48)] public int64 Constant;
    [FieldOffset(56)] public int64 Precision;
    [FieldOffset(64)] public int64 Tolerance;
    [FieldOffset(72)] public Timeval Time;
    [FieldOffset(88)] public int64 Tick;
    [FieldOffset(96)] public int64 Ppsfreq;
    [FieldOffset(104)] public int64 Jitter;
    [FieldOffset(112)] public int32 Shift;
    [FieldOffset(120)] public int64 Stabil;
    [FieldOffset(128)] public int64 Jitcnt;
    [FieldOffset(136)] public int64 Calcnt;
    [FieldOffset(144)] public int64 Errcnt;
    [FieldOffset(152)] public int64 Stbcnt;
    [FieldOffset(160)] public int32 Tai;
    [FieldOffset(164)] internal array44<byte> _;
}

partial struct Time_t /*num:int64*/;

partial struct Tms {
    public int64 Utime;
    public int64 Stime;
    public int64 Cutime;
    public int64 Cstime;
}

partial struct Utimbuf {
    public int64 Actime;
    public int64 Modtime;
}

partial struct Rusage {
    public Timeval Utime;
    public Timeval Stime;
    public int64 Maxrss;
    public int64 Ixrss;
    public int64 Idrss;
    public int64 Isrss;
    public int64 Minflt;
    public int64 Majflt;
    public int64 Nswap;
    public int64 Inblock;
    public int64 Oublock;
    public int64 Msgsnd;
    public int64 Msgrcv;
    public int64 Nsignals;
    public int64 Nvcsw;
    public int64 Nivcsw;
}

[global::go.golib.GoInlineLayout(Size = 144, Align = 8), StructLayout(LayoutKind.Explicit, Size = 144)] partial struct Stat_t {
    [FieldOffset(0)] public uint64 Dev;
    [FieldOffset(8)] public uint64 Ino;
    [FieldOffset(16)] public uint64 Nlink;
    [FieldOffset(24)] public uint32 Mode;
    [FieldOffset(28)] public uint32 Uid;
    [FieldOffset(32)] public uint32 Gid;
    [FieldOffset(36)] internal int32 _;
    [FieldOffset(40)] public uint64 Rdev;
    [FieldOffset(48)] public int64 Size;
    [FieldOffset(56)] public int64 Blksize;
    [FieldOffset(64)] public int64 Blocks;
    [FieldOffset(72)] public Timespec Atim;
    [FieldOffset(88)] public Timespec Mtim;
    [FieldOffset(104)] public Timespec Ctim;
    [FieldOffset(120)] internal array3<int64> __;
}

partial struct Dirent {
    public uint64 Ino;
    public int64 Off;
    public uint16 Reclen;
    public uint8 Type;
    public array<int8> Name = new(256);
    internal array<byte> _ = new(5);
}

[global::go.golib.GoInlineLayout(Size = 32, Align = 8), StructLayout(LayoutKind.Explicit, Size = 32)] partial struct Flock_t {
    [FieldOffset(0)] public int16 Type;
    [FieldOffset(2)] public int16 Whence;
    [FieldOffset(8)] public int64 Start;
    [FieldOffset(16)] public int64 Len;
    [FieldOffset(24)] public int32 Pid;
    [FieldOffset(28)] internal array4<byte> _;
}

partial struct DmNameList {
    public uint64 Dev;
    public uint32 Next;
    public array<byte> Name = new(0);
    internal array<byte> _ = new(4);
}

public static UntypedInt FADV_DONTNEED => 0x4;
public static UntypedInt FADV_NOREUSE => 0x5;

partial struct RawSockaddrNFCLLCP {
    public uint16 Sa_family;
    public uint32 Dev_idx;
    public uint32 Target_idx;
    public uint32 Nfc_protocol;
    public uint8 Dsap;
    public uint8 Ssap;
    public array<uint8> Service_name = new(63);
    public uint64 Service_name_len;
}

[global::go.golib.GoInlineLayout(Size = 16, Align = 2), StructLayout(LayoutKind.Explicit, Size = 16)] partial struct RawSockaddr {
    [FieldOffset(0)] public uint16 Family;
    [FieldOffset(2)] public array14<int8> Data;
}

partial struct RawSockaddrAny {
    public RawSockaddr Addr;
    public array<int8> Pad = new(96);
}

partial struct Iovec {
    public ж<byte> Base;
    public uint64 Len;
}

partial struct Msghdr {
    public ж<byte> Name;
    public uint32 Namelen;
    public ж<Iovec> Iov;
    public uint64 Iovlen;
    public ж<byte> Control;
    public uint64 Controllen;
    public int32 Flags;
    internal array<byte> _ = new(4);
}

partial struct Mmsghdr {
    public Msghdr Hdr;
    public uint32 Len;
    internal array<byte> _ = new(4);
}

partial struct Cmsghdr {
    public uint64 Len;
    public int32 Level;
    public int32 Type;
}

partial struct ifreq {
    public array<byte> Ifrn = new(16);
    public array<byte> Ifru = new(24);
}

public static UntypedInt SizeofSockaddrNFCLLCP => 0x60;
public static UntypedInt SizeofIovec => 0x10;
public static UntypedInt SizeofMsghdr => 0x38;
public static UntypedInt SizeofMmsghdr => 0x40;
public static UntypedInt SizeofCmsghdr => 0x10;

public static UntypedInt SizeofSockFprog => 0x10;

partial struct PtraceRegs {
    public uint64 R15;
    public uint64 R14;
    public uint64 R13;
    public uint64 R12;
    public uint64 Rbp;
    public uint64 Rbx;
    public uint64 R11;
    public uint64 R10;
    public uint64 R9;
    public uint64 R8;
    public uint64 Rax;
    public uint64 Rcx;
    public uint64 Rdx;
    public uint64 Rsi;
    public uint64 Rdi;
    public uint64 Orig_rax;
    public uint64 Rip;
    public uint64 Cs;
    public uint64 Eflags;
    public uint64 Rsp;
    public uint64 Ss;
    public uint64 Fs_base;
    public uint64 Gs_base;
    public uint64 Ds;
    public uint64 Es;
    public uint64 Fs;
    public uint64 Gs;
}

partial struct FdSet {
    public array<int64> Bits = new(16);
}

[global::go.golib.GoInlineLayout(Size = 112, Align = 8), StructLayout(LayoutKind.Explicit, Size = 112)] partial struct Sysinfo_t {
    [FieldOffset(0)] public int64 Uptime;
    [FieldOffset(8)] public array3<uint64> Loads;
    [FieldOffset(32)] public uint64 Totalram;
    [FieldOffset(40)] public uint64 Freeram;
    [FieldOffset(48)] public uint64 Sharedram;
    [FieldOffset(56)] public uint64 Bufferram;
    [FieldOffset(64)] public uint64 Totalswap;
    [FieldOffset(72)] public uint64 Freeswap;
    [FieldOffset(80)] public uint16 Procs;
    [FieldOffset(82)] public uint16 Pad;
    [FieldOffset(88)] public uint64 Totalhigh;
    [FieldOffset(96)] public uint64 Freehigh;
    [FieldOffset(104)] public uint32 Unit;
    [FieldOffset(108)] [GoAlignmentCarrier(typeof(array<int8>))] internal readonly byte _;
    [FieldOffset(108)] internal array4<byte> __;
}

[global::go.golib.GoInlineLayout(Size = 32, Align = 8), StructLayout(LayoutKind.Explicit, Size = 32)] partial struct Ustat_t {
    [FieldOffset(0)] public int32 Tfree;
    [FieldOffset(8)] public uint64 Tinode;
    [FieldOffset(16)] public array6<int8> Fname;
    [FieldOffset(22)] public array6<int8> Fpack;
    [FieldOffset(28)] internal array4<byte> _;
}

partial struct EpollEvent {
    public uint32 Events;
    public int32 Fd;
    public int32 Pad;
}

public static UntypedInt OPEN_TREE_CLOEXEC => 0x80000;

public static UntypedInt POLLRDHUP => 0x2000;

[global::go.golib.GoInlineLayout(Size = 128, Align = 8), StructLayout(LayoutKind.Explicit, Size = 128)] partial struct Sigset_t {
    [FieldOffset(0)] public array16<uint64> Val;
}

internal static UntypedInt _C__NSIG => 0x41;

public static UntypedInt SIG_BLOCK => 0x0;
public static UntypedInt SIG_UNBLOCK => 0x1;
public static UntypedInt SIG_SETMASK => 0x2;

[global::go.golib.GoInlineLayout(Size = 128, Align = 4), StructLayout(LayoutKind.Explicit, Size = 128)] partial struct Siginfo {
    [FieldOffset(0)] public int32 Signo;
    [FieldOffset(4)] public int32 Errno;
    [FieldOffset(8)] public int32 Code;
    [FieldOffset(12)] internal int32 _;
    [FieldOffset(16)] internal array112<byte> __;
}

[global::go.golib.GoInlineLayout(Size = 44, Align = 4), StructLayout(LayoutKind.Explicit, Size = 44)] partial struct Termios {
    [FieldOffset(0)] public uint32 Iflag;
    [FieldOffset(4)] public uint32 Oflag;
    [FieldOffset(8)] public uint32 Cflag;
    [FieldOffset(12)] public uint32 Lflag;
    [FieldOffset(16)] public uint8 Line;
    [FieldOffset(17)] public array19<uint8> Cc;
    [FieldOffset(36)] public uint32 Ispeed;
    [FieldOffset(40)] public uint32 Ospeed;
}

partial struct Taskstats {
    public uint16 Version;
    public uint32 Ac_exitcode;
    public uint8 Ac_flag;
    public uint8 Ac_nice;
    public uint64 Cpu_count;
    public uint64 Cpu_delay_total;
    public uint64 Blkio_count;
    public uint64 Blkio_delay_total;
    public uint64 Swapin_count;
    public uint64 Swapin_delay_total;
    public uint64 Cpu_run_real_total;
    public uint64 Cpu_run_virtual_total;
    public array<int8> Ac_comm = new(32);
    public uint8 Ac_sched;
    public array<uint8> Ac_pad = new(3);
    internal array<byte> _ = new(4);
    public uint32 Ac_uid;
    public uint32 Ac_gid;
    public uint32 Ac_pid;
    public uint32 Ac_ppid;
    public uint32 Ac_btime;
    public uint64 Ac_etime;
    public uint64 Ac_utime;
    public uint64 Ac_stime;
    public uint64 Ac_minflt;
    public uint64 Ac_majflt;
    public uint64 Coremem;
    public uint64 Virtmem;
    public uint64 Hiwater_rss;
    public uint64 Hiwater_vm;
    public uint64 Read_char;
    public uint64 Write_char;
    public uint64 Read_syscalls;
    public uint64 Write_syscalls;
    public uint64 Read_bytes;
    public uint64 Write_bytes;
    public uint64 Cancelled_write_bytes;
    public uint64 Nvcsw;
    public uint64 Nivcsw;
    public uint64 Ac_utimescaled;
    public uint64 Ac_stimescaled;
    public uint64 Cpu_scaled_run_real_total;
    public uint64 Freepages_count;
    public uint64 Freepages_delay_total;
    public uint64 Thrashing_count;
    public uint64 Thrashing_delay_total;
    public uint64 Ac_btime64;
    public uint64 Compact_count;
    public uint64 Compact_delay_total;
    public uint32 Ac_tgid;
    public uint64 Ac_tgetime;
    public uint64 Ac_exe_dev;
    public uint64 Ac_exe_inode;
    public uint64 Wpcopy_count;
    public uint64 Wpcopy_delay_total;
    public uint64 Irq_count;
    public uint64 Irq_delay_total;
    public uint64 Cpu_delay_max;
    public uint64 Cpu_delay_min;
    public uint64 Blkio_delay_max;
    public uint64 Blkio_delay_min;
    public uint64 Swapin_delay_max;
    public uint64 Swapin_delay_min;
    public uint64 Freepages_delay_max;
    public uint64 Freepages_delay_min;
    public uint64 Thrashing_delay_max;
    public uint64 Thrashing_delay_min;
    public uint64 Compact_delay_max;
    public uint64 Compact_delay_min;
    public uint64 Wpcopy_delay_max;
    public uint64 Wpcopy_delay_min;
    public uint64 Irq_delay_max;
    public uint64 Irq_delay_min;
    public KernelTimespec Cpu_delay_max_ts;
    public KernelTimespec Blkio_delay_max_ts;
    public KernelTimespec Swapin_delay_max_ts;
    public KernelTimespec Freepages_delay_max_ts;
    public KernelTimespec Thrashing_delay_max_ts;
    public KernelTimespec Compact_delay_max_ts;
    public KernelTimespec Wpcopy_delay_max_ts;
    public KernelTimespec Irq_delay_max_ts;
}

public partial struct cpuMask /*num:uint64*/;

internal static UntypedInt _NCPUBITS => 0x40;

public static UntypedInt CBitFieldMaskBit0 => 0x1;
public static UntypedInt CBitFieldMaskBit1 => 0x2;
public static UntypedInt CBitFieldMaskBit2 => 0x4;
public static UntypedInt CBitFieldMaskBit3 => 0x8;
public static UntypedInt CBitFieldMaskBit4 => 0x10;
public static UntypedInt CBitFieldMaskBit5 => 0x20;
public static UntypedInt CBitFieldMaskBit6 => 0x40;
public static UntypedInt CBitFieldMaskBit7 => 0x80;
public static UntypedInt CBitFieldMaskBit8 => 0x100;
public static UntypedInt CBitFieldMaskBit9 => 0x200;
public static UntypedInt CBitFieldMaskBit10 => 0x400;
public static UntypedInt CBitFieldMaskBit11 => 0x800;
public static UntypedInt CBitFieldMaskBit12 => 0x1000;
public static UntypedInt CBitFieldMaskBit13 => 0x2000;
public static UntypedInt CBitFieldMaskBit14 => 0x4000;
public static UntypedInt CBitFieldMaskBit15 => 0x8000;
public static UntypedInt CBitFieldMaskBit16 => 0x10000;
public static UntypedInt CBitFieldMaskBit17 => 0x20000;
public static UntypedInt CBitFieldMaskBit18 => 0x40000;
public static UntypedInt CBitFieldMaskBit19 => 0x80000;
public static UntypedInt CBitFieldMaskBit20 => 0x100000;
public static UntypedInt CBitFieldMaskBit21 => 0x200000;
public static UntypedInt CBitFieldMaskBit22 => 0x400000;
public static UntypedInt CBitFieldMaskBit23 => 0x800000;
public static UntypedInt CBitFieldMaskBit24 => 0x1000000;
public static UntypedInt CBitFieldMaskBit25 => 0x2000000;
public static UntypedInt CBitFieldMaskBit26 => 0x4000000;
public static UntypedInt CBitFieldMaskBit27 => 0x8000000;
public static UntypedInt CBitFieldMaskBit28 => 0x10000000;
public static UntypedInt CBitFieldMaskBit29 => 0x20000000;
public static UntypedInt CBitFieldMaskBit30 => 0x40000000;
public static UntypedInt CBitFieldMaskBit31 => 0x80000000;
public static UntypedInt CBitFieldMaskBit32 => 0x100000000;
public static UntypedInt CBitFieldMaskBit33 => 0x200000000;
public static UntypedInt CBitFieldMaskBit34 => 0x400000000;
public static UntypedInt CBitFieldMaskBit35 => 0x800000000;
public static UntypedInt CBitFieldMaskBit36 => 0x1000000000;
public static UntypedInt CBitFieldMaskBit37 => 0x2000000000;
public static UntypedInt CBitFieldMaskBit38 => 0x4000000000;
public static UntypedInt CBitFieldMaskBit39 => 0x8000000000;
public static UntypedInt CBitFieldMaskBit40 => 0x10000000000;
public static UntypedInt CBitFieldMaskBit41 => 0x20000000000;
public static UntypedInt CBitFieldMaskBit42 => 0x40000000000;
public static UntypedInt CBitFieldMaskBit43 => 0x80000000000;
public static UntypedInt CBitFieldMaskBit44 => 0x100000000000;
public static UntypedInt CBitFieldMaskBit45 => 0x200000000000;
public static UntypedInt CBitFieldMaskBit46 => 0x400000000000;
public static UntypedInt CBitFieldMaskBit47 => 0x800000000000;
public static UntypedInt CBitFieldMaskBit48 => 0x1000000000000;
public static UntypedInt CBitFieldMaskBit49 => 0x2000000000000;
public static UntypedInt CBitFieldMaskBit50 => 0x4000000000000;
public static UntypedInt CBitFieldMaskBit51 => 0x8000000000000;
public static UntypedInt CBitFieldMaskBit52 => 0x10000000000000;
public static UntypedInt CBitFieldMaskBit53 => 0x20000000000000;
public static UntypedInt CBitFieldMaskBit54 => 0x40000000000000;
public static UntypedInt CBitFieldMaskBit55 => 0x80000000000000;
public static UntypedInt CBitFieldMaskBit56 => 0x100000000000000;
public static UntypedInt CBitFieldMaskBit57 => 0x200000000000000;
public static UntypedInt CBitFieldMaskBit58 => 0x400000000000000;
public static UntypedInt CBitFieldMaskBit59 => 0x800000000000000;
public static UntypedInt CBitFieldMaskBit60 => 0x1000000000000000;
public static UntypedInt CBitFieldMaskBit61 => 0x2000000000000000;
public static UntypedInt CBitFieldMaskBit62 => 0x4000000000000000;
public static UntypedInt CBitFieldMaskBit63 => 0x8000000000000000;

[global::go.golib.GoInlineLayout(Size = 128, Align = 8), StructLayout(LayoutKind.Explicit, Size = 128)] partial struct SockaddrStorage {
    [FieldOffset(0)] public uint16 Family;
    [FieldOffset(2)] public array118<byte> Data;
    [FieldOffset(120)] internal uint64 _;
}

partial struct HDGeometry {
    public uint8 Heads;
    public uint8 Sectors;
    public uint16 Cylinders;
    public uint64 Start;
}

[global::go.golib.GoInlineLayout(Size = 120, Align = 8), StructLayout(LayoutKind.Explicit, Size = 120)] partial struct Statfs_t {
    [FieldOffset(0)] public int64 Type;
    [FieldOffset(8)] public int64 Bsize;
    [FieldOffset(16)] public uint64 Blocks;
    [FieldOffset(24)] public uint64 Bfree;
    [FieldOffset(32)] public uint64 Bavail;
    [FieldOffset(40)] public uint64 Files;
    [FieldOffset(48)] public uint64 Ffree;
    [FieldOffset(56)] public Fsid Fsid;
    [FieldOffset(64)] public int64 Namelen;
    [FieldOffset(72)] public int64 Frsize;
    [FieldOffset(80)] public int64 Flags;
    [FieldOffset(88)] public array4<int64> Spare;
}

partial struct TpacketHdr {
    public uint64 Status;
    public uint32 Len;
    public uint32 Snaplen;
    public uint16 Mac;
    public uint16 Net;
    public uint32 Sec;
    public uint32 Usec;
    internal array<byte> _ = new(4);
}

public static UntypedInt SizeofTpacketHdr => 0x20;

partial struct RTCPLLInfo {
    public int32 Ctrl;
    public int32 Value;
    public int32 Max;
    public int32 Min;
    public int32 Posmult;
    public int32 Negmult;
    public int64 Clock;
}

partial struct BlkpgPartition {
    public int64 Start;
    public int64 Length;
    public int32 Pno;
    public array<uint8> Devname = new(64);
    public array<uint8> Volname = new(64);
    internal array<byte> _ = new(4);
}

public static UntypedInt BLKPG => 0x1269;

partial struct CryptoUserAlg {
    public array<int8> Name = new(64);
    public array<int8> Driver_name = new(64);
    public array<int8> Module_name = new(64);
    public uint32 Type;
    public uint32 Mask;
    public uint32 Refcnt;
    public uint32 Flags;
}

partial struct CryptoStatAEAD {
    public array<int8> Type = new(64);
    public uint64 Encrypt_cnt;
    public uint64 Encrypt_tlen;
    public uint64 Decrypt_cnt;
    public uint64 Decrypt_tlen;
    public uint64 Err_cnt;
}

partial struct CryptoStatAKCipher {
    public array<int8> Type = new(64);
    public uint64 Encrypt_cnt;
    public uint64 Encrypt_tlen;
    public uint64 Decrypt_cnt;
    public uint64 Decrypt_tlen;
    public uint64 Verify_cnt;
    public uint64 Sign_cnt;
    public uint64 Err_cnt;
}

partial struct CryptoStatCipher {
    public array<int8> Type = new(64);
    public uint64 Encrypt_cnt;
    public uint64 Encrypt_tlen;
    public uint64 Decrypt_cnt;
    public uint64 Decrypt_tlen;
    public uint64 Err_cnt;
}

partial struct CryptoStatCompress {
    public array<int8> Type = new(64);
    public uint64 Compress_cnt;
    public uint64 Compress_tlen;
    public uint64 Decompress_cnt;
    public uint64 Decompress_tlen;
    public uint64 Err_cnt;
}

partial struct CryptoStatHash {
    public array<int8> Type = new(64);
    public uint64 Hash_cnt;
    public uint64 Hash_tlen;
    public uint64 Err_cnt;
}

partial struct CryptoStatKPP {
    public array<int8> Type = new(64);
    public uint64 Setsecret_cnt;
    public uint64 Generate_public_key_cnt;
    public uint64 Compute_shared_secret_cnt;
    public uint64 Err_cnt;
}

partial struct CryptoStatRNG {
    public array<int8> Type = new(64);
    public uint64 Generate_cnt;
    public uint64 Generate_tlen;
    public uint64 Seed_cnt;
    public uint64 Err_cnt;
}

partial struct CryptoStatLarval {
    public array<int8> Type = new(64);
}

partial struct CryptoReportLarval {
    public array<int8> Type = new(64);
}

partial struct CryptoReportHash {
    public array<int8> Type = new(64);
    public uint32 Blocksize;
    public uint32 Digestsize;
}

partial struct CryptoReportCipher {
    public array<int8> Type = new(64);
    public uint32 Blocksize;
    public uint32 Min_keysize;
    public uint32 Max_keysize;
}

partial struct CryptoReportBlkCipher {
    public array<int8> Type = new(64);
    public array<int8> Geniv = new(64);
    public uint32 Blocksize;
    public uint32 Min_keysize;
    public uint32 Max_keysize;
    public uint32 Ivsize;
}

partial struct CryptoReportAEAD {
    public array<int8> Type = new(64);
    public array<int8> Geniv = new(64);
    public uint32 Blocksize;
    public uint32 Maxauthsize;
    public uint32 Ivsize;
}

partial struct CryptoReportComp {
    public array<int8> Type = new(64);
}

partial struct CryptoReportRNG {
    public array<int8> Type = new(64);
    public uint32 Seedsize;
}

partial struct CryptoReportAKCipher {
    public array<int8> Type = new(64);
}

partial struct CryptoReportKPP {
    public array<int8> Type = new(64);
}

partial struct CryptoReportAcomp {
    public array<int8> Type = new(64);
}

partial struct LoopInfo {
    public int32 Number;
    public uint64 Device;
    public uint64 Inode;
    public uint64 Rdevice;
    public int32 Offset;
    public int32 Encrypt_type;
    public int32 Encrypt_key_size;
    public int32 Flags;
    public array<int8> Name = new(64);
    public array<uint8> Encrypt_key = new(32);
    public array<uint64> Init = new(2);
    public array<int8> Reserved = new(4);
    internal array<byte> _ = new(4);
}

partial struct TIPCSubscr {
    public TIPCServiceRange Seq;
    public uint32 Timeout;
    public uint32 Filter;
    public array<int8> Handle = new(8);
}

partial struct TIPCSIOCLNReq {
    public uint32 Peer;
    public uint32 Id;
    public array<int8> Linkname = new(68);
}

partial struct TIPCSIOCNodeIDReq {
    public uint32 Peer;
    public array<int8> Id = new(16);
}

partial struct PPSKInfo {
    public uint32 Assert_sequence;
    public uint32 Clear_sequence;
    public PPSKTime Assert_tu;
    public PPSKTime Clear_tu;
    public int32 Current_mode;
    internal array<byte> _ = new(4);
}

public static UntypedInt PPS_GETPARAMS => 0x800870a1;
public static UntypedInt PPS_SETPARAMS => 0x400870a2;
public static UntypedInt PPS_GETCAP => 0x800870a3;
public static UntypedInt PPS_FETCH => 0xc00870a4;

public static UntypedInt PIDFD_NONBLOCK => 0x800;
public static UntypedInt PIDFD_THREAD => 0x80;
public static UntypedInt PIDFD_GET_CGROUP_NAMESPACE => 0xff01;
public static UntypedInt PIDFD_GET_IPC_NAMESPACE => 0xff02;
public static UntypedInt PIDFD_GET_MNT_NAMESPACE => 0xff03;
public static UntypedInt PIDFD_GET_NET_NAMESPACE => 0xff04;
public static UntypedInt PIDFD_GET_PID_NAMESPACE => 0xff05;
public static UntypedInt PIDFD_GET_PID_FOR_CHILDREN_NAMESPACE => 0xff06;
public static UntypedInt PIDFD_GET_TIME_NAMESPACE => 0xff07;
public static UntypedInt PIDFD_GET_TIME_FOR_CHILDREN_NAMESPACE => 0xff08;
public static UntypedInt PIDFD_GET_USER_NAMESPACE => 0xff09;
public static UntypedInt PIDFD_GET_UTS_NAMESPACE => 0xff0a;

[global::go.golib.GoInlineLayout(Size = 48, Align = 8), StructLayout(LayoutKind.Explicit, Size = 48)] partial struct SysvIpcPerm {
    [FieldOffset(0)] public int32 Key;
    [FieldOffset(4)] public uint32 Uid;
    [FieldOffset(8)] public uint32 Gid;
    [FieldOffset(12)] public uint32 Cuid;
    [FieldOffset(16)] public uint32 Cgid;
    [FieldOffset(20)] public uint32 Mode;
    [FieldOffset(24)] [GoAlignmentCarrier(typeof(array<uint8>))] internal readonly byte _;
    [FieldOffset(24)] public uint16 Seq;
    [FieldOffset(26)] internal uint16 __;
    [FieldOffset(32)] internal uint64 ___;
    [FieldOffset(40)] internal uint64 ____;
}
[global::go.golib.GoInlineLayout(Size = 112, Align = 8), StructLayout(LayoutKind.Explicit, Size = 112)] partial struct SysvShmDesc {
    [FieldOffset(0)] public SysvIpcPerm Perm;
    [FieldOffset(48)] public uint64 Segsz;
    [FieldOffset(56)] public int64 Atime;
    [FieldOffset(64)] public int64 Dtime;
    [FieldOffset(72)] public int64 Ctime;
    [FieldOffset(80)] public int32 Cpid;
    [FieldOffset(84)] public int32 Lpid;
    [FieldOffset(88)] public uint64 Nattch;
    [FieldOffset(96)] internal uint64 _;
    [FieldOffset(104)] internal uint64 __;
}

public static UntypedInt GPIO_GET_CHIPINFO_IOCTL => 0x8044b401;

public static UntypedInt IPMICTL_SEND_COMMAND => 0x8028690d;
public static UntypedInt IPMICTL_RECEIVE_MSG => 0xc030690c;

partial struct IPMIReq {
    public ж<uint8> Addr;
    public uint32 Len;
    public int64 Msgid;
    public IPMIMsg Msg;
}
partial struct IPMIRecv {
    public int32 Recv_type;
    public ж<uint8> Addr;
    public uint32 Addr_len;
    public int64 Msgid;
    public IPMIMsg Msg;
}

} // end unix_package
