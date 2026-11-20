namespace go.github.com.mattn.go_isatty;

using errors = errors_package;
using strings = strings_package;
using syscall = syscall_package;
using utf16 = unicode.utf16_package;
using @unsafe = unsafe_package;
using unicode;

partial class isatty_package {

internal static uintptr objectNameInfo => 1;
internal static UntypedInt fileNameInfo => 2;
internal static UntypedInt fileTypePipe => 3;

internal static ж<syscall.LazyDLL> kernel32 = syscall.NewLazyDLL("kernel32.dll");
internal static ж<syscall.LazyDLL> ntdll = syscall.NewLazyDLL("ntdll.dll");
internal static ж<syscall.LazyProc> procGetConsoleMode = kernel32.NewProc("GetConsoleMode");
internal static ж<syscall.LazyProc> procGetFileInformationByHandleEx = kernel32.NewProc("GetFileInformationByHandleEx");
internal static ж<syscall.LazyProc> procGetFileType = kernel32.NewProc("GetFileType");
internal static ж<syscall.LazyProc> procNtQueryObject = ntdll.NewProc("NtQueryObject");

[GoInit] internal static void init() {
    if (procGetFileInformationByHandleEx.Find() != nil) {
        procGetFileInformationByHandleEx = default;
    }
    if (procNtQueryObject.Find() != nil) {
        procNtQueryObject = default;
    }
}

public static bool IsTerminal(uintptr fd) {
    ref var st = ref heap(new uint32(), out var Ꮡst);
    var ᴋ0 = Ꮡst;
        var (r, _, e) = syscall.Syscall(procGetConsoleMode.Addr(), 2, fd, (uintptr)ᴋ0, 0);
    System.GC.KeepAlive(ᴋ0);
    return r != 0 && e == 0;
}

internal static bool isCygwinPipeName(@string name) {
    var token = strings.Split(name, "-");
    if (len(token) < 5) {
        return false;
    }

    if (token[0] != @"\msys" && token[0] != @"\cygwin" && token[0] != @"\Device\NamedPipe\msys" && token[0] != @"\Device\NamedPipe\cygwin") {
        return false;
    }

    if (token[1] == "") {
        return false;
    }

    if (!strings.HasPrefix(token[2], "pty")) {
        return false;
    }

    if (token[3] != @"from" && token[3] != @"to") {
        return false;
    }

    if (token[4] != "master") {
        return false;
    }

    foreach (var (_, t) in range(token[5..])) {
        if (t == ""u8) {
            return false;
        }
    }

    return true;
}

internal static (@string, error) getFileNameByHandle(uintptr fd) {
    if (procNtQueryObject == nil) {
        return ("", errors.New("ntdll.dll: NtQueryObject not supported"));
    }

    ref var buf = ref heap(new array<uint16>(264), out var Ꮡbuf);
    ref var result = ref heap(new nint(), out var Ꮡresult);
    var ᴋ1 = Ꮡbuf;
    var ᴋ2 = Ꮡresult;
        var (r, _, e) = syscall.Syscall6(procNtQueryObject.Addr(), 5, fd, objectNameInfo, (uintptr)ᴋ1, (uintptr)(2 * len(buf)), (uintptr)ᴋ2, 0);
    System.GC.KeepAlive(ᴋ1);
    System.GC.KeepAlive(ᴋ2);
    if (r != 0) {
        return ("", e);
    }
    return ((@string)utf16.Decode(buf.slice(4, (uint16)(4 + buf[0] / 2))), default);
}

public static bool IsCygwinTerminal(uintptr fd) {
    if (procGetFileInformationByHandleEx == nil) {
        var (name, err) = getFileNameByHandle(fd);
        if (err != nil) {
            return false;
        }
        return isCygwinPipeName(name);
    }

    var (ft, _, e) = syscall.Syscall(procGetFileType.Addr(), 1, fd, 0, 0);
    if (ft != fileTypePipe || e != 0) {
        return false;
    }

    ref var buf = ref heap(new array<uint16>(262), out var Ꮡbuf);
    var ᴋ3 = Ꮡbuf;
        (var r, _, e) = syscall.Syscall6(procGetFileInformationByHandleEx.Addr(), 4, fd, fileNameInfo, (uintptr)ᴋ3, (uintptr)(len(buf) * 2), 0, 0);
    System.GC.KeepAlive(ᴋ3);
    if (r == 0 || e != 0) {
        return false;
    }

    var l = ~Ꮡbuf.Reinterpret<array<uint16>, uint32>();
    return isCygwinPipeName((@string)utf16.Decode(buf.slice(2, (nint)(2 + l / 2))));
}

} // end isatty_package
