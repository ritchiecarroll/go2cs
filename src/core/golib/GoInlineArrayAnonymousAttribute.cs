// GoInlineArrayAnonymousAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.ComponentModel;

namespace go.golib;

/// <summary>
/// Records one ANONYMOUS struct type this package holds with inline arrays, as a digest of its key
/// (docs/phase4/DESIGN-array-value-storage.md, 7.7, ruling C).
/// </summary>
/// <remarks>
/// A converter record, read from <c>package_info.cs</c>. A package that imports this one and admits an identical struct
/// reuses this package's lift of it only when the two admissions agree. <see cref="GoInlineArrayKeysAttribute"/> counts
/// the anonymous keys but cannot say which they are once there are two or more, so the importer reads this record
/// instead. Nothing reads the attribute by reflection.
/// </remarks>
/// <param name="digest">The first 16 hex digits of the SHA-256 of the admitted key, as the converter spells it.</param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class GoInlineArrayAnonymousAttribute(string digest) : Attribute
{
    /// <summary>
    /// Gets the digest of the admitted anonymous key.
    /// </summary>
    public string Digest => digest;
}
