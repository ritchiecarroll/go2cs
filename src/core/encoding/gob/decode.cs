// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:generate go run decgen.go -output dec_helpers.go
namespace go.encoding;

using encoding = encoding_package;
using errors = errors_package;
using saferio = @internal.saferio_package;
using io = io_package;
using math = math_package;
using bits = go.math.bits_package;
using reflect = reflect_package;
using @internal;
using go.math;

partial class gob_package {

internal static error errBadUint = errors.New("gob: encoded unsigned integer out of range"u8);
internal static error errBadType = errors.New("gob: unknown type id or corrupted data"u8);
internal static error errRange = errors.New("gob: bad data: field numbers out of bounds"u8);

// type decHelper is a methodless func type — rendered inline as its base delegate

// decoderState is the execution state of an instance of the decoder. A new state
// is created for nested objects.
[GoType] partial struct decoderState {
    internal ж<Decoder> dec;
    // The buffer is stored with an extra indirection because it may be replaced
    // if we load a type during decode (when reading an interface value).
    internal ж<decBuffer> b;
    internal nint fieldnum;          // the last field number read.
    internal ж<decoderState> next; // for free list
}

// decBuffer is an extremely simple, fast implementation of a read-only byte buffer.
// It is initialized by calling Size and then copying the data into the slice returned by Bytes().
[GoType] partial struct decBuffer {
    internal slice<byte> data;
    internal nint offset; // Read offset.
}

[GoRecv] internal static (nint, error) Read(this ref decBuffer d, slice<byte> p) {
    nint n = copy(p, d.data.slice(d.offset));
    if (n == 0 && len(p) != 0) {
        return (0, io.EOF);
    }
    d.offset += n;
    return (n, default!);
}

[GoRecv] internal static void Drop(this ref decBuffer d, nint n) {
    if (n > d.Len()) {
        throw panic("drop");
    }
    d.offset += n;
}

[GoRecv] internal static (byte, error) ReadByte(this ref decBuffer d) {
    if (d.offset >= len(d.data)) {
        return (0, io.EOF);
    }
    var c = d.data[d.offset];
    d.offset++;
    return (c, default!);
}

[GoRecv] internal static nint Len(this ref decBuffer d) {
    return len(d.data) - d.offset;
}

[GoRecv] internal static slice<byte> Bytes(this ref decBuffer d) {
    return d.data.slice(d.offset);
}

// SetBytes sets the buffer to the bytes, discarding any existing data.
[GoRecv] internal static void SetBytes(this ref decBuffer d, slice<byte> data) {
    d.data = data;
    d.offset = 0;
}

[GoRecv] internal static void Reset(this ref decBuffer d) {
    d.data = d.data[0..0];
    d.offset = 0;
}

// We pass the bytes.Buffer separately for easier testing of the infrastructure
// without requiring a full Decoder.
internal static ж<decoderState> newDecoderState(this ж<Decoder> Ꮡdec, ж<decBuffer> Ꮡbuf) {
    ref var dec = ref Ꮡdec.DerefOrNull();

    var d = dec.freeList;
    if (d == nil){
        d = @new<decoderState>();
        d.Value.dec = Ꮡdec;
    } else {
        dec.freeList = d.Value.next;
    }
    d.Value.b = Ꮡbuf;
    return d;
}

[GoRecv] internal static void freeDecoderState(this ref Decoder dec, ж<decoderState> Ꮡd) {
    ref var d = ref Ꮡd.DerefOrNull();

    d.next = dec.freeList;
    dec.freeList = Ꮡd;
}

internal static error overflow(@string name) {
    return errors.New(@"value for """u8 + name + @""" out of range"u8);
}

// decodeUintReader reads an encoded unsigned integer from an io.Reader.
// Used only by the Decoder to read the message length.
internal static (uint64 x, nint width, error err) decodeUintReader(io.Reader r, slice<byte> buf) {
    uint64 x = default!;
    nint width = default!;
    error err = default!;

    width = 1;
    (var n, err) = io.ReadFull(r, buf.slice(0, width));
    if (n == 0) {
        return (x, width, err);
    }
    var b = buf[0];
    if (b <= 0x7f) {
        return ((uint64)b, width, default!);
    }
    n = -(nint)(int8)b;
    if (n > uint64Size) {
        err = errBadUint;
        return (x, width, err);
    }
    (width, err) = io.ReadFull(r, buf.slice(0, n));
    if (err != default!) {
        if (AreEqual(err, io.EOF)) {
            err = io.ErrUnexpectedEOF;
        }
        return (x, width, err);
    }
    // Could check that the high byte is zero but it's not worth it.
    foreach (var (_, bΔ1) in buf.slice(0, width)) {
        x = (uint64)((x << (int)(8)) | (uint64)bΔ1);
    }
    width++; // +1 for length byte
    return (x, width, err);
}

// decodeUint reads an encoded unsigned integer from state.r.
// Does not check for overflow.
[GoRecv] internal static uint64 /*x*/ decodeUint(this ref decoderState state) {
    uint64 x = default!;

    var (b, err) = state.b.ReadByte();
    if (err != default!) {
        error_(err);
    }
    if (b <= 0x7f) {
        return (uint64)b;
    }
    nint n = -(nint)(int8)b;
    if (n > uint64Size) {
        error_(errBadUint);
    }
    var buf = state.b.Bytes();
    if (len(buf) < n) {
        errorf("invalid uint data length %d: exceeds input size %d"u8, n, len(buf));
    }
    // Don't need to check error; it's safe to loop regardless.
    // Could check that the high byte is zero but it's not worth it.
    foreach (var (_, bΔ1) in buf.slice(0, n)) {
        x = (uint64)((x << (int)(8)) | (uint64)bΔ1);
    }
    state.b.Drop(n);
    return x;
}

// decodeInt reads an encoded signed integer from state.r.
// Does not check for overflow.
[GoRecv] internal static int64 decodeInt(this ref decoderState state) {
    var x = state.decodeUint();
    if ((uint64)(x & 1) != 0) {
        return ~(int64)((x >> (int)(1)));
    }
    return (int64)((x >> (int)(1)));
}

// getLength decodes the next uint and makes sure it is a possible
// size for a data item that follows, which means it must fit in a
// non-negative int and fit in the buffer.
[GoRecv] internal static (nint, bool) getLength(this ref decoderState state) {
    nint n = (nint)state.decodeUint();
    if (n < 0 || state.b.Len() < n || tooBig <= n) {
        return (0, false);
    }
    return (n, true);
}

// type decOp is a methodless func type — rendered inline as its base delegate

// The 'instructions' of the decoding machine
[GoType] partial struct decInstr {
    internal Action<ж<decInstr>, ж<decoderState>, reflectꓸValue> op;
    internal nint field;  // field number of the wire type
    internal slice<nint> index; // field access indices for destination type
    internal error ovfl; // error message for overflow/underflow (for arrays, of the elements)
}

// ignoreUint discards a uint value with no destination.
internal static void ignoreUint(ж<decInstr> Ꮡi, ж<decoderState> Ꮡstate, reflectꓸValue v) {
    ref var state = ref Ꮡstate.DerefOrNull();

    state.decodeUint();
}

// ignoreTwoUints discards a uint value with no destination. It's used to skip
// complex values.
internal static void ignoreTwoUints(ж<decInstr> Ꮡi, ж<decoderState> Ꮡstate, reflectꓸValue v) {
    ref var state = ref Ꮡstate.DerefOrNull();

    state.decodeUint();
    state.decodeUint();
}

// Since the encoder writes no zeros, if we arrive at a decoder we have
// a value to extract and store. The field number has already been read
// (it's how we knew to call this decoder).
// Each decoder is responsible for handling any indirections associated
// with the data structure. If any pointer so reached is nil, allocation must
// be done.

// decAlloc takes a value and returns a settable value that can
// be assigned to. If the value is a pointer, decAlloc guarantees it points to storage.
// The callers to the individual decoders are expected to have used decAlloc.
// The individual decoders don't need it.
internal static reflectꓸValue decAlloc(reflectꓸValue v) {
    while (v.Kind() == reflect.ΔPointer) {
        if (v.IsNil()) {
            v.Set(reflect.New(v.Type().Elem()));
        }
        v = v.Elem();
    }
    return v;
}

// decBool decodes a uint and stores it as a boolean in value.
internal static void decBool(ж<decInstr> Ꮡi, ж<decoderState> Ꮡstate, reflectꓸValue value) {
    ref var state = ref Ꮡstate.DerefOrNull();

    value.SetBool(state.decodeUint() != 0);
}

// decInt8 decodes an integer and stores it as an int8 in value.
internal static void decInt8(ж<decInstr> Ꮡi, ж<decoderState> Ꮡstate, reflectꓸValue value) {
    ref var i = ref Ꮡi.DerefOrNull();
    ref var state = ref Ꮡstate.DerefOrNull();

    var v = state.decodeInt();
    if (v < math.MinInt8 || math.MaxInt8 < v) {
        error_(i.ovfl);
    }
    value.SetInt(v);
}

// decUint8 decodes an unsigned integer and stores it as a uint8 in value.
internal static void decUint8(ж<decInstr> Ꮡi, ж<decoderState> Ꮡstate, reflectꓸValue value) {
    ref var i = ref Ꮡi.DerefOrNull();
    ref var state = ref Ꮡstate.DerefOrNull();

    var v = state.decodeUint();
    if (math.MaxUint8 < v) {
        error_(i.ovfl);
    }
    value.SetUint(v);
}

// decInt16 decodes an integer and stores it as an int16 in value.
internal static void decInt16(ж<decInstr> Ꮡi, ж<decoderState> Ꮡstate, reflectꓸValue value) {
    ref var i = ref Ꮡi.DerefOrNull();
    ref var state = ref Ꮡstate.DerefOrNull();

    var v = state.decodeInt();
    if (v < math.MinInt16 || math.MaxInt16 < v) {
        error_(i.ovfl);
    }
    value.SetInt(v);
}

// decUint16 decodes an unsigned integer and stores it as a uint16 in value.
internal static void decUint16(ж<decInstr> Ꮡi, ж<decoderState> Ꮡstate, reflectꓸValue value) {
    ref var i = ref Ꮡi.DerefOrNull();
    ref var state = ref Ꮡstate.DerefOrNull();

    var v = state.decodeUint();
    if (math.MaxUint16 < v) {
        error_(i.ovfl);
    }
    value.SetUint(v);
}

// decInt32 decodes an integer and stores it as an int32 in value.
internal static void decInt32(ж<decInstr> Ꮡi, ж<decoderState> Ꮡstate, reflectꓸValue value) {
    ref var i = ref Ꮡi.DerefOrNull();
    ref var state = ref Ꮡstate.DerefOrNull();

    var v = state.decodeInt();
    if (v < math.MinInt32 || math.MaxInt32 < v) {
        error_(i.ovfl);
    }
    value.SetInt(v);
}

// decUint32 decodes an unsigned integer and stores it as a uint32 in value.
internal static void decUint32(ж<decInstr> Ꮡi, ж<decoderState> Ꮡstate, reflectꓸValue value) {
    ref var i = ref Ꮡi.DerefOrNull();
    ref var state = ref Ꮡstate.DerefOrNull();

    var v = state.decodeUint();
    if (math.MaxUint32 < v) {
        error_(i.ovfl);
    }
    value.SetUint(v);
}

// decInt64 decodes an integer and stores it as an int64 in value.
internal static void decInt64(ж<decInstr> Ꮡi, ж<decoderState> Ꮡstate, reflectꓸValue value) {
    ref var state = ref Ꮡstate.DerefOrNull();

    var v = state.decodeInt();
    value.SetInt(v);
}

// decUint64 decodes an unsigned integer and stores it as a uint64 in value.
internal static void decUint64(ж<decInstr> Ꮡi, ж<decoderState> Ꮡstate, reflectꓸValue value) {
    ref var state = ref Ꮡstate.DerefOrNull();

    var v = state.decodeUint();
    value.SetUint(v);
}

// Floating-point numbers are transmitted as uint64s holding the bits
// of the underlying representation. They are sent byte-reversed, with
// the exponent end coming out first, so integer floating point numbers
// (for example) transmit more compactly. This routine does the
// unswizzling.
internal static float64 float64FromBits(uint64 u) {
    var v = bits.ReverseBytes64(u);
    return math.Float64frombits(v);
}

// float32FromBits decodes an unsigned integer, treats it as a 32-bit floating-point
// number, and returns it. It's a helper function for float32 and complex64.
// It returns a float64 because that's what reflection needs, but its return
// value is known to be accurately representable in a float32.
internal static float64 float32FromBits(uint64 u, error ovfl) {
    var v = float64FromBits(u);
    var av = v;
    if (av < 0D) {
        av = -av;
    }
    // +Inf is OK in both 32- and 64-bit floats. Underflow is always OK.
    if (math.MaxFloat32 < av && av <= math.MaxFloat64) {
        error_(ovfl);
    }
    return v;
}

// decFloat32 decodes an unsigned integer, treats it as a 32-bit floating-point
// number, and stores it in value.
internal static void decFloat32(ж<decInstr> Ꮡi, ж<decoderState> Ꮡstate, reflectꓸValue value) {
    ref var i = ref Ꮡi.DerefOrNull();
    ref var state = ref Ꮡstate.DerefOrNull();

    value.SetFloat(float32FromBits(state.decodeUint(), i.ovfl));
}

// decFloat64 decodes an unsigned integer, treats it as a 64-bit floating-point
// number, and stores it in value.
internal static void decFloat64(ж<decInstr> Ꮡi, ж<decoderState> Ꮡstate, reflectꓸValue value) {
    ref var state = ref Ꮡstate.DerefOrNull();

    value.SetFloat(float64FromBits(state.decodeUint()));
}

// decComplex64 decodes a pair of unsigned integers, treats them as a
// pair of floating point numbers, and stores them as a complex64 in value.
// The real part comes first.
internal static void decComplex64(ж<decInstr> Ꮡi, ж<decoderState> Ꮡstate, reflectꓸValue value) {
    ref var i = ref Ꮡi.DerefOrNull();
    ref var state = ref Ꮡstate.DerefOrNull();

    var real = float32FromBits(state.decodeUint(), i.ovfl);
    var imag = float32FromBits(state.decodeUint(), i.ovfl);
    value.SetComplex(complex(real, imag));
}

// decComplex128 decodes a pair of unsigned integers, treats them as a
// pair of floating point numbers, and stores them as a complex128 in value.
// The real part comes first.
internal static void decComplex128(ж<decInstr> Ꮡi, ж<decoderState> Ꮡstate, reflectꓸValue value) {
    ref var state = ref Ꮡstate.DerefOrNull();

    var real = float64FromBits(state.decodeUint());
    var imag = float64FromBits(state.decodeUint());
    value.SetComplex(complex(real, imag));
}

// decUint8Slice decodes a byte slice and stores in value a slice header
// describing the data.
// uint8 slices are encoded as an unsigned count followed by the raw bytes.
internal static void decUint8Slice(ж<decInstr> Ꮡi, ж<decoderState> Ꮡstate, reflectꓸValue value) {
    ref var state = ref Ꮡstate.DerefOrNull();

    var (n, ok) = state.getLength();
    if (!ok) {
        errorf("bad %s slice length: %d"u8, value.Type(), n);
    }
    if (value.Cap() < n){
        nint safe = saferio.SliceCap<byte>((uint64)n);
        if (safe < 0) {
            errorf("%s slice too big: %d elements"u8, value.Type(), n);
        }
        value.Set(reflect.MakeSlice(value.Type(), safe, safe));
        nint ln = safe;
        nint iΔ1 = 0;
        while (iΔ1 < n) {
            if (iΔ1 >= ln) {
                // We didn't allocate the entire slice,
                // due to using saferio.SliceCap.
                // Grow the slice for one more element.
                // The slice is full, so this should
                // bump up the capacity.
                value.Grow(1);
            }
            // Copy into s up to the capacity or n,
            // whichever is less.
            ln = value.Cap();
            if (ln > n) {
                ln = n;
            }
            value.SetLen(ln);
            var sub = value.Slice(iΔ1, ln);
            {
                var (_, err) = state.b.Read(sub.Bytes()); if (err != default!) {
                    errorf("error decoding []byte at %d: %s"u8, iΔ1, err);
                }
            }
            iΔ1 = ln;
        }
    } else {
        value.SetLen(n);
        {
            var (_, err) = state.b.Read(value.Bytes()); if (err != default!) {
                errorf("error decoding []byte: %s"u8, err);
            }
        }
    }
}

// decString decodes byte array and stores in value a string header
// describing the data.
// Strings are encoded as an unsigned count followed by the raw bytes.
internal static void decString(ж<decInstr> Ꮡi, ж<decoderState> Ꮡstate, reflectꓸValue value) {
    ref var state = ref Ꮡstate.DerefOrNull();

    var (n, ok) = state.getLength();
    if (!ok) {
        errorf("bad %s slice length: %d"u8, value.Type(), n);
    }
    // Read the data.
    var data = state.b.Bytes();
    if (len(data) < n) {
        errorf("invalid string length %d: exceeds input size %d"u8, n, len(data));
    }
    @string s = ((@string)(data.slice(0, n)));
    state.b.Drop(n);
    value.SetString(s);
}

// ignoreUint8Array skips over the data for a byte slice value with no destination.
internal static void ignoreUint8Array(ж<decInstr> Ꮡi, ж<decoderState> Ꮡstate, reflectꓸValue value) {
    ref var state = ref Ꮡstate.DerefOrNull();

    var (n, ok) = state.getLength();
    if (!ok) {
        errorf("slice length too large"u8);
    }
    nint bn = state.b.Len();
    if (bn < n) {
        errorf("invalid slice length %d: exceeds input size %d"u8, n, bn);
    }
    state.b.Drop(n);
}

// Execution engine

// The encoder engine is an array of instructions indexed by field number of the incoming
// decoder. It is executed with random access according to field number.
[GoType] partial struct decEngine {
    internal slice<decInstr> instr;
    internal nint numInstr; // the number of active instructions
}

// decodeSingle decodes a top-level value that is not a struct and stores it in value.
// Such values are preceded by a zero, making them have the memory layout of a
// struct field (although with an illegal field number).
internal static void decodeSingle(this ж<Decoder> Ꮡdec, ж<decEngine> Ꮡengine, reflectꓸValue value) {
    GoFrame ᒐ = default;
    try {
        ref var engine = ref Ꮡengine.DerefOrNull();

        var state = Ꮡdec.newDecoderState(Ꮡdec.of(Decoder.Ꮡbuf));
        defer(Ꮡdec.freeDecoderState, state, ref ᒐ);
        state.Value.fieldnum = singletonField;
        if (state.decodeUint() != 0) {
            errorf("decode: corrupted data: non-zero delta for singleton"u8);
        }
        var instr = Ꮡ(engine.instr, singletonField);
        (~instr).op(instr, state, value);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// decodeStruct decodes a top-level struct and stores it in value.
// Indir is for the value, not the type. At the time of the call it may
// differ from ut.indir, which was computed when the engine was built.
// This state cannot arise for decodeSingle, which is called directly
// from the user's value, not from the innards of an engine.
internal static void decodeStruct(this ж<Decoder> Ꮡdec, ж<decEngine> Ꮡengine, reflectꓸValue value) {
    GoFrame ᒐ = default;
    try {
        ref var engine = ref Ꮡengine.DerefOrNull();

        var state = Ꮡdec.newDecoderState(Ꮡdec.of(Decoder.Ꮡbuf));
        defer(Ꮡdec.freeDecoderState, state, ref ᒐ);
        state.Value.fieldnum = -1;
        while ((~state).b.Len() > 0) {
            nint delta = (nint)state.decodeUint();
            if (delta < 0) {
                errorf("decode: corrupted data: negative delta"u8);
            }
            if (delta == 0) {
                // struct terminator is zero delta fieldnum
                break;
            }
            if ((~state).fieldnum >= len(engine.instr) - delta) {
                // subtract to compare without overflow
                error_(errRange);
            }
            nint fieldnum = (~state).fieldnum + delta;
            var instr = Ꮡ(engine.instr, fieldnum);
            reflectꓸValue field = new(nil);
            if ((~instr).index != default!) {
                // Otherwise the field is unknown to us and instr.op is an ignore op.
                field = value.FieldByIndex((~instr).index);
                if (field.Kind() == reflect.ΔPointer) {
                    field = decAlloc(field);
                }
            }
            (~instr).op(instr, state, field);
            state.Value.fieldnum = fieldnum;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static reflectꓸValue noValue = new(nil);

// ignoreStruct discards the data for a struct with no destination.
internal static void ignoreStruct(this ж<Decoder> Ꮡdec, ж<decEngine> Ꮡengine) {
    GoFrame ᒐ = default;
    try {
        ref var engine = ref Ꮡengine.DerefOrNull();

        var state = Ꮡdec.newDecoderState(Ꮡdec.of(Decoder.Ꮡbuf));
        defer(Ꮡdec.freeDecoderState, state, ref ᒐ);
        state.Value.fieldnum = -1;
        while ((~state).b.Len() > 0) {
            nint delta = (nint)state.decodeUint();
            if (delta < 0) {
                errorf("ignore decode: corrupted data: negative delta"u8);
            }
            if (delta == 0) {
                // struct terminator is zero delta fieldnum
                break;
            }
            nint fieldnum = (~state).fieldnum + delta;
            if (fieldnum >= len(engine.instr)) {
                error_(errRange);
            }
            var instr = Ꮡ(engine.instr, fieldnum);
            (~instr).op(instr, state, noValue);
            state.Value.fieldnum = fieldnum;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// ignoreSingle discards the data for a top-level non-struct value with no
// destination. It's used when calling Decode with a nil value.
internal static void ignoreSingle(this ж<Decoder> Ꮡdec, ж<decEngine> Ꮡengine) {
    GoFrame ᒐ = default;
    try {
        ref var engine = ref Ꮡengine.DerefOrNull();

        var state = Ꮡdec.newDecoderState(Ꮡdec.of(Decoder.Ꮡbuf));
        defer(Ꮡdec.freeDecoderState, state, ref ᒐ);
        state.Value.fieldnum = singletonField;
        nint delta = (nint)state.decodeUint();
        if (delta != 0) {
            errorf("decode: corrupted data: non-zero delta for singleton"u8);
        }
        var instr = Ꮡ(engine.instr, singletonField);
        (~instr).op(instr, state, noValue);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// decodeArrayHelper does the work for decoding arrays and slices.
[GoRecv] internal static void decodeArrayHelper(this ref Decoder dec, ж<decoderState> Ꮡstate, reflectꓸValue value, Action<ж<decInstr>, ж<decoderState>, reflectꓸValue> elemOp, nint length, error ovfl, Func<ж<decoderState>, reflectꓸValue, nint, error, bool> helper) {
    ref var state = ref Ꮡstate.DerefOrNull();

    if (helper != default! && helper(Ꮡstate, value, length, ovfl)) {
        return;
    }
    var instr = Ꮡ(new decInstr(elemOp, 0, default!, ovfl));
    var isPtr = value.Type().Elem().Kind() == reflect.ΔPointer;
    nint ln = value.Len();
    for (nint i = 0; i < length; i++) {
        if (state.b.Len() == 0) {
            errorf("decoding array or slice: length exceeds input size (%d elements)"u8, length);
        }
        if (i >= ln) {
            // This is a slice that we only partially allocated.
            // Grow it up to length.
            value.Grow(1);
            nint cp = value.Cap();
            if (cp > length) {
                cp = length;
            }
            value.SetLen(cp);
            ln = cp;
        }
        var v = value.Index(i);
        if (isPtr) {
            v = decAlloc(v);
        }
        elemOp(instr, Ꮡstate, v);
    }
}

// decodeArray decodes an array and stores it in value.
// The length is an unsigned integer preceding the elements. Even though the length is redundant
// (it's part of the type), it's a useful check and is included in the encoding.
[GoRecv] internal static void decodeArray(this ref Decoder dec, ж<decoderState> Ꮡstate, reflectꓸValue value, Action<ж<decInstr>, ж<decoderState>, reflectꓸValue> elemOp, nint length, error ovfl, Func<ж<decoderState>, reflectꓸValue, nint, error, bool> helper) {
    ref var state = ref Ꮡstate.DerefOrNull();

    {
        var n = state.decodeUint(); if (n != (uint64)length) {
            errorf("length mismatch in decodeArray"u8);
        }
    }
    dec.decodeArrayHelper(Ꮡstate, value, elemOp, length, ovfl, helper);
}

// decodeIntoValue is a helper for map decoding.
internal static reflectꓸValue decodeIntoValue(ж<decoderState> Ꮡstate, Action<ж<decInstr>, ж<decoderState>, reflectꓸValue> op, bool isPtr, reflectꓸValue value, ж<decInstr> Ꮡinstr) {
    var v = value;
    if (isPtr) {
        v = decAlloc(value);
    }
    op(Ꮡinstr, Ꮡstate, v);
    return value;
}

// decodeMap decodes a map and stores it in value.
// Maps are encoded as a length followed by key:value pairs.
// Because the internals of maps are not visible to us, we must
// use reflection rather than pointer magic.
[GoRecv] internal static void decodeMap(this ref Decoder dec, reflectꓸType mtyp, ж<decoderState> Ꮡstate, reflectꓸValue value, Action<ж<decInstr>, ж<decoderState>, reflectꓸValue> keyOp, Action<ж<decInstr>, ж<decoderState>, reflectꓸValue> elemOp, error ovfl) {
    ref var state = ref Ꮡstate.DerefOrNull();

    nint n = (nint)state.decodeUint();
    if (value.IsNil()) {
        value.Set(reflect.MakeMapWithSize(mtyp, n));
    }
    var keyIsPtr = mtyp.Key().Kind() == reflect.ΔPointer;
    var elemIsPtr = mtyp.Elem().Kind() == reflect.ΔPointer;
    var keyInstr = Ꮡ(new decInstr(keyOp, 0, default!, ovfl));
    var elemInstr = Ꮡ(new decInstr(elemOp, 0, default!, ovfl));
    var keyP = reflect.New(mtyp.Key());
    var elemP = reflect.New(mtyp.Elem());
    for (nint i = 0; i < n; i++) {
        var key = decodeIntoValue(Ꮡstate, keyOp, keyIsPtr, keyP.Elem(), keyInstr);
        var elem = decodeIntoValue(Ꮡstate, elemOp, elemIsPtr, elemP.Elem(), elemInstr);
        value.SetMapIndex(key, elem);
        keyP.Elem().SetZero();
        elemP.Elem().SetZero();
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noErrorˢ = "no error"u8;

// ignoreArrayHelper does the work for discarding arrays and slices.
[GoRecv] internal static void ignoreArrayHelper(this ref Decoder dec, ж<decoderState> Ꮡstate, Action<ж<decInstr>, ж<decoderState>, reflectꓸValue> elemOp, nint length) {
    ref var state = ref Ꮡstate.DerefOrNull();

    var instr = Ꮡ(new decInstr(elemOp, 0, default!, errors.New(noErrorˢ)));
    for (nint i = 0; i < length; i++) {
        if (state.b.Len() == 0) {
            errorf("decoding array or slice: length exceeds input size (%d elements)"u8, length);
        }
        elemOp(instr, Ꮡstate, noValue);
    }
}

// ignoreArray discards the data for an array value with no destination.
[GoRecv] internal static void ignoreArray(this ref Decoder dec, ж<decoderState> Ꮡstate, Action<ж<decInstr>, ж<decoderState>, reflectꓸValue> elemOp, nint length) {
    ref var state = ref Ꮡstate.DerefOrNull();

    {
        var n = state.decodeUint(); if (n != (uint64)length) {
            errorf("length mismatch in ignoreArray"u8);
        }
    }
    dec.ignoreArrayHelper(Ꮡstate, elemOp, length);
}

// ignoreMap discards the data for a map value with no destination.
[GoRecv] internal static void ignoreMap(this ref Decoder dec, ж<decoderState> Ꮡstate, Action<ж<decInstr>, ж<decoderState>, reflectꓸValue> keyOp, Action<ж<decInstr>, ж<decoderState>, reflectꓸValue> elemOp) {
    ref var state = ref Ꮡstate.DerefOrNull();

    nint n = (nint)state.decodeUint();
    var keyInstr = Ꮡ(new decInstr(keyOp, 0, default!, errors.New(noErrorˢ)));
    var elemInstr = Ꮡ(new decInstr(elemOp, 0, default!, errors.New(noErrorˢ)));
    for (nint i = 0; i < n; i++) {
        keyOp(keyInstr, Ꮡstate, noValue);
        elemOp(elemInstr, Ꮡstate, noValue);
    }
}

// decodeSlice decodes a slice and stores it in value.
// Slices are encoded as an unsigned length followed by the elements.
[GoRecv] internal static void decodeSlice(this ref Decoder dec, ж<decoderState> Ꮡstate, reflectꓸValue value, Action<ж<decInstr>, ж<decoderState>, reflectꓸValue> elemOp, error ovfl, Func<ж<decoderState>, reflectꓸValue, nint, error, bool> helper) {
    ref var state = ref Ꮡstate.DerefOrNull();

    var u = state.decodeUint();
    var typ = value.Type();
    var size = (uint64)typ.Elem().Size();
    var nBytes = u * size;
    nint n = (nint)u;
    // Take care with overflow in this calculation.
    if (n < 0 || (uint64)n != u || nBytes > tooBig || (size > 0 && nBytes / size != u)) {
        // We don't check n against buffer length here because if it's a slice
        // of interfaces, there will be buffer reloads.
        errorf("%s slice too big: %d elements of %d bytes"u8, typ.Elem(), u, size);
    }
    if (value.Cap() < n){
        nint safe = saferio.SliceCapWithSize(size, (uint64)n);
        if (safe < 0) {
            errorf("%s slice too big: %d elements of %d bytes"u8, typ.Elem(), u, size);
        }
        value.Set(reflect.MakeSlice(typ, safe, safe));
    } else {
        value.SetLen(n);
    }
    dec.decodeArrayHelper(Ꮡstate, value, elemOp, n, ovfl, helper);
}

// ignoreSlice skips over the data for a slice value with no destination.
[GoRecv] internal static void ignoreSlice(this ref Decoder dec, ж<decoderState> Ꮡstate, Action<ж<decInstr>, ж<decoderState>, reflectꓸValue> elemOp) {
    ref var state = ref Ꮡstate.DerefOrNull();

    dec.ignoreArrayHelper(Ꮡstate, elemOp, (nint)state.decodeUint());
}

// decodeInterface decodes an interface value and stores it in value.
// Interfaces are encoded as the name of a concrete type followed by a value.
// If the name is empty, the value is nil and no value is sent.
internal static void decodeInterface(this ж<Decoder> Ꮡdec, reflectꓸType ityp, ж<decoderState> Ꮡstate, reflectꓸValue value) {
    ref var dec = ref Ꮡdec.DerefOrNull();
    ref var state = ref Ꮡstate.DerefOrNull();

    // Read the name of the concrete type.
    var nr = state.decodeUint();
    if (nr > ((uint64)1 << (int)(31))) {
        // zero is permissible for anonymous types
        errorf("invalid type name length %d"u8, nr);
    }
    if (nr > (uint64)state.b.Len()) {
        errorf("invalid type name length %d: exceeds input size"u8, nr);
    }
    nint n = (nint)nr;
    var name = state.b.Bytes().slice(0, n);
    state.b.Drop(n);
    // Allocate the destination interface value.
    if (len(name) == 0) {
        // Copy the nil interface value to the target.
        value.SetZero();
        return;
    }
    if (len(name) > 1024) {
        errorf("name too long (%d bytes): %.20q..."u8, len(name), name);
    }
    // The concrete type must be registered.
    var (typi, ok) = ᏑnameToConcreteType.Load(((@string)name));
    if (!ok) {
        errorf("name not registered for interface: %q"u8, name);
    }
    var typ = typi._<reflectꓸType>();
    // Read the type id of the concrete value.
    var concreteId = Ꮡdec.decodeTypeSequence(true);
    if (concreteId < 0) {
        error_(dec.err);
    }
    // Byte count of value is next; we don't care what it is (it's there
    // in case we want to ignore the value by skipping it completely).
    state.decodeUint();
    // Read the concrete value.
    var v = allocValue(typ);
    Ꮡdec.decodeValue(concreteId, v);
    if (dec.err != default!) {
        error_(dec.err);
    }
    // Assign the concrete value to the interface.
    // Tread carefully; it might not satisfy the interface.
    if (!typ.AssignableTo(ityp)) {
        errorf("%s is not assignable to type %s"u8, typ, ityp);
    }
    // Copy the interface value to the target.
    value.Set(v);
}

// ignoreInterface discards the data for an interface value with no destination.
internal static void ignoreInterface(this ж<Decoder> Ꮡdec, ж<decoderState> Ꮡstate) {
    ref var dec = ref Ꮡdec.DerefOrNull();
    ref var state = ref Ꮡstate.DerefOrNull();

    // Read the name of the concrete type.
    var (n, ok) = state.getLength();
    if (!ok) {
        errorf("bad interface encoding: name too large for buffer"u8);
    }
    nint bn = state.b.Len();
    if (bn < n) {
        errorf("invalid interface value length %d: exceeds input size %d"u8, n, bn);
    }
    state.b.Drop(n);
    var id = Ꮡdec.decodeTypeSequence(true);
    if (id < 0) {
        error_(dec.err);
    }
    // At this point, the decoder buffer contains a delimited value. Just toss it.
    (n, ok) = state.getLength();
    if (!ok) {
        errorf("bad interface encoding: data length too large for buffer"u8);
    }
    state.b.Drop(n);
}

// decodeGobDecoder decodes something implementing the GobDecoder interface.
// The data is encoded as a byte slice.
[GoRecv] internal static void decodeGobDecoder(this ref Decoder dec, ж<userTypeInfo> Ꮡut, ж<decoderState> Ꮡstate, reflectꓸValue value) {
    ref var ut = ref Ꮡut.DerefOrNull();
    ref var state = ref Ꮡstate.DerefOrNull();

    // Read the bytes for the value.
    var (n, ok) = state.getLength();
    if (!ok) {
        errorf("GobDecoder: length too large for buffer"u8);
    }
    var b = state.b.Bytes();
    if (len(b) < n) {
        errorf("GobDecoder: invalid data length %d: exceeds input size %d"u8, n, len(b));
    }
    b = b.slice(0, n);
    state.b.Drop(n);
    error err = default!;
    // We know it's one of these.
    var exprᴛ1 = ut.externalDec;
    if (exprᴛ1 == xGob) {
        err = value.Interface()._<GobDecoder>().GobDecode(b);
    }
    else if (exprᴛ1 == xBinary) {
        err = value.Interface()._<encoding.BinaryUnmarshaler>().UnmarshalBinary(b);
    }
    else if (exprᴛ1 == xText) {
        err = value.Interface()._<encoding.TextUnmarshaler>().UnmarshalText(b);
    }

    if (err != default!) {
        error_(err);
    }
}

// ignoreGobDecoder discards the data for a GobDecoder value with no destination.
[GoRecv] internal static void ignoreGobDecoder(this ref Decoder dec, ж<decoderState> Ꮡstate) {
    ref var state = ref Ꮡstate.DerefOrNull();

    // Read the bytes for the value.
    var (n, ok) = state.getLength();
    if (!ok) {
        errorf("GobDecoder: length too large for buffer"u8);
    }
    nint bn = state.b.Len();
    if (bn < n) {
        errorf("GobDecoder: invalid data length %d: exceeds input size %d"u8, n, bn);
    }
    state.b.Drop(n);
}

// Index by Go types.
internal static array<Action<ж<decInstr>, ж<decoderState>, reflectꓸValue>> decOpTable = new golib.SparseArray<Action<ж<decInstr>, ж<decoderState>, reflectꓸValue>>{
    [1] = decBool,
    [3] = decInt8,
    [4] = decInt16,
    [5] = decInt32,
    [6] = decInt64,
    [8] = decUint8,
    [9] = decUint16,
    [10] = decUint32,
    [11] = decUint64,
    [13] = decFloat32,
    [14] = decFloat64,
    [15] = decComplex64,
    [16] = decComplex128,
    [24] = decString
}.array(25);

// Indexed by gob types.  tComplex will be added during type.init().
internal static map<typeId, Action<ж<decInstr>, ж<decoderState>, reflectꓸValue>> decIgnoreOpMap;
internal static void initᴛdecIgnoreOpMap() { decIgnoreOpMap = new map<typeId, Action<ж<decInstr>, ж<decoderState>, reflectꓸValue>>{
    [tBool] = ignoreUint,
    [tInt] = ignoreUint,
    [tUint] = ignoreUint,
    [tFloat] = ignoreUint,
    [tBytes] = ignoreUint8Array,
    [tString] = ignoreUint8Array,
    [tComplex] = ignoreTwoUints
}; }

// decOpFor returns the decoding op for the base type under rt and
// the indirection count to reach it.
internal static ж<Action<ж<decInstr>, ж<decoderState>, reflectꓸValue>> decOpFor(this ж<Decoder> Ꮡdec, typeId wireId, reflectꓸType rt, @string name, map<reflectꓸType, ж<Action<ж<decInstr>, ж<decoderState>, reflectꓸValue>>> inProgress) {
    ref var dec = ref Ꮡdec.DerefOrNull();

    var ut = userType(rt);
    // If the type implements GobEncoder, we handle it without further processing.
    if ((~ut).externalDec != 0) {
        return dec.gobDecodeOpFor(ut);
    }
    // If this type is already in progress, it's a recursive type (e.g. map[string]*T).
    // Return the pointer to the op we're already building.
    {
        var opPtr = inProgress[rt]; if (opPtr != nil) {
            return opPtr;
        }
    }
    var typ = ut.Value.@base;
    ref var op = ref heap<Action<ж<decInstr>, ж<decoderState>, reflectꓸValue>>(out var Ꮡop);
    reflectꓸKind k = typ.Kind();
    if ((nint)(nuint)k < len(decOpTable)) {
        op = decOpTable[k];
    }
    if (op == default!) {
        inProgress[rt] = Ꮡop;
        // Special cases
        {
            var t = typ;
            var exprᴛ1 = t.Kind();
            if (exprᴛ1 == reflect.Array) {
                name = "element of "u8 + name;
                var elemId = dec.wireType[wireId].Value.ArrayT.Value.Elem;
                var elemOp = Ꮡdec.decOpFor(elemId, t.Elem(), name, inProgress);
                var ovfl = overflow(name);
                var helper = decArrayHelper[t.Elem().Kind()];
                var elemOpʗ1 = elemOp;
                var helperʗ1 = helper;
                var ovflʗ1 = ovfl;
                var tʗ1 = t;
                op = (ж<decInstr> i, ж<decoderState> state, reflectꓸValue value) => {
                    (~state).dec.decodeArray(state, value, elemOpʗ1.ValueSlot, tʗ1.Len(), ovflʗ1, helperʗ1);
                };
            }
            else if (exprᴛ1 == reflect.Map) {
                var keyId = dec.wireType[wireId].Value.MapT.Value.Key;
                var elemId = dec.wireType[wireId].Value.MapT.Value.Elem;
                var keyOp = Ꮡdec.decOpFor(keyId, t.Key(), "key of "u8 + name, inProgress);
                var elemOp = Ꮡdec.decOpFor(elemId, t.Elem(), "element of "u8 + name, inProgress);
                var ovfl = overflow(name);
                var elemOpʗ2 = elemOp;
                var keyOpʗ1 = keyOp;
                var ovflʗ2 = ovfl;
                var tʗ2 = t;
                op = (ж<decInstr> i, ж<decoderState> state, reflectꓸValue value) => {
                    (~state).dec.decodeMap(tʗ2, state, value, keyOpʗ1.ValueSlot, elemOpʗ2.ValueSlot, ovflʗ2);
                };
            }
            else if (exprᴛ1 == reflect.ΔSlice) {
                do {
                    name = "element of "u8 + name;
                    if (t.Elem().Kind() == reflect.Uint8) {
                        op = decUint8Slice;
                        break;
                    }
                    typeId elemId = default!;
                    {
                        var tt = builtinIdToType(wireId); if (tt != default!){
                            elemId = tt._<ж<sliceType>>().Value.Elem;
                        } else {
                            elemId = dec.wireType[wireId].Value.SliceT.Value.Elem;
                        }
                    }
                    var elemOp = Ꮡdec.decOpFor(elemId, t.Elem(), name, inProgress);
                    var ovfl = overflow(name);
                    var helper = decSliceHelper[t.Elem().Kind()];
                    var elemOpʗ3 = elemOp;
                    var helperʗ2 = helper;
                    var ovflʗ3 = ovfl;
                    op = (ж<decInstr> i, ж<decoderState> state, reflectꓸValue value) => {
                        (~state).dec.decodeSlice(state, value, elemOpʗ3.ValueSlot, ovflʗ3, helperʗ2);
                    };
                } while (false);
            }
            else if (exprᴛ1 == reflect.Struct) {
                var utΔ2 = userType(typ);
                var (enginePtr, err) = Ꮡdec.getDecEnginePtr(wireId, // Generate a closure that calls out to the engine for the nested type.
 utΔ2);
                if (err != default!) {
                    error_(err);
                }
                var enginePtrʗ1 = enginePtr;
                op = (ж<decInstr> i, ж<decoderState> state, reflectꓸValue value) => {
                    // indirect through enginePtr to delay evaluation for recursive structs.
                    Ꮡdec.decodeStruct(enginePtrʗ1.ValueSlot, value);
                };
            }
            else if (exprᴛ1 == reflect.ΔInterface) {
                var tʗ3 = t;
                op = (ж<decInstr> i, ж<decoderState> state, reflectꓸValue value) => {
                    (~state).dec.decodeInterface(tʗ3, state, value);
                };
            }
        }

    }
    if (op == default!) {
        errorf("decode can't handle type %s"u8, rt);
    }
    return Ꮡop;
}

internal static nint maxIgnoreNestingDepth = 10000;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidNestingDepthˢ = "invalid nesting depth"u8;

// decIgnoreOpFor returns the decoding op for a field that has no destination.
internal static ж<Action<ж<decInstr>, ж<decoderState>, reflectꓸValue>> decIgnoreOpFor(this ж<Decoder> Ꮡdec, typeId wireId, map<typeId, ж<Action<ж<decInstr>, ж<decoderState>, reflectꓸValue>>> inProgress) {
    GoFrame ᒐ = default;
    try {
        ref var dec = ref Ꮡdec.DerefOrNull();

        // Track how deep we've recursed trying to skip nested ignored fields.
        dec.ignoreDepth++;
        defer(() => {
            Ꮡdec.Value.ignoreDepth--;
        }, ref ᒐ);
        if (dec.ignoreDepth > maxIgnoreNestingDepth) {
            error_(errors.New(invalidNestingDepthˢ));
        }
        // If this type is already in progress, it's a recursive type (e.g. map[string]*T).
        // Return the pointer to the op we're already building.
        {
            var opPtr = inProgress[wireId]; if (opPtr != nil) {
                return opPtr;
            }
        }
        ref var op = ref heap<Action<ж<decInstr>, ж<decoderState>, reflectꓸValue>>(out var Ꮡop);
        (op, var ok) = decIgnoreOpMap[wireId, ꟷ];
        if (!ok) {
            inProgress[wireId] = Ꮡop;
            if (wireId == tInterface) {
                // Special case because it's a method: the ignored item might
                // define types and we need to record their state in the decoder.
                op = (ж<decInstr> i, ж<decoderState> state, reflectꓸValue value) => {
                    (~state).dec.ignoreInterface(state);
                };
                return Ꮡop;
            }
            // Special cases
            var wire = dec.wireType[wireId];
            switch (ᐧ) {
            case {} when wire == nil: {
                errorf("bad data: undefined type %s"u8, wireId.@string());
                break;
            }
            case {} when (~wire).ArrayT != nil: {
                var elemId = wire.Value.ArrayT.Value.Elem;
                var elemOp = Ꮡdec.decIgnoreOpFor(elemId, inProgress);
                var elemOpʗ1 = elemOp;
                var wireʗ1 = wire;
                op = (ж<decInstr> i, ж<decoderState> state, reflectꓸValue value) => {
                    (~state).dec.ignoreArray(state, elemOpʗ1.ValueSlot, (~(~wireʗ1).ArrayT).Len);
                };
                break;
            }
            case {} when (~wire).MapT != nil: {
                var keyId = dec.wireType[wireId].Value.MapT.Value.Key;
                var elemId = dec.wireType[wireId].Value.MapT.Value.Elem;
                var keyOp = Ꮡdec.decIgnoreOpFor(keyId, inProgress);
                var elemOp = Ꮡdec.decIgnoreOpFor(elemId, inProgress);
                var elemOpʗ2 = elemOp;
                var keyOpʗ1 = keyOp;
                op = (ж<decInstr> i, ж<decoderState> state, reflectꓸValue value) => {
                    (~state).dec.ignoreMap(state, keyOpʗ1.ValueSlot, elemOpʗ2.ValueSlot);
                };
                break;
            }
            case {} when (~wire).SliceT != nil: {
                var elemId = wire.Value.SliceT.Value.Elem;
                var elemOp = Ꮡdec.decIgnoreOpFor(elemId, inProgress);
                var elemOpʗ3 = elemOp;
                op = (ж<decInstr> i, ж<decoderState> state, reflectꓸValue value) => {
                    (~state).dec.ignoreSlice(state, elemOpʗ3.ValueSlot);
                };
                break;
            }
            case {} when (~wire).StructT != nil: {
                var (enginePtr, err) = Ꮡdec.getIgnoreEnginePtr(wireId);
                if (err != default!) {
                    // Generate a closure that calls out to the engine for the nested type.
                    error_(err);
                }
                var enginePtrʗ1 = enginePtr;
                op = (ж<decInstr> i, ж<decoderState> state, reflectꓸValue value) => {
                    // indirect through enginePtr to delay evaluation for recursive structs
                    (~state).dec.ignoreStruct(enginePtrʗ1.ValueSlot);
                };
                break;
            }
            case {} when ((~wire).GobEncoderT != nil) || ((~wire).BinaryMarshalerT != nil) || ((~wire).TextMarshalerT != nil): {
                op = (ж<decInstr> i, ж<decoderState> state, reflectꓸValue value) => {
                    (~state).dec.ignoreGobDecoder(state);
                };
                break;
            }}

        }
        if (op == default!) {
            errorf("bad data: ignore can't handle type %s"u8, wireId.@string());
        }
        return Ꮡop;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// gobDecodeOpFor returns the op for a type that is known to implement
// GobDecoder.
[GoRecv] internal static ж<Action<ж<decInstr>, ж<decoderState>, reflectꓸValue>> gobDecodeOpFor(this ref Decoder dec, ж<userTypeInfo> Ꮡut) {
    ref var ut = ref Ꮡut.DerefOrNull();

    var rcvrType = ut.user;
    if (ut.decIndir == -1){
        rcvrType = reflect.PointerTo(rcvrType);
    } else 
    if (ut.decIndir > 0) {
        for (var i = (int8)0; i < ut.decIndir; i++) {
            rcvrType = rcvrType.Elem();
        }
    }
    ref var op = ref heap<Action<ж<decInstr>, ж<decoderState>, reflectꓸValue>>(out var Ꮡop);
    var rcvrTypeʗ1 = rcvrType;
    op = (ж<decInstr> i, ж<decoderState> state, reflectꓸValue value) => {
        // We now have the base type. We need its address if the receiver is a pointer.
        if (value.Kind() != reflect.ΔPointer && rcvrTypeʗ1.Kind() == reflect.ΔPointer) {
            value = value.Addr();
        }
        (~state).dec.decodeGobDecoder(Ꮡut, state, value);
    };
    return Ꮡop;
}

// compatibleType asks: Are these two gob Types compatible?
// Answers the question for basic types, arrays, maps and slices, plus
// GobEncoder/Decoder pairs.
// Structs are considered ok; fields will be checked later.
[GoRecv] internal static bool compatibleType(this ref Decoder dec, reflectꓸType fr, typeId fw, map<reflectꓸType, typeId> inProgress) {
    {
        var (rhs, okΔ1) = inProgress[fr, ꟷ]; if (okΔ1) {
            return rhs == fw;
        }
    }
    inProgress[fr] = fw;
    var ut = userType(fr);
    var (wire, ok) = dec.wireType[fw, ꟷ];
    // If wire was encoded with an encoding method, fr must have that method.
    // And if not, it must not.
    // At most one of the booleans in ut is set.
    // We could possibly relax this constraint in the future in order to
    // choose the decoding method using the data in the wireType.
    // The parentheses look odd but are correct.
    if (((~ut).externalDec == xGob) != (ok && (~wire).GobEncoderT != nil) || ((~ut).externalDec == xBinary) != (ok && (~wire).BinaryMarshalerT != nil) || ((~ut).externalDec == xText) != (ok && (~wire).TextMarshalerT != nil)) {
        return false;
    }
    if ((~ut).externalDec != 0) {
        // This test trumps all others.
        return true;
    }
    {
        var t = ut.Value.@base;
        var exprᴛ1 = t.Kind();
        if (exprᴛ1 == reflect.ΔBool) {
            return fw == tBool;
        }
        if (exprᴛ1 == reflect.ΔInt || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64) {
            return fw == tInt;
        }
        if (exprᴛ1 == reflect.ΔUint || exprᴛ1 == reflect.Uint8 || exprᴛ1 == reflect.Uint16 || exprᴛ1 == reflect.Uint32 || exprᴛ1 == reflect.Uint64 || exprᴛ1 == reflect.Uintptr) {
            return fw == tUint;
        }
        if (exprᴛ1 == reflect.Float32 || exprᴛ1 == reflect.Float64) {
            return fw == tFloat;
        }
        if (exprᴛ1 == reflect.Complex64 || exprᴛ1 == reflect.Complex128) {
            return fw == tComplex;
        }
        if (exprᴛ1 == reflect.ΔString) {
            return fw == tString;
        }
        if (exprᴛ1 == reflect.ΔInterface) {
            return fw == tInterface;
        }
        if (exprᴛ1 == reflect.Array) {
            if (!ok || (~wire).ArrayT == nil) {
                // chan, etc: cannot handle.
                return false;
            }
            var Δarray = wire.Value.ArrayT;
            return t.Len() == (~Δarray).Len && dec.compatibleType(t.Elem(), (~Δarray).Elem, inProgress);
        }
        if (exprᴛ1 == reflect.Map) {
            if (!ok || (~wire).MapT == nil) {
                return false;
            }
            var MapType = wire.Value.MapT;
            return dec.compatibleType(t.Key(), (~MapType).Key, inProgress) && dec.compatibleType(t.Elem(), (~MapType).Elem, inProgress);
        }
        if (exprᴛ1 == reflect.ΔSlice) {
            if (t.Elem().Kind() == reflect.Uint8) {
                // Is it an array of bytes?
                return fw == tBytes;
            }
            // Extract and compare element types.
            ж<sliceType> sw = default!;
            {
                var tt = builtinIdToType(fw); if (tt != default!){
                    (sw, _) = tt._<ж<sliceType>>(ᐧ);
                } else 
                if (wire != nil) {
                    sw = wire.Value.SliceT;
                }
            }
            var elem = userType(t.Elem()).Value.@base;
            return sw != nil && dec.compatibleType(elem, (~sw).Elem, inProgress);
        }
        if (exprᴛ1 == reflect.Struct) {
            return true;
        }
        { /* default: */
            return false;
        }
    }

}

// typeString returns a human-readable description of the type identified by remoteId.
internal static @string typeString(this ж<Decoder> Ꮡdec, typeId remoteId) {
    GoFrame ᒐ = default;
    try {
        ref var dec = ref Ꮡdec.DerefOrNull();

        ᏑtypeLock.Lock();
        defer(ᏑtypeLock.Unlock, ref ᒐ);
        {
            var t = idToType(remoteId); if (t != default!) {
                // globally known type.
                return t.@string();
            }
        }
        return dec.wireType[remoteId].@string();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// compileSingle compiles the decoder engine for a non-struct top-level value, including
// GobDecoders.
internal static (ж<decEngine> engine, error err) compileSingle(this ж<Decoder> Ꮡdec, typeId remoteId, ж<userTypeInfo> Ꮡut) {
    ж<decEngine> engine = default!;
    error err = default!;

    ref var dec = ref Ꮡdec.DerefOrNull();
    ref var ut = ref Ꮡut.DerefOrNull();
    var rt = ut.user;
    engine = @new<decEngine>();
    engine.Value.instr = new slice<decInstr>(1); // one item
    @string name = rt.String(); // best we can do
    if (!dec.compatibleType(rt, remoteId, new map<reflectꓸType, typeId>())) {
        @string remoteType = Ꮡdec.typeString(remoteId);
        // Common confusing case: local interface type, remote concrete type.
        if (ut.@base.Kind() == reflect.ΔInterface && remoteId != tInterface) {
            return (default!, errors.New("gob: local interface type "u8 + name + " can only be decoded from remote interface type; received concrete type "u8 + remoteType));
        }
        return (default!, errors.New("gob: decoding into local type "u8 + name + ", received remote type "u8 + remoteType));
    }
    var op = Ꮡdec.decOpFor(remoteId, rt, name, new map<reflectꓸType, ж<Action<ж<decInstr>, ж<decoderState>, reflectꓸValue>>>());
    var ovfl = errors.New(@"value for """u8 + name + @""" out of range"u8);
    engine.Value.instr[singletonField] = new decInstr(op.ValueSlot, singletonField, default!, ovfl);
    engine.Value.numInstr = 1;
    return (engine, err);
}

// compileIgnoreSingle compiles the decoder engine for a non-struct top-level value that will be discarded.
internal static ж<decEngine> compileIgnoreSingle(this ж<Decoder> Ꮡdec, typeId remoteId) {
    var engine = @new<decEngine>();
    engine.Value.instr = new slice<decInstr>(1); // one item
    var op = Ꮡdec.decIgnoreOpFor(remoteId, new map<typeId, ж<Action<ж<decInstr>, ж<decoderState>, reflectꓸValue>>>());
    var ovfl = overflow(Ꮡdec.typeString(remoteId));
    engine.Value.instr[0] = new decInstr(op.ValueSlot, 0, default!, ovfl);
    engine.Value.numInstr = 1;
    return engine;
}

// compileDec compiles the decoder engine for a value. If the value is not a struct,
// it calls out to compileSingle.
internal static (ж<decEngine> engine, error err) compileDec(this ж<Decoder> Ꮡdec, typeId remoteId, ж<userTypeInfo> Ꮡut) {
    ж<decEngine> engine = default!;
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var dec = ref Ꮡdec.DerefOrNull();
        ref var ut = ref Ꮡut.DerefOrNull();

        ref var err = ref Ꮡerr.ValueSlot;
        defer(ᴛ1 => catchError(ref ᴛ1.DerefOrNull()), Ꮡerr, ref ᒐ);
        var rt = ut.@base;
        var srt = rt;
        if (srt.Kind() != reflect.Struct || ut.externalDec != 0) {
            (engine, err) = Ꮡdec.compileSingle(remoteId, Ꮡut); goto ᒐdone;
        }
        ж<structType> wireStruct = default!;
        // Builtin types can come from global pool; the rest must be defined by the decoder.
        // Also we know we're decoding a struct now, so the client must have sent one.
        {
            var t = builtinIdToType(remoteId); if (t != default!){
                (wireStruct, _) = t._<ж<structType>>(ᐧ);
            } else {
                var wire = dec.wireType[remoteId];
                if (wire == nil) {
                    error_(errBadType);
                }
                wireStruct = wire.Value.StructT;
            }
        }
        if (wireStruct == nil) {
            errorf("type mismatch in decoder: want struct type %s; got non-struct"u8, rt);
        }
        engine = @new<decEngine>();
        engine.Value.instr = new slice<decInstr>(len((~wireStruct).Field));
        var seen = new map<reflectꓸType, ж<Action<ж<decInstr>, ж<decoderState>, reflectꓸValue>>>();
        // Loop over the fields of the wire type.
        for (nint fieldnum = 0; fieldnum < len((~wireStruct).Field); fieldnum++) {
            var wireField = (~wireStruct).Field[fieldnum];
            if (wireField.Name == ""u8) {
                errorf("empty name for remote field of type %s"u8, (~wireStruct).Name);
            }
            var ovfl = overflow(wireField.Name);
            // Find the field of the local type with the same name.
            var (localField, present) = srt.FieldByName(wireField.Name);
            // TODO(r): anonymous names
            if (!present || !isExported(wireField.Name)) {
                var opΔ1 = Ꮡdec.decIgnoreOpFor(wireField.Id, new map<typeId, ж<Action<ж<decInstr>, ж<decoderState>, reflectꓸValue>>>());
                engine.Value.instr[fieldnum] = new decInstr(opΔ1.ValueSlot, fieldnum, default!, ovfl);
                continue;
            }
            if (!dec.compatibleType(localField.Type, wireField.Id, new map<reflectꓸType, typeId>())) {
                errorf("wrong type (%s) for received field %s.%s"u8, localField.Type, (~wireStruct).Name, wireField.Name);
            }
            var op = Ꮡdec.decOpFor(wireField.Id, localField.Type, localField.Name, seen);
            engine.Value.instr[fieldnum] = new decInstr(op.ValueSlot, fieldnum, localField.Index, ovfl);
            engine.Value.numInstr++;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (engine, Ꮡerr.ValueSlot);
}

// getDecEnginePtr returns the engine for the specified type.
internal static (ж<ж<decEngine>> enginePtr, error err) getDecEnginePtr(this ж<Decoder> Ꮡdec, typeId remoteId, ж<userTypeInfo> Ꮡut) {
    ж<ж<decEngine>> enginePtr = default!;
    error err = default!;

    ref var dec = ref Ꮡdec.DerefOrNull();
    ref var ut = ref Ꮡut.DerefOrNull();
    var rt = ut.user;
    var (decoderMap, ok) = dec.decoderCache[rt, ꟷ];
    if (!ok) {
        decoderMap = new map<typeId, ж<ж<decEngine>>>();
        dec.decoderCache[rt] = decoderMap;
    }
    {
        (enginePtr, ok) = decoderMap[remoteId, ꟷ]; if (!ok) {
            // To handle recursive types, mark this engine as underway before compiling.
            enginePtr = @new<ж<decEngine>>();
            decoderMap[remoteId] = enginePtr;
            (enginePtr.ValueSlot, err) = Ꮡdec.compileDec(remoteId, Ꮡut);
            if (err != default!) {
                delete(decoderMap, remoteId);
            }
        }
    }
    return (enginePtr, err);
}

// emptyStruct is the type we compile into when ignoring a struct value.
[GoType] partial struct emptyStruct {
}

internal static reflectꓸType emptyStructType = reflect.TypeFor<emptyStruct>();

// getIgnoreEnginePtr returns the engine for the specified type when the value is to be discarded.
internal static (ж<ж<decEngine>> enginePtr, error err) getIgnoreEnginePtr(this ж<Decoder> Ꮡdec, typeId wireId) {
    ж<ж<decEngine>> enginePtr = default!;
    error err = default!;

    ref var dec = ref Ꮡdec.DerefOrNull();
    bool ok = default!;
    {
        (enginePtr, ok) = dec.ignorerCache[wireId, ꟷ]; if (!ok) {
            // To handle recursive types, mark this engine as underway before compiling.
            enginePtr = @new<ж<decEngine>>();
            dec.ignorerCache[wireId] = enginePtr;
            var wire = dec.wireType[wireId];
            if (wire != nil && (~wire).StructT != nil){
                (enginePtr.ValueSlot, err) = Ꮡdec.compileDec(wireId, userType(emptyStructType));
            } else {
                enginePtr.ValueSlot = Ꮡdec.compileIgnoreSingle(wireId);
            }
            if (err != default!) {
                delete(dec.ignorerCache, wireId);
            }
        }
    }
    return (enginePtr, err);
}

// decodeValue decodes the data stream representing a value and stores it in value.
internal static void decodeValue(this ж<Decoder> Ꮡdec, typeId wireId, reflectꓸValue value) {
    GoFrame ᒐ = default;
    try {
        ref var dec = ref Ꮡdec.DerefOrNull();

        defer(ᴛ1 => catchError(ref ᴛ1.DerefOrNull()), Ꮡdec.of(Decoder.Ꮡerr), ref ᒐ);
        // If the value is nil, it means we should just ignore this item.
        if (!value.IsValid()) {
            Ꮡdec.decodeIgnoredValue(wireId);
            return;
        }
        // Dereference down to the underlying type.
        var ut = userType(value.Type());
        var @base = ut.Value.@base;
        ж<ж<decEngine>> enginePtr = default!;
        (enginePtr, dec.err) = Ꮡdec.getDecEnginePtr(wireId, ut);
        if (dec.err != default!) {
            return;
        }
        value = decAlloc(value);
        var engine = enginePtr.ValueSlot;
        {
            var st = @base; if (st.Kind() == reflect.Struct && (~ut).externalDec == 0){
                var wt = dec.wireType[wireId];
                if ((~engine).numInstr == 0 && st.NumField() > 0 && wt != nil && len((~(~wt).StructT).Field) > 0) {
                    @string name = @base.Name();
                    errorf("type mismatch: no fields matched compiling decoder for %s"u8, name);
                }
                Ꮡdec.decodeStruct(engine, value);
            } else {
                Ꮡdec.decodeSingle(engine, value);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// decodeIgnoredValue decodes the data stream representing a value of the specified type and discards it.
internal static void decodeIgnoredValue(this ж<Decoder> Ꮡdec, typeId wireId) {
    ref var dec = ref Ꮡdec.DerefOrNull();

    ж<ж<decEngine>> enginePtr = default!;
    (enginePtr, dec.err) = Ꮡdec.getIgnoreEnginePtr(wireId);
    if (dec.err != default!) {
        return;
    }
    var wire = dec.wireType[wireId];
    if (wire != nil && (~wire).StructT != nil){
        Ꮡdec.ignoreStruct(enginePtr.ValueSlot);
    } else {
        Ꮡdec.ignoreSingle(enginePtr.ValueSlot);
    }
}

internal static UntypedInt intBits => /* 32 << (^uint(0) >> 63) */ 64;
internal static UntypedInt uintptrBits => /* 32 << (^uintptr(0) >> 63) */ 64;

[GoInit] internal static void init() {
    Action<ж<decInstr>, ж<decoderState>, reflectꓸValue> iop = default!;
    Action<ж<decInstr>, ж<decoderState>, reflectꓸValue> uop = default!;
    var exprᴛ1 = intBits;
    if (exprᴛ1 == 32) {
        iop = decInt32;
        uop = decUint32;
    }
    else if (exprᴛ1 == 64) {
        iop = decInt64;
        uop = decUint64;
    }
    else { /* default: */
        throw panic("gob: unknown size of int/uint");
    }

    decOpTable[reflect.ΔInt] = iop;
    decOpTable[reflect.ΔUint] = uop;
    // Finally uintptr
    var exprᴛ2 = uintptrBits;
    if (exprᴛ2 == 32) {
        uop = decUint32;
    }
    else if (exprᴛ2 == 64) {
        uop = decUint64;
    }
    else { /* default: */
        throw panic("gob: unknown size of uintptr");
    }

    decOpTable[reflect.Uintptr] = uop;
}

// Gob depends on being able to take the address
// of zeroed Values it creates, so use this wrapper instead
// of the standard reflect.Zero.
// Each call allocates once.
internal static reflectꓸValue allocValue(reflectꓸType t) {
    return reflect.New(t).Elem();
}

} // end gob_package
