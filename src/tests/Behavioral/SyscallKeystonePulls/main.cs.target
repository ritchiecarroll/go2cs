namespace go;

using fmt = fmt_package;
using exec = os.exec_package;
using user = os.user_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
using strings = strings_package;
using os;
using path;

partial class main_package {

internal static void Main() {
    var (p, err) = exec.LookPath("go");
    if (err != nil) {
        fmt.Println("LookPath(go): error:", err);
    } else {
        fmt.Println("LookPath(go):", strings.TrimSuffix(filepath.Base(p), ".exe"), "absolute:", filepath.IsAbs(p));
    }

    (_, err) = exec.LookPath("no-such-executable-go2cs-keystone");
    fmt.Println("LookPath(missing): error:", err != nil);

    if (runtime.GOOS != "darwin"u8) {
        fmt.Println("done");
        return;
    }

    (var u, err) = user.Current();
    fmt.Println("user.Current: error:", err != nil);
    if (err == nil) {
        fmt.Println("user.Current: uid set:", u.Uid != ""u8, "gid set:", u.Gid != ""u8,
            "username set:", u.Username != ""u8);

        var (byName, e) = user.Lookup(u.Username);
        fmt.Println("user.Lookup(username): error:", e != nil,
            "uid round-trips:", e == nil && byName.Uid == u.Uid);

        (var g, e) = user.LookupGroupId(u.Gid);
        fmt.Println("user.LookupGroupId(gid): error:", e != nil,
            "name set:", e == nil && g.Name != ""u8);
        if (e == nil) {
            var (byGroup, e2) = user.LookupGroup(g.Name);
            fmt.Println("user.LookupGroup(name): error:", e2 != nil,
                "gid round-trips:", e2 == nil && byGroup.Gid == g.Gid);
        }
    }

    fmt.Println("done");
}

} // end main_package
