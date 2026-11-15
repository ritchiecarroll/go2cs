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
