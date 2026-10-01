// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using ast = global::go.go.ast_package;
using build = global::go.go.build_package;
using importer = global::go.go.importer_package;
using parser = global::go.go.parser_package;
using printer = global::go.go.printer_package;
using token = global::go.go.token_package;
using types = global::go.go.types_package;
using testenv = @internal.testenv_package;
using Δos = os_package;
using Δregexp = regexp_package;
using Δruntime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using @unsafe = unsafe_package;
using fs = global::go.io.fs_package;
using global::go.go;
using static global::go.runtime_internal_test_package;
using Δio = io_package;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string alignRuntimeTestGoˢ = "./align_runtime_test.go"u8;
internal static readonly @string unsafeOffsetofWWˢ = @"unsafe[.]Offsetof[(](\w+){}[.](\w+)[)]"u8;
internal static readonly @string unsafePointerWˢ = @"unsafe[.]Pointer[(]&(\w+)[)]"u8;
internal static readonly @string runtimeˢ = "runtime"u8;

// Check that 64-bit fields on which we apply atomic operations
// are aligned to 8 bytes. This can be a problem on 32-bit systems.
public static void TestAtomicAlignment(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoBuild(new runtime_test_package.testing_TжTB(Ꮡt)); // go command needed to resolve std .a files for importer.Default().
    // Read the code making the tables above, to see which fields and
    // variables we are currently checking.
    var @checked = new map<@string, bool>{};
    var (x, err) = Δos.ReadFile(alignRuntimeTestGoˢ);
    if (err != default!) {
        Ꮡt.Fatalf("read failed: %v"u8, err);
    }
    var fieldDesc = new map<nint, @string>{};
    var r = Δregexp.MustCompile(unsafeOffsetofWWˢ);
    var matches = r.FindAllStringSubmatch(((@string)x), -1);
    foreach (var (i, vΔ1) in matches) {
        @checked["field runtime." + vΔ1[1] + "." + vΔ1[2]] = true;
        fieldDesc[i] = vΔ1[1] + "." + vΔ1[2];
    }
    var varDesc = new map<nint, @string>{};
    r = Δregexp.MustCompile(unsafePointerWˢ);
    matches = r.FindAllStringSubmatch(((@string)x), -1);
    foreach (var (i, vΔ2) in matches) {
        @checked["var " + vΔ2[1]] = true;
        varDesc[i] = vΔ2[1];
    }
    // Check all of our alignments. This is the actual core of the test.
    foreach (var (i, d) in runtime_internal_test_package.AtomicFields) {
        if (d % 8 != 0) {
            Ꮡt.Errorf("field alignment of %s failed: offset is %d"u8, fieldDesc[i], d);
        }
    }
    foreach (var (i, p) in runtime_internal_test_package.AtomicVariables) {
        if ((uintptr)p % 8 != 0) {
            Ꮡt.Errorf("variable alignment of %s failed: address is %x"u8, varDesc[i], @unsafe.Pointer.OrTypedNil(p));
        }
    }
    // The code above is the actual test. The code below attempts to check
    // that the tables used by the code above are exhaustive.
    // Parse the whole runtime package, checking that arguments of
    // appropriate atomic operations are in the list above.
    var fset = token.NewFileSet();
    (var m, err) = parser.ParseDir(fset, "."u8, default!, 0);
    if (err != default!) {
        Ꮡt.Fatalf("parsing runtime failed: %v"u8, err);
    }
    var pkg = m[runtimeˢ]; // Note: ignore runtime_test and main packages
    // Filter files by those for the current architecture/os being tested.
    var fileMap = new map<@string, bool>{};
    foreach (var (_, f) in buildableFiles(Ꮡt, "."u8)) {
        fileMap[f] = true;
    }
    slice<ж<ast.File>> files = default!;
    foreach (var (fname, f) in (~pkg).Files) {
        if (fileMap[fname]) {
            files = append(files, f);
        }
    }
    // Call go/types to analyze the runtime package.
    ref var info = ref heap(new typesꓸInfo(), out var Ꮡinfo);
    info.Types = new map<ast.Expr, types.TypeAndValue>{};
    ref var conf = ref heap<types.Config>(out var Ꮡconf);
    conf = new types.Config(Importer: importer.Default());
    (_, err) = Ꮡconf.Check(runtimeˢ, fset, files, Ꮡinfo);
    if (err != default!) {
        Ꮡt.Fatalf("typechecking runtime failed: %v"u8, err);
    }
    // Analyze all atomic.*64 callsites.
    ref var v = ref heap<Visitor>(out var Ꮡv);
    v = new Visitor(t: Ꮡt, fset: fset, types: info.Types, @checked: @checked);
    ast.Walk(new runtime_test_package.VisitorжVisitor(Ꮡv), new ast.PackageжNode(pkg));
}

[GoType] partial struct Visitor {
    internal ж<token.FileSet> fset;
    internal map<ast.Expr, types.TypeAndValue> types;
    internal map<@string, bool> @checked;
    internal ж<testing.T> t;
}

public static ast.Visitor Visit(this ж<Visitor> Ꮡv, ast.Node n) {
    ref var v = ref Ꮡv.DerefOrNull();

    var (c, ok) = n._<ж<ast.CallExpr>>(ᐧ);
    if (!ok) {
        return new runtime_test_package.VisitorжVisitor(Ꮡv);
    }
    (var f, ok) = (~c).Fun._<ж<ast.SelectorExpr>>(ᐧ);
    if (!ok) {
        return new runtime_test_package.VisitorжVisitor(Ꮡv);
    }
    (var p, ok) = (~f).X._<ж<ast.Ident>>(ᐧ);
    if (!ok) {
        return new runtime_test_package.VisitorжVisitor(Ꮡv);
    }
    if ((~p).Name != "atomic"u8) {
        return new runtime_test_package.VisitorжVisitor(Ꮡv);
    }
    if (!strings.HasSuffix((~(~f).Sel).Name, "64"u8)) {
        return new runtime_test_package.VisitorжVisitor(Ꮡv);
    }
    var a = (~c).Args[0];
    // This is a call to atomic.XXX64(a, ...). Make sure a is aligned to 8 bytes.
    // XXX = one of Load, Store, Cas, etc.
    // The arg we care about the alignment of is always the first one.
    {
        var (u, okΔ1) = a._<ж<ast.UnaryExpr>>(ᐧ); if (okΔ1 && (~u).Op == token.AND) {
            v.checkAddr((~u).X);
            return new runtime_test_package.VisitorжVisitor(Ꮡv);
        }
    }
    // Other cases there's nothing we can check. Assume we're ok.
    v.t.Logf("unchecked atomic operation %s %v"u8, v.fset.Position(n.Pos()), v.print(n));
    return new runtime_test_package.VisitorжVisitor(Ꮡv);
}

// checkAddr checks to make sure n is a properly aligned address for a 64-bit atomic operation.
[GoRecv] internal static void checkAddr(this ref Visitor v, ast.Node n) {
    switch (n.type()) {
    case ж<ast.IndexExpr> nΔ1: {
        v.checkAddr((~nΔ1).X);
        return;
    }
    case ж<ast.Ident> nΔ1: {
        @string key = "var "u8 + v.print(new ast.IdentжNode(nΔ1));
        if (!v.@checked[key]) {
            // Alignment of an array element is the same as the whole array.
            v.t.Errorf("unchecked variable %s %s"u8, v.fset.Position(nΔ1.Pos()), key);
        }
        return;
    }
    case ж<ast.SelectorExpr> nΔ1: {
        var t = v.types[(~nΔ1).X].Type;
        if (t == default!) {
            // Not sure what is happening here, go/types fails to
            // type the selector arg on some platforms.
            return;
        }
        {
            var (p, ok) = t._<ж<types.Pointer>>(ᐧ); if (ok){
                // Note: we assume here that the pointer p in p.foo is properly
                // aligned. We just check that foo is at a properly aligned offset.
                t = p.Elem();
            } else {
                v.checkAddr((~nΔ1).X);
            }
        }
        if (AreEqual(t.Underlying(), t)) {
            v.t.Errorf("analysis can't handle unnamed type %s %v"u8, v.fset.Position(nΔ1.Pos()), t);
        }
        @string key = "field "u8 + t.String() + "."u8 + (~(~nΔ1).Sel).Name;
        if (!v.@checked[key]) {
            v.t.Errorf("unchecked field %s %s"u8, v.fset.Position(nΔ1.Pos()), key);
        }
        break;
    }
    default: {
        var nΔ1 = n;
        v.t.Errorf("unchecked atomic address %s %v"u8, v.fset.Position(nΔ1.Pos()), v.print(nΔ1));
        break;
    }}
}

[GoRecv] internal static @string print(this ref Visitor v, ast.Node n) {
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    printer.Fprint(new runtime_test_package.strings_BuilderжWriter(Ꮡb), v.fset, n);
    return b.String();
}

// buildableFiles returns the list of files in the given directory
// that are actually used for the build, given GOOS/GOARCH restrictions.
internal static slice<@string> buildableFiles(ж<testing.T> Ꮡt, @string dir) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var ctxt = ref heap<build.Context>(out var Ꮡctxt);
    ctxt = global::go.go.build_package.Default;
    ctxt.CgoEnabled = true;
    var (pkg, err) = Ꮡctxt.ImportDir(dir, 0);
    if (err != default!) {
        Ꮡt.Fatalf("can't find buildable files: %v"u8, err);
    }
    return (~pkg).GoFiles;
}

} // end runtime_test_package
