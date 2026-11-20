namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial interface badge {
    @string label();
}

partial struct gold {
}

} partial class mainᴛgold {

internal static @string label(this gold _) { return "gold"; }

} partial class main_package {


partial struct reservedᴛ1 /*dyn*/ {
    /*embed*/ internal badge badge;
}
internal static ж<reservedᴛ1> reserved = @new<reservedᴛ1>();

internal static void main() {
    badge b = new reservedᴛ1жbadge(reserved);
    fmt.Println(b != nil);

    reserved.Value.badge = new gold();
    fmt.Println(b.label());

    var local = @new<reservedᴛ1>();
    local.Value.badge = new gold();

    badge b2 = new reservedᴛ1жbadge(local);
    fmt.Println("local:", b2.label());
}

} // end main_package
