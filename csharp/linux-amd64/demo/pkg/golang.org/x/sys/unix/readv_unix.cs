// Copyright 2026 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using @unsafe = unsafe_package;

partial class unix_package {

internal static UntypedInt minIovec => 8;

internal static slice<Iovec> appendBytes(slice<Iovec> vecs, slice<slice<byte>> bs) {
    var vecsᴷ = new stackAppend<Iovec>(moves: true);

    foreach (var (_, b) in range(bs)) {
        Iovec v = default;
        v.SetLen(len(b));
        if (len(b) > 0) {
            v.Base = Ꮡ(b, 0);
        } else {
            v.Base = Ꮡ_zero.Reinterpret<uintptr, byte>();
        }
        vecs = append(vecsᴷ, vecs, v);
    }
    vecs = move2heap(vecsᴷ, vecs);
    return vecs;
}

internal static void writevRaceDetect(slice<Iovec> iovecs, nint n) {
    if (!raceenabled) {
        return;
    }
    for (nint i = 0; n > 0 && i < len(iovecs); i++) {
        nint m = min((nint)iovecs[i].Len, n);
        n -= m;
        if (m > 0) {
            raceReadRange(@unsafe.Pointer.FromPinnedBox(iovecs[i].Base), m);
        }
    }
}

internal static void readvRaceDetect(slice<Iovec> iovecs, nint n, error err) {
    if (!raceenabled) {
        return;
    }
    for (nint i = 0; n > 0 && i < len(iovecs); i++) {
        nint m = min((nint)iovecs[i].Len, n);
        n -= m;
        if (m > 0) {
            raceWriteRange(@unsafe.Pointer.FromPinnedBox(iovecs[i].Base), m);
        }
    }
    if (err == nil) {
        raceAcquire(@unsafe.Pointer.FromPinnedBox(ᏑioSync));
    }
}

public static (nint n, error err) Readv(nint fd, slice<slice<byte>> iovs) {
    nint n = default;
    error err = default;

    var iovecs = new slice<Iovec>(0, minIovec);
    iovecs = appendBytes(iovecs, iovs);
    (n, err) = readv(fd, iovecs);
    readvRaceDetect(iovecs, n, err);
    return (n, err);
}

public static (nint n, error err) Preadv(nint fd, slice<slice<byte>> iovs, int64 offset) {
    nint n = default;
    error err = default;

    var iovecs = new slice<Iovec>(0, minIovec);
    iovecs = appendBytes(iovecs, iovs);
    (n, err) = preadv(fd, iovecs, offset);
    readvRaceDetect(iovecs, n, err);
    return (n, err);
}

public static (nint n, error err) Writev(nint fd, slice<slice<byte>> iovs) {
    nint n = default;
    error err = default;

    var iovecs = new slice<Iovec>(0, minIovec);
    iovecs = appendBytes(iovecs, iovs);
    if (raceenabled) {
        raceReleaseMerge(@unsafe.Pointer.FromPinnedBox(ᏑioSync));
    }
    (n, err) = writev(fd, iovecs);
    writevRaceDetect(iovecs, n);
    return (n, err);
}

public static (nint n, error err) Pwritev(nint fd, slice<slice<byte>> iovs, int64 offset) {
    nint n = default;
    error err = default;

    var iovecs = new slice<Iovec>(0, minIovec);
    iovecs = appendBytes(iovecs, iovs);
    if (raceenabled) {
        raceReleaseMerge(@unsafe.Pointer.FromPinnedBox(ᏑioSync));
    }
    (n, err) = pwritev(fd, iovecs, offset);
    writevRaceDetect(iovecs, n);
    return (n, err);
}

} // end unix_package
