namespace go;

using fmt = fmt_package;
using Δio = io_package;
using os = os_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string noSuchFileForGo2csˢ = "no-such-file-for-go2cs"u8;

internal static void use(Func<@string, (Δio.ReadCloser, error)> open) {
    var (rc, err) = open(noSuchFileForGo2csˢ);
    fmt.Println(rc == default!, err != default!);
}

internal static void Main() {
    var open = (Δio.ReadCloser, error) (@string name) => {
        var (ᴛ1, ᴛ2) = os.Open(name);
        return (new os_FileжReadCloser(ᴛ1), ᴛ2);
    };
    use(open);
}

} // end main_package
