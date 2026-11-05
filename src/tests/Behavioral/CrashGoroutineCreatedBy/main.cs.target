namespace go;

using bufio = bufio_package;
using bytes = bytes_package;
using fmt = fmt_package;
using Δos = os_package;
using exec = go.os.exec_package;
using filepath = path.filepath_package;
using Δregexp = regexp_package;
using strings = strings_package;
using time = time_package;
using go.os;
using path;
using Δio = io_package;

partial class main_package {

internal static void Main() {
    if (Δos.Getenv("CRASH_GOROUTINE_CHILD") == "1"u8) {
        child();
        return;
    }
    parent();
}

internal static partial void child() {
    goǃ(worker, GoStack.Bounded);
    time.Sleep(time.ΔMinute);
}

internal static void worker() {
    throw panic("boom in worker");
}

internal static ж<Δregexp.Regexp> header = Δregexp.MustCompile(@"^goroutine (\d+) \[running\]:$");

internal static void parent() {
    var linesᴷ = new stackAppend<@string>(moves: true);

    var cmd = exec.Command(Δos.Args[0]);
    cmd.Env = append(Δos.Environ(), (@string)"CRASH_GOROUTINE_CHILD=1", (@string)"GOTRACEBACK=single");
    ref var stderr = ref heap(new bytes.Buffer(), out var Ꮡstderr);
    cmd.Stderr = new bytes_BufferжWriter(Ꮡstderr);
    var err = cmd.Run();

    nint code = -1;
    {
        var (exit, ok) = err._<ж<exec.ExitError>>(ᐧ); if (ok) {
            code = exit.Value.ProcessState.ExitCode();
        }
    }
    fmt.Println("child exit code:", code);

    slice<@string> lines = default;
    var scanner = bufio.NewScanner(new bytes_BufferжReader(Ꮡstderr));
    while (scanner.Scan()) {
        lines = append(linesᴷ, lines, scanner.Text());
    }

    lines = move2heap(linesᴷ, lines);
    foreach (var (i, line) in lines) {
        switch (ᐧ) {
        case {} when strings.HasPrefix(line, "panic: "): {
            fmt.Println(line);
            break;
        }
        case {} when header.MatchString(line): {
            @string id = header.FindStringSubmatch(line)[1];
            fmt.Println("header names a goroutine other than main:", id != "1"u8);
            if (i + 1 < len(lines)) {
                fmt.Println("crashing frame:", lines[i + 1]);
            }
            break;
        }
        case {} when strings.HasPrefix(line, "created by "): {
            fmt.Println(line);
            if (i + 1 < len(lines)) {
                @string position = strings.TrimPrefix(lines[i + 1], "\t");
                {
                    nint space = strings.IndexByte(position, (byte)' '); if (space >= 0) {
                        position = position.slice(0, space);
                    }
                }
                {
                    nint colon = strings.LastIndexByte(position, (byte)':'); if (colon >= 0) {
                        position = position.slice(0, colon);
                    }
                }
                fmt.Println("created at file:", filepath.Base(position));
            }
            break;
        }}

    }
}

} // end main_package
