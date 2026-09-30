// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.log;

using fmt = fmt_package;
using math = math_package;
using runtime = runtime_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using time = time_package;
using @unsafe = unsafe_package;
using System.Runtime.CompilerServices;
using io = io_package;
using ꓸꓸꓸAttr = Span<slog_package.Attr>;

partial class slog_package {

// A Value can represent any Go value, but unlike type any,
// it can represent most small values without an allocation.
// The zero Value corresponds to nil.
[GoType] partial struct Value {
    internal array<Action> _ = new(0); // disallow ==
    // num holds the value for Kinds Int64, Uint64, Float64, Bool and Duration,
    // the string length for KindString, and nanoseconds since the epoch for KindTime.
    internal uint64 num;
    // If any is of type Kind, then the value is in num as described above.
    // If any is of type *time.Location, then the Kind is Time and time.Time value
    // can be constructed from the Unix nanos in num and the location (monotonic time
    // is not preserved).
    // If any is of type stringptr, then the Kind is String and the string value
    // consists of the length in num and the pointer in any.
    // Otherwise, the Kind is Any and any is the value.
    // (This implies that Attrs cannot store values of type Kind, *time.Location
    // or stringptr.)
    internal any any;
}

[GoType("ж<byte>")] partial class stringptr;

[GoType("ж<Attr>")] partial class groupptr;

[GoType("num:nint")] partial struct ΔKind;

// The following list is sorted alphabetically, but it's also important that
// KindAny is 0 so that a zero Value represents nil.
public static ΔKind KindAny => /* iota */ 0;
public static ΔKind KindBool => 1;
public static ΔKind KindDuration => 2;
public static ΔKind KindFloat64 => 3;
public static ΔKind KindInt64 => 4;
public static ΔKind KindString => 5;
public static ΔKind KindTime => 6;
public static ΔKind KindUint64 => 7;
public static ΔKind KindGroup => 8;
public static ΔKind KindLogValuer => 9;

internal static slice<@string> kindStrings = new @string[]{
    "Any"u8,
    "Bool"u8,
    "Duration"u8,
    "Float64"u8,
    "Int64"u8,
    "String"u8,
    "Time"u8,
    "Uint64"u8,
    "Group"u8,
    "LogValuer"u8
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unknownSlogKindˢ = "<unknown slog.Kind>"u8;

public static @string String(this ΔKind k) {
    if (k >= 0 && (nint)k < len(kindStrings)) {
        return kindStrings[k];
    }
    return unknownSlogKindˢ;
}

[GoType("num:nint")] partial struct kind;

// Kind returns v's Kind.
public static ΔKind Kind(this Value v) {
    switch (v.any.type()) {
    case ΔKind x: {
        return x;
    }
    case stringptr x: {
        return KindString;
    }
    case timeLocation _:
    case timeTime _: {
        var x = v.any;
        return KindTime;
    }
    case groupptr x: {
        return KindGroup;
    }
    case {} Δx when Δx._<ΔLogValuer>(out var x): {
        return KindLogValuer;
    }
    case kind x: {
        return KindAny;
    }
    default: {
        var x = v.any;
        return KindAny;
    }}
}

// a kind is just a wrapper for a Kind
//////////////// Constructors

// StringValue returns a new [Value] for a string.
public static Value StringValue(@string value) {
    return new Value(num: (uint64)len(value), any: new stringptr(@unsafe.StringData(value)));
}

// IntValue returns a [Value] for an int.
public static Value IntValue(nint v) {
    return Int64Value((int64)v);
}

// Int64Value returns a [Value] for an int64.
public static Value Int64Value(int64 v) {
    return new Value(num: (uint64)v, any: KindInt64);
}

// Uint64Value returns a [Value] for a uint64.
public static Value Uint64Value(uint64 v) {
    return new Value(num: v, any: KindUint64);
}

// Float64Value returns a [Value] for a floating-point number.
public static Value Float64Value(float64 v) {
    return new Value(num: math.Float64bits(v), any: KindFloat64);
}

// BoolValue returns a [Value] for a bool.
public static Value BoolValue(bool v) {
    var u = (uint64)0;
    if (v) {
        u = 1;
    }
    return new Value(num: u, any: KindBool);
}

[GoType("ж<timeꓸLocation>")] partial class timeLocation;

[GoType("global::go.time_package.Time")] partial struct timeTime;

// TimeValue returns a [Value] for a [time.Time].
// It discards the monotonic portion.
public static Value TimeValue(time.Time v) {
    if (v.IsZero()) {
        // UnixNano on the zero time is undefined, so represent the zero time
        // with a nil *time.Location instead. time.Time.Location method never
        // returns nil, so a Value with any == timeLocation(nil) cannot be
        // mistaken for any other Value, time.Time or otherwise.
        return new Value(any: ((timeLocation)nil));
    }
    var nsec = v.UnixNano();
    var t = time_package.Unix(0, nsec);
    if (v.Equal(t)) {
        // UnixNano correctly represents the time, so use a zero-alloc representation.
        return new Value(num: (uint64)nsec, any: new timeLocation(v.Location()));
    }
    // Fall back to the general form.
    // Strip the monotonic portion to match the other representation.
    return new Value(any: ((timeTime)v.Round(0)));
}

// DurationValue returns a [Value] for a [time.Duration].
public static Value DurationValue(time.Duration v) {
    return new Value(num: (uint64)v.Nanoseconds(), any: KindDuration);
}

// GroupValue returns a new [Value] for a list of Attrs.
// The caller must not subsequently mutate the argument slice.
public static Value GroupValue(params ꓸꓸꓸAttr @asʗp) {
    var @as = @asʗp.slice();

    // Remove empty groups.
    // It is simpler overall to do this at construction than
    // to check each Group recursively for emptiness.
    {
        nint n = countEmptyGroups(@as); if (n > 0) {
            var as2 = new slice<Attr>(0, len(@as) - n);
            foreach (var (_, a) in @as) {
                if (!a.Value.isEmptyGroup()) {
                    as2 = builtin.append(as2, a);
                }
            }
            @as = as2;
        }
    }
    return new Value(num: (uint64)len(@as), any: new groupptr(@unsafe.SliceData(@as)));
}

// countEmptyGroups returns the number of empty group values in its argument.
internal static nint countEmptyGroups(slice<Attr> @as) {
    nint n = 0;
    foreach (var (_, a) in @as) {
        if (a.Value.isEmptyGroup()) {
            n++;
        }
    }
    return n;
}

// AnyValue returns a [Value] for the supplied value.
//
// If the supplied value is of type Value, it is returned
// unmodified.
//
// Given a value of one of Go's predeclared string, bool, or
// (non-complex) numeric types, AnyValue returns a Value of kind
// [KindString], [KindBool], [KindUint64], [KindInt64], or [KindFloat64].
// The width of the original numeric type is not preserved.
//
// Given a [time.Time] or [time.Duration] value, AnyValue returns a Value of kind
// [KindTime] or [KindDuration]. The monotonic time is not preserved.
//
// For nil, or values of all other types, including named types whose
// underlying type is numeric, AnyValue returns a value of kind [KindAny].
public static Value AnyValue(any v) {
    switch (v.type()) {
    case @string vΔ1: {
        return StringValue(vΔ1);
    }
    case nint vΔ1: {
        return Int64Value((int64)vΔ1);
    }
    case nuint vΔ1: {
        return Uint64Value((uint64)vΔ1);
    }
    case int64 vΔ1: {
        return Int64Value(vΔ1);
    }
    case uint64 vΔ1: {
        return Uint64Value(vΔ1);
    }
    case bool vΔ1: {
        return BoolValue(vΔ1);
    }
    case time_package.Duration vΔ1: {
        return DurationValue(vΔ1);
    }
    case time_package.Time vΔ1: {
        return TimeValue(vΔ1);
    }
    case uint8 vΔ1: {
        return Uint64Value((uint64)vΔ1);
    }
    case uint16 vΔ1: {
        return Uint64Value((uint64)vΔ1);
    }
    case uint32 vΔ1: {
        return Uint64Value((uint64)vΔ1);
    }
    case uintptr vΔ1: {
        return Uint64Value((uint64)vΔ1);
    }
    case int8 vΔ1: {
        return Int64Value((int64)vΔ1);
    }
    case int16 vΔ1: {
        return Int64Value((int64)vΔ1);
    }
    case int32 vΔ1: {
        return Int64Value((int64)vΔ1);
    }
    case float64 vΔ1: {
        return Float64Value(vΔ1);
    }
    case float32 vΔ1: {
        return Float64Value((float64)vΔ1);
    }
    case slice<Attr> vΔ1: {
        return GroupValue(vΔ1.ꓸꓸꓸ);
    }
    case ΔKind vΔ1: {
        return new Value(any: ((kind)(nint)vΔ1));
    }
    case Value vΔ1: {
        return vΔ1;
    }
    default: {
        var vΔ1 = v;
        return new Value(any: vΔ1);
    }}
}

//////////////// Accessors

// Any returns v's value as an any.
public static any Any(this Value v) {
    var exprᴛ1 = v.Kind();
    if (exprᴛ1 == KindAny) {
        {
            var (k, ok) = v.any._<kind>(ᐧ); if (ok) {
                return ((ΔKind)(nint)k);
            }
        }
        return v.any;
    }
    if (exprᴛ1 == KindLogValuer) {
        return v.any;
    }
    if (exprᴛ1 == KindGroup) {
        return v.group();
    }
    if (exprᴛ1 == KindInt64) {
        return (int64)v.num;
    }
    if (exprᴛ1 == KindUint64) {
        return v.num;
    }
    if (exprᴛ1 == KindFloat64) {
        return v.@float();
    }
    if (exprᴛ1 == KindString) {
        return v.str();
    }
    if (exprᴛ1 == KindBool) {
        return v.@bool();
    }
    if (exprᴛ1 == KindDuration) {
        return v.duration();
    }
    if (exprᴛ1 == KindTime) {
        return v.time();
    }
    { /* default: */
        throw panic(fmt.Sprintf("bad kind: %s"u8, v.Kind()));
    }

}

// String returns Value's value as a string, formatted like [fmt.Sprint]. Unlike
// the methods Int64, Float64, and so on, which panic if v is of the
// wrong kind, String never panics.
public static @string String(this Value v) {
    {
        var (sp, ok) = v.any._<stringptr>(ᐧ); if (ok) {
            return @unsafe.String(sp, v.num);
        }
    }
    slice<byte> buf = default!;
    return ((@string)v.append(buf));
}

internal static @string str(this Value v) {
    return @unsafe.String(v.any._<stringptr>(), v.num);
}

// Int64 returns v's value as an int64. It panics
// if v is not a signed integer.
public static int64 Int64(this Value v) {
    {
        ΔKind g = v.Kind();
        ΔKind w = KindInt64; if (g != w) {
            throw panic(fmt.Sprintf("Value kind is %s, not %s"u8, g, w));
        }
    }
    return (int64)v.num;
}

// Uint64 returns v's value as a uint64. It panics
// if v is not an unsigned integer.
public static uint64 Uint64(this Value v) {
    {
        ΔKind g = v.Kind();
        ΔKind w = KindUint64; if (g != w) {
            throw panic(fmt.Sprintf("Value kind is %s, not %s"u8, g, w));
        }
    }
    return v.num;
}

// Bool returns v's value as a bool. It panics
// if v is not a bool.
public static bool Bool(this Value v) {
    {
        ΔKind g = v.Kind();
        ΔKind w = KindBool; if (g != w) {
            throw panic(fmt.Sprintf("Value kind is %s, not %s"u8, g, w));
        }
    }
    return v.@bool();
}

internal static bool @bool(this Value v) {
    return v.num == 1;
}

// Duration returns v's value as a [time.Duration]. It panics
// if v is not a time.Duration.
public static time.Duration Duration(this Value v) {
    {
        ΔKind g = v.Kind();
        ΔKind w = KindDuration; if (g != w) {
            throw panic(fmt.Sprintf("Value kind is %s, not %s"u8, g, w));
        }
    }
    return v.duration();
}

internal static time.Duration duration(this Value v) {
    return ((time.Duration)(int64)v.num);
}

// Float64 returns v's value as a float64. It panics
// if v is not a float64.
public static float64 Float64(this Value v) {
    {
        ΔKind g = v.Kind();
        ΔKind w = KindFloat64; if (g != w) {
            throw panic(fmt.Sprintf("Value kind is %s, not %s"u8, g, w));
        }
    }
    return v.@float();
}

internal static float64 @float(this Value v) {
    return math.Float64frombits(v.num);
}

// Time returns v's value as a [time.Time]. It panics
// if v is not a time.Time.
public static time.Time Time(this Value v) {
    {
        ΔKind g = v.Kind();
        ΔKind w = KindTime; if (g != w) {
            throw panic(fmt.Sprintf("Value kind is %s, not %s"u8, g, w));
        }
    }
    return v.time();
}

// See TimeValue to understand how times are represented.
internal static time.Time time(this Value v) {
    switch (v.any.type()) {
    case timeLocation a: {
        if (a == nil) {
            return new time_package.Time(nil);
        }
        return time_package.Unix(0, (int64)v.num).In(a);
    }
    case timeTime a: {
        return ((time.Time)a);
    }
    default: {
        var a = v.any;
        throw panic(fmt.Sprintf("bad time type %T"u8, v.any));
        break;
    }}
}

// LogValuer returns v's value as a LogValuer. It panics
// if v is not a LogValuer.
public static ΔLogValuer LogValuer(this Value v) {
    return v.any._<ΔLogValuer>();
}

// Group returns v's value as a []Attr.
// It panics if v's [Kind] is not [KindGroup].
public static slice<Attr> Group(this Value v) {
    {
        var (sp, ok) = v.any._<groupptr>(ᐧ); if (ok) {
            return @unsafe.Slice((ж<Attr>)(sp), v.num);
        }
    }
    throw panic("Group: bad kind");
}

internal static slice<Attr> group(this Value v) {
    return @unsafe.Slice((ж<Attr>)(v.any._<groupptr>()), v.num);
}

//////////////// Other

// Equal reports whether v and w represent the same Go value.
public static bool Equal(this Value v, Value w) {
    ΔKind k1 = v.Kind();
    ΔKind k2 = w.Kind();
    if (k1 != k2) {
        return false;
    }
    var exprᴛ1 = k1;
    if (exprᴛ1 == KindInt64 || exprᴛ1 == KindUint64 || exprᴛ1 == KindBool || exprᴛ1 == KindDuration) {
        return v.num == w.num;
    }
    if (exprᴛ1 == KindString) {
        return v.str() == w.str();
    }
    if (exprᴛ1 == KindFloat64) {
        return v.@float() == w.@float();
    }
    if (exprᴛ1 == KindTime) {
        return v.time().Equal(w.time());
    }
    if (exprᴛ1 == KindAny || exprᴛ1 == KindLogValuer) {
        return AreEqual(v.any, w.any); // may panic if non-comparable
    }
    if (exprᴛ1 == KindGroup) {
        return slices.EqualFunc<slice<Attr>, slice<Attr>, Attr, Attr>(v.group(), w.group(), ((Func<Attr, Attr, bool>)(Equal)));
    }
    { /* default: */
        throw panic(fmt.Sprintf("bad kind: %s"u8, k1));
    }

}

// isEmptyGroup reports whether v is a group that has no attributes.
internal static bool isEmptyGroup(this Value v) {
    if (v.Kind() != KindGroup) {
        return false;
    }
    // We do not need to recursively examine the group's Attrs for emptiness,
    // because GroupValue removed them when the group was constructed, and
    // groups are immutable.
    return len(v.group()) == 0;
}

// append appends a text representation of v to dst.
// v is formatted as with fmt.Sprint.
internal static slice<byte> append(this Value v, slice<byte> dst) {
    var exprᴛ1 = v.Kind();
    if (exprᴛ1 == KindString) {
        return builtin.append(dst, v.str().ꓸꓸꓸ);
    }
    if (exprᴛ1 == KindInt64) {
        return strconv.AppendInt(dst, (int64)v.num, 10);
    }
    if (exprᴛ1 == KindUint64) {
        return strconv.AppendUint(dst, v.num, 10);
    }
    if (exprᴛ1 == KindFloat64) {
        return strconv.AppendFloat(dst, v.@float(), (rune)'g', -1, 64);
    }
    if (exprᴛ1 == KindBool) {
        return strconv.AppendBool(dst, v.@bool());
    }
    if (exprᴛ1 == KindDuration) {
        return builtin.append(dst, v.duration().String().ꓸꓸꓸ);
    }
    if (exprᴛ1 == KindTime) {
        return builtin.append(dst, v.time().String().ꓸꓸꓸ);
    }
    if (exprᴛ1 == KindGroup) {
        return fmt.Append(dst, v.group());
    }
    if (exprᴛ1 == KindAny || exprᴛ1 == KindLogValuer) {
        return fmt.Append(dst, v.any);
    }
    { /* default: */
        throw panic(fmt.Sprintf("bad kind: %s"u8, v.Kind()));
    }

}

// A LogValuer is any Go value that can convert itself into a Value for logging.
//
// This mechanism may be used to defer expensive operations until they are
// needed, or to expand a single value into a sequence of components.
[GoType] partial interface ΔLogValuer {
    Value LogValue();
}

internal static UntypedInt maxLogValues => 100;

// Resolve repeatedly calls LogValue on v while it implements [LogValuer],
// and returns the result.
// If v resolves to a group, the group's attributes' values are not recursively
// resolved.
// If the number of LogValue calls exceeds a threshold, a Value containing an
// error is returned.
// Resolve's return value is guaranteed not to be of Kind [KindLogValuer].
public static Value /*rv*/ Resolve(this Value v) {
    Value rv = default!;
    GoFrame ᒐ = default;
    try {
        var orig = v;
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    rv = AnyValue(fmt.Errorf("LogValue panicked\n%s"u8, stack(3, 5)));
                }
            }
        }, ref ᒐ);
        for (nint i = 0; i < maxLogValues; i++) {
            if (v.Kind() != KindLogValuer) {
                rv = v; goto ᒐdone;
            }
            v = v.LogValuer().LogValue();
        }
        var err = fmt.Errorf("LogValue called too many times on Value of type %T"u8, orig.Any());
        rv = AnyValue(err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return rv;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noStackˢ = "(no stack)"u8;

[MethodImpl(MethodImplOptions.NoInlining)] internal static @string stack(nint skip, nint nFrames) {
    var pcs = new slice<uintptr>(nFrames + 1);
    nint n = runtime.Callers(skip + 1, pcs);
    if (n == 0) {
        return noStackˢ;
    }
    var frames = runtime.CallersFrames(pcs.slice(0, n));
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    nint i = 0;
    while (ᐧ) {
        var (frame, more) = frames.Next();
        fmt.Fprintf(new strings_BuilderжWriter(Ꮡb), "called from %s (%s:%d)\n"u8, frame.Function, frame.File, frame.Line);
        if (!more) {
            break;
        }
        i++;
        if (i >= nFrames) {
            fmt.Fprintf(new strings_BuilderжWriter(Ꮡb), "(rest of stack elided)\n"u8);
            break;
        }
    }
    return b.String();
}

} // end slog_package
