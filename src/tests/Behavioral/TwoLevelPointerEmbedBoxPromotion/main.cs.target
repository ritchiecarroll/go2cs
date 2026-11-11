namespace go;

using errors = errors_package;
using fmt = fmt_package;
using Δio = io_package;
using Δnet = net_package;
using Δsync = sync_package;
using time = time_package;

partial class main_package {

partial struct rawFile {
    internal Δsync.Mutex mu;
    internal bool closed;
    internal deadline rd, wd;
    internal slice<byte> data;
}

partial struct deadline {
    internal time.Time t;
}

internal static error set(this ж<deadline> Ꮡd, time.Time t) {
    var tʗ1 = t;
    void fire() {
        Ꮡd.Value.t = tʗ1;
    }
    fire();
    return default;
}

internal static error Close(this ж<rawFile> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    f.mu.Lock();
    f.closed = true;
    f.mu.Unlock();
    return default;
}

internal static (nint, error) Read(this ref rawFile f, slice<byte> b) { return (copy(b, f.data), default); }

internal static (nint, error) Write(this ref rawFile f, slice<byte> b) {
    f.data = appendꓸꓸꓸ(f.data, b);
    return (len(b), default);
}

internal static error SetReadDeadline(this ж<rawFile> Ꮡf, time.Time t) { return Ꮡf.Ꮡrd.set(t); }
internal static error SetWriteDeadline(this ж<rawFile> Ꮡf, time.Time t) { return Ꮡf.Ꮡwd.set(t); }
internal static error Flush(this ref rawFile f) { return default; }

partial struct addr /*@string*/;

internal static @string Network(this addr a) { return "pipe"; }
internal static @string String(this addr a) { return (@string)a; }

partial struct pipe {
    /*embed*/ internal ж<rawFile> rawFile;
    internal @string path;
}

internal static netꓸAddr LocalAddr(this ref pipe p) { return ((addr)p.path); }
internal static netꓸAddr RemoteAddr(this ref pipe p) { return ((addr)p.path); }
internal static error Disconnect(this ref pipe p) { return default; }

internal static error SetDeadline(this ж<pipe> Ꮡp, time.Time t) {
    _ = Ꮡp.SetReadDeadline(t);
    return Ꮡp.SetWriteDeadline(t);
}

partial struct msgPipe {
    /*embed*/ internal pipe pipe;
    internal bool writeClosed;
}

internal static error CloseWrite(this ref msgPipe m) {
    m.writeClosed = true;
    return default;
}

internal static (nint, error) Write(this ref msgPipe m, slice<byte> b) {
    if (m.writeClosed) {
        return (0, errors.New("closed"));
    }
    return m.rawFile.Write(b);
}

internal static (nint, error) Read(this ref msgPipe m, slice<byte> b) { return m.rawFile.Read(b); }

partial interface PipeConn :
    Δnet.Conn
{
    error Disconnect();
    error Flush();
}

internal static PipeConn _ᴛ1ʗ = new pipeжPipeConn(((ж<pipe>)nil));
internal static PipeConn _ᴛ2ʗ = new msgPipeжPipeConn(((ж<msgPipe>)nil));
internal static Δnet.Conn _ᴛ3ʗ = new msgPipeжConn(((ж<msgPipe>)nil));

internal static void use(PipeConn c) {
    (_, _) = c.Write(slice<byte>("hi"u8));
    var b = new slice<byte>(4);
    var (n, _) = c.Read(b);
    var when = time.Unix(100, 0);
    _ = c.SetReadDeadline(when);
    _ = c.SetWriteDeadline(when);
    _ = c.SetDeadline(when);
    _ = c.Flush();
    fmt.Println(n, (@string)(b.slice(0, n)), c.LocalAddr().String(), c.RemoteAddr().Network());
    _ = c.Close();
}

partial struct @file {
    internal Δsync.Mutex mu;
    internal nint n;
}

internal static nint bump(this ж<@file> Ꮡf) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var f = ref Ꮡf.DerefOrNull();

        f.mu.Lock();
        ᒐd1 = true;
        f.n++;
        return f.n;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡf.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

partial struct handle {
    /*embed*/ internal ж<@file> @file;
}

partial interface bumper {
    nint bump();
}

internal static void main() {
    var f1 = Ꮡ(new rawFile());
    use(new pipeжPipeConn(Ꮡ(new pipe(rawFile: f1, path: @"\.\pipe\a"))));
    fmt.Println(f1.closed, f1.rd.t.Unix(), f1.wd.t.Unix());

    var f2 = Ꮡ(new rawFile());
    var m = Ꮡ(new msgPipe(pipe: new pipe(rawFile: f2, path: @"\.\pipe\b")));
    use(new msgPipeжPipeConn(m));
    fmt.Println(f2.closed, f2.rd.t.Unix(), f2.wd.t.Unix(), m.CloseWrite() == nil, m.writeClosed);

    var f3 = Ꮡ(new rawFile());
    Δio.Closer c = new msgPipe(pipe: new pipe(rawFile: f3));
    _ = c.Close();
    fmt.Println(f3.closed);

    ref var h = ref heap<handle>(new handle(Ꮡ(new @file())), out var Ꮡh);
    bumper b = new handleжbumper(Ꮡh);
    fmt.Println(b.bump(), b.bump());
    b = h;
    fmt.Println(b.bump(), h.n);

    any x = new msgPipe(pipe: new pipe(rawFile: Ꮡ(new rawFile())));
    var (_, isCloser) = x._<Δio.Closer>(ᐧ);
    var (_, isConn) = x._<Δnet.Conn>(ᐧ);
    x = Ꮡ(new msgPipe(pipe: new pipe(rawFile: Ꮡ(new rawFile()))));
    var (_, isPipeConn) = x._<PipeConn>(ᐧ);
    x = h;
    var (_, isBumper) = x._<bumper>(ᐧ);
    fmt.Println(isCloser, isConn, isPipeConn, isBumper);
}

} // end main_package
