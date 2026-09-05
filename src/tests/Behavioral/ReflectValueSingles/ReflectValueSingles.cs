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

[GoType("num:nint")] partial struct integer;

[GoType("[]byte")] partial struct MyBytes;

[GoType("[0]byte")] partial struct MyBytesArray0;

[GoType("[4]byte")] partial struct MyBytesArray;

[GoType("ж<array<byte>>")] partial class MyBytesArrayPtr0;

[GoType("ж<array<byte>>")] partial class MyBytesArrayPtr;

[GoType("bytes_package.Buffer")] partial struct MyBuffer;

internal static void convRow(@string label, any x, any want) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Printf("%-34s PANIC: %v\n"u8, label, r);
                }
            }
        }, ref ᒐ);
        var v = reflect.ValueOf(x).Convert(reflect.TypeOf(want));
        fmt.Printf("%-34s type==%v deepEqual=%v nil=%v\n"u8, label, AreEqual(v.Type(), reflect.TypeOf(want)), reflect.DeepEqual(v.Interface(), want), v.Kind() == reflect.Ptr && v.IsNil());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("chan nint")] partial struct IntChan;

[GoType("chan nint")] partial struct IntChanRecv;

[GoType("chan nint")] partial struct IntChanSend;

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
private static readonly object sameFieldTwiceˢ = (@string)"same field twice:"u8;
private static readonly object fieldVsOtherBoxˢ = (@string)" field vs other box:"u8;
private static readonly object mapSameFieldTwiceFoundˢ = (@string)" map: same field twice found:"u8;
private static readonly @string byteNil0Byteˢ = "[]byte(nil) -> *[0]byte"u8;
private static readonly @string byte0Byteˢ = "[]byte{} -> *[0]byte"u8;
private static readonly @string byte71Byteˢ = "[]byte{7} -> *[1]byte"u8;
private static readonly @string myBytes91Byteˢ = "MyBytes{9} -> *[1]byte"u8;
private static readonly @string byte1234MyBytesArrayPtrˢ = "[]byte{1,2,3,4} -> MyBytesArrayPtr"u8;
private static readonly @string byteNilMyBytesArrayPtr0ˢ = "[]byte(nil) -> MyBytesArrayPtr0"u8;
private static readonly @string byte1234MyBytesArrayˢ = "[]byte{1,2,3,4} -> *MyBytesArray"u8;
private static readonly @string byteNilMyBytesArray0ˢ = "[]byte(nil) -> *MyBytesArray0"u8;
private static readonly @string new0ByteMyBytesArray0ˢ = "new([0]byte) -> *MyBytesArray0"u8;
private static readonly @string newMyBytesArray00Byteˢ = "new(MyBytesArray0) -> *[0]byte"u8;
private static readonly @string myBytesArrayPtr0Nil0Byteˢ = "MyBytesArrayPtr0(nil) -> *[0]byte"u8;
private static readonly @string byteNilMyBytesArrayPtr0ˢ2 = "(*[0]byte)(nil) -> MyBytesArrayPtr0"u8;
private static readonly @string newIntIntegerˢ = "new(int) -> *integer"u8;
private static readonly @string newIntegerIntˢ = "new(integer) -> *int"u8;
private static readonly @string myBufferBytesBufferˢ = "*MyBuffer -> *bytes.Buffer"u8;
private static readonly object arrayPointerAliasesTheˢ = (@string)"array pointer aliases the slice:"u8;
private static readonly @string convertShortSliceˢ = "Convert short slice"u8;
private static readonly @string cannotConvertSliceWithˢ = "cannot convert slice with length 4 to pointer to array with length 8"u8;
private static readonly @string intChanNilChanIntˢ = "IntChan(nil) -> chan<- int"u8;
private static readonly @string intChanNilChanIntˢ2 = "IntChan(nil) -> <-chan int"u8;
private static readonly @string chanIntNilIntChanRecvˢ = "chan int(nil) -> IntChanRecv"u8;
private static readonly @string chanIntNilIntChanSendˢ = "chan int(nil) -> IntChanSend"u8;
private static readonly @string intChanRecvNilChanIntˢ = "IntChanRecv(nil) -> <-chan int"u8;
private static readonly @string chanIntNilIntChanRecvˢ2 = "<-chan int(nil) -> IntChanRecv"u8;
private static readonly @string intChanSendNilChanIntˢ = "IntChanSend(nil) -> chan<- int"u8;
private static readonly @string chanIntNilIntChanSendˢ2 = "chan<- int(nil) -> IntChanSend"u8;
private static readonly @string intChanNilChanIntˢ3 = "IntChan(nil) -> chan int"u8;
private static readonly object convertedSendOnlyViewˢ = (@string)"converted send-only view type:"u8;
private static readonly object interfaceTypeˢ = (@string)" interface type:"u8;
private static readonly object receivedOnTheOriginalˢ = (@string)" received on the original:"u8;

[GoLocalName("S")] [GoType("[]byte")] internal partial struct main_S;

[GoLocalName("A")] [GoType("[4]byte")] internal partial struct main_A;

[GoLocalName("B")] [GoType("num:byte")] internal partial struct main_B;

[GoLocalName("SB")] [GoType("[]main_B")] internal partial struct main_SB;

[GoLocalName("AB")] [GoType("[4]main_B")] internal partial struct main_AB;

[GoLocalName("MyBuffer")] [GoType("bytes_package.Buffer")] internal partial struct main_MyBuffer;

[GoType("dyn")] internal partial struct main_holder {
    internal ж<nint> p;
}

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
    ref var h = ref heap(new main_holder(), out var Ꮡh);
    @unsafe.Pointer fp = @unsafe.Pointer.FromBox(Ꮡh.of(main_holder.Ꮡp));
    var fieldKeyed = new map<@unsafe.Pointer, nint>{[fp] = 3};
    var (_, foundField) = fieldKeyed[@unsafe.Pointer.FromBox(Ꮡh.of(main_holder.Ꮡp)), ꟷ];
    fmt.Println(sameFieldTwiceˢ, fp == @unsafe.Pointer.FromBox(Ꮡh.of(main_holder.Ꮡp)), fieldVsOtherBoxˢ, fp == new @unsafe.Pointer(n), mapSameFieldTwiceFoundˢ, foundField);
    convRow(byteNil0Byteˢ, slice<byte>(default!), ж<array<byte>>.NilBoxOfDims(0L));
    convRow(byte0Byteˢ, new byte[]{}.slice(), Ꮡ(new array<byte>(0)));
    convRow(byte71Byteˢ, new byte[]{7}.slice(), Ꮡ(new byte[]{7}.array()));
    convRow(myBytes91Byteˢ, ((MyBytes)new byte[]{9}.slice()), Ꮡ(new byte[]{9}.array()));
    convRow(byte1234MyBytesArrayPtrˢ, new byte[]{1, 2, 3, 4}.slice(), new MyBytesArrayPtr(Ꮡ(new byte[]{1, 2, 3, 4}.array())));
    convRow(byteNilMyBytesArrayPtr0ˢ, slice<byte>(default!), ((MyBytesArrayPtr0)nil));
    convRow(byte1234MyBytesArrayˢ, new byte[]{1, 2, 3, 4}.slice(), Ꮡ(new MyBytesArray(new byte[]{1, 2, 3, 4}.array())));
    convRow(byteNilMyBytesArray0ˢ, slice<byte>(default!), ж<MyBytesArray0>.NilBoxOfDims(0L));
    convRow(new0ByteMyBytesArray0ˢ, Ꮡ(new array<byte>(0)), @new<MyBytesArray0>());
    convRow(newMyBytesArray00Byteˢ, @new<MyBytesArray0>(), Ꮡ(new array<byte>(0)));
    convRow(myBytesArrayPtr0Nil0Byteˢ, ((MyBytesArrayPtr0)nil), ж<array<byte>>.NilBoxOfDims(0L));
    convRow(byteNilMyBytesArrayPtr0ˢ2, ж<array<byte>>.NilBoxOfDims(0L), ((MyBytesArrayPtr0)nil));
    convRow(newIntIntegerˢ, @new<nint>(), @new<integer>());
    convRow(newIntegerIntˢ, @new<integer>(), @new<nint>());
    convRow(myBufferBytesBufferˢ, @new<main_MyBuffer>(), @new<bytes.Buffer>());
    var src = new byte[]{1, 2, 3, 4}.slice();
    var ap = reflect.ValueOf(src).Convert(reflect.TypeOf(ж<array<byte>>.NilBoxOfDims(4L))).Interface()._<ж<array<byte>>>();
    ap.Value[2] = 99;
    fmt.Println(arrayPointerAliasesTheˢ, src[2] == 99);
    ref var sh = ref heap<reflectꓸValue>(out var Ꮡsh);
    sh = reflect.ValueOf(new byte[]{1, 2, 3, 4}.slice());
    var shʗ1 = sh;
    expectPanic(convertShortSliceˢ, cannotConvertSliceWithˢ, () => {
        shʗ1.Convert(reflect.TypeOf(ж<array<byte>>.NilBoxOfDims(8L)));
    });
    convRow(intChanNilChanIntˢ, ((IntChan)default!), channel/*<-*/<nint>.SendOnly);
    convRow(intChanNilChanIntˢ2, ((IntChan)default!), /*<-*/channel<nint>.RecvOnly);
    convRow(chanIntNilIntChanRecvˢ, (channel<nint>)(default!), ((IntChanRecv)default!));
    convRow(chanIntNilIntChanSendˢ, (channel<nint>)(default!), ((IntChanSend)default!));
    convRow(intChanRecvNilChanIntˢ, ((IntChanRecv)default!), /*<-*/channel<nint>.RecvOnly);
    convRow(chanIntNilIntChanRecvˢ2, /*<-*/channel<nint>.RecvOnly, ((IntChanRecv)default!));
    convRow(intChanSendNilChanIntˢ, ((IntChanSend)default!), channel/*<-*/<nint>.SendOnly);
    convRow(chanIntNilIntChanSendˢ2, channel/*<-*/<nint>.SendOnly, ((IntChanSend)default!));
    convRow(intChanNilChanIntˢ3, ((IntChan)default!), (channel<nint>)(default!));
    var live = new IntChan(1);
    var sendOnly = reflect.ValueOf(live).Convert(reflect.TypeOf(channel/*<-*/<nint>.SendOnly));
    sendOnly.Interface()._<channel/*<-*/<nint>>().ᐸꟷ(7);
    fmt.Println(convertedSendOnlyViewˢ, sendOnly.Type(), interfaceTypeˢ, reflect.TypeOf(sendOnly.Interface()), receivedOnTheOriginalˢ, ᐸꟷ<nint>(live));
}

} // end main_package
