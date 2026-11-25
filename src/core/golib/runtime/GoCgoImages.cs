// GoCgoImages.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace go.golib;

/// <summary>
/// The Go roots one cgo call hands C, and the IMAGES it hands C for the roots no door can pin (cgo Phase 3, increment
/// 3.4, the struct image door: docs/phase4/DESIGN-cgo-phase3-marshaling.md 19.7 and 19.9). A struct pointer that is a
/// reinterpret view of a Go struct holding a managed reference (net's <c>(*C.struct_sockaddr)(unsafe.Pointer(&amp;sa))</c>,
/// sa a syscall.RawSockaddrInet4 whose inline arrays are references here) has no storage C can read, so its ROOT is
/// encoded at Go's own offsets into a buffer the wrapper pins for the call, and decoded back after it.
/// </summary>
/// <remarks>
/// <para>
/// One set per call, declared by the wrapper of any C function that takes a struct pointer. Within it (P3's read of
/// 19.7, COORD inbox H 20261125T034229Z):
/// <list type="bullet">
/// <item>ONE image per root, keyed by the root's allocation identity (<c>ReferentObject</c>): a second view of the same
/// root shares the first buffer, and the root is decoded once (B5);</item>
/// <item>a root that one parameter hands C as an image and another through the mirror or the scalar door is refused by
/// name, never handed over as two copies (B5);</item>
/// <item>a root is decoded back only when C changed its image, so a goroutine's write to the Go value during a blocking
/// call is not erased by a buffer C never wrote (B6), and an untouched image never re-encodes padding, a NaN payload or a
/// bool byte other than 0 and 1 (B7).</item>
/// </list>
/// The window where both C and a goroutine write the root is the documented limit: C's write lands at copy-back, not
/// live (docs/Limitations.md, beside the scalar door's).
/// </para>
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)] public struct GoCgoImages
{
    private enum Door { Image, Other }

    private sealed class Root
    {
        // The allocations this entry hands C: for an image, its root and every backing array inside the root's value (an
        // inline Go array is its own allocation here); for another door, the pointer's root and the storage it pins.
        internal required object[] Keys;
        internal required Door Door;
        internal IGoReflectBox? Box;
        internal Type? Type;
        internal byte[]? Image;
        internal byte[]? Before;

        internal bool Overlaps(object[] keys)
        {
            foreach (object mine in Keys)
            {
                foreach (object key in keys)
                {
                    if (ReferenceEquals(mine, key))
                        return true;
                }
            }

            return false;
        }
    }

    private List<Root>? m_roots;

    private Root? Find(object[] keys, Door door)
    {
        if (m_roots is null)
            return null;

        foreach (Root root in m_roots)
        {
            if (root.Door == door && root.Overlaps(keys))
                return root;
        }

        return null;
    }

    private static PanicException BothDoors(string function) =>
        RuntimeErrorPanic.ErrorString($"cgo: two pointers handed to C.{function} name the same Go value, one as an image (a struct view no door can pin) and one through another door; go2cs would hand C two copies of one Go value, so it refuses the call");

    // A pointer the mirror, the scalar door or a pin hands C: refused when it names an allocation an image already holds
    // (the image's root, or an array inside it: `&sa.Addr[0]` beside a view of sa).
    internal void ClaimOther(object[] keys, string function)
    {
        if (Find(keys, Door.Image) is not null)
            throw BothDoors(function);

        (m_roots ??= []).Add(new Root { Keys = keys, Door = Door.Other });
    }

    // The image of a root: the existing buffer when another parameter already imaged it, else a new buffer of `size`
    // bytes (at least the root's own Go size; the tail past the root zeroed), the root encoded into it.
    internal unsafe GoCgoPin Image(object[] keys, IGoReflectBox box, object value, Type type, nuint rootSize, nuint size, string function)
    {
        if (Find(keys, Door.Other) is not null)
            throw BothDoors(function);

        if (Find(keys, Door.Image) is { Image: { } shared })
            return new GoCgoPin(shared, 0);

        byte[] image = AllocationCounter.NewArray<byte>((int)Math.Max(rootSize, size));

        fixed (byte* at = image)
            NativeStructMarshal.EncodeValue(value, type, at);

        (m_roots ??= []).Add(new Root { Keys = keys, Door = Door.Image, Box = box, Type = type, Image = image, Before = AllocationCounter.CopyOf<byte>(image) });

        return new GoCgoPin(image, 0);
    }

    // The keys of an image: the root, then every backing array the root's value holds, nested structs and the elements
    // of an array of structs included. An image root holds no Go pointer, so these are all the allocations it spans.
    internal static object[] ImageKeys(object root, object value)
    {
        List<object> keys = [root];
        AddBackings(keys, value);
        return [.. keys];
    }

    // The value is a converted Go struct (or an array<T> of one) whose fields the registry keeps, as NativeStructMarshal's
    // encode of the same value already relies on.
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2075", Justification = GoTypeRegistry.RegisteredTypeJustification)]
    private static void AddBackings(List<object> keys, object? value)
    {
        if (value is null)
            return;

        Type type = value.GetType();

        if (type.IsPrimitive || type.IsEnum || !type.IsValueType)
            return;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(array<>))
        {
            if (type.GetField("m_array", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(value) is not Array backing)
                return;

            keys.Add(backing);

            Type element = type.GetGenericArguments()[0];

            if (!element.IsPrimitive && !element.IsEnum && element.IsValueType)
            {
                foreach (object? item in backing)
                    AddBackings(keys, item);
            }

            return;
        }

        foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            AddBackings(keys, field.GetValue(value));
    }

    /// <summary>
    /// After the call: each image C changed is decoded back into its root, once. An image C left byte-for-byte alone is not
    /// decoded at all.
    /// </summary>
    public readonly unsafe void CopyBack()
    {
        if (m_roots is null)
            return;

        foreach (Root root in m_roots)
        {
            if (root is not { Door: Door.Image, Image: { } image, Before: { } before, Box: { } box, Type: { } type } || image.AsSpan().SequenceEqual(before))
                continue;

            fixed (byte* at = image)
                box.WriteBoxSlot(NativeStructMarshal.DecodeValue(type, at));
        }
    }

    // TEST SEAM: the number of distinct images this call holds.
    internal readonly int ImageCount
    {
        get
        {
            int count = 0;

            if (m_roots is not null)
            {
                foreach (Root root in m_roots)
                {
                    if (root.Door == Door.Image)
                        count++;
                }
            }

            return count;
        }
    }
}
