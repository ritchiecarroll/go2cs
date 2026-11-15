global using Sqlite3_vtab_cursor = go.ForwardAliasChainRecord.lib_package.Tsqlite3_vtab_cursorᴛ1;
global using Tsqlite3_vtab_cursor = go.ForwardAliasChainRecord.lib_package.Tsqlite3_vtab_cursorᴛ1;
global using Cursor = go.ForwardAliasChainRecord.lib_package.Tsqlite3_vtab_cursorᴛ1;
global using sqlite3_vtab_cursor = go.ForwardAliasChainRecord.lib_package.Tsqlite3_vtab_cursorᴛ1;

namespace go.ForwardAliasChainRecord;

partial class lib_package {

public partial struct Tsqlite3_vtab_cursorᴛ1 /*dyn*/ {
    public uintptr FpVtab;
    public int32 FnRef;
}

public static Sqlite3_vtab_cursor Open(int32 n) { return new Sqlite3_vtab_cursor(FpVtab: 7, FnRef: n); }

} // end lib_package
