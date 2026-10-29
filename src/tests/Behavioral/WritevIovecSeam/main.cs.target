namespace go;

using fmt = fmt_package;
using Δio = io_package;
using Δnet = net_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static partial void Main() {
    GoFrame ᒐ = default;
    try {
        var (ln, err) = Δnet.Listen("tcp", "127.0.0.1:0");
        if (err != nil) {
            fmt.Println("listen:", err);
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
                if (errΔ1 != nil) {
                    doneʗ1.ᐸꟷ(default);
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

        (var c, err) = Δnet.Dial("tcp", ln.Addr().String());
        if (err != nil) {
            fmt.Println("dial:", err);
            return;
        }

        ref var bufs = ref heap<Δnet.Buffers>(new Δnet.Buffers(new slice<byte>[]{}.slice()), out var Ꮡbufs);
        for (nint i = 0; i < 10; i++) {
            bufs = append(bufs, slice<byte>([(byte)i]));
        }

        (var n, err) = Ꮡbufs.WriteTo(new net_ConnᴠWriter(c));
        c._<ж<Δnet.TCPConn>>().CloseWrite();
        fmt.Println("wrote:", n, "err:", err);

        var got = ᐸꟷ(done);
        fmt.Println("len:", len(got));
        fmt.Println("bytes:", got);

        var ordered = len(got) == 10;
        for (nint i = 0; ordered && i < 10; i++) {
            ordered = got[i] == (byte)i;
        }
        fmt.Println("each iovec delivered its own byte in order:", ordered);
        c.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
