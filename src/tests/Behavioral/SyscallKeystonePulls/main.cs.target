namespace go;

using fmt = fmt_package;
using exec = os.exec_package;
using user = os.user_package;
using filepath = path.filepath_package;
using strings = strings_package;
using os;
using path;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object lookPathGoErrorˢ = (@string)"LookPath(go): error:"u8;
private static readonly object lookPathGoˢ = (@string)"LookPath(go):"u8;
private static readonly @string exeˢ = ".exe"u8;
private static readonly object absoluteˢ = (@string)"absolute:"u8;
private static readonly object userCurrentErrorˢ = (@string)"user.Current: error:"u8;
private static readonly object userCurrentUsernameNonˢ = (@string)"user.Current: username non-empty:"u8;
private static readonly object uidNonEmptyˢ = (@string)"uid non-empty:"u8;
private static readonly object homeAbsoluteˢ = (@string)"home absolute:"u8;
private static readonly @string noSuchExecutableGo2csˢ = "no-such-executable-go2cs-keystone"u8;
private static readonly object lookPathMissingErrorˢ = (@string)"LookPath(missing): error:"u8;
private static readonly object doneˢ = (@string)"done"u8;

internal static void Main() {
    var (p, err) = exec.LookPath("go"u8);
    if (err != default!){
        fmt.Println(lookPathGoErrorˢ, err);
    } else {
        fmt.Println(lookPathGoˢ, strings.TrimSuffix(filepath.Base(p), exeˢ), absoluteˢ, filepath.IsAbs(p));
    }
    (var u, err) = user.Current();
    if (err != default!){
        fmt.Println(userCurrentErrorˢ, err);
    } else {
        fmt.Println(userCurrentUsernameNonˢ, (~u).Username != ""u8, uidNonEmptyˢ, (~u).Uid != ""u8, homeAbsoluteˢ, filepath.IsAbs((~u).HomeDir));
    }
    (_, err) = exec.LookPath(noSuchExecutableGo2csˢ);
    fmt.Println(lookPathMissingErrorˢ, err != default!);
    fmt.Println(doneˢ);
}

} // end main_package
