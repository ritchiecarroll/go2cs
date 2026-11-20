// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using sql = database.sql_package;
using driver = database.sql.driver_package;
using errors = errors_package;
using fmt = fmt_package;
using reflect = reflect_package;
using strconv = strconv_package;
using time = time_package;
using database;
using database.sql;

partial class sqlite_package {

internal static error errNilPtr = errors.New("destination pointer is nil");

internal static error convertAssign(any dest, any src) {
    switch (src.type()) {
    case @string s: {
        switch (dest.type()) {
        case ж<@string> d: {
            if (d == nil) {
                return errNilPtr;
            }
            d.Value = s;
            return default;
        }
        case ж<slice<byte>> d: {
            if (d == nil) {
                return errNilPtr;
            }
            d.ValueSlot = slice<byte>(s);
            return default;
        }
        case ж<sql.RawBytes> d: {
            if (d == nil) {
                return errNilPtr;
            }
            d.ValueSlot = append((d.ValueSlot)[..0], s.ꓸꓸꓸ);
            return default;
        }}
        break;
    }
    case slice<byte> s: {
        switch (dest.type()) {
        case ж<@string> d: {
            if (d == nil) {
                return errNilPtr;
            }
            d.Value = (@string)s;
            return default;
        }
        case {} Δd when Δd._<ж<any>>(out var d): {
            if (d == nil) {
                return errNilPtr;
            }
            d.ValueSlot = cloneBytes(s);
            return default;
        }
        case ж<slice<byte>> d: {
            if (d == nil) {
                return errNilPtr;
            }
            d.ValueSlot = cloneBytes(s);
            return default;
        }
        case ж<sql.RawBytes> d: {
            if (d == nil) {
                return errNilPtr;
            }
            d.ValueSlot = s;
            return default;
        }}
        break;
    }
    case time.Time s: {
        switch (dest.type()) {
        case ж<time.Time> d: {
            d.Value = s;
            return default;
        }
        case ж<@string> d: {
            d.Value = s.Format(time.RFC3339Nano);
            return default;
        }
        case ж<slice<byte>> d: {
            if (d == nil) {
                return errNilPtr;
            }
            d.ValueSlot = slice<byte>(s.Format(time.RFC3339Nano));
            return default;
        }
        case ж<sql.RawBytes> d: {
            if (d == nil) {
                return errNilPtr;
            }
            d.ValueSlot = s.AppendFormat((d.ValueSlot)[..0], time.RFC3339Nano);
            return default;
        }}
        break;
    }
    case null: {
        switch (dest.type()) {
        case {} Δd when Δd._<ж<any>>(out var d): {
            if (d == nil) {
                return errNilPtr;
            }
            d.ValueSlot = default;
            return default;
        }
        case ж<slice<byte>> d: {
            if (d == nil) {
                return errNilPtr;
            }
            d.ValueSlot = default;
            return default;
        }
        case ж<sql.RawBytes> d: {
            if (d == nil) {
                return errNilPtr;
            }
            d.ValueSlot = default;
            return default;
        }}
        break;
    }}

    reflect.Value sv = new(nil);

    switch (dest.type()) {
    case ж<@string> d: {
        sv = reflect.ValueOf(src);
        switch (sv.Kind()) {
        case var ᴛ when ᴛ == reflect.Bool || ᴛ == reflect.Int || ᴛ == reflect.Int8 || ᴛ == reflect.Int16 || ᴛ == reflect.Int32 || ᴛ == reflect.Int64 || ᴛ == reflect.Uint || ᴛ == reflect.Uint8 || ᴛ == reflect.Uint16 || ᴛ == reflect.Uint32 || ᴛ == reflect.Uint64 || ᴛ == reflect.Float32 || ᴛ == reflect.Float64: {
            d.Value = asString(src);
            return default;
        }}

        break;
    }
    case ж<slice<byte>> d: {
        sv = reflect.ValueOf(src);
        {
            var (b, ok) = asBytes(default, sv); if (ok) {
                d.ValueSlot = b;
                return default;
            }
        }
        break;
    }
    case ж<sql.RawBytes> d: {
        sv = reflect.ValueOf(src);
        {
            var (b, ok) = asBytes(((slice<byte>)(d.ValueSlot))[..0], sv); if (ok) {
                d.ValueSlot = (sql.RawBytes)b;
                return default;
            }
        }
        break;
    }
    case ж<bool> d: {
        var (bv, err) = driver.Bool.ConvertValue(src);
        if (err == nil) {
            d.Value = bv._<bool>();
        }
        return err;
    }
    case {} Δd when Δd._<ж<any>>(out var d): {
        d.ValueSlot = src;
        return default;
    }}

    if (dest._<sql.Scanner>(out var scanner)) {
        return scanner.Scan(src);
    }

    var dpv = reflect.ValueOf(dest);
    if (dpv.Kind() != reflect.Ptr) {
        return errors.New("destination not a pointer");
    }
    if (dpv.IsNil()) {
        return errNilPtr;
    }

    if (!sv.IsValid()) {
        sv = reflect.ValueOf(src);
    }

    var dv = reflect.Indirect(dpv);
    if (sv.IsValid() && sv.Type().AssignableTo(dv.Type())) {
        switch (src.type()) {
        case slice<byte> b: {
            dv.Set(reflect.ValueOf(cloneBytes(b)));
            break;
        }
        default: {
            var b = src;
            dv.Set(sv);
            break;
        }}
        return default;
    }

    if (dv.Kind() == sv.Kind() && sv.Type().ConvertibleTo(dv.Type())) {
        dv.Set(sv.Convert(dv.Type()));
        return default;
    }

    switch (dv.Kind()) {
    case var ᴛ when ᴛ == reflect.Ptr: {
        if (src == nil) {
            dv.Set(reflect.Zero(dv.Type()));
            return default;
        }
        dv.Set(reflect.New(dv.Type().Elem()));
        return convertAssign(dv.Interface(), src);
    }
    case var ᴛ when ᴛ == reflect.Int || ᴛ == reflect.Int8 || ᴛ == reflect.Int16 || ᴛ == reflect.Int32 || ᴛ == reflect.Int64: {
        @string s = asString(src);
        var (i64, err) = strconv.ParseInt(s, 10, dv.Type().Bits());
        if (err != nil) {
            err = strconvErr(err);
            return fmt.Errorf("converting driver.Value type %T (%q) to a %s: %v"u8, src, s, dv.Kind(), err);
        }
        dv.SetInt(i64);
        return default;
    }
    case var ᴛ when ᴛ == reflect.Uint || ᴛ == reflect.Uint8 || ᴛ == reflect.Uint16 || ᴛ == reflect.Uint32 || ᴛ == reflect.Uint64: {
        @string s = asString(src);
        var (u64, err) = strconv.ParseUint(s, 10, dv.Type().Bits());
        if (err != nil) {
            err = strconvErr(err);
            return fmt.Errorf("converting driver.Value type %T (%q) to a %s: %v"u8, src, s, dv.Kind(), err);
        }
        dv.SetUint(u64);
        return default;
    }
    case var ᴛ when ᴛ == reflect.Float32 || ᴛ == reflect.Float64: {
        @string s = asString(src);
        var (f64, err) = strconv.ParseFloat(s, dv.Type().Bits());
        if (err != nil) {
            err = strconvErr(err);
            return fmt.Errorf("converting driver.Value type %T (%q) to a %s: %v"u8, src, s, dv.Kind(), err);
        }
        dv.SetFloat(f64);
        return default;
    }
    case var ᴛ when ᴛ == reflect.String: {
        switch (src.type()) {
        case @string v: {
            dv.SetString(v);
            return default;
        }
        case slice<byte> v: {
            dv.SetString((@string)v);
            return default;
        }}
        break;
    }}

    return fmt.Errorf("unsupported Scan, storing driver.Value type %T into type %T"u8, src, dest);
}

internal static error strconvErr(error err) {
    var (ne, ok) = err._<ж<strconv.NumError>>(ᐧ); if (ok) {
        return ne.Err;
    }
    return err;
}

internal static slice<byte> cloneBytes(slice<byte> b) {
    if (b == nil) {
        return default;
    }
    var c = new slice<byte>(len(b));
    copy(c, b);
    return c;
}

internal static @string asString(any src) {
    switch (src.type()) {
    case @string v: {
        return v;
    }
    case slice<byte> v: {
        return (@string)v;
    }}
    var rv = reflect.ValueOf(src);
    switch (rv.Kind()) {
    case var ᴛ when ᴛ == reflect.Int || ᴛ == reflect.Int8 || ᴛ == reflect.Int16 || ᴛ == reflect.Int32 || ᴛ == reflect.Int64: {
        return strconv.FormatInt(rv.Int(), 10);
    }
    case var ᴛ when ᴛ == reflect.Uint || ᴛ == reflect.Uint8 || ᴛ == reflect.Uint16 || ᴛ == reflect.Uint32 || ᴛ == reflect.Uint64: {
        return strconv.FormatUint(rv.Uint(), 10);
    }
    case var ᴛ when ᴛ == reflect.Float64: {
        return strconv.FormatFloat(rv.Float(), (byte)'g', -1, 64);
    }
    case var ᴛ when ᴛ == reflect.Float32: {
        return strconv.FormatFloat(rv.Float(), (byte)'g', -1, 32);
    }
    case var ᴛ when ᴛ == reflect.Bool: {
        return strconv.FormatBool(rv.Bool());
    }}

    return fmt.Sprintf("%v"u8, src);
}

internal static (slice<byte> b, bool ok) asBytes(slice<byte> buf, reflect.Value rv) {
    slice<byte> b = default;
    bool ok = default;

    switch (rv.Kind()) {
    case var ᴛ when ᴛ == reflect.Int || ᴛ == reflect.Int8 || ᴛ == reflect.Int16 || ᴛ == reflect.Int32 || ᴛ == reflect.Int64: {
        return (strconv.AppendInt(buf, rv.Int(), 10), true);
    }
    case var ᴛ when ᴛ == reflect.Uint || ᴛ == reflect.Uint8 || ᴛ == reflect.Uint16 || ᴛ == reflect.Uint32 || ᴛ == reflect.Uint64: {
        return (strconv.AppendUint(buf, rv.Uint(), 10), true);
    }
    case var ᴛ when ᴛ == reflect.Float32: {
        return (strconv.AppendFloat(buf, rv.Float(), (byte)'g', -1, 32), true);
    }
    case var ᴛ when ᴛ == reflect.Float64: {
        return (strconv.AppendFloat(buf, rv.Float(), (byte)'g', -1, 64), true);
    }
    case var ᴛ when ᴛ == reflect.Bool: {
        return (strconv.AppendBool(buf, rv.Bool()), true);
    }
    case var ᴛ when ᴛ == reflect.String: {
        @string s = rv.String();
        return (append(buf, s.ꓸꓸꓸ), true);
    }}

    return (b, ok);
}

} // end sqlite_package
