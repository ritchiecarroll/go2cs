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
using static slog_package;

partial class slog_package {

// A Level is the importance or severity of a log event.
// The higher the level, the more important or severe the event.
partial struct Level /*num:nint*/;

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
public static Level LevelDebug => -4;
public static Level LevelInfo => 0;
public static Level LevelWarn => 4;
public static Level LevelError => 8;

} partial class slog_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string debugˢᴍLevel => ref debugˢᵇᴍLevel;
private static readonly @string debugˢᵇᴍLevel = "DEBUG"u8;
internal static ref readonly @string infoˢᴍLevel => ref infoˢᵇᴍLevel;
private static readonly @string infoˢᵇᴍLevel = "INFO"u8;
internal static ref readonly @string warnˢᴍLevel => ref warnˢᵇᴍLevel;
private static readonly @string warnˢᵇᴍLevel = "WARN"u8;
internal static ref readonly @string errorˢᴍLevel => ref errorˢᵇᴍLevel;
private static readonly @string errorˢᵇᴍLevel = "ERROR"u8;

// String returns a name for the level.
// If the level has a name, then that name
// in uppercase is returned.
// If the level is between named values, then
// an integer is appended to the uppercased name.
// Examples:
//
//	LevelWarn.String() => "WARN"
//	(LevelInfo+2).String() => "INFO+2"
[GoMethodBody("String")] internal static @string Stringᴍ(Level l) {
    @string str(@string @base, Level val) {
        if (val == 0) {
            return @base;
        }
        @string sval = strconv.Itoa((nint)val);
        if (val > 0) {
            sval = "+"u8 + sval;
        }
        return @base + sval;
    }

    switch (ᐧ) {
    case {} when l < LevelInfo: {
        return str(debugˢᴍLevel, l - LevelDebug);
    }
    case {} when l < LevelWarn: {
        return str(infoˢᴍLevel, l - LevelInfo);
    }
    case {} when l < LevelError: {
        return str(warnˢᴍLevel, l - LevelWarn);
    }
    default: {
        return str(errorˢᴍLevel, l - LevelError);
    }}
}

// MarshalJSON implements [encoding/json.Marshaler]
// by quoting the output of [Level.String].
[GoMethodBody("MarshalJSON")] internal static (slice<byte>, error) MarshalJSONᴍ(Level l) {
    // AppendQuote is sufficient for JSON-encoding all Level strings.
    // They don't contain any runes that would produce invalid JSON
    // when escaped.
    return (strconv.AppendQuote(default, l.String()), default);
}

// UnmarshalJSON implements [encoding/json.Unmarshaler]
// It accepts any string produced by [Level.MarshalJSON],
// ignoring case.
// It also accepts numeric offsets that would result in a different string on
// output. For example, "Error-8" would marshal as "INFO".
[GoMethodBody("UnmarshalJSON")] internal static error UnmarshalJSONᴍ(ж<Level> Ꮡl, slice<byte> data) {
    var (s, err) = strconv.Unquote((@string)data);
    if (err != nil) {
        return err;
    }
    return Ꮡl.parse(s);
}

// AppendText implements [encoding.TextAppender]
// by calling [Level.String].
[GoMethodBody("AppendText")] internal static (slice<byte>, error) AppendTextᴍ(Level l, slice<byte> b) {
    return (append(b, l.String().ꓸꓸꓸ), default);
}

// MarshalText implements [encoding.TextMarshaler]
// by calling [Level.AppendText].
[GoMethodBody("MarshalText")] internal static (slice<byte>, error) MarshalTextᴍ(Level l) {
    return l.AppendText(default);
}

// UnmarshalText implements [encoding.TextUnmarshaler].
// It accepts any string produced by [Level.MarshalText],
// ignoring case.
// It also accepts numeric offsets that would result in a different string on
// output. For example, "Error-8" would marshal as "INFO".
[GoMethodBody("UnmarshalText")] internal static error UnmarshalTextᴍ(ж<Level> Ꮡl, slice<byte> data) {
    return Ꮡl.parse((@string)data);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unknownNameˢᴍLevel => ref unknownNameˢᵇᴍLevel;
private static readonly @string unknownNameˢᵇᴍLevel = "unknown name"u8;

[GoMethodBody("parse")] internal static error /*err*/ parseᴍ(ж<Level> Ꮡl, @string s) {
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var l = ref Ꮡl.DerefOrNull();

        defer(() => {
            if (err != nil) {
                err = fmt.Errorf("slog: level string %q: %w"u8, s, err);
            }
        }, ref ᒐ);

        @string name = s;
        nint offset = 0;
        nint i = strings.IndexAny(s, "+-"); if (i >= 0) {
            name = s.slice(0, i);
            (offset, err) = strconv.Atoi(s.slice(i));
            if (err != nil) {
                goto ᒐdone;
            }
        }
        var exprᴛ1 = strings.ToUpper(name);
        switch (ᐧ) {
        case {} when exprᴛ1 == "DEBUG"u8: {
            l = LevelDebug;
            break;
        }
        case {} when exprᴛ1 == "INFO"u8: {
            l = LevelInfo;
            break;
        }
        case {} when exprᴛ1 == "WARN"u8: {
            l = LevelWarn;
            break;
        }
        case {} when exprᴛ1 == "ERROR"u8: {
            l = LevelError;
            break;
        }
        default: {
            err = errors.New(unknownNameˢᴍLevel); goto ᒐdone;
        }}
        l += ((Level)offset);
        err = default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return err;
}

// Level returns the receiver.
// It implements [Leveler].
[GoMethodBody("Level")] internal static Level Levelᴍ(Level l) { return l; }

} partial class slog_package {

// A LevelVar is a [Level] variable, to allow a [Handler] level to change
// dynamically.
// It implements [Leveler] as well as a Set method,
// and it is safe for use by multiple goroutines.
// The zero LevelVar corresponds to [LevelInfo].
partial struct LevelVar {
    internal atomic.Int64 val;
}

} partial class slog_package {

// Level returns v's level.
[GoMethodBody("Level")] internal static Level Levelᴍ(ж<LevelVar> Ꮡv) {
    return (Level)(nint)Ꮡv.Ꮡval.Load();
}

// Set sets v's level to l.
[GoMethodBody("Set")] internal static void Setᴍ(ж<LevelVar> Ꮡv, Level l) {
    Ꮡv.Ꮡval.Store((int64)l);
}

[GoMethodBody("String")] internal static @string Stringᴍ(ж<LevelVar> Ꮡv) {
    return fmt.Sprintf("LevelVar(%s)"u8, Ꮡv.Level());
}

// AppendText implements [encoding.TextAppender]
// by calling [Level.AppendText].
[GoMethodBody("AppendText")] internal static (slice<byte>, error) AppendTextᴍ(ж<LevelVar> Ꮡv, slice<byte> b) {
    return Ꮡv.Level().AppendText(b);
}

// MarshalText implements [encoding.TextMarshaler]
// by calling [LevelVar.AppendText].
[GoMethodBody("MarshalText")] internal static (slice<byte>, error) MarshalTextᴍ(ж<LevelVar> Ꮡv) {
    return Ꮡv.AppendText(default);
}

// UnmarshalText implements [encoding.TextUnmarshaler]
// by calling [Level.UnmarshalText].
[GoMethodBody("UnmarshalText")] internal static error UnmarshalTextᴍ(ж<LevelVar> Ꮡv, slice<byte> data) {
    ref var l = ref heap(new Level(), out var Ꮡl);
    var err = Ꮡl.UnmarshalText(data); if (err != nil) {
        return err;
    }
    Ꮡv.Set(l);
    return default;
}

} partial class slog_package {

// A Leveler provides a [Level] value.
//
// As Level itself implements Leveler, clients typically supply
// a Level value wherever a Leveler is needed, such as in [HandlerOptions].
// Clients who need to vary the level dynamically can provide a more complex
// Leveler implementation such as *[LevelVar].
partial interface Leveler {
    Level Level();
}

} // end slog_package
