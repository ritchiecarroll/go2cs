namespace go;

using bytes = bytes_package;
using fmt = fmt_package;
using Δio = io_package;
using reflect = reflect_package;
using strings = strings_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static void expectPanic(@string label, @string want, Action f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var r = recover();
            @string msg = fmt.Sprint(r);
            fmt.Printf("%-16s panicked: %v  mentions %q: %v  text: %s\n"u8, label, r != default!, want, strings.Contains(msg, want), msg);
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] partial struct gA {
}

[GoType] partial struct gB<T> {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string setLen10ˢ = "SetLen(10)"u8;
private static readonly @string setLenˢ = "SetLen"u8;
private static readonly @string setCap10ˢ = "SetCap(10)"u8;
private static readonly @string setCapˢ = "SetCap"u8;
private static readonly @string setLen1ˢ = "SetLen(-1)"u8;
private static readonly @string setCap1ˢ = "SetCap(-1)"u8;
private static readonly @string setCap6Lenˢ = "SetCap(6)<len"u8;
private static readonly object afterSetLen5LenCapˢ = (@string)"after SetLen(5): len, cap ="u8;
private static readonly object afterSetCap6LenCapˢ = (@string)"after SetCap(6): len, cap ="u8;
private static readonly object afterSetCap5LenCapˢ = (@string)"after SetCap(5): len, cap ="u8;
private static readonly object contentsˢ = (@string)"contents"u8;
private static readonly @string setCap4Lenˢ = "SetCap(4)<len"u8;
private static readonly @string setLen6Capˢ = "SetLen(6)>cap"u8;
private static readonly @string arraySetLenˢ = "array SetLen"u8;
private static readonly @string arraySetCapˢ = "array SetCap"u8;
private static readonly object writeThroughTheReCappedˢ = (@string)"write through the re-capped window seen by the original:"u8;
private static readonly @string bytesOnIntˢ = "Bytes on int"u8;
private static readonly @string onIntValueˢ = "on int Value"u8;
private static readonly @string bytesStringˢ = "Bytes []string"u8;
private static readonly @string ofNonByteSliceˢ = "of non-byte slice"u8;
private static readonly object sBytesˢ = (@string)"S bytes:"u8;
private static readonly object aliasesXˢ = (@string)"aliases x:"u8;
private static readonly @string bytes4ByteValueˢ = "Bytes [4]byte value"u8;
private static readonly @string unaddressableˢ = "unaddressable"u8;
private static readonly @string bytes4Byteˢ = "Bytes *[4]byte"u8;
private static readonly @string onPtrValueˢ = "on ptr Value"u8;
private static readonly object aBytesˢ = (@string)"A bytes:"u8;
private static readonly object aliasesAˢ = (@string)"aliases a:"u8;
private static readonly object bBytesˢ = (@string)"[]B   bytes:"u8;
private static readonly object bBytesˢ2 = (@string)"*[4]B bytes:"u8;
private static readonly object sbBytesˢ = (@string)"SB    bytes:"u8;
private static readonly object abBytesˢ = (@string)"*AB   bytes:"u8;
private static readonly @string bytesAbValueˢ = "Bytes AB value"u8;
private static readonly object abBytesˢ2 = (@string)"AB bytes:"u8;
private static readonly object aliasesAbˢ = (@string)"aliases ab:"u8;
private static readonly @string bytes4Intˢ = "Bytes [4]int"u8;
private static readonly @string ofNonByteArrayˢ = "of non-byte array"u8;
private static readonly object nameGBGAˢ = (@string)"Name gB[gA]:"u8;
private static readonly object nameGBGBGAˢ = (@string)"Name gB[gB[gA]]:"u8;
private static readonly object stringGBGAˢ = (@string)"String gB[gA]:"u8;
private static readonly object namePlainGAˢ = (@string)"Name plain gA:"u8;
private static readonly object entryIsB2ˢ = (@string)"#5 entry is b2:"u8;
private static readonly object mapIndexB1Elemˢ = (@string)"#5 MapIndex(b1).Elem().UnsafePointer() == unsafe.Pointer(b2):"u8;
private static readonly object mapIndexB1UnsafePointerˢ = (@string)"#7 MapIndex(b1).UnsafePointer() == unsafe.Pointer(b2):"u8;
private static readonly object reflectTwiceˢ = (@string)"reflect twice:"u8;
private static readonly object reflectVsUnsafePointerNˢ = (@string)" reflect vs unsafe.Pointer(n):"u8;
private static readonly object differentBoxesˢ = (@string)"different boxes:"u8;
private static readonly object interiorBaseˢ = (@string)" interior != base:"u8;
private static readonly object baseBaseˢ = (@string)" base == base:"u8;
private static readonly object mapSameBoxTwiceFoundˢ = (@string)"map: same box twice found:"u8;
private static readonly object sameNumberTwiceFoundˢ = (@string)" same number twice found:"u8;
private static readonly object otherBoxFoundˢ = (@string)" other box found:"u8;

[GoLocalName("S")] [GoType("[]byte")] internal partial struct main_S;

[GoLocalName("A")] [GoType("[4]byte")] internal partial struct main_A;

[GoLocalName("B")] [GoType("num:byte")] internal partial struct main_B;

[GoLocalName("SB")] [GoType("[]main_B")] internal partial struct main_SB;

[GoLocalName("AB")] [GoType("[4]main_B")] internal partial struct main_AB;

[GoLocalName("MyBuffer")] [GoType("bytes_package.Buffer")] internal partial struct main_MyBuffer;

internal static void Main() {
    ref var xs = ref heap<slice<nint>>(out var Ꮡxs);
    xs = new nint[]{1, 2, 3, 4, 5, 6, 7, 8}.slice();
    ref var xa = ref heap<array<nint>>(out var Ꮡxa);
    xa = new nint[]{10, 20, 30, 40, 50, 60, 70, 80}.array();
    ref var vs = ref heap<reflectꓸValue>(out var Ꮡvs);
    vs = reflect.ValueOf(Ꮡxs).Elem();
    var vsʗ1 = vs;
    expectPanic(setLen10ˢ, setLenˢ, () => {
        vsʗ1.SetLen(10);
    });
    var vsʗ2 = vs;
    expectPanic(setCap10ˢ, setCapˢ, () => {
        vsʗ2.SetCap(10);
    });
    var vsʗ3 = vs;
    expectPanic(setLen1ˢ, setLenˢ, () => {
        vsʗ3.SetLen(-1);
    });
    var vsʗ4 = vs;
    expectPanic(setCap1ˢ, setCapˢ, () => {
        vsʗ4.SetCap(-1);
    });
    var vsʗ5 = vs;
    expectPanic(setCap6Lenˢ, setCapˢ, () => {
        vsʗ5.SetCap(6);
    });
    vs.SetLen(5);
    fmt.Println(afterSetLen5LenCapˢ, len(xs), cap(xs));
    vs.SetCap(6);
    fmt.Println(afterSetCap6LenCapˢ, len(xs), cap(xs));
    vs.SetCap(5);
    fmt.Println(afterSetCap5LenCapˢ, len(xs), cap(xs), contentsˢ, xs);
    var vsʗ6 = vs;
    expectPanic(setCap4Lenˢ, setCapˢ, () => {
        vsʗ6.SetCap(4);
    });
    var vsʗ7 = vs;
    expectPanic(setLen6Capˢ, setLenˢ, () => {
        vsʗ7.SetLen(6);
    });
    ref var va = ref heap<reflectꓸValue>(out var Ꮡva);
    va = reflect.ValueOf(Ꮡxa).Elem();
    var vaʗ1 = va;
    expectPanic(arraySetLenˢ, setLenˢ, () => {
        vaʗ1.SetLen(8);
    });
    var vaʗ2 = va;
    expectPanic(arraySetCapˢ, setCapˢ, () => {
        vaʗ2.SetCap(8);
    });
    var backing = xs[..(int)(cap(xs))];
    backing[0] = 99;
    fmt.Println(writeThroughTheReCappedˢ, xs[0] == 99);
    expectPanic(bytesOnIntˢ, onIntValueˢ, () => {
        reflect.ValueOf((nint)(0)).Bytes();
    });
    expectPanic(bytesStringˢ, ofNonByteSliceˢ, () => {
        reflect.ValueOf(new @string[]{}.slice()).Bytes();
    });
    var x = new main_S(new byte[]{1, 2, 3, 4}.slice());
    var y = reflect.ValueOf(x).Bytes();
    y[0] = 42;
    fmt.Println(sBytesˢ, y, aliasesXˢ, x[0] == 42);
    ref var a = ref heap<main_A>(out var Ꮡa);
    a = new main_A(new byte[]{1, 2, 3, 4}.array());
    expectPanic(bytes4ByteValueˢ, unaddressableˢ, () => {
        reflect.ValueOf(Ꮡa.Value).Bytes();
    });
    expectPanic(bytes4Byteˢ, onPtrValueˢ, () => {
        reflect.ValueOf(Ꮡa).Bytes();
    });
    var b = reflect.ValueOf(Ꮡa).Elem().Bytes();
    b[1] = 43;
    fmt.Println(aBytesˢ, b, aliasesAˢ, a[1] == 43);
    fmt.Println(bBytesˢ, reflect.ValueOf(new main_B[]{1, 2, 3, 4}.slice()).Bytes());
    fmt.Println(bBytesˢ2, reflect.ValueOf(Ꮡ(new array<main_B>(4))).Elem().Bytes());
    fmt.Println(sbBytesˢ, reflect.ValueOf(new main_SB(new main_B[]{1, 2, 3, 4}.slice())).Bytes());
    fmt.Println(abBytesˢ, reflect.ValueOf(@new<main_AB>()).Elem().Bytes());
    ref var ab = ref heap<main_AB>(out var Ꮡab);
    ab = new main_AB(new main_B[]{5, 6, 7, 8}.array());
    expectPanic(bytesAbValueˢ, unaddressableˢ, () => {
        reflect.ValueOf(Ꮡab.Value).Bytes();
    });
    var c = reflect.ValueOf(Ꮡab).Elem().Bytes();
    c[2] = 44;
    fmt.Println(abBytesˢ2, c, aliasesAbˢ, ab[2] == 44);
    expectPanic(bytes4Intˢ, ofNonByteArrayˢ, () => {
        reflect.ValueOf(Ꮡ(new nint[]{}.array(4))).Elem().Bytes();
    });
    fmt.Println(nameGBGAˢ, reflect.TypeOf(@new<gB<gA>>()).Elem().Name());
    fmt.Println(nameGBGBGAˢ, reflect.TypeOf(@new<gB<gB<gA>>>()).Elem().Name());
    fmt.Println(stringGBGAˢ, reflect.TypeOf(new gB<gA>(nil)).String());
    fmt.Println(namePlainGAˢ, reflect.TypeOf(new gA(nil)).Name());
    var m5 = new map<Δio.Reader, Δio.Writer>();
    var mv5 = reflect.ValueOf(m5);
    var (b1, b2) = (@new<bytes.Buffer>(), @new<bytes.Buffer>());
    mv5.SetMapIndex(reflect.ValueOf(b1.OrTypedNil()), reflect.ValueOf(b2.OrTypedNil()));
    var (x5, ok5) = m5[new bytes_BufferжReader(b1), ꟷ];
    fmt.Println(entryIsB2ˢ, AreEqual(x5, b2), ok5);
    @unsafe.Pointer p5 = (uintptr)mv5.MapIndex(reflect.ValueOf(b1.OrTypedNil())).Elem().UnsafePointer();
    fmt.Println(mapIndexB1Elemˢ, p5 == new @unsafe.Pointer(b2));
    var m7 = new map<ж<main_MyBuffer>, ж<bytes.Buffer>>();
    var mv7 = reflect.ValueOf(m7);
    var (k7, v7) = (@new<main_MyBuffer>(), @new<bytes.Buffer>());
    mv7.SetMapIndex(reflect.ValueOf(k7.OrTypedNil()), reflect.ValueOf(v7.OrTypedNil()));
    @unsafe.Pointer p7 = (uintptr)mv7.MapIndex(reflect.ValueOf(k7.OrTypedNil())).UnsafePointer();
    fmt.Println(mapIndexB1UnsafePointerˢ, p7 == new @unsafe.Pointer(v7));
    var n = @new<nint>();
    @unsafe.Pointer pn1 = (uintptr)reflect.ValueOf(n.OrTypedNil()).UnsafePointer();
    @unsafe.Pointer pn2 = (uintptr)reflect.ValueOf(n.OrTypedNil()).UnsafePointer();
    fmt.Println(reflectTwiceˢ, pn1 == pn2, reflectVsUnsafePointerNˢ, pn1 == new @unsafe.Pointer(n));
    var n2 = @new<nint>();
    ref var arr = ref heap(new array<int64>(4), out var Ꮡarr);
    fmt.Println(differentBoxesˢ, pn1 == new @unsafe.Pointer(n2), interiorBaseˢ, (uintptr)@unsafe.Add(new @unsafe.Pointer(Ꮡarr), 8) != new @unsafe.Pointer(Ꮡarr), baseBaseˢ, new @unsafe.Pointer(Ꮡarr) == new @unsafe.Pointer(Ꮡarr));
    var keyed = new map<@unsafe.Pointer, nint>{[(uintptr)reflect.ValueOf(n.OrTypedNil()).UnsafePointer()] = 1};
    var (_, foundBox) = keyed[new @unsafe.Pointer(n), ꟷ];
    var interior = new map<@unsafe.Pointer, nint>{[(uintptr)@unsafe.Add(new @unsafe.Pointer(Ꮡarr), 8)] = 2};
    var (_, foundNumber) = interior[(uintptr)@unsafe.Add(new @unsafe.Pointer(Ꮡarr), 8), ꟷ];
    var (_, missOther) = keyed[new @unsafe.Pointer(n2), ꟷ];
    fmt.Println(mapSameBoxTwiceFoundˢ, foundBox, sameNumberTwiceFoundˢ, foundNumber, otherBoxFoundˢ, missOther);
}

} // end main_package
