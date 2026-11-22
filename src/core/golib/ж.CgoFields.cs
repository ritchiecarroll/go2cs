// ж.CgoFields.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using go.golib;

namespace go;

// cgo Phase 3, increment 3.2, seat 1a (docs/phase4/DESIGN-cgo-phase3-marshaling.md 18.9): the pointer field of a C
// struct. A struct cmd/cgo declared for a C struct with a pointer field carries GoCgoStructAttribute, and go2cs-gen
// reads and writes each such field through the members below in place of a ref property. In C memory the field is a
// word at its Go offset, where the struct's CLR slot holds a managed reference.
public abstract partial class ж<T>
{
    // The refusals, each by name. A load and a store over C memory take S-B1's wording (unsafe.cs: LoadFromCText,
    // StoreIntoCText); a C struct laid over Go memory has texts of its own, since that memory is Go's; and a slot over Go
    // storage that no pin holds still is S-B1's UnheldSlotText, word for word.
    private const string CgoFieldLoadFromCText = "cgo struct field load: a Go pointer read out of C memory is refused (C memory must not hold a Go pointer)";
    private const string CgoFieldStoreIntoCText = "cgo struct field store: storing a Go pointer into C memory is refused (C memory must not hold a Go pointer)";
    private const string CgoFieldLoadOverGoText = "cgo struct field load: a Go pointer in a C struct laid over Go memory is not read in 3.2 (go2cs keeps no record of the pointer stored there)";
    private const string CgoFieldStoreOverGoText = "cgo struct field store: a Go pointer stored into a C struct laid over Go memory is not supported in 3.2";
    private const string CgoFieldUnheldText = "unsafe.Pointer word: the slot is a native box over Go storage that nothing holds still";

    /// <summary>
    /// Reads a pointer field of a C struct (a struct carrying <see cref="GoCgoStructAttribute"/>): Go's <c>p.f</c>
    /// for <c>f</c> a <c>*U</c>, the getter go2cs-gen emits for it.
    /// </summary>
    /// <remarks>
    /// A managed receiver, a C struct value Go allocated, reads its slot, as <see cref="field{TF}"/> does. A native
    /// receiver reads the C word at the field's Go offset: 0 is nil, a word of C memory is a native box over it, and a
    /// word that names Go memory is refused by name, since go2cs keeps no record of a Go pointer stored there.
    /// </remarks>
    public ж<U> cgoPointerField<U>(FieldRefFunc<T, ж<U>> fieldRefFunc) =>
        this is NativeBox<T> ? CgoPointerFieldRead(fieldRefFunc) : fieldRefFunc(ref Value);

    /// <summary>
    /// Stores into a pointer field of a C struct: Go's <c>p.f = v</c>, the setter go2cs-gen emits beside
    /// <see cref="cgoPointerField{U}(FieldRefFunc{T, ж{U}})"/>.
    /// </summary>
    /// <remarks>
    /// A managed receiver stores into its slot. A native receiver stores a C word at the field's Go offset: 0 for nil,
    /// the address of a pointer to C memory, and a Go pointer is refused by name: C memory must not hold one, and a C
    /// struct laid over Go memory does not record one in 3.2.
    /// </remarks>
    public void cgoPointerField<U>(FieldRefFunc<T, ж<U>> fieldRefFunc, ж<U> value)
    {
        if (this is NativeBox<T>)
            CgoPointerFieldWrite(fieldRefFunc, value);
        else
            fieldRefFunc(ref Value) = value;
    }

    /// <summary>
    /// The address of a word field of a C struct, for <c>unsafe.Pointer.LoadWord</c> and <c>StoreWord</c>: the slot the
    /// getter and setter go2cs-gen emits for an <c>unsafe.Pointer</c> field of a <see cref="GoCgoStructAttribute"/>
    /// struct read and write through.
    /// </summary>
    /// <remarks>
    /// It is <see cref="of{TElem}(FieldRefFunc{T, TElem})"/>, except that a native receiver whose field has no known Go
    /// offset is refused by name. <c>of</c> keeps a managed view there, which the word doors would read at the CLR slot of
    /// a struct in native memory: the wrong bytes, with no refusal.
    /// </remarks>
    public ж<TF> cgoWordField<TF>(FieldRefFunc<T, TF> fieldRefFunc)
    {
        if (this is NativeBox<T>)
            RequireGoFieldOffset(fieldRefFunc);

        return of(fieldRefFunc);
    }

    // A native receiver's field with no known Go offset is refused by name: its CLR slot is not where the field's bytes
    // are. A nil native receiver is Go's nil dereference.
    private void RequireGoFieldOffset<TF>(FieldRefFunc<T, TF> fieldRefFunc)
    {
        if (NativeAddress == 0)
            throw RuntimeErrorPanic.NilPointerDereference();

        if (FieldRefBox<TF>.GoFieldOffsetOf(this, fieldRefFunc) < 0)
            throw RuntimeErrorPanic.NativeStructLayoutMismatch(typeof(T), $"field {FieldRefBox<TF>.FieldNameOf(fieldRefFunc)} has no known Go offset");
    }

    // The field's word slot over a native receiver, and which memory it is in (GoPointerWords.Classify): C memory alone,
    // Go memory a pin holds still, or neither. Unheld is refused here, for a read and a store alike.
    private NativeBox<ж<U>> CgoPointerFieldSlot<U>(FieldRefFunc<T, ж<U>> fieldRefFunc, out GoPointerWords.SlotKind kind)
    {
        RequireGoFieldOffset(fieldRefFunc);

        ж<ж<U>> slot = of(fieldRefFunc);
        kind = GoPointerWords.Classify(slot);

        return kind switch
        {
            GoPointerWords.SlotKind.CMemory or GoPointerWords.SlotKind.GoMemory => (NativeBox<ж<U>>)slot,
            GoPointerWords.SlotKind.Unheld => throw RuntimeErrorPanic.RuntimeRaised(CgoFieldUnheldText),
            _ => throw RuntimeErrorPanic.NativeStructLayoutMismatch(typeof(T), $"field {FieldRefBox<ж<U>>.FieldNameOf(fieldRefFunc)} has no known Go offset")
        };
    }

    // The read over a native receiver: 0 is nil, a word Go memory names is refused, and any other word is C memory.
    private unsafe ж<U> CgoPointerFieldRead<U>(FieldRefFunc<T, ж<U>> fieldRefFunc)
    {
        NativeBox<ж<U>> slot = CgoPointerFieldSlot(fieldRefFunc, out GoPointerWords.SlotKind kind);
        nuint word = *(nuint*)slot.NativeAddress;

        if (word == 0)
            return null!;

        if (GoPointerWords.NamesGoMemory(word))
            throw RuntimeErrorPanic.RuntimeRaised(kind == GoPointerWords.SlotKind.CMemory ? CgoFieldLoadFromCText : CgoFieldLoadOverGoText);

        // A fresh box per read. Go's == on two reads compares the address (NativeBox's Equals reads the native identity),
        // so l.next == l.next holds, but the two reads are not one CLR object: a ReferenceEquals site would tell them
        // apart. The `**C.T` load (seat 1c) mints the same way.
        return new NativeBox<U>(word);
    }

    // The store over a native receiver: 0 for nil, the address of a pointer to C memory alone, and a Go pointer refused.
    // C memory alone is whatever box names it (GoPointerWords.IsCMemoryAlone): a native box over C memory, or an element
    // of a native-backed slice that keeps no anchor (ж.ElemRefBox.cs, 3.1's C2). Its address is the box's native
    // identity.
    private unsafe void CgoPointerFieldWrite<U>(FieldRefFunc<T, ж<U>> fieldRefFunc, ж<U> value)
    {
        NativeBox<ж<U>> slot = CgoPointerFieldSlot(fieldRefFunc, out GoPointerWords.SlotKind kind);
        nuint word;

        if (value is null || value.IsNilPointer)
            word = 0;
        else if (GoPointerWords.IsCMemoryAlone(value) && value.NativeIdentity is var address and not 0)
            word = address;
        else
            throw RuntimeErrorPanic.RuntimeRaised(kind == GoPointerWords.SlotKind.CMemory ? CgoFieldStoreIntoCText : CgoFieldStoreOverGoText);

        *(nuint*)slot.NativeAddress = word;
    }
}
