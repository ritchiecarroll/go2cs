// GoArrayDimsCompanionAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Marks an assembly with an exported generic function that carries an array-dims OVERLOAD: an extra
/// type parameter, bound to <see cref="IGoArrayDims"/>, that carries the length of an unnamed array type
/// argument C# cannot keep in the type.
/// </summary>
/// <param name="functionName">The Go name of the generic function.</param>
/// <param name="typeParamIndex">The index, in the function's Go type-parameter list, of the type parameter the companion serves.</param>
/// <remarks>
/// C# spells every Go <c>[N]E</c> as the one <c>array&lt;E&gt;</c>, so a function that synthesizes a type
/// descriptor from its type parameter alone (<c>unique.Make</c> through <c>abi.TypeFor[T]</c>) has no
/// length to give an array type argument. The function gains an arity-2 overload beside the arity every
/// consumer already binds, and the arity-1 method forwards <see cref="ᴀnone"/>. A consumer in another
/// assembly reads this record and supplies the dims phantom (<c>Make&lt;array&lt;@string&gt;, ᴀ2&gt;</c>) only when
/// its type argument is an unnamed array; it never re-derives the overload from the function's Go source,
/// so a consumer converted against an assembly without the record emits arity 1. Sibling of
/// <see cref="GoDynamicTypeLiftAttribute"/>, another record a consumer reads back from a dependency.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[EditorBrowsable(EditorBrowsableState.Never)] public class GoArrayDimsCompanionAttribute(string functionName, int typeParamIndex) : Attribute
{
    /// <summary>
    /// Gets the Go name of the generic function.
    /// </summary>
    public string FunctionName => functionName;

    /// <summary>
    /// Gets the index of the type parameter the companion serves.
    /// </summary>
    public int TypeParamIndex => typeParamIndex;
}
