// CgoPointerWords.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

namespace go.golib;

/// <summary>
/// The word core of a C pointer slot: a word of C memory, or of Go memory a pin holds still, that holds a C address where
/// the CLR would hold a managed reference.
/// </summary>
/// <remarks>
/// cgo Phase 3, increment 3.2 (docs/phase4/DESIGN-cgo-phase3-marshaling.md 18.9 and 18.11). One protocol, two callers:
/// the pointer-field accessor of a C struct (part 1a, <see cref="ж{T}.cgoPointerField{U}(FieldRefFunc{T, ж{U}})"/>)
/// and the <c>**C.T</c> load and store (part 1c, <see cref="GoCgoArgs.LoadPointer{U}"/> and
/// <see cref="GoCgoArgs.StorePointer{U}"/>).
/// </remarks>
internal static class CgoPointerWords
{
    // The refusals, each by name. A load and a store over C memory take S-B1's wording (unsafe.cs: LoadFromCText,
    // StoreIntoCText); a C struct laid over Go memory has texts of its own, since that memory is Go's; and a slot over Go
    // storage that no pin holds still is S-B1's UnheldSlotText, word for word.
    private const string LoadFromCText = "cgo struct field load: a Go pointer read out of C memory is refused (C memory must not hold a Go pointer)";
    private const string StoreIntoCText = "cgo struct field store: storing a Go pointer into C memory is refused (C memory must not hold a Go pointer)";
    private const string LoadOverGoText = "cgo struct field load: a Go pointer in a C struct laid over Go memory is not read in 3.2 (go2cs keeps no record of the pointer stored there)";
    private const string StoreOverGoText = "cgo struct field store: a Go pointer stored into a C struct laid over Go memory is not supported in 3.2";
    private const string UnheldText = "unsafe.Pointer word: the slot is a native box over Go storage that nothing holds still";

    // C memory alone that names no address. Unreachable for a non-nil value, so no row can construct it: a native box
    // over C memory alone has a non-zero address (a zero box is nil, taken above), and an element of a native-backed
    // slice has its element's address, a slice over native memory refusing a nil base (slice.OverNativeMemory). It is
    // named rather than read as a Go pointer, whose text would name the wrong cause.
    private const string NoAddressText = "cgo pointer store: a pointer to C memory that names no address";

    // Which memory the slot is in (GoPointerWords.Classify): C memory alone, or Go memory a pin holds still. Unheld is
    // refused, for a read and a store alike, and a native box at address 0 is Go's nil dereference.
    private static GoPointerWords.SlotKind Kind<U>(NativeBox<ж<U>> slot) =>
        GoPointerWords.Classify(slot) switch
        {
            GoPointerWords.SlotKind.CMemory => GoPointerWords.SlotKind.CMemory,
            GoPointerWords.SlotKind.GoMemory => GoPointerWords.SlotKind.GoMemory,
            GoPointerWords.SlotKind.Unheld => throw RuntimeErrorPanic.RuntimeRaised(UnheldText),
            _ => throw RuntimeErrorPanic.NilPointerDereference()
        };

    // The read: 0 is nil, a word Go memory names is refused, and any other word is C memory.
    internal static unsafe ж<U> ReadWord<U>(NativeBox<ж<U>> slot)
    {
        GoPointerWords.SlotKind kind = Kind(slot);
        nuint word = *(nuint*)slot.NativeAddress;

        if (word == 0)
            return null!;

        if (GoPointerWords.NamesGoMemory(word))
            throw RuntimeErrorPanic.RuntimeRaised(kind == GoPointerWords.SlotKind.CMemory ? LoadFromCText : LoadOverGoText);

        // A fresh box per read. Go's == on two reads compares the address (NativeBox's Equals reads the native identity),
        // so l.next == l.next holds, but the two reads are not one CLR object: a ReferenceEquals site would tell them
        // apart. The `**C.T` load (part 1c) is this read.
        return new NativeBox<U>(word);
    }

    // The store: 0 for nil, the address of a pointer to C memory alone, and a Go pointer refused. C memory alone is
    // whatever box names it (GoPointerWords.IsCMemoryAlone): a native box over C memory, or an element of a native-backed
    // slice that keeps no anchor (ж.ElemRefBox.cs, 3.1's C2). Its address is the box's native identity.
    internal static unsafe void WriteWord<U>(NativeBox<ж<U>> slot, ж<U> value)
    {
        GoPointerWords.SlotKind kind = Kind(slot);
        nuint word;

        if (value is null || value.IsNilPointer)
            word = 0;
        else if (GoPointerWords.IsCMemoryAlone(value))
            word = value.NativeIdentity is var address and not 0 ? address : throw RuntimeErrorPanic.RuntimeRaised(NoAddressText);
        else
            throw RuntimeErrorPanic.RuntimeRaised(kind == GoPointerWords.SlotKind.CMemory ? StoreIntoCText : StoreOverGoText);

        *(nuint*)slot.NativeAddress = word;
    }
}
