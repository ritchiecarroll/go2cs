// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using bytes = bytes_package;
using fmt = fmt_package;
using ast = global::go.go.ast_package;
using parser = global::go.go.parser_package;
using printer = global::go.go.printer_package;
using token = global::go.go.token_package;
using strings = strings_package;
using global::go.go;
using io = io_package;
using static global::go.go.printer_internal_test_package;

partial class printer_test_package {

internal static (ж<ast.FuncDecl> fun, ж<token.FileSet> fset) parseFunc(@string filename, @string functionname) {
    ж<ast.FuncDecl> fun = default!;
    ж<token.FileSet> fset = default!;

    fset = token.NewFileSet();
    {
        var (@file, err) = parser.ParseFile(fset, filename, default!, 0); if (err == default!) {
            foreach (var (_, d) in (~@file).Decls) {
                {
                    var (f, ok) = d._<ж<ast.FuncDecl>>(ᐧ); if (ok && (~(~f).Name).Name == functionname) {
                        fun = f;
                        return (fun, fset);
                    }
                }
            }
        }
    }
    throw panic("function not found");
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string exampleTestGoˢ = "example_test.go"u8;
internal static readonly @string printSelfˢ = "printSelf"u8;

internal static void printSelf() {
    // Parse source file and extract the AST without comments for
    // this function, with position information referring to the
    // file set fset.
    var (funcAST, fset) = parseFunc(exampleTestGoˢ, printSelfˢ);
    // Print the function body into buffer buf.
    // The file set is provided to the printer so that it knows
    // about the original source formatting and can add additional
    // line breaks where they were present in the source.
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    printer.Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, (~funcAST).Body.OrTypedNil());
    // Remove braces {} enclosing the function body, unindent,
    // and trim leading and trailing white space.
    @string s = Ꮡbuf.String();
    s = s.slice(1, len(s) - 1);
    s = strings.TrimSpace(strings.ReplaceAll(s, "\n\t"u8, "\n"u8));
    // Print the cleaned-up body text to stdout.
    fmt.Println(s);
}

public static void ExampleFprint() {
    printSelf();
}

// Output:
// funcAST, fset := parseFunc("example_test.go", "printSelf")
//
// var buf bytes.Buffer
// printer.Fprint(&buf, fset, funcAST.Body)
//
// s := buf.String()
// s = s[1 : len(s)-1]
// s = strings.TrimSpace(strings.ReplaceAll(s, "\n\t", "\n"))
//
// fmt.Println(s)

} // end printer_test_package
