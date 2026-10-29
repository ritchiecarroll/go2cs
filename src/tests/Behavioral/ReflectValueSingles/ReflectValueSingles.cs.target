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
            fmt.Printf("%-16s panicked: %v  mentions %q: %v  text: %s\n"u8, label, r != nil, want, strings.Contains(msg, want), msg);
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct gA {
}

partial struct gB<T> {
}

partial struct integer /*num:nint*/;

partial struct MyBytes /*[]byte*/;

partial struct MyBytesArray0 /*[0]byte*/;

partial struct MyBytesArray /*[4]byte*/;

/*[0]*/ partial class MyBytesArrayPtr0 /*ж<array<byte>>*/;

/*[4]*/ partial class MyBytesArrayPtr /*ж<array<byte>>*/;

partial struct MyBuffer /*global::go.bytes_package.Buffer*/;

internal static void convRow(@string label, any x, any want) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != nil) {
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

partial struct IntChan /*chan nint*/;

partial struct IntChanRecv /*<-chan nint*/;

partial struct IntChanSend /*chan<- nint*/;

internal partial struct main_S /*[]byte*/;

internal partial struct main_A /*[4]byte*/;

internal partial struct main_B /*num:byte*/;

internal partial struct main_SB /*[]main_B*/;

internal partial struct main_AB /*[4]main_B*/;

internal partial struct main_MyBuffer /*global::go.bytes_package.Buffer*/;

internal partial struct main_holder /*dyn*/ {
    internal ж<nint> p;
}

internal partial struct main_chanSlots /*dyn*/ {
    public IntChanRecv R;
    public IntChanSend S;
}

internal partial struct main_ptrSlots /*dyn*/ {
    public MyBytesArrayPtr P;
    public MyBytesArrayPtr0 Z;
}

internal static void Main() {
    ref var xs = ref heap<slice<nint>>(slice<nint>([1, 2, 3, 4, 5, 6, 7, 8]), out var Ꮡxs);
    ref var xa = ref heap<array<nint>>(new nint[]{10, 20, 30, 40, 50, 60, 70, 80}.array(), out var Ꮡxa);
    ref var vs = ref heap<reflectꓸValue>(reflect.ValueOf(Ꮡxs).Elem(), out var Ꮡvs);
    var vsʗ1 = vs;
    expectPanic("SetLen(10)", "SetLen", () => {
        vsʗ1.SetLen(10);
    });
    var vsʗ2 = vs;
    expectPanic("SetCap(10)", "SetCap", () => {
        vsʗ2.SetCap(10);
    });
    var vsʗ3 = vs;
    expectPanic("SetLen(-1)", "SetLen", () => {
        vsʗ3.SetLen(-1);
    });
    var vsʗ4 = vs;
    expectPanic("SetCap(-1)", "SetCap", () => {
        vsʗ4.SetCap(-1);
    });
    var vsʗ5 = vs;
    expectPanic("SetCap(6)<len", "SetCap", () => {
        vsʗ5.SetCap(6);
    });
    vs.SetLen(5);
    fmt.Println("after SetLen(5): len, cap =", len(xs), cap(xs));
    vs.SetCap(6);
    fmt.Println("after SetCap(6): len, cap =", len(xs), cap(xs));
    vs.SetCap(5);
    fmt.Println("after SetCap(5): len, cap =", len(xs), cap(xs), "contents", xs);
    var vsʗ6 = vs;
    expectPanic("SetCap(4)<len", "SetCap", () => {
        vsʗ6.SetCap(4);
    });
    var vsʗ7 = vs;
    expectPanic("SetLen(6)>cap", "SetLen", () => {
        vsʗ7.SetLen(6);
    });
    ref var va = ref heap<reflectꓸValue>(reflect.ValueOf(Ꮡxa).Elem(), out var Ꮡva);
    var vaʗ1 = va;
    expectPanic("array SetLen", "SetLen", () => {
        vaʗ1.SetLen(8);
    });
    var vaʗ2 = va;
    expectPanic("array SetCap", "SetCap", () => {
        vaʗ2.SetCap(8);
    });
    var backing = xs.slice(0, cap(xs));
    backing[0] = 99;
    fmt.Println("write through the re-capped window seen by the original:", xs[0] == 99);
    expectPanic("Bytes on int", "on int Value", () => {
        reflect.ValueOf((nint)0).Bytes();
    });
    expectPanic("Bytes []string", "of non-byte slice", () => {
        reflect.ValueOf(slice<@string>([])).Bytes();
    });
    var x = new main_S(new byte[]{1, 2, 3, 4}.slice());
    var y = reflect.ValueOf(x).Bytes();
    y[0] = 42;
    fmt.Println("S bytes:", y, "aliases x:", x[0] == 42);
    ref var a = ref heap<main_A>(new main_A(new byte[]{1, 2, 3, 4}.array()), out var Ꮡa);
    expectPanic("Bytes [4]byte value", "unaddressable", () => {
        reflect.ValueOf(Ꮡa.Value).Bytes();
    });
    expectPanic("Bytes *[4]byte", "on ptr Value", () => {
        reflect.ValueOf(Ꮡa).Bytes();
    });
    var b = reflect.ValueOf(Ꮡa).Elem().Bytes();
    b[1] = 43;
    fmt.Println("A bytes:", b, "aliases a:", a[1] == 43);
    fmt.Println("[]B   bytes:", reflect.ValueOf(slice<main_B>([1, 2, 3, 4])).Bytes());
    fmt.Println("*[4]B bytes:", reflect.ValueOf(Ꮡ(new array<main_B>(4))).Elem().Bytes());
    fmt.Println("SB    bytes:", reflect.ValueOf(new main_SB(new main_B[]{1, 2, 3, 4}.slice())).Bytes());
    fmt.Println("*AB   bytes:", reflect.ValueOf(@new<main_AB>()).Elem().Bytes());
    ref var ab = ref heap<main_AB>(new main_AB(new main_B[]{5, 6, 7, 8}.array()), out var Ꮡab);
    expectPanic("Bytes AB value", "unaddressable", () => {
        reflect.ValueOf(Ꮡab.Value).Bytes();
    });
    var c = reflect.ValueOf(Ꮡab).Elem().Bytes();
    c[2] = 44;
    fmt.Println("AB bytes:", c, "aliases ab:", ab[2] == 44);
    expectPanic("Bytes [4]int", "of non-byte array", () => {
        reflect.ValueOf(Ꮡ(new nint[]{}.array(4))).Elem().Bytes();
    });

    fmt.Println("Name gB[gA]:", reflect.TypeOf(@new<gB<gA>>()).Elem().Name());
    fmt.Println("Name gB[gB[gA]]:", reflect.TypeOf(@new<gB<gB<gA>>>()).Elem().Name());
    fmt.Println("String gB[gA]:", reflect.TypeOf(new gB<gA>()).String());
    fmt.Println("Name plain gA:", reflect.TypeOf(new gA()).Name());

    var m5 = new map<Δio.Reader, Δio.Writer>();
    var mv5 = reflect.ValueOf(m5);
    var (b1, b2) = (@new<bytes.Buffer>(), @new<bytes.Buffer>());
    mv5.SetMapIndex(reflect.ValueOf(b1.OrTypedNil()), reflect.ValueOf(b2.OrTypedNil()));
    var (x5, ok5) = m5[new bytes_BufferжReader(b1), ꟷ];
    fmt.Println("#5 entry is b2:", AreEqual(x5, b2), ok5);
    @unsafe.Pointer p5 = (uintptr)mv5.MapIndex(reflect.ValueOf(b1.OrTypedNil())).Elem().UnsafePointer();
    fmt.Println("#5 MapIndex(b1).Elem().UnsafePointer() == unsafe.Pointer(b2):", p5 == @unsafe.Pointer.FromPinnedBox(b2));
    var m7 = new map<ж<main_MyBuffer>, ж<bytes.Buffer>>();
    var mv7 = reflect.ValueOf(m7);
    var (k7, v7) = (@new<main_MyBuffer>(), @new<bytes.Buffer>());
    mv7.SetMapIndex(reflect.ValueOf(k7.OrTypedNil()), reflect.ValueOf(v7.OrTypedNil()));
    @unsafe.Pointer p7 = (uintptr)mv7.MapIndex(reflect.ValueOf(k7.OrTypedNil())).UnsafePointer();
    fmt.Println("#7 MapIndex(b1).UnsafePointer() == unsafe.Pointer(b2):", p7 == @unsafe.Pointer.FromPinnedBox(v7));
    var n = @new<nint>();
    @unsafe.Pointer pn1 = (uintptr)reflect.ValueOf(n.OrTypedNil()).UnsafePointer();
    @unsafe.Pointer pn2 = (uintptr)reflect.ValueOf(n.OrTypedNil()).UnsafePointer();
    fmt.Println("reflect twice:", pn1 == pn2, " reflect vs unsafe.Pointer(n):", pn1 == @unsafe.Pointer.FromPinnedBox(n));
    var n2 = @new<nint>();
    ref var arr = ref heap(new array<int64>(4), out var Ꮡarr);
    fmt.Println("different boxes:", pn1 == @unsafe.Pointer.FromPinnedBox(n2), " interior != base:", (uintptr)@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡarr), 8) != @unsafe.Pointer.FromPinnedBox(Ꮡarr), " base == base:", @unsafe.Pointer.FromPinnedBox(Ꮡarr) == @unsafe.Pointer.FromPinnedBox(Ꮡarr));

    var keyed = new map<@unsafe.Pointer, nint>{[(uintptr)reflect.ValueOf(n.OrTypedNil()).UnsafePointer()] = 1};
    var (_, foundBox) = keyed[@unsafe.Pointer.FromPinnedBox(n), ꟷ];
    var interior = new map<@unsafe.Pointer, nint>{[(uintptr)@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡarr), 8)] = 2};
    var (_, foundNumber) = interior[(uintptr)@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡarr), 8), ꟷ];
    var (_, missOther) = keyed[@unsafe.Pointer.FromPinnedBox(n2), ꟷ];
    fmt.Println("map: same box twice found:", foundBox, " same number twice found:", foundNumber, " other box found:", missOther);
    ref var h = ref heap(new main_holder(), out var Ꮡh);
    @unsafe.Pointer fp = @unsafe.Pointer.FromBox(Ꮡh.of(main_holder.Ꮡp));
    var fieldKeyed = new map<@unsafe.Pointer, nint>{[fp] = 3};
    var (_, foundField) = fieldKeyed[@unsafe.Pointer.FromBox(Ꮡh.of(main_holder.Ꮡp)), ꟷ];
    fmt.Println("same field twice:", fp == @unsafe.Pointer.FromBox(Ꮡh.of(main_holder.Ꮡp)), " field vs other box:", fp == @unsafe.Pointer.FromPinnedBox(n), " map: same field twice found:", foundField);

    convRow("[]byte(nil) -> *[0]byte", slice<byte>(default), ж<array<byte>>.NilBoxOfDims(0L));
    convRow("[]byte{} -> *[0]byte", slice<byte>([]), Ꮡ(new array<byte>(0)));
    convRow("[]byte{7} -> *[1]byte", slice<byte>([7]), Ꮡ(new byte[]{7}.array()));
    convRow("MyBytes{9} -> *[1]byte", (MyBytes)slice<byte>([9]), Ꮡ(new byte[]{9}.array()));
    convRow("[]byte{1,2,3,4} -> MyBytesArrayPtr", slice<byte>([1, 2, 3, 4]), new MyBytesArrayPtr(Ꮡ(new byte[]{1, 2, 3, 4}.array())));
    convRow("[]byte(nil) -> MyBytesArrayPtr0", slice<byte>(default), (MyBytesArrayPtr0)nil);
    convRow("[]byte{1,2,3,4} -> *MyBytesArray", slice<byte>([1, 2, 3, 4]), Ꮡ(new MyBytesArray(new byte[]{1, 2, 3, 4}.array())));
    convRow("[]byte(nil) -> *MyBytesArray0", slice<byte>(default), ж<MyBytesArray0>.NilBoxOfDims(0L));
    convRow("new([0]byte) -> *MyBytesArray0", Ꮡ(new array<byte>(0)), @new<MyBytesArray0>());
    convRow("new(MyBytesArray0) -> *[0]byte", @new<MyBytesArray0>(), Ꮡ(new array<byte>(0)));
    convRow("MyBytesArrayPtr0(nil) -> *[0]byte", (MyBytesArrayPtr0)nil, ж<array<byte>>.NilBoxOfDims(0L));
    convRow("(*[0]byte)(nil) -> MyBytesArrayPtr0", ж<array<byte>>.NilBoxOfDims(0L), (MyBytesArrayPtr0)nil);
    convRow("new(int) -> *integer", @new<nint>(), @new<integer>());
    convRow("new(integer) -> *int", @new<integer>(), @new<nint>());
    convRow("*MyBuffer -> *bytes.Buffer", @new<main_MyBuffer>(), @new<bytes.Buffer>());
    var src = slice<byte>([1, 2, 3, 4]);
    var ap = reflect.ValueOf(src).Convert(reflect.TypeOf(ж<array<byte>>.NilBoxOfDims(4L))).Interface()._<ж<array<byte>>>();
    ap.Value[2] = 99;
    fmt.Println("array pointer aliases the slice:", src[2] == 99);
    ref var sh = ref heap<reflectꓸValue>(reflect.ValueOf(slice<byte>([1, 2, 3, 4])), out var Ꮡsh);
    var shʗ1 = sh;
    expectPanic("Convert short slice", "cannot convert slice with length 4 to pointer to array with length 8", () => {
        shʗ1.Convert(reflect.TypeOf(ж<array<byte>>.NilBoxOfDims(8L)));
    });

    convRow("IntChan(nil) -> chan<- int", (IntChan)default, channel/*<-*/<nint>.SendOnly);
    convRow("IntChan(nil) -> <-chan int", (IntChan)default, /*<-*/channel<nint>.RecvOnly);
    convRow("chan int(nil) -> IntChanRecv", (channel<nint>)(default), (IntChanRecv)default);
    convRow("chan int(nil) -> IntChanSend", (channel<nint>)(default), (IntChanSend)default);
    convRow("IntChanRecv(nil) -> <-chan int", (IntChanRecv)default, /*<-*/channel<nint>.RecvOnly);
    convRow("<-chan int(nil) -> IntChanRecv", /*<-*/channel<nint>.RecvOnly, (IntChanRecv)default);
    convRow("IntChanSend(nil) -> chan<- int", (IntChanSend)default, channel/*<-*/<nint>.SendOnly);
    convRow("chan<- int(nil) -> IntChanSend", channel/*<-*/<nint>.SendOnly, (IntChanSend)default);
    convRow("IntChan(nil) -> chan int", (IntChan)default, (channel<nint>)(default));
    var live = new IntChan(1);
    var sendOnly = reflect.ValueOf(live).Convert(reflect.TypeOf(channel/*<-*/<nint>.SendOnly));
    sendOnly.Interface()._<channel/*<-*/<nint>>().ᐸꟷ(7);
    fmt.Println("converted send-only view type:", sendOnly.Type(), " interface type:", reflect.TypeOf(sendOnly.Interface()), " received on the original:", ᐸꟷ<nint>(live));

    void setRow(@string label, reflectꓸType slotΔ1, reflectꓸValue xΔ1) {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                {
                    var r = recover(); if (r != nil) {
                        fmt.Printf("%-38s PANIC: %v\n"u8, label, r);
                    }
                }
            }, ref ᒐ);
            var v = reflect.New(slotΔ1).Elem();
            v.Set(xΔ1);
            fmt.Printf("%-38s slot=%v value=%v nil=%v\n"u8, label, v.Type(), reflect.TypeOf(v.Interface()), v.IsNil());
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    var (sendT, recvT, bidiT) = (reflect.TypeOf(channel/*<-*/<nint>.SendOnly), reflect.TypeOf(/*<-*/channel<nint>.RecvOnly), reflect.TypeOf((channel<nint>)(default)));
    setRow("chan<- int <- IntChan(nil).Convert", sendT, reflect.ValueOf((IntChan)default).Convert(sendT));
    setRow("<-chan int <- chan int(nil).Convert", recvT, reflect.ValueOf((channel<nint>)(default)).Convert(recvT));
    setRow("chan int <- chan<- int(nil)", bidiT, reflect.ValueOf(channel/*<-*/<nint>.SendOnly));
    setRow("<-chan int <- chan<- int(nil)", recvT, reflect.ValueOf(channel/*<-*/<nint>.SendOnly));
    setRow("chan<- int <- chan int(nil)", sendT, reflect.ValueOf((channel<nint>)(default)));
    setRow("chan<- int <- live IntChan.Convert", sendT, reflect.ValueOf(live).Convert(sendT));
    var slot = reflect.New(sendT).Elem();
    slot.Set(reflect.ValueOf(live));
    slot.Interface()._<channel/*<-*/<nint>>().ᐸꟷ(8);
    fmt.Println("send-only slot value type:", reflect.TypeOf(slot.Interface()), " received on the original:", ᐸꟷ<nint>(live));

    fmt.Println("IntChanRecv dir:", reflect.TypeOf((IntChanRecv)default).ChanDir(), " IntChanSend dir:", reflect.TypeOf((IntChanSend)default).ChanDir(), " IntChan dir:", reflect.TypeOf((IntChan)default).ChanDir());
    void canRow(any xΔ2, any yΔ1) {
        fmt.Printf("%-14v -> %-14v convertible=%v assignable=%v\n"u8, reflect.TypeOf(xΔ2), reflect.TypeOf(yΔ1), reflect.TypeOf(xΔ2).ConvertibleTo(reflect.TypeOf(yΔ1)), reflect.TypeOf(xΔ2).AssignableTo(reflect.TypeOf(yΔ1)));
    }
    canRow(/*<-*/channel<nint>.RecvOnly, (IntChanRecv)default);
    canRow(channel/*<-*/<nint>.SendOnly, (IntChanSend)default);
    canRow((IntChan)default, (IntChanRecv)default);
    canRow((IntChanRecv)default, (IntChan)default);
    canRow((IntChanRecv)default, channel/*<-*/<nint>.SendOnly);
    canRow((IntChanRecv)default, (channel<nint>)(default));
    canRow((IntChanSend)default, /*<-*/channel<nint>.RecvOnly);
    canRow((IntChanRecv)default, (IntChanSend)default);
    var cst = reflect.TypeOf(new main_chanSlots());
    fmt.Println("IntChanRecv identity value/slot/elem:", AreEqual(reflect.TypeOf((IntChanRecv)default), cst.Field(0).Type), AreEqual(reflect.TypeOf((IntChanRecv)default), reflect.TypeOf((ж<IntChanRecv>)nil).Elem()), cst.Field(0).Type.ChanDir());
    fmt.Println("IntChanSend identity value/slot/elem:", AreEqual(reflect.TypeOf((IntChanSend)default), cst.Field(1).Type), AreEqual(reflect.TypeOf((IntChanSend)default), reflect.TypeOf((ж<IntChanSend>)nil).Elem()), cst.Field(1).Type.ChanDir());
    fmt.Println("chan slot zero re-describes:", reflect.TypeOf(reflect.Zero(cst.Field(0).Type).Interface()), reflect.Zero(cst.Field(0).Type).Type().ChanDir());

    fmt.Println("named ptr-to-array one descriptor:", AreEqual(reflect.TypeOf((MyBytesArrayPtr0)nil), reflect.TypeOf(new MyBytesArrayPtr0(Ꮡ(new array<byte>(0))))), AreEqual(reflect.TypeOf((MyBytesArrayPtr)nil), reflect.TypeOf(new MyBytesArrayPtr(Ꮡ(new array<byte>(4))))), " elem:", reflect.TypeOf((MyBytesArrayPtr)nil).Elem(), reflect.TypeOf((MyBytesArrayPtr0)nil).Elem().Len());
    canRow((MyBytes)default, (MyBytesArrayPtr0)nil);
    canRow((MyBytesArrayPtr0)nil, ж<array<byte>>.NilBoxOfDims(0L));
    canRow((MyBytesArrayPtr)nil, ж<array<byte>>.NilBoxOfDims(4L));
    canRow((MyBytesArrayPtr)nil, (MyBytesArrayPtr0)nil);
    var pst = reflect.TypeOf(new main_ptrSlots());
    fmt.Println("MyBytesArrayPtr identity value/slot/elem:", AreEqual(reflect.TypeOf(new MyBytesArrayPtr(Ꮡ(new array<byte>(4)))), pst.Field(0).Type), AreEqual(reflect.TypeOf((MyBytesArrayPtr)nil), reflect.TypeOf((ж<MyBytesArrayPtr>)nil).Elem()), pst.Field(0).Type.Elem().Len());
    fmt.Println("MyBytesArrayPtr0 identity value/slot/elem:", AreEqual(reflect.TypeOf(new MyBytesArrayPtr0(Ꮡ(new array<byte>(0)))), pst.Field(1).Type), AreEqual(reflect.TypeOf((MyBytesArrayPtr0)nil), reflect.TypeOf((ж<MyBytesArrayPtr0>)nil).Elem()), pst.Field(1).Type.Elem().Len());
    fmt.Println("ptr slot New re-describes:", reflect.TypeOf(reflect.New(pst.Field(1).Type).Elem().Interface()), reflect.New(pst.Field(0).Type).Elem().Type().Elem().Len());
}

} // end main_package
