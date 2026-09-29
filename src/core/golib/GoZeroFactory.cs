// GoZeroFactory.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// The Go zero value of <typeparamref name="T"/> where it must be CONSTRUCTED rather than taken as
/// <c>default(T)</c>: a converted struct holding a fixed-size array or a promoted-embed box, directly or through a
/// nested field. go2cs-gen registers <see cref="Create"/> for each such struct in a module initializer of the
/// struct's own assembly; it stays null for every type whose <c>default</c> is already its Go zero value.
/// </summary>
/// <typeparam name="T">The struct type.</typeparam>
/// <remarks>
/// Read by <see cref="builtin.GoZero{T}()"/>. A static generic field is one load per closed <typeparamref name="T"/>,
/// with no reflection, so nothing here needs a trimming annotation.
/// </remarks>
public static class GoZeroFactory<T>
{
    /// <summary>
    /// Constructs the Go zero value of <typeparamref name="T"/>, or null when <c>default(T)</c> already is one.
    /// </summary>
    public static Func<T>? Create;
}
