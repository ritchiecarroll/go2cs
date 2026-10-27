namespace go;

using fmt = fmt_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static void Main() {
    GoFrame ᒐ = default;
    try {
        var (b, err) = syscall.Mmap(-1, 0, 4096, (nint)((nint)syscall.PROT_READ | (nint)syscall.PROT_WRITE), (nint)((nint)syscall.MAP_ANON | (nint)syscall.MAP_PRIVATE));

        if (err != default!) {
            fmt.Println("mmap:", err);
            return;
        }

        defer(syscall.Munmap, b, ref ᒐ);

        var p = array<byte>.AliasPointer(Ꮡ(b, 0), 8);

        var q = Ꮡ(b, 4);
        var r = p.at<byte>(4);
        fmt.Println("stored slice element == array element:", q == p.at<byte>(4), r == q);
        fmt.Println("different elements                    :", q == p.at<byte>(3), p.at<byte>(5) == q);

        var rebuilt = Ꮡ(b, 4);
        fmt.Println("unsafe.Pointer round trip             :", rebuilt == p.at<byte>(4), r == rebuilt);

        var viaUintptr = (ж<byte>)(uintptr)((@unsafe.Pointer)((uintptr)Ꮡ(b, 0) + 4));
        fmt.Println("uintptr arithmetic                    :", viaUintptr == r, q == viaUintptr);

        var seen = new map<ж<byte>, @string>{[q] = "slice element"};
        fmt.Println("map keyed by one finds the other      :", seen[r]);

        var m = new slice<byte>(8);
        fmt.Println("managed element vs native element     :", Ꮡ(m, 4) == r, q == Ꮡ(m, 4));

        r.Value = 42;
        fmt.Println("one storage                           :", q.Value, b[4], rebuilt.Value);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
