// print_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// HAND-OWNED companion to print.go -- the two types the Go compiler prints specially.
//
// Go's print.go says it outright: "The compiler knows that a print of a value of this type should use
// printhex instead of printuint (decimal)" (hex) and "should use printquoted instead of printstring"
// (quoted). gc lowers print(x) to one printX call chosen by x's static type, so print(hex(v)) writes
// 0x<digits> and print(quoted(s)) writes the escaped, double-quoted form. go2cs binds every print to
// golib's builtin.print, which formats an argument by ToString and cannot name these runtime types, so
// both printed their underlying value (a decimal, a raw string): TestPrintQuoted failed on all of its
// subtests, and every print(hex(...)) in the runtime's diagnostics (the hexdump symbol lines, the
// throw texts of lock_spinbit.cs and plugin.cs) printed decimal.
//
// The dependency is inverted, as for the panic values (panicvalues_impl.cs): golib owns the marker
// interface IRuntimePrintable, and the two types implement it here with the CONVERTED printhex and
// printquoted, so the bytes are exactly the ones Go's printers write. builtin.print holds the runtime's
// printlock around the whole statement and printlock is recursive, so the nested lock is Go's own.
[module: go.GoManualConversion]

namespace go;

partial class runtime_package {

partial struct Δhex : IRuntimePrintable {
    void IRuntimePrintable.RuntimePrint() => printhex((uint64)this);
}

partial struct quoted : IRuntimePrintable {
    void IRuntimePrintable.RuntimePrint() => printquoted((@string)this);
}

// The two values as print arguments, for golib's tests: both types are internal to this package.
public static object GoQuotedPrintArg(@string s) => (quoted)s;

public static object GoHexPrintArg(uint64 v) => (Δhex)v;

} // end runtime_package
