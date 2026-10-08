// GoCopyBoundAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Marks a <c>this ref X</c> receiver method that belongs to <c>X</c>'s VALUE method set and is bound
/// through a COPY of its receiver.
/// </summary>
/// <remarks>
/// <para>
/// A by-reference receiver is a POINTER-receiver method unless it carries this attribute. The one shape
/// that is not: go2cs-gen's forwarder of a pointer-receiver method promoted through an embedded POINTER,
/// which Go places in the outer type's value method set (the embedded pointer is what the method mutates
/// through, and a copy shares it). The generator writes this attribute on that forwarder, so converted
/// code needs no mark on its own <c>this ref</c> methods (docs/PLAN-marker-comment-parity.md, 5.1).
/// </para>
/// <para>
/// A <see cref="GoRecvAttribute"/> still present (hand-written files keep it) means pointer receiver, as
/// it always has; the two are never written together.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public sealed class GoCopyBoundAttribute : Attribute;
