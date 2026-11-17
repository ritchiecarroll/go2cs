// IHeaderDataCarrier.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

namespace go.golib;

/// <summary>
/// A header box whose Data is a NUMBER (<c>reflect.StringHeader</c>, <c>reflect.SliceHeader</c>), asked by
/// <c>unsafe.Pointer.FromHeaderData</c> for Go's rule-6 shape, <c>unsafe.Pointer(h.Data)</c> in one expression
/// (docs/phase4/DESIGN-sqlite-ccgo-ts.md, QW-8): the header's current Data word, and the box that holds what it names.
/// </summary>
/// <remarks>
/// Internal: golib's own header boxes are its only implementers, and <c>go.unsafe</c> reaches it through golib's
/// <c>InternalsVisibleTo</c> grant. A public interface would be one more exported type the binary-compat floor carries,
/// and a foreign implementer could make <c>FromHeaderData</c> trust a word golib never minted.
/// </remarks>
internal interface IHeaderDataCarrier
{
    /// <summary>
    /// The header's Data word now, and in <paramref name="carried"/> the element box it was minted from when the word is
    /// the one this box minted (a header over Go memory), or null (an immortal copy, native memory, or a word the program
    /// stored). A pointer built from both keeps <paramref name="carried"/>, and so the memory, alive while it lives.
    /// </summary>
    nuint HeaderDataWord(out object? carried);
}
