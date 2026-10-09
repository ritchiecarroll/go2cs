// Copyright 2016 Google Inc.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.github.com.google;

using binary = encoding.binary_package;
using fmt = fmt_package;
using os = os_package;
using encoding;

partial class uuid_package {

partial struct ΔDomain /*num:byte*/;

public static ΔDomain Person => /* Domain(0) */ 0;
public static ΔDomain Group => /* Domain(1) */ 1;
public static ΔDomain Org => /* Domain(2) */ 2;

public static (UUID, error) NewDCESecurity(ΔDomain domain, uint32 id) {
    var (uuid, err) = NewUUID();
    if (err == default!) {
        uuid[6] = (byte)(((byte)(uuid[6] & 0x0f)) | 0x20);
        uuid[9] = (byte)domain;
        binary.BigEndian.PutUint32(uuid[0..], id);
    }
    return (uuid.Clone(), err);
}

public static (UUID, error) NewDCEPerson() {
    return NewDCESecurity(Person, (uint32)os.Getuid());
}

public static (UUID, error) NewDCEGroup() {
    return NewDCESecurity(Group, (uint32)os.Getgid());
}

public static ΔDomain Domain(this UUID uuid) {
    uuid = uuid.Clone();

    return ((ΔDomain)uuid[9]);
}

public static uint32 ID(this UUID uuid) {
    uuid = uuid.Clone();

    return binary.BigEndian.Uint32(uuid[0..4]);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string personˢ = "Person"u8;
internal static readonly @string groupˢ = "Group"u8;
internal static readonly @string orgˢ = "Org"u8;

public static @string String(this ΔDomain d) {
    var exprᴛ1 = d;
    if (exprᴛ1 == Person) {
        return personˢ;
    }
    if (exprᴛ1 == Group) {
        return groupˢ;
    }
    if (exprᴛ1 == Org) {
        return orgˢ;
    }

    return fmt.Sprintf("Domain%d"u8, (nint)(byte)d);
}

} // end uuid_package
