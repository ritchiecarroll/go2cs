namespace go;

using fmt = fmt_package;
using Δnet = net_package;
using os = os_package;
using filepath = path.filepath_package;
using System.Runtime.CompilerServices;
using path;

partial class main_package {

internal static partial void Main() {
    GoFrame ᒐ = default;
    try {
        var (dir, err) = os.MkdirTemp("", "abstractaddr");
        if (err != default!) {
            fmt.Println("FATAL mkdtemp:", err);
            return;
        }
        defer(os.RemoveAll, dir, ref ᒐ);

        (var ta, err) = Δnet.ResolveUnixAddr("unix", filepath.Join(dir, "s"));
        if (err != default!) {
            fmt.Println("FATAL resolve:", err);
            return;
        }
        (var ln, err) = Δnet.ListenUnix("unix", ta);
        if (err != default!) {
            fmt.Println("FATAL listen:", err);
            return;
        }
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var lnʗ2 = ln;
        goǃ(() => {
            {
                var (cΔ1, e) = lnʗ2.Accept(); if (e == default!) {
                    cΔ1.Close();
                }
            }
        });

        (var c, err) = Δnet.DialUnix("unix", nil, ta);
        if (err != default!) {
            fmt.Println("FATAL dial:", err);
            return;
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);

        var la = c.LocalAddr()._<ж<Δnet.UnixAddr>>();
        fmt.Println("net       =", (~la).Net);
        fmt.Println("name len  =", len((~la).Name));
        fmt.Println("name is @ =", (~la).Name == "@"u8);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
