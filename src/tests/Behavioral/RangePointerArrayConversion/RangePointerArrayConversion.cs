namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static void main() {
    ref var a = ref heap<array<nint>>(array<nint>([10, 20, 30]), out var Ꮡa);
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡa);
    nint sum = 0;
    foreach (var (i, x) in range(@unsafe.Pointer.View<nint>(p, 0, 3, 3))) {
        sum += i + x;
    }
    fmt.Println(sum);

    ref var rows = ref heap<array<array<nint>>>(new array<nint>[]{new nint[]{1, 2}.array(), new nint[]{3, 4}.array()}.array(), out var Ꮡrows);
    @unsafe.Pointer q = @unsafe.Pointer.FromPinnedBox(Ꮡrows);
    nint total = 0;
    foreach (var (i, vᴛ1) in range((@unsafe.Pointer.To<array<array<nint>>>(q)).Value)) {
        var row = vᴛ1.Clone();

        total += i + row[0] * 10 + row[1];
    }
    fmt.Println(total);
}

} // end main_package
