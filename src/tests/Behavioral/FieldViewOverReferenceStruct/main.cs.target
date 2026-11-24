namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct rawInet4 {
    internal uint16 family;
    internal uint16 port;
    internal array<byte> addr = new(4);
    internal array<uint8> zero = new(8);
}

partial struct pair {
    internal uint16 a;
    internal uint16 b;
}

partial struct record {
    internal @string label;
    internal uint32 word;
    internal pair both;
    internal uint16 tail;
}

internal static readonly ж<rawInet4> Ꮡglobal = new StaticBox<rawInet4>(new rawInet4(family: 2, port: 0x1234, addr: array<byte>([10, 0, 0, 1])));
internal static ref rawInet4 global => ref ((StaticBox<rawInet4>)Ꮡglobal).ValueSlot;

internal static void encodePort(ж<rawInet4> raw, nint port) {
    var p = @unsafe.ArrayPointer<byte>.Of(raw.Ꮡport, 2);
    p.ElementRef(0) = (byte)(port >> 8);
    p.ElementRef(1) = (byte)port;
}

internal static nint decodePort(ж<rawInet4> raw) {
    var p = @unsafe.ArrayPointer<byte>.Of(raw.Ꮡport, 2);
    return ((nint)p.ElementRef(0) << 8) + (nint)p.ElementRef(1);
}

internal static byte directRead(ж<rawInet4> raw) {
    return @unsafe.ArrayPointer<byte>.Of(raw.Ꮡport, 2).ElementRef(1);
}

internal static void directWrite(ж<rawInet4> raw, byte v) {
    @unsafe.ArrayPointer<byte>.Of(raw.Ꮡport, 2).ElementRef(0) = v;
}

internal static void directOpAssign(ж<rawInet4> raw) {
    @unsafe.ArrayPointer<byte>.Of(raw.Ꮡport, 2).ElementRef(1) += 2;
    @unsafe.ArrayPointer<byte>.Of(raw.Ꮡport, 2).ElementRef(1)++;
}

internal static nint directLen(ж<rawInet4> raw) {
    return 2;
}

internal static (byte, byte, uint16) aliasBothWays(ж<rawInet4> Ꮡraw) {
    ref var raw = ref Ꮡraw.DerefOrNull();

    var p = @unsafe.ArrayPointer<byte>.Of(Ꮡraw.Ꮡport, 2);
    raw.port = 0xBEEF;
    var (lo, hi) = (p.ElementRef(0), p.ElementRef(1));
    p.ElementRef(1) = 0x01;
    return (lo, hi, raw.port);
}

internal static byte twoViews(ж<rawInet4> raw) {
    var p = @unsafe.ArrayPointer<byte>.Of(raw.Ꮡport, 2);
    var q = @unsafe.ArrayPointer<byte>.Of(raw.Ꮡport, 2);
    q.ElementRef(0) = 0x77;
    return p.ElementRef(0);
}

internal static (byte, byte) wordBytes(ж<record> r) {
    var p = @unsafe.ArrayPointer<byte>.Of(r.Ꮡword, 4);
    p.ElementRef(3) = 0x7F;
    return (p.ElementRef(0), p.ElementRef(3));
}

internal static uint16 structHalves(ж<record> r) {
    var p = @unsafe.ArrayPointer<uint16>.Of(r.Ꮡboth, 2);
    p.ElementRef(1) = 0x3333;
    return p.ElementRef(0);
}

internal static byte structBytes(ж<record> r) {
    var p = @unsafe.ArrayPointer<byte>.Of(r.Ꮡboth, 4);
    return p.ElementRef(3);
}

internal static array<byte> wholeLoad(ж<rawInet4> raw) {
    var p = @unsafe.ArrayPointer<byte>.Of(raw.Ꮡport, 2);
    var v = @unsafe.ArrayPointer<byte>.Load(p);
    p.ElementRef(0) = 0xAB;
    return v.Clone();
}

internal static void wholeStore(ж<rawInet4> raw, /*[2]*/ array<byte> v) {
    v = v.Clone();

    var p = @unsafe.ArrayPointer<byte>.Of(raw.Ꮡport, 2);
    @unsafe.ArrayPointer<byte>.Store(p, v);
}

internal static nint countRounds(ж<rawInet4> raw) {
    var p = @unsafe.ArrayPointer<byte>.Of(raw.Ꮡport, 2);
    nint n = 0;

    foreach (var i in range(p, 2)) {
        n += i + 1;
    }

    return n + 2;
}

internal static (byte, byte, uint16) storeCollectRead(ж<rawInet4> Ꮡraw, byte v) {
    ref var raw = ref Ꮡraw.DerefOrNull();

    var p = @unsafe.ArrayPointer<byte>.Of(Ꮡraw.Ꮡport, 2);
    p.ElementRef(0) = v;
    p.ElementRef(1) = (byte)(v + 1);
    runtime.GC();
    var (lo, hi) = (p.ElementRef(0), p.ElementRef(1));
    p.ElementRef(1) = 0x5A;
    runtime.GC();
    return (lo, hi, raw.port);
}

internal static bool sameAsItsField(ж<rawInet4> raw) {
    var p = @unsafe.ArrayPointer<byte>.Of(raw.Ꮡport, 2);
    return @unsafe.Pointer.FromPinnedBox(p) == @unsafe.Pointer.FromPinnedBox(raw.Ꮡport);
}

internal static bool sameAsAnotherField(ж<rawInet4> raw) {
    var p = @unsafe.ArrayPointer<byte>.Of(raw.Ꮡport, 2);
    return @unsafe.Pointer.FromPinnedBox(p) == @unsafe.Pointer.FromPinnedBox(raw.Ꮡfamily);
}

internal static bool sameAcrossStructs(ж<rawInet4> x, ж<rawInet4> y) {
    var p = @unsafe.ArrayPointer<byte>.Of(x.Ꮡport, 2);
    var q = @unsafe.ArrayPointer<byte>.Of(y.Ꮡport, 2);
    return p == q || @unsafe.Pointer.FromPinnedBox(p) == @unsafe.Pointer.FromPinnedBox(y.Ꮡport);
}

internal static bool twoViewsAreOnePointer(ж<rawInet4> raw) {
    var p = @unsafe.ArrayPointer<byte>.Of(raw.Ꮡport, 2);
    var q = @unsafe.ArrayPointer<byte>.Of(raw.Ꮡport, 2);
    var f = @unsafe.ArrayPointer<byte>.Of(raw.Ꮡfamily, 2);
    return p == q && p != f && p != nil;
}

partial struct sockInet4 {
    internal nint port;
    internal array<byte> addr = new(4);
    internal rawInet4 raw;
}

internal static void sockaddr(this ж<sockInet4> Ꮡsa) {
    ref var sa = ref Ꮡsa.DerefOrNull();

    var p = @unsafe.ArrayPointer<byte>.Of(Ꮡsa.Ꮡraw.Ꮡport, 2);
    p.ElementRef(0) = (byte)(sa.port >> 8);
    p.ElementRef(1) = (byte)sa.port;
    sa.raw.addr.Assign(sa.addr);
}

internal static nint anyToPort(ж<sockInet4> sa) {
    var p = @unsafe.ArrayPointer<byte>.Of(sa.Ꮡraw.Ꮡport, 2);
    return ((nint)p.ElementRef(0) << 8) + (nint)p.ElementRef(1);
}

internal static byte twoDeepDirect(ж<sockInet4> sa) {
    @unsafe.ArrayPointer<byte>.Of(sa.Ꮡraw.Ꮡport, 2).ElementRef(1)++;
    return @unsafe.ArrayPointer<byte>.Of(sa.Ꮡraw.Ꮡport, 2).ElementRef(1);
}

internal static uint16 variableRoot() {
    ref var s = ref heap(new rawInet4(), out var Ꮡs);
    var p = @unsafe.ArrayPointer<byte>.Of(Ꮡs.Ꮡport, 2);
    p.ElementRef(0) = 0xCD;
    p.ElementRef(1) = 0x12;
    return s.port;
}

internal static uint16 packageRoot() {
    var p = @unsafe.ArrayPointer<byte>.Of(Ꮡglobal.Ꮡport, 2);
    p.ElementRef(1) = 0x77;
    return global.port;
}

internal static @string arrayFieldOfVariable() {
    ref var raw = ref heap(new rawInet4(), out var Ꮡraw);
    raw.addr.Assign(array<byte>([0x7f, 0xff, 0, 1]));
    var p = @unsafe.ArrayPointer<int8>.Of(Ꮡraw.Ꮡaddr, 4);
    p.ElementRef(3) = (int8)(-2);
    var s = @unsafe.ArrayPointer<int8>.Of(Ꮡraw.Ꮡaddr, 4).slice();
    return fmt.Sprint(p.ElementRef(0), p.ElementRef(1), s[3], raw.addr[3], len(s));
}

internal static @string /*text*/ outOfRange(ж<rawInet4> raw, nint i) {
    @string text = default;
    GoFrame ᒐ = default;
    try {
        defer([GoFunc("outOfRange.func1")] () => {
            text = fmt.Sprint(recover());
        }, ref ᒐ);

        var p = @unsafe.ArrayPointer<byte>.Of(raw.Ꮡport, 2);
        text = fmt.Sprint(p.ElementRef(i));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return text;
}

internal static void main() {
    var raw = Ꮡ(new rawInet4(family: 2, port: 0x1234, addr: array<byte>([127, 0, 0, 1])));

    encodePort(raw, 8080);
    fmt.Printf("encode: port=%#04x decode=%d family=%d addr=%v\n"u8, raw.port, decodePort(raw), raw.family, raw.addr);

    raw.port = 0x1234;
    fmt.Printf("direct read: %#02x\n"u8, directRead(raw));
    directWrite(raw, 0xCD);
    directOpAssign(raw);
    fmt.Printf("direct write: port=%#04x len=%d\n"u8, raw.port, directLen(raw));

    var (lo, hi, port) = aliasBothWays(raw);
    fmt.Printf("alias: %#02x %#02x port=%#04x\n"u8, lo, hi, port);
    fmt.Printf("two views: %#02x port=%#04x\n"u8, twoViews(raw), raw.port);

    var r = Ꮡ(new record(label: "r", word: 0x0A0B0C0D, both: new pair(a: 0x1111, b: 0x2222), tail: 0x5678));
    var (b0, b3) = wordBytes(r);
    fmt.Printf("word: %#02x %#02x word=%#08x\n"u8, b0, b3, r.word);
    fmt.Printf("struct field: %#04x b=%#04x byte=%#02x tail=%#04x label=%s\n"u8, structHalves(r), r.both.b, structBytes(r), r.tail, r.label);

    raw.port = 0x1234;
    var v = wholeLoad(raw);
    fmt.Printf("whole load: %#02x %#02x port=%#04x\n"u8, v[0], v[1], raw.port);
    wholeStore(raw, array<byte>([1, 2]));
    fmt.Printf("whole store: port=%#04x family=%d addr=%v rounds=%d\n"u8, raw.port, raw.family, raw.addr, countRounds(raw));

    (lo, hi, port) = storeCollectRead(raw, 0x41);
    fmt.Printf("collect: %#02x %#02x port=%#04x addr=%v\n"u8, lo, hi, port, raw.addr);

    var other = Ꮡ(new rawInet4(port: 0x1234));
    fmt.Printf("address: field=%v otherField=%v otherStruct=%v twoViews=%v\n"u8, sameAsItsField(raw), sameAsAnotherField(raw), sameAcrossStructs(raw, other), twoViewsAreOnePointer(raw));

    encodePort(Ꮡglobal, 443);
    fmt.Printf("global: port=%#04x decode=%d addr=%v\n"u8, global.port, decodePort(Ꮡglobal), global.addr);

    var structs = slice<rawInet4>([new(port: 1), new(port: 2)]);
    encodePort(Ꮡ(structs, 1), 0x0102);
    fmt.Printf("element: %#04x %#04x\n"u8, structs[0].port, structs[1].port);

    var sa = Ꮡ(new sockInet4(port: 8443, addr: array<byte>([192, 168, 0, 1])));
    sa.sockaddr();
    fmt.Printf("two deep: raw.port=%#04x decode=%d raw.addr=%v\n"u8, sa.raw.port, anyToPort(sa), sa.raw.addr);
    fmt.Printf("two deep direct: %#02x raw.port=%#04x\n"u8, twoDeepDirect(sa), sa.raw.port);
    fmt.Printf("variable: %#04x package: %#04x\n"u8, variableRoot(), packageRoot());
    fmt.Printf("array field of a variable: %s\n"u8, arrayFieldOfVariable());

    fmt.Printf("out of range: %s\n"u8, outOfRange(raw, 2));
}

} // end main_package
