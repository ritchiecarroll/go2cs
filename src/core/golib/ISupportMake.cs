// ISupportMake.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

namespace go;

/// <summary>
/// Defines an interface to support 'make' function parameters.
/// </summary>
public interface ISupportMake<out T> : IGoReflectMake where T : ISupportMake<T>
{
    /// <summary>
    /// Initializes type with make size and capacity parameters.
    /// </summary>
    /// <param name="p1">First integer parameter, commonly for size.</param>
    /// <param name="p2">Second integer parameter, commonly for capacity.</param>
    static abstract T Make(nint p1, nint p2);

    // reflect.MakeSlice / MakeMap / MakeChan's construction (GoReflect.MakeContainer; trim stage 3c-2a), compiled for every
    // container and generated named container wrapper the program has. Reached through a zero value of the container type,
    // so it reads nothing of this value: it is the static Make, asked of a type known only at run time.
    object IGoReflectMake.ReflectMake(nint p1, nint p2) => T.Make(p1, p2)!;
}

/// <summary>
/// The object-form <c>make</c> of a container type known only at run time (<c>GoReflect.MakeContainer</c>), implemented
/// once, generically, by <see cref="ISupportMake{T}"/>'s default member (trim stage 3c-2a).
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IGoReflectMake
{
    /// <summary>A new value of this container type made with <paramref name="p1"/> and <paramref name="p2"/>.</summary>
    object ReflectMake(nint p1, nint p2);
}
