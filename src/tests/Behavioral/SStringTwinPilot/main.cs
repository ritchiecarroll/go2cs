namespace go;

using errors = errors_package;
using fmt = fmt_package;
using reflect = reflect_package;
using runtime = runtime_package;
using utf8 = unicode.utf8_package;
using unicode;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object argˢ = (@string)"arg"u8;

internal static @string apply(Funcꓸꓸꓸ<@string, any, @string> f, @string format) {
    return f(format, argˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string hLloˢ = "héllo, 世界"u8;

internal static void Main() {
    GoFrame ᒐ = default;
    try {
        fmt.Println(fmt.Sprintf("xxx"u8));
        @string dynamic = "dyn %d";
        fmt.Println(fmt.Sprintf(dynamic, (nint)1));
        var b = slice<byte>("from bytes %d"u8);
        fmt.Println(fmt.Sprintf((@string)b, (nint)2));
        fmt.Println(fmt.Sprintf("con"u8 + "cat %s"u8, "ok"));
        fmt.Printf("printf %v %q\n"u8, (nint)3, "q");

        Funcꓸꓸꓸ<@string, any, @string> f = fmt.Sprintfᶠ;
        fmt.Println(f("via local %s"u8, (@string)"ok"u8));
        var table = new map<@string, any>{["printf"] = ((Funcꓸꓸꓸ<@string, any, @string>)(fmt.Sprintfᶠ))};
        fmt.Println(table["printf"]._<Funcꓸꓸꓸ<@string, any, @string>>()("via table %d", (nint)4));
        fmt.Println(apply(fmt.Sprintfᶠ, "via argument %s"));

        var p1 = reflect.ValueOf(fmt.Sprintfᶠ).Pointer();
        var p2 = reflect.ValueOf((f).OrTypedNilFunc()).Pointer();
        fmt.Println("same pointer:", p1 == p2);
        fmt.Println("name:", runtime.FuncForPC(p1).Name());

        ref var n = ref heap(new nint(), out var Ꮡn);
        var (count, err) = fmt.Sscanf("42"u8, "%d"u8, Ꮡn);
        fmt.Println(count, err, n);
        fmt.Println(fmt.Errorf("wrapped: %w"u8, errors.New("inner")));
        fmt.Println((@string)fmt.Appendf(default, "appended %d"u8, (nint)6));
        fmt.Println(utf8.RuneCountInString(hLloˢ));
        var (r, size) = utf8.DecodeRuneInString("é!"u8);
        fmt.Println(r, size);
        var raw = slice<byte>("héllo, 世界"u8);
        fmt.Println(utf8.RuneCountInString((sstring)raw), utf8.RuneCount(raw[1..]));
        var (r2, size2) = utf8.DecodeRuneInString((sstring)(raw[1..]));
        fmt.Println(r2, size2);
        defer(ᴛ1 => fmt.Println("deferred count", ᴛ1), utf8.RuneCountInString((@string)raw), ref ᒐ);
        raw[1] = (byte)'e';
        fmt.Println(utf8.RuneCountInString((sstring)raw));
        fmt.Printf("%x %X %08b %5.2f|%-6s|%6s|\n"u8, (nint)255, "hi", (nint)5, 3.14159D, "left", "right");

        defer(() => fmt.Printf("deferred %s\n"u8, "printf"), ref ᒐ);
        fmt.Println("done");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
