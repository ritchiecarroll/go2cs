namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface badge {
    @string label();
}

partial struct gold {
}

internal static @string label(this gold _) {
    return "gold";
}


partial struct reservedᴛ1 /*dyn*/ {
    /*embed*/ internal badge badge;
}
internal static ж<reservedᴛ1> reserved = @new<reservedᴛ1>();

internal static void Main() {
    badge b = new reservedᴛ1жbadge(reserved);
    fmt.Println(b != nil);

    reserved.Value.badge = new gold(nil);
    fmt.Println(b.label());

    var local = @new<reservedᴛ1>();
    local.Value.badge = new gold(nil);

    badge b2 = new reservedᴛ1жbadge(local);
    fmt.Println("local:", b2.label());
}

} // end main_package
