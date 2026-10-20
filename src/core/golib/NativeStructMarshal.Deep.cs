// NativeStructMarshal.Deep.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using static go.GoReflect;

#pragma warning disable IL2060
#pragma warning disable IL2071
#pragma warning disable IL3050

namespace go;

public static unsafe partial class NativeStructMarshal
{
    /// <summary>
    /// The windows funnel's form: every token argument crosses as a DEEP, identity-mapped Go-layout native
    /// copy -- pointer fields included -- and every copy is decoded back after the call.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Windows hands a system DLL whatever pointer Go hands it, and Go 1.26 added callers whose pointee is
    /// reference-bearing in C#: <c>WSADuplicateSocket</c>'s <c>*WSAProtocolInfo</c> (fixed-array fields) under
    /// <c>net.FileConn</c>, and <c>SetEntriesInAcl</c>'s <c>*EXPLICIT_ACCESS</c> (a pointer field, read for
    /// <c>count</c> entries) and <c>**ACL</c> (an OUT word). The funnel refused every such token by name; this
    /// carries them, under four rules:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>
    ///     COPY ONLY WHAT HAS NO ADDRESS. A pointer field whose pointee converts to a real, pinned address
    ///     (a blittable struct, a byte buffer's <c>&amp;b[0]</c>) passes that address: the callee reads and
    ///     writes the caller's own memory exactly as in Go, which overlapped I/O depends on, since it writes
    ///     the buffer after the call returns. Only a pointee that answers an order TOKEN is copied.
    ///   </description></item>
    ///   <item><description>
    ///     A COPY IS DEEP AND IDENTITY-MAPPED. Each token pointee is encoded once per call at Go's layout; a
    ///     second pointer to it (an alias, or a cycle) stores the same native address. A token that names an
    ///     ELEMENT of an array or slice copies the contiguous run from that element to the end of its backing,
    ///     because a callee given <c>&amp;entries[0]</c> and a count reads <c>count</c> entries.
    ///   </description></item>
    ///   <item><description>
    ///     EVERY COPY COMES BACK. After the call, success or failure, each copy is decoded into the storage it
    ///     came from. A pointer word the callee CHANGED decodes as the address it wrote: nil for 0, the copied
    ///     box when it names a copy, and otherwise a native-address box (<see cref="NativeBox{T}"/>, the
    ///     kernel-returned kind), so an OUT word such as <c>SetEntriesInAcl</c>'s <c>newACL</c> reaches the
    ///     caller's <c>LocalFree</c> as the real address.
    ///   </description></item>
    ///   <item><description>
    ///     WHAT HAS NO NATIVE IMAGE IS REFUSED BY NAME: a string, slice, map, channel, func, interface or
    ///     non-nil unsafe.Pointer anywhere inside a copied pointee, a layout this class cannot prove, and a box
    ///     that is another VIEW (a field view beside its struct's box, either order) of storage already copied.
    ///   </description></item>
    /// </list>
    /// <para>
    /// PRECONDITION: the callee does not RETAIN a copied pointee past the call. Every copy is freed when the
    /// call returns, so a callee that keeps the pointer would write freed memory. That holds for every
    /// funnel call site the census found reaching here (2026-10-16: 19 live pointer arguments in auto-converted
    /// wrappers). A callee known to retain one is refused by name before any copy is made: the caller passes
    /// that check (the windows funnel keeps the list, keyed by proc name, empty today).
    /// </para>
    /// <para>
    /// PIN LIFETIME. A pointer field's pinned address (the first rule) is the box's own pin, the one every
    /// <c>uintptr(unsafe.Pointer(p))</c> takes: registered at the pin moment and held for the box's life, not
    /// released at return. That is what overlapped I/O needs, since the kernel writes the buffer after the call.
    /// <c>WSARecv</c> and <c>WSASend</c> never reach this form: they are hand-owned
    /// (<c>zsyscall_windows_wsa_impl.cs</c>), staging native WSABUFs and an OVERLAPPED whose operation pins
    /// each <c>Buf</c> until the completion is harvested, so every argument they pass is already an address.
    /// </para>
    /// </remarks>
    public static TResult CallDeep<TState, TResult>(ReadOnlySpan<nuint> args, TState state, delegate*<TState, ReadOnlySpan<nuint>, TResult> invoke)
    {
        Span<nuint> marshalled = stackalloc nuint[args.Length];
        args.CopyTo(marshalled);

        DeepCall call = new();

        try
        {
            for (int i = 0; i < marshalled.Length; i++)
            {
                if (ManagedPointerTokens.IsTaggedToken(marshalled[i]))
                    marshalled[i] = call.AddressOf(marshalled[i], $"argument {i}");
            }

            TResult result = invoke(state, marshalled);
            call.DecodeAll();
            return result;
        }
        finally
        {
            call.FreeAll();
        }
    }

    /// <summary>Whether a token argument of Go type <paramref name="type"/> can cross the deep form (the guard's probe).</summary>
    internal static string? DeepRefusal(Type type) => deepRefusal(type, 0, new HashSet<Type>());

    /// <summary>
    /// The outermost value at <paramref name="box"/>'s Go address that the deep form can copy: what a token for that
    /// address is copied as.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A FIRST field's view and its struct's box are one address in Go, so they share one order token, and the token
    /// resolves to whichever was converted last: &amp;s and &amp;s.f passed together could both resolve to the narrower
    /// f, and a callee reading s from the first would read past f's copy. The token names the outermost value at that
    /// address, so that is what is copied (f lies at offset 0 inside it); a parent with no native image keeps the
    /// narrower view, which is then all the caller can have meant. A REINTERPRET view is a hop at Go offset 0 too, so
    /// <c>(*byte)(unsafe.Pointer(&amp;rdb.header))</c> widens to rdb, whose bytes a callee such as DeviceIoControl
    /// reads past the header (FieldRefBox's StorageKind, the reinterpret-of-a-first-field exception, decides when such a
    /// view crosses as a token at all).
    /// </para>
    /// <para>
    /// A WIDENED copy is the whole root, and the decode after the call writes the whole root back: a field of it the
    /// callee did not touch is rewritten with the value it had when the call began, so a write another goroutine makes
    /// to a sibling field DURING the call is lost. Go has no such window. Every caller of this shape today passes a
    /// call-local root (os_windows_test.go's createDirLink, whose rdb lives in createMountPoint or createSymbolicLink).
    /// </para>
    /// </remarks>
    internal static object WidestAtSameAddress(object box)
    {
        while (box is IFirstFieldView { FirstFieldParent: IGoReflectBox parent } &&
               PointeeTypeOfValue(parent) is { } parentType && HasDeepImage(parentType))
        {
            box = parent;
        }

        return box;
    }

    // DeepRefusal walks the type and allocates; the widening walk asks on every ж -> uintptr of a reinterpret view, so
    // the answer, a pure function of the type, is kept per type.
    private static bool HasDeepImage(Type type) => s_hasDeepImage.GetOrAdd(type, static t => DeepRefusal(t) is null);

    private static readonly ConcurrentDictionary<Type, bool> s_hasDeepImage = new();

    // Null when every field of the type has a native image under the deep rules, else the path of the first
    // field that does not. Pointer fields are not followed here: their pointees are judged per value, when
    // the walk reaches them, which is also how a self-referencing type terminates.
    private static string? deepRefusal(Type type, int depth, HashSet<Type> active)
    {
        if (depth > MaxDepth)
            return type.Name;

        int kind = KindOf(type);

        switch (kind)
        {
            case GoReflect.Pointer:
                return pointerFieldElement(type) is null ? type.Name : null;
            case GoReflect.Array:
                Type? element = ElementType(type);
                return element is null ? type.Name : deepRefusal(element, depth + 1, active) is { } inner ? "[]" + inner : null;
            case GoReflect.Struct:
                break;
            default:
                return Leaf.Of(type) is null ? type.Name : null;
        }

        if (!active.Add(type))
            return null;

        try
        {
            if (!TryGoSizeOf(type, null, out _) || GoFieldOffsets(type) is null)
                return type.Name;

            foreach (GoFieldInfo field in GoFields(type))
            {
                if (field.Path.Length != 1 || field.BoxHop[0])
                    return field.Name;

                int fieldKind = KindOf(field.Type);

                // unsafe.Pointer has no box to follow: it crosses only as nil, checked per value.
                if (fieldKind == GoReflect.UnsafePointer)
                    continue;

                if (isReferenceKind(fieldKind) && fieldKind != GoReflect.Pointer)
                    return field.Name;

                if (fieldKind == GoReflect.Array && field.ArrayDims is not { Length: 1 })
                    return field.Name;

                if (deepRefusal(field.Type, depth + 1, active) is { } inner)
                    return field.Name + "." + inner;
            }

            return null;
        }
        finally
        {
            active.Remove(type);
        }
    }

    // The pointee type of a pointer FIELD the deep form can write back into: only a plain ж<E>, which a
    // native-address box of the same E can be stored in. A generated named-pointer wrapper is not.
    private static Type? pointerFieldElement(Type pointerType) =>
        pointerType.IsGenericType && pointerType.BaseType is not null && typeof(IGoReflectBox).IsAssignableFrom(pointerType) &&
        pointerType.IsAbstract && pointerType.GetGenericTypeDefinition() == typeof(ж<>) ? pointerType.GetGenericArguments()[0] : null;

    /// <summary>One native copy: a box's pointee, or a run of elements of one backing array.</summary>
    private sealed class DeepCopy
    {
        public required Type Type { get; init; }
        public required nint Buffer { get; init; }
        public required nuint Stride { get; init; }
        public required string Where { get; init; }

        // Exactly one of the two sources: a box (Count == 1), or a backing array and the run's first index.
        public object? Box { get; init; }
        public Array? Backing { get; init; }
        public int Start { get; init; }
        public int Count { get; init; } = 1;

        public nuint Size => Stride * (nuint)Count;
    }

    /// <summary>A pointer word the encode wrote: where it lives in native memory and what it held.</summary>
    private readonly record struct WordSlot(nuint Written);

    /// <summary>The per-call state: the identity map, the copies in encode order, and every word written.</summary>
    private sealed class DeepCall
    {
        private readonly Dictionary<object, DeepCopy> m_byBox = new(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<Array, DeepCopy> m_byBacking = new(ReferenceEqualityComparer.Instance);

        // The storage each copy was taken from (ManagedPointerTokens.AllocationIdOf, which every view of one
        // allocation shares), so a second box that is a VIEW of storage already copied is caught: two
        // independent copies of one storage would come back last-writer-wins.
        private readonly Dictionary<ulong, DeepCopy> m_byAllocation = new();
        private readonly List<DeepCopy> m_copies = new();

        // A native word's address -> the value the encode wrote there, so the decode can tell a word the
        // callee changed from one it left alone.
        private readonly Dictionary<nint, nuint> m_words = new();

        /// <summary>
        /// The native address a token stands for in this call: its copy's buffer (created on first sight),
        /// offset for an element inside a copied run.
        /// </summary>
        public nuint AddressOf(nuint token, string where)
        {
            if (ManagedPointerTokens.Resolve(token) is not { } box)
            {
                throw panic($"syscall: {where} is a managed pointer token that no longer resolves to a live " +
                            "value, so it has no native image to pass");
            }

            box = WidestAtSameAddress(box);

            if (m_byBox.TryGetValue(box, out DeepCopy? known))
                return (nuint)known.Buffer;

            if (box is IElementRun run)
            {
                if (run.RunBacking is not { } backing)
                {
                    throw panic($"syscall: {where} points to an element of a collection with no contiguous " +
                                "backing array, so the run of elements a native callee reads from it has no copy");
                }

                return addressInRun(box, backing, (int)run.RunIndex, where);
            }

            if (PointeeTypeOfValue(box) is not { } type)
                throw panic($"syscall: {where} is a managed pointer token whose pointee type cannot be read");

            refuseUnlessDeep(type, where);

            ulong allocation = ManagedPointerTokens.AllocationIdOf(box);
            refuseViewAlias(allocation, where);

            nuint stride = (nuint)GoSizeOf(type);
            DeepCopy copy = allocate(new DeepCopy { Type = type, Buffer = 0, Stride = stride, Where = where, Box = box }, stride);
            m_byBox[box] = copy;
            m_byAllocation[allocation] = copy;

            object? value = ReadPointerSlot(box);
            encodeValue(value, type, (byte*)copy.Buffer);

            return (nuint)copy.Buffer;
        }

        private nuint addressInRun(object box, Array backing, int index, string where)
        {
            if (m_byBacking.TryGetValue(backing, out DeepCopy? run))
            {
                // An alias into a run already copied: the same native element.
                if (index >= run.Start && index < run.Start + run.Count)
                    return (nuint)run.Buffer + (nuint)(index - run.Start) * run.Stride;

                throw panic($"syscall: {where} points to element {index} of an array whose run from element " +
                            $"{run.Start} is already being passed; an earlier element cannot join it");
            }

            Type type = backing.GetType().GetElementType()!;
            refuseUnlessDeep(type, where);

            ulong allocation = ManagedPointerTokens.AllocationIdOf(box);
            refuseViewAlias(allocation, where);

            nuint stride = (nuint)GoSizeOf(type);
            int count = backing.Length - index;
            DeepCopy copy = allocate(new DeepCopy { Type = type, Buffer = 0, Stride = stride, Where = where, Backing = backing, Start = index, Count = count }, stride * (nuint)count);

            m_byBacking[backing] = copy;
            m_byBox[box] = copy;
            m_byAllocation[allocation] = copy;

            for (int i = 0; i < count; i++)
                encodeValue(backing.GetValue(index + i), type, (byte*)copy.Buffer + (nuint)i * stride);

            return (nuint)copy.Buffer;
        }

        // A box that is a different VIEW of storage already copied (a field view beside its struct's own box,
        // either order, or two field views of one struct): refused by name rather than copied twice.
        private void refuseViewAlias(ulong allocation, string where)
        {
            if (m_byAllocation.TryGetValue(allocation, out DeepCopy? earlier))
            {
                throw panic($"syscall: {where} aliases {earlier.Where}'s storage (a field or element view of " +
                            "the same Go value), so the two cannot be copied to native memory independently");
            }
        }

        private static void refuseUnlessDeep(Type type, string where)
        {
            if (DeepRefusal(type) is { } field)
            {
                throw panic($"syscall: {where} points to a Go {type.Name} whose field {field} has no native " +
                            "image (a string, slice, map, channel, func or interface, or a layout that cannot " +
                            "be proven), so it cannot be passed to native code");
            }
        }

        private DeepCopy allocate(DeepCopy shape, nuint size)
        {
            nuint bytes = size == 0 ? 1 : size;
            nint buffer = (nint)NativeMemory.AlignedAlloc(bytes, 16);
            NativeMemory.Clear((void*)buffer, bytes);

            DeepCopy copy = new()
            {
                Type = shape.Type, Buffer = buffer, Stride = shape.Stride, Where = shape.Where,
                Box = shape.Box, Backing = shape.Backing, Start = shape.Start, Count = shape.Count
            };

            m_copies.Add(copy);
            return copy;
        }

        // ---- encode ----

        private void encodeValue(object? value, Type type, byte* at)
        {
            switch (KindOf(type))
            {
                case GoReflect.Struct:
                    encodeStruct(value!, type, at);
                    break;
                case GoReflect.Array:
                    Type element = ElementType(type)!;
                    nuint stride = (nuint)GoSizeOf(element);
                    IArray array = (IArray)value!;

                    for (nint index = 0; index < array.Length; index++)
                        encodeValue(array[index], element, at + (nuint)index * stride);

                    break;
                case GoReflect.Pointer:
                    encodeWord(value, at);
                    break;
                default:
                    Leaf.Of(type)!.Write(value!, at);
                    break;
            }
        }

        private void encodeStruct(object structValue, Type type, byte* destination)
        {
            GoFieldInfo[] fields = GoFields(type);
            nint[] offsets = GoFieldOffsets(type)!;

            for (int i = 0; i < fields.Length; i++)
            {
                GoFieldInfo field = fields[i];
                object? value = field.Read(structValue);

                if (KindOf(field.Type) == GoReflect.UnsafePointer)
                {
                    if (value is not (null or INilPointer { IsNilPointer: true }))
                    {
                        throw panic($"syscall: the Go {type.Name} passed to native code holds a non-nil " +
                                    $"unsafe.Pointer in field {field.Name}, which has no native image");
                    }

                    continue;
                }

                encodeValue(value, field.Type, destination + offsets[i]);
            }
        }

        // A pointer word: 0 for nil, the pinned address for a pointee that has one, and the copy's buffer for
        // a token pointee (recursively, through the identity map).
        private void encodeWord(object? pointer, byte* at)
        {
            nuint word = 0;

            if (pointer is IGoReflectBox { IsNilPointer: false } box)
            {
                word = box.NativeArgumentAddress();

                if (ManagedPointerTokens.IsTaggedToken(word))
                    word = AddressOf(word, "a pointer field");
            }

            *(nuint*)at = word;
            m_words[(nint)at] = word;
        }

        // ---- decode ----

        public void DecodeAll()
        {
            foreach (DeepCopy copy in m_copies)
            {
                if (copy.Backing is { } backing)
                {
                    for (int i = 0; i < copy.Count; i++)
                    {
                        int index = copy.Start + i;
                        backing.SetValue(decodeValue(backing.GetValue(index), copy.Type, (byte*)copy.Buffer + (nuint)i * copy.Stride), index);
                    }

                    continue;
                }

                object box = copy.Box!;
                WritePointerSlot(box, decodeValue(ReadPointerSlot(box), copy.Type, (byte*)copy.Buffer));
            }
        }

        // Answers the decoded value: a struct is decoded into its boxed copy in place and returned for the
        // caller to store; an array is decoded through its own storage.
        private object? decodeValue(object? value, Type type, byte* at)
        {
            switch (KindOf(type))
            {
                case GoReflect.Struct:
                    GoFieldInfo[] fields = GoFields(type);
                    nint[] offsets = GoFieldOffsets(type)!;

                    for (int i = 0; i < fields.Length; i++)
                    {
                        GoFieldInfo field = fields[i];

                        if (KindOf(field.Type) == GoReflect.UnsafePointer)
                            continue;

                        FieldInfo member = field.Path[0];
                        member.SetValue(value, decodeValue(member.GetValue(value), field.Type, at + offsets[i]));
                    }

                    return value;
                case GoReflect.Array:
                    Type element = ElementType(type)!;
                    nuint stride = (nuint)GoSizeOf(element);
                    IArray array = (IArray)value!;

                    for (nint index = 0; index < array.Length; index++)
                        array[index] = decodeValue(array[index], element, at + (nuint)index * stride);

                    return value;
                case GoReflect.Pointer:
                    return decodeWord(value, type, at);
                default:
                    return Leaf.Of(type)!.Read(at);
            }
        }

        private object? decodeWord(object? pointer, Type pointerType, byte* at)
        {
            nuint now = *(nuint*)at;

            // Unchanged: the managed pointer still names what the word was built from (a copied pointee comes
            // back through its own copy's decode).
            if (m_words.TryGetValue((nint)at, out nuint written) && written == now)
                return pointer;

            if (now == 0)
                return null;

            // The callee pointed the word at one of this call's copies. The start of a box's copy is that box;
            // anywhere else in a copy (an element of a run, an interior field) is memory freed at return, so a
            // native box over it would dangle: refused by name rather than decoded.
            foreach (DeepCopy copy in m_copies)
            {
                if (now < (nuint)copy.Buffer || now >= (nuint)copy.Buffer + copy.Size)
                    continue;

                if (copy.Box is { } box && now == (nuint)copy.Buffer)
                    return box;

                throw panic($"syscall: native code left a pointer word naming the inside of a Go {copy.Type.Name} " +
                            "copy, which is freed when the call returns");
            }

            return NewNativeBox(pointerFieldElement(pointerType)!, now);
        }

        public void FreeAll()
        {
            foreach (DeepCopy copy in m_copies)
                NativeMemory.AlignedFree((void*)copy.Buffer);

            m_copies.Clear();
        }
    }

    // A native-address box over an element type known only at run time (the kernel-returned kind).
    private static object NewNativeBox(Type element, nuint address) =>
        typeof(NativeStructMarshal).GetMethod(nameof(newNativeBox), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(element).Invoke(null, [address])!;

    private static object newNativeBox<T>(nuint address) => new NativeBox<T>(address);
}

/// <summary>
/// An element pointer's backing array and absolute index, when it has one (the fast arm of
/// <see cref="ElemRefBox{T}"/>): what the deep marshal copies a contiguous run from.
/// </summary>
internal interface IElementRun
{
    Array? RunBacking { get; }

    nint RunIndex { get; }
}
