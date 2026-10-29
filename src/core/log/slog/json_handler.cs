// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.log;

using bytes = bytes_package;
using context = context_package;
using json = go.encoding.json_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using buffer = go.log.slog.@internal.buffer_package;
using strconv = strconv_package;
using sync = sync_package;
using time = time_package;
using utf8 = go.unicode.utf8_package;
using go.encoding;
using go.log.slog.@internal;
using go.unicode;

partial class slog_package {

// JSONHandler is a [Handler] that writes Records to an [io.Writer] as
// line-delimited JSON objects.
partial struct JSONHandler {
    /*embed*/ internal ж<commonHandler> commonHandler;
}

// NewJSONHandler creates a [JSONHandler] that writes to w,
// using the given options.
// If opts is nil, the default options are used.
public static ж<JSONHandler> NewJSONHandler(io.Writer w, ж<HandlerOptions> Ꮡopts) {
    ref var opts = ref Ꮡopts.DerefOrNull();

    if (Ꮡopts == nil) {
        Ꮡopts = Ꮡ(new HandlerOptions()); opts = ref Ꮡopts.DerefOrNull();
    }
    return Ꮡ(new JSONHandler(
        Ꮡ(new commonHandler(
            json: true,
            w: w,
            opts: opts,
            mu: Ꮡ(new sync.Mutex())
        ))
    ));
}

// Enabled reports whether the handler handles records at the given level.
// The handler ignores records whose level is lower.
public static bool Enabled(this ref JSONHandler h, context.Context _, ΔLevel level) {
    return h.commonHandler.enabled(level);
}

// WithAttrs returns a new [JSONHandler] whose attributes consists
// of h's attributes followed by attrs.
public static ΔHandler WithAttrs(this ref JSONHandler h, slice<Attr> attrs) {
    return new JSONHandlerжΔHandler(Ꮡ(new JSONHandler(commonHandler: h.commonHandler.withAttrs(attrs))));
}

public static ΔHandler WithGroup(this ref JSONHandler h, @string name) {
    return new JSONHandlerжΔHandler(Ꮡ(new JSONHandler(commonHandler: h.commonHandler.withGroup(name))));
}

// Handle formats its argument [Record] as a JSON object on a single line.
//
// If the Record's time is zero, the time is omitted.
// Otherwise, the key is "time"
// and the value is output as with json.Marshal.
//
// The level's key is "level" and its value is the result of calling [Level.String].
//
// If the AddSource option is set and source information is available,
// the key is "source", and the value is a record of type [Source].
//
// The message's key is "msg".
//
// To modify these or other attributes, or remove them from the output, use
// [HandlerOptions.ReplaceAttr].
//
// Values are formatted as with an [encoding/json.Encoder] with SetEscapeHTML(false),
// with two exceptions.
//
// First, an Attr whose Value is of type error is formatted as a string, by
// calling its Error method. Only errors in Attrs receive this special treatment,
// not errors embedded in structs, slices, maps or other data structures that
// are processed by the [encoding/json] package.
//
// Second, an encoding failure does not cause Handle to return an error.
// Instead, the error message is formatted as a string.
//
// Each call to Handle results in a single serialized call to io.Writer.Write.
public static error Handle(this ref JSONHandler h, context.Context _, Record r) {
    r = r.ΔClone();

    return h.commonHandler.handle(r);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string timeTimeYearOutsideOfˢ = "time.Time year outside of range [0,9999]"u8;

// Adapted from time.Time.MarshalJSON to avoid allocation.
internal static void appendJSONTime(ж<handleState> Ꮡs, time.Time t) {
    ref var s = ref Ꮡs.DerefOrNull();

    {
        nint y = t.Year(); if (y < 0 || y >= 10000) {
            // RFC 3339 is clear that years are 4 digits exactly.
            // See golang.org/issue/4556#c15 for more discussion.
            s.appendError(errors.New(timeTimeYearOutsideOfˢ));
        }
    }
    s.buf.WriteByte((byte)'"');
    s.buf.ValueSlot = t.AppendFormat(s.buf.ValueSlot, time_package.RFC3339Nano);
    s.buf.WriteByte((byte)'"');
}

internal static error appendJSONValue(ж<handleState> Ꮡs, Value v) {
    ref var s = ref Ꮡs.DerefOrNull();

    switch (v.Kind()) {
    case var ᴛ when ᴛ == KindString: {
        s.appendString(v.str());
        break;
    }
    case var ᴛ when ᴛ == KindInt64: {
        s.buf.ValueSlot = strconv.AppendInt(s.buf.ValueSlot, v.Int64(), 10);
        break;
    }
    case var ᴛ when ᴛ == KindUint64: {
        s.buf.ValueSlot = strconv.AppendUint(s.buf.ValueSlot, v.Uint64(), 10);
        break;
    }
    case var ᴛ when ᴛ == KindFloat64: {
        // json.Marshal is funny about floats; it doesn't
        // always match strconv.AppendFloat. So just call it.
        // That's expensive, but floats are rare.
        {
            var err = appendJSONMarshal(s.buf, v.Float64()); if (err != nil) {
                return err;
            }
        }
        break;
    }
    case var ᴛ when ᴛ == KindBool: {
        s.buf.ValueSlot = strconv.AppendBool(s.buf.ValueSlot, v.Bool());
        break;
    }
    case var ᴛ when ᴛ == KindDuration: {
        // Do what json.Marshal does.
        s.buf.ValueSlot = strconv.AppendInt(s.buf.ValueSlot, (int64)v.Duration(), 10);
        break;
    }
    case var ᴛ when ᴛ == KindTime: {
        Ꮡs.appendTime(v.Time());
        break;
    }
    case var ᴛ when ᴛ == KindAny: {
        var a = v.Any();
        var (_, jm) = a._<json.Marshaler>(ᐧ);
        {
            var (err, ok) = a._<error>(ᐧ); if (ok && !jm) {
                s.appendString(err.Error());
            } else {
                return appendJSONMarshal(s.buf, a);
            }
        }
        break;
    }
    default: {
        throw panic(fmt.Sprintf("bad kind: %s"u8, v.Kind()));
        break;
    }}
    return default;
}

partial struct jsonEncoder {
    internal ж<bytes.Buffer> buf;
    // Use a json.Encoder to avoid escaping HTML.
    internal ж<json.Encoder> json;
}

[GoLazy] partial class lazyᴛjsonEncoderPool { internal static ж<sync.Pool> jsonEncoderPool = Ꮡ(new sync.Pool(
    New: () => {
        var enc = Ꮡ(new jsonEncoder(
            buf: @new<bytes.Buffer>()
        ));
        enc.json = json.NewEncoder(new bytes_BufferжWriter(enc.buf));
        enc.json.SetEscapeHTML(false);
        return enc.OrTypedNil();
    }
)); }

internal static error appendJSONMarshal(ж<buffer.Buffer> Ꮡbuf, any v) {
    GoFrame ᒐ = default;
    try {
        ref var buf = ref Ꮡbuf.DerefOrNull();

        var j = jsonEncoderPool.Get()._<ж<jsonEncoder>>();
        var jʗ1 = j;
        defer(() => {
            // To reduce peak allocation, return only smaller buffers to the pool.
            const nint maxBufferSize = /* 16 << 10 */ 16384;
            if (jʗ1.buf.Cap() > maxBufferSize) {
                return;
            }
            jʗ1.buf.Reset();
            jsonEncoderPool.Put(jʗ1.OrTypedNil());
        }, ref ᒐ);

        {
            var err = j.json.Encode(v); if (err != nil) {
                return err;
            }
        }

        var bs = j.buf.Bytes();
        buf.Write(bs.slice(0, len(bs) - 1)); // remove final newline
        return default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string u00ˢ = @"u00"u8;
internal static readonly @string ufffdˢ = @"\ufffd"u8;
internal static readonly @string u202ˢ = @"\u202"u8;

// appendEscapedJSONString escapes s for JSON and appends it to buf.
// It does not surround the string in quotation marks.
//
// Modified from encoding/json/encode.go:encodeState.string,
// with escapeHTML set to false.
internal static slice<byte> appendEscapedJSONString(slice<byte> buf, @string s) {
    void @char(byte b) {
        buf = builtin.append(buf, b);
    }
    void str(@string sΔ1) {
        buf = builtin.append(buf, sΔ1.ꓸꓸꓸ);
    }

    nint start = 0;
    for (nint i = 0; i < len(s); ) {
        {
            var b = s[i]; if (b < utf8.RuneSelf) {
                if (safeSet[b]) {
                    i++;
                    continue;
                }
                if (start < i) {
                    str(s.slice(start, i));
                }
                @char((byte)'\\');
                switch (b) {
                case (byte)'\\' or (byte)'"': {
                    @char(b);
                    break;
                }
                case (byte)'\n': {
                    @char((byte)'n');
                    break;
                }
                case (byte)'\r': {
                    @char((byte)'r');
                    break;
                }
                case (byte)'\t': {
                    @char((byte)'t');
                    break;
                }
                default: {
                    // This encodes bytes < 0x20 except for \t, \n and \r.
                    str(u00ˢ);
                    @char(hex[(b >> 4)]);
                    @char(hex[(byte)(b & 0xF)]);
                    break;
                }}
                i++;
                start = i;
                continue;
            }
        }
        var (c, size) = utf8.DecodeRuneInString(s.slice(i));
        if (c == utf8.RuneError && size == 1) {
            if (start < i) {
                str(s.slice(start, i));
            }
            str(ufffdˢ);
            i += size;
            start = i;
            continue;
        }
        // U+2028 is LINE SEPARATOR.
        // U+2029 is PARAGRAPH SEPARATOR.
        // They are both technically valid characters in JSON strings,
        // but don't work in JSONP, which has to be evaluated as JavaScript,
        // and can lead to security holes there. It is valid JSON to
        // escape them, so we do so unconditionally.
        // See http://timelessrepo.com/json-isnt-a-javascript-subset for discussion.
        if (c == '\u2028' || c == '\u2029') {
            if (start < i) {
                str(s.slice(start, i));
            }
            str(u202ˢ);
            @char(hex[(c & 0xF)]);
            i += size;
            start = i;
            continue;
        }
        i += size;
    }
    if (start < len(s)) {
        str(s.slice(start));
    }
    return buf;
}

internal static readonly @string hex = "0123456789abcdef"u8;

// Copied from encoding/json/tables.go.
//
// safeSet holds the value true if the ASCII character with the given array
// position can be represented inside a JSON string without any further
// escaping.
//
// All values are true except for the ASCII control characters (0-31), the
// double quote ("), and the backslash character ("\").
[GoLazy] partial class lazyᴛsafeSet { internal static array<bool> safeSet = new array<bool>(128){
    [' '] = true,
    ['!'] = true,
    ['"'] = false,
    ['#'] = true,
    ['$'] = true,
    ['%'] = true,
    ['&'] = true,
    ['\''] = true,
    ['('] = true,
    [')'] = true,
    ['*'] = true,
    ['+'] = true,
    [','] = true,
    ['-'] = true,
    ['.'] = true,
    ['/'] = true,
    ['0'] = true,
    ['1'] = true,
    ['2'] = true,
    ['3'] = true,
    ['4'] = true,
    ['5'] = true,
    ['6'] = true,
    ['7'] = true,
    ['8'] = true,
    ['9'] = true,
    [':'] = true,
    [';'] = true,
    ['<'] = true,
    ['='] = true,
    ['>'] = true,
    ['?'] = true,
    ['@'] = true,
    ['A'] = true,
    ['B'] = true,
    ['C'] = true,
    ['D'] = true,
    ['E'] = true,
    ['F'] = true,
    ['G'] = true,
    ['H'] = true,
    ['I'] = true,
    ['J'] = true,
    ['K'] = true,
    ['L'] = true,
    ['M'] = true,
    ['N'] = true,
    ['O'] = true,
    ['P'] = true,
    ['Q'] = true,
    ['R'] = true,
    ['S'] = true,
    ['T'] = true,
    ['U'] = true,
    ['V'] = true,
    ['W'] = true,
    ['X'] = true,
    ['Y'] = true,
    ['Z'] = true,
    ['['] = true,
    ['\\'] = false,
    [']'] = true,
    ['^'] = true,
    ['_'] = true,
    ['`'] = true,
    ['a'] = true,
    ['b'] = true,
    ['c'] = true,
    ['d'] = true,
    ['e'] = true,
    ['f'] = true,
    ['g'] = true,
    ['h'] = true,
    ['i'] = true,
    ['j'] = true,
    ['k'] = true,
    ['l'] = true,
    ['m'] = true,
    ['n'] = true,
    ['o'] = true,
    ['p'] = true,
    ['q'] = true,
    ['r'] = true,
    ['s'] = true,
    ['t'] = true,
    ['u'] = true,
    ['v'] = true,
    ['w'] = true,
    ['x'] = true,
    ['y'] = true,
    ['z'] = true,
    ['{'] = true,
    ['|'] = true,
    ['}'] = true,
    ['~'] = true,
    ['\x7f'] = true
}; }

} // end slog_package
