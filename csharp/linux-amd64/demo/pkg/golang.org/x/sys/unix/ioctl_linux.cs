// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using @unsafe = unsafe_package;
using syscall = syscall_package;

partial class unix_package {

public static (nint, error) IoctlRetInt(nint fd, nuint req) {
    var (ret, _, err) = Syscall(SYS_IOCTL, (uintptr)fd, (uintptr)req, 0);
    if (err != 0) {
        return (0, err);
    }
    return ((nint)ret, default);
}

public static (uint32, error) IoctlGetUint32(nint fd, nuint req) {
    ref var value = ref heap(new uint32(), out var Ꮡvalue);
    var err = ioctlPtr(fd, req, @unsafe.Pointer.FromPinnedBox(Ꮡvalue));
    return (value, err);
}

public static (ж<RTCTime>, error) IoctlGetRTCTime(nint fd) {
    ref var value = ref heap(new RTCTime(), out var Ꮡvalue);
    var err = ioctlPtr(fd, RTC_RD_TIME, @unsafe.Pointer.FromPinnedBox(Ꮡvalue));
    return (Ꮡvalue, err);
}

public static error IoctlSetRTCTime(nint fd, ж<RTCTime> value) {
    return ioctlPtr(fd, RTC_SET_TIME, @unsafe.Pointer.FromPinnedBox(value));
}

public static (ж<RTCWkAlrm>, error) IoctlGetRTCWkAlrm(nint fd) {
    ref var value = ref heap(new RTCWkAlrm(), out var Ꮡvalue);
    var err = ioctlPtr(fd, RTC_WKALM_RD, @unsafe.Pointer.FromPinnedBox(Ꮡvalue));
    return (Ꮡvalue, err);
}

public static error IoctlSetRTCWkAlrm(nint fd, ж<RTCWkAlrm> value) {
    return ioctlPtr(fd, RTC_WKALM_SET, @unsafe.Pointer.FromPinnedBox(value));
}

public static (ж<EthtoolDrvinfo>, error) IoctlGetEthtoolDrvinfo(nint fd, @string ifname) {
    var (ifr, err) = NewIfreq(ifname);
    if (err != nil) {
        return (default, err);
    }

    ref var value = ref heap<EthtoolDrvinfo>(new EthtoolDrvinfo(Cmd: ETHTOOL_GDRVINFO), out var Ꮡvalue);
    ref var ifrd = ref heap<ifreqData>(ifr.withData(@unsafe.Pointer.FromPinnedBox(Ꮡvalue)), out var Ꮡifrd);

    err = ioctlIfreqData(fd, SIOCETHTOOL, Ꮡifrd);
    return (Ꮡvalue, err);
}

public static (ж<EthtoolTsInfo>, error) IoctlGetEthtoolTsInfo(nint fd, @string ifname) {
    var (ifr, err) = NewIfreq(ifname);
    if (err != nil) {
        return (default, err);
    }

    ref var value = ref heap<EthtoolTsInfo>(new EthtoolTsInfo(Cmd: ETHTOOL_GET_TS_INFO), out var Ꮡvalue);
    ref var ifrd = ref heap<ifreqData>(ifr.withData(@unsafe.Pointer.FromPinnedBox(Ꮡvalue)), out var Ꮡifrd);

    err = ioctlIfreqData(fd, SIOCETHTOOL, Ꮡifrd);
    return (Ꮡvalue, err);
}

public static (ж<HwTstampConfig>, error) IoctlGetHwTstamp(nint fd, @string ifname) {
    var (ifr, err) = NewIfreq(ifname);
    if (err != nil) {
        return (default, err);
    }

    ref var value = ref heap<HwTstampConfig>(new HwTstampConfig(), out var Ꮡvalue);
    ref var ifrd = ref heap<ifreqData>(ifr.withData(@unsafe.Pointer.FromPinnedBox(Ꮡvalue)), out var Ꮡifrd);

    err = ioctlIfreqData(fd, SIOCGHWTSTAMP, Ꮡifrd);
    return (Ꮡvalue, err);
}

public static error IoctlSetHwTstamp(nint fd, @string ifname, ж<HwTstampConfig> cfg) {
    var (ifr, err) = NewIfreq(ifname);
    if (err != nil) {
        return err;
    }
    ref var ifrd = ref heap<ifreqData>(ifr.withData(@unsafe.Pointer.FromPinnedBox(cfg)), out var Ꮡifrd);
    return ioctlIfreqData(fd, SIOCSHWTSTAMP, Ꮡifrd);
}

public static int32 FdToClockID(nint fd) { return (int32)(((nint)(~fd) << 3) | 3); }

public static (ж<PtpClockCaps>, error) IoctlPtpClockGetcaps(nint fd) {
    ref var value = ref heap(new PtpClockCaps(), out var Ꮡvalue);
    var err = ioctlPtr(fd, PTP_CLOCK_GETCAPS2, @unsafe.Pointer.FromPinnedBox(Ꮡvalue));
    return (Ꮡvalue, err);
}

public static (ж<PtpSysOffsetPrecise>, error) IoctlPtpSysOffsetPrecise(nint fd) {
    ref var value = ref heap(new PtpSysOffsetPrecise(), out var Ꮡvalue);
    var err = ioctlPtr(fd, PTP_SYS_OFFSET_PRECISE2, @unsafe.Pointer.FromPinnedBox(Ꮡvalue));
    return (Ꮡvalue, err);
}

public static (ж<PtpSysOffsetExtended>, error) IoctlPtpSysOffsetExtended(nint fd, nuint samples) {
    ref var value = ref heap<PtpSysOffsetExtended>(new PtpSysOffsetExtended(Samples: (uint32)samples), out var Ꮡvalue);
    var err = ioctlPtr(fd, PTP_SYS_OFFSET_EXTENDED2, @unsafe.Pointer.FromPinnedBox(Ꮡvalue));
    return (Ꮡvalue, err);
}

public static (ж<PtpPinDesc>, error) IoctlPtpPinGetfunc(nint fd, nuint index) {
    ref var value = ref heap<PtpPinDesc>(new PtpPinDesc(Index: (uint32)index), out var Ꮡvalue);
    var err = ioctlPtr(fd, PTP_PIN_GETFUNC2, @unsafe.Pointer.FromPinnedBox(Ꮡvalue));
    return (Ꮡvalue, err);
}

public static error IoctlPtpPinSetfunc(nint fd, ж<PtpPinDesc> pd) {
    return ioctlPtr(fd, PTP_PIN_SETFUNC2, @unsafe.Pointer.FromPinnedBox(pd));
}

public static error IoctlPtpPeroutRequest(nint fd, ж<PtpPeroutRequest> r) {
    return ioctlPtr(fd, PTP_PEROUT_REQUEST2, @unsafe.Pointer.FromPinnedBox(r));
}

public static error IoctlPtpExttsRequest(nint fd, ж<PtpExttsRequest> r) {
    return ioctlPtr(fd, PTP_EXTTS_REQUEST2, @unsafe.Pointer.FromPinnedBox(r));
}

public static (ж<WatchdogInfo>, error) IoctlGetWatchdogInfo(nint fd) {
    ref var value = ref heap(new WatchdogInfo(), out var Ꮡvalue);
    var err = ioctlPtr(fd, WDIOC_GETSUPPORT, @unsafe.Pointer.FromPinnedBox(Ꮡvalue));
    return (Ꮡvalue, err);
}

public static error IoctlWatchdogKeepalive(nint fd) {
    return ioctl(fd, WDIOC_KEEPALIVE, 0);
}

public static error IoctlFileCloneRange(nint destFd, ж<FileCloneRange> value) {
    return ioctlPtr(destFd, FICLONERANGE, @unsafe.Pointer.FromPinnedBox(value));
}

public static error IoctlFileClone(nint destFd, nint srcFd) {
    return ioctl(destFd, FICLONE, (uintptr)srcFd);
}

partial struct FileDedupeRange {
    public uint64 Src_offset;
    public uint64 Src_length;
    public uint16 Reserved1;
    public uint32 Reserved2;
    public slice<FileDedupeRangeInfo> Info;
}

partial struct FileDedupeRangeInfo {
    public int64 Dest_fd;
    public uint64 Dest_offset;
    public uint64 Bytes_deduped;
    public int32 Status;
    public uint32 Reserved;
}

public static error IoctlFileDedupeRange(nint srcFd, ж<FileDedupeRange> Ꮡvalue) {
    ref var value = ref Ꮡvalue.DerefOrNull();

    var buf = new slice<byte>((nint)SizeofRawFileDedupeRange + len(value.Info) * (nint)SizeofRawFileDedupeRangeInfo);
    var rawrange = Ꮡ(buf, 0).Reinterpret<byte, RawFileDedupeRange>();
    rawrange.Src_offset = value.Src_offset;
    rawrange.Src_length = value.Src_length;
    rawrange.Dest_count = (uint16)len(value.Info);
    rawrange.Reserved1 = value.Reserved1;
    rawrange.Reserved2 = value.Reserved2;

    foreach (var i in range(len(value.Info))) {
        var rawinfo = @unsafe.Pointer.To<RawFileDedupeRangeInfo>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 0)), (uintptr)SizeofRawFileDedupeRange + (uintptr)(i * (nint)SizeofRawFileDedupeRangeInfo)));
        rawinfo.Dest_fd = value.Info[i].Dest_fd;
        rawinfo.Dest_offset = value.Info[i].Dest_offset;
        rawinfo.Bytes_deduped = value.Info[i].Bytes_deduped;
        rawinfo.Status = value.Info[i].Status;
        rawinfo.Reserved = value.Info[i].Reserved;
    }

    var err = ioctlPtr(srcFd, FIDEDUPERANGE, @unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 0)));

    foreach (var i in range(len(value.Info))) {
        var rawinfo = @unsafe.Pointer.To<RawFileDedupeRangeInfo>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 0)), (uintptr)SizeofRawFileDedupeRange + (uintptr)(i * (nint)SizeofRawFileDedupeRangeInfo)));
        value.Info[i].Dest_fd = rawinfo.Dest_fd;
        value.Info[i].Dest_offset = rawinfo.Dest_offset;
        value.Info[i].Bytes_deduped = rawinfo.Bytes_deduped;
        value.Info[i].Status = rawinfo.Status;
        value.Info[i].Reserved = rawinfo.Reserved;
    }

    return err;
}

public static error IoctlHIDGetDesc(nint fd, ж<HIDRawReportDescriptor> value) {
    return ioctlPtr(fd, HIDIOCGRDESC, @unsafe.Pointer.FromPinnedBox(value));
}

public static (ж<HIDRawDevInfo>, error) IoctlHIDGetRawInfo(nint fd) {
    ref var value = ref heap(new HIDRawDevInfo(), out var Ꮡvalue);
    var err = ioctlPtr(fd, HIDIOCGRAWINFO, @unsafe.Pointer.FromPinnedBox(Ꮡvalue));
    return (Ꮡvalue, err);
}

public static (@string, error) IoctlHIDGetRawName(nint fd) {
    ref var value = ref heap(new array<byte>(128), out var Ꮡvalue);
    var err = ioctlPtr(fd, _HIDIOCGRAWNAME, @unsafe.Pointer.FromPinnedBox(Ꮡvalue.at<byte>(0)));
    return (ByteSliceToString(value[..]), err);
}

public static (@string, error) IoctlHIDGetRawPhys(nint fd) {
    ref var value = ref heap(new array<byte>(64), out var Ꮡvalue);
    var err = ioctlPtr(fd, _HIDIOCGRAWPHYS, @unsafe.Pointer.FromPinnedBox(Ꮡvalue.at<byte>(0)));
    return (ByteSliceToString(value[..]), err);
}

public static (@string, error) IoctlHIDGetRawUniq(nint fd) {
    ref var value = ref heap(new array<byte>(64), out var Ꮡvalue);
    var err = ioctlPtr(fd, _HIDIOCGRAWUNIQ, @unsafe.Pointer.FromPinnedBox(Ꮡvalue.at<byte>(0)));
    return (ByteSliceToString(value[..]), err);
}

public static error IoctlIfreq(nint fd, nuint req, ж<Ifreq> value) {
    return ioctlPtr(fd, req, @unsafe.Pointer.FromPinnedBox(value.Ꮡraw));
}

internal static error ioctlIfreqData(nint fd, nuint req, ж<ifreqData> value) {
    return ioctlPtr(fd, req, @unsafe.Pointer.FromPinnedBox(value));
}

public static (ж<KCMClone>, error) IoctlKCMClone(nint fd) {
    ref var info = ref heap(new KCMClone(), out var Ꮡinfo);
    var err = ioctlPtr(fd, SIOCKCMCLONE, @unsafe.Pointer.FromPinnedBox(Ꮡinfo)); if (err != nil) {
        return (default, err);
    }

    return (Ꮡinfo, default);
}

public static error IoctlKCMAttach(nint fd, KCMAttach infoʗp) {
    ref var info = ref heap(infoʗp, out var Ꮡinfo);

    return ioctlPtr(fd, SIOCKCMATTACH, @unsafe.Pointer.FromPinnedBox(Ꮡinfo));
}

public static error IoctlKCMUnattach(nint fd, KCMUnattach infoʗp) {
    ref var info = ref heap(infoʗp, out var Ꮡinfo);

    return ioctlPtr(fd, SIOCKCMUNATTACH, @unsafe.Pointer.FromPinnedBox(Ꮡinfo));
}

public static (ж<LoopInfo64>, error) IoctlLoopGetStatus64(nint fd) {
    ref var value = ref heap(new LoopInfo64(), out var Ꮡvalue);
    var err = ioctlPtr(fd, LOOP_GET_STATUS64, @unsafe.Pointer.FromPinnedBox(Ꮡvalue)); if (err != nil) {
        return (default, err);
    }
    return (Ꮡvalue, default);
}

public static error IoctlLoopSetStatus64(nint fd, ж<LoopInfo64> value) {
    return ioctlPtr(fd, LOOP_SET_STATUS64, @unsafe.Pointer.FromPinnedBox(value));
}

public static error IoctlLoopConfigure(nint fd, ж<LoopConfig> value) {
    return ioctlPtr(fd, LOOP_CONFIGURE, @unsafe.Pointer.FromPinnedBox(value));
}

public static error IoctlPidfdInfo(nint fd, ж<PidfdInfo> info) {
    return ioctlPtr(fd, PIDFD_GET_INFO, @unsafe.Pointer.FromPinnedBox(info));
}

} // end unix_package
