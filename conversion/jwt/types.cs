namespace go.github.com.golang_jwt.jwt;

using json = encoding.json_package;
using fmt = fmt_package;
using math = math_package;
using strconv = strconv_package;
using time = time_package;
using encoding;

partial class jwt_package {

public static time.Duration TimePrecision = time.ΔSecond;

public static bool MarshalSingleStringAsArray = true;

partial struct NumericDate {
    public partial ref time_package.Time Time { get; }
}

public static ж<NumericDate> NewNumericDate(time.Time t) {
    return Ꮡ(new NumericDate(t.Truncate(TimePrecision)));
}

internal static ж<NumericDate> newNumericDateFromSeconds(float64 f) {
    var (round, frac) = math.Modf(f);
    return NewNumericDate(time.Unix((int64)round, (int64)(frac * 1e9D)));
}

public static (slice<byte> b, error err) MarshalJSON(this NumericDate date) {
    nint prec = default!;
    if (TimePrecision < time.ΔSecond) {
        prec = (nint)math.Log10((float64)(int64)time.ΔSecond / (float64)(int64)TimePrecision);
    }
    var truncatedDate = date.Time.Truncate(TimePrecision);
    @string seconds = strconv.FormatInt(truncatedDate.Unix(), 10);
    @string nanosecondsOffset = strconv.FormatFloat((float64)truncatedDate.Nanosecond() / (float64)(int64)time.ΔSecond, (rune)'f', prec, 64);
    var output = appendꓸꓸꓸ(slice<byte>(seconds), slice<byte>(nanosecondsOffset)[1..]);
    return (output, default!);
}

public static error /*err*/ UnmarshalJSON(this ref NumericDate date, slice<byte> b) {
    error err = default!;

    ref var number = ref heap(new json.Number(), out var Ꮡnumber);
    float64 f = default!;
    {
        err = json.Unmarshal(b, Ꮡnumber); if (err != default!) {
            return fmt.Errorf("could not parse NumericData: %w"u8, err);
        }
    }
    {
        (f, err) = number.Float64(); if (err != default!) {
            return fmt.Errorf("could not convert json number value to float: %w"u8, err);
        }
    }
    var n = newNumericDateFromSeconds(f);
    date = n.Value;
    return default!;
}

partial struct ClaimStrings /*[]@string*/;

public static error /*err*/ UnmarshalJSON(this ref ClaimStrings s, slice<byte> data) {
    error err = default!;

    ref var value = ref heap<any>(out var Ꮡvalue);
    {
        err = json.Unmarshal(data, Ꮡvalue); if (err != default!) {
            return err;
        }
    }
    slice<@string> aud = default!;
    switch (value.type()) {
    case @string v: {
        aud = append(aud, v);
        break;
    }
    case slice<@string> v: {
        aud = ((ClaimStrings)v);
        break;
    }
    case slice<any> v: {
        foreach (var (_, vv) in v) {
            var (vs, ok) = vv._<@string>(ᐧ);
            if (!ok) {
                return ErrInvalidType;
            }
            aud = append(aud, vs);
        }
        break;
    }
    case null: {
        return default!;
    }
    default: {
        var v = value;
        return ErrInvalidType;
    }}
    s = aud;
    return err;
}

public static (slice<byte> b, error err) MarshalJSON(this ClaimStrings s) {
    if (len(s) == 1 && !MarshalSingleStringAsArray) {
        return json.Marshal(s[0]);
    }
    return json.Marshal(((slice<@string>)s));
}

} // end jwt_package
