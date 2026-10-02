namespace go;

using fmt = fmt_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object mmapˢ = (@string)"mmap:"u8;
private static readonly object bytesAtGoOffsets2And3ˢ = (@string)"bytes at Go offsets 2 and 3:"u8;
private static readonly object portReadThroughTheAliasˢ = (@string)"port read through the alias:"u8;
private static readonly object bytesAt014ˢ = (@string)"bytes at 0, 1, 4:"u8;

internal static void Main() {
    GoFrame ᒐ = default;
    try {
        var (page, err) = syscall.Mmap(-1, 0, 4096, (nint)((nint)syscall.PROT_READ | (nint)syscall.PROT_WRITE), (nint)((nint)syscall.MAP_ANON | (nint)syscall.MAP_PRIVATE));
        if (err != default!) {
            fmt.Println(mmapˢ, err);
            return;
        }
        defer(syscall.Munmap, page, ref ᒐ);
        var sa = Ꮡ(page, 0).Reinterpret<byte, syscall.RawSockaddrInet4>();
        nint port = 8080;
        var p = (NativeFieldArrayPointer<byte>(sa.of(syscall.RawSockaddrInet4.ᏑPort), 2) ?? (ж<array<byte>>)(uintptr)(@unsafe.Pointer.FromPinnedBox(sa.of(syscall.RawSockaddrInet4.ᏑPort))));
        p.ElementRef(0) = (byte)((port >> (int)(8)));
        p.ElementRef(1) = (byte)port;
        fmt.Println(bytesAtGoOffsets2And3ˢ, page[2], page[3]);
        var q = (NativeFieldArrayPointer<byte>(sa.of(syscall.RawSockaddrInet4.ᏑPort), 2) ?? (ж<array<byte>>)(uintptr)(@unsafe.Pointer.FromPinnedBox(sa.of(syscall.RawSockaddrInet4.ᏑPort))));
        fmt.Println(portReadThroughTheAliasˢ, (nint)(((nint)q.ElementRef(0) << (int)(8)) | (nint)q.ElementRef(1)));
        fmt.Println(bytesAt014ˢ, page[0], page[1], page[4]);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
