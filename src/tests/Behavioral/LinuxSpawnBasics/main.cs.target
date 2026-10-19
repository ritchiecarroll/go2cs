namespace go;

using fmt = fmt_package;
using Δos = os_package;
using exec = go.os.exec_package;
using strings = strings_package;
using go.os;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object childHelloFromTheSpawnedˢ = (@string)"child: hello from the spawned image"u8;
private static readonly object childMarkerˢ = (@string)"child: marker ="u8;
private static readonly object parentUnexpectedNilErrorˢ = (@string)"parent: unexpected nil error, want exit status 3"u8;
private static readonly object parentSpawnFailedˢ = (@string)"parent: spawn failed:"u8;
private static readonly object parentChildExitCodeˢ = (@string)"parent: child exit code ="u8;
private static readonly object parentCapturedˢ = (@string)"parent: captured"u8;
private static readonly object linesˢ = (@string)"lines"u8;

internal static void Main() {
    if (Δos.Getenv("SPAWN_BASICS_CHILD") == "1"u8) {
        fmt.Println(childHelloFromTheSpawnedˢ);
        fmt.Println(childMarkerˢ, Δos.Getenv("SPAWN_BASICS_MARKER"));
        Δos.Exit(3);
    }

    var cmd = exec.Command(Δos.Args[0]);
    cmd.Value.Env = append(Δos.Environ(), "SPAWN_BASICS_CHILD=1"u8, "SPAWN_BASICS_MARKER=xyzzy");

    var (@out, err) = cmd.CombinedOutput();

    fmt.Print(((@string)@out));
    if (err == default!) {
        fmt.Println(parentUnexpectedNilErrorˢ);
        return;
    }

    var (exitErr, ok) = err._<ж<exec.ExitError>>(ᐧ);
    if (!ok) {
        fmt.Println(parentSpawnFailedˢ, strings.ReplaceAll(err.Error(), Δos.Args[0], "<self>"));
        return;
    }

    fmt.Println(parentChildExitCodeˢ, exitErr.Value.ProcessState.ExitCode());
    fmt.Println(parentCapturedˢ, len(strings.Split(strings.TrimSpace(((@string)@out)), "\n")), linesˢ);
}

} // end main_package
