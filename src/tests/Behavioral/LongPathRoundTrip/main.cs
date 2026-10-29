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

internal partial struct main_probes /*dyn*/ {
    internal @string label;
    internal @string path;
}

internal static void Main() {
    GoFrame ᒐ = default;
    try {
        var namesᴷ = new stackAppend<@string>();

        var (@base, err) = os.MkdirTemp("", "go2cs-longpath");

        if (err != nil) {
            fmt.Println("mkdirtemp failed:", err);
            return;
        }

        defer(os.RemoveAll, @base, ref ᒐ);

        @string deep = @base;

        for (nint i = 0; i < 12; i++) {
            deep = filepath.Join(deep, strings.Repeat("d", 24));
        }

        fmt.Println("exceeds MAX_PATH:", len(deep) > 260);

        {
            var errΔ1 = os.MkdirAll(deep, 493); if (errΔ1 != nil) {
                fmt.Println("mkdirall failed:", errΔ1);
                return;
            }
        }

        (var dirInfo, err) = os.Stat(deep);
        fmt.Println("dir stat:", err == nil, dirInfo != nil && dirInfo.IsDir(), dirInfo != nil && dirInfo.Name() == strings.Repeat("d", 24));

        @string sep = (@string)(rune)filepath.Separator;
        @string name = "probe.txt";
        @string @file = deep + sep + name;
        @string payload = "the quick brown fox jumps over the lazy dog";

        {
            var errΔ2 = os.WriteFile(@file, slice<byte>(payload), 420); if (errΔ2 != nil) {
                fmt.Println("writefile failed:", errΔ2);
                return;
            }
        }

        (var data, err) = os.ReadFile(@file);
        fmt.Println("read back:", err == nil, ((sstring)data) == payload, len(data));

        (var info, err) = os.Stat(@file);
        fmt.Println("file stat:", err == nil, info != nil && info.Size() == (int64)len(payload), info != nil && !info.IsDir(), info != nil && info.Name() == name);

        (var handle, err) = os.OpenFile(@file, (nint)(os.O_APPEND | os.O_WRONLY), 420);

        if (err == nil) {
            var (_, werr) = handle.WriteString("!");
            var cerr = handle.Close();
            fmt.Println("append:", werr == nil, cerr == nil);
        } else {
            fmt.Println("openfile failed:", err);
        }

        (var grown, err) = os.ReadFile(@file);
        fmt.Println("after append:", err == nil, len(grown) == len(payload) + 1, strings.HasSuffix((@string)grown, "g!"));

        @string second = deep + sep + "another.dat"u8;

        {
            var errΔ3 = os.WriteFile(second, slice<byte>([1, 2, 3]), 420); if (errΔ3 != nil) {
                fmt.Println("second writefile failed:", errΔ3);
            }
        }

        (var entries, err) = os.ReadDir(deep);
        var names = new slice<@string>(0, len(entries));

        foreach (var (_, entry) in entries) {
            names = append(namesᴷ, names, fmt.Sprintf("%s:%v"u8, entry.Name(), entry.IsDir()));
        }

        sort.Strings(names);
        fmt.Println("readdir:", err == nil, len(names), strings.Join(names, ","));

        var probes = slice<main_probes>([
            new("dot-segment", deep + sep + "."u8 + sep + name),
            new("dotdot-segment", deep + sep + "nope"u8 + sep + ".."u8 + sep + name),
            new("double-separator", deep + sep + sep + name),
            new("trailing-dot", @file + "."u8)
        ]);

        foreach (var (_, probe) in probes) {
            var (probeInfo, probeErr) = os.Stat(probe.path);
            fmt.Println("normalize " + probe.label + ":", probeErr == nil, probeInfo != nil && probeInfo.Size() == (int64)(len(payload) + 1));
        }

        @string renamed = deep + sep + "renamed.txt"u8;
        var renameErr = os.Rename(@file, renamed);
        var (_, oldStatErr) = os.Stat(@file);
        var (moved, readErr) = os.ReadFile(renamed);
        fmt.Println("rename:", renameErr == nil, os.IsNotExist(oldStatErr), readErr == nil, len(moved) == len(payload) + 1);

        fmt.Println("remove one:", os.Remove(renamed) == nil);

        var removeErr = os.RemoveAll(@base);
        var (_, goneErr) = os.Stat(@base);
        fmt.Println("removeall:", removeErr == nil, os.IsNotExist(goneErr));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
