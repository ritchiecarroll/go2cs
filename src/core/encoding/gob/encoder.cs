// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using errors = errors_package;
using io = io_package;
using reflect = reflect_package;
using sync = sync_package;

partial class gob_package {

// An Encoder manages the transmission of type and data information to the
// other side of a connection.  It is safe for concurrent use by multiple
// goroutines.
partial struct Encoder {
    internal sync.Mutex mutex;              // each item must be sent atomically
    internal slice<io.Writer> w;        // where to send the data
    internal map<reflectꓸType, typeId> sent; // which types we've already sent
    internal ж<encoderState> countState;        // stage for writing counts
    internal ж<encoderState> freeList;        // list of free encoderStates; avoids reallocation
    internal encBuffer byteBuf;               // buffer for top-level encoderState
    internal error err;
}

// Before we encode a message, we reserve space at the head of the
// buffer in which to encode its length. This means we can use the
// buffer to assemble the message without another allocation.
internal static UntypedInt maxLength => 9; // Maximum size of an encoded length.

internal static slice<byte> spaceForLength = new slice<byte>(maxLength);

// NewEncoder returns a new encoder that will transmit on the [io.Writer].
public static ж<Encoder> NewEncoder(io.Writer w) {
    var enc = @new<Encoder>();
    enc.Value.w = new io.Writer[]{w}.slice();
    enc.Value.sent = new map<reflectꓸType, typeId>();
    enc.Value.countState = enc.newEncoderState(@new<encBuffer>());
    return enc;
}

// writer returns the innermost writer the encoder is using.
internal static io.Writer writer(this ref Encoder enc) {
    return enc.w[len(enc.w) - 1];
}

// pushWriter adds a writer to the encoder.
internal static void pushWriter(this ref Encoder enc, io.Writer w) {
    enc.w = append(enc.w, w);
}

// popWriter pops the innermost writer.
internal static void popWriter(this ref Encoder enc) {
    enc.w = enc.w.slice(0, len(enc.w) - 1);
}

internal static void setError(this ref Encoder enc, error err) {
    if (enc.err == default!) {
        // remember the first.
        enc.err = err;
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gobEncoderMessageTooBigˢ = "gob: encoder: message too big"u8;

// writeMessage sends the data item preceded by an unsigned count of its length.
internal static void writeMessage(this ref Encoder enc, io.Writer w, ж<encBuffer> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    // Space has been reserved for the length at the head of the message.
    // This is a little dirty: we grab the slice from the bytes.Buffer and massage
    // it by hand.
    var message = b.Bytes();
    nint messageLen = len(message) - (nint)maxLength;
    // Length cannot be bigger than the decoder can handle.
    if (messageLen >= tooBig) {
        enc.setError(errors.New(gobEncoderMessageTooBigˢ));
        return;
    }
    // Encode the length.
    (~enc.countState).b.Reset();
    enc.countState.encodeUint((uint64)messageLen);
    // Copy the length to be a prefix of the message.
    nint offset = (nint)maxLength - (~enc.countState).b.Len();
    copy(message.slice(offset), (~enc.countState).b.Bytes());
    // Write the data.
    var (_, err) = w.Write(message.slice(offset));
    // Drain the buffer and restore the space at the front for the count of the next message.
    b.Reset();
    b.Write(spaceForLength);
    if (err != default!) {
        enc.setError(err);
    }
}

// sendActualType sends the requested type, without further investigation, unless
// it's been sent before.
internal static bool /*sent*/ sendActualType(this ж<Encoder> Ꮡenc, io.Writer w, ж<encoderState> Ꮡstate, ж<userTypeInfo> Ꮡut, reflectꓸType actual) {
    bool sent = default!;

    ref var enc = ref Ꮡenc.DerefOrNull();
    ref var state = ref Ꮡstate.DerefOrNull();
    ref var ut = ref Ꮡut.DerefOrNull();
    {
        var (_, alreadySent) = enc.sent[actual, ꟷ]; if (alreadySent) {
            return false;
        }
    }
    var (info, err) = getTypeInfo(ref (Ꮡut).DerefOrNull());
    if (err != default!) {
        enc.setError(err);
        return sent;
    }
    // Send the pair (-id, type)
    // Id:
    state.encodeInt(-(int64)(int32)(~info).id);
    // Type:
    Ꮡenc.encode(state.b, reflect.ValueOf((~info).wire), wireTypeUserInfo);
    enc.writeMessage(w, state.b);
    if (enc.err != default!) {
        return sent;
    }
    // Remember we've sent this type, both what the user gave us and the base type.
    enc.sent[ut.@base] = info.Value.id;
    if (!AreEqual(ut.user, ut.@base)) {
        enc.sent[ut.user] = info.Value.id;
    }
    // Now send the inner types
    {
        var st = actual;
        var exprᴛ1 = st.Kind();
        if (exprᴛ1 == reflect.Struct) {
            for (nint i = 0; i < st.NumField(); i++) {
                if (isExported(st.Field(i).Name)) {
                    Ꮡenc.sendType(w, Ꮡstate, st.Field(i).Type);
                }
            }
        }
        else if (exprᴛ1 == reflect.Array || exprᴛ1 == reflect.ΔSlice) {
            Ꮡenc.sendType(w, Ꮡstate, st.Elem());
        }
        else if (exprᴛ1 == reflect.Map) {
            Ꮡenc.sendType(w, Ꮡstate, st.Key());
            Ꮡenc.sendType(w, Ꮡstate, st.Elem());
        }
    }

    return true;
}

// sendType sends the type info to the other side, if necessary.
internal static bool /*sent*/ sendType(this ж<Encoder> Ꮡenc, io.Writer w, ж<encoderState> Ꮡstate, reflectꓸType origt) {
    bool sent = default!;

    var ut = userType(origt);
    if ((~ut).externalEnc != 0) {
        // The rules are different: regardless of the underlying type's representation,
        // we need to tell the other side that the base type is a GobEncoder.
        return Ꮡenc.sendActualType(w, Ꮡstate, ut, (~ut).@base);
    }
    // It's a concrete value, so drill down to the base type.
    {
        var rt = ut.Value.@base;
        var exprᴛ1 = rt.Kind();
        if (exprᴛ1 == reflect.ΔSlice) {
            do {
                if (rt.Elem().Kind() == reflect.Uint8) {
                    // Basic types and interfaces do not need to be described.
                    // If it's []uint8, don't send; it's considered basic.
                    return sent;
                }
                break;
            } while (false);
        }
        else if (exprᴛ1 == reflect.Array) {
            do {
                break;
            } while (false);
        }
        else if (exprᴛ1 == reflect.Map) {
            do {
                break;
            } while (false);
        }
        else if (exprᴛ1 == reflect.Struct) {
            do {
                break;
            } while (false);
        }
        else if (exprᴛ1 == reflect.Chan || exprᴛ1 == reflect.Func) {
            return sent;
        }
        else { /* default: */
            return sent;
        }
    }

    // Otherwise we do send.
    // arrays must be sent so we know their lengths and element types.
    // maps must be sent so we know their lengths and key/value types.
    // structs must be sent so we know their fields.
    // If we get here, it's a field of a struct; ignore it.
    return Ꮡenc.sendActualType(w, Ꮡstate, ut, (~ut).@base);
}

// Encode transmits the data item represented by the empty interface value,
// guaranteeing that all necessary type information has been transmitted first.
// Passing a nil pointer to Encoder will panic, as they cannot be transmitted by gob.
public static error Encode(this ж<Encoder> Ꮡenc, any e) {
    return Ꮡenc.EncodeValue(reflect.ValueOf(e));
}

// sendTypeDescriptor makes sure the remote side knows about this type.
// It will send a descriptor if this is the first time the type has been
// sent.
internal static void sendTypeDescriptor(this ж<Encoder> Ꮡenc, io.Writer w, ж<encoderState> Ꮡstate, ж<userTypeInfo> Ꮡut) {
    ref var enc = ref Ꮡenc.DerefOrNull();
    ref var ut = ref Ꮡut.DerefOrNull();

    // Make sure the type is known to the other side.
    // First, have we already sent this type?
    var rt = ut.@base;
    if (ut.externalEnc != 0) {
        rt = ut.user;
    }
    {
        var (_, alreadySent) = enc.sent[rt, ꟷ]; if (!alreadySent) {
            // No, so send it.
            var sent = Ꮡenc.sendType(w, Ꮡstate, rt);
            if (enc.err != default!) {
                return;
            }
            // If the type info has still not been transmitted, it means we have
            // a singleton basic type (int, []byte etc.) at top level. We don't
            // need to send the type info but we do need to update enc.sent.
            if (!sent) {
                var (info, err) = getTypeInfo(ref (Ꮡut).DerefOrNull());
                if (err != default!) {
                    enc.setError(err);
                    return;
                }
                enc.sent[rt] = info.Value.id;
            }
        }
    }
}

// sendTypeId sends the id, which must have already been defined.
internal static void sendTypeId(this ref Encoder enc, ж<encoderState> Ꮡstate, ж<userTypeInfo> Ꮡut) {
    ref var state = ref Ꮡstate.DerefOrNull();
    ref var ut = ref Ꮡut.DerefOrNull();

    // Identify the type of this top-level value.
    state.encodeInt((int64)(int32)enc.sent[ut.@base]);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gobCannotEncodeNilValueˢ = "gob: cannot encode nil value"u8;

// EncodeValue transmits the data item represented by the reflection value,
// guaranteeing that all necessary type information has been transmitted first.
// Passing a nil pointer to EncodeValue will panic, as they cannot be transmitted by gob.
public static error EncodeValue(this ж<Encoder> Ꮡenc, reflectꓸValue value) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var enc = ref Ꮡenc.DerefOrNull();

        if (value.Kind() == reflect.Invalid) {
            return errors.New(gobCannotEncodeNilValueˢ);
        }
        if (value.Kind() == reflect.ΔPointer && value.IsNil()) {
            throw panic("gob: cannot encode nil pointer of type " + value.Type().String());
        }
        // Make sure we're single-threaded through here, so multiple
        // goroutines can share an encoder.
        enc.mutex.Lock();
        ᒐd1 = true;
        // Remove any nested writers remaining due to previous errors.
        enc.w = enc.w[0..1];
        var (ut, err) = validUserType(value.Type());
        if (err != default!) {
            return err;
        }
        enc.err = default!;
        enc.byteBuf.Reset();
        enc.byteBuf.Write(spaceForLength);
        var state = Ꮡenc.newEncoderState(Ꮡenc.of(Encoder.ᏑbyteBuf));
        Ꮡenc.sendTypeDescriptor(enc.writer(), state, ut);
        enc.sendTypeId(state, ut);
        if (enc.err != default!) {
            return enc.err;
        }
        // Encode the object.
        Ꮡenc.encode((~state).b, value, ut);
        if (enc.err == default!) {
            enc.writeMessage(enc.writer(), (~state).b);
        }
        enc.freeEncoderState(state);
        return enc.err;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡenc.DerefOrNull().mutex.Unlock(); ᒐ.Run(); }
}

} // end gob_package
