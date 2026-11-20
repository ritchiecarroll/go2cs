namespace go.modernc.org.sqlite.lib;

partial class sqlite3_package {

public static UntypedInt SQLITE_ENABLE_LOCKING_STYLE => 0;

public static UntypedInt TIMER_ABSTIME => 1;

internal static UntypedInt _XOPEN_SOURCE => 600;

[GoLazy] partial class lazyᴛ_aMap { internal static array<uint8> _aMap = new array<uint8>(5){
    [0] = (uint8)SQLITE_ROLLBACK,
    [1] = (uint8)SQLITE_ABORT,
    [2] = (uint8)SQLITE_FAIL,
    [3] = (uint8)SQLITE_IGNORE,
    [4] = (uint8)SQLITE_REPLACE
}; }

} // end sqlite3_package
