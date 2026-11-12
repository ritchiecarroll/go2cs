// ISliceWrap.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

namespace go;

/// <summary>
/// Marks a slice-shaped type that can wrap an existing <see cref="slice{T}"/> window in its own
/// type WITHOUT copying — the factory a generic body needs to keep a constrained type parameter
/// (<c>S ~[]E</c>) through sub-slice and append operations (Go's sub-slice of a named slice type
/// yields the same named type, sharing backing storage).
/// </summary>
/// <typeparam name="TSelf">Implementing type (the named slice type itself).</typeparam>
/// <typeparam name="T">Element type.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)] public interface ISliceWrap<out TSelf, T>
{
    /// <summary>
    /// Wraps the given slice window in <typeparamref name="TSelf"/>, sharing its backing storage.
    /// </summary>
    /// <param name="source">Slice window to wrap.</param>
    /// <returns>A <typeparamref name="TSelf"/> over the same storage.</returns>
    static abstract TSelf Wrap(in slice<T> source);
}
