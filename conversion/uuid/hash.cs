// Copyright 2016 Google Inc.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.github.com.google;

using md5 = crypto.md5_package;
using sha1 = crypto.sha1_package;
using hash = hash_package;
using crypto;

partial class uuid_package {

public static UUID NameSpaceDNS;
internal static void initᴛNameSpaceDNS() {
    var (ᴛ1, ᴛ2) = Parse("6ba7b810-9dad-11d1-80b4-00c04fd430c8"u8);
    NameSpaceDNS = Must(ᴛ1, ᴛ2);
}
public static UUID NameSpaceURL;
internal static void initᴛNameSpaceURL() {
    var (ᴛ3, ᴛ4) = Parse("6ba7b811-9dad-11d1-80b4-00c04fd430c8"u8);
    NameSpaceURL = Must(ᴛ3, ᴛ4);
}
public static UUID NameSpaceOID;
internal static void initᴛNameSpaceOID() {
    var (ᴛ5, ᴛ6) = Parse("6ba7b812-9dad-11d1-80b4-00c04fd430c8"u8);
    NameSpaceOID = Must(ᴛ5, ᴛ6);
}
public static UUID NameSpaceX500;
internal static void initᴛNameSpaceX500() {
    var (ᴛ7, ᴛ8) = Parse("6ba7b814-9dad-11d1-80b4-00c04fd430c8"u8);
    NameSpaceX500 = Must(ᴛ7, ᴛ8);
}
public static UUID Nil;
public static UUID Max = new UUID(new byte[]{
    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
}.array());

public static UUID NewHash(hash.Hash h, UUID space, slice<byte> data, nint version) {
    space = space.Clone();

    h.Reset();
    h.Write(space[..]);
    h.Write(data);
    var s = h.Sum(default!);
    UUID uuid = default!;
    copy(uuid[..], s);
    uuid[6] = (byte)(((byte)(uuid[6] & 0x0f)) | (uint8)((((nint)(version & 0xf)) << (int)(4))));
    uuid[8] = (byte)(((byte)(uuid[8] & 0x3f)) | 0x80);
    return uuid.Clone();
}

public static UUID NewMD5(UUID space, slice<byte> data) {
    space = space.Clone();

    return NewHash(md5.New(), space, data, 3);
}

public static UUID NewSHA1(UUID space, slice<byte> data) {
    space = space.Clone();

    return NewHash(sha1.New(), space, data, 5);
}

} // end uuid_package
