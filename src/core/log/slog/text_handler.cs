// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.log;

using context = context_package;
using encoding = encoding_package;
using fmt = fmt_package;
using io = io_package;
using reflect = reflect_package;
using strconv = strconv_package;
using sync = sync_package;
using unicode = unicode_package;
using utf8 = go.unicode.utf8_package;
using buffer = go.log.slog.@internal.buffer_package;
using go.log.slog.@internal;
using go.unicode;
using time = time_package;

partial class slog_package {

// TextHandler is a [Handler] that writes Records to an [io.Writer] as a
// sequence of key=value pairs separated by spaces and followed by a newline.
[GoType] partial struct TextHandler {
    internal partial ref ж<commonHandler> commonHandler { get; }
}

// NewTextHandler creates a [TextHandler] that writes to w,
// using the given options.
// If opts is nil, the default options are used.
public static ж<TextHandler> NewTextHandler(io.Writer w, ж<HandlerOptions> Ꮡopts) {
    ref var opts = ref Ꮡopts.DerefOrNull();

    if (Ꮡopts == nil) {
        Ꮡopts = Ꮡ(new HandlerOptions(nil)); opts = ref Ꮡopts.DerefOrNull();
    }
    return Ꮡ(new TextHandler(
        Ꮡ(new commonHandler(
            json: false,
            w: w,
            opts: opts,
            mu: Ꮡ(new sync.Mutex(nil))
        ))
    ));
}

// Enabled reports whether the handler handles records at the given level.
// The handler ignores records whose level is lower.
[GoRecv] public static bool Enabled(this ref TextHandler h, context.Context _, ΔLevel level) {
    return h.commonHandler.enabled(level);
}

// WithAttrs returns a new [TextHandler] whose attributes consists
// of h's attributes followed by attrs.
[GoRecv] public static ΔHandler WithAttrs(this ref TextHandler h, slice<Attr> attrs) {
    return new TextHandlerжΔHandler(Ꮡ(new TextHandler(commonHandler: h.commonHandler.withAttrs(attrs))));
}

[GoRecv] public static ΔHandler WithGroup(this ref TextHandler h, @string name) {
    return new TextHandlerжΔHandler(Ꮡ(new TextHandler(commonHandler: h.commonHandler.withGroup(name))));
}

// Handle formats its argument [Record] as a single line of space-separated
// key=value items.
//
// If the Record's time is zero, the time is omitted.
// Otherwise, the key is "time"
// and the value is output in RFC3339 format with millisecond precision.
//
// If the Record's level is zero, the level is omitted.
// Otherwise, the key is "level"
// and the value of [Level.String] is output.
//
// If the AddSource option is set and source information is available,
// the key is "source" and the value is output as FILE:LINE.
//
// The message's key is "msg".
//
// To modify these or other attributes, or remove them from the output, use
// [HandlerOptions.ReplaceAttr].
//
// If a value implements [encoding.TextMarshaler], the result of MarshalText is
// written. Otherwise, the result of [fmt.Sprint] is written.
//
// Keys and values are quoted with [strconv.Quote] if they contain Unicode space
// characters, non-printing characters, '"' or '='.
//
// Keys inside groups consist of components (keys or group names) separated by
// dots. No further escaping is performed.
// Thus there is no way to determine from the key "a.b.c" whether there
// are two groups "a" and "b" and a key "c", or a single group "a.b" and a key "c",
// or single group "a" and a key "b.c".
// If it is necessary to reconstruct the group structure of a key
// even in the presence of dots inside components, use
// [HandlerOptions.ReplaceAttr] to encode that information in the key.
//
// Each call to Handle results in a single serialized call to
// io.Writer.Write.
[GoRecv] public static error Handle(this ref TextHandler h, context.Context _, Record r) {
    r = r.ΔClone();

    return h.commonHandler.handle(r);
}

internal static error appendTextValue(ж<handleState> Ꮡs, Value v) {
    ref var s = ref Ꮡs.DerefOrNull();

    var exprᴛ1 = v.Kind();
    if (exprᴛ1 == KindString) {
        s.appendString(v.str());
    }
    else if (exprᴛ1 == KindTime) {
        Ꮡs.appendTime(v.time());
    }
    else if (exprᴛ1 == KindAny) {
        {
            var (tm, ok) = v.any._<encoding.TextMarshaler>(ᐧ); if (ok) {
                var (data, err) = tm.MarshalText();
                if (err != default!) {
                    return err;
                }
                // TODO: avoid the conversion to string.
                s.appendString(((@string)data));
                return default!;
            }
        }
        {
            var (bs, ok) = byteSlice(v.any); if (ok) {
                // As of Go 1.19, this only allocates for strings longer than 32 bytes.
                s.buf.WriteString(strconv.Quote(((@string)bs)));
                return default!;
            }
        }
        s.appendString(fmt.Sprintf("%+v"u8, v.Any()));
    }
    else { /* default: */
        s.buf.ValueSlot = v.append(s.buf.ValueSlot);
    }

    return default!;
}

// byteSlice returns its argument as a []byte if the argument's
// underlying type is []byte, along with a second return value of true.
// Otherwise it returns nil, false.
internal static (slice<byte>, bool) byteSlice(any a) {
    {
        var (bs, ok) = a._<slice<byte>>(ᐧ); if (ok) {
            return (bs, true);
        }
    }
    // Like Printf's %s, we allow both the slice type and the byte element type to be named.
    var t = reflect.TypeOf(a);
    if (t != default! && t.Kind() == reflect.ΔSlice && t.Elem().Kind() == reflect.Uint8) {
        return (reflect.ValueOf(a).Bytes(), true);
    }
    return (default!, false);
}

internal static bool needsQuoting(@string s) {
    if (len(s) == 0) {
        return true;
    }
    for (nint i = 0; i < len(s); ) {
        var b = s[i];
        if (b < utf8.RuneSelf) {
            // Quote anything except a backslash that would need quoting in a
            // JSON string, as well as space and '='
            if (b != (rune)'\\' && (b == (rune)' ' || b == (rune)'=' || !safeSet[b])) {
                return true;
            }
            i++;
            continue;
        }
        var (r, size) = utf8.DecodeRuneInString(s.slice(i));
        if (r == utf8.RuneError || unicode.IsSpace(r) || !unicode.IsPrint(r)) {
            return true;
        }
        i += size;
    }
    return false;
}

} // end slog_package
