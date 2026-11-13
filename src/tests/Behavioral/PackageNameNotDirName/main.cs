namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;
using sqlite3 = PackageNameNotDirName.lib.sqlite3_package;
using PackageNameNotDirName.lib;

partial class main_package {

internal static void busy(@unsafe.Pointer p, int32 t, int64 rows) {
    var db = (ж<sqlite3.Tdb>)(uintptr)(p);
    db.FbusyTimeout = t;
    db.Frows = (libꓸSqlite3_int64)rows;
}

internal static void main() {
    var buf = new slice<int64>(4);
    busy(@unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 0)), 250, 9);
    fmt.Println((int32)buf[0], buf[3]);
}

} // end main_package
