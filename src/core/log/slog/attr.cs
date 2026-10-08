// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.log;

using time = time_package;
using ꓸꓸꓸany = Span<any>;

partial class slog_package {

// An Attr is a key-value pair.
partial struct Attr {
    public @string Key;
    public Value Value;
}

// String returns an Attr for a string value.
public static Attr String(@string key, @string value) {
    return new Attr(key, StringValue(value));
}

// Int64 returns an Attr for an int64.
public static Attr Int64(@string key, int64 value) {
    return new Attr(key, Int64Value(value));
}

// Int converts an int to an int64 and returns
// an Attr with that value.
public static Attr Int(@string key, nint value) {
    return Int64(key, (int64)value);
}

// Uint64 returns an Attr for a uint64.
public static Attr Uint64(@string key, uint64 v) {
    return new Attr(key, Uint64Value(v));
}

// Float64 returns an Attr for a floating-point number.
public static Attr Float64(@string key, float64 v) {
    return new Attr(key, Float64Value(v));
}

// Bool returns an Attr for a bool.
public static Attr Bool(@string key, bool v) {
    return new Attr(key, BoolValue(v));
}

// Time returns an Attr for a [time.Time].
// It discards the monotonic portion.
public static Attr Time(@string key, time.Time v) {
    return new Attr(key, TimeValue(v));
}

// Duration returns an Attr for a [time.Duration].
public static Attr Duration(@string key, time.Duration v) {
    return new Attr(key, DurationValue(v));
}

// Group returns an Attr for a Group [Value].
// The first argument is the key; the remaining arguments
// are converted to Attrs as in [Logger.Log].
//
// Use Group to collect several key-value pairs under a single
// key on a log line, or as the result of LogValue
// in order to log a single value as multiple Attrs.
public static Attr Group(@string key, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    return new Attr(key, GroupValue(argsToAttrSlice(args).ꓸꓸꓸ));
}

internal static slice<Attr> argsToAttrSlice(slice<any> args) {
    Attr attr = default!;
    slice<Attr> attrs = default!;
    while (len(args) > 0) {
        (attr, args) = argsToAttr(args);
        attrs = builtin.append(attrs, attr);
    }
    return attrs;
}

// Any returns an Attr for the supplied value.
// See [AnyValue] for how values are treated.
public static Attr Any(@string key, any value) {
    return new Attr(key, AnyValue(value));
}

// Equal reports whether a and b have equal keys and values.
public static bool Equal(this Attr a, Attr b) {
    return a.Key == b.Key && a.Value.Equal(b.Value);
}

public static @string String(this Attr a) {
    return a.Key + "="u8 + a.Value.String();
}

// isEmpty reports whether a has an empty key and a nil value.
// That can be written as Attr{} or Any("", nil).
internal static bool isEmpty(this Attr a) {
    return a.Key == ""u8 && a.Value.num == 0 && a.Value.any == default!;
}

} // end slog_package
