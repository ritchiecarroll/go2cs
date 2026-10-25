// DynamicSecurityMethodAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace System.Security;

/// <summary>
/// Marks a method whose CALLER's frame Go code observes, so the CLR never erases that frame with an implicit tail call.
/// </summary>
/// <remarks>
/// <para>
/// Go never tail-calls, so <c>runtime.Caller</c>, <c>runtime.Callers</c>, <c>t.Helper</c> and log's <c>Lshortfile</c> always
/// see the frame of the function that called them. The optimizing JIT, unlike Go, replaces a call in tail position with a
/// jump when the arguments allow it (on linux/amd64 nearly always), and the calling frame then never appears in a stack
/// walk: slog's <c>TestCallDepthConnection</c> read <c>logger_test.go:294</c> where Go reads <c>:256</c>, because the
/// closure <c>func() { log.Fatalf(...) }</c> tail-called <c>Fatalf</c> (G, 2026-10-25; <c>DOTNET_FastTailCalls=0</c> cleared
/// it).
/// </para>
/// <para>
/// The C# compiler recognizes an attribute by THIS full name, wherever it is declared, and writes
/// <c>MethodAttributes.RequireSecObject</c> on the method it marks. The VM's <c>canTailCall</c> refuses an IMPLICIT tail call
/// into a method carrying that flag (the rule that keeps a <c>StackCrawlMark</c> method's caller on the stack), from every
/// caller in every assembly. CoreLib's own attribute of this name is internal, so golib declares the public one the
/// converted packages use. go2cs-gen writes it beside <c>[MethodImpl(MethodImplOptions.NoInlining)]</c> on every method the
/// converter keeps a frame for, and the hand-owned frame APIs carry it by hand (runtime's <c>Callers</c> and <c>Stack</c>,
/// testing's reporting members).
/// </para>
/// <para>
/// It is a runtime BEHAVIOR, not a documented contract, so GolibTests' <c>TailCallFrameGuardTests</c> fails the day a .NET
/// stops honoring it: an optimized lambda tail-calling a marked method must keep its frame, while the unmarked control
/// loses it. A DELEGATE call cannot be protected from the callee's side (the exact callee is the delegate's <c>Invoke</c>),
/// so the converter marks such a caller <c>NoInlining</c> instead, which makes no implicit tail call at all.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
public sealed class DynamicSecurityMethodAttribute : Attribute
{
}
