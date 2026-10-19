namespace go;

using fmt = fmt_package;
using Δio = io_package;
using Δos = os_package;
using exec = go.os.exec_package;
using go.os;

partial class main_package {

internal static void Main() {
    if (Δos.Getenv("EOF_BARRIER_CHILD") == "1"u8) {
        child();
        return;
    }
    parent();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object readyˢ = (@string)"READY"u8;

internal static void child() {
    fmt.Println(readyˢ);
    Δos.Stdout.Close();
    array<byte> release = new(1);
    Δos.Stdin.Read(release[..]);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object stdinPipeErrorˢ = (@string)"StdinPipe error:"u8;
private static readonly object stdoutPipeErrorˢ = (@string)"StdoutPipe error:"u8;
private static readonly object startErrorˢ = (@string)"Start error:"u8;
private static readonly object eofPrecededChildExitˢ = (@string)"EOF preceded child exit"u8;

internal static void parent() {
    var cmd = exec.Command(Δos.Args[0]);
    cmd.Value.Env = append(Δos.Environ(), "EOF_BARRIER_CHILD=1"u8);
    var (stdin, err) = cmd.StdinPipe();
    if (err != default!) {
        fmt.Println(stdinPipeErrorˢ, err);
        Δos.Exit(1);
    }
    (var @out, err) = cmd.StdoutPipe();
    if (err != default!) {
        fmt.Println(stdoutPipeErrorˢ, err);
        Δos.Exit(1);
    }
    {
        var errΔ1 = cmd.Start(); if (errΔ1 != default!) {
            fmt.Println(startErrorˢ, errΔ1);
            Δos.Exit(1);
        }
    }
    var (data, _) = Δio.ReadAll(@out);
    stdin.Write(new byte[]{(rune)'\n'}.slice());
    stdin.Close();
    cmd.Wait();
    fmt.Printf("child wrote %q\n"u8, ((@string)data));
    fmt.Println(eofPrecededChildExitˢ);
}

} // end main_package
