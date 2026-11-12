namespace go;

using fmt = fmt_package;
using Δio = io_package;
using os = os_package;
using strconv = strconv_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static nint envInt(@string name, nint def) {
    var (v, err) = strconv.Atoi(os.Getenv(name)); if (err == nil && v > 0) {
        return v;
    }

    return def;
}

internal static void osReader(ж<os.File> r, nint rounds, channel/*<-*/<nint> @out) {
    var buf = new slice<byte>(1);
    nint sum = 0;

    for (nint i = 0; i < rounds; i++) {
        var (n, err) = r.Read(buf);

        if (err != nil || n != 1) {
            @out.ᐸꟷ(-1);
            return;
        }

        sum += (nint)buf[0];
    }

    @out.ᐸꟷ(sum);
}

internal static void ioReader(ж<Δio.PipeReader> r, nint rounds, channel/*<-*/<nint> @out) {
    var buf = new slice<byte>(1);
    nint sum = 0;

    for (nint i = 0; i < rounds; i++) {
        var (n, err) = r.Read(buf);

        if (err != nil || n != 1) {
            @out.ᐸꟷ(-1);
            return;
        }

        sum += (nint)buf[0];
    }

    @out.ᐸꟷ(sum);
}

internal static void ponger(/*<-*/channel<nint> @in, channel/*<-*/<nint> @out) {
    foreach (var v in @in) {
        @out.ᐸꟷ(v + 1);
    }

    close(@out);
}

internal static partial nint channelWork(nint pairs, nint rounds) {
    var results = new channel<nint>(pairs);

    for (nint p = 0; p < pairs; p++) {
        var resultsʗ1 = results;
        goǃ([MethodImpl(MethodImplOptions.NoInlining)] () => {
            var ping = new channel<nint>(0);
            var pong = new channel<nint>(0);

            goǃ(ponger, ping.AsRecvOnly(), pong.AsSendOnly());

            nint v = 0;

            for (nint i = 0; i < rounds; i++) {
                ping.ᐸꟷ(v);
                v = ᐸꟷ(pong);
            }

            close(ping);
            ᐸꟷ(pong);
            resultsʗ1.ᐸꟷ(v);
        });
    }

    nint total = 0;

    for (nint p = 0; p < pairs; p++) {
        total += ᐸꟷ(results);
    }

    return total;
}

internal static (nint sum, nint bad) collect(/*<-*/channel<nint> @out, nint k) {
    nint sum = default;
    nint bad = default;

    for (nint i = 0; i < k; i++) {
        nint v = ᐸꟷ(@out);

        if (v < 0) {
            bad++;
        } else {
            sum += v;
        }
    }

    return (sum, bad);
}

internal static partial void main() {
    @string kind = os.Getenv("MN_IO_KIND");
    nint k = envInt("MN_IO_K", 8);
    nint rounds = envInt("MN_IO_ROUNDS", 3);
    nint pairs = envInt("MN_IO_PAIRS", 50);

    if (kind == ""u8 || kind == "os"u8) {
        var @out = new channel<nint>(k);
        var writers = new slice<ж<os.File>>(k);

        for (nint i = 0; i < k; i++) {
            var (r, w, err) = os.Pipe();

            if (err != nil) {
                fmt.Println("os.Pipe failed at", i);
                return;
            }

            writers[i] = w;

            goǃ(osReader, r, rounds, @out.AsSendOnly());
        }

        fmt.Println("os readers started:", k);
        fmt.Println("channel work beside os readers:", channelWork(pairs, 100));

        for (nint round = 0; round < rounds; round++) {
            for (nint i = 0; i < k; i++) {
                writers[i].Write([(byte)(round + 1)]);
            }
        }

        var (sum, bad) = collect(@out.AsRecvOnly(), k);

        fmt.Println("os readers done: sum", sum, "bad", bad);
    }

    if (kind == ""u8 || kind == "io"u8) {
        var @out = new channel<nint>(k);
        var writers = new slice<ж<Δio.PipeWriter>>(k);

        for (nint i = 0; i < k; i++) {
            var (r, w) = Δio.Pipe();

            writers[i] = w;

            goǃ(ioReader, r, rounds, @out.AsSendOnly());
        }

        fmt.Println("io readers started:", k);
        fmt.Println("channel work beside io readers:", channelWork(pairs, 100));

        for (nint round = 0; round < rounds; round++) {
            for (nint i = 0; i < k; i++) {
                writers[i].Write([(byte)(round + 1)]);
            }
        }

        var (sum, bad) = collect(@out.AsRecvOnly(), k);

        fmt.Println("io readers done: sum", sum, "bad", bad);
    }
}

} // end main_package
