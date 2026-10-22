namespace go;

using fmt = fmt_package;
using os = os_package;
using filepath = path.filepath_package;
using sort = sort_package;
using strings = strings_package;
using syscall = syscall_package;
using time = time_package;
using fs = io.fs_package;
using path;

partial class main_package {

partial struct entry {
    internal @string name;
    internal @string alt;
    internal bool isDir;
    internal uint32 size;
    internal int64 mtime;
}


partial struct dirsᴛ1 /*dyn*/ {
    internal @string rel;
    internal time.Time stamp;
}
internal static slice<dirsᴛ1> dirs = new dirsᴛ1[]{
    new("", time.Date(2021, time.February, 3, 4, 5, 6, 0, time.ΔUTC)),
    new("LongDirectoryNameWellBeyondEightDotThree", time.Date(2021, time.February, 3, 5, 6, 7, 0, time.ΔUTC)),
    new("ÜnicödeDir", time.Date(2021, time.February, 3, 6, 7, 8, 0, time.ΔUTC))
}.slice();

partial struct filesᴛ1 /*dyn*/ {
    internal @string rel;
    internal nint size;
    internal time.Time stamp;
}
internal static slice<filesᴛ1> files = new filesᴛ1[]{
    new("ReadMe.txt", 7, time.Date(2022, time.March, 4, 5, 6, 7, 0, time.ΔUTC)),
    new("LongDirectoryNameWellBeyondEightDotThree\\Payload.bin", 300, time.Date(2023, time.April, 5, 6, 7, 8, 0, time.ΔUTC)),
    new("ÜnicödeDir\\Inner.dat", 65, time.Date(2024, time.May, 6, 7, 8, 9, 0, time.ΔUTC))
}.slice();

internal static void Main() {
    @string root = filepath.Join(os.TempDir(), fmt.Sprintf("go2cs_findfirstfile_%d"u8, os.Getpid()));
    os.RemoveAll(root);

    build(root);

    foreach (var (_, d) in dirs) {
        fmt.Printf("-- enumerate %s --\n"u8, ascii(shown(d.rel)));
        foreach (var (_, e) in enumerate(filepath.Join(root, d.rel))) {
            fmt.Printf("%s alt=%s dir=%v size=%d mtime=%d\n"u8, ascii(e.name), ascii(e.alt), e.isDir, e.size, e.mtime);
        }
    }

    fmt.Println("-- single lookup --");
    var one = lookup(filepath.Join(root, "ReadMe.txt"));
    fmt.Printf("%s alt=%s dir=%v size=%d mtime=%d\n"u8, ascii(one.name), ascii(one.alt), one.isDir, one.size, one.mtime);

    fmt.Println("-- missing --");
    var (_, err) = find(filepath.Join(root, "NoSuchFile.txt"));
    fmt.Println(AreEqual(err, syscall.ERROR_FILE_NOT_FOUND));

    fmt.Println("-- canonical case --");
    (var rootEval, err) = filepath.EvalSymlinks(root);
    if (err != default!) {
        fatal("EvalSymlinks(root)", err);
    }
    foreach (var (_, f) in files) {
        var (got, errΔ1) = filepath.EvalSymlinks(strings.ToLower(filepath.Join(root, f.rel)));
        if (errΔ1 != default!) {
            fatal("EvalSymlinks", errΔ1);
        }
        (var rel, errΔ1) = filepath.Rel(rootEval, got);
        if (errΔ1 != default!) {
            fatal("Rel", errΔ1);
        }
        fmt.Println(ascii(rel));
    }

    os.RemoveAll(root);
}

internal static void build(@string root) {
    foreach (var (_, d) in dirs) {
        {
            var err = os.MkdirAll(filepath.Join(root, d.rel), 493); if (err != default!) {
                fatal("MkdirAll", err);
            }
        }
    }

    foreach (var (_, f) in files) {
        var body = new slice<byte>(f.size);
        foreach (var i in range(body)) {
            body[i] = (byte)((rune)'a' + i % 26);
        }
        {
            var err = os.WriteFile(filepath.Join(root, f.rel), body, 420); if (err != default!) {
                fatal("WriteFile", err);
            }
        }
    }

    foreach (var (_, f) in files) {
        {
            var err = os.Chtimes(filepath.Join(root, f.rel), f.stamp, f.stamp); if (err != default!) {
                fatal("Chtimes", err);
            }
        }
    }
    for (nint i = len(dirs) - 1; i >= 0; i--) {
        {
            var err = os.Chtimes(filepath.Join(root, dirs[i].rel), dirs[i].stamp, dirs[i].stamp); if (err != default!) {
                fatal("Chtimes", err);
            }
        }
    }
}

internal static slice<entry> enumerate(@string dir) {
    GoFrame ᒐ = default;
    try {
        var (pattern, err) = syscall.UTF16PtrFromString(dir + @"\*"u8);
        if (err != default!) {
            fatal("UTF16PtrFromString", err);
        }

        ref var data = ref heap(new syscall.Win32finddata(), out var Ꮡdata);

        (var h, err) = syscall.FindFirstFile(pattern, Ꮡdata);
        if (err != default!) {
            fatal("FindFirstFile", err);
        }
        defer(syscall.FindClose, h, ref ᒐ);

        slice<entry> @out = default!;
        while (ᐧ) {
            {
                var (e, ok) = toEntry(Ꮡdata); if (ok) {
                    @out = append(@out, e);
                }
            }
            {
                var errΔ1 = syscall.FindNextFile(h, Ꮡdata); if (errΔ1 != default!) {
                    if (AreEqual(errΔ1, syscall.ERROR_NO_MORE_FILES)) {
                        break;
                    }
                    fatal("FindNextFile", errΔ1);
                }
            }
        }

        var outʗ1 = @out;
        sort.Slice(@out, (nint i, nint j) => outʗ1[i].name < outʗ1[j].name);
        return @out;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static entry lookup(@string path) {
    var (e, err) = find(path);
    if (err != default!) {
        fatal("FindFirstFile", err);
    }
    return e;
}

internal static (entry, error) find(@string path) {
    var (p, err) = syscall.UTF16PtrFromString(path);
    if (err != default!) {
        return (new entry(nil), err);
    }

    ref var data = ref heap(new syscall.Win32finddata(), out var Ꮡdata);

    (var h, err) = syscall.FindFirstFile(p, Ꮡdata);
    if (err != default!) {
        return (new entry(nil), err);
    }
    syscall.FindClose(h);

    var (e, _) = toEntry(Ꮡdata);
    return (e, default!);
}

internal static (entry, bool) toEntry(ж<syscall.Win32finddata> Ꮡdata) {
    ref var data = ref Ꮡdata.DerefOrNull();

    @string name = syscall.UTF16ToString(data.FileName[..]);
    if (name == "."u8 || name == ".."u8) {
        return (new entry(nil), false);
    }
    return (new entry(
        name: name,
        alt: altName(ref (Ꮡdata).DerefOrNull()),
        isDir: (data.FileAttributes & (uint32)syscall.FILE_ATTRIBUTE_DIRECTORY) != 0,
        size: data.FileSizeLow,
        mtime: data.LastWriteTime.Nanoseconds()
    ), true);
}

internal static @string altName(ref syscall.Win32finddata data) {
    if (data.AlternateFileName[0] == 0) {
        return "";
    }
    return syscall.UTF16ToString(data.AlternateFileName[..]);
}

internal static @string shown(@string rel) {
    if (rel == ""u8) {
        return "<root>";
    }
    return rel;
}

internal static @string ascii(@string s) {
    @string hexDigits = "0123456789abcdef";

    var @out = new slice<byte>(0, len(s));
    foreach (var (_, r) in s) {
        if (r >= 0x20 && r < 0x7f) {
            @out = append(@out, (byte)r);
            continue;
        }
        @out = append(@out, (byte)((rune)'\\'), (byte)((rune)'u'));
        for (nint shift = 12; shift >= 0; shift -= 4) {
            @out = append(@out, hexDigits[((r.Rsh((nuint)shift)) & 0xf)]);
        }
    }
    return (@string)@out;
}

internal static void fatal(@string what, error err) {
    fmt.Println("FATAL", what, err);
    os.Exit(1);
}

} // end main_package
