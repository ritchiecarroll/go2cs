// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using errors = errors_package;
using fmt = fmt_package;
using types = global::go.go.types_package;
using strings = strings_package;
using testing = testing_package;
using global::go.go;
using io = io_package;
using static global::go.go.types_internal_test_package;
using token = global::go.go.token_package;

partial class types_test_package {

internal static error checkMono(ж<testing.T> t, @string body) {
    @string src = "package x; import `unsafe`; var _ unsafe.Pointer;\n"u8 + body;

    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    ref var conf = ref heap<types.Config>(new types.Config(
        Error: (error err) => {
            fmt.Fprintln(new types_test_package.strings_BuilderжWriter(Ꮡbuf), err);
        },
        Importer: defaultImporter(fset) // TODO(adonovan): use same FileSet as typecheck
    ), out var Ꮡconf);
    typecheck(src, Ꮡconf, nil);
    if (buf.Len() == 0) {
        return default;
    }
    return errors.New(strings.TrimRight(buf.String(), "\n"));
}

public static void TestMonoGood(ж<testing.T> t) {
    foreach (var (i, good) in range(goods)) {
        var err = checkMono(t, good); if (err != nil) {
            t.Errorf("%d: unexpected failure: %v", i, err);
        }
    }
}

public static void TestMonoBad(ж<testing.T> t) {
    foreach (var (i, bad) in range(bads)) {
        var err = checkMono(t, bad); if (err == nil) {
            t.Errorf("%d: unexpected success", i);
        } else {
            t.Log(err);
        }
    }
}

internal static slice<@string> goods = [
    "func F[T any](x T) { F(x) }",
    "func F[T, U, V any]() { F[U, V, T](); F[V, T, U]() }",
    "type Ring[A, B, C any] struct { L *Ring[B, C, A]; R *Ring[C, A, B] }",
    "func F[T any]() { type U[T any] [unsafe.Sizeof(F[*T])]byte }",
    "func F[T any]() { type U[T any] [unsafe.Sizeof(F[*T])]byte; var _ U[int] }",
    "type U[T any] [unsafe.Sizeof(F[*T])]byte; func F[T any]() { var _ U[U[int]] }",
    "func F[T any]() { type A = int; F[A]() }"
];

// TODO(mdempsky): Validate specific error messages and positioning.
internal static slice<@string> bads = [
    "func F[T any](x T) { F(&x) }",
    "func F[T any]() { F[*T]() }",
    "func F[T any]() { F[[]T]() }",
    "func F[T any]() { F[[1]T]() }",
    "func F[T any]() { F[chan T]() }",
    "func F[T any]() { F[map[*T]int]() }",
    "func F[T any]() { F[map[error]T]() }",
    "func F[T any]() { F[func(T)]() }",
    "func F[T any]() { F[func() T]() }",
    "func F[T any]() { F[struct{ t T }]() }",
    "func F[T any]() { F[interface{ t() T }]() }",
    "type U[_ any] int; func F[T any]() { F[U[T]]() }",
    "func F[T any]() { type U int; F[U]() }",
    "func F[T any]() { type U int; F[*U]() }",
    "type U[T any] int; func (U[T]) m() { var _ U[*T] }",
    "type U[T any] int; func (*U[T]) m() { var _ U[*T] }",
    "type U[T1 any] [unsafe.Sizeof(F[*T1])]byte; func F[T2 any]() { var _ U[T2] }",
    "func F[A, B, C, D, E any]() { F[B, C, D, E, *A]() }",
    "type U[_ any] int; const X = unsafe.Sizeof(func() { type A[T any] U[A[*T]] })",
    "func F[T any]() { type A = *T; F[A]() }",
    "type A[T any] struct { _ A[*T] }"
];

} // end types_test_package
