namespace go.SameNameOs.hooks;

using fs = io.fs_package;
using time = time_package;
using io;
using static os_package;

partial class os_package {

partial struct FileInfo {
    internal @string name;
    internal int64 size;
}

public static ж<FileInfo> NewFileInfo(@string name, int64 size) { return Ꮡ(new FileInfo(name, size)); }

} partial class osᴛFileInfo {

public static @string Name(this ref FileInfo f) { return f.name; }
public static int64 Size(this ref FileInfo f) { return f.size; }
public static fs.FileMode Mode(this ref FileInfo f) { return 420; }
public static time.Time ModTime(this ref FileInfo f) { return new time.Time(); }
public static bool IsDir(this ref FileInfo f) { return false; }
public static any Sys(this ref FileInfo f) { return default; }

}
