// IElementViewSlot.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

namespace go;

/// <summary>
/// EXPERIMENT (i9, B4 lever G): an element box's field-view slot, reached without naming
/// <c>ElemRefBox&lt;T&gt;</c> from <c>ж&lt;T&gt;</c>.
/// </summary>
/// <remarks>
/// <c>ж&lt;T&gt;</c>'s view lookup tested <c>this is ElemRefBox&lt;T&gt;</c>. That one mention makes every
/// pointer type construct its element box, which names <c>array&lt;T&gt;</c>, whose <c>AliasPointer</c> names
/// <c>ж&lt;array&lt;T&gt;&gt;</c>, and so on one nesting level deeper each lap: the generic cycle Native AOT
/// expands to its cut-off. A non-generic face names nothing per type.
/// </remarks>
internal interface IElementViewSlot
{
    /// <summary>The head of the box's view list.</summary>
    ViewEntry? Views { get; }

    /// <summary>The slot itself, for the publishing path.</summary>
    ref ViewEntry? ViewsRef { get; }
}
