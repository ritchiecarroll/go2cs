// GoParkWeakAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.ComponentModel;

namespace go.golib;

/// <summary>
/// Marks the twin of a WEAK member: the twin can park, but a call to the member never makes its caller a twin.
/// </summary>
/// <remarks>
/// <para>
/// M:N (docs/phase4/DESIGN-mn-nopark.md, N1). A caller that is a twin for another reason awaits the marked twin as
/// before; a caller with no other park stays plain and blocks its thread for the call. A lock is the case: a mutex
/// held briefly would otherwise make every function that takes it a state machine. A caller in any assembly reads
/// the mark from metadata, so one member is never read two ways.
/// </para>
/// <para>
/// A member never carries both this mark and <see cref="GoParkForwarderAttribute"/>, which says the member cannot
/// park. The mark is version-safe both ways: adding or removing it later changes only the carrier a blocked caller
/// holds, never a result.
/// </para>
/// <para>
/// Frozen surface (PublicSurface.golib.txt, [mn-frozen]): generated code in user assemblies names it, and sync's
/// hand-written lock twins carry it.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class GoParkWeakAttribute : Attribute;
