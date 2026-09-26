// GoCpuProfilerRegistration.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Linked into an OPTED-IN program by GoCpuProfiler.targets, never compiled into the companion itself:
// the program's own module initializer always runs, so the sampler is registered before any Go code
// can start a CPU profile.

namespace go.CpuProfiler.Registration;

internal static class GoCpuProfilerRegistration
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void Register() => go.CpuProfiler.EventPipeSampler.Register();
}
