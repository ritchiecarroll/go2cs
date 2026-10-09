// Copyright 2021 Google Inc.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.github.com.google;

using bytes = bytes_package;
using driver = database.sql.driver_package;
using json = encoding.json_package;
using fmt = fmt_package;
using database.sql;
using encoding;

partial class uuid_package {

internal static slice<byte> jsonNull = slice<byte>("null"u8);

partial struct NullUUID {
    public UUID UUID;
    public bool Valid;
}

public static error Scan(this ref NullUUID nu, any value) {
    if (value == default!) {
        (nu.UUID, nu.Valid) = (Nil.Clone(), false);
        return default!;
    }
    var err = nu.UUID.Scan(value);
    if (err != default!) {
        nu.Valid = false;
        return err;
    }
    nu.Valid = true;
    return default!;
}

public static (driverꓸValue, error) Value(this NullUUID nu) {
    nu = nu.ΔClone();

    if (!nu.Valid) {
        return (default!, default!);
    }
    return nu.UUID.Value();
}

public static (slice<byte>, error) MarshalBinary(this NullUUID nu) {
    nu = nu.ΔClone();

    if (nu.Valid) {
        return (nu.UUID[..], default!);
    }
    return (slice<byte>(default!), default!);
}

public static error UnmarshalBinary(this ref NullUUID nu, slice<byte> data) {
    if (len(data) != 16) {
        return fmt.Errorf("invalid UUID (got %d bytes)"u8, len(data));
    }
    copy(nu.UUID[..], data);
    nu.Valid = true;
    return default!;
}

public static (slice<byte>, error) MarshalText(this NullUUID nu) {
    nu = nu.ΔClone();

    if (nu.Valid) {
        return nu.UUID.MarshalText();
    }
    return (jsonNull, default!);
}

public static error UnmarshalText(this ref NullUUID nu, slice<byte> data) {
    var (id, err) = ParseBytes(data);
    if (err != default!) {
        nu.Valid = false;
        return err;
    }
    nu.UUID = id.Clone();
    nu.Valid = true;
    return default!;
}

public static (slice<byte>, error) MarshalJSON(this NullUUID nu) {
    nu = nu.ΔClone();

    if (nu.Valid) {
        return json.Marshal(nu.UUID);
    }
    return (jsonNull, default!);
}

public static error UnmarshalJSON(this ж<NullUUID> Ꮡnu, slice<byte> data) {
    ref var nu = ref Ꮡnu.DerefOrNull();

    if (bytes.Equal(data, jsonNull)) {
        nu = new NullUUID(nil);
        return default!;
    }
    var err = json.Unmarshal(data, Ꮡnu.of(NullUUID.ᏑUUID));
    nu.Valid = err == default!;
    return err;
}

} // end uuid_package
