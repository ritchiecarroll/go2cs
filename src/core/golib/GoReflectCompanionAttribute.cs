// GoReflectCompanionAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Marks a bridge-owned companion field (e.g. <c>reflect.Value.boxed</c>/<c>addrBox</c>,
/// <c>MapIter.mapEnum</c>, <c>abi.Type.sysType</c>) as NOT a Go struct field, so the reflection
/// bridge's Go-field projection (<c>GoReflect.GoFields</c>) excludes it — by declaration, never
/// by a hard-coded name list (each bridge increment adds companions; a stale name list silently
/// reports phantom Go fields).
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class GoReflectCompanionAttribute : Attribute
{
}
