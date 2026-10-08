namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct namedString /*@string*/;

partial struct namedByte /*num:byte*/;

partial struct namedRune /*num:rune*/;

partial struct namedByteSlice /*[]namedByte*/;

partial struct namedRuneSlice /*[]namedRune*/;

partial struct plainByteSlice /*[]byte*/;

internal static nint takesBytes(slice<byte> b) {
    return len(b);
}

internal static nint takesNamedBytes(slice<namedByte> b) {
    return len(b);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string hLloˢ = "héllo"u8;
private static readonly @string abcˢ = "abc"u8;

internal static void Main() {
    ref var ns = ref heap(new namedString(), out var Ꮡns);
    ns = "hi"u8;
    fmt.Println(slice<byte>((@string)ns));
    fmt.Println(slice<rune>((@string)ns));
    fmt.Println(takesBytes(slice<byte>((@string)ns)));
    var pns = Ꮡns;
    fmt.Println(slice<byte>((@string)pns.Value));
    namedString wide = hLloˢ;
    fmt.Println(len(slice<byte>((@string)wide)), len(slice<rune>((@string)wide)));
    var nb = widen<byte, namedByte>(slice<byte>((@string)"hello"u8), elemᴛ0 => (namedByte)elemᴛ0);
    fmt.Println(nb, len(nb));
    var nr = widen<rune, namedRune>(slice<rune>((@string)"héllo"u8), elemᴛ0 => (namedRune)elemᴛ0);
    fmt.Println(nr, len(nr));
    @string plain = abcˢ;
    fmt.Println(widen<byte, namedByte>(slice<byte>(plain), elemᴛ0 => (namedByte)elemᴛ0));
    var nbs = ((namedByteSlice)widen<byte, namedByte>(slice<byte>((@string)"abc"u8), elemᴛ0 => (namedByte)elemᴛ0));
    var nrs = ((namedRuneSlice)widen<rune, namedRune>(slice<rune>((@string)"héllo"u8), elemᴛ0 => (namedRune)elemᴛ0));
    fmt.Println(nbs, nrs);
    @string plainVar = abcˢ;
    fmt.Println(((namedByteSlice)widen<byte, namedByte>(slice<byte>(plainVar), elemᴛ0 => (namedByte)elemᴛ0)), ((plainByteSlice)slice<byte>(plainVar)));
    fmt.Println(((namedByteSlice)widen<byte, namedByte>(slice<byte>((@string)ns), elemᴛ0 => (namedByte)elemᴛ0)), ((plainByteSlice)slice<byte>((@string)ns)));
    fmt.Println(takesNamedBytes(widen<byte, namedByte>(slice<byte>((@string)"wxyz"u8), elemᴛ0 => (namedByte)elemᴛ0)));
    fmt.Println(widen<byte, namedByte>(slice<byte>((@string)ns), elemᴛ0 => (namedByte)elemᴛ0));
    fmt.Println(((@string)widen<namedByte, byte>(new namedByte[]{104, 105}.slice(), elemᴛ0 => (byte)elemᴛ0)));
    fmt.Println(((@string)widen<namedByte, byte>((slice<namedByte>)nbs, elemᴛ0 => (byte)elemᴛ0)));
    fmt.Println(((@string)widen<namedRune, rune>(new namedRune[]{104, 233, 105}.slice(), elemᴛ0 => (rune)elemᴛ0)));
    fmt.Println(((@string)widen<namedRune, rune>((slice<namedRune>)nrs, elemᴛ0 => (rune)elemᴛ0)));
    @string src = abcˢ;
    var copyOf = widen<byte, namedByte>(slice<byte>(src), elemᴛ0 => (namedByte)elemᴛ0);
    copyOf[0] = 122;
    fmt.Println(src, ((@string)widen<namedByte, byte>(copyOf, elemᴛ0 => (byte)elemᴛ0)));
    fmt.Println(slice<byte>("lit"u8));
    fmt.Println(slice<byte>(plain));
    fmt.Println(slice<rune>(plain));
    var pbs = ((plainByteSlice)slice<byte>((@string)"abc"u8));
    fmt.Println(pbs, ((@string)(slice<byte>)pbs));
}

} // end main_package
