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

internal static void child() {
    fmt.Println("READY");
    Δos.Stdout.Close();
    array<byte> release = new(1);
    Δos.Stdin.Read(release[..]);
}

internal static void parent() {
    var cmd = exec.Command(Δos.Args[0]);
    cmd.Env = append(Δos.Environ(), "EOF_BARRIER_CHILD=1"u8);
    var (stdin, err) = cmd.StdinPipe();
    if (err != nil) {
        fmt.Println("StdinPipe error:", err);
        Δos.Exit(1);
    }
    (var @out, err) = cmd.StdoutPipe();
    if (err != nil) {
        fmt.Println("StdoutPipe error:", err);
        Δos.Exit(1);
    }
    {
        var errΔ1 = cmd.Start(); if (errΔ1 != nil) {
            fmt.Println("Start error:", errΔ1);
            Δos.Exit(1);
        }
    }
    var (data, _) = Δio.ReadAll(@out);
    stdin.Write(slice<byte>([(byte)'\n']));
    stdin.Close();
    cmd.Wait();
    fmt.Printf("child wrote %q\n"u8, (@string)data);
    fmt.Println("EOF preceded child exit");
}

} // end main_package
