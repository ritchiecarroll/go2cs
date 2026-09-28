// NativeStructMarshal.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static go.GoReflect;

// The leaf copiers are closed over runtime Types with MakeGenericMethod, as the rest of GoReflect's
// machinery is (GoReflect.MethodSets.cs carries the same set); the leaf types are unmanaged scalars.
#pragma warning disable IL2060
#pragma warning disable IL2071
#pragma warning disable IL3050

namespace go;

/// <summary>
/// Carries a Go struct that has no native image across a raw system call: encoded into a Go-layout
/// native buffer before the call, decoded back into its box after it.
/// </summary>
/// <remarks>
/// <para>
/// A Go struct holding a fixed array (<c>Termios</c>'s <c>Cc [19]uint8</c>) holds a managed
/// <c>array&lt;T&gt;</c> in go2cs, so its box has no pinnable storage and <c>ж&lt;T&gt; → uintptr</c>
/// answers the box's ORDER TOKEN rather than an address (ж.cs, the reference-bearing arm). The kernel
/// refuses the token, which is non-canonical by construction, with EFAULT. That is safe but wrong: it
/// is why go-isatty's <c>ioctl(TCGETS)</c> answered "not a terminal" and fatih/color printed no colors
/// on Linux. The standard library answers the same class with a hand-owned blittable mirror per
/// wrapper (fstat, fstatat, Uname), but a package converted with <c>-recurse</c> (golang.org/x/sys)
/// never gets one.
/// </para>
/// <para>
/// So the linux keystone (internal/runtime/syscall's Syscall6) asks this class whenever an argument
/// carries the token tag. The token is resolved through the provenance registry, and the box's Go
/// type decides:
/// </para>
/// <list type="bullet">
///   <item><description>
///     scalars, fixed arrays of scalars and nested structs of the same: ENCODED at Go's own field
///     offsets (<see cref="GoFieldOffsets"/>, the layout reflect reports) into a zeroed native buffer
///     of Go's size, the call runs on the buffer's address, and the buffer is DECODED back into the
///     box, success or failure, because the kernel may write a partial result. The Go types of
///     golang.org/x/sys ARE the kernel's (ztypes files are generated from its C headers), so Go's
///     layout is the kernel's layout.
///   </description></item>
///   <item><description>
///     a pointer, string, slice, map, channel, func, interface or unsafe.Pointer anywhere inside:
///     REFUSED BY NAME with a panic. Such a field's value is a managed reference with no native
///     meaning, and no encoding can give it one (an identity problem, not a layout one).
///   </description></item>
///   <item><description>
///     anything this class cannot prove (an embedded or wrapper-projected field, an array whose
///     length the type does not carry, a leaf whose managed size is not Go's): left alone, so the
///     kernel answers EFAULT exactly as it did before this class existed.
///   </description></item>
/// </list>
/// <para>
/// A token that does not RESOLVE, because its box was collected before the call or the number is a
/// token plus an offset, is also left alone and reads EFAULT. That is the retired keystone tether's
/// lesson (docs/phase4/DESIGN-syscall-pinning.md §7): a resolve can miss, so a miss must degrade to the
/// old answer, never to a write into memory the call does not own.
/// </para>
/// <para>
/// The collision question, measured (G, 2026-09-26): the keystone saw 16,882,986 arguments across
/// 2,813,823 calls in 109 linux test processes and not one carried the tag, while a token planted in
/// each process was seen 109 times of 109. Only code outside the standard library reaches this path.
/// </para>
/// </remarks>
public static unsafe class NativeStructMarshal
{
    /// <summary>The raw call the keystone runs once the arguments are final.</summary>
    public delegate (uintptr r1, uintptr r2, uintptr errno) Syscall6Invoker(uintptr num, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6);

    /// <summary>
    /// Whether any argument carries the managed-pointer-token tag. The keystone's fast path: six bit
    /// tests, and nothing else, for every call that passes none.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AnyToken(uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6)
    {
        return ManagedPointerTokens.IsTaggedToken((nuint)a1) || ManagedPointerTokens.IsTaggedToken((nuint)a2) ||
               ManagedPointerTokens.IsTaggedToken((nuint)a3) || ManagedPointerTokens.IsTaggedToken((nuint)a4) ||
               ManagedPointerTokens.IsTaggedToken((nuint)a5) || ManagedPointerTokens.IsTaggedToken((nuint)a6);
    }

    /// <summary>
    /// Runs <paramref name="invoke"/> with every token argument that resolves to a marshallable box
    /// replaced by the address of a Go-layout native copy, and decodes each copy back afterwards.
    /// </summary>
    public static (uintptr r1, uintptr r2, uintptr errno) Call(uintptr num, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6, Syscall6Invoker invoke)
    {
        Span<nuint> args = [(nuint)a1, (nuint)a2, (nuint)a3, (nuint)a4, (nuint)a5, (nuint)a6];
        return marshalCall(args, nilPointersAsZero: false, (num, invoke), &invokeSyscall6);
    }

    private static (uintptr, uintptr, uintptr) invokeSyscall6((uintptr num, Syscall6Invoker invoke) state, ReadOnlySpan<nuint> a) =>
        state.invoke(state.num, a[0], a[1], a[2], a[3], a[4], a[5]);

    /// <summary>
    /// The darwin libc funnel's form (<see cref="GoLibcCall.Call"/>): the same marshal over up to
    /// <see cref="GoLibcCall.MaxArgs"/> arguments, under the NIL-POINTER policy rather than the linux
    /// keystone's panic.
    /// </summary>
    /// <remarks>
    /// Every darwin syscall reaches libc through one funnel (syscall/darwin/syscall_darwin_impl.cs
    /// <c>call</c>), so a pointer to a reference-bearing Go struct -- Stat_t, Statfs_t, Timeval, Rusage,
    /// RawSockaddrAny and their kin -- reached libc as an order token and answered EFAULT (and, before
    /// field views tokenized, as the raw unpinned address of an auto-laid-out struct). COORD ruling
    /// 2026-09-27, option (b): a POINTER or unsafe.Pointer field that is NIL encodes as 0, which is what
    /// libc reads for a nil pointer; a NON-nil one, or any string, slice, map, channel, func or interface
    /// field, leaves the token in place so libc answers EFAULT, exactly as before -- never the linux
    /// panic, which would turn kevent's and sendmsg's errno into a crash. Pointer words are not decoded
    /// back: a nil stays nil. Two follow-ups are named here, each its own later sizing:
    /// Msghdr/Iovec as hand-own mirrors (their pointers are never nil in use), and Kevent_t.Udata, which
    /// the kernel never dereferences and returns verbatim, so a token there may round-trip the way Go's
    /// opaque pointer does.
    /// </remarks>
    internal static nuint CallLibc(nint fn, ReadOnlySpan<nuint> args, GoLibcErrnoRule rule, nint errnoReader, out nuint errno)
    {
        Span<nuint> copy = stackalloc nuint[args.Length];
        args.CopyTo(copy);

        (nuint r, nuint e) result = marshalCall(copy, nilPointersAsZero: true, (fn, rule, errnoReader), &invokeLibc);
        errno = result.e;
        return result.r;
    }

    private static (nuint, nuint) invokeLibc((nint fn, GoLibcErrnoRule rule, nint errnoReader) state, ReadOnlySpan<nuint> a)
    {
        nuint r = GoLibcCall.CallUnmarshalled(state.fn, a, state.rule, state.errnoReader, out nuint errno);
        return (r, errno);
    }

    /// <summary>Whether any of <paramref name="args"/> carries the managed-pointer-token tag.</summary>
    internal static bool AnyToken(ReadOnlySpan<nuint> args)
    {
        foreach (nuint arg in args)
        {
            if (ManagedPointerTokens.IsTaggedToken(arg))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Runs <paramref name="invoke"/> with every token argument that resolves to a marshallable box
    /// replaced by the address of a Go-layout native copy, and decodes each copy back afterwards.
    /// </summary>
    private static TResult marshalCall<TState, TResult>(Span<nuint> args, bool nilPointersAsZero, TState state, delegate*<TState, ReadOnlySpan<nuint>, TResult> invoke)
    {
        if (args.Length > MaxArguments)
            throw new ArgumentException($"go2cs: a marshalled call with {args.Length} arguments; the scratch holds {MaxArguments}", nameof(args));

        Span<nuint> original = stackalloc nuint[args.Length];
        args.CopyTo(original);
        Span<nint> buffers = stackalloc nint[args.Length];

        // Per-call scratch on the stack, not the heap: a raw `new T[n]` here would allocate on every
        // marshalled call and is exactly the raw backing form the NoUncountedBackingAllocations guard names.
        ArgumentBoxes boxes = default;
        ArgumentTypes types = default;

        buffers.Clear();

        try
        {
            for (int i = 0; i < args.Length; i++)
            {
                nuint number = args[i];

                if (!ManagedPointerTokens.IsTaggedToken(number))
                    continue;

                // The same token in two arguments is one box and must be one buffer, or the second
                // decode would overwrite the first's result with a stale copy.
                int earlier = sameTokenBefore(original, i);

                if (earlier >= 0)
                {
                    if (buffers[earlier] != 0)
                        args[i] = (nuint)buffers[earlier];

                    continue;
                }

                // A MISS (collected box, token arithmetic) keeps the token, and the kernel's EFAULT.
                if (ManagedPointerTokens.Resolve(number) is not { } box || PointeeTypeOfValue(box) is not { } type)
                    continue;

                Shape shape = shapeOf(type, nilPointersAsZero);

                if (shape.PointerField is not null)
                {
                    // Under the nil-pointer policy a reference field with no word image leaves the token.
                    if (nilPointersAsZero)
                        continue;

                    throw panic($"syscall: argument {i + 1} points to a Go {type.Name} whose field " +
                                $"{shape.PointerField} holds a managed reference (a pointer, string, slice, " +
                                "map, channel, func or interface) with no native meaning, so it cannot be " +
                                "handed to the kernel. Hand-own this wrapper against a blittable mirror.");
                }

                if (!shape.Marshallable)
                    continue;

                object value = ReadPointerSlot(box)!;

                // A pointer word crosses only as nil (0); a non-nil one keeps the token, and EFAULT.
                if (shape.HasPointerWords && !pointerWordsAreNil(value, type))
                    continue;

                nint buffer = (nint)NativeMemory.AlignedAlloc(shape.Size, 16);
                buffers[i] = buffer;
                NativeMemory.Clear((void*)buffer, shape.Size);

                encode(value, type, (byte*)buffer);

                boxes[i] = box;
                types[i] = type;
                args[i] = (nuint)buffer;
            }

            TResult result = invoke(state, args);

            for (int i = 0; i < args.Length; i++)
            {
                if (boxes[i] is not { } box)
                    continue;

                object value = ReadPointerSlot(box)!;
                decode(value, types[i]!, (byte*)buffers[i]);
                WritePointerSlot(box, value);
            }

            return result;
        }
        finally
        {
            for (int i = 0; i < buffers.Length; i++)
            {
                if (buffers[i] != 0)
                    NativeMemory.AlignedFree((void*)buffers[i]);
            }
        }
    }

    private static int sameTokenBefore(ReadOnlySpan<nuint> original, int index)
    {
        for (int i = 0; i < index; i++)
        {
            if (original[i] == original[index])
                return i;
        }

        return -1;
    }

    // ---- the shape: can this Go struct be encoded, and if not, why not ----

    /// <summary>
    /// A struct type's verdict: marshallable (with its Go size, and whether it carries pointer words that
    /// cross only as nil), pointer-bearing (naming the field), or neither.
    /// </summary>
    private sealed record Shape(bool Marshallable, nuint Size, string? PointerField, bool HasPointerWords = false);

    private static readonly Shape Unsupported = new(false, 0, null);

    private static readonly ConcurrentDictionary<Type, Shape> s_shapes = new();
    private static readonly ConcurrentDictionary<Type, Shape> s_nilPointerShapes = new();

    private static Shape shapeOf(Type type, bool nilPointersAsZero) => nilPointersAsZero ?
        s_nilPointerShapes.GetOrAdd(type, static t => classify(t, 0, nilPointersAsZero: true)) :
        s_shapes.GetOrAdd(type, static t => classify(t, 0, nilPointersAsZero: false));

    /// <summary>
    /// The native buffer size a token argument of Go struct type <paramref name="type"/> would get, or
    /// null when the type is refused or left alone. Exposed for the guard's test.
    /// </summary>
    internal static nuint? MarshalledSizeOf(Type type, bool nilPointersAsZero = false) =>
        shapeOf(type, nilPointersAsZero) is { Marshallable: true, Size: var size } ? size : null;

    // The widest caller is GoLibcCall.MaxArgs; the linux keystone passes six.
    private const int MaxArguments = 9;

    // A value type cannot contain itself (CS0523), so nesting depth is bounded by the declared types;
    // the cap only turns a misclassification into "unsupported" rather than a stack overflow.
    private const int MaxDepth = 32;

    private static Shape classify(Type type, int depth, bool nilPointersAsZero)
    {
        if (depth > MaxDepth || KindOf(type) != GoReflect.Struct || !TryGoSizeOf(type, null, out nuint size) || GoFieldOffsets(type) is not { } offsets)
            return Unsupported;

        GoFieldInfo[] fields = GoFields(type);

        // A LAST field of size zero (x/sys's `[0]uint8` flexible-array tails, InotifyEvent's Name) takes a
        // trailing byte in Go, so that a pointer to it cannot point past the struct: Go's sizeof is then
        // beyond that field's offset (InotifyEvent: offset 16, size 20). A layout that reports the size
        // AT the offset has omitted the byte, and a buffer of that size would let the kernel write past
        // its end, which is heap corruption, not EFAULT. Refused whatever the layout pass does, so this
        // holds before and after GoReflect's own trailing-byte fix.
        if (fields.Length > 0 && lastFieldIsZeroSize(fields[^1]) && size <= (nuint)offsets[^1])
            return Unsupported;

        bool hasPointerWords = false;

        foreach (GoFieldInfo field in fields)
        {
            // Only a plain field is read and written through its one FieldInfo; an embed's box hop or
            // a defined-type wrapper's descent is a path this class does not follow.
            if (field.Path.Length != 1 || field.BoxHop[0])
                return Unsupported;

            int kind = KindOf(field.Type);

            if (isReferenceKind(kind))
            {
                // Under the nil-pointer policy a pointer or unsafe.Pointer is one machine word that crosses
                // as 0 when nil (checked per call); every other reference kind still has no image.
                if (nilPointersAsZero && kind is GoReflect.Pointer or GoReflect.UnsafePointer)
                {
                    hasPointerWords = true;
                    continue;
                }

                return new Shape(false, 0, field.Name);
            }

            switch (kind)
            {
                case GoReflect.Struct:
                    Shape nested = classify(field.Type, depth + 1, nilPointersAsZero);

                    if (nested.PointerField is not null)
                        return new Shape(false, 0, field.Name + "." + nested.PointerField);

                    if (!nested.Marshallable)
                        return Unsupported;

                    hasPointerWords |= nested.HasPointerWords;
                    break;
                case GoReflect.Array:
                    Type? element = ElementType(field.Type);

                    if (element is null || isReferenceKind(KindOf(element)))
                        return element is null ? Unsupported : new Shape(false, 0, field.Name + "[]");

                    if (field.ArrayDims is not { Length: 1 } || Leaf.Of(element) is null)
                        return Unsupported;

                    break;
                default:
                    if (Leaf.Of(field.Type) is null)
                        return Unsupported;

                    break;
            }
        }

        return new Shape(true, size, null, hasPointerWords);
    }

    // Whether every pointer word the nil-pointer policy admitted is nil in this value, nested structs
    // included: a Go nil pointer is a C# null or a nil-constructed box (INilPointer).
    private static bool pointerWordsAreNil(object structValue, Type type)
    {
        foreach (GoFieldInfo field in GoFields(type))
        {
            switch (KindOf(field.Type))
            {
                case GoReflect.Pointer:
                case GoReflect.UnsafePointer:
                    if (field.Read(structValue) is not (null or INilPointer { IsNilPointer: true }))
                        return false;

                    break;
                case GoReflect.Struct:
                    if (!pointerWordsAreNil(field.Read(structValue)!, field.Type))
                        return false;

                    break;
            }
        }

        return true;
    }

    private static bool lastFieldIsZeroSize(GoFieldInfo field) =>
        TryGoSizeOf(field.Type, KindOf(field.Type) == GoReflect.Array ? field.ArrayDims : null, out nuint size) && size == 0;

    // Per-call scratch for marshalCall(), held on the stack: MaxArguments slots.
    [InlineArray(MaxArguments)]
    private struct ArgumentBoxes
    {
        private object? m_element;
    }

    [InlineArray(MaxArguments)]
    private struct ArgumentTypes
    {
        private Type? m_element;
    }

    private static bool isReferenceKind(int kind) =>
        kind is GoReflect.Pointer or GoReflect.UnsafePointer or GoReflect.String or GoReflect.Slice or GoReflect.Map or GoReflect.Chan or GoReflect.Func or GoReflect.Interface;

    // ---- encode / decode, walking Go's own field order and offsets ----

    private static void encode(object structValue, Type type, byte* destination)
    {
        GoFieldInfo[] fields = GoFields(type);
        nint[] offsets = GoFieldOffsets(type)!;

        for (int i = 0; i < fields.Length; i++)
        {
            GoFieldInfo field = fields[i];
            byte* at = destination + offsets[i];
            object? value = field.Read(structValue);

            switch (KindOf(field.Type))
            {
                case GoReflect.Struct:
                    encode(value!, field.Type, at);
                    break;
                case GoReflect.Pointer:
                case GoReflect.UnsafePointer:
                    // Nil by the per-call check: the zeroed buffer already holds its 0.
                    break;
                case GoReflect.Array:
                    Leaf element = Leaf.Of(ElementType(field.Type)!)!;
                    IArray array = (IArray)value!;

                    for (nint index = 0; index < array.Length; index++)
                        element.Write(array[index]!, at + index * element.Size);

                    break;
                default:
                    Leaf.Of(field.Type)!.Write(value!, at);
                    break;
            }
        }
    }

    // Decodes INTO the boxed struct value in place: a scalar through its FieldInfo, an array element
    // through the array's own storage (which the value shares with the box), a nested struct into its
    // own boxed copy that is then set back.
    private static void decode(object structValue, Type type, byte* source)
    {
        GoFieldInfo[] fields = GoFields(type);
        nint[] offsets = GoFieldOffsets(type)!;

        for (int i = 0; i < fields.Length; i++)
        {
            GoFieldInfo field = fields[i];
            byte* at = source + offsets[i];
            FieldInfo member = field.Path[0];

            switch (KindOf(field.Type))
            {
                case GoReflect.Struct:
                    object nested = member.GetValue(structValue)!;
                    decode(nested, field.Type, at);
                    member.SetValue(structValue, nested);
                    break;
                case GoReflect.Pointer:
                case GoReflect.UnsafePointer:
                    // A pointer word has no managed image to decode into: the nil the call carried stays.
                    break;
                case GoReflect.Array:
                    Leaf element = Leaf.Of(ElementType(field.Type)!)!;
                    IArray array = (IArray)member.GetValue(structValue)!;

                    for (nint index = 0; index < array.Length; index++)
                        array[index] = element.Read(at + index * element.Size);

                    break;
                default:
                    member.SetValue(structValue, Leaf.Of(field.Type)!.Read(at));
                    break;
            }
        }
    }

    // ---- a leaf: one unmanaged value whose managed size IS its Go size, copied as raw bytes ----

    private sealed class Leaf
    {
        public required nint Size { get; init; }
        public required Action<object, nint> WriteRaw { get; init; }
        public required Func<nint, object> ReadRaw { get; init; }

        public void Write(object value, byte* destination) => WriteRaw(value, (nint)destination);

        public object Read(byte* source) => ReadRaw((nint)source);

        private static readonly ConcurrentDictionary<Type, Leaf?> s_leaves = new();

        // A scalar kind (a named Go type over a scalar included) whose managed type holds no reference
        // and occupies exactly Go's size; anything else has no raw image to copy.
        public static Leaf? Of(Type type) => s_leaves.GetOrAdd(type, static t =>
        {
            if (KindOf(t) is < GoReflect.Bool or > GoReflect.Complex128 || !t.IsValueType || containsReferences(t))
                return null;

            nint goSize = GoSizeOf(t);

            if (goSize <= 0 || sizeOf(t) != goSize)
                return null;

            MethodInfo write = typeof(Leaf).GetMethod(nameof(write_), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(t);
            MethodInfo read = typeof(Leaf).GetMethod(nameof(read_), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(t);

            return new Leaf
            {
                Size = goSize,
                WriteRaw = write.CreateDelegate<Action<object, nint>>(),
                ReadRaw = read.CreateDelegate<Func<nint, object>>()
            };
        });

        private static void write_<T>(object value, nint destination) where T : unmanaged =>
            Unsafe.WriteUnaligned((void*)destination, (T)value);

        private static object read_<T>(nint source) where T : unmanaged =>
            Unsafe.ReadUnaligned<T>((void*)source);

        private static bool containsReferences(Type t) =>
            (bool)typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.IsReferenceOrContainsReferences))!.MakeGenericMethod(t).Invoke(null, null)!;

        private static int sizeOf(Type t) =>
            (int)typeof(Unsafe).GetMethod(nameof(Unsafe.SizeOf))!.MakeGenericMethod(t).Invoke(null, null)!;
    }
}
