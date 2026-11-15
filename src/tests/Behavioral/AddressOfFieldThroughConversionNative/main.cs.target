namespace go;

using binary = encoding.binary_package;
using fmt = fmt_package;
using atomic = sync.atomic_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using encoding;
using sync;

partial class main_package {

partial struct rec {
    internal int32 cnt;
    internal int32 id;
}

internal static void main() {
    GoFrame ᒐ = default;
    try {
        var (page, err) = syscall.Mmap(-1, 0, 4096, (nint)((nint)syscall.PROT_READ | (nint)syscall.PROT_WRITE), (nint)((nint)syscall.MAP_ANON | (nint)syscall.MAP_PRIVATE));
        if (err != nil) {
            fmt.Println("mmap:", err);
            return;
        }
        defer(syscall.Munmap, page, ref ᒐ);

        @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ(page, 0));

        atomic.StoreInt32(@unsafe.Pointer.To<rec>(p).Ꮡid, 7);
        fmt.Println("atomic store:", (@unsafe.Pointer.To<rec>(p)).Value.id, binary.LittleEndian.Uint32(page[4..]));

        (@unsafe.Pointer.To<rec>(p).Ꮡcnt).Value = 11;
        fmt.Println("union write:", (@unsafe.Pointer.To<rec>(p)).Value.cnt, binary.LittleEndian.Uint32(page[0..]));

        var q = @unsafe.Pointer.To<rec>(p).Ꮡid;
        q.Value = 9;
        fmt.Println("kept address:", (@unsafe.Pointer.To<rec>(p)).Value.id, binary.LittleEndian.Uint32(page[4..]), (uintptr)q - (uintptr)p);

        fmt.Println("parenthesized CAS:", atomic.CompareAndSwapInt32((@unsafe.Pointer.To<rec>(p)).Ꮡid, 9, 4), binary.LittleEndian.Uint32(page[4..]));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
