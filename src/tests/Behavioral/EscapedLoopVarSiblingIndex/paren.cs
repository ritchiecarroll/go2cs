namespace go;

using System.Runtime.CompilerServices;

partial class main_package {

internal static partial nint parenIndex() {
    ref var s = ref heap<slice<nint>>(out var Ꮡs);
    s = new nint[]{0, 0, 0}.slice();
    var p = Ꮡs;
    nint i = 100;
    {
        nint iΔ1 = 1;
        (p.ValueSlot)[iΔ1] = 9;
    }
    _ = i;
    return s[1];
}

} // end main_package
