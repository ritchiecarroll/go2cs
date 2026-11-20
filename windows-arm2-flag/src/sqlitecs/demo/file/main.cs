namespace go.sqlitecs.demo;

using sql = database.sql_package;
using fmt = fmt_package;
using os = os_package;
using filepath = path.filepath_package;
// blank import: modernc.org.sqlite_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using database;
using fs = io.fs_package;
using io;
using path;

partial class main_package {

internal static void fail(@string step, error err) {
    fmt.Println(step + ":", err);
    os.Exit(1);
}

internal static ж<sql.DB> open(@string path) {
    var (db, err) = sql.Open("sqlite", path);
    if (err != nil) {
        fail("open", err);
    }
    db.SetMaxOpenConns(1);
    return db;
}

internal static void create(@string path, @string mode) {
    var db = open(path);
    if (mode == "wal"u8) {
        ref var got = ref heap(new @string(), out var Ꮡgot);
        var errΔ1 = db.QueryRow("pragma journal_mode=wal").Scan(Ꮡgot); if (errΔ1 != nil) {
            fail("journal_mode", errΔ1);
        }
        fmt.Printf("[%s] set journal_mode=%s\n"u8, mode, got);
    }
    foreach (var (_, q) in range(slice<@string>([
        "create table t(id integer primary key, name text, score real)",
        "insert into t(name, score) values ('alpha', 1.5), ('beta', 2.25), ('gamma', -3)",
        "create index t_name on t(name)"
    ]))) {
        var (_, errΔ2) = db.Exec(q); if (errΔ2 != nil) {
            fail("exec", errΔ2);
        }
    }
    var errΔ3 = db.Close(); if (errΔ3 != nil) {
        fail("close", errΔ3);
    }
    var (info, err) = os.Stat(path);
    if (err != nil) {
        fail("stat", err);
    }
    fmt.Printf("[%s] created: file present=%t, nonempty=%t\n"u8, mode, true, info.Size() > 0);
}

internal static void reopen(@string path, @string mode) {
    var db = open(path);
    ref var journal = ref heap(new @string(), out var Ꮡjournal);
    ref var integrity = ref heap(new @string(), out var Ꮡintegrity);
    ref var pages = ref heap(new nint(), out var Ꮡpages);
    ref var count = ref heap(new nint(), out var Ꮡcount);
    var errΔ1 = db.QueryRow("pragma journal_mode").Scan(Ꮡjournal); if (errΔ1 != nil) {
        fail("journal_mode", errΔ1);
    }
    var errΔ2 = db.QueryRow("pragma integrity_check").Scan(Ꮡintegrity); if (errΔ2 != nil) {
        fail("integrity_check", errΔ2);
    }
    var errΔ3 = db.QueryRow("pragma page_count").Scan(Ꮡpages); if (errΔ3 != nil) {
        fail("page_count", errΔ3);
    }
    var errΔ4 = db.QueryRow("select count(*) from t").Scan(Ꮡcount); if (errΔ4 != nil) {
        fail("count", errΔ4);
    }
    fmt.Printf("[%s] reopened: journal_mode=%s integrity=%s pages=%d rows=%d\n"u8, mode, journal, integrity, pages, count);
    var (rows, err) = db.Query("select id, name, score from t order by score desc");
    if (err != nil) {
        fail("query", err);
    }
    while (rows.Next()) {
        ref var id = ref heap(new nint(), out var Ꮡid);
        ref var nameΔ1 = ref heap(new @string(), out var ᏑnameΔ1);
        ref var score = ref heap(new float64(), out var Ꮡscore);
        var errΔ5 = rows.Scan(Ꮡid, ᏑnameΔ1, Ꮡscore); if (errΔ5 != nil) {
            fail("scan", errΔ5);
        }
        fmt.Printf("[%s] %d %s %v\n"u8, mode, id, nameΔ1, score);
    }
    var errΔ6 = rows.Err(); if (errΔ6 != nil) {
        fail("rows", errΔ6);
    }
    rows.Close();
    ref var name = ref heap(new @string(), out var Ꮡname);
    var errΔ7 = db.QueryRow("select name from t where name >= 'b' order by name limit 1").Scan(Ꮡname); if (errΔ7 != nil) {
        fail("index", errΔ7);
    }
    fmt.Printf("[%s] first name from b: %s\n"u8, mode, name);
    var errΔ8 = db.Close(); if (errΔ8 != nil) {
        fail("close", errΔ8);
    }
}

internal static void main() {
    GoFrame ᒐ = default;
    try {
        var (dir, err) = os.MkdirTemp("", "sqlitecs-demo-file-");
        if (err != nil) {
            fail("tempdir", err);
        }
        defer(os.RemoveAll, dir, ref ᒐ);

        ref var version = ref heap(new @string(), out var Ꮡversion);
        var db = open(":memory:");
        var errΔ1 = db.QueryRow("select sqlite_version()").Scan(Ꮡversion); if (errΔ1 != nil) {
            fail("version", errΔ1);
        }
        db.Close();
        fmt.Println("sqlite", version);

        foreach (var (_, mode) in range(slice<@string>(["delete", "wal"]))) {
            @string path = filepath.Join(dir, "demo-" + mode + ".db");
            create(path, mode);
            reopen(path, mode);
            foreach (var (_, suffix) in range(slice<@string>(["", "-wal", "-shm"]))) {
                var errΔ2 = os.Remove(path + suffix); if (errΔ2 != nil && !os.IsNotExist(errΔ2)) {
                    fail("remove", errΔ2);
                }
            }
            var (_, errΔ3) = os.Stat(path);
            fmt.Printf("[%s] removed: %t\n"u8, mode, os.IsNotExist(errΔ3));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
