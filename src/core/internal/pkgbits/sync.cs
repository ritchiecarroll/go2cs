// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using fmt = fmt_package;
using Δruntime = runtime_package;
using strings = strings_package;
using ꓸꓸꓸuintptr = Span<uintptr>;

partial class pkgbits_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string cmdCompileInternalNoderˢ = "cmd/compile/internal/noder."u8;

// fmtFrames formats a backtrace for reporting reader/writer desyncs.
internal static slice<@string> fmtFrames(params ꓸꓸꓸuintptr pcsʗp) {
    var pcs = pcsʗp.slice();

    ref var res = ref heap<slice<@string>>(out var Ꮡres);
    res = new slice<@string>(0, len(pcs));
    walkFrames(pcs, (@string @file, nint line, @string name, uintptr offset) => {
        // Trim package from function name. It's just redundant noise.
        name = strings.TrimPrefix(name, cmdCompileInternalNoderˢ);
        Ꮡres.ValueSlot = append(Ꮡres.ValueSlot, fmt.Sprintf("%s:%v: %s +0x%v"u8, @file, line, name, offset));
    });
    return res;
}

// type frameVisitor is a methodless func type — rendered inline as its base delegate

// walkFrames calls visit for each call frame represented by pcs.
//
// pcs should be a slice of PCs, as returned by runtime.Callers.
internal static void walkFrames(slice<uintptr> pcs, Action<@string, nint, @string, uintptr> visit) {
    if (len(pcs) == 0) {
        return;
    }
    var frames = Δruntime.CallersFrames(pcs);
    while (ᐧ) {
        var (frame, more) = frames.Next();
        visit(frame.File, frame.Line, frame.Function, frame.PC - frame.Entry);
        if (!more) {
            return;
        }
    }
}

partial struct SyncMarker /*num:nint*/;

//go:generate stringer -type=SyncMarker -trimprefix=Sync
internal static SyncMarker _ᴛ1ʗ => /* iota */ 0;
// Public markers (known to go/types importers).
public static SyncMarker SyncEOF => 1;
public static SyncMarker SyncBool => 2;
public static SyncMarker SyncInt64 => 3;
public static SyncMarker SyncUint64 => 4;
public static SyncMarker SyncString => 5;
public static SyncMarker SyncValue => 6;
public static SyncMarker SyncVal => 7;
public static SyncMarker SyncRelocs => 8;
public static SyncMarker SyncReloc => 9;
public static SyncMarker SyncUseReloc => 10;
public static SyncMarker SyncPublic => 11;
public static SyncMarker SyncPos => 12;
public static SyncMarker SyncPosBase => 13;
public static SyncMarker SyncObject => 14;
public static SyncMarker SyncObject1 => 15;
public static SyncMarker SyncPkg => 16;
public static SyncMarker SyncPkgDef => 17;
public static SyncMarker SyncMethod => 18;
public static SyncMarker SyncType => 19;
public static SyncMarker SyncTypeIdx => 20;
public static SyncMarker SyncTypeParamNames => 21;
public static SyncMarker SyncSignature => 22;
public static SyncMarker SyncParams => 23;
public static SyncMarker SyncParam => 24;
public static SyncMarker SyncCodeObj => 25;
public static SyncMarker SyncSym => 26;
public static SyncMarker SyncLocalIdent => 27;
public static SyncMarker SyncSelector => 28;
public static SyncMarker SyncPrivate => 29;
public static SyncMarker SyncFuncExt => 30;
public static SyncMarker SyncVarExt => 31;
public static SyncMarker SyncTypeExt => 32;
public static SyncMarker SyncPragma => 33;
public static SyncMarker SyncExprList => 34;
public static SyncMarker SyncExprs => 35;
public static SyncMarker SyncExpr => 36;
public static SyncMarker SyncExprType => 37;
public static SyncMarker SyncAssign => 38;
public static SyncMarker SyncOp => 39;
public static SyncMarker SyncFuncLit => 40;
public static SyncMarker SyncCompLit => 41;
public static SyncMarker SyncDecl => 42;
public static SyncMarker SyncFuncBody => 43;
public static SyncMarker SyncOpenScope => 44;
public static SyncMarker SyncCloseScope => 45;
public static SyncMarker SyncCloseAnotherScope => 46;
public static SyncMarker SyncDeclNames => 47;
public static SyncMarker SyncDeclName => 48;
public static SyncMarker SyncStmts => 49;
public static SyncMarker SyncBlockStmt => 50;
public static SyncMarker SyncIfStmt => 51;
public static SyncMarker SyncForStmt => 52;
public static SyncMarker SyncSwitchStmt => 53;
public static SyncMarker SyncRangeStmt => 54;
public static SyncMarker SyncCaseClause => 55;
public static SyncMarker SyncCommClause => 56;
public static SyncMarker SyncSelectStmt => 57;
public static SyncMarker SyncDecls => 58;
public static SyncMarker SyncLabeledStmt => 59;
public static SyncMarker SyncUseObjLocal => 60;
public static SyncMarker SyncAddLocal => 61;
public static SyncMarker SyncLinkname => 62;
public static SyncMarker SyncStmt1 => 63;
public static SyncMarker SyncStmtsEnd => 64;
public static SyncMarker SyncLabel => 65;
public static SyncMarker SyncOptLabel => 66;
public static SyncMarker SyncMultiExpr => 67;
public static SyncMarker SyncRType => 68;
public static SyncMarker SyncConvRTTI => 69;

} // end pkgbits_package
