global using ptrdiff_t = long;
global using sqlite3_io_methods = go.modernc.org.sqlite.lib.sqlite3_package.Tsqlite3_io_methodsᴛ1;
global using timezone = go.modernc.org.sqlite.lib.sqlite3_package.Ttimezoneᴛ1;
global using uint_fast32_t = uint;

namespace go.modernc.org.sqlite.lib;

partial class sqlite3_package {

public static UntypedInt EOF => -1;

public static UntypedInt FOPEN_MAX => 20;

public static UntypedInt SEEK_CUR => 1;

public static UntypedInt SEEK_END => 2;

public static UntypedInt SEEK_SET => 0;

public static UntypedInt SQLITE_MUTEX_NOOP => 1;

} // end sqlite3_package
