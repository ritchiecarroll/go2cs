namespace go;

using fmt = fmt_package;
using Δnet = net_package;
using os = os_package;
using filepath = path.filepath_package;
using System.Runtime.CompilerServices;
using path;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string abstractaddrˢ = "abstractaddr"u8;
private static readonly object fatalMkdtempˢ = (@string)"FATAL mkdtemp:"u8;
private static readonly @string unixˢ = "unix"u8;
private static readonly object fatalResolveˢ = (@string)"FATAL resolve:"u8;
private static readonly object fatalListenˢ = (@string)"FATAL listen:"u8;
private static readonly object fatalDialˢ = (@string)"FATAL dial:"u8;
private static readonly object netˢ = (@string)"net       ="u8;
private static readonly object nameLenˢ = (@string)"name len  ="u8;
private static readonly object nameIsˢ = (@string)"name is @ ="u8;

internal static partial void Main() {
    GoFrame ᒐ = default;
    try {
        var (dir, err) = os.MkdirTemp(""u8, abstractaddrˢ);
        if (err != default!) {
            fmt.Println(fatalMkdtempˢ, err);
            return;
        }
        defer(os.RemoveAll, dir, ref ᒐ);
        (var ta, err) = Δnet.ResolveUnixAddr(unixˢ, filepath.Join(dir, "s"));
        if (err != default!) {
            fmt.Println(fatalResolveˢ, err);
            return;
        }
        (var ln, err) = Δnet.ListenUnix(unixˢ, ta);
        if (err != default!) {
            fmt.Println(fatalListenˢ, err);
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
        (var c, err) = Δnet.DialUnix(unixˢ, nil, ta);
        if (err != default!) {
            fmt.Println(fatalDialˢ, err);
            return;
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        var la = c.LocalAddr()._<ж<Δnet.UnixAddr>>();
        fmt.Println(netˢ, (~la).Net);
        fmt.Println(nameLenˢ, len((~la).Name));
        fmt.Println(nameIsˢ, (~la).Name == "@"u8);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
