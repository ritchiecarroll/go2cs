// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package format implements standard formatting of Go source.
//
// Note that formatting of Go source code changes over time, so tools relying on
// consistent formatting should execute a specific version of the gofmt binary
// instead of using this package. That way, the formatting will be stable, and
// the tools won't need to be recompiled each time gofmt changes.
//
// For example, pre-submit checks that use this package directly would behave
// differently depending on what Go version each developer uses, causing the
// check to be inherently fragile.
namespace go.go;

using bytes = bytes_package;
using fmt = fmt_package;
using ast = global::go.go.ast_package;
using parser = global::go.go.parser_package;
using printer = global::go.go.printer_package;
using token = global::go.go.token_package;
using io = io_package;
using global::go.go;

partial class format_package {

// Keep these in sync with cmd/gofmt/gofmt.go.
internal static UntypedInt tabWidth => 8;
internal static printer.Mode printerMode => /* printer.UseSpaces | printer.TabIndent | printerNormalizeNumbers */ 1073741830;

// printerNormalizeNumbers means to canonicalize number literal prefixes
// and exponents while printing. See https://golang.org/doc/go1.13#gofmt.
//
// This value is defined in go/printer specifically for go/format and cmd/gofmt.
internal static UntypedInt printerNormalizeNumbers => /* 1 << 30 */ 1073741824;

internal static readonly ж<printer.Config> Ꮡconfig = new StaticBox<printer.Config>(new printer.Config(Mode: printerMode, Tabwidth: tabWidth));
internal static ref printer.Config config => ref ((StaticBox<printer.Config>)Ꮡconfig).ValueSlot;

internal static parser.Mode parserMode => /* parser.ParseComments | parser.SkipObjectResolution */ 68;

// Node formats node in canonical gofmt style and writes the result to dst.
//
// The node type must be *[ast.File], *[printer.CommentedNode], [][ast.Decl],
// [][ast.Stmt], or assignment-compatible to [ast.Expr], [ast.Decl], [ast.Spec],
// or [ast.Stmt]. Node does not modify node. Imports are not sorted for
// nodes representing partial source files (for instance, if the node is
// not an *[ast.File] or a *[printer.CommentedNode] not wrapping an *[ast.File]).
//
// The function may return early (before the entire result is written)
// and return a formatting error, for instance due to an incorrect AST.
public static error Node(io.Writer dst, ж<token.FileSet> fset, any node) {
    // Determine if we have a complete source file (file != nil).
    ж<ast.File> @file = default;
    ж<printer.CommentedNode> cnode = default;
    switch (node.type()) {
    case ж<ast.File> n: {
        @file = n;
        break;
    }
    case ж<printer.CommentedNode> n: {
        {
            var (f, ok) = n.Node._<ж<ast.File>>(ᐧ); if (ok) {
                @file = f;
                cnode = n;
            }
        }
        break;
    }}

    // Sort imports if necessary.
    if (@file != nil && hasUnsortedImports(ref (@file).DerefOrNull())) {
        // Make a copy of the AST because ast.SortImports is destructive.
        // TODO(gri) Do this more efficiently.
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        var err = Ꮡconfig.Fprint(new bytes_BufferжWriter(Ꮡbuf), fset, @file.OrTypedNil());
        if (err != nil) {
            return err;
        }
        (@file, err) = parser.ParseFile(fset, ""u8, buf.Bytes(), parserMode);
        if (err != nil) {
            // We should never get here. If we do, provide good diagnostic.
            return fmt.Errorf("format.Node internal error (%s)"u8, err);
        }
        ast.SortImports(fset, @file);

        // Use new file with sorted imports.
        node = @file.OrTypedNil();
        if (cnode != nil) {
            node = Ꮡ(new printer.CommentedNode(Node: @file.OrTypedNil(), Comments: cnode.Comments));
        }
    }

    return Ꮡconfig.Fprint(dst, fset, node);
}

// Source formats src in canonical gofmt style and returns the result
// or an (I/O or syntax) error. src is expected to be a syntactically
// correct Go source file, or a list of Go declarations or statements.
//
// If src is a partial source file, the leading and trailing space of src
// is applied to the result (such that it has the same leading and trailing
// space as src), and the result is indented by the same amount as the first
// line of src containing code. Imports are not sorted for partial source files.
public static (slice<byte>, error) Source(slice<byte> src) {
    var fset = token.NewFileSet();
    var (@file, sourceAdj, indentAdj, err) = parse(fset, ""u8, src, true);
    if (err != nil) {
        return (default, err);
    }

    if (sourceAdj == nil) {
        // Complete source file.
        // TODO(gri) consider doing this always.
        ast.SortImports(fset, @file);
    }

    return format(fset, @file, sourceAdj, indentAdj, src, config);
}

internal static bool hasUnsortedImports(ref ast.File @file) {
    foreach (var (_, d) in @file.Decls) {
        var (dΔ1, ok) = d._<ж<ast.GenDecl>>(ᐧ);
        if (!ok || dΔ1.Tok != token.IMPORT) {
            // Not an import declaration, so we're done.
            // Imports are always first.
            return false;
        }
        if (dΔ1.Lparen.IsValid()) {
            // For now assume all grouped imports are unsorted.
            // TODO(gri) Should check if they are sorted already.
            return true;
        }
        // Ungrouped imports are sorted by default.
    }
    return false;
}

} // end format_package
