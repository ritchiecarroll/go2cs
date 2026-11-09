namespace go.github.com.itchyny;

using utf8 = go.unicode.utf8_package;
using go.unicode;

partial class gojq_package {

public static @string Preview(any v) {
    var bs = jsonLimitedMarshal(v, 32);
    nint l = 30; if (len(bs) > l) {
        @string trailing = default;
        switch (v.type()) {
        case @string: {
            trailing = @" ...""";
            break;
        }
        case slice<any>: {
            trailing = " ...]";
            break;
        }
        case map<@string, any>: {
            trailing = " ...}";
            break;
        }
        default: {
            trailing = " ...";
            break;
        }}

        while (len(bs) > l - len(trailing)) {
            var (_, size) = utf8.DecodeLastRune(bs);
            bs = bs.slice(0, len(bs) - size);
        }
        bs = builtin.append(bs, trailing.ꓸꓸꓸ);
    }
    return (@string)bs;
}

internal static slice<byte> /*bs*/ jsonLimitedMarshal(any v, nint n) {
    slice<byte> bs = default;
    GoFrame ᒐ = default;
    try {
        var w = Ꮡ(new limitedWriter(buf: new slice<byte>(n)));
        var wʗ1 = w;
        defer(() => {
            _ = recover();
            bs = wʗ1.Bytes();
        }, ref ᒐ);
        (Ꮡ(new encoder(w: new limitedWriterжencoder_w(w)))).encode(v);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return bs;
}

partial struct limitedWriter {
    internal slice<byte> buf;
    internal nint off;
}

internal static (nint, error) Write(this ref limitedWriter w, slice<byte> bs) {
    nint n = copy(w.buf.slice(w.off), bs);
    w.off += n; if (w.off == len(w.buf)) {
        throw panic(new EmptyStruct());
    }
    return (n, default);
}

internal static error WriteByte(this ref limitedWriter w, byte b) {
    w.buf[w.off] = b;
    w.off++; if (w.off == len(w.buf)) {
        throw panic(new EmptyStruct());
    }
    return default;
}

internal static (nint, error) WriteString(this ref limitedWriter w, @string s) {
    nint n = copy(w.buf.slice(w.off), s);
    w.off += n; if (w.off == len(w.buf)) {
        throw panic(new EmptyStruct());
    }
    return (n, default);
}

internal static slice<byte> Bytes(this ref limitedWriter w) {
    return w.buf.slice(0, w.off);
}

} // end gojq_package
