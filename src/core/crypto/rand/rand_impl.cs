// rand_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written body for crypto/rand's runtime fatal bridge. rand.cs:58 carries Go's own comment,
// "fatal is [runtime.fatal], pushed via linkname", and :60 the bodyless partial; the push does not
// arrive in this corpus, so PartialStubGenerator fills it with a throwing NotImplementedException.
//
// REACH, measured at this tree: ONE caller, rand.cs:85 — the read failure path, reporting
// "crypto/rand: failed to read random data (see https://go.dev/issue/66821)". Go follows that call
// with `panic("unreachable") // To be sure.`, which is unreachable BECAUSE fatal exits. With the
// stub in place the call did not exit: it threw, and the panic below it was the second thing the
// caller could have seen. For the one package whose failure mode must not be negotiable, that is
// the wrong shape of failure, whatever the text says.

// WHY A COMPANION AND NOT A //go:linkname REGISTRY ROW. The converter CAN forward a push into the
// consumer package, and five of RED 7's members are being wired that way (crypto/internal/fips140's
// three, crypto/internal/sysrand's fatal and crypto/internal/fips140hash's sha3Unwrap — G's seat).
// A row is admissible only where the consumer's project ALREADY references the pushing package.
// crypto.rand.csproj carries NO reference to runtime, so this member takes the companion, not the row.
// COORD's split at 1fa7940a0, on G's reading of the project files at 5bb307d57e.
//
// WHY golib HOLDS THE PRIMITIVE, and not `runtime`. golib is the only assembly BELOW every converted
// package. FatalReport.cs argues this for itself and names this very set of consumers — it was
// written for the fatal shims already known, and these are the members the 1.24 hop added to the
// same class. One primitive there, a one-line forward here.
//
// WHAT THE FORWARD PRESERVES. Go's fatal is UNRECOVERABLE from its first instruction: no recover(),
// no deferred function, no catch. FatalReport.Fatal writes `fatal error: <text>`, a blank line and a
// Go-spelled traceback, then calls Environment.Exit(2), so the unrecoverability is structural rather
// than a property of the exception type — which is exactly what a throwing stub does NOT give: a
// NotImplementedException is an ordinary managed exception any frame above can catch.
//
// userFault is Go's throwType axis, read from Go's OWN push at the pin rather than inferred:
// runtime/panic.go:1041 pushes rand_fatal, whose body is `fatal(s)` — throwTypeUser, userFault TRUE.

// Aliased rather than imported wholesale: this file needs exactly one golib type, and a blanket
// `using go.golib` would also pull that namespace's extension methods into a hand-owned file sitting
// beside converted code.
using FatalReport = go.golib.FatalReport;

// Hand-owned (no *_impl.go exists, so a reconvert never regenerates it); marked so the marker-based
// readers see it as well as the suffix-based ones.
[module: go.GoManualConversion]

#pragma warning disable CS8826 // the converted partial names the blank Go parameter `_`; this hand-owned body keeps its readable name

namespace go.crypto;

partial class rand_package
{
    internal static partial void fatal(@string s) => FatalReport.Fatal(s, userFault: true);
}
