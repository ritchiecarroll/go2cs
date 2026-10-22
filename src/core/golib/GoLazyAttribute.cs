// GoLazyAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Marks the holder class of a Go package var built on its FIRST READ (docs/DESIGN-lazy-static-data.md).
/// </summary>
/// <remarks>
/// <para>
/// The converter writes the var's declaration, exactly as it would write an eager one, inside a holder class named for
/// the var: <c>[GoLazy] partial class lazyᴛ_Lu { internal static ж&lt;RangeTable&gt; _Lu = Ꮡ(new RangeTable(…)); }</c>.
/// The holder's field initializer runs when the CLR first initializes the holder, which is the var's first read, instead
/// of in the package class's static constructor. go2cs-gen's <c>LazyHolderGenerator</c> writes the var's <c>ref</c>
/// accessor in the package class, under the var's Go name, so every use site reads as it did.
/// </para>
/// <para>
/// A holder must hold exactly one static field, named for the holder (GO2CS0009 refuses any other by name).
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class GoLazyAttribute : Attribute
{
    /// <summary>
    /// The var is EXPORTED and a lazy initializer of its package reads it: an importer may assign it before that
    /// initializer runs, where Go's init-time build saw its initial value. The generator keeps the initial value apart, in
    /// the holder's <c>initial</c> field, and the converter has lazy initializers read that field.
    /// </summary>
    public bool Initial { get; set; }
}
