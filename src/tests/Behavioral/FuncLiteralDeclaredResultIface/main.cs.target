namespace go;

using fmt = fmt_package;
using Δio = io_package;
using os = os_package;

partial class main_package {

internal static void use(Func<@string, (Δio.ReadCloser, error)> open) {
    var (rc, err) = open("no-such-file-for-go2cs");
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
