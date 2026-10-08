global using entry = go.main_package.entryᴛ1;

namespace go;

using fmt = fmt_package;

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

internal static entry Process(this impl _, entry e) {
    e.ID++;
    e.Name = e.Name + "!"u8;
    return e;
}

public static entry Run(deps d) {
    return d.Process(new entry(ID: 1, Name: "seed"u8));
}

internal static void Main() {
    var r = Run(new impl(nil));
    fmt.Println(r.ID, r.Name);
}

} // end main_package
