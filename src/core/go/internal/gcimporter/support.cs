// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements support functionality for ureader.go.
namespace go.go.@internal;

using fmt = fmt_package;
using token = global::go.go.token_package;
using pkgbits = global::go.@internal.pkgbits_package;
using sync = sync_package;
using global::go.@internal;
using global::go.go;
using ꓸꓸꓸany = Span<any>;

partial class gcimporter_package {

internal static void assert(bool b) {
    if (!b) {
        throw panic("assertion failed");
    }
}

internal static void errorf(@string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    throw panic(fmt.Sprintf(format, args.ꓸꓸꓸ));
}

// Synthesize a token.Pos
[GoType] partial struct fakeFileSet {
    internal ж<token.FileSet> fset;
    internal map<@string, ж<fileInfo>> files;
}

[GoType] partial struct fileInfo {
    internal ж<tokenꓸFile> @file;
    internal nint lastline;
}

internal static UntypedInt maxlines => /* 64 * 1024 */ 65536;

[GoRecv] internal static tokenꓸPos pos(this ref fakeFileSet s, @string @file, nint line, nint column) {
    // TODO(mdempsky): Make use of column.
    // Since we don't know the set of needed file positions, we reserve
    // maxlines positions per file. We delay calling token.File.SetLines until
    // all positions have been calculated (by way of fakeFileSet.setLines), so
    // that we can avoid setting unnecessary lines. See also golang/go#46586.
    var f = s.files[@file];
    if (f == nil) {
        f = Ꮡ(new fileInfo(@file: s.fset.AddFile(@file, -1, maxlines)));
        s.files[@file] = f;
    }
    if (line > maxlines) {
        line = 1;
    }
    if (line > (~f).lastline) {
        f.Value.lastline = line;
    }
    // Return a fake position assuming that f.file consists only of newlines.
    return ((tokenꓸPos)((~f).@file.Base() + line - 1));
}

[GoRecv] internal static void setLines(this ref fakeFileSet s) {
    ᏑfakeLinesOnce.Do(() => {
        fakeLines = new slice<nint>(maxlines);
        foreach (var (i, _) in fakeLines) {
            fakeLines[i] = i;
        }
    });
    foreach (var (_, f) in s.files) {
        (~f).@file.SetLines(fakeLines.slice(0, (~f).lastline));
    }
}

internal static slice<nint> fakeLines;
internal static ж<sync.Once> ᏑfakeLinesOnce = new StandardBox<sync.Once>(default(sync.Once));
internal static ref sync.Once fakeLinesOnce => ref ᏑfakeLinesOnce.Value;

// See cmd/compile/internal/noder.derivedInfo.
[GoType] partial struct derivedInfo {
    internal pkgbits.Index idx;
    internal bool needed;
}

// See cmd/compile/internal/noder.typeInfo.
[GoType] partial struct typeInfo {
    internal pkgbits.Index idx;
    internal bool derived;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string dotᶜ = "·"u8;

// See cmd/compile/internal/types.SplitVargenSuffix.
internal static (@string @base, @string suffix) splitVargenSuffix(@string name) {
    nint i = len(name);
    while (i > 0 && name[i - 1] >= (rune)'0' && name[i - 1] <= (rune)'9') {
        i--;
    }
    @string dot = dotᶜ;
    if (i >= len(dot) && name.slice(i - len(dot), i) == dot) {
        i -= len(dot);
        return (name.slice(0, i), name.slice(i));
    }
    return (name, "");
}

} // end gcimporter_package
