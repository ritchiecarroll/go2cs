namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct hdr {
    internal uint16 family;
    internal uint16 pad;
    internal uint16 port;
    internal uint16 tail;
    internal uint64 word;
}

internal static readonly ж<hdr> Ꮡglobal = new StaticBox<hdr>(new hdr(port: 0x1234, tail: 0x5678, word: 0x1111111122222222UL));
internal static ref hdr global => ref ((StaticBox<hdr>)Ꮡglobal).ValueSlot;

internal static byte directRead(ж<hdr> h) {
    return @unsafe.ArrayPointer<byte>.Of(h.Ꮡport, 2).ElementRef(1);
}

internal static void directWrite(ж<hdr> h, byte v) {
    @unsafe.ArrayPointer<byte>.Of(h.Ꮡport, 2).ElementRef(0) = v;
}

internal static void directOpAssign(ж<hdr> h) {
    @unsafe.ArrayPointer<byte>.Of(h.Ꮡport, 2).ElementRef(1) += 2;
    @unsafe.ArrayPointer<byte>.Of(h.Ꮡport, 2).ElementRef(1)++;
}

internal static uint32 directDeref(ж<hdr> h) {
    @unsafe.ArrayPointer<uint32>.Of(h.Ꮡword, 2).ElementRef(0)++;
    return @unsafe.ArrayPointer<uint32>.Of(h.Ꮡword, 2).ElementRef(0);
}

internal static ж<byte> directAddr(ж<hdr> h) {
    return (@unsafe.ArrayPointer<byte>.Of(h.Ꮡport, 2)).at<byte>(1);
}

internal static nint directLen(ж<hdr> h) {
    return 2;
}

internal static slice<byte> directSlice(ж<hdr> h) {
    return @unsafe.ArrayPointer<byte>.Of(h.Ꮡport, 2).slice();
}

internal static uint32 directWord(ж<hdr> h) {
    return @unsafe.ArrayPointer<uint32>.Of(h.Ꮡword, 2).ElementRef(1);
}

internal static byte localRead(ж<hdr> h) {
    var p = @unsafe.ArrayPointer<byte>.Of(h.Ꮡport, 2);
    return p.ElementRef(1);
}

internal static void localWrite(ж<hdr> h, byte v) {
    var p = @unsafe.ArrayPointer<byte>.Of(h.Ꮡport, 2);
    p.ElementRef(0) = v;
}

internal static nint localRange(ж<hdr> h) {
    var p = @unsafe.ArrayPointer<byte>.Of(h.Ꮡport, 2);
    nint sum = 0;

    foreach (var i in range(p, 2)) {
        sum += ((nint)p.ElementRef(i)).Lsh((int64)(8 * i));
    }

    return sum;
}

internal static uint32 localWord(ж<hdr> h) {
    var t = @unsafe.ArrayPointer<uint32>.Of(h.Ꮡword, 2);
    var (s1, s0) = (t.ElementRef(0), t.ElementRef(1));
    s1 ^= (uint32)((s1 << 17));
    s1 = s1 ^ s0 ^ s1 >> 7 ^ s0 >> 16;
    (t.ElementRef(0), t.ElementRef(1)) = (s0, s1);
    return s0 + s1;
}

internal static slice<byte> localSlice(ж<hdr> h) {
    var p = @unsafe.ArrayPointer<byte>.Of(h.Ꮡport, 2);
    return p.slice();
}

internal static uint16 localPastTheField(ж<hdr> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    var p = @unsafe.ArrayPointer<byte>.Of(Ꮡh.Ꮡport, 4);
    p.ElementRef(3) = 0x9A;
    return h.tail;
}

internal static array<byte> wholeDirect(ж<hdr> h) {
    return @unsafe.ArrayPointer<byte>.Load(@unsafe.ArrayPointer<byte>.Of(h.Ꮡport, 2));
}

internal static array<byte> wholeLocal(ж<hdr> h) {
    var p = @unsafe.ArrayPointer<byte>.Of(h.Ꮡport, 2);
    return @unsafe.ArrayPointer<byte>.Load(p);
}

internal static array<uint32> wholeWord(ж<hdr> h) {
    return @unsafe.ArrayPointer<uint32>.Load(@unsafe.ArrayPointer<uint32>.Of(h.Ꮡword, 2));
}

internal static (byte, byte) wholeIsACopy(ж<hdr> h) {
    var p = @unsafe.ArrayPointer<byte>.Of(h.Ꮡport, 2);
    var a = @unsafe.ArrayPointer<byte>.Load(p);
    p.ElementRef(0)++;
    return (a[0], p.ElementRef(0));
}

internal static bool wholeCompare(ж<hdr> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    var p = @unsafe.ArrayPointer<byte>.Of(Ꮡh.Ꮡport, 2);
    return @unsafe.ArrayPointer<byte>.Load(p) == array<byte>([(byte)h.port, (byte)(h.port >> 8)]);
}

internal static nint sum2(/*[2]*/ array<byte> a) {
    a = a.Clone();

    return (nint)a[0] + (nint)a[1];
}

internal static nint wholeArgument(ж<hdr> h) {
    var p = @unsafe.ArrayPointer<byte>.Of(h.Ꮡport, 2);
    return sum2(@unsafe.ArrayPointer<byte>.Load(p));
}

internal static void wholeStoreDirect(ж<hdr> h, /*[2]*/ array<byte> v) {
    v = v.Clone();

    @unsafe.ArrayPointer<byte>.Store(@unsafe.ArrayPointer<byte>.Of(h.Ꮡport, 2), v);
}

internal static void wholeStoreLocal(ж<hdr> h, /*[2]*/ array<byte> v) {
    v = v.Clone();

    var p = @unsafe.ArrayPointer<byte>.Of(h.Ꮡport, 2);
    @unsafe.ArrayPointer<byte>.Store(p, v);
}

internal static nint rangeValue(ж<hdr> h) {
    var p = @unsafe.ArrayPointer<byte>.Of(h.Ꮡport, 2);
    nint sum = 0;

    foreach (var (_, b) in range(p.slice())) {
        sum += (nint)b;
    }

    return sum;
}

internal static nint rangeValueDirect(ж<hdr> h) {
    nint sum = 0;

    foreach (var (i, b) in range(@unsafe.ArrayPointer<byte>.Of(h.Ꮡport, 2).slice())) {
        sum += ((nint)b).Lsh((int64)(8 * i));
    }

    return sum;
}

internal static nint rangeValueSeesAStore(ж<hdr> h) {
    var p = @unsafe.ArrayPointer<byte>.Of(h.Ꮡport, 2);
    nint sum = 0;

    foreach (var (i, b) in range(p.slice())) {
        if (i == 0) {
            p.ElementRef(1) = 0x40;
        }

        sum += (nint)b;
    }

    return sum;
}

internal static nint rangeDerefIsACopy(ж<hdr> h) {
    var p = @unsafe.ArrayPointer<byte>.Of(h.Ꮡport, 2);
    nint sum = 0;

    foreach (var (i, b) in range(@unsafe.ArrayPointer<byte>.Load(p))) {
        if (i == 0) {
            p.ElementRef(1) = 0x50;
        }

        sum += (nint)b;
    }

    return sum;
}

internal static byte derefIndex(ж<hdr> h) {
    var p = @unsafe.ArrayPointer<byte>.Of(h.Ꮡport, 2);
    p.ElementRef(0) = 0x77;
    p.ElementRef(1)++;
    return p.ElementRef(0);
}

internal static slice<byte> derefSlice(ж<hdr> h) {
    var p = @unsafe.ArrayPointer<byte>.Of(h.Ꮡport, 2);
    return p.slice(0, 1);
}

partial struct raw {
    internal array<byte> port = new(2);
    internal array<byte> word = new(8);
}

internal static raw loadInLiteral(ж<hdr> h) {
    return new raw(
        port: @unsafe.ArrayPointer<byte>.Load(@unsafe.ArrayPointer<byte>.Of(h.Ꮡport, 2)),
        word: @unsafe.ArrayPointer<byte>.Load(@unsafe.ArrayPointer<byte>.Of(h.Ꮡword, 8))
    );
}

internal static array<byte> loadSent(ж<hdr> h) {
    var p = @unsafe.ArrayPointer<byte>.Of(h.Ꮡport, 2);
    var ch = new channel<array<byte>>(1, ChanCargo.Of(null, new nint[] { 2 }));
    ch.ᐸꟷ((@unsafe.ArrayPointer<byte>.Load(p)));
    return ᐸꟷ(ch);
}

internal static nint derefUnread(ж<hdr> h) {
    var p = @unsafe.ArrayPointer<byte>.Of(h.Ꮡport, 2);
    nint n = 2 * 10;

    foreach (var i in range(2)) {
        n += i;
    }

    return n;
}

internal static void arms(@string root, ж<hdr> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    (h.family, h.pad, h.port, h.tail, h.word) = (2, 0, 0x1234, 0x5678, 0x1111111122222222UL);

    fmt.Printf("%s direct read [1]: %#x\n"u8, root, directRead(Ꮡh));
    directWrite(Ꮡh, 0xAB);
    fmt.Printf("%s direct write [0]: port %#x\n"u8, root, h.port);
    directAddr(Ꮡh).Value = 0xCD;
    fmt.Printf("%s direct address: port %#x\n"u8, root, h.port);
    directOpAssign(Ꮡh);
    fmt.Printf("%s direct compound and step: port %#x\n"u8, root, h.port);
    fmt.Printf("%s direct explicit dereference: %#x, word %#x\n"u8, root, directDeref(Ꮡh), h.word);
    fmt.Printf("%s direct len: %d\n"u8, root, directLen(Ꮡh));
    var s = directSlice(Ꮡh);
    s[0] = 0x11;
    fmt.Printf("%s direct slice: len %d cap %d port %#x\n"u8, root, len(s), cap(s), h.port);
    fmt.Printf("%s direct word [1]: %#x\n"u8, root, directWord(Ꮡh));

    fmt.Printf("%s local read [1]: %#x\n"u8, root, localRead(Ꮡh));
    localWrite(Ꮡh, 0x22);
    fmt.Printf("%s local write [0]: port %#x\n"u8, root, h.port);
    fmt.Printf("%s local range by index: %#x\n"u8, root, localRange(Ꮡh));
    fmt.Printf("%s local word: %#x, word %#x\n"u8, root, localWord(Ꮡh), h.word);
    s = localSlice(Ꮡh);
    s[1] = 0x33;
    fmt.Printf("%s local slice: len %d cap %d port %#x\n"u8, root, len(s), cap(s), h.port);
    fmt.Printf("%s local view past the field: tail %#x\n"u8, root, localPastTheField(Ꮡh));
    (h.port, h.word) = (0x1234, 0x1111111122222222UL);
    fmt.Printf("%s whole load: %v %v\n"u8, root, wholeDirect(Ꮡh), wholeLocal(Ꮡh));
    fmt.Printf("%s whole load of a word: %#x\n"u8, root, wholeWord(Ꮡh));
    var (before, after) = wholeIsACopy(Ꮡh);
    fmt.Printf("%s whole load is a copy: %#x then %#x, port %#x\n"u8, root, before, after, h.port);
    fmt.Printf("%s whole load compared: %v, as an argument: %#x\n"u8, root, wholeCompare(Ꮡh), wholeArgument(Ꮡh));
    wholeStoreDirect(Ꮡh, array<byte>([0xA1, 0xA2]));
    fmt.Printf("%s whole store: port %#x\n"u8, root, h.port);
    wholeStoreLocal(Ꮡh, array<byte>([0xB1, 0xB2]));
    fmt.Printf("%s whole store through a local: port %#x, tail %#x\n"u8, root, h.port, h.tail);
    fmt.Printf("%s range by value: %#x, where written: %#x\n"u8, root, rangeValue(Ꮡh), rangeValueDirect(Ꮡh));
    fmt.Printf("%s range by value sees a store: %#x, port %#x\n"u8, root, rangeValueSeesAStore(Ꮡh), h.port);
    fmt.Printf("%s range of the dereference is a copy: %#x, port %#x\n"u8, root, rangeDerefIsACopy(Ꮡh), h.port);
    fmt.Printf("%s dereference indexed: %#x, port %#x\n"u8, root, derefIndex(Ꮡh), h.port);
    s = derefSlice(Ꮡh);
    s[0] = 0x66;
    fmt.Printf("%s dereference sliced: len %d cap %d port %#x\n"u8, root, len(s), cap(s), h.port);
    h.word = 0x0102030405060708UL;
    fmt.Printf("%s in a struct literal: %v, sent: %v\n"u8, root, loadInLiteral(Ꮡh), loadSent(Ꮡh));
    fmt.Printf("%s length and index range of the dereference: %d\n"u8, root, derefUnread(Ꮡh));
    fmt.Printf("%s nothing else moved: family %d pad %d\n"u8, root, h.family, h.pad);
}

internal static void main() {
    arms("heap", Ꮡ(new hdr()));

    arms("package", Ꮡglobal);

    var hs = new slice<hdr>(3);
    arms("element", Ꮡ(hs, 1));
    fmt.Println("its neighbours:", hs[0], hs[2]);

    var buf = new slice<byte>(32);
    arms("over bytes", Ꮡ(buf, 8).Reinterpret<byte, hdr>());
    fmt.Println("the bytes before and after it:", buf[..8], buf[24..]);
}

} // end main_package
