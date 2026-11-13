// GoInlineLayoutAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.ComponentModel;

namespace go.golib;

/// <summary>
/// Marks a struct the converter admitted to inline arrays (<c>-inline-arrays</c>; docs/phase4/DESIGN-array-value-storage.md,
/// 7.7.4): its arrays are members of the <c>go.arrays</c> family, its <c>[0]T</c> fields are alignment carriers, and its
/// fields sit at Go's offsets.
/// </summary>
/// <remarks>
/// go2cs-gen implements <see cref="IGoManagedLayout"/> for every struct carrying it, so golib's inline-layout gate
/// (<c>GoReflect.InlineLayoutMismatch</c>) can compare the struct's managed layout with its Go layout at every level.
/// </remarks>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class GoInlineLayoutAttribute : Attribute;
