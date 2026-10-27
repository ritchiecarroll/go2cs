// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package internal contains definitions used by both log and log/slog.
namespace go.log;

partial class internal_package {

// DefaultOutput holds a function which calls the default log.Logger's
// output function.
// It allows slog.defaultHandler to call into an unexported function of
// the log package.
public static readonly ж<Func<uintptr, slice<byte>, error>> ᏑDefaultOutput = new StaticBox<Func<uintptr, slice<byte>, error>>(default(Func<uintptr, slice<byte>, error>));
public static ref Func<uintptr, slice<byte>, error> DefaultOutput => ref ((StaticBox<Func<uintptr, slice<byte>, error>>)ᏑDefaultOutput).ValueSlot;

} // end internal_package
