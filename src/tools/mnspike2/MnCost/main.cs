namespace go;

using sha256 = crypto.sha256_package;
using fmt = fmt_package;
using Δio = io_package;
using os = os_package;
using runtime = runtime_package;
using strconv = strconv_package;
using Δsync = sync_package;
using time = time_package;
using crypto;
using hash = hash_package;

partial class main_package {

internal static nint envInt(@string name, nint def) {
    var (v, err) = strconv.Atoi(os.Getenv(name)); if (err == nil && v > 0) {
        return v;
    }

    return def;
}

internal static partial nint maybePark(channel<nint> c, nint i) {
    if (i < 0) {
        return ᐸꟷ(c);
    }

    return i + 1;
}

internal static nint callLoop(channel<nint> c, nint n) {
    nint sum = 0;

    for (nint i = 0; i < n; i++) {
        sum += maybePark(c, i);
    }

    return sum;
}

internal static nint mutexLoop(ж<Δsync.Mutex> mu, channel<nint> c, nint n) {
    nint x = 0;

    for (nint i = 0; i < n; i++) {
        if (i < 0) {
            x += ᐸꟷ(c);
        }

        mu.Lock();
        x++;
        mu.Unlock();
    }

    return x;
}

internal static partial nint /*r*/ withDefer(channel<nint> c, nint i) {
    nint r = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            r++;
        }, ref ᒐ);

        if (i < 0) {
            r = ᐸꟷ(c); goto ᒐdone;
        }

        r = i;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static nint deferLoop(channel<nint> c, nint n) {
    nint sum = 0;

    for (nint i = 0; i < n; i++) {
        sum += withDefer(c, i);
    }

    return sum;
}

partial struct memReader {
    internal nint left;
}

internal static (nint, error) Read(this ref memReader m, slice<byte> p) {
    if (m.left == 0) {
        return (0, Δio.EOF);
    }

    nint n = len(p);

    if (n > m.left) {
        n = m.left;
    }

    for (nint i = 0; i < n; i += 64) {
        p[i] = (byte)i;
    }

    m.left -= n;

    return (n, default);
}

partial struct countWriter {
    internal int64 n;
}

internal static (nint, error) Write(this ref countWriter w, slice<byte> p) {
    w.n += (int64)len(p);
    return (len(p), default);
}

internal static nint copyLoop(channel<nint> c, nint chunks) {
    if (chunks < 0) {
        return ᐸꟷ(c);
    }

    var src = Ꮡ(new memReader(left: chunks * 512));
    var dst = Ꮡ(new countWriter());
    var (n, err) = Δio.CopyBuffer(new countWriterжWriter(dst), new memReaderжReader(src), new slice<byte>(512));

    if (err != nil || n != dst.n) {
        return -1;
    }

    return (nint)(n / 512);
}

internal static nint hashLoop(channel<nint> c, nint chunks) {
    if (chunks < 0) {
        return ᐸꟷ(c);
    }

    var src = Ꮡ(new memReader(left: chunks * 512));
    var h = sha256.New();
    var (n, err) = Δio.CopyBuffer(h, new memReaderжReader(src), new slice<byte>(512));

    if (err != nil || len(h.Sum(default)) != sha256.Size) {
        return -1;
    }

    return (nint)(n / 512);
}

internal static (uint64, time.Time) begin() {
    ref var m = ref heap(new runtime.MemStats(), out var Ꮡm);

    runtime.ReadMemStats(Ꮡm);

    return (m.TotalAlloc, time.Now());
}

internal static void end(@string mode, nint rep, nint n, nint got, uint64 alloc, time.Time start) {
    ref var m = ref heap(new runtime.MemStats(), out var Ꮡm);

    var took = time.Since(start);
    runtime.ReadMemStats(Ꮡm);

    fmt.Fprintf(new os.FileжWriter(os.Stderr), "COST mode=%s rep=%d n=%d check=%d ns-per-op=%.2f bytes-per-op=%.2f\n"u8, mode, rep, n, got,
        (float64)took.Nanoseconds() / (float64)n, (float64)(m.TotalAlloc - alloc) / (float64)n);
}

internal static void main() {
    @string mode = os.Getenv("MN_COST_MODE");
    nint n = envInt("MN_COST_N", 1000);
    var c = new channel<nint>(0);
    var mu = Ꮡ(new Δsync.Mutex());
    nint reps = 5;

    if (mode == "hold"u8) {
        time.Sleep(((time.Duration)(int64)n) * time.Millisecond);
    }

    if (mode == ""u8 || mode == "hold"u8) {
        reps = 0;
    }

    for (nint rep = 0; rep < reps; rep++) {
        nint got = 0;
        var (alloc, start) = begin();

        var exprᴛ1 = mode;
        switch (ᐧ) {
        case {} when exprᴛ1 == "call"u8: {
            got = callLoop(c, n);
            break;
        }
        case {} when exprᴛ1 == "mutex"u8: {
            got = mutexLoop(mu, c, n);
            break;
        }
        case {} when exprᴛ1 == "defer"u8: {
            got = deferLoop(c, n);
            break;
        }
        case {} when exprᴛ1 == "copy"u8: {
            got = copyLoop(c, n);
            break;
        }
        case {} when exprᴛ1 == "hash"u8: {
            got = hashLoop(c, n);
            break;
        }}

        end(mode, rep, n, got, alloc, start);
    }

    fmt.Println("ok", callLoop(c, 10), mutexLoop(Ꮡ(new Δsync.Mutex()), c, 10), deferLoop(c, 10), copyLoop(c, 10));
}

} // end main_package
