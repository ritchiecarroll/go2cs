// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using token = global::go.go.token_package;
using os = os_package;
using testing = testing_package;
using ast = global::go.go.ast_package;
using global::go.go;
using static global::go.go.parser_package;

partial class parser_internal_test_package {

internal static slice<byte> src = readFile("../printer/nodes.go");

internal static slice<byte> readFile(@string filename) {
    var (data, err) = os.ReadFile(filename);
    if (err != nil) {
        throw panic(err);
    }
    return data;
}

public static void BenchmarkParse(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.SetBytes((int64)len(src));
    for (nint i = 0; i < b.N; i++) {
        var (_, err) = ParseFile(token.NewFileSet(), ""u8, src, ParseComments); if (err != nil) {
            Ꮡb.Fatalf("benchmark failed due to parse error: %s", err);
        }
    }
}

public static void BenchmarkParseOnly(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.SetBytes((int64)len(src));
    for (nint i = 0; i < b.N; i++) {
        var (_, err) = ParseFile(token.NewFileSet(), ""u8, src, (global::go.go.parser_package.Mode)(ParseComments | SkipObjectResolution)); if (err != nil) {
            Ꮡb.Fatalf("benchmark failed due to parse error: %s", err);
        }
    }
}

public static void BenchmarkResolve(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.SetBytes((int64)len(src));
    for (nint i = 0; i < b.N; i++) {
        b.StopTimer();
        var fset = token.NewFileSet();
        var (@file, err) = ParseFile(fset, ""u8, src, SkipObjectResolution);
        if (err != nil) {
            Ꮡb.Fatalf("benchmark failed due to parse error: %s", err);
        }
        b.StartTimer();
        var handle = fset.File(@file.Package);
        resolveFile(ref (@file).DerefOrNull(), handle, default);
    }
}

} // end parser_internal_test_package
