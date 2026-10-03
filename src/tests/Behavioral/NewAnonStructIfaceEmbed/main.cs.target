namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial interface badge {
    @string label();
}

[GoType] partial struct gold {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string goldˢ = "gold"u8;

internal static @string label(this gold _) {
    return goldˢ;
}


[GoType("dyn")] partial struct reservedᴛ1 {
    [GoEmbedded] internal badge badge;
}
internal static ж<reservedᴛ1> reserved = @new<reservedᴛ1>();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object localˢ = (@string)"local:"u8;

internal static void Main() {
    badge b = new reservedᴛ1жbadge(reserved);
    fmt.Println(b != default!);
    reserved.Value.badge = new gold(nil);
    fmt.Println(b.label());
    var local = @new<reservedᴛ1>();
    local.Value.badge = new gold(nil);
    badge b2 = new reservedᴛ1жbadge(local);
    fmt.Println(localˢ, b2.label());
}

} // end main_package
