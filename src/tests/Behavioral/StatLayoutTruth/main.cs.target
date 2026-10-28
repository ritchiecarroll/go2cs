namespace go;

using fmt = fmt_package;
using fs = io.fs_package;
using os = os_package;
using filepath = path.filepath_package;
using time = time_package;
using io;
using path;

partial class main_package {

internal static void Main() {
    GoFrame ᒐ = default;
    try {
        var (dir, err) = os.MkdirTemp("", "statlayouttruth");
        if (err != nil) {
            fmt.Println("MkdirTemp error:", err);
            return;
        }
        defer(os.RemoveAll, dir, ref ᒐ);

        @string sub = filepath.Join(dir, "sub");
        {
            var errΔ1 = os.Mkdir(sub, 493); if (errΔ1 != nil) {
                fmt.Println("Mkdir error:", errΔ1);
                return;
            }
        }
        var payload = slice<byte>("0123456789abcdef"u8);
        @string @file = filepath.Join(dir, "a.txt");
        {
            var errΔ2 = os.WriteFile(@file, payload, 420); if (errΔ2 != nil) {
                fmt.Println("WriteFile error:", errΔ2);
                return;
            }
        }
        {
            var errΔ3 = os.WriteFile(filepath.Join(sub, "b.txt"), payload[..5], 420); if (errΔ3 != nil) {
                fmt.Println("WriteFile error:", errΔ3);
                return;
            }
        }

        (var fi, err) = os.Stat(dir);
        if (err != nil) {
            fmt.Println("Stat dir error:", err);
            return;
        }
        fmt.Println("stat dir: IsDir =", fi.IsDir(), "IsRegular =", fi.Mode().IsRegular());

        (fi, err) = os.Stat(@file);
        if (err != nil) {
            fmt.Println("Stat file error:", err);
            return;
        }
        fmt.Println("stat file: IsDir =", fi.IsDir(), "IsRegular =", fi.Mode().IsRegular(), "Size =", fi.Size(), "Name =", fi.Name());
        fmt.Println("stat file: modtime within a day =", time.Since(fi.ModTime()) < (time.Duration)(86400000000000L), "nonzero =", !fi.ModTime().IsZero());

        (fi, err) = os.Lstat(@file);
        if (err != nil) {
            fmt.Println("Lstat file error:", err);
            return;
        }
        fmt.Println("lstat file: Size =", fi.Size(), "IsRegular =", fi.Mode().IsRegular());

        (var f, err) = os.Open(@file);
        if (err != nil) {
            fmt.Println("Open error:", err);
            return;
        }
        (fi, err) = f.Stat();
        f.Close();
        if (err != nil) {
            fmt.Println("File.Stat error:", err);
            return;
        }
        fmt.Println("fstat: Size =", fi.Size(), "IsRegular =", fi.Mode().IsRegular());

        (var matches, err) = filepath.Glob(filepath.Join(dir, "*.txt"));
        fmt.Println("glob *.txt:", len(matches), err);

        (var entries, err) = os.ReadDir(dir);
        fmt.Println("readdir:", len(entries), err);
        foreach (var (_, e) in entries) {
            var (info, ierr) = e.Info();
            fmt.Println("  entry:", e.Name(), "IsDir =", e.IsDir(), "Info agrees =", ierr == nil && info.IsDir() == e.IsDir());
        }

        nint visited = 0;
        nint files = 0;
        var werr = filepath.WalkDir(dir, error (@string p, fs.DirEntry d, error errΔ4) => {
            if (errΔ4 != nil) {
                return errΔ4;
            }
            visited++;
            if (!d.IsDir()) {
                files++;
            }
            return default;
        });
        fmt.Println("walkdir: visited =", visited, "files =", files, "err =", werr);

        fmt.Println("pid > 0:", os.Getpid() > 0, "ppid >= 0:", os.Getppid() >= 0);
        fmt.Println("uid callable:", os.Getuid() >= -1, "gid callable:", os.Getgid() >= -1);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
