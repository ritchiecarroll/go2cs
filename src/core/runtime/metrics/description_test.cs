// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using bytes = bytes_package;
using flag = flag_package;
using fmt = fmt_package;
using ast = global::go.go.ast_package;
using doc = global::go.go.doc_package;
using comment = global::go.go.doc.comment_package;
using format = global::go.go.format_package;
using parser = global::go.go.parser_package;
using token = global::go.go.token_package;
using diff = @internal.diff_package;
using os = os_package;
using regexp = regexp_package;
using metrics = global::go.runtime.metrics_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using @internal;
using fs = io.fs_package;
using global::go.go;
using global::go.go.doc;
using global::go.runtime;
using io = io_package;

partial class metrics_test_package {

// Implemented in the runtime.
//
//go:linkname runtime_readMetricNames
/*linkname*/ internal static partial slice<@string> runtime_readMetricNames() {
    return global::go.runtime_package.readMetricNames();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string pNamePUnitˢ = "^(?P<name>/[^:]+):(?P<unit>[^:*/]+(?:[*/][^:*/]+)*)$"u8;

public static void TestNames(ж<testing.T> Ꮡt) {
    // Note that this regexp is promised in the package docs for Description. Do not change.
    var r = regexp.MustCompile(pNamePUnitˢ);
    var all = metrics.All();
    foreach (var (i, d) in all) {
        if (!r.MatchString(d.Name)) {
            Ꮡt.Errorf("name %q does not match regexp %#q"u8, d.Name, r.OrTypedNil());
        }
        if (i > 0 && all[i - 1].Name >= all[i].Name) {
            Ꮡt.Fatalf("allDesc not sorted: %s ≥ %s"u8, all[i - 1].Name, all[i].Name);
        }
    }
    var names = runtime_readMetricNames();
    slices.Sort<slice<@string>, @string>(names);
    var samples = new slice<metrics.Sample>(len(names));
    foreach (var (i, name) in names) {
        samples[i].Name = name;
    }
    metrics.Read(samples);
    foreach (var (_, d) in all) {
        while (len(samples) > 0 && samples[0].Name < d.Name) {
            Ꮡt.Errorf("%s: reported by runtime but not listed in All"u8, samples[0].Name);
            samples = samples[1..];
        }
        if (len(samples) == 0 || d.Name < samples[0].Name) {
            Ꮡt.Errorf("%s: listed in All but not reported by runtime"u8, d.Name);
            continue;
        }
        if (samples[0].Value.Kind() != d.Kind) {
            Ꮡt.Errorf("%s: runtime reports %v but All reports %v"u8, d.Name, samples[0].Value.Kind(), d.Kind);
        }
        samples = samples[1..];
    }
}

internal static @string wrap(@string prefix, @string text, nint width) {
    var doc = Ꮡ(new comment.Doc(Content: new comment.Block[]{new comment.ParagraphжBlock(Ꮡ(new comment.Paragraph(Text: new commentꓸText[]{((comment.Plain)text)}.slice())))}.slice()));
    var pr = Ꮡ(new comment.Printer(TextPrefix: prefix, TextWidth: width));
    return ((@string)pr.Text(doc));
}

internal static @string formatDesc(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    foreach (var (i, d) in metrics.All()) {
        if (i > 0) {
            fmt.Fprintf(new strings_BuilderжWriter(Ꮡb), "\n"u8);
        }
        fmt.Fprintf(new strings_BuilderжWriter(Ꮡb), "%s\n"u8, d.Name);
        fmt.Fprintf(new strings_BuilderжWriter(Ꮡb), "%s"u8, wrap("\t"u8, d.ΔDescription, 80 - 2 * 8));
    }
    return b.String();
}

internal static ж<bool> generate = flag.Bool("generate"u8, false, "update doc.go for go generate"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string docGoˢ = "doc.go"u8;
private static readonly object noDocCommentInDocGoˢ = (@string)"no doc comment in doc.go"u8;
private static readonly @string runtimeMetricsˢ = "runtime/metrics"u8;
private static readonly object docNewFromFilesLostDocˢ = (@string)"doc.NewFromFiles lost doc comment"u8;
private static readonly @string supportedMetricsˢ = "Supported metrics"u8;
private static readonly @string oldˢ = "old"u8;
private static readonly @string wantˢ = "want"u8;

public static void TestDocs(ж<testing.T> Ꮡt) {
    @string want = formatDesc(Ꮡt);
    var (src, err) = os.ReadFile(docGoˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var fset = token.NewFileSet();
    (var f, err) = parser.ParseFile(fset, docGoˢ, src, parser.ParseComments);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var fdoc = f.Value.Doc;
    if (fdoc == nil) {
        Ꮡt.Fatal(noDocCommentInDocGoˢ);
    }
    (var pkg, err) = doc.NewFromFiles(fset, new ж<ast.File>[]{f}.slice(), runtimeMetricsˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if ((~pkg).Doc == ""u8) {
        Ꮡt.Fatal(docNewFromFilesLostDocˢ);
    }
    var docΔ1 = @new<comment.Parser>().Parse((~pkg).Doc);
    var expectCode = false;
    var foundCode = false;
    var updated = false;
    foreach (var (_, block) in (~docΔ1).Content) {
        switch (block.type()) {
        case ж<comment.Heading> b: {
            expectCode = false;
            if (AreEqual((~b).Text[0], ((comment.Plain)(@string)supportedMetricsˢ))) {
                expectCode = true;
            }
            break;
        }
        case ж<comment.Code> b: {
            if (expectCode) {
                foundCode = true;
                if ((~b).Text != want) {
                    if (!generate.Value) {
                        Ꮡt.Fatalf("doc comment out of date; use go generate to rebuild\n%s"u8, diff.Diff(oldˢ, slice<byte>((~b).Text), wantˢ, slice<byte>(want)));
                    }
                    b.Value.Text = want;
                    updated = true;
                }
            }
            break;
        }}
    }
    if (!foundCode) {
        Ꮡt.Fatalf("did not find Supported metrics list in doc.go"u8);
    }
    if (updated){
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "go test -generate: writing new doc.go\n"u8);
        bytes.Buffer buf = default!;
        buf.Write(src.slice(0, fdoc.Pos() - (~f).FileStart));
        buf.WriteString("/*\n"u8);
        buf.Write(@new<comment.Printer>().Comment(docΔ1));
        buf.WriteString("*/"u8);
        buf.Write(src.slice(fdoc.End() - (~f).FileStart));
        var (srcΔ1, errΔ1) = format.Source(buf.Bytes());
        if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
        {
            var errΔ2 = os.WriteFile(docGoˢ, srcΔ1, 438); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
    } else 
    if (generate.Value) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "go test -generate: doc.go already up-to-date\n"u8);
    }
}

} // end metrics_test_package
