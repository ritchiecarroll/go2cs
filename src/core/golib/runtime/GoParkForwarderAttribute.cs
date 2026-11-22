// GoParkForwarderAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.ComponentModel;

namespace go.golib;

/// <summary>
/// Marks a twin that is a FORWARDER: it calls its plain member and returns a completed task, because the generator
/// did not write a state machine for that member (it cannot park, or its body or plain lowering refused one).
/// </summary>
/// <remarks>
/// <para>
/// M:N (docs/phase4/DESIGN-mn-twin-set.md, R3 and 12.1). A package built with twins gives every public member of its
/// converted code a twin of the Go-shaped signature, so a twin never disappears from one release to the next. Where
/// that twin would only wrap the plain call, it is a forwarder and carries this mark. A caller's twin, in this
/// assembly or another, binds the PLAIN member of a marked twin and never awaits the forwarder.
/// </para>
/// <para>
/// Frozen surface (PublicSurface.golib.txt, [mn-frozen]): generated code in user assemblies names it.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class GoParkForwarderAttribute : Attribute;
