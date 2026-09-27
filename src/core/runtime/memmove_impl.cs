// memmove_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Concurrent;
using System.Reflection;
using go.golib;

[module: go.GoManualConversion]

namespace go;

// Bodies for runtime.memmove and runtime.memclrNoHeapPointers -- two assembly primitives
// (runtime/memmove_*.s, memclr_*.s) that runtime/stubs.go declares bodyless and the
// PartialStubGenerator otherwise fills with a NotImplementedException. They DO have an exact managed
// form (stubs_impl.cs's rule), and leaving them throwing killed the runtime row's test host at
// TestMemmoveAtomicity (10,233 of 10,891 results in): `Memmove` and `MemclrNoHeapPointers` are
// export_test.go aliases the suite calls directly, inside a goroutine.
//
// memequal (memequal_*.s) joined them later with the same three-way reading of a pointer; its
// first reached caller is traceMap.put (tracemap_impl.cs).
//
// Hand-owned: there is no memmove_impl.go, so a -stdlib reconvert never regenerates this file.
//
// WHAT A POINTER IS HERE decides the copy, three ways:
//
//   1. Both numbers are REAL ADDRESSES -- native memory, or managed storage golib pinned when the
//      pointer was taken (a primitive element array). The copy is a byte copy, and
//      Buffer.MemoryCopy is documented overlap-safe ("as if the source were first copied to a
//      temporary location"), which is memmove's contract rather than memcpy's.
//
//   2. Both are ORDER TOKENS naming managed ARRAY ELEMENTS -- what `unsafe.Pointer(&s[i])` yields
//      over an array the CLR cannot pin, e.g. `[N]*int`. There are no bytes to copy: each element is
//      a managed reference. The Go meaning of copying n bytes of a pointer array is copying
//      n / sizeof(elem) whole elements, so that is what is done, through a temporary (overlap-safe in
//      either direction) and one element store at a time. That also gives Go's guarantee
//      TestMemmoveAtomicity checks: a concurrent reader sees each pointer slot either old or new,
//      never torn, because a reference store is atomic.
//
//   3. Anything else -- a token that resolves to no element, a token beside a real address, or an
//      element type whose Go size this file cannot state -- is REFUSED loudly with golib's arm-2a
//      panic (Q44 §10.3), never byte-copied: a reference-bearing managed value has no byte image,
//      and writing one would fabricate references.
//
// THE ORDER: elements (2) are tried BEFORE addresses (1). A number that is not a token is not
// always a stable address: golib's FromBox mints a TRANSIENT one (the element's address under a
// `fixed` that has already ended) and retains the element box beside it, which is what a slice
// header's array word carries ((*slice)(unsafe.Pointer(&b)), export_test.go's MemclrBytes). A
// compacting collection between that read and the copy moves the backing, and writing through the
// old number misses the slice and lands on whatever the collector put there: runtime's TestMemclr
// read "failed clear mem[200] = 238" on linux. The retained box is always current, so a pointer that
// names managed elements is served through them, pinned or not. Case 1 keeps every pointer that
// names no managed element: native memory and bare numbers.
partial class runtime_package
{
    internal static partial void memmove(unsafe_package.Pointer to, unsafe_package.Pointer from, uintptr n)
    {
        if (n.Value == 0)
            return;

        nuint dst = to is null ? 0 : to.Value.Value;
        nuint src = from is null ? 0 : from.Value.Value;

        // Go faults on a nil operand; a raw copy through address 0 would be an access violation that
        // takes the process down instead of the catchable nil-dereference panic Go raises.
        if (dst == 0 || src == 0)
            throw RuntimeErrorPanic.NilPointerDereference();

        // Elements first, whenever both pointers name managed array elements (see the header: a
        // number that is not a token can still be a TRANSIENT address the collector has moved).
        if (TryElementRange(to!, n, out IArray? dstArray, out int dstIndex, out int count) &&
            TryElementRange(from!, n, out IArray? srcArray, out int srcIndex, out int srcCount) &&
            count == srcCount && ElementTypeOf(dstArray!) == ElementTypeOf(srcArray!))
        {
            object?[] staged = new object?[count];

            for (int i = 0; i < count; i++)
                staged[i] = srcArray![srcIndex + i];

            for (int i = 0; i < count; i++)
                dstArray![dstIndex + i] = staged[i];

            return;
        }

        if (!ManagedPointerTokens.IsTaggedToken(dst) && !ManagedPointerTokens.IsTaggedToken(src))
        {
            unsafe
            {
                Buffer.MemoryCopy((void*)src, (void*)dst, n.Value, n.Value);
            }

            return;
        }

        throw RefuseTokenBytes(ManagedPointerTokens.IsTaggedToken(dst) ? dst : src);
    }

    // memequal (runtime/memequal_*.s, declared bodyless in stubs.go) reports whether the size bytes at a
    // and b are equal. The same three cases as memmove decide it: two real addresses compare as bytes;
    // two order tokens over managed array elements compare element by element (the Go meaning of
    // comparing size bytes of such arrays); anything else refuses by name. traceMap.put reaches it
    // with the node's managed data slice and the caller's string bytes.
    internal static partial bool memequal(unsafe_package.Pointer a, unsafe_package.Pointer b, uintptr size)
    {
        if (size.Value == 0)
            return true;

        nuint left = a is null ? 0 : a.Value.Value;
        nuint right = b is null ? 0 : b.Value.Value;

        if (left == 0 || right == 0)
            throw RuntimeErrorPanic.NilPointerDereference();

        if (left == right)
            return true;

        if (!ManagedPointerTokens.IsTaggedToken(left) && !ManagedPointerTokens.IsTaggedToken(right))
        {
            unsafe
            {
                int n = checked((int)size.Value);
                return new ReadOnlySpan<byte>((void*)left, n).SequenceEqual(new ReadOnlySpan<byte>((void*)right, n));
            }
        }

        if (TryElementRange(a!, size, out IArray? leftArray, out int leftIndex, out int count) &&
            TryElementRange(b!, size, out IArray? rightArray, out int rightIndex, out int rightCount) &&
            count == rightCount && ElementTypeOf(leftArray!) == ElementTypeOf(rightArray!))
        {
            for (int i = 0; i < count; i++)
            {
                if (!Equals(leftArray![leftIndex + i], rightArray![rightIndex + i]))
                    return false;
            }

            return true;
        }

        throw RefuseTokenBytes(ManagedPointerTokens.IsTaggedToken(left) ? left : right);
    }

    internal static partial void memclrNoHeapPointers(unsafe_package.Pointer ptr, uintptr n)
    {
        if (n.Value == 0)
            return;

        nuint address = ptr is null ? 0 : ptr.Value.Value;

        if (address == 0)
            throw RuntimeErrorPanic.NilPointerDereference();

        // Elements first, whenever the pointer names managed array elements (see the header).
        if (TryElementRange(ptr!, n, out IArray? array, out int index, out int count))
        {
            object? zero = ZeroOf(ElementTypeOf(array!));

            for (int i = 0; i < count; i++)
                array![index + i] = zero;

            return;
        }

        if (!ManagedPointerTokens.IsTaggedToken(address))
        {
            unsafe
            {
                new Span<byte>((void*)address, checked((int)n.Value)).Clear();
            }

            return;
        }

        throw RefuseTokenBytes(address);
    }

    // The element run a token names: its collection, first index, and how many whole elements n bytes
    // span. False when the token resolves to no array element, when the element type has no Go size
    // this file can state, or when n is not a whole number of elements (a partial element of a
    // managed value has no meaning).
    private static bool TryElementRange(unsafe_package.Pointer pointer, uintptr n, out IArray? array, out int index, out int count)
    {
        array = null;
        index = 0;
        count = 0;

        if (pointer.Referent is not { } box || ArrayRefOf(box) is not var (collection, first))
            return false;

        nuint size = GoSizeOf(ElementTypeOf(collection));

        if (size == 0 || n.Value % size != 0)
            return false;

        array = collection;
        index = first;
        count = checked((int)(n.Value / size));

        return true;
    }

    // Go's size for the element types a token can name. A reference-typed CLR element is a Go
    // POINTER-shaped word (ж<T>, a named pointer wrapper, a pointer-represented value), so it is
    // PtrSize; a CLR primitive is its own width. Everything else answers 0 -- refused, not guessed.
    private static nuint GoSizeOf(Type? element)
    {
        if (element is null)
            return 0;

        if (!element.IsValueType)
            return (nuint)IntPtr.Size;

        return element.IsPrimitive ? (nuint)System.Runtime.InteropServices.Marshal.SizeOf(element) : 0;
    }

    private static Type? ElementTypeOf(IArray array) => array.Source?.GetType().GetElementType();

    private static object? ZeroOf(Type? element) =>
        element is { IsValueType: true } ? Activator.CreateInstance(element) : null;

    // ж<T>.ArrayRef is an internal VIRTUAL on a generic base; the caller here holds the box as
    // object. One PropertyInfo per closed box type, cached.
    private static readonly ConcurrentDictionary<Type, PropertyInfo?> s_arrayRefProperty = new();

    private static (IArray, int)? ArrayRefOf(object box)
    {
        PropertyInfo? property = s_arrayRefProperty.GetOrAdd(box.GetType(), static type =>
            type.GetProperty("ArrayRef", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));

        return property?.GetValue(box) as (IArray, int)?;
    }

    private static PanicException RefuseTokenBytes(nuint token) =>
        RuntimeErrorPanic.UnsafePointerOrderTokenDereferenced(typeof(byte), token);

    // TEST SEAMS (pinner_impl.cs's pattern): runtime's own suite reaches these through export_test.go
    // (Memmove, MemclrNoHeapPointers); GolibTests is not in the InternalsVisibleTo grant.
    public static void GoMemmove(unsafe_package.Pointer to, unsafe_package.Pointer from, uintptr n) => memmove(to, from, n);

    public static void GoMemclrNoHeapPointers(unsafe_package.Pointer ptr, uintptr n) => memclrNoHeapPointers(ptr, n);
}
