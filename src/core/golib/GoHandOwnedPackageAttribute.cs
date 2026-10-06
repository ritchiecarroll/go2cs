// GoHandOwnedPackageAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Marks an assembly as a HAND-OWNED Go package: one go2cs never converts, whose source is written by hand.
/// </summary>
/// <remarks>
/// <para>
/// Converted code no longer spells what its declarations are: a Go pointer receiver is an unmarked
/// <c>this ref T</c> method, and a Go type is a struct or interface in the package class
/// (docs/PLAN-marker-comment-parity.md). Hand-written code is not held to those shapes, so a hand-owned
/// package opts out with this attribute, written once in its own source as
/// <c>[assembly: GoHandOwnedPackage]</c>. Inside it, a declaration is a Go method or a Go type only by its
/// attribute (<see cref="GoRecvAttribute"/>, <see cref="GoTypeAttribute"/>), which hand-written files keep.
/// </para>
/// <para>
/// The signal travels with the package's source, so the source generators read it from the compilation and
/// golib from the loaded assembly, through every build channel alike. A project that says nothing is a
/// converted one. The packages that carry it are <c>testing</c> and <c>unsafe</c>, the two hand-owned
/// projects with no converted file whose files do not all carry <see cref="GoManualConversionAttribute"/>'s
/// file marker.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class GoHandOwnedPackageAttribute : Attribute;
