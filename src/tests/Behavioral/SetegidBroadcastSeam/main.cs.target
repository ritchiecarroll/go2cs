namespace go;

using fmt = fmt_package;
using os = os_package;
using runtime = runtime_package;
using strings = strings_package;
using syscall = syscall_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static @string gidLine(nint tid) {
    var (b, err) = os.ReadFile(fmt.Sprintf("/proc/self/task/%d/status"u8, tid));
    if (err != default!) {
        return "read error";
    }
    foreach (var (_, line) in strings.Split(((@string)b), "\n")) {
        if (strings.HasPrefix(line, "Gid:")) {
            return strings.Join(strings.Fields(line[4..]), " ");
        }
    }
    return "no Gid line";
}

internal static partial void Main() {
    if (runtime.GOOS != "linux"u8) {
        fmt.Println("not linux; nothing to observe");
        return;
    }

    if (os.Geteuid() != 0) {
        fmt.Println("SKIP: needs root -- this seam changes the process's effective gid and reads /proc");
        return;
    }

    var tidCh = new channel<nint>(0);
    var release = new channel<EmptyStruct>(0);
    var done = new channel<@string>(0);

    var doneʗ1 = done;
    var releaseʗ1 = release;
    var tidChʗ1 = tidCh;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            runtime.LockOSThread();
            defer(runtime.UnlockOSThread, ref ᒐ);
            nint tid = syscall.Gettid();
            tidChʗ1.ᐸꟷ(tid);
            ᐸꟷ(releaseʗ1);
            doneʗ1.ᐸꟷ(gidLine(tid));
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });

    nint parked = ᐸꟷ(tidCh);
    nint self = syscall.Gettid();

    fmt.Println("before main  :", gidLine(self));
    fmt.Println("before parked:", gidLine(parked));

    {
        var err = syscall.Setegid(1); if (err != default!) {
            fmt.Println("setegid(1) failed:", err);
            return;
        }
    }

    @string mainAfter = gidLine(self);
    fmt.Println("after  main  :", mainAfter);

    close(release);
    @string parkedAfter = ᐸꟷ(done);
    fmt.Println("after  parked:", parkedAfter);
    fmt.Println("the parked thread followed the change:", parkedAfter == mainAfter);

    {
        var err = syscall.Setegid(0); if (err != default!) {
            fmt.Println("restore failed:", err);
            return;
        }
    }
    fmt.Println("restored main:", gidLine(self));
}

} // end main_package
