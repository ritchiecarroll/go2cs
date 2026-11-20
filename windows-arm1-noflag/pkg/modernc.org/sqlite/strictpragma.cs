// Copyright 2026 The Sqlite Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using errors = errors_package;
using strings = strings_package;
using atomic = go.sync.atomic_package;
using go.sync;

partial class sqlite_package {

public static error ErrMultiStatementPragma = errors.New(
    "sqlite: _pragma value is more than one SQL statement");

internal static readonly ж<atomic.Bool> ᏑstrictPragmas = new StaticBox<atomic.Bool>(default(atomic.Bool));
internal static ref atomic.Bool strictPragmas => ref ((StaticBox<atomic.Bool>)ᏑstrictPragmas).ValueSlot;

public static bool /*prev*/ StrictPragmas(bool on) {
    return ᏑstrictPragmas.Swap(on);
}

public static bool StrictPragmasEnabled() {
    return ᏑstrictPragmas.Load();
}

internal static bool singleStatement(@string sql) {
    {
        nint i = strings.IndexByte(sql, 0); if (i >= 0) {
            sql = sql.slice(0, i);
        }
    }

    nint end = statementEnd(sql);
    if (end < 0) {
        return true;
    }

    for (nint i = end + 1; i < len(sql); ) {
        var c = sql[i];
        switch (ᐧ) {
        case {} when c == (byte)';' || c == (byte)' ' || c == (byte)'\t' || c == (byte)'\n' || c == (byte)'\f' || c == (byte)'\r': {
            i++;
            break;
        }
        case {} when c == (byte)'-' && i + 1 < len(sql) && sql[i + 1] == (byte)'-': {
            i = lineCommentEnd(sql, i);
            break;
        }
        case {} when c == (byte)'/' && i + 2 < len(sql) && sql[i + 1] == (byte)'*': {
            i = blockCommentEnd(sql, i);
            break;
        }
        default: {
            return false;
        }}

    }
    return true;
}

internal static nint statementEnd(@string sql) {
    for (nint i = 0; i < len(sql); ) {
        var c = sql[i];
        switch (ᐧ) {
        case {} when c is (byte)';': {
            return i;
        }
        case {} when c == (byte)'\'' || c == (byte)'"' || c == (byte)'`': {
            i = quotedEnd(sql, i, c);
            break;
        }
        case {} when c is (byte)'[': {
            i = quotedEnd(sql, i, (byte)']');
            break;
        }
        case {} when c == (byte)'-' && i + 1 < len(sql) && sql[i + 1] == (byte)'-': {
            i = lineCommentEnd(sql, i);
            break;
        }
        case {} when c == (byte)'/' && i + 2 < len(sql) && sql[i + 1] == (byte)'*': {
            i = blockCommentEnd(sql, i);
            break;
        }
        default: {
            i++;
            break;
        }}

    }
    return -1;
}

internal static nint quotedEnd(@string sql, nint i, byte close) {
    for (nint j = i + 1; j < len(sql); j++) {
        if (sql[j] != close) {
            continue;
        }
        if (close != ']' && j + 1 < len(sql) && sql[j + 1] == close) {
            j++;
            continue;
        }
        return j + 1;
    }
    return len(sql);
}

internal static nint lineCommentEnd(@string sql, nint i) {
    for (nint j = i + 2; j < len(sql); j++) {
        if (sql[j] == '\n') {
            return j + 1;
        }
    }
    return len(sql);
}

internal static nint blockCommentEnd(@string sql, nint i) {
    for (nint j = i + 2; j + 1 < len(sql); j++) {
        if (sql[j] == '*' && sql[j + 1] == '/') {
            return j + 2;
        }
    }
    return len(sql);
}

} // end sqlite_package
