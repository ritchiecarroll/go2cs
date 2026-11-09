namespace go.github.com.itchyny;

using bytes = bytes_package;
using json = encoding.json_package;
using fmt = fmt_package;
using io = io_package;
using math = math_package;
using big = go.math.big_package;
using sort = sort_package;
using strconv = strconv_package;
using strings = strings_package;
using utf8 = go.unicode.utf8_package;
using encoding;
using go.math;
using go.unicode;

partial class gojq_package {

public static (slice<byte>, error) Marshal(any v) {
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    (Ꮡ(new encoder(w: new bytes_Bufferжencoder_w(Ꮡb)))).encode(v);
    return (b.Bytes(), default);
}

internal static @string jsonMarshal(any v) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    (Ꮡ(new encoder(w: new strings_Builderжencoder_w(Ꮡsb)))).encode(v);
    return sb.String();
}

internal static void jsonEncodeString(ж<strings.Builder> sb, @string v) {
    (Ꮡ(new encoder(w: new strings_Builderжencoder_w(sb)))).encodeString(v);
}

partial interface encoder_w /*dyn*/ :
    io.Writer,
    io.ByteWriter,
    io.StringWriter
{
}

partial struct encoder {
    internal encoder_w w;
    internal array<byte> buf = new(64);
}

internal static void encode(this ref encoder e, any v) {
    switch (v.type()) {
    case null: {
        e.w.WriteString("null");
        break;
    }
    case bool vΔ1: {
        if (vΔ1) {
            e.w.WriteString("true");
        } else {
            e.w.WriteString("false");
        }
        break;
    }
    case nint vΔ1: {
        e.w.Write(strconv.AppendInt(e.buf[..0], (int64)vΔ1, 10));
        break;
    }
    case float64 vΔ1: {
        e.encodeFloat64(vΔ1);
        break;
    }
    case ж<bigꓸInt> vΔ1: {
        e.w.Write(vΔ1.Append(e.buf[..0], 10));
        break;
    }
    case json.Number vΔ1: {
        e.w.WriteString(vΔ1.String());
        break;
    }
    case @string vΔ1: {
        e.encodeString(vΔ1);
        break;
    }
    case slice<any> vΔ1: {
        e.encodeArray(vΔ1);
        break;
    }
    case map<@string, any> vΔ1: {
        e.encodeObject(vΔ1);
        break;
    }
    default: {
        var vΔ1 = v;
        throw panic(fmt.Sprintf("invalid type: %[1]T (%[1]v)"u8, vΔ1));
        break;
    }}
}

internal static void encodeFloat64(this ref encoder e, float64 f) {
    if (math.IsNaN(f)) {
        e.w.WriteString("null");
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
    e.w.Write(buf);
}

internal static void encodeString(this ref encoder e, @string s) {
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
}

internal static void encodeArray(this ref encoder e, slice<any> vs) {
    e.w.WriteByte((byte)'[');
    foreach (var (i, v) in range(vs)) {
        if (i > 0) {
            e.w.WriteByte((byte)',');
        }
        e.encode(v);
    }
    e.w.WriteByte((byte)']');
}

internal partial struct encodeObject_keyVal /*dyn*/ {
    internal @string key;
    internal any val;
}

internal static void encodeObject(this ref encoder e, map<@string, any> vs) {
    e.w.WriteByte((byte)'{');
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
            e.w.WriteByte((byte)',');
        }
        e.encodeString(kv.key);
        e.w.WriteByte((byte)':');
        e.encode(kv.val);
    }
    e.w.WriteByte((byte)'}');
}

} // end gojq_package
