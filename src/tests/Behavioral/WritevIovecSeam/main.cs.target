namespace go;

using fmt = fmt_package;
using Δio = io_package;
using Δnet = net_package;
using System.Runtime.CompilerServices;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string tcpˢ = "tcp"u8;
private static readonly object listenˢ = (@string)"listen:"u8;
private static readonly object dialˢ = (@string)"dial:"u8;
private static readonly object wroteˢ = (@string)"wrote:"u8;
private static readonly object errˢ = (@string)"err:"u8;
private static readonly object lenˢ = (@string)"len:"u8;
private static readonly object bytesˢ = (@string)"bytes:"u8;
private static readonly object eachIovecDeliveredItsOwnˢ = (@string)"each iovec delivered its own byte in order:"u8;

[MethodImpl(MethodImplOptions.NoInlining)] internal static void Main() {
    GoFrame ᒐ = default;
    try {
        var (ln, err) = Δnet.Listen(tcpˢ, "127.0.0.1:0"u8);
        if (err != default!) {
            fmt.Println(listenˢ, err);
            return;
        }
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var done = new channel<slice<byte>>(1);
        var doneʗ1 = done;
        var lnʗ2 = ln;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                var (cΔ1, errΔ1) = lnʗ2.Accept();
                if (errΔ1 != default!) {
                    doneʗ1.ᐸꟷ(default!);
                    return;
                }
                var cʗ1 = cΔ1;
                defer(() => cʗ1.Close(), ref ᒐ);
                var (gotΔ1, _) = Δio.ReadAll(new net_ConnᴠReader(cΔ1));
                doneʗ1.ᐸꟷ(gotΔ1);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        (var c, err) = Δnet.Dial(tcpˢ, ln.Addr().String());
        if (err != default!) {
            fmt.Println(dialˢ, err);
            return;
        }
        ref var bufs = ref heap<Δnet.Buffers>(out var Ꮡbufs);
        bufs = new Δnet.Buffers(new slice<byte>[]{}.slice());
        for (nint i = 0; i < 10; i++) {
            bufs = append(bufs, new byte[]{(byte)i}.slice());
        }
        (var n, err) = Ꮡbufs.WriteTo(new net_ConnᴠWriter(c));
        c._<ж<Δnet.TCPConn>>().CloseWrite();
        fmt.Println(wroteˢ, n, errˢ, err);
        var got = ᐸꟷ(done);
        fmt.Println(lenˢ, len(got));
        fmt.Println(bytesˢ, got);
        var ordered = len(got) == 10;
        for (nint i = 0; ordered && i < 10; i++) {
            ordered = got[i] == (byte)i;
        }
        fmt.Println(eachIovecDeliveredItsOwnˢ, ordered);
        c.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
