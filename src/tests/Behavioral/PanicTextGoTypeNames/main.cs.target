namespace go;

using fmt = fmt_package;
using runtime = runtime_package;

partial class main_package {

internal static void pin(@string label, any v) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            fmt.Println(label + ":", recover());
        }, ref ᒐ);

        runtime.Pinner p = new(nil);
        p.Pin(v);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void assertString(@string label, any v) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            fmt.Println(label + ":", recover());
        }, ref ᒐ);

        _ = v._<@string>();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void hashKey(@string label, any k) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            fmt.Println(label + ":", recover());
        }, ref ᒐ);

        var m = new map<any, nint>{};
        m[k] = 1;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    pin("pin int32", (int32)5);
    pin("pin rune", (rune)'a');
    pin("pin uint8", (uint8)1);
    pin("pin byte", (byte)2);
    pin("pin int", (nint)5);
    pin("pin uint", (nuint)5);
    pin("pin int64", (int64)5);
    pin("pin uint32", (uint32)5);
    pin("pin float64", 1.5D);
    pin("pin string", "s");

    assertString("assert int32", (int32)5);
    assertString("assert uint8", (uint8)1);
    assertString("assert int", (nint)5);

    hashKey("hash []int32", slice<int32>([1]));
    hashKey("hash []byte", slice<byte>([1]));
    hashKey("hash []int", slice<nint>([1]));
}

} // end main_package
