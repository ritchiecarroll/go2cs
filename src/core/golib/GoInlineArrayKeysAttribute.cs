// GoInlineArrayKeysAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.ComponentModel;

namespace go.golib;

/// <summary>
/// Records how many struct types this package holds with inline arrays, and a digest of their keys, ANONYMOUS struct
/// types included (docs/phase4/DESIGN-array-value-storage.md, 7.7, part D).
/// </summary>
/// <remarks>
/// A converter record, read from <c>package_info.cs</c>. A <c>-tests</c> conversion recomputes the admission from the
/// production files under the recorded rule and refuses when the count or digest differs, so a package whose admitted
/// types are all anonymous (which <see cref="GoInlineArrayTypeAttribute"/> cannot name) is still checked. Nothing reads
/// the attribute by reflection.
/// </remarks>
/// <param name="count">The number of struct types admitted.</param>
/// <param name="digest">The first 16 hex digits of the SHA-256 of the admitted keys, sorted, one per line.</param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class GoInlineArrayKeysAttribute(int count, string digest) : Attribute
{
    /// <summary>
    /// Gets the number of struct types admitted.
    /// </summary>
    public int Count => count;

    /// <summary>
    /// Gets the digest of the admitted keys.
    /// </summary>
    public string Digest => digest;
}
