// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements a simple printer performance benchmark:
// go test -bench=BenchmarkPrint
namespace go.go;

using bytes = bytes_package;
using ast = global::go.go.ast_package;
using parser = global::go.go.parser_package;
using token = global::go.go.token_package;
using io = io_package;
using log = log_package;
using os = os_package;
using testing = testing_package;
using global::go.go;
using static global::go.go.printer_package;

partial class printer_internal_test_package {

internal static ж<ast.File> fileNode;
internal static int64 fileSize;
internal static ast.Decl declNode;
internal static int64 declSize;

internal static void testprint(io.Writer @out, ast.Node node) {
    {
        var err = (Ꮡ(new Config((global::go.go.printer_package.Mode)((global::go.go.printer_package.Mode)(TabIndent | UseSpaces) | normalizeNumbers), 8, 0))).Fprint(@out, fset, node); if (err != default!) {
            log.Fatalf("print error: %s"u8, err);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string filenameᶜ = "testdata/parser.go"u8;

// cannot initialize in init because (printer) Fprint launches goroutines.
internal static void initialize() {
    @string filename = filenameᶜ;
    var (src, err) = os.ReadFile(filename);
    if (err != default!) {
        log.Fatalf("%s"u8, err);
    }
    (var @file, err) = parser.ParseFile(fset, filename, src, parser.ParseComments);
    if (err != default!) {
        log.Fatalf("%s"u8, err);
    }
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    testprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), new ast.FileжNode(@file));
    if (!bytes.Equal(buf.Bytes(), src)) {
        log.Fatalf("print error: %s not idempotent"u8, filename);
    }
    fileNode = @file;
    fileSize = (int64)len(src);
    foreach (var (_, decl) in (~@file).Decls) {
        // The first global variable, which is pretty short:
        //
        //	var unresolved = new(ast.Object)
        {
            var (declΔ1, ok) = decl._<ж<ast.GenDecl>>(ᐧ); if (ok && (~declΔ1).Tok == token.VAR) {
                declNode = new ast.GenDeclжDecl(declΔ1);
                declSize = (int64)(fset.Position(declΔ1.End()).Offset - fset.Position(declΔ1.Pos()).Offset);
                break;
            }
        }
    }
}

public static void BenchmarkPrintFile(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (fileNode == nil) {
        initialize();
    }
    b.ReportAllocs();
    b.SetBytes(fileSize);
    for (nint i = 0; i < b.N; i++) {
        testprint(io.Discard, new ast.FileжNode(fileNode));
    }
}

public static void BenchmarkPrintDecl(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (declNode == default!) {
        initialize();
    }
    b.ReportAllocs();
    b.SetBytes(declSize);
    for (nint i = 0; i < b.N; i++) {
        testprint(io.Discard, declNode);
    }
}

} // end printer_internal_test_package
