// dnsclient_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementation of dnsclient.go's //go:linkname-into-runtime hook (`runtime_rand`,
// provided by runtime.rand). go2cs emits it as a bodyless `partial` method, and without a body here
// the PartialStubGenerator fills it with a throwing stub — the same shape, and the same remedy, as
// os/tempfile_impl.cs, math/rand/rand_impl.cs and math/rand/v2/rand_impl.cs, which are this file's
// three precedents.
//
// WHY IT SURFACED ONLY NOW, WHICH IS THE WHOLE STORY. `net` reaches this hook exclusively through
// the pure-Go resolver in dnsclient_unix.go — `newRequest` picks the DNS query ID with `randInt()`
// (linux/dnsclient_unix.cs:54), and dnsclient.go's own `randIntn` weights SRV target selection
// (:191) and shuffles the address list (:237). Windows never gets there: its resolver is the
// system one (GetAddrInfoW), so the stub sat unreached for the entire Windows campaign. On Linux
// the pure-Go resolver IS the resolver, so the FIRST name lookup the platform ever performed died
// here:
//
//   System.NotImplementedException: runtime_rand: external (assembly or cgo) function is not implemented
//     at net.runtime_rand (PartialStubGenerator stub)
//     at net.randInt        ... dnsclient.cs:23
//     at net.newRequest     ... linux/dnsclient_unix.cs:54
//     at net.exchange       ... linux/dnsclient_unix.cs:188
//     at net.tryOneName     ... linux/dnsclient_unix.cs:345
//     at net.goLookupIPCNAMEOrder (on a goroutine)
//
// MEASURED 2026-08-22, immediately after the Linux readiness poller landed
// (docs/phase4/DESIGN-linux-readiness-poller.md, S2): the poller made sockets work, crypto/tls's
// suite ran for the first time on Linux, and it then hung in TestVerifyHostname — which dials
// www.google.com for real — because the lookup goroutine died on this stub and the test waited for
// a result that could never arrive. A decomposition probe on the same distro confirmed the split:
// native Go resolved www.google.com in 110 ms (16 addresses) while the converted program threw here,
// so the wall was neither the network nor the poller. This is the "adjacent wall" the Windows
// netpoller design named (DESIGN-netpoll-managed-poller.md §6: "DNS, interfaces, runtime_rand"),
// reached from the other side.
//
// WHAT GO ASKS OF runtime.rand HERE. Not statistical quality and not cryptographic strength — Go's
// runtime.rand is a chacha8 PRNG seeded by the runtime, used for exactly three things above. Two
// properties matter:
//
//   1. It must differ ACROSS PROCESSES and be hard for an OFF-PATH party to predict. The DNS query
//      ID is the classic case: an attacker who can guess the 16-bit ID (and the source port) can
//      race a forged answer into the resolver. Go's own bar is a per-process runtime PRNG, not
//      crypto/rand; this file matches that bar rather than exceeding it, because raising it would
//      diverge from the behavior every net test measures. (The truncation to 16 bits is Go's, in
//      newRequest, and is not this file's to widen.)
//   2. It must be safe to call from any goroutine. `goLookupIPCNAMEOrder` fans out concurrent
//      lookups by design (the stack above is one of them), and Go's per-M source needs no lock.
//
// Random.Shared answers both: the CLR seeds it from the OS at first use (per-process, not
// reproducible across runs) and documents every member as thread-safe. NextBytes fills the full
// 64 bits rather than NextInt64's 63, so the value is a faithful uint64 before randInt clears the
// sign bit. Identical in body and in reasoning to os/tempfile_impl.cs — deliberately, since the
// contract is the same one.
//
// SCOPE. Platform-neutral: dnsclient.go is a flat file compiled for every GOOS, so this companion is
// flat too, and Windows — which never calls it — is unaffected in behavior and identical in bytes.

using System;

// Hand-owned (no dnsclient_impl.go exists, so a reconvert never regenerates it); marked for
// consistency with the other hand-owned companions.
[module: go.GoManualConversion]

namespace go;

partial class net_package
{
    internal static partial uint64 runtime_rand()
    {
        Span<byte> bits = stackalloc byte[8];
        Random.Shared.NextBytes(bits);
        return BitConverter.ToUInt64(bits);
    }
}
