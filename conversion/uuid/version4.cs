// Copyright 2016 Google Inc.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.github.com.google;

using io = io_package;

partial class uuid_package {

public static UUID New() {
    var (ᴛ1, ᴛ2) = NewRandom();
    return Must(ᴛ1, ᴛ2);
}

public static @string NewString() {
    var (ᴛ3, ᴛ4) = NewRandom();
    return Must(ᴛ3, ᴛ4).String();
}

public static (UUID, error) NewRandom() {
    if (!poolEnabled) {
        return NewRandomFromReader(rander);
    }
    return newRandomFromPool();
}

public static (UUID, error) NewRandomFromReader(io.Reader r) {
    UUID uuid = default!;
    var (_, err) = io.ReadFull(r, uuid[..]);
    if (err != default!) {
        return (Nil.Clone(), err);
    }
    uuid[6] = (byte)(((byte)(uuid[6] & 0x0f)) | 0x40);
    uuid[8] = (byte)(((byte)(uuid[8] & 0x3f)) | 0x80);
    return (uuid.Clone(), default!);
}

internal static (UUID, error) newRandomFromPool() {
    UUID uuid = default!;
    ᏑpoolMu.Lock();
    if (poolPos == randPoolSize) {
        var (_, err) = io.ReadFull(rander, pool[..]);
        if (err != default!) {
            ᏑpoolMu.Unlock();
            return (Nil.Clone(), err);
        }
        poolPos = 0;
    }
    copy(uuid[..], pool.slice(poolPos, (poolPos + 16)));
    poolPos += 16;
    ᏑpoolMu.Unlock();
    uuid[6] = (byte)(((byte)(uuid[6] & 0x0f)) | 0x40);
    uuid[8] = (byte)(((byte)(uuid[8] & 0x3f)) | 0x80);
    return (uuid.Clone(), default!);
}

} // end uuid_package
