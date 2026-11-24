namespace go.SameNameOs.hooks;

using fs = io.fs_package;
using time = time_package;
using io;

partial class os_package {

partial struct FileInfo {
    internal @string name;
    internal int64 size;
}

public static ж<FileInfo> NewFileInfo(@string name, int64 size) { return Ꮡ(new FileInfo(name, size)); }

internal static @string Name⁔(/*this*/ ref FileInfo f) { return f.name; }
internal static int64 Size⁔(/*this*/ ref FileInfo f) { return f.size; }
internal static fs.FileMode Mode⁔(/*this*/ ref FileInfo f) { return 420; }
internal static time.Time ModTime⁔(/*this*/ ref FileInfo f) { return new time.Time(); }
internal static bool IsDir⁔(/*this*/ ref FileInfo f) { return false; }
internal static any Sys⁔(/*this*/ ref FileInfo f) { return default; }

} // end os_package
