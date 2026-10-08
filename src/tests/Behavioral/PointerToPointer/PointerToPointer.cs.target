namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Buffer {
    internal slice<byte> buf;
    internal nint off;
    internal int8 lastRead;
}

internal const int8 opRead = -1;
internal const int8 opInvalid = 0;

internal static void Main() {
    ref var a = ref heap(new nint(), out var Ꮡa);
    ref var ptr = ref heap<ж<nint>>(out var Ꮡptr);
    ref var pptr = ref heap<ж<ж<nint>>>(out var Ꮡpptr);
    ж<ж<ж<nint>>> ppptr = default!;
    a = 3000;
    ptr = Ꮡa;
    pptr = Ꮡptr;
    ppptr = Ꮡpptr;
    fmt.Printf("Value of a = %d\n"u8, a);
    PrintValPtr(ptr);
    fmt.Printf("Main-function return value available at *ptr = %d\n"u8, EscapePrintValPtr(ptr).Value);
    fmt.Printf("Main-function updated value available at *ptr = %d\n"u8, ptr.Value);
    PrintValPtr2Ptr(pptr);
    PrintValPtr2Ptr2Ptr(ppptr);
    a = 1900;
    fmt.Printf("Value of a = %d\n"u8, a);
    PrintValPtr(ptr);
    fmt.Printf("Main-function return value available at *ptr = %d\n"u8, EscapePrintValPtr(ptr).Value);
    fmt.Printf("Main-function updated value available at *ptr = %d\n"u8, ptr.Value);
    PrintValPtr2Ptr(pptr);
    PrintValPtr2Ptr2Ptr(ppptr);
    ref var b = ref heap<Buffer>(out var Ꮡb);
    b = new Buffer(nil);
    PrintValPtr(Ꮡb.of(Buffer.Ꮡoff));
    PrintValPtr(Ꮡb.of(Buffer.Ꮡoff));
    derefElements();
}

public static (nint n, error err) Read(this ref Buffer b, slice<byte> p) {
    nint n = default!;

    b.lastRead = opInvalid;
    b.off += n;
    if (n > 0) {
        b.lastRead = opRead;
    }
    (Ꮡ(new Buffer(buf: p))).Read(p);
    return (n, default!);
}

public static ж<Buffer> /*b1*/ NewBuffer(slice<byte> buf) {
    return Ꮡ(new Buffer(buf: buf));
}

public static void PrintValPtr(ж<nint> Ꮡptr) {
    ref var ptr = ref Ꮡptr.DerefOrNull();

    fmt.Printf("Value available at *ptr = %d\n"u8, ptr);
    ptr++;
}

public static ж<nint> EscapePrintValPtr(ж<nint> Ꮡout) {
    ref var @out = ref Ꮡout.DerefOrNull();

    fmt.Printf("Value available at *ptr = %d\n"u8, @out);
    ref var i = ref heap<nint>(out var Ꮡi);
    i = 99;
    Ꮡout = Ꮡi; @out = ref Ꮡout.DerefOrNull();
    fmt.Printf("Intra-function updated value available at *ptr = %d\n"u8, @out);
    PrintValPtr(Ꮡout);
    return Ꮡout;
}

public static void PrintValPtr2Ptr(ж<ж<nint>> Ꮡpptr) {
    ref var pptr = ref Ꮡpptr.DerefOrNull();

    fmt.Printf("Value available at **pptr = %d\n"u8, pptr.Value);
}

public static void PrintValPtr2Ptr2Ptr(ж<ж<ж<nint>>> Ꮡppptr) {
    ref var ppptr = ref Ꮡppptr.DerefOrNull();

    fmt.Printf("Value available at ***pptr = %d\n"u8, ppptr.ValueSlot.Value);
}

internal static void derefElements() {
    ref var a1 = ref heap<nint>(out var Ꮡa1);
    a1 = 10;
    ref var a2 = ref heap<nint>(out var Ꮡa2);
    a2 = 20;
    var boxes = new ж<nint>[]{Ꮡa1, Ꮡa2}.slice();
    boxes[0].Value = boxes[0].Value + 5;
    boxes[1].Value = readBox(boxes, 1) * 3;
    fmt.Println(a1, a2, boxes[0].Value);
}

internal static nint readBox(slice<ж<nint>> items, nint i) {
    return items[i].Value;
}

} // end main_package
