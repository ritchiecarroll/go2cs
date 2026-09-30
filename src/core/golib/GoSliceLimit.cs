// GoSliceLimit.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace

using System;

namespace go;

/// <summary>
/// The ONE definition of the longest slice this runtime can span, shared by every door that turns a Go
/// length into a managed window: the native-backed slice window (<c>slice&lt;T&gt;.OverNativeMemory</c>)
/// and <c>unsafe.Slice</c> / <c>unsafe.String</c>. A slice here is built over CLR arrays and spans, so
/// no non-zero-size slice is longer than <see cref="Array.MaxLength"/>; a zero-size element never spans
/// its storage and keeps Go's own length rules (its representation limit stays Int32, the length field).
/// </summary>
public static class GoSliceLimit
{
    /// <summary>Whether <paramref name="length"/> is longer than any managed span can hold.</summary>
    public static bool LongerThanAnySpan(long length) => length > Array.MaxLength;

    /// <summary>Whether <typeparamref name="T"/> occupies no storage in Go (see <c>GoZeroSizeFacts</c>).</summary>
    public static bool IsZeroSize<T>() => GoZeroSizeFacts<T>.IsZeroSize;

    /// <summary>
    /// Whether a window of <paramref name="length"/> elements of <typeparamref name="T"/> is refused: it
    /// is longer than any managed span and the element has storage to span.
    /// </summary>
    public static bool ExceedsManagedSpan<T>(long length) => LongerThanAnySpan(length) && !GoZeroSizeFacts<T>.IsZeroSize;
}
