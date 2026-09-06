namespace go;

using fmt = fmt_package;
using exec = os.exec_package;
using user = os.user_package;
using filepath = path.filepath_package;
using Δruntime = runtime_package;
using strings = strings_package;
using os;
using path;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object lookPathGoErrorˢ = (@string)"LookPath(go): error:"u8;
private static readonly object lookPathGoˢ = (@string)"LookPath(go):"u8;
private static readonly @string exeˢ = ".exe"u8;
private static readonly object absoluteˢ = (@string)"absolute:"u8;
private static readonly @string noSuchExecutableGo2csˢ = "no-such-executable-go2cs-keystone"u8;
private static readonly object lookPathMissingErrorˢ = (@string)"LookPath(missing): error:"u8;
private static readonly object doneˢ = (@string)"done"u8;
private static readonly object userCurrentErrorˢ = (@string)"user.Current: error:"u8;
private static readonly object userCurrentUidSetˢ = (@string)"user.Current: uid set:"u8;
private static readonly object gidSetˢ = (@string)"gid set:"u8;
private static readonly object usernameSetˢ = (@string)"username set:"u8;
private static readonly object userLookupUsernameErrorˢ = (@string)"user.Lookup(username): error:"u8;
private static readonly object uidRoundTripsˢ = (@string)"uid round-trips:"u8;
private static readonly object userLookupGroupIdGidˢ = (@string)"user.LookupGroupId(gid): error:"u8;
private static readonly object nameSetˢ = (@string)"name set:"u8;
private static readonly object userLookupGroupNameErrorˢ = (@string)"user.LookupGroup(name): error:"u8;
private static readonly object gidRoundTripsˢ = (@string)"gid round-trips:"u8;

internal static void Main() {
    var (p, err) = exec.LookPath("go"u8);
    if (err != default!){
        fmt.Println(lookPathGoErrorˢ, err);
    } else {
        fmt.Println(lookPathGoˢ, strings.TrimSuffix(filepath.Base(p), exeˢ), absoluteˢ, filepath.IsAbs(p));
    }
    (_, err) = exec.LookPath(noSuchExecutableGo2csˢ);
    fmt.Println(lookPathMissingErrorˢ, err != default!);
    if (Δruntime.GOOS != "darwin"u8) {
        fmt.Println(doneˢ);
        return;
    }
    (var u, err) = user.Current();
    fmt.Println(userCurrentErrorˢ, err != default!);
    if (err == default!) {
        fmt.Println(userCurrentUidSetˢ, (~u).Uid != ""u8, gidSetˢ, (~u).Gid != ""u8,
            usernameSetˢ, (~u).Username != ""u8);
        var (byName, e) = user.Lookup((~u).Username);
        fmt.Println(userLookupUsernameErrorˢ, e != default!,
            uidRoundTripsˢ, e == default! && (~byName).Uid == (~u).Uid);
        (var g, e) = user.LookupGroupId((~u).Gid);
        fmt.Println(userLookupGroupIdGidˢ, e != default!,
            nameSetˢ, e == default! && (~g).Name != ""u8);
        if (e == default!) {
            var (byGroup, e2) = user.LookupGroup((~g).Name);
            fmt.Println(userLookupGroupNameErrorˢ, e2 != default!,
                gidRoundTripsˢ, e2 == default! && (~byGroup).Gid == (~g).Gid);
        }
    }
    fmt.Println(doneˢ);
}

} // end main_package
