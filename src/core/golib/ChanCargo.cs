//******************************************************************************************************
//  ChanCargo.cs - Gbtc
//
//  Copyright © 2026, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//******************************************************************************************************

namespace go;

/// <summary>
/// The unified channel-VALUE cargo (descriptor cargo, increment D): the per-level direction CHAIN and
/// the element ARRAY DIMS of the Go channel type a value was born with — the two facts the managed
/// <c>channel&lt;T&gt;</c> cannot hold, since <c>chan&lt;- [3]int</c>, <c>chan (&lt;-chan int)</c> and
/// <c>chan int</c> all emit over one generic instantiation.
/// </summary>
/// <remarks>
/// ONE reference where a one-byte <see cref="GoChanDir"/> used to sit, so both cargos ride a single
/// field change and the struct's padding is what pays for it — measured, not argued, by the
/// GolibTests cost row. Immutable, and <c>null</c> is the unstamped channel: every pre-D constructor
/// maps <see cref="GoChanDir.Unstamped"/> and <see cref="GoChanDir.Both"/> to it, which keeps the
/// bidirectional channel's canonical spelling exactly what it was.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class ChanCargo
{
    /// <summary>The direction chain, outermost first; never empty on a live instance.</summary>
    public readonly GoChanDir[]? DirChain;

    /// <summary>The element's array dims, outermost first, when the element is a fixed-size array.</summary>
    public readonly nint[]? ElemDims;

    private ChanCargo(GoChanDir[]? dirChain, nint[]? elemDims)
    {
        DirChain = dirChain;
        ElemDims = elemDims;
    }

    // The two scalar directions every pre-D stamp site produces, interned so a directional nil
    // channel allocates nothing per instance.
    private static readonly ChanCargo s_send = new([GoChanDir.Send], null);
    private static readonly ChanCargo s_recv = new([GoChanDir.Recv], null);

    /// <summary>The cargo for a scalar direction, or <c>null</c> for the unstamped/bidirectional case.</summary>
    public static ChanCargo? Of(GoChanDir direction) => direction switch
    {
        GoChanDir.Send => s_send,
        GoChanDir.Recv => s_recv,
        _ => null
    };

    /// <summary>The cargo for a full chain and dims, or <c>null</c> when both are absent.</summary>
    public static ChanCargo? Of(GoChanDir[]? dirChain, nint[]? elemDims)
    {
        if (dirChain is { Length: 0 })
            dirChain = null;

        return dirChain is null && elemDims is null ? null : new ChanCargo(dirChain, elemDims);
    }

    /// <summary>This channel's OWN direction: the chain's head, or unstamped.</summary>
    public GoChanDir Head => DirChain is { Length: > 0 } ? DirChain[0] : GoChanDir.Unstamped;
}
