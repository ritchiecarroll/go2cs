// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using bytes = bytes_package;
using errors = errors_package;
using flag = flag_package;
using math = math_package;
using rand = go.math.rand_package;
using reflect = reflect_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using @unsafe = unsafe_package;
using go.math;
using io = io_package;
using static go.encoding.gob_package;
using ꓸꓸꓸany = Span<any>;

partial class gob_internal_test_package {

internal static ж<bool> doFuzzTests = flag.Bool("gob.fuzz", false, "run the fuzz tests, which are large and very slow");

// Guarantee encoding format by comparing some encodings to hand-written values
public partial struct EncodeT {
    internal uint64 x;
    internal slice<byte> b;
}

internal static slice<EncodeT> encodeT = [
    new(0x00, [0x00]),
    new(0x0F, [0x0F]),
    new(0xFF, [0xFF, 0xFF]),
    new(0xFFFF, [0xFE, 0xFF, 0xFF]),
    new(0xFFFFFF, [0xFD, 0xFF, 0xFF, 0xFF]),
    new(0xFFFFFFFFU, [0xFC, 0xFF, 0xFF, 0xFF, 0xFF]),
    new(0xFFFFFFFFFFUL, [0xFB, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF]),
    new(0xFFFFFFFFFFFFUL, [0xFA, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF]),
    new(0xFFFFFFFFFFFFFFUL, [0xF9, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF]),
    new(0xFFFFFFFFFFFFFFFFUL, [0xF8, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF]),
    new(0x1111, [0xFE, 0x11, 0x11]),
    new(0x1111111111111111UL, [0xF8, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11]),
    new(0x8888888888888888UL, [0xF8, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88]),
    new((uint64)1 << 63, [0xF8, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00])
];

// testError is meant to be used as a deferred function to turn a panic(gobError) into a
// plain test.Error call.
internal static partial void testError(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var calledByDefer = GoFrame.TakeDeferredCall(); // Go: is this call the deferred one?

        var e = recover(calledByDefer); if (e != nil) {
            t.Error(e._<gobError>().err); // Will re-panic if not one of our errors, such as a runtime error.
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static ж<global::go.encoding.gob_package.decBuffer> newDecBuffer(slice<byte> data) {
    return Ꮡ(new decBuffer(
        data: data
    ));
}

// Test basic encode/decode routines for unsigned integers
public static void TestUintCodec(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(testError, t, ref ᒐ, recovers: true);
        var b = @new<global::go.encoding.gob_package.encBuffer>();
        var encState = newEncoderState(b);
        foreach (var (_, tt) in range(encodeT)) {
            b.Reset();
            encState.encodeUint(tt.x);
            if (!bytes.Equal(tt.b, b.Bytes())) {
                t.Errorf("encodeUint: %#x encode: expected % x got % x", tt.x, tt.b, b.Bytes());
            }
        }
        for (var u = (uint64)0; ; u = (u + 1) * 7) {
            b.Reset();
            encState.encodeUint(u);
            var decState = newDecodeState(newDecBuffer(b.Bytes()));
            var v = decState.decodeUint();
            if (u != v) {
                t.Errorf("Encode/Decode: sent %#x received %#x", u, v);
            }
            if ((uint64)(u & ((uint64)1 << 63)) != 0) {
                break;
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void verifyInt(int64 i, ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(testError, t, ref ᒐ, recovers: true);
        ж<global::go.encoding.gob_package.encBuffer> b = @new<global::go.encoding.gob_package.encBuffer>();
        var encState = newEncoderState(b);
        encState.encodeInt(i);
        var decState = newDecodeState(newDecBuffer(b.Bytes()));
        var j = decState.decodeInt();
        if (i != j) {
            t.Errorf("Encode/Decode: sent %#x received %#x", (uint64)i, (uint64)j);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Test basic encode/decode routines for signed integers
public static void TestIntCodec(ж<testing.T> t) {
    for (var u = (uint64)0; ; u = (u + 1) * 7) {
        // Do positive and negative values
        var i = (int64)u;
        verifyInt(i, t);
        verifyInt(-i, t);
        verifyInt(~i, t);
        if ((uint64)(u & ((uint64)1 << 63)) != 0) {
            break;
        }
    }
    verifyInt(-9223372036854775808L, t); // a tricky case
}

// The result of encoding a true boolean with field number 7
internal static slice<byte> boolResult = [0x07, 0x01];

// The result of encoding a number 17 with field number 7
internal static slice<byte> signedResult = [0x07, (byte)(2 * 17)];
internal static slice<byte> unsignedResult = [0x07, 17];
internal static slice<byte> floatResult = [0x07, 0xFE, 0x31, 0x40];

// The result of encoding a number 17+19i with field number 7
internal static slice<byte> complexResult = [0x07, 0xFE, 0x31, 0x40, 0xFE, 0x33, 0x40];

// The result of encoding "hello" with field number 7
internal static slice<byte> bytesResult = [0x07, 0x05, (byte)'h', (byte)'e', (byte)'l', (byte)'l', (byte)'o'];

internal static ж<global::go.encoding.gob_package.decoderState> newDecodeState(ж<global::go.encoding.gob_package.decBuffer> buf) {
    var d = @new<global::go.encoding.gob_package.decoderState>();
    d.b = buf;
    return d;
}

internal static ж<global::go.encoding.gob_package.encoderState> newEncoderState(ж<global::go.encoding.gob_package.encBuffer> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.Reset();
    var state = Ꮡ(new encoderState(enc: nil, b: Ꮡb));
    state.fieldnum = -1;
    return state;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloˢ => ref helloˢᵇ;
private static readonly @string helloˢᵇ = "hello"u8;

// Test instruction execution for encoding.
// Do not run the machine yet; instead do individual instructions crafted by hand.
public static void TestScalarEncInstructions(ж<testing.T> t) {
    ж<global::go.encoding.gob_package.encBuffer> b = @new<global::go.encoding.gob_package.encBuffer>();

    // bool
    {
        bool data = true;
        var instr = Ꮡ(new encInstr(new Action<ж<global::go.encoding.gob_package.encInstr>, ж<global::go.encoding.gob_package.encoderState>, reflectꓸValue>(encBool), 6, default, 0));
        var state = newEncoderState(b);
        instr.op(instr, state, reflect.ValueOf(data));
        if (!bytes.Equal(boolResult, b.Bytes())) {
            t.Errorf("bool enc instructions: expected % x got % x", boolResult, b.Bytes());
        }
    }

    // int
    {
        b.Reset();
        nint data = 17;
        var instr = Ꮡ(new encInstr(new Action<ж<global::go.encoding.gob_package.encInstr>, ж<global::go.encoding.gob_package.encoderState>, reflectꓸValue>(encInt), 6, default, 0));
        var state = newEncoderState(b);
        instr.op(instr, state, reflect.ValueOf(data));
        if (!bytes.Equal(signedResult, b.Bytes())) {
            t.Errorf("int enc instructions: expected % x got % x", signedResult, b.Bytes());
        }
    }

    // uint
    {
        b.Reset();
        nuint data = 17;
        var instr = Ꮡ(new encInstr(new Action<ж<global::go.encoding.gob_package.encInstr>, ж<global::go.encoding.gob_package.encoderState>, reflectꓸValue>(encUint), 6, default, 0));
        var state = newEncoderState(b);
        instr.op(instr, state, reflect.ValueOf(data));
        if (!bytes.Equal(unsignedResult, b.Bytes())) {
            t.Errorf("uint enc instructions: expected % x got % x", unsignedResult, b.Bytes());
        }
    }

    // int8
    {
        b.Reset();
        int8 data = 17;
        var instr = Ꮡ(new encInstr(new Action<ж<global::go.encoding.gob_package.encInstr>, ж<global::go.encoding.gob_package.encoderState>, reflectꓸValue>(encInt), 6, default, 0));
        var state = newEncoderState(b);
        instr.op(instr, state, reflect.ValueOf(data));
        if (!bytes.Equal(signedResult, b.Bytes())) {
            t.Errorf("int8 enc instructions: expected % x got % x", signedResult, b.Bytes());
        }
    }

    // uint8
    {
        b.Reset();
        uint8 data = 17;
        var instr = Ꮡ(new encInstr(new Action<ж<global::go.encoding.gob_package.encInstr>, ж<global::go.encoding.gob_package.encoderState>, reflectꓸValue>(encUint), 6, default, 0));
        var state = newEncoderState(b);
        instr.op(instr, state, reflect.ValueOf(data));
        if (!bytes.Equal(unsignedResult, b.Bytes())) {
            t.Errorf("uint8 enc instructions: expected % x got % x", unsignedResult, b.Bytes());
        }
    }

    // int16
    {
        b.Reset();
        int16 data = 17;
        var instr = Ꮡ(new encInstr(new Action<ж<global::go.encoding.gob_package.encInstr>, ж<global::go.encoding.gob_package.encoderState>, reflectꓸValue>(encInt), 6, default, 0));
        var state = newEncoderState(b);
        instr.op(instr, state, reflect.ValueOf(data));
        if (!bytes.Equal(signedResult, b.Bytes())) {
            t.Errorf("int16 enc instructions: expected % x got % x", signedResult, b.Bytes());
        }
    }

    // uint16
    {
        b.Reset();
        uint16 data = 17;
        var instr = Ꮡ(new encInstr(new Action<ж<global::go.encoding.gob_package.encInstr>, ж<global::go.encoding.gob_package.encoderState>, reflectꓸValue>(encUint), 6, default, 0));
        var state = newEncoderState(b);
        instr.op(instr, state, reflect.ValueOf(data));
        if (!bytes.Equal(unsignedResult, b.Bytes())) {
            t.Errorf("uint16 enc instructions: expected % x got % x", unsignedResult, b.Bytes());
        }
    }

    // int32
    {
        b.Reset();
        int32 data = 17;
        var instr = Ꮡ(new encInstr(new Action<ж<global::go.encoding.gob_package.encInstr>, ж<global::go.encoding.gob_package.encoderState>, reflectꓸValue>(encInt), 6, default, 0));
        var state = newEncoderState(b);
        instr.op(instr, state, reflect.ValueOf(data));
        if (!bytes.Equal(signedResult, b.Bytes())) {
            t.Errorf("int32 enc instructions: expected % x got % x", signedResult, b.Bytes());
        }
    }

    // uint32
    {
        b.Reset();
        uint32 data = 17;
        var instr = Ꮡ(new encInstr(new Action<ж<global::go.encoding.gob_package.encInstr>, ж<global::go.encoding.gob_package.encoderState>, reflectꓸValue>(encUint), 6, default, 0));
        var state = newEncoderState(b);
        instr.op(instr, state, reflect.ValueOf(data));
        if (!bytes.Equal(unsignedResult, b.Bytes())) {
            t.Errorf("uint32 enc instructions: expected % x got % x", unsignedResult, b.Bytes());
        }
    }

    // int64
    {
        b.Reset();
        int64 data = 17;
        var instr = Ꮡ(new encInstr(new Action<ж<global::go.encoding.gob_package.encInstr>, ж<global::go.encoding.gob_package.encoderState>, reflectꓸValue>(encInt), 6, default, 0));
        var state = newEncoderState(b);
        instr.op(instr, state, reflect.ValueOf(data));
        if (!bytes.Equal(signedResult, b.Bytes())) {
            t.Errorf("int64 enc instructions: expected % x got % x", signedResult, b.Bytes());
        }
    }

    // uint64
    {
        b.Reset();
        uint64 data = 17;
        var instr = Ꮡ(new encInstr(new Action<ж<global::go.encoding.gob_package.encInstr>, ж<global::go.encoding.gob_package.encoderState>, reflectꓸValue>(encUint), 6, default, 0));
        var state = newEncoderState(b);
        instr.op(instr, state, reflect.ValueOf(data));
        if (!bytes.Equal(unsignedResult, b.Bytes())) {
            t.Errorf("uint64 enc instructions: expected % x got % x", unsignedResult, b.Bytes());
        }
    }

    // float32
    {
        b.Reset();
        float32 data = 17F;
        var instr = Ꮡ(new encInstr(new Action<ж<global::go.encoding.gob_package.encInstr>, ж<global::go.encoding.gob_package.encoderState>, reflectꓸValue>(encFloat), 6, default, 0));
        var state = newEncoderState(b);
        instr.op(instr, state, reflect.ValueOf(data));
        if (!bytes.Equal(floatResult, b.Bytes())) {
            t.Errorf("float32 enc instructions: expected % x got % x", floatResult, b.Bytes());
        }
    }

    // float64
    {
        b.Reset();
        float64 data = 17D;
        var instr = Ꮡ(new encInstr(new Action<ж<global::go.encoding.gob_package.encInstr>, ж<global::go.encoding.gob_package.encoderState>, reflectꓸValue>(encFloat), 6, default, 0));
        var state = newEncoderState(b);
        instr.op(instr, state, reflect.ValueOf(data));
        if (!bytes.Equal(floatResult, b.Bytes())) {
            t.Errorf("float64 enc instructions: expected % x got % x", floatResult, b.Bytes());
        }
    }

    // bytes == []uint8
    {
        b.Reset();
        var data = slice<byte>("hello"u8);
        var instr = Ꮡ(new encInstr(new Action<ж<global::go.encoding.gob_package.encInstr>, ж<global::go.encoding.gob_package.encoderState>, reflectꓸValue>(encUint8Array), 6, default, 0));
        var state = newEncoderState(b);
        instr.op(instr, state, reflect.ValueOf(data));
        if (!bytes.Equal(bytesResult, b.Bytes())) {
            t.Errorf("bytes enc instructions: expected % x got % x", bytesResult, b.Bytes());
        }
    }

    // string
    {
        b.Reset();
        @string data = helloˢ;
        var instr = Ꮡ(new encInstr(new Action<ж<global::go.encoding.gob_package.encInstr>, ж<global::go.encoding.gob_package.encoderState>, reflectꓸValue>(encString), 6, default, 0));
        var state = newEncoderState(b);
        instr.op(instr, state, reflect.ValueOf(data));
        if (!bytes.Equal(bytesResult, b.Bytes())) {
            t.Errorf("string enc instructions: expected % x got % x", bytesResult, b.Bytes());
        }
    }
}

internal static void execDec(ж<global::go.encoding.gob_package.decInstr> Ꮡinstr, ж<global::go.encoding.gob_package.decoderState> Ꮡstate, ж<testing.T> t, reflectꓸValue value) {
    GoFrame ᒐ = default;
    try {
        ref var instr = ref Ꮡinstr.DerefOrNull();
        ref var state = ref Ꮡstate.DerefOrNull();

        defer(testError, t, ref ᒐ, recovers: true);
        nint v = (nint)state.decodeUint();
        if (v + state.fieldnum != 6) {
            t.Fatalf("decoding field number %d, got %d", (nint)6, v + state.fieldnum);
        }
        instr.op(Ꮡinstr, Ꮡstate, value.Elem());
        state.fieldnum = 6;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static ж<global::go.encoding.gob_package.decoderState> newDecodeStateFromData(slice<byte> data) {
    var b = newDecBuffer(data);
    var state = newDecodeState(b);
    state.fieldnum = -1;
    return state;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string overflowˢ => ref overflowˢᵇ;
private static readonly @string overflowˢᵇ = "overflow"u8;

// Test instruction execution for decoding.
// Do not run the machine yet; instead do individual instructions crafted by hand.
public static void TestScalarDecInstructions(ж<testing.T> t) {
    var ovfl = errors.New(overflowˢ);

    // bool
    {
        ref var data = ref heap(new bool(), out var Ꮡdata);
        var instr = Ꮡ(new decInstr(new Action<ж<global::go.encoding.gob_package.decInstr>, ж<global::go.encoding.gob_package.decoderState>, reflectꓸValue>(decBool), 6, default, ovfl));
        var state = newDecodeStateFromData(boolResult);
        execDec(instr, state, t, reflect.ValueOf(Ꮡdata));
        if (data != true) {
            t.Errorf("bool a = %v not true", data);
        }
    }
    // int
    {
        ref var data = ref heap(new nint(), out var Ꮡdata);
        var instr = Ꮡ(new decInstr(decOpTable[reflect.ΔInt], 6, default, ovfl));
        var state = newDecodeStateFromData(signedResult);
        execDec(instr, state, t, reflect.ValueOf(Ꮡdata));
        if (data != 17) {
            t.Errorf("int a = %v not 17", data);
        }
    }

    // uint
    {
        ref var data = ref heap(new nuint(), out var Ꮡdata);
        var instr = Ꮡ(new decInstr(decOpTable[reflect.ΔUint], 6, default, ovfl));
        var state = newDecodeStateFromData(unsignedResult);
        execDec(instr, state, t, reflect.ValueOf(Ꮡdata));
        if (data != 17) {
            t.Errorf("uint a = %v not 17", data);
        }
    }

    // int8
    {
        ref var data = ref heap(new int8(), out var Ꮡdata);
        var instr = Ꮡ(new decInstr(new Action<ж<global::go.encoding.gob_package.decInstr>, ж<global::go.encoding.gob_package.decoderState>, reflectꓸValue>(decInt8), 6, default, ovfl));
        var state = newDecodeStateFromData(signedResult);
        execDec(instr, state, t, reflect.ValueOf(Ꮡdata));
        if (data != 17) {
            t.Errorf("int8 a = %v not 17", data);
        }
    }

    // uint8
    {
        ref var data = ref heap(new uint8(), out var Ꮡdata);
        var instr = Ꮡ(new decInstr(new Action<ж<global::go.encoding.gob_package.decInstr>, ж<global::go.encoding.gob_package.decoderState>, reflectꓸValue>(decUint8), 6, default, ovfl));
        var state = newDecodeStateFromData(unsignedResult);
        execDec(instr, state, t, reflect.ValueOf(Ꮡdata));
        if (data != 17) {
            t.Errorf("uint8 a = %v not 17", data);
        }
    }

    // int16
    {
        ref var data = ref heap(new int16(), out var Ꮡdata);
        var instr = Ꮡ(new decInstr(new Action<ж<global::go.encoding.gob_package.decInstr>, ж<global::go.encoding.gob_package.decoderState>, reflectꓸValue>(decInt16), 6, default, ovfl));
        var state = newDecodeStateFromData(signedResult);
        execDec(instr, state, t, reflect.ValueOf(Ꮡdata));
        if (data != 17) {
            t.Errorf("int16 a = %v not 17", data);
        }
    }

    // uint16
    {
        ref var data = ref heap(new uint16(), out var Ꮡdata);
        var instr = Ꮡ(new decInstr(new Action<ж<global::go.encoding.gob_package.decInstr>, ж<global::go.encoding.gob_package.decoderState>, reflectꓸValue>(decUint16), 6, default, ovfl));
        var state = newDecodeStateFromData(unsignedResult);
        execDec(instr, state, t, reflect.ValueOf(Ꮡdata));
        if (data != 17) {
            t.Errorf("uint16 a = %v not 17", data);
        }
    }

    // int32
    {
        ref var data = ref heap(new int32(), out var Ꮡdata);
        var instr = Ꮡ(new decInstr(new Action<ж<global::go.encoding.gob_package.decInstr>, ж<global::go.encoding.gob_package.decoderState>, reflectꓸValue>(decInt32), 6, default, ovfl));
        var state = newDecodeStateFromData(signedResult);
        execDec(instr, state, t, reflect.ValueOf(Ꮡdata));
        if (data != 17) {
            t.Errorf("int32 a = %v not 17", data);
        }
    }

    // uint32
    {
        ref var data = ref heap(new uint32(), out var Ꮡdata);
        var instr = Ꮡ(new decInstr(new Action<ж<global::go.encoding.gob_package.decInstr>, ж<global::go.encoding.gob_package.decoderState>, reflectꓸValue>(decUint32), 6, default, ovfl));
        var state = newDecodeStateFromData(unsignedResult);
        execDec(instr, state, t, reflect.ValueOf(Ꮡdata));
        if (data != 17) {
            t.Errorf("uint32 a = %v not 17", data);
        }
    }

    // uintptr
    {
        ref var data = ref heap(new uintptr(), out var Ꮡdata);
        var instr = Ꮡ(new decInstr(decOpTable[reflect.Uintptr], 6, default, ovfl));
        var state = newDecodeStateFromData(unsignedResult);
        execDec(instr, state, t, reflect.ValueOf(Ꮡdata));
        if (data != 17) {
            t.Errorf("uintptr a = %v not 17", data);
        }
    }

    // int64
    {
        ref var data = ref heap(new int64(), out var Ꮡdata);
        var instr = Ꮡ(new decInstr(new Action<ж<global::go.encoding.gob_package.decInstr>, ж<global::go.encoding.gob_package.decoderState>, reflectꓸValue>(decInt64), 6, default, ovfl));
        var state = newDecodeStateFromData(signedResult);
        execDec(instr, state, t, reflect.ValueOf(Ꮡdata));
        if (data != 17) {
            t.Errorf("int64 a = %v not 17", data);
        }
    }

    // uint64
    {
        ref var data = ref heap(new uint64(), out var Ꮡdata);
        var instr = Ꮡ(new decInstr(new Action<ж<global::go.encoding.gob_package.decInstr>, ж<global::go.encoding.gob_package.decoderState>, reflectꓸValue>(decUint64), 6, default, ovfl));
        var state = newDecodeStateFromData(unsignedResult);
        execDec(instr, state, t, reflect.ValueOf(Ꮡdata));
        if (data != 17) {
            t.Errorf("uint64 a = %v not 17", data);
        }
    }

    // float32
    {
        ref var data = ref heap(new float32(), out var Ꮡdata);
        var instr = Ꮡ(new decInstr(new Action<ж<global::go.encoding.gob_package.decInstr>, ж<global::go.encoding.gob_package.decoderState>, reflectꓸValue>(decFloat32), 6, default, ovfl));
        var state = newDecodeStateFromData(floatResult);
        execDec(instr, state, t, reflect.ValueOf(Ꮡdata));
        if (data != 17F) {
            t.Errorf("float32 a = %v not 17", data);
        }
    }

    // float64
    {
        ref var data = ref heap(new float64(), out var Ꮡdata);
        var instr = Ꮡ(new decInstr(new Action<ж<global::go.encoding.gob_package.decInstr>, ж<global::go.encoding.gob_package.decoderState>, reflectꓸValue>(decFloat64), 6, default, ovfl));
        var state = newDecodeStateFromData(floatResult);
        execDec(instr, state, t, reflect.ValueOf(Ꮡdata));
        if (data != 17D) {
            t.Errorf("float64 a = %v not 17", data);
        }
    }

    // complex64
    {
        ref var data = ref heap(new complex64(), out var Ꮡdata);
        var instr = Ꮡ(new decInstr(decOpTable[reflect.Complex64], 6, default, ovfl));
        var state = newDecodeStateFromData(complexResult);
        execDec(instr, state, t, reflect.ValueOf(Ꮡdata));
        if (data != 17F + 19F.i()) {
            t.Errorf("complex a = %v not 17+19i", data);
        }
    }

    // complex128
    {
        ref var data = ref heap(new complex128(), out var Ꮡdata);
        var instr = Ꮡ(new decInstr(decOpTable[reflect.Complex128], 6, default, ovfl));
        var state = newDecodeStateFromData(complexResult);
        execDec(instr, state, t, reflect.ValueOf(Ꮡdata));
        if (data != 17D + 19D.i()) {
            t.Errorf("complex a = %v not 17+19i", data);
        }
    }

    // bytes == []uint8
    {
        ref var data = ref heap<slice<byte>>(out var Ꮡdata);
        var instr = Ꮡ(new decInstr(new Action<ж<global::go.encoding.gob_package.decInstr>, ж<global::go.encoding.gob_package.decoderState>, reflectꓸValue>(decUint8Slice), 6, default, ovfl));
        var state = newDecodeStateFromData(bytesResult);
        execDec(instr, state, t, reflect.ValueOf(Ꮡdata));
        if (((sstring)data) != "hello"u8) {
            t.Errorf(@"bytes a = %q not ""hello""", (@string)data);
        }
    }

    // string
    {
        ref var data = ref heap(new @string(), out var Ꮡdata);
        var instr = Ꮡ(new decInstr(new Action<ж<global::go.encoding.gob_package.decInstr>, ж<global::go.encoding.gob_package.decoderState>, reflectꓸValue>(decString), 6, default, ovfl));
        var state = newDecodeStateFromData(bytesResult);
        execDec(instr, state, t, reflect.ValueOf(Ꮡdata));
        if (data != "hello"u8) {
            t.Errorf(@"bytes a = %q not ""hello""", data);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string string1ˢ => ref string1ˢᵇ;
private static readonly @string string1ˢᵇ = "string1"u8;
internal static ref readonly @string string2ˢ => ref string2ˢᵇ;
private static readonly @string string2ˢᵇ = "string2"u8;
internal static readonly object encodeˢ = (@string)"encode:"u8;
internal static readonly object decodeˢ = (@string)"decode:"u8;

internal partial struct TestEndToEnd_T2 /*dyn*/ {
    public @string T;
}

internal partial struct TestEndToEnd_T3 /*dyn*/ {
    public float64 X;
    public ж<nint> Z;
}

internal partial struct TestEndToEnd_T1 /*dyn*/ {
    public nint A, B, C;
    public map<@string, ж<float64>> M;
    public map<nint, TestEndToEnd_T3> M2;
    public map<@string, @string> Mstring;
    public map<nint, ж<nint>> Mintptr;
    public map<complex128, complex128> Mcomp;
    public /*[2]*/ map</*[2]*/ array<@string>, array<ж<float64>>> Marr;
    public map<@string, nint> EmptyMap; // to check that we receive a non-nil map.
    public /*[3]*/ ж<array<float64>> N;
    public /*[2]*/ ж<array<@string>> Strs;
    public ж<slice<int64>> Int64s;
    public complex64 RI;
    public @string S;
    public slice<byte> Y;
    public ж<TestEndToEnd_T2> T;
}

public static void TestEndToEnd(ж<testing.T> t) {
    ref var pi = ref heap<float64>(3.14159D, out var Ꮡpi);
    ref var e = ref heap<float64>(2.71828D, out var Ꮡe);
    ref var two = ref heap<float64>(2.0D, out var Ꮡtwo);
    ref var meaning = ref heap<nint>(42, out var Ꮡmeaning);
    ref var fingers = ref heap<nint>(5, out var Ꮡfingers);
    ref var s1 = ref heap<@string>(string1ˢ, out var Ꮡs1);
    ref var s2 = ref heap<@string>(string2ˢ, out var Ꮡs2);
    complex128 comp1 = complex(1.0D, 1.0D);
    complex128 comp2 = complex(1.0D, 1.0D);
    array<@string> arr1 = new(2);
    arr1[0] = s1;
    arr1[1] = s2;
    array<@string> arr2 = new(2);
    arr2[0] = s2;
    arr2[1] = s1;
    array<ж<float64>> floatArr1 = new(2);
    floatArr1[0] = Ꮡpi;
    floatArr1[1] = Ꮡe;
    array<ж<float64>> floatArr2 = new(2);
    floatArr2[0] = Ꮡe;
    floatArr2[1] = Ꮡtwo;
    var t1 = Ꮡ(new TestEndToEnd_T1(
        A: 17,
        B: 18,
        C: -5,
        M: new map<@string, ж<float64>>{["pi"] = Ꮡpi, ["e"] = Ꮡe},
        M2: new map<nint, TestEndToEnd_T3>{[4] = new(X: pi, Z: Ꮡmeaning), [10] = new(X: e, Z: Ꮡfingers)},
        Mstring: new map<@string, @string>{["pi"] = "3.14", ["e"] = "2.71"},
        Mintptr: new map<nint, ж<nint>>{[meaning] = Ꮡfingers, [fingers] = Ꮡmeaning},
        Mcomp: new map<complex128, complex128>{[comp1] = comp2, [comp2] = comp1},
        Marr: new map<array<@string>, array<ж<float64>>>{[arr1.Clone()] = floatArr1.Clone(), [arr2.Clone()] = floatArr2.Clone()},
        EmptyMap: new map<@string, nint>(),
        N: Ꮡ(new float64[]{1.5D, 2.5D, 3.5D}.array()),
        Strs: Ꮡ(new @string[]{s1, s2}.array()),
        Int64s: Ꮡ(slice<int64>([77, 89, 123412342134L])),
        RI: 17F + -23F.i(),
        S: "Now is the time",
        Y: slice<byte>("hello, sailor"u8),
        T: Ꮡ(new TestEndToEnd_T2("this is T2"))
    ));
    var b = @new<bytes.Buffer>();
    var err = NewEncoder(new gob_test_package.bytes_BufferжWriter(b)).Encode(t1.OrTypedNil());
    if (err != nil) {
        t.Error(encodeˢ, err);
    }
    ref var _t1 = ref heap(new TestEndToEnd_T1(), out var Ꮡ_t1);
    err = NewDecoder(new gob_test_package.bytes_BufferжReader(b)).Decode(Ꮡ_t1);
    if (err != nil) {
        t.Fatal(decodeˢ, err);
    }
    if (!reflect.DeepEqual(t1.OrTypedNil(), Ꮡ_t1)) {
        t.Errorf("encode expected %v got %v", t1.Value, _t1);
    }
    // Be absolutely sure the received map is non-nil.
    if (t1.EmptyMap == nil) {
        t.Errorf("nil map sent");
    }
    if (_t1.EmptyMap == nil) {
        t.Errorf("nil map received");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object wrongOverflowErrorForˢ = (@string)"wrong overflow error for int8:"u8;
internal static readonly object wrongUnderflowErrorForˢ = (@string)"wrong underflow error for int8:"u8;
internal static readonly object wrongOverflowErrorForˢ2 = (@string)"wrong overflow error for int16:"u8;
internal static readonly object wrongUnderflowErrorForˢ2 = (@string)"wrong underflow error for int16:"u8;
internal static readonly object wrongOverflowErrorForˢ3 = (@string)"wrong overflow error for int32:"u8;
internal static readonly object wrongUnderflowErrorForˢ3 = (@string)"wrong underflow error for int32:"u8;
internal static readonly object wrongOverflowErrorForˢ4 = (@string)"wrong overflow error for uint8:"u8;
internal static readonly object wrongOverflowErrorForˢ5 = (@string)"wrong overflow error for uint16:"u8;
internal static readonly object wrongOverflowErrorForˢ6 = (@string)"wrong overflow error for uint32:"u8;
internal static readonly object wrongOverflowErrorForˢ7 = (@string)"wrong overflow error for float32:"u8;
internal static readonly object wrongOverflowErrorForˢ8 = (@string)"wrong overflow error for complex64:"u8;

internal partial struct TestOverflow_inputT /*dyn*/ {
    public int64 Maxi;
    public int64 Mini;
    public uint64 Maxu;
    public float64 Maxf;
    public float64 Minf;
    public complex128 Maxc;
    public complex128 Minc;
}

internal partial struct TestOverflow_outi8 /*dyn*/ {
    public int8 Maxi;
    public int8 Mini;
}

internal partial struct TestOverflow_outi16 /*dyn*/ {
    public int16 Maxi;
    public int16 Mini;
}

internal partial struct TestOverflow_outi32 /*dyn*/ {
    public int32 Maxi;
    public int32 Mini;
}

internal partial struct TestOverflow_outu8 /*dyn*/ {
    public uint8 Maxu;
}

internal partial struct TestOverflow_outu16 /*dyn*/ {
    public uint16 Maxu;
}

internal partial struct TestOverflow_outu32 /*dyn*/ {
    public uint32 Maxu;
}

internal partial struct TestOverflow_outf32 /*dyn*/ {
    public float32 Maxf;
    public float32 Minf;
}

internal partial struct TestOverflow_outc64 /*dyn*/ {
    public complex64 Maxc;
    public complex64 Minc;
}

public static void TestOverflow(ж<testing.T> t) {
    TestOverflow_inputT it = default;
    error err = default;
    var b = @new<bytes.Buffer>();
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));

    // int8
    b.Reset();
    it = new TestOverflow_inputT(
        Maxi: math.MaxInt8 + 1
    );
    ref var o1 = ref heap(new TestOverflow_outi8(), out var Ꮡo1);
    enc.Encode(it);
    err = dec.Decode(Ꮡo1);
    if (err == nil || err.Error() != @"value for ""Maxi"" out of range"u8) {
        t.Error(wrongOverflowErrorForˢ, err);
    }
    it = new TestOverflow_inputT(
        Mini: math.MinInt8 - 1
    );
    b.Reset();
    enc.Encode(it);
    err = dec.Decode(Ꮡo1);
    if (err == nil || err.Error() != @"value for ""Mini"" out of range"u8) {
        t.Error(wrongUnderflowErrorForˢ, err);
    }

    // int16
    b.Reset();
    it = new TestOverflow_inputT(
        Maxi: math.MaxInt16 + 1
    );
    ref var o2 = ref heap(new TestOverflow_outi16(), out var Ꮡo2);
    enc.Encode(it);
    err = dec.Decode(Ꮡo2);
    if (err == nil || err.Error() != @"value for ""Maxi"" out of range"u8) {
        t.Error(wrongOverflowErrorForˢ2, err);
    }
    it = new TestOverflow_inputT(
        Mini: math.MinInt16 - 1
    );
    b.Reset();
    enc.Encode(it);
    err = dec.Decode(Ꮡo2);
    if (err == nil || err.Error() != @"value for ""Mini"" out of range"u8) {
        t.Error(wrongUnderflowErrorForˢ2, err);
    }

    // int32
    b.Reset();
    it = new TestOverflow_inputT(
        Maxi: 2147483648L
    );
    ref var o3 = ref heap(new TestOverflow_outi32(), out var Ꮡo3);
    enc.Encode(it);
    err = dec.Decode(Ꮡo3);
    if (err == nil || err.Error() != @"value for ""Maxi"" out of range"u8) {
        t.Error(wrongOverflowErrorForˢ3, err);
    }
    it = new TestOverflow_inputT(
        Mini: -2147483649L
    );
    b.Reset();
    enc.Encode(it);
    err = dec.Decode(Ꮡo3);
    if (err == nil || err.Error() != @"value for ""Mini"" out of range"u8) {
        t.Error(wrongUnderflowErrorForˢ3, err);
    }

    // uint8
    b.Reset();
    it = new TestOverflow_inputT(
        Maxu: math.MaxUint8 + 1
    );
    ref var o4 = ref heap(new TestOverflow_outu8(), out var Ꮡo4);
    enc.Encode(it);
    err = dec.Decode(Ꮡo4);
    if (err == nil || err.Error() != @"value for ""Maxu"" out of range"u8) {
        t.Error(wrongOverflowErrorForˢ4, err);
    }

    // uint16
    b.Reset();
    it = new TestOverflow_inputT(
        Maxu: math.MaxUint16 + 1
    );
    ref var o5 = ref heap(new TestOverflow_outu16(), out var Ꮡo5);
    enc.Encode(it);
    err = dec.Decode(Ꮡo5);
    if (err == nil || err.Error() != @"value for ""Maxu"" out of range"u8) {
        t.Error(wrongOverflowErrorForˢ5, err);
    }

    // uint32
    b.Reset();
    it = new TestOverflow_inputT(
        Maxu: math.MaxUint32 + 1
    );
    ref var o6 = ref heap(new TestOverflow_outu32(), out var Ꮡo6);
    enc.Encode(it);
    err = dec.Decode(Ꮡo6);
    if (err == nil || err.Error() != @"value for ""Maxu"" out of range"u8) {
        t.Error(wrongOverflowErrorForˢ6, err);
    }

    // float32
    b.Reset();
    it = new TestOverflow_inputT(
        Maxf: math.MaxFloat32 * 2D
    );
    ref var o7 = ref heap(new TestOverflow_outf32(), out var Ꮡo7);
    enc.Encode(it);
    err = dec.Decode(Ꮡo7);
    if (err == nil || err.Error() != @"value for ""Maxf"" out of range"u8) {
        t.Error(wrongOverflowErrorForˢ7, err);
    }

    // complex64
    b.Reset();
    it = new TestOverflow_inputT(
        Maxc: complex((float64)(math.MaxFloat32 * 2D), (float64)(math.MaxFloat32 * 2D))
    );
    ref var o8 = ref heap(new TestOverflow_outc64(), out var Ꮡo8);
    enc.Encode(it);
    err = dec.Decode(Ꮡo8);
    if (err == nil || err.Error() != @"value for ""Maxc"" out of range"u8) {
        t.Error(wrongOverflowErrorForˢ8, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string level1ˢ => ref level1ˢᵇ;
private static readonly @string level1ˢᵇ = "level1"u8;
internal static ref readonly @string level2ˢ => ref level2ˢᵇ;
private static readonly @string level2ˢᵇ = "level2"u8;
internal static readonly object decoderErrorˢ = (@string)"decoder error:"u8;

internal partial struct TestNesting_RT /*dyn*/ {
    public @string A;
    public ж<TestNesting_RT> Next;
}

public static void TestNesting(ж<testing.T> t) {
    var rt = @new<TestNesting_RT>();
    rt.A = level1ˢ;
    rt.Next = @new<TestNesting_RT>();
    rt.Next.A = level2ˢ;
    var b = @new<bytes.Buffer>();
    NewEncoder(new gob_test_package.bytes_BufferжWriter(b)).Encode(rt.OrTypedNil());
    ref var drt = ref heap(new TestNesting_RT(), out var Ꮡdrt);
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    var err = dec.Decode(Ꮡdrt);
    if (err != nil) {
        t.Fatal(decoderErrorˢ, err);
    }
    if (drt.A != rt.A) {
        t.Errorf("nesting: encode expected %v got %v", rt.Value, drt);
    }
    if (drt.Next == nil) {
        t.Errorf("nesting: recursion failed");
    }
    if (drt.Next.A != rt.Next.A) {
        t.Errorf("nesting: encode expected %v got %v", rt.Next.Value, drt.Next.Value);
    }
}

// These three structures have the same data with different indirections
public partial struct T0 {
    public nint A;
    public nint B;
    public nint C;
    public nint D;
}
public partial struct T1 {
    public nint A;
    public ж<nint> B;
    public ж<ж<nint>> C;
    public ж<ж<ж<nint>>> D;
}
public partial struct T2 {
    public ж<ж<ж<nint>>> A;
    public ж<ж<nint>> B;
    public ж<nint> C;
    public nint D;
}

public static void TestAutoIndirection(ж<testing.T> t) {
    // First transfer t1 into t0
    ref var t1 = ref heap(new T1(), out var Ꮡt1);
    t1.A = 17;
    t1.B = @new<nint>();
    t1.B.Value = 177;
    t1.C = @new<ж<nint>>();
    t1.C.ValueSlot = @new<nint>();
    (t1.C.ValueSlot).Value = 1777;
    t1.D = @new<ж<ж<nint>>>();
    t1.D.ValueSlot = @new<ж<nint>>();
    (t1.D.ValueSlot).ValueSlot = @new<nint>();
    ((t1.D.ValueSlot).ValueSlot).Value = 17777;
    var b = @new<bytes.Buffer>();
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
    enc.Encode(t1);
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    ref var t0 = ref heap(new T0(), out var Ꮡt0);
    dec.Decode(Ꮡt0);
    if (t0.A != 17 || t0.B != 177 || t0.C != 1777 || t0.D != 17777) {
        t.Errorf("t1->t0: expected {17 177 1777 17777}; got %v", t0);
    }

    // Now transfer t2 into t0
    ref var t2 = ref heap(new T2(), out var Ꮡt2);
    t2.D = 17777;
    t2.C = @new<nint>();
    t2.C.Value = 1777;
    t2.B = @new<ж<nint>>();
    t2.B.ValueSlot = @new<nint>();
    (t2.B.ValueSlot).Value = 177;
    t2.A = @new<ж<ж<nint>>>();
    t2.A.ValueSlot = @new<ж<nint>>();
    (t2.A.ValueSlot).ValueSlot = @new<nint>();
    ((t2.A.ValueSlot).ValueSlot).Value = 17;
    b.Reset();
    enc.Encode(t2);
    t0 = new T0();
    dec.Decode(Ꮡt0);
    if (t0.A != 17 || t0.B != 177 || t0.C != 1777 || t0.D != 17777) {
        t.Errorf("t2->t0 expected {17 177 1777 17777}; got %v", t0);
    }

    // Now transfer t0 into t1
    t0 = new T0(17, 177, 1777, 17777);
    b.Reset();
    enc.Encode(t0);
    t1 = new T1();
    dec.Decode(Ꮡt1);
    if (t1.A != 17 || t1.B.Value != 177 || (t1.C.ValueSlot).Value != 1777 || ((t1.D.ValueSlot).ValueSlot).Value != 17777) {
        t.Errorf("t0->t1 expected {17 177 1777 17777}; got {%d %d %d %d}", t1.A, t1.B.Value, (t1.C.ValueSlot).Value, ((t1.D.ValueSlot).ValueSlot).Value);
    }

    // Now transfer t0 into t2
    b.Reset();
    enc.Encode(t0);
    t2 = new T2();
    dec.Decode(Ꮡt2);
    if (((t2.A.ValueSlot).ValueSlot).Value != 17 || (t2.B.ValueSlot).Value != 177 || t2.C.Value != 1777 || t2.D != 17777) {
        t.Errorf("t0->t2 expected {17 177 1777 17777}; got {%d %d %d %d}", ((t2.A.ValueSlot).ValueSlot).Value, (t2.B.ValueSlot).Value, t2.C.Value, t2.D);
    }

    // Now do t2 again but without pre-allocated pointers.
    b.Reset();
    enc.Encode(t0);
    ((t2.A.ValueSlot).ValueSlot).Value = 0;
    (t2.B.ValueSlot).Value = 0;
    t2.C.Value = 0;
    t2.D = 0;
    dec.Decode(Ꮡt2);
    if (((t2.A.ValueSlot).ValueSlot).Value != 17 || (t2.B.ValueSlot).Value != 177 || t2.C.Value != 1777 || t2.D != 17777) {
        t.Errorf("t0->t2 expected {17 177 1777 17777}; got {%d %d %d %d}", ((t2.A.ValueSlot).ValueSlot).Value, (t2.B.ValueSlot).Value, t2.C.Value, t2.D);
    }
}

public partial struct RT0 {
    public nint A;
    public @string B;
    public float64 C;
}
public partial struct RT1 {
    public float64 C;
    public @string B;
    public nint A;
    public @string NotSet;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object decodeErrorˢ = (@string)"decode error:"u8;

public static void TestReorderedFields(ж<testing.T> t) {
    RT0 rt0 = default;
    rt0.A = 17;
    rt0.B = helloˢ;
    rt0.C = 3.14159D;
    var b = @new<bytes.Buffer>();
    NewEncoder(new gob_test_package.bytes_BufferжWriter(b)).Encode(rt0);
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    ref var rt1 = ref heap(new RT1(), out var Ꮡrt1);
    // Wire type is RT0, local type is RT1.
    var err = dec.Decode(Ꮡrt1);
    if (err != nil) {
        t.Fatal(decodeErrorˢ, err);
    }
    if (rt0.A != rt1.A || rt0.B != rt1.B || rt0.C != rt1.C) {
        t.Errorf("rt1->rt0: expected %v; got %v", rt0, rt1);
    }
}

// Like an RT0 but with fields we'll ignore on the decode side.
public partial struct IT0 {
    public int64 A;
    public @string B;
    public slice<nint> Ignore_d;
    public array<float64> Ignore_e = new(3);
    public bool Ignore_f;
    public @string Ignore_g;
    public slice<byte> Ignore_h;
    public ж<RT1> Ignore_i;
    public map<@string, nint> Ignore_m;
    public float64 C;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string payNoAttentionˢ => ref payNoAttentionˢᵇ;
private static readonly @string payNoAttentionˢᵇ = "pay no attention"u8;
internal static readonly object errorˢ = (@string)"error: "u8;

public static void TestIgnoredFields(ж<testing.T> t) {
    IT0 it0 = new();
    it0.A = 17;
    it0.B = helloˢ;
    it0.C = 3.14159D;
    it0.Ignore_d = [1, 2, 3];
    it0.Ignore_e[0] = 1.0D;
    it0.Ignore_e[1] = 2.0D;
    it0.Ignore_e[2] = 3.0D;
    it0.Ignore_f = true;
    it0.Ignore_g = payNoAttentionˢ;
    it0.Ignore_h = slice<byte>("to the curtain"u8);
    it0.Ignore_i = Ꮡ(new RT1(3.1D, "hi", 7, "hello"));
    it0.Ignore_m = new map<@string, nint>{["one"] = 1, ["two"] = 2};

    var b = @new<bytes.Buffer>();
    NewEncoder(new gob_test_package.bytes_BufferжWriter(b)).Encode(it0);
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    ref var rt1 = ref heap(new RT1(), out var Ꮡrt1);
    // Wire type is IT0, local type is RT1.
    var err = dec.Decode(Ꮡrt1);
    if (err != nil) {
        t.Error(errorˢ, err);
    }
    if ((nint)it0.A != rt1.A || it0.B != rt1.B || it0.C != rt1.C) {
        t.Errorf("rt0->rt1: expected %v; got %v", it0, rt1);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorGotNoneˢ = (@string)"expected error; got none"u8;
internal static ref readonly @string recursiveˢ => ref recursiveˢᵇ;
private static readonly @string recursiveˢᵇ = "recursive"u8;
internal static readonly object expectedRecursiveTypeˢ = (@string)"expected recursive type error; got"u8;

internal partial class TestBadRecursiveType_Rec /*ж<ж<ж<TestBadRecursiveType_Rec>>>*/;

public static void TestBadRecursiveType(ж<testing.T> t) {
    ref var rec = ref heap<TestBadRecursiveType_Rec>(out var Ꮡrec);
    var b = @new<bytes.Buffer>();
    var err = NewEncoder(new gob_test_package.bytes_BufferжWriter(b)).Encode(Ꮡrec);
    if (err == nil) {
        t.Error(expectedErrorGotNoneˢ);
    } else if (!strings.Contains(err.Error(), recursiveˢ)) {
        t.Error(expectedRecursiveTypeˢ, err);
    }
    // Can't test decode easily because we can't encode one, so we can't pass one to a Decoder.
}

public partial struct Indirect {
    public /*[3]*/ ж<ж<ж<array<nint>>>> A;
    public ж<ж<ж<slice<nint>>>> S;
    public ж<ж<ж<ж<map<@string, nint>>>>> M;
}

public partial struct Direct {
    public array<nint> A = new(3);
    public slice<nint> S;
    public map<@string, nint> M;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string oneˢ => ref oneˢᵇ;
private static readonly @string oneˢᵇ = "one"u8;
internal static ref readonly @string twoˢ => ref twoˢᵇ;
private static readonly @string twoˢᵇ = "two"u8;
internal static ref readonly @string threeˢ => ref threeˢᵇ;
private static readonly @string threeˢᵇ = "three"u8;
internal static ref readonly @string fourˢ => ref fourˢᵇ;
private static readonly @string fourˢᵇ = "four"u8;
internal static ref readonly @string fiveˢ => ref fiveˢᵇ;
private static readonly @string fiveˢᵇ = "five"u8;
internal static ref readonly @string sixˢ => ref sixˢᵇ;
private static readonly @string sixˢᵇ = "six"u8;

public static void TestIndirectSliceMapArray(ж<testing.T> t) {
    // Marshal indirect, unmarshal to direct.
    ref var i = ref heap<ж<Indirect>>(@new<Indirect>(), out var Ꮡi);
    i.A = @new<ж<ж<array<nint>>>>();
    i.A.ValueSlot = @new<ж<array<nint>>>();
    (i.A.ValueSlot).ValueSlot = Ꮡ(new array<nint>(3));
    ((i.A.ValueSlot).ValueSlot).Value = new nint[]{1, 2, 3}.array();
    i.S = @new<ж<ж<slice<nint>>>>();
    i.S.ValueSlot = @new<ж<slice<nint>>>();
    (i.S.ValueSlot).ValueSlot = @new<slice<nint>>();
    ((i.S.ValueSlot).ValueSlot).ValueSlot = [4, 5, 6];
    i.M = @new<ж<ж<ж<map<@string, nint>>>>>();
    i.M.ValueSlot = @new<ж<ж<map<@string, nint>>>>();
    (i.M.ValueSlot).ValueSlot = @new<ж<map<@string, nint>>>();
    ((i.M.ValueSlot).ValueSlot).ValueSlot = @new<map<@string, nint>>();
    (((i.M.ValueSlot).ValueSlot).ValueSlot).ValueSlot = new map<@string, nint>{["one"] = 1, ["two"] = 2, ["three"] = 3};
    var b = @new<bytes.Buffer>();
    NewEncoder(new gob_test_package.bytes_BufferжWriter(b)).Encode(i.OrTypedNil());
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    ref var d = ref heap(new Direct(), out var Ꮡd);
    var err = dec.Decode(Ꮡd);
    if (err != nil) {
        t.Error(errorˢ, err);
    }
    if (len(d.A) != 3 || d.A[0] != 1 || d.A[1] != 2 || d.A[2] != 3) {
        t.Errorf("indirect to direct: d.A is %v not %v", d.A, ((i.A.ValueSlot).ValueSlot).Value);
    }
    if (len(d.S) != 3 || d.S[0] != 4 || d.S[1] != 5 || d.S[2] != 6) {
        t.Errorf("indirect to direct: d.S is %v not %v", d.S, ((i.S.ValueSlot).ValueSlot).ValueSlot);
    }
    if (len(d.M) != 3 || d.M[oneˢ] != 1 || d.M[twoˢ] != 2 || d.M[threeˢ] != 3) {
        t.Errorf("indirect to direct: d.M is %v not %v", d.M, ((i.M.ValueSlot).ValueSlot).ValueSlot.OrTypedNil());
    }
    // Marshal direct, unmarshal to indirect.
    d.A = new nint[]{11, 22, 33}.array();
    d.S = [44, 55, 66];
    d.M = new map<@string, nint>{["four"] = 4, ["five"] = 5, ["six"] = 6};
    i = @new<Indirect>();
    b.Reset();
    NewEncoder(new gob_test_package.bytes_BufferжWriter(b)).Encode(d);
    dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    err = dec.Decode(Ꮡi);
    if (err != nil) {
        t.Fatal(errorˢ, err);
    }
    if (len(((i.A.ValueSlot).ValueSlot).Value) != 3 || (((i.A.ValueSlot).ValueSlot).Value)[0] != 11 || (((i.A.ValueSlot).ValueSlot).Value)[1] != 22 || (((i.A.ValueSlot).ValueSlot).Value)[2] != 33) {
        t.Errorf("direct to indirect: ***i.A is %v not %v", ((i.A.ValueSlot).ValueSlot).Value, d.A);
    }
    if (len(((i.S.ValueSlot).ValueSlot).ValueSlot) != 3 || (((i.S.ValueSlot).ValueSlot).ValueSlot)[0] != 44 || (((i.S.ValueSlot).ValueSlot).ValueSlot)[1] != 55 || (((i.S.ValueSlot).ValueSlot).ValueSlot)[2] != 66) {
        t.Errorf("direct to indirect: ***i.S is %v not %v", ((i.S.ValueSlot).ValueSlot).ValueSlot, ((i.S.ValueSlot).ValueSlot).ValueSlot);
    }
    if (len((((i.M.ValueSlot).ValueSlot).ValueSlot).ValueSlot) != 3 || ((((i.M.ValueSlot).ValueSlot).ValueSlot).ValueSlot)[fourˢ] != 4 || ((((i.M.ValueSlot).ValueSlot).ValueSlot).ValueSlot)[fiveˢ] != 5 || ((((i.M.ValueSlot).ValueSlot).ValueSlot).ValueSlot)[sixˢ] != 6) {
        t.Errorf("direct to indirect: ****i.M is %v not %v", (((i.M.ValueSlot).ValueSlot).ValueSlot).ValueSlot, d.M);
    }
}

// An interface with several implementations
public partial interface Squarer {
    nint Square();
}

public partial struct Int /*num:nint*/;

public static nint Square(this Int i) {
    return (nint)(i * i);
}

public partial struct Float /*num:float64*/;

public static nint Square(this Float f) {
    return (nint)(f * f);
}

public partial struct Vector /*[]nint*/;

public static nint Square(this Vector v) {
    nint sum = 0;
    foreach (var (_, x) in range<nint>(v)) {
        sum += x * x;
    }
    return sum;
}

public partial struct Point {
    public nint X, Y;
}

public static nint Square(this Point p) {
    return p.X * p.X + p.Y * p.Y;
}

// A struct with interfaces in it.
public partial struct InterfaceItem {
    public nint I;
    public Squarer Sq1, Sq2, Sq3;
    public float64 F;
    public slice<Squarer> Sq;
}

// The same struct without interfaces
public partial struct NoInterfaceItem {
    public nint I;
    public float64 F;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedNoEncodeErrorGotˢ = (@string)"expected no encode error; got"u8;
internal static readonly object normalIntDidNotDecodeˢ = (@string)"normal int did not decode correctly"u8;
internal static readonly object intDidNotDecodeCorrectlyˢ = (@string)"Int did not decode correctly"u8;
internal static readonly object floatDidNotDecodeˢ = (@string)"Float did not decode correctly"u8;
internal static readonly object vectorDidNotDecodeˢ = (@string)"Vector did not decode correctly"u8;
internal static readonly object normalFloatDidNotDecodeˢ = (@string)"normal float did not decode correctly"u8;

public static void TestInterface(ж<testing.T> t) {
    ref var iVal = ref heap<Int>((Int)3, out var ᏑiVal);
    ref var fVal = ref heap<Float>((Float)5D, out var ᏑfVal);
    // Sending a Vector will require that the receiver define a type in the middle of
    // receiving the value for item2.
    var vVal = new Vector(new nint[]{1, 2, 3}.slice());
    var b = @new<bytes.Buffer>();
    var item1 = Ꮡ(new InterfaceItem(1, iVal, fVal, vVal, 11.5D, [iVal, fVal, default, vVal]));
    // Register the types.
    Register((Int)0);
    Register((Float)0D);
    Register(new Vector(new nint[]{}.slice()));
    var err = NewEncoder(new gob_test_package.bytes_BufferжWriter(b)).Encode(item1.OrTypedNil());
    if (err != nil) {
        t.Error(expectedNoEncodeErrorGotˢ, err);
    }

    ref var item2 = ref heap<InterfaceItem>(new InterfaceItem(), out var Ꮡitem2);
    err = NewDecoder(new gob_test_package.bytes_BufferжReader(b)).Decode(Ꮡitem2);
    if (err != nil) {
        t.Fatal(decodeˢ, err);
    }
    if (item2.I != item1.I) {
        t.Error(normalIntDidNotDecodeˢ);
    }
    if (item2.Sq1 == nil || item2.Sq1.Square() != iVal.Square()) {
        t.Error(intDidNotDecodeCorrectlyˢ);
    }
    if (item2.Sq2 == nil || item2.Sq2.Square() != fVal.Square()) {
        t.Error(floatDidNotDecodeˢ);
    }
    if (item2.Sq3 == nil || item2.Sq3.Square() != vVal.Square()) {
        t.Error(vectorDidNotDecodeˢ);
    }
    if (item2.F != item1.F) {
        t.Error(normalFloatDidNotDecodeˢ);
    }
    // Now check that we received a slice of Squarers correctly, including a nil element
    if (len(item1.Sq) != len(item2.Sq)) {
        t.Fatalf("[]Squarer length wrong: got %d; expected %d", len(item2.Sq), len(item1.Sq));
    }
    foreach (var (i, v1) in range(item1.Sq)) {
        var v2 = item2.Sq[i];
        if (v1 == nil || v2 == nil) {
            if (v1 != nil || v2 != nil) {
                t.Errorf("item %d inconsistent nils", i);
            }
        } else if (v1.Square() != v2.Square()) {
            t.Errorf("item %d inconsistent values: %v %v", i, v1, v2);
        }
    }
}

// A struct with all basic types, stored in interfaces.
public partial struct BasicInterfaceItem {
    public any Int, Int8, Int16, Int32, Int64;
    public any Uint, Uint8, Uint16, Uint32, Uint64;
    public any Float32, Float64;
    public any Complex64, Complex128;
    public any Bool;
    public any String;
    public any Bytes;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object booleanShouldBeTrueˢ = (@string)"boolean should be true"u8;

public static void TestInterfaceBasic(ж<testing.T> t) {
    var b = @new<bytes.Buffer>();
    var item1 = Ꮡ(new BasicInterfaceItem(
        (nint)1, (int8)1, (int16)1, (int32)1, (int64)1,
        (nuint)1, (uint8)1, (uint16)1, (uint32)1, (uint64)1,
        (float32)1F, 1.0D,
        (complex64)1F.i(), (complex128)1D.i(),
        true,
        helloˢ,
        slice<byte>("sailor"u8)
    ));
    var err = NewEncoder(new gob_test_package.bytes_BufferжWriter(b)).Encode(item1.OrTypedNil());
    if (err != nil) {
        t.Error(expectedNoEncodeErrorGotˢ, err);
    }

    ref var item2 = ref heap<ж<BasicInterfaceItem>>(Ꮡ(new BasicInterfaceItem()), out var Ꮡitem2);
    err = NewDecoder(new gob_test_package.bytes_BufferжReader(b)).Decode(Ꮡitem2);
    if (err != nil) {
        t.Fatal(decodeˢ, err);
    }
    if (!reflect.DeepEqual(item1.OrTypedNil(), item2.OrTypedNil())) {
        t.Errorf("encode expected %v got %v", item1.OrTypedNil(), item2.OrTypedNil());
    }
    // Hand check a couple for correct types.
    {
        var (v, ok) = item2.Bool._<bool>(ᐧ); if (!ok || !v) {
            t.Error(booleanShouldBeTrueˢ);
        }
    }
    {
        var (v, ok) = item2.String._<@string>(ᐧ); if (!ok || v != item1.String._<@string>()) {
            t.Errorf("string should be %v is %v", item1.String, v);
        }
    }
}

public partial struct ΔString /*@string*/;

public partial struct PtrInterfaceItem {
    public any Str1; // basic
    public any Str2; // derived
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string howdyˢ => ref howdyˢᵇ;
private static readonly @string howdyˢᵇ = "howdy"u8;
internal static ref readonly @string kiddoˢ => ref kiddoˢᵇ;
private static readonly @string kiddoˢᵇ = "kiddo"u8;

// We'll send pointers; should receive values.
// Also check that we can register T but send *T.
public static void TestInterfacePointer(ж<testing.T> t) {
    var b = @new<bytes.Buffer>();
    ref var str1 = ref heap<@string>(howdyˢ, out var Ꮡstr1);
    ref var str2 = ref heap<ΔString>((ΔString)(@string)kiddoˢ, out var Ꮡstr2);
    var item1 = Ꮡ(new PtrInterfaceItem(
        Ꮡstr1,
        Ꮡstr2
    ));
    // Register the type.
    Register(str2);
    var err = NewEncoder(new gob_test_package.bytes_BufferжWriter(b)).Encode(item1.OrTypedNil());
    if (err != nil) {
        t.Error(expectedNoEncodeErrorGotˢ, err);
    }

    ref var item2 = ref heap<ж<PtrInterfaceItem>>(Ꮡ(new PtrInterfaceItem()), out var Ꮡitem2);
    err = NewDecoder(new gob_test_package.bytes_BufferжReader(b)).Decode(Ꮡitem2);
    if (err != nil) {
        t.Fatal(decodeˢ, err);
    }
    // Hand test for correct types and values.
    {
        var (v, ok) = item2.Str1._<@string>(ᐧ); if (!ok || v != str1) {
            t.Errorf("basic string failed: %q should be %q", v, str1);
        }
    }
    {
        var (v, ok) = item2.Str2._<ΔString>(ᐧ); if (!ok || v != str2) {
            t.Errorf("derived type String failed: %q should be %q", v, str2);
        }
    }
}

public static void TestIgnoreInterface(ж<testing.T> t) {
    ref var iVal = ref heap<Int>((Int)3, out var ᏑiVal);
    ref var fVal = ref heap<Float>((Float)5D, out var ᏑfVal);
    // Sending a Point will require that the receiver define a type in the middle of
    // receiving the value for item2.
    ref var pVal = ref heap<Point>(new Point(2, 3), out var ᏑpVal);
    var b = @new<bytes.Buffer>();
    var item1 = Ꮡ(new InterfaceItem(1, iVal, fVal, pVal, 11.5D, default));
    // Register the types.
    Register((Int)0);
    Register((Float)0D);
    Register(new Point());
    var err = NewEncoder(new gob_test_package.bytes_BufferжWriter(b)).Encode(item1.OrTypedNil());
    if (err != nil) {
        t.Error(expectedNoEncodeErrorGotˢ, err);
    }

    ref var item2 = ref heap<NoInterfaceItem>(new NoInterfaceItem(), out var Ꮡitem2);
    err = NewDecoder(new gob_test_package.bytes_BufferжReader(b)).Decode(Ꮡitem2);
    if (err != nil) {
        t.Fatal(decodeˢ, err);
    }
    if (item2.I != item1.I) {
        t.Error(normalIntDidNotDecodeˢ);
    }
    if (item2.F != item1.F) {
        t.Error(normalFloatDidNotDecodeˢ);
    }
}

public partial struct U {
    public nint A;
    public @string B;
    internal float64 c;
    public nuint D;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object u1CModifiedˢ = (@string)"u1.c modified"u8;

public static void TestUnexportedFields(ж<testing.T> t) {
    U u0 = default;
    u0.A = 17;
    u0.B = helloˢ;
    u0.c = 3.14159D;
    u0.D = 23;
    var b = @new<bytes.Buffer>();
    NewEncoder(new gob_test_package.bytes_BufferжWriter(b)).Encode(u0);
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    ref var u1 = ref heap(new U(), out var Ꮡu1);
    u1.c = 1234D;
    var err = dec.Decode(Ꮡu1);
    if (err != nil) {
        t.Fatal(decodeErrorˢ, err);
    }
    if (u0.A != u1.A || u0.B != u1.B || u0.D != u1.D) {
        t.Errorf("u1->u0: expected %v; got %v", u0, u1);
    }
    if (u1.c != 1234D) {
        t.Error(u1CModifiedˢ);
    }
}

internal static slice<any> singletons = [
    true,
    (nint)7,
    (nuint)10,
    3.2D,
    (@string)"hello",
    new nint[]{11, 22, 33}.array(),
    slice<float32>([0.5F, 0.25F, 0.125F]),
    new map<@string, nint>{["one"] = 1, ["two"] = 2}
];

public static void TestDebugSingleton(ж<testing.T> t) {
    if (debugFunc == nil) {
        return;
    }
    var b = @new<bytes.Buffer>();
    // Accumulate a number of values and print them out all at once.
    foreach (var (_, x) in range(singletons)) {
        var err = NewEncoder(new gob_test_package.bytes_BufferжWriter(b)).Encode(x);
        if (err != nil) {
            t.Fatal(encodeˢ, err);
        }
    }
    debugFunc(new gob_test_package.bytes_BufferжReader(b));
}

// A type that won't be defined in the gob until we send it in an interface value.
public partial struct OnTheFly {
    public nint A;
}

public partial struct DT {
    //	X OnTheFly
    public nint A;
    public @string B;
    public float64 C;
    public any I;
    public any J;
    public any I_nil;
    public map<@string, nint> M;
    public array<nint> T = new(3);
    public slice<@string> S;
}

internal static DT newDT() {
    DT dt = new();
    dt.A = 17;
    dt.B = helloˢ;
    dt.C = 3.14159D;
    dt.I = (nint)271828;
    dt.J = new OnTheFly(3);
    dt.I_nil = default;
    dt.M = new map<@string, nint>{["one"] = 1, ["two"] = 2};
    dt.T = new nint[]{11, 22, 33}.array();
    dt.S = ["hi", "joe"];
    return dt.ΔClone();
}

public static void TestDebugStruct(ж<testing.T> t) {
    if (debugFunc == nil) {
        return;
    }
    Register(new OnTheFly());
    var dt = newDT();
    var b = @new<bytes.Buffer>();
    var err = NewEncoder(new gob_test_package.bytes_BufferжWriter(b)).Encode(dt);
    if (err != nil) {
        t.Fatal(encodeˢ, err);
    }
    var debugBuffer = bytes.NewBuffer(b.Bytes());
    ref var dt2 = ref heap<ж<DT>>(Ꮡ(new DT()), out var Ꮡdt2);
    err = NewDecoder(new gob_test_package.bytes_BufferжReader(b)).Decode(Ꮡdt2);
    if (err != nil) {
        t.Error(decodeˢ, err);
    }
    debugFunc(new gob_test_package.bytes_BufferжReader(debugBuffer));
}

internal static error encFuzzDec(ж<rand.Rand> Ꮡrng, any @inʗp) {
    ref var rng = ref Ꮡrng.DerefOrNull();
    ref var @in = ref heap(@inʗp, out var Ꮡin);

    var buf = @new<bytes.Buffer>();
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(buf));
    {
        var err = enc.Encode(Ꮡin); if (err != nil) {
            return err;
        }
    }

    var b = buf.Bytes();
    foreach (var (i, bi) in range(b)) {
        if (rng.Intn(10) < 3) {
            b[i] = (byte)(bi + (uint8)rng.Intn(256));
        }
    }

    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(buf));
    ref var e = ref heap<any>(out var Ꮡe);
    {
        var err = dec.Decode(Ꮡe); if (err != nil) {
            return err;
        }
    }
    return default;
}

// This does some "fuzz testing" by attempting to decode a sequence of random bytes.
public static void TestFuzz(ж<testing.T> t) {
    if (!doFuzzTests.Value) {
        t.Skipf("disabled; run with -gob.fuzz to enable");
    }

    // all possible inputs
    var input = slice<any>([
        @new<nint>(),
        @new<float32>(),
        @new<float64>(),
        @new<complex128>(),
        Ꮡ(new ByteStruct(255)),
        Ꮡ(new ArrayStruct()),
        Ꮡ(new StringStruct("hello")),
        Ꮡ(new GobTest1(0, Ꮡ(new StringStruct("hello"))))
    ]);
    testFuzz(t, time.Now().UnixNano(), 100, input.ꓸꓸꓸ);
}

public static void TestFuzzRegressions(ж<testing.T> t) {
    if (!doFuzzTests.Value) {
        t.Skipf("disabled; run with -gob.fuzz to enable");
    }

    // An instance triggering a type name of length ~102 GB.
    testFuzz(t, 1328492090837718000L, 100, @new<float32>());
    // An instance triggering a type name of 1.6 GB.
    // Note: can take several minutes to run.
    testFuzz(t, 1330522872628565000L, 100, @new<nint>());
}

internal static void testFuzz(ж<testing.T> t, int64 seed, nint n, params ꓸꓸꓸany inputʗp) {
    var input = inputʗp.sslice();

    foreach (var (_, e) in range(input)) {
        t.Logf("seed=%d n=%d e=%T", seed, n, e);
        var rng = rand.New(rand.NewSource(seed));
        for (nint i = 0; i < n; i++) {
            encFuzzDec(rng, e);
        }
    }
}

// TestFuzzOneByte tries to decode corrupted input sequences
// and checks that no panic occurs.
public static void TestFuzzOneByte(ж<testing.T> t) {
    var indicesᴷ = new stackAppend<nint>();

    if (!doFuzzTests.Value) {
        t.Skipf("disabled; run with -gob.fuzz to enable");
    }

    var buf = @new<strings.Builder>();
    Register(new OnTheFly());
    var dt = newDT();
    {
        var err = NewEncoder(new gob_test_package.strings_BuilderжWriter(buf)).Encode(dt); if (err != nil) {
            t.Fatal(err);
        }
    }
    @string s = buf.String();

    var indices = new slice<nint>(0, len(s));
    for (nint i = 0; i < len(s); i++) {
        switch (i) {
        case 14 or 167 or 231 or 265: { // a slice length, corruptions are not handled yet.
            continue;
            break;
        }
        case 248: {
            // Large map size, which currently causes an out of memory panic.
            // See golang.org/issue/24308 and golang.org/issue/20221.
            continue;
            break;
        }}
        indices = append(indicesᴷ, indices, i);
    }
    if (testing.Short()) {
        indices = [1, 111, 178]; // known fixed panics
    }
    foreach (var (_, i) in range(indices)) {
        for (nint jᴛ1 = 0; jᴛ1 < 256; jᴛ1 += 3) {
            var j = jᴛ1;
            var b = slice<byte>(s);
            b[i] ^= (byte)((byte)j);
            ref var e = ref heap(new DT(), out var Ꮡe);
            var bʗ1 = b;
            func(() => {
                GoFrame ᒐ = default;
                try {
                    defer(() => {
                        var p = recover(); if (p != nil) {
                            t.Errorf("crash for b[%d] ^= 0x%x", i, j);
                            throw panic(p);
                        }
                    }, ref ᒐ);
                    var err = NewDecoder(new gob_test_package.bytes_ReaderжReader(bytes.NewReader(bʗ1))).Decode(Ꮡe);
                    _ = err;
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            })();
        }
    }
}

// Don't crash, just give error with invalid type id.
// Issue 9649.
public static void TestErrorInvalidTypeId(ж<testing.T> t) {
    var data = slice<byte>([0x01, 0x00, 0x01, 0x00]);
    var d = NewDecoder(new gob_test_package.bytes_ReaderжReader(bytes.NewReader(data)));
    // When running d.Decode(&foo) the first time the decoder stops
    // after []byte{0x01, 0x00} and reports an errBadType. Running
    // d.Decode(&foo) again on exactly the same input sequence should
    // give another errBadType, but instead caused a panic because
    // decoderMap wasn't cleaned up properly after the first error.
    for (nint i = 0; i < 2; i++) {
        ref var foo = ref heap(new EmptyStruct(), out var Ꮡfoo);
        var err = d.Decode(Ꮡfoo);
        if (!AreEqual(err, errBadType)) {
            t.Fatalf("decode: expected %s, got %s", errBadType, err);
        }
    }
}

public partial struct LargeSliceByte {
    public slice<byte> S;
}

public partial struct LargeSliceInt8 {
    public slice<int8> S;
}

public partial struct StringPair {
    public @string A, B;
}

public partial struct LargeSliceStruct {
    public slice<StringPair> S;
}

public partial struct LargeSliceString {
    public slice<@string> S;
}

internal static void testEncodeDecode(ж<testing.T> t, any @in, any @out) {
    t.Helper();
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    var err = NewEncoder(new gob_test_package.bytes_BufferжWriter(Ꮡb)).Encode(@in);
    if (err != nil) {
        t.Fatal(encodeˢ, err);
    }
    err = NewDecoder(new gob_test_package.bytes_BufferжReader(Ꮡb)).Decode(@out);
    if (err != nil) {
        t.Fatal(decodeˢ, err);
    }
    if (!reflect.DeepEqual(@in, @out)) {
        t.Errorf("output mismatch");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string byteˢ => ref byteˢᵇ;
private static readonly @string byteˢᵇ = "byte"u8;
internal static ref readonly @string int8ˢ => ref int8ˢᵇ;
private static readonly @string int8ˢᵇ = "int8"u8;
internal static ref readonly @string structˢ => ref structˢᵇ;
private static readonly @string structˢᵇ = "struct"u8;
internal static ref readonly @string stringˢ => ref stringˢᵇ;
private static readonly @string stringˢᵇ = "string"u8;

public static void TestLargeSlice(ж<testing.T> t) {
    t.Run(byteˢ, (ж<testing.T> tΔ1) => {
        if (/* unsafe.Sizeof(uintptr(0)) */ (uintptr)8 > 4) {
            tΔ1.Parallel(); // Only run in parallel in a large address space
        }
        var s = new slice<byte>((10 << 21));
        foreach (var i in range(len(s))) {
            s[i] = (byte)i;
        }
        var st = Ꮡ(new LargeSliceByte(S: s));
        var rt = Ꮡ(new LargeSliceByte());
        testEncodeDecode(tΔ1, st.OrTypedNil(), rt.OrTypedNil());
    });
    t.Run(int8ˢ, (ж<testing.T> tΔ2) => {
        if (/* unsafe.Sizeof(uintptr(0)) */ (uintptr)8 > 4) {
            tΔ2.Parallel();
        }
        var s = new slice<int8>((10 << 21));
        foreach (var i in range(len(s))) {
            s[i] = (int8)i;
        }
        var st = Ꮡ(new LargeSliceInt8(S: s));
        var rt = Ꮡ(new LargeSliceInt8());
        testEncodeDecode(tΔ2, st.OrTypedNil(), rt.OrTypedNil());
    });
    t.Run(structˢ, (ж<testing.T> tΔ3) => {
        if (/* unsafe.Sizeof(uintptr(0)) */ (uintptr)8 > 4) {
            tΔ3.Parallel();
        }
        var s = new slice<StringPair>((1 << 21));
        foreach (var i in range(len(s))) {
            s[i].A = (@string)(rune)i;
            s[i].B = s[i].A;
        }
        var st = Ꮡ(new LargeSliceStruct(S: s));
        var rt = Ꮡ(new LargeSliceStruct());
        testEncodeDecode(tΔ3, st.OrTypedNil(), rt.OrTypedNil());
    });
    t.Run(stringˢ, (ж<testing.T> tΔ4) => {
        if (/* unsafe.Sizeof(uintptr(0)) */ (uintptr)8 > 4) {
            tΔ4.Parallel();
        }
        var s = new slice<@string>((1 << 21));
        foreach (var i in range(len(s))) {
            s[i] = (@string)(rune)i;
        }
        var st = Ꮡ(new LargeSliceString(S: s));
        var rt = Ꮡ(new LargeSliceString());
        testEncodeDecode(tΔ4, st.OrTypedNil(), rt.OrTypedNil());
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object encodeDecodeExpectedˢ = (@string)"Encode/Decode: expected error but got err == nil"u8;

public static void TestLocalRemoteTypesMismatch(ж<testing.T> t) {
    // Test data is from https://go.dev/issue/62117.
    var testData = slice<byte>([9, 127, 3, 1, 2, 255, 128, 0, 0, 0, 3, 255, 128, 0]);

    ref var v = ref heap<slice<ж<EmptyStruct>>>(out var Ꮡv);
    var buf = bytes.NewBuffer(testData);
    var err = NewDecoder(new gob_test_package.bytes_BufferжReader(buf)).Decode(Ꮡv);
    if (err == nil) {
        t.Error(encodeDecodeExpectedˢ);
    }
}

} // end gob_internal_test_package
