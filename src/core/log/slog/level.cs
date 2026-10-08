// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.log;

using errors = errors_package;
using fmt = fmt_package;
using strconv = strconv_package;
using strings = strings_package;
using atomic = go.sync.atomic_package;
using go.sync;

partial class slog_package {

partial struct ΔLevel /*num:nint*/;

// Names for common levels.
//
// Level numbers are inherently arbitrary,
// but we picked them to satisfy three constraints.
// Any system can map them to another numbering scheme if it wishes.
//
// First, we wanted the default level to be Info, Since Levels are ints, Info is
// the default value for int, zero.
//
// Second, we wanted to make it easy to use levels to specify logger verbosity.
// Since a larger level means a more severe event, a logger that accepts events
// with smaller (or more negative) level means a more verbose logger. Logger
// verbosity is thus the negation of event severity, and the default verbosity
// of 0 accepts all events at least as severe as INFO.
//
// Third, we wanted some room between levels to accommodate schemes with named
// levels between ours. For example, Google Cloud Logging defines a Notice level
// between Info and Warn. Since there are only a few of these intermediate
// levels, the gap between the numbers need not be large. Our gap of 4 matches
// OpenTelemetry's mapping. Subtracting 9 from an OpenTelemetry level in the
// DEBUG, INFO, WARN and ERROR ranges converts it to the corresponding slog
// Level range. OpenTelemetry also has the names TRACE and FATAL, which slog
// does not. But those OpenTelemetry levels can still be represented as slog
// Levels by using the appropriate integers.
public static ΔLevel LevelDebug => -4;

public static ΔLevel LevelInfo => 0;

public static ΔLevel LevelWarn => 4;

public static ΔLevel LevelError => 8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string debugˢ = "DEBUG"u8;
internal static readonly @string infoˢ = "INFO"u8;
internal static readonly @string warnˢ = "WARN"u8;
internal static readonly @string errorˢ = "ERROR"u8;

// String returns a name for the level.
// If the level has a name, then that name
// in uppercase is returned.
// If the level is between named values, then
// an integer is appended to the uppercased name.
// Examples:
//
//	LevelWarn.String() => "WARN"
//	(LevelInfo+2).String() => "INFO+2"
public static @string String(this ΔLevel l) {
    @string str(@string @base, ΔLevel val) {
        if (val == 0) {
            return @base;
        }
        return fmt.Sprintf("%s%+d"u8, @base, val);
    }
    switch (ᐧ) {
    case {} when l < LevelInfo: {
        return str(debugˢ, l - LevelDebug);
    }
    case {} when l < LevelWarn: {
        return str(infoˢ, l - LevelInfo);
    }
    case {} when l < LevelError: {
        return str(warnˢ, l - LevelWarn);
    }
    default: {
        return str(errorˢ, l - LevelError);
    }}

}

// MarshalJSON implements [encoding/json.Marshaler]
// by quoting the output of [Level.String].
public static (slice<byte>, error) MarshalJSON(this ΔLevel l) {
    // AppendQuote is sufficient for JSON-encoding all Level strings.
    // They don't contain any runes that would produce invalid JSON
    // when escaped.
    return (strconv.AppendQuote(default!, l.String()), default!);
}

// UnmarshalJSON implements [encoding/json.Unmarshaler]
// It accepts any string produced by [Level.MarshalJSON],
// ignoring case.
// It also accepts numeric offsets that would result in a different string on
// output. For example, "Error-8" would marshal as "INFO".
public static error UnmarshalJSON(this ж<ΔLevel> Ꮡl, slice<byte> data) {
    var (s, err) = strconv.Unquote(((@string)data));
    if (err != default!) {
        return err;
    }
    return Ꮡl.parse(s);
}

// AppendText implements [encoding.TextAppender]
// by calling [Level.String].
public static (slice<byte>, error) AppendText(this ΔLevel l, slice<byte> b) {
    return (builtin.append(b, l.String().ꓸꓸꓸ), default!);
}

// MarshalText implements [encoding.TextMarshaler]
// by calling [Level.AppendText].
public static (slice<byte>, error) MarshalText(this ΔLevel l) {
    return l.AppendText(default!);
}

// UnmarshalText implements [encoding.TextUnmarshaler].
// It accepts any string produced by [Level.MarshalText],
// ignoring case.
// It also accepts numeric offsets that would result in a different string on
// output. For example, "Error-8" would marshal as "INFO".
public static error UnmarshalText(this ж<ΔLevel> Ꮡl, slice<byte> data) {
    return Ꮡl.parse(((@string)data));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unknownNameˢ = "unknown name"u8;

internal static error /*err*/ parse(this ж<ΔLevel> Ꮡl, @string s) {
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var l = ref Ꮡl.DerefOrNull();

        defer(() => {
            if (err != default!) {
                err = fmt.Errorf("slog: level string %q: %w"u8, s, err);
            }
        }, ref ᒐ);
        @string name = s;
        nint offset = 0;
        {
            nint i = strings.IndexAny(s, "+-"u8); if (i >= 0) {
                name = s.slice(0, i);
                (offset, err) = strconv.Atoi(s.slice(i));
                if (err != default!) {
                    goto ᒐdone;
                }
            }
        }
        var exprᴛ1 = strings.ToUpper(name);
        if (exprᴛ1 == "DEBUG"u8) {
            l = LevelDebug;
        }
        else if (exprᴛ1 == "INFO"u8) {
            l = LevelInfo;
        }
        else if (exprᴛ1 == "WARN"u8) {
            l = LevelWarn;
        }
        else if (exprᴛ1 == "ERROR"u8) {
            l = LevelError;
        }
        else { /* default: */
            err = errors.New(unknownNameˢ); goto ᒐdone;
        }

        l += ((ΔLevel)offset);
        err = default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return err;
}

// Level returns the receiver.
// It implements [Leveler].
public static ΔLevel Level(this ΔLevel l) {
    return l;
}

// A LevelVar is a [Level] variable, to allow a [Handler] level to change
// dynamically.
// It implements [Leveler] as well as a Set method,
// and it is safe for use by multiple goroutines.
// The zero LevelVar corresponds to [LevelInfo].
partial struct LevelVar {
    internal atomic.Int64 val;
}

// Level returns v's level.
public static ΔLevel Level(this ж<LevelVar> Ꮡv) {
    return ((ΔLevel)(nint)Ꮡv.of(LevelVar.Ꮡval).Load());
}

// Set sets v's level to l.
public static void Set(this ж<LevelVar> Ꮡv, ΔLevel l) {
    Ꮡv.of(LevelVar.Ꮡval).Store((int64)(nint)l);
}

public static @string String(this ж<LevelVar> Ꮡv) {
    return fmt.Sprintf("LevelVar(%s)"u8, Ꮡv.Level());
}

// AppendText implements [encoding.TextAppender]
// by calling [Level.AppendText].
public static (slice<byte>, error) AppendText(this ж<LevelVar> Ꮡv, slice<byte> b) {
    return Ꮡv.Level().AppendText(b);
}

// MarshalText implements [encoding.TextMarshaler]
// by calling [LevelVar.AppendText].
public static (slice<byte>, error) MarshalText(this ж<LevelVar> Ꮡv) {
    return Ꮡv.AppendText(default!);
}

// UnmarshalText implements [encoding.TextUnmarshaler]
// by calling [Level.UnmarshalText].
public static error UnmarshalText(this ж<LevelVar> Ꮡv, slice<byte> data) {
    ref var l = ref heap(new ΔLevel(), out var Ꮡl);
    {
        var err = Ꮡl.UnmarshalText(data); if (err != default!) {
            return err;
        }
    }
    Ꮡv.Set(l);
    return default!;
}

// A Leveler provides a [Level] value.
//
// As Level itself implements Leveler, clients typically supply
// a Level value wherever a Leveler is needed, such as in [HandlerOptions].
// Clients who need to vary the level dynamically can provide a more complex
// Leveler implementation such as *[LevelVar].
partial interface Leveler {
    ΔLevel Level();
}

} // end slog_package
