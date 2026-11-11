// IGoValueClone.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Marks a converted Go STRUCT whose C# value copy is not a complete copy on its own — it holds
/// one or more FIXED-SIZE ARRAY fields (directly, or through another such struct).
/// </summary>
/// <remarks>
/// Go copies an array inline at every value transfer, but the emitted <see cref="array{T}"/> is a
/// struct over a shared <c>T[]</c> backing, so a plain C# struct copy leaves the arrays ALIASED —
/// the copy's writes reach back into the source (crypto/sha256's <c>d0 := *d</c> in <c>Sum</c> is
/// the canonical case: finalizing the copy destroyed the caller's running state). The converter
/// stamps such a struct with <see cref="GoValueCloneAttribute"/>, go2cs-gen generates the
/// field-precise <c>Clone()</c>, and every Go by-value copy site calls it.
///
/// This marker is what lets a CONTAINER clone recurse: <see cref="array{T}.Clone"/> re-clones an
/// element that is itself an array wrapper (<see cref="IArray"/>) or one of these structs, so
/// <c>[2]digest</c> copies both digests' arrays exactly as Go does.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)] public interface IGoValueClone : ICloneable;
