// comparable.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System.Numerics;

namespace go;

// TODO: Delete this, native types do not implement this interface - must use IEqualityOperators directly instead when comparable is encountered

/// <summary>
/// Represents the C# implementation of the Go built-in <c>comparable</c> constraint.
/// </summary>
/// <remarks>
/// Defines constraints for the `==`, `!=` operators.
/// </remarks>
/// <typeparam name="T">Constrained type.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)] public interface comparable<T> : 
    IEqualityOperators<T, T, bool> 
    where T : comparable<T>
{
}
