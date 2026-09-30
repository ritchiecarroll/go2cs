// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:generate go run encgen.go -output enc_helpers.go
namespace go.encoding;

using encoding = encoding_package;
using binary = go.encoding.binary_package;
using math = math_package;
using bits = go.math.bits_package;
using reflect = reflect_package;
using sync = sync_package;
using go.encoding;
using go.math;
using go.sync;
using io = io_package;

partial class gob_package {

internal static UntypedInt uint64Size => 8;

// type encHelper is a methodless func type — rendered inline as its base delegate

// encoderState is the global execution state of an instance of the encoder.
// Field numbers are delta encoded and always increase. The field
// number is initialized to -1 so 0 comes out as delta(1). A delta of
// 0 terminates the structure.
[GoType] partial struct encoderState {
    internal ж<Encoder> enc;
    internal ж<encBuffer> b;
    internal bool sendZero;                 // encoding an array element or map key/value pair; send zero values
    internal nint fieldnum;                 // the last field number written.
    internal array<byte> buf = new(1 + uint64Size); // buffer used by the encoder; here to avoid allocation.
    internal ж<encoderState> next;     // for free list
}

// encBuffer is an extremely simple, fast implementation of a write-only byte buffer.
// It never returns a non-nil error, but Write returns an error value so it matches io.Writer.
[GoType] partial struct encBuffer {
    internal slice<byte> data;
    internal array<byte> scratch = new(64);
}

internal static ж<sync.Pool> ᏑencBufferPool = new StandardBox<sync.Pool>(new sync.Pool(
    New: () => {
        var e = @new<encBuffer>();
        e.Value.data = (~e).scratch[0..0];
        return e.OrTypedNil();
    }
));
internal static ref sync.Pool encBufferPool => ref ᏑencBufferPool.Value;

[GoRecv] internal static void writeByte(this ref encBuffer e, byte c) {
    e.data = append(e.data, c);
}

[GoRecv] internal static (nint, error) Write(this ref encBuffer e, slice<byte> p) {
    e.data = appendꓸꓸꓸ(e.data, p);
    return (len(p), default!);
}

[GoRecv] internal static void WriteString(this ref encBuffer e, @string s) {
    e.data = append(e.data, s.ꓸꓸꓸ);
}

[GoRecv] internal static nint Len(this ref encBuffer e) {
    return len(e.data);
}

[GoRecv] internal static slice<byte> Bytes(this ref encBuffer e) {
    return e.data;
}

[GoRecv] internal static void Reset(this ref encBuffer e) {
    if (len(e.data) >= tooBig){
        e.data = e.scratch[0..0];
    } else {
        e.data = e.data[0..0];
    }
}

internal static ж<encoderState> newEncoderState(this ж<Encoder> Ꮡenc, ж<encBuffer> Ꮡb) {
    ref var enc = ref Ꮡenc.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    var e = enc.freeList;
    if (e == nil){
        e = @new<encoderState>();
        e.Value.enc = Ꮡenc;
    } else {
        enc.freeList = e.Value.next;
    }
    e.Value.sendZero = false;
    e.Value.fieldnum = 0;
    e.Value.b = Ꮡb;
    if (len(b.data) == 0) {
        b.data = b.scratch[0..0];
    }
    return e;
}

[GoRecv] internal static void freeEncoderState(this ref Encoder enc, ж<encoderState> Ꮡe) {
    ref var e = ref Ꮡe.DerefOrNull();

    e.next = enc.freeList;
    enc.freeList = Ꮡe;
}

// Unsigned integers have a two-state encoding. If the number is less
// than 128 (0 through 0x7F), its value is written directly.
// Otherwise the value is written in big-endian byte order preceded
// by the byte length, negated.

// encodeUint writes an encoded unsigned integer to state.b.
[GoRecv] internal static void encodeUint(this ref encoderState state, uint64 x) {
    if (x <= 0x7F) {
        state.b.writeByte((uint8)x);
        return;
    }
    binary.BigEndian.PutUint64(state.buf[1..], x);
    nint bc = (bits.LeadingZeros64(x) >> (int)(3)); // 8 - bytelen(x)
    state.buf[bc] = (uint8)(bc - (nint)uint64Size); // and then we subtract 8 to get -bytelen(x)
    state.b.Write(state.buf.slice(bc, uint64Size + 1));
}

// encodeInt writes an encoded signed integer to state.w.
// The low bit of the encoding says whether to bit complement the (other bits of the)
// uint to recover the int.
[GoRecv] internal static void encodeInt(this ref encoderState state, int64 i) {
    uint64 x = default!;
    if (i < 0){
        x = (uint64)((uint64)((~i << (int)(1))) | 1);
    } else {
        x = (uint64)((i << (int)(1)));
    }
    state.encodeUint(x);
}

// type encOp is a methodless func type — rendered inline as its base delegate

// The 'instructions' of the encoding machine
[GoType] partial struct encInstr {
    internal Action<ж<encInstr>, ж<encoderState>, reflectꓸValue> op;
    internal nint field;  // field number in input
    internal slice<nint> index; // struct index
    internal nint indir;  // how many pointer indirections to reach the value in the struct
}

// update emits a field number and updates the state to record its value for delta encoding.
// If the instruction pointer is nil, it does nothing
[GoRecv] internal static void update(this ref encoderState state, ж<encInstr> Ꮡinstr) {
    ref var instr = ref Ꮡinstr.DerefOrNull();

    if (Ꮡinstr != nil) {
        state.encodeUint((uint64)(instr.field - state.fieldnum));
        state.fieldnum = instr.field;
    }
}

// Each encoder for a composite is responsible for handling any
// indirections associated with the elements of the data structure.
// If any pointer so reached is nil, no bytes are written. If the
// data item is zero, no bytes are written. Single values - ints,
// strings etc. - are indirected before calling their encoders.
// Otherwise, the output (for a scalar) is the field number, as an
// encoded integer, followed by the field data in its appropriate
// format.

// encIndirect dereferences pv indir times and returns the result.
internal static reflectꓸValue encIndirect(reflectꓸValue pv, nint indir) {
    for (; indir > 0; indir--) {
        if (pv.IsNil()) {
            break;
        }
        pv = pv.Elem();
    }
    return pv;
}

// encBool encodes the bool referenced by v as an unsigned 0 or 1.
internal static void encBool(ж<encInstr> Ꮡi, ж<encoderState> Ꮡstate, reflectꓸValue v) {
    ref var state = ref Ꮡstate.DerefOrNull();

    var b = v.Bool();
    if (b || state.sendZero) {
        state.update(Ꮡi);
        if (b){
            state.encodeUint(1);
        } else {
            state.encodeUint(0);
        }
    }
}

// encInt encodes the signed integer (int int8 int16 int32 int64) referenced by v.
internal static void encInt(ж<encInstr> Ꮡi, ж<encoderState> Ꮡstate, reflectꓸValue v) {
    ref var state = ref Ꮡstate.DerefOrNull();

    var value = v.Int();
    if (value != 0 || state.sendZero) {
        state.update(Ꮡi);
        state.encodeInt(value);
    }
}

// encUint encodes the unsigned integer (uint uint8 uint16 uint32 uint64 uintptr) referenced by v.
internal static void encUint(ж<encInstr> Ꮡi, ж<encoderState> Ꮡstate, reflectꓸValue v) {
    ref var state = ref Ꮡstate.DerefOrNull();

    var value = v.Uint();
    if (value != 0 || state.sendZero) {
        state.update(Ꮡi);
        state.encodeUint(value);
    }
}

// floatBits returns a uint64 holding the bits of a floating-point number.
// Floating-point numbers are transmitted as uint64s holding the bits
// of the underlying representation. They are sent byte-reversed, with
// the exponent end coming out first, so integer floating point numbers
// (for example) transmit more compactly. This routine does the
// swizzling.
internal static uint64 floatBits(float64 f) {
    var u = math.Float64bits(f);
    return bits.ReverseBytes64(u);
}

// encFloat encodes the floating point value (float32 float64) referenced by v.
internal static void encFloat(ж<encInstr> Ꮡi, ж<encoderState> Ꮡstate, reflectꓸValue v) {
    ref var state = ref Ꮡstate.DerefOrNull();

    var f = v.Float();
    if (f != 0D || state.sendZero) {
        var bits = floatBits(f);
        state.update(Ꮡi);
        state.encodeUint(bits);
    }
}

// encComplex encodes the complex value (complex64 complex128) referenced by v.
// Complex numbers are just a pair of floating-point numbers, real part first.
internal static void encComplex(ж<encInstr> Ꮡi, ж<encoderState> Ꮡstate, reflectꓸValue v) {
    ref var i = ref Ꮡi.DerefOrNull();
    ref var state = ref Ꮡstate.DerefOrNull();

    var c = v.Complex();
    if (c != 0D.i() || state.sendZero) {
        var rpart = floatBits(real(c));
        var ipart = floatBits(imag(c));
        state.update(Ꮡi);
        state.encodeUint(rpart);
        state.encodeUint(ipart);
    }
}

// encUint8Array encodes the byte array referenced by v.
// Byte arrays are encoded as an unsigned count followed by the raw bytes.
internal static void encUint8Array(ж<encInstr> Ꮡi, ж<encoderState> Ꮡstate, reflectꓸValue v) {
    ref var state = ref Ꮡstate.DerefOrNull();

    var b = v.Bytes();
    if (len(b) > 0 || state.sendZero) {
        state.update(Ꮡi);
        state.encodeUint((uint64)len(b));
        state.b.Write(b);
    }
}

// encString encodes the string referenced by v.
// Strings are encoded as an unsigned count followed by the raw bytes.
internal static void encString(ж<encInstr> Ꮡi, ж<encoderState> Ꮡstate, reflectꓸValue v) {
    ref var state = ref Ꮡstate.DerefOrNull();

    @string s = v.String();
    if (len(s) > 0 || state.sendZero) {
        state.update(Ꮡi);
        state.encodeUint((uint64)len(s));
        state.b.WriteString(s);
    }
}

// encStructTerminator encodes the end of an encoded struct
// as delta field number of 0.
internal static void encStructTerminator(ж<encInstr> Ꮡi, ж<encoderState> Ꮡstate, reflectꓸValue v) {
    ref var state = ref Ꮡstate.DerefOrNull();

    state.encodeUint(0);
}

// Execution engine

// encEngine an array of instructions indexed by field number of the encoding
// data, typically a struct. It is executed top to bottom, walking the struct.
[GoType] partial struct encEngine {
    internal slice<encInstr> instr;
}

internal static UntypedInt singletonField => 0;

// valid reports whether the value is valid and a non-nil pointer.
// (Slices, maps, and chans take care of themselves.)
internal static bool valid(reflectꓸValue v) {
    var exprᴛ1 = v.Kind();
    if (exprᴛ1 == reflect.Invalid) {
        return false;
    }
    if (exprᴛ1 == reflect.ΔPointer) {
        return !v.IsNil();
    }

    return true;
}

// encodeSingle encodes a single top-level non-struct value.
internal static void encodeSingle(this ж<Encoder> Ꮡenc, ж<encBuffer> Ꮡb, ж<encEngine> Ꮡengine, reflectꓸValue value) {
    GoFrame ᒐ = default;
    try {
        ref var engine = ref Ꮡengine.DerefOrNull();

        var state = Ꮡenc.newEncoderState(Ꮡb);
        defer(Ꮡenc.freeEncoderState, state, ref ᒐ);
        state.Value.fieldnum = singletonField;
        // There is no surrounding struct to frame the transmission, so we must
        // generate data even if the item is zero. To do this, set sendZero.
        state.Value.sendZero = true;
        var instr = Ꮡ(engine.instr, singletonField);
        if ((~instr).indir > 0) {
            value = encIndirect(value, (~instr).indir);
        }
        if (valid(value)) {
            (~instr).op(instr, state, value);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// encodeStruct encodes a single struct value.
internal static void encodeStruct(this ж<Encoder> Ꮡenc, ж<encBuffer> Ꮡb, ж<encEngine> Ꮡengine, reflectꓸValue value) {
    GoFrame ᒐ = default;
    try {
        ref var engine = ref Ꮡengine.DerefOrNull();

        if (!valid(value)) {
            return;
        }
        var state = Ꮡenc.newEncoderState(Ꮡb);
        defer(Ꮡenc.freeEncoderState, state, ref ᒐ);
        state.Value.fieldnum = -1;
        for (nint i = 0; i < len(engine.instr); i++) {
            var instr = Ꮡ(engine.instr, i);
            if (i >= value.NumField()) {
                // encStructTerminator
                (~instr).op(instr, state, new reflectꓸValue(nil));
                break;
            }
            var field = value.FieldByIndex((~instr).index);
            if ((~instr).indir > 0) {
                field = encIndirect(field, (~instr).indir);
                // TODO: Is field guaranteed valid? If so we could avoid this check.
                if (!valid(field)) {
                    continue;
                }
            }
            (~instr).op(instr, state, field);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// encodeArray encodes an array.
internal static void encodeArray(this ж<Encoder> Ꮡenc, ж<encBuffer> Ꮡb, reflectꓸValue value, Action<ж<encInstr>, ж<encoderState>, reflectꓸValue> op, nint elemIndir, nint length, Func<ж<encoderState>, reflectꓸValue, bool> helper) {
    GoFrame ᒐ = default;
    try {
        var state = Ꮡenc.newEncoderState(Ꮡb);
        defer(Ꮡenc.freeEncoderState, state, ref ᒐ);
        state.Value.fieldnum = -1;
        state.Value.sendZero = true;
        state.encodeUint((uint64)length);
        if (helper != default! && helper(state, value)) {
            return;
        }
        for (nint i = 0; i < length; i++) {
            var elem = value.Index(i);
            if (elemIndir > 0) {
                elem = encIndirect(elem, elemIndir);
                // TODO: Is elem guaranteed valid? If so we could avoid this check.
                if (!valid(elem)) {
                    errorf("encodeArray: nil element"u8);
                }
            }
            op(nil, state, elem);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// encodeReflectValue is a helper for maps. It encodes the value v.
internal static void encodeReflectValue(ж<encoderState> Ꮡstate, reflectꓸValue v, Action<ж<encInstr>, ж<encoderState>, reflectꓸValue> op, nint indir) {
    for (nint i = 0; i < indir && v.IsValid(); i++) {
        v = reflect.Indirect(v);
    }
    if (!v.IsValid()) {
        errorf("encodeReflectValue: nil element"u8);
    }
    op(nil, Ꮡstate, v);
}

// encodeMap encodes a map as unsigned count followed by key:value pairs.
internal static void encodeMap(this ж<Encoder> Ꮡenc, ж<encBuffer> Ꮡb, reflectꓸValue mv, Action<ж<encInstr>, ж<encoderState>, reflectꓸValue> keyOp, Action<ж<encInstr>, ж<encoderState>, reflectꓸValue> elemOp, nint keyIndir, nint elemIndir) {
    ref var enc = ref Ꮡenc.DerefOrNull();

    var state = Ꮡenc.newEncoderState(Ꮡb);
    state.Value.fieldnum = -1;
    state.Value.sendZero = true;
    state.encodeUint((uint64)mv.Len());
    var mi = mv.MapRange();
    while (mi.Next()) {
        encodeReflectValue(state, mi.Key(), keyOp, keyIndir);
        encodeReflectValue(state, mi.Value(), elemOp, elemIndir);
    }
    enc.freeEncoderState(state);
}

// encodeInterface encodes the interface value iv.
// To send an interface, we send a string identifying the concrete type, followed
// by the type identifier (which might require defining that type right now), followed
// by the concrete value. A nil value gets sent as the empty string for the name,
// followed by no value.
internal static void encodeInterface(this ж<Encoder> Ꮡenc, ж<encBuffer> Ꮡb, reflectꓸValue iv) {
    ref var enc = ref Ꮡenc.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    // Gobs can encode nil interface values but not typed interface
    // values holding nil pointers, since nil pointers point to no value.
    var elem = iv.Elem();
    if (elem.Kind() == reflect.ΔPointer && elem.IsNil()) {
        errorf("gob: cannot encode nil pointer of type %s inside interface"u8, iv.Elem().Type());
    }
    var state = Ꮡenc.newEncoderState(Ꮡb);
    state.Value.fieldnum = -1;
    state.Value.sendZero = true;
    if (iv.IsNil()) {
        state.encodeUint(0);
        return;
    }
    var ut = userType(iv.Elem().Type());
    var (namei, ok) = ᏑconcreteTypeToName.Load((~ut).@base);
    if (!ok) {
        errorf("type not registered for interface: %s"u8, (~ut).@base);
    }
    @string name = namei._<@string>();
    // Send the name.
    state.encodeUint((uint64)len(name));
    (~state).b.WriteString(name);
    // Define the type id if necessary.
    Ꮡenc.sendTypeDescriptor(enc.writer(), state, ut);
    // Send the type id.
    enc.sendTypeId(state, ut);
    // Encode the value into a new buffer. Any nested type definitions
    // should be written to b, before the encoded value.
    enc.pushWriter(new encBufferжWriter(Ꮡb));
    var data = ᏑencBufferPool.Get()._<ж<encBuffer>>();
    data.Write(spaceForLength);
    Ꮡenc.encode(data, elem, ut);
    if (enc.err != default!) {
        error_(enc.err);
    }
    enc.popWriter();
    enc.writeMessage(new encBufferжWriter(Ꮡb), data);
    data.Reset();
    ᏑencBufferPool.Put(data.OrTypedNil());
    if (enc.err != default!) {
        error_(enc.err);
    }
    enc.freeEncoderState(state);
}

// encodeGobEncoder encodes a value that implements the GobEncoder interface.
// The data is sent as a byte array.
internal static void encodeGobEncoder(this ж<Encoder> Ꮡenc, ж<encBuffer> Ꮡb, ж<userTypeInfo> Ꮡut, reflectꓸValue v) {
    ref var enc = ref Ꮡenc.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();
    ref var ut = ref Ꮡut.DerefOrNull();

    // TODO: should we catch panics from the called method?
    slice<byte> data = default!;
    error err = default!;
    // We know it's one of these.
    var exprᴛ1 = ut.externalEnc;
    if (exprᴛ1 == xGob) {
        (data, err) = v.Interface()._<GobEncoder>().GobEncode();
    }
    else if (exprᴛ1 == xBinary) {
        (data, err) = v.Interface()._<encoding.BinaryMarshaler>().MarshalBinary();
    }
    else if (exprᴛ1 == xText) {
        (data, err) = v.Interface()._<encoding.TextMarshaler>().MarshalText();
    }

    if (err != default!) {
        error_(err);
    }
    var state = Ꮡenc.newEncoderState(Ꮡb);
    state.Value.fieldnum = -1;
    state.encodeUint((uint64)len(data));
    (~state).b.Write(data);
    enc.freeEncoderState(state);
}

internal static array<Action<ж<encInstr>, ж<encoderState>, reflectꓸValue>> encOpTable = new golib.SparseArray<Action<ж<encInstr>, ж<encoderState>, reflectꓸValue>>{
    [1] = encBool,
    [2] = encInt,
    [3] = encInt,
    [4] = encInt,
    [5] = encInt,
    [6] = encInt,
    [7] = encUint,
    [8] = encUint,
    [9] = encUint,
    [10] = encUint,
    [11] = encUint,
    [12] = encUint,
    [13] = encFloat,
    [14] = encFloat,
    [15] = encComplex,
    [16] = encComplex,
    [24] = encString
}.array(25);

// encOpFor returns (a pointer to) the encoding op for the base type under rt and
// the indirection count to reach it.
internal static (ж<Action<ж<encInstr>, ж<encoderState>, reflectꓸValue>>, nint) encOpFor(reflectꓸType rt, map<reflectꓸType, ж<Action<ж<encInstr>, ж<encoderState>, reflectꓸValue>>> inProgress, map<ж<typeInfo>, bool> building) {
    var ut = userType(rt);
    // If the type implements GobEncoder, we handle it without further processing.
    if ((~ut).externalEnc != 0) {
        return gobEncodeOpFor(ut);
    }
    // If this type is already in progress, it's a recursive type (e.g. map[string]*T).
    // Return the pointer to the op we're already building.
    {
        var opPtr = inProgress[rt]; if (opPtr != nil) {
            return (opPtr, (~ut).indir);
        }
    }
    var typ = ut.Value.@base;
    nint indir = ut.Value.indir;
    reflectꓸKind k = typ.Kind();
    ref var op = ref heap<Action<ж<encInstr>, ж<encoderState>, reflectꓸValue>>(out var Ꮡop);
    if ((nint)(nuint)k < len(encOpTable)) {
        op = encOpTable[k];
    }
    if (op == default!) {
        inProgress[rt] = Ꮡop;
        // Special cases
        {
            var t = typ;
            var exprᴛ1 = t.Kind();
            if (exprᴛ1 == reflect.ΔSlice) {
                do {
                    if (t.Elem().Kind() == reflect.Uint8) {
                        op = encUint8Array;
                        break;
                    }
                    var (elemOp, elemIndir) = encOpFor(t.Elem(), // Slices have a header; we decode it to find the underlying array.
 inProgress, building);
                    var helper = encSliceHelper[t.Elem().Kind()];
                    var elemOpʗ1 = elemOp;
                    var helperʗ1 = helper;
                    op = (ж<encInstr> i, ж<encoderState> state, reflectꓸValue Δslice) => {
                        if (!(~state).sendZero && Δslice.Len() == 0) {
                            return;
                        }
                        state.update(i);
                        (~state).enc.encodeArray((~state).b, Δslice, elemOpʗ1.ValueSlot, elemIndir, Δslice.Len(), helperʗ1);
                    };
                } while (false);
            }
            else if (exprᴛ1 == reflect.Array) {
                var (elemOp, elemIndir) = encOpFor(t.Elem(), // True arrays have size in the type.
 inProgress, building);
                var helper = encArrayHelper[t.Elem().Kind()];
                var elemOpʗ2 = elemOp;
                var helperʗ2 = helper;
                op = (ж<encInstr> i, ж<encoderState> state, reflectꓸValue Δarray) => {
                    state.update(i);
                    (~state).enc.encodeArray((~state).b, Δarray, elemOpʗ2.ValueSlot, elemIndir, Δarray.Len(), helperʗ2);
                };
            }
            else if (exprᴛ1 == reflect.Map) {
                var (keyOp, keyIndir) = encOpFor(t.Key(), inProgress, building);
                var (elemOp, elemIndir) = encOpFor(t.Elem(), inProgress, building);
                var elemOpʗ3 = elemOp;
                var keyOpʗ1 = keyOp;
                op = (ж<encInstr> i, ж<encoderState> state, reflectꓸValue mv) => {
                    // We send zero-length (but non-nil) maps because the
                    // receiver might want to use the map.  (Maps don't use append.)
                    if (!(~state).sendZero && mv.IsNil()) {
                        return;
                    }
                    state.update(i);
                    (~state).enc.encodeMap((~state).b, mv, keyOpʗ1.ValueSlot, elemOpʗ3.ValueSlot, keyIndir, elemIndir);
                };
            }
            else if (exprᴛ1 == reflect.Struct) {
                getEncEngine(ref (userType(typ)).DerefOrNull(), // Generate a closure that calls out to the engine for the nested type.
 building);
                var info = mustGetTypeInfo(typ);
                var infoʗ1 = info;
                op = (ж<encInstr> i, ж<encoderState> state, reflectꓸValue sv) => {
                    state.update(i);
                    // indirect through info to delay evaluation for recursive structs
                    var enc = infoʗ1.of(typeInfo.Ꮡencoder).Load();
                    (~state).enc.encodeStruct((~state).b, enc, sv);
                };
            }
            else if (exprᴛ1 == reflect.ΔInterface) {
                op = (ж<encInstr> i, ж<encoderState> state, reflectꓸValue iv) => {
                    if (!(~state).sendZero && (!iv.IsValid() || iv.IsNil())) {
                        return;
                    }
                    state.update(i);
                    (~state).enc.encodeInterface((~state).b, iv);
                };
            }
        }

    }
    if (op == default!) {
        errorf("can't happen: encode type %s"u8, rt);
    }
    return (Ꮡop, indir);
}

// gobEncodeOpFor returns the op for a type that is known to implement GobEncoder.
internal static (ж<Action<ж<encInstr>, ж<encoderState>, reflectꓸValue>>, nint) gobEncodeOpFor(ж<userTypeInfo> Ꮡut) {
    ref var ut = ref Ꮡut.DerefOrNull();

    var rt = ut.user;
    if (ut.encIndir == -1){
        rt = reflect.PointerTo(rt);
    } else 
    if (ut.encIndir > 0) {
        for (var i = (int8)0; i < ut.encIndir; i++) {
            rt = rt.Elem();
        }
    }
    ref var op = ref heap<Action<ж<encInstr>, ж<encoderState>, reflectꓸValue>>(out var Ꮡop);
    var rtʗ1 = rt;
    op = (ж<encInstr> i, ж<encoderState> state, reflectꓸValue v) => {
        if (Ꮡut.Value.encIndir == -1) {
            // Need to climb up one level to turn value into pointer.
            if (!v.CanAddr()) {
                errorf("unaddressable value of type %s"u8, rtʗ1);
            }
            v = v.Addr();
        }
        if (!(~state).sendZero && v.IsZero()) {
            return;
        }
        state.update(i);
        (~state).enc.encodeGobEncoder((~state).b, Ꮡut, v);
    };
    return (Ꮡop, (nint)ut.encIndir); // encIndir: op will get called with p == address of receiver.
}

// compileEnc returns the engine to compile the type.
internal static ж<encEngine> compileEnc(ref userTypeInfo ut, map<ж<typeInfo>, bool> building) {
    var srt = ut.@base;
    var engine = @new<encEngine>();
    var seen = new map<reflectꓸType, ж<Action<ж<encInstr>, ж<encoderState>, reflectꓸValue>>>();
    var rt = ut.@base;
    if (ut.externalEnc != 0) {
        rt = ut.user;
    }
    if (ut.externalEnc == 0 && srt.Kind() == reflect.Struct){
        for ((nint fieldNum, nint wireFieldNum) = (0, 0); fieldNum < srt.NumField(); fieldNum++) {
            var f = srt.Field(fieldNum);
            if (!isSent(ref f)) {
                continue;
            }
            var (op, indir) = encOpFor(f.Type, seen, building);
            engine.Value.instr = append((~engine).instr, new encInstr(op.ValueSlot, wireFieldNum, f.Index, indir));
            wireFieldNum++;
        }
        if (srt.NumField() > 0 && len((~engine).instr) == 0) {
            errorf("type %s has no exported fields"u8, rt);
        }
        engine.Value.instr = append((~engine).instr, new encInstr(new Action<ж<encInstr>, ж<encoderState>, reflectꓸValue>(encStructTerminator), 0, default!, 0));
    } else {
        engine.Value.instr = new slice<encInstr>(1);
        var (op, indir) = encOpFor(rt, seen, building);
        engine.Value.instr[0] = new encInstr(op.ValueSlot, singletonField, default!, indir);
    }
    return engine;
}

// getEncEngine returns the engine to compile the type.
internal static ж<encEngine> getEncEngine(ref userTypeInfo ut, map<ж<typeInfo>, bool> building) {
    var (info, err) = getTypeInfo(ref ut);
    if (err != default!) {
        error_(err);
    }
    var enc = info.of(typeInfo.Ꮡencoder).Load();
    if (enc == nil) {
        enc = buildEncEngine(info, ref ut, building);
    }
    return enc;
}

internal static ж<encEngine> buildEncEngine(ж<typeInfo> Ꮡinfo, ref userTypeInfo ut, map<ж<typeInfo>, bool> building) {
    GoFrame ᒐ = default;
    try {
        ref var info = ref Ꮡinfo.DerefOrNull();

        // Check for recursive types.
        if (building != default! && building[Ꮡinfo]) {
            return default!;
        }
        info.encInit.Lock();
        defer(Ꮡinfo.of(typeInfo.ᏑencInit).Unlock, ref ᒐ);
        var enc = Ꮡinfo.of(typeInfo.Ꮡencoder).Load();
        if (enc == nil) {
            if (building == default!) {
                building = new map<ж<typeInfo>, bool>();
            }
            building[Ꮡinfo] = true;
            enc = compileEnc(ref ut, building);
            Ꮡinfo.of(typeInfo.Ꮡencoder).Store(enc);
        }
        return enc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static void encode(this ж<Encoder> Ꮡenc, ж<encBuffer> Ꮡb, reflectꓸValue value, ж<userTypeInfo> Ꮡut) {
    GoFrame ᒐ = default;
    try {
        ref var ut = ref Ꮡut.DerefOrNull();

        defer(ᴛ1 => catchError(ref ᴛ1.DerefOrNull()), Ꮡenc.of(Encoder.Ꮡerr), ref ᒐ);
        var engine = getEncEngine(ref (Ꮡut).DerefOrNull(), default!);
        nint indir = ut.indir;
        if (ut.externalEnc != 0) {
            indir = (nint)ut.encIndir;
        }
        for (nint i = 0; i < indir; i++) {
            value = reflect.Indirect(value);
        }
        if (ut.externalEnc == 0 && value.Type().Kind() == reflect.Struct){
            Ꮡenc.encodeStruct(Ꮡb, engine, value);
        } else {
            Ꮡenc.encodeSingle(Ꮡb, engine, value);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end gob_package
