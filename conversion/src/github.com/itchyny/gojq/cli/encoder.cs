namespace go.github.com.itchyny.gojq;

using bytes = bytes_package;
using cmp = cmp_package;
using json = encoding.json_package;
using fmt = fmt_package;
using io = io_package;
using math = math_package;
using big = go.math.big_package;
using sort = sort_package;
using strconv = strconv_package;
using utf8 = unicode.utf8_package;
using encoding;
using go.math;
using unicode;

partial class cli_package {

partial struct encoder {
    internal io.Writer @out;
    internal ж<bytes.Buffer> w;
    internal bool tab;
    internal nint indent;
    internal nint depth;
    internal array<byte> buf = new(64);
}

internal static ж<encoder> newEncoder(bool tab, nint indent) {
    return Ꮡ(new encoder(w: @new<bytes.Buffer>(), tab: tab, indent: indent));
}

internal static error flush(this ref encoder e) {
    var (_, err) = e.@out.Write(e.w.Bytes());
    e.w.Reset();
    return err;
}

internal static error marshal(this ref encoder e, any v, io.Writer w) {
    e.@out = w;
    return cmp.Or(e.encode(v), e.flush());
}

internal static error encode(this ref encoder e, any v) {
    switch (v.type()) {
    case null: {
        e.write(slice<byte>("null"u8), nullColor);
        break;
    }
    case bool vΔ1: {
        if (vΔ1) {
            e.write(slice<byte>("true"u8), trueColor);
        } else {
            e.write(slice<byte>("false"u8), falseColor);
        }
        break;
    }
    case nint vΔ1: {
        e.write(strconv.AppendInt(e.buf[..0], (int64)vΔ1, 10), numberColor);
        break;
    }
    case float64 vΔ1: {
        e.encodeFloat64(vΔ1);
        break;
    }
    case ж<bigꓸInt> vΔ1: {
        e.write(vΔ1.Append(e.buf[..0], 10), numberColor);
        break;
    }
    case json.Number vΔ1: {
        e.write(slice<byte>(vΔ1.String()), numberColor);
        break;
    }
    case @string vΔ1: {
        e.encodeString(vΔ1, stringColor);
        break;
    }
    case slice<any> vΔ1: {
        {
            var err = e.encodeArray(vΔ1); if (err != nil) {
                return err;
            }
        }
        break;
    }
    case map<@string, any> vΔ1: {
        {
            var err = e.encodeObject(vΔ1); if (err != nil) {
                return err;
            }
        }
        break;
    }
    default: {
        var vΔ1 = v;
        throw panic(fmt.Sprintf("invalid type: %[1]T (%[1]v)"u8, vΔ1));
        break;
    }}
    if (e.w.Len() > 8 * 1024) {
        return e.flush();
    }
    return default;
}

internal static void encodeFloat64(this ref encoder e, float64 f) {
    if (math.IsNaN(f)) {
        e.write(slice<byte>("null"u8), nullColor);
        return;
    }
    f = min(max(f, (float64)(-math.MaxFloat64)), (float64)(math.MaxFloat64));
    var format = (byte)'f';
    var x = math.Abs(f); if (x != 0D && x < 1e-6D || x >= 1e21D) {
        format = (byte)'e';
    }
    var buf = strconv.AppendFloat(e.buf[..0], f, format, -1, 64);
    if (format == 'e') {
        nint n = len(buf); if (n >= 4 && buf[n - 4] == 'e' && buf[n - 3] == '-' && buf[n - 2] == '0') {
            buf[n - 2] = buf[n - 1];
            buf = buf.slice(0, n - 1);
        }
    }
    e.write(buf, numberColor);
}

internal static void encodeString(this ref encoder e, @string s, slice<byte> color) {
    if (color != nil) {
        setColor(e.w, color);
    }
    e.w.WriteByte((byte)'"');
    nint start = 0;
    for (nint i = 0; i < len(s); ) {
        var b = s[i]; if (b < utf8.RuneSelf) {
            if (' ' <= b && b <= '~' && b != '"' && b != '\\') {
                i++;
                continue;
            }
            if (start < i) {
                e.w.WriteString(s.slice(start, i));
            }
            switch (b) {
            case (byte)'"': {
                e.w.WriteString(@"\""");
                break;
            }
            case (byte)'\\': {
                e.w.WriteString(@"\\");
                break;
            }
            case (byte)'\b': {
                e.w.WriteString(@"\b");
                break;
            }
            case (byte)'\f': {
                e.w.WriteString(@"\f");
                break;
            }
            case (byte)'\n': {
                e.w.WriteString(@"\n");
                break;
            }
            case (byte)'\r': {
                e.w.WriteString(@"\r");
                break;
            }
            case (byte)'\t': {
                e.w.WriteString(@"\t");
                break;
            }
            default: {
                @string hex = "0123456789abcdef";
                e.w.WriteString(@"\u00");
                e.w.WriteByte(hex[(b >> 4)]);
                e.w.WriteByte(hex[(byte)(b & 0xF)]);
                break;
            }}

            i++;
            start = i;
            continue;
        }
        var (c, size) = utf8.DecodeRuneInString(s.slice(i));
        if (c == utf8.RuneError && size == 1) {
            if (start < i) {
                e.w.WriteString(s.slice(start, i));
            }
            e.w.WriteString(@"\ufffd");
            i += size;
            start = i;
            continue;
        }
        i += size;
    }
    if (start < len(s)) {
        e.w.WriteString(s.slice(start));
    }
    e.w.WriteByte((byte)'"');
    if (color != nil) {
        setColor(e.w, resetColor);
    }
}

internal static error encodeArray(this ref encoder e, slice<any> vs) {
    e.writeByte((byte)'[', arrayColor);
    e.depth += e.indent;
    foreach (var (i, v) in range(vs)) {
        if (i > 0) {
            e.writeByte((byte)',', arrayColor);
        }
        if (e.indent >= 0) {
            e.writeIndent();
        }
        var err = e.encode(v); if (err != nil) {
            return err;
        }
    }
    e.depth -= e.indent;
    if (len(vs) > 0 && e.indent >= 0) {
        e.writeIndent();
    }
    e.writeByte((byte)']', arrayColor);
    return default;
}

internal partial struct encodeObject_keyVal /*dyn*/ {
    internal @string key;
    internal any val;
}

internal static error encodeObject(this ref encoder e, map<@string, any> vs) {
    e.writeByte((byte)'{', objectColor);
    e.depth += e.indent;
    var kvs = new slice<encodeObject_keyVal>(len(vs));
    nint i = default;
    foreach (var (k, v) in vs) {
        kvs[i] = new encodeObject_keyVal(k, v);
        i++;
    }
    var kvsʗ1 = kvs;
    sort.Slice(kvs, (nint iΔ1, nint j) => kvsʗ1[iΔ1].key < kvsʗ1[j].key);
    foreach (var (iΔ2, kv) in range(kvs)) {
        if (iΔ2 > 0) {
            e.writeByte((byte)',', objectColor);
        }
        if (e.indent >= 0) {
            e.writeIndent();
        }
        e.encodeString(kv.key, objectKeyColor);
        e.writeByte((byte)':', objectColor);
        if (e.indent >= 0) {
            e.w.WriteByte((byte)' ');
        }
        var err = e.encode(kv.val); if (err != nil) {
            return err;
        }
    }
    e.depth -= e.indent;
    if (len(vs) > 0 && e.indent >= 0) {
        e.writeIndent();
    }
    e.writeByte((byte)'}', objectColor);
    return default;
}

internal static void writeIndent(this ref encoder e) {
    e.w.WriteByte((byte)'\n');
    nint n = e.depth; if (n > 0) {
        if (e.tab) {
            e.writeIndentInternal(n, "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t");
        } else {
            e.writeIndentInternal(n, "                                ");
        }
    }
}

internal static void writeIndentInternal(this ref encoder e, nint n, @string spaces) {
    nint l = len(spaces); if (n <= l) {
        e.w.WriteString(spaces.slice(0, n));
    } else {
        e.w.WriteString(spaces);
        for (n -= l; n > 0; (n, l) = (n - l, l * 2)) {
            if (n < l) {
                l = n;
            }
            e.w.Write(e.w.Bytes().slice(e.w.Len() - l));
        }
    }
}

internal static void writeByte(this ref encoder e, byte b, slice<byte> color) {
    if (color == nil) {
        e.w.WriteByte(b);
    } else {
        setColor(e.w, color);
        e.w.WriteByte(b);
        setColor(e.w, resetColor);
    }
}

internal static void write(this ref encoder e, slice<byte> bs, slice<byte> color) {
    if (color == nil) {
        e.w.Write(bs);
    } else {
        setColor(e.w, color);
        e.w.Write(bs);
        setColor(e.w, resetColor);
    }
}

} // end cli_package
