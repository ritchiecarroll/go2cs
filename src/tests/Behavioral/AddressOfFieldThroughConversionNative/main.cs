namespace go;

using binary = encoding.binary_package;
using fmt = fmt_package;
using atomic = sync.atomic_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using System.Runtime.InteropServices;
using encoding;
using go.golib;
using sync;

partial class main_package {

partial struct rec {
    internal int32 cnt;
    internal int32 id;
}

[global::go.golib.GoInlineLayout(Size = 8, Align = 8), StructLayout(LayoutKind.Explicit, Size = 8)] partial struct expr_u /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<int32>))] internal readonly int iValue;
    [FieldOffset(0)] internal uintptr zToken;
}

[global::go.golib.GoInlineLayout(Size = 4, Align = 4), StructLayout(LayoutKind.Explicit, Size = 4)] partial struct expr_w /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<int32>))] internal readonly int iOfst;
    [FieldOffset(0)] internal int32 iJoin;
}

[global::go.golib.GoInlineLayout(Size = 32, Align = 8), StructLayout(LayoutKind.Explicit, Size = 32)] partial struct expr {
    [FieldOffset(0)] internal uint8 op;
    [FieldOffset(4)] internal uint32 flags;
    [FieldOffset(8)] internal expr_u u;
    [FieldOffset(16)] internal uintptr left;
    [FieldOffset(24)] internal expr_w w;
    [FieldOffset(28)] internal int32 n;
}

internal static void store(uintptr a, int32 v) { (@unsafe.Pointer.To<int32>((@unsafe.Pointer)a)).Value = v; }

[global::go.golib.GoInlineLayout(Size = 8, Align = 8), StructLayout(LayoutKind.Explicit, Size = 8)] internal partial struct main_type /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uint64>))] internal readonly long i;
    [FieldOffset(0)] internal float64 f;
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

        var b = (uintptr)p + 256;
        var e = page[256..];
        var le = binary.LittleEndian;

        (@unsafe.Pointer.To<expr>((@unsafe.Pointer)b).Ꮡu.Reinterpret<expr_u, int32>()).Value = 77;
        fmt.Println("union store:", le.Uint32(e[8..]));

        le.PutUint32(e[8..], 78);
        fmt.Println("union read:", ~@unsafe.Pointer.To<expr>((@unsafe.Pointer)b).Ꮡu.Reinterpret<expr_u, int32>());

        (@unsafe.Pointer.To<expr>((@unsafe.Pointer)b).Ꮡu.Reinterpret<expr_u, int32>()).Value += 2;
        (@unsafe.Pointer.To<expr>((@unsafe.Pointer)b).Ꮡu.Reinterpret<expr_u, int32>()).Value++;
        fmt.Println("union op-assign:", le.Uint32(e[8..]));

        (@unsafe.Pointer.To<expr>((@unsafe.Pointer)b).Ꮡu.Reinterpret<expr_u, int32>()).Value--;
        (@unsafe.Pointer.To<expr>((@unsafe.Pointer)b).Ꮡu.Reinterpret<expr_u, int32>()).Value--;
        fmt.Println("union decrement:", le.Uint32(e[8..]));

        (@unsafe.Pointer.To<expr>((@unsafe.Pointer)b).Ꮡu.Reinterpret<expr_u, uintptr>()).Value = unchecked((nuint)0x1122334455UL);
        fmt.Println("union pointer member:", le.Uint64(e[8..]) == 0x1122334455UL);

        (@unsafe.Pointer.To<expr>((@unsafe.Pointer)b).Ꮡw.Reinterpret<expr_w, int32>()).Value = 41;
        (@unsafe.Pointer.To<expr>((@unsafe.Pointer)b)).Value.n = 42;
        fmt.Println("second union:", le.Uint32(e[24..]), le.Uint32(e[28..]));

        (@unsafe.Pointer.To<expr>((@unsafe.Pointer)b).Ꮡflags.Reinterpret<uint32, uint16>()).Value = 0xBEEF;
        fmt.Printf("narrow store: %#x\n"u8, le.Uint32(e[4..]));

        store((uintptr)@unsafe.Pointer.To<expr>((@unsafe.Pointer)b).Ꮡw, 43);
        fmt.Println("address as uintptr:", le.Uint32(e[24..]), (uintptr)@unsafe.Pointer.To<expr>((@unsafe.Pointer)b).Ꮡw - b);

        atomic.AddInt32(@unsafe.Pointer.To<expr>((@unsafe.Pointer)b).Ꮡw.Reinterpret<expr_w, int32>(), 2);
        fmt.Println("atomic on member:", le.Uint32(e[24..]));

        var k = @unsafe.Pointer.To<expr>((@unsafe.Pointer)b).Ꮡu.Reinterpret<expr_u, int32>();
        k.Value = 5;
        fmt.Println("kept member address:", le.Uint32(e[8..]));

        (@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<expr>((@unsafe.Pointer)b).Ꮡu), 0))).Value = 0x66;
        (@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<expr>((@unsafe.Pointer)b).Ꮡu), 4))).Value = 0x67;
        fmt.Printf("explicit star: %#x\n"u8, le.Uint64(e[8..]));

        var d = b + 64;
        (@unsafe.Pointer.To<float64>((@unsafe.Pointer)d)).Value = 1.5D;
        (@unsafe.Pointer.To<main_type>((@unsafe.Pointer)d).nilcheck().Reinterpret<main_type, uint64>()).Value <<= 1;
        fmt.Printf("whole-object union: %#x %#x\n"u8, le.Uint64(e[64..]), ~@unsafe.Pointer.To<main_type>((@unsafe.Pointer)d).nilcheck().Reinterpret<main_type, uint64>());

        var v = @unsafe.Pointer.To<expr>((@unsafe.Pointer)b);
        (v.Ꮡu.Reinterpret<expr_u, int32>()).Value = 6;
        fmt.Println("pointer variable base:", le.Uint32(e[8..]));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
