// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime;

using bytes = bytes_package;
using binary = encoding.binary_package;
using fmt = fmt_package;
using maps = go.@internal.runtime.maps_package;
using reflect = reflect_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using abi = go.@internal.abi_package;
using encoding;
using go.@internal.runtime;
using io = io_package;
using static go.@internal.runtime.maps_internal_test_package;

partial class maps_test_package {

// The input to FuzzTable is a binary-encoded array of fuzzCommand structs.
//
// Each fuzz call begins with an empty Map[uint16, uint32].
//
// Each command is then executed on the map in sequence. Operations with
// output (e.g., Get) are verified against a reference map.
partial struct fuzzCommand {
    public fuzzOp Op;
    // Used for Get, Put, Delete.
    public uint16 Key;
    // Used for Put.
    public uint32 Elem;
}

// Encoded size of fuzzCommand.
internal static nint fuzzCommandSize = binary.Size(new fuzzCommand());

public partial struct fuzzOp /*num:uint8*/;

internal static fuzzOp fuzzOpGet => /* iota */ 0;
internal static fuzzOp fuzzOpPut => 1;
internal static fuzzOp fuzzOpDelete => 2;

internal static slice<byte> encode(slice<fuzzCommand> fc) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    {
        var err = binary.Write(new maps_test_package.bytes_BufferжWriter(Ꮡbuf), binary.LittleEndian, fc); if (err != nil) {
            throw panic(fmt.Sprintf("error writing %v: %v"u8, fc, err));
        }
    }
    return buf.Bytes();
}

internal static slice<fuzzCommand> decode(slice<byte> b) {
    // Round b down to a multiple of fuzzCommand size. i.e., ignore extra
    // bytes of input.
    nint entries = len(b) / fuzzCommandSize;
    nint usefulSize = entries * fuzzCommandSize;
    b = b.slice(0, usefulSize);

    ref var fc = ref heap<slice<fuzzCommand>>(new slice<fuzzCommand>(entries), out var Ꮡfc);
    var buf = bytes.NewReader(b);
    {
        var err = binary.Read(new maps_test_package.bytes_ReaderжReader(buf), binary.LittleEndian, Ꮡfc); if (err != nil) {
            throw panic(fmt.Sprintf("error reading %v: %v"u8, b, err));
        }
    }

    return fc;
}

public static void TestEncodeDecode(ж<testing.T> t) {
    var fc = slice<fuzzCommand>([
        new(
            Op: fuzzOpPut,
            Key: 123,
            Elem: 456
        ),
        new(
            Op: fuzzOpGet,
            Key: 123
        )
    ]);

    var b = encode(fc);
    var got = decode(b);
    if (!reflect.DeepEqual(fc, got)) {
        t.Errorf("encode-decode roundtrip got %+v want %+v", got, fc);
    }

    // Extra trailing bytes ignored.
    b = append(b, (byte)42);
    got = decode(b);
    if (!reflect.DeepEqual(fc, got)) {
        t.Errorf("encode-decode (extra byte) roundtrip got %+v want %+v", got, fc);
    }
}

public static void FuzzTable(ж<testing.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    // All of the ops.
    f.Add(encode(slice<fuzzCommand>([
        new(
            Op: fuzzOpPut,
            Key: 123,
            Elem: 456
        ),
        new(
            Op: fuzzOpDelete,
            Key: 123
        ),
        new(
            Op: fuzzOpGet,
            Key: 123
        )
    ])));

    // Add enough times to trigger grow.
    f.Add(encode(slice<fuzzCommand>([
        new(
            Op: fuzzOpPut,
            Key: 1,
            Elem: 101
        ),
        new(
            Op: fuzzOpPut,
            Key: 2,
            Elem: 102
        ),
        new(
            Op: fuzzOpPut,
            Key: 3,
            Elem: 103
        ),
        new(
            Op: fuzzOpPut,
            Key: 4,
            Elem: 104
        ),
        new(
            Op: fuzzOpPut,
            Key: 5,
            Elem: 105
        ),
        new(
            Op: fuzzOpPut,
            Key: 6,
            Elem: 106
        ),
        new(
            Op: fuzzOpPut,
            Key: 7,
            Elem: 107
        ),
        new(
            Op: fuzzOpPut,
            Key: 8,
            Elem: 108
        ),
        new(
            Op: fuzzOpGet,
            Key: 1
        ),
        new(
            Op: fuzzOpDelete,
            Key: 2
        ),
        new(
            Op: fuzzOpPut,
            Key: 2,
            Elem: 42
        ),
        new(
            Op: fuzzOpGet,
            Key: 2
        )
    ])));

    Ꮡf.Fuzz((ж<testing.T> t, slice<byte> @in) => {
        var fc = decode(@in);
        if (len(fc) == 0) {
            return;
        }

        var (m, typ) = maps_internal_test_package.NewTestMap<uint16, uint32>(8);
        var @ref = new map<uint16, uint32>();
        foreach (var (_, vᴛ1) in fc) {
            ref var c = ref heap(new fuzzCommand(), out var Ꮡc);
            c = vᴛ1;

            switch (c.Op) {
            case var ᴛ when ᴛ == fuzzOpGet: {
                var (elemPtr, ok) = m.Get(typ, @unsafe.Pointer.FromPinnedBox(Ꮡc.ᏑKey));
                var (refElem, refOK) = @ref[c.Key, ꟷ];

                if (ok != refOK) {
                    t.Errorf("Get(%d) got ok %v want ok %v", c.Key, ok, refOK);
                }
                if (!ok) {
                    continue;
                }
                var gotElem = ~(ж<uint32>)(uintptr)(elemPtr);
                if (gotElem != refElem) {
                    t.Errorf("Get(%d) got %d want %d", c.Key, gotElem, refElem);
                }
                break;
            }
            case var ᴛ when ᴛ == fuzzOpPut: {
                m.Put(typ, @unsafe.Pointer.FromPinnedBox(Ꮡc.ᏑKey), @unsafe.Pointer.FromPinnedBox(Ꮡc.ᏑElem));
                @ref[c.Key] = c.Elem;
                break;
            }
            case var ᴛ when ᴛ == fuzzOpDelete: {
                m.Delete(typ, @unsafe.Pointer.FromPinnedBox(Ꮡc.ᏑKey));
                delete(@ref, c.Key);
                break;
            }
            default: {
                // Just skip this command to keep the fuzzer
                // less constrained.
                continue;
                break;
            }}
        }
    });
}

} // end maps_test_package
