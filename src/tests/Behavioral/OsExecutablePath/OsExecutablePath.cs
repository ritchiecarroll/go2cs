namespace go;

using fmt = fmt_package;
using os = os_package;
using filepath = path.filepath_package;
using fs = io.fs_package;
using io;
using path;

partial class main_package {

internal static void Main() {
    var (path, err) = os.Executable();
    fmt.Println("error is nil:", err == nil);
    if (err != nil) {
        fmt.Println("error:", err);
        return;
    }
    fmt.Println("path is absolute:", filepath.IsAbs(path));

    var (info, statErr) = os.Stat(path);
    fmt.Println("path exists:", statErr == nil);
    if (statErr == nil) {
        fmt.Println("path is a regular file:", info.Mode().IsRegular());
    }

    var (again, err2) = os.Executable();
    fmt.Println("second call agrees:", err2 == nil && again == path);
}

} // end main_package
