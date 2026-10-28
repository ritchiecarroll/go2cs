namespace go;

using fmt = fmt_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static void Main() {
    GoFrame ᒐ = default;
    try {
        var (page, err) = syscall.Mmap(-1, 0, 4096, (nint)((nint)syscall.PROT_READ | (nint)syscall.PROT_WRITE), (nint)((nint)syscall.MAP_ANON | (nint)syscall.MAP_PRIVATE));

        if (err != nil) {
            fmt.Println("mmap:", err);
            return;
        }

        defer(syscall.Munmap, page, ref ᒐ);

        var sa = Ꮡ(page, 0).Reinterpret<byte, syscall.RawSockaddrInet4>();

        nint port = 8080;
        var p = @unsafe.ArrayPointer<byte>.Of(sa.of(syscall.RawSockaddrInet4.ᏑPort), 2);
        p.ElementRef(0) = (byte)(port >> 8);
        p.ElementRef(1) = (byte)port;

        fmt.Println("bytes at Go offsets 2 and 3:", page[2], page[3]);

        var q = @unsafe.ArrayPointer<byte>.Of(sa.of(syscall.RawSockaddrInet4.ᏑPort), 2);
        fmt.Println("port read through the alias:", (nint)q.ElementRef(0) << 8 | (nint)q.ElementRef(1));

        fmt.Println("bytes at 0, 1, 4:", page[0], page[1], page[4]);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
