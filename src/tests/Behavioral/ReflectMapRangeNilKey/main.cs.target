namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial struct namedAny /*map[any, nint]*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object anyˢ = (@string)"any:"u8;
private static readonly object namedˢ = (@string)"named:"u8;
private static readonly object ptrˢ = (@string)"ptr:"u8;
private static readonly object errˢ = (@string)"err:"u8;
private static readonly object plainˢ = (@string)"plain:"u8;
private static readonly object sliceValueˢ = (@string)"slice value:"u8;
private static readonly object nilValueˢ = (@string)"nil value:"u8;

internal static void Main() {
    var anyKeys = new map<any, nint>{[default!] = 1, [(@string)"b"u8] = 2, [(nint)(7)] = 3};
    nint iface = 0;
    nint nils = 0;
    nint invalid = 0;
    nint sum = 0;
    var iter = reflect.ValueOf(anyKeys).MapRange();
    while (iter.Next()) {
        var (k, v) = (iter.Key(), iter.Value());
        if (!k.IsValid()) {
            invalid++;
            continue;
        }
        if (k.Kind() == reflect.ΔInterface) {
            iface++;
            if (k.IsNil()) {
                nils++;
            }
        }
        if (v.Kind() == reflect.ΔInt) {
            sum += (nint)v.Int();
        }
    }
    fmt.Println(anyˢ, iface, nils, invalid, sum, reflect.ValueOf(anyKeys).Len());
    var named = new namedAny(new map<any, nint>{[default!] = 4, [(@string)"b"u8] = 5});
    (iface, nils, invalid, sum) = (0, 0, 0, 0);
    iter = reflect.ValueOf(named).MapRange();
    while (iter.Next()) {
        var (k, v) = (iter.Key(), iter.Value());
        if (!k.IsValid()) {
            invalid++;
            continue;
        }
        if (k.Kind() == reflect.ΔInterface) {
            iface++;
            if (k.IsNil()) {
                nils++;
            }
        }
        sum += (nint)v.Int();
    }
    fmt.Println(namedˢ, iface, nils, invalid, sum, reflect.ValueOf(named).Len());
    ref var n = ref heap<nint>(out var Ꮡn);
    n = 9;
    var ptrKeys = new map<ж<nint>, @string>{[default!] = "nil"u8, [Ꮡn] = "n"u8};
    nint ptrs = 0;
    nint pnils = 0;
    nint pinvalid = 0;
    iter = reflect.ValueOf(ptrKeys).MapRange();
    while (iter.Next()) {
        var k = iter.Key();
        if (!k.IsValid()) {
            pinvalid++;
            continue;
        }
        if (k.Kind() == reflect.ΔPointer) {
            ptrs++;
            if (k.IsNil()) {
                pnils++;
            }
        }
    }
    fmt.Println(ptrˢ, ptrs, pnils, pinvalid, reflect.ValueOf(ptrKeys).Len());
    var errKeys = new map<error, nint>{[default!] = 6};
    nint eiface = 0;
    nint enils = 0;
    iter = reflect.ValueOf(errKeys).MapRange();
    while (iter.Next()) {
        var k = iter.Key();
        if (k.IsValid() && k.Kind() == reflect.ΔInterface) {
            eiface++;
            if (k.IsNil()) {
                enils++;
            }
        }
    }
    fmt.Println(errˢ, eiface, enils, reflect.ValueOf(errKeys).Len());
    var plain = new map<@string, nint>{["a"u8] = 1, ["b"u8] = 2};
    nint strs = 0;
    nint snils = 0;
    nint psum = 0;
    iter = reflect.ValueOf(plain).MapRange();
    while (iter.Next()) {
        var (k, v) = (iter.Key(), iter.Value());
        if (k.Kind() == reflect.ΔString) {
            strs++;
        }
        if (k.IsValid() && k.Kind() == reflect.ΔString && k.String() == ""u8) {
            snils++;
        }
        psum += (nint)v.Int();
    }
    fmt.Println(plainˢ, strs, snils, psum, reflect.ValueOf(plain).Len());
    var slices = new map<@string, slice<nint>>{["k"u8] = new nint[]{1, 2, 3}.slice()};
    iter = reflect.ValueOf(slices).MapRange();
    while (iter.Next()) {
        var v = iter.Value();
        fmt.Println(sliceValueˢ, v.Kind() == reflect.ΔSlice, v.Len(), v.IsNil());
    }
    var anyVals = new map<@string, any>{["k"u8] = default!};
    iter = reflect.ValueOf(anyVals).MapRange();
    while (iter.Next()) {
        var v = iter.Value();
        fmt.Println(nilValueˢ, v.IsValid(), v.Kind() == reflect.ΔInterface, v.IsNil());
    }
}

} // end main_package
