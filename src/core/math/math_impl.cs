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

    internal static partial float64 archExp(float64 x) => exp(x);

    internal static partial float64 archLog(float64 x) => log(x);

    internal static partial float64 archHypot(float64 p, float64 q) => hypot(p, q);

    internal static partial float64 archFloor(float64 x) => floor(x);

    internal static partial float64 archCeil(float64 x) => ceil(x);

    internal static partial float64 archTrunc(float64 x) => trunc(x);

    internal static partial float64 archMax(float64 x, float64 y) => max(x, y);

    internal static partial float64 archMin(float64 x, float64 y) => min(x, y);
}
