namespace go;

using fmt = fmt_package;
using Δruntime = runtime_package;
using debug = go.runtime.debug_package;
using go.runtime;

partial class main_package {

internal static void Main() {
    var (bi, ok) = debug.ReadBuildInfo();
    fmt.Println("ok:", ok);

    if (!ok) {
        return;
    }

    fmt.Println("path:", bi.Path);
    fmt.Println("main.path:", bi.Main.Path);
    fmt.Println("main.version:", bi.Main.Version);

    foreach (var (_, setting) in range(bi.Settings)) {
        var exprᴛ1 = setting.Key;
        switch (ᐧ) {
        case {} when exprᴛ1 == "GOOS"u8: {
            fmt.Printf("%s: %s\n"u8, setting.Key, setting.Value);
            break;
        }
        case {} when exprᴛ1 == "GOARCH"u8: {
            fmt.Println("GOARCH is runtime.GOARCH:", setting.Value == Δruntime.GOARCH);
            break;
        }}

    }
}

} // end main_package
