namespace go;

using fmt = fmt_package;
using os = os_package;
using filepath = path.filepath_package;
using fs = io.fs_package;
using io;
using path;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object errorIsNilˢ = (@string)"error is nil:"u8;
private static readonly object errorˢ = (@string)"error:"u8;
private static readonly object pathIsAbsoluteˢ = (@string)"path is absolute:"u8;
private static readonly object pathExistsˢ = (@string)"path exists:"u8;
private static readonly object pathIsARegularFileˢ = (@string)"path is a regular file:"u8;
private static readonly object secondCallAgreesˢ = (@string)"second call agrees:"u8;

internal static void Main() {
    var (path, err) = os.Executable();
    fmt.Println(errorIsNilˢ, err == default!);
    if (err != default!) {
        fmt.Println(errorˢ, err);
        return;
    }
    fmt.Println(pathIsAbsoluteˢ, filepath.IsAbs(path));
    var (info, statErr) = os.Stat(path);
    fmt.Println(pathExistsˢ, statErr == default!);
    if (statErr == default!) {
        fmt.Println(pathIsARegularFileˢ, info.Mode().IsRegular());
    }
    var (again, err2) = os.Executable();
    fmt.Println(secondCallAgreesˢ, err2 == default! && again == path);
}

} // end main_package
