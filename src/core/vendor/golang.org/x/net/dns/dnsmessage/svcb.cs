// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.net.dns;

using slices = slices_package;

partial class dnsmessage_package {

// An SVCBResource is an SVCB Resource record.
partial struct ΔSVCBResource {
    public uint16 Priority;
    public Name Target;
    public slice<SVCParam> Params; // Must be in strict increasing order by Key.
}

internal static Type realType(this ref ΔSVCBResource r) {
    return TypeSVCB;
}

// GoString implements fmt.GoStringer.GoString.
public static @string GoString(this ref ΔSVCBResource r) {
    var b = slice<byte>("dnsmessage.SVCBResource{" + "Priority: " + printUint16(r.Priority) + ", " + "Target: " + r.Target.GoString() + ", " + "Params: []dnsmessage.SVCParam{");
    if (len(r.Params) > 0) {
        b = append(b, r.Params[0].GoString().ꓸꓸꓸ);
        foreach (var (_, p) in r.Params[1..]) {
            b = append(b, ((@string)(", "u8 + p.GoString())).ꓸꓸꓸ);
        }
    }
    b = append(b, ((@string)"}}"u8).ꓸꓸꓸ);
    return ((@string)b);
}

// An HTTPSResource is an HTTPS Resource record.
// It has the same format as the SVCB record.
partial struct ΔHTTPSResource {
    // Alias for SVCB resource record.
    /*embed*/ public ΔSVCBResource SVCBResource;
}

internal static Type realType(this ref ΔHTTPSResource r) {
    return TypeHTTPS;
}

// GoString implements fmt.GoStringer.GoString.
public static @string GoString(this ref ΔHTTPSResource r) {
    return "dnsmessage.HTTPSResource{SVCBResource: "u8 + r.SVCBResource.GoString() + "}"u8;
}

// GetParam returns a parameter value by key.
public static (slice<byte> value, bool ok) GetParam(this ref ΔSVCBResource r, SVCParamKey key) {
    foreach (var (i, _) in r.Params) {
        if (r.Params[i].Key == key) {
            return (r.Params[i].Value, true);
        }
        if (r.Params[i].Key > key) {
            break;
        }
    }
    return (default!, false);
}

// SetParam sets a parameter value by key.
// The Params list is kept sorted by key.
public static void SetParam(this ref ΔSVCBResource r, SVCParamKey key, slice<byte> value) {
    nint i = 0;
    while (i < len(r.Params)) {
        if (r.Params[i].Key >= key) {
            break;
        }
        i++;
    }

    if (i < len(r.Params) && r.Params[i].Key == key) {
        r.Params[i].Value = value;
        return;
    }

    r.Params = slices.Insert(r.Params, i, new SVCParam(Key: key, Value: value));
}

// DeleteParam deletes a parameter by key.
// It returns true if the parameter was present.
public static bool DeleteParam(this ref ΔSVCBResource r, SVCParamKey key) {
    foreach (var (i, _) in r.Params) {
        if (r.Params[i].Key == key) {
            r.Params = slices.Delete<slice<SVCParam>, SVCParam>(r.Params, i, i + 1);
            return true;
        }
        if (r.Params[i].Key > key) {
            break;
        }
    }
    return false;
}

// A SVCParam is a service parameter.
partial struct SVCParam {
    public SVCParamKey Key;
    public slice<byte> Value;
}

// GoString implements fmt.GoStringer.GoString.
public static @string GoString(this SVCParam p) {
    return "dnsmessage.SVCParam{"u8 + "Key: "u8 + p.Key.GoString() + ", "u8 + "Value: []byte{"u8 + printByteSlice(p.Value) + "}}"u8;
}

// A SVCParamKey is a key for a service parameter.
partial struct SVCParamKey /*num:uint16*/;

// Values defined at https://www.iana.org/assignments/dns-svcb/dns-svcb.xhtml#dns-svcparamkeys.
public static SVCParamKey SVCParamMandatory => 0;
public static SVCParamKey SVCParamALPN => 1;
public static SVCParamKey SVCParamNoDefaultALPN => 2;
public static SVCParamKey SVCParamPort => 3;
public static SVCParamKey SVCParamIPv4Hint => 4;
public static SVCParamKey SVCParamECH => 5;
public static SVCParamKey SVCParamIPv6Hint => 6;
public static SVCParamKey SVCParamDOHPath => 7;
public static SVCParamKey SVCParamOHTTP => 8;
public static SVCParamKey SVCParamTLSSupportedGroups => 9;

internal static map<SVCParamKey, @string> svcParamKeyNames = new map<SVCParamKey, @string>{
    [SVCParamMandatory] = "Mandatory"u8,
    [SVCParamALPN] = "ALPN"u8,
    [SVCParamNoDefaultALPN] = "NoDefaultALPN"u8,
    [SVCParamPort] = "Port"u8,
    [SVCParamIPv4Hint] = "IPv4Hint"u8,
    [SVCParamECH] = "ECH"u8,
    [SVCParamIPv6Hint] = "IPv6Hint"u8,
    [SVCParamDOHPath] = "DOHPath"u8,
    [SVCParamOHTTP] = "OHTTP"u8,
    [SVCParamTLSSupportedGroups] = "TLSSupportedGroups"u8
};

// String implements fmt.Stringer.String.
public static @string String(this SVCParamKey k) {
    {
        var (n, ok) = svcParamKeyNames[k, ꟷ]; if (ok) {
            return n;
        }
    }
    return printUint16((uint16)k);
}

// GoString implements fmt.GoStringer.GoString.
public static @string GoString(this SVCParamKey k) {
    {
        var (n, ok) = svcParamKeyNames[k, ꟷ]; if (ok) {
            return "dnsmessage.SVCParam"u8 + n;
        }
    }
    return printUint16((uint16)k);
}

internal static (slice<byte>, error) pack(this ref ΔSVCBResource r, slice<byte> msg, map<@string, uint16> _Δp2, nint _Δp3) {
    var oldMsg = msg;
    msg = packUint16(msg, r.Priority);
    // https://datatracker.ietf.org/doc/html/rfc3597#section-4 prohibits name
    // compression for RR types that are not "well-known".
    // https://datatracker.ietf.org/doc/html/rfc9460#section-2.2 explicitly states that
    // compression of the Target is prohibited, following RFC 3597.
    (msg, var err) = r.Target.pack(msg, default!, 0);
    if (err != default!) {
        return (oldMsg, new nestedErrorжerror(Ꮡ(new nestedError("SVCBResource.Target"u8, err))));
    }
    SVCParamKey previousKey = default!;
    foreach (var (i, param) in r.Params) {
        if (i > 0 && param.Key <= previousKey) {
            return (oldMsg, new nestedErrorжerror(Ꮡ(new nestedError("SVCBResource.Params"u8, errParamOutOfOrder))));
        }
        if (len(param.Value) > ((1 << (int)(16))) - 1) {
            return (oldMsg, new nestedErrorжerror(Ꮡ(new nestedError("SVCBResource.Params"u8, errTooLongSVCBValue))));
        }
        msg = packUint16(msg, (uint16)param.Key);
        msg = packUint16(msg, (uint16)len(param.Value));
        msg = appendꓸꓸꓸ(msg, param.Value);
    }
    return (msg, default!);
}

internal static (ΔSVCBResource, error) unpackSVCBResource(slice<byte> msg, nint off, uint16 length) {
    // Wire format reference: https://www.rfc-editor.org/rfc/rfc9460.html#section-2.2.
    var r = new ΔSVCBResource(nil);
    nint paramsOff = off;
    nint bodyEnd = off + (nint)length;

    error err = default!;
    {
        (r.Priority, paramsOff, err) = unpackUint16(msg, paramsOff); if (err != default!) {
            return (new ΔSVCBResource(nil), new nestedErrorжerror(Ꮡ(new nestedError("Priority"u8, err))));
        }
    }

    {
        (paramsOff, err) = r.Target.unpack(msg, paramsOff); if (err != default!) {
            return (new ΔSVCBResource(nil), new nestedErrorжerror(Ꮡ(new nestedError("Target"u8, err))));
        }
    }

    // Two-pass parsing to avoid allocations.
    // First, count the number of params.
    nint n = 0;
    uint16 totalValueLen = default!;
    off = paramsOff;
    uint16 previousKey = default!;
    while (off < bodyEnd) {
        uint16 key = default!;
        uint16 size = default!;
        {
            (key, off, err) = unpackUint16(msg, off); if (err != default!) {
                return (new ΔSVCBResource(nil), new nestedErrorжerror(Ꮡ(new nestedError("Params key"u8, err))));
            }
        }
        if (n > 0 && key <= previousKey) {
            // As per https://www.rfc-editor.org/rfc/rfc9460.html#section-2.2, clients MUST
            // consider the RR malformed if the SvcParamKeys are not in strictly increasing numeric order
            return (new ΔSVCBResource(nil), new nestedErrorжerror(Ꮡ(new nestedError("Params"u8, errParamOutOfOrder))));
        }
        {
            (size, off, err) = unpackUint16(msg, off); if (err != default!) {
                return (new ΔSVCBResource(nil), new nestedErrorжerror(Ꮡ(new nestedError("Params value length"u8, err))));
            }
        }
        if (off + (nint)size > bodyEnd) {
            return (new ΔSVCBResource(nil), errResourceLen);
        }
        totalValueLen += size;
        off += (nint)size;
        n++;
    }
    if (off != bodyEnd) {
        return (new ΔSVCBResource(nil), errResourceLen);
    }

    // Second, fill in the params.
    r.Params = new slice<SVCParam>(n);
    // valuesBuf is used to hold all param values to reduce allocations.
    // Each param's Value slice will point into this buffer.
    var valuesBuf = new slice<byte>(totalValueLen);
    off = paramsOff;
    for (nint i = 0; i < n; i++) {
        var p = Ꮡ(r.Params, i);
        uint16 key = default!;
        uint16 size = default!;
        {
            (key, off, err) = unpackUint16(msg, off); if (err != default!) {
                return (new ΔSVCBResource(nil), new nestedErrorжerror(Ꮡ(new nestedError("param key"u8, err))));
            }
        }
        p.Value.Key = ((SVCParamKey)key);
        {
            (size, off, err) = unpackUint16(msg, off); if (err != default!) {
                return (new ΔSVCBResource(nil), new nestedErrorжerror(Ꮡ(new nestedError("param length"u8, err))));
            }
        }
        if (len(msg.slice(off)) < (nint)size) {
            return (new ΔSVCBResource(nil), new nestedErrorжerror(Ꮡ(new nestedError("param value"u8, errCalcLen))));
        }
        if (copy(valuesBuf, msg.slice(off).slice(0, (nint)size)) != (nint)size) {
            return (new ΔSVCBResource(nil), new nestedErrorжerror(Ꮡ(new nestedError("param value"u8, errCalcLen))));
        }
        p.Value.Value = valuesBuf.slice(0, size, size);
        valuesBuf = valuesBuf.slice(size);
        off += (nint)size;
    }

    return (r.ΔClone(), default!);
}

// genericSVCBResource parses a single Resource Record compatible with SVCB.
internal static (ΔSVCBResource, error) genericSVCBResource(this ref Parser p, Type svcbType) {
    if (!p.resHeaderValid || p.resHeaderType != svcbType) {
        return (new ΔSVCBResource(nil), ErrNotStarted);
    }
    var (r, err) = unpackSVCBResource(p.msg, p.off, p.resHeaderLength);
    if (err != default!) {
        return (new ΔSVCBResource(nil), err);
    }
    p.off += (nint)p.resHeaderLength;
    p.resHeaderValid = false;
    p.index++;
    return (r.ΔClone(), default!);
}

// SVCBResource parses a single SVCBResource.
//
// One of the XXXHeader methods must have been called before calling this
// method.
public static (ΔSVCBResource, error) SVCBResource(this ref Parser p) {
    return p.genericSVCBResource(TypeSVCB);
}

// HTTPSResource parses a single HTTPSResource.
//
// One of the XXXHeader methods must have been called before calling this
// method.
public static (ΔHTTPSResource, error) HTTPSResource(this ref Parser p) {
    var (svcb, err) = p.genericSVCBResource(TypeHTTPS);
    if (err != default!) {
        return (new ΔHTTPSResource(nil), err);
    }
    return (new ΔHTTPSResource(svcb.ΔClone()), default!);
}

// genericSVCBResource is the generic implementation for adding SVCB-like resources.
internal static error genericSVCBResource(this ж<Builder> Ꮡb, ResourceHeader h, ΔSVCBResource r) {
    h = h.ΔClone();
    r = r.ΔClone();
    ref var b = ref Ꮡb.DerefOrNull();

    {
        var errΔ1 = b.checkResourceSection(); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    var (msg, lenOff, err) = h.pack(b.msg, b.compression, b.start);
    if (err != default!) {
        return new nestedErrorжerror(Ꮡ(new nestedError("ResourceHeader"u8, err)));
    }
    nint preLen = len(msg);
    {
        (msg, err) = r.pack(msg, b.compression, b.start); if (err != default!) {
            return new nestedErrorжerror(Ꮡ(new nestedError("ResourceBody"u8, err)));
        }
    }
    {
        var errΔ2 = h.fixLen(msg, lenOff, preLen); if (errΔ2 != default!) {
            return errΔ2;
        }
    }
    {
        var errΔ3 = Ꮡb.incrementSectionCount(); if (errΔ3 != default!) {
            return errΔ3;
        }
    }
    b.msg = msg;
    return default!;
}

// SVCBResource adds a single SVCBResource.
public static error SVCBResource(this ж<Builder> Ꮡb, ResourceHeader h, ΔSVCBResource r) {
    h = h.ΔClone();
    r = r.ΔClone();

    h.Type = r.realType();
    return Ꮡb.genericSVCBResource(h, r);
}

// HTTPSResource adds a single HTTPSResource.
public static error HTTPSResource(this ж<Builder> Ꮡb, ResourceHeader h, ΔHTTPSResource r) {
    h = h.ΔClone();

    h.Type = r.realType();
    return Ꮡb.genericSVCBResource(h, r.SVCBResource);
}

} // end dnsmessage_package
