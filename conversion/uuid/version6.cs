// Copyright 2023 Google Inc.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.github.com.google;

using binary = encoding.binary_package;
using encoding;

partial class uuid_package {

public static (UUID, error) NewV6() {
    UUID uuid = default!;
    var (now, seq, err) = GetTime();
    if (err != default!) {
        return (uuid.Clone(), err);
    }
    binary.BigEndian.PutUint64(uuid[0..], (uint64)(int64)now);
    binary.BigEndian.PutUint16(uuid[8..], seq);
    uuid[6] = (byte)(0x60 | ((byte)(uuid[6] & 0x0F)));
    uuid[8] = (byte)(0x80 | ((byte)(uuid[8] & 0x3F)));
    ᏑnodeMu.Lock();
    if (nodeID == zeroID) {
        setNodeInterface(""u8);
    }
    copy(uuid[10..], nodeID[..]);
    ᏑnodeMu.Unlock();
    return (uuid.Clone(), default!);
}

} // end uuid_package
