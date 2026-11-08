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
private static ref readonly @string pNamePUnitˢ => ref pNamePUnitˢᵇ;
private static readonly @string pNamePUnitˢᵇ = "^(?P<name>/[^:]+):(?P<unit>[^:*/]+(?:[*/][^:*/]+)*)$"u8;

public static void TestNames(ж<testing.T> t) {
    // Note that this regexp is promised in the package docs for Description. Do not change.
    var r = regexp.MustCompile(pNamePUnitˢ);
    var all = metrics.All();
    foreach (var (i, d) in range(all)) {
        if (!r.MatchString(d.Name)) {
            t.Errorf("name %q does not match regexp %#q", d.Name, r.OrTypedNil());
        }
        if (i > 0 && all[i - 1].Name >= all[i].Name) {
            t.Fatalf("allDesc not sorted: %s ≥ %s", all[i - 1].Name, all[i].Name);
        }
    }

    var names = runtime_readMetricNames();
    slices.Sort<slice<@string>, @string>(names);
    var samples = new slice<metrics.Sample>(len(names));
    foreach (var (i, name) in range(names)) {
        samples[i].Name = name;
    }
    metrics.Read(samples);

    foreach (var (_, d) in range(all)) {
        while (len(samples) > 0 && samples[0].Name < d.Name) {
            t.Errorf("%s: reported by runtime but not listed in All", samples[0].Name);
            samples = samples[1..];
        }
        if (len(samples) == 0 || d.Name < samples[0].Name) {
            t.Errorf("%s: listed in All but not reported by runtime", d.Name);
            continue;
        }
        if (samples[0].Value.Kind() != d.Kind) {
            t.Errorf("%s: runtime reports %v but All reports %v", d.Name, samples[0].Value.Kind(), d.Kind);
        }
        samples = samples[1..];
    }
}

internal static @string wrap(@string prefix, @string text, nint width) {
    var doc = Ꮡ(new comment.Doc(Content: [new comment.ParagraphжBlock(Ꮡ(new comment.Paragraph(Text: [((comment.Plain)text)])))]));
    var pr = Ꮡ(new comment.Printer(TextPrefix: prefix, TextWidth: width));
    return (@string)pr.Text(doc);
}

internal static @string formatDesc(ж<testing.T> t) {
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    foreach (var (i, d) in range(metrics.All())) {
        if (i > 0) {
            fmt.Fprintf(new strings_BuilderжWriter(Ꮡb), "\n"u8);
        }
        fmt.Fprintf(new strings_BuilderжWriter(Ꮡb), "%s\n"u8, d.Name);
        fmt.Fprintf(new strings_BuilderжWriter(Ꮡb), "%s"u8, wrap("\t", d.ΔDescription, 80 - 2 * 8));
    }
    return b.String();
}

internal static ж<bool> generate = flag.Bool("generate", false, "update doc.go for go generate");

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string docGoˢ => ref docGoˢᵇ;
private static readonly @string docGoˢᵇ = "doc.go"u8;
private static readonly object noDocCommentInDocGoˢ = (@string)"no doc comment in doc.go"u8;
private static ref readonly @string runtimeMetricsˢ => ref runtimeMetricsˢᵇ;
private static readonly @string runtimeMetricsˢᵇ = "runtime/metrics"u8;
private static readonly object docNewFromFilesLostDocˢ = (@string)"doc.NewFromFiles lost doc comment"u8;
private static ref readonly @string supportedMetricsˢ => ref supportedMetricsˢᵇ;
private static readonly @string supportedMetricsˢᵇ = "Supported metrics"u8;
private static ref readonly @string oldˢ => ref oldˢᵇ;
private static readonly @string oldˢᵇ = "old"u8;
private static ref readonly @string wantˢ => ref wantˢᵇ;
private static readonly @string wantˢᵇ = "want"u8;

public static void TestDocs(ж<testing.T> t) {
    @string want = formatDesc(t);

    var (src, err) = os.ReadFile(docGoˢ);
    if (err != nil) {
        t.Fatal(err);
    }
    var fset = token.NewFileSet();
    (var f, err) = parser.ParseFile(fset, docGoˢ, src, parser.ParseComments);
    if (err != nil) {
        t.Fatal(err);
    }
    var fdoc = f.Doc;
    if (fdoc == nil) {
        t.Fatal(noDocCommentInDocGoˢ);
    }
    (var pkg, err) = doc.NewFromFiles(fset, [f], runtimeMetricsˢ);
    if (err != nil) {
        t.Fatal(err);
    }
    if (pkg.Doc == ""u8) {
        t.Fatal(docNewFromFilesLostDocˢ);
    }
    var docΔ1 = @new<comment.Parser>().Parse(pkg.Doc);
    var expectCode = false;
    var foundCode = false;
    var updated = false;
    foreach (var (_, block) in range(docΔ1.Content)) {
        switch (block.type()) {
        case ж<comment.Heading> b: {
            expectCode = false;
            if (AreEqual(b.Text[0], ((comment.Plain)(@string)supportedMetricsˢ))) {
                expectCode = true;
            }
            break;
        }
        case ж<comment.Code> b: {
            if (expectCode) {
                foundCode = true;
                if (b.Text != want) {
                    if (!generate.Value) {
                        t.Fatalf("doc comment out of date; use go generate to rebuild\n%s", diff.Diff(oldˢ, slice<byte>(b.Text), wantˢ, slice<byte>(want)));
                    }
                    b.Text = want;
                    updated = true;
                }
            }
            break;
        }}
    }

    if (!foundCode) {
        t.Fatalf("did not find Supported metrics list in doc.go");
    }
    if (updated) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "go test -generate: writing new doc.go\n"u8);
        bytes.Buffer buf = default;
        buf.Write(src.slice(0, fdoc.Pos() - f.FileStart));
        buf.WriteString("/*\n");
        buf.Write(@new<comment.Printer>().Comment(docΔ1));
        buf.WriteString("*/");
        buf.Write(src.slice(fdoc.End() - f.FileStart));
        var (srcΔ1, errΔ1) = format.Source(buf.Bytes());
        if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
        var errΔ2 = os.WriteFile(docGoˢ, srcΔ1, 438); if (errΔ2 != nil) {
            t.Fatal(errΔ2);
        }
    } else if (generate.Value) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "go test -generate: doc.go already up-to-date\n"u8);
    }
}

} // end metrics_test_package
