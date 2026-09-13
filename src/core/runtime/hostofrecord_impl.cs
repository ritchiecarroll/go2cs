// hostofrecord_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The one place the corpus knows which operating system it was BUILT for, checked once against the
// one it is RUNNING on.
//
// A published package whose C# varies by GOOS ships a per-RID assembly for every runtime identifier
// the release supports, plus a RID-agnostic `lib/` copy NuGet resolves when no shipped RID matches.
// That copy has to carry some flavor and it carries the host of record, Windows — so on a RID this
// release does not ship, a Linux consumer silently loads the Windows corpus and gets Windows answers.
// push-nuget.ps1's header has always called that "the honest cost of this choice"; this is the line
// that makes it audible instead of documented. Rationale, and why neither `ref/` nor an exception is
// the better answer, is in golib's GoHostOfRecord.
//
// THE FILE IS FLAT, and that is the whole design. `GOOS` is declared per-GOOS (runtime/<goos>/
// extern.cs), so this one source compiles in every flavor and the only thing that differs between
// them is the constant it hands over. Copying it per platform would put three files where the
// question is one, and would make the neutral `lib/` slot's provenance a question again — it is
// byte-identical to the win-x64 flavor because it IS that flavor, duplicated at pack time.
//
// A module initializer is the right slot for the same reason goargs_impl.cs and goenvs_impl.cs use
// one: it runs when the runtime assembly is first touched, before any converted Go code in it, once.
// Every converted program reaches `runtime` (os -> runtime on every path that has an os), so a
// mismatch is announced before the first wrong answer rather than after it.
//
// This file has no `<name>.go` counterpart, so a -stdlib reconvert never emits over it; the module
// marker states the ownership explicitly and matches the other hand-owned runtime files.

using System.Runtime.CompilerServices;

[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    [ModuleInitializer]
    internal static void ᴛVerifyHostOfRecord()
    {
        GoHostOfRecord.Verify(GOOS);
    }
}
