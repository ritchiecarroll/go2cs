// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package json implements encoding and decoding of JSON as defined in
// RFC 7159. The mapping between JSON and Go values is described
// in the documentation for the Marshal and Unmarshal functions.
//
// See "JSON and Go" for an introduction to this package:
// https://golang.org/doc/articles/json_and_go.html
namespace go.encoding;

using bytes = bytes_package;
using cmp = cmp_package;
using encoding = encoding_package;
using base64 = go.encoding.base64_package;
using fmt = fmt_package;
using math = math_package;
using reflect = reflect_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using unicode = unicode_package;
using utf8 = go.unicode.utf8_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for linkname
using @unsafe = unsafe_package;
using go.encoding;
using go.unicode;

partial class json_package {

// Marshal returns the JSON encoding of v.
//
// Marshal traverses the value v recursively.
// If an encountered value implements [Marshaler]
// and is not a nil pointer, Marshal calls [Marshaler.MarshalJSON]
// to produce JSON. If no [Marshaler.MarshalJSON] method is present but the
// value implements [encoding.TextMarshaler] instead, Marshal calls
// [encoding.TextMarshaler.MarshalText] and encodes the result as a JSON string.
// The nil pointer exception is not strictly necessary
// but mimics a similar, necessary exception in the behavior of
// [Unmarshaler.UnmarshalJSON].
//
// Otherwise, Marshal uses the following type-dependent default encodings:
//
// Boolean values encode as JSON booleans.
//
// Floating point, integer, and [Number] values encode as JSON numbers.
// NaN and +/-Inf values will return an [UnsupportedValueError].
//
// String values encode as JSON strings coerced to valid UTF-8,
// replacing invalid bytes with the Unicode replacement rune.
// So that the JSON will be safe to embed inside HTML <script> tags,
// the string is encoded using [HTMLEscape],
// which replaces "<", ">", "&", U+2028, and U+2029 are escaped
// to "\u003c","\u003e", "\u0026", "\u2028", and "\u2029".
// This replacement can be disabled when using an [Encoder],
// by calling [Encoder.SetEscapeHTML](false).
//
// Array and slice values encode as JSON arrays, except that
// []byte encodes as a base64-encoded string, and a nil slice
// encodes as the null JSON value.
//
// Struct values encode as JSON objects.
// Each exported struct field becomes a member of the object, using the
// field name as the object key, unless the field is omitted for one of the
// reasons given below.
//
// The encoding of each struct field can be customized by the format string
// stored under the "json" key in the struct field's tag.
// The format string gives the name of the field, possibly followed by a
// comma-separated list of options. The name may be empty in order to
// specify options without overriding the default field name.
//
// The "omitempty" option specifies that the field should be omitted
// from the encoding if the field has an empty value, defined as
// false, 0, a nil pointer, a nil interface value, and any array,
// slice, map, or string of length zero.
//
// As a special case, if the field tag is "-", the field is always omitted.
// Note that a field with name "-" can still be generated using the tag "-,".
//
// Examples of struct field tags and their meanings:
//
//	// Field appears in JSON as key "myName".
//	Field int `json:"myName"`
//
//	// Field appears in JSON as key "myName" and
//	// the field is omitted from the object if its value is empty,
//	// as defined above.
//	Field int `json:"myName,omitempty"`
//
//	// Field appears in JSON as key "Field" (the default), but
//	// the field is skipped if empty.
//	// Note the leading comma.
//	Field int `json:",omitempty"`
//
//	// Field is ignored by this package.
//	Field int `json:"-"`
//
//	// Field appears in JSON as key "-".
//	Field int `json:"-,"`
//
// The "omitzero" option specifies that the field should be omitted
// from the encoding if the field has a zero value, according to rules:
//
// 1) If the field type has an "IsZero() bool" method, that will be used to
// determine whether the value is zero.
//
// 2) Otherwise, the value is zero if it is the zero value for its type.
//
// If both "omitempty" and "omitzero" are specified, the field will be omitted
// if the value is either empty or zero (or both).
//
// The "string" option signals that a field is stored as JSON inside a
// JSON-encoded string. It applies only to fields of string, floating point,
// integer, or boolean types. This extra level of encoding is sometimes used
// when communicating with JavaScript programs:
//
//	Int64String int64 `json:",string"`
//
// The key name will be used if it's a non-empty string consisting of
// only Unicode letters, digits, and ASCII punctuation except quotation
// marks, backslash, and comma.
//
// Embedded struct fields are usually marshaled as if their inner exported fields
// were fields in the outer struct, subject to the usual Go visibility rules amended
// as described in the next paragraph.
// An anonymous struct field with a name given in its JSON tag is treated as
// having that name, rather than being anonymous.
// An anonymous struct field of interface type is treated the same as having
// that type as its name, rather than being anonymous.
//
// The Go visibility rules for struct fields are amended for JSON when
// deciding which field to marshal or unmarshal. If there are
// multiple fields at the same level, and that level is the least
// nested (and would therefore be the nesting level selected by the
// usual Go rules), the following extra rules apply:
//
// 1) Of those fields, if any are JSON-tagged, only tagged fields are considered,
// even if there are multiple untagged fields that would otherwise conflict.
//
// 2) If there is exactly one field (tagged or not according to the first rule), that is selected.
//
// 3) Otherwise there are multiple fields, and all are ignored; no error occurs.
//
// Handling of anonymous struct fields is new in Go 1.1.
// Prior to Go 1.1, anonymous struct fields were ignored. To force ignoring of
// an anonymous struct field in both current and earlier versions, give the field
// a JSON tag of "-".
//
// Map values encode as JSON objects. The map's key type must either be a
// string, an integer type, or implement [encoding.TextMarshaler]. The map keys
// are sorted and used as JSON object keys by applying the following rules,
// subject to the UTF-8 coercion described for string values above:
//   - keys of any string type are used directly
//   - keys that implement [encoding.TextMarshaler] are marshaled
//   - integer keys are converted to strings
//
// Pointer values encode as the value pointed to.
// A nil pointer encodes as the null JSON value.
//
// Interface values encode as the value contained in the interface.
// A nil interface value encodes as the null JSON value.
//
// Channel, complex, and function values cannot be encoded in JSON.
// Attempting to encode such a value causes Marshal to return
// an [UnsupportedTypeError].
//
// JSON cannot represent cyclic data structures and Marshal does not
// handle them. Passing cyclic structures to Marshal will result in
// an error.
public static (slice<byte>, error) Marshal(any v) {
    GoFrame ᒐ = default;
    try {
        var e = newEncodeState();
        defer(ᏑencodeStatePool.Put, e.OrTypedNil(), ref ᒐ);
        var err = e.marshal(v, new encOpts(escapeHTML: true));
        if (err != default!) {
            return (default!, err);
        }
        var buf = appendꓸꓸꓸ(slice<byte>(default!), e.of(encodeState.ᏑBuffer).Bytes());
        return (buf, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// MarshalIndent is like [Marshal] but applies [Indent] to format the output.
// Each JSON element in the output will begin on a new line beginning with prefix
// followed by one or more copies of indent according to the indentation nesting.
public static (slice<byte>, error) MarshalIndent(any v, @string prefix, @string indent) {
    var (b, err) = Marshal(v);
    if (err != default!) {
        return (default!, err);
    }
    var b2 = new slice<byte>(0, (nint)indentGrowthFactor * len(b));
    (b2, err) = appendIndent(b2, b, prefix, indent);
    if (err != default!) {
        return (default!, err);
    }
    return (b2, default!);
}

// Marshaler is the interface implemented by types that
// can marshal themselves into valid JSON.
partial interface Marshaler {
    (slice<byte>, error) MarshalJSON();
}

// An UnsupportedTypeError is returned by [Marshal] when attempting
// to encode an unsupported value type.
partial struct UnsupportedTypeError {
    public reflectꓸType Type;
}

public static @string Error(this ref UnsupportedTypeError e) {
    return "json: unsupported type: "u8 + e.Type.String();
}

// An UnsupportedValueError is returned by [Marshal] when attempting
// to encode an unsupported value.
partial struct UnsupportedValueError {
    public reflectꓸValue Value;
    public @string Str;
}

public static @string Error(this ref UnsupportedValueError e) {
    return "json: unsupported value: "u8 + e.Str;
}

// Before Go 1.2, an InvalidUTF8Error was returned by [Marshal] when
// attempting to encode a string value with invalid UTF-8 sequences.
// As of Go 1.2, [Marshal] instead coerces the string to valid UTF-8 by
// replacing invalid bytes with the Unicode replacement rune U+FFFD.
//
// Deprecated: No longer used; kept for compatibility.
partial struct InvalidUTF8Error {
    public @string S; // the whole string value that caused the error
}

public static @string Error(this ref InvalidUTF8Error e) {
    return "json: invalid UTF-8 in string: "u8 + strconv.Quote(e.S);
}

// A MarshalerError represents an error from calling a
// [Marshaler.MarshalJSON] or [encoding.TextMarshaler.MarshalText] method.
partial struct MarshalerError {
    public reflectꓸType Type;
    public error Err;
    internal @string sourceFunc;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string marshalJSONˢ = "MarshalJSON"u8;

public static @string Error(this ref MarshalerError e) {
    @string srcFunc = e.sourceFunc;
    if (srcFunc == ""u8) {
        srcFunc = marshalJSONˢ;
    }
    return "json: error calling "u8 + srcFunc + " for type "u8 + e.Type.String() + ": "u8 + e.Err.Error();
}

// Unwrap returns the underlying error.
public static error Unwrap(this ref MarshalerError e) {
    return e.Err;
}

internal static readonly @string hex = "0123456789abcdef"u8;

// An encodeState encodes JSON into a bytes.Buffer.
partial struct encodeState {
    public partial ref bytes_package.Buffer Buffer { get; } // accumulated output
    // Keep track of what pointers we've seen in the current recursive call
    // path, to avoid cycles that could lead to a stack overflow. Only do
    // the relatively expensive map operations if ptrLevel is larger than
    // startDetectingCyclesAfter, so that we skip the work if we're within a
    // reasonable amount of nested pointers deep.
    internal nuint ptrLevel;
    internal map<any, EmptyStruct> ptrSeen;
}

internal static UntypedInt startDetectingCyclesAfter => 1000;

internal static ж<sync.Pool> ᏑencodeStatePool = new StandardBox<sync.Pool>(default(sync.Pool));
internal static ref sync.Pool encodeStatePool => ref ᏑencodeStatePool.Value;

internal static ж<encodeState> newEncodeState() {
    {
        var v = ᏑencodeStatePool.Get(); if (v != default!) {
            var e = v._<ж<encodeState>>();
            e.of(encodeState.ᏑBuffer).Reset();
            if (len((~e).ptrSeen) > 0) {
                throw panic("ptrEncoder.encode should have emptied ptrSeen via defers");
            }
            e.Value.ptrLevel = 0;
            return e;
        }
    }
    return Ꮡ(new encodeState(ptrSeen: new map<any, EmptyStruct>()));
}

// jsonError is an error wrapper type for internal use only.
// Panics with errors are wrapped in jsonError so that the top-level recover
// can distinguish intentional panics from this package.
partial struct jsonError {
    /*embed*/ internal error error;
}

internal static error /*err*/ marshal(this ж<encodeState> Ꮡe, any v, encOpts opts) {
    error err = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    {
                        ref var je = ref heap<jsonError>(out var Ꮡje);
                        (je, var ok) = r._<jsonError>(ᐧ); if (ok){
                            err = je.error;
                        } else {
                            throw panic(r);
                        }
                    }
                }
            }
        }, ref ᒐ);
        Ꮡe.reflectValue(reflect.ValueOf(v), opts);
        err = default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return err;
}

// error aborts the encoding by panicking with err wrapped in jsonError.
internal static void error(this ref encodeState e, error err) {
    throw panic(new jsonError(err));
}

internal static bool isEmptyValue(reflectꓸValue v) {
    var exprᴛ1 = v.Kind();
    if (exprᴛ1 == reflect.Array || exprᴛ1 == reflect.Map || exprᴛ1 == reflect.ΔSlice || exprᴛ1 == reflect.ΔString) {
        return v.Len() == 0;
    }
    if (exprᴛ1 == reflect.ΔBool || exprᴛ1 == reflect.ΔInt || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64 || exprᴛ1 == reflect.ΔUint || exprᴛ1 == reflect.Uint8 || exprᴛ1 == reflect.Uint16 || exprᴛ1 == reflect.Uint32 || exprᴛ1 == reflect.Uint64 || exprᴛ1 == reflect.Uintptr || exprᴛ1 == reflect.Float32 || exprᴛ1 == reflect.Float64 || exprᴛ1 == reflect.ΔInterface || exprᴛ1 == reflect.ΔPointer) {
        return v.IsZero();
    }

    return false;
}

internal static void reflectValue(this ж<encodeState> Ꮡe, reflectꓸValue v, encOpts opts) {
    valueEncoder(v)(Ꮡe, v, opts);
}

partial struct encOpts {
    // quoted causes primitive fields to be encoded inside JSON strings.
    internal bool quoted;
    // escapeHTML causes '<', '>', and '&' to be escaped in JSON strings.
    internal bool escapeHTML;
}

// type encoderFunc is a methodless func type — rendered inline as its base delegate

internal static ж<sync.Map> ᏑencoderCache = new StandardBox<sync.Map>(default(sync.Map));
internal static ref sync.Map encoderCache => ref ᏑencoderCache.Value; // map[reflect.Type]encoderFunc

internal static Action<ж<encodeState>, reflectꓸValue, encOpts> valueEncoder(reflectꓸValue v) {
    if (!v.IsValid()) {
        return invalidValueEncoder;
    }
    return typeEncoder(v.Type());
}

internal static Action<ж<encodeState>, reflectꓸValue, encOpts> typeEncoder(reflectꓸType t) {
    {
        var (fiΔ1, ok) = ᏑencoderCache.Load(t); if (ok) {
            return fiΔ1._<Action<ж<encodeState>, reflectꓸValue, encOpts>>();
        }
    }
    // To deal with recursive types, populate the map with an
    // indirect func before we build it. This type waits on the
    // real func (f) to be ready and then calls it. This indirect
    // func is only used for recursive types.
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    
    ref var f = ref heap<Action<ж<encodeState>, reflectꓸValue, encOpts>>(out var Ꮡf);
    Ꮡwg.Add(1);
    var (fi, loaded) = ᏑencoderCache.LoadOrStore(t, new Action<ж<encodeState>, reflectꓸValue, encOpts>((ж<encodeState> e, reflectꓸValue v, encOpts opts) => {
        Ꮡwg.Wait();
        Ꮡf.ValueSlot(e, v, opts);
    }));
    if (loaded) {
        return fi._<Action<ж<encodeState>, reflectꓸValue, encOpts>>();
    }
    // Compute the real encoder and replace the indirect func with it.
    f = newTypeEncoder(t, true);
    Ꮡwg.Done();
    ᏑencoderCache.Store(t, (f).OrTypedNilFunc());
    return f;
}

internal static reflectꓸType marshalerType = reflect.TypeFor<Marshaler>();
internal static reflectꓸType textMarshalerType = reflect.TypeFor<encoding.TextMarshaler>();

// newTypeEncoder constructs an encoderFunc for a type.
// The returned encoder only checks CanAddr when allowAddr is true.
internal static Action<ж<encodeState>, reflectꓸValue, encOpts> newTypeEncoder(reflectꓸType t, bool allowAddr) {
    // If we have a non-pointer value whose type implements
    // Marshaler with a value receiver, then we're better off taking
    // the address of the value - otherwise we end up with an
    // allocation as we cast the value to an interface.
    if (t.Kind() != reflect.ΔPointer && allowAddr && reflect.PointerTo(t).Implements(marshalerType)) {
        return newCondAddrEncoder(new Action<ж<encodeState>, reflectꓸValue, encOpts>(addrMarshalerEncoder), newTypeEncoder(t, false));
    }
    if (t.Implements(marshalerType)) {
        return marshalerEncoder;
    }
    if (t.Kind() != reflect.ΔPointer && allowAddr && reflect.PointerTo(t).Implements(textMarshalerType)) {
        return newCondAddrEncoder(new Action<ж<encodeState>, reflectꓸValue, encOpts>(addrTextMarshalerEncoder), newTypeEncoder(t, false));
    }
    if (t.Implements(textMarshalerType)) {
        return textMarshalerEncoder;
    }
    var exprᴛ1 = t.Kind();
    if (exprᴛ1 == reflect.ΔBool) {
        return boolEncoder;
    }
    if (exprᴛ1 == reflect.ΔInt || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64) {
        return intEncoder;
    }
    if (exprᴛ1 == reflect.ΔUint || exprᴛ1 == reflect.Uint8 || exprᴛ1 == reflect.Uint16 || exprᴛ1 == reflect.Uint32 || exprᴛ1 == reflect.Uint64 || exprᴛ1 == reflect.Uintptr) {
        return uintEncoder;
    }
    if (exprᴛ1 == reflect.Float32) {
        return float32Encoder;
    }
    if (exprᴛ1 == reflect.Float64) {
        return float64Encoder;
    }
    if (exprᴛ1 == reflect.ΔString) {
        return stringEncoder;
    }
    if (exprᴛ1 == reflect.ΔInterface) {
        return interfaceEncoder;
    }
    if (exprᴛ1 == reflect.Struct) {
        return newStructEncoder(t);
    }
    if (exprᴛ1 == reflect.Map) {
        return newMapEncoder(t);
    }
    if (exprᴛ1 == reflect.ΔSlice) {
        return newSliceEncoder(t);
    }
    if (exprᴛ1 == reflect.Array) {
        return newArrayEncoder(t);
    }
    if (exprᴛ1 == reflect.ΔPointer) {
        return newPtrEncoder(t);
    }
    { /* default: */
        return unsupportedTypeEncoder;
    }

}

internal static void invalidValueEncoder(ж<encodeState> Ꮡe, reflectꓸValue v, encOpts _) {
    Ꮡe.of(encodeState.ᏑBuffer).WriteString(nullˢ);
}

internal static void marshalerEncoder(ж<encodeState> Ꮡe, reflectꓸValue v, encOpts opts) {
    ref var e = ref Ꮡe.DerefOrNull();

    if (v.Kind() == reflect.ΔPointer && v.IsNil()) {
        Ꮡe.of(encodeState.ᏑBuffer).WriteString(nullˢ);
        return;
    }
    var (m, ok) = v.Interface()._<Marshaler>(ᐧ);
    if (!ok) {
        Ꮡe.of(encodeState.ᏑBuffer).WriteString(nullˢ);
        return;
    }
    var (b, err) = m.MarshalJSON();
    if (err == default!) {
        Ꮡe.of(encodeState.ᏑBuffer).Grow(len(b));
        var @out = Ꮡe.of(encodeState.ᏑBuffer).AvailableBuffer();
        (@out, err) = appendCompact(@out, b, opts.escapeHTML);
        e.Buffer.Write(@out);
    }
    if (err != default!) {
        e.error(new MarshalerErrorжerror(Ꮡ(new MarshalerError(v.Type(), err, "MarshalJSON"u8))));
    }
}

internal static void addrMarshalerEncoder(ж<encodeState> Ꮡe, reflectꓸValue v, encOpts opts) {
    ref var e = ref Ꮡe.DerefOrNull();

    var va = v.Addr();
    if (va.IsNil()) {
        Ꮡe.of(encodeState.ᏑBuffer).WriteString(nullˢ);
        return;
    }
    var m = va.Interface()._<Marshaler>();
    var (b, err) = m.MarshalJSON();
    if (err == default!) {
        Ꮡe.of(encodeState.ᏑBuffer).Grow(len(b));
        var @out = Ꮡe.of(encodeState.ᏑBuffer).AvailableBuffer();
        (@out, err) = appendCompact(@out, b, opts.escapeHTML);
        e.Buffer.Write(@out);
    }
    if (err != default!) {
        e.error(new MarshalerErrorжerror(Ꮡ(new MarshalerError(v.Type(), err, "MarshalJSON"u8))));
    }
}

internal static void textMarshalerEncoder(ж<encodeState> Ꮡe, reflectꓸValue v, encOpts opts) {
    ref var e = ref Ꮡe.DerefOrNull();

    if (v.Kind() == reflect.ΔPointer && v.IsNil()) {
        Ꮡe.of(encodeState.ᏑBuffer).WriteString(nullˢ);
        return;
    }
    var (m, ok) = v.Interface()._<encoding.TextMarshaler>(ᐧ);
    if (!ok) {
        Ꮡe.of(encodeState.ᏑBuffer).WriteString(nullˢ);
        return;
    }
    var (b, err) = m.MarshalText();
    if (err != default!) {
        e.error(new MarshalerErrorжerror(Ꮡ(new MarshalerError(v.Type(), err, "MarshalText"u8))));
    }
    Ꮡe.of(encodeState.ᏑBuffer).Write(appendString(Ꮡe.of(encodeState.ᏑBuffer).AvailableBuffer(), b, opts.escapeHTML));
}

internal static void addrTextMarshalerEncoder(ж<encodeState> Ꮡe, reflectꓸValue v, encOpts opts) {
    ref var e = ref Ꮡe.DerefOrNull();

    var va = v.Addr();
    if (va.IsNil()) {
        Ꮡe.of(encodeState.ᏑBuffer).WriteString(nullˢ);
        return;
    }
    var m = va.Interface()._<encoding.TextMarshaler>();
    var (b, err) = m.MarshalText();
    if (err != default!) {
        e.error(new MarshalerErrorжerror(Ꮡ(new MarshalerError(v.Type(), err, "MarshalText"u8))));
    }
    Ꮡe.of(encodeState.ᏑBuffer).Write(appendString(Ꮡe.of(encodeState.ᏑBuffer).AvailableBuffer(), b, opts.escapeHTML));
}

internal static void boolEncoder(ж<encodeState> Ꮡe, reflectꓸValue v, encOpts opts) {
    var b = Ꮡe.of(encodeState.ᏑBuffer).AvailableBuffer();
    b = mayAppendQuote(b, opts.quoted);
    b = strconv.AppendBool(b, v.Bool());
    b = mayAppendQuote(b, opts.quoted);
    Ꮡe.of(encodeState.ᏑBuffer).Write(b);
}

internal static void intEncoder(ж<encodeState> Ꮡe, reflectꓸValue v, encOpts opts) {
    var b = Ꮡe.of(encodeState.ᏑBuffer).AvailableBuffer();
    b = mayAppendQuote(b, opts.quoted);
    b = strconv.AppendInt(b, v.Int(), 10);
    b = mayAppendQuote(b, opts.quoted);
    Ꮡe.of(encodeState.ᏑBuffer).Write(b);
}

internal static void uintEncoder(ж<encodeState> Ꮡe, reflectꓸValue v, encOpts opts) {
    var b = Ꮡe.of(encodeState.ᏑBuffer).AvailableBuffer();
    b = mayAppendQuote(b, opts.quoted);
    b = strconv.AppendUint(b, v.Uint(), 10);
    b = mayAppendQuote(b, opts.quoted);
    Ꮡe.of(encodeState.ᏑBuffer).Write(b);
}

partial struct floatEncoder /*num:nint*/;

internal static void encode(this floatEncoder bits, ж<encodeState> Ꮡe, reflectꓸValue v, encOpts opts) {
    ref var e = ref Ꮡe.DerefOrNull();

    var f = v.Float();
    if (math.IsInf(f, 0) || math.IsNaN(f)) {
        e.error(new UnsupportedValueErrorжerror(Ꮡ(new UnsupportedValueError(v, strconv.FormatFloat(f, (rune)'g', -1, (nint)bits)))));
    }
    // Convert as if by ES6 number to string conversion.
    // This matches most other JSON generators.
    // See golang.org/issue/6384 and golang.org/issue/14135.
    // Like fmt %g, but the exponent cutoffs are different
    // and exponents themselves are not padded to two digits.
    var b = Ꮡe.of(encodeState.ᏑBuffer).AvailableBuffer();
    b = mayAppendQuote(b, opts.quoted);
    var abs = math.Abs(f);
    var fmt = (byte)(rune)'f';
    // Note: Must use float32 comparisons for underlying float32 value to get precise cutoffs right.
    if (abs != 0D) {
        if (bits == 64 && (abs < 1e-6D || abs >= 1e21D) || bits == 32 && ((float32)abs < 1e-6F || (float32)abs >= 1e21F)) {
            fmt = (rune)'e';
        }
    }
    b = strconv.AppendFloat(b, f, fmt, -1, (nint)bits);
    if (fmt == (rune)'e') {
        // clean up e-09 to e-9
        nint n = len(b);
        if (n >= 4 && b[n - 4] == (rune)'e' && b[n - 3] == (rune)'-' && b[n - 2] == (rune)'0') {
            b[n - 2] = b[n - 1];
            b = b.slice(0, n - 1);
        }
    }
    b = mayAppendQuote(b, opts.quoted);
    Ꮡe.of(encodeState.ᏑBuffer).Write(b);
}

internal static Action<ж<encodeState>, reflectꓸValue, encOpts> float32Encoder = (ж<encodeState> p1, reflectꓸValue p2, encOpts p3) => (((floatEncoder)32)).encode(p1, p2, p3);
internal static Action<ж<encodeState>, reflectꓸValue, encOpts> float64Encoder = (ж<encodeState> p1, reflectꓸValue p2, encOpts p3) => (((floatEncoder)64)).encode(p1, p2, p3);

internal static void stringEncoder(ж<encodeState> Ꮡe, reflectꓸValue v, encOpts opts) {
    ref var e = ref Ꮡe.DerefOrNull();

    if (AreEqual(v.Type(), numberType)) {
        @string numStr = v.String();
        // In Go1.5 the empty string encodes to "0", while this is not a valid number literal
        // we keep compatibility so check validity after this.
        if (numStr == ""u8) {
            numStr = "0"u8; // Number's zero-val
        }
        if (!isValidNumber(numStr)) {
            e.error(fmt.Errorf("json: invalid number literal %q"u8, numStr));
        }
        var b = Ꮡe.of(encodeState.ᏑBuffer).AvailableBuffer();
        b = mayAppendQuote(b, opts.quoted);
        b = append(b, numStr.ꓸꓸꓸ);
        b = mayAppendQuote(b, opts.quoted);
        Ꮡe.of(encodeState.ᏑBuffer).Write(b);
        return;
    }
    if (opts.quoted){
        var b = appendString(default!, v.String(), opts.escapeHTML);
        Ꮡe.of(encodeState.ᏑBuffer).Write(appendString(Ꮡe.of(encodeState.ᏑBuffer).AvailableBuffer(), b, false)); // no need to escape again since it is already escaped
    } else {
        Ꮡe.of(encodeState.ᏑBuffer).Write(appendString(Ꮡe.of(encodeState.ᏑBuffer).AvailableBuffer(), v.String(), opts.escapeHTML));
    }
}

// isValidNumber reports whether s is a valid JSON number literal.
//
// isValidNumber should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname isValidNumber
internal static bool isValidNumber(@string s) {
    // This function implements the JSON numbers grammar.
    // See https://tools.ietf.org/html/rfc7159#section-6
    // and https://www.json.org/img/number.png
    if (s == ""u8) {
        return false;
    }
    // Optional -
    if (s[0] == (rune)'-') {
        s = s[1..];
        if (s == ""u8) {
            return false;
        }
    }
    // Digits
    switch (ᐧ) {
    default: {
        return false;
    }
    case {} when s[0] is (rune)'0': {
        s = s[1..];
        break;
    }
    case {} when (rune)'1' <= s[0] && s[0] <= (rune)'9': {
        s = s[1..];
        while (len(s) > 0 && (rune)'0' <= s[0] && s[0] <= (rune)'9') {
            s = s[1..];
        }
        break;
    }}

    // . followed by 1 or more digits.
    if (len(s) >= 2 && s[0] == (rune)'.' && (rune)'0' <= s[1] && s[1] <= (rune)'9') {
        s = s[2..];
        while (len(s) > 0 && (rune)'0' <= s[0] && s[0] <= (rune)'9') {
            s = s[1..];
        }
    }
    // e or E followed by an optional - or + and
    // 1 or more digits.
    if (len(s) >= 2 && (s[0] == (rune)'e' || s[0] == (rune)'E')) {
        s = s[1..];
        if (s[0] == (rune)'+' || s[0] == (rune)'-') {
            s = s[1..];
            if (s == ""u8) {
                return false;
            }
        }
        while (len(s) > 0 && (rune)'0' <= s[0] && s[0] <= (rune)'9') {
            s = s[1..];
        }
    }
    // Make sure we are at the end.
    return s == ""u8;
}

internal static void interfaceEncoder(ж<encodeState> Ꮡe, reflectꓸValue v, encOpts opts) {
    if (v.IsNil()) {
        Ꮡe.of(encodeState.ᏑBuffer).WriteString(nullˢ);
        return;
    }
    Ꮡe.reflectValue(v.Elem(), opts);
}

internal static void unsupportedTypeEncoder(ж<encodeState> Ꮡe, reflectꓸValue v, encOpts _) {
    ref var e = ref Ꮡe.DerefOrNull();

    e.error(new UnsupportedTypeErrorжerror(Ꮡ(new UnsupportedTypeError(v.Type()))));
}

partial struct structEncoder {
    internal structFields fields;
}

partial struct structFields {
    internal slice<field> list;
    internal map<@string, ж<field>> byExactName;
    internal map<@string, ж<field>> byFoldedName;
}

internal static void encode(this structEncoder se, ж<encodeState> Ꮡe, reflectꓸValue v, encOpts opts) {
    var next = (byte)(rune)'{';
FieldLoop:
    foreach (var (i, _) in se.fields.list) {
        var f = Ꮡ(se.fields.list, i);
        // Find the nested struct field by following f.index.
        var fv = v;
        foreach (var (_, iΔ1) in (~f).index) {
            if (fv.Kind() == reflect.ΔPointer) {
                if (fv.IsNil()) {
                    goto continue_FieldLoop;
                }
                fv = fv.Elem();
            }
            fv = fv.Field(iΔ1);
        }
        if (((~f).omitEmpty && isEmptyValue(fv)) || ((~f).omitZero && ((~f).isZero == default! && fv.IsZero() || ((~f).isZero != default! && (~f).isZero(fv))))) {
            continue;
        }
        Ꮡe.of(encodeState.ᏑBuffer).WriteByte(next);
        next = (rune)',';
        if (opts.escapeHTML){
            Ꮡe.of(encodeState.ᏑBuffer).WriteString((~f).nameEscHTML);
        } else {
            Ꮡe.of(encodeState.ᏑBuffer).WriteString((~f).nameNonEsc);
        }
        opts.quoted = f.Value.quoted;
        (~f).encoder(Ꮡe, fv, opts);
continue_FieldLoop:;
    }
break_FieldLoop:;
    if (next == (rune)'{'){
        Ꮡe.of(encodeState.ᏑBuffer).WriteString("{}"u8);
    } else {
        Ꮡe.of(encodeState.ᏑBuffer).WriteByte((rune)'}');
    }
}

internal static Action<ж<encodeState>, reflectꓸValue, encOpts> newStructEncoder(reflectꓸType t) {
    var se = new structEncoder(fields: cachedTypeFields(t));
    var seʗ1 = se;
    return (ж<encodeState> p1, reflectꓸValue p2, encOpts p3) => seʗ1.encode(p1, p2, p3);
}

partial struct mapEncoder {
    internal Action<ж<encodeState>, reflectꓸValue, encOpts> elemEnc;
}

internal static void encode(this mapEncoder me, ж<encodeState> Ꮡe, reflectꓸValue v, encOpts opts) {
    GoFrame ᒐ = default;
    try {
        ref var e = ref Ꮡe.DerefOrNull();

        if (v.IsNil()) {
            Ꮡe.of(encodeState.ᏑBuffer).WriteString(nullˢ);
            return;
        }
        {
            e.ptrLevel++; if (e.ptrLevel > startDetectingCyclesAfter) {
                // We're a large number of nested ptrEncoder.encode calls deep;
                // start checking if we've run into a pointer cycle.
                @unsafe.Pointer ptr = (uintptr)v.UnsafePointer();
                {
                    var (_, ok) = e.ptrSeen[@unsafe.Pointer.OrTypedNil(ptr), ꟷ]; if (ok) {
                        e.error(new UnsupportedValueErrorжerror(Ꮡ(new UnsupportedValueError(v, fmt.Sprintf("encountered a cycle via %s"u8, v.Type())))));
                    }
                }
                e.ptrSeen[@unsafe.Pointer.OrTypedNil(ptr)] = new EmptyStruct();
                defer((ᴛ1, ᴛ2) => delete(ᴛ1, ᴛ2), Ꮡe.Value.ptrSeen, @unsafe.Pointer.OrTypedNil(ptr), ref ᒐ);
            }
        }
        Ꮡe.of(encodeState.ᏑBuffer).WriteByte((rune)'{');
        // Extract and sort the keys.
        slice<reflectWithString> sv = new slice<reflectWithString>(v.Len(), () => new());
        
        ж<reflect.MapIter> mi = v.MapRange();
        
        error err = default!;
        for (nint i = 0; mi.Next(); i++) {
            {
                (sv[i].ks, err) = resolveKeyName(mi.Key()); if (err != default!) {
                    e.error(fmt.Errorf("json: encoding error for type %q: %q"u8, v.Type().String(), err.Error()));
                }
            }
            sv[i].v = mi.Value();
        }
        slices.SortFunc(sv, (reflectWithString i, reflectWithString j) => strings.Compare(i.ks, j.ks));
        foreach (var (i, kv) in sv) {
            if (i > 0) {
                Ꮡe.of(encodeState.ᏑBuffer).WriteByte((rune)',');
            }
            Ꮡe.of(encodeState.ᏑBuffer).Write(appendString(Ꮡe.of(encodeState.ᏑBuffer).AvailableBuffer(), kv.ks, opts.escapeHTML));
            Ꮡe.of(encodeState.ᏑBuffer).WriteByte((rune)':');
            me.elemEnc(Ꮡe, kv.v, opts);
        }
        Ꮡe.of(encodeState.ᏑBuffer).WriteByte((rune)'}');
        e.ptrLevel--;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static Action<ж<encodeState>, reflectꓸValue, encOpts> newMapEncoder(reflectꓸType t) {
    var exprᴛ1 = t.Key().Kind();
    if (exprᴛ1 == reflect.ΔString || exprᴛ1 == reflect.ΔInt || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64 || exprᴛ1 == reflect.ΔUint || exprᴛ1 == reflect.Uint8 || exprᴛ1 == reflect.Uint16 || exprᴛ1 == reflect.Uint32 || exprᴛ1 == reflect.Uint64 || exprᴛ1 == reflect.Uintptr) {
    }
    else { /* default: */
        if (!t.Key().Implements(textMarshalerType)) {
            return unsupportedTypeEncoder;
        }
    }

    var me = new mapEncoder(typeEncoder(t.Elem()));
    var meʗ1 = me;
    return (ж<encodeState> p1, reflectꓸValue p2, encOpts p3) => meʗ1.encode(p1, p2, p3);
}

internal static void encodeByteSlice(ж<encodeState> Ꮡe, reflectꓸValue v, encOpts _) {
    if (v.IsNil()) {
        Ꮡe.of(encodeState.ᏑBuffer).WriteString(nullˢ);
        return;
    }
    var s = v.Bytes();
    var b = Ꮡe.of(encodeState.ᏑBuffer).AvailableBuffer();
    b = append(b, (byte)((rune)'"'));
    b = base64.StdEncoding.AppendEncode(b, s);
    b = append(b, (byte)((rune)'"'));
    Ꮡe.of(encodeState.ᏑBuffer).Write(b);
}

// sliceEncoder just wraps an arrayEncoder, checking to make sure the value isn't nil.
partial struct sliceEncoder {
    internal Action<ж<encodeState>, reflectꓸValue, encOpts> arrayEnc;
}

internal partial struct encode_ptr /*dyn*/ {
    internal any ptr; // always an unsafe.Pointer, but avoids a dependency on package unsafe
    internal nint len;
}

internal static void encode(this sliceEncoder se, ж<encodeState> Ꮡe, reflectꓸValue v, encOpts opts) {
    GoFrame ᒐ = default;
    try {
        ref var e = ref Ꮡe.DerefOrNull();

        if (v.IsNil()) {
            Ꮡe.of(encodeState.ᏑBuffer).WriteString(nullˢ);
            return;
        }
        {
            e.ptrLevel++; if (e.ptrLevel > startDetectingCyclesAfter) {
                // We're a large number of nested ptrEncoder.encode calls deep;
                // start checking if we've run into a pointer cycle.
                // Here we use a struct to memorize the pointer to the first element of the slice
                // and its length.
                ref var ptr = ref heap<encode_ptr>(out var Ꮡptr);
                ptr = new encode_ptr(@unsafe.Pointer.OrTypedNil((uintptr)v.UnsafePointer()), v.Len());
                {
                    var (_, ok) = e.ptrSeen[ptr, ꟷ]; if (ok) {
                        e.error(new UnsupportedValueErrorжerror(Ꮡ(new UnsupportedValueError(v, fmt.Sprintf("encountered a cycle via %s"u8, v.Type())))));
                    }
                }
                e.ptrSeen[ptr] = new EmptyStruct();
                defer((ᴛ1, ᴛ2) => delete(ᴛ1, ᴛ2), Ꮡe.Value.ptrSeen, ptr, ref ᒐ);
            }
        }
        se.arrayEnc(Ꮡe, v, opts);
        e.ptrLevel--;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static Action<ж<encodeState>, reflectꓸValue, encOpts> newSliceEncoder(reflectꓸType t) {
    // Byte slices get special treatment; arrays don't.
    if (t.Elem().Kind() == reflect.Uint8) {
        var p = reflect.PointerTo(t.Elem());
        if (!p.Implements(marshalerType) && !p.Implements(textMarshalerType)) {
            return encodeByteSlice;
        }
    }
    var enc = new sliceEncoder(newArrayEncoder(t));
    var encʗ1 = enc;
    return (ж<encodeState> p1, reflectꓸValue p2, encOpts p3) => encʗ1.encode(p1, p2, p3);
}

partial struct arrayEncoder {
    internal Action<ж<encodeState>, reflectꓸValue, encOpts> elemEnc;
}

internal static void encode(this arrayEncoder ae, ж<encodeState> Ꮡe, reflectꓸValue v, encOpts opts) {
    Ꮡe.of(encodeState.ᏑBuffer).WriteByte((rune)'[');
    nint n = v.Len();
    for (nint i = 0; i < n; i++) {
        if (i > 0) {
            Ꮡe.of(encodeState.ᏑBuffer).WriteByte((rune)',');
        }
        ae.elemEnc(Ꮡe, v.Index(i), opts);
    }
    Ꮡe.of(encodeState.ᏑBuffer).WriteByte((rune)']');
}

internal static Action<ж<encodeState>, reflectꓸValue, encOpts> newArrayEncoder(reflectꓸType t) {
    var enc = new arrayEncoder(typeEncoder(t.Elem()));
    var encʗ1 = enc;
    return (ж<encodeState> p1, reflectꓸValue p2, encOpts p3) => encʗ1.encode(p1, p2, p3);
}

partial struct ptrEncoder {
    internal Action<ж<encodeState>, reflectꓸValue, encOpts> elemEnc;
}

internal static void encode(this ptrEncoder pe, ж<encodeState> Ꮡe, reflectꓸValue v, encOpts opts) {
    GoFrame ᒐ = default;
    try {
        ref var e = ref Ꮡe.DerefOrNull();

        if (v.IsNil()) {
            Ꮡe.of(encodeState.ᏑBuffer).WriteString(nullˢ);
            return;
        }
        {
            e.ptrLevel++; if (e.ptrLevel > startDetectingCyclesAfter) {
                // We're a large number of nested ptrEncoder.encode calls deep;
                // start checking if we've run into a pointer cycle.
                var ptr = v.Interface();
                {
                    var (_, ok) = e.ptrSeen[ptr, ꟷ]; if (ok) {
                        e.error(new UnsupportedValueErrorжerror(Ꮡ(new UnsupportedValueError(v, fmt.Sprintf("encountered a cycle via %s"u8, v.Type())))));
                    }
                }
                e.ptrSeen[ptr] = new EmptyStruct();
                defer((ᴛ1, ᴛ2) => delete(ᴛ1, ᴛ2), Ꮡe.Value.ptrSeen, ptr, ref ᒐ);
            }
        }
        pe.elemEnc(Ꮡe, v.Elem(), opts);
        e.ptrLevel--;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static Action<ж<encodeState>, reflectꓸValue, encOpts> newPtrEncoder(reflectꓸType t) {
    var enc = new ptrEncoder(typeEncoder(t.Elem()));
    var encʗ1 = enc;
    return (ж<encodeState> p1, reflectꓸValue p2, encOpts p3) => encʗ1.encode(p1, p2, p3);
}

partial struct condAddrEncoder {
    internal Action<ж<encodeState>, reflectꓸValue, encOpts> canAddrEnc, elseEnc;
}

internal static void encode(this condAddrEncoder ce, ж<encodeState> Ꮡe, reflectꓸValue v, encOpts opts) {
    if (v.CanAddr()){
        ce.canAddrEnc(Ꮡe, v, opts);
    } else {
        ce.elseEnc(Ꮡe, v, opts);
    }
}

// newCondAddrEncoder returns an encoder that checks whether its value
// CanAddr and delegates to canAddrEnc if so, else to elseEnc.
internal static Action<ж<encodeState>, reflectꓸValue, encOpts> newCondAddrEncoder(Action<ж<encodeState>, reflectꓸValue, encOpts> canAddrEnc, Action<ж<encodeState>, reflectꓸValue, encOpts> elseEnc) {
    var enc = new condAddrEncoder(canAddrEnc: canAddrEnc, elseEnc: elseEnc);
    var encʗ1 = enc;
    return (ж<encodeState> p1, reflectꓸValue p2, encOpts p3) => encʗ1.encode(p1, p2, p3);
}

internal static bool isValidTag(@string s) {
    if (s == ""u8) {
        return false;
    }
    foreach (var (_, c) in s) {
        switch (ᐧ) {
        case {} when strings.ContainsRune("!#$%&()*+-./:;<=>?@[]^_{|}~ "u8, c): {
            break;
        }
        case {} when !unicode.IsLetter(c) && !unicode.IsDigit(c): {
            return false;
        }}

    }
    // Backslash and quote chars are reserved, but
    // otherwise any punctuation chars are allowed
    // in a tag name.
    return true;
}

internal static reflectꓸType typeByIndex(reflectꓸType t, slice<nint> index) {
    foreach (var (_, i) in index) {
        if (t.Kind() == reflect.ΔPointer) {
            t = t.Elem();
        }
        t = t.Field(i).Type;
    }
    return t;
}

partial struct reflectWithString {
    internal reflectꓸValue v;
    internal @string ks;
}

internal static (@string, error) resolveKeyName(reflectꓸValue k) {
    if (k.Kind() == reflect.ΔString) {
        return (k.String(), default!);
    }
    {
        var (tm, ok) = k.Interface()._<encoding.TextMarshaler>(ᐧ); if (ok) {
            if (k.Kind() == reflect.ΔPointer && k.IsNil()) {
                return ("", default!);
            }
            var (buf, err) = tm.MarshalText();
            return (((@string)buf), err);
        }
    }
    var exprᴛ1 = k.Kind();
    if (exprᴛ1 == reflect.ΔInt || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64) {
        return (strconv.FormatInt(k.Int(), 10), default!);
    }
    if (exprᴛ1 == reflect.ΔUint || exprᴛ1 == reflect.Uint8 || exprᴛ1 == reflect.Uint16 || exprᴛ1 == reflect.Uint32 || exprᴛ1 == reflect.Uint64 || exprᴛ1 == reflect.Uintptr) {
        return (strconv.FormatUint(k.Uint(), 10), default!);
    }

    throw panic("unexpected map key type");
}

internal static slice<byte> appendString<Bytes>(slice<byte> dst, Bytes src, bool escapeHTML)
    where Bytes : /* []byte | string */ IByteSeq<Bytes, byte>, new()
{
    dst = append(dst, (byte)((rune)'"'));
    nint start = 0;
    for (nint i = 0; i < len(src); ) {
        {
            var b = src[i]; if (b < utf8.RuneSelf) {
                if (htmlSafeSet[b] || (!escapeHTML && safeSet[b])) {
                    i++;
                    continue;
                }
                dst = append(dst, src.slice(start, i).ꓸꓸꓸ);
                switch (b) {
                case (rune)'\\' or (rune)'"': {
                    dst = append(dst, (byte)((rune)'\\'), b);
                    break;
                }
                case (rune)'\b': {
                    dst = append(dst, (byte)((rune)'\\'), (byte)((rune)'b'));
                    break;
                }
                case (rune)'\f': {
                    dst = append(dst, (byte)((rune)'\\'), (byte)((rune)'f'));
                    break;
                }
                case (rune)'\n': {
                    dst = append(dst, (byte)((rune)'\\'), (byte)((rune)'n'));
                    break;
                }
                case (rune)'\r': {
                    dst = append(dst, (byte)((rune)'\\'), (byte)((rune)'r'));
                    break;
                }
                case (rune)'\t': {
                    dst = append(dst, (byte)((rune)'\\'), (byte)((rune)'t'));
                    break;
                }
                default: {
                    dst = append(dst, // This encodes bytes < 0x20 except for \b, \f, \n, \r and \t.
 // If escapeHTML is set, it also escapes <, >, and &
 // because they can lead to security holes when
 // user-controlled strings are rendered into JSON
 // and served to some browsers.
 (byte)((rune)'\\'), (byte)((rune)'u'), (byte)((rune)'0'), (byte)((rune)'0'), hex[(b >> (int)(4))], hex[(byte)(b & 0xF)]);
                    break;
                }}

                i++;
                start = i;
                continue;
            }
        }
        // TODO(https://go.dev/issue/56948): Use generic utf8 functionality.
        // For now, cast only a small portion of byte slices to a string
        // so that it can be stack allocated. This slows down []byte slightly
        // due to the extra copy, but keeps string performance roughly the same.
        nint n = len(src) - i;
        if (n > utf8.UTFMax) {
            n = utf8.UTFMax;
        }
        var (c, size) = utf8.DecodeRuneInString(src.slice(i, i + n).ToGoString());
        if (c == utf8.RuneError && size == 1) {
            dst = append(dst, src.slice(start, i).ꓸꓸꓸ);
            dst = append(dst, ((@string)@"\ufffd"u8).ꓸꓸꓸ);
            i += size;
            start = i;
            continue;
        }
        // U+2028 is LINE SEPARATOR.
        // U+2029 is PARAGRAPH SEPARATOR.
        // They are both technically valid characters in JSON strings,
        // but don't work in JSONP, which has to be evaluated as JavaScript,
        // and can lead to security holes there. It is valid JSON to
        // escape them, so we do so unconditionally.
        // See https://en.wikipedia.org/wiki/JSON#Safety.
        if (c == (rune)'\u2028' || c == (rune)'\u2029') {
            dst = append(dst, src.slice(start, i).ꓸꓸꓸ);
            dst = append(dst, (byte)((rune)'\\'), (byte)((rune)'u'), (byte)((rune)'2'), (byte)((rune)'0'), (byte)((rune)'2'), hex[(rune)(c & 0xF)]);
            i += size;
            start = i;
            continue;
        }
        i += size;
    }
    dst = append(dst, src.slice(start).ꓸꓸꓸ);
    dst = append(dst, (byte)((rune)'"'));
    return dst;
}

// A field represents a single field found in a struct.
partial struct field {
    internal @string name;
    internal slice<byte> nameBytes; // []byte(name)
    internal @string nameNonEsc; // `"` + name + `":`
    internal @string nameEscHTML; // `"` + HTMLEscape(name) + `":`
    internal bool tag;
    internal slice<nint> index;
    internal reflectꓸType typ;
    internal bool omitEmpty;
    internal bool omitZero;
    internal Func<reflectꓸValue, bool> isZero;
    internal bool quoted;
    internal Action<ж<encodeState>, reflectꓸValue, encOpts> encoder;
}

public partial interface isZeroer {
    bool IsZero();
}

internal static reflectꓸType isZeroerType = reflect.TypeFor<isZeroer>();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string jsonˢ = "json"u8;
internal static readonly @string stringˢ = "string"u8;
internal static readonly @string omitemptyˢ = "omitempty"u8;
internal static readonly @string omitzeroˢ = "omitzero"u8;

// typeFields returns a list of fields that JSON should recognize for the given type.
// The algorithm is breadth-first search over the set of structs to include - the top struct
// and then any reachable anonymous structs.
//
// typeFields should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname typeFields
internal static structFields typeFields(reflectꓸType t) {
    // Anonymous fields to explore at the current level and the next.
    var current = new field[]{}.slice();
    var next = new field[]{new(typ: t)}.slice();
    // Count of queued names for current level and the next.
    map<reflectꓸType, nint> count = default!;
    map<reflectꓸType, nint> nextCount = default!;
    // Types already visited at an earlier level.
    var visited = new map<reflectꓸType, bool>{};
    // Fields found.
    slice<field> fields = default!;
    // Buffer to run appendHTMLEscape on field names.
    slice<byte> nameEscBuf = default!;
    while (len(next) > 0) {
        (current, next) = (next, current[..0]);
        (count, nextCount) = (nextCount, new map<reflectꓸType, nint>{});
        foreach (var (_, f) in current) {
            if (visited[f.typ]) {
                continue;
            }
            visited[f.typ] = true;
            // Scan f.typ for fields to include.
            for (nint i = 0; i < f.typ.NumField(); i++) {
                var sf = f.typ.Field(i);
                if (sf.Anonymous){
                    var tΔ1 = sf.Type;
                    if (tΔ1.Kind() == reflect.ΔPointer) {
                        tΔ1 = tΔ1.Elem();
                    }
                    if (!sf.IsExported() && tΔ1.Kind() != reflect.Struct) {
                        // Ignore embedded fields of unexported non-struct types.
                        continue;
                    }
                } else 
                if (!sf.IsExported()) {
                    // Do not ignore embedded fields of unexported struct types
                    // since they may have exported fields.
                    // Ignore unexported non-embedded fields.
                    continue;
                }
                @string tag = sf.Tag.Get(jsonˢ);
                if (tag == "-"u8) {
                    continue;
                }
                var (name, opts) = parseTag(tag);
                if (!isValidTag(name)) {
                    name = ""u8;
                }
                var index = new slice<nint>(len(f.index) + 1);
                copy(index, f.index);
                index[len(f.index)] = i;
                var ft = sf.Type;
                if (ft.Name() == ""u8 && ft.Kind() == reflect.ΔPointer) {
                    // Follow pointer.
                    ft = ft.Elem();
                }
                // Only strings, floats, integers, and booleans can be quoted.
                var quoted = false;
                if (opts.Contains(stringˢ)) {
                    var exprᴛ1 = ft.Kind();
                    if (exprᴛ1 == reflect.ΔBool || exprᴛ1 == reflect.ΔInt || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64 || exprᴛ1 == reflect.ΔUint || exprᴛ1 == reflect.Uint8 || exprᴛ1 == reflect.Uint16 || exprᴛ1 == reflect.Uint32 || exprᴛ1 == reflect.Uint64 || exprᴛ1 == reflect.Uintptr || exprᴛ1 == reflect.Float32 || exprᴛ1 == reflect.Float64 || exprᴛ1 == reflect.ΔString) {
                        quoted = true;
                    }

                }
                // Record found field and index sequence.
                if (name != ""u8 || !sf.Anonymous || ft.Kind() != reflect.Struct) {
                    var tagged = name != ""u8;
                    if (name == ""u8) {
                        name = sf.Name;
                    }
                    var field = new field(
                        name: name,
                        tag: tagged,
                        index: index,
                        typ: ft,
                        omitEmpty: opts.Contains(omitemptyˢ),
                        omitZero: opts.Contains(omitzeroˢ),
                        quoted: quoted
                    );
                    field.nameBytes = slice<byte>(field.name);
                    // Build nameEscHTML and nameNonEsc ahead of time.
                    nameEscBuf = appendHTMLEscape(nameEscBuf[..0], field.nameBytes);
                    field.nameEscHTML = @""""u8 + ((sstring)nameEscBuf) + @""":"u8;
                    field.nameNonEsc = @""""u8 + field.name + @""":"u8;
                    if (field.omitZero) {
                        var tΔ2 = sf.Type;
                        // Provide a function that uses a type's IsZero method.
                        switch (ᐧ) {
                        case {} when tΔ2.Kind() == reflect.ΔInterface && tΔ2.Implements(isZeroerType): {
                            field.isZero = (reflectꓸValue v) => {
                                // Avoid panics calling IsZero on a nil interface or
                                // non-nil interface with nil pointer.
                                return v.IsNil() || (v.Elem().Kind() == reflect.ΔPointer && v.Elem().IsNil()) || v.Interface()._<isZeroer>().IsZero();
                            };
                            break;
                        }
                        case {} when tΔ2.Kind() == reflect.ΔPointer && tΔ2.Implements(isZeroerType): {
                            field.isZero = (reflectꓸValue v) => {
                                // Avoid panics calling IsZero on nil pointer.
                                return v.IsNil() || v.Interface()._<isZeroer>().IsZero();
                            };
                            break;
                        }
                        case {} when tΔ2.Implements(isZeroerType): {
                            field.isZero = (reflectꓸValue v) => v.Interface()._<isZeroer>().IsZero();
                            break;
                        }
                        case {} when reflect.PointerTo(tΔ2).Implements(isZeroerType): {
                            field.isZero = (reflectꓸValue v) => {
                                if (!v.CanAddr()) {
                                    // Temporarily box v so we can take the address.
                                    var v2 = reflect.New(v.Type()).Elem();
                                    v2.Set(v);
                                    v = v2;
                                }
                                return v.Addr().Interface()._<isZeroer>().IsZero();
                            };
                            break;
                        }}

                    }
                    fields = append(fields, field);
                    if (count[f.typ] > 1) {
                        // If there were multiple instances, add a second,
                        // so that the annihilation code will see a duplicate.
                        // It only cares about the distinction between 1 and 2,
                        // so don't bother generating any more copies.
                        fields = append(fields, fields[len(fields) - 1]);
                    }
                    continue;
                }
                // Record new anonymous struct to explore in next round.
                nextCount[ft]++;
                if (nextCount[ft] == 1) {
                    next = append(next, new field(name: ft.Name(), index: index, typ: ft));
                }
            }
        }
    }
    slices.SortFunc(fields, (field a, field b) => {
        // sort field by name, breaking ties with depth, then
        // breaking ties with "name came from json tag", then
        // breaking ties with index sequence.
        {
            nint c = strings.Compare(a.name, b.name); if (c != 0) {
                return c;
            }
        }
        {
            nint c = cmp.Compare(len(a.index), len(b.index)); if (c != 0) {
                return c;
            }
        }
        if (a.tag != b.tag) {
            if (a.tag) {
                return -1;
            }
            return +1;
        }
        return slices.Compare<slice<nint>, nint>(a.index, b.index);
    });
    // Delete all fields that are hidden by the Go rules for embedded fields,
    // except that fields with JSON tags are promoted.
    // The fields are sorted in primary order of name, secondary order
    // of field index length. Loop over names; for each name, delete
    // hidden fields by choosing the one dominant field that survives.
    var @out = fields[..0];
    for ((nint advance, nint i) = (0, 0); i < len(fields); i += advance) {
        // One iteration per name.
        // Find the sequence of fields with the name of this first field.
        var fi = fields[i];
        @string name = fi.name;
        for (advance = 1; i + advance < len(fields); advance++) {
            var fj = fields[i + advance];
            if (fj.name != name) {
                break;
            }
        }
        if (advance == 1) {
            // Only one field with this name
            @out = append(@out, fi);
            continue;
        }
        var (dominant, ok) = dominantField(fields.slice(i, i + advance));
        if (ok) {
            @out = append(@out, dominant);
        }
    }
    fields = @out;
    slices.SortFunc(fields, (field i, field j) => slices.Compare<slice<nint>, nint>(i.index, j.index));
    foreach (var (i, _) in fields) {
        var f = Ꮡ(fields, i);
        f.Value.encoder = typeEncoder(typeByIndex(t, (~f).index));
    }
    var exactNameIndex = new map<@string, ж<field>>(len(fields));
    var foldedNameIndex = new map<@string, ж<field>>(len(fields));
    foreach (var (i, field) in fields) {
        exactNameIndex[field.name] = Ꮡ(fields, i);
        // For historical reasons, first folded match takes precedence.
        {
            var (_, ok) = foldedNameIndex[tmpstring(foldName(field.nameBytes)), ꟷ]; if (!ok) {
                foldedNameIndex[((@string)foldName(field.nameBytes))] = Ꮡ(fields, i);
            }
        }
    }
    return new structFields(fields, exactNameIndex, foldedNameIndex);
}

// dominantField looks through the fields, all of which are known to
// have the same name, to find the single field that dominates the
// others using Go's embedding rules, modified by the presence of
// JSON tags. If there are multiple top-level fields, the boolean
// will be false: This condition is an error in Go and we skip all
// the fields.
internal static (field, bool) dominantField(slice<field> fields) {
    // The fields are sorted in increasing index-length order, then by presence of tag.
    // That means that the first field is the dominant one. We need only check
    // for error cases: two fields at top level, either both tagged or neither tagged.
    if (len(fields) > 1 && len(fields[0].index) == len(fields[1].index) && fields[0].tag == fields[1].tag) {
        return (new field(nil), false);
    }
    return (fields[0], true);
}

internal static ж<sync.Map> ᏑfieldCache = new StandardBox<sync.Map>(default(sync.Map));
internal static ref sync.Map fieldCache => ref ᏑfieldCache.Value; // map[reflect.Type]structFields

// cachedTypeFields is like typeFields but uses a cache to avoid repeated work.
internal static structFields cachedTypeFields(reflectꓸType t) {
    {
        var (fΔ1, ok) = ᏑfieldCache.Load(t); if (ok) {
            return fΔ1._<structFields>();
        }
    }
    var (f, _) = ᏑfieldCache.LoadOrStore(t, typeFields(t));
    return f._<structFields>();
}

internal static slice<byte> mayAppendQuote(slice<byte> b, bool quoted) {
    if (quoted) {
        b = append(b, (byte)((rune)'"'));
    }
    return b;
}

} // end json_package
