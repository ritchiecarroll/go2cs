// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package scannerhooks defines nonexported channels between parser and scanner.
// Ideally this package could be eliminated by adding API to scanner.
namespace go.go.@internal;

using token = global::go.go.token_package;
using global::go.go;

partial class scannerhooks_package {

public static readonly ж<Func<any, tokenꓸPos>> ᏑStringEnd = new StaticBox<Func<any, tokenꓸPos>>(default(Func<any, tokenꓸPos>));
public static ref Func<any, tokenꓸPos> StringEnd => ref ((StaticBox<Func<any, tokenꓸPos>>)ᏑStringEnd).ValueSlot;

} // end scannerhooks_package
