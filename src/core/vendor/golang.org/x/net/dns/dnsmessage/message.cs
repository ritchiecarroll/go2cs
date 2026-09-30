// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package dnsmessage provides a mostly RFC 1035 compliant implementation of
// DNS message packing and unpacking.
//
// The package also supports messages with Extension Mechanisms for DNS
// (EDNS(0)) as defined in RFC 6891.
//
// This implementation is designed to minimize heap allocations and avoid
// unnecessary packing and unpacking as much as possible.
namespace go.vendor.golang.org.x.net.dns;

using errors = errors_package;

partial class dnsmessage_package {

[GoType("num:uint16")] partial struct Type;

// Message formats
public static Type TypeA => 1;
public static Type TypeNS => 2;
public static Type TypeCNAME => 5;
public static Type TypeSOA => 6;
public static Type TypePTR => 12;
public static Type TypeMX => 15;
public static Type TypeTXT => 16;
public static Type TypeAAAA => 28;
public static Type TypeSRV => 33;
public static Type TypeOPT => 41;
public static Type TypeWKS => 11;
public static Type TypeHINFO => 13;
public static Type TypeMINFO => 14;
public static Type TypeAXFR => 252;
public static Type TypeALL => 255;

internal static map<Type, @string> typeNames = new map<Type, @string>{
    [TypeA] = "TypeA"u8,
    [TypeNS] = "TypeNS"u8,
    [TypeCNAME] = "TypeCNAME"u8,
    [TypeSOA] = "TypeSOA"u8,
    [TypePTR] = "TypePTR"u8,
    [TypeMX] = "TypeMX"u8,
    [TypeTXT] = "TypeTXT"u8,
    [TypeAAAA] = "TypeAAAA"u8,
    [TypeSRV] = "TypeSRV"u8,
    [TypeOPT] = "TypeOPT"u8,
    [TypeWKS] = "TypeWKS"u8,
    [TypeHINFO] = "TypeHINFO"u8,
    [TypeMINFO] = "TypeMINFO"u8,
    [TypeAXFR] = "TypeAXFR"u8,
    [TypeALL] = "TypeALL"u8
};

// String implements fmt.Stringer.String.
public static @string String(this Type t) {
    {
        var (n, ok) = typeNames[t, ꟷ]; if (ok) {
            return n;
        }
    }
    return printUint16((uint16)t);
}

// GoString implements fmt.GoStringer.GoString.
public static @string GoString(this Type t) {
    {
        var (n, ok) = typeNames[t, ꟷ]; if (ok) {
            return "dnsmessage."u8 + n;
        }
    }
    return printUint16((uint16)t);
}

[GoType("num:uint16")] partial struct Class;

public static Class ClassINET => 1;
public static Class ClassCSNET => 2;
public static Class ClassCHAOS => 3;
public static Class ClassHESIOD => 4;
public static Class ClassANY => 255;

internal static map<Class, @string> classNames = new map<Class, @string>{
    [ClassINET] = "ClassINET"u8,
    [ClassCSNET] = "ClassCSNET"u8,
    [ClassCHAOS] = "ClassCHAOS"u8,
    [ClassHESIOD] = "ClassHESIOD"u8,
    [ClassANY] = "ClassANY"u8
};

// String implements fmt.Stringer.String.
public static @string String(this Class c) {
    {
        var (n, ok) = classNames[c, ꟷ]; if (ok) {
            return n;
        }
    }
    return printUint16((uint16)c);
}

// GoString implements fmt.GoStringer.GoString.
public static @string GoString(this Class c) {
    {
        var (n, ok) = classNames[c, ꟷ]; if (ok) {
            return "dnsmessage."u8 + n;
        }
    }
    return printUint16((uint16)c);
}

[GoType("num:uint16")] partial struct OpCode;

// GoString implements fmt.GoStringer.GoString.
public static @string GoString(this OpCode o) {
    return printUint16((uint16)o);
}

[GoType("num:uint16")] partial struct RCode;

// Header.RCode values.
public static RCode RCodeSuccess => 0;      // NoError

public static RCode RCodeFormatError => 1;  // FormErr

public static RCode RCodeServerFailure => 2; // ServFail

public static RCode RCodeNameError => 3;    // NXDomain

public static RCode RCodeNotImplemented => 4; // NotImp

public static RCode RCodeRefused => 5;      // Refused

internal static map<RCode, @string> rCodeNames = new map<RCode, @string>{
    [RCodeSuccess] = "RCodeSuccess"u8,
    [RCodeFormatError] = "RCodeFormatError"u8,
    [RCodeServerFailure] = "RCodeServerFailure"u8,
    [RCodeNameError] = "RCodeNameError"u8,
    [RCodeNotImplemented] = "RCodeNotImplemented"u8,
    [RCodeRefused] = "RCodeRefused"u8
};

// String implements fmt.Stringer.String.
public static @string String(this RCode r) {
    {
        var (n, ok) = rCodeNames[r, ꟷ]; if (ok) {
            return n;
        }
    }
    return printUint16((uint16)r);
}

// GoString implements fmt.GoStringer.GoString.
public static @string GoString(this RCode r) {
    {
        var (n, ok) = rCodeNames[r, ꟷ]; if (ok) {
            return "dnsmessage."u8 + n;
        }
    }
    return printUint16((uint16)r);
}

internal static @string printPaddedUint8(uint8 i) {
    var b = (byte)i;
    return ((@string)new byte[]{
        (byte)(b / 100 + (rune)'0'),
        (byte)(b / 10 % 10 + (rune)'0'),
        (byte)(b % 10 + (rune)'0')
    }.slice());
}

internal static slice<byte> printUint8Bytes(slice<byte> buf, uint8 i) {
    var b = (byte)i;
    if (i >= 100) {
        buf = append(buf, (byte)(b / 100 + (rune)'0'));
    }
    if (i >= 10) {
        buf = append(buf, (byte)(b / 10 % 10 + (rune)'0'));
    }
    return append(buf, (byte)(b % 10 + (rune)'0'));
}

internal static @string printByteSlice(slice<byte> b) {
    if (len(b) == 0) {
        return ""u8;
    }
    var buf = new slice<byte>(0, 5 * len(b));
    buf = printUint8Bytes(buf, (uint8)b[0]);
    foreach (var (_, n) in b[1..]) {
        buf = append(buf, (byte)((rune)','), (byte)((rune)' '));
        buf = printUint8Bytes(buf, (uint8)n);
    }
    return ((@string)buf);
}

internal static readonly @string hexDigits = "0123456789abcdef"u8;

internal static @string printString(slice<byte> str) {
    var buf = new slice<byte>(0, len(str));
    for (nint i = 0; i < len(str); i++) {
        var c = str[i];
        if (c == (rune)'.' || c == (rune)'-' || c == (rune)' ' || (rune)'A' <= c && c <= (rune)'Z' || (rune)'a' <= c && c <= (rune)'z' || (rune)'0' <= c && c <= (rune)'9') {
            buf = append(buf, c);
            continue;
        }
        var upper = (byte)((c >> (int)(4)));
        var lower = (byte)((((byte)(c << (int)(4))) >> (int)(4)));
        buf = append(
            buf,
            (byte)((rune)'\\'),
            (byte)((rune)'x'),
            hexDigits[upper],
            hexDigits[lower]);
    }
    return ((@string)buf);
}

internal static @string printUint16(uint16 i) {
    return printUint32((uint32)i);
}

internal static @string printUint32(uint32 i) {
    // Max value is 4294967295.
    var buf = new slice<byte>(10);
    for (var (b, d) = (buf, (uint32)1000000000); d > 0; d /= 10) {
        b[0] = (byte)(i / d % 10 + (rune)'0');
        if (b[0] == (rune)'0' && len(b) == len(buf) && len(buf) > 1) {
            buf = buf[1..];
        }
        b = b[1..];
        i %= d;
    }
    return ((@string)buf);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string trueˢ = "true"u8;
private static readonly @string falseˢ = "false"u8;

internal static @string printBool(bool b) {
    if (b) {
        return trueˢ;
    }
    return falseˢ;
}

public static error ErrNotStarted = errors.New("parsing/packing of this type isn't available yet"u8);
public static error ErrSectionDone = errors.New("parsing/packing of this section has completed"u8);
internal static error errBaseLen = errors.New("insufficient data for base length type"u8);
internal static error errCalcLen = errors.New("insufficient data for calculated length type"u8);
internal static error errReserved = errors.New("segment prefix is reserved"u8);
internal static error errTooManyPtr = errors.New("too many pointers (>10)"u8);
internal static error errInvalidPtr = errors.New("invalid pointer"u8);
internal static error errInvalidName = errors.New("invalid dns name"u8);
internal static error errNilResouceBody = errors.New("nil resource body"u8);
internal static error errResourceLen = errors.New("insufficient data for resource body length"u8);
internal static error errSegTooLong = errors.New("segment length too long"u8);
internal static error errNameTooLong = errors.New("name too long"u8);
internal static error errZeroSegLen = errors.New("zero length segment"u8);
internal static error errResTooLong = errors.New("resource length too long"u8);
internal static error errTooManyQuestions = errors.New("too many Questions to pack (>65535)"u8);
internal static error errTooManyAnswers = errors.New("too many Answers to pack (>65535)"u8);
internal static error errTooManyAuthorities = errors.New("too many Authorities to pack (>65535)"u8);
internal static error errTooManyAdditionals = errors.New("too many Additionals to pack (>65535)"u8);
internal static error errNonCanonicalName = errors.New("name is not in canonical format (it must end with a .)"u8);
internal static error errStringTooLong = errors.New("character string exceeds maximum length (255)"u8);

// Internal constants.
internal static UntypedInt packStartingCap => 512;

internal static UntypedInt uint16Len => 2;

internal static UntypedInt uint32Len => 4;

internal static UntypedInt headerLen => /* 6 * uint16Len */ 12;

[GoType] partial struct nestedError {
    // s is the current level's error message.
    internal @string s;
    // err is the nested error.
    internal error err;
}

// nestedError implements error.Error.
[GoRecv] internal static @string Error(this ref nestedError e) {
    return e.s + ": "u8 + e.err.Error();
}

// Header is a representation of a DNS message header.
[GoType] partial struct Header {
    public uint16 ID;
    public bool Response;
    public OpCode OpCode;
    public bool Authoritative;
    public bool Truncated;
    public bool RecursionDesired;
    public bool RecursionAvailable;
    public bool AuthenticData;
    public bool CheckingDisabled;
    public RCode RCode;
}

[GoRecv] internal static (uint16 id, uint16 bits) pack(this ref Header m) {
    uint16 id = default!;
    uint16 bits = default!;

    id = m.ID;
    bits = (uint16)((uint16)((uint16)m.OpCode << (int)(11)) | (uint16)m.RCode);
    if (m.RecursionAvailable) {
        bits |= (uint16)(headerBitRA);
    }
    if (m.RecursionDesired) {
        bits |= (uint16)(headerBitRD);
    }
    if (m.Truncated) {
        bits |= (uint16)(headerBitTC);
    }
    if (m.Authoritative) {
        bits |= (uint16)(headerBitAA);
    }
    if (m.Response) {
        bits |= (uint16)(headerBitQR);
    }
    if (m.AuthenticData) {
        bits |= (uint16)(headerBitAD);
    }
    if (m.CheckingDisabled) {
        bits |= (uint16)(headerBitCD);
    }
    return (id, bits);
}

// GoString implements fmt.GoStringer.GoString.
[GoRecv] public static @string GoString(this ref Header m) {
    return "dnsmessage.Header{"u8 + "ID: "u8 + printUint16(m.ID) + ", "u8 + "Response: "u8 + printBool(m.Response) + ", "u8 + "OpCode: "u8 + m.OpCode.GoString() + ", "u8 + "Authoritative: "u8 + printBool(m.Authoritative) + ", "u8 + "Truncated: "u8 + printBool(m.Truncated) + ", "u8 + "RecursionDesired: "u8 + printBool(m.RecursionDesired) + ", "u8 + "RecursionAvailable: "u8 + printBool(m.RecursionAvailable) + ", "u8 + "AuthenticData: "u8 + printBool(m.AuthenticData) + ", "u8 + "CheckingDisabled: "u8 + printBool(m.CheckingDisabled) + ", "u8 + "RCode: "u8 + m.RCode.GoString() + "}"u8;
}

// Message is a representation of a DNS message.
[GoType] partial struct Message {
    public partial ref Header Header { get; }
    public slice<ΔQuestion> Questions;
    public slice<Resource> Answers;
    public slice<Resource> Authorities;
    public slice<Resource> Additionals;
}

[GoType("num:uint8")] partial struct section;

internal static section sectionNotStarted => /* iota */ 0;
internal static section sectionHeader => 1;
internal static section sectionQuestions => 2;
internal static section sectionAnswers => 3;
internal static section sectionAuthorities => 4;
internal static section sectionAdditionals => 5;
internal static section sectionDone => 6;
internal static UntypedInt headerBitQR => /* 1 << 15 */ 32768; // query/response (response=1)
internal static UntypedInt headerBitAA => /* 1 << 10 */ 1024; // authoritative
internal static UntypedInt headerBitTC => /* 1 << 9 */ 512; // truncated
internal static UntypedInt headerBitRD => /* 1 << 8 */ 256; // recursion desired
internal static UntypedInt headerBitRA => /* 1 << 7 */ 128; // recursion available
internal static UntypedInt headerBitAD => /* 1 << 5 */ 32; // authentic data
internal static UntypedInt headerBitCD => /* 1 << 4 */ 16; // checking disabled

internal static map<section, @string> sectionNames = new map<section, @string>{
    [sectionHeader] = "header"u8,
    [sectionQuestions] = "Question"u8,
    [sectionAnswers] = "Answer"u8,
    [sectionAuthorities] = "Authority"u8,
    [sectionAdditionals] = "Additional"u8
};

// header is the wire format for a DNS message header.
[GoType] partial struct Δheader {
    internal uint16 id;
    internal uint16 bits;
    internal uint16 questions;
    internal uint16 answers;
    internal uint16 authorities;
    internal uint16 additionals;
}

[GoRecv] internal static uint16 count(this ref Δheader h, section sec) {
    var exprᴛ1 = sec;
    if (exprᴛ1 == sectionQuestions) {
        return h.questions;
    }
    if (exprᴛ1 == sectionAnswers) {
        return h.answers;
    }
    if (exprᴛ1 == sectionAuthorities) {
        return h.authorities;
    }
    if (exprᴛ1 == sectionAdditionals) {
        return h.additionals;
    }

    return 0;
}

// pack appends the wire format of the header to msg.
[GoRecv] internal static slice<byte> pack(this ref Δheader h, slice<byte> msg) {
    msg = packUint16(msg, h.id);
    msg = packUint16(msg, h.bits);
    msg = packUint16(msg, h.questions);
    msg = packUint16(msg, h.answers);
    msg = packUint16(msg, h.authorities);
    return packUint16(msg, h.additionals);
}

[GoRecv] internal static (nint, error) unpack(this ref Δheader h, slice<byte> msg, nint off) {
    nint newOff = off;
    error err = default!;
    {
        (h.id, newOff, err) = unpackUint16(msg, newOff); if (err != default!) {
            return (off, new nestedErrorжerror(Ꮡ(new nestedError("id"u8, err))));
        }
    }
    {
        (h.bits, newOff, err) = unpackUint16(msg, newOff); if (err != default!) {
            return (off, new nestedErrorжerror(Ꮡ(new nestedError("bits"u8, err))));
        }
    }
    {
        (h.questions, newOff, err) = unpackUint16(msg, newOff); if (err != default!) {
            return (off, new nestedErrorжerror(Ꮡ(new nestedError("questions"u8, err))));
        }
    }
    {
        (h.answers, newOff, err) = unpackUint16(msg, newOff); if (err != default!) {
            return (off, new nestedErrorжerror(Ꮡ(new nestedError("answers"u8, err))));
        }
    }
    {
        (h.authorities, newOff, err) = unpackUint16(msg, newOff); if (err != default!) {
            return (off, new nestedErrorжerror(Ꮡ(new nestedError("authorities"u8, err))));
        }
    }
    {
        (h.additionals, newOff, err) = unpackUint16(msg, newOff); if (err != default!) {
            return (off, new nestedErrorжerror(Ꮡ(new nestedError("additionals"u8, err))));
        }
    }
    return (newOff, default!);
}

[GoRecv] internal static Header header(this ref Δheader h) {
    return new Header(
        ID: h.id,
        Response: ((uint16)(h.bits & (uint16)headerBitQR)) != 0,
        OpCode: (OpCode)(((OpCode)((h.bits >> (int)(11)))) & 0xF),
        Authoritative: ((uint16)(h.bits & (uint16)headerBitAA)) != 0,
        Truncated: ((uint16)(h.bits & (uint16)headerBitTC)) != 0,
        RecursionDesired: ((uint16)(h.bits & (uint16)headerBitRD)) != 0,
        RecursionAvailable: ((uint16)(h.bits & (uint16)headerBitRA)) != 0,
        AuthenticData: ((uint16)(h.bits & (uint16)headerBitAD)) != 0,
        CheckingDisabled: ((uint16)(h.bits & (uint16)headerBitCD)) != 0,
        RCode: ((RCode)((uint16)(h.bits & 0xF)))
    );
}

// A Resource is a DNS resource record.
[GoType] partial struct Resource {
    public ResourceHeader Header;
    public ResourceBody Body;
}

[GoRecv] public static @string GoString(this ref Resource r) {
    return "dnsmessage.Resource{"u8 + "Header: "u8 + r.Header.GoString() + ", Body: &"u8 + r.Body.GoString() + "}"u8;
}

// A ResourceBody is a DNS resource record minus the header.
[GoType] partial interface ResourceBody {
    // pack packs a Resource except for its header.
    (slice<byte>, error) pack(slice<byte> msg, map<@string, uint16> compression, nint compressionOff);
    // realType returns the actual type of the Resource. This is used to
    // fill in the header Type field.
    Type realType();
    // GoString implements fmt.GoStringer.GoString.
    @string GoString();
}

// pack appends the wire format of the Resource to msg.
[GoRecv] internal static (slice<byte>, error) pack(this ref Resource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) {
    if (r.Body == default!) {
        return (msg, errNilResouceBody);
    }
    var oldMsg = msg;
    r.Header.Type = r.Body.realType();
    (msg, var lenOff, var err) = r.Header.pack(msg, compression, compressionOff);
    if (err != default!) {
        return (msg, new nestedErrorжerror(Ꮡ(new nestedError("ResourceHeader"u8, err))));
    }
    nint preLen = len(msg);
    (msg, err) = r.Body.pack(msg, compression, compressionOff);
    if (err != default!) {
        return (msg, new nestedErrorжerror(Ꮡ(new nestedError("content"u8, err))));
    }
    {
        var errΔ1 = r.Header.fixLen(msg, lenOff, preLen); if (errΔ1 != default!) {
            return (oldMsg, errΔ1);
        }
    }
    return (msg, default!);
}

// A Parser allows incrementally parsing a DNS message.
//
// When parsing is started, the Header is parsed. Next, each Question can be
// either parsed or skipped. Alternatively, all Questions can be skipped at
// once. When all Questions have been parsed, attempting to parse Questions
// will return the [ErrSectionDone] error.
// After all Questions have been either parsed or skipped, all
// Answers, Authorities and Additionals can be either parsed or skipped in the
// same way, and each type of Resource must be fully parsed or skipped before
// proceeding to the next type of Resource.
//
// Parser is safe to copy to preserve the parsing state.
//
// Note that there is no requirement to fully skip or parse the message.
[GoType] partial struct Parser {
    internal slice<byte> msg;
    internal Δheader header;
    internal section section;
    internal nint off;
    internal nint index;
    internal bool resHeaderValid;
    internal nint resHeaderOffset;
    internal Type resHeaderType;
    internal uint16 resHeaderLength;
}

// Start parses the header and enables the parsing of Questions.
[GoRecv] public static (Header, error) Start(this ref Parser p, slice<byte> msg) {
    if (p.msg != default!) {
        p = new Parser(nil);
    }
    p.msg = msg;
    error err = default!;
    {
        (p.off, err) = p.header.unpack(msg, 0); if (err != default!) {
            return (new Header(nil), new nestedErrorжerror(Ꮡ(new nestedError("unpacking header"u8, err))));
        }
    }
    p.section = sectionQuestions;
    return (p.header.header(), default!);
}

[GoRecv] internal static error checkAdvance(this ref Parser p, section sec) {
    if (p.section < sec) {
        return ErrNotStarted;
    }
    if (p.section > sec) {
        return ErrSectionDone;
    }
    p.resHeaderValid = false;
    if (p.index == (nint)p.header.count(sec)) {
        p.index = 0;
        p.section++;
        return ErrSectionDone;
    }
    return default!;
}

[GoRecv] internal static (Resource, error) resource(this ref Parser p, section sec) {
    Resource r = new();
    error err = default!;
    (r.Header, err) = p.resourceHeader(sec);
    if (err != default!) {
        return (r.ΔClone(), err);
    }
    p.resHeaderValid = false;
    (r.Body, p.off, err) = unpackResourceBody(p.msg, p.off, r.Header);
    if (err != default!) {
        return (new Resource(nil), new nestedErrorжerror(Ꮡ(new nestedError("unpacking " + sectionNames[sec], err))));
    }
    p.index++;
    return (r.ΔClone(), default!);
}

[GoRecv] internal static (ResourceHeader, error) resourceHeader(this ref Parser p, section sec) {
    if (p.resHeaderValid) {
        p.off = p.resHeaderOffset;
    }
    {
        var errΔ1 = p.checkAdvance(sec); if (errΔ1 != default!) {
            return (new ResourceHeader(nil), errΔ1);
        }
    }
    ResourceHeader hdr = new();
    var (off, err) = hdr.unpack(p.msg, p.off);
    if (err != default!) {
        return (new ResourceHeader(nil), err);
    }
    p.resHeaderValid = true;
    p.resHeaderOffset = p.off;
    p.resHeaderType = hdr.Type;
    p.resHeaderLength = hdr.Length;
    p.off = off;
    return (hdr.ΔClone(), default!);
}

[GoRecv] internal static error skipResource(this ref Parser p, section sec) {
    if (p.resHeaderValid && p.section == sec) {
        nint newOff = p.off + (nint)p.resHeaderLength;
        if (newOff > len(p.msg)) {
            return errResourceLen;
        }
        p.off = newOff;
        p.resHeaderValid = false;
        p.index++;
        return default!;
    }
    {
        var errΔ1 = p.checkAdvance(sec); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    error err = default!;
    (p.off, err) = skipResource(p.msg, p.off);
    if (err != default!) {
        return new nestedErrorжerror(Ꮡ(new nestedError("skipping: " + sectionNames[sec], err)));
    }
    p.index++;
    return default!;
}

// Question parses a single Question.
[GoRecv] public static (ΔQuestion, error) Question(this ref Parser p) {
    {
        var errΔ1 = p.checkAdvance(sectionQuestions); if (errΔ1 != default!) {
            return (new ΔQuestion(nil), errΔ1);
        }
    }
    Name name = new();
    var (off, err) = name.unpack(p.msg, p.off);
    if (err != default!) {
        return (new ΔQuestion(nil), new nestedErrorжerror(Ꮡ(new nestedError("unpacking Question.Name"u8, err))));
    }
    (var typ, off, err) = unpackType(p.msg, off);
    if (err != default!) {
        return (new ΔQuestion(nil), new nestedErrorжerror(Ꮡ(new nestedError("unpacking Question.Type"u8, err))));
    }
    (var @class, off, err) = unpackClass(p.msg, off);
    if (err != default!) {
        return (new ΔQuestion(nil), new nestedErrorжerror(Ꮡ(new nestedError("unpacking Question.Class"u8, err))));
    }
    p.off = off;
    p.index++;
    return (new ΔQuestion(name.ΔClone(), typ, @class), default!);
}

// AllQuestions parses all Questions.
[GoRecv] public static (slice<ΔQuestion>, error) AllQuestions(this ref Parser p) {
    // Multiple questions are valid according to the spec,
    // but servers don't actually support them. There will
    // be at most one question here.
    //
    // Do not pre-allocate based on info in p.header, since
    // the data is untrusted.
    var qs = new ΔQuestion[]{}.slice();
    while (ᐧ) {
        var (q, err) = p.Question();
        if (AreEqual(err, ErrSectionDone)) {
            return (qs, default!);
        }
        if (err != default!) {
            return (default!, err);
        }
        qs = append(qs, q.ΔClone());
    }
}

// SkipQuestion skips a single Question.
[GoRecv] public static error SkipQuestion(this ref Parser p) {
    {
        var errΔ1 = p.checkAdvance(sectionQuestions); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    var (off, err) = skipName(p.msg, p.off);
    if (err != default!) {
        return new nestedErrorжerror(Ꮡ(new nestedError("skipping Question Name"u8, err)));
    }
    {
        (off, err) = skipType(p.msg, off); if (err != default!) {
            return new nestedErrorжerror(Ꮡ(new nestedError("skipping Question Type"u8, err)));
        }
    }
    {
        (off, err) = skipClass(p.msg, off); if (err != default!) {
            return new nestedErrorжerror(Ꮡ(new nestedError("skipping Question Class"u8, err)));
        }
    }
    p.off = off;
    p.index++;
    return default!;
}

// SkipAllQuestions skips all Questions.
[GoRecv] public static error SkipAllQuestions(this ref Parser p) {
    while (ᐧ) {
        {
            var err = p.SkipQuestion(); if (AreEqual(err, ErrSectionDone)){
                return default!;
            } else 
            if (err != default!) {
                return err;
            }
        }
    }
}

// AnswerHeader parses a single Answer ResourceHeader.
[GoRecv] public static (ResourceHeader, error) AnswerHeader(this ref Parser p) {
    return p.resourceHeader(sectionAnswers);
}

// Answer parses a single Answer Resource.
[GoRecv] public static (Resource, error) Answer(this ref Parser p) {
    return p.resource(sectionAnswers);
}

// AllAnswers parses all Answer Resources.
[GoRecv] public static (slice<Resource>, error) AllAnswers(this ref Parser p) {
    // The most common query is for A/AAAA, which usually returns
    // a handful of IPs.
    //
    // Pre-allocate up to a certain limit, since p.header is
    // untrusted data.
    nint n = (nint)p.header.answers;
    if (n > 20) {
        n = 20;
    }
    var @as = new slice<Resource>(0, () => new(), n);
    while (ᐧ) {
        var (a, err) = p.Answer();
        if (AreEqual(err, ErrSectionDone)) {
            return (@as, default!);
        }
        if (err != default!) {
            return (default!, err);
        }
        @as = append(@as, a.ΔClone());
    }
}

// SkipAnswer skips a single Answer Resource.
//
// It does not perform a complete validation of the resource header, which means
// it may return a nil error when the [AnswerHeader] would actually return an error.
[GoRecv] public static error SkipAnswer(this ref Parser p) {
    return p.skipResource(sectionAnswers);
}

// SkipAllAnswers skips all Answer Resources.
[GoRecv] public static error SkipAllAnswers(this ref Parser p) {
    while (ᐧ) {
        {
            var err = p.SkipAnswer(); if (AreEqual(err, ErrSectionDone)){
                return default!;
            } else 
            if (err != default!) {
                return err;
            }
        }
    }
}

// AuthorityHeader parses a single Authority ResourceHeader.
[GoRecv] public static (ResourceHeader, error) AuthorityHeader(this ref Parser p) {
    return p.resourceHeader(sectionAuthorities);
}

// Authority parses a single Authority Resource.
[GoRecv] public static (Resource, error) Authority(this ref Parser p) {
    return p.resource(sectionAuthorities);
}

// AllAuthorities parses all Authority Resources.
[GoRecv] public static (slice<Resource>, error) AllAuthorities(this ref Parser p) {
    // Authorities contains SOA in case of NXDOMAIN and friends,
    // otherwise it is empty.
    //
    // Pre-allocate up to a certain limit, since p.header is
    // untrusted data.
    nint n = (nint)p.header.authorities;
    if (n > 10) {
        n = 10;
    }
    var @as = new slice<Resource>(0, () => new(), n);
    while (ᐧ) {
        var (a, err) = p.Authority();
        if (AreEqual(err, ErrSectionDone)) {
            return (@as, default!);
        }
        if (err != default!) {
            return (default!, err);
        }
        @as = append(@as, a.ΔClone());
    }
}

// SkipAuthority skips a single Authority Resource.
//
// It does not perform a complete validation of the resource header, which means
// it may return a nil error when the [AuthorityHeader] would actually return an error.
[GoRecv] public static error SkipAuthority(this ref Parser p) {
    return p.skipResource(sectionAuthorities);
}

// SkipAllAuthorities skips all Authority Resources.
[GoRecv] public static error SkipAllAuthorities(this ref Parser p) {
    while (ᐧ) {
        {
            var err = p.SkipAuthority(); if (AreEqual(err, ErrSectionDone)){
                return default!;
            } else 
            if (err != default!) {
                return err;
            }
        }
    }
}

// AdditionalHeader parses a single Additional ResourceHeader.
[GoRecv] public static (ResourceHeader, error) AdditionalHeader(this ref Parser p) {
    return p.resourceHeader(sectionAdditionals);
}

// Additional parses a single Additional Resource.
[GoRecv] public static (Resource, error) Additional(this ref Parser p) {
    return p.resource(sectionAdditionals);
}

// AllAdditionals parses all Additional Resources.
[GoRecv] public static (slice<Resource>, error) AllAdditionals(this ref Parser p) {
    // Additionals usually contain OPT, and sometimes A/AAAA
    // glue records.
    //
    // Pre-allocate up to a certain limit, since p.header is
    // untrusted data.
    nint n = (nint)p.header.additionals;
    if (n > 10) {
        n = 10;
    }
    var @as = new slice<Resource>(0, () => new(), n);
    while (ᐧ) {
        var (a, err) = p.Additional();
        if (AreEqual(err, ErrSectionDone)) {
            return (@as, default!);
        }
        if (err != default!) {
            return (default!, err);
        }
        @as = append(@as, a.ΔClone());
    }
}

// SkipAdditional skips a single Additional Resource.
//
// It does not perform a complete validation of the resource header, which means
// it may return a nil error when the [AdditionalHeader] would actually return an error.
[GoRecv] public static error SkipAdditional(this ref Parser p) {
    return p.skipResource(sectionAdditionals);
}

// SkipAllAdditionals skips all Additional Resources.
[GoRecv] public static error SkipAllAdditionals(this ref Parser p) {
    while (ᐧ) {
        {
            var err = p.SkipAdditional(); if (AreEqual(err, ErrSectionDone)){
                return default!;
            } else 
            if (err != default!) {
                return err;
            }
        }
    }
}

// CNAMEResource parses a single CNAMEResource.
//
// One of the XXXHeader methods must have been called before calling this
// method.
[GoRecv] public static (ΔCNAMEResource, error) CNAMEResource(this ref Parser p) {
    if (!p.resHeaderValid || p.resHeaderType != TypeCNAME) {
        return (new ΔCNAMEResource(nil), ErrNotStarted);
    }
    var (r, err) = unpackCNAMEResource(p.msg, p.off);
    if (err != default!) {
        return (new ΔCNAMEResource(nil), err);
    }
    p.off += (nint)p.resHeaderLength;
    p.resHeaderValid = false;
    p.index++;
    return (r.ΔClone(), default!);
}

// MXResource parses a single MXResource.
//
// One of the XXXHeader methods must have been called before calling this
// method.
[GoRecv] public static (ΔMXResource, error) MXResource(this ref Parser p) {
    if (!p.resHeaderValid || p.resHeaderType != TypeMX) {
        return (new ΔMXResource(nil), ErrNotStarted);
    }
    var (r, err) = unpackMXResource(p.msg, p.off);
    if (err != default!) {
        return (new ΔMXResource(nil), err);
    }
    p.off += (nint)p.resHeaderLength;
    p.resHeaderValid = false;
    p.index++;
    return (r.ΔClone(), default!);
}

// NSResource parses a single NSResource.
//
// One of the XXXHeader methods must have been called before calling this
// method.
[GoRecv] public static (ΔNSResource, error) NSResource(this ref Parser p) {
    if (!p.resHeaderValid || p.resHeaderType != TypeNS) {
        return (new ΔNSResource(nil), ErrNotStarted);
    }
    var (r, err) = unpackNSResource(p.msg, p.off);
    if (err != default!) {
        return (new ΔNSResource(nil), err);
    }
    p.off += (nint)p.resHeaderLength;
    p.resHeaderValid = false;
    p.index++;
    return (r.ΔClone(), default!);
}

// PTRResource parses a single PTRResource.
//
// One of the XXXHeader methods must have been called before calling this
// method.
[GoRecv] public static (ΔPTRResource, error) PTRResource(this ref Parser p) {
    if (!p.resHeaderValid || p.resHeaderType != TypePTR) {
        return (new ΔPTRResource(nil), ErrNotStarted);
    }
    var (r, err) = unpackPTRResource(p.msg, p.off);
    if (err != default!) {
        return (new ΔPTRResource(nil), err);
    }
    p.off += (nint)p.resHeaderLength;
    p.resHeaderValid = false;
    p.index++;
    return (r.ΔClone(), default!);
}

// SOAResource parses a single SOAResource.
//
// One of the XXXHeader methods must have been called before calling this
// method.
[GoRecv] public static (ΔSOAResource, error) SOAResource(this ref Parser p) {
    if (!p.resHeaderValid || p.resHeaderType != TypeSOA) {
        return (new ΔSOAResource(nil), ErrNotStarted);
    }
    var (r, err) = unpackSOAResource(p.msg, p.off);
    if (err != default!) {
        return (new ΔSOAResource(nil), err);
    }
    p.off += (nint)p.resHeaderLength;
    p.resHeaderValid = false;
    p.index++;
    return (r.ΔClone(), default!);
}

// TXTResource parses a single TXTResource.
//
// One of the XXXHeader methods must have been called before calling this
// method.
[GoRecv] public static (ΔTXTResource, error) TXTResource(this ref Parser p) {
    if (!p.resHeaderValid || p.resHeaderType != TypeTXT) {
        return (new ΔTXTResource(nil), ErrNotStarted);
    }
    var (r, err) = unpackTXTResource(p.msg, p.off, p.resHeaderLength);
    if (err != default!) {
        return (new ΔTXTResource(nil), err);
    }
    p.off += (nint)p.resHeaderLength;
    p.resHeaderValid = false;
    p.index++;
    return (r, default!);
}

// SRVResource parses a single SRVResource.
//
// One of the XXXHeader methods must have been called before calling this
// method.
[GoRecv] public static (ΔSRVResource, error) SRVResource(this ref Parser p) {
    if (!p.resHeaderValid || p.resHeaderType != TypeSRV) {
        return (new ΔSRVResource(nil), ErrNotStarted);
    }
    var (r, err) = unpackSRVResource(p.msg, p.off);
    if (err != default!) {
        return (new ΔSRVResource(nil), err);
    }
    p.off += (nint)p.resHeaderLength;
    p.resHeaderValid = false;
    p.index++;
    return (r.ΔClone(), default!);
}

// AResource parses a single AResource.
//
// One of the XXXHeader methods must have been called before calling this
// method.
[GoRecv] public static (ΔAResource, error) AResource(this ref Parser p) {
    if (!p.resHeaderValid || p.resHeaderType != TypeA) {
        return (new ΔAResource(nil), ErrNotStarted);
    }
    var (r, err) = unpackAResource(p.msg, p.off);
    if (err != default!) {
        return (new ΔAResource(nil), err);
    }
    p.off += (nint)p.resHeaderLength;
    p.resHeaderValid = false;
    p.index++;
    return (r.ΔClone(), default!);
}

// AAAAResource parses a single AAAAResource.
//
// One of the XXXHeader methods must have been called before calling this
// method.
[GoRecv] public static (ΔAAAAResource, error) AAAAResource(this ref Parser p) {
    if (!p.resHeaderValid || p.resHeaderType != TypeAAAA) {
        return (new ΔAAAAResource(nil), ErrNotStarted);
    }
    var (r, err) = unpackAAAAResource(p.msg, p.off);
    if (err != default!) {
        return (new ΔAAAAResource(nil), err);
    }
    p.off += (nint)p.resHeaderLength;
    p.resHeaderValid = false;
    p.index++;
    return (r.ΔClone(), default!);
}

// OPTResource parses a single OPTResource.
//
// One of the XXXHeader methods must have been called before calling this
// method.
[GoRecv] public static (ΔOPTResource, error) OPTResource(this ref Parser p) {
    if (!p.resHeaderValid || p.resHeaderType != TypeOPT) {
        return (new ΔOPTResource(nil), ErrNotStarted);
    }
    var (r, err) = unpackOPTResource(p.msg, p.off, p.resHeaderLength);
    if (err != default!) {
        return (new ΔOPTResource(nil), err);
    }
    p.off += (nint)p.resHeaderLength;
    p.resHeaderValid = false;
    p.index++;
    return (r, default!);
}

// UnknownResource parses a single UnknownResource.
//
// One of the XXXHeader methods must have been called before calling this
// method.
[GoRecv] public static (ΔUnknownResource, error) UnknownResource(this ref Parser p) {
    if (!p.resHeaderValid) {
        return (new ΔUnknownResource(nil), ErrNotStarted);
    }
    var (r, err) = unpackUnknownResource(p.resHeaderType, p.msg, p.off, p.resHeaderLength);
    if (err != default!) {
        return (new ΔUnknownResource(nil), err);
    }
    p.off += (nint)p.resHeaderLength;
    p.resHeaderValid = false;
    p.index++;
    return (r, default!);
}

// Unpack parses a full Message.
[GoRecv] public static error Unpack(this ref Message m, slice<byte> msg) {
    Parser p = default!;
    error err = default!;
    {
        (m.Header, err) = p.Start(msg); if (err != default!) {
            return err;
        }
    }
    {
        (m.Questions, err) = p.AllQuestions(); if (err != default!) {
            return err;
        }
    }
    {
        (m.Answers, err) = p.AllAnswers(); if (err != default!) {
            return err;
        }
    }
    {
        (m.Authorities, err) = p.AllAuthorities(); if (err != default!) {
            return err;
        }
    }
    {
        (m.Additionals, err) = p.AllAdditionals(); if (err != default!) {
            return err;
        }
    }
    return default!;
}

// Pack packs a full Message.
[GoRecv] public static (slice<byte>, error) Pack(this ref Message m) {
    return m.AppendPack(new slice<byte>(0, packStartingCap));
}

// AppendPack is like Pack but appends the full Message to b and returns the
// extended buffer.
[GoRecv] public static (slice<byte>, error) AppendPack(this ref Message m, slice<byte> b) {
    // Validate the lengths. It is very unlikely that anyone will try to
    // pack more than 65535 of any particular type, but it is possible and
    // we should fail gracefully.
    if (len(m.Questions) > (nint)(unchecked((uint16)(~(uint16)0)))) {
        return (default!, errTooManyQuestions);
    }
    if (len(m.Answers) > (nint)(unchecked((uint16)(~(uint16)0)))) {
        return (default!, errTooManyAnswers);
    }
    if (len(m.Authorities) > (nint)(unchecked((uint16)(~(uint16)0)))) {
        return (default!, errTooManyAuthorities);
    }
    if (len(m.Additionals) > (nint)(unchecked((uint16)(~(uint16)0)))) {
        return (default!, errTooManyAdditionals);
    }
    Δheader h = default!;
    (h.id, h.bits) = m.Header.pack();
    h.questions = (uint16)len(m.Questions);
    h.answers = (uint16)len(m.Answers);
    h.authorities = (uint16)len(m.Authorities);
    h.additionals = (uint16)len(m.Additionals);
    nint compressionOff = len(b);
    var msg = h.pack(b);
    // RFC 1035 allows (but does not require) compression for packing. RFC
    // 1035 requires unpacking implementations to support compression, so
    // unconditionally enabling it is fine.
    //
    // DNS lookups are typically done over UDP, and RFC 1035 states that UDP
    // DNS messages can be a maximum of 512 bytes long. Without compression,
    // many DNS response messages are over this limit, so enabling
    // compression will help ensure compliance.
    var compression = new map<@string, uint16>{};
    foreach (var (i, _) in m.Questions) {
        error err = default!;
        {
            (msg, err) = m.Questions[i].pack(msg, compression, compressionOff); if (err != default!) {
                return (default!, new nestedErrorжerror(Ꮡ(new nestedError("packing Question"u8, err))));
            }
        }
    }
    foreach (var (i, _) in m.Answers) {
        error err = default!;
        {
            (msg, err) = m.Answers[i].pack(msg, compression, compressionOff); if (err != default!) {
                return (default!, new nestedErrorжerror(Ꮡ(new nestedError("packing Answer"u8, err))));
            }
        }
    }
    foreach (var (i, _) in m.Authorities) {
        error err = default!;
        {
            (msg, err) = m.Authorities[i].pack(msg, compression, compressionOff); if (err != default!) {
                return (default!, new nestedErrorжerror(Ꮡ(new nestedError("packing Authority"u8, err))));
            }
        }
    }
    foreach (var (i, _) in m.Additionals) {
        error err = default!;
        {
            (msg, err) = m.Additionals[i].pack(msg, compression, compressionOff); if (err != default!) {
                return (default!, new nestedErrorжerror(Ꮡ(new nestedError("packing Additional"u8, err))));
            }
        }
    }
    return (msg, default!);
}

// GoString implements fmt.GoStringer.GoString.
[GoRecv] public static @string GoString(this ref Message m) {
    @string s = "dnsmessage.Message{Header: "u8 + m.Header.GoString() + ", "u8 + "Questions: []dnsmessage.Question{"u8;
    if (len(m.Questions) > 0) {
        s += m.Questions[0].GoString();
        foreach (var (_, vᴛ1) in m.Questions[1..]) {
            var q = vᴛ1.ΔClone();

            s += ", "u8 + q.GoString();
        }
    }
    s += "}, Answers: []dnsmessage.Resource{"u8;
    if (len(m.Answers) > 0) {
        s += m.Answers[0].GoString();
        foreach (var (_, vᴛ2) in m.Answers[1..]) {
            var a = vᴛ2.ΔClone();

            s += ", "u8 + a.GoString();
        }
    }
    s += "}, Authorities: []dnsmessage.Resource{"u8;
    if (len(m.Authorities) > 0) {
        s += m.Authorities[0].GoString();
        foreach (var (_, vᴛ3) in m.Authorities[1..]) {
            var a = vᴛ3.ΔClone();

            s += ", "u8 + a.GoString();
        }
    }
    s += "}, Additionals: []dnsmessage.Resource{"u8;
    if (len(m.Additionals) > 0) {
        s += m.Additionals[0].GoString();
        foreach (var (_, vᴛ4) in m.Additionals[1..]) {
            var a = vᴛ4.ΔClone();

            s += ", "u8 + a.GoString();
        }
    }
    return s + "}}"u8;
}

// A Builder allows incrementally packing a DNS message.
//
// Example usage:
//
//	buf := make([]byte, 2, 514)
//	b := NewBuilder(buf, Header{...})
//	b.EnableCompression()
//	// Optionally start a section and add things to that section.
//	// Repeat adding sections as necessary.
//	buf, err := b.Finish()
//	// If err is nil, buf[2:] will contain the built bytes.
[GoType] partial struct Builder {
    // msg is the storage for the message being built.
    internal slice<byte> msg;
    // section keeps track of the current section being built.
    internal section section;
    // header keeps track of what should go in the header when Finish is
    // called.
    internal Δheader header;
    // start is the starting index of the bytes allocated in msg for header.
    internal nint start;
    // compression is a mapping from name suffixes to their starting index
    // in msg.
    internal map<@string, uint16> compression;
}

// NewBuilder creates a new builder with compression disabled.
//
// Note: Most users will want to immediately enable compression with the
// EnableCompression method. See that method's comment for why you may or may
// not want to enable compression.
//
// The DNS message is appended to the provided initial buffer buf (which may be
// nil) as it is built. The final message is returned by the (*Builder).Finish
// method, which includes buf[:len(buf)] and may return the same underlying
// array if there was sufficient capacity in the slice.
public static Builder NewBuilder(slice<byte> buf, Header h) {
    if (buf == default!) {
        buf = new slice<byte>(0, packStartingCap);
    }
    var b = new Builder(msg: buf, start: len(buf));
    (b.header.id, b.header.bits) = h.pack();
    array<byte> hb = new(12); /* headerLen */
    b.msg = appendꓸꓸꓸ(b.msg, hb[..]);
    b.section = sectionHeader;
    return b;
}

// EnableCompression enables compression in the Builder.
//
// Leaving compression disabled avoids compression related allocations, but can
// result in larger message sizes. Be careful with this mode as it can cause
// messages to exceed the UDP size limit.
//
// According to RFC 1035, section 4.1.4, the use of compression is optional, but
// all implementations must accept both compressed and uncompressed DNS
// messages.
//
// Compression should be enabled before any sections are added for best results.
[GoRecv] public static void EnableCompression(this ref Builder b) {
    b.compression = new map<@string, uint16>{};
}

[GoRecv] internal static error startCheck(this ref Builder b, section s) {
    if (b.section <= sectionNotStarted) {
        return ErrNotStarted;
    }
    if (b.section > s) {
        return ErrSectionDone;
    }
    return default!;
}

// StartQuestions prepares the builder for packing Questions.
[GoRecv] public static error StartQuestions(this ref Builder b) {
    {
        var err = b.startCheck(sectionQuestions); if (err != default!) {
            return err;
        }
    }
    b.section = sectionQuestions;
    return default!;
}

// StartAnswers prepares the builder for packing Answers.
[GoRecv] public static error StartAnswers(this ref Builder b) {
    {
        var err = b.startCheck(sectionAnswers); if (err != default!) {
            return err;
        }
    }
    b.section = sectionAnswers;
    return default!;
}

// StartAuthorities prepares the builder for packing Authorities.
[GoRecv] public static error StartAuthorities(this ref Builder b) {
    {
        var err = b.startCheck(sectionAuthorities); if (err != default!) {
            return err;
        }
    }
    b.section = sectionAuthorities;
    return default!;
}

// StartAdditionals prepares the builder for packing Additionals.
[GoRecv] public static error StartAdditionals(this ref Builder b) {
    {
        var err = b.startCheck(sectionAdditionals); if (err != default!) {
            return err;
        }
    }
    b.section = sectionAdditionals;
    return default!;
}

internal static error incrementSectionCount(this ж<Builder> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ж<uint16> count = default!;
    error err = default!;
    var exprᴛ1 = b.section;
    if (exprᴛ1 == sectionQuestions) {
        count = Ꮡb.of(Builder.Ꮡheader).of(dnsmessage_package.Δheader.Ꮡquestions);
        err = errTooManyQuestions;
    }
    else if (exprᴛ1 == sectionAnswers) {
        count = Ꮡb.of(Builder.Ꮡheader).of(dnsmessage_package.Δheader.Ꮡanswers);
        err = errTooManyAnswers;
    }
    else if (exprᴛ1 == sectionAuthorities) {
        count = Ꮡb.of(Builder.Ꮡheader).of(dnsmessage_package.Δheader.Ꮡauthorities);
        err = errTooManyAuthorities;
    }
    else if (exprᴛ1 == sectionAdditionals) {
        count = Ꮡb.of(Builder.Ꮡheader).of(dnsmessage_package.Δheader.Ꮡadditionals);
        err = errTooManyAdditionals;
    }

    if (count.Value == unchecked((uint16)(~(uint16)0))) {
        return err;
    }
    count.Value++;
    return default!;
}

// Question adds a single Question.
public static error Question(this ж<Builder> Ꮡb, ΔQuestion q) {
    q = q.ΔClone();

    ref var b = ref Ꮡb.DerefOrNull();
    if (b.section < sectionQuestions) {
        return ErrNotStarted;
    }
    if (b.section > sectionQuestions) {
        return ErrSectionDone;
    }
    var (msg, err) = q.pack(b.msg, b.compression, b.start);
    if (err != default!) {
        return err;
    }
    {
        var errΔ1 = Ꮡb.incrementSectionCount(); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    b.msg = msg;
    return default!;
}

[GoRecv] internal static error checkResourceSection(this ref Builder b) {
    if (b.section < sectionAnswers) {
        return ErrNotStarted;
    }
    if (b.section > sectionAdditionals) {
        return ErrSectionDone;
    }
    return default!;
}

// CNAMEResource adds a single CNAMEResource.
public static error CNAMEResource(this ж<Builder> Ꮡb, ResourceHeader h, ΔCNAMEResource r) {
    h = h.ΔClone();
    r = r.ΔClone();

    ref var b = ref Ꮡb.DerefOrNull();
    {
        var errΔ1 = b.checkResourceSection(); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    h.Type = r.realType();
    var (msg, lenOff, err) = h.pack(b.msg, b.compression, b.start);
    if (err != default!) {
        return new nestedErrorжerror(Ꮡ(new nestedError("ResourceHeader"u8, err)));
    }
    nint preLen = len(msg);
    {
        (msg, err) = r.pack(msg, b.compression, b.start); if (err != default!) {
            return new nestedErrorжerror(Ꮡ(new nestedError("CNAMEResource body"u8, err)));
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

// MXResource adds a single MXResource.
public static error MXResource(this ж<Builder> Ꮡb, ResourceHeader h, ΔMXResource r) {
    h = h.ΔClone();
    r = r.ΔClone();

    ref var b = ref Ꮡb.DerefOrNull();
    {
        var errΔ1 = b.checkResourceSection(); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    h.Type = r.realType();
    var (msg, lenOff, err) = h.pack(b.msg, b.compression, b.start);
    if (err != default!) {
        return new nestedErrorжerror(Ꮡ(new nestedError("ResourceHeader"u8, err)));
    }
    nint preLen = len(msg);
    {
        (msg, err) = r.pack(msg, b.compression, b.start); if (err != default!) {
            return new nestedErrorжerror(Ꮡ(new nestedError("MXResource body"u8, err)));
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

// NSResource adds a single NSResource.
public static error NSResource(this ж<Builder> Ꮡb, ResourceHeader h, ΔNSResource r) {
    h = h.ΔClone();
    r = r.ΔClone();

    ref var b = ref Ꮡb.DerefOrNull();
    {
        var errΔ1 = b.checkResourceSection(); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    h.Type = r.realType();
    var (msg, lenOff, err) = h.pack(b.msg, b.compression, b.start);
    if (err != default!) {
        return new nestedErrorжerror(Ꮡ(new nestedError("ResourceHeader"u8, err)));
    }
    nint preLen = len(msg);
    {
        (msg, err) = r.pack(msg, b.compression, b.start); if (err != default!) {
            return new nestedErrorжerror(Ꮡ(new nestedError("NSResource body"u8, err)));
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

// PTRResource adds a single PTRResource.
public static error PTRResource(this ж<Builder> Ꮡb, ResourceHeader h, ΔPTRResource r) {
    h = h.ΔClone();
    r = r.ΔClone();

    ref var b = ref Ꮡb.DerefOrNull();
    {
        var errΔ1 = b.checkResourceSection(); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    h.Type = r.realType();
    var (msg, lenOff, err) = h.pack(b.msg, b.compression, b.start);
    if (err != default!) {
        return new nestedErrorжerror(Ꮡ(new nestedError("ResourceHeader"u8, err)));
    }
    nint preLen = len(msg);
    {
        (msg, err) = r.pack(msg, b.compression, b.start); if (err != default!) {
            return new nestedErrorжerror(Ꮡ(new nestedError("PTRResource body"u8, err)));
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

// SOAResource adds a single SOAResource.
public static error SOAResource(this ж<Builder> Ꮡb, ResourceHeader h, ΔSOAResource r) {
    h = h.ΔClone();
    r = r.ΔClone();

    ref var b = ref Ꮡb.DerefOrNull();
    {
        var errΔ1 = b.checkResourceSection(); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    h.Type = r.realType();
    var (msg, lenOff, err) = h.pack(b.msg, b.compression, b.start);
    if (err != default!) {
        return new nestedErrorжerror(Ꮡ(new nestedError("ResourceHeader"u8, err)));
    }
    nint preLen = len(msg);
    {
        (msg, err) = r.pack(msg, b.compression, b.start); if (err != default!) {
            return new nestedErrorжerror(Ꮡ(new nestedError("SOAResource body"u8, err)));
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

// TXTResource adds a single TXTResource.
public static error TXTResource(this ж<Builder> Ꮡb, ResourceHeader h, ΔTXTResource r) {
    h = h.ΔClone();

    ref var b = ref Ꮡb.DerefOrNull();
    {
        var errΔ1 = b.checkResourceSection(); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    h.Type = r.realType();
    var (msg, lenOff, err) = h.pack(b.msg, b.compression, b.start);
    if (err != default!) {
        return new nestedErrorжerror(Ꮡ(new nestedError("ResourceHeader"u8, err)));
    }
    nint preLen = len(msg);
    {
        (msg, err) = r.pack(msg, b.compression, b.start); if (err != default!) {
            return new nestedErrorжerror(Ꮡ(new nestedError("TXTResource body"u8, err)));
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

// SRVResource adds a single SRVResource.
public static error SRVResource(this ж<Builder> Ꮡb, ResourceHeader h, ΔSRVResource r) {
    h = h.ΔClone();
    r = r.ΔClone();

    ref var b = ref Ꮡb.DerefOrNull();
    {
        var errΔ1 = b.checkResourceSection(); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    h.Type = r.realType();
    var (msg, lenOff, err) = h.pack(b.msg, b.compression, b.start);
    if (err != default!) {
        return new nestedErrorжerror(Ꮡ(new nestedError("ResourceHeader"u8, err)));
    }
    nint preLen = len(msg);
    {
        (msg, err) = r.pack(msg, b.compression, b.start); if (err != default!) {
            return new nestedErrorжerror(Ꮡ(new nestedError("SRVResource body"u8, err)));
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

// AResource adds a single AResource.
public static error AResource(this ж<Builder> Ꮡb, ResourceHeader h, ΔAResource r) {
    h = h.ΔClone();
    r = r.ΔClone();

    ref var b = ref Ꮡb.DerefOrNull();
    {
        var errΔ1 = b.checkResourceSection(); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    h.Type = r.realType();
    var (msg, lenOff, err) = h.pack(b.msg, b.compression, b.start);
    if (err != default!) {
        return new nestedErrorжerror(Ꮡ(new nestedError("ResourceHeader"u8, err)));
    }
    nint preLen = len(msg);
    {
        (msg, err) = r.pack(msg, b.compression, b.start); if (err != default!) {
            return new nestedErrorжerror(Ꮡ(new nestedError("AResource body"u8, err)));
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

// AAAAResource adds a single AAAAResource.
public static error AAAAResource(this ж<Builder> Ꮡb, ResourceHeader h, ΔAAAAResource r) {
    h = h.ΔClone();
    r = r.ΔClone();

    ref var b = ref Ꮡb.DerefOrNull();
    {
        var errΔ1 = b.checkResourceSection(); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    h.Type = r.realType();
    var (msg, lenOff, err) = h.pack(b.msg, b.compression, b.start);
    if (err != default!) {
        return new nestedErrorжerror(Ꮡ(new nestedError("ResourceHeader"u8, err)));
    }
    nint preLen = len(msg);
    {
        (msg, err) = r.pack(msg, b.compression, b.start); if (err != default!) {
            return new nestedErrorжerror(Ꮡ(new nestedError("AAAAResource body"u8, err)));
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

// OPTResource adds a single OPTResource.
public static error OPTResource(this ж<Builder> Ꮡb, ResourceHeader h, ΔOPTResource r) {
    h = h.ΔClone();

    ref var b = ref Ꮡb.DerefOrNull();
    {
        var errΔ1 = b.checkResourceSection(); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    h.Type = r.realType();
    var (msg, lenOff, err) = h.pack(b.msg, b.compression, b.start);
    if (err != default!) {
        return new nestedErrorжerror(Ꮡ(new nestedError("ResourceHeader"u8, err)));
    }
    nint preLen = len(msg);
    {
        (msg, err) = r.pack(msg, b.compression, b.start); if (err != default!) {
            return new nestedErrorжerror(Ꮡ(new nestedError("OPTResource body"u8, err)));
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

// UnknownResource adds a single UnknownResource.
public static error UnknownResource(this ж<Builder> Ꮡb, ResourceHeader h, ΔUnknownResource r) {
    h = h.ΔClone();

    ref var b = ref Ꮡb.DerefOrNull();
    {
        var errΔ1 = b.checkResourceSection(); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    h.Type = r.realType();
    var (msg, lenOff, err) = h.pack(b.msg, b.compression, b.start);
    if (err != default!) {
        return new nestedErrorжerror(Ꮡ(new nestedError("ResourceHeader"u8, err)));
    }
    nint preLen = len(msg);
    {
        (msg, err) = r.pack(msg, b.compression, b.start); if (err != default!) {
            return new nestedErrorжerror(Ꮡ(new nestedError("UnknownResource body"u8, err)));
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

// Finish ends message building and generates a binary message.
[GoRecv] public static (slice<byte>, error) Finish(this ref Builder b) {
    if (b.section < sectionHeader) {
        return (default!, ErrNotStarted);
    }
    b.section = sectionDone;
    // Space for the header was allocated in NewBuilder.
    b.header.pack(b.msg.slice(b.start, b.start));
    return (b.msg, default!);
}

// A ResourceHeader is the header of a DNS resource record. There are
// many types of DNS resource records, but they all share the same header.
[GoType] partial struct ResourceHeader {
    // Name is the domain name for which this resource record pertains.
    public Name Name;
    // Type is the type of DNS resource record.
    //
    // This field will be set automatically during packing.
    public Type Type;
    // Class is the class of network to which this DNS resource record
    // pertains.
    public Class Class;
    // TTL is the length of time (measured in seconds) which this resource
    // record is valid for (time to live). All Resources in a set should
    // have the same TTL (RFC 2181 Section 5.2).
    public uint32 TTL;
    // Length is the length of data in the resource record after the header.
    //
    // This field will be set automatically during packing.
    public uint16 Length;
}

// GoString implements fmt.GoStringer.GoString.
[GoRecv] public static @string GoString(this ref ResourceHeader h) {
    return "dnsmessage.ResourceHeader{"u8 + "Name: "u8 + h.Name.GoString() + ", "u8 + "Type: "u8 + h.Type.GoString() + ", "u8 + "Class: "u8 + h.Class.GoString() + ", "u8 + "TTL: "u8 + printUint32(h.TTL) + ", "u8 + "Length: "u8 + printUint16(h.Length) + "}"u8;
}

// pack appends the wire format of the ResourceHeader to oldMsg.
//
// lenOff is the offset in msg where the Length field was packed.
[GoRecv] internal static (slice<byte> msg, nint lenOff, error err) pack(this ref ResourceHeader h, slice<byte> oldMsg, map<@string, uint16> compression, nint compressionOff) {
    slice<byte> msg = default!;
    nint lenOff = default!;
    error err = default!;

    msg = oldMsg;
    {
        (msg, err) = h.Name.pack(msg, compression, compressionOff); if (err != default!) {
            return (oldMsg, 0, new nestedErrorжerror(Ꮡ(new nestedError("Name"u8, err))));
        }
    }
    msg = packType(msg, h.Type);
    msg = packClass(msg, h.Class);
    msg = packUint32(msg, h.TTL);
    lenOff = len(msg);
    msg = packUint16(msg, h.Length);
    return (msg, lenOff, default!);
}

[GoRecv] internal static (nint, error) unpack(this ref ResourceHeader h, slice<byte> msg, nint off) {
    nint newOff = off;
    error err = default!;
    {
        (newOff, err) = h.Name.unpack(msg, newOff); if (err != default!) {
            return (off, new nestedErrorжerror(Ꮡ(new nestedError("Name"u8, err))));
        }
    }
    {
        (h.Type, newOff, err) = unpackType(msg, newOff); if (err != default!) {
            return (off, new nestedErrorжerror(Ꮡ(new nestedError("Type"u8, err))));
        }
    }
    {
        (h.Class, newOff, err) = unpackClass(msg, newOff); if (err != default!) {
            return (off, new nestedErrorжerror(Ꮡ(new nestedError("Class"u8, err))));
        }
    }
    {
        (h.TTL, newOff, err) = unpackUint32(msg, newOff); if (err != default!) {
            return (off, new nestedErrorжerror(Ꮡ(new nestedError("TTL"u8, err))));
        }
    }
    {
        (h.Length, newOff, err) = unpackUint16(msg, newOff); if (err != default!) {
            return (off, new nestedErrorжerror(Ꮡ(new nestedError("Length"u8, err))));
        }
    }
    return (newOff, default!);
}

// fixLen updates a packed ResourceHeader to include the length of the
// ResourceBody.
//
// lenOff is the offset of the ResourceHeader.Length field in msg.
//
// preLen is the length that msg was before the ResourceBody was packed.
[GoRecv] internal static error fixLen(this ref ResourceHeader h, slice<byte> msg, nint lenOff, nint preLen) {
    nint conLen = len(msg) - preLen;
    if (conLen > (nint)(unchecked((uint16)(~(uint16)0)))) {
        return errResTooLong;
    }
    // Fill in the length now that we know how long the content is.
    packUint16(msg.slice(lenOff, lenOff), (uint16)conLen);
    h.Length = (uint16)conLen;
    return default!;
}

// EDNS(0) wire constants.
internal static UntypedInt edns0Version => 0;

internal static UntypedInt edns0DNSSECOK => 0x00008000;

internal static UntypedInt ednsVersionMask => 0x00ff0000;

internal static UntypedInt edns0DNSSECOKMask => 0x00ff8000;

// SetEDNS0 configures h for EDNS(0).
//
// The provided extRCode must be an extended RCode.
[GoRecv] public static error SetEDNS0(this ref ResourceHeader h, nint udpPayloadLen, RCode extRCode, bool dnssecOK) {
    h.Name = new Name(Data: new byte[]{(rune)'.'}.array(255), Length: 1); // RFC 6891 section 6.1.2
    h.Type = TypeOPT;
    h.Class = ((Class)(uint16)udpPayloadLen);
    h.TTL = (((uint32)(uint16)extRCode >> (int)(4)) << (int)(24));
    if (dnssecOK) {
        h.TTL |= (uint32)(edns0DNSSECOK);
    }
    return default!;
}

// DNSSECAllowed reports whether the DNSSEC OK bit is set.
[GoRecv] public static bool DNSSECAllowed(this ref ResourceHeader h) {
    return (uint32)(h.TTL & (uint32)edns0DNSSECOKMask) == edns0DNSSECOK; // RFC 6891 section 6.1.3
}

// ExtendedRCode returns an extended RCode.
//
// The provided rcode must be the RCode in DNS message header.
[GoRecv] public static RCode ExtendedRCode(this ref ResourceHeader h, RCode rcode) {
    if ((uint32)(h.TTL & (uint32)ednsVersionMask) == edns0Version) {
        // RFC 6891 section 6.1.3
        return (RCode)(((RCode)(uint16)(((h.TTL >> (int)(24)) << (int)(4)))) | rcode);
    }
    return rcode;
}

internal static (nint, error) skipResource(slice<byte> msg, nint off) {
    var (newOff, err) = skipName(msg, off);
    if (err != default!) {
        return (off, new nestedErrorжerror(Ꮡ(new nestedError("Name"u8, err))));
    }
    {
        (newOff, err) = skipType(msg, newOff); if (err != default!) {
            return (off, new nestedErrorжerror(Ꮡ(new nestedError("Type"u8, err))));
        }
    }
    {
        (newOff, err) = skipClass(msg, newOff); if (err != default!) {
            return (off, new nestedErrorжerror(Ꮡ(new nestedError("Class"u8, err))));
        }
    }
    {
        (newOff, err) = skipUint32(msg, newOff); if (err != default!) {
            return (off, new nestedErrorжerror(Ꮡ(new nestedError("TTL"u8, err))));
        }
    }
    (var length, newOff, err) = unpackUint16(msg, newOff);
    if (err != default!) {
        return (off, new nestedErrorжerror(Ꮡ(new nestedError("Length"u8, err))));
    }
    {
        newOff += (nint)length; if (newOff > len(msg)) {
            return (off, errResourceLen);
        }
    }
    return (newOff, default!);
}

// packUint16 appends the wire format of field to msg.
internal static slice<byte> packUint16(slice<byte> msg, uint16 field) {
    return append(msg, (byte)((field >> (int)(8))), (byte)field);
}

internal static (uint16, nint, error) unpackUint16(slice<byte> msg, nint off) {
    if (off + (nint)uint16Len > len(msg)) {
        return (0, off, errBaseLen);
    }
    return ((uint16)((uint16)((uint16)msg[off] << (int)(8)) | (uint16)msg[off + 1]), off + (nint)uint16Len, default!);
}

internal static (nint, error) skipUint16(slice<byte> msg, nint off) {
    if (off + (nint)uint16Len > len(msg)) {
        return (off, errBaseLen);
    }
    return (off + (nint)uint16Len, default!);
}

// packType appends the wire format of field to msg.
internal static slice<byte> packType(slice<byte> msg, Type field) {
    return packUint16(msg, (uint16)field);
}

internal static (Type, nint, error) unpackType(slice<byte> msg, nint off) {
    var (t, o, err) = unpackUint16(msg, off);
    return (((Type)t), o, err);
}

internal static (nint, error) skipType(slice<byte> msg, nint off) {
    return skipUint16(msg, off);
}

// packClass appends the wire format of field to msg.
internal static slice<byte> packClass(slice<byte> msg, Class field) {
    return packUint16(msg, (uint16)field);
}

internal static (Class, nint, error) unpackClass(slice<byte> msg, nint off) {
    var (c, o, err) = unpackUint16(msg, off);
    return (((Class)c), o, err);
}

internal static (nint, error) skipClass(slice<byte> msg, nint off) {
    return skipUint16(msg, off);
}

// packUint32 appends the wire format of field to msg.
internal static slice<byte> packUint32(slice<byte> msg, uint32 field) {
    return append(
        msg,
        (byte)((field >> (int)(24))),
        (byte)((field >> (int)(16))),
        (byte)((field >> (int)(8))),
        (byte)field);
}

internal static (uint32, nint, error) unpackUint32(slice<byte> msg, nint off) {
    if (off + (nint)uint32Len > len(msg)) {
        return (0, off, errBaseLen);
    }
    var v = (uint32)((uint32)((uint32)(((uint32)msg[off] << (int)(24)) | ((uint32)msg[off + 1] << (int)(16))) | ((uint32)msg[off + 2] << (int)(8))) | (uint32)msg[off + 3]);
    return (v, off + (nint)uint32Len, default!);
}

internal static (nint, error) skipUint32(slice<byte> msg, nint off) {
    if (off + (nint)uint32Len > len(msg)) {
        return (off, errBaseLen);
    }
    return (off + (nint)uint32Len, default!);
}

// packText appends the wire format of field to msg.
internal static (slice<byte>, error) packText(slice<byte> msg, @string field) {
    nint l = len(field);
    if (l > 255) {
        return (default!, errStringTooLong);
    }
    msg = append(msg, (byte)l);
    msg = append(msg, field.ꓸꓸꓸ);
    return (msg, default!);
}

internal static (@string, nint, error) unpackText(slice<byte> msg, nint off) {
    if (off >= len(msg)) {
        return ("", off, errBaseLen);
    }
    nint beginOff = off + 1;
    nint endOff = beginOff + (nint)msg[off];
    if (endOff > len(msg)) {
        return ("", off, errCalcLen);
    }
    return (((@string)(msg.slice(beginOff, endOff))), endOff, default!);
}

// packBytes appends the wire format of field to msg.
internal static slice<byte> packBytes(slice<byte> msg, slice<byte> field) {
    return appendꓸꓸꓸ(msg, field);
}

internal static (nint, error) unpackBytes(slice<byte> msg, nint off, slice<byte> field) {
    nint newOff = off + len(field);
    if (newOff > len(msg)) {
        return (off, errBaseLen);
    }
    copy(field, msg.slice(off, newOff));
    return (newOff, default!);
}

internal static UntypedInt nonEncodedNameMax => 254;

// A Name is a non-encoded and non-escaped domain name. It is used instead of strings to avoid
// allocations.
[GoType] partial struct Name {
    public array<byte> Data = new(255);
    public uint8 Length;
}

// NewName creates a new Name from a string.
public static (Name, error) NewName(@string name) {
    var n = new Name(Length: (uint8)len(name));
    if (len(name) > len(n.Data)) {
        return (new Name(nil), errCalcLen);
    }
    copy(n.Data[..], name);
    return (n.ΔClone(), default!);
}

// MustNewName creates a new Name from a string and panics on error.
public static Name MustNewName(@string name) {
    var (n, err) = NewName(name);
    if (err != default!) {
        throw panic("creating name: " + err.Error());
    }
    return n.ΔClone();
}

// String implements fmt.Stringer.String.
//
// Note: characters inside the labels are not escaped in any way.
public static @string String(this Name n) {
    n = n.ΔClone();

    return ((@string)(n.Data.slice(0, n.Length)));
}

// GoString implements fmt.GoStringer.GoString.
[GoRecv] public static @string GoString(this ref Name n) {
    return @"dnsmessage.MustNewName("""u8 + printString(n.Data.slice(0, n.Length)) + @""")"u8;
}

// pack appends the wire format of the Name to msg.
//
// Domain names are a sequence of counted strings split at the dots. They end
// with a zero-length string. Compression can be used to reuse domain suffixes.
//
// The compression map will be updated with new domain suffixes. If compression
// is nil, compression will not be used.
[GoRecv] internal static (slice<byte>, error) pack(this ref Name n, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) {
    var oldMsg = msg;
    if (n.Length > nonEncodedNameMax) {
        return (default!, errNameTooLong);
    }
    // Add a trailing dot to canonicalize name.
    if (n.Length == 0 || n.Data[(uint8)(n.Length - 1)] != (rune)'.') {
        return (oldMsg, errNonCanonicalName);
    }
    // Allow root domain.
    if (n.Data[0] == (rune)'.' && n.Length == 1) {
        return (append(msg, (byte)(0)), default!);
    }
    @string nameAsStr = default!;
    // Emit sequence of counted strings, chopping at dots.
    for ((nint i, nint begin) = (0, 0); i < (nint)n.Length; i++) {
        // Check for the end of the segment.
        if (n.Data[i] == (rune)'.') {
            // The two most significant bits have special meaning.
            // It isn't allowed for segments to be long enough to
            // need them.
            if (i - begin >= (1 << (int)(6))) {
                return (oldMsg, errSegTooLong);
            }
            // Segments must have a non-zero length.
            if (i - begin == 0) {
                return (oldMsg, errZeroSegLen);
            }
            msg = append(msg, (byte)(i - begin));
            for (nint j = begin; j < i; j++) {
                msg = append(msg, n.Data[j]);
            }
            begin = i + 1;
            continue;
        }
        // We can only compress domain suffixes starting with a new
        // segment. A pointer is two bytes with the two most significant
        // bits set to 1 to indicate that it is a pointer.
        if ((i == 0 || n.Data[i - 1] == (rune)'.') && compression != default!) {
            {
                var (ptr, ok) = compression[tmpstring(n.Data.slice(i, n.Length)), ꟷ]; if (ok) {
                    // Hit. Emit a pointer instead of the rest of
                    // the domain.
                    return (append(msg, (byte)((uint16)((ptr >> (int)(8)) | 0xC0)), (byte)ptr), default!);
                }
            }
            // Miss. Add the suffix to the compression table if the
            // offset can be stored in the available 14 bits.
            nint newPtr = len(msg) - compressionOff;
            if (newPtr <= (nint)((unchecked((uint16)(~(uint16)0)) >> (int)(2)))) {
                if (nameAsStr == ""u8) {
                    // allocate n.Data on the heap once, to avoid allocating it
                    // multiple times (for next labels).
                    nameAsStr = ((@string)(n.Data.slice(0, n.Length)));
                }
                compression[nameAsStr.slice(i)] = (uint16)newPtr;
            }
        }
    }
    return (append(msg, (byte)(0)), default!);
}

// unpack unpacks a domain name.
[GoRecv] internal static (nint, error) unpack(this ref Name n, slice<byte> msg, nint off) {
    // currOff is the current working offset.
    nint currOff = off;
    // newOff is the offset where the next record will start. Pointers lead
    // to data that belongs to other names and thus doesn't count towards to
    // the usage of this name.
    nint newOff = off;
    // ptr is the number of pointers followed.
    nint ptr = default!;
    // Name is a slice representation of the name data.
    var name = n.Data[..0];
Loop:
    while (ᐧ) {
        if (currOff >= len(msg)) {
            return (off, errBaseLen);
        }
        nint c = (nint)msg[currOff];
        currOff++;
        switch ((nint)(c & 0xC0)) {
        case 0x00: {
            if (c == 0x00) {
                // String segment
                // A zero length signals the end of the name.
                goto break_Loop;
            }
            nint endOff = currOff + c;
            if (endOff > len(msg)) {
                return (off, errCalcLen);
            }
            foreach (var (_, v) in msg.slice(currOff, endOff)) {
                // Reject names containing dots.
                // See issue golang/go#56246
                if (v == (rune)'.') {
                    return (off, errInvalidName);
                }
            }
            name = appendꓸꓸꓸ(name, msg.slice(currOff, endOff));
            name = append(name, (byte)((rune)'.'));
            currOff = endOff;
            break;
        }
        case 0xC0: {
            if (currOff >= len(msg)) {
                // Pointer
                return (off, errInvalidPtr);
            }
            var c1 = msg[currOff];
            currOff++;
            if (ptr == 0) {
                newOff = currOff;
            }
            {
                ptr++; if (ptr > 10) {
                    // Don't follow too many pointers, maybe there's a loop.
                    return (off, errTooManyPtr);
                }
            }
            currOff = (nint)((((nint)(c ^ 0xC0)) << (int)(8)) | (nint)c1);
            break;
        }
        default: {
            return (off, errReserved);
        }}

continue_Loop:;
    }
break_Loop:;
    // Prefixes 0x80 and 0x40 are reserved.
    if (len(name) == 0) {
        name = append(name, (byte)((rune)'.'));
    }
    if (len(name) > nonEncodedNameMax) {
        return (off, errNameTooLong);
    }
    n.Length = (uint8)len(name);
    if (ptr == 0) {
        newOff = currOff;
    }
    return (newOff, default!);
}

internal static (nint, error) skipName(slice<byte> msg, nint off) {
    // newOff is the offset where the next record will start. Pointers lead
    // to data that belongs to other names and thus doesn't count towards to
    // the usage of this name.
    nint newOff = off;
Loop:
    while (ᐧ) {
        if (newOff >= len(msg)) {
            return (off, errBaseLen);
        }
        nint c = (nint)msg[newOff];
        newOff++;
        switch ((nint)(c & 0xC0)) {
        case 0x00: {
            if (c == 0x00) {
                // A zero length signals the end of the name.
                goto break_Loop;
            }
            newOff += c;
            if (newOff > len(msg)) {
                // literal string
                return (off, errCalcLen);
            }
            break;
        }
        case 0xC0: {
            newOff++;
            goto break_Loop;
            break;
        }
        default: {
            return (off, errReserved);
        }}

continue_Loop:;
    }
break_Loop:;
    // Pointer to somewhere else in msg.
    // Pointers are two bytes.
    // Don't follow the pointer as the data here has ended.
    // Prefixes 0x80 and 0x40 are reserved.
    return (newOff, default!);
}

// A Question is a DNS query.
[GoType] partial struct ΔQuestion {
    public Name Name;
    public Type Type;
    public Class Class;
}

// pack appends the wire format of the Question to msg.
[GoRecv] internal static (slice<byte>, error) pack(this ref ΔQuestion q, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) {
    (msg, var err) = q.Name.pack(msg, compression, compressionOff);
    if (err != default!) {
        return (msg, new nestedErrorжerror(Ꮡ(new nestedError("Name"u8, err))));
    }
    msg = packType(msg, q.Type);
    return (packClass(msg, q.Class), default!);
}

// GoString implements fmt.GoStringer.GoString.
[GoRecv] public static @string GoString(this ref ΔQuestion q) {
    return "dnsmessage.Question{"u8 + "Name: "u8 + q.Name.GoString() + ", "u8 + "Type: "u8 + q.Type.GoString() + ", "u8 + "Class: "u8 + q.Class.GoString() + "}"u8;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string cnameˢ = "CNAME"u8;
private static readonly @string soaˢ = "SOA"u8;
private static readonly @string ptrˢ = "PTR"u8;
private static readonly @string txtˢ = "TXT"u8;
private static readonly @string aaaaˢ = "AAAA"u8;
private static readonly @string srvˢ = "SRV"u8;
private static readonly @string optˢ = "OPT"u8;
private static readonly @string unknownˢ = "Unknown"u8;

internal static (ResourceBody, nint, error) unpackResourceBody(slice<byte> msg, nint off, ResourceHeader hdr) {
    hdr = hdr.ΔClone();

    ResourceBody r = default!;
    error err = default!;
    @string name = default!;
    var exprᴛ1 = hdr.Type;
    if (exprᴛ1 == TypeA) {
        ref var rb = ref heap(new ΔAResource(), out var Ꮡrb);
        (rb, err) = unpackAResource(msg, off);
        r = new ΔAResourceжResourceBody(Ꮡrb);
        name = "A"u8;
    }
    else if (exprᴛ1 == TypeNS) {
        ref var rb = ref heap(new ΔNSResource(), out var Ꮡrb);
        (rb, err) = unpackNSResource(msg, off);
        r = new ΔNSResourceжResourceBody(Ꮡrb);
        name = "NS"u8;
    }
    else if (exprᴛ1 == TypeCNAME) {
        ref var rb = ref heap(new ΔCNAMEResource(), out var Ꮡrb);
        (rb, err) = unpackCNAMEResource(msg, off);
        r = new ΔCNAMEResourceжResourceBody(Ꮡrb);
        name = cnameˢ;
    }
    else if (exprᴛ1 == TypeSOA) {
        ref var rb = ref heap(new ΔSOAResource(), out var Ꮡrb);
        (rb, err) = unpackSOAResource(msg, off);
        r = new ΔSOAResourceжResourceBody(Ꮡrb);
        name = soaˢ;
    }
    else if (exprᴛ1 == TypePTR) {
        ref var rb = ref heap(new ΔPTRResource(), out var Ꮡrb);
        (rb, err) = unpackPTRResource(msg, off);
        r = new ΔPTRResourceжResourceBody(Ꮡrb);
        name = ptrˢ;
    }
    else if (exprᴛ1 == TypeMX) {
        ref var rb = ref heap(new ΔMXResource(), out var Ꮡrb);
        (rb, err) = unpackMXResource(msg, off);
        r = new ΔMXResourceжResourceBody(Ꮡrb);
        name = "MX"u8;
    }
    else if (exprᴛ1 == TypeTXT) {
        ref var rb = ref heap(new ΔTXTResource(), out var Ꮡrb);
        (rb, err) = unpackTXTResource(msg, off, hdr.Length);
        r = new ΔTXTResourceжResourceBody(Ꮡrb);
        name = txtˢ;
    }
    else if (exprᴛ1 == TypeAAAA) {
        ref var rb = ref heap(new ΔAAAAResource(), out var Ꮡrb);
        (rb, err) = unpackAAAAResource(msg, off);
        r = new ΔAAAAResourceжResourceBody(Ꮡrb);
        name = aaaaˢ;
    }
    else if (exprᴛ1 == TypeSRV) {
        ref var rb = ref heap(new ΔSRVResource(), out var Ꮡrb);
        (rb, err) = unpackSRVResource(msg, off);
        r = new ΔSRVResourceжResourceBody(Ꮡrb);
        name = srvˢ;
    }
    else if (exprᴛ1 == TypeOPT) {
        ref var rb = ref heap(new ΔOPTResource(), out var Ꮡrb);
        (rb, err) = unpackOPTResource(msg, off, hdr.Length);
        r = new ΔOPTResourceжResourceBody(Ꮡrb);
        name = optˢ;
    }
    else { /* default: */
        ref var rb = ref heap(new ΔUnknownResource(), out var Ꮡrb);
        (rb, err) = unpackUnknownResource(hdr.Type, msg, off, hdr.Length);
        r = new ΔUnknownResourceжResourceBody(Ꮡrb);
        name = unknownˢ;
    }

    if (err != default!) {
        return (default!, off, new nestedErrorжerror(Ꮡ(new nestedError(name + " record"u8, err))));
    }
    return (r, off + (nint)hdr.Length, default!);
}

// A CNAMEResource is a CNAME Resource record.
[GoType] partial struct ΔCNAMEResource {
    public Name CNAME;
}

[GoRecv] internal static Type realType(this ref ΔCNAMEResource r) {
    return TypeCNAME;
}

// pack appends the wire format of the CNAMEResource to msg.
[GoRecv] internal static (slice<byte>, error) pack(this ref ΔCNAMEResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) {
    return r.CNAME.pack(msg, compression, compressionOff);
}

// GoString implements fmt.GoStringer.GoString.
[GoRecv] public static @string GoString(this ref ΔCNAMEResource r) {
    return "dnsmessage.CNAMEResource{CNAME: "u8 + r.CNAME.GoString() + "}"u8;
}

internal static (ΔCNAMEResource, error) unpackCNAMEResource(slice<byte> msg, nint off) {
    Name cname = new();
    {
        var (_, err) = cname.unpack(msg, off); if (err != default!) {
            return (new ΔCNAMEResource(nil), err);
        }
    }
    return (new ΔCNAMEResource(cname.ΔClone()), default!);
}

// An MXResource is an MX Resource record.
[GoType] partial struct ΔMXResource {
    public uint16 Pref;
    public Name MX;
}

[GoRecv] internal static Type realType(this ref ΔMXResource r) {
    return TypeMX;
}

// pack appends the wire format of the MXResource to msg.
[GoRecv] internal static (slice<byte>, error) pack(this ref ΔMXResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) {
    var oldMsg = msg;
    msg = packUint16(msg, r.Pref);
    (msg, var err) = r.MX.pack(msg, compression, compressionOff);
    if (err != default!) {
        return (oldMsg, new nestedErrorжerror(Ꮡ(new nestedError("MXResource.MX"u8, err))));
    }
    return (msg, default!);
}

// GoString implements fmt.GoStringer.GoString.
[GoRecv] public static @string GoString(this ref ΔMXResource r) {
    return "dnsmessage.MXResource{"u8 + "Pref: "u8 + printUint16(r.Pref) + ", "u8 + "MX: "u8 + r.MX.GoString() + "}"u8;
}

internal static (ΔMXResource, error) unpackMXResource(slice<byte> msg, nint off) {
    (var pref, off, var err) = unpackUint16(msg, off);
    if (err != default!) {
        return (new ΔMXResource(nil), new nestedErrorжerror(Ꮡ(new nestedError("Pref"u8, err))));
    }
    Name mx = new();
    {
        var (_, errΔ1) = mx.unpack(msg, off); if (errΔ1 != default!) {
            return (new ΔMXResource(nil), new nestedErrorжerror(Ꮡ(new nestedError("MX"u8, errΔ1))));
        }
    }
    return (new ΔMXResource(pref, mx.ΔClone()), default!);
}

// An NSResource is an NS Resource record.
[GoType] partial struct ΔNSResource {
    public Name NS;
}

[GoRecv] internal static Type realType(this ref ΔNSResource r) {
    return TypeNS;
}

// pack appends the wire format of the NSResource to msg.
[GoRecv] internal static (slice<byte>, error) pack(this ref ΔNSResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) {
    return r.NS.pack(msg, compression, compressionOff);
}

// GoString implements fmt.GoStringer.GoString.
[GoRecv] public static @string GoString(this ref ΔNSResource r) {
    return "dnsmessage.NSResource{NS: "u8 + r.NS.GoString() + "}"u8;
}

internal static (ΔNSResource, error) unpackNSResource(slice<byte> msg, nint off) {
    Name ns = new();
    {
        var (_, err) = ns.unpack(msg, off); if (err != default!) {
            return (new ΔNSResource(nil), err);
        }
    }
    return (new ΔNSResource(ns.ΔClone()), default!);
}

// A PTRResource is a PTR Resource record.
[GoType] partial struct ΔPTRResource {
    public Name PTR;
}

[GoRecv] internal static Type realType(this ref ΔPTRResource r) {
    return TypePTR;
}

// pack appends the wire format of the PTRResource to msg.
[GoRecv] internal static (slice<byte>, error) pack(this ref ΔPTRResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) {
    return r.PTR.pack(msg, compression, compressionOff);
}

// GoString implements fmt.GoStringer.GoString.
[GoRecv] public static @string GoString(this ref ΔPTRResource r) {
    return "dnsmessage.PTRResource{PTR: "u8 + r.PTR.GoString() + "}"u8;
}

internal static (ΔPTRResource, error) unpackPTRResource(slice<byte> msg, nint off) {
    Name ptr = new();
    {
        var (_, err) = ptr.unpack(msg, off); if (err != default!) {
            return (new ΔPTRResource(nil), err);
        }
    }
    return (new ΔPTRResource(ptr.ΔClone()), default!);
}

// An SOAResource is an SOA Resource record.
[GoType] partial struct ΔSOAResource {
    public Name NS;
    public Name MBox;
    public uint32 Serial;
    public uint32 Refresh;
    public uint32 Retry;
    public uint32 Expire;
    // MinTTL the is the default TTL of Resources records which did not
    // contain a TTL value and the TTL of negative responses. (RFC 2308
    // Section 4)
    public uint32 MinTTL;
}

[GoRecv] internal static Type realType(this ref ΔSOAResource r) {
    return TypeSOA;
}

// pack appends the wire format of the SOAResource to msg.
[GoRecv] internal static (slice<byte>, error) pack(this ref ΔSOAResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) {
    var oldMsg = msg;
    (msg, var err) = r.NS.pack(msg, compression, compressionOff);
    if (err != default!) {
        return (oldMsg, new nestedErrorжerror(Ꮡ(new nestedError("SOAResource.NS"u8, err))));
    }
    (msg, err) = r.MBox.pack(msg, compression, compressionOff);
    if (err != default!) {
        return (oldMsg, new nestedErrorжerror(Ꮡ(new nestedError("SOAResource.MBox"u8, err))));
    }
    msg = packUint32(msg, r.Serial);
    msg = packUint32(msg, r.Refresh);
    msg = packUint32(msg, r.Retry);
    msg = packUint32(msg, r.Expire);
    return (packUint32(msg, r.MinTTL), default!);
}

// GoString implements fmt.GoStringer.GoString.
[GoRecv] public static @string GoString(this ref ΔSOAResource r) {
    return "dnsmessage.SOAResource{"u8 + "NS: "u8 + r.NS.GoString() + ", "u8 + "MBox: "u8 + r.MBox.GoString() + ", "u8 + "Serial: "u8 + printUint32(r.Serial) + ", "u8 + "Refresh: "u8 + printUint32(r.Refresh) + ", "u8 + "Retry: "u8 + printUint32(r.Retry) + ", "u8 + "Expire: "u8 + printUint32(r.Expire) + ", "u8 + "MinTTL: "u8 + printUint32(r.MinTTL) + "}"u8;
}

internal static (ΔSOAResource, error) unpackSOAResource(slice<byte> msg, nint off) {
    Name ns = new();
    (off, var err) = ns.unpack(msg, off);
    if (err != default!) {
        return (new ΔSOAResource(nil), new nestedErrorжerror(Ꮡ(new nestedError("NS"u8, err))));
    }
    Name mbox = new();
    {
        (off, err) = mbox.unpack(msg, off); if (err != default!) {
            return (new ΔSOAResource(nil), new nestedErrorжerror(Ꮡ(new nestedError("MBox"u8, err))));
        }
    }
    (var serial, off, err) = unpackUint32(msg, off);
    if (err != default!) {
        return (new ΔSOAResource(nil), new nestedErrorжerror(Ꮡ(new nestedError("Serial"u8, err))));
    }
    (var refresh, off, err) = unpackUint32(msg, off);
    if (err != default!) {
        return (new ΔSOAResource(nil), new nestedErrorжerror(Ꮡ(new nestedError("Refresh"u8, err))));
    }
    (var retry, off, err) = unpackUint32(msg, off);
    if (err != default!) {
        return (new ΔSOAResource(nil), new nestedErrorжerror(Ꮡ(new nestedError("Retry"u8, err))));
    }
    (var expire, off, err) = unpackUint32(msg, off);
    if (err != default!) {
        return (new ΔSOAResource(nil), new nestedErrorжerror(Ꮡ(new nestedError("Expire"u8, err))));
    }
    (var minTTL, _, err) = unpackUint32(msg, off);
    if (err != default!) {
        return (new ΔSOAResource(nil), new nestedErrorжerror(Ꮡ(new nestedError("MinTTL"u8, err))));
    }
    return (new ΔSOAResource(ns.ΔClone(), mbox.ΔClone(), serial, refresh, retry, expire, minTTL), default!);
}

// A TXTResource is a TXT Resource record.
[GoType] partial struct ΔTXTResource {
    public slice<@string> TXT;
}

[GoRecv] internal static Type realType(this ref ΔTXTResource r) {
    return TypeTXT;
}

// pack appends the wire format of the TXTResource to msg.
[GoRecv] internal static (slice<byte>, error) pack(this ref ΔTXTResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) {
    var oldMsg = msg;
    foreach (var (_, s) in r.TXT) {
        error err = default!;
        (msg, err) = packText(msg, s);
        if (err != default!) {
            return (oldMsg, err);
        }
    }
    return (msg, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string dnsmessageTXTResourceTxtˢ = "dnsmessage.TXTResource{TXT: []string{"u8;

// GoString implements fmt.GoStringer.GoString.
[GoRecv] public static @string GoString(this ref ΔTXTResource r) {
    @string s = dnsmessageTXTResourceTxtˢ;
    if (len(r.TXT) == 0) {
        return s + "}}"u8;
    }
    s += @""""u8 + printString(slice<byte>(r.TXT[0]));
    foreach (var (_, t) in r.TXT[1..]) {
        s += @""", """u8 + printString(slice<byte>(t));
    }
    return s + @"""}}"u8;
}

internal static (ΔTXTResource, error) unpackTXTResource(slice<byte> msg, nint off, uint16 length) {
    var txts = new slice<@string>(0, 1);
    for (var n = (uint16)0; n < length; ) {
        @string t = default!;
        error err = default!;
        {
            (t, off, err) = unpackText(msg, off); if (err != default!) {
                return (new ΔTXTResource(nil), new nestedErrorжerror(Ꮡ(new nestedError("text"u8, err))));
            }
        }
        // Check if we got too many bytes.
        if ((uint16)(length - n) < (uint16)((uint16)len(t) + 1)) {
            return (new ΔTXTResource(nil), errCalcLen);
        }
        n += (uint16)((uint16)len(t) + 1);
        txts = append(txts, t);
    }
    return (new ΔTXTResource(txts), default!);
}

// An SRVResource is an SRV Resource record.
[GoType] partial struct ΔSRVResource {
    public uint16 Priority;
    public uint16 Weight;
    public uint16 Port;
    public Name Target; // Not compressed as per RFC 2782.
}

[GoRecv] internal static Type realType(this ref ΔSRVResource r) {
    return TypeSRV;
}

// pack appends the wire format of the SRVResource to msg.
[GoRecv] internal static (slice<byte>, error) pack(this ref ΔSRVResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) {
    var oldMsg = msg;
    msg = packUint16(msg, r.Priority);
    msg = packUint16(msg, r.Weight);
    msg = packUint16(msg, r.Port);
    (msg, var err) = r.Target.pack(msg, default!, compressionOff);
    if (err != default!) {
        return (oldMsg, new nestedErrorжerror(Ꮡ(new nestedError("SRVResource.Target"u8, err))));
    }
    return (msg, default!);
}

// GoString implements fmt.GoStringer.GoString.
[GoRecv] public static @string GoString(this ref ΔSRVResource r) {
    return "dnsmessage.SRVResource{"u8 + "Priority: "u8 + printUint16(r.Priority) + ", "u8 + "Weight: "u8 + printUint16(r.Weight) + ", "u8 + "Port: "u8 + printUint16(r.Port) + ", "u8 + "Target: "u8 + r.Target.GoString() + "}"u8;
}

internal static (ΔSRVResource, error) unpackSRVResource(slice<byte> msg, nint off) {
    (var priority, off, var err) = unpackUint16(msg, off);
    if (err != default!) {
        return (new ΔSRVResource(nil), new nestedErrorжerror(Ꮡ(new nestedError("Priority"u8, err))));
    }
    (var weight, off, err) = unpackUint16(msg, off);
    if (err != default!) {
        return (new ΔSRVResource(nil), new nestedErrorжerror(Ꮡ(new nestedError("Weight"u8, err))));
    }
    (var port, off, err) = unpackUint16(msg, off);
    if (err != default!) {
        return (new ΔSRVResource(nil), new nestedErrorжerror(Ꮡ(new nestedError("Port"u8, err))));
    }
    Name target = new();
    {
        var (_, errΔ1) = target.unpack(msg, off); if (errΔ1 != default!) {
            return (new ΔSRVResource(nil), new nestedErrorжerror(Ꮡ(new nestedError("Target"u8, errΔ1))));
        }
    }
    return (new ΔSRVResource(priority, weight, port, target.ΔClone()), default!);
}

// An AResource is an A Resource record.
[GoType] partial struct ΔAResource {
    public array<byte> A = new(4);
}

[GoRecv] internal static Type realType(this ref ΔAResource r) {
    return TypeA;
}

// pack appends the wire format of the AResource to msg.
[GoRecv] internal static (slice<byte>, error) pack(this ref ΔAResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) {
    return (packBytes(msg, r.A[..]), default!);
}

// GoString implements fmt.GoStringer.GoString.
[GoRecv] public static @string GoString(this ref ΔAResource r) {
    return "dnsmessage.AResource{"u8 + "A: [4]byte{"u8 + printByteSlice(r.A[..]) + "}}"u8;
}

internal static (ΔAResource, error) unpackAResource(slice<byte> msg, nint off) {
    array<byte> a = new(4);
    {
        var (_, err) = unpackBytes(msg, off, a[..]); if (err != default!) {
            return (new ΔAResource(nil), err);
        }
    }
    return (new ΔAResource(a.Clone()), default!);
}

// An AAAAResource is an AAAA Resource record.
[GoType] partial struct ΔAAAAResource {
    public array<byte> AAAA = new(16);
}

[GoRecv] internal static Type realType(this ref ΔAAAAResource r) {
    return TypeAAAA;
}

// GoString implements fmt.GoStringer.GoString.
[GoRecv] public static @string GoString(this ref ΔAAAAResource r) {
    return "dnsmessage.AAAAResource{"u8 + "AAAA: [16]byte{"u8 + printByteSlice(r.AAAA[..]) + "}}"u8;
}

// pack appends the wire format of the AAAAResource to msg.
[GoRecv] internal static (slice<byte>, error) pack(this ref ΔAAAAResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) {
    return (packBytes(msg, r.AAAA[..]), default!);
}

internal static (ΔAAAAResource, error) unpackAAAAResource(slice<byte> msg, nint off) {
    array<byte> aaaa = new(16);
    {
        var (_, err) = unpackBytes(msg, off, aaaa[..]); if (err != default!) {
            return (new ΔAAAAResource(nil), err);
        }
    }
    return (new ΔAAAAResource(aaaa.Clone()), default!);
}

// An OPTResource is an OPT pseudo Resource record.
//
// The pseudo resource record is part of the extension mechanisms for DNS
// as defined in RFC 6891.
[GoType] partial struct ΔOPTResource {
    public slice<Option> Options;
}

// An Option represents a DNS message option within OPTResource.
//
// The message option is part of the extension mechanisms for DNS as
// defined in RFC 6891.
[GoType] partial struct Option {
    public uint16 Code; // option code
    public slice<byte> Data;
}

// GoString implements fmt.GoStringer.GoString.
[GoRecv] public static @string GoString(this ref Option o) {
    return "dnsmessage.Option{"u8 + "Code: "u8 + printUint16(o.Code) + ", "u8 + "Data: []byte{"u8 + printByteSlice(o.Data) + "}}"u8;
}

[GoRecv] internal static Type realType(this ref ΔOPTResource r) {
    return TypeOPT;
}

[GoRecv] internal static (slice<byte>, error) pack(this ref ΔOPTResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) {
    foreach (var (_, opt) in r.Options) {
        msg = packUint16(msg, opt.Code);
        var l = (uint16)len(opt.Data);
        msg = packUint16(msg, l);
        msg = packBytes(msg, opt.Data);
    }
    return (msg, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string dnsmessageOPTResourceˢ = "dnsmessage.OPTResource{Options: []dnsmessage.Option{"u8;

// GoString implements fmt.GoStringer.GoString.
[GoRecv] public static @string GoString(this ref ΔOPTResource r) {
    @string s = dnsmessageOPTResourceˢ;
    if (len(r.Options) == 0) {
        return s + "}}"u8;
    }
    s += r.Options[0].GoString();
    foreach (var (_, vᴛ1) in r.Options[1..]) {
        var o = vᴛ1;

        s += ", "u8 + o.GoString();
    }
    return s + "}}"u8;
}

internal static (ΔOPTResource, error) unpackOPTResource(slice<byte> msg, nint off, uint16 length) {
    slice<Option> opts = default!;
    for (nint oldOff = off; off < oldOff + (nint)length; ) {
        error err = default!;
        Option o = default!;
        (o.Code, off, err) = unpackUint16(msg, off);
        if (err != default!) {
            return (new ΔOPTResource(nil), new nestedErrorжerror(Ꮡ(new nestedError("Code"u8, err))));
        }
        uint16 l = default!;
        (l, off, err) = unpackUint16(msg, off);
        if (err != default!) {
            return (new ΔOPTResource(nil), new nestedErrorжerror(Ꮡ(new nestedError("Data"u8, err))));
        }
        o.Data = new slice<byte>(l);
        if (copy(o.Data, msg.slice(off)) != (nint)l) {
            return (new ΔOPTResource(nil), new nestedErrorжerror(Ꮡ(new nestedError("Data"u8, errCalcLen))));
        }
        off += (nint)l;
        opts = append(opts, o);
    }
    return (new ΔOPTResource(opts), default!);
}

// An UnknownResource is a catch-all container for unknown record types.
[GoType] partial struct ΔUnknownResource {
    public Type Type;
    public slice<byte> Data;
}

[GoRecv] internal static Type realType(this ref ΔUnknownResource r) {
    return r.Type;
}

// pack appends the wire format of the UnknownResource to msg.
[GoRecv] internal static (slice<byte>, error) pack(this ref ΔUnknownResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) {
    return (packBytes(msg, r.Data[..]), default!);
}

// GoString implements fmt.GoStringer.GoString.
[GoRecv] public static @string GoString(this ref ΔUnknownResource r) {
    return "dnsmessage.UnknownResource{"u8 + "Type: "u8 + r.Type.GoString() + ", "u8 + "Data: []byte{"u8 + printByteSlice(r.Data) + "}}"u8;
}

internal static (ΔUnknownResource, error) unpackUnknownResource(Type recordType, slice<byte> msg, nint off, uint16 length) {
    var parsed = new ΔUnknownResource(
        Type: recordType,
        Data: new slice<byte>(length)
    );
    {
        var (_, err) = unpackBytes(msg, off, parsed.Data); if (err != default!) {
            return (new ΔUnknownResource(nil), err);
        }
    }
    return (parsed, default!);
}

} // end dnsmessage_package
