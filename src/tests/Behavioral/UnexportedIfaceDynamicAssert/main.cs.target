namespace go;

using fmt = fmt_package;
using Δio = io_package;

partial class main_package {

partial interface buffersWriter {
    (int64, error) writeBuffers(ж<Buffers> _);
}

partial struct Buffers /*[]slice<byte>*/;

public static (int64, error) WriteTo(this ж<Buffers> Ꮡv, Δio.Writer w) {
    ref var v = ref Ꮡv.DerefOrNull();

    {
        var (wv, ok) = w._<buffersWriter>(ᐧ); if (ok) {
            return wv.writeBuffers(Ꮡv);
        }
    }
    int64 n = default!;
    foreach (var (_, b) in v) {
        var (nb, err) = w.Write(b);
        n += (int64)nb;
        if (err != default!) {
            return (n, err);
        }
    }
    return (n, default!);
}

partial struct sink {
    internal slice<byte> bytes;
    internal @string mode;
}

partial struct conn {
    internal ж<sink> fd;
}

internal static bool ok(this ж<conn> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    return Ꮡc != nil && c.fd != nil;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string writeˢ = "write"u8;

internal static (nint, error) Write(this ж<conn> Ꮡc, slice<byte> b) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (!Ꮡc.ok()) {
        return (0, fmt.Errorf("invalid conn"u8));
    }
    c.fd.Value.bytes = appendꓸꓸꓸ((~c.fd).bytes, b);
    c.fd.Value.mode = writeˢ;
    return (len(b), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string writeBuffersˢ = "writeBuffers"u8;

internal static (int64, error) writeBuffers(this ж<conn> Ꮡc, ж<Buffers> Ꮡv) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var v = ref Ꮡv.DerefOrNull();

    if (!Ꮡc.ok()) {
        return (0, fmt.Errorf("invalid conn"u8));
    }
    int64 n = default!;
    foreach (var (_, b) in v) {
        c.fd.Value.bytes = appendꓸꓸꓸ((~c.fd).bytes, b);
        n += (int64)len(b);
    }
    c.fd.Value.mode = writeBuffersˢ;
    return (n, default!);
}

partial struct TCPConn {
    internal partial ref conn conn { get; }
}

partial struct plainConn {
    internal ж<sink> fd;
}

internal static bool ok(this ж<plainConn> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    return Ꮡc != nil && c.fd != nil;
}

internal static (nint, error) Write(this ж<plainConn> Ꮡc, slice<byte> b) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (!Ꮡc.ok()) {
        return (0, fmt.Errorf("invalid conn"u8));
    }
    c.fd.Value.bytes = appendꓸꓸꓸ((~c.fd).bytes, b);
    c.fd.Value.mode = writeˢ;
    return (len(b), default!);
}

partial struct PlainConn {
    internal partial ref plainConn plainConn { get; }
}

partial struct valueSink {
    internal ж<sink> fd;
}

internal static (nint, error) Write(this valueSink s, slice<byte> b) {
    s.fd.Value.bytes = appendꓸꓸꓸ((~s.fd).bytes, b);
    s.fd.Value.mode = writeˢ;
    return (len(b), default!);
}

internal static (int64, error) writeBuffers(this valueSink s, ж<Buffers> Ꮡv) {
    ref var v = ref Ꮡv.DerefOrNull();

    int64 n = default!;
    foreach (var (_, b) in v) {
        s.fd.Value.bytes = appendꓸꓸꓸ((~s.fd).bytes, b);
        n += (int64)len(b);
    }
    s.fd.Value.mode = writeBuffersˢ;
    return (n, default!);
}

partial struct ValueSink {
    internal partial ref valueSink valueSink { get; }
}

internal static void report(@string label, ref sink s, int64 n, error err) {
    fmt.Println(label, n, err, s.mode, ((@string)s.bytes));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string tcpConnˢ = "TCPConn"u8;
private static readonly @string connˢ = "conn"u8;
private static readonly @string valueSinkˢ = "ValueSink"u8;
private static readonly @string plainConnˢ = "PlainConn"u8;
private static readonly object directˢ = (@string)"direct"u8;
private static readonly object missˢ = (@string)"miss"u8;

internal static void Main() {
    ref var bufs = ref heap<Buffers>(out var Ꮡbufs);
    bufs = new Buffers(new slice<byte>[]{slice<byte>("go"u8), slice<byte>("2"u8), slice<byte>("cs"u8)}.slice());
    var tcpSink = Ꮡ(new sink(nil));
    var tcp = Ꮡ(new TCPConn(new conn(fd: tcpSink)));
    Δio.Writer w = new TCPConnжWriter(tcp);
    var (n, err) = Ꮡbufs.WriteTo(w);
    report(tcpConnˢ, ref (tcpSink).DerefOrNull(), n, err);
    var connSink = Ꮡ(new sink(nil));
    var c = Ꮡ(new conn(fd: connSink));
    (n, err) = Ꮡbufs.WriteTo(new connжWriter(c));
    report(connˢ, ref (connSink).DerefOrNull(), n, err);
    var vsSink = Ꮡ(new sink(nil));
    var vs = new ValueSink(new valueSink(fd: vsSink));
    (n, err) = Ꮡbufs.WriteTo(vs);
    report(valueSinkˢ, ref (vsSink).DerefOrNull(), n, err);
    var plainSink = Ꮡ(new sink(nil));
    var p = Ꮡ(new PlainConn(new plainConn(fd: plainSink)));
    (n, err) = Ꮡbufs.WriteTo(new PlainConnжWriter(p));
    report(plainConnˢ, ref (plainSink).DerefOrNull(), n, err);
    {
        var (wv, ok) = w._<buffersWriter>(ᐧ); if (ok){
            ref var more = ref heap<Buffers>(out var Ꮡmore);
            more = new Buffers(new slice<byte>[]{slice<byte>("!"u8)}.slice());
            (n, _) = wv.writeBuffers(Ꮡmore);
            fmt.Println(directˢ, n, ((@string)(~tcpSink).bytes));
        } else {
            fmt.Println(directˢ, missˢ);
        }
    }
}

} // end main_package
