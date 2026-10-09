// Copyright 2016 Google Inc.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.github.com.google;

using fmt = fmt_package;

partial class uuid_package {

public static (slice<byte>, error) MarshalText(this UUID uuid) {
    uuid = uuid.Clone();

    array<byte> js = new(36);
    encodeHex(js[..], uuid);
    return (js[..], default!);
}

public static error UnmarshalText(this ref UUID uuid, slice<byte> data) {
    var (id, err) = ParseBytes(data);
    if (err != default!) {
        return err;
    }
    uuid = id.Clone();
    return default!;
}

public static (slice<byte>, error) MarshalBinary(this UUID uuid) {
    uuid = uuid.Clone();

    return (uuid[..], default!);
}

public static error UnmarshalBinary(this ref UUID uuid, slice<byte> data) {
    if (len(data) != 16) {
        return fmt.Errorf("invalid UUID (got %d bytes)"u8, len(data));
    }
    copy(uuid.Value[..], data);
    return default!;
}

} // end uuid_package
