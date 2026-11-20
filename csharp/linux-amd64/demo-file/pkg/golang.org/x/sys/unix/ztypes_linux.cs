namespace go.golang.org.x.sys;

using System.Runtime.InteropServices;
using go.arrays;
using go.golib;

partial class unix_package {

public static UntypedInt SizeofShort => 0x2;
public static UntypedInt SizeofInt => 0x4;
public static UntypedInt SizeofLongLong => 0x8;
public static UntypedInt PathMax => 0x1000;

partial struct _C_short /*num:int16*/;

partial struct _C_int /*num:int32*/;

partial struct _C_long_long /*num:int64*/;

partial struct KernelTimespec {
    public int64 Sec;
    public int64 Nsec;
}

partial struct ItimerSpec {
    public Timespec Interval;
    public Timespec Value;
}

partial struct Itimerval {
    public Timeval Interval;
    public Timeval Value;
}

public static UntypedInt ADJ_OFFSET => 0x1;
public static UntypedInt ADJ_FREQUENCY => 0x2;
public static UntypedInt ADJ_MAXERROR => 0x4;
public static UntypedInt ADJ_ESTERROR => 0x8;
public static UntypedInt ADJ_STATUS => 0x10;
public static UntypedInt ADJ_TIMECONST => 0x20;
public static UntypedInt ADJ_TAI => 0x80;
public static UntypedInt ADJ_SETOFFSET => 0x100;
public static UntypedInt ADJ_MICRO => 0x1000;
public static UntypedInt ADJ_NANO => 0x2000;
public static UntypedInt ADJ_TICK => 0x4000;
public static UntypedInt ADJ_OFFSET_SINGLESHOT => 0x8001;
public static UntypedInt ADJ_OFFSET_SS_READ => 0xa001;

public static UntypedInt STA_PLL => 0x1;
public static UntypedInt STA_PPSFREQ => 0x2;
public static UntypedInt STA_PPSTIME => 0x4;
public static UntypedInt STA_FLL => 0x8;
public static UntypedInt STA_INS => 0x10;
public static UntypedInt STA_DEL => 0x20;
public static UntypedInt STA_UNSYNC => 0x40;
public static UntypedInt STA_FREQHOLD => 0x80;
public static UntypedInt STA_PPSSIGNAL => 0x100;
public static UntypedInt STA_PPSJITTER => 0x200;
public static UntypedInt STA_PPSWANDER => 0x400;
public static UntypedInt STA_PPSERROR => 0x800;
public static UntypedInt STA_CLOCKERR => 0x1000;
public static UntypedInt STA_NANO => 0x2000;
public static UntypedInt STA_MODE => 0x4000;
public static UntypedInt STA_CLK => 0x8000;

public static UntypedInt TIME_OK => 0x0;
public static UntypedInt TIME_INS => 0x1;
public static UntypedInt TIME_DEL => 0x2;
public static UntypedInt TIME_OOP => 0x3;
public static UntypedInt TIME_WAIT => 0x4;
public static UntypedInt TIME_ERROR => 0x5;
public static UntypedInt TIME_BAD => 0x5;

partial struct Rlimit {
    public uint64 Cur;
    public uint64 Max;
}

partial struct _Gid_t /*num:uint32*/;

[global::go.golib.GoInlineLayout(Size = 16, Align = 8), StructLayout(LayoutKind.Explicit, Size = 16)] partial struct StatxTimestamp {
    [FieldOffset(0)] public int64 Sec;
    [FieldOffset(8)] public uint32 Nsec;
    [FieldOffset(12)] internal int32 _;
}

[global::go.golib.GoInlineLayout(Size = 256, Align = 8), StructLayout(LayoutKind.Explicit, Size = 256)] partial struct Statx_t {
    [FieldOffset(0)] public uint32 Mask;
    [FieldOffset(4)] public uint32 Blksize;
    [FieldOffset(8)] public uint64 Attributes;
    [FieldOffset(16)] public uint32 Nlink;
    [FieldOffset(20)] public uint32 Uid;
    [FieldOffset(24)] public uint32 Gid;
    [FieldOffset(28)] public uint16 Mode;
    [FieldOffset(30)] internal array1<uint16> _;
    [FieldOffset(32)] public uint64 Ino;
    [FieldOffset(40)] public uint64 Size;
    [FieldOffset(48)] public uint64 Blocks;
    [FieldOffset(56)] public uint64 Attributes_mask;
    [FieldOffset(64)] public StatxTimestamp Atime;
    [FieldOffset(80)] public StatxTimestamp Btime;
    [FieldOffset(96)] public StatxTimestamp Ctime;
    [FieldOffset(112)] public StatxTimestamp Mtime;
    [FieldOffset(128)] public uint32 Rdev_major;
    [FieldOffset(132)] public uint32 Rdev_minor;
    [FieldOffset(136)] public uint32 Dev_major;
    [FieldOffset(140)] public uint32 Dev_minor;
    [FieldOffset(144)] public uint64 Mnt_id;
    [FieldOffset(152)] public uint32 Dio_mem_align;
    [FieldOffset(156)] public uint32 Dio_offset_align;
    [FieldOffset(160)] public uint64 Subvol;
    [FieldOffset(168)] public uint32 Atomic_write_unit_min;
    [FieldOffset(172)] public uint32 Atomic_write_unit_max;
    [FieldOffset(176)] public uint32 Atomic_write_segments_max;
    [FieldOffset(180)] public uint32 Dio_read_offset_align;
    [FieldOffset(184)] public uint32 Atomic_write_unit_max_opt;
    [FieldOffset(188)] internal array1<uint32> __;
    [FieldOffset(192)] internal array8<uint64> ___;
}

[global::go.golib.GoInlineLayout(Size = 8, Align = 4), StructLayout(LayoutKind.Explicit, Size = 8)] partial struct Fsid {
    [FieldOffset(0)] public array2<int32> Val;
}

partial struct FileCloneRange {
    public int64 Src_fd;
    public uint64 Src_offset;
    public uint64 Src_length;
    public uint64 Dest_offset;
}

partial struct RawFileDedupeRange {
    public uint64 Src_offset;
    public uint64 Src_length;
    public uint16 Dest_count;
    public uint16 Reserved1;
    public uint32 Reserved2;
}

partial struct RawFileDedupeRangeInfo {
    public int64 Dest_fd;
    public uint64 Dest_offset;
    public uint64 Bytes_deduped;
    public int32 Status;
    public uint32 Reserved;
}

public static UntypedInt SizeofRawFileDedupeRange => 0x18;
public static UntypedInt SizeofRawFileDedupeRangeInfo => 0x20;
public static UntypedInt FILE_DEDUPE_RANGE_SAME => 0x0;
public static UntypedInt FILE_DEDUPE_RANGE_DIFFERS => 0x1;

partial struct FscryptPolicy {
    public uint8 Version;
    public uint8 Contents_encryption_mode;
    public uint8 Filenames_encryption_mode;
    public uint8 Flags;
    public array<uint8> Master_key_descriptor = new(8);
}

partial struct FscryptKey {
    public uint32 Mode;
    public array<uint8> Raw = new(64);
    public uint32 Size;
}

partial struct FscryptPolicyV1 {
    public uint8 Version;
    public uint8 Contents_encryption_mode;
    public uint8 Filenames_encryption_mode;
    public uint8 Flags;
    public array<uint8> Master_key_descriptor = new(8);
}

partial struct FscryptPolicyV2 {
    public uint8 Version;
    public uint8 Contents_encryption_mode;
    public uint8 Filenames_encryption_mode;
    public uint8 Flags;
    public uint8 Log2_data_unit_size;
    internal array<uint8> _ = new(3);
    public array<uint8> Master_key_identifier = new(16);
}

partial struct FscryptGetPolicyExArg {
    public uint64 Size;
    public array<byte> Policy = new(24);
}

partial struct FscryptKeySpecifier {
    public uint32 Type;
    internal uint32 _;
    public array<byte> U = new(32);
}

partial struct FscryptAddKeyArg {
    public FscryptKeySpecifier Key_spec;
    public uint32 Raw_size;
    public uint32 Key_id;
    public uint32 Flags;
    internal array<uint32> _ = new(7);
}

partial struct FscryptRemoveKeyArg {
    public FscryptKeySpecifier Key_spec;
    public uint32 Removal_status_flags;
    internal array<uint32> _ = new(5);
}

partial struct FscryptGetKeyStatusArg {
    public FscryptKeySpecifier Key_spec;
    internal array<uint32> _ = new(6);
    public uint32 Status;
    public uint32 Status_flags;
    public uint32 User_count;
    internal array<uint32> __ = new(13);
}

partial struct DmIoctl {
    public array<uint32> Version = new(3);
    public uint32 Data_size;
    public uint32 Data_start;
    public uint32 Target_count;
    public int32 Open_count;
    public uint32 Flags;
    public uint32 Event_nr;
    internal uint32 _;
    public uint64 Dev;
    public array<byte> Name = new(128);
    public array<byte> Uuid = new(129);
    public array<byte> Data = new(7);
}

partial struct DmTargetSpec {
    public uint64 Sector_start;
    public uint64 Length;
    public int32 Status;
    public uint32 Next;
    public array<byte> Target_type = new(16);
}

partial struct DmTargetDeps {
    public uint32 Count;
    internal uint32 _;
}

partial struct DmTargetVersions {
    public uint32 Next;
    public array<uint32> Version = new(3);
}

partial struct DmTargetMsg {
    public uint64 Sector;
}

public static UntypedInt SizeofDmIoctl => 0x138;
public static UntypedInt SizeofDmTargetSpec => 0x28;

partial struct KeyctlDHParams {
    public int32 Private;
    public int32 Prime;
    public int32 Base;
}

public static UntypedInt FADV_NORMAL => 0x0;
public static UntypedInt FADV_RANDOM => 0x1;
public static UntypedInt FADV_SEQUENTIAL => 0x2;
public static UntypedInt FADV_WILLNEED => 0x3;

partial struct RawSockaddrInet4 {
    public uint16 Family;
    public uint16 Port;
    public array<byte> Addr = new(4);
    public array<uint8> Zero = new(8);
}

partial struct RawSockaddrInet6 {
    public uint16 Family;
    public uint16 Port;
    public uint32 Flowinfo;
    public array<byte> Addr = new(16);
    public uint32 Scope_id;
}

partial struct RawSockaddrUnix {
    public uint16 Family;
    public array<int8> Path = new(108);
}

partial struct RawSockaddrLinklayer {
    public uint16 Family;
    public uint16 Protocol;
    public int32 Ifindex;
    public uint16 Hatype;
    public uint8 Pkttype;
    public uint8 Halen;
    public array<uint8> Addr = new(8);
}

partial struct RawSockaddrNetlink {
    public uint16 Family;
    public uint16 Pad;
    public uint32 Pid;
    public uint32 Groups;
}

partial struct RawSockaddrHCI {
    public uint16 Family;
    public uint16 Dev;
    public uint16 Channel;
}

partial struct RawSockaddrL2 {
    public uint16 Family;
    public uint16 Psm;
    public array<uint8> Bdaddr = new(6);
    public uint16 Cid;
    public uint8 Bdaddr_type;
    internal array<byte> _ = new(1);
}

partial struct RawSockaddrRFCOMM {
    public uint16 Family;
    public array<uint8> Bdaddr = new(6);
    public uint8 Channel;
    internal array<byte> _ = new(1);
}

partial struct RawSockaddrCAN {
    public uint16 Family;
    public int32 Ifindex;
    public array<byte> Addr = new(16);
}

partial struct RawSockaddrALG {
    public uint16 Family;
    public array<uint8> Type = new(14);
    public uint32 Feat;
    public uint32 Mask;
    public array<uint8> Name = new(64);
}

partial struct RawSockaddrVM {
    public uint16 Family;
    public uint16 Reserved1;
    public uint32 Port;
    public uint32 Cid;
    public uint8 Flags;
    public array<uint8> Zero = new(3);
}

partial struct RawSockaddrXDP {
    public uint16 Family;
    public uint16 Flags;
    public uint32 Ifindex;
    public uint32 Queue_id;
    public uint32 Shared_umem_fd;
}

/* [0x1e]byte */
partial struct RawSockaddrPPPoX /*[30]byte*/;

partial struct RawSockaddrTIPC {
    public uint16 Family;
    public uint8 Addrtype;
    public int8 Scope;
    public array<byte> Addr = new(12);
}

partial struct RawSockaddrL2TPIP {
    public uint16 Family;
    public uint16 Unused;
    public array<byte> Addr = new(4);
    public uint32 Conn_id;
    internal array<uint8> _ = new(4);
}

partial struct RawSockaddrL2TPIP6 {
    public uint16 Family;
    public uint16 Unused;
    public uint32 Flowinfo;
    public array<byte> Addr = new(16);
    public uint32 Scope_id;
    public uint32 Conn_id;
}

partial struct RawSockaddrIUCV {
    public uint16 Family;
    public uint16 Port;
    public uint32 Addr;
    public array<int8> Nodeid = new(8);
    public array<int8> User_id = new(8);
    public array<int8> Name = new(8);
}

partial struct RawSockaddrNFC {
    public uint16 Sa_family;
    public uint32 Dev_idx;
    public uint32 Target_idx;
    public uint32 Nfc_protocol;
}

public partial struct _Socklen /*num:uint32*/;

partial struct Linger {
    public int32 Onoff;
    public int32 ΔLinger;
}

[global::go.golib.GoInlineLayout(Size = 8, Align = 1), StructLayout(LayoutKind.Explicit, Size = 8)] partial struct IPMreq {
    [FieldOffset(0)] public array4<byte> Multiaddr;
    [FieldOffset(4)] public array4<byte> Interface;
}

[global::go.golib.GoInlineLayout(Size = 12, Align = 4), StructLayout(LayoutKind.Explicit, Size = 12)] partial struct IPMreqn {
    [FieldOffset(0)] public array4<byte> Multiaddr;
    [FieldOffset(4)] public array4<byte> Address;
    [FieldOffset(8)] public int32 Ifindex;
}

[global::go.golib.GoInlineLayout(Size = 20, Align = 4), StructLayout(LayoutKind.Explicit, Size = 20)] partial struct IPv6Mreq {
    [FieldOffset(0)] public array16<byte> Multiaddr;
    [FieldOffset(16)] public uint32 Interface;
}

[global::go.golib.GoInlineLayout(Size = 16, Align = 4), StructLayout(LayoutKind.Explicit, Size = 16)] partial struct PacketMreq {
    [FieldOffset(0)] public int32 Ifindex;
    [FieldOffset(4)] public uint16 Type;
    [FieldOffset(6)] public uint16 Alen;
    [FieldOffset(8)] public array8<uint8> Address;
}

[global::go.golib.GoInlineLayout(Size = 12, Align = 4), StructLayout(LayoutKind.Explicit, Size = 12)] partial struct Inet4Pktinfo {
    [FieldOffset(0)] public int32 Ifindex;
    [FieldOffset(4)] public array4<byte> Spec_dst;
    [FieldOffset(8)] public array4<byte> Addr;
}

[global::go.golib.GoInlineLayout(Size = 20, Align = 4), StructLayout(LayoutKind.Explicit, Size = 20)] partial struct Inet6Pktinfo {
    [FieldOffset(0)] public array16<byte> Addr;
    [FieldOffset(16)] public uint32 Ifindex;
}

partial struct IPv6MTUInfo {
    public RawSockaddrInet6 Addr;
    public uint32 Mtu;
}

[global::go.golib.GoInlineLayout(Size = 32, Align = 4), StructLayout(LayoutKind.Explicit, Size = 32)] partial struct ICMPv6Filter {
    [FieldOffset(0)] public array8<uint32> Data;
}

partial struct Ucred {
    public int32 Pid;
    public uint32 Uid;
    public uint32 Gid;
}

[global::go.golib.GoInlineLayout(Size = 280, Align = 8), StructLayout(LayoutKind.Explicit, Size = 280)] partial struct TCPInfo {
    [FieldOffset(0)] public uint8 State;
    [FieldOffset(1)] public uint8 Ca_state;
    [FieldOffset(2)] public uint8 Retransmits;
    [FieldOffset(3)] public uint8 Probes;
    [FieldOffset(4)] public uint8 Backoff;
    [FieldOffset(5)] public uint8 Options;
    [FieldOffset(8)] public uint32 Rto;
    [FieldOffset(12)] public uint32 Ato;
    [FieldOffset(16)] public uint32 Snd_mss;
    [FieldOffset(20)] public uint32 Rcv_mss;
    [FieldOffset(24)] public uint32 Unacked;
    [FieldOffset(28)] public uint32 Sacked;
    [FieldOffset(32)] public uint32 Lost;
    [FieldOffset(36)] public uint32 Retrans;
    [FieldOffset(40)] public uint32 Fackets;
    [FieldOffset(44)] public uint32 Last_data_sent;
    [FieldOffset(48)] public uint32 Last_ack_sent;
    [FieldOffset(52)] public uint32 Last_data_recv;
    [FieldOffset(56)] public uint32 Last_ack_recv;
    [FieldOffset(60)] public uint32 Pmtu;
    [FieldOffset(64)] public uint32 Rcv_ssthresh;
    [FieldOffset(68)] public uint32 Rtt;
    [FieldOffset(72)] public uint32 Rttvar;
    [FieldOffset(76)] public uint32 Snd_ssthresh;
    [FieldOffset(80)] public uint32 Snd_cwnd;
    [FieldOffset(84)] public uint32 Advmss;
    [FieldOffset(88)] public uint32 Reordering;
    [FieldOffset(92)] public uint32 Rcv_rtt;
    [FieldOffset(96)] public uint32 Rcv_space;
    [FieldOffset(100)] public uint32 Total_retrans;
    [FieldOffset(104)] public uint64 Pacing_rate;
    [FieldOffset(112)] public uint64 Max_pacing_rate;
    [FieldOffset(120)] public uint64 Bytes_acked;
    [FieldOffset(128)] public uint64 Bytes_received;
    [FieldOffset(136)] public uint32 Segs_out;
    [FieldOffset(140)] public uint32 Segs_in;
    [FieldOffset(144)] public uint32 Notsent_bytes;
    [FieldOffset(148)] public uint32 Min_rtt;
    [FieldOffset(152)] public uint32 Data_segs_in;
    [FieldOffset(156)] public uint32 Data_segs_out;
    [FieldOffset(160)] public uint64 Delivery_rate;
    [FieldOffset(168)] public uint64 Busy_time;
    [FieldOffset(176)] public uint64 Rwnd_limited;
    [FieldOffset(184)] public uint64 Sndbuf_limited;
    [FieldOffset(192)] public uint32 Delivered;
    [FieldOffset(196)] public uint32 Delivered_ce;
    [FieldOffset(200)] public uint64 Bytes_sent;
    [FieldOffset(208)] public uint64 Bytes_retrans;
    [FieldOffset(216)] public uint32 Dsack_dups;
    [FieldOffset(220)] public uint32 Reord_seen;
    [FieldOffset(224)] public uint32 Rcv_ooopack;
    [FieldOffset(228)] public uint32 Snd_wnd;
    [FieldOffset(232)] public uint32 Rcv_wnd;
    [FieldOffset(236)] public uint32 Rehash;
    [FieldOffset(240)] public uint16 Total_rto;
    [FieldOffset(242)] public uint16 Total_rto_recoveries;
    [FieldOffset(244)] public uint32 Total_rto_time;
    [FieldOffset(248)] public uint32 Received_ce;
    [FieldOffset(252)] public uint32 Delivered_e1_bytes;
    [FieldOffset(256)] public uint32 Delivered_e0_bytes;
    [FieldOffset(260)] public uint32 Delivered_ce_bytes;
    [FieldOffset(264)] public uint32 Received_e1_bytes;
    [FieldOffset(268)] public uint32 Received_e0_bytes;
    [FieldOffset(272)] public uint32 Received_ce_bytes;
    [FieldOffset(276)] internal array4<byte> _;
}

partial struct TCPVegasInfo {
    public uint32 Enabled;
    public uint32 Rttcnt;
    public uint32 Rtt;
    public uint32 Minrtt;
}

partial struct TCPDCTCPInfo {
    public uint16 Enabled;
    public uint16 Ce_state;
    public uint32 Alpha;
    public uint32 Ab_ecn;
    public uint32 Ab_tot;
}

partial struct TCPBBRInfo {
    public uint32 Bw_lo;
    public uint32 Bw_hi;
    public uint32 Min_rtt;
    public uint32 Pacing_gain;
    public uint32 Cwnd_gain;
}

partial struct CanFilter {
    public uint32 Id;
    public uint32 Mask;
}

partial struct TCPRepairOpt {
    public uint32 Code;
    public uint32 Val;
}

public static UntypedInt SizeofSockaddrInet4 => 0x10;
public static UntypedInt SizeofSockaddrInet6 => 0x1c;
public static UntypedInt SizeofSockaddrAny => 0x70;
public static UntypedInt SizeofSockaddrUnix => 0x6e;
public static UntypedInt SizeofSockaddrLinklayer => 0x14;
public static UntypedInt SizeofSockaddrNetlink => 0xc;
public static UntypedInt SizeofSockaddrHCI => 0x6;
public static UntypedInt SizeofSockaddrL2 => 0xe;
public static UntypedInt SizeofSockaddrRFCOMM => 0xa;
public static UntypedInt SizeofSockaddrCAN => 0x18;
public static UntypedInt SizeofSockaddrALG => 0x58;
public static UntypedInt SizeofSockaddrVM => 0x10;
public static UntypedInt SizeofSockaddrXDP => 0x10;
public static UntypedInt SizeofSockaddrPPPoX => 0x1e;
public static UntypedInt SizeofSockaddrTIPC => 0x10;
public static UntypedInt SizeofSockaddrL2TPIP => 0x10;
public static UntypedInt SizeofSockaddrL2TPIP6 => 0x20;
public static UntypedInt SizeofSockaddrIUCV => 0x20;
public static UntypedInt SizeofSockaddrNFC => 0x10;
public static UntypedInt SizeofLinger => 0x8;
public static UntypedInt SizeofIPMreq => 0x8;
public static UntypedInt SizeofIPMreqn => 0xc;
public static UntypedInt SizeofIPv6Mreq => 0x14;
public static UntypedInt SizeofPacketMreq => 0x10;
public static UntypedInt SizeofInet4Pktinfo => 0xc;
public static UntypedInt SizeofInet6Pktinfo => 0x14;
public static UntypedInt SizeofIPv6MTUInfo => 0x20;
public static UntypedInt SizeofICMPv6Filter => 0x20;
public static UntypedInt SizeofUcred => 0xc;
public static UntypedInt SizeofTCPInfo => 0x118;
public static UntypedInt SizeofTCPCCInfo => 0x14;
public static UntypedInt SizeofCanFilter => 0x8;
public static UntypedInt SizeofTCPRepairOpt => 0x8;

public static UntypedInt NDA_UNSPEC => 0x0;
public static UntypedInt NDA_DST => 0x1;
public static UntypedInt NDA_LLADDR => 0x2;
public static UntypedInt NDA_CACHEINFO => 0x3;
public static UntypedInt NDA_PROBES => 0x4;
public static UntypedInt NDA_VLAN => 0x5;
public static UntypedInt NDA_PORT => 0x6;
public static UntypedInt NDA_VNI => 0x7;
public static UntypedInt NDA_IFINDEX => 0x8;
public static UntypedInt NDA_MASTER => 0x9;
public static UntypedInt NDA_LINK_NETNSID => 0xa;
public static UntypedInt NDA_SRC_VNI => 0xb;
public static UntypedInt NTF_USE => 0x1;
public static UntypedInt NTF_SELF => 0x2;
public static UntypedInt NTF_MASTER => 0x4;
public static UntypedInt NTF_PROXY => 0x8;
public static UntypedInt NTF_EXT_LEARNED => 0x10;
public static UntypedInt NTF_OFFLOADED => 0x20;
public static UntypedInt NTF_ROUTER => 0x80;
public static UntypedInt NUD_INCOMPLETE => 0x1;
public static UntypedInt NUD_REACHABLE => 0x2;
public static UntypedInt NUD_STALE => 0x4;
public static UntypedInt NUD_DELAY => 0x8;
public static UntypedInt NUD_PROBE => 0x10;
public static UntypedInt NUD_FAILED => 0x20;
public static UntypedInt NUD_NOARP => 0x40;
public static UntypedInt NUD_PERMANENT => 0x80;
public static UntypedInt NUD_NONE => 0x0;
public static UntypedInt IFA_UNSPEC => 0x0;
public static UntypedInt IFA_ADDRESS => 0x1;
public static UntypedInt IFA_LOCAL => 0x2;
public static UntypedInt IFA_LABEL => 0x3;
public static UntypedInt IFA_BROADCAST => 0x4;
public static UntypedInt IFA_ANYCAST => 0x5;
public static UntypedInt IFA_CACHEINFO => 0x6;
public static UntypedInt IFA_MULTICAST => 0x7;
public static UntypedInt IFA_FLAGS => 0x8;
public static UntypedInt IFA_RT_PRIORITY => 0x9;
public static UntypedInt IFA_TARGET_NETNSID => 0xa;
public static UntypedInt IFAL_LABEL => 0x2;
public static UntypedInt IFAL_ADDRESS => 0x1;
public static UntypedInt RT_SCOPE_UNIVERSE => 0x0;
public static UntypedInt RT_SCOPE_SITE => 0xc8;
public static UntypedInt RT_SCOPE_LINK => 0xfd;
public static UntypedInt RT_SCOPE_HOST => 0xfe;
public static UntypedInt RT_SCOPE_NOWHERE => 0xff;
public static UntypedInt RT_TABLE_UNSPEC => 0x0;
public static UntypedInt RT_TABLE_COMPAT => 0xfc;
public static UntypedInt RT_TABLE_DEFAULT => 0xfd;
public static UntypedInt RT_TABLE_MAIN => 0xfe;
public static UntypedInt RT_TABLE_LOCAL => 0xff;
public static UntypedInt RT_TABLE_MAX => 0xffffffff;
public static UntypedInt RTA_UNSPEC => 0x0;
public static UntypedInt RTA_DST => 0x1;
public static UntypedInt RTA_SRC => 0x2;
public static UntypedInt RTA_IIF => 0x3;
public static UntypedInt RTA_OIF => 0x4;
public static UntypedInt RTA_GATEWAY => 0x5;
public static UntypedInt RTA_PRIORITY => 0x6;
public static UntypedInt RTA_PREFSRC => 0x7;
public static UntypedInt RTA_METRICS => 0x8;
public static UntypedInt RTA_MULTIPATH => 0x9;
public static UntypedInt RTA_FLOW => 0xb;
public static UntypedInt RTA_CACHEINFO => 0xc;
public static UntypedInt RTA_TABLE => 0xf;
public static UntypedInt RTA_MARK => 0x10;
public static UntypedInt RTA_MFC_STATS => 0x11;
public static UntypedInt RTA_VIA => 0x12;
public static UntypedInt RTA_NEWDST => 0x13;
public static UntypedInt RTA_PREF => 0x14;
public static UntypedInt RTA_ENCAP_TYPE => 0x15;
public static UntypedInt RTA_ENCAP => 0x16;
public static UntypedInt RTA_EXPIRES => 0x17;
public static UntypedInt RTA_PAD => 0x18;
public static UntypedInt RTA_UID => 0x19;
public static UntypedInt RTA_TTL_PROPAGATE => 0x1a;
public static UntypedInt RTA_IP_PROTO => 0x1b;
public static UntypedInt RTA_SPORT => 0x1c;
public static UntypedInt RTA_DPORT => 0x1d;
public static UntypedInt RTN_UNSPEC => 0x0;
public static UntypedInt RTN_UNICAST => 0x1;
public static UntypedInt RTN_LOCAL => 0x2;
public static UntypedInt RTN_BROADCAST => 0x3;
public static UntypedInt RTN_ANYCAST => 0x4;
public static UntypedInt RTN_MULTICAST => 0x5;
public static UntypedInt RTN_BLACKHOLE => 0x6;
public static UntypedInt RTN_UNREACHABLE => 0x7;
public static UntypedInt RTN_PROHIBIT => 0x8;
public static UntypedInt RTN_THROW => 0x9;
public static UntypedInt RTN_NAT => 0xa;
public static UntypedInt RTN_XRESOLVE => 0xb;
public static UntypedInt PREFIX_UNSPEC => 0x0;
public static UntypedInt PREFIX_ADDRESS => 0x1;
public static UntypedInt PREFIX_CACHEINFO => 0x2;
public static UntypedInt SizeofNlMsghdr => 0x10;
public static UntypedInt SizeofNlMsgerr => 0x14;
public static UntypedInt SizeofRtGenmsg => 0x1;
public static UntypedInt SizeofNlAttr => 0x4;
public static UntypedInt SizeofRtAttr => 0x4;
public static UntypedInt SizeofIfInfomsg => 0x10;
public static UntypedInt SizeofPrefixmsg => 0xc;
public static UntypedInt SizeofPrefixCacheinfo => 0x8;
public static UntypedInt SizeofIfAddrmsg => 0x8;
public static UntypedInt SizeofIfAddrlblmsg => 0xc;
public static UntypedInt SizeofIfaCacheinfo => 0x10;
public static UntypedInt SizeofRtMsg => 0xc;
public static UntypedInt SizeofRtNexthop => 0x8;
public static UntypedInt SizeofNdUseroptmsg => 0x10;
public static UntypedInt SizeofNdMsg => 0xc;

partial struct NlMsghdr {
    public uint32 Len;
    public uint16 Type;
    public uint16 Flags;
    public uint32 Seq;
    public uint32 Pid;
}

partial struct NlMsgerr {
    public int32 Error;
    public NlMsghdr Msg;
}

partial struct RtGenmsg {
    public uint8 Family;
}

partial struct NlAttr {
    public uint16 Len;
    public uint16 Type;
}

partial struct RtAttr {
    public uint16 Len;
    public uint16 Type;
}

partial struct IfInfomsg {
    public uint8 Family;
    internal uint8 _;
    public uint16 Type;
    public int32 Index;
    public uint32 Flags;
    public uint32 Change;
}

partial struct Prefixmsg {
    public uint8 Family;
    public uint8 Pad1;
    public uint16 Pad2;
    public int32 Ifindex;
    public uint8 Type;
    public uint8 Len;
    public uint8 Flags;
    public uint8 Pad3;
}

partial struct PrefixCacheinfo {
    public uint32 Preferred_time;
    public uint32 Valid_time;
}

partial struct IfAddrmsg {
    public uint8 Family;
    public uint8 Prefixlen;
    public uint8 Flags;
    public uint8 Scope;
    public uint32 Index;
}

partial struct IfAddrlblmsg {
    public uint8 Family;
    internal uint8 _;
    public uint8 Prefixlen;
    public uint8 Flags;
    public uint32 Index;
    public uint32 Seq;
}

partial struct IfaCacheinfo {
    public uint32 Prefered;
    public uint32 Valid;
    public uint32 Cstamp;
    public uint32 Tstamp;
}

partial struct RtMsg {
    public uint8 Family;
    public uint8 Dst_len;
    public uint8 Src_len;
    public uint8 Tos;
    public uint8 Table;
    public uint8 Protocol;
    public uint8 Scope;
    public uint8 Type;
    public uint32 Flags;
}

partial struct RtNexthop {
    public uint16 Len;
    public uint8 Flags;
    public uint8 Hops;
    public int32 Ifindex;
}

partial struct NdUseroptmsg {
    public uint8 Family;
    public uint8 Pad1;
    public uint16 Opts_len;
    public int32 Ifindex;
    public uint8 Icmp_type;
    public uint8 Icmp_code;
    public uint16 Pad2;
    public uint32 Pad3;
}

partial struct NdMsg {
    public uint8 Family;
    public uint8 Pad1;
    public uint16 Pad2;
    public int32 Ifindex;
    public uint16 State;
    public uint8 Flags;
    public uint8 Type;
}

public static UntypedInt ICMP_FILTER => 0x1;
public static UntypedInt ICMPV6_FILTER => 0x1;
public static UntypedInt ICMPV6_FILTER_BLOCK => 0x1;
public static UntypedInt ICMPV6_FILTER_BLOCKOTHERS => 0x3;
public static UntypedInt ICMPV6_FILTER_PASS => 0x2;
public static UntypedInt ICMPV6_FILTER_PASSONLY => 0x4;

public static UntypedInt SizeofSockFilter => 0x8;

partial struct SockFilter {
    public uint16 Code;
    public uint8 Jt;
    public uint8 Jf;
    public uint32 K;
}

partial struct SockFprog {
    public uint16 Len;
    public ж<SockFilter> Filter;
}

partial struct InotifyEvent {
    public int32 Wd;
    public uint32 Mask;
    public uint32 Cookie;
    public uint32 Len;
}

public static UntypedInt SizeofInotifyEvent => 0x10;

public static UntypedInt SI_LOAD_SHIFT => 0x10;

[global::go.golib.GoInlineLayout(Size = 390, Align = 1), StructLayout(LayoutKind.Explicit, Size = 390)] partial struct Utsname {
    [FieldOffset(0)] public array65<byte> Sysname;
    [FieldOffset(65)] public array65<byte> Nodename;
    [FieldOffset(130)] public array65<byte> Release;
    [FieldOffset(195)] public array65<byte> Version;
    [FieldOffset(260)] public array65<byte> Machine;
    [FieldOffset(325)] public array65<byte> Domainname;
}

public static UntypedInt AT_EMPTY_PATH => 0x1000;
public static UntypedInt AT_FDCWD => /* -0x64 */ -100;
public static UntypedInt AT_NO_AUTOMOUNT => 0x800;
public static UntypedInt AT_REMOVEDIR => 0x200;
public static UntypedInt AT_STATX_SYNC_AS_STAT => 0x0;
public static UntypedInt AT_STATX_FORCE_SYNC => 0x2000;
public static UntypedInt AT_STATX_DONT_SYNC => 0x4000;
public static UntypedInt AT_RECURSIVE => 0x8000;
public static UntypedInt AT_SYMLINK_FOLLOW => 0x400;
public static UntypedInt AT_SYMLINK_NOFOLLOW => 0x100;
public static UntypedInt AT_EACCESS => 0x200;
public static UntypedInt OPEN_TREE_CLONE => 0x1;
public static UntypedInt MOVE_MOUNT_F_SYMLINKS => 0x1;
public static UntypedInt MOVE_MOUNT_F_AUTOMOUNTS => 0x2;
public static UntypedInt MOVE_MOUNT_F_EMPTY_PATH => 0x4;
public static UntypedInt MOVE_MOUNT_T_SYMLINKS => 0x10;
public static UntypedInt MOVE_MOUNT_T_AUTOMOUNTS => 0x20;
public static UntypedInt MOVE_MOUNT_T_EMPTY_PATH => 0x40;
public static UntypedInt MOVE_MOUNT_SET_GROUP => 0x100;
public static UntypedInt FSOPEN_CLOEXEC => 0x1;
public static UntypedInt FSPICK_CLOEXEC => 0x1;
public static UntypedInt FSPICK_SYMLINK_NOFOLLOW => 0x2;
public static UntypedInt FSPICK_NO_AUTOMOUNT => 0x4;
public static UntypedInt FSPICK_EMPTY_PATH => 0x8;
public static UntypedInt FSMOUNT_CLOEXEC => 0x1;
public static UntypedInt FSCONFIG_SET_FLAG => 0x0;
public static UntypedInt FSCONFIG_SET_STRING => 0x1;
public static UntypedInt FSCONFIG_SET_BINARY => 0x2;
public static UntypedInt FSCONFIG_SET_PATH => 0x3;
public static UntypedInt FSCONFIG_SET_PATH_EMPTY => 0x4;
public static UntypedInt FSCONFIG_SET_FD => 0x5;
public static UntypedInt FSCONFIG_CMD_CREATE => 0x6;
public static UntypedInt FSCONFIG_CMD_RECONFIGURE => 0x7;

partial struct OpenHow {
    public uint64 Flags;
    public uint64 Mode;
    public uint64 Resolve;
}

public static UntypedInt SizeofOpenHow => 0x18;

partial struct NsIdReq {
    public uint32 Size;
    public uint32 Spare;
    public uint64 Ns_id;
    public uint32 Ns_type;
    public uint32 Spare2;
    public uint64 User_ns_id;
}

public static UntypedInt NS_ID_REQ_SIZE_VER0 => 0x20;
public static UntypedInt LISTNS_CURRENT_USER => 0xffffffffffffffff;
public static UntypedInt TIME_NS => 0x80;
public static UntypedInt MNT_NS => 0x20000;
public static UntypedInt CGROUP_NS => 0x2000000;
public static UntypedInt UTS_NS => 0x4000000;
public static UntypedInt IPC_NS => 0x8000000;
public static UntypedInt USER_NS => 0x10000000;
public static UntypedInt PID_NS => 0x20000000;
public static UntypedInt NET_NS => 0x40000000;

public static UntypedInt RESOLVE_BENEATH => 0x8;
public static UntypedInt RESOLVE_IN_ROOT => 0x10;
public static UntypedInt RESOLVE_NO_MAGICLINKS => 0x2;
public static UntypedInt RESOLVE_NO_SYMLINKS => 0x4;
public static UntypedInt RESOLVE_NO_XDEV => 0x1;

partial struct PollFd {
    public int32 Fd;
    public int16 Events;
    public int16 Revents;
}

public static UntypedInt POLLIN => 0x1;
public static UntypedInt POLLPRI => 0x2;
public static UntypedInt POLLOUT => 0x4;
public static UntypedInt POLLERR => 0x8;
public static UntypedInt POLLHUP => 0x10;
public static UntypedInt POLLNVAL => 0x20;

partial struct sigset_argpack {
    internal ж<Sigset_t> ss;
    internal uintptr ssLen;
}

partial struct SignalfdSiginfo {
    public uint32 Signo;
    public int32 Errno;
    public int32 Code;
    public uint32 Pid;
    public uint32 Uid;
    public int32 Fd;
    public uint32 Tid;
    public uint32 Band;
    public uint32 Overrun;
    public uint32 Trapno;
    public int32 Status;
    public int32 Int;
    public uint64 Ptr;
    public uint64 Utime;
    public uint64 Stime;
    public uint64 Addr;
    public uint16 Addr_lsb;
    internal uint16 _;
    public int32 Syscall;
    public uint64 Call_addr;
    public uint32 Arch;
    internal array<uint8> __ = new(28);
}

partial struct Winsize {
    public uint16 Row;
    public uint16 Col;
    public uint16 Xpixel;
    public uint16 Ypixel;
}

public static UntypedInt TASKSTATS_CMD_UNSPEC => 0x0;
public static UntypedInt TASKSTATS_CMD_GET => 0x1;
public static UntypedInt TASKSTATS_CMD_NEW => 0x2;
public static UntypedInt TASKSTATS_TYPE_UNSPEC => 0x0;
public static UntypedInt TASKSTATS_TYPE_PID => 0x1;
public static UntypedInt TASKSTATS_TYPE_TGID => 0x2;
public static UntypedInt TASKSTATS_TYPE_STATS => 0x3;
public static UntypedInt TASKSTATS_TYPE_AGGR_PID => 0x4;
public static UntypedInt TASKSTATS_TYPE_AGGR_TGID => 0x5;
public static UntypedInt TASKSTATS_TYPE_NULL => 0x6;
public static UntypedInt TASKSTATS_CMD_ATTR_UNSPEC => 0x0;
public static UntypedInt TASKSTATS_CMD_ATTR_PID => 0x1;
public static UntypedInt TASKSTATS_CMD_ATTR_TGID => 0x2;
public static UntypedInt TASKSTATS_CMD_ATTR_REGISTER_CPUMASK => 0x3;
public static UntypedInt TASKSTATS_CMD_ATTR_DEREGISTER_CPUMASK => 0x4;

partial struct CGroupStats {
    public uint64 Sleeping;
    public uint64 Running;
    public uint64 Stopped;
    public uint64 Uninterruptible;
    public uint64 Io_wait;
}

public static UntypedInt CGROUPSTATS_CMD_UNSPEC => 0x3;
public static UntypedInt CGROUPSTATS_CMD_GET => 0x4;
public static UntypedInt CGROUPSTATS_CMD_NEW => 0x5;
public static UntypedInt CGROUPSTATS_TYPE_UNSPEC => 0x0;
public static UntypedInt CGROUPSTATS_TYPE_CGROUP_STATS => 0x1;
public static UntypedInt CGROUPSTATS_CMD_ATTR_UNSPEC => 0x0;
public static UntypedInt CGROUPSTATS_CMD_ATTR_FD => 0x1;

partial struct Genlmsghdr {
    public uint8 Cmd;
    public uint8 Version;
    public uint16 Reserved;
}

public static UntypedInt CTRL_CMD_UNSPEC => 0x0;
public static UntypedInt CTRL_CMD_NEWFAMILY => 0x1;
public static UntypedInt CTRL_CMD_DELFAMILY => 0x2;
public static UntypedInt CTRL_CMD_GETFAMILY => 0x3;
public static UntypedInt CTRL_CMD_NEWOPS => 0x4;
public static UntypedInt CTRL_CMD_DELOPS => 0x5;
public static UntypedInt CTRL_CMD_GETOPS => 0x6;
public static UntypedInt CTRL_CMD_NEWMCAST_GRP => 0x7;
public static UntypedInt CTRL_CMD_DELMCAST_GRP => 0x8;
public static UntypedInt CTRL_CMD_GETMCAST_GRP => 0x9;
public static UntypedInt CTRL_CMD_GETPOLICY => 0xa;
public static UntypedInt CTRL_ATTR_UNSPEC => 0x0;
public static UntypedInt CTRL_ATTR_FAMILY_ID => 0x1;
public static UntypedInt CTRL_ATTR_FAMILY_NAME => 0x2;
public static UntypedInt CTRL_ATTR_VERSION => 0x3;
public static UntypedInt CTRL_ATTR_HDRSIZE => 0x4;
public static UntypedInt CTRL_ATTR_MAXATTR => 0x5;
public static UntypedInt CTRL_ATTR_OPS => 0x6;
public static UntypedInt CTRL_ATTR_MCAST_GROUPS => 0x7;
public static UntypedInt CTRL_ATTR_POLICY => 0x8;
public static UntypedInt CTRL_ATTR_OP_POLICY => 0x9;
public static UntypedInt CTRL_ATTR_OP => 0xa;
public static UntypedInt CTRL_ATTR_OP_UNSPEC => 0x0;
public static UntypedInt CTRL_ATTR_OP_ID => 0x1;
public static UntypedInt CTRL_ATTR_OP_FLAGS => 0x2;
public static UntypedInt CTRL_ATTR_MCAST_GRP_UNSPEC => 0x0;
public static UntypedInt CTRL_ATTR_MCAST_GRP_NAME => 0x1;
public static UntypedInt CTRL_ATTR_MCAST_GRP_ID => 0x2;
public static UntypedInt CTRL_ATTR_POLICY_UNSPEC => 0x0;
public static UntypedInt CTRL_ATTR_POLICY_DO => 0x1;
public static UntypedInt CTRL_ATTR_POLICY_DUMP => 0x2;
public static UntypedInt CTRL_ATTR_POLICY_DUMP_MAX => 0x2;

internal static UntypedInt _CPU_SETSIZE => 0x400;

public static UntypedInt BDADDR_BREDR => 0x0;
public static UntypedInt BDADDR_LE_PUBLIC => 0x1;
public static UntypedInt BDADDR_LE_RANDOM => 0x2;

partial struct PerfEventAttr {
    public uint32 Type;
    public uint32 Size;
    public uint64 Config;
    public uint64 Sample;
    public uint64 Sample_type;
    public uint64 Read_format;
    public uint64 Bits;
    public uint32 Wakeup;
    public uint32 Bp_type;
    public uint64 Ext1;
    public uint64 Ext2;
    public uint64 Branch_sample_type;
    public uint64 Sample_regs_user;
    public uint32 Sample_stack_user;
    public int32 Clockid;
    public uint64 Sample_regs_intr;
    public uint32 Aux_watermark;
    public uint16 Sample_max_stack;
    internal uint16 _;
    public uint32 Aux_sample_size;
    internal uint32 __;
    public uint64 Sig_data;
}

partial struct PerfEventMmapPage {
    public uint32 Version;
    public uint32 Compat_version;
    public uint32 Lock;
    public uint32 Index;
    public int64 Offset;
    public uint64 Time_enabled;
    public uint64 Time_running;
    public uint64 Capabilities;
    public uint16 Pmc_width;
    public uint16 Time_shift;
    public uint32 Time_mult;
    public uint64 Time_offset;
    public uint64 Time_zero;
    public uint32 Size;
    internal uint32 _;
    public uint64 Time_cycles;
    public uint64 Time_mask;
    internal array<uint8> __ = new(928);
    public uint64 Data_head;
    public uint64 Data_tail;
    public uint64 Data_offset;
    public uint64 Data_size;
    public uint64 Aux_head;
    public uint64 Aux_tail;
    public uint64 Aux_offset;
    public uint64 Aux_size;
}

public const uint64 PerfBitDisabled = /* CBitFieldMaskBit0 */ 1;
public static UntypedInt PerfBitInherit => /* CBitFieldMaskBit1 */ 2;
public static UntypedInt PerfBitPinned => /* CBitFieldMaskBit2 */ 4;
public static UntypedInt PerfBitExclusive => /* CBitFieldMaskBit3 */ 8;
public static UntypedInt PerfBitExcludeUser => /* CBitFieldMaskBit4 */ 16;
public static UntypedInt PerfBitExcludeKernel => /* CBitFieldMaskBit5 */ 32;
public static UntypedInt PerfBitExcludeHv => /* CBitFieldMaskBit6 */ 64;
public static UntypedInt PerfBitExcludeIdle => /* CBitFieldMaskBit7 */ 128;
public static UntypedInt PerfBitMmap => /* CBitFieldMaskBit8 */ 256;
public static UntypedInt PerfBitComm => /* CBitFieldMaskBit9 */ 512;
public static UntypedInt PerfBitFreq => /* CBitFieldMaskBit10 */ 1024;
public static UntypedInt PerfBitInheritStat => /* CBitFieldMaskBit11 */ 2048;
public static UntypedInt PerfBitEnableOnExec => /* CBitFieldMaskBit12 */ 4096;
public static UntypedInt PerfBitTask => /* CBitFieldMaskBit13 */ 8192;
public static UntypedInt PerfBitWatermark => /* CBitFieldMaskBit14 */ 16384;
public static UntypedInt PerfBitPreciseIPBit1 => /* CBitFieldMaskBit15 */ 32768;
public static UntypedInt PerfBitPreciseIPBit2 => /* CBitFieldMaskBit16 */ 65536;
public static UntypedInt PerfBitMmapData => /* CBitFieldMaskBit17 */ 131072;
public static UntypedInt PerfBitSampleIDAll => /* CBitFieldMaskBit18 */ 262144;
public static UntypedInt PerfBitExcludeHost => /* CBitFieldMaskBit19 */ 524288;
public static UntypedInt PerfBitExcludeGuest => /* CBitFieldMaskBit20 */ 1048576;
public static UntypedInt PerfBitExcludeCallchainKernel => /* CBitFieldMaskBit21 */ 2097152;
public static UntypedInt PerfBitExcludeCallchainUser => /* CBitFieldMaskBit22 */ 4194304;
public static UntypedInt PerfBitMmap2 => /* CBitFieldMaskBit23 */ 8388608;
public static UntypedInt PerfBitCommExec => /* CBitFieldMaskBit24 */ 16777216;
public static UntypedInt PerfBitUseClockID => /* CBitFieldMaskBit25 */ 33554432;
public static UntypedInt PerfBitContextSwitch => /* CBitFieldMaskBit26 */ 67108864;
public static UntypedInt PerfBitWriteBackward => /* CBitFieldMaskBit27 */ 134217728;

public static UntypedInt PERF_TYPE_HARDWARE => 0x0;
public static UntypedInt PERF_TYPE_SOFTWARE => 0x1;
public static UntypedInt PERF_TYPE_TRACEPOINT => 0x2;
public static UntypedInt PERF_TYPE_HW_CACHE => 0x3;
public static UntypedInt PERF_TYPE_RAW => 0x4;
public static UntypedInt PERF_TYPE_BREAKPOINT => 0x5;
public static UntypedInt PERF_TYPE_MAX => 0x6;
public static UntypedInt PERF_COUNT_HW_CPU_CYCLES => 0x0;
public static UntypedInt PERF_COUNT_HW_INSTRUCTIONS => 0x1;
public static UntypedInt PERF_COUNT_HW_CACHE_REFERENCES => 0x2;
public static UntypedInt PERF_COUNT_HW_CACHE_MISSES => 0x3;
public static UntypedInt PERF_COUNT_HW_BRANCH_INSTRUCTIONS => 0x4;
public static UntypedInt PERF_COUNT_HW_BRANCH_MISSES => 0x5;
public static UntypedInt PERF_COUNT_HW_BUS_CYCLES => 0x6;
public static UntypedInt PERF_COUNT_HW_STALLED_CYCLES_FRONTEND => 0x7;
public static UntypedInt PERF_COUNT_HW_STALLED_CYCLES_BACKEND => 0x8;
public static UntypedInt PERF_COUNT_HW_REF_CPU_CYCLES => 0x9;
public static UntypedInt PERF_COUNT_HW_MAX => 0xa;
public static UntypedInt PERF_COUNT_HW_CACHE_L1D => 0x0;
public static UntypedInt PERF_COUNT_HW_CACHE_L1I => 0x1;
public static UntypedInt PERF_COUNT_HW_CACHE_LL => 0x2;
public static UntypedInt PERF_COUNT_HW_CACHE_DTLB => 0x3;
public static UntypedInt PERF_COUNT_HW_CACHE_ITLB => 0x4;
public static UntypedInt PERF_COUNT_HW_CACHE_BPU => 0x5;
public static UntypedInt PERF_COUNT_HW_CACHE_NODE => 0x6;
public static UntypedInt PERF_COUNT_HW_CACHE_MAX => 0x7;
public static UntypedInt PERF_COUNT_HW_CACHE_OP_READ => 0x0;
public static UntypedInt PERF_COUNT_HW_CACHE_OP_WRITE => 0x1;
public static UntypedInt PERF_COUNT_HW_CACHE_OP_PREFETCH => 0x2;
public static UntypedInt PERF_COUNT_HW_CACHE_OP_MAX => 0x3;
public static UntypedInt PERF_COUNT_HW_CACHE_RESULT_ACCESS => 0x0;
public static UntypedInt PERF_COUNT_HW_CACHE_RESULT_MISS => 0x1;
public static UntypedInt PERF_COUNT_HW_CACHE_RESULT_MAX => 0x2;
public static UntypedInt PERF_COUNT_SW_CPU_CLOCK => 0x0;
public static UntypedInt PERF_COUNT_SW_TASK_CLOCK => 0x1;
public static UntypedInt PERF_COUNT_SW_PAGE_FAULTS => 0x2;
public static UntypedInt PERF_COUNT_SW_CONTEXT_SWITCHES => 0x3;
public static UntypedInt PERF_COUNT_SW_CPU_MIGRATIONS => 0x4;
public static UntypedInt PERF_COUNT_SW_PAGE_FAULTS_MIN => 0x5;
public static UntypedInt PERF_COUNT_SW_PAGE_FAULTS_MAJ => 0x6;
public static UntypedInt PERF_COUNT_SW_ALIGNMENT_FAULTS => 0x7;
public static UntypedInt PERF_COUNT_SW_EMULATION_FAULTS => 0x8;
public static UntypedInt PERF_COUNT_SW_DUMMY => 0x9;
public static UntypedInt PERF_COUNT_SW_BPF_OUTPUT => 0xa;
public static UntypedInt PERF_COUNT_SW_MAX => 0xc;
public static UntypedInt PERF_SAMPLE_IP => 0x1;
public static UntypedInt PERF_SAMPLE_TID => 0x2;
public static UntypedInt PERF_SAMPLE_TIME => 0x4;
public static UntypedInt PERF_SAMPLE_ADDR => 0x8;
public static UntypedInt PERF_SAMPLE_READ => 0x10;
public static UntypedInt PERF_SAMPLE_CALLCHAIN => 0x20;
public static UntypedInt PERF_SAMPLE_ID => 0x40;
public static UntypedInt PERF_SAMPLE_CPU => 0x80;
public static UntypedInt PERF_SAMPLE_PERIOD => 0x100;
public static UntypedInt PERF_SAMPLE_STREAM_ID => 0x200;
public static UntypedInt PERF_SAMPLE_RAW => 0x400;
public static UntypedInt PERF_SAMPLE_BRANCH_STACK => 0x800;
public static UntypedInt PERF_SAMPLE_REGS_USER => 0x1000;
public static UntypedInt PERF_SAMPLE_STACK_USER => 0x2000;
public static UntypedInt PERF_SAMPLE_WEIGHT => 0x4000;
public static UntypedInt PERF_SAMPLE_DATA_SRC => 0x8000;
public static UntypedInt PERF_SAMPLE_IDENTIFIER => 0x10000;
public static UntypedInt PERF_SAMPLE_TRANSACTION => 0x20000;
public static UntypedInt PERF_SAMPLE_REGS_INTR => 0x40000;
public static UntypedInt PERF_SAMPLE_PHYS_ADDR => 0x80000;
public static UntypedInt PERF_SAMPLE_AUX => 0x100000;
public static UntypedInt PERF_SAMPLE_CGROUP => 0x200000;
public static UntypedInt PERF_SAMPLE_DATA_PAGE_SIZE => 0x400000;
public static UntypedInt PERF_SAMPLE_CODE_PAGE_SIZE => 0x800000;
public static UntypedInt PERF_SAMPLE_WEIGHT_STRUCT => 0x1000000;
public static UntypedInt PERF_SAMPLE_MAX => 0x2000000;
public static UntypedInt PERF_SAMPLE_BRANCH_USER_SHIFT => 0x0;
public static UntypedInt PERF_SAMPLE_BRANCH_KERNEL_SHIFT => 0x1;
public static UntypedInt PERF_SAMPLE_BRANCH_HV_SHIFT => 0x2;
public static UntypedInt PERF_SAMPLE_BRANCH_ANY_SHIFT => 0x3;
public static UntypedInt PERF_SAMPLE_BRANCH_ANY_CALL_SHIFT => 0x4;
public static UntypedInt PERF_SAMPLE_BRANCH_ANY_RETURN_SHIFT => 0x5;
public static UntypedInt PERF_SAMPLE_BRANCH_IND_CALL_SHIFT => 0x6;
public static UntypedInt PERF_SAMPLE_BRANCH_ABORT_TX_SHIFT => 0x7;
public static UntypedInt PERF_SAMPLE_BRANCH_IN_TX_SHIFT => 0x8;
public static UntypedInt PERF_SAMPLE_BRANCH_NO_TX_SHIFT => 0x9;
public static UntypedInt PERF_SAMPLE_BRANCH_COND_SHIFT => 0xa;
public static UntypedInt PERF_SAMPLE_BRANCH_CALL_STACK_SHIFT => 0xb;
public static UntypedInt PERF_SAMPLE_BRANCH_IND_JUMP_SHIFT => 0xc;
public static UntypedInt PERF_SAMPLE_BRANCH_CALL_SHIFT => 0xd;
public static UntypedInt PERF_SAMPLE_BRANCH_NO_FLAGS_SHIFT => 0xe;
public static UntypedInt PERF_SAMPLE_BRANCH_NO_CYCLES_SHIFT => 0xf;
public static UntypedInt PERF_SAMPLE_BRANCH_TYPE_SAVE_SHIFT => 0x10;
public static UntypedInt PERF_SAMPLE_BRANCH_HW_INDEX_SHIFT => 0x11;
public static UntypedInt PERF_SAMPLE_BRANCH_PRIV_SAVE_SHIFT => 0x12;
public static UntypedInt PERF_SAMPLE_BRANCH_COUNTERS => 0x80000;
public static UntypedInt PERF_SAMPLE_BRANCH_MAX_SHIFT => 0x14;
public static UntypedInt PERF_SAMPLE_BRANCH_USER => 0x1;
public static UntypedInt PERF_SAMPLE_BRANCH_KERNEL => 0x2;
public static UntypedInt PERF_SAMPLE_BRANCH_HV => 0x4;
public static UntypedInt PERF_SAMPLE_BRANCH_ANY => 0x8;
public static UntypedInt PERF_SAMPLE_BRANCH_ANY_CALL => 0x10;
public static UntypedInt PERF_SAMPLE_BRANCH_ANY_RETURN => 0x20;
public static UntypedInt PERF_SAMPLE_BRANCH_IND_CALL => 0x40;
public static UntypedInt PERF_SAMPLE_BRANCH_ABORT_TX => 0x80;
public static UntypedInt PERF_SAMPLE_BRANCH_IN_TX => 0x100;
public static UntypedInt PERF_SAMPLE_BRANCH_NO_TX => 0x200;
public static UntypedInt PERF_SAMPLE_BRANCH_COND => 0x400;
public static UntypedInt PERF_SAMPLE_BRANCH_CALL_STACK => 0x800;
public static UntypedInt PERF_SAMPLE_BRANCH_IND_JUMP => 0x1000;
public static UntypedInt PERF_SAMPLE_BRANCH_CALL => 0x2000;
public static UntypedInt PERF_SAMPLE_BRANCH_NO_FLAGS => 0x4000;
public static UntypedInt PERF_SAMPLE_BRANCH_NO_CYCLES => 0x8000;
public static UntypedInt PERF_SAMPLE_BRANCH_TYPE_SAVE => 0x10000;
public static UntypedInt PERF_SAMPLE_BRANCH_HW_INDEX => 0x20000;
public static UntypedInt PERF_SAMPLE_BRANCH_PRIV_SAVE => 0x40000;
public static UntypedInt PERF_SAMPLE_BRANCH_MAX => 0x100000;
public static UntypedInt PERF_BR_UNKNOWN => 0x0;
public static UntypedInt PERF_BR_COND => 0x1;
public static UntypedInt PERF_BR_UNCOND => 0x2;
public static UntypedInt PERF_BR_IND => 0x3;
public static UntypedInt PERF_BR_CALL => 0x4;
public static UntypedInt PERF_BR_IND_CALL => 0x5;
public static UntypedInt PERF_BR_RET => 0x6;
public static UntypedInt PERF_BR_SYSCALL => 0x7;
public static UntypedInt PERF_BR_SYSRET => 0x8;
public static UntypedInt PERF_BR_COND_CALL => 0x9;
public static UntypedInt PERF_BR_COND_RET => 0xa;
public static UntypedInt PERF_BR_ERET => 0xb;
public static UntypedInt PERF_BR_IRQ => 0xc;
public static UntypedInt PERF_BR_SERROR => 0xd;
public static UntypedInt PERF_BR_NO_TX => 0xe;
public static UntypedInt PERF_BR_EXTEND_ABI => 0xf;
public static UntypedInt PERF_BR_MAX => 0x10;
public static UntypedInt PERF_SAMPLE_REGS_ABI_NONE => 0x0;
public static UntypedInt PERF_SAMPLE_REGS_ABI_32 => 0x1;
public static UntypedInt PERF_SAMPLE_REGS_ABI_64 => 0x2;
public static UntypedInt PERF_TXN_ELISION => 0x1;
public static UntypedInt PERF_TXN_TRANSACTION => 0x2;
public static UntypedInt PERF_TXN_SYNC => 0x4;
public static UntypedInt PERF_TXN_ASYNC => 0x8;
public static UntypedInt PERF_TXN_RETRY => 0x10;
public static UntypedInt PERF_TXN_CONFLICT => 0x20;
public static UntypedInt PERF_TXN_CAPACITY_WRITE => 0x40;
public static UntypedInt PERF_TXN_CAPACITY_READ => 0x80;
public static UntypedInt PERF_TXN_MAX => 0x100;
public static UntypedInt PERF_TXN_ABORT_MASK => /* -0x100000000 */ -4294967296;
public static UntypedInt PERF_TXN_ABORT_SHIFT => 0x20;
public static UntypedInt PERF_FORMAT_TOTAL_TIME_ENABLED => 0x1;
public static UntypedInt PERF_FORMAT_TOTAL_TIME_RUNNING => 0x2;
public static UntypedInt PERF_FORMAT_ID => 0x4;
public static UntypedInt PERF_FORMAT_GROUP => 0x8;
public static UntypedInt PERF_FORMAT_LOST => 0x10;
public static UntypedInt PERF_FORMAT_MAX => 0x20;
public static UntypedInt PERF_IOC_FLAG_GROUP => 0x1;
public static UntypedInt PERF_RECORD_MMAP => 0x1;
public static UntypedInt PERF_RECORD_LOST => 0x2;
public static UntypedInt PERF_RECORD_COMM => 0x3;
public static UntypedInt PERF_RECORD_EXIT => 0x4;
public static UntypedInt PERF_RECORD_THROTTLE => 0x5;
public static UntypedInt PERF_RECORD_UNTHROTTLE => 0x6;
public static UntypedInt PERF_RECORD_FORK => 0x7;
public static UntypedInt PERF_RECORD_READ => 0x8;
public static UntypedInt PERF_RECORD_SAMPLE => 0x9;
public static UntypedInt PERF_RECORD_MMAP2 => 0xa;
public static UntypedInt PERF_RECORD_AUX => 0xb;
public static UntypedInt PERF_RECORD_ITRACE_START => 0xc;
public static UntypedInt PERF_RECORD_LOST_SAMPLES => 0xd;
public static UntypedInt PERF_RECORD_SWITCH => 0xe;
public static UntypedInt PERF_RECORD_SWITCH_CPU_WIDE => 0xf;
public static UntypedInt PERF_RECORD_NAMESPACES => 0x10;
public static UntypedInt PERF_RECORD_KSYMBOL => 0x11;
public static UntypedInt PERF_RECORD_BPF_EVENT => 0x12;
public static UntypedInt PERF_RECORD_CGROUP => 0x13;
public static UntypedInt PERF_RECORD_TEXT_POKE => 0x14;
public static UntypedInt PERF_RECORD_AUX_OUTPUT_HW_ID => 0x15;
public static UntypedInt PERF_RECORD_MAX => 0x17;
public static UntypedInt PERF_RECORD_KSYMBOL_TYPE_UNKNOWN => 0x0;
public static UntypedInt PERF_RECORD_KSYMBOL_TYPE_BPF => 0x1;
public static UntypedInt PERF_RECORD_KSYMBOL_TYPE_OOL => 0x2;
public static UntypedInt PERF_RECORD_KSYMBOL_TYPE_MAX => 0x3;
public static UntypedInt PERF_BPF_EVENT_UNKNOWN => 0x0;
public static UntypedInt PERF_BPF_EVENT_PROG_LOAD => 0x1;
public static UntypedInt PERF_BPF_EVENT_PROG_UNLOAD => 0x2;
public static UntypedInt PERF_BPF_EVENT_MAX => 0x3;
public static UntypedInt PERF_CONTEXT_HV => /* -0x20 */ -32;
public static UntypedInt PERF_CONTEXT_KERNEL => /* -0x80 */ -128;
public static UntypedInt PERF_CONTEXT_USER => /* -0x200 */ -512;
public static UntypedInt PERF_CONTEXT_GUEST => /* -0x800 */ -2048;
public static UntypedInt PERF_CONTEXT_GUEST_KERNEL => /* -0x880 */ -2176;
public static UntypedInt PERF_CONTEXT_GUEST_USER => /* -0xa00 */ -2560;
public static UntypedInt PERF_CONTEXT_MAX => /* -0xfff */ -4095;

[global::go.golib.GoInlineLayout(Size = 216, Align = 8), StructLayout(LayoutKind.Explicit, Size = 216)] partial struct TCPMD5Sig {
    [FieldOffset(0)] public SockaddrStorage Addr;
    [FieldOffset(128)] public uint8 Flags;
    [FieldOffset(129)] public uint8 Prefixlen;
    [FieldOffset(130)] public uint16 Keylen;
    [FieldOffset(132)] public int32 Ifindex;
    [FieldOffset(136)] public array80<uint8> Key;
}

partial struct HDDriveCmdHdr {
    public uint8 Command;
    public uint8 Number;
    public uint8 Feature;
    public uint8 Count;
}

partial struct HDDriveID {
    public uint16 Config;
    public uint16 Cyls;
    public uint16 Reserved2;
    public uint16 Heads;
    public uint16 Track_bytes;
    public uint16 Sector_bytes;
    public uint16 Sectors;
    public uint16 Vendor0;
    public uint16 Vendor1;
    public uint16 Vendor2;
    public array<uint8> Serial_no = new(20);
    public uint16 Buf_type;
    public uint16 Buf_size;
    public uint16 Ecc_bytes;
    public array<uint8> Fw_rev = new(8);
    public array<uint8> Model = new(40);
    public uint8 Max_multsect;
    public uint8 Vendor3;
    public uint16 Dword_io;
    public uint8 Vendor4;
    public uint8 Capability;
    public uint16 Reserved50;
    public uint8 Vendor5;
    public uint8 TPIO;
    public uint8 Vendor6;
    public uint8 TDMA;
    public uint16 Field_valid;
    public uint16 Cur_cyls;
    public uint16 Cur_heads;
    public uint16 Cur_sectors;
    public uint16 Cur_capacity0;
    public uint16 Cur_capacity1;
    public uint8 Multsect;
    public uint8 Multsect_valid;
    public uint32 Lba_capacity;
    public uint16 Dma_1word;
    public uint16 Dma_mword;
    public uint16 Eide_pio_modes;
    public uint16 Eide_dma_min;
    public uint16 Eide_dma_time;
    public uint16 Eide_pio;
    public uint16 Eide_pio_iordy;
    public array<uint16> Words69_70 = new(2);
    public array<uint16> Words71_74 = new(4);
    public uint16 Queue_depth;
    public array<uint16> Words76_79 = new(4);
    public uint16 Major_rev_num;
    public uint16 Minor_rev_num;
    public uint16 Command_set_1;
    public uint16 Command_set_2;
    public uint16 Cfsse;
    public uint16 Cfs_enable_1;
    public uint16 Cfs_enable_2;
    public uint16 Csf_default;
    public uint16 Dma_ultra;
    public uint16 Trseuc;
    public uint16 TrsEuc;
    public uint16 CurAPMvalues;
    public uint16 Mprc;
    public uint16 Hw_config;
    public uint16 Acoustic;
    public uint16 Msrqs;
    public uint16 Sxfert;
    public uint16 Sal;
    public uint32 Spg;
    public uint64 Lba_capacity_2;
    public array<uint16> Words104_125 = new(22);
    public uint16 Last_lun;
    public uint16 Word127;
    public uint16 Dlf;
    public uint16 Csfo;
    public array<uint16> Words130_155 = new(26);
    public uint16 Word156;
    public array<uint16> Words157_159 = new(3);
    public uint16 Cfa_power;
    public array<uint16> Words161_175 = new(15);
    public array<uint16> Words176_205 = new(30);
    public array<uint16> Words206_254 = new(49);
    public uint16 Integrity_word;
}

public static UntypedInt ST_MANDLOCK => 0x40;
public static UntypedInt ST_NOATIME => 0x400;
public static UntypedInt ST_NODEV => 0x4;
public static UntypedInt ST_NODIRATIME => 0x800;
public static UntypedInt ST_NOEXEC => 0x8;
public static UntypedInt ST_NOSUID => 0x2;
public static UntypedInt ST_RDONLY => 0x1;
public static UntypedInt ST_RELATIME => 0x1000;
public static UntypedInt ST_SYNCHRONOUS => 0x10;

partial struct Tpacket2Hdr {
    public uint32 Status;
    public uint32 Len;
    public uint32 Snaplen;
    public uint16 Mac;
    public uint16 Net;
    public uint32 Sec;
    public uint32 Nsec;
    public uint16 Vlan_tci;
    public uint16 Vlan_tpid;
    internal array<uint8> _ = new(4);
}

partial struct Tpacket3Hdr {
    public uint32 Next_offset;
    public uint32 Sec;
    public uint32 Nsec;
    public uint32 Snaplen;
    public uint32 Len;
    public uint32 Status;
    public uint16 Mac;
    public uint16 Net;
    public TpacketHdrVariant1 Hv1;
    internal array<uint8> _ = new(8);
}

partial struct TpacketHdrVariant1 {
    public uint32 Rxhash;
    public uint32 Vlan_tci;
    public uint16 Vlan_tpid;
    internal uint16 _;
}

partial struct TpacketBlockDesc {
    public uint32 Version;
    public uint32 To_priv;
    public array<byte> Hdr = new(40);
}

partial struct TpacketBDTS {
    public uint32 Sec;
    public uint32 Usec;
}

partial struct TpacketHdrV1 {
    public uint32 Block_status;
    public uint32 Num_pkts;
    public uint32 Offset_to_first_pkt;
    public uint32 Blk_len;
    public uint64 Seq_num;
    public TpacketBDTS Ts_first_pkt;
    public TpacketBDTS Ts_last_pkt;
}

partial struct TpacketReq {
    public uint32 Block_size;
    public uint32 Block_nr;
    public uint32 Frame_size;
    public uint32 Frame_nr;
}

partial struct TpacketReq3 {
    public uint32 Block_size;
    public uint32 Block_nr;
    public uint32 Frame_size;
    public uint32 Frame_nr;
    public uint32 Retire_blk_tov;
    public uint32 Sizeof_priv;
    public uint32 Feature_req_word;
}

partial struct TpacketStats {
    public uint32 Packets;
    public uint32 Drops;
}

partial struct TpacketStatsV3 {
    public uint32 Packets;
    public uint32 Drops;
    public uint32 Freeze_q_cnt;
}

partial struct TpacketAuxdata {
    public uint32 Status;
    public uint32 Len;
    public uint32 Snaplen;
    public uint16 Mac;
    public uint16 Net;
    public uint16 Vlan_tci;
    public uint16 Vlan_tpid;
}

public static UntypedInt TPACKET_V1 => 0x0;
public static UntypedInt TPACKET_V2 => 0x1;
public static UntypedInt TPACKET_V3 => 0x2;

public static UntypedInt SizeofTpacket2Hdr => 0x20;
public static UntypedInt SizeofTpacket3Hdr => 0x30;
public static UntypedInt SizeofTpacketStats => 0x8;
public static UntypedInt SizeofTpacketStatsV3 => 0xc;

public static UntypedInt IFLA_UNSPEC => 0x0;
public static UntypedInt IFLA_ADDRESS => 0x1;
public static UntypedInt IFLA_BROADCAST => 0x2;
public static UntypedInt IFLA_IFNAME => 0x3;
public static UntypedInt IFLA_MTU => 0x4;
public static UntypedInt IFLA_LINK => 0x5;
public static UntypedInt IFLA_QDISC => 0x6;
public static UntypedInt IFLA_STATS => 0x7;
public static UntypedInt IFLA_COST => 0x8;
public static UntypedInt IFLA_PRIORITY => 0x9;
public static UntypedInt IFLA_MASTER => 0xa;
public static UntypedInt IFLA_WIRELESS => 0xb;
public static UntypedInt IFLA_PROTINFO => 0xc;
public static UntypedInt IFLA_TXQLEN => 0xd;
public static UntypedInt IFLA_MAP => 0xe;
public static UntypedInt IFLA_WEIGHT => 0xf;
public static UntypedInt IFLA_OPERSTATE => 0x10;
public static UntypedInt IFLA_LINKMODE => 0x11;
public static UntypedInt IFLA_LINKINFO => 0x12;
public static UntypedInt IFLA_NET_NS_PID => 0x13;
public static UntypedInt IFLA_IFALIAS => 0x14;
public static UntypedInt IFLA_NUM_VF => 0x15;
public static UntypedInt IFLA_VFINFO_LIST => 0x16;
public static UntypedInt IFLA_STATS64 => 0x17;
public static UntypedInt IFLA_VF_PORTS => 0x18;
public static UntypedInt IFLA_PORT_SELF => 0x19;
public static UntypedInt IFLA_AF_SPEC => 0x1a;
public static UntypedInt IFLA_GROUP => 0x1b;
public static UntypedInt IFLA_NET_NS_FD => 0x1c;
public static UntypedInt IFLA_EXT_MASK => 0x1d;
public static UntypedInt IFLA_PROMISCUITY => 0x1e;
public static UntypedInt IFLA_NUM_TX_QUEUES => 0x1f;
public static UntypedInt IFLA_NUM_RX_QUEUES => 0x20;
public static UntypedInt IFLA_CARRIER => 0x21;
public static UntypedInt IFLA_PHYS_PORT_ID => 0x22;
public static UntypedInt IFLA_CARRIER_CHANGES => 0x23;
public static UntypedInt IFLA_PHYS_SWITCH_ID => 0x24;
public static UntypedInt IFLA_LINK_NETNSID => 0x25;
public static UntypedInt IFLA_PHYS_PORT_NAME => 0x26;
public static UntypedInt IFLA_PROTO_DOWN => 0x27;
public static UntypedInt IFLA_GSO_MAX_SEGS => 0x28;
public static UntypedInt IFLA_GSO_MAX_SIZE => 0x29;
public static UntypedInt IFLA_PAD => 0x2a;
public static UntypedInt IFLA_XDP => 0x2b;
public static UntypedInt IFLA_EVENT => 0x2c;
public static UntypedInt IFLA_NEW_NETNSID => 0x2d;
public static UntypedInt IFLA_IF_NETNSID => 0x2e;
public static UntypedInt IFLA_TARGET_NETNSID => 0x2e;
public static UntypedInt IFLA_CARRIER_UP_COUNT => 0x2f;
public static UntypedInt IFLA_CARRIER_DOWN_COUNT => 0x30;
public static UntypedInt IFLA_NEW_IFINDEX => 0x31;
public static UntypedInt IFLA_MIN_MTU => 0x32;
public static UntypedInt IFLA_MAX_MTU => 0x33;
public static UntypedInt IFLA_PROP_LIST => 0x34;
public static UntypedInt IFLA_ALT_IFNAME => 0x35;
public static UntypedInt IFLA_PERM_ADDRESS => 0x36;
public static UntypedInt IFLA_PROTO_DOWN_REASON => 0x37;
public static UntypedInt IFLA_PARENT_DEV_NAME => 0x38;
public static UntypedInt IFLA_PARENT_DEV_BUS_NAME => 0x39;
public static UntypedInt IFLA_GRO_MAX_SIZE => 0x3a;
public static UntypedInt IFLA_TSO_MAX_SIZE => 0x3b;
public static UntypedInt IFLA_TSO_MAX_SEGS => 0x3c;
public static UntypedInt IFLA_ALLMULTI => 0x3d;
public static UntypedInt IFLA_DEVLINK_PORT => 0x3e;
public static UntypedInt IFLA_GSO_IPV4_MAX_SIZE => 0x3f;
public static UntypedInt IFLA_GRO_IPV4_MAX_SIZE => 0x40;
public static UntypedInt IFLA_DPLL_PIN => 0x41;
public static UntypedInt IFLA_PROTO_DOWN_REASON_UNSPEC => 0x0;
public static UntypedInt IFLA_PROTO_DOWN_REASON_MASK => 0x1;
public static UntypedInt IFLA_PROTO_DOWN_REASON_VALUE => 0x2;
public static UntypedInt IFLA_PROTO_DOWN_REASON_MAX => 0x2;
public static UntypedInt IFLA_INET_UNSPEC => 0x0;
public static UntypedInt IFLA_INET_CONF => 0x1;
public static UntypedInt IFLA_INET6_UNSPEC => 0x0;
public static UntypedInt IFLA_INET6_FLAGS => 0x1;
public static UntypedInt IFLA_INET6_CONF => 0x2;
public static UntypedInt IFLA_INET6_STATS => 0x3;
public static UntypedInt IFLA_INET6_MCAST => 0x4;
public static UntypedInt IFLA_INET6_CACHEINFO => 0x5;
public static UntypedInt IFLA_INET6_ICMP6STATS => 0x6;
public static UntypedInt IFLA_INET6_TOKEN => 0x7;
public static UntypedInt IFLA_INET6_ADDR_GEN_MODE => 0x8;
public static UntypedInt IFLA_INET6_RA_MTU => 0x9;
public static UntypedInt IFLA_BR_UNSPEC => 0x0;
public static UntypedInt IFLA_BR_FORWARD_DELAY => 0x1;
public static UntypedInt IFLA_BR_HELLO_TIME => 0x2;
public static UntypedInt IFLA_BR_MAX_AGE => 0x3;
public static UntypedInt IFLA_BR_AGEING_TIME => 0x4;
public static UntypedInt IFLA_BR_STP_STATE => 0x5;
public static UntypedInt IFLA_BR_PRIORITY => 0x6;
public static UntypedInt IFLA_BR_VLAN_FILTERING => 0x7;
public static UntypedInt IFLA_BR_VLAN_PROTOCOL => 0x8;
public static UntypedInt IFLA_BR_GROUP_FWD_MASK => 0x9;
public static UntypedInt IFLA_BR_ROOT_ID => 0xa;
public static UntypedInt IFLA_BR_BRIDGE_ID => 0xb;
public static UntypedInt IFLA_BR_ROOT_PORT => 0xc;
public static UntypedInt IFLA_BR_ROOT_PATH_COST => 0xd;
public static UntypedInt IFLA_BR_TOPOLOGY_CHANGE => 0xe;
public static UntypedInt IFLA_BR_TOPOLOGY_CHANGE_DETECTED => 0xf;
public static UntypedInt IFLA_BR_HELLO_TIMER => 0x10;
public static UntypedInt IFLA_BR_TCN_TIMER => 0x11;
public static UntypedInt IFLA_BR_TOPOLOGY_CHANGE_TIMER => 0x12;
public static UntypedInt IFLA_BR_GC_TIMER => 0x13;
public static UntypedInt IFLA_BR_GROUP_ADDR => 0x14;
public static UntypedInt IFLA_BR_FDB_FLUSH => 0x15;
public static UntypedInt IFLA_BR_MCAST_ROUTER => 0x16;
public static UntypedInt IFLA_BR_MCAST_SNOOPING => 0x17;
public static UntypedInt IFLA_BR_MCAST_QUERY_USE_IFADDR => 0x18;
public static UntypedInt IFLA_BR_MCAST_QUERIER => 0x19;
public static UntypedInt IFLA_BR_MCAST_HASH_ELASTICITY => 0x1a;
public static UntypedInt IFLA_BR_MCAST_HASH_MAX => 0x1b;
public static UntypedInt IFLA_BR_MCAST_LAST_MEMBER_CNT => 0x1c;
public static UntypedInt IFLA_BR_MCAST_STARTUP_QUERY_CNT => 0x1d;
public static UntypedInt IFLA_BR_MCAST_LAST_MEMBER_INTVL => 0x1e;
public static UntypedInt IFLA_BR_MCAST_MEMBERSHIP_INTVL => 0x1f;
public static UntypedInt IFLA_BR_MCAST_QUERIER_INTVL => 0x20;
public static UntypedInt IFLA_BR_MCAST_QUERY_INTVL => 0x21;
public static UntypedInt IFLA_BR_MCAST_QUERY_RESPONSE_INTVL => 0x22;
public static UntypedInt IFLA_BR_MCAST_STARTUP_QUERY_INTVL => 0x23;
public static UntypedInt IFLA_BR_NF_CALL_IPTABLES => 0x24;
public static UntypedInt IFLA_BR_NF_CALL_IP6TABLES => 0x25;
public static UntypedInt IFLA_BR_NF_CALL_ARPTABLES => 0x26;
public static UntypedInt IFLA_BR_VLAN_DEFAULT_PVID => 0x27;
public static UntypedInt IFLA_BR_PAD => 0x28;
public static UntypedInt IFLA_BR_VLAN_STATS_ENABLED => 0x29;
public static UntypedInt IFLA_BR_MCAST_STATS_ENABLED => 0x2a;
public static UntypedInt IFLA_BR_MCAST_IGMP_VERSION => 0x2b;
public static UntypedInt IFLA_BR_MCAST_MLD_VERSION => 0x2c;
public static UntypedInt IFLA_BR_VLAN_STATS_PER_PORT => 0x2d;
public static UntypedInt IFLA_BR_MULTI_BOOLOPT => 0x2e;
public static UntypedInt IFLA_BR_MCAST_QUERIER_STATE => 0x2f;
public static UntypedInt IFLA_BR_FDB_N_LEARNED => 0x30;
public static UntypedInt IFLA_BR_FDB_MAX_LEARNED => 0x31;
public static UntypedInt IFLA_BRPORT_UNSPEC => 0x0;
public static UntypedInt IFLA_BRPORT_STATE => 0x1;
public static UntypedInt IFLA_BRPORT_PRIORITY => 0x2;
public static UntypedInt IFLA_BRPORT_COST => 0x3;
public static UntypedInt IFLA_BRPORT_MODE => 0x4;
public static UntypedInt IFLA_BRPORT_GUARD => 0x5;
public static UntypedInt IFLA_BRPORT_PROTECT => 0x6;
public static UntypedInt IFLA_BRPORT_FAST_LEAVE => 0x7;
public static UntypedInt IFLA_BRPORT_LEARNING => 0x8;
public static UntypedInt IFLA_BRPORT_UNICAST_FLOOD => 0x9;
public static UntypedInt IFLA_BRPORT_PROXYARP => 0xa;
public static UntypedInt IFLA_BRPORT_LEARNING_SYNC => 0xb;
public static UntypedInt IFLA_BRPORT_PROXYARP_WIFI => 0xc;
public static UntypedInt IFLA_BRPORT_ROOT_ID => 0xd;
public static UntypedInt IFLA_BRPORT_BRIDGE_ID => 0xe;
public static UntypedInt IFLA_BRPORT_DESIGNATED_PORT => 0xf;
public static UntypedInt IFLA_BRPORT_DESIGNATED_COST => 0x10;
public static UntypedInt IFLA_BRPORT_ID => 0x11;
public static UntypedInt IFLA_BRPORT_NO => 0x12;
public static UntypedInt IFLA_BRPORT_TOPOLOGY_CHANGE_ACK => 0x13;
public static UntypedInt IFLA_BRPORT_CONFIG_PENDING => 0x14;
public static UntypedInt IFLA_BRPORT_MESSAGE_AGE_TIMER => 0x15;
public static UntypedInt IFLA_BRPORT_FORWARD_DELAY_TIMER => 0x16;
public static UntypedInt IFLA_BRPORT_HOLD_TIMER => 0x17;
public static UntypedInt IFLA_BRPORT_FLUSH => 0x18;
public static UntypedInt IFLA_BRPORT_MULTICAST_ROUTER => 0x19;
public static UntypedInt IFLA_BRPORT_PAD => 0x1a;
public static UntypedInt IFLA_BRPORT_MCAST_FLOOD => 0x1b;
public static UntypedInt IFLA_BRPORT_MCAST_TO_UCAST => 0x1c;
public static UntypedInt IFLA_BRPORT_VLAN_TUNNEL => 0x1d;
public static UntypedInt IFLA_BRPORT_BCAST_FLOOD => 0x1e;
public static UntypedInt IFLA_BRPORT_GROUP_FWD_MASK => 0x1f;
public static UntypedInt IFLA_BRPORT_NEIGH_SUPPRESS => 0x20;
public static UntypedInt IFLA_BRPORT_ISOLATED => 0x21;
public static UntypedInt IFLA_BRPORT_BACKUP_PORT => 0x22;
public static UntypedInt IFLA_BRPORT_MRP_RING_OPEN => 0x23;
public static UntypedInt IFLA_BRPORT_MRP_IN_OPEN => 0x24;
public static UntypedInt IFLA_BRPORT_MCAST_EHT_HOSTS_LIMIT => 0x25;
public static UntypedInt IFLA_BRPORT_MCAST_EHT_HOSTS_CNT => 0x26;
public static UntypedInt IFLA_BRPORT_LOCKED => 0x27;
public static UntypedInt IFLA_BRPORT_MAB => 0x28;
public static UntypedInt IFLA_BRPORT_MCAST_N_GROUPS => 0x29;
public static UntypedInt IFLA_BRPORT_MCAST_MAX_GROUPS => 0x2a;
public static UntypedInt IFLA_BRPORT_NEIGH_VLAN_SUPPRESS => 0x2b;
public static UntypedInt IFLA_BRPORT_BACKUP_NHID => 0x2c;
public static UntypedInt IFLA_INFO_UNSPEC => 0x0;
public static UntypedInt IFLA_INFO_KIND => 0x1;
public static UntypedInt IFLA_INFO_DATA => 0x2;
public static UntypedInt IFLA_INFO_XSTATS => 0x3;
public static UntypedInt IFLA_INFO_SLAVE_KIND => 0x4;
public static UntypedInt IFLA_INFO_SLAVE_DATA => 0x5;
public static UntypedInt IFLA_VLAN_UNSPEC => 0x0;
public static UntypedInt IFLA_VLAN_ID => 0x1;
public static UntypedInt IFLA_VLAN_FLAGS => 0x2;
public static UntypedInt IFLA_VLAN_EGRESS_QOS => 0x3;
public static UntypedInt IFLA_VLAN_INGRESS_QOS => 0x4;
public static UntypedInt IFLA_VLAN_PROTOCOL => 0x5;
public static UntypedInt IFLA_VLAN_QOS_UNSPEC => 0x0;
public static UntypedInt IFLA_VLAN_QOS_MAPPING => 0x1;
public static UntypedInt IFLA_MACVLAN_UNSPEC => 0x0;
public static UntypedInt IFLA_MACVLAN_MODE => 0x1;
public static UntypedInt IFLA_MACVLAN_FLAGS => 0x2;
public static UntypedInt IFLA_MACVLAN_MACADDR_MODE => 0x3;
public static UntypedInt IFLA_MACVLAN_MACADDR => 0x4;
public static UntypedInt IFLA_MACVLAN_MACADDR_DATA => 0x5;
public static UntypedInt IFLA_MACVLAN_MACADDR_COUNT => 0x6;
public static UntypedInt IFLA_MACVLAN_BC_QUEUE_LEN => 0x7;
public static UntypedInt IFLA_MACVLAN_BC_QUEUE_LEN_USED => 0x8;
public static UntypedInt IFLA_MACVLAN_BC_CUTOFF => 0x9;
public static UntypedInt IFLA_VRF_UNSPEC => 0x0;
public static UntypedInt IFLA_VRF_TABLE => 0x1;
public static UntypedInt IFLA_VRF_PORT_UNSPEC => 0x0;
public static UntypedInt IFLA_VRF_PORT_TABLE => 0x1;
public static UntypedInt IFLA_MACSEC_UNSPEC => 0x0;
public static UntypedInt IFLA_MACSEC_SCI => 0x1;
public static UntypedInt IFLA_MACSEC_PORT => 0x2;
public static UntypedInt IFLA_MACSEC_ICV_LEN => 0x3;
public static UntypedInt IFLA_MACSEC_CIPHER_SUITE => 0x4;
public static UntypedInt IFLA_MACSEC_WINDOW => 0x5;
public static UntypedInt IFLA_MACSEC_ENCODING_SA => 0x6;
public static UntypedInt IFLA_MACSEC_ENCRYPT => 0x7;
public static UntypedInt IFLA_MACSEC_PROTECT => 0x8;
public static UntypedInt IFLA_MACSEC_INC_SCI => 0x9;
public static UntypedInt IFLA_MACSEC_ES => 0xa;
public static UntypedInt IFLA_MACSEC_SCB => 0xb;
public static UntypedInt IFLA_MACSEC_REPLAY_PROTECT => 0xc;
public static UntypedInt IFLA_MACSEC_VALIDATION => 0xd;
public static UntypedInt IFLA_MACSEC_PAD => 0xe;
public static UntypedInt IFLA_MACSEC_OFFLOAD => 0xf;
public static UntypedInt IFLA_XFRM_UNSPEC => 0x0;
public static UntypedInt IFLA_XFRM_LINK => 0x1;
public static UntypedInt IFLA_XFRM_IF_ID => 0x2;
public static UntypedInt IFLA_XFRM_COLLECT_METADATA => 0x3;
public static UntypedInt IFLA_IPVLAN_UNSPEC => 0x0;
public static UntypedInt IFLA_IPVLAN_MODE => 0x1;
public static UntypedInt IFLA_IPVLAN_FLAGS => 0x2;
public static UntypedInt IFLA_NETKIT_UNSPEC => 0x0;
public static UntypedInt IFLA_NETKIT_PEER_INFO => 0x1;
public static UntypedInt IFLA_NETKIT_PRIMARY => 0x2;
public static UntypedInt IFLA_NETKIT_POLICY => 0x3;
public static UntypedInt IFLA_NETKIT_PEER_POLICY => 0x4;
public static UntypedInt IFLA_NETKIT_MODE => 0x5;
public static UntypedInt IFLA_VXLAN_UNSPEC => 0x0;
public static UntypedInt IFLA_VXLAN_ID => 0x1;
public static UntypedInt IFLA_VXLAN_GROUP => 0x2;
public static UntypedInt IFLA_VXLAN_LINK => 0x3;
public static UntypedInt IFLA_VXLAN_LOCAL => 0x4;
public static UntypedInt IFLA_VXLAN_TTL => 0x5;
public static UntypedInt IFLA_VXLAN_TOS => 0x6;
public static UntypedInt IFLA_VXLAN_LEARNING => 0x7;
public static UntypedInt IFLA_VXLAN_AGEING => 0x8;
public static UntypedInt IFLA_VXLAN_LIMIT => 0x9;
public static UntypedInt IFLA_VXLAN_PORT_RANGE => 0xa;
public static UntypedInt IFLA_VXLAN_PROXY => 0xb;
public static UntypedInt IFLA_VXLAN_RSC => 0xc;
public static UntypedInt IFLA_VXLAN_L2MISS => 0xd;
public static UntypedInt IFLA_VXLAN_L3MISS => 0xe;
public static UntypedInt IFLA_VXLAN_PORT => 0xf;
public static UntypedInt IFLA_VXLAN_GROUP6 => 0x10;
public static UntypedInt IFLA_VXLAN_LOCAL6 => 0x11;
public static UntypedInt IFLA_VXLAN_UDP_CSUM => 0x12;
public static UntypedInt IFLA_VXLAN_UDP_ZERO_CSUM6_TX => 0x13;
public static UntypedInt IFLA_VXLAN_UDP_ZERO_CSUM6_RX => 0x14;
public static UntypedInt IFLA_VXLAN_REMCSUM_TX => 0x15;
public static UntypedInt IFLA_VXLAN_REMCSUM_RX => 0x16;
public static UntypedInt IFLA_VXLAN_GBP => 0x17;
public static UntypedInt IFLA_VXLAN_REMCSUM_NOPARTIAL => 0x18;
public static UntypedInt IFLA_VXLAN_COLLECT_METADATA => 0x19;
public static UntypedInt IFLA_VXLAN_LABEL => 0x1a;
public static UntypedInt IFLA_VXLAN_GPE => 0x1b;
public static UntypedInt IFLA_VXLAN_TTL_INHERIT => 0x1c;
public static UntypedInt IFLA_VXLAN_DF => 0x1d;
public static UntypedInt IFLA_VXLAN_VNIFILTER => 0x1e;
public static UntypedInt IFLA_VXLAN_LOCALBYPASS => 0x1f;
public static UntypedInt IFLA_VXLAN_LABEL_POLICY => 0x20;
public static UntypedInt IFLA_GENEVE_UNSPEC => 0x0;
public static UntypedInt IFLA_GENEVE_ID => 0x1;
public static UntypedInt IFLA_GENEVE_REMOTE => 0x2;
public static UntypedInt IFLA_GENEVE_TTL => 0x3;
public static UntypedInt IFLA_GENEVE_TOS => 0x4;
public static UntypedInt IFLA_GENEVE_PORT => 0x5;
public static UntypedInt IFLA_GENEVE_COLLECT_METADATA => 0x6;
public static UntypedInt IFLA_GENEVE_REMOTE6 => 0x7;
public static UntypedInt IFLA_GENEVE_UDP_CSUM => 0x8;
public static UntypedInt IFLA_GENEVE_UDP_ZERO_CSUM6_TX => 0x9;
public static UntypedInt IFLA_GENEVE_UDP_ZERO_CSUM6_RX => 0xa;
public static UntypedInt IFLA_GENEVE_LABEL => 0xb;
public static UntypedInt IFLA_GENEVE_TTL_INHERIT => 0xc;
public static UntypedInt IFLA_GENEVE_DF => 0xd;
public static UntypedInt IFLA_GENEVE_INNER_PROTO_INHERIT => 0xe;
public static UntypedInt IFLA_BAREUDP_UNSPEC => 0x0;
public static UntypedInt IFLA_BAREUDP_PORT => 0x1;
public static UntypedInt IFLA_BAREUDP_ETHERTYPE => 0x2;
public static UntypedInt IFLA_BAREUDP_SRCPORT_MIN => 0x3;
public static UntypedInt IFLA_BAREUDP_MULTIPROTO_MODE => 0x4;
public static UntypedInt IFLA_PPP_UNSPEC => 0x0;
public static UntypedInt IFLA_PPP_DEV_FD => 0x1;
public static UntypedInt IFLA_GTP_UNSPEC => 0x0;
public static UntypedInt IFLA_GTP_FD0 => 0x1;
public static UntypedInt IFLA_GTP_FD1 => 0x2;
public static UntypedInt IFLA_GTP_PDP_HASHSIZE => 0x3;
public static UntypedInt IFLA_GTP_ROLE => 0x4;
public static UntypedInt IFLA_GTP_CREATE_SOCKETS => 0x5;
public static UntypedInt IFLA_GTP_RESTART_COUNT => 0x6;
public static UntypedInt IFLA_GTP_LOCAL => 0x7;
public static UntypedInt IFLA_GTP_LOCAL6 => 0x8;
public static UntypedInt IFLA_BOND_UNSPEC => 0x0;
public static UntypedInt IFLA_BOND_MODE => 0x1;
public static UntypedInt IFLA_BOND_ACTIVE_SLAVE => 0x2;
public static UntypedInt IFLA_BOND_MIIMON => 0x3;
public static UntypedInt IFLA_BOND_UPDELAY => 0x4;
public static UntypedInt IFLA_BOND_DOWNDELAY => 0x5;
public static UntypedInt IFLA_BOND_USE_CARRIER => 0x6;
public static UntypedInt IFLA_BOND_ARP_INTERVAL => 0x7;
public static UntypedInt IFLA_BOND_ARP_IP_TARGET => 0x8;
public static UntypedInt IFLA_BOND_ARP_VALIDATE => 0x9;
public static UntypedInt IFLA_BOND_ARP_ALL_TARGETS => 0xa;
public static UntypedInt IFLA_BOND_PRIMARY => 0xb;
public static UntypedInt IFLA_BOND_PRIMARY_RESELECT => 0xc;
public static UntypedInt IFLA_BOND_FAIL_OVER_MAC => 0xd;
public static UntypedInt IFLA_BOND_XMIT_HASH_POLICY => 0xe;
public static UntypedInt IFLA_BOND_RESEND_IGMP => 0xf;
public static UntypedInt IFLA_BOND_NUM_PEER_NOTIF => 0x10;
public static UntypedInt IFLA_BOND_ALL_SLAVES_ACTIVE => 0x11;
public static UntypedInt IFLA_BOND_MIN_LINKS => 0x12;
public static UntypedInt IFLA_BOND_LP_INTERVAL => 0x13;
public static UntypedInt IFLA_BOND_PACKETS_PER_SLAVE => 0x14;
public static UntypedInt IFLA_BOND_AD_LACP_RATE => 0x15;
public static UntypedInt IFLA_BOND_AD_SELECT => 0x16;
public static UntypedInt IFLA_BOND_AD_INFO => 0x17;
public static UntypedInt IFLA_BOND_AD_ACTOR_SYS_PRIO => 0x18;
public static UntypedInt IFLA_BOND_AD_USER_PORT_KEY => 0x19;
public static UntypedInt IFLA_BOND_AD_ACTOR_SYSTEM => 0x1a;
public static UntypedInt IFLA_BOND_TLB_DYNAMIC_LB => 0x1b;
public static UntypedInt IFLA_BOND_PEER_NOTIF_DELAY => 0x1c;
public static UntypedInt IFLA_BOND_AD_LACP_ACTIVE => 0x1d;
public static UntypedInt IFLA_BOND_MISSED_MAX => 0x1e;
public static UntypedInt IFLA_BOND_NS_IP6_TARGET => 0x1f;
public static UntypedInt IFLA_BOND_COUPLED_CONTROL => 0x20;
public static UntypedInt IFLA_BOND_AD_INFO_UNSPEC => 0x0;
public static UntypedInt IFLA_BOND_AD_INFO_AGGREGATOR => 0x1;
public static UntypedInt IFLA_BOND_AD_INFO_NUM_PORTS => 0x2;
public static UntypedInt IFLA_BOND_AD_INFO_ACTOR_KEY => 0x3;
public static UntypedInt IFLA_BOND_AD_INFO_PARTNER_KEY => 0x4;
public static UntypedInt IFLA_BOND_AD_INFO_PARTNER_MAC => 0x5;
public static UntypedInt IFLA_BOND_SLAVE_UNSPEC => 0x0;
public static UntypedInt IFLA_BOND_SLAVE_STATE => 0x1;
public static UntypedInt IFLA_BOND_SLAVE_MII_STATUS => 0x2;
public static UntypedInt IFLA_BOND_SLAVE_LINK_FAILURE_COUNT => 0x3;
public static UntypedInt IFLA_BOND_SLAVE_PERM_HWADDR => 0x4;
public static UntypedInt IFLA_BOND_SLAVE_QUEUE_ID => 0x5;
public static UntypedInt IFLA_BOND_SLAVE_AD_AGGREGATOR_ID => 0x6;
public static UntypedInt IFLA_BOND_SLAVE_AD_ACTOR_OPER_PORT_STATE => 0x7;
public static UntypedInt IFLA_BOND_SLAVE_AD_PARTNER_OPER_PORT_STATE => 0x8;
public static UntypedInt IFLA_BOND_SLAVE_PRIO => 0x9;
public static UntypedInt IFLA_VF_INFO_UNSPEC => 0x0;
public static UntypedInt IFLA_VF_INFO => 0x1;
public static UntypedInt IFLA_VF_UNSPEC => 0x0;
public static UntypedInt IFLA_VF_MAC => 0x1;
public static UntypedInt IFLA_VF_VLAN => 0x2;
public static UntypedInt IFLA_VF_TX_RATE => 0x3;
public static UntypedInt IFLA_VF_SPOOFCHK => 0x4;
public static UntypedInt IFLA_VF_LINK_STATE => 0x5;
public static UntypedInt IFLA_VF_RATE => 0x6;
public static UntypedInt IFLA_VF_RSS_QUERY_EN => 0x7;
public static UntypedInt IFLA_VF_STATS => 0x8;
public static UntypedInt IFLA_VF_TRUST => 0x9;
public static UntypedInt IFLA_VF_IB_NODE_GUID => 0xa;
public static UntypedInt IFLA_VF_IB_PORT_GUID => 0xb;
public static UntypedInt IFLA_VF_VLAN_LIST => 0xc;
public static UntypedInt IFLA_VF_BROADCAST => 0xd;
public static UntypedInt IFLA_VF_VLAN_INFO_UNSPEC => 0x0;
public static UntypedInt IFLA_VF_VLAN_INFO => 0x1;
public static UntypedInt IFLA_VF_LINK_STATE_AUTO => 0x0;
public static UntypedInt IFLA_VF_LINK_STATE_ENABLE => 0x1;
public static UntypedInt IFLA_VF_LINK_STATE_DISABLE => 0x2;
public static UntypedInt IFLA_VF_STATS_RX_PACKETS => 0x0;
public static UntypedInt IFLA_VF_STATS_TX_PACKETS => 0x1;
public static UntypedInt IFLA_VF_STATS_RX_BYTES => 0x2;
public static UntypedInt IFLA_VF_STATS_TX_BYTES => 0x3;
public static UntypedInt IFLA_VF_STATS_BROADCAST => 0x4;
public static UntypedInt IFLA_VF_STATS_MULTICAST => 0x5;
public static UntypedInt IFLA_VF_STATS_PAD => 0x6;
public static UntypedInt IFLA_VF_STATS_RX_DROPPED => 0x7;
public static UntypedInt IFLA_VF_STATS_TX_DROPPED => 0x8;
public static UntypedInt IFLA_VF_PORT_UNSPEC => 0x0;
public static UntypedInt IFLA_VF_PORT => 0x1;
public static UntypedInt IFLA_PORT_UNSPEC => 0x0;
public static UntypedInt IFLA_PORT_VF => 0x1;
public static UntypedInt IFLA_PORT_PROFILE => 0x2;
public static UntypedInt IFLA_PORT_VSI_TYPE => 0x3;
public static UntypedInt IFLA_PORT_INSTANCE_UUID => 0x4;
public static UntypedInt IFLA_PORT_HOST_UUID => 0x5;
public static UntypedInt IFLA_PORT_REQUEST => 0x6;
public static UntypedInt IFLA_PORT_RESPONSE => 0x7;
public static UntypedInt IFLA_IPOIB_UNSPEC => 0x0;
public static UntypedInt IFLA_IPOIB_PKEY => 0x1;
public static UntypedInt IFLA_IPOIB_MODE => 0x2;
public static UntypedInt IFLA_IPOIB_UMCAST => 0x3;
public static UntypedInt IFLA_HSR_UNSPEC => 0x0;
public static UntypedInt IFLA_HSR_SLAVE1 => 0x1;
public static UntypedInt IFLA_HSR_SLAVE2 => 0x2;
public static UntypedInt IFLA_HSR_MULTICAST_SPEC => 0x3;
public static UntypedInt IFLA_HSR_SUPERVISION_ADDR => 0x4;
public static UntypedInt IFLA_HSR_SEQ_NR => 0x5;
public static UntypedInt IFLA_HSR_VERSION => 0x6;
public static UntypedInt IFLA_HSR_PROTOCOL => 0x7;
public static UntypedInt IFLA_HSR_INTERLINK => 0x8;
public static UntypedInt IFLA_STATS_UNSPEC => 0x0;
public static UntypedInt IFLA_STATS_LINK_64 => 0x1;
public static UntypedInt IFLA_STATS_LINK_XSTATS => 0x2;
public static UntypedInt IFLA_STATS_LINK_XSTATS_SLAVE => 0x3;
public static UntypedInt IFLA_STATS_LINK_OFFLOAD_XSTATS => 0x4;
public static UntypedInt IFLA_STATS_AF_SPEC => 0x5;
public static UntypedInt IFLA_STATS_GETSET_UNSPEC => 0x0;
public static UntypedInt IFLA_STATS_GET_FILTERS => 0x1;
public static UntypedInt IFLA_STATS_SET_OFFLOAD_XSTATS_L3_STATS => 0x2;
public static UntypedInt IFLA_OFFLOAD_XSTATS_UNSPEC => 0x0;
public static UntypedInt IFLA_OFFLOAD_XSTATS_CPU_HIT => 0x1;
public static UntypedInt IFLA_OFFLOAD_XSTATS_HW_S_INFO => 0x2;
public static UntypedInt IFLA_OFFLOAD_XSTATS_L3_STATS => 0x3;
public static UntypedInt IFLA_OFFLOAD_XSTATS_HW_S_INFO_UNSPEC => 0x0;
public static UntypedInt IFLA_OFFLOAD_XSTATS_HW_S_INFO_REQUEST => 0x1;
public static UntypedInt IFLA_OFFLOAD_XSTATS_HW_S_INFO_USED => 0x2;
public static UntypedInt IFLA_XDP_UNSPEC => 0x0;
public static UntypedInt IFLA_XDP_FD => 0x1;
public static UntypedInt IFLA_XDP_ATTACHED => 0x2;
public static UntypedInt IFLA_XDP_FLAGS => 0x3;
public static UntypedInt IFLA_XDP_PROG_ID => 0x4;
public static UntypedInt IFLA_XDP_DRV_PROG_ID => 0x5;
public static UntypedInt IFLA_XDP_SKB_PROG_ID => 0x6;
public static UntypedInt IFLA_XDP_HW_PROG_ID => 0x7;
public static UntypedInt IFLA_XDP_EXPECTED_FD => 0x8;
public static UntypedInt IFLA_EVENT_NONE => 0x0;
public static UntypedInt IFLA_EVENT_REBOOT => 0x1;
public static UntypedInt IFLA_EVENT_FEATURES => 0x2;
public static UntypedInt IFLA_EVENT_BONDING_FAILOVER => 0x3;
public static UntypedInt IFLA_EVENT_NOTIFY_PEERS => 0x4;
public static UntypedInt IFLA_EVENT_IGMP_RESEND => 0x5;
public static UntypedInt IFLA_EVENT_BONDING_OPTIONS => 0x6;
public static UntypedInt IFLA_TUN_UNSPEC => 0x0;
public static UntypedInt IFLA_TUN_OWNER => 0x1;
public static UntypedInt IFLA_TUN_GROUP => 0x2;
public static UntypedInt IFLA_TUN_TYPE => 0x3;
public static UntypedInt IFLA_TUN_PI => 0x4;
public static UntypedInt IFLA_TUN_VNET_HDR => 0x5;
public static UntypedInt IFLA_TUN_PERSIST => 0x6;
public static UntypedInt IFLA_TUN_MULTI_QUEUE => 0x7;
public static UntypedInt IFLA_TUN_NUM_QUEUES => 0x8;
public static UntypedInt IFLA_TUN_NUM_DISABLED_QUEUES => 0x9;
public static UntypedInt IFLA_RMNET_UNSPEC => 0x0;
public static UntypedInt IFLA_RMNET_MUX_ID => 0x1;
public static UntypedInt IFLA_RMNET_FLAGS => 0x2;
public static UntypedInt IFLA_MCTP_UNSPEC => 0x0;
public static UntypedInt IFLA_MCTP_NET => 0x1;
public static UntypedInt IFLA_DSA_UNSPEC => 0x0;
public static UntypedInt IFLA_DSA_CONDUIT => 0x1;
public static UntypedInt IFLA_DSA_MASTER => 0x1;

public static UntypedInt NETKIT_NEXT => /* -0x1 */ -1;
public static UntypedInt NETKIT_PASS => 0x0;
public static UntypedInt NETKIT_DROP => 0x2;
public static UntypedInt NETKIT_REDIRECT => 0x7;
public static UntypedInt NETKIT_L2 => 0x0;
public static UntypedInt NETKIT_L3 => 0x1;

public static UntypedInt NF_INET_PRE_ROUTING => 0x0;
public static UntypedInt NF_INET_LOCAL_IN => 0x1;
public static UntypedInt NF_INET_FORWARD => 0x2;
public static UntypedInt NF_INET_LOCAL_OUT => 0x3;
public static UntypedInt NF_INET_POST_ROUTING => 0x4;
public static UntypedInt NF_INET_NUMHOOKS => 0x5;

public static UntypedInt NF_NETDEV_INGRESS => 0x0;
public static UntypedInt NF_NETDEV_EGRESS => 0x1;
public static UntypedInt NF_NETDEV_NUMHOOKS => 0x2;

public static UntypedInt NFPROTO_UNSPEC => 0x0;
public static UntypedInt NFPROTO_INET => 0x1;
public static UntypedInt NFPROTO_IPV4 => 0x2;
public static UntypedInt NFPROTO_ARP => 0x3;
public static UntypedInt NFPROTO_NETDEV => 0x5;
public static UntypedInt NFPROTO_BRIDGE => 0x7;
public static UntypedInt NFPROTO_IPV6 => 0xa;
public static UntypedInt NFPROTO_DECNET => 0xc;
public static UntypedInt NFPROTO_NUMPROTO => 0xd;

public static UntypedInt SO_ORIGINAL_DST => 0x50;

partial struct Nfgenmsg {
    public uint8 Nfgen_family;
    public uint8 Version;
    public uint16 Res_id;
}

public static UntypedInt NFNL_BATCH_UNSPEC => 0x0;
public static UntypedInt NFNL_BATCH_GENID => 0x1;

public static UntypedInt NFT_REG_VERDICT => 0x0;
public static UntypedInt NFT_REG_1 => 0x1;
public static UntypedInt NFT_REG_2 => 0x2;
public static UntypedInt NFT_REG_3 => 0x3;
public static UntypedInt NFT_REG_4 => 0x4;
public static UntypedInt NFT_REG32_00 => 0x8;
public static UntypedInt NFT_REG32_01 => 0x9;
public static UntypedInt NFT_REG32_02 => 0xa;
public static UntypedInt NFT_REG32_03 => 0xb;
public static UntypedInt NFT_REG32_04 => 0xc;
public static UntypedInt NFT_REG32_05 => 0xd;
public static UntypedInt NFT_REG32_06 => 0xe;
public static UntypedInt NFT_REG32_07 => 0xf;
public static UntypedInt NFT_REG32_08 => 0x10;
public static UntypedInt NFT_REG32_09 => 0x11;
public static UntypedInt NFT_REG32_10 => 0x12;
public static UntypedInt NFT_REG32_11 => 0x13;
public static UntypedInt NFT_REG32_12 => 0x14;
public static UntypedInt NFT_REG32_13 => 0x15;
public static UntypedInt NFT_REG32_14 => 0x16;
public static UntypedInt NFT_REG32_15 => 0x17;
public static UntypedInt NFT_CONTINUE => /* -0x1 */ -1;
public static UntypedInt NFT_BREAK => /* -0x2 */ -2;
public static UntypedInt NFT_JUMP => /* -0x3 */ -3;
public static UntypedInt NFT_GOTO => /* -0x4 */ -4;
public static UntypedInt NFT_RETURN => /* -0x5 */ -5;
public static UntypedInt NFT_MSG_NEWTABLE => 0x0;
public static UntypedInt NFT_MSG_GETTABLE => 0x1;
public static UntypedInt NFT_MSG_DELTABLE => 0x2;
public static UntypedInt NFT_MSG_NEWCHAIN => 0x3;
public static UntypedInt NFT_MSG_GETCHAIN => 0x4;
public static UntypedInt NFT_MSG_DELCHAIN => 0x5;
public static UntypedInt NFT_MSG_NEWRULE => 0x6;
public static UntypedInt NFT_MSG_GETRULE => 0x7;
public static UntypedInt NFT_MSG_DELRULE => 0x8;
public static UntypedInt NFT_MSG_NEWSET => 0x9;
public static UntypedInt NFT_MSG_GETSET => 0xa;
public static UntypedInt NFT_MSG_DELSET => 0xb;
public static UntypedInt NFT_MSG_NEWSETELEM => 0xc;
public static UntypedInt NFT_MSG_GETSETELEM => 0xd;
public static UntypedInt NFT_MSG_DELSETELEM => 0xe;
public static UntypedInt NFT_MSG_NEWGEN => 0xf;
public static UntypedInt NFT_MSG_GETGEN => 0x10;
public static UntypedInt NFT_MSG_TRACE => 0x11;
public static UntypedInt NFT_MSG_NEWOBJ => 0x12;
public static UntypedInt NFT_MSG_GETOBJ => 0x13;
public static UntypedInt NFT_MSG_DELOBJ => 0x14;
public static UntypedInt NFT_MSG_GETOBJ_RESET => 0x15;
public static UntypedInt NFT_MSG_NEWFLOWTABLE => 0x16;
public static UntypedInt NFT_MSG_GETFLOWTABLE => 0x17;
public static UntypedInt NFT_MSG_DELFLOWTABLE => 0x18;
public static UntypedInt NFT_MSG_GETRULE_RESET => 0x19;
public static UntypedInt NFT_MSG_MAX => 0x22;
public static UntypedInt NFTA_LIST_UNSPEC => 0x0;
public static UntypedInt NFTA_LIST_ELEM => 0x1;
public static UntypedInt NFTA_HOOK_UNSPEC => 0x0;
public static UntypedInt NFTA_HOOK_HOOKNUM => 0x1;
public static UntypedInt NFTA_HOOK_PRIORITY => 0x2;
public static UntypedInt NFTA_HOOK_DEV => 0x3;
public static UntypedInt NFT_TABLE_F_DORMANT => 0x1;
public static UntypedInt NFTA_TABLE_UNSPEC => 0x0;
public static UntypedInt NFTA_TABLE_NAME => 0x1;
public static UntypedInt NFTA_TABLE_FLAGS => 0x2;
public static UntypedInt NFTA_TABLE_USE => 0x3;
public static UntypedInt NFTA_CHAIN_UNSPEC => 0x0;
public static UntypedInt NFTA_CHAIN_TABLE => 0x1;
public static UntypedInt NFTA_CHAIN_HANDLE => 0x2;
public static UntypedInt NFTA_CHAIN_NAME => 0x3;
public static UntypedInt NFTA_CHAIN_HOOK => 0x4;
public static UntypedInt NFTA_CHAIN_POLICY => 0x5;
public static UntypedInt NFTA_CHAIN_USE => 0x6;
public static UntypedInt NFTA_CHAIN_TYPE => 0x7;
public static UntypedInt NFTA_CHAIN_COUNTERS => 0x8;
public static UntypedInt NFTA_CHAIN_PAD => 0x9;
public static UntypedInt NFTA_RULE_UNSPEC => 0x0;
public static UntypedInt NFTA_RULE_TABLE => 0x1;
public static UntypedInt NFTA_RULE_CHAIN => 0x2;
public static UntypedInt NFTA_RULE_HANDLE => 0x3;
public static UntypedInt NFTA_RULE_EXPRESSIONS => 0x4;
public static UntypedInt NFTA_RULE_COMPAT => 0x5;
public static UntypedInt NFTA_RULE_POSITION => 0x6;
public static UntypedInt NFTA_RULE_USERDATA => 0x7;
public static UntypedInt NFTA_RULE_PAD => 0x8;
public static UntypedInt NFTA_RULE_ID => 0x9;
public static UntypedInt NFT_RULE_COMPAT_F_INV => 0x2;
public static UntypedInt NFT_RULE_COMPAT_F_MASK => 0x2;
public static UntypedInt NFTA_RULE_COMPAT_UNSPEC => 0x0;
public static UntypedInt NFTA_RULE_COMPAT_PROTO => 0x1;
public static UntypedInt NFTA_RULE_COMPAT_FLAGS => 0x2;
public static UntypedInt NFT_SET_ANONYMOUS => 0x1;
public static UntypedInt NFT_SET_CONSTANT => 0x2;
public static UntypedInt NFT_SET_INTERVAL => 0x4;
public static UntypedInt NFT_SET_MAP => 0x8;
public static UntypedInt NFT_SET_TIMEOUT => 0x10;
public static UntypedInt NFT_SET_EVAL => 0x20;
public static UntypedInt NFT_SET_OBJECT => 0x40;
public static UntypedInt NFT_SET_POL_PERFORMANCE => 0x0;
public static UntypedInt NFT_SET_POL_MEMORY => 0x1;
public static UntypedInt NFTA_SET_DESC_UNSPEC => 0x0;
public static UntypedInt NFTA_SET_DESC_SIZE => 0x1;
public static UntypedInt NFTA_SET_UNSPEC => 0x0;
public static UntypedInt NFTA_SET_TABLE => 0x1;
public static UntypedInt NFTA_SET_NAME => 0x2;
public static UntypedInt NFTA_SET_FLAGS => 0x3;
public static UntypedInt NFTA_SET_KEY_TYPE => 0x4;
public static UntypedInt NFTA_SET_KEY_LEN => 0x5;
public static UntypedInt NFTA_SET_DATA_TYPE => 0x6;
public static UntypedInt NFTA_SET_DATA_LEN => 0x7;
public static UntypedInt NFTA_SET_POLICY => 0x8;
public static UntypedInt NFTA_SET_DESC => 0x9;
public static UntypedInt NFTA_SET_ID => 0xa;
public static UntypedInt NFTA_SET_TIMEOUT => 0xb;
public static UntypedInt NFTA_SET_GC_INTERVAL => 0xc;
public static UntypedInt NFTA_SET_USERDATA => 0xd;
public static UntypedInt NFTA_SET_PAD => 0xe;
public static UntypedInt NFTA_SET_OBJ_TYPE => 0xf;
public static UntypedInt NFT_SET_ELEM_INTERVAL_END => 0x1;
public static UntypedInt NFTA_SET_ELEM_UNSPEC => 0x0;
public static UntypedInt NFTA_SET_ELEM_KEY => 0x1;
public static UntypedInt NFTA_SET_ELEM_DATA => 0x2;
public static UntypedInt NFTA_SET_ELEM_FLAGS => 0x3;
public static UntypedInt NFTA_SET_ELEM_TIMEOUT => 0x4;
public static UntypedInt NFTA_SET_ELEM_EXPIRATION => 0x5;
public static UntypedInt NFTA_SET_ELEM_USERDATA => 0x6;
public static UntypedInt NFTA_SET_ELEM_EXPR => 0x7;
public static UntypedInt NFTA_SET_ELEM_PAD => 0x8;
public static UntypedInt NFTA_SET_ELEM_OBJREF => 0x9;
public static UntypedInt NFTA_SET_ELEM_LIST_UNSPEC => 0x0;
public static UntypedInt NFTA_SET_ELEM_LIST_TABLE => 0x1;
public static UntypedInt NFTA_SET_ELEM_LIST_SET => 0x2;
public static UntypedInt NFTA_SET_ELEM_LIST_ELEMENTS => 0x3;
public static UntypedInt NFTA_SET_ELEM_LIST_SET_ID => 0x4;
public static UntypedInt NFT_DATA_VALUE => 0x0;
public static UntypedInt NFT_DATA_VERDICT => 0xffffff00;
public static UntypedInt NFTA_DATA_UNSPEC => 0x0;
public static UntypedInt NFTA_DATA_VALUE => 0x1;
public static UntypedInt NFTA_DATA_VERDICT => 0x2;
public static UntypedInt NFTA_VERDICT_UNSPEC => 0x0;
public static UntypedInt NFTA_VERDICT_CODE => 0x1;
public static UntypedInt NFTA_VERDICT_CHAIN => 0x2;
public static UntypedInt NFTA_EXPR_UNSPEC => 0x0;
public static UntypedInt NFTA_EXPR_NAME => 0x1;
public static UntypedInt NFTA_EXPR_DATA => 0x2;
public static UntypedInt NFTA_IMMEDIATE_UNSPEC => 0x0;
public static UntypedInt NFTA_IMMEDIATE_DREG => 0x1;
public static UntypedInt NFTA_IMMEDIATE_DATA => 0x2;
public static UntypedInt NFTA_BITWISE_UNSPEC => 0x0;
public static UntypedInt NFTA_BITWISE_SREG => 0x1;
public static UntypedInt NFTA_BITWISE_DREG => 0x2;
public static UntypedInt NFTA_BITWISE_LEN => 0x3;
public static UntypedInt NFTA_BITWISE_MASK => 0x4;
public static UntypedInt NFTA_BITWISE_XOR => 0x5;
public static UntypedInt NFT_BYTEORDER_NTOH => 0x0;
public static UntypedInt NFT_BYTEORDER_HTON => 0x1;
public static UntypedInt NFTA_BYTEORDER_UNSPEC => 0x0;
public static UntypedInt NFTA_BYTEORDER_SREG => 0x1;
public static UntypedInt NFTA_BYTEORDER_DREG => 0x2;
public static UntypedInt NFTA_BYTEORDER_OP => 0x3;
public static UntypedInt NFTA_BYTEORDER_LEN => 0x4;
public static UntypedInt NFTA_BYTEORDER_SIZE => 0x5;
public static UntypedInt NFT_CMP_EQ => 0x0;
public static UntypedInt NFT_CMP_NEQ => 0x1;
public static UntypedInt NFT_CMP_LT => 0x2;
public static UntypedInt NFT_CMP_LTE => 0x3;
public static UntypedInt NFT_CMP_GT => 0x4;
public static UntypedInt NFT_CMP_GTE => 0x5;
public static UntypedInt NFTA_CMP_UNSPEC => 0x0;
public static UntypedInt NFTA_CMP_SREG => 0x1;
public static UntypedInt NFTA_CMP_OP => 0x2;
public static UntypedInt NFTA_CMP_DATA => 0x3;
public static UntypedInt NFT_RANGE_EQ => 0x0;
public static UntypedInt NFT_RANGE_NEQ => 0x1;
public static UntypedInt NFTA_RANGE_UNSPEC => 0x0;
public static UntypedInt NFTA_RANGE_SREG => 0x1;
public static UntypedInt NFTA_RANGE_OP => 0x2;
public static UntypedInt NFTA_RANGE_FROM_DATA => 0x3;
public static UntypedInt NFTA_RANGE_TO_DATA => 0x4;
public static UntypedInt NFT_LOOKUP_F_INV => 0x1;
public static UntypedInt NFTA_LOOKUP_UNSPEC => 0x0;
public static UntypedInt NFTA_LOOKUP_SET => 0x1;
public static UntypedInt NFTA_LOOKUP_SREG => 0x2;
public static UntypedInt NFTA_LOOKUP_DREG => 0x3;
public static UntypedInt NFTA_LOOKUP_SET_ID => 0x4;
public static UntypedInt NFTA_LOOKUP_FLAGS => 0x5;
public static UntypedInt NFT_DYNSET_OP_ADD => 0x0;
public static UntypedInt NFT_DYNSET_OP_UPDATE => 0x1;
public static UntypedInt NFT_DYNSET_F_INV => 0x1;
public static UntypedInt NFTA_DYNSET_UNSPEC => 0x0;
public static UntypedInt NFTA_DYNSET_SET_NAME => 0x1;
public static UntypedInt NFTA_DYNSET_SET_ID => 0x2;
public static UntypedInt NFTA_DYNSET_OP => 0x3;
public static UntypedInt NFTA_DYNSET_SREG_KEY => 0x4;
public static UntypedInt NFTA_DYNSET_SREG_DATA => 0x5;
public static UntypedInt NFTA_DYNSET_TIMEOUT => 0x6;
public static UntypedInt NFTA_DYNSET_EXPR => 0x7;
public static UntypedInt NFTA_DYNSET_PAD => 0x8;
public static UntypedInt NFTA_DYNSET_FLAGS => 0x9;
public static UntypedInt NFT_PAYLOAD_LL_HEADER => 0x0;
public static UntypedInt NFT_PAYLOAD_NETWORK_HEADER => 0x1;
public static UntypedInt NFT_PAYLOAD_TRANSPORT_HEADER => 0x2;
public static UntypedInt NFT_PAYLOAD_INNER_HEADER => 0x3;
public static UntypedInt NFT_PAYLOAD_TUN_HEADER => 0x4;
public static UntypedInt NFT_PAYLOAD_CSUM_NONE => 0x0;
public static UntypedInt NFT_PAYLOAD_CSUM_INET => 0x1;
public static UntypedInt NFT_PAYLOAD_CSUM_SCTP => 0x2;
public static UntypedInt NFT_PAYLOAD_L4CSUM_PSEUDOHDR => 0x1;
public static UntypedInt NFTA_PAYLOAD_UNSPEC => 0x0;
public static UntypedInt NFTA_PAYLOAD_DREG => 0x1;
public static UntypedInt NFTA_PAYLOAD_BASE => 0x2;
public static UntypedInt NFTA_PAYLOAD_OFFSET => 0x3;
public static UntypedInt NFTA_PAYLOAD_LEN => 0x4;
public static UntypedInt NFTA_PAYLOAD_SREG => 0x5;
public static UntypedInt NFTA_PAYLOAD_CSUM_TYPE => 0x6;
public static UntypedInt NFTA_PAYLOAD_CSUM_OFFSET => 0x7;
public static UntypedInt NFTA_PAYLOAD_CSUM_FLAGS => 0x8;
public static UntypedInt NFT_EXTHDR_F_PRESENT => 0x1;
public static UntypedInt NFT_EXTHDR_OP_IPV6 => 0x0;
public static UntypedInt NFT_EXTHDR_OP_TCPOPT => 0x1;
public static UntypedInt NFTA_EXTHDR_UNSPEC => 0x0;
public static UntypedInt NFTA_EXTHDR_DREG => 0x1;
public static UntypedInt NFTA_EXTHDR_TYPE => 0x2;
public static UntypedInt NFTA_EXTHDR_OFFSET => 0x3;
public static UntypedInt NFTA_EXTHDR_LEN => 0x4;
public static UntypedInt NFTA_EXTHDR_FLAGS => 0x5;
public static UntypedInt NFTA_EXTHDR_OP => 0x6;
public static UntypedInt NFTA_EXTHDR_SREG => 0x7;
public static UntypedInt NFT_META_LEN => 0x0;
public static UntypedInt NFT_META_PROTOCOL => 0x1;
public static UntypedInt NFT_META_PRIORITY => 0x2;
public static UntypedInt NFT_META_MARK => 0x3;
public static UntypedInt NFT_META_IIF => 0x4;
public static UntypedInt NFT_META_OIF => 0x5;
public static UntypedInt NFT_META_IIFNAME => 0x6;
public static UntypedInt NFT_META_OIFNAME => 0x7;
public static UntypedInt NFT_META_IIFTYPE => 0x8;
public static UntypedInt NFT_META_OIFTYPE => 0x9;
public static UntypedInt NFT_META_SKUID => 0xa;
public static UntypedInt NFT_META_SKGID => 0xb;
public static UntypedInt NFT_META_NFTRACE => 0xc;
public static UntypedInt NFT_META_RTCLASSID => 0xd;
public static UntypedInt NFT_META_SECMARK => 0xe;
public static UntypedInt NFT_META_NFPROTO => 0xf;
public static UntypedInt NFT_META_L4PROTO => 0x10;
public static UntypedInt NFT_META_BRI_IIFNAME => 0x11;
public static UntypedInt NFT_META_BRI_OIFNAME => 0x12;
public static UntypedInt NFT_META_PKTTYPE => 0x13;
public static UntypedInt NFT_META_CPU => 0x14;
public static UntypedInt NFT_META_IIFGROUP => 0x15;
public static UntypedInt NFT_META_OIFGROUP => 0x16;
public static UntypedInt NFT_META_CGROUP => 0x17;
public static UntypedInt NFT_META_PRANDOM => 0x18;
public static UntypedInt NFT_RT_CLASSID => 0x0;
public static UntypedInt NFT_RT_NEXTHOP4 => 0x1;
public static UntypedInt NFT_RT_NEXTHOP6 => 0x2;
public static UntypedInt NFT_RT_TCPMSS => 0x3;
public static UntypedInt NFT_HASH_JENKINS => 0x0;
public static UntypedInt NFT_HASH_SYM => 0x1;
public static UntypedInt NFTA_HASH_UNSPEC => 0x0;
public static UntypedInt NFTA_HASH_SREG => 0x1;
public static UntypedInt NFTA_HASH_DREG => 0x2;
public static UntypedInt NFTA_HASH_LEN => 0x3;
public static UntypedInt NFTA_HASH_MODULUS => 0x4;
public static UntypedInt NFTA_HASH_SEED => 0x5;
public static UntypedInt NFTA_HASH_OFFSET => 0x6;
public static UntypedInt NFTA_HASH_TYPE => 0x7;
public static UntypedInt NFTA_META_UNSPEC => 0x0;
public static UntypedInt NFTA_META_DREG => 0x1;
public static UntypedInt NFTA_META_KEY => 0x2;
public static UntypedInt NFTA_META_SREG => 0x3;
public static UntypedInt NFTA_RT_UNSPEC => 0x0;
public static UntypedInt NFTA_RT_DREG => 0x1;
public static UntypedInt NFTA_RT_KEY => 0x2;
public static UntypedInt NFT_CT_STATE => 0x0;
public static UntypedInt NFT_CT_DIRECTION => 0x1;
public static UntypedInt NFT_CT_STATUS => 0x2;
public static UntypedInt NFT_CT_MARK => 0x3;
public static UntypedInt NFT_CT_SECMARK => 0x4;
public static UntypedInt NFT_CT_EXPIRATION => 0x5;
public static UntypedInt NFT_CT_HELPER => 0x6;
public static UntypedInt NFT_CT_L3PROTOCOL => 0x7;
public static UntypedInt NFT_CT_SRC => 0x8;
public static UntypedInt NFT_CT_DST => 0x9;
public static UntypedInt NFT_CT_PROTOCOL => 0xa;
public static UntypedInt NFT_CT_PROTO_SRC => 0xb;
public static UntypedInt NFT_CT_PROTO_DST => 0xc;
public static UntypedInt NFT_CT_LABELS => 0xd;
public static UntypedInt NFT_CT_PKTS => 0xe;
public static UntypedInt NFT_CT_BYTES => 0xf;
public static UntypedInt NFT_CT_AVGPKT => 0x10;
public static UntypedInt NFT_CT_ZONE => 0x11;
public static UntypedInt NFT_CT_EVENTMASK => 0x12;
public static UntypedInt NFT_CT_SRC_IP => 0x13;
public static UntypedInt NFT_CT_DST_IP => 0x14;
public static UntypedInt NFT_CT_SRC_IP6 => 0x15;
public static UntypedInt NFT_CT_DST_IP6 => 0x16;
public static UntypedInt NFT_CT_ID => 0x17;
public static UntypedInt NFTA_CT_UNSPEC => 0x0;
public static UntypedInt NFTA_CT_DREG => 0x1;
public static UntypedInt NFTA_CT_KEY => 0x2;
public static UntypedInt NFTA_CT_DIRECTION => 0x3;
public static UntypedInt NFTA_CT_SREG => 0x4;
public static UntypedInt NFT_LIMIT_PKTS => 0x0;
public static UntypedInt NFT_LIMIT_PKT_BYTES => 0x1;
public static UntypedInt NFT_LIMIT_F_INV => 0x1;
public static UntypedInt NFTA_LIMIT_UNSPEC => 0x0;
public static UntypedInt NFTA_LIMIT_RATE => 0x1;
public static UntypedInt NFTA_LIMIT_UNIT => 0x2;
public static UntypedInt NFTA_LIMIT_BURST => 0x3;
public static UntypedInt NFTA_LIMIT_TYPE => 0x4;
public static UntypedInt NFTA_LIMIT_FLAGS => 0x5;
public static UntypedInt NFTA_LIMIT_PAD => 0x6;
public static UntypedInt NFTA_COUNTER_UNSPEC => 0x0;
public static UntypedInt NFTA_COUNTER_BYTES => 0x1;
public static UntypedInt NFTA_COUNTER_PACKETS => 0x2;
public static UntypedInt NFTA_COUNTER_PAD => 0x3;
public static UntypedInt NFTA_LOG_UNSPEC => 0x0;
public static UntypedInt NFTA_LOG_GROUP => 0x1;
public static UntypedInt NFTA_LOG_PREFIX => 0x2;
public static UntypedInt NFTA_LOG_SNAPLEN => 0x3;
public static UntypedInt NFTA_LOG_QTHRESHOLD => 0x4;
public static UntypedInt NFTA_LOG_LEVEL => 0x5;
public static UntypedInt NFTA_LOG_FLAGS => 0x6;
public static UntypedInt NFTA_QUEUE_UNSPEC => 0x0;
public static UntypedInt NFTA_QUEUE_NUM => 0x1;
public static UntypedInt NFTA_QUEUE_TOTAL => 0x2;
public static UntypedInt NFTA_QUEUE_FLAGS => 0x3;
public static UntypedInt NFTA_QUEUE_SREG_QNUM => 0x4;
public static UntypedInt NFT_QUOTA_F_INV => 0x1;
public static UntypedInt NFT_QUOTA_F_DEPLETED => 0x2;
public static UntypedInt NFTA_QUOTA_UNSPEC => 0x0;
public static UntypedInt NFTA_QUOTA_BYTES => 0x1;
public static UntypedInt NFTA_QUOTA_FLAGS => 0x2;
public static UntypedInt NFTA_QUOTA_PAD => 0x3;
public static UntypedInt NFTA_QUOTA_CONSUMED => 0x4;
public static UntypedInt NFT_REJECT_ICMP_UNREACH => 0x0;
public static UntypedInt NFT_REJECT_TCP_RST => 0x1;
public static UntypedInt NFT_REJECT_ICMPX_UNREACH => 0x2;
public static UntypedInt NFT_REJECT_ICMPX_NO_ROUTE => 0x0;
public static UntypedInt NFT_REJECT_ICMPX_PORT_UNREACH => 0x1;
public static UntypedInt NFT_REJECT_ICMPX_HOST_UNREACH => 0x2;
public static UntypedInt NFT_REJECT_ICMPX_ADMIN_PROHIBITED => 0x3;
public static UntypedInt NFTA_REJECT_UNSPEC => 0x0;
public static UntypedInt NFTA_REJECT_TYPE => 0x1;
public static UntypedInt NFTA_REJECT_ICMP_CODE => 0x2;
public static UntypedInt NFT_NAT_SNAT => 0x0;
public static UntypedInt NFT_NAT_DNAT => 0x1;
public static UntypedInt NFTA_NAT_UNSPEC => 0x0;
public static UntypedInt NFTA_NAT_TYPE => 0x1;
public static UntypedInt NFTA_NAT_FAMILY => 0x2;
public static UntypedInt NFTA_NAT_REG_ADDR_MIN => 0x3;
public static UntypedInt NFTA_NAT_REG_ADDR_MAX => 0x4;
public static UntypedInt NFTA_NAT_REG_PROTO_MIN => 0x5;
public static UntypedInt NFTA_NAT_REG_PROTO_MAX => 0x6;
public static UntypedInt NFTA_NAT_FLAGS => 0x7;
public static UntypedInt NFTA_MASQ_UNSPEC => 0x0;
public static UntypedInt NFTA_MASQ_FLAGS => 0x1;
public static UntypedInt NFTA_MASQ_REG_PROTO_MIN => 0x2;
public static UntypedInt NFTA_MASQ_REG_PROTO_MAX => 0x3;
public static UntypedInt NFTA_REDIR_UNSPEC => 0x0;
public static UntypedInt NFTA_REDIR_REG_PROTO_MIN => 0x1;
public static UntypedInt NFTA_REDIR_REG_PROTO_MAX => 0x2;
public static UntypedInt NFTA_REDIR_FLAGS => 0x3;
public static UntypedInt NFTA_DUP_UNSPEC => 0x0;
public static UntypedInt NFTA_DUP_SREG_ADDR => 0x1;
public static UntypedInt NFTA_DUP_SREG_DEV => 0x2;
public static UntypedInt NFTA_FWD_UNSPEC => 0x0;
public static UntypedInt NFTA_FWD_SREG_DEV => 0x1;
public static UntypedInt NFTA_OBJREF_UNSPEC => 0x0;
public static UntypedInt NFTA_OBJREF_IMM_TYPE => 0x1;
public static UntypedInt NFTA_OBJREF_IMM_NAME => 0x2;
public static UntypedInt NFTA_OBJREF_SET_SREG => 0x3;
public static UntypedInt NFTA_OBJREF_SET_NAME => 0x4;
public static UntypedInt NFTA_OBJREF_SET_ID => 0x5;
public static UntypedInt NFTA_GEN_UNSPEC => 0x0;
public static UntypedInt NFTA_GEN_ID => 0x1;
public static UntypedInt NFTA_GEN_PROC_PID => 0x2;
public static UntypedInt NFTA_GEN_PROC_NAME => 0x3;
public static UntypedInt NFTA_FIB_UNSPEC => 0x0;
public static UntypedInt NFTA_FIB_DREG => 0x1;
public static UntypedInt NFTA_FIB_RESULT => 0x2;
public static UntypedInt NFTA_FIB_FLAGS => 0x3;
public static UntypedInt NFT_FIB_RESULT_UNSPEC => 0x0;
public static UntypedInt NFT_FIB_RESULT_OIF => 0x1;
public static UntypedInt NFT_FIB_RESULT_OIFNAME => 0x2;
public static UntypedInt NFT_FIB_RESULT_ADDRTYPE => 0x3;
public static UntypedInt NFTA_FIB_F_SADDR => 0x1;
public static UntypedInt NFTA_FIB_F_DADDR => 0x2;
public static UntypedInt NFTA_FIB_F_MARK => 0x4;
public static UntypedInt NFTA_FIB_F_IIF => 0x8;
public static UntypedInt NFTA_FIB_F_OIF => 0x10;
public static UntypedInt NFTA_FIB_F_PRESENT => 0x20;
public static UntypedInt NFTA_CT_HELPER_UNSPEC => 0x0;
public static UntypedInt NFTA_CT_HELPER_NAME => 0x1;
public static UntypedInt NFTA_CT_HELPER_L3PROTO => 0x2;
public static UntypedInt NFTA_CT_HELPER_L4PROTO => 0x3;
public static UntypedInt NFTA_OBJ_UNSPEC => 0x0;
public static UntypedInt NFTA_OBJ_TABLE => 0x1;
public static UntypedInt NFTA_OBJ_NAME => 0x2;
public static UntypedInt NFTA_OBJ_TYPE => 0x3;
public static UntypedInt NFTA_OBJ_DATA => 0x4;
public static UntypedInt NFTA_OBJ_USE => 0x5;
public static UntypedInt NFTA_TRACE_UNSPEC => 0x0;
public static UntypedInt NFTA_TRACE_TABLE => 0x1;
public static UntypedInt NFTA_TRACE_CHAIN => 0x2;
public static UntypedInt NFTA_TRACE_RULE_HANDLE => 0x3;
public static UntypedInt NFTA_TRACE_TYPE => 0x4;
public static UntypedInt NFTA_TRACE_VERDICT => 0x5;
public static UntypedInt NFTA_TRACE_ID => 0x6;
public static UntypedInt NFTA_TRACE_LL_HEADER => 0x7;
public static UntypedInt NFTA_TRACE_NETWORK_HEADER => 0x8;
public static UntypedInt NFTA_TRACE_TRANSPORT_HEADER => 0x9;
public static UntypedInt NFTA_TRACE_IIF => 0xa;
public static UntypedInt NFTA_TRACE_IIFTYPE => 0xb;
public static UntypedInt NFTA_TRACE_OIF => 0xc;
public static UntypedInt NFTA_TRACE_OIFTYPE => 0xd;
public static UntypedInt NFTA_TRACE_MARK => 0xe;
public static UntypedInt NFTA_TRACE_NFPROTO => 0xf;
public static UntypedInt NFTA_TRACE_POLICY => 0x10;
public static UntypedInt NFTA_TRACE_PAD => 0x11;
public static UntypedInt NFT_TRACETYPE_UNSPEC => 0x0;
public static UntypedInt NFT_TRACETYPE_POLICY => 0x1;
public static UntypedInt NFT_TRACETYPE_RETURN => 0x2;
public static UntypedInt NFT_TRACETYPE_RULE => 0x3;
public static UntypedInt NFTA_NG_UNSPEC => 0x0;
public static UntypedInt NFTA_NG_DREG => 0x1;
public static UntypedInt NFTA_NG_MODULUS => 0x2;
public static UntypedInt NFTA_NG_TYPE => 0x3;
public static UntypedInt NFTA_NG_OFFSET => 0x4;
public static UntypedInt NFT_NG_INCREMENTAL => 0x0;
public static UntypedInt NFT_NG_RANDOM => 0x1;

public static UntypedInt NFTA_TARGET_UNSPEC => 0x0;
public static UntypedInt NFTA_TARGET_NAME => 0x1;
public static UntypedInt NFTA_TARGET_REV => 0x2;
public static UntypedInt NFTA_TARGET_INFO => 0x3;
public static UntypedInt NFTA_MATCH_UNSPEC => 0x0;
public static UntypedInt NFTA_MATCH_NAME => 0x1;
public static UntypedInt NFTA_MATCH_REV => 0x2;
public static UntypedInt NFTA_MATCH_INFO => 0x3;
public static UntypedInt NFTA_COMPAT_UNSPEC => 0x0;
public static UntypedInt NFTA_COMPAT_NAME => 0x1;
public static UntypedInt NFTA_COMPAT_REV => 0x2;
public static UntypedInt NFTA_COMPAT_TYPE => 0x3;

partial struct RTCTime {
    public int32 Sec;
    public int32 Min;
    public int32 Hour;
    public int32 Mday;
    public int32 Mon;
    public int32 Year;
    public int32 Wday;
    public int32 Yday;
    public int32 Isdst;
}

partial struct RTCWkAlrm {
    public uint8 Enabled;
    public uint8 Pending;
    public RTCTime Time;
}

partial struct BlkpgIoctlArg {
    public int32 Op;
    public int32 Flags;
    public int32 Datalen;
    public ж<byte> Data;
}

public static UntypedInt BLKPG_ADD_PARTITION => 0x1;
public static UntypedInt BLKPG_DEL_PARTITION => 0x2;
public static UntypedInt BLKPG_RESIZE_PARTITION => 0x3;

public static UntypedInt NETNSA_NONE => 0x0;
public static UntypedInt NETNSA_NSID => 0x1;
public static UntypedInt NETNSA_PID => 0x2;
public static UntypedInt NETNSA_FD => 0x3;
public static UntypedInt NETNSA_TARGET_NSID => 0x4;
public static UntypedInt NETNSA_CURRENT_NSID => 0x5;

partial struct XDPRingOffset {
    public uint64 Producer;
    public uint64 Consumer;
    public uint64 Desc;
    public uint64 Flags;
}

partial struct XDPMmapOffsets {
    public XDPRingOffset Rx;
    public XDPRingOffset Tx;
    public XDPRingOffset Fr;
    public XDPRingOffset Cr;
}

partial struct XDPUmemReg {
    public uint64 Addr;
    public uint64 Len;
    public uint32 Size;
    public uint32 Headroom;
    public uint32 Flags;
    public uint32 Tx_metadata_len;
}

partial struct XDPStatistics {
    public uint64 Rx_dropped;
    public uint64 Rx_invalid_descs;
    public uint64 Tx_invalid_descs;
    public uint64 Rx_ring_full;
    public uint64 Rx_fill_ring_empty_descs;
    public uint64 Tx_ring_empty_descs;
}

partial struct XDPDesc {
    public uint64 Addr;
    public uint32 Len;
    public uint32 Options;
}

public static UntypedInt NCSI_CMD_UNSPEC => 0x0;
public static UntypedInt NCSI_CMD_PKG_INFO => 0x1;
public static UntypedInt NCSI_CMD_SET_INTERFACE => 0x2;
public static UntypedInt NCSI_CMD_CLEAR_INTERFACE => 0x3;
public static UntypedInt NCSI_ATTR_UNSPEC => 0x0;
public static UntypedInt NCSI_ATTR_IFINDEX => 0x1;
public static UntypedInt NCSI_ATTR_PACKAGE_LIST => 0x2;
public static UntypedInt NCSI_ATTR_PACKAGE_ID => 0x3;
public static UntypedInt NCSI_ATTR_CHANNEL_ID => 0x4;
public static UntypedInt NCSI_PKG_ATTR_UNSPEC => 0x0;
public static UntypedInt NCSI_PKG_ATTR => 0x1;
public static UntypedInt NCSI_PKG_ATTR_ID => 0x2;
public static UntypedInt NCSI_PKG_ATTR_FORCED => 0x3;
public static UntypedInt NCSI_PKG_ATTR_CHANNEL_LIST => 0x4;
public static UntypedInt NCSI_CHANNEL_ATTR_UNSPEC => 0x0;
public static UntypedInt NCSI_CHANNEL_ATTR => 0x1;
public static UntypedInt NCSI_CHANNEL_ATTR_ID => 0x2;
public static UntypedInt NCSI_CHANNEL_ATTR_VERSION_MAJOR => 0x3;
public static UntypedInt NCSI_CHANNEL_ATTR_VERSION_MINOR => 0x4;
public static UntypedInt NCSI_CHANNEL_ATTR_VERSION_STR => 0x5;
public static UntypedInt NCSI_CHANNEL_ATTR_LINK_STATE => 0x6;
public static UntypedInt NCSI_CHANNEL_ATTR_ACTIVE => 0x7;
public static UntypedInt NCSI_CHANNEL_ATTR_FORCED => 0x8;
public static UntypedInt NCSI_CHANNEL_ATTR_VLAN_LIST => 0x9;
public static UntypedInt NCSI_CHANNEL_ATTR_VLAN_ID => 0xa;

partial struct ScmTimestamping {
    public array<Timespec> Ts = new(3);
}

public static UntypedInt SOF_TIMESTAMPING_TX_HARDWARE => 0x1;
public static UntypedInt SOF_TIMESTAMPING_TX_SOFTWARE => 0x2;
public static UntypedInt SOF_TIMESTAMPING_RX_HARDWARE => 0x4;
public static UntypedInt SOF_TIMESTAMPING_RX_SOFTWARE => 0x8;
public static UntypedInt SOF_TIMESTAMPING_SOFTWARE => 0x10;
public static UntypedInt SOF_TIMESTAMPING_SYS_HARDWARE => 0x20;
public static UntypedInt SOF_TIMESTAMPING_RAW_HARDWARE => 0x40;
public static UntypedInt SOF_TIMESTAMPING_OPT_ID => 0x80;
public static UntypedInt SOF_TIMESTAMPING_TX_SCHED => 0x100;
public static UntypedInt SOF_TIMESTAMPING_TX_ACK => 0x200;
public static UntypedInt SOF_TIMESTAMPING_OPT_CMSG => 0x400;
public static UntypedInt SOF_TIMESTAMPING_OPT_TSONLY => 0x800;
public static UntypedInt SOF_TIMESTAMPING_OPT_STATS => 0x1000;
public static UntypedInt SOF_TIMESTAMPING_OPT_PKTINFO => 0x2000;
public static UntypedInt SOF_TIMESTAMPING_OPT_TX_SWHW => 0x4000;
public static UntypedInt SOF_TIMESTAMPING_BIND_PHC => 0x8000;
public static UntypedInt SOF_TIMESTAMPING_OPT_ID_TCP => 0x10000;
public static UntypedInt SOF_TIMESTAMPING_LAST => 0x40000;
public static UntypedInt SOF_TIMESTAMPING_MASK => 0x7ffff;
public static UntypedInt SCM_TSTAMP_SND => 0x0;
public static UntypedInt SCM_TSTAMP_SCHED => 0x1;
public static UntypedInt SCM_TSTAMP_ACK => 0x2;

partial struct SockExtendedErr {
    public uint32 Errno;
    public uint8 Origin;
    public uint8 Type;
    public uint8 Code;
    public uint8 Pad;
    public uint32 Info;
    public uint32 Data;
}

partial struct FanotifyEventMetadata {
    public uint32 Event_len;
    public uint8 Vers;
    public uint8 Reserved;
    public uint16 Metadata_len;
    public uint64 Mask;
    public int32 Fd;
    public int32 Pid;
}

partial struct FanotifyResponse {
    public int32 Fd;
    public uint32 Response;
}

public static UntypedInt CRYPTO_MSG_BASE => 0x10;
public static UntypedInt CRYPTO_MSG_NEWALG => 0x10;
public static UntypedInt CRYPTO_MSG_DELALG => 0x11;
public static UntypedInt CRYPTO_MSG_UPDATEALG => 0x12;
public static UntypedInt CRYPTO_MSG_GETALG => 0x13;
public static UntypedInt CRYPTO_MSG_DELRNG => 0x14;
public static UntypedInt CRYPTO_MSG_GETSTAT => 0x15;

public static UntypedInt CRYPTOCFGA_UNSPEC => 0x0;
public static UntypedInt CRYPTOCFGA_PRIORITY_VAL => 0x1;
public static UntypedInt CRYPTOCFGA_REPORT_LARVAL => 0x2;
public static UntypedInt CRYPTOCFGA_REPORT_HASH => 0x3;
public static UntypedInt CRYPTOCFGA_REPORT_BLKCIPHER => 0x4;
public static UntypedInt CRYPTOCFGA_REPORT_AEAD => 0x5;
public static UntypedInt CRYPTOCFGA_REPORT_COMPRESS => 0x6;
public static UntypedInt CRYPTOCFGA_REPORT_RNG => 0x7;
public static UntypedInt CRYPTOCFGA_REPORT_CIPHER => 0x8;
public static UntypedInt CRYPTOCFGA_REPORT_AKCIPHER => 0x9;
public static UntypedInt CRYPTOCFGA_REPORT_KPP => 0xa;
public static UntypedInt CRYPTOCFGA_REPORT_ACOMP => 0xb;
public static UntypedInt CRYPTOCFGA_STAT_LARVAL => 0xc;
public static UntypedInt CRYPTOCFGA_STAT_HASH => 0xd;
public static UntypedInt CRYPTOCFGA_STAT_BLKCIPHER => 0xe;
public static UntypedInt CRYPTOCFGA_STAT_AEAD => 0xf;
public static UntypedInt CRYPTOCFGA_STAT_COMPRESS => 0x10;
public static UntypedInt CRYPTOCFGA_STAT_RNG => 0x11;
public static UntypedInt CRYPTOCFGA_STAT_CIPHER => 0x12;
public static UntypedInt CRYPTOCFGA_STAT_AKCIPHER => 0x13;
public static UntypedInt CRYPTOCFGA_STAT_KPP => 0x14;
public static UntypedInt CRYPTOCFGA_STAT_ACOMP => 0x15;

public static UntypedInt BPF_REG_0 => 0x0;
public static UntypedInt BPF_REG_1 => 0x1;
public static UntypedInt BPF_REG_2 => 0x2;
public static UntypedInt BPF_REG_3 => 0x3;
public static UntypedInt BPF_REG_4 => 0x4;
public static UntypedInt BPF_REG_5 => 0x5;
public static UntypedInt BPF_REG_6 => 0x6;
public static UntypedInt BPF_REG_7 => 0x7;
public static UntypedInt BPF_REG_8 => 0x8;
public static UntypedInt BPF_REG_9 => 0x9;
public static UntypedInt BPF_REG_10 => 0xa;
public static UntypedInt BPF_CGROUP_ITER_ORDER_UNSPEC => 0x0;
public static UntypedInt BPF_CGROUP_ITER_SELF_ONLY => 0x1;
public static UntypedInt BPF_CGROUP_ITER_DESCENDANTS_PRE => 0x2;
public static UntypedInt BPF_CGROUP_ITER_DESCENDANTS_POST => 0x3;
public static UntypedInt BPF_CGROUP_ITER_ANCESTORS_UP => 0x4;
public static UntypedInt BPF_MAP_CREATE => 0x0;
public static UntypedInt BPF_MAP_LOOKUP_ELEM => 0x1;
public static UntypedInt BPF_MAP_UPDATE_ELEM => 0x2;
public static UntypedInt BPF_MAP_DELETE_ELEM => 0x3;
public static UntypedInt BPF_MAP_GET_NEXT_KEY => 0x4;
public static UntypedInt BPF_PROG_LOAD => 0x5;
public static UntypedInt BPF_OBJ_PIN => 0x6;
public static UntypedInt BPF_OBJ_GET => 0x7;
public static UntypedInt BPF_PROG_ATTACH => 0x8;
public static UntypedInt BPF_PROG_DETACH => 0x9;
public static UntypedInt BPF_PROG_TEST_RUN => 0xa;
public static UntypedInt BPF_PROG_RUN => 0xa;
public static UntypedInt BPF_PROG_GET_NEXT_ID => 0xb;
public static UntypedInt BPF_MAP_GET_NEXT_ID => 0xc;
public static UntypedInt BPF_PROG_GET_FD_BY_ID => 0xd;
public static UntypedInt BPF_MAP_GET_FD_BY_ID => 0xe;
public static UntypedInt BPF_OBJ_GET_INFO_BY_FD => 0xf;
public static UntypedInt BPF_PROG_QUERY => 0x10;
public static UntypedInt BPF_RAW_TRACEPOINT_OPEN => 0x11;
public static UntypedInt BPF_BTF_LOAD => 0x12;
public static UntypedInt BPF_BTF_GET_FD_BY_ID => 0x13;
public static UntypedInt BPF_TASK_FD_QUERY => 0x14;
public static UntypedInt BPF_MAP_LOOKUP_AND_DELETE_ELEM => 0x15;
public static UntypedInt BPF_MAP_FREEZE => 0x16;
public static UntypedInt BPF_BTF_GET_NEXT_ID => 0x17;
public static UntypedInt BPF_MAP_LOOKUP_BATCH => 0x18;
public static UntypedInt BPF_MAP_LOOKUP_AND_DELETE_BATCH => 0x19;
public static UntypedInt BPF_MAP_UPDATE_BATCH => 0x1a;
public static UntypedInt BPF_MAP_DELETE_BATCH => 0x1b;
public static UntypedInt BPF_LINK_CREATE => 0x1c;
public static UntypedInt BPF_LINK_UPDATE => 0x1d;
public static UntypedInt BPF_LINK_GET_FD_BY_ID => 0x1e;
public static UntypedInt BPF_LINK_GET_NEXT_ID => 0x1f;
public static UntypedInt BPF_ENABLE_STATS => 0x20;
public static UntypedInt BPF_ITER_CREATE => 0x21;
public static UntypedInt BPF_LINK_DETACH => 0x22;
public static UntypedInt BPF_PROG_BIND_MAP => 0x23;
public static UntypedInt BPF_MAP_TYPE_UNSPEC => 0x0;
public static UntypedInt BPF_MAP_TYPE_HASH => 0x1;
public static UntypedInt BPF_MAP_TYPE_ARRAY => 0x2;
public static UntypedInt BPF_MAP_TYPE_PROG_ARRAY => 0x3;
public static UntypedInt BPF_MAP_TYPE_PERF_EVENT_ARRAY => 0x4;
public static UntypedInt BPF_MAP_TYPE_PERCPU_HASH => 0x5;
public static UntypedInt BPF_MAP_TYPE_PERCPU_ARRAY => 0x6;
public static UntypedInt BPF_MAP_TYPE_STACK_TRACE => 0x7;
public static UntypedInt BPF_MAP_TYPE_CGROUP_ARRAY => 0x8;
public static UntypedInt BPF_MAP_TYPE_LRU_HASH => 0x9;
public static UntypedInt BPF_MAP_TYPE_LRU_PERCPU_HASH => 0xa;
public static UntypedInt BPF_MAP_TYPE_LPM_TRIE => 0xb;
public static UntypedInt BPF_MAP_TYPE_ARRAY_OF_MAPS => 0xc;
public static UntypedInt BPF_MAP_TYPE_HASH_OF_MAPS => 0xd;
public static UntypedInt BPF_MAP_TYPE_DEVMAP => 0xe;
public static UntypedInt BPF_MAP_TYPE_SOCKMAP => 0xf;
public static UntypedInt BPF_MAP_TYPE_CPUMAP => 0x10;
public static UntypedInt BPF_MAP_TYPE_XSKMAP => 0x11;
public static UntypedInt BPF_MAP_TYPE_SOCKHASH => 0x12;
public static UntypedInt BPF_MAP_TYPE_CGROUP_STORAGE_DEPRECATED => 0x13;
public static UntypedInt BPF_MAP_TYPE_CGROUP_STORAGE => 0x13;
public static UntypedInt BPF_MAP_TYPE_REUSEPORT_SOCKARRAY => 0x14;
public static UntypedInt BPF_MAP_TYPE_PERCPU_CGROUP_STORAGE => 0x15;
public static UntypedInt BPF_MAP_TYPE_QUEUE => 0x16;
public static UntypedInt BPF_MAP_TYPE_STACK => 0x17;
public static UntypedInt BPF_MAP_TYPE_SK_STORAGE => 0x18;
public static UntypedInt BPF_MAP_TYPE_DEVMAP_HASH => 0x19;
public static UntypedInt BPF_MAP_TYPE_STRUCT_OPS => 0x1a;
public static UntypedInt BPF_MAP_TYPE_RINGBUF => 0x1b;
public static UntypedInt BPF_MAP_TYPE_INODE_STORAGE => 0x1c;
public static UntypedInt BPF_MAP_TYPE_TASK_STORAGE => 0x1d;
public static UntypedInt BPF_MAP_TYPE_BLOOM_FILTER => 0x1e;
public static UntypedInt BPF_MAP_TYPE_USER_RINGBUF => 0x1f;
public static UntypedInt BPF_MAP_TYPE_CGRP_STORAGE => 0x20;
public static UntypedInt BPF_PROG_TYPE_UNSPEC => 0x0;
public static UntypedInt BPF_PROG_TYPE_SOCKET_FILTER => 0x1;
public static UntypedInt BPF_PROG_TYPE_KPROBE => 0x2;
public static UntypedInt BPF_PROG_TYPE_SCHED_CLS => 0x3;
public static UntypedInt BPF_PROG_TYPE_SCHED_ACT => 0x4;
public static UntypedInt BPF_PROG_TYPE_TRACEPOINT => 0x5;
public static UntypedInt BPF_PROG_TYPE_XDP => 0x6;
public static UntypedInt BPF_PROG_TYPE_PERF_EVENT => 0x7;
public static UntypedInt BPF_PROG_TYPE_CGROUP_SKB => 0x8;
public static UntypedInt BPF_PROG_TYPE_CGROUP_SOCK => 0x9;
public static UntypedInt BPF_PROG_TYPE_LWT_IN => 0xa;
public static UntypedInt BPF_PROG_TYPE_LWT_OUT => 0xb;
public static UntypedInt BPF_PROG_TYPE_LWT_XMIT => 0xc;
public static UntypedInt BPF_PROG_TYPE_SOCK_OPS => 0xd;
public static UntypedInt BPF_PROG_TYPE_SK_SKB => 0xe;
public static UntypedInt BPF_PROG_TYPE_CGROUP_DEVICE => 0xf;
public static UntypedInt BPF_PROG_TYPE_SK_MSG => 0x10;
public static UntypedInt BPF_PROG_TYPE_RAW_TRACEPOINT => 0x11;
public static UntypedInt BPF_PROG_TYPE_CGROUP_SOCK_ADDR => 0x12;
public static UntypedInt BPF_PROG_TYPE_LWT_SEG6LOCAL => 0x13;
public static UntypedInt BPF_PROG_TYPE_LIRC_MODE2 => 0x14;
public static UntypedInt BPF_PROG_TYPE_SK_REUSEPORT => 0x15;
public static UntypedInt BPF_PROG_TYPE_FLOW_DISSECTOR => 0x16;
public static UntypedInt BPF_PROG_TYPE_CGROUP_SYSCTL => 0x17;
public static UntypedInt BPF_PROG_TYPE_RAW_TRACEPOINT_WRITABLE => 0x18;
public static UntypedInt BPF_PROG_TYPE_CGROUP_SOCKOPT => 0x19;
public static UntypedInt BPF_PROG_TYPE_TRACING => 0x1a;
public static UntypedInt BPF_PROG_TYPE_STRUCT_OPS => 0x1b;
public static UntypedInt BPF_PROG_TYPE_EXT => 0x1c;
public static UntypedInt BPF_PROG_TYPE_LSM => 0x1d;
public static UntypedInt BPF_PROG_TYPE_SK_LOOKUP => 0x1e;
public static UntypedInt BPF_PROG_TYPE_SYSCALL => 0x1f;
public static UntypedInt BPF_PROG_TYPE_NETFILTER => 0x20;
public static UntypedInt BPF_CGROUP_INET_INGRESS => 0x0;
public static UntypedInt BPF_CGROUP_INET_EGRESS => 0x1;
public static UntypedInt BPF_CGROUP_INET_SOCK_CREATE => 0x2;
public static UntypedInt BPF_CGROUP_SOCK_OPS => 0x3;
public static UntypedInt BPF_SK_SKB_STREAM_PARSER => 0x4;
public static UntypedInt BPF_SK_SKB_STREAM_VERDICT => 0x5;
public static UntypedInt BPF_CGROUP_DEVICE => 0x6;
public static UntypedInt BPF_SK_MSG_VERDICT => 0x7;
public static UntypedInt BPF_CGROUP_INET4_BIND => 0x8;
public static UntypedInt BPF_CGROUP_INET6_BIND => 0x9;
public static UntypedInt BPF_CGROUP_INET4_CONNECT => 0xa;
public static UntypedInt BPF_CGROUP_INET6_CONNECT => 0xb;
public static UntypedInt BPF_CGROUP_INET4_POST_BIND => 0xc;
public static UntypedInt BPF_CGROUP_INET6_POST_BIND => 0xd;
public static UntypedInt BPF_CGROUP_UDP4_SENDMSG => 0xe;
public static UntypedInt BPF_CGROUP_UDP6_SENDMSG => 0xf;
public static UntypedInt BPF_LIRC_MODE2 => 0x10;
public static UntypedInt BPF_FLOW_DISSECTOR => 0x11;
public static UntypedInt BPF_CGROUP_SYSCTL => 0x12;
public static UntypedInt BPF_CGROUP_UDP4_RECVMSG => 0x13;
public static UntypedInt BPF_CGROUP_UDP6_RECVMSG => 0x14;
public static UntypedInt BPF_CGROUP_GETSOCKOPT => 0x15;
public static UntypedInt BPF_CGROUP_SETSOCKOPT => 0x16;
public static UntypedInt BPF_TRACE_RAW_TP => 0x17;
public static UntypedInt BPF_TRACE_FENTRY => 0x18;
public static UntypedInt BPF_TRACE_FEXIT => 0x19;
public static UntypedInt BPF_MODIFY_RETURN => 0x1a;
public static UntypedInt BPF_LSM_MAC => 0x1b;
public static UntypedInt BPF_TRACE_ITER => 0x1c;
public static UntypedInt BPF_CGROUP_INET4_GETPEERNAME => 0x1d;
public static UntypedInt BPF_CGROUP_INET6_GETPEERNAME => 0x1e;
public static UntypedInt BPF_CGROUP_INET4_GETSOCKNAME => 0x1f;
public static UntypedInt BPF_CGROUP_INET6_GETSOCKNAME => 0x20;
public static UntypedInt BPF_XDP_DEVMAP => 0x21;
public static UntypedInt BPF_CGROUP_INET_SOCK_RELEASE => 0x22;
public static UntypedInt BPF_XDP_CPUMAP => 0x23;
public static UntypedInt BPF_SK_LOOKUP => 0x24;
public static UntypedInt BPF_XDP => 0x25;
public static UntypedInt BPF_SK_SKB_VERDICT => 0x26;
public static UntypedInt BPF_SK_REUSEPORT_SELECT => 0x27;
public static UntypedInt BPF_SK_REUSEPORT_SELECT_OR_MIGRATE => 0x28;
public static UntypedInt BPF_PERF_EVENT => 0x29;
public static UntypedInt BPF_TRACE_KPROBE_MULTI => 0x2a;
public static UntypedInt BPF_LSM_CGROUP => 0x2b;
public static UntypedInt BPF_STRUCT_OPS => 0x2c;
public static UntypedInt BPF_NETFILTER => 0x2d;
public static UntypedInt BPF_TCX_INGRESS => 0x2e;
public static UntypedInt BPF_TCX_EGRESS => 0x2f;
public static UntypedInt BPF_TRACE_UPROBE_MULTI => 0x30;
public static UntypedInt BPF_LINK_TYPE_UNSPEC => 0x0;
public static UntypedInt BPF_LINK_TYPE_RAW_TRACEPOINT => 0x1;
public static UntypedInt BPF_LINK_TYPE_TRACING => 0x2;
public static UntypedInt BPF_LINK_TYPE_CGROUP => 0x3;
public static UntypedInt BPF_LINK_TYPE_ITER => 0x4;
public static UntypedInt BPF_LINK_TYPE_NETNS => 0x5;
public static UntypedInt BPF_LINK_TYPE_XDP => 0x6;
public static UntypedInt BPF_LINK_TYPE_PERF_EVENT => 0x7;
public static UntypedInt BPF_LINK_TYPE_KPROBE_MULTI => 0x8;
public static UntypedInt BPF_LINK_TYPE_STRUCT_OPS => 0x9;
public static UntypedInt BPF_LINK_TYPE_NETFILTER => 0xa;
public static UntypedInt BPF_LINK_TYPE_TCX => 0xb;
public static UntypedInt BPF_LINK_TYPE_UPROBE_MULTI => 0xc;
public static UntypedInt BPF_PERF_EVENT_UNSPEC => 0x0;
public static UntypedInt BPF_PERF_EVENT_UPROBE => 0x1;
public static UntypedInt BPF_PERF_EVENT_URETPROBE => 0x2;
public static UntypedInt BPF_PERF_EVENT_KPROBE => 0x3;
public static UntypedInt BPF_PERF_EVENT_KRETPROBE => 0x4;
public static UntypedInt BPF_PERF_EVENT_TRACEPOINT => 0x5;
public static UntypedInt BPF_PERF_EVENT_EVENT => 0x6;
public static UntypedInt BPF_F_KPROBE_MULTI_RETURN => 0x1;
public static UntypedInt BPF_F_UPROBE_MULTI_RETURN => 0x1;
public static UntypedInt BPF_ANY => 0x0;
public static UntypedInt BPF_NOEXIST => 0x1;
public static UntypedInt BPF_EXIST => 0x2;
public static UntypedInt BPF_F_LOCK => 0x4;
public static UntypedInt BPF_F_NO_PREALLOC => 0x1;
public static UntypedInt BPF_F_NO_COMMON_LRU => 0x2;
public static UntypedInt BPF_F_NUMA_NODE => 0x4;
public static UntypedInt BPF_F_RDONLY => 0x8;
public static UntypedInt BPF_F_WRONLY => 0x10;
public static UntypedInt BPF_F_STACK_BUILD_ID => 0x20;
public static UntypedInt BPF_F_ZERO_SEED => 0x40;
public static UntypedInt BPF_F_RDONLY_PROG => 0x80;
public static UntypedInt BPF_F_WRONLY_PROG => 0x100;
public static UntypedInt BPF_F_CLONE => 0x200;
public static UntypedInt BPF_F_MMAPABLE => 0x400;
public static UntypedInt BPF_F_PRESERVE_ELEMS => 0x800;
public static UntypedInt BPF_F_INNER_MAP => 0x1000;
public static UntypedInt BPF_F_LINK => 0x2000;
public static UntypedInt BPF_F_PATH_FD => 0x4000;
public static UntypedInt BPF_STATS_RUN_TIME => 0x0;
public static UntypedInt BPF_STACK_BUILD_ID_EMPTY => 0x0;
public static UntypedInt BPF_STACK_BUILD_ID_VALID => 0x1;
public static UntypedInt BPF_STACK_BUILD_ID_IP => 0x2;
public static UntypedInt BPF_F_RECOMPUTE_CSUM => 0x1;
public static UntypedInt BPF_F_INVALIDATE_HASH => 0x2;
public static UntypedInt BPF_F_HDR_FIELD_MASK => 0xf;
public static UntypedInt BPF_F_PSEUDO_HDR => 0x10;
public static UntypedInt BPF_F_MARK_MANGLED_0 => 0x20;
public static UntypedInt BPF_F_MARK_ENFORCE => 0x40;
public static UntypedInt BPF_F_INGRESS => 0x1;
public static UntypedInt BPF_F_TUNINFO_IPV6 => 0x1;
public static UntypedInt BPF_F_SKIP_FIELD_MASK => 0xff;
public static UntypedInt BPF_F_USER_STACK => 0x100;
public static UntypedInt BPF_F_FAST_STACK_CMP => 0x200;
public static UntypedInt BPF_F_REUSE_STACKID => 0x400;
public static UntypedInt BPF_F_USER_BUILD_ID => 0x800;
public static UntypedInt BPF_F_ZERO_CSUM_TX => 0x2;
public static UntypedInt BPF_F_DONT_FRAGMENT => 0x4;
public static UntypedInt BPF_F_SEQ_NUMBER => 0x8;
public static UntypedInt BPF_F_NO_TUNNEL_KEY => 0x10;
public static UntypedInt BPF_F_TUNINFO_FLAGS => 0x10;
public static UntypedInt BPF_F_INDEX_MASK => 0xffffffff;
public static UntypedInt BPF_F_CURRENT_CPU => 0xffffffff;
public static UntypedInt BPF_F_CTXLEN_MASK => 0xfffff00000000;
public static UntypedInt BPF_F_CURRENT_NETNS => /* -0x1 */ -1;
public static UntypedInt BPF_CSUM_LEVEL_QUERY => 0x0;
public static UntypedInt BPF_CSUM_LEVEL_INC => 0x1;
public static UntypedInt BPF_CSUM_LEVEL_DEC => 0x2;
public static UntypedInt BPF_CSUM_LEVEL_RESET => 0x3;
public static UntypedInt BPF_F_ADJ_ROOM_FIXED_GSO => 0x1;
public static UntypedInt BPF_F_ADJ_ROOM_ENCAP_L3_IPV4 => 0x2;
public static UntypedInt BPF_F_ADJ_ROOM_ENCAP_L3_IPV6 => 0x4;
public static UntypedInt BPF_F_ADJ_ROOM_ENCAP_L4_GRE => 0x8;
public static UntypedInt BPF_F_ADJ_ROOM_ENCAP_L4_UDP => 0x10;
public static UntypedInt BPF_F_ADJ_ROOM_NO_CSUM_RESET => 0x20;
public static UntypedInt BPF_F_ADJ_ROOM_ENCAP_L2_ETH => 0x40;
public static UntypedInt BPF_F_ADJ_ROOM_DECAP_L3_IPV4 => 0x80;
public static UntypedInt BPF_F_ADJ_ROOM_DECAP_L3_IPV6 => 0x100;
public static UntypedInt BPF_ADJ_ROOM_ENCAP_L2_MASK => 0xff;
public static UntypedInt BPF_ADJ_ROOM_ENCAP_L2_SHIFT => 0x38;
public static UntypedInt BPF_F_SYSCTL_BASE_NAME => 0x1;
public static UntypedInt BPF_LOCAL_STORAGE_GET_F_CREATE => 0x1;
public static UntypedInt BPF_SK_STORAGE_GET_F_CREATE => 0x1;
public static UntypedInt BPF_F_GET_BRANCH_RECORDS_SIZE => 0x1;
public static UntypedInt BPF_RB_NO_WAKEUP => 0x1;
public static UntypedInt BPF_RB_FORCE_WAKEUP => 0x2;
public static UntypedInt BPF_RB_AVAIL_DATA => 0x0;
public static UntypedInt BPF_RB_RING_SIZE => 0x1;
public static UntypedInt BPF_RB_CONS_POS => 0x2;
public static UntypedInt BPF_RB_PROD_POS => 0x3;
public static UntypedInt BPF_RINGBUF_BUSY_BIT => 0x80000000;
public static UntypedInt BPF_RINGBUF_DISCARD_BIT => 0x40000000;
public static UntypedInt BPF_RINGBUF_HDR_SZ => 0x8;
public static UntypedInt BPF_SK_LOOKUP_F_REPLACE => 0x1;
public static UntypedInt BPF_SK_LOOKUP_F_NO_REUSEPORT => 0x2;
public static UntypedInt BPF_ADJ_ROOM_NET => 0x0;
public static UntypedInt BPF_ADJ_ROOM_MAC => 0x1;
public static UntypedInt BPF_HDR_START_MAC => 0x0;
public static UntypedInt BPF_HDR_START_NET => 0x1;
public static UntypedInt BPF_LWT_ENCAP_SEG6 => 0x0;
public static UntypedInt BPF_LWT_ENCAP_SEG6_INLINE => 0x1;
public static UntypedInt BPF_LWT_ENCAP_IP => 0x2;
public static UntypedInt BPF_F_BPRM_SECUREEXEC => 0x1;
public static UntypedInt BPF_F_BROADCAST => 0x8;
public static UntypedInt BPF_F_EXCLUDE_INGRESS => 0x10;
public static UntypedInt BPF_SKB_TSTAMP_UNSPEC => 0x0;
public static UntypedInt BPF_SKB_TSTAMP_DELIVERY_MONO => 0x1;
public static UntypedInt BPF_OK => 0x0;
public static UntypedInt BPF_DROP => 0x2;
public static UntypedInt BPF_REDIRECT => 0x7;
public static UntypedInt BPF_LWT_REROUTE => 0x80;
public static UntypedInt BPF_FLOW_DISSECTOR_CONTINUE => 0x81;
public static UntypedInt BPF_SOCK_OPS_RTO_CB_FLAG => 0x1;
public static UntypedInt BPF_SOCK_OPS_RETRANS_CB_FLAG => 0x2;
public static UntypedInt BPF_SOCK_OPS_STATE_CB_FLAG => 0x4;
public static UntypedInt BPF_SOCK_OPS_RTT_CB_FLAG => 0x8;
public static UntypedInt BPF_SOCK_OPS_PARSE_ALL_HDR_OPT_CB_FLAG => 0x10;
public static UntypedInt BPF_SOCK_OPS_PARSE_UNKNOWN_HDR_OPT_CB_FLAG => 0x20;
public static UntypedInt BPF_SOCK_OPS_WRITE_HDR_OPT_CB_FLAG => 0x40;
public static UntypedInt BPF_SOCK_OPS_ALL_CB_FLAGS => 0x7f;
public static UntypedInt BPF_SOCK_OPS_VOID => 0x0;
public static UntypedInt BPF_SOCK_OPS_TIMEOUT_INIT => 0x1;
public static UntypedInt BPF_SOCK_OPS_RWND_INIT => 0x2;
public static UntypedInt BPF_SOCK_OPS_TCP_CONNECT_CB => 0x3;
public static UntypedInt BPF_SOCK_OPS_ACTIVE_ESTABLISHED_CB => 0x4;
public static UntypedInt BPF_SOCK_OPS_PASSIVE_ESTABLISHED_CB => 0x5;
public static UntypedInt BPF_SOCK_OPS_NEEDS_ECN => 0x6;
public static UntypedInt BPF_SOCK_OPS_BASE_RTT => 0x7;
public static UntypedInt BPF_SOCK_OPS_RTO_CB => 0x8;
public static UntypedInt BPF_SOCK_OPS_RETRANS_CB => 0x9;
public static UntypedInt BPF_SOCK_OPS_STATE_CB => 0xa;
public static UntypedInt BPF_SOCK_OPS_TCP_LISTEN_CB => 0xb;
public static UntypedInt BPF_SOCK_OPS_RTT_CB => 0xc;
public static UntypedInt BPF_SOCK_OPS_PARSE_HDR_OPT_CB => 0xd;
public static UntypedInt BPF_SOCK_OPS_HDR_OPT_LEN_CB => 0xe;
public static UntypedInt BPF_SOCK_OPS_WRITE_HDR_OPT_CB => 0xf;
public static UntypedInt BPF_TCP_ESTABLISHED => 0x1;
public static UntypedInt BPF_TCP_SYN_SENT => 0x2;
public static UntypedInt BPF_TCP_SYN_RECV => 0x3;
public static UntypedInt BPF_TCP_FIN_WAIT1 => 0x4;
public static UntypedInt BPF_TCP_FIN_WAIT2 => 0x5;
public static UntypedInt BPF_TCP_TIME_WAIT => 0x6;
public static UntypedInt BPF_TCP_CLOSE => 0x7;
public static UntypedInt BPF_TCP_CLOSE_WAIT => 0x8;
public static UntypedInt BPF_TCP_LAST_ACK => 0x9;
public static UntypedInt BPF_TCP_LISTEN => 0xa;
public static UntypedInt BPF_TCP_CLOSING => 0xb;
public static UntypedInt BPF_TCP_NEW_SYN_RECV => 0xc;
public static UntypedInt BPF_TCP_MAX_STATES => 0xe;
public static UntypedInt TCP_BPF_IW => 0x3e9;
public static UntypedInt TCP_BPF_SNDCWND_CLAMP => 0x3ea;
public static UntypedInt TCP_BPF_DELACK_MAX => 0x3eb;
public static UntypedInt TCP_BPF_RTO_MIN => 0x3ec;
public static UntypedInt TCP_BPF_SYN => 0x3ed;
public static UntypedInt TCP_BPF_SYN_IP => 0x3ee;
public static UntypedInt TCP_BPF_SYN_MAC => 0x3ef;
public static UntypedInt BPF_LOAD_HDR_OPT_TCP_SYN => 0x1;
public static UntypedInt BPF_WRITE_HDR_TCP_CURRENT_MSS => 0x1;
public static UntypedInt BPF_WRITE_HDR_TCP_SYNACK_COOKIE => 0x2;
public static UntypedInt BPF_DEVCG_ACC_MKNOD => 0x1;
public static UntypedInt BPF_DEVCG_ACC_READ => 0x2;
public static UntypedInt BPF_DEVCG_ACC_WRITE => 0x4;
public static UntypedInt BPF_DEVCG_DEV_BLOCK => 0x1;
public static UntypedInt BPF_DEVCG_DEV_CHAR => 0x2;
public static UntypedInt BPF_FIB_LOOKUP_DIRECT => 0x1;
public static UntypedInt BPF_FIB_LOOKUP_OUTPUT => 0x2;
public static UntypedInt BPF_FIB_LOOKUP_SKIP_NEIGH => 0x4;
public static UntypedInt BPF_FIB_LOOKUP_TBID => 0x8;
public static UntypedInt BPF_FIB_LKUP_RET_SUCCESS => 0x0;
public static UntypedInt BPF_FIB_LKUP_RET_BLACKHOLE => 0x1;
public static UntypedInt BPF_FIB_LKUP_RET_UNREACHABLE => 0x2;
public static UntypedInt BPF_FIB_LKUP_RET_PROHIBIT => 0x3;
public static UntypedInt BPF_FIB_LKUP_RET_NOT_FWDED => 0x4;
public static UntypedInt BPF_FIB_LKUP_RET_FWD_DISABLED => 0x5;
public static UntypedInt BPF_FIB_LKUP_RET_UNSUPP_LWT => 0x6;
public static UntypedInt BPF_FIB_LKUP_RET_NO_NEIGH => 0x7;
public static UntypedInt BPF_FIB_LKUP_RET_FRAG_NEEDED => 0x8;
public static UntypedInt BPF_MTU_CHK_SEGS => 0x1;
public static UntypedInt BPF_MTU_CHK_RET_SUCCESS => 0x0;
public static UntypedInt BPF_MTU_CHK_RET_FRAG_NEEDED => 0x1;
public static UntypedInt BPF_MTU_CHK_RET_SEGS_TOOBIG => 0x2;
public static UntypedInt BPF_FD_TYPE_RAW_TRACEPOINT => 0x0;
public static UntypedInt BPF_FD_TYPE_TRACEPOINT => 0x1;
public static UntypedInt BPF_FD_TYPE_KPROBE => 0x2;
public static UntypedInt BPF_FD_TYPE_KRETPROBE => 0x3;
public static UntypedInt BPF_FD_TYPE_UPROBE => 0x4;
public static UntypedInt BPF_FD_TYPE_URETPROBE => 0x5;
public static UntypedInt BPF_FLOW_DISSECTOR_F_PARSE_1ST_FRAG => 0x1;
public static UntypedInt BPF_FLOW_DISSECTOR_F_STOP_AT_FLOW_LABEL => 0x2;
public static UntypedInt BPF_FLOW_DISSECTOR_F_STOP_AT_ENCAP => 0x4;
public static UntypedInt BPF_CORE_FIELD_BYTE_OFFSET => 0x0;
public static UntypedInt BPF_CORE_FIELD_BYTE_SIZE => 0x1;
public static UntypedInt BPF_CORE_FIELD_EXISTS => 0x2;
public static UntypedInt BPF_CORE_FIELD_SIGNED => 0x3;
public static UntypedInt BPF_CORE_FIELD_LSHIFT_U64 => 0x4;
public static UntypedInt BPF_CORE_FIELD_RSHIFT_U64 => 0x5;
public static UntypedInt BPF_CORE_TYPE_ID_LOCAL => 0x6;
public static UntypedInt BPF_CORE_TYPE_ID_TARGET => 0x7;
public static UntypedInt BPF_CORE_TYPE_EXISTS => 0x8;
public static UntypedInt BPF_CORE_TYPE_SIZE => 0x9;
public static UntypedInt BPF_CORE_ENUMVAL_EXISTS => 0xa;
public static UntypedInt BPF_CORE_ENUMVAL_VALUE => 0xb;
public static UntypedInt BPF_CORE_TYPE_MATCHES => 0xc;
public static UntypedInt BPF_F_TIMER_ABS => 0x1;

public static UntypedInt TCA_UNSPEC => 0x0;
public static UntypedInt TCA_KIND => 0x1;
public static UntypedInt TCA_OPTIONS => 0x2;
public static UntypedInt TCA_STATS => 0x3;
public static UntypedInt TCA_XSTATS => 0x4;
public static UntypedInt TCA_RATE => 0x5;
public static UntypedInt TCA_FCNT => 0x6;
public static UntypedInt TCA_STATS2 => 0x7;
public static UntypedInt TCA_STAB => 0x8;
public static UntypedInt TCA_PAD => 0x9;
public static UntypedInt TCA_DUMP_INVISIBLE => 0xa;
public static UntypedInt TCA_CHAIN => 0xb;
public static UntypedInt TCA_HW_OFFLOAD => 0xc;
public static UntypedInt TCA_INGRESS_BLOCK => 0xd;
public static UntypedInt TCA_EGRESS_BLOCK => 0xe;
public static UntypedInt TCA_DUMP_FLAGS => 0xf;
public static UntypedInt TCA_EXT_WARN_MSG => 0x10;
public static UntypedInt RTNLGRP_NONE => 0x0;
public static UntypedInt RTNLGRP_LINK => 0x1;
public static UntypedInt RTNLGRP_NOTIFY => 0x2;
public static UntypedInt RTNLGRP_NEIGH => 0x3;
public static UntypedInt RTNLGRP_TC => 0x4;
public static UntypedInt RTNLGRP_IPV4_IFADDR => 0x5;
public static UntypedInt RTNLGRP_IPV4_MROUTE => 0x6;
public static UntypedInt RTNLGRP_IPV4_ROUTE => 0x7;
public static UntypedInt RTNLGRP_IPV4_RULE => 0x8;
public static UntypedInt RTNLGRP_IPV6_IFADDR => 0x9;
public static UntypedInt RTNLGRP_IPV6_MROUTE => 0xa;
public static UntypedInt RTNLGRP_IPV6_ROUTE => 0xb;
public static UntypedInt RTNLGRP_IPV6_IFINFO => 0xc;
public static UntypedInt RTNLGRP_DECnet_IFADDR => 0xd;
public static UntypedInt RTNLGRP_NOP2 => 0xe;
public static UntypedInt RTNLGRP_DECnet_ROUTE => 0xf;
public static UntypedInt RTNLGRP_DECnet_RULE => 0x10;
public static UntypedInt RTNLGRP_NOP4 => 0x11;
public static UntypedInt RTNLGRP_IPV6_PREFIX => 0x12;
public static UntypedInt RTNLGRP_IPV6_RULE => 0x13;
public static UntypedInt RTNLGRP_ND_USEROPT => 0x14;
public static UntypedInt RTNLGRP_PHONET_IFADDR => 0x15;
public static UntypedInt RTNLGRP_PHONET_ROUTE => 0x16;
public static UntypedInt RTNLGRP_DCB => 0x17;
public static UntypedInt RTNLGRP_IPV4_NETCONF => 0x18;
public static UntypedInt RTNLGRP_IPV6_NETCONF => 0x19;
public static UntypedInt RTNLGRP_MDB => 0x1a;
public static UntypedInt RTNLGRP_MPLS_ROUTE => 0x1b;
public static UntypedInt RTNLGRP_NSID => 0x1c;
public static UntypedInt RTNLGRP_MPLS_NETCONF => 0x1d;
public static UntypedInt RTNLGRP_IPV4_MROUTE_R => 0x1e;
public static UntypedInt RTNLGRP_IPV6_MROUTE_R => 0x1f;
public static UntypedInt RTNLGRP_NEXTHOP => 0x20;
public static UntypedInt RTNLGRP_BRVLAN => 0x21;
public static UntypedInt RTNLGRP_MCTP_IFADDR => 0x22;
public static UntypedInt RTNLGRP_TUNNEL => 0x23;
public static UntypedInt RTNLGRP_STATS => 0x24;
public static UntypedInt RTNLGRP_IPV4_MCADDR => 0x25;
public static UntypedInt RTNLGRP_IPV6_MCADDR => 0x26;
public static UntypedInt RTNLGRP_IPV6_ACADDR => 0x27;
public static UntypedInt TCA_ROOT_UNSPEC => 0x0;
public static UntypedInt TCA_ROOT_TAB => 0x1;
public static UntypedInt TCA_ROOT_FLAGS => 0x2;
public static UntypedInt TCA_ROOT_COUNT => 0x3;
public static UntypedInt TCA_ROOT_TIME_DELTA => 0x4;
public static UntypedInt TCA_ROOT_EXT_WARN_MSG => 0x5;

partial struct CapUserHeader {
    public uint32 Version;
    public int32 Pid;
}

partial struct CapUserData {
    public uint32 Effective;
    public uint32 Permitted;
    public uint32 Inheritable;
}

public static UntypedInt LINUX_CAPABILITY_VERSION_1 => 0x19980330;
public static UntypedInt LINUX_CAPABILITY_VERSION_2 => 0x20071026;
public static UntypedInt LINUX_CAPABILITY_VERSION_3 => 0x20080522;

public static UntypedInt LO_FLAGS_READ_ONLY => 0x1;
public static UntypedInt LO_FLAGS_AUTOCLEAR => 0x4;
public static UntypedInt LO_FLAGS_PARTSCAN => 0x8;
public static UntypedInt LO_FLAGS_DIRECT_IO => 0x10;

[global::go.golib.GoInlineLayout(Size = 232, Align = 8), StructLayout(LayoutKind.Explicit, Size = 232)] partial struct LoopInfo64 {
    [FieldOffset(0)] public uint64 Device;
    [FieldOffset(8)] public uint64 Inode;
    [FieldOffset(16)] public uint64 Rdevice;
    [FieldOffset(24)] public uint64 Offset;
    [FieldOffset(32)] public uint64 Sizelimit;
    [FieldOffset(40)] public uint32 Number;
    [FieldOffset(44)] public uint32 Encrypt_type;
    [FieldOffset(48)] public uint32 Encrypt_key_size;
    [FieldOffset(52)] public uint32 Flags;
    [FieldOffset(56)] public array64<uint8> File_name;
    [FieldOffset(120)] public array64<uint8> Crypt_name;
    [FieldOffset(184)] public array32<uint8> Encrypt_key;
    [FieldOffset(216)] public array2<uint64> Init;
}
[global::go.golib.GoInlineLayout(Size = 304, Align = 8), StructLayout(LayoutKind.Explicit, Size = 304)] partial struct LoopConfig {
    [FieldOffset(0)] public uint32 Fd;
    [FieldOffset(4)] public uint32 Size;
    [FieldOffset(8)] public LoopInfo64 Info;
    [FieldOffset(240)] internal array8<uint64> _;
}

partial struct TIPCSocketAddr {
    public uint32 Ref;
    public uint32 Node;
}

partial struct TIPCServiceRange {
    public uint32 Type;
    public uint32 Lower;
    public uint32 Upper;
}

partial struct TIPCServiceName {
    public uint32 Type;
    public uint32 Instance;
    public uint32 Domain;
}

partial struct TIPCEvent {
    public uint32 Event;
    public uint32 Lower;
    public uint32 Upper;
    public TIPCSocketAddr Port;
    public TIPCSubscr S;
}

partial struct TIPCGroupReq {
    public uint32 Type;
    public uint32 Instance;
    public uint32 Scope;
    public uint32 Flags;
}

public static UntypedInt TIPC_CLUSTER_SCOPE => 0x2;
public static UntypedInt TIPC_NODE_SCOPE => 0x3;

public static UntypedInt SYSLOG_ACTION_CLOSE => 0;
public static UntypedInt SYSLOG_ACTION_OPEN => 1;
public static UntypedInt SYSLOG_ACTION_READ => 2;
public static UntypedInt SYSLOG_ACTION_READ_ALL => 3;
public static UntypedInt SYSLOG_ACTION_READ_CLEAR => 4;
public static UntypedInt SYSLOG_ACTION_CLEAR => 5;
public static UntypedInt SYSLOG_ACTION_CONSOLE_OFF => 6;
public static UntypedInt SYSLOG_ACTION_CONSOLE_ON => 7;
public static UntypedInt SYSLOG_ACTION_CONSOLE_LEVEL => 8;
public static UntypedInt SYSLOG_ACTION_SIZE_UNREAD => 9;
public static UntypedInt SYSLOG_ACTION_SIZE_BUFFER => 10;

public static UntypedInt DEVLINK_CMD_UNSPEC => 0x0;
public static UntypedInt DEVLINK_CMD_GET => 0x1;
public static UntypedInt DEVLINK_CMD_SET => 0x2;
public static UntypedInt DEVLINK_CMD_NEW => 0x3;
public static UntypedInt DEVLINK_CMD_DEL => 0x4;
public static UntypedInt DEVLINK_CMD_PORT_GET => 0x5;
public static UntypedInt DEVLINK_CMD_PORT_SET => 0x6;
public static UntypedInt DEVLINK_CMD_PORT_NEW => 0x7;
public static UntypedInt DEVLINK_CMD_PORT_DEL => 0x8;
public static UntypedInt DEVLINK_CMD_PORT_SPLIT => 0x9;
public static UntypedInt DEVLINK_CMD_PORT_UNSPLIT => 0xa;
public static UntypedInt DEVLINK_CMD_SB_GET => 0xb;
public static UntypedInt DEVLINK_CMD_SB_SET => 0xc;
public static UntypedInt DEVLINK_CMD_SB_NEW => 0xd;
public static UntypedInt DEVLINK_CMD_SB_DEL => 0xe;
public static UntypedInt DEVLINK_CMD_SB_POOL_GET => 0xf;
public static UntypedInt DEVLINK_CMD_SB_POOL_SET => 0x10;
public static UntypedInt DEVLINK_CMD_SB_POOL_NEW => 0x11;
public static UntypedInt DEVLINK_CMD_SB_POOL_DEL => 0x12;
public static UntypedInt DEVLINK_CMD_SB_PORT_POOL_GET => 0x13;
public static UntypedInt DEVLINK_CMD_SB_PORT_POOL_SET => 0x14;
public static UntypedInt DEVLINK_CMD_SB_PORT_POOL_NEW => 0x15;
public static UntypedInt DEVLINK_CMD_SB_PORT_POOL_DEL => 0x16;
public static UntypedInt DEVLINK_CMD_SB_TC_POOL_BIND_GET => 0x17;
public static UntypedInt DEVLINK_CMD_SB_TC_POOL_BIND_SET => 0x18;
public static UntypedInt DEVLINK_CMD_SB_TC_POOL_BIND_NEW => 0x19;
public static UntypedInt DEVLINK_CMD_SB_TC_POOL_BIND_DEL => 0x1a;
public static UntypedInt DEVLINK_CMD_SB_OCC_SNAPSHOT => 0x1b;
public static UntypedInt DEVLINK_CMD_SB_OCC_MAX_CLEAR => 0x1c;
public static UntypedInt DEVLINK_CMD_ESWITCH_GET => 0x1d;
public static UntypedInt DEVLINK_CMD_ESWITCH_SET => 0x1e;
public static UntypedInt DEVLINK_CMD_DPIPE_TABLE_GET => 0x1f;
public static UntypedInt DEVLINK_CMD_DPIPE_ENTRIES_GET => 0x20;
public static UntypedInt DEVLINK_CMD_DPIPE_HEADERS_GET => 0x21;
public static UntypedInt DEVLINK_CMD_DPIPE_TABLE_COUNTERS_SET => 0x22;
public static UntypedInt DEVLINK_CMD_RESOURCE_SET => 0x23;
public static UntypedInt DEVLINK_CMD_RESOURCE_DUMP => 0x24;
public static UntypedInt DEVLINK_CMD_RELOAD => 0x25;
public static UntypedInt DEVLINK_CMD_PARAM_GET => 0x26;
public static UntypedInt DEVLINK_CMD_PARAM_SET => 0x27;
public static UntypedInt DEVLINK_CMD_PARAM_NEW => 0x28;
public static UntypedInt DEVLINK_CMD_PARAM_DEL => 0x29;
public static UntypedInt DEVLINK_CMD_REGION_GET => 0x2a;
public static UntypedInt DEVLINK_CMD_REGION_SET => 0x2b;
public static UntypedInt DEVLINK_CMD_REGION_NEW => 0x2c;
public static UntypedInt DEVLINK_CMD_REGION_DEL => 0x2d;
public static UntypedInt DEVLINK_CMD_REGION_READ => 0x2e;
public static UntypedInt DEVLINK_CMD_PORT_PARAM_GET => 0x2f;
public static UntypedInt DEVLINK_CMD_PORT_PARAM_SET => 0x30;
public static UntypedInt DEVLINK_CMD_PORT_PARAM_NEW => 0x31;
public static UntypedInt DEVLINK_CMD_PORT_PARAM_DEL => 0x32;
public static UntypedInt DEVLINK_CMD_INFO_GET => 0x33;
public static UntypedInt DEVLINK_CMD_HEALTH_REPORTER_GET => 0x34;
public static UntypedInt DEVLINK_CMD_HEALTH_REPORTER_SET => 0x35;
public static UntypedInt DEVLINK_CMD_HEALTH_REPORTER_RECOVER => 0x36;
public static UntypedInt DEVLINK_CMD_HEALTH_REPORTER_DIAGNOSE => 0x37;
public static UntypedInt DEVLINK_CMD_HEALTH_REPORTER_DUMP_GET => 0x38;
public static UntypedInt DEVLINK_CMD_HEALTH_REPORTER_DUMP_CLEAR => 0x39;
public static UntypedInt DEVLINK_CMD_FLASH_UPDATE => 0x3a;
public static UntypedInt DEVLINK_CMD_FLASH_UPDATE_END => 0x3b;
public static UntypedInt DEVLINK_CMD_FLASH_UPDATE_STATUS => 0x3c;
public static UntypedInt DEVLINK_CMD_TRAP_GET => 0x3d;
public static UntypedInt DEVLINK_CMD_TRAP_SET => 0x3e;
public static UntypedInt DEVLINK_CMD_TRAP_NEW => 0x3f;
public static UntypedInt DEVLINK_CMD_TRAP_DEL => 0x40;
public static UntypedInt DEVLINK_CMD_TRAP_GROUP_GET => 0x41;
public static UntypedInt DEVLINK_CMD_TRAP_GROUP_SET => 0x42;
public static UntypedInt DEVLINK_CMD_TRAP_GROUP_NEW => 0x43;
public static UntypedInt DEVLINK_CMD_TRAP_GROUP_DEL => 0x44;
public static UntypedInt DEVLINK_CMD_TRAP_POLICER_GET => 0x45;
public static UntypedInt DEVLINK_CMD_TRAP_POLICER_SET => 0x46;
public static UntypedInt DEVLINK_CMD_TRAP_POLICER_NEW => 0x47;
public static UntypedInt DEVLINK_CMD_TRAP_POLICER_DEL => 0x48;
public static UntypedInt DEVLINK_CMD_HEALTH_REPORTER_TEST => 0x49;
public static UntypedInt DEVLINK_CMD_RATE_GET => 0x4a;
public static UntypedInt DEVLINK_CMD_RATE_SET => 0x4b;
public static UntypedInt DEVLINK_CMD_RATE_NEW => 0x4c;
public static UntypedInt DEVLINK_CMD_RATE_DEL => 0x4d;
public static UntypedInt DEVLINK_CMD_LINECARD_GET => 0x4e;
public static UntypedInt DEVLINK_CMD_LINECARD_SET => 0x4f;
public static UntypedInt DEVLINK_CMD_LINECARD_NEW => 0x50;
public static UntypedInt DEVLINK_CMD_LINECARD_DEL => 0x51;
public static UntypedInt DEVLINK_CMD_SELFTESTS_GET => 0x52;
public static UntypedInt DEVLINK_CMD_MAX => 0x54;
public static UntypedInt DEVLINK_PORT_TYPE_NOTSET => 0x0;
public static UntypedInt DEVLINK_PORT_TYPE_AUTO => 0x1;
public static UntypedInt DEVLINK_PORT_TYPE_ETH => 0x2;
public static UntypedInt DEVLINK_PORT_TYPE_IB => 0x3;
public static UntypedInt DEVLINK_SB_POOL_TYPE_INGRESS => 0x0;
public static UntypedInt DEVLINK_SB_POOL_TYPE_EGRESS => 0x1;
public static UntypedInt DEVLINK_SB_THRESHOLD_TYPE_STATIC => 0x0;
public static UntypedInt DEVLINK_SB_THRESHOLD_TYPE_DYNAMIC => 0x1;
public static UntypedInt DEVLINK_ESWITCH_MODE_LEGACY => 0x0;
public static UntypedInt DEVLINK_ESWITCH_MODE_SWITCHDEV => 0x1;
public static UntypedInt DEVLINK_ESWITCH_INLINE_MODE_NONE => 0x0;
public static UntypedInt DEVLINK_ESWITCH_INLINE_MODE_LINK => 0x1;
public static UntypedInt DEVLINK_ESWITCH_INLINE_MODE_NETWORK => 0x2;
public static UntypedInt DEVLINK_ESWITCH_INLINE_MODE_TRANSPORT => 0x3;
public static UntypedInt DEVLINK_ESWITCH_ENCAP_MODE_NONE => 0x0;
public static UntypedInt DEVLINK_ESWITCH_ENCAP_MODE_BASIC => 0x1;
public static UntypedInt DEVLINK_PORT_FLAVOUR_PHYSICAL => 0x0;
public static UntypedInt DEVLINK_PORT_FLAVOUR_CPU => 0x1;
public static UntypedInt DEVLINK_PORT_FLAVOUR_DSA => 0x2;
public static UntypedInt DEVLINK_PORT_FLAVOUR_PCI_PF => 0x3;
public static UntypedInt DEVLINK_PORT_FLAVOUR_PCI_VF => 0x4;
public static UntypedInt DEVLINK_PORT_FLAVOUR_VIRTUAL => 0x5;
public static UntypedInt DEVLINK_PORT_FLAVOUR_UNUSED => 0x6;
public static UntypedInt DEVLINK_PARAM_CMODE_RUNTIME => 0x0;
public static UntypedInt DEVLINK_PARAM_CMODE_DRIVERINIT => 0x1;
public static UntypedInt DEVLINK_PARAM_CMODE_PERMANENT => 0x2;
public static UntypedInt DEVLINK_PARAM_CMODE_MAX => 0x2;
public static UntypedInt DEVLINK_PARAM_FW_LOAD_POLICY_VALUE_DRIVER => 0x0;
public static UntypedInt DEVLINK_PARAM_FW_LOAD_POLICY_VALUE_FLASH => 0x1;
public static UntypedInt DEVLINK_PARAM_FW_LOAD_POLICY_VALUE_DISK => 0x2;
public static UntypedInt DEVLINK_PARAM_FW_LOAD_POLICY_VALUE_UNKNOWN => 0x3;
public static UntypedInt DEVLINK_PARAM_RESET_DEV_ON_DRV_PROBE_VALUE_UNKNOWN => 0x0;
public static UntypedInt DEVLINK_PARAM_RESET_DEV_ON_DRV_PROBE_VALUE_ALWAYS => 0x1;
public static UntypedInt DEVLINK_PARAM_RESET_DEV_ON_DRV_PROBE_VALUE_NEVER => 0x2;
public static UntypedInt DEVLINK_PARAM_RESET_DEV_ON_DRV_PROBE_VALUE_DISK => 0x3;
public static UntypedInt DEVLINK_ATTR_STATS_RX_PACKETS => 0x0;
public static UntypedInt DEVLINK_ATTR_STATS_RX_BYTES => 0x1;
public static UntypedInt DEVLINK_ATTR_STATS_RX_DROPPED => 0x2;
public static UntypedInt DEVLINK_ATTR_STATS_MAX => 0x2;
public static UntypedInt DEVLINK_FLASH_OVERWRITE_SETTINGS_BIT => 0x0;
public static UntypedInt DEVLINK_FLASH_OVERWRITE_IDENTIFIERS_BIT => 0x1;
public static UntypedInt DEVLINK_FLASH_OVERWRITE_MAX_BIT => 0x1;
public static UntypedInt DEVLINK_TRAP_ACTION_DROP => 0x0;
public static UntypedInt DEVLINK_TRAP_ACTION_TRAP => 0x1;
public static UntypedInt DEVLINK_TRAP_ACTION_MIRROR => 0x2;
public static UntypedInt DEVLINK_TRAP_TYPE_DROP => 0x0;
public static UntypedInt DEVLINK_TRAP_TYPE_EXCEPTION => 0x1;
public static UntypedInt DEVLINK_TRAP_TYPE_CONTROL => 0x2;
public static UntypedInt DEVLINK_ATTR_TRAP_METADATA_TYPE_IN_PORT => 0x0;
public static UntypedInt DEVLINK_ATTR_TRAP_METADATA_TYPE_FA_COOKIE => 0x1;
public static UntypedInt DEVLINK_RELOAD_ACTION_UNSPEC => 0x0;
public static UntypedInt DEVLINK_RELOAD_ACTION_DRIVER_REINIT => 0x1;
public static UntypedInt DEVLINK_RELOAD_ACTION_FW_ACTIVATE => 0x2;
public static UntypedInt DEVLINK_RELOAD_ACTION_MAX => 0x2;
public static UntypedInt DEVLINK_RELOAD_LIMIT_UNSPEC => 0x0;
public static UntypedInt DEVLINK_RELOAD_LIMIT_NO_RESET => 0x1;
public static UntypedInt DEVLINK_RELOAD_LIMIT_MAX => 0x1;
public static UntypedInt DEVLINK_ATTR_UNSPEC => 0x0;
public static UntypedInt DEVLINK_ATTR_BUS_NAME => 0x1;
public static UntypedInt DEVLINK_ATTR_DEV_NAME => 0x2;
public static UntypedInt DEVLINK_ATTR_PORT_INDEX => 0x3;
public static UntypedInt DEVLINK_ATTR_PORT_TYPE => 0x4;
public static UntypedInt DEVLINK_ATTR_PORT_DESIRED_TYPE => 0x5;
public static UntypedInt DEVLINK_ATTR_PORT_NETDEV_IFINDEX => 0x6;
public static UntypedInt DEVLINK_ATTR_PORT_NETDEV_NAME => 0x7;
public static UntypedInt DEVLINK_ATTR_PORT_IBDEV_NAME => 0x8;
public static UntypedInt DEVLINK_ATTR_PORT_SPLIT_COUNT => 0x9;
public static UntypedInt DEVLINK_ATTR_PORT_SPLIT_GROUP => 0xa;
public static UntypedInt DEVLINK_ATTR_SB_INDEX => 0xb;
public static UntypedInt DEVLINK_ATTR_SB_SIZE => 0xc;
public static UntypedInt DEVLINK_ATTR_SB_INGRESS_POOL_COUNT => 0xd;
public static UntypedInt DEVLINK_ATTR_SB_EGRESS_POOL_COUNT => 0xe;
public static UntypedInt DEVLINK_ATTR_SB_INGRESS_TC_COUNT => 0xf;
public static UntypedInt DEVLINK_ATTR_SB_EGRESS_TC_COUNT => 0x10;
public static UntypedInt DEVLINK_ATTR_SB_POOL_INDEX => 0x11;
public static UntypedInt DEVLINK_ATTR_SB_POOL_TYPE => 0x12;
public static UntypedInt DEVLINK_ATTR_SB_POOL_SIZE => 0x13;
public static UntypedInt DEVLINK_ATTR_SB_POOL_THRESHOLD_TYPE => 0x14;
public static UntypedInt DEVLINK_ATTR_SB_THRESHOLD => 0x15;
public static UntypedInt DEVLINK_ATTR_SB_TC_INDEX => 0x16;
public static UntypedInt DEVLINK_ATTR_SB_OCC_CUR => 0x17;
public static UntypedInt DEVLINK_ATTR_SB_OCC_MAX => 0x18;
public static UntypedInt DEVLINK_ATTR_ESWITCH_MODE => 0x19;
public static UntypedInt DEVLINK_ATTR_ESWITCH_INLINE_MODE => 0x1a;
public static UntypedInt DEVLINK_ATTR_DPIPE_TABLES => 0x1b;
public static UntypedInt DEVLINK_ATTR_DPIPE_TABLE => 0x1c;
public static UntypedInt DEVLINK_ATTR_DPIPE_TABLE_NAME => 0x1d;
public static UntypedInt DEVLINK_ATTR_DPIPE_TABLE_SIZE => 0x1e;
public static UntypedInt DEVLINK_ATTR_DPIPE_TABLE_MATCHES => 0x1f;
public static UntypedInt DEVLINK_ATTR_DPIPE_TABLE_ACTIONS => 0x20;
public static UntypedInt DEVLINK_ATTR_DPIPE_TABLE_COUNTERS_ENABLED => 0x21;
public static UntypedInt DEVLINK_ATTR_DPIPE_ENTRIES => 0x22;
public static UntypedInt DEVLINK_ATTR_DPIPE_ENTRY => 0x23;
public static UntypedInt DEVLINK_ATTR_DPIPE_ENTRY_INDEX => 0x24;
public static UntypedInt DEVLINK_ATTR_DPIPE_ENTRY_MATCH_VALUES => 0x25;
public static UntypedInt DEVLINK_ATTR_DPIPE_ENTRY_ACTION_VALUES => 0x26;
public static UntypedInt DEVLINK_ATTR_DPIPE_ENTRY_COUNTER => 0x27;
public static UntypedInt DEVLINK_ATTR_DPIPE_MATCH => 0x28;
public static UntypedInt DEVLINK_ATTR_DPIPE_MATCH_VALUE => 0x29;
public static UntypedInt DEVLINK_ATTR_DPIPE_MATCH_TYPE => 0x2a;
public static UntypedInt DEVLINK_ATTR_DPIPE_ACTION => 0x2b;
public static UntypedInt DEVLINK_ATTR_DPIPE_ACTION_VALUE => 0x2c;
public static UntypedInt DEVLINK_ATTR_DPIPE_ACTION_TYPE => 0x2d;
public static UntypedInt DEVLINK_ATTR_DPIPE_VALUE => 0x2e;
public static UntypedInt DEVLINK_ATTR_DPIPE_VALUE_MASK => 0x2f;
public static UntypedInt DEVLINK_ATTR_DPIPE_VALUE_MAPPING => 0x30;
public static UntypedInt DEVLINK_ATTR_DPIPE_HEADERS => 0x31;
public static UntypedInt DEVLINK_ATTR_DPIPE_HEADER => 0x32;
public static UntypedInt DEVLINK_ATTR_DPIPE_HEADER_NAME => 0x33;
public static UntypedInt DEVLINK_ATTR_DPIPE_HEADER_ID => 0x34;
public static UntypedInt DEVLINK_ATTR_DPIPE_HEADER_FIELDS => 0x35;
public static UntypedInt DEVLINK_ATTR_DPIPE_HEADER_GLOBAL => 0x36;
public static UntypedInt DEVLINK_ATTR_DPIPE_HEADER_INDEX => 0x37;
public static UntypedInt DEVLINK_ATTR_DPIPE_FIELD => 0x38;
public static UntypedInt DEVLINK_ATTR_DPIPE_FIELD_NAME => 0x39;
public static UntypedInt DEVLINK_ATTR_DPIPE_FIELD_ID => 0x3a;
public static UntypedInt DEVLINK_ATTR_DPIPE_FIELD_BITWIDTH => 0x3b;
public static UntypedInt DEVLINK_ATTR_DPIPE_FIELD_MAPPING_TYPE => 0x3c;
public static UntypedInt DEVLINK_ATTR_PAD => 0x3d;
public static UntypedInt DEVLINK_ATTR_ESWITCH_ENCAP_MODE => 0x3e;
public static UntypedInt DEVLINK_ATTR_RESOURCE_LIST => 0x3f;
public static UntypedInt DEVLINK_ATTR_RESOURCE => 0x40;
public static UntypedInt DEVLINK_ATTR_RESOURCE_NAME => 0x41;
public static UntypedInt DEVLINK_ATTR_RESOURCE_ID => 0x42;
public static UntypedInt DEVLINK_ATTR_RESOURCE_SIZE => 0x43;
public static UntypedInt DEVLINK_ATTR_RESOURCE_SIZE_NEW => 0x44;
public static UntypedInt DEVLINK_ATTR_RESOURCE_SIZE_VALID => 0x45;
public static UntypedInt DEVLINK_ATTR_RESOURCE_SIZE_MIN => 0x46;
public static UntypedInt DEVLINK_ATTR_RESOURCE_SIZE_MAX => 0x47;
public static UntypedInt DEVLINK_ATTR_RESOURCE_SIZE_GRAN => 0x48;
public static UntypedInt DEVLINK_ATTR_RESOURCE_UNIT => 0x49;
public static UntypedInt DEVLINK_ATTR_RESOURCE_OCC => 0x4a;
public static UntypedInt DEVLINK_ATTR_DPIPE_TABLE_RESOURCE_ID => 0x4b;
public static UntypedInt DEVLINK_ATTR_DPIPE_TABLE_RESOURCE_UNITS => 0x4c;
public static UntypedInt DEVLINK_ATTR_PORT_FLAVOUR => 0x4d;
public static UntypedInt DEVLINK_ATTR_PORT_NUMBER => 0x4e;
public static UntypedInt DEVLINK_ATTR_PORT_SPLIT_SUBPORT_NUMBER => 0x4f;
public static UntypedInt DEVLINK_ATTR_PARAM => 0x50;
public static UntypedInt DEVLINK_ATTR_PARAM_NAME => 0x51;
public static UntypedInt DEVLINK_ATTR_PARAM_GENERIC => 0x52;
public static UntypedInt DEVLINK_ATTR_PARAM_TYPE => 0x53;
public static UntypedInt DEVLINK_ATTR_PARAM_VALUES_LIST => 0x54;
public static UntypedInt DEVLINK_ATTR_PARAM_VALUE => 0x55;
public static UntypedInt DEVLINK_ATTR_PARAM_VALUE_DATA => 0x56;
public static UntypedInt DEVLINK_ATTR_PARAM_VALUE_CMODE => 0x57;
public static UntypedInt DEVLINK_ATTR_REGION_NAME => 0x58;
public static UntypedInt DEVLINK_ATTR_REGION_SIZE => 0x59;
public static UntypedInt DEVLINK_ATTR_REGION_SNAPSHOTS => 0x5a;
public static UntypedInt DEVLINK_ATTR_REGION_SNAPSHOT => 0x5b;
public static UntypedInt DEVLINK_ATTR_REGION_SNAPSHOT_ID => 0x5c;
public static UntypedInt DEVLINK_ATTR_REGION_CHUNKS => 0x5d;
public static UntypedInt DEVLINK_ATTR_REGION_CHUNK => 0x5e;
public static UntypedInt DEVLINK_ATTR_REGION_CHUNK_DATA => 0x5f;
public static UntypedInt DEVLINK_ATTR_REGION_CHUNK_ADDR => 0x60;
public static UntypedInt DEVLINK_ATTR_REGION_CHUNK_LEN => 0x61;
public static UntypedInt DEVLINK_ATTR_INFO_DRIVER_NAME => 0x62;
public static UntypedInt DEVLINK_ATTR_INFO_SERIAL_NUMBER => 0x63;
public static UntypedInt DEVLINK_ATTR_INFO_VERSION_FIXED => 0x64;
public static UntypedInt DEVLINK_ATTR_INFO_VERSION_RUNNING => 0x65;
public static UntypedInt DEVLINK_ATTR_INFO_VERSION_STORED => 0x66;
public static UntypedInt DEVLINK_ATTR_INFO_VERSION_NAME => 0x67;
public static UntypedInt DEVLINK_ATTR_INFO_VERSION_VALUE => 0x68;
public static UntypedInt DEVLINK_ATTR_SB_POOL_CELL_SIZE => 0x69;
public static UntypedInt DEVLINK_ATTR_FMSG => 0x6a;
public static UntypedInt DEVLINK_ATTR_FMSG_OBJ_NEST_START => 0x6b;
public static UntypedInt DEVLINK_ATTR_FMSG_PAIR_NEST_START => 0x6c;
public static UntypedInt DEVLINK_ATTR_FMSG_ARR_NEST_START => 0x6d;
public static UntypedInt DEVLINK_ATTR_FMSG_NEST_END => 0x6e;
public static UntypedInt DEVLINK_ATTR_FMSG_OBJ_NAME => 0x6f;
public static UntypedInt DEVLINK_ATTR_FMSG_OBJ_VALUE_TYPE => 0x70;
public static UntypedInt DEVLINK_ATTR_FMSG_OBJ_VALUE_DATA => 0x71;
public static UntypedInt DEVLINK_ATTR_HEALTH_REPORTER => 0x72;
public static UntypedInt DEVLINK_ATTR_HEALTH_REPORTER_NAME => 0x73;
public static UntypedInt DEVLINK_ATTR_HEALTH_REPORTER_STATE => 0x74;
public static UntypedInt DEVLINK_ATTR_HEALTH_REPORTER_ERR_COUNT => 0x75;
public static UntypedInt DEVLINK_ATTR_HEALTH_REPORTER_RECOVER_COUNT => 0x76;
public static UntypedInt DEVLINK_ATTR_HEALTH_REPORTER_DUMP_TS => 0x77;
public static UntypedInt DEVLINK_ATTR_HEALTH_REPORTER_GRACEFUL_PERIOD => 0x78;
public static UntypedInt DEVLINK_ATTR_HEALTH_REPORTER_AUTO_RECOVER => 0x79;
public static UntypedInt DEVLINK_ATTR_FLASH_UPDATE_FILE_NAME => 0x7a;
public static UntypedInt DEVLINK_ATTR_FLASH_UPDATE_COMPONENT => 0x7b;
public static UntypedInt DEVLINK_ATTR_FLASH_UPDATE_STATUS_MSG => 0x7c;
public static UntypedInt DEVLINK_ATTR_FLASH_UPDATE_STATUS_DONE => 0x7d;
public static UntypedInt DEVLINK_ATTR_FLASH_UPDATE_STATUS_TOTAL => 0x7e;
public static UntypedInt DEVLINK_ATTR_PORT_PCI_PF_NUMBER => 0x7f;
public static UntypedInt DEVLINK_ATTR_PORT_PCI_VF_NUMBER => 0x80;
public static UntypedInt DEVLINK_ATTR_STATS => 0x81;
public static UntypedInt DEVLINK_ATTR_TRAP_NAME => 0x82;
public static UntypedInt DEVLINK_ATTR_TRAP_ACTION => 0x83;
public static UntypedInt DEVLINK_ATTR_TRAP_TYPE => 0x84;
public static UntypedInt DEVLINK_ATTR_TRAP_GENERIC => 0x85;
public static UntypedInt DEVLINK_ATTR_TRAP_METADATA => 0x86;
public static UntypedInt DEVLINK_ATTR_TRAP_GROUP_NAME => 0x87;
public static UntypedInt DEVLINK_ATTR_RELOAD_FAILED => 0x88;
public static UntypedInt DEVLINK_ATTR_HEALTH_REPORTER_DUMP_TS_NS => 0x89;
public static UntypedInt DEVLINK_ATTR_NETNS_FD => 0x8a;
public static UntypedInt DEVLINK_ATTR_NETNS_PID => 0x8b;
public static UntypedInt DEVLINK_ATTR_NETNS_ID => 0x8c;
public static UntypedInt DEVLINK_ATTR_HEALTH_REPORTER_AUTO_DUMP => 0x8d;
public static UntypedInt DEVLINK_ATTR_TRAP_POLICER_ID => 0x8e;
public static UntypedInt DEVLINK_ATTR_TRAP_POLICER_RATE => 0x8f;
public static UntypedInt DEVLINK_ATTR_TRAP_POLICER_BURST => 0x90;
public static UntypedInt DEVLINK_ATTR_PORT_FUNCTION => 0x91;
public static UntypedInt DEVLINK_ATTR_INFO_BOARD_SERIAL_NUMBER => 0x92;
public static UntypedInt DEVLINK_ATTR_PORT_LANES => 0x93;
public static UntypedInt DEVLINK_ATTR_PORT_SPLITTABLE => 0x94;
public static UntypedInt DEVLINK_ATTR_PORT_EXTERNAL => 0x95;
public static UntypedInt DEVLINK_ATTR_PORT_CONTROLLER_NUMBER => 0x96;
public static UntypedInt DEVLINK_ATTR_FLASH_UPDATE_STATUS_TIMEOUT => 0x97;
public static UntypedInt DEVLINK_ATTR_FLASH_UPDATE_OVERWRITE_MASK => 0x98;
public static UntypedInt DEVLINK_ATTR_RELOAD_ACTION => 0x99;
public static UntypedInt DEVLINK_ATTR_RELOAD_ACTIONS_PERFORMED => 0x9a;
public static UntypedInt DEVLINK_ATTR_RELOAD_LIMITS => 0x9b;
public static UntypedInt DEVLINK_ATTR_DEV_STATS => 0x9c;
public static UntypedInt DEVLINK_ATTR_RELOAD_STATS => 0x9d;
public static UntypedInt DEVLINK_ATTR_RELOAD_STATS_ENTRY => 0x9e;
public static UntypedInt DEVLINK_ATTR_RELOAD_STATS_LIMIT => 0x9f;
public static UntypedInt DEVLINK_ATTR_RELOAD_STATS_VALUE => 0xa0;
public static UntypedInt DEVLINK_ATTR_REMOTE_RELOAD_STATS => 0xa1;
public static UntypedInt DEVLINK_ATTR_RELOAD_ACTION_INFO => 0xa2;
public static UntypedInt DEVLINK_ATTR_RELOAD_ACTION_STATS => 0xa3;
public static UntypedInt DEVLINK_ATTR_PORT_PCI_SF_NUMBER => 0xa4;
public static UntypedInt DEVLINK_ATTR_RATE_TYPE => 0xa5;
public static UntypedInt DEVLINK_ATTR_RATE_TX_SHARE => 0xa6;
public static UntypedInt DEVLINK_ATTR_RATE_TX_MAX => 0xa7;
public static UntypedInt DEVLINK_ATTR_RATE_NODE_NAME => 0xa8;
public static UntypedInt DEVLINK_ATTR_RATE_PARENT_NODE_NAME => 0xa9;
public static UntypedInt DEVLINK_ATTR_REGION_MAX_SNAPSHOTS => 0xaa;
public static UntypedInt DEVLINK_ATTR_LINECARD_INDEX => 0xab;
public static UntypedInt DEVLINK_ATTR_LINECARD_STATE => 0xac;
public static UntypedInt DEVLINK_ATTR_LINECARD_TYPE => 0xad;
public static UntypedInt DEVLINK_ATTR_LINECARD_SUPPORTED_TYPES => 0xae;
public static UntypedInt DEVLINK_ATTR_NESTED_DEVLINK => 0xaf;
public static UntypedInt DEVLINK_ATTR_SELFTESTS => 0xb0;
public static UntypedInt DEVLINK_ATTR_MAX => 0xb9;
public static UntypedInt DEVLINK_DPIPE_FIELD_MAPPING_TYPE_NONE => 0x0;
public static UntypedInt DEVLINK_DPIPE_FIELD_MAPPING_TYPE_IFINDEX => 0x1;
public static UntypedInt DEVLINK_DPIPE_MATCH_TYPE_FIELD_EXACT => 0x0;
public static UntypedInt DEVLINK_DPIPE_ACTION_TYPE_FIELD_MODIFY => 0x0;
public static UntypedInt DEVLINK_DPIPE_FIELD_ETHERNET_DST_MAC => 0x0;
public static UntypedInt DEVLINK_DPIPE_FIELD_IPV4_DST_IP => 0x0;
public static UntypedInt DEVLINK_DPIPE_FIELD_IPV6_DST_IP => 0x0;
public static UntypedInt DEVLINK_DPIPE_HEADER_ETHERNET => 0x0;
public static UntypedInt DEVLINK_DPIPE_HEADER_IPV4 => 0x1;
public static UntypedInt DEVLINK_DPIPE_HEADER_IPV6 => 0x2;
public static UntypedInt DEVLINK_RESOURCE_UNIT_ENTRY => 0x0;
public static UntypedInt DEVLINK_PORT_FUNCTION_ATTR_UNSPEC => 0x0;
public static UntypedInt DEVLINK_PORT_FUNCTION_ATTR_HW_ADDR => 0x1;
public static UntypedInt DEVLINK_PORT_FN_ATTR_STATE => 0x2;
public static UntypedInt DEVLINK_PORT_FN_ATTR_OPSTATE => 0x3;
public static UntypedInt DEVLINK_PORT_FN_ATTR_CAPS => 0x4;
public static UntypedInt DEVLINK_PORT_FUNCTION_ATTR_MAX => 0x6;

partial struct FsverityDigest {
    public uint16 Algorithm;
    public uint16 Size;
}

partial struct FsverityEnableArg {
    public uint32 Version;
    public uint32 Hash_algorithm;
    public uint32 Block_size;
    public uint32 Salt_size;
    public uint64 Salt_ptr;
    public uint32 Sig_size;
    internal uint32 _;
    public uint64 Sig_ptr;
    internal array<uint64> __ = new(11);
}

partial struct Nhmsg {
    public uint8 Family;
    public uint8 Scope;
    public uint8 Protocol;
    public uint8 Resvd;
    public uint32 Flags;
}

public static UntypedInt SizeofNhmsg => 0x8;

partial struct NexthopGrp {
    public uint32 Id;
    public uint8 Weight;
    public uint8 High;
    public uint16 Resvd2;
}

public static UntypedInt SizeofNexthopGrp => 0x8;

public static UntypedInt NHA_UNSPEC => 0x0;
public static UntypedInt NHA_ID => 0x1;
public static UntypedInt NHA_GROUP => 0x2;
public static UntypedInt NHA_GROUP_TYPE => 0x3;
public static UntypedInt NHA_BLACKHOLE => 0x4;
public static UntypedInt NHA_OIF => 0x5;
public static UntypedInt NHA_GATEWAY => 0x6;
public static UntypedInt NHA_ENCAP_TYPE => 0x7;
public static UntypedInt NHA_ENCAP => 0x8;
public static UntypedInt NHA_GROUPS => 0x9;
public static UntypedInt NHA_MASTER => 0xa;

public static UntypedInt CAN_RAW_FILTER => 0x1;
public static UntypedInt CAN_RAW_ERR_FILTER => 0x2;
public static UntypedInt CAN_RAW_LOOPBACK => 0x3;
public static UntypedInt CAN_RAW_RECV_OWN_MSGS => 0x4;
public static UntypedInt CAN_RAW_FD_FRAMES => 0x5;
public static UntypedInt CAN_RAW_JOIN_FILTERS => 0x6;

[global::go.golib.GoInlineLayout(Size = 40, Align = 4), StructLayout(LayoutKind.Explicit, Size = 40)] partial struct WatchdogInfo {
    [FieldOffset(0)] public uint32 Options;
    [FieldOffset(4)] public uint32 Version;
    [FieldOffset(8)] public array32<uint8> Identity;
}

partial struct PPSFData {
    public PPSKInfo Info;
    public PPSKTime Timeout;
}

partial struct PPSKParams {
    public int32 Api_version;
    public int32 Mode;
    public PPSKTime Assert_off_tu;
    public PPSKTime Clear_off_tu;
}

partial struct PPSKTime {
    public int64 Sec;
    public int32 Nsec;
    public uint32 Flags;
}

public static UntypedInt LWTUNNEL_ENCAP_NONE => 0x0;
public static UntypedInt LWTUNNEL_ENCAP_MPLS => 0x1;
public static UntypedInt LWTUNNEL_ENCAP_IP => 0x2;
public static UntypedInt LWTUNNEL_ENCAP_ILA => 0x3;
public static UntypedInt LWTUNNEL_ENCAP_IP6 => 0x4;
public static UntypedInt LWTUNNEL_ENCAP_SEG6 => 0x5;
public static UntypedInt LWTUNNEL_ENCAP_BPF => 0x6;
public static UntypedInt LWTUNNEL_ENCAP_SEG6_LOCAL => 0x7;
public static UntypedInt LWTUNNEL_ENCAP_RPL => 0x8;
public static UntypedInt LWTUNNEL_ENCAP_IOAM6 => 0x9;
public static UntypedInt LWTUNNEL_ENCAP_XFRM => 0xa;
public static UntypedInt LWTUNNEL_ENCAP_MAX => 0xa;
public static UntypedInt MPLS_IPTUNNEL_UNSPEC => 0x0;
public static UntypedInt MPLS_IPTUNNEL_DST => 0x1;
public static UntypedInt MPLS_IPTUNNEL_TTL => 0x2;
public static UntypedInt MPLS_IPTUNNEL_MAX => 0x2;

public static UntypedInt ETHTOOL_ID_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_RX_COPYBREAK => 0x1;
public static UntypedInt ETHTOOL_TX_COPYBREAK => 0x2;
public static UntypedInt ETHTOOL_PFC_PREVENTION_TOUT => 0x3;
public static UntypedInt ETHTOOL_TUNABLE_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_TUNABLE_U8 => 0x1;
public static UntypedInt ETHTOOL_TUNABLE_U16 => 0x2;
public static UntypedInt ETHTOOL_TUNABLE_U32 => 0x3;
public static UntypedInt ETHTOOL_TUNABLE_U64 => 0x4;
public static UntypedInt ETHTOOL_TUNABLE_STRING => 0x5;
public static UntypedInt ETHTOOL_TUNABLE_S8 => 0x6;
public static UntypedInt ETHTOOL_TUNABLE_S16 => 0x7;
public static UntypedInt ETHTOOL_TUNABLE_S32 => 0x8;
public static UntypedInt ETHTOOL_TUNABLE_S64 => 0x9;
public static UntypedInt ETHTOOL_PHY_ID_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_PHY_DOWNSHIFT => 0x1;
public static UntypedInt ETHTOOL_PHY_FAST_LINK_DOWN => 0x2;
public static UntypedInt ETHTOOL_PHY_EDPD => 0x3;
public static UntypedInt ETHTOOL_LINK_EXT_STATE_AUTONEG => 0x0;
public static UntypedInt ETHTOOL_LINK_EXT_STATE_LINK_TRAINING_FAILURE => 0x1;
public static UntypedInt ETHTOOL_LINK_EXT_STATE_LINK_LOGICAL_MISMATCH => 0x2;
public static UntypedInt ETHTOOL_LINK_EXT_STATE_BAD_SIGNAL_INTEGRITY => 0x3;
public static UntypedInt ETHTOOL_LINK_EXT_STATE_NO_CABLE => 0x4;
public static UntypedInt ETHTOOL_LINK_EXT_STATE_CABLE_ISSUE => 0x5;
public static UntypedInt ETHTOOL_LINK_EXT_STATE_EEPROM_ISSUE => 0x6;
public static UntypedInt ETHTOOL_LINK_EXT_STATE_CALIBRATION_FAILURE => 0x7;
public static UntypedInt ETHTOOL_LINK_EXT_STATE_POWER_BUDGET_EXCEEDED => 0x8;
public static UntypedInt ETHTOOL_LINK_EXT_STATE_OVERHEAT => 0x9;
public static UntypedInt ETHTOOL_LINK_EXT_SUBSTATE_AN_NO_PARTNER_DETECTED => 0x1;
public static UntypedInt ETHTOOL_LINK_EXT_SUBSTATE_AN_ACK_NOT_RECEIVED => 0x2;
public static UntypedInt ETHTOOL_LINK_EXT_SUBSTATE_AN_NEXT_PAGE_EXCHANGE_FAILED => 0x3;
public static UntypedInt ETHTOOL_LINK_EXT_SUBSTATE_AN_NO_PARTNER_DETECTED_FORCE_MODE => 0x4;
public static UntypedInt ETHTOOL_LINK_EXT_SUBSTATE_AN_FEC_MISMATCH_DURING_OVERRIDE => 0x5;
public static UntypedInt ETHTOOL_LINK_EXT_SUBSTATE_AN_NO_HCD => 0x6;
public static UntypedInt ETHTOOL_LINK_EXT_SUBSTATE_LT_KR_FRAME_LOCK_NOT_ACQUIRED => 0x1;
public static UntypedInt ETHTOOL_LINK_EXT_SUBSTATE_LT_KR_LINK_INHIBIT_TIMEOUT => 0x2;
public static UntypedInt ETHTOOL_LINK_EXT_SUBSTATE_LT_KR_LINK_PARTNER_DID_NOT_SET_RECEIVER_READY => 0x3;
public static UntypedInt ETHTOOL_LINK_EXT_SUBSTATE_LT_REMOTE_FAULT => 0x4;
public static UntypedInt ETHTOOL_LINK_EXT_SUBSTATE_LLM_PCS_DID_NOT_ACQUIRE_BLOCK_LOCK => 0x1;
public static UntypedInt ETHTOOL_LINK_EXT_SUBSTATE_LLM_PCS_DID_NOT_ACQUIRE_AM_LOCK => 0x2;
public static UntypedInt ETHTOOL_LINK_EXT_SUBSTATE_LLM_PCS_DID_NOT_GET_ALIGN_STATUS => 0x3;
public static UntypedInt ETHTOOL_LINK_EXT_SUBSTATE_LLM_FC_FEC_IS_NOT_LOCKED => 0x4;
public static UntypedInt ETHTOOL_LINK_EXT_SUBSTATE_LLM_RS_FEC_IS_NOT_LOCKED => 0x5;
public static UntypedInt ETHTOOL_LINK_EXT_SUBSTATE_BSI_LARGE_NUMBER_OF_PHYSICAL_ERRORS => 0x1;
public static UntypedInt ETHTOOL_LINK_EXT_SUBSTATE_BSI_UNSUPPORTED_RATE => 0x2;
public static UntypedInt ETHTOOL_LINK_EXT_SUBSTATE_CI_UNSUPPORTED_CABLE => 0x1;
public static UntypedInt ETHTOOL_LINK_EXT_SUBSTATE_CI_CABLE_TEST_FAILURE => 0x2;
public static UntypedInt ETHTOOL_FLASH_ALL_REGIONS => 0x0;
public static UntypedInt ETHTOOL_F_UNSUPPORTED__BIT => 0x0;
public static UntypedInt ETHTOOL_F_WISH__BIT => 0x1;
public static UntypedInt ETHTOOL_F_COMPAT__BIT => 0x2;
public static UntypedInt ETHTOOL_FEC_NONE_BIT => 0x0;
public static UntypedInt ETHTOOL_FEC_AUTO_BIT => 0x1;
public static UntypedInt ETHTOOL_FEC_OFF_BIT => 0x2;
public static UntypedInt ETHTOOL_FEC_RS_BIT => 0x3;
public static UntypedInt ETHTOOL_FEC_BASER_BIT => 0x4;
public static UntypedInt ETHTOOL_FEC_LLRS_BIT => 0x5;
public static UntypedInt ETHTOOL_LINK_MODE_10baseT_Half_BIT => 0x0;
public static UntypedInt ETHTOOL_LINK_MODE_10baseT_Full_BIT => 0x1;
public static UntypedInt ETHTOOL_LINK_MODE_100baseT_Half_BIT => 0x2;
public static UntypedInt ETHTOOL_LINK_MODE_100baseT_Full_BIT => 0x3;
public static UntypedInt ETHTOOL_LINK_MODE_1000baseT_Half_BIT => 0x4;
public static UntypedInt ETHTOOL_LINK_MODE_1000baseT_Full_BIT => 0x5;
public static UntypedInt ETHTOOL_LINK_MODE_Autoneg_BIT => 0x6;
public static UntypedInt ETHTOOL_LINK_MODE_TP_BIT => 0x7;
public static UntypedInt ETHTOOL_LINK_MODE_AUI_BIT => 0x8;
public static UntypedInt ETHTOOL_LINK_MODE_MII_BIT => 0x9;
public static UntypedInt ETHTOOL_LINK_MODE_FIBRE_BIT => 0xa;
public static UntypedInt ETHTOOL_LINK_MODE_BNC_BIT => 0xb;
public static UntypedInt ETHTOOL_LINK_MODE_10000baseT_Full_BIT => 0xc;
public static UntypedInt ETHTOOL_LINK_MODE_Pause_BIT => 0xd;
public static UntypedInt ETHTOOL_LINK_MODE_Asym_Pause_BIT => 0xe;
public static UntypedInt ETHTOOL_LINK_MODE_2500baseX_Full_BIT => 0xf;
public static UntypedInt ETHTOOL_LINK_MODE_Backplane_BIT => 0x10;
public static UntypedInt ETHTOOL_LINK_MODE_1000baseKX_Full_BIT => 0x11;
public static UntypedInt ETHTOOL_LINK_MODE_10000baseKX4_Full_BIT => 0x12;
public static UntypedInt ETHTOOL_LINK_MODE_10000baseKR_Full_BIT => 0x13;
public static UntypedInt ETHTOOL_LINK_MODE_10000baseR_FEC_BIT => 0x14;
public static UntypedInt ETHTOOL_LINK_MODE_20000baseMLD2_Full_BIT => 0x15;
public static UntypedInt ETHTOOL_LINK_MODE_20000baseKR2_Full_BIT => 0x16;
public static UntypedInt ETHTOOL_LINK_MODE_40000baseKR4_Full_BIT => 0x17;
public static UntypedInt ETHTOOL_LINK_MODE_40000baseCR4_Full_BIT => 0x18;
public static UntypedInt ETHTOOL_LINK_MODE_40000baseSR4_Full_BIT => 0x19;
public static UntypedInt ETHTOOL_LINK_MODE_40000baseLR4_Full_BIT => 0x1a;
public static UntypedInt ETHTOOL_LINK_MODE_56000baseKR4_Full_BIT => 0x1b;
public static UntypedInt ETHTOOL_LINK_MODE_56000baseCR4_Full_BIT => 0x1c;
public static UntypedInt ETHTOOL_LINK_MODE_56000baseSR4_Full_BIT => 0x1d;
public static UntypedInt ETHTOOL_LINK_MODE_56000baseLR4_Full_BIT => 0x1e;
public static UntypedInt ETHTOOL_LINK_MODE_25000baseCR_Full_BIT => 0x1f;
public static UntypedInt ETHTOOL_LINK_MODE_25000baseKR_Full_BIT => 0x20;
public static UntypedInt ETHTOOL_LINK_MODE_25000baseSR_Full_BIT => 0x21;
public static UntypedInt ETHTOOL_LINK_MODE_50000baseCR2_Full_BIT => 0x22;
public static UntypedInt ETHTOOL_LINK_MODE_50000baseKR2_Full_BIT => 0x23;
public static UntypedInt ETHTOOL_LINK_MODE_100000baseKR4_Full_BIT => 0x24;
public static UntypedInt ETHTOOL_LINK_MODE_100000baseSR4_Full_BIT => 0x25;
public static UntypedInt ETHTOOL_LINK_MODE_100000baseCR4_Full_BIT => 0x26;
public static UntypedInt ETHTOOL_LINK_MODE_100000baseLR4_ER4_Full_BIT => 0x27;
public static UntypedInt ETHTOOL_LINK_MODE_50000baseSR2_Full_BIT => 0x28;
public static UntypedInt ETHTOOL_LINK_MODE_1000baseX_Full_BIT => 0x29;
public static UntypedInt ETHTOOL_LINK_MODE_10000baseCR_Full_BIT => 0x2a;
public static UntypedInt ETHTOOL_LINK_MODE_10000baseSR_Full_BIT => 0x2b;
public static UntypedInt ETHTOOL_LINK_MODE_10000baseLR_Full_BIT => 0x2c;
public static UntypedInt ETHTOOL_LINK_MODE_10000baseLRM_Full_BIT => 0x2d;
public static UntypedInt ETHTOOL_LINK_MODE_10000baseER_Full_BIT => 0x2e;
public static UntypedInt ETHTOOL_LINK_MODE_2500baseT_Full_BIT => 0x2f;
public static UntypedInt ETHTOOL_LINK_MODE_5000baseT_Full_BIT => 0x30;
public static UntypedInt ETHTOOL_LINK_MODE_FEC_NONE_BIT => 0x31;
public static UntypedInt ETHTOOL_LINK_MODE_FEC_RS_BIT => 0x32;
public static UntypedInt ETHTOOL_LINK_MODE_FEC_BASER_BIT => 0x33;
public static UntypedInt ETHTOOL_LINK_MODE_50000baseKR_Full_BIT => 0x34;
public static UntypedInt ETHTOOL_LINK_MODE_50000baseSR_Full_BIT => 0x35;
public static UntypedInt ETHTOOL_LINK_MODE_50000baseCR_Full_BIT => 0x36;
public static UntypedInt ETHTOOL_LINK_MODE_50000baseLR_ER_FR_Full_BIT => 0x37;
public static UntypedInt ETHTOOL_LINK_MODE_50000baseDR_Full_BIT => 0x38;
public static UntypedInt ETHTOOL_LINK_MODE_100000baseKR2_Full_BIT => 0x39;
public static UntypedInt ETHTOOL_LINK_MODE_100000baseSR2_Full_BIT => 0x3a;
public static UntypedInt ETHTOOL_LINK_MODE_100000baseCR2_Full_BIT => 0x3b;
public static UntypedInt ETHTOOL_LINK_MODE_100000baseLR2_ER2_FR2_Full_BIT => 0x3c;
public static UntypedInt ETHTOOL_LINK_MODE_100000baseDR2_Full_BIT => 0x3d;
public static UntypedInt ETHTOOL_LINK_MODE_200000baseKR4_Full_BIT => 0x3e;
public static UntypedInt ETHTOOL_LINK_MODE_200000baseSR4_Full_BIT => 0x3f;
public static UntypedInt ETHTOOL_LINK_MODE_200000baseLR4_ER4_FR4_Full_BIT => 0x40;
public static UntypedInt ETHTOOL_LINK_MODE_200000baseDR4_Full_BIT => 0x41;
public static UntypedInt ETHTOOL_LINK_MODE_200000baseCR4_Full_BIT => 0x42;
public static UntypedInt ETHTOOL_LINK_MODE_100baseT1_Full_BIT => 0x43;
public static UntypedInt ETHTOOL_LINK_MODE_1000baseT1_Full_BIT => 0x44;
public static UntypedInt ETHTOOL_LINK_MODE_400000baseKR8_Full_BIT => 0x45;
public static UntypedInt ETHTOOL_LINK_MODE_400000baseSR8_Full_BIT => 0x46;
public static UntypedInt ETHTOOL_LINK_MODE_400000baseLR8_ER8_FR8_Full_BIT => 0x47;
public static UntypedInt ETHTOOL_LINK_MODE_400000baseDR8_Full_BIT => 0x48;
public static UntypedInt ETHTOOL_LINK_MODE_400000baseCR8_Full_BIT => 0x49;
public static UntypedInt ETHTOOL_LINK_MODE_FEC_LLRS_BIT => 0x4a;
public static UntypedInt ETHTOOL_LINK_MODE_100000baseKR_Full_BIT => 0x4b;
public static UntypedInt ETHTOOL_LINK_MODE_100000baseSR_Full_BIT => 0x4c;
public static UntypedInt ETHTOOL_LINK_MODE_100000baseLR_ER_FR_Full_BIT => 0x4d;
public static UntypedInt ETHTOOL_LINK_MODE_100000baseCR_Full_BIT => 0x4e;
public static UntypedInt ETHTOOL_LINK_MODE_100000baseDR_Full_BIT => 0x4f;
public static UntypedInt ETHTOOL_LINK_MODE_200000baseKR2_Full_BIT => 0x50;
public static UntypedInt ETHTOOL_LINK_MODE_200000baseSR2_Full_BIT => 0x51;
public static UntypedInt ETHTOOL_LINK_MODE_200000baseLR2_ER2_FR2_Full_BIT => 0x52;
public static UntypedInt ETHTOOL_LINK_MODE_200000baseDR2_Full_BIT => 0x53;
public static UntypedInt ETHTOOL_LINK_MODE_200000baseCR2_Full_BIT => 0x54;
public static UntypedInt ETHTOOL_LINK_MODE_400000baseKR4_Full_BIT => 0x55;
public static UntypedInt ETHTOOL_LINK_MODE_400000baseSR4_Full_BIT => 0x56;
public static UntypedInt ETHTOOL_LINK_MODE_400000baseLR4_ER4_FR4_Full_BIT => 0x57;
public static UntypedInt ETHTOOL_LINK_MODE_400000baseDR4_Full_BIT => 0x58;
public static UntypedInt ETHTOOL_LINK_MODE_400000baseCR4_Full_BIT => 0x59;
public static UntypedInt ETHTOOL_LINK_MODE_100baseFX_Half_BIT => 0x5a;
public static UntypedInt ETHTOOL_LINK_MODE_100baseFX_Full_BIT => 0x5b;
public static UntypedInt ETHTOOL_MSG_USER_NONE => 0x0;
public static UntypedInt ETHTOOL_MSG_STRSET_GET => 0x1;
public static UntypedInt ETHTOOL_MSG_LINKINFO_GET => 0x2;
public static UntypedInt ETHTOOL_MSG_LINKINFO_SET => 0x3;
public static UntypedInt ETHTOOL_MSG_LINKMODES_GET => 0x4;
public static UntypedInt ETHTOOL_MSG_LINKMODES_SET => 0x5;
public static UntypedInt ETHTOOL_MSG_LINKSTATE_GET => 0x6;
public static UntypedInt ETHTOOL_MSG_DEBUG_GET => 0x7;
public static UntypedInt ETHTOOL_MSG_DEBUG_SET => 0x8;
public static UntypedInt ETHTOOL_MSG_WOL_GET => 0x9;
public static UntypedInt ETHTOOL_MSG_WOL_SET => 0xa;
public static UntypedInt ETHTOOL_MSG_FEATURES_GET => 0xb;
public static UntypedInt ETHTOOL_MSG_FEATURES_SET => 0xc;
public static UntypedInt ETHTOOL_MSG_PRIVFLAGS_GET => 0xd;
public static UntypedInt ETHTOOL_MSG_PRIVFLAGS_SET => 0xe;
public static UntypedInt ETHTOOL_MSG_RINGS_GET => 0xf;
public static UntypedInt ETHTOOL_MSG_RINGS_SET => 0x10;
public static UntypedInt ETHTOOL_MSG_CHANNELS_GET => 0x11;
public static UntypedInt ETHTOOL_MSG_CHANNELS_SET => 0x12;
public static UntypedInt ETHTOOL_MSG_COALESCE_GET => 0x13;
public static UntypedInt ETHTOOL_MSG_COALESCE_SET => 0x14;
public static UntypedInt ETHTOOL_MSG_PAUSE_GET => 0x15;
public static UntypedInt ETHTOOL_MSG_PAUSE_SET => 0x16;
public static UntypedInt ETHTOOL_MSG_EEE_GET => 0x17;
public static UntypedInt ETHTOOL_MSG_EEE_SET => 0x18;
public static UntypedInt ETHTOOL_MSG_TSINFO_GET => 0x19;
public static UntypedInt ETHTOOL_MSG_CABLE_TEST_ACT => 0x1a;
public static UntypedInt ETHTOOL_MSG_CABLE_TEST_TDR_ACT => 0x1b;
public static UntypedInt ETHTOOL_MSG_TUNNEL_INFO_GET => 0x1c;
public static UntypedInt ETHTOOL_MSG_FEC_GET => 0x1d;
public static UntypedInt ETHTOOL_MSG_FEC_SET => 0x1e;
public static UntypedInt ETHTOOL_MSG_MODULE_EEPROM_GET => 0x1f;
public static UntypedInt ETHTOOL_MSG_STATS_GET => 0x20;
public static UntypedInt ETHTOOL_MSG_PHC_VCLOCKS_GET => 0x21;
public static UntypedInt ETHTOOL_MSG_MODULE_GET => 0x22;
public static UntypedInt ETHTOOL_MSG_MODULE_SET => 0x23;
public static UntypedInt ETHTOOL_MSG_PSE_GET => 0x24;
public static UntypedInt ETHTOOL_MSG_PSE_SET => 0x25;
public static UntypedInt ETHTOOL_MSG_RSS_GET => 0x26;
public static UntypedInt ETHTOOL_MSG_PLCA_GET_CFG => 0x27;
public static UntypedInt ETHTOOL_MSG_PLCA_SET_CFG => 0x28;
public static UntypedInt ETHTOOL_MSG_PLCA_GET_STATUS => 0x29;
public static UntypedInt ETHTOOL_MSG_MM_GET => 0x2a;
public static UntypedInt ETHTOOL_MSG_MM_SET => 0x2b;
public static UntypedInt ETHTOOL_MSG_MODULE_FW_FLASH_ACT => 0x2c;
public static UntypedInt ETHTOOL_MSG_PHY_GET => 0x2d;
public static UntypedInt ETHTOOL_MSG_TSCONFIG_GET => 0x2e;
public static UntypedInt ETHTOOL_MSG_TSCONFIG_SET => 0x2f;
public static UntypedInt ETHTOOL_MSG_USER_MAX => 0x33;
public static UntypedInt ETHTOOL_MSG_KERNEL_NONE => 0x0;
public static UntypedInt ETHTOOL_MSG_STRSET_GET_REPLY => 0x1;
public static UntypedInt ETHTOOL_MSG_LINKINFO_GET_REPLY => 0x2;
public static UntypedInt ETHTOOL_MSG_LINKINFO_NTF => 0x3;
public static UntypedInt ETHTOOL_MSG_LINKMODES_GET_REPLY => 0x4;
public static UntypedInt ETHTOOL_MSG_LINKMODES_NTF => 0x5;
public static UntypedInt ETHTOOL_MSG_LINKSTATE_GET_REPLY => 0x6;
public static UntypedInt ETHTOOL_MSG_DEBUG_GET_REPLY => 0x7;
public static UntypedInt ETHTOOL_MSG_DEBUG_NTF => 0x8;
public static UntypedInt ETHTOOL_MSG_WOL_GET_REPLY => 0x9;
public static UntypedInt ETHTOOL_MSG_WOL_NTF => 0xa;
public static UntypedInt ETHTOOL_MSG_FEATURES_GET_REPLY => 0xb;
public static UntypedInt ETHTOOL_MSG_FEATURES_SET_REPLY => 0xc;
public static UntypedInt ETHTOOL_MSG_FEATURES_NTF => 0xd;
public static UntypedInt ETHTOOL_MSG_PRIVFLAGS_GET_REPLY => 0xe;
public static UntypedInt ETHTOOL_MSG_PRIVFLAGS_NTF => 0xf;
public static UntypedInt ETHTOOL_MSG_RINGS_GET_REPLY => 0x10;
public static UntypedInt ETHTOOL_MSG_RINGS_NTF => 0x11;
public static UntypedInt ETHTOOL_MSG_CHANNELS_GET_REPLY => 0x12;
public static UntypedInt ETHTOOL_MSG_CHANNELS_NTF => 0x13;
public static UntypedInt ETHTOOL_MSG_COALESCE_GET_REPLY => 0x14;
public static UntypedInt ETHTOOL_MSG_COALESCE_NTF => 0x15;
public static UntypedInt ETHTOOL_MSG_PAUSE_GET_REPLY => 0x16;
public static UntypedInt ETHTOOL_MSG_PAUSE_NTF => 0x17;
public static UntypedInt ETHTOOL_MSG_EEE_GET_REPLY => 0x18;
public static UntypedInt ETHTOOL_MSG_EEE_NTF => 0x19;
public static UntypedInt ETHTOOL_MSG_TSINFO_GET_REPLY => 0x1a;
public static UntypedInt ETHTOOL_MSG_CABLE_TEST_NTF => 0x1b;
public static UntypedInt ETHTOOL_MSG_CABLE_TEST_TDR_NTF => 0x1c;
public static UntypedInt ETHTOOL_MSG_TUNNEL_INFO_GET_REPLY => 0x1d;
public static UntypedInt ETHTOOL_MSG_FEC_GET_REPLY => 0x1e;
public static UntypedInt ETHTOOL_MSG_FEC_NTF => 0x1f;
public static UntypedInt ETHTOOL_MSG_MODULE_EEPROM_GET_REPLY => 0x20;
public static UntypedInt ETHTOOL_MSG_STATS_GET_REPLY => 0x21;
public static UntypedInt ETHTOOL_MSG_PHC_VCLOCKS_GET_REPLY => 0x22;
public static UntypedInt ETHTOOL_MSG_MODULE_GET_REPLY => 0x23;
public static UntypedInt ETHTOOL_MSG_MODULE_NTF => 0x24;
public static UntypedInt ETHTOOL_MSG_PSE_GET_REPLY => 0x25;
public static UntypedInt ETHTOOL_MSG_RSS_GET_REPLY => 0x26;
public static UntypedInt ETHTOOL_MSG_PLCA_GET_CFG_REPLY => 0x27;
public static UntypedInt ETHTOOL_MSG_PLCA_GET_STATUS_REPLY => 0x28;
public static UntypedInt ETHTOOL_MSG_PLCA_NTF => 0x29;
public static UntypedInt ETHTOOL_MSG_MM_GET_REPLY => 0x2a;
public static UntypedInt ETHTOOL_MSG_MM_NTF => 0x2b;
public static UntypedInt ETHTOOL_MSG_MODULE_FW_FLASH_NTF => 0x2c;
public static UntypedInt ETHTOOL_MSG_PHY_GET_REPLY => 0x2d;
public static UntypedInt ETHTOOL_MSG_PHY_NTF => 0x2e;
public static UntypedInt ETHTOOL_MSG_TSCONFIG_GET_REPLY => 0x2f;
public static UntypedInt ETHTOOL_MSG_TSCONFIG_SET_REPLY => 0x30;
public static UntypedInt ETHTOOL_MSG_KERNEL_MAX => 0x36;
public static UntypedInt ETHTOOL_FLAG_COMPACT_BITSETS => 0x1;
public static UntypedInt ETHTOOL_FLAG_OMIT_REPLY => 0x2;
public static UntypedInt ETHTOOL_FLAG_STATS => 0x4;
public static UntypedInt ETHTOOL_A_HEADER_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_HEADER_DEV_INDEX => 0x1;
public static UntypedInt ETHTOOL_A_HEADER_DEV_NAME => 0x2;
public static UntypedInt ETHTOOL_A_HEADER_FLAGS => 0x3;
public static UntypedInt ETHTOOL_A_HEADER_MAX => 0x4;
public static UntypedInt ETHTOOL_A_BITSET_BIT_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_BITSET_BIT_INDEX => 0x1;
public static UntypedInt ETHTOOL_A_BITSET_BIT_NAME => 0x2;
public static UntypedInt ETHTOOL_A_BITSET_BIT_VALUE => 0x3;
public static UntypedInt ETHTOOL_A_BITSET_BIT_MAX => 0x3;
public static UntypedInt ETHTOOL_A_BITSET_BITS_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_BITSET_BITS_BIT => 0x1;
public static UntypedInt ETHTOOL_A_BITSET_BITS_MAX => 0x1;
public static UntypedInt ETHTOOL_A_BITSET_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_BITSET_NOMASK => 0x1;
public static UntypedInt ETHTOOL_A_BITSET_SIZE => 0x2;
public static UntypedInt ETHTOOL_A_BITSET_BITS => 0x3;
public static UntypedInt ETHTOOL_A_BITSET_VALUE => 0x4;
public static UntypedInt ETHTOOL_A_BITSET_MASK => 0x5;
public static UntypedInt ETHTOOL_A_BITSET_MAX => 0x5;
public static UntypedInt ETHTOOL_A_STRING_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_STRING_INDEX => 0x1;
public static UntypedInt ETHTOOL_A_STRING_VALUE => 0x2;
public static UntypedInt ETHTOOL_A_STRING_MAX => 0x2;
public static UntypedInt ETHTOOL_A_STRINGS_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_STRINGS_STRING => 0x1;
public static UntypedInt ETHTOOL_A_STRINGS_MAX => 0x1;
public static UntypedInt ETHTOOL_A_STRINGSET_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_STRINGSET_ID => 0x1;
public static UntypedInt ETHTOOL_A_STRINGSET_COUNT => 0x2;
public static UntypedInt ETHTOOL_A_STRINGSET_STRINGS => 0x3;
public static UntypedInt ETHTOOL_A_STRINGSET_MAX => 0x3;
public static UntypedInt ETHTOOL_A_STRINGSETS_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_STRINGSETS_STRINGSET => 0x1;
public static UntypedInt ETHTOOL_A_STRINGSETS_MAX => 0x1;
public static UntypedInt ETHTOOL_A_STRSET_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_STRSET_HEADER => 0x1;
public static UntypedInt ETHTOOL_A_STRSET_STRINGSETS => 0x2;
public static UntypedInt ETHTOOL_A_STRSET_COUNTS_ONLY => 0x3;
public static UntypedInt ETHTOOL_A_STRSET_MAX => 0x3;
public static UntypedInt ETHTOOL_A_LINKINFO_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_LINKINFO_HEADER => 0x1;
public static UntypedInt ETHTOOL_A_LINKINFO_PORT => 0x2;
public static UntypedInt ETHTOOL_A_LINKINFO_PHYADDR => 0x3;
public static UntypedInt ETHTOOL_A_LINKINFO_TP_MDIX => 0x4;
public static UntypedInt ETHTOOL_A_LINKINFO_TP_MDIX_CTRL => 0x5;
public static UntypedInt ETHTOOL_A_LINKINFO_TRANSCEIVER => 0x6;
public static UntypedInt ETHTOOL_A_LINKINFO_MAX => 0x6;
public static UntypedInt ETHTOOL_A_LINKMODES_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_LINKMODES_HEADER => 0x1;
public static UntypedInt ETHTOOL_A_LINKMODES_AUTONEG => 0x2;
public static UntypedInt ETHTOOL_A_LINKMODES_OURS => 0x3;
public static UntypedInt ETHTOOL_A_LINKMODES_PEER => 0x4;
public static UntypedInt ETHTOOL_A_LINKMODES_SPEED => 0x5;
public static UntypedInt ETHTOOL_A_LINKMODES_DUPLEX => 0x6;
public static UntypedInt ETHTOOL_A_LINKMODES_MASTER_SLAVE_CFG => 0x7;
public static UntypedInt ETHTOOL_A_LINKMODES_MASTER_SLAVE_STATE => 0x8;
public static UntypedInt ETHTOOL_A_LINKMODES_LANES => 0x9;
public static UntypedInt ETHTOOL_A_LINKMODES_RATE_MATCHING => 0xa;
public static UntypedInt ETHTOOL_A_LINKMODES_MAX => 0xa;
public static UntypedInt ETHTOOL_A_LINKSTATE_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_LINKSTATE_HEADER => 0x1;
public static UntypedInt ETHTOOL_A_LINKSTATE_LINK => 0x2;
public static UntypedInt ETHTOOL_A_LINKSTATE_SQI => 0x3;
public static UntypedInt ETHTOOL_A_LINKSTATE_SQI_MAX => 0x4;
public static UntypedInt ETHTOOL_A_LINKSTATE_EXT_STATE => 0x5;
public static UntypedInt ETHTOOL_A_LINKSTATE_EXT_SUBSTATE => 0x6;
public static UntypedInt ETHTOOL_A_LINKSTATE_EXT_DOWN_CNT => 0x7;
public static UntypedInt ETHTOOL_A_LINKSTATE_MAX => 0x7;
public static UntypedInt ETHTOOL_A_DEBUG_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_DEBUG_HEADER => 0x1;
public static UntypedInt ETHTOOL_A_DEBUG_MSGMASK => 0x2;
public static UntypedInt ETHTOOL_A_DEBUG_MAX => 0x2;
public static UntypedInt ETHTOOL_A_WOL_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_WOL_HEADER => 0x1;
public static UntypedInt ETHTOOL_A_WOL_MODES => 0x2;
public static UntypedInt ETHTOOL_A_WOL_SOPASS => 0x3;
public static UntypedInt ETHTOOL_A_WOL_MAX => 0x3;
public static UntypedInt ETHTOOL_A_FEATURES_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_FEATURES_HEADER => 0x1;
public static UntypedInt ETHTOOL_A_FEATURES_HW => 0x2;
public static UntypedInt ETHTOOL_A_FEATURES_WANTED => 0x3;
public static UntypedInt ETHTOOL_A_FEATURES_ACTIVE => 0x4;
public static UntypedInt ETHTOOL_A_FEATURES_NOCHANGE => 0x5;
public static UntypedInt ETHTOOL_A_FEATURES_MAX => 0x5;
public static UntypedInt ETHTOOL_A_PRIVFLAGS_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_PRIVFLAGS_HEADER => 0x1;
public static UntypedInt ETHTOOL_A_PRIVFLAGS_FLAGS => 0x2;
public static UntypedInt ETHTOOL_A_PRIVFLAGS_MAX => 0x2;
public static UntypedInt ETHTOOL_A_RINGS_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_RINGS_HEADER => 0x1;
public static UntypedInt ETHTOOL_A_RINGS_RX_MAX => 0x2;
public static UntypedInt ETHTOOL_A_RINGS_RX_MINI_MAX => 0x3;
public static UntypedInt ETHTOOL_A_RINGS_RX_JUMBO_MAX => 0x4;
public static UntypedInt ETHTOOL_A_RINGS_TX_MAX => 0x5;
public static UntypedInt ETHTOOL_A_RINGS_RX => 0x6;
public static UntypedInt ETHTOOL_A_RINGS_RX_MINI => 0x7;
public static UntypedInt ETHTOOL_A_RINGS_RX_JUMBO => 0x8;
public static UntypedInt ETHTOOL_A_RINGS_TX => 0x9;
public static UntypedInt ETHTOOL_A_RINGS_RX_BUF_LEN => 0xa;
public static UntypedInt ETHTOOL_A_RINGS_TCP_DATA_SPLIT => 0xb;
public static UntypedInt ETHTOOL_A_RINGS_CQE_SIZE => 0xc;
public static UntypedInt ETHTOOL_A_RINGS_TX_PUSH => 0xd;
public static UntypedInt ETHTOOL_A_RINGS_RX_PUSH => 0xe;
public static UntypedInt ETHTOOL_A_RINGS_TX_PUSH_BUF_LEN => 0xf;
public static UntypedInt ETHTOOL_A_RINGS_TX_PUSH_BUF_LEN_MAX => 0x10;
public static UntypedInt ETHTOOL_A_RINGS_HDS_THRESH => 0x11;
public static UntypedInt ETHTOOL_A_RINGS_HDS_THRESH_MAX => 0x12;
public static UntypedInt ETHTOOL_A_RINGS_MAX => 0x12;
public static UntypedInt ETHTOOL_A_CHANNELS_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_CHANNELS_HEADER => 0x1;
public static UntypedInt ETHTOOL_A_CHANNELS_RX_MAX => 0x2;
public static UntypedInt ETHTOOL_A_CHANNELS_TX_MAX => 0x3;
public static UntypedInt ETHTOOL_A_CHANNELS_OTHER_MAX => 0x4;
public static UntypedInt ETHTOOL_A_CHANNELS_COMBINED_MAX => 0x5;
public static UntypedInt ETHTOOL_A_CHANNELS_RX_COUNT => 0x6;
public static UntypedInt ETHTOOL_A_CHANNELS_TX_COUNT => 0x7;
public static UntypedInt ETHTOOL_A_CHANNELS_OTHER_COUNT => 0x8;
public static UntypedInt ETHTOOL_A_CHANNELS_COMBINED_COUNT => 0x9;
public static UntypedInt ETHTOOL_A_CHANNELS_MAX => 0x9;
public static UntypedInt ETHTOOL_A_COALESCE_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_COALESCE_HEADER => 0x1;
public static UntypedInt ETHTOOL_A_COALESCE_RX_USECS => 0x2;
public static UntypedInt ETHTOOL_A_COALESCE_RX_MAX_FRAMES => 0x3;
public static UntypedInt ETHTOOL_A_COALESCE_RX_USECS_IRQ => 0x4;
public static UntypedInt ETHTOOL_A_COALESCE_RX_MAX_FRAMES_IRQ => 0x5;
public static UntypedInt ETHTOOL_A_COALESCE_TX_USECS => 0x6;
public static UntypedInt ETHTOOL_A_COALESCE_TX_MAX_FRAMES => 0x7;
public static UntypedInt ETHTOOL_A_COALESCE_TX_USECS_IRQ => 0x8;
public static UntypedInt ETHTOOL_A_COALESCE_TX_MAX_FRAMES_IRQ => 0x9;
public static UntypedInt ETHTOOL_A_COALESCE_STATS_BLOCK_USECS => 0xa;
public static UntypedInt ETHTOOL_A_COALESCE_USE_ADAPTIVE_RX => 0xb;
public static UntypedInt ETHTOOL_A_COALESCE_USE_ADAPTIVE_TX => 0xc;
public static UntypedInt ETHTOOL_A_COALESCE_PKT_RATE_LOW => 0xd;
public static UntypedInt ETHTOOL_A_COALESCE_RX_USECS_LOW => 0xe;
public static UntypedInt ETHTOOL_A_COALESCE_RX_MAX_FRAMES_LOW => 0xf;
public static UntypedInt ETHTOOL_A_COALESCE_TX_USECS_LOW => 0x10;
public static UntypedInt ETHTOOL_A_COALESCE_TX_MAX_FRAMES_LOW => 0x11;
public static UntypedInt ETHTOOL_A_COALESCE_PKT_RATE_HIGH => 0x12;
public static UntypedInt ETHTOOL_A_COALESCE_RX_USECS_HIGH => 0x13;
public static UntypedInt ETHTOOL_A_COALESCE_RX_MAX_FRAMES_HIGH => 0x14;
public static UntypedInt ETHTOOL_A_COALESCE_TX_USECS_HIGH => 0x15;
public static UntypedInt ETHTOOL_A_COALESCE_TX_MAX_FRAMES_HIGH => 0x16;
public static UntypedInt ETHTOOL_A_COALESCE_RATE_SAMPLE_INTERVAL => 0x17;
public static UntypedInt ETHTOOL_A_COALESCE_USE_CQE_MODE_TX => 0x18;
public static UntypedInt ETHTOOL_A_COALESCE_USE_CQE_MODE_RX => 0x19;
public static UntypedInt ETHTOOL_A_COALESCE_MAX => 0x20;
public static UntypedInt ETHTOOL_A_PAUSE_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_PAUSE_HEADER => 0x1;
public static UntypedInt ETHTOOL_A_PAUSE_AUTONEG => 0x2;
public static UntypedInt ETHTOOL_A_PAUSE_RX => 0x3;
public static UntypedInt ETHTOOL_A_PAUSE_TX => 0x4;
public static UntypedInt ETHTOOL_A_PAUSE_STATS => 0x5;
public static UntypedInt ETHTOOL_A_PAUSE_MAX => 0x6;
public static UntypedInt ETHTOOL_A_PAUSE_STAT_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_PAUSE_STAT_PAD => 0x1;
public static UntypedInt ETHTOOL_A_PAUSE_STAT_TX_FRAMES => 0x2;
public static UntypedInt ETHTOOL_A_PAUSE_STAT_RX_FRAMES => 0x3;
public static UntypedInt ETHTOOL_A_PAUSE_STAT_MAX => 0x4;
public static UntypedInt ETHTOOL_A_EEE_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_EEE_HEADER => 0x1;
public static UntypedInt ETHTOOL_A_EEE_MODES_OURS => 0x2;
public static UntypedInt ETHTOOL_A_EEE_MODES_PEER => 0x3;
public static UntypedInt ETHTOOL_A_EEE_ACTIVE => 0x4;
public static UntypedInt ETHTOOL_A_EEE_ENABLED => 0x5;
public static UntypedInt ETHTOOL_A_EEE_TX_LPI_ENABLED => 0x6;
public static UntypedInt ETHTOOL_A_EEE_TX_LPI_TIMER => 0x7;
public static UntypedInt ETHTOOL_A_EEE_MAX => 0x7;
public static UntypedInt ETHTOOL_A_TSINFO_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_TSINFO_HEADER => 0x1;
public static UntypedInt ETHTOOL_A_TSINFO_TIMESTAMPING => 0x2;
public static UntypedInt ETHTOOL_A_TSINFO_TX_TYPES => 0x3;
public static UntypedInt ETHTOOL_A_TSINFO_RX_FILTERS => 0x4;
public static UntypedInt ETHTOOL_A_TSINFO_PHC_INDEX => 0x5;
public static UntypedInt ETHTOOL_A_TSINFO_STATS => 0x6;
public static UntypedInt ETHTOOL_A_TSINFO_HWTSTAMP_PROVIDER => 0x7;
public static UntypedInt ETHTOOL_A_TSINFO_MAX => 0x9;
public static UntypedInt ETHTOOL_A_CABLE_TEST_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_CABLE_TEST_HEADER => 0x1;
public static UntypedInt ETHTOOL_A_CABLE_TEST_MAX => 0x1;
public static UntypedInt ETHTOOL_A_CABLE_RESULT_CODE_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_CABLE_RESULT_CODE_OK => 0x1;
public static UntypedInt ETHTOOL_A_CABLE_RESULT_CODE_OPEN => 0x2;
public static UntypedInt ETHTOOL_A_CABLE_RESULT_CODE_SAME_SHORT => 0x3;
public static UntypedInt ETHTOOL_A_CABLE_RESULT_CODE_CROSS_SHORT => 0x4;
public static UntypedInt ETHTOOL_A_CABLE_PAIR_A => 0x0;
public static UntypedInt ETHTOOL_A_CABLE_PAIR_B => 0x1;
public static UntypedInt ETHTOOL_A_CABLE_PAIR_C => 0x2;
public static UntypedInt ETHTOOL_A_CABLE_PAIR_D => 0x3;
public static UntypedInt ETHTOOL_A_CABLE_RESULT_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_CABLE_RESULT_PAIR => 0x1;
public static UntypedInt ETHTOOL_A_CABLE_RESULT_CODE => 0x2;
public static UntypedInt ETHTOOL_A_CABLE_RESULT_MAX => 0x3;
public static UntypedInt ETHTOOL_A_CABLE_FAULT_LENGTH_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_CABLE_FAULT_LENGTH_PAIR => 0x1;
public static UntypedInt ETHTOOL_A_CABLE_FAULT_LENGTH_CM => 0x2;
public static UntypedInt ETHTOOL_A_CABLE_FAULT_LENGTH_MAX => 0x3;
public static UntypedInt ETHTOOL_A_CABLE_TEST_NTF_STATUS_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_CABLE_TEST_NTF_STATUS_STARTED => 0x1;
public static UntypedInt ETHTOOL_A_CABLE_TEST_NTF_STATUS_COMPLETED => 0x2;
public static UntypedInt ETHTOOL_A_CABLE_NEST_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_CABLE_NEST_RESULT => 0x1;
public static UntypedInt ETHTOOL_A_CABLE_NEST_FAULT_LENGTH => 0x2;
public static UntypedInt ETHTOOL_A_CABLE_NEST_MAX => 0x2;
public static UntypedInt ETHTOOL_A_CABLE_TEST_NTF_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_CABLE_TEST_NTF_HEADER => 0x1;
public static UntypedInt ETHTOOL_A_CABLE_TEST_NTF_STATUS => 0x2;
public static UntypedInt ETHTOOL_A_CABLE_TEST_NTF_NEST => 0x3;
public static UntypedInt ETHTOOL_A_CABLE_TEST_NTF_MAX => 0x3;
public static UntypedInt ETHTOOL_A_CABLE_TEST_TDR_CFG_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_CABLE_TEST_TDR_CFG_FIRST => 0x1;
public static UntypedInt ETHTOOL_A_CABLE_TEST_TDR_CFG_LAST => 0x2;
public static UntypedInt ETHTOOL_A_CABLE_TEST_TDR_CFG_STEP => 0x3;
public static UntypedInt ETHTOOL_A_CABLE_TEST_TDR_CFG_PAIR => 0x4;
public static UntypedInt ETHTOOL_A_CABLE_TEST_TDR_CFG_MAX => 0x4;
public static UntypedInt ETHTOOL_A_CABLE_TEST_TDR_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_CABLE_TEST_TDR_HEADER => 0x1;
public static UntypedInt ETHTOOL_A_CABLE_TEST_TDR_CFG => 0x2;
public static UntypedInt ETHTOOL_A_CABLE_TEST_TDR_MAX => 0x2;
public static UntypedInt ETHTOOL_A_CABLE_AMPLITUDE_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_CABLE_AMPLITUDE_PAIR => 0x1;
public static UntypedInt ETHTOOL_A_CABLE_AMPLITUDE_mV => 0x2;
public static UntypedInt ETHTOOL_A_CABLE_AMPLITUDE_MAX => 0x2;
public static UntypedInt ETHTOOL_A_CABLE_PULSE_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_CABLE_PULSE_mV => 0x1;
public static UntypedInt ETHTOOL_A_CABLE_PULSE_MAX => 0x1;
public static UntypedInt ETHTOOL_A_CABLE_STEP_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_CABLE_STEP_FIRST_DISTANCE => 0x1;
public static UntypedInt ETHTOOL_A_CABLE_STEP_LAST_DISTANCE => 0x2;
public static UntypedInt ETHTOOL_A_CABLE_STEP_STEP_DISTANCE => 0x3;
public static UntypedInt ETHTOOL_A_CABLE_STEP_MAX => 0x3;
public static UntypedInt ETHTOOL_A_CABLE_TDR_NEST_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_CABLE_TDR_NEST_STEP => 0x1;
public static UntypedInt ETHTOOL_A_CABLE_TDR_NEST_AMPLITUDE => 0x2;
public static UntypedInt ETHTOOL_A_CABLE_TDR_NEST_PULSE => 0x3;
public static UntypedInt ETHTOOL_A_CABLE_TDR_NEST_MAX => 0x3;
public static UntypedInt ETHTOOL_A_CABLE_TEST_TDR_NTF_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_CABLE_TEST_TDR_NTF_HEADER => 0x1;
public static UntypedInt ETHTOOL_A_CABLE_TEST_TDR_NTF_STATUS => 0x2;
public static UntypedInt ETHTOOL_A_CABLE_TEST_TDR_NTF_NEST => 0x3;
public static UntypedInt ETHTOOL_A_CABLE_TEST_TDR_NTF_MAX => 0x3;
public static UntypedInt ETHTOOL_UDP_TUNNEL_TYPE_VXLAN => 0x0;
public static UntypedInt ETHTOOL_UDP_TUNNEL_TYPE_GENEVE => 0x1;
public static UntypedInt ETHTOOL_UDP_TUNNEL_TYPE_VXLAN_GPE => 0x2;
public static UntypedInt ETHTOOL_A_TUNNEL_UDP_ENTRY_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_TUNNEL_UDP_ENTRY_PORT => 0x1;
public static UntypedInt ETHTOOL_A_TUNNEL_UDP_ENTRY_TYPE => 0x2;
public static UntypedInt ETHTOOL_A_TUNNEL_UDP_ENTRY_MAX => 0x2;
public static UntypedInt ETHTOOL_A_TUNNEL_UDP_TABLE_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_TUNNEL_UDP_TABLE_SIZE => 0x1;
public static UntypedInt ETHTOOL_A_TUNNEL_UDP_TABLE_TYPES => 0x2;
public static UntypedInt ETHTOOL_A_TUNNEL_UDP_TABLE_ENTRY => 0x3;
public static UntypedInt ETHTOOL_A_TUNNEL_UDP_TABLE_MAX => 0x3;
public static UntypedInt ETHTOOL_A_TUNNEL_UDP_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_TUNNEL_UDP_TABLE => 0x1;
public static UntypedInt ETHTOOL_A_TUNNEL_UDP_MAX => 0x1;
public static UntypedInt ETHTOOL_A_TUNNEL_INFO_UNSPEC => 0x0;
public static UntypedInt ETHTOOL_A_TUNNEL_INFO_HEADER => 0x1;
public static UntypedInt ETHTOOL_A_TUNNEL_INFO_UDP_PORTS => 0x2;
public static UntypedInt ETHTOOL_A_TUNNEL_INFO_MAX => 0x2;

public static UntypedInt TCP_V4_FLOW => 0x1;
public static UntypedInt UDP_V4_FLOW => 0x2;
public static UntypedInt TCP_V6_FLOW => 0x5;
public static UntypedInt UDP_V6_FLOW => 0x6;
public static UntypedInt ESP_V4_FLOW => 0xa;
public static UntypedInt ESP_V6_FLOW => 0xc;
public static UntypedInt IP_USER_FLOW => 0xd;
public static UntypedInt IPV6_USER_FLOW => 0xe;
public static UntypedInt IPV6_FLOW => 0x11;
public static UntypedInt ETHER_FLOW => 0x12;

public static UntypedInt SPEED_UNKNOWN => /* -0x1 */ -1;

[global::go.golib.GoInlineLayout(Size = 196, Align = 4), StructLayout(LayoutKind.Explicit, Size = 196)] partial struct EthtoolDrvinfo {
    [FieldOffset(0)] public uint32 Cmd;
    [FieldOffset(4)] public array32<byte> Driver;
    [FieldOffset(36)] public array32<byte> Version;
    [FieldOffset(68)] public array32<byte> Fw_version;
    [FieldOffset(100)] public array32<byte> Bus_info;
    [FieldOffset(132)] public array32<byte> Erom_version;
    [FieldOffset(164)] public array12<byte> Reserved2;
    [FieldOffset(176)] public uint32 N_priv_flags;
    [FieldOffset(180)] public uint32 N_stats;
    [FieldOffset(184)] public uint32 Testinfo_len;
    [FieldOffset(188)] public uint32 Eedump_len;
    [FieldOffset(192)] public uint32 Regdump_len;
}

[global::go.golib.GoInlineLayout(Size = 44, Align = 4), StructLayout(LayoutKind.Explicit, Size = 44)] partial struct EthtoolTsInfo {
    [FieldOffset(0)] public uint32 Cmd;
    [FieldOffset(4)] public uint32 So_timestamping;
    [FieldOffset(8)] public int32 Phc_index;
    [FieldOffset(12)] public uint32 Tx_types;
    [FieldOffset(16)] public array3<uint32> Tx_reserved;
    [FieldOffset(28)] public uint32 Rx_filters;
    [FieldOffset(32)] public array3<uint32> Rx_reserved;
}

partial struct HwTstampConfig {
    public int32 Flags;
    public int32 Tx_type;
    public int32 Rx_filter;
}

public static UntypedInt HWTSTAMP_FILTER_NONE => 0x0;
public static UntypedInt HWTSTAMP_FILTER_ALL => 0x1;
public static UntypedInt HWTSTAMP_FILTER_SOME => 0x2;
public static UntypedInt HWTSTAMP_FILTER_PTP_V1_L4_EVENT => 0x3;
public static UntypedInt HWTSTAMP_FILTER_PTP_V1_L4_SYNC => 0x4;
public static UntypedInt HWTSTAMP_FILTER_PTP_V1_L4_DELAY_REQ => 0x5;
public static UntypedInt HWTSTAMP_FILTER_PTP_V2_L4_EVENT => 0x6;
public static UntypedInt HWTSTAMP_FILTER_PTP_V2_L4_SYNC => 0x7;
public static UntypedInt HWTSTAMP_FILTER_PTP_V2_L4_DELAY_REQ => 0x8;
public static UntypedInt HWTSTAMP_FILTER_PTP_V2_L2_EVENT => 0x9;
public static UntypedInt HWTSTAMP_FILTER_PTP_V2_L2_SYNC => 0xa;
public static UntypedInt HWTSTAMP_FILTER_PTP_V2_L2_DELAY_REQ => 0xb;
public static UntypedInt HWTSTAMP_FILTER_PTP_V2_EVENT => 0xc;
public static UntypedInt HWTSTAMP_FILTER_PTP_V2_SYNC => 0xd;
public static UntypedInt HWTSTAMP_FILTER_PTP_V2_DELAY_REQ => 0xe;
public static UntypedInt HWTSTAMP_FILTER_NTP_ALL => 0xf;

public static UntypedInt HWTSTAMP_TX_OFF => 0x0;
public static UntypedInt HWTSTAMP_TX_ON => 0x1;
public static UntypedInt HWTSTAMP_TX_ONESTEP_SYNC => 0x2;
public static UntypedInt HWTSTAMP_TX_ONESTEP_P2P => 0x3;

[global::go.golib.GoInlineLayout(Size = 80, Align = 4), StructLayout(LayoutKind.Explicit, Size = 80)] partial struct PtpClockCaps {
    [FieldOffset(0)] public int32 Max_adj;
    [FieldOffset(4)] public int32 N_alarm;
    [FieldOffset(8)] public int32 N_ext_ts;
    [FieldOffset(12)] public int32 N_per_out;
    [FieldOffset(16)] public int32 Pps;
    [FieldOffset(20)] public int32 N_pins;
    [FieldOffset(24)] public int32 Cross_timestamping;
    [FieldOffset(28)] public int32 Adjust_phase;
    [FieldOffset(32)] public int32 Max_phase_adj;
    [FieldOffset(36)] public array11<int32> Rsv;
}

[global::go.golib.GoInlineLayout(Size = 16, Align = 8), StructLayout(LayoutKind.Explicit, Size = 16)] partial struct PtpClockTime {
    [FieldOffset(0)] public int64 Sec;
    [FieldOffset(8)] public uint32 Nsec;
    [FieldOffset(12)] public uint32 Reserved;
}

partial struct PtpExttsEvent {
    public PtpClockTime T;
    public uint32 Index;
    public uint32 Flags;
    public array<uint32> Rsv = new(2);
}

[global::go.golib.GoInlineLayout(Size = 16, Align = 4), StructLayout(LayoutKind.Explicit, Size = 16)] partial struct PtpExttsRequest {
    [FieldOffset(0)] public uint32 Index;
    [FieldOffset(4)] public uint32 Flags;
    [FieldOffset(8)] public array2<uint32> Rsv;
}

partial struct PtpPeroutRequest {
    public PtpClockTime StartOrPhase;
    public PtpClockTime Period;
    public uint32 Index;
    public uint32 Flags;
    public PtpClockTime On;
}

[global::go.golib.GoInlineLayout(Size = 96, Align = 4), StructLayout(LayoutKind.Explicit, Size = 96)] partial struct PtpPinDesc {
    [FieldOffset(0)] public array64<byte> Name;
    [FieldOffset(64)] public uint32 Index;
    [FieldOffset(68)] public uint32 Func;
    [FieldOffset(72)] public uint32 Chan;
    [FieldOffset(76)] public array5<uint32> Rsv;
}

partial struct PtpSysOffset {
    public uint32 Samples;
    public array<uint32> Rsv = new(3);
    public array<PtpClockTime> Ts = new(51);
}

[global::go.golib.GoInlineLayout(Size = 1216, Align = 8), StructLayout(LayoutKind.Explicit, Size = 1216)] partial struct PtpSysOffsetExtended {
    [FieldOffset(0)] public uint32 Samples;
    [FieldOffset(4)] public int32 Clockid;
    [FieldOffset(8)] public array2<uint32> Rsv;
    [FieldOffset(16)] public array25<array3<PtpClockTime>> Ts;
}

[global::go.golib.GoInlineLayout(Size = 64, Align = 8), StructLayout(LayoutKind.Explicit, Size = 64)] partial struct PtpSysOffsetPrecise {
    [FieldOffset(0)] public PtpClockTime Device;
    [FieldOffset(16)] public PtpClockTime Realtime;
    [FieldOffset(32)] public PtpClockTime Monoraw;
    [FieldOffset(48)] public array4<uint32> Rsv;
}

public static UntypedInt PTP_PF_NONE => 0x0;
public static UntypedInt PTP_PF_EXTTS => 0x1;
public static UntypedInt PTP_PF_PEROUT => 0x2;
public static UntypedInt PTP_PF_PHYSYNC => 0x3;

partial struct HIDRawReportDescriptor {
    public uint32 Size;
    public array<uint8> Value = new(4096);
}

partial struct HIDRawDevInfo {
    public uint32 Bustype;
    public int16 Vendor;
    public int16 Product;
}

public static UntypedInt CLOSE_RANGE_UNSHARE => 0x2;
public static UntypedInt CLOSE_RANGE_CLOEXEC => 0x4;

public static UntypedInt NLMSGERR_ATTR_MSG => 0x1;
public static UntypedInt NLMSGERR_ATTR_OFFS => 0x2;
public static UntypedInt NLMSGERR_ATTR_COOKIE => 0x3;

partial struct EraseInfo {
    public uint32 Start;
    public uint32 Length;
}

partial struct EraseInfo64 {
    public uint64 Start;
    public uint64 Length;
}

partial struct MtdOobBuf {
    public uint32 Start;
    public uint32 Length;
    public ж<uint8> Ptr;
}

partial struct MtdOobBuf64 {
    public uint64 Start;
    public uint32 Pad;
    public uint32 Length;
    public uint64 Ptr;
}

partial struct MtdWriteReq {
    public uint64 Start;
    public uint64 Len;
    public uint64 Ooblen;
    public uint64 Data;
    public uint64 Oob;
    public uint8 Mode;
    internal array<uint8> _ = new(7);
}

partial struct MtdInfo {
    public uint8 Type;
    public uint32 Flags;
    public uint32 Size;
    public uint32 Erasesize;
    public uint32 Writesize;
    public uint32 Oobsize;
    internal uint64 _;
}

partial struct RegionInfo {
    public uint32 Offset;
    public uint32 Erasesize;
    public uint32 Numblocks;
    public uint32 Regionindex;
}

partial struct OtpInfo {
    public uint32 Start;
    public uint32 Length;
    public uint32 Locked;
}

partial struct NandOobinfo {
    public uint32 Useecc;
    public uint32 Eccbytes;
    public array<array<uint32>> Oobfree = new(8, () => new(2));
    public array<uint32> Eccpos = new(32);
}

partial struct NandOobfree {
    public uint32 Offset;
    public uint32 Length;
}

partial struct NandEcclayout {
    public uint32 Eccbytes;
    public array<uint32> Eccpos = new(64);
    public uint32 Oobavail;
    public array<NandOobfree> Oobfree = new(8);
}

partial struct MtdEccStats {
    public uint32 Corrected;
    public uint32 Failed;
    public uint32 Badblocks;
    public uint32 Bbtblocks;
}

public static UntypedInt MTD_OPS_PLACE_OOB => 0x0;
public static UntypedInt MTD_OPS_AUTO_OOB => 0x1;
public static UntypedInt MTD_OPS_RAW => 0x2;

public static UntypedInt MTD_FILE_MODE_NORMAL => 0x0;
public static UntypedInt MTD_FILE_MODE_OTP_FACTORY => 0x1;
public static UntypedInt MTD_FILE_MODE_OTP_USER => 0x2;
public static UntypedInt MTD_FILE_MODE_RAW => 0x3;

public static UntypedInt NFC_CMD_UNSPEC => 0x0;
public static UntypedInt NFC_CMD_GET_DEVICE => 0x1;
public static UntypedInt NFC_CMD_DEV_UP => 0x2;
public static UntypedInt NFC_CMD_DEV_DOWN => 0x3;
public static UntypedInt NFC_CMD_DEP_LINK_UP => 0x4;
public static UntypedInt NFC_CMD_DEP_LINK_DOWN => 0x5;
public static UntypedInt NFC_CMD_START_POLL => 0x6;
public static UntypedInt NFC_CMD_STOP_POLL => 0x7;
public static UntypedInt NFC_CMD_GET_TARGET => 0x8;
public static UntypedInt NFC_EVENT_TARGETS_FOUND => 0x9;
public static UntypedInt NFC_EVENT_DEVICE_ADDED => 0xa;
public static UntypedInt NFC_EVENT_DEVICE_REMOVED => 0xb;
public static UntypedInt NFC_EVENT_TARGET_LOST => 0xc;
public static UntypedInt NFC_EVENT_TM_ACTIVATED => 0xd;
public static UntypedInt NFC_EVENT_TM_DEACTIVATED => 0xe;
public static UntypedInt NFC_CMD_LLC_GET_PARAMS => 0xf;
public static UntypedInt NFC_CMD_LLC_SET_PARAMS => 0x10;
public static UntypedInt NFC_CMD_ENABLE_SE => 0x11;
public static UntypedInt NFC_CMD_DISABLE_SE => 0x12;
public static UntypedInt NFC_CMD_LLC_SDREQ => 0x13;
public static UntypedInt NFC_EVENT_LLC_SDRES => 0x14;
public static UntypedInt NFC_CMD_FW_DOWNLOAD => 0x15;
public static UntypedInt NFC_EVENT_SE_ADDED => 0x16;
public static UntypedInt NFC_EVENT_SE_REMOVED => 0x17;
public static UntypedInt NFC_EVENT_SE_CONNECTIVITY => 0x18;
public static UntypedInt NFC_EVENT_SE_TRANSACTION => 0x19;
public static UntypedInt NFC_CMD_GET_SE => 0x1a;
public static UntypedInt NFC_CMD_SE_IO => 0x1b;
public static UntypedInt NFC_CMD_ACTIVATE_TARGET => 0x1c;
public static UntypedInt NFC_CMD_VENDOR => 0x1d;
public static UntypedInt NFC_CMD_DEACTIVATE_TARGET => 0x1e;
public static UntypedInt NFC_ATTR_UNSPEC => 0x0;
public static UntypedInt NFC_ATTR_DEVICE_INDEX => 0x1;
public static UntypedInt NFC_ATTR_DEVICE_NAME => 0x2;
public static UntypedInt NFC_ATTR_PROTOCOLS => 0x3;
public static UntypedInt NFC_ATTR_TARGET_INDEX => 0x4;
public static UntypedInt NFC_ATTR_TARGET_SENS_RES => 0x5;
public static UntypedInt NFC_ATTR_TARGET_SEL_RES => 0x6;
public static UntypedInt NFC_ATTR_TARGET_NFCID1 => 0x7;
public static UntypedInt NFC_ATTR_TARGET_SENSB_RES => 0x8;
public static UntypedInt NFC_ATTR_TARGET_SENSF_RES => 0x9;
public static UntypedInt NFC_ATTR_COMM_MODE => 0xa;
public static UntypedInt NFC_ATTR_RF_MODE => 0xb;
public static UntypedInt NFC_ATTR_DEVICE_POWERED => 0xc;
public static UntypedInt NFC_ATTR_IM_PROTOCOLS => 0xd;
public static UntypedInt NFC_ATTR_TM_PROTOCOLS => 0xe;
public static UntypedInt NFC_ATTR_LLC_PARAM_LTO => 0xf;
public static UntypedInt NFC_ATTR_LLC_PARAM_RW => 0x10;
public static UntypedInt NFC_ATTR_LLC_PARAM_MIUX => 0x11;
public static UntypedInt NFC_ATTR_SE => 0x12;
public static UntypedInt NFC_ATTR_LLC_SDP => 0x13;
public static UntypedInt NFC_ATTR_FIRMWARE_NAME => 0x14;
public static UntypedInt NFC_ATTR_SE_INDEX => 0x15;
public static UntypedInt NFC_ATTR_SE_TYPE => 0x16;
public static UntypedInt NFC_ATTR_SE_AID => 0x17;
public static UntypedInt NFC_ATTR_FIRMWARE_DOWNLOAD_STATUS => 0x18;
public static UntypedInt NFC_ATTR_SE_APDU => 0x19;
public static UntypedInt NFC_ATTR_TARGET_ISO15693_DSFID => 0x1a;
public static UntypedInt NFC_ATTR_TARGET_ISO15693_UID => 0x1b;
public static UntypedInt NFC_ATTR_SE_PARAMS => 0x1c;
public static UntypedInt NFC_ATTR_VENDOR_ID => 0x1d;
public static UntypedInt NFC_ATTR_VENDOR_SUBCMD => 0x1e;
public static UntypedInt NFC_ATTR_VENDOR_DATA => 0x1f;
public static UntypedInt NFC_SDP_ATTR_UNSPEC => 0x0;
public static UntypedInt NFC_SDP_ATTR_URI => 0x1;
public static UntypedInt NFC_SDP_ATTR_SAP => 0x2;

partial struct LandlockRulesetAttr {
    public uint64 Handled_access_fs;
    public uint64 Handled_access_net;
    public uint64 Scoped;
    public uint64 Quiet_access_fs;
    public uint64 Quiet_access_net;
    public uint64 Quiet_scoped;
}

partial struct LandlockPathBeneathAttr {
    public uint64 Allowed_access;
    public int32 Parent_fd;
}

public static UntypedInt LANDLOCK_RULE_PATH_BENEATH => 0x1;

public static UntypedInt PIDFD_SIGNAL_THREAD => 0x1;
public static UntypedInt PIDFD_SIGNAL_THREAD_GROUP => 0x2;
public static UntypedInt PIDFD_SIGNAL_PROCESS_GROUP => 0x4;
public static UntypedInt PIDFD_SELF_THREAD => /* -0x2710 */ -10000;
public static UntypedInt PIDFD_SELF_THREAD_GROUP => /* -0x2711 */ -10001;
public static UntypedInt PIDFD_SELF => /* -0x2710 */ -10000;
public static UntypedInt PIDFD_SELF_PROCESS => /* -0x2711 */ -10001;
public static UntypedInt PIDFD_INFO_PID => 0x1;
public static UntypedInt PIDFD_INFO_CREDS => 0x2;
public static UntypedInt PIDFD_INFO_CGROUPID => 0x4;
public static UntypedInt PIDFD_INFO_EXIT => 0x8;
public static UntypedInt PIDFD_INFO_COREDUMP => 0x10;
public static UntypedInt PIDFD_COREDUMPED => 0x1;
public static UntypedInt PIDFD_COREDUMP_SKIP => 0x2;
public static UntypedInt PIDFD_COREDUMP_USER => 0x4;
public static UntypedInt PIDFD_COREDUMP_ROOT => 0x8;
public static UntypedInt PIDFD_INFO_SIZE_VER0 => 0x40;
public static UntypedInt PIDFD_GET_INFO => 0xc058ff0b;

public static UntypedInt SizeofPidfdInfo => 0x58;

partial struct PidfdInfo {
    public uint64 Mask;
    public uint64 Cgroupid;
    public uint32 Pid;
    public uint32 Tgid;
    public uint32 Ppid;
    public uint32 Ruid;
    public uint32 Rgid;
    public uint32 Euid;
    public uint32 Egid;
    public uint32 Suid;
    public uint32 Sgid;
    public uint32 Fsuid;
    public uint32 Fsgid;
    public int32 Exit_code;
    public uint32 Coredump_mask;
    public uint32 Coredump_signal;
    public uint32 Coredump_code;
    public uint32 Coredump_pad;
    public uint64 Supported_mask;
}

public static UntypedInt IPC_CREAT => 0x200;
public static UntypedInt IPC_EXCL => 0x400;
public static UntypedInt IPC_NOWAIT => 0x800;
public static UntypedInt IPC_PRIVATE => 0x0;
internal static UntypedInt ipc_64 => 0x100;

public static UntypedInt IPC_RMID => 0x0;
public static UntypedInt IPC_SET => 0x1;
public static UntypedInt IPC_STAT => 0x2;

public static UntypedInt SHM_RDONLY => 0x1000;
public static UntypedInt SHM_RND => 0x2000;

partial struct MountAttr {
    public uint64 Attr_set;
    public uint64 Attr_clr;
    public uint64 Propagation;
    public uint64 Userns_fd;
}

public static UntypedInt WG_CMD_GET_DEVICE => 0x0;
public static UntypedInt WG_CMD_SET_DEVICE => 0x1;
public static UntypedInt WGDEVICE_F_REPLACE_PEERS => 0x1;
public static UntypedInt WGDEVICE_A_UNSPEC => 0x0;
public static UntypedInt WGDEVICE_A_IFINDEX => 0x1;
public static UntypedInt WGDEVICE_A_IFNAME => 0x2;
public static UntypedInt WGDEVICE_A_PRIVATE_KEY => 0x3;
public static UntypedInt WGDEVICE_A_PUBLIC_KEY => 0x4;
public static UntypedInt WGDEVICE_A_FLAGS => 0x5;
public static UntypedInt WGDEVICE_A_LISTEN_PORT => 0x6;
public static UntypedInt WGDEVICE_A_FWMARK => 0x7;
public static UntypedInt WGDEVICE_A_PEERS => 0x8;
public static UntypedInt WGPEER_F_REMOVE_ME => 0x1;
public static UntypedInt WGPEER_F_REPLACE_ALLOWEDIPS => 0x2;
public static UntypedInt WGPEER_F_UPDATE_ONLY => 0x4;
public static UntypedInt WGPEER_A_UNSPEC => 0x0;
public static UntypedInt WGPEER_A_PUBLIC_KEY => 0x1;
public static UntypedInt WGPEER_A_PRESHARED_KEY => 0x2;
public static UntypedInt WGPEER_A_FLAGS => 0x3;
public static UntypedInt WGPEER_A_ENDPOINT => 0x4;
public static UntypedInt WGPEER_A_PERSISTENT_KEEPALIVE_INTERVAL => 0x5;
public static UntypedInt WGPEER_A_LAST_HANDSHAKE_TIME => 0x6;
public static UntypedInt WGPEER_A_RX_BYTES => 0x7;
public static UntypedInt WGPEER_A_TX_BYTES => 0x8;
public static UntypedInt WGPEER_A_ALLOWEDIPS => 0x9;
public static UntypedInt WGPEER_A_PROTOCOL_VERSION => 0xa;
public static UntypedInt WGALLOWEDIP_A_UNSPEC => 0x0;
public static UntypedInt WGALLOWEDIP_A_FAMILY => 0x1;
public static UntypedInt WGALLOWEDIP_A_IPADDR => 0x2;
public static UntypedInt WGALLOWEDIP_A_CIDR_MASK => 0x3;

public static UntypedInt NL_ATTR_TYPE_INVALID => 0x0;
public static UntypedInt NL_ATTR_TYPE_FLAG => 0x1;
public static UntypedInt NL_ATTR_TYPE_U8 => 0x2;
public static UntypedInt NL_ATTR_TYPE_U16 => 0x3;
public static UntypedInt NL_ATTR_TYPE_U32 => 0x4;
public static UntypedInt NL_ATTR_TYPE_U64 => 0x5;
public static UntypedInt NL_ATTR_TYPE_S8 => 0x6;
public static UntypedInt NL_ATTR_TYPE_S16 => 0x7;
public static UntypedInt NL_ATTR_TYPE_S32 => 0x8;
public static UntypedInt NL_ATTR_TYPE_S64 => 0x9;
public static UntypedInt NL_ATTR_TYPE_BINARY => 0xa;
public static UntypedInt NL_ATTR_TYPE_STRING => 0xb;
public static UntypedInt NL_ATTR_TYPE_NUL_STRING => 0xc;
public static UntypedInt NL_ATTR_TYPE_NESTED => 0xd;
public static UntypedInt NL_ATTR_TYPE_NESTED_ARRAY => 0xe;
public static UntypedInt NL_ATTR_TYPE_BITFIELD32 => 0xf;
public static UntypedInt NL_POLICY_TYPE_ATTR_UNSPEC => 0x0;
public static UntypedInt NL_POLICY_TYPE_ATTR_TYPE => 0x1;
public static UntypedInt NL_POLICY_TYPE_ATTR_MIN_VALUE_S => 0x2;
public static UntypedInt NL_POLICY_TYPE_ATTR_MAX_VALUE_S => 0x3;
public static UntypedInt NL_POLICY_TYPE_ATTR_MIN_VALUE_U => 0x4;
public static UntypedInt NL_POLICY_TYPE_ATTR_MAX_VALUE_U => 0x5;
public static UntypedInt NL_POLICY_TYPE_ATTR_MIN_LENGTH => 0x6;
public static UntypedInt NL_POLICY_TYPE_ATTR_MAX_LENGTH => 0x7;
public static UntypedInt NL_POLICY_TYPE_ATTR_POLICY_IDX => 0x8;
public static UntypedInt NL_POLICY_TYPE_ATTR_POLICY_MAXTYPE => 0x9;
public static UntypedInt NL_POLICY_TYPE_ATTR_BITFIELD32_MASK => 0xa;
public static UntypedInt NL_POLICY_TYPE_ATTR_PAD => 0xb;
public static UntypedInt NL_POLICY_TYPE_ATTR_MASK => 0xc;
public static UntypedInt NL_POLICY_TYPE_ATTR_MAX => 0xc;

partial struct CANBitTiming {
    public uint32 Bitrate;
    public uint32 Sample_point;
    public uint32 Tq;
    public uint32 Prop_seg;
    public uint32 Phase_seg1;
    public uint32 Phase_seg2;
    public uint32 Sjw;
    public uint32 Brp;
}

partial struct CANBitTimingConst {
    public array<uint8> Name = new(16);
    public uint32 Tseg1_min;
    public uint32 Tseg1_max;
    public uint32 Tseg2_min;
    public uint32 Tseg2_max;
    public uint32 Sjw_max;
    public uint32 Brp_min;
    public uint32 Brp_max;
    public uint32 Brp_inc;
}

partial struct CANClock {
    public uint32 Freq;
}

partial struct CANBusErrorCounters {
    public uint16 Txerr;
    public uint16 Rxerr;
}

partial struct CANCtrlMode {
    public uint32 Mask;
    public uint32 Flags;
}

partial struct CANDeviceStats {
    public uint32 Bus_error;
    public uint32 Error_warning;
    public uint32 Error_passive;
    public uint32 Bus_off;
    public uint32 Arbitration_lost;
    public uint32 Restarts;
}

public static UntypedInt CAN_STATE_ERROR_ACTIVE => 0x0;
public static UntypedInt CAN_STATE_ERROR_WARNING => 0x1;
public static UntypedInt CAN_STATE_ERROR_PASSIVE => 0x2;
public static UntypedInt CAN_STATE_BUS_OFF => 0x3;
public static UntypedInt CAN_STATE_STOPPED => 0x4;
public static UntypedInt CAN_STATE_SLEEPING => 0x5;
public static UntypedInt CAN_STATE_MAX => 0x6;

public static UntypedInt IFLA_CAN_UNSPEC => 0x0;
public static UntypedInt IFLA_CAN_BITTIMING => 0x1;
public static UntypedInt IFLA_CAN_BITTIMING_CONST => 0x2;
public static UntypedInt IFLA_CAN_CLOCK => 0x3;
public static UntypedInt IFLA_CAN_STATE => 0x4;
public static UntypedInt IFLA_CAN_CTRLMODE => 0x5;
public static UntypedInt IFLA_CAN_RESTART_MS => 0x6;
public static UntypedInt IFLA_CAN_RESTART => 0x7;
public static UntypedInt IFLA_CAN_BERR_COUNTER => 0x8;
public static UntypedInt IFLA_CAN_DATA_BITTIMING => 0x9;
public static UntypedInt IFLA_CAN_DATA_BITTIMING_CONST => 0xa;
public static UntypedInt IFLA_CAN_TERMINATION => 0xb;
public static UntypedInt IFLA_CAN_TERMINATION_CONST => 0xc;
public static UntypedInt IFLA_CAN_BITRATE_CONST => 0xd;
public static UntypedInt IFLA_CAN_DATA_BITRATE_CONST => 0xe;
public static UntypedInt IFLA_CAN_BITRATE_MAX => 0xf;

partial struct KCMAttach {
    public int32 Fd;
    public int32 Bpf_fd;
}

partial struct KCMUnattach {
    public int32 Fd;
}

partial struct KCMClone {
    public int32 Fd;
}

public static UntypedInt NL80211_AC_BE => 0x2;
public static UntypedInt NL80211_AC_BK => 0x3;
public static UntypedInt NL80211_ACL_POLICY_ACCEPT_UNLESS_LISTED => 0x0;
public static UntypedInt NL80211_ACL_POLICY_DENY_UNLESS_LISTED => 0x1;
public static UntypedInt NL80211_AC_VI => 0x1;
public static UntypedInt NL80211_AC_VO => 0x0;
public static UntypedInt NL80211_AP_SETTINGS_EXTERNAL_AUTH_SUPPORT => 0x1;
public static UntypedInt NL80211_AP_SETTINGS_SA_QUERY_OFFLOAD_SUPPORT => 0x2;
public static UntypedInt NL80211_AP_SME_SA_QUERY_OFFLOAD => 0x1;
public static UntypedInt NL80211_ATTR_4ADDR => 0x53;
public static UntypedInt NL80211_ATTR_ACK => 0x5c;
public static UntypedInt NL80211_ATTR_ACK_SIGNAL => 0x107;
public static UntypedInt NL80211_ATTR_ACL_POLICY => 0xa5;
public static UntypedInt NL80211_ATTR_ADMITTED_TIME => 0xd4;
public static UntypedInt NL80211_ATTR_AIRTIME_WEIGHT => 0x112;
public static UntypedInt NL80211_ATTR_AKM_SUITES => 0x4c;
public static UntypedInt NL80211_ATTR_AP_ISOLATE => 0x60;
public static UntypedInt NL80211_ATTR_AP_SETTINGS_FLAGS => 0x135;
public static UntypedInt NL80211_ATTR_ASSOC_SPP_AMSDU => 0x14a;
public static UntypedInt NL80211_ATTR_AUTH_DATA => 0x9c;
public static UntypedInt NL80211_ATTR_AUTH_TYPE => 0x35;
public static UntypedInt NL80211_ATTR_BANDS => 0xef;
public static UntypedInt NL80211_ATTR_BEACON_HEAD => 0xe;
public static UntypedInt NL80211_ATTR_BEACON_INTERVAL => 0xc;
public static UntypedInt NL80211_ATTR_BEACON_TAIL => 0xf;
public static UntypedInt NL80211_ATTR_BG_SCAN_PERIOD => 0x98;
public static UntypedInt NL80211_ATTR_BSS_BASIC_RATES => 0x24;
public static UntypedInt NL80211_ATTR_BSS => 0x2f;
public static UntypedInt NL80211_ATTR_BSS_CTS_PROT => 0x1c;
public static UntypedInt NL80211_ATTR_BSS_DUMP_INCLUDE_USE_DATA => 0x147;
public static UntypedInt NL80211_ATTR_BSS_HT_OPMODE => 0x6d;
public static UntypedInt NL80211_ATTR_BSSID => 0xf5;
public static UntypedInt NL80211_ATTR_BSS_SELECT => 0xe3;
public static UntypedInt NL80211_ATTR_BSS_SHORT_PREAMBLE => 0x1d;
public static UntypedInt NL80211_ATTR_BSS_SHORT_SLOT_TIME => 0x1e;
public static UntypedInt NL80211_ATTR_CENTER_FREQ1 => 0xa0;
public static UntypedInt NL80211_ATTR_CENTER_FREQ1_OFFSET => 0x123;
public static UntypedInt NL80211_ATTR_CENTER_FREQ2 => 0xa1;
public static UntypedInt NL80211_ATTR_CHANNEL_WIDTH => 0x9f;
public static UntypedInt NL80211_ATTR_CH_SWITCH_BLOCK_TX => 0xb8;
public static UntypedInt NL80211_ATTR_CH_SWITCH_COUNT => 0xb7;
public static UntypedInt NL80211_ATTR_CIPHER_SUITE_GROUP => 0x4a;
public static UntypedInt NL80211_ATTR_CIPHER_SUITES => 0x39;
public static UntypedInt NL80211_ATTR_CIPHER_SUITES_PAIRWISE => 0x49;
public static UntypedInt NL80211_ATTR_CNTDWN_OFFS_BEACON => 0xba;
public static UntypedInt NL80211_ATTR_CNTDWN_OFFS_PRESP => 0xbb;
public static UntypedInt NL80211_ATTR_COALESCE_RULE => 0xb6;
public static UntypedInt NL80211_ATTR_COALESCE_RULE_CONDITION => 0x2;
public static UntypedInt NL80211_ATTR_COALESCE_RULE_DELAY => 0x1;
public static UntypedInt NL80211_ATTR_COALESCE_RULE_MAX => 0x3;
public static UntypedInt NL80211_ATTR_COALESCE_RULE_PKT_PATTERN => 0x3;
public static UntypedInt NL80211_ATTR_COLOR_CHANGE_COLOR => 0x130;
public static UntypedInt NL80211_ATTR_COLOR_CHANGE_COUNT => 0x12f;
public static UntypedInt NL80211_ATTR_COLOR_CHANGE_ELEMS => 0x131;
public static UntypedInt NL80211_ATTR_CONN_FAILED_REASON => 0x9b;
public static UntypedInt NL80211_ATTR_CONTROL_PORT => 0x44;
public static UntypedInt NL80211_ATTR_CONTROL_PORT_ETHERTYPE => 0x66;
public static UntypedInt NL80211_ATTR_CONTROL_PORT_NO_ENCRYPT => 0x67;
public static UntypedInt NL80211_ATTR_CONTROL_PORT_NO_PREAUTH => 0x11e;
public static UntypedInt NL80211_ATTR_CONTROL_PORT_OVER_NL80211 => 0x108;
public static UntypedInt NL80211_ATTR_COOKIE => 0x58;
public static UntypedInt NL80211_ATTR_CQM_BEACON_LOSS_EVENT => 0x8;
public static UntypedInt NL80211_ATTR_CQM => 0x5e;
public static UntypedInt NL80211_ATTR_CQM_MAX => 0x9;
public static UntypedInt NL80211_ATTR_CQM_PKT_LOSS_EVENT => 0x4;
public static UntypedInt NL80211_ATTR_CQM_RSSI_HYST => 0x2;
public static UntypedInt NL80211_ATTR_CQM_RSSI_LEVEL => 0x9;
public static UntypedInt NL80211_ATTR_CQM_RSSI_THOLD => 0x1;
public static UntypedInt NL80211_ATTR_CQM_RSSI_THRESHOLD_EVENT => 0x3;
public static UntypedInt NL80211_ATTR_CQM_TXE_INTVL => 0x7;
public static UntypedInt NL80211_ATTR_CQM_TXE_PKTS => 0x6;
public static UntypedInt NL80211_ATTR_CQM_TXE_RATE => 0x5;
public static UntypedInt NL80211_ATTR_CRIT_PROT_ID => 0xb3;
public static UntypedInt NL80211_ATTR_CSA_C_OFF_BEACON => 0xba;
public static UntypedInt NL80211_ATTR_CSA_C_OFF_PRESP => 0xbb;
public static UntypedInt NL80211_ATTR_CSA_C_OFFSETS_TX => 0xcd;
public static UntypedInt NL80211_ATTR_CSA_IES => 0xb9;
public static UntypedInt NL80211_ATTR_DEVICE_AP_SME => 0x8d;
public static UntypedInt NL80211_ATTR_DFS_CAC_TIME => 0x7;
public static UntypedInt NL80211_ATTR_DFS_REGION => 0x92;
public static UntypedInt NL80211_ATTR_DISABLE_EHT => 0x137;
public static UntypedInt NL80211_ATTR_DISABLE_HE => 0x12d;
public static UntypedInt NL80211_ATTR_DISABLE_HT => 0x93;
public static UntypedInt NL80211_ATTR_DISABLE_VHT => 0xaf;
public static UntypedInt NL80211_ATTR_DISCONNECTED_BY_AP => 0x47;
public static UntypedInt NL80211_ATTR_DONT_WAIT_FOR_ACK => 0x8e;
public static UntypedInt NL80211_ATTR_DTIM_PERIOD => 0xd;
public static UntypedInt NL80211_ATTR_DURATION => 0x57;
public static UntypedInt NL80211_ATTR_EHT_CAPABILITY => 0x136;
public static UntypedInt NL80211_ATTR_EMA_RNR_ELEMS => 0x145;
public static UntypedInt NL80211_ATTR_EML_CAPABILITY => 0x13d;
public static UntypedInt NL80211_ATTR_EXT_CAPA => 0xa9;
public static UntypedInt NL80211_ATTR_EXT_CAPA_MASK => 0xaa;
public static UntypedInt NL80211_ATTR_EXTERNAL_AUTH_ACTION => 0x104;
public static UntypedInt NL80211_ATTR_EXTERNAL_AUTH_SUPPORT => 0x105;
public static UntypedInt NL80211_ATTR_EXT_FEATURES => 0xd9;
public static UntypedInt NL80211_ATTR_FEATURE_FLAGS => 0x8f;
public static UntypedInt NL80211_ATTR_FILS_CACHE_ID => 0xfd;
public static UntypedInt NL80211_ATTR_FILS_DISCOVERY => 0x126;
public static UntypedInt NL80211_ATTR_FILS_ERP_NEXT_SEQ_NUM => 0xfb;
public static UntypedInt NL80211_ATTR_FILS_ERP_REALM => 0xfa;
public static UntypedInt NL80211_ATTR_FILS_ERP_RRK => 0xfc;
public static UntypedInt NL80211_ATTR_FILS_ERP_USERNAME => 0xf9;
public static UntypedInt NL80211_ATTR_FILS_KEK => 0xf2;
public static UntypedInt NL80211_ATTR_FILS_NONCES => 0xf3;
public static UntypedInt NL80211_ATTR_FRAME => 0x33;
public static UntypedInt NL80211_ATTR_FRAME_MATCH => 0x5b;
public static UntypedInt NL80211_ATTR_FRAME_TYPE => 0x65;
public static UntypedInt NL80211_ATTR_FREQ_AFTER => 0x3b;
public static UntypedInt NL80211_ATTR_FREQ_BEFORE => 0x3a;
public static UntypedInt NL80211_ATTR_FREQ_FIXED => 0x3c;
public static UntypedInt NL80211_ATTR_FREQ_RANGE_END => 0x3;
public static UntypedInt NL80211_ATTR_FREQ_RANGE_MAX_BW => 0x4;
public static UntypedInt NL80211_ATTR_FREQ_RANGE_START => 0x2;
public static UntypedInt NL80211_ATTR_FTM_RESPONDER => 0x10e;
public static UntypedInt NL80211_ATTR_FTM_RESPONDER_STATS => 0x10f;
public static UntypedInt NL80211_ATTR_GENERATION => 0x2e;
public static UntypedInt NL80211_ATTR_HANDLE_DFS => 0xbf;
public static UntypedInt NL80211_ATTR_HE_6GHZ_CAPABILITY => 0x125;
public static UntypedInt NL80211_ATTR_HE_BSS_COLOR => 0x11b;
public static UntypedInt NL80211_ATTR_HE_CAPABILITY => 0x10d;
public static UntypedInt NL80211_ATTR_HE_OBSS_PD => 0x117;
public static UntypedInt NL80211_ATTR_HIDDEN_SSID => 0x7e;
public static UntypedInt NL80211_ATTR_HT_CAPABILITY => 0x1f;
public static UntypedInt NL80211_ATTR_HT_CAPABILITY_MASK => 0x94;
public static UntypedInt NL80211_ATTR_HW_TIMESTAMP_ENABLED => 0x144;
public static UntypedInt NL80211_ATTR_IE_ASSOC_RESP => 0x80;
public static UntypedInt NL80211_ATTR_IE => 0x2a;
public static UntypedInt NL80211_ATTR_IE_PROBE_RESP => 0x7f;
public static UntypedInt NL80211_ATTR_IE_RIC => 0xb2;
public static UntypedInt NL80211_ATTR_IFACE_SOCKET_OWNER => 0xcc;
public static UntypedInt NL80211_ATTR_IFINDEX => 0x3;
public static UntypedInt NL80211_ATTR_IFNAME => 0x4;
public static UntypedInt NL80211_ATTR_IFTYPE_AKM_SUITES => 0x11c;
public static UntypedInt NL80211_ATTR_IFTYPE => 0x5;
public static UntypedInt NL80211_ATTR_IFTYPE_EXT_CAPA => 0xe6;
public static UntypedInt NL80211_ATTR_INACTIVITY_TIMEOUT => 0x96;
public static UntypedInt NL80211_ATTR_INTERFACE_COMBINATIONS => 0x78;
public static UntypedInt NL80211_ATTR_KEY_CIPHER => 0x9;
public static UntypedInt NL80211_ATTR_KEY => 0x50;
public static UntypedInt NL80211_ATTR_KEY_DATA => 0x7;
public static UntypedInt NL80211_ATTR_KEY_DEFAULT => 0xb;
public static UntypedInt NL80211_ATTR_KEY_DEFAULT_MGMT => 0x28;
public static UntypedInt NL80211_ATTR_KEY_DEFAULT_TYPES => 0x6e;
public static UntypedInt NL80211_ATTR_KEY_IDX => 0x8;
public static UntypedInt NL80211_ATTR_KEYS => 0x51;
public static UntypedInt NL80211_ATTR_KEY_SEQ => 0xa;
public static UntypedInt NL80211_ATTR_KEY_TYPE => 0x37;
public static UntypedInt NL80211_ATTR_LOCAL_MESH_POWER_MODE => 0xa4;
public static UntypedInt NL80211_ATTR_LOCAL_STATE_CHANGE => 0x5f;
public static UntypedInt NL80211_ATTR_MAC_ACL_MAX => 0xa7;
public static UntypedInt NL80211_ATTR_MAC_ADDRS => 0xa6;
public static UntypedInt NL80211_ATTR_MAC => 0x6;
public static UntypedInt NL80211_ATTR_MAC_HINT => 0xc8;
public static UntypedInt NL80211_ATTR_MAC_MASK => 0xd7;
public static UntypedInt NL80211_ATTR_MAX_AP_ASSOC_STA => 0xca;
public static UntypedInt NL80211_ATTR_MAX => 0x16e;
public static UntypedInt NL80211_ATTR_MAX_CRIT_PROT_DURATION => 0xb4;
public static UntypedInt NL80211_ATTR_MAX_CSA_COUNTERS => 0xce;
public static UntypedInt NL80211_ATTR_MAX_HW_TIMESTAMP_PEERS => 0x143;
public static UntypedInt NL80211_ATTR_MAX_MATCH_SETS => 0x85;
public static UntypedInt NL80211_ATTR_MAX_NUM_AKM_SUITES => 0x13c;
public static UntypedInt NL80211_ATTR_MAX_NUM_PMKIDS => 0x56;
public static UntypedInt NL80211_ATTR_MAX_NUM_SCAN_SSIDS => 0x2b;
public static UntypedInt NL80211_ATTR_MAX_NUM_SCHED_SCAN_PLANS => 0xde;
public static UntypedInt NL80211_ATTR_MAX_NUM_SCHED_SCAN_SSIDS => 0x7b;
public static UntypedInt NL80211_ATTR_MAX_REMAIN_ON_CHANNEL_DURATION => 0x6f;
public static UntypedInt NL80211_ATTR_MAX_SCAN_IE_LEN => 0x38;
public static UntypedInt NL80211_ATTR_MAX_SCAN_PLAN_INTERVAL => 0xdf;
public static UntypedInt NL80211_ATTR_MAX_SCAN_PLAN_ITERATIONS => 0xe0;
public static UntypedInt NL80211_ATTR_MAX_SCHED_SCAN_IE_LEN => 0x7c;
public static UntypedInt NL80211_ATTR_MBSSID_CONFIG => 0x132;
public static UntypedInt NL80211_ATTR_MBSSID_ELEMS => 0x133;
public static UntypedInt NL80211_ATTR_MCAST_RATE => 0x6b;
public static UntypedInt NL80211_ATTR_MDID => 0xb1;
public static UntypedInt NL80211_ATTR_MEASUREMENT_DURATION => 0xeb;
public static UntypedInt NL80211_ATTR_MEASUREMENT_DURATION_MANDATORY => 0xec;
public static UntypedInt NL80211_ATTR_MESH_CONFIG => 0x23;
public static UntypedInt NL80211_ATTR_MESH_ID => 0x18;
public static UntypedInt NL80211_ATTR_MESH_PEER_AID => 0xed;
public static UntypedInt NL80211_ATTR_MESH_SETUP => 0x70;
public static UntypedInt NL80211_ATTR_MGMT_SUBTYPE => 0x29;
public static UntypedInt NL80211_ATTR_MLD_ADDR => 0x13a;
public static UntypedInt NL80211_ATTR_MLD_CAPA_AND_OPS => 0x13e;
public static UntypedInt NL80211_ATTR_MLO_LINK_DISABLED => 0x146;
public static UntypedInt NL80211_ATTR_MLO_LINK_ID => 0x139;
public static UntypedInt NL80211_ATTR_MLO_LINKS => 0x138;
public static UntypedInt NL80211_ATTR_MLO_SUPPORT => 0x13b;
public static UntypedInt NL80211_ATTR_MLO_TTLM_DLINK => 0x148;
public static UntypedInt NL80211_ATTR_MLO_TTLM_ULINK => 0x149;
public static UntypedInt NL80211_ATTR_MNTR_FLAGS => 0x17;
public static UntypedInt NL80211_ATTR_MPATH_INFO => 0x1b;
public static UntypedInt NL80211_ATTR_MPATH_NEXT_HOP => 0x1a;
public static UntypedInt NL80211_ATTR_MULTICAST_TO_UNICAST_ENABLED => 0xf4;
public static UntypedInt NL80211_ATTR_MU_MIMO_FOLLOW_MAC_ADDR => 0xe8;
public static UntypedInt NL80211_ATTR_MU_MIMO_GROUP_DATA => 0xe7;
public static UntypedInt NL80211_ATTR_NAN_FUNC => 0xf0;
public static UntypedInt NL80211_ATTR_NAN_MASTER_PREF => 0xee;
public static UntypedInt NL80211_ATTR_NAN_MATCH => 0xf1;
public static UntypedInt NL80211_ATTR_NETNS_FD => 0xdb;
public static UntypedInt NL80211_ATTR_NOACK_MAP => 0x95;
public static UntypedInt NL80211_ATTR_NSS => 0x106;
public static UntypedInt NL80211_ATTR_OBSS_COLOR_BITMAP => 0x12e;
public static UntypedInt NL80211_ATTR_OFFCHANNEL_TX_OK => 0x6c;
public static UntypedInt NL80211_ATTR_OPER_CLASS => 0xd6;
public static UntypedInt NL80211_ATTR_OPMODE_NOTIF => 0xc2;
public static UntypedInt NL80211_ATTR_P2P_CTWINDOW => 0xa2;
public static UntypedInt NL80211_ATTR_P2P_OPPPS => 0xa3;
public static UntypedInt NL80211_ATTR_PAD => 0xe5;
public static UntypedInt NL80211_ATTR_PBSS => 0xe2;
public static UntypedInt NL80211_ATTR_PEER_AID => 0xb5;
public static UntypedInt NL80211_ATTR_PEER_MEASUREMENTS => 0x111;
public static UntypedInt NL80211_ATTR_PID => 0x52;
public static UntypedInt NL80211_ATTR_PMK => 0xfe;
public static UntypedInt NL80211_ATTR_PMKID => 0x55;
public static UntypedInt NL80211_ATTR_PMK_LIFETIME => 0x11f;
public static UntypedInt NL80211_ATTR_PMKR0_NAME => 0x102;
public static UntypedInt NL80211_ATTR_PMK_REAUTH_THRESHOLD => 0x120;
public static UntypedInt NL80211_ATTR_PMKSA_CANDIDATE => 0x86;
public static UntypedInt NL80211_ATTR_PORT_AUTHORIZED => 0x103;
public static UntypedInt NL80211_ATTR_POWER_RULE_MAX_ANT_GAIN => 0x5;
public static UntypedInt NL80211_ATTR_POWER_RULE_MAX_EIRP => 0x6;
public static UntypedInt NL80211_ATTR_POWER_RULE_PSD => 0x8;
public static UntypedInt NL80211_ATTR_PREV_BSSID => 0x4f;
public static UntypedInt NL80211_ATTR_PRIVACY => 0x46;
public static UntypedInt NL80211_ATTR_PROBE_RESP => 0x91;
public static UntypedInt NL80211_ATTR_PROBE_RESP_OFFLOAD => 0x90;
public static UntypedInt NL80211_ATTR_PROTOCOL_FEATURES => 0xad;
public static UntypedInt NL80211_ATTR_PS_STATE => 0x5d;
public static UntypedInt NL80211_ATTR_PUNCT_BITMAP => 0x142;
public static UntypedInt NL80211_ATTR_QOS_MAP => 0xc7;
public static UntypedInt NL80211_ATTR_RADAR_BACKGROUND => 0x134;
public static UntypedInt NL80211_ATTR_RADAR_EVENT => 0xa8;
public static UntypedInt NL80211_ATTR_REASON_CODE => 0x36;
public static UntypedInt NL80211_ATTR_RECEIVE_MULTICAST => 0x121;
public static UntypedInt NL80211_ATTR_RECONNECT_REQUESTED => 0x12b;
public static UntypedInt NL80211_ATTR_REG_ALPHA2 => 0x21;
public static UntypedInt NL80211_ATTR_REG_INDOOR => 0xdd;
public static UntypedInt NL80211_ATTR_REG_INITIATOR => 0x30;
public static UntypedInt NL80211_ATTR_REG_RULE_FLAGS => 0x1;
public static UntypedInt NL80211_ATTR_REG_RULES => 0x22;
public static UntypedInt NL80211_ATTR_REG_TYPE => 0x31;
public static UntypedInt NL80211_ATTR_REKEY_DATA => 0x7a;
public static UntypedInt NL80211_ATTR_REQ_IE => 0x4d;
public static UntypedInt NL80211_ATTR_RESP_IE => 0x4e;
public static UntypedInt NL80211_ATTR_ROAM_SUPPORT => 0x83;
public static UntypedInt NL80211_ATTR_RX_FRAME_TYPES => 0x64;
public static UntypedInt NL80211_ATTR_RX_HW_TIMESTAMP => 0x140;
public static UntypedInt NL80211_ATTR_RXMGMT_FLAGS => 0xbc;
public static UntypedInt NL80211_ATTR_RX_SIGNAL_DBM => 0x97;
public static UntypedInt NL80211_ATTR_S1G_CAPABILITY => 0x128;
public static UntypedInt NL80211_ATTR_S1G_CAPABILITY_MASK => 0x129;
public static UntypedInt NL80211_ATTR_SAE_DATA => 0x9c;
public static UntypedInt NL80211_ATTR_SAE_PASSWORD => 0x115;
public static UntypedInt NL80211_ATTR_SAE_PWE => 0x12a;
public static UntypedInt NL80211_ATTR_SAR_SPEC => 0x12c;
public static UntypedInt NL80211_ATTR_SCAN_FLAGS => 0x9e;
public static UntypedInt NL80211_ATTR_SCAN_FREQ_KHZ => 0x124;
public static UntypedInt NL80211_ATTR_SCAN_FREQUENCIES => 0x2c;
public static UntypedInt NL80211_ATTR_SCAN_GENERATION => 0x2e;
public static UntypedInt NL80211_ATTR_SCAN_SSIDS => 0x2d;
public static UntypedInt NL80211_ATTR_SCAN_START_TIME_TSF_BSSID => 0xea;
public static UntypedInt NL80211_ATTR_SCAN_START_TIME_TSF => 0xe9;
public static UntypedInt NL80211_ATTR_SCAN_SUPP_RATES => 0x7d;
public static UntypedInt NL80211_ATTR_SCHED_SCAN_DELAY => 0xdc;
public static UntypedInt NL80211_ATTR_SCHED_SCAN_INTERVAL => 0x77;
public static UntypedInt NL80211_ATTR_SCHED_SCAN_MATCH => 0x84;
public static UntypedInt NL80211_ATTR_SCHED_SCAN_MATCH_SSID => 0x1;
public static UntypedInt NL80211_ATTR_SCHED_SCAN_MAX_REQS => 0x100;
public static UntypedInt NL80211_ATTR_SCHED_SCAN_MULTI => 0xff;
public static UntypedInt NL80211_ATTR_SCHED_SCAN_PLANS => 0xe1;
public static UntypedInt NL80211_ATTR_SCHED_SCAN_RELATIVE_RSSI => 0xf6;
public static UntypedInt NL80211_ATTR_SCHED_SCAN_RSSI_ADJUST => 0xf7;
public static UntypedInt NL80211_ATTR_SMPS_MODE => 0xd5;
public static UntypedInt NL80211_ATTR_SOCKET_OWNER => 0xcc;
public static UntypedInt NL80211_ATTR_SOFTWARE_IFTYPES => 0x79;
public static UntypedInt NL80211_ATTR_SPLIT_WIPHY_DUMP => 0xae;
public static UntypedInt NL80211_ATTR_SSID => 0x34;
public static UntypedInt NL80211_ATTR_STA_AID => 0x10;
public static UntypedInt NL80211_ATTR_STA_CAPABILITY => 0xab;
public static UntypedInt NL80211_ATTR_STA_EXT_CAPABILITY => 0xac;
public static UntypedInt NL80211_ATTR_STA_FLAGS2 => 0x43;
public static UntypedInt NL80211_ATTR_STA_FLAGS => 0x11;
public static UntypedInt NL80211_ATTR_STA_INFO => 0x15;
public static UntypedInt NL80211_ATTR_STA_LISTEN_INTERVAL => 0x12;
public static UntypedInt NL80211_ATTR_STA_PLINK_ACTION => 0x19;
public static UntypedInt NL80211_ATTR_STA_PLINK_STATE => 0x74;
public static UntypedInt NL80211_ATTR_STA_SUPPORTED_CHANNELS => 0xbd;
public static UntypedInt NL80211_ATTR_STA_SUPPORTED_OPER_CLASSES => 0xbe;
public static UntypedInt NL80211_ATTR_STA_SUPPORTED_RATES => 0x13;
public static UntypedInt NL80211_ATTR_STA_SUPPORT_P2P_PS => 0xe4;
public static UntypedInt NL80211_ATTR_STATUS_CODE => 0x48;
public static UntypedInt NL80211_ATTR_STA_TX_POWER => 0x114;
public static UntypedInt NL80211_ATTR_STA_TX_POWER_SETTING => 0x113;
public static UntypedInt NL80211_ATTR_STA_VLAN => 0x14;
public static UntypedInt NL80211_ATTR_STA_WME => 0x81;
public static UntypedInt NL80211_ATTR_SUPPORT_10_MHZ => 0xc1;
public static UntypedInt NL80211_ATTR_SUPPORT_5_MHZ => 0xc0;
public static UntypedInt NL80211_ATTR_SUPPORT_AP_UAPSD => 0x82;
public static UntypedInt NL80211_ATTR_SUPPORTED_COMMANDS => 0x32;
public static UntypedInt NL80211_ATTR_SUPPORTED_IFTYPES => 0x20;
public static UntypedInt NL80211_ATTR_SUPPORT_IBSS_RSN => 0x68;
public static UntypedInt NL80211_ATTR_SUPPORT_MESH_AUTH => 0x73;
public static UntypedInt NL80211_ATTR_SURVEY_INFO => 0x54;
public static UntypedInt NL80211_ATTR_SURVEY_RADIO_STATS => 0xda;
public static UntypedInt NL80211_ATTR_TD_BITMAP => 0x141;
public static UntypedInt NL80211_ATTR_TDLS_ACTION => 0x88;
public static UntypedInt NL80211_ATTR_TDLS_DIALOG_TOKEN => 0x89;
public static UntypedInt NL80211_ATTR_TDLS_EXTERNAL_SETUP => 0x8c;
public static UntypedInt NL80211_ATTR_TDLS_INITIATOR => 0xcf;
public static UntypedInt NL80211_ATTR_TDLS_OPERATION => 0x8a;
public static UntypedInt NL80211_ATTR_TDLS_PEER_CAPABILITY => 0xcb;
public static UntypedInt NL80211_ATTR_TDLS_SUPPORT => 0x8b;
public static UntypedInt NL80211_ATTR_TESTDATA => 0x45;
public static UntypedInt NL80211_ATTR_TID_CONFIG => 0x11d;
public static UntypedInt NL80211_ATTR_TIMED_OUT => 0x41;
public static UntypedInt NL80211_ATTR_TIMEOUT => 0x110;
public static UntypedInt NL80211_ATTR_TIMEOUT_REASON => 0xf8;
public static UntypedInt NL80211_ATTR_TSID => 0xd2;
public static UntypedInt NL80211_ATTR_TWT_RESPONDER => 0x116;
public static UntypedInt NL80211_ATTR_TX_FRAME_TYPES => 0x63;
public static UntypedInt NL80211_ATTR_TX_HW_TIMESTAMP => 0x13f;
public static UntypedInt NL80211_ATTR_TX_NO_CCK_RATE => 0x87;
public static UntypedInt NL80211_ATTR_TXQ_LIMIT => 0x10a;
public static UntypedInt NL80211_ATTR_TXQ_MEMORY_LIMIT => 0x10b;
public static UntypedInt NL80211_ATTR_TXQ_QUANTUM => 0x10c;
public static UntypedInt NL80211_ATTR_TXQ_STATS => 0x109;
public static UntypedInt NL80211_ATTR_TX_RATES => 0x5a;
public static UntypedInt NL80211_ATTR_UNSOL_BCAST_PROBE_RESP => 0x127;
public static UntypedInt NL80211_ATTR_UNSPEC => 0x0;
public static UntypedInt NL80211_ATTR_USE_MFP => 0x42;
public static UntypedInt NL80211_ATTR_USER_PRIO => 0xd3;
public static UntypedInt NL80211_ATTR_USER_REG_HINT_TYPE => 0x9a;
public static UntypedInt NL80211_ATTR_USE_RRM => 0xd0;
public static UntypedInt NL80211_ATTR_VENDOR_DATA => 0xc5;
public static UntypedInt NL80211_ATTR_VENDOR_EVENTS => 0xc6;
public static UntypedInt NL80211_ATTR_VENDOR_ID => 0xc3;
public static UntypedInt NL80211_ATTR_VENDOR_SUBCMD => 0xc4;
public static UntypedInt NL80211_ATTR_VHT_CAPABILITY => 0x9d;
public static UntypedInt NL80211_ATTR_VHT_CAPABILITY_MASK => 0xb0;
public static UntypedInt NL80211_ATTR_VLAN_ID => 0x11a;
public static UntypedInt NL80211_ATTR_WANT_1X_4WAY_HS => 0x101;
public static UntypedInt NL80211_ATTR_WDEV => 0x99;
public static UntypedInt NL80211_ATTR_WIPHY_ANTENNA_AVAIL_RX => 0x72;
public static UntypedInt NL80211_ATTR_WIPHY_ANTENNA_AVAIL_TX => 0x71;
public static UntypedInt NL80211_ATTR_WIPHY_ANTENNA_RX => 0x6a;
public static UntypedInt NL80211_ATTR_WIPHY_ANTENNA_TX => 0x69;
public static UntypedInt NL80211_ATTR_WIPHY_BANDS => 0x16;
public static UntypedInt NL80211_ATTR_WIPHY_CHANNEL_TYPE => 0x27;
public static UntypedInt NL80211_ATTR_WIPHY => 0x1;
public static UntypedInt NL80211_ATTR_WIPHY_COVERAGE_CLASS => 0x59;
public static UntypedInt NL80211_ATTR_WIPHY_DYN_ACK => 0xd1;
public static UntypedInt NL80211_ATTR_WIPHY_EDMG_BW_CONFIG => 0x119;
public static UntypedInt NL80211_ATTR_WIPHY_EDMG_CHANNELS => 0x118;
public static UntypedInt NL80211_ATTR_WIPHY_FRAG_THRESHOLD => 0x3f;
public static UntypedInt NL80211_ATTR_WIPHY_FREQ => 0x26;
public static UntypedInt NL80211_ATTR_WIPHY_FREQ_HINT => 0xc9;
public static UntypedInt NL80211_ATTR_WIPHY_FREQ_OFFSET => 0x122;
public static UntypedInt NL80211_ATTR_WIPHY_INTERFACE_COMBINATIONS => 0x14c;
public static UntypedInt NL80211_ATTR_WIPHY_NAME => 0x2;
public static UntypedInt NL80211_ATTR_WIPHY_RADIOS => 0x14b;
public static UntypedInt NL80211_ATTR_WIPHY_RETRY_LONG => 0x3e;
public static UntypedInt NL80211_ATTR_WIPHY_RETRY_SHORT => 0x3d;
public static UntypedInt NL80211_ATTR_WIPHY_RTS_THRESHOLD => 0x40;
public static UntypedInt NL80211_ATTR_WIPHY_SELF_MANAGED_REG => 0xd8;
public static UntypedInt NL80211_ATTR_WIPHY_TX_POWER_LEVEL => 0x62;
public static UntypedInt NL80211_ATTR_WIPHY_TX_POWER_SETTING => 0x61;
public static UntypedInt NL80211_ATTR_WIPHY_TXQ_PARAMS => 0x25;
public static UntypedInt NL80211_ATTR_WOWLAN_TRIGGERS => 0x75;
public static UntypedInt NL80211_ATTR_WOWLAN_TRIGGERS_SUPPORTED => 0x76;
public static UntypedInt NL80211_ATTR_WPA_VERSIONS => 0x4b;
public static UntypedInt NL80211_AUTHTYPE_AUTOMATIC => 0xa;
public static UntypedInt NL80211_AUTHTYPE_FILS_PK => 0x7;
public static UntypedInt NL80211_AUTHTYPE_FILS_SK => 0x5;
public static UntypedInt NL80211_AUTHTYPE_FILS_SK_PFS => 0x6;
public static UntypedInt NL80211_AUTHTYPE_FT => 0x2;
public static UntypedInt NL80211_AUTHTYPE_MAX => 0x9;
public static UntypedInt NL80211_AUTHTYPE_NETWORK_EAP => 0x3;
public static UntypedInt NL80211_AUTHTYPE_OPEN_SYSTEM => 0x0;
public static UntypedInt NL80211_AUTHTYPE_SAE => 0x4;
public static UntypedInt NL80211_AUTHTYPE_SHARED_KEY => 0x1;
public static UntypedInt NL80211_BAND_2GHZ => 0x0;
public static UntypedInt NL80211_BAND_5GHZ => 0x1;
public static UntypedInt NL80211_BAND_60GHZ => 0x2;
public static UntypedInt NL80211_BAND_6GHZ => 0x3;
public static UntypedInt NL80211_BAND_ATTR_EDMG_BW_CONFIG => 0xb;
public static UntypedInt NL80211_BAND_ATTR_EDMG_CHANNELS => 0xa;
public static UntypedInt NL80211_BAND_ATTR_FREQS => 0x1;
public static UntypedInt NL80211_BAND_ATTR_HT_AMPDU_DENSITY => 0x6;
public static UntypedInt NL80211_BAND_ATTR_HT_AMPDU_FACTOR => 0x5;
public static UntypedInt NL80211_BAND_ATTR_HT_CAPA => 0x4;
public static UntypedInt NL80211_BAND_ATTR_HT_MCS_SET => 0x3;
public static UntypedInt NL80211_BAND_ATTR_IFTYPE_DATA => 0x9;
public static UntypedInt NL80211_BAND_ATTR_MAX => 0xd;
public static UntypedInt NL80211_BAND_ATTR_RATES => 0x2;
public static UntypedInt NL80211_BAND_ATTR_S1G_CAPA => 0xd;
public static UntypedInt NL80211_BAND_ATTR_S1G_MCS_NSS_SET => 0xc;
public static UntypedInt NL80211_BAND_ATTR_VHT_CAPA => 0x8;
public static UntypedInt NL80211_BAND_ATTR_VHT_MCS_SET => 0x7;
public static UntypedInt NL80211_BAND_IFTYPE_ATTR_EHT_CAP_MAC => 0x8;
public static UntypedInt NL80211_BAND_IFTYPE_ATTR_EHT_CAP_MCS_SET => 0xa;
public static UntypedInt NL80211_BAND_IFTYPE_ATTR_EHT_CAP_PHY => 0x9;
public static UntypedInt NL80211_BAND_IFTYPE_ATTR_EHT_CAP_PPE => 0xb;
public static UntypedInt NL80211_BAND_IFTYPE_ATTR_HE_6GHZ_CAPA => 0x6;
public static UntypedInt NL80211_BAND_IFTYPE_ATTR_HE_CAP_MAC => 0x2;
public static UntypedInt NL80211_BAND_IFTYPE_ATTR_HE_CAP_MCS_SET => 0x4;
public static UntypedInt NL80211_BAND_IFTYPE_ATTR_HE_CAP_PHY => 0x3;
public static UntypedInt NL80211_BAND_IFTYPE_ATTR_HE_CAP_PPE => 0x5;
public static UntypedInt NL80211_BAND_IFTYPE_ATTR_IFTYPES => 0x1;
public static UntypedInt NL80211_BAND_IFTYPE_ATTR_MAX => 0xd;
public static UntypedInt NL80211_BAND_IFTYPE_ATTR_VENDOR_ELEMS => 0x7;
public static UntypedInt NL80211_BAND_LC => 0x5;
public static UntypedInt NL80211_BAND_S1GHZ => 0x4;
public static UntypedInt NL80211_BITRATE_ATTR_2GHZ_SHORTPREAMBLE => 0x2;
public static UntypedInt NL80211_BITRATE_ATTR_MAX => 0x2;
public static UntypedInt NL80211_BITRATE_ATTR_RATE => 0x1;
public static UntypedInt NL80211_BSS_BEACON_IES => 0xb;
public static UntypedInt NL80211_BSS_BEACON_INTERVAL => 0x4;
public static UntypedInt NL80211_BSS_BEACON_TSF => 0xd;
public static UntypedInt NL80211_BSS_BSSID => 0x1;
public static UntypedInt NL80211_BSS_CANNOT_USE_6GHZ_PWR_MISMATCH => 0x2;
public static UntypedInt NL80211_BSS_CANNOT_USE_NSTR_NONPRIMARY => 0x1;
public static UntypedInt NL80211_BSS_CANNOT_USE_REASONS => 0x18;
public static UntypedInt NL80211_BSS_CANNOT_USE_UHB_PWR_MISMATCH => 0x2;
public static UntypedInt NL80211_BSS_CAPABILITY => 0x5;
public static UntypedInt NL80211_BSS_CHAIN_SIGNAL => 0x13;
public static UntypedInt NL80211_BSS_CHAN_WIDTH_10 => 0x1;
public static UntypedInt NL80211_BSS_CHAN_WIDTH_1 => 0x3;
public static UntypedInt NL80211_BSS_CHAN_WIDTH_20 => 0x0;
public static UntypedInt NL80211_BSS_CHAN_WIDTH_2 => 0x4;
public static UntypedInt NL80211_BSS_CHAN_WIDTH_5 => 0x2;
public static UntypedInt NL80211_BSS_CHAN_WIDTH => 0xc;
public static UntypedInt NL80211_BSS_FREQUENCY => 0x2;
public static UntypedInt NL80211_BSS_FREQUENCY_OFFSET => 0x14;
public static UntypedInt NL80211_BSS_INFORMATION_ELEMENTS => 0x6;
public static UntypedInt NL80211_BSS_LAST_SEEN_BOOTTIME => 0xf;
public static UntypedInt NL80211_BSS_MAX => 0x18;
public static UntypedInt NL80211_BSS_MLD_ADDR => 0x16;
public static UntypedInt NL80211_BSS_MLO_LINK_ID => 0x15;
public static UntypedInt NL80211_BSS_PAD => 0x10;
public static UntypedInt NL80211_BSS_PARENT_BSSID => 0x12;
public static UntypedInt NL80211_BSS_PARENT_TSF => 0x11;
public static UntypedInt NL80211_BSS_PRESP_DATA => 0xe;
public static UntypedInt NL80211_BSS_SEEN_MS_AGO => 0xa;
public static UntypedInt NL80211_BSS_SELECT_ATTR_BAND_PREF => 0x2;
public static UntypedInt NL80211_BSS_SELECT_ATTR_MAX => 0x3;
public static UntypedInt NL80211_BSS_SELECT_ATTR_RSSI_ADJUST => 0x3;
public static UntypedInt NL80211_BSS_SELECT_ATTR_RSSI => 0x1;
public static UntypedInt NL80211_BSS_SIGNAL_MBM => 0x7;
public static UntypedInt NL80211_BSS_SIGNAL_UNSPEC => 0x8;
public static UntypedInt NL80211_BSS_STATUS_ASSOCIATED => 0x1;
public static UntypedInt NL80211_BSS_STATUS_AUTHENTICATED => 0x0;
public static UntypedInt NL80211_BSS_STATUS => 0x9;
public static UntypedInt NL80211_BSS_STATUS_IBSS_JOINED => 0x2;
public static UntypedInt NL80211_BSS_TSF => 0x3;
public static UntypedInt NL80211_BSS_USE_FOR => 0x17;
public static UntypedInt NL80211_BSS_USE_FOR_MLD_LINK => 0x2;
public static UntypedInt NL80211_BSS_USE_FOR_NORMAL => 0x1;
public static UntypedInt NL80211_CHAN_HT20 => 0x1;
public static UntypedInt NL80211_CHAN_HT40MINUS => 0x2;
public static UntypedInt NL80211_CHAN_HT40PLUS => 0x3;
public static UntypedInt NL80211_CHAN_NO_HT => 0x0;
public static UntypedInt NL80211_CHAN_WIDTH_10 => 0x7;
public static UntypedInt NL80211_CHAN_WIDTH_160 => 0x5;
public static UntypedInt NL80211_CHAN_WIDTH_16 => 0xc;
public static UntypedInt NL80211_CHAN_WIDTH_1 => 0x8;
public static UntypedInt NL80211_CHAN_WIDTH_20 => 0x1;
public static UntypedInt NL80211_CHAN_WIDTH_20_NOHT => 0x0;
public static UntypedInt NL80211_CHAN_WIDTH_2 => 0x9;
public static UntypedInt NL80211_CHAN_WIDTH_320 => 0xd;
public static UntypedInt NL80211_CHAN_WIDTH_40 => 0x2;
public static UntypedInt NL80211_CHAN_WIDTH_4 => 0xa;
public static UntypedInt NL80211_CHAN_WIDTH_5 => 0x6;
public static UntypedInt NL80211_CHAN_WIDTH_80 => 0x3;
public static UntypedInt NL80211_CHAN_WIDTH_80P80 => 0x4;
public static UntypedInt NL80211_CHAN_WIDTH_8 => 0xb;
public static UntypedInt NL80211_CMD_ABORT_SCAN => 0x72;
public static UntypedInt NL80211_CMD_ACTION => 0x3b;
public static UntypedInt NL80211_CMD_ACTION_TX_STATUS => 0x3c;
public static UntypedInt NL80211_CMD_ADD_LINK => 0x94;
public static UntypedInt NL80211_CMD_ADD_LINK_STA => 0x96;
public static UntypedInt NL80211_CMD_ADD_NAN_FUNCTION => 0x75;
public static UntypedInt NL80211_CMD_ADD_TX_TS => 0x69;
public static UntypedInt NL80211_CMD_ASSOC_COMEBACK => 0x93;
public static UntypedInt NL80211_CMD_ASSOCIATE => 0x26;
public static UntypedInt NL80211_CMD_AUTHENTICATE => 0x25;
public static UntypedInt NL80211_CMD_CANCEL_REMAIN_ON_CHANNEL => 0x38;
public static UntypedInt NL80211_CMD_CHANGE_NAN_CONFIG => 0x77;
public static UntypedInt NL80211_CMD_CHANNEL_SWITCH => 0x66;
public static UntypedInt NL80211_CMD_CH_SWITCH_NOTIFY => 0x58;
public static UntypedInt NL80211_CMD_CH_SWITCH_STARTED_NOTIFY => 0x6e;
public static UntypedInt NL80211_CMD_COLOR_CHANGE_ABORTED => 0x90;
public static UntypedInt NL80211_CMD_COLOR_CHANGE_COMPLETED => 0x91;
public static UntypedInt NL80211_CMD_COLOR_CHANGE_REQUEST => 0x8e;
public static UntypedInt NL80211_CMD_COLOR_CHANGE_STARTED => 0x8f;
public static UntypedInt NL80211_CMD_CONNECT => 0x2e;
public static UntypedInt NL80211_CMD_CONN_FAILED => 0x5b;
public static UntypedInt NL80211_CMD_CONTROL_PORT_FRAME => 0x81;
public static UntypedInt NL80211_CMD_CONTROL_PORT_FRAME_TX_STATUS => 0x8b;
public static UntypedInt NL80211_CMD_CRIT_PROTOCOL_START => 0x62;
public static UntypedInt NL80211_CMD_CRIT_PROTOCOL_STOP => 0x63;
public static UntypedInt NL80211_CMD_DEAUTHENTICATE => 0x27;
public static UntypedInt NL80211_CMD_DEL_BEACON => 0x10;
public static UntypedInt NL80211_CMD_DEL_INTERFACE => 0x8;
public static UntypedInt NL80211_CMD_DEL_KEY => 0xc;
public static UntypedInt NL80211_CMD_DEL_MPATH => 0x18;
public static UntypedInt NL80211_CMD_DEL_NAN_FUNCTION => 0x76;
public static UntypedInt NL80211_CMD_DEL_PMK => 0x7c;
public static UntypedInt NL80211_CMD_DEL_PMKSA => 0x35;
public static UntypedInt NL80211_CMD_DEL_STATION => 0x14;
public static UntypedInt NL80211_CMD_DEL_TX_TS => 0x6a;
public static UntypedInt NL80211_CMD_DEL_WIPHY => 0x4;
public static UntypedInt NL80211_CMD_DISASSOCIATE => 0x28;
public static UntypedInt NL80211_CMD_DISCONNECT => 0x30;
public static UntypedInt NL80211_CMD_EXTERNAL_AUTH => 0x7f;
public static UntypedInt NL80211_CMD_FLUSH_PMKSA => 0x36;
public static UntypedInt NL80211_CMD_FRAME => 0x3b;
public static UntypedInt NL80211_CMD_FRAME_TX_STATUS => 0x3c;
public static UntypedInt NL80211_CMD_FRAME_WAIT_CANCEL => 0x43;
public static UntypedInt NL80211_CMD_FT_EVENT => 0x61;
public static UntypedInt NL80211_CMD_GET_BEACON => 0xd;
public static UntypedInt NL80211_CMD_GET_COALESCE => 0x64;
public static UntypedInt NL80211_CMD_GET_FTM_RESPONDER_STATS => 0x82;
public static UntypedInt NL80211_CMD_GET_INTERFACE => 0x5;
public static UntypedInt NL80211_CMD_GET_KEY => 0x9;
public static UntypedInt NL80211_CMD_GET_MESH_CONFIG => 0x1c;
public static UntypedInt NL80211_CMD_GET_MESH_PARAMS => 0x1c;
public static UntypedInt NL80211_CMD_GET_MPATH => 0x15;
public static UntypedInt NL80211_CMD_GET_MPP => 0x6b;
public static UntypedInt NL80211_CMD_GET_POWER_SAVE => 0x3e;
public static UntypedInt NL80211_CMD_GET_PROTOCOL_FEATURES => 0x5f;
public static UntypedInt NL80211_CMD_GET_REG => 0x1f;
public static UntypedInt NL80211_CMD_GET_SCAN => 0x20;
public static UntypedInt NL80211_CMD_GET_STATION => 0x11;
public static UntypedInt NL80211_CMD_GET_SURVEY => 0x32;
public static UntypedInt NL80211_CMD_GET_WIPHY => 0x1;
public static UntypedInt NL80211_CMD_GET_WOWLAN => 0x49;
public static UntypedInt NL80211_CMD_JOIN_IBSS => 0x2b;
public static UntypedInt NL80211_CMD_JOIN_MESH => 0x44;
public static UntypedInt NL80211_CMD_JOIN_OCB => 0x6c;
public static UntypedInt NL80211_CMD_LEAVE_IBSS => 0x2c;
public static UntypedInt NL80211_CMD_LEAVE_MESH => 0x45;
public static UntypedInt NL80211_CMD_LEAVE_OCB => 0x6d;
public static UntypedInt NL80211_CMD_LINKS_REMOVED => 0x9a;
public static UntypedInt NL80211_CMD_MAX => 0xa7;
public static UntypedInt NL80211_CMD_MICHAEL_MIC_FAILURE => 0x29;
public static UntypedInt NL80211_CMD_MODIFY_LINK_STA => 0x97;
public static UntypedInt NL80211_CMD_NAN_MATCH => 0x78;
public static UntypedInt NL80211_CMD_NEW_BEACON => 0xf;
public static UntypedInt NL80211_CMD_NEW_INTERFACE => 0x7;
public static UntypedInt NL80211_CMD_NEW_KEY => 0xb;
public static UntypedInt NL80211_CMD_NEW_MPATH => 0x17;
public static UntypedInt NL80211_CMD_NEW_PEER_CANDIDATE => 0x48;
public static UntypedInt NL80211_CMD_NEW_SCAN_RESULTS => 0x22;
public static UntypedInt NL80211_CMD_NEW_STATION => 0x13;
public static UntypedInt NL80211_CMD_NEW_SURVEY_RESULTS => 0x33;
public static UntypedInt NL80211_CMD_NEW_WIPHY => 0x3;
public static UntypedInt NL80211_CMD_NOTIFY_CQM => 0x40;
public static UntypedInt NL80211_CMD_NOTIFY_RADAR => 0x86;
public static UntypedInt NL80211_CMD_OBSS_COLOR_COLLISION => 0x8d;
public static UntypedInt NL80211_CMD_PEER_MEASUREMENT_COMPLETE => 0x85;
public static UntypedInt NL80211_CMD_PEER_MEASUREMENT_RESULT => 0x84;
public static UntypedInt NL80211_CMD_PEER_MEASUREMENT_START => 0x83;
public static UntypedInt NL80211_CMD_PMKSA_CANDIDATE => 0x50;
public static UntypedInt NL80211_CMD_PORT_AUTHORIZED => 0x7d;
public static UntypedInt NL80211_CMD_PROBE_CLIENT => 0x54;
public static UntypedInt NL80211_CMD_PROBE_MESH_LINK => 0x88;
public static UntypedInt NL80211_CMD_RADAR_DETECT => 0x5e;
public static UntypedInt NL80211_CMD_REG_BEACON_HINT => 0x2a;
public static UntypedInt NL80211_CMD_REG_CHANGE => 0x24;
public static UntypedInt NL80211_CMD_REGISTER_ACTION => 0x3a;
public static UntypedInt NL80211_CMD_REGISTER_BEACONS => 0x55;
public static UntypedInt NL80211_CMD_REGISTER_FRAME => 0x3a;
public static UntypedInt NL80211_CMD_RELOAD_REGDB => 0x7e;
public static UntypedInt NL80211_CMD_REMAIN_ON_CHANNEL => 0x37;
public static UntypedInt NL80211_CMD_REMOVE_LINK => 0x95;
public static UntypedInt NL80211_CMD_REMOVE_LINK_STA => 0x98;
public static UntypedInt NL80211_CMD_REQ_SET_REG => 0x1b;
public static UntypedInt NL80211_CMD_ROAM => 0x2f;
public static UntypedInt NL80211_CMD_SCAN_ABORTED => 0x23;
public static UntypedInt NL80211_CMD_SCHED_SCAN_RESULTS => 0x4d;
public static UntypedInt NL80211_CMD_SCHED_SCAN_STOPPED => 0x4e;
public static UntypedInt NL80211_CMD_SET_BEACON => 0xe;
public static UntypedInt NL80211_CMD_SET_BSS => 0x19;
public static UntypedInt NL80211_CMD_SET_CHANNEL => 0x41;
public static UntypedInt NL80211_CMD_SET_COALESCE => 0x65;
public static UntypedInt NL80211_CMD_SET_CQM => 0x3f;
public static UntypedInt NL80211_CMD_SET_FILS_AAD => 0x92;
public static UntypedInt NL80211_CMD_SET_HW_TIMESTAMP => 0x99;
public static UntypedInt NL80211_CMD_SET_INTERFACE => 0x6;
public static UntypedInt NL80211_CMD_SET_KEY => 0xa;
public static UntypedInt NL80211_CMD_SET_MAC_ACL => 0x5d;
public static UntypedInt NL80211_CMD_SET_MCAST_RATE => 0x5c;
public static UntypedInt NL80211_CMD_SET_MESH_CONFIG => 0x1d;
public static UntypedInt NL80211_CMD_SET_MESH_PARAMS => 0x1d;
public static UntypedInt NL80211_CMD_SET_MGMT_EXTRA_IE => 0x1e;
public static UntypedInt NL80211_CMD_SET_MPATH => 0x16;
public static UntypedInt NL80211_CMD_SET_MULTICAST_TO_UNICAST => 0x79;
public static UntypedInt NL80211_CMD_SET_NOACK_MAP => 0x57;
public static UntypedInt NL80211_CMD_SET_PMK => 0x7b;
public static UntypedInt NL80211_CMD_SET_PMKSA => 0x34;
public static UntypedInt NL80211_CMD_SET_POWER_SAVE => 0x3d;
public static UntypedInt NL80211_CMD_SET_QOS_MAP => 0x68;
public static UntypedInt NL80211_CMD_SET_REG => 0x1a;
public static UntypedInt NL80211_CMD_SET_REKEY_OFFLOAD => 0x4f;
public static UntypedInt NL80211_CMD_SET_SAR_SPECS => 0x8c;
public static UntypedInt NL80211_CMD_SET_STATION => 0x12;
public static UntypedInt NL80211_CMD_SET_TID_CONFIG => 0x89;
public static UntypedInt NL80211_CMD_SET_TID_TO_LINK_MAPPING => 0x9b;
public static UntypedInt NL80211_CMD_SET_TX_BITRATE_MASK => 0x39;
public static UntypedInt NL80211_CMD_SET_WDS_PEER => 0x42;
public static UntypedInt NL80211_CMD_SET_WIPHY => 0x2;
public static UntypedInt NL80211_CMD_SET_WIPHY_NETNS => 0x31;
public static UntypedInt NL80211_CMD_SET_WOWLAN => 0x4a;
public static UntypedInt NL80211_CMD_STA_OPMODE_CHANGED => 0x80;
public static UntypedInt NL80211_CMD_START_AP => 0xf;
public static UntypedInt NL80211_CMD_START_NAN => 0x73;
public static UntypedInt NL80211_CMD_START_P2P_DEVICE => 0x59;
public static UntypedInt NL80211_CMD_START_SCHED_SCAN => 0x4b;
public static UntypedInt NL80211_CMD_STOP_AP => 0x10;
public static UntypedInt NL80211_CMD_STOP_NAN => 0x74;
public static UntypedInt NL80211_CMD_STOP_P2P_DEVICE => 0x5a;
public static UntypedInt NL80211_CMD_STOP_SCHED_SCAN => 0x4c;
public static UntypedInt NL80211_CMD_TDLS_CANCEL_CHANNEL_SWITCH => 0x70;
public static UntypedInt NL80211_CMD_TDLS_CHANNEL_SWITCH => 0x6f;
public static UntypedInt NL80211_CMD_TDLS_MGMT => 0x52;
public static UntypedInt NL80211_CMD_TDLS_OPER => 0x51;
public static UntypedInt NL80211_CMD_TESTMODE => 0x2d;
public static UntypedInt NL80211_CMD_TRIGGER_SCAN => 0x21;
public static UntypedInt NL80211_CMD_UNEXPECTED_4ADDR_FRAME => 0x56;
public static UntypedInt NL80211_CMD_UNEXPECTED_FRAME => 0x53;
public static UntypedInt NL80211_CMD_UNPROT_BEACON => 0x8a;
public static UntypedInt NL80211_CMD_UNPROT_DEAUTHENTICATE => 0x46;
public static UntypedInt NL80211_CMD_UNPROT_DISASSOCIATE => 0x47;
public static UntypedInt NL80211_CMD_UNSPEC => 0x0;
public static UntypedInt NL80211_CMD_UPDATE_CONNECT_PARAMS => 0x7a;
public static UntypedInt NL80211_CMD_UPDATE_FT_IES => 0x60;
public static UntypedInt NL80211_CMD_UPDATE_OWE_INFO => 0x87;
public static UntypedInt NL80211_CMD_VENDOR => 0x67;
public static UntypedInt NL80211_CMD_WIPHY_REG_CHANGE => 0x71;
public static UntypedInt NL80211_COALESCE_CONDITION_MATCH => 0x0;
public static UntypedInt NL80211_COALESCE_CONDITION_NO_MATCH => 0x1;
public static UntypedInt NL80211_CONN_FAIL_BLOCKED_CLIENT => 0x1;
public static UntypedInt NL80211_CONN_FAIL_MAX_CLIENTS => 0x0;
public static UntypedInt NL80211_CQM_RSSI_BEACON_LOSS_EVENT => 0x2;
public static UntypedInt NL80211_CQM_RSSI_THRESHOLD_EVENT_HIGH => 0x1;
public static UntypedInt NL80211_CQM_RSSI_THRESHOLD_EVENT_LOW => 0x0;
public static UntypedInt NL80211_CQM_TXE_MAX_INTVL => 0x708;
public static UntypedInt NL80211_CRIT_PROTO_APIPA => 0x3;
public static UntypedInt NL80211_CRIT_PROTO_DHCP => 0x1;
public static UntypedInt NL80211_CRIT_PROTO_EAPOL => 0x2;
public static UntypedInt NL80211_CRIT_PROTO_MAX_DURATION => 0x1388;
public static UntypedInt NL80211_CRIT_PROTO_UNSPEC => 0x0;
public static UntypedInt NL80211_DFS_AVAILABLE => 0x2;
public static UntypedInt NL80211_DFS_ETSI => 0x2;
public static UntypedInt NL80211_DFS_FCC => 0x1;
public static UntypedInt NL80211_DFS_JP => 0x3;
public static UntypedInt NL80211_DFS_UNAVAILABLE => 0x1;
public static UntypedInt NL80211_DFS_UNSET => 0x0;
public static UntypedInt NL80211_DFS_USABLE => 0x0;
public static UntypedInt NL80211_EDMG_BW_CONFIG_MAX => 0xf;
public static UntypedInt NL80211_EDMG_BW_CONFIG_MIN => 0x4;
public static UntypedInt NL80211_EDMG_CHANNELS_MAX => 0x3c;
public static UntypedInt NL80211_EDMG_CHANNELS_MIN => 0x1;
public static UntypedInt NL80211_EHT_MAX_CAPABILITY_LEN => 0x33;
public static UntypedInt NL80211_EHT_MIN_CAPABILITY_LEN => 0xd;
public static UntypedInt NL80211_EXTERNAL_AUTH_ABORT => 0x1;
public static UntypedInt NL80211_EXTERNAL_AUTH_START => 0x0;
public static UntypedInt NL80211_EXT_FEATURE_4WAY_HANDSHAKE_AP_PSK => 0x32;
public static UntypedInt NL80211_EXT_FEATURE_4WAY_HANDSHAKE_STA_1X => 0x10;
public static UntypedInt NL80211_EXT_FEATURE_4WAY_HANDSHAKE_STA_PSK => 0xf;
public static UntypedInt NL80211_EXT_FEATURE_ACCEPT_BCAST_PROBE_RESP => 0x12;
public static UntypedInt NL80211_EXT_FEATURE_ACK_SIGNAL_SUPPORT => 0x1b;
public static UntypedInt NL80211_EXT_FEATURE_AIRTIME_FAIRNESS => 0x21;
public static UntypedInt NL80211_EXT_FEATURE_AP_PMKSA_CACHING => 0x22;
public static UntypedInt NL80211_EXT_FEATURE_AQL => 0x28;
public static UntypedInt NL80211_EXT_FEATURE_AUTH_AND_DEAUTH_RANDOM_TA => 0x40;
public static UntypedInt NL80211_EXT_FEATURE_BEACON_PROTECTION_CLIENT => 0x2e;
public static UntypedInt NL80211_EXT_FEATURE_BEACON_PROTECTION => 0x29;
public static UntypedInt NL80211_EXT_FEATURE_BEACON_RATE_HE => 0x36;
public static UntypedInt NL80211_EXT_FEATURE_BEACON_RATE_HT => 0x7;
public static UntypedInt NL80211_EXT_FEATURE_BEACON_RATE_LEGACY => 0x6;
public static UntypedInt NL80211_EXT_FEATURE_BEACON_RATE_VHT => 0x8;
public static UntypedInt NL80211_EXT_FEATURE_BSS_COLOR => 0x3a;
public static UntypedInt NL80211_EXT_FEATURE_BSS_PARENT_TSF => 0x4;
public static UntypedInt NL80211_EXT_FEATURE_CAN_REPLACE_PTK0 => 0x1f;
public static UntypedInt NL80211_EXT_FEATURE_CONTROL_PORT_NO_PREAUTH => 0x2a;
public static UntypedInt NL80211_EXT_FEATURE_CONTROL_PORT_OVER_NL80211 => 0x1a;
public static UntypedInt NL80211_EXT_FEATURE_CONTROL_PORT_OVER_NL80211_TX_STATUS => 0x30;
public static UntypedInt NL80211_EXT_FEATURE_CQM_RSSI_LIST => 0xd;
public static UntypedInt NL80211_EXT_FEATURE_DATA_ACK_SIGNAL_SUPPORT => 0x1b;
public static UntypedInt NL80211_EXT_FEATURE_DEL_IBSS_STA => 0x2c;
public static UntypedInt NL80211_EXT_FEATURE_DFS_CONCURRENT => 0x43;
public static UntypedInt NL80211_EXT_FEATURE_DFS_OFFLOAD => 0x19;
public static UntypedInt NL80211_EXT_FEATURE_ENABLE_FTM_RESPONDER => 0x20;
public static UntypedInt NL80211_EXT_FEATURE_EXT_KEY_ID => 0x24;
public static UntypedInt NL80211_EXT_FEATURE_FILS_CRYPTO_OFFLOAD => 0x3b;
public static UntypedInt NL80211_EXT_FEATURE_FILS_DISCOVERY => 0x34;
public static UntypedInt NL80211_EXT_FEATURE_FILS_MAX_CHANNEL_TIME => 0x11;
public static UntypedInt NL80211_EXT_FEATURE_FILS_SK_OFFLOAD => 0xe;
public static UntypedInt NL80211_EXT_FEATURE_FILS_STA => 0x9;
public static UntypedInt NL80211_EXT_FEATURE_HIGH_ACCURACY_SCAN => 0x18;
public static UntypedInt NL80211_EXT_FEATURE_LOW_POWER_SCAN => 0x17;
public static UntypedInt NL80211_EXT_FEATURE_LOW_SPAN_SCAN => 0x16;
public static UntypedInt NL80211_EXT_FEATURE_MFP_OPTIONAL => 0x15;
public static UntypedInt NL80211_EXT_FEATURE_MGMT_TX_RANDOM_TA => 0xa;
public static UntypedInt NL80211_EXT_FEATURE_MGMT_TX_RANDOM_TA_CONNECTED => 0xb;
public static UntypedInt NL80211_EXT_FEATURE_MULTICAST_REGISTRATIONS => 0x2d;
public static UntypedInt NL80211_EXT_FEATURE_MU_MIMO_AIR_SNIFFER => 0x2;
public static UntypedInt NL80211_EXT_FEATURE_OCE_PROBE_REQ_DEFERRAL_SUPPRESSION => 0x14;
public static UntypedInt NL80211_EXT_FEATURE_OCE_PROBE_REQ_HIGH_TX_RATE => 0x13;
public static UntypedInt NL80211_EXT_FEATURE_OPERATING_CHANNEL_VALIDATION => 0x31;
public static UntypedInt NL80211_EXT_FEATURE_OWE_OFFLOAD_AP => 0x42;
public static UntypedInt NL80211_EXT_FEATURE_OWE_OFFLOAD => 0x41;
public static UntypedInt NL80211_EXT_FEATURE_POWERED_ADDR_CHANGE => 0x3d;
public static UntypedInt NL80211_EXT_FEATURE_PROTECTED_TWT => 0x2b;
public static UntypedInt NL80211_EXT_FEATURE_PROT_RANGE_NEGO_AND_MEASURE => 0x39;
public static UntypedInt NL80211_EXT_FEATURE_PUNCT => 0x3e;
public static UntypedInt NL80211_EXT_FEATURE_RADAR_BACKGROUND => 0x3c;
public static UntypedInt NL80211_EXT_FEATURE_RRM => 0x1;
public static UntypedInt NL80211_EXT_FEATURE_SAE_OFFLOAD_AP => 0x33;
public static UntypedInt NL80211_EXT_FEATURE_SAE_OFFLOAD => 0x26;
public static UntypedInt NL80211_EXT_FEATURE_SCAN_FREQ_KHZ => 0x2f;
public static UntypedInt NL80211_EXT_FEATURE_SCAN_MIN_PREQ_CONTENT => 0x1e;
public static UntypedInt NL80211_EXT_FEATURE_SCAN_RANDOM_SN => 0x1d;
public static UntypedInt NL80211_EXT_FEATURE_SCAN_START_TIME => 0x3;
public static UntypedInt NL80211_EXT_FEATURE_SCHED_SCAN_BAND_SPECIFIC_RSSI_THOLD => 0x23;
public static UntypedInt NL80211_EXT_FEATURE_SCHED_SCAN_RELATIVE_RSSI => 0xc;
public static UntypedInt NL80211_EXT_FEATURE_SECURE_LTF => 0x37;
public static UntypedInt NL80211_EXT_FEATURE_SECURE_NAN => 0x3f;
public static UntypedInt NL80211_EXT_FEATURE_SECURE_RTT => 0x38;
public static UntypedInt NL80211_EXT_FEATURE_SET_SCAN_DWELL => 0x5;
public static UntypedInt NL80211_EXT_FEATURE_SPP_AMSDU_SUPPORT => 0x44;
public static UntypedInt NL80211_EXT_FEATURE_STA_TX_PWR => 0x25;
public static UntypedInt NL80211_EXT_FEATURE_TXQS => 0x1c;
public static UntypedInt NL80211_EXT_FEATURE_UNSOL_BCAST_PROBE_RESP => 0x35;
public static UntypedInt NL80211_EXT_FEATURE_VHT_IBSS => 0x0;
public static UntypedInt NL80211_EXT_FEATURE_VLAN_OFFLOAD => 0x27;
public static UntypedInt NL80211_FEATURE_ACKTO_ESTIMATION => 0x800000;
public static UntypedInt NL80211_FEATURE_ACTIVE_MONITOR => 0x20000;
public static UntypedInt NL80211_FEATURE_ADVERTISE_CHAN_LIMITS => 0x4000;
public static UntypedInt NL80211_FEATURE_AP_MODE_CHAN_WIDTH_CHANGE => 0x40000;
public static UntypedInt NL80211_FEATURE_AP_SCAN => 0x100;
public static UntypedInt NL80211_FEATURE_CELL_BASE_REG_HINTS => 0x8;
public static UntypedInt NL80211_FEATURE_DS_PARAM_SET_IE_IN_PROBES => 0x80000;
public static UntypedInt NL80211_FEATURE_DYNAMIC_SMPS => 0x2000000;
public static UntypedInt NL80211_FEATURE_FULL_AP_CLIENT_STATE => 0x8000;
public static UntypedInt NL80211_FEATURE_HT_IBSS => 0x2;
public static UntypedInt NL80211_FEATURE_INACTIVITY_TIMER => 0x4;
public static UntypedInt NL80211_FEATURE_LOW_PRIORITY_SCAN => 0x40;
public static UntypedInt NL80211_FEATURE_MAC_ON_CREATE => 0x8000000;
public static UntypedInt NL80211_FEATURE_ND_RANDOM_MAC_ADDR => 0x80000000;
public static UntypedInt NL80211_FEATURE_NEED_OBSS_SCAN => 0x400;
public static UntypedInt NL80211_FEATURE_P2P_DEVICE_NEEDS_CHANNEL => 0x10;
public static UntypedInt NL80211_FEATURE_P2P_GO_CTWIN => 0x800;
public static UntypedInt NL80211_FEATURE_P2P_GO_OPPPS => 0x1000;
public static UntypedInt NL80211_FEATURE_QUIET => 0x200000;
public static UntypedInt NL80211_FEATURE_SAE => 0x20;
public static UntypedInt NL80211_FEATURE_SCAN_FLUSH => 0x80;
public static UntypedInt NL80211_FEATURE_SCAN_RANDOM_MAC_ADDR => 0x20000000;
public static UntypedInt NL80211_FEATURE_SCHED_SCAN_RANDOM_MAC_ADDR => 0x40000000;
public static UntypedInt NL80211_FEATURE_SK_TX_STATUS => 0x1;
public static UntypedInt NL80211_FEATURE_STATIC_SMPS => 0x1000000;
public static UntypedInt NL80211_FEATURE_SUPPORTS_WMM_ADMISSION => 0x4000000;
public static UntypedInt NL80211_FEATURE_TDLS_CHANNEL_SWITCH => 0x10000000;
public static UntypedInt NL80211_FEATURE_TX_POWER_INSERTION => 0x400000;
public static UntypedInt NL80211_FEATURE_USERSPACE_MPM => 0x10000;
public static UntypedInt NL80211_FEATURE_VIF_TXPOWER => 0x200;
public static UntypedInt NL80211_FEATURE_WFA_TPC_IE_IN_PROBES => 0x100000;
public static UntypedInt NL80211_FILS_DISCOVERY_ATTR_INT_MAX => 0x2;
public static UntypedInt NL80211_FILS_DISCOVERY_ATTR_INT_MIN => 0x1;
public static UntypedInt NL80211_FILS_DISCOVERY_ATTR_MAX => 0x3;
public static UntypedInt NL80211_FILS_DISCOVERY_ATTR_TMPL => 0x3;
public static UntypedInt NL80211_FILS_DISCOVERY_TMPL_MIN_LEN => 0x2a;
public static UntypedInt NL80211_FREQUENCY_ATTR_16MHZ => 0x19;
public static UntypedInt NL80211_FREQUENCY_ATTR_1MHZ => 0x15;
public static UntypedInt NL80211_FREQUENCY_ATTR_2MHZ => 0x16;
public static UntypedInt NL80211_FREQUENCY_ATTR_4MHZ => 0x17;
public static UntypedInt NL80211_FREQUENCY_ATTR_8MHZ => 0x18;
public static UntypedInt NL80211_FREQUENCY_ATTR_ALLOW_6GHZ_VLP_AP => 0x21;
public static UntypedInt NL80211_FREQUENCY_ATTR_CAN_MONITOR => 0x20;
public static UntypedInt NL80211_FREQUENCY_ATTR_DFS_CAC_TIME => 0xd;
public static UntypedInt NL80211_FREQUENCY_ATTR_DFS_CONCURRENT => 0x1d;
public static UntypedInt NL80211_FREQUENCY_ATTR_DFS_STATE => 0x7;
public static UntypedInt NL80211_FREQUENCY_ATTR_DFS_TIME => 0x8;
public static UntypedInt NL80211_FREQUENCY_ATTR_DISABLED => 0x2;
public static UntypedInt NL80211_FREQUENCY_ATTR_FREQ => 0x1;
public static UntypedInt NL80211_FREQUENCY_ATTR_GO_CONCURRENT => 0xf;
public static UntypedInt NL80211_FREQUENCY_ATTR_INDOOR_ONLY => 0xe;
public static UntypedInt NL80211_FREQUENCY_ATTR_IR_CONCURRENT => 0xf;
public static UntypedInt NL80211_FREQUENCY_ATTR_MAX => 0x29;
public static UntypedInt NL80211_FREQUENCY_ATTR_MAX_TX_POWER => 0x6;
public static UntypedInt NL80211_FREQUENCY_ATTR_NO_10MHZ => 0x11;
public static UntypedInt NL80211_FREQUENCY_ATTR_NO_160MHZ => 0xc;
public static UntypedInt NL80211_FREQUENCY_ATTR_NO_20MHZ => 0x10;
public static UntypedInt NL80211_FREQUENCY_ATTR_NO_320MHZ => 0x1a;
public static UntypedInt NL80211_FREQUENCY_ATTR_NO_6GHZ_AFC_CLIENT => 0x1f;
public static UntypedInt NL80211_FREQUENCY_ATTR_NO_6GHZ_VLP_CLIENT => 0x1e;
public static UntypedInt NL80211_FREQUENCY_ATTR_NO_80MHZ => 0xb;
public static UntypedInt NL80211_FREQUENCY_ATTR_NO_EHT => 0x1b;
public static UntypedInt NL80211_FREQUENCY_ATTR_NO_HE => 0x13;
public static UntypedInt NL80211_FREQUENCY_ATTR_NO_HT40_MINUS => 0x9;
public static UntypedInt NL80211_FREQUENCY_ATTR_NO_HT40_PLUS => 0xa;
public static UntypedInt NL80211_FREQUENCY_ATTR_NO_IBSS => 0x3;
public static UntypedInt NL80211_FREQUENCY_ATTR_NO_IR => 0x3;
public static UntypedInt NL80211_FREQUENCY_ATTR_NO_UHB_AFC_CLIENT => 0x1f;
public static UntypedInt NL80211_FREQUENCY_ATTR_NO_UHB_VLP_CLIENT => 0x1e;
public static UntypedInt NL80211_FREQUENCY_ATTR_OFFSET => 0x14;
public static UntypedInt NL80211_FREQUENCY_ATTR_PASSIVE_SCAN => 0x3;
public static UntypedInt NL80211_FREQUENCY_ATTR_PSD => 0x1c;
public static UntypedInt NL80211_FREQUENCY_ATTR_RADAR => 0x5;
public static UntypedInt NL80211_FREQUENCY_ATTR_WMM => 0x12;
public static UntypedInt NL80211_FTM_RESP_ATTR_CIVICLOC => 0x3;
public static UntypedInt NL80211_FTM_RESP_ATTR_ENABLED => 0x1;
public static UntypedInt NL80211_FTM_RESP_ATTR_LCI => 0x2;
public static UntypedInt NL80211_FTM_RESP_ATTR_MAX => 0x3;
public static UntypedInt NL80211_FTM_STATS_ASAP_NUM => 0x4;
public static UntypedInt NL80211_FTM_STATS_FAILED_NUM => 0x3;
public static UntypedInt NL80211_FTM_STATS_MAX => 0xa;
public static UntypedInt NL80211_FTM_STATS_NON_ASAP_NUM => 0x5;
public static UntypedInt NL80211_FTM_STATS_OUT_OF_WINDOW_TRIGGERS_NUM => 0x9;
public static UntypedInt NL80211_FTM_STATS_PAD => 0xa;
public static UntypedInt NL80211_FTM_STATS_PARTIAL_NUM => 0x2;
public static UntypedInt NL80211_FTM_STATS_RESCHEDULE_REQUESTS_NUM => 0x8;
public static UntypedInt NL80211_FTM_STATS_SUCCESS_NUM => 0x1;
public static UntypedInt NL80211_FTM_STATS_TOTAL_DURATION_MSEC => 0x6;
public static UntypedInt NL80211_FTM_STATS_UNKNOWN_TRIGGERS_NUM => 0x7;
public static readonly @string NL80211_GENL_NAME = "nl80211"u8;
public static UntypedInt NL80211_HE_BSS_COLOR_ATTR_COLOR => 0x1;
public static UntypedInt NL80211_HE_BSS_COLOR_ATTR_DISABLED => 0x2;
public static UntypedInt NL80211_HE_BSS_COLOR_ATTR_MAX => 0x3;
public static UntypedInt NL80211_HE_BSS_COLOR_ATTR_PARTIAL => 0x3;
public static UntypedInt NL80211_HE_MAX_CAPABILITY_LEN => 0x36;
public static UntypedInt NL80211_HE_MIN_CAPABILITY_LEN => 0x10;
public static UntypedInt NL80211_HE_NSS_MAX => 0x8;
public static UntypedInt NL80211_HE_OBSS_PD_ATTR_BSS_COLOR_BITMAP => 0x4;
public static UntypedInt NL80211_HE_OBSS_PD_ATTR_MAX => 0x6;
public static UntypedInt NL80211_HE_OBSS_PD_ATTR_MAX_OFFSET => 0x2;
public static UntypedInt NL80211_HE_OBSS_PD_ATTR_MIN_OFFSET => 0x1;
public static UntypedInt NL80211_HE_OBSS_PD_ATTR_NON_SRG_MAX_OFFSET => 0x3;
public static UntypedInt NL80211_HE_OBSS_PD_ATTR_PARTIAL_BSSID_BITMAP => 0x5;
public static UntypedInt NL80211_HE_OBSS_PD_ATTR_SR_CTRL => 0x6;
public static UntypedInt NL80211_HIDDEN_SSID_NOT_IN_USE => 0x0;
public static UntypedInt NL80211_HIDDEN_SSID_ZERO_CONTENTS => 0x2;
public static UntypedInt NL80211_HIDDEN_SSID_ZERO_LEN => 0x1;
public static UntypedInt NL80211_HT_CAPABILITY_LEN => 0x1a;
public static UntypedInt NL80211_IFACE_COMB_BI_MIN_GCD => 0x7;
public static UntypedInt NL80211_IFACE_COMB_LIMITS => 0x1;
public static UntypedInt NL80211_IFACE_COMB_MAXNUM => 0x2;
public static UntypedInt NL80211_IFACE_COMB_NUM_CHANNELS => 0x4;
public static UntypedInt NL80211_IFACE_COMB_RADAR_DETECT_REGIONS => 0x6;
public static UntypedInt NL80211_IFACE_COMB_RADAR_DETECT_WIDTHS => 0x5;
public static UntypedInt NL80211_IFACE_COMB_STA_AP_BI_MATCH => 0x3;
public static UntypedInt NL80211_IFACE_COMB_UNSPEC => 0x0;
public static UntypedInt NL80211_IFACE_LIMIT_MAX => 0x1;
public static UntypedInt NL80211_IFACE_LIMIT_TYPES => 0x2;
public static UntypedInt NL80211_IFACE_LIMIT_UNSPEC => 0x0;
public static UntypedInt NL80211_IFTYPE_ADHOC => 0x1;
public static UntypedInt NL80211_IFTYPE_AKM_ATTR_IFTYPES => 0x1;
public static UntypedInt NL80211_IFTYPE_AKM_ATTR_MAX => 0x2;
public static UntypedInt NL80211_IFTYPE_AKM_ATTR_SUITES => 0x2;
public static UntypedInt NL80211_IFTYPE_AP => 0x3;
public static UntypedInt NL80211_IFTYPE_AP_VLAN => 0x4;
public static UntypedInt NL80211_IFTYPE_MAX => 0xe;
public static UntypedInt NL80211_IFTYPE_MESH_POINT => 0x7;
public static UntypedInt NL80211_IFTYPE_MONITOR => 0x6;
public static UntypedInt NL80211_IFTYPE_NAN => 0xc;
public static UntypedInt NL80211_IFTYPE_OCB => 0xb;
public static UntypedInt NL80211_IFTYPE_P2P_CLIENT => 0x8;
public static UntypedInt NL80211_IFTYPE_P2P_DEVICE => 0xa;
public static UntypedInt NL80211_IFTYPE_P2P_GO => 0x9;
public static UntypedInt NL80211_IFTYPE_STATION => 0x2;
public static UntypedInt NL80211_IFTYPE_UNSPECIFIED => 0x0;
public static UntypedInt NL80211_IFTYPE_WDS => 0x5;
public static UntypedInt NL80211_KCK_EXT_LEN_32 => 0x20;
public static UntypedInt NL80211_KCK_EXT_LEN => 0x18;
public static UntypedInt NL80211_KCK_LEN => 0x10;
public static UntypedInt NL80211_KEK_EXT_LEN => 0x20;
public static UntypedInt NL80211_KEK_LEN => 0x10;
public static UntypedInt NL80211_KEY_CIPHER => 0x3;
public static UntypedInt NL80211_KEY_DATA => 0x1;
public static UntypedInt NL80211_KEY_DEFAULT_BEACON => 0xa;
public static UntypedInt NL80211_KEY_DEFAULT => 0x5;
public static UntypedInt NL80211_KEY_DEFAULT_MGMT => 0x6;
public static UntypedInt NL80211_KEY_DEFAULT_TYPE_MULTICAST => 0x2;
public static UntypedInt NL80211_KEY_DEFAULT_TYPES => 0x8;
public static UntypedInt NL80211_KEY_DEFAULT_TYPE_UNICAST => 0x1;
public static UntypedInt NL80211_KEY_IDX => 0x2;
public static UntypedInt NL80211_KEY_MAX => 0xb;
public static UntypedInt NL80211_KEY_MODE => 0x9;
public static UntypedInt NL80211_KEY_NO_TX => 0x1;
public static UntypedInt NL80211_KEY_RX_TX => 0x0;
public static UntypedInt NL80211_KEY_SEQ => 0x4;
public static UntypedInt NL80211_KEY_SET_TX => 0x2;
public static UntypedInt NL80211_KEY_TYPE => 0x7;
public static UntypedInt NL80211_KEYTYPE_GROUP => 0x0;
public static UntypedInt NL80211_KEYTYPE_PAIRWISE => 0x1;
public static UntypedInt NL80211_KEYTYPE_PEERKEY => 0x2;
public static UntypedInt NL80211_MAX_NR_AKM_SUITES => 0x2;
public static UntypedInt NL80211_MAX_NR_CIPHER_SUITES => 0x5;
public static UntypedInt NL80211_MAX_SUPP_HT_RATES => 0x4d;
public static UntypedInt NL80211_MAX_SUPP_RATES => 0x20;
public static UntypedInt NL80211_MAX_SUPP_REG_RULES => 0x80;
public static UntypedInt NL80211_MAX_SUPP_SELECTORS => 0x80;
public static UntypedInt NL80211_MBSSID_CONFIG_ATTR_EMA => 0x5;
public static UntypedInt NL80211_MBSSID_CONFIG_ATTR_INDEX => 0x3;
public static UntypedInt NL80211_MBSSID_CONFIG_ATTR_MAX => 0x6;
public static UntypedInt NL80211_MBSSID_CONFIG_ATTR_MAX_EMA_PROFILE_PERIODICITY => 0x2;
public static UntypedInt NL80211_MBSSID_CONFIG_ATTR_MAX_INTERFACES => 0x1;
public static UntypedInt NL80211_MBSSID_CONFIG_ATTR_TX_IFINDEX => 0x4;
public static UntypedInt NL80211_MESHCONF_ATTR_MAX => 0x1f;
public static UntypedInt NL80211_MESHCONF_AUTO_OPEN_PLINKS => 0x7;
public static UntypedInt NL80211_MESHCONF_AWAKE_WINDOW => 0x1b;
public static UntypedInt NL80211_MESHCONF_CONFIRM_TIMEOUT => 0x2;
public static UntypedInt NL80211_MESHCONF_CONNECTED_TO_AS => 0x1f;
public static UntypedInt NL80211_MESHCONF_CONNECTED_TO_GATE => 0x1d;
public static UntypedInt NL80211_MESHCONF_ELEMENT_TTL => 0xf;
public static UntypedInt NL80211_MESHCONF_FORWARDING => 0x13;
public static UntypedInt NL80211_MESHCONF_GATE_ANNOUNCEMENTS => 0x11;
public static UntypedInt NL80211_MESHCONF_HOLDING_TIMEOUT => 0x3;
public static UntypedInt NL80211_MESHCONF_HT_OPMODE => 0x16;
public static UntypedInt NL80211_MESHCONF_HWMP_ACTIVE_PATH_TIMEOUT => 0xb;
public static UntypedInt NL80211_MESHCONF_HWMP_CONFIRMATION_INTERVAL => 0x19;
public static UntypedInt NL80211_MESHCONF_HWMP_MAX_PREQ_RETRIES => 0x8;
public static UntypedInt NL80211_MESHCONF_HWMP_NET_DIAM_TRVS_TIME => 0xd;
public static UntypedInt NL80211_MESHCONF_HWMP_PATH_TO_ROOT_TIMEOUT => 0x17;
public static UntypedInt NL80211_MESHCONF_HWMP_PERR_MIN_INTERVAL => 0x12;
public static UntypedInt NL80211_MESHCONF_HWMP_PREQ_MIN_INTERVAL => 0xc;
public static UntypedInt NL80211_MESHCONF_HWMP_RANN_INTERVAL => 0x10;
public static UntypedInt NL80211_MESHCONF_HWMP_ROOT_INTERVAL => 0x18;
public static UntypedInt NL80211_MESHCONF_HWMP_ROOTMODE => 0xe;
public static UntypedInt NL80211_MESHCONF_MAX_PEER_LINKS => 0x4;
public static UntypedInt NL80211_MESHCONF_MAX_RETRIES => 0x5;
public static UntypedInt NL80211_MESHCONF_MIN_DISCOVERY_TIMEOUT => 0xa;
public static UntypedInt NL80211_MESHCONF_NOLEARN => 0x1e;
public static UntypedInt NL80211_MESHCONF_PATH_REFRESH_TIME => 0x9;
public static UntypedInt NL80211_MESHCONF_PLINK_TIMEOUT => 0x1c;
public static UntypedInt NL80211_MESHCONF_POWER_MODE => 0x1a;
public static UntypedInt NL80211_MESHCONF_RETRY_TIMEOUT => 0x1;
public static UntypedInt NL80211_MESHCONF_RSSI_THRESHOLD => 0x14;
public static UntypedInt NL80211_MESHCONF_SYNC_OFFSET_MAX_NEIGHBOR => 0x15;
public static UntypedInt NL80211_MESHCONF_TTL => 0x6;
public static UntypedInt NL80211_MESH_POWER_ACTIVE => 0x1;
public static UntypedInt NL80211_MESH_POWER_DEEP_SLEEP => 0x3;
public static UntypedInt NL80211_MESH_POWER_LIGHT_SLEEP => 0x2;
public static UntypedInt NL80211_MESH_POWER_MAX => 0x3;
public static UntypedInt NL80211_MESH_POWER_UNKNOWN => 0x0;
public static UntypedInt NL80211_MESH_SETUP_ATTR_MAX => 0x8;
public static UntypedInt NL80211_MESH_SETUP_AUTH_PROTOCOL => 0x8;
public static UntypedInt NL80211_MESH_SETUP_ENABLE_VENDOR_METRIC => 0x2;
public static UntypedInt NL80211_MESH_SETUP_ENABLE_VENDOR_PATH_SEL => 0x1;
public static UntypedInt NL80211_MESH_SETUP_ENABLE_VENDOR_SYNC => 0x6;
public static UntypedInt NL80211_MESH_SETUP_IE => 0x3;
public static UntypedInt NL80211_MESH_SETUP_USERSPACE_AMPE => 0x5;
public static UntypedInt NL80211_MESH_SETUP_USERSPACE_AUTH => 0x4;
public static UntypedInt NL80211_MESH_SETUP_USERSPACE_MPM => 0x7;
public static UntypedInt NL80211_MESH_SETUP_VENDOR_PATH_SEL_IE => 0x3;
public static UntypedInt NL80211_MFP_NO => 0x0;
public static UntypedInt NL80211_MFP_OPTIONAL => 0x2;
public static UntypedInt NL80211_MFP_REQUIRED => 0x1;
public static UntypedInt NL80211_MIN_REMAIN_ON_CHANNEL_TIME => 0xa;
public static UntypedInt NL80211_MNTR_FLAG_ACTIVE => 0x6;
public static UntypedInt NL80211_MNTR_FLAG_CONTROL => 0x3;
public static UntypedInt NL80211_MNTR_FLAG_COOK_FRAMES => 0x5;
public static UntypedInt NL80211_MNTR_FLAG_FCSFAIL => 0x1;
public static UntypedInt NL80211_MNTR_FLAG_MAX => 0x7;
public static UntypedInt NL80211_MNTR_FLAG_OTHER_BSS => 0x4;
public static UntypedInt NL80211_MNTR_FLAG_PLCPFAIL => 0x2;
public static UntypedInt NL80211_MPATH_FLAG_ACTIVE => 0x1;
public static UntypedInt NL80211_MPATH_FLAG_FIXED => 0x8;
public static UntypedInt NL80211_MPATH_FLAG_RESOLVED => 0x10;
public static UntypedInt NL80211_MPATH_FLAG_RESOLVING => 0x2;
public static UntypedInt NL80211_MPATH_FLAG_SN_VALID => 0x4;
public static UntypedInt NL80211_MPATH_INFO_DISCOVERY_RETRIES => 0x7;
public static UntypedInt NL80211_MPATH_INFO_DISCOVERY_TIMEOUT => 0x6;
public static UntypedInt NL80211_MPATH_INFO_EXPTIME => 0x4;
public static UntypedInt NL80211_MPATH_INFO_FLAGS => 0x5;
public static UntypedInt NL80211_MPATH_INFO_FRAME_QLEN => 0x1;
public static UntypedInt NL80211_MPATH_INFO_HOP_COUNT => 0x8;
public static UntypedInt NL80211_MPATH_INFO_MAX => 0x9;
public static UntypedInt NL80211_MPATH_INFO_METRIC => 0x3;
public static UntypedInt NL80211_MPATH_INFO_PATH_CHANGE => 0x9;
public static UntypedInt NL80211_MPATH_INFO_SN => 0x2;
public static readonly @string NL80211_MULTICAST_GROUP_CONFIG = "config"u8;
public static readonly @string NL80211_MULTICAST_GROUP_MLME = "mlme"u8;
public static readonly @string NL80211_MULTICAST_GROUP_NAN = "nan"u8;
public static readonly @string NL80211_MULTICAST_GROUP_REG = "regulatory"u8;
public static readonly @string NL80211_MULTICAST_GROUP_SCAN = "scan"u8;
public static readonly @string NL80211_MULTICAST_GROUP_TESTMODE = "testmode"u8;
public static readonly @string NL80211_MULTICAST_GROUP_VENDOR = "vendor"u8;
public static UntypedInt NL80211_NAN_FUNC_ATTR_MAX => 0x10;
public static UntypedInt NL80211_NAN_FUNC_CLOSE_RANGE => 0x9;
public static UntypedInt NL80211_NAN_FUNC_FOLLOW_UP => 0x2;
public static UntypedInt NL80211_NAN_FUNC_FOLLOW_UP_DEST => 0x8;
public static UntypedInt NL80211_NAN_FUNC_FOLLOW_UP_ID => 0x6;
public static UntypedInt NL80211_NAN_FUNC_FOLLOW_UP_REQ_ID => 0x7;
public static UntypedInt NL80211_NAN_FUNC_INSTANCE_ID => 0xf;
public static UntypedInt NL80211_NAN_FUNC_MAX_TYPE => 0x2;
public static UntypedInt NL80211_NAN_FUNC_PUBLISH_BCAST => 0x4;
public static UntypedInt NL80211_NAN_FUNC_PUBLISH => 0x0;
public static UntypedInt NL80211_NAN_FUNC_PUBLISH_TYPE => 0x3;
public static UntypedInt NL80211_NAN_FUNC_RX_MATCH_FILTER => 0xd;
public static UntypedInt NL80211_NAN_FUNC_SERVICE_ID => 0x2;
public static UntypedInt NL80211_NAN_FUNC_SERVICE_ID_LEN => 0x6;
public static UntypedInt NL80211_NAN_FUNC_SERVICE_INFO => 0xb;
public static UntypedInt NL80211_NAN_FUNC_SERVICE_SPEC_INFO_MAX_LEN => 0xff;
public static UntypedInt NL80211_NAN_FUNC_SRF => 0xc;
public static UntypedInt NL80211_NAN_FUNC_SRF_MAX_LEN => 0xff;
public static UntypedInt NL80211_NAN_FUNC_SUBSCRIBE_ACTIVE => 0x5;
public static UntypedInt NL80211_NAN_FUNC_SUBSCRIBE => 0x1;
public static UntypedInt NL80211_NAN_FUNC_TERM_REASON => 0x10;
public static UntypedInt NL80211_NAN_FUNC_TERM_REASON_ERROR => 0x2;
public static UntypedInt NL80211_NAN_FUNC_TERM_REASON_TTL_EXPIRED => 0x1;
public static UntypedInt NL80211_NAN_FUNC_TERM_REASON_USER_REQUEST => 0x0;
public static UntypedInt NL80211_NAN_FUNC_TTL => 0xa;
public static UntypedInt NL80211_NAN_FUNC_TX_MATCH_FILTER => 0xe;
public static UntypedInt NL80211_NAN_FUNC_TYPE => 0x1;
public static UntypedInt NL80211_NAN_MATCH_ATTR_MAX => 0x2;
public static UntypedInt NL80211_NAN_MATCH_FUNC_LOCAL => 0x1;
public static UntypedInt NL80211_NAN_MATCH_FUNC_PEER => 0x2;
public static UntypedInt NL80211_NAN_SOLICITED_PUBLISH => 0x1;
public static UntypedInt NL80211_NAN_SRF_ATTR_MAX => 0x4;
public static UntypedInt NL80211_NAN_SRF_BF => 0x2;
public static UntypedInt NL80211_NAN_SRF_BF_IDX => 0x3;
public static UntypedInt NL80211_NAN_SRF_INCLUDE => 0x1;
public static UntypedInt NL80211_NAN_SRF_MAC_ADDRS => 0x4;
public static UntypedInt NL80211_NAN_UNSOLICITED_PUBLISH => 0x2;
public static UntypedInt NL80211_NUM_ACS => 0x4;
public static UntypedInt NL80211_P2P_PS_SUPPORTED => 0x1;
public static UntypedInt NL80211_P2P_PS_UNSUPPORTED => 0x0;
public static UntypedInt NL80211_PKTPAT_MASK => 0x1;
public static UntypedInt NL80211_PKTPAT_OFFSET => 0x3;
public static UntypedInt NL80211_PKTPAT_PATTERN => 0x2;
public static UntypedInt NL80211_PLINK_ACTION_BLOCK => 0x2;
public static UntypedInt NL80211_PLINK_ACTION_NO_ACTION => 0x0;
public static UntypedInt NL80211_PLINK_ACTION_OPEN => 0x1;
public static UntypedInt NL80211_PLINK_BLOCKED => 0x6;
public static UntypedInt NL80211_PLINK_CNF_RCVD => 0x3;
public static UntypedInt NL80211_PLINK_ESTAB => 0x4;
public static UntypedInt NL80211_PLINK_HOLDING => 0x5;
public static UntypedInt NL80211_PLINK_LISTEN => 0x0;
public static UntypedInt NL80211_PLINK_OPN_RCVD => 0x2;
public static UntypedInt NL80211_PLINK_OPN_SNT => 0x1;
public static UntypedInt NL80211_PMKSA_CANDIDATE_BSSID => 0x2;
public static UntypedInt NL80211_PMKSA_CANDIDATE_INDEX => 0x1;
public static UntypedInt NL80211_PMKSA_CANDIDATE_PREAUTH => 0x3;
public static UntypedInt NL80211_PMSR_ATTR_MAX => 0x7;
public static UntypedInt NL80211_PMSR_ATTR_MAX_PEERS => 0x1;
public static UntypedInt NL80211_PMSR_ATTR_PEERS => 0x5;
public static UntypedInt NL80211_PMSR_ATTR_RANDOMIZE_MAC_ADDR => 0x3;
public static UntypedInt NL80211_PMSR_ATTR_REPORT_AP_TSF => 0x2;
public static UntypedInt NL80211_PMSR_ATTR_TYPE_CAPA => 0x4;
public static UntypedInt NL80211_PMSR_FTM_CAPA_ATTR_ASAP => 0x1;
public static UntypedInt NL80211_PMSR_FTM_CAPA_ATTR_BANDWIDTHS => 0x6;
public static UntypedInt NL80211_PMSR_FTM_CAPA_ATTR_MAX_BURSTS_EXPONENT => 0x7;
public static UntypedInt NL80211_PMSR_FTM_CAPA_ATTR_MAX => 0x1f;
public static UntypedInt NL80211_PMSR_FTM_CAPA_ATTR_MAX_FTMS_PER_BURST => 0x8;
public static UntypedInt NL80211_PMSR_FTM_CAPA_ATTR_NON_ASAP => 0x2;
public static UntypedInt NL80211_PMSR_FTM_CAPA_ATTR_NON_TRIGGER_BASED => 0xa;
public static UntypedInt NL80211_PMSR_FTM_CAPA_ATTR_PREAMBLES => 0x5;
public static UntypedInt NL80211_PMSR_FTM_CAPA_ATTR_REQ_CIVICLOC => 0x4;
public static UntypedInt NL80211_PMSR_FTM_CAPA_ATTR_REQ_LCI => 0x3;
public static UntypedInt NL80211_PMSR_FTM_CAPA_ATTR_TRIGGER_BASED => 0x9;
public static UntypedInt NL80211_PMSR_FTM_FAILURE_BAD_CHANGED_PARAMS => 0x7;
public static UntypedInt NL80211_PMSR_FTM_FAILURE_INVALID_TIMESTAMP => 0x5;
public static UntypedInt NL80211_PMSR_FTM_FAILURE_NO_RESPONSE => 0x1;
public static UntypedInt NL80211_PMSR_FTM_FAILURE_PEER_BUSY => 0x6;
public static UntypedInt NL80211_PMSR_FTM_FAILURE_PEER_NOT_CAPABLE => 0x4;
public static UntypedInt NL80211_PMSR_FTM_FAILURE_REJECTED => 0x2;
public static UntypedInt NL80211_PMSR_FTM_FAILURE_UNSPECIFIED => 0x0;
public static UntypedInt NL80211_PMSR_FTM_FAILURE_WRONG_CHANNEL => 0x3;
public static UntypedInt NL80211_PMSR_FTM_REQ_ATTR_ASAP => 0x1;
public static UntypedInt NL80211_PMSR_FTM_REQ_ATTR_BSS_COLOR => 0xd;
public static UntypedInt NL80211_PMSR_FTM_REQ_ATTR_BURST_DURATION => 0x5;
public static UntypedInt NL80211_PMSR_FTM_REQ_ATTR_BURST_PERIOD => 0x4;
public static UntypedInt NL80211_PMSR_FTM_REQ_ATTR_FTMS_PER_BURST => 0x6;
public static UntypedInt NL80211_PMSR_FTM_REQ_ATTR_LMR_FEEDBACK => 0xc;
public static UntypedInt NL80211_PMSR_FTM_REQ_ATTR_MAX => 0x17;
public static UntypedInt NL80211_PMSR_FTM_REQ_ATTR_NON_TRIGGER_BASED => 0xb;
public static UntypedInt NL80211_PMSR_FTM_REQ_ATTR_NUM_BURSTS_EXP => 0x3;
public static UntypedInt NL80211_PMSR_FTM_REQ_ATTR_NUM_FTMR_RETRIES => 0x7;
public static UntypedInt NL80211_PMSR_FTM_REQ_ATTR_PREAMBLE => 0x2;
public static UntypedInt NL80211_PMSR_FTM_REQ_ATTR_REQUEST_CIVICLOC => 0x9;
public static UntypedInt NL80211_PMSR_FTM_REQ_ATTR_REQUEST_LCI => 0x8;
public static UntypedInt NL80211_PMSR_FTM_REQ_ATTR_TRIGGER_BASED => 0xa;
public static UntypedInt NL80211_PMSR_FTM_RESP_ATTR_BURST_DURATION => 0x7;
public static UntypedInt NL80211_PMSR_FTM_RESP_ATTR_BURST_INDEX => 0x2;
public static UntypedInt NL80211_PMSR_FTM_RESP_ATTR_BUSY_RETRY_TIME => 0x5;
public static UntypedInt NL80211_PMSR_FTM_RESP_ATTR_CIVICLOC => 0x14;
public static UntypedInt NL80211_PMSR_FTM_RESP_ATTR_DIST_AVG => 0x10;
public static UntypedInt NL80211_PMSR_FTM_RESP_ATTR_DIST_SPREAD => 0x12;
public static UntypedInt NL80211_PMSR_FTM_RESP_ATTR_DIST_VARIANCE => 0x11;
public static UntypedInt NL80211_PMSR_FTM_RESP_ATTR_FAIL_REASON => 0x1;
public static UntypedInt NL80211_PMSR_FTM_RESP_ATTR_FTMS_PER_BURST => 0x8;
public static UntypedInt NL80211_PMSR_FTM_RESP_ATTR_LCI => 0x13;
public static UntypedInt NL80211_PMSR_FTM_RESP_ATTR_MAX => 0x21;
public static UntypedInt NL80211_PMSR_FTM_RESP_ATTR_NUM_BURSTS_EXP => 0x6;
public static UntypedInt NL80211_PMSR_FTM_RESP_ATTR_NUM_FTMR_ATTEMPTS => 0x3;
public static UntypedInt NL80211_PMSR_FTM_RESP_ATTR_NUM_FTMR_SUCCESSES => 0x4;
public static UntypedInt NL80211_PMSR_FTM_RESP_ATTR_PAD => 0x15;
public static UntypedInt NL80211_PMSR_FTM_RESP_ATTR_RSSI_AVG => 0x9;
public static UntypedInt NL80211_PMSR_FTM_RESP_ATTR_RSSI_SPREAD => 0xa;
public static UntypedInt NL80211_PMSR_FTM_RESP_ATTR_RTT_AVG => 0xd;
public static UntypedInt NL80211_PMSR_FTM_RESP_ATTR_RTT_SPREAD => 0xf;
public static UntypedInt NL80211_PMSR_FTM_RESP_ATTR_RTT_VARIANCE => 0xe;
public static UntypedInt NL80211_PMSR_FTM_RESP_ATTR_RX_RATE => 0xc;
public static UntypedInt NL80211_PMSR_FTM_RESP_ATTR_TX_RATE => 0xb;
public static UntypedInt NL80211_PMSR_PEER_ATTR_ADDR => 0x1;
public static UntypedInt NL80211_PMSR_PEER_ATTR_CHAN => 0x2;
public static UntypedInt NL80211_PMSR_PEER_ATTR_MAX => 0x5;
public static UntypedInt NL80211_PMSR_PEER_ATTR_REQ => 0x3;
public static UntypedInt NL80211_PMSR_PEER_ATTR_RESP => 0x4;
public static UntypedInt NL80211_PMSR_REQ_ATTR_DATA => 0x1;
public static UntypedInt NL80211_PMSR_REQ_ATTR_GET_AP_TSF => 0x2;
public static UntypedInt NL80211_PMSR_REQ_ATTR_MAX => 0x2;
public static UntypedInt NL80211_PMSR_RESP_ATTR_AP_TSF => 0x4;
public static UntypedInt NL80211_PMSR_RESP_ATTR_DATA => 0x1;
public static UntypedInt NL80211_PMSR_RESP_ATTR_FINAL => 0x5;
public static UntypedInt NL80211_PMSR_RESP_ATTR_HOST_TIME => 0x3;
public static UntypedInt NL80211_PMSR_RESP_ATTR_MAX => 0x6;
public static UntypedInt NL80211_PMSR_RESP_ATTR_PAD => 0x6;
public static UntypedInt NL80211_PMSR_RESP_ATTR_STATUS => 0x2;
public static UntypedInt NL80211_PMSR_STATUS_FAILURE => 0x3;
public static UntypedInt NL80211_PMSR_STATUS_REFUSED => 0x1;
public static UntypedInt NL80211_PMSR_STATUS_SUCCESS => 0x0;
public static UntypedInt NL80211_PMSR_STATUS_TIMEOUT => 0x2;
public static UntypedInt NL80211_PMSR_TYPE_FTM => 0x1;
public static UntypedInt NL80211_PMSR_TYPE_INVALID => 0x0;
public static UntypedInt NL80211_PMSR_TYPE_MAX => 0x1;
public static UntypedInt NL80211_PREAMBLE_DMG => 0x3;
public static UntypedInt NL80211_PREAMBLE_HE => 0x4;
public static UntypedInt NL80211_PREAMBLE_HT => 0x1;
public static UntypedInt NL80211_PREAMBLE_LEGACY => 0x0;
public static UntypedInt NL80211_PREAMBLE_VHT => 0x2;
public static UntypedInt NL80211_PROBE_RESP_OFFLOAD_SUPPORT_80211U => 0x8;
public static UntypedInt NL80211_PROBE_RESP_OFFLOAD_SUPPORT_P2P => 0x4;
public static UntypedInt NL80211_PROBE_RESP_OFFLOAD_SUPPORT_WPS2 => 0x2;
public static UntypedInt NL80211_PROBE_RESP_OFFLOAD_SUPPORT_WPS => 0x1;
public static UntypedInt NL80211_PROTOCOL_FEATURE_SPLIT_WIPHY_DUMP => 0x1;
public static UntypedInt NL80211_PS_DISABLED => 0x0;
public static UntypedInt NL80211_PS_ENABLED => 0x1;
public static UntypedInt NL80211_RADAR_CAC_ABORTED => 0x2;
public static UntypedInt NL80211_RADAR_CAC_FINISHED => 0x1;
public static UntypedInt NL80211_RADAR_CAC_STARTED => 0x5;
public static UntypedInt NL80211_RADAR_DETECTED => 0x0;
public static UntypedInt NL80211_RADAR_NOP_FINISHED => 0x3;
public static UntypedInt NL80211_RADAR_PRE_CAC_EXPIRED => 0x4;
public static UntypedInt NL80211_RATE_INFO_10_MHZ_WIDTH => 0xb;
public static UntypedInt NL80211_RATE_INFO_160_MHZ_WIDTH => 0xa;
public static UntypedInt NL80211_RATE_INFO_16_MHZ_WIDTH => 0x1d;
public static UntypedInt NL80211_RATE_INFO_1_MHZ_WIDTH => 0x19;
public static UntypedInt NL80211_RATE_INFO_2_MHZ_WIDTH => 0x1a;
public static UntypedInt NL80211_RATE_INFO_320_MHZ_WIDTH => 0x12;
public static UntypedInt NL80211_RATE_INFO_40_MHZ_WIDTH => 0x3;
public static UntypedInt NL80211_RATE_INFO_4_MHZ_WIDTH => 0x1b;
public static UntypedInt NL80211_RATE_INFO_5_MHZ_WIDTH => 0xc;
public static UntypedInt NL80211_RATE_INFO_80_MHZ_WIDTH => 0x8;
public static UntypedInt NL80211_RATE_INFO_80P80_MHZ_WIDTH => 0x9;
public static UntypedInt NL80211_RATE_INFO_8_MHZ_WIDTH => 0x1c;
public static UntypedInt NL80211_RATE_INFO_BITRATE32 => 0x5;
public static UntypedInt NL80211_RATE_INFO_BITRATE => 0x1;
public static UntypedInt NL80211_RATE_INFO_EHT_GI_0_8 => 0x0;
public static UntypedInt NL80211_RATE_INFO_EHT_GI_1_6 => 0x1;
public static UntypedInt NL80211_RATE_INFO_EHT_GI_3_2 => 0x2;
public static UntypedInt NL80211_RATE_INFO_EHT_GI => 0x15;
public static UntypedInt NL80211_RATE_INFO_EHT_MCS => 0x13;
public static UntypedInt NL80211_RATE_INFO_EHT_NSS => 0x14;
public static UntypedInt NL80211_RATE_INFO_EHT_RU_ALLOC_106 => 0x3;
public static UntypedInt NL80211_RATE_INFO_EHT_RU_ALLOC_106P26 => 0x4;
public static UntypedInt NL80211_RATE_INFO_EHT_RU_ALLOC_242 => 0x5;
public static UntypedInt NL80211_RATE_INFO_EHT_RU_ALLOC_26 => 0x0;
public static UntypedInt NL80211_RATE_INFO_EHT_RU_ALLOC_2x996 => 0xb;
public static UntypedInt NL80211_RATE_INFO_EHT_RU_ALLOC_2x996P484 => 0xc;
public static UntypedInt NL80211_RATE_INFO_EHT_RU_ALLOC_3x996 => 0xd;
public static UntypedInt NL80211_RATE_INFO_EHT_RU_ALLOC_3x996P484 => 0xe;
public static UntypedInt NL80211_RATE_INFO_EHT_RU_ALLOC_484 => 0x6;
public static UntypedInt NL80211_RATE_INFO_EHT_RU_ALLOC_484P242 => 0x7;
public static UntypedInt NL80211_RATE_INFO_EHT_RU_ALLOC_4x996 => 0xf;
public static UntypedInt NL80211_RATE_INFO_EHT_RU_ALLOC_52 => 0x1;
public static UntypedInt NL80211_RATE_INFO_EHT_RU_ALLOC_52P26 => 0x2;
public static UntypedInt NL80211_RATE_INFO_EHT_RU_ALLOC_996 => 0x8;
public static UntypedInt NL80211_RATE_INFO_EHT_RU_ALLOC_996P484 => 0x9;
public static UntypedInt NL80211_RATE_INFO_EHT_RU_ALLOC_996P484P242 => 0xa;
public static UntypedInt NL80211_RATE_INFO_EHT_RU_ALLOC => 0x16;
public static UntypedInt NL80211_RATE_INFO_HE_1XLTF => 0x0;
public static UntypedInt NL80211_RATE_INFO_HE_2XLTF => 0x1;
public static UntypedInt NL80211_RATE_INFO_HE_4XLTF => 0x2;
public static UntypedInt NL80211_RATE_INFO_HE_DCM => 0x10;
public static UntypedInt NL80211_RATE_INFO_HE_GI_0_8 => 0x0;
public static UntypedInt NL80211_RATE_INFO_HE_GI_1_6 => 0x1;
public static UntypedInt NL80211_RATE_INFO_HE_GI_3_2 => 0x2;
public static UntypedInt NL80211_RATE_INFO_HE_GI => 0xf;
public static UntypedInt NL80211_RATE_INFO_HE_MCS => 0xd;
public static UntypedInt NL80211_RATE_INFO_HE_NSS => 0xe;
public static UntypedInt NL80211_RATE_INFO_HE_RU_ALLOC_106 => 0x2;
public static UntypedInt NL80211_RATE_INFO_HE_RU_ALLOC_242 => 0x3;
public static UntypedInt NL80211_RATE_INFO_HE_RU_ALLOC_26 => 0x0;
public static UntypedInt NL80211_RATE_INFO_HE_RU_ALLOC_2x996 => 0x6;
public static UntypedInt NL80211_RATE_INFO_HE_RU_ALLOC_484 => 0x4;
public static UntypedInt NL80211_RATE_INFO_HE_RU_ALLOC_52 => 0x1;
public static UntypedInt NL80211_RATE_INFO_HE_RU_ALLOC_996 => 0x5;
public static UntypedInt NL80211_RATE_INFO_HE_RU_ALLOC => 0x11;
public static UntypedInt NL80211_RATE_INFO_MAX => 0x20;
public static UntypedInt NL80211_RATE_INFO_MCS => 0x2;
public static UntypedInt NL80211_RATE_INFO_S1G_MCS => 0x17;
public static UntypedInt NL80211_RATE_INFO_S1G_NSS => 0x18;
public static UntypedInt NL80211_RATE_INFO_SHORT_GI => 0x4;
public static UntypedInt NL80211_RATE_INFO_VHT_MCS => 0x6;
public static UntypedInt NL80211_RATE_INFO_VHT_NSS => 0x7;
public static UntypedInt NL80211_REGDOM_SET_BY_CORE => 0x0;
public static UntypedInt NL80211_REGDOM_SET_BY_COUNTRY_IE => 0x3;
public static UntypedInt NL80211_REGDOM_SET_BY_DRIVER => 0x2;
public static UntypedInt NL80211_REGDOM_SET_BY_USER => 0x1;
public static UntypedInt NL80211_REGDOM_TYPE_COUNTRY => 0x0;
public static UntypedInt NL80211_REGDOM_TYPE_CUSTOM_WORLD => 0x2;
public static UntypedInt NL80211_REGDOM_TYPE_INTERSECTION => 0x3;
public static UntypedInt NL80211_REGDOM_TYPE_WORLD => 0x1;
public static UntypedInt NL80211_REG_RULE_ATTR_MAX => 0x8;
public static UntypedInt NL80211_REKEY_DATA_AKM => 0x4;
public static UntypedInt NL80211_REKEY_DATA_KCK => 0x2;
public static UntypedInt NL80211_REKEY_DATA_KEK => 0x1;
public static UntypedInt NL80211_REKEY_DATA_REPLAY_CTR => 0x3;
public static UntypedInt NL80211_REPLAY_CTR_LEN => 0x8;
public static UntypedInt NL80211_RRF_ALLOW_6GHZ_VLP_AP => 0x1000000;
public static UntypedInt NL80211_RRF_AUTO_BW => 0x800;
public static UntypedInt NL80211_RRF_DFS => 0x10;
public static UntypedInt NL80211_RRF_DFS_CONCURRENT => 0x200000;
public static UntypedInt NL80211_RRF_GO_CONCURRENT => 0x1000;
public static UntypedInt NL80211_RRF_IR_CONCURRENT => 0x1000;
public static UntypedInt NL80211_RRF_NO_160MHZ => 0x10000;
public static UntypedInt NL80211_RRF_NO_320MHZ => 0x40000;
public static UntypedInt NL80211_RRF_NO_6GHZ_AFC_CLIENT => 0x800000;
public static UntypedInt NL80211_RRF_NO_6GHZ_VLP_CLIENT => 0x400000;
public static UntypedInt NL80211_RRF_NO_80MHZ => 0x8000;
public static UntypedInt NL80211_RRF_NO_CCK => 0x2;
public static UntypedInt NL80211_RRF_NO_EHT => 0x80000;
public static UntypedInt NL80211_RRF_NO_HE => 0x20000;
public static UntypedInt NL80211_RRF_NO_HT40 => 0x6000;
public static UntypedInt NL80211_RRF_NO_HT40MINUS => 0x2000;
public static UntypedInt NL80211_RRF_NO_HT40PLUS => 0x4000;
public static UntypedInt NL80211_RRF_NO_IBSS => 0x80;
public static UntypedInt NL80211_RRF_NO_INDOOR => 0x4;
public static UntypedInt NL80211_RRF_NO_IR_ALL => 0x180;
public static UntypedInt NL80211_RRF_NO_IR => 0x80;
public static UntypedInt NL80211_RRF_NO_OFDM => 0x1;
public static UntypedInt NL80211_RRF_NO_OUTDOOR => 0x8;
public static UntypedInt NL80211_RRF_NO_UHB_AFC_CLIENT => 0x800000;
public static UntypedInt NL80211_RRF_NO_UHB_VLP_CLIENT => 0x400000;
public static UntypedInt NL80211_RRF_PASSIVE_SCAN => 0x80;
public static UntypedInt NL80211_RRF_PSD => 0x100000;
public static UntypedInt NL80211_RRF_PTMP_ONLY => 0x40;
public static UntypedInt NL80211_RRF_PTP_ONLY => 0x20;
public static UntypedInt NL80211_RXMGMT_FLAG_ANSWERED => 0x1;
public static UntypedInt NL80211_RXMGMT_FLAG_EXTERNAL_AUTH => 0x2;
public static UntypedInt NL80211_SAE_PWE_BOTH => 0x3;
public static UntypedInt NL80211_SAE_PWE_HASH_TO_ELEMENT => 0x2;
public static UntypedInt NL80211_SAE_PWE_HUNT_AND_PECK => 0x1;
public static UntypedInt NL80211_SAE_PWE_UNSPECIFIED => 0x0;
public static UntypedInt NL80211_SAR_ATTR_MAX => 0x2;
public static UntypedInt NL80211_SAR_ATTR_SPECS => 0x2;
public static UntypedInt NL80211_SAR_ATTR_SPECS_END_FREQ => 0x4;
public static UntypedInt NL80211_SAR_ATTR_SPECS_MAX => 0x4;
public static UntypedInt NL80211_SAR_ATTR_SPECS_POWER => 0x1;
public static UntypedInt NL80211_SAR_ATTR_SPECS_RANGE_INDEX => 0x2;
public static UntypedInt NL80211_SAR_ATTR_SPECS_START_FREQ => 0x3;
public static UntypedInt NL80211_SAR_ATTR_TYPE => 0x1;
public static UntypedInt NL80211_SAR_TYPE_POWER => 0x0;
public static UntypedInt NL80211_SCAN_FLAG_ACCEPT_BCAST_PROBE_RESP => 0x20;
public static UntypedInt NL80211_SCAN_FLAG_AP => 0x4;
public static UntypedInt NL80211_SCAN_FLAG_COLOCATED_6GHZ => 0x4000;
public static UntypedInt NL80211_SCAN_FLAG_FILS_MAX_CHANNEL_TIME => 0x10;
public static UntypedInt NL80211_SCAN_FLAG_FLUSH => 0x2;
public static UntypedInt NL80211_SCAN_FLAG_FREQ_KHZ => 0x2000;
public static UntypedInt NL80211_SCAN_FLAG_HIGH_ACCURACY => 0x400;
public static UntypedInt NL80211_SCAN_FLAG_LOW_POWER => 0x200;
public static UntypedInt NL80211_SCAN_FLAG_LOW_PRIORITY => 0x1;
public static UntypedInt NL80211_SCAN_FLAG_LOW_SPAN => 0x100;
public static UntypedInt NL80211_SCAN_FLAG_MIN_PREQ_CONTENT => 0x1000;
public static UntypedInt NL80211_SCAN_FLAG_OCE_PROBE_REQ_DEFERRAL_SUPPRESSION => 0x80;
public static UntypedInt NL80211_SCAN_FLAG_OCE_PROBE_REQ_HIGH_TX_RATE => 0x40;
public static UntypedInt NL80211_SCAN_FLAG_RANDOM_ADDR => 0x8;
public static UntypedInt NL80211_SCAN_FLAG_RANDOM_SN => 0x800;
public static UntypedInt NL80211_SCAN_RSSI_THOLD_OFF => /* -0x12c */ -300;
public static UntypedInt NL80211_SCHED_SCAN_MATCH_ATTR_BSSID => 0x5;
public static UntypedInt NL80211_SCHED_SCAN_MATCH_ATTR_MAX => 0x6;
public static UntypedInt NL80211_SCHED_SCAN_MATCH_ATTR_RELATIVE_RSSI => 0x3;
public static UntypedInt NL80211_SCHED_SCAN_MATCH_ATTR_RSSI_ADJUST => 0x4;
public static UntypedInt NL80211_SCHED_SCAN_MATCH_ATTR_RSSI => 0x2;
public static UntypedInt NL80211_SCHED_SCAN_MATCH_ATTR_SSID => 0x1;
public static UntypedInt NL80211_SCHED_SCAN_MATCH_PER_BAND_RSSI => 0x6;
public static UntypedInt NL80211_SCHED_SCAN_PLAN_INTERVAL => 0x1;
public static UntypedInt NL80211_SCHED_SCAN_PLAN_ITERATIONS => 0x2;
public static UntypedInt NL80211_SCHED_SCAN_PLAN_MAX => 0x2;
public static UntypedInt NL80211_SMPS_DYNAMIC => 0x2;
public static UntypedInt NL80211_SMPS_MAX => 0x2;
public static UntypedInt NL80211_SMPS_OFF => 0x0;
public static UntypedInt NL80211_SMPS_STATIC => 0x1;
public static UntypedInt NL80211_STA_BSS_PARAM_BEACON_INTERVAL => 0x5;
public static UntypedInt NL80211_STA_BSS_PARAM_CTS_PROT => 0x1;
public static UntypedInt NL80211_STA_BSS_PARAM_DTIM_PERIOD => 0x4;
public static UntypedInt NL80211_STA_BSS_PARAM_MAX => 0x5;
public static UntypedInt NL80211_STA_BSS_PARAM_SHORT_PREAMBLE => 0x2;
public static UntypedInt NL80211_STA_BSS_PARAM_SHORT_SLOT_TIME => 0x3;
public static UntypedInt NL80211_STA_FLAG_ASSOCIATED => 0x7;
public static UntypedInt NL80211_STA_FLAG_AUTHENTICATED => 0x5;
public static UntypedInt NL80211_STA_FLAG_AUTHORIZED => 0x1;
public static UntypedInt NL80211_STA_FLAG_MAX => 0x8;
public static UntypedInt NL80211_STA_FLAG_MAX_OLD_API => 0x6;
public static UntypedInt NL80211_STA_FLAG_MFP => 0x4;
public static UntypedInt NL80211_STA_FLAG_SHORT_PREAMBLE => 0x2;
public static UntypedInt NL80211_STA_FLAG_SPP_AMSDU => 0x8;
public static UntypedInt NL80211_STA_FLAG_TDLS_PEER => 0x6;
public static UntypedInt NL80211_STA_FLAG_WME => 0x3;
public static UntypedInt NL80211_STA_INFO_ACK_SIGNAL_AVG => 0x23;
public static UntypedInt NL80211_STA_INFO_ACK_SIGNAL => 0x22;
public static UntypedInt NL80211_STA_INFO_AIRTIME_LINK_METRIC => 0x29;
public static UntypedInt NL80211_STA_INFO_AIRTIME_WEIGHT => 0x28;
public static UntypedInt NL80211_STA_INFO_ASSOC_AT_BOOTTIME => 0x2a;
public static UntypedInt NL80211_STA_INFO_BEACON_LOSS => 0x12;
public static UntypedInt NL80211_STA_INFO_BEACON_RX => 0x1d;
public static UntypedInt NL80211_STA_INFO_BEACON_SIGNAL_AVG => 0x1e;
public static UntypedInt NL80211_STA_INFO_BSS_PARAM => 0xf;
public static UntypedInt NL80211_STA_INFO_CHAIN_SIGNAL_AVG => 0x1a;
public static UntypedInt NL80211_STA_INFO_CHAIN_SIGNAL => 0x19;
public static UntypedInt NL80211_STA_INFO_CONNECTED_TIME => 0x10;
public static UntypedInt NL80211_STA_INFO_CONNECTED_TO_AS => 0x2b;
public static UntypedInt NL80211_STA_INFO_CONNECTED_TO_GATE => 0x26;
public static UntypedInt NL80211_STA_INFO_DATA_ACK_SIGNAL_AVG => 0x23;
public static UntypedInt NL80211_STA_INFO_EXPECTED_THROUGHPUT => 0x1b;
public static UntypedInt NL80211_STA_INFO_FCS_ERROR_COUNT => 0x25;
public static UntypedInt NL80211_STA_INFO_INACTIVE_TIME => 0x1;
public static UntypedInt NL80211_STA_INFO_LLID => 0x4;
public static UntypedInt NL80211_STA_INFO_LOCAL_PM => 0x14;
public static UntypedInt NL80211_STA_INFO_MAX => 0x2b;
public static UntypedInt NL80211_STA_INFO_NONPEER_PM => 0x16;
public static UntypedInt NL80211_STA_INFO_PAD => 0x21;
public static UntypedInt NL80211_STA_INFO_PEER_PM => 0x15;
public static UntypedInt NL80211_STA_INFO_PLID => 0x5;
public static UntypedInt NL80211_STA_INFO_PLINK_STATE => 0x6;
public static UntypedInt NL80211_STA_INFO_RX_BITRATE => 0xe;
public static UntypedInt NL80211_STA_INFO_RX_BYTES64 => 0x17;
public static UntypedInt NL80211_STA_INFO_RX_BYTES => 0x2;
public static UntypedInt NL80211_STA_INFO_RX_DROP_MISC => 0x1c;
public static UntypedInt NL80211_STA_INFO_RX_DURATION => 0x20;
public static UntypedInt NL80211_STA_INFO_RX_MPDUS => 0x24;
public static UntypedInt NL80211_STA_INFO_RX_PACKETS => 0x9;
public static UntypedInt NL80211_STA_INFO_SIGNAL_AVG => 0xd;
public static UntypedInt NL80211_STA_INFO_SIGNAL => 0x7;
public static UntypedInt NL80211_STA_INFO_STA_FLAGS => 0x11;
public static UntypedInt NL80211_STA_INFO_TID_STATS => 0x1f;
public static UntypedInt NL80211_STA_INFO_T_OFFSET => 0x13;
public static UntypedInt NL80211_STA_INFO_TX_BITRATE => 0x8;
public static UntypedInt NL80211_STA_INFO_TX_BYTES64 => 0x18;
public static UntypedInt NL80211_STA_INFO_TX_BYTES => 0x3;
public static UntypedInt NL80211_STA_INFO_TX_DURATION => 0x27;
public static UntypedInt NL80211_STA_INFO_TX_FAILED => 0xc;
public static UntypedInt NL80211_STA_INFO_TX_PACKETS => 0xa;
public static UntypedInt NL80211_STA_INFO_TX_RETRIES => 0xb;
public static UntypedInt NL80211_STA_WME_MAX => 0x2;
public static UntypedInt NL80211_STA_WME_MAX_SP => 0x2;
public static UntypedInt NL80211_STA_WME_UAPSD_QUEUES => 0x1;
public static UntypedInt NL80211_SURVEY_INFO_CHANNEL_TIME_BUSY => 0x5;
public static UntypedInt NL80211_SURVEY_INFO_CHANNEL_TIME => 0x4;
public static UntypedInt NL80211_SURVEY_INFO_CHANNEL_TIME_EXT_BUSY => 0x6;
public static UntypedInt NL80211_SURVEY_INFO_CHANNEL_TIME_RX => 0x7;
public static UntypedInt NL80211_SURVEY_INFO_CHANNEL_TIME_TX => 0x8;
public static UntypedInt NL80211_SURVEY_INFO_FREQUENCY => 0x1;
public static UntypedInt NL80211_SURVEY_INFO_FREQUENCY_OFFSET => 0xc;
public static UntypedInt NL80211_SURVEY_INFO_IN_USE => 0x3;
public static UntypedInt NL80211_SURVEY_INFO_MAX => 0xc;
public static UntypedInt NL80211_SURVEY_INFO_NOISE => 0x2;
public static UntypedInt NL80211_SURVEY_INFO_PAD => 0xa;
public static UntypedInt NL80211_SURVEY_INFO_TIME_BSS_RX => 0xb;
public static UntypedInt NL80211_SURVEY_INFO_TIME_BUSY => 0x5;
public static UntypedInt NL80211_SURVEY_INFO_TIME => 0x4;
public static UntypedInt NL80211_SURVEY_INFO_TIME_EXT_BUSY => 0x6;
public static UntypedInt NL80211_SURVEY_INFO_TIME_RX => 0x7;
public static UntypedInt NL80211_SURVEY_INFO_TIME_SCAN => 0x9;
public static UntypedInt NL80211_SURVEY_INFO_TIME_TX => 0x8;
public static UntypedInt NL80211_TDLS_DISABLE_LINK => 0x4;
public static UntypedInt NL80211_TDLS_DISCOVERY_REQ => 0x0;
public static UntypedInt NL80211_TDLS_ENABLE_LINK => 0x3;
public static UntypedInt NL80211_TDLS_PEER_HE => 0x8;
public static UntypedInt NL80211_TDLS_PEER_HT => 0x1;
public static UntypedInt NL80211_TDLS_PEER_VHT => 0x2;
public static UntypedInt NL80211_TDLS_PEER_WMM => 0x4;
public static UntypedInt NL80211_TDLS_SETUP => 0x1;
public static UntypedInt NL80211_TDLS_TEARDOWN => 0x2;
public static UntypedInt NL80211_TID_CONFIG_ATTR_AMPDU_CTRL => 0x9;
public static UntypedInt NL80211_TID_CONFIG_ATTR_AMSDU_CTRL => 0xb;
public static UntypedInt NL80211_TID_CONFIG_ATTR_MAX => 0xd;
public static UntypedInt NL80211_TID_CONFIG_ATTR_NOACK => 0x6;
public static UntypedInt NL80211_TID_CONFIG_ATTR_OVERRIDE => 0x4;
public static UntypedInt NL80211_TID_CONFIG_ATTR_PAD => 0x1;
public static UntypedInt NL80211_TID_CONFIG_ATTR_PEER_SUPP => 0x3;
public static UntypedInt NL80211_TID_CONFIG_ATTR_RETRY_LONG => 0x8;
public static UntypedInt NL80211_TID_CONFIG_ATTR_RETRY_SHORT => 0x7;
public static UntypedInt NL80211_TID_CONFIG_ATTR_RTSCTS_CTRL => 0xa;
public static UntypedInt NL80211_TID_CONFIG_ATTR_TIDS => 0x5;
public static UntypedInt NL80211_TID_CONFIG_ATTR_TX_RATE => 0xd;
public static UntypedInt NL80211_TID_CONFIG_ATTR_TX_RATE_TYPE => 0xc;
public static UntypedInt NL80211_TID_CONFIG_ATTR_VIF_SUPP => 0x2;
public static UntypedInt NL80211_TID_CONFIG_DISABLE => 0x1;
public static UntypedInt NL80211_TID_CONFIG_ENABLE => 0x0;
public static UntypedInt NL80211_TID_STATS_MAX => 0x6;
public static UntypedInt NL80211_TID_STATS_PAD => 0x5;
public static UntypedInt NL80211_TID_STATS_RX_MSDU => 0x1;
public static UntypedInt NL80211_TID_STATS_TX_MSDU => 0x2;
public static UntypedInt NL80211_TID_STATS_TX_MSDU_FAILED => 0x4;
public static UntypedInt NL80211_TID_STATS_TX_MSDU_RETRIES => 0x3;
public static UntypedInt NL80211_TID_STATS_TXQ_STATS => 0x6;
public static UntypedInt NL80211_TIMEOUT_ASSOC => 0x3;
public static UntypedInt NL80211_TIMEOUT_AUTH => 0x2;
public static UntypedInt NL80211_TIMEOUT_SCAN => 0x1;
public static UntypedInt NL80211_TIMEOUT_UNSPECIFIED => 0x0;
public static UntypedInt NL80211_TKIP_DATA_OFFSET_ENCR_KEY => 0x0;
public static UntypedInt NL80211_TKIP_DATA_OFFSET_RX_MIC_KEY => 0x18;
public static UntypedInt NL80211_TKIP_DATA_OFFSET_TX_MIC_KEY => 0x10;
public static UntypedInt NL80211_TX_POWER_AUTOMATIC => 0x0;
public static UntypedInt NL80211_TX_POWER_FIXED => 0x2;
public static UntypedInt NL80211_TX_POWER_LIMITED => 0x1;
public static UntypedInt NL80211_TXQ_ATTR_AC => 0x1;
public static UntypedInt NL80211_TXQ_ATTR_AIFS => 0x5;
public static UntypedInt NL80211_TXQ_ATTR_CWMAX => 0x4;
public static UntypedInt NL80211_TXQ_ATTR_CWMIN => 0x3;
public static UntypedInt NL80211_TXQ_ATTR_MAX => 0x5;
public static UntypedInt NL80211_TXQ_ATTR_QUEUE => 0x1;
public static UntypedInt NL80211_TXQ_ATTR_TXOP => 0x2;
public static UntypedInt NL80211_TXQ_Q_BE => 0x2;
public static UntypedInt NL80211_TXQ_Q_BK => 0x3;
public static UntypedInt NL80211_TXQ_Q_VI => 0x1;
public static UntypedInt NL80211_TXQ_Q_VO => 0x0;
public static UntypedInt NL80211_TXQ_STATS_BACKLOG_BYTES => 0x1;
public static UntypedInt NL80211_TXQ_STATS_BACKLOG_PACKETS => 0x2;
public static UntypedInt NL80211_TXQ_STATS_COLLISIONS => 0x8;
public static UntypedInt NL80211_TXQ_STATS_DROPS => 0x4;
public static UntypedInt NL80211_TXQ_STATS_ECN_MARKS => 0x5;
public static UntypedInt NL80211_TXQ_STATS_FLOWS => 0x3;
public static UntypedInt NL80211_TXQ_STATS_MAX => 0xb;
public static UntypedInt NL80211_TXQ_STATS_MAX_FLOWS => 0xb;
public static UntypedInt NL80211_TXQ_STATS_OVERLIMIT => 0x6;
public static UntypedInt NL80211_TXQ_STATS_OVERMEMORY => 0x7;
public static UntypedInt NL80211_TXQ_STATS_TX_BYTES => 0x9;
public static UntypedInt NL80211_TXQ_STATS_TX_PACKETS => 0xa;
public static UntypedInt NL80211_TX_RATE_AUTOMATIC => 0x0;
public static UntypedInt NL80211_TXRATE_DEFAULT_GI => 0x0;
public static UntypedInt NL80211_TX_RATE_FIXED => 0x2;
public static UntypedInt NL80211_TXRATE_FORCE_LGI => 0x2;
public static UntypedInt NL80211_TXRATE_FORCE_SGI => 0x1;
public static UntypedInt NL80211_TXRATE_GI => 0x4;
public static UntypedInt NL80211_TXRATE_HE => 0x5;
public static UntypedInt NL80211_TXRATE_HE_GI => 0x6;
public static UntypedInt NL80211_TXRATE_HE_LTF => 0x7;
public static UntypedInt NL80211_TXRATE_HT => 0x2;
public static UntypedInt NL80211_TXRATE_LEGACY => 0x1;
public static UntypedInt NL80211_TX_RATE_LIMITED => 0x1;
public static UntypedInt NL80211_TXRATE_MAX => 0xa;
public static UntypedInt NL80211_TXRATE_MCS => 0x2;
public static UntypedInt NL80211_TXRATE_VHT => 0x3;
public static UntypedInt NL80211_UNSOL_BCAST_PROBE_RESP_ATTR_INT => 0x1;
public static UntypedInt NL80211_UNSOL_BCAST_PROBE_RESP_ATTR_MAX => 0x2;
public static UntypedInt NL80211_UNSOL_BCAST_PROBE_RESP_ATTR_TMPL => 0x2;
public static UntypedInt NL80211_USER_REG_HINT_CELL_BASE => 0x1;
public static UntypedInt NL80211_USER_REG_HINT_INDOOR => 0x2;
public static UntypedInt NL80211_USER_REG_HINT_USER => 0x0;
public static UntypedInt NL80211_VENDOR_ID_IS_LINUX => 0x80000000;
public static UntypedInt NL80211_VHT_CAPABILITY_LEN => 0xc;
public static UntypedInt NL80211_VHT_NSS_MAX => 0x8;
public static UntypedInt NL80211_WIPHY_NAME_MAXLEN => 0x40;
public static UntypedInt NL80211_WIPHY_RADIO_ATTR_FREQ_RANGE => 0x2;
public static UntypedInt NL80211_WIPHY_RADIO_ATTR_INDEX => 0x1;
public static UntypedInt NL80211_WIPHY_RADIO_ATTR_INTERFACE_COMBINATION => 0x3;
public static UntypedInt NL80211_WIPHY_RADIO_ATTR_MAX => 0x5;
public static UntypedInt NL80211_WIPHY_RADIO_FREQ_ATTR_END => 0x2;
public static UntypedInt NL80211_WIPHY_RADIO_FREQ_ATTR_MAX => 0x2;
public static UntypedInt NL80211_WIPHY_RADIO_FREQ_ATTR_START => 0x1;
public static UntypedInt NL80211_WMMR_AIFSN => 0x3;
public static UntypedInt NL80211_WMMR_CW_MAX => 0x2;
public static UntypedInt NL80211_WMMR_CW_MIN => 0x1;
public static UntypedInt NL80211_WMMR_MAX => 0x4;
public static UntypedInt NL80211_WMMR_TXOP => 0x4;
public static UntypedInt NL80211_WOWLAN_PKTPAT_MASK => 0x1;
public static UntypedInt NL80211_WOWLAN_PKTPAT_OFFSET => 0x3;
public static UntypedInt NL80211_WOWLAN_PKTPAT_PATTERN => 0x2;
public static UntypedInt NL80211_WOWLAN_TCP_DATA_INTERVAL => 0x9;
public static UntypedInt NL80211_WOWLAN_TCP_DATA_PAYLOAD => 0x6;
public static UntypedInt NL80211_WOWLAN_TCP_DATA_PAYLOAD_SEQ => 0x7;
public static UntypedInt NL80211_WOWLAN_TCP_DATA_PAYLOAD_TOKEN => 0x8;
public static UntypedInt NL80211_WOWLAN_TCP_DST_IPV4 => 0x2;
public static UntypedInt NL80211_WOWLAN_TCP_DST_MAC => 0x3;
public static UntypedInt NL80211_WOWLAN_TCP_DST_PORT => 0x5;
public static UntypedInt NL80211_WOWLAN_TCP_SRC_IPV4 => 0x1;
public static UntypedInt NL80211_WOWLAN_TCP_SRC_PORT => 0x4;
public static UntypedInt NL80211_WOWLAN_TCP_WAKE_MASK => 0xb;
public static UntypedInt NL80211_WOWLAN_TCP_WAKE_PAYLOAD => 0xa;
public static UntypedInt NL80211_WOWLAN_TRIG_4WAY_HANDSHAKE => 0x8;
public static UntypedInt NL80211_WOWLAN_TRIG_ANY => 0x1;
public static UntypedInt NL80211_WOWLAN_TRIG_DISCONNECT => 0x2;
public static UntypedInt NL80211_WOWLAN_TRIG_EAP_IDENT_REQUEST => 0x7;
public static UntypedInt NL80211_WOWLAN_TRIG_GTK_REKEY_FAILURE => 0x6;
public static UntypedInt NL80211_WOWLAN_TRIG_GTK_REKEY_SUPPORTED => 0x5;
public static UntypedInt NL80211_WOWLAN_TRIG_MAGIC_PKT => 0x3;
public static UntypedInt NL80211_WOWLAN_TRIG_NET_DETECT => 0x12;
public static UntypedInt NL80211_WOWLAN_TRIG_NET_DETECT_RESULTS => 0x13;
public static UntypedInt NL80211_WOWLAN_TRIG_PKT_PATTERN => 0x4;
public static UntypedInt NL80211_WOWLAN_TRIG_RFKILL_RELEASE => 0x9;
public static UntypedInt NL80211_WOWLAN_TRIG_TCP_CONNECTION => 0xe;
public static UntypedInt NL80211_WOWLAN_TRIG_UNPROTECTED_DEAUTH_DISASSOC => 0x14;
public static UntypedInt NL80211_WOWLAN_TRIG_WAKEUP_PKT_80211 => 0xa;
public static UntypedInt NL80211_WOWLAN_TRIG_WAKEUP_PKT_80211_LEN => 0xb;
public static UntypedInt NL80211_WOWLAN_TRIG_WAKEUP_PKT_8023 => 0xc;
public static UntypedInt NL80211_WOWLAN_TRIG_WAKEUP_PKT_8023_LEN => 0xd;
public static UntypedInt NL80211_WOWLAN_TRIG_WAKEUP_TCP_CONNLOST => 0x10;
public static UntypedInt NL80211_WOWLAN_TRIG_WAKEUP_TCP_MATCH => 0xf;
public static UntypedInt NL80211_WOWLAN_TRIG_WAKEUP_TCP_NOMORETOKENS => 0x11;
public static UntypedInt NL80211_WPA_VERSION_1 => 0x1;
public static UntypedInt NL80211_WPA_VERSION_2 => 0x2;
public static UntypedInt NL80211_WPA_VERSION_3 => 0x4;

public static UntypedInt FRA_UNSPEC => 0x0;
public static UntypedInt FRA_DST => 0x1;
public static UntypedInt FRA_SRC => 0x2;
public static UntypedInt FRA_IIFNAME => 0x3;
public static UntypedInt FRA_GOTO => 0x4;
public static UntypedInt FRA_UNUSED2 => 0x5;
public static UntypedInt FRA_PRIORITY => 0x6;
public static UntypedInt FRA_UNUSED3 => 0x7;
public static UntypedInt FRA_UNUSED4 => 0x8;
public static UntypedInt FRA_UNUSED5 => 0x9;
public static UntypedInt FRA_FWMARK => 0xa;
public static UntypedInt FRA_FLOW => 0xb;
public static UntypedInt FRA_TUN_ID => 0xc;
public static UntypedInt FRA_SUPPRESS_IFGROUP => 0xd;
public static UntypedInt FRA_SUPPRESS_PREFIXLEN => 0xe;
public static UntypedInt FRA_TABLE => 0xf;
public static UntypedInt FRA_FWMASK => 0x10;
public static UntypedInt FRA_OIFNAME => 0x11;
public static UntypedInt FRA_PAD => 0x12;
public static UntypedInt FRA_L3MDEV => 0x13;
public static UntypedInt FRA_UID_RANGE => 0x14;
public static UntypedInt FRA_PROTOCOL => 0x15;
public static UntypedInt FRA_IP_PROTO => 0x16;
public static UntypedInt FRA_SPORT_RANGE => 0x17;
public static UntypedInt FRA_DPORT_RANGE => 0x18;
public static UntypedInt FR_ACT_UNSPEC => 0x0;
public static UntypedInt FR_ACT_TO_TBL => 0x1;
public static UntypedInt FR_ACT_GOTO => 0x2;
public static UntypedInt FR_ACT_NOP => 0x3;
public static UntypedInt FR_ACT_RES3 => 0x4;
public static UntypedInt FR_ACT_RES4 => 0x5;
public static UntypedInt FR_ACT_BLACKHOLE => 0x6;
public static UntypedInt FR_ACT_UNREACHABLE => 0x7;
public static UntypedInt FR_ACT_PROHIBIT => 0x8;

public static UntypedInt AUDIT_NLGRP_NONE => 0x0;
public static UntypedInt AUDIT_NLGRP_READLOG => 0x1;

public static UntypedInt TUN_F_CSUM => 0x1;
public static UntypedInt TUN_F_TSO4 => 0x2;
public static UntypedInt TUN_F_TSO6 => 0x4;
public static UntypedInt TUN_F_TSO_ECN => 0x8;
public static UntypedInt TUN_F_UFO => 0x10;
public static UntypedInt TUN_F_USO4 => 0x20;
public static UntypedInt TUN_F_USO6 => 0x40;

public static UntypedInt VIRTIO_NET_HDR_F_NEEDS_CSUM => 0x1;
public static UntypedInt VIRTIO_NET_HDR_F_DATA_VALID => 0x2;
public static UntypedInt VIRTIO_NET_HDR_F_RSC_INFO => 0x4;

public static UntypedInt VIRTIO_NET_HDR_GSO_NONE => 0x0;
public static UntypedInt VIRTIO_NET_HDR_GSO_TCPV4 => 0x1;
public static UntypedInt VIRTIO_NET_HDR_GSO_UDP => 0x3;
public static UntypedInt VIRTIO_NET_HDR_GSO_TCPV6 => 0x4;
public static UntypedInt VIRTIO_NET_HDR_GSO_UDP_L4 => 0x5;
public static UntypedInt VIRTIO_NET_HDR_GSO_ECN => 0x80;

partial struct SchedAttr {
    public uint32 Size;
    public uint32 Policy;
    public uint64 Flags;
    public int32 Nice;
    public uint32 Priority;
    public uint64 Runtime;
    public uint64 Deadline;
    public uint64 Period;
    public uint32 Util_min;
    public uint32 Util_max;
}

public static UntypedInt SizeofSchedAttr => 0x38;

partial struct Cachestat_t {
    public uint64 Cache;
    public uint64 Dirty;
    public uint64 Writeback;
    public uint64 Evicted;
    public uint64 Recently_evicted;
}
partial struct CachestatRange {
    public uint64 Off;
    public uint64 Len;
}

public static UntypedInt SK_MEMINFO_RMEM_ALLOC => 0x0;
public static UntypedInt SK_MEMINFO_RCVBUF => 0x1;
public static UntypedInt SK_MEMINFO_WMEM_ALLOC => 0x2;
public static UntypedInt SK_MEMINFO_SNDBUF => 0x3;
public static UntypedInt SK_MEMINFO_FWD_ALLOC => 0x4;
public static UntypedInt SK_MEMINFO_WMEM_QUEUED => 0x5;
public static UntypedInt SK_MEMINFO_OPTMEM => 0x6;
public static UntypedInt SK_MEMINFO_BACKLOG => 0x7;
public static UntypedInt SK_MEMINFO_DROPS => 0x8;
public static UntypedInt SK_MEMINFO_VARS => 0x9;
public static UntypedInt SKNLGRP_NONE => 0x0;
public static UntypedInt SKNLGRP_INET_TCP_DESTROY => 0x1;
public static UntypedInt SKNLGRP_INET_UDP_DESTROY => 0x2;
public static UntypedInt SKNLGRP_INET6_TCP_DESTROY => 0x3;
public static UntypedInt SKNLGRP_INET6_UDP_DESTROY => 0x4;
public static UntypedInt SK_DIAG_BPF_STORAGE_REQ_NONE => 0x0;
public static UntypedInt SK_DIAG_BPF_STORAGE_REQ_MAP_FD => 0x1;
public static UntypedInt SK_DIAG_BPF_STORAGE_REP_NONE => 0x0;
public static UntypedInt SK_DIAG_BPF_STORAGE => 0x1;
public static UntypedInt SK_DIAG_BPF_STORAGE_NONE => 0x0;
public static UntypedInt SK_DIAG_BPF_STORAGE_PAD => 0x1;
public static UntypedInt SK_DIAG_BPF_STORAGE_MAP_ID => 0x2;
public static UntypedInt SK_DIAG_BPF_STORAGE_MAP_VALUE => 0x3;

partial struct SockDiagReq {
    public uint8 Family;
    public uint8 Protocol;
}

public static UntypedInt RTM_NEWNVLAN => 0x70;

public static UntypedInt MPOL_BIND => 0x2;
public static UntypedInt MPOL_DEFAULT => 0x0;
public static UntypedInt MPOL_F_ADDR => 0x2;
public static UntypedInt MPOL_F_MEMS_ALLOWED => 0x4;
public static UntypedInt MPOL_F_MOF => 0x8;
public static UntypedInt MPOL_F_MORON => 0x10;
public static UntypedInt MPOL_F_NODE => 0x1;
public static UntypedInt MPOL_F_NUMA_BALANCING => 0x2000;
public static UntypedInt MPOL_F_RELATIVE_NODES => 0x4000;
public static UntypedInt MPOL_F_SHARED => 0x1;
public static UntypedInt MPOL_F_STATIC_NODES => 0x8000;
public static UntypedInt MPOL_INTERLEAVE => 0x3;
public static UntypedInt MPOL_LOCAL => 0x4;
public static UntypedInt MPOL_MAX => 0x7;
public static UntypedInt MPOL_MF_INTERNAL => 0x10;
public static UntypedInt MPOL_MF_LAZY => 0x8;
public static UntypedInt MPOL_MF_MOVE_ALL => 0x4;
public static UntypedInt MPOL_MF_MOVE => 0x2;
public static UntypedInt MPOL_MF_STRICT => 0x1;
public static UntypedInt MPOL_MF_VALID => 0x7;
public static UntypedInt MPOL_MODE_FLAGS => 0xe000;
public static UntypedInt MPOL_PREFERRED => 0x1;
public static UntypedInt MPOL_PREFERRED_MANY => 0x5;
public static UntypedInt MPOL_WEIGHTED_INTERLEAVE => 0x6;

public static UntypedInt GPIO_V2_GET_LINEINFO_IOCTL => 0xc100b405;
public static UntypedInt GPIO_V2_GET_LINE_IOCTL => 0xc250b407;
public static UntypedInt GPIO_V2_LINE_GET_VALUES_IOCTL => 0xc010b40e;
public static UntypedInt GPIO_V2_LINE_SET_VALUES_IOCTL => 0xc010b40f;
public static UntypedInt GPIO_V2_GET_LINEINFO_WATCH_IOCTL => 0xc100b406;
public static UntypedInt GPIO_GET_LINEINFO_UNWATCH_IOCTL => 0xc004b40c;
public static UntypedInt GPIO_V2_LINE_ATTR_ID_FLAGS => 0x1;
public static UntypedInt GPIO_V2_LINE_ATTR_ID_OUTPUT_VALUES => 0x2;
public static UntypedInt GPIO_V2_LINE_ATTR_ID_DEBOUNCE => 0x3;
public static UntypedInt GPIO_V2_LINE_CHANGED_REQUESTED => 0x1;
public static UntypedInt GPIO_V2_LINE_CHANGED_RELEASED => 0x2;
public static UntypedInt GPIO_V2_LINE_CHANGED_CONFIG => 0x3;
public static UntypedInt GPIO_V2_LINE_EVENT_RISING_EDGE => 0x1;
public static UntypedInt GPIO_V2_LINE_EVENT_FALLING_EDGE => 0x2;

partial struct GPIOChipInfo {
    public array<byte> Name = new(32);
    public array<byte> Label = new(32);
    public uint32 Lines;
}
partial struct GPIOV2LineValues {
    public uint64 Bits;
    public uint64 Mask;
}
partial struct GPIOV2LineAttribute {
    public uint32 Id;
    internal uint32 _;
    public uint64 Flags;
}
partial struct GPIOV2LineConfigAttribute {
    public GPIOV2LineAttribute Attr;
    public uint64 Mask;
}
partial struct GPIOV2LineConfig {
    public uint64 Flags;
    public uint32 Num_attrs;
    internal array<uint32> _ = new(5);
    public array<GPIOV2LineConfigAttribute> Attrs = new(10);
}
partial struct GPIOV2LineRequest {
    public array<uint32> Offsets = new(64);
    public array<byte> Consumer = new(32);
    public GPIOV2LineConfig Config;
    public uint32 Num_lines;
    public uint32 Event_buffer_size;
    internal array<uint32> _ = new(5);
    public int32 Fd;
}
partial struct GPIOV2LineInfo {
    public array<byte> Name = new(32);
    public array<byte> Consumer = new(32);
    public uint32 Offset;
    public uint32 Num_attrs;
    public uint64 Flags;
    public array<GPIOV2LineAttribute> Attrs = new(10);
    internal array<uint32> _ = new(4);
}
partial struct GPIOV2LineInfoChanged {
    public GPIOV2LineInfo Info;
    public uint64 Timestamp_ns;
    public uint32 Event_type;
    internal array<uint32> _ = new(5);
}
partial struct GPIOV2LineEvent {
    public uint64 Timestamp_ns;
    public uint32 Id;
    public uint32 Offset;
    public uint32 Seqno;
    public uint32 Line_seqno;
    internal array<uint32> _ = new(6);
}

public static UntypedInt IPMI_MAX_ADDR_SIZE => 0x20;
public static UntypedInt IPMI_SYSTEM_INTERFACE_ADDR_TYPE => 0xc;
public static UntypedInt IPMI_BMC_CHANNEL => 0xf;
public static UntypedInt IPMI_RESPONSE_RECV_TYPE => 0x1;

partial struct IPMISystemInterfaceAddr {
    public int32 Type;
    public int16 Channel;
    public uint8 Lun;
    internal array<byte> _ = new(1);
}
partial struct IPMIMsg {
    public uint8 Netfn;
    public uint8 Cmd;
    public uint16 Len;
    public ж<uint8> Data;
}

} // end unix_package
