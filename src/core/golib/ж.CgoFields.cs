// ж.CgoFields.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using go.golib;

namespace go;

// cgo Phase 3, increment 3.2, part 1a (docs/phase4/DESIGN-cgo-phase3-marshaling.md 18.9): the pointer field of a C
// struct. A struct cmd/cgo declared for a C struct with a pointer field carries GoCgoStructAttribute, and go2cs-gen
// reads and writes each such field through the members below in place of a ref property. In C memory the field is a
// word at its Go offset, where the struct's CLR slot holds a managed reference.
public abstract partial class ж<T>
{
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

    // The field's word slot over a native receiver: a native box at the field's Go offset. CgoPointerWords reads and
    // writes it, and classifies its memory first.
    private NativeBox<ж<U>> CgoPointerFieldSlot<U>(FieldRefFunc<T, ж<U>> fieldRefFunc)
    {
        RequireGoFieldOffset(fieldRefFunc);

        return of(fieldRefFunc) is NativeBox<ж<U>> slot
            ? slot
            : throw RuntimeErrorPanic.NativeStructLayoutMismatch(typeof(T), $"field {FieldRefBox<ж<U>>.FieldNameOf(fieldRefFunc)} has no known Go offset");
    }

    // The read over a native receiver (CgoPointerWords.ReadWord).
    private ж<U> CgoPointerFieldRead<U>(FieldRefFunc<T, ж<U>> fieldRefFunc) =>
        CgoPointerWords.ReadWord(CgoPointerFieldSlot(fieldRefFunc));

    // The store over a native receiver (CgoPointerWords.WriteWord).
    private void CgoPointerFieldWrite<U>(FieldRefFunc<T, ж<U>> fieldRefFunc, ж<U> value) =>
        CgoPointerWords.WriteWord(CgoPointerFieldSlot(fieldRefFunc), value);
}
