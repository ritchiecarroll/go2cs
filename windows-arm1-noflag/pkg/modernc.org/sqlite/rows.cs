// Copyright 2025 The Sqlite Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using context = context_package;
using driver = database.sql.driver_package;
using fmt = fmt_package;
using io = io_package;
using math = math_package;
using reflect = reflect_package;
using strings = strings_package;
using time = time_package;
using sqlite3 = go.modernc.org.sqlite.lib.sqlite3_package;
using database.sql;
using Δlibc = go.modernc.org.libc_package;
using static sqlite_package;

partial class sqlite_package {

partial struct rows {
    internal slice<cBinding> allocs;
    internal ж<conn> c;
    internal slice<@string> columns;
    internal slice<@string> decltypes;
    internal slice<int8> parseFmtIdx;
    internal uintptr pstmt;
    internal context.Context ctx;
    internal bool doStep;
    internal bool empty;
    internal bool reuseStmt;
}

internal static (ж<rows> r, error err) newRows(context.Context ctx, ж<conn> Ꮡc, uintptr pstmt, ж<slice<cBinding>> Ꮡallocs, bool empty) {
    ж<rows> r = default;
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();
        ref var allocs = ref Ꮡallocs.DerefOrNull();
        ref var err = ref Ꮡerr.ValueSlot;

        slice<cBinding> a = default;
        if (Ꮡallocs != nil) {
            a = allocs;
            allocs = default;
        }
        r = Ꮡ(new rows(c: Ꮡc, pstmt: pstmt, allocs: a, empty: empty, ctx: ctx));

        defer(() => {
            if (Ꮡerr.ValueSlot != nil) {
                r.Close();
                r = default;
            }
        }, ref ᒐ);

        (var n, err) = c.columnCount(pstmt);
        if (err != nil) {
            (r, err) = (default, err); goto ᒐdone;
        }

        r.columns = new slice<@string>(n);
        r.decltypes = new slice<@string>(n);
        r.parseFmtIdx = new slice<int8>(n);
        foreach (var i in range(len(r.columns))) {
            (r.columns[i], err) = r.c.columnName(pstmt, i); if (err != nil) {
                (r, err) = (default, err); goto ᒐdone;
            }
            r.decltypes[i] = strings.ToUpper(r.c.columnDeclType(pstmt, i));
            r.parseFmtIdx[i] = (int8)(-1);
        }

        (r, err) = (r, default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (r, Ꮡerr.ValueSlot);
}

} partial class sqliteᴛrows {

internal static error /*err*/ Close(this ж<rows> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    var allocs = r.allocs;
    r.allocs = default;
    r.c.freeAllocs(allocs);

    var allocsʗ1 = allocs;
    return r.c.withWaitCtx(r.ctx, () => {
        if (Ꮡr.Value.reuseStmt) {
            var e = Ꮡr.Value.c.reset(Ꮡr.Value.pstmt); if (e != nil) {
                return e;
            }
            return Ꮡr.Value.c.clearBindings(Ꮡr.Value.pstmt, allocsʗ1);
        }

        return Ꮡr.Value.c.finalize(Ꮡr.Value.pstmt);
    });
}

internal static slice<@string> /*c*/ Columns(this ref rows r) {
    return r.columns;
}

internal static error /*err*/ Next(this ж<rows> Ꮡr, slice<driverꓸValue> dest) {
    error err = default;
    ref var r = ref Ꮡr.DerefOrNull();

    if (r.empty) {
        return io.EOF;
    }

    nint rc = sqlite3.SQLITE_ROW;
    if (r.doStep) {
        (rc, err) = Ꮡr.step(); if (err != nil) {
            return err;
        }
    }

    r.doStep = true;
    switch (rc) {
    case var ᴛ when ᴛ == sqlite3.SQLITE_ROW: {
        nint g = len(dest);
        nint e = len(r.columns); if (g != e) {
            return fmt.Errorf("sqlite: Next: have %v destination values, expected %v"u8, g, e);
        }

        foreach (var i in range(len(dest))) {
            var (ct, errΔ7) = r.c.columnType(r.pstmt, i);
            if (errΔ7 != nil) {
                return errΔ7;
            }

            switch (ct) {
            case var ᴛ2 when ᴛ2 == sqlite3.SQLITE_INTEGER: {
                var (v, errΔ9) = r.c.columnInt64(r.pstmt, i);
                if (errΔ9 != nil) {
                    return errΔ9;
                }

                if (!r.c.intToTime) {
                    dest[i] = v;
                } else {
                    var exprᴛ3 = r.ColumnTypeDatabaseTypeName(i);
                    switch (ᐧ) {
                    case {} when exprᴛ3 == "DATE"u8 || exprᴛ3 == "DATETIME"u8 || exprᴛ3 == "TIMESTAMP"u8: {
                        var exprᴛ4 = r.c.integerTimeFormat;
                        switch (ᐧ) {
                        case {} when exprᴛ4 == "unix_micro"u8: {
                            dest[i] = r.c.applyTimezone(time.UnixMicro(v).UTC());
                            continue;
                            break;
                        }
                        case {} when exprᴛ4 == "unix_nano"u8: {
                            dest[i] = r.c.applyTimezone(time.Unix(0, v).UTC());
                            continue;
                            break;
                        }}

                        if (v > 1000000000000 || v < -1000000000000) {
                            dest[i] = r.c.applyTimezone(time.UnixMilli(v).UTC());
                        } else {
                            dest[i] = r.c.applyTimezone(time.Unix(v, 0).UTC());
                        }
                        break;
                    }
                    default: {
                        dest[i] = v;
                        break;
                    }}

                }
                break;
            }
            case var ᴛ2 when ᴛ2 == sqlite3.SQLITE_FLOAT: {
                var (v, errΔ10) = r.c.columnDouble(r.pstmt, i);
                if (errΔ10 != nil) {
                    return errΔ10;
                }

                dest[i] = v;
                break;
            }
            case var ᴛ2 when ᴛ2 == sqlite3.SQLITE_TEXT: {
                var (v, errΔ11) = r.c.columnText(r.pstmt, i);
                if (errΔ11 != nil) {
                    return errΔ11;
                }

                var exprᴛ5 = r.ColumnTypeDatabaseTypeName(i);
                switch (ᐧ) {
                case {} when exprᴛ5 == "DATE"u8 || exprᴛ5 == "DATETIME"u8 || exprᴛ5 == "TIMESTAMP"u8: {
                    var (val, ok, idx) = r.c.parseTime(v, (nint)r.parseFmtIdx[i]);
                    if (ok && r.parseFmtIdx[i] < 0 && idx >= 0) {
                        r.parseFmtIdx[i] = (int8)idx;
                    }
                    dest[i] = val;
                    break;
                }
                default: {
                    if (r.c.textToTime && r.decltypes[i] == "") {
                        var (val, ok, idx) = r.c.parseTime(v, (nint)r.parseFmtIdx[i]); if (ok) {
                            if (r.parseFmtIdx[i] < 0 && idx >= 0) {
                                r.parseFmtIdx[i] = (int8)idx;
                            }
                            dest[i] = val;
                            continue;
                        }
                    }
                    dest[i] = v;
                    break;
                }}

                break;
            }
            case var ᴛ2 when ᴛ2 == sqlite3.SQLITE_BLOB: {
                var (v, errΔ12) = r.c.columnBlob(r.pstmt, i);
                if (errΔ12 != nil) {
                    return errΔ12;
                }

                dest[i] = v;
                break;
            }
            case var ᴛ2 when ᴛ2 == sqlite3.SQLITE_NULL: {
                dest[i] = default;
                break;
            }
            default: {
                return fmt.Errorf("internal error: rc %d"u8, rc);
            }}

        }
        return default;
    }
    case var ᴛ when ᴛ == sqlite3.SQLITE_DONE: {
        return io.EOF;
    }
    default: {
        return r.c.errstr((int32)rc);
    }}

}

internal static (nint rc, error err) step(this ж<rows> Ꮡr) {
    nint rc = default;
    error err = default;
    ref var r = ref Ꮡr.DerefOrNull();

    if (r.ctx == nil || r.ctx.Done() == nil) {
        return r.c.step(r.pstmt);
    }

    err = r.c.withWaitCtx(r.ctx, () => {
        error errΔ1 = default;
        (rc, errΔ1) = Ꮡr.Value.c.step(Ꮡr.Value.pstmt);
        return errΔ1;
    });
    return (rc, err);
}

internal static @string ColumnTypeDatabaseTypeName(this ref rows r, nint index) {
    return r.decltypes[index];
}

internal static (int64 length, bool ok) ColumnTypeLength(this ref rows r, nint index) {
    var (t, err) = r.c.columnType(r.pstmt, index);
    if (err != nil) {
        return (0, false);
    }

    switch (t) {
    case var ᴛ when ᴛ == sqlite3.SQLITE_INTEGER: {
        return (0, false);
    }
    case var ᴛ when ᴛ == sqlite3.SQLITE_FLOAT: {
        return (0, false);
    }
    case var ᴛ when ᴛ == sqlite3.SQLITE_TEXT: {
        return (math.MaxInt64, true);
    }
    case var ᴛ when ᴛ == sqlite3.SQLITE_BLOB: {
        return (math.MaxInt64, true);
    }
    case var ᴛ when ᴛ == sqlite3.SQLITE_NULL: {
        return (0, false);
    }
    default: {
        return (0, false);
    }}

}

internal static (bool nullable, bool ok) ColumnTypeNullable(this ref rows r, nint index) {
    return (true, true);
}

internal static (int64 precision, int64 scale, bool ok) ColumnTypePrecisionScale(this ref rows r, nint index) {
    return (0, 0, false);
}

internal static reflect.Type ColumnTypeScanType(this ref rows r, nint index) {
    var (t, err) = r.c.columnType(r.pstmt, index);
    if (err != nil) {
        return reflect.TypeOf("");
    }

    switch (t) {
    case var ᴛ when ᴛ == sqlite3.SQLITE_INTEGER: {
        var exprᴛ2 = r.decltypes[index];
        switch (ᐧ) {
        case {} when exprᴛ2 == "BOOLEAN"u8: {
            return reflect.TypeOf(false);
        }
        case {} when exprᴛ2 == "DATE"u8 || exprᴛ2 == "DATETIME"u8 || exprᴛ2 == "TIME"u8 || exprᴛ2 == "TIMESTAMP"u8: {
            return reflect.TypeOf(new time.Time());
        }
        default: {
            return reflect.TypeOf((int64)0);
        }}

        break;
    }
    case var ᴛ when ᴛ == sqlite3.SQLITE_FLOAT: {
        return reflect.TypeOf((float64)0D);
    }
    case var ᴛ when ᴛ == sqlite3.SQLITE_TEXT: {
        if (r.c.textToTime) {
            var exprᴛ3 = r.decltypes[index];
            switch (ᐧ) {
            case {} when exprᴛ3 == "DATE"u8 || exprᴛ3 == "DATETIME"u8 || exprᴛ3 == "TIME"u8 || exprᴛ3 == "TIMESTAMP"u8: {
                return reflect.TypeOf(new time.Time());
            }}

        }
        return reflect.TypeOf("");
    }
    case var ᴛ when ᴛ == sqlite3.SQLITE_BLOB: {
        return reflect.TypeOf(slice<byte>(default));
    }
    case var ᴛ when ᴛ == sqlite3.SQLITE_NULL: {
        return reflect.TypeOf(default);
    }
    default: {
        return reflect.TypeOf("");
    }}

}

} partial class sqliteᴛconn {

internal static error reset(this ref conn c, uintptr pstmt) {
    var rc = sqlite3.Xsqlite3_reset(c.tls, pstmt); if (rc != sqlite3.SQLITE_OK) {
        return c.errstr(rc);
    }
    return default;
}

} // end sqliteᴛconn
