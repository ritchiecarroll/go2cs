namespace go;

using fmt = fmt_package;
using os = os_package;
using time = time_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static partial void Main() {
    var (r, w, err) = os.Pipe();
    if (err != default!) {
        fmt.Println("pipe error:", err);
        return;
    }
    var done = new channel<@string>(1);
    var doneʗ1 = done;
    var rʗ1 = r;
    goǃ(() => {
        ref var buf = ref heap(new array<byte>(16), out var Ꮡbuf);
        var (_, rerr) = rʗ1.Read(buf[..]);
        if (rerr != default!) {
            doneʗ1.ᐸꟷ(rerr.Error());
        } else {
            doneʗ1.ᐸꟷ("read returned no error");
        }
    });
    time.Sleep(300 * time.Millisecond);
    r.Close();
    var selᴛ1 = done;
    var selᴛ2 = time.After((time.Duration)(5000000000L));
    switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out var msg): {
        fmt.Println("read unblocked:", msg);
        break;
    }
    case 1 when selᴛ2.ꟷᐳ(out _): {
        fmt.Println("read did NOT unblock");
        break;
    }}
    w.Close();
}

} // end main_package
