// GoReflect.InteriorAlias.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;

namespace go;

// ---------------------------------------------------------------------------------------------
// INTERIOR ALIAS RESOLUTION — Q44 seat (c), M4 (ruled 2026-09-26, ledger 19ab026dca).
//
// THE SITE. reflect's own `setField` writes `*(*V)(unsafe.Add(unsafe.Pointer(&in), offset))` over a
// REFERENCE-BEARING struct S. Such storage has no address, so the number the uintptr operator receives
// is S's box ORDER TOKEN plus a Go-layout byte offset, and before this step both of the operator's
// arms that see it only ever refused: arm 2a at offset 0 (a live token of another pointee type) and
// the token-arithmetic refusal at any other offset.
//
// THE STEP. Given the BASE box and the offset k, walk the base's GO layout — the struct field whose
// [offset, offset + GoSizeOf) contains k (GoFieldOffsets, the same memoized pass that stamps Size_),
// or the array element k / stride — repeating on the remainder. It STOPS only where the remainder is
// 0 AND the node's type IS V (exact System.Type equality), and answers with golib's EXISTING alias
// boxes (FieldAliasBox, reflect's Field(i).Addr() factory, and the element box `at` builds), so the
// write lands in the real field and `&s.f == &s.f` holds across two resolutions.
//
// IT REINTERPRETS NOTHING. Every other outcome — a remainder inside a scalar, a type mismatch (which
// would be a reinterpret), a promoted-embed box hop, padding, an array whose dims are unknown, an
// unknowable layout — answers null, and the operator keeps TODAY's refusal exactly as it was. A
// layout-identical DEFINED type (Go's `type T int64` against `int64`) is a type mismatch here by
// ruling; widening that is a named follow-up, not a silent extension.
// ---------------------------------------------------------------------------------------------
public static partial class GoReflect
{
    /// <summary>
    /// reflect.NewAt's resolution of its pointer: the box over REAL storage that <paramref name="number"/>
    /// names as a <paramref name="target"/>, or <c>null</c> when it names none, and NewAt keeps the zero
    /// box it has always returned.
    /// </summary>
    /// <remarks>
    /// <para>
    /// go-cmp reads an unexported field as <c>reflect.NewAt(f.Type, unsafe.Pointer(v.UnsafeAddr() +
    /// f.Offset)).Elem()</c>. NewAt ignored its pointer, so every such field read as zero and two
    /// structs that differed only there compared equal (cmpopts IgnoreUnexported, IgnoreFields).
    /// </para>
    /// <para>
    /// EXACT ONLY (COORD ruling, 2026-10-04): the number must be a live registered token (or a pinned
    /// address the table still validates), resolved at offset 0, or a live token's own block plus an
    /// offset; and the resolution is the interior-alias walk above, which answers only where the Go
    /// offset lands exactly on a <paramref name="target"/>-typed node. A token whose box has been
    /// collected does not resolve (the table holds boxes WEAKLY), so a stale pointer answers
    /// <c>null</c> too. No token value is minted or changed here.
    /// </para>
    /// </remarks>
    public static object? ResolveNewAtPointee(nuint number, Type target)
    {
        if (number == 0)
            return null;

        if (ManagedPointerTokens.Resolve(number) is { } box)
            return ResolveInteriorAlias(box, 0, target, out _);

        if (ManagedPointerTokens.IsTokenArithmetic(number) &&
            ManagedPointerTokens.ResolveArithmeticBase(number, out nuint offset) is { } baseBox)
        {
            return ResolveInteriorAlias(baseBox, offset, target, out _);
        }

        return null;
    }

    /// <summary>
    /// Resolves the Go interior pointer <c>(*V)(base + offset)</c> to an alias box over the real
    /// storage of <paramref name="baseBox"/>, or returns <c>null</c> when the offset does not land
    /// EXACTLY on a <paramref name="target"/>-typed node of the base's Go layout.
    /// </summary>
    /// <param name="baseBox">The live <c>ж&lt;S&gt;</c> the order token named.</param>
    /// <param name="offset">The Go-layout byte offset from the base.</param>
    /// <param name="target">The requested pointee type V.</param>
    /// <param name="shape">The PATH SHAPE taken (<c>.f[i].g</c>) or, on a refusal, its NAME — the census record.</param>
    internal static object? ResolveInteriorAlias(object baseBox, nuint offset, Type target, out string shape)
    {
        shape = "";

        if (boxPointeeType(baseBox) is not { } nodeType)
        {
            shape = "refused: unresolvable base";
            return null;
        }

        object node = baseBox;
        nint[]? dims = KindOf(nodeType) == Array ? ArrayDimsOfValue(readBoxValue(baseBox)) : null;

        for (int depth = 0; depth <= MaxLayoutDepth; depth++)
        {
            if (offset == 0 && nodeType == target)
                return node;

            switch (KindOf(nodeType))
            {
                case Struct:
                {
                    if (GoFieldOffsets(nodeType) is not { } offsets)
                    {
                        shape += " refused: unknowable layout";
                        return null;
                    }

                    GoFieldInfo[] fields = GoFields(nodeType);
                    int chosen = -1;

                    for (int i = 0; i < fields.Length; i++)
                    {
                        nint[]? fieldDims = KindOf(fields[i].Type) == Array ? fields[i].ArrayDims : null;

                        // A zero-size field contains no byte, so it can never be the one k lands in.
                        if (!TryGoSizeOf(fields[i].Type, fieldDims, out nuint size) || size == 0)
                            continue;

                        nuint start = (nuint)offsets[i];

                        if (offset >= start && offset - start < size)
                        {
                            chosen = i;
                            break;
                        }
                    }

                    if (chosen < 0)
                    {
                        shape += " refused: offset in padding";
                        return null;
                    }

                    GoFieldInfo field = fields[chosen];

                    // A promoted embed held through a ж<T> box is a SECOND allocation in the managed
                    // model and a contiguous one in Go's: no offset may cross it.
                    if (System.Array.IndexOf(field.BoxHop, true) >= 0)
                    {
                        shape += $" refused: box hop at .{field.Name}";
                        return null;
                    }

                    node = FieldAliasBox(node, field);
                    offset -= (nuint)offsets[chosen];
                    nodeType = field.Type;
                    dims = KindOf(field.Type) == Array ? field.ArrayDims : null;
                    shape += $".{field.Name}";
                    continue;
                }

                case Array:
                {
                    if (dims is not { Length: > 0 } || dims[0] < 0 || ElementType(nodeType) is not { } elemType)
                    {
                        shape += " refused: unknown-dims array";
                        return null;
                    }

                    nint[]? innerDims = dims.Length > 1 ? dims[1..] : null;

                    if (!TryGoSizeOf(elemType, innerDims, out nuint stride) || stride == 0)
                    {
                        shape += " refused: unknowable stride";
                        return null;
                    }

                    nuint index = offset / stride;

                    if (index >= (nuint)dims[0])
                    {
                        shape += " refused: past the array";
                        return null;
                    }

                    node = ElementAliasBoxOfBox(node, elemType, (nint)index);
                    offset -= index * stride;
                    nodeType = elemType;
                    dims = innerDims;
                    shape += $"[{index}]";
                    continue;
                }

                default:
                    shape += offset == 0
                        ? $" refused: type mismatch ({nodeType.Name} is not {target.Name})"
                        : " refused: offset inside a scalar";
                    return null;
            }
        }

        shape += " refused: depth";
        return null;
    }

    // The T of the ж<T> a box derives from — the pointee's static Go type.
    private static Type? boxPointeeType(object box)
    {
        for (Type? type = box.GetType(); type is not null; type = type.BaseType)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ж<>))
                return type.GetGenericArguments()[0];
        }

        return null;
    }

    private static object? readBoxValue(object box) => ReadPointerSlot(box);
}
