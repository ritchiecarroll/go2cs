namespace go;

using fmt = fmt_package;
using os = os_package;
using filepath = path.filepath_package;
using sort = sort_package;
using strings = strings_package;
using fs = io.fs_package;
using io;
using path;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string go2csLongpathˢ = "go2cs-longpath"u8;
private static readonly object mkdirtempFailedˢ = (@string)"mkdirtemp failed:"u8;
private static readonly object exceedsMaxPathˢ = (@string)"exceeds MAX_PATH:"u8;
private static readonly object mkdirallFailedˢ = (@string)"mkdirall failed:"u8;
private static readonly object dirStatˢ = (@string)"dir stat:"u8;
private static readonly @string probeTxtˢ = "probe.txt"u8;
private static readonly @string theQuickBrownFoxJumpsˢ = "the quick brown fox jumps over the lazy dog"u8;
private static readonly object writefileFailedˢ = (@string)"writefile failed:"u8;
private static readonly object readBackˢ = (@string)"read back:"u8;
private static readonly object fileStatˢ = (@string)"file stat:"u8;
private static readonly object appendˢ = (@string)"append:"u8;
private static readonly object openfileFailedˢ = (@string)"openfile failed:"u8;
private static readonly object afterAppendˢ = (@string)"after append:"u8;
private static readonly object secondWritefileFailedˢ = (@string)"second writefile failed:"u8;
private static readonly object readdirˢ = (@string)"readdir:"u8;
private static readonly object renameˢ = (@string)"rename:"u8;
private static readonly object removeOneˢ = (@string)"remove one:"u8;
private static readonly object removeallˢ = (@string)"removeall:"u8;

internal partial struct main_probes /*dyn*/ {
    internal @string label;
    internal @string path;
}

internal static void Main() {
    GoFrame ᒐ = default;
    try {
        var (@base, err) = os.MkdirTemp(""u8, go2csLongpathˢ);
        if (err != default!) {
            fmt.Println(mkdirtempFailedˢ, err);
            return;
        }
        defer(os.RemoveAll, @base, ref ᒐ);
        @string deep = @base;
        for (nint i = 0; i < 12; i++) {
            deep = filepath.Join(deep, strings.Repeat("d"u8, 24));
        }
        fmt.Println(exceedsMaxPathˢ, len(deep) > 260);
        {
            var errΔ1 = os.MkdirAll(deep, 493); if (errΔ1 != default!) {
                fmt.Println(mkdirallFailedˢ, errΔ1);
                return;
            }
        }
        (var dirInfo, err) = os.Stat(deep);
        fmt.Println(dirStatˢ, err == default!, dirInfo != default! && dirInfo.IsDir(), dirInfo != default! && dirInfo.Name() == strings.Repeat("d"u8, 24));
        @string sep = ((@string)(rune)filepath.Separator);
        @string name = probeTxtˢ;
        @string @file = deep + sep + name;
        @string payload = theQuickBrownFoxJumpsˢ;
        {
            var errΔ2 = os.WriteFile(@file, slice<byte>(payload), 420); if (errΔ2 != default!) {
                fmt.Println(writefileFailedˢ, errΔ2);
                return;
            }
        }
        (var data, err) = os.ReadFile(@file);
        fmt.Println(readBackˢ, err == default!, ((sstring)data) == payload, len(data));
        (var info, err) = os.Stat(@file);
        fmt.Println(fileStatˢ, err == default!, info != default! && info.Size() == (int64)len(payload), info != default! && !info.IsDir(), info != default! && info.Name() == name);
        (var handle, err) = os.OpenFile(@file, (nint)(os.O_APPEND | os.O_WRONLY), 420);
        if (err == default!){
            var (_, werr) = handle.WriteString("!"u8);
            var cerr = handle.Close();
            fmt.Println(appendˢ, werr == default!, cerr == default!);
        } else {
            fmt.Println(openfileFailedˢ, err);
        }
        (var grown, err) = os.ReadFile(@file);
        fmt.Println(afterAppendˢ, err == default!, len(grown) == len(payload) + 1, strings.HasSuffix(((@string)grown), "g!"u8));
        @string second = deep + sep + "another.dat"u8;
        {
            var errΔ3 = os.WriteFile(second, new byte[]{1, 2, 3}.slice(), 420); if (errΔ3 != default!) {
                fmt.Println(secondWritefileFailedˢ, errΔ3);
            }
        }
        (var entries, err) = os.ReadDir(deep);
        var names = new slice<@string>(0, len(entries));
        foreach (var (_, entry) in entries) {
            names = append(names, fmt.Sprintf("%s:%v"u8, entry.Name(), entry.IsDir()));
        }
        sort.Strings(names);
        fmt.Println(readdirˢ, err == default!, len(names), strings.Join(names, ","u8));
        var probes = new main_probes[]{
            new("dot-segment"u8, deep + sep + "."u8 + sep + name),
            new("dotdot-segment"u8, deep + sep + "nope"u8 + sep + ".."u8 + sep + name),
            new("double-separator"u8, deep + sep + sep + name),
            new("trailing-dot"u8, @file + "."u8)
        }.slice();
        foreach (var (_, probe) in probes) {
            var (probeInfo, probeErr) = os.Stat(probe.path);
            fmt.Println("normalize " + probe.label + ":", probeErr == default!, probeInfo != default! && probeInfo.Size() == (int64)(len(payload) + 1));
        }
        @string renamed = deep + sep + "renamed.txt"u8;
        var renameErr = os.Rename(@file, renamed);
        var (_, oldStatErr) = os.Stat(@file);
        var (moved, readErr) = os.ReadFile(renamed);
        fmt.Println(renameˢ, renameErr == default!, os.IsNotExist(oldStatErr), readErr == default!, len(moved) == len(payload) + 1);
        fmt.Println(removeOneˢ, os.Remove(renamed) == default!);
        var removeErr = os.RemoveAll(@base);
        var (_, goneErr) = os.Stat(@base);
        fmt.Println(removeallˢ, removeErr == default!, os.IsNotExist(goneErr));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
