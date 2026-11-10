// GoImage.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

using System;
using System.Buffers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using go.golib;
using static go.GoReflect;

namespace go;

/// <summary>
/// A window of the Go-layout BYTES of the struct a box holds: Go's <c>(*[unsafe.Sizeof(s)]byte)(unsafe.Pointer(&amp;s))[lo:hi]</c>
/// where the struct is REFERENCE-BEARING, so it has no address to view. <c>copy</c> accepts it for either argument.
/// </summary>
/// <remarks>
/// <para>
/// Design: docs/phase4/DESIGN-reinterpret-seam.md (COORD ruling (A)). A struct holding a fixed array keeps that array as a
/// managed <see cref="array{T}"/>, so its Go-layout bytes live in two managed objects and no <see cref="slice{T}"/> can alias
/// them, and a box cannot observe a write made through a view it handed out. So the view is not an alias: <c>copy</c> builds
/// the struct's Go-layout image, copies into or out of the window, and for a destination decodes the image back into the
/// box. Within that statement it is exactly Go's: the count <c>copy</c> returns, a partial copy leaving the bytes past it as
/// they were, and padding reading zero. The converter emits it only as a direct argument of <c>copy</c>; any other use of
/// the view keeps the order-token refusal by name.
/// </para>
/// <para>
/// Slicing narrows the window and touches nothing. The image is built per <c>copy</c> from a plan computed once per struct
/// type, in a stack buffer (a pooled one above 4 KiB): nothing is allocated on the GC heap per call.
/// </para>
/// </remarks>
public readonly struct GoImage<S>
{
    private readonly ж<S> m_box;
    private readonly nint m_low;
    private readonly nint m_high;

    private GoImage(ж<S> box, nint low, nint high)
    {
        m_box = box;
        m_low = low;
        m_high = high;
    }

    /// <summary>The whole image of <paramref name="box"/>'s struct, refused by name where the struct has none.</summary>
    public static GoImage<S> Of(ж<S>? box)
    {
        if (box is null || box.IsNilPointer)
            throw RuntimeErrorPanic.NilPointerDereference();

        NativeStructMarshal.ImagePlan<S> plan = NativeStructMarshal.ImagePlan<S>.Instance;

        if (plan.Refusal is not null)
            throw builtin.panic(plan.Refusal);

        return new GoImage<S>(box, 0, plan.Size);
    }

    /// <summary>The window's length in bytes, Go's <c>len</c> of the sliced view.</summary>
    public nint Length => m_high - m_low;

    /// <summary>Go's <c>p[:]</c>.</summary>
    public GoImage<S> slice() => this;

    /// <summary>Go's <c>p[low:]</c>.</summary>
    public GoImage<S> slice(nint low) => slice(low, Length);

    /// <summary>Go's <c>p[low:high]</c>.</summary>
    public GoImage<S> slice(nint low, nint high)
    {
        if ((nuint)high > (nuint)Length || (nuint)low > (nuint)high)
            throw RuntimeErrorPanic.LengthSliceBoundsOutOfRange(low, high, Length);

        return new GoImage<S>(m_box, m_low + low, m_low + high);
    }

    /// <summary>Go's <c>p[low:high:max]</c>. The capacity has no effect on a <c>copy</c>, so only its bounds are checked.</summary>
    public GoImage<S> slice(nint low, nint high, nint max)
    {
        if (low < 0 || high < low || max < high || max > Length)
            throw RuntimeErrorPanic.SliceBoundsOutOfRange(low, high, max, Length);

        return new GoImage<S>(m_box, m_low + low, m_low + high);
    }

    /// <summary>The converter's constant-bound form of a slice expression, <c>p[2..10]</c>.</summary>
    public GoImage<S> this[Range range]
    {
        get
        {
            nint low = range.Start.IsFromEnd ? Length - range.Start.Value : range.Start.Value;
            nint high = range.End.IsFromEnd ? Length - range.End.Value : range.End.Value;
            return slice(low, high);
        }
    }

    // copy(view, src): the image, src's first min(len, len) bytes into the window, the image decoded back into the box.
    internal unsafe nint CopyIn(ReadOnlySpan<byte> source)
    {
        nint count = Math.Min(Length, source.Length);

        if (count == 0)
            return 0;

        NativeStructMarshal.ImagePlan<S> plan = NativeStructMarshal.ImagePlan<S>.Instance;
        ref S value = ref m_box.ValueSlot;
        byte[]? rented = null;
        Span<byte> image = plan.Size <= StackImageLimit ? stackalloc byte[(int)plan.Size] : (rented = ArrayPool<byte>.Shared.Rent((int)plan.Size)).AsSpan(0, (int)plan.Size);

        try
        {
            image.Clear();
            plan.Encode(ref value, image);
            source[..(int)count].CopyTo(image.Slice((int)m_low, (int)count));
            plan.Decode(ref value, image);
            return count;
        }
        finally
        {
            if (rented is not null)
                ArrayPool<byte>.Shared.Return(rented);
        }
    }

    // copy(dst, view): the image, the window's first min(len, len) bytes into dst.
    internal unsafe nint CopyOut(Span<byte> destination)
    {
        nint count = Math.Min(Length, destination.Length);

        if (count == 0)
            return 0;

        NativeStructMarshal.ImagePlan<S> plan = NativeStructMarshal.ImagePlan<S>.Instance;
        ref S value = ref m_box.ValueSlot;
        byte[]? rented = null;
        Span<byte> image = plan.Size <= StackImageLimit ? stackalloc byte[(int)plan.Size] : (rented = ArrayPool<byte>.Shared.Rent((int)plan.Size)).AsSpan(0, (int)plan.Size);

        try
        {
            image.Clear();
            plan.Encode(ref value, image);
            image.Slice((int)m_low, (int)count).CopyTo(destination);
            return count;
        }
        finally
        {
            if (rented is not null)
                ArrayPool<byte>.Shared.Return(rented);
        }
    }

    // Above this the image is rented rather than stack-allocated. darwin's Dirent is 1048 bytes.
    private const int StackImageLimit = 4096;
}

public static partial class builtin
{
    /// <summary>Go's <c>copy(dst, src)</c> into a Go-layout byte view of a reference-bearing struct (<see cref="GoImage{S}"/>).</summary>
    public static nint copy<S>(GoImage<S> dst, in slice<byte> src) => dst.CopyIn(src.ToSpan());

    /// <summary>Go's <c>copy(dst, src)</c> out of a Go-layout byte view of a reference-bearing struct (<see cref="GoImage{S}"/>).</summary>
    public static nint copy<S>(in slice<byte> dst, GoImage<S> src) => src.CopyOut(dst.ToSpan());

    /// <summary>Go's <c>copy(dst, src)</c> of a string's bytes into a Go-layout byte view of a reference-bearing struct.</summary>
    public static nint copy<S>(GoImage<S> dst, in @string src) => dst.CopyIn(src.ToSpan());
}

public static unsafe partial class NativeStructMarshal
{
    /// <summary>
    /// How one struct type's Go-layout image is built from, and decoded back into, a value of it: computed once per type.
    /// </summary>
    /// <remarks>
    /// The fields are walked in Go's order at Go's offsets (<see cref="GoFieldOffsets"/>, the layout reflect reports), as the
    /// libc funnel's encode does, but each one is reached by its CLR offset inside the value rather than through
    /// <see cref="FieldInfo"/>, so no value is boxed per call. The CLR offsets are measured once, on a zero instance, by the
    /// address <see cref="TypedReference"/> gives each field while the instance is fixed. A fixed array is reached through
    /// <see cref="array{T}.ToSpan"/> with one closed accessor per Go scalar kind: no instantiation is made at run time,
    /// which a Native AOT image could not run (the Leaf note above). A struct with any reference field anywhere has no
    /// image and is refused by name; a shape this class cannot prove is refused too.
    /// </remarks>
    internal sealed class ImagePlan<S>
    {
        public static readonly ImagePlan<S> Instance = new();

        /// <summary>Go's size of the struct: the image's length.</summary>
        public readonly nint Size;

        /// <summary>Why the struct has no image, or null when it has one.</summary>
        public readonly string? Refusal;

        private readonly Segment[] m_segments = [];

        private readonly record struct Segment(nint GoOffset, nint ClrOffset, nint Size, IArrayImage? Array, nint Length);

        private ImagePlan()
        {
            Type type = typeof(S);

            if (!type.IsValueType || KindOf(type) != GoReflect.Struct || !TryGoSizeOf(type, null, out nuint size))
            {
                Refusal = refusal(type, "its Go layout is not known");
                return;
            }

            Size = (nint)size;
            System.Collections.Generic.List<Segment> segments = [];
            System.Collections.Generic.List<FieldInfo[]> paths = [];

            if (!plan(type, [], 0, segments, paths, out string? why))
            {
                Refusal = refusal(type, why!);
                return;
            }

            // A boxed zero S: the instance whose field addresses measure the CLR offsets.
            object zero = default(S)!;

            if (!measure(zero, segments, paths, out why))
            {
                Refusal = refusal(type, why!);
                return;
            }

            m_segments = [.. segments];
        }

        private static string refusal(Type type, string why) =>
            $"go2cs: copy through a byte view of a Go {GoTypeName(type)}: {why}, so it has no Go-layout image to copy " +
            "(docs/phase4/DESIGN-reinterpret-seam.md)";

        // The segments of `type` reached by `path` from S, at Go offset `goBase`; their CLR offsets are filled in by measure.
        private static bool plan(Type type, FieldInfo[] path, nint goBase, System.Collections.Generic.List<Segment> segments,
            System.Collections.Generic.List<FieldInfo[]> paths, out string? why)
        {
            GoFieldInfo[] fields = GoFields(type);
            nint[]? offsets = GoFieldOffsets(type);
            why = null;

            if (offsets is null)
            {
                why = "its Go field offsets are not known";
                return false;
            }

            for (int i = 0; i < fields.Length; i++)
            {
                GoFieldInfo field = fields[i];

                if (field.Path.Length != 1 || field.BoxHop[0])
                {
                    why = $"field {field.Name} is reached through an embed, which this view does not follow";
                    return false;
                }

                FieldInfo member = field.Path[0];
                FieldInfo[] memberPath = [.. path, member];
                nint goOffset = goBase + offsets[i];
                int kind = KindOf(field.Type);

                if (isReferenceKind(kind))
                {
                    why = $"field {field.Name} holds a managed reference (a pointer, string, slice, map, channel, func or interface)";
                    return false;
                }

                switch (kind)
                {
                    case GoReflect.Struct:
                        if (!field.Type.IsValueType || !plan(field.Type, memberPath, goOffset, segments, paths, out why))
                        {
                            why ??= $"field {field.Name} is a struct this view cannot lay out";
                            return false;
                        }

                        break;
                    case GoReflect.Array:
                        Type? element = ElementType(field.Type);

                        if (element is null || field.ArrayDims is not { Length: 1 } dims || ArrayImage.For(element) is not { } access ||
                            Leaf.Of(element) is not { } leaf || leaf.Size != access.ElementSize)
                        {
                            why = $"field {field.Name} is an array whose element has no raw image";
                            return false;
                        }

                        segments.Add(new Segment(goOffset, 0, leaf.Size * dims[0], access, dims[0]));
                        paths.Add(memberPath);
                        break;
                    default:
                        if (Leaf.Of(field.Type) is not { } scalar)
                        {
                            why = $"field {field.Name} has no raw image";
                            return false;
                        }

                        segments.Add(new Segment(goOffset, 0, scalar.Size, null, 0));
                        paths.Add(memberPath);
                        break;
                }
            }

            return true;
        }

        // Fills in each segment's CLR offset: the address TypedReference gives the field, less the instance's first byte,
        // read while the instance is fixed so neither can move between the two reads.
        private static bool measure(object zero, System.Collections.Generic.List<Segment> segments, System.Collections.Generic.List<FieldInfo[]> paths,
            out string? why)
        {
            why = null;

            {
                if (paths.Count != segments.Count)
                {
                    why = "its field walk did not settle";
                    return false;
                }

                fixed (byte* data = &RawDataOf(zero))
                {
                    for (int i = 0; i < segments.Count; i++)
                    {
                        TypedReference reference = TypedReference.MakeTypedReference(zero, paths[i]);
                        nint address = *(nint*)&reference;
                        nint clrOffset = address - (nint)data;
                        nint clrSize = ClrSizeOf(paths[i][^1].FieldType);

                        if (clrOffset < 0 || clrOffset + clrSize > ClrSizeOf(zero.GetType()))
                        {
                            why = $"field {paths[i][^1].Name} could not be located in the CLR value";
                            return false;
                        }

                        segments[i] = segments[i] with { ClrOffset = clrOffset };
                    }
                }

                return true;
            }
        }

        // The first byte of a boxed value type's data (CoreLib's RuntimeHelpers.GetRawData): a box is its type pointer
        // followed by the value.
        private sealed class RawBox
        {
            public byte Data;
        }

        private static ref byte RawDataOf(object box) => ref Unsafe.As<RawBox>(box).Data;

        /// <summary>Writes <paramref name="value"/>'s Go-layout image into the zeroed <paramref name="image"/>.</summary>
        public void Encode(ref S value, Span<byte> image)
        {
            ref byte data = ref Unsafe.As<S, byte>(ref value);

            foreach (Segment segment in m_segments)
            {
                Span<byte> target = image.Slice((int)segment.GoOffset, (int)segment.Size);

                if (segment.Array is { } array)
                    array.CopyOut(ref Unsafe.Add(ref data, segment.ClrOffset), target, segment.Length);
                else
                    MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref data, segment.ClrOffset), (int)segment.Size).CopyTo(target);
            }
        }

        /// <summary>Writes <paramref name="image"/> back into <paramref name="value"/>'s fields; padding is not stored.</summary>
        public void Decode(ref S value, ReadOnlySpan<byte> image)
        {
            ref byte data = ref Unsafe.As<S, byte>(ref value);

            foreach (Segment segment in m_segments)
            {
                ReadOnlySpan<byte> source = image.Slice((int)segment.GoOffset, (int)segment.Size);

                if (segment.Array is { } array)
                    array.CopyIn(ref Unsafe.Add(ref data, segment.ClrOffset), source, segment.Length);
                else
                    source.CopyTo(MemoryMarshal.CreateSpan(ref Unsafe.Add(ref data, segment.ClrOffset), (int)segment.Size));
            }
        }
    }

    /// <summary>One fixed-array field's elements as raw bytes, for a closed set of Go scalar element types.</summary>
    internal interface IArrayImage
    {
        nint ElementSize { get; }

        void CopyOut(ref byte field, Span<byte> image, nint length);

        void CopyIn(ref byte field, ReadOnlySpan<byte> image, nint length);
    }

    internal static class ArrayImage
    {
        // Closed over Go's scalar kinds: each instantiation is made here, statically, so a Native AOT image has its code.
        public static IArrayImage? For(Type element) =>
            element == typeof(byte) ? ArrayImage<byte>.Instance :
            element == typeof(sbyte) ? ArrayImage<sbyte>.Instance :
            element == typeof(bool) ? ArrayImage<bool>.Instance :
            element == typeof(short) ? ArrayImage<short>.Instance :
            element == typeof(ushort) ? ArrayImage<ushort>.Instance :
            element == typeof(int) ? ArrayImage<int>.Instance :
            element == typeof(uint) ? ArrayImage<uint>.Instance :
            element == typeof(long) ? ArrayImage<long>.Instance :
            element == typeof(ulong) ? ArrayImage<ulong>.Instance :
            element == typeof(nint) ? ArrayImage<nint>.Instance :
            element == typeof(nuint) ? ArrayImage<nuint>.Instance :
            element == typeof(float) ? ArrayImage<float>.Instance :
            element == typeof(double) ? ArrayImage<double>.Instance :
            null;
    }

    internal sealed class ArrayImage<E> : IArrayImage where E : unmanaged
    {
        public static readonly ArrayImage<E> Instance = new();

        public nint ElementSize => Unsafe.SizeOf<E>();

        public void CopyOut(ref byte field, Span<byte> image, nint length)
        {
            // A zero-value array with no storage reads as Go's zeros, which the cleared image already holds.
            Span<E> elements = Unsafe.As<byte, array<E>>(ref field).ToSpan();
            MemoryMarshal.AsBytes(elements[..(int)Math.Min(elements.Length, length)]).CopyTo(image);
        }

        public void CopyIn(ref byte field, ReadOnlySpan<byte> image, nint length)
        {
            ref array<E> array = ref Unsafe.As<byte, array<E>>(ref field);

            // A zero-value array with no storage gets its Go length before the bytes land.
            if (array.Length != length)
                array = new array<E>((int)length);

            image.CopyTo(MemoryMarshal.AsBytes(array.ToSpan()));
        }
    }
}
