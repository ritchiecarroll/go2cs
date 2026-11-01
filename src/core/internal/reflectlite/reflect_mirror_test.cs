// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using fmt = fmt_package;
using ast = global::go.go.ast_package;
using parser = global::go.go.parser_package;
using token = global::go.go.token_package;
using fs = global::go.io.fs_package;
using os = os_package;
using filepath = path.filepath_package;
using Δruntime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using sync = global::go.sync_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using global::go;
using global::go.go;
using global::go.io;
using path;
using static global::go.@internal.reflectlite_internal_test_package;

partial class reflectlite_test_package {

internal static slice<@string> typeNames = slice<@string>([
    "uncommonType",
    "arrayType",
    "chanType",
    "funcType",
    "interfaceType",
    "ptrType",
    "sliceType",
    "structType"
]);

partial struct visitor {
    internal map<@string, map<@string, bool>> m;
}

internal static visitor newVisitor() {
    var v = new visitor();
    v.m = new map<@string, map<@string, bool>>();

    return v;
}

internal static bool filter(this visitor v, @string name) {
    return slices.Contains(typeNames, name);
}

internal static ast.Visitor Visit(this visitor v, ast.Node n) {
    switch (n.type()) {
    case ж<ast.TypeSpec> x: {
        if (v.filter(x.Name.String())) {
            {
                var (st, ok) = x.Type._<ж<ast.StructType>>(ᐧ); if (ok) {
                    v.m[x.Name.String()] = new map<@string, bool>();
                    foreach (var (_, field) in st.Fields.List) {
                        @string k = fmt.Sprintf("%s"u8, field.Type);
                        if (len(field.Names) > 0) {
                            k = field.Names[0].Name;
                        }
                        v.m[x.Name.String()].Set(k, true);
                    }
                }
            }
        }
        break;
    }}
    return v;
}

internal static void loadTypes(@string path, @string pkgName, visitor v) {
    var fset = token.NewFileSet();

    var filter = (fs.FileInfo fi) => strings.HasSuffix(fi.Name(), ".go");
    var (pkgs, err) = parser.ParseDir(fset, path, filter, 0);
    if (err != nil) {
        throw panic(err);
    }

    var pkg = pkgs[pkgName];

    foreach (var (_, f) in pkg.Files) {
        ast.Walk(v, new ast.FileжNode(f));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string srcˢ => ref srcˢᵇ;
private static readonly @string srcˢᵇ = "src"u8;
internal static ref readonly @string reflectˢ => ref reflectˢᵇ;
private static readonly @string reflectˢᵇ = "reflect"u8;

internal partial struct TestMirrorWithReflect_type /*dyn*/ {
    internal @string path, pkg;
    internal visitor v;
}

public static partial void TestMirrorWithReflect(ж<testing.T> t) {
    // TODO when the dust clears, figure out what this should actually test.
    t.Skipf("reflect and reflectlite are out of sync for now");
    @string reflectDir = filepath.Join(Δruntime.GOROOT(), srcˢ, reflectˢ);
    {
        var (_, err) = os.Stat(reflectDir); if (os.IsNotExist(err)) {
            // On some mobile builders, the test binary executes on a machine without a
            // complete GOROOT source tree.
            t.Skipf("GOROOT source not present");
        }
    }

    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    var (rl, r) = (newVisitor(), newVisitor());

    foreach (var (_, vᴛ1) in slice<TestMirrorWithReflect_type>([
        new(".", "reflectlite", rl),
        new(reflectDir, "reflect", r)
    ])) {
        ref var tc = ref heap(new TestMirrorWithReflect_type(), out var Ꮡtc);
        tc = vᴛ1;

        Ꮡwg.Add(1);
        var tcʗ1 = tc;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                loadTypes(tcʗ1.path, tcʗ1.pkg, tcʗ1.v);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();

    if (len(rl.m) != len(r.m)) {
        t.Fatalf("number of types mismatch, reflect: %d, reflectlite: %d (%+v, %+v)", len(r.m), len(rl.m), r.m, rl.m);
    }

    foreach (var (typName, _) in r.m) {
        if (len(r.m[typName]) != len(rl.m[typName])) {
            t.Errorf("type %s number of fields mismatch, reflect: %d, reflectlite: %d", typName, len(r.m[typName]), len(rl.m[typName]));
            continue;
        }
        foreach (var (field, _) in r.m[typName]) {
            {
                var (_, ok) = rl.m[typName][field, ꟷ]; if (!ok) {
                    t.Errorf(@"Field mismatch, reflect have ""%s"", relectlite does not.", field);
                }
            }
        }
    }
}

} // end reflectlite_test_package
