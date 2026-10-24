namespace go;

using errors = errors_package;
using fmt = fmt_package;
using os = os_package;
using strconv = strconv_package;
using strings = strings_package;
using atomic = sync.atomic_package;
using syscall = syscall_package;
using time = time_package;
using @unsafe = unsafe_package;
using System.Runtime.CompilerServices;
using sync;

partial class main_package {

internal static UntypedInt pipeAccessDuplex => 0x3;
internal static UntypedInt fileFlagOverlapped => 0x40000000;
internal static UntypedInt pipeTypeByte => 0x0;
internal static UntypedInt pipeReadModeByte => 0x0;

internal static ж<syscall.LazyDLL> kernel32 = syscall.NewLazyDLL("kernel32.dll");
internal static ж<syscall.LazyProc> procCreateNamedPipe = kernel32.NewProc("CreateNamedPipeW");
internal static ж<atomic.Uint64> Ꮡcounter = new StaticBox<atomic.Uint64>(default(atomic.Uint64));
internal static ref atomic.Uint64 counter => ref Ꮡcounter.Value;
internal static ж<atomic.Int64> Ꮡwritten = new StaticBox<atomic.Int64>(default(atomic.Int64));
internal static ref atomic.Int64 written => ref Ꮡwritten.Value;
internal static slice<ж<os.File>> keep;

internal static (ж<os.File>, syscallꓸHandle) pipe() {
    @string name = @"\\.\pipe\go2cs-pipeshared-"u8 + strconv.Itoa(os.Getpid()) + "-"u8 + strconv.FormatUint(Ꮡcounter.Add(1), 10);
    var (wname, _) = syscall.UTF16PtrFromString(name);

    var (h, _, e) = procCreateNamedPipe.Call((uintptr)wname, (uintptr)((uintptr)pipeAccessDuplex | (uintptr)fileFlagOverlapped),
        (uintptr)((uintptr)pipeTypeByte | (uintptr)pipeReadModeByte), 1, 4096, 4096, 0, 0);
    if (((syscallꓸHandle)h) == syscall.InvalidHandle) {
        throw panic(fmt.Sprintf("CreateNamedPipe: %v"u8, e));
    }

    var (c, err) = syscall.CreateFile(wname, (uint32)((uint32)syscall.GENERIC_READ | (uint32)syscall.GENERIC_WRITE), 0, nil,
        syscall.OPEN_EXISTING, fileFlagOverlapped, 0);
    if (err != default!) {
        throw panic(fmt.Sprintf("CreateFile: %v"u8, err));
    }

    return (os.NewFile(h, name), c);
}

internal static @string readLines(ж<os.File> Ꮡr, nint n) {
    slice<byte> got = default!;
    var buf = new slice<byte>(256);

    while (strings.Count((@string)got, "\n") < n) {
        var (k, err) = Ꮡr.Read(buf);
        got = appendꓸꓸꓸ(got, buf.slice(0, k));
        if (err != default!) {
            return ((sstring)got) + "<"u8 + err.Error() + ">"u8;
        }
    }

    return (@string)got;
}

internal static partial void step(@string name, Func<@string> body) {
    Ꮡwritten.Store(0);
    var done = new channel<@string>(1);
    var doneʗ1 = done;
    goǃ(() => {
        doneʗ1.ᐸꟷ(body());
    });

    var selᴛ1 = done;
    var selᴛ2 = time.After((time.Duration)(10000000000L));
    switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out var got): {
        fmt.Printf("%s: %s\n"u8, name, got);
        break;
    }
    case 1 when selᴛ2.ꟷᐳ(out _): {
        fmt.Printf("%s: HANG (no return in 10s, %d writes returned)\n"u8, name, Ꮡwritten.Load());
        break;
    }}
}

internal partial struct main_type /*dyn*/ {
    internal ж<os.File> f;
    internal @string text;
}

internal static void Main() {
    var (server, client) = pipe();

    ref var dup = ref heap(new syscallꓸHandle(), out var Ꮡdup);
    var (self, _) = syscall.GetCurrentProcess();
    {
        var err = syscall.DuplicateHandle(self, client, self, Ꮡdup, 0, false, syscall.DUPLICATE_SAME_ACCESS); if (err != default!) {
            throw panic(fmt.Sprintf("DuplicateHandle: %v"u8, err));
        }
    }

    var stdout = os.NewFile((uintptr)client, "stdout");
    var stderr = os.NewFile((uintptr)dup, "stderr");

    var serverʗ1 = server;
    var stderrʗ1 = stderr;
    var stdoutʗ1 = stdout;
    step("1 stdout, stderr, stdout", () => {
        foreach (var (_, w) in new main_type[]{new(stdoutʗ1, "stdout line\n"), new(stderrʗ1, "stderr line\n"), new(stdoutʗ1, "stdout again\n")}.slice()) {
            {
                var (_, err) = w.f.Write(slice<byte>(w.text)); if (err != default!) {
                    return w.f.Name() + " write: "u8 + err.Error();
                }
            }
        }
        return strconv.Quote(readLines(serverʗ1, 3));
    });

    var stderrʗ2 = stderr;
    var stdoutʗ2 = stdout;
    step("2 deadlines", () => {
        var refused = stderrʗ2.SetDeadline(time.Now().Add(time.ΔHour));
        var bound = stdoutʗ2.SetDeadline(time.Now().Add(time.ΔHour));
        stdoutʗ2.SetDeadline(new time.Time(nil));
        return fmt.Sprintf("stderr ErrNoDeadline=%v stdout err=%v"u8, errors.Is(refused, os.ErrNoDeadline), bound);
    });

    var serverʗ2 = server;
    var stderrʗ3 = stderr;
    step("3 read through stderr", () => {
        {
            var (_, err) = serverʗ2.Write(slice<byte>("reply\n"u8)); if (err != default!) {
                return "server write: "u8 + err.Error();
            }
        }
        return strconv.Quote(readLines(stderrʗ3, 1));
    });

    step("4 one handle, two files", () => {
        var (server2, client2) = pipe();
        var first = os.NewFile((uintptr)client2, "first");
        var second = os.NewFile((uintptr)client2, "second");
        keep = append(keep, first, second);

        {
            var (_, err) = second.Write(slice<byte>("second line\n"u8)); if (err != default!) {
                return "second write: "u8 + err.Error();
            }
        }
        var deadline = first.SetWriteDeadline(time.Now().Add(time.ΔHour));
        {
            var (_, err) = first.Write(slice<byte>("first line\n"u8)); if (err != default!) {
                return "first write: "u8 + err.Error();
            }
        }
        @string arrived = readLines(server2, 2);

        first.SetReadDeadline(time.Now().Add(50 * time.Millisecond));
        var (_, timeout) = first.Read(new slice<byte>(16));

        return fmt.Sprintf("%q first deadline err=%v first read timed out=%v second ErrNoDeadline=%v"u8,
            arrived, deadline, errors.Is(timeout, os.ErrDeadlineExceeded),
            errors.Is(second.SetDeadline(time.Now().Add(time.ΔHour)), os.ErrNoDeadline));
    });

    step("5 Fd disassociates", () => {
        var (server3, client3) = pipe();
        var f = os.NewFile((uintptr)client3, "f");

        {
            var (_, err) = f.Write(slice<byte>("before Fd\n"u8)); if (err != default!) {
                return "write before Fd: "u8 + err.Error();
            }
        }
        var before = f.SetDeadline(new time.Time(nil));
        f.Fd();
        var after = f.SetDeadline(time.Now().Add(time.ΔHour));
        {
            var (_, err) = f.Write(slice<byte>("after Fd\n"u8)); if (err != default!) {
                return "write after Fd: "u8 + err.Error();
            }
        }

        return fmt.Sprintf("%q deadline before Fd err=%v after Fd ErrNoDeadline=%v"u8,
            readLines(server3, 2), before, errors.Is(after, os.ErrNoDeadline));
    });

    step("6 a handle number reused after a close", () => {
        var (server4, client4) = pipe();
        var first = os.NewFile((uintptr)client4, "first");
        var second = os.NewFile((uintptr)client4, "second");
        keep = append(keep, first, second);

        {
            var (_, err) = first.Write(slice<byte>("before close\n"u8)); if (err != default!) {
                return "first write: "u8 + err.Error();
            }
        }
        @string before = readLines(server4, 1);

        var (server5, other) = pipe();

        second.Close();

        syscallꓸHandle reused = default!;
        for (nint i = 0; i < 256 && reused == 0; i++) {
            ref var d = ref heap(new syscallꓸHandle(), out var Ꮡd);
            {
                var err = syscall.DuplicateHandle(self, other, self, Ꮡd, 0, false, syscall.DUPLICATE_SAME_ACCESS); if (err != default!) {
                    return "DuplicateHandle: "u8 + err.Error();
                }
            }
            if (d == client4) {
                reused = d;
            }
        }
        if (reused == 0) {
            return "the closed number was not handed out again";
        }

        var f = os.NewFile((uintptr)reused, "reused");
        keep = append(keep, f);

        {
            var (_, err) = f.Write(slice<byte>("after reuse\n"u8)); if (err != default!) {
                return "write through the reused number: "u8 + err.Error();
            }
        }

        return fmt.Sprintf("%q %q"u8, before, readLines(server5, 1));
    });

    step("7 consecutive writes on one bound FD", [MethodImpl(MethodImplOptions.NoInlining)] () => consecutiveWrites(false));
    step("7 the same, with skip-on-success set", [MethodImpl(MethodImplOptions.NoInlining)] () => consecutiveWrites(true));
}

internal static partial @string consecutiveWrites(bool skipOnSuccess) {
    var (server7, client7) = pipe();
    if (skipOnSuccess) {
        {
            var err = syscall.SetFileCompletionNotificationModes(client7, syscall.FILE_SKIP_COMPLETION_PORT_ON_SUCCESS); if (err != default!) {
                return "SetFileCompletionNotificationModes: "u8 + err.Error();
            }
        }
    }
    var f = os.NewFile((uintptr)client7, "f");

    for (nint i = 1; i <= 8; i++) {
        {
            var (_, err) = f.Write(slice<byte>("write " + strconv.Itoa(i) + "\n")); if (err != default!) {
                return fmt.Sprintf("small write %d: %v"u8, i, err);
            }
        }
        Ꮡwritten.Add(1);
    }
    @string small = readLines(server7, 8);

    UntypedInt large = 4;
    UntypedInt chunk = /* 16 << 10 */ 16384;
    var drained = new channel<@string>(1);
    var drainedʗ1 = drained;
    var server7ʗ1 = server7;
    goǃ(() => {
        nint total = 0;
        var buf = new slice<byte>(4096);
        while (total < (nint)(large * chunk)) {
            var (k, err) = server7ʗ1.Read(buf);
            total += k;
            if (err != default!) {
                drainedʗ1.ᐸꟷ(fmt.Sprintf("%d bytes <%v>"u8, total, err));
                return;
            }
        }
        drainedʗ1.ᐸꟷ(fmt.Sprintf("%d bytes"u8, total));
    });

    for (nint i = 1; i <= large; i++) {
        {
            var (n, err) = f.Write(slice<byte>(strings.Repeat(strconv.Itoa(i), chunk))); if (err != default! || n != chunk) {
                return fmt.Sprintf("large write %d: n=%d err=%v"u8, i, n, err);
            }
        }
        Ꮡwritten.Add(1);
    }

    return fmt.Sprintf("%d small lines, last %q; %d large writes, %s drained"u8,
        strings.Count(small, "\n"), small.slice(strings.LastIndex(small.slice(0, len(small) - 1), "\n") + 1), (nint)(large), ᐸꟷ(drained));
}

} // end main_package
