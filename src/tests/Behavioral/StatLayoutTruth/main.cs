namespace go;

using fmt = fmt_package;
using fs = io.fs_package;
using os = os_package;
using filepath = path.filepath_package;
using time = time_package;
using io;
using path;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object mkdirTempErrorˢ = (@string)"MkdirTemp error:"u8;
private static readonly object mkdirErrorˢ = (@string)"Mkdir error:"u8;
private static readonly object writeFileErrorˢ = (@string)"WriteFile error:"u8;
private static readonly object statDirErrorˢ = (@string)"Stat dir error:"u8;
private static readonly object statDirIsDirˢ = (@string)"stat dir: IsDir ="u8;
private static readonly object isRegularˢ = (@string)"IsRegular ="u8;
private static readonly object statFileErrorˢ = (@string)"Stat file error:"u8;
private static readonly object statFileIsDirˢ = (@string)"stat file: IsDir ="u8;
private static readonly object sizeˢ = (@string)"Size ="u8;
private static readonly object nameˢ = (@string)"Name ="u8;
private static readonly object statFileModtimeWithinAˢ = (@string)"stat file: modtime within a day ="u8;
private static readonly object nonzeroˢ = (@string)"nonzero ="u8;
private static readonly object lstatFileErrorˢ = (@string)"Lstat file error:"u8;
private static readonly object lstatFileSizeˢ = (@string)"lstat file: Size ="u8;
private static readonly object openErrorˢ = (@string)"Open error:"u8;
private static readonly object fileStatErrorˢ = (@string)"File.Stat error:"u8;
private static readonly object fstatSizeˢ = (@string)"fstat: Size ="u8;
private static readonly object globTxtˢ = (@string)"glob *.txt:"u8;
private static readonly object readdirˢ = (@string)"readdir:"u8;
private static readonly object entryˢ = (@string)"  entry:"u8;
private static readonly object isDirˢ = (@string)"IsDir ="u8;
private static readonly object infoAgreesˢ = (@string)"Info agrees ="u8;
private static readonly object walkdirVisitedˢ = (@string)"walkdir: visited ="u8;
private static readonly object filesˢ = (@string)"files ="u8;
private static readonly object errˢ = (@string)"err ="u8;
private static readonly object pid0ˢ = (@string)"pid > 0:"u8;
private static readonly object ppid0ˢ = (@string)"ppid >= 0:"u8;
private static readonly object uidCallableˢ = (@string)"uid callable:"u8;
private static readonly object gidCallableˢ = (@string)"gid callable:"u8;

internal static void Main() {
    GoFrame ᒐ = default;
    try {
        var (dir, err) = os.MkdirTemp("", "statlayouttruth");
        if (err != default!) {
            fmt.Println(mkdirTempErrorˢ, err);
            return;
        }
        defer(os.RemoveAll, dir, ref ᒐ);

        @string sub = filepath.Join(dir, "sub");
        {
            var errΔ1 = os.Mkdir(sub, 493); if (errΔ1 != default!) {
                fmt.Println(mkdirErrorˢ, errΔ1);
                return;
            }
        }
        var payload = slice<byte>("0123456789abcdef"u8);
        @string @file = filepath.Join(dir, "a.txt");
        {
            var errΔ2 = os.WriteFile(@file, payload, 420); if (errΔ2 != default!) {
                fmt.Println(writeFileErrorˢ, errΔ2);
                return;
            }
        }
        {
            var errΔ3 = os.WriteFile(filepath.Join(sub, "b.txt"), payload[..5], 420); if (errΔ3 != default!) {
                fmt.Println(writeFileErrorˢ, errΔ3);
                return;
            }
        }

        (var fi, err) = os.Stat(dir);
        if (err != default!) {
            fmt.Println(statDirErrorˢ, err);
            return;
        }
        fmt.Println(statDirIsDirˢ, fi.IsDir(), isRegularˢ, fi.Mode().IsRegular());

        (fi, err) = os.Stat(@file);
        if (err != default!) {
            fmt.Println(statFileErrorˢ, err);
            return;
        }
        fmt.Println(statFileIsDirˢ, fi.IsDir(), isRegularˢ, fi.Mode().IsRegular(), sizeˢ, fi.Size(), nameˢ, fi.Name());
        fmt.Println(statFileModtimeWithinAˢ, time.Since(fi.ModTime()) < (time.Duration)(86400000000000L), nonzeroˢ, !fi.ModTime().IsZero());

        (fi, err) = os.Lstat(@file);
        if (err != default!) {
            fmt.Println(lstatFileErrorˢ, err);
            return;
        }
        fmt.Println(lstatFileSizeˢ, fi.Size(), isRegularˢ, fi.Mode().IsRegular());

        (var f, err) = os.Open(@file);
        if (err != default!) {
            fmt.Println(openErrorˢ, err);
            return;
        }
        (fi, err) = f.Stat();
        f.Close();
        if (err != default!) {
            fmt.Println(fileStatErrorˢ, err);
            return;
        }
        fmt.Println(fstatSizeˢ, fi.Size(), isRegularˢ, fi.Mode().IsRegular());

        (var matches, err) = filepath.Glob(filepath.Join(dir, "*.txt"));
        fmt.Println(globTxtˢ, len(matches), err);

        (var entries, err) = os.ReadDir(dir);
        fmt.Println(readdirˢ, len(entries), err);
        foreach (var (_, e) in entries) {
            var (info, ierr) = e.Info();
            fmt.Println(entryˢ, e.Name(), isDirˢ, e.IsDir(), infoAgreesˢ, ierr == default! && info.IsDir() == e.IsDir());
        }

        nint visited = 0;
        nint files = 0;
        var werr = filepath.WalkDir(dir, error (@string p, fs.DirEntry d, error errΔ4) => {
            if (errΔ4 != default!) {
                return errΔ4;
            }
            visited++;
            if (!d.IsDir()) {
                files++;
            }
            return default!;
        });
        fmt.Println(walkdirVisitedˢ, visited, filesˢ, files, errˢ, werr);

        fmt.Println(pid0ˢ, os.Getpid() > 0, ppid0ˢ, os.Getppid() >= 0);
        fmt.Println(uidCallableˢ, os.Getuid() >= -1, gidCallableˢ, os.Getgid() >= -1);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
