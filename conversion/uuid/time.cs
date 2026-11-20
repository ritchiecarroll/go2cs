// Copyright 2016 Google Inc.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.github.com.google;

using binary = encoding.binary_package;
using sync = sync_package;
using time = time_package;
using encoding;
using static uuid_package;

partial class uuid_package {

partial struct Time /*num:int64*/;

internal static UntypedInt lillian => 2299160;
internal static UntypedInt unix => 2440587;
internal static UntypedInt epoch => /* unix - lillian */ 141427;
internal static UntypedInt g1582 => /* epoch * 86400 */ 12219292800;
internal static UntypedInt g1582ns100 => /* g1582 * 10000000 */ 122192928000000000;

internal static readonly ж<sync.Mutex> ᏑtimeMu = new StaticBox<sync.Mutex>(default(sync.Mutex));
internal static ref sync.Mutex timeMu => ref ((StaticBox<sync.Mutex>)ᏑtimeMu).ValueSlot;
internal static uint64 lasttime;
internal static uint16 clockSeq;
internal static Func<time.Time> timeNow = time.Now;

} partial class uuidᴛTime {

public static (int64 sec, int64 nsec) UnixTime(this Time t) {
    int64 sec = default;
    int64 nsec = default;

    sec = (int64)(t - (int64)g1582ns100);
    nsec = (sec % 10000000) * 100;
    sec /= 10000000;
    return (sec, nsec);
}

} partial class uuid_package {

public static (Time, uint16, error) GetTime() {
    GoFrame ᒐ = default;
    try {
        defer(ᏑtimeMu.Unlock, ref ᒐ);
        ᏑtimeMu.Lock();
        return getTime();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static (Time, uint16, error) getTime() {
    var t = timeNow();

    if (clockSeq == 0) {
        setClockSequence(-1);
    }
    var now = (uint64)(t.UnixNano() / 100) + (uint64)g1582ns100;

    if (now <= lasttime) {
        clockSeq = (uint16)(((uint16)((clockSeq + 1) & 0x3fff)) | 0x8000);
    }
    lasttime = now;
    return ((Time)(int64)now, clockSeq, default);
}

public static nint ClockSequence() {
    GoFrame ᒐ = default;
    try {
        defer(ᏑtimeMu.Unlock, ref ᒐ);
        ᏑtimeMu.Lock();
        return clockSequence();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static nint clockSequence() {
    if (clockSeq == 0) {
        setClockSequence(-1);
    }
    return (nint)((uint16)(clockSeq & 0x3fff));
}

public static void SetClockSequence(nint seq) {
    GoFrame ᒐ = default;
    try {
        defer(ᏑtimeMu.Unlock, ref ᒐ);
        ᏑtimeMu.Lock();
        setClockSequence(seq);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void setClockSequence(nint seq) {
    if (seq == -1) {
        array<byte> b = new(2);
        randomBits(b[..]);
        seq = (nint)b[0] << 8 | (nint)b[1];
    }
    var oldSeq = clockSeq;
    clockSeq = (uint16)((uint16)(seq & 0x3fff) | 0x8000);
    if (oldSeq != clockSeq) {
        lasttime = 0;
    }
}

} partial class uuidᴛUUID {

public static Time Time(this UUID uuid) {
    uuid = uuid.Clone();

    Time t = default;
    switch (uuid.Version()) {
    case var ᴛ when ᴛ == (Version)(6): {
        var time = binary.BigEndian.Uint64(uuid[..8]);
        t = (Time)(int64)time;
        break;
    }
    case var ᴛ when ᴛ == (Version)(7): {
        var time = binary.BigEndian.Uint64(uuid[..8]);
        t = (Time)(int64)((time >> 16) * 10000 + (uint64)g1582ns100);
        break;
    }
    default: {
        var time = (int64)binary.BigEndian.Uint32(uuid[0..4]);
        time |= (int64)(((int64)binary.BigEndian.Uint16(uuid[4..6]) << 32));
        time |= (int64)(((int64)((uint16)(binary.BigEndian.Uint16(uuid[6..8]) & 0xfff)) << 48));
        t = (Time)time;
        break;
    }}

    return t;
}

public static nint ClockSequence(this UUID uuid) {
    uuid = uuid.Clone();

    return (nint)binary.BigEndian.Uint16(uuid[8..10]) & 0x3fff;
}

} // end uuidᴛUUID
