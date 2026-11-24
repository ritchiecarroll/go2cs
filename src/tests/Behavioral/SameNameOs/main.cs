namespace go;

using fmt = fmt_package;
using os = os_package;
using los = SameNameOs.hooks.os_package;
using fs = io.fs_package;
using io;

partial class main_package {

internal static @string describe(fs.FileInfo fi) {
    return fmt.Sprintf("%s %d %v"u8, fi.Name(), fi.Size(), fi.IsDir());
}

internal static void main() {
    var h = los.NewFileInfo("a.txt", 42);
    fmt.Println(describe(new os_FileInfoжFileInfo(h)));

    fs.FileInfo fi = new os_FileInfoжFileInfo(los.NewFileInfo("b.txt", 7));
    fmt.Println(fi.Name(), fi.Size());
}

} // end main_package
