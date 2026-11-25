// GoCgoImages.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;

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
        internal required object Key;
        internal required Door Door;
        internal IGoReflectBox? Box;
        internal Type? Type;
        internal byte[]? Image;
        internal byte[]? Before;
    }

    private List<Root>? m_roots;

    private Root? Find(object key)
    {
        if (m_roots is null)
            return null;

        foreach (Root root in m_roots)
        {
            if (ReferenceEquals(root.Key, key))
                return root;
        }

        return null;
    }

    private static PanicException BothDoors(string function) =>
        RuntimeErrorPanic.ErrorString($"cgo: two pointers handed to C.{function} name the same Go value, one as an image (a struct view no door can pin) and one through another door; go2cs would hand C two copies of one Go value, so it refuses the call");

    // A root the mirror or the scalar door hands C: refused when an image of it is already handed over.
    internal void ClaimOther(object key, string function)
    {
        Root? found = Find(key);

        if (found is { Door: Door.Image })
            throw BothDoors(function);

        if (found is null)
            (m_roots ??= []).Add(new Root { Key = key, Door = Door.Other });
    }

    // The image of a root: the existing buffer when another parameter already imaged it, else a new buffer of `size`
    // bytes (at least the root's own Go size; the tail past the root zeroed), the root encoded into it.
    internal unsafe GoCgoPin Image(object key, IGoReflectBox box, object value, Type type, nuint rootSize, nuint size, string function)
    {
        Root? found = Find(key);

        if (found is { Door: Door.Other })
            throw BothDoors(function);

        if (found is { Image: { } shared })
            return new GoCgoPin(shared, 0);

        byte[] image = new byte[(int)Math.Max(rootSize, size)];

        fixed (byte* at = image)
            NativeStructMarshal.EncodeValue(value, type, at);

        (m_roots ??= []).Add(new Root { Key = key, Door = Door.Image, Box = box, Type = type, Image = image, Before = (byte[])image.Clone() });

        return new GoCgoPin(image, 0);
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
