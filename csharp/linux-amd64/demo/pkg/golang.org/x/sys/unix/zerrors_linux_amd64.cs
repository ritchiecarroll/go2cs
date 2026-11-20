namespace go.golang.org.x.sys;

using syscall = syscall_package;

partial class unix_package {

public static UntypedInt B1000000 => 0x1008;
public static UntypedInt B115200 => 0x1002;
public static UntypedInt B1152000 => 0x1009;
public static UntypedInt B1500000 => 0x100a;
public static UntypedInt B2000000 => 0x100b;
public static UntypedInt B230400 => 0x1003;
public static UntypedInt B2500000 => 0x100c;
public static UntypedInt B3000000 => 0x100d;
public static UntypedInt B3500000 => 0x100e;
public static UntypedInt B4000000 => 0x100f;
public static UntypedInt B460800 => 0x1004;
public static UntypedInt B500000 => 0x1005;
public static UntypedInt B57600 => 0x1001;
public static UntypedInt B576000 => 0x1006;
public static UntypedInt B921600 => 0x1007;
public static UntypedInt BLKALIGNOFF => 0x127a;
public static UntypedInt BLKBSZGET => 0x80081270;
public static UntypedInt BLKBSZSET => 0x40081271;
public static UntypedInt BLKDISCARD => 0x1277;
public static UntypedInt BLKDISCARDZEROES => 0x127c;
public static UntypedInt BLKFLSBUF => 0x1261;
public static UntypedInt BLKFRAGET => 0x1265;
public static UntypedInt BLKFRASET => 0x1264;
public static UntypedInt BLKGETDISKSEQ => 0x80081280;
public static UntypedInt BLKGETSIZE => 0x1260;
public static UntypedInt BLKGETSIZE64 => 0x80081272;
public static UntypedInt BLKIOMIN => 0x1278;
public static UntypedInt BLKIOOPT => 0x1279;
public static UntypedInt BLKPBSZGET => 0x127b;
public static UntypedInt BLKRAGET => 0x1263;
public static UntypedInt BLKRASET => 0x1262;
public static UntypedInt BLKROGET => 0x125e;
public static UntypedInt BLKROSET => 0x125d;
public static UntypedInt BLKROTATIONAL => 0x127e;
public static UntypedInt BLKRRPART => 0x125f;
public static UntypedInt BLKSECDISCARD => 0x127d;
public static UntypedInt BLKSECTGET => 0x1267;
public static UntypedInt BLKSECTSET => 0x1266;
public static UntypedInt BLKSSZGET => 0x1268;
public static UntypedInt BLKZEROOUT => 0x127f;
public static UntypedInt BOTHER => 0x1000;
public static UntypedInt BS1 => 0x2000;
public static UntypedInt BSDLY => 0x2000;
public static UntypedInt CBAUD => 0x100f;
public static UntypedInt CBAUDEX => 0x1000;
public static UntypedInt CIBAUD => 0x100f0000;
public static UntypedInt CLOCAL => 0x800;
public static UntypedInt CR1 => 0x200;
public static UntypedInt CR2 => 0x400;
public static UntypedInt CR3 => 0x600;
public static UntypedInt CRDLY => 0x600;
public static UntypedInt CREAD => 0x80;
public static UntypedInt CS6 => 0x10;
public static UntypedInt CS7 => 0x20;
public static UntypedInt CS8 => 0x30;
public static UntypedInt CSIZE => 0x30;
public static UntypedInt CSTOPB => 0x40;
public static UntypedInt DM_MPATH_PROBE_PATHS => 0xfd12;
public static UntypedInt ECCGETLAYOUT => 0x81484d11;
public static UntypedInt ECCGETSTATS => 0x80104d12;
public static UntypedInt ECHOCTL => 0x200;
public static UntypedInt ECHOE => 0x10;
public static UntypedInt ECHOK => 0x20;
public static UntypedInt ECHOKE => 0x800;
public static UntypedInt ECHONL => 0x40;
public static UntypedInt ECHOPRT => 0x400;
public static UntypedInt EFD_CLOEXEC => 0x80000;
public static UntypedInt EFD_NONBLOCK => 0x800;
public static UntypedInt EPIOCGPARAMS => 0x80088a02;
public static UntypedInt EPIOCSPARAMS => 0x40088a01;
public static UntypedInt EPOLL_CLOEXEC => 0x80000;
public static UntypedInt EXTPROC => 0x10000;
public static UntypedInt FF1 => 0x8000;
public static UntypedInt FFDLY => 0x8000;
public static UntypedInt FICLONE => 0x40049409;
public static UntypedInt FICLONERANGE => 0x4020940d;
public static UntypedInt FLUSHO => 0x1000;
public static UntypedInt FP_XSTATE_MAGIC2 => 0x46505845;
public static UntypedInt FS_IOC_ENABLE_VERITY => 0x40806685;
public static UntypedInt FS_IOC_GETFLAGS => 0x80086601;
public static UntypedInt FS_IOC_GET_ENCRYPTION_NONCE => 0x8010661b;
public static UntypedInt FS_IOC_GET_ENCRYPTION_POLICY => 0x400c6615;
public static UntypedInt FS_IOC_GET_ENCRYPTION_PWSALT => 0x40106614;
public static UntypedInt FS_IOC_SETFLAGS => 0x40086602;
public static UntypedInt FS_IOC_SET_ENCRYPTION_POLICY => 0x800c6613;
public static UntypedInt F_GETLK => 0x5;
public static UntypedInt F_GETLK64 => 0x5;
public static UntypedInt F_GETOWN => 0x9;
public static UntypedInt F_RDLCK => 0x0;
public static UntypedInt F_SETLK => 0x6;
public static UntypedInt F_SETLK64 => 0x6;
public static UntypedInt F_SETLKW => 0x7;
public static UntypedInt F_SETLKW64 => 0x7;
public static UntypedInt F_SETOWN => 0x8;
public static UntypedInt F_UNLCK => 0x2;
public static UntypedInt F_WRLCK => 0x1;
public static UntypedInt HIDIOCGRAWINFO => 0x80084803;
public static UntypedInt HIDIOCGRDESC => 0x90044802;
public static UntypedInt HIDIOCGRDESCSIZE => 0x80044801;
public static UntypedInt HIDIOCREVOKE => 0x4004480d;
public static UntypedInt HUPCL => 0x400;
public static UntypedInt ICANON => 0x2;
public static UntypedInt IEXTEN => 0x8000;
public static UntypedInt IN_CLOEXEC => 0x80000;
public static UntypedInt IN_NONBLOCK => 0x800;
public static UntypedInt IOCTL_MEI_NOTIFY_GET => 0x80044803;
public static UntypedInt IOCTL_MEI_NOTIFY_SET => 0x40044802;
public static UntypedInt IOCTL_VM_SOCKETS_GET_LOCAL_CID => 0x7b9;
public static UntypedInt IPV6_FLOWINFO_MASK => 0xffffff0f;
public static UntypedInt IPV6_FLOWLABEL_MASK => 0xffff0f00;
public static UntypedInt ISIG => 0x1;
public static UntypedInt IUCLC => 0x200;
public static UntypedInt IXOFF => 0x1000;
public static UntypedInt IXON => 0x400;
public static UntypedInt MAP_32BIT => 0x40;
public static UntypedInt MAP_ABOVE4G => 0x80;
public static UntypedInt MAP_ANON => 0x20;
public static UntypedInt MAP_ANONYMOUS => 0x20;
public static UntypedInt MAP_DENYWRITE => 0x800;
public static UntypedInt MAP_EXECUTABLE => 0x1000;
public static UntypedInt MAP_GROWSDOWN => 0x100;
public static UntypedInt MAP_HUGETLB => 0x40000;
public static UntypedInt MAP_LOCKED => 0x2000;
public static UntypedInt MAP_NONBLOCK => 0x10000;
public static UntypedInt MAP_NORESERVE => 0x4000;
public static UntypedInt MAP_POPULATE => 0x8000;
public static UntypedInt MAP_STACK => 0x20000;
public static UntypedInt MAP_SYNC => 0x80000;
public static UntypedInt MCL_CURRENT => 0x1;
public static UntypedInt MCL_FUTURE => 0x2;
public static UntypedInt MCL_ONFAULT => 0x4;
public static UntypedInt MEMERASE => 0x40084d02;
public static UntypedInt MEMERASE64 => 0x40104d14;
public static UntypedInt MEMGETBADBLOCK => 0x40084d0b;
public static UntypedInt MEMGETINFO => 0x80204d01;
public static UntypedInt MEMGETOOBSEL => 0x80c84d0a;
public static UntypedInt MEMGETREGIONCOUNT => 0x80044d07;
public static UntypedInt MEMISLOCKED => 0x80084d17;
public static UntypedInt MEMLOCK => 0x40084d05;
public static UntypedInt MEMREAD => 0xc0404d1a;
public static UntypedInt MEMREADOOB => 0xc0104d04;
public static UntypedInt MEMSETBADBLOCK => 0x40084d0c;
public static UntypedInt MEMUNLOCK => 0x40084d06;
public static UntypedInt MEMWRITEOOB => 0xc0104d03;
public static UntypedInt MTDFILEMODE => 0x4d13;
public static UntypedInt NFDBITS => 0x40;
public static UntypedInt NLDLY => 0x100;
public static UntypedInt NOFLSH => 0x80;
public static UntypedInt NS_GET_ID => 0x8008b70d;
public static UntypedInt NS_GET_MNTNS_ID => 0x8008b705;
public static UntypedInt NS_GET_NSTYPE => 0xb703;
public static UntypedInt NS_GET_OWNER_UID => 0xb704;
public static UntypedInt NS_GET_PARENT => 0xb702;
public static UntypedInt NS_GET_PID_FROM_PIDNS => 0x8004b706;
public static UntypedInt NS_GET_PID_IN_PIDNS => 0x8004b708;
public static UntypedInt NS_GET_TGID_FROM_PIDNS => 0x8004b707;
public static UntypedInt NS_GET_TGID_IN_PIDNS => 0x8004b709;
public static UntypedInt NS_GET_USERNS => 0xb701;
public static UntypedInt OLCUC => 0x2;
public static UntypedInt ONLCR => 0x4;
public static UntypedInt OTPERASE => 0x400c4d19;
public static UntypedInt OTPGETREGIONCOUNT => 0x40044d0e;
public static UntypedInt OTPGETREGIONINFO => 0x400c4d0f;
public static UntypedInt OTPLOCK => 0x800c4d10;
public static UntypedInt OTPSELECT => 0x80044d0d;
public static UntypedInt O_APPEND => 0x400;
public static UntypedInt O_ASYNC => 0x2000;
public static UntypedInt O_CLOEXEC => 0x80000;
public static UntypedInt O_CREAT => 0x40;
public static UntypedInt O_DIRECT => 0x4000;
public static UntypedInt O_DIRECTORY => 0x10000;
public static UntypedInt O_DSYNC => 0x1000;
public static UntypedInt O_EXCL => 0x80;
public static UntypedInt O_FSYNC => 0x101000;
public static UntypedInt O_LARGEFILE => 0x0;
public static UntypedInt O_NDELAY => 0x800;
public static UntypedInt O_NOATIME => 0x40000;
public static UntypedInt O_NOCTTY => 0x100;
public static UntypedInt O_NOFOLLOW => 0x20000;
public static UntypedInt O_NONBLOCK => 0x800;
public static UntypedInt O_PATH => 0x200000;
public static UntypedInt O_RSYNC => 0x101000;
public static UntypedInt O_SYNC => 0x101000;
public static UntypedInt O_TMPFILE => 0x410000;
public static UntypedInt O_TRUNC => 0x200;
public static UntypedInt PARENB => 0x100;
public static UntypedInt PARODD => 0x200;
public static UntypedInt PENDIN => 0x4000;
public static UntypedInt PERF_EVENT_IOC_DISABLE => 0x2401;
public static UntypedInt PERF_EVENT_IOC_ENABLE => 0x2400;
public static UntypedInt PERF_EVENT_IOC_ID => 0x80082407;
public static UntypedInt PERF_EVENT_IOC_MODIFY_ATTRIBUTES => 0x4008240b;
public static UntypedInt PERF_EVENT_IOC_PAUSE_OUTPUT => 0x40042409;
public static UntypedInt PERF_EVENT_IOC_PERIOD => 0x40082404;
public static UntypedInt PERF_EVENT_IOC_QUERY_BPF => 0xc008240a;
public static UntypedInt PERF_EVENT_IOC_REFRESH => 0x2402;
public static UntypedInt PERF_EVENT_IOC_RESET => 0x2403;
public static UntypedInt PERF_EVENT_IOC_SET_BPF => 0x40042408;
public static UntypedInt PERF_EVENT_IOC_SET_FILTER => 0x40082406;
public static UntypedInt PERF_EVENT_IOC_SET_OUTPUT => 0x2405;
public static UntypedInt PPPIOCATTACH => 0x4004743d;
public static UntypedInt PPPIOCATTCHAN => 0x40047438;
public static UntypedInt PPPIOCBRIDGECHAN => 0x40047435;
public static UntypedInt PPPIOCCONNECT => 0x4004743a;
public static UntypedInt PPPIOCDETACH => 0x4004743c;
public static UntypedInt PPPIOCDISCONN => 0x7439;
public static UntypedInt PPPIOCGASYNCMAP => 0x80047458;
public static UntypedInt PPPIOCGCHAN => 0x80047437;
public static UntypedInt PPPIOCGDEBUG => 0x80047441;
public static UntypedInt PPPIOCGFLAGS => 0x8004745a;
public static UntypedInt PPPIOCGIDLE => 0x8010743f;
public static UntypedInt PPPIOCGIDLE32 => 0x8008743f;
public static UntypedInt PPPIOCGIDLE64 => 0x8010743f;
public static UntypedInt PPPIOCGL2TPSTATS => 0x80487436;
public static UntypedInt PPPIOCGMRU => 0x80047453;
public static UntypedInt PPPIOCGRASYNCMAP => 0x80047455;
public static UntypedInt PPPIOCGUNIT => 0x80047456;
public static UntypedInt PPPIOCGXASYNCMAP => 0x80207450;
public static UntypedInt PPPIOCSACTIVE => 0x40107446;
public static UntypedInt PPPIOCSASYNCMAP => 0x40047457;
public static UntypedInt PPPIOCSCOMPRESS => 0x4010744d;
public static UntypedInt PPPIOCSDEBUG => 0x40047440;
public static UntypedInt PPPIOCSFLAGS => 0x40047459;
public static UntypedInt PPPIOCSMAXCID => 0x40047451;
public static UntypedInt PPPIOCSMRRU => 0x4004743b;
public static UntypedInt PPPIOCSMRU => 0x40047452;
public static UntypedInt PPPIOCSNPMODE => 0x4008744b;
public static UntypedInt PPPIOCSPASS => 0x40107447;
public static UntypedInt PPPIOCSRASYNCMAP => 0x40047454;
public static UntypedInt PPPIOCSXASYNCMAP => 0x4020744f;
public static UntypedInt PPPIOCUNBRIDGECHAN => 0x7434;
public static UntypedInt PPPIOCXFERUNIT => 0x744e;
public static UntypedInt PR_SET_PTRACER_ANY => 0xffffffffffffffff;
public static UntypedInt PTP_CLOCK_GETCAPS => 0x80503d01;
public static UntypedInt PTP_CLOCK_GETCAPS2 => 0x80503d0a;
public static UntypedInt PTP_ENABLE_PPS => 0x40043d04;
public static UntypedInt PTP_ENABLE_PPS2 => 0x40043d0d;
public static UntypedInt PTP_EXTTS_REQUEST => 0x40103d02;
public static UntypedInt PTP_EXTTS_REQUEST2 => 0x40103d0b;
public static UntypedInt PTP_MASK_CLEAR_ALL => 0x3d13;
public static UntypedInt PTP_MASK_EN_SINGLE => 0x40043d14;
public static UntypedInt PTP_PEROUT_REQUEST => 0x40383d03;
public static UntypedInt PTP_PEROUT_REQUEST2 => 0x40383d0c;
public static UntypedInt PTP_PIN_SETFUNC => 0x40603d07;
public static UntypedInt PTP_PIN_SETFUNC2 => 0x40603d10;
public static UntypedInt PTP_SYS_OFFSET => 0x43403d05;
public static UntypedInt PTP_SYS_OFFSET2 => 0x43403d0e;
public static UntypedInt PTRACE_ARCH_PRCTL => 0x1e;
public static UntypedInt PTRACE_GETFPREGS => 0xe;
public static UntypedInt PTRACE_GETFPXREGS => 0x12;
public static UntypedInt PTRACE_GET_THREAD_AREA => 0x19;
public static UntypedInt PTRACE_OLDSETOPTIONS => 0x15;
public static UntypedInt PTRACE_SETFPREGS => 0xf;
public static UntypedInt PTRACE_SETFPXREGS => 0x13;
public static UntypedInt PTRACE_SET_THREAD_AREA => 0x1a;
public static UntypedInt PTRACE_SINGLEBLOCK => 0x21;
public static UntypedInt PTRACE_SYSEMU => 0x1f;
public static UntypedInt PTRACE_SYSEMU_SINGLESTEP => 0x20;
public static UntypedInt RLIMIT_AS => 0x9;
public static UntypedInt RLIMIT_MEMLOCK => 0x8;
public static UntypedInt RLIMIT_NOFILE => 0x7;
public static UntypedInt RLIMIT_NPROC => 0x6;
public static UntypedInt RLIMIT_RSS => 0x5;
public static UntypedInt RNDADDENTROPY => 0x40085203;
public static UntypedInt RNDADDTOENTCNT => 0x40045201;
public static UntypedInt RNDCLEARPOOL => 0x5206;
public static UntypedInt RNDGETENTCNT => 0x80045200;
public static UntypedInt RNDGETPOOL => 0x80085202;
public static UntypedInt RNDRESEEDCRNG => 0x5207;
public static UntypedInt RNDZAPENTCNT => 0x5204;
public static UntypedInt RTC_AIE_OFF => 0x7002;
public static UntypedInt RTC_AIE_ON => 0x7001;
public static UntypedInt RTC_ALM_READ => 0x80247008;
public static UntypedInt RTC_ALM_SET => 0x40247007;
public static UntypedInt RTC_EPOCH_READ => 0x8008700d;
public static UntypedInt RTC_EPOCH_SET => 0x4008700e;
public static UntypedInt RTC_IRQP_READ => 0x8008700b;
public static UntypedInt RTC_IRQP_SET => 0x4008700c;
public static UntypedInt RTC_PARAM_GET => 0x40187013;
public static UntypedInt RTC_PARAM_SET => 0x40187014;
public static UntypedInt RTC_PIE_OFF => 0x7006;
public static UntypedInt RTC_PIE_ON => 0x7005;
public static UntypedInt RTC_PLL_GET => 0x80207011;
public static UntypedInt RTC_PLL_SET => 0x40207012;
public static UntypedInt RTC_RD_TIME => 0x80247009;
public static UntypedInt RTC_SET_TIME => 0x4024700a;
public static UntypedInt RTC_UIE_OFF => 0x7004;
public static UntypedInt RTC_UIE_ON => 0x7003;
public static UntypedInt RTC_VL_CLR => 0x7014;
public static UntypedInt RTC_VL_READ => 0x80047013;
public static UntypedInt RTC_WIE_OFF => 0x7010;
public static UntypedInt RTC_WIE_ON => 0x700f;
public static UntypedInt RTC_WKALM_RD => 0x80287010;
public static UntypedInt RTC_WKALM_SET => 0x4028700f;
public static UntypedInt SCM_DEVMEM_DMABUF => 0x4f;
public static UntypedInt SCM_DEVMEM_LINEAR => 0x4e;
public static UntypedInt SCM_INQ => 0x54;
public static UntypedInt SCM_TIMESTAMPING => 0x25;
public static UntypedInt SCM_TIMESTAMPING_OPT_STATS => 0x36;
public static UntypedInt SCM_TIMESTAMPING_PKTINFO => 0x3a;
public static UntypedInt SCM_TIMESTAMPNS => 0x23;
public static UntypedInt SCM_TS_OPT_ID => 0x51;
public static UntypedInt SCM_TXTIME => 0x3d;
public static UntypedInt SCM_WIFI_STATUS => 0x29;
public static UntypedInt SECCOMP_IOCTL_NOTIF_ADDFD => 0x40182103;
public static UntypedInt SECCOMP_IOCTL_NOTIF_ID_VALID => 0x40082102;
public static UntypedInt SECCOMP_IOCTL_NOTIF_SET_FLAGS => 0x40082104;
public static UntypedInt SFD_CLOEXEC => 0x80000;
public static UntypedInt SFD_NONBLOCK => 0x800;
public static UntypedInt SIOCATMARK => 0x8905;
public static UntypedInt SIOCGPGRP => 0x8904;
public static UntypedInt SIOCGSTAMPNS_NEW => 0x80108907;
public static UntypedInt SIOCGSTAMP_NEW => 0x80108906;
public static UntypedInt SIOCINQ => 0x541b;
public static UntypedInt SIOCOUTQ => 0x5411;
public static UntypedInt SIOCSPGRP => 0x8902;
public static UntypedInt SOCK_CLOEXEC => 0x80000;
public static UntypedInt SOCK_DGRAM => 0x2;
public static UntypedInt SOCK_NONBLOCK => 0x800;
public static UntypedInt SOCK_STREAM => 0x1;
public static UntypedInt SOL_SOCKET => 0x1;
public static UntypedInt SO_ACCEPTCONN => 0x1e;
public static UntypedInt SO_ATTACH_BPF => 0x32;
public static UntypedInt SO_ATTACH_REUSEPORT_CBPF => 0x33;
public static UntypedInt SO_ATTACH_REUSEPORT_EBPF => 0x34;
public static UntypedInt SO_BINDTODEVICE => 0x19;
public static UntypedInt SO_BINDTOIFINDEX => 0x3e;
public static UntypedInt SO_BPF_EXTENSIONS => 0x30;
public static UntypedInt SO_BROADCAST => 0x6;
public static UntypedInt SO_BSDCOMPAT => 0xe;
public static UntypedInt SO_BUF_LOCK => 0x48;
public static UntypedInt SO_BUSY_POLL => 0x2e;
public static UntypedInt SO_BUSY_POLL_BUDGET => 0x46;
public static UntypedInt SO_CNX_ADVICE => 0x35;
public static UntypedInt SO_COOKIE => 0x39;
public static UntypedInt SO_DETACH_REUSEPORT_BPF => 0x44;
public static UntypedInt SO_DEVMEM_DMABUF => 0x4f;
public static UntypedInt SO_DEVMEM_DONTNEED => 0x50;
public static UntypedInt SO_DEVMEM_LINEAR => 0x4e;
public static UntypedInt SO_DOMAIN => 0x27;
public static UntypedInt SO_DONTROUTE => 0x5;
public static UntypedInt SO_ERROR => 0x4;
public static UntypedInt SO_INCOMING_CPU => 0x31;
public static UntypedInt SO_INCOMING_NAPI_ID => 0x38;
public static UntypedInt SO_INQ => 0x54;
public static UntypedInt SO_KEEPALIVE => 0x9;
public static UntypedInt SO_LINGER => 0xd;
public static UntypedInt SO_LOCK_FILTER => 0x2c;
public static UntypedInt SO_MARK => 0x24;
public static UntypedInt SO_MAX_PACING_RATE => 0x2f;
public static UntypedInt SO_MEMINFO => 0x37;
public static UntypedInt SO_NETNS_COOKIE => 0x47;
public static UntypedInt SO_NOFCS => 0x2b;
public static UntypedInt SO_OOBINLINE => 0xa;
public static UntypedInt SO_PASSCRED => 0x10;
public static UntypedInt SO_PASSPIDFD => 0x4c;
public static UntypedInt SO_PASSRIGHTS => 0x53;
public static UntypedInt SO_PASSSEC => 0x22;
public static UntypedInt SO_PEEK_OFF => 0x2a;
public static UntypedInt SO_PEERCRED => 0x11;
public static UntypedInt SO_PEERGROUPS => 0x3b;
public static UntypedInt SO_PEERPIDFD => 0x4d;
public static UntypedInt SO_PEERSEC => 0x1f;
public static UntypedInt SO_PREFER_BUSY_POLL => 0x45;
public static UntypedInt SO_PROTOCOL => 0x26;
public static UntypedInt SO_RCVBUF => 0x8;
public static UntypedInt SO_RCVBUFFORCE => 0x21;
public static UntypedInt SO_RCVLOWAT => 0x12;
public static UntypedInt SO_RCVMARK => 0x4b;
public static UntypedInt SO_RCVPRIORITY => 0x52;
public static UntypedInt SO_RCVTIMEO => 0x14;
public static UntypedInt SO_RCVTIMEO_NEW => 0x42;
public static UntypedInt SO_RCVTIMEO_OLD => 0x14;
public static UntypedInt SO_RESERVE_MEM => 0x49;
public static UntypedInt SO_REUSEADDR => 0x2;
public static UntypedInt SO_REUSEPORT => 0xf;
public static UntypedInt SO_RXQ_OVFL => 0x28;
public static UntypedInt SO_SECURITY_AUTHENTICATION => 0x16;
public static UntypedInt SO_SECURITY_ENCRYPTION_NETWORK => 0x18;
public static UntypedInt SO_SECURITY_ENCRYPTION_TRANSPORT => 0x17;
public static UntypedInt SO_SELECT_ERR_QUEUE => 0x2d;
public static UntypedInt SO_SNDBUF => 0x7;
public static UntypedInt SO_SNDBUFFORCE => 0x20;
public static UntypedInt SO_SNDLOWAT => 0x13;
public static UntypedInt SO_SNDTIMEO => 0x15;
public static UntypedInt SO_SNDTIMEO_NEW => 0x43;
public static UntypedInt SO_SNDTIMEO_OLD => 0x15;
public static UntypedInt SO_TIMESTAMPING => 0x25;
public static UntypedInt SO_TIMESTAMPING_NEW => 0x41;
public static UntypedInt SO_TIMESTAMPING_OLD => 0x25;
public static UntypedInt SO_TIMESTAMPNS => 0x23;
public static UntypedInt SO_TIMESTAMPNS_NEW => 0x40;
public static UntypedInt SO_TIMESTAMPNS_OLD => 0x23;
public static UntypedInt SO_TIMESTAMP_NEW => 0x3f;
public static UntypedInt SO_TXREHASH => 0x4a;
public static UntypedInt SO_TXTIME => 0x3d;
public static UntypedInt SO_TYPE => 0x3;
public static UntypedInt SO_WIFI_STATUS => 0x29;
public static UntypedInt SO_ZEROCOPY => 0x3c;
public static UntypedInt TAB1 => 0x800;
public static UntypedInt TAB2 => 0x1000;
public static UntypedInt TAB3 => 0x1800;
public static UntypedInt TABDLY => 0x1800;
public static UntypedInt TCFLSH => 0x540b;
public static UntypedInt TCGETA => 0x5405;
public static UntypedInt TCGETS => 0x5401;
public static UntypedInt TCGETS2 => 0x802c542a;
public static UntypedInt TCGETX => 0x5432;
public static UntypedInt TCSAFLUSH => 0x2;
public static UntypedInt TCSBRK => 0x5409;
public static UntypedInt TCSBRKP => 0x5425;
public static UntypedInt TCSETA => 0x5406;
public static UntypedInt TCSETAF => 0x5408;
public static UntypedInt TCSETAW => 0x5407;
public static UntypedInt TCSETS => 0x5402;
public static UntypedInt TCSETS2 => 0x402c542b;
public static UntypedInt TCSETSF => 0x5404;
public static UntypedInt TCSETSF2 => 0x402c542d;
public static UntypedInt TCSETSW => 0x5403;
public static UntypedInt TCSETSW2 => 0x402c542c;
public static UntypedInt TCSETX => 0x5433;
public static UntypedInt TCSETXF => 0x5434;
public static UntypedInt TCSETXW => 0x5435;
public static UntypedInt TCXONC => 0x540a;
public static UntypedInt TFD_CLOEXEC => 0x80000;
public static UntypedInt TFD_NONBLOCK => 0x800;
public static UntypedInt TIOCCBRK => 0x5428;
public static UntypedInt TIOCCONS => 0x541d;
public static UntypedInt TIOCEXCL => 0x540c;
public static UntypedInt TIOCGDEV => 0x80045432;
public static UntypedInt TIOCGETD => 0x5424;
public static UntypedInt TIOCGEXCL => 0x80045440;
public static UntypedInt TIOCGICOUNT => 0x545d;
public static UntypedInt TIOCGISO7816 => 0x80285442;
public static UntypedInt TIOCGLCKTRMIOS => 0x5456;
public static UntypedInt TIOCGPGRP => 0x540f;
public static UntypedInt TIOCGPKT => 0x80045438;
public static UntypedInt TIOCGPTLCK => 0x80045439;
public static UntypedInt TIOCGPTN => 0x80045430;
public static UntypedInt TIOCGPTPEER => 0x5441;
public static UntypedInt TIOCGRS485 => 0x542e;
public static UntypedInt TIOCGSERIAL => 0x541e;
public static UntypedInt TIOCGSID => 0x5429;
public static UntypedInt TIOCGSOFTCAR => 0x5419;
public static UntypedInt TIOCGWINSZ => 0x5413;
public static UntypedInt TIOCINQ => 0x541b;
public static UntypedInt TIOCLINUX => 0x541c;
public static UntypedInt TIOCMBIC => 0x5417;
public static UntypedInt TIOCMBIS => 0x5416;
public static UntypedInt TIOCMGET => 0x5415;
public static UntypedInt TIOCMIWAIT => 0x545c;
public static UntypedInt TIOCMSET => 0x5418;
public static UntypedInt TIOCM_CAR => 0x40;
public static UntypedInt TIOCM_CD => 0x40;
public static UntypedInt TIOCM_CTS => 0x20;
public static UntypedInt TIOCM_DSR => 0x100;
public static UntypedInt TIOCM_RI => 0x80;
public static UntypedInt TIOCM_RNG => 0x80;
public static UntypedInt TIOCM_SR => 0x10;
public static UntypedInt TIOCM_ST => 0x8;
public static UntypedInt TIOCNOTTY => 0x5422;
public static UntypedInt TIOCNXCL => 0x540d;
public static UntypedInt TIOCOUTQ => 0x5411;
public static UntypedInt TIOCPKT => 0x5420;
public static UntypedInt TIOCSBRK => 0x5427;
public static UntypedInt TIOCSCTTY => 0x540e;
public static UntypedInt TIOCSERCONFIG => 0x5453;
public static UntypedInt TIOCSERGETLSR => 0x5459;
public static UntypedInt TIOCSERGETMULTI => 0x545a;
public static UntypedInt TIOCSERGSTRUCT => 0x5458;
public static UntypedInt TIOCSERGWILD => 0x5454;
public static UntypedInt TIOCSERSETMULTI => 0x545b;
public static UntypedInt TIOCSERSWILD => 0x5455;
public static UntypedInt TIOCSETD => 0x5423;
public static UntypedInt TIOCSIG => 0x40045436;
public static UntypedInt TIOCSISO7816 => 0xc0285443;
public static UntypedInt TIOCSLCKTRMIOS => 0x5457;
public static UntypedInt TIOCSPGRP => 0x5410;
public static UntypedInt TIOCSPTLCK => 0x40045431;
public static UntypedInt TIOCSRS485 => 0x542f;
public static UntypedInt TIOCSSERIAL => 0x541f;
public static UntypedInt TIOCSSOFTCAR => 0x541a;
public static UntypedInt TIOCSTI => 0x5412;
public static UntypedInt TIOCSWINSZ => 0x5414;
public static UntypedInt TIOCVHANGUP => 0x5437;
public static UntypedInt TOSTOP => 0x100;
public static UntypedInt TUNATTACHFILTER => 0x401054d5;
public static UntypedInt TUNDETACHFILTER => 0x401054d6;
public static UntypedInt TUNGETDEVNETNS => 0x54e3;
public static UntypedInt TUNGETFEATURES => 0x800454cf;
public static UntypedInt TUNGETFILTER => 0x801054db;
public static UntypedInt TUNGETIFF => 0x800454d2;
public static UntypedInt TUNGETSNDBUF => 0x800454d3;
public static UntypedInt TUNGETVNETBE => 0x800454df;
public static UntypedInt TUNGETVNETHDRSZ => 0x800454d7;
public static UntypedInt TUNGETVNETLE => 0x800454dd;
public static UntypedInt TUNSETCARRIER => 0x400454e2;
public static UntypedInt TUNSETDEBUG => 0x400454c9;
public static UntypedInt TUNSETFILTEREBPF => 0x800454e1;
public static UntypedInt TUNSETGROUP => 0x400454ce;
public static UntypedInt TUNSETIFF => 0x400454ca;
public static UntypedInt TUNSETIFINDEX => 0x400454da;
public static UntypedInt TUNSETLINK => 0x400454cd;
public static UntypedInt TUNSETNOCSUM => 0x400454c8;
public static UntypedInt TUNSETOFFLOAD => 0x400454d0;
public static UntypedInt TUNSETOWNER => 0x400454cc;
public static UntypedInt TUNSETPERSIST => 0x400454cb;
public static UntypedInt TUNSETQUEUE => 0x400454d9;
public static UntypedInt TUNSETSNDBUF => 0x400454d4;
public static UntypedInt TUNSETSTEERINGEBPF => 0x800454e0;
public static UntypedInt TUNSETTXFILTER => 0x400454d1;
public static UntypedInt TUNSETVNETBE => 0x400454de;
public static UntypedInt TUNSETVNETHDRSZ => 0x400454d8;
public static UntypedInt TUNSETVNETLE => 0x400454dc;
public static UntypedInt UBI_IOCATT => 0x40186f40;
public static UntypedInt UBI_IOCDET => 0x40046f41;
public static UntypedInt UBI_IOCEBCH => 0x40044f02;
public static UntypedInt UBI_IOCEBER => 0x40044f01;
public static UntypedInt UBI_IOCEBISMAP => 0x80044f05;
public static UntypedInt UBI_IOCEBMAP => 0x40084f03;
public static UntypedInt UBI_IOCEBUNMAP => 0x40044f04;
public static UntypedInt UBI_IOCMKVOL => 0x40986f00;
public static UntypedInt UBI_IOCRMVOL => 0x40046f01;
public static UntypedInt UBI_IOCRNVOL => 0x51106f03;
public static UntypedInt UBI_IOCRPEB => 0x40046f04;
public static UntypedInt UBI_IOCRSVOL => 0x400c6f02;
public static UntypedInt UBI_IOCSETVOLPROP => 0x40104f06;
public static UntypedInt UBI_IOCSPEB => 0x40046f05;
public static UntypedInt UBI_IOCVOLCRBLK => 0x40804f07;
public static UntypedInt UBI_IOCVOLRMBLK => 0x4f08;
public static UntypedInt UBI_IOCVOLUP => 0x40084f00;
public static UntypedInt VDISCARD => 0xd;
public static UntypedInt VEOF => 0x4;
public static UntypedInt VEOL => 0xb;
public static UntypedInt VEOL2 => 0x10;
public static UntypedInt VMIN => 0x6;
public static UntypedInt VREPRINT => 0xc;
public static UntypedInt VSTART => 0x8;
public static UntypedInt VSTOP => 0x9;
public static UntypedInt VSUSP => 0xa;
public static UntypedInt VSWTC => 0x7;
public static UntypedInt VT1 => 0x4000;
public static UntypedInt VTDLY => 0x4000;
public static UntypedInt VTIME => 0x5;
public static UntypedInt VWERASE => 0xe;
public static UntypedInt WDIOC_GETBOOTSTATUS => 0x80045702;
public static UntypedInt WDIOC_GETPRETIMEOUT => 0x80045709;
public static UntypedInt WDIOC_GETSTATUS => 0x80045701;
public static UntypedInt WDIOC_GETSUPPORT => 0x80285700;
public static UntypedInt WDIOC_GETTEMP => 0x80045703;
public static UntypedInt WDIOC_GETTIMELEFT => 0x8004570a;
public static UntypedInt WDIOC_GETTIMEOUT => 0x80045707;
public static UntypedInt WDIOC_KEEPALIVE => 0x80045705;
public static UntypedInt WDIOC_SETOPTIONS => 0x80045704;
public static UntypedInt WORDSIZE => 0x40;
public static UntypedInt XCASE => 0x4;
public static UntypedInt XTABS => 0x1800;
internal static UntypedInt _HIDIOCGRAWNAME => 0x80804804;
internal static UntypedInt _HIDIOCGRAWPHYS => 0x80404805;
internal static UntypedInt _HIDIOCGRAWUNIQ => 0x80404808;

public static syscall.Errno EADDRINUSE => /* syscall.Errno(0x62) */ 98;
public static syscall.Errno EADDRNOTAVAIL => /* syscall.Errno(0x63) */ 99;
public static syscall.Errno EADV => /* syscall.Errno(0x44) */ 68;
public static syscall.Errno EAFNOSUPPORT => /* syscall.Errno(0x61) */ 97;
public static syscall.Errno EALREADY => /* syscall.Errno(0x72) */ 114;
public static syscall.Errno EBADE => /* syscall.Errno(0x34) */ 52;
public static syscall.Errno EBADFD => /* syscall.Errno(0x4d) */ 77;
public static syscall.Errno EBADMSG => /* syscall.Errno(0x4a) */ 74;
public static syscall.Errno EBADR => /* syscall.Errno(0x35) */ 53;
public static syscall.Errno EBADRQC => /* syscall.Errno(0x38) */ 56;
public static syscall.Errno EBADSLT => /* syscall.Errno(0x39) */ 57;
public static syscall.Errno EBFONT => /* syscall.Errno(0x3b) */ 59;
public static syscall.Errno ECANCELED => /* syscall.Errno(0x7d) */ 125;
public static syscall.Errno ECHRNG => /* syscall.Errno(0x2c) */ 44;
public static syscall.Errno ECOMM => /* syscall.Errno(0x46) */ 70;
public static syscall.Errno ECONNABORTED => /* syscall.Errno(0x67) */ 103;
public static syscall.Errno ECONNREFUSED => /* syscall.Errno(0x6f) */ 111;
public static syscall.Errno ECONNRESET => /* syscall.Errno(0x68) */ 104;
public static syscall.Errno EDEADLK => /* syscall.Errno(0x23) */ 35;
public static syscall.Errno EDEADLOCK => /* syscall.Errno(0x23) */ 35;
public static syscall.Errno EDESTADDRREQ => /* syscall.Errno(0x59) */ 89;
public static syscall.Errno EDOTDOT => /* syscall.Errno(0x49) */ 73;
public static syscall.Errno EDQUOT => /* syscall.Errno(0x7a) */ 122;
public static syscall.Errno EFSBADCRC => /* syscall.Errno(0x4a) */ 74;
public static syscall.Errno EFSCORRUPTED => /* syscall.Errno(0x75) */ 117;
public static syscall.Errno EFTYPE => /* syscall.Errno(0x86) */ 134;
public static syscall.Errno EHOSTDOWN => /* syscall.Errno(0x70) */ 112;
public static syscall.Errno EHOSTUNREACH => /* syscall.Errno(0x71) */ 113;
public static syscall.Errno EHWPOISON => /* syscall.Errno(0x85) */ 133;
public static syscall.Errno EIDRM => /* syscall.Errno(0x2b) */ 43;
public static syscall.Errno EILSEQ => /* syscall.Errno(0x54) */ 84;
public static syscall.Errno EINPROGRESS => /* syscall.Errno(0x73) */ 115;
public static syscall.Errno EISCONN => /* syscall.Errno(0x6a) */ 106;
public static syscall.Errno EISNAM => /* syscall.Errno(0x78) */ 120;
public static syscall.Errno EKEYEXPIRED => /* syscall.Errno(0x7f) */ 127;
public static syscall.Errno EKEYREJECTED => /* syscall.Errno(0x81) */ 129;
public static syscall.Errno EKEYREVOKED => /* syscall.Errno(0x80) */ 128;
public static syscall.Errno EL2HLT => /* syscall.Errno(0x33) */ 51;
public static syscall.Errno EL2NSYNC => /* syscall.Errno(0x2d) */ 45;
public static syscall.Errno EL3HLT => /* syscall.Errno(0x2e) */ 46;
public static syscall.Errno EL3RST => /* syscall.Errno(0x2f) */ 47;
public static syscall.Errno ELIBACC => /* syscall.Errno(0x4f) */ 79;
public static syscall.Errno ELIBBAD => /* syscall.Errno(0x50) */ 80;
public static syscall.Errno ELIBEXEC => /* syscall.Errno(0x53) */ 83;
public static syscall.Errno ELIBMAX => /* syscall.Errno(0x52) */ 82;
public static syscall.Errno ELIBSCN => /* syscall.Errno(0x51) */ 81;
public static syscall.Errno ELNRNG => /* syscall.Errno(0x30) */ 48;
public static syscall.Errno ELOOP => /* syscall.Errno(0x28) */ 40;
public static syscall.Errno EMEDIUMTYPE => /* syscall.Errno(0x7c) */ 124;
public static syscall.Errno EMSGSIZE => /* syscall.Errno(0x5a) */ 90;
public static syscall.Errno EMULTIHOP => /* syscall.Errno(0x48) */ 72;
public static syscall.Errno ENAMETOOLONG => /* syscall.Errno(0x24) */ 36;
public static syscall.Errno ENAVAIL => /* syscall.Errno(0x77) */ 119;
public static syscall.Errno ENETDOWN => /* syscall.Errno(0x64) */ 100;
public static syscall.Errno ENETRESET => /* syscall.Errno(0x66) */ 102;
public static syscall.Errno ENETUNREACH => /* syscall.Errno(0x65) */ 101;
public static syscall.Errno ENOANO => /* syscall.Errno(0x37) */ 55;
public static syscall.Errno ENOBUFS => /* syscall.Errno(0x69) */ 105;
public static syscall.Errno ENOCSI => /* syscall.Errno(0x32) */ 50;
public static syscall.Errno ENODATA => /* syscall.Errno(0x3d) */ 61;
public static syscall.Errno ENOKEY => /* syscall.Errno(0x7e) */ 126;
public static syscall.Errno ENOLCK => /* syscall.Errno(0x25) */ 37;
public static syscall.Errno ENOLINK => /* syscall.Errno(0x43) */ 67;
public static syscall.Errno ENOMEDIUM => /* syscall.Errno(0x7b) */ 123;
public static syscall.Errno ENOMSG => /* syscall.Errno(0x2a) */ 42;
public static syscall.Errno ENONET => /* syscall.Errno(0x40) */ 64;
public static syscall.Errno ENOPKG => /* syscall.Errno(0x41) */ 65;
public static syscall.Errno ENOPROTOOPT => /* syscall.Errno(0x5c) */ 92;
public static syscall.Errno ENOSR => /* syscall.Errno(0x3f) */ 63;
public static syscall.Errno ENOSTR => /* syscall.Errno(0x3c) */ 60;
public static syscall.Errno ENOSYS => /* syscall.Errno(0x26) */ 38;
public static syscall.Errno ENOTCONN => /* syscall.Errno(0x6b) */ 107;
public static syscall.Errno ENOTEMPTY => /* syscall.Errno(0x27) */ 39;
public static syscall.Errno ENOTNAM => /* syscall.Errno(0x76) */ 118;
public static syscall.Errno ENOTRECOVERABLE => /* syscall.Errno(0x83) */ 131;
public static syscall.Errno ENOTSOCK => /* syscall.Errno(0x58) */ 88;
public static syscall.Errno ENOTSUP => /* syscall.Errno(0x5f) */ 95;
public static syscall.Errno ENOTUNIQ => /* syscall.Errno(0x4c) */ 76;
public static syscall.Errno EOPNOTSUPP => /* syscall.Errno(0x5f) */ 95;
public static syscall.Errno EOVERFLOW => /* syscall.Errno(0x4b) */ 75;
public static syscall.Errno EOWNERDEAD => /* syscall.Errno(0x82) */ 130;
public static syscall.Errno EPFNOSUPPORT => /* syscall.Errno(0x60) */ 96;
public static syscall.Errno EPROTO => /* syscall.Errno(0x47) */ 71;
public static syscall.Errno EPROTONOSUPPORT => /* syscall.Errno(0x5d) */ 93;
public static syscall.Errno EPROTOTYPE => /* syscall.Errno(0x5b) */ 91;
public static syscall.Errno EREMCHG => /* syscall.Errno(0x4e) */ 78;
public static syscall.Errno EREMOTE => /* syscall.Errno(0x42) */ 66;
public static syscall.Errno EREMOTEIO => /* syscall.Errno(0x79) */ 121;
public static syscall.Errno ERESTART => /* syscall.Errno(0x55) */ 85;
public static syscall.Errno ERFKILL => /* syscall.Errno(0x84) */ 132;
public static syscall.Errno ESHUTDOWN => /* syscall.Errno(0x6c) */ 108;
public static syscall.Errno ESOCKTNOSUPPORT => /* syscall.Errno(0x5e) */ 94;
public static syscall.Errno ESRMNT => /* syscall.Errno(0x45) */ 69;
public static syscall.Errno ESTALE => /* syscall.Errno(0x74) */ 116;
public static syscall.Errno ESTRPIPE => /* syscall.Errno(0x56) */ 86;
public static syscall.Errno ETIME => /* syscall.Errno(0x3e) */ 62;
public static syscall.Errno ETIMEDOUT => /* syscall.Errno(0x6e) */ 110;
public static syscall.Errno ETOOMANYREFS => /* syscall.Errno(0x6d) */ 109;
public static syscall.Errno EUCLEAN => /* syscall.Errno(0x75) */ 117;
public static syscall.Errno EUNATCH => /* syscall.Errno(0x31) */ 49;
public static syscall.Errno EUSERS => /* syscall.Errno(0x57) */ 87;
public static syscall.Errno EXFULL => /* syscall.Errno(0x36) */ 54;

public static syscall.Signal SIGBUS => /* syscall.Signal(0x7) */ 7;
public static syscall.Signal SIGCHLD => /* syscall.Signal(0x11) */ 17;
public static syscall.Signal SIGCLD => /* syscall.Signal(0x11) */ 17;
public static syscall.Signal SIGCONT => /* syscall.Signal(0x12) */ 18;
public static syscall.Signal SIGIO => /* syscall.Signal(0x1d) */ 29;
public static syscall.Signal SIGPOLL => /* syscall.Signal(0x1d) */ 29;
public static syscall.Signal SIGPROF => /* syscall.Signal(0x1b) */ 27;
public static syscall.Signal SIGPWR => /* syscall.Signal(0x1e) */ 30;
public static syscall.Signal SIGSTKFLT => /* syscall.Signal(0x10) */ 16;
public static syscall.Signal SIGSTOP => /* syscall.Signal(0x13) */ 19;
public static syscall.Signal SIGSYS => /* syscall.Signal(0x1f) */ 31;
public static syscall.Signal SIGTSTP => /* syscall.Signal(0x14) */ 20;
public static syscall.Signal SIGTTIN => /* syscall.Signal(0x15) */ 21;
public static syscall.Signal SIGTTOU => /* syscall.Signal(0x16) */ 22;
public static syscall.Signal SIGURG => /* syscall.Signal(0x17) */ 23;
public static syscall.Signal SIGUSR1 => /* syscall.Signal(0xa) */ 10;
public static syscall.Signal SIGUSR2 => /* syscall.Signal(0xc) */ 12;
public static syscall.Signal SIGVTALRM => /* syscall.Signal(0x1a) */ 26;
public static syscall.Signal SIGWINCH => /* syscall.Signal(0x1c) */ 28;
public static syscall.Signal SIGXCPU => /* syscall.Signal(0x18) */ 24;
public static syscall.Signal SIGXFSZ => /* syscall.Signal(0x19) */ 25;


partial struct errorListᴛ1 /*dyn*/ {
    internal syscall.Errno num;
    internal @string name;
    internal @string desc;
}
[GoLazy] partial class lazyᴛerrorList { internal static array<errorListᴛ1> errorList = new errorListᴛ1[]{
    new(1, "EPERM", "operation not permitted"),
    new(2, "ENOENT", "no such file or directory"),
    new(3, "ESRCH", "no such process"),
    new(4, "EINTR", "interrupted system call"),
    new(5, "EIO", "input/output error"),
    new(6, "ENXIO", "no such device or address"),
    new(7, "E2BIG", "argument list too long"),
    new(8, "ENOEXEC", "exec format error"),
    new(9, "EBADF", "bad file descriptor"),
    new(10, "ECHILD", "no child processes"),
    new(11, "EAGAIN", "resource temporarily unavailable"),
    new(12, "ENOMEM", "cannot allocate memory"),
    new(13, "EACCES", "permission denied"),
    new(14, "EFAULT", "bad address"),
    new(15, "ENOTBLK", "block device required"),
    new(16, "EBUSY", "device or resource busy"),
    new(17, "EEXIST", "file exists"),
    new(18, "EXDEV", "invalid cross-device link"),
    new(19, "ENODEV", "no such device"),
    new(20, "ENOTDIR", "not a directory"),
    new(21, "EISDIR", "is a directory"),
    new(22, "EINVAL", "invalid argument"),
    new(23, "ENFILE", "too many open files in system"),
    new(24, "EMFILE", "too many open files"),
    new(25, "ENOTTY", "inappropriate ioctl for device"),
    new(26, "ETXTBSY", "text file busy"),
    new(27, "EFBIG", "file too large"),
    new(28, "ENOSPC", "no space left on device"),
    new(29, "ESPIPE", "illegal seek"),
    new(30, "EROFS", "read-only file system"),
    new(31, "EMLINK", "too many links"),
    new(32, "EPIPE", "broken pipe"),
    new(33, "EDOM", "numerical argument out of domain"),
    new(34, "ERANGE", "numerical result out of range"),
    new(35, "EDEADLK", "resource deadlock avoided"),
    new(36, "ENAMETOOLONG", "file name too long"),
    new(37, "ENOLCK", "no locks available"),
    new(38, "ENOSYS", "function not implemented"),
    new(39, "ENOTEMPTY", "directory not empty"),
    new(40, "ELOOP", "too many levels of symbolic links"),
    new(42, "ENOMSG", "no message of desired type"),
    new(43, "EIDRM", "identifier removed"),
    new(44, "ECHRNG", "channel number out of range"),
    new(45, "EL2NSYNC", "level 2 not synchronized"),
    new(46, "EL3HLT", "level 3 halted"),
    new(47, "EL3RST", "level 3 reset"),
    new(48, "ELNRNG", "link number out of range"),
    new(49, "EUNATCH", "protocol driver not attached"),
    new(50, "ENOCSI", "no CSI structure available"),
    new(51, "EL2HLT", "level 2 halted"),
    new(52, "EBADE", "invalid exchange"),
    new(53, "EBADR", "invalid request descriptor"),
    new(54, "EXFULL", "exchange full"),
    new(55, "ENOANO", "no anode"),
    new(56, "EBADRQC", "invalid request code"),
    new(57, "EBADSLT", "invalid slot"),
    new(59, "EBFONT", "bad font file format"),
    new(60, "ENOSTR", "device not a stream"),
    new(61, "ENODATA", "no data available"),
    new(62, "ETIME", "timer expired"),
    new(63, "ENOSR", "out of streams resources"),
    new(64, "ENONET", "machine is not on the network"),
    new(65, "ENOPKG", "package not installed"),
    new(66, "EREMOTE", "object is remote"),
    new(67, "ENOLINK", "link has been severed"),
    new(68, "EADV", "advertise error"),
    new(69, "ESRMNT", "srmount error"),
    new(70, "ECOMM", "communication error on send"),
    new(71, "EPROTO", "protocol error"),
    new(72, "EMULTIHOP", "multihop attempted"),
    new(73, "EDOTDOT", "RFS specific error"),
    new(74, "EBADMSG", "bad message"),
    new(75, "EOVERFLOW", "value too large for defined data type"),
    new(76, "ENOTUNIQ", "name not unique on network"),
    new(77, "EBADFD", "file descriptor in bad state"),
    new(78, "EREMCHG", "remote address changed"),
    new(79, "ELIBACC", "can not access a needed shared library"),
    new(80, "ELIBBAD", "accessing a corrupted shared library"),
    new(81, "ELIBSCN", ".lib section in a.out corrupted"),
    new(82, "ELIBMAX", "attempting to link in too many shared libraries"),
    new(83, "ELIBEXEC", "cannot exec a shared library directly"),
    new(84, "EILSEQ", "invalid or incomplete multibyte or wide character"),
    new(85, "ERESTART", "interrupted system call should be restarted"),
    new(86, "ESTRPIPE", "streams pipe error"),
    new(87, "EUSERS", "too many users"),
    new(88, "ENOTSOCK", "socket operation on non-socket"),
    new(89, "EDESTADDRREQ", "destination address required"),
    new(90, "EMSGSIZE", "message too long"),
    new(91, "EPROTOTYPE", "protocol wrong type for socket"),
    new(92, "ENOPROTOOPT", "protocol not available"),
    new(93, "EPROTONOSUPPORT", "protocol not supported"),
    new(94, "ESOCKTNOSUPPORT", "socket type not supported"),
    new(95, "ENOTSUP", "operation not supported"),
    new(96, "EPFNOSUPPORT", "protocol family not supported"),
    new(97, "EAFNOSUPPORT", "address family not supported by protocol"),
    new(98, "EADDRINUSE", "address already in use"),
    new(99, "EADDRNOTAVAIL", "cannot assign requested address"),
    new(100, "ENETDOWN", "network is down"),
    new(101, "ENETUNREACH", "network is unreachable"),
    new(102, "ENETRESET", "network dropped connection on reset"),
    new(103, "ECONNABORTED", "software caused connection abort"),
    new(104, "ECONNRESET", "connection reset by peer"),
    new(105, "ENOBUFS", "no buffer space available"),
    new(106, "EISCONN", "transport endpoint is already connected"),
    new(107, "ENOTCONN", "transport endpoint is not connected"),
    new(108, "ESHUTDOWN", "cannot send after transport endpoint shutdown"),
    new(109, "ETOOMANYREFS", "too many references: cannot splice"),
    new(110, "ETIMEDOUT", "connection timed out"),
    new(111, "ECONNREFUSED", "connection refused"),
    new(112, "EHOSTDOWN", "host is down"),
    new(113, "EHOSTUNREACH", "no route to host"),
    new(114, "EALREADY", "operation already in progress"),
    new(115, "EINPROGRESS", "operation now in progress"),
    new(116, "ESTALE", "stale file handle"),
    new(117, "EFSCORRUPTED", "structure needs cleaning"),
    new(118, "ENOTNAM", "not a XENIX named type file"),
    new(119, "ENAVAIL", "no XENIX semaphores available"),
    new(120, "EISNAM", "is a named type file"),
    new(121, "EREMOTEIO", "remote I/O error"),
    new(122, "EDQUOT", "disk quota exceeded"),
    new(123, "ENOMEDIUM", "no medium found"),
    new(124, "EMEDIUMTYPE", "wrong medium type"),
    new(125, "ECANCELED", "operation canceled"),
    new(126, "ENOKEY", "required key not available"),
    new(127, "EKEYEXPIRED", "key has expired"),
    new(128, "EKEYREVOKED", "key has been revoked"),
    new(129, "EKEYREJECTED", "key was rejected by service"),
    new(130, "EOWNERDEAD", "owner died"),
    new(131, "ENOTRECOVERABLE", "state not recoverable"),
    new(132, "ERFKILL", "operation not possible due to RF-kill"),
    new(133, "EHWPOISON", "memory page has hardware error"),
    new(134, "EFTYPE", "unknown error 134")
}.array(); }


partial struct signalListᴛ1 /*dyn*/ {
    internal syscall.Signal num;
    internal @string name;
    internal @string desc;
}
[GoLazy] partial class lazyᴛsignalList { internal static array<signalListᴛ1> signalList = new signalListᴛ1[]{
    new(1, "SIGHUP", "hangup"),
    new(2, "SIGINT", "interrupt"),
    new(3, "SIGQUIT", "quit"),
    new(4, "SIGILL", "illegal instruction"),
    new(5, "SIGTRAP", "trace/breakpoint trap"),
    new(6, "SIGABRT", "aborted"),
    new(7, "SIGBUS", "bus error"),
    new(8, "SIGFPE", "floating point exception"),
    new(9, "SIGKILL", "killed"),
    new(10, "SIGUSR1", "user defined signal 1"),
    new(11, "SIGSEGV", "segmentation fault"),
    new(12, "SIGUSR2", "user defined signal 2"),
    new(13, "SIGPIPE", "broken pipe"),
    new(14, "SIGALRM", "alarm clock"),
    new(15, "SIGTERM", "terminated"),
    new(16, "SIGSTKFLT", "stack fault"),
    new(17, "SIGCHLD", "child exited"),
    new(18, "SIGCONT", "continued"),
    new(19, "SIGSTOP", "stopped (signal)"),
    new(20, "SIGTSTP", "stopped"),
    new(21, "SIGTTIN", "stopped (tty input)"),
    new(22, "SIGTTOU", "stopped (tty output)"),
    new(23, "SIGURG", "urgent I/O condition"),
    new(24, "SIGXCPU", "CPU time limit exceeded"),
    new(25, "SIGXFSZ", "file size limit exceeded"),
    new(26, "SIGVTALRM", "virtual timer expired"),
    new(27, "SIGPROF", "profiling timer expired"),
    new(28, "SIGWINCH", "window changed"),
    new(29, "SIGIO", "I/O possible"),
    new(30, "SIGPWR", "power failure"),
    new(31, "SIGSYS", "bad system call")
}.array(); }

} // end unix_package
