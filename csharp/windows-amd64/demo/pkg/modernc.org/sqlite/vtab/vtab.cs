global using Value = object;

namespace go.modernc.org.sqlite;

using sql = database.sql_package;
using driver = database.sql.driver_package;
using errors = errors_package;
using database;
using database.sql;
using ꓸꓸꓸint32 = Span<int32>;
using static vtab_package;

partial class vtab_package {

partial struct Context {
    internal Func<@string, error> declare;
    internal Func<error> constraintSupport;
    internal Funcꓸꓸꓸ<int32, int32, error> config;
}

} partial class vtabᴛContext {

public static error Declare(this Context c, @string schema) {
    if (c.declare == nil) {
        return errors.New("vtab: declare not available in this context");
    }
    return c.declare(schema);
}

public static error EnableConstraintSupport(this Context c) {
    if (c.constraintSupport == nil) {
        return errors.New("vtab: constraint support not available in this context");
    }
    return c.constraintSupport();
}

public static error Config(this Context c, int32 op, params ꓸꓸꓸint32 argsʗp) {
    var args = argsʗp.sslice();

    if (c.config == nil) {
        return errors.New("vtab: config not available in this context");
    }
    return c.config(op, args.ꓸꓸꓸ);
}

} partial class vtab_package {

public static Context NewContext(Func<@string, error> declare) { return new Context(declare: declare); }

public static Context NewContextWithConstraintSupport(Func<@string, error> declare, Func<error> constraintSupport) {
    return new Context(declare: declare, constraintSupport: constraintSupport);
}

public static Context NewContextWithConfig(Func<@string, error> declare, Func<error> constraintSupport, Funcꓸꓸꓸ<int32, int32, error> config) {
    return new Context(declare: declare, constraintSupport: constraintSupport, config: config);
}

partial interface Module {
    (Table, error) Create(Context ctx, slice<@string> args);
    (Table, error) Connect(Context ctx, slice<@string> args);
}

partial interface Table {
    error BestIndex(ж<IndexInfo> info);
    (Cursor, error) Open();
    error Disconnect();
    error Destroy();
}

partial interface Renamer {
    error Rename(@string newName);
}

partial interface Transactional :
    driver.Tx
{
    error Begin();
    error Sync();
    error Savepoint(nint i);
    error Release(nint i);
    error RollbackTo(nint i);
}

partial interface Cursor {
    error Filter(nint idxNum, @string idxStr, slice<Value> vals);
    error Next();
    bool Eof();
    (driverꓸValue, error) Column(nint col);
    (int64, error) Rowid();
    error Close();
}

partial interface Updater {
    error Insert(slice<Value> cols, ж<int64> rowid);
    error Update(int64 oldRowid, slice<Value> cols, ж<int64> newRowid);
    error Delete(int64 oldRowid);
}

partial interface VolatileArgsOpter {
    bool VolatileArgs();
}

partial struct ConstraintOp /*num:nint*/;

public static ConstraintOp OpUnknown => /* iota */ 0;
public static ConstraintOp OpEQ => 1;
public static ConstraintOp OpGT => 2;
public static ConstraintOp OpLE => 3;
public static ConstraintOp OpLT => 4;
public static ConstraintOp OpGE => 5;
public static ConstraintOp OpMATCH => 6;
public static ConstraintOp OpNE => 7;
public static ConstraintOp OpIS => 8;
public static ConstraintOp OpISNOT => 9;
public static ConstraintOp OpISNULL => 10;
public static ConstraintOp OpISNOTNULL => 11;
public static ConstraintOp OpLIKE => 12;
public static ConstraintOp OpGLOB => 13;
public static ConstraintOp OpREGEXP => 14;
public static ConstraintOp OpFUNCTION => 15;
public static ConstraintOp OpLIMIT => 16;
public static ConstraintOp OpOFFSET => 17;

partial struct Constraint {
    public nint Column;
    public ConstraintOp Op;
    public bool Usable;
    public nint ArgIndex;
    public bool Omit;
}

partial struct OrderBy {
    public nint Column;
    public bool Desc;
}

partial struct IndexInfo {
    public slice<Constraint> Constraints;
    public slice<OrderBy> OrderBy;
    public int64 IdxNum;
    public @string IdxStr;
    public nint IdxFlags;
    public bool OrderByConsumed;
    public float64 EstimatedCost;
    public int64 EstimatedRows;
    public uint64 ColUsed;
}

public static UntypedInt IndexScanUnique => 1;

public static error ErrNotImplemented = errors.New("vtab: RegisterModule not wired into engine");

internal static Func<@string, Module, error> registerHook;

public static void SetRegisterFunc(Func<@string, Module, error> fn) { registerHook = fn; }

public static error RegisterModule(ж<sql.DB> Ꮡdb, @string name, Module m) {
    ref var db = ref Ꮡdb.DerefOrNull();

    if (name == ""u8) {
        return errors.New("vtab: module name must be non-empty");
    }
    if (m == nil) {
        return errors.New("vtab: module implementation is nil");
    }
    if (Ꮡdb != nil) {
        var (r, ok) = db.Driver()._<ModuleRegisterer>(ᐧ); if (ok) {
            return r.RegisterModule(name, m);
        }
    }
    if (registerHook == nil) {
        return ErrNotImplemented;
    }
    return registerHook(name, m);
}

partial interface ModuleRegisterer {
    error RegisterModule(@string name, Module m);
}

} // end vtab_package
