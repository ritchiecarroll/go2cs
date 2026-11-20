namespace go.sqlitecs;

using sql = database.sql_package;
using fmt = fmt_package;
using os = os_package;
// blank import: modernc.org.sqlite_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using database;

partial class main_package {

internal static void main() {
    GoFrame ᒐ = default;
    try {
        var (db, err) = sql.Open("sqlite", ":memory:");
        if (err != nil) {
            fmt.Println("open:", err);
            os.Exit(1);
        }
        var dbʗ1 = db;
        defer(() => dbʗ1.Close(), ref ᒐ);
        foreach (var (_, q) in range(slice<@string>([
            "create table t(id integer primary key, name text, score real)",
            "insert into t(name, score) values ('alpha', 1.5), ('beta', 2.25), ('gamma', -3)"
        ]))) {
            var (_, errΔ1) = db.Exec(q); if (errΔ1 != nil) {
                fmt.Println("exec:", errΔ1);
                os.Exit(1);
            }
        }
        ref var version = ref heap(new @string(), out var Ꮡversion);
        var errΔ2 = db.QueryRow("select sqlite_version()").Scan(Ꮡversion); if (errΔ2 != nil) {
            fmt.Println("version:", errΔ2);
            os.Exit(1);
        }
        fmt.Println("sqlite", version);
        (var rows, err) = db.Query("select id, name, score from t order by score desc");
        if (err != nil) {
            fmt.Println("query:", err);
            os.Exit(1);
        }
        var rowsʗ1 = rows;
        defer(() => rowsʗ1.Close(), ref ᒐ);
        while (rows.Next()) {
            ref var id = ref heap(new nint(), out var Ꮡid);
            ref var name = ref heap(new @string(), out var Ꮡname);
            ref var score = ref heap(new float64(), out var Ꮡscore);
            var errΔ3 = rows.Scan(Ꮡid, Ꮡname, Ꮡscore); if (errΔ3 != nil) {
                fmt.Println("scan:", errΔ3);
                os.Exit(1);
            }
            fmt.Println(id, name, score);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
