// Copyright 2023 Google Inc.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.github.com.google;

using io = io_package;
using time = time_package;

partial class uuid_package {

public static (UUID, error) NewV7() {
    var (uuid, err) = NewRandom();
    if (err != default!) {
        return (uuid.Clone(), err);
    }
    makeV7(uuid[..]);
    return (uuid.Clone(), default!);
}

public static (UUID, error) NewV7FromReader(io.Reader r) {
    var (uuid, err) = NewRandomFromReader(r);
    if (err != default!) {
        return (uuid.Clone(), err);
    }
    makeV7(uuid[..]);
    return (uuid.Clone(), default!);
}

internal static void makeV7(slice<byte> uuid) {
    _ = uuid[15];
    var (t, s) = getV7Time();
    uuid[0] = (byte)((t >> (int)(40)));
    uuid[1] = (byte)((t >> (int)(32)));
    uuid[2] = (byte)((t >> (int)(24)));
    uuid[3] = (byte)((t >> (int)(16)));
    uuid[4] = (byte)((t >> (int)(8)));
    uuid[5] = (byte)t;
    uuid[6] = (byte)(0x70 | ((byte)(0x0F & (byte)((s >> (int)(8))))));
    uuid[7] = (byte)s;
}

internal static int64 lastV7time;

internal static UntypedInt nanoPerMilli => 1000000;

internal static (int64 milli, int64 seq) getV7Time() {
    int64 milli = default!;
    int64 seq = default!;
    GoFrame ᒐ = default;
    try {
        ᏑtimeMu.Lock();
        defer(ᏑtimeMu.Unlock, ref ᒐ);
        var nano = timeNow().UnixNano();
        milli = nano / (int64)nanoPerMilli;
        seq = ((nano - milli * (int64)nanoPerMilli) >> (int)(8));
        var now = (milli << (int)(12)) + seq;
        if (now <= lastV7time) {
            now = lastV7time + 1;
            milli = (now >> (int)(12));
            seq = (int64)(now & 0xfff);
        }
        lastV7time = now;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (milli, seq);
}

} // end uuid_package
