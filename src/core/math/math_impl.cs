// math_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

namespace go;

partial class math_package
{
    // math's Exp/Log/Hypot/Floor/Ceil/Trunc/Max/Min are assembly-optimized on amd64/arm64/s390x/etc.,
    // so the converter emitted the arch* variants as bodyless partials — throwing stubs. Go ships
    // pure-Go fallbacks (exp/log/hypot/floor/ceil/trunc/max/min, used on non-asm platforms); these
    // supply the equivalent managed bodies by delegating to them, so the family RUNS on the managed
    // target. Living in an _impl.cs companion (never emitted by the converter) makes this durable by
    // construction: a -stdlib reconvert regenerates the pristine *_asm.cs stubs and these bodies remain.
    //
    // ONLY THE MEMBERS EVERY BUILD DECLARES BODYLESS ARE HERE. Go's exp_asm.go, floor_asm.go and dim_asm.go
    // cover amd64 and arm64 alike, so their partials are flat and so are these bodies. archLog and archHypot
    // are assembly only at amd64. At linux/arm64 Go's own log_stub.go and hypot_noasm.go body them, so their
    // bodies live in amd64/log_asm_impl.cs and amd64/hypot_asm_impl.cs (S3 site 9). arm64's own assembly
    // member, archExp2, is bodied in linux-arm64/exp2_asm_impl.cs.

    internal static partial float64 archExp(float64 x) => exp(x);

    internal static partial float64 archFloor(float64 x) => floor(x);

    internal static partial float64 archCeil(float64 x) => ceil(x);

    internal static partial float64 archTrunc(float64 x) => trunc(x);

    internal static partial float64 archMax(float64 x, float64 y) => max(x, y);

    internal static partial float64 archMin(float64 x, float64 y) => min(x, y);
}
