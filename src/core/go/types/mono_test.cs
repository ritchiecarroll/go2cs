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

internal static error checkMono(ж<testing.T> Ꮡt, @string body) {
    @string src = "package x; import `unsafe`; var _ unsafe.Pointer;\n"u8 + body;

    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    ref var conf = ref heap<types.Config>(out var Ꮡconf);
    conf = new types.Config(
        Error: (error err) => {
            fmt.Fprintln(new types_test_package.strings_BuilderжWriter(Ꮡbuf), err);
        },
        Importer: defaultImporter(fset) // TODO(adonovan): use same FileSet as typecheck
    );
    typecheck(src, Ꮡconf, nil);
    if (buf.Len() == 0) {
        return default!;
    }
    return errors.New(strings.TrimRight(buf.String(), "\n"u8));
}

public static void TestMonoGood(ж<testing.T> Ꮡt) {
    foreach (var (i, good) in goods) {
        {
            var err = checkMono(Ꮡt, good); if (err != default!) {
                Ꮡt.Errorf("%d: unexpected failure: %v"u8, i, err);
            }
        }
    }
}

public static void TestMonoBad(ж<testing.T> Ꮡt) {
    foreach (var (i, bad) in bads) {
        {
            var err = checkMono(Ꮡt, bad); if (err == default!){
                Ꮡt.Errorf("%d: unexpected success"u8, i);
            } else {
                Ꮡt.Log(err);
            }
        }
    }
}

internal static slice<@string> goods = new @string[]{
    "func F[T any](x T) { F(x) }"u8,
    "func F[T, U, V any]() { F[U, V, T](); F[V, T, U]() }"u8,
    "type Ring[A, B, C any] struct { L *Ring[B, C, A]; R *Ring[C, A, B] }"u8,
    "func F[T any]() { type U[T any] [unsafe.Sizeof(F[*T])]byte }"u8,
    "func F[T any]() { type U[T any] [unsafe.Sizeof(F[*T])]byte; var _ U[int] }"u8,
    "type U[T any] [unsafe.Sizeof(F[*T])]byte; func F[T any]() { var _ U[U[int]] }"u8,
    "func F[T any]() { type A = int; F[A]() }"u8
}.slice();

// TODO(mdempsky): Validate specific error messages and positioning.
internal static slice<@string> bads = new @string[]{
    "func F[T any](x T) { F(&x) }"u8,
    "func F[T any]() { F[*T]() }"u8,
    "func F[T any]() { F[[]T]() }"u8,
    "func F[T any]() { F[[1]T]() }"u8,
    "func F[T any]() { F[chan T]() }"u8,
    "func F[T any]() { F[map[*T]int]() }"u8,
    "func F[T any]() { F[map[error]T]() }"u8,
    "func F[T any]() { F[func(T)]() }"u8,
    "func F[T any]() { F[func() T]() }"u8,
    "func F[T any]() { F[struct{ t T }]() }"u8,
    "func F[T any]() { F[interface{ t() T }]() }"u8,
    "type U[_ any] int; func F[T any]() { F[U[T]]() }"u8,
    "func F[T any]() { type U int; F[U]() }"u8,
    "func F[T any]() { type U int; F[*U]() }"u8,
    "type U[T any] int; func (U[T]) m() { var _ U[*T] }"u8,
    "type U[T any] int; func (*U[T]) m() { var _ U[*T] }"u8,
    "type U[T1 any] [unsafe.Sizeof(F[*T1])]byte; func F[T2 any]() { var _ U[T2] }"u8,
    "func F[A, B, C, D, E any]() { F[B, C, D, E, *A]() }"u8,
    "type U[_ any] int; const X = unsafe.Sizeof(func() { type A[T any] U[A[*T]] })"u8,
    "func F[T any]() { type A = *T; F[A]() }"u8,
    "type A[T any] struct { _ A[*T] }"u8
}.slice();

} // end types_test_package
