global using entry = go.main_package.entryᴛ1;

namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

public partial struct entryᴛ1 /*dyn*/ {
    public nint ID;
    public @string Name;
}

public partial interface deps {
    entry Process(entry e);
}

partial struct impl {
}

} partial class mainᴛimpl {

internal static entry Process(this impl _, entry e) {
    e.ID++;
    e.Name = e.Name + "!"u8;
    return e;
}

} partial class main_package {

public static entry Run(deps d) {
    return d.Process(new entry(ID: 1, Name: "seed"));
}

internal static void main() {
    var r = Run(new impl());
    fmt.Println(r.ID, r.Name);
}

} // end main_package
