namespace go;

using fmt = fmt_package;
using Δos = os_package;
using exec = go.os.exec_package;
using strconv = strconv_package;
using strings = strings_package;
using go.os;

partial class main_package {

internal static readonly @string token = "argv0-token-7f3a"u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string argv0Depthˢ = "ARGV0_DEPTH"u8;
private static readonly object osExecutableˢ = (@string)"os.Executable:"u8;

internal static void Main() {
    var (depth, _) = strconv.Atoi(Δos.Getenv(argv0Depthˢ));
    if (depth > 3) {
        fmt.Printf("depth %d: REFUSED (cap)\n"u8, depth);
        Δos.Exit(3);
    }
    if (Δos.Args[0] == token) {
        fmt.Printf("depth %d: child saw the token in os.Args[0]; extra args %q\n"u8, depth, Δos.Args[1..]);
        Δos.Exit(0);
    }
    if (depth > 0) {
        fmt.Printf("depth %d: child did NOT see the token (os.Args[0] ends %q); re-executing again\n"u8, depth, tail(Δos.Args[0]));
    }
    var (exe, err) = Δos.Executable();
    if (err != default!) {
        fmt.Println(osExecutableˢ, err);
        Δos.Exit(1);
    }
    var cmd = exec.Command(exe);
    cmd.Value.Path = exe;
    cmd.Value.Args = new @string[]{token, "-marker"u8, "x"u8}.slice();
    cmd.Value.Env = append(Δos.Environ(), "ARGV0_DEPTH="u8 + strconv.Itoa(depth + 1));
    (var @out, err) = cmd.CombinedOutput();
    fmt.Print(((@string)@out));
    if (depth == 0) {
        fmt.Printf("parent: child exit error = %v\n"u8, err);
    }
}

internal static @string tail(@string s) {
    {
        nint i = strings.LastIndex(s, "/"u8); if (i >= 0) {
            return s.slice(i + 1);
        }
    }
    return s;
}

} // end main_package
