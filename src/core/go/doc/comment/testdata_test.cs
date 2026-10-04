// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.doc;

using bytes = bytes_package;
using json = encoding.json_package;
using fmt = fmt_package;
using diff = @internal.diff_package;
using txtar = @internal.txtar_package;
using filepath = path.filepath_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using encoding;
using io = io_package;
using path;
using static global::go.go.doc.comment_package;

partial class comment_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataTxtˢ = "testdata/*.txt"u8;
internal static readonly @string goDocCommentˢ = "go/doc/comment"u8;
internal static readonly object inputˢ = (@string)"input"u8;
internal static readonly @string haveˢ = "have"u8;

public static void TestTestdata(ж<testing.T> Ꮡt) {
    var (files, _) = filepath.Glob(testdataTxtˢ);
    if (len(files) == 0) {
        Ꮡt.Fatalf("no testdata"u8);
    }
    ref var p = ref heap(new global::go.go.doc.comment_package.Parser(), out var Ꮡp);
    p.Words = new map<@string, @string>{
        ["italicword"u8] = ""u8,
        ["linkedword"u8] = "https://example.com/linkedword"u8
    };
    p.LookupPackage = (@string importPath, bool ok) (@string name) => {
        if (name == "comment"u8) {
            return (goDocCommentˢ, true);
        }
        return DefaultLookupPackage(name);
    };
    p.LookupSym = (@string recv, @string name) => {
        if (recv == "Parser"u8 && name == "Parse"u8 || recv == ""u8 && name == "Doc"u8 || recv == ""u8 && name == "NoURL"u8) {
            return true;
        }
        return false;
    };
    slice<byte> stripDollars(slice<byte> b) {
        // Remove trailing $ on lines.
        // They make it easier to see lines with trailing spaces,
        // as well as turning them into lines without trailing spaces,
        // in case editors remove trailing spaces.
        return bytes.ReplaceAll(b, slice<byte>("$\n"u8), slice<byte>("\n"u8));
    }
    foreach (var (_, @file) in files) {
        var stripDollarsʗ1 = stripDollars;
        Ꮡt.Run(filepath.Base(@file), (ж<testing.T> tΔ1) => {
            ref var pr = ref heap(new global::go.go.doc.comment_package.Printer(), out var Ꮡpr);
            var (a, err) = txtar.ParseFile(@file);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            if (len((~a).Comment) > 0) {
                var errΔ1 = json.Unmarshal((~a).Comment, Ꮡpr);
                if (errΔ1 != default!) {
                    tΔ1.Fatalf("unmarshaling top json: %v"u8, errΔ1);
                }
            }
            if (len((~a).Files) < 1 || (~a).Files[0].Name != "input"u8) {
                tΔ1.Fatalf("first file is not %q"u8, inputˢ);
            }
            var d = Ꮡp.Parse(((@string)stripDollarsʗ1((~a).Files[0].Data)));
            foreach (var (_, f) in (~a).Files[1..]) {
                var want = stripDollarsʗ1(f.Data);
                while (len(want) >= 2 && want[len(want) - 1] == (rune)'\n' && want[len(want) - 2] == (rune)'\n') {
                    want = want.slice(0, len(want) - 1);
                }
                slice<byte> @out = default!;
                var exprᴛ1 = f.Name;
                if (exprᴛ1 == "dump"u8) {
                    @out = dump(d);
                }
                else if (exprᴛ1 == "gofmt"u8) {
                    @out = Ꮡpr.Comment(d);
                }
                else if (exprᴛ1 == "html"u8) {
                    @out = Ꮡpr.HTML(d);
                }
                else if (exprᴛ1 == "markdown"u8) {
                    @out = Ꮡpr.Markdown(d);
                }
                else if (exprᴛ1 == "text"u8) {
                    @out = Ꮡpr.Text(d);
                }
                else { /* default: */
                    tΔ1.Fatalf("unknown output file %q"u8, f.Name);
                }

                if (((sstring)@out) != ((sstring)want)) {
                    tΔ1.Errorf("%s: %s"u8, @file, diff.Diff(f.Name, want, haveˢ, @out));
                }
            }
        });
    }
}

internal static slice<byte> dump(ж<global::go.go.doc.comment_package.Doc> Ꮡd) {
    ref var @out = ref heap(new bytes.Buffer(), out var Ꮡout);
    dumpTo(Ꮡout, 0, Ꮡd.OrTypedNil());
    return @out.Bytes();
}

internal static void dumpTo(ж<bytes.Buffer> Ꮡout, nint indent, any x) {
    switch (x.type()) {
    default: {
        var xΔ1 = x;
        fmt.Fprintf(new comment_internal_test_package.bytes_BufferжWriter(Ꮡout), "?%T"u8, xΔ1);
        break;
    }
    case ж<global::go.go.doc.comment_package.Doc> xΔ1: {
        fmt.Fprintf(new comment_internal_test_package.bytes_BufferжWriter(Ꮡout), "Doc"u8);
        dumpTo(Ꮡout, indent + 1, (~xΔ1).Content);
        if (len((~xΔ1).Links) > 0) {
            dumpNL(Ꮡout, indent + 1);
            fmt.Fprintf(new comment_internal_test_package.bytes_BufferжWriter(Ꮡout), "Links"u8);
            dumpTo(Ꮡout, indent + 2, (~xΔ1).Links);
        }
        fmt.Fprintf(new comment_internal_test_package.bytes_BufferжWriter(Ꮡout), "\n"u8);
        break;
    }
    case slice<ж<global::go.go.doc.comment_package.LinkDef>> xΔ1: {
        foreach (var (_, def) in xΔ1) {
            dumpNL(Ꮡout, indent);
            dumpTo(Ꮡout, indent, def.OrTypedNil());
        }
        break;
    }
    case ж<global::go.go.doc.comment_package.LinkDef> xΔ1: {
        fmt.Fprintf(new comment_internal_test_package.bytes_BufferжWriter(Ꮡout), "LinkDef Used:%v Text:%q URL:%s"u8, (~xΔ1).Used, (~xΔ1).Text, (~xΔ1).URL);
        break;
    }
    case slice<global::go.go.doc.comment_package.Block> xΔ1: {
        foreach (var (_, blk) in xΔ1) {
            dumpNL(Ꮡout, indent);
            dumpTo(Ꮡout, indent, blk);
        }
        break;
    }
    case ж<global::go.go.doc.comment_package.Heading> xΔ1: {
        fmt.Fprintf(new comment_internal_test_package.bytes_BufferжWriter(Ꮡout), "Heading"u8);
        dumpTo(Ꮡout, indent + 1, (~xΔ1).Text);
        break;
    }
    case ж<global::go.go.doc.comment_package.List> xΔ1: {
        fmt.Fprintf(new comment_internal_test_package.bytes_BufferжWriter(Ꮡout), "List ForceBlankBefore=%v ForceBlankBetween=%v"u8, (~xΔ1).ForceBlankBefore, (~xΔ1).ForceBlankBetween);
        dumpTo(Ꮡout, indent + 1, (~xΔ1).Items);
        break;
    }
    case slice<ж<global::go.go.doc.comment_package.ListItem>> xΔ1: {
        foreach (var (_, item) in xΔ1) {
            dumpNL(Ꮡout, indent);
            dumpTo(Ꮡout, indent, item.OrTypedNil());
        }
        break;
    }
    case ж<global::go.go.doc.comment_package.ListItem> xΔ1: {
        fmt.Fprintf(new comment_internal_test_package.bytes_BufferжWriter(Ꮡout), "Item Number=%q"u8, (~xΔ1).Number);
        dumpTo(Ꮡout, indent + 1, (~xΔ1).Content);
        break;
    }
    case ж<global::go.go.doc.comment_package.Paragraph> xΔ1: {
        fmt.Fprintf(new comment_internal_test_package.bytes_BufferжWriter(Ꮡout), "Paragraph"u8);
        dumpTo(Ꮡout, indent + 1, (~xΔ1).Text);
        break;
    }
    case ж<global::go.go.doc.comment_package.Code> xΔ1: {
        fmt.Fprintf(new comment_internal_test_package.bytes_BufferжWriter(Ꮡout), "Code"u8);
        dumpTo(Ꮡout, indent + 1, (~xΔ1).Text);
        break;
    }
    case slice<global::go.go.doc.comment_package.ΔText> xΔ1: {
        foreach (var (_, t) in xΔ1) {
            dumpNL(Ꮡout, indent);
            dumpTo(Ꮡout, indent, t);
        }
        break;
    }
    case Plain xΔ1: {
        if (!strings.Contains(((@string)xΔ1), "\n"u8)){
            fmt.Fprintf(new comment_internal_test_package.bytes_BufferжWriter(Ꮡout), "Plain %q"u8, ((@string)xΔ1));
        } else {
            fmt.Fprintf(new comment_internal_test_package.bytes_BufferжWriter(Ꮡout), "Plain"u8);
            dumpTo(Ꮡout, indent + 1, ((@string)xΔ1));
        }
        break;
    }
    case Italic xΔ1: {
        if (!strings.Contains(((@string)xΔ1), "\n"u8)){
            fmt.Fprintf(new comment_internal_test_package.bytes_BufferжWriter(Ꮡout), "Italic %q"u8, ((@string)xΔ1));
        } else {
            fmt.Fprintf(new comment_internal_test_package.bytes_BufferжWriter(Ꮡout), "Italic"u8);
            dumpTo(Ꮡout, indent + 1, ((@string)xΔ1));
        }
        break;
    }
    case @string xΔ1: {
        foreach (var (_, line) in strings.SplitAfter(xΔ1, "\n"u8)) {
            if (line != ""u8) {
                dumpNL(Ꮡout, indent);
                fmt.Fprintf(new comment_internal_test_package.bytes_BufferжWriter(Ꮡout), "%q"u8, line);
            }
        }
        break;
    }
    case ж<global::go.go.doc.comment_package.Link> xΔ1: {
        fmt.Fprintf(new comment_internal_test_package.bytes_BufferжWriter(Ꮡout), "Link %q"u8, (~xΔ1).URL);
        dumpTo(Ꮡout, indent + 1, (~xΔ1).Text);
        break;
    }
    case ж<global::go.go.doc.comment_package.DocLink> xΔ1: {
        fmt.Fprintf(new comment_internal_test_package.bytes_BufferжWriter(Ꮡout), "DocLink pkg:%q, recv:%q, name:%q"u8, (~xΔ1).ImportPath, (~xΔ1).Recv, (~xΔ1).Name);
        dumpTo(Ꮡout, indent + 1, (~xΔ1).Text);
        break;
    }}
}

internal static void dumpNL(ж<bytes.Buffer> Ꮡout, nint n) {
    ref var @out = ref Ꮡout.DerefOrNull();

    @out.WriteByte((rune)'\n');
    for (nint i = 0; i < n; i++) {
        @out.WriteByte((rune)'\t');
    }
}

} // end comment_internal_test_package
