// Copyright 2016 Google Inc.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.github.com.google;

using binary = encoding.binary_package;
using encoding;

partial class uuid_package {

public static (UUID, error) NewUUID() {
    UUID uuid = default!;
    var (now, seq, err) = GetTime();
    if (err != default!) {
        return (uuid.Clone(), err);
    }
    var timeLow = (uint32)(int64)((ΔTime)(now & 0xffffffffL));
    var timeMid = (uint16)(int64)((ΔTime)(((now >> (int)(32))) & 0xffff));
    var timeHi = (uint16)(int64)((ΔTime)(((now >> (int)(48))) & 0x0fff));
    timeHi |= (uint16)(0x1000);
    binary.BigEndian.PutUint32(uuid[0..], timeLow);
    binary.BigEndian.PutUint16(uuid[4..], timeMid);
    binary.BigEndian.PutUint16(uuid[6..], timeHi);
    binary.BigEndian.PutUint16(uuid[8..], seq);
    ᏑnodeMu.Lock();
    if (nodeID == zeroID) {
        setNodeInterface(""u8);
    }
    copy(uuid[10..], nodeID[..]);
    ᏑnodeMu.Unlock();
    return (uuid.Clone(), default!);
}

} // end uuid_package
