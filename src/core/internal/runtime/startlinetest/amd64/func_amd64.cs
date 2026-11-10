// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package startlinetest contains helpers for runtime_test.TestStartLineAsm.
namespace go.@internal.runtime;

partial class startlinetest_package {

// Defined in func_amd64.s, this is a trivial assembly function that calls
// runtime_test.callerStartLine.
public static partial nint AsmFunc();

// Provided by runtime_test.
public static readonly ж<Func<bool, nint>> ᏑCallerStartLine = new StaticBox<Func<bool, nint>>(default(Func<bool, nint>));
public static ref Func<bool, nint> CallerStartLine => ref ((StaticBox<Func<bool, nint>>)ᏑCallerStartLine).ValueSlot;

} // end startlinetest_package
