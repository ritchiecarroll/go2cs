// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using bytes = bytes_package;
using encoding = encoding_package;
using errors = errors_package;
using fmt = fmt_package;
using reflect = reflect_package;
using runtime = runtime_package;
using strconv = strconv_package;
using strings = strings_package;
using io = io_package;

partial class xml_package {

// BUG(rsc): Mapping between XML elements and data structures is inherently flawed:
// an XML element is an order-dependent collection of anonymous
// values, while a data structure is an order-independent collection
// of named values.
// See [encoding/json] for a textual representation more suitable
// to data structures.

// Unmarshal parses the XML-encoded data and stores the result in
// the value pointed to by v, which must be an arbitrary struct,
// slice, or string. Well-formed data that does not fit into v is
// discarded.
//
// Because Unmarshal uses the reflect package, it can only assign
// to exported (upper case) fields. Unmarshal uses a case-sensitive
// comparison to match XML element names to tag values and struct
// field names.
//
// Unmarshal maps an XML element to a struct using the following rules.
// In the rules, the tag of a field refers to the value associated with the
// key 'xml' in the struct field's tag (see the example above).
//
//   - If the struct has a field of type []byte or string with tag
//     ",innerxml", Unmarshal accumulates the raw XML nested inside the
//     element in that field. The rest of the rules still apply.
//
//   - If the struct has a field named XMLName of type Name,
//     Unmarshal records the element name in that field.
//
//   - If the XMLName field has an associated tag of the form
//     "name" or "namespace-URL name", the XML element must have
//     the given name (and, optionally, name space) or else Unmarshal
//     returns an error.
//
//   - If the XML element has an attribute whose name matches a
//     struct field name with an associated tag containing ",attr" or
//     the explicit name in a struct field tag of the form "name,attr",
//     Unmarshal records the attribute value in that field.
//
//   - If the XML element has an attribute not handled by the previous
//     rule and the struct has a field with an associated tag containing
//     ",any,attr", Unmarshal records the attribute value in the first
//     such field.
//
//   - If the XML element contains character data, that data is
//     accumulated in the first struct field that has tag ",chardata".
//     The struct field may have type []byte or string.
//     If there is no such field, the character data is discarded.
//
//   - If the XML element contains comments, they are accumulated in
//     the first struct field that has tag ",comment".  The struct
//     field may have type []byte or string. If there is no such
//     field, the comments are discarded.
//
//   - If the XML element contains a sub-element whose name matches
//     the prefix of a tag formatted as "a" or "a>b>c", unmarshal
//     will descend into the XML structure looking for elements with the
//     given names, and will map the innermost elements to that struct
//     field. A tag starting with ">" is equivalent to one starting
//     with the field name followed by ">".
//
//   - If the XML element contains a sub-element whose name matches
//     a struct field's XMLName tag and the struct field has no
//     explicit name tag as per the previous rule, unmarshal maps
//     the sub-element to that struct field.
//
//   - If the XML element contains a sub-element whose name matches a
//     field without any mode flags (",attr", ",chardata", etc), Unmarshal
//     maps the sub-element to that struct field.
//
//   - If the XML element contains a sub-element that hasn't matched any
//     of the above rules and the struct has a field with tag ",any",
//     unmarshal maps the sub-element to that struct field.
//
//   - An anonymous struct field is handled as if the fields of its
//     value were part of the outer struct.
//
//   - A struct field with tag "-" is never unmarshaled into.
//
// If Unmarshal encounters a field type that implements the Unmarshaler
// interface, Unmarshal calls its UnmarshalXML method to produce the value from
// the XML element.  Otherwise, if the value implements
// [encoding.TextUnmarshaler], Unmarshal calls that value's UnmarshalText method.
//
// Unmarshal maps an XML element to a string or []byte by saving the
// concatenation of that element's character data in the string or
// []byte. The saved []byte is never nil.
//
// Unmarshal maps an attribute value to a string or []byte by saving
// the value in the string or slice.
//
// Unmarshal maps an attribute value to an [Attr] by saving the attribute,
// including its name, in the Attr.
//
// Unmarshal maps an XML element or attribute value to a slice by
// extending the length of the slice and mapping the element or attribute
// to the newly created value.
//
// Unmarshal maps an XML element or attribute value to a bool by
// setting it to the boolean value represented by the string. Whitespace
// is trimmed and ignored.
//
// Unmarshal maps an XML element or attribute value to an integer or
// floating-point field by setting the field to the result of
// interpreting the string value in decimal. There is no check for
// overflow. Whitespace is trimmed and ignored.
//
// Unmarshal maps an XML element to a Name by recording the element
// name.
//
// Unmarshal maps an XML element to a pointer by setting the pointer
// to a freshly allocated value and then mapping the element to that value.
//
// A missing element or empty attribute value will be unmarshaled as a zero value.
// If the field is a slice, a zero value will be appended to the field. Otherwise, the
// field will be set to its zero value.
public static error Unmarshal(slice<byte> data, any v) {
    return NewDecoder(new bytes_ReaderжReader(bytes.NewReader(data))).Decode(v);
}

// Decode works like [Unmarshal], except it reads the decoder
// stream to find the start element.
public static error Decode(this ж<Decoder> Ꮡd, any v) {
    return Ꮡd.DecodeElement(v, nil);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nonPointerPassedToˢ = "non-pointer passed to Unmarshal"u8;
internal static readonly @string nilPointerPassedToˢ = "nil pointer passed to Unmarshal"u8;

// DecodeElement works like [Unmarshal] except that it takes
// a pointer to the start XML element to decode into v.
// It is useful when a client reads some raw XML tokens itself
// but also wants to defer to [Unmarshal] for some elements.
public static error DecodeElement(this ж<Decoder> Ꮡd, any v, ж<StartElement> Ꮡstart) {
    var val = reflect.ValueOf(v);
    if (val.Kind() != reflect.ΔPointer) {
        return errors.New(nonPointerPassedToˢ);
    }
    if (val.IsNil()) {
        return errors.New(nilPointerPassedToˢ);
    }
    return Ꮡd.unmarshal(val.Elem(), Ꮡstart, 0);
}

partial struct UnmarshalError /*@string*/;

public static @string Error(this UnmarshalError e) {
    return ((@string)e);
}

// Unmarshaler is the interface implemented by objects that can unmarshal
// an XML element description of themselves.
//
// UnmarshalXML decodes a single XML element
// beginning with the given start element.
// If it returns an error, the outer call to Unmarshal stops and
// returns that error.
// UnmarshalXML must consume exactly one XML element.
// One common implementation strategy is to unmarshal into
// a separate value with a layout matching the expected XML
// using d.DecodeElement, and then to copy the data from
// that value into the receiver.
// Another common strategy is to use d.Token to process the
// XML object one token at a time.
// UnmarshalXML may not use d.RawToken.
partial interface Unmarshaler {
    error UnmarshalXML(ж<Decoder> d, StartElement start);
}

// UnmarshalerAttr is the interface implemented by objects that can unmarshal
// an XML attribute description of themselves.
//
// UnmarshalXMLAttr decodes a single XML attribute.
// If it returns an error, the outer call to [Unmarshal] stops and
// returns that error.
// UnmarshalXMLAttr is used only for struct fields with the
// "attr" option in the field tag.
partial interface UnmarshalerAttr {
    error UnmarshalXMLAttr(Attr attr);
}

// receiverType returns the receiver type to use in an expression like "%s.MethodName".
internal static @string receiverType(any val) {
    var t = reflect.TypeOf(val);
    if (t.Name() != ""u8) {
        return t.String();
    }
    return "("u8 + t.String() + ")"u8;
}

// unmarshalInterface unmarshals a single XML element into val.
// start is the opening tag of the element.
internal static error unmarshalInterface(this ж<Decoder> Ꮡd, Unmarshaler val, ж<StartElement> Ꮡstart) {
    ref var d = ref Ꮡd.DerefOrNull();
    ref var start = ref Ꮡstart.DerefOrNull();

    // Record that decoder must stop at end tag corresponding to start.
    d.pushEOF();
    d.unmarshalDepth++;
    var err = val.UnmarshalXML(Ꮡd, start);
    d.unmarshalDepth--;
    if (err != default!) {
        d.popEOF();
        return err;
    }
    if (!d.popEOF()) {
        return fmt.Errorf("xml: %s.UnmarshalXML did not consume entire <%s> element"u8, receiverType(val), start.Name.Local);
    }
    return default!;
}

// unmarshalTextInterface unmarshals a single XML element into val.
// The chardata contained in the element (but not its children)
// is passed to the text unmarshaler.
internal static error unmarshalTextInterface(this ref Decoder d, encoding.TextUnmarshaler val) {
    slice<byte> buf = default!;
    nint depth = 1;
    while (depth > 0) {
        var (t, err) = d.Token();
        if (err != default!) {
            return err;
        }
        switch (t.type()) {
        case CharData tΔ1: {
            if (depth == 1) {
                buf = appendꓸꓸꓸ(buf, tΔ1);
            }
            break;
        }
        case StartElement tΔ1: {
            depth++;
            break;
        }
        case EndElement tΔ1: {
            depth--;
            break;
        }}
    }
    return val.UnmarshalText(buf);
}

// unmarshalAttr unmarshals a single XML attribute into val.
internal static error unmarshalAttr(this ref Decoder d, reflectꓸValue val, Attr attr) {
    if (val.Kind() == reflect.ΔPointer) {
        if (val.IsNil()) {
            val.Set(reflect.New(val.Type().Elem()));
        }
        val = val.Elem();
    }
    if (val.CanInterface() && val.Type().Implements(unmarshalerAttrType)) {
        // This is an unmarshaler with a non-pointer receiver,
        // so it's likely to be incorrect, but we do what we're told.
        return val.Interface()._<UnmarshalerAttr>().UnmarshalXMLAttr(attr);
    }
    if (val.CanAddr()) {
        var pv = val.Addr();
        if (pv.CanInterface() && pv.Type().Implements(unmarshalerAttrType)) {
            return pv.Interface()._<UnmarshalerAttr>().UnmarshalXMLAttr(attr);
        }
    }
    // Not an UnmarshalerAttr; try encoding.TextUnmarshaler.
    if (val.CanInterface() && val.Type().Implements(textUnmarshalerType)) {
        // This is an unmarshaler with a non-pointer receiver,
        // so it's likely to be incorrect, but we do what we're told.
        return val.Interface()._<encoding.TextUnmarshaler>().UnmarshalText(slice<byte>(attr.Value));
    }
    if (val.CanAddr()) {
        var pv = val.Addr();
        if (pv.CanInterface() && pv.Type().Implements(textUnmarshalerType)) {
            return pv.Interface()._<encoding.TextUnmarshaler>().UnmarshalText(slice<byte>(attr.Value));
        }
    }
    if (val.Type().Kind() == reflect.ΔSlice && val.Type().Elem().Kind() != reflect.Uint8) {
        // Slice of element values.
        // Grow slice.
        nint n = val.Len();
        val.Grow(1);
        val.SetLen(n + 1);
        // Recur to read element into slice.
        {
            var err = d.unmarshalAttr(val.Index(n), attr); if (err != default!) {
                val.SetLen(n);
                return err;
            }
        }
        return default!;
    }
    if (AreEqual(val.Type(), attrType)) {
        val.Set(reflect.ValueOf(attr));
        return default!;
    }
    return copyValue(val, slice<byte>(attr.Value));
}

internal static reflectꓸType attrType = reflect.TypeFor<Attr>();
internal static reflectꓸType unmarshalerType = reflect.TypeFor<Unmarshaler>();
internal static reflectꓸType unmarshalerAttrType = reflect.TypeFor<UnmarshalerAttr>();
internal static reflectꓸType textUnmarshalerType = reflect.TypeFor<encoding.TextUnmarshaler>();

internal static UntypedInt maxUnmarshalDepth => 10000;
internal static UntypedInt maxUnmarshalDepthWasm => 5000; // go.dev/issue/56498

internal static error errUnmarshalDepth = errors.New("exceeded max depth"u8);

// Unmarshal a single XML element into val.
internal static error unmarshal(this ж<Decoder> Ꮡd, reflectꓸValue val, ж<StartElement> Ꮡstart, nint depth) {
    ref var d = ref Ꮡd.DerefOrNull();
    ref var start = ref Ꮡstart.DerefOrNull();

    if (depth >= maxUnmarshalDepth || runtime.GOARCH == "wasm"u8 && depth >= maxUnmarshalDepthWasm) {
        return errUnmarshalDepth;
    }
    // Find start element if we need it.
    if (Ꮡstart == nil) {
        while (ᐧ) {
            var (tok, errΔ1) = d.Token();
            if (errΔ1 != default!) {
                return errΔ1;
            }
            {
                ref var t = ref heap<StartElement>(out var Ꮡt);
                (t, var ok) = tok._<StartElement>(ᐧ); if (ok) {
                    Ꮡstart = Ꮡt; start = ref Ꮡstart.DerefOrNull();
                    break;
                }
            }
        }
    }
    // Load value from interface, but only if the result will be
    // usefully addressable.
    if (val.Kind() == reflect.ΔInterface && !val.IsNil()) {
        var e = val.Elem();
        if (e.Kind() == reflect.ΔPointer && !e.IsNil()) {
            val = e;
        }
    }
    if (val.Kind() == reflect.ΔPointer) {
        if (val.IsNil()) {
            val.Set(reflect.New(val.Type().Elem()));
        }
        val = val.Elem();
    }
    if (val.CanInterface() && val.Type().Implements(unmarshalerType)) {
        // This is an unmarshaler with a non-pointer receiver,
        // so it's likely to be incorrect, but we do what we're told.
        return Ꮡd.unmarshalInterface(val.Interface()._<Unmarshaler>(), Ꮡstart);
    }
    if (val.CanAddr()) {
        var pv = val.Addr();
        if (pv.CanInterface() && pv.Type().Implements(unmarshalerType)) {
            return Ꮡd.unmarshalInterface(pv.Interface()._<Unmarshaler>(), Ꮡstart);
        }
    }
    if (val.CanInterface() && val.Type().Implements(textUnmarshalerType)) {
        return d.unmarshalTextInterface(val.Interface()._<encoding.TextUnmarshaler>());
    }
    if (val.CanAddr()) {
        var pv = val.Addr();
        if (pv.CanInterface() && pv.Type().Implements(textUnmarshalerType)) {
            return d.unmarshalTextInterface(pv.Interface()._<encoding.TextUnmarshaler>());
        }
    }
    slice<byte> data = default!;
    reflectꓸValue saveData = new(nil);
    slice<byte> comment = default!;
    reflectꓸValue saveComment = new(nil);
    reflectꓸValue saveXML = new(nil);
    nint saveXMLIndex = default!;
    slice<byte> saveXMLData = default!;
    reflectꓸValue saveAny = new(nil);
    reflectꓸValue sv = new(nil);
    ж<typeInfo> tinfo = default!;
    error err = default!;
    {
        var v = val;
        var exprᴛ1 = v.Kind();
        if (exprᴛ1 == reflect.ΔInterface) {
            return d.Skip();
        }
        if (exprᴛ1 == reflect.ΔSlice) {
            do {
                var typ = v.Type();
                if (typ.Elem().Kind() == reflect.Uint8) {
                    // TODO: For now, simply ignore the field. In the near
                    //       future we may choose to unmarshal the start
                    //       element on it, if not nil.
                    // []byte
                    saveData = v;
                    break;
                }
                nint n = v.Len();
                v.Grow(1);
                v.SetLen(n + 1);
                {
                    var errΔ6 = Ꮡd.unmarshal(v.Index(n), // Slice of element values.
 // Grow slice.
 // Recur to read element into slice.
 Ꮡstart, depth + 1); if (errΔ6 != default!) {
                        v.SetLen(n);
                        return errΔ6;
                    }
                }
                return default!;
            } while (false);
        }
        else if (exprᴛ1 == reflect.ΔBool || exprᴛ1 == reflect.Float32 || exprᴛ1 == reflect.Float64 || exprᴛ1 == reflect.ΔInt || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64 || exprᴛ1 == reflect.ΔUint || exprᴛ1 == reflect.Uint8 || exprᴛ1 == reflect.Uint16 || exprᴛ1 == reflect.Uint32 || exprᴛ1 == reflect.Uint64 || exprᴛ1 == reflect.Uintptr || exprᴛ1 == reflect.ΔString) {
            saveData = v;
        }
        else if (exprᴛ1 == reflect.Struct) {
            do {
                var typ = v.Type();
                if (AreEqual(typ, nameType)) {
                    v.Set(reflect.ValueOf(start.Name));
                    break;
                }
                sv = v;
                (tinfo, err) = getTypeInfo(typ);
                if (err != default!) {
                    return err;
                }
                if ((~tinfo).xmlname != nil) {
                    // Validate and assign element name.
                    var finfo = tinfo.Value.xmlname;
                    if ((~finfo).name != ""u8 && (~finfo).name != start.Name.Local) {
                        return ((UnmarshalError)("expected element type <"u8 + (~finfo).name + "> but have <"u8 + start.Name.Local + ">"u8));
                    }
                    if ((~finfo).xmlns != ""u8 && (~finfo).xmlns != start.Name.Space) {
                        @string e = "expected element <"u8 + (~finfo).name + "> in name space "u8 + (~finfo).xmlns + " but have "u8;
                        if (start.Name.Space == ""u8){
                            e += "no name space"u8;
                        } else {
                            e += start.Name.Space;
                        }
                        return ((UnmarshalError)e);
                    }
                    var fv = finfo.value(sv, initNilPointers);
                    {
                        var (_, ok) = fv.Interface()._<Name>(ᐧ); if (ok) {
                            fv.Set(reflect.ValueOf(start.Name));
                        }
                    }
                }
                foreach (var (_, a) in start.Attr) {
                    // Assign attributes.
                    var handled = false;
                    nint any = -1;
                    foreach (var (i, _) in (~tinfo).fields) {
                        var finfo = Ꮡ((~tinfo).fields, i);
                        var exprᴛ2 = (fieldFlags)((~finfo).flags & fMode);
                        if (exprᴛ2 == fAttr) {
                            var strv = finfo.value(sv, initNilPointers);
                            if (a.Name.Local == (~finfo).name && ((~finfo).xmlns == ""u8 || (~finfo).xmlns == a.Name.Space)) {
                                {
                                    var errΔ8 = d.unmarshalAttr(strv, a); if (errΔ8 != default!) {
                                        return errΔ8;
                                    }
                                }
                                handled = true;
                            }
                        }
                        else if (exprᴛ2 == (fieldFlags)((fieldFlags)(fAny | fAttr))) {
                            if (any == -1) {
                                any = i;
                            }
                        }

                    }
                    if (!handled && any >= 0) {
                        var finfo = Ꮡ((~tinfo).fields, any);
                        var strv = finfo.value(sv, initNilPointers);
                        {
                            var errΔ9 = d.unmarshalAttr(strv, a); if (errΔ9 != default!) {
                                return errΔ9;
                            }
                        }
                    }
                }
                foreach (var (i, _) in (~tinfo).fields) {
                    // Determine whether we need to save character data or comments.
                    var finfo = Ꮡ((~tinfo).fields, i);
                    var exprᴛ3 = (fieldFlags)((~finfo).flags & fMode);
                    if (exprᴛ3 == fCDATA || exprᴛ3 == fCharData) {
                        if (!saveData.IsValid()) {
                            saveData = finfo.value(sv, initNilPointers);
                        }
                    }
                    else if (exprᴛ3 == fComment) {
                        if (!saveComment.IsValid()) {
                            saveComment = finfo.value(sv, initNilPointers);
                        }
                    }
                    else if (exprᴛ3 == fAny || exprᴛ3 == (fieldFlags)((fieldFlags)(fAny | fElement))) {
                        if (!saveAny.IsValid()) {
                            saveAny = finfo.value(sv, initNilPointers);
                        }
                    }
                    else if (exprᴛ3 == fInnerXML) {
                        if (!saveXML.IsValid()) {
                            saveXML = finfo.value(sv, initNilPointers);
                            if (d.saved == nil){
                                saveXMLIndex = 0;
                                d.saved = @new<bytes.Buffer>();
                            } else {
                                saveXMLIndex = d.savedOffset();
                            }
                        }
                    }

                }
            } while (false);
        }
        else { /* default: */
            return errors.New("unknown type "u8 + v.Type().String());
        }
    }

    // Find end element.
    // Process sub-elements along the way.
Loop:
    while (ᐧ) {
        nint savedOffset = default!;
        if (saveXML.IsValid()) {
            savedOffset = d.savedOffset();
        }
        var (tok, errΔ10) = d.Token();
        if (errΔ10 != default!) {
            return errΔ10;
        }
        switch (tok.type()) {
        case StartElement tᴛ1: {
            ref var t = ref heap(tᴛ1, out var Ꮡt);
            var consumed = false;
            if (sv.IsValid()) {
                // unmarshalPath can call unmarshal, so we need to pass the depth through so that
                // we can continue to enforce the maximum recursion limit.
                (consumed, errΔ10) = Ꮡd.unmarshalPath(tinfo, sv, default!, Ꮡt, depth);
                if (errΔ10 != default!) {
                    return errΔ10;
                }
                if (!consumed && saveAny.IsValid()) {
                    consumed = true;
                    {
                        var errΔ11 = Ꮡd.unmarshal(saveAny, Ꮡt, depth + 1); if (errΔ11 != default!) {
                            return errΔ11;
                        }
                    }
                }
            }
            if (!consumed) {
                {
                    var errΔ12 = d.Skip(); if (errΔ12 != default!) {
                        return errΔ12;
                    }
                }
            }
            break;
        }
        case EndElement t: {
            if (saveXML.IsValid()) {
                saveXMLData = d.saved.Bytes().slice(saveXMLIndex, savedOffset);
                if (saveXMLIndex == 0) {
                    d.saved = default!;
                }
            }
            goto break_Loop;
            break;
        }
        case CharData t: {
            if (saveData.IsValid()) {
                data = appendꓸꓸꓸ(data, t);
            }
            break;
        }
        case Comment t: {
            if (saveComment.IsValid()) {
                comment = appendꓸꓸꓸ(comment, t);
            }
            break;
        }}
continue_Loop:;
    }
break_Loop:;
    if (saveData.IsValid() && saveData.CanInterface() && saveData.Type().Implements(textUnmarshalerType)) {
        {
            var errΔ13 = saveData.Interface()._<encoding.TextUnmarshaler>().UnmarshalText(data); if (errΔ13 != default!) {
                return errΔ13;
            }
        }
        saveData = new reflectꓸValue(nil);
    }
    if (saveData.IsValid() && saveData.CanAddr()) {
        var pv = saveData.Addr();
        if (pv.CanInterface() && pv.Type().Implements(textUnmarshalerType)) {
            {
                var errΔ14 = pv.Interface()._<encoding.TextUnmarshaler>().UnmarshalText(data); if (errΔ14 != default!) {
                    return errΔ14;
                }
            }
            saveData = new reflectꓸValue(nil);
        }
    }
    {
        var errΔ15 = copyValue(saveData, data); if (errΔ15 != default!) {
            return errΔ15;
        }
    }
    {
        var t = saveComment;
        var exprᴛ4 = t.Kind();
        if (exprᴛ4 == reflect.ΔString) {
            t.SetString(((@string)comment));
        }
        else if (exprᴛ4 == reflect.ΔSlice) {
            t.Set(reflect.ValueOf(comment));
        }
    }

    {
        var t = saveXML;
        var exprᴛ5 = t.Kind();
        if (exprᴛ5 == reflect.ΔString) {
            t.SetString(((@string)saveXMLData));
        }
        else if (exprᴛ5 == reflect.ΔSlice) {
            if (t.Type().Elem().Kind() == reflect.Uint8) {
                t.Set(reflect.ValueOf(saveXMLData));
            }
        }
    }

    return default!;
}

internal static error /*err*/ copyValue(reflectꓸValue dst, slice<byte> src) {
    var dst0 = dst;
    if (dst.Kind() == reflect.ΔPointer) {
        if (dst.IsNil()) {
            dst.Set(reflect.New(dst.Type().Elem()));
        }
        dst = dst.Elem();
    }
    // Save accumulated data.
    var exprᴛ1 = dst.Kind();
    if (exprᴛ1 == reflect.Invalid) {
    }
    else if (exprᴛ1 == reflect.ΔInt || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64) {
        if (len(src) == 0) {
            // Probably a comment.
            dst.SetInt(0);
            return default!;
        }
        var (itmp, errΔ2) = strconv.ParseInt(strings.TrimSpace(((@string)src)), 10, dst.Type().Bits());
        if (errΔ2 != default!) {
            return errΔ2;
        }
        dst.SetInt(itmp);
    }
    else if (exprᴛ1 == reflect.ΔUint || exprᴛ1 == reflect.Uint8 || exprᴛ1 == reflect.Uint16 || exprᴛ1 == reflect.Uint32 || exprᴛ1 == reflect.Uint64 || exprᴛ1 == reflect.Uintptr) {
        if (len(src) == 0) {
            dst.SetUint(0);
            return default!;
        }
        var (utmp, errΔ3) = strconv.ParseUint(strings.TrimSpace(((@string)src)), 10, dst.Type().Bits());
        if (errΔ3 != default!) {
            return errΔ3;
        }
        dst.SetUint(utmp);
    }
    else if (exprᴛ1 == reflect.Float32 || exprᴛ1 == reflect.Float64) {
        if (len(src) == 0) {
            dst.SetFloat(0D);
            return default!;
        }
        var (ftmp, errΔ4) = strconv.ParseFloat(strings.TrimSpace(((@string)src)), dst.Type().Bits());
        if (errΔ4 != default!) {
            return errΔ4;
        }
        dst.SetFloat(ftmp);
    }
    else if (exprᴛ1 == reflect.ΔBool) {
        if (len(src) == 0) {
            dst.SetBool(false);
            return default!;
        }
        var (value, errΔ5) = strconv.ParseBool(strings.TrimSpace(((@string)src)));
        if (errΔ5 != default!) {
            return errΔ5;
        }
        dst.SetBool(value);
    }
    else if (exprᴛ1 == reflect.ΔString) {
        dst.SetString(((@string)src));
    }
    else if (exprᴛ1 == reflect.ΔSlice) {
        if (len(src) == 0) {
            // non-nil to flag presence
            src = new byte[]{}.slice();
        }
        dst.SetBytes(src);
    }
    else { /* default: */
        return errors.New("cannot unmarshal into "u8 + dst0.Type().String());
    }

    return default!;
}

// unmarshalPath walks down an XML structure looking for wanted
// paths, and calls unmarshal on them.
// The consumed result tells whether XML elements have been consumed
// from the Decoder until start's matching end element, or if it's
// still untouched because start is uninteresting for sv's fields.
internal static (bool consumed, error err) unmarshalPath(this ж<Decoder> Ꮡd, ж<typeInfo> Ꮡtinfo, reflectꓸValue sv, slice<@string> parents, ж<StartElement> Ꮡstart, nint depth) {
    error err = default!;

    ref var d = ref Ꮡd.DerefOrNull();
    ref var tinfo = ref Ꮡtinfo.DerefOrNull();
    ref var start = ref Ꮡstart.DerefOrNull();
    var recurse = false;
Loop:
    foreach (var (i, _) in tinfo.fields) {
        var finfo = Ꮡ(tinfo.fields, i);
        if ((fieldFlags)((~finfo).flags & fElement) == 0 || len((~finfo).parents) < len(parents) || (~finfo).xmlns != ""u8 && (~finfo).xmlns != start.Name.Space) {
            continue;
        }
        foreach (var (j, _) in parents) {
            if (parents[j] != (~finfo).parents[j]) {
                goto continue_Loop;
            }
        }
        if (len((~finfo).parents) == len(parents) && (~finfo).name == start.Name.Local) {
            // It's a perfect match, unmarshal the field.
            return (true, Ꮡd.unmarshal(finfo.value(sv, initNilPointers), Ꮡstart, depth + 1));
        }
        if (len((~finfo).parents) > len(parents) && (~finfo).parents[len(parents)] == start.Name.Local) {
            // It's a prefix for the field. Break and recurse
            // since it's not ok for one field path to be itself
            // the prefix for another field path.
            recurse = true;
            // We can reuse the same slice as long as we
            // don't try to append to it.
            parents = (~finfo).parents.slice(0, len(parents) + 1);
            break;
        }
continue_Loop:;
    }
break_Loop:;
    if (!recurse) {
        // We have no business with this element.
        return (false, default!);
    }
    // The element is not a perfect match for any field, but one
    // or more fields have the path to this element as a parent
    // prefix. Recurse and attempt to match these.
    while (ᐧ) {
        ΔToken tok = default!;
        (tok, err) = d.Token();
        if (err != default!) {
            return (true, err);
        }
        switch (tok.type()) {
        case StartElement tᴛ2: {
            ref var t = ref heap(tᴛ2, out var Ꮡt);
            var (consumed2, errΔ1) = Ꮡd.unmarshalPath(Ꮡtinfo, // the recursion depth of unmarshalPath is limited to the path length specified
 // by the struct field tag, so we don't increment the depth here.
 sv, parents, Ꮡt, depth);
            if (errΔ1 != default!) {
                return (true, errΔ1);
            }
            if (!consumed2) {
                {
                    var errΔ2 = d.Skip(); if (errΔ2 != default!) {
                        return (true, errΔ2);
                    }
                }
            }
            break;
        }
        case EndElement t: {
            return (true, default!);
        }}
    }
}

// Skip reads tokens until it has consumed the end element
// matching the most recent start element already consumed,
// skipping nested structures.
// It returns nil if it finds an end element matching the start
// element; otherwise it returns an error describing the problem.
public static error Skip(this ref Decoder d) {
    int64 depth = default!;
    while (ᐧ) {
        var (tok, err) = d.Token();
        if (err != default!) {
            return err;
        }
        switch (tok.type()) {
        case StartElement: {
            depth++;
            break;
        }
        case EndElement: {
            if (depth == 0) {
                return default!;
            }
            depth--;
            break;
        }}

    }
}

} // end xml_package
