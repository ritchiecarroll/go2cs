// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using bufio = bufio_package;
using bytes = bytes_package;
using encoding = encoding_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using reflect = reflect_package;
using strconv = strconv_package;
using strings = strings_package;

partial class xml_package {

public static readonly @string Header = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n";

// Marshal returns the XML encoding of v.
//
// Marshal handles an array or slice by marshaling each of the elements.
// Marshal handles a pointer by marshaling the value it points at or, if the
// pointer is nil, by writing nothing. Marshal handles an interface value by
// marshaling the value it contains or, if the interface value is nil, by
// writing nothing. Marshal handles all other data by writing one or more XML
// elements containing the data.
//
// The name for the XML elements is taken from, in order of preference:
//   - the tag on the XMLName field, if the data is a struct
//   - the value of the XMLName field of type [Name]
//   - the tag of the struct field used to obtain the data
//   - the name of the struct field used to obtain the data
//   - the name of the marshaled type
//
// The XML element for a struct contains marshaled elements for each of the
// exported fields of the struct, with these exceptions:
//   - the XMLName field, described above, is omitted.
//   - a field with tag "-" is omitted.
//   - a field with tag "name,attr" becomes an attribute with
//     the given name in the XML element.
//   - a field with tag ",attr" becomes an attribute with the
//     field name in the XML element.
//   - a field with tag ",chardata" is written as character data,
//     not as an XML element.
//   - a field with tag ",cdata" is written as character data
//     wrapped in one or more <![CDATA[ ... ]]> tags, not as an XML element.
//   - a field with tag ",innerxml" is written verbatim, not subject
//     to the usual marshaling procedure.
//   - a field with tag ",comment" is written as an XML comment, not
//     subject to the usual marshaling procedure. It must not contain
//     the "--" string within it.
//   - a field with a tag including the "omitempty" option is omitted
//     if the field value is empty. The empty values are false, 0, any
//     nil pointer or interface value, and any array, slice, map, or
//     string of length zero.
//   - an anonymous struct field is handled as if the fields of its
//     value were part of the outer struct.
//   - an anonymous struct field of interface type is treated the same as having
//     that type as its name, rather than being anonymous.
//   - a field implementing [Marshaler] is written by calling its MarshalXML
//     method.
//   - a field implementing [encoding.TextMarshaler] is written by encoding the
//     result of its MarshalText method as text.
//
// If a field uses a tag "a>b>c", then the element c will be nested inside
// parent elements a and b. Fields that appear next to each other that name
// the same parent will be enclosed in one XML element.
//
// If the XML name for a struct field is defined by both the field tag and the
// struct's XMLName field, the names must match.
//
// See [MarshalIndent] for an example.
//
// Marshal will return an error if asked to marshal a channel, function, or map.
public static (slice<byte>, error) Marshal(any v) {
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    var enc = NewEncoder(new bytes_BufferжWriter(Ꮡb));
    {
        var err = enc.Encode(v); if (err != default!) {
            return (default!, err);
        }
    }
    {
        var err = enc.Close(); if (err != default!) {
            return (default!, err);
        }
    }
    return (b.Bytes(), default!);
}

// Marshaler is the interface implemented by objects that can marshal
// themselves into valid XML elements.
//
// MarshalXML encodes the receiver as zero or more XML elements.
// By convention, arrays or slices are typically encoded as a sequence
// of elements, one per entry.
// Using start as the element tag is not required, but doing so
// will enable [Unmarshal] to match the XML elements to the correct
// struct field.
// One common implementation strategy is to construct a separate
// value with a layout corresponding to the desired XML and then
// to encode it using e.EncodeElement.
// Another common strategy is to use repeated calls to e.EncodeToken
// to generate the XML output one token at a time.
// The sequence of encoded tokens must make up zero or more valid
// XML elements.
[GoType] partial interface Marshaler {
    error MarshalXML(ж<Encoder> e, StartElement start);
}

// MarshalerAttr is the interface implemented by objects that can marshal
// themselves into valid XML attributes.
//
// MarshalXMLAttr returns an XML attribute with the encoded value of the receiver.
// Using name as the attribute name is not required, but doing so
// will enable [Unmarshal] to match the attribute to the correct
// struct field.
// If MarshalXMLAttr returns the zero attribute [Attr]{}, no attribute
// will be generated in the output.
// MarshalXMLAttr is used only for struct fields with the
// "attr" option in the field tag.
[GoType] partial interface MarshalerAttr {
    (Attr, error) MarshalXMLAttr(Name name);
}

// MarshalIndent works like [Marshal], but each XML element begins on a new
// indented line that starts with prefix and is followed by one or more
// copies of indent according to the nesting depth.
public static (slice<byte>, error) MarshalIndent(any v, @string prefix, @string indent) {
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    var enc = NewEncoder(new bytes_BufferжWriter(Ꮡb));
    enc.Indent(prefix, indent);
    {
        var err = enc.Encode(v); if (err != default!) {
            return (default!, err);
        }
    }
    {
        var err = enc.Close(); if (err != default!) {
            return (default!, err);
        }
    }
    return (b.Bytes(), default!);
}

// An Encoder writes XML data to an output stream.
[GoType] partial struct Encoder {
    internal printer p;
}

// NewEncoder returns a new encoder that writes to w.
public static ж<Encoder> NewEncoder(io.Writer w) {
    var e = Ꮡ(new Encoder(new printer(w: bufio.NewWriter(w))));
    e.Value.p.encoder = e;
    return e;
}

// Indent sets the encoder to generate XML in which each element
// begins on a new indented line that starts with prefix and is followed by
// one or more copies of indent according to the nesting depth.
[GoRecv] public static void Indent(this ref Encoder enc, @string prefix, @string indent) {
    enc.p.prefix = prefix;
    enc.p.indent = indent;
}

// Encode writes the XML encoding of v to the stream.
//
// See the documentation for [Marshal] for details about the conversion
// of Go values to XML.
//
// Encode calls [Encoder.Flush] before returning.
public static error Encode(this ж<Encoder> Ꮡenc, any v) {
    ref var enc = ref Ꮡenc.DerefOrNull();

    var err = Ꮡenc.of(Encoder.Ꮡp).marshalValue(reflect.ValueOf(v), nil, nil);
    if (err != default!) {
        return err;
    }
    return enc.p.w.Flush();
}

// EncodeElement writes the XML encoding of v to the stream,
// using start as the outermost tag in the encoding.
//
// See the documentation for [Marshal] for details about the conversion
// of Go values to XML.
//
// EncodeElement calls [Encoder.Flush] before returning.
public static error EncodeElement(this ж<Encoder> Ꮡenc, any v, StartElement startʗp) {
    ref var enc = ref Ꮡenc.DerefOrNull();

    ref var start = ref heap(startʗp, out var Ꮡstart);
    var err = Ꮡenc.of(Encoder.Ꮡp).marshalValue(reflect.ValueOf(v), nil, Ꮡstart);
    if (err != default!) {
        return err;
    }
    return enc.p.w.Flush();
}

internal static slice<byte> begComment = slice<byte>("<!--"u8);
internal static slice<byte> endComment = slice<byte>("-->"u8);
internal static slice<byte> endProcInst = slice<byte>("?>"u8);

// EncodeToken writes the given XML token to the stream.
// It returns an error if [StartElement] and [EndElement] tokens are not properly matched.
//
// EncodeToken does not call [Encoder.Flush], because usually it is part of a larger operation
// such as [Encoder.Encode] or [Encoder.EncodeElement] (or a custom [Marshaler]'s MarshalXML invoked
// during those), and those will call Flush when finished.
// Callers that create an Encoder and then invoke EncodeToken directly, without
// using Encode or EncodeElement, need to call Flush when finished to ensure
// that the XML is written to the underlying writer.
//
// EncodeToken allows writing a [ProcInst] with Target set to "xml" only as the first token
// in the stream.
public static error EncodeToken(this ж<Encoder> Ꮡenc, ΔToken t) {
    var p = Ꮡenc.of(Encoder.Ꮡp);
    switch (t.type()) {
    case StartElement tΔ1ᴛ1: {
        ref var tΔ1 = ref heap(tΔ1ᴛ1, out var ᏑtΔ1);
        {
            var err = p.writeStart(ᏑtΔ1); if (err != default!) {
                return err;
            }
        }
        break;
    }
    case EndElement tΔ1: {
        {
            var err = p.writeEnd(tΔ1.Name); if (err != default!) {
                return err;
            }
        }
        break;
    }
    case CharData tΔ1: {
        escapeText(new printerжWriter(p), tΔ1, false);
        break;
    }
    case Comment tΔ1: {
        if (bytes.Contains(tΔ1, endComment)) {
            return fmt.Errorf("xml: EncodeToken of Comment containing --> marker"u8);
        }
        p.WriteString("<!--"u8);
        p.Write(tΔ1);
        p.WriteString("-->"u8);
        return p.cachedWriteError();
    }
    case ProcInst tΔ1: {
        if (tΔ1.Target == "xml"u8 && (~p).w.Buffered() != 0) {
            // First token to be encoded which is also a ProcInst with target of xml
            // is the xml declaration. The only ProcInst where target of xml is allowed.
            return fmt.Errorf("xml: EncodeToken of ProcInst xml target only valid for xml declaration, first token encoded"u8);
        }
        if (!isNameString(tΔ1.Target)) {
            return fmt.Errorf("xml: EncodeToken of ProcInst with invalid Target"u8);
        }
        if (bytes.Contains(tΔ1.Inst, endProcInst)) {
            return fmt.Errorf("xml: EncodeToken of ProcInst containing ?> marker"u8);
        }
        p.WriteString("<?"u8);
        p.WriteString(tΔ1.Target);
        if (len(tΔ1.Inst) > 0) {
            p.WriteByte((rune)' ');
            p.Write(tΔ1.Inst);
        }
        p.WriteString("?>"u8);
        break;
    }
    case Directive tΔ1: {
        if (!isValidDirective(tΔ1)) {
            return fmt.Errorf("xml: EncodeToken of Directive containing wrong < or > markers"u8);
        }
        p.WriteString("<!"u8);
        p.Write(tΔ1);
        p.WriteString(">"u8);
        break;
    }
    default: {
        var tΔ1 = t;
        return fmt.Errorf("xml: EncodeToken of invalid token type"u8);
    }}
    return p.cachedWriteError();
}

// isValidDirective reports whether dir is a valid directive text,
// meaning angle brackets are matched, ignoring comments and strings.
internal static bool isValidDirective(Directive dir) {
    nint depth = default!;
    uint8 inquote = default!;
    bool incomment = default!;
    foreach (var (i, c) in dir) {
        switch (ᐧ) {
        case {} when incomment: {
            if (c == (rune)'>') {
                {
                    nint n = 1 + i - len(endComment); if (n >= 0 && bytes.Equal(dir.slice(n, i + 1), endComment)) {
                        incomment = false;
                    }
                }
            }
            break;
        }
        case {} when inquote is not 0: {
            if (c == inquote) {
                // Just ignore anything in comment
                inquote = 0;
            }
            break;
        }
        case {} when c == (rune)'\'' || c == (rune)'"': {
            inquote = c;
            break;
        }
        case {} when c is (rune)'<': {
            if (i + len(begComment) < len(dir) && bytes.Equal(dir.slice(i, i + len(begComment)), // Just ignore anything within quotes
 begComment)){
                incomment = true;
            } else {
                depth++;
            }
            break;
        }
        case {} when c is (rune)'>': {
            if (depth == 0) {
                return false;
            }
            depth--;
            break;
        }}

    }
    return depth == 0 && inquote == 0 && !incomment;
}

// Flush flushes any buffered XML to the underlying writer.
// See the [Encoder.EncodeToken] documentation for details about when it is necessary.
[GoRecv] public static error Flush(this ref Encoder enc) {
    return enc.p.w.Flush();
}

// Close the Encoder, indicating that no more data will be written. It flushes
// any buffered XML to the underlying writer and returns an error if the
// written XML is invalid (e.g. by containing unclosed elements).
[GoRecv] public static error Close(this ref Encoder enc) {
    return enc.p.Close();
}

[GoType] partial struct printer {
    internal ж<bufio.Writer> w;
    internal ж<Encoder> encoder;
    internal nint seq;
    internal @string indent;
    internal @string prefix;
    internal nint depth;
    internal bool indentedIn;
    internal bool putNewline;
    internal map<@string, @string> attrNS; // map prefix -> name space
    internal map<@string, @string> attrPrefix; // map name space -> prefix
    internal slice<@string> prefixes;
    internal slice<Name> tags;
    internal bool closed;
    internal error err;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xmlˢ = "xml"u8;
internal static readonly @string xmlnsˢ = @"xmlns:"u8;

// createAttrPrefix finds the name space prefix attribute to use for the given name space,
// defining a new prefix if necessary. It returns the prefix.
internal static @string createAttrPrefix(this ж<printer> Ꮡp, @string url) {
    ref var p = ref Ꮡp.DerefOrNull();

    {
        @string prefixΔ1 = p.attrPrefix[url]; if (prefixΔ1 != ""u8) {
            return prefixΔ1;
        }
    }
    // The "http://www.w3.org/XML/1998/namespace" name space is predefined as "xml"
    // and must be referred to that way.
    // (The "http://www.w3.org/2000/xmlns/" name space is also predefined as "xmlns",
    // but users should not be trying to use that one directly - that's our job.)
    if (url == xmlURL) {
        return xmlPrefix;
    }
    // Need to define a new name space.
    if (p.attrPrefix == default!) {
        p.attrPrefix = new map<@string, @string>();
        p.attrNS = new map<@string, @string>();
    }
    // Pick a name. We try to use the final element of the path
    // but fall back to _.
    @string prefix = strings.TrimRight(url, "/"u8);
    {
        nint i = strings.LastIndex(prefix, "/"u8); if (i >= 0) {
            prefix = prefix.slice(i + 1);
        }
    }
    if (prefix == ""u8 || !isName(slice<byte>(prefix)) || strings.Contains(prefix, ":"u8)) {
        prefix = "_"u8;
    }
    // xmlanything is reserved and any variant of it regardless of
    // case should be matched, so:
    //    (('X'|'x') ('M'|'m') ('L'|'l'))
    // See Section 2.3 of https://www.w3.org/TR/REC-xml/
    if (len(prefix) >= 3 && strings.EqualFold(prefix[..3], xmlˢ)) {
        prefix = "_"u8 + prefix;
    }
    if (p.attrNS[prefix] != "") {
        // Name is taken. Find a better one.
        for (p.seq++; ᐧ ; p.seq++) {
            {
                @string id = prefix + "_"u8 + strconv.Itoa(p.seq); if (p.attrNS[id] == "") {
                    prefix = id;
                    break;
                }
            }
        }
    }
    p.attrPrefix[url] = prefix;
    p.attrNS[prefix] = url;
    p.WriteString(xmlnsˢ);
    p.WriteString(prefix);
    p.WriteString(@"="""u8);
    EscapeText(new printerжWriter(Ꮡp), slice<byte>(url));
    p.WriteString(@""" "u8);
    p.prefixes = append(p.prefixes, prefix);
    return prefix;
}

// deleteAttrPrefix removes an attribute name space prefix.
[GoRecv] internal static void deleteAttrPrefix(this ref printer p, @string prefix) {
    delete(p.attrPrefix, p.attrNS[prefix]);
    delete(p.attrNS, prefix);
}

[GoRecv] internal static void markPrefix(this ref printer p) {
    p.prefixes = append(p.prefixes, ""u8);
}

[GoRecv] internal static void popPrefix(this ref printer p) {
    while (len(p.prefixes) > 0) {
        @string prefix = p.prefixes[len(p.prefixes) - 1];
        p.prefixes = p.prefixes.slice(0, len(p.prefixes) - 1);
        if (prefix == ""u8) {
            break;
        }
        p.deleteAttrPrefix(prefix);
    }
}

internal static reflectꓸType marshalerType = reflect.TypeFor<Marshaler>();
internal static reflectꓸType marshalerAttrType = reflect.TypeFor<MarshalerAttr>();
internal static reflectꓸType textMarshalerType = reflect.TypeFor<encoding.TextMarshaler>();

// marshalValue writes one or more XML elements representing val.
// If val was obtained from a struct field, finfo must have its details.
internal static error marshalValue(this ж<printer> Ꮡp, reflectꓸValue val, ж<fieldInfo> Ꮡfinfo, ж<StartElement> ᏑstartTemplate) {
    ref var p = ref Ꮡp.DerefOrNull();
    ref var finfo = ref Ꮡfinfo.DerefOrNull();
    ref var startTemplate = ref ᏑstartTemplate.DerefOrNull();

    if (ᏑstartTemplate != nil && startTemplate.Name.Local == ""u8) {
        return fmt.Errorf("xml: EncodeElement of StartElement with missing name"u8);
    }
    if (!val.IsValid()) {
        return default!;
    }
    if (Ꮡfinfo != nil && (fieldFlags)(finfo.flags & fOmitEmpty) != 0 && isEmptyValue(val)) {
        return default!;
    }
    // Drill into interfaces and pointers.
    // This can turn into an infinite loop given a cyclic chain,
    // but it matches the Go 1 behavior.
    while (val.Kind() == reflect.ΔInterface || val.Kind() == reflect.ΔPointer) {
        if (val.IsNil()) {
            return default!;
        }
        val = val.Elem();
    }
    reflectꓸKind kind = val.Kind();
    var typ = val.Type();
    // Check for marshaler.
    if (val.CanInterface() && typ.Implements(marshalerType)) {
        return p.marshalInterface(val.Interface()._<Marshaler>(), defaultStart(typ, Ꮡfinfo, ᏑstartTemplate));
    }
    if (val.CanAddr()) {
        var pv = val.Addr();
        if (pv.CanInterface() && pv.Type().Implements(marshalerType)) {
            return p.marshalInterface(pv.Interface()._<Marshaler>(), defaultStart(pv.Type(), Ꮡfinfo, ᏑstartTemplate));
        }
    }
    // Check for text marshaler.
    if (val.CanInterface() && typ.Implements(textMarshalerType)) {
        return Ꮡp.marshalTextInterface(val.Interface()._<encoding.TextMarshaler>(), defaultStart(typ, Ꮡfinfo, ᏑstartTemplate));
    }
    if (val.CanAddr()) {
        var pv = val.Addr();
        if (pv.CanInterface() && pv.Type().Implements(textMarshalerType)) {
            return Ꮡp.marshalTextInterface(pv.Interface()._<encoding.TextMarshaler>(), defaultStart(pv.Type(), Ꮡfinfo, ᏑstartTemplate));
        }
    }
    // Slices and arrays iterate over the elements. They do not have an enclosing tag.
    if ((kind == reflect.ΔSlice || kind == reflect.Array) && typ.Elem().Kind() != reflect.Uint8) {
        for ((nint i, nint n) = (0, val.Len()); i < n; i++) {
            {
                var errΔ1 = Ꮡp.marshalValue(val.Index(i), Ꮡfinfo, ᏑstartTemplate); if (errΔ1 != default!) {
                    return errΔ1;
                }
            }
        }
        return default!;
    }
    var (tinfo, err) = getTypeInfo(typ);
    if (err != default!) {
        return err;
    }
    // Create start element.
    // Precedence for the XML element name is:
    // 0. startTemplate
    // 1. XMLName field in underlying struct;
    // 2. field name/tag in the struct field; and
    // 3. type name
    ref var start = ref heap(new StartElement(), out var Ꮡstart);
    if (ᏑstartTemplate != nil){
        start.Name = startTemplate.Name;
        start.Attr = appendꓸꓸꓸ(start.Attr, startTemplate.Attr);
    } else 
    if ((~tinfo).xmlname != nil) {
        var xmlname = tinfo.Value.xmlname;
        if ((~xmlname).name != ""u8){
            (start.Name.Space, start.Name.Local) = (xmlname.Value.xmlns, xmlname.Value.name);
        } else {
            var fv = xmlname.value(val, dontInitNilPointers);
            {
                var (v, ok) = fv.Interface()._<Name>(ᐧ); if (ok && v.Local != ""u8) {
                    start.Name = v;
                }
            }
        }
    }
    if (start.Name.Local == ""u8 && Ꮡfinfo != nil) {
        (start.Name.Space, start.Name.Local) = (finfo.xmlns, finfo.name);
    }
    if (start.Name.Local == ""u8) {
        @string name = typ.Name();
        {
            nint i = strings.IndexByte(name, (rune)'['); if (i >= 0) {
                // Truncate generic instantiation name. See issue 48318.
                name = name.slice(0, i);
            }
        }
        if (name == ""u8) {
            return new UnsupportedTypeErrorжerror(Ꮡ(new UnsupportedTypeError(typ)));
        }
        start.Name.Local = name;
    }
    // Attributes
    foreach (var (i, _) in (~tinfo).fields) {
        var finfoΔ1 = Ꮡ((~tinfo).fields, i);
        if ((fieldFlags)((~finfoΔ1).flags & fAttr) == 0) {
            continue;
        }
        var fv = finfoΔ1.value(val, dontInitNilPointers);
        if ((fieldFlags)((~finfoΔ1).flags & fOmitEmpty) != 0 && (!fv.IsValid() || isEmptyValue(fv))) {
            continue;
        }
        if (fv.Kind() == reflect.ΔInterface && fv.IsNil()) {
            continue;
        }
        var name = new Name(Space: (~finfoΔ1).xmlns, Local: (~finfoΔ1).name);
        {
            var errΔ2 = p.marshalAttr(Ꮡstart, name, fv); if (errΔ2 != default!) {
                return errΔ2;
            }
        }
    }
    // If an empty name was found, namespace is overridden with an empty space
    if ((~tinfo).xmlname != nil && start.Name.Space == ""u8 && (~(~tinfo).xmlname).xmlns == ""u8 && (~(~tinfo).xmlname).name == ""u8 && len(p.tags) != 0 && p.tags[len(p.tags) - 1].Space != ""u8) {
        start.Attr = append(start.Attr, new Attr(new Name(""u8, xmlnsPrefix), ""u8));
    }
    {
        var errΔ3 = Ꮡp.writeStart(Ꮡstart); if (errΔ3 != default!) {
            return errΔ3;
        }
    }
    if (val.Kind() == reflect.Struct){
        err = Ꮡp.marshalStruct(tinfo, val);
    } else {
        var (s, b, err1) = p.marshalSimple(typ, val);
        if (err1 != default!){
            err = err1;
        } else 
        if (b != default!){
            EscapeText(new printerжWriter(Ꮡp), b);
        } else {
            p.EscapeString(s);
        }
    }
    if (err != default!) {
        return err;
    }
    {
        var errΔ4 = p.writeEnd(start.Name); if (errΔ4 != default!) {
            return errΔ4;
        }
    }
    return p.cachedWriteError();
}

// marshalAttr marshals an attribute with the given name and value, adding to start.Attr.
[GoRecv] internal static error marshalAttr(this ref printer p, ж<StartElement> Ꮡstart, Name name, reflectꓸValue val) {
    ref var start = ref Ꮡstart.DerefOrNull();

    if (val.CanInterface() && val.Type().Implements(marshalerAttrType)) {
        var (attr, errΔ1) = val.Interface()._<MarshalerAttr>().MarshalXMLAttr(name);
        if (errΔ1 != default!) {
            return errΔ1;
        }
        if (attr.Name.Local != ""u8) {
            start.Attr = append(start.Attr, attr);
        }
        return default!;
    }
    if (val.CanAddr()) {
        var pv = val.Addr();
        if (pv.CanInterface() && pv.Type().Implements(marshalerAttrType)) {
            var (attr, errΔ2) = pv.Interface()._<MarshalerAttr>().MarshalXMLAttr(name);
            if (errΔ2 != default!) {
                return errΔ2;
            }
            if (attr.Name.Local != ""u8) {
                start.Attr = append(start.Attr, attr);
            }
            return default!;
        }
    }
    if (val.CanInterface() && val.Type().Implements(textMarshalerType)) {
        var (text, errΔ3) = val.Interface()._<encoding.TextMarshaler>().MarshalText();
        if (errΔ3 != default!) {
            return errΔ3;
        }
        start.Attr = append(start.Attr, new Attr(name, ((@string)text)));
        return default!;
    }
    if (val.CanAddr()) {
        var pv = val.Addr();
        if (pv.CanInterface() && pv.Type().Implements(textMarshalerType)) {
            var (text, errΔ4) = pv.Interface()._<encoding.TextMarshaler>().MarshalText();
            if (errΔ4 != default!) {
                return errΔ4;
            }
            start.Attr = append(start.Attr, new Attr(name, ((@string)text)));
            return default!;
        }
    }
    // Dereference or skip nil pointer, interface values.
    var exprᴛ1 = val.Kind();
    if (exprᴛ1 == reflect.ΔPointer || exprᴛ1 == reflect.ΔInterface) {
        if (val.IsNil()) {
            return default!;
        }
        val = val.Elem();
    }

    // Walk slices.
    if (val.Kind() == reflect.ΔSlice && val.Type().Elem().Kind() != reflect.Uint8) {
        nint n = val.Len();
        for (nint i = 0; i < n; i++) {
            {
                var errΔ5 = p.marshalAttr(Ꮡstart, name, val.Index(i)); if (errΔ5 != default!) {
                    return errΔ5;
                }
            }
        }
        return default!;
    }
    if (AreEqual(val.Type(), attrType)) {
        start.Attr = append(start.Attr, val.Interface()._<Attr>());
        return default!;
    }
    var (s, b, err) = p.marshalSimple(val.Type(), val);
    if (err != default!) {
        return err;
    }
    if (b != default!) {
        s = ((@string)b);
    }
    start.Attr = append(start.Attr, new Attr(name, s));
    return default!;
}

// defaultStart returns the default start element to use,
// given the reflect type, field info, and start template.
internal static StartElement defaultStart(reflectꓸType typ, ж<fieldInfo> Ꮡfinfo, ж<StartElement> ᏑstartTemplate) {
    ref var finfo = ref Ꮡfinfo.DerefOrNull();
    ref var startTemplate = ref ᏑstartTemplate.DerefOrNull();

    StartElement start = default!;
    // Precedence for the XML element name is as above,
    // except that we do not look inside structs for the first field.
    if (ᏑstartTemplate != nil){
        start.Name = startTemplate.Name;
        start.Attr = appendꓸꓸꓸ(start.Attr, startTemplate.Attr);
    } else 
    if (Ꮡfinfo != nil && finfo.name != ""u8){
        start.Name.Local = finfo.name;
        start.Name.Space = finfo.xmlns;
    } else 
    if (typ.Name() != ""u8){
        start.Name.Local = typ.Name();
    } else {
        // Must be a pointer to a named type,
        // since it has the Marshaler methods.
        start.Name.Local = typ.Elem().Name();
    }
    return start;
}

// marshalInterface marshals a Marshaler interface value.
[GoRecv] internal static error marshalInterface(this ref printer p, Marshaler val, StartElement start) {
    // Push a marker onto the tag stack so that MarshalXML
    // cannot close the XML tags that it did not open.
    p.tags = append(p.tags, new Name(nil));
    nint n = len(p.tags);
    var err = val.MarshalXML(p.encoder, start);
    if (err != default!) {
        return err;
    }
    // Make sure MarshalXML closed all its tags. p.tags[n-1] is the mark.
    if (len(p.tags) > n) {
        return fmt.Errorf("xml: %s.MarshalXML wrote invalid XML: <%s> not closed"u8, receiverType(val), p.tags[len(p.tags) - 1].Local);
    }
    p.tags = p.tags.slice(0, n - 1);
    return default!;
}

// marshalTextInterface marshals a TextMarshaler interface value.
internal static error marshalTextInterface(this ж<printer> Ꮡp, encoding.TextMarshaler val, StartElement startʗp) {
    ref var p = ref Ꮡp.DerefOrNull();

    ref var start = ref heap(startʗp, out var Ꮡstart);
    {
        var errΔ1 = Ꮡp.writeStart(Ꮡstart); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    var (text, err) = val.MarshalText();
    if (err != default!) {
        return err;
    }
    EscapeText(new printerжWriter(Ꮡp), text);
    return p.writeEnd(start.Name);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xmlnsˢ2 = @" xmlns="""u8;

// writeStart writes the given start element.
internal static error writeStart(this ж<printer> Ꮡp, ж<StartElement> Ꮡstart) {
    ref var p = ref Ꮡp.DerefOrNull();
    ref var start = ref Ꮡstart.DerefOrNull();

    if (start.Name.Local == ""u8) {
        return fmt.Errorf("xml: start tag with no name"u8);
    }
    p.tags = append(p.tags, start.Name);
    p.markPrefix();
    p.writeIndent(1);
    p.WriteByte((rune)'<');
    p.WriteString(start.Name.Local);
    if (start.Name.Space != ""u8) {
        p.WriteString(xmlnsˢ2);
        p.EscapeString(start.Name.Space);
        p.WriteByte((rune)'"');
    }
    // Attributes
    foreach (var (_, attr) in start.Attr) {
        var name = attr.Name;
        if (name.Local == ""u8) {
            continue;
        }
        p.WriteByte((rune)' ');
        if (name.Space != ""u8) {
            p.WriteString(Ꮡp.createAttrPrefix(name.Space));
            p.WriteByte((rune)':');
        }
        p.WriteString(name.Local);
        p.WriteString(@"="""u8);
        p.EscapeString(attr.Value);
        p.WriteByte((rune)'"');
    }
    p.WriteByte((rune)'>');
    return default!;
}

[GoRecv] internal static error writeEnd(this ref printer p, Name name) {
    if (name.Local == ""u8) {
        return fmt.Errorf("xml: end tag with no name"u8);
    }
    if (len(p.tags) == 0 || p.tags[len(p.tags) - 1].Local == ""u8) {
        return fmt.Errorf("xml: end tag </%s> without start tag"u8, name.Local);
    }
    {
        var top = p.tags[len(p.tags) - 1]; if (top != name) {
            if (top.Local != name.Local) {
                return fmt.Errorf("xml: end tag </%s> does not match start tag <%s>"u8, name.Local, top.Local);
            }
            return fmt.Errorf("xml: end tag </%s> in namespace %s does not match start tag <%s> in namespace %s"u8, name.Local, name.Space, top.Local, top.Space);
        }
    }
    p.tags = p.tags.slice(0, len(p.tags) - 1);
    p.writeIndent(-1);
    p.WriteByte((rune)'<');
    p.WriteByte((rune)'/');
    p.WriteString(name.Local);
    p.WriteByte((rune)'>');
    p.popPrefix();
    return default!;
}

[GoRecv] internal static (@string, slice<byte>, error) marshalSimple(this ref printer p, reflectꓸType typ, reflectꓸValue val) {
    var exprᴛ1 = val.Kind();
    if (exprᴛ1 == reflect.ΔInt || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64) {
        return (strconv.FormatInt(val.Int(), 10), default!, default!);
    }
    if (exprᴛ1 == reflect.ΔUint || exprᴛ1 == reflect.Uint8 || exprᴛ1 == reflect.Uint16 || exprᴛ1 == reflect.Uint32 || exprᴛ1 == reflect.Uint64 || exprᴛ1 == reflect.Uintptr) {
        return (strconv.FormatUint(val.Uint(), 10), default!, default!);
    }
    if (exprᴛ1 == reflect.Float32 || exprᴛ1 == reflect.Float64) {
        return (strconv.FormatFloat(val.Float(), (rune)'g', -1, val.Type().Bits()), default!, default!);
    }
    if (exprᴛ1 == reflect.ΔString) {
        return (val.String(), default!, default!);
    }
    if (exprᴛ1 == reflect.ΔBool) {
        return (strconv.FormatBool(val.Bool()), default!, default!);
    }
    if (exprᴛ1 == reflect.Array) {
        do {
            if (typ.Elem().Kind() != reflect.Uint8) {
                break;
            }
            // [...]byte
            slice<byte> bytes = default!;
            if (val.CanAddr()){
                bytes = val.Bytes();
            } else {
                bytes = new slice<byte>(val.Len());
                reflect.Copy(reflect.ValueOf(bytes), val);
            }
            return ("", bytes, default!);
        } while (false);
    }
    else if (exprᴛ1 == reflect.ΔSlice) {
        do {
            if (typ.Elem().Kind() != reflect.Uint8) {
                break;
            }
            return ("", val.Bytes(), default!);
        } while (false);
    }

    // []byte
    return ("", default!, new UnsupportedTypeErrorжerror(Ꮡ(new UnsupportedTypeError(typ))));
}

internal static slice<byte> ddBytes = slice<byte>("--"u8);

// indirect drills into interfaces and pointers, returning the pointed-at value.
// If it encounters a nil interface or pointer, indirect returns that nil value.
// This can turn into an infinite loop given a cyclic chain,
// but it matches the Go 1 behavior.
internal static reflectꓸValue indirect(reflectꓸValue vf) {
    while (vf.Kind() == reflect.ΔInterface || vf.Kind() == reflect.ΔPointer) {
        if (vf.IsNil()) {
            return vf;
        }
        vf = vf.Elem();
    }
    return vf;
}

internal static error marshalStruct(this ж<printer> Ꮡp, ж<typeInfo> Ꮡtinfo, reflectꓸValue val) {
    ref var p = ref Ꮡp.DerefOrNull();
    ref var tinfo = ref Ꮡtinfo.DerefOrNull();

    var s = new parentStack(p: Ꮡp);
    foreach (var (i, _) in tinfo.fields) {
        var finfo = Ꮡ(tinfo.fields, i);
        if ((fieldFlags)((~finfo).flags & fAttr) != 0) {
            continue;
        }
        var vf = finfo.value(val, dontInitNilPointers);
        if (!vf.IsValid()) {
            // The field is behind an anonymous struct field that's
            // nil. Skip it.
            continue;
        }
        var exprᴛ1 = (fieldFlags)((~finfo).flags & fMode);
        if (exprᴛ1 == fCDATA || exprᴛ1 == fCharData) {
            var emit = EscapeText;
            if ((fieldFlags)((~finfo).flags & fMode) == fCDATA) {
                emit = emitCDATA;
            }
            {
                var err = s.trim((~finfo).parents); if (err != default!) {
                    return err;
                }
            }
            if (vf.CanInterface() && vf.Type().Implements(textMarshalerType)) {
                var (data, err) = vf.Interface()._<encoding.TextMarshaler>().MarshalText();
                if (err != default!) {
                    return err;
                }
                {
                    var errΔ1 = emit(new printerжWriter(Ꮡp), data); if (errΔ1 != default!) {
                        return errΔ1;
                    }
                }
                continue;
            }
            if (vf.CanAddr()) {
                var pv = vf.Addr();
                if (pv.CanInterface() && pv.Type().Implements(textMarshalerType)) {
                    var (data, err) = pv.Interface()._<encoding.TextMarshaler>().MarshalText();
                    if (err != default!) {
                        return err;
                    }
                    {
                        var errΔ1 = emit(new printerжWriter(Ꮡp), data); if (errΔ1 != default!) {
                            return errΔ1;
                        }
                    }
                    continue;
                }
            }
            array<byte> scratch = new(64);
            vf = indirect(vf);
            var exprᴛ2 = vf.Kind();
            if (exprᴛ2 == reflect.ΔInt || exprᴛ2 == reflect.Int8 || exprᴛ2 == reflect.Int16 || exprᴛ2 == reflect.Int32 || exprᴛ2 == reflect.Int64) {
                {
                    var err = emit(new printerжWriter(Ꮡp), strconv.AppendInt(scratch[..0], vf.Int(), 10)); if (err != default!) {
                        return err;
                    }
                }
            }
            else if (exprᴛ2 == reflect.ΔUint || exprᴛ2 == reflect.Uint8 || exprᴛ2 == reflect.Uint16 || exprᴛ2 == reflect.Uint32 || exprᴛ2 == reflect.Uint64 || exprᴛ2 == reflect.Uintptr) {
                {
                    var err = emit(new printerжWriter(Ꮡp), strconv.AppendUint(scratch[..0], vf.Uint(), 10)); if (err != default!) {
                        return err;
                    }
                }
            }
            else if (exprᴛ2 == reflect.Float32 || exprᴛ2 == reflect.Float64) {
                {
                    var err = emit(new printerжWriter(Ꮡp), strconv.AppendFloat(scratch[..0], vf.Float(), (rune)'g', -1, vf.Type().Bits())); if (err != default!) {
                        return err;
                    }
                }
            }
            else if (exprᴛ2 == reflect.ΔBool) {
                {
                    var err = emit(new printerжWriter(Ꮡp), strconv.AppendBool(scratch[..0], vf.Bool())); if (err != default!) {
                        return err;
                    }
                }
            }
            else if (exprᴛ2 == reflect.ΔString) {
                {
                    var err = emit(new printerжWriter(Ꮡp), slice<byte>(vf.String())); if (err != default!) {
                        return err;
                    }
                }
            }
            else if (exprᴛ2 == reflect.ΔSlice) {
                {
                    var (elem, ok) = vf.Interface()._<slice<byte>>(ᐧ); if (ok) {
                        {
                            var err = emit(new printerжWriter(Ꮡp), elem); if (err != default!) {
                                return err;
                            }
                        }
                    }
                }
            }

            continue;
        }
        else if (exprᴛ1 == fComment) {
            {
                var err = s.trim((~finfo).parents); if (err != default!) {
                    return err;
                }
            }
            vf = indirect(vf);
            reflectꓸKind k = vf.Kind();
            if (!(k == reflect.ΔString || k == reflect.ΔSlice && vf.Type().Elem().Kind() == reflect.Uint8)) {
                return fmt.Errorf("xml: bad type for comment field of %s"u8, val.Type());
            }
            if (vf.Len() == 0) {
                continue;
            }
            p.writeIndent(0);
            p.WriteString("<!--"u8);
            var dashDash = false;
            var dashLast = false;
            var exprᴛ3 = k;
            if (exprᴛ3 == reflect.ΔString) {
                @string sΔ4 = vf.String();
                dashDash = strings.Contains(sΔ4, "--"u8);
                dashLast = sΔ4[len(sΔ4) - 1] == (rune)'-';
                if (!dashDash) {
                    p.WriteString(sΔ4);
                }
            }
            else if (exprᴛ3 == reflect.ΔSlice) {
                var b = vf.Bytes();
                dashDash = bytes.Contains(b, ddBytes);
                dashLast = b[len(b) - 1] == (rune)'-';
                if (!dashDash) {
                    p.Write(b);
                }
            }
            else { /* default: */
                throw panic("can't happen");
            }

            if (dashDash) {
                return fmt.Errorf(@"xml: comments must not contain ""--"""u8);
            }
            if (dashLast) {
                // "--->" is invalid grammar. Make it "- -->"
                p.WriteByte((rune)' ');
            }
            p.WriteString("-->"u8);
            continue;
        }
        else if (exprᴛ1 == fInnerXML) {
            vf = indirect(vf);
            var iface = vf.Interface();
            switch (iface.type()) {
            case slice<byte> raw: {
                p.Write(raw);
                continue;
                break;
            }
            case @string raw: {
                p.WriteString(raw);
                continue;
                break;
            }}
        }
        else if (exprᴛ1 == fElement || exprᴛ1 == (fieldFlags)((fieldFlags)(fElement | fAny))) {
            {
                var err = s.trim((~finfo).parents); if (err != default!) {
                    return err;
                }
            }
            if (len((~finfo).parents) > len(s.stack)) {
                if (vf.Kind() != reflect.ΔPointer && vf.Kind() != reflect.ΔInterface || !vf.IsNil()) {
                    {
                        var err = s.push((~finfo).parents.slice(len(s.stack))); if (err != default!) {
                            return err;
                        }
                    }
                }
            }
        }

        {
            var err = Ꮡp.marshalValue(vf, finfo, nil); if (err != default!) {
                return err;
            }
        }
    }
    s.trim(default!);
    return p.cachedWriteError();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string useOfClosedEncoderˢ = "use of closed Encoder"u8;

// Write implements io.Writer
[GoRecv] internal static (nint n, error err) Write(this ref printer p, slice<byte> b) {
    nint n = default!;

    if (p.closed && p.err == default!) {
        p.err = errors.New(useOfClosedEncoderˢ);
    }
    if (p.err == default!) {
        (n, p.err) = p.w.Write(b);
    }
    return (n, p.err);
}

// WriteString implements io.StringWriter
[GoRecv] internal static (nint n, error err) WriteString(this ref printer p, @string s) {
    nint n = default!;

    if (p.closed && p.err == default!) {
        p.err = errors.New(useOfClosedEncoderˢ);
    }
    if (p.err == default!) {
        (n, p.err) = p.w.WriteString(s);
    }
    return (n, p.err);
}

// WriteByte implements io.ByteWriter
[GoRecv] internal static error WriteByte(this ref printer p, byte c) {
    if (p.closed && p.err == default!) {
        p.err = errors.New(useOfClosedEncoderˢ);
    }
    if (p.err == default!) {
        p.err = p.w.WriteByte(c);
    }
    return p.err;
}

// Close the Encoder, indicating that no more data will be written. It flushes
// any buffered XML to the underlying writer and returns an error if the
// written XML is invalid (e.g. by containing unclosed elements).
[GoRecv] internal static error Close(this ref printer p) {
    if (p.closed) {
        return default!;
    }
    p.closed = true;
    {
        var err = p.w.Flush(); if (err != default!) {
            return err;
        }
    }
    if (len(p.tags) > 0) {
        return fmt.Errorf("unclosed tag <%s>"u8, p.tags[len(p.tags) - 1].Local);
    }
    return default!;
}

// return the bufio Writer's cached write error
[GoRecv] internal static error cachedWriteError(this ref printer p) {
    var (_, err) = p.Write(default!);
    return err;
}

[GoRecv] internal static void writeIndent(this ref printer p, nint depthDelta) {
    if (len(p.prefix) == 0 && len(p.indent) == 0) {
        return;
    }
    if (depthDelta < 0) {
        p.depth--;
        if (p.indentedIn) {
            p.indentedIn = false;
            return;
        }
        p.indentedIn = false;
    }
    if (p.putNewline){
        p.WriteByte((rune)'\n');
    } else {
        p.putNewline = true;
    }
    if (len(p.prefix) > 0) {
        p.WriteString(p.prefix);
    }
    if (len(p.indent) > 0) {
        for (nint i = 0; i < p.depth; i++) {
            p.WriteString(p.indent);
        }
    }
    if (depthDelta > 0) {
        p.depth++;
        p.indentedIn = true;
    }
}

[GoType] partial struct parentStack {
    internal ж<printer> p;
    internal slice<@string> stack;
}

// trim updates the XML context to match the longest common prefix of the stack
// and the given parents. A closing tag will be written for every parent
// popped. Passing a zero slice or nil will close all the elements.
[GoRecv] internal static error trim(this ref parentStack s, slice<@string> parents) {
    nint split = 0;
    for (; split < len(parents) && split < len(s.stack); split++) {
        if (parents[split] != s.stack[split]) {
            break;
        }
    }
    for (nint i = len(s.stack) - 1; i >= split; i--) {
        {
            var err = s.p.writeEnd(new Name(Local: s.stack[i])); if (err != default!) {
                return err;
            }
        }
    }
    s.stack = s.stack.slice(0, split);
    return default!;
}

// push adds parent elements to the stack and writes open tags.
[GoRecv] internal static error push(this ref parentStack s, slice<@string> parents) {
    for (nint i = 0; i < len(parents); i++) {
        {
            var err = s.p.writeStart(Ꮡ(new StartElement(Name: new Name(Local: parents[i])))); if (err != default!) {
                return err;
            }
        }
    }
    s.stack = appendꓸꓸꓸ(s.stack, parents);
    return default!;
}

// UnsupportedTypeError is returned when [Marshal] encounters a type
// that cannot be converted into XML.
[GoType] partial struct UnsupportedTypeError {
    public reflectꓸType Type;
}

[GoRecv] public static @string Error(this ref UnsupportedTypeError e) {
    return "xml: unsupported type: "u8 + e.Type.String();
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

} // end xml_package
