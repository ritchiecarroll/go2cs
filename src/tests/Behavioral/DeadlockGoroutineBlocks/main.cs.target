namespace go;

using bufio = bufio_package;
using bytes = bytes_package;
using fmt = fmt_package;
using Δos = os_package;
using exec = go.os.exec_package;
using filepath = path.filepath_package;
using Δregexp = regexp_package;
using runtime = runtime_package;
using strings = strings_package;
using go.os;
using path;
using Δio = io_package;

partial class main_package {

internal static void recvNil() {
    channel<nint> c = default;
    ᐸꟷ(c);
}

internal static void sendNil() {
    channel<nint> c = default;
    c.ᐸꟷ(1);
}

internal static partial void child() {
    var exprᴛ1 = Δos.Getenv("DEADLOCK_BLOCKS_CHILD");
    switch (ᐧ) {
    case {} when exprᴛ1 == "select"u8: {
        select();
        break;
    }
    case {} when exprᴛ1 == "three"u8: {
        goǃ(recvNil);
        goǃ(sendNil);
        channel<nint> c = default;
        foreach (var _ᴛ1 in c) {
        }
        break;
    }
    case {} when exprᴛ1 == "goexit"u8: {
        goǃ(recvNil);
        runtime.Goexit();
        break;
    }}

}

internal static ж<Δregexp.Regexp> goroutineID = Δregexp.MustCompile(@"goroutine (\d+)");
internal static ж<Δregexp.Regexp> offset = Δregexp.MustCompile(@" \+0x[0-9a-f]+$");

internal static void main() {
    if (Δos.Getenv("DEADLOCK_BLOCKS_CHILD") != ""u8) {
        child();
        return;
    }

    foreach (var (_, shape) in range(slice<@string>(["select", "three", "goexit"]))) {
        report(shape);
    }
}

internal static void report(@string shape) {
    var cmd = exec.Command(Δos.Args[0]);
    cmd.Env = append(Δos.Environ(), "DEADLOCK_BLOCKS_CHILD="u8 + shape);
    ref var stderr = ref heap(new bytes.Buffer(), out var Ꮡstderr);
    cmd.Stderr = new bytes_BufferжWriter(Ꮡstderr);
    var err = cmd.Run();

    nint code = -1;
    var (exit, ok) = err._<ж<exec.ExitError>>(ᐧ); if (ok) {
        code = exit.Value.ProcessState.ExitCode();
    }
    fmt.Printf("== %s: exit code %d\n"u8, shape, code);

    var ids = new map<@string, nint>{};
    var scanner = bufio.NewScanner(new bytes_BufferжReader(Ꮡstderr));
    while (scanner.Scan()) {
        @string line = scanner.Text();
        if (strings.HasPrefix(line, "\t")) {
            @string position = offset.ReplaceAllString(line[1..], "");
            nint colon = strings.LastIndexByte(position, (byte)':'); if (colon >= 0) {
                position = filepath.Base(position.slice(0, colon)) + position.slice(colon);
            }
            line = "\t"u8 + position;
        }
        var idsʗ1 = ids;
        line = goroutineID.ReplaceAllStringFunc(line, (@string m) => {
            @string id = goroutineID.FindStringSubmatch(m)[1];
            var (_, seen) = idsʗ1[id, ꟷ]; if (!seen) {
                idsʗ1[id] = len(idsʗ1) + 1;
            }
            return fmt.Sprintf("goroutine #%d"u8, idsʗ1[id]);
        });
        fmt.Println(line);
    }
}

} // end main_package
