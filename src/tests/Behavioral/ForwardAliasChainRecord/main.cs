namespace go;

using fmt = fmt_package;
using lib = ForwardAliasChainRecord.lib_package;
using ForwardAliasChainRecord;

partial class main_package {

internal static void main() {
    libꓸSqlite3_vtab_cursor c = lib.Open(2);
    c.FpVtab++;

    libꓸCursor d = c;
    d.FnRef *= 10;

    fmt.Println(c.FpVtab, c.FnRef, d.FpVtab, d.FnRef);

    libꓸVisitor v = new lib_DoublerᴠTvisitorᴛ1(new lib.Doubler(By: 3));
    fmt.Println(v.Visit(14));
}

} // end main_package
