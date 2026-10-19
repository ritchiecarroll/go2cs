namespace go;

using fmt = fmt_package;
using ꓸꓸꓸbyte = Span<byte>;

partial class main_package {

internal static void probe(@string name, bool isNil, nint length, nint capacity) {
    fmt.Println(name, isNil, length, capacity);
}

internal static void Main() {
    slice<byte> zero = default!;
    probe("zeroValue", zero == default!, len(zero), cap(zero));

    var emptyLit = new byte[]{}.slice();
    probe("emptyLiteral", emptyLit == default!, len(emptyLit), cap(emptyLit));

    var emptyStr = slice<byte>(""u8);
    probe("emptyStringConv", emptyStr == default!, len(emptyStr), cap(emptyStr));

    var nilConv = slice<byte>(default!);
    probe("nilConversion", nilConv == default!, len(nilConv), cap(nilConv));

    var nilReslice = zero[0..0];
    probe("nilReslice", nilReslice == default!, len(nilReslice), cap(nilReslice));

    var made = new slice<byte>(0);
    probe("makeZero", made == default!, len(made), cap(made));

    var x = new byte[]{1, 2}.slice();
    probe("resliceToEmpty", x[..0] == default!, len(x[..0]), cap(x[..0]));

    var tail = x[2..2];
    probe("resliceTailCapZero", tail == default!, len(tail), cap(tail));

    var appendNilNothing = append(slice<byte>(default!));
    probe("appendNilNothing", appendNilNothing == default!, len(appendNilNothing), cap(appendNilNothing));

    var appendEmptyNothing = append(new byte[]{}.slice());
    probe("appendEmptyNothing", appendEmptyNothing == default!, len(appendEmptyNothing), cap(appendEmptyNothing));

    var cloneShape = appendꓸꓸꓸ(new byte[]{}.slice(), x[..0]);
    probe("cloneShape", cloneShape == default!, len(cloneShape), cap(cloneShape));

    var trim = x;
    while (len(trim) > 0) {
        trim = trim.slice(0, len(trim) - 1);
    }
    probe("trimShape", trim == default!, len(trim), cap(trim));

    slice<@string> zs = default!;
    probe("stringSliceZero", zs == default!, len(zs), cap(zs));
    probe("stringSliceEmpty", new @string[]{}.slice() == default!, len(new @string[]{}.slice()), cap(new @string[]{}.slice()));

    variadic("packZeroArgs");
    variadic("packOneArg", 1);
    variadic("packTwoArgs", 1, 2);

    slice<byte> nilPack = default!;
    variadic("spreadNil"u8, nilPack.ꓸꓸꓸ);
    variadic("spreadEmptyLiteral"u8, new byte[]{}.slice().ꓸꓸꓸ);
    variadic("spreadMakeZero"u8, new slice<byte>(0).ꓸꓸꓸ);
    variadic("spreadResliceTailCapZero"u8, x[2..2].ꓸꓸꓸ);
    variadic("spreadNonEmpty"u8, x.ꓸꓸꓸ);

    variadicDerived("packZeroArgsDerived");
    variadicDerived("packOneArgDerived", 9);
}

internal static void variadic(@string name, params ꓸꓸꓸbyte packʗp) {
    var pack = packʗp.slice();

    probe(name, pack == default!, len(pack), cap(pack));
}

internal static void variadicDerived(@string name, params ꓸꓸꓸbyte packʗp) {
    var pack = packʗp.slice();

    probe(name + "/reslice"u8, pack[0..0] == default!, len(pack[0..0]), cap(pack[0..0]));
    var appended = append(pack);
    probe(name + "/appendNothing"u8, appended == default!, len(appended), cap(appended));
    fmt.Println(name + "/reSpread", spreadThrough(pack.ꓸꓸꓸ) == default!);
}

internal static slice<byte> spreadThrough(params ꓸꓸꓸbyte packʗp) {
    var pack = packʗp.slice();

    return pack;
}

} // end main_package
