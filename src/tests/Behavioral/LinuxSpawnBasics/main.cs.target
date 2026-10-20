namespace go;

using fmt = fmt_package;
using Δos = os_package;
using exec = go.os.exec_package;
using strings = strings_package;
using go.os;

partial class main_package {

internal static void Main() {
    if (Δos.Getenv("SPAWN_BASICS_CHILD") == "1"u8) {
        fmt.Println("child: hello from the spawned image");
        fmt.Println("child: marker =", Δos.Getenv("SPAWN_BASICS_MARKER"));
        Δos.Exit(3);
    }

    var cmd = exec.Command(Δos.Args[0]);
    cmd.Value.Env = append(Δos.Environ(), "SPAWN_BASICS_CHILD=1"u8, "SPAWN_BASICS_MARKER=xyzzy");

    var (@out, err) = cmd.CombinedOutput();

    fmt.Print((@string)@out);
    if (err == default!) {
        fmt.Println("parent: unexpected nil error, want exit status 3");
        return;
    }

    var (exitErr, ok) = err._<ж<exec.ExitError>>(ᐧ);
    if (!ok) {
        fmt.Println("parent: spawn failed:", strings.ReplaceAll(err.Error(), Δos.Args[0], "<self>"));
        return;
    }

    fmt.Println("parent: child exit code =", exitErr.Value.ProcessState.ExitCode());
    fmt.Println("parent: captured", len(strings.Split(strings.TrimSpace((@string)@out), "\n")), "lines");
}

} // end main_package
