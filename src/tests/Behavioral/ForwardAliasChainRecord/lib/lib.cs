global using Sqlite3_vtab_cursor = go.ForwardAliasChainRecord.lib_package.Tsqlite3_vtab_cursorᴛ1;
global using Tsqlite3_vtab_cursor = go.ForwardAliasChainRecord.lib_package.Tsqlite3_vtab_cursorᴛ1;
global using Cursor = go.ForwardAliasChainRecord.lib_package.Tsqlite3_vtab_cursorᴛ1;
global using sqlite3_vtab_cursor = go.ForwardAliasChainRecord.lib_package.Tsqlite3_vtab_cursorᴛ1;
global using Visitor = go.ForwardAliasChainRecord.lib_package.Tvisitorᴛ1;
global using Tvisitor = go.ForwardAliasChainRecord.lib_package.Tvisitorᴛ1;
global using visitor = go.ForwardAliasChainRecord.lib_package.Tvisitorᴛ1;

namespace go.ForwardAliasChainRecord;

partial class lib_package {

public partial struct Tsqlite3_vtab_cursorᴛ1 /*dyn*/ {
    public uintptr FpVtab;
    public int32 FnRef;
}

public static Sqlite3_vtab_cursor Open(int32 n) { return new Sqlite3_vtab_cursor(FpVtab: 7, FnRef: n); }

partial interface Tvisitorᴛ1 /*dyn*/ {
    int32 Visit(int32 n);
}

partial struct Doubler {
    public int32 By;
}

internal static int32 Visit⁔(/*this*/ Doubler d, int32 n) { return n * d.By; }

} // end lib_package
